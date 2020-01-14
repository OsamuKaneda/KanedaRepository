using System;
using System.Collections.Generic;
using System.Text;

using CamUtil;

namespace CamUtil.LCode
{
	/// <summary>
	/// 元のＮＣデータの各コードの仕様を設定します[不変]
	/// </summary>
	public class NcDigit
	{
		/// <summary>LCode.NumCode で使用する座標値を表すＮＣコードです。ＣＡＭや加工機は異なることがあります。</summary>
		private const string indexCoodinate = "ABCIJKQRUVWXYZ";

		private const string indexStr = ";" + StringCAM.ABC0 + "#";

		// /////////////
		// 以上 static
		// /////////////





		/// <summary>各ＮＣデータコードＡ－Ｚの仕様</summary>
		public CamUtil.RO_ListChar<NcDigits> Data { get { return m_Data.AsReadOnly; } }
		private readonly CamUtil.RO_ListChar<NcDigits>.InnerArray m_Data;

		/// <summary>
		/// NcTejun の場合（usr9/ASDM/PTP, NCSPEED_edit, NCSPEED_org のフォーマット）の Post を作成します
		/// </summary>
		public NcDigit() : this(indexCoodinate, 1000) {; }

		/// <summary>
		/// ポストプロセッサから NcDigit を作成します
		/// </summary>
		/// <param name="post">ポストプロセッサ</param>
		public NcDigit(PostProcessor post) : this(indexCoodinate, 1000) {
			// //////////////////////////////////////////////////////////////////
			// 現状、各ＣＡＭシステム・ポストプロセッサにおいて標準と変更はない
			// //////////////////////////////////////////////////////////////////
		}
		/// <summary>
		/// 加工機情報から NcDigit を作成します
		/// </summary>
		/// <param name="machID">ポストの名称</param>
		/// <param name="digit">座標値の場合で小数点なしの場合の単位を表します</param>
		public NcDigit(Machine.MachID machID, int digit) {
			string zahyo = "ABCIJKQRUVWXYZ";

			m_Data = new RO_ListChar<NcDigits>.InnerArray(NcDigit.indexStr);
			foreach (char cc in NcDigit.indexStr) {
				if (cc == 'F')
					m_Data[cc] = new NcDigits(zahyo.IndexOf(cc) >= 0, (int?)null);
				else
					m_Data[cc] = new NcDigits(zahyo.IndexOf(cc) >= 0, zahyo.IndexOf(cc) >= 0 ? digit : (int?)null);
			}
		}
		/// <summary>
		/// 座標値のＮＣコードリストとその単位から NcDigit を作成します
		/// </summary>
		/// <param name="zahyo">座標値のリストです</param>
		/// <param name="digit">座標値の場合で小数点なしの場合の単位を表します</param>
		private NcDigit(string zahyo, int digit) {
			m_Data = new RO_ListChar<NcDigits>.InnerArray(NcDigit.indexStr);
			foreach (char cc in NcDigit.indexStr) {
				if (cc == 'F')
					m_Data[cc] = new NcDigits(zahyo.IndexOf(cc) >= 0, (int?)null);
				else
					m_Data[cc] = new NcDigits(zahyo.IndexOf(cc) >= 0, zahyo.IndexOf(cc) >= 0 ? digit : (int?)null);
			}
		}

		/// <summary>
		/// ディープコピーコンストラクタ
		/// </summary>
		public NcDigit(NcDigit source) {
			this.m_Data = new RO_ListChar<NcDigits>.InnerArray(NcDigit.indexStr);
			foreach (char cc in NcDigit.indexStr)
				this.m_Data[cc] = source.m_Data[cc];
		}

		/// <summary>
		/// 各ＮＣコードの小数点の処理方法を設定する
		/// </summary>
		public readonly struct NcDigits
		{
			/// <summary>ＮＣコードが記号（;%/(#）の場合</summary>
			public static NcDigits symbol = new NcDigits(false, null);

			/// <summary>ＮＣコードが座標値を表す場合trueとなります</summary>
			public readonly bool axis;
			/// <summary>ＮＣコードが座標値で小数点なしの場合の桁数を表します</summary>
			public readonly int? sdgt;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="p_axis"></param>
			/// <param name="p_sdgt"></param>
			public NcDigits(bool p_axis, int? p_sdgt) {
				this.axis = p_axis;
				this.sdgt = p_sdgt;
				if (this.axis) {
					if (!this.sdgt.HasValue) throw new Exception("NcDigits エラー");
				}
				else {
					if (this.sdgt.HasValue) throw new Exception("NcDigits エラー");
				}
			}
		}
	}
}
