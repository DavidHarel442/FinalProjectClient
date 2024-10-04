using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectClient
{
    public partial class LoginForm : Form
    {
        private TcpServerCommunication tcpServer;
        public LoginForm(TcpServerCommunication TcpServer)
        {
            InitializeComponent();
            tcpServer = TcpServer;
            Console.WriteLine("Attempting to connect");
            tcpServer.ConnectToServer();


            tcpServer.messageHandler.LoginSuccess += OnLoginSuccess;
        }
        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void login_Click(object sender, EventArgs e)
        {
            if (!tcpServer.usernameSent)
            {
                Console.WriteLine("sending username");
                tcpServer.HandleUsernameMessage(username.Text);
                
            }
            Thread.Sleep(1000);
            tcpServer.SendMessage("Login", username.Text + '\t' + password.Text);
        }

        private void CreateAccount_Click(object sender, EventArgs e)
        {
            RegisterForm register = new RegisterForm(tcpServer);
            this.Hide();
            register.ShowDialog();
        }

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

        private void OnLoginSuccess(object sender, string message)
        {
            this.Invoke((MethodInvoker)delegate {
                HomePage homePage = new HomePage(tcpServer);
                homePage.Show();
                this.Hide();
            });
        }
    }
}
