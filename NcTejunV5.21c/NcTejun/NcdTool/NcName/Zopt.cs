using System;
using System.Collections.Generic;
using System.Text;

namespace NcdTool.NcName
{
	/// <summary>
	/// ＮＣデータの状態（存在、メインなど）により分岐するための情報
	/// </summary>
	class Zopt
	{
		/// <summary>出力すべきＮＣデータのとき"true"となる静的インスタンス</summary>
		public static Zopt NcOutput = new Zopt { output = true, not_dummy = true, exist_ncdata = true, exist_match = true, not_sansho = true };
		/// <summary>ＮＣデータが存在するとき"true"となる静的インスタンス</summary>
		public static Zopt NcExist = new Zopt { not_dummy = true, exist_ncdata = true };
		/// <summary>ＮＣデータがダミー（=="&amp;"）でないときに"true"となる静的インスタンス</summary>
		public static Zopt NotDummy = new Zopt { not_dummy = true };
		/// <summary>ＮＣデータが工具表とのマッチングをしているときに"true"となる静的インスタンス</summary>
		public static Zopt NcMatch = new Zopt { not_dummy = true, matching = true };
		/// <summary>手順書に出力すべきＮＣデータのとき"true"となる静的インスタンス</summary>
		public static Zopt NcTjnout = new Zopt { not_dummy = true, exist_ncdata = true, exist_match = true };



		/// <summary>   1: output data    IsUpper(nmod) || nknum!=nul 手順で出力すると設定（大文字の'N'あるいは参照データ）</summary>
		private bool output = false;
		/// <summary>   2: reject dummy  ncsd.ncdata!=null　ダミーではないＮＣデータ</summary>
		private bool not_dummy = false;
		/// <summary>   4: exist NCdata  fsiz&gt;0</summary>
		private bool exist_ncdata = false;
		/// <summary>   8: exist nc-inf   itdat&gt;0</summary>
		private bool exist_match = false;
		/// <summary>  16: reject sansho  nknum==NULL</summary>
		private bool not_sansho = false;
		/// <summary>  32: matching only</summary>
		private bool matching = false;
		// /// <summary> 512: GENERALdata only mcno=="GENERAL"</summary>
		// private bool general = false;
		// /// <summary>  64: mach uniq data nkmch==NULL</summary>
		// private bool uniq_mach = false;
		// /// <summary>   8: uniq nc-data   ninum[2]==NULL</summary>
		// private bool uniq_ncdata = false;
		// /// <summary>  16: not main-data  subp[0]==NULL</summary>
		// private bool notmain = false;
		// /// <summary>1024: not main||sub</summary>
		// private bool normalonly = false;
		// /// <summary>  32: exit conv-data stat()==0</summary>
		// private bool exist_conv = false;
		// /// <summary> 128: insert T00 tool</summary>
		// private bool t100 = false;

		/// <summary>メッセージ</summary>
		public string Mess { get { return m_mess; } }
		private string m_mess = "";

		/// <summary>
		/// 処理対象のＮＣデータか判定する
		/// </summary>
		/// <param name="ncsd">ＮＣデータ情報</param>
		/// <returns>処理対象の場合 true</returns>
		internal bool Nctoks(NcName.NcNam ncsd) {
			if (output)
				if (Char.IsUpper(ncsd.Nmod) != true && ncsd.Jnno.Nknum == null) {
					m_mess = ncsd.nnam + "加工手順にて出力が抑制されています";
					return false;
				}
			if (not_dummy) {
				if (ncsd.Ncdata == null) {
					if (ncsd.nnam != NcNam.DMY) throw new Exception("jefdbqefbdqhewbd");
					if (ncsd.Itdat.HasValue) if (ncsd.Itdat != 0) throw new Exception("jefdbqefbdqhewbd");
					m_mess = ncsd.nnam + "ＮＣデータがダミーです";
					return false;
				}
				else {
					if (ncsd.nnam == NcNam.DMY) throw new Exception("jefdbqefbdqhewbd");
				}
			}
			if (exist_ncdata) {
				// ========= チェック ===========
				// １．ncsd.ncdata == null		ダミーのＮＣデータ（nnam == NcNam.DMY）
				// ２．ncsd.ncdata.fsiz < 0		ＮＣデータが存在しない（ＮＣ加工情報は存在する）
				// 上記は itdat.HasValue の場合、 ncsd.itdat == 0 と同等
				if (ncsd.Ncdata != null && ncsd.Ncdata.fsiz >= 0)
					if (ncsd.Itdat.HasValue && ncsd.Itdat == 0) throw new Exception("jefdbqefbdqhewbd");

				if (ncsd.Ncdata != null && ncsd.Ncdata.fsiz < 0) {
					if (ncsd.Itdat.HasValue) if (ncsd.Itdat != 0) throw new Exception("jefdbqefbdqhewbd");
					m_mess = ncsd.nnam + "ＮＣデータが存在しません";
					return false;
				}
			}
			if (exist_match)
				if (ncsd.Tdat.Exists(Match) == false) {
					m_mess = ncsd.nnam + "使用する工具が１つも工具表に存在しません";
					return false;
				}
			if (not_sansho)
				if (ncsd.Jnno.Nknum != null) {
					m_mess = ncsd.nnam + "他のＮＣデータを参照しているデータです";
					return false;
				}
			if (matching)
				if (NcTejun.TejunSet.ToolSheet.Match(ncsd.tsheet) == false) {
					m_mess = ncsd.nnam + "工具表とのマッチングをしていないデータです";
					return false;
				}
			/*
			if (general)
				if (Program.tejun.baseNcFormName != CamUtil.BaseNcForm.GENERAL) {
					m_mess = ncsd.nnam + "GENERALで出力されたデータではありません";
					return false;
				}
			*/
			/*
			if (uniq_mach)
				if (ncsd.jnno.nkmch != null) {
					m_mess = ncsd.nnam + "他のＮＣデータを参照しているデータです";
					return false;
				}
			*/
			/*
			if (uniq_ncdata)
				if (ncsd.ninum1 != null) {
					m_mess = ncsd.nnam + "共通に使用するデータです";
					return false;
				}
			*/
			return true;
		}

		/// <summary>工具が選定されたＮＣ出力可能な工具単位データが１つ以上存在するかを判定</summary>
		private bool Match(Kogu skog) { return skog.Output; }
	}
}
