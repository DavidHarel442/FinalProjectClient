using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectClient
{
    public partial class ChangePasswordForm : Form
    {// this class is incharge of the 'PasswordChange' form. 

        /// <summary>
        /// this property is an object of type LoginForm and is used to go back to the login form
        /// </summary>
        public LoginForm LoginForm = null;
        /// <summary>
        /// a property sent through each class starting from Program. which the client uses to communicate with the server
        /// </summary>
        private TcpServerCommunication tcpServer;
        /// <summary>
        /// default constructor. incharge of initializing the form
        /// </summary>
        public ChangePasswordForm(TcpServerCommunication client,LoginForm loginForm)
        {
            InitializeComponent();
            this.tcpServer = client;
            LoginForm = loginForm;
            MessageHandler.SetCurrentForm(this);

        }
        /// <summary>
        /// this function is an Event Handler, it happens when the client presses the 'SendMail' button. 
        /// it takes the username wrote in the TextBox above('username') the SendMail button and send a mail with a Verification Code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendMail_Click(object sender, EventArgs e)
        {
            if (username.Text.Length > 1)
            {
                tcpServer.HandleUsernameMessage(username.Text);
                Thread.Sleep(1000);
                LoginForm.forgotPasswordClick = true;
                tcpServer.SendMessage("SendForgotPassword",username.Text);
                SendMail.Enabled = false;
                username.Enabled = false;
                SecondLabel.Visible = true;
                ValidateCode.Visible = true;
                TheCode.Visible = true;
            }
        }
        /// <summary>
        /// this is an Event Handler for when you press the button ('ValidateCode').
        /// It send a message to the server to validate the code that the user recieved in the email
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidateCode_Click(object sender, EventArgs e)
        {
            tcpServer.SendMessage("ValidateCode", username.Text + "\t" + TheCode.Text);
        }
        /// <summary>
        /// this function handles the situation after the code gets validated
        /// </summary>
        public void HandleAfterCodeValidated()
        {
            ValidateCode.Enabled = false;
            TheCode.Enabled = false;
            LabelGuide.Visible = true;
            NewPassword.Visible = true;
            ButtonChangePas.Visible = true;
        }
        /// <summary>
        /// this is an Event Handler function for when you press on the button 'ButtonChangePas'.
        /// it send a message to the server with the password wrote in the TextBox Above it 'NewPassword'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonChangePas_Click(object sender, EventArgs e)
        {
            if (ValidatePassword(NewPassword.Text))
            {
                tcpServer.SendMessage("ChangePassword", username.Text + "\t" + NewPassword.Text);
            }
            else
            {
                MessageBox.Show("fix password: password should have: 1 capital letter, 1 lowercase letter, 1 numberic value and 1 special character");
            }
        }
        /// <summary>
        /// this function checks if your password is valid.
        /// has atleast 1 small letter. 1 capital letter. 1 special case letter, And at least 1 number
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidatePassword(string password)
        {

            string Uppercase = @"[A-Z]";
            string lowerCase = @"[a-z]";
            string number = @"\d";
            string symbol = @"[^a-zA-Z0-9]";
            if (Regex.IsMatch(password, Uppercase) && Regex.IsMatch(password, lowerCase) && Regex.IsMatch(password, number) && Regex.IsMatch(password, symbol))
            {
                return true;
            }
            return false;

        }
        /// <summary>
        /// this function is an Event Handler when the client presses on the 'BackToLogin' button.
        /// it then returns them to the login form and closes the ForgotPassword form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackToLogin_Click(object sender, EventArgs e)
        {
            LoginForm = new LoginForm(tcpServer,false);
            this.Hide();
            LoginForm.ShowDialog();
        }
        /// <summary>
        /// handle the situation after you change your password
        /// </summary>
        public void HandleAfterPasswordChange()
        {
            ButtonChangePas.Enabled = false;
            NewPassword.Enabled = false;
        }
    }
}
