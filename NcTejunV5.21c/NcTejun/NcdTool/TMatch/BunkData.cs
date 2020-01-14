using System;
using System.Collections.Generic;
using System.Text;

namespace NcdTool.TMatch
{
	/// <summary>
	/// 複数の工具を持つＮＣデータの工具単位への分割情報
	/// </summary>
	class BunkData
	{
		/// <summary>
		/// この工具単位データの分割連番を返す（&gt;=1）
		/// </summary>
		/// <returns></returns>
		public int NcBunkNum(TMatch.MatchK mtchK) {
			int ii = m_bunk;
			foreach (NcName.Kogu skog in parent.Tdat)
				foreach (TMatch.MatchK mtmp in skog.matchK) {
					if (mtmp.Ochg) ii++;
					if (mtmp == mtchK) return ii;
				}
			throw new Exception("Ｏ番号のエラーです１");
		}

		/// <summary>ＮＣデータのファイル分割数（＞＝１）</summary>
		private int Bunknum {
			get {
				int ii = 0;
				foreach (NcName.Kogu skog in parent.Tdat)
					if (skog.matchK != null)
						foreach (MatchK mchk in skog.matchK)
							if (mchk.Ochg) ii++;
				if (ii == 0) CamUtil.LogOut.CheckCount("BunkData 034", false, "加工対象ではないＮＣデータを参照している " + parent.nnam);
				return ii;
			}
		}

		/// <summary>
		/// 連番付けが必要なデータ 2015/12/17
		/// </summary>
		public bool Renban {
			get {
				// 分割あり
				if (Bunknum > 1)
					return true;
				// 同一ＮＣデータで複数回出力する
				if (parent.Jnno.SameNc)	// if (parent.jnno.same0 == true || parent.jnno.same1 != null)
					return true;
				return false;
			}
		}
	
		/// <summary>工具表"B"などによる分割の番号の初期値。一般には０</summary>
		private int m_bunk;
		/// <summary>リンク</summary>
		private NcName.NcNam parent;


		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="ncsd"></param>
		public BunkData(NcName.NcNam ncsd) {

			parent = ncsd;

			// 分割の番号の初期値bunkの計算
			if (ncsd.Jnno.same1 != null) {
				NcName.NcNam ssss = ncsd.Jnno.same1;
				m_bunk = ssss.BunkData.m_bunk + ssss.BunkData.Bunknum;
				if (m_bunk >= 99) throw new Exception("NCDATA NAME No.(bunk) ERROR " + ncsd.nnam);
				if (m_bunk == -1) throw new Exception("qewfqerf");
			}
			else
				m_bunk = 0;
		}
	}
}
