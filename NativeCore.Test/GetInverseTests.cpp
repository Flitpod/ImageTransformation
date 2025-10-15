#include "pch.h"
#include "MatrixTests.h"

namespace MatrixTests {
	class GetInverseTests :
		public ::testing::TestWithParam<std::tuple<NativeCore::Matrix, NativeCore::Matrix>> {
	};

	INSTANTIATE_TEST_CASE_P(
		,
		GetInverseTests,
		::testing::Values(
			std::make_tuple( // 2X2 matrix for GetInverse test
				NativeCore::Matrix(2, 2, { 1,2,3,4 }),				  // original
				NativeCore::Matrix(2, 2, { -2,1,1.5,-0.5 })			  // expected
			),
			std::make_tuple( // 3x3 matrix for GetInverse test
				NativeCore::Matrix(3, 3, { 1,0,0,0,1,0,0,0,1 }),      // original
				NativeCore::Matrix(3, 3, { 1,0,0,0,1,0,0,0,1 })       // expected
			)
		)
	);

	TEST_P(GetInverseTests, GetInverse_Matrices_ShouldPass) {
		// arrage
		auto param = GetParam();
		auto matrix = std::get<0>(param);
		auto expected = std::get<1>(param);

		// act
		auto actual = matrix.GetInverse();

		// assert
		MatricesAreEqual(actual, expected);
	}
}