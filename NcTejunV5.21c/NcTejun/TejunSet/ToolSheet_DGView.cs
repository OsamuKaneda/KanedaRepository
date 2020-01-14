using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Drawing.Printing;
using CamUtil;

namespace NcTejun.TejunSet
{
	/// <summary>
	/// 工具表を表示・修正するフォーム
	/// </summary>
	partial class ToolSheet : Form
	{
		/// <summary>
		/// データテーブルのコラムのセットとデータグリッドビューの設定
		/// </summary>
		private void ColumnsSet() {

			// /////////////////////////////
			// データグリッドビューの設定
			// /////////////////////////////
			dataGridView1.ScrollBars = ScrollBars.Both;
			dataGridView1.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
			dataGridView1.Sorted += new EventHandler(DataGridView1_Sorted);
			dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dataGridView1.CellFormatting += DataGridView1_CellFormatting;

			// データビューコラムの幅の設定
			//dataGridView1.AllowUserToResizeColumns = true;
			for (int ii = 0; ii < dataGridView1.ColumnCount; ii++)
				dataGridView1.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			dataGridView1.Columns["対応ＮＣ"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			// データ文字の表示位置
			foreach (DataGridViewColumn tempColumn in dataGridView1.Columns) {
				if (tempColumn.ValueType == typeof(string) || tempColumn.ValueType == typeof(bool)) { ;}
				else
					tempColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
			}

			dataGridView1.Columns["変更"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["シートNo"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["工具No"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["加工長"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["使用回数"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["対応ＮＣ"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["ツールセット"].ToolTipText = "左クリック：ツールセットでソート";
			dataGridView1.Columns["シートNo"].ToolTipText = "左クリック：シートNo.工具No.でソート";
			dataGridView1.Columns["工具No"].ToolTipText = "左クリック：シートNo.工具No.でソート";
			dataGridView1.Columns["工具名PC"].ToolTipText = "左クリック：工具名でソート";
			//dataGridView1.Columns["工具名UX"].ToolTipText = "左クリック：工具名でソート";
			dataGridView1.Columns["ＮＣ限定"].ToolTipText = "左クリック：ＮＣ限定名でソート";
			dataGridView1.Columns["突出し量PC"].ToolTipText = "左クリック：突出し量でソート";
			//dataGridView1.Columns["突出し量UX"].ToolTipText = "左クリック：突出し量でソート";
			dataGridView1.Columns["ホルダ１"].ToolTipText = "左クリック：ホルダ１名でソート";
			dataGridView1.Columns["消耗率"].ToolTipText = "左クリック：消耗率でソート";
			dataGridView1.Columns["加工順"].ToolTipText = "左クリック：加工順でソート";
			dataGridView1.Columns["標準セット"].ToolTipText = "左クリック：標準セットでソート";

			dataGridView1.Columns["tol"].Visible = false;
			dataGridView1.Columns["標準セット"].Visible = false;

			dataGridView1.Columns["ID"].ReadOnly = true;
			dataGridView1.Columns["ツールセット"].ReadOnly = true;
			dataGridView1.Columns["変更"].ReadOnly = true;
			dataGridView1.Columns["消耗率"].ReadOnly = true;
			dataGridView1.Columns["加工長"].ReadOnly = true;
			dataGridView1.Columns["使用回数"].ReadOnly = true;
			dataGridView1.Columns["加工順"].ReadOnly = true;
			dataGridView1.Columns["対応ＮＣ"].ReadOnly = true;
			dataGridView1.Columns["ID"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["ツールセット"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["変更"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["変更"].DefaultCellStyle.ForeColor = Color.Red;
			dataGridView1.Columns["消耗率"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["加工長"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["使用回数"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["加工順"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["対応ＮＣ"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["消耗率"].DefaultCellStyle.Format = "0.000 %";
			dataGridView1.Columns["加工長"].DefaultCellStyle.Format = "0.00";
			dataGridView1.Columns["工具名PC"].ReadOnly = true;
			dataGridView1.Columns["ホルダ１"].ReadOnly = true;
			dataGridView1.Columns["突出し量PC"].ReadOnly = true;
			dataGridView1.Columns["高速Ｓ"].ReadOnly = true;
			dataGridView1.Columns["工具名PC"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["ホルダ１"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["突出し量PC"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["高速Ｓ"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			if (NcdTool.Tejun.Mach.Toool_nc) {
				dataGridView1.Columns["分割"].ReadOnly = true;
				dataGridView1.Columns["分割"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			}

			return;
		}




		// ////////////////////////////
		// ////////////////////////////
		// イベントハンドラ
		// ////////////////////////////
		// ////////////////////////////

		// ////////////////////////////
		// フォーム
		// ////////////////////////////
		private void FormToolSheet_FormClosing(object sender, FormClosingEventArgs e) {
			if (!DgvCommit) {
				MessageBox.Show(
					"工具表が編集中です。上下左右の矢印キーを用いて編集を確定するか、" +
					"Escキーで編集をキャンセルさせてから実行してください。");
				e.Cancel = true;
				return;
			}
			if (!Tolst_commit) {
				MessageBox.Show("確定処理がされていません。確定するか、取消しを実行してください。");
				e.Cancel = true;
				return;
			}
			if (!Tolst_save) {
				DialogResult result = MessageBox.Show(
					"このツールシート（" + TolstName + "）は保存せず終了します。よろしいですか？",
					"ToolSheet",
					MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				if (result == DialogResult.Cancel) {
					e.Cancel = true;
					return;
				}
			}

			Tolst_commit = false;
			((Form1)this.MdiParent).TsheetClose(this);
			Program.frm1.TsheetClear(this);
		}
		// 最初の表示
		private void ToolSheet_Shown(object sender, EventArgs e) {
			if (TolstName == "") {
				// ///////////////////////
				// 工具表無しの場合の設定
				// ///////////////////////
				印刷PToolStripButton.Enabled = true;
				行追加toolStripButton.Enabled = false;
				切り取りUToolStripButton.Enabled = false;
				this.Size = new Size(800, 100);
				this.Location = new Point(100, 100);

				// 「再作成」以外の「ツールシート編集メニュー」
				{
					//toolStripSplitButton1.Enabled = false;
					不足工具追加ToolStripMenuItem.Enabled = false;
					//ホルダー突出し量ToolStripMenuItem.Enabled = false;
					//作成ToolStripMenuItem.Enabled = false;
					//追加ToolStripMenuItem.Enabled = false;
					工具番号整理ToolStripMenuItem.Enabled = false;
					//選択工具以降ToolStripMenuItem.Enabled = false;
					//選択工具以降シート分割ToolStripMenuItem.Enabled = false;
				}
			}
			else {
				// ＸＭＬ情報がないＮＣデータがある場合は「ツールシート編集メニュー」は使用できない
				// 使用できるようにするとどうなる
				string fList = "";
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsTS(this.TolstName)) {
					if (ncnam.Itdat == 0) continue;
				}
				if (fList != "") {
					MessageBox.Show(
						fList +
						"はツールセット情報がないため突出し量やホルダー名を自動で\n" +
						"決める場合は、従来通り「ホルダー優先度」などの情報を用います。",
						"TOOL",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				}
			}
		}

		// //////////////////////////////////////
		// 変更を確定しないメニュー/ボタン/イベント
		// //////////////////////////////////////

		/*
		void tolstData_RowChanged(object sender, DataRowChangeEventArgs e) {
			MessageBox.Show("The method or operation is not implemented.");

			if (e.Row["変更"] == DBNull.Value)
				e.Row["変更"] = 'E';
			//if (dvRow.Row["変更"] == DBNull.Value)
			//	dvRow.Row["変更"] = 'E';
			//SetEnable_Decision(true);
		}
		*/

		private void 行追加toolStripButton_Click(object sender, EventArgs e) {
			DataRow workRow;
			List<DataRow> selectRows = new List<DataRow>();
			if (dataGridView1.SelectedRows.Count == 0) {
				workRow = TolstData.NewRow();
				TolstData.RowsAdd(workRow);
			}
			else {
				foreach (DataGridViewRow dRow in dataGridView1.SelectedRows) {
					selectRows.Add(((DataRowView)dRow.DataBoundItem).Row);
					workRow = TolstData.NewRow();
					for (int jj = 0; jj < TolstData.Columns.Count; jj++)
						workRow[jj] = ((DataRowView)dRow.DataBoundItem).Row[jj];
					TolstData.RowsAdd(workRow);
				}
			}
			DataGridView1_UserAddedRow(dataGridView1, new DataGridViewRowEventArgs(dataGridView1.Rows[0]));

			//MessageBox.Show(dataGridView1.Rows[0].DataBoundItem.GetType().ToString());
			dataGridView1.Select();
			foreach (DataGridViewRow dgRow in dataGridView1.Rows) {
				foreach (DataRow dRow in selectRows) {
					if (((DataRowView)dgRow.DataBoundItem).Row == dRow) {
						dgRow.Selected = true;
						break;
					}
				}
			}
			Tolst_commit = false;
		}
		private void 切り取りUToolStripButton_Click(object sender, EventArgs e) {
			//SendKeys.Send("{DEL}");
			// UserDeletedRow がコールされる
			int cnt = dataGridView1.SelectedRows.Count;
			foreach (DataGridViewRow vRow in dataGridView1.SelectedRows)
				((DataRowView)vRow.DataBoundItem).Row.Delete();
			//if (cnt != 0)
			//	m_rireki.Rireki_Save(tolstData.Copy());	// 変更履歴の追加
			if (cnt != 0)
				Tolst_commit = false;
		}
		/// <summary>
		/// ＮＣデータ／工具単位に工具番号を設定する
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStripButton_TMoSet_Click(object sender, EventArgs e) {
			if (DgvCommit == false) {
				MessageBox.Show(
					"工具表が編集中です。上下左右の矢印キーを用いて編集を確定するか、" +
					"Escキーで編集をキャンセルさせてから実行してください。");
				return;
			}
			List<NcdTool.Tool.TSData.TS_Edit_Data> tseList = new List<NcdTool.Tool.TSData.TS_Edit_Data>();
			TNoSet tNoSet = new TNoSet(TolstName, tseList);
			tNoSet.ShowDialog();
			if (tseList.Count > 0) {

				while (dataGridView1.Rows.Count > 0)
					((DataRowView)dataGridView1.Rows[0].DataBoundItem).Row.Delete();

				foreach (NcdTool.Tool.TSData.TS_Edit_Data tse in tseList)
					TolstData.ItemSetTNoSet(tse);

				Tolst_commit = false;
			}
		}





		// ////////////////////////////
		// 変更を確定するメニュー/ボタン
		// ////////////////////////////

		/// <summary>工具表確定ボタンクリック</summary>
		private void ToolStripButton_Decision_Click(object sender, EventArgs e) {

			//foreach (DataRow dRow in this.tolstData.Rows) {
			//	if (dRow.RowState == DataRowState.Deleted)
			//		continue;
			//	MessageBox.Show("S" + dRow["シートNo"].ToString() + " T" + dRow["工具No"].ToString());
			//}

			try { Matching_TSheet(false, 0, 0, this, 0); }
			catch (Exception ex) {
				MessageBox.Show(ex.Message + "工具表は確定できません。");
				return;
			}

			Tolst_save = false;
		}
		/// <summary>取消し</summary>
		private void ToolStripButton_Cancel_Click(object sender, EventArgs e) {
			TolstData.RejectChanges();
			Tolst_commit = true;
		}

		private void 上書き保存SToolStripButton_Click(object sender, EventArgs e) {

			if (TolstName == "") {
				MessageBox.Show("工具表に名前がない（手順書に 'T 工具表名' の行がない）ため保存できません。");
				return;
			}
			if (!Tolst_commit) {
				MessageBox.Show("確定処理がされていません。確定するか、取消しを実行してください。");
				return;
			}

			bool tsave = TolstData.SaveTolst();
			if (tsave) {
				Tolst_save = true;
				m_henkoNumb++;
				MessageBox.Show("工具表を保存しました");
			}
		}
		/// <summary>再読込み</summary>
		private void 開くOtoolStripButton_Click(object sender, EventArgs e) {

			DialogResult result = DialogResult.Cancel;
			result = MessageBox.Show(
				"ツールシートの変更を保存せずに、再度ＰＣの工具表を読み込みますか",
				"ツールシートの再読込み",
				MessageBoxButtons.OKCancel);
			if (result != DialogResult.OK) return;

			TolstSet();
			try { Matching_TSheet(false, 0, 0, this, 0); }
			catch (Exception ex) {
				MessageBox.Show(ex.Message + "工具表はオープンできません。");
				return;
			}
			Tolst_save = TolstData.ChgShomo ? false : true;
			m_henkoFile = false;
		}

		/// <summary>全ての保存されている消耗率を削除する</summary>
		private void ToolStripButton_Shomo_Click(object sender, EventArgs e) {
			TolstData.ConsumptClear();
			toolStripButton_Shomo.Enabled = false;
			Matching_TSheet(false, 0, 0, this, 0);

			Tolst_save = false;
		}

		private void 元に戻すtoolStripButton_Click(object sender, EventArgs e) {
			//tolstData = ((DataTable)m_rireki.Rireki_Back()).Copy();
			//dataGridView1.DataSource = tolstData;
		}

		private void やり直しtoolStripButton_Click(object sender, EventArgs e) {
			//tolstData = ((DataTable)m_rireki.Rireki_Fwrd()).Copy();
			//dataGridView1.DataSource = tolstData;
		}

		private void ヘルプLToolStripButton_Click(object sender, EventArgs e) {

		}

		private void 再作成ToolStripMenuItem_Click(object sender, EventArgs e) {

			if (TolstData.RowsCount > 0) {
				DialogResult result = MessageBox.Show(
					"ツールシートをクリアし、再作成しますか",
					"ツールシートの再作成",
					MessageBoxButtons.OKCancel);
				if (result != DialogResult.OK)
					return;
			}

			// データテーブル情報のクリア
			TolstData.Clear();
			this.m_stinc = 0;

			// データテーブル情報の読み込みとマッチング
			try { Matching_TSheet(true, 0, 0, this, 0); }
			catch (Exception ex) {
				MessageBox.Show(ex.Message + "工具表の再作成はできません。");
				Tolst_commit = false;
				return;
			}
			Application.DoEvents();


			// ////////////////////////////////////////////////////////////
			// 工具無しの場合使用できなかった、工具編集メニューの使用を許可する
			// ////////////////////////////////////////////////////////////
			行追加toolStripButton.Enabled = true;
			切り取りUToolStripButton.Enabled = true;
			// 「再作成」以外の「ツールシート編集メニュー」
			{
				//toolStripSplitButton1.Enabled = true;
				不足工具追加ToolStripMenuItem.Enabled = true;
				//ホルダー突出し量ToolStripMenuItem.Enabled = true;
				工具番号整理ToolStripMenuItem.Enabled = true;
			}

			Tolst_save = false;
		}
		private void 不足工具追加ToolStripMenuItem_Click(object sender, EventArgs e) {
			//MessageBox.Show(
			//	"  tolstData=" + tolstData.Rows.Count.ToString() +
			//	"  tolsLink=" + tolsLink.Count +
			//	"  UNIXツールシートのUPDATE回数=" + henkoNumb.ToString() +
			//	"  ツールシートの変更の有無=" + henkoFile.ToString() +
			//	"  TolsクラスとＤＢの整合以降の変更の有無=" + henkoTols.ToString()
			//	);

			if (DgvCommit == false) {
				MessageBox.Show(
					"工具表が編集中です。上下左右の矢印キーを用いて編集を確定するか、" +
					"Escキーで編集をキャンセルさせてから実行してください。");
				return;
			}

			// 変更を確定する
			//if (toolStripButton_Decision.Enabled == true)
			//	Decision();

			// データテーブルが変更されている場合はaddを実行
			try { Matching_TSheet(true, 0, 0, this, 0); }
			catch (Exception ex) {
				MessageBox.Show(ex.Message + "不足工具は追加できません。");
				return;
			}

			Tolst_save = false;
		}

		// 工具番号整理
		private void 選択工具以降ToolStripMenuItem_Click(object sender, EventArgs e) {

			if (DgvCommit == false) {
				MessageBox.Show(
					"工具表が編集中です。上下左右の矢印キーを用いて編集を確定するか、" +
					"Escキーで編集をキャンセルさせてから実行してください。");
				return;
			}
			if (dataGridView1.SelectedRows.Count == 0) {
				MessageBox.Show("工具表の行が選択されていません。行全体を選択してから実行してください。");
				return;
			}

			// 選択している工具のシート番号、工具番号、加工順を特定する
			SelectSTK(out int snum, out int tnum, out int kjun);
			if (kjun <= 0) {
				MessageBox.Show("指定された工具に加工順序のデータがありません。");
				return;
			}

			// 実行
			//MessageBox.Show("今から実行");
			try { Matching_TSheet(true, snum, tnum - 1, this, kjun); }
			catch (Exception ex) {
				MessageBox.Show(ex.Message + "工具番号整理はできません。");
				return;
			}

			Tolst_save = false;
		}

		private void 選択工具以降シート分割ToolStripMenuItem_Click(object sender, EventArgs e) {

			if (DgvCommit == false) {
				MessageBox.Show(
					"工具表が編集中です。上下左右の矢印キーを用いて編集を確定するか、" +
					"Escキーで編集をキャンセルさせてから実行してください。");
				return;
			}
			if (dataGridView1.SelectedRows.Count == 0) {
				MessageBox.Show("工具表の行が選択されていません。行全体を選択してから実行してください。");
				return;
			}

			// 選択している工具のシート番号、工具番号、加工順を特定する
			SelectSTK(out int snum, out int tnum, out int kjun);
			if (kjun <= 0) {
				MessageBox.Show("指定された工具に加工順序のデータがありません。");
				return;
			}

			// 指示された加工順以降の工具を消去
			TolstData.Delete(kjun);

			// 変更を確定する
			//if (toolStripButton_Decision.Enabled == true)
			//	Decision();

			//m_rireki.Rireki_Save(tolstData.Copy());	// 変更履歴の追加
			//MessageBox.Show("今から実行");
			try {
				// シート分割の設定
				this.m_stinc = snum;
				// 実行
				Matching_TSheet(true, 0, 0, this, 0);
				this.m_stinc = 0;
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message + "シート分割はできません。");
				return;
			}

			Tolst_save = false;
		}

		/// <summary>
		/// 選択している工具の最大シート番号、最大工具番号、その時の加工順を特定する
		/// </summary>
		/// <param name="snum"></param>
		/// <param name="tnum"></param>
		/// <param name="kjun"></param>
		private void SelectSTK(out int snum, out int tnum, out int kjun) {
			snum = -1;
			tnum = -1;
			kjun = -1;

			DataRow tRow;
			foreach (DataGridViewRow vRow in dataGridView1.SelectedRows) {
				tRow = ((DataRowView)vRow.DataBoundItem).Row;
				if (snum < (int)tRow["シートNo"]) {
					snum = (int)tRow["シートNo"];
					tnum = (int)tRow["工具No"];
					kjun = TolstData.KakoJun(tRow);
				}
				else if (snum == (int)tRow["シートNo"] && tnum < (int)tRow["工具No"]) {
					tnum = (int)tRow["工具No"];
					kjun = TolstData.KakoJun(tRow);
				}
			}
		}

		// ////////////////////////////////
		// dataGridView1のイベントハンドラ
		// ////////////////////////////////

		// スタイルの設定
		void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
			DataGridView dgv = (DataGridView)sender;
			if (dgv.Columns[e.ColumnIndex].Name == "突出し量PC") {
				if ((bool)dgv.Rows[e.RowIndex].Cells["標準セット"].Value)
					dgv.Rows[e.RowIndex].Cells["突出し量PC"].Style.ForeColor = Color.Black;
				else
					dgv.Rows[e.RowIndex].Cells["突出し量PC"].Style.ForeColor = Color.Red;
			}
		}

		private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e) {
			MessageBox.Show("入力されたデータが入力規則に適合していません。");
		}

		private void DataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e) {
			//tolstData.DefaultSort();
			Tolst_commit = false;
		}
		private void DataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e) {
			Tolst_commit = false;
		}
		private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
			this.dataGridView1.CellValueChanged -= this.DataGridView1_CellValueChanged;
			if (dataGridView1.Rows[e.RowIndex].Cells["変更"].Value == DBNull.Value)
				dataGridView1.Rows[e.RowIndex].Cells["変更"].Value = 'E';
			this.Validate();			// 行を確定
			Tolst_commit = false;		// 「確定」、「取消」ボタンを有効に
			this.dataGridView1.CellValueChanged +=
				new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellValueChanged);
		}

		// 以下ソート関連
		void DataGridView1_Sorted(object sender, EventArgs e) {
			dataGridView1.ShowCellToolTips = true;
			statusStrip1.Items[0].Text = "ソート：" + TolstData.DefaultView.Sort;
		}
		private void DataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
			if (e.Button == MouseButtons.Left)
				switch (e.ColumnIndex) {
				case 4:	// シート
				case 5:	// 工具番号
					TolstData.DefaultView.Sort = "シートNo,工具No";
					break;
				}

		}
	}
}
