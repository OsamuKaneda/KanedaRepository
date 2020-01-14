using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.IO;
using CamUtil;

namespace NcTejun.Output
{
	partial class NcOutput
	{
		public class NcOutList
		{
			/// <summary>ＮＣデータ単位、出力単位、工具単位に整理</summary>
			public readonly NcToolL[][][] NcdOutAll;
			/// <summary>手順の工具単位リスト</summary>
			public List<NcToolL> Tj { get { return NcdOutAll.SelectMany(c => c).SelectMany(c => c).ToList(); } }
			/// <summary>手順の工具単位リスト。共有を除く。</summary>
			public List<NcToolL> Tl { get { return NcdOutAll.SelectMany(c => c).SelectMany(c => c).Where(c => c.nknum == null).ToList(); } }
			/// <summary>ＮＣデータ単位、出力単位、工具単位リスト。出力データのみに限定。</summary>
			public NcToolL[][][] NcdOutTol {
				get {
					return NcdOutAll
						.Select(
							c => c.Select(
								d => d.Select(e => e).Where(e => e.nknum == null && e.Skog.Output).ToArray()
							).Where(d => d.Length > 0).ToArray()
						).Where(c => c.Length > 0).ToArray();
				}
			}

			public NcOutList() {
				string otNam = null;

				NcToolL ncoutName0, ncoutName1;
				List<NcToolL> ntmp0 = new List<NcToolL>();
				List<NcToolL[]> ntmp1 = new List<NcToolL[]>();
				List<NcToolL[][]> ntmp2 = new List<NcToolL[][]>();

				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll) {
					ncoutName1 = null;
					ntmp0.Clear();
					ntmp1.Clear();
					if (ncnam.Jnno.Nknum == null) {
						for (int jj = 0; jj < ncnam.Itdat; jj++) for (int kk = 0; kk < ncnam.Tdat[jj].matchK.Length; kk++) {
								ncoutName0 = ncoutName1;
								ncoutName1 = new NcOutput.NcToolL(ncnam, jj, kk, null);
								if (ncoutName0 == null) {
									otNam = ncoutName1.Outnam;
									continue;
								}
								ntmp0.Add(ncoutName0);
								if (otNam != ncoutName1.Outnam) {
									ntmp1.Add(ntmp0.ToArray());
									ntmp0.Clear();
									otNam = ncoutName1.Outnam;
								}
							}
						if (ncoutName1 != null) {
							ntmp0.Add(ncoutName1);
							ntmp1.Add(ntmp0.ToArray());
						}
					}
					else {
						NcToolL[][] ntmpA = null;
						foreach (NcToolL[][] cc in ntmp2) if (ncnam.Jnno.Nknum == cc[0][0].Ncnam) ntmpA = cc;
						if (ntmpA == null) throw new Exception("ewfrvwefrvbh");
						if (ntmpA[0][0].nknum != null) throw new Exception("qwhedbqehb");
						if (ntmpA[0][0].Ncnam.Itdat != 1) throw new Exception("参照ＮＣデータエラー");
						foreach (NcToolL[] ntmpB in ntmpA) {
							foreach (NcToolL ntmp in ntmpB) ntmp0.Add(new NcOutput.NcToolL(ncnam, ntmp.TdatNo, ntmp.MatchNo, ntmp));
							ntmp1.Add(ntmp0.ToArray());
							ntmp0.Clear();
						}
					}
					if (ntmp1.Count > 0) ntmp2.Add(ntmp1.ToArray());
				}
				NcdOutAll = ntmp2.ToArray();
			}

			/// <summary>
			/// 出力ファイル数を数える（異なる出力名の数）
			/// </summary>
			/// <returns>出力ファイル数</returns>
			public int OF_Count() { return NcdOutAll.SelectMany(c => c).Count(); }

			/// <summary>
			/// １ファイル内の工具単位出力リスト
			/// </summary>
			/// <param name="count"></param>
			/// <returns></returns>
			public List<NcToolL> ListInFile(int count) {
				return NcdOutAll.SelectMany(c => c).ElementAt(count).ToList();
			}
		}

		/// <summary>ＮＣ出力対象の工具単位の情報を保存するクラス</summary>
		public class NcToolL
		{
			/// <summary>共有する情報</summary>
			public readonly NcToolL nknum;

			/// <summary>工具単位情報</summary>
			public NcdTool.NcName.Kogu Skog { get { return m_ncnam.Tdat[m_tdatNo]; } }
			/// <summary>マッチング情報</summary>
			public NcdTool.TMatch.MatchK Smch { get { return m_ncnam.Tdat[m_tdatNo].matchK[m_matchNo]; } }

			/// <summary>ＮＣデータ情報</summary>
			public NcdTool.NcName.NcNam Ncnam { get { return m_ncnam; } }
			private readonly NcdTool.NcName.NcNam m_ncnam;

			/// <summary>マッチング情報matchKのNo。分割しない場合は０のみ（>=0）add in 2015/02/05</summary>
			public int MatchNo { get { return m_matchNo; } }
			private readonly int m_matchNo;

			public string TolstName { get { return m_tolstName; } }
			private readonly string m_tolstName;

			//public int progNo { get { return m_progNo; } }
			//private readonly int m_progNo;

			public int TdatNo { get { return m_tdatNo; } }
			private readonly int m_tdatNo;

			/// <summary>出力ファイル名</summary>
			public string Outnam { get { return m_outnam; } }
			/// <summary>自動分割を考慮したＮＣデータ出力名</summary>
			public string OutnamNEW { get { return ServerPC.PTPName.FileNameTrim(NcdTool.Tejun.BaseNcForm, m_outnam); } }
			private readonly string m_outnam;

			/// <summary>データベースの工具情報</summary>
			public readonly NcdTool.ToolSetInfo tolInfo;

			// /// <summary>同一工具で加工する工具単位ＮＣデータの数</summary>
			// public int tncNum;

			/// <summary>ＮＣデータ座標値の最大と最小</summary>
			public CamUtil.LCode.NcLineCode.NcDist ncdist;

			/// <summary>ＮＣデータの出力可否</summary>
			public bool tNodeChecked;

			// ////////////////////////////
			// ２つのディメンジョンの設定
			// ////////////////////////////
			/// <summary>加工手順の次元設定値＝Ｇ０１変換に使用する</summary>
			public int? DimensionG01 {
				get {
					switch (NcdTool.Tejun.BaseNcForm.Id) {
					case CamUtil.BaseNcForm.ID.GENERAL:
						return Convert.ToInt32(m_ncnam.nmgt.Dimn.ToString());
					case CamUtil.BaseNcForm.ID.BUHIN:
						return null;
					default:
						throw new Exception("aregbqehrb");
					}
				}
			}

			/// <summary>インデックスの工具単位の情報</summary>
			public Index index;

			// ///////////////////////////
			// 以下は作成後に情報を追加する
			// ///////////////////////////



			public NcToolL(NcdTool.NcName.NcNam ncnam, int tdatNo, int matchNo, NcToolL p_nknum) {
				this.nknum = p_nknum;
				this.m_tolstName = ncnam.tsheet;
				//this.m_progNo = progNo;
				this.m_ncnam = ncnam;
				this.m_tdatNo = tdatNo;
				this.m_matchNo = matchNo;

				this.m_outnam = ncnam.Tdat[tdatNo].Oname(ncnam.Tdat[tdatNo].matchK[matchNo]);

				//string tsCAM = ncnam.tdat[tdatNo].tld.tsetCAM_Name;
				tolInfo = new NcdTool.ToolSetInfo(ncnam.Tdat[tdatNo].Tld.XmlT.SNAME, ncnam.Tdat[tdatNo].matchK[matchNo].K2.Tlgn);

				// /////////////////////////////////////////////////////////////////////////////////////////////
				// これはToolSetInfo を正しく更新すれば不要になる
				// /////////////////////////////////////////////////////////////////////////////////////////////
				// 上記最小突出し量が標準より長い場合は、非標準のツールセットとして処理する
				// （代替可能なツールセットがない場合）
				NcdTool.Tool.Tol tol = ncnam.Tdat[tdatNo].matchK[matchNo].K2.Tlgn;
				if (tol.Toolset.ToutMatch(tol.Ttsk) == false) {
					// ///////////////////////////////////////////////////////////////////////////////////////
					// すでにmatchTは非標準になっている。こちらを使うべきで工具データを変更すべきでないのでは
					// ///////////////////////////////////////////////////////////////////////////////////////
					tol.SetTemporary(tol.Ttsk);
					LogOut.CheckCount("_NcOutput_NcToolL 184", false, "非標準に設定");
					if (tol.Toolset.tset_name != tol.matchT.Tset.tset_name)
						LogOut.CheckCount("_NcOutput_NcToolL 186", false, "エラー。非標準にセット" + $" Tol = {tol.Toolset.tset_name} MatchT = {tol.matchT.Tset.tset_name}");
				}
				else {
					if (tol.Toolset.tset_name != tol.matchT.Tset.tset_name)
						LogOut.CheckCount("_NcOutput_NcToolL 192", false, "エラー。標準のデータ" + $" Tol = {tol.Toolset.tset_name} MatchT = {tol.matchT.Tset.tset_name}");
				}

				// 以降は後から作成し入力
				//tmpLifeRate = 1.0;
				index = null;
			}

			public NcToolL Clone() {
				NcToolL ncTool = new NcToolL(this.Ncnam, this.TdatNo, this.MatchNo, this.nknum);
				return ncTool;
			}
		}
	}
}
