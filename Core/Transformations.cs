using ImageTransformation.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public enum MatrixType
    {
        Base, Homogenous
    }
    public static class DoubleExtender
    {
        public static double ToRadian(this double degree)
        {
            return Math.PI * (degree / 180);
        }
    }
    public static class Transformations
    {
        public static Matrix Rotation(double degree, MatrixType matrixType = MatrixType.Homogenous)
        {
            double sine = Math.Sin(degree.ToRadian());
            double cosine = Math.Cos(degree.ToRadian());
            if(matrixType == MatrixType.Base)
            {
                return new Matrix()
                {
                    Values = new double[2, 2]
                    {
                        { cosine, -sine},
                        { sine, cosine}
                    }
                };
            }
            return new Matrix()
            {
                Values = new double[3, 3]
                {
                    { cosine, -sine, 0 },
                    { sine, cosine, 0 },
                    { 0, 0, 1 }
                }
            };
        }
        public static Matrix Rotation(double degree, double centerRow, double centerCol)
        {
            Matrix rotation = Transformations.Rotation(degree, MatrixType.Homogenous);
            Matrix translationForward = Transformations.Translation(-centerRow, -centerCol); 
            Matrix translationBackward = Transformations.Translation(centerRow, centerCol); 
            return translationBackward * (rotation * translationForward);
        }

        public static Matrix Translation(double rowDirection, double colDirection, MatrixType matrixType = MatrixType.Homogenous)
        {
            if(matrixType == MatrixType.Base)
            {
                return new Matrix()
                {
                    Values = new double[2, 1]
                    {
                        {rowDirection },
                        {colDirection}
                    }
                };
            }
            return new Matrix()
            {
                Values = new double[3, 3]
                {
                    { 1, 0, rowDirection },
                    { 0, 1, colDirection },
                    { 0, 0, 1 }
                }
            };
        }

        public static Matrix Scale(double rowDirection, double colDirection, MatrixType matrixType = MatrixType.Homogenous)
        {
            if (matrixType == MatrixType.Base)
            {
                return new Matrix()
                {
                    Values = new double[2, 2]
                    {
                        { rowDirection, 0 },
                        { 0, colDirection}
                    }
                };
            }
            return new Matrix()
            {
                Values = new double[3, 3]
                {
                    { rowDirection, 0, 0 },
                    { 0, colDirection, 0 },
                    { 0, 0, 1 }
                }
            };
        }

        public static Matrix Identity(MatrixType matrixType = MatrixType.Homogenous)
        {
            if (matrixType == MatrixType.Base)
            {
                return new Matrix()
                {
                    Values = new double[2, 2]
                    {
                        {1, 0 },
                        { 0, 1 }
                    }
                };
            }
            return new Matrix()
            {
                Values = new double[3, 3]
                {
                    { 1, 0, 0 },
                    { 0, 1, 0 },
                    { 0, 0, 1 }
                }
            };
        }

    }
}
