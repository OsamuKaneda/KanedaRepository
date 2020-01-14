using System;
using System.Collections.Generic;
using System.Text;

using CamUtil.LCode;
using System.Text.RegularExpressions;

namespace NcTejun.NcRead
{
	/// <summary>
	/// ＮＣデータを変換する抽象クラス
	/// 準備されたtxtdに基づき変換する（変換後：txtd.OutLine）
	/// </summary>
	abstract class Conv : INcConvert
	{
		/// <summary>ＮＣデータの情報</summary>
		//protected NcdTool.NcName.NcNam ncname { get { return toolList[0].ncnam; } }
		protected Ncd ncname;
		protected struct Ncd
		{
			//
			// 注意！！ここから tdat[tcnt] を呼ぶと NcNam の tcnt と NcListCode の tcnt が一致しないため不具合となる。ncoutName.skog を使用すること
			//
			private NcdTool.NcName.NcNam ncname;
			public string Nnam { get { return ncname.nnam; } }
			public NcdTool.NcName.NcData Ncdata { get { return ncname.Ncdata; } }
			public NcdTool.NcName.Nmgt Nmgt { get { return ncname.nmgt; } }
			public NcdTool.NcName.NcNam.St_nggt Nggt { get { return ncname.nggt; } }
			public Ncd(NcdTool.NcName.NcNam ncn) { ncname = ncn; }
		}

		/// <summary>ＮＣデータの加工条件</summary>
		protected Output.Index_Main IndexMain { get { return Output.Index.IndexMain; } }

		/// <summary>現在処理中の工具情報</summary>
		protected Output.NcOutput.NcToolL NcoutName { get { return toolList[Tcnt]; } }
		/// <summary>現在処理中の工具の加工条件</summary>
		protected Output.Index Index { get { return toolList[Tcnt].index; } }
		/// <summary>現在処理中の工具</summary>
		protected NcdTool.ToolSetInfo TolInfo { get { return toolList[Tcnt].tolInfo; } }
		/// <summary>計測データの場合 true</summary>
		protected bool Keisoku { get { return toolList[Tcnt].tolInfo.Toolset.Probe; } }

		private readonly List<Output.NcOutput.NcToolL> toolList;

		private int Tcnt { get { return Math.Max(0, NcQue[0].Tcnt); } }

		// 追加終了

		/// <summary>シーケンスＮｏ．用</summary>
		protected long sequence = 0;

		/// <summary>ＮＣデータが保存されているバッファー</summary>
		public NcQueue NcQue { get { return m_ncQue; } }
		private readonly NcQueue m_ncQue;

		/// <summary>同一エラーの抑制用</summary>
		protected List<string> errCheck = new List<string>();


		/// <summary>
		/// 共通コンストラクタ（StreamNcR2を使う場合）
		/// </summary>
		/// <param name="bufnum">処理行の前後の参照可能な行数</param>
		/// <param name="nconly">ＮＣコード以外の情報を消去する</param>
		/// <param name="apz">最初の工具のＺ座標値</param>
		/// <param name="commentOutput">出力時のコメント出力初期値</param>
		/// <param name="toolList"></param>
		protected Conv(int bufnum, bool nconly, double[] apz, bool commentOutput, List<Output.NcOutput.NcToolL> toolList) {
			m_ncQue = new NcQueue(10, nconly, apz, NcdTool.Tejun.BaseNcForm, NcLineCode.GeneralDigit, commentOutput, true);
			this.toolList = toolList;
			this.ncname = new Ncd(toolList[0].Ncnam);
		}


		/// <summary>
		/// ＮＣデータの変換
		/// </summary>
		/// <returns>ＮＣデータ複数行の出力</returns>
		public abstract OutLine ConvExec();






		// ///////////////////////////////////////////////////////////////////////////////////
		// 変換仕様
		// ///////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// 回転数０のＳコード（例：S00000）を削除する
		/// </summary>
		/// <param name="txtd"></param>
		protected void Delete_S0(NcLineCode txtd) {
			// ユニックスでは"S0"がエラーとなるため計測データでも
			// 暫定的に"S20"としている。よってここで"S20"を消去する
			if (TolInfo.Toolset.Probe) {
				if (CamUtil.StringCAM.GetNcCode(txtd.OutLine.Txt, 'S')[0] != "S00000") throw new Exception("プローブの回転数が０以外に設定されている NC=" + txtd.OutLine.Txt);
				txtd.OutLine.Set(CamUtil.LCode.NcLineCode.NcDelChar(txtd.OutLine.Txt, 'S'));
				return;
			}
			else {
				if ((int)Math.Round(Convert.ToDouble(CamUtil.StringCAM.GetNcCode(txtd.OutLine.Txt, 'S')[0].Substring(1))) == 0)
					throw new Exception("回転数が０に設定されている NC=" + txtd.OutLine.Txt);
				return;
			}
		}

		/// <summary>
		/// 工具径補正量（メモリー運転用）、工具長補正量（メモリー運転用）、ツールセットＩＤを追加する
		/// </summary>
		/// <param name="txtd"></param>
		protected void SetMWC(NcLineCode txtd) {
			string sout = txtd.OutLine.Txt.Substring(0, txtd.OutLine.Txt.Length);

			// 工具径・工具長さ補正量、ツールセットＩＤ(C)を挿入
			if (Index.hosei_r.HasValue)
				sout += "M" + Index.hosei_r.Value.ToString("0.0###");
			if (Index.hosei_l.HasValue)
				sout += "W" + Index.hosei_l.Value.ToString("0.0###");

			sout += "C" + TolInfo.Toolset.ID + ".0";

			txtd.OutLine.Set(sout);
			return;
		}

		/// <summary>
		/// ツールセット全長を追加する in 2012/07/23
		/// </summary>
		/// <param name="txtd"></param>
		protected void SetUUU(NcLineCode txtd) {
			// ツールセット全長(U)を挿入
			txtd.OutLine.Set(txtd.OutLine.Txt + "U" + TolInfo.Min_length.ToString("0.0###"));
		}

		/// <summary>
		/// 加工前に工具測定しない場合、G100をG101に変換する
		/// </summary>
		/// <param name="txtd">G100を含む変換するＮＣデータ</param>
		/// <returns>変換後のＮＣデータ</returns>
		protected void G101(NcLineCode txtd) {
			if (Keisoku)
				txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G100", "G101"));
		}

		/// <summary>
		/// 工具測定しない場合（加工後）
		/// </summary>
		/// <param name="txtd"></param>
		/// <param name="mt"></param>
		protected void M98P7(NcLineCode txtd, Match mt) {
			txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'P', "0007"));
			return;
		}

		/// <summary>
		/// sedのs変換
		/// </summary>
		/// <param name="ddat">元の文字列</param>
		/// <param name="regptr">変換されるRegex</param>
		/// <param name="chikan">置換する文字列</param>
		/// <param name="glob">グローバル変換の場合はtrue</param>
		protected string Substitute(string ddat, Regex regptr, string chikan, bool glob) {
			string outs = ddat;
			int ptmp;
			Match regout;
			string ret0, ret1, chik, stmp;
			string[] chip = new string[5];

			ptmp = 0;
			//while (rgxqt(edit[ii].regptr, regout, adat[sdat].Substring(ptmp))) {
			while ((regout = regptr.Match(outs.Substring(ptmp))).Success) {
				int loc1 = regout.Index;
				int ncurs = regout.Index + regout.Length;

				ret0 = Retn(regout, outs.Substring(ptmp), 1);
				ret1 = Retn(regout, outs.Substring(ptmp), 2);
				if (chikan.IndexOf('%') >= 0)
					throw new Exception("Check Stop");
				chik = Chikan(chikan, chip, ret0, ret1);
				stmp = String.Format(chik, chip[0], chip[1], chip[2], chip[3], chip[4]);
				outs =
					outs.Substring(0, ptmp + loc1) +
					stmp +
					outs.Substring(ptmp + ncurs);
				ptmp += loc1 + stmp.Length;
				if (!glob) break;
			}
			return outs;
		}

		/// <summary>
		/// 部分マッチした文字列
		/// </summary>
		/// <param name="regout">マッチングの出力</param>
		/// <param name="strg">対象文字列</param>
		/// <param name="num">Ｎ番目のマッチした文字列（Ｎ＞＝１）</param>
		/// <returns></returns>
		private string Retn(Match regout, string strg, int num) {
			if (regout.Groups.Count > num)
				return strg.Substring(regout.Groups[num].Index, regout.Groups[num].Length);
			else
				return "";
		}

		private string Chikan(string chik, string[] chip, string ret0, string ret1) {
			StringBuilder ret = new StringBuilder();
			int jj = 0;
			while (jj < chik.Length) {
				if (chik[jj] == '%') {
					// '%'の次にさらに'%'を挿入する
					ret.Append("%%");
					jj++;
				}
				else
					ret.Append(chik[jj]);
				jj++;
			}
			return ret.ToString();
		}
	}
}
