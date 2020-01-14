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

namespace NcTejun
{
	/// <summary>
	/// 
	/// </summary>
	partial class Form1 : Form
	{
		/// <summary>ＰＣの手順の場合、その手順データのあるフォルダー名</summary>
		public string TjnDir { get { return frmTejun?.TjnNam_folder; } }

		/// <summary>手順ファイル使用履歴</summary>
		private List<string> tejunNames = new List<string>();

		/// <summary>加工手順の表示フォーム</summary>
		private TejunSet.FormTejun frmTejun;

		/// <summary>工具表一覧</summary>
		internal TejunSet.ToolSheet[] TsheetList { get { return m_tsheetList.ToArray(); } }
		private List<TejunSet.ToolSheet> m_tsheetList = new List<TejunSet.ToolSheet>();
		/// <summary>工具表の作成</summary>
		public void MakeToolTable() {
			TsheetClear();
			int ii = 0;
			foreach (string tset in NcdTool.Tejun.NcList.TsNameList) {
				// 同一名の工具表がない場合に新規工具表を作成する
				if (m_tsheetList.Exists(ts => ts.TolstName == tset) == false) {
					m_tsheetList.Add(new TejunSet.ToolSheet(tset, ii));
					ii++;
				}
			}
		}
		/// <summary>工具表のクリア</summary>
		internal void TsheetClear() { foreach (TejunSet.ToolSheet ts in m_tsheetList) ts.CloseAtOnse(); m_tsheetList.Clear(); }
		/// <summary>工具表のクリア</summary>
		/// <param name="ts"></param>
		internal void TsheetClear(TejunSet.ToolSheet ts) { ts.CloseAtOnse(); m_tsheetList.Remove(ts); }

		///// <summary>ヘルプの表示フォーム</summary>
		//private FormHelp frmHelp;

		/// <summary>
		/// 
		/// </summary>
		public Form1() {

			// スプラッシュの表示
			FormSplash frmSplash = new FormSplash() {
				StartPosition = FormStartPosition.CenterScreen
			};
			frmSplash.Show();

			InitializeComponent();
			this.IsMdiContainer = true;

			// テンポラリー
			string ddat;

			// フォームサイズの設定
			this.Size = new Size(900, 650);
			this.StartPosition = FormStartPosition.CenterScreen;

			// ////////////////////////////////////////////
			// チェック
			//FormToolSheet aaa = new FormToolSheet("ts1");
			//aaa.Show();
			//return;
			// ////////////////////////////////////////////

			// ローカルホスト名の取得
			// ローカルＩＰアドレスの取得
			// 初期検索フォルダーの設定
			Application.DoEvents();
			LocalHost.LocalHostSet();
			Application.DoEvents();

			//MessageBox.Show(
			//	"すべてマクロ展開する"
			//	, "暫定処置");

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

			// イニシャライズファイルの読み込み
			if (File.Exists(LocalHost.Homedir + "NcTejun.ini")) {
				using (StreamReader sr = new StreamReader(LocalHost.Homedir + "NcTejun.ini")) {
					while (sr.EndOfStream != true) {
						ddat = sr.ReadLine();
						if (ddat == null)
							continue;
						string[] ff = ddat.Split(new char[] { '=', ',' },StringSplitOptions.RemoveEmptyEntries);
						if (ff[0].Trim() == "tejunNames")
							for (int ii = 1; ii < ff.Length; ii++)
								tejunNames.Add(ff[ii]);
					}
				}
			}

			// ＳＱＬサーバー情報（加工情報）の取得
			NcdTool.ToolSetInfo.DataSet();
			// ＳＱＬサーバー情報（カサブランカ情報）の取得
			CasaData.DataSet();

			// テスト
			/*
			{
				CamUtil.Angle3 abc = new Angle3(CamUtil.Angle3.Unit.DEGREE, 95, -90, 12.23);
				CamUtil.Angle3 ax;
				NcRead.Rot_Axis aa;
				aa = new NcTejun.NcRead.Rot_Axis(true, abc);
				ax = aa.DMU200P();
				aa = new NcTejun.NcRead.Rot_Axis(abc);
				ax = aa.DMU200P();
			}
			*/

			frmSplash.Close();
		}
		/// <summary>
		/// 手順のフォームが閉じられたとき
		/// </summary>
		public void TejunClose() {
			OpenCreate(true);
		}
		/// <summary>
		/// 工具表のフォームが閉じられたとき
		/// </summary>
		public void TsheetClose(TejunSet.ToolSheet ts) {
			frmTejun.Tejun_commit = false;
		}

		/// <summary>
		/// フォーム表示直後に手順データをインポートする（中止 in 2012/09/06）
		/// </summary>
		private void Form1_Shown(object sender, EventArgs e) {
			//InportToolStrip_Click(sender, e);
			OpenCreate(true);
		}




		/// <summary>
		/// 新規作成（メニュー、ボタン）
		/// </summary>
		private void NewToolStrip_Click(object sender, EventArgs e)
		{
			// 手順データを表示するフォームの作成
			frmTejun = new TejunSet.FormTejun();
			Application.DoEvents();
			frmTejun.MdiParent = this;
			frmTejun.Show();
			Application.DoEvents();
			// 新手順クラスの作成と表示は、手順フォームにおいて修正後
			// 「確定」処理実行時に実施する。

			OpenCreate(false);
		}
		/// <summary>
		/// 手順データをインポートする（メニュー、アイコンボタン）
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void InportToolStrip_Click(object sender, EventArgs e) {
		}

		/// <summary>手順データ選択（メニュー、アイコンボタン）</summary>
		private void OpenToolStrip_Click(object sender, EventArgs e) {
			StringBuilder fulTejunName = new StringBuilder();
			// 独自のダイアログを使用
			TejunSet.FormSelectFolder frmSFolder = new TejunSet.FormSelectFolder(fulTejunName);
			DialogResult result = frmSFolder.ShowDialog();
			if (result != DialogResult.OK) return;

			ReadTejun(fulTejunName.ToString());
			return;
		}
		/// <summary>フォームのドラッグ＆ドロップ</summary>
		private void Form1_DragEnter(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;
		}
		/// <summary>フォームのドラッグ＆ドロップ</summary>
		private void Form1_DragDrop(object sender, DragEventArgs e) {
			DataObject dObject = new DataObject(DataFormats.FileDrop, e.Data.GetData(DataFormats.FileDrop));
			string path;
			if (dObject.GetFileDropList().Count == 1) {
				path = dObject.GetFileDropList()[0];
				if (Directory.Exists(path)) {
					if (path.IndexOf(ServerPC.EDT) >= 0) {
						ReadTejun(Path.GetFullPath(path + "\\Tejun"));
						return;
					}
					if (path.IndexOf(ServerPC.ORG) >= 0) {
						ReadTejun(Path.GetFullPath(path + "\\TejunBase"));
						return;
					}
				}
			}
		}
		/// <summary>手順データを開く</summary>
		private void ReadTejun(string fulName) {
			// 加工手順書の読込み
			//Program.tejun = null;

			// 手順データを表示するフォームの作成と新手順クラスの作成
			Application.DoEvents();
			try {
				frmTejun = new TejunSet.FormTejun(fulName);
				Application.DoEvents();
				frmTejun.MdiParent = this;
				frmTejun.Show();
				Application.DoEvents();
			}
			catch {
				OpenCreate(true);
				return;
			}
			OpenCreate(false);
		}



		/// <summary>
		/// 手順データを上書き保存する
		/// </summary>
		private void 上書き保存SToolStripButton_Click(object sender, EventArgs e) {
			上書き保存SToolStripMenuItem_Click(sender, e);
		}
		private void 上書き保存SToolStripMenuItem_Click(object sender, EventArgs e) {
			;
		}

		/// <summary>
		/// 加工手順の表示
		/// </summary>
		private void 加工手順ToolStripMenuItem_Click(object sender, EventArgs e) {
			Application.DoEvents();
			// ワーニングメッセージの表示
			LogOut.Warning("TejunGet");

			// 加工手順書の表示
			// テキスト表示フォームの作成
			TejunSet.FormTjnText frmText = new TejunSet.FormTjnText() {
				Text = $"手順：{NcdTool.Tejun.TejunName}　加工機：{NcdTool.Tejun.Mach.name}",
				Size = new Size(700, 400)
			};
			// 実行
			Output.TejunSheetDB ts = new Output.TejunSheetDB();
			frmText.richTextBox1.Text = ts.Hyoji().ToString();
			frmText.ShowDialog();
			Application.DoEvents();
		}

		/// <summary>
		/// ツールシートの表示
		/// </summary>
		private void ツールシートToolStripMenuItem_Click(object sender, EventArgs e) {
			foreach (TejunSet.ToolSheet tsheet in this.TsheetList) {
				// テキスト表示フォームの作成
				System.Text.StringBuilder Text1 = new StringBuilder();
				//TejunXqt.optxqt(tset, Text1, 'T');
				{
					if (tsheet.NcNamsCount == 0)
						continue;
					LogOut.Warning("TejunGet");
					Output.ToolSheetDB tsdList = new Output.ToolSheetDB(tsheet, true);
					Text1 = tsdList.Tool_out_dsp();
				}
				Application.DoEvents();
				FormMessageBox.Show("工具表:" + tsheet.TolstName, Text1.ToString());
			}
			Application.DoEvents();
		}

		private void ヘルプLToolStripButton_Click(object sender, EventArgs e) {
			//frmHelp = new FormHelp();
			//frmHelp.Show();
		}

		/// <summary>
		/// NcTejunの終了処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			// 手順名リストの保管
			using (StreamWriter sw = new StreamWriter(LocalHost.Homedir + "NcTejun.ini", false)) {
				sw.WriteLine("[INIT]");
				sw.Write("tejunNames=");
				for (int jj = 0; jj < tejunNames.Count; jj++) {
					if (jj == 12)
						break;
					if (jj != 0)
						sw.Write(",");
					sw.Write(tejunNames[jj]);
				}
				sw.WriteLine();
			}

			LogOut.CheckCountOutput();
		}

		private bool CheckEurekaOutput(string pname) {

			// 手順書が確定しているかチェックする
			if (TejunCommitCheck() == false)
				return true;

			if (NcdTool.Tejun.BaseNcForm.Id != BaseNcForm.ID.GENERAL) {
				MessageBox.Show(
					"ＧＥＮＥＲＡＬのＮＣデータではありません。" + pname + "実行できません。",
					pname,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return true;
			}

			// ＸＭＬ、ＮＣデータの存在チェック
			foreach (NcdTool.NcName.NcData ncD in NcdTool.Tejun.NcList.NcData) {
				if (File.Exists(ncD.fulnamePC) == false) {
					MessageBox.Show(
						$"{ncD.nnam} にＰＣ内ＮＣデータが存在しません。{pname}実行できません。",
						pname,
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return true;
				}
			}
			// ＮＣデータの指定チェック
			if (NcdTool.Tejun.NcList.NcNamsAll.Count == 0) {
				MessageBox.Show(
					$"ＮＣデータが１つも指定されていません。{pname}実行できません。",
					pname,
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return true;
			}
			// ＮＣデータの工具長補正がないか確認 2013/08/22
			foreach (NcdTool.NcName.NcNam ncd in NcdTool.Tejun.NcList.NcNamsAll) {
				if (ncd.nggt.ToolLengthHosei.Zero != true) {
					string lHosei = ncd.nggt.ToolLengthHosei.Auto ? "AUTO" : ncd.nggt.ToolLengthHosei.ValueHosei().ToString("0.0##");
					MessageBox.Show(
						$"{ncd.nnam} は工具長補正量（L={lHosei}）が設定されています。{pname}実行できません。",
						pname,
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Eureka計算用情報の出力
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStripEureka_Click(object sender, EventArgs e) {

			// 実行不可
			if (CheckEurekaOutput("シミュレーション")) return;

			// プログラムのバージョンのチェック
			if (CamUtil.ProgVersion.CheckVersion(null)) return;

			// 出力用フォルダーの作成
			//string dirnO = ServerPC.SvrFldrSorg + Program.tejun.tejunName.Substring(1, 2) + "\\" + Program.tejun.tejunName;
			string dirnO = TjnDir;
			if (Directory.Exists(dirnO)) {
				DialogResult result0 = DialogResult.OK;
				if (File.Exists(dirnO + "\\Tejun")) {
					result0 = MessageBox.Show(
						"以前出力したシミュレーションデータが存在します。上書きしますか？",
						"CHECK NCSPEED OVERWRITE",
						MessageBoxButtons.OKCancel,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2);
				}
				if (result0 != DialogResult.OK)
					return;
				//Directory.Delete(dirnO, true);
				//Directory.CreateDirectory(dirnO);
				foreach (string fnam in Directory.GetFiles(dirnO, "*.*"))
					if (Path.GetFileName(fnam) != "TejunBase")
						File.Delete(fnam);
			}

			// //////////////////////////////////////////////
			// 現在の手順書の保存
			// //////////////////////////////////////////////
			frmTejun.上書き保存SToolStripButton_Click(this, EventArgs.Empty);

			// /////////////////////////////////
			// ＮＣＳＰＥＥＤ用データ変換の実行
			// /////////////////////////////////
			NcdTool.NcSIM.NcSimul ncsimul = new NcdTool.NcSIM.NcSimul();
			DialogResult result = ncsimul.NcSimulSet(frmTejun.ToListString(), dirnO);
			if (result != DialogResult.OK) return;

			// //////////////////////////////////////////////
			// 位置情報の変換とコピー
			// //////////////////////////////////////////////
			string srcName = ServerPC.SvrFldrS + "layout\\" + NcdTool.Tejun.TejunName;
			if (File.Exists(srcName)) {
				string[] split = null;
				using (StreamReader sr = new StreamReader(srcName)) {
					while (!sr.EndOfStream) {
						string stemp = sr.ReadLine();
						if (stemp.Substring(0, 5) != "0001\t") continue;
						split = stemp.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						break;
					}
				}

				if (split == null || split.Length != 4)
					throw new Exception("位置情報 " + srcName + " の読込みエラー");
				StreamWriter sw = new StreamWriter(dirnO + "\\" + NcdTool.Tejun.TejunName + ".layout");
				sw.WriteLine("{0:f3}\t {1:f3}\t {2:f3}\t", -Convert.ToDouble(split[1]), -Convert.ToDouble(split[2]), -Convert.ToDouble(split[3]));
				sw.Close();
			}

			// 終了メッセージ
			MessageBox.Show("シミュレーション用データ出力は正常に終了しました。工具数 = " + ncsimul.ncdCount.Count);
			OpenCreate(false);

			Application.DoEvents();
			LogOut.CheckCountOutput();
		}

		/// <summary>
		/// ＮＣデータの出力
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToolStrip_Texas_Click(object sender, EventArgs e) {

			// プログラムのバージョンのチェック
			if (CamUtil.ProgVersion.CheckVersion(null)) return;

			// 手順書が確定しているかチェックする
			if (TejunCommitCheck() == false)
				return;

			// 出力可能な設備名か確認する
			if (!NcdTool.Tejun.Mach.CheckOutput) {
				//if (!mcn1.MC_Name.CheckOutput(Program.tejun.mach.machn)) {
				MessageBox.Show(
					NcdTool.Tejun.Mach.name + "はＮＣデータ出力にまだ対応していない設備です。");
				//return;
			}

			// ＮＣデータの指定チェック
			if (NcdTool.Tejun.NcList.NcNamsAll.Count == 0) {
				MessageBox.Show(
					"ＮＣデータが１つも指定されていません。ＮＣデータの出力は実行できません。",
					"ＮＣ出力",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			// Ｏ番号のセットの確認
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NcExist)
				foreach (NcdTool.NcName.Kogu skog in ncnam.Tdat)
					for (int mm = 0; mm < skog.matchK.Length; mm++) {
						try { int ii = skog.matchK[mm].K2.Onum; }
						catch {
							MessageBox.Show(
								"Ｏ番号がセットされていません。", "ＮＣ出力",
								MessageBoxButtons.OK, MessageBoxIcon.Warning);
							return;
						}
					}
			// シミュレーションと加工機の整合
			if (NcdTool.Tejun.Mach.Dmu) {
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NcExist) {
					if (ncnam.Ncdata.ncInfo.xmlD.SmNAM == "EUREKA")
						if (ncnam.Ncdata.ncInfo.xmlD.SmMCN != NcdTool.Tejun.Mach.name)
							MessageBox.Show($"ＮＣデータ検証は{ncnam.Ncdata.ncInfo.xmlD.SmMCN}で実施されているが、データの出力先は{NcdTool.Tejun.Mach.name}である。");
					break;
				}
			}
			// マシンヘッドと加工機の整合
			try {
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NcExist)
					MachineHead.CheckMachine(ncnam.Ncdata.ncInfo.xmlD.MHEAD, ncnam.Ncdata.ncInfo.xmlD.PostProcessor, NcdTool.Tejun.Mach.name);
			}
			catch(Exception ex) {
					MessageBox.Show(ex.Message);
					return;
			}

			// 加工機とM04工具との整合 2018/08/28
			if (NcdTool.Tejun.Mach.ncCode.M04 == false) {
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NcExist)
					foreach (NcdTool.NcName.Kogu skog in ncnam.Tdat)
						if (skog.TsetCHK) if (skog.TsetCHG.Tset_m0304 == "M04") {
								MessageBox.Show("この設備はＭ０４加工に対応していません", "ＮＣ出力", MessageBoxButtons.OK, MessageBoxIcon.Warning);
								return;
							}
			}

			// ////////////////////////////
			// 工具表との関連付けをチェック
			// ////////////////////////////
			if (0 == 0) {
				int sheetNo = 0;
				int sheetYes = 0;
				string aaa = "";
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NotDummy) {
					if (ncnam.Itdat == 0) {
						sheetNo++;
						aaa += $"  {ncnam.nnam} null_tool\n";
					}
					foreach (NcdTool.NcName.Kogu skog in ncnam.Tdat) {
						//if (skog.tsetCHG.tset_name == null) continue;	// 元々、出力しないものは除く
						if (skog.TsetCHK == false) continue;	// 元々、出力しないものは除く
						if (skog.matchK.Length == 0) {
							sheetNo++;
							aaa += $"  {ncnam.nnam} {skog.TsetCHG.Tset_name}\n";
						}
						for (int mm = 0; mm < skog.matchK.Length; mm++) {
							if (skog.matchK[mm].K2.Tlgn == null) {
								sheetNo++;
								aaa += $"  {ncnam.nnam} {skog.TsetCHG.Tset_name}\n";
								continue;
							}
							if (skog.matchK[mm].K2.Tlgn.Tmod != '0') {
								sheetNo++;
								aaa += $"{skog.matchK[mm].K2.Tlgn.Tmod.ToString()} {ncnam.nnam} {skog.TsetCHG.Tset_name}\n";
								continue;
							}
							sheetYes++;
						}
					}
				}
				if (sheetYes == 0) {
					MessageBox.Show("debug : 出力可能なＮＣデータはありません。工具表を作成してください。", "Form1");
					return;
				}
				if (sheetNo > 0) {
					DialogResult result =
						MessageBox.Show("工具表に関連付けられていない以下のＮＣデータがあります。\n" + aaa, "Form1",
						MessageBoxButtons.OKCancel,
						MessageBoxIcon.Warning);
					if (result != DialogResult.OK)
						return;
				}
			}

			// ＮＣデータの工具長補正がないか確認 2013/08/22
			if (NcdTool.Tejun.Ncspeed) {
				int kosuu = 0;
				kosuu = NcdTool.Tejun.NcList.NcNamsAll.Count(ncd => ncd.nggt.ToolLengthHosei.Zero != true);
				if (kosuu > 0)
					MessageBox.Show(
						"工具長補正量が手順内で設定されたＮＣデータが存在します。",
						"工具長補正", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			// 倒れ補正量の整合性を確認 2015/10/08
			if (true) {
				int kosuu1 = 0, kosuu2 = 0;
				foreach (NcdTool.NcName.NcNam ncd in NcdTool.Tejun.NcList.NcNamsAll_NcExist) {
					foreach (NcdTool.NcName.Kogu skog in ncd.Tdat) {
						if (skog.Tld.XmlT.ClMv_Offset == 0.0) continue;
						if (skog.Tld.XmlT.ClMv_Offset + skog.Tld.XmlT.ClMv_Z_axis >= -0.0001) {
							if (ncd.nggt.ToolLengthHosei.Zero != true) kosuu1++;
							if (ncd.nggt.trns.Z != 0.0) kosuu1++;
						}
						if (skog.Tld.XmlT.ClMv_Offset + skog.Tld.XmlT.ClMv_Z_axis + ncd.nggt.ToolLengthHosei.ValueHosei(skog) + ncd.nggt.trns.Z < 0.0)
							kosuu2++;
					}
				}
				if (kosuu1 > 0)
					MessageBox.Show(
						"倒れ補正のためのＺ軸/工具軸補正が複数設定されたＮＣデータが存在します。",
						"倒れ式など", MessageBoxButtons.OK, MessageBoxIcon.Information);
				if (kosuu2 > 0)
					MessageBox.Show(
						"倒れ補正のためのＺ軸/工具軸補正が不足しているＮＣデータが存在します。",
						"倒れ式など", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			// 隅取り、ペンシル加工の整合性を確認 2018/02/28
			if (true) {
				int kosuu1 = 0;
				double zz;
				foreach (NcdTool.NcName.NcNam ncd in NcdTool.Tejun.NcList.NcNamsAll_NcExist) {
					if ((zz = ncd.SumiPencil()) > 0.0) {
						foreach(NcdTool.NcName.Kogu skog in ncd.Tdat) {
							if (ncd.nggt.ToolLengthHosei.ValueHosei(skog) < zz && ncd.nggt.trns.Z < zz) kosuu1++;
						}
					}
				}
				if (kosuu1 > 0)
					MessageBox.Show(
						"隅取りペンシル加工の補正をしていないＮＣデータが存在します。",
						"倒れ式など", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			// ポストコメントPU, PZの整合性を確認 2018/02/28
			if (true) {
				int kosuu1 = 0;
				foreach (NcdTool.NcName.NcNam ncd in NcdTool.Tejun.NcList.NcNamsAll_NcExist) {
					if (ncd.PUPZ())
						if (Math.Abs(ncd.nmgt.Fratt - 0.5) > 0.01) kosuu1++;
				}
				if (kosuu1 > 0)
					MessageBox.Show(
						"底面３Ｄ送り減速の設定をしていないＮＣデータが存在します。",
						"倒れ式など", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}

			Application.DoEvents();

			// /////////////////////
			// NcOutput の作成と実行
			// /////////////////////
			Output.FormNcSet ncset = null;
			try {
				switch (NcdTool.Tejun.BaseNcForm.Id) {
				case BaseNcForm.ID.BUHIN:
					ncset = new Output.FormNcSet_Buhin(NcdTool.Tejun.Mach);
					break;
				case BaseNcForm.ID.GENERAL:
					if (NcdTool.Tejun.Mach.Toool_nc)
						ncset = new Output.FormNcSet_Texas();		// ＮＣデータとindexファイルを出力する
					else
						ncset = new Output.FormNcSet_NonTexas();	// ＮＣデータのみ出力する
					break;
				default: throw new Exception("qjrefbqrebf");
				}
				ncset.ShowDialog();
				//NcTejun.Output.NcOutput_Ncd aa = new NcTejun.Output.NcOutput_Ncd();
				//aa.Output(this);
			}
			catch (Exception ex) {
				if (ncset != null) { ncset.Dispose(); ncset = null; }
				MessageBox.Show(ex.Message);
			}
			Application.DoEvents();
			LogOut.CheckCountOutput();
		}
		private bool TejunCommitCheck() {

			// tejunが作成されていない場合？
			if (NcdTool.Tejun.NcList.NcNamsAll.Count == 0) {
				MessageBox.Show("手順を作成してください");
				return false;
			}

			// 手順のデータグリッドビューのセルがコミットされていない場合
			if (!frmTejun.DgvCommit) {
				MessageBox.Show(
					"手順データが編集中です。リターンor矢印キーを用いて編集を確定するか、" +
					"Escキーで編集をキャンセルさせてから実行してください。");
				return false;
			}
			// 手順フォームが編集されている場合
			if (!frmTejun.Tejun_commit) {
				MessageBox.Show("手順を確定してから実行してください");
				return false;
			}

			// 工具表
			foreach (TejunSet.ToolSheet tolst in TsheetList) {
				// 工具表のデータグリッドビューのセルがコミットされていない場合
				if (!tolst.DgvCommit) {
					MessageBox.Show($"工具表{tolst.TolstName}が編集中です。上下左右の矢印キーを用いて編集を確定するか、Escキーで編集をキャンセルさせてから実行してください。");
					return false;
				}
				// 工具表フォームが編集されている場合
				if (!tolst.Tolst_commit) {
					MessageBox.Show("工具表" + tolst.TolstName + "を確定してから実行してください");
					return false;
				}
			}
			return true;
		}

		private void OpenCreate(bool enable) {
			新規作成NToolStripButton.Enabled = enable;
			新規作成NToolStripMenuItem.Enabled = enable;
			開くOToolStripMenuItem.Enabled = enable;
			開くOToolStripButton.Enabled = enable;
			//ItoolStripMenuItem.Enabled = enable;
			this.AllowDrop = enable;
		}

		/// <summary>
		/// Form1のボタンの有効無効の設定
		/// </summary>
		public void CheckedChanged() {
			bool checkTejun = frmTejun == null ? false : frmTejun.Tejun_commit;			//手順が確定

			int tsheetCommitNo = 0;		//工具表が確定している数
			if (NcdTool.Tejun.NcList.NcNamsAll.Count != 0) {
				foreach (TejunSet.ToolSheet ts in TsheetList) if (ts.Tolst_commit) tsheetCommitNo++;
			}

			toolStrip_Texas.Enabled =
			toolStripEureka.Enabled =
			ツールシートToolStripMenuItem.Enabled =
			加工手順ToolStripMenuItem.Enabled = (checkTejun && tsheetCommitNo == TsheetList.Length);

			// 「開く」でオープンされた加工手順の場合の追加処置（NCSPEED関連）
			if (NcdTool.Tejun.NcList.NcNamsAll.Count != 0) {
				if (NcdTool.Tejun.Ncspeed) {
					toolStripEureka.Enabled = false;
					//if (NcdTool.Tejun.collision()) toolStrip_Texas.Enabled = false;
					foreach (NcdTool.NcName.NcNam ncsd in NcdTool.Tejun.NcList.NcNamsAll_NcExist) {
						if (ncsd.nggt.zaisGrp != CamUtil.Material.ZgrpgetPC(ncsd.Ncdata.ncInfo.xmlD.SmMAT)) {
							System.Windows.Forms.MessageBox.Show("検証の材質と手順で設定された材質が異なる。データ出力できません。");
							toolStrip_Texas.Enabled = false;
							break;
						}
						for (int ii = 0; ii < ncsd.Itdat; ii++)
							if (ncsd.Ncdata.ncInfo.xmlD[ii].SmCLC != "OK") {
								System.Windows.Forms.MessageBox.Show("検証で干渉が検出されています。データ出力できません。");
								toolStrip_Texas.Enabled = false;
								break;
							}
					}
				}
				else {
					if (File.Exists(TjnDir + "\\Tejun")) toolStrip_Texas.Enabled = false;
				}
			}
			else {
				toolStripEureka.Enabled = false;
			}
		}

		/// <summary>
		/// 出力済みのＮＣデータのコピー
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void 出力済みＮＣデータ処理ToolStripMenuItem_Click(object sender, EventArgs e) {
			using (FormTexasCopy aa = new FormTexasCopy()) {
				aa.Show();
			}
		}

		/// <summary>
		/// ＮＣデータの設備での動作確認
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TestToolStripMenuItem_Click(object sender, EventArgs e) {

			Application.DoEvents();
			StringBuilder errMessage = new StringBuilder();
			DialogResult result = DialogResult.OK;
			using (FormCommonDialog frmDia = new FormCommonDialog("マクロ解析", MacroExec, errMessage)) {
				result = frmDia.ShowDialog();
			}
			if (result == DialogResult.OK)
				MessageBox.Show("マクロ解析の終了。結果は以下のファイルに保存しました。C:\\ncd\\TGProgram\\temp\\_main_simulate");
			else
				MessageBox.Show(errMessage.ToString(), "マクロ解析", MessageBoxButtons.OK, MessageBoxIcon.Error);

			return;
		}
		public void MacroExec(Label mess) {
			string dir0 = Directory.GetCurrentDirectory();	// 保存するディレクトリ
			Angle3[][] angle;
			double[] feedrate;

			File.Delete(dir0 + "\\_main_simulate");

			foreach (NcdTool.NcName.NcNam ncd in NcdTool.Tejun.NcList.NcNamsAll) {
				if (!NcdTool.NcName.Zopt.NcOutput.Nctoks(ncd)) continue;
				// /////////////////
				// ＮＣデータの作成
				// /////////////////
				mess.Text = ncd.nnam;
				// 回転軸の回転角を設定する
				angle = ncd.Tdat.Select(kog => kog.Tld.XmlT.MachiningAxisList).ToArray();
				// 標準の送り速度を設定する
				feedrate = ncd.Tdat.Select(kog => kog.Tld.XmlT.FEEDR).ToArray();

				using (TejunSet._main_simulate main = new TejunSet._main_simulate(
				ncd.nnam, NcdTool.Tejun.Mach, dir0, ncd.Ncdata.ncInfo.xmlD.CamDimension, angle, feedrate, ncd.nggt)) {

					try { main.ReadLine_File(); }
					catch (Exception ex) { MessageBox.Show(ex.Message); }

					Application.DoEvents();
				}
			}
		}
	}
}