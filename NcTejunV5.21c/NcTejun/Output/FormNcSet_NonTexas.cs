using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NcTejun.Output
{
	partial class FormNcSet_NonTexas : FormNcSet
	{
		//private Index_Main indexMain;

		//List<NcOutput.NcOutList> ncoutList;

		public FormNcSet_NonTexas()
			: base(0) {

			InitializeComponent();

			buttonOK.Enabled = true;
			textBoxOutName.Enabled = true;

			// 加工深さの計算設定
			foreach (NcOutput.NcToolL tnam in ncOutput.ncoutList.Tl) {
				if (tnam.Ncnam.Ncdata.ncInfo.xmlD.CamDimension != 2) {
					checkBox_minZ.Enabled = true;
					break;
				}
			}

			// treeビューの設定
			this.treeView1.Location = new System.Drawing.Point(0, 0);
			this.treeView1.Size = new System.Drawing.Size(this.Size.Width - 180, this.Size.Height - 80);
			this.Controls.Add(this.treeView1);
			// 出力名
			textBoxOutName.Text = NcdTool.Tejun.TejunName;

			TreeviewSet(treeView1, ncOutput.ncoutList);

		}

		/// <summary>
		/// 「確定」ボタンのクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonOK_Click(object sender, EventArgs e) {

			// 出力可否の設定
			CheckTreeView();

			// 出力フォルダー名
			//ncOutput.lfldr = null;
			ncOutput.lfldr = textBoxOutName.Text;

			// ＮＣデータファイルの上書き等確認
			//（Yes:新規,上書き No:上書きしない Cancel:）
			DialogResult result = CheckNcFile(NcdTool.Tejun.Seba, ncOutput.lfldr);
			if (result == DialogResult.Cancel)
				return;

			// ＮＣ、工具表の出力の設定
			//ncOutput.PcConvCheck = true;
			//checkBox_unix.Checked = true;
			//ncOutput.NcOutputSet(false);

			// ボタンの設定
			treeView1.Enabled = false;
			//checkBox_unix.Enabled = false;
			checkBox_minZ.Enabled = false;
			buttonOK.Enabled = false;
			textBoxOutName.Enabled = false;

			buttonNc.Visible = true;
			if (!NcdTool.Tejun.Mach.CheckOutput)
				buttonNc.Enabled = false;

			//buttonTejun.Visible = true;
			buttonTool.Visible = true;

		}

		/// <summary>
		/// ＮＣデータの出力処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonNc_Click(object sender, EventArgs e) {
			NcSet_NcOutput();
		}

		/// <summary>
		/// 工具表の出力
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonTool_Click(object sender, EventArgs e) {
			// ワーニングメッセージのクリア
			CamUtil.LogOut.warn = new StringBuilder();

			// データベースへの保存の可否
			bool saveOK = true;
			if (CamUtil.ProgVersion.Debug) {
				if (NcdTool.Tejun.TejunName[0] != 'Z') saveOK = false;
				foreach (string ss in NcdTool.Tejun.NcList.TsNameList) if (ss != "" && ss[0] != 'Z') saveOK = false;
				if (saveOK) {
					DialogResult result = MessageBox.Show("手順ＤＢ/工具表ＤＢへ出力しますか", "debag", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result != DialogResult.Yes) saveOK = false;
				}
				else {
					MessageBox.Show("デバッグモードの場合は'Z'で始まる名称以外の手順書/工具表はＤＢにデータに出力しません。");
				}
			}
			// 工具表のＤＢ出力と表示
			Tolout(saveOK, checkBox_minZ.Checked);
			// 手順書のＤＢ出力と表示
			Tjnout(saveOK);
		}
	}
}