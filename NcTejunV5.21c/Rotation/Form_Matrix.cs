using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using CamUtil;

namespace RotationAngle
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

			Angle3 ax;
			Angle3.JIKU jiku = Angle3.JIKU.Null;
			RotationAxis aa;

			// ///////////////////
			// ベクトルの回転
			// ///////////////////
			Vector3 vv;
			vv = new Vector3(Convert.ToDouble(textBox_X.Text), Convert.ToDouble(textBox_Y.Text), Convert.ToDouble(textBox_Z.Text))
				.Rotation(17, Angle3.DtoR(23.0));
			vv = new Vector3(Convert.ToDouble(textBox_X.Text), Convert.ToDouble(textBox_Y.Text), Convert.ToDouble(textBox_Z.Text))
				.Rotation(18, Angle3.DtoR(23.0));
			vv = new Vector3(Convert.ToDouble(textBox_X.Text), Convert.ToDouble(textBox_Y.Text), Convert.ToDouble(textBox_Z.Text))
				.Rotation(19, Angle3.DtoR(-33.0));

			if (Euler_XYZ.Checked) jiku = Angle3.JIKU.Euler_XYZ;
			if (Euler_ZXZ.Checked) jiku = Angle3.JIKU.Euler_ZXZ;
			if (SPATIAL.Checked) jiku = Angle3.JIKU.Spatial;
			if (DMU_BC.Checked) jiku = Angle3.JIKU.DMU_BC;
			if (MCC3016VG_AC.Checked) jiku = Angle3.JIKU.MCCVG_AC;
			if (D500_AC.Checked) jiku = Angle3.JIKU.D500_AC;
			if (jiku == Angle3.JIKU.Null) throw new Exception("kefqeb");

			vv = new Vector3(Convert.ToDouble(textBox_X.Text), Convert.ToDouble(textBox_Y.Text), Convert.ToDouble(textBox_Z.Text));
			aa = new RotationAxis(new Angle3(jiku, vv * Math.PI / 180.0));

			ax = aa.Euler_XYZ();
			textBox_BX.Text = ax.DegA.ToString("f5");
			textBox_BY.Text = ax.DegB.ToString("f5");
			textBox_BZ.Text = ax.DegC.ToString("f5");

			ax = aa.Euler_ZXZ();
			textBox_CX.Text = ax.DegA.ToString("f5");
			textBox_CY.Text = ax.DegB.ToString("f5");
			textBox_CZ.Text = ax.DegC.ToString("f5");

			ax = aa.SPATIAL();
			textBox_DX.Text = ax.DegA.ToString("f5");
			textBox_DY.Text = ax.DegB.ToString("f5");
			textBox_DZ.Text = ax.DegC.ToString("f5");

			try { ax = aa.DMU_BC(); }
			catch { ax = new Angle3(Angle3.JIKU.DMU_BC, new Vector3(Double.NaN, Double.NaN, Double.NaN)); }
			textBox_AX.Text = ax.DegA.ToString("f5");
			textBox_AY.Text = ax.DegB.ToString("f5");
			textBox_AZ.Text = ax.DegC.ToString("f5");

			try { ax = aa.MCCVG_AC(); }
			catch { ax = new Angle3(Angle3.JIKU.MCCVG_AC, new Vector3(Double.NaN, Double.NaN, Double.NaN)); }
			textBox_FX.Text = ax.DegA.ToString("f5");
			textBox_FY.Text = ax.DegB.ToString("f5");
			textBox_FZ.Text = ax.DegC.ToString("f5");

			try { ax = aa.D500_AC(); }
			catch { ax = new Angle3(Angle3.JIKU.D500_AC, new Vector3(Double.NaN, Double.NaN, Double.NaN)); }
			textBox_EX.Text = ax.DegA.ToString("f5");
			textBox_EY.Text = ax.DegB.ToString("f5");
			textBox_EZ.Text = ax.DegC.ToString("f5");

			Vector3 qq;
			qq = aa.Transform(RotationAxis.TRANSFORM.FeatureToWork,
				new Vector3(Convert.ToDouble(textBox_PX.Text), Convert.ToDouble(textBox_PY.Text), Convert.ToDouble(textBox_PZ.Text)));
			textBox_QX.Text = qq.X.ToString("f5");
			textBox_QY.Text = qq.Y.ToString("f5");
			textBox_QZ.Text = qq.Z.ToString("f5");
			qq = aa.Transform(RotationAxis.TRANSFORM.WorkToFeature,
				new Vector3(Convert.ToDouble(textBox_PX.Text), Convert.ToDouble(textBox_PY.Text), Convert.ToDouble(textBox_PZ.Text)));
			textBox_RX.Text = qq.X.ToString("f5");
			textBox_RY.Text = qq.Y.ToString("f5");
			textBox_RZ.Text = qq.Z.ToString("f5");

			// 四元数の計算チェック
			try { Matrix mm = aa.General(); }
			catch { ;}

			Application.DoEvents();

		}
	}
}
