using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	/// <summary>
	/// 以下の処理を実行する
	/// １．検証用のデータを元に戻す(NcSIM.RegForm)
	/// ２．検証用のデータを使用して回転数・送り速度の比率を加味する
	/// ３．出力しない工具を削除する
	/// </summary>
	/// <remarks>
	/// 検証システムの送り速度最適化の場合、切削部分のみに最適化がされており、
	/// アプローチ・リトラクト部分はここで回転数と送り速度の比率を反映する。
	/// 比率をかけるか否かの判断は NcSIM.RegForm の情報を用いる。
	/// </remarks>
	class SimSpinFeed : INcConvert
	{
		/// <summary>ＮＣデータの情報</summary>
		private NcdTool.NcName.NcNam ncname;
		/// <summary>工具単位情報</summary>
		private NcdTool.NcName.Kogu Skog { get { return ncname.Tdat[m_ncQue[0].Tcnt]; } }
		/// <summary>出力しない工具の削除</summary>
		private readonly bool remove;
		private readonly bool[] koteiLink;

		/// <summary>ＮＣデータが保存されているバッファー</summary>
		public NcQueue NcQue { get { return m_ncQue; } }
		private readonly NcQueue m_ncQue;

		/// <summary>出力用ＮＣ情報</summary>
		private NcLineCode nlcOut;
		/// <summary>工具の消去に使用</summary>
		bool t00;

		/// <summary>シミュレーション後のＮＣデータを標準仕様に戻す方法を提供</summary>
		NcdTool.NcSIM.RegForm regf;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="p_ncname">ＮＣデータ情報</param>
		/// <param name="p_remove">出力しない工具の削除（検証中の場合のみ削除しない）</param>
		/// <param name="setsuzoku"></param>
		public SimSpinFeed(NcdTool.NcName.NcNam p_ncname, bool p_remove, bool[] setsuzoku) {
			ncname = p_ncname;
			remove = p_remove;
			regf = new NcdTool.NcSIM.RegForm(p_ncname);
			m_ncQue = new NcQueue(0, false, (double[])null, NcdTool.Tejun.BaseNcForm, NcLineCode.GeneralDigit, true, true);
			nlcOut = new NcLineCode((double[])null, NcdTool.Tejun.BaseNcForm, NcLineCode.GeneralDigit, true, true);
			t00 = false;
			this.koteiLink = setsuzoku;
		}

		/// <summary>
		/// シミュレーション後のＮＣデータを標準仕様に戻す
		/// </summary>
		/// <remarks>
		/// this.ncQue[0]はシミュレーションの情報をそのまま入力しているため、
		/// サブプログラムが設定されているところなどでまったく異なるモードに
		/// なっている場合がある。よって、this.ncQue[0]のb_26 などのプロパティは
		/// 使用できないため、NcLineCode output を新たに設けNcLine()にて正しいモードが設定
		/// されるようにしている。
		/// 最終的には、this.ncQue[0].outLineには正しい変換結果を保存する。
		/// </remarks>
		/// <returns></returns>
		public OutLine ConvExec() {
			NcLineCode nlcInp;
			string ddat;

			nlcInp = this.NcQue[0];
			if (nlcInp.OutLine.Count != 1) {
				if (nlcInp.OutLine.Count == 0)
					throw new Exception(ncname.nnam + ": シミュレーション時に作成したＮＣデータに空白行が存在します。");
				throw new Exception("fvrqerfvqefrvqkefmv");
			}
			if (nlcInp.B_26('N')) {
				throw new Exception($"ＮＣデータ：{ncname.nnam}にシーケンス番号が含まれている。{nlcInp.NcLine}");
			}

			// //////////////////////////////////////////////////
			// シミュレーション出力のＮＣデータを標準仕様に戻す
			// //////////////////////////////////////////////////
			ddat = regf.Conv(nlcInp.OutLine.NcLineOut());
			if (ddat == null) {
				nlcOut.NextLine("");
				nlcInp.OutLine.Set("", "");
				return nlcOut.OutLine;
			}

			// 前のルーティンと_ncMainでチェックのため出力。変更不可。
			//txtd.outLine.Set(ddat);
			//txtd.outLine.commOut = false;

			// /////////////////////////////////////////////////////////
			// outputにデータをセットするとともにT00 の工具の処理をする
			// /////////////////////////////////////////////////////////
			if (t00) {
				if (nlcInp.B_g100) throw new Exception("aeqrfqref");
				if (nlcInp.B_p0006)
					t00 = false;
				if (remove)
					nlcOut.NextLine("");
				else nlcOut.NextLine(ddat);
			}
			else if (nlcInp.B_g100 && Skog.Output == false) {
				t00 = true;
				if (remove) {
					nlcOut.NextLine("G100T00");
					nlcOut.OutLine.Set("", "");
				}
				else nlcOut.NextLine(ddat);
			}
			else
				nlcOut.NextLine(ddat);
			if (koteiLink[0] == false) if (CamUtil.ClLink5Axis.Exists(nlcOut.NcLine))
					koteiLink[0] = true;	// 工程間ＣＬ接続有無の設定

			// ///////////////////////////////////////////////
			// 回転数と送り速度の比率を反映する
			// ///////////////////////////////////////////////
			if (nlcInp.Tcnt != nlcOut.Tcnt) throw new Exception("efbqhwefbqheb");
			if (nlcOut.Tcnt < 0 || t00) {
				;
			}
			// Ｇ１００の処理(G100T01)
			else if (nlcOut.B_g100 == true) {
				// Conv_NcRun1で回転数が設定されるためここでは不要
				;
			}
			else {
				// 回転数の設定（形状測定マクロP8755のみがＳを持つ）
				if (nlcOut.B_26('S')) SpinSet(nlcOut);
				// 送り速度の設定
				FeedSet(nlcOut);
			}
			if (nlcOut.OutLine.Count > 1) throw new Exception("qfbqfrbqref");
			nlcInp.OutLine.Set(nlcOut.OutLine.Txt, nlcOut.OutLine.Comment);
			return nlcOut.OutLine;
		}

		/// <summary>
		/// 回転数の設定
		/// </summary>
		/// <param name="txtd"></param>
		private void SpinSet(NcLineCode txtd) {

			if (txtd.B_g100) {
				throw new Exception("ここは実行されない");
			}
			else {
				if (!txtd.B_g6) {
					// G66P8290のマクロ展開でＳコードを使用するがここでは展開後のデータは読み込まないためエラーで可
					throw new Exception(ncname.nnam + ": ＮＣデータの回転数がG100以外で設定されている " + txtd.OutLine.Txt);
				}
				else {
					switch (txtd.G6p.ProgNo) {
					case 8755:	// 形状測定マクロ
						// 引数でＳコードを使用している。意味は回転数でないため比率は考慮不要
						break;
					default:
						throw new Exception(ncname.nnam + ": ＮＣデータの回転数がG100以外で設定されている " + txtd.OutLine.Txt);
					}
				}
			}
		}

		/// <summary>
		/// 送り速度の設定
		/// </summary>
		/// <param name="txtd"></param>
		private void FeedSet(NcLineCode txtd) {

			if (txtd.B_26('F')) {
				if (regf.FeedSetArea) {
					if (this.Skog.ChgFeed) {
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', this.Skog.CutFeedRate));
					}
				}
				else {
					if (this.Skog.ChgFeedMachine) {
						txtd.OutLine.Set(NcLineCode.NcRateSF(txtd.OutLine.Txt, 'F', this.Skog.CutFeedRateMachine));
					}
				}
			}

			// /////////////////////////////////////////
			// カスタムマクロでの送り速度設定のチェック
			// /////////////////////////////////////////
			if (txtd.G6 == 67) { ;}
			else {
				CamUtil.CamNcD.MacroCode mCode = new CamUtil.CamNcD.MacroCode(NcdTool.Tejun.BaseNcForm, txtd.G6p.ProgNo);
				if (mCode.hlist.IndexOf('F') < 0) {
					if (txtd.B_26('F'))
						throw new Exception(ncname.nnam + ": P8401, P8402のマクロ呼出しで送り速度が設定されている：" + txtd.OutLine.Txt);
				}
				else {
					if (txtd.B_g6 && !txtd.B_26('F'))
						throw new Exception(ncname.nnam + ": P8401, P8402以外ののマクロ呼出しで送り速度が設定されていない：" + txtd.OutLine.Txt);
				}
			}
		}
	}
}
