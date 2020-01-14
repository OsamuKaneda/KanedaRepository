using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using CamUtil;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	/// <summary>
	/// 一般ＮＣデータ用の変換仕様
	/// </summary>
	class Conv_Normal : Conv
	{
		/// <summary>加工機情報</summary>
		private NcdTool.Mcn1 mach;
		/// <summary>M98P6を検出する正規表現</summary>
		private Regex re_P6;

		private bool g84mess = false;
		private int kotei = -1;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Conv_Normal(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach)
			: base(10, false, null, true, toolList) {
				//this.re_P6 = new Regex("M98 *P0*6[^0-9]");
				this.re_P6 = new Regex("M98 *P0*6([^0-9]|$)");
				this.mach = mach;
		}

		public override OutLine ConvExec() {
			Match match;
			NcLineCode txtd = this.NcQue[0];

			//===== G100 =====//
			if (txtd.B_g100) {
				// 計測データの"S0"を消去
				Delete_S0(txtd);

				// 計測データのG100をG101に変換
				// プローブの工具長を測定するためコメントアウト by 坂本 in 2014/03/06
				//G101(txtd);

				switch (mach.ID) {
				case Machine.MachID.MCC2013:
				//case Machine.machID.MCD1513:
					// 工具径・工具長さ補正量、ツールセットＩＤ(C)を挿入
					SetMWC(txtd);
					break;
				case Machine.MachID.YMC430:
					// 工具測定識別記号の"F"を"E"に変換する
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace('F', 'E'));
					break;
				case Machine.MachID.YBM1218V:
					// ツールセット全長を追加する in 2012/07/23
					SetUUU(txtd);
					break;
				}
			}

			// G84 チェック
			if (txtd.B_g8 && txtd.G8 == 84 && mach.Performance == "HI" && g84mess == false) {
				System.Windows.Forms.MessageBox.Show("高速加工機でＧ８４のタップが使われている。");
				g84mess = true;
			}

			//===== M98P6 =====//
			match = re_P6.Match(txtd.OutLine.Txt);
			//if (match.Success || txtd.Code('M').l ==8) {    要不要が不明 2015/06/26
			if (match.Success) {
				// 計測データのM98P6をM98P7に変更
				if (TolInfo.Toolset.Probe)
					M98P7(txtd, match);
			}

			// 加工工程間の逃がし動作の処理
			Kotei(txtd.OutLine.Txt);
			if (kotei == 0) {
				txtd.OutLine.Set(ClLink5Axis.Kotei);
				throw new Exception("工程間の移動はまだ対応していない。");
			}
			return txtd.OutLine;
		}

		/// <summary>
		/// 加工工程間移動の行数設定とチェック
		/// </summary>
		/// <param name="outLine"></param>
		protected void Kotei(string outLine) {
			if (kotei == -1) {
				if (outLine.IndexOf("G04X1234") >= 0)
					throw new Exception("古い変換仕様です。");
				if (outLine.IndexOf(ClLink5Axis.Kotei) >= 0)
					kotei = 0;
			}
			else kotei++;

			// チェック＆リセット
			switch (kotei) {
			case 0:
				break;
			case 1:
				if (outLine.IndexOf('X') < 0 || outLine.IndexOf('Y') < 0)
					throw new Exception("工程間移動にＸＹ座標がない");
				break;
			case 2:
				if (outLine.IndexOf('Z') < 0 && outLine.IndexOf("G05") < 0)
					throw new Exception("工程間移動にＺ座標がない");
				kotei = -1;
				break;
			}
		}

	}
}
