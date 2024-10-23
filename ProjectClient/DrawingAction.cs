using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient
{
    public class DrawingAction
    {// for every action a user does on the drawing board the object is created witch the specific properties.
     // the class is also responsible for transfering all the properties into a json which is eventually sent to the server

        /// <summary>
        /// A string representing the type of action ("DrawLine", "Erase", "Fill"). with get and set
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// A Point struct representing the X and Y coordinates of the starting point. with get and set
        /// </summary>
        public Point StartPoint { get; set; }
        /// <summary>
        /// A Point struct representing the X and Y coordinates of the ending point. with get and set
        /// </summary>
        public Point EndPoint { get; set; }
        /// <summary>
        /// A Color struct representing the RGB values of the drawing color. with get and set
        /// </summary>
        public Color Color { get; set; }
        /// <summary>
        /// A float value representing the size of the drawing tool (pen width). with get and set
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// Serializes the current DrawingAction object to a JSON string.
        /// takes the object of the current "DrawingAction" and transfers it into a JSON string.
        /// this function is used to send the server the action the user performed.
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        /// <summary>
        /// Deserializes a JSON string to a DrawingAction object.
        /// A new DrawingAction object created from the deserialized JSON data.
        /// this function is used to transfer the action done by another user, that is received from the server to a DrawingAction object which is easy to understand
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static DrawingAction Deserialize(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DrawingAction>(json);
        }
    }
}
