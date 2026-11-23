using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using VM.Core;
using VM.PlatformSDKCS;
using VMControls.Winform.Release;

namespace SLTtechSoft.Class
{
    public static class VisionMasterUiHelper
    {
        public static void RunOnUiThread(Control control, Action action)
        {
            if (action == null) return;
            if (control == null || control.IsDisposed) return;

            try
            {
                if (control.InvokeRequired) control.Invoke(action);
                else action();
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        /// <summary>
        /// VisionMaster can globally disable module callbacks for performance. The embedded
        /// editor and VmRenderControl require those callbacks to show images/results.
        /// </summary>
        public static void EnableAllModuleCallbacks()
        {
            try { VmSolution.Instance?.EnableModulesCallback(); } catch { }
        }


        /// <summary>
        /// For an Image Source module configured as SDK, HIK's own PlatformSDK sample binds
        /// VmRenderControl.ModuleSource directly to the ImageSourceModuleTool.  This is more
        /// reliable than binding the procedure when the input image is injected by SetImageData().
        /// </summary>

        /// <summary>
        /// Bind VmRenderControl to an inspection module instead of Image Source.
        /// This is the official VisionMaster pattern used by the CircleFind sample and
        /// allows the module's native result graphics/overlays to be rendered on MAIN.
        /// </summary>
        public static void BindModule(VmRenderControl display, object module)
        {
            if (display == null || module == null) return;
            try
            {
                MethodInfo enable = module.GetType().GetMethod("EnableResultCallback", Type.EmptyTypes);
                enable?.Invoke(module, null);
            }
            catch { }

            RunOnUiThread(display, () =>
            {
                try
                {
                    display.SetRenderInterval(0);
                    PropertyInfo p = display.GetType().GetProperty("ModuleSource", BindingFlags.Public | BindingFlags.Instance);
                    p?.SetValue(display, module, null);
                    display.Refresh();
                    display.Invalidate();
                    display.Update();
                }
                catch { }
            });
        }

        public static void RefreshModule(VmRenderControl display, object module, bool fit)
        {
            if (display == null || module == null) return;
            RunOnUiThread(display, () =>
            {
                try
                {
                    PropertyInfo p = display.GetType().GetProperty("ModuleSource", BindingFlags.Public | BindingFlags.Instance);
                    object current = null;
                    try { current = p?.GetValue(display, null); } catch { }
                    if (!ReferenceEquals(current, module)) p?.SetValue(display, module, null);
                    display.Refresh();
                    display.Invalidate(true);
                    display.Update();
                    if (fit) display.InitView();
                }
                catch { }
            });
        }

        public static void BindImageSource(VmRenderControl display, ImageSourceModuleCs.ImageSourceModuleTool imageSource)
        {
            if (display == null || imageSource == null) return;
            try { imageSource.EnableResultCallback(); } catch { }

            RunOnUiThread(display, () =>
            {
                try
                {
                    display.SetRenderInterval(0);
                    display.ModuleSource = imageSource;
                    display.Refresh();
                    display.Invalidate();
                    display.Update();
                }
                catch { }
            });
        }

        public static void RefreshImageSource(VmRenderControl display, ImageSourceModuleCs.ImageSourceModuleTool imageSource, bool fit)
        {
            if (display == null || imageSource == null) return;
            RunOnUiThread(display, () =>
            {
                try
                {
                    if (!ReferenceEquals(display.ModuleSource, imageSource)) display.ModuleSource = imageSource;
                    display.Refresh();
                    display.Invalidate();
                    display.Update();
                    if (fit) display.InitView();
                }
                catch { }
            });
        }

        public static void BindProcedure(VmRenderControl display, VmProcedure procedure)
        {
            if (display == null || procedure == null) return;
            try { procedure.EnableResultCallback(); } catch { }

            RunOnUiThread(display, () =>
            {
                try
                {
                    display.SetRenderInterval(0);
                    display.ModuleSource = procedure;
                    display.Refresh();
                    display.Invalidate();
                }
                catch { }
            });
        }

        /// <summary>
        /// Refreshes a render control after Procedure.Run(). Binding is kept on the same
        /// procedure instance so VisionMaster can render the latest image and graphics.
        /// </summary>
        public static void RefreshRender(VmRenderControl display, VmProcedure procedure, bool fit)
        {
            if (display == null || procedure == null) return;
            RunOnUiThread(display, () =>
            {
                try
                {
                    if (!ReferenceEquals(display.ModuleSource, procedure)) display.ModuleSource = procedure;
                    display.Refresh();
                    display.Invalidate();
                    display.Update();
                    if (fit) display.InitView();
                }
                catch { }
            });
        }

        public static void ShowBitmap(VmRenderControl display, Bitmap bitmap)
        {
            if (display == null || bitmap == null) return;

            // Different VisionMaster 4.x builds expose ImageSource as an ImageData type from
            // VMControls.RenderInterface. Construct that type by reflection when possible.
            // This keeps live-camera preview compatible with multiple VM 4.x revisions.
            Bitmap copy = new Bitmap(bitmap);
            RunOnUiThread(display, () =>
            {
                object renderImage = null;
                ImageBaseData baseData = null;
                try
                {
                    PropertyInfo property = display.GetType().GetProperty("ImageSource", BindingFlags.Public | BindingFlags.Instance);
                    if (property == null || !property.CanWrite) return;

                    Type targetType = property.PropertyType;

                    if (targetType.IsAssignableFrom(typeof(Bitmap)))
                    {
                        renderImage = copy;
                    }
                    else
                    {
                        baseData = new ImageBaseData(copy);

                        if (targetType.IsAssignableFrom(typeof(ImageBaseData)))
                        {
                            renderImage = baseData;
                        }
                        else
                        {
                            ConstructorInfo ctor = targetType.GetConstructor(new[] { typeof(Bitmap) });
                            if (ctor != null) renderImage = ctor.Invoke(new object[] { copy });

                            if (renderImage == null)
                            {
                                ctor = targetType.GetConstructor(new[] { typeof(ImageBaseData) });
                                if (ctor != null) renderImage = ctor.Invoke(new object[] { baseData });
                            }

                            if (renderImage == null)
                            {
                                foreach (string methodName in new[] { "FromBitmap", "Create", "CreateFromBitmap" })
                                {
                                    MethodInfo method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Bitmap) }, null);
                                    if (method == null) continue;
                                    renderImage = method.Invoke(null, new object[] { copy });
                                    if (renderImage != null) break;
                                }
                            }
                        }
                    }

                    if (renderImage != null && targetType.IsInstanceOfType(renderImage))
                    {
                        property.SetValue(display, renderImage, null);
                        display.Refresh();
                        display.Invalidate();
                        display.Update();
                    }
                }
                catch { }
                finally
                {
                    // If ImageSource directly keeps ImageBaseData, do not dispose it here.
                    // The render control owns the assigned data. Otherwise it was only used
                    // as a temporary conversion source and can be released.
                    if (baseData != null && !ReferenceEquals(renderImage, baseData)) baseData.Dispose();
                    if (!ReferenceEquals(renderImage, copy)) copy.Dispose();
                }
            });
        }

        public static void Fit(VmRenderControl display)
        {
            if (display == null) return;
            RunOnUiThread(display, () =>
            {
                try { display.InitView(); } catch { }
            });
        }

        /// <summary>
        /// Use VmMainViewConfigControl instead of VmProcedureConfigControl. The MainView
        /// control is the closer equivalent of the legacy Cognex tool editor and supports the
        /// procedure work area plus module configuration/render views.
        /// </summary>
        public static void BindProcedureEditor(VmMainViewConfigControl editor, VmProcedure procedure)
        {
            if (editor == null || procedure == null) return;
            string procedureName = procedure.Name;
            if (string.IsNullOrWhiteSpace(procedureName)) procedureName = procedure.UniqueName;
            if (string.IsNullOrWhiteSpace(procedureName)) throw new InvalidOperationException("VisionMaster procedure name is empty.");

            try { procedure.EnableResultCallback(); } catch { }
            EnableAllModuleCallbacks();

            RunOnUiThread(editor, () =>
            {
                editor.BindSingleProcedure(procedureName);
                editor.SetParamTabEditable(true);
                editor.Refresh();
                editor.Invalidate();
            });
        }

        public static void RefreshProcedureEditor(VmMainViewConfigControl editor)
        {
            if (editor == null) return;
            RunOnUiThread(editor, () =>
            {
                try
                {
                    editor.Refresh();
                    editor.Invalidate(true);
                    editor.Update();
                }
                catch { }
            });
        }

        public static Bitmap SaveRenderedImage(VmRenderControl display, string path, Bitmap fallback = null)
        {
            if (display != null && !display.IsDisposed)
            {
                Bitmap result = null;
                RunOnUiThread(display, () =>
                {
                    try
                    {
                        display.SaveRenderedImage(path, 95);
                        result = LoadBitmapUnlocked(path);
                    }
                    catch { }
                });
                if (result != null) return result;
            }

            Bitmap captured = Capture(display, fallback);
            if (captured != null)
            {
                captured.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                return captured;
            }
            return null;
        }

        public static Bitmap LoadBitmapUnlocked(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path)) return null;
            using (Bitmap source = new Bitmap(path)) return new Bitmap(source);
        }

        public static Bitmap Capture(Control control, Bitmap fallback = null)
        {
            if (control != null && !control.IsDisposed && control.Width > 0 && control.Height > 0)
            {
                Bitmap result = null;
                RunOnUiThread(control, () =>
                {
                    try
                    {
                        Bitmap bitmap = new Bitmap(control.Width, control.Height);
                        control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, control.Size));
                        result = bitmap;
                    }
                    catch { }
                });
                if (result != null) return result;
            }
            return fallback == null ? null : new Bitmap(fallback);
        }
    }
}
