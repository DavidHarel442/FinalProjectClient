namespace ProjectClient
{
    partial class ChangePasswordForm
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
            this.BackToLogin = new System.Windows.Forms.Label();
            this.LabelGuide = new System.Windows.Forms.Label();
            this.NewPassword = new System.Windows.Forms.TextBox();
            this.ButtonChangePas = new System.Windows.Forms.Button();
            this.ValidateCode = new System.Windows.Forms.Button();
            this.SendMail = new System.Windows.Forms.Button();
            this.TheCode = new System.Windows.Forms.TextBox();
            this.SecondLabel = new System.Windows.Forms.Label();
            this.GetStartedLabel = new System.Windows.Forms.Label();
            this.f = new System.Windows.Forms.Label();
            this.username = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // BackToLogin
            // 
            this.BackToLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BackToLogin.Font = new System.Drawing.Font("Nirmala UI", 12F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BackToLogin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.BackToLogin.Location = new System.Drawing.Point(114, 576);
            this.BackToLogin.Name = "BackToLogin";
            this.BackToLogin.Size = new System.Drawing.Size(133, 34);
            this.BackToLogin.TabIndex = 82;
            this.BackToLogin.Text = "Back to Login";
            this.BackToLogin.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.BackToLogin.Click += new System.EventHandler(this.BackToLogin_Click);
            // 
            // LabelGuide
            // 
            this.LabelGuide.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelGuide.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(165)))), ((int)(((byte)(169)))));
            this.LabelGuide.Location = new System.Drawing.Point(21, 385);
            this.LabelGuide.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LabelGuide.Name = "LabelGuide";
            this.LabelGuide.Size = new System.Drawing.Size(346, 53);
            this.LabelGuide.TabIndex = 81;
            this.LabelGuide.Text = "Put Here The New Password. your Password Must Contain 1 capital letter, 1 lowerca" +
    "se letter, 1 numberic value and 1 special character";
            this.LabelGuide.Visible = false;
            // 
            // NewPassword
            // 
            this.NewPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(231)))), ((int)(((byte)(233)))));
            this.NewPassword.Cursor = System.Windows.Forms.Cursors.Hand;
            this.NewPassword.Location = new System.Drawing.Point(52, 457);
            this.NewPassword.Margin = new System.Windows.Forms.Padding(4);
            this.NewPassword.Multiline = true;
            this.NewPassword.Name = "NewPassword";
            this.NewPassword.Size = new System.Drawing.Size(244, 41);
            this.NewPassword.TabIndex = 80;
            this.NewPassword.Visible = false;
            // 
            // ButtonChangePas
            // 
            this.ButtonChangePas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ButtonChangePas.Location = new System.Drawing.Point(112, 523);
            this.ButtonChangePas.Name = "ButtonChangePas";
            this.ButtonChangePas.Size = new System.Drawing.Size(126, 40);
            this.ButtonChangePas.TabIndex = 79;
            this.ButtonChangePas.Text = "Change Password";
            this.ButtonChangePas.UseVisualStyleBackColor = true;
            this.ButtonChangePas.Visible = false;
            this.ButtonChangePas.Click += new System.EventHandler(this.ButtonChangePas_Click);
            // 
            // ValidateCode
            // 
            this.ValidateCode.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ValidateCode.Location = new System.Drawing.Point(112, 320);
            this.ValidateCode.Name = "ValidateCode";
            this.ValidateCode.Size = new System.Drawing.Size(126, 38);
            this.ValidateCode.TabIndex = 78;
            this.ValidateCode.Text = "ValidateCode";
            this.ValidateCode.UseVisualStyleBackColor = true;
            this.ValidateCode.Visible = false;
            this.ValidateCode.Click += new System.EventHandler(this.ValidateCode_Click);
            // 
            // SendMail
            // 
            this.SendMail.Location = new System.Drawing.Point(112, 168);
            this.SendMail.Name = "SendMail";
            this.SendMail.Size = new System.Drawing.Size(126, 36);
            this.SendMail.TabIndex = 77;
            this.SendMail.Text = "Send Email";
            this.SendMail.UseVisualStyleBackColor = true;
            this.SendMail.Click += new System.EventHandler(this.SendMail_Click);
            // 
            // TheCode
            // 
            this.TheCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(231)))), ((int)(((byte)(233)))));
            this.TheCode.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TheCode.ForeColor = System.Drawing.Color.Black;
            this.TheCode.Location = new System.Drawing.Point(52, 272);
            this.TheCode.Margin = new System.Windows.Forms.Padding(4);
            this.TheCode.Multiline = true;
            this.TheCode.Name = "TheCode";
            this.TheCode.Size = new System.Drawing.Size(244, 32);
            this.TheCode.TabIndex = 76;
            this.TheCode.Visible = false;
            // 
            // SecondLabel
            // 
            this.SecondLabel.AutoSize = true;
            this.SecondLabel.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SecondLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(165)))), ((int)(((byte)(169)))));
            this.SecondLabel.Location = new System.Drawing.Point(13, 228);
            this.SecondLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SecondLabel.Name = "SecondLabel";
            this.SecondLabel.Size = new System.Drawing.Size(346, 17);
            this.SecondLabel.TabIndex = 75;
            this.SecondLabel.Text = "Check Your Email And put here The Code you Recieved";
            this.SecondLabel.Visible = false;
            // 
            // GetStartedLabel
            // 
            this.GetStartedLabel.AutoSize = true;
            this.GetStartedLabel.Font = new System.Drawing.Font("MS UI Gothic", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GetStartedLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.GetStartedLabel.Location = new System.Drawing.Point(88, 25);
            this.GetStartedLabel.Name = "GetStartedLabel";
            this.GetStartedLabel.Size = new System.Drawing.Size(208, 27);
            this.GetStartedLabel.TabIndex = 74;
            this.GetStartedLabel.Text = "Reset Password";
            // 
            // f
            // 
            this.f.AutoSize = true;
            this.f.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.f.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(164)))), ((int)(((byte)(165)))), ((int)(((byte)(169)))));
            this.f.Location = new System.Drawing.Point(13, 73);
            this.f.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.f.Name = "f";
            this.f.Size = new System.Drawing.Size(158, 17);
            this.f.TabIndex = 73;
            this.f.Text = "What is your Username?";
            // 
            // username
            // 
            this.username.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(231)))), ((int)(((byte)(233)))));
            this.username.Location = new System.Drawing.Point(52, 107);
            this.username.Margin = new System.Windows.Forms.Padding(4);
            this.username.Multiline = true;
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(244, 28);
            this.username.TabIndex = 72;
            // 
            // ChangePasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(380, 634);
            this.Controls.Add(this.BackToLogin);
            this.Controls.Add(this.LabelGuide);
            this.Controls.Add(this.NewPassword);
            this.Controls.Add(this.ButtonChangePas);
            this.Controls.Add(this.ValidateCode);
            this.Controls.Add(this.SendMail);
            this.Controls.Add(this.TheCode);
            this.Controls.Add(this.SecondLabel);
            this.Controls.Add(this.GetStartedLabel);
            this.Controls.Add(this.f);
            this.Controls.Add(this.username);
            this.Font = new System.Drawing.Font("Nirmala UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ChangePasswordForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ChangePasswordForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label BackToLogin;
        public System.Windows.Forms.Label LabelGuide;
        public System.Windows.Forms.TextBox NewPassword;
        public System.Windows.Forms.Button ButtonChangePas;
        public System.Windows.Forms.Button ValidateCode;
        public System.Windows.Forms.Button SendMail;
        public System.Windows.Forms.TextBox TheCode;
        public System.Windows.Forms.Label SecondLabel;
        private System.Windows.Forms.Label GetStartedLabel;
        private System.Windows.Forms.Label f;
        public System.Windows.Forms.TextBox username;
    }
}