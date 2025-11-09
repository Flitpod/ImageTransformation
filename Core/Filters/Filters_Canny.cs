using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Processors;
using ImageTransformation.Core;

namespace Core
{
    public partial class Filters
    {
        /// <summary>
        /// Apply canny edge detection on the image and save the result into the destination image
        /// </summary>
        /// <param name="source">RGB 8 bit depth bitmap image</param>
        /// <param name="destination">8 bit depth, 1 channel, binary image with 2 value: 0 - background, 255 - foreground / edge</param>
        public static void ApplyCanny(Bitmap source, ref Bitmap destination)
        {
            // STEPS
            // 1. rgb -> gray
            // 2. Gauss filter for noise reduction
            // 3. Sobel X and Y with stored gradient directions
            // 4. non maximum edge pixels supression in normal direction to the edge
            // 5. double threshold on edges -> hysteresis bandwidth (25, 255)
            // 6. convolve weak and strong edges with window and glue strong edges together

            // 0.1 Check image sources
            CannyProcessor.NullCheckImages(
                source: source,
                destination: ref destination
            );

            // 0.2 Create buffer for latter calculations
            Bitmap buffer = new Bitmap(destination.Width, destination.Height, PixelFormat.Format8bppIndexed);

            // 0.3 Lock the bitmaps
            CannyProcessor.LockBitmaps(
                source: source,
                destination: destination,
                buffer: buffer,
                out BitmapData bitmapDataSource,
                out BitmapData bitmapDataDestination,
                out BitmapData bitmapDataBuffer
            );

            // 1. convert to gray image
            CannyProcessor.ConvertToGray(
                source: source,
                destination: destination,
                bitmapDataSource: bitmapDataSource,
                bitmapDataDestination: bitmapDataDestination
            );

            // 2. apply gauss filter
            CannyProcessor.GaussBlur(
                source: destination,
                destination: buffer,
                bitmapDataSource: bitmapDataDestination,
                bitmapDataDestination: bitmapDataBuffer
            );

            // 3. apply sobel x and y filter and save edge direction into gradient direct map
            #region direction description
            // create gradient direction buffer array
            // 0 ---------------- 0
            //         /|\
            // *     /  |  \      *
            //    /     |     \
            //  3       |       1
            //      *   2   *
            //
            // # if angle > 180 degree (PI) => angle -= 180
            //
            // direction look-up table
            //
            // # 0 +- 22.5 -> 0
            // # 45 +- 22.5 -> 1
            // # 90 +- 22.5 -> 2
            // # 135 +- 22.5 -> 3
            #endregion
            CannyProcessor.ApplySobel(
                source: buffer,
                destination: destination,
                bitmapDataSource: bitmapDataBuffer,
                bitmapDataDestination: bitmapDataDestination,
                gradientDirectionMap: out int[,] gradientDirectionMap,
                gradientMaxValue: out int gradientMaxValue
            );


            // set the frame to value 0
            CannyProcessor.SetFrameToZero(
                image: destination,
                bitmapDataImage: bitmapDataDestination,
                frameDepth: 4
            );

            // 4. Non maximums suppression in norm direction
            CannyProcessor.SuppressNonMaximums(
                source: destination,
                destination: buffer,
                bitmapDataSource: bitmapDataDestination,
                bitmapDataDestination: bitmapDataBuffer,
                gradientDirectionMap: gradientDirectionMap
            );

            // 5. double threshold on edges -> hysteresis bandwidth
            CannyProcessor.ApplyDoubleThreshold(
                source: buffer,
                destination: buffer,
                bitmapDataSource: bitmapDataBuffer,
                bitmapDataDestination: bitmapDataBuffer,
                gradientMaxValue: gradientMaxValue,
                thresholdLowRatio: 0.25,
                thresholdHighRatio: 0.75
            );

            // 6. convolve weak and strong edges with window and glue strong edges together
            CannyProcessor.ProcessWeakAndStrongEdges(
                source: buffer,
                destination: destination,
                bitmapDataSource: bitmapDataBuffer,
                bitmapDataDestination: bitmapDataDestination
            );

            // Unlock bitmaps
            CannyProcessor.UnlockBitmaps(
                source: source,
                destination: destination,
                buffer: buffer,
                bitmapDataSource: bitmapDataSource,
                bitmapDataDestination: bitmapDataDestination,
                bitmapDataBuffer: bitmapDataBuffer
            );
        }
    }
}
