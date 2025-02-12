using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Filters
{
    public partial class Filters
    {
        /// <summary>
        /// Convert the input rgb image content into grayscale image and save it into destination bitmap image
        /// </summary>
        /// <param name="source">Bitmap image, RGB format</param>
        /// <param name="destination">Bitmap, RGB format, every channel contains the same value since grayscale image</param>
        public static unsafe void ConvertToGray(Bitmap source, Bitmap destination)
        {
            //  null checks
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null || source.Width != destination.Width || source.Height != destination.Height)
            {
                throw new ArgumentException(nameof(destination));
            }

            // lock the images in the memory during the image processing
            BitmapData bmData_src = source.LockBits(new Rectangle(new Point(0, 0), source.Size), ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData bmData_dest = destination.LockBits(new Rectangle(new Point(0, 0), destination.Size), ImageLockMode.WriteOnly, destination.PixelFormat);

            // save properties into variables are necessarry for process algorithm
            byte* ptrSrc0 = (byte*)bmData_src.Scan0;
            byte* ptrDest0 = (byte*)bmData_dest.Scan0;
            int strideSrc = (int)bmData_src.Stride;
            int strideDest = (int)bmData_dest.Stride;
            int pixFormatSrc = (int)bmData_src.PixelFormat / 3;
            int pixFormatDest = (int)bmData_dest.PixelFormat / 3;
            int witdh = (int)bmData_src.Width;
            int height = (int)bmData_src.Height;

            // generate lut for transformation
            byte[] lutGray = new byte[3 * 256];
            for (int i = 0; i < 3 * 256; i++)
            {
                lutGray[i] = (byte)(i / 3);
            }

            // process the image parallel
            // column loop
            Parallel.For(0, height, (col) =>
            {
                // row loop
                for (int row = 0; row < witdh; row++)
                {
                    // create pointers to source data and destination data address
                    byte* ptrSrc = (byte*)(ptrSrc0 + row * strideSrc + col * pixFormatSrc);
                    byte* ptrDest = (byte*)(ptrDest0 + row * strideDest + col * pixFormatDest);

                    // calculate the gray value from rgb values for each pixel
                    byte grayvalue = lutGray[ptrSrc[0] + ptrSrc[1] + ptrSrc[2]];

                    // set the calculated gray value in the destination image
                    for (int i = 0; i < 3; i++)
                    {
                        ptrDest[i] = grayvalue;
                    }
                }
            });

            // unlock the bitmaps
            source.UnlockBits(bmData_src);
            destination.UnlockBits(bmData_dest);
        }
    }
}
