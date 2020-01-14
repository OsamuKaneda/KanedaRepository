using System;
using System.Collections.Generic;
using System.Text;

namespace NcdTool.TMatch
{
	class MatchK
	{
		/// <summary>マッチング方法のオプションの設定</summary>
		internal readonly struct Option
		{
			/// <summary>シート間で工具寿命を整合させ工具番号を揃える場合 true。 寿命を考慮するように変更 2016/02/11</summary>
			static public bool TNo_Match = false;
		}



		/// <summary>同一工具単位情報内の加工順</summary>
		public readonly int no;

		/// <summary>工具単位ＮＣ情報へのリンク</summary>
		public readonly NcName.Kogu kogu;
		/// <summary>部品加工機の自動寿命分割情報</summary>
		public readonly TMatch.Divide.DivData divData;
		/// <summary>減速考慮切削加工時間(min)</summary>
		public double Rtim { get { return divData.ncdist.CuttingTime * kogu.CutTimeRate; } }
		/// <summary>減速考慮G01時間(min)</summary>
		public double Ntim { get { return divData.ncdist.FeedTime * kogu.CutTimeRate; } }
		/// <summary>減速考慮全加工時間(min)</summary>
		public double Atim { get { return Ntim + divData.ncdist.NonFeedTime(Tejun.Mach.Rapx, Tejun.Mach.Rapz); } }







		/// <summary>マッチングにより決定したシート番号（ＮＣ）cf. matchT.snumT</summary>
		public int SnumN { get { return m_snumN; } }	private readonly int m_snumN;

		/// <summary>
		/// この工具交換で新たなＮＣデータを作成する場合true
		/// true になる場合
		/// １．テキサスの場合
		/// ２．ＮＣデータの最初の工具
		/// ３．シート番号が変わる
		/// ４．tmch.tlgn.bnum != 0 工具で分割指示がある場合（≒分割ルール）
		/// </summary>
		public bool Ochg { get { return (Tejun.Mach.Toool_nc) ? true : m_ochg; } }	private readonly bool m_ochg;

		/// <summary>M01 挿入</summary>
		public readonly bool m01;

		/// <summary>すべてのマッチングが終了してから決定される情報</summary>
		public MatchK2 K2;


	
		/// <summary>
		///  マッチング処理をするコンストラクタ
		/// </summary>
		/// <param name="ii">同一工具単位ＮＣデータ内の加工順</param>
		/// <param name="nowKogu">現在マッチしたＮＣ情報</param>
		/// <param name="divd">自動寿命分割の情報</param>
		/// <param name="preMtch">直前に工具が決定したマッチング情報</param>
		/// <param name="koho">工具</param>
		/// <param name="snum">シート番号</param>
		/// <param name="rule"></param>
		public MatchK(int ii, NcName.Kogu nowKogu, TMatch.Divide.DivData divd, MatchK preMtch, Tool.Tol koho, int snum, TSRule rule) {

			// 加工順
			this.no = ii;
			// 工具単位ＮＣ情報
			this.kogu = nowKogu;
			// 自動寿命分割情報
			this.divData = divd;

			// //////////////////////////////////
			// シートの設定
			this.m_snumN = snum;

			// //////////////////////////////////
			// ＮＣデータの工具ごとの分割の設定
			if (preMtch == null || nowKogu.Parent.nnam != preMtch.kogu.Parent.nnam || nowKogu.Parent.Jnno.same1 == preMtch.kogu.Parent) {
				this.m_ochg = true;
			}
			else if (snum != preMtch.SnumN || koho.Bnum == true) {
				this.m_ochg = true;
				if (snum == preMtch.SnumN && koho.Bnum != true)
					CamUtil.LogOut.warn.AppendLine($"Message === AUTO DIVIDE ( {nowKogu.Parent.nnam} {nowKogu.TsetCHG.Tset_name} )");
			}
			else
				this.m_ochg = false;

			// //////////////////////////////////
			// Ｍコード出力の設定
			this.m01 = koho.EdtM001;
			if (rule.iRule_m01 && koho.EdtM001 == false) {
				System.Windows.Forms.MessageBox.Show($"標準で必要な M01 が設定されていない。in {nowKogu.TsetCHG.Tset_name} {nowKogu.Parent.nnam}");
			}

			// //////////////////////////////////
			// 高速スピンドルの設定
			if (nowKogu.TsetCHG.Hsp != koho.Hsp) throw new Exception("PROGRAM ERROR in MatchK");

			// /////////////////////////////////////////////////////////////////////////////
			// 工具表へのリンクなど
			// 工具番号が等しい工具はここで工具を設定しないと区分できないため。 2015/02/19
			// /////////////////////////////////////////////////////////////////////////////
			this.K2 = new MatchK2(koho);

			// メッセージ
			if (koho.Tmod == '2') {
				if (rule.Match(koho, "bun") == false)
					System.Windows.Forms.MessageBox.Show("分割のルールが工具と一致していない。");
				if (rule.Match(koho, "m01") == false)
					System.Windows.Forms.MessageBox.Show("M01挿入ルールが工具と一致していない。");
			}
		}



		/// <summary>
		/// すべてのマッチングが終了してから決定される情報
		/// </summary>
		internal class MatchK2
		{
			/// <summary>
			/// 工具 tlgn をセットする
			/// </summary>
			/// <param name="tolstname"></param>
			/// <param name="tolArray"></param>
			static public void Set_Tool(string tolstname, NcdTool.Tool.Tol[] tolArray) {
				List<Tool.Tol> tols;
				foreach (NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsTS(tolstname))
					foreach (NcName.Kogu skog in ncnam.Tdat)
						foreach (TMatch.MatchK mchk in skog.matchK) {
							tols = MatchT.ListTol_MatchK(mchk, tolArray);
							if (tols.Contains(mchk.K2.ttmp))
								mchk.K2.m_tlgn = mchk.K2.ttmp;
							else if (mchk.K2.ttmp.Tmod == '2' && tols.Count == 1) {
								// 複数の同一ツールセット工具が追加され、突出し量調整された場合
								mchk.K2.m_tlgn = tols[0];
								if (mchk.K2.m_tlgn.Tmod != '2')
									throw new Exception("	qefdbqefdbqheb");
								if (mchk.K2.m_tlgn.Toolsetname != mchk.K2.ttmp.Toolsetname)
									throw new Exception("	qefdbqefdbqheb");
							}
							else
								throw new Exception("jwefdbqwebfhb");
						}
			}

			/// <summary>
			/// すべての工具表のＯ番号を決める（一部の工具表の変更が他にも波及する）
			/// </summary>
			static public void Set_Onum() {
				int? oNow = null;
				int? oInc = null;
				bool stt;
				foreach (NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll) {
					if (ncnam.nmgt.OnumS.HasValue) {
						oNow = ncnam.nmgt.OnumS.Value;
						oInc = ncnam.nmgt.OnumI.Value;
						oNow -= oInc;
					}
					stt = true;
					foreach (NcName.Kogu skog in ncnam.Tdat) {
						//if (skog.tsetCHG.tset_name == null) continue;
						if (skog.TsetCHK == false) continue;
						foreach (MatchK mchk in skog.matchK) {
							if (ncnam.Jnno.Nknum == null) {
								if (mchk.Ochg) {
									stt = false;
									oNow += oInc;
									if (oNow == 10000) oNow++;
								}
								if (stt) throw new Exception("ＮＣデータの最初の工具のochgがtrueではない");
								mchk.K2.m_onum = oNow;
							}
							else {
								mchk.K2.m_onum = ncnam.Jnno.Nknum.Tdat[skog.TNo].matchK[mchk.no].K2.Onum;
							}
						}
					}
				}
			}

			/// <summary>
			/// 次工具の番号を決定する
			/// </summary>
			static public void Set_NextToolNo() {
				MatchK prem = null;
				foreach (NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll)
					foreach (NcName.Kogu skog in ncnam.Tdat) {
						//if (skog.tsetCHG.tset_name == null) continue;
						if (skog.TsetCHK == false) continue;
						foreach (MatchK mchk in skog.matchK) {
							mchk.K2.m_jtnum = 0;
							if (prem != null)
								if (prem.SnumN == mchk.SnumN)
									prem.K2.m_jtnum = mchk.K2.Tnum;
							prem = mchk;
						}
					}
			}

			// ////////////////////////////
			// 以上　static
			// ////////////////////////////






			/// <summary>仮の工具</summary>
			public readonly Tool.Tol ttmp;

			/// <summary>マッチングによって決定した工具番号</summary>
			public int Tnum { get { return m_tlgn.matchT.Tnum; } }

			/// <summary>Tolへのリンク</summary>
			public Tool.Tol Tlgn { get { return m_tlgn; } }
			private Tool.Tol m_tlgn;

			/// <summary>Ｏ番号</summary>
			internal int Onum { get { return m_onum.Value; } }
			private int? m_onum;

			/// <summary>次工具の工具番号（０：は無し）</summary>
			internal int Jtnum { get { return m_jtnum.Value; } }
			private int? m_jtnum;



			/// <summary>
			/// コンストラクタ
			/// </summary>
			public MatchK2(Tool.Tol tlgn) {
				this.ttmp = tlgn;
				this.m_tlgn = null;
				this.m_onum = null;
				this.m_jtnum = null;
			}
		}


		/// <summary>
		/// 工具単位分割ルール、M01挿入ルールを管理
		/// </summary>
		public readonly struct TSRule
		{
			/// <summary>ＮＣデータの工具ごとの分割の設定</summary>	
			public readonly bool iRule_bun;
			/// <summary>ＮＣデータのＭ０１，Ｍ１００の挿入の設定</summary>	
			public readonly bool iRule_m01;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="nowKogu">判定する工具単位ＮＣ</param>
			/// <param name="preKogu">直前の加工の工具単位ＮＣ。分割された２番目以降はnowKoguと同じKoguとなる</param>
			/// <param name="allbunk">すべて分割する場合 true</param>
			public TSRule(NcName.Kogu nowKogu, NcName.Kogu preKogu, bool allbunk) {
				iRule_bun = iRule_m01 = false;

				// ＮＣデータの工具ごとの分割の設定
				iRule_bun = allbunk || TSRule0(nowKogu, preKogu);

				// ＮＣデータのＭ０１の挿入の設定
				iRule_m01 = TSRule1(nowKogu, iRule_bun ? null : preKogu);
			}

			/// <summary>
			/// 工具がルールに一致しているかを検証
			/// </summary>
			/// <param name="koho">調べる工具</param>
			/// <param name="rule_name">ルール名：bun, m01, null のいずれか。null の場合は両方を検証</param>
			/// <returns></returns>
			public bool Match(Tool.Tol koho, string rule_name) {
				bool match = true;
				if (rule_name != null && rule_name != "bun" && rule_name != "m01")
					throw new Exception("eqargfbqnrfnqh");

				if (rule_name==null || rule_name == "bun") {
					if (!Tejun.Mach.Toool_nc)
						if ((koho.Bnum == true) != iRule_bun)
							match = false;
				}
				if (rule_name == null || rule_name == "m01") {
					if (koho.EdtM001 == true && iRule_m01 == false)
						match = false;
				}
				return match;
			}

			/// <summary>
			/// 工具単位分割ルール（ true：分割要 ）
			/// </summary>
			/// <param name="nowKogu">判定する工具単位ＮＣ</param>
			/// <param name="preKogu">直前の加工の工具単位ＮＣ</param>
			private bool TSRule0(NcName.Kogu nowKogu, NcName.Kogu preKogu) {
				if (preKogu != null) {
					if (nowKogu.TsetCHG.Tset_cutter_type_caelum == "BOR")
						if (preKogu.TsetCHG.Tset_cutter_type_caelum != "BOR")
							return true;
					if (nowKogu.TsetCHG.Tset_cutter_type_caelum == "TAP" || nowKogu.TsetCHG.Tset_cutter_type_caelum == "REM")
						if (
						preKogu.TsetCHG.Tset_tool_name != "AIRBLOW" &&
						preKogu.TsetCHG.Tset_cutter_type_caelum != "TAP" &&
						preKogu.TsetCHG.Tset_cutter_type_caelum != "REM")
							return true;
				}
				return false;
			}

			/// <summary>
			/// M01挿入ルール（true：挿入要）
			/// </summary>
			/// <param name="nowKogu">判定する工具単位ＮＣ</param>
			/// <param name="preKogu">直前の加工の工具単位ＮＣ</param>
			private bool TSRule1(NcName.Kogu nowKogu, NcName.Kogu preKogu) {
				if (preKogu != null)
					if (nowKogu == nowKogu.Parent.Tdat[0])
						preKogu = null;

				if (nowKogu.TsetCHG.Tset_cutter_type_caelum == "BOR") {
					if (preKogu == null)
						return true;
					if (preKogu.TsetCHG.Tset_cutter_type_caelum != "BOR")
						return true;
				}
				/* 廃止 2018/05/07
				else if (nowKogu.tsetCHG.tset_cutter_type_caelum == "TAP" || nowKogu.tsetCHG.tset_cutter_type_caelum == "REM") {
					if (preKogu == null)
						return true;
					if (
					preKogu.tsetCHG.tset_tool_name != "AIRBLOW" &&
					preKogu.tsetCHG.tset_cutter_type_caelum != "TAP" &&
					preKogu.tsetCHG.tset_cutter_type_caelum != "REM")
						return true;
				}
				*/
				return false;
			}
		}
	}
}
