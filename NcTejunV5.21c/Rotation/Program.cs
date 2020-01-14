using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RotationAngle
{
	static class Program
	{
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main() {
			// ログ出力イベントの登録
			CamUtil.LogOut.idle_exit = new Action(() => {
				Application.Idle += new EventHandler(Application_IdleExit);
				Application.ApplicationExit += new EventHandler(Application_IdleExit);
			});

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form_Matrix());
		}

		// イベントの実行
		private static void Application_IdleExit(Object sender, EventArgs e) {
			CamUtil.LogOut.CheckCountOutput();
			Application.Idle -= new EventHandler(Application_IdleExit);
			Application.ApplicationExit -= new EventHandler(Application_IdleExit);
		}
	}
}
