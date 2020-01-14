using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using CamUtil;

namespace NCSEND2.KYDATA
{
	partial class FormCsvData : Form
	{
		private static string ASCII = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		public FormCsvData(string dnam, CamSystem camSystem, string originZ) {
			InitializeComponent();

			List<KyData> KyDList = KyData.CreateKakoYoryo(dnam, camSystem, originZ == "" ? (double?)null : Convert.ToDouble(originZ));
			if (KyDList.Count == 0) return;
			if (KyDList[0].CSV == false) return;

			///////////////////////
			// リストビューの設定
			///////////////////////
			listView1.View = View.Details;
			listView1.Enabled = true;
			listView1.HideSelection = true;         // コントロールがフォーカスを失ったときに、選択されている項目が強調表示されない
			listView1.LabelEdit = false;             // コントロールの項目のラベルをユーザーが編集できる
			listView1.CausesValidation = false;     // そのコントロールが原因で、フォーカスを受け取っても検証されない
			listView1.FullRowSelect = true;         // 項目をクリックすると項目とそのすべてのサブ項目を選択する
			listView1.ShowItemToolTips = true;      // リストアイテムで設定されたツールヒントを表示する
			listView1.CheckBoxes = false;            // チェックボックスを表示する
			listView1.AllowColumnReorder = true;    // コラムの順序変更を許可する
			listView1.MultiSelect = false;

			int columnNo1 = 0, columnNo2 = 0;
			foreach (KyData kyd in KyDList) {
				if (columnNo1 < kyd.CountColumn1()) columnNo1 = kyd.CountColumn1();
				if (columnNo2 < kyd.CountColumn2()) columnNo2 = kyd.CountColumn2();
			}
			ListView.ColumnHeaderCollection clmHeader = new ListView.ColumnHeaderCollection(listView1);
			clmHeader.Add("ファイル名", 50, HorizontalAlignment.Left);
			for (int ii = 0; ii < columnNo2; ii++) clmHeader.Add(String.Concat(ASCII[ii / 26], ASCII[ii % 26 + 1]) + ii.ToString(" 0"), 50, HorizontalAlignment.Left);
			for (int ii = 0; ii < columnNo1; ii++) clmHeader.Add(String.Concat(ASCII[ii / 26], ASCII[ii % 26 + 1]) + ii.ToString(" 0"), 50, HorizontalAlignment.Left);
			foreach (KyData kyd in KyDList) listView1.Items.AddRange(kyd.LVItems(columnNo1, columnNo2));
		}
	}
}