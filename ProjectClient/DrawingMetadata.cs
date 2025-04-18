using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient
{
    /// <summary>
    /// Represents metadata for a drawing stored on the server.
    /// Contains information about the drawing's name, creator, and modification time.
    /// </summary>
    public class DrawingMetadata
    {
        /// <summary>
        /// The name of the drawing, used as a unique identifier for storage and retrieval
        /// </summary>
        public string Name;

        /// <summary>
        /// The username of the user who created the drawing
        /// </summary>
        public string CreatedBy;

        /// <summary>
        /// The timestamp when the drawing was last modified
        /// </summary>
        public DateTime LastModified;
    }
}
