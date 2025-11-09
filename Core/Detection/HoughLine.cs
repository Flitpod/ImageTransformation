using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageTransformation.Core;

namespace Core.Detection
{
    public class HoughLine
    {
        // PUBLIC METHODS
        /// <summary>
        /// Apply canny edge detection on the source image. Process the voting operation on the edge detected image, and select the top N detected lines.
        /// </summary>
        /// <param name="source">8 bit depth, 3 channel RGB image</param>
        /// <param name="numberOfTopLines">Top n lines to select which are got the most votes</param>
        /// <returns></returns>
        public static IEnumerable<LinePolar> FindTopNLine(Bitmap source, int numberOfTopLines)
        {
            // STEPS
            // 1. Apply Canny on RGB image
            // 2. Create Accumulator array and fill with votes based on Canny edge detected image
            // 3. Filter out the first nth highest vote and return them as IEnumerable<LinePolar> descending by votes

            // 0.1 create edge detected image reference
            Bitmap edgeDetectedImage = new Bitmap(
                width: source.Width,
                height: source.Height,
                format: PixelFormat.Format8bppIndexed
            );

            // 1. Apply canny edge detection on the source image and save the result into edgeDetectedImage 
            Filters.ApplyCanny(
                source: source,
                destination: ref edgeDetectedImage
            );

            // 2.0 create local variables with values for later usage
            // measured in pixel
            double deltaRadius = 5;
            // measured in radian - 180 / 50 = 3.6 degree
            double deltaTheta = (Math.PI) / 100;

            // 2. Create Accumulator array and fill with votes based on Canny edge detected image
            ComputeVoting(
                source: edgeDetectedImage,
                deltaRadius: deltaRadius,
                deltaTheta: deltaTheta,
                votesAccumulator: out Matrix votesAccumulator
            );

            // supress locally the non-maximum votes
            SupressLocalNonMaximumVotes(
                votesAccumulator: votesAccumulator,
                windowSize: 7
            );

            // 3. Filter out the first nth highest vote and return them as IEnumerable<LinePolar> descending by votes
            IEnumerable<LinePolar> topNLines = SelectTopNLines(
                votesAccumulator: votesAccumulator,
                numberOfTopLines: numberOfTopLines,
                deltaRadius: deltaRadius,
                deltaTheta: deltaTheta
            );

            // return the selected top N lines
            return topNLines;
        }

        public static void ViewVotes(Bitmap source, ref Bitmap destination)
        {
            // 0.1 create edge detected image reference
            Bitmap edgeDetectedImage = new Bitmap(
                width: source.Width,
                height: source.Height,
                format: PixelFormat.Format8bppIndexed
            );

            // 1. Apply canny edge detection on the source image and save the result into edgeDetectedImage 
            Filters.ApplyCanny(
                source: source,
                destination: ref edgeDetectedImage
            );

            // 2.0 create local variables with values for later usage
            // measured in pixel
            double deltaRadius = 5;
            // measured in radian - 180 / 50 = 3.6 degree
            double deltaTheta = Math.PI / 100;

            // 2. Create Accumulator array and fill with votes based on Canny edge detected image
            ComputeVoting(
                source: edgeDetectedImage,
                deltaRadius: deltaRadius,
                deltaTheta: deltaTheta,
                votesAccumulator: out Matrix votesAccumulator
            );

            // supress locally the non-maximum votes
            SupressLocalNonMaximumVotes(
                votesAccumulator: votesAccumulator,
                windowSize: 7
            );

            // write the accumulator's state into the destination image
            WriteAccumulatorIntoImage(
                votesAccumulator: votesAccumulator,
                destination: ref destination
            );
        }


        // PRIVATE METHODS
        /// <summary>
        /// Process the voting based on the edge detected image. Creates an accumulator 2d double array which contains the votes.
        /// Accumulator[row = Radius, col = Theta]. Radius = [0, sqrt(h^2 + w^2)] pixel. Theta = [-PI/2, PI/2] rad.
        /// </summary>
        /// <param name="source">8 bit depth, edge detected binary image (0 or 255 value only)</param>
        /// <param name="deltaRadius">Measured in pixel. sqrt(h^2 + w^2) = Nr * deltaRadius, where N is the number of different values in the range of (Radius =) [0, sqrt(h^2 + w^2)]</param>
        /// <param name="deltaTheta">Measured in Radian. PI = N * deltaTheta, where Nt is the number of different values in the range of (Theta =) [-PI/2, PI/2]</param>
        /// <param name="votesAccumulator">Accumulator[row = Radius, col = Theta]. Radius = [0, sqrt(h^2 + w^2)]. Theta = [-PI/2, PI/2].</param>
        private static unsafe void ComputeVoting(Bitmap source, double deltaRadius, double deltaTheta, out Matrix votesAccumulator)
        {
            // lock bitmap in the memory
            BitmapData bitmapDataSource = source.LockBits(
                rect: new Rectangle(new Point(0, 0), source.Size),
                flags: ImageLockMode.ReadOnly,
                format: PixelFormat.Format8bppIndexed
            );

            // save variables locally for faster access
            int height = bitmapDataSource.Height;
            int width = bitmapDataSource.Width;
            int stride = bitmapDataSource.Stride;
            byte* ptrSrc0 = (byte*)bitmapDataSource.Scan0;

            // calculate values are necessary for computation
            int diagonalImageLength = (int)Math.Sqrt(height * height + width * width);
            int numberOfRadiuses = (int)(diagonalImageLength / deltaRadius);
            int numberOfThetas = (int)((Math.PI) / deltaTheta);

            // create accumulator 2d array
            //      row direction -> Radius values
            //      col direction -> Theta values
            votesAccumulator = new Matrix()
            {
                Values = new double[numberOfRadiuses, numberOfThetas]
            };

            // walk through the image and if there is an edge point, vote based on the coordinates of the edge point
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    if (ptrSrc0[row * stride + col] == 255)
                    {
                        VoteForCoordinates(
                            votesAccumulator: votesAccumulator,
                            row: row,
                            col: col,
                            deltaRadius: deltaRadius,
                            deltaTheta: deltaTheta
                        );
                    }
                }
            }

            // unlock the source bitmap
            source.UnlockBits(bitmapDataSource);
        }
        private static void VoteForCoordinates(Matrix votesAccumulator, int row, int col, double deltaRadius, double deltaTheta)
        {
            // calculate the start and end values for loops
            double thetaStart = 0;
            double thetaEnd = Math.PI;
            int rows = votesAccumulator.Rows;
            int cols = votesAccumulator.Cols;

            // theta loop
            for (double theta = thetaStart; theta <= thetaEnd; theta += deltaTheta)
            {
                // compute radius
                double radius = col * Math.Cos(theta) + row * Math.Sin(theta);

                // compute voting indexes for the given coordinates (row, col) and the current theta value
                int rowIndex = (int)(radius / deltaRadius);
                int colIndex = (int)((theta) / deltaTheta);

                if (rowIndex >= 0 && rowIndex < rows && colIndex >= 0 && colIndex < cols)
                {
                    // increment the votings in the cell of computed indeces
                    votesAccumulator[rowIndex, colIndex]++;
                }
            }
        }
        private static void SupressLocalNonMaximumVotes(Matrix votesAccumulator, int windowSize)
        {
            // calculate the loop sizes
            int rows = votesAccumulator.Rows - windowSize + 1;
            int cols = votesAccumulator.Cols - windowSize + 1;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // variable to find the 
                    double windowMax = 0;

                    // window loop
                    for (int windowRow = 0; windowRow < windowSize; windowRow++)
                    {
                        for (int windowCol = 0; windowCol < windowSize; windowCol++)
                        {
                            if (votesAccumulator[row: row + windowRow, col: col + windowCol] > windowMax)
                            {
                                windowMax = votesAccumulator[row: row + windowRow, col: col + windowCol];
                            }
                        }
                    }

                    // suppress any non maximum values
                    for (int windowRow = 0; windowRow < windowSize; windowRow++)
                    {
                        for (int windowCol = 0; windowCol < windowSize; windowCol++)
                        {
                            if (votesAccumulator[row: row + windowRow, col: col + windowCol] != windowMax)
                            {
                                votesAccumulator[row: row + windowRow, col: col + windowCol] = 0;
                            }
                        }
                    }
                }
            }
        }
        private static IEnumerable<LinePolar> SelectTopNLines(Matrix votesAccumulator, int numberOfTopLines, double deltaRadius, double deltaTheta)
        {
            var topNVotes = votesAccumulator
                .OrderByDescending(value => value)
                .Take(numberOfTopLines)
                .ToArray();

            // Tuple.Item1 = number of votes
            // Tuple.Item2 = LinePolar
            List<Tuple<double, LinePolar>> linePolarsWithVotes = new List<Tuple<double, LinePolar>>();

            // save local values for faster access
            int rows = votesAccumulator.Rows;
            int cols = votesAccumulator.Cols;

            // walk through the accumulator matrix to find the top N votes and the related row and col indeces
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (topNVotes.Contains(votesAccumulator[row, col]))
                    {
                        // calculate the radius and the theta based on the row and col indeces
                        double radius = row * deltaRadius;
                        double theta = (col * deltaTheta);

                        // add the LinePolar to the result collection with votes
                        linePolarsWithVotes.Add(
                            Tuple.Create(
                                item1: votesAccumulator[row, col],
                                item2: new LinePolar()
                                {
                                    Radius = radius,
                                    Theta = theta,
                                }
                            )
                        );
                    }
                }
            }

            // return the nth top lines in descending order by votes
            //      Tuple.Item1 = number of votes
            //      Tuple.Item2 = LinePolar
            return linePolarsWithVotes
                .OrderByDescending(tuple => tuple.Item1)
                .Select(tuple => tuple.Item2);
        }

        private static unsafe void WriteAccumulatorIntoImage(Matrix votesAccumulator, ref Bitmap destination)
        {
            // create destination image with the correct size
            destination = new Bitmap(width: votesAccumulator.Cols, height: votesAccumulator.Rows, format: PixelFormat.Format32bppArgb);

            // lock bitmap
            BitmapData bitmapDataDest = destination.LockBits(
                rect: new Rectangle(0, 0, votesAccumulator.Cols, votesAccumulator.Rows),
                flags: ImageLockMode.ReadWrite,
                format: PixelFormat.Format24bppRgb
            );

            // get data locally for faster access
            int height = destination.Height;
            int width = destination.Width;
            int stride = bitmapDataDest.Stride;
            int pixFormat = stride / width;
            byte* ptrDest0 = (byte*)bitmapDataDest.Scan0;

            // get the maximum numer of votes fo scaling
            double maxVotes = votesAccumulator.Max();

            // copy the content of the accumulator into the image
            Parallel.For(fromInclusive: 0, toExclusive: height, body: (row) =>
            {
                for (int col = 0; col < width; col++)
                {
                    // create pointer to the current pixel
                    byte* ptrDest = (byte*)(ptrDest0 + row * stride + col * pixFormat);

                    // calculate the scale pixel value
                    byte value = (byte)((votesAccumulator[row, col] / maxVotes) * 255);

                    // set the scaled value in the destination image
                    for (int channel = 0; channel < 3; channel++)
                    {
                        ptrDest[channel] = value;
                    }
                }
            });

            // unlock bitmap
            destination.UnlockBits(bitmapDataDest);
        }
    }
}
