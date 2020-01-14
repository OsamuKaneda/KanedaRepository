using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MacroUtil
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

			Form1 frm1 = new Form1();
			Application.Run(frm1);
		}
	}
}