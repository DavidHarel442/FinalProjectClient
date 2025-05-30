﻿namespace ProjectClient
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
            this.btnCalibrateColor = new System.Windows.Forms.Button();
            this.SaveDrawing = new System.Windows.Forms.Button();
            this.drawingName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnLoadDrawing = new System.Windows.Forms.Button();
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
            this.ShowCamera.Location = new System.Drawing.Point(680, 263);
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
            this.username.Location = new System.Drawing.Point(894, 42);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(68, 17);
            this.username.TabIndex = 82;
            this.username.Text = "username";
            // 
            // pic_color
            // 
            this.pic_color.BackColor = System.Drawing.Color.Black;
            this.pic_color.Location = new System.Drawing.Point(230, 33);
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
            this.btn_pencil.Location = new System.Drawing.Point(395, 20);
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
            this.btn_fill.Location = new System.Drawing.Point(301, 20);
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
            this.Camera.Location = new System.Drawing.Point(679, 118);
            this.Camera.Name = "Camera";
            this.Camera.Size = new System.Drawing.Size(283, 138);
            this.Camera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Camera.TabIndex = 0;
            this.Camera.TabStop = false;
            this.Camera.Click += new System.EventHandler(this.Camera_Click);
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
            this.btn_eraser.Location = new System.Drawing.Point(495, 20);
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
            this.drawingPic.Location = new System.Drawing.Point(-7, 118);
            this.drawingPic.Name = "drawingPic";
            this.drawingPic.Size = new System.Drawing.Size(680, 680);
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
            this.btn_Clear.Location = new System.Drawing.Point(587, 40);
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
            this.PenSize.Location = new System.Drawing.Point(404, 84);
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
            this.EraserSize.Location = new System.Drawing.Point(504, 84);
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
            this.StartDrawing.Location = new System.Drawing.Point(718, 383);
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
            this.Label3.Location = new System.Drawing.Point(715, 350);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(220, 17);
            this.Label3.TabIndex = 95;
            this.Label3.Text = "If You Want to Draw From Camera";
            this.Label3.Visible = false;
            // 
            // btnCalibrateColor
            // 
            this.btnCalibrateColor.Location = new System.Drawing.Point(680, 20);
            this.btnCalibrateColor.Name = "btnCalibrateColor";
            this.btnCalibrateColor.Size = new System.Drawing.Size(173, 63);
            this.btnCalibrateColor.TabIndex = 96;
            this.btnCalibrateColor.Text = "btnCalibrateColor";
            this.btnCalibrateColor.UseVisualStyleBackColor = true;
            this.btnCalibrateColor.Click += new System.EventHandler(this.btnCalibrateColor_Click);
            // 
            // SaveDrawing
            // 
            this.SaveDrawing.Location = new System.Drawing.Point(710, 579);
            this.SaveDrawing.Name = "SaveDrawing";
            this.SaveDrawing.Size = new System.Drawing.Size(229, 42);
            this.SaveDrawing.TabIndex = 97;
            this.SaveDrawing.Text = "Save Drawing";
            this.SaveDrawing.UseVisualStyleBackColor = true;
            this.SaveDrawing.Click += new System.EventHandler(this.SaveDrawing_Click);
            // 
            // drawingName
            // 
            this.drawingName.Location = new System.Drawing.Point(710, 562);
            this.drawingName.Name = "drawingName";
            this.drawingName.Size = new System.Drawing.Size(229, 20);
            this.drawingName.TabIndex = 98;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(165)))), ((int)(((byte)(169)))));
            this.label1.Location = new System.Drawing.Point(708, 542);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(211, 17);
            this.label1.TabIndex = 99;
            this.label1.Text = "what is the name of the Drawing";
            this.label1.Visible = false;
            // 
            // btnLoadDrawing
            // 
            this.btnLoadDrawing.Location = new System.Drawing.Point(710, 643);
            this.btnLoadDrawing.Name = "btnLoadDrawing";
            this.btnLoadDrawing.Size = new System.Drawing.Size(229, 42);
            this.btnLoadDrawing.TabIndex = 100;
            this.btnLoadDrawing.Text = "Load Drawing";
            this.btnLoadDrawing.UseVisualStyleBackColor = true;
            this.btnLoadDrawing.Click += new System.EventHandler(this.btnLoadDrawing_Click);
            // 
            // SharedDrawingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(993, 818);
            this.Controls.Add(this.btnLoadDrawing);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.drawingName);
            this.Controls.Add(this.SaveDrawing);
            this.Controls.Add(this.btnCalibrateColor);
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
        private System.Windows.Forms.Button btnCalibrateColor;
        private System.Windows.Forms.Button SaveDrawing;
        private System.Windows.Forms.TextBox drawingName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLoadDrawing;
    }
}