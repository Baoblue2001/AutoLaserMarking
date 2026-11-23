using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SLTtechSoft
{
    public partial class FormOption : Form
    {
        public string PathFileParameter = "D:\\InitialData\\Laser Marking\\Parameter.txt";
        // Khai bao form chinh
        Form1 _form1;
        //Khai bao bien
        public Parameter Parameters { get; set; }
        private CameraSetting[] cameraSettings = new CameraSetting[4];
        private PLCConfigForm[] PLCSettings = new PLCConfigForm[2];
        public string[][] FolderImageFiles = new string[4][];
        public int[] IndexImageFolder = new int[4];
        public string[] FileImagePaths = new string[4];
        public FormOption()
        {
            InitializeComponent();
        }
        public void InitializeUI(Form1 obj)
        {
            _form1 = obj;
            string[] myPort = System.IO.Ports.SerialPort.GetPortNames();
            cbbPrinterPort.Items.AddRange(myPort);
            cbbScannerPort.Items.AddRange(myPort);
            tbCamDisplayColumn.KeyDown += txtNumber_TextChanged;
            tbCamDisplayColumn.Leave += txtNumber_Leave;
            tbCamDisplayRow.KeyDown += txtNumber_TextChanged;
            tbCamDisplayRow.Leave += txtNumber_Leave;

            InitialListControl();
            ReadParameter();

        }
        private void InitialListControl()
        {
            cameraSettings = new CameraSetting[4];
            FolderImageFiles = new string[4][];
            IndexImageFolder = new int[4];
            FileImagePaths = new string[4];

            for (int i = 0; i < 4; i++)
            {
                cameraSettings[i] = new CameraSetting();
                cameraSettings[i].Dock = DockStyle.Fill;
                cameraSettings[i].FolderDirector.TextChanged += FolderDirector_TextChanged;
                cameraSettings[i].FolderDirector.Tag = i.ToString();
                cameraSettings[i].FileDirector.TextChanged += FileDirector_TextChanged;
                cameraSettings[i].FileDirector.Tag = i.ToString();
                cameraSettings[i].RefreshFolderImage.Click += RefreshFolderImage_Click; 
                cameraSettings[i].RefreshFolderImage.Tag = i.ToString();
                cameraSettings[i].RefreshFileImage.Click += RefreshFileImage_Click;
                cameraSettings[i].RefreshFileImage.Tag = i.ToString();

                cameraSettings[i].FolderDirector.DoubleClick += FolderDirector_DoubleClick; 
                cameraSettings[i].FileDirector.DoubleClick += FileDirector_DoubleClick;
                cameraSettings[i].btnConnectCam.Click += BtnConnectCam_Click;
                cameraSettings[i].btnConnectCam.Tag = i.ToString();
            }
            tabPageCam1.Controls.Add(cameraSettings[0]);
            tabPageCam2.Controls.Add(cameraSettings[1]);
            tabPageCam3.Controls.Add(cameraSettings[2]);
            tabPageCam4.Controls.Add(cameraSettings[3]);

            for (int i = 0; i < 2; i++)
            {
                PLCSettings[i] = new PLCConfigForm();
                PLCSettings[i].Tag = i.ToString();
                PLCSettings[i].Dock = DockStyle.Fill;
                PLCSettings[i].btnRefresh.Click += PLCRefresh_Click;
                PLCSettings[i].btnRefresh.Tag += i.ToString();
                PLCSettings[i].btnPLCConnect.Click += PLCConnect_Click;
                PLCSettings[i].btnPLCConnect.Tag += i.ToString();
                PLCSettings[i].btnPLCDisconnect.Click += PLCDisconnect_Click;
                PLCSettings[i].btnPLCDisconnect.Tag += i.ToString();
            }
            tabPagePLC1.Controls.Add(PLCSettings[0]);
            tabPagePLC2.Controls.Add(PLCSettings[1]);
        }
       
        private void PLCRefresh_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            int index = Convert.ToInt16(button.Tag);
            string[] myPort = System.IO.Ports.SerialPort.GetPortNames();
            PLCSettings[index].PortName.Items.Clear();
            PLCSettings[index].PortName.Items.AddRange(myPort);
        }
        private void PLCConnect_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            int index = Convert.ToInt16(button.Tag);
            _form1.ConnectPLC(index);
        }
        private void PLCDisconnect_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            int index = Convert.ToInt16(button.Tag);
            _form1.DisconnectPLC(index);
        }
        private void BtnConnectCam_Click(object sender, EventArgs e)
        {
            Button textBoxCam = sender as Button;
            int cam = Convert.ToInt16(textBoxCam.Tag);
            _form1.InitializeCamera(cam);
            if (_form1._VisionSystem[cam].CAM.IsOpen)
            {
                MessageBox.Show($"Cam {cam} Connected!");
            }
            else
            {
                MessageBox.Show($"Cam {cam} can not connect!");
            }
        }

        private void FileDirector_DoubleClick(object sender, EventArgs e)
        {
            TextBox textBoxCam = sender as TextBox;
            int cam = Convert.ToInt16(textBoxCam.Tag);
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxCam.Text = dlg.FileName;
            }
        }

        private void FolderDirector_DoubleClick(object sender, EventArgs e)
        {
            TextBox textBoxCam = sender as TextBox;
            int cam = Convert.ToInt16(textBoxCam.Tag);
            System.Windows.Forms.FolderBrowserDialog fldDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (fldDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxCam.Text = fldDialog.SelectedPath;
            }
        }

        private void RefreshFileImage_Click(object sender, EventArgs e)
        {
            Button textBoxCam = sender as Button;
            int cam = Convert.ToInt16(textBoxCam.Tag);
            if (string.IsNullOrWhiteSpace(cameraSettings[cam].FileDirector.Text)) return;
            FileImagePaths[cam] = cameraSettings[cam].FileDirector.Text;

        }

        private void RefreshFolderImage_Click(object sender, EventArgs e)
        {
            Button textBoxCam = sender as Button;
            int cam = Convert.ToInt16(textBoxCam.Tag);
            IndexImageFolder[cam] = 0;
            cameraSettings[cam].FolderDirector.Text = "";
        }

        private void FileDirector_TextChanged(object sender, EventArgs e)
        {
            TextBox textBoxCam = sender as TextBox;
            int cam = Convert.ToInt16(textBoxCam.Tag);
            if (string.IsNullOrWhiteSpace(textBoxCam.Text)) { FileImagePaths[cam] = null; return; }
            FileImagePaths[cam] = textBoxCam.Text;
        }

        private void FolderDirector_TextChanged(object sender, EventArgs e)
        {
            TextBox textBoxCam = sender as TextBox;
            int cam = Convert.ToInt16(textBoxCam.Tag);
            if (textBoxCam.Text == "" || textBoxCam.Text == null) return;
            FolderImageFiles[cam] = Directory.GetFiles(textBoxCam.Text);
            IndexImageFolder[cam] = 0;
        }
        public void ReadParameter()
        {
            try
            {
                if (File.Exists(PathFileParameter))
                {
                    string ReadText = File.ReadAllText(PathFileParameter);
                    Parameters = JsonConvert.DeserializeObject<Parameter>(ReadText);
                    ShowParameterInterface(Parameters);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ReadParameter");
            }
        }
        public void ShowParameterInterface(Parameter parameter)
        {
            try
            {
                txbMCTittle.Text = parameter.Machine.MCTitle;
                txbPassWord.Text = parameter.Machine.Password;

                txbMES_HostIP.Text = parameter.MES.HostIP;
                txbMES_Port.Text = parameter.MES.Port.ToString();
                txbMES_MCCode.Text = parameter.MES.McCode;
                txbMES_CheckCount.Text = parameter.MES.CheckCount;
                cbMesEnable.Checked = parameter.MES.MesEnabel;

                cbbPrinterPort.Text = parameter.Printer.PortName;
                cbbPrinterBaudrate.Text = parameter.Printer.BaudRate.ToString();
                cbbPrinterDataBits.Text = parameter.Printer.DataBits.ToString();
                cbbPrinterParity.Text = parameter.Printer.Parity;
                cbbPrinterStopbits.Text = parameter.Printer.StopBits;

                txbScanner_HostIP.Text = parameter.Scanner.HostIP;
                txbScanner_Port.Text = parameter.Scanner.Port.ToString();

                cbbScannerPort.Text = parameter.Scanner.PortName;
                cbbScannerBaudrate.Text = parameter.Scanner.BaudRate.ToString();
                cbbScannerDataBits.Text = parameter.Scanner.DataBits.ToString();
                cbbScannerParity.Text = parameter.Scanner.Parity;
                cbbScannerStopbits.Text = parameter.Scanner.StopBits;

                for (int i = 0; i < 2; i++)
                {
                    PLCSettings[i].Use.Checked = parameter.PLC[i].Use;
                    PLCSettings[i].tbName.Text = parameter.PLC[i].Name;
                    PLCSettings[i].HostIP.Text = parameter.PLC[i].HostIP;
                    PLCSettings[i].Port.Text = parameter.PLC[i].Port.ToString();

                    switch (parameter.PLC[i].Protocol)
                    {
                        case Class.Protocol.Modbus:
                            {
                                PLCSettings[i].rdModbus.Checked = true;
                                break;
                            }
                        case Class.Protocol.XGT:
                            {
                                PLCSettings[i].rdXGT.Checked = true;
                                break;
                            }
                        case Class.Protocol.SLMP:
                            {
                                PLCSettings[i].rdSLMP.Checked = true;
                                break;
                            }
                        default:
                            {
                                PLCSettings[i].rdModbus.Checked = true;
                                break;
                            }
                    }
                    switch (parameter.PLC[i].Mode)
                    {
                        case FluentModbusPLC.ModbusMode.Tcp:
                            {
                                PLCSettings[i].rdTCP.Checked = true;
                                break;
                            }
                        case FluentModbusPLC.ModbusMode.Rtu:
                            {
                                PLCSettings[i].rdRTU.Checked = true;
                                break;
                            }
                      
                        default:
                            {
                                PLCSettings[i].rdModbus.Checked = true;
                                break;
                            }
                    }


                    PLCSettings[i].PortName.Text = parameter.PLC[i].PortName;
                    PLCSettings[i].Baudrate.Text = parameter.PLC[i].BaudRate.ToString();
                    PLCSettings[i].Databits.Text = parameter.PLC[i].DataBits.ToString();
                    PLCSettings[i].Parity.Text = parameter.PLC[i].Parity;
                    PLCSettings[i].Stopbits.Text = parameter.PLC[i].StopBits;
                }

                cbCallModelManual.Checked = parameter.Option.CallModelManual;
              
                txbCommonPath.Text = parameter.CommonPath;
                txtFilesave.Text = parameter.PathfileSave;
                txtoffset1.Text = parameter.offsettaleble1;
                txtoffset2.Text = parameter.offsettaleble2;
                _form1.formMain.txtNGALl.Text = parameter.text_NGAll;
                _form1.formMain.txtOkAll.Text = parameter.text_OKAll;
                _form1.formMain.txtTotalALL.Text = parameter.text_ALLCount;

                tbCamDisplayColumn.Text = parameter.CamDisplay.Column.ToString();
                tbCamDisplayRow.Text = parameter.CamDisplay.Row.ToString();

                for (int i = 0; i < _form1.NumberOfCamera; i++)
                {
                    if (parameter.Cam[i] == null)
                    {
                        parameter.Cam[i] = new CamSetting();
                        parameter.Cam[i].HostIP = "127.0.0.1";
                        
                    }
                        
                    cameraSettings[i].IPAddress.Text = parameter.Cam[i].HostIP;
                    cameraSettings[i].tbName.Text = parameter.Cam[i].Name;
                    cameraSettings[i].Use.Checked = parameter.Cam[i].Use;
                    cameraSettings[i].SaveOrigin.Checked = parameter.Cam[i].SaveOrigin;
                    cameraSettings[i].SaveGraphic.Checked = parameter.Cam[i].SaveGraphic;
                    cameraSettings[i].SaveOriginSeparate.Checked = parameter.Cam[i].SaveOriginSeparate;
                    cameraSettings[i].ReverseX.Checked = parameter.Cam[i].ReverseX;
                    cameraSettings[i].ReverseY.Checked = parameter.Cam[i].ReverseY;

                    switch (parameter.Cam[i].ImageSource)
                    {
                        case ImageSource.Cam:
                            {
                                cameraSettings[i].ModeCam.Checked = true;
                                break;
                            }
                        case ImageSource.Folder:
                            {
                                cameraSettings[i].ModeFolder.Checked = true;
                                break;
                            }
                        case ImageSource.File:
                            {
                                cameraSettings[i].ModeFile.Checked = true;
                                break;
                            }
                        default: break;
                    }

                    switch (parameter.Cam[i].TriggerMode)
                    {
                        case TriggerMode.Software:
                            {
                                cameraSettings[i].rdSoftwareTrigger.Checked = true;
                                break;
                            }
                        case TriggerMode.HardwareAuto:
                            {
                                cameraSettings[i].rdHardwareAutoTrigger.Checked = true;
                                break;
                            }
                        case TriggerMode.HardwareManual:
                            {
                                cameraSettings[i].rdHardwareManualTrigger.Checked = true;
                                break;
                            }

                        default: break;
                    }

                    switch (parameter.Cam[i].CameraManufacture)
                    {
                        case CameraManufacture.VisionPro:
                            {
                                // Legacy value: use HIK after VisionMaster migration.
                                cameraSettings[i].rdHIK.Checked = true;
                                break;
                            }
                        case CameraManufacture.HIK:
                            {
                                cameraSettings[i].rdHIK.Checked = true;
                                break;
                            }
                        case CameraManufacture.Basler:
                            {
                                cameraSettings[i].rdBasler.Checked = true;
                                break;
                            }
                        default: break;
                    }

                    switch (parameter.Cam[i].CamConnectType)
                    {
                        case CamConnectType.Gige:
                            {
                                cameraSettings[i].rdGige.Checked = true;
                                break;
                            }
                        case CamConnectType.Usb:
                            {
                                cameraSettings[i].rdUSB.Checked = true;
                                break;
                            }
                        
                        default: break;
                    }

                    cameraSettings[i].FolderDirector.Text = parameter.Cam[i].ImageFolder;
                    cameraSettings[i].FileDirector.Text = parameter.Cam[i].ImageFile;
                }

            }
            catch (Exception ex)
            { 
            
            }
            
        }

        public bool CheckIPAddress(string IP)
        {
            
            try
            {
                if (!IP.Contains(".")) return false;
                string[] filter = IP.Split('.');
                if(filter.Length !=4 ) return false;

                foreach (string filter2 in filter)
                {
                    int thisValue = Convert.ToInt32(filter2);
                    if (thisValue < 0 || thisValue > 255) return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            
        }
        public Parameter SaveAllParameter()
        {
            Parameter parameter = new Parameter();
            try
            {
                parameter.Machine.MCTitle = txbMCTittle.Text;
                parameter.Machine.Password = txbPassWord.Text;

                if (!CheckIPAddress(txbMES_HostIP.Text))
                    throw new Exception("MES HOST IP FORMAT IS NOT CORRECT! SAVE PARAMETER FAIL");

                parameter.MES.HostIP = txbMES_HostIP.Text;
                parameter.MES.Port = Convert.ToInt32(txbMES_Port.Text);
                parameter.MES.McCode = txbMES_MCCode.Text;
                parameter.MES.CheckCount = txbMES_CheckCount.Text;
                parameter.MES.MesEnabel = cbMesEnable.Checked;

                parameter.Printer.PortName = cbbPrinterPort.Text;
                parameter.Printer.BaudRate = Convert.ToInt32(cbbPrinterBaudrate.Text);
                parameter.Printer.DataBits = Convert.ToInt32(cbbPrinterDataBits.Text);
                parameter.Printer.Parity = cbbPrinterParity.Text;
                parameter.Printer.StopBits = cbbPrinterStopbits.Text;

                if (!CheckIPAddress(txbScanner_HostIP.Text))
                    throw new Exception("SCANNER HOST IP FORMAT IS NOT CORRECT! SAVE PARAMETER FAIL");

                parameter.Scanner.HostIP = txbScanner_HostIP.Text;
                parameter.Scanner.Port = Convert.ToInt32(txbScanner_Port.Text);
                parameter.Scanner.PortName = cbbScannerPort.Text;
                parameter.Scanner.BaudRate = Convert.ToInt32(cbbScannerBaudrate.Text);
                parameter.Scanner.DataBits = Convert.ToInt32(cbbScannerDataBits.Text);
                parameter.Scanner.Parity = cbbScannerParity.Text;
                parameter.Scanner.StopBits = cbbScannerStopbits.Text;

                parameter.PLC = new PLCSetting[2];

                for (int i = 0; i < 2; i++)
                {
                    parameter.PLC[i] = new PLCSetting();
                    parameter.PLC[i].Use = PLCSettings[i].Use.Checked;
                    parameter.PLC[i].Name = PLCSettings[i].tbName.Text;
                    parameter.PLC[i].Protocol = PLCSettings[i].Protocol;
                    parameter.PLC[i].Mode = PLCSettings[i].Mode;

                    if (!CheckIPAddress(PLCSettings[i].HostIP.Text))
                    {
                        PLCSettings[i].HostIP.Text = "0.0.0.0";
                        parameter.PLC[i].HostIP = "0.0.0.0";
                    }
                    else
                    {
                        parameter.PLC[i].HostIP = PLCSettings[i].HostIP.Text;
                    }
                    parameter.PLC[i].Port = Convert.ToInt32(PLCSettings[i].Port.Text);

                    parameter.PLC[i].PortName = PLCSettings[i].PortName.Text;
                    parameter.PLC[i].BaudRate = Convert.ToInt32(PLCSettings[i].Baudrate.Text);
                    parameter.PLC[i].DataBits = Convert.ToInt32(PLCSettings[i].Databits.Text);
                    parameter.PLC[i].Parity = PLCSettings[i].Parity.Text;
                    parameter.PLC[i].StopBits = PLCSettings[i].Stopbits.Text;

                }
               
                parameter.Option.CallModelManual = cbCallModelManual.Checked;
                parameter.PathfileSave = txtFilesave.Text;
                parameter.CommonPath = txbCommonPath.Text;
                parameter.offsettaleble1 = txtoffset1.Text;
                parameter.offsettaleble2 = txtoffset2.Text;
                parameter.text_NGAll = _form1.formMain.txtNGALl.Text;
                parameter.text_OKAll = _form1.formMain.txtOkAll.Text;
                parameter.text_ALLCount = _form1.formMain.txtTotalALL.Text;

                parameter.CamDisplay.Column = Convert.ToInt32(tbCamDisplayColumn.Text);
                parameter.CamDisplay.Row = Convert.ToInt32(tbCamDisplayRow.Text);

                for (int i = 0; i < _form1.NumberOfCamera; i++)
                {
                    parameter.Cam[i] = new CamSetting();

                    if (!CheckIPAddress(cameraSettings[i].IPAddress.Text))
                    {
                        cameraSettings[i].IPAddress.Text = "0.0.0.0";
                        parameter.Cam[i].HostIP = "0.0.0.0";
                    }
                    else
                    {
                        parameter.Cam[i].HostIP = cameraSettings[i].IPAddress.Text;
                    }
                    parameter.Cam[i].Use = cameraSettings[i].Use.Checked;
                    parameter.Cam[i].Name = cameraSettings[i].tbName.Text;
                    parameter.Cam[i].SaveGraphic = cameraSettings[i].SaveGraphic.Checked;
                    parameter.Cam[i].SaveOrigin = cameraSettings[i].SaveOrigin.Checked;
                    parameter.Cam[i].SaveOriginSeparate = cameraSettings[i].SaveOriginSeparate.Checked;
                    if (cameraSettings[i].ModeCam.Checked) parameter.Cam[i].ImageSource = ImageSource.Cam;
                    if (cameraSettings[i].ModeFolder.Checked) parameter.Cam[i].ImageSource = ImageSource.Folder;
                    if (cameraSettings[i].ModeFile.Checked) parameter.Cam[i].ImageSource = ImageSource.File;

                    if (cameraSettings[i].rdSoftwareTrigger.Checked) parameter.Cam[i].TriggerMode = TriggerMode.Software;
                    if (cameraSettings[i].rdHardwareAutoTrigger.Checked) parameter.Cam[i].TriggerMode = TriggerMode.HardwareAuto;
                    if (cameraSettings[i].rdHardwareManualTrigger.Checked) parameter.Cam[i].TriggerMode = TriggerMode.HardwareManual;

                    if (cameraSettings[i].rdVisionPro.Checked) parameter.Cam[i].CameraManufacture = CameraManufacture.HIK;
                    if (cameraSettings[i].rdHIK.Checked) parameter.Cam[i].CameraManufacture = CameraManufacture.HIK;
                    if (cameraSettings[i].rdBasler.Checked) parameter.Cam[i].CameraManufacture = CameraManufacture.Basler;
                    
                    if (cameraSettings[i].rdGige.Checked) parameter.Cam[i].CamConnectType = CamConnectType.Gige;
                    if (cameraSettings[i].rdUSB.Checked) parameter.Cam[i].CamConnectType = CamConnectType.Usb;

                    parameter.Cam[i].ImageFolder = cameraSettings[i].FolderDirector.Text;
                    parameter.Cam[i].ImageFile = cameraSettings[i].FileDirector.Text;
                    parameter.Cam[i].ReverseX = cameraSettings[i].ReverseX.Checked;
                    parameter.Cam[i].ReverseY = cameraSettings[i].ReverseY.Checked;
                }
            }
            catch (Exception ex) 
            {
                parameter = null;
                MessageBox.Show(ex.ToString(), "Save Parameter Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return parameter;
        }

        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            if (_form1.MachineMode == Form1.ModeMachine.Auto)
                _form1.formMain.SwitchModeAuto(Form1.ModeMachine.Manual);
            Parameter parameter = SaveAllParameter();
           
            if (parameter != null) 
            {
                Parameters = parameter;
                string json = JsonConvert.SerializeObject(Parameters);
                //string PathFileParameter = MyDirectory() + @"\Parameter.txt";
                File.WriteAllText(PathFileParameter, json);
            }
            MessageBox.Show("SAVE PARAMETER SUCCESSFULL!", "Parameter", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
        }

        private void btnPrinterConnect_Click(object sender, EventArgs e)
        {
          
        }

        private void btnPrinterDisConnect_Click(object sender, EventArgs e)
        {
            
        }

        private void btnScannerConnect_Click(object sender, EventArgs e)
        {
           
        }

        private void btnScannerDisconnect_Click(object sender, EventArgs e)
        {
        
        }

        private void txbCommonPath_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txbCommonPath.Text = dlg.SelectedPath;
            }
        }

        private void btnRefreshPortPrinter_Click(object sender, EventArgs e)
        {
            string[] myPort = System.IO.Ports.SerialPort.GetPortNames();
            cbbPrinterPort.Items.Clear();
            cbbPrinterPort.Items.AddRange(myPort);
        }

        private void btnRefeshPortScanner_Click(object sender, EventArgs e)
        {
            string[] myPort = System.IO.Ports.SerialPort.GetPortNames();
            cbbScannerPort.Items.Clear();
            cbbScannerPort.Items.AddRange(myPort);
        }

        private void numBcodeText3_Click(object sender, EventArgs e)
        {
            NumbericEdit ThisnumbericEdit = sender as NumbericEdit;
            NumPad numPad = new NumPad();
            numPad.InitialUI(ThisnumbericEdit, null, _form1);
            numPad.Show();
        }

        private void btnMesConnect_Click(object sender, EventArgs e)
        {
            _form1.MES.ConnectTCP(Parameters.MES.HostIP, Parameters.MES.Port);
        }

        private void btnMesDisconnect_Click(object sender, EventArgs e)
        {
            _form1.MES.Disconnect();
        }

        private void btnMesTest_Click(object sender, EventArgs e)
        {
            //_form1.MES.DataInformationRequest(txbMES_CheckCount.Text);
        }

        private void txtFilesave_DoubleClick(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();


            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFilesave.Text = dlg.SelectedPath;


            }
        }

        private void txtoffset2_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
            openFileDialog.Title = "Chọn file Excel";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                txtoffset2.Text = filePath;
            }
        }

        private void txtoffset1_TextChanged(object sender, EventArgs e)
        {
           
        }
        private void txtoffset1_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
            openFileDialog.Title = "Chọn file Excel";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                txtoffset1.Text = filePath;
            }
        }
        private void txtNumber_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (int.TryParse(textBox.Text, out int value) && value >= 5) textBox.Text = "3";
        }
        private void txtNumber_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (!int.TryParse(textBox.Text, out int value) || value <= 0 || value >= 5) textBox.Text = "1";
        }
    }
}
