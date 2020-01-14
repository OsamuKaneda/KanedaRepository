using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Test
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// 
		/// </summary>
		public Form1() {
			InitializeComponent();

		}

		private void Button_Refer_Click(object sender, EventArgs e) {
			Form_Reference frm = new Form_Reference();
			frm.Show();
		}

		private void Button_Matrix_Click(object sender, EventArgs e) {
			Form_Matrix frm = new Form_Matrix();
			frm.Show();
		}
	}
}