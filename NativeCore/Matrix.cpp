#include "pch.h"
#include "Matrix.h"
#include <stdexcept>

/*
Static sized matrix class
*/

namespace NativeCore {

	// ctors
	Matrix::Matrix()
		: m_Rows(0), m_Cols(0) {
	}
	Matrix::Matrix(int rows, int cols)
		: m_Rows(rows), m_Cols(cols)
	{
		m_Values = std::vector<double>(rows * cols);
		for (int i = 0; i < m_Values.size(); i++) {
			m_Values[i] = 0;
		}
	}
	Matrix::Matrix(int rows, int cols, const double* values)
		: m_Rows(rows), m_Cols(cols)
	{
		m_Values = std::vector<double>(rows * cols);
		for (int i = 0; i < m_Values.size(); i++) {
			m_Values[i] = 0;
		}
	}
	Matrix::Matrix(int rows, int cols, const std::vector<double>& values)
		: m_Rows(rows), m_Cols(cols), m_Values(values) {
	}

	// --- getters ---
	int Matrix::GetRows() const {
		return m_Rows;
	}
	int Matrix::GetCols() const {
		return m_Cols;
	}

	// --- operator overrides --- 
	// -- indexers
	const double Matrix::operator()(int row, int col) const {
		ThrowIfIndexOutOfRange(row, col);
		int position = row * m_Cols + col;
		return m_Values[position];
	}
	double& Matrix::operator()(int row, int col) {
		ThrowIfIndexOutOfRange(row, col);
		int position = row * m_Cols + col;
		return m_Values[position];
	}

	// -- Constant multiplication
	Matrix Matrix::operator*(const double constant) {
		Matrix result(m_Rows, m_Cols);
		for (int row = 0; row < m_Rows; row++) {
			for (int col = 0; col < m_Cols; col++) {
				result(row, col) = constant * (*this)(row, col);
			}
		}
		return result;
	}

	// -- Matrix multiplication
	Matrix Matrix::operator*(const Matrix& right) const {
		if (!CanMultiply(right)) {
			throw std::length_error("Can not multiply the matrix due to incompatible size!");
		}

		Matrix result(m_Rows, right.GetCols());
		for (int row = 0; row < m_Rows; row++) {
			for (int r_col = 0; r_col < right.GetCols(); r_col++) {
				for (int pos = 0; pos < m_Cols; pos++) {
					result(row, r_col) += (*this)(row, pos) * right(pos, r_col);
				}
			}
		}

		return result;
	}

	// methods
	void Matrix::ThrowIfIndexOutOfRange(int row, int col) const {
		if (row >= m_Rows || col >= m_Cols) {
			throw std::overflow_error("Index out of the range!");
		}
	}

	bool Matrix::CanMultiply(const Matrix& right) const {
		return m_Cols == right.GetRows();
	}

	int Matrix::CheckRowChangeReturnSign(Matrix& matrix, int row, int col) {
		if (matrix(row, col) == 0)
		{
			int rowToSwapIdx = row + 1;
			while (rowToSwapIdx < matrix.GetRows() && matrix(rowToSwapIdx, col) == 0)
			{
				rowToSwapIdx++;
			}
			if (rowToSwapIdx < matrix.GetRows())
			{
				for (; col < matrix.GetCols(); col++)
				{
					double temp = matrix(row, col);
					matrix(row, col) = matrix(rowToSwapIdx, col);
					matrix(rowToSwapIdx, col) = temp;
				}
			}
			return -1;
		}
		return 1;
	}

	Matrix Matrix::GetTranspose() const {
		Matrix result(m_Cols, m_Rows);
		for (int row = 0; row < m_Rows; row++) {
			for (int col = 0; col < m_Cols; col++) {
				result(col, row) = (*this)(row, col);
			}
		}
		return result;
	}

	Matrix Matrix::GetSubMatrixWithoutRowAndCol(int row, int col) const {
		Matrix result(m_Rows - 1, m_Cols - 1);
		int rRow = 0;
		for (int i = 0; i < m_Rows; i++) {
			if (i == row) {
				continue;
			}
			int rCol = 0;
			for (int j = 0; j < m_Cols; j++) {
				if (j == col) {
					continue;
				}
				result(rRow, rCol) = (*this)(i, j);
				rCol++;
			}
			rRow++;
		}
		return result;
	}

	Matrix Matrix::GetReducedEchelonForm(int& signForDeterminant) const {
		if (m_Rows != m_Cols) {
			throw std::length_error("Matrix is not n x n sized!");
		}
		Matrix result(*this);
		signForDeterminant = 1;
		for (int k = 1; k < m_Rows; k++) {
			for (int i = k; i < m_Rows; i++) {
				signForDeterminant *= Matrix::CheckRowChangeReturnSign(result, k - 1, k - 1);
				double mul = -1 * (result(i, k - 1) / result(k - 1, k - 1));
				if (std::_Is_nan(mul)) {
					mul = 1;
				}
				for (int j = k - 1; j < m_Cols; j++) {
					result(i, j) = result(i, j) + (mul * result(k - 1, j));
				}
			}
		}
		return result;
	}

	// --- TODO - Add implementation ---
	//double Matrix::GetDeterminant() const {
	//	// TODO: implementation - public
	//}

	//Matrix Matrix::GetAdjointMatrix() const {
	//	// TODO: implementation - public
	//}

	//Matrix Matrix::GetInverse() const {
	//	// TODO: implementation - public
	//}
}