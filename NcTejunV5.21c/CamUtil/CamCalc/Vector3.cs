using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil
{
	/// <summary>
	/// ３次元ベクトルを表します。
	/// </summary>
	public readonly struct Vector3
	{
		/// <summary>０ベクトル(0.0, 0.0, 0.0)</summary>
		static public readonly Vector3 v0 = new Vector3(0.0, 0.0, 0.0);
		/// <summary>単位座標ベクトル(1.0, 0.0, 0.0)</summary>
		static public readonly Vector3 vi = new Vector3(1.0, 0.0, 0.0);
		/// <summary>単位座標ベクトル(0.0, 1.0, 0.0)</summary>
		static public readonly Vector3 vj = new Vector3(0.0, 1.0, 0.0);
		/// <summary>単位座標ベクトル(0.0, 0.0, 1.0)</summary>
		static public readonly Vector3 vk = new Vector3(0.0, 0.0, 1.0);

		/// <summary>ベクトルの長さ</summary>
		static public double Length(double x, double y) { return Math.Sqrt(x * x + y * y); }
		/// <summary>ベクトルの長さ</summary>
		static public double Length(double x, double y, double z) { return Math.Sqrt(x * x + y * y + z * z); }

		/// <summary>スカラー積</summary>
		static public double Vscal(Vector3 c1, Vector3 c2) {
			return c1.x * c2.x + c1.y * c2.y + c1.z * c2.z;
		}
		/// <summary>ベクトル積</summary>
		static public Vector3 Vvect(Vector3 c1, Vector3 c2) {
			return new Vector3(c1.y * c2.z - c1.z * c2.y, c1.z * c2.x - c1.x * c2.z, c1.x * c2.y - c1.y * c2.x);
		}

		/// <summary>
		/// 指定した２つのベクトルの角度を返します（0&gt;=angle&gt;=π）
		/// </summary>
		/// <param name="c1">ベクトル１</param>
		/// <param name="c2">ベクトル２</param>
		/// <returns>ラジアン単位のベクトル間の角度（０以上π以下）。</returns>
		static public double Vangle(Vector3 c1, Vector3 c2) {
			double vs = Vscal(c1.Unit(), c2.Unit());
			if (vs > 1.0) vs = 1.0;
			if (vs < -1.0) vs = -1.0;
			return Math.Acos(vs);
		}

		/// <summary>
		/// 各要素ごとに大きい方の値で作成します
		/// </summary>
		/// <param name="xyz1">ベクトル１</param>
		/// <param name="xyz2">ベクトル２</param>
		/// <returns>各Ｘ、Ｙ、Ｚごとに大きい方を採用したベクトル</returns>
		static public Vector3 Max(Vector3 xyz1, Vector3 xyz2) {
			return new Vector3(Math.Max(xyz1.x, xyz2.x), Math.Max(xyz1.y, xyz2.y), Math.Max(xyz1.z, xyz2.z));
		}
		/// <summary>
		/// 各要素ごとに小さい方の値で作成します
		/// </summary>
		/// <param name="xyz1">ベクトル１</param>
		/// <param name="xyz2">ベクトル２</param>
		/// <returns>各Ｘ、Ｙ、Ｚごとに小さい方を採用したベクトル</returns>
		static public Vector3 Min(Vector3 xyz1, Vector3 xyz2) {
			return new Vector3(Math.Min(xyz1.x, xyz2.x), Math.Min(xyz1.y, xyz2.y), Math.Min(xyz1.z, xyz2.z));
		}

		// /////////////////////////////
		// 以下は演算子のオーバーロード
		// /////////////////////////////

		/// <summary>３Ｄベクトルの==演算子</summary>
		public static bool operator ==(Vector3 c1, Vector3 c2) { return c1.Equals(c2); }
		/// <summary>３Ｄベクトルの!=演算子</summary>
		public static bool operator !=(Vector3 c1, Vector3 c2) { return !c1.Equals(c2); }
		/// <summary>３Ｄベクトルの－単項演算子</summary>
		public static Vector3 operator -(Vector3 c1) { return new Vector3(-c1.x, -c1.y, -c1.z); }
		/// <summary>３Ｄベクトルの＋演算子</summary>
		public static Vector3 operator +(Vector3 c1, Vector3 c2) { return new Vector3(c1.x + c2.x, c1.y + c2.y, c1.z + c2.z); }
		/// <summary>３Ｄベクトルの－演算子</summary>
		public static Vector3 operator -(Vector3 c1, Vector3 c2) { return new Vector3(c1.x - c2.x, c1.y - c2.y, c1.z - c2.z); }
		/// <summary>３Ｄベクトルと実数の＊演算子</summary>
		public static Vector3 operator *(Vector3 c1, double d2) { return new Vector3(c1.x * d2, c1.y * d2, c1.z * d2); }
		/// <summary>３Ｄベクトルと実数の＊演算子</summary>
		public static Vector3 operator *(double d2, Vector3 c1) { return new Vector3(c1.x * d2, c1.y * d2, c1.z * d2); }
		/// <summary>３Ｄベクトルと実数の／演算子</summary>
		public static Vector3 operator /(Vector3 c1, double d2) { return new Vector3(c1.x / d2, c1.y / d2, c1.z / d2); }
	
		/// <summary>３行３列のマトリックスと列ベクトルの積。結果はベクトル</summary>
		/// <param name="in1">入力（３行３列）</param>
		/// <param name="in2">入力（３次元ベクトル）</param>
		/// <returns>３次元ベクトル</returns>
		static public Vector3 operator *(Matrix in1, Vector3 in2) {
			if (in1.LengthRow != 3 || in1.LengthCol != 3) throw new Exception("afbaerfbqerh");
			return (in1 * in2.ToMatrix()).ToVector3();
		}

		// ////////////
		// 以上静的
		// ////////////








		/// <summary>各ｘｙｚの要素</summary>
		private readonly double x, y, z;

		/// <summary>X座標</summary>
		public double X { get { return x; } }
		/// <summary>Y座標</summary>
		public double Y { get { return y; } }
		/// <summary>Z座標</summary>
		public double Z { get { return z; } }

		/// <summary>絶対値（ベクトルの長さ）</summary>
		public double Abs { get { return Length(x, y, z); } }
		/// <summary>0ベクトル判定</summary>
		public bool Zero { get { return x == 0.0 && y == 0.0 && z == 0.0; } }


		/// <summary>
		/// ＸＹＺの座標値から作成するコンストラクタ
		/// </summary>
		/// <param name="x">Ｘ座標値</param>
		/// <param name="y">Ｙ座標値</param>
		/// <param name="z">Ｚ座標値</param>
		public Vector3(double x, double y, double z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}




		/// <summary>列ベクトルとしてマトリックスに変換します</summary>
		public Matrix ToMatrix() { return new Matrix(new double[,] { { x }, { y }, { z } }); }

		/// <summary>
		/// 基本軸回りに回転したベクトルを出力します
		/// </summary>
		/// <param name="g171819">平面設定値（=17[0,0,1],18[0,1,0],19[1,0,0]）</param>
		/// <param name="ang">軸周りの回転角度(単位：radian)</param>
		/// <returns>このインスタンスを回転したベクトル</returns>
		public Vector3 Rotation(int g171819, double ang) {
			Vector3 mat;
			switch (g171819) {
			case 17:	// Ｚ軸回り
				// |Cos(angl) -Sin(angl) 0| |X|
				// |Sin(angl)  Cos(angl) 0|*|Y|
				// | 0         0         1| |Z|
				mat = new Vector3(
					this.x * Math.Cos(ang) - this.y * Math.Sin(ang),
					this.x * Math.Sin(ang) + this.y * Math.Cos(ang),
					this.z);
				break;
			case 18:	// Ｙ軸回り
				// | Cos(angl) 0 Sin(angl)| |X|
				// | 0         1 0        |*|Y|
				// |-Sin(angl) 0 Cos(angl)| |Z|
				mat = new Vector3(
					this.z * Math.Sin(ang) + this.x * Math.Cos(ang),
					this.y,
					this.z * Math.Cos(ang) - this.x * Math.Sin(ang));
				break;
			case 19:	// Ｘ軸回り
				// |1 0          0        | |X|
				// |0 Cos(angl) -Sin(angl)|*|Y|
				// |0 Sin(angl)  Cos(angl)| |Z|
				mat = new Vector3(
					this.x,
					this.y * Math.Cos(ang) - this.z * Math.Sin(ang),
					this.y * Math.Sin(ang) + this.z * Math.Cos(ang));
				break;
			default:
				throw new Exception("kawfarjb");
			}
			return mat;
		}



		/// <summary>
		/// 指定の大きさのベクトルに変換します
		/// </summary>
		/// <param name="unit">ベクトルの大きさ</param>
		/// <returns>大きさを変換したベクトル</returns>
		public Vector3 Unit(double unit) { return Unit() * unit; }

		/// <summary>
		/// 単位ベクトルに変換します
		/// </summary>
		/// <returns>大きさ１に変換したベクトル</returns>
		public Vector3 Unit() {
			if (this.Zero) throw new Exception("単位ベクトルの計算で０ベクトルが指示されました。");
			Vector3 vec = this / this.Abs;
			if (Math.Abs(vec.x) >= 1.0 || (vec.y == 0.0 && vec.z == 0.0)) { return Math.Sign(vec.x) * vi; }
			if (Math.Abs(vec.y) >= 1.0 || (vec.z == 0.0 && vec.x == 0.0)) { return Math.Sign(vec.y) * vj; }
			if (Math.Abs(vec.z) >= 1.0 || (vec.x == 0.0 && vec.y == 0.0)) { return Math.Sign(vec.z) * vk; }
			return vec;
		}

		/// <summary>２次元ベクトルへ変換します</summary>
		/// <param name="g171819">平面指定ＧコードNo.(17,18,19)</param>
		/// <returns>このインスタンスの変換された２次元ベクトル</returns>
		public CamUtil._Geometry_Plane.Vector2 ToVector2(int g171819) {
			switch (g171819) {
			case 17: return new CamUtil._Geometry_Plane.Vector2(x, y);
			case 18: return new CamUtil._Geometry_Plane.Vector2(z, x);
			case 19: return new CamUtil._Geometry_Plane.Vector2(y, z);
			default: throw new Exception("aefaebrfb");
			}
		}

		/// <summary>
		/// 指定平面へベクトルを投影します
		/// </summary>
		/// <param name="plane">指定平面の垂直ベクトル</param>
		/// <returns>投影された３次元ベクトル</returns>
		public Vector3 Projection(Vector3 plane) {
			if (plane == vi || plane == -vi) return new Vector3(0.0, y, z);
			if (plane == vj || plane == -vj) return new Vector3(x, 0.0, z);
			if (plane == vk || plane == -vk) return new Vector3(x, y, 0.0);
			// 暫定チェック
			{
				Vector3 tmp = plane.Unit(Vscal(this, plane.Unit())) + Vvect(plane.Unit(), Vvect(this, plane.Unit()));
				LogOut.CheckCount("Vector3 0230", false, "this={this.ToString()} tmp={tmp.ToString()}");
				if (Math.Abs(this.x - tmp.x) > 0.0001 || Math.Abs(this.y - tmp.y) > 0.0001 || Math.Abs(this.z - tmp.z) > 0.0001)
					throw new Exception("awefbqrhfbqwerhbf");
			}
			return Vvect(plane.Unit(), Vvect(this, plane.Unit()));
		}

		/// <summary>
		/// 各軸個別の異方スケーリングを実行します
		/// </summary>
		/// <param name="scale">各軸のスケーリングする値</param>
		/// <returns>各軸ごとにスケーリングされた３次元ベクトル</returns>
		public Vector3 Scaling(Vector3 scale) {
			return new Vector3(this.x * scale.x, this.y * scale.y, this.z * scale.z);
		}


		/// <summary>このインスタンスの数値を、それと等価な文字列形式に変換します。</summary>
		/// <returns>このインスタンスの値の文字列形式。</returns>
		public override string ToString() { return String.Format("X={0,10:f4} Y={1,10:f4} Z={2,10:f4}", x, y, z); }

		/// <summary>このインスタンスと指定した Vector3 オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj が Vector3 のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public override bool Equals(object obj) { return Equals((Vector3)obj); }
		private bool Equals(Vector3 obj) { return this.x == obj.x && this.y == obj.y && this.z == obj.z; }

		/// <summary>このインスタンスのハッシュ コードを返します。</summary>
		/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
		public override int GetHashCode() { return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode(); }
	}
}
