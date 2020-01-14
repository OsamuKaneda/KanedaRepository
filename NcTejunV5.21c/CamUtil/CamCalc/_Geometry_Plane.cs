using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil
{
	/// <summary>
	/// 平面幾何。平面ベクトル演算の処理と、直線、円それぞれの交点などを処理します。
	/// </summary>
	/// <remarks>
	/// ３Ｄベクトルから平面ベクトルへの変換は３ＤベクトルCamUtil.Vector3のToVector2(int G171819)メソッドを用います
	/// </remarks>
	public class _Geometry_Plane
	{
		/// <summary>２次元ベクトルのスカラー積</summary>
		/// <param name="c1">始点ベクトル</param>
		/// <param name="c2">終点ベクトル</param>
		static public double Vscal(Vector2 c1, Vector2 c2) { return c1.X * c2.X + c1.Y * c2.Y; }
		/// <summary>２次元ベクトルのベクトル積</summary>
		/// <param name="c1">始点ベクトル</param>
		/// <param name="c2">終点ベクトル</param>
		static public double Vvect(Vector2 c1, Vector2 c2) { return c1.X * c2.Y - c1.Y * c2.X; }

		/// <summary>
		/// 円弧補間の回転角度（-2π&gt;angle&gt;2π）。始終点ベクトルの円弧長が0.00001mm未満の場合はエラーとします
		/// </summary>
		/// <remarks>
		/// ２つのベクトルがほぼ同一方向の場合は回転角度が０か±３６０かの判断が不可能なためエラーとし、よって2πあるいは-2πの円弧（全円）には対応しません。
		/// </remarks>
		/// <param name="h1">始点２次元ベクトル</param>
		/// <param name="h2">終点２次元ベクトル</param>
		/// <param name="g0203">回転方向（2:時計回り or 3:反時計回り）</param>
		/// <returns>回転角度(G0203==2:０以下, G0203==3:０以上)</returns>
		static public double Vangle(Vector2 h1, Vector2 h2, int g0203) {
			double ang = Math.Atan2(Vvect(h1, h2), Vscal(h1, h2));
			if (Math.Abs(ang) * h1.Abs < 0.00001)
				throw new Exception("同一方向（２点間の円弧長が0.00001mm未満）のベクトルの角度は０か２πかの判別ができないため対応しません。");
			switch (g0203) {
			case 2: if (ang > 0.0) ang -= 2 * Math.PI; break;
			case 3: if (ang < 0.0) ang += 2 * Math.PI; break;
			default: throw new Exception("回転方向G0203 は２あるいは３のみです");
			}
			return ang;
		}

		/// <summary>
		/// 交点（２直線）
		/// </summary>
		/// <param name="l1">直線１</param>
		/// <param name="l2">直線２</param>
		/// <returns>交点の位置ベクトル</returns>
		static public Vector2 Intersection(StraightLine l1, StraightLine l2) {
			return new Vector2(
				(l1.B * l2.C - l2.B * l1.C) / (l1.A * l2.B - l2.A * l1.B),
				(l1.C * l2.A - l2.C * l1.A) / (l1.A * l2.B - l2.A * l1.B));
		}

		/// <summary>
		/// 交点（直線と円）
		/// </summary>
		/// <remarks>交点があるかないかは判断しません。別途、直線と円との距離などにより判定してください。</remarks>
		/// <param name="ll">直線</param>
		/// <param name="cc">円</param>
		/// <param name="start">直線の方向の開始点側の交点を出力する場合はtrue、終点側はfalse</param>
		/// <returns>交点の位置ベクトル</returns>
		static public Vector2 Intersection(StraightLine ll, Circle cc, bool start) {
			double dist = ll.Distance(cc.Center);
			Vector2[] vv = new Vector2[2];
			vv[0] = new Vector2(
				cc.Center.X - ll.A * dist - ll.B * Math.Sqrt(cc.Radius * cc.Radius - dist * dist),
				cc.Center.Y - ll.B * dist + ll.A * Math.Sqrt(cc.Radius * cc.Radius - dist * dist));
			vv[1] = new Vector2(
				cc.Center.X - ll.A * dist + ll.B * Math.Sqrt(cc.Radius * cc.Radius - dist * dist),
				cc.Center.Y - ll.B * dist - ll.A * Math.Sqrt(cc.Radius * cc.Radius - dist * dist));
			return start ? vv[0] : vv[1];
		}

		/// <summary>
		/// 交点（円と円）
		/// </summary>
		/// <param name="c1">円１</param>
		/// <param name="c2">円２</param>
		/// <param name="left">c1の中心からc2の中心方向のベクトルの左側の場合 true、右側の場合 false。</param>
		/// <returns>交点の位置ベクトル</returns>
		static public Vector2 Intersection(Circle c1, Circle c2, bool left) {
			// ２つの円の交点は以下の連立方程式の解
			// (x - c1.X)2 + (y - c1.Y)2 = (c1.R)2
			// (x - c2.X)2 + (y - c2.Y)2 = (c2.R)2
			// 	上記２式の差を取ると２つの円の交点を通る直線の式になる
			// 2*(c2.X - c1.X)*x + 2*(c2.Y - c1.Y)*y + (c2.R*c2.R - c1.R*c1.R) - (c2.X*c2.X - c1.X*c1.X) - (c2.Y*c2.Y - c1.Y*c1.Y) = 0
			// (c2.X - c1.X)*x + (c2.Y - c1.Y)*y + ((c2.R*c2.R - c1.R*c1.R) - (c2.X*c2.X + c2.Y*c2.Y) + (c1.X*c1.X) + c1.Y*c1.Y))/2 = 0
			StraightLine L1 = new StraightLine(
				c2.Center.X - c1.Center.X,
				c2.Center.Y - c1.Center.Y,
				((c2.Radius * c2.Radius - c1.Radius * c1.Radius) - (c2.Center.Abs * c2.Center.Abs - c1.Center.Abs * c1.Center.Abs)) / 2.0);
			// チェック
			if (Vscal((c2.Center - c1.Center).Unit().Rotation(Math.PI / 2.0), L1.ToVector2) < 0.999) throw new Exception("efbqhfeb");

			// ２つの円の交点を通る直線と円の交点を求める
			Vector2 vec = Intersection(L1, c1, !left);
			if (left) if (Vvect(c2.Center - c1.Center, vec - c1.Center) < -1.0e-8) throw new Exception("efbqhfeb");
			if (!left) if (Vvect(c2.Center - c1.Center, vec - c1.Center) > +1.0e-8) throw new Exception("efbqhfeb");
			return vec;
		}



		/// <summary>
		/// ２次元ベクトル
		/// </summary>
		public readonly struct Vector2
		{
			/// <summary>０ベクトル(0.0, 0.0)</summary>
			static public readonly Vector2 v0 = new Vector2(0.0, 0.0);
			/// <summary>単位座標ベクトル(1.0, 0.0)</summary>
			static public readonly Vector2 vi = new Vector2(1.0, 0.0);
			/// <summary>単位座標ベクトル(0.0, 1.0)</summary>
			static public readonly Vector2 vj = new Vector2(0.0, 1.0);

			// /////////////////////////////
			// 以下は演算子のオーバーロード
			// /////////////////////////////

			/// <summary>２Ｄベクトルの==演算子</summary>
			public static bool operator ==(Vector2 c1, Vector2 c2) { return c1.Equals(c2); }
			/// <summary>２Ｄベクトルの!=演算子</summary>
			public static bool operator !=(Vector2 c1, Vector2 c2) { return !c1.Equals(c2); }
			/// <summary>２Ｄベクトルの－単項演算子</summary>
			public static Vector2 operator -(Vector2 c1) { return new Vector2(-c1.x, -c1.y); }
			/// <summary>２Ｄベクトルの＋演算子</summary>
			public static Vector2 operator +(Vector2 c1, Vector2 c2) { return new Vector2(c1.x + c2.x, c1.y + c2.y); }
			/// <summary>２Ｄベクトルの－演算子</summary>
			public static Vector2 operator -(Vector2 c1, Vector2 c2) { return new Vector2(c1.x - c2.x, c1.y - c2.y); }
			/// <summary>２Ｄベクトルと実数の＊演算子</summary>
			public static Vector2 operator *(double d2, Vector2 c1) { return new Vector2(c1.x * d2, c1.y * d2); }
			/// <summary>２Ｄベクトルと実数の＊演算子</summary>
			public static Vector2 operator *(Vector2 c1, double d2) { return new Vector2(c1.x * d2, c1.y * d2); }
			/// <summary>２Ｄベクトルと実数の／演算子</summary>
			public static Vector2 operator /(Vector2 c1, double d2) { return new Vector2(c1.x / d2, c1.y / d2); }

			// ////////////
			// 以上静的
			// ////////////






			private readonly double x, y;
			/// <summary>X座標値</summary>
			public double X { get { return x; } }
			/// <summary>Y座標値</summary>
			public double Y { get { return y; } }

			/// <summary>絶対値（ベクトルの長さ）</summary>
			public double Abs { get { return Vector3.Length(x, y); } }

			/// <summary>
			/// ＸＹの座標値から作成するコンストラクタ
			/// </summary>
			/// <param name="x">Ｘ座標値</param>
			/// <param name="y">Ｙ座標値</param>
			public Vector2(double x, double y) { this.x = x; this.y = y; }


			/// <summary>
			/// 指定の大きさのベクトルに変換します
			/// </summary>
			/// <param name="unit">ベクトルの大きさ</param>
			/// <returns>大きさを変換したベクトル</returns>
			public Vector2 Unit(double unit) { return Unit() * unit; }
			/// <summary>
			/// 単位ベクトルに変換します
			/// </summary>
			/// <returns>大きさ１に変換したベクトル</returns>
			public Vector2 Unit() {
				Vector2 vec = this / this.Abs;
				if (vec.x >= 1.0) { return vi; }
				if (vec.y >= 1.0) { return vj; }
				if (vec.x <= -1.0) { return -vi; }
				if (vec.y <= -1.0) { return -vj; }
				return vec;
			}

			/// <summary>３次元ベクトルへ変換します</summary>
			/// <param name="g171819">平面指定ＧコードNo.(17,18,19)</param>
			/// <param name="kijun">基準点</param>
			/// <returns>変換された３次元ベクトル</returns>
			public Vector3 ToVector3(int g171819, Vector3 kijun) {
				switch (g171819) {
				case 17: return new Vector3(x, y, kijun.Z);
				case 18: return new Vector3(y, kijun.Y, x);
				case 19: return new Vector3(kijun.X, x, y);
				default: throw new Exception("平面指定Ｇコードは17, 18, 19 のみです");
				}
			}

			/// <summary>
			/// ベクトルを回転します
			/// </summary>
			/// <param name="ang">回転角度（ラジアン）</param>
			/// <returns>回転後のベクトル</returns>
			public Vector2 Rotation(double ang) { return new Vector2(x * Math.Cos(ang) - y * Math.Sin(ang), x * Math.Sin(ang) + y * Math.Cos(ang)); }


			/// <summary>このインスタンスと指定した Vector2 オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
			/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
			/// <returns>obj が Vector2 のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
			public override bool Equals(object obj) { return Equals((Vector2)obj); }
			private bool Equals(Vector2 obj) { return this.x == obj.x && this.y == obj.y; }

			/// <summary>このインスタンスの数値を、それと等価な文字列形式に変換します。</summary>
			/// <returns>このインスタンスの値の文字列形式。</returns>
			public override string ToString() { return String.Format("X={0,10:f4} Y={1,10:f4}", x, y); }

			/// <summary>このインスタンスのハッシュ コードを返します。</summary>
			/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
			public override int GetHashCode() { return this.x.GetHashCode() ^ this.y.GetHashCode(); }

		}

		/// <summary>
		/// 平面の直線
		/// </summary>
		public readonly struct StraightLine
		{
			// X * cos(angle + Math.PI / 2.0) + Y * sin(angle + Math.PI / 2.0) - p = 0
			private readonly double angle;			// 直線の方向の角度（ -π<angle<=π ）
			private readonly double p;				// 原点からの距離（直線の方向の右側に原点がある場合が正）

			/// <summary>一般形 AX + BY + C = 0 のＡ（A*A + B*B = 1）</summary>
			public double A { get { return Math.Cos(angle + Math.PI / 2.0); } }
			/// <summary>一般形 AX + BY + C = 0 のＢ（A*A + B*B = 1）</summary>
			public double B { get { return Math.Sin(angle + Math.PI / 2.0); } }
			/// <summary>一般形 AX + BY + C = 0 のＣ（A*A + B*B = 1）</summary>
			public double C { get { return -p; } }

			/// <summary>直線の傾き</summary>
			public double Tangent { get { return Math.Tan(angle); } }
			/// <summary>直線方向の単位ベクトル</summary>
			public Vector2 ToVector2 { get { return new Vector2(Math.Cos(angle), Math.Sin(angle)); } }

			/// <summary>
			/// ２点を通る直線、あるいは２点の垂直二等分線を作成します
			/// ２点を通る直線の場合、直線の向きはベクトル v2 - v1 の向き、垂直二等分線の場合、直線の向きは点 v1 が左側となる向き。
			/// </summary>
			/// <param name="v1">基準（左側）</param>
			/// <param name="v2">方向（右側）</param>
			/// <param name="on">２点を通る場合 true、垂直二等分線の場合 false</param>
			public StraightLine(Vector2 v1, Vector2 v2, bool on) {
				if (on) {
					// (v2.X - v1.X)(y - v1.Y) = (v2.Y - v1.Y)(x - v1.X)
					angle = Math.Atan2((v2.Y - v1.Y), (v2.X - v1.X));
					p = 0.0;	// 仮設定
					p = v1.X * this.A + v1.Y * this.B;
				}
				else {
					angle = Math.Atan2((v2.X - v1.X), -(v2.Y - v1.Y));
					p = 0.0;	// 仮設定
					p = (v1.X + v2.X) / 2 * this.A + (v1.Y + v2.Y) / 2 * this.B;
				}
			}

			/// <summary>
			/// 一般形 aX + bY + c = 0 のa, b, c より作成します
			/// </summary>
			/// <param name="a">一般形 aX + bY + c = 0 のa</param>
			/// <param name="b">一般形 aX + bY + c = 0 のb</param>
			/// <param name="c">一般形 aX + bY + c = 0 のc</param>
			public StraightLine(double a, double b, double c) {
				angle = Math.Atan2(a, -b);
				p = 0.0;	// 仮設定
				p = (Math.Abs(a) - Math.Abs(b) >= 0) ? (-c * this.A / a) : (-c * this.B / b);
			}

			/// <summary>
			/// 点との距離を計算します。直線の向きの左側に点がある場合は正、右側が負となります
			/// </summary>
			/// <param name="p1">２次元の点の座標値</param>
			/// <returns>この直線と点との距離</returns>
			public double Distance(Vector2 p1) { return p1.X * this.A + p1.Y * this.B + this.C; }

			/// <summary>
			/// 指定された点から直線へ下した垂線の足の座標値を計算します
			/// </summary>
			/// <param name="p1">垂線の元となる点の座標値</param>
			/// <returns>垂線の足の座標値</returns>
			public Vector2 Perpendicular(Vector2 p1) { return new Vector2(p1.X - this.A * Distance(p1), p1.Y - this.B * Distance(p1)); }
		}

		/// <summary>
		/// 円
		/// </summary>
		public readonly struct Circle
		{
			// (x-c.X)2 + (y-c.Y)2 = r2
			private readonly Vector2 c;
			private readonly double r;
			/// <summary>円の中心の座標値</summary>
			public Vector2 Center { get { return c; } }
			/// <summary>円の半径</summary>
			public Double Radius { get { return r; } }

			/// <summary>
			/// 中心と半径から円を作成します
			/// </summary>
			/// <param name="center">中心点</param>
			/// <param name="radius">半径</param>
			public Circle(Vector2 center, double radius) {
				c = center;
				r = radius;
			}

			/// <summary>
			/// 円周上の２点と半径から円を作成します
			/// </summary>
			/// <param name="p1">円周上の点１</param>
			/// <param name="p2">円周上の点２</param>
			/// <param name="radius">半径</param>
			/// <param name="left">p2-p1ベクトルの左側に中心がある円を作成する場合はtrue、右側はfalse</param>
			public Circle(Vector2 p1, Vector2 p2, double radius, bool left) {
				c = Intersection(new Circle(p1, radius), new Circle(p2, radius), left);
				r = radius;
			}

			/// <summary>
			/// 円周上の３点から円を作成します
			/// </summary>
			/// <param name="p1">円周上の点１</param>
			/// <param name="p2">円周上の点２</param>
			/// <param name="p3">円周上の点３</param>
			public Circle(Vector2 p1, Vector2 p2, Vector2 p3) {
				StraightLine l1 = new StraightLine(p1, p2, false);
				StraightLine l2 = new StraightLine(p2, p3, false);
				c = Intersection(l1, l2);
				r = (p2 - c).Abs;
			}

			/// <summary>
			/// 円周上の点を通る接線を計算します。円周との距離は０．１以下にしてください
			/// </summary>
			/// <param name="p1">円周上の点</param>
			/// <returns>接線</returns>
			public StraightLine Tangent(Vector2 p1) {
				// 点と円の中心との距離を円の半径に合わせる
				if (Math.Abs(r - (p1 - c).Abs) > 0.1) LogOut.CheckCount("_Geometry_Plane 342", false, "円周との距離が0.1以上");
				p1 = c + (p1 - c).Unit(r);
				// 接線の公式による
				// (x - c.X)*(p1.X - c.X) + (y - c.Y)*(p1.Y - c.Y) = r*r
				return new StraightLine(p1.X - c.X, p1.Y - c.Y, -c.X * (p1.X - c.X) - c.Y * (p1.Y - c.Y) - r * r);
			}
			/// <summary>
			/// 円外の点を通る接線を計算します。直線方向はp1から円に向かう方向です
			/// </summary>
			/// <param name="p1">接線が通る円外の点</param>
			/// <param name="left">p1から円の中心に向かって左側の接線の場合 true、右側の場合は false。</param>
			/// <returns>接線</returns>
			public StraightLine Tangent(Vector2 p1, bool left) {
				Circle c1 = new Circle((p1 + c) / 2.0, (c - p1).Abs / 2.0);
				if (c1.r <= this.r / 2.0) throw new Exception("p1が円周上か円内にある。");
				return new StraightLine(p1, Intersection(c1, this, left), true);
			}
		}
	}
}
