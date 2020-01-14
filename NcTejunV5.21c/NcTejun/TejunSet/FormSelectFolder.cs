using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using CamUtil;
using System.IO;

namespace NcTejun.TejunSet
{
	/// <summary>
	/// 加工の手順データを選択する
	/// </summary>
	partial class FormSelectFolder : Form
	{
		StringBuilder tejunName;
		string tejunfNedt;
		string tejunfNorg;

		public FormSelectFolder(StringBuilder tejunN) {
			TreeNode node0;

			InitializeComponent();

			this.tejunName = tejunN;
			tejunfNedt = "Tejun";
			tejunfNorg = "TejunBase";

			node0 = this.treeView1.Nodes.Add(Path.GetDirectoryName(Path.GetDirectoryName(ServerPC.SvrFldrSorg)));

			// NCSPEED_edit
			node0.Nodes.Add(ServerPC.EDT);
			// NCSPEED_org
			node0.Nodes.Add(ServerPC.ORG);

			button_OK.Enabled = false;
		}

		// /////////////
		// イベント処理
		// /////////////

		/// <summary>ツリービューの表示</summary>
		private void FormSelectFolder_Shown(object sender, EventArgs e) {
			this.treeView1.TopNode.Expand();
			Application.DoEvents();
		}

		/// <summary>ツリーノードの展開前に２つ下のノードに子ノードを割り当てる</summary>
		private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			foreach (TreeNode snode in e.Node.Nodes) {
				if (snode.Nodes.Count > 0) continue;
				foreach (string subf in Directory.GetDirectories(snode.FullPath))
					snode.Nodes.Add(Path.GetFileName(subf));
			}
		}

		/// <summary>最後のツリーを選択した場合のみＯＫボタンを有効にする。手順ファイルの有無もチェックする 2014/03/12</summary>
		private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			button_OK.Enabled = (treeView1.SelectedNode.Nodes.Count == 0);
			if (button_OK.Enabled) {
				if (treeView1.SelectedNode.FullPath.IndexOf(ServerPC.EDT) >= 0) {
					if (!File.Exists(treeView1.SelectedNode.FullPath + "\\" + tejunfNedt))
						button_OK.Enabled = false;
				}
				else {
					if (!File.Exists(treeView1.SelectedNode.FullPath + "\\" + tejunfNorg))
						button_OK.Enabled = false;
				}
			}
		}

		/// <summary>選択完了時、手順名を出力する</summary>
		private void Button_OK_Click(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode.FullPath.IndexOf(ServerPC.EDT) >= 0)
				tejunName.Append(treeView1.SelectedNode.FullPath + "\\" + tejunfNedt);
			else
				tejunName.Append(treeView1.SelectedNode.FullPath + "\\" + tejunfNorg);
			this.Close();
		}

		private void Button_CANCEL_Click(object sender, EventArgs e) {
			this.Close();
		}
	}
}