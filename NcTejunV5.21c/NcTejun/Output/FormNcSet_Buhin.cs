using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Data.SqlClient;

namespace NcTejun.Output
{
	partial class FormNcSet_Buhin : FormNcSet
	{
		// /// <summary>ＮＣデータ、手順書、工具表の出力の情報を作成する主要クラス</summary>
		// private Output.NcOutput ncOutput;

		private bool outOK = false;

		/// <summary>部品素材のサイズＸＹＺ</summary>
		double[] size;


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public FormNcSet_Buhin(NcdTool.Mcn1 mach)
			: base(0) {

			InitializeComponent();

			// ＯＫの初期設定
			buttonOK.Enabled = false;

			if (Casa.KATA_CODE_ARI) {
				Casa.SetDBORD();
				comboBox_buhin.SelectedIndex = -1;
				comboBox_buhin.DropDownStyle = ComboBoxStyle.DropDownList;
				comboBox_buhin.DataSource = Casa.DBORD;
				comboBox_buhin.DisplayMember = "BUHIN_NAME";
				comboBox_buhin.ValueMember = "BUHIN_CODE";
				comboBox_buhin.SelectedIndex = -1;
				comboBox_buhin.Enabled = Casa.KATA_CODE_ARI;
			}
			else
				comboBox_buhin.Enabled = false;

			// /////////////////////////////
			// その他のコントロールの初期設定
			// /////////////////////////////
			// 内製管理No
			textBox_naisei.Text = NcdTool.Tejun.Seba;
			textBox_naisei.Enabled = false;
			// 製造番号
			textBox_seizo.Text = Casa.SEIBAN;
			textBox_seizo.Enabled = false;
			// 金型名
			textBox_katana.Text = Casa.KATA_NAME;
			textBox_katana.Enabled = false;
			// 工程
			comboBox_kotei.SelectedIndex = -1;
			comboBox_kotei.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox_kotei.Enabled = Casa.KATA_CODE_ARI;
			// コメント（プロダクト名を使う）
			foreach (NcOutput.NcToolL tmp in ncOutput.ncoutList.Tl) {
				if (tmp.Ncnam.nnam.IndexOf("airblow") >= 0) continue;
				if (tmp.Ncnam.Ncdata.ncInfo.xmlD.ProductsName.Length > 10) {
					textBoxComment.Text = NcdTool.Tejun.TejunName + "_" + tmp.Ncnam.Ncdata.ncInfo.xmlD.ProductsName.Substring(10);
					break;
				}
			}


			// 運転方法（ＤＮＣ可否）
			combo_dnc.SelectedIndex = 0;
			combo_dnc.Enabled = false;

			// 段取り方法
			comboBox_Dandori.SelectedIndex = -1;
			comboBox_Dandori.Enabled = true;
			comboBox_Dandori.Visible = true;
			label9.Visible = true;
			DataTable Dandori = SetDanDori(mach.name);
			comboBox_Dandori.DataBindings.Clear();
			comboBox_Dandori.DataSource = Dandori;
			comboBox_Dandori.DisplayMember = "Name";
			comboBox_Dandori.ValueMember = "Height";
			comboBox_Dandori.SelectedIndex = -1;
			comboBox_Dandori.SelectedIndexChanged += new System.EventHandler(this.ComboBox_Dandori_SelectedIndexChanged);

			// 常駐ではない最大の工具番号を調べ、工具分離仕様に用いる
			int tmax = 0;
			foreach (NcOutput.NcToolL tmp in ncOutput.ncoutList.Tl) {
				if (tmp.Smch.K2.Tlgn.Perm_tool) continue;
				if (tmax < tmp.Smch.K2.Tnum)
					tmax = tmp.Smch.K2.Tnum;
			}
			switch (mach.ID) {
			case CamUtil.Machine.MachID.D500:
				// パレット番号
				comboBox_pallet.SelectedIndex = -1;
				break;
			case CamUtil.Machine.MachID.LineaM:
				// パレット番号
				comboBox_pallet.SelectedIndex = 0;
				comboBox_pallet.Text = "0";
				comboBox_pallet.Visible = false;
				label2.Visible = false;
				break;
			default:
				throw new Exception("qefbqrfbqhr");
			}

			// 型サイズ
			groupBox3.Enabled = true;
			groupBox3.Visible = true;

			Dcheck(ncOutput.ncoutList);
			if (ncOutput.ncoutList.Tl[0].Ncnam.Ncdata.ncInfo.xmlD.PostProcessor.Id != CamUtil.PostProcessor.ID.MES_BEF_BU)
				throw new Exception("最初のＮＣデータが前計測のポストでない");

			if (ncOutput.ncoutList.Tl[0].Ncnam.Ncdata.ncInfo.ncinfoSchemaVer.Older("V14")) {
				using (StreamReader sr = new StreamReader(ncOutput.ncoutList.Tl[0].Ncnam.Ncdata.fulnamePC)) {
					try { size = Buhin_Size(sr); }
					catch { throw; }
				}
			}
			else {
				size = ncOutput.ncoutList.Tl[0].Ncnam.Ncdata.ncInfo.xmlD.MaterialSize;
			}
			textBox_X.Text = size[0].ToString("0.000");
			textBox_Y.Text = size[1].ToString("0.000");
			textBox_H.Text = size[2].ToString("0.000");

			// treeビューの設定
			this.treeView1.Location = new System.Drawing.Point(0, 0);
			this.treeView1.Size = new System.Drawing.Size(this.Size.Width - 473, this.Size.Height - 38);
			this.Controls.Add(treeView1);
			TreeviewSet(treeView1, ncOutput.ncoutList);

			// 出力名
			if (FormNcSet.OUT != null)
				textBoxOutName.Text = FormNcSet.OUT;
			else
				textBoxOutName.Text =
					NcdTool.Tejun.TejunName + "_" +
					ncOutput.ncoutList.Tl[0].Ncnam.Ncdata.ncInfo.xmlD.ProcessName.Substring(3);
			textBoxOutName.Enabled = false;

			// 以前の設定による上書き（これ以降はＤＮＣの設定で出力名を変更）
			IndexSet();

			OKenableSet();
	
			// 部品コンボボックスを選択しておく
			comboBox_buhin.Select();

			//現在、機能していない。後日、使用可否検討
			//IndexRead();

		}

		// 段取りタイプをＤＢから取得
		private DataTable SetDanDori(string machine) {
			DataTable Dandori = new DataTable();

			using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
			using (SqlCommand com = new SqlCommand() { Connection = connection }) {
				connection.Open();
				com.CommandText = "SELECT Name, Height FROM dbo.NcConv_BaseHeight WHERE (Machine = @machine) ORDER BY Sort";
				com.Parameters.Add("@machine", SqlDbType.VarChar, 10).Value = machine;
				using (SqlDataAdapter adapter = new SqlDataAdapter(com)) { adapter.Fill(Dandori); }
			}
			return Dandori;
		}

		/// <summary>
		/// 部品のサイズを測定データから取得
		/// </summary>
		/// <param name="obj1"></param>
		/// <returns></returns>
		private double[] Buhin_Size(StreamReader obj1) {
			double[] size = new double[3];
			CamUtil.LCode.NcLineCode txtd = new CamUtil.LCode.NcLineCode((double[])null, NcdTool.Tejun.BaseNcForm, CamUtil.LCode.NcLineCode.GeneralDigit, false, true);
			CamUtil.Vector3 xyz, ijk;

			int ii = -1;
			int jj = 0;
			while (obj1.EndOfStream != true) {
				txtd.NextLine(obj1.ReadLine());

				//ＮＣデータ基準のチェック
				if (txtd.NcLine.IndexOf("G65P8730") == 0) {
					xyz = txtd.XYZ;
					if (xyz.X != 0 || xyz.Y != 0 || xyz.Z != 0)
						MessageBox.Show("ＮＣデータ基準が (0,0,0) でない");
				}

				//基準出しの取り込み＆チェック
				if (txtd.NcLine.IndexOf("G65P8746") == 0 || txtd.NcLine.IndexOf("G65P9346") == 0) {
					xyz = txtd.XYZ;
					ijk = txtd.IJK;
					if (ii == -1) ii = 0;
					switch (txtd.Code('T').L) {
					case 1:
						if (ijk.X == 0 && ijk.Z == 0) {
							size[1] = 2.0 * Math.Abs(xyz.Y);
							jj = 1;
						}
						else if (ijk.Y == 0 && ijk.Z == 0) {
							size[0] = 2.0 * Math.Abs(xyz.X);
							jj = 2;
						}
						else
							throw new Exception("前計測データ（１点目）の計測方向が異常");
						ii = ii + 1;
						break;
					case 2:
						if (ijk.X == 0 && ijk.Z == 0 && jj == 1) { ;}
						else if (ijk.Y == 0 && ijk.Z == 0 && jj == 2) { ;}
						else
							throw new Exception("前計測データ（２点目）の計測方向が異常");
						ii = ii + 1;
						break;
					case 3:
						if (ijk.Y == 0 && ijk.Z == 0 && jj == 1)
							size[0] = 2.0 * Math.Abs(xyz.X);
						else if (ijk.X == 0 && ijk.Z == 0 && jj == 2)
							size[1] = 2.0 * Math.Abs(xyz.Y);
						else
							throw new Exception("前計測データ（３点目）の計測方向が異常");
						ii = ii + 1;
						break;
					default:
						throw new Exception("前計測データ（Ｎ点目）の計測が異常");
					}
				}
				if (txtd.NcLine.IndexOf("G65P9345") == 0) {
					xyz = txtd.XYZ;
					ijk = txtd.IJK;
					if (txtd.Code('T').L == 0) {
						size[2] = Math.Abs(xyz.Z);
						if (ijk.X != 0 || ijk.Y != 0)
							throw new Exception("前計測データ（０点目）の計測方向が異常");
					}
					else
						throw new Exception("前計測データ（Ｎ点目）の計測が異常");
				}
			}
			if (ii != -1 && ii != 3)
				throw new Exception("前計測データ測定点の数が異常");

			return size;
		}

		/// <summary>
		/// 最初の表示
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormNcSelect_Shown(object sender, EventArgs e) {
			Set_outOK(false);
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
			//comboBox2.SelectedItem = null;
			Set_outOK(false);
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

			Casa.SetDBRTG((string)comboBox_buhin.SelectedValue);
			comboBox_kotei.Enabled = true;
			comboBox_kotei.DataBindings.Clear();
			comboBox_kotei.DataSource = Casa.DBRTG;
			comboBox_kotei.DisplayMember = "KOUTEI_COMM";
			comboBox_kotei.ValueMember = "KOUTEI_NO";
			comboBox_kotei.SelectedIndex = -1;
			return;
		}
		/// <summary>
		/// 工程名の入力
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_kotei_SelectedIndexChanged(object sender, EventArgs e) {
			OKenableSet();
		}
		/// <summary>
		/// 段取りタイプの変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_Dandori_SelectedIndexChanged(object sender, EventArgs e) {
			textBox_Z.Text = (Convert.ToDouble(textBox_H.Text) - (double)comboBox_Dandori.SelectedValue).ToString("0.000");
			OKenableSet();
		}
		/// <summary>
		/// パレット番号の変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_pallet_SelectedIndexChanged(object sender, EventArgs e) {
			OKenableSet();
		}

		/// <summary>
		/// 工具分離仕様の変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_bunri_SelectedIndexChanged(object sender, EventArgs e) {
			OKenableSet();
		}

		/// <summary>
		/// 部品サイズの変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextBox_X_TextChanged(object sender, EventArgs e) {
			OKenableSet();
		}
		/// <summary>
		/// 部品サイズの変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextBox_Y_TextChanged(object sender, EventArgs e) {
			OKenableSet();
		}

		/// <summary>
		/// 「確定」ボタンのクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonOK_Click(object sender, EventArgs e) {

			// コメントの文字数の制限（稼働実績のSAGYO_SECTIONの文字数制限による）
			if (textBoxComment.Text.Length > 16) {
				MessageBox.Show("１６文字を超えるコメント（セットアップ名）は処理できません。");
				return;
			}
			// コメントの文字数の制限（セットアップ名が空であるとテキサスの受信でエラーとなるため）2016/03/29
			if (textBoxComment.Text.Length == 0) {
				MessageBox.Show("文字のないコメント（セットアップ名）は処理できません。");
				return;
			}

			// 出力可否の設定
			int noCount = CheckTreeView();
			// インデックス（メイン）データの作成
			Index.SetMain_BU(this);

			// 出力フォルダー名
			ncOutput.lfldr = textBoxOutName.Text;

			// ＮＣデータファイルの上書き等確認
			//（Yes:新規,上書き No:上書きしない Cancel:）
			DialogResult result = CheckNcFile(NcdTool.Tejun.Seba, ncOutput.lfldr);
			if (result == DialogResult.Cancel)
				return;

			// ＮＣ、工具表の出力の設定
			ncOutput.NcOutputSet();

			// 入力情報の保存
			FormNcSet.OUT = ncOutput.lfldr;
			FormNcSet.casaData = Index.IndexMain;

			// ボタンの再設定
			Set_outOK(true);
			if (result == DialogResult.No)
				buttonNc.Enabled = false;
		}

		// /////////////////////////
		// 「確定」以後のイベント処理
		// /////////////////////////

		private void ButtonNc_Click(object sender, EventArgs e) {
			NcSet_NcOutput();
		}
		private void ButtonTool_Click(object sender, EventArgs e) {
			// ワーニングメッセージのクリア
			CamUtil.LogOut.warn = new StringBuilder();

			// データベースへの保存の可否
			bool saveOK = true;
			if (NcdTool.Tejun.NcList.TsNameList[0] != "" && CamUtil.ProgVersion.Debug) {
				saveOK = false;
				if (NcdTool.Tejun.NcList.TsNameList[0][0] == 'Z') {
					DialogResult result = MessageBox.Show("Ｚで始まる工具表：" + NcdTool.Tejun.NcList.TsNameList[0] + " をＤＢへ出力する場合 Yes", "工具表ＤＢ出力", MessageBoxButtons.YesNo);
					if (result == DialogResult.Yes) saveOK = true;
				}
				else
					MessageBox.Show("デバッグモードの場合は'Z'で始まる名称以外の手順書/工具表はＤＢにデータに出力しません。");
			}
			// 工具表のＤＢ出力と表示
			Tolout(saveOK, false);
			// 手順書のＤＢ出力と表示
			Tjnout(saveOK);
		}

		// //////////////////
		// イベント処理終了
		// //////////////////


		/// <summary>
		/// 以前の設定情報によるデータの設定
		/// </summary>
		/// <returns></returns>
		private void IndexSet() {
			if (FormNcSet.casaData == null)
				return;

			System.Data.DataRowView aa;
			if (Casa.KATA_CODE_ARI) {
				for (int ii = 0; ii < comboBox_buhin.Items.Count; ii++) {
					aa = (System.Data.DataRowView)comboBox_buhin.Items[ii];
					if (FormNcSet.casaData.process_no == (string)aa["BUHIN_CODE"]) {
						comboBox_buhin.SelectedIndex = ii;
						break;
					}
				}
				for (int ii = 0; ii < comboBox_kotei.Items.Count; ii++) {
					aa = (System.Data.DataRowView)comboBox_kotei.Items[ii];
					if (FormNcSet.casaData.step_no == aa["KOUTEI_NO"].ToString()) {
						comboBox_kotei.SelectedIndex = ii;
						break;
					}
				}
			}
			textBoxComment.Text = FormNcSet.casaData.setupName;
			// 段取りタイプ
			if (FormNcSet.casaData.progress.Length > 0)
				comboBox_Dandori.Text = FormNcSet.casaData.progress;
			else {
				comboBox_Dandori.SelectedIndex = -1;	// 不明
				comboBox_Dandori.Text = "(未設定)";
			}
			return;
		}

		/*
		/// <summary>
		/// 前回に出力したindex情報からデータを設定する。現在、機能していない。
		/// </summary>
		/// <returns></returns>
		private bool IndexRead() {
			if (Program.tejun.casaData != null)
				return false;
			if (Program.tejun.casaData == null)
				return false;

			string folder;
			folder = NcOutput.Dir_PTR + Program.tejun.seba;
			if (!System.IO.Directory.Exists(folder))
				return false;

			folder += @"\" + Program.tejun.tejunName;
			if (!System.IO.Directory.Exists(folder))
				return false;

			folder += @"\index";
			if (!System.IO.File.Exists(folder))
				return false;

			StreamReader sr = new StreamReader(folder);
			Index_Main casaTemp = new Index_Main(sr);
			sr.Close();

			System.Data.DataRowView aa;
			if (Casa.DBORD.Rows.Count != 0) {
				for (int ii = 0; ii < comboBox_buhin.Items.Count; ii++) {
					aa = (System.Data.DataRowView)comboBox_buhin.Items[ii];
					if (casaTemp.process_no == (string)aa["BUHIN_CODE"]) {
						comboBox_buhin.SelectedIndex = ii;
						break;
					}
				}
				for (int ii = 0; ii < comboBox_kotei.Items.Count; ii++) {
					aa = (System.Data.DataRowView)comboBox_kotei.Items[ii];
					if (casaTemp.step_no == aa["KOUTEI_NO"].ToString()) {
						comboBox_kotei.SelectedIndex = ii;
						break;
					}
				}
			}
			textBoxComment.Text = casaTemp.setupName;
			// ＸＹ基準
			// 段取り方法
			if (Convert.ToInt32(casaTemp.progress) >= 0)
				comboBox_Dandori.SelectedIndex = Convert.ToInt32(casaTemp.progress);
			else throw new Exception("afraebrfbwrh");
			return true;
		}
		*/



		private void OKenableSet() {
			Set_outOK(false);
			buttonOK.Enabled = false;

			if (comboBox_Dandori.SelectedIndex < 0)
				return;
			if (comboBox_pallet.SelectedIndex < 0)
				return;
			if (comboBox_bKubun.SelectedIndex < 0)
				return;

			// 製造番号がカサブランカに存在する場合
			if (Casa.KATA_CODE_ARI)
				if (comboBox_kotei.SelectedIndex < 0)
					return;
			// 金型サイズ入力の確認
			if (comboBox_Dandori.SelectedIndex == 0) {
				try {
					if (Convert.ToDouble(textBox_X.Text) < 0.01)
						return;
					double dtmp = Convert.ToDouble(textBox_Y.Text);
				}
				catch { return; }
			}
			// ＯＫボタンと出力名変更の有効化
			buttonOK.Enabled = true;
			textBoxOutName.Enabled = true;
		}
		/// <summary>
		/// 出力の３ボタンの制御
		/// </summary>
		/// <param name="set">true:表示する  false:非表示にする（情報入力継続）</param>
		protected override void Set_outOK(bool set) {
			outOK = set;

			if (set) {
				treeView1.Enabled = false;
				combo_dnc.Enabled = false;
				textBoxOutName.Enabled = false;
				comboBox_buhin.Enabled = false;
				comboBox_kotei.Enabled = false;
				comboBox_pallet.Enabled = false;
				comboBox_bKubun.Enabled = false;
				comboBox_Dandori.Enabled = false;
				textBoxComment.Enabled = false;
				textBox_X.Enabled = false;
				textBox_Y.Enabled = false;
				//checkBox_Rapid.Enabled = false;
				buttonCancel.Text = "終了";
			}
			buttonNc.Visible = set;
			buttonTool.Visible = set;
			buttonOK.Visible = !set;
		}








		/// <summary>
		/// ＮＣデータの並び順のチェック
		/// </summary>
		private void Dcheck(NcOutput.NcOutList ncout) {
			NcOutput.NcToolL now, pre;
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
				if (Mp700(now))
					if (now.Skog.Tld.XmlT.TRTIP)
						throw new Exception("タッチセンサーのＮＣリファレンス点が中心ではありません");

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
		private bool Mp700(NcOutput.NcToolL nctl) {
			if (nctl.Skog.TsetCHG.Tset_name == "MP700") return true;
			if (nctl.Skog.TsetCHG.Tset_name == "MP700_B") return true;
			return false;
		}
	}
}