using ImageTransformation.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tests
{
    [TestFixture]
    internal class TransformationTests
    {
        public static object[] RotationDataSource_ShouldPass =
        {
            new object[]
            {
                30,
                Dimension.D3,
                new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { Math.Cos(30.0.ToRadian()), -Math.Sin(30.0.ToRadian()), 0},
                        { Math.Sin(30.0.ToRadian()), Math.Cos(30.0.ToRadian()), 0},
                        { 0, 0, 1}
                    }
                },
            }
        };
        [TestCaseSource(nameof(RotationDataSource_ShouldPass))]
        public void GetRotationMatrix_Functional_ShouldPass(double degree, Dimension dimension, Matrix expected)
        {
            // act
            Matrix actual = Transformations.Rotation(degree, dimension);

            // assert
            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        }


        public static object[] TranslationDataSource_ShouldPass =
        {
            new object[]
            {
                1,
                2,
                Dimension.D3,
                new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { 1, 0, 1},
                        { 0, 1, 2},
                        { 0, 0, 1}
                    }
                },
            },
            new object[]
            {
                1,
                2,
                Dimension.D2,
                new Matrix()
                {
                    Values = new double[2,2]
                    {
                        { 1, 1},
                        { 0, 2}
                    }
                },
            }
        };
        [TestCaseSource(nameof(TranslationDataSource_ShouldPass))]
        public void GetTranslationMatrix_Functional_ShouldPass(double rowDirection, double colDirection, Dimension dimension, Matrix expected)
        {
            // act
            Matrix actual = Transformations.Translation(rowDirection, colDirection, dimension);

            // assert
            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        }


        public static object[] ScaleDataSource_ShouldPass =
        {
            new object[]
            {
                2,
                3,
                Dimension.D3,
                new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { 2, 0, 0},
                        { 0, 3, 0},
                        { 0, 0, 1}
                    }
                },
            },
            new object[]
            {
                2,
                3,
                Dimension.D2,
                new Matrix()
                {
                    Values = new double[2,2]
                    {
                        { 2, 0},
                        { 0, 3}
                    }
                },
            }
        };
        [TestCaseSource(nameof(ScaleDataSource_ShouldPass))]
        public void GetScaleMatrix_Functional_ShouldPass(double rowDirection, double colDirection, Dimension dimension, Matrix expected)
        {
            // act
            Matrix actual = Transformations.Scale(rowDirection, colDirection, dimension);

            // assert
            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        }

        public static object[] ProjectiveTransformationDataSource_ShouldPass = new object[]
        {
            new object[]
            {
                new List<Tuple<Tuple<double, double>, Tuple<double, double>>>()
                {
                    new Tuple<Tuple<double, double>, Tuple<double, double>>(new Tuple<double, double>(0.0, 0.0), new Tuple<double, double>(20.0, 20.0)),
                    new Tuple<Tuple<double, double>, Tuple<double, double>>(new Tuple<double, double>(0.0, 100.0), new Tuple<double, double>(19.0476905, 114.2857143)),
                    new Tuple<Tuple<double, double>, Tuple<double, double>>(new Tuple<double, double>(50.0, 100.0), new Tuple<double, double>(63.6363636, 109.090909)),
                    new Tuple<Tuple<double, double>, Tuple<double, double>>(new Tuple<double, double>(50.0, 0.0), new Tuple<double, double>(66.6666666, 19.0476905)),
                },
                new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { 1, 0, 20 },
                        { 0, 1, 20 },
                        { 0.001, 0.0005, 1 },
                    }
                }
            }
        };
        [TestCaseSource(nameof(ProjectiveTransformationDataSource_ShouldPass))]
        public void GetProjectiveMatrix_Functional_ShouldPass(IEnumerable<Tuple<Tuple<double, double>, Tuple<double, double>>> points, Matrix expected)
        {
            // act
            double error = 1e-4;
            var actual = Transformations.GetProjectionMatrix(points);

            // assert
            for (int row = 0; row < expected.Rows; row++)
            {
                for (int col = 0; col < expected.Cols; col++)
                {
                    Assert.That(actual[row, col], Is.EqualTo(expected[row, col]).Within(error));
                }
            }
        }
    }
}
