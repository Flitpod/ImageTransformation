using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Core.Processors;
using ImageTransformation.Core;

namespace Core
{
    public enum InterpolationTypes
    {
        None, NativeBilinear, Floating_FromSource, Dir4_FromSource, Dir8_FromSource, Dir4_ToDestination, Dir8_ToDestination
    }

    public class TransformBitmap
    {
        public static unsafe void ExecuteBackward(Bitmap source, ref Bitmap result, Matrix transformation)
        {
            // null and size checks
            CheckBitmaps(source, ref result);

            // get the inverse of the transformation
            Matrix invTransformation = transformation.GetInverse();

            // get dimension information from transformation
            Dimension dimension = (Dimension)transformation.Rows;
            int dim = transformation.Rows;

            // calculation of the transformation
            {
                // get data for transformation
                int height = source.Height;
                int width = source.Width;
                BitmapData bitmapDataSrc = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                BitmapData bitmapDataDest = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                int stride = bitmapDataSrc.Stride;
                byte* ptrSrc0 = (byte*)bitmapDataSrc.Scan0;
                byte* ptrDest0 = (byte*)bitmapDataDest.Scan0;
                int pixelFormat = 4;

                Parallel.For(0, height, (row) =>
                //for(int row = 0; row < height; row++) 
                {
                    for (int col = 0; col < width; col++)
                    {
                        // pointer to the actual pixel on the destination bitmap
                        byte* ptrDest = (byte*)(ptrDest0 + row * stride + col * pixelFormat);

                        // get the vector contains the actual pixel coordinates
                        Matrix coordsDest = Matrix.GetVector(x: row, y: col, dimension: dimension);

                        // calcuate the destination cordinates (pixels)
                        Matrix coordsSrc = invTransformation * coordsDest;

                        // Calculate the coordinates of the source pixels
                        int rowSrc = (int)(coordsSrc[0, 0] + 0.5);
                        int colSrc = (int)(coordsSrc[1, 0] + 0.5);

                        if (rowSrc < 0 || rowSrc >= height || colSrc < 0 || colSrc >= width)
                        {
                            ptrDest[0] = 0;
                            ptrDest[1] = 0;
                            ptrDest[2] = 0;
                        }
                        else
                        {
                            byte* ptrSrc = (byte*)(ptrSrc0 + rowSrc * stride + colSrc * pixelFormat);
                            ptrDest[0] = ptrSrc[0];
                            ptrDest[1] = ptrSrc[1];
                            ptrDest[2] = ptrSrc[2];
                        }
                    }
                });

                source.UnlockBits(bitmapDataSrc);
                result.UnlockBits(bitmapDataDest);
            }
        }

        public static unsafe void ExecuteBackward(Bitmap source, ref Bitmap result, Matrix transformation, InterpolationTypes interpolationType = InterpolationTypes.Floating_FromSource, double weightCloserNeighbour = 0.25)
        {
            // null and size checks
            CheckBitmaps(source, ref result);

            // get the inverse of the transformation
            Matrix invTransformation = transformation.GetInverse();

            // get dimension information from transformation
            Dimension dimension = (Dimension)transformation.Rows;
            int dim = transformation.Rows;

            // if interpolation type is Floating_FromSource
            if (interpolationType == InterpolationTypes.Floating_FromSource)
            {
                // create bitmap from source with 1 pixel padding
                Bitmap sourcePadded = new Bitmap(source.Width + 2, source.Height + 2);

                // calculation of the transformation
                {
                    // get data for transformation
                    int heightR = result.Height;
                    int widthR = result.Width;
                    int heightS = sourcePadded.Height;
                    int widthS = sourcePadded.Width;
                    BitmapData bitmapDataSrcPadded = sourcePadded.LockBits(new Rectangle(0, 0, widthS, heightS), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                    BitmapData bitmapDataDest = result.LockBits(new Rectangle(0, 0, widthR, heightR), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                    int strideS = bitmapDataSrcPadded.Stride;
                    int strideR = bitmapDataDest.Stride;
                    byte* ptrSrc0 = (byte*)bitmapDataSrcPadded.Scan0;
                    byte* ptrDest0 = (byte*)bitmapDataDest.Scan0;
                    int pixelFormat = 4;

                    // copy the source image content into the padded image
                    {
                        BitmapData bitmapDataSrc = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                        byte* ptrSrcOrig0 = (byte*)bitmapDataSrc.Scan0;
                        byte* ptrDestPadded0 = (byte*)(bitmapDataSrcPadded.Scan0 + strideS + pixelFormat);
                        Parallel.For(0, heightR, (row) =>
                        {
                            for (int col = 0; col < widthR; col++)
                            {
                                byte* ptrSrc = (byte*)(ptrSrcOrig0 + row * strideR + col * pixelFormat);
                                byte* ptrDest = (byte*)(ptrDestPadded0 + row * strideS + col * pixelFormat);
                                ptrDest[0] = ptrSrc[0];
                                ptrDest[1] = ptrSrc[1];
                                ptrDest[2] = ptrSrc[2];
                            }
                        });
                        source.UnlockBits(bitmapDataSrc);
                    }

                    Parallel.For(0, heightR, (row) =>
                    //for(int row = 0; row < heightR; row++) 
                    {
                        for (int col = 0; col < widthR; col++)
                        {
                            // pointer to the actual pixel on the destination bitmap
                            byte* ptrDest = (byte*)(ptrDest0 + row * strideR + col * pixelFormat);

                            // get the vector contains the actual pixel coordinates
                            Matrix coordsDest = Matrix.GetVector(x: row, y: col, dimension: dimension);

                            // calcuate the destination cordinates (pixels)
                            Matrix coordsSrc = invTransformation * coordsDest;

                            // Calculate the coordinates of the source pixels
                            double scale = coordsSrc[dim - 1, 0];
                            if (scale < 0.01)
                            {
                                scale = 0.01;
                            }
                            double rowSrcF = (coordsSrc[0, 0] / scale) + 1;
                            double colSrcF = (coordsSrc[1, 0] / scale) + 1;
                            int rowSrc = (int)(rowSrcF);
                            int colSrc = (int)(colSrcF);

                            // inside the image
                            if (rowSrcF >= 0 && rowSrcF < (heightS - 1) && colSrcF >= 0 && colSrcF < (widthS - 1))
                            {
                                // pointer to the source pixel and for the surronding pixels
                                byte* ptrSrc = (byte*)(ptrSrc0 + rowSrc * strideS + colSrc * pixelFormat);

                                // weights for neighbouring pixels
                                double rowSrcF_Down = rowSrcF - rowSrc;
                                double rowSrcF_Up = 1 - rowSrcF_Down;
                                double colSrcF_Right = colSrcF - colSrc;
                                double colSrcF_Left = 1 - colSrcF_Right;
                                double[] weights = new double[4]
                                {
                                rowSrcF_Up * colSrcF_Left,    // (0 - [up,left]);
                                rowSrcF_Up * colSrcF_Right,   // (1 - [up,right]);
                                rowSrcF_Down * colSrcF_Left,  // (2 - [down,left]);
                                rowSrcF_Down * colSrcF_Right  // (3 - [down,right]);
                                };

                                for (int i = 0; i < 3; i++)
                                {
                                    ptrDest[i] = (byte)(weights[0] * ptrSrc[i]
                                                      + weights[1] * ptrSrc[pixelFormat + i]
                                                      + weights[2] * ptrSrc[strideS + i]
                                                      + weights[3] * ptrSrc[strideS + pixelFormat + i]);
                                }
                            }
                            // outside of 1 pixel extra margin around the image
                            else
                            {
                                for (int i = 0; i < 3; i++)
                                    ptrDest[i] = 0;
                            }
                        }
                    });

                    sourcePadded.UnlockBits(bitmapDataSrcPadded);
                    result.UnlockBits(bitmapDataDest);
                }
                return;
            }

            // calculation of the transformation with other interpolation types
            {
                // get data for transformation
                int height = source.Height;
                int width = source.Width;
                BitmapData bitmapDataSrc = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                BitmapData bitmapDataDest = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                int stride = bitmapDataSrc.Stride;
                byte* ptrSrc0 = (byte*)bitmapDataSrc.Scan0;
                byte* ptrDest0 = (byte*)bitmapDataDest.Scan0;
                int pixelFormat = 4;

                Parallel.For(0, height, (row) =>
                //for(int row = 0; row < height; row++) 
                {
                    for (int col = 0; col < width; col++)
                    {
                        // pointer to the actual pixel on the destination bitmap
                        byte* ptrDest = (byte*)(ptrDest0 + row * stride + col * pixelFormat);

                        // get the vector contains the actual pixel coordinates
                        Matrix coordsDest = Matrix.GetVector(x: row, y: col, dimension: dimension);

                        // calcuate the destination cordinates (pixels)
                        Matrix coordsSrc = invTransformation * coordsDest;

                        // Calculate the coordinates of the source pixels
                        int rowSrc = (int)(coordsSrc[0, 0] + 0.5);
                        int colSrc = (int)(coordsSrc[1, 0] + 0.5);

                        // pointer to the source pixel and for the surronding pixels
                        byte* ptrSrc = (byte*)(ptrSrc0 + rowSrc * stride + colSrc * pixelFormat);

                        // summarize pixel values due to selected interpolation type
                        if (interpolationType == InterpolationTypes.None)
                        {
                            // actual source pixel
                            if (rowSrc >= 0 && rowSrc < height && colSrc >= 0 && colSrc < width)
                                for (int i = 0; (i < 3); i++)
                                    ptrDest[i] = ptrSrc[i];
                            else
                                for (int i = 0; (i < 3); i++)
                                    ptrDest[i] = 0;
                        }
                        else if (interpolationType == InterpolationTypes.Dir4_FromSource)
                        {
                            int[] pixelValues = new int[3];
                            int divider = 0;

                            // actual source pixel
                            if (rowSrc >= 0 && rowSrc < height && colSrc >= 0 && colSrc < width)
                            {
                                for (int i = 0; (i < 3); i++)
                                    pixelValues[i] += ptrSrc[i] * 4;
                                divider += 4;

                                // upper pixel 
                                if (rowSrc - 1 >= 0)
                                {
                                    for (int i = 0; (i < 3); i++)
                                        pixelValues[i] += ptrSrc[-stride + i];
                                    divider += 1;
                                }

                                // lower pixel
                                if (rowSrc + 1 < height)
                                {
                                    for (int i = 0; (i < 3); i++)
                                        pixelValues[i] += ptrSrc[stride + i];
                                    divider += 1;
                                }

                                // right pixel
                                if (colSrc + 1 < width)
                                {
                                    for (int i = 0; (i < 3); i++)
                                        pixelValues[i] += ptrSrc[pixelFormat + i];
                                    divider += 1;
                                }

                                // left pixel
                                if (colSrc - 1 >= 0)
                                {
                                    for (int i = 0; (i < 3); i++)
                                        pixelValues[i] += ptrSrc[-pixelFormat + i];
                                    divider += 1;
                                }
                            }

                            // set the destination pixel value
                            if (divider == 0)
                            {
                                for (int i = 0; (i < 3); i++)
                                    ptrDest[i] = 0;
                            }
                            else
                            {
                                for (int i = 0; (i < 3); i++)
                                    ptrDest[i] = (byte)(pixelValues[i] / divider + 0.5);
                            }
                        }
                        else if (interpolationType == InterpolationTypes.Dir8_FromSource)
                        {
                            int[] pixelValues = new int[3];
                            int divider = 0;

                            // actual source pixel
                            if (rowSrc >= 0 && rowSrc < height && colSrc >= 0 && colSrc < width)
                            {
                                for (int i = 0; (i < 3); i++)
                                    pixelValues[i] += ptrSrc[i] * 8;
                                divider += 8;

                                // upper pixel
                                if (rowSrc - 1 >= 0)
                                {
                                    for (int i = 0; (i < 3); i++)
                                        pixelValues[i] += ptrSrc[-stride + i] * 2;
                                    divider += 2;

                                    // upper left pixel
                                    if (colSrc - 1 >= 0)
                                    {
                                        for (int i = 0; (i < 3); i++)
                                            pixelValues[i] += ptrSrc[-stride - pixelFormat + i];
                                        divider += 1;
                                    }

                                    // upper right pixel
                                    if (colSrc + 1 < width)
                                    {
                                        for (int i = 0; (i < 3); i++)
                                            pixelValues[i] += ptrSrc[-stride + pixelFormat + i];
                                        divider += 1;
                                    }
                                }

                                // lower pixel
                                if (rowSrc + 1 < height)
                                {
                                    for (int i = 0; (i < 3); i++)
                                        pixelValues[i] += ptrSrc[stride + i] * 2;
                                    divider += 2;

                                    // lower left pixel
                                    if (colSrc - 1 >= 0)
                                    {
                                        for (int i = 0; (i < 3); i++)
                                            pixelValues[i] += ptrSrc[stride - pixelFormat + i];
                                        divider += 1;
                                    }

                                    // lower right pixel
                                    if (colSrc + 1 < width)
                                    {
                                        for (int i = 0; (i < 3); i++)
                                            pixelValues[i] += ptrSrc[stride + pixelFormat + i];
                                        divider += 1;
                                    }
                                }

                                // left pixel
                                if (colSrc - 1 >= 0)
                                {
                                    for (int i = 0; (i < 3); i++)
                                        pixelValues[i] += ptrSrc[-pixelFormat + i] * 2;
                                    divider += 2;
                                }

                                // right pixel
                                if (colSrc + 1 < width)
                                {
                                    for (int i = 0; (i < 3); i++)
                                        pixelValues[i] += ptrSrc[pixelFormat + i] * 2;
                                    divider += 2;
                                }
                            }

                            // set the destination pixel value
                            if (divider == 0)
                            {
                                for (int i = 0; (i < 3); i++)
                                    ptrDest[i] = 0;
                            }
                            else
                            {
                                for (int i = 0; (i < 3); i++)
                                    ptrDest[i] = (byte)(pixelValues[i] / divider + 0.5);
                            }
                        }
                        else if (interpolationType == InterpolationTypes.Dir4_ToDestination)
                        {
                            // actual source pixel
                            if (rowSrc >= 0 && rowSrc < height && colSrc >= 0 && colSrc < width)
                            {
                                for (int i = 0; (i < 3); i++)
                                    ptrDest[i] = ptrSrc[i];

                                // weights for surronding pixels
                                double weightActual = 1 - weightCloserNeighbour;

                                // upper destination pixel 
                                if (row - 1 >= 0)
                                    for (int i = 0; (i < 3); i++)
                                        ptrDest[-stride + i] = (byte)(ptrDest[-stride + i] * weightActual + ptrSrc[i] * weightCloserNeighbour);

                                // lower destination pixel
                                if (row + 1 < height)
                                    for (int i = 0; (i < 3); i++)
                                        ptrDest[stride + i] = (byte)(ptrDest[stride + i] * weightActual + ptrSrc[i] * weightCloserNeighbour);

                                // left destination pixel
                                if (col - 1 >= 0)
                                    for (int i = 0; (i < 3); i++)
                                        ptrDest[-pixelFormat + i] = (byte)(ptrDest[-pixelFormat + i] * weightActual + ptrSrc[i] * weightCloserNeighbour);

                                // right destination pixel
                                if (col + 1 < width)
                                    for (int i = 0; (i < 3); i++)
                                        ptrDest[pixelFormat + i] = (byte)(ptrDest[pixelFormat + i] * weightActual + ptrSrc[i] * weightCloserNeighbour);
                            }
                            else
                                for (int i = 0; (i < 3); i++)
                                    ptrDest[i] = 0;
                        }
                        else if (interpolationType == InterpolationTypes.Dir8_ToDestination)
                        {
                            // actual source pixel
                            if (rowSrc >= 0 && rowSrc < height && colSrc >= 0 && colSrc < width)
                            {
                                for (int i = 0; i < 3; i++)
                                    ptrDest[i] = ptrSrc[i];

                                // weights for surronding pixels
                                double weightCloserActual = 1 - weightCloserNeighbour;
                                double weightFarerNeighbour = weightCloserNeighbour / 2;
                                double weightFarerActual = 1 - weightFarerNeighbour;

                                // upper destination pixel 
                                if (row - 1 >= 0)
                                {
                                    for (int i = 0; i < 3; i++)
                                        ptrDest[-stride + i] = (byte)(ptrDest[-stride + i] * weightCloserActual + ptrSrc[i] * weightCloserNeighbour);

                                    // upper left destination pixel
                                    if (col - 1 >= 0)
                                        for (int i = 0; (i < 3); i++)
                                            ptrDest[-stride - pixelFormat + i] = (byte)(ptrDest[-stride - pixelFormat + i] * weightFarerActual + ptrSrc[i] * weightFarerNeighbour);

                                    // upper right destination pixel
                                    if (col + 1 < width)
                                        for (int i = 0; i < 3; i++)
                                            ptrDest[-stride + pixelFormat + i] = (byte)(ptrDest[-stride + pixelFormat + i] * weightFarerActual + ptrSrc[i] * weightFarerNeighbour);
                                }

                                // lower destination pixel
                                if (row + 1 < height)
                                {
                                    for (int i = 0; i < 3; i++)
                                        ptrDest[stride + i] = (byte)(ptrDest[stride + i] * weightCloserActual + ptrSrc[i] * weightCloserNeighbour);

                                    // lower left destination pixel
                                    if (col - 1 >= 0)
                                        for (int i = 0; (i < 3); i++)
                                            ptrDest[stride - pixelFormat + i] = (byte)(ptrDest[stride - pixelFormat + i] * weightFarerActual + ptrSrc[i] * weightFarerNeighbour);

                                    // lower right destination pixel
                                    if (col + 1 < height)
                                        for (int i = 0; (i < 3); i++)
                                            ptrDest[stride + pixelFormat + i] = (byte)(ptrDest[stride + pixelFormat + i] * weightFarerActual + ptrSrc[i] * weightFarerNeighbour);
                                }

                                // left destination pixel
                                if (col - 1 >= 0)
                                    for (int i = 0; (i < 3); i++)
                                        ptrDest[-pixelFormat + i] = (byte)(ptrDest[-pixelFormat + i] * weightCloserActual + ptrSrc[i] * weightCloserNeighbour);

                                // right destination pixel
                                if (col + 1 < width)
                                    for (int i = 0; (i < 3); i++)
                                        ptrDest[pixelFormat + i] = (byte)(ptrDest[pixelFormat + i] * weightCloserActual + ptrSrc[i] * weightCloserNeighbour);
                            }
                            else
                                for (int i = 0; (i < 3); i++)
                                    ptrDest[i] = 0;
                        }
                    }
                });

                source.UnlockBits(bitmapDataSrc);
                result.UnlockBits(bitmapDataDest);
            }
        }

        public static unsafe void ExecuteBackward_FloatingInterpolation(Bitmap source, ref Bitmap result, Matrix transformation)
        {
            // null and size checks
            CheckBitmaps(source, ref result);

            // get the inverse of the transformation
            Matrix invTransformation = transformation.GetInverse();

            // create bitmap from source with 1 pixel padding
            Bitmap sourcePadded = new Bitmap(source.Width + 2, source.Height + 2);

            // get dimension information from transformation
            Dimension dimension = (Dimension)transformation.Rows;
            int dim = transformation.Rows;

            // calculation of the transformation
            {
                // get data for transformation
                int heightR = result.Height;
                int widthR = result.Width;
                int heightS = sourcePadded.Height;
                int widthS = sourcePadded.Width;
                BitmapData bitmapDataSrcPadded = sourcePadded.LockBits(new Rectangle(0, 0, widthS, heightS), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                BitmapData bitmapDataDest = result.LockBits(new Rectangle(0, 0, widthR, heightR), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                int strideS = bitmapDataSrcPadded.Stride;
                int strideR = bitmapDataDest.Stride;
                byte* ptrSrc0 = (byte*)bitmapDataSrcPadded.Scan0;
                byte* ptrDest0 = (byte*)bitmapDataDest.Scan0;
                int pixelFormat = 4;

                // copy the source image content into the padded image
                {
                    BitmapData bitmapDataSrc = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                    byte* ptrSrcOrig0 = (byte*)bitmapDataSrc.Scan0;
                    byte* ptrDestPadded0 = (byte*)(bitmapDataSrcPadded.Scan0 + strideS + pixelFormat);
                    Parallel.For(0, heightR, (row) =>
                    {
                        for (int col = 0; col < widthR; col++)
                        {
                            byte* ptrSrc = (byte*)(ptrSrcOrig0 + row * strideR + col * pixelFormat);
                            byte* ptrDest = (byte*)(ptrDestPadded0 + row * strideS + col * pixelFormat);
                            ptrDest[0] = ptrSrc[0];
                            ptrDest[1] = ptrSrc[1];
                            ptrDest[2] = ptrSrc[2];
                        }
                    });
                    source.UnlockBits(bitmapDataSrc);
                }

                Parallel.For(0, heightR, (row) =>
                //for(int row = 0; row < heightR; row++) 
                {
                    for (int col = 0; col < widthR; col++)
                    {
                        // pointer to the actual pixel on the destination bitmap
                        byte* ptrDest = (byte*)(ptrDest0 + row * strideR + col * pixelFormat);

                        // get the vector contains the actual pixel coordinates
                        Matrix coordsDest = Matrix.GetVector(x: row, y: col, dimension: dimension);

                        // calcuate the destination cordinates (pixels)
                        Matrix coordsSrc = invTransformation * coordsDest;

                        // Calculate the coordinates of the source pixels
                        double scale = coordsSrc[dim - 1, 0];
                        if (scale < 0.01)
                        {
                            scale = 0.01;
                        }
                        double rowSrcF = (coordsSrc[0, 0] / scale) + 1;
                        double colSrcF = (coordsSrc[1, 0] / scale) + 1;
                        int rowSrc = (int)(rowSrcF);
                        int colSrc = (int)(colSrcF);

                        // inside the image
                        if (rowSrcF >= 0 && rowSrcF < (heightS - 1) && colSrcF >= 0 && colSrcF < (widthS - 1))
                        {
                            // pointer to the source pixel and for the surronding pixels
                            byte* ptrSrc = (byte*)(ptrSrc0 + rowSrc * strideS + colSrc * pixelFormat);

                            // weights for neighbouring pixels
                            double rowSrcF_Down = rowSrcF - rowSrc;
                            double rowSrcF_Up = 1 - rowSrcF_Down;
                            double colSrcF_Right = colSrcF - colSrc;
                            double colSrcF_Left = 1 - colSrcF_Right;
                            double[] weights = new double[4]
                            {
                                rowSrcF_Up * colSrcF_Left,    // (0 - [up,left]);
                                rowSrcF_Up * colSrcF_Right,   // (1 - [up,right]);
                                rowSrcF_Down * colSrcF_Left,  // (2 - [down,left]);
                                rowSrcF_Down * colSrcF_Right  // (3 - [down,right]);
                            };

                            for (int i = 0; i < 3; i++)
                            {
                                ptrDest[i] = (byte)(weights[0] * ptrSrc[i]
                                                  + weights[1] * ptrSrc[pixelFormat + i]
                                                  + weights[2] * ptrSrc[strideS + i]
                                                  + weights[3] * ptrSrc[strideS + pixelFormat + i]);
                            }
                        }
                        // outside of 1 pixel extra margin around the image
                        else
                        {
                            for (int i = 0; i < 3; i++)
                                ptrDest[i] = 0;
                        }
                    }
                });

                sourcePadded.UnlockBits(bitmapDataSrcPadded);
                result.UnlockBits(bitmapDataDest);
            }
        }

        public static unsafe void ExecuteForward(Bitmap source, ref Bitmap result, Matrix transformation)
        {
            // null and size checks
            CheckBitmaps(source, ref result);

            // set the result bitmap to black
            result.SetTo(red: 0, green: 0, blue: 0);

            // get the dimension of the transformation
            int dim = transformation.Rows;

            // calculation of the transformation
            {
                // get data for transformation
                int height = source.Height;
                int width = source.Width;
                BitmapData bitmapDataSrc = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                BitmapData bitmapDataDest = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
                int stride = bitmapDataSrc.Stride;
                byte* ptrSrc0 = (byte*)bitmapDataSrc.Scan0;
                byte* ptrDest0 = (byte*)bitmapDataDest.Scan0;
                int pixelFormat = 4;

                Parallel.For(0, height, (row) =>
                //for(int row = 0; row < height; row++) 
                {
                    for (int col = 0; col < width; col++)
                    {
                        // pointer to the actual pixel on the source bitmap
                        byte* ptrSrc = (byte*)(ptrSrc0 + row * stride + col * pixelFormat);

                        Matrix coordsSrc = Matrix.GetVector(x: row, y: col, dimension: (Dimension)dim);

                        // calcuate the destination cordinates (pixels)
                        Matrix coodrsDest = transformation * coordsSrc;

                        PrintPixelForward(ptrSrc, ptrDest0, stride, pixelFormat, coodrsDest, height, width);
                    }
                });

                source.UnlockBits(bitmapDataSrc);
                result.UnlockBits(bitmapDataDest);
            }
        }

        private static unsafe void PrintPixelForward(byte* ptrSrc, byte* ptrDest0, int stride, int pixelFormat, Matrix coordsDest, int height, int width)
        {
            double scale = coordsDest[coordsDest.Rows - 1, 0];
            scale = scale < 0.01 ? 0.01 : scale;
            int rowDest = (int)((coordsDest[0, 0] / scale) + 0.5);
            int colDest = (int)((coordsDest[1, 0] / scale) + 0.5);
            if (rowDest < 0 || rowDest >= height || colDest < 0 || colDest >= width)
            {
                return;
            }
            byte* ptrDest = (byte*)(ptrDest0 + rowDest * stride + colDest * pixelFormat);
            ptrDest[0] = ptrSrc[0];
            ptrDest[1] = ptrSrc[1];
            ptrDest[2] = ptrSrc[2];
        }

        private static void CheckBitmaps(Bitmap source, ref Bitmap result)
        {
            if (source == null)
                throw new ArgumentNullException("Source is null.");
            if (result == null || source.Size != result.Size)
            {
                result = new Bitmap(source.Width, source.Height);
            }
        }
    }
}
