using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using CamUtil;
using NcCode.nccode;

namespace NcCode
{
	/// <summary>
	/// NcTejunで直接実行される以外のＮＣデータ解析プログラムの基本クラスです。
	/// </summary>
	/// <remarks>
	/// ・ncoutm()：ＮＣデータ１行ごとにコールされるメソッドです。
	/// ・ncoute()：ＮＣデータの最後にコールされるメソッドです。
	/// ・Close() ：エラーで終了した場合にコールされるメソッドです。
	/// </remarks>
	abstract public class _main
	{
		/// <summary>現在の移動量積算値</summary>
		internal static NcDst sdst = new NcDst(0);

		/// <summary>GENERAL加工機名称</summary>
		public const string mcGENERAL = "GENERAL";
		/// <summary>エラー処理方法</summary>
		static public _main Error = null;

		/// <summary>ＮＣデータなどのチェック出力</summary>
		static protected string m_swNcCheckName = null;

		// /////////////////////////////////
		// 以上が静的変数
		// /////////////////////////////////



		/// <summary>ＮＣデータなどのチェック出力</summary>
		internal System.IO.StreamWriter SwNcCheck { get { return m_swNcCheck; } }
		/// <summary>ＮＣデータなどのチェック出力</summary>
		protected System.IO.StreamWriter m_swNcCheck = null;

		/// <summary>最新のNcMod</summary>
		private NcMod tmod;
		/// <summary>ＮＣデータ名</summary>
		protected string ncName;
		//public Machine machine = null;

		/// <summary>ＮＣデータの行数（進行状況の表示に使用 ADD in 2014/10/22）</summary>
		protected int? maxLineNo;

		/// <summary>ＮＣデータの入力と実行するNcRederクラス</summary>
		private NcRW ncReader = null;

		/// <summary>
		/// ＮＣプログラム検索フォルダーリスト
		/// （最後に'/'がある場合はその後に加工機名が追加される）
		/// </summary>
		public abstract string[] Ncdir { get; protected set; }
		//private abstract string[] m_ncdir;

		/// <summary>
		/// サブプログラム名リスト
		/// </summary>
		public List<string> sfil = new List<string>();



		/// <summary>
		/// 出力ＮＣデータ各行での処理を記述する
		/// </summary>
		/// <param name="nout"></param>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		public abstract void Ncoutm(NcOuts nout, NcMod mod, OCode lcode);
		/// <summary>
		/// 出力ＮＣデータの最終での処理を記述する
		/// </summary>
		public abstract void Ncoute();
		/// <summary>
		/// 終了処理
		/// </summary>
		public abstract void Close();
		/// <summary>
		/// ＮＣデータ解析時におけるエラーの処理を実行
		/// </summary>
		/// <param name="errn">エラー番号</param>
		/// <param name="errc">表示するコメント</param>
		public abstract void Ncerr(int errn, string errc);



		/// <summary>
		/// 共通コンストラクタ
		/// </summary>
		/// <param name="program_name"></param>
		/// <param name="machine_name">参照するマクロのフォルダー名の加工機部分の名称（例：\usr9\ASDM\CAMCTL\NCMEM\[machine_name]）</param>
		/// <param name="machineID">加工機情報の初期設定に使用する加工機ID</param>
		public _main(string program_name, string machine_name, CamUtil.Machine.MachID machineID) {

			if (Error == null)
				Error = this;

			// 静的変数の設定
			sdst = new NcDst(0);
			// Postの初期化
			Post.Init(Machine.BNcForm(machineID));

			this.ncName = program_name;

			// ＮＣデータのフォルダ名の作成
			for (int ii = 0; ii < Ncdir.Length; ii++) {
				if (Ncdir[ii][Ncdir[ii].Length - 1] == '/') throw new Exception("ncdir ERROR in _main");
				if (Ncdir[ii][Ncdir[ii].Length - 1] == '\\')
					Ncdir[ii] += machine_name;
			}

			// 加工機情報の初期化
			NcMachine.ParaInit(machineID);

			// 初期化
			tmod = new NcMod();

			// Ｇコード初期値を設定
			tmod.Params2401(NcMachine.ParaData(2401, 0));
		}

		/// <summary>
		/// 入出力の設定。
		/// FILE : ReadLine_File()により全行の処理を完了する
		/// PC_FILE : ReadLine_File()により全行の処理を完了する
		/// CALL : NcWR_CallのWriteLineにより書き込み、最後に_main.ncoute()を実行する
		/// </summary>
		/// <param name="file_Call"></param>
		/// <param name="trs_mir">NCSPEED専用の移動/ミラー情報</param>
		/// <param name="xyz">初期のＸＹＺ位置</param>
		/// <param name="mac_name">展開するマクロ名</param>
		protected void MainNcdSet(string file_Call, Transp_Mirror trs_mir, CamUtil.Vector3 xyz, string mac_name) {

			switch (file_Call) {
			case "PC_FILE":
				try {
					NcMod.Fmsub newfmsub = new NcMod.Fmsub(0, 1, ncName);
					//string[] tmpDir = CamUtil.CamNcD.NcDataN.SearchPTP(ncName, ".ncd");
					NcFileReader nfr = new NcFileReader(ServerPC.FulNcName(ncName + ".ncd"));
					maxLineNo = nfr.MaxLineNo;
					ncReader = new NcRW(this, tmod, newfmsub, null, nfr, trs_mir, new Ncdep(0));
				}
				catch (Exception ex) { Ncerr(3, ex.Message); }
				break;
			case "CALL":
				// 加工終了後に開始点高さまで戻るマクロのＺ高さをチェックする
				switch (mac_name) {
				case "P8010":
				case "P8013":
				case "P8015":
				case "P8200":
				case "P8290":
				case "P8401":
				case "P8402":
					if (xyz.Z <= 0.0)
						LogOut.CheckCount("NcCode._main 337", true, $"{this.ncName}の{mac_name}マクロ展開の開始点Ｚが0.0以下になっています（{xyz}）。正しいか確認願います。");
					break;
				}
				try { ncReader = new NcRW(this, tmod, xyz); }
				catch (Exception ex) { Ncerr(3, ex.Message); }
				break;
			default:
				throw new Exception("プログラムエラーkqefnqjrwe");
			}

			// ///////////////////////////////////////
			// 初期化後に NcRWのstatic データをクリアする
			// （もっとスマートな方法はないのか？）
			// ///////////////////////////////////////
			NcRW.sakiy0 = -1;

		}

		/// <summary>
		/// メインＮＣデータの全行の読み込み（ファイルから）
		/// </summary>
		public void ReadLine_File() {
			try {
				ncReader.ReadAll();
				System.Windows.Forms.Application.DoEvents();
				ncReader.NcClose();
				if (SwNcCheck != null)
					SwNcCheck.WriteLine("サブプロのクローズ in ReadLine_File：" + ncName);
			}
			catch (Exception ex) {
				ncReader.NcClose();
				this.Close();
				throw new Exception(ncName + " " + ex.Message);
			}
			// ***********
			// 最終出力 **
			// ***********
			Ncoute();
		}
		/// <summary>
		/// メインＮＣデータ１行を処理させます
		/// </summary>
		/// <param name="ddat"></param>
		public void WriteLine(string ddat) {
			ncReader.WriteLine(ddat);
		}
		/// <summary>メインＮＣデータの処理をリセットします</summary>
		protected void WriteEnd() { ncReader = null; }
	}
}
