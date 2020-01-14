using System;
using System.Collections.Generic;
using System.Text;

namespace NcCode.nccode
{
	/// <summary>
	/// ファイルからＮＣデータの読み込み、書き込みを処理するクラスです。１行ごとの双方処理が可能です。
	/// </summary>
	internal class NcRW
	{
		/// <summary>固定サイクル呼び出しの最大値</summary>
		protected const int FIXMAX = 1;
		/// <summary>マクロ呼び出しの最大値</summary>
		protected const int MACMAX = 4;
		/// <summary>サブプロ呼び出しの最大値</summary>
		protected const int SUBMAX = 8;

		private const int sakiyomi = 3;

		static private NcOut[] nout = new NcOut[sakiyomi + 2];
		static private OCode[] lcode = new OCode[(sakiyomi + 1) * 30];
		static private NcMod[] mod = new NcMod[(sakiyomi + 1) * 30];

		/// <summary>noutデータの最大位置（＝保存しているnoutデータ数－１）</summary>
		internal static int sakiy0 = -1;
		/// <summary>現在保存すべきnoutデータ数</summary>
		static private int sakiy1 = 1;

		/// <summary>非直線の位置の出力モード（true:出力）</summary>
		static private bool hichoku = false;

		/// <summary>
		/// ＮＣ制御文の現在の状況を保存します。
		/// </summary>
		internal readonly struct Mthd
		{
			/// <summary>制御文の状況区分</summary>
			public enum MODE
			{
				/// <summary>非制御文</summary>
				NORMAL,
				/// <summary>マクロＧＯＴＯ文</summary>
				GOTO,
				/// <summary>マクロＷＨＩＬＥの終了文</summary>
				WHILE_END,
				/// <summary>マクロＷＨＩＬＥの不成立</summary>
				WHILE_NO,
				/// <summary>マクロＷＨＩＬＥの成立</summary>
				WHILE_YES
			}

			/// <summary>ＮＣ制御文の種類</summary>
			public readonly MODE control;
			/// <summary>GOTOのシーケンスナンバー、DOループのナンバー</summary>
			public readonly int gotoNo;
			/// <summary></summary>
			public Mthd(int dummy) {
				control = MODE.NORMAL;
				gotoNo = 0;
			}
			/// <summary></summary>
			private Mthd(MODE control, int gotoNo) {
				this.control = control;
				this.gotoNo = gotoNo;
			}
			/// <summary>ＮＣ制御文の現在の状況の更新</summary>
			public Mthd UpDate(MODE control, int gotoNo) {
				return new Mthd(control, gotoNo);
			}
			/// <summary>ＮＣ制御文の現在の状況の更新</summary>
			public Mthd UpDate(MODE control) {
				return new Mthd(control, this.gotoNo);
			}
			/// <summary>ＮＣ制御文の現在の状況の更新</summary>
			public Mthd UpDate(int gotoNo) {
				return new Mthd(this.control, gotoNo);
			}

			/// <summary>
			/// マクロ文（制御文）の処理
			/// </summary>
			/// <param name="p_submthd"></param>
			/// <param name="codeD"></param>
			/// <returns></returns>
			public Mthd(Mthd p_submthd, CodeD codeD) {
				switch (codeD.StringMacro()) {
				case "THEN":
					this.control = p_submthd.control;
					this.gotoNo = p_submthd.gotoNo;
					break;
				case "IF":
					if (p_submthd.control == Mthd.MODE.WHILE_NO)
						throw new Exception("qwefwerfre");
					this.control = p_submthd.control;
					this.gotoNo = p_submthd.gotoNo;
					break;
				case "GOTO":
					if (p_submthd.control == Mthd.MODE.WHILE_NO)
						throw new Exception("qwefwerfre");
					//p_submthd = p_submthd.UpDate(NcRW.Mthd.MODE.GOTO, codeD.ToInt);
					this.control = Mthd.MODE.GOTO;
					this.gotoNo = codeD.ToInt;
					break;
				case "END":
					//p_submthd = p_submthd.UpDate(NcRW.Mthd.MODE.WHILE_END, codeD.ToInt);
					this.control = Mthd.MODE.WHILE_END;
					this.gotoNo = codeD.ToInt;
					break;
				case "DO":
					if (p_submthd.control == Mthd.MODE.NORMAL) {
						//p_submthd = p_submthd.UpDate(NcRW.Mthd.MODE.WHILE_YES);
						this.control = Mthd.MODE.WHILE_YES;
					}
					else {
						this.control = p_submthd.control;
					}
					//p_submthd = p_submthd.UpDate(codeD.ToInt);
					this.gotoNo = codeD.ToInt;
					break;
				case "WHILE":
					if (p_submthd.control == Mthd.MODE.WHILE_NO)
						throw new Exception("qwefwerfre");
					if (codeD.ToBoolean) {
						this.control = p_submthd.control;
						this.gotoNo = p_submthd.gotoNo;
					}
					else {
						//p_submthd = p_submthd.UpDate(NcRW.Mthd.MODE.WHILE_NO);
						this.control = Mthd.MODE.WHILE_NO;
						this.gotoNo = p_submthd.gotoNo;
					}
					break;
				case "BPRNT":
				case "DPRNT":
				case "POPEN":
				case "PCLOS":
				default:
					throw new Exception("対象外のマクロコードです。");
				}
			}
		}

		// //////////////////////////////////////////////////////////
		// 以上 static
		// //////////////////////////////////////////////////////////






		// /// <summary>最終出力用</summary>
		// public NcOuts tout;


		/// <summary>
		/// 呼出し元の__mainクラス add in 2008/07/23
		///   ncoutm ncoute ncdir sfilの参照に用いる
		/// </summary>
		private _main _main_;

		/// <summary>最新のNcModを保存する</summary>
		private NcMod tmod;
		/// <summary>各オブジェクト用マクロ変数</summary>
		private NcMachine.Variable fsub;

		/// <summary>データ読み込み時の情報を保存する（fsubtは使用しない）</summary>
		private NcMod.Fmsub readInfo;
		/// <summary>NcCodeGのndepをコピーする（将来はここのみにする）</summary>
		private Ncdep ndep;

		// /// <summary>プログラム開始行番号</summary>
		private int stlin = 0;

		/// <summary>
		/// ＮＣデータ読み込みの状況
		/// -1:有意データ前、0:有意データ、1:加工終了Ｍコード読み込み、2:終了
		/// </summary>
		private int start;


		/// <summary>サブルーチンのＮＣファイルリーダ</summary>
		private NcRW nextPro;

		/// <summary>
		/// 各ＮＣデータ行での動作を決定（特にサブプロ）
		/// </summary>
		private NcMod.Ido ido = new NcMod.Ido(0);

		// /// <summary>サブＮＣのストリームリーダ</summary>
		private NcFileReader fp = null;

		/// <summary>マクロ展開のデータに移動ミラーを設定する変数。NCSPEEDで使用するために設定 in 2010/06/22</summary>
		private Transp_Mirror transp_mirror;



		/// <summary>
		/// 完全なＮＣデータ（%;～%;）を１行単位で読込み／処理／書込みするクラスを作成するコンストラクタ
		/// （ncgenより使用する場合）
		/// </summary>
		/// <param name="main">呼出し元の__mainクラス</param>
		/// <param name="tmod">初期のＮＣのモード</param>
		/// <param name="xyz">初期のＸＹＺ位置</param>
		internal NcRW(_main main, NcMod tmod, CamUtil.Vector3 xyz)
		{
			this._main_ = main;
			this.ndep = new Ncdep(0);
			this.readInfo = new NcMod.Fmsub(0, 1, "call", null);
			this.tmod = tmod;
			fsub = new NcMachine.Variable();	// ADD in 2017/11/08

			// １ではうまく動かないので変更 2008/07/22
			//this.start = 1;
			this.start = -1;

			nout[0] = new NcOut(new CamUtil.Ichi(xyz, CamUtil.Vector3.v0, Post.PostData['X'].sdgt));	// 初期位置のセット
			this.transp_mirror = null;
		}

		/// <summary>
		/// ファイル内にあるＮＣデータのREAD,WRITEを処理するクラスを作成するコンストラクタ
		/// </summary>
		/// <param name="main">呼出し元の__mainクラス</param>
		/// <param name="tmod">現在のtmod。ここに保存しておく</param>
		/// <param name="fmsub">呼び出し条件</param>
		/// <param name="pre">呼び出し元の変数</param>
		/// <param name="nfr">ストリームリーダー</param>
		/// <param name="trs_mir">NCSPEED専用の手順で設定された移動ミラー情報</param>
		/// <param name="moto"></param>
		internal NcRW(_main main, NcMod tmod, NcMod.Fmsub fmsub, NcMachine.Variable pre, NcFileReader nfr, Transp_Mirror trs_mir, Ncdep moto)
		{
			// メインプログラムのセット
			this._main_ = main;
			if (this._main_ == null)
				throw new Exception("wefbdawefb");

			// 暫定チェック
			switch (fmsub.subk) {
			case 1:
			case 3:
				if (fmsub.iloop.HasValue == true) throw new Exception("awefbwrehfb"); break;
			case 2:
				if (fmsub.iloop.HasValue == false) throw new Exception("awefbwrehfb"); break;
			}
			this.readInfo = new NcMod.Fmsub(fmsub.subk, fmsub.iloop, nfr.Ncnam, fmsub.fsubt);
			this.tmod = tmod;

			if (this._main_.SwNcCheck != null)
				this._main_.SwNcCheck.WriteLine("サブプロのオープンin NcRW_File：" + fmsub.ncnam);

			this.ndep = moto;
			if (readInfo.subk == 1 && ndep.subf + 1 > FIXMAX)
				throw new Exception("FIXED CYCLE FUKASA ERROR");
			if (readInfo.subk == 2 && ndep.subm + 1 > MACMAX)
				throw new Exception("MACRO FUKASA ERROR");
			if (ndep.Depth + 1 > SUBMAX)
				throw new Exception("SUB-PROGRAM FUKASA ERROR");

			fp = nfr;

			switch (readInfo.subk) {
			case 0:
				if (pre != null) throw new Exception("メインの呼出しエラー");
				fsub = new NcMachine.Variable();
				nout[0] = new NcOut(new CamUtil.Ichi(new CamUtil.Vector3(0.0, 0.0, 500.0), CamUtil.Vector3.v0, Post.PostData['X'].sdgt));	// 初期位置のセット
				break;
			case 1:
			case 2:
				if (pre == null) throw new Exception("固定サイクル/カスタムマクロの呼出しエラー");
				fsub = new NcMachine.Variable(pre);
				//vSet();
				break;
			case 3:
				if (pre == null) throw new Exception("一般サブの呼出しエラー");
				fsub = pre;
				break;
			}
			this.transp_mirror = trs_mir;
		}

		/// <summary>
		/// マクロ変数の設定を実行する
		/// </summary>
		private void VSet() {
			bool shoki = true;
			for (int vno = 1; vno <= NcMachine.Variable.MMAX; vno++)
				if (fsub[vno] != null) { shoki = false; break; }

			if (shoki == false) {
				foreach (NcMod.Fmsub.FsubT ftmp in this.readInfo.fsubt) {
					// 初期設定値と異なる場合はいままで考慮がなかったのでエラーとしてとりあえず処理する
					if (fsub[ftmp.varia] != ftmp.value) {
						if (CamUtil.ProgVersion.Debug)
							System.Windows.Forms.MessageBox.Show("マクロ変数の設定でエラーが検出されました。");
						else
							throw new Exception("マクロ変数の設定でエラーが検出されました。");
					}
				}
				for (int vno = 1; vno <= NcMachine.Variable.MMAX; vno++)
					fsub[vno] = null;
			}
			foreach (NcMod.Fmsub.FsubT ftmp in this.readInfo.fsubt)
				fsub[ftmp.varia] = ftmp.value;
		}

		/// <summary>
		/// （NcEW_Call）メインＮＣデータを１行処理し、出力を_main.ncoutmへ渡す
		/// </summary>
		/// <param name="ddat"></param>
		public void WriteLine(string ddat)
		{
			Mthd mthd = new Mthd(0);
			int ierr;
			// ///////////////////////////////////////////////////////
			// ＮＣデータ１行の処理
			// ///////////////////////////////////////////////////////
			if ((ierr = Nccode(0, ddat, ref start, ref mthd)) != 0)
				IERR(ierr, ddat, 0);
			// ///////////////////////////////////////////////////////
			if (mthd.control == Mthd.MODE.NORMAL) {
				if (start == 1 && mthd.gotoNo != -1)
					start++;
			}
			else
				throw new Exception("aergaergfaer");
			return;
		}
		/// <summary>
		/// （NcRW_File）サブプロの読出し（メインも）。モーダル（G81,G66...）の場合は繰返し使用される
		/// </summary>
		internal void ReadAll()
		{
			if (this._main_.SwNcCheck != null)
				this._main_.SwNcCheck.WriteLine("サブプロの呼出し開始 in NcRW_File.ReadAll：" + this.readInfo.ncnam);
			Mthd mthd = new Mthd(0);
			int dodep;
			int icnt;
			//ＮＣデータの行番号
			int line;

			if (fp == null) {
				if (this._main_.SwNcCheck != null)
					this._main_.SwNcCheck.WriteLine("サブプロの呼出し終了？？ in NcRW_File.ReadAll：" + this.readInfo.ncnam);
				return;
			}
			switch (readInfo.subk) {
			case -1: ndep = ndep.UpDateSub(-1); break;
			case 0: ndep = ndep.UpDateSub(0); break;
			case 1: ndep = ndep.UpDateFix(ndep.subf + 1, new CamUtil.LCode.Gcode(80)); break;
			case 2: ndep = ndep.UpDateMac(ndep.subm + 1, new CamUtil.LCode.Gcode(67)); break;
			case 3: ndep = ndep.UpDateSub(ndep.subs + 1); break;
			default: break;
			}

			mthd = mthd.UpDate(Mthd.MODE.NORMAL);
			dodep = -1;
			int[,] donum = new int[2, 3];
			int[] gonum = new int[2];
			icnt = 0;

			do {
				((NcFileReader)fp).Rewind();
				line = 0;
				// start -> -1:mui  0:nc-data  1:M99,M30,M02  2:mui
				start = -1;
				while (fp.EndOfStream != true && start <= 1) {
					int ierr;

					// ///////////////////////////////////////////////////////
					// ＮＣデータの読込み
					string ddat = fp.ReadLine();
					// ///////////////////////////////////////////////////////

					line++;
					if (line < stlin)
						continue;
					else if (line == stlin)
						start = 0;

					if (line % 1000 == 0)
						System.Windows.Forms.Application.DoEvents();

					mthd = mthd.UpDate(-1);
					switch (mthd.control) {
					case Mthd.MODE.WHILE_YES: // WHILE JOKEN YES (DO LOOP)
						break;
					case Mthd.MODE.WHILE_NO: // WHILE JOKEN NO
						{
							CodeDList cList = new CodeDList(ddat, this.fsub);
							if (cList.Count == 0) throw new Exception($"{readInfo.ncnam} ( {ddat} ) NC-DATA number (line={line})");
							if (cList.CodeCount('%') != 0)
								start++;
							if (mthd.control == Mthd.MODE.NORMAL && mthd.gotoNo != -1) {
								start = 2;
								throw new Exception("WHILE_NO が NORMAL に変わることはない！");
							}
							if (start == 0 && cList.CodeCount('#') != 0 && cList.CodeData('(').String().IndexOf("END") == 0 && donum[0, dodep] == cList.CodeData('(').ToInt) {
								if (dodep < -1) throw new Exception("DO LOOP error");
								mthd = mthd.UpDate(Mthd.MODE.NORMAL);
								dodep--;
								continue;
							}
						}
						break;
					case Mthd.MODE.WHILE_END:	// DO ni return (read ENDDO)
						if (line == donum[1, dodep]) {
							if (dodep < -1) throw new Exception("DO LOOP error");
							mthd = mthd.UpDate(Mthd.MODE.NORMAL);
							start = 0;
							dodep--;
						}
						else
							mthd = mthd.UpDate(Mthd.MODE.WHILE_END);
						break;
					case Mthd.MODE.GOTO: // GOTO
						{
							CodeDList cList = new CodeDList(ddat, this.fsub);
							if (cList.CodeCount('%') != 0)
								start++;
							if (start == 0) {
								if (cList.Count == 0) throw new Exception($"不正な文字が検出された　{readInfo.ncnam} ( {ddat} ) NC-DATA number (line={line})");
								if (cList.CodeCount('N') != 0 && cList.CodeData('N').ToInt == gonum[0])
									mthd = mthd.UpDate(Mthd.MODE.NORMAL);
								else if (icnt == 0 && cList.CodeCount('O') != 0)
									start = 2;
							}
							else if (start == 1)
								start = 2;
							if (line == gonum[1]) throw new Exception("GOTO error");
						}
						break;
					case Mthd.MODE.NORMAL:
						break;
					default:
						throw new Exception($"{readInfo.ncnam} ( {ddat} ) program error");
					}
					if (mthd.control == Mthd.MODE.NORMAL) {

						// ///////////////////////////////////////////////////////
						// ＮＣデータ１行の処理
						try { if ((ierr = Nccode(line, ddat, ref start, ref mthd)) != 0) IERR(ierr, ddat, line); }
						catch (Exception ex) { throw new Exception($"({line} {ddat})\n{ex.Message}"); }
						// ///////////////////////////////////////////////////////

						switch (mthd.control) {
						case Mthd.MODE.WHILE_YES: // WHILE JOKEN YES
							mthd = mthd.UpDate(Mthd.MODE.NORMAL);
							dodep++;
							if (dodep > 2) throw new Exception("DO LOOP over");
							donum[0, dodep] = mthd.gotoNo;
							donum[1, dodep] = line;
							break;
						case Mthd.MODE.WHILE_NO: // WHILE JOKEN NO
							dodep++;
							if (dodep > 2) throw new Exception("DO LOOP over");
							donum[0, dodep] = mthd.gotoNo;
							donum[1, dodep] = line;
							break;
						case Mthd.MODE.WHILE_END:
							icnt = 0;
							start = 2;
							break;
						case Mthd.MODE.GOTO:
							icnt = 0;
							gonum[0] = mthd.gotoNo;
							gonum[1] = line;
							break;
						case Mthd.MODE.NORMAL:
							if (start == 1 && mthd.gotoNo != -1)
								start++;
							break;
						default:
							break;
						}
					}
					// ----- loop end ----- //

				}
				icnt++;
				if (start == -1) throw new Exception("NC-DATA '%' not found");
				if (start == 0 || start == 1) throw new Exception("NC-DATA is not complete.");
			} while ((mthd.control == Mthd.MODE.GOTO || mthd.control == Mthd.MODE.WHILE_END) && icnt < 2);

			if (mthd.control != Mthd.MODE.NORMAL) throw new Exception("macro command error");

			if (this._main_.SwNcCheck != null)
				this._main_.SwNcCheck.WriteLine("サブプロの呼出し終了 in NcRW_File.ReadAll：" + this.readInfo.ncnam);
			return;
		}

		/// <summary>
		/// １ショットのサブの終了とモーダルの終了。
		/// （G80,G67）で呼出され、ファイルをクローズする。
		/// </summary>
		internal void NcClose()
		{
			if (fp == null) return;
			fp.Close();
			fp = null;
			return;
		}

		/// <summary>
		/// ＮＣデータ１行を処理する（NcCode._mainで使用、ncrunでは使われない）
		/// </summary>
		/// <param name="line">行番号</param>
		/// <param name="ddat">処理するＮＣデータ１行の文字列</param>
		/// <param name="start">ＮＣデータの現状の状況</param>
		/// <param name="mthd"></param>
		/// <returns></returns>
		internal int Nccode(int line, string ddat, ref int start, ref NcRW.Mthd mthd)
		{
			OCode tcode;
			int ii;

			// ///////////////////////////////////
			// 前行の情報をシステム変数に保存する
			// ///////////////////////////////////
			fsub.SystemSet_COPY(nout[0].Out0);

			// ////////////////////////////////
			// １行のデータ情報（tcode）のセット
			// ////////////////////////////////
			//tcode = new OCode();
			if (this._main_ == null)
				throw new Exception("wefbdawefb");
			if (mthd.control != Mthd.MODE.NORMAL)
				throw new Exception("qwefqerbfqer");

			// プログラムのチェック用に停止する
			if (ddat.IndexOf("G66") >= 0) {
				;
				//System.Windows.Forms.MessageBox.Show(ddat);
			}

			CodeDList codeD_ncspd = new CodeDList(ddat, fsub);
			tcode = new OCode(out int? errsw, line, codeD_ncspd, start, mthd, this._main_, transp_mirror, ndep);
			errsw = errsw ?? 1;

			// このＮＣデータ行による制御文情報の更新
			mthd = tcode.submthd;
			// このＮＣデータ行によるマクロ変数の更新
			fsub.SystemSet_NCCODE(tcode);
			// このＮＣデータ行によるコール文情報の更新
			this.ndep = tcode.subdep1;

			// チェック出力
			if (this._main_.SwNcCheck != null) {
				if (tcode.Subc + ndep.subf + ndep.subm > 0 || tcode.nctx.IndexOf("G6") >= 0) {
					this._main_.SwNcCheck.Write($"{tcode.Subc.ToString()} {ndep.subf.ToString()} {ndep.subm.ToString()} {tcode.nctx}");

					this._main_.SwNcCheck.WriteLine();

					if (tcode.nctx.IndexOf("P8011") >= 0)
						this._main_.SwNcCheck.Flush();
				}
			}

			if (tcode.codeData.CodeCount('%') != 0)
				start++;
			if (errsw != 0)
				return errsw.Value;
			if (start != 0)
				return 0;

			// MACRO ERROR
			if (fsub[3000].HasValue == true)
				throw new Exception($"MACRO(3000)ERROR CODE={(int)fsub[3000]} {tcode.cmt}");

			// G code ERROR shori
			switch (tcode.gsw) {
			case 3:
				throw new Exception("fatal error. " + ddat);
			case 2:
				_main.Error.Ncerr(1, "1line is  ignored. " + ddat);
				return 0;
			case 1:
			default:
				break;
			}

			// ////////////////////////////////////////////////
			// ＮＣの現在モード（lmod）と位置情報（ido）を更新
			if (tmod.NcCod2(tcode, ref ido, false, ndep, fsub) != 0)
				return 0;
			// ////////////////////////////////////////////////

			// ////////////////////////////////
			// サブプロオープン（nccod2より移動）
			// ////////////////////////////////
			if (tmod.subInfo.subk != 0) {
				if (nextPro != null) {
					throw new Exception("ＮＣデータの分割に対応するため、固定サイクル中のサブプログラム呼出しは認めません。");
				}
				NcFileReader aa = new NcFileReader(tmod.subInfo.ncnam, this._main_.Ncdir, this._main_.sfil);
				nextPro = new NcRW(this._main_, tmod, tmod.subInfo, fsub, aa, null, ndep);
			}
			// モーダル呼出し中のマクロ変数とループ回数の変更
			else if (nextPro != null) {
				if (tmod.subInfo.iloop.HasValue) throw new Exception("qfqfbqhrfb");
				if (tmod.subInfo.fsubt.Count > 0) {
					// 新たな引数で更新する
					CamUtil.LogOut.CheckCount("NcRW 801", true, "固定サイクルあるいはマクロ引数の値が変更された");
					nextPro.readInfo = nextPro.readInfo.SetFsub(tmod.subInfo.fsubt);
				}
			}

			// program return mode
			if (ido.norm == 0 && tcode.Mst[0] != 0) {
				if (tcode.Mst[1] == 2 || tcode.Mst[1] == 30 || tcode.Mst[1] == 99)
					start = 1;
			}

			// ///////////////////////////////////////////
			// 中間データ出力とサブプロ呼出しのループ
			// ///////////////////////////////////////////
			int idoCount;
			if (nextPro != null) {
				// nextPro.readInfo.iloopは設定行でセットされたモーダルな繰り返し数（カスタムマクロの場合のみ使用）
				idoCount = tmod.LoopNo ?? nextPro.readInfo.iloop.Value;
			}
			else
				idoCount = 1;

			if (nextPro != null) {
				// //////////////////////////////////
				// 以下はiloopの暫定チェック
				// //////////////////////////////////
				if (tcode.Gst12 || ndep.gg12.Equals(67) == false || tmod.GGroupValue[0].Equals(65)) {
					if (tmod.GGroupValue[0].Equals(65) || (tcode.Gst12 && ndep.gg12.Equals(67) == false)) {
						if (tmod.subInfo.iloop.HasValue == false) throw new Exception("wefbqhewfbqheb");
					}
					else {
						if (tmod.subInfo.iloop.HasValue == true) throw new Exception("wefbqhewfbqheb");
					}
				}
				else { if (tmod.subInfo.iloop.HasValue == true) throw new Exception("wefbqhewfbqheb"); }

				// //////////////////////////////////
				// 以下はloopNoの暫定チェック
				// //////////////////////////////////
				// サブプロ
				bool m98 = false; if (tcode.Mst[0] != 0 && tcode.Mst[1] == 98 && tcode.codeData.CodeCount('P') != 0) m98 = true;
				if (m98)
					if (tmod.LoopNo.HasValue == false) throw new Exception("loopNoが設定されていない");

				// マクロ
				if (tcode.Gst12 || ndep.gg12.Equals(67) == false || tmod.GGroupValue[0].Equals(65)) {
					if (ndep.gg12.Equals(66.1) || tmod.GGroupValue[0].Equals(65)) {
						if (tmod.LoopNo.HasValue == true) throw new Exception("loopNoが設定されている");
					}
					else if (tcode.Gst12 == false && ndep.gg12.Equals(66)) {
						// モーダルマクロG66の移動を伴わない行の場合に実行しないチェック 2017/09/28
						if (tcode.codeData.CodeCount('X') + tcode.codeData.CodeCount('Y') + tcode.codeData.CodeCount('Z') == 0) {
							if (nextPro.readInfo.subk != 0) throw new Exception("wefbqhewfbqheb");
							if (tmod.LoopNo.Value != 0) throw new Exception("wefbqhewfbqheb");
						}
						else {
							if (nextPro.readInfo.subk != 2) throw new Exception("wefbqhewfbqheb");
							if (tmod.LoopNo.HasValue) throw new Exception("wefbqhewfbqheb");
						}
					}
					else { if (tmod.LoopNo.Value != 0) throw new Exception("wefbqhewfbqheb"); }
				}

				// 固定サイクル
				if (!ndep.gg09.Equals(80)) {
					if (tcode.codeData.CodeCount('X') + tcode.codeData.CodeCount('Y') + tcode.codeData.CodeCount('Z') + tcode.codeData.CodeCount('R') == 0) {
						if (nextPro.readInfo.subk != 0) throw new Exception("wefbqhewfbqheb");
						if (tmod.LoopNo.Value != 0) throw new Exception("wefbqhewfbqheb");
					}
					else {
						if (nextPro.readInfo.subk != 1) throw new Exception("wefbqhewfbqheb");
						if (tmod.LoopNo.Value == 0) throw new Exception("wefbqhewfbqheb");
					}
				}
				else if (tcode.Gst09) {
					if (tmod.LoopNo.Value != 0) throw new Exception("wefbqhewfbqheb");
				}
			}

			do {
				// ////////////////
				// 新規noutの作成
				// ////////////////
				if (tmod.Mcro == false || sakiy0 < 0) {
					sakiy0 += 1;
					for (ii = sakiy0; ii >= 0; ii--) {
						nout[ii + 1] = nout[ii];
					}
					// sakiy0<0であった時、nout[0]は事前に初期化されている
					nout[0] = new NcOut(nout[1], tmod.Mcro);
				}

				// ////////////////////////////
				// lcode,modの保存とimacの設定
				// ////////////////////////////
				for (ii = nout[sakiy0].imac; ii >= 0; ii--) {
					lcode[ii + 1] = lcode[ii];
					mod[ii + 1] = mod[ii];
				}
				lcode[0] = tcode;
				mod[0] = tmod.Clone();
				mod[0].subInfo = new NcMod.Fmsub(tmod.subInfo.subk, null, null);
				for (ii = sakiy0; ii >= 0; ii--)
					nout[ii].imac++;

				// ////////////////////
				// nout[0]の情報作成
				// ////////////////////
				if (mod[0].Mcro == false)
					nout[0].Ncxyz(ido.pass, lcode[0], mod[0], fsub);

				// ////////////////////////////////
				// 出力する最終位置(sakiy[1])を設定
				// ////////////////////////////////
				sakiy1 = Ncbuf(lcode[0], mod[0], nout[0].Out0.idoa, ido.norm);

				// /////////////////
				// ＮＣデータの出力
				// /////////////////
				for (ii = sakiy0; ii >= sakiy1; ii--) {
					Ncoput(ii, nout, mod, lcode);
					sakiy0--;
					if (ii == 0) {
						// 最終出力用の変数に入れる
						//tout = nout[0].out0;
					}
				}

				// ///////////////////
				// サブプロを呼び出す
				// ///////////////////
				if (nextPro != null) if (nextPro.readInfo.subk > 0 && idoCount > 0) {
						try {
							switch (nextPro.readInfo.subk) {
							case 1:	/* fixed */
							case 2:	/* usermacro */
								nextPro.VSet(); break;
							}
							switch (nextPro.readInfo.subk) {
							case 1:	/* fixed */
							case 2:	/* usermacro */
							case 3:	/* Mcode sub */
								nextPro.ReadAll();
								break;
							default:
								throw new Exception("ido subk program ERROR");
							}
						}
						catch (Exception ex) { throw new Exception(nextPro.readInfo.ncnam + " " + ex.Message); }
					}
			} while ((--idoCount) > 0);

			// close 1shot macro
			// close modal macro
			if (nextPro != null) {
				switch (nextPro.readInfo.subk) {
				case 1:
					if (ndep.gg09.Equals(80)) {
						if (this._main_.SwNcCheck != null) this._main_.SwNcCheck.WriteLine("サブプロのクローズ in NcRW.nccode：" + nextPro.readInfo.ncnam);
						nextPro.NcClose(); nextPro = null;
					}
					break;
				case 2:
					if (ndep.gg12.Equals(67)) {
						if (this._main_.SwNcCheck != null) this._main_.SwNcCheck.WriteLine("サブプロのクローズ in NcRW.nccode：" + nextPro.readInfo.ncnam);
						nextPro.NcClose(); nextPro = null;
					}
					break;
				case 3:
					if (0 == 0) {
						if (this._main_.SwNcCheck != null) this._main_.SwNcCheck.WriteLine("サブプロのクローズ in NcRW.nccode：" + nextPro.readInfo.ncnam);
						nextPro.NcClose(); nextPro = null;
					}
					break;
				}
			}

			return 0;
		}

		private int Ncbuf(OCode tcode, NcMod lmod, int idoa, int errsw)
			//struct ocode *tcode;
			//struct ncmod *lmod;
			//int idoa;
		{
			int sakiy;

			if (lmod.GGroupValue[0].Equals(10) ||
				lmod.GGroupValue[0].Equals(10.1) ||
				lmod.GGroupValue[0].Equals(11) ||
				lmod.GGroupValue[0].Equals(31) ||
				lmod.GGroupValue[0].Equals(31.1) ||
				lmod.GGroupValue[0].Equals(31.2) ||
				lmod.GGroupValue[0].Equals(31.3) ||
				lmod.GGroupValue[0].Equals(37) ||
				lmod.GGroupValue[0].Equals(28) ||
				lmod.GGroupValue[0].Equals(30) ||
				lmod.GGroupValue[0].Equals(53) ||
				tcode.Gg[6].Gst ||
				tcode.Gg[4].Gst)
				sakiy = 0;
			else if (lmod.GGroupValue[7].Equals(40))
				sakiy = 1;
			else {
				if (idoa == 2)
					sakiy = 2;
				else
					sakiy = 3;
			}
			// 一般のＮＣデータでありＭの終了コード（M0,M1,M2,M30）である場合、
			// バッファー内容を空にする（sakiy=0とし最後まで出力する）
			if (errsw == 0 && tcode.Mst[0] != 0 &&
			(tcode.Mst[1] == 0 ||
			(tcode.Mst[1] == 1 && NcMachine.ostop == true) ||
			 tcode.Mst[1] == 2 ||
			 tcode.Mst[1] == 30))
				sakiy = 0;

			if (sakiy > sakiyomi)
				sakiy = sakiyomi;
			return sakiy;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ptr">出力するＮＣ情報のouuインデックス番号</param>
		/// <param name="ouu">ouu[ptr][0]:最終位置、ouu[ptr][1]:最終直前の中間位置、ouu[ptr][2]:その前の中間位置、ouu[ptr+1][0]移動前の位置</param>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		private void Ncoput(int ptr, NcOut[] ouu, NcMod[] mod, OCode[] lcode)
		{
			int ii, jj, kk, chu;
			int imod;
			//int ncoutm();
			//ncout *tmpo[3];
			NcOuts[] tmpo = new NcOuts[3];
			//int ncdist();

			imod = ouu[ptr].imac;

			for (ii = NcOut.CHUU; ii >= 0; ii--) {
				if (ouu[ptr][ii] == null)
					continue;
				// 移動前の位置tmpo[1]を求める
				if (ii != NcOut.CHUU && ouu[ptr][ii + 1] != null)
					tmpo[1] = ouu[ptr][ii + 1];
				else
					tmpo[1] = ouu[ptr + 1][0];
				// 加工点tmpo[0]を求める
				tmpo[0] = ouu[ptr][ii];
				// 移動先の位置tmpo[2]を求める
				if (ii != 0)
					tmpo[2] = ouu[ptr][ii - 1];
				else if (ptr > 0)
					tmpo[2] = ouu[ptr - 1][0];
				else
					tmpo[2] = null;

				// SET sdst
				chu = _main.sdst.NcDist(tmpo, mod[imod], fsub);

				// MIDDLE OUTPUT
				if (ii != 0) {
					if ((NcMachine.ParaData(1400, 0) & 0x10) != 0) throw new Exception("早送り直線補間でエラー");
					//if (hichoku == false && chu != 0)
					if (hichoku == false)
						jj = imod + 1;
					else
						jj = imod;
				}
				else
					jj = (ptr > 0) ? ouu[ptr - 1].imac + 1 : 0;

				for (kk = imod; kk >= jj; kk--) {
					if (lcode[kk].subdep1.subs < 0)
						throw new Exception("lcode.subdep0の廃止のためのチェック");
					if (lcode[kk].subdep1.subs >= 0)
						//_main.main.ncoutm(tmpo[0], mod[kk], lcode[kk]);
						this._main_.Ncoutm(tmpo[0], mod[kk], lcode[kk]);
				}
			}
		}

		/// <summary>
		/// nccodeのエラー処理
		/// </summary>
		/// <param name="ierr">エラー番号</param>
		/// <param name="ddat">エラー発生のＮＣデータ行</param>
		/// <param name="line">エラー発生の行番号</param>
		private void IERR(int ierr, string ddat, int line) {
			switch (ierr) {
			case -1:
				throw new Exception("statment error --- " + ddat);
			case 1:
				throw new Exception($"{readInfo.ncnam} ( {ddat} ) NC-DATA number (line={line})");
			case 2:
				throw new Exception($"{readInfo.ncnam} ( {ddat} ) NC-DATA character (line={line})");
			default:
				throw new Exception($"{readInfo.ncnam} ( {ddat} ) error ????? (line={line})");
			}
		}
	}
}
