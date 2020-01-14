using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using NcCode.nccode;
using System.Text.RegularExpressions;
using CamUtil;

namespace NcdTool.NcSIM
{
	/// <summary>
	/// NCSPEEDでＮＣデータ検証するためのＣＬデータを作成する
	/// </summary>
	internal class _main_ncspd : NcCode._main, IDisposable
	{
		/// <summary>次元 2or3 add in 2015/03/20</summary>
		private readonly int camDimension;
		/// <summary>工具軸角度 add in 2015/03/20</summary>
		private readonly Angle3[][] axis_angl;
		private int koteiNo;
		/// <summary>標準送り速度 add in 2016/05/20</summary>
		private readonly double[] stdFeed;
		/// <summary>同時５軸加工 add in 2017/07/31</summary>
		private readonly bool[] simult5;

		private System.IO.StreamWriter sw;
		private readonly Regex semic;
		private Regex comme;
		private bool nextG100;
		private Label label;

		private bool simOut = false;
		private bool cutOut = false;	// 切削範囲 add in 2016/05/20
		/// <summary>サブプログラムの種類 1:固定サイクル 2:G65 3:G66 4:M98</summary>
		private short subOut = 0;

		/// <summary>メイン入力ＮＣデータの行番号</summary>
		private int nclnMain = 0;
		/// <summary>サブＮＣデータの出力行数</summary>
		private int nclnSubp = 0;

		/// <summary>固定サイクルのフォーマット</summary>
		private string fixFormat;
		/// <summary>カスタムマクロのフォーマット</summary>
		private string macFormat;

		//public int incdir = 2;
		/// <summary>
		/// ＮＣデータの検索パス
		/// </summary>
		public override string[] Ncdir {
			get { return m_ncdir; }
			protected set { m_ncdir = value; }
		}
		private string[] m_ncdir ={
			//"/usr1/ASDM/CAMCTL/NCSPC/",
			//"/usr1/ASDM/CAMCTL/NCMEM/",
			ServerPC.SvrFldrM,
			ServerPC.SvrName + @"\h\usr9\ASDM\CAMCTL\NCMEM_ncspeed\",
			ServerPC.SvrFldrE};

		/// <summary>
		/// NCSPEEDにて検証するためのＮＣコードを作成します。"GENERAL"のマクロコードを用います。
		/// </summary>
		/// <param name="program_name">ＮＣデータ名</param>
		/// <param name="label1"></param>
		/// <param name="dirname">保存するディレクトリ</param>
		/// <param name="camDimension">加工軸に依存する次元</param>
		/// <param name="angle">回転軸の回転角度</param>
		/// <param name="feedrate">標準の送り速度</param>
		/// <param name="simult">同時５軸の場合はtrue</param>
		/// <param name="nggt">移動、ミラー、反転の情報</param>
		public _main_ncspd(string program_name, Label label1, string dirname, int camDimension, Angle3[][] angle, double[] feedrate, bool[] simult, NcName.NcNam.St_nggt nggt)
			: base(program_name, NcCode._main.mcGENERAL, CamUtil.Machine.MachID.NULL) {

			this.camDimension = camDimension;

			this.axis_angl = angle;
			this.stdFeed = feedrate;
			this.simult5 = simult;

			// revをチェックするまでの暫定処置
			if (nggt.rev) {
				CamUtil.LogOut.CheckCount("_main_ncspd 084", false, "REVはまだ使用できません。藤本までご連絡ください。");
				throw new Exception("REVはまだ使用できません。藤本までご連絡ください。");
			}

			// メインＮＣデータ設定
			NcCode.nccode.Transp_Mirror trs_mir = null;
			if (nggt.trns != Vector3.v0 || nggt.Mirr != NcZahyo.Null || nggt.rev) {
				trs_mir = new NcCode.nccode.Transp_Mirror(nggt.trns, nggt.Mirr, nggt.rev);
			}

			MainNcdSet("PC_FILE", trs_mir, new CamUtil.Vector3(0.0, 0.0, 500.0), null);

			sw = new System.IO.StreamWriter(dirname + "\\" + ncName);
			m_swNcCheckName = "_main_ncspd";
			m_swNcCheck = new System.IO.StreamWriter(m_swNcCheckName, false, Encoding.Default);

			semic = new Regex("; *$|; *\\(");
			comme = new Regex("\\(.*\\)");
			nextG100 = false;
			this.label = label1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nout"></param>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		public override void Ncoutm(NcOuts nout, NcMod mod, OCode lcode)
		{
			string chout, chmae, chato;
			Match match;
			RotationAxis rotA;

			// 初期設定時の出力は無視する
			if (sw == null) return;

			if (lcode.subdep1.Depth == 0) {
				// メインプログラムの連続性のチェック
				if (nclnMain > 0 && nclnMain + 1 != lcode.ncln)
					throw new Exception($"前：{nclnMain.ToString()}　後：{lcode.ncln.ToString()}");
				nclnMain = lcode.ncln;

				// 表示
				if (nclnMain % 15 == 0) {
					label.Text = String.Format(" {0}を変換しています。 {1:000.0 %} ", this.ncName, maxLineNo.HasValue ? ((double)nclnMain / maxLineNo.Value) : 100);
					Application.DoEvents();
				}
				//else {
				//	label.Text = String.Format("{0}を変換しています。 {1:000.0 %}", this.ncName, maxLineNo.HasValue ? ((double)nclnMain / maxLineNo.Value) : 100);
				//	Application.DoEvents();
				//}

				// /////////////////////////////////
				// メインプログラムのテキスト作成
				// /////////////////////////////////
				if (lcode.Gg[0].Equals(65.0) && lcode.codeData.CodeData('G').Equals(100.0) == false)
					// 単純呼出しマクロの場合（G100を除く）
					chout = NclineCall(mod, lcode);
				else if (lcode.Gst12 && mod.GGroupValue[12].Equals(67.0) == false)
					// モーダル呼出しマクロの設定行の場合
					chout = NclineCall(mod, lcode);
				else if (mod.GGroupValue[12].Equals(66.1) == true)
					// 毎ブロック呼出しマクロのモーダル行の場合
					chout = NclineCall(mod, lcode);
				else
					chout = NclineMain(mod, lcode);

				// /////////////////////////////////
				// 前後に追加するテキスト作成
				// /////////////////////////////////
				chmae = chato = null;
				//if (chout + ";" != lcode.nctx)
				//	MessageBox.Show("ERROR in:" + lcode.nctx + "  out:" + chout + ";");
				if (nextG100) {
					koteiNo = 0;
					chout = "G00G90X0Y0Z100000";
					chato += "\n(SIM START)";
					rotA = new RotationAxis(axis_angl[mod.ToolCount - 1][0]);
					if (camDimension == 2)
						chato += "\n" + ClLink5Axis.Start_G + rotA.Euler_ZXZ().ToString(3) + "(" + axis_angl[mod.ToolCount - 1][0].ToString(3) + ")";
					else
						chato += "\n" + ClLink5Axis.Start_G +
							"B" + rotA.DMU_BC().DegB.ToString("0.0##") + "C" + rotA.DMU_BC().DegC.ToString("0.0##") +
							"(" + axis_angl[mod.ToolCount - 1][koteiNo].ToString(3) + ")";
					simOut = true;
				}

				// ////////////////////////////////////////////////
				// シミュレーション範囲内の切削送り速度を検出する add in 2016/05/20
				// ////////////////////////////////////////////////
				// ////////////////////////////////////////////////
				// シミュレーション範囲内のサブコールを検出する
				// ////////////////////////////////////////////////
				if (simOut) {

					// カスタムマクロの処理（単純呼出し）
					{
						if (subOut == 2) {
							chmae += "\n(SUB END)";
							subOut = 0;
						}
						if (lcode.Gg[0].Equals(65.0)) {
							match = comme.Match(chout);
							if (match.Success)
								chout = chout.Replace(chout.Substring(match.Index, match.Length), "");
							chout = "(" + chout + ")";
							//MessageBox.Show("カスタムマクロ：" + mod.gg[12].ToInt().ToString() + chout);
							chmae += "\n(SUB START)";
							subOut = 2;
						}
					}

					// 固定サイクルの処理
					if (lcode.Gg[9].Gst || mod.GGroupValue[9].Equals(80.0) == false) {
						match = comme.Match(chout);
						if (match.Success)
							chout = chout.Replace(chout.Substring(match.Index, match.Length), "");
						// サブから復帰後の処理
						if (nclnSubp > 0) {
							if (lcode.Gg[9].Gst && mod.GGroupValue[9].Equals(80.0))
								chato += "\n(SUB END)";
							else
								chmae += "\n(G80)\n(SUB END)";
							subOut = 0;
						}
						if (!mod.GGroupValue[9].Equals(80.0)) {
							//MessageBox.Show("固定サイクル：" + mod.gg[9].ToInt().ToString() + chout);
							chmae += "\n(SUB START)";
							subOut = 1;
							int ix = -1, iy = -1, iz = -1;
							if (lcode.Gg[9].Gst) {
								ix = chout.IndexOf('X');
								iy = chout.IndexOf('Y');
								iz = chout.IndexOf('Z');
								if (ix < 0 || iy < 0 || iz < 0)
									throw new Exception("wqerkgwrgtnwhj");
								fixFormat = chout;
								fixFormat = fixFormat.Replace(fixFormat.Substring(iy, iz - iy), "{1:s}");
								fixFormat = fixFormat.Replace(fixFormat.Substring(ix, iy - ix), "{0:s}");
								chato += "\nG00G90" + chout.Substring(ix, iz - ix);
							}
							else {
								ix = chout.IndexOf('X');
								iy = chout.IndexOf('Y');
								iz = chout.IndexOfAny("ABCDEFGHIJKLMNOPQRSTUVWZ".ToCharArray());
								if (ix < 0 || iy < 0 || iz >= 0)
									throw new Exception("未対応の固定サイクルフォーマットです");
								chato += "\nG00G90" + chout;
								chout = String.Format(fixFormat, chout.Substring(ix, iy - ix), chout.Substring(iy));
							}
						}
						if (chout.Length > 0) chout = "(" + chout + ")";
					}
					else if (subOut == 1) throw new Exception("qwfqerhfbwerbfh");

					// カスタムマクロの処理（モーダル呼出し）
					if (
					(lcode.Gst12 == false || lcode.Gg[12].Equals(65.0) == false) &&
					(lcode.Gst12 == true || mod.GGroupValue[12].Equals(67.0) == false)) {
						match = comme.Match(chout);
						if (match.Success)
							chout = chout.Replace(chout.Substring(match.Index, match.Length), "");
						// サブから復帰後の処理
						if (nclnSubp > 0) {
							if (lcode.Gg[12].Equals(67.0))
								chato += "\n(SUB END)";
							else
								chmae += "\n(G67)\n(SUB END)";
							subOut = 0;
						}
						if (lcode.Gst12 == false || lcode.Gg[12].Equals(67.0) == false) {
							if (lcode.Gst12) {
								//MessageBox.Show("カスタムマクロ：" + mod.gg[12].ToInt().ToString() + chout);
								chmae += "\n(SUB START)";
								subOut = 3;
								macFormat = chout;
							}
							else {
								if (nclnSubp != 0) {
									chmae += "\n(SUB START)";
									chmae += "\n(" + macFormat + ")";
									subOut = 3;
								}
								chato += "\nG00G90" + chout;
							}
						}
						chout = "(" + chout + ")";
					}
					else if (subOut == 3) throw new Exception("qwfqerhfbwerbfh");
					// サブプログラム　サブ内のコードは出力しない
					if (subOut == 4) subOut = 0;
					if (lcode.Mst[0] > 0 && lcode.Mst[1] == 98) {
						if (chout.IndexOf(ClLink5Axis.ChgAX) >= 0) {
							koteiNo++;
							Angle3 aaa = new Angle3(
								axis_angl[mod.ToolCount - 1][koteiNo].Jiku, lcode.nctx.Substring(8, lcode.nctx.IndexOf('(') - 8));
							if (!Angle3.MachineEquals(aaa, axis_angl[mod.ToolCount - 1][koteiNo], 3))
								throw new Exception($"工具軸エラー：{lcode.nctx}  {ClLink5Axis.ChgAX}{axis_angl[mod.ToolCount - 1][koteiNo].ToString()}");
							rotA = new RotationAxis(axis_angl[mod.ToolCount - 1][koteiNo]);
							if (camDimension == 2)
								chout = ClLink5Axis.ChgAX_G + rotA.Euler_ZXZ().ToString(3) + "(" + axis_angl[mod.ToolCount - 1][koteiNo].ToString(3) + ")";
							else
								chout = ClLink5Axis.ChgAX_G +
									"B" + rotA.DMU_BC().DegB.ToString("0.0##") + "C" + rotA.DMU_BC().DegC.ToString("0.0##") +
									"(" + axis_angl[mod.ToolCount - 1][koteiNo].ToString(3) + ")";
						}
						if (chout.IndexOf(ClLink5Axis.Kotei) == 0) {
							chout = ClLink5Axis.Kotei_G;
						}
						if (chout.IndexOf("M98P0006") >= 0) {
							chmae += "\n(SIM END)";
							simOut = false;
						}
						subOut = 4;
					}

					// ////////////////
					// 切削領域の設定
					// ////////////////
					if (cutOut) {
						if (chmae != null && chmae.IndexOf("(SIM END)") >= 0) {
							chmae = chmae.Insert(chmae.IndexOf("(SIM END)"), "(CUT END)\n");
							cutOut = false;
						}
						else if (subOut == 0) {
							if (lcode.Gg[1].Equals(0)) if (mod.Hokan != 0 || lcode.nctx.IndexOfAny(new char[] { 'X', 'Y', 'Z', 'A', 'B', 'C' }) < 0)
									throw new Exception("(CUT END) の出力設定エラー");
							if (mod.Hokan == 0 && lcode.nctx.IndexOfAny(new char[] { 'X', 'Y', 'Z', 'A', 'B', 'C' }) >= 0) {
								chmae += "\n(CUT END)";
								cutOut = false;
							}
							else if (lcode.codeData.CodeCount('F') > 0) {
								double ff = lcode.codeData.CodeData('F').ToDouble;
								if (Math.Abs(stdFeed[mod.ToolCount - 1] - ff) >= 1.0) {
									chmae += "\n(CUT END)";
									cutOut = false;
								}
							}
						}
					}
					else {
						if (chmae != null && chmae.IndexOf("(SUB START)") >= 0) {
							chmae = chmae.Insert(chmae.IndexOf("(SUB START)"), "(CUT START)\n");
							cutOut = true;
						}
						else if (subOut == 0) {
							if (!lcode.Gg[1].Equals(0) && lcode.codeData.CodeCount('F') > 0) {
								double ff = lcode.codeData.CodeData('F').ToDouble;
								if (Math.Abs(stdFeed[mod.ToolCount - 1] - ff) < 1.0) {
									chmae += "\n(CUT START)";
									cutOut = true;
								}
							}
						}
					}
					// ////////////////

				}
					

				// ///////////////
				// テキスト出力
				// ///////////////
				if (chmae != null) sw.WriteLine(chmae.Substring(1));
				if (chout != null) sw.WriteLine(chout);
				if (chato != null) sw.WriteLine(chato.Substring(1));
				nclnSubp = 0;

				// ///////////////
				// nextG100設定
				// ///////////////
				if (nextG100) {
					if (lcode.nctx.IndexOf("G00G90X0Y0") < 0)
						throw new Exception("wefnqerjfnwerjn");
					nextG100 = false;
				}
				else if (lcode.nctx.IndexOf("G100T") >= 0)
					nextG100 = true;
			}
			// ///////////////////////////////////////
			// サブプログラム出力（G100,M98P0006, M98... 以外）
			// ///////////////////////////////////////
			else if (simOut && subOut != 4) {
				switch (mod.Subk) {
				case 0:
					chout = NclineSub(mod, lcode);
					if (chout == "G80")
						chout = null;
					break;
				case 1:
					// 固定サイクルの処理
					chout = NclineSub(mod, lcode);
					int ix = -1, iy = -1, iz = -1;
					ix = chout.IndexOf('X');
					iy = chout.IndexOf('Y');
					iz = chout.IndexOf('Z');
					if (iz < 0)
						iz = chout.Length + 1;
					if (ix >= 0)
						chout = "G00G90" + chout.Substring(ix, iz - ix);
					else if (iy >= 0)
						chout = "G00G90" + chout.Substring(iy, iz - iy);
					else
						chout = null;
					break;
				case 2:
					// １ショットマクロの処理
					chout = null;
					break;
				case 3:
				default:
					throw new Exception("qwefdqwerfqervfgv");
				}

				if (chout != null)
					sw.WriteLine(chout);
				nclnSubp++;
				//label.Text = this.ncName + "を変換しています。" + nclnMain.ToString().PadLeft(6) + nclnSubp.ToString("-000");
				//Application.DoEvents();
			}

			return;
		}

		/// <summary>
		/// ＮＣデータのテキスト変換（メイン）
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		/// <returns></returns>
		private string NclineMain(NcMod mod, OCode lcode) {
			string chout = "";

			foreach (CodeD codeD in lcode.codeData) {
				switch (codeD.ncChar) {
				case ';':
					break;
				case '%':
				case '/':
					chout += codeD.ncChar;
					break;
				case '(':
				case '#':
					chout += codeD.String();
					break;
				case 'G':
				case 'M':
				case 'T':
					string aaa;
					aaa = codeD.ToStringAuto();
					if (aaa.Length == 1 || (aaa.Length == 3 && aaa.IndexOf('.') == 1)) aaa = "0" + aaa;
					chout += codeD.ncChar + aaa;
					break;
				case 'O':
				case 'P':
					chout += codeD.ncChar + codeD.ToStringAuto().PadLeft(4, '0');
					break;

				// /////////////////////////////////////////////////////////////////////////////////////
				// 同時５軸加工時、小数点ありの座標値は小数点ありとして出力するように変更 in 2017/07/31
				// /////////////////////////////////////////////////////////////////////////////////////
				case 'A':
				case 'B':
				case 'C':
				case 'I':
				case 'J':
				case 'K':
				case 'U':
				case 'V':
				case 'W':
				case 'X':
				case 'Y':
				case 'Z':
					// G100 あるいは同時５軸でない場合、小数点なし
					if ((lcode.Gg[0].Equals(65.0) && lcode.codeData.CodeData('G').Equals(100.0)) || simult5[mod.ToolCount - 1] == false)
						chout += codeD.ncChar + codeD.ToStringAuto();
					else
						chout += codeD.ncChar + codeD.ToString("0.0");
					break;
				// /////////////////////////////////////////////////////////////////////////////////////
				// /////////////////////////////////////////////////////////////////////////////////////

				case 'N':
				default:
					chout += codeD.ncChar + codeD.ToStringAuto();
					break;
				}
			}

			// ＮＣＳＰＥＥＤのＮＣデータ仕様（DIN?）に対応
			chout = NCSPEED("G0203", chout, mod, lcode);

			if (chout.Length == 0) return null;
			return chout;
		}
		/// <summary>
		/// ＮＣデータのテキスト変換（マクロコール文）
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		/// <returns></returns>
		private string NclineCall(NcMod mod, OCode lcode) {
			string chout = "";

			foreach (CodeD codeD in lcode.codeData) {
				switch (codeD.ncChar) {
				case ';':
				case '%':
				case '/':
				case '(':
				case '#':
					break;
				case 'G':
					string aaa;
					aaa = codeD.ToStringAuto();
					if (aaa.Length == 1 || (aaa.Length == 3 && aaa.IndexOf('.') == 1)) aaa = "0" + aaa;
					chout += codeD.ncChar + aaa;
					break;
				case 'L':
				case 'N':
				case 'O':
				case 'P':
					chout += codeD.ncChar + codeD.ToStringAuto().PadLeft(4, '0');
					break;
				default:
					chout += codeD.ncChar + codeD.ToString("0.0");
					break;
				}
			}
			if (chout.Length == 0) return null;
			return chout;
		}
		/// <summary>
		/// ＮＣデータのテキスト変換（サブプログラム）
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		/// <returns></returns>
		private string NclineSub(NcMod mod, OCode lcode) {
			string chout = "";

			// マクロコールはプリントしない
			if (lcode.Gg[0].Equals(65.0))
				return null;
			for (int ii = 1; ii <= lcode.Mst[0]; ii++)
				if (lcode.Mst[ii] == 98)
					return null;

			foreach (CodeD codeD in lcode.codeData) {
				switch (codeD.ncChar) {
				case ';':
				case '%':
				case '/':
				case '(':
				case '#':
				case 'O':
				case 'N':
					break;
				case 'G':
				case 'M':
				case 'T':
					string aaa;
					if (codeD.ncChar == 'M' && codeD.ToInt == 99) break;
					aaa = codeD.ToStringAuto();
					if (aaa.Length == 1 || (aaa.Length == 3 && aaa.IndexOf('.') == 1)) aaa = "0" + aaa;
					chout += codeD.ncChar + aaa;
					break;
				case 'P':
					chout += codeD.ncChar + codeD.ToStringAuto().PadLeft(4, '0');
					break;
				default:
					chout += codeD.ncChar + codeD.ToStringAuto();
					break;
				}
			}
			if (chout.Length == 0) return null;

			// ＮＣＳＰＥＥＤのＮＣデータ仕様（DIN?）に対応
			chout = NCSPEED("G0203", chout, mod, lcode);
			chout = NCSPEED("G91", chout, mod, lcode);

			return chout;
		}

		/// <summary>
		/// ＮＣＳＰＥＥＤのＮＣデータ仕様（DIN?）に対応
		/// 円弧補間：G02,G03は省略できない。I0,J0は省略できない。
		/// 増分値モード：G91は１ショットであり全行に出力要？？
		/// </summary>
		/// <param name="type"></param>
		/// <param name="chout"></param>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		/// <returns></returns>
		private string NCSPEED(string type, string chout, NcMod mod, OCode lcode) {
			string NCSPEED = chout;

			switch (type) {
			case "G91":
				//if (mod.gg[3].ToInt() == 91 && lcode.gg[3].gst == false)
				//	NCSPEED = "G91" + NCSPEED;
				break;
			case "G0203":
				if (mod.GGroupValue[1].ToInt() == 2 || mod.GGroupValue[1].ToInt() == 3) {
					if (lcode.Gg[1].Gst == false)
						NCSPEED = "G" + mod.GGroupValue[1].ToInt().ToString("00") + NCSPEED;
					if (lcode.codeData.CodeCount('R') == 0) {
						switch (mod.GGroupValue[2].ToInt()) {
						case 17:
							if (lcode.codeData.CodeCount('I') == 0 && lcode.codeData.CodeCount('J') > 0) {
								NCSPEED = CamUtil.LCode.NcLineCode.NcInsertChar(NCSPEED, 'J', "I0", true);
							}
							if (lcode.codeData.CodeCount('I') > 0 && lcode.codeData.CodeCount('J') == 0) {
								NCSPEED = CamUtil.LCode.NcLineCode.NcInsertChar(NCSPEED, 'I', "J0", false);
							}
							break;
						case 18:
							if (lcode.codeData.CodeCount('I') == 0 && lcode.codeData.CodeCount('K') > 0) {
								NCSPEED = CamUtil.LCode.NcLineCode.NcInsertChar(NCSPEED, 'K', "I0", true);
							}
							if (lcode.codeData.CodeCount('I') > 0 && lcode.codeData.CodeCount('K') == 0) {
								NCSPEED = CamUtil.LCode.NcLineCode.NcInsertChar(NCSPEED, 'I', "K0", false);
							}
							break;
						case 19:
							if (lcode.codeData.CodeCount('J') == 0 && lcode.codeData.CodeCount('K') > 0) {
								NCSPEED = CamUtil.LCode.NcLineCode.NcInsertChar(NCSPEED, 'K', "J0", true);
							}
							if (lcode.codeData.CodeCount('J') > 0 && lcode.codeData.CodeCount('K') == 0) {
								NCSPEED = CamUtil.LCode.NcLineCode.NcInsertChar(NCSPEED, 'J', "K0", false);
							}
							break;
						default:
							throw new Exception("awefqvegfrvg");
						}
					}
				}
				break;
			default:
				throw new Exception("jqbfqrhbfhqr");
			}
			return NCSPEED;
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Ncoute()
			//struct ncout out;
		{
			//NcOuts nout = ncReader.tout;
			sw.WriteLine("%");
			base.WriteEnd();
			sw.Close();
		}

		/// <summary>
		/// ＮＣデータ解析時におけるエラーの処理を実行
		/// </summary>
		/// <param name="errn">エラー番号</param>
		/// <param name="errc">コメント</param>
		public override void Ncerr(int errn, string errc)
			//int errn;
			//char errc[];
		{
			//void exit();
			switch (errn) {
			case 0: /* check data print */
				return;
			case 1: /* warning data print */
				//MessageBox.Show(errc, "ncerr",
				//	MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			case 2: /* error data print */
				//MessageBox.Show(errc, "ncerr",
				//	MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			default: /* fatal error */
				MessageBox.Show(errc + " code=" + errn, "ncerr",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
				Application.Exit();
				throw new Exception();
			//exit(errn);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Close()
		{
			sw.Close();
			m_swNcCheck.Close();
			m_swNcCheck = null;
		}

		public void Dispose() {
			if (sw != null) { sw.Dispose(); sw = null; }
			if (m_swNcCheck != null) { m_swNcCheck.Dispose(); m_swNcCheck = null; }
		}
	}
}

