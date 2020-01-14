using System;
using System.Windows.Forms;

using System.IO;
using System.Text.RegularExpressions;

namespace MacroUtil
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Form1 : Form
	{
		private static Regex reg1 = new Regex("^O[0-9][0-9]*");
		private static Regex reg2 = new Regex("^O[0-9][0-9][0-9][0-9]$");

		/// <summary>
		/// 改行コードを取得（ "\r\n" OR "\n" ）
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string LastLineCode(string fileName) {
			using (var sr = new StreamReader(fileName)) {
				int data0 = 0, data1;
				while ((data1 = sr.Read()) != '\n') {
					if (data1 < 0) throw new Exception("改行コードが見つかりませんでした。");
					data0 = data1;
				}
				return (data0 == '\r') ? "\r\n" : "\n";
			}
		}




		/// <summary>
		/// 
		/// </summary>
		public Form1() {
			InitializeComponent();
		}

		/// <summary>
		/// Ｏ番号ごとにファイルを分割する
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Div_Click(object sender, EventArgs e) {
			StreamReader sr;
			StreamWriter sw;

			System.Windows.Forms.OpenFileDialog openFileDialog1 = new OpenFileDialog() {
				Multiselect = false,
				CheckFileExists = true,
				//InitialDirectory = @"\\nt0049np\shared\06-03_改善\01_スタッフ（企画部・改）\03_工程改善（製造部・改）\03 切削工程"
			};
			if (openFileDialog1.ShowDialog() == DialogResult.Cancel) { return; }

			// 改行コードを取得
			string lineCode = Form1.LastLineCode(openFileDialog1.FileName);

			// 分割
			toolStripStatusLabel1.Text = "分割開始";
			sr = null;
			sw = null;
			try {
				sr = new StreamReader(openFileDialog1.FileName);
				string ddat;
				Match match;
				string fnam;

				while (!sr.EndOfStream) {
					ddat = sr.ReadLine();
					if ((match = Form1.reg1.Match(ddat)).Success) {
						fnam = match.Value;
						if (fnam.Length > 5) throw new Exception("qwefbqebf");
						if (sw != null) {
							sw.Write("%" + lineCode);
							sw.Close();
						}
						while (fnam.Length < 5) fnam = "O0" + fnam.Substring(1);
						toolStripStatusLabel1.Text = fnam;
						Application.DoEvents();
						if (File.Exists(Path.GetDirectoryName(openFileDialog1.FileName) + "\\" + fnam)) {
							MessageBox.Show("ファイル" + fnam + "はすでに存在します。以下をキャンセルします");
							return;
						}
						sw = new StreamWriter(Path.GetDirectoryName(openFileDialog1.FileName) + "\\" + fnam);
						sw.Write("%" + lineCode);
					}
					if (sw == null) continue;
					if (ddat == String.Empty) continue;
					if (ddat == "%") continue;
					sw.Write(ddat + lineCode);
				}
				if (sw != null) {
					sw.Write("%" + lineCode);
				}
			}
			finally {
				if (sr != null) { sr.Dispose(); sr = null; }
				if (sw != null) { sw.Dispose(); sw = null; }
			}
			toolStripStatusLabel1.Text = "分割終了";
			return;
		}

		/// <summary>
		/// 行末コードなどを整える
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_CRLF_Click(object sender, EventArgs e) {
			FormDivode frm = new FormDivode();
			frm.Show();
		}

		/// <summary>
		/// １６進表示
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_16_Click(object sender, EventArgs e) {
			System.Windows.Forms.OpenFileDialog openFileDialog1 = new OpenFileDialog() {
				Multiselect = false,
				CheckFileExists = true,
				//InitialDirectory = @"\\nt0049np\shared\06-03_改善\01_スタッフ（企画部・改）\03_工程改善（製造部・改）\03 切削工程"
			};
			if (openFileDialog1.ShowDialog() == DialogResult.Cancel) {
				this.Close();
				Application.Exit();
				return;
			}

			char[] chr256 = new char[256];
			int ccnt;

			using (StreamReader sr = new StreamReader(openFileDialog1.FileName)) {
				FormHex frmHex = new FormHex();
				while (true) {
					ccnt = sr.Read(chr256, 0, 256);
					frmHex.SetText(ccnt, chr256);
					DialogResult result = frmHex.ShowDialog();
					if (result == DialogResult.Cancel)
						break;
					if (sr.EndOfStream)
						break;
				}
			}
		}

		/// <summary>
		/// テスト
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Test_Click(object sender, EventArgs e) {
			char[] chr256 = new char[256];
			FormHex frmHex = new FormHex();

			for (int ii = 0; ii < 256; ii++)
				chr256[ii] = (char)ii;
			frmHex.SetText(256, chr256);

			DialogResult result = frmHex.ShowDialog();
		}

	}
}