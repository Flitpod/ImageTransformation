using ImageTransformation.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class DoubleExtender
    {
        public static double ToRadian(this double degree)
        {
            return Math.PI * (degree / 180);
        }
    }
    public static class Transformations
    {
        public static Matrix Rotation(double degree, Dimension dimension)
        {
            double sine = Math.Sin(degree.ToRadian());
            double cosine = Math.Cos(degree.ToRadian());

            int dim = (int)dimension;

            Matrix matrix = new Matrix
            {
                Values = new double[dim, dim],
            };

            matrix[0, 0] = cosine;
            matrix[1, 0] = sine;
            matrix[0, 1] = -sine;
            matrix[1, 1] = cosine;

            for (int i = 2; i < dim; i++)
            {
                matrix[i, i] = 1;
            }

            return matrix;
        }
        public static Matrix Rotation(double degree, double centerRow, double centerCol, Dimension dimension)
        {
            Matrix rotation = Transformations.Rotation(degree, dimension);
            Matrix translationForward = Transformations.Translation(-centerRow, -centerCol, dimension);
            Matrix translationBackward = Transformations.Translation(centerRow, centerCol, dimension);
            return translationBackward * (rotation * translationForward);
        }

        public static Matrix Translation(double rowDirection, double colDirection, Dimension dimension)
        {
            int dim = (int)dimension;

            Matrix matrix = new Matrix
            {
                Values = new double[dim, dim],
            };


            for (int i = 0; i < dim; i++)
            {
                matrix[i, i] = 1;
            }

            matrix[0, dim - 1] = rowDirection;
            matrix[1, dim - 1] = colDirection;

            return matrix;
        }

        public static Matrix Scale(double rowDirection, double colDirection, Dimension dimension)
        {
            int dim = (int)dimension;

            Matrix matrix = new Matrix
            {
                Values = new double[dim, dim],
            };

            matrix[0, 0] = rowDirection;
            matrix[1, 1] = colDirection;

            for (int i = 2; i < dim; i++)
            {
                matrix[i, i] = 1;
            }

            return matrix;
        }

        public static Matrix Identity(Dimension dimension)
        {
            int dim = (int)dimension;

            Matrix matrix = new Matrix
            {
                Values = new double[dim, dim],
            };

            for (int i = 0; i < dim; i++)
            {
                matrix[i, i] = 1;
            }

            return matrix;
        }

        /// <summary>
        /// Calculates the projective transformation matrix H = [[h11 h12 h13], [h21 h22 h23], [h31 h32 1]] from the given 4 point pairs.
        /// </summary>
        /// <param name="pointPairs">IEnumerable of tuples which are holding 2 tuples for each point. 
        /// [((p1x, p1y), (p1x', p1y')), ...]</param>
        /// <returns>The H projective transformation matrix calculated from the given point pairs</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Matrix GetProjectionMatrix(IEnumerable<Tuple<Tuple<double, double>, Tuple<double, double>>> pointPairs)
        {
            // check if there are 4 corresponding point pairs
            if (pointPairs.Count() != 4)
            {
                throw new ArgumentException("There was no given 4 corresponding point pairs!");
            }

            // construct the A equtations matrix and the corresponding p result vector
            Matrix equtations = new Matrix(rows: 8, cols: 8);
            Matrix pointsVectorTransformed = new Matrix(rows: 8, cols: 1);
            var pointsArray = pointPairs.ToArray();

            // fill the equtations bómatrix and pvector with 
            for (int doubleRow = 0; doubleRow < 8; doubleRow += 2)
            {
                // get the actual points
                var currentPointPairs = pointsArray[doubleRow / 2];
                var pointOriginal = currentPointPairs.Item1;
                var pointTransformed = currentPointPairs.Item2;

                // get the current pointpairs exact values
                double pxOrigin = pointOriginal.Item1;
                double pyOrigin = pointOriginal.Item2;
                double pxTransformed = pointTransformed.Item1;
                double pyTransformed = pointTransformed.Item2;

                // fill equtations
                // parameters for the x row
                equtations[doubleRow, 0] = pxOrigin;
                equtations[doubleRow, 1] = pyOrigin;
                equtations[doubleRow, 2] = 1;
                equtations[doubleRow, 6] = -pxTransformed * pxOrigin;
                equtations[doubleRow, 7] = -pxTransformed * pyOrigin;

                // parameters for the y row
                equtations[doubleRow + 1, 3] = pxOrigin;
                equtations[doubleRow + 1, 4] = pyOrigin;
                equtations[doubleRow + 1, 5] = 1;
                equtations[doubleRow + 1, 6] = -pyTransformed * pxOrigin;
                equtations[doubleRow + 1, 7] = -pyTransformed * pyOrigin;

                // fill point
                pointsVectorTransformed[doubleRow, 0] = pxTransformed;
                pointsVectorTransformed[doubleRow + 1, 0] = pyTransformed;
            }

            // get the inverse of equtations matrix
            Matrix invEqutations = equtations.GetInverse();

            // calculate the h - projective transformation vector
            Matrix h = invEqutations * pointsVectorTransformed;

            // create the result H projectrive transformation matrix from the h vector values
            Matrix H = new Matrix()
            {
                Values = new double[3, 3]
                {
                    { h[0, 0], h[1, 0], h[2, 0] },
                    { h[3, 0], h[4, 0], h[5, 0] },
                    { h[6, 0], h[7, 0], 1}
                }
            };

            return H;
        }
    }
}
