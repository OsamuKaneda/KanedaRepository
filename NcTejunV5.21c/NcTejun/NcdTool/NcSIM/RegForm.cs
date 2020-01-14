using System;
using System.Collections.Generic;
using System.Text;

namespace NcdTool.NcSIM
{
	/// <summary>
	/// NCSPEED, Eurika出力のＮＣデータを標準フォーマットに戻す
	/// </summary>
	class RegForm
	{
		/// <summary>終了コード</summary>
		static readonly char[] stdEndCode = new char[] { ';' };
		/// <summary>ＸＹコード</summary>
		static readonly char[] xyCode = new char[] { 'X', 'Y' };


		/// <summary>送り速度に比率をかける場合（送り速度最適化の場合はアプローチ・リトラクトのみ）</summary>
		public bool FeedSetArea { get { return smFDC ? !isCut : true; } }

		/// <summary>ＮＣデータ名（エラーメッセージ用）</summary>
		private readonly string ncnam;
		/// <summary>サブコール中のＮＣ行の場合：true</summary>
		private bool isSub;
		/// <summary>ＤＢの加工条件で加工中の場合：true</summary>
		private bool isCut;

		/// <summary>送り速度最適化の場合：true</summary>
		private readonly bool smFDC;
		/// <summary>分割され最初の"(CUT START)"がないＮＣデータの場合true</summary>
		private bool cutBunk;
		/// <summary>"(SIM START)"からの行番号</summary>
		private int simNum;
		/// <summary>"傾斜の場合はtrue(1)</summary>
		private readonly bool simKeisha;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ncname">ＮＣデータ情報</param>
		public RegForm(NcName.NcNam ncname) {
			this.ncnam = ncname.nnam;
			this.isSub = false;
			this.isCut = false;

			this.smFDC = false;
			this.cutBunk = false;
			this.simNum = 0;
			this.simKeisha = false;

			if (ncname.Ncdata.ncInfo.xmlD.SmNAM != null) {
				// シミュレーションのＮＣデータであるため、
				if (ncname.Itdat.Value != 1) throw new Exception("afrqref");
				this.smFDC = ncname.Tdat[0].Tld.XmlT.SmFDC;

				// 分割され、"(CUT START)"前に"(CUT END)", "(SUB START)"が存在するＮＣデータかチェックする
				string ddat;
				int numb = 0;
				using (System.IO.StreamReader fp = new System.IO.StreamReader(ncname.Ncdata.fulnamePC)) {
					while (true) {
						ddat = fp.ReadLine();
						numb++;
						if (ddat == "(CUT START)" || ddat == "(CUT END)" || ddat == "(SIM END)") break;
					}
				}
				switch (ddat) {
				case "(CUT START)":
					break;
				case "(CUT END)":
					this.cutBunk = true;
					break;
				case "(SIM END)":
					this.cutBunk = true;
					//
					// (CUT END) がなく、切削加工データもないと思われる場合はエラーとする
					if (numb < 12) {
						CamUtil.LogOut.CheckCount("RegForm 75", false, "切削加工部分の無いＮＣデータが見つかりました。in " + ncnam);
						if (this.smFDC)
							throw new Exception(
								"分割された送り速度最適化ＮＣデータで加工終了位置の情報が見つかりませんでした。in NCSPEED\n" +
								"V5.01以前の古いバージョンである、切削加工部分の無いＮＣデータである、などが考えられます。");
					}
					//
					break;
				default:
					throw new Exception("SIM END が存在しない");
				}
				// 傾斜加工かチェック
				if (ncname.Tdat[0].Tld.XmlT.Keisha) simKeisha = true;
				if (ncname.Ncdata.ncInfo.xmlD.SmNAM == "EUREKA") simKeisha = true;	// 藤井君のエラーの対応 2017/08/31
			}
		}

		/// <summary>
		///  ＮＣＳＰＥＥＤのコメントを削除する（展開した検証用の部分も削除）
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>
		public string Conv(string txt) {
			string endCode;
			if (txt.LastIndexOfAny(stdEndCode) == txt.Length - 1)
				endCode = stdEndCode[0].ToString();
			else
				endCode = "";
			// 空の行の場合に終了コード";"が追加されるバグを修正 2019/04/23 磯部
			if (txt.Length == 0) {
				endCode = "";
			}
			if (endCode != "") {
				CamUtil.LogOut.CheckCount("RegForm 0114", false, "ＮＣデータ終了コード';' が使用された");
			}

			// シミュレーションで使用していた工程間移動関連のマクロコールを元のサブコールに戻す
			// string ddat = txt.Replace("G65P969", "M98P969").TrimEnd(StdEndCode);
			string ddat = CamUtil.ClLink5Axis.ChangeNormalCode(txt).TrimEnd(stdEndCode);

			// ////////////////////////////////////
			// 最初の"(CUT START)"がない場合の処理
			// ////////////////////////////////////
			if (simNum > 0) {
				simNum++;
				if (cutBunk) {
					if (simKeisha) {
						// /////////////////////////////////////////
						// 傾斜加工時の切削加工までの動作をチェック
						// /////////////////////////////////////////
						switch (simNum) {
						case 2:		// 傾斜の場合の２行目
							if (ddat.IndexOf(CamUtil.ClLink5Axis.Start) < 0) throw new Exception("傾斜加工時の工具分割エラー");
							break;
						case 3:		// ＸＹの早送り移動
							if (ddat.IndexOfAny(xyCode) < 0 || ddat.IndexOf('Z') >= 0) throw new Exception("傾斜加工時の工具分割エラー");
							break;
						case 4:		// Ｚの早送り移動
							if (ddat.IndexOfAny(xyCode) >= 0 || ddat.IndexOf('Z') < 0) throw new Exception("傾斜加工時の工具分割エラー");
							break;
						case 5:		// 工具軸の早送り　あるいはサブコール
							if (ddat == "(SUB START)") {
								isCut = true;
								cutBunk = false;
								break;
							}
							if (ddat.IndexOf("G01") >= 0) throw new Exception("傾斜加工時の工具分割エラー");
							break;
						case 6:		// 工具軸のアプローチ
							if (ddat.IndexOf("G01") < 0) throw new Exception("傾斜加工時の工具分割エラー");
							break;
						case 7:		// 切削加工
							isCut = true;
							cutBunk = false;
							break;
						}
					}
					else {
						// /////////////////////////////////////////
						// 傾斜なし加工時の切削加工までの動作をチェック
						// /////////////////////////////////////////
						switch (simNum) {
						case 2:		// ＸＹの早送り移動
							if (ddat.IndexOfAny(xyCode) < 0 || ddat.IndexOf('Z') >= 0) throw new Exception("qfrwrujfgwsrgvuj");
							break;
						case 3:		// Ｚの早送り移動
							if (ddat.IndexOfAny(xyCode) >= 0 || ddat.IndexOf('Z') < 0) throw new Exception("qfrwrujfgwsrgvuj");
							break;
						case 4:		// 工具軸のアプローチ　あるいはサブコール
							if (ddat == "(SUB START)") {
								isCut = true;
								cutBunk = false;
								break;
							}
							if (ddat.IndexOfAny(xyCode) >= 0 || ddat.IndexOf('Z') < 0) throw new Exception("qfrwrujfgwsrgvuj");
							break;
						case 5:		// 切削加工
							isCut = true;
							cutBunk = false;
							break;
						}
					}
				}
			}

			// ////////////////////////////////////
			// モード設定とサブプロの処理
			// ////////////////////////////////////
			switch (ddat) {
			case "(SIM START)":
			case "(SIM END)":
				if (isSub)
					throw new Exception("NCSPEEDエラー in " + ncnam + "。SUBのSTART,ENDが対応していない");
				//分割された場合、"(CUT END)"なしに終了する場合があるためエラーとしない
				//（この場合G00X100.0の移動であるため、加工中と判断しても特に問題とはならない）
				//if (isCut)
				//	throw new Exception("NCSPEEDエラー in " + ncnam + "。CUTのSTART,ENDが対応していない");
				if (ddat == "(SIM START)") simNum = 1;
				ddat = null;
				break;
			case "(SUB START)":
				if (isSub)
					throw new Exception("NCSPEEDエラー in " + ncnam + "。SUBのSTART,ENDが対応していない");
				isSub = true;
				ddat = null;
				break;
			case "(SUB END)":
				if (isSub == false)
					throw new Exception("NCSPEEDエラー in " + ncnam + "。SUBのSTART,ENDが対応していない");
				isSub = false;
				ddat = null;
				break;
			case "(CUT START)":
				if (isSub)
					throw new Exception("NCSPEEDエラー in " + ncnam + "。CUTのSTART,ENDが対応していない");
				if (isCut)
					throw new Exception("NCSPEEDエラー in " + ncnam + "。CUTのSTART,ENDが対応していない");
				isCut = true;
				ddat = null;
				break;
			case "(CUT END)":
				if (isSub)
					throw new Exception("NCSPEEDエラー in " + ncnam + "。CUTのSTART,ENDが対応していない");
				if (isCut == false)
					throw new Exception("NCSPEEDエラー in " + ncnam + "。CUTのSTART,ENDが対応していない");
				isCut = false;
				ddat = null;
				break;
			default:
				if (isSub) {
					if (ddat.IndexOf('(') == 0) {
						if (ddat.IndexOf(')') != ddat.Length - 1) throw new Exception("aeffrwqaref");
						ddat = ddat.Substring(1, ddat.Length - 2) + endCode;
					}
					else
						ddat = null;
				}
				else {
					if (ddat.IndexOf(CamUtil.ClLink5Axis.Start) == 0)
						ddat = null;
					// ADD in 2015/0827
					else if (ddat == "G00G90X0Y0Z100000")
						ddat = "G00G90X0Y0" + endCode;
					else
						ddat += endCode;

				}
				break;
			}
			return ddat;
		}
	}
}
