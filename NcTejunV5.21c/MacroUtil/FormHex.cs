using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MacroUtil
{
	/// <summary>
	/// 文字コードを１６進数で表示する
	/// </summary>
	public partial class FormHex : Form
	{
		/// <summary>
		/// 
		/// </summary>
		public FormHex() {
			InitializeComponent();
			// 文字幅一定（0とOを区分可能、欧文）
			//this.richTextBox1.Font = new System.Drawing.Font("ＭＳ ゴシック", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			//this.richTextBox1.Font = new System.Drawing.Font("DejaVu Sans Mono", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			//this.richTextBox1.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.richTextBox1.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		}

		/// <summary>
		/// 最大２５６文字のコードを設定する
		/// </summary>
		/// <param name="length"></param>
		/// <param name="chr256"></param>
		public void SetText(int length, char[] chr256) {
			byte[] byte16 = new byte[16];
			int ccnt;
			int start;

			richTextBox1.Text = "";
			if (length <= 0) return;
			if (length > 256) throw new Exception("aerfbwerfhbh");

			start = 0;
			while (true) {
				ccnt = Math.Min(16, length - start);
				for (int ii = 0; ii < ccnt; ii++)
					byte16[ii] = Convert.ToByte(chr256[start + ii]);
				richTextBox1.Text += BitConverter.ToString(byte16).Replace('-', ' ').PadRight(3 * 17);
				for (int ii = 0; ii < ccnt; ii++)
					richTextBox1.Text += CharToString(chr256[start + ii]) + " ";
				start += 16;
				if (start >= length) break;
				richTextBox1.Text += "\r\n";
			}
		}
		private string CharToString(char cc) {
			if (Char.IsLetterOrDigit(cc) || Char.IsSymbol(cc) || Char.IsPunctuation(cc)) {
				switch (cc) {
				case (char)0xAD: return "AD";
				default:
					return " " + cc;
				}
			}
			else if (Char.IsSeparator(cc))
				return "SP";
			else if (Char.IsControl(cc)) {
				switch (cc) {
				case (char)0: return @"\0";	// NUL	Null
				case (char)7: return @"\a";	// BEL	ビープ音
				case (char)8: return @"\b";	// BS	後退（バックスペース）
				case (char)9: return @"\t";	// HT	水平タブ
				case (char)10: return @"\n";	// LF	改行（ラインフィード）
				case (char)11: return @"\v";	// VT	垂直タブ
				case (char)12: return @"\f";	// FF	書式送り（フォーム フィード）
				case (char)13: return @"\r";	// CR	行頭復帰（キャリッジリターン）
				case (char)27: return @"\e";	// ESC	エスケープ
				case (char)127: return @"DL";	// DEL	抹消（デリート）
				default:
					return "CC";
				}
			}
			else
				return "XX";
		}

		private void ButtonOk_Click(object sender, EventArgs e) {
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void ButtonCancel_Click(object sender, EventArgs e) {
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}