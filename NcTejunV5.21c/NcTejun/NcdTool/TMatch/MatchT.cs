using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NcdTool.TMatch
{
	/// <summary>工具番号を決定する優先順位（小さい方優先）</summary>
	internal enum TPrio
	{
		/// <summary>優先順位１．工具表で決められている番号</summary>
		SET_TOOLSHEET,
		/// <summary>優先順位２．加工機に常駐している工具</summary>
		SET_PERMANENT,
		/// <summary>優先順位３．工具番号自動設定時の工具リストの番号</summary>
		SET_TOOLLIST,
		/// <summary>優先順位４．前のシートで同じ工具が使われている番号</summary>
		SAME_TOOL,
		/// <summary>優先順位５．その他の使用している工具</summary>
		ELSE_TOOL
	}

	/// <summary>
	/// 加工機マガジンに入っている１つの工具を表します。工具表とＮＣデータとのマッチングにより作成されます。
	/// 工具表の複数の工具から１つのMatchT が参照されることがあります。この工具を使用するＮＣデータのリストを保持します。
	/// </summary>
	class MatchT : IComparable<MatchT>
	{
		/// <summary>ダミー</summary>
		static public MatchT Dummy(int tno) { return new MatchT(null, TPrio.SET_TOOLSHEET, 0, tno, 0.0, false); }

		/// <summary>
		/// 優先順位が最低の工具番号を順に移動してttを空ける
		/// </summary>
		/// <param name="matchList"></param>
		/// <param name="aki">現在空いている工具番号</param>
		/// <param name="tt">空けるべき工具番号</param>
		/// <returns></returns>
		static public int TnumIDO(MatchT[] matchList, int aki, int tt) {
			int nextAki = aki;
			for (int ii = aki - 1; ii > tt; ii--) {
				if (matchList[ii].tp != TPrio.ELSE_TOOL) continue;
				// すでに他のシートで使用されている場合を除く
				if (matchList[ii].SnumList.Count > 1) continue;

				matchList[nextAki] = matchList[ii];
				matchList[nextAki].m_tnum = nextAki;
				nextAki = ii;
			}
			matchList[nextAki] = matchList[tt];
			matchList[nextAki].m_tnum = nextAki;
			nextAki = tt;
			return nextAki;
		}

		/// <summary>
		/// 優先順位が最低の工具番号を順に移動してttを空ける
		/// </summary>
		/// <param name="matchList"></param>
		/// <param name="aki">現在空いている工具番号</param>
		/// <param name="tt">空けるべき工具番号</param>
		/// <returns></returns>
		static public void TnumIDO2(List<MatchT> matchList, int aki, int tt) {
			int nextAki = aki, itmp;
			if (aki <= tt) throw new Exception("qkefbqreb");

			CamUtil.LogOut.CheckOutput(CamUtil.LogOut.FNAM.TMATCHING, Tejun.TejunName, Tejun.Mach.name,
				$"{"工具の移動"} aki={aki:d} tt={tt:d}");

			// tnumでソート
			matchList.Sort();

			for (int ii = matchList.Count - 1; ii >= 0; ii--) {
				if (matchList[ii].Tnum > aki) continue;
				if (matchList[ii].tp != TPrio.ELSE_TOOL) continue;
				// すでに他のシートで使用されている場合を除く
				if (matchList[ii].SnumList.Count > 1) continue;

				itmp = nextAki;
				nextAki = matchList[ii].Tnum;
				matchList[ii].m_tnum = itmp;
				if (nextAki == tt) break;
			}
			if (nextAki != tt) throw new Exception("qkefbqreb");
		}

		/// <summary>
		/// 同一のツールセットが複数追加された場合に、突出し量の必要な加工とそうでない加工を
		/// 区分して複数の工具に配分し、突出し量が小さくて済む工具を多く得るようにする。
		/// </summary>
		/// <param name="tols"></param>
		static public void SortTsk(Tool.Tol[] tols) {
			List<string> tsetN = new List<string>();
			List<Tool.Tol> tList = new List<Tool.Tol>();
			int kcnt, imax, imin;

			// 最大のシート番号
			int smax = 0;
			foreach (Tool.Tol koho in tols)
				if (koho.matchT != null)
					if (smax < koho.matchT.SnumT) smax = koho.matchT.SnumT;

			// シート単位のループ
			for (int snum = 1; snum <= smax; snum++) {
				tsetN.Clear();
				// ツールセット単位のループ
				foreach (Tool.Tol koho in tols) {
					if (koho.matchT == null) continue;
					if (koho.matchT.SnumT != snum) continue;
					if (tsetN.Contains(koho.Toolsetname)) continue;
					tsetN.Add(koho.Toolsetname);
					// 突出し量がツールセットに適合しているか否かのループ
					foreach (bool tskmatch in new bool[] { true, false }) {
						tList.Clear();
						imax = Int32.MinValue;
						imin = Int32.MaxValue;
						kcnt = 0;
						foreach (Tool.Tol sad in tols) {
							if (sad.Tmod != '2') continue;
							if (sad.Nnam.Count > 0) continue;
							if (sad.matchT == null) continue;
							if (sad.matchT.SnumT != snum) continue;
							if (sad.Toolsetname != koho.Toolsetname) continue;
							if (sad.Toolset.ToutMatch(sad.matchT.Umax) != tskmatch) continue;
							foreach (MatchK mchk in sad.matchT.m_kogList)
								if (sad.Toolset.ToutMatch(mchk.kogu.Tld.XmlT.TULEN) != tskmatch)
									throw new Exception("qefbqefrbqhebfhq");
							tList.Add(sad);
							kcnt += sad.matchT.KoguCount;
							imax = Math.Max(imax, sad.matchT.Umax);
							imin = Math.Min(imin, sad.matchT.Umin);
						}
						if (tList.Count <= 1) continue;
						if (tList.Count == kcnt) continue;
						if (imax == imin) continue;

						// 突出し量の整理の実行
						int iout = 99;
						if (CamUtil.ProgVersion.Debug) {
							iout = MatchT.SortTsk2(tList);
							System.Windows.Forms.MessageBox.Show($"突出し量の整理{iout.ToString()} {koho.matchT.Tset.tset_name}");
						}
						CamUtil.LogOut.CheckOutput(CamUtil.LogOut.FNAM.TMATCHING, Tejun.TejunName, Tejun.Mach.name,
							$"{"突出し量の整理"} tset={koho.matchT.Tset.tset_name} iout={iout}");
					}
				}
			}
		}
		/// <summary>
		/// 突出し量の大きい方からすべてセット、（可能なものは突出し量の小さい方に移動する）
		/// </summary>
		/// <param name="tList"></param>
		/// <returns>0:正常 1:正常(小さい方への移動あり) 2:異常</returns>
		static private int SortTsk2(List<Tool.Tol> tList) {
			int iout = 0;
			Tool.Tol koho;
			bool ng;
			List<MatchK> kList;
			List<Tool.Tol> dummy;

			double minCons;	// 最小の消耗割合
			double minTtsk; // 最小の突出し量

			// 対象の工具リストに関わる工具単位ＮＣ情報をkListに保存し
			// 一時的に工具単位ＮＣ情報の保存先として使用するdummyを作成する
			kList = new List<MatchK>();
			dummy = new List<Tool.Tol>();
			foreach (Tool.Tol tol in tList) {
				kList.AddRange(tol.matchT.m_kogList);
				koho = new Tool.Tol(tol.matchT.m_kogList[0].kogu, null, tol.Bnum, tol.EdtM001, Tejun.Mach);
				koho.matchT = new MatchT(null, TPrio.ELSE_TOOL, 1, 1, 0.0, false);
				dummy.Add(koho);
			}

			// まず、突出し量の大きい方から全てをセットする
			kList.Sort((x, y) => {
				int diff = 0;
				// 突出し量の大きい順、加工の逆順にソート
				if (diff == 0) diff = (int)Math.Ceiling(y.kogu.Tld.XmlT.TULEN) - (int)Math.Ceiling(x.kogu.Tld.XmlT.TULEN);
				if (diff == 0) diff = y.kogu.kakoJun - x.kogu.kakoJun;
				// 加工順にソート
				//diff = x.kogu.kakoJun - y.kogu.kakoJun;
				return diff;
			});
			if (0 == 0) {
				// kListの表示
				string ss = "";
				foreach (MatchK mchk in kList)
					ss += String.Format("{0,-14}  TULEN={1,3:f1}  KAKOJUN={2,3:d}  LEN={3:f1}\n",
						mchk.kogu.Parent.nnam, mchk.kogu.Tld.XmlT.TULEN, mchk.kogu.kakoJun, mchk.divData.Consumption);
				System.Windows.Forms.MessageBox.Show(ss);
			}
			minCons = minTtsk = 100000.0;
			foreach (MatchK mchk in kList) {
				if (minCons > mchk.divData.Consumption) minCons = mchk.divData.Consumption;
				if (minTtsk > mchk.kogu.Tld.XmlT.TULEN) minTtsk = mchk.kogu.Tld.XmlT.TULEN;
				ng = true;
				for (int tt = dummy.Count - 1; tt >= 0; tt--) {
					if (dummy[tt].matchT.KoguCount > 0)
						if (mchk.divData.Consumption + dummy[tt].matchT.Consumption > 100.0) continue;
					dummy[tt].matchT.m_kogList.Add(mchk);
					ng = false;
					break;
				}
				// データの整理に失敗した場合
				if (ng) return 2;
			}

			// 単独で占有する工具をＮＣ限定する
			for (int tt = 0; tt < dummy.Count; tt++) {
				if (dummy[tt].matchT.m_kogList.Count != 1) continue;
				if (dummy[tt].matchT.Consumption + minCons <= 100.0) continue;
				if (dummy[tt].matchT.Umax == dummy[dummy.Count - 1].matchT.Umax) continue;
				dummy[tt].Nnam.Add(dummy[tt].matchT.m_kogList[0].kogu.Parent.nnam);
			}

			// 突出し量のより小さい方へ移動
			/* 未完成
			kList.Sort(KoguKKJ);
			foreach (MatchK mchk in kList) {
				// 現在セットされている工具
				koho = null;
				for (int tt = 0; tt < dummy.Count; tt++)
					if (dummy[tt].matchT.m_kogList.Contains(mchk)) {
						koho = dummy[tt];
						break;
					}
				// 移動。できない場合は限定する
				for (int tt = 0; tt < dummy.Count; tt++) {
					if (dummy[tt].matchT.umax >= koho.matchT.umax) break;
					if (mchk.kogu.tld.xmlT.TULEN < dummy[tt].matchT.umax)
						if (mchk.divData.consumption + dummy[tt].matchT.consumption <= 100.0) {
							dummy[tt].matchT.m_kogList.Add(mchk);
							koho.matchT.m_kogList.Remove(mchk);
							break;
						}
				}
			}
			*/

			// 突出し量を工具表にセットしクリア
			for (int tt = 0; tt < tList.Count; tt++) {
				dummy[tt].Set_Match();
				dummy[tt].matchT.m_kogList.Clear();
			}

			// 工具マッチングと同様に、加工順にＮＣデータをセットする
			foreach (MatchK mchk in kList) {
				koho = NcdTool.Tool.Tol.Tnadds(dummy.ToArray(), dummy[0].matchT.SnumT, mchk.kogu, mchk.divData);
				// 工具数が不足してしまった場合
				if (koho == null) return 2;
				koho.matchT.m_kogList.Add(mchk);
			}
			// 元データのKoguリストをクリア＆セット
			for (int tt = 0; tt < tList.Count; tt++) {
				tList[tt].matchT.m_kogList.Clear();
				tList[tt].matchT.m_kogList.AddRange(dummy[tt].matchT.m_kogList);
				foreach (string stmp in dummy[tt].Nnam) tList[tt].Nnam.Add(stmp);
			}
			return iout;
		}



		/// <summary>
		/// MatchK が含まれる工具のリストを出力する（複数の工具から参照されているMatchTがあれば複数となる）
		/// </summary>
		/// <param name="mchk"></param>
		/// <param name="tstols"></param>
		static public List<Tool.Tol> ListTol_MatchK(TMatch.MatchK mchk, Tool.Tol[] tstols) {
			return tstols.Where(tol => tol.matchT != null && tol.matchT.m_kogList.Contains(mchk)).ToList();
		}

		// /////////////
		// 以上 static
		// /////////////








		/// <summary>この工具を使用するＮＣデータ数</summary>
		internal int KoguCount { get { return m_kogList.Count; } }
		/// <summary>シート番号リスト（参考：マガジン数よりシート最大工具番号が大きい場合は各ＮＣデータで同一ではない）</summary>
		public List<int> SnumList { get { return m_kogList.Select(mchk => mchk.SnumN).Distinct().ToList(); } }

		/// <summary>最後のシート番号ＮＣ（参考：マガジン数よりシート最大工具番号が大きい場合は各ＮＣデータで同一ではない）</summary>
		public int SnumN_Last { get { return (m_kogList.Count > 0) ? m_kogList[m_kogList.Count - 1].SnumN : 0; } }

		/// <summary>この工具を使用するＮＣデータリスト</summary>
		private List<MatchK> m_kogList;

		/// <summary>対応する突出し量最大の代表ツールセット</summary>
		internal CamUtil.ToolSetData.ToolSet Tset { get { return m_tset; } set { m_tset = value == null ? null : (CamUtil.ToolSetData.ToolSet)value.Clone(); } }
		private CamUtil.ToolSetData.ToolSet m_tset;

		/// <summary>工具番号を使用する優先順位</summary>
		internal TPrio tp;
		/// <summary>パーマネント工具を設定する</summary>
		public bool Perm_tool { get { return m_perm_tool; } }
		private readonly bool m_perm_tool;

		/// <summary>マッチングにより決定したシート番号（工具表）cf. MatchK.snumN</summary>
		public int SnumT { get { return m_snumT; } }
		private readonly int m_snumT;

		/// <summary>マッチングにより決定した工具番号。シートが複数あっても同じ番号に限定する</summary>
		public int Tnum { get { return m_tnum; } }
		private int m_tnum;

		/// <summary>マッチングにより決定した最大突出し量（int）</summary>
		//public double umax { get { return Math.Ceiling(m_umax); } }
		public int Umax { get { return m_kogList.Select(mchk => (int)Math.Ceiling(mchk.kogu.Tld.XmlT.TULEN)).DefaultIfEmpty(0).Max(); } }
		/// <summary>マッチングにより決定した最小突出し量（int）</summary>
		public int Umin { get { return m_kogList.Select(mchk => (int)Math.Ceiling(mchk.kogu.Tld.XmlT.TULEN)).DefaultIfEmpty(int.MaxValue).Min(); } }

		/// <summary>最大加工深さ</summary>
		//public double? zchi { get { return m_zchi; } }
		public double? Zchi {
			get {
				IEnumerable<double?> list = m_kogList.Select(mchk => mchk.divData.ncdist.Min?.Z);
				return list.All(mchk => mchk.HasValue) ? list.Min() : (double?)null;
			}
		}

		/// <summary>消耗率の積算（単位％）</summary>
		public double Consumption { get { return initial + m_kogList.Sum(mchk => mchk.divData.Consumption); } }
		/// <summary>以前出力した工具の消耗率の値</summary>
		private double initial;

		/// <summary>
		/// 基本コンストラクタ（唯一）
		/// </summary>
		/// <param name="p_tset">ツールセット。ダミーの場合はNULLにする</param>
		/// <param name="tp"></param>
		/// <param name="snumT">シート番号</param>
		/// <param name="tnum">工具番号</param>
		/// <param name="consumpt">工具表で設定された消耗率</param>
		/// <param name="perm_tool">パーマネント工具</param>
		public MatchT(CamUtil.ToolSetData.ToolSet p_tset, TPrio tp, int snumT, int tnum, double consumpt, bool perm_tool) {
			this.Tset = p_tset;
			this.m_kogList = new List<MatchK>();
			this.tp = tp;
			this.m_snumT = snumT;
			this.m_tnum = tnum;
			this.initial = consumpt;
			this.m_perm_tool = perm_tool;
		}

		/// <summary>
		/// 使用するＮＣデータの追加
		/// </summary>
		/// <param name="mchk"></param>
		public void Add_Nc(MatchK mchk) { this.m_kogList.Add(mchk); }
		/// <summary>
		/// ２つ目以降の工具の消耗率の追加
		/// </summary>
		/// <param name="consumpt"></param>
		public void Add_Consumpt(double consumpt) { this.initial += consumpt; }

		/// <summary>
		/// 非標準ツートセットを作成する
		/// </summary>
		/// <param name="umax"></param>
		public void SetTemporary(double umax) { m_tset = new CamUtil.ToolSetData.ToolSet(Tset.tset_name, umax); }







		// /////////////////////////////
		// 以下は工具表作成時に使用する
		// /////////////////////////////

		/// <summary>指定した工具のシート番号の有無（参考：マガジン数よりシート最大工具番号が大きい場合は各ＮＣデータで同一ではない）</summary>
		public bool SnumN_Exist(Tool.Tol tol, int iis) {

			// matchTがnullでなくm_kogList.Countが0の場合は
			// snumTとsnumNが等しい場合のみである
			if (m_kogList.Count == 0)
				return iis == tol.matchT.SnumT;

			return m_kogList.Any(mchk => mchk.SnumN == iis && mchk.K2.Tlgn == tol);
		}

		/// <summary>マッチングにより決定した合計加工時間</summary>
		public double Ntim() {
			return m_kogList.Sum(mchk => mchk.Ntim);
		}
		/// <summary>マッチングにより決定した合計加工時間</summary>
		public double Ntim(Tool.Tol tol, int iis) {
			return m_kogList.Where(mchk => mchk.SnumN == iis && mchk.K2.Tlgn == tol).Sum(mchk => mchk.Ntim);
		}

		/// <summary>マッチングにより決定した回転数</summary>
		public int? Schi() {
			IEnumerable<int> list = m_kogList.Select(mchk => mchk.kogu.CutSpinRate()).Distinct();
			return list.Count() == 1 ? list.First() : (int?)null;
		}
		/// <summary>マッチングにより決定した回転数</summary>
		public int? Schi(Tool.Tol tol, int iis) {
			IEnumerable<int> list = m_kogList.Where(mchk => mchk.SnumN == iis && mchk.K2.Tlgn == tol).Select(mchk => mchk.kogu.CutSpinRate()).Distinct();
			return list.Count() == 1 ? list.First() : (int?)null;
		}

		/// <summary>マッチングにより決定した送り速度</summary>
		public int? Fchi() {
			IEnumerable<int> list = m_kogList.Select(mchk => mchk.kogu.CutFeedRate()).Distinct();
			return list.Count() == 1 ? list.First() : (int?)null;
		}
		/// <summary>マッチングにより決定した送り速度</summary>
		public int? Fchi(Tool.Tol tol, int iis) {
			IEnumerable<int> list = m_kogList.Where(mchk => mchk.SnumN == iis && mchk.K2.Tlgn == tol).Select(mchk => mchk.kogu.CutFeedRate()).Distinct();
			return list.Count() == 1 ? list.First() : (int?)null;
		}

		/// <summary>出力ＮＣデータリスト（全て）</summary>
		public List<string> NnamList() {
			return m_kogList.Select(mchk => mchk.kogu.Oname(mchk)).Distinct().ToList();
		}
		/// <summary>出力ＮＣデータリスト（工具ごと）</summary>
		public List<string> NnamList(Tool.Tol tol, int iis) {
			return m_kogList.Where(mchk => mchk.SnumN == iis && mchk.K2.Tlgn == tol).Select(mchk => mchk.kogu.Oname(mchk)).Distinct().ToList();
		}

		/// <summary>ホルダー管理区分（全て）</summary>
		public string Hld_kanri() {
			IEnumerable<string> list = m_kogList.Select(mchk => mchk.kogu.TsetCHG.hld_knri).Distinct();
			return list.Count() == 1 ? list.First() : null;
		}
		/// <summary>ホルダー管理区分（工具ごと）</summary>
		public string Hld_kanri(Tool.Tol tol, int iis) {
			IEnumerable<string> list = m_kogList.Where(mchk => mchk.SnumN == iis && mchk.K2.Tlgn == tol).Select(mchk => mchk.kogu.TsetCHG.hld_knri).Distinct();
			return list.Count() == 1 ? list.First() : null;
		}
		/// <summary>工具番号比較</summary>
		public int CompareTo(MatchT mt) { return Tnum.CompareTo(mt.Tnum); }

		// 追加 2016/09/16
		/// <summary>最小加工順（全て）</summary>
		public int KakouJun() {
			return m_kogList.Select(mchk => mchk.kogu.kakoJun).DefaultIfEmpty(99999).Min();
		}
		/// <summary>最小加工順（工具ごと）</summary>
		public int KakouJun(Tool.Tol tol, int iis) {
			return m_kogList.Where(mchk => mchk.SnumN == iis && mchk.K2.Tlgn == tol).Select(mchk => mchk.kogu.kakoJun).DefaultIfEmpty(99999).Min();
		}
	}
}
