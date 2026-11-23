using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentModbusPLC;

namespace SLTtechSoft
{
    public class MachineSetting
    {
        public string MCTitle = "";
        public string Password = "";
    }
    public class MesSetting
    {
        public string HostIP = "0.0.0.0";
        public int Port = 4000;
        public string McCode = "";
        public string CheckCount = "";
        public bool MesEnabel = false;
    }
    public class CamSetting
    {
        public string Name { get; set; }
        public string HostIP = "0.0.0.0";
        public bool Use = false;
        public bool SaveGraphic = false;
        public bool SaveOrigin = false;
        public bool SaveOriginSeparate = false;
        public ImageSource ImageSource = ImageSource.Cam;
        public TriggerMode TriggerMode = TriggerMode.Software;
        public CameraManufacture CameraManufacture = CameraManufacture.HIK;
        public CamConnectType CamConnectType = CamConnectType.Gige;
        public string ImageFolder = "";
        public string ImageFile = "";
        public bool ReverseX = false;
        public bool ReverseY = false;
    }
    public enum CamConnectType
    {
        Gige,
        Usb,

    }
    public enum CameraManufacture
    {
        VisionPro,
        HIK,
        Basler
    }
    public enum ImageSource
    {
        Cam,
        Folder,
        File
    }
    public enum TriggerMode
    {
        Software,
        HardwareAuto,
        HardwareManual
    }
    public class PrinterSetting
    {
        public string PortName = "COM1";
        public int BaudRate = 9600;
        public int DataBits = 0;
        public string Parity = "None";
        public string StopBits = "One";
    }
    public class ScannerSetting
    {
        public string HostIP = "0.0.0.0";
        public int Port = 4000;

        public string PortName = "COM1";
        public int BaudRate = 9600;
        public int DataBits = 0;
        public string Parity = "None";
        public string StopBits = "One";
    }
    public class PLCSetting
    {
        public bool Use = false;
        public string Name { get; set; }
        public Class.Protocol Protocol = Class.Protocol.Modbus;
        public ModbusMode Mode;
        public string HostIP = "0.0.0.0";
        public int Port = 4000;
        public int ThreadInterval = 5;
        public string PortName = "COM1";
        public int BaudRate = 9600;
        public int DataBits = 0;
        public string Parity = "None";
        public string StopBits = "One";

    }
    public class OptionSetting
    {
        public bool CallModelManual { get; set; }

    }
    public class CamDisplayClass
    { 
        public int Column { get; set; }
        public int Row { get; set; }

    }


    public class Parameter
    { 
        public MachineSetting Machine  = new MachineSetting();
        public CamSetting[] Cam = new CamSetting[4];
        
        public MesSetting MES = new MesSetting();
        public PrinterSetting Printer = new PrinterSetting();
        public ScannerSetting Scanner = new ScannerSetting();
       
        public PLCSetting[] PLC = new PLCSetting[2];
        public OptionSetting Option = new OptionSetting();
        public CamDisplayClass CamDisplay = new CamDisplayClass();

        public string CommonPath = "";
        public string pathfile = "";
        public string pathfile1 = "";
        public string PathfileSave = "";
        public string offsettaleble1 = "";
        public string offsettaleble2 = "";
        public string text_OKleft = "";
        public string text_NGleft = "";
        public string text_ALLleft = "";
        public string text_OKRight = "";
        public string text_NGRight = "";
        public string text_ALLRight = "";
        public string text_OKAll = "";
        public string text_NGAll = "";
        public string text_ALLCount = "";
    }
    
}
