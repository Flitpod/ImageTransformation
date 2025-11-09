using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Processors;

namespace Core.Detection
{
    public class QuadrangleDetector
    {
        /// <summary>
        /// Try to detect quadrangle shape on the input image. 
        /// If a quadrangle detected on the image, the method returns true 
        /// and the corners out parameter contains the found corners represented in the cartesian space. Else the method returns false.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="corners"></param>
        /// <returns></returns>
        public static bool FindCorners(Bitmap source, out IEnumerable<Point> corners)
        {
            // find the top 4 lines on the image using HoughLine
            var lines = HoughLine.FindTopNLine(
                source: source,
                numberOfTopLines: 4
            );

            // find the all intersection points between the detected 4 lines
            var intersections = LineIntersectionProcessor.GetIntersectionsFromPolarLines(lines: lines);

            // filter out the points are located inside the image
            int height = source.Height;
            int width = source.Width;
            var intersectionsInsideTheImage = intersections.Where(point =>
            {
                if (point.X >= 0 && point.X < width && point.Y >= 0 && point.Y < height)
                {
                    return true;
                }
                return false;
            });

            // check if there are exactly 4 tedected intersections / corners or not
            if (intersectionsInsideTheImage.Count() == 4)
            {
                corners = intersectionsInsideTheImage;
                return true;
            }

            corners = new List<Point>();
            return false;
        }
    }
}
