using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;

namespace CamUtil.LCode
{
	/// <summary>
	/// ＮＣデータ１行内に含まれるＮＣコードのリストを読み取り専用として公開します。
	/// </summary>
	public class NumList : RO_Collection<NcLineCode.NumCode>
	{
		/// <summary>
		/// ＮＣデータ１行内に含まれるＮＣコードのリスト。List&lt;NcLineCode.numCode&gt;を private readonly として隠蔽します。
		/// </summary>
		public class NumListData
		{
			/// <summary>ＮＣデータ１行内に含まれるＮＣコードのリスト</summary>
			private readonly List<NcLineCode.NumCode> numL;
			/// <summary>ＮＣデータ１行内に含まれるＮＣコードの読み取り専用のリスト</summary>
			public readonly NumList AsReadOnly;

			/// <summary>空のリストを作成します</summary>
			public NumListData() { numL = new List<NcLineCode.NumCode>(); AsReadOnly = new NumList(this.numL); }

			/// <summary>単一の NcLineCode.numCode より作成します</summary>
			/// <param name="num">ＮＣコード１件の情報</param>
			public NumListData(NcLineCode.NumCode num) : this() { numL.Add(num); }

			/// <summary>指定した配列よりコピーした要素から作成します</summary>
			/// <param name="array">ＮＣコード情報の配列</param>
			public NumListData(IEnumerable<NcLineCode.NumCode> array) { numL = new List<NcLineCode.NumCode>(array); AsReadOnly = new NumList(this.numL); }

			/// <summary>
			/// ＮＣデータ入力行ncLineから作成します。
			/// </summary>
			/// <param name="ncLine">ＮＣデータ入力行</param>
			/// <param name="post">ＣＡＭポストの小数点桁数</param>
			public NumListData(string ncLine, NcDigit post)
				: this() {
				try {
					foreach(string ss in StringCAM.SplitNcCode(ncLine))
						numL.Add(new NcLineCode.NumCode(ss[0], ss.Substring(1), post.Data[ss[0]]));
				}
				catch (FormatException) {
					MessageBox.Show("ＮＣデータの文字列 '" + ncLine + "' が不正です", "NcCode", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Application.Exit();
					throw;
				}
				catch (Exception ex) {
					MessageBox.Show(ex.Message + " NCDATA=" + ncLine, "NcCode", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Application.Exit();
					throw;
				}
			}
		}






		/// <summary>
		/// ＮＣデータ１行内に含まれるＮＣコードのリストの読み取り専用インスタンスを作成します
		/// </summary>
		/// <param name="array">元のNcLineCode.numCodeのリスト</param>
		private NumList(IList<NcLineCode.NumCode> array) : base(array) { ;}

		/// <summary>
		/// ＮＣコードごとのＮＣデータ１行内での指定数を出力します
		/// </summary>
		/// <param name="cc">出力するＮＣコード</param>
		/// <returns>ＮＣコードの１行内での指定数</returns>
		public int NcCount(char cc) { return this.FindAll(num => num.ncChar == cc).Count; }

		/// <summary>
		/// ＮＣデータ１行に定義されたＮＣコードの最初に見つかった情報を出力します。存在しない場合はSystem.ArgumentNullExceptionとなります
		/// </summary>
		/// <param name="cc">出力するＮＣコード</param>
		/// <returns>コードの情報</returns>
		public NcLineCode.NumCode Code(char cc) { return this.Find(num => num.ncChar == cc); }

		/// <summary>このインスタンスと指定した NumListRO オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public bool Equals(List<NcLineCode.NumCode> obj) {
			if (this.Count != obj.Count) return false;
			for (int ii = 0; ii < this.Count; ii++)
				if (!this[ii].Equals(obj[ii])) return false;
			return true;
		}
	}
}