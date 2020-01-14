using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Data.SqlClient;
using Excel = Microsoft.Office.Interop.Excel;

namespace NcTejun.Output
{
	partial class FormNcSet : Form
	{
		/// <summary>
		/// ツリービューのダブルクリックバグ対策
		/// </summary>
		protected class NoClickTree : TreeView
		{
			/// <summary></summary>
			protected override void WndProc(ref Message m) {
				// Suppress WM_LBUTTONDBLCLK
				if (m.Msg == 0x203) { m.Result = IntPtr.Zero; }
				else base.WndProc(ref m);
			}
		}

		/// <summary>FormTexasSetでの出力名設定値 保存</summary>
		static public string OUT;
		/// <summary>FormTexasSetでのカサブランカ情報 保存</summary>
		static public Index_Main casaData;





		/// <summary>ＮＣデータ、手順書、工具表の出力の情報を作成する主要クラス</summary>
		protected NcOutput ncOutput;

		/// <summary>カサブランカＤＢのデータセット</summary>
		public CasaData Casa;

		/// <summary></summary>
		//protected System.Windows.Forms.TreeView treeView1;
		protected NoClickTree treeView1;

		/// <summary>コンストラクタ</summary>
		public FormNcSet() { InitializeComponent(); }

		/// <summary>コンストラクタ</summary>
		public FormNcSet(int dummy) {

			InitializeComponent();

			// クラスの作成
			// ＮＣデータの出力リストの作成
			this.ncOutput = new NcOutput();

			// /////////////////////////
			// workName(combobox1)の設定
			// /////////////////////////
			// 金型情報の取得
			this.Casa = new CasaData(NcdTool.Tejun.Seba, NcdTool.Tejun.Mach.CasMachineName);

			// /////////////////////////
			// treeビューの初期設定
			// /////////////////////////
			this.treeView1 = new NoClickTree() {
				Name = "treeView1",
				Anchor = (System.Windows.Forms.AnchorStyles)(
				System.Windows.Forms.AnchorStyles.Top |
				System.Windows.Forms.AnchorStyles.Bottom |
				System.Windows.Forms.AnchorStyles.Left |
				System.Windows.Forms.AnchorStyles.Right),
				CheckBoxes = true,
				Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128))),
				ItemHeight = 18,
				TabStop = false
			};
		}
		/// <summary>
		/// ＮＣデータを作成したフォームから出力します
		/// </summary>
		protected void NcSet_NcOutput() {

			// ////////////////////////////////////
			// ////////////////////////////////////
			// チェックのための修正時に表示する
			if (CamUtil.ProgVersion.Debug) {
				//MessageBox.Show("クリアランスプレーンはチェックのため１００にしています。");
				//MessageBox.Show("VersionとRADIUS_HOSEIはチェックのため出力されていません。");
				//MessageBox.Show("安定したら文字挿入時にＧ０１の後とＦの前にスペースを挿入すること");
			}
			// ////////////////////////////////////
			// ////////////////////////////////////

			// ＮＣ出力ログの出力
			Log_NcOutput();

			// 出力データを入れるフォルダーの作成
			if (ncOutput.MakePTRFolder() == false)
				return;

			// ////////////////////////////////////
			// ＮＣデータ出力実行フォームの作成と実行
			// ////////////////////////////////////
			ncOutput.frmOutText = new TejunSet.FormTjnText();

			//FormNcOutMessage frmNcOutMessage = new FormNcOutMessage(ncOutput);
			//DialogResult frmNcOutputResult = frmNcOutMessage.ShowDialog();
			StringBuilder errMessage = new StringBuilder();
			//FormCommonDialog frmNcOutMessage = new FormCommonDialog("NcOutput", ncOutput, errMessage);
			DialogResult frmNcOutputResult = DialogResult.OK;
			using (CamUtil.FormCommonDialog frmNcOutMessage = new CamUtil.FormCommonDialog("ＮＣデータ出力", ncOutput.NcOutputExe, errMessage)) {
				frmNcOutputResult = frmNcOutMessage.ShowDialog();
			}
			if (frmNcOutputResult != DialogResult.OK) {
				MessageBox.Show(errMessage.ToString(), "NcOutput",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				ncOutput.frmOutText.richTextBox1.Text += errMessage.ToString();
			}

			// ////////////////////////////////////
			// ////////////////////////////////////

			// メッセージの表示
			if (frmNcOutputResult == DialogResult.Cancel) {
				ncOutput.frmOutText.Text = "エラー表示";
				ncOutput.frmOutText.Show();
				return;
			}
			if (ncOutput.frmOutText.richTextBox1.Text.Length > 0) {
				ncOutput.frmOutText.Text = "メッセージ";
				ncOutput.frmOutText.Show();
				return;
			}
			MessageBox.Show("ＮＣデータは正常に出力されました。\n" + DateTime.Now.ToString()
				, "手順名：" + NcdTool.Tejun.TejunName);
			return;
		}

		protected void TreeviewSet(TreeView treeV, NcOutput.NcOutList ncoutList) {
			TreeNode s_node, n_node, t_node;
			string tolstname;
			bool outnc;

			// ////////////////
			// treeビューの設定
			// ////////////////
			treeV.BeginUpdate();
			treeV.Nodes.Clear();
			treeV.CheckBoxes = true;
			treeV.ShowNodeToolTips = true;

			treeV.TopNode = new TreeNode("手順");

			s_node = n_node = t_node = null;
			tolstname = "\\";	// ツールシート単位
			for (int ii = 0; ii < ncoutList.OF_Count(); ii++) {
				NcOutput.NcToolL nlist0 = ncoutList.ListInFile(ii)[0];
				// ツールシート処理
				if (tolstname != nlist0.TolstName) {
					tolstname = nlist0.TolstName;
					s_node = new TreeNode(nlist0.TolstName) {
						Checked = true,
						Tag = nlist0.TolstName
					};
					treeV.Nodes.Add(s_node);
				}
				// ＮＣデータ処理
				n_node = new TreeNode(nlist0.Outnam);
				outnc = false;
				foreach (NcOutput.NcToolL nlist in ncoutList.ListInFile(ii)) {
					// 工具データ処理
					t_node = new TreeNode(nlist.tolInfo.TsetCAM.tscamName);

					//if (nlist.skog.matchK[nlist.matchNo].K2.tlgn != null && nlist.skog.matchK[nlist.matchNo].K2.tlgn.tmod == '0') {
					if (nlist.Skog.Output && nlist.nknum == null) {
						t_node.Checked = true;
						t_node.Tag = nlist;
						outnc = true;
					}
					else {
						t_node.Checked = false;
						t_node.Tag = null;	// Checked を true にできない場合
					}
					n_node.Nodes.Add(t_node);
				}
				if (outnc) {
					n_node.Checked = true;
					n_node.Tag = nlist0.Ncnam;
				}
				else {
					n_node.Checked = false;
					n_node.Tag = null;
				}
				s_node.Nodes.Add(n_node);
			}
			treeV.EndUpdate();

			// sNodeと、一部の工具を出力しないnNodeを展開する
			foreach (TreeNode sNode in treeV.Nodes) {
				sNode.Expand();
				foreach (TreeNode nNode in sNode.Nodes)
					foreach (TreeNode tNode in nNode.Nodes)
						if (nNode.Tag != null && tNode.Tag == null) nNode.Expand();
			}

			treeView1.BeforeCheck += new TreeViewCancelEventHandler(TreeView1_BeforeCheck);
			treeView1.AfterCheck += new TreeViewEventHandler(TreeView1_AfterCheck);
		}

		/// <summary>
		/// ツリービューのチェックボックスのオンオフ
		/// </summary>
		/// <remarks>
		/// ツリービューのチェックボックスをオフとした場合には中間出力されたＮＣデータを削除することで機能して
		/// いるので、ファイル単位でオンオフが可能である。NcNam.Kogu.outputの場合のようにSimSpinFeedでファイル
		/// 内の工具単位で削除すれば工具単位でも可能とはなるが、頻度としては極端に少ないのでそこまでする価値はない。
		/// </remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void TreeView1_BeforeCheck(object sender, TreeViewCancelEventArgs e)
		{
			if (e.Node.Level >= 2 || e.Node.Tag == null) {
				e.Cancel = true;
				return;
			}
		}
		/// <summary>
		/// ツリービューのチェックボックスのオンオフ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void TreeView1_AfterCheck(object sender, TreeViewEventArgs e)
		{
			Set_outOK(false);
		}

		/// <summary>
		/// 確定時にツリービューのチェック情報をＮＣ出力リストに反映する
		/// </summary>
		/// <returns>出力しないＮＣデータ数</returns>
		protected int CheckTreeView() {
			foreach (NcOutput.NcToolL nlist in ncOutput.ncoutList.Tj)
				nlist.tNodeChecked = false;

			foreach (TreeNode sNode in this.treeView1.Nodes)
				foreach (TreeNode nNode in sNode.Nodes)
					foreach (TreeNode tNode in nNode.Nodes)
						if (tNode.Tag != null)
							((NcOutput.NcToolL)tNode.Tag).tNodeChecked = (sNode.Checked && nNode.Checked && tNode.Checked);

			List<string> noOutput = new List<string>();
			foreach (NcOutput.NcToolL nlist in ncOutput.ncoutList.Tj)
				if (nlist.tNodeChecked == false && nlist.nknum == null) noOutput.Add(nlist.Outnam + " " + nlist.tolInfo.Toolset.tset_name);
			if (noOutput.Count > 0) {
				string sout = "出力されないＮＣデータが工具単位で " + noOutput.Count + " 個あります";
				foreach (string ss in noOutput) sout += "\n    " + ss;
				MessageBox.Show(sout);
			}
			return noOutput.Count;
		}
		/// <summary>
		/// 出力の３ボタンの制御
		/// </summary>
		/// <param name="set">true:表示する  false:非表示にする（情報入力継続）</param>
		protected virtual void Set_outOK(bool set) { return; }





		/// <summary>
		/// ＮＣデータの上書き確認
		/// </summary>
		protected DialogResult CheckNcFile(string seba, string lfldr) {
			DialogResult result = DialogResult.Yes;

			string folder = NcOutput.Dir_PTR + seba;
			if (Directory.Exists(folder)) {
				folder += @"\" + lfldr;
				if (Directory.Exists(folder)) {
					result = MessageBox.Show(
						"すでに同じ名前（" + lfldr + "）で加工手順が出力されています。",
						"出力名の確認",
						MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2);
					if (result == DialogResult.OK)
						result = DialogResult.Yes;
				}
			}
			return result;
		}

		/// <summary>
		/// 工具表の出力
		/// </summary>
		protected void Tolout(bool saveOK, bool zchi) {
			ToolSheetDB tsdList;

			foreach (TejunSet.ToolSheet tsheet in Program.frm1.TsheetList) {
				if (tsheet.NcNamsCount == 0)
					continue;

				// 最大加工深さの計算
				if (zchi) Calc_MinZ(tsheet);

				// ///////////////////
				// 工具表ＤＢへの出力
				// ///////////////////
				if (tsheet.TolstName != "" && saveOK) {
					try {
						tsdList = new ToolSheetDB(tsheet, false);
						tsdList.Tool_out_tdb();
					}
					catch (Exception ex) {
						CamUtil.LogOut.CheckCount("工具表/手順書ＤＢ登録エラー", false, NcdTool.Tejun.TejunName + " : " + ex.Message);
						MessageBox.Show($"工具表をデータベースに登録できませんでした。もう一度実行してください。{tsheet.TolstName}\n\n{ex.Message}");
						return;
					}
				}

				switch (NcdTool.Tejun.BaseNcForm.Id) {
				case CamUtil.BaseNcForm.ID.BUHIN:
					// ///////////////////////////////////
					// 工具表の表示（エクセルで）
					// エクセルが表示可能な場合使用可能にすること。tryでは引っかからない。 2015/01/08"
					// ///////////////////////////////////
					/*
					Excel.Application ExcelApp = new Excel.Application();
					//エクセルを表示
					ExcelApp.Visible = true;
					//エクセルテンプレートより新規作成
					Excel.Workbook WorkBook = ExcelApp.Workbooks.Add(@"\\nt0040np\tgsite_data\Documents\部品NC加工指示書.xlt");
					//マクロの実行
					ExcelApp.Run("SetData", NcdTool.Tejun.seba, NcdTool.Tejun.tejunName, tset.tolstName);
					//ワークブックを閉じる
					//WorkBook.Close();
					//エクセルを閉じる
					//ExcelApp.Quit();
					*/
					break;
				case CamUtil.BaseNcForm.ID.GENERAL:
				default:
					// ///////////////////////////////////
					// 工具表の表示（エクセル貼付け用））
					// ///////////////////////////////////
					tsdList = new ToolSheetDB(tsheet, true);
					Application.DoEvents();
					// ワーニングメッセージの表示
					CamUtil.LogOut.Warning("tolout");
					string str2 = tsdList.Tool_out_exl();   // new
					Application.DoEvents();
					if (str2 != null) {
						TejunSet.FormTjnText frmOutText = new TejunSet.FormTjnText() { Text = "工具表" };
						frmOutText.richTextBox1.Text = str2;
						frmOutText.ShowDialog();
					}
					break;
				}
			}
		}

		/// <summary>
		/// 手順書の出力
		/// </summary>
		protected void Tjnout(bool saveOK) {
			TejunSheetDB tjnList;

			// //////////////////////////////////////////////////////
			// 手順書のＤＢ出力（部品のみ、基本は工具表作成後に実施）
			// //////////////////////////////////////////////////////
			try {
				// 手順情報の作成
				tjnList = new TejunSheetDB();
				// 手順ＤＢへの保存
				if (saveOK)
					tjnList.DBOut(true);
				if (NcdTool.Tejun.BaseNcForm.Id == CamUtil.BaseNcForm.ID.BUHIN) {
					if (saveOK)
						MessageBox.Show("工具データは正常に出力されました。\n" + "部品NC加工指示書.xltにて表示してください", "手順名：" + NcdTool.Tejun.TejunName);
				}
				else {
					// 手順の表示
					TejunSet.FormTjnText frmOutText = new TejunSet.FormTjnText();
					foreach (string stmp in tjnList.TS_List()) frmOutText.richTextBox1.Text += stmp + "\n";
					frmOutText.ShowDialog();
				}
			}
			catch (Exception ex) {
				CamUtil.LogOut.CheckCount("工具表/手順書ＤＢ登録エラー", false, NcdTool.Tejun.TejunName + " : " + ex.Message);
				MessageBox.Show("手順書をデータベースに登録できませんでした。\n\n" + ex.Message, "手順名：" + NcdTool.Tejun.TejunName);
			}
		}

		private void Calc_MinZ(TejunSet.ToolSheet tsheet) {
			CamUtil.LCode.NcLineCode.NcDist passLength;
			CamUtil.LCode.NcLineCode txtd;
			string ddat;
			int tcnt;

			foreach (NcdTool.NcName.NcNam nnam in NcdTool.Tejun.NcList.NcNamsTS(tsheet.TolstName)) {
				tcnt = 0;
				foreach (NcdTool.NcName.Kogu skog in nnam.Tdat) {
					using (StreamReader sr = new StreamReader(skog.Parent.Ncdata.fulnamePC)) {
						passLength = new CamUtil.LCode.NcLineCode.NcDist(null, skog.Tld.XmlT.MachiningAxisList);
						txtd = new CamUtil.LCode.NcLineCode(skog.Parent.Ncdata.ncInfo.xmlD.NcClrPlaneList, NcdTool.Tejun.BaseNcForm, CamUtil.LCode.NcLineCode.GeneralDigit, false, true);
						NcdTool.NcSIM.RegForm regf = new NcdTool.NcSIM.RegForm(nnam);
						while (!sr.EndOfStream) {
							ddat = regf.Conv(sr.ReadLine());
							if (ddat != null) {
								txtd.NextLine(ddat);
								if (txtd.Tcnt == tcnt) passLength.PassLength(txtd);
							}
						}
					}
					for (int ii = 0; ii < skog.matchK.Length; ii++) {
						if (skog.matchK[ii].divData.ncdist.Min == null)
							skog.matchK[ii].divData.ncdist.MinAdd(passLength.Min);
						if (skog.matchK[ii].divData.ncdist.Max == null)
							skog.matchK[ii].divData.ncdist.MaxAdd(passLength.Max);
					}
					tcnt++;
				}
			}
		}

		/// <summary>
		/// ＮＣ出力ログの出力
		/// </summary>
		private void Log_NcOutput() {
			int tcnt = 0;
			string tilt = ncOutput.doji5ax ? "S" : ncOutput.keisha.ToString().Substring(0, 1);
			string same = "F", ninn = "Fls", move = "", prob = "F", simu = "";
			foreach (TejunSet.ToolSheet tsht in Program.frm1.TsheetList)
				tcnt += tsht.Tols.Count;
			foreach (NcOutput.NcToolL nctl in ncOutput.ncoutList.Tl)
				if (nctl.Ncnam.Jnno.SameNc) { same = "T"; break; }

			foreach (NcOutput.NcToolL nctl in ncOutput.ncoutList.Tl)
				if (nctl.Skog.Tld.XmlT.OPTLF != null) { ninn = "tlf"; break; }
			foreach (NcOutput.NcToolL nctl in ncOutput.ncoutList.Tl)
				if (nctl.Skog.Tld.XmlT.OPTION()) { ninn = "sfr"; break; }

			foreach (NcOutput.NcToolL nctl in ncOutput.ncoutList.Tl) {
				if (nctl.Skog.Tld.XmlT.ClMv_Offset != 0.0 && move.IndexOf("O") < 0) move += "O";
				//if (nctl.Skog.Tld.XmlT.ClMv_T_axis != 0.0 && move.IndexOf("T") < 0) move += "T";
				if (nctl.Skog.Tld.XmlT.ClMv_X_axis != 0.0 && move.IndexOf("U") < 0) move += "U";
				if (nctl.Skog.Tld.XmlT.ClMv_Y_axis != 0.0 && move.IndexOf("V") < 0) move += "V";
				if (nctl.Skog.Tld.XmlT.ClMv_Z_axis != 0.0 && move.IndexOf("W") < 0) move += "W";
				if (nctl.Ncnam.nggt.trns.X != 0.0 && move.IndexOf("X") < 0) move += "X";
				if (nctl.Ncnam.nggt.trns.Y != 0.0 && move.IndexOf("Y") < 0) move += "Y";
				if (nctl.Ncnam.nggt.trns.Z != 0.0 && move.IndexOf("Z") < 0) move += "Z";
				if (nctl.Ncnam.nggt.ToolLengthHosei.Zero != true && move.IndexOf("L") < 0) move += "L";
			}
			if (move.Length == 0) move = "F";

			foreach (NcOutput.NcToolL nctl in ncOutput.ncoutList.Tl)
				if (nctl.Skog.matchK[0].K2.Tlgn.Toolset.ToolFormType == "PRB" || nctl.Skog.matchK[0].K2.Tlgn.Toolset.ToolFormType == "PRV") { prob = "T"; break; }

			if (NcdTool.Tejun.Ncspeed) {
				foreach (NcOutput.NcToolL nctl in ncOutput.ncoutList.Tl) {
					if (nctl.Skog.Tld.XmlT.SmTSS && simu.IndexOf("1.TS選択") < 0) simu += "1.TS選択";   // ツールセット選択
					if (nctl.Skog.Tld.XmlT.SmTSB && simu.IndexOf("2.TS最適") < 0) simu += "2.TS最適";   //ツールセット最適化
					if (nctl.Skog.Tld.XmlT.SmFDC && simu.IndexOf("3.FD最適") < 0) simu += "3.FD最適";   //送り速度最適化
					if (nctl.Skog.Tld.XmlT.SmACD && simu.IndexOf("4.AC削除") < 0) simu += "4.AC削除";   //エアカット削除
					if (nctl.Skog.Tld.XmlT.SmLNS && simu.IndexOf("5.TL分割") < 0) simu += "5.TL分割";   //工具寿命分割
				}
				if (simu.Length == 0) simu = "0.collis";    //コリジョン
			}
			else simu = "F";

			CamUtil.LogOut.CheckOutput(CamUtil.LogOut.FNAM.NCDOUTPUT, NcdTool.Tejun.TejunName, NcdTool.Tejun.Mach.name,
				$"tcnt={tcnt,-3:d} tilt={tilt} same={same} ninn={ninn,-3} move={move,-3} prob={prob} simu={simu}");
		}
	}
}