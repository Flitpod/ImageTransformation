#pragma once

#include "pch.h"
#include "dllimpexp.h"
#include <vector>

/*
Static sized matrix class
*/

namespace NativeCore {

	class NATIVECORE_API Matrix {
	private:
		// members
		int m_Rows;
		int m_Cols;
		std::vector<double> m_Values;

		// methods
		void ThrowIfIndexOutOfRange(int row, int col) const;
		bool CanMultiply(const Matrix& right) const;

	public:
		// --- ctors ---
		Matrix();
		Matrix(int rows, int cols);
		Matrix(int rows, int cols, const double* values);
		Matrix(int rows, int cols, const std::vector<double>& values);

		// --- getters ---
		int GetRows() const;
		int GetCols() const;

		// --- operator overrides --- 
		// -- indexers
		const double operator()(int row, int col) const;
		double& operator()(int row, int col);

		// -- Constant multiplication
		Matrix operator*(const double constant);

		// -- Matrix multiplication
		Matrix operator*(const Matrix& right) const;

		static int CheckRowChangeReturnSign(Matrix& matrix, int row, int col);
		Matrix GetTranspose() const;
		Matrix GetSubMatrixWithoutRowAndCol(int row, int col) const;
		Matrix GetReducedEchelonForm(int& singForDeterminant) const;
		double GetDeterminant() const;
		Matrix GetAdjointMatrix() const;
		// TODO: implementation + unit tests
		// Matrix GetInverse() const;
	};
}