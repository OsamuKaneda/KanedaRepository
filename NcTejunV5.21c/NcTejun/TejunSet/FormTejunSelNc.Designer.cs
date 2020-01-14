namespace NcTejun.TejunSet
{
	partial class FormTejunSelNc
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
			this.radio_Add = new System.Windows.Forms.RadioButton();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button_Serch = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.button_Insert = new System.Windows.Forms.Button();
			this.panel_Ichi = new System.Windows.Forms.Panel();
			this.radio_Ins = new System.Windows.Forms.RadioButton();
			this.button_Insert2 = new System.Windows.Forms.Button();
			this.button_Cancel = new System.Windows.Forms.Button();
			this.comboBox_CAM = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.radio_time = new System.Windows.Forms.RadioButton();
			this.radio_abcd = new System.Windows.Forms.RadioButton();
			this.panel_Ichi.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// radio_Add
			// 
			this.radio_Add.AutoSize = true;
			this.radio_Add.Checked = true;
			this.radio_Add.Location = new System.Drawing.Point(6, 3);
			this.radio_Add.Name = "radio_Add";
			this.radio_Add.Size = new System.Drawing.Size(80, 16);
			this.radio_Add.TabIndex = 6;
			this.radio_Add.TabStop = true;
			this.radio_Add.Text = "最後に追加";
			this.radio_Add.UseVisualStyleBackColor = true;
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(6, 64);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(153, 19);
			this.textBox1.TabIndex = 1;
			// 
			// button_Serch
			// 
			this.button_Serch.Location = new System.Drawing.Point(93, 90);
			this.button_Serch.Name = "button_Serch";
			this.button_Serch.Size = new System.Drawing.Size(66, 23);
			this.button_Serch.TabIndex = 2;
			this.button_Serch.Text = "検索実行";
			this.button_Serch.UseVisualStyleBackColor = true;
			this.button_Serch.Click += new System.EventHandler(this.Button_Serch_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(5, 49);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(154, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "先頭の文字列 or 検索パターン";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// listBox1
			// 
			this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 12;
			this.listBox1.Location = new System.Drawing.Point(190, 8);
			this.listBox1.Name = "listBox1";
			this.listBox1.ScrollAlwaysVisible = true;
			this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listBox1.Size = new System.Drawing.Size(121, 292);
			this.listBox1.TabIndex = 3;
			// 
			// button_Insert
			// 
			this.button_Insert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Insert.Location = new System.Drawing.Point(236, 312);
			this.button_Insert.Name = "button_Insert";
			this.button_Insert.Size = new System.Drawing.Size(75, 23);
			this.button_Insert.TabIndex = 4;
			this.button_Insert.Text = "入力";
			this.button_Insert.UseVisualStyleBackColor = true;
			this.button_Insert.Click += new System.EventHandler(this.Button_Insert_Click);
			// 
			// panel_Ichi
			// 
			this.panel_Ichi.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel_Ichi.Controls.Add(this.radio_Ins);
			this.panel_Ichi.Controls.Add(this.radio_Add);
			this.panel_Ichi.Location = new System.Drawing.Point(12, 172);
			this.panel_Ichi.Name = "panel_Ichi";
			this.panel_Ichi.Size = new System.Drawing.Size(172, 47);
			this.panel_Ichi.TabIndex = 5;
			// 
			// radio_Ins
			// 
			this.radio_Ins.AutoSize = true;
			this.radio_Ins.Location = new System.Drawing.Point(6, 23);
			this.radio_Ins.Name = "radio_Ins";
			this.radio_Ins.Size = new System.Drawing.Size(116, 16);
			this.radio_Ins.TabIndex = 7;
			this.radio_Ins.TabStop = true;
			this.radio_Ins.Text = "カーソルの前に挿入";
			this.radio_Ins.UseVisualStyleBackColor = true;
			// 
			// button_Insert2
			// 
			this.button_Insert2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Insert2.Location = new System.Drawing.Point(153, 312);
			this.button_Insert2.Name = "button_Insert2";
			this.button_Insert2.Size = new System.Drawing.Size(75, 23);
			this.button_Insert2.TabIndex = 6;
			this.button_Insert2.Text = "全て入力";
			this.button_Insert2.UseVisualStyleBackColor = true;
			this.button_Insert2.Click += new System.EventHandler(this.Button_Insert2_Click);
			// 
			// button_Cancel
			// 
			this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Cancel.Location = new System.Drawing.Point(67, 312);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(75, 23);
			this.button_Cancel.TabIndex = 7;
			this.button_Cancel.Text = "キャンセル";
			this.button_Cancel.UseVisualStyleBackColor = true;
			this.button_Cancel.Click += new System.EventHandler(this.Button_Cancel_Click);
			// 
			// comboBox_CAM
			// 
			this.comboBox_CAM.FormattingEnabled = true;
			this.comboBox_CAM.Location = new System.Drawing.Point(6, 24);
			this.comboBox_CAM.Name = "comboBox_CAM";
			this.comboBox_CAM.Size = new System.Drawing.Size(153, 20);
			this.comboBox_CAM.TabIndex = 8;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(4, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 12);
			this.label2.TabIndex = 9;
			this.label2.Text = "出力ＣＡＭシステム限定";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 157);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(121, 12);
			this.label3.TabIndex = 10;
			this.label3.Text = "加工手順への入力位置";
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.textBox1);
			this.panel1.Controls.Add(this.button_Serch);
			this.panel1.Controls.Add(this.comboBox_CAM);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Location = new System.Drawing.Point(14, 23);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(170, 120);
			this.panel1.TabIndex = 11;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(14, 8);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(29, 12);
			this.label4.TabIndex = 12;
			this.label4.Text = "検索";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(10, 237);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(121, 12);
			this.label5.TabIndex = 14;
			this.label5.Text = "加工手順への挿入順序";
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel2.Controls.Add(this.radio_time);
			this.panel2.Controls.Add(this.radio_abcd);
			this.panel2.Location = new System.Drawing.Point(10, 252);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(172, 47);
			this.panel2.TabIndex = 13;
			// 
			// radio_time
			// 
			this.radio_time.AutoSize = true;
			this.radio_time.Location = new System.Drawing.Point(6, 23);
			this.radio_time.Name = "radio_time";
			this.radio_time.Size = new System.Drawing.Size(83, 16);
			this.radio_time.TabIndex = 7;
			this.radio_time.TabStop = true;
			this.radio_time.Text = "出力時間順";
			this.radio_time.UseVisualStyleBackColor = true;
			// 
			// radio_abcd
			// 
			this.radio_abcd.AutoSize = true;
			this.radio_abcd.Checked = true;
			this.radio_abcd.Location = new System.Drawing.Point(6, 3);
			this.radio_abcd.Name = "radio_abcd";
			this.radio_abcd.Size = new System.Drawing.Size(59, 16);
			this.radio_abcd.TabIndex = 6;
			this.radio_abcd.TabStop = true;
			this.radio_abcd.Text = "ABC順";
			this.radio_abcd.UseVisualStyleBackColor = true;
			// 
			// FormTejunSelNc
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(318, 342);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.button_Cancel);
			this.Controls.Add(this.button_Insert2);
			this.Controls.Add(this.panel_Ichi);
			this.Controls.Add(this.button_Insert);
			this.Controls.Add(this.listBox1);
			this.Name = "FormTejunSelNc";
			this.Text = "FormTejunSelNc";
			this.panel_Ichi.ResumeLayout(false);
			this.panel_Ichi.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button_Serch;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button button_Insert;
		private System.Windows.Forms.Panel panel_Ichi;
		private System.Windows.Forms.RadioButton radio_Ins;
		private System.Windows.Forms.Button button_Insert2;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.RadioButton radio_Add;
		private System.Windows.Forms.ComboBox comboBox_CAM;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RadioButton radio_time;
		private System.Windows.Forms.RadioButton radio_abcd;
	}
}