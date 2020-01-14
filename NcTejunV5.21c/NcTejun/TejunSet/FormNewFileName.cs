using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NcTejun.TejunSet
{
	/// <summary>
	/// 
	/// </summary>
	partial class FormNewFileName : Form
	{
		StringBuilder sb;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tName"></param>
		public FormNewFileName(StringBuilder tName) {
			InitializeComponent();
			sb = tName;
			textBox1.Text = sb.ToString();
			sb.Remove(0, sb.Length);
		}

		private void Button_OK_Click(object sender, EventArgs e) {
			sb.Append(textBox1.Text);
			this.Close();
		}

		private void Button_CANCEL_Click(object sender, EventArgs e) {
			this.Close();
		}
	}
}