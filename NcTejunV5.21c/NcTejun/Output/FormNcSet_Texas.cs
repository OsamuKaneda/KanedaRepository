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
	partial class FormNcSet_Texas : FormNcSet
	{
		// /// <summary>ＮＣデータ、手順書、工具表の出力の情報を作成する主要クラス</summary>
		// private Output.NcOutput ncOutput;

		private bool outOK = false;
		private bool _5Faces;

		// /// <summary>出力する工具のリスト</summary>
		// private FormToolPrint.ListTolP tolP;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public FormNcSet_Texas()
			: base(0) {

			InitializeComponent();

			_5Faces = NcdTool.Tejun.Mach.Milling_5Faces;

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
			// コメント
			textBoxComment.Text = NcdTool.Tejun.TejunName;


			// ///////////////////////
			// ＤＭＧ専用機能の設定
			// ///////////////////////
			//combo_dnc.Items.AddRange(new object[] { "ＤＮＣ運転", "メモリ運転" });
			//comboBox_kataKijun.Items.AddRange(new object[] { "固定型タイプ", "可動型タイプ" });
			//comboBox_Dandori.Items.AddRange(new object[] { "立置き（横型ＭＣ）", "平置き（表が上）", "平置き（裏が上）" });
			//comboBox_Hoko.Items.AddRange(new object[] { "表面", "裏面" });
			// 運転方法（ＤＮＣ可否）
			combo_dnc.SelectedIndex = 0;
			combo_dnc.Enabled = false;
			if (_5Faces) {
				// ＸＹ基準
				comboBox_kataKijun.SelectedIndex = 0;
				comboBox_kataKijun.Enabled = false;
				comboBox_kataKijun.Visible = false;
				label2.Visible = false;
				// 段取り方法
				comboBox_Dandori.Items.Clear();
				comboBox_Dandori.Items.Add("表が上、左基準");	// 固定型タイプ
				comboBox_Dandori.Items.Add("表が上、右基準");	// 可動型タイプ
				comboBox_Dandori.Items.Add("裏が上、右基準");	// 固定型タイプ
				comboBox_Dandori.Items.Add("裏が上、左基準");	// 可動型タイプ
				comboBox_Dandori.SelectedIndex = -1;
				comboBox_Dandori.Text = "(未設定)";
				comboBox_Dandori.Enabled = true;
				// 加工方向
				comboBox_Hoko.SelectedIndex = -1;
				comboBox_Hoko.Text = "(未設定)";
				comboBox_Hoko.Enabled = false;
			}
			else {
				// ＸＹ基準
				comboBox_kataKijun.SelectedIndex = 0;
				comboBox_kataKijun.Enabled = false;
				comboBox_kataKijun.Visible = false;
				label2.Visible = false;
				// 段取り方法
				comboBox_Dandori.SelectedIndex = 0;
				comboBox_Dandori.Enabled = false;
				comboBox_Dandori.Visible = false;
				label9.Visible = false;
				// 加工方向
				comboBox_Hoko.SelectedIndex = 0;
				comboBox_Hoko.Enabled = false;
				comboBox_Hoko.Visible = false;
				label7.Visible = false;
				// 型サイズ
				groupBox3.Enabled = false;
				groupBox3.Visible = false;
			}

			// treeビューの設定
			this.treeView1.Location = new System.Drawing.Point(0, 0);
			//this.treeView1.Size = new System.Drawing.Size(202, 286);
			this.treeView1.Size = new System.Drawing.Size(this.Size.Width - 473, this.Size.Height - 38);
			this.Controls.Add(treeView1);
			TreeviewSet(treeView1, ncOutput.ncoutList);

			// 出力名
			if (FormNcSet.OUT != null)
				textBoxOutName.Text = FormNcSet.OUT;
			else
				textBoxOutName.Text = NcdTool.Tejun.TejunName;
			textBoxOutName.Enabled = false;

			// 以前の設定による上書き（これ以降はＤＮＣの設定で出力名を変更）
			IndexSet();

			// ＮＣデータの出力可否チェック
			//OutputChk();

			OKenableSet();
	
			// 部品コンボボックスを選択しておく
			comboBox_buhin.Select();

			//現在、機能していない。後日、使用可否検討
			//IndexRead();

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
			if (comboBox_buhin.ValueMember == "")
				return;
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
			// ＸＹ基準の設定
			if (_5Faces) {
				comboBox_Dandori.SelectedIndex = -1;
				comboBox_Dandori.Text = "(未設定)";
				comboBox_Dandori.Enabled = true;
			}
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
		/// 部品名で区分できない場合のＸＹ基準（固定型タイプ、可動型タイプ）の選択
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_kata_SelectedIndexChanged(object sender, EventArgs e) {
			OKenableSet();
		}
		/// <summary>
		/// 段取り方法の変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_Dandori_SelectedIndexChanged(object sender, EventArgs e) {
			groupBox3.Visible = false;
			Set_comboHoko();
			OKenableSet();
		}
		/// <summary>
		/// 加工方向の選択
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_Hoko_SelectedIndexChanged(object sender, EventArgs e) {
			OKenableSet();
		}
		private void TextBox_X_TextChanged(object sender, EventArgs e) {
			OKenableSet();
		}
		private void TextBox_Y_TextChanged(object sender, EventArgs e) {
			//OKenableSet();
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

			// チェック加工方向
			// { "表面", "裏面", "正面", "背面", "右側面", "左側面" }

			if (_5Faces && ncOutput.keisha == false && ncOutput.tjnHoko.Value == false) {
				switch (comboBox_Dandori.SelectedIndex) {
				case 0:
				case 1:
					if (comboBox_Hoko.SelectedIndex == 1) {
						MessageBox.Show("加工方向に「裏面」は選択できません。");
						return;
					}
					break;
				case 2:
				case 3:
					if (comboBox_Hoko.SelectedIndex == 0) {
						MessageBox.Show("加工方向に「表面」は選択できません。");
						return;
					}
					break;
				default:
					throw new Exception("wedfaqg");
				}
			}

			// ＸＹ基準のチェック
			if (_5Faces) {
				if (
					(comboBox_buhin.Text.IndexOf("固定型") >= 0 && comboBox_Dandori.SelectedIndex != 0 && comboBox_Dandori.SelectedIndex != 2) ||
					(comboBox_buhin.Text.IndexOf("可動型") >= 0 && comboBox_Dandori.SelectedIndex != 1 && comboBox_Dandori.SelectedIndex != 3)
				) {
					DialogResult res = MessageBox.Show(
						"金型の「段取り方向」が「ワーク名」と整合していません。このままＮＣデータを作成しますか。", "固定型/可動型の確認",
						MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
					if (res != DialogResult.OK) return;
				}
			}

			// 出力可否の設定
			int noCount = CheckTreeView();
			// インデックス（メイン）データの作成
			Index.SetMain_OM(this);

			// 出力フォルダー名
			ncOutput.lfldr = textBoxOutName.Text;

			// ＮＣデータファイルの上書き等確認
			//（Yes:新規,上書き No:上書きしない Cancel:）
			DialogResult result = CheckNcFile(NcdTool.Tejun.Seba, ncOutput.lfldr);
			if (result == DialogResult.Cancel)
				return;

			// ＮＣ、工具表の出力の設定
			if (NcdTool.Tejun.Mach.DncName == CamUtil.Machine.DNCName.TEXAS)
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
			if (_5Faces) {
				switch (FormNcSet.casaData.progress) {
				case "1":	//"平置き 表が上"
					if (FormNcSet.casaData.kotkad)
						comboBox_Dandori.SelectedIndex = 0;
					else
						comboBox_Dandori.SelectedIndex = 1;
					break;
				case "2":	//"平置き 裏が上"
					if (FormNcSet.casaData.kotkad)
						comboBox_Dandori.SelectedIndex = 2;
					else
						comboBox_Dandori.SelectedIndex = 3;
					break;
				default:
					comboBox_Dandori.SelectedIndex = -1;	// 不明
					comboBox_Dandori.Text = "(未設定)";
					break;
				}
				// 加工方向
				if (comboBox_Dandori.SelectedIndex < 0) {
					comboBox_Hoko.SelectedIndex = -1;
					comboBox_Hoko.Text = "(未設定)";
				}
				else {
					Set_comboHoko();
				}
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
			if (dmg) {
				// ＸＹ基準
				// 段取り方法
				if (casaTemp.progress == "0")
					comboBox_Dandori.SelectedIndex = 0;	//"立置き（横型ＭＣ）"
				else if (casaTemp.progress == "1")
					comboBox_Dandori.SelectedIndex = 1;	//"平置き（表が上）"
				else if (casaTemp.progress == "2")
					comboBox_Dandori.SelectedIndex = 2;	//"平置き（裏が上）"
				else throw new Exception("afraebrfbwrh");
				// 加工方向
				set_comboHoko();
			}
			return true;
		}
		*/



		/// <summary>
		/// 加工方向のコンボボックスを設定する
		/// </summary>
		private void Set_comboHoko() {

			comboBox_Hoko.Items.Clear();

			// すべてTEBISでポストG0の傾斜が存在する場合
			if (ncOutput.keisha) {
				comboBox_Hoko.Items.AddRange(new object[] { "ＮＣ設定方向(上面基準)" });
				comboBox_Hoko.SelectedIndex = 0;
				comboBox_Hoko.Text = comboBox_Hoko.Items[0].ToString();
				comboBox_Hoko.Enabled = true;
				return;
			}

			// 手順で加工方向が設定されている場合
			if (ncOutput.tjnHoko.Value) {
				comboBox_Hoko.Items.AddRange(new object[] { "手順で設定された方向" });
				comboBox_Hoko.SelectedIndex = 0;
				comboBox_Hoko.Text = comboBox_Hoko.Items[0].ToString();
				comboBox_Hoko.Enabled = true;
				return;
			}

			comboBox_Hoko.Items.AddRange(new object[] { "表面", "裏面", "正面", "背面", "右側面", "左側面" });
			comboBox_Hoko.SelectedIndex = -1;
			comboBox_Hoko.Text = "(未設定)";
			comboBox_Hoko.Enabled = true;
		}

		private void OKenableSet() {
			Set_outOK(false);
			buttonOK.Enabled = false;

			if (comboBox_Dandori.SelectedIndex < 0)
				return;
			if (comboBox_Hoko.SelectedIndex < 0)
				return;
			// 製造番号がカサブランカに存在する場合
			if (Casa.KATA_CODE_ARI)
				if (comboBox_kotei.SelectedIndex < 0)
					return;
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
				if (_5Faces) comboBox_Dandori.Enabled = false;
				if (_5Faces) comboBox_Hoko.Enabled = false;
				textBoxComment.Enabled = false;
				textBox_X.Enabled = false;
				textBox_Y.Enabled = false;
				//checkBox_Rapid.Enabled = false;
				buttonCancel.Text = "終了";
			}
			buttonNc.Visible = set;
			//buttonTejun.Visible = set;
			buttonTool.Visible = set;
			buttonOK.Visible = !set;
		}

		/// <summary>
		/// ＮＣデータの出力可否の確認
		/// </summary>
		/// <returns></returns>
		private bool OutputChk() {
			return true;
		}
	}
}