#include "pch.h"
#include "API.h"
#include "Matrix.h"
#include "LinearTransformation.h"

extern "C" {
	// transformation
	NATIVECORE_API int ExecuteHomographyTransformation(
		int matrixRows,
		int matrixCols,
		double* matrixValues,
		const unsigned char* imgSrc,
		unsigned char* imgDst,
		const int pixelFormat,
		const int height,
		const int width,
		const int stride
	) {
		try {
			NativeCore::Matrix transformation(matrixRows, matrixCols, matrixValues);
			int result = NativeCore::LinearTransformation::Execute(
				transformation,
				imgSrc,
				imgDst,
				pixelFormat,
				height,
				width,
				stride
			);
			return S_OK;
		}
		catch (const std::exception& e) {
			return E_FAIL;
		}
	}
}