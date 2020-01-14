using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil.LCode
{
	/// <summary>
	/// ＮＣデータを出力する複数行を保存します。
	/// </summary>
	public class OutLine : ICloneable
	{
		/// <summary>変換後のＮＣデータ</summary>
		public string Txt { get { return m_txt; } }
		private string m_txt;
		/// <summary>前に追加する複数のＮＣデータ</summary>
		private List<string> mae;
		/// <summary>後ろに追加する複数のＮＣデータ</summary>
		private List<string> ato;

		/// <summary>コメントを出力する場合は true</summary>
		public bool CommOut { set { m_commOut = value; } }
		private bool m_commOut;

		/// <summary>出力するコメント</summary>
		public string Comment { get { return m_comment; } }
		private string m_comment;

		/// <summary>次に出力すべき行の番号。１行づつ出力する場合に使用します</summary>
		public int OutputNo { get { return m_outputNo; } }
		private int m_outputNo;


		/// <summary>
		/// 未定義のデータとして作成する唯一のコンストラクタ
		/// </summary>
		/// <param name="comm">コメントを出力する場合は true</param>
		internal OutLine(bool comm) {
			this.m_txt = null;
			this.mae = new List<string>();
			this.ato = new List<string>();
			this.m_commOut = comm;
			this.m_comment = "";
			this.m_outputNo = 0;
		}

		/// <summary>クローンを作成します</summary>
		/// <returns></returns>
		public OutLine Clone() {
			OutLine clone = (OutLine)this.MemberwiseClone();
			clone.mae = new List<string>();
			foreach (string str in this.mae) clone.mae.Add(str);
			clone.ato = new List<string>();
			foreach (string str in this.ato) clone.ato.Add(str);
			return clone;
		}
		object ICloneable.Clone() { return Clone(); }

		/// <summary>
		/// 前後に追加した行はそのままで、ＮＣデータの出力１行を指定した文字列で置き換えます
		/// </summary>
		/// <param name="ncline">置き換える文字列</param>
		public void Set(string ncline) { this.m_txt = ncline; }

		/// <summary>
		/// 前後に追加した行はそのままで、ＮＣデータの出力１行とコメントを指定した文字列で置き換えます
		/// </summary>
		/// <param name="ncline">置き換えるＮＣ文字列</param>
		/// <param name="comment">置き換えるコメント文字列</param>
		public void Set(string ncline, string comment) { this.m_txt = ncline; this.m_comment = comment; }

		/// <summary>基準の行の直前に行を追加します</summary>
		public void MaeAdd(string str) { mae.Add(str); }
		/// <summary>最初の行の前に行を追加します</summary>
		public void MaeInsert(string str) { mae.Insert(0, str); }
		/// <summary>最後の行の後ろに行を追加します</summary>
		public void AtoAdd(string str) { ato.Add(str); }
		/// <summary>基準の行の直後に行を追加します</summary>
		private void AtoInsert(string str) { ato.Insert(0, str); }

		/// <summary>
		/// 出力するＮＣデータの行数を返します。コメントのみが出力される行も含みます。
		/// </summary>
		public int Count {
			get {
				if ((Txt != null && Txt != "") || (m_commOut && this.Comment.Length > 0))
					return (mae != null ? mae.Count : 0) + (ato != null ? ato.Count : 0) + 1;
				else
					return (mae != null ? mae.Count : 0) + (ato != null ? ato.Count : 0);
			}
		}
		/// <summary>
		/// 出力する行の１行を返します
		/// </summary>
		/// <param name="lineNo">行を指定する数値。０以上、Count未満です。</param>
		/// <returns>指定した１行の文字列</returns>
		public string this[int lineNo] {
			get {
				int mid = 0;
				if ((Txt != null && Txt != "") || (m_commOut && this.Comment.Length > 0))
					mid++;
				m_outputNo = lineNo + 1;

				if (lineNo < mae.Count)
					return mae[lineNo];
				else if (lineNo < mae.Count + mid) {
					if (m_commOut && m_comment.Length > 0)
						return Txt + "(" + m_comment + ")";
					else
						return Txt;
				}
				else
					return ato[lineNo - mae.Count - mid];
			}
		}

		/// <summary>
		/// 出力するＮＣデータの全行を一括して返します。行末は"\r\n"です
		/// </summary>
		/// <returns></returns>
		public string NcLineOut() {
			string str = "";
			for (int ii = 0; ii < Count; ii++)
				str += this[ii] + "\r\n";
			if (str.Length >= 2)
				str = str.Substring(0, str.Length - 2);
			m_outputNo = Count;
			return str;
		}

		/// <summary>
		/// シーケンス番号を挿入します
		/// </summary>
		/// <param name="seqNo">この値に１を加えた後に挿入します</param>
		public void Sequence(ref long seqNo) {
			for (int ii = 0; ii < mae.Count; ii++) mae[ii] = "N" + (++seqNo).ToString() + " " + mae[ii];
			if (Txt != "") m_txt = "N" + (++seqNo).ToString() + " " + m_txt;
			for (int ii = 0; ii < ato.Count; ii++) ato[ii] = "N" + (++seqNo).ToString() + " " + ato[ii];
		}
		/// <summary>
		/// 各行の末尾に指定の文字列を追加します
		/// </summary>
		/// <param name="str">追加する文字列</param>
		public void AddString(string str) {
			for (int ii = 0; ii < mae.Count; ii++) mae[ii] += str;
			if (Txt != "") m_txt += str;
			for (int ii = 0; ii < ato.Count; ii++) ato[ii] += str;
		}

		/// <summary>
		/// 出力するコメントを追加します。すでにコメントが存在する場合は末尾にスペースを加えて追加します。コメント出力可否は変化しません。
		/// </summary>
		/// <param name="comm">追加するコメント</param>
		public void Addcomment(string comm) {
			if (m_comment != "") m_comment += " ";
			m_comment += comm;
		}

		/// <summary>コメントを削除します</summary>
		public void DelelseComment() { m_comment = ""; }

		/// <summary>このインスタンスと指定した OutLine オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public bool Equals(OutLine obj) {
			if (this.m_txt != obj.m_txt) return false;
			if (this.mae.Count != obj.mae.Count) return false;
			if (this.ato.Count != obj.ato.Count) return false;
			for (int ii = 0; ii < this.mae.Count; ii++) if (this.mae[ii] != obj.mae[ii]) return false;
			for (int ii = 0; ii < this.ato.Count; ii++) if (this.ato[ii] != obj.ato[ii]) return false;
			if (this.m_commOut != obj.m_commOut) return false;
			if (this.m_comment != obj.m_comment) return false;
			if (this.m_outputNo != obj.m_outputNo) return false;
			return true;
		}
	}
}
