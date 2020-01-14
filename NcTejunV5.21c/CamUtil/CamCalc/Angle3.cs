using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil
{
	/// <summary>
	/// ３個の軸角度で表す空間の回転を設定します。
	/// </summary>
	public readonly struct Angle3
	{
		/// <summary>
		/// 軸の回転指定方法
		/// </summary>
		public enum JIKU
		{
			/// <summary>指定なし（回転なし）</summary>
			Null,
			/// <summary>ＸＹＺ系オイラー角（TEBIS 標準角、G0出力時）</summary>
			Euler_XYZ,
			/// <summary>ＺＸＺ系オイラー角（ファナックの傾斜面加工指令の座標系指定角度）</summary>
			Euler_ZXZ,
			/// <summary>空間角ＡＢＣ（TEBIS iTNC540 の２次元加工時）</summary>
			Spatial,
			/// <summary>DMU200PのＢＣ軸回転角（TEBIS iTNC540 の３次元加工時）</summary>
			DMU_BC,
			/// <summary>MCC3016VGのＡＣ軸回転角</summary>
			MCCVG_AC,
			/// <summary>D500のＡＣ軸回転角</summary>
			D500_AC,
			/// <summary>任意軸回りの回転</summary>
			General
		}
		/// <summary>軸の回転指定方法の名称からJIKU を決定します</summary>
		static public JIKU JIKU_Type(string name) {
			switch (name) {
			case "Null": return Angle3.JIKU.Null;
			case "Euler_XYZ": return Angle3.JIKU.Euler_XYZ;
			case "Euler_ZXZ": return Angle3.JIKU.Euler_ZXZ;
			case "Spatial": return Angle3.JIKU.Spatial;
			case "DMU_BC": return Angle3.JIKU.DMU_BC;
			case "MCCVG_AC": return Angle3.JIKU.MCCVG_AC;
			case "D500_AC": return Angle3.JIKU.D500_AC;
			case "General": return Angle3.JIKU.General;
			default: throw new Exception("qrgfnqejrgnrnffadfa");
			}
		}
		/// <summary>軸の回転指定方法JIKU から軸の名称を決定します</summary>
		static public String JIKU_Name(JIKU type) {
			switch (type) {
			case Angle3.JIKU.Null: return "Null";
			case Angle3.JIKU.Euler_XYZ: return "Euler_XYZ";
			case Angle3.JIKU.Euler_ZXZ: return "Euler_ZXZ";
			case Angle3.JIKU.Spatial: return "Spatial";
			case Angle3.JIKU.DMU_BC: return "DMU_BC";
			case Angle3.JIKU.MCCVG_AC: return "MCCVG_AC";
			case Angle3.JIKU.D500_AC: return "D500_AC";
			case Angle3.JIKU.General: return "General";
			default: throw new Exception("efqbfaherfqefrq");
			}
		}

		/// <summary>ラジアンを度へ変換します（/Math.PI*180.0）</summary>
		public static double RtoD(double radian) { return radian / Math.PI * 180.0; }
		/// <summary>度をラジアンへ変換します（*Math.PI/180.0）</summary>
		public static double DtoR(double degree) { return degree * Math.PI / 180.0; }

		/// <summary>０角度(0.0, 0.0, 0.0)</summary>
		static public readonly Angle3 a0 = new Angle3(JIKU.Null, Vector3.v0);

		/// <summary>
		/// degree単位でＮＣデータの角度の同一性を確認
		/// </summary>
		/// <param name="a1">比較する角度１</param>
		/// <param name="a2">比較する角度２</param>
		/// <param name="decimals">小数点以下の有効桁数</param>
		/// <returns></returns>
		public static bool MachineEquals(Angle3 a1, Angle3 a2, int decimals) {
			if ((int)(Math.Round(a1.DegA, decimals) * decimals) != (int)(Math.Round(a2.DegA, decimals) * decimals)) return false;
			if ((int)(Math.Round(a1.DegB, decimals) * decimals) != (int)(Math.Round(a2.DegB, decimals) * decimals)) return false;
			if ((int)(Math.Round(a1.DegC, decimals) * decimals) != (int)(Math.Round(a2.DegC, decimals) * decimals)) return false;
			return true;
		}

		// ////////////
		// 以上静的
		// ////////////






		/// <summary>回転タイプ</summary>
		private readonly JIKU m_jiku;
		/// <summary>３個の軸回転角度（単位はラジアン）</summary>
		private readonly Vector3 m_abc;

		/// <summary>軸の回転タイプ</summary>
		public JIKU Jiku { get { return m_jiku; } }
		/// <summary>Ａ軸の回転角度（ラジアン）</summary>
		public double A { get { return m_abc.X; } }
		/// <summary>Ｂ軸の回転角度（ラジアン）</summary>
		public double B { get { return m_abc.Y; } }
		/// <summary>Ｃ軸の回転角度（ラジアン）</summary>
		public double C { get { return m_abc.Z; } }

		/// <summary>Ａ軸の回転角度（度）</summary>
		public double DegA { get { return RtoD(m_abc.X); } }
		/// <summary>Ｂ軸の回転角度（度）</summary>
		public double DegB { get { return RtoD(m_abc.Y); } }
		/// <summary>Ｃ軸の回転角度（度）</summary>
		public double DegC { get { return RtoD(m_abc.Z); } }

		/// <summary>絶対値（ラジアン）</summary>
		public double Abs { get { return m_abc.Abs; } }




		/// <summary>
		/// 回転タイプと軸回転角度（Vector3）から作成します
		/// </summary>
		/// <param name="p_jiku">回転タイプ</param>
		/// <param name="v1">軸回転角度（単位はラジアン）</param>
		public Angle3(JIKU p_jiku, Vector3 v1) {
			m_jiku = p_jiku;
			m_abc = v1;
			if (m_jiku == JIKU.Null && m_abc != a0.m_abc) throw new Exception("JIKU のエラー in Angle3");
		}
		/// <summary>角度Ａを設定します</summary>
		internal Angle3 SetA(double value) { return new Angle3(this.m_jiku, new Vector3(value, this.m_abc.Y, this.m_abc.Z)); }
		/// <summary>角度Ｂを設定します</summary>
		internal Angle3 SetB(double value) { return new Angle3(this.m_jiku, new Vector3(this.m_abc.X, value, this.m_abc.Z)); }
		/// <summary>角度Ｃを設定します</summary>
		internal Angle3 SetC(double value) { return new Angle3(this.m_jiku, new Vector3(this.m_abc.X, this.m_abc.Y, value)); }

		/// <summary>
		/// 回転タイプと軸回転角度（単位は度）のテキストデータから作成します
		/// </summary>
		/// <param name="p_jiku">回転タイプ</param>
		/// <param name="strABC">軸回転角度（単位は度）を表す文字列</param>
		public Angle3(JIKU p_jiku, string strABC) {
			int xx;
			double aa, bb, cc;
			aa = bb = cc = 0.0;

			m_jiku = p_jiku;
			if ((xx = strABC.IndexOf('C')) >= 0) {
				cc = Convert.ToDouble(strABC.Substring(xx + 1));
				strABC = strABC.Substring(0, xx);
			}
			if ((xx = strABC.IndexOf('B')) >= 0) {
				bb = Convert.ToDouble(strABC.Substring(xx + 1));
				strABC = strABC.Substring(0, xx);
			}
			if ((xx = strABC.IndexOf('A')) >= 0) {
				aa = Convert.ToDouble(strABC.Substring(xx + 1));
			}
			m_abc = new Vector3(DtoR(aa), DtoR(bb), DtoR(cc));
			if (m_jiku == JIKU.Null && m_abc != a0.m_abc) throw new Exception("JIKU のエラー in Angle3");
		}



		/// <summary>
		/// ３個の軸回転角度を表すベクトルを出力します
		/// </summary>
		/// <returns>軸回転角度（ラジアン）</returns>
		public Vector3 ToVector() { return m_abc; }

		/// <summary>このインスタンスの数値を、それと等価な文字列形式に変換します。</summary>
		/// <returns>このインスタンスの値の文字列形式。</returns>
		public override string ToString() {
			return "A" + this.DegA.ToString("0.###") + "B" + this.DegB.ToString("0.###") + "C" + this.DegC.ToString("0.###");
		}
		/// <summary>このインスタンスの数値を、それと等価な文字列形式に変換します。（小数点以下設定。0は省略）</summary>
		/// <returns>このインスタンスの値の文字列形式。</returns>
		public string ToString(int digt) {
			string form = "0.0".PadRight(digt + 2, '#');
			return "A" + this.DegA.ToString(form) + "B" + this.DegB.ToString(form) + "C" + this.DegC.ToString(form);
		}
	}
}
