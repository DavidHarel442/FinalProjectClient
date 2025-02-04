using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectClient
{
    public class UdpImageClient
    {
        private readonly UdpClient udpClient;
        private readonly string serverIp;
        private readonly int serverPort;
        public readonly DrawingManager drawingManager;
        private bool isStreaming;
        private CancellationTokenSource cancellationTokenSource;

        public UdpImageClient(string serverIp, int serverPort, DrawingManager drawingManager)
        {
            this.serverIp = serverIp;
            this.serverPort = serverPort;
            this.drawingManager = drawingManager;
            udpClient = new UdpClient();
            isStreaming = false;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartStreaming()
        {
            if (isStreaming) return;

            isStreaming = true;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public void StopStreaming()
        {
            if (!isStreaming) return;

            isStreaming = false;
            cancellationTokenSource.Cancel();
        }

        public async Task SendFrameAsync(Bitmap frame)
        {
            try
            {
                int newWidth = 320;
                int newHeight = 240;

                using (Bitmap resizedFrame = new Bitmap(newWidth, newHeight))
                using (Graphics g = Graphics.FromImage(resizedFrame))
                {
                    g.DrawImage(frame, 0, 0, newWidth, newHeight);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        var jpegEncoder = ImageCodecInfo.GetImageEncoders()
                            .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        var encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 30L);

                        resizedFrame.Save(ms, jpegEncoder, encoderParams);
                        byte[] frameData = ms.ToArray();

                        var packet = new FramePacket
                        {
                            DrawingMode = drawingManager.currentToolMode,
                            PenColor = drawingManager.drawingPen.Color,
                            PenSize = drawingManager.drawingPen.Width,
                            FrameData = frameData
                        };
                        Console.WriteLine($"Sending packet - Mode: {packet.DrawingMode}, Color: R{packet.PenColor.R}G{packet.PenColor.G}B{packet.PenColor.B}, Size: {packet.PenSize}");
                        byte[] packetData = packet.Serialize();

                        int chunkSize = 60000;
                        for (int i = 0; i < packetData.Length; i += chunkSize)
                        {
                            int size = Math.Min(chunkSize, packetData.Length - i);
                            byte[] chunk = new byte[size];
                            Array.Copy(packetData, i, chunk, 0, size);
                            await udpClient.SendAsync(chunk, chunk.Length, serverIp, serverPort);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending frame: {ex.Message}");
            }
        }

        public void Close()
        {
            StopStreaming();
            udpClient.Close();
        }
    }
}
