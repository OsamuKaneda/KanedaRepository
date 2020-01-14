using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NcdTool.Tool
{
	/// <summary>
	/// 使用する消耗データの選択
	/// </summary>
	partial class TSFormLife : Form
	{
		List<string> rmTejun;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rmTejun">消耗率を消去する手順名</param>
		/// <param name="tejunNames">表示する手順名</param>
		/// <param name="ktejunName">工具表作成手順名</param>
		public TSFormLife(List<string> rmTejun, string tejunNames, string ktejunName) {
			TreeNode s_node;
			InitializeComponent();
			this.rmTejun = rmTejun;

			this.Text += " 作成：" + ktejunName;

			// treeビューの設定
			this.treeView1.BeginUpdate();
			this.treeView1.CheckBoxes = true;
			this.treeView1.ShowLines = false;
			this.treeView1.Nodes.Clear();
			foreach (string stmp in tejunNames.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
				s_node = new TreeNode(stmp) { Checked = true };
				this.treeView1.Nodes.Add(s_node);
			}
			this.treeView1.ExpandAll();
			this.treeView1.EndUpdate();
		}

		private void Button_OK_Click(object sender, EventArgs e) {
			foreach (TreeNode tnode in this.treeView1.Nodes)
				if (tnode.Checked == false) rmTejun.Add(tnode.Text);
			this.Close();
		}
	}
}
