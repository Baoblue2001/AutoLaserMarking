using System.Diagnostics;

namespace SLTtechSoft.Class
{
    public class Vision
    {
    }

    public class VisionSystem
    {
        public Stopwatch StopWatchVision = new Stopwatch();
        public CamVisionClass.ICamera CAM;
        public bool IsTriggerStart = false;
        public bool TriggerOne = false;
        public int TriggerIndex = 0;
        public VisionMasterProcess[] VisionProcesses;
        public bool IsVisionProcessInitial = false;
        public string Code = "";
        public int ResultTriggerOne = 0;
        public bool ErrorVision = false;
        public int CamGrapTime;
        public string TimeVision = "";
        public bool FinishTrigger;
        public bool ErrorGrap;
        public System.Drawing.Bitmap LastInputImage;
    }

    public class Coordinate
    {
        public double X;
        public double Y;
    }

    public class PMAlignResult
    {
        public int Count { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

    // Generic result DTOs kept for source compatibility after removing the legacy vision types.
    public class ResultsLine
    {
        public PMAlignResult PMCoor { get; set; }
        public object Line { get; set; }
        public Coordinate MachineCoor { get; set; }
        public string Name { get; set; }
        public int ToolIndex { get; set; }
    }

    public class ResultsPoint
    {
        public PMAlignResult PMCoor { get; set; }
        public object Point { get; set; }
        public Coordinate MachineCoor { get; set; }
        public string Name { get; set; }
        public int ToolIndex { get; set; }
    }

    public class ResultsCircle
    {
        public PMAlignResult PMCoor { get; set; }
        public object Circle { get; set; }
        public Coordinate MachineCoor { get; set; }
        public string Name { get; set; }
        public int ToolIndex { get; set; }
    }
}
