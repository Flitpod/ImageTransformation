using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageTransformation.Core;
using System.IO.MemoryMappedFiles;

namespace Core.Processors
{
    internal class CannyProcessor
    {
        internal static void NullCheckImages(
            Bitmap source,
            ref Bitmap destination
        )
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (destination == null || source.Width != destination.Width || source.Height != destination.Height || PixelFormat.Format8bppIndexed != destination.PixelFormat)
            {
                destination = new Bitmap(width: source.Width, height: source.Height, format: PixelFormat.Format8bppIndexed);
            }
        }

        internal static void LockBitmaps(
            Bitmap source,
            Bitmap destination,
            Bitmap buffer,
            out BitmapData bitmapDataSource,
            out BitmapData bitmapDataDestination,
            out BitmapData bitmapDataBuffer
        )
        {
            // source image lock in memory
            bitmapDataSource = source.LockBits(
                rect: new Rectangle(new Point(0, 0), source.Size),
                flags: ImageLockMode.ReadOnly,
                format: source.PixelFormat
            );

            // destination image lock in memory
            bitmapDataDestination = destination.LockBits(
                rect: new Rectangle(new Point(0, 0), destination.Size),
                flags: ImageLockMode.ReadWrite,
                format: destination.PixelFormat
            );

            // buffer image lock in memory
            bitmapDataBuffer = buffer.LockBits(
                rect: new Rectangle(new Point(0, 0), buffer.Size),
                flags: ImageLockMode.ReadWrite,
                format: destination.PixelFormat
            );
        }

        /// <summary>
        /// Convert a 3 channel (rgb) image into an 8 bit depth grayscale image.
        /// </summary>
        /// <param name="source">24 or 32 bit depth RGB image</param>
        /// <param name="destination">8 bit depth grayscale image</param>
        internal static unsafe void ConvertToGray(
            Bitmap source,
            Bitmap destination,
            BitmapData bitmapDataSource,
            BitmapData bitmapDataDestination
        )
        {
            // get info locally for faster access
            byte* ptrSrc0 = (byte*)bitmapDataSource.Scan0;
            byte* ptrDest0 = (byte*)bitmapDataDestination.Scan0;
            int strideSrc = (int)bitmapDataSource.Stride;
            int strideDest = (int)bitmapDataDestination.Stride;
            int witdh = (int)bitmapDataSource.Width;
            int height = (int)bitmapDataSource.Height;
            int pixFormatSrc = strideSrc / witdh;
            int pixFormatDest = strideDest / witdh;

            // generate lut for transformation
            byte[] lutGray = new byte[3 * 256];
            for (int i = 0; i < 3 * 256; i++)
            {
                lutGray[i] = (byte)(i / 3);
            }

            // process the image parallel
            // column loop
            Parallel.For(0, height, (row) =>
            {
                // row loop
                for (int col = 0; col < witdh; col++)
                {
                    // create pointers to source data and destination data address
                    byte* ptrSrc = (byte*)(ptrSrc0 + row * strideSrc + col * pixFormatSrc);
                    byte* ptrDest = (byte*)(ptrDest0 + row * strideDest + col * pixFormatDest);

                    // calculate the gray value from rgb values for each pixel
                    byte grayvalue = lutGray[ptrSrc[0] + ptrSrc[1] + ptrSrc[2]];

                    // set the calculated gray value in the destination image
                    ptrDest[0] = grayvalue;
                }
            });
        }

        /// <summary>
        /// Apply binomial gauss blur with 3x3 kernel on the 8 bit depth grayscale image
        /// </summary>
        /// <param name="source">8 bit depth, grayscale image</param>
        /// <param name="destination">8 bit depth, grayscale image</param>
        /// <param name="bitmapDataSource"></param>
        /// <param name="bitmapDataDestination"></param>
        internal unsafe static void GaussBlur(
            Bitmap source,
            Bitmap destination,
            BitmapData bitmapDataSource,
            BitmapData bitmapDataDestination
        )
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
            int filterScaleFactor = (int)filter.Sum();

            // get info locally for faster access
            int strideSrc = (int)bitmapDataSource.Stride;
            int strideDest = (int)bitmapDataDestination.Stride;
            byte* ptrSrc0 = (byte*)bitmapDataSource.Scan0;
            byte* ptrDest0 = (byte*)(bitmapDataDestination.Scan0 + strideDest + 1);
            int width = (int)bitmapDataSource.Width - filter.Cols + 1;
            int height = (int)bitmapDataSource.Height - filter.Rows + 1;

            // row loop
            Parallel.For(fromInclusive: 0, toExclusive: height, body: (row) =>
            {
                // col loop
                for (int col = 0; col < width; col++)
                {
                    // create pointers to source data and destination data address
                    byte* ptrSrc = (byte*)(ptrSrc0 + row * strideSrc + col);
                    byte* ptrDest = (byte*)(ptrDest0 + row * strideDest + col);

                    // create buffer for results
                    double value = 0;

                    // store locally the size of the filter window
                    int windowHeight = filter.Rows;
                    int windowWidth = filter.Cols;

                    // apply filter factors on input image data
                    for (int wRow = 0; wRow < windowHeight; wRow++)
                    {
                        for (int wCol = 0; wCol < windowWidth; wCol++)
                        {
                            value += ptrSrc[wRow * strideSrc + wCol] * filter[wRow, wCol];
                        }
                    }

                    // set the convolved filter value in the destination image
                    ptrDest[0] = (byte)(value / filterScaleFactor);
                }
            });
        }

        /// <summary>
        /// Compute the sobel edge gradient values and directions. 
        /// The gradient values/lengths will be stored in the destination image.
        /// The gradient directions are mapped values based on 4 different gradient directions, gives back the map.
        /// The method also gives back the maximum value/length of gradients.
        /// </summary>
        /// <param name="source">8 bit depth, grayscale image</param>
        /// <param name="destination">8 bit depth, grayscale image, includes only sobel detected edges</param>
        /// <param name="bitmapDataSource"></param>
        /// <param name="bitmapDataDestination"></param>
        /// <param name="gradientDirectionMap">Map with the same size as the image size without the frame. 
        /// Stores gradient directions { 0,1,2,3 }.
        /// </param>
        /// <param name="gradientMaxValue">Maximum gradient value/length.</param>
        internal static unsafe void ApplySobel(
            Bitmap source,
            Bitmap destination,
            BitmapData bitmapDataSource,
            BitmapData bitmapDataDestination,
            out int[,] gradientDirectionMap,
            out int gradientMaxValue
        )
        {
            #region Description
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
            #endregion

            // get info locally for faster access
            int height = source.Height - 2;
            int width = source.Width - 2;
            int stride = bitmapDataSource.Stride;
            byte* ptrSrc0 = (byte*)bitmapDataSource.Scan0;
            byte* ptrDest0 = (byte*)(bitmapDataDestination.Scan0 + stride + 1);

            // result direction map initialization
            int[,] directionMap = new int[height, width];

            // local maximum storing collection
            ConcurrentBag<int> localMaximums = new ConcurrentBag<int>();

            // create local variables are used frequently
            const double directionUnit = Math.PI / 8;
            double sqrtOf2 = Math.Sqrt(2);
            int localMax = 0;

            // parallel calculation
            Parallel.For(fromInclusive: 0, toExclusive: height,

                // execute once before loop starts 
                localInit: () => 0,

                // execute in every loop
                body: (row, loopState, localMax) =>
                {
                    for (int col = 0; col < width; col++)
                    {
                        // pointer to current pixels
                        byte* ptrSrc = (byte*)(ptrSrc0 + row * stride + col);
                        byte* ptrDest = (byte*)(ptrDest0 + row * stride + col);

                        // calculate x direction gradient value an divide it with 4 (>>2)
                        int gradX =
                            ptrSrc[0]
                            + 2 * ptrSrc[stride]
                            + ptrSrc[2 * stride]
                            - ptrSrc[2]
                            - 2 * ptrSrc[stride + 2]
                            - ptrSrc[2 * (stride + 1)];
                        gradX >>= 2;

                        // calculate y direction gradient value an divide it with 4 (>>2)
                        int gradY =
                            ptrSrc[0]
                            + 2 * ptrSrc[1]
                            + ptrSrc[2]
                            - ptrSrc[2 * stride]
                            - 2 * ptrSrc[2 * stride + 1]
                            - ptrSrc[2 * (stride + 1)];
                        gradY >>= 2;

                        // calculate gradient length and save it into the destination image
                        int gradLength = (int)(Math.Sqrt(gradX * gradX + gradY * gradY) / sqrtOf2);
                        ptrDest[0] = (byte)gradLength;

                        // checks if the current value is bigger than the already find local maximum
                        localMax = Math.Max(localMax, gradLength);

                        // calculate gradient direction
                        double direction = Math.Atan2(y: gradY, x: gradX);

                        // limit direction into the range of [0,180] degree, [0, PI] rad
                        if (direction > Math.PI)
                        {
                            direction -= Math.PI;
                        }

                        // find gradient map value, directionUnit is PI / 8
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
                    // return the thread local maximum value
                    return localMax;
                },

                // execute once after loop ends
                localFinally: (localMax) => localMaximums.Add(localMax)
            );

            // set the gradientDirectionMap reference to the created directionMap
            gradientDirectionMap = directionMap;

            // initialize the maximum gradient out parameter with the value of the maximum gradient length
            gradientMaxValue = localMaximums.Max();
        }

        internal static unsafe void SetFrameToZero(
            Bitmap image,
            BitmapData bitmapDataImage,
            int frameDepth = 2
        )
        {
            // get info locally for faster access
            int height = image.Height;
            int width = image.Width;
            int stride = bitmapDataImage.Stride;
            byte* ptrImg0 = (byte*)bitmapDataImage.Scan0;
            long imageLength = height * stride - 1;
            for (int depth = 0; depth < frameDepth; depth++)
            {
                // top and bottom rows
                for (int col = 0; col < width; col++)
                {
                    // upper row
                    ptrImg0[depth * stride + col] = 0;

                    // bottom row
                    ptrImg0[imageLength - (depth * stride + col)] = 0;
                }

                // left and right cols
                for (int row = 0; row < height; row++)
                {
                    // left col
                    ptrImg0[row * stride + depth] = 0;

                    // right col
                    // ptrImg0[imageLength - (row * stride + depth) - 1] = 0;
                    ptrImg0[imageLength - (row * stride + depth)] = 0;
                }
            }
        }

        internal static unsafe void SuppressNonMaximums(
            Bitmap source,
            Bitmap destination,
            BitmapData bitmapDataSource,
            BitmapData bitmapDataDestination,
            int[,] gradientDirectionMap
        )
        {
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
            //
            //
            //
            // Gradient direction map interpretation
            //
            //      1  2  3
            //      0  p  0
            //      3  2  1
            //
            // Algorithm description:
            //  - p is the current pixel (x, y)
            //  - in the given direction must be checked the neighbouring values
            //  - if there is higher value than value of p, then p must be suppressed (0 value)
            //  - else the value of p must be preserved
            // 
            // Implementation explanation:
            //  - an offset is introduced for getting the correct neighbouring pixels due to the direction map value
            // 
            // Table of corresponding offsets:
            //      --------------------------------------------------
            //      | Dir || rowLeft | colLeft | rowRight | colRight |
            //      --------------------------------------------------
            //      |  0  ||    0    |   -1    |     0    |     1    |
            //      |  1  ||   -1    |   -1    |     1    |     1    |
            //      |  2  ||   -1    |    0    |     1    |     0    |
            //      |  3  ||    1    |   -1    |    -1    |     1    |
            #endregion

            // get info locally for faster access
            int height = source.Height - 2;
            int width = source.Width - 2;
            int stride = bitmapDataSource.Stride;
            byte* ptrSrc0 = (byte*)(bitmapDataSource.Scan0 + stride + 1);
            byte* ptrDest0 = (byte*)(bitmapDataDestination.Scan0 + stride + 1);

            // parallel process the image an suppress the non maximum values in normal direction
            Parallel.For(fromInclusive: 0, toExclusive: height, (row) =>
            {
                for (int col = 0; col < width; col++)
                {
                    // create pointers to the current pixels in the source and destination image
                    byte* ptrSrc = (byte*)(ptrSrc0 + row * stride + col);
                    byte* ptrDest = (byte*)(ptrDest0 + row * stride + col);

                    // get spatial offset values due to gradient direction map value
                    var (rowLeft, colLeft, rowRight, colRight) = (gradientDirectionMap[row, col]) switch
                    {
                        0 => (0, -1, 0, 1),
                        1 => (-1, -1, 1, 1),
                        2 => (-1, 0, 1, 0),
                        3 => (1, -1, -1, 1),
                    };

                    // calculate memory offset values based on spatial offset values
                    int offsetLeft = rowLeft * stride + colLeft;
                    int offsetRight = rowRight * stride + colRight;

                    // suppress the non maximum values
                    byte currentValue = ptrSrc[0];
                    if (currentValue < ptrSrc[offsetLeft] || currentValue < ptrSrc[offsetRight])
                    {
                        // the current pixel from the source image has a value smaller than neighbouring pixels, set it to 0
                        currentValue = 0;
                    }

                    // set the destination pixel value to the calulated value
                    ptrDest[0] = currentValue;
                }
            });
        }

        /// <summary>
        /// Applies double/hysteresis threshold on the image. Strong edge pixels will have value of 255, and weak edge pixels will have the value of 128.
        /// </summary>
        /// <param name="source">Bitmap image</param>
        /// <param name="destination">Bitmap image</param>
        /// <param name="bitmapDataSource">BitmapData for source image</param>
        /// <param name="bitmapDataDestination">BitmapData for destination image</param>
        /// <param name="thresholdLow">Lower threshold, below this the corresponding pixel value will be 0. Above the threshold the value will be 128 as marked weak edge</param>
        /// <param name="thresholdHigh">Above or equal this the pixel value will be 255.</param>
        internal static unsafe void ApplyDoubleThreshold(
            Bitmap source,
            Bitmap destination,
            BitmapData bitmapDataSource,
            BitmapData bitmapDataDestination,
            int gradientMaxValue,
            double thresholdLowRatio = 0.25,
            double thresholdHighRatio = 0.9
        )
        {
            // calculate the threshold values
            int thresholdLow = (int)(gradientMaxValue * thresholdLowRatio);
            int thresholdHigh = (int)(gradientMaxValue * thresholdHighRatio);

            // get info locally for faster access
            int height = source.Height;
            int width = source.Width;
            int stride = bitmapDataSource.Stride;
            byte* ptrSrc0 = (byte*)bitmapDataSource.Scan0;
            byte* ptrDest0 = (byte*)bitmapDataDestination.Scan0;

            // Process double threshold on the image
            Parallel.For(fromInclusive: 0, toExclusive: height, (row) =>
            {
                for (int col = 0; col < width; col++)
                {
                    // pointer to current pixels
                    byte* ptrSrc = (byte*)(ptrSrc0 + row * stride + col);
                    byte* ptrDest = (byte*)(ptrDest0 + row * stride + col);

                    // apply double threshold
                    byte currentValue = ptrSrc[0];
                    if (currentValue >= thresholdHigh)
                    {
                        currentValue = 255;
                    }
                    else if (currentValue >= thresholdLow)
                    {
                        currentValue = 128;
                    }
                    else
                    {
                        currentValue = 0;
                    }

                    // set the calulated pixel value in the destination image based on the thresholds
                    ptrDest[0] = currentValue;
                }
            });
        }

        /// <summary>
        /// This method convolve the source image with a 3x3 kernel.
        /// - When find a strong pixel, write it to the destination image
        /// - When find a weak pixel, then:
        ///      - If there is at least 1 strong pixel in the kernel, then the result pixel will be set to 255
        ///      - If there is no strong pixel in the kernel, then the result pixel will be set to 255
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="bitmapDataSource"></param>
        /// <param name="bitmapDataDestination"></param>
        internal static unsafe void ProcessWeakAndStrongEdges(
            Bitmap source,
            Bitmap destination,
            BitmapData bitmapDataSource,
            BitmapData bitmapDataDestination
        )
        {
            //  DESCRIPTION
            //
            // This method convolve the source image with a 3x3 kernel.
            // - When find a strong pixel, write it to the destination image
            // - When find a weak pixel, then:
            //      - If there is at least 1 strong pixel in the kernel, then the result pixel will be set to 255
            //      - If there is no strong pixel in the kernel, then the result pixel will be set to 255

            // get info locally for faster access
            int height = source.Height - 2;
            int width = source.Width - 2;
            int stride = bitmapDataSource.Stride;
            byte* ptrSrc0 = (byte*)(bitmapDataSource.Scan0 + stride + 1);
            byte* ptrDest0 = (byte*)(bitmapDataDestination.Scan0 + stride + 1);

            // process the image parallel
            Parallel.For(fromInclusive: 0, toExclusive: height, body: (row) =>
            {
                for (int col = 0; col < width; col++)
                {
                    // pointer to current pixels
                    byte* ptrSrc = (byte*)(ptrSrc0 + row * stride + col);
                    byte* ptrDest = (byte*)(ptrDest0 + row * stride + col);

                    // get the current value
                    byte currentValue = ptrSrc[0];

                    // Investigate the current pixel value. Possible values are:
                    //  - 0 -> no action, write into destination image
                    //  - 255 -> no action, write into destination image
                    //  - 128 -> check around the current pixel, is there at least one strong pixel in the kernel
                    if (currentValue == 128)
                    {
                        // kernel row
                        for (int wRow = -1; wRow <= 1; wRow++)
                        {
                            // kernel col
                            for (int wCol = -1; wCol <= 1; wCol++)
                            {
                                // if at least 1 strong pixel
                                if (ptrSrc[wRow * stride + wCol] == 255)
                                {
                                    currentValue = 255;
                                }
                            }
                        }

                        // if there was no strong pixel in the kernel around the current pixel,
                        // then the destination pixel value must set to 0
                        if (currentValue == 128)
                        {
                            currentValue = 0;
                        }
                    }

                    // set the destination pixel value based on the checked and corrected value
                    ptrDest[0] = currentValue;
                }
            });
        }

        internal static void UnlockBitmaps(
            Bitmap source,
            Bitmap destination,
            Bitmap buffer,
            BitmapData bitmapDataSource,
            BitmapData bitmapDataDestination,
            BitmapData bitmapDataBuffer
        )
        {
            source.UnlockBits(bitmapDataSource);
            destination.UnlockBits(bitmapDataDestination);
            buffer.UnlockBits(bitmapDataBuffer);
        }
    }
}
