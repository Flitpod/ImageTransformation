#include "pch.h"
#include "Matrix.h"
#include <stdexcept>

/*
Static sized matrix class
*/

namespace NativeCore {

	// methods
	void Matrix::ThrowIfIndexOutOfRange(int row, int col) const {
		if (row >= m_Rows || col >= m_Cols) {
			throw std::overflow_error("Index out of the range!");
		}
	}

	bool Matrix::CanMultiply(const Matrix& right) const {
		return m_Cols == right.GetRows();
	}

	// ctors
	Matrix::Matrix()
		: m_Rows(0), m_Cols(0) {}
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
}