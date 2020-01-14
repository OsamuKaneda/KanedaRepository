using System;
using System.Collections.Generic;
using System.Text;

//#pragma warning disable 1591
namespace NcCode.nccode
{
	/// <summary>
	/// ＮＣデータ１行の情報を保存します。[不変]
	/// </summary>
	public class OCode
	{
		/// <summary>
		/// Ｇコードの設定状況
		/// 0:正常 1:Ｇコード無視 2:Ｇコードの１行無視 3:続行不能
		/// </summary>
		public readonly int gsw;
		/// <summary>ＧコードグループごとのＧコード設定値</summary>
		public CamUtil.RO_Collection<CamUtil.LCode.Gcode> Gg { get { return m_gg.AsReadOnly; } } private readonly CamUtil.RO_Collection<CamUtil.LCode.Gcode>.InnerArray m_gg;

		/// <summary>Ｍコードの設定値（１～）、mst[0]は１行の設定数</summary>
		public CamUtil.RO_Collection<int> Mst { get { return m_mst.AsReadOnly; } } private readonly CamUtil.RO_Collection<int>.InnerArray m_mst;

		/// <summary>ＮＣデータ１行のコードを順に保管した情報（移動、ミラーをまとめて実施）</summary>
		public readonly CodeDList codeData;

		/// <summary>G10,G11の設定状況（-2:未設定 -1:G11 0:G10）</summary>
		public readonly int g10;
		/// <summary>送り速度指定方法（Ｆ一桁送りの場合はFalse）</summary>
		public readonly bool nrmFeed;

		/// <summary>プログラム深さ（この行実行後の）</summary>
		public readonly Ncdep subdep1;

		/// <summary></summary>
		internal readonly NcRW.Mthd submthd;

		/// <summary>ＮＣデータの行ナンバー</summary>
		public readonly int ncln;

		/// <summary>入力時のＮＣデータ一行の文字列（移動ミラーは反映されていないので注意！！）</summary>
		public readonly string nctx;

		/// <summary>コメントの文字列</summary>
		public readonly string cmt;

		/// <summary>Ｇコードマクロ呼出しでのマクロプログラム番号</summary>
		public int Subc {
			get {
				int iout = 0;
				if (Gg[0].Gst && Gg[0].MacroCall) iout = Gg[0].macName;
				if (Gg[12].Gst && Gg[12].MacroCall) iout = Gg[12].macName;
				return iout;
			}
		}

		/// <summary>
		/// Ｇコードマクロを含むモーダルなマクロコールを設定する行であるか判定（G67を含む）。
		/// これにgg[0].Equals(65)を加えれば、すべてのマクロコールの設定行となる。
		/// </summary>
		public bool Gst12 { get { return m_gg[12].Gst; } }
		/// <summary>固定サイクルを設定する行であるか</summary>
		public bool Gst09 { get { return m_gg[9].Gst; } }

		/// <summary>
		/// ＮＣデータ１行の内容を解析しOCodeを作成する
		/// </summary>
		/// <param name="retValue"></param>
		/// <param name="line">ＮＣデータの行ナンバー</param>
		/// <param name="codeD_ncspd">ＮＣデータ行</param>
		/// <param name="start">開始モード</param>
		/// <param name="mthd"></param>
		/// <param name="_main_">呼出し元の__mainクラス</param>
		/// <param name="trs_mir">マクロ展開時に反映する移動・ミラー・反転情報。NCSPEEDで使用し、他では展開後メインに組み込まれるため反映しない</param>
		/// <param name="sdep"></param>
		/// <returns></returns>
		internal OCode(out int? retValue, int line, CodeDList codeD_ncspd, int start, NcRW.Mthd mthd, _main _main_, Transp_Mirror trs_mir, Ncdep sdep) {
			retValue = null;
			List<CodeD> ltmp = new List<CodeD>();
			CodeD codeD;

			{
				gsw = 0;
				m_gg = new CamUtil.RO_Collection<CamUtil.LCode.Gcode>.InnerArray(CamUtil.LCode.Gcode.GGRPNUM);
				m_gg[0] = new CamUtil.LCode.Gcode();	// 初期化
				for (int ii = 1; ii < m_gg.Length; ii++)
					m_gg[ii] = new CamUtil.LCode.Gcode();	// 初期化
				m_mst = new CamUtil.RO_Collection<int>.InnerArray(11);
				//m_mst[0] = 0;
				m_mst[0] = 0;
				nrmFeed = true;
				cmt = "";
				//m_subc = 0;
				g10 = -2;
				//gcodeList = new List<Gcode>();
			}

			// Mthd.MODE.NORMAL以外の場合はCodeDListを使うようになった。
			//if (mthd.control != NcRW.Mthd.MODE.NORMAL) {
			//	throw new Exception("qwefqerbfqer");
			//}

			subdep1 = sdep;
			ncln = line;
			nctx = codeD_ncspd.NcText;
			submthd = mthd;
			codeData = new CodeDList(ltmp);		// 途中でreturnする場合のためにセットしておく

			// まだ、'%'が実行される前の無意味のデータの場合は
			// 最初のデータが'%'以外の場合はデータを解析しない
			if (start == -1) {
				if (codeD_ncspd.Count > 0 && codeD_ncspd[0].ncChar == '%') { ;}
				else { retValue = 0;  return; }
			}

			int spd_cnt = 0;
			string sCode;
			while (spd_cnt < codeD_ncspd.Count) {
				codeD = codeD_ncspd[spd_cnt];
				switch (codeD.ncChar) {
				case ';':
					ltmp.Add(codeD);
					{ retValue = 0; break; }
				case '/':
					if (start != 0 || this.submthd.control != NcRW.Mthd.MODE.NORMAL) { retValue = 0; break; }
					ltmp.Add(codeD);
					if (NcMachine.oskip == true) { retValue = 0; break; }
					break;
				case '(':
					if (start != 0 || this.submthd.control != NcRW.Mthd.MODE.NORMAL) { retValue = 0; break; }
					ltmp.Add(codeD);
					this.cmt = codeD.String();
					break;
				case '%':
					ltmp.Add(codeD);
					break;
				case '#':	/* macro value settei  */
					sCode = codeD.String();
					// マクロ（代入文）
					if (sCode[0] == '#') {
						if (codeD.SetVariable != true) throw new Exception("代入文と制御文を区分するsetVariableのエラー");
						if (start != 0 || this.submthd.control != NcRW.Mthd.MODE.NORMAL) { retValue = 0; break; }

						// 代入の実行 SystemSet ===== SystemSet_NCCODE_Newへ移動 =====
						//m_fsub[codeD.vno.Value] = codeD.vdt;
						//icnt0++;

						ltmp.Add(codeD);
					}
					// マクロ（制御文）
					else {
						if (codeD.SetVariable == true) throw new Exception("代入文と制御文を区分するsetVariableのエラー");
						if (start != 0 || this.submthd.control == NcRW.Mthd.MODE.GOTO) { retValue = 0; break; }

						//WHILE_NOのチェック（WHILE と同一行の DO でここに来る）
						if (this.submthd.control == NcRW.Mthd.MODE.WHILE_NO) {
							if (codeD.StringMacro() != "DO") throw new Exception("aerfqwervfg");
						}

						// 制御文の処理
						this.submthd = new NcRW.Mthd(this.submthd, codeD);

						ltmp.Add(codeD);

						// IF文が false の場合、以降のコードは実行しない
						if (codeD.StringMacro() == "IF") {
							if (codeD.ToBoolean == false) {
								while (spd_cnt + 1 < codeD_ncspd.Count) {
									if (codeD_ncspd[spd_cnt + 1].ncChar == ';') break;
									if (codeD_ncspd[spd_cnt + 1].ncChar == '(') break;
									spd_cnt++;
								}
							}
						}
					}
					break;
				default:
					// NORMAL NCDATA
					if (start == 1 && this.submthd.control == NcRW.Mthd.MODE.NORMAL && codeD.ncChar == 'O')
						this.submthd = this.submthd.UpDate(9999);
					if (start != 0) { retValue = 0; break; }
					if (this.submthd.control != NcRW.Mthd.MODE.NORMAL && codeD.ncChar != 'N' && codeD.ncChar != 'O') { retValue = 0; break; }

					if (codeD.CodeOutput) {
						ltmp.Add(codeD);
						//ncset1(ref this.nrmFeed, this.m_gg, this.m_mst, codeD);

						// /////////////////////////////////
						// Ｆコード（nrmFeed, gg[1]）の設定
						// /////////////////////////////////
						if (codeD.ncChar == 'F') {
							this.nrmFeed = true;
							// Ｆの一桁送りの場合
							if (codeD.decim == false && codeD.ToInt <= 9) {
								if (codeD.ToInt == 0)
									this.m_gg[1] = new CamUtil.LCode.Gcode(0);
								else if (codeD.String().Substring(1).Length == 1) {
									CamUtil.LogOut.CheckCount("OCode 317", false, "Ｆ１桁送りの設定");
									this.nrmFeed = false;
								}
							}
						}

						// //////////////////////
						// Ｍコード（mst）の設定
						// //////////////////////
						if (codeD.ncChar == 'M') {
							if (Gg[0].Equals(65) || (Gst12 && subdep1.gg12.Equals(66.1)) || (Gst12 && subdep1.gg12.Equals(66))) {
							}
							else {
								this.m_mst[0]++;
								this.m_mst[m_mst[0]] = codeD.ToInt;
							}
						}

						// //////////////////////////////////////////////
						// Ｇコード（gg, gsw, subc, subdep1, g10）の設定
						// //////////////////////////////////////////////
						if (codeD.ncChar == 'G') {
							CamUtil.LCode.Gcode gcd = new CamUtil.LCode.Gcode(codeD.ToDouble, NcMachine.sgc, NcMachine.GCodeMacro);
							// 送り速度（毎分、毎回転）の倍率設定
							if (gcd.group == 5) {
								if (gcd.ToDouble() == 94.0) Post.FcodeSet(1);      /* F */
								if (gcd.ToDouble() == 95.0) Post.FcodeSet(100);    /* F */
							}
							this.m_gg[gcd.group] = gcd;
							//this.gcodeList.Add(gcd);
							if (gcd.gsw > this.gsw) this.gsw = gcd.gsw;
							NcCodg(ref subdep1, ref this.g10, gcd);
						}
					}
					break;
				}
				if (retValue.HasValue) break;
				spd_cnt++;
			}
			codeData = new CodeDList(ltmp);

			// //////////////////////////////////////////////
			// 移動、ミラーの設定
			// //////////////////////////////////////////////
			// ドウェル行は実行しない in 2018/10/09
			if (subdep1.Depth == 0 && trs_mir != null && this.Gg[0].Equals(4) == false) {
				codeData = new CodeDList(Trans_Mirror(trs_mir));
				m_gg = new CamUtil.RO_Collection<CamUtil.LCode.Gcode>.InnerArray(CamUtil.LCode.Gcode.GGRPNUM);
				for (int ii = 0; ii < CamUtil.LCode.Gcode.GGRPNUM; ii++) m_gg[ii] = new CamUtil.LCode.Gcode();  // 初期化
				CamUtil.LCode.Gcode gcd;
				foreach (CodeD cod in codeData) if (cod.ncChar == 'G') {
						gcd = new CamUtil.LCode.Gcode(cod.ToDouble, NcMachine.sgc, NcMachine.GCodeMacro);
						m_gg[gcd.group] = gcd;
					}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="trs_mir"></param>
		private List<CodeD> Trans_Mirror(Transp_Mirror trs_mir) {
			CodeD item;
			CamUtil.CamNcD.MacroCode mCode;

			// 固定サイクル（G80を除く）
			if (this.subdep1.gg09.Equals(80.0) == false)
				mCode = new CamUtil.CamNcD.MacroCode(Post.pName, this.subdep1.gg09.ToInt());
			// カスタムマクロ（G67を除く）
			else if (this.subdep1.gg12.Equals(67) == false || this.Gg[0].Equals(65.0)) {
				// カスタムマクロ設定行（G67を除く）
				if (this.Gst12 || this.Gg[0].Equals(65.0)) {
					// Ｇコード呼出しマクロ（単純、モーダル）
					if (this.Subc > 0)
						mCode = new CamUtil.CamNcD.MacroCode(Post.pName, this.Subc);
					// 一般呼出しマクロ（単純、モーダル）
					else
						mCode = new CamUtil.CamNcD.MacroCode(Post.pName, this.codeData.CodeData('P').ToInt);
				}
				// G66モーダル行（基本ＸＹのみなので一般ＮＣデータに準ずる）
				else if (this.subdep1.gg12.Equals(66))
					mCode = CamUtil.CamNcD.MacroCode.NORM_NC;
				// G66.1モーダル行
				else
					throw new Exception("G66.1のモーダル行には対応していません。G65を使用してください。");
			}
			// G80, G67 を含む一般ＮＣデータ行
			else
				mCode = CamUtil.CamNcD.MacroCode.NORM_NC;

			switch (mCode.kako) {
			case CamUtil.CamNcD.MacroCode.KAKO.K_CHK:
				if (CamUtil.ProgVersion.Debug)
					CamUtil.LogOut.CheckCount("CodeD 424", true, "移動時の処理方法が登録されていないマクロが使用されました。");
				else
					throw new Exception("移動時の処理方法が登録されていないマクロが使用されました。");
				break;
			case CamUtil.CamNcD.MacroCode.KAKO.K_TRU:
				break;
			case CamUtil.CamNcD.MacroCode.KAKO.K_FLS:
				throw new Exception("移動時の処理が定義できないマクロが使用されました。");
			case CamUtil.CamNcD.MacroCode.KAKO.K_NON:
				break;
			}

			List<CodeD> ndList = new List<CodeD>();
			for (int ii = 0; ii < this.codeData.Count; ii++) {
				item = this.codeData[ii];
				// 移動ミラーを実行し item に反映
				// G100の次の行（原点座標）も移動されるので注意（NCSPPEDは以降に修正）
				if (trs_mir.m_ido != CamUtil.Vector3.v0)
					item = item.Transp(trs_mir.m_ido, this, mCode);
				if (trs_mir.mirr != CamUtil.NcZahyo.Null)
					item = item.Mirror2(trs_mir.mirr, this, mCode);
				if (trs_mir.rev && item.ncChar == 'W')
					item = item.Reverse(this);
				ndList.Add(item);
			}
			return ndList;
		}

		/// <summary>
		/// Ｇコードの処理
		/// </summary>
		/// <returns></returns>
		private void NcCodg(ref Ncdep p_subdep1, ref int p_g10, CamUtil.LCode.Gcode gchg)
		{
			if (gchg.MacroCall) {
				//p_subc = gchg.macName;
			}
			else {
				if (gchg.gsw == 0) {
					// Ｇコードグループ１（G00, G01, G02, G03）で固定サイクルモードはオフになる
					if (gchg.group == 1 && !p_subdep1.gg09.Equals(80))
						p_subdep1 = p_subdep1.UpDateFix(p_subdep1.subf, new CamUtil.LCode.Gcode(80));

					// データ設定
					if (gchg.group == 0 && gchg.Equals(10)) p_g10 = 0;
					if (gchg.group == 0 && gchg.Equals(11)) p_g10 = -1;
				}
			}
			if (gchg.group == 9)
				p_subdep1 = p_subdep1.UpDateFix(p_subdep1.subf, gchg);
			else if (gchg.group == 12)
				p_subdep1 = p_subdep1.UpDateMac(p_subdep1.subm, gchg);

			return;
		}

		/// <summary>このインスタンスと指定した OCode オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public bool Equals(OCode obj) {

			//if (!this.m_fsub.Equals(obj.m_fsub)) return false;
			if (this.gsw != obj.gsw) return false;
			for (int ii = 0; ii < CamUtil.LCode.Gcode.GGRPNUM; ii++)
				if (!this.Gg[ii].Equals(obj.Gg[ii])) return false;

			if (!codeData.Equals(obj.codeData)) return false;
			for (int ii = 0; ii < 11; ii++)
				if (this.Mst[ii] != obj.Mst[ii]) return false;

			if (this.nrmFeed != obj.nrmFeed) return false;

			if (this.cmt != obj.cmt) return false;
			//if (this.subc != obj.subc) return false;
			if (this.g10 != obj.g10) return false;
			//if (!this.subdep0.Equals(obj.subdep0)) return false;
			if (this.subdep1 != obj.subdep1) return false;
			if (this.submthd.control != obj.submthd.control) return false;
			if (this.submthd.gotoNo != obj.submthd.gotoNo) return false;

			if (this.ncln != obj.ncln) return false;
			if (this.nctx != obj.nctx) return false;
			//if (this.sub_close != obj.sub_close) return false;
			//if (this.transp != obj.transp) return false;
			//if (this.mirror != obj.mirror) return false;
			//if (this.reverse != obj.reverse) return false;
			return true;
		}
	}
}
