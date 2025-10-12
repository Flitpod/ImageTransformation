#include "pch.h"
#include "..\NativeCore\Matrix.h"

namespace MatrixTests {

	// === PARAMS ===================================================================================
	// MATRIX MULTIPLICATION test parameters struct
	struct MatrixMultiplicationTestData {
		NativeCore::Matrix Left;
		NativeCore::Matrix Right;
		NativeCore::Matrix Expected;

		MatrixMultiplicationTestData(
			int l_rows, int l_cols, std::vector<double> leftValues,
			int r_rows, int r_cols, std::vector<double> rightValues,
			int e_rows, int e_cols, std::vector<double> expectedValues
		) : Left(l_rows, l_cols, leftValues),
			Right(r_rows, r_cols, rightValues),
			Expected(e_rows, e_cols, expectedValues) {
		}
	};

	// MATRIX MULTIPLICATION test class
	class MatrixMultiplicationTests :
		public ::testing::TestWithParam<MatrixMultiplicationTestData> {
	};

	// MATRIX MULTIPLICATION test parameters fixture
	INSTANTIATE_TEST_CASE_P(
		,
		MatrixMultiplicationTests,
		::testing::Values(
			MatrixMultiplicationTestData( // I * B == B, identity * matrix
				2, 2, { 1,0,0,1 }, // left
				2, 2, { 1,2,3,4 }, // right
				2, 2, { 1,2,3,4 } // expected
			),
			MatrixMultiplicationTestData( // I * B == B, identity * matrix
				3, 3, { 1,0,0,0,1,0,0,0,1 }, // left
				3, 3, { 1,2,3,4,5,6,7,8,9 }, // right
				3, 3, { 1,2,3,4,5,6,7,8,9 } // expected
			),
			MatrixMultiplicationTestData( // A * B == C, horizontal flip
				2, 2, { 0,1,1,0 }, // left
				2, 2, { 1,2,3,4 }, // right
				2, 2, { 3,4,1,2 } // expected
			),
			MatrixMultiplicationTestData( // A * B == C, vertical flip
				2, 2, { -2.5, 1.5, -3.5, 2.5 }, // left
				2, 2, { 1,2,3,4 }, // right
				2, 2, { 2,1,4,3 } // expected
			),
			MatrixMultiplicationTestData( // I * B == B, identity * vector
				3, 3, { 1,0,0,0,1,0,0,0,1 }, // left
				3, 1, { 2,3,1 }, // right
				3, 1, { 2,3,1 } // expected
			),
			MatrixMultiplicationTestData( // A * B == B, rotate pos 90 * vector
				3, 3, { 0,-1,0,1,0,0,0,0,1 }, // left
				3, 1, { 2,3,1 }, // right
				3, 1, { -3,2,1 } // expected
			)
		)
	);

	// === HELPERS ==================================================================================
	// MATRIX equality assert helper function
	void MatricesAreEqual(const NativeCore::Matrix& actual, const NativeCore::Matrix& expected) {
		EXPECT_EQ(actual.GetRows(), expected.GetRows());
		EXPECT_EQ(actual.GetCols(), expected.GetCols());
		for (int row = 0; row < expected.GetRows(); row++) {
			for (int col = 0; col < expected.GetCols(); col++) {
				EXPECT_EQ(actual(row, col), expected(row, col));
			}
		}
	}

	// === TEST CASES ===============================================================================
	// MATRIX MULTIPLICATION with multiple params test
	TEST_P(MatrixMultiplicationTests, Multiply_Matrices_ShouldPass) {
		// arrange
		auto data = GetParam();

		// act
		auto actual = data.Left * data.Right;

		// assert
		MatricesAreEqual(actual, data.Expected);
	}
}

