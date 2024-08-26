using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageTransformation.Core
{
    public struct MSize
    {
        public MSize(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
        }

        public int Rows { get; private set; }
        public int Cols { get; private set; }
    }

    public class Matrix
    {
        // fields - properties
        private double[,] values;
        public double[,] Values
        {
            get
            {
                return values;
            }
            set
            {
                this.Rows = value.GetLength(0);
                this.Cols = value.GetLength(1);
                this.MSize = new MSize(this.Rows, this.Cols);
                this.values = value;
            }
        }
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public MSize MSize { get; private set; }

        // ctors
        public Matrix()
        {
            
        }
        public Matrix(int rows, int cols)
        {
            Values = new double[rows, cols];
        }
        public Matrix(MSize mSize)
        {
            Values = new double[mSize.Rows, mSize.Cols];
        }


        // static methods
        private static bool CanMultiply(Matrix left, Matrix right) => (left.Cols == right.Rows);
        public static int CheckRowChangeReturnSign(Matrix matrix, int row, int col)
        {
            if (matrix[row, col] == 0)
            {
                int rowToSwapIdx = row + 1;
                while (rowToSwapIdx < matrix.Rows && matrix[rowToSwapIdx, col] == 0)
                {
                    rowToSwapIdx++;
                }
                if (rowToSwapIdx < matrix.Rows)
                {
                    for(; col< matrix.Cols; col++)
                    {
                        (matrix[row, col], matrix[rowToSwapIdx, col]) = (matrix[rowToSwapIdx, col], matrix[row, col]);
                    }
                }
                return -1;
            }
            return 1;
        }


        // operator overload
        public static Matrix operator *(Matrix left, Matrix right)
        {
            if (!CanMultiply(left, right))
                throw new Exception("Can not multiply matrices due to size incompatility.");

            Matrix result = new Matrix(rows: left.Rows, cols: right.Cols);

            for (int i = 0; i < result.Rows; i++)
            {
                for (int j = 0; j < result.Cols; j++)
                {
                    double mElement = 0;
                    for (int k = 0; k < left.Cols; k++)
                    {
                        mElement += left[i, k] * right[k, j];
                    }
                    result[i, j] = mElement;
                }
            }
            return result;
        }
        public static Matrix operator *(Matrix matrix, double constant)
        {
            Matrix result = new Matrix(rows: matrix.Rows, cols: matrix.Cols);
            for (int i = 0; i < result.Rows; i++)
            {
                for (int j = 0; j < matrix.Cols; j++)
                {
                    result[i, j] = constant * matrix[i, j];
                }
            }
            return result;
        }
        public static Matrix operator *(double constant, Matrix matrix)
        {
            return matrix * constant;
        }
        public double this[int row, int col]
        {
            get
            {
                return this.Values[row, col];
            }
            set
            {
                this.Values[row, col] = value;
            }
        }


        // member methods
        public Matrix GetInverse()
        {
            double determinant = this.GetDeterminant();
            if (determinant == 0) throw new Exception("Matrix has no inverse.");
            return this.GetAdjointMatrix() * (1 / determinant);
        }
        public double GetDeterminant()
        {
            Matrix REF = this.GetReducedEchelonForm(out int sign);
            double result = 1;
            for (int i = 0; i < REF.Rows; i++)
            {
                result *= REF[i, i];
            }
            return result * sign;
        }
        public Matrix GetReducedEchelonForm(out int signForDeterminant)
        {
            if (this.Rows != this.Cols) throw new Exception("Matrix is not n x n sized!");
            Matrix result = this.GetDeepCopy();
            signForDeterminant = 1;
            for (int k = 1; k < result.Rows; k++)
            {
                for (int i = k; i < result.Rows; i++)
                {
                    signForDeterminant *= CheckRowChangeReturnSign(result, k - 1, k - 1);
                    double mul = -1 * (result[i, k - 1] / result[k - 1, k - 1]);
                    if (double.IsNaN(mul)) mul = 1;
                    for (int j = k - 1; j < result.Cols; j++)
                    {
                        result[i, j] = result[i, j] + (mul * result[k - 1, j]);
                    }
                }
            }
            return result;
        }
        public Matrix GetAdjointMatrix()
        {
            Matrix result = new Matrix(rows: this.Rows, cols: this.Cols);
            for (int i = 0; i < result.Rows; i++)
            {
                for (int j = 0; j < result.Cols; j++)
                {
                    result[i, j] = Math.Pow(-1, i + j) * this.GetSubMatrixWithoutRowAndCol(i, j).GetDeterminant();
                }
            }
            return result.GetTranspose();
        }
        public Matrix GetTranspose()
        {
            Matrix result = new Matrix(rows: this.Cols, cols: this.Rows);
            for (int i = 0; i < this.Rows; i++)
            {
                for (int j = 0; j < this.Cols; j++)
                {
                    result[j, i] = this[i, j];
                }
            }
            return result;
        }
        public Matrix GetSubMatrixWithoutRowAndCol(int row, int col)
        {
            Matrix result = new Matrix(rows: this.Rows - 1, cols: this.Cols - 1);
            int rRow = 0;
            for (int i = 0; i < this.Rows; i++)
            {
                if(i == row)
                {
                    continue;
                }
                int rCol = 0;
                for (int j = 0; j < this.Cols; j++)
                {
                    if (j == col)
                    {
                        continue;
                    }
                    result[rRow, rCol] = this[i, j];
                    rCol++;
                }
                rRow++;
            }
            return result;
        }
        public Matrix GetDeepCopy()
        {
            Matrix result = new Matrix(rows: this.Rows, cols: this.Cols);
            for (int i = 0; i < this.Rows; i++)
            {
                for (int j = 0; j < this.Cols; j++)
                {
                    result[i, j] = this[i, j];
                }
            }
            return result;
        }
    }
}
