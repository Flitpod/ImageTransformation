#include "MatrixTests.h"

namespace MatrixTests
{
	class GetReducedEchelonFormTests : public ::testing::TestWithParam<std::tuple<NativeCore::Matrix, NativeCore::Matrix>>
	{
	};

	INSTANTIATE_TEST_CASE_P(
		,
		GetReducedEchelonFormTests,
		::testing::Values(
			std::make_tuple(
				NativeCore::Matrix(3, 3, {1, 2, 3, 2, 3, 3, 2, 4, 3}),	 // original
				NativeCore::Matrix(3, 3, {1, 2, 3, 0, -1, -3, 0, 0, -3}) // expected
				)));

	TEST_P(GetReducedEchelonFormTests, GetReducedEchelonForm_Matrices_ShouldPass)
	{
		// arrange
		auto param = GetParam();
		auto original = std::get<0>(param);
		auto expected = std::get<1>(param);

		// act
		int signForDeterminant;
		auto actual = original.GetReducedEchelonForm(signForDeterminant);

		// assert
		MatricesAreEqual(actual, expected);
	}
}