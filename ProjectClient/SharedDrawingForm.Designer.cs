namespace ProjectClient
{
    partial class SharedDrawingForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Camera = new System.Windows.Forms.PictureBox();
            this.ShowCamera = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.Camera)).BeginInit();
            this.SuspendLayout();
            // 
            // Camera
            // 
            this.Camera.Location = new System.Drawing.Point(482, 24);
            this.Camera.Name = "Camera";
            this.Camera.Size = new System.Drawing.Size(282, 138);
            this.Camera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Camera.TabIndex = 0;
            this.Camera.TabStop = false;
            // 
            // ShowCamera
            // 
            this.ShowCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.ShowCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ShowCamera.FlatAppearance.BorderSize = 0;
            this.ShowCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ShowCamera.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShowCamera.ForeColor = System.Drawing.SystemColors.Window;
            this.ShowCamera.Location = new System.Drawing.Point(482, 169);
            this.ShowCamera.Margin = new System.Windows.Forms.Padding(4);
            this.ShowCamera.Name = "ShowCamera";
            this.ShowCamera.Size = new System.Drawing.Size(282, 83);
            this.ShowCamera.TabIndex = 81;
            this.ShowCamera.Text = "Start Camera";
            this.ShowCamera.UseVisualStyleBackColor = false;
            this.ShowCamera.Click += new System.EventHandler(this.ShowCamera_Click);
            // 
            // SharedDrawingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ShowCamera);
            this.Controls.Add(this.Camera);
            this.Name = "SharedDrawingForm";
            this.Text = "SharedDrawingForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SharedDrawingForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.Camera)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox Camera;
        private System.Windows.Forms.Button ShowCamera;
    }
}