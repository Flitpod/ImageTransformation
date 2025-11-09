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
        /// Convert the input rgb image content into YCrCb image and maps the max(Cr,Cb) as grayscale image for every pixel and save it into destination bitmap image
        /// </summary>
        /// <param name="source">Bitmap image, RGB format</param>
        /// <param name="destination">Bitmap, RGB format, every channel contains the same value since grayscale image</param>
        public static unsafe void ConvertToMaxCrCbGray(Bitmap source, ref Bitmap destination)
        {
            //  null checks
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null || source.Width != destination.Width || source.Height != destination.Height)
            {
                destination = new Bitmap(width: source.Width, height: source.Height, format: source.PixelFormat);
            }

            // lock the images in the memory during the image processing
            BitmapData bmData_src = source.LockBits(new Rectangle(new Point(0, 0), source.Size), ImageLockMode.ReadOnly, source.PixelFormat);
            BitmapData bmData_dest = destination.LockBits(new Rectangle(new Point(0, 0), destination.Size), ImageLockMode.WriteOnly, destination.PixelFormat);

            // save properties into variables are necessarry for process algorithm
            byte* ptrSrc0 = (byte*)bmData_src.Scan0;
            byte* ptrDest0 = (byte*)bmData_dest.Scan0;
            int strideSrc = (int)bmData_src.Stride;
            int strideDest = (int)bmData_dest.Stride;
            int witdh = (int)bmData_src.Width;
            int height = (int)bmData_src.Height;
            int pixFormatSrc = strideSrc / witdh;
            int pixFormatDest = strideDest / witdh;

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

                    // calculate the CrCb value from rgb values for each pixel                                        
                    // ┌─  ─┐       ┌─             ─┐┌─  ─┐   ┌─   ─┐        
                    // │ Y' │    1  │  66  129   25 ││ R' │   │ 16  │        
                    // │ Cr │ = --- │ -38  -74  112 ││ G' │ + │ 128 │        
                    // │ Cb │   256 │ 112  -94  -18 ││ B' │   │ 128 │        
                    // └─  ─┘       └─             ─┘└─  ─┘   └─   ─┘                                              
                    byte CrAbs = (byte)(Math.Abs(-38 * ptrSrc[2] - 74 * ptrSrc[1] + 112 * ptrSrc[0]) / 256);
                    byte CbAbs = (byte)(Math.Abs(112 * ptrSrc[2] - 94 * ptrSrc[1] - 18 * ptrSrc[0]) / 256);
                    byte grayvalue = CbAbs > CrAbs ? CbAbs : CrAbs;

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
