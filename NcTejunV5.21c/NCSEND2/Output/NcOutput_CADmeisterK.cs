using System;
using System.Collections.Generic;
using System.Text;

using CamUtil.LCode;
using System.IO;
using System.Windows.Forms;

namespace NCSEND2.Output
{
	class NcOutput_CADmeisterK : NcOutput
	{
		// コンストラクタ
		public NcOutput_CADmeisterK(NcReader sr, NCINFO.NcInfoCam ncd, double kataZ) : base(sr, ncd) { this.kataZ = kataZ; }

		/// <summary>
		/// ＮＣデータ１工具分の出力
		/// </summary>
		/// <param name="tcnt">工具順の数</param>
		/// <param name="tfoo">ＮＣデータの出力先</param>
		/// <param name="aprzd">加工原点Ｚ</param>
		/// <returns>パスの移動距離</returns>
		protected override NcLineCode.NcDist NcConvertN(int tcnt, StreamWriter tfoo, double aprzd) {
			NcLineCode txtd;
			int jj;

			// ＤＢ切削条件（回転数）
			if (ncd.NcdtInfo[tcnt].Spidle < 0.001)
				throw new Exception("回転数のデータがありません。");
			double RateS = ncd.sqlDB[tcnt].Spin / ncd.NcdtInfo[tcnt].Spidle;
			// ＤＢ切削条件（送り速度）
			if (ncd.NcdtInfo[tcnt].Feedrt < 0.001)
				throw new Exception("切削送り速度のデータがありません。");
			double RateF = ncd.sqlDB[tcnt].Feedrate / ncd.NcdtInfo[tcnt].Feedrt;

			// 出力ＮＣデータの情報。加工長を計算するために使用
			NcLineCode outCode = new NcLineCode(new double[] { aprzd }, CamUtil.BaseNcForm.GENERAL, post, false, false);
			outCode.NextLine("%");
			NcLineCode.NcDist passLength =
				new NcLineCode.NcDist(ncd.sqlDB[tcnt].Feedrate, ncd.xmlD[tcnt].MachiningAxisList);

			while (true) {
				if (ncQue.QueueMax == 0)
					break;
				if (ncQue.NcPeek(1).Tcnt > tcnt)
					break;

				//txtd = ReadNcd_haisi();
				txtd = ncQue.NextLine(sr);

				switch (txtd.LnumT) {
				case 1: // Ｇ１００の処理(G100T01)
					if (txtd.B_g100 != true) {
						MessageBox.Show(
							"NCDATA ERROR lnum=1", "NcOutput",
							MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
						throw new Exception("");
					}
					// G100, Tコード, Sコード のみにする
					if (Math.Abs(txtd.Xyzsf.Si - ncd.NcdtInfo[tcnt].Spidle) > 0.99)
						throw new Exception("ＮＣデータ内の回転数がＸＭＬ情報と異なる。");
					jj = (int)Math.Round(txtd.Xyzsf.S * RateS, MidpointRounding.AwayFromZero);
					txtd.OutLine.Set("G100T99S" + jj.ToString("00000"));

					// ＩコードをＳの前に挿入
					txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'S', ncd.sqlDB[tcnt].toolsetTemp.ICode, true));
					// 工具種類・直径情報コードをＳの前に挿入
					txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'S', ncd.sqlDB[tcnt].toolsetTemp.DCode, true));
					if (txtd.OutLine.Comment != "")
						txtd.OutLine.Set(txtd.OutLine.Txt + "(" + txtd.OutLine.Comment + ")");
					break;
				case 2:	// 基準Ｚの処理(G90G00Z100000)
					txtd.OutLine.Set("");
					break;
				case 3:	// 基準ＸＹの処理(X0Y0)
					if (txtd.OutLine.Txt.IndexOf("X0Y0") < 0)
						throw new Exception("afwebrfbg");
					txtd.OutLine.Set("G00G90X0Y0");
					break;
				case 4:	// 加工点への移動(X100000Y311000;)
					if (txtd.B_26('X') == false || txtd.B_26('Y') == false)
						throw new Exception("aefrrhbh");
					break;
				default:	// 加工
					// 固定サイクルの設定行に現在のＸＹの値を表示
					if (txtd.G8 != 80) {
						if (txtd.B_26('X') != true && txtd.B_26('Y') != true && txtd.B_26('Z') != true && txtd.B_26('R') != true)
							throw new Exception("awfarfb");
						// Ｙコードの追加
						if (txtd.OutLine.Txt.IndexOf('Y') < 0) {
							if (txtd.B_26('X'))
								txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'X', "Y" + txtd.Xyzsf.Yi.ToString(), false));
							else {
								txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, new char[] { 'Z', 'R' }, "Y" + txtd.Xyzsf.Yi.ToString(), true));
							}
						}
						// Ｘコードの追加
						if (txtd.OutLine.Txt.IndexOf('X') < 0) {
							txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'Y', "X" + txtd.Xyzsf.Xi.ToString(), true));
						}
					}
					// カスタムマクロ内のＸＹを整合
					if (txtd.B_g6 && txtd.G6 != 67) {
						if (txtd.B_26('X') || txtd.B_26('Y'))
							throw new Exception("wefqgfrvqgfrqrvfg");
					}
					else if (txtd.G6 != 67) {
						if (txtd.B_26('X') != true && txtd.B_26('Y') != true)
							throw new Exception("awfarfb");
						// Ｙコードの追加
						if (txtd.OutLine.Txt.IndexOf('Y') < 0) {
							txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'X', "Y" + txtd.Xyzsf.Yi.ToString(), false));
						}
						// Ｘコードの追加
						if (txtd.OutLine.Txt.IndexOf('X') < 0) {
							txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'Y', "X" + txtd.Xyzsf.Xi.ToString(), true));
						}
					}
					// ＦコードをＤＢ条件に変更
					if (txtd.B_26('F') == true) {
						if (Math.Abs(txtd.Xyzsf.Fi - ncd.NcdtInfo[tcnt].Feedrt) > 0.99)
							if (txtd.B_g6 && txtd.Xyzsf.Fi == 0) {; }
							else
								MessageBox.Show($"ＮＣデータ{ncd.OutName}の送り速度がF={txtd.Xyzsf.Fi.ToString()} でありＸＭＬ情報と異なる。");
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));
					}
					// G00後のG01,G02,G03のある行にＦコードを挿入
					else if (txtd.B_g1 && txtd.G1 != 0 && ncQue.NcPeek(-1).G1 == 0) {
						txtd.OutLine.Set(txtd.OutLine.Txt + "F" + txtd.Xyzsf.Fi.ToString());
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', RateF));
					}
					break;
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
						throw new Exception($"{ss}加工長計算でエラーが発生しました。\n{ex.Message}");
					}
				}
			}
			return passLength;
		}
	}
}
