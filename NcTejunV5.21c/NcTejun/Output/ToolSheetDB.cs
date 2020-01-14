using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Data.SqlClient;

namespace NcTejun.Output
{
	partial class ToolSheetDB
	{
		private string k;
		private int i;
		private string a;
		private string m;
		private string s;
		private string b;
		private string z;

		/// <summary>マガジン数</summary>
		private int tMAX;

		private TejunSet.ToolSheet tsheet;
		private List<Tsdata> tsdList;

		/// <summary>
		/// 工具表の情報を作成
		/// </summary>
		/// <param name="tsheet"></param>
		/// <param name="same">同一工具番号の工具情報を統一</param>
		public ToolSheetDB(TejunSet.ToolSheet tsheet, bool same) {
			string sdat;
			this.tsheet = tsheet;

			k = NcdTool.Tejun.TejunName;
			i = NcdTool.Tejun.Uid;
			a = NcdTool.Tejun.Seba;
			m = NcdTool.Tejun.Mach.name;
			s = tsheet.TolstName;
			sdat = "";
			foreach (NcdTool.NcName.NcNam nnam in NcdTool.Tejun.NcList.NcNamsTS_NcExist(tsheet.TolstName)) {
				if (nnam.Ncdata.ncInfo.xmlD.BUHIN_NAME.Length == 0) continue;
				if (sdat.Length == 0) {
					sdat = nnam.Ncdata.ncInfo.xmlD.BUHIN_NAME;
					continue;
				}
				if (sdat == nnam.Ncdata.ncInfo.xmlD.BUHIN_NAME) continue;

				if (nnam.Ncdata.ncInfo.xmlD.BUHIN_NAME.Length < sdat.Length)
					sdat = sdat.Substring(0, nnam.Ncdata.ncInfo.xmlD.BUHIN_NAME.Length);
				for (int kk = 0; kk < sdat.Length; kk++)
					if (sdat[kk] != nnam.Ncdata.ncInfo.xmlD.BUHIN_NAME[kk]) {
						sdat = sdat.Substring(0, kk);
						break;
					}
				if (sdat.Length == 0)
					break;
			}
			b = sdat;
			z = NcdTool.Tejun.NcList.Mzais;
			tMAX = NcdTool.Tejun.Mach.Nmgz;


			// ////////////////////
			// 新しいtsdataの作成
			// ////////////////////
			tsdList = new List<Tsdata>();
			List<Tsdata> tsdList2 = tsdList;
			foreach (NcdTool.Tool.Tol sad in tsheet.Tols) {
				if (sad.matchT == null) continue;
				if (tMAX < sad.matchT.Tnum) tMAX = sad.matchT.Tnum;
				if (sad.matchT.SnumList.Count == 0)
					// ＮＣデータとマッチングしなかった工具
					tsdList2.Add(new Tsdata(sad, sad.matchT.SnumT, same));
				else {
					// ＮＣデータとマッチングした工具
					foreach (int iis in sad.matchT.SnumList)
						tsdList2.Add(new Tsdata(sad, iis, same));
				}
			}
			tsdList2.Sort((t1, t2) => {
				if (t1.snum != t2.snum) return t1.snum - t2.snum;
				if (t1.tnum != t2.tnum) return t1.tnum - t2.tnum;
				if (t1.kakoJun != t2.kakoJun) return t1.kakoJun - t2.kakoJun;
				return 0;
			});
		}

		/// <summary>
		/// GENERAL ポストの工具表作成
		/// </summary>
		public void Tool_out_tdb() {

			if (tsdList.Count == 0) return;

			// ///////////////////////
			// TOOL_SHEET_HEAD の作成
			// ///////////////////////
			//adapter_TOOL_SHEET_HEAD.Fill(TSSet, "TOOL_SHEET_HEAD");	delete 2018/07/16
			DataRow dRow_HEAD = TS_Head(this.tsheet, tsdList.Count);
			if (dRow_HEAD == null) return;	// すでに存在するが上書きしない場合

			// ///////////////////////
			// TOOL_SHEET の作成
			// ///////////////////////
			TS_Tool(dRow_HEAD, tsdList);
			//MessageBox.Show("TOOL_SHEET.Rows=" + TSSet.Tables["TOOL_SHEET"].Rows.Count.ToString());
		}

		/// <summary>
		/// 表示用工具表の作成new
		/// </summary>
		/// <returns></returns>
		public System.Text.StringBuilder Tool_out_dsp() {
			System.Text.StringBuilder gsetText = new StringBuilder();
			List<string> ksnam = new List<string>();

			gsetText.AppendLine("Pag: T : TOOL NAME  : DIAM :  S  :  F  : FUKASA: D :Tim:Tsu:HOLDER:");
			gsetText.AppendLine("---:---:------------:------:-----:-----:-------:---:---:---:------:");
			foreach (ToolSheetDB.Tsdata tsd in tsdList) {
				gsetText.AppendLine(tsd.Tsout('T'));
			}
			return gsetText;
		}

		/// <summary>
		/// エクセル貼り付け用ページ分割された工具表の作成new
		/// </summary>
		/// <returns></returns>
		public string Tool_out_exl() {

			int PMAX = 30;	// １ページに表示する工具数
			if (tsdList.Count == 0) return null;

			// /////////////////
			// シート情報の作成
			// /////////////////
			List<Sheet> sheet = new List<Sheet>();
			foreach (ToolSheetDB.Tsdata str in tsdList) {
				int index = -1;
				Sheet stmp;
				for (int ii = 0; ii < sheet.Count; ii++)
					if (sheet[ii].snum == str.snum) index = ii;
				if (index < 0) {
					sheet.Add(new Sheet(str.snum));
					index = sheet.Count - 1;
				}
				stmp = sheet[index].Maxmin(str.tnum);
				sheet.RemoveAt(index);
				sheet.Insert(index, stmp);
			}
			if (sheet.Count == 0) {
				MessageBox.Show(" ERROR WAS OCCRED in page-no. serch !!");
				return null;
			}

			// ///////////////////
			// ページ情報の作成
			// ///////////////////
			// ptmp[0]	ページ数
			// ptmp[1]	１ページあたりのシート数（TMAX <= PAMX）
			// ptmp[2]	１シートあたりのページ数（TMAX >  PAMX）
			int[] pCnt = new int[] { 999999, 1, 1 };
			pCnt[2] = (tMAX + PMAX - 1) / PMAX;
			if (tMAX > PMAX) {
				pCnt[1] = 1;
				pCnt[0] = sheet.Count * pCnt[2];
			}
			else {
				pCnt[1] = PMAX / tMAX;
				pCnt[0] = (sheet.Count + pCnt[1] - 1) / pCnt[1];
			}

			// ///////////////
			// メイン　ループ
			// ///////////////
			List<string> tmpA = new List<string>();
			List<string> tmpX = new List<string>();

			int sno = 0;	// シートsheet の番号
			for (int pno = 1; pno <= pCnt[0]; pno++) {
				// tcnt[1] : 開始工具番号
				// tcnt[2] : 終了工具番号
				int[] tcnt = new int[3] { 9999, 0, 0 };

				if (tMAX > PMAX) {
					// マガジン数が１ページ以上の場合
					tcnt[2] = PMAX * (pno - pCnt[2] * sno);
					tcnt[1] = tcnt[2] - PMAX + 1;
					if (tcnt[2] > tMAX) tcnt[2] = tMAX;

					// 最大工具番号より大きい場合は出力しない add 2008/10/29
					if (sheet[sno].stmax < tcnt[1]) {
						if (tcnt[2] == tMAX) sno += 1;
						continue;
					}
					// 最小工具番号より小さい場合は出力しない add 2008/10/29
					if (sheet[sno].stmin > tcnt[2]) {
						if (tcnt[2] == tMAX) sno += 1;
						continue;
					}
				}
				else {
					tcnt[1] = 1;
					tcnt[2] = tMAX;
				}

				// //////////////////////////////
				// １ページ内のシート数のループ
				// //////////////////////////////
				for (int jj = 0; jj < pCnt[1]; jj++) {
					if (sno >= sheet.Count) break;
					List<ToolSheetDB.Tsdata> tmpT = new List<ToolSheetDB.Tsdata>();
					tmpT.Clear();
					foreach (ToolSheetDB.Tsdata str in tsdList)
						if (str.snum == sheet[sno].snum)
							tmpT.Add(str);

					tmpX.Clear();
					{
						int ltmp = 0;
						if (tMAX > 1) while (ltmp < tmpT.Count && tcnt[1] > tmpT[ltmp].tnum) ltmp++;

						for (int tno = tcnt[1]; tno <= tcnt[2]; tno++) {
							if (ltmp >= tmpT.Count) {
								tmpX.Add("P" + sheet[sno].snum.ToString() + "  T" + tno.ToString("00") + " & & & & & & & & &");
								continue;
							}

							if (tno > (tMAX > 1 ? tmpT[ltmp].tnum : 1))
								throw new Exception("wefbaerfbqarehbfer");
							if (tno < (tMAX > 1 ? tmpT[ltmp].tnum : 1)) {
								tmpX.Add("P" + sheet[sno].snum.ToString() + "  T" + tno.ToString("00") + " & & & & & & & & &");
								continue;
							}

							// /////////////////////////
							// tno == tmpT[ltmp].tnum
							// /////////////////////////

							//	同じ工具番号の工具情報を追加
							string ttol = "";
							int ltmpnext = ltmp + 1;
							while (ltmpnext < tmpT.Count) {
								if (tno != tmpT[ltmpnext].tnum) break;
								ttol += "-" + tmpT[ltmpnext].Tset.ToolName;
								ltmpnext++;
							}
							if (ttol != "") ttol = " " + ttol.Substring(1);

							// 出力
							tmpX.Add(tmpT[ltmp].Tsout('t') + ttol);
							// 次の工具番号の開始位置ltmp セット
							ltmp = ltmpnext;
						}
					}

					if (tmpX.Count == 0)
						MessageBox.Show(pno + " PAGE empty");
					else {
						for (int ii = 0; ii < tmpX.Count; ii++)
							tmpA.Add(tmpX[ii]);
					}
					if (tcnt[2] == tMAX) sno += 1;
				}
			}

			// エクセルのコピペ用データの作成と表示
			if (tmpA.Count != 0) {
				string outtxt = "";
				for (int ii = 0; ii < tmpA.Count; ii++)
					outtxt += tmpA[ii] + "\n";
				return outtxt;
			}
			else
				return null;
		}

		/// <summary></summary>
		private readonly struct Sheet
		{
			public readonly int snum;
			public readonly int stmax;
			public readonly int stmin;

			public Sheet(int snum) {
				this.snum = snum;
				this.stmax = 0;
				this.stmin = 9999;
			}
			private Sheet(int snum, int stmax, int stmin) {
				this.snum = snum;
				this.stmax = stmax;
				this.stmin = stmin;
			}

			public Sheet Maxmin(int tnum) {
				return new Sheet(this.snum, Math.Max(this.stmax, tnum), Math.Min(this.stmin, tnum));
			}
		}

		/// <summary>
		/// データベース工具表１行の情報[不変(Listあり)]
		/// </summary>
		private readonly struct Tsdata
		{
			/// <summary>消耗率</summary>
			public double Consume_rate { get { return Math.Round(10.0 * m_consume_rate, MidpointRounding.AwayFromZero) / 10.0; } } private readonly double m_consume_rate;

			// 以上、2014/12/24 追加


			/// <summary>シート番号</summary>
			public readonly int snum;
			/// <summary>Ｔ番号</summary>
			public readonly int tnum;

			public readonly bool perm_tool;

			/// <summary>ツールセットＩＤ（ＦＴＮ番号）</summary>
			public readonly string toolset_ID;

			/// <summary>ＮＣデータ数</summary>
			public readonly short nnum;

			/// <summary>ツールセット情報</summary>
			public CamUtil.ToolSetData.ToolSet Tset { get { return m_tset; } } private readonly CamUtil.ToolSetData.ToolSet m_tset;

			/// <summary>突出し量</summary>
			public readonly double ttsk;
			/// <summary>深さ</summary>
			public readonly double? zchi;

			/// <summary>加工時間</summary>
			public int Ntim {
				get {
					if (m_ntim == 0.0) return 0;
					int itmp = (int)Math.Round(m_ntim);
					if (itmp == 0) itmp++;
					return itmp;
				}
			}
			private readonly double m_ntim;

			/// <summary>出力ＮＣデータ名リストの数（最大文字数１００）</summary>
			public int NnamCount { get { return m_nnamList.Count; } }
			/// <summary>
			/// 出力ＮＣデータ名リスト
			/// </summary>
			/// <param name="maxData">最大ＮＣデータ表示数</param>
			/// <returns></returns>
			public string Nnam(int maxData) {
				string sout = "";
				int cnt = 0;
				foreach (string ntmp in m_nnamList) {
					if (sout.Length + ntmp.Length + 1 > 100) break;
					if (sout != "") sout += "-";
					sout += ntmp;
					if (++cnt == maxData) break;
				}
				return sout;
			}
			private readonly List<string> m_nnamList;

			/// <summary>回転数（異なる場合は null）</summary>
			public readonly int? schi;
			/// <summary>送り速度（異なる場合は null）</summary>
			public readonly int? fchi;
			/// <summary>Ｄ番号</summary>
			public int Dnum { get { return tnum + NcdTool.Tejun.Mach.ncCode.DIN; } }
			/// <summary>ホルダー管理名</summary>
			public readonly string hld_knri;
			/// <summary>加工順 add in 2016/09/16</summary>
			public readonly int kakoJun;

			/// <summary>
			/// コンストラクタnew（ＧＥＮＥＲＡＬ用）
			/// </summary>
			/// <param name="tsd0">工具</param>
			/// <param name="p_snum">シート番号</param>
			/// <param name="same">同一工具番号の工具情報を統一</param>
			public Tsdata(NcdTool.Tool.Tol tsd0, int p_snum, bool same) {

				//　異なる工具でも同じツールセットを出力している

				this.snum = p_snum;
				this.tnum = tsd0.matchT.Tnum;
				this.perm_tool = tsd0.Perm_tool;
				// ツールセットＩＤは突出し量により変更されるので必ずmatchTから取得する
				this.toolset_ID = tsd0.matchT.Tset.ID;

				if (same) {
					this.m_tset = tsd0.matchT.Tset;
					this.hld_knri = tsd0.matchT.Hld_kanri();		// ホルダー管理
					this.schi = tsd0.matchT.Schi();				// 回転数（すべて一致しない場合はnull）
					this.fchi = tsd0.matchT.Fchi();				// 送り速度（すべて一致しない場合はnull）
					this.m_ntim = tsd0.matchT.Ntim();				// 加工時間
					this.nnum = (short)tsd0.matchT.NnamList().Count;
					this.m_nnamList = new List<string>(tsd0.matchT.NnamList());
					this.kakoJun = tsd0.matchT.KakouJun();		// 加工順
				}
				else {
					this.m_tset = tsd0.Toolset;
					this.hld_knri = tsd0.matchT.Hld_kanri(tsd0, p_snum);		// ホルダー管理
					this.schi = tsd0.matchT.Schi(tsd0, p_snum);				// 回転数（すべて一致しない場合はnull）
					this.fchi = tsd0.matchT.Fchi(tsd0, p_snum);				// 送り速度（すべて一致しない場合はnull）
					this.m_ntim = tsd0.matchT.Ntim(tsd0, p_snum);				// 加工時間
					this.nnum = (short)tsd0.matchT.NnamList(tsd0, p_snum).Count;
					this.m_nnamList = new List<string>(tsd0.matchT.NnamList(tsd0, p_snum));
					this.kakoJun = tsd0.matchT.KakouJun(tsd0, p_snum);		// 加工順
				}
				this.zchi = tsd0.matchT.Zchi;
				this.ttsk = Math.Max(tsd0.Perm_tool ? tsd0.Toolset.ToutLength : tsd0.matchT.Umax, tsd0.Ttsk);

				m_consume_rate = tsd0.Consumption_new;
			}

			/// <summary>
			/// １つの工具のデータ表示
			/// </summary>
			/// <param name="copt">オプション t or T</param>
			/// <returns>出力文字列</returns>
			public string Tsout(char copt) {
				StringBuilder gsetText = new StringBuilder();

				gsetText.Append("P" + this.snum.ToString().PadRight(2));
				if (NcdTool.Tejun.Mach.Nmgz > 1 || copt != 't')
					gsetText.Append(" T" + this.tnum.ToString("00"));
				else
					gsetText.Append(" P" + this.snum.ToString("00"));
				gsetText.Append(" " + this.Tset.ToolName.PadRight(12));		// 統一工具名を表に追加
				gsetText.Append(" " + this.Tset.Diam.ToString("0.00").PadLeft(6));
				gsetText.Append(this.schi.HasValue ? " " + this.schi.Value.ToString().PadLeft(5) : "     &");
				gsetText.Append(this.fchi.HasValue ? " " + this.fchi.Value.ToString().PadLeft(5) : "     &");
				gsetText.Append(this.zchi.HasValue ? " " + this.zchi.Value.ToString("0.00").PadLeft(7) : "       &");
				gsetText.Append(" " + this.Dnum.ToString().PadLeft(3));
				gsetText.Append(" " + this.Ntim.ToString("0.").PadLeft(3));
				gsetText.Append(" " + this.ttsk.ToString("0.").PadLeft(3));
				gsetText.Append(" " + this.Tset.HolderName);
				gsetText.Append(this.NnamCount > 0 ? " " + this.Nnam(6) : " &");
				return gsetText.ToString();
			}
		}
	}
}
