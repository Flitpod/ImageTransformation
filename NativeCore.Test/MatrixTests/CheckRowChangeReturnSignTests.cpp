#include "MatrixTests.h"

namespace MatrixTests
{
	class CheckRowChangeReturnSignTests : public ::testing::TestWithParam<std::tuple<NativeCore::Matrix, NativeCore::Matrix, int, int, int>>
	{
	};

	INSTANTIATE_TEST_SUITE_P(
		CheckRowChangeReturnSignTestsSuite,
		CheckRowChangeReturnSignTests,
		::testing::Values(
			std::make_tuple(										   // transpose of 3x3 matrix
				NativeCore::Matrix(3, 3, {1, 2, 3, 0, 0, 3, 0, 4, 3}), // actual
				NativeCore::Matrix(3, 3, {1, 2, 3, 0, 4, 3, 0, 0, 3}), // expected
				1,													   // row
				1,													   // col
				-1													   // expected sign
				),
			std::make_tuple(										   // transpose of 3x3 matrix
				NativeCore::Matrix(3, 3, {0, 4, 3, 1, 2, 3, 0, 0, 3}), // actual
				NativeCore::Matrix(3, 3, {1, 2, 3, 0, 4, 3, 0, 0, 3}), // expected
				0,													   // row
				0,													   // col
				-1													   // expected sign
				)));

	TEST_P(CheckRowChangeReturnSignTests, CheckRowChangeReturnSign_Matrices_ShouldPass)
	{
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