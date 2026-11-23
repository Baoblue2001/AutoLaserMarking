namespace SLTtechSoft
{
    partial class PLCConfigForm
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rdSLMP = new SLTSoft.RJControl.RJRadio();
            this.rdXGT = new SLTSoft.RJControl.RJRadio();
            this.rdModbus = new SLTSoft.RJControl.RJRadio();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rdRTU = new SLTSoft.RJControl.RJRadio();
            this.rdTCP = new SLTSoft.RJControl.RJRadio();
            this.Use = new System.Windows.Forms.CheckBox();
            this.label24 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.Port = new System.Windows.Forms.TextBox();
            this.HostIP = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.Stopbits = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.Parity = new System.Windows.Forms.ComboBox();
            this.Databits = new System.Windows.Forms.ComboBox();
            this.PortName = new System.Windows.Forms.ComboBox();
            this.Baudrate = new System.Windows.Forms.ComboBox();
            this.label29 = new System.Windows.Forms.Label();
            this.btnPLCConnect = new System.Windows.Forms.Button();
            this.btnPLCDisconnect = new System.Windows.Forms.Button();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rdSLMP);
            this.groupBox3.Controls.Add(this.rdXGT);
            this.groupBox3.Controls.Add(this.rdModbus);
            this.groupBox3.Location = new System.Drawing.Point(14, 43);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox3.Size = new System.Drawing.Size(124, 134);
            this.groupBox3.TabIndex = 26;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Protocol";
            // 
            // rdSLMP
            // 
            this.rdSLMP.AutoSize = true;
            this.rdSLMP.BackColor = System.Drawing.Color.White;
            this.rdSLMP.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdSLMP.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdSLMP.Location = new System.Drawing.Point(7, 93);
            this.rdSLMP.Margin = new System.Windows.Forms.Padding(4);
            this.rdSLMP.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdSLMP.Name = "rdSLMP";
            this.rdSLMP.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdSLMP.Size = new System.Drawing.Size(74, 27);
            this.rdSLMP.TabIndex = 8;
            this.rdSLMP.Text = "SLMP";
            this.rdSLMP.UnCheckedColor = System.Drawing.Color.Black;
            this.rdSLMP.UseVisualStyleBackColor = false;
            this.rdSLMP.CheckedChanged += new System.EventHandler(this.rdSLMP_CheckedChanged);
            // 
            // rdXGT
            // 
            this.rdXGT.AutoSize = true;
            this.rdXGT.BackColor = System.Drawing.Color.White;
            this.rdXGT.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdXGT.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdXGT.Location = new System.Drawing.Point(7, 58);
            this.rdXGT.Margin = new System.Windows.Forms.Padding(4);
            this.rdXGT.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdXGT.Name = "rdXGT";
            this.rdXGT.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdXGT.Size = new System.Drawing.Size(64, 27);
            this.rdXGT.TabIndex = 7;
            this.rdXGT.Text = "XGT";
            this.rdXGT.UnCheckedColor = System.Drawing.Color.Black;
            this.rdXGT.UseVisualStyleBackColor = false;
            this.rdXGT.CheckedChanged += new System.EventHandler(this.rdXGT_CheckedChanged);
            // 
            // rdModbus
            // 
            this.rdModbus.AutoSize = true;
            this.rdModbus.BackColor = System.Drawing.Color.White;
            this.rdModbus.Checked = true;
            this.rdModbus.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdModbus.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdModbus.Location = new System.Drawing.Point(7, 24);
            this.rdModbus.Margin = new System.Windows.Forms.Padding(4);
            this.rdModbus.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdModbus.Name = "rdModbus";
            this.rdModbus.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdModbus.Size = new System.Drawing.Size(90, 27);
            this.rdModbus.TabIndex = 6;
            this.rdModbus.TabStop = true;
            this.rdModbus.Text = "Modbus";
            this.rdModbus.UnCheckedColor = System.Drawing.Color.Black;
            this.rdModbus.UseVisualStyleBackColor = false;
            this.rdModbus.CheckedChanged += new System.EventHandler(this.rdModbus_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rdRTU);
            this.groupBox4.Controls.Add(this.rdTCP);
            this.groupBox4.Location = new System.Drawing.Point(159, 43);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.groupBox4.Size = new System.Drawing.Size(127, 134);
            this.groupBox4.TabIndex = 27;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Mode";
            // 
            // rdRTU
            // 
            this.rdRTU.AutoSize = true;
            this.rdRTU.BackColor = System.Drawing.Color.White;
            this.rdRTU.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdRTU.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdRTU.Location = new System.Drawing.Point(9, 58);
            this.rdRTU.Margin = new System.Windows.Forms.Padding(4);
            this.rdRTU.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdRTU.Name = "rdRTU";
            this.rdRTU.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdRTU.Size = new System.Drawing.Size(63, 27);
            this.rdRTU.TabIndex = 9;
            this.rdRTU.Text = "RTU";
            this.rdRTU.UnCheckedColor = System.Drawing.Color.Black;
            this.rdRTU.UseVisualStyleBackColor = false;
            this.rdRTU.CheckedChanged += new System.EventHandler(this.rdRTU_CheckedChanged);
            // 
            // rdTCP
            // 
            this.rdTCP.AutoSize = true;
            this.rdTCP.BackColor = System.Drawing.Color.White;
            this.rdTCP.Checked = true;
            this.rdTCP.CheckedColor = System.Drawing.Color.RoyalBlue;
            this.rdTCP.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.rdTCP.Location = new System.Drawing.Point(9, 24);
            this.rdTCP.Margin = new System.Windows.Forms.Padding(4);
            this.rdTCP.MinimumSize = new System.Drawing.Size(0, 27);
            this.rdTCP.Name = "rdTCP";
            this.rdTCP.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.rdTCP.Size = new System.Drawing.Size(62, 27);
            this.rdTCP.TabIndex = 8;
            this.rdTCP.TabStop = true;
            this.rdTCP.Text = "TCP";
            this.rdTCP.UnCheckedColor = System.Drawing.Color.Black;
            this.rdTCP.UseVisualStyleBackColor = false;
            this.rdTCP.CheckedChanged += new System.EventHandler(this.rdTCP_CheckedChanged);
            // 
            // Use
            // 
            this.Use.AutoSize = true;
            this.Use.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Use.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Use.Location = new System.Drawing.Point(302, 13);
            this.Use.Margin = new System.Windows.Forms.Padding(4);
            this.Use.Name = "Use";
            this.Use.Size = new System.Drawing.Size(49, 21);
            this.Use.TabIndex = 28;
            this.Use.Text = "Use";
            this.Use.UseVisualStyleBackColor = true;
            // 
            // label24
            // 
            this.label24.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label24.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label24.Location = new System.Drawing.Point(6, 34);
            this.label24.Margin = new System.Windows.Forms.Padding(3);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(117, 23);
            this.label24.TabIndex = 60;
            this.label24.Text = "HOST IP";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label26
            // 
            this.label26.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label26.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label26.Location = new System.Drawing.Point(7, 72);
            this.label26.Margin = new System.Windows.Forms.Padding(3);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(117, 23);
            this.label26.TabIndex = 62;
            this.label26.Text = "PORT";
            this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Port
            // 
            this.Port.Location = new System.Drawing.Point(130, 72);
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(127, 25);
            this.Port.TabIndex = 63;
            this.Port.Text = "502";
            this.Port.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // HostIP
            // 
            this.HostIP.Location = new System.Drawing.Point(130, 34);
            this.HostIP.Name = "HostIP";
            this.HostIP.Size = new System.Drawing.Size(127, 25);
            this.HostIP.TabIndex = 61;
            this.HostIP.Text = "127.0.0.1";
            this.HostIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label24);
            this.groupBox1.Controls.Add(this.HostIP);
            this.groupBox1.Controls.Add(this.Port);
            this.groupBox1.Controls.Add(this.label26);
            this.groupBox1.Location = new System.Drawing.Point(14, 224);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 227);
            this.groupBox1.TabIndex = 79;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "TCP Config";
            // 
            // ID
            // 
            this.ID.Location = new System.Drawing.Point(144, 182);
            this.ID.Name = "ID";
            this.ID.Size = new System.Drawing.Size(127, 25);
            this.ID.TabIndex = 81;
            this.ID.Text = "1";
            this.ID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(21, 182);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 23);
            this.label1.TabIndex = 80;
            this.label1.Text = "ID";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnRefresh);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.Stopbits);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label28);
            this.groupBox2.Controls.Add(this.Parity);
            this.groupBox2.Controls.Add(this.Databits);
            this.groupBox2.Controls.Add(this.PortName);
            this.groupBox2.Controls.Add(this.Baudrate);
            this.groupBox2.Controls.Add(this.label29);
            this.groupBox2.Location = new System.Drawing.Point(292, 224);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(252, 227);
            this.groupBox2.TabIndex = 80;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "RTU Config";
            // 
            // btnRefresh
            // 
            this.btnRefresh.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnRefresh.Location = new System.Drawing.Point(129, 63);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(111, 27);
            this.btnRefresh.TabIndex = 93;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(6, 34);
            this.label8.Margin = new System.Windows.Forms.Padding(3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(117, 23);
            this.label8.TabIndex = 83;
            this.label8.Text = "Port Name";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Stopbits
            // 
            this.Stopbits.FormattingEnabled = true;
            this.Stopbits.Items.AddRange(new object[] {
            "One",
            "Two"});
            this.Stopbits.Location = new System.Drawing.Point(129, 189);
            this.Stopbits.Name = "Stopbits";
            this.Stopbits.Size = new System.Drawing.Size(111, 25);
            this.Stopbits.TabIndex = 92;
            this.Stopbits.Text = "One";
            // 
            // label9
            // 
            this.label9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label9.Location = new System.Drawing.Point(6, 98);
            this.label9.Margin = new System.Windows.Forms.Padding(3);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 23);
            this.label9.TabIndex = 84;
            this.label9.Text = "Baudrate";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label10.Location = new System.Drawing.Point(6, 191);
            this.label10.Margin = new System.Windows.Forms.Padding(3);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(117, 23);
            this.label10.TabIndex = 91;
            this.label10.Text = "Stop bits";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label28
            // 
            this.label28.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label28.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label28.Location = new System.Drawing.Point(5, 129);
            this.label28.Margin = new System.Windows.Forms.Padding(3);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(117, 23);
            this.label28.TabIndex = 85;
            this.label28.Text = "Data bits";
            this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Parity
            // 
            this.Parity.FormattingEnabled = true;
            this.Parity.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even",
            "Mark",
            "Space"});
            this.Parity.Location = new System.Drawing.Point(129, 158);
            this.Parity.Name = "Parity";
            this.Parity.Size = new System.Drawing.Size(111, 25);
            this.Parity.TabIndex = 90;
            this.Parity.Text = "None";
            // 
            // Databits
            // 
            this.Databits.FormattingEnabled = true;
            this.Databits.Items.AddRange(new object[] {
            "7",
            "8"});
            this.Databits.Location = new System.Drawing.Point(129, 127);
            this.Databits.Name = "Databits";
            this.Databits.Size = new System.Drawing.Size(112, 25);
            this.Databits.TabIndex = 88;
            this.Databits.Text = "8";
            // 
            // PortName
            // 
            this.PortName.FormattingEnabled = true;
            this.PortName.Location = new System.Drawing.Point(129, 32);
            this.PortName.Name = "PortName";
            this.PortName.Size = new System.Drawing.Size(113, 25);
            this.PortName.TabIndex = 86;
            this.PortName.Text = "COM22";
            // 
            // Baudrate
            // 
            this.Baudrate.FormattingEnabled = true;
            this.Baudrate.Items.AddRange(new object[] {
            "4800",
            "9600",
            "14400",
            "19200",
            "38400",
            "56000",
            "57600",
            "115200"});
            this.Baudrate.Location = new System.Drawing.Point(129, 96);
            this.Baudrate.Name = "Baudrate";
            this.Baudrate.Size = new System.Drawing.Size(113, 25);
            this.Baudrate.TabIndex = 87;
            this.Baudrate.Text = "9600";
            // 
            // label29
            // 
            this.label29.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label29.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label29.Location = new System.Drawing.Point(6, 160);
            this.label29.Margin = new System.Windows.Forms.Padding(3);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(117, 23);
            this.label29.TabIndex = 89;
            this.label29.Text = "Parity";
            this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnPLCConnect
            // 
            this.btnPLCConnect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnPLCConnect.Location = new System.Drawing.Point(290, 49);
            this.btnPLCConnect.Name = "btnPLCConnect";
            this.btnPLCConnect.Size = new System.Drawing.Size(113, 45);
            this.btnPLCConnect.TabIndex = 80;
            this.btnPLCConnect.Text = "Connect";
            this.btnPLCConnect.UseVisualStyleBackColor = true;
            // 
            // btnPLCDisconnect
            // 
            this.btnPLCDisconnect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnPLCDisconnect.Location = new System.Drawing.Point(409, 49);
            this.btnPLCDisconnect.Name = "btnPLCDisconnect";
            this.btnPLCDisconnect.Size = new System.Drawing.Size(113, 45);
            this.btnPLCDisconnect.TabIndex = 79;
            this.btnPLCDisconnect.Text = "Disconnect";
            this.btnPLCDisconnect.UseVisualStyleBackColor = true;
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(144, 13);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(127, 25);
            this.tbName.TabIndex = 83;
            this.tbName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(21, 14);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 23);
            this.label2.TabIndex = 82;
            this.label2.Text = "Name";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PLCConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.ID);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Use);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnPLCDisconnect);
            this.Controls.Add(this.btnPLCConnect);
            this.Font = new System.Drawing.Font("Segoe UI", 9.857143F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "PLCConfigForm";
            this.Size = new System.Drawing.Size(553, 467);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        public SLTSoft.RJControl.RJRadio rdSLMP;
        public SLTSoft.RJControl.RJRadio rdXGT;
        public SLTSoft.RJControl.RJRadio rdModbus;
        private System.Windows.Forms.GroupBox groupBox4;
        public SLTSoft.RJControl.RJRadio rdRTU;
        public SLTSoft.RJControl.RJRadio rdTCP;
        public System.Windows.Forms.CheckBox Use;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label26;
        public System.Windows.Forms.TextBox Port;
        public System.Windows.Forms.TextBox HostIP;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.TextBox ID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label29;
        public System.Windows.Forms.Button btnRefresh;
        public System.Windows.Forms.Button btnPLCConnect;
        public System.Windows.Forms.Button btnPLCDisconnect;
        public System.Windows.Forms.ComboBox Stopbits;
        public System.Windows.Forms.ComboBox Parity;
        public System.Windows.Forms.ComboBox Databits;
        public System.Windows.Forms.ComboBox PortName;
        public System.Windows.Forms.ComboBox Baudrate;
        public System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label2;
    }
}
