using System;
using System.Collections.Generic;
using System.Text;

using CamUtil;

namespace NcCode.nccode
{
	/// <summary>
	/// ＮＣデータの１つのＮＣコードを表します。[不変]
	/// </summary>
	public class CodeD
	{
		/// <summary>ＮＣデータのマクロコード番号を決定するための文字列</summary>
		private const string macn = "_ABCIJKDEFGHLMNOPQRSTUVWXYZ";

		/// <summary>
		/// 保存形式の整数値を作成。コンストラクタと比較時に使用する
		/// </summary>
		/// <param name="axis">座標値か</param>
		/// <param name="pir">小数点の有無（false:なし、true:あり）</param>
		/// <param name="rdt">データの値</param>
		/// <param name="idgt">保存倍率</param>
		/// <returns></returns>
		static private int? SetD(bool axis, bool pir, double? rdt, int idgt)
		{
			if (idgt <= 0)
				throw new Exception("idgtの値が異常");
			int? dtj;

			if (rdt == null) return null;

			if (pir == false && axis) {
				if ((NcMachine.ParaData(1004, 0) & 0x04) != 0 && (NcMachine.ParaData(1004, 0) & 0x03) != 1) {
					dtj = (int)rdt.Value * 10;
					// チェック（一般には１倍である。特にncgenの場合は必ず）
					System.Windows.Forms.MessageBox.Show("コード１０倍モードである。バグである可能性大");
					throw new Exception();
				}
				dtj = (int)rdt.Value;
			}
			else
				dtj = (int)Math.Round(idgt * rdt.Value);
			return dtj;
		}

		/// <summary>指定した２つの CodeD オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj1">比較するオブジェクト１</param>
		/// <param name="obj2">比較するオブジェクト２</param>
		/// <returns>obj1がobj2の値に等しい場合は true。それ以外の場合は false。</returns>
		public static bool Equals(CodeD obj1, CodeD obj2) {
			if (obj1 == null) return obj2 == null;
			if (obj2 == null) return false;

			if (obj1.decim != obj2.decim) return false;
			if (obj1.intData != obj2.intData) return false;
			if (obj1.axis != obj2.axis) return false;
			if (obj1.iDigit != obj2.iDigit) return false;

			// ３変数を追加 in 2017/07/05
			if (obj1.ncChar != obj2.ncChar) return false;
			if (obj1.macVNo != obj2.macVNo) return false;
			if (obj1.macData != obj2.macData) return false;

			if (obj1.strData != obj2.strData) {
				if (Char.IsUpper(obj1.strData[0]) && Char.IsUpper(obj1.strData[1])) {
					System.Windows.Forms.MessageBox.Show($"Old:{obj1.strData}  New:{obj2.strData}");
					return true;
				}
				else return false;
			}
			return true;
		}

		// ///////////
		// 以上が静的
		// ///////////




		/// <summary>座標値であればtrue（整数値の処理方法の区分に使用）</summary>
		private readonly bool axis;
		/// <summary>整数化する倍率</summary>
		private readonly int iDigit;
		/// <summary>数値のＮＣデータそのままの文字列。dtjなどが変更されても変化しないので出力には使用できない</summary>
		private readonly string strData;

		/// <summary>ＮＣコード;ABCDEFGHIJKLMNOPQRSTUVWXYZ/(%# 。numと統一するために追加</summary>
		public readonly char ncChar;

		/// <summary>ＮＣコードの数値が#0でない場合。（数値が#0の場合はコード自体がないものとみなすため）</summary>
		public bool CodeOutput { get { return Char.IsLetter(ncChar) ? intData.HasValue : true; } }

		/// <summary>マクロ文で、変数代入文の場合trueで制御文の場合falseとなる</summary>
		public bool SetVariable { get { return macVNo.HasValue; } }

		/// <summary>ＮＣコードの整数化した値</summary>
		private readonly int? intData;

		/// <summary>小数点の有無（false:無し、true:あり）</summary>
		public readonly bool decim;


		/// <summary>マクロ変数の番号（add in 2010/09/30）</summary>
		public readonly int? macVNo;
		/// <summary>マクロ代入文の値（add in 2010/10/01）</summary>
		public readonly double? macData;

		/// <summary>ＮＣコードに対応するマクロコード番号</summary>
		public int MacroNo { get { return macn.IndexOf(ncChar); } }

		/// <summary>
		/// 代入マクロを作成するコンストラクタ
		/// </summary>
		/// <param name="p_sdat">ＮＣデータテキスト</param>
		/// <param name="macroNo">マクロ変数</param>
		/// <param name="data">代入する値</param>
		internal CodeD(string p_sdat, int macroNo, double? data) {
			this.axis = false;
			this.iDigit = 1;
			this.strData = p_sdat;

			this.ncChar = '#';
			this.decim = true;

			this.macVNo = macroNo;
			if (data == null) {
				this.macData = null;
				this.intData = null;
			}
			else {
				this.macData = data.Value;
				this.intData = (int)Math.Round(data.Value);
			}
		}
		/// <summary>
		/// 特殊コード（;/%(とマクロ制御指令）を処理するコンストラクタ
		/// </summary>
		/// <param name="cchar">ＮＣコード</param>
		/// <param name="p_sdat">ＮＣデータテキスト</param>
		/// <param name="data">データの値</param>
		internal CodeD(char cchar, string p_sdat, int? data) {
			this.ncChar = cchar;
			this.axis = false;
			this.iDigit = 1;
			this.strData = p_sdat;
			if (data.HasValue) {
				// GOTO, DO, END, IF, WHILE
				this.decim = false;
				this.intData = data;
			}
			else {
				// ;, /, %, (, THEN
				this.decim = true;
				this.intData = null;
			}
			this.macVNo = null;
			this.macData = null;
		}
		/// <summary>
		/// 一般ＮＣデータから作成するコンストラクタ
		/// </summary>
		/// <param name="v1">マクロ変数値</param>
		/// <param name="p_sdat">ＮＣデータテキスト</param>
		/// <param name="macroLine">マクロの設定行の場合は true</param>
		internal CodeD(St_vsw v1, string p_sdat, bool macroLine) {
			this.ncChar = p_sdat[0];
			this.axis = Post.PostData[ncChar].axis;
			if (axis == false && macroLine && v1.pir)
				this.iDigit = 1000;
			else {
				if (Post.PostData[ncChar].sdgt <= 0)
					throw new Exception("sdgtが設定されていません");
				this.iDigit = Post.PostData[ncChar].sdgt;
			}
			this.strData = p_sdat;
			this.decim = v1.pir;
			if (v1.Sswbl)
				intData = SetD(axis, v1.pir, v1.Rdt, iDigit);
			else
				intData = SetD(axis, v1.pir, null, iDigit);
			this.macVNo = null;
			this.macData = null;
		}
		/// <summary>
		/// ＮＣデータからではなく数値から作成するコンストラクタ
		/// </summary>
		/// <param name="pChar">ＮＣコード</param>
		/// <param name="pir">ピリオドの有無（false:無し、true:あり）</param>
		/// <param name="data">データの値</param>
		/// <param name="p_idgt">保存倍率</param>
		internal CodeD(char pChar, bool pir, double data, int p_idgt) {
			if (p_idgt <= 0)
				throw new Exception("sdgtが設定されていません");
			this.axis = Post.PostData[pChar].axis;
			this.iDigit = p_idgt;
			this.strData = null;

			this.ncChar = pChar;
			this.decim = pir;
			this.intData = SetD(axis, decim, data, p_idgt);
			this.macVNo = null;
			this.macData = null;
		}
		/// <summary>
		/// dtj,ptj以外をコピーするコンストラクタ
		/// </summary>
		/// <param name="src">元にするCodeD</param>
		/// <param name="data">データの値</param>
		private CodeD(CodeD src, int data) {
			this.axis = src.axis;
			this.iDigit = src.iDigit;
			this.strData = src.strData;
			//this.sdat = null;

			this.ncChar = src.ncChar;
			this.decim = src.decim;
			this.intData = data;
			this.macVNo = null;
			this.macData = null;
		}

		/// <summary>コピーを作成します</summary>
		/// <returns>コピー</returns>
		internal CodeD Clone() { return (CodeD)this.MemberwiseClone(); }

		// /////////////
		// 一般メソッド
		// /////////////

		/// <summary>整数化の可否を判断します</summary>
		public bool ToInt_OK {
			get {
				if (axis)
					return true;
				return intData.Value == (int)(Math.Ceiling((double)intData.Value / iDigit) * iDigit);
			}
		}
		/// <summary>整数化した数値を返します（非座標値の場合はidgtの商）</summary>
		public int ToInt {
			get {
				if (axis)
					return intData.Value;
				else if (ToInt_OK)
					return intData.Value / iDigit;
				else
					throw new InvalidOperationException("整数化はできません");
			}
		}
		/// <summary>実数化した数値を返します（idgtの商）</summary>
		public double ToDouble { get { return (double)intData.Value / iDigit; } }
		/// <summary>ブーリアン化数値</summary>
		public bool ToBoolean {
			get {
				if (intData.Value == -1) return true;
				if (intData.Value == 0) return false;
				throw new InvalidOperationException("ブーリアン化はできません");
			}
		}


		/// <summary>ＮＣデータのままの文字列を出力します（英文字のＮＣコード付）</summary>
		public string String()
		{
			if (strData == null)
				throw new Exception("ＮＣデータの文字列情報がない。");
			return strData;
		}
		/// <summary>マクロ制御文の文字列のみを出力します</summary>
		public string StringMacro() {
			if (strData == null)
				throw new Exception("ＮＣデータの文字列情報がない。");
			int ii = 0;
			while (true) {
				if (ii == strData.Length) break;
				if (StringCAM.ABC0.IndexOf(strData[ii]) < 0) break;
				ii++;
			}
			return strData.Substring(0, ii);
		}
		/// <summary>使用不可</summary>
		public new string ToString() {
			return ToStringAuto();
		}
		/// <summary>可能であれば整数で出力します</summary>
		public string ToStringAuto() {
			if (ToInt_OK)
				return ToInt.ToString();
			else
				return ToString("0.0");
		}
		/// <summary>
		/// ＮＣデータをフォーマット（実数）して出力します
		/// </summary>
		/// <param name="baseform">"0."あるいは"0.0"とする。必要な少数点以下の桁も補われる</param>
		/// <returns></returns>
		public string ToString(string baseform)
		{
			string sout, form;
			if (baseform != "0." && baseform != "0.0")
				throw new Exception("ＮＣデータのフォーマットエラー。小数点以下の桁数は自動です。");

			// check
			if (intData.Value != (intData.Value / iDigit) * iDigit) {
				if (intData.Value % iDigit != 0) { ;}
				else throw new Exception("jwbfeqefrbr");
			}
			else {
				if (intData.Value % iDigit != 0) throw new Exception("jwbfeqefrbr");
			}

			// 小数点以下に数値が必要な場合桁数を補う
			if (intData.Value != (intData.Value / iDigit) * iDigit) {
				int aaa = (int)Math.Round(Math.Log10((double)iDigit));
				if (aaa != 0) {
					form = "0.";
					form = form.PadRight(2 + aaa, '0');
					sout = ToDouble.ToString(form);
					while (sout[sout.Length - 1] == '0')
						sout = sout.Substring(0, sout.Length - 1);
				}
				else {
					sout = ToDouble.ToString(baseform);
					throw new Exception("ありえない！！");
					//System.Windows.Forms.MessageBox.Show("出力形式を変更します＝" + code[m_stj].ToString() + sout);
				}
			}
			else {
				sout = ToDouble.ToString(baseform);
				if (sout.IndexOf('.') < 0) throw new Exception("jwefvagvfrg");
			}
			return sout;
		}

		/// <summary>
		/// 手順で設定されたミラーを処理します
		/// </summary>
		/// <param name="p_mirr">ミラー情報</param>
		/// <param name="lcode">ＮＣデータ１行の情報</param>
		/// <param name="mCode">固定サイクルあるいはカスタムマクロの情報</param>
		/// <returns></returns>
		internal CodeD Mirror2(NcZahyo p_mirr, OCode lcode, CamUtil.CamNcD.MacroCode mCode) {
			CodeD newd = this.Clone();
			char IDO = 'N';

			if (mCode.xlist.IndexOf(ncChar) >= 0) {
				if (p_mirr.X.HasValue && (ncChar == 'I' || ncChar == 'X'))
					newd = new CodeD(this, -intData.Value);
				if (p_mirr.Y.HasValue && (ncChar == 'J' || ncChar == 'Y'))
					newd = new CodeD(this, -intData.Value);
				IDO = 'X';
				if (ncChar == 'I' || ncChar == 'X' || ncChar == 'J' || ncChar == 'Y') { ;}
				else throw new Exception($"ミラーのxlistエラー xlist={mCode.xlist}  {mCode.mess}");
			}

			// //////////////////////////////////////
			// カスタムマクロ（単純呼出し、毎ブロック呼出し）
			// //////////////////////////////////////
			if (lcode.Gg[0].Equals(65.0) || lcode.subdep1.gg12.Equals(66.1)) {
				// G100 の場合はミラーの処理は何もしない
				if (lcode.codeData.CodeData('G').ToDouble == 100.0) {
					if (IDO == 'X') throw new Exception("ミラーのエラー " + mCode.mess);
				}
				if (IDO == 'X') LogOut.CheckCount("CodeD 603", false, "単純呼出しカスタムマクロでミラーを実行しました。X " + mCode.mess);
				return newd;
			}

			// //////////////////////////////////////
			// カスタムマクロ（移動指令呼出し）
			// //////////////////////////////////////
			if (lcode.subdep1.gg12.Equals(66.0)) {
				if (lcode.Gst12) {
					if (IDO == 'X') throw new Exception("ミラーのエラー " + mCode.mess);
				}
				if (IDO == 'X') LogOut.CheckCount("CodeD 614", false, "移動指令呼出しカスタムマクロでミラーを実行しました。X " + mCode.mess);
				return newd;
			}
			// //////////////////////////////////////
			// 固定サイクル
			// //////////////////////////////////////
			if (lcode.subdep1.gg09.Equals(80.0) == false) {
				if (IDO == 'X') LogOut.CheckCount("CodeD 621", false, "固定サイクルでミラーを実行しました。X " + mCode.mess);
				return newd;
			}

			// //////////////////////////////////////
			// 一般部
			// //////////////////////////////////////
			{
				switch (ncChar) {
				case 'G':
					if (p_mirr.ToMirrABC.Z < 0 && ToInt_OK) {
						switch (ToInt) {
						case 2: newd = new CodeD(this, 3 * iDigit); break;
						case 3: newd = new CodeD(this, 2 * iDigit); break;
						case 41: newd = new CodeD(this, 42 * iDigit); break;
						case 42: newd = new CodeD(this, 41 * iDigit); break;
						}
					}
					if (IDO == 'X') throw new Exception("ミラーのエラー " + mCode.mess);
					break;
				case 'I':
				case 'J':
				case 'X':
				case 'Y':
					if (IDO != 'X') throw new Exception("ミラーのエラー " + mCode.mess);
					break;
				default:
					if (IDO == 'X') throw new Exception("ミラーのエラー " + mCode.mess);
					break;
				}
				return newd;
			}
		}
		/// <summary>
		/// 手順で設定された平行移動を処理します
		/// </summary>
		/// <param name="p_ido">移動ベクトル</param>
		/// <param name="lcode">ＮＣデータ１行の情報</param>
		/// <param name="mCode">固定サイクルあるいはカスタムマクロの情報</param>
		/// <returns></returns>
		internal CodeD Transp(Vector3 p_ido, OCode lcode, CamUtil.CamNcD.MacroCode mCode) {
			CodeD newd = this.Clone();
			char IDO = 'N';

			if (mCode.xlist.IndexOf(ncChar) >= 0) {
				switch (ncChar) {
				case 'X': newd = new CodeD(this, intData.Value + (int)Math.Round(iDigit * p_ido.X)); IDO = 'X'; break;
				case 'Y': newd = new CodeD(this, intData.Value + (int)Math.Round(iDigit * p_ido.Y)); IDO = 'X'; break;
				case 'I':
				case 'J':
					break;
				default:
					throw new Exception("移動のエラー");
				}
			}
			if (mCode.zlist.IndexOf(ncChar) >= 0) {
				newd = new CodeD(this, intData.Value + (int)Math.Round(iDigit * p_ido.Z));
				IDO = 'Z';
			}

			// //////////////////////////////////////
			// カスタムマクロ（単純呼出し、毎ブロック呼出し）
			// //////////////////////////////////////
			if (lcode.Gg[0].Equals(65.0) || lcode.subdep1.gg12.Equals(66.1)) {
				// G100 の場合は移動の処理は何もしない
				if (lcode.codeData.CodeData('G').ToDouble == 100.0) {
					if (IDO != 'N') throw new Exception("移動のエラー " + mCode.mess);
				}
				if (IDO == 'X') LogOut.CheckCount("CodeD 694", false, "カスタムマクロ（単純呼出し）で移動を実行しました。X " + mCode.mess);
				if (IDO == 'Z') LogOut.CheckCount("CodeD 695", false, "カスタムマクロ（単純呼出し）で移動を実行しました。Z " + mCode.mess);
				return newd;
			}

			// //////////////////////////////////////
			// カスタムマクロ（移動指令呼出し）
			// //////////////////////////////////////
			if (lcode.subdep1.gg12.Equals(66.0)) {
				if (lcode.Gst12) {
					if (IDO == 'X') throw new Exception("移動のエラー " + mCode.mess);
					if (IDO == 'Z') LogOut.CheckCount("CodeD 705", false, "カスタムマクロ（移動指令呼出し）で移動を実行しました。Z " + mCode.mess);
				}
				else {
					if (IDO == 'X') LogOut.CheckCount("CodeD 708", false, "カスタムマクロ（移動指令呼出し）で移動を実行しました。X " + mCode.mess);
					if (IDO == 'Z') throw new Exception("移動のエラー " + mCode.mess);
				}
				return newd;
			}

			// //////////////////////////////////////
			// 固定サイクル
			// //////////////////////////////////////
			if (lcode.subdep1.gg09.Equals(80.0) == false) {
				if (IDO == 'X') LogOut.CheckCount("CodeD 718", false, "固定サイクルで移動を実行しました。X " + mCode.mess);
				if (IDO == 'Z') LogOut.CheckCount("CodeD 719", false, "固定サイクルで移動を実行しました。Z " + mCode.mess);
				return newd;
			}

			// //////////////////////////////////////
			// 一般部
			// //////////////////////////////////////
			if (0 == 0) {
				switch (ncChar) {
				case 'X':
				case 'Y':
					if (IDO != 'X') throw new Exception("移動のエラー " + mCode.mess); break;
				case 'Z':
					if (IDO != 'Z') throw new Exception("移動のエラー " + mCode.mess); break;
				default:
					if (IDO != 'N') throw new Exception("移動のエラー " + mCode.mess); break;
				}
				return newd;
			}
		}
		/// <summary>
		/// 手順で設定された反転を処理します（現在未使用です）
		/// </summary>
		/// <param name="lcode">ＮＣデータ１行の情報</param>
		/// <returns></returns>
		internal CodeD Reverse(OCode lcode) {
			CodeD newd = this.Clone();

			// //////////////////////////////////////
			// カスタムマクロ（移動指令呼出し設定行）
			// //////////////////////////////////////
			if (lcode.subdep1.gg12.Equals(66.0) && lcode.Gst12) {
				string progNo = lcode.codeData.CodeData('P').ToInt.ToString("0000");
				if (CamUtil.CamNcD.MacroCode.MacroIndex(progNo) >= 0) {
					if (ToDouble == 0.0)
						newd = new CodeD(this, 1 * iDigit);
					else if (ToDouble == 1.0)
						newd = new CodeD(this, 0);
					else throw new Exception("反転処理エラー");
					LogOut.CheckCount("CodeD 758", false, "反転処理を実行しました P" + progNo);
				}
			}
			return newd;
		}

		/// <summary>このインスタンスと指定した CodeD オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public bool Equals(CodeD obj) { return Equals(this, obj); }

		/// <summary>整数値との比較</summary>
		public bool Equals(int ii) {
			int? aaa = SetD(axis, false, ii, this.iDigit);
			return intData.Value == aaa.Value;
		}
		/// <summary>実数値との比較</summary>
		public bool Equals(double dd) {
			int? aaa = SetD(axis, true, dd, this.iDigit);
			return intData.Value == aaa.Value;
		}
	}
}