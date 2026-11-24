using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SLTtechSoft
{
    public class PlcDic
    {
        //Read
        //AutoBits
        public static string Auto = "Auto";
        public static string Stop = "Stop";
        public static string ComFlash = "ComFlash";
        public static string ComOn = "ComOn";
        public static string[] TriggerCam = new string[] { "TriggerCam1", "TriggerCam2", "TriggerCam3", "TriggerCam4" };
        public static string[] LightOn = new string[] { "LightOn1", "LightOn2", "LightOn3", "LightOn4" };

        public static string Trigger_ReadCode = "Trigger_ReadCode";
        public static string DonePosCam1 = "DonePosCam1";

        public static string[] FinishTrigger = new string[] { "FinishTrigger1", "FinishTrigger2", "FinishTrigger3", "FinishTrigger4" };
        public static string[] ResultCam = new string[] { "ResultCam1", "ResultCam2", "ResultCam3", "ResultCam4" };
        public static string ModelNo = "ModelNo";
        /// <summary>
        /// bit
        /// </summary>
        public static string MarkingStartConfirm = "MarkingStartConfirm";
        public static string MarkingCompleteConfirm = "MarkingCompleteConfirm";
        public static string StartMarkingProcess = "StartMarkingProcess";
        public static string MarkingOneDone = "MarkingOneDone";
        public static string Require_CheckFiducial = "Require_CheckFiducial";
        public static string StartCheckFiducial = "StartCheckFiducial";
        public static string Require_CheckCode = "Require_CheckCode";
        public static string StartCheckCode = "StartCheckCode";
        public static string ScanImageTop = "ScanImageTop";
        public static string ScanImageBottom = "ScanImageBottom";
        /// <summary>
        /// WORD 
        /// </summary>
        public static string CurentPosX = "CurentPosX";
        public static string CurentPosY = "CurentPosY";
        //Write
        /// <summary>
        /// bit
        /// </summary>
        public static string WriteDataToPlcDone = "WriteDataToPlcDone";
        public static string MarkingStart = "MarkingStart";
        public static string Restart_Marking = "Restart_Marking";
        public static string MarkingComplete = "MarkingComplete";
        public static string CheckFiducialOneDone = "CheckFiducialOneDone";
        public static string CheckFiducialALLDone = "CheckFiducialALLDone";
        public static string ResultFace_Corrrect = "ResultFace_Corrrect";
        public static string CheckCodeOneDone = "CheckCodeOneDone";
        public static string CheckCodeALLDone = "CheckCodeALLDone";
        public static string Result_Code_Exists = "Result_Code_Exists";
        public static string ScanImageTopDone = "ScanImageTopDone";
        public static string ScanImageBottomDone = "ScanImageBottomDone";
        /// <summary>
        /// Word 
        /// </summary>
        public static string MarkingPosX = "MarkingPosX";
        public static string MarkingPosY = "MarkingPosY";
        public static string CheckFiducial_PosX = "CheckFiducial_PosX";
        public static string CheckFiducial_PosY = "CheckFiducial_PosY";
        public static string CheckCode_PosX = "CheckCode_PosX";
        public static string CheckCode_PosY = "CheckCode_PosY";

    }


    public class PLCCommunication
    {
        public Read Read = new Read();
        public Write Write = new Write();
    }
    public class Read
    {
        public BitList InputBits = new BitList();
        public BitList AutoBits = new BitList();
      
        public UshortList WordUshorts = new UshortList();
        public IntegerList WordIntegers = new IntegerList();
        public FloatList WordFloats = new FloatList();
        public ushort[] ErrorWord = new ushort[14];
        public bool[] ErrorBits = new bool[14];
    }
    public class Write
    {
        public BitList OuputBits = new BitList();
        public BitList AutoBits = new BitList();

        public UshortList WordUshorts = new UshortList();
        public IntegerList WordIntegers = new IntegerList();
        public FloatList WordFloats = new FloatList();
    }
   
    public class BitList
    {
        private readonly List<string> _names = new List<string>();
        private readonly Dictionary<string, bool> _values = new Dictionary<string, bool>();

        public void Add(string name, bool value = false)
        {
            if (!_values.ContainsKey(name)) _names.Add(name);
            _values[name] = value;
        }

        public bool this[string name]
        {
            get => _values[name];
            set => _values[name] = value;
        }

        public bool this[int index]
        {
            get => _values[_names[index]];
            set => _values[_names[index]] = value;
        }
        public string GetName(int index)
        {
            return _names[index];
        }

        public int GetIndex(string name)
        {
            return _names.IndexOf(name);
        }
        public int Count => _names.Count;
    }
    public class UshortList
    {
        private readonly List<string> _names = new List<string>();
        private readonly Dictionary<string, ushort> _values = new Dictionary<string, ushort>();

        public void Add(string name, ushort value = 0)
        {
            if (!_values.ContainsKey(name)) _names.Add(name);
            _values[name] = value;
        }

        public ushort this[string name]
        {
            get => _values[name];
            set => _values[name] = value;
        }

        public ushort this[int index]
        {
            get => _values[_names[index]];
            set => _values[_names[index]] = value;
        }
        public string GetName(int index)
        {
            return _names[index];
        }

        public int GetIndex(string name)
        {
            return _names.IndexOf(name);
        }
        public int Count => _names.Count;
    }
    public class IntegerList
    {
        private readonly List<string> _names = new List<string>();
        private readonly Dictionary<string, int> _values = new Dictionary<string, int>();

        public void Add(string name, int value = 0)
        {
            if (!_values.ContainsKey(name)) _names.Add(name);
            _values[name] = value;
        }

        public int this[string name]
        {
            get => _values[name];
            set => _values[name] = value;
        }

        public int this[int index]
        {
            get => _values[_names[index]];
            set => _values[_names[index]] = value;
        }
        public string GetName(int index)
        {
            return _names[index];
        }

        public int GetIndex(string name)
        {
            return _names.IndexOf(name);
        }
        public int Count => _names.Count;
    }
    public class FloatList
    {
        private readonly List<string> _names = new List<string>();
        private readonly Dictionary<string, float> _values = new Dictionary<string, float>();

        public void Add(string name, int value = 0)
        {
            if (!_values.ContainsKey(name)) _names.Add(name);
            _values[name] = value;
        }

        public float this[string name]
        {
            get => _values[name];
            set => _values[name] = value;
        }

        public float this[int index]
        {
            get => _values[_names[index]];
            set => _values[_names[index]] = value;
        }
        public string GetName(int index)
        {
            return _names[index];
        }

        public int GetIndex(string name)
        {
            return _names.IndexOf(name);
        }
        public int Count => _names.Count;
    }
}
