using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using CamUtil;

namespace NcTejun.Output
{
	/// <summary>
	/// まず、FormTexasSetの「ＮＣデータ出力」によりTexasNcOutputが実行され、
	/// frmNcOutputを介してNcOutputExe以下がfrmNcOutput.ShowDialog() より使用される
	/// </summary>
	partial class NcOutput
	{
		/// <summary>ＮＣデータ出力メッセージの保存</summary>
		public TejunSet.FormTjnText frmOutText;
		/// <summary>ＮＣデータ出力フォルダー名</summary>
		protected string folder;

		/// <summary>
		/// 出力データを入れるフォルダーの作成
		/// </summary>
		/// <returns>成功の可否</returns>
		public bool MakePTRFolder() {
			folder = Dir_PTR + NcdTool.Tejun.Seba;
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			folder = folder + @"\" + lfldr;
			if (Directory.Exists(folder)) {
				DialogResult resultMess = MessageBox.Show(
					"以前出力したＮＣ出力データ（手順名：" + lfldr + "）は上書きするために消去します",
					"上書き確認",
					MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button2);
				if (resultMess == DialogResult.Cancel)
					return false;

				try { Directory.Delete(folder, true); }
				catch { ;}
			}
			if (Directory.Exists(folder)) {
				MessageBox.Show("フォルダーが使用中でした。もう一度実行してください。");
				return false;
			}
			Directory.CreateDirectory(folder);
			Directory.CreateDirectory(folder + @"\unix");
			return true;
		}

		/// <summary>
		/// ＮＣ出力の実行。FormNcOutputよりコールされる
		/// </summary>
		/// <param name="disp_message">実行時に表示するメッセージ Application.DoEvents(); で更新する</param>
		public void NcOutputExe(Label disp_message) {
			// インデックスファイルの書き出し
			StreamWriter sw_ind;
			// メインファイルの書き出し
			StreamWriter sw_pro;
			// 工具チェックファイルの書き出し
			StreamWriter sw_tck;
			// インデックスファイルなどのリセット
			sw_ind = sw_pro = sw_tck = null;

			// LineaMの初期設定ＮＣデータ
			string stnam = null;

			// //////////////////////////////////////////////////////////////////
			// ＮＣデータの変換(ncgen)
			// //////////////////////////////////////////////////////////////////
			//if (PcConvCheck || PcConvOnly)
			switch (NcdTool.Tejun.BaseNcForm.Id) {
			case BaseNcForm.ID.GENERAL:
				// NcOutput.NcConv()
				// NcTejun.ncgen._ncMain()
				// NcTejun.ncgen._main_a.ncread()
				// NcTejun.ncgen.NcRun()
				NcConv_General(disp_message);
				break;
			case BaseNcForm.ID.BUHIN:
				NcConv_Buhin(disp_message);
				break;
			default: throw new Exception("edbqehdb");
			}




			if (NcdTool.Tejun.Mach.ID == Machine.MachID.LineaM) {
				//===================================
				//スケジュールデータなどの作成と送信
				//===================================
				stnam = ScheduleFileOutput();
			}

			// ///////////////////////////
			// インデックスファイル作成
			// ///////////////////////////
			if (NcdTool.Tejun.Mach.Toool_nc) {
				// インデックスファイル作成の準備
				sw_ind = new StreamWriter("index", false, Encoding.Default);
				// インデックスファイルの先頭行作成(1)
				try { IndexFileOutput1(sw_ind); }
				catch {
					sw_ind.Close();
					Directory.Delete(folder, true);
					throw;
				}
			}
			// ///////////////////////////
			// メインファイル作成の準備
			// ///////////////////////////
			switch (NcdTool.Tejun.Mach.ID) {
			case Machine.MachID.DMU200P:
			case Machine.MachID.DMU210P:
			case Machine.MachID.DMU210P2:
				sw_pro = new StreamWriter("mainf", false, Encoding.Default);
				// メインファイルの先頭行作成(1)
				sw_pro.WriteLine("% MAIN G71");
				break;
			case Machine.MachID.V77:
			//case Machine.machID.SNC106:
				sw_pro = new StreamWriter("mainf", false, Encoding.Default);
				// メインファイルの先頭行作成(1)
				sw_pro.WriteLine("%");
				sw_pro.WriteLine("O0001");
				break;
			}

			// //////////////////////////////////////////
			// 工具チェック用ＮＣデータを作成（高速機）
			// //////////////////////////////////////////
			if (NcdTool.Tejun.Mach.Performance == "HI") {
				sw_tck = new StreamWriter("_toolchk", false, Encoding.Default);
				if (NcdTool.Tejun.Mach.Dmu)
					sw_tck.WriteLine("% MAIN G71");
				else
					sw_tck.WriteLine("%");
			}
			// 工具チェックファイルの工具番号リストを定義する
			List<int> sw_tck_tno = new List<int>();
			if (NcdTool.Tejun.Mach.ID == Machine.MachID.LineaM || NcdTool.Tejun.Mach.ID == Machine.MachID.D500) {
				sw_tck_tno.AddRange(NcdTool.Tejun.Mach.Mgrs.Select(tol => tol.Unum));
			}

			// //////////////////////////////////////////
			// 機械仕様への変換
			// //////////////////////////////////////////
			try {
				switch (NcdTool.Tejun.Mach.DncName) {
				case Machine.DNCName.TEXAS:
				case Machine.DNCName.DM10:
					NcOut_TEXAS(disp_message, NcdTool.Tejun.Mach, sw_tck_tno, sw_ind, sw_pro, sw_tck);
					Directory.Delete(folder + @"\unix", true);
					break;
				case Machine.DNCName.HP:
				case Machine.DNCName.AOI:
				case Machine.DNCName.CIMX:
				default:
					NcOut_NONTEXAS(disp_message, NcdTool.Tejun.Mach, sw_tck_tno, sw_pro, sw_tck);
					Directory.Delete(folder + @"\unix", true);
					// 空であればフォルダーを削除する
					//if (File.Exists(folder + "\\_TOOLCHK")) File.Delete(folder + "\\_TOOLCHK");
					if (Directory.GetFiles(folder).Length == 0) {
						try { Directory.Delete(folder); }
						catch { ;}
					}
					break;
				}
			}
			catch {
				if (sw_ind != null) sw_ind.Close();
				if (sw_pro != null) sw_pro.Close();
				if (sw_tck != null) sw_tck.Close();
				Directory.Delete(folder, true);
				throw;
			}

			// //////////////////////////////////////////
			// インデックスファイルなどの後処理
			// //////////////////////////////////////////
			if (NcdTool.Tejun.Mach.Toool_nc) {
				sw_ind.Close();
				if (Index.IndexMain.dnc)
					File.Move("index", folder + @"\index");
			}
			if (sw_pro != null) {
				switch (NcdTool.Tejun.Mach.ID) {
				case Machine.MachID.DMU200P:
				case Machine.MachID.DMU210P:
				case Machine.MachID.DMU210P2:
					sw_pro.Write("N9999 M30");
					sw_pro.Close();
					File.Move("mainf", folder + @"\MAIN.I");
					break;
				case Machine.MachID.V77:
				//case Machine.machID.SNC106:
					sw_pro.WriteLine("M30");
					sw_pro.WriteLine("%");
					sw_pro.Close();
					File.Move("mainf", folder + @"\_MAIN");
					break;
				default: throw new Exception("qwefbqwehfbqh");
				}
			}
			if (sw_tck != null) {
				if (NcdTool.Tejun.Mach.Dmu) {
					sw_tck.WriteLine("N9999 M30");
					sw_tck.WriteLine("% MAIN G71");
				}
				else {
					sw_tck.WriteLine("M30");
					sw_tck.WriteLine("%");
				}
				sw_tck.Close();

				// AOIなど他のディレクトリへコピーされた場合は出力しない
				if (Directory.Exists(folder))
					File.Move("_toolchk", folder + @"\_TOOLCHK");
				else
					File.Delete("_toolchk");
			}
			if (NcdTool.Tejun.Mach.ID == Machine.MachID.LineaM) {
				File.Move(stnam, folder + @"\" + stnam);
				File.Copy("MC1_O2000.dnc", folder + @"\MC1_O3000.dnc");
				File.Move("MC1_O2000.dnc", folder + @"\MC1_O2000.dnc");
			}
		}

		/// <summary>
		/// ＮＣデータの変換(ncgen)
		/// </summary>
		/// <param name="disp_message"></param>
		private void NcConv_General(Label disp_message) {
			int errNo = 0;
			//int chkNo = 0;
			//bool kensho;
			Random rnd = new Random();

			NcdTool.NcName.NcNam ncNam;
			string otNam;
			List<NcToolL> toolList = new List<NcToolL>();
			//NcToolL[][][] NcdOutTol;

			// チェックのための変数
			int chk_cnt = 0, chk_3do = 0, chk_3dh = 0, chk_rev = 0, chk_lnk = 0;

			if (ProgVersion.NcOutChkProb > 20) MessageBox.Show("NcOutChkProb は２０より大きい");

			foreach (NcToolL[][] unitNCD in ncoutList.NcdOutTol) {
				// /////////////////
				// ＮＣデータの変換
				// /////////////////
				ncNam = unitNCD[0][0].Ncnam;
				//kensho = false;

				// 同一ＮＣデータ内の工具単位情報のリストtoolList
				toolList.Clear();
				foreach (NcToolL[] unitOUT in unitNCD) toolList.AddRange(unitOUT);

				disp_message.Text = "ＮＣデータの変換中（" + ncNam.nnam + "）";

				// チェックの変数セット
				chk_cnt++;
				if (ncNam.nmgt.Dimn == '3') chk_3do++;
				if (ncNam.nmgt.Dimn == '3')
					if (NcdTool.Tejun.Mach.ncCode.HSP_ON != null) chk_3dh++;
				if (ncNam.nggt.rev) chk_rev++;

				try {
					using (NcMain main = new NcMain(toolList, NcdTool.Tejun.Mach, ".nc")) {
						main.Ncmain(disp_message, NcdTool.Tejun.Mach);
						if (main.Setsuzoku[0]) chk_lnk++;   // 工程間接続の設定
					}
				}
				catch (Exception ex) {
					errNo++;
					if (ex.InnerException == null) throw;
					throw new Exception("ＮＣデータ変換の一部で以下の致命的エラーが発生しました\n" + ex.Message);
				}
				// /////////////////
				// 出力単位の処理
				// /////////////////
				foreach (NcToolL[] unitOUT in unitNCD) {
					otNam = unitOUT[0].Outnam;
					if (unitOUT[0].tNodeChecked == false) {
						if (ProgVersion.Debug) MessageBox.Show($"debug: ＮＣデータ{otNam}   工具{unitOUT[0].tolInfo.Toolset.ToolName} は出力されません");
						File.Delete(otNam + ".nc");
						//if (kensho) File.Delete(otNam + "_chk.nc");
						continue;
					}
					Application.DoEvents();
					File.Move(otNam + ".nc", folder + @"\unix\" + otNam);
				}
			}
			// 変換状況の出力
			LogOut.CheckOutput(LogOut.FNAM.NCCONVERT, NcdTool.Tejun.TejunName, NcdTool.Tejun.Mach.name,
				$"Count={chk_cnt:d} 3d={chk_3do:d} 3dhi={chk_3dh:d} rev={chk_rev:d} lnk={chk_lnk:d}");
		}

		/// <summary>
		/// ＮＣデータの工具単位分割(BaseNcForm._5AXIS)
		/// </summary>
		/// <param name="disp_message"></param>
		private void NcConv_Buhin(Label disp_message) {
			CamUtil.LCode.NcLineCode nowd, nxtd;
			CamUtil.LCode.NcQueue ncQue;

			StreamWriter fpo = null;
			string[] outtxt, tline;
			List<NcToolL> tdat = new List<NcToolL>();
			string com = "";
			bool skip = false;	// 分割時の行スキップ用変数

			int tlNo;
			int clNo;
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll) {
				Application.DoEvents();
				tlNo = clNo = -1;
				tline = null;
				tdat.Clear();
				foreach (NcToolL ncoutName in ncoutList.Tl) {
					if (ncnam == ncoutName.Ncnam)
						tdat.Add(ncoutName);
				}

				// 工具１本で分割なしの場合はコピーのみ実行する
				if (tdat.Count == 1) {
					if (tdat[0].tNodeChecked)
						File.Copy(ncnam.Ncdata.fulnamePC, folder + @"\unix\" + tdat[0].Outnam);
					else
						if (ProgVersion.Debug)
							MessageBox.Show($"debug: ＮＣデータ{tdat[0].Outnam}   工具{tdat[0].tolInfo.Toolset.ToolName} は出力されません");
				}
				else {
					using (StreamReader fpi2 = new StreamReader(ncnam.Ncdata.fulnamePC)) {
						ncQue = new CamUtil.LCode.NcQueue(5, false, ncnam.Ncdata.ncInfo.xmlD.NcClrPlaneList, NcdTool.Tejun.BaseNcForm, CamUtil.LCode.NcLineCode.GeneralDigit, true, true);
						while (true) {
							ncQue.NextLine(fpi2);
							if (ncQue[0] == null) {
								break;
							}

							nowd = ncQue[0];
							nxtd = ncQue.QueueMax > 0 ? ncQue[1] : null;

							if (nowd.NcLine.IndexOf("%") == 0) continue;
							// G100 の前に挿入するためにここでは出力しない。後からtline[1]に代入し出力する。
							if (nowd.NcLine.Length == 0 || nowd.NcLine.IndexOf("G65P8730") == 0) {
								com = nowd.OutLine[0];
								continue;
							}

							// G100 とそれ以降３行の文字列の保存
							// N   T
							// 2 :   :"(................)"
							// 3 : 1 :"G100T"
							// 4 : 2 :"G65P9376"
							// 5 : 3 :"G65P8700"
							// 6 : 4 :"X....Y...
							// 7 : 5 :"M98P8103"	(ない場合はG65P8102)
							// 8 : 6 :"G65P8102..." (ない場合もある)
							if (nowd.B_g100) {
								tline = new string[9];
								clNo = 0;
								tline[3] = nowd.OutLine[0];
							}
							else if (nowd.LnumT <= 6) {
								tline[nowd.LnumT + 2] = nowd.OutLine[0];
								switch (nowd.LnumT + 2) {
								case 4:
									if (tline[4].IndexOf("G65P9376") != 0) { fpo?.Close(); throw new Exception("qewfbqewhr"); }
									break;
								case 5:
									if (tline[5].IndexOf("G65P8700") != 0) { fpo?.Close(); throw new Exception("qewfbqewhr"); }
									break;
								case 6:
									if (tline[6].IndexOf("X") != 0) { fpo?.Close(); throw new Exception("qewfbqewhr"); }
									break;
								case 7:
									if (tline[7].IndexOf("M98P8103") != 0 && tline[7].IndexOf("G65P8102") != 0) { fpo?.Close(); throw new Exception("qewfbqewhr"); }
									break;
								case 8:
									if (tline[8].IndexOf("G65P8102") != 0 && tline[8].IndexOf("Z") != 0) { fpo?.Close(); throw new Exception("qewfbqewhr"); }
									break;
								}
							}
							else if (nowd.NcLine.IndexOf("G65P8700") == 0)  // 途中での加工方向の変更を反映
								tline[5] = nowd.OutLine[0];

							// ＣＬ数の積算
							if (nowd.NcLine.IndexOf("M98P9017") == 0) clNo++;

							// 出力行の作成（寿命分割）
							outtxt = Divide(ncQue, tline, tlNo < 0 ? null : tdat[tlNo].Smch, clNo, ref skip);

							// データの出力とファイル分割処理
							for (int ii = 0; ii < outtxt.Length; ii++) {
								if (outtxt[ii].IndexOf("G100T") == 0 || outtxt[ii].IndexOf("M30") == 0 || outtxt[ii].IndexOf("M02") == 0) {
									if (tlNo >= 0) {
										fpo.WriteLine("M30");
										fpo.WriteLine("%");
										fpo.Close();
										if (tdat[tlNo].tNodeChecked)
											File.Move(tdat[tlNo].Outnam + ".nc", folder + @"\unix\" + tdat[tlNo].Outnam);
										else {
											File.Delete(tdat[tlNo].Outnam + ".nc");
											if (ProgVersion.Debug)
												MessageBox.Show($"debug: ＮＣデータ{tdat[tlNo].Outnam}   工具{tdat[tlNo].tolInfo.Toolset.ToolName} は出力されません");
										}
									}
									if (outtxt[ii].IndexOf("M30") == 0 || outtxt[ii].IndexOf("M02") == 0) {
										fpi2.ReadToEnd();
										break;
									}
									tlNo++;
									fpo = new StreamWriter(tdat[tlNo].Outnam + ".nc");
									fpo.WriteLine("%");
									// Ｇ１００の前のコメントとG65P8730の処理
									if (com.Length > 0) { tline[2] = com; com = ""; }
									if (tline[2] != null) fpo.WriteLine(tline[2]);
								}
								// コメントの出力
								if (com.Length > 0) { fpo.WriteLine(com); com = ""; }
								// 出力
								fpo.WriteLine(outtxt[ii]);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// 寿命分割など出力する文字列の作成
		/// </summary>
		/// <param name="ncQue">ＮＣデータ行</param>
		/// <param name="tline">保存されたＧ１００近辺の文字列</param>
		/// <param name="match">マッチングと寿命分割情報</param>
		/// <param name="clNo">現在処理中のＣＬ番号</param>
		/// <param name="skip">読み飛ばし可否変数</param>
		/// <returns></returns>
		private string[] Divide(CamUtil.LCode.NcQueue ncQue, string[] tline, NcdTool.TMatch.MatchK match, int clNo, ref bool skip) {
			List<string> outtxt = new List<string>();
			int mfc = 80000;

			CamUtil.LCode.NcLineCode nowd = ncQue[0];
			CamUtil.LCode.NcLineCode nxtd = ncQue[1];

			// //////////////////////////////
			// 寿命分割なし
			// //////////////////////////////
			if (match == null || match.divData.kotEnd == Int32.MaxValue) {
				outtxt.Add(nowd.OutLine[0]);
			}
			// //////////////////////////////
			// ＣＬの切れ目で分割（M98P9017）
			// //////////////////////////////
			else if (match.divData.divPoint == null) {
				if (match.divData.kotEnd >= clNo)
					outtxt.Add(nowd.OutLine[0]);
				else {
					if (nowd.NcLine != "M98P9017") throw new Exception("qwefqberfhbqh");
					outtxt.Add("M98P9306");
					outtxt.Add(tline[3]);	// G100
					outtxt.Add(tline[4]);
					if (nxtd.NcLine.IndexOf("G65P8700") != 0)
						outtxt.Add(tline[5]);
				}
			}
			// //////////////////////////////
			// ＣＬの途中で分割
			// //////////////////////////////
			else {
				if (match.divData.kotEnd != clNo || nowd.LnumN < match.divData.divPoint.Value)
					outtxt.Add(nowd.OutLine[0]);
				else {
					if (nowd.LnumN == match.divData.divPoint.Value) {
						if (nowd.G1 != 0 && nowd.Xyzsf.Fi != mfc) {
							; //MessageBox.Show("新たな方法でＮＣデータを寿命分割します。作成後ＮＣデータのチェックが必要なので藤本まで連絡ください。");
						}
						else
							skip = true;	// スキップオン
						outtxt.Add(nowd.OutLine[0]);
					}
					// 最後の早送りまで読み飛ばす
					else if (skip) {
						// 現在行が早送りではなくなってしまった。
						if (nowd.G1 != 0 && nowd.Xyzsf.Fi != mfc)
							throw new Exception("寿命分割で予期しないエラーが発生した。");
						// 次行が早送りではない箇所が見つかった
						if (nxtd.G1 != 0 && nxtd.Xyzsf.Fi != mfc)
							skip = false;	// スキップオフ
						else if (nxtd.OutLine.Txt.Length == 0) {
							// 次次行が早送りではない箇所が見つかった
							if (ncQue[2].G1 != 0 && ncQue[2].Xyzsf.Fi != mfc) {
								//check実施によりコメント消去 2017/01/12
								//MessageBox.Show("新たな方法でＮＣデータを寿命分割します。作成後ＮＣデータのチェックが必要なので藤本まで連絡ください。");
								skip = false;	// スキップオフ
								nxtd = ncQue[2];
							}
						}
					}
					if (skip == false) {
						if (nowd.G1 != 0 && nowd.Xyzsf.Fi != mfc) {
							// 切削送りで寿命分割する場合
							outtxt.Add("G01Z" + ((double)(nowd.Xyzsf.Z + 10.0)).ToString("0.0###") + "F" + mfc.ToString("0"));
						}
						outtxt.Add("M09");
						outtxt.Add("M98P9306");
						outtxt.Add(tline[3]);	// G100
						outtxt.Add(tline[4]);
						outtxt.Add(tline[5]);
						outtxt.Add("X" + nowd.Xyzsf.X.ToString("0.0###") + "Y" + nowd.Xyzsf.Y.ToString("0.0###"));
						if (tline[7].IndexOf("M98P8103") == 0) outtxt.Add(tline[7]);
						if (tline[7].IndexOf("G65P8102") == 0) outtxt.Add(tline[7]);
						if (tline[8].IndexOf("G65P8102") == 0) outtxt.Add(tline[8]);
						if (nowd.G1 != 0 && nowd.Xyzsf.Fi != mfc) {
							// 切削送りで寿命分割する場合
							outtxt.Add("G01Z" + ((double)(nowd.Xyzsf.Z + 5.0)).ToString("0.0###") + "F" + mfc);
							outtxt.Add("G01Z" + nowd.Xyzsf.Z.ToString("0.0###") + "F" + nowd.Xyzsf.Fi);
						}
						else {
							outtxt.Add("Z" + nowd.Xyzsf.Z.ToString("0.0###"));
						}
					}
				}
			}
			return outtxt.ToArray();
		}

		private string ScheduleFileOutput() {
			string outname;
			int ii;
			string dncdir = "c:\\NCProgram";
			int tsh = 0;	// シート番号 - 1
			//==============================
			//工具本数による使用データの設定
			//==============================
			string LGT20;
			outname = "TOOL_GT20.ncd";
			LGT20 = "#833=1(TOOL MAX No >20)";

			//==============================
			//初期設定ＮＣデータの作成と送信
			//==============================
			string process = this.ncoutList.Tl[0].Ncnam.Ncdata.ncInfo.xmlD.ProcessName;
			using (StreamWriter otxt = new StreamWriter(outname)) {
				otxt.WriteLine("%");
				otxt.WriteLine("O9020");
				otxt.WriteLine(LGT20);
				otxt.Write("G65P9375");
				for (ii = 0; ii < process.Length; ii++) {
					if (ii % 3 == 0)
						otxt.Write("I");
					else if (ii % 3 == 1)
						otxt.Write("J");
					else
						otxt.Write("K");
					otxt.Write(Char.ConvertToUtf32(process, ii));
					otxt.Write(".");
				}
				otxt.WriteLine("(" + process + ")");
				otxt.WriteLine("#834=" + Index.IndexMain.baseHeight);
				otxt.WriteLine("#844=" + Index.IndexMain.mold_X.ToString("0.00"));
				otxt.WriteLine("#845=" + Index.IndexMain.mold_Y.ToString("0.00"));
				otxt.WriteLine("M99");
				otxt.WriteLine("%");
			}

			//==============================
			//スケジュールデータの作成と送信
			//==============================
			using (StreamWriter otxt = new StreamWriter("MC1_O2000.dnc")) {
				ii = 1;
				//初期設定ＮＣデータのスケジュール組み込み
				otxt.WriteLine("[NCPROG" + ii + "]");
				otxt.WriteLine("FOLDER=" + dncdir + "\\" + lfldr);
				otxt.WriteLine("NC_FILE=" + outname);
				otxt.WriteLine("FILE_TIME=2005/04/01 00:00:00");
				otxt.WriteLine("FINISH=0");
				otxt.WriteLine("STOP=0");
				ii = ii + 1;
				//工具事前チェックのスケジュール組み込み
				otxt.WriteLine("[NCPROG" + ii + "]");
				otxt.WriteLine("FOLDER=" + dncdir + "\\" + lfldr);
				otxt.WriteLine("NC_FILE=" + "TOOL_CHECK" + tsh + ".ncd");
				otxt.WriteLine("FILE_TIME=2005/04/01 00:00:00");
				otxt.WriteLine("FINISH=1");
				otxt.WriteLine("STOP=0");
				ii = ii + 1;
				for (int jj = 0; jj < ncoutList.Tl.Count; jj++) {
					if (tsh < ncoutList.Tl[jj].Smch.SnumN - 1) {
						tsh = ncoutList.Tl[jj].Smch.SnumN - 1;
						//工具シート追加
						otxt.WriteLine("[NCPROG" + ii + "]");
						otxt.WriteLine("FOLDER=" + dncdir);
						otxt.WriteLine("NC_FILE=M00.ncd");
						otxt.WriteLine("FILE_TIME=2005/04/01 00:00:00");
						otxt.WriteLine("FINISH=0");
						otxt.WriteLine("STOP=0");
						ii = ii + 1;
						//工具事前チェックのスケジュール組み込み
						otxt.WriteLine("[NCPROG" + ii + "]");
						otxt.WriteLine("FOLDER=" + dncdir + "\\" + lfldr);
						otxt.WriteLine("NC_FILE=" + "TOOL_CHECK" + tsh + ".ncd");
						otxt.WriteLine("FILE_TIME=2005/04/01 00:00:00");
						otxt.WriteLine("FINISH=1");
						otxt.WriteLine("STOP=0");
						ii = ii + 1;
					}
					otxt.WriteLine("[NCPROG" + ii + "]");
					otxt.WriteLine("FOLDER=" + dncdir + "\\" + lfldr);
					otxt.WriteLine("NC_FILE=" + ncoutList.Tl[jj].OutnamNEW + ".ncd");
					otxt.WriteLine("FILE_TIME=" + ncoutList.Tl[jj].Ncnam.Ncdata.ftim.ToString("yyyy/MM/dd HH:mm:ss"));
					otxt.WriteLine("FINISH=0");
					otxt.WriteLine("STOP=0");
					ii = ii + 1;
				}
			}
			return outname;
		}

		/// <summary>
		/// インデックスファイルの先頭行を出力する
		/// </summary>
		private void IndexFileOutput1(StreamWriter sw_ind) {
			//if (workName.IndexOfAny(" \\/:*?\"<>|".ToCharArray()) >= 0)
			if (Index.IndexMain.workName.IndexOfAny("\\/:*?\"<>|".ToCharArray()) >= 0)
				throw new Exception("使用できない文字がワーク名に存在します");
			if (Index.IndexMain.comment.IndexOfAny("\\/:*?\"<>|".ToCharArray()) >= 0)
				throw new Exception("使用できない文字がコメントに存在します");
			if (Index.IndexMain.progress.IndexOfAny("\\/:*?\"<>|".ToCharArray()) >= 0)
				throw new Exception("使用できない文字が座標系に存在します");
			if (Index.IndexMain.setupName.Length == 0)
				throw new Exception("qrjfbwqergvfwqnfr");


			if (Index.IndexMain.dnc) {
				sw_ind.WriteLine("work-name:" + Index.IndexMain.workName);
				sw_ind.WriteLine("setup-name:" + Index.IndexMain.setupName);
				{
					// 稼働時間の自動収集用
					sw_ind.WriteLine("order-no:" + Index.IndexMain.order_no);		// 受注ＮＯ
					sw_ind.WriteLine("parts-no:" + Index.IndexMain.parts_no);		// 型ＮＯ
					sw_ind.WriteLine("step-no:" + Index.IndexMain.step_no);			// 工程ＮＯ
					sw_ind.WriteLine("process-no:" + Index.IndexMain.process_no);	// 部品ＮＯ
				}
				sw_ind.WriteLine("work:" + Index.IndexMain.comment);
				sw_ind.WriteLine("progress:" + Index.IndexMain.progress);
				switch (NcdTool.Tejun.Mach.ID) {
				case Machine.MachID.DMU200P:
				case Machine.MachID.DMU210P:
				case Machine.MachID.DMU210P2:
					sw_ind.WriteLine("(w=54)");
					break;
				case Machine.MachID.D500:
				case Machine.MachID.LineaM:
					sw_ind.WriteLine("(w={0},tool_check={1},lgt20={2},base={3:0.000},base2={4:0.000},sozai_u={5:0.00},sozai_v={6:0.00},height={7})",
						Index.IndexMain.pallet, 0, 1, Index.IndexMain.baseHeight, Index.IndexMain.baseHeight, Index.IndexMain.mold_X, Index.IndexMain.mold_Y, Index.IndexMain.Height);
					break;
				default:
					sw_ind.WriteLine("(w=54)");
					break;
				}
			}
		}
	}
}
