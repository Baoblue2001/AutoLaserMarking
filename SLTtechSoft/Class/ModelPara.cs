using SLTtechSoft.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLTtechSoft
{
    public class CamPara
    {
        public DataTable VisionTable = new DataTable();
        public double ResolutionImage = 0.1;
        public double ResolutionView = 0.1;
        public string LaserImageFile = "";
        public string LaserTopImageFile = "";
        public string LaserBotImageFile = "";
        public LaserSurfaceParameter Top = new LaserSurfaceParameter();
        public LaserSurfaceParameter Bot = new LaserSurfaceParameter();
    }

    public class LaserSurfaceParameter
    {
        public List<LaserMarkingParameter> Markings = new List<LaserMarkingParameter>();
        public List<LaserFiducialParameter> Fiducials = new List<LaserFiducialParameter>();
        public double ResolutionImage = 1.0;
        public double ImagePixelToMM = 1.0;
        public double ResolutionImageRobot = 1.0;
        public bool EnableServoAxisTransform = false;
        public bool SwapServoAxes = false;
        public bool ReverseServoX = false;
        public bool ReverseServoY = false;

        // Physical coordinate represented by the top-left pixel of the current stitched image.
        // Pixel coordinates are converted using ImagePixelToMM only.
        public bool HasStitchCoordinateSystem = false;
        public double StitchOriginX;
        public double StitchOriginY;
        public double StitchPixelToMM;

        // Each scanned image has its own servo position; image paths are relative to the model folder.
        public List<LaserScanImageParameter> ScanImages = new List<LaserScanImageParameter>();
    }

    public class LaserScanImageParameter
    {
        // Stable identity keeps Marking/Fiducial references attached to the source image,
        // even when the preview contact sheet is rebuilt or its tiles are reordered.
        public string Id = Guid.NewGuid().ToString("N");
        public string ImagePath = "";
        public double? ServoX;
        public double? ServoY;
        public DateTime CaptureTime;
    }

    public class LaserMarkingParameter
    {
        public int Id;
        public string ImageId = "";
        public string Product = "0";
        public string Type = "QR";
        public string PrintType = "QR";
        public string Content = "";
        public string Rotate = "";
        // For ImageId-backed items, PositionX/Y are source-image pixels relative to that image.
        // Empty ImageId preserves the legacy stitched-image pixel coordinate format.
        public double PositionX;
        public double PositionY;
    }

    public class LaserFiducialParameter
    {
        public int Id;
        public string ImageId = "";
        // For ImageId-backed items, PositionX/Y are source-image pixels relative to that image.
        // Empty ImageId preserves the legacy stitched-image pixel coordinate format.
        public double PositionX;
        public double PositionY;
    }
    public class ModelParameter
    {
        public string ModelName = "";
        public CamPara[] Cam = new CamPara[4];
        public string RowTray = "";
        public string ColTray = "";
        public PLCModelPara pLCModelPara = new PLCModelPara();
        public bool model2Array = false;
    }
    public class CurrentModelInfomation
    {
        public string PathFileModel = "";
        public string MachineNo = "0";
        public int QualityCount = 0;
        public DataTable ModelInfo = new DataTable();
        public int ModelNo = 0;
        public PLCInternalPara pLCInternalPara = new PLCInternalPara();
    }
    public class PLCInternalPara
    {
       

    }
    public class PLCModelPara
    {
        public int SpeedStep {  get; set; }
        public int nudStepSize { get; set; }
        public int tbOPRCreepSpeedX { get; set; }
        public int tbOprSpeedX { get; set; }
        public int NudACC_Conv1 { get; set; }
        public int NudDEC_Conv1 { get; set; }
        public int NudACC_Conv2 { get; set; }
        public int NudDEC_Conv2 { get; set; }
        public int nudACC_X { get; set; }
        public int nudDEC_X { get; set; }
        public int nudPoint1 { get; set; }
        public int nudPoint2 { get; set; }
        public int nudPoint3 { get; set; }
        public int nudPoint4 { get; set; }
        public int nudPoint5 { get; set; }
        public int nudPoint6 { get; set; }
        public int nudSpeed { get; set; }
        public int nudSpeedJog { get; set; }
        public int SpeedConv1 { get; set; }
        public int SpeedConv2 { get; set; }

    }
   
}
