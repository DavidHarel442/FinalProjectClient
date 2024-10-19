using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ProjectClient
{
    public partial class SharedDrawingForm : Form
    {// this form is incharge of the Shared Drawing Board
        private readonly object drawingLock = new object();
        private TcpServerCommunication tcpServer;
        private CameraManager cameraManager;
        public DrawingManager drawingManager;
        private bool paint = false;
        private Point py;
        private Point previousPoint;
        private bool isDrawing = false;
        public SharedDrawingForm(TcpServerCommunication tcpServer, string username1)
        {
            try
            {
                InitializeComponent();
                this.tcpServer = tcpServer;
                username.Text = username1;
                MessageHandler.SetCurrentForm(this);
                cameraManager = new CameraManager(Camera);
                drawingManager = new DrawingManager(drawingPic, tcpServer);
                MessageHandler.SetDrawingManager(drawingManager);
                drawingManager.DrawingActionPerformed += DrawingManager_DrawingActionPerformed;

                tcpServer.SendMessage("RequestFullDrawingState", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing SharedDrawingForm: {ex.Message}");
            }
        }

        private void ShowCamera_Click(object sender, EventArgs e)
        {
            try
            {
                if (ShowCamera.Text == "Start Camera")
                {
                    if (cameraManager.StartCamera())
                    {
                        ShowCamera.Text = "Stop Camera";
                    }
                }
                else
                {
                    cameraManager.StopCamera();
                    ShowCamera.Text = "Start Camera";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Camera error: {ex.Message}");
            }
        }

        private void SharedDrawingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            cameraManager.StopCamera();
        }

        private void DrawingManager_DrawingActionPerformed(object sender, DrawingAction e)
        {
            try
            {
                Console.WriteLine($"Sending Drawing Action: {e.Type}");
                tcpServer.SendMessage("DrawingAction", e.Serialize());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending drawing action: {ex.Message}");
            }
        }

        public void HandleFullDrawingState(string base64Image)
        {
            try
            {
                byte[] imageData = Convert.FromBase64String(base64Image);
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    Bitmap receivedBitmap = new Bitmap(ms);
                    drawingManager.SetFullDrawingState(receivedBitmap);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling full drawing state: {ex.Message}");
            }
        }

        public void SendFullDrawingState(string recipientIP)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    drawingPic.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();
                    string base64Image = Convert.ToBase64String(imageBytes);
                    tcpServer.SendMessage("SendFullDrawingState", $"{recipientIP}\t{base64Image}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending full drawing state: {ex.Message}");
            }
        }

        public void ApplyDrawingAction(DrawingAction action)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ApplyDrawingAction(action)));
                return;
            }

            drawingManager.ApplyDrawingAction(action);
            drawingPic.Invalidate();
        }

        private void drawingPic_MouseDown(object sender, MouseEventArgs e)
        {
            isDrawing = true;
            previousPoint = e.Location;
        }

        private void drawingPic_MouseUp(object sender, MouseEventArgs e)
        {
            isDrawing = false;
            
        }

        private void drawingPic_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                DrawingAction action = drawingManager.Draw(e.Location, previousPoint);
                if (action != null)
                {
                    tcpServer.SendMessage("DrawingAction", action.Serialize());
                }
                previousPoint = e.Location;

                // Ensure UI update is on the UI thread
                if (drawingPic.InvokeRequired)
                {
                    drawingPic.Invoke(new Action(() => drawingPic.Invalidate()));
                }
                else
                {
                    drawingPic.Invalidate();
                }
            }
        }

        private void btn_eraser_Click(object sender, EventArgs e)
        {
            drawingManager.SetDrawingMode(2);
        }

        private void btn_Clear_Click(object sender, EventArgs e)
        {
            try
            {
                drawingManager.Clear();
                drawingPic.Invalidate();
                DrawingAction clearAction = new DrawingAction { Type = "Clear" };
                tcpServer.SendMessage("DrawingAction", clearAction.Serialize());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing drawing: {ex.Message}");
            }
        }

        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        {
            Color selectedColor = drawingManager.PickColor(color_picker, e.Location);
            pic_color.BackColor = selectedColor;
            drawingManager.SetPenColor(selectedColor);
        }

        private void btn_pencil_Click(object sender, EventArgs e)
        {
            drawingManager.SetDrawingMode(1);
        }

        private void btn_fill_Click(object sender, EventArgs e)
        {
            drawingManager.SetDrawingMode(7);
        }

        private void PenSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (float.TryParse(PenSize.Text, out float size))
            {
                drawingManager.SetPenSize(size);
            }
        }

        private void EraserSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (float.TryParse(EraserSize.Text, out float size))
            {
                drawingManager.SetEraserSize(size);
            }
        }

        private async void drawingPic_MouseClick(object sender, MouseEventArgs e)
        {
            if (drawingManager.index == 7)  // Fill mode
            {
                Color fillColor = pic_color.BackColor;
                DrawingAction fillAction = new DrawingAction
                {
                    Type = "Fill",
                    StartPoint = e.Location,
                    Color = fillColor
                };

                try
                {
                    await drawingManager.FillAsync(e.Location, fillColor);
                    tcpServer.SendMessage("DrawingAction", fillAction.Serialize());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during fill operation: {ex.Message}", "Fill Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
