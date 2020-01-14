using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using CamUtil;

namespace Test
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Form_Matrix : Form
	{
		/// <summary>
		/// 
		/// </summary>
		public Form_Matrix() {
			InitializeComponent();
		}

		private void Button_Exec_Click(object sender, EventArgs e) {

			// /////////////////
			// 行列式の計算
			// /////////////////
			double[,] mat = new double[,] {
				{5,4,3,2,1},
				{0,0,0,1,2},
				{1,1,1,1,0},
				{1,-2,-3,-4,-2},
				{1,0,1,1,1}
			};
			//double dtmp = new Matrix(mat).Determinant_old();
			double dtmp = new Matrix(mat).Determinant();
			// 答えは１４

			// 逆行列
			Matrix qq = new Matrix(mat) * new Matrix(mat).Inverse();

		}
	}
}