using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using CamUtil;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	/// <summary>
	/// 主型用ＮＣデータの変換。文字列の変換１
	/// </summary>
	class Conv_NcRun1 : Conv
	{
		/// <summary>入力するＮＣデータの最初の５行のチェック用正規表現</summary>
		static private Regex[] tnget = new Regex[5];
		/// <summary>全行コメントの正規表現</summary>
		static private Regex tncmm;

		/// <summary>
		/// static のコンスタラクタ
		/// </summary>
		static Conv_NcRun1() {
			tnget[0] = new Regex("^G100T");
			//tnget[1] = new Regex("^G100T[0-9]+(I[0-9]+){0,1}[^ISGOT]*S[0-9]+;");
			tnget[1] = new Regex("^G100T[0-9]+(I[0-9]+){0,1}[^ISGOT]*S[0-9]+(\\(.*\\)){0,1};");
			tnget[2] = new Regex("^G00G90X[-.0-9]+Y[-.0-9]+(Z[-.0-9]+){0,1};");
			tnget[3] = new Regex("^(X[-.0-9]+){0,1}(Y[-.0-9]+){0,1};");
			//tnget[4] = new Regex("^Z[-.0-9]+;");
			tncmm = new Regex("^[(;]");
		}

		/// <summary>
		/// ＮＣデータのチェック
		/// </summary>
		/// <param name="ddat">ＮＣデータ行</param>
		/// <param name="iget">ＮＣデータの行ナンバー</param>
		/// <returns></returns>
		static private int Nctchk(ref int iget, string ddat) {

			// G100Tの行を見つけたらチェックを開始
			if (iget == 0) {
				if (Conv_NcRun1.tnget[0].Match(ddat).Success == false)
					return 0;
				iget++;
			}

			// 全行コメントの場合そのまま終了（次の行でチェック）
			if (Conv_NcRun1.tncmm.IsMatch(ddat)) return 0;

			if (Conv_NcRun1.tnget[iget].IsMatch(ddat) == false) {
				System.Windows.Forms.MessageBox.Show(
					$"ＮＣデータの開始{((int)(iget + 2)).ToString()}行目 \"{ddat}\" はGENERALのフォーマット \"{Conv_NcRun1.tnget[iget].ToString()}\" に適合していません。");
				// add 2010/05/21
				throw new Exception(
					$"ＮＣデータの開始{((int)(iget + 2)).ToString()}行目 \"{ddat}\" はGENERALのフォーマット \"{Conv_NcRun1.tnget[iget].ToString()}\" に適合していません。");
			}

			iget++;
			if (iget == 4)
				iget = 0;
			return 0;
		}

		// //////////////
		// 以上、static
		// //////////////



		private static Regex regG41G66 = new Regex("G[46][126].*D");
		private static Regex regG41 = new Regex("G4[12]");
		private static Regex regG4012 = new Regex("G4[012][A-Z;]");
		private static Regex regG39 = new Regex("^G39[A-Z]");
		private static Regex regG05 = new Regex("G0*1[A-Z;]");

		/// <summary>加工機情報</summary>
		NcdTool.Mcn1 mach;
		/// <summary>G05の挿入に使用</summary>
		bool g05;

		/// <summary>入力ＮＣのチェックに使用</summary>
		int iget;

		//sed0s(0, ServerPC.SvrFldrM + @"GENERAL\EDITSTT");
		//a:1:;:s:;.*$:;:
		//a:1: :s: +::g
		//a:1:G83:Q:Q[0-9.]*::
		//a:1:G65P8200:Q:Q[0-9.]*::
		//a:1:G66P8200:Q:Q[0-9.]*::
		//a:3: :s: +::g
		//b:1:G100T:T:
		//c:1:G100T:s:T[0-9]*:T$t:
		//c:1:G100T:s:S[0-9]*:S$s:
		//c:1:G00G90X,G100T,P-1:s:X.*:$x;:
		//c:1:G100T00*[A-Z;],M98P0*6[A-Z;(]:d
		//d:1:^M30;:s:M30;:M99;:
		//e:1:M98P0*6[A-Z;(]:i:G00$c;:
		//e:1:M98P0*6[A-Z;(]:i:$p;:
		//f:1:M98P0*6[A-Z;(]:i:G00$z;:
		//f:1:M98P0*6[A-Z;(]:i:$x;:
		//b:1:G[46][126].*D:s:D[0-9]*:D$d:
		//b:3:G100T:M:M01;
		//g:1:G0*1[A-Z;],,N1:a:G05;:
		//g:1:%,M98P9699,P-3:i:G05;:
		//g:1:%,G65P9699,P-3:i:G05;:
		//g:1:M98P0*6[A-Z;(]:i:G91X0Y0Z0;:
		//g:5:G[78][0-9][A-Z;]:E:KOUSOKU-MODE:
		//h:1:G4[12]:s:[IJKD]-*[0-9.]*::g
		//h:1:G4[012][A-Z;]:s:G4[012]::
		//h:1:^G39[A-Z]:d
		//k:1:G65:K
		//k:1:G66:K

		//sed0s(1, Program.tejun.mach.mcn2.sedf[0]);
		// -- MCC2013 --
		//1:\(:s:\([^)]*\)::g
		//1:^G39[A-Z]:d
		//3:G100:i:#527=11.;:
		//3:G100T:s:U[-0-9.]*K[-0-9.]*W[-0-9.]*S:S:
		//3:G4[12][A-Z;]:s:[IJK][-IJK0-9.]*::
		//3:G40M98P0*6[A-Z;(]:s:G40::
		//3:^[^XY]*Z[0-9],!Z,P-1:m:M100;
		//5:G05:s:;:P10000;:
		//5:M98P0*6[;A-Z(]:i:G05P0;:
		//5:M100,G100,P-4:d
		//5:M100,M98P0*6[A-Z;(],P+1:d
		//5:M100,M98P0*6[A-Z;(],P+2:d
		//6:^O:s:;:($%);:
		// -- MCC2013_G01 追加 --
		//0:G0*1[A-EG-Z][^F]*;:E:
		//6:G00*[XYZ;]:s:;:F16000;:
		//6:G00*[XYZ;]:s:G00*:G01:
		//6:^F:s:^:G01:

		//sed0s(1, ServerPC.SvrFldrM + @"GENERAL\EDITEND");
		//i:4:%,M0*3;,P-1:a:$r:
		//i:4:M0*5;:i:$R:
		//b:6:^;:d

		public Conv_NcRun1(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach)
			: base(5, false, null, true, toolList) {

			this.mach = mach;
			this.iget = 0;
		}

		public override OutLine ConvExec() {
			NcLineCode txtd = this.NcQue[0];

			// 入力されたＮＣデータのG100以下３行のチェック
			Conv_NcRun1.Nctchk(ref iget, txtd.NcLine + ";");

			// M98P969への変換チェック
			if (ClLink5Axis.Exists_G(txtd.NcLine))
				throw new Exception("Ｇ６５Ｐ９６９はＭ９８Ｐ９６９に変換されているはずです。");

			if (txtd.B_g100)
				g05 = false;

			// /////////
			// EDITSTT
			// /////////
			if (0 == 0) Sed_a(txtd);					// abzc
			if (ncname.Nmgt.Dimn == '3') Sed_h(txtd);	// h
			if (ncname.Nmgt.Dimn == '3') {
				if (mach.ncCode.HSP_ON != null) {
					if (mach.ncCode.HSP_OFF == "G91X0Y0Z0")
						Sed_g(txtd, mach.ncCode.HSP_ON, null, mach.ncCode.HSP_OFF);		// g
					else {
						LogOut.CheckCount("Conv_NcRun1 117", false, "不要であるが旧のルーチンとの整合を取るために挿入する暫定処置");
						Sed_g(txtd, mach.ncCode.HSP_ON, "G91X0Y0Z0", mach.ncCode.HSP_OFF);		// g
					}
				}
				else if (mach.ID == Machine.MachID.V77) {
					LogOut.CheckCount("Conv_NcRun1 123", false, "不要であるが旧のルーチンとの整合を取るために挿入する暫定処置");
					Sed_g(txtd, null, null, "G05P0");		// g
				}
			}
			else {
				// FNC208 以外の以下の加工機では２次元加工時も最後の"G05P0"を出力している
				// 将来は削除する
				switch (mach.ID) {
				case Machine.MachID.MCC2013:
				case Machine.MachID.YBM1218V:
				case Machine.MachID.MCC3016VG:
				case Machine.MachID.V77:
				case Machine.MachID.KENSYO:
					Sed_g(txtd, null, null, "G05P0");		// g
					break;
				}
			}
			if (ncname.Nggt.rev) Sed_k(txtd);			// k

			// /////////
			// Machine
			// /////////

			//1:\(:s:\([^)]*\)::g コメントを削除（済）
			txtd.OutLine.DelelseComment();
			//1:^G39[A-Z]:d
			if (regG39.Match(txtd.NcLine).Success) {
				LogOut.CheckCount("Conv_NcRun1 154", false, "G39 Exists");
				txtd.OutLine.Set("");
			}

			switch (mach.ID) {
			case Machine.MachID.DMU200P:
			case Machine.MachID.DMU210P:
			case Machine.MachID.DMU210P2:
				break;
			default:
				if (txtd.NcLine.IndexOf(ClLink5Axis.Kotei) >= 0)
					LogOut.CheckCount("Conv_NcRun1 164", false, "工程間移動あり " + mach.name);
				break;
			}
			if (ClLink5Axis.KOTEI_IDO(mach.ID) == false) {
				//1:G65P9699:E:
				if (txtd.NcLine.IndexOf(ClLink5Axis.Kotei) >= 0)
					throw new Exception(mach.name + "は工程間移動のＮＣデータには対応していません in Conv");
			}
			// /////////
			// EDITEND
			// /////////

			return txtd.OutLine;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="txtd"></param>
		private void Sed_a(NcLineCode txtd) {
			string ddat = txtd.OutLine.Txt;

			// a:1:;:s:;.*$:;:
			if (ddat.IndexOf(";") >= 0)
				ddat = Substitute(ddat, new Regex(";.*$"), "", false);

			// a:1: :s: +::g
			if (ddat.IndexOf(" ") >= 0) {
				LogOut.CheckCount("Conv_NcRun1 243", false,
					$"スペースが見つかりましたので削除します。 入力 = {ddat}  削除後 = {Substitute(ddat, new Regex(" +"), "", true)}");
				ddat = Substitute(ddat, new Regex(" +"), "", true);
			}

			// 廃止
			// a:1:G83:Q:Q[0-9.]*::
			// a:1:G65P8200:Q:Q[0-9.]*::
			// a:1:G66P8200:Q:Q[0-9.]*::

			// b:1:G100T:T:
			// c:1:G100T:s:T[0-9]*:T$t:
			// c:1:G100T:s:S[0-9]*:S$s:
			if (ddat.IndexOf("O") == 0) ddat = "";
			if (txtd.B_g100) {
				if (NcoutName.Skog.MatchK0.Ochg) {
					if (txtd.Tcnt > 0) {
						txtd.OutLine.MaeAdd("M30");
						txtd.OutLine.MaeAdd("%");
						txtd.OutLine.MaeAdd("%");
					}
					//6:^O:s:;:($%);:	５から移動
					txtd.OutLine.MaeAdd("O" + NcoutName.Skog.MatchK0.K2.Onum.ToString("0000") + "(" + NcoutName.Outnam + ")");
				}
				ddat = NcLineCode.NcSetValue(ddat, 'T', NcoutName.Skog.MatchK0.K2.Tnum.ToString("00"));
				ddat = NcLineCode.NcSetValue(ddat, 'S', NcoutName.Skog.CutSpinRate().ToString("00000"));
			}

			// c:1:G00G90X,G100T,P-1:s:X.*:$x;:
			if (this.NcQue[0].NcLine.IndexOf("G00G90X") >= 0 && this.NcQue[-1].B_g100) {
				ddat = "G00G90" +
					"X" + ((double)ncname.Ncdata.ncInfo.xmlD.OriginX * 1000.0).ToString("f0") +
					"Y" + ((double)ncname.Ncdata.ncInfo.xmlD.OriginY * 1000.0).ToString("f0");
			}

			/* SimSpinFeed に移動
			// c:1:G100T00*[A-Z;],M98P0*6[A-Z;(]:d
			if (txtd.b_g100 && ncoutName.skog.matchK0.K2.tnum == 0) t00 = true;
			if (t00) ddat = "";
			if (SimSpinFeed.regP6.Match(txtd.ncLine + ";").Success) t00 = false;
			*/

			// b:1:G[46][126].*D:s:D[0-9]*:D$d:
			if (regG41G66.Match(txtd.NcLine).Success) {
				// 上記の従来の変換は整数になるように見えるが実は".0"があった場合Regexでマッチしないため小数点以下は残る。よって、以下で正解 2019/05/31
				//ddat = NcLineCode.NcSetValue2(ddat, 'D', (NcoutName.Skog.MatchK0.K2.Tnum + mach.ncCode.DIN).ToString("00"));
				ddat = NcLineCode.NcSetValue(ddat, 'D', (NcoutName.Skog.MatchK0.K2.Tnum + mach.ncCode.DIN).ToString(txtd.Code('D').decim ? "00.0" : "00"));
			}

			txtd.OutLine.Set(ddat);
			return;
		}

		/// <summary>
		/// ２次元の工具径補正付きのフライス加工を３次元加工で使用可能にする。
		/// </summary>
		/// <remarks>
		/// 現在、２Ｄから３Ｄへの変換を認めていないため不要と思われる
		/// </remarks>
		/// <param name="txtd"></param>
		private void Sed_h(NcLineCode txtd) {
			if (regG4012.Match(txtd.NcLine + ";").Success) {
				LogOut.CheckCount("Conv_NcRun1 324", false, "工具径オフセット付きＮＣデータを３Ｄ加工に使用します。");
			}
			string ddat = txtd.OutLine.Txt;

			// h:1:G4[12]:s:[IJKD]-*[0-9.]*::g
			if (regG41.Match(ddat).Success) {
				if (txtd.B_26('I')) ddat = NcLineCode.NcDelChar(ddat, 'I');
				if (txtd.B_26('J')) ddat = NcLineCode.NcDelChar(ddat, 'J');
				if (txtd.B_26('K')) ddat = NcLineCode.NcDelChar(ddat, 'K');
				if (txtd.B_26('D')) ddat = NcLineCode.NcDelChar(ddat, 'D');
			}

			// h:1:G4[012][A-Z;]:s:G4[012]::
			if (regG4012.Match(txtd.NcLine + ";").Success) {
				ddat = Substitute(ddat, new Regex("G4[012]"), "", false);
			}

			// h:1:^G39[A-Z]:d
			if (regG39.Match(txtd.NcLine).Success)
				ddat = "";

			txtd.OutLine.Set(ddat);
			return;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="txtd"></param>
		/// <param name="on">高速加工モードＯＮのコード</param>
		/// <param name="off1">高速加工モードＯＦＦのコード１</param>
		/// <param name="off2">高速加工モードＯＦＦのコード２</param>
		private void Sed_g(NcLineCode txtd, string on, string off1, string off2) {
			string ddat = txtd.OutLine.Txt;

			// g:1:G0*1[A-Z;],,N1:a:G05;:
			if (g05 == false)
				if (regG05.Match(txtd.NcLine + ";").Success) {
					if (on != null) txtd.OutLine.AtoAdd(on);
					g05 = true;
				}

			if (this.NcQue.QueueMin <= -3) {
				// g:1:%,M98P9699,P-3:i:G05;:
				if (this.NcQue[-3].NcLine.IndexOf(ClLink5Axis.Kotei) >= 0) {
					LogOut.CheckCount("Conv_NcRun1 306", false, this.ncname.Nnam);
					if (on != null) txtd.OutLine.MaeAdd(on);
				}

			}

			// g:1:M98P0*6[A-Z;(]:i:G91X0Y0Z0;:
			if (txtd.B_p0006) {
				if (off1 != null) txtd.OutLine.MaeAdd(off1);
				if (off2 != null) txtd.OutLine.MaeAdd(off2);
			}

			txtd.OutLine.Set(ddat);
			return;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="txtd"></param>
		private void Sed_k(NcLineCode txtd) {
			string ddat = txtd.OutLine.Txt;

			// k:1:G65:K
			if (txtd.NcLine.IndexOf("G65") >= 0)
				ddat = Mirchg3(ddat, txtd.G6p.ProgNo);

				// k:1:G66:K
			if (txtd.NcLine.IndexOf("G66") >= 0)
				ddat = Mirchg3(ddat, txtd.G6p.ProgNo);

			txtd.OutLine.Set(ddat);
			return;
		}

		/// <summary>
		/// マクロのミラーコードＷを変更する
		/// </summary>
		/// <param name="sdat"></param>
		/// <param name="progNo">マクロプログラムの番号</param>
		/// <returns></returns>
		private string Mirchg3(string sdat, int progNo) {
			int loc1;

			if (CamUtil.CamNcD.MacroCode.MacroIndex(progNo.ToString("0000")) < 0) {
				if (sdat.IndexOf("W") >= 0) throw new Exception("MIRROR DATA ; ERROR\n");
				return sdat;
			}

			System.Windows.Forms.MessageBox.Show(" " + progNo.ToString() + " W change \n");

			if ((loc1 = sdat.IndexOf("W1.0")) >= 0)
				return sdat.Substring(0, loc1) + sdat.Substring(loc1 + 4);
			if ((loc1 = sdat.IndexOf("W0.0")) >= 0)
				return sdat.Substring(0, loc1 + 1) + "1" + sdat.Substring(loc1 + 2);
			if (sdat.IndexOf("W") < 0)
				return sdat + "W1.0";

			throw new Exception("MIRROR DATA W ERROR\n");
		}
	}
}