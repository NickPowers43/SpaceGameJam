using System;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// The Math2d class will need a specialized 2x2 matrix with optimizations. This
    /// will represent the 2x2 matrix while also providing the operations that make
    /// calculations on a 2d plane a lot easier.
    /// </summary>
    public sealed class Matrix2x2
    {
        #region Static Fields

        private static int MATRIX_SIZE = 2;

        #endregion

        #region Fields

        private float[,] matrix;

        #endregion

        #region Static Variables

        /// <summary>
        /// This will return the idenity matrix.
        /// </summary>
        public static Matrix2x2 identity
        {
            get
            {
                var identityMatrix = new Matrix2x2();
                identityMatrix[0, 0] = 1;
                identityMatrix[1, 1] = 1;
                return identityMatrix;
            }
        }

        /// <summary>
        /// This will return a matrix with all
        /// zeros. This is easily accomplished
        /// by just creating a new matrix, but
        /// this will add to readibility.
        /// </summary>
        public static Matrix2x2 zero
        {
            get
            {
                return new Matrix2x2();
            }
        }

        /// <summary>
        /// This will return a matrix in which all
        /// the elements are 1.
        /// </summary>
        public static Matrix2x2 one
        {
            get
            {
                var matrix = new Matrix2x2();
                matrix[0, 0] = 1;
                matrix[0, 1] = 1;
                matrix[1, 0] = 1;
                matrix[1, 1] = 1;
                return matrix;
            }
        }

        #endregion

        #region Public Variables

        /// <summary>
        /// This will show if the matrix is an idenity matrix.
        /// </summary>
        public bool isIdentity
        {
            get
            {
                return matrix[0, 0] == 1 && matrix[0, 1] == 0 && matrix[1, 0] == 0 && matrix[1, 1] == 1;
            }
        }

        /// <summary>
        /// This will return the matrix that has
        /// been transposed. A transposed matrix
        /// has the row and column switched.
        /// </summary>
        public Matrix2x2 transpose
        {
            get
            {
                var transposeMatrix = new Matrix2x2();
                transposeMatrix[0, 0] = matrix[0, 0];
                transposeMatrix[0, 1] = matrix[1, 0];
                transposeMatrix[1, 0] = matrix[0, 1];
                transposeMatrix[1, 1] = matrix[1, 1];
                return transposeMatrix;
            }
        }

        /// <summary>
        /// This will return the inverted matrix.
        /// </summary>
        public Matrix2x2 inverse
        {
            get
            {
                var inverseMatrix = new Matrix2x2();
                float det = this.determinant;
                inverseMatrix[0, 0] = matrix[1, 1];
                inverseMatrix[0, 1] = -matrix[1, 0];
                inverseMatrix[1, 0] = -matrix[0, 1];
                inverseMatrix[1, 1] = matrix[0, 0];
                return inverseMatrix * (1 / det);
            }
        }

        /// <summary>
        /// This will calculate the determinant of the
        /// matrix.
        /// </summary>
        public float determinant
        {
            get
            {
                return matrix[0, 0] * matrix[1, 1] - matrix[1, 0] * matrix[0, 1];
            }
        }

        /// <summary>
        /// This will allow the caller to access the
        /// matrix directly.
        /// </summary>
        /// <param name="key1">The row of the matrix.</param>
        /// <param name="key2">The column of the matrix.</param>
        /// <returns>The element at the location.</returns>
        public float this[int key1, int key2]
        {
            get
            {
                if (key1 >= MATRIX_SIZE || key1 < 0 || key2 >= MATRIX_SIZE || key2 < 0)
                {
                    throw new IndexOutOfRangeException();
                }
                return matrix[key1, key2];
            }
            set
            {
                if (key1 >= MATRIX_SIZE || key1 < 0 || key2 >= MATRIX_SIZE || key2 < 0)
                {
                    throw new IndexOutOfRangeException();
                }
                matrix[key1, key2] = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// This will create a new matrix that is defaulted at the
        /// zero matrix.
        /// </summary>
        public Matrix2x2()
        {
            matrix = new float[MATRIX_SIZE, MATRIX_SIZE];
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This will transpose the matrix.
        /// </summary>
        public void Transpose()
        {
            float tmp = matrix[0, 1];
            matrix[0, 1] = matrix[1, 0];
            matrix[1, 0] = tmp;
        }

        /// <summary>
        /// This will invert the matrix.
        /// </summary>
        public void Invert()
        {
            float det = this.determinant;
            float tmp = matrix[0, 0];
            matrix[0, 0] = matrix[1, 1] / det;
            matrix[1, 1] = tmp / det;

            tmp = matrix[0, 1];
            matrix[0, 1] = -matrix[1, 0] / det;
            matrix[1, 0] = -tmp / det;
        }

        /// <summary>
        /// This will return a row of the matrix.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <returns>Vector that represents the row.</returns>
        public Vector2 GetRow(int row)
        {
            if (row >= MATRIX_SIZE || row < 0)
                throw new IndexOutOfRangeException();

            var rowVec = new Vector2();
            for (int col = 0; col < MATRIX_SIZE; col++)
            {
                rowVec[col] = matrix[row, col];
            }
            return rowVec;
        }

        /// <summary>
        /// This will return a column of the matrix.
        /// </summary>
        /// <param name="col">The column index.</param>
        /// <returns>Vector2 of the column.</returns>
        public Vector2 GetCol(int col)
        {
            if (col >= MATRIX_SIZE || col < 0)
                throw new IndexOutOfRangeException();

            var colVec = new Vector2();
            for (int row = 0; row < MATRIX_SIZE; row++)
            {
                colVec[row] = matrix[row, col];
            }
            return colVec;
        }

        /// <summary>
        /// This will set a row of the matrix.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="rowVec">The Vector2 of elements to set the row.</param>
        public void SetRow(int row, Vector2 rowVec)
        {
            if (row >= MATRIX_SIZE || row < 0)
                throw new IndexOutOfRangeException();
            for (int col = 0; col < MATRIX_SIZE; col++)
            {
                matrix[row, col] = rowVec[col];
            }
        }

        /// <summary>
        /// This will set a col of the matrix.
        /// </summary>
        /// <param name="row">The col index.</param>
        /// <param name="rowVec">The Vector2 of elements to set the col.</param>
        public void SetCol(int col, Vector2 colVec)
        {
            if (col >= MATRIX_SIZE || col < 0)
                throw new IndexOutOfRangeException();
            for (int row = 0; row < MATRIX_SIZE; row++)
            {
                matrix[row, col] = colVec[row];
            }
        }

        /// <summary>
        /// This will return the matrix in string format. 
        /// </summary>
        /// <returns>The matrix</returns>
        public override string ToString()
        {
            return "[" + matrix[0, 0] + ", " + matrix[0, 1] + "]\n[" + matrix[1, 0] + ", " + matrix[1, 1] + "]";
        }

        /// <summary>
        /// This overrides the hash code only to call the base one just
        /// to get rid of compiler warnings.
        /// </summary>
        /// <returns>The Matrix's hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// This does an element wise comparison on both matrices
        /// to determine if they are equal to each other.
        /// </summary>
        /// <param name="obj">The matrix to compare.</param>
        /// <returns>If the two matricies have the same elements.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Matrix2x2))
                return false;
            Matrix2x2 mat = (Matrix2x2)obj;
            return mat == this;
        }

        #endregion

        #region Operator Overload

        /// <summary>
        /// This will multiply the matrix by a scalar which is an element-wise
        /// multiplication.
        /// </summary>
        /// <param name="scalar">The scalar to multiply each element by.</param>
        /// <param name="matrix">The 2x2 matrix.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix2x2 operator *(float scalar, Matrix2x2 matrix)
        {
            return matrix * scalar;
        }

        /// <summary>
        /// This will multiply the matrix by a scalar which is an element-wise
        /// multiplication.
        /// </summary>
        /// <param name="matrix">The 2x2 matrix.</param>
        /// <param name="scalar">The scalar to multiply each element by.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix2x2 operator *(Matrix2x2 matrix, float scalar)
        {
            var scaledMatrix = new Matrix2x2();
            for (int row = 0; row < MATRIX_SIZE; row++)
            {
                for (int col = 0; col < MATRIX_SIZE; col++)
                {
                    scaledMatrix[row, col] = matrix[row, col] * scalar;
                }
            }
            return scaledMatrix;
        }

        /// <summary>
        /// This will add two 2x2 matricies together.
        /// </summary>
        /// <param name="mat1">2x2 Matrix</param>
        /// <param name="mat2">2x2 Matrix</param>
        /// <returns>The summed matrix.</returns>
        public static Matrix2x2 operator +(Matrix2x2 mat1, Matrix2x2 mat2)
        {
            var matrix = new Matrix2x2();
            for (int row = 0; row < MATRIX_SIZE; row++)
            {
                for (int col = 0; col < MATRIX_SIZE; col++)
                {
                    matrix[row, col] = mat1[row, col] + mat2[row, col];
                }
            }
            return matrix;
        }

        /// <summary>
        /// This will give the result of mat1 - mat2 where it is an
        /// element-wise subtraction.
        /// </summary>
        /// <param name="mat1">2x2 Matrix</param>
        /// <param name="mat2">2x2 Matrix</param>
        /// <returns>The result of mat1 - mat2</returns>
        public static Matrix2x2 operator -(Matrix2x2 mat1, Matrix2x2 mat2)
        {
            mat2 *= -1;
            return mat1 + mat2;
        }

        /// <summary>
        /// This will compare each element of both matricies and if each element is
        /// the same, then they are the same matrix.
        /// </summary>
        /// <param name="mat1">2x2 matrix</param>
        /// <param name="mat2">2x2 matrix</param>
        /// <returns>If the matrix share the same values.</returns>
        public static bool operator ==(Matrix2x2 mat1, Matrix2x2 mat2)
        {
            for (int row = 0; row < MATRIX_SIZE; row++)
            {
                for (int col = 0; col < MATRIX_SIZE; col++)
                {
                    if (mat1[row, col] != mat2[row, col])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// This will return true if both matricies don't share the same
        /// element.s
        /// </summary>
        /// <param name="mat1">2x2 matrix</param>
        /// <param name="mat2">2x2 matrix</param>
        /// <returns>If the matrix doesn't share the same values.</returns>
        public static bool operator !=(Matrix2x2 mat1, Matrix2x2 mat2)
        {
            return !(mat1 == mat2);
        }

        #endregion
    }
}