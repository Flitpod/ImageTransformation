using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Processors
{
    public static class BitmapExtender
    {
        public static unsafe void SetTo(this Bitmap bmp, byte red = 0, byte green = 0, byte blue = 0)
        {
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            int height = bmp.Height;
            int width = bmp.Width;
            int stride = bitmapData.Stride;
            byte* ptr0 = (byte*)bitmapData.Scan0;
            int pixelFormat = 4;
            Parallel.For(0, height, (row) =>
            {
                for (int col = 0; col < width; col++)
                {
                    byte* ptr = (byte*)(ptr0 + row * stride + col * pixelFormat);
                    ptr[0] = blue;
                    ptr[1] = green;
                    ptr[2] = red;
                }
            });
            bmp.UnlockBits(bitmapData);
        }
    }

}
