using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NcdTool.TMatch
{
	/// <summary>
	/// マガジン内の工具情報。ここで MatchT 情報の元も作成する。
	/// マガジン数より大きい常時セット工具の番号に対応した in 2016/2/16
	/// （その他の番号は小さい順に割り当てるためマガジン数以下である）
	/// </summary>
	class SheetData
	{
		/// <summary>
		/// 工具番号の最大値がマガジン数を超えている場合０以上となる。ただし、常時セット工具を除く。
		/// （マガジン数が今回使用する加工機より多い加工機で工具表を作成した場合に再作成を不要にする処置）
		/// </summary>
		private int exmach;

		/// <summary>このシートに設定されたマッチデータ</summary>
		private List<MatchT> matchTList;

		/// <summary>使用する加工機のマガジン数</summary>
		private readonly int nmgz;
		/// <summary>ポット番号に関わらず自由に工具番号を決められる加工機かどうか</summary>
		private readonly bool free;

		/// <summary>現在のＮＣデータで使用しているシート番号</summary>
		public int ShtNo_N { get { return m_shtNo_N; } }
		private int m_shtNo_N;
		/// <summary>現在検索中の工具表のシート番号</summary>
		public int ShtNo_T { get { return m_shtNo_T; } }
		private int m_shtNo_T;

		/// <summary>これ以下にはマッチさせない工具番号のあるシート番号</summary>
		private readonly int initS;
		/// <summary>これ以下にはマッチさせない工具番号</summary>
		private readonly int initT;

		/// <summary>
		/// シート情報を初期化する
		/// </summary>
		/// <param name="nmgz"></param>
		/// <param name="initS"></param>
		/// <param name="initT"></param>
		public SheetData(int nmgz, int initS, int initT) {
			this.nmgz = nmgz;
			this.free = Tejun.Mach.FreeTNo;
			this.m_shtNo_N = 0;
			this.m_shtNo_T = 0;
			this.matchTList = new List<MatchT>();

			this.exmach = 0;
			this.initS = initS;
			this.initT = initT;
		}

		/// <summary>
		///  開始時のツールシート番号を設定する
		/// </summary>
		/// <param name="tsn"></param>
		/// <param name="tols">使用する工具リスト</param>
		public void ShokiSet(int tsn, Tool.Tol[] tols) {
			if (tsn == 0)
				NextSheet(m_shtNo_T + 1, tols);
			else if (m_shtNo_T != tsn)
				NextSheet(tsn, tols);
		}

		/// <summary>
		///  この工具を使用するためのシート番号、工具番号を設定する
		/// </summary>
		/// <param name="koho"></param>
		/// <param name="nowKogu">現在マッチしたＮＣ情報</param>
		/// <param name="preTol">ひとつ前のシート内でマッチする工具</param>
		/// <param name="tols">使用する工具リスト</param>
		public MatchT KohoSheetSet(Tool.Tol koho, NcName.Kogu nowKogu, Tool.Tol preTol, Tool.Tol[] tols) {
			MatchT mtmp;

			if (koho.Tmod != '1' && koho.Rnum > m_shtNo_T) {
				NextSheet(koho.Rnum, tols);	// ***新たなシートに移る***（使用すべき工具のシート番号が大きい）
				preTol = null;
			}
			while ((mtmp = Set_Match(koho, nowKogu, preTol)) == null)
				NextSheet(null, tols);		// ***新たなシートに移る***（空きが無いため追加できなかった）
			//System.Windows.Forms.MessageBox.Show(shtNo_N.ToString() + "  " + mtmp.tnum.ToString());
			return mtmp;
		}



		/// <summary>
		/// シート情報を作成する。工具番号が確定しているマッチング情報も作成
		/// </summary>
		/// <param name="toolSheetNo">工具表のシート番号</param>
		/// <param name="tols">使用する工具リスト</param>
		private void NextSheet(int? toolSheetNo, Tool.Tol[] tols) {

			// ＮＣデータのシート番号のみ増加させる場合の判定
			// （まだ未使用の工具が残っている場合にＮＣデータのシート番号のみ増加させる）
			if (toolSheetNo.HasValue == false && exmach > 0) {
				bool samet = false;
				foreach (Tool.Tol sad in tols) {
					if (sad.Rnum != m_shtNo_T) continue;
					if (sad.matchT != null) if (sad.matchT.KoguCount > 0) continue;
					samet = true;
					break;
				}
				if (samet) exmach++;
				else exmach = 0;
			}

			// 工具表の次のシート番号に移動
			if (toolSheetNo.HasValue || exmach == 0) {
				// 工具表のシート番号が指定された場合
				if (toolSheetNo.HasValue) {
					if (toolSheetNo.Value <= this.m_shtNo_T) throw new Exception("qwefbdqhfrbh");
					this.m_shtNo_T = toolSheetNo.Value;
				}
				else
					this.m_shtNo_T++;

				// 工具表と加工機マガジン数が一致するかチェック
				exmach = 0;
				if (free) {
					// 工具番号の番号の種類がマガジン数より小さいか
					List<int> ilist = new List<int>();
					foreach (Tool.Tol sad in Tejun.Mach.Mgrs) {
						if (sad.Unum <= 0) continue;
						if (ilist.IndexOf(sad.Unum) < 0) ilist.Add(sad.Unum);
					}
					foreach (Tool.Tol sad in tols) {
						if (sad.Rnum != m_shtNo_T) continue;
						if (sad.Tmod != '0') continue;
						if (ilist.IndexOf(sad.Unum) < 0) ilist.Add(sad.Unum);
						if (ilist.Count <= nmgz) continue;
						CamUtil.LogOut.warn.AppendLine(
							"マガジン数より多くの工具番号が S" + sad.Rnum.ToString() + " で見つかりました。シートは自動で分割されます。");
						exmach = 1;
						break;
					}
				}
				else {
					// 工具番号の番号の最大値がマガジン数より小さいか
					foreach (Tool.Tol sad in tols) {
						if (sad.Rnum != m_shtNo_T) continue;
						if (sad.Tmod != '0') continue;
						if (sad.Unum <= nmgz) continue;
						CamUtil.LogOut.warn.AppendLine(
							"マガジン数より大きい工具番号が S" + sad.Rnum.ToString() + " で見つかりました。工具番号は自動で設定します。");
						exmach = 1;
						break;
					}
				}
			}

			// shtNo_T 以外のシート情報リセット
			this.m_shtNo_N++;
			this.matchTList.Clear();

			// このシートの工具が空で以降のシートには工具がある場合はプラスし繰返す
			if (exmach == 0)
				while (true) {
					int overSnum = 0;	// このシートＮｏ以上の工具数
					int sameSnum = 0;	// このシートＮｏの工具数
					foreach (Tool.Tol sad in tols) {
						if (sad.Tmod == '1' || sad.Unum <= 0) continue;
						// 工具番号が大きくても常時セット工具番号の場合は対象に含める ADD in 2016/02/23
						// 工具番号が大きくても構わない ADD in 2016/03/03
						// if (sad.unum > nmgz) continue;
						//if (sad.unum > nmgz && sad.unum != Mgrs_Exists(sad.toolset.tset_name, sad.ttsk)) continue;
						if (sad.Rnum > m_shtNo_T) overSnum++;
						if (sad.Rnum != m_shtNo_T) continue;

						sameSnum++;
						MatchT mtnum = Tool_Exists(sad.Unum);
						if (mtnum == null) {
							if (sad.matchT == null)
								sad.matchT = new MatchT(sad.Toolset, TPrio.SET_TOOLSHEET, m_shtNo_T, sad.Unum, sad.Initial_consumpt, sad.Perm_tool);
							matchTList.Add(sad.matchT);
						}
						else {
							// ///////////////////////////////////////
							// 同じ工具番号の工具が複数ある場合の処理
							// ///////////////////////////////////////
							if (sad.matchT == null) {
								if (SameTNo(mtnum.Tset, sad.Toolset) == false)
									throw new Exception($"シート番号{sad.Rnum.ToString("00")} 工具番号{sad.Unum.ToString("00")} で異なる工具形状あるいはホルダーが指定された。");
								// 突出し量が長い場合は長い方のツールセットに入れ替える
								if (mtnum.Tset.ToutLength < sad.Toolset.ToutLength) {
									if (mtnum.Perm_tool)
										throw new Exception("常時セット工具に異なる工具が追加された。");
									mtnum.Tset = sad.Toolset;
								}
								sad.matchT = mtnum;
								// 初期消耗率を追加する
								mtnum.Add_Consumpt(sad.Initial_consumpt);
							}
							else {
								if (sad.matchT != mtnum)
									throw new Exception("fvwefvwfvjwfnvj");
							}
						}
					}

					if (overSnum <= 0 || sameSnum != 0) break;
					this.m_shtNo_T++;
					this.m_shtNo_N++;
				}

			// パーマネント工具をダミーで入れておく。
			foreach (Tool.Tol sad in Tejun.Mach.Mgrs)
				if (sad.Unum > 0)
					if (Tool_Exists(sad.Unum) == null) {
						matchTList.Add(new MatchT(sad.Toolset, TPrio.SET_PERMANENT, m_shtNo_T, sad.Unum, 0.0, true));
					}
			// リナンバーのための使用できない工具を設定
			// 間違いを修正する 2016/01/22
			//if (m_shtNo_N <= initS)
			//	for (int tt = 1; tt <= initT; tt++)
			//		if (matchTList[tt] == null) matchTList[tt] = MatchT.Dummy;
			if (m_shtNo_N < initS)
				for (int tt = 1; matchTList.Count < nmgz; tt++)
					if (Tool_Exists(tt) == null) matchTList.Add(MatchT.Dummy(tt));
			if (m_shtNo_N == initS)
				for (int tt = 1; matchTList.Count < nmgz && tt <= initT; tt++)
					if (Tool_Exists(tt) == null) matchTList.Add(MatchT.Dummy(tt));
		}

		/// <summary>
		/// 空きの番号を考慮し、工具に工具番号を与える
		/// </summary>
		/// <param name="tol">工具情報</param>
		/// <param name="skog">工具単位ＮＣデータの情報（マッチングしなかった工具の処理の場合は null）</param>
		/// <param name="preTol">ひとつ前のシート内でマッチする工具</param>
		private MatchT Set_Match(Tool.Tol tol, NcName.Kogu skog, Tool.Tol preTol) {

			// ///////////////////////////
			// 工具番号の決定のルール
			// ///////////////////////////
			// １．登録済みの工具
			//		・工具表の工具(sad.tmod == '0' && sad.unum > 0 && sad.unum <= nmgz)
			//		・前のシートですでに使われている場合その番号（exmatch > 0）
			// ２．加工機内の工具との整合した場合
			// ３．最大マガジン数以下の工具番号（工具番号がマガジン数を越えている場合）
			// ４．前のシートの同一ツールセットの番号
			// ５．空きの番号

			int tt;
			TPrio tp;
			int aki;

			// ///////////////////////////////
			// 工具番号が登録済みの工具の場合
			// ///////////////////////////////
			if (tol.matchT != null) {
				tt = tol.matchT.Tnum;
				tp = TPrio.SET_TOOLSHEET;
				MatchT mtnum = Tool_Exists(tt);

				// すでに指定の工具番号で登録済みのデータは何もしない
				if (tol.matchT == mtnum) {
					;
				}
				// 工具番号よりマガジン数が小さい場合は以下のように
				// 他の工具がすでにこの番号を使用している場合がある。
				// この場合は番号をずらして空きの番号にする。
				else {
					if (exmach == 0) throw new Exception("qefbqrfbh");
					if (mtnum == null) { ;}
					else if (PermOverWrite(tt)) { ;}
					else {
						// 候補の工具番号ttが使用中で使用できない場合
						if (mtnum.tp <= tp) return null;	// 次のシートへ
						// 最初の空き位置を検索
						aki = Tool_MinTNo();
						if (aki != Tool_MinTNo2()) throw new Exception("暫定チェック");
						if (aki == 0) return null;	// 次のシートへ
						MatchT.TnumIDO2(matchTList, aki, tt);
					}
					matchTList.Add(tol.matchT);
				}
				return Tool_Exists(tt);
			}

			// ///////////////////////////////////
			// 以下、工具番号が未登録の工具の場合
			// ///////////////////////////////////
			//
			// 工具番号決定順（ツールセットは必ず一致）
			// １．突出し量が整合しているパーマネント工具
			// ２．最初の空き番号
			//

			// ------------------------------------------------
			// 候補の工具番号を決定する
			// ------------------------------------------------
			tt = 0;
			tp = TPrio.ELSE_TOOL;
			aki = Tool_MinTNo();
			if (aki != Tool_MinTNo2()) throw new Exception("暫定チェック");

			// 加工機内の工具との整合
			if (tt == 0)
				if (skog != null) {
					tt = Mgrs_Exists(skog.TsetCHG.Tset_name, skog.Tld.XmlT.TULEN);
					if (tt > 0) tp = TPrio.SET_PERMANENT;
				}

			// //////////////////////////////////
			// 空き領域がない場合は次のシートへ
			// //////////////////////////////////
			if (tt == 0)
				if (aki == 0)
					return null;

			// 最大マガジン数以下の工具表の番号と整合（工具番号がマガジン数を越えている場合）
			// 復活 2016/03/03
			if (tt == 0)
				if (tol.Tmod == '0' && (free || tol.Unum <= nmgz)) {
					if (exmach == 0) throw new Exception("edbqfdbqhredbh");
					tt = tol.Unum;
					tp = TPrio.SET_TOOLLIST;
				}

			// 前のシートの同一ツールセットの番号との整合
			// 前のシートの工具番号に揃える機能を停止する in 2014/10/30
			if (tt == 0)
				if (preTol != null && MatchK.Option.TNo_Match) {
					if (!preTol.NoMatch && preTol.matchT.SnumN_Last + 1 == m_shtNo_N) {
						tt = preTol.matchT.Tnum;
						tp = TPrio.SAME_TOOL;
					}
				}

			// 候補がない場合、最初の空きを工具番号とする
			if (tt == 0) {
				tt = aki;
				tp = TPrio.ELSE_TOOL;
			}

			// ------------------------------------------------
			// 候補の工具番号の使用可否決定
			// ------------------------------------------------

			// 候補の工具番号が未使用の場合
			if (Tool_Exists(tt) == null) {
				;
			}
			// 候補の工具番号がまだ使用されていないパーマネント工具の場合
			else if (tp <= TPrio.SET_PERMANENT && PermOverWrite(tt)) {
				switch (tp) {
				case TPrio.SET_TOOLSHEET:
					System.Windows.Forms.MessageBox.Show("この工具番号" + tt.ToString() + "は常時セットしてある工具番号です。");
					break;
				case TPrio.SET_PERMANENT:
					break;
				default:
					throw new Exception("qerjfbqb");
				}
			}
			// 優先順位の低い工具が使用している場合
			else if (tp < Tool_Exists(tt).tp) {
				// 優先順位が最低の工具番号を順に移動して候補工具番号ttを空ける
				MatchT.TnumIDO2(matchTList, aki, tt);
			}
			// 優先順位の高い工具が使用している場合
			else {
				tt = aki;
				tp = TPrio.ELSE_TOOL;
			}

			// ------------------------------------------------
			// 情報の作成（工具へのリンクとシート・工具番号）
			// ------------------------------------------------
			if (Tool_Exists(tt) != null) matchTList.Remove(Tool_Exists(tt));	// ダミーで入っていたパーマネント工具を消去
			matchTList.Add(new MatchT(tol.Toolset, tp, m_shtNo_T, tt, tol.Initial_consumpt, tp == TPrio.SET_PERMANENT));
			return Tool_Exists(tt);
		}

		/// <summary>
		/// 候補の工具番号にまだ使用されていないパーマネント工具がセットされている場合、その工具番号を使用する
		/// </summary>
		/// <param name="tt"></param>
		/// <returns></returns>
		private bool PermOverWrite(int tt) {
			if (tt == 0) return false;
			MatchT mtnum = Tool_Exists(tt);
			if (mtnum != null)
				if (mtnum.tp == TPrio.SET_PERMANENT && mtnum.KoguCount == 0)
					return true;
			return false;
		}

		/// <summary>
		/// 同一の工具番号を認める場合はtrue
		/// </summary>
		/// <param name="tol1"></param>
		/// <param name="tol2"></param>
		/// <returns></returns>
		private bool SameTNo(CamUtil.ToolSetData.ToolSet tol1, CamUtil.ToolSetData.ToolSet tol2) {
			if (tol1.ToolFormName != tol2.ToolFormName) return false;
			if (tol1.HolderName != tol2.HolderName) return false;
			return true;
		}

		/// <summary>指定工具番号の工具を抽出</summary>
		private MatchT Tool_Exists(int tno) { foreach (MatchT mt in matchTList) if (mt.Tnum == tno) return mt; return null; }
		/// <summary>最小の空き工具番号を抽出（最大マガジン数以下の範囲。無い場合は０を返す）</summary>
		private int Tool_MinTNo() {
			if (matchTList.Count > nmgz) throw new Exception("qfqrefhnwtgwefrn");
			if (matchTList.Count == nmgz) return 0;
			for (int tno = 1; tno <= nmgz; tno++) if (Tool_Exists(tno) == null) return tno; return 0;
		}
		private int Tool_MinTNo2() {
			matchTList.Sort();
			for (int tno = 1; tno < matchTList.Count; tno++) if (matchTList[tno].Tnum == matchTList[tno - 1].Tnum) throw new Exception("同一工具番号の工具が存在する");
			if (matchTList.Count > nmgz) throw new Exception("qfqrefhnwtgwefrn");
			if (matchTList.Count == nmgz) return 0;
			for (int tno = 1; tno <= matchTList.Count; tno++) if (tno != matchTList[tno - 1].Tnum) return tno;
			return matchTList.Count + 1;
		}
		/// <summary>
		/// 常時セット工具の判断
		/// </summary>
		/// <param name="tsetname">ツールセット名</param>
		/// <param name="tout">突出し量</param>
		/// <returns>工具番号。不整合の場合は０</returns>
		private int Mgrs_Exists(string tsetname, double tout) {
			return Tejun.Mach.Mgrs.Where(sad => tsetname == sad.Toolset.tset_name && sad.Toolset.ToutMatch(tout)).Select(sad => sad.Unum).FirstOrDefault();
		}
	}
}
