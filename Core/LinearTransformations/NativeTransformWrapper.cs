using ImageTransformation.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.LinearTransformations
{
    public class NativeTransformWrapper
    {
        [DllImport("NativeCore.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ExecuteHomographyTransformation(
            int matrixRows,
            int matrixCols,
            IntPtr matrixValues,
            IntPtr imgSrc,
            IntPtr imgDst,
            int pixelFormat,
            int height,
            int width,
            int stride
        );

        public static unsafe void Execute(Matrix transformation, Bitmap source, Bitmap destination)
        {
            // lock bitmaps and get image data
            PixelFormat format = source.PixelFormat;
            BitmapData sourceBitmapData = source.LockBits(
                rect: new Rectangle(0, 0, source.Width, source.Height),
                flags: ImageLockMode.ReadOnly,
                format: format);
            BitmapData destinationBitmapData = destination.LockBits(
                rect: new Rectangle(0, 0, source.Width, source.Height),
                flags: ImageLockMode.ReadWrite,
                format: format);

            try
            {
                int height = sourceBitmapData.Height;
                int width = sourceBitmapData.Width;
                int stride = sourceBitmapData.Stride;
                int pixelFormat = format switch
                {
                    PixelFormat.Format24bppRgb => 3,
                    _ => 4
                };

                // close transformation array
                double[] transformationValues = transformation.ToArray();
                fixed (double* matrixValues = transformationValues)
                {
                    // call the native core transformation function
                    int hresult = ExecuteHomographyTransformation(
                        matrixRows: 3,
                        matrixCols: 3,
                        matrixValues: (IntPtr)matrixValues,
                        imgSrc: sourceBitmapData.Scan0,
                        imgDst: destinationBitmapData.Scan0,
                        pixelFormat: pixelFormat,
                        height: height,
                        width: width,
                        stride: stride
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                // unlock bitmaps
                source.UnlockBits(sourceBitmapData);
                destination.UnlockBits(destinationBitmapData);
            }
        }
    }
}
