using System;
using System.Collections.Generic;
using System.Text;

using CamUtil.LCode;
using System.IO;
using System.Windows.Forms;

namespace NCSEND2.Output
{
	class NcOutput_Tebis_BU : NcOutput
	{
		// コンストラクタ
		public NcOutput_Tebis_BU(NcReader sr, NCINFO.NcInfoCam ncd) : base(sr, ncd) { }

		/// <summary>
		/// ＮＣデータ１工具分の出力
		/// </summary>
		/// <param name="tcnt">工具順の数</param>
		/// <param name="tfoo">ＮＣデータの出力先</param>
		/// <param name="aprzd">加工原点Ｚ</param>
		/// <returns>パスの移動距離</returns>
		protected override NcLineCode.NcDist NcConvertN(int tcnt, StreamWriter tfoo, double aprzd) {
			NcLineCode txtd;
			string[] tline = new string[5];
			short ladd;

			bool jikA = false;	// Ａ軸補正する場合はtrue
			List<CamUtil.Vector3> toucPoi = new List<CamUtil.Vector3>();
			List<CamUtil.Vector3> normVec = new List<CamUtil.Vector3>();

			// 出力ＮＣデータの情報。加工長を計算するために使用
			NcLineCode outCode = new NcLineCode(new double[] { aprzd }, CamUtil.BaseNcForm.BUHIN, post, false, false);
			outCode.NextLine("%");
			NcLineCode.NcDist passLength =
				new NcLineCode.NcDist(ncd.sqlDB[tcnt].Feedrate, ncd.xmlD[tcnt].MachiningAxisList);

			// 「加工精度」のチェック
			switch (ncd.sqlDB[tcnt].Accuracy) {
			case "NON_MODE":
				if (ncd.xmlD[tcnt].KCLSS != null)
					MessageBox.Show($"加工精度の値が異なる。TOOL={ncd.sqlDB[tcnt].tscamName}\n    DB={ncd.sqlDB[tcnt].Accuracy}  POST={ncd.xmlD[tcnt].KCLSS}");
				break;
			default:
				if (ncd.xmlD[tcnt].KCLSS != ncd.sqlDB[tcnt].Accuracy)
					MessageBox.Show($"加工精度の値が異なる。TOOL={ncd.sqlDB[tcnt].tscamName}\n    DB={ncd.sqlDB[tcnt].Accuracy}  POST={ncd.xmlD[tcnt].KCLSS}");
				break;
			}

			// ＤＢ切削条件（回転数）
			double RateS;
			if (ncd.NcdtInfo[tcnt].Spidle < 0.001) {
				if (ncd.sqlDB[tcnt].toolsetTemp.Probe) RateS = 1.0;
				else throw new Exception("SPINDL 0");
			}
			else
				RateS = ncd.sqlDB[tcnt].Spin / ncd.NcdtInfo[tcnt].Spidle;

			// ＤＢ切削条件（送り速度）
			double RateF;
			if (ncd.sqlDB[tcnt].tscamName == "MP700_B")
				RateF = 1.0;
			else {
				if (ncd.NcdtInfo[tcnt].Feedrt < 0.001) {
					MessageBox.Show("切削送り速度のデータがありません。",
						"SET KDATA",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					throw new Exception("FEEDRATE 0");
				}
				RateF = ncd.sqlDB[tcnt].Feedrate / ncd.NcdtInfo[tcnt].Feedrt;
			}

			// /////////////////////////
			// 試行のためのコード start
			// /////////////////////////
			/*
			if (Program.debug) {
				if (Math.Abs(ncd.sqlDB[tcnt].spin - ncd.NcdtInfo[tcnt].Spidle) > 0.1 || Math.Abs(ncd.sqlDB[tcnt].feedrate - ncd.NcdtInfo[tcnt].Feedrt) > 0.1) {
					MessageBox.Show("回転数、送り速度は試行のため変更しません " +
						Path.GetFileName(ncd.FullNameCam) + "   tcnt=" + tcnt + "   tCam=" + ncd.sqlDB[tcnt].tscamName + "\n" +
						ncd.NcdtInfo[tcnt].Spidle.ToString("00000") + " -> " + ncd.sqlDB[tcnt].spin.ToString("00000") + "   " +
						ncd.NcdtInfo[tcnt].Feedrt.ToString("0000") + " -> " + ncd.sqlDB[tcnt].feedrate.ToString("0000"));
					RateS = 1.0;
					RateF = 1.0;
				}
			}
			*/
			// /////////////////////////
			// 試行のためのコード end
			// /////////////////////////

			ladd = 0;
			while (true) {
				if (ncQue.QueueMax == 0)
					break;

				// //////////////////////////////////////////////////////////////////////////////
				// ncQue.NextLine(sr) を使う場合はここで
				// ncQue.NcPeek(1).tcnt != tcnt を ncQue.NcPeek(1).tcnt > tcnt に変更しておく
				// //////////////////////////////////////////////////////////////////////////////
				if (ncQue.NcPeek(1).Tcnt > tcnt)
					break;

				txtd = ncQue.NextLine(sr);
				//txtd = ReadNcd();

				// =======================================================
				// Ｄ５００導入に伴う追加 2013/10/21
				// =======================================================

				// G102(高速高精度加工モード設定)をマクロ化しクーラントの後へ移動
				// 但し穴あけ加工では、不要でありまた加工のマクロでエラーとなるので実行しない
				// 実行するように変更。リニアムのヘリカルマクロはNcTejunで元のコードに戻す 2015/01/19
				if (txtd.OutLine.Txt.IndexOf("G102") == 0) {
					//if (this.ncd.xmlD[tcnt].KTYPE == "MFeat")
					//	txtd.outLine.Set("");
					//else {
					if (ncQue[+2].OutLine.Txt == "M08" || ncQue[+2].OutLine.Txt == "M12")
						ncQue[+2].OutLine.AtoAdd("G65P8102" + txtd.OutLine.Txt.Substring(4));
					else
						ncQue[+2].OutLine.MaeAdd("G65P8102" + txtd.OutLine.Txt.Substring(4));
					txtd.OutLine.Set("");
					//}
				}

				// クーラントのマクロ化
				else if (txtd.OutLine.Txt == "M08" || txtd.OutLine.Txt == "M12")
					txtd.OutLine.Set("M98P8103");

				// クリアランスプレーンへの移動のマクロ化
				else if (txtd.OutLine.Txt == "G00G90Z420000")
					txtd.OutLine.Set("M98P8101");
				else if (txtd.OutLine.Txt == "Z420000")
					txtd.OutLine.Set("M98P8101");

				// ヘリカル補間、円錐補間のマクロ化
				else if (txtd.OutLine.Txt.IndexOf("G02") == 0 && txtd.OutLine.Txt.IndexOf("Z") >= 0 && txtd.OutLine.Txt.IndexOf("P") >= 0) {
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("P", "H"));
					txtd.OutLine.Set("G65P8104D" + txtd.OutLine.Txt.Substring(1));
				}
				else if (txtd.OutLine.Txt.IndexOf("G03") == 0 && txtd.OutLine.Txt.IndexOf("Z") >= 0 && txtd.OutLine.Txt.IndexOf("P") >= 0) {
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("P", "H"));
					txtd.OutLine.Set("G65P8104D" + txtd.OutLine.Txt.Substring(1));
				}

				// リジッドタップ加工のマクロ化
				else if (txtd.OutLine.Txt.IndexOf("G95G84") == 0)
					txtd.OutLine.Set("G65P8105" + txtd.OutLine.Txt.Substring(6));

				// その他のＰコードは禁止
				else if (txtd.OutLine.Txt.IndexOf("P") >= 0 && txtd.OutLine.Txt.IndexOf("(") != 0
					&& txtd.OutLine.Txt.IndexOf("G65") != 0 && txtd.OutLine.Txt.IndexOf("M98") != 0
					&& txtd.OutLine.Txt.IndexOf("G82") != 0 && txtd.OutLine.Txt.IndexOf("G84") != 0
					&& txtd.OutLine.Txt.IndexOf("G86") != 0) {
					throw new Exception("プログラムエラー155 不明の'P'コードが出力された :" + txtd);
				}

				// 数値指定によるクリアランスプレーンへの移動は禁止
				else if (txtd.OutLine.Txt.IndexOf("Z420") >= 0) {
					if (txtd.OutLine.Txt.IndexOf("Z420000") >= 0)
						throw new Exception("プログラムエラー106:" + txtd.OutLine.Txt);
					if (txtd.OutLine.Txt.IndexOf("Z420.") >= 0)
						throw new Exception("プログラムエラー106:" + txtd.OutLine.Txt);
				}
				else {
					if (this.ncd.xmlD[tcnt].KTYPE == "MFeat" && txtd.LnumT == 4)
						ladd = 1;
				}
				// =======================================================
				// =======================================================

				// Ｇ１００の処理(G100T01)
				if (txtd.LnumT == 1) {
					if (txtd.B_g100 != true) {
						MessageBox.Show(
							"NCDATA ERROR lnum=1", "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}

					// 工具測定記号をチェック
					if (txtd.NumList[2].ncChar != ncd.sqlDB[tcnt].toolsetTemp.MeasType5AXIS) {
						// ポストでは工具先頭文字が'E'の場合のみ測定識別記号を'F'としているが、
						// 工具先頭文字が'F'の場合もフラットエンドミルであり測定識別記号を'F'とする
						if (txtd.NumList[2].ncChar == 'E' && ncd.sqlDB[tcnt].toolsetTemp.ToolName[0] == 'F')
							txtd.OutLine.Set(txtd.OutLine.Txt.Replace('E', 'F'));
						else
							throw new Exception(
								$"工具{ncd.sqlDB[tcnt].toolsetTemp.ToolFormName} 測定識別記号がＤＢとポストで異なっている。" +
								$" DB={ncd.sqlDB[tcnt].toolsetTemp.MeasType5AXIS.ToString()}   {txtd.OutLine.Txt}");
					}

					// ＳコードをＤＢ条件に変更
					if (RateS != 1.0)
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'S', RateS));

					// ======================================================
					// Ｄ５００導入に伴う変更 2014/04/03
					// G100にクーラントコードを追加
					// =======================================================
					string ww = ncd.sqlDB[tcnt].coolant.CoolCode_BuhinNC();
					if (ww.Length > 0) {
						txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'M', ww, true));
					}
				}
				// リジッドタップ加工のＤＢ条件変更
				else if (txtd.OutLine.Txt.IndexOf("G65P8105") == 0) {
					// ＳコードをＤＢ条件に変更
					if (RateS != 1.0) {
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'S', spin => spin * RateS, false));
					}
					// Ｆコードを毎回転送りに変更
					double rtmp = ncd.sqlDB[tcnt].Feedrate / ncd.sqlDB[tcnt].Spin;
					txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'F', rtmp.ToString("0.0###")));
				}
				else {
					// チェック２行～７行
					Check2_7((int)(txtd.LnumT + ladd), txtd.OutLine.Txt);

					// ＦコードをＤＢ条件に変更
					if (txtd.B_26('F') == true && txtd.Xyzsf.Fi != NcLineCode.RF_5AXIS)
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));

					// Ｆコード未設定のチェック
					if (txtd.B_g1 && txtd.G1 != 0 && txtd.Xyzsf.Fi == 0)
						throw new Exception("最初の送り速度が設定されていない");

					// 前計測の場合、座標値から材料寸法を得る 2019/04/08 change in 2019/05/22
					if (ncd.xmlD.PostProcessor.Id == CamUtil.PostProcessor.ID.MES_BEF_BU && txtd.B_g6) {
						if (txtd.G6p.ProgNo == 9345 || txtd.G6p.ProgNo == 9346) {
							if (txtd.Code('T').L != toucPoi.Count) throw new Exception("qerfbqherfb");
							toucPoi.Add(txtd.XYZ);
							normVec.Add(txtd.IJK);
							// Ａ軸補正の測定はＴ番号に１を加えて４、５とする
							switch (txtd.Code('T').L) {
							case 0: // 材料上面の測定（材料違いの検証のみ）
							case 1: // ＸＹ平面平行だし１点目でＸＹ基準１と共用
							case 2: // ＸＹ平面平行だし２点目
								break;
							default:
								if (txtd.Code('T').L == 3 && txtd.IJK == CamUtil.Vector3.vk) jikA = true;	// Ａ軸平行出しをする
								if (jikA && txtd.Code('T').L <= 5) {
									switch (txtd.Code('T').L) {
									case 3: // Ａ軸平行だし１点目でＺ基準と共用(T4)
										txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'T', 4.ToString())); break;
									case 4: // Ａ軸平行だし２点目(T5)
										txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'T', 5.ToString())); break;
									case 5: // ＸＹ基準２(T3)
										txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'T', 3.ToString())); break;
									}
								}
								else {
									switch (txtd.Code('T').L) {
									case 3: // ＸＹ基準２(T3)
										//txtd.OutLine.Set(NcLineCode.NcSetValue2(txtd.OutLine.Txt, 'T', 3.ToString()));
										break;
									default:    // 材料寸法を得るための反基面側の測定
										txtd.OutLine.Set(""); break;
									}
								}
								break;
							}
						}
					}
				}

				// 工具名称でポスト設定される値のチェック
				ToolNameCheck(txtd, tcnt);

				txtd.OutLine.CommOut = true;
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
			// 前計測の場合、座標値から材料寸法を得る 2019/04/08 change in 2019/05/22
			if (ncd.xmlD.PostProcessor.Id == CamUtil.PostProcessor.ID.MES_BEF_BU) {
				CamUtil.Vector3 size;
				if (ncd.xmlD.MHEAD == "d500") {
					if (jikA) CamUtil.LogOut.CheckCount("NcOutput_Tebis_BU 299", false, "Ａ軸補正 " + Path.GetFileName(ncd.FullNameCam));
					if (CamUtil.Vector3.Vscal(normVec[1], CamUtil.Vector3.vi) != 0) CamUtil.LogOut.CheckCount("NcOutput_Tebis_BU 300", false, "Ｃ軸補正におけるＸ軸方向測定 " + Path.GetFileName(ncd.FullNameCam));
				}
				else {
					if (jikA) throw new Exception("Ａ軸補正に対応しているのはＤ５００加工機のみです");
					if (CamUtil.Vector3.Vscal(normVec[1], CamUtil.Vector3.vi) != 0) throw new Exception("Ｃ軸補正におけるＸ軸方向測定に対応しているのはＤ５００加工機のみです");
				}
				if (normVec[0] != CamUtil.Vector3.vk)
					throw new Exception($"前測定の法線ベクトルエラー T0={normVec[0].ToString()}");
				if (CamUtil.Vector3.Vscal(normVec[1], normVec[0]) != 0.0 || normVec[1] != normVec[2])
					throw new Exception($"前測定の法線ベクトルエラー T1={normVec[1].ToString()}, T2={normVec[2].ToString()}");
				if (jikA) {
					if (normVec[3] != CamUtil.Vector3.vk)
						throw new Exception($"前測定の法線ベクトルエラー T4={normVec[3].ToString()}");
					if (normVec[4] != CamUtil.Vector3.vk)
						throw new Exception($"前測定の法線ベクトルエラー T5={normVec[4].ToString()}");
					if (CamUtil.Vector3.Vscal(normVec[5], normVec[0]) != 0.0 || CamUtil.Vector3.Vscal(normVec[5], normVec[1]) != 0.0)
						throw new Exception($"前測定の法線ベクトルエラー T3={normVec[5].ToString()}");
					if (normVec.Count == 6) {; }
					else if (normVec.Count == 8) {
						// 反基面側測定ベクトルのチェック
						if (normVec[6] != -normVec[1] && normVec[6] != -normVec[5]) throw new Exception("前測定の反基面側測定ベクトルエラー");
						if (normVec[7] != -normVec[1] && normVec[7] != -normVec[5]) throw new Exception("前測定の反基面側測定ベクトルエラー");
						if (CamUtil.Vector3.Vscal(normVec[6], normVec[7]) != 0.0) throw new Exception("前測定の反基面側測定ベクトルエラー");
					}
					else throw new Exception("前測定の測定点数エラー");
				}
				else {
					if (CamUtil.Vector3.Vscal(normVec[3], normVec[0]) != 0.0 || CamUtil.Vector3.Vscal(normVec[3], normVec[1]) != 0.0)
						throw new Exception("前測定の法線ベクトルエラー T3=" + normVec[3].ToString());
					if (normVec.Count == 4) {; }
					else if (normVec.Count == 6) {
						// 反基面側測定ベクトルのチェック
						if (normVec[4] != -normVec[1] && normVec[4] != -normVec[3]) throw new Exception("前測定の反基面側測定ベクトルエラー。測定順序が正しいか確認してください。");
						if (normVec[5] != -normVec[1] && normVec[5] != -normVec[3]) throw new Exception("前測定の反基面側測定ベクトルエラー");
						if (CamUtil.Vector3.Vscal(normVec[4], normVec[5]) != 0.0) throw new Exception("前測定の反基面側測定ベクトルエラー");
					}
					else throw new Exception("前測定の測定点数エラー");
				}
				CamUtil.Vector3 matSizeMin = toucPoi[0];
				CamUtil.Vector3 matSizeMax = toucPoi[0];
				for (int ii = 1; ii < toucPoi.Count; ii++) {
					if (normVec[ii] == CamUtil.Vector3.vk) continue;
					matSizeMin = CamUtil.Vector3.Min(matSizeMin, toucPoi[ii]);
					matSizeMax = CamUtil.Vector3.Max(matSizeMax, toucPoi[ii]);
				}

				if (toucPoi.Count - (jikA ? 2 : 0) == 4) {
					// 反基面側測定なし 片側測定値の２倍の値とする
					size = new CamUtil.Vector3(
						2.0 * Math.Max(Math.Abs(matSizeMax.X), Math.Abs(matSizeMin.X)),
						2.0 * Math.Max(Math.Abs(matSizeMax.Y), Math.Abs(matSizeMin.Y)),
						matSizeMax.Z);
				}
				else
				if (toucPoi.Count - (jikA ? 2 : 0) == 6) {
					// 反基面側測定あり 測定値の最大値と最小値の差とする
					size = new CamUtil.Vector3(
						matSizeMax.X - matSizeMin.X,
						matSizeMax.Y - matSizeMin.Y,
						matSizeMax.Z);
				}
				else throw new Exception("前測定の法線ベクトルエラー。測定順序が正しいか確認してください。");
				// 材料寸法のＸＭＬ保存
				ncd.XmlInsertMaterialSize(size);
			}

			return passLength;
		}

		/// <summary>
		/// 工具名称でポスト設定される値のチェック
		/// </summary>
		/// <param name="txtd"></param>
		/// <param name="tcnt"></param>
		private void ToolNameCheck(NcLineCode txtd, int tcnt) {
			if (txtd.B_g100) {
				if (ncd.xmlD[tcnt].TTYPE == "Flat cutter") {
					if (
					ncd.xmlD[tcnt].SNAME.Substring(0, 1) == "E" ||
					ncd.xmlD[tcnt].SNAME.Substring(0, 1) == "F"
					) {
						if (txtd.OutLine.Txt.IndexOf('F') < 0)
							throw new Exception("qwefqbefrbqher");
					}
					else {
						if (txtd.OutLine.Txt.IndexOf('E') < 0)
							throw new Exception("qwefqbefrbqher");
					}
				}
			}

			else if (ncd.xmlD[tcnt].SNAME == "TS600SH") {
				;
			}
			else if (txtd.OutLine.Txt.IndexOf("G65P8104D") == 0) {
				CamUtil.Vector3 sttp = txtd.Xyzsf.PreToXYZ() - txtd.Center();
				CamUtil.Vector3 endp = (txtd.Xyzsf.ToXYZ() - txtd.Center()).Projection(CamUtil.Vector3.vk);
				// 始終点の回転中心からの距離がほぼ同じなので円筒（ヘリカル）となる
				if (Math.Abs(sttp.Abs - endp.Abs) < 0.002) {
					;
				}
				// 始終点の回転中心からの距離が異なるので円錐となる
				else {
					if (txtd.Code('P').L != 1) throw new Exception("edbqwehb");

					double[] PP = new double[6];
					switch (ncd.sqlDB[tcnt].tscamName.Substring(0, 6)) {
					case "TP1X8H":		// 1/8 D=8.80
						PP[1] = 8.5;					// 基準孔深さ
						PP[2] = 25.4 / 28.0;			// ピッチ
						PP[3] = 8.6;					// 下孔径
						PP[4] = 7.5 - 9.1 / 16;		// 工具先端径
						PP[5] = 9.197 + 0.150 * 2;	// 基準孔径
						break;
					case "TP1X4H":		// 1/4 D=11.65
						PP[1] = 12.0;				// 基準孔深さ
						PP[2] = 25.4 / 19.0;			// ピッチ
						PP[3] = 11.6;				// 下孔径
						PP[4] = 10.0 - 14.7 / 16;	// 工具先端径
						PP[5] = 12.407 + 0.187 * 2;	// 基準孔径
						break;
					case "TP3X8H":		// 3/8 D=15.55
						PP[1] = 13.0;				// 基準孔深さ
						PP[2] = 25.4 / 19.0;			// ピッチ
						PP[3] = 15.5;				// 下孔径
						PP[4] = 10.0 - 14.7 / 16;	// 工具先端径
						PP[5] = 15.85 + 0.215 * 2;	// 基準孔径
						break;
					case "TP1X2H":		// 1/2 D=18.83
						PP[1] = 16.0;				// 基準孔深さ
						PP[2] = 25.4 / 14.0;			// ピッチ
						PP[3] = 18.5;				// 下孔径
						PP[4] = 12.0 - 20.0 / 16;	// 工具先端径
						PP[5] = 19.955 + 0.216 * 2;	// 基準孔径
						break;
					case "TP3X4H":		// 3/4 D=24.55
						PP[1] = 18.0;				// 基準孔深さ
						PP[2] = 25.4 / 14.0;			// ピッチ
						PP[3] = 24.5;				// 下孔径
						PP[4] = 12.0 - 20.0 / 16;	// 工具先端径
						PP[5] = 25.316 + 0.303 * 2;	// 基準孔径
						break;
					case "TP1X1H":		// 1/1 D=?????
						PP[1] = 20.0;				// 基準孔深さ
						PP[2] = 25.4 / 11.0;			// ピッチ
						PP[3] = 30.5;				// 下孔径
						PP[4] = 20.0 - 27.7 / 16;	// 工具先端径
						PP[5] = 31.999 + 0.278 * 2;	// 基準孔径
						break;
					default:
						throw new Exception("PT-TAP ERROR");
					}
					double cld = (PP[5] - PP[2] / 16 - PP[4]) / 2;
					if (Math.Abs(cld - txtd.Radius()) > 0.001)
						throw new Exception("	efdbhew	fdbh");
				}
			}
		}

		private void Check2_7(int lineNo,　string txt) {
			switch (lineNo) {
			case 2:						// 工具名をマクロ変数に入力(G65P9376)
				if (txt.IndexOf("G65P9376") != 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam)); break;
			case 3:						// 工具軸設定処理(G65P8700A_B_)
				if (txt.IndexOf("G65P8700") != 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam)); break;
			case 4:
				switch (ncd.xmlD.PostProcessor.Id) {
				case CamUtil.PostProcessor.ID.GEN_BU:			// 加工モードの設定(G65P8102)
				case CamUtil.PostProcessor.ID.CPC_D500_BU:		// 加工モードの設定(G65P8102)
					if (txt.Length > 0 && txt.IndexOf("G65P8102") != 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam)); break;
				case CamUtil.PostProcessor.ID.MES_BEF_BU:
					if (txt.IndexOf("M98P8101") != 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam)); break;
				case CamUtil.PostProcessor.ID.MES_AFT_BU:
					if (txt.IndexOf("G00G90X0Y0") != 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam)); break;
				}
				break;
			case 5:
				switch (ncd.xmlD.PostProcessor.Id) {
				case CamUtil.PostProcessor.ID.GEN_BU:			// 加工点上空へ移動(X_Y_)
				case CamUtil.PostProcessor.ID.CPC_D500_BU:		// 加工点上空へ移動(X_Y_)
				case CamUtil.PostProcessor.ID.MES_AFT_BU:
					if (txt.IndexOf("X") < 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam));
					if (txt.IndexOf("Y") < 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam));
					break;
				case CamUtil.PostProcessor.ID.MES_BEF_BU:
					if (txt.IndexOf("G65P9345") != 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam)); break;
				}
				break;
			case 6:
				switch (ncd.xmlD.PostProcessor.Id) {
				case CamUtil.PostProcessor.ID.GEN_BU:			// クーラントの処理(M98P8103)
				case CamUtil.PostProcessor.ID.CPC_D500_BU:		// クーラントの処理(M98P8103)
					if (txt.IndexOf("M98P8103") != 0) throw new Exception("クーラントのコードが見つかりませんでした in " + Path.GetFileName(ncd.FullNameCam)); break;
				case CamUtil.PostProcessor.ID.MES_BEF_BU:
					if (txt.IndexOf("G65P9346") != 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam)); break;
				case CamUtil.PostProcessor.ID.MES_AFT_BU:	// 加工点近くへアプローチ(Z_)
					if (txt.IndexOf("Z") < 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam)); break;
				}
				break;
			case 7:
				switch (ncd.xmlD.PostProcessor.Id) {
				case CamUtil.PostProcessor.ID.GEN_BU:			// 加工点近くへアプローチ(Z_)
				case CamUtil.PostProcessor.ID.CPC_D500_BU:		// 加工点近くへアプローチ(Z_)
					if (txt.IndexOf("Z") < 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam)); break;
				case CamUtil.PostProcessor.ID.MES_AFT_BU:	// 測定開始処理(G65P8750)
					if (txt.IndexOf("G65P8750") < 0) throw new Exception("ＮＣフォーマット異常 in " + Path.GetFileName(ncd.FullNameCam)); break;
				}
				break;
			}
		}
	}
}
