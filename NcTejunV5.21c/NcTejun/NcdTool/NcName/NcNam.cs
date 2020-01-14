using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.IO;
using CamUtil;
using CamUtil.CamNcD;

namespace NcdTool.NcName
{
	/// <summary>
	/// 手順で設定された下記の情報
	/// nnam:ＮＣデータ名、nggt:モード、移動など、ncdata:ＮＣデータ情報へのリンク
	/// </summary>
	partial class NcNam
	{
		/// <summary>無意味なＮＣデータ名。手順書の空の行を挿入するためなどに用いる</summary>
		public const string DMY = "&";

		/// <summary>
		/// 実数の座標値が一致しているとみなす値
		/// </summary>
		public const double FLMIN = 0.0001;

		/// <summary>ＮＣデータ情報の取得（工具マッチング前）</summary>
		/// <param name="disp_message"></param>
		static public void SetKdata(Label disp_message) {

			// ////////////////////////////////////////////
			// ＮＣＤＡＴＡの情報を入力
			// ////////////////////////////////////////////
			disp_message.Text = "ＮＣデータのＣＡＭ出力情報を取得しています";
			Application.DoEvents();
			if (Tejun.BaseNcForm.Id == CamUtil.BaseNcForm.ID.EMPTY) throw new Exception("qerfqerh");
			foreach (NcNam ncnam in Tejun.NcList.NcNamsAll)
				ncnam.SetNcData();

			// ////////////////////////////////////////////
			// 工具単位の情報を取得（ncdataデータ必要）
			// ////////////////////////////////////////////
			disp_message.Text = "ＮＣデータの工具単位情報を取得しています";
			Application.DoEvents();
			int kakojun = 0;    // 加工順序の設定
			foreach (NcNam ncnam in Tejun.NcList.NcNamsAll) {
				ncnam.Ncsget(ref kakojun, Tejun.Mach.name);
				// 参照するＮＣデータで工具が複数の場合
				if (ncnam.Jnno.Nknum != null && ncnam.Itdat > 1)
					throw new Exception(ncnam.nnam + "の参照で小文字の'n'の複数工具のＮＣデータへの参照は未対応です。");
			}

			// //////////////////////////
			// 加工機ごとの特殊データ作成
			// //////////////////////////
			foreach (NcNam ncnam in Tejun.NcList.NcNamsAll) {
				switch (Tejun.Mach.ID) {
				case CamUtil.Machine.MachID.MHG_1500:
					// ガンドリルの変換仕様設定
					disp_message.Text = "ガンドリルの変換仕様を設定しています";
					Application.DoEvents();
					if (ncnam.nnam != NcNam.DMY)
						ncnam.Holes = new Holes(ncnam);
					break;
				case CamUtil.Machine.MachID.LineaM:
				case CamUtil.Machine.MachID.D500:
					// 加工工程ごとの加工長の計算
					disp_message.Text = "加工工程ごとの加工長を計算しています";
					Application.DoEvents();
					foreach (Kogu skog in ncnam.Tdat)
						skog.NcLength_Kotei();
					break;
				}
			}
			return;
		}

		/// <summary>切削条件を設定する。現在は工具と無関係となっている（工具マッチング後）</summary>
		static public void SetCutting(string tolstName) {
			Kogu.SetCutting(tolstName);
		}

		/// <summary>ＮＣデータの分割情報を設定（工具マッチング後）</summary>
		static public void BunkSet() {
			foreach (NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll)
				ncnam.BunkData = new NcdTool.TMatch.BunkData(ncnam);
		}

		// //////////////
		// 以上、static
		// //////////////






		/// <summary>一般ＮＣデータのＮＣデータ出力区分。N:出力　n:出力しない。ユニックス時代にあったサブプログラム機能は廃止した</summary>
		/// <remarks>
		/// IsUpper()==falseについて。従来は参照するＮＣデータがある場合のみ認めていたが、無い場合でもエラーとしないように変更した 2018/09/21
		/// </remarks>
		public char Nmod { get { return (nnam == NcNam.DMY || Itdat == 0) ? Char.ToUpper(m_nmod) : m_nmod; } }
		private readonly char m_nmod;
		/// <summary>ツールシート名</summary>
		public readonly string tsheet;

		/// <summary>ＮＣデータ名</summary>
		public readonly string nnam;

		/// <summary>
		/// ＣＡＭで作成されたＮＣデータ情報（ADD in 2007.03.14）
		/// ダミーＮＣデータの場合はnullとする 2015/10/29
		/// ＮＣデータがまだ存在しない場合は ncdata.fsiz0 となる
		/// </summary>
		public NcData Ncdata { get; private set; }
		/// <summary>加工手順データのモードそのまま</summary>
		public readonly Nmgt nmgt;
		/// <summary>加工手順でＮＣに設定された情報</summary>
		public readonly St_nggt nggt;

		/// <summary>ＮＣデータ内の工具交換数（tdat設定前はnull 以降は０以上）</summary>
		public int? Itdat { get { return m_tdat?.Count; } }
		/// <summary>ＮＣデータ内の工具情報</summary>
		internal CamUtil.RO_Collection<Kogu> Tdat { get { return m_tdat.AsReadOnly; } }
		private CamUtil.RO_Collection<Kogu>.InnerList m_tdat;

		/// <summary>ＮＣデータの参照情報</summary>
		public St_jnno Jnno { get; private set; }
		/// <summary>ＮＣデータの参照情報</summary>
		public readonly struct St_jnno
		{
			/// <summary>同一のＮＣデータを複数回使用する時に最初に使用するＮＣデータの参照が返される</summary>
			public NcNam Nknum { get { return m_nknum != null && m_nknum.Jnno.m_nknum != null ? m_nknum.Jnno.Nknum : m_nknum; } }
			/// <summary>同一のＮＣデータを複数回使用する時にひとつ前に使用するＮＣデータの参照が返される</summary>
			private readonly NcNam m_nknum;

			/// <summary>同一名のＮＣデータの場合 true（先頭も含む）</summary>
			public bool SameNc { get { return same0 || same1 != null; } }
			/// <summary>同一名のＮＣデータの最初の場合 true。共有する（nknum!=null）場合は false とする（private に変更すること）</summary>
			private readonly bool same0;
			/// <summary>１つ前の同一名のＮＣデータ NcNam。共有する（nknum!=null）場合は null とする（先頭の場合は null）</summary>
			public readonly NcNam same1;
			/// <summary>CL DATA START NUMBER</summary>
			public int ClStartNo { get { return (same1 != null) ? same1.Itdat.Value + same1.Jnno.ClStartNo : 1; } }

			/// <summary>
			/// コンストラクタ ninumより依存関係(jnno)を設定
			/// </summary>
			/// <param name="p_nknum">同一のＮＣデータを共有し複数回使用する場合</param>
			/// <param name="p_samen0">共有はしないが同一ＮＣデータ名がある場合先頭のＮＣデータを設定</param>
			/// <param name="p_samen1">共有はしないが同一ＮＣデータ名がある場合に１つ前のＮＣデータを設定</param>
			public St_jnno(NcNam p_nknum, bool p_samen0, NcNam p_samen1) {
				this.m_nknum = p_nknum;
				this.same0 = p_samen0;
				this.same1 = p_samen1;
				if (this.m_nknum != null && this.SameNc) throw new Exception("qjwhfdbqhbe");
			}
		}

		/// <summary>ＮＣデータの分割情報（マッチング後に設定）add in 2014/10/31</summary>
		public TMatch.BunkData BunkData { get; private set; }

		// 新規作成 2015/09/02
		/// <summary>ガンドリルへの変換仕様</summary>
		public Holes Holes { get; private set; }









		/// <summary>
		/// 唯一のコンストラクタ
		/// </summary>
		/// <param name="gmod"></param>
		/// <param name="ff"></param>
		/// <param name="tsheetname"></param>
		/// <param name="ncNamList"></param>
		public NcNam(Nmgt gmod, string[] ff, string tsheetname, List<NcNam> ncNamList) {
			string p_nnam = ff[1];   // ＮＣデータ名
			NcNam sameName;

			m_nmod = ff[0][0];   // モード 'N' or 'n'

			this.nnam = p_nnam;
			this.tsheet = tsheetname;
			this.nmgt = new Nmgt(gmod);                 // gmodをそのままコピー
			this.nggt = new St_nggt(ff, gmod.SetZais);      // nmod、材質、ミラー、移動の設定
			this.Ncdata = null;                             // this.SetNcData() にて設定
			this.m_tdat = null;                             // this.ncsget() にて設定
															//m_sorg0 = m_sorg1 = m_sorg2 = m_sorg3 = 0.0;	// this.ncmget() にて設定

			// 同一名ＮＣデータの情報作成
			sameName = ncNamList.FindLast(ncd => ncd.nnam == p_nnam);
			if (p_nnam == NcNam.DMY || sameName == null)
				this.Jnno = new St_jnno(null, false, null);
			else {
				// /////////////////////////////////////
				// 参照ありの場合
				// /////////////////////////////////////
				if (Char.IsUpper(m_nmod)) {
					// /////////////////////////////////////
					// それぞれＮＣデータを出力し共有しない
					// /////////////////////////////////////
					if (sameName.Jnno.Nknum != null || ncNamList.Exists(ncd => ncd.Jnno.Nknum != null && ncd.Jnno.Nknum == sameName))
						throw new Exception(this.nnam + "の参照で小文字の'n'と大文字の'N'のいずれかのみ使用可能です。");
					this.Jnno = new St_jnno(null, false, sameName);
					if (sameName.Jnno.same1 == null) {
						if (sameName.Jnno.SameNc) throw new Exception("qjehrfqhre");
						sameName.Jnno = new St_jnno(null, true, null);  // 共有ではない参照の先頭の場合再設定
					}
				}
				else {
					// /////////////////////////////////////
					// 小文字の場合は共有する
					// /////////////////////////////////////
					if (sameName.Jnno.Nknum == null && Char.IsUpper(sameName.Nmod) == false) {
						// 参照すべきＮＣデータが出力無しの設定の場合は参照しない
						this.Jnno = new St_jnno(null, false, null);
					}
					else {
						// 参照先が共有しない複数加工の場合
						if (sameName.Jnno.SameNc)
							throw new Exception(this.nnam + "の参照で小文字の'n'と大文字の'N'のいずれかのみ使用可能です。");
						// 工具表が異なる場合
						if (this.tsheet != sameName.tsheet)
							throw new Exception(this.nnam + "の参照で小文字の'n'の異なる工具表ＮＣデータへの参照はできません。");
						// 前のＮＣデータと同一条件（回転数、送り速度、移動量など）ではなく参照できない場合
						if (!this.Ni_Equals(sameName, out string mess))
							throw new Exception($"{mess}   {this.nnam}はＮＣデータ変換仕様が異なります。大文字の'N'を使用してください。");

						this.Jnno = new St_jnno(sameName, false, null);
						if (sameName.Jnno.Nknum == null)
							CamUtil.LogOut.warn.AppendLine(this.nnam + "は１つの出力ＮＣデータを共有して使用するように設定されました。");
					}
				}
			}
			return;
		}




		/// <summary>
		/// NcDataを設定する
		/// </summary>
		private void SetNcData() {
			string ss;
			if (this.nnam == NcNam.DMY) return;

			if ((this.Jnno.same1 ?? this.Jnno.Nknum) != null)
				this.Ncdata = (this.Jnno.same1 ?? this.Jnno.Nknum).Ncdata;
			else {
				// ＮＣＳＰＥＥＤ検証済みの場合のみ手順のあるフォルダー内のＮＣデータ等を使用する
				this.Ncdata = new NcData(this.nnam, (NcdTool.Tejun.Ncspeed) ? NcdTool.Tejun.TjnDir : null);
				if (NcdTool.Tejun.BaseNcForm.Id != this.Ncdata.ncInfo.xmlD.BaseNcFormat.Id)
					throw new Exception(this.nnam + "において、手順内にGENERAL, 5AXISのＮＣデータが混在しています。");
				if ((ss = this.Ncdata.ncInfo.xmlD.ClpCheck()) != null)
					throw new Exception(this.nnam + "において、クリアランス高さが２０mm未満のＮＣデータが存在する " + ss);
			}
		}

		/// <summary>
		/// ＫＤＡＴＡの情報を入力( nsgt2, tdat.nsgt1 を作成）
		/// </summary>
		/// <param name="kakojun"></param>
		/// <param name="mcnName"></param>
		private void Ncsget(ref int kakojun, string mcnName) {
			Kogu kog;
			NcNam ssnam = this.Jnno.same1 ?? this.Jnno.Nknum;
			this.m_tdat = new RO_Collection<Kogu>.InnerList();

			// 存在しないＮＣデータの場合、情報も取らない
			if (Ncdata == null) {
				return;
			}
			// 存在しないＮＣデータの場合、情報も取らない
			if (Ncdata.fsiz < 0) {
				if (ssnam == null)
					CamUtil.LogOut.warn.AppendLine(nnam + " : ＮＣデータが存在しません。");
				return;
			}

			// 参照しているＮＣデータの処理
			if (ssnam != null) {
				// tdatのコピー
				foreach (Kogu skog in ssnam.Tdat) {
					kog = new Kogu(this, skog, ++kakojun);
					m_tdat.Add(kog);
				}
				return;
			}

			if (Ncdata.fsiz == 0)
				CamUtil.LogOut.warn.AppendLine(nnam + " : ＮＣデータが空です。");
			if (Ncdata.Tld.Length != Ncdata.ncInfo.xmlD.ToolCount)
				throw new Exception("tld ncInfo のエラー");

			// ////////////
			// tdat の作成
			// ////////////
			foreach (NcDataT stld in Ncdata.Tld) {
				kog = new Kogu(this, stld, ++kakojun, mcnName);
				m_tdat.Add(kog);
			}
			if (Itdat == 0)
				CamUtil.LogOut.warn.AppendLine("加工情報が空です。 " + nnam + " ＣＡＭから再出力してください。");
		}

		private bool Ni_Equals(NcNam ssnam, out string mess) {
			mess = "01.メインとサブ";
			mess = "02.ＮＣデータ名"; if (this.nnam != ssnam.nnam) return false;
			mess = "03.マクロ加工方向反転"; if (nggt.rev != ssnam.nggt.rev) return false;
			mess = "04.軸のミラー"; if (nggt.Mirr != ssnam.nggt.Mirr) return false;
			mess = "05.軸の移動量"; if (nggt.trns != ssnam.nggt.trns) return false;
			mess = "06.金型の材質"; if (nggt.zaisGrp != ssnam.nggt.zaisGrp) return false;
			mess = "07.工具長補正量"; if (St_nggt.LHosei.Equals(nggt.ToolLengthHosei, ssnam.nggt.ToolLengthHosei) == false) return false;
			mess = "11.回転数の増減比"; if (Math.Abs(nmgt.Sratt - ssnam.nmgt.Sratt) > FLMIN) return false;
			mess = "12.送り速度の増減比"; if (Math.Abs(nmgt.Fratt - ssnam.nmgt.Fratt) > FLMIN) return false;
			mess = "13.２次元３次元区分"; if (nmgt.Dimn != ssnam.nmgt.Dimn) return false;
			mess = ""; return true;
		}

		// /////////////////
		// 倒れ補正値など
		// /////////////////

		/// <summary>
		/// ポストのコメントが'S'隅取り、'P'ペンシル加工の場合のＬ/Ｚ補正量を計算する
		/// </summary>
		/// <returns>Ｌ/Ｚ補正量</returns>
		public double SumiPencil() {
			double zido, ftmp;
			string comm;
			bool err;

			if (this.Tdat[0].Tld.XmlT.ClMv_Z_axis != 0.0) return 0.0; // ＣＬ移動済の場合は入力しない

			// 残し量が０ではない場合は入力しない
			foreach (Kogu kog in this.Tdat)
				if (kog.Tld.XmlT.NOKOS.HasValue && kog.Tld.XmlT.NOKOS.Value > 0.0) return 0.0;

			// 工具間でコメントが"S", "P" で同一の場合以外は入力しない
			err = false;
			comm = "";
			foreach (Kogu kog in this.Tdat) {
				switch (kog.Tld.XmlT.CMMNT) {
				case "S":
				case "P":
					if (comm == "") comm = "SP";
					else if (comm != "SP") err = true;
					break;
				default:
					if (comm == "") comm = "XX";
					else if (comm != "XX") err = true;
					break;
				}
			}
			if (err) System.Windows.Forms.MessageBox.Show(this.nnam + " はポストのコメントが異なるデータが定義されています。");
			if (err || comm != "SP") return 0.0;

			// 工具間で工具径によりＺ移動量が異なる場合は入力しない
			err = false;
			zido = 0.0;
			foreach (Kogu kog in this.Tdat) {
				switch (kog.Tld.XmlT.CMMNT) {
				case "S": ftmp = kog.TsetCHG.Tset_diam >= 2.0 ? 0.01 : 0.02; break;
				case "P": ftmp = 0.01; break;
				default: throw new Exception("qkfbqehr");
				}
				if (zido == 0.0) zido = ftmp;
				else if (Math.Abs(zido - ftmp) > 0.001) err = true;
			}
			if (err) System.Windows.Forms.MessageBox.Show(this.nnam + " は工具径が異なるデータが定義されています。");
			if (err) return 0.0;
			return zido;
		}
		/// <summary>
		/// ポストのコメントが'PU'、'PZ'の場合の送り速度減速の可否を出力する
		/// </summary>
		/// <returns>送り速度減速する場合 true</returns>
		public bool PUPZ() {
			foreach (Kogu kog in this.Tdat)
				if (kog.Tld.XmlT.CMMNT == "PU" || kog.Tld.XmlT.CMMNT == "PZ") return true;
			return false;
		}
	}
}