using System;
using System.Collections.Generic;
using System.Text;

namespace NcCode.nccode
{
	/// <summary>
	/// マクロ変数の値を保存します。
	/// </summary>
	internal readonly struct St_vsw
	{
		/// <summary>
		/// ＮＣデータ行の検索をする。最終文字＋１で'\0'を出力する
		/// </summary>
		/// <param name="ddat"></param>
		/// <param name="ipos"></param>
		/// <returns></returns>
		static public char ToC(string ddat, short ipos) {
			if (ipos >= 0 && ipos < ddat.Length)
				return ddat[ipos];
			else if (ipos == ddat.Length)
				return '\0';
			else
				throw new Exception("aefrvbaehrf");
		}




		/// <summary>現在の変数値</summary>
		private readonly NcMachine.Variable fsub;

		/// <summary>値の有無（true:定義済み false:未定義＝#0）</summary>
		public bool Sswbl {
			get {
				if (bdt == null) {
					if (m_rdt.HasValue) return true;
					else return false;
				}
				else throw new Exception("論理値になっているので値の有無は取得できません");
			}
		}
		/// <summary>数値</summary>
		public double Rdt {
			get {
				if (bdt == null) {
					if (m_rdt.HasValue)
						return m_rdt.Value;
					// ADD 2009/06/03
					//return m_rdt.Value;
					return 0.0;
				}
				else throw new Exception("論理値になっているので数値は取得できません");
			}
		}
		private readonly double? m_rdt;

		/// <summary>ピリオドの有無</summary>
		public readonly bool pir;
		/// <summary>マクロ変数由来？</summary>
		public readonly bool mcr;
		/// <summary>文字数？</summary>
		public readonly int nch;
		/// <summary>論理値</summary>
		public readonly bool? bdt;

		/// <summary>
		/// マクロの演算結果よりSt_vsw を作成する（nccalc）
		/// </summary>
		/// <param name="ddat">解析するＮＣデータ行</param>
		/// <param name="qpoi">文字の位置</param>
		/// <param name="juna">直前の演算子の優先順位（再帰呼出し時）</param>
		/// <param name="adrs">ＮＣデータ行の場合にセットするＮＣコード（A,B...）。ROUND()で使用する</param>
		/// <param name="p_fsub">マクロ変数</param>
		public St_vsw(string ddat, ref short qpoi, int juna, char adrs, NcMachine.Variable p_fsub) {
			int jun1 = 0, jun2 = 0;
			char ope1 = '\0', ope2 = '\0';
			short vpoi;
			St_vsw v2;

			this = new St_vsw(ddat, ref qpoi, adrs, p_fsub);    // １番目の数値(this.rdt)を取得
			while (ddat[qpoi] == ' ') qpoi++;

			while (qpoi >= 0 && ddat[qpoi] != ']' && ddat[qpoi] != ';' && ddat[qpoi] != '(') {
				while (ddat[qpoi] == ' ') qpoi++;

				qpoi = SubCalc(ddat, qpoi, ref ope1, ref jun1);	// １番目の後ろの演算子(ope1)と計算順位(jun1)を取得
				if (ope1 == ' ') break;				// １番目の後ろに演算子なし
				vpoi = qpoi;
				v2 = new St_vsw(ddat, ref vpoi, adrs, p_fsub);	// ２番目の数値(v2.rdt)を取得
				if (vpoi < 0) {
					qpoi = -1;						// ２番目の数値がない
					CamUtil.LogOut.CheckCount("OCode_vsw 736", false, "エラーとするべきではないか");
				}
				else {
					SubCalc(ddat, vpoi, ref ope2, ref jun2);	// ２番目の後ろの演算子(ope2)と計算順位(jun2)を取得
					if (jun1 > jun2)
						v2 = new St_vsw(ddat, ref qpoi, jun1, adrs, p_fsub);	// 後を先に計算後演算を実行
					else
						qpoi = vpoi;											// 先に演算を実行
					switch (ope1) {
					case '+': m_rdt = Rdt + v2.Rdt; break;
					case '-': m_rdt = Rdt - v2.Rdt; break;
					case '*': m_rdt = Rdt * v2.Rdt; break;
					case '/': m_rdt = Rdt / v2.Rdt; break;
					case '|':
						if (!bdt.HasValue) throw new Exception("OR演算子は使用できません");
						bdt = bdt.Value || v2.bdt.Value;
						break;
					case 'x':
						throw new Exception("XOR演算子は使用できません");
					case '&':
						if (!bdt.HasValue) throw new Exception("AND演算子は使用できません");
						bdt = bdt.Value && v2.bdt.Value;
						break;
					case 'm': m_rdt = (double)((int)Math.Round(Rdt) % (int)Math.Round(v2.Rdt)); break;
					case 'E': bdt = (Sswbl && v2.Sswbl) ? (Rdt == v2.Rdt ? true : false) : (Sswbl == v2.Sswbl ? true : false); break;
					case 'N': bdt = (Sswbl && v2.Sswbl) ? (Rdt != v2.Rdt ? true : false) : (Sswbl != v2.Sswbl ? true : false); break;
					case 'G': bdt = (Rdt > v2.Rdt ? true : false); break;
					case 'L': bdt = (Rdt < v2.Rdt ? true : false); break;
					case 'g': bdt = (Rdt >= v2.Rdt ? true : false); break;
					case 'l': bdt = (Rdt <= v2.Rdt ? true : false); break;
					default: throw new Exception("	qefdqewfdh");
					}
					// 追加 at 2011/08/10
					pir = true;
					mcr = mcr | v2.mcr;
				}
				// 前の演算を先に実行するため終了する
				if (juna != 0 && juna <= jun2) break;
			}
			return;
		}

		/// <summary>
		/// １つのＮＣコードよりSt_vsw を作成する（ncval）
		/// </summary>
		/// <param name="ddat">解析するＮＣデータ行</param>
		/// <param name="qpoi">文字の位置</param>
		/// <param name="adrs">ＮＣデータ行の場合にセットするＮＣコード（A,B...）。ROUND()で使用する</param>
		/// <param name="p_fsub">マクロ変数</param>
		public St_vsw(string ddat, ref short qpoi, char adrs, NcMachine.Variable p_fsub) {
			string mcod;
			int ii, ilen, fgou, jtmp;
			short dpoi;
			St_vsw v1;

			this.fsub = p_fsub;
			this.pir = false;
			this.mcr = false;
			this.nch = 0;
			this.bdt = null;
			fgou = 1;

			while (ToC(ddat, qpoi) == ' ')
				qpoi++;
			if (ToC(ddat, qpoi) == '-') {
				qpoi++;
				nch++;
				fgou = -1;
			}
			else if (ToC(ddat, qpoi) == '+') {
				qpoi++;
				nch++;
			}
			dpoi = qpoi;

			if (Char.IsDigit(ToC(ddat, qpoi)) == true || ToC(ddat, qpoi) == '.') {
				while (Char.IsDigit(ToC(ddat, qpoi)) == true) {
					qpoi++;
					nch++;
				}
				if (ToC(ddat, qpoi) == '.') {
					pir = true;
					qpoi++;
					nch++;
					while (Char.IsDigit(ToC(ddat, qpoi)) == true) {
						qpoi++;
						nch++;
					}
				}
				while (ToC(ddat, qpoi) == ' ') qpoi++;

				ilen = qpoi - dpoi;
				if (pir == false)
					m_rdt = (double)Convert.ToInt32(ddat.Substring(dpoi, ilen));
				else
					m_rdt = Convert.ToDouble(ddat.Substring(dpoi, ilen));
			}
			else if (ToC(ddat, qpoi) == '#') {
				qpoi++;
				v1 = new St_vsw(ddat, ref qpoi, '\0', p_fsub);
				if (qpoi >= 0 && v1.Sswbl) {
					jtmp = (int)Math.Floor(0.5 + v1.Rdt);
					if (jtmp >= 0) {
						if (fsub[jtmp].HasValue == true) {
							if (jtmp == 3011)
								m_rdt = 10000 * DateTime.Now.Year + 100 * DateTime.Now.Month + DateTime.Now.Day;
							else if (jtmp == 3012)
								m_rdt = 10000 * DateTime.Now.Hour + 100 * DateTime.Now.Minute + DateTime.Now.Second;
							else
								m_rdt = (double)fsub[jtmp];
						}
						else
							m_rdt = null;
					}
					else
						m_rdt = (double)NcMachine.ParaData(-jtmp, 0);
				}
				else {
					m_rdt = v1.m_rdt;
					bdt = v1.bdt;
				}
				pir = true;
				mcr = true;
				nch = v1.nch + (qpoi - dpoi);
			}
			else if (ToC(ddat, qpoi) == '[') {
				qpoi++;
				v1 = new St_vsw(ddat, ref qpoi, 0, adrs, p_fsub);
				while (ToC(ddat, qpoi) == ' ') qpoi++;
				if (ToC(ddat, qpoi) == ']')
					qpoi++;
				else
					_main.Error.Ncerr(2, "macro [] error\n");
				while (ToC(ddat, qpoi) == ' ') qpoi++;
				pir = v1.pir;
				mcr = true;
				m_rdt = v1.m_rdt;
				bdt = v1.bdt;
				nch = v1.nch + (qpoi - dpoi);
			}
			else if (Char.IsUpper(ToC(ddat, qpoi)) == true) {
				ii = 1;
				while (Char.IsUpper(ToC(ddat, (short)(qpoi + ii))) == true) ii++;
				mcod = ddat.Substring(qpoi, ii);
				qpoi += (short)ii;
				switch (mcod) {
				case "SIN":
				case "COS":
				case "TAN":
				case "SQRT":
				case "ABS":
				case "ROUND":
				case "FIX":
				case "FUP":
				case "ACOS":
				case "ASIN":
				case "LN":
				case "EXP":
					v1 = new St_vsw(ddat, ref qpoi, adrs, p_fsub);
					break;
				default:
					_main.Error.Ncerr(2, "fanction " + mcod + " error\n");
					qpoi = -1;
					throw new Exception("ewdbqwefdb");
				}

				if (qpoi < 0 || v1.Sswbl != true) {
					m_rdt = v1.m_rdt;
					bdt = v1.bdt;
				}
				else {
					switch (mcod) {
					case "SIN": m_rdt = Math.Sin(v1.Rdt * Math.PI / 180.0); break;
					case "COS": m_rdt = Math.Cos(v1.Rdt * Math.PI / 180.0); break;
					case "TAN": m_rdt = Math.Tan(v1.Rdt * Math.PI / 180.0); break;
					case "SQRT": m_rdt = Math.Sqrt(v1.Rdt); break;
					case "ABS": m_rdt = Math.Abs(v1.Rdt); break;
					case "FIX": m_rdt = (v1.Rdt >= 0) ? Math.Floor(v1.Rdt) : Math.Ceiling(v1.Rdt); break;
					case "FUP": m_rdt = (v1.Rdt >= 0) ? Math.Ceiling(v1.Rdt) : Math.Floor(v1.Rdt); break;
					case "ACOS": m_rdt = Math.Acos(v1.Rdt) * 180.0 / Math.PI; break;
					case "ASIN": m_rdt = Math.Asin(v1.Rdt) * 180.0 / Math.PI; break;
					case "LN": m_rdt = Math.Log(v1.Rdt); break;
					case "EXP": m_rdt = Math.Exp(v1.Rdt); break;
					case "ROUND":
						// ///////////////////////////////////////////////////////////////////
						// ユニックスと異なり切り捨てられる場合があるため 1e-12 調整する
						// ///////////////////////////////////////////////////////////////////
						double eps = Math.Sign(v1.Rdt) * 1e-12;
						// ///////////////////////////////////////////////////////////////////
						if (adrs == '\0')
							m_rdt = Math.Round((v1.Rdt + eps), MidpointRounding.AwayFromZero);
						else {
							m_rdt = Math.Round((v1.Rdt + eps) * Post.PostData[adrs].sdgt, MidpointRounding.AwayFromZero) / Post.PostData[adrs].sdgt;
						}

						break;
					default:
						throw new Exception("ewdbqwefdb");
					}
				}
				pir = true;
				mcr = true;
				nch = v1.nch + (qpoi - dpoi);
			}
			else {
				qpoi = -1;
				throw new Exception("ewdbqwefdb");
			}

			m_rdt *= fgou;
			return;
		}

		/// <summary>
		/// 演算子と計算順位を抽出する
		/// </summary>
		/// <param name="ddat">解析するＮＣデータ行</param>
		/// <param name="qpoi">文字の初期位置</param>
		/// <param name="qtmp">演算子</param>
		/// <param name="jun">計算順位（1:積など、2:和など、3:論理和など）</param>
		/// <returns>t次の文字位置</returns>
		private short SubCalc(string ddat, short qpoi, ref char qtmp, ref int jun) {
			string mcod;
			int ii;

			qtmp = ' ';

			ii = 1;
			if (Char.IsUpper(ddat[qpoi]) != false) {
				while (Char.IsUpper(ddat[qpoi + ii]) != false)
					ii++;
				mcod = ddat.Substring(qpoi, ii);
				//mcod[ii] = '\0';
				switch (mcod) {
				case "OR": qtmp = '|'; break;
				case "XOR": qtmp = 'x'; break;
				case "AND": qtmp = '&'; break;
				case "MOD": qtmp = 'm'; break;
				case "EQ": qtmp = 'E'; break;
				case "NE": qtmp = 'N'; break;
				case "GT": qtmp = 'G'; break;
				case "LT": qtmp = 'L'; break;
				case "GE": qtmp = 'g'; break;
				case "LE": qtmp = 'l'; break;
				default:
					_main.Error.Ncerr(2, "MACRO CALC ERROR " + ddat.Substring(qpoi));
					break;
				}
			}
			else if (ddat[qpoi] == '+' || ddat[qpoi] == '-' || ddat[qpoi] == '*' || ddat[qpoi] == '/')
				qtmp = ddat[qpoi];
			else if (ddat[qpoi] == ']' || ddat[qpoi] == ';' || ddat[qpoi] == '(') {
				;
			}
			else {
				_main.Error.Ncerr(2, "MACRO CALC ERROR " + ddat.Substring(qpoi));
			}

			if (qtmp != ' ')
				qpoi += (short)ii;
			switch (qtmp) {
			case '*':
			case '/':
			case '&':
			case 'm':
				jun = 1;
				break;
			case '+':
			case '-':
			case '|':
			case 'x':
				jun = 2;
				break;
			case 'E':
			case 'N':
			case 'G':
			case 'L':
			case 'g':
			case 'l':
				jun = 3;
				break;
			default:
				jun = 9999;
				break;
			}

			return qpoi;
		}

		/// <summary>このインスタンスと指定した st_vsw オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj が st_vsw のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		private bool Equals(St_vsw obj) {
			if (this.fsub != obj.fsub) return false;
			if (this.pir != obj.pir) return false;
			if (this.mcr != obj.mcr) return false;
			if (this.nch != obj.nch) return false;
			if (this.m_rdt != obj.m_rdt) return false;
			if (this.bdt != obj.bdt) return false;
			return true;
		}
	}
}
