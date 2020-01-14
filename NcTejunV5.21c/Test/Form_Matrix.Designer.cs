namespace Test
{
	partial class Form_Matrix
	{
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナで生成されたコード

		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent() {
			this.textBox_X = new System.Windows.Forms.TextBox();
			this.textBox_Y = new System.Windows.Forms.TextBox();
			this.textBox_Z = new System.Windows.Forms.TextBox();
			this.button_Exec = new System.Windows.Forms.Button();
			this.label_input = new System.Windows.Forms.Label();
			this.label_output = new System.Windows.Forms.Label();
			this.textBox_AZ = new System.Windows.Forms.TextBox();
			this.textBox_AY = new System.Windows.Forms.TextBox();
			this.textBox_AX = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox_BZ = new System.Windows.Forms.TextBox();
			this.textBox_BY = new System.Windows.Forms.TextBox();
			this.textBox_BX = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox_CZ = new System.Windows.Forms.TextBox();
			this.textBox_CY = new System.Windows.Forms.TextBox();
			this.textBox_CX = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textBox_DZ = new System.Windows.Forms.TextBox();
			this.textBox_DY = new System.Windows.Forms.TextBox();
			this.textBox_DX = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.DMU200P_BC = new System.Windows.Forms.RadioButton();
			this.label4 = new System.Windows.Forms.Label();
			this.Euler_XYZ = new System.Windows.Forms.RadioButton();
			this.Euler_ZXZ = new System.Windows.Forms.RadioButton();
			this.SPATIAL = new System.Windows.Forms.RadioButton();
			this.MCC3016VG_AC = new System.Windows.Forms.RadioButton();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBox_X
			// 
			this.textBox_X.Location = new System.Drawing.Point(14, 24);
			this.textBox_X.Name = "textBox_X";
			this.textBox_X.Size = new System.Drawing.Size(100, 19);
			this.textBox_X.TabIndex = 0;
			// 
			// textBox_Y
			// 
			this.textBox_Y.Location = new System.Drawing.Point(14, 50);
			this.textBox_Y.Name = "textBox_Y";
			this.textBox_Y.Size = new System.Drawing.Size(100, 19);
			this.textBox_Y.TabIndex = 1;
			// 
			// textBox_Z
			// 
			this.textBox_Z.Location = new System.Drawing.Point(14, 76);
			this.textBox_Z.Name = "textBox_Z";
			this.textBox_Z.Size = new System.Drawing.Size(100, 19);
			this.textBox_Z.TabIndex = 2;
			// 
			// button_Exec
			// 
			this.button_Exec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Exec.Location = new System.Drawing.Point(534, 191);
			this.button_Exec.Name = "button_Exec";
			this.button_Exec.Size = new System.Drawing.Size(75, 23);
			this.button_Exec.TabIndex = 3;
			this.button_Exec.Text = "実行";
			this.button_Exec.UseVisualStyleBackColor = true;
			this.button_Exec.Click += new System.EventHandler(this.Button_Exec_Click);
			// 
			// label_input
			// 
			this.label_input.AutoSize = true;
			this.label_input.Location = new System.Drawing.Point(12, 9);
			this.label_input.Name = "label_input";
			this.label_input.Size = new System.Drawing.Size(102, 12);
			this.label_input.TabIndex = 4;
			this.label_input.Text = "回転角ＡＢＣの入力";
			// 
			// label_output
			// 
			this.label_output.AutoSize = true;
			this.label_output.Location = new System.Drawing.Point(411, 128);
			this.label_output.Name = "label_output";
			this.label_output.Size = new System.Drawing.Size(83, 12);
			this.label_output.TabIndex = 8;
			this.label_output.Text = "DMU200P_BC()";
			// 
			// textBox_AZ
			// 
			this.textBox_AZ.Location = new System.Drawing.Point(413, 195);
			this.textBox_AZ.Name = "textBox_AZ";
			this.textBox_AZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_AZ.TabIndex = 7;
			// 
			// textBox_AY
			// 
			this.textBox_AY.Location = new System.Drawing.Point(413, 169);
			this.textBox_AY.Name = "textBox_AY";
			this.textBox_AY.Size = new System.Drawing.Size(100, 19);
			this.textBox_AY.TabIndex = 6;
			// 
			// textBox_AX
			// 
			this.textBox_AX.Location = new System.Drawing.Point(413, 143);
			this.textBox_AX.Name = "textBox_AX";
			this.textBox_AX.Size = new System.Drawing.Size(100, 19);
			this.textBox_AX.TabIndex = 5;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 128);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 12);
			this.label1.TabIndex = 12;
			this.label1.Text = "Euler_XYZ()";
			// 
			// textBox_BZ
			// 
			this.textBox_BZ.Location = new System.Drawing.Point(14, 195);
			this.textBox_BZ.Name = "textBox_BZ";
			this.textBox_BZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_BZ.TabIndex = 11;
			// 
			// textBox_BY
			// 
			this.textBox_BY.Location = new System.Drawing.Point(14, 169);
			this.textBox_BY.Name = "textBox_BY";
			this.textBox_BY.Size = new System.Drawing.Size(100, 19);
			this.textBox_BY.TabIndex = 10;
			// 
			// textBox_BX
			// 
			this.textBox_BX.Location = new System.Drawing.Point(14, 143);
			this.textBox_BX.Name = "textBox_BX";
			this.textBox_BX.Size = new System.Drawing.Size(100, 19);
			this.textBox_BX.TabIndex = 9;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(144, 128);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 12);
			this.label2.TabIndex = 16;
			this.label2.Text = "Euler_ZXZ()";
			// 
			// textBox_CZ
			// 
			this.textBox_CZ.Location = new System.Drawing.Point(146, 195);
			this.textBox_CZ.Name = "textBox_CZ";
			this.textBox_CZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_CZ.TabIndex = 15;
			// 
			// textBox_CY
			// 
			this.textBox_CY.Location = new System.Drawing.Point(146, 169);
			this.textBox_CY.Name = "textBox_CY";
			this.textBox_CY.Size = new System.Drawing.Size(100, 19);
			this.textBox_CY.TabIndex = 14;
			// 
			// textBox_CX
			// 
			this.textBox_CX.Location = new System.Drawing.Point(146, 143);
			this.textBox_CX.Name = "textBox_CX";
			this.textBox_CX.Size = new System.Drawing.Size(100, 19);
			this.textBox_CX.TabIndex = 13;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(275, 128);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(59, 12);
			this.label3.TabIndex = 20;
			this.label3.Text = "SPATIAL()";
			// 
			// textBox_DZ
			// 
			this.textBox_DZ.Location = new System.Drawing.Point(277, 195);
			this.textBox_DZ.Name = "textBox_DZ";
			this.textBox_DZ.Size = new System.Drawing.Size(100, 19);
			this.textBox_DZ.TabIndex = 19;
			// 
			// textBox_DY
			// 
			this.textBox_DY.Location = new System.Drawing.Point(277, 169);
			this.textBox_DY.Name = "textBox_DY";
			this.textBox_DY.Size = new System.Drawing.Size(100, 19);
			this.textBox_DY.TabIndex = 18;
			// 
			// textBox_DX
			// 
			this.textBox_DX.Location = new System.Drawing.Point(277, 143);
			this.textBox_DX.Name = "textBox_DX";
			this.textBox_DX.Size = new System.Drawing.Size(100, 19);
			this.textBox_DX.TabIndex = 17;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.MCC3016VG_AC);
			this.groupBox1.Controls.Add(this.SPATIAL);
			this.groupBox1.Controls.Add(this.Euler_ZXZ);
			this.groupBox1.Controls.Add(this.Euler_XYZ);
			this.groupBox1.Controls.Add(this.DMU200P_BC);
			this.groupBox1.Location = new System.Drawing.Point(146, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(232, 92);
			this.groupBox1.TabIndex = 21;
			this.groupBox1.TabStop = false;
			// 
			// DMU200P_BC
			// 
			this.DMU200P_BC.AutoSize = true;
			this.DMU200P_BC.Location = new System.Drawing.Point(113, 18);
			this.DMU200P_BC.Name = "DMU200P_BC";
			this.DMU200P_BC.Size = new System.Drawing.Size(93, 16);
			this.DMU200P_BC.TabIndex = 3;
			this.DMU200P_BC.Text = "DMU200P_BC";
			this.DMU200P_BC.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(144, 9);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(77, 12);
			this.label4.TabIndex = 22;
			this.label4.Text = "回転方法選択";
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
			// MCC3016VG_AC
			// 
			this.MCC3016VG_AC.AutoSize = true;
			this.MCC3016VG_AC.Location = new System.Drawing.Point(113, 38);
			this.MCC3016VG_AC.Name = "MCC3016VG_AC";
			this.MCC3016VG_AC.Size = new System.Drawing.Size(108, 16);
			this.MCC3016VG_AC.TabIndex = 4;
			this.MCC3016VG_AC.Text = "MCC3016VG_AC";
			this.MCC3016VG_AC.UseVisualStyleBackColor = true;
			// 
			// Form_Matrix
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(626, 226);
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
			this.Text = "Form_Matrix";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBox_X;
		private System.Windows.Forms.TextBox textBox_Y;
		private System.Windows.Forms.TextBox textBox_Z;
		private System.Windows.Forms.Button button_Exec;
		private System.Windows.Forms.Label label_input;
		private System.Windows.Forms.Label label_output;
		private System.Windows.Forms.TextBox textBox_AZ;
		private System.Windows.Forms.TextBox textBox_AY;
		private System.Windows.Forms.TextBox textBox_AX;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_BZ;
		private System.Windows.Forms.TextBox textBox_BY;
		private System.Windows.Forms.TextBox textBox_BX;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox_CZ;
		private System.Windows.Forms.TextBox textBox_CY;
		private System.Windows.Forms.TextBox textBox_CX;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBox_DZ;
		private System.Windows.Forms.TextBox textBox_DY;
		private System.Windows.Forms.TextBox textBox_DX;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton SPATIAL;
		private System.Windows.Forms.RadioButton Euler_ZXZ;
		private System.Windows.Forms.RadioButton Euler_XYZ;
		private System.Windows.Forms.RadioButton DMU200P_BC;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.RadioButton MCC3016VG_AC;
	}
}