using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace NcdTool.NcName
{
	/// <summary>
	/// ＣＡＭでデータを作成した時のＮＣデータの情報[不変(ncInfo内部を除く)]
	/// </summary>
	class NcData
	{
		/// <summary>ＮＣデータ名（手順での指定名）</summary>
		public readonly string nnam;
		/// <summary>ＮＣデータのExtension含むファイル名（ＰＣ）ＸＭＬデータもない場合は null。</summary>
		public readonly string fulnamePC;
		/// <summary>ファイルサイズ</summary>
		public readonly long fsiz;
		/// <summary>最終更新日時</summary>
		public readonly DateTime ftim;

		/// <summary>ＣＳＶによる新加工情報(add 2006.07.28)</summary>
		public readonly CamUtil.CamNcD.NcInfo ncInfo;

		/// <summary>ＣＡＭでデータを作成した時の工具単位の情報</summary>
		public NcDataT[] Tld { get { return m_tld.ToArray(); } } private readonly List<NcDataT> m_tld;

		/// <summary>
		/// データのセットコンストラクタ（唯一）
		/// </summary>
		/// <param name="p_nnam">ＮＣデータ名</param>
		/// <param name="spDir">ＮＣデータの存在を特別に確認するフォルダー名</param>
		public NcData(string p_nnam, string spDir) {

			string tmpn = null;
			this.m_tld = new List<NcDataT>();
			this.nnam = p_nnam;

			// 設定されたフォルダー内のＸＭＬデータを検索
			if (spDir != null) {
				tmpn = spDir + "\\" + nnam + ".xml";
				this.fulnamePC = Path.ChangeExtension(tmpn, null);
			}
			// ＰＴＰフォルダー内のＸＭＬデータを検索
			else {
				tmpn = CamUtil.ServerPC.FulNcName(nnam + ".xml");
				this.fulnamePC = Path.ChangeExtension(tmpn, ".ncd");
			}
			if (File.Exists(tmpn) == false)
				throw new Exception($"ＮＣ情報ファイル : {Path.GetFileName(tmpn)} が指定フォルダー : {Path.GetDirectoryName(tmpn)} に存在しない");

			// //////////////////////////////////////////
			// ＸＭＬ情報の作成
			// //////////////////////////////////////////
			this.ncInfo = new CamUtil.CamNcD.NcInfo(this.fulnamePC);
			if (this.ncInfo == null) throw new Exception("qefdbqwefhqwfb");

			// ＰＣのＮＣデータからファイル情報を得る
			if (File.Exists(this.fulnamePC)) {
				FileInfo aa = new FileInfo(this.fulnamePC);
				this.fsiz = aa.Length;
				this.ftim = aa.LastWriteTime;
			}
			else {
				this.fsiz = -1;
				this.ftim = DateTime.MinValue;
			}
			// /////////////////////////////////////
			// NcInfo から nsgt2,tld.nsgt1を作成する
			// /////////////////////////////////////
			//Set_Kdata(nnam);
			for (int ii = 0; ii < ncInfo.xmlD.ToolCount; ii++)
				this.m_tld.Add(new NcDataT(ii, ncInfo, this.fulnamePC));
		}
	}
}
