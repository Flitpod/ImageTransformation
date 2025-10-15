#include "pch.h"
#include "MatrixTests.h"

namespace MatrixTests {
	class TransposeTests :
		public ::testing::TestWithParam<std::tuple<NativeCore::Matrix, NativeCore::Matrix>> {
	};

	INSTANTIATE_TEST_CASE_P(
		,
		TransposeTests,
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

	TEST_P(TransposeTests, Transpose_Matrices_ShouldPass) {
		// arrange
		auto param = GetParam();
		auto original = std::get<0>(param);
		auto expected = std::get<1>(param);

		// act
		auto actual = original.GetTranspose();

		// assert
		MatricesAreEqual(actual, expected);
	}
}