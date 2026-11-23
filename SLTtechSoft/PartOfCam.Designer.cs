namespace SLTtechSoft
{
    partial class PartOfCam
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
            this.components = new System.ComponentModel.Container();
            this.mDisplay = new VMControls.Winform.Release.VmRenderControl();
            this.FitImage = new System.Windows.Forms.Button();
            this.Extent = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label = new System.Windows.Forms.Label();
            this.lbResult1 = new System.Windows.Forms.Label();
            this.lbResult2 = new System.Windows.Forms.Label();
            this.lbResult3 = new System.Windows.Forms.Label();
            this.lbResult4 = new System.Windows.Forms.Label();
            this.lbResult5 = new System.Windows.Forms.Label();
            this.lbResult6 = new System.Windows.Forms.Label();
            this.lbResult7 = new System.Windows.Forms.Label();
            this.lbResult8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mDisplay
            // 
            this.mDisplay.BackColor = System.Drawing.Color.Black;
            this.mDisplay.CoordinateInfoVisible = true;
            this.mDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mDisplay.ImageSource = null;
            this.mDisplay.Location = new System.Drawing.Point(2, 2);
            this.mDisplay.Margin = new System.Windows.Forms.Padding(0);
            this.mDisplay.ModuleSource = null;
            this.mDisplay.Name = "mDisplay";
            this.mDisplay.Size = new System.Drawing.Size(550, 322);
            this.mDisplay.TabIndex = 5;
            // 
            // FitImage
            // 
            this.FitImage.BackColor = System.Drawing.SystemColors.Control;
            this.FitImage.BackgroundImage = global::SLTtechSoft.Properties.Resources.FitImage1;
            this.FitImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.FitImage.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.FitImage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FitImage.Location = new System.Drawing.Point(29, 1);
            this.FitImage.Margin = new System.Windows.Forms.Padding(0);
            this.FitImage.Name = "FitImage";
            this.FitImage.Size = new System.Drawing.Size(22, 22);
            this.FitImage.TabIndex = 6;
            this.toolTip1.SetToolTip(this.FitImage, "Fit");
            this.FitImage.UseVisualStyleBackColor = false;
            // 
            // Extent
            // 
            this.Extent.BackColor = System.Drawing.SystemColors.Control;
            this.Extent.BackgroundImage = global::SLTtechSoft.Properties.Resources.Drop_down1;
            this.Extent.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.Extent.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.Extent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Extent.Location = new System.Drawing.Point(1, 1);
            this.Extent.Margin = new System.Windows.Forms.Padding(0);
            this.Extent.Name = "Extent";
            this.Extent.Size = new System.Drawing.Size(22, 22);
            this.Extent.TabIndex = 3;
            this.toolTip1.SetToolTip(this.Extent, "Extent");
            this.Extent.UseVisualStyleBackColor = false;
            // 
            // label
            // 
            this.label.BackColor = System.Drawing.Color.Yellow;
            this.label.Font = new System.Drawing.Font("Segoe UI", 9.857143F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label.Location = new System.Drawing.Point(63, 1);
            this.label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(167, 22);
            this.label.TabIndex = 7;
            this.label.Text = "First Time Align";
            this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbResult1
            // 
            this.lbResult1.AutoSize = true;
            this.lbResult1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(100)))));
            this.lbResult1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbResult1.ForeColor = System.Drawing.Color.White;
            this.lbResult1.Location = new System.Drawing.Point(6, 34);
            this.lbResult1.Name = "lbResult1";
            this.lbResult1.Size = new System.Drawing.Size(45, 17);
            this.lbResult1.TabIndex = 8;
            this.lbResult1.Text = "label1";
            // 
            // lbResult2
            // 
            this.lbResult2.AutoSize = true;
            this.lbResult2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(100)))));
            this.lbResult2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbResult2.ForeColor = System.Drawing.Color.White;
            this.lbResult2.Location = new System.Drawing.Point(6, 59);
            this.lbResult2.Name = "lbResult2";
            this.lbResult2.Size = new System.Drawing.Size(45, 17);
            this.lbResult2.TabIndex = 9;
            this.lbResult2.Text = "label2";
            // 
            // lbResult3
            // 
            this.lbResult3.AutoSize = true;
            this.lbResult3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(100)))));
            this.lbResult3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbResult3.ForeColor = System.Drawing.Color.White;
            this.lbResult3.Location = new System.Drawing.Point(6, 84);
            this.lbResult3.Name = "lbResult3";
            this.lbResult3.Size = new System.Drawing.Size(45, 17);
            this.lbResult3.TabIndex = 10;
            this.lbResult3.Text = "label2";
            // 
            // lbResult4
            // 
            this.lbResult4.AutoSize = true;
            this.lbResult4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(100)))));
            this.lbResult4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbResult4.ForeColor = System.Drawing.Color.White;
            this.lbResult4.Location = new System.Drawing.Point(6, 109);
            this.lbResult4.Name = "lbResult4";
            this.lbResult4.Size = new System.Drawing.Size(45, 17);
            this.lbResult4.TabIndex = 11;
            this.lbResult4.Text = "label3";
            // 
            // lbResult5
            // 
            this.lbResult5.AutoSize = true;
            this.lbResult5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(100)))));
            this.lbResult5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbResult5.ForeColor = System.Drawing.Color.White;
            this.lbResult5.Location = new System.Drawing.Point(6, 134);
            this.lbResult5.Name = "lbResult5";
            this.lbResult5.Size = new System.Drawing.Size(45, 17);
            this.lbResult5.TabIndex = 12;
            this.lbResult5.Text = "label3";
            // 
            // lbResult6
            // 
            this.lbResult6.AutoSize = true;
            this.lbResult6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(100)))));
            this.lbResult6.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbResult6.ForeColor = System.Drawing.Color.White;
            this.lbResult6.Location = new System.Drawing.Point(6, 159);
            this.lbResult6.Name = "lbResult6";
            this.lbResult6.Size = new System.Drawing.Size(45, 17);
            this.lbResult6.TabIndex = 13;
            this.lbResult6.Text = "label3";
            // 
            // lbResult7
            // 
            this.lbResult7.AutoSize = true;
            this.lbResult7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(100)))));
            this.lbResult7.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbResult7.ForeColor = System.Drawing.Color.White;
            this.lbResult7.Location = new System.Drawing.Point(6, 184);
            this.lbResult7.Name = "lbResult7";
            this.lbResult7.Size = new System.Drawing.Size(45, 17);
            this.lbResult7.TabIndex = 14;
            this.lbResult7.Text = "label3";
            // 
            // lbResult8
            // 
            this.lbResult8.AutoSize = true;
            this.lbResult8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(100)))));
            this.lbResult8.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbResult8.ForeColor = System.Drawing.Color.White;
            this.lbResult8.Location = new System.Drawing.Point(6, 209);
            this.lbResult8.Name = "lbResult8";
            this.lbResult8.Size = new System.Drawing.Size(45, 17);
            this.lbResult8.TabIndex = 15;
            this.lbResult8.Text = "label4";
            // 
            // PartOfCam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lime;
            this.Controls.Add(this.lbResult8);
            this.Controls.Add(this.lbResult7);
            this.Controls.Add(this.lbResult6);
            this.Controls.Add(this.lbResult5);
            this.Controls.Add(this.lbResult4);
            this.Controls.Add(this.lbResult3);
            this.Controls.Add(this.lbResult2);
            this.Controls.Add(this.lbResult1);
            this.Controls.Add(this.label);
            this.Controls.Add(this.FitImage);
            this.Controls.Add(this.Extent);
            this.Controls.Add(this.mDisplay);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "PartOfCam";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.Size = new System.Drawing.Size(554, 326);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.Button Extent;
        public VMControls.Winform.Release.VmRenderControl mDisplay;
        public System.Windows.Forms.Button FitImage;
        public System.Windows.Forms.ToolTip toolTip1;
        public System.Windows.Forms.Label label;
        private System.Windows.Forms.Label lbResult1;
        private System.Windows.Forms.Label lbResult2;
        private System.Windows.Forms.Label lbResult3;
        private System.Windows.Forms.Label lbResult4;
        private System.Windows.Forms.Label lbResult5;
        private System.Windows.Forms.Label lbResult6;
        private System.Windows.Forms.Label lbResult7;
        private System.Windows.Forms.Label lbResult8;
    }
}
