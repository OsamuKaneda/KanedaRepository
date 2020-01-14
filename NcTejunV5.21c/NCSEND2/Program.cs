// 01 2006/05/20 スプラッシュを表示
// 02 2006/05/20 データ送信ダイアログを表示
// 03 2006/05/21 出力名変更に対応
// 04 2006/05/21 ＮＣデータ変換送信の中止に対応
// 05 2006/05/21 カスタムマクロのＰには小数点をつけない
// 06 2006/05/22 送信時・出力名変更時にサーバを見に行く(分割時も)
// 07 2006/05/23 テンプファイルのxxxをなくす
// 08 2006/05/23 セミコロンはコメントの前に
// 09 2006/05/23 ８４００マクロ使用可能に
// 10 2006/05/23 エンドミルのシフト量（Ｉ）を入れる
// 11 2006/05/24 工具シートを作成する
//               K (手順名)
//               T (工具番号) (工具名－ピリオド以降も要) (突出し量－小数点以下２桁) (ホルダー名)
//
//
// 00 0000/00/00 加工原点が固定になっている
// 00 0000/00/00 ポット番号フリーに対応する
// 00 0000/00/00 使用寿命割合がほしい
//
// 00 0000/00/00 残し量をX、Zに分ける
// 00 0000/00/00 ＮＣデータ変換中・送信中のキャンセルを可能に
// 00 0000/00/00 ウィンドウの状態を次回のために保存する
// 00 0000/00/00 固定サイクル情報をストラクチャーにする
// 00 0000/00/00 Application.Exit()の終了がスムーズでない

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

using CamUtil;

namespace NCSEND2
{
	static class Program
	{
		static public string mess = "\n " + ProgVersion.VerName1 + "\n";

		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		/// 
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// プログラムのバージョンのチェック
			Application.DoEvents();
			if (CamUtil.ProgVersion.CheckVersion("NCSEND2.exe")) return;
			// ログ出力イベントの登録
			LogOut.idle_exit = new Action(() => {
				Application.Idle += new EventHandler(Application_IdleExit);
				Application.ApplicationExit += new EventHandler(Application_IdleExit);
			});

			// プログラムの実行
			try {
				Form1 frmForm1 = new Form1();
				Application.Run(frmForm1);
			}
			catch (Exception ex) {
				System.Windows.Forms.MessageBox.Show(ex.Message,
					"Program", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
