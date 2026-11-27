using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using VM.Core;
using VM.PlatformSDKCS;

namespace SLTtechSoft.Class
{
    /// <summary>
    /// Adapter between the machine application and a VisionMaster .prc process.
    /// Expected procedure configuration:
    ///   Image input : an Image Source module configured with Image Source = SDK.
    ///                 Common/default module names are auto-detected.
    ///   Result      : procedure output "Result" (0 = NG, non-zero = OK). String "out" is also supported.
    ///   Output image: optional procedure output "OutputImage" (fallback: "ImageData").
    /// </summary>
    public sealed class VisionMasterProcess : IDisposable
    {
        public VmProcedure Procedure { get; private set; }
        public string FilePath { get; private set; }
        private Bitmap _lastInputImage;
        private Bitmap _lastOutputImage;
        public Bitmap LastInputImage => _lastInputImage;
        public Bitmap LastOutputImage => _lastOutputImage;
        public int LastInputWidth { get; private set; }
        public int LastInputHeight { get; private set; }
        public string LastError { get; private set; } = "";
        public string LastResultSource { get; private set; } = "";
        public string LastResultText { get; private set; } = "";
        public string DisplayModuleName { get; private set; } = "";
        private object _displayModule;
        private object _cachedResultObject;
        private string _cachedResultOutputName = "";
        private string _cachedResultSource = "";

        private VisionMasterProcess() { }

        public static VisionMasterProcess Load(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            if (!System.IO.File.Exists(fileName)) throw new System.IO.FileNotFoundException("VisionMaster process file not found.", fileName);

            VmProcedure procedure = VmProcedure.Load(fileName);
            if (procedure == null) throw new InvalidOperationException("VmProcedure.Load returned null: " + fileName);

            VisionMasterProcess process = new VisionMasterProcess { Procedure = procedure, FilePath = fileName };
            // VisionMaster render callbacks only need to be enabled when the procedure is loaded/bound.
            // Do not re-enable them on every trigger; that creates unnecessary work and UI jitter.
            process.EnableDisplayCallbacks();
            return process;
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(FilePath)) throw new InvalidOperationException("VisionMaster procedure file path is empty.");
            SaveAs(FilePath);
        }

        public void SaveAs(string fileName)
        {
            if (_disposed || Procedure == null) throw new InvalidOperationException("VisionMaster procedure is not loaded.");
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("VisionMaster procedure file path is empty.", nameof(fileName));
            Procedure.SaveAs(fileName);
        }

        public int Run(Bitmap inputImage)
        {
            return Run(inputImage, false, true);
        }

        public int Run(Bitmap inputImage, bool cacheOutputImage)
        {
            return Run(inputImage, false, cacheOutputImage);
        }

        public int Run(Bitmap inputImage, bool cacheInputImage, bool cacheOutputImage)
        {
            if (_disposed || Procedure == null) throw new InvalidOperationException("VisionMaster procedure is not loaded.");
            if (inputImage == null) throw new ArgumentNullException(nameof(inputImage));

            LastError = "";
            LastResultSource = "";
            LastResultText = "";
            LastInputWidth = inputImage.Width;
            LastInputHeight = inputImage.Height;

            // Do not clone a full camera frame on every trigger. A 5~12 MP frame can be tens of MB,
            // and the old path cloned it several times before VisionMaster even started running.
            // Keep a copy only when the caller really needs to save the original image.
            if (cacheInputImage) ReplaceBitmap(ref _lastInputImage, inputImage);
            else
            {
                _lastInputImage?.Dispose();
                _lastInputImage = null;
            }

            Bitmap vmInput = EnsureVisionMasterPixelFormat(inputImage, out bool vmInputCreated);
            ImageBaseData inputData = null;
            try
            {
                // LocateDemoCs style: the procedure/render binding is prepared once.
                // A trigger should only inject the image and run the procedure.
                inputData = SetInputImage(vmInput);
                Procedure.Run();
            }
            finally
            {
                inputData?.Dispose();
                if (vmInputCreated) vmInput.Dispose();
            }

            int result;
            if (!TryGetResult(out result))
            {
                result = 0;
                LastError = "VisionMaster result was not found. Configure a Procedure output named Result/out, or add a Format module with a readable output such as format_out.";
            }

            if (cacheOutputImage)
            {
                Bitmap output = TryGetOutputImage("OutputImage") ?? TryGetOutputImage("ImageData");
                ReplaceBitmap(ref _lastOutputImage, output ?? inputImage);
                output?.Dispose();
            }
            else
            {
                _lastOutputImage?.Dispose();
                _lastOutputImage = null;
            }
            return result;
        }

        /// <summary>
        /// Reads a numeric output from the most recently executed VisionMaster procedure.
        /// The output name must match the name configured in the .prc file.
        /// </summary>
        public bool TryGetNumericOutput(string outputName, out double value)
        {
            value = 0d;
            if (_disposed || Procedure == null || string.IsNullOrWhiteSpace(outputName))
                return false;

            if (TryReadNumericOutputFromObject(Procedure.ModuResult, outputName, out value))
                return true;

            // VisionMaster projects often expose tool outputs on a module result rather
            // than on Procedure.ModuResult. Search modules from last to first, matching
            // the execution order used by TryGetResult().
            object[] modules = ToObjectArray(EnumerateProcedureModules());
            for (int i = modules.Length - 1; i >= 0; i--)
            {
                object module = modules[i];
                if (module == null) continue;
                object moduleResult = GetPropertyValue(module, "ModuResult");
                if (TryReadNumericOutputFromObject(moduleResult, outputName, out value))
                    return true;
            }

            return false;
        }

        private bool TryReadNumericOutputFromObject(
            object resultObject, string outputName, out double value)
        {
            value = 0d;
            if (resultObject == null) return false;

            object rawValue = GetPropertyOrFieldValue(resultObject, outputName);
            if (rawValue == null)
            {
                foreach (string methodName in new[]
                {
                    "GetOutputDouble", "GetOutputFloat", "GetOutputInt",
                    "GetOutputLong", "GetOutputString"
                })
                {
                    try
                    {
                        MethodInfo method = resultObject.GetType().GetMethod(
                            methodName, new[] { typeof(string) });
                        if (method == null) continue;

                        rawValue = method.Invoke(resultObject, new object[] { outputName });
                        if (rawValue != null) break;
                    }
                    catch { }
                }
            }

            return TryConvertOutputDouble(rawValue, out value);
        }

        private static bool TryConvertOutputDouble(object rawValue, out double value)
        {
            value = 0d;
            if (rawValue == null) return false;

            try
            {
                if (rawValue is IConvertible && !(rawValue is string))
                {
                    value = Convert.ToDouble(rawValue, CultureInfo.InvariantCulture);
                    return !double.IsNaN(value) && !double.IsInfinity(value);
                }
            }
            catch { }

            string text;
            if (!TryExtractString(rawValue, out text, 0) || string.IsNullOrWhiteSpace(text))
            {
                object nestedValue = GetPropertyOrFieldValue(rawValue, "Value");
                if (nestedValue != null && !ReferenceEquals(nestedValue, rawValue))
                    return TryConvertOutputDouble(nestedValue, out value);
                return false;
            }

            return (double.TryParse(text.Trim(), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out value) ||
                    double.TryParse(text.Trim(), NumberStyles.Float,
                        CultureInfo.CurrentCulture, out value)) &&
                   !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private ImageSourceModuleCs.ImageSourceModuleTool _imageSourceModule;
        public string ImageSourceModuleName { get; private set; } = "";

        /// <summary>
        /// Enable the result callbacks required by VmRenderControl and VisionMaster editor controls.
        /// This is intentionally called before every SDK-triggered run because VisionMaster can
        /// disable callbacks globally for performance.
        /// </summary>
        public void EnableDisplayCallbacks()
        {
            try { Procedure?.EnableResultCallback(); } catch { }
            try { ResolveImageSourceModule()?.EnableResultCallback(); } catch { }
            try
            {
                object displayModule = GetDisplayModule();
                MethodInfo enable = displayModule?.GetType().GetMethod("EnableResultCallback", Type.EmptyTypes);
                enable?.Invoke(displayModule, null);
            }
            catch { }
        }

        /// <summary>Returns the Image Source module used for SDK image injection, if available.</summary>
        public ImageSourceModuleCs.ImageSourceModuleTool GetImageSourceModule()
        {
            try { return ResolveImageSourceModule(); }
            catch { return null; }
        }

        /// <summary>
        /// Returns the module that should be used as VmRenderControl.ModuleSource on MAIN.
        /// Binding the Image Source only displays the raw image. Binding an inspection module
        /// (Blob Analysis, Circle Find, Match, Line Find...) lets VisionMaster render its native graphics.
        /// A module whose name contains Display/Graphic is preferred; otherwise the last inspection
        /// module in the procedure is selected while utility modules are skipped.
        /// </summary>
        public object GetDisplayModule()
        {
            if (_displayModule != null) return _displayModule;

            object best = null;
            int bestScore = int.MinValue;
            int order = 0;
            foreach (object module in EnumerateProcedureModules())
            {
                if (module == null) continue;
                string name = GetObjectName(module);
                string typeName = module.GetType().Name ?? "";
                string key = (name + " " + typeName).ToLowerInvariant();
                int score = ScoreDisplayModule(key, order++);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = module;
                    DisplayModuleName = name;
                }
            }

            // If there is no inspection tool, falling back to Image Source is still better than blank.
            if (best == null || bestScore < 0)
            {
                best = GetImageSourceModule();
                DisplayModuleName = ImageSourceModuleName;
            }

            _displayModule = best;
            return _displayModule;
        }

        private static int ScoreDisplayModule(string key, int order)
        {
            if (string.IsNullOrWhiteSpace(key)) return int.MinValue / 2;
            if (key.Contains("image source") || key.Contains("imagesource")) return -10000;
            if (key.Contains("image output") || key.Contains("imageoutput")) return -9000;
            if (key.Contains("format") || key.Contains("string") || key.Contains("save image") || key.Contains("saveimage")) return -8000;
            if (key.Contains("display") || key.Contains("graphic")) return 10000 + order;

            string[] visionKeywords =
            {
                "blob", "circle", "line", "match", "measure", "find", "ocr", "code", "defect",
                "segment", "classif", "locate", "edge", "caliper", "contour", "pattern", "inspect",
                "geometry", "ellipse", "rect", "point", "feature", "track"
            };
            foreach (string word in visionKeywords) if (key.Contains(word)) return 5000 + order;

            // Keep ordinary processing modules as a low priority fallback. Later modules win ties.
            return 100 + order;
        }

        private IEnumerable EnumerateProcedureModules()
        {
            if (Procedure == null) yield break;
            object list = null;
            try { list = Procedure.GetProcedureModuleList(); } catch { }
            if (!(list is IEnumerable))
            {
                try { list = Procedure.GetAllModuleList(); } catch { }
            }
            if (list is IEnumerable enumerable)
                foreach (object item in enumerable) yield return item;
        }

        private static string GetObjectName(object obj)
        {
            if (obj == null) return "";
            foreach (string propertyName in new[] { "Name", "StrModuleName", "UniqueName" })
            {
                try
                {
                    PropertyInfo p = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    object value = p?.GetValue(obj, null);
                    if (value != null && !string.IsNullOrWhiteSpace(value.ToString())) return value.ToString();
                }
                catch { }
            }
            return obj.GetType().Name;
        }

        private ImageBaseData SetInputImage(Bitmap bitmap)
        {
            ImageBaseData imageData = new ImageBaseData(bitmap);
            try
            {
                ImageSourceModuleCs.ImageSourceModuleTool imageSource = ResolveImageSourceModule();
                // VM 4.3 added this wait flag. true blocks until the SDK has finished copying the
                // whole frame and is very noticeable with multi-megapixel images. Keep imageData
                // alive through Procedure.Run(), but let VisionMaster perform the copy asynchronously.
                imageSource.SetImageData(imageData, false);
                return imageData;
            }
            catch (Exception ex)
            {
                imageData.Dispose();
                throw new InvalidOperationException(
                    "Cannot send image to VisionMaster Image Source. Set the Image Source module to SDK mode. " +
                    "The adapter searches common names such as 'InputImage', 'Image Source', 'Image Source1' and indexed variants.", ex);
            }
        }

        private ImageSourceModuleCs.ImageSourceModuleTool ResolveImageSourceModule()
        {
            if (_imageSourceModule != null) return _imageSourceModule;

            string[] preferredNames =
            {
                "InputImage", "Image Source", "Image Source1", "ImageSource", "ImageSource1",
                "0 Image Source", "0Image Source", "0 ImageSource", "0ImageSource"
            };

            foreach (string name in preferredNames)
            {
                ImageSourceModuleCs.ImageSourceModuleTool module = TryGetImageSourceModule(name);
                if (module != null) return CacheImageSourceModule(module, name);
            }

            for (int i = 0; i <= 50; i++)
            {
                string[] names =
                {
                    "Image Source" + i, "ImageSource" + i,
                    i + " Image Source", i + "Image Source",
                    i + " ImageSource", i + "ImageSource"
                };

                foreach (string name in names)
                {
                    ImageSourceModuleCs.ImageSourceModuleTool module = TryGetImageSourceModule(name);
                    if (module != null) return CacheImageSourceModule(module, name);
                }
            }

            throw new InvalidOperationException(
                "No VisionMaster Image Source module was found. Add an Image Source module to the procedure, " +
                "set Image Source = SDK, and preferably rename the module to 'InputImage' or keep the default name 'Image Source1'.");
        }

        private ImageSourceModuleCs.ImageSourceModuleTool TryGetImageSourceModule(string name)
        {
            try
            {
                return Procedure[name] as ImageSourceModuleCs.ImageSourceModuleTool;
            }
            catch
            {
                return null;
            }
        }

        private ImageSourceModuleCs.ImageSourceModuleTool CacheImageSourceModule(ImageSourceModuleCs.ImageSourceModuleTool module, string name)
        {
            _imageSourceModule = module;
            ImageSourceModuleName = name ?? "";
            return module;
        }

        private static Bitmap EnsureVisionMasterPixelFormat(Bitmap source, out bool created)
        {
            created = false;
            if (source.PixelFormat == PixelFormat.Format8bppIndexed || source.PixelFormat == PixelFormat.Format24bppRgb) return source;

            Bitmap converted = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(converted)) g.DrawImageUnscaled(source, 0, 0);
            created = true;
            return converted;
        }

        private bool TryGetResult(out int result)
        {
            result = 0;

            // Hot path after the first successful trigger: reuse the exact result object/output
            // instead of enumerating every module and probing many output names each frame.
            if (_cachedResultObject != null && !string.IsNullOrWhiteSpace(_cachedResultOutputName))
            {
                if (TryReadResultValue(_cachedResultObject, _cachedResultOutputName, out result, out string cachedText))
                {
                    LastResultSource = _cachedResultSource;
                    LastResultText = cachedText;
                    return true;
                }
                _cachedResultObject = null;
                _cachedResultOutputName = "";
                _cachedResultSource = "";
            }

            // 1) Best/cleanest contract: Procedure output Result/result/OK/out.
            foreach (string outputName in new[] { "Result", "result", "OK", "ok", "out" })
            {
                if (TryReadResultValue(Procedure?.ModuResult, outputName, out result, out string text))
                {
                    LastResultSource = "Procedure." + outputName;
                    LastResultText = text;
                    CacheResultAccessor(Procedure?.ModuResult, outputName, LastResultSource);
                    return true;
                }
            }

            // 2) VisionMaster projects often finish with a Format module instead of exposing a
            // Procedure output. Read format_out (or another meaningful output) automatically.
            object[] modules = ToObjectArray(EnumerateProcedureModules());
            for (int i = modules.Length - 1; i >= 0; i--)
            {
                object module = modules[i];
                string moduleName = GetObjectName(module);
                string key = (moduleName + " " + module.GetType().Name).ToLowerInvariant();
                if (!key.Contains("format")) continue;

                object moduResult = GetPropertyValue(module, "ModuResult");
                foreach (string outputName in new[] { "format_out", "FormatOut", "Result", "result", "out", "Output", "Value" })
                {
                    if (TryReadResultValue(moduResult, outputName, out result, out string text))
                    {
                        LastResultSource = moduleName + "." + outputName;
                        LastResultText = text;
                        CacheResultAccessor(moduResult, outputName, LastResultSource);
                        return true;
                    }
                }

                if (TryReadFirstMeaningfulOutput(moduResult, out result, out string actualName, out string actualText))
                {
                    LastResultSource = moduleName + "." + actualName;
                    LastResultText = actualText;
                    CacheResultAccessor(moduResult, actualName, LastResultSource);
                    return true;
                }
            }

            // 3) Final fallback: if the inspection module itself exposes Result/OK/out, accept it.
            for (int i = modules.Length - 1; i >= 0; i--)
            {
                object module = modules[i];
                string moduleName = GetObjectName(module);
                string key = (moduleName + " " + module.GetType().Name).ToLowerInvariant();
                if (key.Contains("image source") || key.Contains("imagesource") || key.Contains("image output") || key.Contains("imageoutput")) continue;
                object moduResult = GetPropertyValue(module, "ModuResult");
                foreach (string outputName in new[] { "Result", "result", "OK", "ok", "out" })
                {
                    if (TryReadResultValue(moduResult, outputName, out result, out string text))
                    {
                        LastResultSource = moduleName + "." + outputName;
                        LastResultText = text;
                        CacheResultAccessor(moduResult, outputName, LastResultSource);
                        return true;
                    }
                }
            }

            return false;
        }

        private void CacheResultAccessor(object resultObject, string outputName, string source)
        {
            if (resultObject == null || string.IsNullOrWhiteSpace(outputName)) return;
            _cachedResultObject = resultObject;
            _cachedResultOutputName = outputName;
            _cachedResultSource = source ?? "";
        }

        private bool TryReadResultValue(object resultObject, string outputName, out int result, out string text)
        {
            result = 0;
            text = "";
            if (resultObject == null || string.IsNullOrWhiteSpace(outputName)) return false;

            // Generated ModuResult classes commonly expose outputs as normal properties/fields.
            object value = GetPropertyOrFieldValue(resultObject, outputName);
            if (value != null && TryConvertResultValue(value, out result, out text)) return true;

            // Dynamic result objects expose GetOutputInt/GetOutputFloat/GetOutputString.
            foreach (string methodName in new[] { "GetOutputInt", "GetOutputLong", "GetOutputFloat", "GetOutputDouble", "GetOutputBool", "GetOutputString" })
            {
                try
                {
                    MethodInfo method = resultObject.GetType().GetMethod(methodName, new[] { typeof(string) });
                    if (method == null) continue;
                    value = method.Invoke(resultObject, new object[] { outputName });
                    if (value != null && TryConvertResultValue(value, out result, out text)) return true;
                }
                catch { }
            }
            return false;
        }

        private bool TryReadFirstMeaningfulOutput(object resultObject, out int result, out string outputName, out string text)
        {
            result = 0;
            outputName = "";
            text = "";
            if (resultObject == null) return false;

            // First inspect public generated properties such as FormatResult.format_out.
            foreach (PropertyInfo p in resultObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead || p.GetIndexParameters().Length != 0 || IsIgnoredResultMember(p.Name)) continue;
                try
                {
                    object value = p.GetValue(resultObject, null);
                    if (TryConvertResultValue(value, out result, out text)) { outputName = p.Name; return true; }
                }
                catch { }
            }
            foreach (FieldInfo f in resultObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsIgnoredResultMember(f.Name)) continue;
                try
                {
                    object value = f.GetValue(resultObject);
                    if (TryConvertResultValue(value, out result, out text)) { outputName = f.Name; return true; }
                }
                catch { }
            }

            // Then use the SDK's output-name list if this result type supports it.
            try
            {
                MethodInfo allMethod = resultObject.GetType().GetMethod("GetAllOutputNameInfo", Type.EmptyTypes);
                object infos = allMethod?.Invoke(resultObject, null);
                if (infos is IEnumerable enumerable)
                {
                    foreach (object info in enumerable)
                    {
                        string name = GetPropertyOrFieldValue(info, "Name")?.ToString();
                        if (string.IsNullOrWhiteSpace(name) || IsIgnoredResultMember(name)) continue;
                        if (TryReadResultValue(resultObject, name, out result, out text)) { outputName = name; return true; }
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool TryConvertResultValue(object value, out int result, out string text)
        {
            result = 0;
            text = "";
            if (value == null) return false;

            if (TryExtractString(value, out string stringValue, 0) && !string.IsNullOrWhiteSpace(stringValue))
            {
                text = stringValue.Trim();
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)) return true;
                if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double d)) { result = Convert.ToInt32(d); return true; }
                string normalized = text.ToUpperInvariant();
                if (normalized == "OK" || normalized == "PASS" || normalized == "TRUE") { result = 1; return true; }
                if (normalized == "NG" || normalized == "FAIL" || normalized == "FALSE") { result = 0; return true; }
            }

            if (TryExtractInt(value, out result, 0))
            {
                text = result.ToString(CultureInfo.InvariantCulture);
                return true;
            }
            return false;
        }

        private static bool IsIgnoredResultMember(string name)
        {
            string n = (name ?? "").ToLowerInvariant();
            return string.IsNullOrWhiteSpace(n) || n.Contains("status") || n.Contains("reserved") ||
                   n.Contains("image") || n.Contains("mask") || n.Contains("time") || n.Contains("count") ||
                   n.Contains("num") || n == "item";
        }

        private static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;
            try
            {
                PropertyInfo p = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                return p?.CanRead == true ? p.GetValue(obj, null) : null;
            }
            catch { return null; }
        }

        private static object GetPropertyOrFieldValue(object obj, string name)
        {
            if (obj == null || string.IsNullOrWhiteSpace(name)) return null;
            try
            {
                PropertyInfo p = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null && p.CanRead && p.GetIndexParameters().Length == 0) return p.GetValue(obj, null);
            }
            catch { }
            try
            {
                FieldInfo f = obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (f != null) return f.GetValue(obj);
            }
            catch { }
            return null;
        }

        private static object[] ToObjectArray(IEnumerable items)
        {
            if (items == null) return new object[0];
            System.Collections.Generic.List<object> list = new System.Collections.Generic.List<object>();
            foreach (object item in items) list.Add(item);
            return list.ToArray();
        }

        private bool TryInvokeOutputGetter(string outputName, out object value)
        {
            value = null;
            object resultObject = Procedure?.ModuResult;
            if (resultObject == null) return false;

            string[] preferredMethods = { "GetOutputInt", "GetOutputLong", "GetOutputFloat", "GetOutputDouble", "GetOutputBool" };
            foreach (string methodName in preferredMethods)
            {
                try
                {
                    MethodInfo method = resultObject.GetType().GetMethod(methodName, new[] { typeof(string) });
                    if (method == null) continue;
                    value = method.Invoke(resultObject, new object[] { outputName });
                    if (value != null) return true;
                }
                catch { }
            }
            return false;
        }

        private bool TryGetStringOutput(string outputName, out string text)
        {
            text = null;
            try
            {
                object value = Procedure.ModuResult.GetOutputString(outputName);
                return TryExtractString(value, out text, 0);
            }
            catch
            {
                return false;
            }
        }

        private Bitmap TryGetOutputImage(string outputName)
        {
            ImageBaseData data = null;
            try
            {
                data = Procedure.ModuResult.GetOutputImageV2(outputName);
                if (data == null) return null;

                using (Bitmap bitmap = data.ToBitmap())
                {
                    if (bitmap == null) return null;
                    return new Bitmap(bitmap);
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                data?.Dispose();
            }
        }

        private static bool TryExtractInt(object value, out int result, int depth)
        {
            result = 0;
            if (value == null || depth > 5) return false;

            try
            {
                if (value is bool b) { result = b ? 1 : 0; return true; }
                if (value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal)
                {
                    result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    return true;
                }

                if (value is IEnumerable enumerable && !(value is string))
                {
                    foreach (object item in enumerable)
                        if (TryExtractInt(item, out result, depth + 1)) return true;
                    return false;
                }

                Type t = value.GetType();
                string[] preferredNames = { "pIntValue", "pIntVal", "pFloatValue", "pFloatVal", "Value", "Val", "Result" };

                foreach (string name in preferredNames)
                {
                    PropertyInfo p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (p != null && p.CanRead && p.GetIndexParameters().Length == 0)
                    {
                        try { if (TryExtractInt(p.GetValue(value, null), out result, depth + 1)) return true; } catch { }
                    }

                    FieldInfo f = t.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (f != null)
                    {
                        try { if (TryExtractInt(f.GetValue(value), out result, depth + 1)) return true; } catch { }
                    }
                }

                foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!p.CanRead || p.GetIndexParameters().Length != 0) continue;
                    string n = p.Name.ToLowerInvariant();
                    if (n.Contains("status") || n.Contains("num") || n.Contains("count") || n.Contains("reserved")) continue;
                    if (!n.Contains("value") && !n.Contains("val") && !n.Contains("result")) continue;
                    try { if (TryExtractInt(p.GetValue(value, null), out result, depth + 1)) return true; } catch { }
                }

                foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    string n = f.Name.ToLowerInvariant();
                    if (n.Contains("status") || n.Contains("num") || n.Contains("count") || n.Contains("reserved")) continue;
                    if (!n.Contains("value") && !n.Contains("val") && !n.Contains("result")) continue;
                    try { if (TryExtractInt(f.GetValue(value), out result, depth + 1)) return true; } catch { }
                }
            }
            catch { }

            return false;
        }

        private static bool TryExtractString(object value, out string text, int depth)
        {
            text = null;
            if (value == null || depth > 5) return false;
            if (value is string s) { text = s; return true; }

            if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                    if (TryExtractString(item, out text, depth + 1)) return true;
                return false;
            }

            Type t = value.GetType();
            string[] preferredNames = { "astStringVal", "astStringValue", "strValue", "Value", "Val", "Text" };

            foreach (string name in preferredNames)
            {
                PropertyInfo p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null && p.CanRead && p.GetIndexParameters().Length == 0)
                {
                    try { if (TryExtractString(p.GetValue(value, null), out text, depth + 1)) return true; } catch { }
                }

                FieldInfo f = t.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (f != null)
                {
                    try { if (TryExtractString(f.GetValue(value), out text, depth + 1)) return true; } catch { }
                }
            }

            foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead || p.GetIndexParameters().Length != 0) continue;
                string n = p.Name.ToLowerInvariant();
                if (n.Contains("status") || n.Contains("num") || n.Contains("count") || n.Contains("reserved")) continue;
                if (!n.Contains("str") && !n.Contains("text") && !n.Contains("value") && !n.Contains("val")) continue;
                try { if (TryExtractString(p.GetValue(value, null), out text, depth + 1)) return true; } catch { }
            }

            foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                string n = f.Name.ToLowerInvariant();
                if (n.Contains("status") || n.Contains("num") || n.Contains("count") || n.Contains("reserved")) continue;
                if (!n.Contains("str") && !n.Contains("text") && !n.Contains("value") && !n.Contains("val")) continue;
                try { if (TryExtractString(f.GetValue(value), out text, depth + 1)) return true; } catch { }
            }

            return false;
        }

        private static void ReplaceBitmap(ref Bitmap target, Bitmap source)
        {
            target?.Dispose();
            target = source == null ? null : new Bitmap(source);
        }

        private volatile bool _disposed;

        /// <summary>
        /// VisionMaster 4.3 lazily loads IO subscription links. VmProcedure.Dispose() first
        /// removes the procedure from the native runtime and then clears managed IO collections.
        /// If a RelateIO link has never been materialized, the managed cleanup can try to query
        /// a module that has already been deleted and VisionMaster shows "Assertion Failed / 找不到输入".
        /// Load every module relation while the complete procedure graph still exists.
        /// </summary>
        private void PrepareProcedureForDispose(VmProcedure procedure)
        {
            if (procedure == null) return;
            LoadModuleRelateIoSafe(procedure);

            object modules = null;
            try { modules = procedure.GetAllModuleList(); } catch { }
            if (!(modules is IEnumerable enumerable)) return;
            foreach (object module in enumerable) LoadModuleRelateIoSafe(module);
        }

        private static void LoadModuleRelateIoSafe(object module)
        {
            if (module == null) return;
            try
            {
                MethodInfo method = module.GetType().GetMethod("LoadModuleRelateIO", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                method?.Invoke(module, null);
            }
            catch { }
        }

        // The VM SDK uses Debug.Assert for this known lazy-RelateIO cleanup condition.
        // Keep assertion UI disabled only while releasing a VM procedure so an SDK assert can
        // never block an unattended machine or application shutdown. The previous setting is restored.
        private static List<KeyValuePair<DefaultTraceListener, bool>> DisableAssertionDialogs()
        {
            List<KeyValuePair<DefaultTraceListener, bool>> states = new List<KeyValuePair<DefaultTraceListener, bool>>();
            try
            {
                foreach (TraceListener listener in Debug.Listeners)
                {
                    DefaultTraceListener defaultListener = listener as DefaultTraceListener;
                    if (defaultListener == null) continue;
                    states.Add(new KeyValuePair<DefaultTraceListener, bool>(defaultListener, defaultListener.AssertUiEnabled));
                    defaultListener.AssertUiEnabled = false;
                }
            }
            catch { }
            return states;
        }

        private static void RestoreAssertionDialogs(List<KeyValuePair<DefaultTraceListener, bool>> states)
        {
            if (states == null) return;
            foreach (KeyValuePair<DefaultTraceListener, bool> state in states)
                try { state.Key.AssertUiEnabled = state.Value; } catch { }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _lastInputImage?.Dispose();
            _lastInputImage = null;
            _lastOutputImage?.Dispose();
            _lastOutputImage = null;
            _imageSourceModule = null;
            _displayModule = null;
            ImageSourceModuleName = "";
            DisplayModuleName = "";

            VmProcedure procedure = Procedure;
            if (procedure != null)
            {
                List<KeyValuePair<DefaultTraceListener, bool>> assertStates = DisableAssertionDialogs();
                try
                {
                    PrepareProcedureForDispose(procedure);
                    procedure.Dispose();
                }
                catch { }
                finally
                {
                    Procedure = null;
                    RestoreAssertionDialogs(assertStates);
                }
            }
            else Procedure = null;

            GC.SuppressFinalize(this);
        }
    }
}
