#include "MatrixTests.h"

namespace MatrixTests
{
    // MATRIX MULTIPLICATION test parameters struct
    struct MultiplicationTestData
    {
        NativeCore::Matrix Left;
        NativeCore::Matrix Right;
        NativeCore::Matrix Expected;

        MultiplicationTestData(int l_rows, int l_cols, std::vector<double> leftValues, int r_rows, int r_cols,
                               std::vector<double> rightValues, int e_rows, int e_cols,
                               std::vector<double> expectedValues)
            : Left(l_rows, l_cols, leftValues), Right(r_rows, r_cols, rightValues),
              Expected(e_rows, e_cols, expectedValues)
        {
        }
    };

    // MATRIX MULTIPLICATION test class
    class MultiplicationTests : public ::testing::TestWithParam<MultiplicationTestData>
    {
    };

    // MATRIX MULTIPLICATION test parameters fixture
    INSTANTIATE_TEST_SUITE_P(MultiplicationTestsSuite, MultiplicationTests,
                             ::testing::Values(MultiplicationTestData( // I * B == B, identity * matrix
                                                   2, 2, {1, 0, 0, 1}, // left
                                                   2, 2, {1, 2, 3, 4}, // right
                                                   2, 2, {1, 2, 3, 4}  // expected
                                                   ),
                                               MultiplicationTestData(                // I * B == B, identity * matrix
                                                   3, 3, {1, 0, 0, 0, 1, 0, 0, 0, 1}, // left
                                                   3, 3, {1, 2, 3, 4, 5, 6, 7, 8, 9}, // right
                                                   3, 3, {1, 2, 3, 4, 5, 6, 7, 8, 9}  // expected
                                                   ),
                                               MultiplicationTestData( // A * B == C, horizontal flip
                                                   2, 2, {0, 1, 1, 0}, // left
                                                   2, 2, {1, 2, 3, 4}, // right
                                                   2, 2, {3, 4, 1, 2}  // expected
                                                   ),
                                               MultiplicationTestData(           // A * B == C, vertical flip
                                                   2, 2, {-2.5, 1.5, -3.5, 2.5}, // left
                                                   2, 2, {1, 2, 3, 4},           // right
                                                   2, 2, {2, 1, 4, 3}            // expected
                                                   ),
                                               MultiplicationTestData(                // I * B == B, identity * vector
                                                   3, 3, {1, 0, 0, 0, 1, 0, 0, 0, 1}, // left
                                                   3, 1, {2, 3, 1},                   // right
                                                   3, 1, {2, 3, 1}                    // expected
                                                   ),
                                               MultiplicationTestData( // A * B == B, rotate pos 90 * vector
                                                   3, 3, {0, -1, 0, 1, 0, 0, 0, 0, 1}, // left
                                                   3, 1, {2, 3, 1},                    // right
                                                   3, 1, {-3, 2, 1}                    // expected
                                                   )));

    // MATRIX MULTIPLICATION with multiple params test
    TEST_P(MultiplicationTests, Multiply_Matrices_ShouldPass)
    {
        // arrange
        auto data = GetParam();

        // act
        auto actual = data.Left * data.Right;

        // assert
        MatricesAreEqual(actual, data.Expected);
    }
} // namespace MatrixTests
