using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using NcCode.nccode;
using System.Text.RegularExpressions;
using CamUtil;

namespace NcTejun.TejunSet
{
	/// <summary>
	/// ＮＣデータの動作をサブプログラムを含めシミュレートする
	/// </summary>
	internal class _main_simulate : NcCode._main, IDisposable
	{
		/// <summary>次元 2or3 add in 2015/03/20</summary>
		private readonly int camDimension;
		/// <summary>工具軸角度 add in 2015/03/20</summary>
		private readonly CamUtil.Angle3[][] axis_angl;
		/// <summary>標準送り速度 add in 2016/05/20</summary>
		private readonly double[] stdFeed;

		private System.IO.StreamWriter sw;
		private readonly Regex semic;
		private Regex comme;

		/// <summary>サブプログラムの種類 1:固定サイクル 2:G65 3:G66 4:M98</summary>
		private readonly short[] subOut;

		/// <summary>メイン入力ＮＣデータの行番号</summary>
		private int nclnMain = 0;

		/// <summary>
		/// ＮＣデータの検索パス
		/// </summary>
		public override string[] Ncdir {
			get { return m_ncdir; }
			protected set { m_ncdir = value; }
		}
		private string[] m_ncdir ={
			//"/usr1/ASDM/CAMCTL/NCSPC/",
			//"/usr1/ASDM/CAMCTL/NCMEM/",
			CamUtil.ServerPC.SvrFldrM,
			CamUtil.ServerPC.SvrName + @"\h\usr9\ASDM\CAMCTL\NCMEM_ncspeed\",
			CamUtil.ServerPC.SvrFldrE};

		/// <summary>
		/// 加工機依存のＮＣデータの実行シミュレーションを行います。加工機にあわせたマクロコードを用います。
		/// </summary>
		/// <param name="program_name">ＮＣデータ名</param>
		/// <param name="mach">加工機情報</param>
		/// <param name="dirname">保存するディレクトリ</param>
		/// <param name="camDimension">加工軸に依存する次元</param>
		/// <param name="angle">回転軸の回転角度</param>
		/// <param name="feedrate">標準の送り速度</param>
		/// <param name="nggt">ＮＣデータの平行移動量、ミラー設定マクロ反転の情報</param>
		public _main_simulate(string program_name, NcdTool.Mcn1 mach, string dirname, int camDimension,
			CamUtil.Angle3[][] angle, double[] feedrate, NcdTool.NcName.NcNam.St_nggt nggt)
			: base(program_name, mach.name, mach.ID) {

			this.camDimension = camDimension;

			this.axis_angl = angle;
			this.stdFeed = feedrate;

			// revをチェックするまでの暫定処置
			if (nggt.rev) {
				CamUtil.LogOut.CheckCount("_main_simulate 069", false, "REVはまだ使用できません。藤本までご連絡ください。");
				throw new Exception("REVはまだ使用できません。藤本までご連絡ください。");
			}

			// メインＮＣデータ設定
			NcCode.nccode.Transp_Mirror trs_mir = null;
			if (nggt.trns != Vector3.v0 || nggt.Mirr != CamUtil.NcZahyo.Null || nggt.rev) {
				trs_mir = new NcCode.nccode.Transp_Mirror(nggt.trns, nggt.Mirr, nggt.rev);
			}

			MainNcdSet("PC_FILE", trs_mir, new CamUtil.Vector3(0.0, 0.0, 500.0), null);

			sw = new System.IO.StreamWriter(dirname + "\\" + ncName);
			m_swNcCheckName = "_main_simulate";
			m_swNcCheck = new System.IO.StreamWriter(m_swNcCheckName, true, Encoding.Default);

			semic = new Regex("; *$|; *\\(");
			comme = new Regex("\\(.*\\)");

			subOut = new short[8];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nout"></param>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		public override void Ncoutm(NcOuts nout, NcMod mod, OCode lcode) {
			string chout, chmae, chato;
			Match match;
			int subd = lcode.subdep1.Depth;	// サブの深さ

			// テスト
			string checkText = lcode.nctx;

			// 初期設定時の出力は無視する
			if (sw == null) return;

			// メインプログラムの連続性のチェック
			if (lcode.subdep1.Depth == 0) {
				if (nclnMain > 0 && nclnMain + 1 != lcode.ncln)
					throw new Exception($"前：{nclnMain.ToString()}　後：{lcode.ncln.ToString()}");
				nclnMain = lcode.ncln;
			}

			// /////////////////////////////////
			// テキスト作成
			// /////////////////////////////////
			if (lcode.Gg[0].Equals(65.0)) {
				// 単純呼出しマクロの場合
				chout = Ncline_Call(mod, lcode);
				if (mod.Subk != 2) throw new Exception("qjfrbqrh");
			}
			else if (lcode.Gst12 && mod.GGroupValue[12].Equals(67.0) == false) {
				// モーダル呼出しマクロの設定行の場合
				chout = Ncline_Call(mod, lcode);
				if (mod.Subk != 2) throw new Exception("qjfrbqrh");
			}
			else if (mod.GGroupValue[12].Equals(66.1) == true) {
				// 毎ブロック呼出しマクロのモーダル行の場合
				chout = Ncline_Call(mod, lcode);
				if (mod.Subk != 2) throw new Exception("qjfrbqrh");
			}
			else {
				chout = Ncline_Main(mod, lcode, subd);
				if (mod.Subk == 2) throw new Exception("qjfrbqrh");
			}

			// /////////////////////////////////
			// 前後に追加するテキスト作成
			// /////////////////////////////////
			chmae = chato = null;

			// 固定サイクルの処理
			if (lcode.Gg[9].Gst || (subOut[subd] == 0 && !mod.GGroupValue[9].Equals(80.0))) {
				match = comme.Match(chout);
				if (match.Success)
					chout = chout.Replace(chout.Substring(match.Index, match.Length), "");
				if (!mod.GGroupValue[9].Equals(80.0)) {
					//MessageBox.Show("固定サイクル：" + mod.gg[9].ToInt().ToString() + chout);
					chato += "\n(===== SUBFIX START =====)";
					subOut[subd + 1] = 1;
					if (chout.Length > 0) chout = "(" + chout + ")";
				}
			}
			else if (subOut[subd + 1] == 1) throw new Exception("qwfqerhfbwerbfh");

			// カスタムマクロの処理（単純呼出し）
			if (lcode.Gg[0].Equals(65.0)) {
				match = comme.Match(chout);
				if (match.Success)
					chout = chout.Replace(chout.Substring(match.Index, match.Length), "");
				chout = "(" + chout + ")";
				//MessageBox.Show("カスタムマクロ：" + mod.gg[12].ToInt().ToString() + chout);
				chato += "\n(===== SUBG65 START =====)";
				subOut[subd + 1] = 2;
			}
			// カスタムマクロの処理（モーダル呼出し）
			else if (lcode.Gst12 || (subOut[subd] == 0 && !mod.GGroupValue[12].Equals(67.0))) {
				match = comme.Match(chout);
				if (match.Success)
					chout = chout.Replace(chout.Substring(match.Index, match.Length), "");
				if (lcode.Gst12) {
					//MessageBox.Show("カスタムマクロ：" + mod.gg[12].ToInt().ToString() + chout);
					chout = "(" + chout + ")";
				}
				else {
					chato += "\n(===== SUBG66 START =====)";
					subOut[subd + 1] = 3;
					chout = "(" + chout + ")";
				}
			}
			else if (subOut[subd + 1] == 3) throw new Exception("qwfqerhfbwerbfh");

			// サブプログラム
			if (lcode.Mst[0] > 0 && lcode.Mst[1] == 98) {
				chmae += "\n(===== SUBSUB START =====)";
				chout = "(" + chout + ")";
				subOut[subd + 1] = 4;
			}

			// /////////////////////////
			// すべてのコールからの復帰
			// /////////////////////////
			if (lcode.Mst[0] > 0 && lcode.Mst[1] == 99) {
				chato += "\n(===== ALLSUB  END  =====)";
				subOut[subd] = 0;
			}

			// ///////////////
			// テキスト出力
			// ///////////////
			foreach (CodeD codeD in lcode.codeData)
				if (codeD.ncChar == '#' || codeD.String().IndexOf('#') > 0) {
					chout = (chout == null ? "          " : chout.PadRight(10)) + "<<" + lcode.nctx + ">>";
					break;
				}
			if (chmae != null) sw.WriteLine(chmae.Substring(1));
			if (chout != null) sw.WriteLine(chout);
			if (chato != null) sw.WriteLine(chato.Substring(1));

			return;
		}

		/// <summary>
		/// ＮＣデータのテキスト変換（メイン）
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		/// <param name="subd">サブプログラムの深さ</param>
		/// <returns></returns>
		private string Ncline_Main(NcMod mod, OCode lcode, int subd) {
			string chout = "";

			foreach (CodeD codeD in lcode.codeData) {
				switch (codeD.ncChar) {
				case ';':
					break;
				case '%':
				case '/':
					chout += codeD.ncChar;
					break;
				case '(':
				case '#':
					if (subd == 0)
						chout += codeD.String();
					break;
				case 'G':
				case 'M':
				case 'T':
					string aaa;
					aaa = codeD.ToStringAuto();
					if (aaa.Length == 1 || (aaa.Length == 3 && aaa.IndexOf('.') == 1)) aaa = "0" + aaa;
					chout += codeD.ncChar + aaa;
					break;
				case 'O':
				case 'P':
					chout += codeD.ncChar + codeD.ToStringAuto().PadLeft(4, '0');
					break;
				case 'N':
				default:
					chout += codeD.ncChar + codeD.ToStringAuto();
					break;
				}
			}

			if (chout.Length == 0) return null;
			return chout;
		}
		/// <summary>
		/// ＮＣデータのテキスト変換（マクロコール文）
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		/// <returns></returns>
		private string Ncline_Call(NcMod mod, OCode lcode) {
			string chout = "";

			foreach (CodeD codeD in lcode.codeData) {
				switch (codeD.ncChar) {
				case ';':
				case '%':
				case '/':
				case '(':
				case '#':
					break;
				case 'G':
					string aaa;
					aaa = codeD.ToStringAuto();
					if (aaa.Length == 1 || (aaa.Length == 3 && aaa.IndexOf('.') == 1)) aaa = "0" + aaa;
					chout += codeD.ncChar + aaa;
					break;
				case 'L':
				case 'N':
				case 'O':
				case 'P':
					chout += codeD.ncChar + codeD.ToStringAuto().PadLeft(4, '0');
					break;
				default:
					chout += codeD.ncChar + codeD.ToString("0.0");
					break;
				}
			}
			if (chout.Length == 0) return null;
			return chout;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Ncoute()
			//struct ncout out;
		{
			//NcOuts nout = ncReader.tout;
			sw.WriteLine("%");
			base.WriteEnd();
			sw.Close();
		}

		/// <summary>
		/// ＮＣデータ解析時におけるエラーの処理を実行
		/// </summary>
		/// <param name="errn">エラー番号</param>
		/// <param name="errc">コメント</param>
		public override void Ncerr(int errn, string errc)
			//int errn;
			//char errc[];
		{
			//void exit();
			switch (errn) {
			case 0: /* check data print */
				return;
			case 1: /* warning data print */
				//MessageBox.Show(errc, "ncerr",
				//	MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			case 2: /* error data print */
				//MessageBox.Show(errc, "ncerr",
				//	MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			default: /* fatal error */
				MessageBox.Show(errc + " code=" + errn, "ncerr",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
				Application.Exit();
				throw new Exception();
			//exit(errn);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Close()
		{
			sw.Close();
			m_swNcCheck.Close();
			m_swNcCheck = null;
		}

		public void Dispose() {
			if (sw != null) { sw.Dispose(); sw = null; }
			if (m_swNcCheck != null) { m_swNcCheck.Dispose(); m_swNcCheck = null; }
		}
	}
}

