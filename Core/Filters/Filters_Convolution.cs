using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using ImageTransformation.Core;

namespace Core
{
    public partial class Filters
    {
        /// <summary>
        /// Convolves the filter on the source image and save it into destination bitmap image
        /// </summary>
        /// <param name="source">Bitmap image, RGB format</param>
        /// <param name="destination">Bitmap, RGB format, every channel contains the same value since grayscale image</param>
        /// <param name="filter">Core.Matrix type, must be nxn size and n must be odd</param>
        /// <param name="filterScaleFactor">Scale factor to compensate the effect of filter values during convolution, 
        /// e.g. preserve the intensity value.</param>
        /// <exception cref="ArgumentNullException">Source image</exception>
        /// <exception cref="ArgumentException">Destination image</exception>
        public static unsafe void ApplyFilter(Bitmap source, ref Bitmap destination, Matrix filter, double filterScaleFactor)
        {
            //  null checks
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null || source.Width != destination.Width || source.Height != destination.Height || source.PixelFormat != destination.PixelFormat)
            {
                destination = new Bitmap(source.Width, source.Height, source.PixelFormat);
            }

            // lock the images in the memory during the image processing
            BitmapData bmData_src = source.LockBits(new Rectangle(new Point(0, 0), source.Size), ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData bmData_dest = destination.LockBits(new Rectangle(new Point(0, 0), destination.Size), ImageLockMode.ReadWrite, destination.PixelFormat);

            // save properties into variables are necessarry for process algorithm
            int offsetDest = filter.Rows / 2;
            int strideSrc = (int)bmData_src.Stride;
            int strideDest = (int)bmData_dest.Stride;
            int pixFormatSrc = strideSrc / bmData_src.Width;
            int pixFormatDest = strideDest / bmData_dest.Width;
            int width = (int)bmData_src.Width;
            int height = (int)bmData_src.Height;
            byte* ptrSrc0 = (byte*)bmData_src.Scan0;
            byte* ptrDest0 = (byte*)bmData_dest.Scan0;

            // copy the frame from the source image into the destination image
            long imageLengthSrc = height * strideSrc;
            long imageLengthDest = height * strideDest;

            // top and bottom rows
            for (int frameDepth = 0; frameDepth < offsetDest; frameDepth++)
            {
                for (int col = 0; col < width; col++)
                {
                    for (int channel = 0; channel < 3; channel++)
                    {
                        // upper rows
                        ptrDest0[frameDepth * strideDest + col * pixFormatDest + channel] =
                            ptrSrc0[frameDepth * strideSrc + col * pixFormatSrc + channel];

                        // bottom rows
                        ptrDest0[imageLengthDest - (frameDepth * strideDest + col * pixFormatDest + channel)] =
                            ptrSrc0[imageLengthSrc - (frameDepth * strideSrc + col * pixFormatSrc + channel)];
                    }
                }
            }

            // left and right cols
            for (int frameDepth = 0; frameDepth < offsetDest; frameDepth++)
            {
                for (int row = 0; row < height; row++)
                {
                    for (int channel = 0; channel < 3; channel++)
                    {
                        // left cols
                        ptrDest0[frameDepth * pixFormatDest + row * strideDest + channel] =
                            ptrSrc0[frameDepth * pixFormatDest + row * strideDest + channel];

                        // right cols
                        ptrDest0[imageLengthDest - (frameDepth * pixFormatDest + row * strideDest + channel + pixFormatDest + 1)] =
                            ptrSrc0[imageLengthSrc - (frameDepth * pixFormatSrc + row * strideSrc + channel + pixFormatSrc + 1)];
                    }
                }
            }

            // rearrange destination pointer start address on the start region of convolution
            ptrDest0 = (byte*)(ptrDest0 + offsetDest * strideDest + offsetDest * pixFormatDest);
            height = height - filter.Rows + 1;
            width = width - filter.Cols + 1;

            // process the image parallel
            // row loop
            Parallel.For(fromInclusive: 0, toExclusive: height, body: (row) =>
            {
                // col loop
                for (int col = 0; col < width; col++)
                {
                    // create pointers to source data and destination data address
                    byte* ptrSrc = (byte*)(ptrSrc0 + row * strideSrc + col * pixFormatSrc);
                    byte* ptrDest = (byte*)(ptrDest0 + row * strideDest + col * pixFormatDest);

                    // create buffer for results
                    double[] values = new double[3];

                    // store locally the size of the filter window
                    int windowHeight = filter.Rows;
                    int windowWidth = filter.Cols;

                    // apply filter factors on input image data
                    for (int channel = 0; channel < 3; channel++)
                    {
                        for (int wRow = 0; wRow < windowHeight; wRow++)
                        {
                            for (int wCol = 0; wCol < windowWidth; wCol++)
                            {
                                values[channel] += ptrSrc[wRow * strideSrc + wCol * pixFormatSrc + channel] * filter[wRow, wCol];
                            }
                        }
                    }

                    // set the convolved filter value in the destination image
                    for (int i = 0; i < 3; i++)
                    {
                        ptrDest[i] = (byte)(values[i] / filterScaleFactor);
                    }
                }
            });

            // unlock the bitmaps
            source.UnlockBits(bmData_src);
            destination.UnlockBits(bmData_dest);
        }
    }
}
