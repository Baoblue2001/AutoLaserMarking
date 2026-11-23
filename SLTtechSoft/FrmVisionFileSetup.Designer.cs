namespace SLTtechSoft
{
    partial class FrmVisionFileSetup
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
            this.panel12 = new System.Windows.Forms.Panel();
            this.lbRowSelected = new System.Windows.Forms.Label();
            this.dataGridViewVision = new System.Windows.Forms.DataGridView();
            this.clNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clExposure = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clDirection = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lbVisionNo = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.lbName = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewVision)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel12
            // 
            this.panel12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.panel12.Controls.Add(this.panel1);
            this.panel12.Controls.Add(this.panel2);
            this.panel12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel12.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.panel12.Location = new System.Drawing.Point(0, 0);
            this.panel12.Margin = new System.Windows.Forms.Padding(2);
            this.panel12.Name = "panel12";
            this.panel12.Size = new System.Drawing.Size(437, 167);
            this.panel12.TabIndex = 4;
            // 
            // lbRowSelected
            // 
            this.lbRowSelected.AutoSize = true;
            this.lbRowSelected.Location = new System.Drawing.Point(242, 13);
            this.lbRowSelected.Name = "lbRowSelected";
            this.lbRowSelected.Size = new System.Drawing.Size(38, 13);
            this.lbRowSelected.TabIndex = 15;
            this.lbRowSelected.Text = "label1";
            // 
            // dataGridViewVision
            // 
            this.dataGridViewVision.AllowUserToAddRows = false;
            this.dataGridViewVision.AllowUserToDeleteRows = false;
            this.dataGridViewVision.AllowUserToResizeColumns = false;
            this.dataGridViewVision.AllowUserToResizeRows = false;
            this.dataGridViewVision.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewVision.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clNo,
            this.clName,
            this.clExposure,
            this.clDirection});
            this.dataGridViewVision.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewVision.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewVision.Name = "dataGridViewVision";
            this.dataGridViewVision.Size = new System.Drawing.Size(437, 127);
            this.dataGridViewVision.TabIndex = 14;
            // 
            // clNo
            // 
            this.clNo.HeaderText = "No.";
            this.clNo.Name = "clNo";
            this.clNo.Width = 30;
            // 
            // clName
            // 
            this.clName.HeaderText = "Name";
            this.clName.Name = "clName";
            // 
            // clExposure
            // 
            this.clExposure.HeaderText = "Exposure";
            this.clExposure.Name = "clExposure";
            // 
            // clDirection
            // 
            this.clDirection.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.clDirection.HeaderText = "Direction";
            this.clDirection.Name = "clDirection";
            // 
            // lbVisionNo
            // 
            this.lbVisionNo.AutoSize = true;
            this.lbVisionNo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbVisionNo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbVisionNo.Location = new System.Drawing.Point(3, 13);
            this.lbVisionNo.Name = "lbVisionNo";
            this.lbVisionNo.Size = new System.Drawing.Size(47, 17);
            this.lbVisionNo.TabIndex = 13;
            this.lbVisionNo.Text = "Vision";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnSave.Location = new System.Drawing.Point(374, 7);
            this.btnSave.Margin = new System.Windows.Forms.Padding(2);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(62, 25);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.lbName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbName.Location = new System.Drawing.Point(88, 13);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(74, 13);
            this.lbName.TabIndex = 0;
            this.lbName.Text = "Vision Cam 1";
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btnLoad.Location = new System.Drawing.Point(308, 7);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(2);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(62, 25);
            this.btnLoad.TabIndex = 10;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Black;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(13, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(184, 2);
            this.label3.TabIndex = 3;
            this.label3.Text = "Vision File:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dataGridViewVision);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 40);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(437, 127);
            this.panel1.TabIndex = 5;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lbVisionNo);
            this.panel2.Controls.Add(this.lbRowSelected);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.btnLoad);
            this.panel2.Controls.Add(this.btnSave);
            this.panel2.Controls.Add(this.lbName);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(437, 40);
            this.panel2.TabIndex = 16;
            // 
            // FrmVisionFileSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel12);
            this.Name = "FrmVisionFileSetup";
            this.Size = new System.Drawing.Size(437, 167);
            this.panel12.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewVision)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel12;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dataGridViewVision;
        private System.Windows.Forms.Label lbRowSelected;
        public System.Windows.Forms.Button btnLoad;
        public System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn clNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn clName;
        private System.Windows.Forms.DataGridViewTextBoxColumn clExposure;
        private System.Windows.Forms.DataGridViewTextBoxColumn clDirection;
        public System.Windows.Forms.Label lbVisionNo;
        public System.Windows.Forms.Label lbName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}
