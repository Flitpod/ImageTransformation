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

            int dim = dimension switch
            {
                Dimension.D2 => 2,
                Dimension.D3 => 3,
                Dimension.D4 => 4,
            };

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
            int dim = dimension switch
            {
                Dimension.D2 => 2,
                Dimension.D3 => 3,
                Dimension.D4 => 4,
            };

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
            int dim = dimension switch
            {
                Dimension.D2 => 2,
                Dimension.D3 => 3,
                Dimension.D4 => 4,
            };

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
            int dim = dimension switch
            {
                Dimension.D2 => 2,
                Dimension.D3 => 3,
                Dimension.D4 => 4,
            };

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

    }
}
