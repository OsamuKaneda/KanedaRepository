using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NcTejun
{
	/// <summary>
	/// 
	/// </summary>
	partial class FormHelp : Form
	{
		/// <summary>
		/// 
		/// </summary>
		public FormHelp() {
			InitializeComponent();
		}

		private void Form2_Load(object sender, EventArgs e) {

			this.reportViewer1.RefreshReport();
		}
	}
}