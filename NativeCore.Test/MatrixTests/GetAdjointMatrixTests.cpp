#include "MatrixTests.h"

namespace MatrixTests
{
    class GetAdjointMatrixTests : public ::testing::TestWithParam<std::tuple<NativeCore::Matrix, NativeCore::Matrix>>
    {
    };

    INSTANTIATE_TEST_SUITE_P(
        GetAdjointMatrixTestsSuite, GetAdjointMatrixTests,
        ::testing::Values(std::make_tuple(                             // 2X2 matrix for GetAdjointMatrix test
                              NativeCore::Matrix(2, 2, {1, 2, 3, 4}),  // original
                              NativeCore::Matrix(2, 2, {4, -2, -3, 1}) // expected
                              ),
                          std::make_tuple( // 3x3 matrix for GetAdjointMatrix test
                              NativeCore::Matrix(3, 3, {71, 8, 5, 7, 8, 5, 2, 5, 8}),               // original
                              NativeCore::Matrix(3, 3, {39, -39, 0, -46, 558, -320, 19, -339, 512}) // expected
                              )));

    TEST_P(GetAdjointMatrixTests, GetAdjointMatrix_Matrices_ShouldPass)
    {
        // arrage
        auto param = GetParam();
        auto matrix = std::get<0>(param);
        auto expected = std::get<1>(param);

        // act
        auto actual = matrix.GetAdjointMatrix();

        // assert
        MatricesAreEqual(actual, expected);
    }
} // namespace MatrixTests