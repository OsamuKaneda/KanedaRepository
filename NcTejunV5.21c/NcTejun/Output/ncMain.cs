using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace NcTejun.Output
{
	class NcMain : IDisposable
	{
		/// <summary>入力するＮＣデータの最初の５行のチェック用正規表現</summary>
		static private Regex[] tnget = new Regex[5];
		/// <summary>全行コメントの正規表現</summary>
		static private Regex tncmm;

		/// <summary>
		/// static のコンスタラクタ
		/// </summary>
		static NcMain() {
			tnget[0] = new Regex("^G100T");
			tnget[1] = new Regex("^G100T[0-9]+(I[0-9]+){0,1}[^ISGOT]*S[0-9]+(\\(.*\\)){0,1}$");
			tnget[2] = new Regex("^G00G90X[-.0-9]+Y[-.0-9]+(Z[-.0-9]+){0,1}$");
			tnget[3] = new Regex("^(X[-.0-9]+){0,1}(Y[-.0-9]+){0,1}$");
			tnget[4] = new Regex("^Z[-.0-9]+$|^G6|^G8");
			tncmm = new Regex("^[(;]");
		}

		/// <summary>
		/// ＮＣデータのチェック
		/// </summary>
		/// <param name="ddat">ＮＣデータ行</param>
		/// <param name="iget">ＮＣデータの行ナンバー</param>
		/// <returns></returns>
		static public void Nctchk(ref int iget, string ddat) {

			// 工具交換後４行までチェック
			if (iget >= tnget.Length) return;

			// G100Tの行を見つけたらチェックを開始
			if (iget == 0) {
				if (NcMain.tnget[0].Match(ddat).Success == false)
					return;
				iget++;
			}

			// 全行コメントの場合そのまま終了（次の行でチェック）
			if (NcMain.tncmm.IsMatch(ddat)) return;
			// 工具軸設定の場合そのまま終了（次の行でチェック）
			if (ddat.IndexOf("G65P9697") == 0) return;

			if (NcMain.tnget[iget].IsMatch(ddat) == false) {
				MessageBox.Show(
					$"ＮＣデータの開始{((int)(iget + 2)).ToString()}行目 \"{ddat}\" はGENERALのフォーマット \"{NcMain.tnget[iget].ToString()}\" に適合していません。");
				// add 2010/05/21
				throw new Exception(
					$"ＮＣデータの開始{((int)(iget + 2)).ToString()}行目 \"{ddat}\" はGENERALのフォーマット \"{NcMain.tnget[iget].ToString()}\" に適合していません。");
			}

			iget++;
			return;
		}

		// //////////////
		// 以上、static
		// //////////////






		/// <summary>出力するＮＣデータの情報</summary>
		public NcdTool.NcName.NcNam npntr;
		/// <summary>出力する工具単位データ。チェックのために追加（Count != toolList[0].ncnam.itdat）</summary>
		private List<Output.NcOutput.NcToolL> toolList;
		/// <summary>工程間ＣＬ接続の有無を調べる</summary>
		public bool[] Setsuzoku = new bool[] { false };
		/// <summary>ＮＣデータの行数</summary>
		public int NcLength { get { return ncsr2.MaxLineNo; } }

		CamUtil.LCode.StreamNcR2 ncsr2;
		StreamWriter nco;   // 新ＮＣ変換の出力先
		readonly string outsuff;

		/// <summary>マクロ展開のクラス</summary>
		NcRead._main_nctnk nctnk = null;

		public NcMain(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach, string outname) {
			this.toolList = toolList;
			this.npntr = toolList[0].Ncnam;
			this.outsuff = outname;

			//	toolList の数は出力する工具単位数に等しい
			// ＮＣデータに出力しない工具（tdat[].output==false）が含まれる場合でも、NcRead.SimSpinFeed() によって削除される
			if (toolList.Count != toolList[0].Ncnam.Tdat.FindAll(skog => skog.Output).Count)
				throw new Exception("toolList の数とＮＣデータより出力する工具数の数が異なる");

			ncsr2 = new CamUtil.LCode.StreamNcR2(npntr.Ncdata.fulnamePC, Nctchk,
					(CamUtil.LCode.INcConvert)new NcRead.SimSpinFeed(toolList[0].Ncnam, true, Setsuzoku),
					(CamUtil.LCode.INcConvert)new NcRead.Conv_NcRun1(toolList, mach),
					(CamUtil.LCode.INcConvert)new NcRead.Conv_NcRun2(toolList, mach, nctnk),
					(CamUtil.LCode.INcConvert)new NcRead.Conv_NcRun3(toolList, mach),
					(CamUtil.LCode.INcConvert)new NcRead.Conv_NcRun4(toolList, mach),
					(CamUtil.LCode.INcConvert)new NcRead.Conv_NcRun5(toolList, mach));
		}

		/// <summary>
		/// ＮＣデータ変換のメインルーチン
		/// </summary>
		/// <param name="disp_message"></param>
		/// <param name="mach"></param>
		/// <returns>0:正常終了 1:異常終了</returns>
		public int Ncmain(Label disp_message, NcdTool.Mcn1 mach) {
			int ii;

			//NcCode.nccode.NcMachine.ParaInit(mach.ID);
			//NcCode.nccode.Post.init();

			// ＮＣデータ変換のメインループ
			try {
				nco = null;
				ii = Ncread2(disp_message);
			}
			catch (Exception ex) {
				CamUtil.LogOut.CheckCount("ncMain 118", false, npntr.nnam + " " + ex.Message);
				throw;
			}
			return ii;
		}

		/// <summary>
		/// ＮＣデータ変換のメインループnew
		/// </summary>
		private int Ncread2(Label disp_message) {

			string ddat2;
			bool? rireki = null;
			bool hyouji;

			//int iget = 0;
			int icnt;

			int tno1 = -1;	// 工具位置（入力）
			int tno2 = -1;	// 工具位置（出力）
			int pers = 1;

			while (true) {
				//　ＮＣデータの読み込み（新しいルーティン）
				// ddat, ddattmp はシミュレーションのコメント整理と回転数・送り速度の比率加味後のデータ
				ddat2 = ncsr2.ReadLine();
				if (ddat2 == null) break;

				icnt = (int)ncsr2.lastConv[0].LnumN;

				// /////////////////
				// 変換履歴の表示
				// /////////////////
				if (CamUtil.ProgVersion.Debug && rireki.HasValue) {
					hyouji = false;
					if (rireki.Value) hyouji = true;
					if (ncsr2.lastConv[0].B_g100) hyouji = true;
					if (ncsr2.lastConv[0].B_g6) hyouji = true;
					if (ncsr2.lastConv[0].B_g8) hyouji = true;
					if (ncsr2.lastConv[0].B_26('M')) hyouji = true;
					if (ncsr2.lastConv[ncsr2.lastConv.Length - 1].OutLine.Comment.Length > 0) hyouji = true;
					if (icnt <= 10 || icnt >= ncsr2.MaxLineNo - 5) hyouji = true;
					if (hyouji) {
						string ss = "N = " + icnt.ToString("000000");
						int ii = 0;
						foreach (CamUtil.LCode.NcLineCode nlc in ncsr2.lastConv) {
							ss += "\n" + (ii == 0 ? "input0" : ("ncrun" + ii.ToString())) + " : " + nlc.OutLine.NcLineOut().Replace("\r\n", "; ");
							ii++;
						}
						DialogResult result = MessageBox.Show(ss.Substring(1), "変換履歴の表示", MessageBoxButtons.YesNoCancel);
						if (result == DialogResult.Cancel) rireki = null;
						if (result == DialogResult.Yes) rireki = true;
						if (result == DialogResult.No) rireki = false;
					}
				}

				// ＮＣデータのファイルへの保存（新）
				if (ddat2[0] == '%') {
					if (nco == null) {
						// 出力ファイル内の工具単位情報のチェック
						int next;
						do {
							tno1++;
							tno2++;
							while (npntr.Tdat[tno1].Output == false) tno1++;	// 出力すべきＮＣデータは１つは存在するのでエラーにならない
							if (npntr.Tdat[tno1] != toolList[tno2].Skog) throw new Exception("Program Error in _ncMain");
							// 次のＮＣ
							next = tno1; while (++next < npntr.Itdat) if (npntr.Tdat[next].Output) break;
						} while (next < npntr.Itdat && npntr.Tdat[next].MatchK0.Ochg == false);

						// 出力ファイル名の設定と出力
						nco = new StreamWriter(npntr.Tdat[tno1].Oname() + outsuff);
						nco.WriteLine(ddat2);
					}
					else {
						nco.WriteLine(ddat2);
						nco.Close();
						nco = null;
					}
				}
				else
					nco.WriteLine(ddat2);

				// ディスクトップの更新
				if ((double)icnt / ncsr2.MaxLineNo > pers / 100.0) {
					disp_message.Text = String.Format("ＮＣデータの変換中（{0}  {1:00}%）", npntr.nnam, pers);
					pers++;
					Application.DoEvents();
				}
			}
			// 出力工具単位ＮＣデータ数のチェック
			tno1++;
			tno2++;
			while (tno1 < npntr.Itdat && npntr.Tdat[tno1].Output == false) tno1++;
			if (tno1 != npntr.Itdat || tno2 != toolList.Count) throw new Exception("afvafdvadfvaf");

			//if (NcRun.start == 0 || NcRun.start == 1)
			//	NcCode.__main.Error.ncerr(3, "NC-DATA is not complete.\n");

			if (nco != null) throw new Exception("qjhefbqrbfh");
			return 0;
		}

		public void Dispose() {
			if (ncsr2 != null) { ncsr2.Dispose(); ncsr2 = null; }
			if (nco != null) { nco.Dispose(); nco = null; }
			if (nctnk != null) { nctnk.Dispose(); nctnk = null; }
		}
	}
}
