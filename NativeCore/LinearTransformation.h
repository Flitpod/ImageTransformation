#pragma once
#include "Matrix.h"

namespace NativeCore {

	class LinearTransformation {
	public:

		/// Executes the transformation via back propagation from the destination location utilizng the inverse transformation.
		static int Execute(
			const Matrix& transformation, 
			const unsigned char* const imgSrc,
			unsigned char* const imgDst,
			const int pixelFormat,
			const int height,
			const int width,
			const int stride
		);
	};
}

