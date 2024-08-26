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
                new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { Math.Cos(30.0.ToRadian()), -Math.Sin(30.0.ToRadian()), 0},
                        { Math.Sin(30.0.ToRadian()), Math.Cos(30.0.ToRadian()), 0},
                        { 0, 0, 1}
                    }
                }
            }
        };
        [TestCaseSource(nameof(RotationDataSource_ShouldPass))]
        public void GetRotationMatrix_Functional_ShouldPass(double degree, Matrix expected)
        {
            // act
            Matrix actual = Transformations.Rotation(degree);

            // assert
            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        }


        public static object[] TranslationDataSource_ShouldPass =
        {
            new object[]
            {
                1,
                2,
                new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { 1, 0, 1},
                        { 0, 1, 2},
                        { 0, 0, 1}
                    }
                },
                MatrixType.Homogenous
            },
            new object[]
            {
                1,
                2,
                new Matrix()
                {
                    Values = new double[2,1]
                    {
                        { 1},
                        { 2}
                    }
                },
                MatrixType.Base
            }
        };
        [TestCaseSource(nameof(TranslationDataSource_ShouldPass))]
        public void GetTranslationMatrix_Functional_ShouldPass(double rowDirection, double colDirection, Matrix expected, MatrixType matrixType)
        {
            // act
            Matrix actual = Transformations.Translation(rowDirection, colDirection, matrixType);

            // assert
            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        }


        public static object[] ScaleDataSource_ShouldPass =
        {
            new object[]
            {
                2,
                3,
                new Matrix()
                {
                    Values = new double[3,3]
                    {
                        { 2, 0, 0},
                        { 0, 3, 0},
                        { 0, 0, 1}
                    }
                },
                MatrixType.Homogenous
            },
            new object[]
            {
                2,
                3,
                new Matrix()
                {
                    Values = new double[2,2]
                    {
                        { 2, 0},
                        { 0, 3}
                    }
                },
                MatrixType.Base
            }
        };
        [TestCaseSource(nameof(ScaleDataSource_ShouldPass))]
        public void GetScaleMatrix_Functional_ShouldPass(double rowDirection, double colDirection, Matrix expected, MatrixType matrixType)
        {
            // act
            Matrix actual = Transformations.Scale(rowDirection, colDirection, matrixType);

            // assert
            Assert.That(actual.Values, Is.EqualTo(expected.Values));
        }
    }
}
