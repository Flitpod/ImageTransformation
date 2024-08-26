using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace App_wpf
{
    public static class BitmapConverter
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource GetBitmapSource(this Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource result = null;
            try
            {
                result = Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap: hBitmap,
                    palette: IntPtr.Zero,
                    sourceRect: Int32Rect.Empty,
                    sizeOptions: BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception) { }
            finally
            {
                DeleteObject(hBitmap);
            }

            return result;
        }
    }
}
