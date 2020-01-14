using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using CamUtil;

namespace NcTejun.TejunSet
{
	/// <summary>
	/// 「加工手順」をデータグリッドビューで表示する。
	/// 表示関連情報はこちらであるが情報・メソッドの本体はTejunクラスである。
	/// </summary>
	partial class FormTejun : Form
	{
		/// <summary>最大文字数（ＤＢの要請）：プロセスシート名</summary>
		private const int maxPSHEET_LENGTH = 16;
		/// <summary>最大文字数（ＤＢの要請）：ツールシート名</summary>
		private const int maxTSHEET_LENGTH = 16;

		//private Form1 frm1 = null;
		///// <summary>加工手順書</summary>
		//public _DataSet tejun;
		//List<string> TejunText;

		/// <summary>フォルダー名</summary>
		public string TjnNam_folder { get { return tjnNam.TjnNameTemp; } }
		/// <summary>手順名と手順モード</summary>
		private TjnNam tjnNam;
		private struct TjnNam
		{
			private enum Crt_Opn_Imp
			{
				/// <summary>新規作成(UNIX)</summary>
				CREATE_UX,
				/// <summary>新規作成(PC)</summary>
				CREATE_PC,
				/// <summary>ユニックスからのインポート</summary>
				IMPORT,
				/// <summary>オープン（検証済 NCSPEED_edit）</summary>
				OPEN0,
				/// <summary>オープン（未検証 NCSPEED）</summary>
				OPEN1
			}

			/// <summary>手順名</summary>
			public string TjnNameTemp { get { if (m_tjnNameTemp != null) return m_tjnNameTemp; else throw new Exception("qrefqberfh"); } }
			/// <summary>手順名が存在する場合</summary>
			public bool TjnNameExists { get { return m_tjnNameTemp != null; } }
			private string m_tjnNameTemp;

			/// <summary>検証の有無</summary>
			public bool Nspd {
				get {
					switch (crt_opn_imp) {
					case Crt_Opn_Imp.OPEN0:
						return true;
					case Crt_Opn_Imp.OPEN1:
					case Crt_Opn_Imp.CREATE_PC:
						return false;
					default:
						throw new Exception("crt_opn_imp ERROR");
					}
				}
			}

			/// <summary>手順データのモード</summary>
			private Crt_Opn_Imp crt_opn_imp;

			/// <summary>手順の新規作成</summary>
			public TjnNam(ToolStripTextBox p_tname) {
				p_tname.Enabled = true;
				crt_opn_imp = Crt_Opn_Imp.CREATE_PC;
				m_tjnNameTemp = null;
			}
			/// <summary>既存の手順を開く</summary>
			public TjnNam(ToolStripTextBox p_tname, string fultejunname) {
				string dir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(fultejunname)));
				if (fultejunname.IndexOf("\\" + ServerPC.EDT + "\\") >= 0) {
					crt_opn_imp = Crt_Opn_Imp.OPEN0;
					p_tname.Enabled = false;
					m_tjnNameTemp = ServerPC.SvrFldrSedt + dir + "\\" + p_tname.Text;
					return;
				}
				if (fultejunname.IndexOf("\\" + ServerPC.ORG + "\\") >= 0) {
					crt_opn_imp = Crt_Opn_Imp.OPEN1;
					p_tname.Enabled = true;
					m_tjnNameTemp = ServerPC.SvrFldrSorg + dir + "\\" + p_tname.Text;
					return;
				}
				throw new Exception("手順の保存用フォルダーではないファイルが選択されました。");
			}
			/// <summary>手順名変更時の処理</summary>
			public void Change(string tname) {
				if (crt_opn_imp == Crt_Opn_Imp.OPEN1)
					crt_opn_imp = Crt_Opn_Imp.CREATE_PC;
				switch (crt_opn_imp) {
				case Crt_Opn_Imp.OPEN0:
					m_tjnNameTemp = ServerPC.TejunName("EDT", tname);
					break;
				case Crt_Opn_Imp.OPEN1:
				case Crt_Opn_Imp.CREATE_PC:
					m_tjnNameTemp = ServerPC.TejunName("ORG", tname);
					break;
				default:
					throw new Exception("crt_opn_imp ERROR");
				}
			}

			/// <summary>保存処理</summary>
			/// <param name="tmpFName">手順が保存されている一時ファイル名</param>
			/// <param name="seiban">製造番号</param>
			/// <returns>保存の可否</returns>
			public bool Hozon(string tmpFName, string seiban) {
				DialogResult result;
				string fulnam;

				switch (crt_opn_imp) {
				case Crt_Opn_Imp.CREATE_PC:
					fulnam = TjnNameTemp + "\\TejunBase";
					if (File.Exists(fulnam)) {
						MessageBox.Show(
							"ファイル：" + Path.GetFileName(TjnNameTemp) + " はすでにＰＣ上に存在しています。保存できません。",
							"ファイルの上書き確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
						result = DialogResult.Cancel;
					}
					else {
						result = MessageBox.Show(
							"新しいＰＣの手順ファイル：" + Path.GetFileName(TjnNameTemp) + " を作成しますか？",
							"ファイルの作成確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
					}
					break;
				case Crt_Opn_Imp.OPEN0:
					fulnam = TjnNameTemp + "\\Tejun";
					result = MessageBox.Show(
						"ＮＣＳＰＥＥＤ検証後の手順データを上書き保存します。",
						"処理確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
					if (result == DialogResult.OK) {
						if (!File.Exists(fulnam + ".org")) File.Copy(fulnam, fulnam + ".org");
						File.Delete(fulnam);
					}
					break;
				case Crt_Opn_Imp.OPEN1:
					fulnam = TjnNameTemp + "\\TejunBase";
					// すでにＮＣＳＰＥＥＤ実行済みであれば保存できない
					if (File.Exists(TjnNameTemp + "\\Tejun")) {
						MessageBox.Show(
							"手順データはＮＣＳＰＥＥＤ実行時に保存されます。手順データ修正後はＮＣＳＰＥＥＤを再実行してください。",
							"処理確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
						result = DialogResult.Cancel;
					}
					else {
						if (File.Exists(fulnam)) File.Delete(fulnam);
						result = DialogResult.OK;
					}
					break;
				default:
					throw new Exception("crt_opn_imp ERROR");
				}
				if (result == DialogResult.OK) {
					if (crt_opn_imp == Crt_Opn_Imp.CREATE_PC)
						crt_opn_imp = Crt_Opn_Imp.OPEN1;
				}

				if (result == DialogResult.OK) {
					// 金型単位のディレクトリを作成する
					Directory.CreateDirectory(TjnNameTemp);
					// ファイルを保存する
					File.Copy(tmpFName, fulnam);
					// 製造番号の保存 2015/07/17
					if (Directory.GetFiles(Path.GetDirectoryName(TjnNameTemp), "*.sno").Length == 0)
						File.Create(Path.GetDirectoryName(TjnNameTemp) + "\\" + seiban + ".sno");
					return true;
				}
				else
					return false;
			}
		}


		/// <summary>現在の手順書が保存できないか保存されていれば true。</summary>
		private bool Tejun_save {
			get { return !上書き保存SToolStripButton.Enabled; }
			set { 上書き保存SToolStripButton.Enabled = !value && tjnNam.TjnNameExists; }
		}

		/// <summary>手順書が確定していれば true。（Enabled==falseで手順名設定済み）</summary>
		public bool Tejun_commit {
			get { return !CommittoolStripButton.Enabled && tjnNam.TjnNameExists; }
			set {
				if (!tjnNam.TjnNameExists) {
					CommittoolStripButton.Enabled = false;
					toolStrip_SFSet.Enabled = false;
					//toolStripMenuItem_Shokichi.Enabled = false;
					toolStrip_Taore.Enabled = false;
				}
				else {
					bool set = CommittoolStripButton.Enabled;
					CommittoolStripButton.Enabled = !value;
					toolStrip_SFSet.Enabled = value;
					//toolStripMenuItem_Shokichi.Enabled = value;
					toolStrip_Taore.Enabled = value;
					// value==trueの場合、手順にエラーがなく確定したことをForm1に知らせる
					if (set == value) Program.frm1.CheckedChanged();
				}
			}
		}
		/// <summary>データグリッドビューのセルが確定していれば true。</summary>
		public bool DgvCommit { get { return !dataGridView1.IsCurrentCellDirty; } }
		/// <summary>内製管理番号が変更されていれば true。</summary>
		private bool naisei_change;

		/// <summary>手順書の名前が変更された直後にtrue</summary>
		private bool change_tname;



		/// <summary>
		/// コンストラクタ（新規）
		/// </summary>
		public FormTejun()
		{
			InitializeComponent();
			SetComboItem();
			tjnNam = new TjnNam(toolStrip_Tname);
			//tejun = null;
			NcdTool.Tejun.Set();

			SetView();

			// 新規作成時に初期の項目を入力するShokiSet();
			{
				string[] aa = new string[2];
				DataGridViewRowCollection rows = this.dataGridView1.Rows;

				toolStrip_Tname.Text = "temp";
				toolStrip_user.Text = "";
				toolStrip_seba.Text = "000000";
				toolStrip_kdate.Text = DateTime.Now.ToString("yyyy/MM/00");
				toolStrip_machn.Text = "DMU200P";

				rows.Add(new string[] { "D", "2D" });
				rows.Add(new string[] { "Z", "S50C" });
				rows.Add(new string[] { "F", "1.0" });
				rows.Add(new string[] { "S", "1.0" });
				rows.Add(new string[] { "O", "1001" });
				//rows.Add(new string[] { "N", "XQINRO" });
			}

			this.Tejun_save = true;
			Tejun_commit = true;
			this.toolStrip_Tname.Enabled = true;
			change_tname = false;
			this.naisei_change = true;
			return;
		}

		/// <summary>
		/// コンストラクタ（ＰＣ内の手順データをオープン）
		/// </summary>
		/// <param name="fultejunname">手順データ名</param>
		public FormTejun(string fultejunname)
		{
			StreamReader fpt = null;	//StreamReader fpt;

			InitializeComponent();

			//m_tjnDir = Path.GetDirectoryName(fultejunname);
			SetComboItem();


			// 手順名を手順データファイルのあるディレクトリ名にする
			// （crt_opn_imp = Crt_Opn_Imp.OPEN1であると変更されるので注意）
			//crt_opn_imp = Crt_Opn_Imp.CREATE_UX;	// ダミーセット

			toolStrip_Tname.Text = Path.GetFileName(Path.GetDirectoryName(fultejunname));
			tjnNam = new TjnNam(toolStrip_Tname, fultejunname);
			//tejun = null;
			NcdTool.Tejun.Set();

			SetView();

			try {
				fpt = new StreamReader(fultejunname);
				TejunRead(fpt);
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message, "FormTejun",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				throw;
			}
			finally {
				fpt?.Dispose();
				fpt = null;
			}
			this.Tejun_save = true;
			Tejun_commit = false;
			change_tname = false;
			this.naisei_change = false;
			return;
		}

		/// <summary>選択リストの作成（コンストラクタで実行）</summary>
		private void SetComboItem() {
			// 製造番号選択リストの作成
			toolStrip_seba2.DropDownStyle = ComboBoxStyle.DropDownList;
			toolStrip_seba2.ComboBox.DataBindings.Clear();
			toolStrip_seba2.ComboBox.DataSource = CasaData.Casa.Tables["DBJNO"];
			toolStrip_seba2.ComboBox.DisplayMember = "KATA_DISP";
			toolStrip_seba2.ComboBox.ValueMember = "J_NO";
			toolStrip_seba2.MaxDropDownItems = 20;
			toolStrip_seba2.DropDownWidth = 320;
			toolStrip_seba2.SelectedIndex = -1;
			toolStrip_seba2.SelectedIndexChanged += new EventHandler(ToolStrip_seba2_SelectedIndexChanged);
			// 加工機選択リストの作成
			//for (int ii = 0; ii < Machine.machList.Length; ii++) {
			//	toolStrip_machn.Items.Add(Machine.machList[ii]);
			//	if (Machine.G01(Machine.machList[ii]))
			//		toolStrip_machn.Items.Add(Machine.machList[ii] + "_G01");
			//}
			toolStrip_machn.Items.AddRange(Machine.MachList);
		}

		// /////////////
		// イベント処理
		// /////////////

		// ボタン

		/// <summary>
		/// 加工手順変更の確定処理（Program.tejunの作成）
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CommittoolStripButton_Click(object sender, EventArgs e) {

			// 手順の簡易チェック
			if (TejunCheckOK() == false) return;

			// 内製管理番号のチェック。修正した場合のみチェックし、そのままの情報（過去の製造番号）は認める
			// ADD in 2013/08/23
			if (naisei_change) {
				bool ng = true;
				if (ng) {
					foreach (DataRow dRow in CasaData.Casa.Tables["DBJNO"].Rows)
						if (toolStrip_seba.Text == (string)dRow["J_NO"]) { ng = false; break; }
				}
				if (ng) {
					MessageBox.Show("使用可能な内製管理Noではない。選択リストから選んでください");
					return;
				}
			}

			// ////////////////////
			// 手順情報をセットする
			// ////////////////////
			try {
				NcdTool.Tejun.Set(
					ToListString(), toolStrip_Tname.Text, toolStrip_user.Text, toolStrip_seba.Text, toolStrip_machn.Text, toolStrip_kdate.Text,
					tjnNam.Nspd, tjnNam.TjnNameTemp);
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message + "\n手順は「確定」できません。");
				return;
			}
			// ////////////////////
			// 加工手順を作成する
			// ////////////////////
			try {
				Program.frm1.TsheetClear();
				CamUtil.LogOut.warn = new StringBuilder();
				_DataSet tejun = new _DataSet();
				tejun.TejunSet();
				Output.FormNcSet.OUT = null;
				Output.FormNcSet.casaData = null;
			}
			catch (Exception ex) {
				switch (ex.Message.Substring(0, 2)) {
				case "手順":	// 手順書のエラー
					MessageBox.Show(ex.Message.Substring(2) + "\n手順は「確定」できません。");
					//((Form1)this.ParentForm).checkTejun.Checked = false;
					return;
				case "工具":	// 工具表のエラー
					MessageBox.Show(
						"工具表で以下のエラーが発生しました。修正してください。\n" +
						ex.Message.Substring(2));
					// 新ツールシートの表示
					foreach (ToolSheet ttmp in Program.frm1.TsheetList) {
						ttmp.MdiParent = this.MdiParent;
						ttmp.Show();
					}
					// 手順を確定
					Tejun_commit = true;
					return;
				default:
					MessageBox.Show("プログラムエラー");
					Application.Exit();
					throw new Exception("プログラムエラー");
				}
			}

			// 新ツールシートの表示
			foreach (ToolSheet ttmp in Program.frm1.TsheetList) {
				ttmp.MdiParent = this.MdiParent;
				ttmp.Show();
			}
			// 手順を確定
			Tejun_commit = true;
			this.naisei_change = false;
		}

		// フォームを閉じる前
		private void FormTejun_FormClosing(object sender, FormClosingEventArgs e) {
			if (Tejun_save == false) {
				DialogResult result = MessageBox.Show(
					"加工手順は保存せず終了します。よろしいですか？", "FromTejun",
					MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				if (result == DialogResult.Cancel) {
					e.Cancel = true;
					return;
				}
			}

			// 工具表のフォームを閉じる
			if (NcdTool.Tejun.NcList.NcNamsAll.Count != 0) {
				foreach (ToolSheet ts in Program.frm1.TsheetList) {
					if (ts.Tolst_save && ts.Tolst_commit && ts.DgvCommit)
						continue;
					DialogResult result = MessageBox.Show(
						"工具表" + ts.TolstName + "は保存せず終了します。よろしいですか？", "FromTejun",
						MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
					if (result == DialogResult.Cancel) {
						e.Cancel = true;
						return;
					}
				}
				Program.frm1.TsheetClear();
			}
			Tejun_commit = false;

			// 親のForm1の処理
			((Form1)this.MdiParent).TejunClose();
		}
		// フォームを閉じる処理後
		private void FormTejun_FormClosed(object sender, FormClosedEventArgs e) {
			//tejun = null;
			NcdTool.Tejun.Set();
		}
		// 手順名の変更
		private void ToolStripName_TextChanged(object sender, EventArgs e) {
			change_tname = true;
		}
		/// <summary>手順名の変更が確定</summary>
		private void ToolStrip_Tname_Leave(object sender, EventArgs e) {
			string[] ddat;

			if (change_tname == false) return;
			change_tname = false;

			// 手順名のチェック
			if (TNameCheckOK() == false) return;

			this.Text = "加工手順： " + toolStrip_Tname.Text;
			tjnNam.Change(toolStrip_Tname.Text);
			Tejun_save = Tejun_commit = false;

			// 製造番号のセット
			if (Directory.Exists(Path.GetDirectoryName(tjnNam.TjnNameTemp))) {
				ddat = Directory.GetFiles(Path.GetDirectoryName(tjnNam.TjnNameTemp), "*.sno");
				switch (ddat.Length) {
				case 1:
					toolStrip_seba.Text = Path.GetFileNameWithoutExtension(ddat[0]);
					break;
				case 2:
					if (Path.GetFileNameWithoutExtension(ddat[0]) == "111111")
						toolStrip_seba.Text = Path.GetFileNameWithoutExtension(ddat[1]);
					if (Path.GetFileNameWithoutExtension(ddat[1]) == "111111")
						toolStrip_seba.Text = Path.GetFileNameWithoutExtension(ddat[0]);
					break;
				}
			}

			// 個人設定の反映
			if (File.Exists(ServerPC.SvrFldrC + "init_tejun\\tejun_" + toolStrip_Tname.Text[0] + ".ini")) {
				char uId = toolStrip_Tname.Text[0];
				char[] demi = new char[] { ':' };
				DataGridViewRow rowD = null, rowZ = null;

				foreach (DataGridViewRow row in dataGridView1.Rows) {
					if (row.Cells[0].Value == null) continue;
					if (((string)row.Cells[0].Value).Length == 0) continue;
					if (rowD == null && ((string)row.Cells[0].Value) == "D") rowD = row;
					if (rowZ == null && ((string)row.Cells[0].Value) == "Z") rowZ = row;
					if (rowD != null && rowZ != null) break;
				}

				using (System.IO.StreamReader sr = new StreamReader(ServerPC.SvrFldrC + "init_tejun\\tejun_" + uId + ".ini")) {
					while (!sr.EndOfStream) {
						ddat = sr.ReadLine().Split(demi);
						if (ddat.Length < 2) continue;
						switch (ddat[0].Trim()) {
						case "usr":
							toolStrip_user.Text = ddat[1].Trim();
							break;
						case "mch":
							toolStrip_machn.Text = ddat[1].Trim();
							break;
						case "dim":
							rowD.Cells[1].Value = ddat[1].Trim();
							break;
						case "mat":
							rowZ.Cells[1].Value = ddat[1].Trim();
							break;
						case "tst":
							if (uId == 'D' || uId == 'L') {
								this.dataGridView1.Rows.Add(new string[] { "T", toolStrip_Tname.Text.Substring(0, 3) });
							}
							else {
								this.dataGridView1.Rows.Add(new string[] { "T", toolStrip_Tname.Text });
							}
							break;
						}
					}
				}
			}
		}

		// 作成者番号の変更
		private void UserName_TextChanged(object sender, EventArgs e)	{
			Tejun_save = Tejun_commit = false;
		}
		// 内製管理番号の変更
		private void ToolStrip_seba_TextChanged(object sender, EventArgs e) {
			Tejun_save = Tejun_commit = false;
			// ADD 2013/09/05
			naisei_change = true;
		}
		// 内製管理番号リストの選択
		private void ToolStrip_seba2_SelectedIndexChanged(object sender, EventArgs e) {
			toolStrip_seba.Text = (string)((DataRowView)toolStrip_seba2.SelectedItem).Row["J_NO"];
		}

		private void ToolStrip_kdate_Click(object sender, EventArgs e) {
			StringBuilder ss = new StringBuilder();
			Calendar cal = new Calendar(ss) {
				StartPosition = FormStartPosition.Manual,
				Location = new Point(Program.frm1.Location.X + 450, Program.frm1.Location.Y + 200)
			};
			cal.ShowDialog(Program.frm1);
			if (ss.Length > 0)
				toolStrip_kdate.Text = ss.ToString();
		}
		// 加工予定日の変更
		private void ToolStrip_kdate_TextChanged(object sender, EventArgs e) {
			Tejun_save = Tejun_commit = false;
		}
		// 加工機名の変更
		private void ToolStrip_machn_TextChanged(object sender, EventArgs e) {
			Tejun_save = Tejun_commit = false;
		}

		internal void 上書き保存SToolStripButton_Click(object sender, EventArgs e) {

			// 手順の簡易チェック
			if (TejunCheckOK() == false) return;

			try {
				// 一時手順ファイルの作成
				System.CodeDom.Compiler.TempFileCollection tmpFile =
					new System.CodeDom.Compiler.TempFileCollection(LocalHost.Tempdir, false);
				string tmpFName = tmpFile.BasePath + ".tmp";
				using (StreamWriter sw = new StreamWriter(tmpFName)) {
					foreach (string str in ToListString())
						sw.WriteLine(str);
				}
				// 保存
				bool result = tjnNam.Hozon(tmpFName, toolStrip_seba.Text);
				if (File.Exists(tmpFName)) File.Delete(tmpFName);
				if (result == false) return;
			}
			catch { return; }

			Tejun_save = true;
		}

		/// <summary>保存、確定前の手順の簡易チェック</summary>
		private bool TejunCheckOK() {

			// データグリッドビューのセルがコミットされていない場合
			if (!DgvCommit) {
				MessageBox.Show(
					"手順データが編集中です。リターンor矢印キーを用いて編集を確定するか、" +
					"Escキーで編集をキャンセルさせてから実行してください。");
				return false;
			}

			// 手順名のチェック
			if (TNameCheckOK() == false)
				return false;

			if (toolStrip_user.Text.Length != 5) {
				MessageBox.Show("作成者番号（工番）の文字数が５ではない");
				return false;
			}
			for (int ii = 0; ii < toolStrip_user.Text.Length; ii++)
				if (Char.IsDigit(toolStrip_user.Text, ii) == false) {
					MessageBox.Show("作成者番号（工番）の文字に数字ではない文字が含まれている");
					return false;
				}
			if (toolStrip_seba.Text.Length != 6) {
				MessageBox.Show("製造番号の文字数が６ではない");
				return false;
			}
			try { Convert.ToDateTime(toolStrip_kdate.Text); }
			catch {
				MessageBox.Show("加工予定日の日付が不正");
				return false;
			}

			foreach (DataGridViewRow row in dataGridView1.Rows) {
				if (row.Cells[0].Value == null) continue;
				if (row.Cells[1].Value == null) continue;
				if (((string)row.Cells[0].Value).IndexOf("T") != 0) continue;
				if (((string)row.Cells[1].Value).Length > maxTSHEET_LENGTH) {
					MessageBox.Show("工具表名の文字数が" + maxTSHEET_LENGTH.ToString() + "を超える。");
					return false;
				}
				break;
			}
			return true;
		}
		/// <summary>手順名のチェック</summary>
		private bool TNameCheckOK() {
			if (toolStrip_Tname.Text.Length < 3) {
				MessageBox.Show("手順名の文字数が３未満である。");
				return false;
			}
			if (toolStrip_Tname.Text.Length > maxPSHEET_LENGTH) {
				MessageBox.Show("手順名の文字数が" + maxPSHEET_LENGTH.ToString() + "を超える。");
				return false;
			}
			for (int ii = 0; ii < 3; ii++)
				if (Char.IsUpper(toolStrip_Tname.Text[ii]) == false) {
					MessageBox.Show("手順名の先頭３文字の中に大文字ではない文字が含まれている。");
					return false;
				}
			return true;
		}

		private void CreateDirectory(string fldName) {
			//string fldName = ServerPC.SvrFldrN + outn.Substring(1, 2).ToUpper() + "\\";
			string outn = fldName.Substring(fldName.Length - 2);
			if (Directory.Exists(fldName))
				return;

			int imoji, imon0, imon1;
			imoji = StringCAM.ABC0.IndexOf(outn[0]) / 2;
			imon0 = DateTime.Now.Month - 4;
			imon1 = DateTime.Now.Month - 5;
			if (imon0 < 0) imon0 += 12;
			if (imon1 < 0) imon0 += 12;

			if (imoji < 12) {
				// とりあえずすべてメッセージのみとする
				//if (imoji == imon0 || imoji == imon1) {
					DialogResult result = MessageBox.Show(
						"新たな金型のためのフォルダー '" + outn + "' を作成します。",
						"フォルダー作成",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
				//}
				//else {
				//	DialogResult result = MessageBox.Show(
				//		"手順データ名の付け方の規則に従っていません。" +
				//		"フォルダー '" + outn + "' を作成しますか？",
				//		"フォルダー作成",
				//		MessageBoxButtons.OKCancel,
				//		MessageBoxIcon.Warning);
				//	if (result == DialogResult.Cancel)
				//		throw new Exception();
				//}
			}
			Directory.CreateDirectory(fldName);
			return;
		}

		/// <summary>
		/// 工具単位で回転数、送り速度を任意値に設定する
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStrip_SFSet_Click(object sender, EventArgs e) {
			if (NcdTool.Tejun.NcList.NcData.Count == 0)
				return;
			Form frm = new FormSFSet(NcdTool.Tejun.NcList.NcData);
			DialogResult result = frm.ShowDialog();
			if (result == DialogResult.OK)
				CommittoolStripButton_Click(toolStrip_SFSet, new EventArgs());
		}

		/// <summary>
		/// 自動で初期設定する値を保存する（個人ごとに）
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStripMenuItem_Shokichi_Click(object sender, EventArgs e) {
			if (tjnNam.Nspd) {
				MessageBox.Show("NCSPEED検証後の手順では実行できません。オリジナルの手順を使用してください。");
				return;
			}
			if (!Tejun_commit) {
				MessageBox.Show("手順が確定していません。手順を確定してから実行してください。");
				return;
			}
			DialogResult result = MessageBox.Show(
				$"手順名が '{toolStrip_Tname.Text[0]}' で始まる手順を新規作成する時の初期値をこの手順のデータで設定します。",
				"個人単位のデータ　作成者番号、加工機名、次元(D)、材質名(Z)、工具表名(T) の初期値を設定",
				MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
			if (result != DialogResult.OK) return;

			using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ServerPC.SvrFldrC + "init_tejun\\tejun_" + toolStrip_Tname.Text[0] + ".ini", false)) {
				string[] slist;
				bool I = true, M = true, D = true, Z = true, T = true, N = true;
				foreach (string stmp in ToListString()) {
					slist = stmp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (slist.Length == 0) continue;
					switch (slist[0]) {
					case "I": if (I && slist.Length > 1) sw.WriteLine("usr : " + slist[1]); I = false; break;
					case "M": if (M && slist.Length > 1) sw.WriteLine("mch : " + slist[1]); M = false; break;
					case "D": if (D && slist.Length > 1) sw.WriteLine("dim : " + slist[1]); D = false; break;
					case "Z": if (Z && slist.Length > 1) sw.WriteLine("mat : " + slist[1]); Z = false; break;
					case "T": if (T && slist.Length > 1) sw.WriteLine("tst : " + slist[1]); T = false; break;
					case "N": N = false; break;
					}
					if (N == false) break;
				}
			}
			MessageBox.Show($"'{toolStrip_Tname.Text[0]}' で始まる手順を新規作成する時の初期値を設定しました。");
		}






		// ///////////////////////////////
		// 以下はデータグリッドビュー関連
		// ///////////////////////////////

		private void SetView() {
			DataGridViewCellStyle style =
				dataGridView1.ColumnHeadersDefaultCellStyle;
			style.BackColor = Color.Navy;
			style.ForeColor = Color.White;
			style.Font = new Font(dataGridView1.Font, FontStyle.Bold);

			dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;
			dataGridView1.Name = "dataGridView1";
			dataGridView1.Location = new Point(8, 8);
			dataGridView1.Size = new Size(500, 300);
			dataGridView1.AutoSizeRowsMode =
				DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
			dataGridView1.ColumnHeadersBorderStyle =
				DataGridViewHeaderBorderStyle.Raised;
			dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
			dataGridView1.GridColor = SystemColors.ActiveBorder;
			dataGridView1.RowHeadersVisible = true;

			dataGridView1.AllowUserToOrderColumns = false;

			// Column
			dataGridView1.ColumnCount = 6;
			dataGridView1.Columns[0].Name = "Key Word";
			dataGridView1.Columns[0].DefaultCellStyle.Alignment =
				DataGridViewContentAlignment.MiddleCenter;
			dataGridView1.Columns[1].Name = "Name";
			dataGridView1.Columns[2].Name = "Data1";
			dataGridView1.Columns[3].Name = "Data2";
			dataGridView1.Columns[4].Name = "Data3";
			dataGridView1.Columns[5].Name = "Data4";
			// ソートの禁止
			for (int ii = 0; ii < dataGridView1.ColumnCount; ii++)
				dataGridView1.Columns[ii].SortMode =
					DataGridViewColumnSortMode.NotSortable;

			// Make the font italic for row four.
			dataGridView1.Columns[4].DefaultCellStyle.Font =
				new Font(DataGridView.DefaultFont, FontStyle.Italic);

			dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dataGridView1.MultiSelect = true;

			dataGridView1.BackgroundColor = Color.Honeydew;
			dataGridView1.Dock = DockStyle.Fill;

			// /////////////////////////////
			// 以下は試しに2007/10/11 に追加
			// /////////////////////////////
			dataGridView1.AllowUserToDeleteRows = true;
			// 行ヘッダーの幅
			dataGridView1.RowHeadersWidth = 30;
			dataGridView1.AllowDrop = true;
			dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;

			//dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView1_CellFormatting);
			//dataGridView1.CellParsing += new DataGridViewCellParsingEventHandler(dataGridView1_CellParsing);
			//addNewRowButton.Click += new EventHandler(addNewRowButton_Click);
			//deleteRowButton.Click += new EventHandler(deleteRowButton_Click);
			//ledgerStyleButton.Click += new EventHandler(ledgerStyleButton_Click);
			//dataGridView1.CellValidating += new DataGridViewCellValidatingEventHandler(dataGridView1_CellValidating);
		}

		/// <summary>
		/// 加工手順のテキスト情報をデータグリッドビューに挿入する
		/// </summary>
		private void TejunRead(StreamReader fpt)
		{
			string[] aa;
			List<string> txtCheck = new List<string>();
			DataGridViewRowCollection rows = this.dataGridView1.Rows;

			toolStrip_kdate.Text = "";

			string ddat;
			int count = 0;
			string stmp;
			while (fpt.EndOfStream != true) {
				ddat = fpt.ReadLine();
				string[] ff = ddat.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (ff.Length > 0) {
					switch (ff[0]) {
					case "I":
						toolStrip_user.Text = ff[1];
						count++;
						break;
					case "B":
						toolStrip_seba.Text = ff[1];
						count++;
						break;
					case "M":
						toolStrip_machn.Text = ff[1];
						count++;
						break;
					case "K":
						toolStrip_kdate.Text = ff[1];
						count++;
						break;
					default:
						aa = new string[ff.Length];
						stmp = "";
						for (int ii = 0; ii < ff.Length; ii++) {
							aa[ii] = ff[ii];
							stmp += aa[ii] + " ";
						}
						rows.Add(aa);
						txtCheck.Add(stmp.Substring(0, stmp.Length - 1));
						break;
					}
				}
			}
			fpt.Close();

			if (toolStrip_kdate.Text.Length == 0)
				throw new Exception("jedbqehdb");

			//File.Delete(tmpFName);
			txtCheck.Insert(0, "I " + toolStrip_user.Text);
			txtCheck.Insert(1, "B " + toolStrip_seba.Text);
			txtCheck.Insert(2, "M " + toolStrip_machn.Text);
		}

		/// <summary>
		/// データグリッドビューの内容をテキストリストにして返す
		/// </summary>
		public List<string> ToListString() {
			StringBuilder qqq = new StringBuilder();
			int maxClm;
			List<string> TejunText = new List<string>();
			TejunText.Clear();

			TejunText.Add("I " + toolStrip_user.Text);
			TejunText.Add("B " + toolStrip_seba.Text);
			TejunText.Add("M " + toolStrip_machn.Text);
			TejunText.Add("K " + toolStrip_kdate.Text);

			foreach (DataGridViewRow row in dataGridView1.Rows) {
				if (row.Cells[0].Value == null)
					continue;
				if (((string)row.Cells[0].Value).Length == 0)
					continue;
				//if (((string)row.Cells[0].Value).Length != 1)
				//	throw new Exception("キーワード＝" + (string)row.Cells[0].Value + "は無効です。修正してください。");

				if (qqq.Length != 0) qqq.Remove(0, qqq.Length);
				qqq.Append(((string)row.Cells[0].Value).Substring(0, 1));
				maxClm = 0;
				for (int ii = 1; ii < dataGridView1.ColumnCount; ii++) {
					if (row.Cells[ii].Value == null)
						break;
					if (((string)row.Cells[ii].Value).Length == 0)
						break;
					maxClm = ii;
					qqq.Append(" " + (string)row.Cells[ii].Value);
				}
				if (maxClm == 0)
					if ((string)row.Cells[0].Value != "E" && (string)row.Cells[0].Value != "C")
						throw new Exception("手順キーワード＝'" + (string)row.Cells[0].Value + "'にデータがありません。");
				TejunText.Add(qqq.ToString());
			}
			return TejunText;
		}

		// ///////////////////////////////////
		// データグリッドビューのイベント処理
		// ///////////////////////////////////

		/// <summary>ＮＣデータを検索して結果をビューに挿入する</summary>
		private void ToolStripButton_srchNC_Click(object sender, EventArgs e) {
			int cnt = dataGridView1.RowCount;
			FormTejunSelNc aaa = new FormTejunSelNc(dataGridView1);
			aaa.ShowDialog();
			if (dataGridView1.RowCount > cnt) {
				Tejun_save = Tejun_commit = false;
			}
		}

		// 倒れ式など

		/// <summary>倒れ式の値をＮＣデータのオフセット量から計算し工具軸補正として入力する</summary>
		private void ToolStrip_HoseiL_Click(object sender, EventArgs e) {
			InsertTaore("L");		// 倒れ補正
			Sumitori("L");			// 隅取りペンシル補正。藤井さんの改善要望により追加 in 2018/02/20
			Tejun_save = Tejun_commit = false;
		}
		/// <summary>倒れ式の値をＮＣデータのオフセット量から計算しＺ軸移動量として入力する</summary>
		private void ToolStrip_HoseiZ_Click(object sender, EventArgs e) {
			InsertTaore("Z");		// 倒れ補正
			Sumitori("Z");			// 隅取りペンシル補正。藤井さんの改善要望により追加 in 2018/02/20
			Tejun_save = Tejun_commit = false;
		}
		/// <summary>送り速度減速の設定をする</summary>
		private void ToolStrip_PUPZ_Click(object sender, EventArgs e) {
			// 藤井さんの改善要望により追加 in 2018/02/20
			List<string> rList = this.ToListString();
			rList.Remove(rList.Find(ss => ss.IndexOf("I ") == 0));
			rList.Remove(rList.Find(ss => ss.IndexOf("B ") == 0));
			rList.Remove(rList.Find(ss => ss.IndexOf("M ") == 0));
			rList.Remove(rList.Find(ss => ss.IndexOf("K ") == 0));
			int shoki = rList.Count;

			// rListに５０％の減速コード（F 0.5）を追加
			Gensoku50(rList);

			// 変更がある場合は更新する
			if (rList.Count > shoki) {
				dataGridView1.Rows.Clear();
				for (int ii = 0; ii < rList.Count; ii++) dataGridView1.Rows.Add(rList[ii].Split(new char[] { ' ' }));
				Tejun_save = Tejun_commit = false;
			}
		}
		/// <summary>倒れの補正</summary>
		private void InsertTaore(string code) {
			if (ProgVersion.NotTrialVersion2) {
				double zz;
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NotDummy) {
					//if ((zz = ncnam.Tdat[0].Taore()) == 0.0) continue;
					if ((zz = ncnam.Tdat.Max(skog => skog.Taore())) == 0.0) continue;
					if (ncnam.Itdat != 1) throw new Exception("複数工具のＮＣデータには倒れ補正をセットできません。");
					foreach (DataGridViewRow row in dataGridView1.Rows) {
						if (row.Cells[0].Value == null) continue;
						if (row.Cells[1].Value == null) continue;
						if (((string)row.Cells[0].Value).IndexOf("N") != 0) continue;
						if ((string)row.Cells[1].Value != ncnam.nnam) continue;
						row.Cells[2].Value = code + zz.ToString("0.00#");
						row.Cells[3].Value = code + "0.02";     // 藤井さんの改善要望により追加 in 2018/02/20
					}
				}
			}
			else {
				double? zz;
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NotDummy) {
					if ((zz = ncnam.Tdat.Max(skog => skog.Taore())) == 0.0) continue;
					if (ncnam.Itdat > 1) {
						if (ncnam.Ncdata.ncInfo.xmlD.BaseNcFormat.Id != CamUtil.BaseNcForm.BUHIN.Id)
							throw new Exception("部品加工以外では複数工具での倒れ補正の処理はできません。");
						zz = null;
					}
					foreach (DataGridViewRow row in dataGridView1.Rows) {
						if (row.Cells[0].Value == null) continue;
						if (row.Cells[1].Value == null) continue;
						if (((string)row.Cells[0].Value).IndexOf("N") != 0) continue;
						if ((string)row.Cells[1].Value != ncnam.nnam) continue;
						row.Cells[2].Value = code + (zz.HasValue ? zz.Value.ToString("0.00#") : " AUTO");
						row.Cells[3].Value = code + "0.02";     // 藤井さんの改善要望により追加 in 2018/02/20
					}
				}
			}
		}
		/// <summary>隅取りペンシル加工の補正</summary>
		private void Sumitori(string code) {
			double zido;
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NotDummy) {
				if ((zido = ncnam.SumiPencil()) == 0.0) continue;
				// 対象のＮＣデータの行を探し、Ｚ移動量を追加する
				foreach (DataGridViewRow row in dataGridView1.Rows) {
					if (row.Cells[0].Value == null) continue;
					if (row.Cells[1].Value == null) continue;
					if (((string)row.Cells[0].Value).IndexOf("N") != 0) continue;
					if ((string)row.Cells[1].Value != ncnam.nnam) continue;
					if (row.Cells[2].Value != null) { System.Windows.Forms.MessageBox.Show(ncnam.nnam + " はすでに移動データが入力済です。"); continue; }
					row.Cells[2].Value = String.Format("{0}{1:0.00#}", code, zido);
				}
			}
		}
		/// <summary>送り速度減速の設定をする</summary>
		private void Gensoku50(List<string> rList) {
			string[] ff;
			char[] splt = new char[] { ' ' };
			int FCHI;
			bool pupzNow, pupzPre;

			pupzPre = false;	// 前の行の"PU", "PZ" の設定有無
			FCHI = 100;			// 元の手順の送り速度比率の設定値
			for (int ii = 0; ii < rList.Count; ii++) {
				ff = rList[ii].Split(splt);
				if (ff[0] == "F") {
					FCHI = (int)Math.Round(Convert.ToDouble(ff[1]) * 100.0);
					if (FCHI != 50) pupzPre = false;
				}
				if (ff[0] != "N") continue;

				// ポストのコメントが"PU", "PZ" のＮＣデータであれば pupzNow = true とする
				pupzNow = false;
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NotDummy) {
					if (ncnam.nnam != ff[1]) continue;
					pupzNow = ncnam.PUPZ();
					break;
				}

				// 送り速度比率を挿入する（中間行）
				if (pupzPre != pupzNow && FCHI != 50)
					rList.Insert(ii, (pupzNow ? "F 0.5" : "F " + (FCHI / 100.0).ToString("0.0#")));
				pupzPre = pupzNow;
			}

			// 送り速度比率を挿入する（最終行）
			pupzNow = false;
			if (pupzPre != pupzNow && FCHI != 50)
				rList.Add(pupzNow ? "F 0.5" : "F " + (FCHI / 100.0).ToString("0.0#"));
		}


		/// <summary>カーソルのある行の上に行を追加する</summary>
		private void ToolStripButton_Insert_Click(object sender, EventArgs e) {
			//dataGridView1.Rows.Insert(dataGridView1.SelectedRows[0].Index, 1);
			int ind = -1;
			bool eq;
			for (int ii = dataGridView1.Rows.Count - 1; ii >= 0; ii--) {
				eq = false;
				foreach (DataGridViewRow dRow in dataGridView1.SelectedRows) {
					if (dRow == dataGridView1.Rows[ii]) {
						eq = true;
						if (ind < 0) ind = ii;
						break;
					}
				}
				if (ind >= 0 && eq == false) {
					dataGridView1.Rows.Insert(ii + 1, ind - ii);
					ind = -1;
					Tejun_save = Tejun_commit = false;
				}
			}
		}

		/// <summary>カーソルのある行を削除する</summary>
		private void ToolStripButton_Remove_Click(object sender, EventArgs e) {
			//if (dataGridView1.SelectedRows[0].IsNewRow) return;
			//dataGridView1.Rows..Remove(dataGridView1.SelectedRows[0]);
			foreach (DataGridViewRow dRow in dataGridView1.SelectedRows)
				if (!dRow.IsNewRow) dataGridView1.Rows.Remove(dRow);
			Tejun_save = Tejun_commit = false;
		}

		/// <summary>カーソルのある行をひとつ上の行に移動する</summary>
		private void ToolStripButton_Up_Click(object sender, EventArgs e) {
			//DataGridViewRow aa = dataGridView1.SelectedRows[0];
			DataGridViewRow ind = null;
			bool eq;
			for (int ii = 0; ii < dataGridView1.Rows.Count; ii++) {
				eq = false;
				foreach (DataGridViewRow dRow in dataGridView1.SelectedRows) {
					if (dRow == dataGridView1.Rows[ii]) {
						eq = true;
						if (ii == 0)
							return;
						if (dataGridView1.Rows[ii].IsNewRow)
							return;
						if (ind == null)
							ind = dataGridView1.Rows[ii - 1];
						break;
					}
				}
				if (ind != null && eq == false) {
					dataGridView1.Rows.Remove(ind);
					dataGridView1.Rows.Insert(ii - 1, ind);
					ind = null;
					Tejun_save = Tejun_commit = false;
				}
			}
		}

		/// <summary>カーソルのある行をひとつ下の行に移動する</summary>
		private void ToolStripButton_Down_Click(object sender, EventArgs e) {
			//DataGridViewRow aa = dataGridView1.SelectedRows[0];
			DataGridViewRow ind = null;
			bool eq;
			for (int ii = dataGridView1.Rows.Count - 1; ii >= 0; ii--) {
				eq = false;
				foreach (DataGridViewRow dRow in dataGridView1.SelectedRows) {
					if (dRow == dataGridView1.Rows[ii]) {
						eq = true;
						if (ii == dataGridView1.Rows.Count - 1)
							return;
						if (dataGridView1.Rows[ii + 1].IsNewRow)
							return;
						if (ind == null)
							ind = dataGridView1.Rows[ii + 1];
						break;
					}
				}
				if (ind != null && eq == false) {
					dataGridView1.Rows.Remove(ind);
					dataGridView1.Rows.Insert(ii + 1, ind);
					ind = null;
					Tejun_save = Tejun_commit = false;
				}
			}
		}

		// セルの値が変更された時
		private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e) {
			Tejun_save = Tejun_commit = false;
		}

		private void コピーCToolStripButton1_Click(object sender, EventArgs e) {
			string ss = "";
			for (int ii = 0; ii < dataGridView1.Rows.Count; ii++)
				foreach (DataGridViewRow dRow in dataGridView1.SelectedRows)
					if (dRow.Index == ii) {
						if (ss != "") ss += "\r\n";
						foreach (DataGridViewCell cell in dRow.Cells)
							ss += "\t" + cell.Value;
					}
			Clipboard.SetText(ss);
		}

		private void 貼り付けPToolStripButton1_Click(object sender, EventArgs e) {
			if (dataGridView1.SelectedRows.Count == 0) return;

			string[] ss1, ss2;
			int jj = 0;
			ss1 = Clipboard.GetText().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			// クリップボードの値がデータグリッドビューの行ではない場合は中止する
			if (ss1.Length == 0) return;
			if (ss1[0].Split(new char[] { '\t' }, StringSplitOptions.None).Length != 7) return;

			// クリップボードの行数と選択行の行数のチェック（選択行内の新しいレコードの行はカウントしない）
			int cnt = 0; foreach (DataGridViewRow dRow in dataGridView1.SelectedRows) if (dRow.IsNewRow == false) cnt++;
			if (dataGridView1.SelectedRows.Count > 1 && cnt != ss1.Length) {
				MessageBox.Show("選択された行数が１でなく、またクリップボード内の行数" + ss1.Length + "とも異なるため貼り付けできません。");
				return;
			}

			if (dataGridView1.SelectedRows.Count == 1) {
				// １行のみ選択の場合
				DataGridViewRow dRow = dataGridView1.SelectedRows[0];
				if (dRow.IsNewRow) {
					while (jj < ss1.Length) {
						ss2 = ss1[jj].Split(new char[] { '\t' }, StringSplitOptions.None);
						dataGridView1.Rows.Add(new string[] { ss2[1], ss2[2], ss2[3], ss2[4], ss2[5], ss2[6] });
						jj++;
					}
				}
				else {
					while (jj < ss1.Length) {
						ss2 = ss1[jj].Split(new char[] { '\t' }, StringSplitOptions.None);
						dataGridView1.Rows.Insert(dRow.Index, new string[] { ss2[1], ss2[2], ss2[3], ss2[4], ss2[5], ss2[6] });
						jj++;
					}
					dataGridView1.Rows.Remove(dRow);
				}
			}
			else {
				// 複数行選択の場合
				foreach (DataGridViewRow dRowAll in  dataGridView1.Rows) {
					foreach (DataGridViewRow dRow in dataGridView1.SelectedRows) {
						if (dRow == dRowAll) {
							ss2 = ss1[jj].Split(new char[] { '\t' }, StringSplitOptions.None);
							dataGridView1.Rows.Insert(dRow.Index, new string[] { ss2[1], ss2[2], ss2[3], ss2[4], ss2[5], ss2[6] });
							dataGridView1.Rows.Remove(dRow);
							jj++;
							break;
						}
					}
					if (jj >= ss1.Length) break;
				}
			}
		}
	}
}