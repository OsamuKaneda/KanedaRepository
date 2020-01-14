using System;
using System.Collections.Generic;
using System.Text;

using CamUtil.LCode;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

using CamUtil;

namespace NCSEND2.Output
{
	class NcOutput_Tebis_OM : NcOutput
	{
		// コンストラクタ
		public NcOutput_Tebis_OM(NcReader sr, NCINFO.NcInfoCam ncd) : base(sr, ncd) { }

		/// <summary>
		/// ＮＣデータ１工具分の出力
		/// </summary>
		/// <param name="tcnt">工具順の数</param>
		/// <param name="tfoo">ＮＣデータの出力先</param>
		/// <param name="aprzd">加工原点Ｚ</param>
		/// <returns>パスの移動距離</returns>
		protected override NcLineCode.NcDist NcConvertN(int tcnt, StreamWriter tfoo, double aprzd) {
			//ListViewItem lim = ncd.lim;
			//long g0x0y0;
			int jj;
			NcLineQue txtd;

			// 出力ＮＣデータの情報。加工長を計算するために使用
			NcLineCode outCode = new NcLineCode(new double[] { aprzd }, BaseNcForm.GENERAL, post, false, false);
			outCode.NextLine("%");
			NcLineCode.NcDist passLength =
				new NcLineCode.NcDist(ncd.sqlDB[tcnt].Feedrate, ncd.xmlD[tcnt].MachiningAxisList);

			// ＤＢ切削条件（回転数）
			double RateS;
			if (ncd.NcdtInfo[tcnt].Spidle < 0.001) {
				if (ncd.sqlDB[tcnt].toolsetTemp.Probe) RateS = 1.0;
				else throw new Exception("SPINDL 0");
			}
			else
				RateS = ncd.sqlDB[tcnt].Spin / ncd.NcdtInfo[tcnt].Spidle;
			// ＤＢ切削条件（送り速度）
			if (ncd.NcdtInfo[tcnt].Feedrt < 0.001) {
				MessageBox.Show("切削送り速度のデータがありません。",
					"SET KDATA",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				throw new Exception("FEEDRATE 0");
			}
			double RateF = ncd.sqlDB[tcnt].Feedrate / ncd.NcdtInfo[tcnt].Feedrt;

			// ＮＣデータの読み込みと変換
			// 1   %
			// 2   (ファイル名)
			// 3   O0001
			// 4 1 G100TxxXxxSxx

			//     A...B...C...		////// add 2008/08/06

			// 5 2 G00G90XxxYxx
			// 6 3 XxxYxx
			// 7 4 Zxx
			// 8 5 G01ZxxFxx
			// 9 6 XxxYxxFxx

			//g0x0y0 = -1;

			// 傾斜加工情報の有無(0:あり、1:無し)
			int abcAxis = 0;	// ABC軸の行がある場合は１になる

			// 傾斜軸の角度（string）
			int keishaNo = 0;
			Angle3[] abcList = ncd.xmlD[tcnt].MachiningAxisList;

			bool notStart = false;
			while (true) {
				if (notStart && ncQue.QueueMax == 0) break;
				if (notStart && ncQue.NcPeek(1).Tcnt > tcnt) break;
				notStart = true;

				//txtd = ReadNcd_haisi();
				txtd = ncQue.NextLine(sr);

				if (txtd.LnumT == 2) {
					if (
					txtd.OutLine.Txt.IndexOf("A") >= 0 &&
					txtd.OutLine.Txt.IndexOf("B") >= 0 &&
					txtd.OutLine.Txt.IndexOf("C") >= 0) {
						abcAxis = 1;
						Angle3 dtmp = new Angle3(abcList[keishaNo].Jiku, txtd.OutLine.Txt);
						if (!Angle3.MachineEquals(dtmp, abcList[keishaNo], 3))
							throw new Exception(
								"ポストのエラー：ＮＣデータとＣＳＶの軸回転コードが同一ではない。" +
								txtd.OutLine.Txt + " , " + abcList[keishaNo].ToString());
						txtd.OutLine.Set("");
						continue;
					}
					else if (abcList[keishaNo].ToVector() != Vector3.v0)
						throw new Exception("ＮＣデータ内に軸回転のコードがない");
				}

				if (txtd.LnumT == 1) {
					if (txtd.B_g100 != true) {
						MessageBox.Show(
							"NCDATA ERROR", "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
					//jj = STRCHR(txtd, "S", 1);
					jj = txtd.OutLine.Txt.IndexOf('S');
					if (jj < 0) {
						MessageBox.Show(
							"G100 ERROR ncd=" + txtd, "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}

					// ＳコードをＤＢ条件に変更
					txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'S', RateS));
					// ＩコードをＳの前に挿入
					txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'S', ncd.sqlDB[tcnt].toolsetTemp.ICode, true));
					// 工具種類・直径情報コードをＳの前に挿入
					txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'S', ncd.sqlDB[tcnt].toolsetTemp.DCode, true));
				}
				else if (txtd.LnumT - abcAxis == 2) {
					if (txtd.OutLine.Txt.IndexOf("G00") != 0) {
						MessageBox.Show(
							"NCDATA ERROR", "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
				}
				else if (txtd.LnumT - abcAxis == 3) {
					if (txtd.OutLine.Txt.IndexOf("G00") == 0)
						txtd.OutLine.Set(txtd.OutLine.Txt.Substring(3));
					if (txtd.OutLine.Txt.IndexOf(' ') >= 0) {
						LogOut.CheckCount("NcOutput_Tebis_OM 148", false, "lnumT==3行目のＮＣデータにスペースが含まれる" + txtd.OutLine.Txt);
						txtd.OutLine.Set(txtd.OutLine.Txt.Replace(" ", ""));
					}
				}
				else {
					// 工具交換時ではない工具軸角度の行＝＝工程の切れ目
					// 加工工程の切れ目がわかるようなコードを挿入する
					if (
						txtd.OutLine.Txt.IndexOf("A") >= 0 &&
						txtd.OutLine.Txt.IndexOf("B") >= 0 &&
						txtd.OutLine.Txt.IndexOf("C") >= 0) {
						Angle3 dtmp = new Angle3(abcList[keishaNo].Jiku, txtd.OutLine.Txt);
						if (!Angle3.MachineEquals(dtmp, abcList[keishaNo], 3)) {
							keishaNo++;
							if (keishaNo >= abcList.Length)
								throw new Exception("ＣＳＶの軸回転コード数がＮＣデータのそれより少ない");
							if (!Angle3.MachineEquals(dtmp, abcList[keishaNo], 3))
								throw new Exception(
									"ポストのエラー：ＮＣデータとＣＳＶの軸回転コードが同一ではない。" +
									txtd.OutLine.Txt + " , " + abcList[keishaNo].ToString());
							txtd.OutLine.Set(ClLink5Axis.ChgAX + dtmp.ToString() + "(TOOLAXIS)");
						}
						else {
							if (ProgVersion.NotTrialVersion1) {
								if (abcList[keishaNo].ToVector() != Vector3.v0)
									txtd.OutLine.Set(ClLink5Axis.Kotei + "(KOUTEI)");
								else
									txtd.OutLine.Set("");
							}
							else {
								// 同時５軸の場合は傾斜角度０でも加工工程の切れ目を入れる 2018/03/01
								if (ncd.xmlD[tcnt].SimultaneousAxisControll) {
									txtd.OutLine.MaeAdd("G00 B0.0 C0.0");
									txtd.OutLine.Set(ClLink5Axis.Kotei + "(KOUTEI)");
								}
								else if (abcList[keishaNo].ToVector() != Vector3.v0)
									txtd.OutLine.Set(ClLink5Axis.Kotei + "(KOUTEI)");
								else
									txtd.OutLine.Set("");
							}
						}
						if (ncQue.NcPeek(+1).B_26('X') == false)
							throw new Exception("wefwefqwef");
						if (ncQue.NcPeek(+1).B_26('Y') == false)
							throw new Exception("wefwefqwef");
					}

					// ＦコードをＤＢ条件に変更
					// （下に移動）
					//if (txtd.b_26['F'] == true)
					//	txtd.NcLine = RateSF(txtd.NcLine, "F", RateF);

					// 固定サイクル　モード
					if (txtd.G8 != 80) {
						;
					}
					// カスタムマクロ　モード
					else if (txtd.G6 != 67 || txtd.B_g6) {
						// Ｇ６５→Ｇ６６への変換と小数点の挿入
						if (txtd.G6 == 65) {

							// 小数点の挿入
							int cstt = -1;
							string tmpLine = "";
							while (true) {
								cstt = txtd.OutLine.Txt.IndexOfAny(ABC_G, cstt + 1);
								if (cstt < 0) {
									MessageBox.Show(
										"不正なG66の行が検出された", "NcOutput",
										MessageBoxButtons.OK, MessageBoxIcon.Error);
									Application.Exit();
									throw new Exception("");
								}
								if (txtd.OutLine.Txt[cstt] == 'P') continue;
								if (txtd.OutLine.Txt[cstt] == 'X') continue;
								if (txtd.OutLine.Txt[cstt] == 'Y') continue;

								double tmpd;
								while (cstt >= 0) {
									switch (txtd.G6p.ProgNo) {
									case 8755:
										tmpd = txtd.G6p[txtd.OutLine.Txt[cstt]].D;
										break;
									case 8401:
									case 8402:
										switch (txtd.OutLine.Txt[cstt]) {
										case 'E':
											tmpd = txtd.G6p[txtd.OutLine.Txt[cstt]].D / 1000;
											break;
										default:
											tmpd = txtd.G6p[txtd.OutLine.Txt[cstt]].D;
											break;
										}
										break;
									default:
										switch (txtd.OutLine.Txt[cstt]) {
										case 'D':
										case 'E':
										case 'H':
										case 'M':
										case 'S':
										case 'T':
											CamUtil.LogOut.CheckCount("NcOutput_Tebis 249", false,
												$"マクロプログラム {txtd.G6p.ProgName}の'{txtd.OutLine.Txt[cstt]}'の小数点位置をチェック願います。結果は藤本まで。");
											MessageBox.Show(
												$"マクロプログラム {txtd.G6p.ProgName}の'{txtd.OutLine.Txt[cstt]}'の小数点位置をチェック願います。結果は藤本まで。");
											break;
										}
										//if (txtd.NcLine[cstt] == 'F')
										//	tmpd = txtd.g6p[txtd.NcLine[cstt]].d * RateF;
										//else
										tmpd = txtd.G6p[txtd.OutLine.Txt[cstt]].D;
										break;
									}
									tmpLine += txtd.OutLine.Txt[cstt] + tmpd.ToString("0.0##");
									cstt = txtd.OutLine.Txt.IndexOfAny(ABC_G, cstt + 1);
								}
								break;
							}

							switch (txtd.G6p.ProgNo) {
							case 8755:  // 計測マクロ(8755)の場合 2009/01/16 MCC3016VG
								DataRow dRow = CamUtil.CamNcD.KijunSize.Tolerance((int)txtd.G6p['Q'].D);
								double dU = (double)dRow["寸法下限許容値_U"];
								double dV = (double)dRow["寸法上限許容値_V"];
								int aa = tmpLine.IndexOf('U');
								if (aa < 0)
									aa = tmpLine.Length;
								txtd.OutLine.Set(
									"G65" + txtd.G6p.ProgName +
									"X" + txtd.G6p['X'].D.ToString("0.0##") +
									"Y" + txtd.G6p['Y'].D.ToString("0.0##") +
									tmpLine.Substring(0, aa) +
									"U" + dU.ToString("0.0##") +
									"V" + dV.ToString("0.0##") +
									//"F" + txtd.g6p['F'].d.ToString("0000"); change in 2010/06/16
									"F" + txtd.G6p['F'].D.ToString("#"));
								// ///////////////////////////////////////////////////////////////////////////////
								// 不具合によるチェックの追加 2019/03/28
								// （測定データは連続していなければならない。初期Ｓは０、以降のＳは１）
								// （測定ポイント間の移動はマクロ内で実施する。ＮＣデータでは実施してはならない）
								// ///////////////////////////////////////////////////////////////////////////////
								{
									string sCode = StringCAM.GetNcCode(txtd.OutLine.Txt, 'S')[0];
									if ((int)Math.Round(Convert.ToDouble(sCode.Substring(1))) != 0) {
										if (ncQue.NcPeek(-1).OutLine.Txt.IndexOf("G65P8755") < 0)
											throw new Exception("測定データのＮＣフォーマットでエラーが発生しました。ＣＡＭからの出力方法をチェックしてください。");
									}
								}
								break;
							default:
								txtd.OutLine.Set("G66" + txtd.G6p.ProgName + tmpLine);
								//txtd.NcLine += ";\r\nX" + txtd.xyzsf['X'].micro.ToString() + "Y" + txtd.xyzsf['Y'].micro.ToString();
								txtd.OutLine.AtoAdd("X" + txtd.Xyzsf.Xi.ToString() + "Y" + txtd.Xyzsf.Yi.ToString());
								if (ncQue.NcPeek(+1).OutLine.Txt.IndexOf("G67") < 0) {
									//txtd.NcLine += ";\r\nG67";
									txtd.OutLine.AtoAdd("G67");
								}
								break;
							}
						}
						else if (txtd.G6 == 66) {
							MessageBox.Show(
								"不正なG66の行が検出された", "NcOutput",
								MessageBoxButtons.OK, MessageBoxIcon.Error);
							Application.Exit();
							throw new Exception("");
						}
					}
					// サブコールモード
					else if (txtd.SubPro.HasValue) {
						;
					}
					// 一般モード
					else {
						// Ｆコードのある行に"G01,2,3"を挿入
						if (txtd.B_26('F')) txtd.AddG123();

						// G41,G42の行にG17を追加
						if (txtd.B_g4 && txtd.G4 != 40) {
							txtd.OutLine.Set("G17" + txtd.OutLine.Txt);
						}
					}

					// Ｆコード未設定のチェック add in 2019/01/08
					if (txtd.B_g1 && txtd.G1 != 0 && txtd.Xyzsf.Fi == 0)
						throw new Exception("最初の送り速度が設定されていない");

					// Ｆコードを変更
					if (txtd.B_26('F') == true) {
						// "F999999"を"G00"に変換
						if (txtd.Xyzsf.Fi == 999999) {
							//if (ProgVersion.RTMVersion) throw new Exception("qjewfbqhewbfdhb");
							if (ncd.xmlD.PostProcessor.Id != CamUtil.PostProcessor.ID.CPC_DMU_OM) throw new Exception("qjerfbqreh");
							txtd.OutLine.Set(NcLineCode.NcDelChar(txtd.OutLine.Txt, 'F'));
							txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G01", "G00"));
						}
						// ＤＢ条件に変更
						else {
							bool integer = (txtd.OutLine.Txt.IndexOf("G65P") >= 0 || txtd.OutLine.Txt.IndexOf("G66P") >= 0) ? false : true;
							txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', feed => feed * RateF, integer));
						}
					}

					// 最後の"G00X0Y0"を削除
					if (txtd.OutLine.Txt == "G00X0Y0" && ncQue.NcPeek(1).OutLine.Txt == "M98P0006") {
						txtd.OutLine.Set("");
					}

					// 最後のリトラクトをチェック
					if (txtd.OutLine.Txt == "M98P0006") {
						int lno = -1;
						if (ncQue.NcPeek(-1).OutLine.Txt == "") lno = -2;
						// 移動方向のＸＹ成分vxyを求める
						Vector3 vxy = Vector3.Vvect(Vector3.vk, Vector3.Vvect(ncQue.NcPeek(lno).Xyzsf.Dist, Vector3.vk));
						if (ncQue.NcPeek(lno).G1 == 0 && vxy.Abs < 0.1) {; }
						else if (ncQue.NcPeek(lno).G1 == 1 && ncQue.NcPeek(lno).Xyzsf.Fi == 999999) {; }    // 同時５軸の場合
						else throw new Exception("加工終了時のクリアランスプレーンへの移動がありません。多くはクリアランスプレーン未設定が原因です。");
					}
				}
				if (txtd.OutLine.Txt.Length > 0)
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
			if (keishaNo != abcList.Length - 1)
				throw new Exception("ＣＳＶの軸回転コード数がＮＣデータのそれより多い");
			return passLength;
		}
	}
}
