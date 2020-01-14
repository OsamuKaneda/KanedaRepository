using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	class Conv_Buhin : Conv
	{
		/// <summary>加工機情報</summary>
		private NcdTool.Mcn1 mach;

		public Conv_Buhin(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach)
			: base(10, false, null, true, toolList) {
			this.mach = mach;
		}

		public override OutLine ConvExec() {
			NcLineCode txtd = this.NcQue[0];

			switch (txtd.G9) {
			case 90: break;
			default: throw new Exception("G91 が検出されました。");
			}

			// Ｇ１００の処理(G100T01)
			if (txtd.B_g100 == true) {
				if ((!Char.IsNumber(txtd.OutLine.Txt[5])) || (!Char.IsNumber(txtd.OutLine.Txt[6])) || Char.IsNumber(txtd.OutLine.Txt[7]))
					throw new Exception("ＮＣデータG100エラー(" + txtd.OutLine.Txt + ")");

				// ///////////////////////////////////////////////////
				// 部品加工でＳコードが変更された場合にログを出力する
				// ///////////////////////////////////////////////////
				if (txtd.Xyzsf.Si != 0 && txtd.Xyzsf.Si != this.NcoutName.Skog.CutSpinRate())
					CamUtil.LogOut.CheckOutput(CamUtil.LogOut.FNAM.BUHINFEED, NcdTool.Tejun.TejunName, NcdTool.Tejun.Mach.name,
						$"{this.ncname.Nnam} {this.NcoutName.TdatNo:d} nc=S{txtd.Xyzsf.Si:d} nw=S{this.NcoutName.Skog.CutSpinRate():d} {""}");
				// ///////////////////////////////////////////////////
				// 部品加工でＦコードが変更された場合にログを出力する
				// ///////////////////////////////////////////////////
				if (this.NcoutName.Skog.Tld.XmlT.FEEDR != this.NcoutName.Skog.CutFeedRate())
					CamUtil.LogOut.CheckOutput(CamUtil.LogOut.FNAM.BUHINFEED, NcdTool.Tejun.TejunName, NcdTool.Tejun.Mach.name,
						$"{this.ncname.Nnam} {this.NcoutName.TdatNo:d} nc=F{this.NcoutName.Skog.Tld.XmlT.FEEDR:f} nw=F{this.NcoutName.Skog.CutFeedRate():d} {""}");

				// 工具番号と次の工具番号の設定
				txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'T', Index.tno.ToString("00")));
				txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'T', "H" + Index.next.ToString("00"), false));

				// 回転数比率の反映
				if (txtd.B_26('S')) SpinSet(txtd);

				// 逆回転工具の設定
				if (TolInfo.Toolset.M0304 == "M04") {
					txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'S', -1.0));
				}
			}
			else {
				if (txtd.B_g4 && txtd.G4 != 40) {

					// フラットエンドミルとヘリカルタップのみ工具径オフセットを認める
					//（ふつうのタップにG41,G42が入っていても残念ながらチェックできない）
					if (TolInfo.Toolset.MeasType5AXIS != 'F' && TolInfo.Toolset.MeasType5AXIS != 'Q')
						throw new Exception($"T{Index.tno.ToString("00")},{NcoutName.OutnamNEW}に径オフセットが入ってます。");

					if (txtd.OutLine.Txt[3] != 'D' ||
					(!Char.IsNumber(txtd.OutLine.Txt[4])) || (!Char.IsNumber(txtd.OutLine.Txt[5])) ||
					(!Char.IsNumber(txtd.OutLine.Txt[6])) || (Char.IsNumber(txtd.OutLine.Txt[7])))
						throw new Exception("ＮＣデータG41,G42エラー");
					txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'D', (Index.tno + NcdTool.Tejun.Mach.ncCode.DIN).ToString("00")));
				}

				// 送り速度の設定
				if (txtd.B_26('F')) FeedSet(txtd);

				// エアーブローの設定（素材の座標値を挿入）
				if (txtd.NcLine.IndexOf("G65P9355") == 0) {
					// //////////////////////////////////////////////////////////////
					// これで従来と同等になるが、実際には小数点以下１桁で十分である
					// //////////////////////////////////////////////////////////////
					txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'U', IndexMain.mold_X.ToString("0.0###")));
					txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'V', IndexMain.mold_Y.ToString("0.0###")));
				}

				// ////////////
				// LineaMのみ
				// ////////////
				if (mach.ID == CamUtil.Machine.MachID.LineaM) {
					//M98P8101を元の直接座標値に戻す
					if (txtd.OutLine.Txt == "M98P8101")
						txtd.OutLine.Set("G00G90Z420.0");
					//G65P8104を元の直接コードに戻す（加工時間が長くなる対策）
					if (txtd.OutLine.Txt.IndexOf("G65P8104") == 0) {
						txtd.OutLine.Set("G" + txtd.OutLine.Txt.Substring(9).Replace("H", "P"));
						// Ｆコードを整数化する
						if (txtd.B_26('F')) {
							txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', ncvalue => ncvalue, true));
						}
					}
				}
			}
			return txtd.OutLine;
		}

		/// <summary>
		/// 回転数の設定
		/// </summary>
		/// <param name="txtd"></param>
		private void SpinSet(NcLineCode txtd) {
			string chk;

			chk = txtd.OutLine.Txt;
			if (this.NcoutName.Skog.ChgSpin) {
				txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'S', this.NcoutName.Skog.CutSpinRate));
			}

			// ///////////////////////////////////////////////////
			// 部品加工でＳコードが変更された場合にログを出力する
			// ///////////////////////////////////////////////////
			if (chk != txtd.OutLine.Txt)
				CamUtil.LogOut.CheckOutput(CamUtil.LogOut.FNAM.BUHINFEED, NcdTool.Tejun.TejunName, NcdTool.Tejun.Mach.name,
					$"{this.ncname.Nnam} {this.NcoutName.TdatNo:d} nc=S{txtd.Xyzsf.Si:d} nw=S{this.NcoutName.Skog.CutSpinRate():d} {chk + "-> " + txtd.OutLine.Txt}");
		}

		/// <summary>
		/// 送り速度の設定
		/// </summary>
		/// <param name="txtd"></param>
		private void FeedSet(NcLineCode txtd) {
			string chk;
			double ff;

			if (txtd.G6 == 67) {
				if (txtd.Xyzsf.Fi == NcLineCode.RF_5AXIS) {
					// 早送り速度を８０ｍから加工機の最大送り速度に変更
					if (Convert.ToInt32(mach.ncCode.RPD) != NcLineCode.RF_5AXIS) {
						txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'F', mach.ncCode.RPD.ToString("00000")));
					}
					chk = txtd.OutLine.Txt;
				}
				else {
					chk = txtd.OutLine.Txt;
					if (this.NcoutName.Skog.ChgFeed) {
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', this.NcoutName.Skog.CutFeedRate));
					}
				}
			}
			else {
				chk = txtd.OutLine.Txt;
				switch (txtd.G6p.ProgNo) {
				case 8104:  // 部品加工用マクロ（ヘリカル/円錐補間）			例：G65P8104D03X-23.6828Y155.265I-2.J0.Z19.6848H569F18.3281(Fの存在は稀)
					if (this.NcoutName.Skog.ChgFeed) {
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', this.NcoutName.Skog.CutFeedRate));
					}
					break;
				case 9355:	// 部品加工用マクロ（エアーブロー）					例：G65P9355U34.0V47.0Z#805Q20.F10000
					// 変更不可とする 2016/04/05
					break;
				case 8105:	// 部品加工用マクロ（リジッドタップ）				例：G65P8105X8.3148Y-10.9628R185.9274Z166.8874S372F1.0
					// Ｆはピッチのため変更不要
					ff = (double)this.NcoutName.Skog.CutFeedRate() / this.NcoutName.Skog.CutSpinRate();
					if (Math.Abs(txtd.G6p['F'].D - ff) > 0.01)
						throw new Exception($"ネジのピッチが整合しない：{txtd.G6p['F'].D.ToString()} -> {ff.ToString()}");
					break;
				case 8102:	// 部品加工用マクロ（加工モード設定）				例：G65P8102A3.B22.
				case 8700:  // 部品加工用マクロ（座標系の設定）					例：G65P8700A-90.B180.
				case 8730:	// 部品加工用マクロ（座標系のクリア）				例：G65P8730X0.Y0.Z0.
				case 8750:	// 部品加工用マクロ（後測定：形状測定開始）			例：G65P8750
				case 8759:	// 部品加工用マクロ（後測定：形状測定終了）			例：G65P8759
				case 9376:	// 部品加工用マクロ（工具名をマクロ変数に入力する）	例：G65P9376I66.J76.K79.I87.(BLOW)
				case 8755:	// 測定												例：G65P8755X4.7193Y5.0673Z287.462I0.J0.K1.R5.
				case 9345:	// 部品加工用マクロ（前測定）						例：G65P9345X13.0002Y13.5012Z288.27I0.J0.K1.T0
				case 9346:	// 部品加工用マクロ（前測定）						例：G65P9346X13.0002Y23.5012Z278.27I0.J1.K0.T1
				default:
					throw new Exception("P8104, P8105, P9355以外のマクロ呼出しで送り速度が設定されている：" + txtd.OutLine.Txt);
				}
			}
			// ///////////////////////////////////////////////////
			// 部品加工でＦコードが変更された場合にログを出力する
			// ///////////////////////////////////////////////////
			if (chk != txtd.OutLine.Txt && txtd.LnumT < 100)
				CamUtil.LogOut.CheckOutput(CamUtil.LogOut.FNAM.BUHINFEED, NcdTool.Tejun.TejunName, NcdTool.Tejun.Mach.name,
					$"{this.ncname.Nnam} {this.NcoutName.TdatNo:d} nc=F{txtd.Xyzsf.Fi:d} nw=F{this.NcoutName.Skog.CutFeedRate():d} {chk + "-> " + txtd.OutLine.Txt}");
		}
	}
}
