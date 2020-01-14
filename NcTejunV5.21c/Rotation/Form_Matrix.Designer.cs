namespace RotationAngle
{
	partial class Form_Matrix
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.D500_AC = new System.Windows.Forms.RadioButton();
			this.MCC3016VG_AC = new System.Windows.Forms.RadioButton();
			this.SPATIAL = new System.Windows.Forms.RadioButton();
			this.Euler_ZXZ = new System.Windows.Forms.RadioButton();
			this.Euler_XYZ = new System.Windows.Forms.RadioButton();
			this.DMU_BC = new System.Windows.Forms.RadioButton();
			this.label3 = new System.Windows.Forms.Label();
			this.textBox_DZ = new System.Windows.Forms.TextBox();
			this.textBox_DY = new System.Windows.Forms.TextBox();
			this.textBox_DX = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox_CZ = new System.Windows.Forms.TextBox();
			this.textBox_CY = new System.Windows.Forms.TextBox();
			this.textBox_CX = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox_BZ = new System.Windows.Forms.TextBox();
			this.textBox_BY = new System.Windows.Forms.TextBox();
			this.textBox_BX = new System.Windows.Forms.TextBox();
			this.label_output = new System.Windows.Forms.Label();
			this.textBox_AZ = new System.Windows.Forms.TextBox();
			this.textBox_AY = new System.Windows.Forms.TextBox();
			this.textBox_AX = new System.Windows.Forms.TextBox();
			this.label_input = new System.Windows.Forms.Label();
			this.button_Exec = new System.Windows.Forms.Button();
			this.textBox_Z = new System.Windows.Forms.TextBox();
			this.textBox_Y = new System.Windows.Forms.TextBox();
			this.textBox_X = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textBox_PZ = new System.Windows.Forms.TextBox();
			this.textBox_PY = new System.Windows.Forms.TextBox();
			this.textBox_PX = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.textBox_QZ = new System.Windows.Forms.TextBox();
			this.textBox_QY = new System.Windows.Forms.TextBox();
			this.textBox_QX = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textBox_RZ = new System.Windows.Forms.TextBox();
			this.textBox_RY = new System.Windows.Forms.TextBox();
			this.textBox_RX = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.textBox_EZ = new System.Windows.Forms.TextBox();
			this.textBox_EY = new System.Windows.Forms.TextBox();
			this.textBox_EX = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.textBox_FZ = new System.Windows.Forms.TextBox();
			this.textBox_FY = new System.Windows.Forms.TextBox();
			this.textBox_FX = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 9);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(77, 12);
			this.label4.TabIndex = 45;
			this.label4.Text = "回転方法選択";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.D500_AC);
			this.groupBox1.Controls.Add(this.MCC3016VG_AC);
			this.groupBox1.Controls.Add(this.SPATIAL);
			this.groupBox1.Controls.Add(this.Euler_ZXZ);
			this.groupBox1.Controls.Add(this.Euler_XYZ);
			this.groupBox1.Controls.Add(this.DMU_BC);
			this.groupBox1.Location = new System.Drawing.Point(14, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(123, 162);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			// 
			// D500_AC
			// 
			this.D500_AC.AutoSize = true;
			this.D500_AC.Location = new System.Drawing.Point(7, 124);
			this.D500_AC.Name = "D500_AC";
			this.D500_AC.Size = new System.Drawing.Size(69, 16);
			this.D500_AC.TabIndex = 5;
			this.D500_AC.Text = "D500_AC";
			this.D500_AC.UseVisualStyleBackColor = true;
			// 
			// MCC3016VG_AC
			// 
			this.MCC3016VG_AC.AutoSize = true;
			this.MCC3016VG_AC.Location = new System.Drawing.Point(7, 102);
			this.MCC3016VG_AC.Name = "MCC3016VG_AC";
			this.MCC3016VG_AC.Size = new System.Drawing.Size(108, 16);
			this.MCC3016VG_AC.TabIndex = 4;
			this.MCC3016VG_AC.Text = "MCC3016VG_AC";
			this.MCC3016VG_AC.UseVisualStyleBackColor = true;
			// 
			// SPATIAL
			// 
			this.SPATIAL.AutoSize = true;
			this.SPATIAL.Location = new System.Drawing.Point(6, 60);
			this.SPATIAL.Name = "SPATIAL";
			this.SPATIAL.Size = new System.Drawing.Size(69, 16);
			this.SPATIAL.TabIndex = 2;
			this.SPATIAL.Text = "SPATIAL";
			this.SPATIAL.UseVisualStyleBackColor = true;
			// 
			// Euler_ZXZ
			// 
			this.Euler_ZXZ.AutoSize = true;
			this.Euler_ZXZ.Location = new System.Drawing.Point(7, 38);
			this.Euler_ZXZ.Name = "Euler_ZXZ";
			this.Euler_ZXZ.Size = new System.Drawing.Size(74, 16);
			this.Euler_ZXZ.TabIndex = 1;
			this.Euler_ZXZ.Text = "Euler_ZXZ";
			this.Euler_ZXZ.UseVisualStyleBackColor = true;
			// 
			// Euler_XYZ
			// 
			this.Euler_XYZ.AutoSize = true;
			this.Euler_XYZ.Checked = true;
			this.Euler_XYZ.Location = new System.Drawing.Point(6, 18);
			this.Euler_XYZ.Name = "Euler_XYZ";
			this.Euler_XYZ.Size = new System.Drawing.Size(74, 16);
			this.Euler_XYZ.TabIndex = 0;
			this.Euler_XYZ.TabStop = true;
			this.Euler_XYZ.Text = "Euler_XYZ";
			this.Euler_XYZ.UseVisualStyleBackColor = true;
			// 
			// DMU_BC
			// 
			this.DMU_BC.AutoSize = true;
			this.DMU_BC.Location = new System.Drawing.Point(7, 82);
			this.DMU_BC.Name = "DMU_BC";
			this.DMU_BC.Size = new System.Drawing.Size(68, 16);
			this.DMU_BC.TabIndex = 3;
			this.DMU_BC.Text = "DMU_BC";
			this.DMU_BC.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(401, 109);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(59, 12);
			this.label3.TabIndex = 43;
			this.label3.Text = "SPATIAL()";
			// 
			// textBox_DZ
			// 
			this.textBox_DZ.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_DZ.Location = new System.Drawing.Point(401, 176);
			this.textBox_DZ.Name = "textBox_DZ";
			this.textBox_DZ.ReadOnly = true;
			this.textBox_DZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_DZ.TabIndex = 42;
			this.textBox_DZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_DY
			// 
			this.textBox_DY.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_DY.Location = new System.Drawing.Point(401, 150);
			this.textBox_DY.Name = "textBox_DY";
			this.textBox_DY.ReadOnly = true;
			this.textBox_DY.Size = new System.Drawing.Size(100, 19);
			this.textBox_DY.TabIndex = 41;
			this.textBox_DY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_DX
			// 
			this.textBox_DX.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_DX.Location = new System.Drawing.Point(401, 124);
			this.textBox_DX.Name = "textBox_DX";
			this.textBox_DX.ReadOnly = true;
			this.textBox_DX.Size = new System.Drawing.Size(100, 19);
			this.textBox_DX.TabIndex = 40;
			this.textBox_DX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(279, 109);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 12);
			this.label2.TabIndex = 39;
			this.label2.Text = "Euler_ZXZ()";
			// 
			// textBox_CZ
			// 
			this.textBox_CZ.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_CZ.Location = new System.Drawing.Point(279, 176);
			this.textBox_CZ.Name = "textBox_CZ";
			this.textBox_CZ.ReadOnly = true;
			this.textBox_CZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_CZ.TabIndex = 38;
			this.textBox_CZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_CY
			// 
			this.textBox_CY.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_CY.Location = new System.Drawing.Point(279, 150);
			this.textBox_CY.Name = "textBox_CY";
			this.textBox_CY.ReadOnly = true;
			this.textBox_CY.Size = new System.Drawing.Size(100, 19);
			this.textBox_CY.TabIndex = 37;
			this.textBox_CY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_CX
			// 
			this.textBox_CX.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_CX.Location = new System.Drawing.Point(279, 124);
			this.textBox_CX.Name = "textBox_CX";
			this.textBox_CX.ReadOnly = true;
			this.textBox_CX.Size = new System.Drawing.Size(100, 19);
			this.textBox_CX.TabIndex = 36;
			this.textBox_CX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(157, 109);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 12);
			this.label1.TabIndex = 35;
			this.label1.Text = "Euler_XYZ()";
			// 
			// textBox_BZ
			// 
			this.textBox_BZ.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_BZ.Location = new System.Drawing.Point(156, 176);
			this.textBox_BZ.Name = "textBox_BZ";
			this.textBox_BZ.ReadOnly = true;
			this.textBox_BZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_BZ.TabIndex = 34;
			this.textBox_BZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_BY
			// 
			this.textBox_BY.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_BY.Location = new System.Drawing.Point(156, 150);
			this.textBox_BY.Name = "textBox_BY";
			this.textBox_BY.ReadOnly = true;
			this.textBox_BY.Size = new System.Drawing.Size(100, 19);
			this.textBox_BY.TabIndex = 33;
			this.textBox_BY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_BX
			// 
			this.textBox_BX.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_BX.Location = new System.Drawing.Point(157, 125);
			this.textBox_BX.Name = "textBox_BX";
			this.textBox_BX.ReadOnly = true;
			this.textBox_BX.Size = new System.Drawing.Size(100, 19);
			this.textBox_BX.TabIndex = 32;
			this.textBox_BX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label_output
			// 
			this.label_output.AutoSize = true;
			this.label_output.Location = new System.Drawing.Point(523, 109);
			this.label_output.Name = "label_output";
			this.label_output.Size = new System.Drawing.Size(58, 12);
			this.label_output.TabIndex = 31;
			this.label_output.Text = "DMU_BC()";
			// 
			// textBox_AZ
			// 
			this.textBox_AZ.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_AZ.Location = new System.Drawing.Point(523, 176);
			this.textBox_AZ.Name = "textBox_AZ";
			this.textBox_AZ.ReadOnly = true;
			this.textBox_AZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_AZ.TabIndex = 30;
			this.textBox_AZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_AY
			// 
			this.textBox_AY.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_AY.Location = new System.Drawing.Point(523, 150);
			this.textBox_AY.Name = "textBox_AY";
			this.textBox_AY.ReadOnly = true;
			this.textBox_AY.Size = new System.Drawing.Size(100, 19);
			this.textBox_AY.TabIndex = 29;
			this.textBox_AY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_AX
			// 
			this.textBox_AX.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_AX.Location = new System.Drawing.Point(523, 124);
			this.textBox_AX.Name = "textBox_AX";
			this.textBox_AX.ReadOnly = true;
			this.textBox_AX.Size = new System.Drawing.Size(100, 19);
			this.textBox_AX.TabIndex = 28;
			this.textBox_AX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label_input
			// 
			this.label_input.AutoSize = true;
			this.label_input.Location = new System.Drawing.Point(154, 9);
			this.label_input.Name = "label_input";
			this.label_input.Size = new System.Drawing.Size(102, 12);
			this.label_input.TabIndex = 27;
			this.label_input.Text = "回転角ＡＢＣの入力";
			// 
			// button_Exec
			// 
			this.button_Exec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Exec.Location = new System.Drawing.Point(791, 211);
			this.button_Exec.Name = "button_Exec";
			this.button_Exec.Size = new System.Drawing.Size(75, 23);
			this.button_Exec.TabIndex = 8;
			this.button_Exec.Text = "実行";
			this.button_Exec.UseVisualStyleBackColor = true;
			this.button_Exec.Click += new System.EventHandler(this.Button_Exec_Click);
			// 
			// textBox_Z
			// 
			this.textBox_Z.Location = new System.Drawing.Point(156, 76);
			this.textBox_Z.Name = "textBox_Z";
			this.textBox_Z.Size = new System.Drawing.Size(100, 19);
			this.textBox_Z.TabIndex = 3;
			this.textBox_Z.Text = "0.0";
			this.textBox_Z.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_Y
			// 
			this.textBox_Y.Location = new System.Drawing.Point(156, 50);
			this.textBox_Y.Name = "textBox_Y";
			this.textBox_Y.Size = new System.Drawing.Size(100, 19);
			this.textBox_Y.TabIndex = 2;
			this.textBox_Y.Text = "0.0";
			this.textBox_Y.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_X
			// 
			this.textBox_X.Location = new System.Drawing.Point(156, 24);
			this.textBox_X.Name = "textBox_X";
			this.textBox_X.Size = new System.Drawing.Size(100, 19);
			this.textBox_X.TabIndex = 1;
			this.textBox_X.Text = "0.0";
			this.textBox_X.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(334, 9);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(77, 12);
			this.label5.TabIndex = 49;
			this.label5.Text = "基準座標ＸＹＺ";
			// 
			// textBox_PZ
			// 
			this.textBox_PZ.Location = new System.Drawing.Point(336, 76);
			this.textBox_PZ.Name = "textBox_PZ";
			this.textBox_PZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_PZ.TabIndex = 7;
			this.textBox_PZ.Text = "0.0";
			this.textBox_PZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_PY
			// 
			this.textBox_PY.Location = new System.Drawing.Point(336, 50);
			this.textBox_PY.Name = "textBox_PY";
			this.textBox_PY.Size = new System.Drawing.Size(100, 19);
			this.textBox_PY.TabIndex = 6;
			this.textBox_PY.Text = "0.0";
			this.textBox_PY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_PX
			// 
			this.textBox_PX.Location = new System.Drawing.Point(336, 24);
			this.textBox_PX.Name = "textBox_PX";
			this.textBox_PX.Size = new System.Drawing.Size(100, 19);
			this.textBox_PX.TabIndex = 5;
			this.textBox_PX.Text = "0.0";
			this.textBox_PX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(456, 9);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(98, 12);
			this.label6.TabIndex = 53;
			this.label6.Text = "フィーチャー⇒モデル";
			// 
			// textBox_QZ
			// 
			this.textBox_QZ.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_QZ.Location = new System.Drawing.Point(458, 76);
			this.textBox_QZ.Name = "textBox_QZ";
			this.textBox_QZ.ReadOnly = true;
			this.textBox_QZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_QZ.TabIndex = 52;
			this.textBox_QZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_QY
			// 
			this.textBox_QY.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_QY.Location = new System.Drawing.Point(458, 50);
			this.textBox_QY.Name = "textBox_QY";
			this.textBox_QY.ReadOnly = true;
			this.textBox_QY.Size = new System.Drawing.Size(100, 19);
			this.textBox_QY.TabIndex = 51;
			this.textBox_QY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_QX
			// 
			this.textBox_QX.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_QX.Location = new System.Drawing.Point(458, 24);
			this.textBox_QX.Name = "textBox_QX";
			this.textBox_QX.ReadOnly = true;
			this.textBox_QX.Size = new System.Drawing.Size(100, 19);
			this.textBox_QX.TabIndex = 50;
			this.textBox_QX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(581, 9);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(98, 12);
			this.label7.TabIndex = 57;
			this.label7.Text = "モデル⇒フィーチャー";
			// 
			// textBox_RZ
			// 
			this.textBox_RZ.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_RZ.Location = new System.Drawing.Point(583, 76);
			this.textBox_RZ.Name = "textBox_RZ";
			this.textBox_RZ.ReadOnly = true;
			this.textBox_RZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_RZ.TabIndex = 56;
			this.textBox_RZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_RY
			// 
			this.textBox_RY.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_RY.Location = new System.Drawing.Point(583, 50);
			this.textBox_RY.Name = "textBox_RY";
			this.textBox_RY.ReadOnly = true;
			this.textBox_RY.Size = new System.Drawing.Size(100, 19);
			this.textBox_RY.TabIndex = 55;
			this.textBox_RY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_RX
			// 
			this.textBox_RX.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_RX.Location = new System.Drawing.Point(583, 24);
			this.textBox_RX.Name = "textBox_RX";
			this.textBox_RX.ReadOnly = true;
			this.textBox_RX.Size = new System.Drawing.Size(100, 19);
			this.textBox_RX.TabIndex = 54;
			this.textBox_RX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(767, 109);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(59, 12);
			this.label8.TabIndex = 61;
			this.label8.Text = "D500_AC()";
			// 
			// textBox_EZ
			// 
			this.textBox_EZ.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_EZ.Location = new System.Drawing.Point(767, 176);
			this.textBox_EZ.Name = "textBox_EZ";
			this.textBox_EZ.ReadOnly = true;
			this.textBox_EZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_EZ.TabIndex = 60;
			this.textBox_EZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_EY
			// 
			this.textBox_EY.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_EY.Location = new System.Drawing.Point(767, 150);
			this.textBox_EY.Name = "textBox_EY";
			this.textBox_EY.ReadOnly = true;
			this.textBox_EY.Size = new System.Drawing.Size(100, 19);
			this.textBox_EY.TabIndex = 59;
			this.textBox_EY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_EX
			// 
			this.textBox_EX.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_EX.Location = new System.Drawing.Point(767, 124);
			this.textBox_EX.Name = "textBox_EX";
			this.textBox_EX.ReadOnly = true;
			this.textBox_EX.Size = new System.Drawing.Size(100, 19);
			this.textBox_EX.TabIndex = 58;
			this.textBox_EX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(645, 109);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(98, 12);
			this.label9.TabIndex = 65;
			this.label9.Text = "MCC3016VG_AC()";
			// 
			// textBox_FZ
			// 
			this.textBox_FZ.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_FZ.Location = new System.Drawing.Point(645, 176);
			this.textBox_FZ.Name = "textBox_FZ";
			this.textBox_FZ.ReadOnly = true;
			this.textBox_FZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_FZ.TabIndex = 64;
			this.textBox_FZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_FY
			// 
			this.textBox_FY.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_FY.Location = new System.Drawing.Point(645, 150);
			this.textBox_FY.Name = "textBox_FY";
			this.textBox_FY.ReadOnly = true;
			this.textBox_FY.Size = new System.Drawing.Size(100, 19);
			this.textBox_FY.TabIndex = 63;
			this.textBox_FY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_FX
			// 
			this.textBox_FX.BackColor = System.Drawing.SystemColors.Menu;
			this.textBox_FX.Location = new System.Drawing.Point(645, 124);
			this.textBox_FX.Name = "textBox_FX";
			this.textBox_FX.ReadOnly = true;
			this.textBox_FX.Size = new System.Drawing.Size(100, 19);
			this.textBox_FX.TabIndex = 62;
			this.textBox_FX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Form_Matrix
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(878, 246);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.textBox_FZ);
			this.Controls.Add(this.textBox_FY);
			this.Controls.Add(this.textBox_FX);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textBox_EZ);
			this.Controls.Add(this.textBox_EY);
			this.Controls.Add(this.textBox_EX);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.textBox_RZ);
			this.Controls.Add(this.textBox_RY);
			this.Controls.Add(this.textBox_RX);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.textBox_QZ);
			this.Controls.Add(this.textBox_QY);
			this.Controls.Add(this.textBox_QX);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.textBox_PZ);
			this.Controls.Add(this.textBox_PY);
			this.Controls.Add(this.textBox_PX);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textBox_DZ);
			this.Controls.Add(this.textBox_DY);
			this.Controls.Add(this.textBox_DX);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBox_CZ);
			this.Controls.Add(this.textBox_CY);
			this.Controls.Add(this.textBox_CX);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBox_BZ);
			this.Controls.Add(this.textBox_BY);
			this.Controls.Add(this.textBox_BX);
			this.Controls.Add(this.label_output);
			this.Controls.Add(this.textBox_AZ);
			this.Controls.Add(this.textBox_AY);
			this.Controls.Add(this.textBox_AX);
			this.Controls.Add(this.label_input);
			this.Controls.Add(this.button_Exec);
			this.Controls.Add(this.textBox_Z);
			this.Controls.Add(this.textBox_Y);
			this.Controls.Add(this.textBox_X);
			this.Name = "Form_Matrix";
			this.Text = "Form1";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton MCC3016VG_AC;
		private System.Windows.Forms.RadioButton SPATIAL;
		private System.Windows.Forms.RadioButton Euler_ZXZ;
		private System.Windows.Forms.RadioButton Euler_XYZ;
		private System.Windows.Forms.RadioButton DMU_BC;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBox_DZ;
		private System.Windows.Forms.TextBox textBox_DY;
		private System.Windows.Forms.TextBox textBox_DX;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox_CZ;
		private System.Windows.Forms.TextBox textBox_CY;
		private System.Windows.Forms.TextBox textBox_CX;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_BZ;
		private System.Windows.Forms.TextBox textBox_BY;
		private System.Windows.Forms.TextBox textBox_BX;
		private System.Windows.Forms.Label label_output;
		private System.Windows.Forms.TextBox textBox_AZ;
		private System.Windows.Forms.TextBox textBox_AY;
		private System.Windows.Forms.TextBox textBox_AX;
		private System.Windows.Forms.Label label_input;
		private System.Windows.Forms.Button button_Exec;
		private System.Windows.Forms.TextBox textBox_Z;
		private System.Windows.Forms.TextBox textBox_Y;
		private System.Windows.Forms.TextBox textBox_X;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textBox_PZ;
		private System.Windows.Forms.TextBox textBox_PY;
		private System.Windows.Forms.TextBox textBox_PX;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textBox_QZ;
		private System.Windows.Forms.TextBox textBox_QY;
		private System.Windows.Forms.TextBox textBox_QX;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox textBox_RZ;
		private System.Windows.Forms.TextBox textBox_RY;
		private System.Windows.Forms.TextBox textBox_RX;
		private System.Windows.Forms.RadioButton D500_AC;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textBox_EZ;
		private System.Windows.Forms.TextBox textBox_EY;
		private System.Windows.Forms.TextBox textBox_EX;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox textBox_FZ;
		private System.Windows.Forms.TextBox textBox_FY;
		private System.Windows.Forms.TextBox textBox_FX;
	}
}