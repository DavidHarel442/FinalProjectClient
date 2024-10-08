namespace ProjectClient
{
    partial class HomePage
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
            this.OpenDrawingForm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // OpenDrawingForm
            // 
            this.OpenDrawingForm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.OpenDrawingForm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.OpenDrawingForm.FlatAppearance.BorderSize = 0;
            this.OpenDrawingForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OpenDrawingForm.Font = new System.Drawing.Font("Nirmala UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OpenDrawingForm.ForeColor = System.Drawing.SystemColors.Window;
            this.OpenDrawingForm.Location = new System.Drawing.Point(49, 187);
            this.OpenDrawingForm.Margin = new System.Windows.Forms.Padding(4);
            this.OpenDrawingForm.Name = "OpenDrawingForm";
            this.OpenDrawingForm.Size = new System.Drawing.Size(216, 83);
            this.OpenDrawingForm.TabIndex = 80;
            this.OpenDrawingForm.Text = "Open Drawing Board";
            this.OpenDrawingForm.UseVisualStyleBackColor = false;
            this.OpenDrawingForm.Click += new System.EventHandler(this.OpenDrawingForm_Click);
            // 
            // HomePage
            // 
            this.ClientSize = new System.Drawing.Size(582, 500);
            this.Controls.Add(this.OpenDrawingForm);
            this.Name = "HomePage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button OpenDrawingForm;
    }
}