using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NcTejun.TejunSet
{
	/// <summary>
	/// 「工具番号手動設定」実行のフォーム
	/// </summary>
	partial class TNoSet : Form
	{
		private List<NcdTool.NcName.NcNam> ncList;
		List<NcdTool.Tool.TSData.TS_Edit_Data> tseListOut;

		internal TNoSet(string tolstName, List<NcdTool.Tool.TSData.TS_Edit_Data> p_tseList) {
			InitializeComponent();

			// 出力情報
			this.tseListOut = p_tseList;

			// 対象とするツールシートの特定
			ncList = new List<NcdTool.NcName.NcNam>();
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsTS(tolstName))
				this.ncList.Add(ncnam);
			if (this.ncList.Count < 1) {
				this.Close();
				return;
			}

			ColumnsSet();
			int ierr = ViewSet();
			if (ierr > 0)
				MessageBox.Show("工具表と関連のない " + ierr.ToString() + "個のＮＣデータは表示していません");

		}
		/// <summary>
		/// データテーブルのコラムのセットとデータグリッドビューの設定
		/// </summary>
		private void ColumnsSet() {

			// /////////////////////////////
			// データグリッドビューの設定
			// /////////////////////////////
			dataGridView1.Columns.Add("加工順", "加工順");
			dataGridView1.Columns.Add("ＮＣデータ", "ＮＣデータ");
			dataGridView1.Columns.Add("ツールセット", "ツールセット");
			dataGridView1.Columns.Add("加工長", "加工長");
			dataGridView1.Columns.Add("シートNo", "シートNo");
			dataGridView1.Columns.Add("工具No", "工具No");

			dataGridView1.AllowUserToDeleteRows = false;
			dataGridView1.AllowUserToAddRows = false;

			dataGridView1.ScrollBars = ScrollBars.Both;
			dataGridView1.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
			dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

			// データビューコラムの幅の設定
			//dataGridView1.AllowUserToResizeColumns = true;
			for (int ii = 0; ii < dataGridView1.ColumnCount; ii++)
				dataGridView1.Columns[ii].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

			// フォーマットなど
			dataGridView1.Columns["加工順"].ReadOnly = true;
			dataGridView1.Columns["ＮＣデータ"].ReadOnly = true;
			dataGridView1.Columns["ツールセット"].ReadOnly = true;
			dataGridView1.Columns["加工長"].ReadOnly = true;
			dataGridView1.Columns["加工順"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["ＮＣデータ"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["ツールセット"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["加工長"].DefaultCellStyle.BackColor = Color.FromName(KnownColor.Control.ToString());
			dataGridView1.Columns["加工順"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["ＮＣデータ"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["ツールセット"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["加工長"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["シートNo"].SortMode = DataGridViewColumnSortMode.NotSortable;
			dataGridView1.Columns["工具No"].SortMode = DataGridViewColumnSortMode.NotSortable;
			// データ文字の表示位置
			dataGridView1.Columns["加工順"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
			dataGridView1.Columns["ＮＣデータ"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridView1.Columns["ツールセット"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
			dataGridView1.Columns["加工長"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
			dataGridView1.Columns["シートNo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
			dataGridView1.Columns["工具No"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

			return;
		}

		/// <summary>
		/// データグリッドビューに値をセットする
		/// </summary>
		/// <returns>表示できなかった工具の数</returns>
		private int ViewSet() {
			int ierr = 0;
			string[] sRow;
			int kjun = 0;

			foreach (NcdTool.NcName.NcNam ncnam in this.ncList)
				foreach (NcdTool.NcName.Kogu skog in ncnam.Tdat) {
					kjun++;
					if (skog.matchK == null)
						ierr++;
					else {
						sRow = new string[] {
							kjun.ToString(),
							ncnam.nnam,
							skog.matchK[0].K2.Tlgn.Toolsetname,
							((double)(skog.Tld.XmlT.NCLEN / 1000.0)).ToString("0.0"),
							skog.matchK[0].K2.Tlgn.Rnum.ToString(),
							skog.matchK[0].K2.Tlgn.Unum.ToString()};
						dataGridView1.Rows.Add(sRow);
					}
				}
			return ierr;
		}
		
		/// <summary>保存 ＆ 終了</summary>
		private void ToolStripButton1_Click(object sender, EventArgs e) {

			NcdTool.Tool.TSData.TS_Edit_Data[] tseListAll = new NcdTool.Tool.TSData.TS_Edit_Data[dataGridView1.Rows.Count];

			if (dataGridView1.IsCurrentCellDirty) {
				MessageBox.Show(
					"表が編集中です。上下左右の矢印キーなどを用いて編集を確定するか、" +
					"Escキーで編集をキャンセルさせてから実行してください。");
				return;
			}

			// グリッドビューの情報をストラクチャに保存する
			int ii = 0;
			foreach (NcdTool.NcName.NcNam ncnam in ncList)
				foreach (NcdTool.NcName.Kogu skog in ncnam.Tdat) {
					if (skog.matchK == null) continue;
					tseListAll[ii] = new NcdTool.Tool.TSData.TS_Edit_Data(skog, dataGridView1.Rows[ii]);
					ii++;
				}

			// tseListOutリストに追加していく
			NcdTool.Tool.TSData.TS_Edit_Data tse1;
			bool tnew;
			for (int jj = 0; jj < tseListAll.Length; jj++) {
				tse1 = tseListAll[jj];
				// すでに作成した工具と同じか？
				tnew = true;
				foreach (NcdTool.Tool.TSData.TS_Edit_Data tse0 in tseListOut) {
					if (tse0.snumN != tse1.snumN) continue;
					if (tse0.tnumN != tse1.tnumN) continue;
					if (tse0.Tsname != tse1.Tsname) continue;
					tnew = false;
					break;
				}
				if (tnew == false) continue;

				// 新規であれば追加する
				for (int kk = jj; kk < tseListAll.Length; kk++) {
					if (tse1.snumN != tseListAll[kk].snumN) continue;
					if (tse1.tnumN != tseListAll[kk].tnumN) continue;
					if (tse1.Tsname != tseListAll[kk].Tsname) continue;
					tse1.gent.Add(tseListAll[kk].ncname);
				}
				tseListOut.Add(tse1);
			}
			this.Close();
		}

		/// <summary>保存 ＆ 継続</summary>
		private void ToolStripButton2_Click(object sender, EventArgs e) {
			this.Close();
		}

		/// <summary>キャンセル</summary>
		private void ToolStripButton3_Click(object sender, EventArgs e) {
			this.Close();
		}
	}
}