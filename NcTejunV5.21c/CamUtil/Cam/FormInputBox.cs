using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CamUtil
{
	/// <summary>
	/// 文字を入力するフォームを作成する共通のダイアログのクラスです。
	/// </summary>
	public partial class FormInputBox : Form
	{
		/// <summary></summary>
		/// <param name="message">メッセージ</param>
		/// <param name="title">タイトル</param>
		/// <param name="textdefault">テキストの初期値</param>
		/// <param name="width">横幅</param>
		/// <param name="height">高さ</param>
		public FormInputBox(string message, string title, string textdefault, int? width, int? height) {
			InitializeComponent();

			this.label1.Text = message;
			this.Text = title;
			this.textBox1.Text = textdefault;
			if (width.HasValue && height.HasValue) this.Size = new Size(width.Value, height.Value);
		}

		/// <summary></summary>
		/// <returns></returns>
		public new string ShowDialog() {
			if (base.ShowDialog() == DialogResult.Cancel)
				return null;
			return this.textBox1.Text;
		}
	}
}
