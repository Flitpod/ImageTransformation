#pragma once
#include "dllimpexp.h"
#include <windows.h>

extern "C"
{
    // transformation
    NATIVECORE_API int ExecuteHomographyTransformation(int matrixRows, int matrixCols, double* matrixValues,
                                                       const unsigned char* imgSrc, unsigned char* imgDst,
                                                       const int pixelFormat, const int height, const int width,
                                                       const int stride);
}
