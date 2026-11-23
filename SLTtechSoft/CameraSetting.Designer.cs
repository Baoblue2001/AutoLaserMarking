namespace SLTtechSoft
{
    partial class CameraSetting
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.rdHardwareManualTrigger = new SLTSoft.RJControl.RJRadio();
            this.rdHardwareAutoTrigger = new SLTSoft.RJControl.RJRadio();
            this.rdSoftwareTrigger = new SLTSoft.RJControl.RJRadio();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnConnectCam = new System.Windows.Forms.Button();
            this.RefreshFileImage = new System.Windows.Forms.Button();
            this.RefreshFolderImage = new System.Windows.Forms.Button();
            this.FileDirector = new System.Windows.Forms.TextBox();
            this.FolderDirector = new System.Windows.Forms.TextBox();
            this.ModeFile = new SLTSoft.RJControl.RJRadio();
            this.ModeFolder = new SLTSoft.RJControl.RJRadio();
            this.ModeCam = new SLTSoft.RJControl.RJRadio();
            this.label29 = new System.Windows.Forms.Label();
            this.IPAddress = new System.Windows.Forms.TextBox();
            this.Use = new System.Windows.Forms.CheckBox();
            this.SaveGraphic = new System.Windows.Forms.CheckBox();
            this.SaveOrigin = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.SaveOriginSeparate = new System.Windows.Forms.CheckBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rdUSB = new SLTSoft.RJControl.RJRadio();
            this.rdGige = new SLTSoft.RJControl.RJRadio();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rdBasler = new SLTSoft.RJControl.RJRadio();
            this.rdHIK = new SLTSoft.RJControl.RJRadio();
            this.rdVisionPro = new SLTSoft.RJControl.RJRadio();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ReverseX = new System.Windows.Forms.CheckBox();
            this.ReverseY = new System.Windows.Forms.CheckBox();
            this.groupBox6.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel6.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.rdHardwareManualTrigger);
            this.groupBox6.Controls.Add(this.rdHardwareAutoTrigger);
            this.groupBox6.Controls.Add(this.rdSoftwareTrigger);
            this.groupBox6.Location = new System.Drawing.Point(168, 10);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox6.Size = new System.Drawing.Size(152, 134);
            this.groupBox6.TabIndex = 24;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Trigger Mode";
            // 
            // rdHardwareManualTrigger
            // 
            this.rdHardwareManualTrigger.AutoSize = true;
            this.rdHardwareManualTrigger.BackColor = System.Drawing.Color.White;
            this.rdHardwareManualTrigger.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdHardwareManualTrigger.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdHardwareManualTrigger.Location = new System.Drawing.Point(7, 93);
            this.rdHardwareManualTrigger.Margin = new System.Windows.Forms.Padding(4);
            this.rdHardwareManualTrigger.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdHardwareManualTrigger.Name = "rdHardwareManualTrigger";
            this.rdHardwareManualTrigger.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdHardwareManualTrigger.Size = new System.Drawing.Size(148, 27);
            this.rdHardwareManualTrigger.TabIndex = 8;
            this.rdHardwareManualTrigger.Text = "Hardware Manual";
            this.rdHardwareManualTrigger.UnCheckedColor = System.Drawing.Color.Black;
            this.rdHardwareManualTrigger.UseVisualStyleBackColor = false;
            // 
            // rdHardwareAutoTrigger
            // 
            this.rdHardwareAutoTrigger.AutoSize = true;
            this.rdHardwareAutoTrigger.BackColor = System.Drawing.Color.White;
            this.rdHardwareAutoTrigger.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdHardwareAutoTrigger.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdHardwareAutoTrigger.Location = new System.Drawing.Point(7, 58);
            this.rdHardwareAutoTrigger.Margin = new System.Windows.Forms.Padding(4);
            this.rdHardwareAutoTrigger.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdHardwareAutoTrigger.Name = "rdHardwareAutoTrigger";
            this.rdHardwareAutoTrigger.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdHardwareAutoTrigger.Size = new System.Drawing.Size(132, 27);
            this.rdHardwareAutoTrigger.TabIndex = 7;
            this.rdHardwareAutoTrigger.Text = "Hardware Auto";
            this.rdHardwareAutoTrigger.UnCheckedColor = System.Drawing.Color.Black;
            this.rdHardwareAutoTrigger.UseVisualStyleBackColor = false;
            // 
            // rdSoftwareTrigger
            // 
            this.rdSoftwareTrigger.AutoSize = true;
            this.rdSoftwareTrigger.BackColor = System.Drawing.Color.White;
            this.rdSoftwareTrigger.Checked = true;
            this.rdSoftwareTrigger.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdSoftwareTrigger.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdSoftwareTrigger.Location = new System.Drawing.Point(7, 24);
            this.rdSoftwareTrigger.Margin = new System.Windows.Forms.Padding(4);
            this.rdSoftwareTrigger.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdSoftwareTrigger.Name = "rdSoftwareTrigger";
            this.rdSoftwareTrigger.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdSoftwareTrigger.Size = new System.Drawing.Size(92, 27);
            this.rdSoftwareTrigger.TabIndex = 6;
            this.rdSoftwareTrigger.TabStop = true;
            this.rdSoftwareTrigger.Text = "Software";
            this.rdSoftwareTrigger.UnCheckedColor = System.Drawing.Color.Black;
            this.rdSoftwareTrigger.UseVisualStyleBackColor = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnConnectCam);
            this.groupBox1.Controls.Add(this.RefreshFileImage);
            this.groupBox1.Controls.Add(this.RefreshFolderImage);
            this.groupBox1.Controls.Add(this.FileDirector);
            this.groupBox1.Controls.Add(this.FolderDirector);
            this.groupBox1.Controls.Add(this.ModeFile);
            this.groupBox1.Controls.Add(this.ModeFolder);
            this.groupBox1.Controls.Add(this.ModeCam);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox1.Size = new System.Drawing.Size(642, 172);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mode";
            // 
            // btnConnectCam
            // 
            this.btnConnectCam.Location = new System.Drawing.Point(518, 35);
            this.btnConnectCam.Name = "btnConnectCam";
            this.btnConnectCam.Size = new System.Drawing.Size(114, 39);
            this.btnConnectCam.TabIndex = 42;
            this.btnConnectCam.Text = "Connect";
            this.btnConnectCam.UseVisualStyleBackColor = true;
            // 
            // RefreshFileImage
            // 
            this.RefreshFileImage.Location = new System.Drawing.Point(518, 128);
            this.RefreshFileImage.Name = "RefreshFileImage";
            this.RefreshFileImage.Size = new System.Drawing.Size(114, 39);
            this.RefreshFileImage.TabIndex = 41;
            this.RefreshFileImage.Text = "Refresh";
            this.RefreshFileImage.UseVisualStyleBackColor = true;
            // 
            // RefreshFolderImage
            // 
            this.RefreshFolderImage.Location = new System.Drawing.Point(518, 83);
            this.RefreshFolderImage.Name = "RefreshFolderImage";
            this.RefreshFolderImage.Size = new System.Drawing.Size(114, 39);
            this.RefreshFolderImage.TabIndex = 40;
            this.RefreshFolderImage.Text = "Refresh";
            this.RefreshFolderImage.UseVisualStyleBackColor = true;
            // 
            // FileDirector
            // 
            this.FileDirector.BackColor = System.Drawing.Color.White;
            this.FileDirector.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileDirector.Location = new System.Drawing.Point(135, 131);
            this.FileDirector.Margin = new System.Windows.Forms.Padding(4);
            this.FileDirector.Name = "FileDirector";
            this.FileDirector.Size = new System.Drawing.Size(376, 20);
            this.FileDirector.TabIndex = 39;
            this.FileDirector.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // FolderDirector
            // 
            this.FolderDirector.BackColor = System.Drawing.Color.White;
            this.FolderDirector.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FolderDirector.Location = new System.Drawing.Point(135, 86);
            this.FolderDirector.Margin = new System.Windows.Forms.Padding(4);
            this.FolderDirector.Name = "FolderDirector";
            this.FolderDirector.Size = new System.Drawing.Size(376, 20);
            this.FolderDirector.TabIndex = 38;
            this.FolderDirector.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // ModeFile
            // 
            this.ModeFile.AutoSize = true;
            this.ModeFile.BackColor = System.Drawing.Color.White;
            this.ModeFile.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.ModeFile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ModeFile.Location = new System.Drawing.Point(12, 126);
            this.ModeFile.Margin = new System.Windows.Forms.Padding(4);
            this.ModeFile.MinimumSize = new System.Drawing.Size(0, 27);
            this.ModeFile.Name = "ModeFile";
            this.ModeFile.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.ModeFile.Size = new System.Drawing.Size(97, 27);
            this.ModeFile.TabIndex = 6;
            this.ModeFile.Text = "ImageFile";
            this.ModeFile.UnCheckedColor = System.Drawing.Color.Black;
            this.ModeFile.UseVisualStyleBackColor = false;
            // 
            // ModeFolder
            // 
            this.ModeFolder.AutoSize = true;
            this.ModeFolder.BackColor = System.Drawing.Color.White;
            this.ModeFolder.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.ModeFolder.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ModeFolder.Location = new System.Drawing.Point(12, 81);
            this.ModeFolder.Margin = new System.Windows.Forms.Padding(4);
            this.ModeFolder.MinimumSize = new System.Drawing.Size(0, 27);
            this.ModeFolder.Name = "ModeFolder";
            this.ModeFolder.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.ModeFolder.Size = new System.Drawing.Size(115, 27);
            this.ModeFolder.TabIndex = 5;
            this.ModeFolder.Text = "ImageFolder";
            this.ModeFolder.UnCheckedColor = System.Drawing.Color.Black;
            this.ModeFolder.UseVisualStyleBackColor = false;
            // 
            // ModeCam
            // 
            this.ModeCam.AutoSize = true;
            this.ModeCam.BackColor = System.Drawing.Color.White;
            this.ModeCam.Checked = true;
            this.ModeCam.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.ModeCam.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ModeCam.Location = new System.Drawing.Point(12, 35);
            this.ModeCam.Margin = new System.Windows.Forms.Padding(4);
            this.ModeCam.MinimumSize = new System.Drawing.Size(0, 27);
            this.ModeCam.Name = "ModeCam";
            this.ModeCam.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.ModeCam.Size = new System.Drawing.Size(86, 27);
            this.ModeCam.TabIndex = 4;
            this.ModeCam.TabStop = true;
            this.ModeCam.Text = "Camera";
            this.ModeCam.UnCheckedColor = System.Drawing.Color.Black;
            this.ModeCam.UseVisualStyleBackColor = false;
            // 
            // label29
            // 
            this.label29.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label29.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label29.Location = new System.Drawing.Point(11, 57);
            this.label29.Margin = new System.Windows.Forms.Padding(4);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(191, 24);
            this.label29.TabIndex = 18;
            this.label29.Text = "Addess";
            this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // IPAddress
            // 
            this.IPAddress.Location = new System.Drawing.Point(216, 56);
            this.IPAddress.Margin = new System.Windows.Forms.Padding(4);
            this.IPAddress.Name = "IPAddress";
            this.IPAddress.Size = new System.Drawing.Size(284, 25);
            this.IPAddress.TabIndex = 19;
            this.IPAddress.Text = "192.168.1.10";
            this.IPAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Use
            // 
            this.Use.AutoSize = true;
            this.Use.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Use.Location = new System.Drawing.Point(15, 10);
            this.Use.Margin = new System.Windows.Forms.Padding(4);
            this.Use.Name = "Use";
            this.Use.Size = new System.Drawing.Size(51, 23);
            this.Use.TabIndex = 20;
            this.Use.Text = "Use";
            this.Use.UseVisualStyleBackColor = true;
            // 
            // SaveGraphic
            // 
            this.SaveGraphic.AutoSize = true;
            this.SaveGraphic.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SaveGraphic.Location = new System.Drawing.Point(86, 10);
            this.SaveGraphic.Margin = new System.Windows.Forms.Padding(4);
            this.SaveGraphic.Name = "SaveGraphic";
            this.SaveGraphic.Size = new System.Drawing.Size(107, 23);
            this.SaveGraphic.TabIndex = 21;
            this.SaveGraphic.Text = "Save Graphic";
            this.SaveGraphic.UseVisualStyleBackColor = true;
            // 
            // SaveOrigin
            // 
            this.SaveOrigin.AutoSize = true;
            this.SaveOrigin.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SaveOrigin.Location = new System.Drawing.Point(201, 10);
            this.SaveOrigin.Margin = new System.Windows.Forms.Padding(4);
            this.SaveOrigin.Name = "SaveOrigin";
            this.SaveOrigin.Size = new System.Drawing.Size(98, 23);
            this.SaveOrigin.TabIndex = 22;
            this.SaveOrigin.Text = "Save Origin";
            this.SaveOrigin.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panel6);
            this.panel3.Location = new System.Drawing.Point(2, 2);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(642, 97);
            this.panel3.TabIndex = 0;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.label1);
            this.panel6.Controls.Add(this.tbName);
            this.panel6.Controls.Add(this.label29);
            this.panel6.Controls.Add(this.IPAddress);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Margin = new System.Windows.Forms.Padding(2);
            this.panel6.Name = "panel6";
            this.panel6.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
            this.panel6.Size = new System.Drawing.Size(642, 97);
            this.panel6.TabIndex = 20;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(11, 15);
            this.label1.Margin = new System.Windows.Forms.Padding(4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 24);
            this.label1.TabIndex = 20;
            this.label1.Text = "Name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(216, 14);
            this.tbName.Margin = new System.Windows.Forms.Padding(4);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(284, 25);
            this.tbName.TabIndex = 21;
            this.tbName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Controls.Add(this.panel3);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.Controls.Add(this.panel4);
            this.flowLayoutPanel1.Controls.Add(this.panel5);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 2);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(657, 493);
            this.flowLayoutPanel1.TabIndex = 27;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.SaveOriginSeparate);
            this.panel2.Controls.Add(this.SaveOrigin);
            this.panel2.Controls.Add(this.Use);
            this.panel2.Controls.Add(this.SaveGraphic);
            this.panel2.Location = new System.Drawing.Point(2, 103);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(642, 42);
            this.panel2.TabIndex = 1;
            // 
            // SaveOriginSeparate
            // 
            this.SaveOriginSeparate.AutoSize = true;
            this.SaveOriginSeparate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SaveOriginSeparate.Location = new System.Drawing.Point(307, 10);
            this.SaveOriginSeparate.Margin = new System.Windows.Forms.Padding(4);
            this.SaveOriginSeparate.Name = "SaveOriginSeparate";
            this.SaveOriginSeparate.Size = new System.Drawing.Size(155, 23);
            this.SaveOriginSeparate.TabIndex = 23;
            this.SaveOriginSeparate.Text = "Save Origin Separate";
            this.SaveOriginSeparate.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.groupBox1);
            this.panel4.Location = new System.Drawing.Point(2, 149);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(642, 172);
            this.panel4.TabIndex = 2;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.groupBox4);
            this.panel5.Controls.Add(this.groupBox3);
            this.panel5.Controls.Add(this.groupBox6);
            this.panel5.Controls.Add(this.groupBox2);
            this.panel5.Location = new System.Drawing.Point(2, 325);
            this.panel5.Margin = new System.Windows.Forms.Padding(2);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(642, 156);
            this.panel5.TabIndex = 3;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rdUSB);
            this.groupBox4.Controls.Add(this.rdGige);
            this.groupBox4.Location = new System.Drawing.Point(480, 11);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox4.Size = new System.Drawing.Size(152, 134);
            this.groupBox4.TabIndex = 26;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Connection Type";
            // 
            // rdUSB
            // 
            this.rdUSB.AutoSize = true;
            this.rdUSB.BackColor = System.Drawing.Color.White;
            this.rdUSB.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdUSB.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdUSB.Location = new System.Drawing.Point(9, 58);
            this.rdUSB.Margin = new System.Windows.Forms.Padding(4);
            this.rdUSB.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdUSB.Name = "rdUSB";
            this.rdUSB.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdUSB.Size = new System.Drawing.Size(64, 27);
            this.rdUSB.TabIndex = 9;
            this.rdUSB.Text = "USB";
            this.rdUSB.UnCheckedColor = System.Drawing.Color.Black;
            this.rdUSB.UseVisualStyleBackColor = false;
            // 
            // rdGige
            // 
            this.rdGige.AutoSize = true;
            this.rdGige.BackColor = System.Drawing.Color.White;
            this.rdGige.Checked = true;
            this.rdGige.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdGige.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdGige.Location = new System.Drawing.Point(9, 24);
            this.rdGige.Margin = new System.Windows.Forms.Padding(4);
            this.rdGige.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdGige.Name = "rdGige";
            this.rdGige.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdGige.Size = new System.Drawing.Size(67, 27);
            this.rdGige.TabIndex = 8;
            this.rdGige.TabStop = true;
            this.rdGige.Text = "Gige";
            this.rdGige.UnCheckedColor = System.Drawing.Color.Black;
            this.rdGige.UseVisualStyleBackColor = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rdBasler);
            this.groupBox3.Controls.Add(this.rdHIK);
            this.groupBox3.Controls.Add(this.rdVisionPro);
            this.groupBox3.Location = new System.Drawing.Point(12, 10);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox3.Size = new System.Drawing.Size(152, 134);
            this.groupBox3.TabIndex = 25;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Camera Manufacture";
            // 
            // rdBasler
            // 
            this.rdBasler.AutoSize = true;
            this.rdBasler.BackColor = System.Drawing.Color.White;
            this.rdBasler.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdBasler.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdBasler.Location = new System.Drawing.Point(7, 93);
            this.rdBasler.Margin = new System.Windows.Forms.Padding(4);
            this.rdBasler.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdBasler.Name = "rdBasler";
            this.rdBasler.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdBasler.Size = new System.Drawing.Size(75, 27);
            this.rdBasler.TabIndex = 8;
            this.rdBasler.Text = "Basler";
            this.rdBasler.UnCheckedColor = System.Drawing.Color.Black;
            this.rdBasler.UseVisualStyleBackColor = false;
            // 
            // rdHIK
            // 
            this.rdHIK.AutoSize = true;
            this.rdHIK.BackColor = System.Drawing.Color.White;
            this.rdHIK.Checked = true;
            this.rdHIK.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdHIK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdHIK.Location = new System.Drawing.Point(7, 58);
            this.rdHIK.Margin = new System.Windows.Forms.Padding(4);
            this.rdHIK.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdHIK.Name = "rdHIK";
            this.rdHIK.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdHIK.Size = new System.Drawing.Size(61, 27);
            this.rdHIK.TabIndex = 7;
            this.rdHIK.Text = "HIK";
            this.rdHIK.UnCheckedColor = System.Drawing.Color.Black;
            this.rdHIK.UseVisualStyleBackColor = false;
            // 
            // rdVisionPro
            // 
            this.rdVisionPro.AutoSize = true;
            this.rdVisionPro.BackColor = System.Drawing.Color.White;
            this.rdVisionPro.Checked = false;
            this.rdVisionPro.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdVisionPro.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdVisionPro.Location = new System.Drawing.Point(7, 24);
            this.rdVisionPro.Margin = new System.Windows.Forms.Padding(4);
            this.rdVisionPro.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdVisionPro.Name = "rdVisionPro";
            this.rdVisionPro.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdVisionPro.Size = new System.Drawing.Size(97, 27);
            this.rdVisionPro.TabIndex = 6;
            this.rdVisionPro.TabStop = true;
            this.rdVisionPro.Text = "VisionPro";
            this.rdVisionPro.UnCheckedColor = System.Drawing.Color.Black;
            this.rdVisionPro.UseVisualStyleBackColor = false;
            this.rdVisionPro.Visible = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ReverseX);
            this.groupBox2.Controls.Add(this.ReverseY);
            this.groupBox2.Location = new System.Drawing.Point(324, 11);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox2.Size = new System.Drawing.Size(152, 134);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Custom Property";
            // 
            // ReverseX
            // 
            this.ReverseX.AutoSize = true;
            this.ReverseX.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReverseX.Location = new System.Drawing.Point(13, 24);
            this.ReverseX.Margin = new System.Windows.Forms.Padding(4);
            this.ReverseX.Name = "ReverseX";
            this.ReverseX.Size = new System.Drawing.Size(87, 23);
            this.ReverseX.TabIndex = 22;
            this.ReverseX.Text = "Reverse X";
            this.ReverseX.UseVisualStyleBackColor = true;
            // 
            // ReverseY
            // 
            this.ReverseY.AutoSize = true;
            this.ReverseY.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReverseY.Location = new System.Drawing.Point(13, 55);
            this.ReverseY.Margin = new System.Windows.Forms.Padding(4);
            this.ReverseY.Name = "ReverseY";
            this.ReverseY.Size = new System.Drawing.Size(87, 23);
            this.ReverseY.TabIndex = 23;
            this.ReverseY.Text = "Reverse Y";
            this.ReverseY.UseVisualStyleBackColor = true;
            // 
            // CameraSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.857143F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CameraSetting";
            this.Size = new System.Drawing.Size(670, 508);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label29;
        public SLTSoft.RJControl.RJRadio rdHardwareAutoTrigger;
        public SLTSoft.RJControl.RJRadio rdSoftwareTrigger;
        public System.Windows.Forms.TextBox FileDirector;
        public System.Windows.Forms.TextBox FolderDirector;
        public SLTSoft.RJControl.RJRadio ModeFile;
        public SLTSoft.RJControl.RJRadio ModeFolder;
        public SLTSoft.RJControl.RJRadio ModeCam;
        public System.Windows.Forms.TextBox IPAddress;
        public System.Windows.Forms.CheckBox SaveGraphic;
        public System.Windows.Forms.CheckBox SaveOrigin;
        public System.Windows.Forms.CheckBox Use;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel6;
        public System.Windows.Forms.Button RefreshFolderImage;
        public System.Windows.Forms.Button RefreshFileImage;
        public System.Windows.Forms.Button btnConnectCam;
        public System.Windows.Forms.CheckBox SaveOriginSeparate;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.CheckBox ReverseX;
        public System.Windows.Forms.CheckBox ReverseY;
        private System.Windows.Forms.GroupBox groupBox3;
        public SLTSoft.RJControl.RJRadio rdHIK;
        public SLTSoft.RJControl.RJRadio rdVisionPro;
        public SLTSoft.RJControl.RJRadio rdBasler;
        private System.Windows.Forms.GroupBox groupBox4;
        public SLTSoft.RJControl.RJRadio rdUSB;
        public SLTSoft.RJControl.RJRadio rdGige;
        public SLTSoft.RJControl.RJRadio rdHardwareManualTrigger;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox tbName;
    }
}
