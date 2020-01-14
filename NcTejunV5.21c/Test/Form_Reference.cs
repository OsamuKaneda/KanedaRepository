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
	public partial class Form_Reference : Form
	{
		/// <summary>
		/// 
		/// </summary>
		struct St_test
		{
			public int ii;
			public double[] dd;
			public St_test(int dummy) {
				ii = 1;
				dd = new double[2];
				dd[0] = 1.2;
				dd[1] = 1.5;
			}
		}

		/// <summary>
		/// ��r�e�X�g�����s����t�H�[���ł�
		/// </summary>
		public Form_Reference() {
			InitializeComponent();
			this.Text = "TEST";

			// �z����̗v�f�̏�����
			// RO_Array<T> : System.Collections.ObjectModel.ReadOnlyCollection ���g���Ƃn�j�BCopyFrom(), ToArray() �̑���� CopyTo() ������B
			// RO_List<T>  : System.Collections.ObjectModel.ReadOnlyCollection ���g���Ƃn�j�����AExists(), Find() ���\�b�h���Ȃ��B
			// RO_Array<T> �͏��������Ă��悢�������b�g�͂Ȃ������BRO_List<T> �͌p���g�p����Bin 2016/12/15
			if (0 == 0) {
				string[] array0 = { "abc", "def", "ghi" };
				string[] array1 = new string[3];
				array0.CopyTo(array1, 0);
				System.Collections.ObjectModel.ReadOnlyCollection<string> array2 = new System.Collections.ObjectModel.ReadOnlyCollection<string>(array0);

				array1[1] = "jkl";				// ���s����邪�{�̂ɉe���͂Ȃ�
				//array2[1] = "jkl";			// �G���[�ƂȂ�

				array0[0] = "mno";
				MessageBox.Show($"{array0[1]} {array1[0]} {array2[0]}");

				List<string> list0 = new List<string>() { "abc", "def", "ghi" };
				//CamUtil.RO_List<string> list1 = new CamUtil.RO_List<string>(list0);
				IList<string> list2 = list0;
				IList<string> list3 = list0.AsReadOnly();
				System.Collections.ObjectModel.ReadOnlyCollection<string> list4 = new System.Collections.ObjectModel.ReadOnlyCollection<string>(list0);

				//list2[1] = "jkl";				// �G���[�ƂȂ�
				list2[1] = "jkl";				// ���s�����
				try { list3[1] = "jkl"; }		// ���s���ɃG���[�ƂȂ�
				catch { ;}
				//list5[1] = "jkl";				// �G���[�ƂȂ�

				list0[0] = "mno";
				MessageBox.Show($"{list2[0]} {list3[0]} {list4[0]}");
			}

			// Matrix
			if (0 == 0) {
				double[,] aaa = new double[3, 3];
				for (int ii = 0; ii < 3; ii++)
					for (int jj = 0; jj < 3; jj++)
						aaa[ii, jj] = ii * 3 + jj;
				CamUtil.Matrix ma, mb;
				ma = new CamUtil.Matrix(aaa);
				mb = ma;
				aaa[0, 0] = 345;

				// string
				string s1, s2;
				s1 = "ABCDE";
				s2 = s1;
				textBox01.Text = $"  s1 = {s1}   s2 = {s2}";
				s1 += "FGH";
				s2 += "IJK";
				textBox02.Text = $"  s1 = {s1}   s2 = {s2}";
			}

			// struct
			if (0 == 0) {
				St_test aa, bb;
				aa = new St_test(0);
				//bb = new st_test(0);
				bb = aa;
				textBox03.Text = $"  aa = {aa.ii} {aa.dd[0]} {aa.dd[1]}  bb = {bb.ii} {bb.dd[0]} {bb.dd[1]}";
				bb.ii = 2;
				bb.dd[0] = 2.2;
				bb.dd[1] = 2.5;
				textBox04.Text = $"  aa = {aa.ii} {aa.dd[0]} {aa.dd[1]}  bb = {bb.ii} {bb.dd[0]} {bb.dd[1]}";
				aa.ii = 3;
				aa.dd[0] = 3.2;
				aa.dd[1] = 3.5;
				textBox05.Text = $"  aa = {aa.ii} {aa.dd[0]} {aa.dd[1]}  bb = {bb.ii} {bb.dd[0]} {bb.dd[1]}";
			}

			Select();
		}

		private void TextBox5_TextChanged(object sender, EventArgs e) {

		}
	}
}