using Core.Detection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Processors
{
    public class LineIntersectionProcessor
    {
        /// <summary>
        ///  Computes the intersection of the provided lines represented in the polar space. The returned result is represented in the Descartes space.
        /// </summary>
        /// <param name="lines">LinePolar type, line representation in polar coordinate form.</param>
        /// <returns></returns>
        public static IEnumerable<Point> GetIntersectionsFromPolarLines(IEnumerable<LinePolar> lines)
        {
            // transform the collection into an array
            var linesArray = lines.ToArray();

            // create list collection for the results
            List<Point> intersections = new List<Point>();

            // iterate trough the all combination of lines and calulate the intersections
            for (int i = 0; i < linesArray.Length - 1; i++)
            {
                for (int j = 0; j < linesArray.Length; j++)
                {
                    if (GetIntersection(linesArray[i], linesArray[j], out Point intersectionPoint))
                    {
                        intersections.Add(intersectionPoint);
                    }
                }
            }

            // return the calculated intersections
            return intersections;
        }

        /// <summary>
        /// Computes the intersection of two line represented in the polar space. The returned result is represented in the Descartes space.
        /// If there is no intersection return false.
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns>True if the intersection exists, false if no intersection.</returns>
        private static bool GetIntersection(LinePolar line1, LinePolar line2, out Point intersectionPoint)
        {
            // caluclate the intersection in a try block
            try
            {
                // caluclate the intersection point coordinates from the polar line representation
                //
                //       r2 * sin(theta1) - r1 * sin(theta2)
                //  x = ------------------------------------
                //              sin(theta1 - theta2)
                //
                //          r1             cos(theta1)
                //  y = -----------  - x * -----------
                //      sin(theta1)        sin(theta1)
                //
                int x = (int)((line2.Radius * Math.Sin(line1.Theta) - line1.Radius * Math.Sin(line2.Theta)) / Math.Sin(line1.Theta - line2.Theta));
                int y = (int)((line1.Radius / Math.Sin(line1.Theta)) - (x * (Math.Cos(line1.Theta) / Math.Sin(line1.Theta))));

                // return the calculated point
                intersectionPoint = new Point(x: x, y: y);
                return true;
            }
            catch (Exception) { }

            // set default value for the point
            intersectionPoint = new Point();
            return false;
        }
    }
}
