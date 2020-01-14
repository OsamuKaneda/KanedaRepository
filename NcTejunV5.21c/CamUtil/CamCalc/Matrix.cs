using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil
{
	/// <summary>
	/// Ｎ行Ｍ列の行列を表します。[不変]
	/// </summary>
	public class Matrix
	{

		// /////////////////////////////
		// 以下は演算子のオーバーロード
		// /////////////////////////////

		/// <summary>－単項演算子</summary>
		public static Matrix operator -(Matrix in1) {
			Matrix output = new Matrix(in1.LengthRow, in1.LengthCol);
			for (int ii = 0; ii < in1.LengthRow; ii++) for (int jj = 0; jj < in1.LengthCol; jj++)
					output.mat[ii, jj] = -in1[ii, jj];
			return output;
		}
		/// <summary>＋演算子</summary>
		static public Matrix operator +(Matrix in1, Matrix in2) {
			Matrix output = new Matrix(in1.LengthRow, in1.LengthCol);
			if (in1.LengthRow != in2.LengthRow || in1.LengthCol != in2.LengthCol)
				throw new Exception("行数あるいは列数が等しくない");
			for (int ii = 0; ii < in1.LengthRow; ii++) for (int jj = 0; jj < in1.LengthCol; jj++)
					output.mat[ii, jj] = in1[ii, jj] + in2[ii, jj];
			return output;
		}
		/// <summary>－演算子</summary>
		static public Matrix operator -(Matrix in1, Matrix in2) {
			Matrix output = new Matrix(in1.LengthRow, in1.LengthCol);
			if (in1.LengthRow != in2.LengthRow || in1.LengthCol != in2.LengthCol)
				throw new Exception("行数あるいは列数が等しくない");
			for (int ii = 0; ii < in1.LengthRow; ii++) for (int jj = 0; jj < in1.LengthCol; jj++)
					output.mat[ii, jj] = in1[ii, jj] - in2[ii, jj];
			return output;
		}
		/// <summary>マトリックスと実数の＊演算子</summary>
		static public Matrix operator *(Matrix in1, double dd) {
			Matrix output = new Matrix(in1.LengthRow, in1.LengthCol);
			for (int ii = 0; ii < in1.LengthRow; ii++) for (int jj = 0; jj < in1.LengthCol; jj++)
					output.mat[ii, jj] = in1[ii, jj] * dd;
			return output;
		}
		/// <summary>実数とマトリックスの＊演算子</summary>
		static public Matrix operator *(double dd, Matrix in1) {
			Matrix output = new Matrix(in1.LengthRow, in1.LengthCol);
			for (int ii = 0; ii < in1.LengthRow; ii++) for (int jj = 0; jj < in1.LengthCol; jj++)
					output.mat[ii, jj] = in1[ii, jj] * dd;
			return output;
		}
		/// <summary>マトリックスとマトリックスの＊演算子</summary>
		/// <param name="in1">入力１（Ｋ行Ｉ列）</param>
		/// <param name="in2">入力２（Ｉ行Ｊ列）</param>
		/// <returns>出力（Ｋ行Ｊ列）</returns>
		static public Matrix operator *(Matrix in1, Matrix in2) {
			// in1[0,0] in1[0,1] in1[0,2] in1[0,3]  in2[0,0] in2[0,1] in2[0,2] in[0,3] in[0,4]
			// in1[1,0] in1[1,1] in1[1,2] in1[1,3]  in2[1,0] in2[1,1] in2[1,2] in[1,3] in[1,4]
			// in1[2,0] in1[2,1] in1[2,2] in1[2,3]  in2[2,0] in2[2,1] in2[2,2] in[2,3] in[2,4]
			//                                      in2[3,0] in2[3,1] in2[3,2] in[3,3] in[3,4]
			if (in1.LengthCol != in2.LengthRow) throw new Exception("入力１の列数と入力２の行数が等しくない");
			Matrix output = new Matrix(in1.LengthRow, in2.LengthCol);
			for (int ii = 0; ii < in1.LengthRow; ii++)
				for (int jj = 0; jj < in2.LengthCol; jj++)
					for (int kk = 0; kk < in2.LengthRow; kk++)
						output.mat[ii, jj] += in1[ii, kk] * in2[kk, jj];
			return output;
		}
		/// <summary>マトリックスと実数の／演算子</summary>
		/// <param name="in1">入力１（Ｋ行Ｉ列）</param>
		/// <param name="in2">入力２</param>
		/// <returns>出力（Ｋ行Ｊ列）</returns>
		static public Matrix operator /(Matrix in1, double in2) {
			Matrix output = new Matrix(in1.LengthRow, in1.LengthCol);
			for (int ii = 0; ii < in1.LengthRow; ii++) for (int jj = 0; jj < in1.LengthCol; jj++)
					output.mat[ii, jj] = in1[ii, jj] / in2;
			return output;
		}

		// /////////////////////////////
		// 以上 static
		// /////////////////////////////









		
		private readonly double[,] mat;

		/// <summary>行数</summary>
		public int LengthRow { get { return this.mat.GetLength(0); } }
		/// <summary>列数</summary>
		public int LengthCol { get { return this.mat.GetLength(1); } }
		/// <summary>指定した要素の値（数値は０から）</summary>
		public double this[int ii, int jj] { get { return mat[ii, jj]; } }

		/// <summary>指定した列数、行数の０行列を作成します</summary>
		/// <param name="row">行数</param>
		/// <param name="col">列数</param>
		public Matrix(int row, int col) {
			this.mat = new double[row, col];
		}
		/// <summary>正方単位行列を作成します</summary>
		/// <param name="dimension">行数、列数（正方行列）</param>
		public Matrix(int dimension) {
			this.mat = new double[dimension, dimension];
			for (int ii = 0; ii < dimension; ii++)
				for (int jj = 0; jj < dimension; jj++)
					this.mat[ii, jj] = (ii == jj) ? 1.0 : 0.0;
		}
		/// <summary>２次元の実数配列を元に行列を作成します</summary>
		public Matrix(double[,] dd) {
			this.mat = new double[dd.GetLength(0), dd.GetLength(1)];
			for (int ii = 0; ii < dd.GetLength(0); ii++)
				for (int jj = 0; jj < dd.GetLength(1); jj++)
					this.mat[ii, jj] = dd[ii, jj];
		}
		/// <summary>クローンを作成します</summary>
		public Matrix Clone() { return new Matrix(this.mat); }


		/// <summary>
		/// このインスタンスの転置行列を作成します
		/// </summary>
		/// <returns>転置行列</returns>
		public Matrix Transposed() {
			Matrix c2 = new Matrix(LengthCol, LengthRow);
			for (int ii = 0; ii < LengthRow; ii++)
				for (int jj = 0; jj < LengthCol; jj++)
					c2.mat[jj, ii] = this.mat[ii, jj];
			return c2;
		}

		/// <summary>
		/// 行列式の値を計算します
		/// </summary>
		/// <returns>行列式の値</returns>
		public double Determinant() {
			if (LengthRow != LengthCol) throw new Exception("正方行列でない");
			if (LengthRow == 1)
				return this.mat[0, 0];
			double ans = 0.0;
			for (int jj = 0; jj < LengthRow; jj++)
				ans += this.mat[0, jj] * this.Cofactor(0, jj);
			return ans;
		}
		/// <summary>
		/// このインスタンスの逆行列を作成します
		/// </summary>
		/// <returns>逆行列</returns>
		public Matrix Inverse() {
			if (LengthRow != LengthCol) throw new Exception("正方行列でない");
			if (LengthRow == 1) throw new Exception("次元数が１である");
			Matrix ee = new Matrix(LengthRow, LengthCol);
			for (int ii = 0; ii < LengthRow; ii++)
				for (int jj = 0; jj < LengthCol; jj++)
					ee.mat[ii, jj] = this.Cofactor(ii, jj);
			return ee.Transposed() / this.Determinant();
		}
		/// <summary>
		/// このインスタンスの余因子（cofactor）を計算します（小行列に(-1)^(si+sj) を掛けたもの）
		/// </summary>
		/// <param name="si">余因子の行番号</param>
		/// <param name="sj">余因子の列番号</param>
		/// <returns>余因子</returns>
		private double Cofactor(int si, int sj) {
			if (LengthRow != LengthCol) throw new Exception("正方行列でない");
			Matrix ee = new Matrix(LengthRow - 1, LengthRow - 1);
			for (int ii = 0; ii < LengthRow; ii++)
				for (int jj = 0; jj < LengthRow; jj++) {
					if (ii < si && jj < sj) ee.mat[ii, jj] = this.mat[ii, jj];
					if (ii < si && jj > sj) ee.mat[ii, jj - 1] = this.mat[ii, jj];
					if (ii > si && jj < sj) ee.mat[ii - 1, jj] = this.mat[ii, jj];
					if (ii > si && jj > sj) ee.mat[ii - 1, jj - 1] = this.mat[ii, jj];
				}
			return (1 - 2 * ((si + sj) % 2)) * ee.Determinant();
		}

		/// <summary>
		/// １行１列の行列を実数に変換します
		/// </summary>
		/// <returns>変換された実数値</returns>
		public Double ToDouble() {
			if (this.LengthRow != 1 || this.LengthCol != 1) throw new Exception("実数に変換可能な行列は１行１列のみです。");
			return mat[0, 0];
		}
		/// <summary>
		/// ３行１列の行列をVector3へ変換します
		/// </summary>
		/// <returns>変換された３次元ベクトル</returns>
		public Vector3 ToVector3() {
			if (this.LengthRow != 3 || this.LengthCol != 1) throw new Exception("３次元ベクトルに変換可能な行列は３行１列のみです。");
			return new Vector3(mat[0, 0], mat[1, 0], mat[2, 0]);
		}
		/// <summary>
		/// ３行Ｎ列の行列をVector3へ変換します
		/// </summary>
		/// <param name="col">列番号</param>
		/// <returns>変換された３次元ベクトル</returns>
		public Vector3 ToVector3(int col) {
			if (this.LengthRow != 3) throw new Exception("３次元ベクトルに変換可能な行列は３行のみです。");
			return new Vector3(mat[0, col], mat[1, col], mat[2, col]);
		}
		/// <summary>実数配列へ変換します</summary>
		/// <returns>変換された実数２次元配列</returns>
		public double[,] ToDoubleArray() {
			double[,] output = new double[LengthRow, LengthCol];
			for (int ii = 0; ii < LengthRow; ii++)
				for (int jj = 0; jj < LengthCol; jj++)
					output[ii, jj] = this.mat[ii, jj];
			return output;
		}

		/// <summary>
		/// ３行３列の行列が直交行列であるか判定します
		/// </summary>
		/// <returns> 直交行列である場合 true、そうでない場合 false</returns>
		public bool Orthogonal() {
			// 行列Pを，各成分が実数であるようなn次の正方行列とするとき，次の４条件は同値（必要かつ十分）になる．
			// よって、いずれか１つを直交行列(orthogonal matrix)の定義とすれば他は直交行列の性質となる
			//(I)　tP P=P tP=E すなわち P−1=tP 
			// 	［* 直交行列の逆行列は転置行列に等しい］
			//(II)　Pv1·Pv2=v1·V2 
			// 	［* 直交行列（で表される一次変換）はベクトルの内積を変えない］
			//(III)　|Pv1|=|v1| 
			// 	［* 直交行列（で表される一次変換）はベクトルの大きさを変えない］
			//(IV)　行列Pの各列を表す列ベクトルvi　（i=1～n）は互いに垂直で，各々の大きさは1である． 
			// 	［* 直交行列の列ベクトルは正規直交基底をなす］

			// ○１つのベクトルとその他の１つの要素より１つの直交行列が定義できるか
			//	(IV)より
			//	vv0 vv1.X が定義済みとすると
			//	vv0·vv1==0、|vv1|=1よりvv1が決まる
			//	vv0·vv2==0、vv1·vv2==0、|vv2|=1よりvv2が決まる
			// ○主対角の３つの数値より１つの直交行列が定義できるか
			//	？？
			Vector3[] vv = new Vector3[3];
			vv[0] = new Vector3(mat[0, 0], mat[1, 0], mat[2, 0]);
			vv[1] = new Vector3(mat[0, 1], mat[1, 1], mat[2, 1]);
			vv[2] = new Vector3(mat[0, 2], mat[1, 2], mat[2, 2]);
			if (Math.Abs(vv[0].Abs - 1.0) > 0.00001) return false;
			if (Math.Abs(vv[1].Abs - 1.0) > 0.00001) return false;
			if (Math.Abs(vv[2].Abs - 1.0) > 0.00001) return false;
			if (Math.Sqrt(Math.Abs(Vector3.Vscal(vv[0], vv[1]))) > 0.00001) return false;
			if (Math.Sqrt(Math.Abs(Vector3.Vscal(vv[0], vv[2]))) > 0.00001) return false;
			if (Math.Sqrt(Math.Abs(Vector3.Vscal(vv[1], vv[2]))) > 0.00001) return false;
			return true;
		}

		/// <summary>このインスタンスと指定した Matrix オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public bool Equals(Matrix obj) {
			if (this.LengthRow != obj.LengthRow) return false;
			if (this.LengthCol != obj.LengthCol) return false;
			for (int ii = 0; ii < LengthRow; ii++) for (int jj = 0; jj < LengthCol; jj++)
					if (this.mat[ii, jj] != obj.mat[ii, jj]) return false;
			return true;
		}
	}
}
