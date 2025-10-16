#include "pch.h"
#include "LinearTransformation.h"
#include <omp.h>

namespace NativeCore {

	int LinearTransformation::Execute(
		const Matrix& transformation,
		const unsigned char* const imgSrc,
		unsigned char* const imgDst,
		const int pixelFormat,
		const int height,
		const int width,
		const int stride
	) {
		// Executes the transformation via back propagation from the destination location utilizng the inverse transformation.
		// 1. Get inverse transformation
		// 2. Iterate over the source image with a parallel outer loop
		// 3. Create 3D vector out of destination pixel positions to get the back propagated source picel coordinates
		// 4. Process bilinear interpolation from the source pixels
		// 5. Set destination pixel value(s)

		// 1. Inverse transformation
		Matrix invTransformation = transformation.GetInverse();

		// 2. Iterate over the image
		#pragma omp parallel for
		for (int row = 0; row < height; row++) {
			for (int col = 0; col < width; col++) {

				// 3. calculate source pixel(s) position
				Matrix dstPixel(3, 1, { (double)row, (double)col, 1 });
				Matrix srcPixel = invTransformation * dstPixel;

				// calculate the back propagated pixel coordinates (scale with the homogenous coordinate)
				double srcRow = srcPixel(0, 0) / srcPixel(2, 0);
				double srcCol = srcPixel(1, 0) / srcPixel(2, 0);
				int srcRowFloor = int(srcRow);
				int srcColFloor = int(srcCol);

				// set pixel values to default black
				unsigned char pixelValues[4] = { 0 };

				// check if the back propagated source coordinates are inside the source image
				if (srcRowFloor >= 0 &&	srcRowFloor < (height - 1) && srcColFloor >= 0 && srcColFloor < (width - 1)) {

					// 4. bilinear interpolation
					// create pointer to the source pixel
					unsigned char* src = (unsigned char*)(imgSrc + stride * srcRowFloor + pixelFormat * srcColFloor);

					// calculate interpolation ratios
					double rowLowerRatio = srcRow - srcRowFloor;
					double colRightRatio = srcCol - srcColFloor;

					double ratio00 = (1 - rowLowerRatio) * (1 - colRightRatio);
					double ratio01 = (1 - rowLowerRatio) * colRightRatio;
					double ratio10 = rowLowerRatio * (1 - colRightRatio);
					double ratio11 = rowLowerRatio * colRightRatio;

					// process bilinear interpolation
					for (int channel = 0; channel < pixelFormat; channel++) {
						double value = ratio00 * src[0];
						value += ratio01 * src[pixelFormat];
						value += ratio10 * src[stride];
						value += ratio11 * src[stride + pixelFormat];
						pixelValues[channel] = (unsigned char)value;
					}
				}

				// create pointer for the destination pixel
				unsigned char* dst = (unsigned char*)(imgDst + stride * row + pixelFormat * col);

				// 5. Set destination pixel values
				for (int channel = 0; channel < pixelFormat; channel++) {
					dst[channel] = pixelValues[channel];
				}
			}
		}

		return 0;
	}
}
