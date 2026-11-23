using SLTtechSoft.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SLTtechSoft
{
    public partial class CameraVisionDisplay : UserControl
    {
        public bool IsShowParaCam = false;

        public int NumberPart { get; set; }
        public PartOfCam[] partOfCams = null;
        public CameraVisionDisplay()
        {
            InitializeComponent();
            IsShowParaCam = false;
            panelParameter.Visible = false;
            Dropdown.BackColor = IsShowParaCam ? Color.Silver : Color.White;
        }

        public void InitialObjectVision()
        {
            if (NumberPart == 0) NumberPart = 8;
            partOfCams = new PartOfCam[NumberPart];
            for (int i = 0; i < NumberPart; i++)
            {
                partOfCams[i] = new PartOfCam();
                partOfCams[i].Name = "PartOfCam" + i.ToString();
                flowLayoutPanelDisplay.Controls.Add(partOfCams[i]);
            }
        }

        private void Dropdown_Click(object sender, EventArgs e)
        {
            IsShowParaCam = !IsShowParaCam;
            panelParameter.Visible = IsShowParaCam;
            Dropdown.BackColor = IsShowParaCam ? Color.Silver : Color.White;
        }
        public void RefreshDisplay()
        {
            if (NumberPart == 0) return;
            int AllWidth = flowLayoutPanelDisplay.Width;
            int AllHeight = flowLayoutPanelDisplay.Height;
            int EachHeight = AllHeight / NumberPart;
            for (int i = 0;i < NumberPart; i++) 
            {
                partOfCams[i].Width = AllWidth;
                partOfCams[i].Height = EachHeight;
                partOfCams[i].Extent.Tag = i.ToString();
                partOfCams[i].Extent.Click -= PartCam_Extent;
                partOfCams[i].Extent.Click += PartCam_Extent;
                partOfCams[i].FitImage.Tag = i.ToString();
                partOfCams[i].FitImage.Click -= btnFitDisplay_Click;
                partOfCams[i].FitImage.Click += btnFitDisplay_Click;
                VisionMasterUiHelper.Fit(partOfCams[i].mDisplay);
            }
        }
        private void btnFitDisplay_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int thisTag = Convert.ToInt32(button.Tag);
            VisionMasterUiHelper.Fit(partOfCams[thisTag].mDisplay);
        }
        private void btnRefreshDisplay_Click(object sender, EventArgs e)
        {
            RefreshDisplay();
        }

        private void PartCam_Extent(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            int thisTag = Convert.ToInt32(button.Tag);
            ExtentPartCam(thisTag);
        }
        public void ExtentPartCam(int index)
        {
            if (NumberPart == 0) return;
            int AllWidth = flowLayoutPanelDisplay.Width;
            int AllHeight = flowLayoutPanelDisplay.Height;
            int EachHeight = AllHeight / NumberPart;
            for (int i = 0; i < NumberPart; i++)
            {
                if (i != index)
                {
                    partOfCams[i].Width = AllWidth;
                    partOfCams[i].Height = 27;
                }
                else
                {
                    partOfCams[i].Width = AllWidth;
                    partOfCams[i].Height = AllHeight - (NumberPart - 1) * 27;
                }
                VisionMasterUiHelper.Fit(partOfCams[i].mDisplay);

            }

        }
        private void ClearFilmStrip_Click(object sender, EventArgs e)
        {
            flplFilmStrip.Controls.Clear();
        }

       
    }

}
