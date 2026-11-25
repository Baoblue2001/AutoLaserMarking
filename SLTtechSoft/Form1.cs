using CamVisionClass;
using DocumentFormat.OpenXml.Drawing.Charts;

// PLC Connection
using FluentModbusPLC;
using Newtonsoft.Json;
using PlcCommunication;
using Protocol;
using SLTtechSoft.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WindowsFormsApp1;
using static SLTtechSoft.ModelTable;

namespace SLTtechSoft
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            CheckLastProcessRunning(false);
            CheckFirstRun();
            InitializeComponent();
            CamPartNo = new int[NumberOfCamera];
            CamJigNo = new int[NumberOfCamera];
            IsCamRunContinous = new bool[NumberOfCamera];
            IsLiveCam = new bool[NumberOfCamera];
            DataWritePLC = new ushort[NumberWritePLC];
            DataWriteWPLC = new ushort[NumberWriteWPLC];
        }
     
        #region Check Last running proces
        private void CheckFirstRun()
        {

            DialogResult result = MessageBox.Show("Yes : Normal Machine Mode \nNo: Test Mode (No connect to PLC and Camera) \nCancel: Exit", "Mode Software", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                IsTestNoPLC = false;
                IsTestNoCam = new bool[] { false, false, false, false };

            }
            if (result == DialogResult.No)
            {
                IsTestNoPLC = true;
                IsTestNoCam = new bool[] { true, true, true, true };

            }
            if (result == DialogResult.Cancel)
            {
                this.Close();
                Environment.Exit(0);
            }
        }
        private void CheckLastProcessRunning(bool bShowMessage)
        {
            if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                if (bShowMessage)
                {
                    MessageBox.Show("Another process is running, Please End last process", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                this.Close();
                Environment.Exit(0);
            }
        }
        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeVision();
            InitialDevice();
            InitialUI();
        }
        private void InitialDevice()
        {
            trd = new Thread(new ThreadStart(this.ThreadTask));
            trd.IsBackground = true;

            threadCam1 = new Thread(new ThreadStart(ThreadCam_Task1));
            threadCam1.IsBackground = true;

            Matrix220 = new SerialPort();
            InitialVariablePLC();
            
            InitialMes();
            // ErrorListContent1 = ReadErrorListContent(0);
            // ErrorListContent2 = ReadErrorListContent(1);
        }
        private void RefreshSizeForm()
        {

            if (formMain != null && IndexFormShowing == 0)
            {
                formMain.WindowState = FormWindowState.Normal;
                formMain.WindowState = FormWindowState.Maximized;
            }
            if (formModel != null && IndexFormShowing == 1)
            {
                formModel.WindowState = FormWindowState.Normal;
                formModel.WindowState = FormWindowState.Maximized;
            }
            if (formIO != null && IndexFormShowing == 2)
            {
                formIO.WindowState = FormWindowState.Normal;
                formIO.WindowState = FormWindowState.Maximized;
            }
            if (formOption != null && IndexFormShowing == 3)
            {
                formOption.WindowState = FormWindowState.Normal;
                formOption.WindowState = FormWindowState.Maximized;
            }

        }
      
        /// <summary>
        /// Menu Click
        /// </summary>
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            RefreshSizeForm();
        }
        private void btnMain_Click(object sender, EventArgs e)
        {
            Program.IsLoggedIn = false;
            if (IndexFormShowing != 0)
            {
                IndexFormShowing = 0;
                formMain.Show();
                formModel.Hide();
                formIO.Hide();
                formOption.Hide();
                btnMain.BackColor = TabControlBackColorON;
                btnModel.BackColor = TabControlBackColorOFF;
                btnIO.BackColor = TabControlBackColorOFF;
                btnOption.BackColor = TabControlBackColorOFF;

              
                formIO.timer1.Enabled = false;
            }
            formModel.IsliveCamModel = false;
            IsLiveCam = new bool[4];
            RefreshSizeForm();
        }
        private void btnModel_Click(object sender, EventArgs e)
        {

            //if (!Program.IsLoggedIn) FormLogin.ShowDialog();
            //if(Program.IsLoggedIn)
            //{
            IsLiveCam = new bool[4];
            if (IndexFormShowing != 1)
                {
                    IndexFormShowing = 1;
                    formMain.Hide();
                    formModel.Show();
                    formIO.Hide();
                    formOption.Hide();
                    btnMain.BackColor = TabControlBackColorOFF;
                    btnModel.BackColor = TabControlBackColorON;
                    btnIO.BackColor = TabControlBackColorOFF;
                    btnOption.BackColor = TabControlBackColorOFF;
               
                    formIO.timer1.Enabled = false;
                }

                RefreshSizeForm();
          // }
            
        }
        private void btnIO_Click(object sender, EventArgs e)
        {
            Program.IsLoggedIn = false;
            IsLiveCam = new bool[4];
            if (IndexFormShowing != 2)
            {
                IndexFormShowing = 2;
                formMain.Hide();
                formModel.Hide();
                formIO.Show();
                formOption.Hide();
                btnMain.BackColor = TabControlBackColorOFF;
                btnModel.BackColor = TabControlBackColorOFF;
                btnIO.BackColor = TabControlBackColorON;
                btnOption.BackColor = TabControlBackColorOFF;
            
                formIO.timer1.Enabled = true;
            }
            formModel.IsliveCamModel = false;
            RefreshSizeForm();
        }
        private void btnOption_Click(object sender, EventArgs e)
        {
            Program.IsLoggedIn = false;
            IsLiveCam = new bool[4];
            if (IndexFormShowing != 3)
            {
                IndexFormShowing = 3;
                formMain.Hide();
                formModel.Hide();
                formIO.Hide();
                formOption.Show();
                btnMain.BackColor = TabControlBackColorOFF;
                btnModel.BackColor = TabControlBackColorOFF;
                btnIO.BackColor = TabControlBackColorOFF;
                btnOption.BackColor = TabControlBackColorON;
                formModel.IsliveCamModel = false;
                formIO.timer1.Enabled = false;
            }
            RefreshSizeForm();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_closingConfirmed) return;

            DialogResult result = MessageBox.Show("Do you want close this application", "Close", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            _closingConfirmed = true;
            _shutdownRequested = true;
            MachineMode = ModeMachine.Manual;
            try { timer1.Enabled = false; } catch { }
            try { formIO.timer1.Enabled = false; } catch { }
            IsLiveCam = new bool[NumberOfCamera];
            IsCamRunContinous = new bool[NumberOfCamera];
            try { DisconnectPLC(0); } catch { }
            try { formModel?.CloseProcedureEditorForShutdown(); } catch { }
            try { formMain?.ReleaseVisionMasterBindings(); } catch { }
            try { DestroyCamera(); } catch { }
            CloseEmbeddedForm(formIO);
            CloseEmbeddedForm(formOption);
            CloseEmbeddedForm(formModel);
            CloseEmbeddedForm(formMain);
            DisposeAllDevice();
        }

        private static void CloseEmbeddedForm(Form form)
        {
            if (form == null || form.IsDisposed) return;
            try { form.Hide(); } catch { }
            try { form.Dispose(); } catch { }
        }
        private void StopWorkerThreads()
        {
            _shutdownRequested = true;
            try { if (threadCam1 != null && threadCam1.IsAlive && Thread.CurrentThread != threadCam1) threadCam1.Join(2000); } catch { }
            try { if (trd != null && trd.IsAlive && Thread.CurrentThread != trd) trd.Join(1000); } catch { }
        }
        public void RunWithVisionRuntimeLock(Action action)
        {
            if (action == null) return;
            lock (_visionRuntimeSync) action();
        }
        public T RunWithVisionRuntimeLock<T>(Func<T> action)
        {
            if (action == null) return default(T);
            lock (_visionRuntimeSync) return action();
        }
        private void DisposeAllDevice()
        {
            try { PLCDispose(0); } catch { }
            try { timer1.Enabled = false; } catch { }
            threadCam1 = null;
            trd = null;
        }
        private void InitialUI()
        {

            ListLabelCamConnected.Add(lbCam1Connected);
            ListLabelCamConnected.Add(lbCam2Connected);
            ListLabelCamConnected.Add(lbCam3Connected);
            ListLabelCamConnected.Add(lbCam4Connected);

            ListLabelAcqTimeCam.Add(lbAcqTimeCam1);
            ListLabelAcqTimeCam.Add(lbAcqTimeCam2);
            ListLabelAcqTimeCam.Add(lbAcqTimeCam3);
            ListLabelAcqTimeCam.Add(lbAcqTimeCam4);

            ListLabelVisionTimeCam.Add(lbVisionTimeCam1);
            ListLabelVisionTimeCam.Add(lbVisionTimeCam2);
            ListLabelVisionTimeCam.Add(lbVisionTimeCam3);
            ListLabelVisionTimeCam.Add(lbVisionTimeCam4);

            PanelCamStatus.Add(panelCam1);
            PanelCamStatus.Add(panelCam2);
            PanelCamStatus.Add(panelCam3);
            PanelCamStatus.Add(panelCam4);
            formIO = new FormIO();
            formIO.TopLevel = false;
            this.panel8.Controls.Add(formIO);

            formOption = new FormOption();
            formOption.TopLevel = false;
            this.panel8.Controls.Add(formOption);

            formModel = new Model();
            formModel.TopLevel = false;
            this.panel8.Controls.Add(formModel);

            formMain = new Main();
            formMain.TopLevel = false;
            this.panel8.Controls.Add(formMain);
            FormLogin = new Login();
            formOption.InitializeUI(this);
            formModel.InitializeUI(this);
            formIO.InitializeUI(this);
            formMain.InitializeUI(this);
            
            formIO.timer1.Enabled = false;

            IndexFormShowing = 0;
            formMain.Show();
            btnMain.BackColor = TabControlBackColorON;
            btnModel.BackColor = TabControlBackColorOFF;
            btnIO.BackColor = TabControlBackColorOFF;
            btnOption.BackColor = TabControlBackColorOFF;
            timer1.Enabled = true;
            if (formOption.Parameters.MES.MesEnabel)
                MES.ConnectTCP(formOption.Parameters.MES.HostIP, formOption.Parameters.MES.Port);
        }
        /// <summary>
        /// Thread PLC Communication
        /// </summary>
        #region PLC Communication
        private void InitialVariablePLC()
        {
            PLC.Read.InputBits.Add("Start");
            PLC.Read.InputBits.Add("Stop");
            PLC.Read.InputBits.Add("Reset");
            PLC.Read.InputBits.Add("EMG");

            PLC.Read.AutoBits.Add(PlcDic.Auto);
            PLC.Read.AutoBits.Add(PlcDic.Stop);
            PLC.Read.AutoBits.Add(PlcDic.ComFlash);
            PLC.Read.AutoBits.Add(PlcDic.ComOn);
            PLC.Read.AutoBits.Add(PlcDic.TriggerCam[0]);
            PLC.Read.AutoBits.Add(PlcDic.TriggerCam[1]);
            PLC.Read.AutoBits.Add(PlcDic.TriggerCam[2]);
            PLC.Read.AutoBits.Add(PlcDic.TriggerCam[3]);
            PLC.Read.AutoBits.Add(PlcDic.Trigger_ReadCode);
            PLC.Read.AutoBits.Add(PlcDic.DonePosCam1);
            PLC.Read.AutoBits.Add(PlcDic.LightOn[0]);
            PLC.Read.AutoBits.Add(PlcDic.LightOn[1]);
            PLC.Read.AutoBits.Add(PlcDic.LightOn[2]);
            PLC.Read.AutoBits.Add(PlcDic.LightOn[3]);
            PLC.Read.AutoBits.Add(PlcDic.MarkingCompleteConfirm);
            PLC.Read.AutoBits.Add(PlcDic.StartMarkingProcess);
            PLC.Read.AutoBits.Add(PlcDic.MarkingStartConfirm);
            PLC.Read.AutoBits.Add(PlcDic.MarkingOneDone);
            PLC.Read.AutoBits.Add(PlcDic.Require_CheckFiducial);
            PLC.Read.AutoBits.Add(PlcDic.StartCheckFiducial);
            PLC.Read.AutoBits.Add(PlcDic.Require_CheckCode);
            PLC.Read.AutoBits.Add(PlcDic.StartCheckCode);
            PLC.Read.AutoBits.Add(PlcDic.ScanImageTop);
            PLC.Read.AutoBits.Add(PlcDic.ScanImageBottom);

            PLC.Read.WordUshorts.Add(PlcDic.ModelNo);
            PLC.Read.WordIntegers.Add(PlcDic.CurentPosX);
            PLC.Read.WordIntegers.Add(PlcDic.CurentPosY);

            //Write
            PLC.Write.AutoBits.Add(PlcDic.Auto);
            PLC.Write.AutoBits.Add(PlcDic.Stop);
            PLC.Write.AutoBits.Add(PlcDic.ComFlash);
            PLC.Write.AutoBits.Add(PlcDic.ComOn);
            PLC.Write.AutoBits.Add(PlcDic.FinishTrigger[0]);
            PLC.Write.AutoBits.Add(PlcDic.FinishTrigger[1]);
            PLC.Write.AutoBits.Add(PlcDic.FinishTrigger[2]);
            PLC.Write.AutoBits.Add(PlcDic.FinishTrigger[3]);
            PLC.Write.AutoBits.Add(PlcDic.ResultCam[0]);
            PLC.Write.AutoBits.Add(PlcDic.ResultCam[1]);
            PLC.Write.AutoBits.Add(PlcDic.ResultCam[2]);
            PLC.Write.AutoBits.Add(PlcDic.ResultCam[3]);
            PLC.Write.AutoBits.Add(PlcDic.LightOn[0]);
            PLC.Write.AutoBits.Add(PlcDic.LightOn[1]);
            PLC.Write.AutoBits.Add(PlcDic.LightOn[2]);
            PLC.Write.AutoBits.Add(PlcDic.LightOn[3]);
            PLC.Write.AutoBits.Add(PlcDic.WriteDataToPlcDone);
            PLC.Write.AutoBits.Add(PlcDic.MarkingStart);
            PLC.Write.AutoBits.Add(PlcDic.Restart_Marking);
            PLC.Write.AutoBits.Add(PlcDic.MarkingComplete);
            PLC.Write.AutoBits.Add(PlcDic.CheckFiducialOneDone);
            PLC.Write.AutoBits.Add(PlcDic.CheckFiducialALLDone);
            PLC.Write.AutoBits.Add(PlcDic.ResultFace_Corrrect);
            PLC.Write.AutoBits.Add(PlcDic.CheckCodeOneDone);
            PLC.Write.AutoBits.Add(PlcDic.CheckCodeALLDone);
            PLC.Write.AutoBits.Add(PlcDic.Result_Code_Exists);
            PLC.Write.AutoBits.Add(PlcDic.ScanImageTopDone);
            PLC.Write.AutoBits.Add(PlcDic.ScanImageBottomDone);

            PLC.Write.WordUshorts.Add(PlcDic.ModelNo);
            PLC.Write.WordIntegers.Add(PlcDic.MarkingPosX);
            PLC.Write.WordIntegers.Add(PlcDic.MarkingPosY);
            PLC.Write.WordIntegers.Add(PlcDic.CheckFiducial_PosX);
            PLC.Write.WordIntegers.Add(PlcDic.CheckFiducial_PosY);
            PLC.Write.WordIntegers.Add(PlcDic.CheckCode_PosX);
            PLC.Write.WordIntegers.Add(PlcDic.CheckCode_PosY);
        }
        private void ThreadTask()
        {
            while (!_shutdownRequested)
            {
                // In Test/Disconnected mode the old code was a tight busy-loop and could consume
                // an entire CPU core, starving WinForms/VisionMaster rendering.
                if (IsTestNoPLC || formOption == null || formOption.Parameters == null || PLCDevice == null || PLCDevice[0] == null || !PLCDevice[0].IsConnected)
                {
                    Thread.Sleep(5);
                    continue;
                }

                ReadDataPLC1();
                AutoScanImage();
                WriteDataPLC1();
            }
        }
        private void ReadDataPLC1()
        {
           // if (IsLiveCam[0]) return;
            StopwatchReadPLC = new Stopwatch();
            StopwatchReadPLC.Start();
            try
            {
                //int[] dataReadPlc123 = Fx5uPLC.ReadRegister(5000,60);
                ushort[] dataReadPlc123 = PLCDevice[0].ReadWords("7000", 2);
                ushort[] D7100 = PLCDevice[0].ReadWords("7100", 20);
                if (true)
                {
                    //if (dataReadPlc123[0] == 0) return;
                    //Auto
                    bool[] D7000 = WordConvert.UshortToBools(dataReadPlc123[0]);
                    bool[] D7001 = WordConvert.UshortToBools(dataReadPlc123[1]);
                    PLC.Read.AutoBits[PlcDic.ComOn] = D7000[0];
                    PLC.Read.AutoBits[PlcDic.ComFlash]  = D7000[1];
                    PLC.Read.AutoBits[PlcDic.TriggerCam[0]]  = D7000[2];
                    PLC.Read.AutoBits[PlcDic.StartMarkingProcess] = D7000[3];
                    PLC.Read.AutoBits[PlcDic.MarkingCompleteConfirm] = D7000[4];
                    PLC.Read.AutoBits[PlcDic.MarkingStartConfirm]  = D7000[5];
                    PLC.Read.AutoBits[PlcDic.MarkingOneDone] = D7000[6];
                    PLC.Read.AutoBits[PlcDic.ScanImageTop] = D7000[7];
                    PLC.Read.AutoBits[PlcDic.ScanImageBottom] = D7000[8];
                    PLC.Read.AutoBits[PlcDic.LightOn[0]]  = D7000[13];
                    PLC.Read.AutoBits[PlcDic.Require_CheckFiducial] = D7000[14];
                    PLC.Read.AutoBits[PlcDic.StartCheckFiducial] = D7000[15];

                    PLC.Read.AutoBits[PlcDic.Require_CheckCode] = D7001[2];
                    PLC.Read.AutoBits[PlcDic.StartCheckCode] = D7001[3];
                    //Word
                    PLC.Read.WordUshorts[PlcDic.ModelNo] = D7100[1];
                    PLC.Read.WordIntegers[PlcDic.CurentPosX] = WordConvert.USigned32Toint(D7100[10], D7100[11]);
                    PLC.Read.WordIntegers[PlcDic.CurentPosY] = WordConvert.USigned32Toint(D7100[12], D7100[13]);
                    //for (int i = 0; i < 14; i++)
                    //{
                    //    bool[] ThisBools = _WordConvert.WordTo16Bit(dataReadPlc123[14 + i]);
                    //    PLC.Read.AutoBits[PlcDic.ErrorWord1[i] = dataReadPlc123[14 + i];
                    //    for (int y = 0; y < 16; y++)
                    //    {
                    //        PLC.Read.AutoBits[PlcDic.ErrorList1[i * 16 + y] = ThisBools[y];
                    //    }
                    //}

                }
            }
            catch (Exception ex)
            {
                WriteLogPC(LogType.Main, "PLC", ex.ToString());
            }
            StopwatchReadPLC.Stop();
            TimeSpan ts = StopwatchReadPLC.Elapsed;
            TimeReadPLC = $"{ts.Seconds}.{PadLeftZeros(ts.Milliseconds, 3)}";
        }
        private void WriteDataPLC1()
        {
      
            StopwatchWritePLC = new Stopwatch();
            StopwatchWritePLC.Start();
            try
            {
                
                PLC.Write.AutoBits[PlcDic.ComOn] = true;
               
                DataWritePLC[0] = WordConvert.Bit16ToWord(
                    PLC.Write.AutoBits[PlcDic.ComOn],
                    PLC.Write.AutoBits[PlcDic.ComFlash],
                    PLC.Write.AutoBits[PlcDic.FinishTrigger[0]],
                    PLC.Write.AutoBits[PlcDic.ResultCam[0]],
                    PLC.Write.AutoBits[PlcDic.WriteDataToPlcDone],
                    PLC.Write.AutoBits[PlcDic.MarkingStart],
                    PLC.Write.AutoBits[PlcDic.Restart_Marking],
                    PLC.Write.AutoBits[PlcDic.MarkingComplete],
                    PLC.Write.AutoBits[PlcDic.ScanImageTopDone],
                    PLC.Write.AutoBits[PlcDic.ScanImageBottomDone],
                    false,
                    false,
                    false,
                    PLC.Write.AutoBits[PlcDic.LightOn[0]],
                    PLC.Write.AutoBits[PlcDic.CheckFiducialOneDone],
                    PLC.Write.AutoBits[PlcDic.CheckFiducialALLDone]
                );
                DataWritePLC[1] = WordConvert.Bit16ToWord(
                    PLC.Write.AutoBits[PlcDic.ResultFace_Corrrect],
                    false,
                    PLC.Write.AutoBits[PlcDic.CheckCodeOneDone],
                    PLC.Write.AutoBits[PlcDic.CheckCodeALLDone],
                    PLC.Write.AutoBits[PlcDic.Result_Code_Exists],
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false
                );
                PLCDevice[0].WriteWords("7500", DataWritePLC);

                //Word
                DataWriteWPLC[1] = PLC.Write.WordUshorts[PlcDic.ModelNo];
                DataWriteWPLC[10] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.MarkingPosX]).Upper;
                DataWriteWPLC[11] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.MarkingPosX]).Lowwer;
                DataWriteWPLC[12] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.MarkingPosY]).Upper;
                DataWriteWPLC[13] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.MarkingPosY]).Lowwer;
                DataWriteWPLC[14] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.CheckFiducial_PosX]).Upper;
                DataWriteWPLC[15] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.CheckFiducial_PosX]).Lowwer;
                DataWriteWPLC[16] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.CheckFiducial_PosY]).Upper;
                DataWriteWPLC[17] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.CheckFiducial_PosY]).Lowwer;
                DataWriteWPLC[18] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.CheckCode_PosX]).Upper;
                DataWriteWPLC[19] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.CheckCode_PosX]).Lowwer;
                DataWriteWPLC[20] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.CheckCode_PosY]).Upper;
                DataWriteWPLC[21] = WordConvert.IntTo2Word(PLC.Write.WordIntegers[PlcDic.CheckCode_PosY]).Lowwer;
                PLCDevice[0].WriteWords("7600", DataWriteWPLC);
            }
            catch (Exception ex)
            {
                WriteLogPC(LogType.Main, "PLC", ex.ToString());
            }
            StopwatchWritePLC.Stop();
            TimeSpan ts2 = StopwatchWritePLC.Elapsed;
            TimeWritePLC = $"{ts2.Seconds}.{PadLeftZeros(ts2.Milliseconds, 3)}";
        }
        // Auto Scan state:
        // 0 = Idle, 1 = Waiting TOP capture/save, 10 = Waiting BOT capture/save,
        // 2 = Wait PLC request release after success, 3 = Wait PLC request release after failure.
        private int IndexProcessAutoScan = 0;
        private int _autoScanCaptureResult = 0; // 0 = pending, 1 = success, -1 = failed
        private string _autoScanError = "";
        private DateTime _pendingLaserSetupCaptureAt = DateTime.MinValue;

        public void AutoScanImage()
        {
            if (IsTestNoPLC) return;
            if (PLCDevice == null || PLCDevice[0] == null || !PLCDevice[0].IsConnected) return;

            bool requestTop = PLC.Read.AutoBits[PlcDic.ScanImageTop];
            bool requestBottom = PLC.Read.AutoBits[PlcDic.ScanImageBottom];

            // Fail a pending camera request if no frame arrives within five seconds.
            Action<Bitmap, int, int> timedOutCapture = null;
            lock (_laserSetupCaptureSync)
            {
                if (_pendingLaserSetupCapture != null &&
                    _pendingLaserSetupCaptureAt != DateTime.MinValue &&
                    (DateTime.Now - _pendingLaserSetupCaptureAt).TotalSeconds >= 5)
                {
                    timedOutCapture = _pendingLaserSetupCapture;
                    _pendingLaserSetupCapture = null;
                    _pendingLaserSetupCaptureCamera = -1;
                    _pendingLaserSetupCaptureAt = DateTime.MinValue;
                }
            }
            if (timedOutCapture != null)
                timedOutCapture(null, 0, 0);

            switch (IndexProcessAutoScan)
            {
                case 0:
                    // Accept one surface request at a time.
                    if (requestTop && !requestBottom)
                    {
                        PLC.Write.AutoBits[PlcDic.ScanImageTopDone] = false;
                        PLC.Write.AutoBits[PlcDic.ScanImageBottomDone] = false;
                        Interlocked.Exchange(ref _autoScanCaptureResult, 0);
                        _autoScanError = "";
                        IndexProcessAutoScan = 1;

                        Ui(() =>
                        {
                            try
                            {
                                if (formModel == null || formModel.laserSettingLayout == null)
                                    throw new InvalidOperationException("Laser Setting form is not initialized.");

                                formModel.laserSettingLayout.SwitchSurface(LaserSettingLayout.LaserSurface.Top);
                                if (!formModel.laserSettingLayout.ScanImageAuto((success, message) =>
                                {
                                    _autoScanError = message ?? "";
                                    Interlocked.Exchange(ref _autoScanCaptureResult, success ? 1 : -1);
                                }))
                                {
                                    _autoScanError = "Could not start TOP image scan.";
                                    Interlocked.Exchange(ref _autoScanCaptureResult, -1);
                                }
                            }
                            catch (Exception ex)
                            {
                                _autoScanError = ex.Message;
                                Interlocked.Exchange(ref _autoScanCaptureResult, -1);
                                WriteLogPC(LogType.Vision, "Auto Scan TOP", ex.ToString());
                            }
                        });
                    }
                    else if (requestBottom && !requestTop)
                    {
                        PLC.Write.AutoBits[PlcDic.ScanImageTopDone] = false;
                        PLC.Write.AutoBits[PlcDic.ScanImageBottomDone] = false;
                        Interlocked.Exchange(ref _autoScanCaptureResult, 0);
                        _autoScanError = "";
                        IndexProcessAutoScan = 10;

                        Ui(() =>
                        {
                            try
                            {
                                if (formModel == null || formModel.laserSettingLayout == null)
                                    throw new InvalidOperationException("Laser Setting form is not initialized.");

                                formModel.laserSettingLayout.SwitchSurface(LaserSettingLayout.LaserSurface.Bot);
                                if (!formModel.laserSettingLayout.ScanImageAuto((success, message) =>
                                {
                                    _autoScanError = message ?? "";
                                    Interlocked.Exchange(ref _autoScanCaptureResult, success ? 1 : -1);
                                }))
                                {
                                    _autoScanError = "Could not start BOTTOM image scan.";
                                    Interlocked.Exchange(ref _autoScanCaptureResult, -1);
                                }
                            }
                            catch (Exception ex)
                            {
                                _autoScanError = ex.Message;
                                Interlocked.Exchange(ref _autoScanCaptureResult, -1);
                                WriteLogPC(LogType.Vision, "Auto Scan BOTTOM", ex.ToString());
                            }
                        });
                    }
                    break;

                case 1:
                case 10:
                    int captureResult = Interlocked.Exchange(ref _autoScanCaptureResult, 0);
                    if (captureResult == 1)
                    {
                        if (IndexProcessAutoScan == 1)
                            PLC.Write.AutoBits[PlcDic.ScanImageTopDone] = true;
                        else
                            PLC.Write.AutoBits[PlcDic.ScanImageBottomDone] = true;

                        WriteLogPC(LogType.Vision, "Auto Scan",
                            IndexProcessAutoScan == 1 ? "TOP image captured and saved successfully." : "BOTTOM image captured and saved successfully.");
                        IndexProcessAutoScan = 2;
                    }
                    else if (captureResult == -1)
                    {
                        PLC.Write.AutoBits[PlcDic.ScanImageTopDone] = false;
                        PLC.Write.AutoBits[PlcDic.ScanImageBottomDone] = false;
                        WriteLogPC(LogType.Vision, "Auto Scan",
                            "Scan failed: " + (string.IsNullOrWhiteSpace(_autoScanError) ? "Unknown capture/save error." : _autoScanError));
                        IndexProcessAutoScan = 3;
                    }
                    break;

                case 2:
                case 3:
                    // Hold the Done bit until PLC releases both request bits.
                    if (!requestTop && !requestBottom)
                    {
                        PLC.Write.AutoBits[PlcDic.ScanImageTopDone] = false;
                        PLC.Write.AutoBits[PlcDic.ScanImageBottomDone] = false;
                        IndexProcessAutoScan = 0;
                    }
                    break;
            }
        }
        private void Ui(Action action)
        {
            if (IsDisposed || Disposing) return;
            if (InvokeRequired) BeginInvoke(action); else action();
        }
        private void PLCUpdateConnectionState(bool connected)
        {
            lbPLCConected.BackColor = connected ? Color.Green : Color.IndianRed;
        }
        private void PLCAddLog(string message)
        {
            WriteLogPC(LogType.Main, $"PLC{1}", message);
        }
        public void ConnectPLC(int index)
        {
            try
            {
                if (IsTestNoPLC) return;
                if (!formOption.Parameters.PLC[index].Use) return;
                if (PLCDevice != null)
                    if (PLCDevice[index] != null)
                    {
                        if (PLCDevice[index].IsConnected)
                        {
                            PLCDevice[index].Disconnect();
                        }
                        PLCDevice[index].ConnectionChanged -= connected => Ui(() => PLCUpdateConnectionState(connected));
                        PLCDevice[index].Log -= message => Ui(() => PLCAddLog(message));
                        PLCDevice[index].Dispose();
                    }
                
                PlcParity sParity = (PlcParity)Enum.Parse(typeof(PlcParity), formOption.Parameters.PLC[index].Parity, true);
                PlcStopBits sStopBits = (PlcStopBits)Enum.Parse(typeof(PlcStopBits), formOption.Parameters.PLC[index].StopBits, true);

                switch (formOption.Parameters.PLC[index].Protocol)
                {
                    case Class.Protocol.Modbus:
                        {
                            if (formOption.Parameters.PLC[index].Mode == ModbusMode.Tcp)
                            {
                                PLCDevice[index] = PlcFactory.Create(new PlcConfig
                                {
                                    Protocol = PlcProtocol.ModbusTcp,
                                    IpAddress = formOption.Parameters.PLC[index].HostIP,
                                    Port = formOption.Parameters.PLC[index].Port,
                                    UnitId = 1,
                                    AutoReconnect = true,
                                    ReconnectInterval = 1000
                                });
                            }
                            else
                            {
                                PLCDevice[index] = PlcFactory.Create(new PlcConfig
                                {
                                    Protocol = PlcProtocol.ModbusRtu,
                                    ComPort = formOption.Parameters.PLC[index].PortName,
                                    BaudRate = formOption.Parameters.PLC[index].BaudRate,
                                    Parity = sParity,
                                    StopBits = sStopBits,
                                    UnitId = 1,
                                    AutoReconnect = true
                                });
                            }
                            break;
                        }
                    case Class.Protocol.XGT:
                        {
                            PLCDevice[index] = PlcFactory.Create(new PlcConfig
                            {
                                Protocol = PlcProtocol.LsXgtTcp,
                                IpAddress = formOption.Parameters.PLC[index].HostIP,
                                Port = 2004,
                                XgtCpuType = PlcXgtCpuType.XgbMk,
                                AutoReconnect = true
                            });
                            break;
                        }
                    case Class.Protocol.SLMP:
                        {
                            PLCDevice[index] = PlcFactory.Create(new PlcConfig
                            {
                                Protocol = PlcProtocol.MitsubishiSlmpTcp,
                                IpAddress = formOption.Parameters.PLC[index].HostIP,
                                Port = formOption.Parameters.PLC[index].Port,
                                XgtCpuType = PlcXgtCpuType.XgbMk,
                                AutoReconnect = true
                            });
                            break;
                        }
                    default:
                        {

                            break;
                        }
                }

                PLCDevice[index].ConnectionChanged += connected => Ui(() => PLCUpdateConnectionState(connected));
                PLCDevice[index].Log += message => Ui(() => PLCAddLog(message));

                PLCDevice[index].Connect();
            }
            catch (Exception ex)
            {
                WriteLogPC(LogType.Main, "PLC Connect", ex.ToString());
            }

        }
        public void DisconnectPLC(int index)
        {
            if (PLCDevice == null || index < 0 || index >= PLCDevice.Length) return;

            if (PLCDevice[index] != null)
            {
              if (PLCDevice[index].IsConnected)
                    PLCDevice[index].Disconnect();
            }
        }
        public void PLCDispose(int index)
        {
            if (PLCDevice == null || index < 0 || index >= PLCDevice.Length) return;

            if (PLCDevice[index] != null)
            {
                PLCDevice[index].Dispose();
            }
        }
        #endregion
        private void timer1_Tick(object sender, EventArgs e)
        {
            //CallModelAutoPLC();
            if (CountIntervalTimer1++ > 20)
            {
                if (formOption != null)
                {
                    if (formOption.Parameters != null)
                    {
                        if (formOption.Parameters.CommonPath != null)
                        {
                            DeleteOldFolderImage(formOption.Parameters.CommonPath,1);
                            //DeleteOldFolderImage(formOption.Parameters.CommonPath, 2);
                        }
                    }
                }
                RunContinueCam();
               
                CountIntervalTimer1 = 0;
            }
            InitialOneTimeTimer();
         ;
            if (!IsTestNoPLC)
            {
                //UpdateErrorListPLC1();
                //UpdateErrorListPLC2();
            }
           
            lbProcessStepNo.Text = ProcessIndex[2].ToString();
            lbProcessStepName.Text = ProcessStepName;

            lbTimeReadPLC.Text = TimeReadPLC;
            lbTimeWritePLC.Text = TimeWritePLC;
          
            if (CountCheckPLC_On++ > 5)
            {
                CountCheckPLC_On = 0;
                PLC.Write.AutoBits[PlcDic.ComFlash] = !PLC.Write.AutoBits[PlcDic.ComFlash];
            }
            //Hiển thị các trạng thái kết nối
            UpdateLabelStatus();
        }
        private List<Label> ListLabelCamConnected = new List<Label>();
        private List<Label> ListLabelAcqTimeCam = new List<Label>();
        private List<Label> ListLabelVisionTimeCam = new List<Label>();
        private void UpdateLabelStatus()
        {
            for (int i = 0; i < NumberOfCamera; i++)
            {
                if (_VisionSystem != null)
                {
                    if (_VisionSystem[i] != null)
                    {
                        if (_VisionSystem[i].CAM != null)
                        {
                            ListLabelCamConnected[i].BackColor = _VisionSystem[i].CAM.IsOpen ? Color.Green : Color.IndianRed;
                        }
                        ListLabelAcqTimeCam[i].Text = _VisionSystem[i].CamGrapTime.ToString();
                        ListLabelVisionTimeCam[i].Text = _VisionSystem[i].TimeVision;
                    }
                }
            }
        }
        private void CallModelAutoPLC()
        {
            if (formOption.Parameters.Option.CallModelManual) return;
            ModelColumns Col = new ModelColumns();

            int ModelNo = PLC.Read.WordUshorts[PlcDic.ModelNo];
            if (ModelNo != ModelNoInternal)
            {
                if (ModelNo >= formModel.modelTable.Table.Rows.Count - 1) return;
                if (formModel.modelTable.Table.Rows[ModelNo].Cells[Col.File].Value == null) return;
                MachineMode = ModeMachine.Manual;

                formModel.LoadAModel(ModelNo);
                MachineMode = ModeMachine.Auto;
                formMain.UpdateControlStatus();
            }
        }
        private void InitialOneTimeTimer()
        {
            if (CountStartThread <= 30)
            {
                CountStartThread++;
            }
            if (CountStartThread == 10)
            {
                InitialPrinter();
                threadCam1.Start();
                ConnectPLC(0);
                trd.Start();
                panelPLC1.Visible = true;
                panelPLC2.Visible = false;
                panelScanner.Visible = false;
                if (formOption != null)
                    if (formOption.Parameters != null)
                    {
                        for (int i = 0; i < NumberOfCamera; i++)
                        {
                            if (formOption.Parameters.Cam[i] != null)
                                if (formOption.Parameters.Cam[i].Use)
                                {
                                    PanelCamStatus[i].Visible = true;
                                    for (int y = 0 ; y < formModel.ListFrmVisionFileSetup[i].GetRowCount(); y++)
                                    {
                                        if (formMain.CameraVisionDisplays[i] != null)
                                            if (formMain.CameraVisionDisplays[i].List[y] != null)
                                                formMain.CameraVisionDisplays[i].List[y].RefreshDisplay();
                                    }
                                }
                        }
                    }
            }
        }
        /// <summary>
        /// Initial VISION COGNEX
        /// </summary>
        public void InitializeVision()
        {
            _VisionSystem = new VisionSystem[NumberOfCamera];
            for (int i = 0; i < _VisionSystem.Length; i++)
            {
                _VisionSystem[i] = new VisionSystem();
            }
            IsCamRunContinous = new bool[NumberOfCamera];
                IsLiveCam = new bool[NumberOfCamera];
        }
        public void InitialAllCameras()
        {
            RunWithVisionRuntimeLock(InitialAllCamerasCore);
        }
        private void InitialAllCamerasCore()
        {
            if (_shutdownRequested) return;
            formMain.SwitchModeAuto(ModeMachine.Manual);
            if (formOption?.Parameters?.Cam == null || formModel?.modelParameter?.Cam == null) return;
            if (formModel.modelParameter.Cam.Length != NumberOfCamera) return;

            for (int i = 0; i < NumberOfCamera; i++)
            {
                if (i >= formOption.Parameters.Cam.Length || formOption.Parameters.Cam[i] == null) continue;
                if (_VisionSystem[i] == null || formModel.modelParameter.Cam[i] == null) continue;

                if (!formOption.Parameters.Cam[i].Use)
                {
                    DisposeVisionMasterProcesses(i);
                    continue;
                }

                // Model Call changes PRC/exposure data, not the physical camera configuration.
                // Reuse an already-open camera and reconnect only when it is missing/closed.
                if (!IsTestNoCam[i] && (_VisionSystem[i].CAM == null || !_VisionSystem[i].CAM.IsOpen)) InitializeCamera(i);

                InitialVisionMasterProcesses(i);

                if (_VisionSystem[i].CAM != null && _VisionSystem[i].CAM.IsOpen && formModel.ListFrmVisionFileSetup[i].GetRowCount() > 0)
                    ChangeParameterCam(i, 0);
            }
        }
        private void DisposeVisionMasterProcesses(int cam)
        {
            if (_VisionSystem == null || cam < 0 || cam >= _VisionSystem.Length || _VisionSystem[cam] == null) return;
            VisionMasterProcess[] processes = _VisionSystem[cam].VisionProcesses;
            if (processes != null)
            {
                foreach (VisionMasterProcess process in processes)
                {
                    try { process?.Dispose(); } catch { }
                }
            }
            _VisionSystem[cam].VisionProcesses = new VisionMasterProcess[0];
            _VisionSystem[cam].IsVisionProcessInitial = false;
        }
        public void InitialVisionMasterProcesses(int cam)
        {
            if (_VisionSystem == null || cam < 0 || cam >= _VisionSystem.Length || _VisionSystem[cam] == null) return;
            _VisionSystem[cam].IsVisionProcessInitial = false;

            int numberOfPart = formModel.ListFrmVisionFileSetup[cam]?.GetRowCount() ?? 0;
            if (numberOfPart <= 0)
            {
                DisposeVisionMasterProcesses(cam);
                return;
            }

            // Calling the same model again must not dispose/reload identical PRC instances.
            // Apart from being unnecessary, repeated VmProcedure.Dispose() is exactly where VM 4.3
            // can hit its lazy-RelateIO assertion. Reuse the runtime when every PRC path is unchanged.
            if (CanReuseVisionMasterProcesses(cam, numberOfPart))
            {
                _VisionSystem[cam].IsVisionProcessInitial = true;
                BindVisionMasterDisplays(cam);
                WriteLogPC(LogType.Vision, $"VisionMaster Cam{cam + 1}", $"Reused {numberOfPart} runtime procedure(s).");
                return;
            }

            // A different PRC set must release the old procedures before loading the new set;
            // VmProcedure.Load registers procedures globally and duplicate Flow entries are unsafe.
            DisposeVisionMasterProcesses(cam);

            VisionMasterProcess[] loaded = new VisionMasterProcess[numberOfPart];
            try
            {
                for (int i = 0; i < numberOfPart; i++)
                {
                    loaded[i] = formModel.LoadVisionMasterProcessIndex(cam, i);
                    if (loaded[i] == null || loaded[i].Procedure == null)
                        throw new Exception($"VisionMaster PRC could not be loaded. Cam {cam + 1}, index {i}, file: {formModel.ListFrmVisionFileSetup[cam].GetFileName(i)}");
                }

                _VisionSystem[cam].VisionProcesses = loaded;
                _VisionSystem[cam].IsVisionProcessInitial = true;
                // Do NOT enable callbacks globally for every module. Runtime displays are bound to
                // specific procedures below; enabling every module callback adds unnecessary work
                // on each trigger when a PRC contains many tools.
                BindVisionMasterDisplays(cam);
                WriteLogPC(LogType.Vision, $"VisionMaster Cam{cam + 1}", $"Loaded {numberOfPart} runtime procedure(s).");
            }
            catch (Exception ex)
            {
                foreach (VisionMasterProcess process in loaded)
                {
                    try { process?.Dispose(); } catch { }
                }
                _VisionSystem[cam].VisionProcesses = new VisionMasterProcess[0];
                _VisionSystem[cam].IsVisionProcessInitial = false;
                WriteLogPC(LogType.Vision, $"VisionMaster Cam{cam + 1} Load", ex.ToString());
            }
        }
        public void BindAllVisionMasterDisplays()
        {
            if (_shutdownRequested || formMain == null || !formMain.IsUiInitialized) return;
            for (int cam = 0; cam < NumberOfCamera; cam++) BindVisionMasterDisplays(cam);
        }
        private void BindVisionMasterDisplays(int cam)
        {
            if (_shutdownRequested || formMain == null || !formMain.IsUiInitialized) return;
            if (_VisionSystem == null || cam < 0 || cam >= _VisionSystem.Length || _VisionSystem[cam] == null) return;
            if (formMain.CameraVisionDisplays == null || cam >= formMain.CameraVisionDisplays.Length || formMain.CameraVisionDisplays[cam] == null) return;

            VisionMasterProcess[] processes = _VisionSystem[cam].VisionProcesses;
            if (processes == null) return;
            int count = Math.Min(processes.Length, formMain.CameraVisionDisplays[cam].List.Count);

            for (int index = 0; index < count; index++)
            {
                VisionMasterProcess process = processes[index];
                CameraVisionDisplay cameraDisplay = formMain.CameraVisionDisplays[cam].List[index];
                if (process?.Procedure == null || cameraDisplay?.partOfCams == null || cameraDisplay.partOfCams.Length == 0 || cameraDisplay.partOfCams[0]?.mDisplay == null) continue;

                // Same pattern as VisionMaster SDK samples: bind VmRenderControl once.
                // Subsequent Procedure.Run() calls update the same render source automatically.
                process.EnableDisplayCallbacks();
                VisionMasterUiHelper.BindProcedure(cameraDisplay.partOfCams[0].mDisplay, process.Procedure);
            }
        }
        private bool CanReuseVisionMasterProcesses(int cam, int numberOfPart)
        {
            VisionMasterProcess[] processes = _VisionSystem[cam].VisionProcesses;
            if (processes == null || processes.Length != numberOfPart) return false;

            for (int i = 0; i < numberOfPart; i++)
            {
                VisionMasterProcess process = processes[i];
                string requestedFile = formModel.ListFrmVisionFileSetup[cam].GetFileName(i);
                if (process == null || process.Procedure == null || !SameVisionFilePath(process.FilePath, requestedFile)) return false;
            }
            return true;
        }
        private static bool SameVisionFilePath(string path1, string path2)
        {
            try
            {
                path1 = NormalizeVisionPathForCompare(path1);
                path2 = NormalizeVisionPathForCompare(path2);
                if (string.IsNullOrWhiteSpace(path1) || string.IsNullOrWhiteSpace(path2)) return false;
                return string.Equals(Path.GetFullPath(path1).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(path2).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
            }
            catch { return string.Equals(path1?.Trim(), path2?.Trim(), StringComparison.OrdinalIgnoreCase); }
        }
        private static string NormalizeVisionPathForCompare(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";
            path = path.Trim();
            if (string.Equals(Path.GetExtension(path), ".vpp", StringComparison.OrdinalIgnoreCase))
            {
                string prc = Path.ChangeExtension(path, ".prc");
                if (File.Exists(prc)) path = prc;
            }
            return path;
        }
        public void ChangeParameterCam(int cam, int index)
        {
            try
            {
                if (_VisionSystem[cam].CAM == null) return;
                if (!_VisionSystem[cam].CAM.IsOpen) return;

                if (formOption.Parameters.Cam[cam].ImageSource == ImageSource.Cam)
                {
                    _VisionSystem[cam].CAM.ExposureTime = formModel.ListFrmVisionFileSetup[cam].GetExposure(index);
                }
            }
            catch (Exception ex)
            {
                //Log
                WriteLogPC(LogType.Vision, $"ChangeParameterCam({cam}), index {index}", ex.ToString());
            }
        }

        /// <summary>
        /// Camera
        /// </summary>
        public void InitializeCamera(int index)
        {
            if (IsTestNoCam[index]) return;
            try
            {
                _VisionSystem[index].StopWatchVision = new Stopwatch();

                if (_VisionSystem[index].CAM != null)
                {
                    _VisionSystem[index].CAM.ImageReceived -= Camera_ImageReceived;
                    _VisionSystem[index].CAM.Error -= Camera_Error;

                    _VisionSystem[index].CAM.Dispose();
                    _VisionSystem[index].CAM = null;
                }

                switch (formOption.Parameters.Cam[index].CameraManufacture)
                {
                    // Legacy VisionPro value is mapped to HIK so old parameter files can be reused
                    // after migrating the vision runtime to VisionMaster.
                    case CameraManufacture.VisionPro:
                    case CameraManufacture.HIK:
                        {
                            _VisionSystem[index].CAM = new CamVisionClass.HIKCamera();
                            break;
                        }
                    case CameraManufacture.Basler:
                        {
                            _VisionSystem[index].CAM = new CamVisionClass.BaslerCamera();
                            break;
                        }
                    default: break;
                }
                _VisionSystem[index].CAM.No = index;

                _VisionSystem[index].CAM.ImageReceived += Camera_ImageReceived;
                _VisionSystem[index].CAM.Error += Camera_Error;
                

                switch (formOption.Parameters.Cam[index].CamConnectType)
                {
                    case CamConnectType.Gige:
                        {
                            _VisionSystem[index].CAM.Open(formOption.Parameters.Cam[index].HostIP);
                            break;
                        }
                    case CamConnectType.Usb:
                        {
                            _VisionSystem[index].CAM.OpenByDeviceID(formOption.Parameters.Cam[index].HostIP);
                            break;
                        }
                   
                    default: break;
                }
                _VisionSystem[index].CAM.ExposureTime = formModel.ListFrmVisionFileSetup[index].GetExposure(0) ;

                // _VisionSystem[index].CAM.Gain = (float)formModel.modelParameter.Cam[index].Gain;
                //_VisionSystem[index].CAM.SetReverseX(formOption.Parameters.Cam[index].ReverseX);
                //_VisionSystem[index].CAM.SetReverseY(formOption.Parameters.Cam[index].ReverseY);

                switch (formOption.Parameters.Cam[index].TriggerMode)
                {
                    case TriggerMode.Software:
                        {
                            _VisionSystem[index].CAM.SetSoftwareTriggerMode();
                            
                            break;
                        }
                    case TriggerMode.HardwareAuto:
                        {
                            _VisionSystem[index].CAM.SetLineTriggerMode();
                            _VisionSystem[index].CAM.SetLightStrobeMode(true);
                            break;
                        }
                    case TriggerMode.HardwareManual:
                        {
                            _VisionSystem[index].CAM.SetLineTriggerMode();
                            _VisionSystem[index].CAM.SetLightStrobeMode(false);
                            break;
                        }
                    default: break;
                }
                _VisionSystem[index].CAM.StartGrabbing();
            }
            catch (Exception ex)
            {
                //Log
                WriteLogPC(LogType.Vision, $"Cam Connect", $"Index:{index}, {ex.Message}");
            }
        }
        private void TriggerOneWithLight1(int index)
        {
            if (_VisionSystem[index].TriggerOne)
            {
                PLC.Write.AutoBits[PlcDic.LightOn[index]] = true;
                if (!PLC.Read.AutoBits[PlcDic.LightOn[index]])
                {
                    AcqStart(index);
                    _VisionSystem[index].TriggerOne = false;
                }
            }

        }
        private bool AcqCanStart(int index)
        {
            // Check trigger model to see if it's okay to call acq fifo's StartAcquire method
            if (_VisionSystem[index].CAM == null)
                return false;
            if (!_VisionSystem[index].CAM.IsOpen)
                return false;

            return true;
        }
        public void AcqStart(int index)
        {
            //if (_VisionSystem[index].IsTriggerStart) return;
            try
            {
                if (AcqCanStart(index))
                {
                    _VisionSystem[index].StopWatchVision.Reset();
                    _VisionSystem[index].StopWatchVision.Start();
                    _VisionSystem[index].IsTriggerStart = true;
                    _VisionSystem[index].CAM.SoftwareTrigger();
                }

            }
            catch (Exception ex)
            {
                WriteLogPC(LogType.Vision, $"Cam{index}", $"{ex.Message}");

            }
          
        }
        public void LiveCamera(int cam)
        {
            if (MachineMode == ModeMachine.Auto) return;
            if (IsCamRunContinous[cam]) return;
            if (_VisionSystem[cam] == null) return;
            if (_VisionSystem[cam].CAM == null) return;

            _VisionSystem[cam].CAM.StopGrabbing();
            _VisionSystem[cam].CAM.SetContinuousMode();
            _VisionSystem[cam].CAM.StartGrabbing();
        }
        public void StopLiveCamera(int cam)
        {
            if (MachineMode == ModeMachine.Auto) return;
            if (IsCamRunContinous[cam]) return;
            if (_VisionSystem[cam] == null) return;
            if (_VisionSystem[cam].CAM == null) return;

            _VisionSystem[cam].CAM.StopGrabbing();
            _VisionSystem[cam].CAM.SetSoftwareTriggerMode();
            _VisionSystem[cam].CAM.StartGrabbing();
        }
        private void RunContinueCam()
        {
            if (MachineMode == ModeMachine.Auto) return;

            for (int i = 0; i < NumberOfCamera; i++)
            {
                if (IsLiveCam[i]) return;
                if (_VisionSystem[i] == null) return;
                if (_VisionSystem[i].VisionProcesses == null) return;
                if (_VisionSystem[i].VisionProcesses.Length == 0 || _VisionSystem[i].VisionProcesses[0] == null) return;

                if (IsCamRunContinous[i])
                {

                    if (formOption.Parameters.Cam[i].ImageSource == ImageSource.Cam)
                    {
                        if (_VisionSystem[i].CAM == null) return;
                        if (!PLC.Read.AutoBits[PlcDic.LightOn[i]])
                            _VisionSystem[i].TriggerOne = true;
                        //AcqStart(i);
                    }
                    else
                    {
                        TriggerCamManual(i,0, false, false);
                    }
                }
            }

        }
        private void Camera_Error(object sender, CameraErrorEventArgs e)
        {
            ICamera camera = sender as ICamera;

            WriteLogPC(LogType.Vision, $"Cam{camera.No}", $"{e.Message}, 0x{e.Message}");
        }
        private readonly object _laserSetupCaptureSync = new object();
        private Action<Bitmap, int, int> _pendingLaserSetupCapture;
        private int _pendingLaserSetupCaptureCamera = -1;

        /// <summary>Request one camera frame for Laser Setup and pair it with the servo position at frame arrival.</summary>
        public bool RequestLaserSetupCapture(int cameraIndex, Action<Bitmap, int, int> onCaptured)
        {
            if (onCaptured == null || _VisionSystem == null || cameraIndex < 0 || cameraIndex >= _VisionSystem.Length
                || _VisionSystem[cameraIndex] == null || _VisionSystem[cameraIndex].CAM == null
                || !_VisionSystem[cameraIndex].CAM.IsOpen)
                return false;

            lock (_laserSetupCaptureSync)
            {
                if (_pendingLaserSetupCapture != null) return false;
                _pendingLaserSetupCapture = onCaptured;
                _pendingLaserSetupCaptureCamera = cameraIndex;
                _pendingLaserSetupCaptureAt = DateTime.Now;
            }

            // Continuous mode: use the next incoming frame. Software-trigger mode: request one frame.
            if (_VisionSystem[cameraIndex].CAM.AcquisitionMode != CameraAcquisitionMode.Continuous)
                AcqStart(cameraIndex);
            return true;
        }

        private void Camera_ImageReceived(object sender,CameraImageEventArgs e)
        {
            ICamera camera = sender as ICamera;
            if (camera == null) return;

            int index = camera.No;
            Action<Bitmap, int, int> scanCallback = null;
            lock (_laserSetupCaptureSync)
            {
                if (_pendingLaserSetupCapture != null && _pendingLaserSetupCaptureCamera == index)
                {
                    scanCallback = _pendingLaserSetupCapture;
                    _pendingLaserSetupCapture = null;
                    _pendingLaserSetupCaptureCamera = -1;
                    _pendingLaserSetupCaptureAt = DateTime.MinValue;
                }
            }

            if (scanCallback != null)
            {
                Bitmap scanImage = null;
                try
                {
                    // Read PLC positions only for the requested setup image, not for every camera frame.
                    int servoX = PLC.Read.WordIntegers[PlcDic.CurentPosX];
                    int servoY = PLC.Read.WordIntegers[PlcDic.CurentPosY];
                    scanImage = (Bitmap)e.Image.Clone();
                    scanCallback(scanImage, servoX, servoY);
                    scanImage = null; // ownership passed to callback
                }
                catch (Exception ex)
                {
                    WriteLogPC(LogType.Vision, "Laser Setup Scan", ex.Message);
                    // The pending request has already been consumed. Notify the caller so
                    // Auto Scan cannot remain in its waiting state after a frame-processing error.
                    try { scanCallback(null, 0, 0); } catch { }
                }
                finally
                {
                    if (scanImage != null) scanImage.Dispose();
                }
            }

            Bitmap image = (Bitmap)e.Image.Clone();
            e.Image.Dispose();
            _VisionSystem[index].CamGrapTime = (int)_VisionSystem[index].CAM.GrabTimeMs;
            // Xử lý ảnh ở camera/background thread
            ProcessImage(index, image);
            image.Dispose();
        }
        private void ProcessImage(int Cam, Bitmap image)
        {
            try
            {
                // Do some processing while acquiring next image
                if (image != null)
                {
                    // image is already a private clone made by Camera_ImageReceived.
                    // Reuse that same Bitmap all the way to VisionMaster instead of cloning it again.
                    ImageInfo ImageGrap = new ImageInfo();
                    ImageGrap.Image = image;
                    ImageGrap.Coordinate = new Coordinate();

                    
                    if (_VisionSystem[Cam].CAM.AcquisitionMode != CameraAcquisitionMode.Continuous 
                        && !formModel.IsliveCamModel)
                    {
                        switch (MachineMode)
                        {
                            case ModeMachine.Auto:
                                {
                                    _VisionSystem[Cam].ResultTriggerOne = TriggerCamAuto(Cam,_VisionSystem[Cam].TriggerIndex, ImageGrap, true, true);
                                    _VisionSystem[Cam].FinishTrigger = true;
                                   
                                    break;
                                }
                            case ModeMachine.Manual:
                                {
                                    // Do not auto-fit the render view on every manual trigger.
                                    // Re-fitting each frame is visually perceived as a jump/flicker.
                                    TriggerCamAuto(Cam, _VisionSystem[Cam].TriggerIndex, ImageGrap, false, false);
                                    break;
                                }
                            case ModeMachine.Teaching:
                                {
                                    formModel.RunOneVisionMasterProcessEdit(ImageGrap.Image);
                                    break;
                                }
                            default: break;
                        }
                        PLC.Write.AutoBits[PlcDic.LightOn[Cam]] = false;
                    }
                    else
                    {
                        // ShowBitmap clones the frame before marshaling to the UI, so the camera
                        // callback can safely dispose its source Bitmap after ProcessImage returns.
                        VisionMasterUiHelper.ShowBitmap(formMain.CameraVisionDisplays[Cam].List[0].partOfCams[0].mDisplay, ImageGrap.Image);
                    }
                }
            }
            catch (Exception ex) 
            {
                //Log
                _VisionSystem[Cam].ErrorGrap = true;
                //_VisionSystem[0].FinishTrigger = true;
                PLC.Write.AutoBits[PlcDic.LightOn[Cam]] = false;
                WriteLogPC(LogType.Vision, $"Cam{Cam}", $"{ex.Message}");
            }
            _VisionSystem[Cam].StopWatchVision.Stop();
            //_VisionSystem[Cam].TimeVision = _VisionSystem[Cam].StopWatchVision.ElapsedMilliseconds.ToString();
            _VisionSystem[Cam].IsTriggerStart = false;
        }
        private void DestroyCamera()
        {
            if (_VisionSystem == null) return;

            for (int i = 0; i < NumberOfCamera; i++)
            {
                if (i >= _VisionSystem.Length || _VisionSystem[i] == null) continue;

                try { _VisionSystem[i].CAM?.Dispose(); } catch { }
                _VisionSystem[i].CAM = null;

                if (_VisionSystem[i].VisionProcesses != null)
                {
                    for (int j = 0; j < _VisionSystem[i].VisionProcesses.Length; j++)
                    {
                        try { _VisionSystem[i].VisionProcesses[j]?.Dispose(); } catch { }
                        _VisionSystem[i].VisionProcesses[j] = null;
                    }
                    _VisionSystem[i].VisionProcesses = null;
                }

                try { _VisionSystem[i].LastInputImage?.Dispose(); } catch { }
                _VisionSystem[i].LastInputImage = null;
            }
        }
        /// <summary>
        /// ThreadCam_Task1
        /// </summary>
        private void ThreadCam_Task1()
        {
            while (!_shutdownRequested)
            {
                ProcessCam(0);
                //ProcessCam(1);
                //ProcessCam(2);

                TriggerOneWithLight1(0);
                //TriggerOneWithLight1(1);
                //TriggerOneWithLight1(2);
                //TriggerOneWithLight1(3);

                // Do not spin at 100% CPU while the machine is in Manual/Test mode.
                // 1 ms still gives virtually immediate software-trigger response but leaves time
                // for WinForms and VmRenderControl to repaint smoothly.
                if (MachineMode == ModeMachine.Manual) Thread.Sleep(1);
                else Thread.Yield();
            }
        }
        int[] CountDelayErrorVision = new int[3];
        private void ProcessCam(int index)
        {

            if (MachineMode == ModeMachine.Manual)
            {
                ProcessIndex[index] = 0;
                _VisionSystem[0].FinishTrigger = false;
                PLC.Write.AutoBits[PlcDic.FinishTrigger[index]] = false;
                return;
            }

            switch (ProcessIndex[index])
            {
                case 0:
                    {
                        
                        if (PLC.Read.AutoBits[PlcDic.TriggerCam[index]] )
                        {
                            PLC.Write.AutoBits[PlcDic.ResultCam[index]] = false;
                            _VisionSystem[index].FinishTrigger = false;
                            ProcessIndex[index] = 5;
                        }
                        break;
                    }
                case 5:
                    {
                        //TriggerCam
                        CountDelayErrorVision[index] = 0;
                        _VisionSystem[index].TriggerOne = true;
                        ProcessIndex[index] = 10;
                        break;
                    }
                case 10:
                    {
                        //_VisionSystem[index].FinishTrigger
                        if (_VisionSystem[index].FinishTrigger)
                        {
                            PLC.Write.AutoBits[PlcDic.FinishTrigger[index]] = true;
                            _VisionSystem[index].TriggerOne = false;
                            ProcessIndex[index] = 15;
                        }
                        //else
                        //{
                        //    if (CountDelayErrorVision[index]++ > 9999999)
                        //    {
                        //        CountDelayErrorVision[index] = 0;
                        //        ProcessIndex[index] = 11;
                        //    }
                        //}
                        

                        break;
                    }
                case 11:
                    {
                        if (CountDelayErrorVision[index]++ > 999999)
                        {
                            ProcessIndex[index] = 0;
                        }
                        break;
                    }
                case 15:
                    {
                        //Done
                        {
                            if (!PLC.Read.AutoBits[PlcDic.TriggerCam[index]])
                            {
                                _VisionSystem[index].FinishTrigger = false;
                                PLC.Write.AutoBits[PlcDic.FinishTrigger[index]] = false;
                                ProcessIndex[index] = 0;
                            }
                        }
                        break;
                    }

                default: break;
            }
        }
        public int TriggerCamAuto(int cam,int index, ImageInfo cogImage8Grey, bool IsFitImage, bool IsFilmStrip)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            int result = 0;
            if (IsTestNoCam[cam]) return 0;
            //if (!IsCamConnected) return 0;
            if (!_VisionSystem[cam].IsVisionProcessInitial) return 0;
            try
            {
                switch (cam)
                {
                    case 0:
                        {
                            result = VisionRun(cam, index, cogImage8Grey, IsFitImage, IsFilmStrip);
                            break;
                        }
                    case 1:
                        {
                            result = VisionRun(cam, index, cogImage8Grey, IsFitImage, IsFilmStrip);
                            break;
                        }
                    case 2:
                        {
                            result = VisionRun(cam, index, cogImage8Grey, IsFitImage, IsFilmStrip);
                            break;
                        }
                    case 3:
                        {
                            result = VisionRun(cam, index, cogImage8Grey, IsFitImage, IsFilmStrip);
                            break;
                        }
                    default: break;
                }
                int indexExport = 0;
                CreatFilmStripOne(cam, indexExport, result, IsFilmStrip, cogImage8Grey?.Image);
            }
            catch (VM.PlatformSDKCS.VmException ex)
            {
                result = 0;
                //Log
                WriteLogPC(LogType.Vision, $"TriggerAutoCam{cam}, Index{index}", ex.Message);
            }
            catch (Exception ex)
            {
                //Log
                WriteLogPC(LogType.Vision, $"TriggerAutoCam{cam}, Index{index}", ex.Message);
                result = 0;
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            _VisionSystem[cam].TimeVision = $"{ts.Seconds}.{PadLeftZeros(ts.Milliseconds, 3)}";
            return result;
        }
        public int TriggerCamManual(int cam, int index, bool IsFitImage, bool IsFilmStrip)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            int result = 0;
            if (!_VisionSystem[cam].IsVisionProcessInitial) return 0;

            ImageInfo image = null;
            try
            {
                image = GetImageVision(cam);
                if (image == null || image.Image == null) return 0;

                result = VisionRun(cam, index, image, IsFitImage, IsFilmStrip);
                CreatFilmStripOne(cam, 0, result, IsFilmStrip, image.Image);
                _VisionSystem[cam].FinishTrigger = true;
               

            }
            catch (VM.PlatformSDKCS.VmException ex)
            {
                WriteLogPC(LogType.Vision, $"TriggerManualCam{cam}, Index{index}", ex.Message);
            }
            catch (Exception ex)
            {
                WriteLogPC(LogType.Vision, $"TriggerManualCam{cam}, Index{index}", ex.Message);
            }
            finally
            {
                image?.Image?.Dispose();
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            _VisionSystem[cam].TimeVision = $"{ts.Seconds}.{PadLeftZeros(ts.Milliseconds, 3)}";
            return result;
        }

        public ImageInfo GetImageVision(int cam)
        {
            ImageInfo thisImageRun = new ImageInfo { Coordinate = new Coordinate() };

            try
            {
                switch (formOption.Parameters.Cam[cam].ImageSource)
                {
                    case ImageSource.Cam:
                        return thisImageRun;

                    case ImageSource.Folder:
                        if (formOption.FolderImageFiles[cam] == null || formOption.FolderImageFiles[cam].Length == 0) return null;
                        if (formOption.IndexImageFolder[cam] >= formOption.FolderImageFiles[cam].Length) formOption.IndexImageFolder[cam] = 0;

                        string folderFile = formOption.FolderImageFiles[cam][formOption.IndexImageFolder[cam]];
                        thisImageRun.Image = VisionMasterUiHelper.LoadBitmapUnlocked(folderFile);
                        formOption.IndexImageFolder[cam]++;
                        return thisImageRun;

                    case ImageSource.File:
                        string fileName = formOption.FileImagePaths[cam];
                        if (string.IsNullOrWhiteSpace(fileName)) fileName = formOption.Parameters.Cam[cam].ImageFile;
                        thisImageRun.Image = VisionMasterUiHelper.LoadBitmapUnlocked(fileName);
                        return thisImageRun;
                }
            }
            catch (Exception ex)
            {
                WriteLogPC(LogType.Vision, $"GetImageVision Cam{cam}", ex.ToString());
            }

            return thisImageRun.Image == null ? null : thisImageRun;
        }
        private int VisionRun(int cam, int index, ImageInfo imageInput, bool isFitImage)
        {
            return VisionRun(cam, index, imageInput, isFitImage, false);
        }
        private int VisionRun(int cam, int index, ImageInfo imageInput, bool isFitImage, bool isFilmStrip)
        {
            lock (_visionRuntimeSync)
            {
                if (_shutdownRequested) return 0;
                return VisionRunCore(cam, index, imageInput, isFitImage, isFilmStrip);
            }
        }
        private int VisionRunCore(int cam, int index, ImageInfo imageInput, bool isFitImage, bool isFilmStrip)
        {
            int result = 0;

            if (imageInput == null || imageInput.Image == null) return 0;

            try
            {
                if (_VisionSystem[cam].VisionProcesses == null || index < 0 || index >= _VisionSystem[cam].VisionProcesses.Length)
                    throw new Exception($"VisionMaster process index invalid. Cam {cam}, index {index}.");

                VisionMasterProcess process = _VisionSystem[cam].VisionProcesses[index];
                if (process == null)
                    throw new Exception($"_VisionSystem[{cam}].VisionProcesses[{index}] == null");

                if (MachineMode != ModeMachine.Teaching)
                {
                    // Trigger hot path: ONE camera Bitmap -> SetImageData -> Procedure.Run -> result.
                    // Cache full-resolution input/output images only if FilmStrip saving really needs them.
                    bool saveOriginal = isFilmStrip && formOption?.Parameters?.Cam != null && cam < formOption.Parameters.Cam.Length && formOption.Parameters.Cam[cam] != null && (formOption.Parameters.Cam[cam].SaveOrigin || formOption.Parameters.Cam[cam].SaveOriginSeparate);
                    bool saveGraphic = isFilmStrip && formOption?.Parameters?.Cam != null && cam < formOption.Parameters.Cam.Length && formOption.Parameters.Cam[cam] != null && formOption.Parameters.Cam[cam].SaveGraphic;
                    result = process.Run(imageInput.Image, saveOriginal, saveGraphic);

                    WriteLogPC(LogType.Vision, $"Vision{cam},Index{index}", result.ToString());
                    PLC.Write.AutoBits[PlcDic.ResultCam[cam]] = result != 0;

                    //if (!string.IsNullOrWhiteSpace(process.LastError))
                    //    WriteLogPC(LogType.Vision, $"Vision{cam},Index{index}", process.LastError);
                }

                // Do not write a UI + disk log for every successful trigger.
                // In V10 this queued a WinForms ListView update and File.AppendAllText on every shot,
                // which produced the visible stop-go repaint in the video. Errors are still logged.
            }
            catch (VM.PlatformSDKCS.VmException ex)
            {
                result = 0;
                PLC.Write.AutoBits[PlcDic.ResultCam[cam]] = false;
                WriteLogPC(LogType.Vision, $"Vision{cam},Index{index}", ex.Message);
            }
            catch (Exception ex)
            {
                result = 0;
                PLC.Write.AutoBits[PlcDic.ResultCam[cam]] = false;
                WriteLogPC(LogType.Vision, $"Vision{cam},Index{index}", ex.ToString());
            }

            _VisionSystem[cam].ErrorVision = result == 0;
            return result;
        }
        /// <summary>
        /// FilmStrip
        /// </summary>
      
        public string FirstTimeTrigger = "";
        private Bitmap ImageShowFilmStip = null;
        private bool IsGettingResult = false;
        private void CreatFilmStripOne(int cam, int index, int result, bool Enable, Bitmap sourceImage = null)
        {
            // Manual trigger passes Enable=false. The old guard had been commented out, so even a
            // manual click performed directory creation, render capture/JPEG save and synchronous
            // UI Invoke calls. That was one of the main causes of the visible trigger stutter.
            if (!Enable) return;
            if (formMain.CameraVisionDisplays[cam].List[index].partOfCams[0].mDisplay == null) return;
            if (!formOption.Parameters.Cam[cam].SaveGraphic && !formOption.Parameters.Cam[cam].SaveOrigin) return;
            string LotNo = formMain.scannedValue;
            int Floor = cam == 0 ? PLC.Read.WordUshorts[PlcDic.ModelNo] : PLC.Read.WordUshorts[PlcDic.ModelNo];
            string Cassette = cam == 0 ? "A" : "B";
            string[] arrListStr = LotNo.Split(' ');
            string LotNoString = arrListStr[0];
            string FloorString = PadLeftZeros(Floor, 2);

            //if (!Enable) return;
            string Datetimenow = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            string FileName = $"{Datetimenow}";
            string CamNoString = (cam + 1).ToString();
            string thisCamJigNoString = PadLeftZeros(CamJigNo[cam], 6);
            FileName += $"_index{index}_QR_{_VisionSystem[cam].Code}";
            string ResultString = result == 1 ? "OK" : "NG";
            string FolderName = DateTime.Now.ToString("yyyy-MM-dd");
            string subpathOrigin = formOption.Parameters.CommonPath + $"\\Cam{CamNoString}\\Origin\\{FolderName}\\{formModel.modelParameter.ModelName}\\Lot_{formMain.txtLot.Text}\\{ResultString}" ;

            string subpathOriginSeparate = formOption.Parameters.CommonPath + $"\\Cam{CamNoString}\\OriginSeparate\\{FolderName}\\{formModel.modelParameter.ModelName}\\Lot_{formMain.txtLot.Text}\\{ResultString}";

            string subpathGraphic = formOption.Parameters.CommonPath + $"\\Cam{CamNoString}\\Graphic\\{FolderName}\\{formModel.modelParameter.ModelName}\\Lot_{formMain.txtLot.Text}\\{ResultString}" ;

            bool IsExitFolderOrigin = Directory.Exists(subpathOrigin);
            if (!IsExitFolderOrigin) Directory.CreateDirectory(subpathOrigin);

            bool IsExitFolderOriginSeparate = Directory.Exists(subpathOriginSeparate);
            if (!IsExitFolderOriginSeparate) Directory.CreateDirectory(subpathOriginSeparate);

            bool IsExitFolderGraphic = Directory.Exists(subpathGraphic);
            if (!IsExitFolderGraphic) Directory.CreateDirectory(subpathGraphic);
            
            string thisPathFileOrigin = subpathOrigin + "\\" + FileName + ".jpg";

            string thisPathFileOriginSeparate = subpathOriginSeparate + "\\" + FileName + ".jpg";

            string thisPathFileGraphic = subpathGraphic + "\\" + FileName + ".jpg";

            SaveImageGraphic(thisPathFileGraphic, cam, index, sourceImage);
            IsGettingResult = true;
            CamJigNo[cam]++;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    UpdateResultTable();
                    formMain.UpdateControlStatus();
                    IsGettingResult = false;
                }));
            }
            catch
            {
                IsGettingResult = false;
            }
            Bitmap original = sourceImage;
            if (original == null && _VisionSystem[cam].VisionProcesses != null && index < _VisionSystem[cam].VisionProcesses.Length)
                original = _VisionSystem[cam].VisionProcesses[index]?.LastInputImage;

            if (formOption.Parameters.Cam[cam].SaveOrigin && original != null) original.Save(thisPathFileOrigin, ImageFormat.Jpeg);
            if (formOption.Parameters.Cam[cam].SaveOriginSeparate && original != null) original.Save(thisPathFileOriginSeparate, ImageFormat.Jpeg);

        
        }
        private void UpdateResultTable()
        {
           
        }
        private void DeleteOldFolderImage(string CommonPathDic,int cam )
        {
            try
            {
                if(cam == 1)
                {
                    string FolderDate = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd");
                    string subpathOrigin1 = CommonPathDic + "\\Cam1\\Origin\\" + FolderDate;
                    string subpathGraphic1 = CommonPathDic + "\\Cam1\\Graphic\\" + FolderDate;
                    if (Directory.Exists(subpathOrigin1)) Directory.Delete(subpathOrigin1, true);
                    if (Directory.Exists(subpathGraphic1)) Directory.Delete(subpathGraphic1, true);
                }
                else if(cam==2)
                {
                    string FolderDate = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd");
                    string subpathOrigin2 = CommonPathDic + "\\Cam2\\Origin\\" + FolderDate;
                    string subpathGraphic2 = CommonPathDic + "\\Cam2\\Graphic\\" + FolderDate;
                    if (Directory.Exists(subpathOrigin2)) Directory.Delete(subpathOrigin2, true);
                    if (Directory.Exists(subpathGraphic2)) Directory.Delete(subpathGraphic2, true);
                }
               
            }
            catch (Exception) { }

        }

        static Bitmap bmp_img_Display = null;
        private void SaveImageGraphic(string Path, int cam, int index, Bitmap sourceImage = null)
        {
            try
            {
                if (formOption.Parameters.Cam[cam].SaveGraphic)
                {
                    if (File.Exists(Path)) throw new Exception($"{Path}, Graphic Image file already Exists.");
                    Bitmap fallback = _VisionSystem[cam].VisionProcesses != null && index < _VisionSystem[cam].VisionProcesses.Length
                        ? _VisionSystem[cam].VisionProcesses[index]?.LastOutputImage
                        : null;
                    if (fallback == null) fallback = sourceImage;

                    bmp_img_Display = VisionMasterUiHelper.SaveRenderedImage(formMain.CameraVisionDisplays[cam].List[index].partOfCams[0].mDisplay, Path, fallback);
                    if (bmp_img_Display == null) throw new Exception("VisionMaster display image is null.");

                    ImageShowFilmStip?.Dispose();
                    ImageShowFilmStip = new Bitmap(bmp_img_Display);
                    bmp_img_Display.Dispose();
                    bmp_img_Display = null;
                }
            }
            catch (AccessViolationException ex)
            {
                //Log
                WriteLogPC(LogType.Vision, $"SaveImageGraphic", ex.Message);
            }
            catch (Exception ex)
            {
                //Log
                WriteLogPC(LogType.Vision, $"SaveImageGraphic", ex.Message);
            }
        }
        private string PadLeftZeros(int Data, int Count)
        {
            string text = "0000000000" + Math.Abs(Data);
            if (Data < 0)
            {
                return "-" + text.Substring(text.Length - Count + 1);
            }
            string result = text.Substring(text.Length - Count);
            return result;
        }
        /// <summary>
        /// Scanner
        /// </summary>
        public void ScannerConnect()
        {
            try
            {
                if (!Matrix220.IsOpen)
                {
                    Matrix220.PortName = formOption.Parameters.Scanner.PortName;
                    Matrix220.BaudRate = formOption.Parameters.Scanner.BaudRate;
                    Matrix220.DataBits = formOption.Parameters.Scanner.DataBits;
                    Parity sParity = (Parity)Enum.Parse(typeof(Parity), formOption.Parameters.Scanner.Parity, true);
                    StopBits sStopBits = (StopBits)Enum.Parse(typeof(StopBits), formOption.Parameters.Scanner.StopBits, true);
                    Matrix220.Parity = sParity;
                    Matrix220.StopBits = sStopBits;
                    Matrix220.Open();
                    lbScannerConected.BackColor = Matrix220.IsOpen ? Color.Green : Color.IndianRed;
                }
            }
            catch (Exception ex)
            {
                //LOG
                WriteLogPC(LogType.Main, "Scanner", "Connect Fail");
                MessageBox.Show(ex.ToString(), "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        public void ScannerDisconnect()
        {
            if (Matrix220.IsOpen)
            {
                Matrix220.Close();
            }
            lbScannerConected.BackColor = Matrix220.IsOpen ? Color.Green : Color.IndianRed;
        }
        private void Scanner_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string BarcodeReceived = Matrix220.ReadLine();

            if (BarcodeReceived.Contains("\r"))
            {
                BarcodeReceived = BarcodeReceived.Replace("\r", "");
            }
            if (BarcodeReceived.Contains("\n"))
            {
                BarcodeReceived = BarcodeReceived.Replace("\n", "");
            }
            _VisionSystem[0].Code = BarcodeReceived;

            Invoke(new EventHandler(DisplayBarcodeRecived));
           
                
        }
        private void DisplayBarcodeRecived(object s, EventArgs e)
        {

            //LOG
            WriteLogPC(LogType.Main, "Scanner", $"Code: {_VisionSystem[0].Code}");
        }

        ///<summary>
        /// Print - Laser marking
        ///</summary>
        public TcpListener Printerlistener;
        private TcpClient Printerclient;
        private NetworkStream Printerstream;
        private readonly int Printerport = 1000;
        public event Action<string> DataReceived;
        public int IndexPatternAuto = 0;
        public int RankIndex = 0;
        public void InitialPrinter()
        {
            Printerlistener = new TcpListener(IPAddress.Any, Printerport);
            Printerlistener.Start();
            WriteLogPC(LogType.Main,"Printer","Server started on port " + Printerport);
            Printerlistener.BeginAcceptTcpClient(new AsyncCallback(PrinterOnClientConnected), null);
        }
        public void PrinterOnClientConnected(IAsyncResult ar)
        {
            Printerclient = Printerlistener.EndAcceptTcpClient(ar);
            Printerstream = Printerclient.GetStream();
            WriteLogPC(LogType.Main, "Printer", "Client Connected");

            Thread thread = new Thread(PrinterReceiveData);
            thread.IsBackground = true;
            thread.Start();
            lblPrinterConnection.BackColor = Color.Green;
        }
        private void PrinterReceiveData()
        {
            byte[] buffer = new byte[1024];
            while (Printerclient.Connected)
            {
                try
                {
                    int bytesRead = Printerstream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string receivedText = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    //LOG
                    this.Invoke(new Action(() =>
                    {
                        WriteLogPC(LogType.Main, "Priter", $"Recived: {receivedText}");
                    }));

                    Printer_HandleDataReceived(receivedText);
                    DataReceived?.Invoke(receivedText); // Gọi sự kiện

                }
                catch (Exception ex)
                {
                    WriteLogPC(LogType.Main, "Printer", "Receive error: " + ex.Message);
                    break;
                }
            }
            WriteLogPC(LogType.Main, "Printer", "Client disconnected");
            Printerstream.Close();
            Printerclient.Close();
        }
        public void PrinterSendData(string message)
        {
            if (Printerclient != null && Printerclient.Connected)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                Printerstream.Write(data, 0, data.Length);

                //LOG
                this.Invoke(new Action(() => {
                    WriteLogPC(LogType.Main, "Printer", $"Sent: {message}");
                }));

            }
            else
            {
                //LOG
                this.Invoke(new Action(() => {
                    WriteLogPC(LogType.Main, "Printer", $"Sent: No client connected.");
                }));

            }
        }
        private void Printer_HandleDataReceived(string PrinterReceived)
        {
            string SetPrintType;
            string SetDirection;
            string SetTextPosition;
            string CurrentSerialNo;
            string CurrentYMMDDN;
            string[] ArrayPrintRecived;
            string ThisPrintType;
            try
            {

                if (true)
                {
                    if (PrinterReceived == null || PrinterReceived == "")
                        throw new Exception("Print Data Recived Empty");
                    if (IndexPatternAuto >= formModel.laserSettingLayout.markings.Count)
                    {
                        PrinterSendData(" ");
                        return;
                    }
                    //if (CurrentTimeMarking >= formModel.modelParameter.patterns[IndexPatternAuto].MarkingTime)
                    //{
                    //    DetectFinishAPatternPrint = true;
                    //}

                    string ThisDirection = "";
                    string ThisPosition = "";

                    ArrayPrintRecived = PrinterReceived.Split(' ');
                    ThisPrintType = ArrayPrintRecived[0];
                    // ThisPrintType : CODE OR TEXT
                    if (ArrayPrintRecived.Length >= 2) ThisDirection = ArrayPrintRecived[1];
                    // ThisDirection : 0 OR 90
                    if (ArrayPrintRecived.Length >= 3) ThisPosition = ArrayPrintRecived[2];
                    // This Position : BOTTOM, RIGHT, LEFT, TOP
                    SetPrintType = formModel.laserSettingLayout.markings[IndexPatternAuto].PrintType;
                    //SET PrintType : Code, Text1, Text 2, Text 3, Code Text 1, Code Text 2, Code Text 3
                    SetDirection = formModel.laserSettingLayout.markings[IndexPatternAuto].Rotate;
                    // Set Direction : 0 or 90
                    SetTextPosition = formModel.laserSettingLayout.markings[IndexPatternAuto].Type;
                    // set text position : Top, Bottom, Left, Right

                    CurrentSerialNo = formModel.laserSettingLayout.markings[IndexPatternAuto].Content;
                    CurrentYMMDDN = formMain.txtYMMDDN.Text;

                    // Chỗ này cần cập nhật Rank theo bảng Rank mới.
                    // Lấy dữ liệu rank
                    string CurrentRank = formMain.txtRank.Text;
                    if (formOption.Parameters.MES.MesEnabel)
                    {
                        if (MesDataInfomation.Rank.Length != 0)
                        {
                            if (formMain.dgvRank.Rows.Count > 0)
                            {
                                if (RankIndex < formMain.dgvRank.Rows.Count - 1)
                                {
                                    CurrentRank = formMain.dgvRank.Rows[RankIndex].Cells[0].Value.ToString();
                                }
                            }
                        }
                    }
                    if (CurrentSerialNo == "" || CurrentYMMDDN == "")
                        throw new Exception("Serial No or YMMDDN is Empty");
                    string DataEmpty = " ";
                    string DataCodeEmpty = " ";
                    string CodeSend = CurrentYMMDDN + CurrentSerialNo;

                    string TextSend = CurrentSerialNo + Environment.NewLine + CurrentYMMDDN + Environment.NewLine + CurrentRank;
                    // Đoạn này sẽ kiểm tra xem là có phải dạng Text1, text 2, text 3 hay k
                    //if (SetPrintType.Contains(PrintOption.PrintType.Text1))
                    //{
                    //    if (formOption.Parameters.Option.BarcodeRankPrint)
                    //    {
                    //        TextSend = CurrentRank;
                    //    }
                    //    else
                    //    {
                    //        TextSend = CurrentSerialNo;
                    //    }
                    //}
                    //else
                    //{
                    //    if (SetPrintType.Contains(PrintOption.PrintType.Text2))
                    //    {
                    //        TextSend = CurrentYMMDDN + Environment.NewLine + CurrentRank;
                    //    }
                    //    else
                    //    {
                    //        if (SetPrintType.Contains(PrintOption.PrintType.Text3))
                    //        {
                    //            TextSend = CurrentSerialNo + Environment.NewLine + CurrentYMMDDN + Environment.NewLine + CurrentRank;
                    //        }
                    //    }
                    //}

                    // xử lý khi trong quá trình Auto
                    if (true)
                    {
                        // trên phần mề EzCard setting Code
                        if (ThisPrintType == "CODE")
                        {
                            if (SetPrintType==ThisPrintType)
                            {
                                
                                    //Gửi xuống máy in
                                    PrinterSendData(CodeSend);

                                    //if (PLC.Read.Auto.Auto)
                                    //{
                                    //    //LOG
                                    //    this.Invoke(new Action(() =>
                                    //    {
                                    //        UpdatePrintScanDataTable(SttUpdatePrintDatatable, CodeSend, null, false);
                                    //    }));

                                    //    SttUpdatePrintDatatable++;
                                    //}
                                    //PLC.Write.Auto.restart_triger = true;
                                    //nếu không chứa Text thì mới tăng lên 1
                                    //if (SetPrintType == "Code")
                                    //{
                                    //    // Tăng Serial lên
                                    //    IncreaseOneSerialNo();
                                    //}
                            }
                            else PrinterSendData(DataCodeEmpty);
                        }
                        else
                        {
                            if (SetPrintType.Contains("Text"))
                            {
                                if (ThisPrintType == SetPrintType&& ThisDirection == SetDirection && ThisPosition == SetTextPosition)
                                {
                                    PrinterSendData(TextSend);
                                    //CurrentTimeMarking++;
                                    //PLC.Write.Auto.restart_triger = true;
                                    //// Tăng Serial lên
                                    //IncreaseOneSerialNo();

                                }
                            }
                            else PrinterSendData(DataEmpty);


                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //LOG
                this.Invoke(new Action(() =>
                {
                    WriteLogPC(LogType.Main, "Printer_Handle", ex.Message);
                }));
            }
        }
        public void PrinterStop()
        {
            Printerstream?.Close();
            Printerclient?.Close();
            Printerlistener?.Stop();
            WriteLogPC(LogType.Main, "Printer", $"Server stopped.");
            
        }
        private void IncreaseOneSerialNo()
        {
            //LOG
            this.Invoke(new Action(() => {
                int CurrentSerialNo_int = Convert.ToInt32(formMain.txtSerialNo.Text);
                string NewSerialNo = PadLeftZeros(CurrentSerialNo_int + 1, 6);
                formMain.txtSerialNo.Text = NewSerialNo;
                formMain.txt1STbarcode.Text = formMain.txtYMMDDN.Text + formMain.txtSerialNo.Text;


                //Reset Rank Index
                if (formOption.Parameters.MES.MesEnabel)
                {
                    InitialDGVRank();
                    if (MesDataInfomation.Rank != null)
                    {
                        RankIndex++;
                        if (RankIndex >= MesDataInfomation.Rank.Length)
                        {
                            RankIndex = 0;
                        }
                    }
                    else
                    {
                        RankIndex = 0;
                    }
                }
                else
                {
                    RankIndex++;
                    if (RankIndex >= 2) RankIndex = 0;
                }


            }));

        }
        private void InitialDGVRank()
        {

            for (int i = 0; i < formMain.dgvRank.Rows.Count; i++)
            {
                formMain.dgvRank.Rows[i].DefaultCellStyle.BackColor = Color.White;
            }
            formMain.dgvRank.Rows[RankIndex].DefaultCellStyle.BackColor = Color.Green;
            formMain.lbRankIndex.Text = $"Rank {RankIndex}";
            if (formOption.Parameters.MES.MesEnabel)
            {
                formMain.txtRank.Text = formMain.dgvRank.Rows[RankIndex].Cells[0].Value.ToString();
                
               
            }

        }
        /// <summary>
        /// Mes
        /// </summary>
        public MESComunication MES = new MESComunication();
        public DataInfomationSend MesDataInfomation = new DataInfomationSend();
        public void InitialMes()
        {
            MES = new MESComunication();
            MES.TransferLog += new MESComunication.TransferLogData(MES_TransferLog);
            MES.ProtocolException += new MESComunication.ProtocolExceptionEventHandler(MES_PrEx);
        }
        private void MES_TransferLog(object sender, string Type, string Data)
        {
            //Log
            this.Invoke(new Action(() =>
            {
                WriteLogPC(LogType.Mes, Type, Data);
            }));
        }
        private void MES_PrEx(object sender, ExceptionCode exCode, string information)
        {
            //Log
            this.Invoke(new Action(() =>
            {
                WriteLogPC(LogType.Mes, exCode.ToString(), information);
            }));
        }
        /// <summary>
        /// Alarm - Log - Error List
        /// </summary>
        private readonly object _logFileLock = new object();

        private void WriteLogError(string logType, string errorCode, string errorContent)
        {
            if (formOption.Parameters == null || formOption.Parameters.CommonPath == null) return;

            try
            {
                string dayTime = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss");
                string dateFolder = DateTime.Now.ToString("MM-yyyy");
                string dateFile = DateTime.Now.ToString("MM-dd-yyyy");
                string path = Path.Combine(formOption.Parameters.CommonPath, logType + "Log", dateFolder);
                string fileName = Path.Combine(path, $"{logType}Log_{dateFile}.csv");

                Directory.CreateDirectory(path);
                errorCode = (errorCode ?? "").Replace("\r", " ").Replace("\n", " ");
                errorContent = (errorContent ?? "").Replace("\r", " ").Replace("\n", " ");

                lock (_logFileLock)
                {
                    if (!File.Exists(fileName)) File.WriteAllText(fileName, "STT,DayTime,Code,Content" + Environment.NewLine, Encoding.UTF8);
                    string line = $"0,{dayTime},{errorCode},{errorContent}{Environment.NewLine}";
                    File.AppendAllText(fileName, line, Encoding.UTF8);
                }
            }
            catch (IOException ex) { Console.WriteLine(ex); }
            catch (Exception ex) { Console.WriteLine(ex); }
        }

        public void WriteLogPC(LogType logType, string code, string content)
        {
            string dateTimeNow = DateTime.Now.ToString("HH:mm:ss");
            string fileGroup = logType == LogType.Main ? "MAIN" : logType == LogType.Mes ? "MES" : "Vision";

            // File IO is never executed on the WinForms thread. Slow SSD/network/antivirus activity
            // therefore cannot freeze VmRenderControl repaint during a trigger.
            ThreadPool.QueueUserWorkItem(_ => WriteLogError(fileGroup, code, content));

            if (formMain == null || formMain.IsDisposed || !formMain.IsHandleCreated) return;
            Action updateUi = () =>
            {
                if (formMain.IsDisposed) return;
                ListView list = logType == LogType.Main ? formMain.listViewMainLog : formMain.listViewMesLog;
                ListViewItem item = new ListViewItem(dateTimeNow);
                item.SubItems.Add(code);
                item.SubItems.Add(content);
                list.Items.Add(item);
                if (list.Items.Count > 20) list.Items.RemoveAt(0);
                if (list.Items.Count > 0) list.Items[list.Items.Count - 1].EnsureVisible();
            };

            try
            {
                if (formMain.InvokeRequired) formMain.BeginInvoke(updateUi);
                else updateUi();
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }
        private string[,] ReadErrorListContent(int index)
        {
            string[,] result = new string[2, 300];
            try
            {
                string PathFileParameter = $"D:\\InitialData\\ErrorListPLC{index}.csv";
                if (!File.Exists(PathFileParameter)) return null;
                string[] lines = File.ReadAllLines(PathFileParameter, Encoding.UTF8);
                if (lines.Length == 0) return null;
                ErrorListCount[index] = lines.Length;
                result = new string[2, lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    result[0, i] = lines[i].Split(',')[0];
                    result[1, i] = lines[i].Split(',')[1];
                }
                IsReadErrorList[index] = true;
            }
            catch (IOException ex)
            {
                result = null;
                MessageBox.Show(ex.ToString(), "Read Error List", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return result;
        }
        
        private void UpdateErrorListPLC1()
        {
            if (!IsReadErrorList[0]) return;

            bool IsErrorPLCTrigger = false;
            bool IsErrorEmpty = true;
            for (int i = 0; i < 14; i++)
            {
                if (PLC.Read.ErrorWord[i] != 0)
                {
                    IsErrorEmpty = false;
                }
                if (CurrentErrorWord1[i] != PLC.Read.ErrorWord[i])
                {
                    IsErrorPLCTrigger = true;
                }
            }
            if (IsErrorPLCTrigger && !IsErrorEmpty)
            {
                for (int i = 0; i < ErrorListCount[0]; i++)
                {
                    if (PLC.Read.ErrorBits[i])
                    {
                        //LOG
                        WriteLogPC(LogType.Main, ErrorListContent1[0, i], ErrorListContent1[1, i]);
                    }
                }
                for (int i = 0; i < 14; i++)
                {
                    CurrentErrorWord1[i] = PLC.Read.ErrorWord[i];
                }
            }
        }

        /// <summary>
        /// vissionData
        /// </summary>
     
      
       
        #region Variables
        //Các biến test máy
        public static bool IsTestNoPLC = true;
        public static bool[] IsTestNoCam = new bool[4];
        public static bool IsTestNoPrint = true;
        public static bool IsTestNoScanner = false;
        public static bool IsTestNoMes = true;
        private string filePath;
        // Các form
        public Main formMain;
        public Model formModel;
        public FormIO formIO;
        public FormOption formOption;
        public Login FormLogin;
        bool Language = false;
        private static Color OKColor = Color.FromArgb(57, 196, 129);
        private static Color NGColor = Color.FromArgb(255, 130, 130);
        //Các thiết bị
        //PLC
        public PLCCommunication PLC = new PLCCommunication();
        private IPlc[] PLCDevice = new IPlc[2];

        public static int threadSleepTime = 5;
        private static int NumberWritePLC = 2;
        private static int NumberWriteWPLC = 30;//Max == 123
        private static ushort[] DataWritePLC;
        private static ushort[] DataWriteWPLC;
        private static ushort[] DataArray2;
        private Stopwatch StopwatchReadPLC = new Stopwatch();
        private Stopwatch StopwatchWritePLC = new Stopwatch();

        //SCanner
        public SerialPort Matrix220 = new SerialPort();
        private string TimeReadPLC;
        private string TimeWritePLC;
        private bool interval = false;
        public string CurrentLotNo = "";

        public enum LogType
        {
            Main,
            Mes,
            PLC,
            Vision
        }

        //Các biến khác
        private Thread trd;
        private Thread threadCam1;
        private readonly object _visionRuntimeSync = new object();
        private volatile bool _shutdownRequested = false;
        private bool _closingConfirmed = false;
        public WordConvert _WordConvert = new WordConvert();
        public int IndexFormShowing = 0;
        //Sản Lượng
        //Camera
        public int NumberOfCamera = 1;
        public int[] ProcessIndex = new int[3];
        public string ProcessStepName = "";
        int ProcessIndex2;
        public string ProcessStepName2 = "";
        public bool IsDualProduct = false;
        public class TrayCellData
        {
            public bool? ResultA { get; set; }
            public bool? ResultB { get; set; }
        }

        public enum ResultVision
        {
            Wait,
            Error,
            NG,
            OK
        }
        public class ImageInfo
        {
            public Bitmap Image { get; set; }
            public Coordinate Coordinate { get; set; }
        }
        public VisionSystem[] _VisionSystem = new VisionSystem[3];
        public int ModelNoInternal = 999;
        //AutoMode
        public enum ModeMachine
        {
            Manual,
            Teaching,
            Auto
        }
        private List<Panel> PanelCamStatus = new List<Panel>();
        public ModeMachine MachineMode = ModeMachine.Manual;
        private int CountCheckPLC_On = 0;
        public bool[] IsCamRunContinous = { false, false, false, false };
        public bool[] IsLiveCam = { false, false, false, false };
        public int[] CamJigNo;
        public int[] CamPartNo;
        //Giao diện
        private int[] ErrorListCount = new int[2];
        private bool[] IsReadErrorList = new bool[2];
        private string[,] ErrorListContent1 = new string[2, 300];
        private string[,] ErrorListContent2 = new string[2, 300];
        public int[] CurrentErrorWord1 = new int[14];
        public int[] CurrentErrorWord2 = new int[14];
        Color TabControlBackColorON = Color.FromArgb(128, 128, 255);
        Color TabControlBackColorOFF = Color.FromArgb(45, 45, 55);
        private int CountStartThread = 0;
        private int CountIntervalTimer1 = 0;
        #endregion Variables
    }
}
