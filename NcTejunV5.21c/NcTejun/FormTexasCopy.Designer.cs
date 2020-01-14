namespace NcTejun
{
	partial class FormTexasCopy
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
			if (casa != null) { casa.Dispose(); casa = null; }
		}

		#region Windows フォーム デザイナで生成されたコード

		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent() {
			this.buttonCancel = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.textBox_katana = new System.Windows.Forms.TextBox();
			this.comboBox_buhin = new System.Windows.Forms.ComboBox();
			this.comboBox_kotei = new System.Windows.Forms.ComboBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.comboBox_seba = new System.Windows.Forms.ComboBox();
			this.comboBox_mach = new System.Windows.Forms.ComboBox();
			this.textBox_seizo = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.buttonOK = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(399, 399);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 9;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(5, 53);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "内製管理No";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(248, 52);
			this.label3.Name = "label3";
			this.label3.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label3.Size = new System.Drawing.Size(62, 20);
			this.label3.TabIndex = 0;
			this.label3.Text = "ワーク名";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(248, 79);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(62, 20);
			this.label4.TabIndex = 0;
			this.label4.Text = "工程名";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBox_katana
			// 
			this.textBox_katana.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_katana.BackColor = System.Drawing.SystemColors.Control;
			this.textBox_katana.Location = new System.Drawing.Point(25, 107);
			this.textBox_katana.Name = "textBox_katana";
			this.textBox_katana.Size = new System.Drawing.Size(187, 19);
			this.textBox_katana.TabIndex = 0;
			this.textBox_katana.TabStop = false;
			// 
			// comboBox_buhin
			// 
			this.comboBox_buhin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox_buhin.FormattingEnabled = true;
			this.comboBox_buhin.Location = new System.Drawing.Point(340, 53);
			this.comboBox_buhin.Name = "comboBox_buhin";
			this.comboBox_buhin.Size = new System.Drawing.Size(121, 20);
			this.comboBox_buhin.TabIndex = 1;
			this.comboBox_buhin.SelectedIndexChanged += new System.EventHandler(this.ComboBox_buhin_SelectedIndexChanged);
			// 
			// comboBox_kotei
			// 
			this.comboBox_kotei.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox_kotei.FormattingEnabled = true;
			this.comboBox_kotei.Location = new System.Drawing.Point(340, 80);
			this.comboBox_kotei.Name = "comboBox_kotei";
			this.comboBox_kotei.Size = new System.Drawing.Size(121, 20);
			this.comboBox_kotei.TabIndex = 2;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.comboBox_seba);
			this.groupBox1.Controls.Add(this.comboBox_mach);
			this.groupBox1.Controls.Add(this.textBox_seizo);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.textBox_katana);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.comboBox_kotei);
			this.groupBox1.Controls.Add(this.comboBox_buhin);
			this.groupBox1.Location = new System.Drawing.Point(7, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(467, 167);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "カサブランカ情報";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(5, 26);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 20);
			this.label2.TabIndex = 13;
			this.label2.Text = "加工機名";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBox_seba
			// 
			this.comboBox_seba.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox_seba.FormattingEnabled = true;
			this.comboBox_seba.Location = new System.Drawing.Point(91, 52);
			this.comboBox_seba.Name = "comboBox_seba";
			this.comboBox_seba.Size = new System.Drawing.Size(121, 20);
			this.comboBox_seba.TabIndex = 3;
			this.comboBox_seba.SelectedIndexChanged += new System.EventHandler(this.ComboBox_seba_SelectedIndexChanged);
			// 
			// comboBox_mach
			// 
			this.comboBox_mach.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox_mach.FormattingEnabled = true;
			this.comboBox_mach.Location = new System.Drawing.Point(91, 26);
			this.comboBox_mach.Name = "comboBox_mach";
			this.comboBox_mach.Size = new System.Drawing.Size(121, 20);
			this.comboBox_mach.TabIndex = 4;
			this.comboBox_mach.SelectedIndexChanged += new System.EventHandler(this.ComboBox_mach_SelectedIndexChanged);
			// 
			// textBox_seizo
			// 
			this.textBox_seizo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_seizo.BackColor = System.Drawing.SystemColors.Control;
			this.textBox_seizo.Location = new System.Drawing.Point(91, 81);
			this.textBox_seizo.Name = "textBox_seizo";
			this.textBox_seizo.Size = new System.Drawing.Size(121, 19);
			this.textBox_seizo.TabIndex = 0;
			this.textBox_seizo.TabStop = false;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(5, 80);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(80, 20);
			this.label10.TabIndex = 0;
			this.label10.Text = "製造番号";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(306, 399);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 13;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// FormTexasCopy
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(482, 428);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonCancel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(370, 262);
			this.Name = "FormTexasCopy";
			this.Text = "ＴＥＸＡＳ出力";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBox_katana;
		private System.Windows.Forms.GroupBox groupBox1;
		public System.Windows.Forms.ComboBox comboBox_buhin;
		public System.Windows.Forms.ComboBox comboBox_kotei;
		public System.Windows.Forms.TextBox textBox_seizo;
		private System.Windows.Forms.Label label10;
		public System.Windows.Forms.ComboBox comboBox_seba;
		public System.Windows.Forms.ComboBox comboBox_mach;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonOK;
	}
}