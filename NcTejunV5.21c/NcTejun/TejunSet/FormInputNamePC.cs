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
	partial class FormInputNamePC : Form
	{
		StringBuilder tejun;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="m_tejunName"></param>
		/// <param name="tejunNames"></param>
		public FormInputNamePC(StringBuilder m_tejunName, List<string> tejunNames) {
			InitializeComponent();
			foreach (string list in tejunNames)
				comboBox1.Items.Add((object)list);
			buttonSearch.Enabled = false;
			buttonOk.Enabled = false;
			tejun = m_tejunName;
		}

		private void ButtonOk_Click(object sender, EventArgs e) {
			if (!Char.IsLetter(comboBox1.Text[0])) {
				MessageBox.Show(
					"指定のファイルは許されないファイル名です。",
					"InputName",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				DialogResult = DialogResult.Cancel;
				return;
			}
			if (File.Exists(CamUtil.ServerPC.SvrName + @"\h\usr9\ASDM\Tejun\" + comboBox1.Text)) {
				DialogResult = DialogResult.OK;
				tejun.Append(comboBox1.Text);
			}
			else {
				MessageBox.Show(
					"指定のファイルはＰＣサーバに存在しません",
					"InputName",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				DialogResult = DialogResult.Cancel;
			}
		}

		private void ButtonCancel_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			//this.Close();
		}

		private void ButtonSearch_Click(object sender, EventArgs e) {
			;
		}

		// ドロップダウンリストから選択され確定したとき
		private void ComboBox1_SelectionChangeCommitted(object sender, EventArgs e) {
			buttonOk.Enabled = true;
		}

		// コンボボックス内のテキストが変更されたとき
		private void ComboBox1_TextUpdate(object sender, EventArgs e) {
			buttonSearch.Enabled = true;
		}
	}
}