#include "MatrixTests.h"

namespace MatrixTests
{
    class GetSubMatrixWithoutRowAndColTests
        : public ::testing::TestWithParam<std::tuple<NativeCore::Matrix, NativeCore::Matrix, int, int>>
    {
    };

    INSTANTIATE_TEST_SUITE_P(GetSubMatrixWithoutRowAndColTestsSuite, GetSubMatrixWithoutRowAndColTests,
                             ::testing::Values(std::make_tuple( // Sub matrix without the given row and col
                                 NativeCore::Matrix(3, 3, {1, 2, 3, 2, 3, 4, 3, 4, 5}), // original
                                 NativeCore::Matrix(2, 2, {1, 3, 3, 5}),                // expected
                                 1,                                                     // row
                                 1                                                      // col
                                 )));

    TEST_P(GetSubMatrixWithoutRowAndColTests, GetSubMatrixWithoutRowAndCol_Matrices_ShouldPass)
    {
        // arrange
        auto param = GetParam();
        auto matrix = std::get<0>(param);
        auto expected = std::get<1>(param);
        auto row = std::get<2>(param);
        auto col = std::get<3>(param);

        // act
        auto actual = matrix.GetSubMatrixWithoutRowAndCol(row, col);

        // assert
        MatricesAreEqual(actual, expected);
    }
} // namespace MatrixTests