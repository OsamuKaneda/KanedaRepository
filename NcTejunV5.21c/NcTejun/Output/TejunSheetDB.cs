using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

using CamUtil;

namespace NcTejun.Output
{
	/// <summary>
	/// 手順情報[不変]
	/// </summary>
	class TejunSheetDB
	{
		private const string connectionStringCasa = "Data Source=nt0040np;Initial Catalog=InputCasa;User ID=cadceus;Password=3933";
		private const string connectionStringTexas = "Data Source=nt0040np;Initial Catalog=TexasDB;User ID=cadceus;Password=3933";
		private const int lmax = 30;
		private const string nkPIC = "&&&&&|&&&&&";

		/// <summary>工程管理ＤＢにより社員番号から氏名を取得する</summary>
		/// <param name="manID">社員番号</param>
		/// <returns>氏名</returns>
		private static string ManName(string manID) {
			string manName = "";

			using (SqlConnection connection = new SqlConnection(TejunSheetDB.connectionStringCasa))
			using (var com = new SqlCommand() { Connection = connection }) {
				connection.Open();
				com.CommandText = "dbo.tjn_SHAIN";
				com.CommandType = CommandType.StoredProcedure;
				com.Parameters.Add(new SqlParameter("@ID", manID));
				manName = (string)com.ExecuteScalar() ?? "";
			}
			return manName;
		}

		/// <summary>工程管理ＤＢより品名を取得する</summary>
		/// <param name="seizo">内製管理番号</param>
		/// <returns>品名</returns>
		private static string Hinmei(string seizo) {
			string hinName = "";

			using (SqlConnection connection = new SqlConnection(TejunSheetDB.connectionStringCasa))
			using (var com = new SqlCommand() { Connection = connection }) {
				connection.Open();
				com.CommandText = "dbo.tjn_HINMEI";
				com.CommandType = CommandType.StoredProcedure;
				com.Parameters.Add(new SqlParameter("@JNO", seizo));
				hinName = (string)com.ExecuteScalar() ?? "";
			}
			return hinName;
		}

		// //////////////
		// 以上 static
		// //////////////







		/// <summary>手順書表題ＤＢ</summary>
		private readonly TEJUN_SHEET tjnSheet;
		/// <summary>手順と工具表のリンクＤＢ</summary>
		private readonly TEJUN_TOOL[] tjnTool;
		/// <summary>プロセスシートＤＢ</summary>
		private readonly List<PROCESS_SHEET> prcSheet;

		/// <summary>総加工時間</summary>
		private readonly double total;
		/// <summary>初期材質名</summary>
		private readonly string zais;
		/// <summary>工具交換時間(sec)</summary>
		private readonly int tchg_time;
		/// <summary>工具交換を含む全加工時間(min)</summary>
		private double Atim(PROCESS_SHEET ps) { return ps.ktim + (ps.ktim > 0.0 ? tchg_time / 60.0 : 0.0); }
		/// <summary>工具交換を含む全加工時間の文字列</summary>
		private string Str_tim(PROCESS_SHEET ps) { return (Math.Abs(Atim(ps)) > NcdTool.NcName.NcNam.FLMIN) ? (Atim(ps)).ToString("f0") : "&"; }

		/// <summary>
		/// 唯一のコンストラクタ
		/// </summary>
		public TejunSheetDB() {
			TejunSet.ToolSheet[] tsheetList = Program.frm1.TsheetList;
			total = 0;
			zais = null;
			tchg_time = NcdTool.Tejun.Mach.Tctim;

			// 文字数制限の管理
			CamUtil.StringLengthDB strLengthErr = new CamUtil.StringLengthDB();

			// //////////////////////
			// ツールセット情報作成
			// //////////////////////
			tjnTool = new TEJUN_TOOL[tsheetList.Length];
			for (int tsetNo = 0; tsetNo < tsheetList.Length; tsetNo++) tjnTool[tsetNo] = new TEJUN_TOOL(tsheetList[tsetNo]);

			// //////////////////////
			// 工具単位のＮＣリスト作成
			// //////////////////////
			int tshtNo;
			prcSheet = new List<PROCESS_SHEET>();
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll) {
				for (tshtNo = 0; tshtNo < tsheetList.Length; tshtNo++) if (tsheetList[tshtNo].TolstName == ncnam.tsheet) break;
				if (ncnam.Itdat == 0 && ncnam.nnam == NcdTool.NcName.NcNam.DMY) {
					// ダミーのＮＣデータの処理（snum==0）
					prcSheet.Add(new PROCESS_SHEET(NcdTool.Tejun.BaseNcForm, ncnam, null, null, tshtNo));
				}
				else {
					foreach (NcdTool.NcName.Kogu kog in ncnam.Tdat) foreach (NcdTool.TMatch.MatchK mck in kog.matchK) {
							try {
								//if (NcdTool.NcName.Zopt.NcOutput.nctoks(ncnam)) {
								if (kog.Output) {
									// 出力するＮＣデータの場合
									prcSheet.Add(new PROCESS_SHEET(NcdTool.Tejun.BaseNcForm, ncnam, kog, mck, strLengthErr, tshtNo));
									total += Atim(prcSheet[prcSheet.Count - 1]);
									if (zais == null) zais = prcSheet[prcSheet.Count - 1].workzai;
								}
								else
									prcSheet.Add(new PROCESS_SHEET(NcdTool.Tejun.BaseNcForm, ncnam, kog, mck, tshtNo));
							}
							catch {
								prcSheet.Add(null);
								CamUtil.LogOut.CheckCount("TejunSheetDB 160", false, "手順書１行作成時にエラーとなった場合の動作確認 " + ncnam.nnam);
							}
						}
				}
			}

			// //////////////////////
			// 手順書の表題情報作成
			// //////////////////////
			tjnSheet = new TEJUN_SHEET(prcSheet.Count, strLengthErr);

			// ///////////////////////////////////////////
			// 文字の一部をカットした場合のメッセージ出力
			// ///////////////////////////////////////////
			strLengthErr.ErrorOut("TejunSheetDB 176");
		}

		/// <summary>
		/// 手順書ＤＢへのデータ出力
		/// </summary>
		/// <param name="save">正式に手順書ＤＢに保存する場合は true</param>
		public void DBOut(bool save) {
			SqlCommand command;
			string spShet, spTool, spCons, spProc;
			if (save) {
				spShet = "dbo.tg_TEJUN2_HD2";
				spTool = "dbo.tg_TEJUN2_TL2";
				spCons = "dbo.tg_TEJUN2_TC2";
				spProc = "dbo.tg_TEJUN2_PS2";
			}
			else {
				spShet = "dbo.tg_TEJUN2test_HD2";
				spTool = "dbo.tg_TEJUN2test_TL2";
				spCons = "dbo.tg_TEJUN2test_TC2";
				spProc = "dbo.tg_TEJUN2test_PS2";
			}

			using (SqlConnection connection = new SqlConnection(TejunSheetDB.connectionStringTexas)) {
				// 手順書のヘッダー情報を出力する
				connection.Open();
				using (command = new SqlCommand(spShet, connection)) {
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add(new SqlParameter("@J_NO", tjnSheet.J_NO));
					command.Parameters.Add(new SqlParameter("@TEJUN_NAME", tjnSheet.TEJUN_NAME));
					command.Parameters.Add(new SqlParameter("@BASE_FORMAT", tjnSheet.BASE_FORMAT.Name));
					command.Parameters.Add(new SqlParameter("@TOOL_SHEET_NAME", tjnSheet.TOOL_SHEET_NAME));

					command.Parameters.Add(new SqlParameter("@SHAIN_CODE", tjnSheet.SHAIN_CODE));
					command.Parameters.Add(new SqlParameter("@MACHINE_NAME", tjnSheet.MACHINE_NAME));
					command.Parameters.Add(new SqlParameter("@KAKO_START", tjnSheet.KAKO_START));
					command.Parameters.Add(new SqlParameter("@PROCESS_NAME", tjnSheet.PROCESS_NAME));
					command.Parameters.Add(new SqlParameter("@PALLET_NO", (object)tjnSheet.PALLET_NO ?? DBNull.Value));
					command.Parameters.Add(new SqlParameter("@DANDR_NAME", (object)tjnSheet.DANDORI_NAME ?? DBNull.Value));
					command.Parameters.Add(new SqlParameter("@DANDR_HEIGHT", (object)tjnSheet.DANDORI_HEIGHT ?? DBNull.Value));
					command.Parameters.Add(new SqlParameter("@SOZAI_X", (object)tjnSheet.SOZAI_X ?? DBNull.Value));
					command.Parameters.Add(new SqlParameter("@SOZAI_Y", (object)tjnSheet.SOZAI_Y ?? DBNull.Value));
					command.Parameters.Add(new SqlParameter("@SOZAI_Z", (object)tjnSheet.SOZAI_Z ?? DBNull.Value));
					command.Parameters.Add(new SqlParameter("@KAKO_TIME", (int)Math.Ceiling(this.total)));

					command.Parameters.Add(new SqlParameter("@MATERIAL_NAME", this.zais));
					command.Parameters.Add(new SqlParameter("@TOOL_CHANGE", this.tchg_time));
					command.Parameters.Add(new SqlParameter("@SIM_SYSTEM", tjnSheet.sim_system));
					command.Parameters.Add(new SqlParameter("@SIM_MACHIN", tjnSheet.sim_machin));
					command.Parameters.Add(new SqlParameter("@KOTEI_CODE", tjnSheet.dbnam0));
					command.Parameters.Add(new SqlParameter("@BUHIN_NAME", tjnSheet.buhin_name));
					command.ExecuteNonQuery();
				}

				// 手順書と工具表の関連情報を出力する
				for (int tshtNo = 0; tshtNo < tjnTool.Length; tshtNo++) {
					if (tjnTool[tshtNo].TSHEET_NAME == "") continue;    // 名前なしの工具表は出力しない
					using (command = new SqlCommand(spTool, connection)) {
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.Add(new SqlParameter("@J_NO", tjnSheet.J_NO));
						command.Parameters.Add(new SqlParameter("@TEJUN_NAME", tjnSheet.TEJUN_NAME));
						command.Parameters.Add(new SqlParameter("@TSHEET_NO", tshtNo));
						command.Parameters.Add(new SqlParameter("@ORIGINAL_J_NO", tjnTool[tshtNo].ORIGINAL_J_NO));
						command.Parameters.Add(new SqlParameter("@ORIGINAL_TEJUN", tjnTool[tshtNo].ORIGINAL_TEJUN));
						command.Parameters.Add(new SqlParameter("@TOOL_SHEET_NAME", tjnTool[tshtNo].TSHEET_NAME));
						command.ExecuteNonQuery();
					}
				}

				// 初期消耗率が０でない場合は各工具の最終消耗率をＤＢに保存する
				bool shomo;
				for (int tshtNo = 0; tshtNo < tjnTool.Length; tshtNo++) {
					shomo = false;
					foreach (NcdTool.Tool.Tol tol in tjnTool[tshtNo].TSHEET.Tols)
						if (tol.Initial_consumpt > 0.0) { shomo = true; break; }
					if (shomo != true) continue;
					foreach (NcdTool.Tool.Tol tol in tjnTool[tshtNo].TSHEET.Tols) {
						if (tol.Unum <= 0) continue;
						using (command = new SqlCommand(spCons, connection)) {
							command.CommandType = CommandType.StoredProcedure;
							command.Parameters.Add(new SqlParameter("@J_NO", tjnSheet.J_NO));
							command.Parameters.Add(new SqlParameter("@TEJUN_NAME", tjnSheet.TEJUN_NAME));
							command.Parameters.Add(new SqlParameter("@TSHEET_NO", tshtNo));
							//command.Parameters.Add(new SqlParameter("@TSHEET_JNO", tjnTool[tsetNo].ORIGINAL_J_NO));
							//command.Parameters.Add(new SqlParameter("@TSHEET_NAME", tjnTool[tsetNo].TSHEET_NAME));

							command.Parameters.Add(new SqlParameter("@magazine_no", tol.Rnum));
							command.Parameters.Add(new SqlParameter("@tool_no", tol.Unum));
							command.Parameters.Add(new SqlParameter("@consume_rate", tol.Consumption_new));
							command.ExecuteNonQuery();
						}
					}
				}

				// プロセスシートを出力する
				for (int ii = 0; ii < prcSheet.Count; ii++) {
					using (command = new SqlCommand(spProc, connection)) {
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.Add(new SqlParameter("@J_NO", tjnSheet.J_NO));
						command.Parameters.Add(new SqlParameter("@TEJUN_NAME", tjnSheet.TEJUN_NAME));
						command.Parameters.Add(new SqlParameter("@KAKOJUN", ii + 1));

						command.Parameters.Add(new SqlParameter("@COMMENT", (object)prcSheet[ii].comnt ?? DBNull.Value));
						command.Parameters.Add(new SqlParameter("@OUT_NAME", prcSheet[ii].outname));    //16
						command.Parameters.Add(new SqlParameter("@HENSHU", prcSheet[ii].hens));         //16

						command.Parameters.Add(new SqlParameter("@TSHEET_NO", prcSheet[ii].tshtNo));
						//command.Parameters.Add(new SqlParameter("@TSHEET_NAME", prcSheet[tsetNo][ii].tset));	//16
						command.Parameters.Add(new SqlParameter("@PAGE_NO", prcSheet[ii].snum));
						command.Parameters.Add(new SqlParameter("@TOOL_NO", prcSheet[ii].tnum));
						command.Parameters.Add(new SqlParameter("@O_NUMBER", prcSheet[ii].onum));
						command.Parameters.Add(new SqlParameter("@TNUM_ONUM", prcSheet[ii].TorO));

						command.Parameters.Add(new SqlParameter("@KTIME_A", Math.Round(prcSheet[ii].ktim, 2)));
						command.Parameters.Add(new SqlParameter("@KTIME_R", Math.Round(prcSheet[ii].rtim, 2)));
						command.Parameters.Add(new SqlParameter("@JUMYO", Math.Round(prcSheet[ii].life, 2)));

						command.Parameters.Add(new SqlParameter("@TOOLNAME", prcSheet[ii].tnam));   //16
						command.Parameters.Add(new SqlParameter("@TOOLDIAM", prcSheet[ii].diam));
						command.Parameters.Add(new SqlParameter("@SPINDLE", prcSheet[ii].schi));
						command.Parameters.Add(new SqlParameter("@FEEDRATE", prcSheet[ii].fchi));

						command.Parameters.Add(new SqlParameter("@SIM_OKNG", prcSheet[ii].str_sim));    //2
						command.Parameters.Add(new SqlParameter("@CAM_CMNT", prcSheet[ii].str_com));    //20
						command.Parameters.Add(new SqlParameter("@AXIS_NUM", prcSheet[ii].axis_num));   //2
						command.Parameters.Add(new SqlParameter("@AXIS_TYP", prcSheet[ii].axis_typ));   //2

						command.Parameters.Add(new SqlParameter("@KAKO_DIRC", (object)prcSheet[ii].kh_direct ?? DBNull.Value)); //20
						command.Parameters.Add(new SqlParameter("@KAKO_MTHD", (object)prcSheet[ii].kh_method ?? DBNull.Value)); //20
						command.Parameters.Add(new SqlParameter("@KAKO_DIM1", prcSheet[ii].kh_dimen1)); //1
						command.Parameters.Add(new SqlParameter("@KAKO_NOKX", prcSheet[ii].kh_nokosX));
						command.Parameters.Add(new SqlParameter("@KAKO_NOKZ", prcSheet[ii].kh_nokosZ));
						command.Parameters.Add(new SqlParameter("@KAKO_DIM2", prcSheet[ii].kh_dimen2)); //1
						command.Parameters.Add(new SqlParameter("@KAKO_PCHX", prcSheet[ii].kh_pitchX));
						command.Parameters.Add(new SqlParameter("@KAKO_PCHZ", prcSheet[ii].kh_pitchZ));

						command.Parameters.Add(new SqlParameter("@MATERIAL_NAME", prcSheet[ii].workzai));

						command.ExecuteNonQuery();
					}
				}
			}
		}

		/// <summary>
		/// エクセルのコピペ用テキストの表示
		/// </summary>
		/// <returns></returns>
		public List<string> TS_List() {
			// 新しいデータ
			List<string> tjnData = new List<string>();
			foreach (var ss in this.prcSheet.GroupUntil(ps => ps.comnt != null))
				tjnData.AddRange(ExcelOutput(ss.ToList()));
			return tjnData;
		}
		private List<string> ExcelOutput(List<PROCESS_SHEET> sum) {
			int gcnt;	// １ページ（３０行）内の行数（１以上３０以下）
			int ccnt;	// 手順書のページ数
			PROCESS_SHEET ps1, ps2, pre;

			List<string> outList = new List<string>();
			List<string> tmpOP = new List<string>();

			// 分割された手順毎の総加工時間を計算する
			double total_page = 0; foreach (PROCESS_SHEET pst in sum) total_page += Atim(pst);
			double stime = total_page;

			string page;	// 工具表のページ番号。前行と同一の場合は"&"とする
			string tmpt1;	// 残り加工時間（時）
			string tmpt2;	// 残り加工時間（分）
			string outn;	// 出力名。前行と同一の場合は"&"とする

			string henk;	//
			string diams;	// 直径文字列
			string schis;	// 回転数文字列
			string fchis;	// 送り速度文字列

			gcnt = 0;	// １ページ（３０行）内の行数（１以上３０以下）
			ccnt = 0;	// 手順書のページ数
			for (int bang = 0; bang < sum.Count; bang++) {
				pre = (bang > 0) ? sum[bang - 1] : null;
				ps1 = sum[bang];
				ps2 = (bang + 1 < sum.Count) ? sum[bang + 1] : null;
				gcnt++;
				if (gcnt > lmax) gcnt = 1;
				if (gcnt == 1) ccnt++;


				// 出力名の作成。同一ＮＣ内工具の場合（==最初のＮＣデータ、異なる出力名、共有ＮＣデータ、以外）は同じ出力名を繰り返さない
				outn = (gcnt == 1 || ps1.outname != pre.outname || ps1.kyoy) ? ps1.outname : "&";
				// 工具表ページデータの作成
				page = (ps1.snum > 0 && (gcnt == 1 || ps1.snum != pre.snum)) ? ps1.snum.ToString() : "&";
				// 残り加工時間の作成
				tmpt1 = "&";
				tmpt2 = "&";
				if (Str_tim(ps1) != "&") {
					stime -= Atim(ps1);
					if (stime < 0) stime = 0.0;
					tmpt1 = ((int)Math.Round(stime) / 60).ToString() + "h";
					tmpt2 = ((int)Math.Round(stime) % 60).ToString();
				}
				henk = "&";
				diams = ps1.diam > 0.0 ? ps1.diam.ToString("##.00") : "&";
				schis = ps1.schi > 0.0 ? ps1.schi.ToString() : "&";
				fchis = ps1.fchi > 0.0 ? ps1.fchi.ToString() : "&";
				// 0      1     2       3    4    5    6    7    8       9       10      11   12 13     14     15    16    17      18       19
				// number Psnum outname tchi tnam diam schi fchi str_tim str_air str_sim hens &  nkpic1 nkpic2 timt1 timt2 comment axis_num axis_typ
				tmpOP.Add(String.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19}",
					(bang + 1).ToString(), page, outn, ps1.Tchi, ps1.tnam, diams, schis, fchis,
					Str_tim(ps1), ps1.Str_air.Trim(), ps1.str_sim, ps1.hens, henk,
					ps1.NkNokosi, ps1.NkPick, tmpt1, tmpt2, ps1.str_com, ps1.axis_num, ps1.axis_typ
				));



				// １ページが３０行になるように空欄を追加する
				// ただし、空欄を追加することでページ数が増える場合は追加しない
				if (SnumDiff(ps1, ps2)) {
					// 次の snum での手順の行数slCount を調べる
					int slCount;
					if (ps2 == null)
						slCount = lmax;
					else {
						slCount = 1;
						PROCESS_SHEET pss1, pss2;
						for (int ii = bang + 1; ii < sum.Count; ii++) {
							pss1 = sum[ii];
							pss2 = (ii + 1 < sum.Count) ? sum[ii + 1] : null;
							if (SnumDiff(pss1, pss2)) break;
							slCount++;
						}
					}

					if (gcnt + slCount > lmax) {
						while (gcnt < lmax) {
							gcnt++;
							tmpOP.Add("& & & & & & & & & & & & & " + nkPIC + " " + nkPIC + " & & & & &");
						}
					}
				}
			}
			outList.AddRange(tjnSheet.Print(ccnt, total_page, this.zais));
			foreach (string stmp in tmpOP) outList.Add(stmp);
			return outList;
		}
		private bool SnumDiff(PROCESS_SHEET ps1, PROCESS_SHEET ps2) {
			if (ps2 == null) return true;		// 最後のデータ
			if (ps2.snum == 0) return false;	// 次のＮＣデータがダミー
			if (ps1.tset == null && ps2.tset != null) return true;	// 現在がダミーで次はダミーではない
			if (ps1.tset != null && ps2.tset != null && ps1.tset != ps2.tset) return true;	// 次とtsetが異なる
			if (ps1.snum != ps2.snum) return true;	// 次とsnumが異なる
			return false;
		}

		/// <summary>
		/// 手順の簡易表示
		/// </summary>
		/// <returns></returns>
		public System.Text.StringBuilder Hyoji() {
			System.Text.StringBuilder gsetText = new StringBuilder();
			string schis, fchis;

			foreach (var psList in this.prcSheet.GroupUntil(ps => ps.comnt != null)) {
				gsetText.Append("Pag: NC NAME    : Ono : Tno : TOOL NAME     :  S  :  F  :L_Hosei: Time: Air:K:\n");
				gsetText.Append("---:------------:-----:-----:---------------:-----:-----:-------:-----:----:-:\n");
				foreach (PROCESS_SHEET ps1 in psList) {
					if (ps1.snum > 0) {
						schis = ps1.schi > 0.0 ? ps1.schi.ToString() : "&";
						fchis = ps1.fchi > 0.0 ? ps1.fchi.ToString() : "&";
						gsetText.AppendLine(
							$"P{ps1.snum:00} {ps1.outname,-12} O{ps1.onum:0000} {ps1.Tchi,-5} {ps1.tnam,-15} " +
							$"{schis,5} {fchis,5} {ps1.hosei_l,7:f3} {Str_tim(ps1),5} {ps1.Str_air} {ps1.str_sim}");
						//sout += (" total=" + this.atim.ToString("#").PadLeft(5)); //出力名単位に集計する
					}
					else {
						gsetText.Append(String.Format("P0  {0,-10}\n", ps1.outname));
					}
				}
			}
			int ii = (int)Math.Round(total) / 60;
			int kk = (int)Math.Round(total) % 60;
			if (ii == 0)
				gsetText.Append(String.Format("                                      ----- TOTAL {0:f0} min -----\n", total));
			else
				gsetText.Append(String.Format("                       ----- TOTAL {0:f0} min, {1:d} hr {2:d} min -----\n", total, ii, kk));
			return gsetText;
		}





		/// <summary>
		/// 手順書表題情報[不変]
		/// </summary>
		private class TEJUN_SHEET
		{
			/// <summary>製造番号</summary>
			public readonly string J_NO;
			/// <summary>手順名</summary>
			public readonly string TEJUN_NAME;

			/// <summary>ベースＮＣデータフォーマット</summary>
			public readonly CamUtil.BaseNcForm BASE_FORMAT;
			public readonly string TOOL_SHEET_NAME;

			/// <summary>ユーザー名</summary>
			public string SHAIN_CODE { get { return "tg" + kouban; } }
			private readonly string kouban;

			/// <summary>加工機名</summary>
			public readonly string MACHINE_NAME;
			/// <summary></summary>
			public readonly DateTime KAKO_START;
			/// <summary></summary>
			public readonly string PROCESS_NAME;
			/// <summary></summary>
			public readonly int? PALLET_NO;
			/// <summary></summary>
			public readonly string DANDORI_NAME;
			/// <summary></summary>
			public readonly double? DANDORI_HEIGHT;
			/// <summary></summary>
			public readonly double? SOZAI_X;
			/// <summary></summary>
			public readonly double? SOZAI_Y;
			/// <summary></summary>
			public readonly double? SOZAI_Z;

			// ///////////////////////////////////////////
			// 部品加工で使用していないプロパティをセット
			// ///////////////////////////////////////////

			/// <summary>シミュレーションシステム名 add in 2018/04/20</summary>
			public readonly string sim_system = "";
			/// <summary>シミュレーション加工機名 add in 2018/04/20</summary>
			public readonly string sim_machin = "";
			/// <summary>ＣＡＭ部品名</summary>
			public readonly string dbnam0;
			/// <summary>ユーザー名</summary>
			private readonly string person;
			/// <summary>品名 add in 2018/04/20</summary>
			private readonly string hinmei = "";
			/// <summary>部品名 add in 2018/04/25</summary>
			public readonly string buhin_name = "";

			/// <summary>手順書の行数？？</summary>
			private readonly int ccnt;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="p_ccnt">手順数？</param>
			/// <param name="strLengthErr"></param>
			public TEJUN_SHEET(int p_ccnt, CamUtil.StringLengthDB strLengthErr) {
				J_NO = NcdTool.Tejun.Seba;
				TEJUN_NAME = NcdTool.Tejun.TejunName;
				BASE_FORMAT = NcdTool.Tejun.BaseNcForm;
				// 最初のＮＣデータの工具表名（不要であるが従来との整合性のため設定する）
				TOOL_SHEET_NAME = null;
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll) {
					if (ncnam.nnam != NcdTool.NcName.NcNam.DMY) {
						TOOL_SHEET_NAME = ncnam.tsheet;
						break;
					}
					if (TOOL_SHEET_NAME != null) break;
				}
				kouban = NcdTool.Tejun.Uid.ToString();
				MACHINE_NAME = NcdTool.Tejun.Mach.name;
				//if (NcdTool.Tejun.machname != NcdTool.Tejun.mach.machn.ToString()) throw new Exception("qawefqrf");

				KAKO_START = NcdTool.Tejun.Kdate;
				// 最初のＮＣデータのプロセス名
				PROCESS_NAME = null;
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll) {
					if (ncnam.nnam != NcdTool.NcName.NcNam.DMY) {
						PROCESS_NAME = strLengthErr.MaxRemove(ncnam.Ncdata.ncInfo.xmlD.ProcessName, "ProcsName");
						break;
					}
					if (PROCESS_NAME != null) break;
				}
				if (FormNcSet.casaData != null) {
					switch (NcdTool.Tejun.BaseNcForm.Id) {
					case CamUtil.BaseNcForm.ID.BUHIN:
						PALLET_NO = FormNcSet.casaData.pallet;
						DANDORI_NAME = FormNcSet.casaData.progress;
						DANDORI_HEIGHT = FormNcSet.casaData.baseHeight;
						SOZAI_X = FormNcSet.casaData.mold_X;
						SOZAI_Y = FormNcSet.casaData.mold_Y;
						SOZAI_Z = Math.Round(FormNcSet.casaData.Height - FormNcSet.casaData.baseHeight, 4);
						break;
					default:
						PALLET_NO = null;
						DANDORI_NAME = FormNcSet.casaData.progress + "_" + FormNcSet.casaData.kakoHoko.ToString();
						DANDORI_HEIGHT = null;
						SOZAI_X = null;
						SOZAI_Y = null;
						SOZAI_Z = null;
						break;
					}
				}
				else {
					PALLET_NO = null;
					DANDORI_NAME = null;
					DANDORI_HEIGHT = null;
					SOZAI_X = null;
					SOZAI_Y = null;
					SOZAI_Z = null;
				}

				// 部品加工で使用していないプロパティをセット
				//workzai = NcdTool.Tejun.mzais();
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NcOutput) {
					string system = ncnam.Ncdata.ncInfo.xmlD.SmNAM ?? "&";
					if (sim_system == "") sim_system = system;
					if (sim_system != system) sim_system = "&";
					string machin = ncnam.Ncdata.ncInfo.xmlD.SmMCN ?? "&";
					if (sim_machin == "") sim_machin = machin;
					if (sim_machin != machin) sim_machin = "&";
				}
				dbnam0 = Index.IndexMain == null ? "" : Index.IndexMain.workName;
				if (dbnam0.Length == 0) dbnam0 = "0";
				if (kouban.Length != 0) {
					try { person = ManName(kouban); }
					catch { person = ""; }
					if (person == "")
						person = "&";
					else
						person = person.Replace(' ', '_').Replace('　', '_');
				}
				else person = "&";
				try {
					hinmei = Hinmei(J_NO) ?? "&";
					if (hinmei.Replace(" ", "").Length == 0) hinmei = "&";
				}
				catch { hinmei = "&"; }
				buhin_name = Index.IndexMain == null ? "&" : (Index.IndexMain.buhinName ?? "&");

				ccnt = p_ccnt;

				// 文字のチェック
				CamUtil.StringLengthDB.Moji_SeizoNumb(J_NO);
				CamUtil.StringLengthDB.Moji_TejunName(TEJUN_NAME);
				CamUtil.StringLengthDB.Moji_ToolSheet(TOOL_SHEET_NAME);
				CamUtil.StringLengthDB.Moji_shainCode(SHAIN_CODE);
				CamUtil.StringLengthDB.Moji_MachnName(MACHINE_NAME);
				//CamUtil.StringLengthDB.Moji_ProcsName(PROCESS_NAME);
				CamUtil.StringLengthDB.Moji_DandrName(DANDORI_NAME);

				CamUtil.StringLengthDB.Moji_SimSystem(sim_system);
				CamUtil.StringLengthDB.Moji_SimMachin(sim_machin);
				CamUtil.StringLengthDB.Moji_KoteiCode(dbnam0);
				CamUtil.StringLengthDB.Moji_BuhinName(buhin_name);
			}

			/// <summary>
			/// エクセル貼付け用文字列を作成する
			/// </summary>
			/// <param name="ccnt"></param>
			/// <param name="total_page">手順書内の総加工時間。手順書を分割した場合this.total と異なる数値となる</param>
			/// <param name="p_zais">手順書の材質グループ名</param>
			/// <returns></returns>
			public List<string> Print(int ccnt, double total_page, string p_zais) {
				List<string> outList = new List<string>();
				string ttime0 = total_page.ToString("f0");
				string ttime1 = ((int)Math.Round(total_page) / 60).ToString();
				string ttime2 = ((int)Math.Round(total_page) % 60).ToString();

				outList.Add(ccnt.ToString());
				outList.Add("0.000 0.000 0.000");	// 金型のサイズ
				outList.Add(J_NO + " " + TEJUN_NAME + " " + p_zais + " " + sim_system + " " + sim_machin);
				outList.Add(
					dbnam0 + " " + "0" + " " + MACHINE_NAME + " " +
					ttime0 + " " + ttime1 + " " + ttime2 + " " +
					person + " " + hinmei + " " + buhin_name);
				return outList;
			}
		}

		/// <summary>
		/// 手順ＤＢと工具ＤＢのリンク[不変]
		/// </summary>
		private class TEJUN_TOOL
		{
			//public int TSHEET_ID { get; private set; }
			public readonly TejunSet.ToolSheet TSHEET;
			public readonly string ORIGINAL_J_NO;
			public readonly string ORIGINAL_TEJUN;
			public readonly string TSHEET_NAME;

			public TEJUN_TOOL(TejunSet.ToolSheet tsheet) {
				//TSHEET_ID = (SELECT TSHEET_ID FROM dbo.TOOL_SHEET_HEAD WHERE (J_NO = tset.tolstData.ktejunSeba) AND (TSHEET_NAME = tset.tolstName))
				TSHEET = tsheet;
				ORIGINAL_J_NO = tsheet.TolstData.KtejunSeba;
				ORIGINAL_TEJUN = tsheet.TolstData.KtejunName;
				TSHEET_NAME = tsheet.TolstName;
				//文字のチェック（名前なしの工具表はチェックしない）
				if (TSHEET_NAME.Length > 0) {
					CamUtil.StringLengthDB.Moji_SeizoNumb(ORIGINAL_J_NO);
					CamUtil.StringLengthDB.Moji_TejunName(ORIGINAL_TEJUN);
					CamUtil.StringLengthDB.Moji_ToolSheet(TSHEET_NAME);
				}
			}
		}

		/// <summary>
		/// 加工手順内の１工具単位の情報[不変]
		/// </summary>
		private class PROCESS_SHEET
		{
			/// <summary>コメント。nullでない場合はテキスト出力で区切ること</summary>
			public readonly string comnt;
			/// <summary>ＮＣ出力名</summary>
			public readonly string outname;
			/// <summary>編集名</summary>
			public readonly string hens;

			/// <summary>ツールセット名</summary>
			public readonly string tset;
			/// <summary>工具表ページ番号</summary>
			public readonly int snum;
			/// <summary>Ｔ番号</summary>
			public readonly int tnum;
			/// <summary>Ｏ番号</summary>
			public readonly int onum;
			/// <summary>Ｔ番号を表示する場合はtrue、Ｏ番号を表示する場合はfalse</summary>
			public readonly bool TorO;
			/// <summary>ＮＣデータの共有の有無</summary>
			public readonly bool kyoy;

			/// <summary>減速考慮切削加工時間(min)</summary>
			public readonly double rtim;
			/// <summary>減速考慮全加工時間(min)</summary>
			public readonly double ktim;
			/// <summary>寿命割合</summary>
			public readonly double life;

			/// <summary>使用工具の名前</summary>
			public readonly string tnam;
			/// <summary>使用工具の直径</summary>
			public readonly double diam;
			/// <summary>回転数</summary>
			public readonly double schi;
			/// <summary>送り速度</summary>
			public readonly double fchi;

			/// <summary>シミュレーション結果</summary>
			public readonly string str_sim;
			/// <summary>ＣＡＭ出力コメント add in 2017/11/16</summary>
			public readonly string str_com;
			/// <summary>３軸５軸加工区分 add in 2018/04/20</summary>
			public readonly string axis_num;
			/// <summary>回転軸の割出し/同時の区分 add in 2018/04/20</summary>
			public readonly string axis_typ;

			/// <summary>加工方向</summary>
			public readonly string kh_direct;
			/// <summary>加工法Ｎｏ（未使用）</summary>
			public readonly string kh_method;
			/// <summary>次元？</summary>
			public readonly string kh_dimen1;
			/// <summary>残し量Ｘ</summary>
			public readonly double kh_nokosX;
			/// <summary>残し量Ｚ（未使用）</summary>
			public readonly double kh_nokosZ;
			/// <summary>次元？</summary>
			public readonly string kh_dimen2;
			/// <summary>切込み量Ｘ</summary>
			public readonly double kh_pitchX;
			/// <summary>切込み量Ｚ</summary>
			public readonly double kh_pitchZ;

			/// <summary>工具径補正量</summary>
			public readonly double hosei_r;
			/// <summary>工具長補正量</summary>
			public readonly double hosei_l;

			/// <summary>材質名</summary>
			public readonly string workzai;
			/// <summary>関連する工具表番号</summary>
			public readonly int tshtNo;


			/// <summary>エアーカット比（%付き）</summary>
			public string Str_air {
				get {
					if (Math.Abs(ktim) > NcdTool.NcName.NcNam.FLMIN)
						return ((double)((1.0 - rtim / ktim) * 100.0)).ToString("f0").PadLeft(3) + "%";
					else return "&".PadLeft(4);
				}
			}
			public string Tchi {
				get {
					if (tnum == 0 && onum == 0) return "&";
					if (TorO)
						return "T" + tnum.ToString("00");
					else
						return "O" + onum.ToString("0000");
				}
			}
			/// <summary>残し量の文字列</summary>
			public string NkNokosi {
				get {
					if (this.kh_direct == null) return nkPIC;
					return (this.kh_nokosX.ToString("0.000") + "&&").PadLeft(11, '&');
				}
			}
			/// <summary>ピック量の文字列</summary>
			public string NkPick {
				get {
					if (this.kh_direct == null) return nkPIC;
					if (Math.Abs(this.kh_pitchX - this.kh_pitchZ) < 0.005 && this.kh_pitchX != 0.0) {
						return (this.kh_pitchX.ToString("0.00") + "&&&").PadLeft(11, '&');
					}
					else {
						string khoho8 = this.kh_pitchX.ToString("0.00");
						string khoho9 = this.kh_pitchZ.ToString("0.00");
						if (khoho8 == "0.00") khoho8 = "&&&&&";
						if (khoho9 == "0.00") khoho9 = "&&&&&";
						return khoho8.PadLeft(5, '&') + "|" + khoho9.PadRight(5, '&');
					}
				}
			}

			/// <summary>
			/// ディープコピー（==ナローコピー）によりクローンを作成します。
			/// </summary>
			/// <returns></returns>
			public PROCESS_SHEET Clone() { return (PROCESS_SHEET)this.MemberwiseClone(); }

			/// <summary>
			/// 手順書１行のデータの保存（情報あり）
			/// </summary>
			/// <param name="bncf"></param>
			/// <param name="ncsd"></param>
			/// <param name="skog"></param>
			/// <param name="smch"></param>
			/// <param name="strLengthErr"></param>
			/// <param name="p_tshtNo"></param>
			public PROCESS_SHEET(CamUtil.BaseNcForm bncf, NcdTool.NcName.NcNam ncsd, NcdTool.NcName.Kogu skog, NcdTool.TMatch.MatchK smch, CamUtil.StringLengthDB strLengthErr, int p_tshtNo) {
				NcdTool.ToolSetInfo.TSetCAM tsetCAM;
				tsetCAM = new NcdTool.ToolSetInfo.TSetCAM(skog.Tld.XmlT.SNAME);

				// コメント、出力名、編集名
				comnt = null;
				if (skog != null && smch != null && ncsd.nmgt.Comnt != null)
					if (skog.TNo == 0 && smch.no == 0 && ncsd.nmgt.Comnt.Length > 0) comnt = ncsd.nmgt.Comnt;
				this.outname = CamUtil.ServerPC.PTPName.FileNameTrim(bncf, skog.Oname(smch));
				this.hens = "&";
				if (ncsd.Ncdata.ncInfo.xmlD.HENSHU_NAME != null)
					if (ncsd.Ncdata.ncInfo.xmlD.HENSHU_NAME.Length != 0) {
						this.hens = strLengthErr.MaxRemove(ncsd.Ncdata.ncInfo.xmlD.HENSHU_NAME.Replace(' ', '_'), "CoordName");
					}

				// 工具番号, Ｏ番号
				tset = ncsd.tsheet;
				snum = smch.SnumN;
				tnum = smch.K2.Tnum;
				onum = smch.K2.Onum;
				TorO = NcdTool.Tejun.Mach.Toool_nc || NcdTool.Tejun.Mach.Nmgz > 1;
				kyoy = ncsd.Jnno.Nknum != null;

				// 加工時間
				ktim = smch.Atim;
				if (bncf.Id == CamUtil.BaseNcForm.ID.BUHIN) {
					ktim = smch.Atim * tsetCAM.Gensoku;
				}
				rtim = smch.Rtim > 0.01 ? smch.Rtim : 0.0;
				life = smch.divData.Consumption;

				// 工具と加工条件
				tnam = skog.TsetCHG.Tset_tool_name;
				if (Math.Abs(skog.TsetCHG.Tset_diam - smch.K2.Tlgn.Toolset.Diam) > 0.01) throw new Exception("暫定処置");
				diam = smch.K2.Tlgn.Toolset.Diam;
				schi = skog.CutSpinRate();
				fchi = skog.CutFeedRate();

				// その他
				str_sim = skog.Tld.XmlT.SmCLC ?? "&";
				str_com = "&";
				if (skog.Tld.XmlT.CMMNT != null && skog.Tld.XmlT.CMMNT.Length > 0) {
					str_com = strLengthErr.MaxRemove(skog.Tld.XmlT.CMMNT, "CamCommnt");
				}
				axis_num = skog.Tld.XmlT.Keisha ? "５軸" : "３軸";
				axis_typ = skog.Tld.XmlT.SimultaneousAxisControll ? "同時" : "割出";
				workzai = ncsd.nmgt.SetZais;

				// 加工条件
				kh_direct = skog.Tld.XmlT.CTDIR ?? "0";
				kh_method = skog.Tld.XmlT.METHD ?? "0";
				kh_dimen1 = "3";
				kh_nokosX = skog.Tld.XmlT.NOKOS ?? 0.0;
				kh_nokosZ = skog.Tld.XmlT.NOKOS ?? 0.0;
				kh_dimen2 = "3";
				kh_pitchX = skog.Tld.XmlT.XPICK ?? 0.0;
				kh_pitchZ = skog.Tld.XmlT.ZPICK ?? 0.0;
				// 工具径補正量 add in 2019/08/09
				if (tsetCAM.KouteiType == "タップ" && NcdTool.Tejun.Mach.Dmu == false)
					hosei_r = 0.0;            // DMU200P以外のタップの径補正は消去する
				else
					hosei_r = tsetCAM.RadRevision ?? 0.0;
				// 工具長補正量 add in 2019/08/09
				hosei_l = ncsd.nggt.ToolLengthHosei.ValueHosei(skog);
				if (hosei_l == 0.0)
					hosei_l = tsetCAM.LenRevision ?? 0.0;

				tshtNo = p_tshtNo;

				// 文字のチェック
				CamUtil.StringLengthDB.Moji_NcOutName(this.outname);
				CamUtil.StringLengthDB.Moji_TejunComm(this.comnt);
				//CamUtil.StringLengthDB.Moji_HenshName(this.hens);
				CamUtil.StringLengthDB.Moji_ToolgName(this.tnam);
				CamUtil.StringLengthDB.Moji_SimlKekka(this.str_sim);
				//CamUtil.StringLengthDB.Moji_CamCommnt(this.str_com);
				CamUtil.StringLengthDB.Moji_AxisNumbr(this.axis_num);
				CamUtil.StringLengthDB.Moji_AxisTypeS(this.axis_typ);
				CamUtil.StringLengthDB.Moji_KH_Direct(this.kh_direct);
				CamUtil.StringLengthDB.Moji_KH_Method(this.kh_method);
				CamUtil.StringLengthDB.Moji_KH_dimns1(this.kh_dimen1);
				CamUtil.StringLengthDB.Moji_KH_dimns2(this.kh_dimen2);

				CamUtil.StringLengthDB.Moji_MaterialG(this.workzai);
			}

			/// <summary>
			/// １行のデータの保存（情報なし）
			/// </summary>
			/// <param name="bncf"></param>
			/// <param name="ncsd"></param>
			/// <param name="skog"></param>
			/// <param name="smch"></param>
			/// <param name="p_tshtNo"></param>
			public PROCESS_SHEET(CamUtil.BaseNcForm bncf, NcdTool.NcName.NcNam ncsd, NcdTool.NcName.Kogu skog, NcdTool.TMatch.MatchK smch, int p_tshtNo) {
				// コメント、出力名、編集名
				comnt = null;
				if (skog != null && smch != null && ncsd.nmgt.Comnt != null)
					if (skog.TNo == 0 && smch.no == 0 && ncsd.nmgt.Comnt.Length > 0) comnt = ncsd.nmgt.Comnt;
				if (skog != null && smch != null)
					this.outname = CamUtil.ServerPC.PTPName.FileNameTrim(bncf, skog.Oname(smch));
				else
					outname = ncsd.nnam;
				hens = "&";

				// 工具番号, Ｏ番号
				tset = null;
				snum = 0;
				tnum = 0;
				onum = 0;
				TorO = false;
				kyoy = ncsd.Jnno.Nknum != null;

				// 加工時間
				ktim = rtim = life = 0.0;

				// 工具と加工条件
				tnam = "&";
				diam = schi = fchi = 0.0;

				// その他
				str_sim = str_com = axis_num = axis_typ = "&";
				workzai = ncsd.nmgt.SetZais;

				// 加工条件
				kh_direct = kh_method = null;
				kh_dimen1 = kh_dimen2 = "3";
				kh_nokosX = kh_nokosZ = kh_pitchX = kh_pitchZ = 0.0;

				tshtNo = p_tshtNo;

				// 文字のチェック
				CamUtil.StringLengthDB.Moji_NcOutName(this.outname);
				CamUtil.StringLengthDB.Moji_TejunComm(this.comnt);

				return;
			}
		}
	}
}
