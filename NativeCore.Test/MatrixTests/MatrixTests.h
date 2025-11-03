#pragma once
#include "..\..\NativeCore\LinearTransformation\Matrix.h"
#include <gtest/gtest.h>

namespace MatrixTests
{
	// MATRIX equality assert helper function
	void MatricesAreEqual(const NativeCore::Matrix &actual, const NativeCore::Matrix &expected);
}