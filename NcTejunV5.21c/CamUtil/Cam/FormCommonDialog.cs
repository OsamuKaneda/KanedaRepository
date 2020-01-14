using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace CamUtil
{
	/// <summary>
	/// 進行状況を表示する共通のダイアログのクラスです。
	/// </summary>
	public partial class FormCommonDialog : Form
	{
		/// <summary>呼出すメソッド</summary>
		private Action<Label> method;
		/// <summary>エラーメッセージ</summary>
		private StringBuilder errMess;

		/// <summary>
		/// 進行状況を表示する共通のダイアログを作成する（tejun, NCSPEED, NcOutput）
		/// </summary>
		/// <param name="caption">表題に表示する文字列</param>
		/// <param name="method">実行するメソッド</param>
		/// <param name="p_errMessage">エラーの場合、返すメッセージを書き込む場所</param>
		public FormCommonDialog(string caption, Action<Label> method, StringBuilder p_errMessage)
		{
			InitializeComponent();

			this.method = method;
			this.errMess = p_errMessage;

			this.Text = caption;
			this.label_message.Text = caption;
		}

		private void FormFormTejunSet_Shown(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			try { method(this.label_message); }
			catch (Exception ex) {
				errMess.Append(ex.Message);
				this.DialogResult = DialogResult.Cancel;
			}
		}

		private void CANCEL_Click(object sender, EventArgs e) {
			LogOut.CheckCount("FormCommonDialog 60", false,
				$"キャンセルボタン {this.Text} sender={sender.GetType().Name} {sender.ToString()} e={e.GetType().Name} {e.ToString()}");
			throw new Exception("キャンセルボタンが押されました。実行を中止します。");
		}
	}
}