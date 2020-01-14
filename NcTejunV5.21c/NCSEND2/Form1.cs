#define DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.IO;

using CamUtil;
using CamUtil.CamNcD;

namespace NCSEND2
{
	partial class Form1 : Form
	{
		/// <summary>ＣＡＭのシステム番号（ラジオボタンの０からの連番 0:TEBIS 1:Dynavista）</summary>
		internal static CamSystem SelCamSystem { get { return m_camSystem; } }
		private static CamSystem m_camSystem = new CamSystem(CamSystem.Empty);
		//public static ToolSetData.TSetCAM tsetInfo;

		// 以上　静的



		List<NCINFO.NcInfoCam> ncDataSet = null;
		KYDATA.FormCsvData frmCsvData;

		public Form1() {
			FormSplash frmSplash = new FormSplash();
			frmSplash.Show();
			Application.DoEvents();

			InitializeComponent();
			Application.DoEvents();

			// ローカルホスト名、ＩＰアドレスの取得
			frmSplash.label1.Text = "ローカルホスト情報の取得中";
			Application.DoEvents();
			LocalHost.LocalHostSet();

			if (ProgVersion.Debug)
				MessageBox.Show($"debug : デバッグ開始 {LocalHost.Name}\n{Program.mess}");
			// ホーム＆カレントディレクトリの設定
			try { LocalHost.HomeTempDirSet(); }
			catch {
				MessageBox.Show("HomeTempDirSet ERROR");
				Application.Exit();
				throw;
			}
			try { Directory.SetCurrentDirectory(LocalHost.Tempdir); }
			catch {
				MessageBox.Show("SetCurrentDirectory ERROR");
				Application.Exit();
				throw;
			}

			// ///////////////////
			// コントロールの設定
			// ///////////////////
			frmSplash.label1.Text = "各種コントロールの設定中";
			Application.DoEvents();

			// TextBox1（ＮＣフォルダー）の設定
			//text1.Text = LocalHost.basedir+@"\NCProgram\";
			//text1.ReadOnly = true;
			text1.Visible = true;

			// ////////////////////////////////////////////////////////////////////////
			// comboBox2（ＣＡＭシステム名）の設定
			// ////////////////////////////////////////////////////////////////////////
			RadioButton rtmp;
			CamSystem ctmp;
			int ii = 0;
			foreach (CamSystem cams in CamSystem.CamSystems.FindAll(cams => cams.Ncsend == true)) {
				rtmp = new RadioButton();
				ctmp = cams;
				rtmp.Name = ctmp.Name;
				rtmp.Text = ctmp.NickName;
				rtmp.Location = new Point(7 + (ii / 5) * 115, 9 + (ii % 5) * 16);
				rtmp.Size = new Size(35, 16);
				rtmp.AutoSize = true;
				rtmp.Click += new EventHandler(RadioButton_Click);
				this.groupBox1.Controls.Add(rtmp);
				ii++;
			}

			// originZ 加工原点Ｚの設定
			originZ.Enabled = false;
			// kataZ 金型上面Ｚの設定
			kataZ.Enabled = false;


			// comboBox1（材質）の設定
			// この材質は class NcOutput に代入され、KDATA へ入力される
			// 実際にはＮＣデータ作成時点ではSTEEL,NON-STEELの決定のみで
			// よいので不要である
			comboBox1.Items.Add("STEEL");
			comboBox1.Items.Add("NON-STEEL");
			comboBox1.SelectedIndex = 0;
			comboBox1.Enabled = true;

			///////////////////////
			// リストビューの設定
			///////////////////////
			frmSplash.label1.Text = "リストビューの設定中";
			Application.DoEvents();
			listView1.View = View.Details;
			listView1.Enabled = true;
			listView1.HideSelection = true;         // コントロールがフォーカスを失ったときに、選択されている項目が強調表示されない
			//listView1.LabelEdit = lvwManual;
			listView1.LabelEdit = true;             // コントロールの項目のラベルをユーザーが編集できる
			listView1.CausesValidation = false;     // そのコントロールが原因で、フォーカスを受け取っても検証されない
			listView1.FullRowSelect = true;         // 項目をクリックすると項目とそのすべてのサブ項目を選択する
			listView1.ShowItemToolTips = true;      // リストアイテムで設定されたツールヒントを表示する
			listView1.CheckBoxes = true;            // チェックボックスを表示する
			listView1.AllowColumnReorder = true;    // コラムの順序変更を許可する
			listView1.MultiSelect = false;
			listView1.GridLines = true;				// グリッド線を表示する

			// リストビューの設定－リストビューの列の表題
			LView.ColumnHeaderSet(listView1);

			// リストビューの設定－ソートの設定
			listView1.Sorting = SortOrder.None;
			listView1.ListViewItemSorter = new ListViewItemComparer(1, true);
			// Connect the ListView.ColumnClick event to the ColumnClick event handler.
			this.listView1.ColumnClick += new ColumnClickEventHandler(ColumnClick);
			// ColumnClick event handler.

			// メニュー・ツールバーの設定
			ｃｓｖデータToolStripMenuItem.Checked = false;
			送信ToolStripMenuItem.Enabled = false;
			送信toolStripButton.Enabled = false;

			frmSplash.label1.Text = "-----------------------------";
			Application.DoEvents();

			// SPLASH の終了
			frmSplash.Close();

		}

		// ////////////////////
		// フォームのイベント
		// ////////////////////
		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			using (StreamWriter sw = new StreamWriter(LocalHost.Homedir + "ncsend.ini", false)) {
				//MessageBox.Show(LocalHostCam.progdir + "ncsend.ini");
			}
			Application.DoEvents();
			LogOut.CheckCountOutput();
		}

		// //////////////////////////////////////
		// ＮＣデータフォルダーセットのイベント
		// //////////////////////////////////////

		/// <summary>
		/// テキストボックスのエンターキー
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void text1_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode != Keys.Enter) return;
			if (!Directory.Exists(text1.Text)) return;
			if (SelCamSystem.Name == CamSystem.Empty) {
				MessageBox.Show(
					"ＣＡＭシステム名を先に選択してください", "Information",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			try { Creat_ncDataSet(true); }
			catch (Exception ex) {
				listView1.Items.Clear();
				ncDataSet.Clear();
				MessageBox.Show(ex.Message, "ＮＣデータエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			Application.DoEvents();
			LogOut.CheckCountOutput();
		}
		/// <summary>テキストボックスのダブルクリック</summary>
		private void Text1_DoubleClick(object sender, EventArgs e) {
			FileNew();
			try { Creat_ncDataSet(true); }
			catch (Exception ex) {
				listView1.Items.Clear();
				ncDataSet.Clear();
				MessageBox.Show(ex.Message, "ＮＣデータエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			Application.DoEvents();
			LogOut.CheckCountOutput();
		}
		/// <summary>テキストボックスのドラッグ＆ドロップ</summary>
		private void Text1_DragEnter(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;
		}
		/// <summary>テキストボックスのドラッグ＆ドロップ</summary>
		private void Text1_DragDrop(object sender, DragEventArgs e) {
			DataObject dObject = new DataObject(DataFormats.FileDrop, e.Data.GetData(DataFormats.FileDrop));
			string path;
			if (dObject.GetFileDropList().Count == 1) {
				path = dObject.GetFileDropList()[0];
				if (Directory.Exists(path)) {
					((TextBox)sender).Text = path;
					try { Creat_ncDataSet(true); }
					catch (Exception ex) {
						listView1.Items.Clear();
						ncDataSet.Clear();
						MessageBox.Show(ex.Message, "ＮＣデータエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			Application.DoEvents();
			LogOut.CheckCountOutput();
		}


		private void FileNew() {
			if (SelCamSystem.Name == CamSystem.Empty) {
				MessageBox.Show(
					"ＣＡＭシステム名を先に選択してください", "Information",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog() {
				ShowNewFolderButton = false,
				Description = "ＮＣデータのあるフォルダーを選択"
			};
			if (text1.Text != "")
				folderBrowserDialog1.SelectedPath = text1.Text;
			else {
				string[] sdir;
				switch (SelCamSystem.Name) {
				case CamSystem.CADCEUS:
					folderBrowserDialog1.SelectedPath = @"\\202.15.85.149\cam\I\KATABAN";
					//folderBrowserDialog1.SelectedPath = @"\\192.16.2.76\share\I\KATABAN";
					//folderBrowserDialog1.SelectedPath = @"Z:\作業場_誰でも編集ＯＫ\03_技術G\sakamoto\NCD\SEND2_CEUS2";
					if (!Directory.Exists(folderBrowserDialog1.SelectedPath))
						folderBrowserDialog1.SelectedPath = LocalHost.Ncdtdir;
					break;
				case CamSystem.WorkNC:
				//case CamSystem.WorkNC_5AXIS:
					folderBrowserDialog1.SelectedPath = @"D:\mnc18";
					//folderBrowserDialog1.SelectedPath = @"Z:\作業場_誰でも編集ＯＫ\03_技術G\sakamoto\NCD\SEND2_WNC";
					if (!Directory.Exists(folderBrowserDialog1.SelectedPath))
						folderBrowserDialog1.SelectedPath = LocalHost.Ncdtdir;
					break;
				/*
				case CamSystem.Caelum:
					folderBrowserDialog1.SelectedPath = @"\\nt0040np\h\usr9\ASDM\caelum";
					if (!Directory.Exists(folderBrowserDialog1.SelectedPath))
						folderBrowserDialog1.SelectedPath = LocalHost.ncdtdir;
					break;
				*/
				default:
					sdir = Directory.GetDirectories(LocalHost.Ncdtdir);
					if (sdir.Length == 0) {
						sdir = new string[1];
						sdir[0] = LocalHost.Ncdtdir;
					}
					else Array.Sort(sdir);
					folderBrowserDialog1.SelectedPath = sdir[0];
					break;
				}
			}
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
				text1.Text = folderBrowserDialog1.SelectedPath;
			else
				text1.Text = "";
		}

		/// <summary>
		/// ＮＣ情報のあるフォルダーを指定した時に実行する。
		/// その情報を解析しNcInfo（工具単位情報を含むXMLなど）を作成する
		/// </summary>
		private void Creat_ncDataSet(bool newInfo) {

			// プログラムのバージョンのチェック
			if (CamUtil.ProgVersion.CheckVersion(null)) return;

			switch (SelCamSystem.Name) {
			case CamSystem.CADCEUS:
			case CamSystem.CADmeisterKDK:
			case CamSystem.CAMTOOL_5AXIS:
				if (CamUtil.ProgVersion.Debug) {
					MessageBox.Show("デバッグ：ＸＭＬ作成とＮＣ作成の最新プログラムが未検証のため一般は凍結中");
				}
				else {
					MessageBox.Show("近年、実行実績がないため凍結中です。再開する場合は藤本までご連絡ください。");
					return;
				}
				break;
			}

			if (text1.Text == "") return;

			// メッセージをまとめて表示
			List<NCINFO.NcInfoCam.MessageData> MessData = new List<NCINFO.NcInfoCam.MessageData>();

			// 保存フォルダーの設定（デバッグ時のみ）
			if (ProgVersion.Debug) {
				ServerPC.TempFolder();
				Application.DoEvents();
			}

			ncDataSet = new List<NCINFO.NcInfoCam>();
			listView1.Items.Clear();
			NCINFO.NcInfoCam.BuhinName = new ServerPC.PTPName(false);
			//LVSet(this.listView1,text1.Text);
			//public void LVSet(ListView lView, string ncDir)

			// 文字切り捨てのチェックのリセット
			StringLengthDB strLengthErr = new StringLengthDB();

			// ///////////////////////////
			// ＣＡＭ情報 KyData2 を作成
			// ///////////////////////////
			List<KYDATA.KyData> KyDList;
			try { KyDList = KYDATA.KyData.CreateKakoYoryo(text1.Text, SelCamSystem, originZ.Text == "" ? (double?)null : Convert.ToDouble(originZ.Text)); }
			catch (Exception ex) { text1.Text = ""; MessageBox.Show(ex.Message); return; }

			// 部品加工用の出力ファイル名を作成する
			NCINFO.NcInfoCam.BuhinName = new ServerPC.PTPName(KyDList[0].Buhin);
			if (NCINFO.NcInfoCam.BuhinName.NameExist == false) return;

			// ＣＡＭ情報 KyData2 より加工情報データ NcINfoCam（ＸＭＬファイル）を作成
			for (int ii = 0; ii < KyDList.Count; ii++) {
				ncDataSet.Add(new NCINFO.NcInfoCam(SelCamSystem, KyDList[ii], MessData, strLengthErr));
			}

			// ＮＣデータの並び替え
			if (KyDList[0].Buhin) {
				// NcTejunで加工順を更新時間でソート可能にするために、ここでの作成も出力順にソートする
				ncDataSet.Sort(NCINFO.NcInfoCam.CompareTime);
				// NcTejunで加工順をテキストファイル順でソート可能にするため
				foreach (string stmp in Directory.GetFiles(text1.Text))
					if (Path.GetExtension(stmp) == ".txt") {
						NCINFO.NcInfoCam.TextSortSet(ncDataSet, stmp);
						ncDataSet.Sort(NCINFO.NcInfoCam.CompareText);
					}
			}
			else {
				ncDataSet.Sort(NCINFO.NcInfoCam.CompareName);
			}

			if (MessData.Count != 0) {
				StringBuilder aa = new StringBuilder();
				foreach (NCINFO.NcInfoCam.MessageData md in MessData)
					aa.Append($"{md.fileName} : {md.mess}\r\n");
				FormMessageBox.Show("突出し量の設定", aa.ToString());
			}
			// ///////////////////////////////////////////
			// 文字の一部をカットした場合のメッセージ出力
			// ///////////////////////////////////////////
			strLengthErr.ErrorOut("Form1 575");

			// リストビューの作成
			ListViewItem lim;
			foreach (NCINFO.NcInfoCam ncd in ncDataSet) {
				// ファイル単位
				lim = new LView.ListViewItemFile(ncd);
				((LView.ListViewItemFile)lim).LVModeSet(ncd);
				listView1.Items.Add(lim);
			}
			listView1.ListViewItemSorter = new ListViewItemComparer(2, true);

			if (this.listView1.Items.Count != 0) {
				if (comboBox1.Text != "") {
					送信ToolStripMenuItem.Enabled = true;
					送信toolStripButton.Enabled = true;
				}
				CSVデータ表示toolStripButton.Enabled = true;
			}
			else {
				送信ToolStripMenuItem.Enabled = false;
				送信toolStripButton.Enabled = false;
				CSVデータ表示toolStripButton.Enabled = false;
			}

			// /////////////////////////////
			// 加工長再計算 ADD 2013/03/06
			// /////////////////////////////
			if (SelCamSystem.Dimension == 2 || SelCamSystem.Name == CamSystem.Tebis)
				Output("計算");
		}


		// //////////////////
		// 以下イベント処理
		// //////////////////

		/// <summary>
		/// ＣＡＭシステムの選択のイベント
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RadioButton_Click(object sender, EventArgs e) {

			if (SelCamSystem.Name == ((RadioButton)sender).Name) return;

			m_camSystem = new CamSystem(((RadioButton)sender).Name);
			text1.Text = "";

			// 加工原点Ｚの初期設定
			switch (SelCamSystem.Name) {
			case CamSystem.Dynavista2D:
				originZ.Text = "100.0";
				break;
			default:
				originZ.Text = "";
				break;
			}

			// 加工上面高さの入力
			switch (SelCamSystem.Name) {
			//case CamSystem.Caelum:
			case CamSystem.CADmeisterKDK:
				kataZ.Enabled = true;
				kataZ.Text = "0.0";
				break;
			default:
				kataZ.Enabled = false;
				kataZ.Text = "";
				break;
			}

			// 突出し量設定メニューとボタンの使用可否
			switch (SelCamSystem.Name) {
			//case CamSystem.Caelum:
			case CamSystem.CADmeisterKDK:
				toolStripButtonTSet.Enabled = true;
				toolStripMenuItemTSet.Enabled = true;
				break;
			case CamSystem.CADCEUS:
			default:
				toolStripButtonTSet.Enabled = false;
				toolStripMenuItemTSet.Enabled = false;
				break;
			}
			// ヘリカル変換メニューとボタンの使用可否
			switch (SelCamSystem.Name) {
			/*
			case CamSystem.Caelum:
				toolStripButtonHelical.Enabled = true;
				toolStripMenuItemHelical.Enabled = true;
				break;
			*/
			case CamSystem.CADCEUS:
			case CamSystem.CADmeisterKDK:
			default:
				toolStripButtonHelical.Enabled = false;
				toolStripMenuItemHelical.Enabled = false;
				break;
			}
			switch (Form1.SelCamSystem.Name) {
			case CamSystem.Tebis:
			case CamSystem.Dynavista2D:
			//case CamSystem.Caelum:
			case CamSystem.CADmeisterKDK:
			case CamSystem.CADCEUS:
				checkBox_XmlSet.Enabled = true;
				break;
			case CamSystem.CAMTOOL:
			case CamSystem.CAMTOOL_5AXIS:
			case CamSystem.WorkNC:
			//case CamSystem.WorkNC_5AXIS:
			default:
				checkBox_XmlSet.Checked = false;
				checkBox_XmlSet.Enabled = false;
				break;
			}
			text1.AllowDrop = true;
		}

		/// <summary>
		/// 材質選択のイベント
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox1_TextChanged(object sender, EventArgs e) {
			//if (NcdServer.Connect && listView1.Items.Count > 0)
			if (listView1.Items.Count > 0) {
				送信ToolStripMenuItem.Enabled = true;
				送信toolStripButton.Enabled = true;
			}
		}

		/// <summary>
		/// 工具分割設定のイベント
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolBunk_CheckedChanged(object sender, EventArgs e) {
			string vName;
			foreach (NCINFO.NcInfoCam ncd in ncDataSet) {
			//for (int ii = 0; ii < ncDataSet.Count; ii++) {
				vName = Path.GetFileNameWithoutExtension(ncd.FullNameCam);
				((LView.ListViewItemFile)listView1.Items[vName]).LVModeSet(ncd);
			}
		}

		/// <summary>
		/// 加工長再計算
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonCalc_Click(object sender, EventArgs e) {
			Output("計算");
		}

		/// <summary>
		/// リストビューのイベント
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListView1_AfterLabelEdit(object sender, LabelEditEventArgs e) {
			if (((ListView)sender).SelectedItems.Count != 1) {
				e.CancelEdit = true;
				return;
			}
			if (e.Label == null) {
				e.CancelEdit = true;
				return;
			}

			int index = ((ListView)sender).SelectedIndices[0];

			if ((NCINFO.NcInfoCam.LVM)listView1.Items[index].Tag == NCINFO.NcInfoCam.LVM.エラー) {
				e.CancelEdit = true;
				return;
			}

			// ＮｃＤａｔａＳｅｔのインデックスを求める
			int index2 = -1;
			for (int jj = 0; jj < ncDataSet.Count; jj++) {
				if (listView1.Items[index].Name == Path.GetFileNameWithoutExtension(ncDataSet[jj].FullNameCam)) {
					index2 = jj;
					break;
				}
			}

			// 入力された文字列をチェックする
			char err = ' ';
			switch (ncDataSet[index2].xmlD.BaseNcFormat.Id) {
			case CamUtil.BaseNcForm.ID.BUHIN:
				foreach (char aa in e.Label) {
					if (Char.IsNumber(aa) || aa == '_' || Char.IsLetter(aa) || aa == '-') { ;}
					else { err = aa; break; }
				}
				break;
			default:
				foreach (char aa in e.Label) {
					if (Char.IsNumber(aa) || aa == '_' || Char.IsUpper(aa)) { ;}
					else { err = aa; break; }
				}
				break;
			}
			if (err != ' ') {
				MessageBox.Show(
					"英数字以外の文字 '" + err.ToString() + "' が含まれています",
					"Form1.listView1_AfterLabelEdit",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				e.CancelEdit = true;
				return;
			}

			// 新たな出力名に対して再計算する
			ncDataSet[index2].OutName = e.Label;
			ncDataSet[index2].CheckOutName();
			((LView.ListViewItemFile)listView1.Items[index]).LVModeSet(ncDataSet[index2]);
			return;
		}
		private void ColumnClick(object o, ColumnClickEventArgs e) {
			// Set the ListViewItemSorter property to a new ListViewItemComparer 
			// object. Setting this property immediately sorts the 
			// ListView using the ListViewItemComparer object.
			if (((ListViewItemComparer)listView1.ListViewItemSorter).col == e.Column) {
				if (((ListViewItemComparer)listView1.ListViewItemSorter).asc == true)
					listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, false);
				else
					listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, true);
			}
			else listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, true);
		}
		private void ListView1_ItemCheck(object sender, ItemCheckEventArgs e) {
			if ((NCINFO.NcInfoCam.LVM)this.listView1.Items[e.Index].Tag == NCINFO.NcInfoCam.LVM.エラー)
				e.NewValue = CheckState.Unchecked;
			if ((NCINFO.NcInfoCam.LVM)this.listView1.Items[e.Index].Tag == NCINFO.NcInfoCam.LVM.出力名)
				e.NewValue = CheckState.Unchecked;
		}


		//////////////////////////////
		// メニュークリックのイベント
		//////////////////////////////

		private void 新規作成NToolStripMenuItem_Click(object sender, EventArgs e) {
			FileNew();
			try { Creat_ncDataSet(true); }
			catch (Exception ex) {
				listView1.Items.Clear();
				ncDataSet.Clear();
				MessageBox.Show(ex.Message, "ＮＣデータエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			Application.DoEvents();
			LogOut.CheckCountOutput();
		}

		/// <summary>
		/// ＮＣデータなどの送信イベント
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void 送信ToolStripMenuItem_Click(object sender, EventArgs e) {
			Output("出力");
			Application.DoEvents();
			LogOut.CheckCountOutput();
		}

		/// <summary>
		/// ＮＣデータなどの変換・送信処理
		/// </summary>
		/// <param name="aim">ＮＣデータ作成の目的</param>
		private void Output(string aim) {

			bool sosin;
			string vName;
			bool _5axis = false; 

			switch (SelCamSystem.Name) {
			//case CamSystem.Caelum:
			case CamSystem.CADmeisterKDK:
				if (kataZ.Text == "") {
					MessageBox.Show("「金型上面Ｚ」の値を設定してから実行してください。");
					return;
				}
				break;
			}

			// 送信の設定
			switch (aim) {
			case "出力":
				sosin = true;
				break;
			case "計算":
				sosin = false;
				break;
			default: throw new Exception("qwfqwerfwvervgfg");
			}

			Output.NcOutput ncOut;
			DialogResult result = DialogResult.OK;

			foreach (NCINFO.NcInfoCam ncd in ncDataSet) {
				vName = Path.GetFileNameWithoutExtension(ncd.FullNameCam);
				switch (aim) {
				case "出力":
					if (listView1.Items[vName].Checked == false)
						continue;
					break;
				case "計算":
					if (ncd.xmlD == null)
						continue;
					if (ncd.xmlD.CamDimension != 2)
						continue;
					if (ncd.xmlD[0].SNAME == "MP700_B")
						continue;
					break;
				}

				// 出力インスタンスの作成
				ncOut = null;
				try {
					ncOut = NCSEND2.Output.NcOutput.Factory(Form1.SelCamSystem, ncd, kataZ);
					if (ncd.xmlD.BaseNcFormat.Id == BaseNcForm.ID.BUHIN) _5axis = true;

					// /////////////////
					// 共通の条件の設定
					// /////////////////
					((Output.NcOutput)ncOut).JokenSet(comboBox1.Text == "STEEL", sosin, checkBox_Unix.Checked, checkBox_XmlSet.Checked);

					if (aim == "出力") {
						// ダイアログを用いてSetMain, SetToolを実行
						StringBuilder errMessage = new StringBuilder();
						using (FormCommonDialog frmtmp = new FormCommonDialog(listView1.Items[vName].Text, ncOut.FtpPut, errMessage)) {
							Application.DoEvents();
							result = frmtmp.ShowDialog();
						}
						// Cancel の場合、以降中止
						if (result != DialogResult.OK) {
							if (errMessage.Length > 0)
								MessageBox.Show(errMessage.ToString(), "変換結果");
							break;
						}
					}
					// 計算
					else {
						Label ltmp = new Label();
						ncOut.FtpPut(ltmp);
					}
				}
				finally {
					ncOut?.Dispose();
					ncOut = null;
				}

				// 加工長、加工時間の再設定
				if (ncd.xmlD.CamDimension == 2 || this.checkBox_XmlSet.Checked)
					((LView.ListViewItemFile)listView1.Items[vName]).LVNcLenSet(ncd);
			}

			if (aim == "出力") {
				// 出力ＮＣデータのリストをNcTejunのために保存する
				using (StreamWriter sw = new StreamWriter("InsertNcDataName", false)) {
					foreach (NCINFO.NcInfoCam ncd in ncDataSet)
						if (listView1.Items[Path.GetFileNameWithoutExtension(ncd.FullNameCam)].Checked)
							sw.WriteLine(ncd.OutName);

					// 部品加工の場合エアーブローを追加する
					if (_5axis) {
						string outn = NCINFO.NcInfoCam.BuhinName.AddName + "airblow";
						File.WriteAllLines(ServerPC.FulNcName(outn + ".ncd"), File.ReadAllLines(ServerPC.SvrFldrC + "prog_airblow.ncd"));
						File.WriteAllLines(ServerPC.FulNcName(outn + ".xml"), File.ReadAllLines(ServerPC.SvrFldrC + "prog_airblow_V14.xml"));
						sw.WriteLine(outn);
					}
				}

				// ログを保存する
				LogOut.CheckOutput(LogOut.FNAM.NCSENDLOG, LocalHost.Name, SelCamSystem.Name,
					$"{ncDataSet[0].OutName} {LocalHost.IPAddress} {text1.Text}");
			}

			if (result == DialogResult.OK) {
				// 実行結果更新
				foreach (NCINFO.NcInfoCam ncd in ncDataSet) {
				//for (int ii = 0; ii < ncDataSet.Count; ii++) {
					vName = Path.GetFileNameWithoutExtension(ncd.FullNameCam);

					// 送信状況を再確認する
					if (listView1.Items[vName].Checked && sosin) {
						ncd.CheckOutName();
						((LView.ListViewItemFile)listView1.Items[vName]).LVModeSet(ncd);
					}
				}
			}
			return;
		}

		private void 新規作成NToolStripButton_Click(object sender, EventArgs e) {
			新規作成NToolStripMenuItem_Click(sender, e);
		}
		private void 送信toolStripButton_Click(object sender, EventArgs e) {
			送信ToolStripMenuItem_Click(sender, e);
		}
		private void CSVデータ表示toolStripButton_Click(object sender, EventArgs e) {
			CSVデータToolStripMenuItem_Click(sender, e);
		}

		/// <summary>
		/// ツールセットＣＡＭと突出し量の変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStripButtonTSet_Click(object sender, EventArgs e) {
			ToolStripMenuItemTSet_Click(sender, e);
		}

		/// <summary>
		/// ヘリカル変換の実行
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStripButtonHelical_Click(object sender, EventArgs e) {
			ToolStripMenuItemHelical_Click(sender, e);
		}

		/// <summary>
		/// 工具単位リストビューの表示
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStripButton2_Click(object sender, EventArgs e) {
			FormLView frmLView = new FormLView(ncDataSet);
			frmLView.Show();
		}



		private void KataZ_TextChanged(object sender, EventArgs e) {
			try { double dd = Convert.ToDouble(kataZ.Text); }
			catch { kataZ.Text = ""; }
		}


		////////////////////////////////////
		// メニュークリックのイベント
		////////////////////////////////////
		
		private void 終了XToolStripMenuItem_Click(object sender, EventArgs e) {
			this.Close();
		}
		private void ツールバーToolStripMenuItem_Click(object sender, EventArgs e) {
			ツールバーToolStripMenuItem.Checked = !ツールバーToolStripMenuItem.Checked;
			toolStrip1.Visible = ツールバーToolStripMenuItem.Checked;
		}
		private void ステータスバーToolStripMenuItem_Click(object sender, EventArgs e) {
			ステータスバーToolStripMenuItem.Checked = !ステータスバーToolStripMenuItem.Checked;
			statusStrip1.Visible = ステータスバーToolStripMenuItem.Checked;
		}

		// ＣＳＶフォームの表示・非表示
		private void CSVデータToolStripMenuItem_Click(object sender, EventArgs e) {
			frmCsvData = new KYDATA.FormCsvData(text1.Text, SelCamSystem, originZ.Text);
			frmCsvData.Show();
		}

		/// <summary>
		/// ツールセットＣＡＭと突出し量の変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStripMenuItemTSet_Click(object sender, EventArgs e) {

			if (CamUtil.ProgVersion.Debug) {
				MessageBox.Show("デバッグ：ＸＭＬ作成とＮＣ作成の最新プログラムが未検証のため一般は凍結中");
			}
			else {
				MessageBox.Show("近年、実行実績がないため凍結中です。再開する場合は藤本までご連絡ください。");
				return;
			}

			// ツールセットＣＡＭを選択し決定する
			KYDATA.FormSelTset selTset = new KYDATA.FormSelTset(ncDataSet, SelCamSystem.Name);
			DialogResult result = selTset.ShowDialog();
			if (result == DialogResult.Cancel) return;
		}

		private void ToolStripMenuItemHelical_Click(object sender, EventArgs e) {
		}
	}

	// Implements the manual sorting of items by columns.
	class ListViewItemComparer : System.Collections.IComparer
	{
		public int col;
		public bool asc;

		public ListViewItemComparer() {
			col = 0;
			asc = true;
		}
		public ListViewItemComparer(int column) {
			col = column;
			asc = true;
		}
		public ListViewItemComparer(int column, bool ascending) {
			col = column;
			asc = ascending;
		}
		public int Compare(object x, object y) {
			string sx = ((ListViewItem)x).SubItems[col].Text;
			string sy = ((ListViewItem)y).SubItems[col].Text;

			if (((ListViewItem)x).ListView.Columns[col].TextAlign == HorizontalAlignment.Right) {
				if (sx == "")
					sx = "-999999999";
				if (sy == "")
					sy = "-999999999";
				if (asc == true)
					return (Convert.ToDouble(sx) >= Convert.ToDouble(sy)) ? 1 : -1;
				else
					return (Convert.ToDouble(sx) >= Convert.ToDouble(sy)) ? -1 : 1;
			}
			else {
				if (asc == true)
					return String.Compare(sx, sy);
				else
					return String.Compare(sy, sx);
			}
		}
	}
}
