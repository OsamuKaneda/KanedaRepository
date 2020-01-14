using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil
{
	/// <summary>
	///剛体の回転を表す四元数です。( cos(θ/2), Vx*sin(θ/2), Vy*sin(θ/2), Vz*sin(θ/2) )
	/// </summary>
	public readonly struct Quaternion
	{
		// /////////////////////////////
		// 演算子のオーバーロード
		// /////////////////////////////
		/// <summary>四元数の－単項演算子（同一の四元数になる）</summary>
		public static Quaternion operator -(Quaternion q1) { return new Quaternion(-q1.W, -q1.X, -q1.Y, -q1.Z); }
		/// <summary>四元数の＋演算子</summary>
		public static Quaternion operator +(Quaternion q1, Quaternion q2) { return new Quaternion(q1.W + q2.W, q1.X + q2.X, q1.Y + q2.Y, q1.Z + q2.Z); }
		/// <summary>四元数の－演算子</summary>
		public static Quaternion operator -(Quaternion q1, Quaternion q2) { return new Quaternion(q1.W - q2.W, q1.X - q2.X, q1.Y - q2.Y, q1.Z - q2.Z); }
		/// <summary>四元数と四元数の＊演算子</summary>
		public static Quaternion operator *(Quaternion q1, Quaternion q2) {
			// W = q1.W * q2.W - q1.V * q2.V,   V = q1.W * q2.V + q2.W * q1.V + q1.V x q2.V
			return new Quaternion(
				q1.W * q2.W - q1.X * q2.X - q2.Y * q1.Y - q1.Z * q2.Z,
				q1.Y * q2.Z - q1.Z * q2.Y + q2.W * q1.X + q1.W * q2.X,
				q1.Z * q2.X - q1.X * q2.Z + q2.W * q1.Y + q1.W * q2.Y,
				q1.X * q2.Y - q1.Y * q2.X + q2.W * q1.Z + q1.W * q2.Z);
		}





		/// <summary>実数部</summary>
		private readonly double w;
		/// <summary>虚数部</summary>
		private readonly Vector3 v;

		/// <summary>実数部</summary>
		public double W { get { return w; } }
		/// <summary>虚数部</summary>
		public Vector3 V { get { return v; } }
		/// <summary>虚数部 i</summary>
		public double X { get { return v.X; } }
		/// <summary>虚数部 j</summary>
		public double Y { get { return v.Y; } }
		/// <summary>虚数部 k</summary>
		public double Z { get { return v.Z; } }
		/// <summary>虚数部のベクトルの大きさ</summary>
		public double Abs { get { return v.Abs; } }

		/// <summary>
		/// ＷとＸＹＺの座標値から作成します
		/// </summary>
		/// <param name="w"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public Quaternion(double w, double x, double y, double z) {
			this.w = w;
			this.v = new Vector3(x, y, z);
		}
		/*
		/// <summary>
		/// ３行３列の回転行列から作成します
		/// </summary>
		/// <param name="rmat">回転行列</param>
		public Quaternion(Matrix rmat) {
			// 任意ベクトル回りの回転公式（列ベクトルの場合）
			// |VxVx(1-cosθ)+   cosθ  VyVx(1-cosθ)-Vz*sinθ  VzVx(1-cosθ)+Vy*sinθ| |x|
			// |VxVy(1-cosθ)+Vz*sinθ  VyVy(1-cosθ)+   cosθ  VzVy(1-cosθ)-Vx*sinθ|*|y|
			// |VxVz(1-cosθ)-Vy*sinθ  VyVz(1-cosθ)+Vx*sinθ  VzVz(1-cosθ)+   cosθ| |z|
			//
			// クォータニオン（四元数）
			// quaternion = ( cos(θ/2), Vx*sin(θ/2), Vy*sin(θ/2), Vz*sin(θ/2) )
			// 行列への変換（列ベクトル）
			// |1-2*q2*q2-2*q3*q3    2*q1*q2-2*q0*q3    2*q1*q3+2*q0*q2|
			// |  2*q1*q2+2*q0*q3  1-2*q1*q1-2*q3*q3    2*q2*q3-2*q0*q1|
			// |  2*q1*q3-2*q0*q2    2*q2*q3+2*q0*q1  1-2*q1*q1-2*q2*q2|

			////////////////////////////////////////////
			// 回転行列からクォータニオンの絶対値の計算
			////////////////////////////////////////////
			// 半角の公式より
			// q0*q0 = (1+cosθ)/2
			// q1*q1 = Vx*Vx*(1-cosθ)/2
			// q2*q2 = Vy*Vy*(1-cosθ)/2
			// q3*q3 = Vz*Vz*(1-cosθ)/2
			// ∴
			// |1  1 -1 -1| |q0*q0|   |(1+cosθ)/2 +Vx*Vx*(1-cosθ) -(1-cosθ)/2|   |a11|
			// |1 -1  1 -1|*|q1*q1| = |(1+cosθ)/2 +Vy*Vy*(1-cosθ) -(1-cosθ)/2| = |a22|
			// |1 -1 -1  1| |q2*q2|   |(1+cosθ)/2 +Vz*Vz*(1-cosθ) -(1-cosθ)/2|   |a33|
			// |1  1  1  1| |q3*q3|   |(1+cosθ)/2 +(1-cosθ)/2|                    | 1 |
			// 逆行列を計算し、
			// |q0*q0|     | 1  1  1  1| |a11|
			// |q1*q1|=1/4*| 1 -1 -1  1|*|a22|
			// |q2*q2|     |-1  1 -1  1| |a33|
			// |q3*q3|     |-1 -1  1  1| | 1 |
			//
			Matrix mtmp44, fugo, qtmp, quat;

			mtmp44 = new Matrix(new double[,] {
				{  1,  1,  1,  1 },
				{  1, -1, -1,  1 },
				{ -1,  1, -1,  1 },
				{ -1, -1,  1,  1 }
				});
			mtmp44 = mtmp44 / 4.0;

			// qtmp = ( a11, a22, a33, 1.0 )'
			qtmp = new Matrix(new double[,] { { rmat[0, 0] }, { rmat[1, 1] }, { rmat[2, 2] }, { 1.0 } });

			// qtmp = ( q0*q0, q1*q1, q2*q2, q3*q3 )'
			qtmp = mtmp44 * qtmp;
			// 平方根を取る
			// qq2 = ( |q0|, |q1|, |q2|, |q3| )'
			qtmp = new Matrix(new double[,] {
				{ Math.Sqrt(Math.Max(0.0, qtmp[0, 0])) },
				{ Math.Sqrt(Math.Max(0.0, qtmp[1, 0])) },
				{ Math.Sqrt(Math.Max(0.0, qtmp[2, 0])) },
				{ Math.Sqrt(Math.Max(0.0, qtmp[3, 0])) }
			});
			// 自乗和の平方根で割る
			quat = qtmp / Math.Sqrt((qtmp.Transposed() * qtmp).ToDouble());

			/////////////////////////////////////////////////////////////////
			// 符号の設定（絶対値が一番大きいものを正として他の符号を決定）
			/////////////////////////////////////////////////////////////////
			// aa[2,1]-a[1,2] = 2*q2*q3+2*q0*q3 - 2*q2*q3-2*q0*q1 = 4*(q0*q1)
			// aa[0,2]-a[2,0] = 2*q1*q3+2*q0*q2 - 2*q1*q3-2*q0*q2 = 4*(q0*q2)
			// aa[1,0]-a[0,1] = 2*q1*q2+2*q0*q3 - 2*q1*q2-2*q0*q3 = 4*(q0*q3)
			// aa[1,0]+a[0,1] = 2*q1*q2+2*q0*q3 + 2*q1*q2-2*q0*q3 = 4*(q1*q2)
			// aa[0,2]+a[2,0] = 2*q1*q3+2*q0*q2 + 2*q1*q3-2*q0*q2 = 4*(q1*q3)
			// aa[2,1]+a[1,2] = 2*q2*q3+2*q0*q3 + 2*q2*q3-2*q0*q1 = 4*(q2*q3)
			if (quat[0, 0] >= quat[1, 0] && quat[0, 0] >= quat[2, 0] && quat[0, 0] >= quat[3, 0]) {
				fugo = new Matrix(new double[,] {
					{ 1, 0, 0, 0 },
					{ 0, Math.Sign(rmat[2, 1] - rmat[1, 2]), 0, 0 },
					{ 0, 0, Math.Sign(rmat[0, 2] - rmat[2, 0]), 0 },
					{ 0, 0, 0, Math.Sign(rmat[1, 0] - rmat[0, 1]) }
				});
			}
			else if (quat[1, 0] >= quat[0, 0] && quat[1, 0] >= quat[2, 0] && quat[1, 0] >= quat[3, 0]) {
				fugo = new Matrix(new double[,] {
					{ Math.Sign(rmat[2, 1] - rmat[1, 2]), 0, 0, 0 },
					{ 0, 1, 0, 0 },
					{ 0, 0, Math.Sign(rmat[1, 0] + rmat[0, 1]), 0 },
					{ 0, 0, 0, Math.Sign(rmat[0, 2] + rmat[2, 0]) }
				});
			}
			else if (quat[2, 0] >= quat[0, 0] && quat[2, 0] >= quat[1, 0] && quat[2, 0] >= quat[3, 0]) {
				fugo = new Matrix(new double[,] {
					{ Math.Sign(rmat[0, 2] - rmat[2, 0]), 0, 0, 0 },
					{ 0, Math.Sign(rmat[1, 0] + rmat[0, 1]), 0, 0 },
					{ 0, 0, 1, 0 },
					{ 0, 0, 0, Math.Sign(rmat[2, 1] + rmat[1, 2]) }
				});
			}
			else if (quat[3, 0] >= quat[0, 0] && quat[3, 0] >= quat[1, 0] && quat[3, 0] >= quat[2, 0]) {
				fugo = new Matrix(new double[,] {
					{ Math.Sign(rmat[1, 0] - rmat[0, 1]), 0, 0, 0 },
					{ 0, Math.Sign(rmat[0, 2] + rmat[2, 0]), 0, 0 },
					{ 0, 0, Math.Sign(rmat[2, 1] + rmat[1, 2]), 0 },
					{ 0, 0, 0, 1 }
				});
			}
			else {
				throw new Exception("awfwebrfw");
			}
			Matrix ans = fugo * quat;

			w = ans[0, 0];
			v = new Vector3(ans[1, 0], ans[2, 0], ans[3, 0]);
		}
		*/

		/// <summary>単位四元数を作成します</summary>
		public Unit ToUnit() { return new Unit(w, v.X, v.Y, v.Z); }

		/// <summary>実数の２次元配列に出力します</summary>
		public double[,] ToDoubleArray() { return new double[,] { { W }, { X }, { Y }, { Z } }; }

		/// <summary>このインスタンスの共役四元数を出力します(conjugation)</summary>
		public Quaternion Conjugation() { return new Quaternion(W, -X, -Y, -Z); }



		/// <summary>
		/// 実数部が正に限定された単位四元数です。３次元の回転を表します。
		/// </summary>
		public readonly struct Unit
		{
			/// <summary>
			/// 回転軸とその軸回りの回転角度から回転を定義します
			/// </summary>
			/// <param name="angl">軸周りの回転角度(radian)</param>
			/// <param name="n">回転軸</param>
			/// <returns></returns>
			static public Unit RotQuat(double angl, Vector3 n) { return new Unit(angl, n); }
			/// <summary>
			/// Ｘ軸周りの回転角度から回転を定義します
			/// </summary>
			/// <param name="angl">軸周りの回転角度(radian)</param>
			/// <returns></returns>
			static public Unit RotQuat_X(double angl) { return new Unit(angl, Vector3.vi); }
			/// <summary>
			/// Ｙ軸周りの回転角度から回転を定義します
			/// </summary>
			/// <param name="angl">軸周りの回転角度(radian)</param>
			/// <returns></returns>
			static public Unit RotQuat_Y(double angl) { return new Unit(angl, Vector3.vj); }
			/// <summary>
			/// Ｚ軸周りの回転角度から回転を定義します
			/// </summary>
			/// <param name="angl">軸周りの回転角度(radian)</param>
			/// <returns></returns>
			static public Unit RotQuat_Z(double angl) { return new Unit(angl, Vector3.vk); }

			// /////////////////////////////
			// 以下は演算子のオーバーロード
			// /////////////////////////////

			/// <summary>３Ｄベクトルの==演算子</summary>
			public static bool operator ==(Quaternion.Unit c1, Quaternion.Unit c2) { return c1.Equals(c2); }
			/// <summary>３Ｄベクトルの!=演算子</summary>
			public static bool operator !=(Quaternion.Unit c1, Quaternion.Unit c2) { return !c1.Equals(c2); }

			/// <summary>単位四元数と単位四元数の＊演算子</summary>
			public static Unit operator *(Quaternion.Unit q1, Quaternion.Unit q2) {
				// W = q1.W * q2.W - q1.V * q2.V,   V = q1.W * q2.V + q2.W * q1.V + q1.V x q2.V
				double v1ww = q1.W; Vector3 v1 = q1.V;
				double v2ww = q2.W; Vector3 v2 = q2.V;
				return new Unit(
					v1ww * v2ww - v1.X * v2.X - v2.Y * v1.Y - v1.Z * v2.Z,
					v1.Y * v2.Z - v1.Z * v2.Y + v2ww * v1.X + v1ww * v2.X,
					v1.Z * v2.X - v1.X * v2.Z + v2ww * v1.Y + v1ww * v2.Y,
					v1.X * v2.Y - v1.Y * v2.X + v2ww * v1.Z + v1ww * v2.Z);
			}

			// ////////////
			// 以上静的
			// ////////////








			// quaternion = ( cos(θ/2), Vx*sin(θ/2), Vy*sin(θ/2), Vz*sin(θ/2) )
			private readonly double ag;		// >= 0, <= π
			private readonly Vector3 vc;		// Length == 1

			/// <summary>回転角θ（&gt;=0, &lt;=π）</summary>
			public double Angle { get { return ag; } }
			/// <summary>回転軸（単位ベクトル）</summary>
			public Vector3 Vector { get { return vc; } }
			/// <summary>四元数実数部</summary>
			public double W { get { return Math.Cos(ag / 2.0); } }
			/// <summary>四元数虚数部</summary>
			public Vector3 V { get { return vc * Math.Sin(ag / 2.0); } }

			/// <summary>
			/// 回転角(radian)と回転軸から作成します
			/// </summary>
			/// <param name="angl">回転角度(radian)</param>
			/// <param name="n">回転軸</param>
			private Unit(double angl, Vector3 n) {
				while (angl > Math.PI) angl -= 2.0 * Math.PI;
				while (angl <= -Math.PI) angl += 2.0 * Math.PI;
				this.ag = Math.Abs(angl);
				if (angl != 0) {
					if (n.Zero) throw new Exception("回転軸が０ベクトルです。");
					this.vc = n.Unit() * Math.Sign(angl);
				}
				else
					this.vc = n.Zero ? Vector3.vk : n.Unit();
			}
			/// <summary>
			/// 実数部Ｗと虚数部ＸＹＺの値から作成します
			/// </summary>
			/// <param name="w">実数部Ｗ</param>
			/// <param name="x">虚数部ベクトルのＸ</param>
			/// <param name="y">虚数部ベクトルのＹ</param>
			/// <param name="z">虚数部ベクトルのＺ</param>
			public Unit(double w, double x, double y, double z) {
				this = new Unit(2.0 * Math.Acos(w), new Vector3(x, y, z));
			}
			/// <summary>
			/// ３行３列の回転行列（直行行列）から作成します
			/// </summary>
			/// <param name="mat">回転行列</param>
			public Unit(Matrix mat) {
				// |1-2*q2*q2-2*q3*q3    2*q1*q2-2*q0*q3    2*q1*q3+2*q0*q2|
				// |  2*q1*q2+2*q0*q3  1-2*q1*q1-2*q3*q3    2*q2*q3-2*q0*q1|
				// |  2*q1*q3-2*q0*q2    2*q2*q3+2*q0*q1  1-2*q1*q1-2*q2*q2|
				// トレース（対角要素の総和）に１を加えると
				// (1-2*q2*q2-2*q3*q3) + (1-2*q1*q1-2*q3*q3) + (1-2*q1*q1-2*q2*q2) + 1 = 4(1-V・V) = 4*W*W
				double w = Math.Sqrt(mat[0, 0] + mat[1, 1] + mat[2, 2] + 1) / 2.0;
				// また
				// mat[2, 1] - mat[1, 2] = (2*q2*q3+2*q0*q1) - (2*q2*q3-2*q0*q1) = 4*q0*q1
				// mat[0, 2] - mat[2, 0] = (2*q1*q3+2*q0*q2) - (2*q1*q3-2*q0*q2) = 4*q0*q2
				// mat[1, 0] - mat[0, 1] = (2*q1*q2+2*q0*q3) - (2*q1*q2-2*q0*q3) = 4*q0*q3
				// よって
				Vector3 v = new Vector3((mat[2, 1] - mat[1, 2]) / 4.0 / w, (mat[0, 2] - mat[2, 0]) / 4.0 / w, (mat[1, 0] - mat[0, 1]) / 4.0 / w);

				// 以下は計算のチェックと直行行列の精度のチェック
				// 対角要素の加減演算に１を加えると同様に求められるが、二乗されているため絶対値しかわからない。計算のチェックに使用する
				// +(1-2*q2*q2-2*q3*q3) - (1-2*q1*q1-2*q3*q3) - (1-2*q1*q1-2*q2*q2) + 1 = 4*q1*q1
				// -(1-2*q2*q2-2*q3*q3) + (1-2*q1*q1-2*q3*q3) - (1-2*q1*q1-2*q2*q2) + 1 = 4*q2*q2
				// -(1-2*q2*q2-2*q3*q3) - (1-2*q1*q1-2*q3*q3) + (1-2*q1*q1-2*q2*q2) + 1 = 4*q3*q3
				double a;
				if (Math.Abs((a = Math.Sqrt(+mat[0, 0] - mat[1, 1] - mat[2, 2] + 1.0) / 2.0) - Math.Abs(v.X)) > 0.000001) throw new Exception(String.Format("エラー　{0:f8} {1:f8}", a, v.X));
				if (Math.Abs((a = Math.Sqrt(-mat[0, 0] + mat[1, 1] - mat[2, 2] + 1.0) / 2.0) - Math.Abs(v.Y)) > 0.000001) throw new Exception(String.Format("エラー　{0:f8} {1:f8}", a, v.Y));
				if (Math.Abs((a = Math.Sqrt(-mat[0, 0] - mat[1, 1] + mat[2, 2] + 1.0) / 2.0) - Math.Abs(v.Z)) > 0.000001) throw new Exception(String.Format("エラー　{0:f8} {1:f8}", a, v.Z));

				this = new Unit(2.0 * Math.Acos(w), v);
			}

			/// <summary>このインスタンスの共役回転四元数を出力します</summary>
			public Unit Conjugation() { return new Unit(ag, -vc); }

			/// <summary>四元数へ変換します</summary>
			public Quaternion ToQuaternion() { return new Quaternion(this.W, this.V.X, this.V.Y, this.V.Z); }

			/// <summary>列ベクトル用の回転行列を出力します</summary>
			public Matrix ToMatrix() {
				// 行列への変換（列ベクトル）
				// |1-2*q2*q2-2*q3*q3    2*q1*q2-2*q0*q3    2*q1*q3+2*q0*q2|
				// |  2*q1*q2+2*q0*q3  1-2*q1*q1-2*q3*q3    2*q2*q3-2*q0*q1|
				// |  2*q1*q3-2*q0*q2    2*q2*q3+2*q0*q1  1-2*q1*q1-2*q2*q2|
				double www = this.W; Vector3 v = this.V;
				return new Matrix(new double[,] {
					{ 1 - 2 * (v.Y * v.Y + v.Z * v.Z),      2 * (v.X * v.Y - www * v.Z),      2 * (v.X * v.Z + www * v.Y) },
					{     2 * (v.X * v.Y + www * v.Z),  1 - 2 * (v.X * v.X + v.Z * v.Z),      2 * (v.Y * v.Z - www * v.X) },
					{     2 * (v.X * v.Z - www * v.Y),      2 * (v.Y * v.Z + www * v.X),  1 - 2 * (v.X * v.X + v.Y * v.Y) }
				});
			}

			/// <summary>
			/// このインスタンスが位置ベクトルの回転を表す場合は位置ベクトルを回転します。
			/// このインスタンスが座標系の回転を表す場合は共役で位置ベクトルを回転します。
			/// </summary>
			/// <param name="vec">回転する位置ベクトル</param>
			/// <returns>回転後の位置ベクトル</returns>
			public Vector3 Rotation(Vector3 vec) {
				// 行列による解法
				//Vector3 ans0 = ToMatrix() * vec;

				// 四元数による解法
				//Quaternion qvec = new Quaternion(0.0, vec.X, vec.Y, vec.Z);
				//Quaternion ans1 = ToQuaternion() * qvec * ToQuaternion().Conjugation();
				//if (Math.Abs(ans0.X - ans1.X) > 0.000001) throw new Exception("qerfbqefrqhreb");
				//if (Math.Abs(ans0.Y - ans1.Y) > 0.000001) throw new Exception("qerfbqefrqhreb");
				//if (Math.Abs(ans0.Z - ans1.Z) > 0.000001) throw new Exception("qerfbqefrqhreb");

				// 四元数の成分W,Vによる展開
				//double w0 = this.W * 0.0 - Vector3.vscal( this.V, vec);
				//Vector3 v0 = this.W * vec + 0.0 * this.V + Vector3.vvect( this.V, vec);
				//double w1 = w0 * this.W - Vector3.vscal(v0, (-this.V));
				//Vector3 v1 = w0 * (-this.V) + this.W * v0 + Vector3.vvect(v0, (-this.V));
				//Vector3 ans2 =
				//	2 * Vector3.vscal(this.V, vec) * this.V
				//	+ (this.W * this.W - Vector3.vscal(this.V, this.V)) * vec
				//	+ 2 * this.W * Vector3.vvect(this.V, vec);
				//if (Math.Abs(ans0.X - ans2.X) > 0.000001) throw new Exception("qerfbqefrqhreb");
				//if (Math.Abs(ans0.Y - ans2.Y) > 0.000001) throw new Exception("qerfbqefrqhreb");
				//if (Math.Abs(ans0.Z - ans2.Z) > 0.000001) throw new Exception("qerfbqefrqhreb");

				// 図形からの算出（「四元数の成分W,Vによる展開」からも算出可能）
				//Vector3 ans3 =
				//	Vector3.vscal(vc, vec) * vc								// 回転中心点
				//	+ Math.Cos(ag) * (vec - Vector3.vscal(vc, vec) * vc)	// 回転Ｘ軸
				//	+ Math.Sin(ag) * Vector3.vvect(vc, vec);				// 回転Ｙ軸
				// 整理すると
				Vector3 ans3 =
						(1 - Math.Cos(ag)) * Vector3.Vscal(vc, vec) * vc
						+ Math.Cos(ag) * vec
						+ Math.Sin(ag) * Vector3.Vvect(vc, vec);
				return ans3;
			}

			/// <summary>
			/// 回転を表す単位四元数を回転後の単位ベクトルの位置誤差で比較します
			/// </summary>
			/// <param name="unt1">比較する単位四元数</param>
			/// <returns>回転後の単位ベクトル間の位置誤差(mm)</returns>
			public double CheckGosa(Unit unt1) {
				Vector3 vv;		// 回転軸ベクトルと垂直のベクトル
				if (Math.Abs(vc.X) <= Math.Abs(vc.Y) && Math.Abs(vc.X) <= Math.Abs(vc.Z))
					vv = Vector3.Vvect(vc, Vector3.vi);
				else if (Math.Abs(vc.Y) <= Math.Abs(vc.Z) && Math.Abs(vc.Y) <= Math.Abs(vc.X))
					vv = Vector3.Vvect(vc, Vector3.vj);
				else
					vv = Vector3.Vvect(vc, Vector3.vk);
				vv = vv / vv.Abs;
				return (unt1.Rotation(vv) - this.Rotation(vv)).Abs;
			}

			/// <summary>このインスタンスと指定した Quaternion.Unit オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
			/// <remarks>半径1000mmで回転後の位置誤差が0.00001mm未満であれば同じ値とします</remarks>
			/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
			/// <returns>obj が Quaternion.Unit のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
			public override bool Equals(object obj) { return Equals((Quaternion.Unit)obj); }
			private bool Equals(Quaternion.Unit obj) {
				double error = CheckGosa(obj);
				if (1000.0 * error > 0.00001) return false;
				if (1000.0 * error > 0.000001)
					LogOut.CheckCount("Quaternion 411", false, $"同一とするが半径1000mmで回転誤差が0.000001mm以上あります。 {(1000.0 * error):e9}");
				return true;
			}

			/// <summary>このインスタンスの数値を、それと等価な文字列形式に変換します。</summary>
			/// <returns>このインスタンスの値の文字列形式。</returns>
			public override string ToString() {
				return String.Format("ag={0,15:e9} vx={1,15:e9} vy={2,15:e9} vz={3,15:e9}", ag, vc.X, vc.Y, vc.Z);
			}

			/// <summary>このインスタンスのハッシュ コードを返します。</summary>
			/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
			public override int GetHashCode() { return this.ag.GetHashCode() ^ this.vc.GetHashCode(); }
		}
	}
}
