using System;
using System.Collections.Generic;
using System.Text;

using CamUtil;

namespace NcCode.nccode
{
	/// <summary>
	/// ＮＣデータの仕様を設定します。
	/// ＧＥＮＥＲＡＬか各加工機用に変換されたデータか、区分がまだ不明確です。
	/// </summary>
	internal static class Post
	{
		/// <summary>実数絶対値の最小</summary>
		internal const double minim = 0.0001;

		private const string indexStr = ";" + StringCAM.ABC0;
		private const string indexCoodinate = "ABCIJKQRUVWXYZ";

		/// <summary>ＮＣデータ作成時のポスト名</summary>
		public static BaseNcForm pName = BaseNcForm.EMPTY;

		/// <summary>各ＮＣデータコードＡ－Ｚの仕様</summary>
		public static CamUtil.RO_ListChar<Posts> PostData { get { return m_PostData.AsReadOnly; } } static CamUtil.RO_ListChar<Posts>.InnerArray m_PostData;

		/// <summary>各ＮＣの整数化コードを保存する倍率。－１は加工機により決定される。</summary>
		private static readonly CamUtil.RO_ListChar<int>.InnerArray m_digit = new RO_ListChar<int>.InnerArray(
			new int[]{
			//  ;   A   B   C  D     E   F   G  H   I   J   K  L  M  N  O  P   Q   R     S  T   U   V   W   X   Y   Z
			    1, -1, -1, -1, 1, 1000, -1, -1, 1, -1, -1, -1, 1, 1, 1, 1, 1, -1, -1, 1000, 1, -1, -1, -1, -1, -1, -1 },
			Post.indexStr
		);

		/// <summary>
		/// イニシャライズ
		/// </summary>
		public static void Init(CamUtil.BaseNcForm bForm) {
			pName = bForm;

			switch (pName.Id) {
			case CamUtil.BaseNcForm.ID.GENERAL:
				foreach (char cc in indexCoodinate) m_digit[cc] = 1000;
				foreach (char cc in "F") m_digit[cc] = 1;
				foreach (char cc in "G") m_digit[cc] = CamUtil.LCode.Gcode.unt0;
				break;
			default:
				throw new Exception("qwfnqrjfnj");
			}
			m_PostData = new RO_ListChar<Posts>.InnerArray(Post.indexStr);
			foreach (char cc in Post.indexStr) { m_PostData[cc] = new Posts((indexCoodinate.IndexOf(cc) >= 0), m_digit[cc]); }
		}
		/// <summary>
		/// 座標値の設定
		/// </summary>
		/// <param name="jj"></param>
		public static void ZahyoSet(int jj) {
			foreach (char cc in indexCoodinate) m_digit[cc] = jj;
			m_PostData = new RO_ListChar<Posts>.InnerArray(Post.indexStr);
			foreach (char cc in Post.indexStr) { m_PostData[cc] = new Posts((indexCoodinate.IndexOf(cc) >= 0), m_digit[cc]); }
		}
		/// <summary>
		/// 座標値の設定
		/// </summary>
		/// <param name="jj"></param>
		public static void FcodeSet(int jj) {
			//m_sdgt[6] = jj;
			m_digit['F'] = jj;
			Post.m_PostData['F'] = new Posts(Post.m_PostData['F'].axis, jj);
		}

		// /////////////
		// 以上 static
		// /////////////




		/// <summary>
		/// 各ＮＣコードの小数点の処理方法を設定する
		/// </summary>
		public readonly struct Posts
		{
			/// <summary>ＮＣコードが座標値を表す場合true</summary>
			public readonly bool axis;

			/// <summary>ＮＣコードが座標値で小数点なしの場合の桁数を表す</summary>
			public readonly int sdgt;
			/// <summary>
			/// 
			/// </summary>
			/// <param name="p_axis"></param>
			/// <param name="p_sdgt2"></param>
			public Posts(bool p_axis, int p_sdgt2) {
				this.axis = p_axis;
				this.sdgt = p_sdgt2;
			}
		}
	}
}
