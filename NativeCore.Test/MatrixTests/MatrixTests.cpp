#include "MatrixTests.h"

namespace MatrixTests
{
	// MATRIX equality assert helper function
	void MatricesAreEqual(const NativeCore::Matrix &actual, const NativeCore::Matrix &expected)
	{
		EXPECT_EQ(actual.GetRows(), expected.GetRows());
		EXPECT_EQ(actual.GetCols(), expected.GetCols());
		for (int row = 0; row < expected.GetRows(); row++)
		{
			for (int col = 0; col < expected.GetCols(); col++)
			{
				EXPECT_EQ(actual(row, col), expected(row, col));
			}
		}
	}
}