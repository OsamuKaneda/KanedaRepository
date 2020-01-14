using System;
using System.Collections.Generic;
using System.Text;

using CamUtil;
using CamUtil.CamNcD;
using System.IO;

namespace NcdTool.NcName
{
	/// <summary>
	/// ＮＣデータで使用する工具関連情報
	/// </summary>
	partial class Kogu
	{
		/// <summary>使用ツールセットが決定できたかどうか判定</summary>
		/// <remarks>
		/// false となる場合（この時マッチング対象外のため、matchK.Length == 0 のままである）
		/// １．加工機で使用可能な工具が未登録である
		/// ２．ＮＣデータ検証で使用されたツールセットと異なる
		/// ３．工具番号固定のツールセットがマガジン内に存在しない
		/// ４．ガンドリルで使用しない工具である
		/// </remarks>
		public bool TsetCHK { get { return m_tsetCHG.HasValue; } }

		/// <summary>出力対象のデータかどうか判定</summary>
		public bool Output {
			get {
				if (TsetCHK == false) return false;

				// マッチング後であれば、使用する工具が工具表に入力されていない場合
				// （マッチング後は必ず matchK.Length > 0 となる。tlgn が null になることはない）
				// （NCSPEED出力など完全なマッチングがなされていない場合はmatchK.Length == 0となる）
				if (matchK.Length > 0)
					foreach (TMatch.MatchK mchk in matchK) if (mchk.K2.Tlgn.Tmod != '0') return false;
				return true;
			}
		}

		/// <summary>親のNcNamへのリンク</summary>
		public NcNam Parent { get { return m_parent; } } private readonly NcNam m_parent;
		/// <summary>親のＮＣデータ内の位置</summary>
		public int TNo { get { for (int ii = 0; ii < this.Parent.Itdat; ii++) if (this == this.Parent.Tdat[ii]) return ii; throw new Exception("qehfrbqher"); } }

		/// <summary>ＮＣデータ作成時に関連付けられた工具の情報そのまま</summary>
		public NcDataT Tld { get { return m_tld; } } private readonly NcDataT m_tld;

		// /////////////////////////////////////////////////////////////////////

		/// <summary>新しいマッチング（ＮＣ）自動寿命分割対応</summary>
		public TMatch.MatchK[] matchK;
		/// <summary>新しいマッチング（ＮＣ）寿命分割なしの場合</summary>
		public TMatch.MatchK MatchK0 { get { if (matchK.Length == 1) return matchK[0]; else throw new Exception(""); } }

		/// <summary>全てのＮＣデータについての加工する工具順</summary>
		public readonly int kakoJun;

		/// <summary>工程ごとの加工長。マッチング時の寿命分割にのみ使用</summary>
		public CamUtil.LCode.NcLineCode.NcDist[] KoteiList { get { return m_koteiList.ToArray(); } } private List<CamUtil.LCode.NcLineCode.NcDist> m_koteiList;

		// /////////////////////////////////////////////////////////////////////








		/// <summary>
		/// tld（KDATAのデータ）よりKoguクラスを作成
		/// </summary>
		/// <param name="ncNam"></param>
		/// <param name="p_tld"></param>
		/// <param name="p_kakojun"></param>
		/// <param name="mcnName"></param>
		public Kogu(NcNam ncNam, NcDataT p_tld, int p_kakojun, string mcnName) {

			this.m_parent = ncNam;
			this.m_tld = p_tld;
			this.kakoJun = p_kakojun;

			// ツールセットＣＡＭと加工機からツールセット名などを決定
			try {
				// ツールセット関連データをセット
				m_tsetCHG = new TSetCHG(p_tld, mcnName);
				// 工具寿命関連データをセット
				tsetMAT = new TSetMAT(this);
			}
			catch (Exception ex) {
				m_tsetCHG = null;
				System.Windows.Forms.MessageBox.Show($"ＮＣ名:{ncNam.nnam}  工具連番:{p_kakojun.ToString("00")}  {ex.Message}");
			}

			matchK = new TMatch.MatchK[0];

		}
		

		/// <summary>
		/// 既存のクラスよりKoguクラスを作成
		/// </summary>
		/// <param name="ncNam"></param>
		/// <param name="src">既存のクラス</param>
		/// <param name="p_kakojun"></param>
		public Kogu(NcNam ncNam, Kogu src, int p_kakojun) {
			this.m_parent = ncNam;
			this.m_tld = src.Tld;	// クラス
			this.kakoJun = p_kakojun;
			//m_toolset = src.m_toolset;

			m_tsetCHG = src.m_tsetCHG;
			tsetMAT = src.tsetMAT;
			//m_nsgt1 = new st_nsgt1(src.m_nsgt1);

			matchK = new TMatch.MatchK[0];
		}

		/// <summary>
		/// 工程ごとの加工長リストの算出
		/// </summary>
		public void NcLength_Kotei() {
			int tnow;

			if (Consumption <= 100.0) return;
			m_koteiList = new List<CamUtil.LCode.NcLineCode.NcDist>();

			// 出力ＮＣデータの情報
			CamUtil.LCode.NcLineCode txtd = new CamUtil.LCode.NcLineCode((double[])null, Tejun.BaseNcForm, CamUtil.LCode.NcLineCode.GeneralDigit, false, true);
			// 加工長の情報
			CamUtil.LCode.NcLineCode.NcDist passLength = new CamUtil.LCode.NcLineCode.NcDist(this.Tld.XmlT.FEEDR, this.Tld.XmlT.MachiningAxisList);

			tnow = -1;
			using (StreamReader sr = new StreamReader(this.Parent.Ncdata.fulnamePC)) {
				while (!sr.EndOfStream) {
					txtd.NextLine(sr.ReadLine());
					if (txtd.B_g100) tnow++;
					if (tnow > this.Tld.SetJun - 1) break;
					if (tnow < this.Tld.SetJun - 1) continue;

					// M98P9017（ＣＬの切れ目）
					// M98P9306（工具終了処理）
					if (txtd.NcLine.IndexOf("M98P9017") >= 0 || txtd.NcLine.IndexOf("M98P9306") >= 0) {
						m_koteiList.Add(passLength);
						passLength = new CamUtil.LCode.NcLineCode.NcDist(this.Tld.XmlT.FEEDR, this.Tld.XmlT.MachiningAxisList);
					}
					else
						passLength.PassLength(txtd);
				}
			}
			return;
		}

		/// <summary>
		/// 出力ファイル名を出力
		/// </summary>
		/// <returns>ファイル名</returns>
		internal string Oname() {
			foreach (Kogu skog in this.Parent.Tdat) {
				if (skog.matchK.Length > 1)
					throw new Exception("出力ファイル名エラー in oname");
			}
			return Oname(this.matchK[0]);
		}
		/// <summary>
		/// 出力ファイル名を出力
		/// </summary>
		/// <param name="mtchK"></param>
		/// <returns></returns>
		internal string Oname(TMatch.MatchK mtchK) {
			if (m_parent == null) return "";

			// 連番をつける場合
			if (this.Parent.BunkData.Renban) {
				return m_parent.nnam + this.Parent.BunkData.NcBunkNum(mtchK).ToString("00");
			}
			else {
				return m_parent.nnam;
			}
		}
		/// <summary>
		/// 倒れ式のＬ/Ｚ補正量を面オフセット量から計算する
		/// </summary>
		/// <returns>Ｌ/Ｚ補正量</returns>
		public double Taore() {
			if (this.Tld.XmlT.ClMv_Z_axis != 0.0) return 0.0; // ＣＬ移動済の場合
			if (this.Tld.XmlT.ClMv_Offset == 0.0) return 0.0; // 倒れの面オフセット無しの場合
			return -this.Tld.XmlT.ClMv_Offset;
		}
	}
}
