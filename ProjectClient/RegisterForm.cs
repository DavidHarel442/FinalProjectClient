using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectClient
{
    public partial class RegisterForm : Form
    {// this class manges the events happening in the 'RegisterToGame' form

        /// <summary>
        /// a property sent through each class starting from HathatulClient. which you use to communicate with the server
        /// </summary>
        public TcpServerCommunication tcpServer = null;



        /// <summary>
        /// defualt constructor which Initialize the form
        /// </summary>
        public RegisterForm(TcpServerCommunication client)
        {
            InitializeComponent();
            this.tcpServer = client;

            tcpServer.messageHandler.RegistrationSuccess += OnRegistrationSuccess;
        }
        /// <summary>
        /// this function checks if all the boxes for the register are filled
        /// </summary>
        /// <returns></returns>
        public bool IsFilled()
        {
            if (username.Text.Count() <= 3 && username.Text.Count() > 12)
            {
                MessageBox.Show("fix name");
                return false;
            }
            if (!ValidatePassword(password.Text))
            {
                MessageBox.Show("fix password: password should have: 1 capital letter, 1 lowercase letter, 1 numberic value and 1 special character");
                return false;
            }
            if (firstname.Text.Count() <= 1 && firstname.Text.Count() > 15)
            {
                MessageBox.Show("fix firstname");
                return false;
            }
            if (lastname.Text.Count() <= 1 && lastname.Text.Count() > 15)
            {
                MessageBox.Show("fix lastname");
                return false;
            }
            if (!IsValidEmail())
            {
                return false;
            }

            if (city.Text.Count() == 0)
            {
                MessageBox.Show("choose city");
                return false;
            }
            if (gender.Text.Count() == 0)
            {
                MessageBox.Show("choose gender");
                return false;
            }
            return true;
        }
        /// <summary>
        /// this function checks if the email wrote in the text box is valid
        /// </summary>
        /// <param name="emailaddress"></param>
        /// <returns></returns>
        public bool IsValidEmail()
        {
            string email = emailTextBox.Text;
            if (email.Length == 0)
                return false;
            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
                string DomainMapper(Match match)
                {
                    var idn = new IdnMapping();
                    string domainName = idn.GetAscii(match.Groups[2].Value);
                    return match.Groups[1].Value + domainName;
                }
            }
            catch (ArgumentException e)
            {
                MessageBox.Show("Email Is Not Valid!");
                return false;
            }
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                MessageBox.Show("Email Is Not Valid!");
                return false;
            }
            return true;
        }
        /// <summary>
        /// this function checks if the password put in is valid.
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
        /// this function is an Event Handler for the register click.
        /// it sends a message to the server with all the info to register to the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void register_Click(object sender, EventArgs e)
        {
            if (this.IsFilled())
            {
                if (!tcpServer.usernameSent)
                {
                    tcpServer.HandleUsernameMessage(username.Text);
                }
                Thread.Sleep(1000);
                string data = username.Text + "\t" + password.Text + "\t" + firstname.Text + "\t" + lastname.Text + "\t" + emailTextBox.Text + "\t" + city.Text + "\t" + gender.Text;
                tcpServer.SendMessage("Register",data);
            }
        }
        /// <summary>
        /// this function is an Event Handler  and it sends the user back to the Login page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackToLogin_Click(object sender, EventArgs e)
        {
            LoginForm BackToGame = new LoginForm(tcpServer);
            this.Hide();
            BackToGame.ShowDialog();
        }



        private void OnRegistrationSuccess(object sender, string message)
        {
            this.Invoke((MethodInvoker)delegate {
                MessageBox.Show($"Registration Successful: {message}");
                HomePage homePage = new HomePage(tcpServer);
                homePage.Show();
                this.Hide();
            });
        }


    }
}
