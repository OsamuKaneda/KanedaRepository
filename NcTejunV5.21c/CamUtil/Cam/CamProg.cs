using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace CamUtil
{
	/// <summary>
	/// プログラムのバージョンを管理します。バージョン番号は major, minor, revision で、試行版の区分は rtm で設定します。
	/// </summary>
	static public class ProgVersion
	{
		// ////////////////////////////////////////////////////////
		/// <summary>バージョン番号</summary>
		private const short major = 5, minor = 21; private const char revision = '0';
		/// <summary>正式版 0、試行版 >0
		/// 試行版1 1bitON 0b0001: 部品加工の同時５軸対応版(坂本)
		/// 試行版2 2bitON 0b0010: 部品加工の倒れ補正（熊崎）
		/// 試行版3 3bitON 0b0100:
		/// 試行版4 4bitON 0b1000:
		/// </summary>
		private static byte rtm = 0b0000;
		// ////////////////////////////////////////////////////////

		/// <summary>試行版１ではない場合 true</summary>
		static public bool NotTrialVersion1 { get { return (rtm & 1) == 0; } }
		/// <summary>試行版２ではない場合 true</summary>
		static public bool NotTrialVersion2 { get { return (rtm & 2) == 0; } }
		/// <summary>試行版３ではない場合 true</summary>
		static public bool NotTrialVersion3 { get { return (rtm & 4) == 0; } }
		/// <summary>試行版４ではない場合 true</summary>
		static public bool NotTrialVersion4 { get { return (rtm & 8) == 0; } }

		/// <summary>デバッグモードか否かのフラッグ</summary>
		static public bool Debug { get { return m_debug; } } static private bool m_debug = false;

		/// <summary>実行プログラムのフルパス名</summary>
		static public string ProgFull = null;
		/// <summary>実行プログラムの更新時間</summary>
		static public DateTime ProgTime;

		/// <summary>バージョン名0</summary>
		static public string VerName0 = String.Format("V{0:d}.{1:d}R{2}{3}", major, minor, revision, rtm == 0 ? "" : ("_" + rtm.ToString()));
		/// <summary>バージョン名1</summary>
		static public string VerName1 = String.Format("V{0:d}.{1:d} Revision {2}", major, minor, revision);
		/// <summary>バージョン名2</summary>
		static public string VerName2 = String.Format("_V{0:d}-{1:d}R{2}", major, minor, revision);

		// ////////////////////////////////////////////////////////
		// 計算速度の基準バージョン		V5.11R5
		// 計算速度の基準ＰＣ			SATB552-010

		/// <summary>３次元ＮＣデータ出力時の暫定処理チェック確率</summary>
		static public int NcOutChkProb = 10;
		/// <summary>NCSEND2実行時間(sec) CAMTOOL/SPD_SAMPLE/JUCD01-20 </summary>
		private const int spd_ncsnd = 32;
		/// <summary>NCSPEED実行時間(sec) PTP/_TEST_speed/MQFK30-41 </summary>
		private const int spd_ncspd = 54;
		/// <summary>OUTPUT実行時間(sec) PTP/_TEST_speed/MQFK30-41(ORG) </summary>
		private const int spd_ncouO = 95;
		/// <summary>OUTPUT実行時間(sec) PTP/_TEST_speed/MQFK30-41(EDIT) </summary>
		private const int spd_ncouE = 91;

		// このバージョンでの実行時間比率
		/// <summary>NCSEND2実行時間比率 CAMTOOL/SPD_SAMPLE/JUCD01-20 </summary>
		private const double rat_ncsnd = 0.94;
		/// <summary>NCSPEED実行時間比率 PTP/_TEST_speed/MQFK30-41 </summary>
		private const double rat_ncspd = 0.91;
		/// <summary>OUTPUT実行時間比率 PTP/_TEST_speed/MQFK30-41(ORG) </summary>
		private const double rat_ncouO = 1.04;
		/// <summary>OUTPUT実行時間比率 PTP/_TEST_speed/MQFK30-41(EDIT) </summary>
		private const double rat_ncouE = 1.09;
		// ////////////////////////////////////////////////////////

		/// <summary>バージョンのチェック</summary>
		static public bool CheckVersion(string progName) {
			if (ProgFull == null) {
				ProgFull = Path.GetFullPath(progName);
				ProgTime = File.GetLastWriteTime(ProgFull);
				// デバッグ中のセット
				if (ProgFull.IndexOf("bin\\Debug\\") >= 0) m_debug = true;
			}
			string newexe = ServerPC.ExecFldr + Path.GetFileName(ProgFull);
			if (!File.Exists(newexe)) {
				System.Windows.Forms.MessageBox.Show(@"\\nt0040np\tgsite_data に接続できませんでした。");
				return true;
			}
			if (Debug == false && ((TimeSpan)(File.GetLastWriteTime(newexe) - ProgTime)).TotalMinutes > 1.0) {
				System.Windows.Forms.MessageBox.Show($"新しい{Path.GetFileName(ProgFull)}のプログラムが存在します。\n{ServerPC.ExecFldr}より更新してください");
				return true;
			}
			LogOut.UpdateCheckListExcept();
			return false;
		}
	}

	/// <summary>
	/// プログラムのログ出力を管理します。
	/// </summary>
	static public class LogOut
	{
		/// <summary>ワーニングのメッセージ</summary>
		static public StringBuilder warn;

		/// <summary>プログラム実行ログ出力の停止リスト</summary>
		static private List<string> checkListExcept = null;
		/// <summary>プログラム実行ログ出力の出力済リスト</summary>
		static public List<string> OutputList;
		/// <summary>プログラム実行ログの回数リスト</summary>
		static public List<int> OutputListCnt;

		/// <summary>LogOutのログ出力のイベントを設定</summary>
		public static Action idle_exit;

		/// <summary>ログを保存するファイル名</summary>
		public enum FNAM
		{
			/// <summary>Log_NCSEND2.txt</summary>
			NCSENDLOG,
			/// <summary>Log_NcOutput.txt</summary>
			NCDOUTPUT,
			/// <summary>Log_MacroTenkai.txt</summary>
			MACROTENK,
			/// <summary>Log_BuhinFcode.txt</summary>
			BUHINFEED,
			/// <summary>Log_Convert.txt</summary>
			NCCONVERT,
			// /// <summary>Log_ToolSetInfo_ERROR.txt</summary>
			// TLSETINFO,
			// /// <summary>Log_TSHEET_ERROR.txt</summary>
			// TOOLSHEET,
			/// <summary>Log_BUNKATSU.txt</summary>
			BUHINBUNK,
			/// <summary>Log_ToolMatching.txt</summary>
			TMATCHING,
			// /// <summary>Log_CONV_ERROR.txt</summary>
			// CONVERROR,
			/// <summary>Log_NCSPEEDJunbi.txt</summary>
			NCSPEEDLG,
			/// <summary>最大値。常に最後においておくこと</summary>
			MAX
		}
		private static readonly string[] fnam;

		static LogOut() {
			warn = new StringBuilder();
			checkListExcept = new List<string>();
			OutputList = new List<string>();
			OutputListCnt = new List<int>();

			fnam = new string[(int)FNAM.MAX];
			fnam[(int)FNAM.NCSENDLOG] = "Log_NCSEND2.txt";
			fnam[(int)FNAM.NCDOUTPUT] = "Log_NcOutput.txt";
			fnam[(int)FNAM.MACROTENK] = "Log_MacroTenkai.txt";
			fnam[(int)FNAM.BUHINFEED] = "Log_BuhinFcode.txt";
			fnam[(int)FNAM.NCCONVERT] = "Log_Convert.txt";
			fnam[(int)FNAM.BUHINBUNK] = "Log_BUNKATSU.txt";
			fnam[(int)FNAM.TMATCHING] = "Log_ToolMatching.txt";
			fnam[(int)FNAM.NCSPEEDLG] = "Log_NCSPEEDJunbi.txt";
		}

		/// <summary>プログラム実行ログ出力の停止リストの更新</summary>
		static public void UpdateCheckListExcept() {
			string ss;
			string fnam0 = String.Format("{0}Log__CheckListExcept{1}.txt", ServerPC.SvrFldrC, "");
			string fnam1 = String.Format("{0}Log__CheckListExcept{1}.txt", ServerPC.SvrFldrC, ProgVersion.VerName2);
			using (StreamReader sr = File.Exists(fnam1) ? new StreamReader(fnam1) : new StreamReader(fnam0)) {
				checkListExcept.Clear();
				while (!sr.EndOfStream) {
					ss = sr.ReadLine();
					if (ss.IndexOf("//") >= 0) ss = ss.Remove(ss.IndexOf("//"));
					ss = ss.Trim();
					if (ss.Length > 0)
						checkListExcept.Add(ss);
				}
			}
		}

		/// <summary>
		/// =========================================================
		/// プログラムの変更に伴う暫定チェック
		/// =========================================================
		/// </summary>
		static public void CheckZantei(string nnam,
			[System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = "",
			[System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
			[System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0) {
			CheckCount($"{Path.GetFileNameWithoutExtension(callerFilePath)} {callerLineNumber:d4}", false, nnam);
		}
		/// <summary>
		/// =========================================================
		/// プログラムの変更に伴う暫定チェックのエラー処理
		/// =========================================================
		/// </summary>
		static public void CheckError(string nnam,
			[System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = "",
			[System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "",
			[System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0) {
			CheckCount($"{Path.GetFileNameWithoutExtension(callerFilePath)} {callerLineNumber:d4}", false, nnam);
			CheckCountOutput();
			throw new Exception(nnam);
		}

		/// <summary>プログラム実行ログを出力する</summary>
		static public void CheckCount(string id, bool message, string nnam) {
			string text; int index;
			if (checkListExcept.Contains(id)) return;
			text = String.Format("{0:yyyy/MM/dd HH:mm} {1} {2} {3}", DateTime.Now, ProgVersion.VerName0, id, nnam);
			if ((index = OutputList.IndexOf(text)) >= 0) {
				OutputListCnt[index]++;
				return;
			}
			if (OutputList.Count == 0) idle_exit();	// イベントの設定
			OutputList.Add(text);
			OutputListCnt.Add(1);
			// メッセージの表示
			if (message) System.Windows.Forms.MessageBox.Show(nnam + " in " + id);
			return;
		}
		/// <summary>プログラム実行ログを出力する（アイドルイベントから実行される）</summary>
		static public void CheckCountOutput() {
			for (int ii = 0; ii < OutputList.Count; ii++)
				File.AppendAllText(ServerPC.SvrFldrC + "Log__CheckCount.txt", $"{OutputList[ii]} ({OutputListCnt[ii].ToString()}){Environment.NewLine}", Encoding.Default);
			OutputList.Clear();
			OutputListCnt.Clear();
		}

		/// <summary>ログを出力する</summary>
		static public void CheckOutput(FNAM fname, string tjnname, string mchname, string data) {
			Encoding encode;
			switch (fname) {
			//case FNAM.TOOLSHEET:	// これによりエラーメッセージの日本語が正しく表示できるか確認する
			//	encode = Encoding.Unicode; break;
			default:
				encode = Encoding.Default; break;
			}
			using (StreamWriter sw = new StreamWriter(ServerPC.SvrFldrC + fnam[(int)fname], true, encode)) {
				sw.WriteLine("{0,-14} {1,-13} {2:yyyy/MM/dd HH:mm} {3}", (ProgVersion.Debug ? "_" : "") + tjnname, mchname, DateTime.Now, data);
			}
		}

		/// <summary>ワーニングメッセージを表示する</summary>
		static public void Warning(string mess) {
			if (warn.Length > 0) {
				System.Windows.Forms.MessageBox.Show(warn.ToString(), mess,
					System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
				warn = new StringBuilder();
			}
		}
	}

	/// <summary>
	/// ５軸加工での複数工程ＣＬ接続するＮＣコードを規定します。
	/// </summary>
	/// <remarks>
	/// 接続で作成されたＣＬは干渉チェックがされていない。３軸加工の場合はクリアランス面まで退避すれば
	/// 特に問題はないが、５軸加工の場合は回避動作時の干渉や工具軸を変える場合の動作で干渉しないように
	/// 考慮しなければならない。
	/// </remarks>
	public readonly struct ClLink5Axis
	{
		/// <summary>工具軸初期値設定コード P9697</summary>
		const string codeStart = "P9697";
		/// <summary>工具軸変更設定コード P9698</summary>
		const string codeChgAX = "P9698";
		/// <summary>工程間移動コード P9699</summary>
		const string codeKotei = "P9699";
		/// <summary>すべてのコード P969</summary>
		const string codeAll = "P969";

		// /////////////////////////////////////////////////////////
		// ＰＴＰ内のＮＣデータにはＭ９８で挿入し接続位置を特定する
		// /////////////////////////////////////////////////////////
		/// <summary>元ＮＣデータの工具軸初期値設定コード M98P9697</summary>
		public static string Start { get { return "M98" + codeStart; } }
		/// <summary>元ＮＣデータの工具軸変更設定コード M98P9698</summary>
		public static string ChgAX { get { return "M98" + codeChgAX; } }
		/// <summary>元ＮＣデータの工程間移動コード M98P9699</summary>
		public static string Kotei { get { return "M98" + codeKotei; } }

		// /////////////////////////////////////////////////////////
		// 検証のＮＣデータにはＧ６５で挿入し接続位置を特定する
		// /////////////////////////////////////////////////////////
		/// <summary>検証ＮＣデータの工具軸初期値設定コード G65P9697</summary>
		public static string Start_G { get { return "G65" + codeStart; } }
		/// <summary>検証ＮＣデータの工具軸変更設定コード G65P9698</summary>
		public static string ChgAX_G { get { return "G65" + codeChgAX; } }
		/// <summary>検証ＮＣデータの工程間移動コード G65P9699</summary>
		public static string Kotei_G { get { return "G65" + codeKotei; } }

		/// <summary>工程間接続コードの有無を調べる</summary>
		public static bool Exists(string org) { return org.IndexOf("M98" + codeAll) >= 0; }

		/// <summary>ユーリカの工程間接続コードを元の接続コードに戻す</summary>
		public static string ChangeNormalCode(string org) { return org.Replace("G65" + codeAll, "M98" + codeAll); }

		/// <summary>ユーリカの工程間接続コードの有無を調べる</summary>
		public static bool Exists_G(string org) { return org.IndexOf("G65" + codeAll) >= 0; }

		/// <summary>工程間移動に対応している加工機</summary>
		public static bool KOTEI_IDO(Machine.MachID id) {
			switch (id) {
			case Machine.MachID.DMU200P:
			case Machine.MachID.DMU210P:
			case Machine.MachID.DMU210P2:
			case Machine.MachID.MCC2013:
			case Machine.MachID.FNC208:
			case Machine.MachID.FNC74:
			case Machine.MachID.V77:
			case Machine.MachID.MHG_1500:
			case Machine.MachID.YMC430:
			case Machine.MachID.KENSYO:
				return true;
			case Machine.MachID.D500:
			case Machine.MachID.LineaM:
			case Machine.MachID.MCC3016VG:
			case Machine.MachID.YBM1218V:
			default:
				return false;
			}
		}
	}

	/// <summary>
	/// 出力するデータベースにより制限される文字数を設定します。
	/// </summary>
	public class StringLengthDB
	{
		/// <summary>プロセス名の最大文字数</summary>
		const int maxProcsName = 30;
		/// <summary>編集名（コーディネート名）の最大文字数</summary>
		const int maxCoordName = 30;
		/// <summary>ＣＡＭコメントの最大文字数</summary>
		const int maxCamCommnt = 20;
		/// <summary>ＮＣデータ出力名の最大文字数</summary>
		const int maxNcOutName = 24;

		const int maxStrLen_01 = 1;
		const int maxStrLen_02 = 2;
		const int maxStrLen_08 = 8;
		const int maxStrLen_10 = 10;
		const int maxStrLen_12 = 12;
		const int maxStrLen_15 = 15;
		const int maxStrLen_16 = 16;
		const int maxStrLen_20 = 20;
		const int maxStrLen_30 = 30;

		/// <summary></summary>
		public static void Moji_SeizoNumb(string ss) { if (ss == null || ss.Length > maxStrLen_20) ErrorSet("製造番号の文字数は２０までです。"); }
		/// <summary></summary>
		public static void Moji_TejunName(string ss) { if (ss == null || ss.Length > maxStrLen_16) ErrorSet("手順名の文字数は１６までです。"); }
		/// <summary></summary>
		public static void Moji_ToolSheet(string ss) { if (ss == null || ss.Length > maxStrLen_16) ErrorSet("ツールシート名の文字数は１６までです。"); }
		/// <summary></summary>
		public static void Moji_shainCode(string ss) { if (ss == null || ss.Length > maxStrLen_10) ErrorSet("社員コードの文字数は１０までです。"); }
		/// <summary></summary>
		public static void Moji_MachnName(string ss) { if (ss == null || ss.Length > maxStrLen_10) ErrorSet("加工機の文字数は１０までです。"); }
		/// <summary></summary>
		public static void Moji_DandrName(string ss) { if (ss != null && ss.Length > maxStrLen_20) ErrorSet("段取り名の文字数は２０までです。"); }
		/// <summary></summary>
		public static void Moji_SimSystem(string ss) { if (ss != null && ss.Length > maxStrLen_12) ErrorSet("シミュレーションシステム名の文字数は１２までです。"); }
		/// <summary></summary>
		public static void Moji_SimMachin(string ss) { if (ss != null && ss.Length > maxStrLen_10) ErrorSet("シミュレーション加工機名の文字数は１０までです。"); }
		/// <summary></summary>
		public static void Moji_KoteiCode(string ss) { if (ss != null && ss.Length > maxStrLen_20) ErrorSet("工程コードの文字数は２０までです。"); }
		/// <summary></summary>
		public static void Moji_BuhinName(string ss) { if (ss != null && ss.Length > maxStrLen_20) ErrorSet("加工部品名の文字数は２０までです。"); }

		/// <summary></summary>
		public static void Moji_NcOutName(string ss) { if (ss == null || ss.Length > maxNcOutName) ErrorSet("ＮＣ出力名の文字数は２４までです。"); }
		/// <summary></summary>
		public static void Moji_TejunComm(string ss) { if (ss != null && ss.Length > maxStrLen_20) ErrorSet("手順のコメントの文字数は２０までです。"); }
		/// <summary></summary>
		public static void Moji_ToolgName(string ss) { if (ss == null || ss.Length > maxStrLen_15) ErrorSet("工具名の文字数は１５までです。"); }
		/// <summary></summary>
		public static void Moji_SimlKekka(string ss) { if (ss == null || ss.Length > maxStrLen_02) ErrorSet("シミュレーション結果の文字数は２のみです。"); } // & OK NG
		/// <summary></summary>
		public static void Moji_AxisNumbr(string ss) { if (ss == null || ss.Length != maxStrLen_02) ErrorSet("制御軸数の文字数は２のみです。"); }	// ３軸 ５軸
		/// <summary></summary>
		public static void Moji_AxisTypeS(string ss) { if (ss == null || ss.Length != maxStrLen_02) ErrorSet("制御軸タイプの文字数は２のみです。"); }	// 割出 同時
		/// <summary></summary>
		public static void Moji_KH_Direct(string ss) { if (ss != null && ss.Length > maxStrLen_20) ErrorSet("加工方向の文字数は２０までです。"); }	// 0
		/// <summary></summary>
		public static void Moji_KH_Method(string ss) { if (ss != null && ss.Length > maxStrLen_20) ErrorSet("加工方法の文字数は２０までです。"); }	// 0
		/// <summary></summary>
		public static void Moji_KH_dimns1(string ss) { if (ss == null || ss.Length != maxStrLen_01) ErrorSet("加工軸数１の文字数は１のみです。"); }	// 3
		/// <summary></summary>
		public static void Moji_KH_dimns2(string ss) { if (ss == null || ss.Length != maxStrLen_01) ErrorSet("加工軸数２の文字数は１のみです。"); }	// 3
		/// <summary></summary>
		public static void Moji_MaterialG(string ss) { if (ss == null || ss.Length > maxStrLen_08) ErrorSet("材質グループ名の文字数は８までです。"); }	// 3

		private static void ErrorSet(string ss) {
			LogOut.CheckCount("CamUtil/StringLengthDB 848", false, ss);
			throw new Exception(ss);
		}

		/// <summary>
		/// 出力名のチェックとメッセージの作成
		/// </summary>
		/// <param name="bnf"></param>
		/// <param name="outName">出力名。部品加工のＮＣデータ識別文字は含まない</param>
		/// <param name="toolCount">工具数</param>
		/// <returns></returns>
		public static string Moji_NcOutName(BaseNcForm bnf, string outName, int toolCount) {
			string ss = "";
			int len;

			switch (bnf.Id) {
			case BaseNcForm.ID.BUHIN:
				// 工具ごとの分割時追加文字数：２、工具寿命分割時追加文字数：２
				len = maxNcOutName - (toolCount == 1 ? 0 : 2) - 2;
				if (outName.Length > len)
					ss = "ＮＣデータ出力名は識別文字を除き" + len.ToString() + "文字までです。出力名をクリックして変更してください。";
				break;
			default:
				// 工具ごとの分割時追加文字数：４、工具寿命分割時追加文字数：２
				len = maxNcOutName - (toolCount == 1 ? 0 : 4) - 2;
				if (outName.Length > len)
					ss = "ＮＣデータ出力名は" + len.ToString() + "文字までです。出力名をクリックして変更してください。";
				break;
			}
			return ss;
		}

		// /////////////
		// 以上 static
		// /////////////



		private string strProcsName;	// エラーとなったプロセス名を保存
		private string strCoordName;	// エラーとなった編集名を保存
		private string strCamCommnt;	// エラーとなったＣＡＭコメントを保存

		/// <summary>唯一のコンストラクタ</summary>
		public StringLengthDB() { strProcsName = strCoordName = strCamCommnt = null; }

		/// <summary>最大文字数を超えている場合に末尾を切り取る</summary>
		/// <param name="ss">文字列</param>
		/// <param name="dnam">ＸＭＬのデータ名</param>
		/// <returns></returns>
		public string MaxRemove(string ss, string dnam) {
			switch (dnam) {
			case "ProcsName":
				if (ss.Length <= maxProcsName) return ss;
				if (strProcsName == null) strProcsName = ss;
				return ss.Remove(maxProcsName);
			case "CoordName":
				if (ss.Length <= maxCoordName) return ss;
				if (strCoordName == null) strCoordName = ss;
				return ss.Remove(maxCoordName);
			case "CamCommnt":
				if (ss.Length <= maxCamCommnt) return ss;
				if (strCamCommnt == null) strCamCommnt = ss;
				return ss.Remove(maxCamCommnt);
			default:
				throw new Exception("qjwedbqeh");
			}
		}

		/// <summary>エラーの文字列を出力</summary>
		/// <param name="mess"></param>
		public void ErrorOut(string mess) {
			if (strProcsName != null) LogOut.CheckCount(mess, true, "プロセス名の「" + strProcsName + "」などは３０文字になるように末尾を切り捨てました");
			if (strCoordName != null) LogOut.CheckCount(mess, true, "編集名の「" + strCoordName + "」などは３０文字になるように末尾を切り捨てました");
			if (strCamCommnt != null) LogOut.CheckCount(mess, true, "ＣＡＭコメントの「" + strCamCommnt + "」などは２０文字になるように末尾を切り捨てました");
		}
	}

	/// <summary>
	/// 読み取り専用コレクションに利用するインデクサを持つジェネリックスクラスです。
	/// </summary>
	/// <remarks>
	/// 使い方：private でRO_Collection.InnerArray あるいはRO_Collection.InnerList を作成し、その中のreadonlyフィールド AsReadOnly をプロパティとして公開します。
	/// RO_Collection.InnerArray は配列として要素の参照変更のみ可能で、InnerList はList&lt;T&gt;をそのまま継承しています。
	/// </remarks>
	public class RO_Collection<T> : System.Collections.ObjectModel.ReadOnlyCollection<T>
	{
		/// <summary>読み取り専用配列プロパティの元となる変更可能な配列を定義します</summary>
		public class InnerArray
		{
			/// <summary>読み取り専用のコレクション</summary>
			public readonly RO_Collection<T> AsReadOnly = null;
			/// <summary>配列の長さ</summary>
			public int Length { get { return items == null ? 0 : items.Length; } }
			/// <summary>変更可能な配列</summary>
			public T this[int index] { get { return items[index]; } set { items[index] = value; } }
			private readonly T[] items = null;

			/// <summary>空の１次元配列を作成します</summary>
			/// <param name="length">配列の長さ</param>
			public InnerArray(int length) { items = new T[length]; AsReadOnly = new RO_Collection<T>(items); }
			/// <summary>指示された配列のインスタンスで１次元配列を作成します。</summary>
			/// <param name="array">１次元配列</param>
			public InnerArray(T[] array) { items = array; AsReadOnly = new RO_Collection<T>(items); }
		}
		/// <summary>読み取り専用配列プロパティの元となる変更可能なList&lt;T&gt;を定義します</summary>
		public class InnerList : System.Collections.Generic.List<T>
		{
			/// <summary>読み取り専用のコレクション</summary>
			public new readonly RO_Collection<T> AsReadOnly = null;
			/// <summary>空のList&lt;T&gt;を作成します</summary>
			public InnerList() : base() { AsReadOnly = new RO_Collection<T>(this); }
			/// <summary>指定したコレクションからコピーした要素を格納しList&lt;T&gt;を作成します</summary>
			/// <param name="list">格納するコレクション</param>
			public InnerList(IList<T> list) : base(list) { AsReadOnly = new RO_Collection<T>(this); }
		}

		/// <summary>新しい読み取り専用リストを作成する唯一のコンストラクタです</summary>
		/// <param name="array">定義された配列</param>
		protected RO_Collection(IList<T> array) : base(array) { ; }

		/// <summary>指定された述語によって定義された条件と一致する要素が含まれているかどうかを判断します</summary>
		public bool Exists(Predicate<T> match) { foreach (T item in Items) if (match(item)) return true; return false; }

		/// <summary>指定された述語によって定義された条件と一致する要素を検索し、全体の中で最もインデックス番号の小さい要素を返します</summary>
		public T Find(Predicate<T> match) { foreach (T item in Items) if (match(item)) return item; return default; }

		/// <summary>指定された述語によって定義された条件と一致するすべての要素を取得します</summary>
		public List<T> FindAll(Predicate<T> match) {
			List<T> sel = new List<T>();
			foreach (T item in Items) if (match(item)) sel.Add(item);
			return sel;
		}

		/// <summary>配列化</summary>
		public T[] ToArray() {
			T[] iout = new T[this.Count];
			for (int ii = 0; ii < this.Count; ii++) iout[ii] = this[ii];
			return iout;
		}
	}

	/// <summary>
	/// 読み取り専用の文字インデクサを持つジェネリックスクラスです。
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class RO_ListChar<T>
	{
		/// <summary>読み取り専用配列プロパティの元となる変更可能な配列を定義します</summary>
		public class InnerArray
		{
			/// <summary>読み取り専用のコレクション</summary>
			public readonly RO_ListChar<T> AsReadOnly = null;
			/// <summary>配列の長さ</summary>
			public int Length { get { return items == null ? 0 : items.Length; } }
			/// <summary>変更可能な配列</summary>
			public T this[char cc] { get { return items[AsReadOnly.IndexChar(cc)]; } set { items[AsReadOnly.IndexChar(cc)] = value; } }
			private readonly T[] items = null;

			/// <summary>空の１次元配列を作成します</summary>
			/// <param name="indexList">使用する文字リスト</param>
			public InnerArray(string indexList) { items = new T[indexList.Length]; AsReadOnly = new RO_ListChar<T>(items, indexList); }
			/// <summary>指示された配列のインスタンスで１次元配列を作成します。</summary>
			/// <param name="array">１次元配列</param>
			/// <param name="indexList">使用する文字リスト</param>
			public InnerArray(T[] array, string indexList) { items = array; AsReadOnly = new RO_ListChar<T>(items, indexList); }
		}

		/// <summary>新しい読み取り専用リストを作成する唯一のコンストラクタです</summary>
		/// <param name="array">定義された配列</param>
		/// <param name="indexList">使用する文字リスト</param>
		private RO_ListChar(T[] array, string indexList) { this.items = array; this.indexCharList = indexList; }

		/// <summary>配列</summary>
		private readonly T[] items;
		/// <summary>インデックス</summary>
		private readonly string indexCharList;
		private int IndexChar(char cc) { return indexCharList.IndexOf(cc); }

		/// <summary>配列要素</summary>
		public T this[char cc] { get { return items[IndexChar(cc)]; } }
	}
}
