using System;
using System.Collections.Generic;
using System.Text;

using CamUtil.LCode;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace NCSEND2.Output
{
	class NcOutput_WorkNC : NcOutput
	{

		// コンストラクタ
		public NcOutput_WorkNC(NcReader sr, NCINFO.NcInfoCam ncd) : base(sr, ncd) { }

		/// <summary>
		/// ＮＣデータ１工具分の出力
		/// </summary>
		/// <param name="tcnt">工具順の数</param>
		/// <param name="tfoo">ＮＣデータの出力先</param>
		/// <param name="aprzd">加工原点Ｚ</param>
		/// <returns>パスの移動距離</returns>
		protected override NcLineCode.NcDist NcConvertN(int tcnt, StreamWriter tfoo, double aprzd) {
			int jj;
			NcLineQue txtd;

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
			//  in out NCDATA			ADD
			//  1      %
			//    1                     G100T99S1000
			//    2						G00G90X0Y0
			//  2      G01G90ZzzFff
			//  3 3    G00XxxYyy
			//  4 4    XxxYyyZzz
			//  5 5    G01ZzzFff

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
				if (txtd.G9 == 91) throw new Exception("G91 が検出されました。");
				//txtd = ReadNcd_haisi();

				if (txtd.LnumT <= 0) {; }
				else if (txtd.LnumT == 1) {
					if (txtd.B_g100 != true) {
						MessageBox.Show(
							"NCDATA ERROR G100", "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
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
				else if (txtd.LnumT == 2) {; }
				else if (txtd.LnumT == 3) {
					txtd.OutLine.Set("");
				}
				else if (txtd.LnumT == 4) {
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G01", ""));
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G90", ""));
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G00", ""));
					if (txtd.OutLine.Txt.IndexOf("F") >= 0)
						txtd.OutLine.Set(txtd.OutLine.Txt.Substring(0, txtd.OutLine.Txt.IndexOf("F")));
				}
				else if (txtd.LnumT == 5) {
					string aaa = ncQue.NcPeek(+1).OutLine.Txt;
					if (aaa.IndexOf('Z') == 0)
						txtd.OutLine.Set("");
					else {
						if (txtd.B_26('X') && txtd.B_26('Y')) {
							if (txtd.Xyzsf.Dist.X != 0.0) throw new Exception("qwefbqfrbqhebfr");
							if (txtd.Xyzsf.Dist.Y != 0.0) throw new Exception("qwefbqfrbqhebfr");
							txtd.OutLine.Set(NcLineCode.NcDelChar(txtd.OutLine.Txt, 'X'));
							txtd.OutLine.Set(NcLineCode.NcDelChar(txtd.OutLine.Txt, 'Y'));
						}
						if (txtd.B_26('F') == true)
							txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));
					}
				}
				// 後ろから４行目
				else if (txtd.Lnumb == -4) {
					string aaa = ncQue.NcPeek(+1).OutLine.Txt;
					if (txtd.OutLine.Txt.Replace("G00", "").IndexOf('Z') == 0)
						if (aaa.Replace("G00", "").IndexOf('Z') == 0)
							txtd.OutLine.Set("");
				}
				// 後ろから３行目
				else if (txtd.Lnumb == -3) {
					txtd.OutLine.Set("G00Z" + (ncd.xmlD.OriginZ * 1000).ToString("0"));
					txtd.OutLine.AtoAdd("M98P0006");
				}
				else {
					// ＦコードをＤＢ条件に変更
					// （下に移動）
					//if (txtd.b_26['F'] == true)
					//	txtd.NcLine = RateSF(txtd.NcLine, "F", RateF);

					// 固定サイクル　モード
					if (txtd.G8 != 80) throw new Exception("dvadsvadfva");
					// カスタムマクロ　モード
					if (txtd.G6 != 67 || txtd.B_g6) throw new Exception("dvadsvadfva");

					// Ｆコードのある行に"G01,2,3"を挿入
					if (txtd.B_26('F')) txtd.AddG123();

					// G41,G42の行にG17を追加
					if (txtd.B_g4 && txtd.G4 != 40) txtd.OutLine.Set("G17" + txtd.OutLine.Txt);

					// ＦコードをＤＢ条件に変更
					if (txtd.B_26('F') == true) txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));
				}
				if (txtd.OutLine.Txt.Length > 0)
					if (txtd.OutLine.Txt.IndexOf("(") != 0) {
						for (int ii = 0; ii < txtd.OutLine.Count; ii++) {
							// 変換されたＮＣデータの出力
							tfoo.WriteLine(txtd.OutLine[ii]);
						}
					}
			}
			return new NcLineCode.NcDist();
		}
	}
}
