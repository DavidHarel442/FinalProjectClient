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
                    DrawingAction action = DrawingAction.Deserialize(message.Arguments);
                    if(currentForm is SharedDrawingForm form)
                        SafeInvoke(() => form.ApplyDrawingAction(action));
                    break;
                case "FullDrawingState":
                    Console.WriteLine($"Received FullDrawingState message with data length: {message.Arguments.Length}");
                    if (currentForm is SharedDrawingForm form1)
                    {
                        Console.WriteLine("Found SharedDrawingForm, calling HandleFullDrawingState");
                        SafeInvoke(() => form1.HandleFullDrawingState(message.Arguments));
                    }
                    else
                    {
                        Console.WriteLine("Error: CurrentForm is not SharedDrawingForm");
                    }
                    break;
                case "SendFullDrawingState":
                    if (currentForm is SharedDrawingForm form2) {
                        string image = form2.GetImageAsString();
                        SafeInvoke(() =>
                            session.SendMessage("SendFullDrawingState", $"{message.Arguments}\t{image}"));
                    }
                    break;
                case "DrawingsList":
                    HandleDrawingsList(message.Arguments);
                    break;

                case "DrawingData":
                    HandleDrawingData(message.Arguments);
                    break;
                case "Success":
                    if (message.Arguments == "DrawingSaved")
                    {
                        SafeInvoke(() => {
                            MessageBox.Show("Drawing saved successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    else if (message.Arguments == "DrawingDeleted")
                    {
                        SafeInvoke(() => {
                            MessageBox.Show("Drawing deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    else
                    {
                        MessageBox.Show($"Success: {message.Arguments}");
                        SuccessHandler(message.Arguments);
                    }
                    break;

                // Update your existing Issue case
                case "Issue":
                    if (message.Arguments == "DrawingNotFound" ||
                        message.Arguments == "FailedToSaveDrawing" ||
                        message.Arguments == "FailedToLoadDrawing" ||
                        message.Arguments == "FailedToDeleteDrawing")
                    {
                        SafeInvoke(() => {
                            MessageBox.Show($"Error: {message.Arguments}", "Drawing Operation Failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                    else
                    {
                        MessageBox.Show($"Issue: {message.Arguments}");
                        IssuesHandler(message.Arguments);
                    }
                    break;
                case "ERROR":
                    Console.WriteLine($"Error: {message.Arguments}");
                    break;
                default:
                    Console.WriteLine($"Unknown command received: {message.Command}");
                    break;
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
        private void HandleDrawingsList(string drawingsListJson)
        {
            SafeInvoke(() =>
            {
                if (currentForm is SharedDrawingForm drawingForm)
                {
                    try
                    {
                        // First, deserialize from JSON to a simple string list
                        List<string> drawingNames = JsonConvert.DeserializeObject<List<string>>(drawingsListJson);

                        // Then convert to a list of DrawingMetadata objects to match your existing method
                        List<DrawingMetadata> drawingsWithMetadata = new List<DrawingMetadata>();

                        foreach (string name in drawingNames)
                        {
                            drawingsWithMetadata.Add(new DrawingMetadata
                            {
                                Name = name,
                                CreatedBy = TcpProtocolMessage.myUsername,
                                LastModified = DateTime.Now // We don't have the actual time, so use current time
                            });
                        }

                        // Call your existing method with the converted list
                        ShowDrawingsListDialog(drawingsWithMetadata);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error processing drawings list: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            });
        }

        private void HandleDrawingData(string imageData)
        {
            SafeInvoke(() => {
                if (currentForm is SharedDrawingForm drawingForm)
                {
                    try
                    {
                        // The drawing will be updated via the FullDrawingState message that's broadcast to all
                        MessageBox.Show("Drawing loaded successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading drawing: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            });
        }

        private void ShowDrawingsListDialog(List<DrawingMetadata> drawings)
        {
            if (drawings == null || drawings.Count == 0)
            {
                MessageBox.Show("You don't have any saved drawings.", "No Drawings",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create a form to display drawings
            Form drawingsDialog = new Form
            {
                Text = "Your Saved Drawings",
                Size = new System.Drawing.Size(400, 500),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // Create a list box to display drawings
            ListBox drawingsList = new ListBox
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(345, 380),
                DisplayMember = "Name"
            };

            // Add drawings to the list
            foreach (var drawing in drawings)
            {
                drawingsList.Items.Add($"{drawing.Name} (Last Modified: {drawing.LastModified.ToString("g")})");
            }

            // Create buttons for actions
            Button loadButton = new Button
            {
                Text = "Load",
                Location = new System.Drawing.Point(20, 420),
                Size = new System.Drawing.Size(100, 30)
            };

            Button deleteButton = new Button
            {
                Text = "Delete",
                Location = new System.Drawing.Point(140, 420),
                Size = new System.Drawing.Size(100, 30)
            };

            Button closeButton = new Button
            {
                Text = "Close",
                Location = new System.Drawing.Point(260, 420),
                Size = new System.Drawing.Size(100, 30)
            };

            // Configure buttons
            loadButton.Click += (s, e) => {
                if (drawingsList.SelectedIndex >= 0)
                {
                    string selectedItem = drawingsList.SelectedItem.ToString();
                    string drawingName = ExtractDrawingName(selectedItem);

                    DialogResult result = MessageBox.Show(
                        "Loading this drawing will replace your current work. Continue?",
                        "Confirm Load",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        TcpServerCommunication client = ((SharedDrawingForm)currentForm).tcpServer;
                        client.SendMessage("LoadDrawing", drawingName);
                        drawingsDialog.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a drawing to load.", "Selection Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            deleteButton.Click += (s, e) => {
                if (drawingsList.SelectedIndex >= 0)
                {
                    string selectedItem = drawingsList.SelectedItem.ToString();
                    string drawingName = ExtractDrawingName(selectedItem);

                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete '{drawingName}'? This cannot be undone.",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        TcpServerCommunication client = ((SharedDrawingForm)currentForm).tcpServer;
                        client.SendMessage("DeleteDrawing", drawingName);
                        drawingsList.Items.RemoveAt(drawingsList.SelectedIndex);

                        if (drawingsList.Items.Count == 0)
                        {
                            MessageBox.Show("No more drawings to display.", "No Drawings",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            drawingsDialog.Close();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a drawing to delete.", "Selection Required",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            closeButton.Click += (s, e) => {
                drawingsDialog.Close();
            };

            // Add controls to the form
            drawingsDialog.Controls.Add(drawingsList);
            drawingsDialog.Controls.Add(loadButton);
            drawingsDialog.Controls.Add(deleteButton);
            drawingsDialog.Controls.Add(closeButton);

            // Show the dialog
            drawingsDialog.ShowDialog();
        }

        private string ExtractDrawingName(string listItem)
        {
            // Extract drawing name from format "Name (Last Modified: date)"
            int endOfName = listItem.IndexOf(" (");
            if (endOfName > 0)
            {
                return listItem.Substring(0, endOfName);
            }
            return listItem; // Fallback
        }



    }
}
