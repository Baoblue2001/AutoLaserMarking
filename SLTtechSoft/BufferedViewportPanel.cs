using System.ComponentModel;
using System.Windows.Forms;

namespace SLTtechSoft
{
    // Keep the buffered viewport control separate from the form code-behind so
    // Visual Studio opens LaserSettingLayout as the form designer target.
    [ToolboxItem(false)]
    internal sealed class BufferedViewportPanel : Panel
    {
        public BufferedViewportPanel()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                    cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }
    }
}
