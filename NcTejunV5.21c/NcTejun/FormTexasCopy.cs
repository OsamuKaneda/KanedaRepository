using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Data.SqlClient;

namespace NcTejun
{
	partial class FormTexasCopy : Form
	{
		private CasaData casa;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public FormTexasCopy() {

			InitializeComponent();

			// 製造番号選択リストの作成
			comboBox_seba.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox_seba.DataBindings.Clear();
			comboBox_seba.DataSource = CasaData.Casa.Tables["DBJNO"];
			comboBox_seba.DisplayMember = "KATA_DISP";
			comboBox_seba.ValueMember = "J_NO";
			comboBox_seba.MaxDropDownItems = 20;
			comboBox_seba.DropDownWidth = 320;
			comboBox_seba.SelectedIndex = -1;
			// 加工機選択リストの作成
			comboBox_mach.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox_mach.DataBindings.Clear();

			using (DataTable Mach = new DataTable("Mach")) {
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_Mach ORDER BY [group]", connection)) {
					connection.Open();
					adapter.Fill(Mach);
				}
				foreach (DataRow dRow in Mach.Rows) {
					if (dRow["未使用"] != DBNull.Value)
						if ((bool)dRow["未使用"] == true) continue;
					if ((string)dRow["DNC名"] != "TEXAS") continue;
					comboBox_mach.Items.Add((string)dRow["設備名"]);
				}
			}
			comboBox_seba.SelectedIndex = -1;

			// ＯＫの初期設定
			buttonOK.Enabled = false;

		}

		// //////////////////
		// 以降、イベント処理
		// //////////////////

		/// <summary>
		/// ワーク名の入力
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_buhin_SelectedIndexChanged(object sender, EventArgs e) {
			string vlue;
			if (comboBox_buhin.ValueMember == "")
				return;
			if (comboBox_buhin.SelectedIndex < 0)
				return;

			if (comboBox_buhin.SelectedValue == null)
				vlue = "null";
			else
				vlue = comboBox_buhin.SelectedValue.ToString();
			//MessageBox.Show(comboBox1.SelectedIndex.ToString()
			//	+ " Value=" + vlue
			//	+ " in comboBox1");

			if (comboBox_buhin.SelectedIndex < 0) {
				comboBox_kotei.DataBindings.Clear();
				return;
			}

			casa.SetDBRTG((string)comboBox_buhin.SelectedValue);
			comboBox_kotei.Enabled = true;
			comboBox_kotei.DataBindings.Clear();
			comboBox_kotei.DataSource = casa.DBRTG;
			comboBox_kotei.DisplayMember = "KOUTEI_COMM";
			comboBox_kotei.ValueMember = "KOUTEI_NO";
			comboBox_kotei.SelectedIndex = -1;
			return;
		}

		// //////////////////
		// イベント処理終了
		// //////////////////


		/// <summary>
		/// 以前の設定情報によるデータの設定
		/// </summary>
		/// <returns></returns>
		private void IndexSet() {
			if (Output.FormNcSet.casaData == null)
				return;

			System.Data.DataRowView aa;
			if (casa.KATA_CODE_ARI) {
				for (int ii = 0; ii < comboBox_buhin.Items.Count; ii++) {
					aa = (System.Data.DataRowView)comboBox_buhin.Items[ii];
					if (Output.FormNcSet.casaData.process_no == (string)aa["BUHIN_CODE"]) {
						comboBox_buhin.SelectedIndex = ii;
						break;
					}
				}
				for (int ii = 0; ii < comboBox_kotei.Items.Count; ii++) {
					aa = (System.Data.DataRowView)comboBox_kotei.Items[ii];
					if (Output.FormNcSet.casaData.step_no == aa["KOUTEI_NO"].ToString()) {
						comboBox_kotei.SelectedIndex = ii;
						break;
					}
				}
			}
			return;
		}


		private void OKenableSet() {
			buttonOK.Enabled = false;

			// 製造番号がカサブランカに存在する場合
			if (casa.KATA_CODE_ARI)
				if (comboBox_kotei.SelectedIndex < 0)
					return;
			// ＯＫボタンと出力名変更の有効化
			buttonOK.Enabled = true;
		}







		/// <summary>
		/// ＮＣデータの並び順のチェック
		/// </summary>
		private void Dcheck(Output.NcOutput.NcOutList ncout) {
			Output.NcOutput.NcToolL now, pre;
			int last;
			string mess;

			last = ncout.Tl.Count - 1;
			mess = null;
			for (int ii = 0; ii < ncout.Tl.Count; ii++) {
				now = ncout.Tl[ii];
				pre = ii > 0 ? ncout.Tl[ii - 1] : null;

				//回転数、送り速度のチェック
				if (Mp700(now) == false) {
					if (now.Skog.Tld.XmlT.SPIND == 0 || now.Skog.Tld.XmlT.FEEDR == 0)
						throw new Exception($"T{now.Skog.matchK[0].K2.Tnum},{now.Skog.Tld.XmlT.SNAME}の回転数or送り速度が０です。");
				}

				//タッチセンサーのＮＣリファレンス点
				if (Mp700(now) == true) {
					if (now.Skog.Tld.XmlT.TRTIP)
						throw new Exception("タッチセンサーのＮＣリファレンス点が中心ではありません");
				}

				//ポストと刃具のチェック
				switch (now.Ncnam.Ncdata.ncInfo.xmlD.PostProcessor.Id) {
				case CamUtil.PostProcessor.ID.MES_BEF_BU:
					if (Mp700(now) == false)
						throw new Exception("前計測の工具セット名がMP700でない");
					if (ii == 0) { ;}
					else
						throw new Exception("加工中にも前計測が実施されている");
					break;
				case CamUtil.PostProcessor.ID.MES_AFT_BU:
					if (Mp700(now) == false)
						throw new Exception("後計測の工具セット名がMP700でない");
					if (ii == 0)
						throw new Exception("最初のＮＣデータが前計測でない");
					if (ii != last) {
						if (mess == null) mess = "加工中に形状計測している";
					}
					if (pre.Outnam.IndexOf("airblow") < 0)
						MessageBox.Show("形状計測の前にエアーブローがない");
					break;
				default:
					if (Mp700(now) == true)
						throw new Exception("加工ＮＣデータの工具セット名でMP700が存在する");
					if (ii == 0)
						throw new Exception("最初のＮＣデータが前計測でない");
					if (ii == last)
						if (mess == null) mess = "加工後の形状計測がない";
					break;
				}
			}
			if (mess != null) MessageBox.Show(mess);
		}
		private bool Mp700(Output.NcOutput.NcToolL nctl) {
			if (nctl.Skog.TsetCHG.Tset_name == "MP700") return true;
			if (nctl.Skog.TsetCHG.Tset_name == "MP700_B") return true;
			return false;
		}

		private void ComboBox_seba_SelectedIndexChanged(object sender, EventArgs e) {
				AAA();
		}

		private void ComboBox_mach_SelectedIndexChanged(object sender, EventArgs e) {
				AAA();
		}

		private void AAA() {
			if (comboBox_seba.Text == "" || comboBox_mach.Text == "") return;

			// 金型情報の取得
			NcdTool.Mcn1 mach = new NcdTool.Mcn1(comboBox_mach.Text);
			casa = new CasaData(comboBox_seba.Text, mach.CasMachineName);

			casa.SetDBORD();
			comboBox_buhin.SelectedIndex = -1;
			comboBox_buhin.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox_buhin.DataSource = casa.DBORD;
			comboBox_buhin.DisplayMember = "BUHIN_NAME";
			comboBox_buhin.ValueMember = "BUHIN_CODE";
			comboBox_buhin.SelectedIndex = -1;
			comboBox_buhin.Enabled = casa.KATA_CODE_ARI;

			// /////////////////////////////
			// その他のコントロールの初期設定
			// /////////////////////////////
			// 製造番号
			textBox_seizo.Text = casa.SEIBAN;
			textBox_seizo.Enabled = false;
			// 金型名
			textBox_katana.Text = casa.KATA_NAME;
			textBox_katana.Enabled = false;
			// 工程
			comboBox_kotei.SelectedIndex = -1;
			comboBox_kotei.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox_kotei.Enabled = casa.KATA_CODE_ARI;
		}
	}
}