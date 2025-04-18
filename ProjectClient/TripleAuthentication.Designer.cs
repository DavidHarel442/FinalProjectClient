namespace ProjectClient
{
    partial class TripleAuthentication
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
            this.HeadLine = new System.Windows.Forms.Label();
            this.code = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.verify = new System.Windows.Forms.Button();
            this.captchaLabel = new System.Windows.Forms.Label();
            this.captcha = new System.Windows.Forms.TextBox();
            this.captchaImage = new System.Windows.Forms.PictureBox();
            this.backToLogin = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.captchaImage)).BeginInit();
            this.SuspendLayout();
            // 
            // HeadLine
            // 
            this.HeadLine.AutoSize = true;
            this.HeadLine.Font = new System.Drawing.Font("MS UI Gothic", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HeadLine.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.HeadLine.Location = new System.Drawing.Point(55, 58);
            this.HeadLine.Name = "HeadLine";
            this.HeadLine.Size = new System.Drawing.Size(193, 27);
            this.HeadLine.TabIndex = 78;
            this.HeadLine.Text = "Authentication";
            // 
            // code
            // 
            this.code.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(231)))), ((int)(((byte)(233)))));
            this.code.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.code.Font = new System.Drawing.Font("MV Boli", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.code.Location = new System.Drawing.Point(82, 151);
            this.code.Margin = new System.Windows.Forms.Padding(4);
            this.code.Multiline = true;
            this.code.Name = "code";
            this.code.Size = new System.Drawing.Size(148, 35);
            this.code.TabIndex = 77;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(165)))), ((int)(((byte)(169)))));
            this.label5.Location = new System.Drawing.Point(101, 118);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(117, 17);
            this.label5.TabIndex = 76;
            this.label5.Text = "Code Sent in mail";
            // 
            // verify
            // 
            this.verify.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.verify.Cursor = System.Windows.Forms.Cursors.Hand;
            this.verify.FlatAppearance.BorderSize = 0;
            this.verify.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.verify.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.verify.ForeColor = System.Drawing.SystemColors.Window;
            this.verify.Location = new System.Drawing.Point(42, 457);
            this.verify.Margin = new System.Windows.Forms.Padding(4);
            this.verify.Name = "verify";
            this.verify.Size = new System.Drawing.Size(216, 35);
            this.verify.TabIndex = 79;
            this.verify.Text = "Verify";
            this.verify.UseVisualStyleBackColor = false;
            this.verify.Click += new System.EventHandler(this.verify_Click);
            // 
            // captchaLabel
            // 
            this.captchaLabel.AutoSize = true;
            this.captchaLabel.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.captchaLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(165)))), ((int)(((byte)(169)))));
            this.captchaLabel.Location = new System.Drawing.Point(101, 263);
            this.captchaLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.captchaLabel.Name = "captchaLabel";
            this.captchaLabel.Size = new System.Drawing.Size(82, 17);
            this.captchaLabel.TabIndex = 80;
            this.captchaLabel.Text = "Put Captcha";
            // 
            // captcha
            // 
            this.captcha.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(231)))), ((int)(((byte)(233)))));
            this.captcha.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.captcha.Font = new System.Drawing.Font("MV Boli", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.captcha.Location = new System.Drawing.Point(60, 365);
            this.captcha.Margin = new System.Windows.Forms.Padding(4);
            this.captcha.Multiline = true;
            this.captcha.Name = "captcha";
            this.captcha.Size = new System.Drawing.Size(170, 35);
            this.captcha.TabIndex = 81;
            // 
            // captchaImage
            // 
            this.captchaImage.Location = new System.Drawing.Point(69, 283);
            this.captchaImage.Name = "captchaImage";
            this.captchaImage.Size = new System.Drawing.Size(149, 75);
            this.captchaImage.TabIndex = 82;
            this.captchaImage.TabStop = false;
            // 
            // backToLogin
            // 
            this.backToLogin.AutoSize = true;
            this.backToLogin.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.backToLogin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(165)))), ((int)(((byte)(169)))));
            this.backToLogin.Location = new System.Drawing.Point(101, 577);
            this.backToLogin.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.backToLogin.Name = "backToLogin";
            this.backToLogin.Size = new System.Drawing.Size(94, 17);
            this.backToLogin.TabIndex = 83;
            this.backToLogin.Text = "Back To Login";
            this.backToLogin.Click += new System.EventHandler(this.backToLogin_Click);
            // 
            // TripleAuthentication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(323, 632);
            this.Controls.Add(this.backToLogin);
            this.Controls.Add(this.captchaImage);
            this.Controls.Add(this.captcha);
            this.Controls.Add(this.captchaLabel);
            this.Controls.Add(this.verify);
            this.Controls.Add(this.HeadLine);
            this.Controls.Add(this.code);
            this.Controls.Add(this.label5);
            this.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TripleAuthentication";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TripleAuthentication";
            ((System.ComponentModel.ISupportInitialize)(this.captchaImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label HeadLine;
        private System.Windows.Forms.TextBox code;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button verify;
        private System.Windows.Forms.Label captchaLabel;
        private System.Windows.Forms.TextBox captcha;
        private System.Windows.Forms.PictureBox captchaImage;
        private System.Windows.Forms.Label backToLogin;
    }
}