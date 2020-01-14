using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using CamUtil;

namespace NcTejun.TejunSet
{
	/// <summary>
	/// 
	/// </summary>
	partial class FormTejunSelNc : Form
	{
		DataGridView dview;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dview"></param>
		public FormTejunSelNc(DataGridView dview) {
			InitializeComponent();
			this.dview = dview;

			listBox1.Items.Clear();

			//comboBox_CAM.Items.AddRange(CamUtil.CamSystem.CamNameList);
			foreach (CamSystem cams in CamSystem.CamSystems.FindAll(cams => cams.Ncsend == true)) {
				comboBox_CAM.Items.Add(cams.Name);
			}

			// 直前のNCSEND2で出力したＮＣデータ名リストを取り込む
			if (File.Exists("InsertNcDataName")) {
				string stmp;
				using (StreamReader sr = new StreamReader("InsertNcDataName")) {
					while (sr.EndOfStream != true) {
						stmp = sr.ReadLine();
						if (stmp.Length != 0)
							listBox1.Items.Add(stmp);
					}
				}
			}

			// ソートを廃止 2015/07/16
			this.label5.Visible = false;
			this.panel2.Visible = false;
			this.radio_abcd.Visible = false;
			this.radio_time.Visible = false;

			DialogResult = DialogResult.Cancel;
		}

		private void Button_Serch_Click(object sender, EventArgs e) {

			// 検索文字列の作成
			string sText = textBox1.Text;
			if (sText.IndexOf('*') < 0 && sText.IndexOf('?') < 0)
				sText += "*";
			sText += ".ncd";
			if (sText == "*.ncd") {
				MessageBox.Show("ＮＣデータの先頭の文字列を入力してください");
				return;
			}

			// サブフォルダーの取得
			string subf;
			if (sText.Length < 3) subf = "";
			else if (!Char.IsLetter(sText[0]) || !Char.IsLetter(sText[1]) || !Char.IsLetter(sText[2])) subf = "";
			else {
				subf = Path.GetFileName(Path.GetDirectoryName(ServerPC.FulNcName(sText))) + "\\";
			}
			if (!Directory.Exists(ServerPC.SvrFldrN + subf)) return;

			// 操作停止
			foreach (Control cc in this.Controls) cc.Enabled = false;

			//　リストボックスのリスト消去
			listBox1.Items.Clear();
			listBox1.Refresh();

			// ファイル名リストの取得
			string[] stmpList = Directory.GetFiles(ServerPC.SvrFldrN + subf, sText, SearchOption.AllDirectories);

			//　リストボックスの更新
			listBox1.BeginUpdate();
			for (int ii = 0; ii < stmpList.Length; ii++) {
				//	最終フォルダー名が２文字のみに限定
				if (subf == "") if (Path.GetFileName(Path.GetDirectoryName(stmpList[ii])).Length != 2) continue;
				if (comboBox_CAM.SelectedIndex >= 0) {
					// ///////////////////////////////////////////////////////
					// ＣＡＭシステムが限定されている場合、ＸＭＬの情報を読む
					CamUtil.CamNcD.NcInfo ncif = new CamUtil.CamNcD.NcInfo(Path.ChangeExtension(stmpList[ii], "xml"));
					if (ncif.xmlD.CamSystemID.Name != (string)comboBox_CAM.SelectedItem) continue;
					// ///////////////////////////////////////////////////////
				}
				listBox1.Items.Add(Path.GetFileNameWithoutExtension(stmpList[ii]));
			}
			listBox1.EndUpdate();

			// 操作開始
			foreach (Control cc in this.Controls) cc.Enabled = true;
			return;
		}

		/// <summary></summary>
		private readonly struct FName
		{
			public readonly string name;
			public readonly DateTime date;
			public FName(string name, DateTime date) { this.name = name; this.date = date; }
		}
		private int Compare_abcd(FName f1, FName f2) { return (String.Compare(f1.name, f2.name)); }
		private int Compare_time(FName f1, FName f2) { return (DateTime.Compare(f1.date, f2.date)); }

		private void Button_Insert_Click(object sender, EventArgs e) {
			List<FName> fNameList = new List<FName>();
			FName fnam;
			FileInfo finfo;

			foreach (string stmp in listBox1.SelectedItems) {
				finfo = new FileInfo(ServerPC.FulNcName(stmp + ".ncd"));
				if (finfo.Exists) {
					fnam = new FName(stmp, finfo.LastWriteTime);
					fNameList.Add(fnam);
				}
			}

			foreach (FName stmp in fNameList) {
				if (radio_Add.Checked)
					dview.Rows.Add(new string[] { "N", Path.GetFileName(stmp.name) });
				else
					dview.Rows.Insert(dview.SelectedRows[0].Index, new string[] { "N", Path.GetFileName(stmp.name) });
			}
			listBox1.ClearSelected();
		}
		private void Button_Insert2_Click(object sender, EventArgs e) {
			List<FName> fNameList = new List<FName>();
			FName fnam;
			FileInfo finfo;

			foreach (string stmp in listBox1.Items) {
				finfo = new FileInfo(ServerPC.FulNcName(stmp + ".ncd"));
				if (finfo.Exists) {
					fnam = new FName(stmp, finfo.LastWriteTime);
					fNameList.Add(fnam);
				}
			}

			foreach (FName stmp in fNameList) {
				if (radio_Add.Checked)
					dview.Rows.Add(new string[] { "N", Path.GetFileName(stmp.name) });
				else
					dview.Rows.Insert(dview.SelectedRows[0].Index, new string[] { "N", Path.GetFileName(stmp.name) });
			}
			this.Close();
		}
		private void Button_Cancel_Click(object sender, EventArgs e) {
			this.Close();
		}
	}
}