using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ProjectClient
{
    public class MessageHandler
    {// this class handles the messages received
        /// <summary>
        /// a property sent through each class starting from HathatulClient. which you use to communicate with the server
        /// </summary>
        private TcpServerCommunication session;

        private static DrawingManager drawingManager;
        public static void SetDrawingManager(DrawingManager drawingManager)
        {
            MessageHandler.drawingManager = drawingManager;
        }
        /// <summary>
        /// this property will contain the current open form.
        /// </summary>
        public static Form currentForm;
        /// <summary>
        /// default constructor 
        /// </summary>
        /// <param name="client"></param>
        public MessageHandler(TcpServerCommunication client)
        {
            session = client;
        }
        /// <summary>
        /// this function will set the current active form as it is
        /// </summary>
        /// <param name="form"></param>
        public static void SetCurrentForm(Form form)
        {
            currentForm = form;
        }
        /// <summary>
        /// this function handles the messages
        /// </summary>
        /// <param name="message"></param>
        public void HandleMessage(TcpProtocolMessage message)
        {
            Console.WriteLine($"Handling message: Command={message.Command},Arguments={message.Arguments}");
            switch (message.Command)
            {
                case "UsernameAccepted":
                    session.usernameSent = true;
                    break;
                case "Registered":
                    SafeInvoke(() => {
                        MessageBox.Show("Registered Successfully");
                        if (MessageHandler.currentForm != null)
                        {
                            MessageHandler.currentForm.Hide();
                            HomePage homePage = new HomePage(session, message.Arguments);
                            homePage.Show();
                        }
                        
                    });
                    break;
                case "LoggedIn":
                    SafeInvoke(() => {
                        if (MessageHandler.currentForm != null)
                        {
                            MessageBox.Show("Logged In Successfully");
                            MessageHandler.currentForm.Hide();
                            HomePage homePage = new HomePage(session, message.Arguments);
                            homePage.Show();

                        }
                    });
                    break;
                case "CaptchaImage":
                    SafeInvoke(() => {
                        if (MessageHandler.currentForm is TripleAuthentication tripleAuth)
                        {
                            tripleAuth.UpdateCaptchaImage(message.Arguments);
                        }
                    });
                    break;
                case "DrawingUpdate":
                    if (currentForm is SharedDrawingForm sharedDrawingForm)
                    {
                        DrawingAction action = DrawingAction.Deserialize(message.Arguments);
                        sharedDrawingForm.ApplyDrawingAction(action);
                    }
                    break;
                case "FullDrawingState":
                    if (currentForm is SharedDrawingForm sharedDrawingForm12)
                    {
                        sharedDrawingForm12.HandleFullDrawingState(message.Arguments);
                    }
                    break;
                case "SendFullDrawingState":
                    if (currentForm is SharedDrawingForm sharedDrawingForm123)
                    {
                        sharedDrawingForm123.SendFullDrawingState(message.Arguments);
                    }
                    break;
                case "DrawingStateUpdate":
                    HandleReceivedDrawingState(message.Arguments);
                    break;
                case "Success":
                    MessageBox.Show($"Success: {message.Arguments}");
                    SuccessHandler(message.Arguments);
                    break;
                case "Issue":
                    MessageBox.Show($"Issue: {message.Arguments}");
                    IssuesHandler(message.Arguments);
                    break;
                case "ERROR":
                    Console.WriteLine($"Error: {message.Arguments}");
                    break;
                default:
                    Console.WriteLine($"Unknown command received: {message.Command}");
                    break;
            }
        }
        private void HandleReceivedDrawingState(string base64Image)
        {
            byte[] imageData = Convert.FromBase64String(base64Image);
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                // Convert the Base64 string back to a Bitmap and display it
                Bitmap receivedBitmap = new Bitmap(ms);

                // Apply the received bitmap to the drawing PictureBox
                drawingManager.drawingPic.Image = receivedBitmap;
                drawingManager.drawingPic.Invalidate();  // Redraw the PictureBox
            }
        }
        
        /// <summary>
        /// handles all issues.
        /// </summary>
        /// <param name="issue"></param>
        private void IssuesHandler(string issue)
        {
            switch (issue){
                case "UsernameDoesntExist":
                    break;
                case "Not Logged In":
                case "the username already exists":
                    if (currentForm is LoginForm)
                    {
                        ((LoginForm)currentForm).forgotPasswordClick = false;
                    }
                    else
                    {
                        SafeInvoke(() => {// if login/register wasnt completed, because username already existed in register. or after verification username+password didnt work
                                          //then it will open the login form again
                            if (MessageHandler.currentForm != null)
                            {
                                MessageHandler.currentForm.Hide();
                                session.Disconnect();// to reset connection . 
                                Thread.Sleep(1000);
                                LoginForm loginForm = new LoginForm(session, true);
                                loginForm.ShowDialog();

                            }
                        });
                    }
                    break;
            }
        }
        /// <summary>
        /// handles all success that something happens when they do.
        /// </summary>
        /// <param name="issue"></param>
        private void SuccessHandler(string success)
        {
            switch (success)
            {
                case "CodeValidated":
                    currentForm.Invoke((Action)(() => ((ChangePasswordForm)currentForm).HandleAfterCodeValidated()));
                    break;
                case "PasswordChanged":
                    currentForm.Invoke((Action)(() => ((ChangePasswordForm)currentForm).HandleAfterPasswordChange()));
                    break;
                case "AuthenticationVerified":
                    if (((TripleAuthentication)currentForm).loginOrRegister)
                    {
                        session.SendMessage("Login", ((TripleAuthentication)currentForm).allInfo);// after successful verification ask for login
                    }
                    else
                    {
                        session.SendMessage("Register", ((TripleAuthentication)currentForm).allInfo);// after successful verification ask for register. before even doing the verification the server checked that username doesnt already exist
                    }
                    break;
            }
        }
        /// <summary>
        /// sends encrypted username
        /// </summary>
        public void SendEncryptedUsername()
        {
            try
            {
                string encodedUsername = Convert.ToBase64String(Encoding.UTF8.GetBytes(TcpProtocolMessage.myUsername));
                session.SendMessage("USERNAME", encodedUsername);
                Console.WriteLine($"Sent encrypted username: {encodedUsername}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending encrypted username: {ex.Message}");
            }
        }
        /// <summary>
        /// this function will invoke if neccessery
        /// </summary>
        /// <param name="action"></param>
        private void SafeInvoke(Action action)// taken from claude
        {
            if (MessageHandler.currentForm != null)
            {
                if (MessageHandler.currentForm.InvokeRequired)
                {
                    MessageHandler.currentForm.Invoke(action);
                }
                else
                {
                    action();
                }
            }
        }




    }
}
