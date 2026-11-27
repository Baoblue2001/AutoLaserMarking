using SLTSoft.RJControl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static SLTtechSoft.Form1;

namespace SLTtechSoft
{
    public partial class Main : Form
    {
        public bool IsUiInitialized { get; private set; } = false;

        public Main()
        {
            InitializeComponent();
          
        }
        public void InitializeUI(Form1 obj)
        {
            _form1 = obj;
            IsShowMainLog = true;
            IsShowVisionLog = true;
            UpdateShowMainLog();
            UpdateShowVisionLog();
            InitialCameraVisionDisplay();
            _form1.formModel.ShowAModelParameter(_form1.formModel.modelParameter);
            txtNGALl.ReadOnly = true;
            txtOkAll.ReadOnly = true;
            txtTotalALL.ReadOnly = true;
            txtNGALl.Text = "0";
            txtOkAll.Text = "0";
            txtTotalALL.Text= "0";
            IsUiInitialized = true;
            _form1.BindAllVisionMasterDisplays();
            UpdateControlStatus();
        }
        private void Main_Shown(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.WindowState = FormWindowState.Maximized;
        }
        private void UpdateShowMainLog()
        {
            panelMainLog.Visible = IsShowMainLog;
            panelMainLogAll.Height = IsShowMainLog ? HeightOfLogView : 30;
            btnDropDownMainLog.BackColor = IsShowMainLog ? System.Drawing.Color.Orange : SystemColors.Control;
        }
        private void UpdateShowVisionLog()
        {
            panelVisionLog.Visible = IsShowVisionLog;
            panelVisonLogAll.Height = IsShowVisionLog ? HeightOfLogView : 30;
            btnDropDownVisionLog.BackColor = IsShowVisionLog ? System.Drawing.Color.Orange : SystemColors.Control;
        }
        public void InitialCameraVisionDisplay()
        {
            ReleaseVisionMasterBindings();
            tableLayoutPanelVisionAll.Controls.Clear();
            ResetCameraVisionDisplays();
            int NumberCamDisplay = 0;

            if (_form1.formOption == null) return;
            if (_form1.formOption.Parameters == null) return;

            for (int i = 0; i < _form1.NumberOfCamera; i++)
            {
                if (_form1.formOption.Parameters.Cam[i] != null)
                {
                    if (_form1.formOption.Parameters.Cam[i].Use)
                    {
                        int NumberOfPart = _form1.formModel.ListFrmVisionFileSetup[i].GetRowCount();
                        NumberCamDisplay += NumberOfPart;
                    }
                }
            }

            int numberColumn = _form1.formOption.Parameters.CamDisplay.Column;
            int numberRow = _form1.formOption.Parameters.CamDisplay.Row;
            int numberCell = numberColumn * numberRow;

            if (numberCell < NumberCamDisplay)
            {
                numberColumn = NumberCamDisplay;
                numberRow = 1;
            }

            tableLayoutPanelVisionAll.RowStyles.Clear();
            tableLayoutPanelVisionAll.ColumnStyles.Clear();
            tableLayoutPanelVisionAll.ColumnCount = numberColumn;
            tableLayoutPanelVisionAll.RowCount = numberRow;
            tableLayoutPanelVisionAll.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            //làm các hàng cột có kích thước bằng nhau
            for (int i = 0; i < numberRow; i++) tableLayoutPanelVisionAll.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / numberRow));
            for (int i = 0; i < numberColumn; i++) tableLayoutPanelVisionAll.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / numberColumn));

            int NoTableColumnToAdd = 0;
            int NoTableRowToAdd = 0;
            CameraVisionDisplays = new ListCamDisplayClass[_form1.NumberOfCamera];
            for (int i = 0; i < _form1.NumberOfCamera; i++)
            {
                int NumberOfPart = _form1.formModel.ListFrmVisionFileSetup[i].GetRowCount();
                CameraVisionDisplays[i] = new ListCamDisplayClass();
                for (int y = 0; y < NumberOfPart; y++)
                {
                    CameraVisionDisplay _CameraVisionDisplays = new CameraVisionDisplay();
                    if (_form1.formOption.Parameters.Cam[i] != null)
                        if (_form1.formOption.Parameters.Cam[i].Use)
                        {
                            _CameraVisionDisplays.NumberPart = 1;
                            _CameraVisionDisplays.InitialObjectVision();
                            _CameraVisionDisplays.partOfCams[0].label.Text = _form1.formModel.ListFrmVisionFileSetup[i].GetName(y);

                            _CameraVisionDisplays.Margin = new Padding(5);
                            _CameraVisionDisplays.Dock = DockStyle.Fill;

                            _CameraVisionDisplays.TriggerOne.Tag = $"{i},{y}";
                            _CameraVisionDisplays.TriggerOne.Click += TriggerCam1;

                            _CameraVisionDisplays.LiveCam.Tag = $"{i},{y}";
                            _CameraVisionDisplays.LiveCam.Click += LiveCam1;

                            _CameraVisionDisplays.RunContinously.Tag = $"{i},{y}";
                            _CameraVisionDisplays.RunContinously.Click += RunContinousCam1;

                            _CameraVisionDisplays.Exposure.Tag = $"{i},{y}";
                            _CameraVisionDisplays.Exposure.Click += numbericEdit1_Click_1;
                            _CameraVisionDisplays.Exposure.TextChanged += Exposure_TextChanged;
                       
                            _CameraVisionDisplays.btnSaveSetting.Click += SaveCamParams;
                            _CameraVisionDisplays.btnSaveSetting.Tag = $"{i},{y}";
                            _CameraVisionDisplays.panelFilmStrip.Size = new Size(0, 0);
                            _CameraVisionDisplays.panelFilmStrip.Visible = false;

                            _CameraVisionDisplays.label1.Text = $"Cam{i + 1}";
                            _CameraVisionDisplays.label1.Tag = $"{i},{y}";
                            _CameraVisionDisplays.label1.DoubleClick += CamVissionDisplayLabel1_DoubleClick;
                          
                            tableLayoutPanelVisionAll.Controls.Add(_CameraVisionDisplays, NoTableColumnToAdd, NoTableRowToAdd);
                            CameraVisionDisplays[i].List.Add(_CameraVisionDisplays);
                            _CameraVisionDisplays.RefreshDisplay();

                          NoTableColumnToAdd++;
                            if (NoTableColumnToAdd >= numberColumn)
                            {
                                NoTableColumnToAdd = 0;
                                NoTableRowToAdd++;
                            }
                        }
                }
               
            }
        }
        public void ReleaseVisionMasterBindings()
        {
            if (CameraVisionDisplays == null) return;
            foreach (ListCamDisplayClass camList in CameraVisionDisplays)
            {
                if (camList == null) continue;
                foreach (CameraVisionDisplay display in camList.List)
                {
                    if (display?.partOfCams == null) continue;
                    foreach (PartOfCam part in display.partOfCams)
                    {
                        if (part?.mDisplay == null) continue;
                        try { part.mDisplay.ModuleSource = null; } catch { }
                    }
                }
            }
        }

        private void ResetCameraVisionDisplays()
        {
            if (CameraVisionDisplays != null)
            {
                foreach (var camList in CameraVisionDisplays)
                {
                    if (camList == null) continue;

                    foreach (var display in camList.List)
                    {
                        if (display == null) continue;
                        if (display.Parent != null) display.Parent.Controls.Remove(display);
                        display.Dispose();
                    }

                    camList.List.Clear();
                }
            }

            CameraVisionDisplays = new ListCamDisplayClass[_form1.NumberOfCamera];

            for (int i = 0; i < CameraVisionDisplays.Length; i++) CameraVisionDisplays[i] = new ListCamDisplayClass();
        }
        private (int Cam, int index) GetTagData(string Tag)
        {
            string[] parts = Tag.Split(',');
            if (parts.Length != 2) return (-1, -1);
            if (!int.TryParse(parts[0], out int cam)) return (-1, -1);
            if (!int.TryParse(parts[1], out int index)) return (-1, -1);
            return (cam, index);
        }
        private void CamVissionDisplayLabel1_DoubleClick(object sender, EventArgs e)
        {
            Label label1 = sender as Label;
            var Tag = GetTagData((string)label1.Tag);
           
            if (!_visionIsFull)
            {
                ShowVisionFull(CameraVisionDisplays[Tag.Cam].List[Tag.index]);
                CameraVisionDisplays[Tag.Cam].List[Tag.index].RefreshDisplay();
            }
            else
            {
                RestoreVisionLayout();
                for (int i = 0; i < _form1.NumberOfCamera; i++)
                {
                    if (_form1.formOption.Parameters.Cam[i] != null)
                    {
                        if (_form1.formOption.Parameters.Cam[i].Use)
                        {
                            for (int y = 0; y < CameraVisionDisplays[Tag.Cam].List.Count; y++)
                            {
                                if (CameraVisionDisplays[i].List[y] != null)
                                    CameraVisionDisplays[i].List[y].RefreshDisplay();
                            }
                        }
                    } 
                }
            }
        }
        private void ShowVisionFull(Control selected)
        {
            if (_visionIsFull || selected == null) return;

            _visionPositions.Clear();
            _visionVisible.Clear();

            foreach (Control control in tableLayoutPanelVisionAll.Controls)
            {
                _visionPositions[control] = tableLayoutPanelVisionAll.GetCellPosition(control);
                _visionVisible[control] = control.Visible;
            }

            tableLayoutPanelVisionAll.SuspendLayout();

            foreach (Control control in tableLayoutPanelVisionAll.Controls) control.Visible = control == selected;

            tableLayoutPanelVisionAll.SetCellPosition(selected, new TableLayoutPanelCellPosition(0, 0));
            tableLayoutPanelVisionAll.SetColumnSpan(selected, tableLayoutPanelVisionAll.ColumnCount);
            tableLayoutPanelVisionAll.SetRowSpan(selected, tableLayoutPanelVisionAll.RowCount);
            selected.Dock = DockStyle.Fill;
            selected.Visible = true;
            selected.BringToFront();

            tableLayoutPanelVisionAll.ResumeLayout(true);
            _visionIsFull = true;
        }
        private void RestoreVisionLayout()
        {
            if (!_visionIsFull) return;

            tableLayoutPanelVisionAll.SuspendLayout();

            foreach (Control control in tableLayoutPanelVisionAll.Controls)
            {
                tableLayoutPanelVisionAll.SetColumnSpan(control, 1);
                tableLayoutPanelVisionAll.SetRowSpan(control, 1);

                if (_visionPositions.TryGetValue(control, out TableLayoutPanelCellPosition pos))
                    tableLayoutPanelVisionAll.SetCellPosition(control, pos);

                control.Dock = DockStyle.Fill;
                control.Visible = _visionVisible.TryGetValue(control, out bool visible) ? visible : true;
            }

            tableLayoutPanelVisionAll.ResumeLayout(true);
            tableLayoutPanelVisionAll.PerformLayout();

            _visionIsFull = false;
        }
        private void Exposure_TextChanged(object sender, EventArgs e)
        {
            TextBox button = sender as TextBox;
            if (button == null) return;
            var Tag = GetTagData((string)button.Tag);

            if (_form1._VisionSystem == null) return;
            if (_form1._VisionSystem[Tag.Cam] == null) return;
            if (_form1._VisionSystem[Tag.Cam].CAM == null) return;
            if (!_form1._VisionSystem[Tag.Cam].CAM.IsOpen) return;
            try
            {
                int exposure = Convert.ToInt32(CameraVisionDisplays[Tag.Cam].List[Tag.index].Exposure.Text);
                _form1._VisionSystem[Tag.Cam].CAM.ExposureTime = exposure;
                if (_form1.formModel.ListFrmVisionFileSetup[Tag.Cam] != null)
                    _form1.formModel.ListFrmVisionFileSetup[Tag.Cam].SetExposure(Tag.index, exposure);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void SaveCamParams(object sender, EventArgs e)
        {
            RJButton btnSaveCam = sender as RJButton;
            if (btnSaveCam?.Tag == null) return;
            var tag = GetTagData(btnSaveCam.Tag.ToString());
            if (tag.Cam < 0 || tag.index < 0) return;

            if (int.TryParse(CameraVisionDisplays[tag.Cam].List[tag.index].Exposure.Text, out int exposure))
                _form1.formModel.ListFrmVisionFileSetup[tag.Cam]?.SetExposure(tag.index, exposure);

            _form1.formModel.SaveAModel();
        }
        private void TriggerCam1(object sender, EventArgs e)
        {
            Button button = sender as Button;
            var Tag = GetTagData((string)button.Tag);
            int cam = Tag.Cam ;
            int index = Tag.index;
            if (_form1.IsCamRunContinous[cam]) return;
            if (_form1.IsLiveCam[cam]) return;
            if (_form1.MachineMode == ModeMachine.Auto) return;

            _form1._VisionSystem[cam].TriggerIndex = index;
            if (_form1.formOption.Parameters.Cam[cam].ImageSource == ImageSource.Cam)
            {
                _form1._VisionSystem[cam].TriggerOne = true;
            }
            else
            {
                _form1.TriggerCamManual(cam, index, true, true);
            }
        }
        private void LiveCam1(object sender, EventArgs e)
        {
            Button button = sender as Button;
            var Tag = GetTagData((string)button.Tag);
            int cam = Tag.Cam;
            int index = Tag.index;

            _form1._VisionSystem[cam].TriggerIndex = index;
            _form1.IsLiveCam[cam] = !_form1.IsLiveCam[cam];
            if (_form1.IsLiveCam[cam])
            {
                _form1.LiveCamera(cam);
            }
            else
            {
                _form1.StopLiveCamera(cam);
            }
            if (!_form1.IsLiveCam[index])
            {
                _form1.PLC.Write.AutoBits[PlcDic.LightOn[index]] = false;
            }
            UpdateControlStatus();
        }
        private void RunContinousCam1(object sender, EventArgs e)
        {
            if (_form1.MachineMode == ModeMachine.Auto) return;
            if (_form1.MachineMode == ModeMachine.Teaching) return;
            Button button = sender as Button;
            var Tag = GetTagData((string)button.Tag);
            int cam = Tag.Cam;
            int index = Tag.index;

            if (_form1.IsLiveCam[cam]) return;
            
            
            _form1.IsCamRunContinous[cam] = !_form1.IsCamRunContinous[cam];
            if (_form1.IsCamRunContinous[cam])
            {
                _form1._VisionSystem[cam].TriggerIndex = index;
            }

            ////Update Controls
            UpdateControlStatus();
        }
        private void btnClearMainLog_Click(object sender, EventArgs e)
        {
            listViewMainLog.Items.Clear();
        }
        private void btnClearMesLog_Click(object sender, EventArgs e)
        {
            listViewMesLog.Items.Clear();
        }
        private void btnClearQuality_Click(object sender, EventArgs e)
        {
            
        }
        private void btnDropDownMainLog_Click(object sender, EventArgs e)
        {
            IsShowMainLog = !IsShowMainLog;
            UpdateShowMainLog();
        }
        private void btnDropDownVisionLog_Click(object sender, EventArgs e)
        {
            IsShowVisionLog = !IsShowVisionLog;
            UpdateShowVisionLog();
        }
        private void btnAutoMode_Click(object sender, EventArgs e)
        {
            SwitchModeAuto(ModeMachine.Auto);

            _form1.PLC.Write.AutoBits[PlcDic.Auto] = true;
            _form1.PLC.Write.AutoBits[PlcDic.Stop] = false;
         
        }
        private void btnStopAuto_Click(object sender, EventArgs e)
        {
            SwitchModeAuto(ModeMachine.Manual);
           
            _form1.PLC.Write.AutoBits[PlcDic.Auto] = false;
            _form1.PLC.Write.AutoBits[PlcDic.Stop] = true;
            _form1.PLC.Write.AutoBits[PlcDic.LightOn[0]] = false;
            _form1.PLC.Write.AutoBits[PlcDic.LightOn[1]] = false;
            _form1.PLC.Write.AutoBits[PlcDic.LightOn[2]] = false;

        }
        private void btnTeaching_Click(object sender, EventArgs e)
        {
            SwitchModeAuto(ModeMachine.Teaching);
        }
        public void SwitchModeAuto(ModeMachine Mode)
        {
            if (_form1 == null) return;
            _form1.MachineMode = Mode;
            for (int i = 0; i < _form1.NumberOfCamera; i++)
            {
                _form1.IsCamRunContinous[i] = false;
                _form1.IsLiveCam[i] = false;
               
            }
            _form1.PLC.Write.AutoBits[PlcDic.LightOn[0]] = false;
            _form1.PLC.Write.AutoBits[PlcDic.LightOn[1]] = false;
            _form1.PLC.Write.AutoBits[PlcDic.LightOn[2] ]= false;

            Invoke(new Action(() => { UpdateControlStatus(); }));
        }
        public void UpdateControlStatus()
        {
            // During Form1.InitialUI(), Model.InitializeUI() is called before Main.InitializeUI().
            // Therefore formMain already exists but _form1/CameraVisionDisplays are not ready yet.
            // Do not touch MAIN controls until this form has completed InitializeUI().
            if (!IsUiInitialized || _form1 == null || IsDisposed || Disposing) return;

            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(UpdateControlStatus)); } catch { }
                return;
            }

            int cameraCount = Math.Min(_form1.NumberOfCamera, CameraVisionDisplays?.Length ?? 0);
            for (int i = 0; i < cameraCount; i++)
            {
                ListCamDisplayClass camDisplays = CameraVisionDisplays[i];
                if (camDisplays?.List == null) continue;

                bool isContinuous = _form1.IsCamRunContinous != null && i < _form1.IsCamRunContinous.Length && _form1.IsCamRunContinous[i];
                bool isLive = _form1.IsLiveCam != null && i < _form1.IsLiveCam.Length && _form1.IsLiveCam[i];

                for (int y = 0; y < camDisplays.List.Count; y++)
                {
                    CameraVisionDisplay display = camDisplays.List[y];
                    if (display == null) continue;
                    if (display.RunContinously != null) display.RunContinously.BackColor = isContinuous ? System.Drawing.Color.Orange : System.Drawing.Color.White;
                    if (display.LiveCam != null) display.LiveCam.BackColor = isLive ? System.Drawing.Color.Orange : System.Drawing.Color.White;
                }
            }

            if (btnAutoMode != null) btnAutoMode.BackColor = _form1.MachineMode == ModeMachine.Auto ? System.Drawing.Color.Lime : System.Drawing.Color.White;
            if (btnStopAuto != null) btnStopAuto.BackColor = _form1.MachineMode == ModeMachine.Manual ? System.Drawing.Color.Red : System.Drawing.Color.White;
            if (btnTeaching != null) btnTeaching.BackColor = _form1.MachineMode == ModeMachine.Teaching ? System.Drawing.Color.Orange : System.Drawing.Color.White;
        }
        public void RefresViewButtonIndex(int index)
        {
            for (int i = 0; i < ButtonsIndexCheck.Count; i++)
            {
                if (index != i)
                {
                    ButtonsIndexCheck[i].BorderColor = SystemColors.Control;
                }
                else
                {
                    ButtonsIndexCheck[i].BorderColor = System.Drawing.Color.Orange;
                }
            }
            _form1.formModel.IndexImageChoosing = index;
        }
        private void numbericEdit1_Click_1(object sender, EventArgs e)
        {
            NumbericEdit ThisnumbericEdit = sender as NumbericEdit;

            NumPad numPad = new NumPad();
            numPad.InitialUI(ThisnumbericEdit, _form1.formModel, _form1);
            numPad.Show();
        }
        private void btnResetCounter_Click(object sender, EventArgs e)
        {
         
        }
        private void btnExtendDisplay_Click(object sender, EventArgs e)
        {
          
        }
        Form1 _form1;
        //public CameraVisionDisplay[] CameraVisionDisplays = new CameraVisionDisplay[4];
        public ListCamDisplayClass[] CameraVisionDisplays = new ListCamDisplayClass[4];
        public class ListCamDisplayClass
        {
            public List<CameraVisionDisplay> List = new List<CameraVisionDisplay>();

        }
        public List<RJButton> ButtonsIndexCheck = new List<RJButton>();
        public int RightOfDecimal = 3;
        public double DecimalPara = 1000;
        public string scannedValue = "";
        bool IsShowMainLog = true;
        bool IsShowVisionLog = true;
        private bool isDisplayExtend = false;
        private int HeightOfLogView = 250;
        private Dictionary<Control, TableLayoutPanelCellPosition> _visionPositions = new Dictionary<Control, TableLayoutPanelCellPosition>();
        private Dictionary<Control, bool> _visionVisible = new Dictionary<Control, bool>();
        private bool _visionIsFull = false;

        private void txtLotNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _form1.CurrentLotNo = txtLotNo.Text;
                GetDataInfomationSerialno();
            }
        }
        public bool GetDataInfomationSerialno()
        {

            if (_form1.formOption.Parameters.MES.MesEnabel)
            {
                _form1.MesDataInfomation = _form1.MES.DataInformationRequest(txtLotNo.Text);

                if (_form1.MES.ExceptionCode != "")
                {
                    //LOG
                    this.Invoke(new Action(() => {
                        _form1.WriteLogPC(LogType.Mes, "MES", _form1.MES.ExceptionCode);
                    }));
                    txtMesResult.Text = "MES online. Get data infomation Error.";
                    txtMesResult.BackColor = Color.OrangeRed;
                    return false;
                }

                txtYMMDDN.Text = _form1.MesDataInfomation.YMMDDN;
                txtSerialNo.Text = _form1.MesDataInfomation.SerialNo;
                txt1STbarcode.Text = _form1.MesDataInfomation.Code;
                dgvRank.Rows.Clear();
                if (_form1.MesDataInfomation.Rank.Length != 0)
                {
                    for (int i = 0; i < _form1.MesDataInfomation.Rank.Length; i++)
                    {
                        dgvRank.Rows.Add(_form1.MesDataInfomation.Rank[i]);
                    }

                    txtRank.Text = _form1.MesDataInfomation.Rank[0];
                    if (_form1.MesDataInfomation.Rank.Length > 1)
                    {
                        txtRank2.Text = _form1.MesDataInfomation.Rank[1];
                    }
                }

                txtArrayCount.Text = _form1.MesDataInfomation.Array.ToString();
                txtMesResult.Text = "MES Online. Get data infomation OK.";
                txtMesResult.BackColor = Color.LawnGreen;
                return true;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtSerialNo.Text))
                {
                    txtSerialNo.Text = "000001";
                }
                string thisyear = DateTime.Now.ToString("yyyy").Substring(3, 1);
                string thisMonth = DateTime.Now.ToString("MM");
                string thisDay = DateTime.Now.ToString("dd");
                txtYMMDDN.Text = thisyear + thisMonth + thisDay;
                txt1STbarcode.Text = txtYMMDDN.Text + txtSerialNo.Text;

                dgvRank.Rows.Clear();
                //txbRank1.Text = "S2R2D2";
                //txbRank2.Text = "S2R2D3";
                dgvRank.Rows.Add(txtRank.Text);
                dgvRank.Rows.Add(txtRank2.Text);
                txtMesResult.Text = "MES offline. Get data infomation OK.";
                txtMesResult.BackColor = Color.LawnGreen;
                return true;
            }

        }

    }
}
