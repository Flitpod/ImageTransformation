#include "MatrixTests.h"

namespace MatrixTests
{
    class GetDeterminantTests : public ::testing::TestWithParam<std::tuple<NativeCore::Matrix, double>>
    {
    };

    INSTANTIATE_TEST_SUITE_P(GetDeterminantTestsSuite, GetDeterminantTests,
                             ::testing::Values(std::make_tuple(                         // 3x3 matrix with determinant 3
                                 NativeCore::Matrix(3, 3, {1, 2, 3, 2, 3, 3, 2, 4, 3}), // matrix
                                 3                                                      // expected
                                 )));

    TEST_P(GetDeterminantTests, GetDeterminant_Matrices_ShouldPass)
    {
        // arrage
        auto param = GetParam();
        auto matrix = std::get<0>(param);
        auto expected = std::get<1>(param);

        // act
        auto actual = matrix.GetDeterminant();

        // assert
        EXPECT_EQ(actual, expected);
    }
} // namespace MatrixTests