using System;
using System.Collections.Generic;
using System.Text;

using CamUtil.LCode;
using System.IO;
using System.Windows.Forms;

namespace NCSEND2.Output
{
	class NcOutput_Dynavista2D : NcOutput
	{
		private long temp_corner_feed;

		private class Hole
		{
			public struct Kako
			{
				public string tool;
				public string ttypb;
				public double z;
			}
			public long x, y;
			public List<Kako> kakos;

			public Hole(long x, long y, Kako kakotmp) {
				this.x = x;
				this.y = y;
				kakos = new List<Kako>();
				kakos.Add(kakotmp);
			}
		}
		List<Hole> holes;

		// コンストラクタ
		public NcOutput_Dynavista2D(NcReader sr, NCINFO.NcInfoCam ncd) : base(sr, ncd) { this.holes = new List<Hole>(); }

		/// <summary>
		/// ＮＣデータ１工具分の出力
		/// </summary>
		/// <param name="tcnt">工具順の数</param>
		/// <param name="tfoo">ＮＣデータの出力先</param>
		/// <param name="aprzd">加工原点Ｚ</param>
		/// <returns>パスの移動距離</returns>
		protected override NcLineCode.NcDist NcConvertN(int tcnt, StreamWriter tfoo, double aprzd) {
			NcLineQue txtd;
			Hole prehole;
			bool nextOut = true;

			// 出力ＮＣデータの情報。加工長を計算するために使用
			NcLineCode outCode = new NcLineCode(new double[] { aprzd }, CamUtil.BaseNcForm.GENERAL, post, false, false);
			outCode.NextLine("%");
			NcLineCode.NcDist passLength =
				new NcLineCode.NcDist(ncd.sqlDB[tcnt].Feedrate, ncd.xmlD[tcnt].MachiningAxisList);

			// ＸＹ原点移動
			string xyzOrg = null;
			// コーナー減速速度の設定
			temp_corner_feed = (long)Math.Round(ncd.NcdtInfo[tcnt].Feedrt * 0.9);

			// ＤＢ切削条件（回転数）
			if (ncd.NcdtInfo[tcnt].Spidle < 0.001)
				throw new Exception("回転数のデータがありません。");
			double RateS = ncd.sqlDB[tcnt].Spin / ncd.NcdtInfo[tcnt].Spidle;
			// ＤＢ切削条件（送り速度）
			if (ncd.NcdtInfo[tcnt].Feedrt < 0.001)
				throw new Exception("切削送り速度のデータがありません。");
			double RateF = ncd.sqlDB[tcnt].Feedrate / ncd.NcdtInfo[tcnt].Feedrt;

			while (true) {
				if (ncQue.QueueMax == 0)
					break;
				if (ncQue.NcPeek(1).Tcnt > tcnt)
					break;

				//txtd = ReadNcd_haisi();
				txtd = ncQue.NextLine(sr);
				if (txtd.NcLine == "M98P0006") if (ncQue.NcPeek(-1).NcLine == "O0001") {
						txtd.OutLine.Set("");
						continue;
					}
				prehole = null;

				// Ｇ１００の処理(G100T01)
				if (txtd.LnumT == 1) {
					if (txtd.B_g100 != true) {
						MessageBox.Show(
							"NCDATA ERROR lnum=1", "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
					// Ｉコードを挿入
					txtd.OutLine.Set(txtd.OutLine.Txt + ncd.sqlDB[tcnt].toolsetTemp.ICode);
					// 工具種類・直径情報コードを挿入
					txtd.OutLine.Set(txtd.OutLine.Txt + ncd.sqlDB[tcnt].toolsetTemp.DCode);
					// Ｓコードを挿入
					for (int ii = 0; ii < ncQue.QueueMax; ii++) {
						//MessageBox.Show(NcPeek(ii).NcLine);
						if (ncQue.NcPeek(ii + 1).B_26('S')) {
							txtd.OutLine.Set(txtd.OutLine.Txt + "S" + ncQue.NcPeek(ii + 1).Xyzsf.Si.ToString());
							// ＳコードをＤＢ条件に変更
							txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'S', RateS));
							if (Math.Abs(ncQue.NcPeek(ii + 1).Xyzsf.Si - ncd.NcdtInfo[tcnt].Spidle) > 0.001)
								throw new Exception("ＮＣデータ内の回転数がＸＭＬ情報と異なる。");
							break;
						}
						if (ii + 1 == ncQue.QueueMax) {
							MessageBox.Show(
								"NCDATA ERROR S code", "NcOutput",
								MessageBoxButtons.OK, MessageBoxIcon.Error);
							Application.Exit();
							throw new Exception("");
						}
					}
					if (txtd.OutLine.Comment != "")
						txtd.OutLine.Set(txtd.OutLine.Txt + "(" + txtd.OutLine.Comment + ")");
				}

				// 基準Ｚの処理(G90G00Z100000)
				else if (txtd.LnumT == 2) {
					if (txtd.B_26('Z') != true) {
						MessageBox.Show(
							"NCDATA ERROR lnum=2", "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
					// 出力しない
					txtd.OutLine.Set("");
				}

				// 基準ＸＹの処理(X0Y0)
				else if (txtd.LnumT == 3) {
					if (txtd.B_26('X') != true ||
						txtd.B_26('Y') != true) {
						MessageBox.Show(
							"NCDATA ERROR lnum=2", "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
					xyzOrg = txtd.OutLine.Txt;
					txtd.OutLine.Set("G00G90" + txtd.OutLine.Txt);
				}

				// 基準平面設定行の処理(G17)
				else if (txtd.LnumT == 4) {
					if (!txtd.OutLine.Txt.Equals("G17")) {
						MessageBox.Show(
							"NCDATA ERROR lnum=4", "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
					// 出力しない
					txtd.OutLine.Set("");
				}

				// 主軸・加工開始の処理(S1800M03)
				else if (txtd.LnumT == 5) { txtd.OutLine.Set("");/* 出力しない */}

				// 加工点への移動(Z25000;X100000Y311000;.....)
				else if (txtd.LnumT == 6) {
					if (txtd.G1 != 0 || (txtd.B_26('Z') == false && ncQue.NcPeek(1).B_26('Z') == false)) {
						MessageBox.Show(
							"安全平面が設定されていません（" + ncd.xmlD.CamDataName + "）",
							"NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
					if (txtd.B_26('Z') != false) {
						if (
							ncQue.NcPeek(1).G1 == 0 &&
							ncQue.NcPeek(1).G6 == 67 &&
							ncQue.NcPeek(1).G8 == 80 &&
							(ncQue.NcPeek(1).B_26('X') || ncQue.NcPeek(1).B_26('Y'))) {
							//tfoo.Write(ncQue.NcPeek(1).NcLineOut());
							string outText = ncQue.NcPeek(1).OutLine.Txt;
							if (outText.IndexOf("X") < 0) { outText = "X0" + outText; }	// ADD in 2019/01/15
							if (outText.IndexOf("Y") < 0) { outText = outText + "Y0"; } // ADD in 2019/01/15
							txtd.OutLine.MaeAdd(outText);
							nextOut = false;
						}
						else {
							//tfoo.WriteLine(xyzOrg);
							txtd.OutLine.MaeAdd(xyzOrg);
						}
					}
					else {	// NcPeek(0).b_26['Z'] != false
						if (
							ncQue.NcPeek(1).G1 == 0 &&
							ncQue.NcPeek(1).G6 == 67 &&
							ncQue.NcPeek(1).G8 == 80 &&
							(txtd.B_26('X') || txtd.B_26('Y'))) {
							//MessageBox.Show("2006/10/24修正により対応したデータ");
							txtd.OutLine.AtoAdd(ncQue.NcPeek(1).OutLine.Txt);
							nextOut = false;
						}
						else {
							MessageBox.Show(
								"安全平面が設定されていません２（" + ncd.xmlD.CamDataName + "）",
								"NcOutput",
								MessageBoxButtons.OK, MessageBoxIcon.Error);
							Application.Exit();
							throw new Exception("");
						}
					}
				}
				// 加工
				else {
					if (nextOut == false) {
						nextOut = true;
						continue;   // 出力しない
					}
					if (txtd.OutLine.Txt.Equals("G98"))
						continue;   // 出力しない
					if (txtd.OutLine.Txt.Equals("M05"))
						continue;   // 出力しない
					if (ncQue.QueueMax > 2) {
						if (ncQue.NcPeek(3).OutLine.Txt.Equals("M98P0006")) {
							if (txtd.G8 == 80 && txtd.G6 == 67 &&
								(txtd.B_26('X') || txtd.B_26('Y')) &&
								txtd.B_26('Z') != true)
								continue;   // 出力しない
						}
					}

					// 固定サイクル、カスタムマクロ　モード
					if (txtd.G8 != 80 || txtd.G6 != 67 || txtd.B_g6) {
						// 孔情報（位置、深さ、工具）の記録
						Hole.Kako kakotmp = new Hole.Kako();
						kakotmp.tool = ncd.sqlDB[tcnt].toolsetTemp.ToolName;
						kakotmp.ttypb = ncd.sqlDB[tcnt].toolsetTemp.CutterTypeCaelum;
						if (txtd.G8 != 80)
							kakotmp.z = txtd.G8p['Z'].D;
						else if (txtd.G6 != 67)
							kakotmp.z = txtd.G6p['Z'].Set ? txtd.G6p['Z'].D : 0.0;	// Ｚ値がないマクロ：回り止め、面取り、ヘリカルタップ
						for (int ii = 0; ii < holes.Count; ii++) {
							if (Math.Abs(holes[ii].x - txtd.Xyzsf.Xi) <= 1 && Math.Abs(holes[ii].y - txtd.Xyzsf.Yi) <= 1) {
								holes[ii].kakos.Add(kakotmp);
								prehole = holes[ii];
								break;
							}
						}
						if (prehole == null) {
							prehole = new Hole(txtd.Xyzsf.Xi, txtd.Xyzsf.Yi, kakotmp);
							holes.Add(prehole);
						}

						// 固定サイクルモード
						if (txtd.G8 != 80) {
							// Ｙコードの追加
							if (txtd.OutLine.Txt.IndexOf('Y') < 0) {
								txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'Z', "Y" + txtd.Xyzsf.Yi.ToString(), true));
							}
							// Ｘコードの追加
							if (txtd.OutLine.Txt.IndexOf('X') < 0) {
								txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'Y', "X" + txtd.Xyzsf.Xi.ToString(), true));
							}
							// Ｆコードの追加（固定サイクル開始行でＦコード無し）
							if (txtd.B_g8 == true && txtd.B_26('F') == false) {
								txtd.OutLine.Set(txtd.OutLine.Txt + "F" + txtd.Xyzsf.Fi.ToString());
							}
						}

						// カスタムマクロモード
						else if (txtd.G6 != 67 || txtd.B_g6) {
							NcLineCode.NumCode code;
							if (txtd.G6 == 67 || txtd.B_g6 == false)
								throw new Exception("ダイナビスタで G66 は対応していない。");

							// カスタムマクロのOutLineは小数点を入力するために再構築する
							txtd.OutLine.Set("G66P" + txtd.G6p.ProgNo.ToString("0000"));
							if (txtd.NumList[0].ncChar != 'G') throw new Exception("ergfnwtrjn");
							if (txtd.NumList[1].ncChar != 'P') throw new Exception("ergfnwtrjn");
							if (txtd.NumList[2].ncChar != 'X') throw new Exception("ergfnwtrjn");
							if (txtd.NumList[3].ncChar != 'Y') throw new Exception("ergfnwtrjn");

							switch (txtd.G6p.ProgNo) {
							case 8010:  // 周り止め
							case 8013:  // 周り止め
							case 8015:  // 周り止め
							case 8011:  // 周り止め（コアピン・スリーブピン逃がし部加工用 ADD 2007.01.25）
							case 8014:  // 周り止め（コアピン・スリーブピン逃がしあり専用 ADD 2007.01.25）
							case 8016:  // 周り止め（コアピン・スリーブピン逃がし部加工用 ADD 2019.06.12）
							case 8019:  // 周り止め（コアピン・スリーブピン逃がしあり専用 ADD 2019.06.12）
								for (int ii = 4; ii < txtd.NumList.Count; ii++) {
									code = txtd.NumList[ii];
									switch (code.ncChar) {
									case 'Q':
										txtd.OutLine.Set(txtd.OutLine.Txt + "I" + code.dblData.ToString("###0.0##"));
										break;
									case 'R':
										txtd.OutLine.Set(txtd.OutLine.Txt + "C" + ((double)(code.dblData - 3.0)).ToString("###0.0##"));
										break;
									case 'Z':
										break;
									case 'P':
										txtd.OutLine.Set(txtd.OutLine.Txt + "U" + ((double)(code.dblData / Math.Pow(10.0, (double)decimalNum))).ToString("###0.0##"));
										break;
									default:
										txtd.OutLine.Set(txtd.OutLine.Txt + code.ncChar.ToString() + code.dblData.ToString("###0.0##"));
										break;
									}
								}
								break;
							case 8025:  // スプルーロック
								for (int ii = 4; ii < txtd.NumList.Count; ii++) {
									code = txtd.NumList[ii];
									switch (txtd.NumList[ii].ncChar) {
									case 'Q':
										txtd.OutLine.Set(txtd.OutLine.Txt + "I" + code.dblData.ToString("###0.0##"));
										break;
									case 'R':
										txtd.OutLine.Set(txtd.OutLine.Txt + "C" + ((double)(code.dblData - 3.0)).ToString("###0.0##"));
										break;
									case 'P':
										txtd.OutLine.Set(txtd.OutLine.Txt + "D" + ((double)(code.dblData / Math.Pow(10.0, (double)decimalNum))).ToString("###0.0##"));
										txtd.OutLine.Set(txtd.OutLine.Txt + "E" + ((double)(code.dblData / Math.Pow(10.0, (double)decimalNum))).ToString("###0.0##"));
										break;
									default:
										txtd.OutLine.Set(txtd.OutLine.Txt + code.ncChar.ToString() + code.dblData.ToString("###0.0##"));
										break;
									}
								}
								break;
							/*
							case 8046:  // エアブロー
								for (int ii = 4; ii < txtd.numList.Count; ii++) {
									code = txtd.numList[ii];
									switch (txtd.numList[ii].ncchar) {
									case 'Z':
										txtd.outLine.Set(txtd.outLine.txt + "Z" + ((double)(code.d + 5.05)).ToString("###0.0##"));
										break;
									default:
										txtd.outLine.Set(txtd.outLine.txt + code.ncchar.ToString() + code.d.ToString("###0.0##"));
										break;
									}
								}
								break;
							*/
							case 8900:  // 多段孔加工（１段目）
								for (int ii = 4; ii < txtd.NumList.Count; ii++)
									txtd.OutLine.Set(txtd.OutLine.Txt + txtd.NumList[ii].ncChar.ToString() + txtd.NumList[ii].dblData.ToString("###0.0##"));
								break;
							case 8200:  // 深孔ドリル加工（２段目以降）
							case 8700:  // 多段孔加工（２段目以降）
							//case 8280:  // ガンドリル
								double clearance = 0.0;
								// １段目の穴加工の有無をチェック
								if (prehole.kakos.Count <= 1) {
									MessageBox.Show(
										"最初の孔で P8200, P8700 が使われた ncline=" + txtd.OutLine.Txt,
										"NcOutput",
										MessageBoxButtons.OK, MessageBoxIcon.Error);
									Application.Exit();
									throw new Exception("");
								}

								for (int ii = 4; ii < txtd.NumList.Count; ii++)
									switch (txtd.NumList[ii].ncChar) {
									case 'R':
										// 加工開始Ｚ位置（Ｋコード）の挿入
										if (txtd.G6p.ProgNo == 8200) clearance = 5.0;
										if (txtd.G6p.ProgNo == 8700) clearance = 5.0;
										if (txtd.G6p.ProgNo == 8280) clearance = 10.0;
										txtd.OutLine.Set(txtd.OutLine.Txt + "K" + (prehole.kakos[prehole.kakos.Count - 2].z + clearance).ToString("###0.0##"));
										txtd.OutLine.Set(txtd.OutLine.Txt + txtd.NumList[ii].ncChar.ToString() + txtd.NumList[ii].dblData.ToString("###0.0##"));
										break;
									default:
										txtd.OutLine.Set(txtd.OutLine.Txt + txtd.NumList[ii].ncChar.ToString() + txtd.NumList[ii].dblData.ToString("###0.0##"));
										break;
									}
								break;
							case 8290:	// 超鋼ドリルマクロ
								if (txtd.B_26('Q') == false) {
									MessageBox.Show(
										"P8290 で必要なＱコードが見つからない ncline=" + txtd.NcLine,
										"NcOutput",
										MessageBoxButtons.OK, MessageBoxIcon.Error);
									Application.Exit();
									throw new Exception("");
								}
								for (int ii = 4; ii < txtd.NumList.Count; ii++)
									switch (txtd.NumList[ii].ncChar) {
									case 'Q':
										txtd.OutLine.Set(txtd.OutLine.Txt + "K-" + txtd.NumList[ii].dblData.ToString("###0.0##"));
										break;
									default:
										txtd.OutLine.Set(txtd.OutLine.Txt + txtd.NumList[ii].ncChar.ToString() + txtd.NumList[ii].dblData.ToString("###0.0##"));
										break;
									}
								break;
							case 8400:  // リジッドタップ
								for (int ii = 4; ii < txtd.NumList.Count; ii++)
									txtd.OutLine.Set(txtd.OutLine.Txt + txtd.NumList[ii].ncChar.ToString() + txtd.NumList[ii].dblData.ToString("###0.0##"));
								break;
							case 8401:  // ヘリカルタップ
							case 8402:  // ヘリカルタップ
								for (int ii = 4; ii < txtd.NumList.Count; ii++) {
									code = txtd.NumList[ii];
									switch (code.ncChar) {
									case 'Q':
										txtd.OutLine.Set(txtd.OutLine.Txt + "E" + code.dblData.ToString("###0.0##"));
										break;
									case 'Z':
									case 'F':
										break;
									default:
										txtd.OutLine.Set(txtd.OutLine.Txt + code.ncChar.ToString() + code.dblData.ToString("###0.0##"));
										break;
									}
								}
								break;
							default:
								MessageBox.Show(
									"未登録のカスタムマクロ " + txtd.G6p.ProgName + " が使用された",
									"NcOutput",
									MessageBoxButtons.OK, MessageBoxIcon.Error);
								Application.Exit();
								throw new Exception("");
							}
							// Ｇ６５→Ｇ６６への変換
							txtd.OutLine.AtoAdd("X" + txtd.Xyzsf.Xi.ToString() + "Y" + txtd.Xyzsf.Yi.ToString());
							txtd.OutLine.AtoAdd("G67");
						}
						if (txtd.OutLine.Txt.IndexOf('F') >= 0) {
							if (Math.Abs(txtd.Xyzsf.Fi - ncd.NcdtInfo[tcnt].Feedrt) > 0.9) throw new Exception("qwfwqerfhwrebfh");
							txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));
						}
					}
					else {
						// G00後のG01,G02,G03のある行にＦコードを挿入 2015/05/08
						if (txtd.B_26('F') == false && txtd.B_g1 && txtd.G1 != 0 && ncQue.NcPeek(-1).G1 == 0) {
							txtd.OutLine.Set(txtd.OutLine.Txt + "F" + txtd.Xyzsf.Fi.ToString());
							txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));
						}
						if (txtd.B_26('F') == true) {
							// Ｆコードのある行に"G01,2,3"を挿入
							txtd.AddG123();

							// ///////////////////
							// コーナー減速の処理
							if (txtd.Xyzsf.Fi == temp_corner_feed) {
								txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'F', ncd.sqlDB[tcnt].Feedrate2.ToString("0")));
							}
							else {
								txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));
							}

						}
					}
				}
				for (int ii = 0; ii < txtd.OutLine.Count; ii++) {
					// 変換されたＮＣデータの出力
					tfoo.WriteLine(txtd.OutLine[ii]);
					// 変換されたＮＣデータの加工長を計算
					try {
						outCode.NextLine(txtd.OutLine[ii]);
						if (txtd.Tcnt == tcnt)
							passLength.PassLength(outCode);
					}
					catch (Exception ex) {
						string ss = "ＮＣ名：" + Path.GetFileName(this.ncd.fulfName) + " ";
						if (outCode.G6 != 67) {
							throw new Exception($"{ss}カスタムマクロ {outCode.G6p.ProgName} の加工長計算でエラーが発生しました。\n{ex.Message}");
						}
						if (outCode.G8 != 80) {
							throw new Exception($"{ss}固定サイクル G {outCode.G8.ToString("00")} の加工長計算でエラーが発生しました。\n{ex.Message}");
						}
						throw new Exception(ss + "加工長計算でエラーが発生しました。\n" + ex.Message);
					}
				}
			}
			return passLength;
		}
	}
}
