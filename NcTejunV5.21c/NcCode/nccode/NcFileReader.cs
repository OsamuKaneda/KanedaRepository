using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace NcCode.nccode
{
	/// <summary>
	/// ＮＣファイルの読取りをするクラスです。ユニックスのファイル、リワインドにも対応しています。ＮＣＳＰＥＥＤのコメントの削除もここで実施しています。
	/// </summary>
	internal sealed class NcFileReader : IDisposable
	{
		// ////////////////////////
		// 暫定チェック 2015/06/23
		// ////////////////////////
		/// <summary>暫定チェック。シミュレーションのデータの場合</summary>
		static public bool sim = false;

		//private bool PC;
		/// <summary>StreamReaderクラスのインスタンス</summary>
		public StreamReader fp;
		/// <summary>ＮＣデータのファイル名</summary>
		public string Ncnam { get { return m_ncnam; } }
		private readonly string m_ncnam;
		/// <summary>Ｏ番号の開始行番号</summary>
		public long Sno { get { return m_sno; } }
		readonly long m_sno;

		/// <summary>読み込むときセミコロンを追加するときtrue</summary>
		private readonly bool addSemiColon;

		/// <summary>現在のストリームの位置がストリームの末尾かどうかを示す値を取得する</summary>
		public bool EndOfStream { get { return fp.EndOfStream; } }

		/// <summary>ＮＣデータの行数（進行状況の表示に使用 ADD in 2014/10/27）</summary>
		public int MaxLineNo { get { return m_maxLineNo; } }
		private readonly int m_maxLineNo;

		/// <summary>
		/// フルファイル名からNcFileReaderを作成します
		/// </summary>
		/// <param name="path">フルファイル名</param>
		public NcFileReader(string path) {
			m_maxLineNo = File.ReadAllLines(path).Length;
			fp = new StreamReader(path);
			addSemiColon = true;
			//m_interval = Math.Min(5000, 1 + m_maxLineNo / 45);
			m_ncnam = Path.GetFileName(path);
			m_sno = 1;

			sim = false;

		}

		/// <summary>
		/// ＮＣデータファイル名とフォルダーリストからNcFileReaderを作成します
		/// </summary>
		/// <param name="name">ファイル名</param>
		/// <param name="ncdir">探すフォルダーリスト</param>
		/// <param name="sfil">サブプログラム名のリスト</param>
		public NcFileReader(string name, string[] ncdir, List<string> sfil)
		{
			int spro = 0;
			string dirnam;
			//CamUtil.ServerUnix.FileInfo fInfo;
			FileInfo fInfoPC;

			this.m_ncnam = name;
			m_maxLineNo = 0;

			if (m_ncnam[0] == 'O') {
				spro = 1;
				for (int ii = 1; ii < this.m_ncnam.Length; ii++)
					if (Char.IsDigit(this.m_ncnam[ii]) == false) {
						spro = 0;
						break;
					}
			}

			// ＯをＰに変えて検索
			if (fp == null && spro == 1) {
				for (int ii = 0; ii < ncdir.Length; ii++) {
					if (ncdir[ii][0] != '\\') throw new Exception("ncdir ERROR in _main");
					dirnam = ncdir[ii] + "\\P" + this.m_ncnam.Substring(1);
					fInfoPC = new FileInfo(dirnam);
					if (fInfoPC.Exists) {
						this.fp = new StreamReader(dirnam);
						this.m_sno = 1;
						break;
					}
				}
			}
			// サブプロのＰＴＰからの検索
			if (fp == null) {
				for (int ii = 0; ii < ncdir.Length; ii++) {
					if (ncdir[ii][0] != '\\') throw new Exception("ncdir ERROR in _main");
					dirnam = ncdir[ii] + "\\" + m_ncnam;
					fInfoPC = new FileInfo(dirnam);
					if (fInfoPC.Exists) {
						this.fp = new StreamReader(dirnam);
						this.m_sno = 1;
						break;
					}
				}
			}
			// サブプログラム名リストより検索
			if (fp == null && spro == 1) {
				throw new Exception("ユニックスのファイルは使用できません");
			}

			addSemiColon = false;

			if (this.fp == null)
				throw new Exception(this.m_ncnam + " can not open.");

			sim = false;

			return;
		}

		/// <summary>
		/// この NcCode.nccode.NcFileReader オブジェクトによって使用されている StreamReader fp のリソースを開放します。
		/// </summary>
		public void Dispose() {
			if (fp != null) { fp.Dispose(); fp = null; }
		}

		/// <summary>
		/// 現在のストリームから１行分の文字を読み取り、そのデータを文字列として返します
		/// </summary>
		/// <returns></returns>
		public string ReadLine() {
			string ddat;
			ddat = fp.ReadLine();
			if (addSemiColon) ddat += ";";
			// ////////////////////////
			// 暫定チェック 2015/06/23
			// ////////////////////////
			if (sim == false) if (ddat.IndexOf("(SIM START)") >= 0)
					throw new Exception("qerfbwqrfhqwrb");
			return ddat;
		}

		/// <summary>
		/// オブジェクトを閉じます
		/// </summary>
		public void Close() {
			fp.Close();
		}

		/// <summary>
		/// ストリームの先頭に戻します
		/// </summary>
		public void Rewind() {
			if (fp.BaseStream.CanSeek == false)
				throw new Exception("fenwejfnwekj");

			if (!fp.EndOfStream) fp.ReadToEnd();
			if (!fp.EndOfStream) System.Windows.Forms.MessageBox.Show("リワインドのエラー");
			fp.BaseStream.Seek(0, SeekOrigin.Begin);
			return;
		}
	}
}
