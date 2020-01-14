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
	/// スクロールバー付のメッセージを表示する共通のダイアログのクラスです。
	/// </summary>
	public partial class FormMessageBox : Form
	{
		static private FormMessageBox mb;
		static FormMessageBox() { mb = new FormMessageBox(); }

		/// <summary>
		/// スクロールバー付のメッセージを表示
		/// </summary>
		/// <param name="label"></param>
		/// <param name="text"></param>
		static public void Show(string label, string text)
		{
			mb.Text = label;
			mb.textBox1.Text = text;
			//mb.Size = new Size(713, 354);
			//mb.textBox1.Font = new Font("HGｺﾞｼｯｸM", (float)9.75);
			mb.ShowDialog();
		}
		/// <summary>
		/// スクロールバー付のメッセージを表示
		/// </summary>
		/// <param name="label"></param>
		/// <param name="text"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		static public void Show(string label, string text, int width, int height)
		{
			mb.Text = label;
			mb.textBox1.Text = text;
			mb.Size = new Size(width, height);
			//mb.textBox1.Font = new Font("HGｺﾞｼｯｸM", (float)9.75);
			mb.ShowDialog();
		}
		/// <summary>
		/// スクロールバー付のメッセージを表示
		/// </summary>
		/// <param name="label"></param>
		/// <param name="text"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="font">
		/// <para>フォントのファミリー名</para>
		/// <para> HGｺﾞｼｯｸM(default)</para>
		/// <para> Courier New</para>
		/// <para> Courier New Bold</para>
		/// <para> Courier New Italic</para>
		/// <para> Courier New Bold Italic</para>
		/// <para> Times New Roman</para>
		/// <para> Times New Roman Bold</para>
		/// <para> Times New Roman Italic</para>
		/// <para> Times New Roman Bold Italic</para>
		/// <para> Arial</para>
		/// <para> Arial Bold</para>
		/// <para> Arial Italic</para>
		/// <para> Arial Bold Italic</para>
		/// <para> Symbol</para>
		/// </param>
		/// <param name="size">フォントサイズ default:9.75pt</param>
		static public void Show(string label, string text, int width, int height, string font, float size) {
			mb.Text = label;
			mb.textBox1.Text = text;
			mb.Size = new Size(width, height);
			mb.textBox1.Font = new Font(font, size);
			mb.ShowDialog();
		}

		static private void OK_Click(object sender, EventArgs e) { mb.Close(); }

		private FormMessageBox()
		{
			InitializeComponent();
			this.OK.Click += new EventHandler(OK_Click);
		}
	}
}
