using System;
using System.Collections.Generic;
using System.Windows.Forms;

using CamUtil;

namespace HoudenEn_V77Convert
{
	static class Program
	{
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// プログラムのバージョンのチェック
			if (CamUtil.ProgVersion.CheckVersion("HoudenEn_V77Convert.exe")) return;
			// ログ出力イベントの登録
			LogOut.idle_exit = new Action(() => {
				Application.Idle += new EventHandler(Application_IdleExit);
				Application.ApplicationExit += new EventHandler(Application_IdleExit);
			});

			// プログラムの実行
			try { Application.Run(new Form1()); }
			catch (Exception ex) {
				MessageBox.Show(ex.Message, "HoudenEn_V77Convert", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
