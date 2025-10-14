#include "pch.h"
#include "..\NativeCore\Matrix.h"

namespace MatrixTests {
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

	// === MATRIX MULTIPLICATION TESTS ===================================================================================
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

	// MATRIX MULTIPLICATION with multiple params test
	TEST_P(MatrixMultiplicationTests, Multiply_Matrices_ShouldPass) {
		// arrange
		auto data = GetParam();

		// act
		auto actual = data.Left * data.Right;

		// assert
		MatricesAreEqual(actual, data.Expected);
	}

	// === MATRIX TRANSPOSE TEST CASES ===============================================================================
	class MatrixTransposeTests :
		public ::testing::TestWithParam<std::tuple<NativeCore::Matrix, NativeCore::Matrix>> {
	};

	INSTANTIATE_TEST_CASE_P(
		,
		MatrixTransposeTests,
		::testing::Values(
			std::make_tuple( // transpose of 3x2 matrix
				NativeCore::Matrix(3, 2, { 1,2,3,4,5,6 }), // original
				NativeCore::Matrix(2, 3, { 1,3,5,2,4,6 })  // expected
			),
			std::make_tuple( // transpose of 3x3 matrix
				NativeCore::Matrix(3, 3, { 1,2,3,4,5,6,7,8,9 }), // original
				NativeCore::Matrix(3, 3, { 1,4,7,2,5,8,3,6,9 })  // transpose
			)
		)
	);

	TEST_P(MatrixTransposeTests, Transpose_Matrices_ShouldPass) {
		// arrange
		auto param = GetParam();
		auto original = std::get<0>(param);
		auto expected = std::get<1>(param);

		// act
		auto actual = original.GetTranspose();

		// assert
		MatricesAreEqual(actual, expected);
	}

	// === MATRIX Check Row Change Return Sign TEST CASES ===========================================================
	class MatrixCheckRowChangeReturnSignTests :
		public ::testing::TestWithParam<
		std::tuple<NativeCore::Matrix, NativeCore::Matrix,int, int, int>> {
	};

	INSTANTIATE_TEST_CASE_P(
		,
		MatrixCheckRowChangeReturnSignTests,
		::testing::Values(
			std::make_tuple( // transpose of 3x3 matrix
				NativeCore::Matrix(3, 3, { 1,2,3,0,0,3,0,4,3 }), // actual
				NativeCore::Matrix(3, 3, { 1,2,3,0,4,3,0,0,3 }), // expected
				1,												 // row
				1,												 // col
				-1												 // expected sign
			),
			std::make_tuple( // transpose of 3x3 matrix
				NativeCore::Matrix(3, 3, { 0,4,3,1,2,3,0,0,3 }), // actual
				NativeCore::Matrix(3, 3, { 1,2,3,0,4,3,0,0,3 }), // expected
				0,												 // row
				0,												 // col
				-1												 // expected sign
			)
		)
	);

	TEST_P(MatrixCheckRowChangeReturnSignTests, CheckRowChangeReturnSign_Matrices_ShouldPass) {
		// arrange
		auto param = GetParam();
		auto matrix = std::get<0>(param);
		auto expected = std::get<1>(param);
		auto row = std::get<2>(param);
		auto col = std::get<3>(param);
		auto expectedSign = std::get<4>(param);

		// act
		auto actualSign = NativeCore::Matrix::CheckRowChangeReturnSign(matrix, row, col);

		// assert
		EXPECT_EQ(actualSign, expectedSign);
		MatricesAreEqual(matrix, expected);
	}
}

