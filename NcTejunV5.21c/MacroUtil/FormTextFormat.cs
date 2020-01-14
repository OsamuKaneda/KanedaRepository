using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MacroUtil
{
	/// <summary>
	/// ＮＣコードの行末コードなどの設定を実施する
	/// </summary>
	public partial class FormDivode : Form
	{
		/// <summary>
		/// 
		/// </summary>
		public FormDivode() {
			InitializeComponent();
		}

		private void buttonOK_Click(object sender, EventArgs e) {
			List<string> henkanFile = new List<string>();
			string ddat;
			char num;

			// 行末のコード
			string lineCode = (checkBox_CR.Checked) ? "\r\n" : "\n";

			System.Windows.Forms.OpenFileDialog openFileDialog1 = new OpenFileDialog() {
				Multiselect = true,
				CheckFileExists = true,
				//InitialDirectory = @"\\nt0049np\shared\06-03_改善\01_スタッフ（企画部・改）\03_工程改善（製造部・改）\03 切削工程"
			};
			openFileDialog1.ShowDialog();
			if (openFileDialog1.FileNames.Length == 0) return;

			foreach (string fName in openFileDialog1.FileNames) {
				// 変換の要否判断
				if (lineCode == Form1.LastLineCode(fName) && !checkBox_SP.Checked && !checkBoxOdd.Checked) {
					continue;
				}

				henkanFile.Add(Path.GetFileName(fName));
				Application.DoEvents();

				// バックアップ用ファイルの作成
				num = '0';
				while (File.Exists(fName + "bak" + num.ToString()))
					num = (num == '9') ? 'A' : num++;
				File.Move(fName, fName + "bak" + num.ToString());

				// 変換の実行
				using (StreamReader ss = new StreamReader(fName + "bak" + num.ToString()))
				using (StreamWriter sw = new StreamWriter(fName)) {
					while (!ss.EndOfStream) {
						ddat = ss.ReadLine();
						if (checkBoxOdd.Checked) {
							if (checkBox_SP.Checked) {
								ddat = ddat.Trim();
							}
							else {
								ddat = ddat.TrimEnd();
							}
							if (ddat.Length % 2 == 0) {
								ddat = ddat + " ";
							}
						}
						else {
							if (checkBox_SP.Checked) {
								ddat = ddat.Trim();
							}
						}
						sw.Write(ddat + lineCode);
					}
				}
			}
			MessageBox.Show(String.Format("{0} 個の変換を実施しました", henkanFile.Count));
		}
	}
}
