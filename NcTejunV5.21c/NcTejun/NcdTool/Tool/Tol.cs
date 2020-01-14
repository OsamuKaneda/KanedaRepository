using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CamUtil;
using CamUtil.CamNcD;
using System.IO;
using System.Windows.Forms;

namespace NcdTool.Tool
{
	partial class Tol : IComparable
	{
		/// <summary>工具表１枚あたりの工具数（最も多い加工機のマガジン数とする）</summary>
		const int tlhMAX = 200;	/* TOL No by 1 TOOL-SHEET (max nmgz)	*/

		/// <summary>工具選択の優先順位（大きい方に差がある場合により順位が高くなる）</summary>
		enum YUSEN
		{
			/// <summary>加工機内の工具</summary>
			PERMTOOL = 32,
			/// <summary>突出し量がＮＣデータと整合</summary>
			TSKMATCH = 16,
			/// <summary>突出し量の現候補との比較</summary>
			TSKCOMPR = 8,
			/// <summary>消耗した割合の現候補との比較</summary>
			CONSUMPT = 4,
			/// <summary>工具番号あり</summary>
			TOOLNUMB = 2,
			/// <summary>工具番号が小さい</summary>
			TLNUMBER = 1,
			/// <summary>初期値</summary>
			SHOKICHI = 0
		}

		/// <summary>
		/// マッチング可能な工具を検索する
		/// </summary>
		/// <param name="tolList">検索する工具のリスト</param>
		/// <param name="snum">シート番号（工具表）</param>
		/// <param name="nowKogu">現在マッチしたＮＣ情報</param>
		/// <param name="divd">自動寿命分割の情報</param>
		/// <returns></returns>
		static public Tool.Tol Tnadds(Tool.Tol[] tolList, int snum, NcName.Kogu nowKogu, TMatch.Divide.DivData divd) {
			// /////////////////////////////////////////////////////
			// マッチングのルール
			// /////////////////////////////////////////////////////
			// １．ツールセット名が同一である工具
			// ２．マガジン内工具限定の処理
			// ３．ＮＣデータの限定がある工具（他シートでも寿命オーバーでも決定）
			// ４．追加された工具の場合は突出し量の整合と非整合を区分する
			// ５．現在のシート番号内の工具
			// ６．突出し量が整合している工具
			// ７．工具寿命がある工具
			// ８．条件を満たす工具が複数ある場合は以下の優先順位にて決定
			// 　　１．工具表に存在する
			// 　　２．加工機内常駐工具（突出し量整合が条件）
			// 　　３．突出し量が整合
			// 　　４．突出し量がより小さい
			// 　　５．工具寿命が１００％に近い

			// /////////////////////////////////////////////////////
			// 次の工具表シートにする場合のルール
			// /////////////////////////////////////////////////////
			// １．ＮＣデータ限定がある工具が他のシートにある場合
			// ２．このシートにマッチする工具がなく、次のシートにある場合
			// ３．工具を追加する必要があるがこのシートに追加する空きが無い場合


			// 工具選択の優先順位（大きい方が高い）
			int tensuu;
			int tensuukoho;
			// 工具選択の前の選択工具との比較（大きい方が高い）
			int hikaku;

			Tool.Tol koho = null;
			tensuukoho = -999;
			foreach (Tool.Tol sad in tolList) {
				tensuu = (int)YUSEN.SHOKICHI;
				hikaku = (int)YUSEN.SHOKICHI;

				// ツールセット名が一致しない
				if (sad.Toolset.tset_name != nowKogu.TsetCHG.Tset_name) continue;

				// マガジン内工具限定
				if (nowKogu.TsetCHG.Tset_FixedNumber) {
					if (sad.NoMatch && !sad.Perm_tool) continue;
					if (!sad.NoMatch && !sad.matchT.Perm_tool) continue;
				}

				// //////////////////////////////////////////
				// このＮＣデータに限定されている工具の処理
				// //////////////////////////////////////////
				if (sad.Nnam.Count > 0) {
					if (sad.Nnam.Contains(nowKogu.Parent.nnam)) {
						koho = sad;
						break;
					}
					continue;
				}

				// ///////////////////////////////
				// 以下はＮＣデータ限定なしの工具
				// ///////////////////////////////

				// 追加工具の場合、標準内標準外のツールセットを区分する（追加工具以外は突出し量比較で対応） add in 2015/08/18
				if (sad.matchT != null && sad.matchT.KoguCount > 0 && sad.Tmod == '2')
					if (sad.Toolset.ToutMatch(sad.matchT.Umax) != sad.Toolset.ToutMatch(nowKogu.Tld.XmlT.TULEN))
						continue;

				// このシートで使用する工具候補となっていない場合
				// 参考：マガジン数が最大工具番号より小さい場合(shtData.exmach>0)でも、
				// 工具表のシート番号ですべての工具を使用可能とする
				if (sad.Tmod == '0' && sad.Unum > 0) {
					if (snum != sad.Rnum) continue;
				}
				else {
					if (sad.Tmod == '0' && sad.Unum < 0)
						throw new Exception("qerfbqefrbqhr");	// 工具番号が０より小さい場合は tmod=='1' となる
					if (sad.matchT != null)
						if (snum != sad.matchT.SnumT) continue;
				}

				// 使用済みでしかも突出し量が不足している工具
				if (sad.Initial_consumpt > 0.0 && sad.Ttsk + 0.001 < nowKogu.Tld.XmlT.TULEN)
					continue;

				// 工具寿命がオーバーしている（ＮＣ限定が設定されている場合はマッチング可能）
				// （消耗率の保存によりマッチしていなくても消耗していることがあるため変更）
				//if (sad.NoMatch == false)
				if (sad.matchT != null) if (sad.matchT.Consumption > 0.0)
						if (divd.Consumption + sad.matchT.Consumption > 100.0)
							continue;

				// ///////////////////////////////
				// 以下は候補工具の評価
				// ///////////////////////////////

				// 工具番号の有無
				if (sad.Tmod != '1' || sad.matchT != null)
					tensuu += (int)YUSEN.TOOLNUMB;

				// 工具表内の常時セット工具
				if (sad.Perm_tool || (sad.matchT != null && sad.matchT.Perm_tool)) {
					// 突出し量が一致しない
					if (sad.Toolset.ToutMatch(nowKogu.Tld.XmlT.TULEN) == false)
						continue;
					tensuu += (int)YUSEN.PERMTOOL;
				}

				// 突出し量がＮＣデータと整合
				if (sad.Ttsk >= nowKogu.Tld.XmlT.TULEN)
					tensuu += (int)YUSEN.TSKMATCH;

				// 突出し量の現候補との比較
				if (sad.Ttsk >= nowKogu.Tld.XmlT.TULEN && koho != null) {
					if (Math.Abs(sad.Ttsk - koho.Ttsk) < 0.01) {
						;
					}
					else if (sad.Ttsk < koho.Ttsk)
						hikaku += (int)YUSEN.TSKCOMPR;
					else if (sad.Ttsk > koho.Ttsk)
						hikaku -= (int)YUSEN.TSKCOMPR;
				}

				// 消耗した割合の現候補との比較
				if (koho != null) {
					if (sad.matchT == null) {
						if (koho.matchT != null)
							hikaku -= (int)YUSEN.CONSUMPT;
					}
					else {
						if (koho.matchT == null)
							hikaku += (int)YUSEN.CONSUMPT;
						else {
							if (sad.matchT.Consumption > koho.matchT.Consumption)
								hikaku += (int)YUSEN.CONSUMPT;
							if (sad.matchT.Consumption < koho.matchT.Consumption)
								hikaku -= (int)YUSEN.CONSUMPT;
						}
					}
				}

				// 工具番号の現候補との比較
				if (koho != null) {
					if (sad.matchT == null) {
						if (koho.matchT != null)
							hikaku -= (int)YUSEN.TLNUMBER;
					}
					else {
						if (koho.matchT == null)
							hikaku += (int)YUSEN.TLNUMBER;
						else {
							if (sad.matchT.Tnum < koho.matchT.Tnum)
								hikaku += (int)YUSEN.TLNUMBER;
							if (sad.matchT.Tnum > koho.matchT.Tnum)
								hikaku -= (int)YUSEN.TLNUMBER;
						}
					}
				}

				/* ＮＣデータ限定で対応するため不要
				
				// 工具単位分割ルール、M01挿入ルール
				if (koho != null) {
					if (rule.Match(koho, null) == true && rule.Match(sad, null) == false)
						hikaku -= (int)YUSEN.BUNK_M01;
					if (rule.Match(koho, null) == false && rule.Match(sad, null) == true)
						hikaku += (int)YUSEN.BUNK_M01;
				}
				*/


				// /////////////////////////
				// 候補工具の更新
				// /////////////////////////
				if (tensuu + hikaku > tensuukoho) {
					koho = sad;
					tensuukoho = tensuu;
				}
			}
			return koho;
		}

		/// <summary>
		/// tmodのデータテーブルへの反映後の処理
		/// </summary>
		/// <param name="tsht"></param>
		/// <param name="addtool">追加の有無</param>
		static public void TmodSet(NcTejun.TejunSet.ToolSheet tsht, bool addtool) {
			foreach (Tol sad in tsht.Tols) {
				switch (sad.Tmod) {
				case '0':
					break;
				case '1': sad.Tmod = '0';
					break;
				case '2': if ((addtool && sad.Addtool)) sad.Tmod = '0';
					break;
				}
			}
		}




		/// <summary>
		/// データテーブルのデータへのリンク
		/// </summary>
		public System.Data.DataRow DRowLink { get { return m_dRow; } set { m_dRow = value; } } System.Data.DataRow m_dRow;

		/// <summary>Ｍ０１の挿入</summary>
		public bool EdtM001 { get; private set; }

		// //////////
		// 新規追加
		// ///////////////////////////////////////////////////////////////
		/// <summary>ツールセット名</summary>
		public string Toolsetname { get { return m_toolset?.tset_name; } }
		/// <summary>ツールセット</summary>
		public CamUtil.ToolSetData.ToolSet Toolset { get { return m_toolset; } } private CamUtil.ToolSetData.ToolSet m_toolset;

		/// <summary> パーマネント工具である（ツールセット名、工具番号、突出し量が整合）</summary>
		public bool PermTool() {
			return Tejun.Mach.Mgrs.Where(sad => this.Toolset.tset_name == sad.Toolsetname && sad.Toolset.ToutMatch(this.Ttsk))
				.Any(sad => this.Unum <= 0 || this.Unum == sad.Unum);
		}

		/// <summary>工具データテーブルに追加して使用可能な工具（perm_toolか工具番号固定でない） add 2016/01/27</summary>
		public bool Addtool { get { return (Perm_tool || !Toolset.FixedNumber); } }
		/// <summary>常時セットの工具であればtrue（加工機に依存する）add 2011/10/05</summary>
		public bool Perm_tool { get; private set; }
		// ///////////////////////////////////////////////////////////////

		/// <summary>スペース区切りで出力したＮＣ限定</summary>
		public string NnamS {
			get {
				string sout = null;
				foreach (string stmp in m_nnam) {
					if (stmp == null)
						throw new Exception("tol.nnam の値が不正");
					if (stmp.Length == 0)
						throw new Exception("tol.nnam の値が不正");
					sout += " " + stmp;
					if (sout[0] == ' ') sout = sout.Substring(1);
				}
				return sout;
			}
		}
		/// <summary>セットされた場合このＮＣデータのみに使用を制限する</summary>
		public List<string> Nnam { get { return m_nnam; } } List<string> m_nnam;

		/// <summary>工具のみの正しい突出し量（焼きバメの場合UNIX工具表より補正量分短い）</summary>
		public double Ttsk { get { if (m_ttsk == null) return -1.0; else return m_ttsk.Value; } }
		private double? m_ttsk;

		/// <summary>各工具の設定情報 0:工具表(番号有り) 1:工具表(番号なし) 2:システムで追加 3:加工機</summary>
		public char Tmod { get; private set; }

		/// <summary>ツールシートにて設定された工具シート番号</summary>
		public int Rnum { get; private set; }

		/// <summary>ツールシートにて設定された工具Ｔ番号
		/// （==0は常にシステム計算の工具番号を使用し工具表で番号を決めない）（&lt;0はシステムに自動設定し工具表に番号を反映させる）</summary>
		public int Unum { get; private set; }

		/// <summary>
		/// 高速スピンドル（分割と無関係にした in 2016/03/01）
		/// </summary>
		public bool Hsp { get; private set; }
		/// <summary>
		/// ＮＣデータ分割の設定
		/// true :分割あり
		/// false:分割なし（高速スピンドル分割含む）
		/// </summary>
		public bool Bnum { get; private set; }	/* bnum false:分割無し、あるいは高速スピンドルによる分割 true:'B'を用いた分割の設定有 */

		// ////////////////////////////
		// 新規追加
		/// <summary>新しいマッチング</summary>
		public TMatch.MatchT matchT;


		/// <summary>ツールシートにより設定された場合の設定順序</summary>
		public int SetJun { get; private set; }
		/// <summary>ソートの方法 0:工具番号順、1:加工順</summary>
		static public int sortMethod;

		///// <summary>使用するＮＣデータの消耗率の和（単位％）（add 2006.08.08）</summary>
		//public double consumption { get { return m_consumption; } }
		//double m_consumption;	// 使用するＮＣデータの消耗率の和（add 2006.08.08）
		/// <summary>消耗率の積算（追加：2006.09.12）（プロパティに変更2010.05.28）</summary>
		public double Consumption_new { get { return matchT == null ? Initial_consumpt : matchT.Consumption; } }
		///// <summary>消耗率の初期値（単位％）（add 2014.11.18）</summary>
		public double Initial_consumpt { get; private set; }
		//public double initial_consumpt { get { double mm = 0.0; foreach (TSData.ItemData dd in m_initial_consumpt) mm += dd.data; return mm; } }
		//private List<TSData.ItemData> m_initial_consumpt;

		private void Shokika() {
			m_dRow = null;
			Tmod = ' ';
			Rnum = 0;
			Unum = -1;
			//m_hsp = false;
			Bnum = false;
			EdtM001 = false;
			//m_edtM100 = false;
			m_toolset = null;
			//m_tnam_kinf = null;
			m_nnam = new List<string>();
			Perm_tool = false;

			m_ttsk = null;
			SetJun = 0;
		}

		/// <summary>
		/// コンストラクタ。データテーブルより作成（ユニックスの工具データ、手修正など）（新マッチングも）
		/// </summary>
		/// <param name="p_tmod"></param>
		/// <param name="dRow"></param>
		/// <param name="count"></param>
		/// <param name="initial"></param>
		/// <param name="machn">加工機名</param>
		public Tol(char p_tmod, System.Data.DataRow dRow, int count, double initial, string machn) {
			matchT = null;

			this.m_dRow = dRow;
			Rnum = (int)dRow["シートNo"];
			Bnum = (bool)dRow["分割"];
			Unum = (int)dRow["工具No"];
			this.Tmod = p_tmod;

			EdtM001 = (bool)dRow["M01"];

			//m_tnam_kinf = dRow["工具名UX"] == DBNull.Value ? null : (string)dRow["工具名UX"];
			m_toolset = null;
			m_ttsk = null;
			if (dRow["ツールセット"] != DBNull.Value)
				m_toolset = new CamUtil.ToolSetData.ToolSet((string)dRow["ツールセット"]);
			if (m_toolset == null)
				throw new Exception("argfnqwerfn");
			if (dRow["突出し量PC"] != DBNull.Value)
				m_ttsk = (double)dRow["突出し量PC"];
			// 高速スピンドル
			Hsp = ToolSetInfo.Get_HolderType(Toolset, machn);

			if (dRow["ＮＣ限定"] == DBNull.Value)
				m_nnam = new List<string>();
			else {
				m_nnam = new List<string>();
				m_nnam.AddRange(
					((string)dRow["ＮＣ限定"]).Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries));
			}

			this.Initial_consumpt = initial;

			SetJun = count;

			Perm_tool = PermTool();

			if (Unum > Tol.tlhMAX)
				throw new Exception("最大値" + Tol.tlhMAX + "より大きな値が設定されている工具番号があります。");
		}

		/// <summary>
		/// コンストラクタ。加工機マガジン設定の工具の作成（新）
		/// </summary>
		/// <param name="tno"></param>
		/// <param name="tset_name"></param>
		/// <param name="machn">加工機名</param>
		public Tol(int tno, string tset_name, string machn) {

			Shokika();

			Tmod = '3';
			Unum = tno;
			//if (m_unum < 0 || m_unum > nmgz)
			//	throw new Exception("RESERVE TOOL NUMBER ERROR");

			m_toolset = new CamUtil.ToolSetData.ToolSet(tset_name);
			//m_hldr[0] = m_toolset.holder_name;
			m_ttsk = m_toolset.ToutLength;
			//m_tnam_kinf = kinf;
			Perm_tool = true;	// perm_toolの設定 2011/10/05
			// 高速スピンドル
			Hsp = ToolSetInfo.Get_HolderType(Toolset, machn);
		}

		/// <summary>
		/// 非標準ツートセットを作成する
		/// </summary>
		/// <param name="ttsk"></param>
		public void SetTemporary(double ttsk) { m_toolset = new ToolSetData.ToolSet(m_toolset.tset_name, ttsk); }

		// //////////////////////// //
		// //////////////////////// //
		//                          //
		// 新マッチングのための設定 //
		//                          //
		// //////////////////////// //
		// //////////////////////// //

		/// <summary>
		/// 新規工具の追加（工具番号未定, permtool未定, 突出し量未定）
		/// </summary>
		/// <param name="skog"></param>
		/// <param name="mcht">同一工具番号のツールを作成する場合にセット</param>
		/// <param name="iRule_bun">ＮＣデータ分割有無</param>
		/// <param name="iRule_m01">Ｍ０１挿入有無</param>
		/// <param name="mach">加工機情報</param>
		public Tol(NcName.Kogu skog, TMatch.MatchT mcht, bool iRule_bun, bool iRule_m01, Mcn1 mach) {
			Shokika();

			this.Tmod = '2';
			this.m_toolset = new CamUtil.ToolSetData.ToolSet(skog.TsetCHG.Tset_name);
			//this.m_hldr[0] = toolset.holder_name;

			this.matchT = mcht;
			if (mcht != null)
				m_nnam.Add(skog.Parent.nnam);

			//this.m_ttsk = Math.Ceiling(skog.tld.xmlT.TULEN);
			// ここではまだ暫定のツールセットである（突出しの整合無し）
			// 必要であれば出力時に正しいツールセット名にされる

			//this.m_tnam_kinf = skog.tld.tnamDB;

			// ＮＣデータ分割設定
			Bnum = iRule_bun;
			if (mach.Toool_nc && Bnum == false) throw new Exception("iRule_bunのエラー");

			// M01 挿入設定
			EdtM001 = iRule_m01;

			//m_edtM100 = false;
			// 高速スピンドル
			Hsp = ToolSetInfo.Get_HolderType(Toolset, mach.name);
		}

		/// <summary>
		/// ＮＣデータとマッチングしていない場合 true
		/// マッチング候補となった場合matchTはnullでなくなるがkoguCountは０
		/// マッチング候補 : 工具のシート番号とＮＣデータのシート番号が等しい場合マッチングしなくても候補となる
		/// </summary>
		public bool NoMatch {
			get {
				if (Initial_consumpt == 0) {
					if (matchT == null) return true;
					if (matchT.KoguCount == 0) return true;
				}
				return false;
			}
		}

		/// <summary>
		/// matchT の工具データへの反映
		/// セットするデータ ttsk, rnum(tmod!=0), unum(tmod!=0)
		/// </summary>
		public void Set_Match() {

			// ToolSetが独立しているはず
			if (matchT != null && Toolset != null) {
				if (Toolset == matchT.Tset)
					throw new Exception("afvwfrbrfrfbharefh");
			}

			if (Tmod != '0') {
				//m_rnum = matchT.SnumOnly.Value;
				Rnum = matchT.SnumT;
				Unum = matchT.Tnum;
				Perm_tool = matchT.Perm_tool;
			}

			// マッチングしていない工具の加工高さにnullをセット
			// マッチングしていない工具の突出し量にnullをセット
			if (NoMatch) { ;}
			else {
				if (Initial_consumpt == 0.0) {
					// 使用済み以外は最大突出し量は必ずセットする
					// 複数の手順で再利用する場合（常時セット工具、部品加工）は標準の突出し量をセットする
					m_ttsk = Math.Max(m_ttsk ?? 0, matchT.Umax);
					if (Tmod == '2')
						if (Perm_tool || Tejun.BaseNcForm.Id == CamUtil.BaseNcForm.ID.BUHIN)
							m_ttsk = Math.Max(m_ttsk.Value, this.Toolset.ToutLength);
				}
				else
					if (m_ttsk < matchT.Umax) throw new Exception("aqewfbqehrfbh");
			}
			return;
		}







		//ksort(ep1,ep2)
		/// <summary>
		/// 工具の表示順の設定
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int CompareTo(object obj)
			//struct Tol **ep1,**ep2;
		{
			if (obj is Tol) { ;}
			else
				throw new ArgumentException("object is not tol");

			Tol temp = (Tol)obj;

			if (this.Rnum != temp.Rnum)
				return this.Rnum - temp.Rnum;
			if (sortMethod == 0) {
				if(this.Unum != temp.Unum)
					return this.Unum - temp.Unum;
				return this.SetJun - temp.SetJun;
			}
			else if (sortMethod == 1) {
				if (this.NoMatch)
					return 1;
				if (temp.NoMatch)
					return -1;
				return this.matchT.KakouJun() - temp.matchT.KakouJun();
			}
			else
				throw new Exception("sortMethod が異常値である");
		}
	}
}
