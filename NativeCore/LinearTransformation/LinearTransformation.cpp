#include "LinearTransformation.h"
#include <omp.h>

namespace NativeCore
{

    int LinearTransformation::Execute(const Matrix& transformation, const unsigned char* const imgSrc,
                                      unsigned char* const imgDst, const int pixelFormat, const int height,
                                      const int width, const int stride)
    {
        // Executes the transformation via back propagation from the destination location utilizng the inverse
        // transformation.
        // 1. Get inverse transformation weights
        // 2. Iterate over the source image with a parallel outer loop
        // 3. Create 3D vector out of destination pixel positions to get the back propagated source pixel coordinates
        // 4. Process bilinear interpolation from the source pixels

        // inverse transformation weights
        Matrix invTransformation = transformation.GetInverse();
        const double m00 = invTransformation(0, 0);
        const double m01 = invTransformation(0, 1);
        const double m02 = invTransformation(0, 2);
        const double m10 = invTransformation(1, 0);
        const double m11 = invTransformation(1, 1);
        const double m12 = invTransformation(1, 2);
        const double m20 = invTransformation(2, 0);
        const double m21 = invTransformation(2, 1);
        const double m22 = invTransformation(2, 2);

// iterate over the image
#pragma omp parallel for schedule(dynamic, 16)
        for (int row = 0; row < height; row++)
        {
            // cache matrix multiplication row values and destination row address
            const double invRow0 = m00 * row + m02;
            const double invRow1 = m10 * row + m12;
            const double invRow2 = m20 * row + m22;
            unsigned char* dstRow = imgDst + stride * row;

            for (int col = 0; col < width; col++)
            {
                // calculate source pixel(s) position
                const double invWeight = 1 / (invRow2 + m21 * col);
                double srcRow = (invRow0 + m01 * col) * invWeight;
                double srcCol = (invRow1 + m11 * col) * invWeight;
                const int srcRowFloor = int(srcRow);
                const int srcColFloor = int(srcCol);

                // create pointer for the destination pixel
                unsigned char* dst = (dstRow + pixelFormat * col);

                // check if the back propagated source coordinates are inside the source image
                if (srcRowFloor >= 0 && srcRowFloor < (height - 1) && srcColFloor >= 0 && srcColFloor < (width - 1))
                {
                    // 4. bilinear interpolation
                    // create pointer to the source pixel
                    const unsigned char* src = (imgSrc + stride * srcRowFloor + pixelFormat * srcColFloor);

                    // calculate interpolation ratios
                    const double rowLowerRatio = srcRow - srcRowFloor;
                    const double colRightRatio = srcCol - srcColFloor;

                    const double w00 = (1 - rowLowerRatio) * (1 - colRightRatio);
                    const double w01 = (1 - rowLowerRatio) * colRightRatio;
                    const double w10 = rowLowerRatio * (1 - colRightRatio);
                    const double w11 = rowLowerRatio * colRightRatio;

                    // process bilinear interpolation and set the destination pixel value
                    for (int channel = 0; channel < pixelFormat; channel++)
                    {
                        double value = w00 * src[channel];
                        value += w01 * src[pixelFormat + channel];
                        value += w10 * src[stride + channel];
                        value += w11 * src[stride + pixelFormat + channel];
                        dst[channel] = (unsigned char)value;
                    }
                }
                else
                {
                    // set destination pixels to black
                    for (int channel = 0; channel < pixelFormat; channel++)
                    {
                        dst[channel] = 0;
                    }
                }
            }
        }

        return 0;
    }
} // namespace NativeCore
