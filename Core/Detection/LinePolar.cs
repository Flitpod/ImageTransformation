using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Detection
{
    /// <summary>
    /// Model class for representing a line in polar coordinate space.
    /// </summary>
    public class LinePolar
    {
        /// <summary>
        /// Length of the radius / normal vector measured in pixel.
        /// </summary>
        public double Radius { get; set; }

        /// <summary>
        /// Angle of the line related to the represented line measured in radian.
        /// </summary>
        public double Theta { get; set; }
    }
}
