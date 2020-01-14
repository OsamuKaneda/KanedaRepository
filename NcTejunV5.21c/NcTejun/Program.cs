using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.IO;
using CamUtil;

namespace NcTejun
{
	/// <summary>
	/// エントリ・ポイント・クラス
	/// </summary>
	static class Program
	{
		static public string mess = "\n " + ProgVersion.VerName1 + "\n";

		// /////////////
		// 工程間接続
		// /////////////
		static public MiKensho _2d_kot_s00 = new MiKensho("２次元加工でのG0ポスト工程間接続（３軸）は未検証です。藤本まで連絡願います。");
		// 2013/07/11 static public MiKensho _2d_kot_s11 = new MiKensho("２次元加工でのG0ポスト工程間接続（傾斜）は未検証です。藤本まで連絡願います。");
		static public MiKensho _2d_kot_t00 = new MiKensho("２次元加工でのG0ポスト工具軸変更を伴う工程間接続（３軸含む）は未検証です。藤本まで連絡願います。");
		static public MiKensho _2d_kot_t11 = new MiKensho("２次元加工でのG0ポスト工具軸変更を伴う工程間接続（傾斜のみ）は一部未検証です。藤本まで連絡願います。");

		static public MiKensho _3d_kot_s00 = new MiKensho("３次元加工でのG0ポスト工程間接続（３軸）は未検証です。藤本まで連絡願います。");
		// 2013/06/03 static public MiKensho _3d_kot_s11 = new MiKensho("３次元加工でのG0ポスト工程間接続（傾斜）は未検証です。藤本まで連絡願います。");
		static public MiKensho _3d_kot_t00 = new MiKensho("３次元加工でのG0ポスト工具軸変更を伴う工程間接続（３軸含む）は未検証です。藤本まで連絡願います。");
		// 2014/06/01 static public MiKensho _3d_kot_t11 = new MiKensho("３次元加工でのG0ポスト工具軸変更を伴う工程間接続（傾斜のみ）は未検証です。藤本まで連絡願います。");
		/// <summary></summary>
		public readonly struct MiKensho
		{
			static public bool _2d_angle0;
			static public bool _2d_keisha;
			static public bool _3d_angle0;
			static public bool _3d_keisha;
			static public bool _sk_angle0;
			static public bool _sk_keisha;

			/// <summary>メッセージ</summary>
			public readonly string mess;

			public MiKensho(string mess) { this.mess = mess; }
		}


		/// <summary>メインフォーム</summary>
		static public Form1 frm1;

		///// <summary>加工手順書</summary>
		//static public NcTejun.Tejun.Tejun tejun = null;

		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// プログラムのバージョンのチェック
			if (CamUtil.ProgVersion.CheckVersion("NcTejun.exe")) return;
			// ログ出力イベントの登録
			LogOut.idle_exit = new Action(() => {
				Application.Idle += new EventHandler(Application_IdleExit);
				Application.ApplicationExit += new EventHandler(Application_IdleExit);
			});

			// プログラムの実行
			try { Application.Run((frm1 = new Form1())); }
			catch (Exception ex) {
				MessageBox.Show(ex.Message, "NcTejun", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
		}

		// イベントの実行
		private static void Application_IdleExit(Object sender, EventArgs e) {
			LogOut.CheckCountOutput();
			Application.Idle -= new EventHandler(Application_IdleExit);
			Application.ApplicationExit -= new EventHandler(Application_IdleExit);
		}
	}
}