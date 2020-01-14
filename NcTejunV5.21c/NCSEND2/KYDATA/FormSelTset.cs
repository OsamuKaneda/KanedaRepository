using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using CamUtil;

namespace NCSEND2.KYDATA
{
	/// <summary>
	/// NcConv_TSetCAMのDataRowよりツールセットＣＡＭを選択するフォームを作成する
	/// </summary>
	partial class FormSelTset : Form
	{
		/// <summary>行表題の幅</summary>
		const int labW = 150;
		/// <summary>加工工程名の幅</summary>
		const int kakW = 110;
		/// <summary>突出し量の幅</summary>
		const int tsuW = 70;
		/// <summary>間隔</summary>
		const int space = 5;


		/// <summary>実行中のＣＡＭシステム名</summary>
		string camSystemName;

		/// <summary>ＫＹデータへのリンク（入出力に使用）</summary>
		object kyList;



		/// <summary>選択結果を保存する場所</summary>
		string[] tSetCamList;
		string[] uLengthList;

		/// <summary>現在の設定値</summary>
		List<int> motoListtset;
		List<string> motoListtsuk;

		/// <summary>ツールセットＣＡＭ選択用のコンボボックスのリスト</summary>
		List<ComboBox> comb;
		/// <summary>突出し量設定用のテキストボックスのリスト</summary>
		List<TextBox> txtb;

		List<string> flNameL = new List<string>();
		List<List<System.Data.DataRow>> tsNameL = new List<List<System.Data.DataRow>>();

		/// <summary>
		/// ツールセットＣＡＭを選択するフォームのコンストラクタ（ＣＡＤＣＥＵＳ、CADmeisterK用）add in 2019/05/07
		/// </summary>
		/// <param name="camSystemName"></param>
		/// <param name="p_kyList"></param>
		internal FormSelTset(string camSystemName, List<KyData> p_kyList) {
			InitializeComponent();
			List<DataRow> dRowL;
			this.camSystemName = camSystemName;
			this.kyList = p_kyList;
			this.Text = "SELECT TOOLSET_CAM";
			this.motoListtset = null;
			this.motoListtsuk = null;

			switch (camSystemName) {
			case CamUtil.CamSystem.CADCEUS:
				p_kyList.Sort(Sort_FileName_KyData);
				foreach (KyData_CADCEUS kyD in p_kyList) {
					flNameL.Add(System.IO.Path.GetFileName(kyD.FulName));
					tsNameL.Add(CamUtil.ToolSetData.TSetCAM_List(Convert.ToDouble(kyD.CsvToolDiam), Convert.ToDouble(kyD.CsvToolRadi), CamUtil.BaseNcForm.GENERAL));
				}
				break;
			case CamUtil.CamSystem.CADmeisterKDK:
				foreach (KyData_CADmeisterKDK csv in p_kyList) {
					flNameL.Add(System.IO.Path.GetFileName(csv.FulName));
					dRowL = CamUtil.ToolSetData.TSetCAM_List(csv.CsvToolName, csv.CsvHoldName, Convert.ToDouble(csv.CsvTsukiRyo), CamUtil.BaseNcForm.GENERAL);
					if (dRowL.Count == 0)
						throw new Exception($" 工具名:{csv.CsvToolName} ホルダー名:{csv.CsvHoldName} 突出し量:{csv.CsvTsukiRyo} に適合するツールセットＣＡＭは存在しません。");
					dRowL.Sort(Sort_TSetCAM);
					tsNameL.Add(dRowL);
				}
				break;
			}
			Label_Combo(flNameL.ToArray(), tsNameL.ToArray());
			buttonOK.Enabled = false;
		}

		/// <summary>
		/// ツールセットＣＡＭを選択するフォームのコンストラクタ（変更用）
		/// </summary>
		/// <param name="p_kyList"></param>
		/// <param name="pCamSystemName"></param>
		internal FormSelTset(List<NCINFO.NcInfoCam> p_kyList, string pCamSystemName)
		{
			InitializeComponent();
			this.kyList = p_kyList;
			this.camSystemName = pCamSystemName;
			this.motoListtset = new List<int>();
			this.motoListtsuk = new List<string>();

			switch (this.camSystemName) {
			case CamUtil.CamSystem.CADCEUS:
				this.Text = "SELECT TOOLSET_CAM";
				foreach (NCINFO.NcInfoCam ncd in p_kyList) {
					flNameL.Add(System.IO.Path.GetFileName(ncd.FullNameCam));
					tsNameL.Add(CamUtil.ToolSetData.TSetCAM_List(ncd.sqlDB[0].toolsetTemp.Diam, ncd.sqlDB[0].toolsetTemp.Crad, CamUtil.BaseNcForm.GENERAL));

					for (int jj = 0; jj < tsNameL[tsNameL.Count - 1].Count; jj++) {
						if (ncd.xmlD[0].SNAME == (string)tsNameL[tsNameL.Count - 1][jj]["tset_name_CAM"]) {
							motoListtset.Add(jj);
							break;
						}
						if (jj == tsNameL[tsNameL.Count - 1].Count - 1)
							throw new Exception("afbwehrbfh");
					}
					motoListtsuk.Add(ncd.xmlD[0].TULEN.ToString());
				}
				break;
			//case CamUtil.CamSystem.Caelum:
			case CamUtil.CamSystem.CADmeisterKDK:
				this.Text = "SELECT TOOLSET_CAM & UsableLength";
				foreach (NCINFO.NcInfoCam ncd in p_kyList) for (int ii = 0; ii < ncd.ToolCount; ii++) {
						flNameL.Add(System.IO.Path.GetFileName(ncd.FullNameCam) + "_" + ncd.sqlDB[ii].Tnam_kinf);
						tsNameL.Add(CamUtil.ToolSetData.TSetCAM_List(ncd.sqlDB[ii].Tnam_kinf, CamUtil.BaseNcForm.GENERAL));

						for (int jj = 0; jj < tsNameL[tsNameL.Count - 1].Count; jj++) {
							if (ncd.xmlD[ii].SNAME == (string)tsNameL[tsNameL.Count - 1][jj]["tset_name_CAM"]) {
								motoListtset.Add(jj);
								break;
							}
							if (jj == tsNameL[tsNameL.Count - 1].Count - 1)
								throw new Exception("afbwehrbfh");
						}
						motoListtsuk.Add(ncd.xmlD[ii].TULEN.ToString());
					}
				break;
			default:
				throw new Exception("eqfbqwefrhbweh");
			}
			Label_Combo(flNameL.ToArray(), tsNameL.ToArray());
			buttonOK.Enabled = false;
		}

		private int Sort_FileName_KyData(KyData x, KyData y) {
			return x.FulName.CompareTo(y.FulName);
		}
		private int Sort_TSetCAM(DataRow x, DataRow y) {
			if (x == null && y == null)
				return 0;
			if (x == null && y != null)
				return -1;
			if (x != null && y == null)
				return 1;

			if ((double)x["priority"] == (double)y["priority"])
				return ((string)x["tset_name_CAM"]).CompareTo((string)y["tset_name_CAM"]);
			else
				return (double)x["priority"] > (double)y["priority"] ? 1 : -1;
		}

		/// <summary>
		/// 表示と結果のリストを作成する
		/// </summary>
		/// <param name="flNameL">項目名のリスト</param>
		/// <param name="tsNameL">選択するNcConv_TsetCamのDataRowリスト</param>
		public void Label_Combo(string[] flNameL, List<DataRow>[] tsNameL) {
			if (flNameL.Length != tsNameL.Length) throw new Exception("wefbqhre");

			tSetCamList = new string[flNameL.Length];
			uLengthList = new string[flNameL.Length];
			comb = new List<ComboBox>();
			txtb = new List<TextBox>();

			// コンボボックスへの設定
			this.SuspendLayout();
			MakeColumn();
			for (int ii = 0; ii < flNameL.Length; ii++)
				MakeList(flNameL[ii], SetDispTable(tsNameL[ii]));
			this.ResumeLayout(false);
		}

		private void MakeColumn() {
			int yy = 4;
			int hh = 16;

			Label Column0 = new Label();
			Label Column1 = new Label();
			Label Column2 = new Label();
			Label Column3 = new Label();

			//Column0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			Column0.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			Column0.Location = new System.Drawing.Point(space, yy);
			Column0.Name = "Column0";
			Column0.Size = new System.Drawing.Size(labW, hh);
			Column0.TabIndex = 0;
			Column0.Text = "ＮＣデータ名_工具名";
			Column0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

			//Column1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			Column1.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			Column1.Location = new System.Drawing.Point(space + labW + space, yy);
			Column1.Name = "Column1";
			Column1.Size = new System.Drawing.Size(kakW, hh);
			Column1.TabIndex = 0;
			Column1.Text = "加工工程名";
			Column1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

			int comW = panel1.Size.Width - (labW + kakW + tsuW + 5 * space);
			Column2.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right));
			//Column2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			Column2.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			Column2.Location = new System.Drawing.Point(space + labW + space + kakW + space, yy);
			Column2.Name = "Column2";
			Column2.Size = new System.Drawing.Size(comW, hh);
			Column2.TabIndex = 0;
			Column2.Text = "ツールセットＣＡＭ";
			Column2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

			Column3.Anchor = ((AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right));
			//Column3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			Column3.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			Column3.Location = new System.Drawing.Point(space + labW + space + kakW + space + comW + space, yy);
			Column3.Name = "Column3";
			Column3.Size = new System.Drawing.Size(tsuW, hh);
			Column3.TabIndex = 0;
			Column3.Text = "突出し量";
			Column3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

			this.panel1.Controls.Add(Column0);
			this.panel1.Controls.Add(Column1);
			this.panel1.Controls.Add(Column2);
			this.panel1.Controls.Add(Column3);
		}

		private void MakeList(string fName, DataTable tsName) {

			int yy = 4 + 16 + 5 + 25 * this.comb.Count;
			int idx = this.comb.Count;

			// 
			// label
			// 
			Label lab = new Label() {
				BackColor = SystemColors.Window,
				BorderStyle = BorderStyle.FixedSingle,
				Font = new Font("MS UI Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(128))),
				Location = new System.Drawing.Point(space, yy),
				Name = "label1",
				Size = new System.Drawing.Size(labW, 20),
				TabIndex = idx,
				Text = fName,
				TextAlign = System.Drawing.ContentAlignment.MiddleLeft
			};

			// 
			// comboBox加工工程名
			// 
			int kakX = space + labW + space;
			ComboBox kak = new ComboBox() {
				Tag = idx,
				Anchor = ((AnchorStyles)(AnchorStyles.Top | AnchorStyles.Left)),
				FormattingEnabled = true,
				Font = new Font("ＭＳ ゴシック", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(128))),
				Location = new System.Drawing.Point(kakX, yy),
				Name = "comboBox0",
				Size = new System.Drawing.Size(kakW, 20),
				TabStop = false,
				MaxDropDownItems = 10
			};
			kak.Items.AddRange(tsName.AsEnumerable().Select(dRow => (string)dRow["koutei"]).Distinct().ToArray());
			kak.SelectedIndex = -1;
			if (kak.Items.Count == 1) {
				kak.SelectedIndex = 0;
				kak.Enabled = false;
			}
			kak.SelectedIndexChanged += new EventHandler(Kak_SelectedIndexChanged);

			// 
			// comboBoxツールセットＣＡＭ
			// 
			int comX = kakX + kakW + space;
			int comW = panel1.Size.Width - comX - space - tsuW - space;
			ComboBox com = new ComboBox() {
				Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right))),
				FormattingEnabled = true,
				Font = new Font("ＭＳ ゴシック", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(128))),
				Location = new System.Drawing.Point(comX, yy),
				Name = "comboBox1",
				Size = new System.Drawing.Size(comW, 20),
				TabIndex = idx + 1,
				MaxDropDownItems = 25,
				DataSource = tsName.DefaultView,
				DisplayMember = "DisplayName",
				ValueMember = "ToolSetCamName"
			};

			//後から設定
			//com.SelectedIndex = -1;
			//com.SelectedIndexChanged += new EventHandler(com_SelectedIndexChanged);

			// 
			// textbox突出し量
			// 
			int tsuX = comX + comW + space;
			TextBox tsu = new TextBox() {
				Anchor = ((AnchorStyles)(AnchorStyles.Top | AnchorStyles.Right)),
				Font = new Font("ＭＳ ゴシック", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(128))),
				Location = new System.Drawing.Point(tsuX, yy),
				Name = "textBox1",
				Size = new System.Drawing.Size(tsuW, 20),
				TabIndex = idx + 1,
				TextAlign = System.Windows.Forms.HorizontalAlignment.Right,
			};
			tsu.TextChanged += new EventHandler(Tsu_TextChanged);
			switch (camSystemName) {
			//case CamUtil.CamSystem.Caelum:
			case CamUtil.CamSystem.CADmeisterKDK:
				tsu.Enabled = true;
				break;
			case CamUtil.CamSystem.CADCEUS:
				tsu.Enabled = false;
				break;
			}


			this.panel1.Controls.Add(lab);
			this.panel1.Controls.Add(kak);
			this.panel1.Controls.Add(com);
			this.panel1.Controls.Add(tsu);

			comb.Add(com);
			txtb.Add(tsu);

		}

		/// <summary>
		/// ツールセットＣＡＭ名から表示用テーブルを作成する
		/// </summary>
		/// <param name="ts_koho"></param>
		/// <returns></returns>
		private DataTable SetDispTable(List<DataRow> ts_koho) {
			List<string> tool = new List<string>();
			DataRow dRow0;
			DataTable table = DispTableColm();
			int sLength = 0;

			foreach (DataRow dRow1 in ts_koho) {
				dRow0 = table.NewRow();
				dRow0["ToolSetCamName"] = dRow1["tset_name_CAM"];
				dRow0["CutterKinfName"] = dRow1["tool_name_kinf"];
				dRow0["koutei"] = dRow1["koutei"];
				table.Rows.Add(dRow0);
				if (sLength < ((string)dRow1["tset_name_CAM"]).Length)
					sLength = ((string)dRow1["tset_name_CAM"]).Length;
			}
			// 表示用コラムDisplayName の作成（すべてのツールセットＣＡＭの文字数から計算）
			for (int ii = 0; ii < table.Rows.Count; ii++)
				if (((string)table.Rows[ii]["ToolSetCamName"]).Length > 0)
					table.Rows[ii]["DisplayName"] =
						((string)table.Rows[ii]["ToolSetCamName"]).PadRight(sLength + 2) +
						(string)table.Rows[ii]["CutterKinfName"];
			return table;
		}

		private DataTable DispTableColm() {
			DataColumn column;

			// Create a DataTable. 
			DataTable table = new DataTable();

			// Create a DataColumn and set various properties. 
			column = new DataColumn() {
				DataType = System.Type.GetType("System.String"),
				AllowDBNull = false,
				Caption = "DisplayName",
				ColumnName = "DisplayName",
				DefaultValue = ""
			};
			table.Columns.Add(column);

			// Create a DataColumn and set various properties. 
			column = new DataColumn() {
				DataType = System.Type.GetType("System.String"),
				AllowDBNull = false,
				Caption = "ToolSetCamName",
				ColumnName = "ToolSetCamName",
				DefaultValue = ""
			};
			table.Columns.Add(column);

			// Create a DataColumn and set various properties. 
			column = new DataColumn() {
				DataType = System.Type.GetType("System.String"),
				AllowDBNull = false,
				Caption = "CutterKinfName",
				ColumnName = "CutterKinfName",
				DefaultValue = ""
			};
			table.Columns.Add(column);

			// Create a DataColumn and set various properties. 
			column = new DataColumn() {
				DataType = System.Type.GetType("System.String"),
				AllowDBNull = false,
				Caption = "koutei",
				ColumnName = "koutei",
				DefaultValue = ""
			};
			table.Columns.Add(column);

			return table;
		}

		// //////////////////
		// 以下はイベント処理
		// //////////////////

		private void FormSelTset_Shown(object sender, EventArgs e) {

			//stmp = "";
			//foreach (ComboBox cb in comb) stmp += cb.SelectedIndex.ToString();
			//MessageBox.Show(stmp);

			int ii = 0;
			foreach (ComboBox cb in comb) {
				cb.SelectedIndex = -1;
				cb.SelectedIndexChanged += new EventHandler(Com_SelectedIndexChanged);
				if (motoListtset != null) {
					// 変更時（前のデータあり）は入力不可にする
					cb.SelectedIndex = motoListtset[ii];
					txtb[ii].Select();
					cb.Enabled = false;
				}
				else if (cb.Items.Count == 1) {
					// データが１つの時は入力しておく
					cb.SelectedIndex = 0;
					txtb[ii].Select();
				}
				if (motoListtsuk != null)
					txtb[ii].Text = motoListtsuk[ii];
				ii++;
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e) {
			int tsCount;


			if (motoListtset == null) {
				// /////////
				// 初期設定
				// /////////
				for (int ii = 0; ii < comb.Count; ii++) tSetCamList[ii] = (string)comb[ii].SelectedValue;
				if (((List<KyData>)kyList).Count != tSetCamList.Length) throw new Exception("awefqwfrbqh");
				switch (camSystemName) {
				case CamUtil.CamSystem.CADCEUS:
					// List<KyData2>にツールセットＣＡＭ情報を保存
					for (int ii = 0; ii < ((List<KyData>)kyList).Count; ii++)
						((KyData_CADCEUS)((List<KyData>)kyList)[ii]).Set_TSETCAM(tSetCamList[ii]);
					break;
				/*
				case CamUtil.CamSystem.Caelum:
					// kdmakeにツールセットＣＡＭ情報を保存
					tsCount = 0;
					foreach (NCINFO.kdmake stdt in (List<kdmake>)kyList) for (int ii = 0; ii < stdt.toolcount; ii++) {
							stdt.tsetCamName[ii] = TSetCamList[tsCount];
							stdt.ULength[ii] = ULengthList[tsCount] == "" ? 0 : Convert.ToDouble(ULengthList[tsCount]);
							tsCount++;
						}
					if (tsCount != TSetCamList.Length)
						throw new Exception("awefqerhfbwerhbfh");
					break;
				*/
				case CamUtil.CamSystem.CADmeisterKDK:
					// List<KyData2>にツールセットＣＡＭ情報を保存
					for (int ii = 0; ii < ((List<KyData>)kyList).Count; ii++)
						((KyData_CADmeisterKDK)((List<KyData>)kyList)[ii]).tsetCamName = tSetCamList[ii];
					break;
				default:
					throw new Exception("frwngfrwjngtjwetn");
				}
			}
			else {
				// /////////
				// 修正
				// /////////
				for (int ii = 0; ii < txtb.Count; ii++) uLengthList[ii] = txtb[ii].Text;
				tsCount = 0;
				foreach (NCINFO.NcInfoCam ncd in (List<NCINFO.NcInfoCam>)kyList) {
					for (int tcnt = 0; tcnt < ncd.ToolCount; tcnt++) {
						// ＸＭＬのツールセットＣＡＭと突出し量の修正
						if (uLengthList[tsCount] != "")
							if (Math.Abs(ncd.xmlD[tcnt].TULEN - Convert.ToDouble(uLengthList[tsCount])) >= 0.1)
								ncd.XmlEditULength(tcnt, Convert.ToDouble(uLengthList[tsCount]), ncd.sqlDB[tcnt].toolsetTemp.Diam, false);
						tsCount++;
					}
				}
			}
		}

		private void SET_ALL_Click(object sender, EventArgs e) {
			foreach (ComboBox cb in comb)
				cb.SelectedIndex = 0;
		}



		void Kak_SelectedIndexChanged(object sender, EventArgs e) {
			if (((ComboBox)sender).SelectedIndex < 0)
				return;

			int idx = (int)((ComboBox)sender).Tag;
			string koutei = (string)((ComboBox)sender).SelectedItem;
			((DataView)comb[idx].DataSource).RowFilter = "[koutei] = '" + koutei + "'";
			comb[idx].SelectedIndex = -1;
		}

		void Com_SelectedIndexChanged(object sender, EventArgs e) {
			for (int ii = 0; ii < comb.Count; ii++)
				if (comb[ii].SelectedIndex < 0) {
					buttonOK.Enabled = false;
					return;
				}
			buttonOK.Enabled = true;
			return;
		}

		void Tsu_TextChanged(object sender, EventArgs e) {
			if (((TextBox)sender).Text == "") return;
			if (((TextBox)sender).Text.Span("0123456789.") == ((TextBox)sender).Text.Length)
				return;
			((TextBox)sender).Text = "";
		}
	}
}