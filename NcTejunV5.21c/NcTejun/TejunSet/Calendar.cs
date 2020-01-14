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
	/// 加工予定日を入力する
	/// </summary>
	partial class Calendar : Form
	{
		private StringBuilder sdate;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ss"></param>
		public Calendar(StringBuilder ss) {
			InitializeComponent();
			sdate = ss;
		}
		private void MonthCalendar1_DateSelected(object sender, DateRangeEventArgs e) {
			sdate.Append(((System.Windows.Forms.MonthCalendar)sender).SelectionStart.ToString("yyyy/MM/dd"));
			this.Close();
		}
	}
}
