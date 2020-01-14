using System;
using System.Collections.Generic;
using System.Text;

using CamUtil.LCode;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace NCSEND2.Output
{
	class NcOutput_CAMTOOL : NcOutput
	{

		// コンストラクタ
		public NcOutput_CAMTOOL(NcReader sr, NCINFO.NcInfoCam ncd) : base(sr, ncd) { }

		/// <summary>
		/// ＮＣデータ１工具分の出力
		/// </summary>
		/// <param name="tcnt">工具順の数</param>
		/// <param name="tfoo">ＮＣデータの出力先</param>
		/// <param name="aprzd">加工原点Ｚ</param>
		/// <returns>パスの移動距離</returns>
		protected override NcLineCode.NcDist NcConvertN(int tcnt, StreamWriter tfoo, double aprzd) {
			NcLineQue txtd;

			// ＤＢ切削条件（回転数）
			if (ncd.NcdtInfo[tcnt].Spidle < 0.001)
				throw new Exception("回転数のデータがありません。");
			double RateS = ncd.sqlDB[tcnt].Spin / ncd.NcdtInfo[tcnt].Spidle;
			// ＤＢ切削条件（送り速度）
			if (ncd.NcdtInfo[tcnt].Feedrt < 0.001)
				throw new Exception("切削送り速度のデータがありません。");
			double RateF = ncd.sqlDB[tcnt].Feedrate / ncd.NcdtInfo[tcnt].Feedrt;

			// ＮＣデータの読み込みと変換
			//    1行(%)			なし
			//    2行(O0001)		なし
			// T1 3行(G100)		工具径の小数点削除、正式回転数に変換
			// T2 4行(G00G90X0Y0)	なし
			// T3 5行(XxxxYyyy)	"G00"を削除
			// T4 6行(Zzzz)		なし
			//    n行				1. "F999999"を早送り（"G00"）に変換
			//						2. "G00"後の"G01","G02","G03"で送り速度の指定がない場合追加（新機能）
			//						3. 送り速度に比率を掛ける（1000, 500, 800, 900 のみ）

			while (true) {
				if (ncQue.QueueMax == 0)
					break;
				if (ncQue.NcPeek(1).Tcnt > tcnt)
					break;

				//txtd = ReadNcd_haisi();
				txtd = ncQue.NextLine(sr);

				if (txtd.LnumT == 1) {
					if (txtd.B_g100 != true) {
						MessageBox.Show("NCDATA ERROR G100", "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
					if (txtd.OutLine.Txt.IndexOf('S') < 0) {
						MessageBox.Show("G100 ERROR ncd=" + txtd, "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
					// abc.WriteLine(ncd.ReadLine().Replace(".0S10000", "S" + tsetCAM.spin.ToString("0")));
					// Ｓコードの前の工具径の小数点を削除
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace(".0S", "S"));
					// ＳコードをＤＢ条件に変更
					txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'S', RateS));
					// Ｉコードを工具径のコード前に挿入
					txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'T', ncd.sqlDB[tcnt].toolsetTemp.ICode, false));
				}
				else if (txtd.LnumT == 2) {
					if (txtd.OutLine.Txt != "G00G90X0Y0")
						throw new Exception("EDIT ERROR aa in NcOutput_CAMTOOL " + txtd.OutLine.Txt);
				}
				else if (txtd.LnumT == 3) {
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G00", ""));
					if (txtd.OutLine.Txt.IndexOf("X") < 0 || txtd.OutLine.Txt.IndexOf("Y") < 0)
						throw new Exception("EDIT ERROR aa in NcOutput_CAMTOOL " + txtd.OutLine.Txt);
				}
				else if (txtd.LnumT == 4) {
					if (txtd.OutLine.Txt.IndexOf("Z") != 0)
						throw new Exception($"EDIT ERROR aa in NcOutput_CAMTOOL {Path.GetFileName(ncd.FullNameCam)} {txtd.OutLine.Txt}");
				}
				else {
					// 固定サイクル　モード
					if (txtd.G8 != 80)
						throw new Exception("dvadsvadfva");
					// カスタムマクロ　モード
					if (txtd.G6 != 67 || txtd.B_g6)
						throw new Exception("dvadsvadfva");

					// Ｆコードのある行に"G01,2,3"を挿入
					if (txtd.B_26('F')) txtd.AddG123();

					// ＦコードをＤＢ条件に変更
					if (txtd.B_26('F') == true) {
						switch (txtd.Xyzsf.Fi) {
						case 1000:
						case 500:
						case 800:
						case 900:
							txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));
							break;
						default:
							throw new Exception($"-- このＩＫＳのデータ{this.ncd.OutName}は取決めた規則（Ｆコードは 1000, 500, 800, 900 の４種類）に適合していません。 --{txtd.OutLine.Txt}");
						}
					}
					// G00後のG01,G02,G03のある行にＦコードを挿入
					else if (txtd.B_g1 && txtd.G1 != 0 && ncQue.NcPeek(-1).G1 == 0) {
						txtd.OutLine.Set(txtd.OutLine.Txt + "F" + txtd.Xyzsf.Fi.ToString());
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));
					}
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
