using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using CamUtil;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	class Conv_VG : Conv
	{
		/// <summary>加工機情報</summary>
		private readonly NcdTool.Mcn1 mach;
		/// <summary>M98P6を検出する正規表現</summary>
		private Regex re_P6;
		/// <summary>G105を検出する正規表現</summary>
		private Regex re_g105;
		/// <summary>G84のチェックメッセージに使用</summary>
		private bool g84mess;

		public Conv_VG(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach)
			: base(10, false, null, false, toolList) {
			this.mach = mach;
			//this.re_P6 = new Regex("M98 *P0*6[^0-9]");
			this.re_P6 = new Regex("M98 *P0*6([^0-9]|$)");
			this.re_g105 = new Regex("G105X-*[0-9][0-9.]*Y-*[0-9][0-9.]*$");
			this.g84mess = false;
			//ncconv = new StreamNcR_Conv("M98 *P0*6[^0-9]", ncoutName);
		}

		public override OutLine ConvExec() {
			//this.txtd = txtd;
			NcLineCode txtd = this.NcQue[0];

			// ///////////////////////////////////////////////////////////////
			// 暫定処置（UNIXでの出力を使用する場合の）
			if (txtd.LnumN == 3 && txtd.NcLine.IndexOf("M01") == 0) {
				LogOut.CheckCount("Conv_VG 039", false, this.ncname.Nnam + " M01 found.");
				txtd.OutLine.Set("");
				return txtd.OutLine;
			}
			if (txtd.LnumN == 3 && txtd.NcLine.IndexOf("M00") == 0) {
				LogOut.CheckCount("Conv_VG 044", false, this.ncname.Nnam + "M00 found.");
				txtd.OutLine.Set("");
				return txtd.OutLine;
			}

			// G100前のコード（%, O001）は処理しない
			if (txtd.Tcnt < 0) return txtd.OutLine;

			// Ｇ１００の処理(G100T01)
			if (txtd.B_g100 == true) {
				// 工具番号のチェック
				if (txtd.Code('T').L != NcoutName.Smch.K2.Tnum)
					throw new Exception("TNo ERROR" +
						"NC  =" + txtd.Code('T').L.ToString("000") +
						"tnum=" + NcoutName.Smch.K2.Tnum.ToString("000"));

				// 計測データの"S0"を消去
				Delete_S0(txtd);

				// 計測データのG100をG101に変換
				// プローブの工具長を測定するためコメントアウト by 坂本 in 2014/03/06
				//G101(txtd);

				// 工具径・工具長さ補正量、ツールセットＩＤ(C)を挿入
				SetMWC(txtd);

				if (!re_g105.IsMatch(this.NcQue.NcPeek(+2).NcLine))
					throw new Exception(
						"工具交換後のＧ１０５データに異常があります。" + this.NcQue.NcPeek(+2).NcLine);
			}
			// 加工工程間の逃がし動作の処理
			else if (txtd.NcLine.IndexOf("M98P8509") == 0) {
				if (!re_g105.IsMatch(this.NcQue.NcPeek(+1).NcLine))
					throw new Exception(
						"工程間移動後のＧ１０５データに異常があります。" + this.NcQue.NcPeek(+1).NcLine);
			}
			//===== M98P6 =====//
			else if (re_P6.IsMatch(txtd.OutLine.Txt)) {
				// 計測データのM98P6をM98P7に変更
				Match match = re_P6.Match(txtd.OutLine.Txt);
				if (TolInfo.Toolset.Probe)
					M98P7(txtd, match);
			}
			//===== G105 傾斜処理 =====//
			else if (txtd.OutLine.Txt.IndexOf("G105") == 0) {

				// チェック工具リファレンス点
				// （Tebis G0 は傾斜で３Ｄの場合工具中心のパス）
				// （プローブは工具中心で出力され、加工機側でも工具中心までの工具長が登録されている）
				if (this.Keisoku)
					if (NcoutName.Skog.Tld.XmlT.TRTIP) throw new Exception("qwkefbvqerbfh");

				if (NcoutName.Skog.Tld.XmlT.Keisha || NcoutName.Skog.Tld.XmlT.TRTIP == false) {
					// 傾斜角度の設定
					//RotationAxis rotSP = new RotationAxis(RotationAxis.JIKU.Euler_XYZ, ncoutName.skog.tld.nsgt1k.ax);
					if (NcoutName.Skog.Tld.XmlT.AxisCount != 1) {
						if (ProgVersion.Debug)
							System.Windows.Forms.MessageBox.Show("傾斜角の異なる工程を含むＮＣデータにはまだ対応していません。");
						else
							throw new Exception("傾斜角の異なる工程を含むＮＣデータにはまだ対応していません。");
					}
					RotationAxis rotSP = new RotationAxis(NcoutName.Skog.Tld.XmlT[0].MachiningAxisAngle);
					Angle3 ax = rotSP.Euler_ZXZ();
					// 加工成立性のチェック
					if (Math.Abs(ax.DegA) > 90.0) throw new Exception("オイラー角の計算が異常");
					if (Math.Abs((int)Math.Round(ax.DegB * 1000)) > 30 * 1000)
						System.Windows.Forms.MessageBox.Show($"ＶＧで加工できない工具の向き（Ａ軸３０度を超える回転角度）が設定されました");
					if ((int)Math.Round(ax.DegA) * 1000 != (int)Math.Round(ax.DegA * 1000))
						System.Windows.Forms.MessageBox.Show($"ＶＧで加工できないＣ軸回転角度{ax.DegA.ToString("0.000")} が指示されました in {NcoutName.Outnam}");
					if ((int)Math.Round(ax.DegB) * 1000 != (int)Math.Round(ax.DegB * 1000))
						System.Windows.Forms.MessageBox.Show($"ＶＧで加工できないＡ軸回転角度{ax.DegB.ToString("0.000")} が指示されました in {NcoutName.Outnam}");

					string sadd = "";

					// Ｚ座標値を挿入する
					if (Index.dimensionAXIS == 3) {
						for (int ii = 1; ii <= 3; ii++) {
							if (this.NcQue.NcPeek(ii).B_26('Z')) {
								if (this.NcQue.NcPeek(ii).G8 != 80)
									break;
								if (this.NcQue.NcPeek(ii).G6 != 67)
									throw new Exception("qwefbqefrhbwerhb"); ;
								//throw new Exception(
								//	"アプローチのＺ値が異常です in " + this.NcPeek(ii).NcLine);
								if (this.NcQue.NcPeek(ii).G1 != 0)
									break;
								//throw new Exception(
								//	"アプローチのＺ値が異常です in " + this.NcPeek(ii).NcLine);
								sadd += "Z" + this.NcQue.NcPeek(ii).Xyzsf.Zi.ToString();
								break;
							}
						}
						if (sadd.Length == 0) {
							System.Windows.Forms.MessageBox.Show(
								"アプローチのＺ値（工具軸方向にアプローチする位置）を100.0に設定します。");
							sadd += "Z100000";
						}


						// 回転角度と次元数と挿入する
						// ax.degCが０でなくとも３次元ＮＣデータはモデル座標系なので変わらない
						//if (ax.degC.ToString("0.0##") != "0.0") throw new Exception("aergvasbghvasbvgbsf");
						sadd +=
							"I" + ax.DegA.ToString("0.0##") +
							"J" + ax.DegB.ToString("0.0##") +
							"K0" +
							"D" + Index.dimensionAXIS.ToString();
						sadd += "R" + NcoutName.tolInfo.Toolset.Crad.ToString("0.0##");
					}
					else {
						// 回転角度と次元数と挿入する
						sadd +=
							"I" + ax.DegA.ToString("0.0##") +
							"J" + ax.DegB.ToString("0.0##") +
							"K" + ax.DegC.ToString("0.0##") +
							"D" + Index.dimensionAXIS.ToString();
					}
					txtd.OutLine.Set(txtd.OutLine.Txt + sadd);
				}
				else
					txtd.OutLine.Set(txtd.OutLine.Txt + "I0.0J0.0K0.0D2");
			}
			// 固定サイクルモード
			// カスタムマクロモード
			else if (txtd.G8 != 80 || txtd.G6 != 67) {
				;
			}
			// 一般モード
			else {
				// G84 のチェック
				if (txtd.B_g8 && txtd.G8 == 84 && g84mess == false) {
					System.Windows.Forms.MessageBox.Show("高速加工機でＧ８４のタップが使われている。");
					g84mess = true;
				}
				// 高速加工モード関連コードは２Ｄで傾斜時は削除
				if (Index.dimensionAXIS == 2 && NcoutName.Skog.Tld.XmlT.Keisha)
					if (txtd.OutLine.Txt.IndexOf("G05P0") == 0) {
						LogOut.CheckCount("Conv_VG 188", false, "２Ｄ傾斜で高速加工モード関連コード検出");
						txtd.OutLine.Set("");
					}
			}
			return txtd.OutLine;
		}
	}
}
