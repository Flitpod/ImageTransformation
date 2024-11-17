using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageTransformation.Core;

namespace Core.Tests
{
    [TestFixture]
    public class MatrixTests
    {
        public static object[] MatricesForMultiplyTests_ShouldPass =
        {
            new object[]
            {
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 1,2 },
                        { 1,2 }
                    }
                },
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 2,1 },
                        { 1,2 }
                    }
                },
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 4,5 },
                        { 4,5 }
                    }
                },
            },
            new object[]
            {
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 1,2 },
                        { 3,4 }
                    }
                },
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 2,1 },
                        { 4,3 }
                    }
                },
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 10,7 },
                        { 22,15 }
                    }
                },
            },
            new object[]
            {
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 1,2 },
                        { 3,4 }
                    }
                },
                new Matrix(2,1)
                {
                    Values = new double[2,1]
                    {
                        { 5 },
                        { 6 }
                    }
                },
                new Matrix(2,1)
                {
                    Values = new double[2,1]
                    {
                        { 17 },
                        { 39 }
                    }
                },
            },
            new object[]
            {
                new Matrix(2,1)
                {
                    Values = new double[2,1]
                    {
                        { 0 },
                        { 1 }
                    }
                },
                new Matrix(1,2)
                {
                    Values = new double[1,2]
                    {
                        { 1, 2 }
                    }
                },
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 0, 0 },
                        { 1, 2 }
                    }
                },
            },
        };
        [TestCaseSource(nameof(MatricesForMultiplyTests_ShouldPass))]
        public void MultiplyOperator_Functional_ShouldPass(Matrix left, Matrix right, Matrix expected)
        {
            // act
            Matrix actual = left * right;

            // assert 
            Assert.That(expected.Values, Is.EqualTo(actual.Values));
        }


        public static object[] MatricesForConstantMultiplyTests_ShouldPass =
        {
            new object[]
            {
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 1,2 },
                        { 1,2 }
                    }
                },
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 2,4 },
                        { 2,4 }
                    }
                },
                2
            },
        };
        [TestCaseSource(nameof(MatricesForConstantMultiplyTests_ShouldPass))]
        public void MultiplyConstantOperator_Functionality_ShouldPass(Matrix matrix, Matrix expected, double constant)
        {
            // act
            Matrix actual = constant * matrix;

            // assert
            Assert.That(expected.Values, Is.EqualTo(actual.Values));
        }


        public static object[] MatricesForMultiplyTests_ShouldThrowException =
        {
            new object[]
            {
                new Matrix(2,2)
                {
                    Values= new double[2,2]
                    {
                        { 1, 2 },
                        { 3 ,4 }
                    }
                },
                new Matrix(2,1)
                {
                    Values = new double[1, 2]
                    {
                        { 1, 2 }
                    }
                }
            }
        };
        [TestCaseSource(nameof(MatricesForMultiplyTests_ShouldThrowException))]
        public void Multiply_Functional_ShouldThrowException(Matrix left, Matrix right)
        {
            // act & assert
            Assert.Throws<Exception>(() =>
            {
                var result = left * right;
                return;
            });
        }


        public static object[] MatricesForRowChangeForReducedEchelonFormCalculation_ShouldPass =
        {
            new object[]
            {
                new Matrix(3,3)
                {
                    Values = new double[3,3]
                    {
                        { 1,2,3 },
                        { 0,0,3 },
                        { 0,4,3 }
                    }
                },
                new Matrix(3,3)
                {
                    Values = new double[3,3]
                    {
                        { 1,2,3 },
                        { 0,4,3 },
                        { 0,0,3 }
                    }
                },
                1,
                1,
                -1
            },
            new object[]
            {
                new Matrix(3,3)
                {
                    Values = new double[3,3]
                    {
                        { 0,4,3 },
                        { 1,2,3 },
                        { 0,0,3 }
                    }
                },
                new Matrix(3,3)
                {
                    Values = new double[3,3]
                    {
                        { 1,2,3 },
                        { 0,4,3 },
                        { 0,0,3 }
                    }
                },
                0,
                0,
                -1
            }
        };
        [TestCaseSource(nameof(MatricesForRowChangeForReducedEchelonFormCalculation_ShouldPass))]
        public void CheckRowChangeReturnSignForReducedEchelonFormCalculation_Functional_ShouldPass(Matrix actual, Matrix expected, int row, int col, int expectedSign)
        {
            // act
            int sign = (int)typeof(Matrix).GetMethod("CheckRowChangeReturnSign")?.Invoke( null ,new object[] { actual, row, col});

            // assert
            Assert.That(sign, Is.EqualTo(expectedSign));
            Assert.That(expected.Values, Is.EqualTo(actual.Values));
        }


        public static object[] MatricesForRowEchelonFormReduction_ShouldPass =
        {
            new object[]
            {
                new Matrix(3,3)
                {
                    Values = new double[3,3]
                    {
                        { 1,2,3 },
                        { 2,3,3 },
                        { 2,4,3 }
                    }
                },
                new Matrix(3,3)
                {
                    Values = new double[3,3]
                    {
                        { 1,2,3 },
                        { 0,-1,-3 },
                        { 0,0,-3 }
                    }
                }
            }
        };
        [TestCaseSource(nameof(MatricesForRowEchelonFormReduction_ShouldPass))]
        public void RowEchelonForm_Functional_ShouldPass(Matrix matrix, Matrix expected)
        {
            // act
            Matrix actual = matrix.GetReducedEchelonForm(out int sign);

            // assert
            Assert.That(expected.Values, Is.EqualTo(actual.Values));
        }


        public static object[] MatricesAndValuesForDeterminantTests_ShouldPass =
        {
            new object[]
            {
                 new Matrix(3,3)
                {
                    Values = new double[3,3]
                    {
                        { 1,2,3 },
                        { 2,3,3 },
                        { 2,4,3 }
                    }
                },
                3
            }
        };
        [TestCaseSource(nameof(MatricesAndValuesForDeterminantTests_ShouldPass))]
        public void Determinant_Functional_ShouldPass(Matrix matrix, double expected)
        {
            // act
            double actual = matrix.GetDeterminant();

            // assert
            Assert.That(expected, Is.EqualTo(actual));
        }


        public static object[] MatricesForSubmatrixWithoutRowAndColTests_ShouldPass =
        {
            new object[]
            {
                new Matrix(3,3)
                {
                    Values= new double[3,3]
                    {
                        {1 ,2 , 3 },
                        {2 ,3 , 4 },
                        {3 ,4 , 5 },
                    }
                },
                new Matrix(2,2)
                {
                    Values= new double[2,2]
                    {
                        {1 , 3 },
                        {3 , 5 },
                    }
                },
                1,
                1
            }
        };
        [TestCaseSource(nameof(MatricesForSubmatrixWithoutRowAndColTests_ShouldPass))]
        public void SubMatrixWithoutRowAndCol_Functional_ShouldPass(Matrix matrix, Matrix expected, int row, int col)
        {
            // act
            Matrix actual = matrix.GetSubMatrixWithoutRowAndCol(row, col);

            // assert
            Assert.That(expected.Values, Is.EqualTo(actual.Values));
        }


        public static object[] MatricesForTransposeTests_ShouldPass =
        {
            new object[]
            {
                new Matrix()
                {
                    Values = new double[3,2]
                    {
                        { 1,2 },
                        { 3,4 },
                        { 5,6 }
                    }
                },
                new Matrix()
                {
                    Values = new double[2,3]
                    {
                        { 1,3,5 },
                        { 2,4,6 }
                    }
                }
            },
            new object[]
            {
                new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { 1,2,3 },
                        { 4,5,6 },
                        { 7,8,9 }
                    }
                },
               new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { 1,4,7 },
                        { 2,5,8 },
                        { 3,6,9 }
                    }
                }
            }
        };
        [TestCaseSource(nameof(MatricesForTransposeTests_ShouldPass))]
        public void Transpose_Functional_ShouldPass(Matrix matrix, Matrix expected)
        {
            // act
            Matrix actual = matrix.GetTranspose();

            // assert
            Assert.That(expected.Values, Is.EqualTo(actual.Values));
        }


        public static object[] MatricesForGetAdjointMatrixTests_ShouldPass =
        {
            new object[]
            {
                new Matrix()
                {
                    Values = new double[2,2]
                    {
                        { 1,2 },
                        { 3,4 }
                    }
                },
                new Matrix()
                {
                    Values = new double[2,2]
                    {
                        { 4,-2 },
                        { -3,1 }
                    }
                }
            },
            new object[]
            {
                new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { 71,8,5 },
                        { 7,8,5 },
                        { 2,5,8 }
                    }
                },
               new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { 39,-39,0 },
                        { -46,558,-320 },
                        { 19,-339,512 }
                    }
                }
            }
        };
        [TestCaseSource(nameof(MatricesForGetAdjointMatrixTests_ShouldPass))]
        public void AdjointMatrix_Functional_ShouldPass(Matrix matrix, Matrix expected)
        {
            // act
            Matrix actual = matrix.GetAdjointMatrix();

            // assert
            Assert.That(expected.Values, Is.EqualTo(actual.Values));
        }


        public static object[] MatricesForInverseTests_ShouldPass =
        {
            new object[]
            {
                 new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { 1,2 },
                        { 3,4 }
                    }
                },
                new Matrix(2,2)
                {
                    Values = new double[2,2]
                    {
                        { -2,1 },
                        { 1.5,-0.5 }
                    }
                }
            },
            new object[]
            {
                new Matrix(3,3)
                {
                    Values= new double[3,3]
                    {
                        {1 ,0 , 0 },
                        {0 ,1 , 0 },
                        {0 ,0 , 1 },
                    }
                },
                new Matrix(3,3)
                {
                    Values= new double[3,3]
                    {
                        {1 ,0 , 0 },
                        {0 ,1 , 0 },
                        {0 ,0 , 1 },
                    }
                }
            }
        };
        [TestCaseSource(nameof(MatricesForInverseTests_ShouldPass))]
        public void Inverse_Functional_ShouldPass(Matrix matrix, Matrix expected)
        {
            // act
            Matrix actual = matrix.GetInverse();

            // assert
            Assert.That(expected.Values, Is.EqualTo(actual.Values));
        }


        public static object[] GetVector_Data_ShouldPass =
        {
            new object[]
            {
                10,
                20,
                Dimension.D2,
                new Matrix
                {
                    Values = new double[2,1]
                    {
                        { 10 },
                        { 20 },
                    }
                }
            },
            new object[]
            {
                11,
                22,
                Dimension.D3,
                new Matrix
                {
                    Values = new double[3,1]
                    {
                        { 11 },
                        { 22 },
                        { 1 },
                    }
                }
            },
            new object[]
            {
                111,
                222,
                Dimension.D4,
                new Matrix
                {
                    Values = new double[4,1]
                    {
                        { 111 },
                        { 222 },
                        { 1 },
                        { 1 },
                    }
                }
            },
        };
        [TestCaseSource(nameof(GetVector_Data_ShouldPass))]
        public void GetVector_Functional_ShouldPass(double x, double y, Dimension dimension, Matrix expected)
        {
            // act
            Matrix actual = Matrix.GetVector(x, y, dimension);

            // assert
            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        }
    }
}
