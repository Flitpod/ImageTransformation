using ImageTransformation.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public partial class Filters
    {
        /// <summary>
        /// Apply canny edge detection on the image and save the result into the destination image
        /// </summary>
        /// <param name="source">RGB 8 bit depth bitmap image</param>
        /// <param name="destination">8 bit depth binary image (3 channel) with 2 value: 0 - background, 255 - foreground / edge</param>
        public static void ApplyCanny(Bitmap source, Bitmap destination)
        {
            // STEPS
            // 1. rgb -> gray
            // 2. Gauss filter for noise reduction
            // 3. Sobel X and Y with stored gradient directions
            // 4. non maximum edge pixels supression in normal direction to the edge
            // 5. double threshold on edges -> hysteresis bandwidth (25, 255)
            // 6. convolve weak and strong edges with window and glue strong edges together

            // 1. convert to gray image
            Filters.ConvertToGray(
                source: source, 
                destination: destination    
            );

            // create buffer for latter calculations
            Bitmap buffer = new Bitmap(destination);

            // 2. apply gauss filter
            GaussBlur(
                source: destination, 
                destination: buffer
            );

            // lock the bitmaps
            LockBitmaps(
                destination: destination, 
                buffer: buffer, 
                out BitmapData bitmapDataDestination, 
                out BitmapData bitmapDataBuffer
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
            ApplySobel(
                source: buffer, 
                destination: destination, 
                bitmapDataSource: bitmapDataBuffer,
                bitmapDataDestination: bitmapDataDestination,
                gradientDirectionMap: out int[,] gradientDirectionMap
            );

            // 4. Non maximums suppression in norm direction


            // set the frame to value 0


            // Unlock bitmaps
            UnlockBitmaps(destination: destination, buffer: buffer, bitmapDataDestination: bitmapDataDestination, bitmapDataBuffer: bitmapDataBuffer);
        }


        private static void GaussBlur(Bitmap source, Bitmap destination)
        {
            // create binomial gauss filter 3x3
            Matrix filter = new Matrix()
            {
                Values = new double[3, 3]
                {
                    { 1, 2, 1 },
                    { 2, 4, 2 },
                    { 1, 2, 1 },
                }
            };

            // apply gauss filter
            Filters.ApplyFilter(
                source: source,
                destination: destination,
                filter: filter,
                filterScaleFactor: filter.Sum()
            );
        }
        private static void LockBitmaps(Bitmap destination, Bitmap buffer, out BitmapData bitmapDataDestination, out BitmapData bitmapDataBuffer)
        {
            bitmapDataDestination = destination.LockBits(
                rect: new Rectangle(new Point(0, 0), destination.Size), 
                flags: ImageLockMode.ReadWrite, 
                format: PixelFormat.Format32bppRgb
            );

            bitmapDataBuffer = buffer.LockBits(
                rect: new Rectangle(new Point(0, 0), buffer.Size),
                flags: ImageLockMode.ReadWrite,
                format: PixelFormat.Format32bppRgb
            );
        }
        private static void UnlockBitmaps(Bitmap destination, Bitmap buffer, BitmapData bitmapDataDestination, BitmapData bitmapDataBuffer)
        {
            destination.UnlockBits(bitmapDataDestination);
            buffer.UnlockBits(bitmapDataDestination);
        }
        private static unsafe void ApplySobel(Bitmap source, Bitmap destination, BitmapData bitmapDataSource, BitmapData bitmapDataDestination, out int[,] gradientDirectionMap)
        {
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
            // direction look up table
            //
            // # 0 +- 22.5 -> 0
            // # 45 +- 22.5 -> 1
            // # 90 +- 22.5 -> 2
            // # 135 +- 22.5 -> 3

            int[,] directionMap = new int[source.Height, source.Width];

            // get info locally for faster access
            int height = source.Height - 2;
            int width = source.Width - 2;
            int stride = bitmapDataSource.Stride;
            int pixFormat = (int)bitmapDataSource.PixelFormat / 8;
            byte* ptrSrc0 = (byte*)bitmapDataSource.Scan0;
            byte* ptrDest0 = (byte*)(bitmapDataSource.Scan0 + stride + pixFormat);

            // parallel calculation
            Parallel.For(fromInclusive: 0,toExclusive: height, (row) =>
            {
                for (int col = 0; col < width; col++)
                {
                    // pointer to current pixels
                    byte* ptrSrc = (byte*)(ptrSrc0 + row * stride + col * pixFormat);
                    byte* ptrDest = (byte*)(ptrDest0 + row * stride + col * pixFormat);

                    // calculate x direction gradient value an divide it with 4 (>>2)
                    int gradX =
                        ptrSrc[0]
                        + 2 * ptrSrc[stride]
                        + ptrSrc[2 * stride]
                        - ptrSrc[2 * pixFormat]
                        - 2 * ptrSrc[stride + 2 * pixFormat]
                        - ptrSrc[2 * (stride + pixFormat)];
                    gradX >>= 2;

                    // calculate y direction gradient value an divide it with 4 (>>2)
                    int gradY =
                        ptrSrc[0]
                        + 2 * ptrSrc[pixFormat]
                        + ptrSrc[2 * pixFormat]
                        - ptrSrc[2 * stride]
                        - 2 * ptrSrc[2 * stride + pixFormat]
                        - ptrSrc[2 * (stride + pixFormat)];
                    gradY >>= 2;

                    // calculate gradient length
                    ptrDest[0] = ptrDest[1] = ptrDest[2] =
                        (byte)(Math.Sqrt(gradX * gradX + gradY * gradY) / Math.Sqrt(2));

                    // calculate gradient direction
                    double direction = Math.Atan2(y: gradY, x: gradX);

                    // limit direction into the range of [0,180] degree, [0, PI] rad
                    if (direction > Math.PI)
                    {
                        direction -= Math.PI;
                    }

                    // find gradient map value
                    const double directionUnit = Math.PI / 8;
                    int gradMapDirection = (direction) switch
                    {
                        >= directionUnit and < 3 * directionUnit => 1,
                        >= 3 * directionUnit and < 5 * directionUnit => 2,
                        >= 5 * directionUnit and < 7 * directionUnit => 3,
                        _ => 0,
                    };

                    // TODO - might need lock
                    directionMap[row, col] = gradMapDirection;
                }
            });

            // set the gradientDirectionMap reference to the created directionMap
            gradientDirectionMap = directionMap;
        }

    }
}
