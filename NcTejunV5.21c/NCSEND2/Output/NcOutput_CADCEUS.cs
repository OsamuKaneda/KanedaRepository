using System;
using System.Collections.Generic;
using System.Text;

using CamUtil.LCode;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace NCSEND2.Output
{
	class NcOutput_CADCEUS : NcOutput
	{

		// コンストラクタ
		public NcOutput_CADCEUS(NcReader sr, NCINFO.NcInfoCam ncd) : base(sr, ncd) { }

		/// <summary>
		/// ＮＣデータ１工具分の出力
		/// </summary>
		/// <param name="tcnt">工具順の数</param>
		/// <param name="tfoo">ＮＣデータの出力先</param>
		/// <param name="aprzd">加工原点Ｚ</param>
		/// <returns>パスの移動距離</returns>
		protected override NcLineCode.NcDist NcConvertN(int tcnt, StreamWriter tfoo, double aprzd) {
			//ListViewItem lim = ncd.lim;
			int jj;
			NcLineQue txtd;

			// 出力ＮＣデータの情報。加工長を計算するために使用
			NcLineCode outCode = new NcLineCode(new double[] { aprzd }, CamUtil.BaseNcForm.GENERAL, post, false, false);
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
			//  1   %
			//    1
			//    2
			//  2   G01G90ZzzFff
			//  3 3 G00XxxYyy
			//  4 4 Zzz
			//  5 5 G01ZzzFff

			//while (NcPeek(0).tcnt == tcnt) {
			while (true) {
				if (ncQue.QueueMax == 0)
					break;
				if (ncQue.NcPeek(1).Tcnt > tcnt)
					break;

				//txtd = ReadNcd_haisi();
				txtd = ncQue.NextLine(sr);

				if (txtd.LnumT == 1) {
					if (txtd.B_g100 != true) {
						MessageBox.Show(
							"NCDATA ERROR G100", "NcOutput",
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

					// Ｓコードを挿入（出力条件）
					txtd.OutLine.Set(txtd.OutLine.Txt.Substring(0, txtd.OutLine.Txt.IndexOf("S")) +
						"S" + ncd.NcdtInfo[tcnt].Spidle.ToString("0"));
					// ＳコードをＤＢ条件に変更
					txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'S', RateS));
					// ＩコードをＳの前に挿入
					txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'S', ncd.sqlDB[tcnt].toolsetTemp.ICode, true));
					// 工具種類・直径情報コードをＳの前に挿入
					txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'S', ncd.sqlDB[tcnt].toolsetTemp.DCode, true));
				}
				else if (txtd.LnumT == 2) {
					if (txtd.OutLine.Txt != "G00G90X0Y0")
						throw new Exception("EDIT ERROR aa in CADCEUS.nc" + txtd.OutLine.Txt);
				}
				else if (txtd.LnumT == 3) {
					if (txtd.OutLine.Txt.Substring(0, 6) != "G00G90")
						throw new Exception("sasasxasxasxasxasx");
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G00G90", ""));
					if (txtd.OutLine.Txt.IndexOf("X") < 0)
						txtd.OutLine.Set("X0" + txtd.OutLine.Txt);
					if (txtd.OutLine.Txt.IndexOf("Y") < 0)
						txtd.OutLine.Set(txtd.OutLine.Txt + "Y0");
				}
				else if (txtd.LnumT == 4) {
					if (ncQue.NcPeek(+1).OutLine.Txt.IndexOf("Z") == 0)
						txtd.OutLine.Set("");
				}
				// 後ろから４行目
				else if (txtd.Lnumb == -4) {
					Regex re = new Regex("^[^Z]*$");
					if (re.IsMatch(txtd.OutLine.Txt)) txtd.OutLine.Set("");
				}
				else {
					// 固定サイクル　モード
					if (txtd.G8 != 80) {
						throw new Exception("dvadsvadfva");
					}
					// カスタムマクロ　モード
					else if (txtd.G6 != 67 || txtd.B_g6) {
						throw new Exception("dvadsvadfva");
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
					// ＦコードをＤＢ条件に変更
					if (txtd.B_26('F') == true)
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));
				}
				if (txtd.OutLine.Txt.Length > 0)
					if (txtd.OutLine.Txt.IndexOf("(") != 0) {
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
								throw new Exception($"{ss}加工長計算でエラーが発生しました。\n{ex.Message}");
							}
						}
					}
			}
			return passLength;
		}
	}
}
