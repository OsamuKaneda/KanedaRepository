using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using CamUtil;
using System.IO;
using System.Data.SqlClient;

namespace NcdTool.NcSIM
{
	/// <summary>
	/// 
	/// </summary>
	partial class FormNcSpeed : Form
	{
		//private DataTable tjnGrpData;
		StringBuilder trGroup;

		internal FormNcSpeed(StringBuilder xx)
		{
			InitializeComponent();

			this.trGroup = xx;

			using (DataTable tjnGrpData = new DataTable("toolset_grp")) {
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.toolset_grp", connection)) {
					connection.Open();
					adapter.Fill(tjnGrpData);
				}


				// 
				// radioButtonの作成
				// 
				System.Windows.Forms.RadioButton rbtmp;
				int yy = 0;
				foreach (DataRow dRow in tjnGrpData.Rows) {
					rbtmp = new RadioButton() {
						AutoSize = true,
						Location = new System.Drawing.Point(6, 10 + yy * 20),
						Name = "radioButton",
						Size = new System.Drawing.Size(88, 16),
						TabIndex = 0,
						TabStop = true,
						Text = (string)dRow[0] + " " + (string)dRow[1],
						UseVisualStyleBackColor = true
					};
					this.groupBox1.Controls.Add(rbtmp);
					yy++;
				}
			}
		}

		private void ButtonCANCEL_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void ButtonOK_Click(object sender, EventArgs e) {

			// ツーリンググループを設定
			foreach (Control ssss in this.groupBox1.Controls)
				if (((RadioButton)ssss).Checked) {
					trGroup.Append(((RadioButton)ssss).Text);
					break;
				}
			this.Close();
		}
	}
}