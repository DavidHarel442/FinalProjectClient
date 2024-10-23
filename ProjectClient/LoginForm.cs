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
using static System.Collections.Specialized.BitVector32;

namespace ProjectClient
{
    public partial class LoginForm : Form
    {
        /// <summary>
        /// this property turns to true if the client pressed on the "ForgotPassword" label.
        /// </summary>
        public bool forgotPasswordClick = false;
        /// <summary>
        /// a property sent through each class starting from Program. which the client uses to communicate with the server
        /// </summary>
        private TcpServerCommunication tcpServer;
        /// <summary>
        /// a static property incharge that if someone presses the 'ChangePassword' button it shows the form of the 'PasswordChange'.
        /// </summary>
        public ChangePasswordForm changePasswordObj = null;
        /// <summary>
        /// constructor, it initializes the LoginForm
        /// </summary>
        /// <param name="TcpServer"></param>
        /// <param name="shouldConnect"></param>
        public LoginForm(TcpServerCommunication TcpServer,bool shouldConnect)
        {
            InitializeComponent();
            tcpServer = TcpServer;
            if (shouldConnect)
            {
                Console.WriteLine("Attempting to connect");
                tcpServer.ConnectToServer();
            }
            MessageHandler.SetCurrentForm(this);


        }

        /// <summary>
        /// this function is An Event Handler that occurs when someone presses on the 'Login' Button.
        /// it sends a message to the server to check whether the username and password wrote are correct in the Sql DataBase
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void login_Click(object sender, EventArgs e)
        {
            if (!tcpServer.usernameSent)
            {
                Console.WriteLine("sending username");
                tcpServer.HandleUsernameMessage(username.Text);

            }
            Thread.Sleep(1000);
            // Comment out the TripleAuthentication part
            // TripleAuthentication triple = new TripleAuthentication(tcpServer, true, username.Text + '\t' + password.Text);
            // tcpServer.SendMessage("SendAuthentication", "");
            // this.Hide();
            // triple.ShowDialog();

            // Instead, directly send the login request
            tcpServer.SendMessage("Login", username.Text + '\t' + password.Text);

        }
        /// <summary>
        /// happens when someone presses on the 'CreateAccount' label.
        /// it transfers them to the 'RegisterToGame' form. and closes the 'LoginToGame' form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateAccount_Click(object sender, EventArgs e)
        {
            RegisterForm register = new RegisterForm(tcpServer);
            this.Hide();
            register.ShowDialog();
        }
        /// <summary>
        /// An Event Handler that occurs when someone is pressing on the 'ShowPassword'. if it wasnt pressed it shows the password wrote in if it was pressed it encrypts it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowPassword.Checked)
            {
                this.password.PasswordChar = '\0';
            }
            else
            {
                this.password.PasswordChar = '•';
            }
        }

        
        /// <summary>
        /// happens when someone presses on the 'ChangePassword' label.
        /// it transfers them to the 'PasswordChange' form. and closes the 'LoginToGame' form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangePassword_Click(object sender, EventArgs e)
        {
            changePasswordObj = new ChangePasswordForm(tcpServer,this);
            this.Hide();
            changePasswordObj.ShowDialog();
        }
        /// <summary>
        /// this function checks if the recieved password is valid.
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
        /// An Event Handler that occurs when someone is pressing on the 'ClearFields' button. and it clears all the fields textboxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClrearFields_Click(object sender, EventArgs e)
        {
            this.username.Clear();
            this.password.Clear();
            this.username.Focus();
        }
    }
}
