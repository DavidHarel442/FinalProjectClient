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
            this.ShowCamera = new System.Windows.Forms.Button();
            this.username = new System.Windows.Forms.Label();
            this.pic_color = new System.Windows.Forms.Button();
            this.btn_pencil = new System.Windows.Forms.Button();
            this.btn_fill = new System.Windows.Forms.Button();
            this.Camera = new System.Windows.Forms.PictureBox();
            this.btn_eraser = new System.Windows.Forms.Button();
            this.color_picker = new System.Windows.Forms.PictureBox();
            this.drawingPic = new System.Windows.Forms.PictureBox();
            this.btn_Clear = new System.Windows.Forms.Button();
            this.PenSize = new System.Windows.Forms.ComboBox();
            this.EraserSize = new System.Windows.Forms.ComboBox();
            this.StartDrawing = new System.Windows.Forms.Button();
            this.Label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Camera)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.color_picker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.drawingPic)).BeginInit();
            this.SuspendLayout();
            // 
            // ShowCamera
            // 
            this.ShowCamera.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.ShowCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ShowCamera.FlatAppearance.BorderSize = 0;
            this.ShowCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ShowCamera.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ShowCamera.ForeColor = System.Drawing.SystemColors.Window;
            this.ShowCamera.Location = new System.Drawing.Point(717, 218);
            this.ShowCamera.Margin = new System.Windows.Forms.Padding(4);
            this.ShowCamera.Name = "ShowCamera";
            this.ShowCamera.Size = new System.Drawing.Size(282, 83);
            this.ShowCamera.TabIndex = 81;
            this.ShowCamera.Text = "Start Camera";
            this.ShowCamera.UseVisualStyleBackColor = false;
            this.ShowCamera.Click += new System.EventHandler(this.ShowCamera_Click);
            // 
            // username
            // 
            this.username.AutoSize = true;
            this.username.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.username.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(165)))), ((int)(((byte)(169)))));
            this.username.Location = new System.Drawing.Point(931, 24);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(68, 17);
            this.username.TabIndex = 82;
            this.username.Text = "username";
            // 
            // pic_color
            // 
            this.pic_color.BackColor = System.Drawing.Color.Black;
            this.pic_color.Location = new System.Drawing.Point(230, 37);
            this.pic_color.Name = "pic_color";
            this.pic_color.Size = new System.Drawing.Size(65, 45);
            this.pic_color.TabIndex = 84;
            this.pic_color.UseVisualStyleBackColor = false;
            // 
            // btn_pencil
            // 
            this.btn_pencil.BackColor = System.Drawing.Color.White;
            this.btn_pencil.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_pencil.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.btn_pencil.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_pencil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_pencil.ForeColor = System.Drawing.Color.Black;
            this.btn_pencil.Image = global::ProjectClient.Properties.Resources.rsz_2pencil12;
            this.btn_pencil.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btn_pencil.Location = new System.Drawing.Point(463, 24);
            this.btn_pencil.Name = "btn_pencil";
            this.btn_pencil.Size = new System.Drawing.Size(75, 58);
            this.btn_pencil.TabIndex = 87;
            this.btn_pencil.Text = "Pencil";
            this.btn_pencil.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btn_pencil.UseVisualStyleBackColor = false;
            this.btn_pencil.Click += new System.EventHandler(this.btn_pencil_Click);
            // 
            // btn_fill
            // 
            this.btn_fill.BackColor = System.Drawing.Color.White;
            this.btn_fill.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_fill.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.btn_fill.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_fill.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_fill.ForeColor = System.Drawing.Color.Black;
            this.btn_fill.Image = global::ProjectClient.Properties.Resources.bucket;
            this.btn_fill.Location = new System.Drawing.Point(382, 24);
            this.btn_fill.Name = "btn_fill";
            this.btn_fill.Size = new System.Drawing.Size(75, 58);
            this.btn_fill.TabIndex = 86;
            this.btn_fill.Text = "Fill";
            this.btn_fill.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btn_fill.UseVisualStyleBackColor = false;
            this.btn_fill.Click += new System.EventHandler(this.btn_fill_Click);
            // 
            // Camera
            // 
            this.Camera.Location = new System.Drawing.Point(717, 73);
            this.Camera.Name = "Camera";
            this.Camera.Size = new System.Drawing.Size(282, 138);
            this.Camera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Camera.TabIndex = 0;
            this.Camera.TabStop = false;
            // 
            // btn_eraser
            // 
            this.btn_eraser.BackColor = System.Drawing.Color.White;
            this.btn_eraser.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_eraser.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.btn_eraser.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_eraser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_eraser.ForeColor = System.Drawing.Color.Black;
            this.btn_eraser.Image = global::ProjectClient.Properties.Resources.eraser;
            this.btn_eraser.Location = new System.Drawing.Point(544, 24);
            this.btn_eraser.Name = "btn_eraser";
            this.btn_eraser.Size = new System.Drawing.Size(75, 58);
            this.btn_eraser.TabIndex = 88;
            this.btn_eraser.Text = "Eraser";
            this.btn_eraser.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btn_eraser.UseVisualStyleBackColor = false;
            this.btn_eraser.Click += new System.EventHandler(this.btn_eraser_Click);
            // 
            // color_picker
            // 
            this.color_picker.Image = global::ProjectClient.Properties.Resources.color_palette;
            this.color_picker.Location = new System.Drawing.Point(5, 4);
            this.color_picker.Name = "color_picker";
            this.color_picker.Size = new System.Drawing.Size(219, 108);
            this.color_picker.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.color_picker.TabIndex = 89;
            this.color_picker.TabStop = false;
            this.color_picker.MouseClick += new System.Windows.Forms.MouseEventHandler(this.color_picker_MouseClick);
            // 
            // drawingPic
            // 
            this.drawingPic.BackColor = System.Drawing.Color.White;
            this.drawingPic.Location = new System.Drawing.Point(5, 118);
            this.drawingPic.Name = "drawingPic";
            this.drawingPic.Size = new System.Drawing.Size(705, 496);
            this.drawingPic.TabIndex = 90;
            this.drawingPic.TabStop = false;
            this.drawingPic.MouseClick += new System.Windows.Forms.MouseEventHandler(this.drawingPic_MouseClick);
            this.drawingPic.MouseDown += new System.Windows.Forms.MouseEventHandler(this.drawingPic_MouseDown);
            this.drawingPic.MouseMove += new System.Windows.Forms.MouseEventHandler(this.drawingPic_MouseMove);
            this.drawingPic.MouseUp += new System.Windows.Forms.MouseEventHandler(this.drawingPic_MouseUp);
            // 
            // btn_Clear
            // 
            this.btn_Clear.BackColor = System.Drawing.Color.White;
            this.btn_Clear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_Clear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.btn_Clear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btn_Clear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Clear.ForeColor = System.Drawing.Color.Black;
            this.btn_Clear.Location = new System.Drawing.Point(625, 24);
            this.btn_Clear.Name = "btn_Clear";
            this.btn_Clear.Size = new System.Drawing.Size(86, 30);
            this.btn_Clear.TabIndex = 91;
            this.btn_Clear.Text = "Clear";
            this.btn_Clear.UseVisualStyleBackColor = false;
            this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
            // 
            // PenSize
            // 
            this.PenSize.FormattingEnabled = true;
            this.PenSize.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "12",
            "14",
            "16",
            "18",
            "20"});
            this.PenSize.Location = new System.Drawing.Point(472, 88);
            this.PenSize.Name = "PenSize";
            this.PenSize.Size = new System.Drawing.Size(57, 21);
            this.PenSize.TabIndex = 92;
            this.PenSize.Text = "1";
            this.PenSize.SelectedIndexChanged += new System.EventHandler(this.PenSize_SelectedIndexChanged);
            // 
            // EraserSize
            // 
            this.EraserSize.FormattingEnabled = true;
            this.EraserSize.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "12",
            "14",
            "16",
            "18",
            "20"});
            this.EraserSize.Location = new System.Drawing.Point(553, 88);
            this.EraserSize.Name = "EraserSize";
            this.EraserSize.Size = new System.Drawing.Size(57, 21);
            this.EraserSize.TabIndex = 93;
            this.EraserSize.Text = "12";
            this.EraserSize.SelectedIndexChanged += new System.EventHandler(this.EraserSize_SelectedIndexChanged);
            // 
            // StartDrawing
            // 
            this.StartDrawing.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.StartDrawing.Cursor = System.Windows.Forms.Cursors.Hand;
            this.StartDrawing.FlatAppearance.BorderSize = 0;
            this.StartDrawing.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StartDrawing.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartDrawing.ForeColor = System.Drawing.SystemColors.Window;
            this.StartDrawing.Location = new System.Drawing.Point(746, 338);
            this.StartDrawing.Margin = new System.Windows.Forms.Padding(4);
            this.StartDrawing.Name = "StartDrawing";
            this.StartDrawing.Size = new System.Drawing.Size(223, 64);
            this.StartDrawing.TabIndex = 94;
            this.StartDrawing.Text = "Start Drawing";
            this.StartDrawing.UseVisualStyleBackColor = false;
            this.StartDrawing.Visible = false;
            this.StartDrawing.Click += new System.EventHandler(this.StartDrawing_Click);
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(165)))), ((int)(((byte)(169)))));
            this.Label3.Location = new System.Drawing.Point(749, 317);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(220, 17);
            this.Label3.TabIndex = 95;
            this.Label3.Text = "If You Want to Draw From Camera";
            this.Label3.Visible = false;
            // 
            // SharedDrawingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1011, 626);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.StartDrawing);
            this.Controls.Add(this.EraserSize);
            this.Controls.Add(this.PenSize);
            this.Controls.Add(this.btn_Clear);
            this.Controls.Add(this.drawingPic);
            this.Controls.Add(this.color_picker);
            this.Controls.Add(this.btn_eraser);
            this.Controls.Add(this.btn_pencil);
            this.Controls.Add(this.btn_fill);
            this.Controls.Add(this.pic_color);
            this.Controls.Add(this.username);
            this.Controls.Add(this.ShowCamera);
            this.Controls.Add(this.Camera);
            this.Name = "SharedDrawingForm";
            this.Text = "SharedDrawingForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SharedDrawingForm_FormClosing_1);
            ((System.ComponentModel.ISupportInitialize)(this.Camera)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.color_picker)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.drawingPic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Camera;
        private System.Windows.Forms.Button ShowCamera;
        private System.Windows.Forms.Label username;
        private System.Windows.Forms.Button pic_color;
        private System.Windows.Forms.Button btn_fill;
        private System.Windows.Forms.Button btn_pencil;
        private System.Windows.Forms.Button btn_eraser;
        private System.Windows.Forms.PictureBox color_picker;
        private System.Windows.Forms.PictureBox drawingPic;
        private System.Windows.Forms.Button btn_Clear;
        private System.Windows.Forms.ComboBox PenSize;
        private System.Windows.Forms.ComboBox EraserSize;
        private System.Windows.Forms.Button StartDrawing;
        private System.Windows.Forms.Label Label3;
    }
}