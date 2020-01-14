using System;
using System.Collections.Generic;
using System.Text;

namespace NcdTool.NcName
{
	/// <summary>
	///	ＣＡＭ作成時のＮＣデータに依存する工具の情報[不変]
	/// </summary>
	class NcDataT
	{
		/// <summary>このツールセットＣＡＭの工具寿命長(m)のＤＢ条件</summary>
		public double LifeDB { get { return m_lifeDB; } }
		private readonly double m_lifeDB;

		/// <summary>ＮＣデータ内の順序（１から開始）</summary>
		public int SetJun { get { return m_setJun; } }
		private readonly int m_setJun;			// ＮＣデータ内のセット順番

		/// <summary>工具単位のＸＭＬ内のデータ</summary>
		public CamUtil.CamNcD.XmlNavi.Tool XmlT { get { return m_xmlT; } }
		private readonly CamUtil.CamNcD.XmlNavi.Tool m_xmlT;

		/// <summary>
		/// コンストラクタ（ＰＣオンリー）
		/// </summary>
		/// <param name="ii"></param>
		/// <param name="ncInfo"></param>
		/// <param name="fulNcName"></param>
		internal NcDataT(int ii, CamUtil.CamNcD.NcInfo ncInfo, string fulNcName) {
			m_setJun = ii + 1;
			m_xmlT = ncInfo.xmlD[ii];

			// 送り速度最適化の場合は送り速度のみでなく回転数の変更もＮＣデータに反映
			// されるため、ここでＮＣデータのＳ値を取り込み確認する
			if (m_xmlT.SmFDC) {
				double schiNC = SetschiNC(fulNcName);
				// 比率は小数点以下３桁までが等しい場合は正しいとする in 2016/08/01
				if (Math.Abs(schiNC / ncInfo.xmlD[ii].SPIND - ncInfo.xmlD[ii].SmSPR) > 0.001)
					throw new Exception("最適化の回転数エラー");
			}
			ToolSetInfo.TSetCAM tsetCAM = new ToolSetInfo.TSetCAM(ncInfo.xmlD[ii].SNAME);
			m_lifeDB = tsetCAM.LifeMaxBase;
		}

		/// <summary>送り速度最適化の場合はＮＣデータのＳ値を取り込む</summary>
		private double SetschiNC(string fulNcName) {
			// 送り速度自動制御の場合はＮＣデータのＳ値を取り込む
			string ddat = "";
			using (System.IO.StreamReader sr = new System.IO.StreamReader(fulNcName)) {
				while (ddat.IndexOf("G100T") < 0) ddat = sr.ReadLine();
			}
			int index_s = ddat.IndexOf('S') + 1;
			int index_n = index_s;
			while (ddat[index_n] == ' ') index_n++;
			while (index_n < ddat.Length && Char.IsNumber(ddat[index_n])) index_n++;
			return Convert.ToDouble(ddat.Substring(index_s, index_n - index_s));
		}






		// ///////////////////////////////////////////////////////////
		// 以下は任意指示値の出力
		// ///////////////////////////////////////////////////////////

		/// <summary>回転数任意指示値（ＸＭＬの値のみ）</summary>
		public double SninnXML {
			get {
				XmlT.OPTION(out double? m_srate, out double? m_frate);
				if (m_srate.HasValue)
					return m_srate.Value * XmlT.SPIND;
				else {
					if (XmlT.parent.SmNAM != null && XmlT.SmFDC == false)
						return XmlT.SmSPR * XmlT.SPIND;
					else
						return XmlT.SPIND;
				}
			}
		}
		/// <summary>送り速度任意指示値（ＸＭＬの値のみ）</summary>
		public double FninnXML {
			get {
				XmlT.OPTION(out double? m_srate, out double? m_frate);
				if (m_frate.HasValue)
					return m_frate.Value * XmlT.FEEDR;
				else {
					if (XmlT.parent.SmNAM != null && XmlT.SmFDC == false)
						return XmlT.SmFDR * XmlT.FEEDR;
					else
						return XmlT.FEEDR;
				}
			}
		}

		/// <summary>工具寿命任意指示値（ＸＭＬの値のみ）</summary>
		public double LninnXML {
			get {
				if (XmlT.OPTLF.HasValue)
					return XmlT.OPTLF.Value * LifeDB;
				else
					return LifeDB;
			}
		}
	}
}
