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
	class NcOutput_WorkNC_5AXIS : NcOutput
	{

		// コンストラクタ
		public NcOutput_WorkNC_5AXIS(NcReader sr, NCINFO.NcInfoCam ncd) : base(sr, ncd) { }

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
			//  in out	NCDATA				ADD
			//   1  0	%					=
			//      0						O0001(by NcReader)
			//      1						G100T99Sss(by NcReader)
			//      2						G00G90X0Y0(by NcReader)
			//   2	3	Aaa1 Ccc1			(check only)
			//   3  4	G00 Xx1 Yy1			Xx1 Yy1
			//   4	5	Zclr				(check only)
			//   5  6	Xx1 Yy1				(移動無し??)
			//   6  7	Xx1 Yy1 Zz1			Zzz
			//   7  8	Xx1 Yy1 Zz2			Zzz				！！この行は傾斜加工では存在しない。非傾斜は未調査！！
			//   8  9	Xx2 Yy2 Zz3			=(工具軸で移動?)
			//   9 10	G01 Xx3 Yy3 Zz4 Ff1	=(アプローチ)
			//  10 11	G01 Xx3 Yy3 Zz4 Ff2	=(切削)

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

				if (txtd.LnumT <= 0) { ;}
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
					// 加工軸のデータ内変更はまだ未対応
					if (ncd.xmlD[tcnt].AxisCount != 1)
						throw new Exception("qfqbfrhqr");

					// ＳコードをＤＢ条件に変更
					txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'S', RateS));
					// ＩコードをＳの前に挿入
					txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'S', ncd.sqlDB[tcnt].toolsetTemp.ICode, true));
					// 工具種類・直径情報コードをＳの前に挿入
					txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'S', ncd.sqlDB[tcnt].toolsetTemp.DCode, true));
				}
				else if (txtd.LnumT == 2) { ;}
				else if (txtd.LnumT == 3) {
					// Aaa Ccc
					if (Math.Abs(txtd.Xyzsf.A - ncd.xmlD[tcnt].MachiningAxisList[0].DegA) > 0.01 || Math.Abs(txtd.Xyzsf.C - ncd.xmlD[tcnt].MachiningAxisList[0].DegC) > 0.01)
						throw new Exception($"ＸＭＬとＮＣの傾斜角度が異なる ＸＭＬ＝{ncd.xmlD[tcnt].MachiningAxisList[0].ToString()}ＮＣ＝{txtd.NcLine}");
					txtd.OutLine.Set("");
				}
				else if (txtd.LnumT == 4) {
					// G00 Xxx Yyy ６行目の座標値を元に決定する
					if (txtd.G1 != 0) throw new Exception("ＮＣデータ３行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					if (txtd.B_26('X') != true) throw new Exception("ＮＣデータ３行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					if (txtd.B_26('Y') != true) throw new Exception("ＮＣデータ３行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					if (txtd.B_26('Z') == true) throw new Exception("ＮＣデータ３行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					if (txtd.G1 != 0) throw new Exception("ＮＣデータ６行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					if (ncQue[+3].B_26('X') != true) throw new Exception("ＮＣデータ６行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					if (ncQue[+3].B_26('Y') != true) throw new Exception("ＮＣデータ６行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					if (ncQue[+3].B_26('Z') != true) throw new Exception("ＮＣデータ６行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					if (Math.Abs(txtd.Xyzsf.X - ncQue[+3].Xyzsf.X) > 0.0015 || Math.Abs(txtd.Xyzsf.Y - ncQue[+3].Xyzsf.Y) > 0.0015)
						throw new Exception($"ＮＣデータ３行目と６行目のＸＹ座標が異なる ＮＣ＝{txtd.NcLine}  {ncQue[+3].NcLine}");
					txtd.OutLine.Set("X" + ncQue[+3].Code('X').S + "Y" + ncQue[+3].Code('Y').S);
				}
				else if (txtd.LnumT == 5) {
					// Zclr クリアランス
					if (txtd.G1 != 0) throw new Exception("ＮＣデータ４行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					//if (Math.Abs(txtd.xyzsf.Z - ncd.xmlD[tcnt].ClrPlaneList[0]) > 0.01) throw new Exception("ＮＣデータ４行目のフォーマットが異常 ＮＣ＝" + txtd.ncLine);
					txtd.OutLine.Set("");
				}
				else if (txtd.LnumT == 6) {
					// Xxx Yyy Zzz 移動なし
					if (txtd.G1 != 0) throw new Exception("ＮＣデータ５行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					if (txtd.Xyzsf.Dist.Abs > 0) throw new Exception("ＮＣデータ５行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					txtd.OutLine.Set("");
				}
				else if (txtd.LnumT == 7) {
					// Xxx Yyy Zzz Ｚ軸方向の移動
					txtd.OutLine.Set(NcLineCode.NcDelChar(txtd.OutLine.Txt, 'X'));
					txtd.OutLine.Set(NcLineCode.NcDelChar(txtd.OutLine.Txt, 'Y'));
				}
				else if (txtd.LnumT == 8 && ncd.xmlD[tcnt].Keisha) {
					// Xxx Yyy Zzz 工具軸方向の移動
					if (txtd.G1 != 0) throw new Exception("ＮＣデータ７行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
				}
				else if (txtd.LnumT == 8 || txtd.LnumT == 9) {
					// G01 Xxx Yyy Zzz 切削
					if (txtd.G1 == 0) throw new Exception("ＮＣデータ７-８行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
				}
				// 後ろから４行目
				else if (txtd.Lnumb == -4) {
					if ((ncQue.NcPeek(+1).Xyzsf.Z - txtd.Xyzsf.Z) > 0.0 && ncQue.NcPeek(+1).Xyzsf.Dist.X == 0.0 && ncQue.NcPeek(+1).Xyzsf.Dist.Y == 0.0)
						txtd.OutLine.Set("");
				}
				// 後ろから３行目
				else if (txtd.Lnumb == -3) {
					if (txtd.Xyzsf.Dist.X != 0.0 || txtd.Xyzsf.Dist.Y != 0.0)
						throw new Exception("ＮＣデータ後ろから３行目のフォーマットが異常 ＮＣ＝" + txtd.NcLine);
					txtd.OutLine.Set("G00Z" + (ncd.xmlD[tcnt].ClrPlaneList[0] * 1000).ToString("0"));
					txtd.OutLine.AtoAdd("M98P0006");
				}
				else {
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
