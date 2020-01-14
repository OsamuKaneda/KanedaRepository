using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using CamUtil.CamNcD;

namespace NCSEND2
{
	/// <summary>
	/// 
	/// </summary>
	partial class FormLView : Form
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ncDataSet"></param>
		internal FormLView(List<NCINFO.NcInfoCam> ncDataSet) {
			ListViewItem lim;

			InitializeComponent();

			listView1.View = View.Details;
			listView1.Enabled = true;
			listView1.HideSelection = true;         // コントロールがフォーカスを失ったときに、選択されている項目が強調表示されない
			//listView1.LabelEdit = lvwManual;
			//listView1.LabelEdit = true;             // コントロールの項目のラベルをユーザーが編集できる
			listView1.CausesValidation = false;     // そのコントロールが原因で、フォーカスを受け取っても検証されない
			listView1.FullRowSelect = true;         // 項目をクリックすると項目とそのすべてのサブ項目を選択する
			listView1.ShowItemToolTips = true;      // リストアイテムで設定されたツールヒントを表示する
			//listView1.CheckBoxes = true;            // チェックボックスを表示する
			listView1.AllowColumnReorder = true;    // コラムの順序変更を許可する
			listView1.MultiSelect = false;
			listView1.GridLines = true;				// グリッド線を表示する

			// リストビューの設定－リストビューの列の表題
			LView.ColumnHeaderSet2(listView1);
			foreach (NCINFO.NcInfoCam ncd in ncDataSet) {
				// 工具単位
				for (int ii = 0; ii < ncd.ToolCount; ii++) {
					lim = new LView.ListViewItemTool(ii, ncd);
					listView1.Items.Add(lim);
				}
			}
		}
	}
}