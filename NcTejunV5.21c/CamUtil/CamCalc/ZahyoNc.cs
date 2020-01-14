using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil
{
	/// <summary>個々の軸 x, y, z, a, b, c を表します。</summary>
	public enum SdgtNo
	{
		/// <summary>座標Ｘ</summary>
		x = 0,
		/// <summary>座標Ｙ</summary>
		y = 1,
		/// <summary>座標Ｚ</summary>
		z = 2,
		/// <summary>座標Ａ</summary>
		a = 3,
		/// <summary>座標Ｂ</summary>
		b = 4,
		/// <summary>座標Ｃ</summary>
		c = 5,
		/// <summary>未定義</summary>
		Null = -1
	}

	/// <summary>
	/// 加工機の位置情報（ＸＹＺＡＢＣの座標値）を処理します。回転軸ＡＢＣは任意です。
	/// </summary>
	public readonly struct Ichi
	{
		/// <summary>同一点とみなす距離</summary>
		private const double gosa = 0.00005;

		/// <summary>０ベクトル(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)</summary>
		static public readonly Ichi p0 = new Ichi(Vector3.v0, Vector3.v0, 1000);

		// /////////////////////////////
		// 演算子のオーバーロード
		// /////////////////////////////

		/// <summary>位置座標値の＝＝演算子。誤差がIchi.Gosa以内</summary>
		public static bool operator ==(Ichi c1, Ichi c2) { return c1.Equals(c2); }
		/// <summary>位置座標値の！＝演算子。誤差がIchi.Gosa以上</summary>
		public static bool operator !=(Ichi c1, Ichi c2) { return !c1.Equals(c2); }
		/// <summary>位置座標値の＋演算子</summary>
		public static Ichi operator +(Ichi c1, Ichi c2) {
			return new Ichi(c1.rot_axis && c2.rot_axis, c1.xyz + c2.xyz, c1.abc + c2.abc, c1.Digit);
		}
		/// <summary>位置座標値の－演算子</summary>
		public static Ichi operator -(Ichi c1, Ichi c2) {
			return new Ichi(c1.rot_axis && c2.rot_axis, c1.xyz - c2.xyz, c1.abc - c2.abc, c1.Digit);
		}
		/// <summary>位置座標値と実数の＊演算子</summary>
		public static Ichi operator *(Ichi c1, double d2) {
			return new Ichi(c1.rot_axis, c1.xyz * d2, c1.abc * d2, c1.Digit);
		}
		/// <summary>位置座標値と実数の＊演算子</summary>
		public static Ichi operator *(double d2, Ichi c1) {
			return new Ichi(c1.rot_axis, c1.xyz * d2, c1.abc * d2, c1.Digit);
		}
		/// <summary>位置座標値と実数の／演算子</summary>
		public static Ichi operator /(Ichi c1, double d2) {
			return new Ichi(c1.rot_axis, c1.xyz / d2, c1.abc / d2, c1.Digit);
		}
		/// <summary>位置座標値とベクトルの＋演算子</summary>
		public static Ichi operator +(Ichi c1, Vector3 c2) {
			return new Ichi(c1.rot_axis, c1.xyz + c2, c1.abc, c1.Digit);
		}
		/// <summary>位置座標値とベクトルの－演算子</summary>
		public static Ichi operator -(Ichi c1, Vector3 c2) {
			return new Ichi(c1.rot_axis, c1.xyz - c2, c1.abc, c1.Digit);
		}

		// ////////////
		// 以上静的
		// ////////////

		
		
		
		
		
		
		
		
		
		/// <summary>回転軸設定の有無</summary>
		public readonly bool rot_axis;
		/// <summary>整数化する場合の倍率を出力する</summary>
		public int Digit { get { return (int)Math.Pow(10.0, (double)logd); } }
		/// <summary>整数化する場合の倍率のログをとった値</summary>
		private readonly int logd;
		/// <summary>個々の軸ＸＹＺ</summary>
		private readonly Vector3 xyz;
		/// <summary>個々の軸ＡＢＣ</summary>
		private readonly Vector3 abc;

		/// <summary>X座標値</summary>
		public double X { get { return xyz.X; } }
		/// <summary>Y座標値</summary>
		public double Y { get { return xyz.Y; } }
		/// <summary>Z座標値</summary>
		public double Z { get { return xyz.Z; } }
		/// <summary>A座標値</summary>
		public double A { get { return abc.X; } }
		/// <summary>B座標値</summary>
		public double B { get { return abc.Y; } }
		/// <summary>C座標値</summary>
		public double C { get { return abc.Z; } }

		/// <summary>X座標整数値</summary>
		public int Xi { get { return (int)Math.Round(xyz.X * Digit); } }
		/// <summary>Y座標整数値</summary>
		public int Yi { get { return (int)Math.Round(xyz.Y * Digit); } }
		/// <summary>Z座標整数値</summary>
		public int Zi { get { return (int)Math.Round(xyz.Z * Digit); } }
		/// <summary>A座標整数値</summary>
		public int Ai { get { return (int)Math.Round(abc.X * Digit); } }
		/// <summary>B座標整数値</summary>
		public int Bi { get { return (int)Math.Round(abc.Y * Digit); } }
		/// <summary>C座標整数値</summary>
		public int Ci { get { return (int)Math.Round(abc.Z * Digit); } }

		/// <summary>
		/// 実数座標値をsdgtNoインデックスより出力します。（単位：mm, deg）
		/// </summary>
		public double this[SdgtNo code] {
			get {
				if (rot_axis) {
					switch (code) {
					case SdgtNo.x: return xyz.X;
					case SdgtNo.y: return xyz.Y;
					case SdgtNo.z: return xyz.Z;
					case SdgtNo.a: return abc.X;
					case SdgtNo.b: return abc.Y;
					case SdgtNo.c: return abc.Z;
					default: throw new ArgumentOutOfRangeException();
					}
				}
				else {
					switch (code) {
					case SdgtNo.x: return xyz.X;
					case SdgtNo.y: return xyz.Y;
					case SdgtNo.z: return xyz.Z;
					default: throw new ArgumentOutOfRangeException();
					}
				}
			}
		}

		/// <summary>
		/// 位置情報(XYZABC)の３次元ベクトル表現より作成します。回転軸無しの場合はABC座標値は使われません。
		/// </summary>
		/// <param name="rot">回転軸の有無</param>
		/// <param name="v1">XYZ 座標値</param>
		/// <param name="a1">ABC 座標値</param>
		/// <param name="digit">整数化倍率（一般には1000）</param>
		private Ichi(bool rot, Vector3 v1, Vector3 a1, int digit) {
			this.rot_axis = rot;
			this.xyz = v1;
			this.abc = rot ? a1 : Vector3.v0;
			this.logd = (int)Math.Log10(digit);
			if (this.logd < 0) throw new Exception("qefdbqehb");
			if (digit != (int)Math.Pow(10.0, (double)this.logd)) throw new Exception("qefdbqehb");
		}

		/// <summary>
		/// 位置情報(XYZ)の３次元ベクトル表現より回転軸無しで作成します
		/// </summary>
		/// <param name="v1">v1 座標値</param>
		/// <param name="digit">整数化倍率</param>
		public Ichi(Vector3 v1, int digit) : this(false, v1, Vector3.v0, digit) { }

		/// <summary>
		/// 位置情報(XYZABC)の３次元ベクトル表現より回転軸ありで作成します。
		/// </summary>
		/// <param name="v1">XYZ 座標値</param>
		/// <param name="a1">ABC 座標値</param>
		/// <param name="digit">整数化倍率</param>
		public Ichi(Vector3 v1, Vector3 a1, int digit) : this(true, v1, a1, digit) { }

		/// <summary>ＸＹＺ３軸ベクトルへ変換します</summary>
		public Vector3 ToXYZ() { return xyz; }
		/// <summary>ＡＢＣ３軸ベクトルへ変換します</summary>
		public Vector3 ToABC() { return abc; }

		/// <summary>
		/// 位置ＸＹＺの異方スケーリングを実行しその位置情報を出力します。スケーリング値がマイナスの場合は回転軸ＡＢＣの向きも計算します。
		/// </summary>
		/// <param name="centr">スケーリングの中心</param>
		/// <param name="scale">異方スケーリング値（-1.0 の場合はミラーとなります）</param>
		/// <returns></returns>
		public Ichi Scaling(Vector3 centr, Vector3 scale) {
			Vector3 rdrct = new Vector3(Math.Sign(scale.Y * scale.Z), Math.Sign(scale.Z * scale.X), Math.Sign(scale.X * scale.Y));
			return new Ichi(this.rot_axis, (this.xyz - centr).Scaling(scale) + centr, this.abc.Scaling(rdrct), this.Digit);
		}

		/// <summary>
		/// このインスタンスの位置を回転します
		/// </summary>
		/// <param name="g171819"></param>
		/// <param name="centr">回転中心</param>
		/// <param name="angle">回転角度(radian)</param>
		/// <returns>回転後の位置情報</returns>
		public Ichi Rotation(int g171819, Vector3 centr, Double angle) {
			return new Ichi(this.rot_axis, (this.xyz - centr).Rotation(g171819, angle) + centr, this.abc, this.Digit);
		}

		/// <summary>座標値を移動情報を元に更新します。ＮＣデータでの座標値更新等に使用できます</summary>
		/// <param name="pXyz">位置を更新するXYZの値</param>
		/// <param name="pAbc">位置を更新するABCの値</param>
		/// <returns>更新された座標値</returns>
		public Ichi Update(NcZahyo pXyz, NcZahyo pAbc) {
			return new Ichi(this.xyz + pXyz, this.abc + pAbc, this.Digit);
		}

		/// <summary>このインスタンスの数値を、それと等価な文字列形式に変換します。</summary>
		/// <returns>このインスタンスの値の文字列形式。</returns>
		public override string ToString() {
			return String.Format("X={0,10:f4} Y={1,10:f4} Z={2,10:f4} A={3,10:f4} B={4,10:f4} C={5,10:f4}", xyz.X, xyz.Y, xyz.Z, abc.X, abc.Y, abc.Z);
		}

		/// <summary>このインスタンスと指定した Ichi オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj が Ichi のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public override bool Equals(object obj) { return Equals((Ichi)obj); }
		private bool Equals(Ichi obj) {
			if (this.rot_axis != obj.rot_axis) return false;
			if (this.Digit != obj.Digit) return false;
			if ((this.xyz - obj.xyz).Abs >= gosa) return false;
			if ((this.abc - obj.abc).Abs >= gosa) return false;
			return true;
		}

		/// <summary>このインスタンスのハッシュ コードを返します。</summary>
		/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
		public override int GetHashCode() {
			return rot_axis.GetHashCode() ^ Digit.GetHashCode() ^ xyz.GetHashCode() ^ abc.GetHashCode();
		}
	}

	/// <summary>移動の量をパルス数（mmのUNIT倍）で表します</summary>
	public readonly struct Pals
	{
		/// <summary>１パルスあたりに進む距離（mm）の逆数</summary>
		const int unit = 10000;
		/// <summary>座標xyzabcのsdgtNoリスト</summary>
		private static readonly SdgtNo[] sdgtList = new SdgtNo[] { SdgtNo.x, SdgtNo.y, SdgtNo.z, SdgtNo.a, SdgtNo.b, SdgtNo.c };

		// /////////////////////////////
		// 演算子のオーバーロード
		// /////////////////////////////

		/// <summary>パルス数の＝＝演算子</summary>
		public static bool operator ==(Pals c1, Pals c2) { return c1.Equals(c2); }
		/// <summary>パルス数の！＝演算子</summary>
		public static bool operator !=(Pals c1, Pals c2) { return !c1.Equals(c2); }

		
		
		/// <summary>有効な値が設定されている場合は true、そうでない場合は false。</summary>
		private readonly bool set;
		/// <summary>パルス数</summary>
		private readonly int x, y, z, a, b, c;

		/// <summary>
		/// 値のないインスタンスを生成します
		/// </summary>
		/// <param name="dummy">ダミーです</param>
		public Pals(int dummy) {
			set = false;
			this.x = this.y = this.z = this.a = this.b = this.c = 0;
		}
		/// <summary>
		/// 位置情報で表された移動量より作成します
		/// </summary>
		/// <param name="ido">パルスの値</param>
		public Pals(Ichi ido) {
			this.set = true;
			this.x = (int)Math.Round(ido.X * unit);
			this.y = (int)Math.Round(ido.Y * unit);
			this.z = (int)Math.Round(ido.Z * unit);
			this.a = (int)Math.Round(ido.A * unit);
			this.b = (int)Math.Round(ido.B * unit);
			this.c = (int)Math.Round(ido.C * unit);
		}
		/// <summary>
		/// 整数配列で表されたパルス数より作成します
		/// </summary>
		/// <param name="xyzabc">パルスの値</param>
		private Pals(int[] xyzabc) {
			this.set = true;
			this.x = xyzabc[0];
			this.y = xyzabc[1];
			this.z = xyzabc[2];
			this.a = xyzabc[3];
			this.b = xyzabc[4];
			this.c = xyzabc[5];
		}


		/// <summary>
		/// sdgtNoをインデックスとしてそれぞれのパルス数を出力します
		/// </summary>
		/// <param name="no">インデックス</param>
		/// <returns>パルス数</returns>
		private int Data(SdgtNo no) {
			if (set == false) throw new Exception("Palsの値がセットされていません");
			switch (no) {
			case SdgtNo.x: return x;
			case SdgtNo.y: return y;
			case SdgtNo.z: return z;
			case SdgtNo.a: return a;
			case SdgtNo.b: return b;
			case SdgtNo.c: return c;
			default: throw new Exception("qwefdbqh");
			}
		}

		/// <summary>Ｘパルス数が０である場合は true</summary>
		public bool X0 { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return x == 0; } }
		/// <summary>Ｙパルス数が０である場合は true</summary>
		public bool Y0 { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return y == 0; } }
		/// <summary>Ｚパルス数が０である場合は true</summary>
		public bool Z0 { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return z == 0; } }
		/// <summary>Ａパルス数が０である場合は true</summary>
		public bool A0 { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return a == 0; } }
		/// <summary>Ｂパルス数が０である場合は true</summary>
		public bool B0 { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return b == 0; } }
		/// <summary>Ｃパルス数が０である場合は true</summary>
		public bool C0 { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return c == 0; } }
		/// <summary>ＸＹＺＡＢＣ軸すべてのパルス数が０である場合は true</summary>
		public bool Xyzabc0 { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return (x | y | z | a | b | c) == 0; } }

		/// <summary>Ｘ移動量</summary>
		public double X { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return (double)x / unit; } }
		/// <summary>Ｙ移動量</summary>
		public double Y { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return (double)y / unit; } }
		/// <summary>Ｚ移動量</summary>
		public double Z { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return (double)z / unit; } }
		/// <summary>Ａ移動量</summary>
		public double A { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return (double)a / unit; } }
		/// <summary>Ｂ移動量</summary>
		public double B { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return (double)b / unit; } }
		/// <summary>Ｃ移動量</summary>
		public double C { get { if (set == false) throw new InvalidOperationException("Palsの値がセットされていません"); return (double)c / unit; } }

		/// <summary>sdgtNoインデックスより移動量を出力します（0:X 1:Y 2:Z 3:A 4:B 5:C）</summary>
		public double this[SdgtNo index] {
			get {
				if (set == false) throw new InvalidOperationException("Palsの値がセットされていません");
				switch (index) {
				case SdgtNo.x: return X;
				case SdgtNo.y: return Y;
				case SdgtNo.z: return Z;
				case SdgtNo.a: return A;
				case SdgtNo.b: return B;
				case SdgtNo.c: return C;
				default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		/// <summary>ＸＹＺの移動量をベクトルとして出力します</summary>
		public Vector3 ToXYZ() { return new Vector3(X, Y, Z); }
		/// <summary>ＡＢＣの移動量をベクトルとして出力します</summary>
		public Vector3 ToABC() { return new Vector3(A, B, C); }
		/// <summary>ＸＹＺＡＢＣの移動量を位置情報として出力します</summary>
		/// <param name="digit">>位置情報の整数化倍率</param>
		/// <returns></returns>
		public Ichi ToIchi(int digit) { return new Ichi(new Vector3(X, Y, Z), new Vector3(A, B, C), digit); }

		/// <summary>ミラーを実行します</summary>
		/// <param name="mirr">ミラーを表す座標値</param>
		/// <returns>ミラーされたパルス数</returns>
		public Pals Scaling(NcZahyo mirr) {
			return new Pals(new Ichi(ToXYZ().Scaling(mirr.ToMirrXYZ), ToABC().Scaling(mirr.ToMirrABC), unit));
		}

		/// <summary>
		/// 各軸の送り速度リストを用いて最小移動時間の軸の移動時間を算出します
		/// </summary>
		/// <param name="rf">各軸X,Y,Z,A,B,Cの送り速度のリスト（mm/min）</param>
		/// <returns>最小移動時間の軸の移動時間（秒）</returns>
		public double Min_Time(int[] rf) {
			double tim0 = Double.MaxValue;
			foreach (SdgtNo jj in sdgtList)
				if (this.Data(jj) != 0)
					tim0 = Math.Min(tim0, Math.Abs(this[jj]) / rf[(int)jj]);
			return tim0 * 60.0;
		}

		/// <summary>
		/// 最も移動時間がかかる軸名を求めます
		/// </summary>
		/// <param name="rf">各軸X,Y,Z,A,B,Cの送り速度のリスト（mm/min）</param>
		/// <param name="max">rtが送り速度最大値の場合は true</param>
		/// <returns>軸名</returns>
		public SdgtNo MaxTimeAxis(int[] rf, bool max) {
			SdgtNo no;
			double mtim;		// 最大到達軸の移動時間(msec)
			double plsc;		// この軸の１msecあたりのパルス数(/msec)

			no = SdgtNo.Null;
			mtim = 0.0;
			foreach (SdgtNo ii in sdgtList) {
				if (this.Data(ii) == 0) continue;
				plsc = rf[(int)ii] / 60.0 / 1000 * unit;
				if (max) plsc = Math.Floor(plsc);
				if (mtim >= Math.Abs(this.Data(ii)) / plsc) continue;
				mtim = Math.Abs(this.Data(ii)) / plsc;
				no = ii;
			}
			if (no == SdgtNo.Null) throw new Exception("qj3efb1qhre");
			return no;
		}

		/// <summary>
		/// 早送りの非直線時の中間点を求めるための移動残量リストを出力します
		/// </summary>
		/// <param name="rf">各軸X,Y,Z,A,B,Cの早送り速度のリスト（mm/min）</param>
		/// <returns>移動残量リスト</returns>
		public List<Pals> HiChokusen(int[] rf) {
			// 出力する中間位置（移動残量）
			List<Pals> chu = new List<Pals>();

			Pals rpls;					// パルス残数
			int[] pcnt = new int[6];	// 1msecあたりの早送りパルス数
			int etim;					// 最初の軸が移動完了するまでの時間（msec）
			int[] rtmp;
			int data;

			rpls = this;
			for (int ii = 0; ii < 6; ii++) pcnt[ii] = (int)Math.Floor(rf[ii] / 60.0 / 1000 * unit);

			while (true) {
				// 最も早く終点に到達する軸の移動時間 etim を求める
				etim = Int32.MaxValue;
				foreach (SdgtNo ii in sdgtList) {
					if (rpls.Data(ii) == 0) continue;
					if (etim > (int)Math.Ceiling(Math.Abs((double)rpls.Data(ii)) / pcnt[(int)ii]))
						etim = (int)Math.Ceiling(Math.Abs((double)rpls.Data(ii)) / pcnt[(int)ii]);
				}
				// etim 後のパルス残数を求める
				rtmp = new int[] { 0, 0, 0, 0, 0, 0 };
				foreach (SdgtNo ii in sdgtList) {
					data = rpls.Data(ii);
					if (Math.Abs(data) <= etim * pcnt[(int)ii])
						data = 0;
					else {
						if (data > 0)
							data -= etim * pcnt[(int)ii];
						else
							data += etim * pcnt[(int)ii];
					}
					rtmp[(int)ii] = data;
				}
				rpls = new Pals(rtmp);
				// 最終位置に到達（すべてのパルスが０）したら終了
				if (rpls.Xyzabc0) break;

				// 新たな中間点の移動残量を追加
				chu.Add(rpls);
			}
			return chu;
		}

		/// <summary>このインスタンスの数値を、それと等価な文字列形式に変換します。</summary>
		/// <returns>このインスタンスの値の文字列形式。</returns>
		public override string ToString() {
			return String.Format("X={0,10:d} Y={1,10:d} Z={2,10:d} A={3,10:d} B={4,10:d} C={5,10:d}", x, y, z, a, b, c);
		}

		/// <summary>このインスタンスと指定した Pals オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public override bool Equals(object obj) { return Equals((Pals)obj); }
		private bool Equals(Pals obj) {
			if (this.set != obj.set) return false;
			if (this.x != obj.x) return false;
			if (this.y != obj.y) return false;
			if (this.z != obj.z) return false;
			if (this.a != obj.a) return false;
			if (this.b != obj.b) return false;
			if (this.c != obj.c) return false;
			return true;
		}
		/// <summary>このインスタンスのハッシュ コードを返します。</summary>
		/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
		public override int GetHashCode() {
			return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ a.GetHashCode() ^ b.GetHashCode() ^ c.GetHashCode();
		}
	}

	/// <summary>現在の位置情報を更新するためのnull許容ベクトルを表します。</summary>
	/// <remarks>ＮＣデータ１行の設定座標値、ミラーする基準位置とミラーの有無を表現します</remarks>
	public readonly struct NcZahyo
	{
		/// <summary>すべての座標値を更新しない場合の値</summary>
		static public readonly NcZahyo Null = new NcZahyo(null, null, null);

		/// <summary>値がある場合に座標値を更新する。ＮＣデータでの座標値更新等に使用できる</summary>
		/// <param name="xyz">元のVector3</param>
		/// <param name="add">更新ベクトル</param>
		/// <returns>更新された３次元ベクトル</returns>
		static private Vector3 Update(Vector3 xyz, NcZahyo add) {
			if (add == NcZahyo.Null || add.AbsInc)
				return new Vector3(add.x ?? xyz.X, add.y ?? xyz.Y, add.z ?? xyz.Z);
			else
				return xyz + new Vector3(add.x ?? 0.0, add.y ?? 0.0, add.z ?? 0.0);
		}

		// /////////////////////////////
		// 演算子のオーバーロード
		// /////////////////////////////
		/// <summary>位置座標値の＝＝演算子</summary>
		public static bool operator ==(NcZahyo c1, NcZahyo c2) { return c1.Equals(c2); }
		/// <summary>位置座標値の！＝演算子</summary>
		public static bool operator !=(NcZahyo c1, NcZahyo c2) { return !c1.Equals(c2); }

		/// <summary>位置座標値の－単項演算子</summary>
		public static NcZahyo operator -(NcZahyo c1) { return new NcZahyo(c1.g3.Value, -c1.x, -c1.y, -c1.z); }
		/// <summary>位置座標値の＋演算子</summary>
		public static Vector3 operator +(Vector3 v1, NcZahyo c1) { return Update(v1, c1); }
		/// <summary>位置座標値の－演算子</summary>
		public static Vector3 operator -(Vector3 v1, NcZahyo c1) { return Update(v1, -c1); }

		// /////////////////////////////
		// 以上、static
		// /////////////////////////////




		/// <summary>Ｇコードグループ３の数値。90 : アブソリュート、91 : インクリメンタル。ミラーを表現する場合、不明の場合はnull</summary>
		private readonly short? g3;
		/// <summary>未定義を含むＸＹＺの座標値</summary>
		private readonly double? x, y, z;

		/// <summary>アブソリュートの場合は true、インクリメンタルの場合は false</summary>
		public bool AbsInc { get { return g3.Value == 90; } }
		/// <summary>Ｘ座標</summary>
		public double? X { get { return x; } }
		/// <summary>Ｙ座標</summary>
		public double? Y { get { return y; } }
		/// <summary>Ｚ座標</summary>
		public double? Z { get { return z; } }

		/// <summary>移動情報の場合で、Ｇコードグループ３（G90, G91）の値とＸＹＺそれぞれの移動値から作成します</summary>
		public NcZahyo(short g3, double? x, double? y, double? z) {
			this.g3 = g3;
			this.x = x;
			this.y = y;
			this.z = z;
		}
		/// <summary>ミラー情報の場合で、ミラーする軸の座標値（ミラーしない軸はnull）から作成します</summary>
		public NcZahyo(double? x, double? y, double? z) {
			this.g3 = null;
			this.x = x;
			this.y = y;
			this.z = z;
		}

		/// <summary>未定義の場合は０として、３次元ベクトルに変換します。</summary>
		public Vector3 ToVector() { return new Vector3(x ?? 0.0, y ?? 0.0, z ?? 0.0); }

		/// <summary>ミラーのオン（負）オフ（正）を絶対値１で符号化します。座標値の異方スケーリングで用います。</summary>
		public Vector3 ToMirrXYZ { get { return new Vector3(x.HasValue ? -1.0 : 1.0, y.HasValue ? -1.0 : 1.0, z.HasValue ? -1.0 : 1.0); } }

		/// <summary>ミラーオンオフによる回転の向きを絶対値１で符号化します。</summary>
		public Vector3 ToMirrABC {
			get {
				return new Vector3(y.HasValue == z.HasValue ? 1.0 : -1.0, z.HasValue == x.HasValue ? 1.0 : -1.0, x.HasValue == y.HasValue ? 1.0 : -1.0);
			}
		}

		/// <summary>このインスタンスと指定した NcZahyo オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public override bool Equals(object obj) { return Equals((NcZahyo)obj); }
		private bool Equals(NcZahyo obj) {
			if (g3 != obj.g3) return false;
			if (x != obj.x) return false;
			if (y != obj.y) return false;
			if (z != obj.z) return false;
			return true;
		}

		/// <summary>このインスタンスのハッシュ コードを返します。</summary>
		/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
		public override int GetHashCode() {
			return g3.GetHashCode() ^ x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
		}
	}
}
