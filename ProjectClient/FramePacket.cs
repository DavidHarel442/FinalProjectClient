using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient
{
    public class FramePacket
    {
        public byte[] FrameData { get; set; }
        public int DrawingMode { get; set; }
        public Color PenColor { get; set; }
        public float PenSize { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // First 4 bytes: Drawing mode
                ms.Write(BitConverter.GetBytes(DrawingMode), 0, 4);

                // Next 4 bytes: Color ARGB
                ms.Write(BitConverter.GetBytes(PenColor.ToArgb()), 0, 4);

                // Next 4 bytes: Pen size
                ms.Write(BitConverter.GetBytes(PenSize), 0, 4);

                // Rest: Frame data
                ms.Write(FrameData, 0, FrameData.Length);

                return ms.ToArray();
            }
        }

        public static FramePacket Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                return new FramePacket
                {
                    DrawingMode = reader.ReadInt32(),
                    PenColor = Color.FromArgb(reader.ReadInt32()),
                    PenSize = reader.ReadSingle(),
                    FrameData = reader.ReadBytes((int)(ms.Length - ms.Position))
                };
            }
        }
    }
}
