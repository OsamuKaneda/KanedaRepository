namespace NcTejun.Output
{
	partial class FormNcSet_Buhin
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
			this.comboBox_pallet = new System.Windows.Forms.ComboBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonNc = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.textBox_naisei = new System.Windows.Forms.TextBox();
			this.textBox_katana = new System.Windows.Forms.TextBox();
			this.comboBox_buhin = new System.Windows.Forms.ComboBox();
			this.comboBox_kotei = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textBoxComment = new System.Windows.Forms.TextBox();
			this.comboBox_Dandori = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.combo_dnc = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.textBoxOutName = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textBox_seizo = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.comboBox_bKubun = new System.Windows.Forms.ComboBox();
			this.label14 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonTool = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label12 = new System.Windows.Forms.Label();
			this.textBox_Z = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textBox_H = new System.Windows.Forms.TextBox();
			this.textBox_Y = new System.Windows.Forms.TextBox();
			this.textBox_X = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// comboBox_pallet
			// 
			this.comboBox_pallet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox_pallet.FormattingEnabled = true;
			this.comboBox_pallet.Items.AddRange(new object[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20"});
			this.comboBox_pallet.Location = new System.Drawing.Point(78, 100);
			this.comboBox_pallet.MaxDropDownItems = 20;
			this.comboBox_pallet.Name = "comboBox_pallet";
			this.comboBox_pallet.Size = new System.Drawing.Size(121, 20);
			this.comboBox_pallet.TabIndex = 6;
			this.comboBox_pallet.Text = "(未設定)";
			this.comboBox_pallet.SelectedIndexChanged += new System.EventHandler(this.ComboBox_pallet_SelectedIndexChanged);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(576, 257);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 9;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonNc
			// 
			this.buttonNc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonNc.Location = new System.Drawing.Point(243, 257);
			this.buttonNc.Name = "buttonNc";
			this.buttonNc.Size = new System.Drawing.Size(91, 23);
			this.buttonNc.TabIndex = 10;
			this.buttonNc.Text = "ＮＣデータ出力";
			this.buttonNc.UseVisualStyleBackColor = true;
			this.buttonNc.Visible = false;
			this.buttonNc.Click += new System.EventHandler(this.ButtonNc_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(5, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "内製管理No";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(5, 104);
			this.label3.Name = "label3";
			this.label3.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.label3.Size = new System.Drawing.Size(62, 20);
			this.label3.TabIndex = 0;
			this.label3.Text = "ワーク名";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(5, 131);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(62, 20);
			this.label4.TabIndex = 0;
			this.label4.Text = "工程名";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBox_naisei
			// 
			this.textBox_naisei.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_naisei.BackColor = System.Drawing.SystemColors.Control;
			this.textBox_naisei.Location = new System.Drawing.Point(73, 22);
			this.textBox_naisei.Name = "textBox_naisei";
			this.textBox_naisei.Size = new System.Drawing.Size(121, 19);
			this.textBox_naisei.TabIndex = 0;
			this.textBox_naisei.TabStop = false;
			// 
			// textBox_katana
			// 
			this.textBox_katana.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_katana.BackColor = System.Drawing.SystemColors.Control;
			this.textBox_katana.Location = new System.Drawing.Point(7, 68);
			this.textBox_katana.Name = "textBox_katana";
			this.textBox_katana.Size = new System.Drawing.Size(187, 19);
			this.textBox_katana.TabIndex = 0;
			this.textBox_katana.TabStop = false;
			// 
			// comboBox_buhin
			// 
			this.comboBox_buhin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox_buhin.FormattingEnabled = true;
			this.comboBox_buhin.Location = new System.Drawing.Point(73, 104);
			this.comboBox_buhin.Name = "comboBox_buhin";
			this.comboBox_buhin.Size = new System.Drawing.Size(121, 20);
			this.comboBox_buhin.TabIndex = 1;
			this.comboBox_buhin.SelectedIndexChanged += new System.EventHandler(this.ComboBox_buhin_SelectedIndexChanged);
			// 
			// comboBox_kotei
			// 
			this.comboBox_kotei.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox_kotei.FormattingEnabled = true;
			this.comboBox_kotei.Location = new System.Drawing.Point(73, 131);
			this.comboBox_kotei.Name = "comboBox_kotei";
			this.comboBox_kotei.Size = new System.Drawing.Size(121, 20);
			this.comboBox_kotei.TabIndex = 2;
			this.comboBox_kotei.SelectedIndexChanged += new System.EventHandler(this.ComboBox_kotei_SelectedIndexChanged);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(5, 158);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 20);
			this.label5.TabIndex = 0;
			this.label5.Text = "コメント";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxComment
			// 
			this.textBoxComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxComment.Location = new System.Drawing.Point(73, 158);
			this.textBoxComment.Name = "textBoxComment";
			this.textBoxComment.Size = new System.Drawing.Size(121, 19);
			this.textBoxComment.TabIndex = 0;
			this.textBoxComment.TabStop = false;
			// 
			// comboBox_Dandori
			// 
			this.comboBox_Dandori.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox_Dandori.FormattingEnabled = true;
			this.comboBox_Dandori.Location = new System.Drawing.Point(78, 74);
			this.comboBox_Dandori.Name = "comboBox_Dandori";
			this.comboBox_Dandori.Size = new System.Drawing.Size(121, 20);
			this.comboBox_Dandori.TabIndex = 4;
			this.comboBox_Dandori.Text = "(未設定)";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(8, 74);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(79, 20);
			this.label9.TabIndex = 0;
			this.label9.Text = "段取りタイプ";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// combo_dnc
			// 
			this.combo_dnc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.combo_dnc.FormattingEnabled = true;
			this.combo_dnc.Items.AddRange(new object[] {
            "ＤＮＣ運転",
            "メモリ運転"});
			this.combo_dnc.Location = new System.Drawing.Point(78, 22);
			this.combo_dnc.Name = "combo_dnc";
			this.combo_dnc.Size = new System.Drawing.Size(100, 20);
			this.combo_dnc.TabIndex = 0;
			this.combo_dnc.TabStop = false;
			this.combo_dnc.Text = "メモリ運転";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(7, 22);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(57, 20);
			this.label6.TabIndex = 0;
			this.label6.Text = "運転方法";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxOutName
			// 
			this.textBoxOutName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxOutName.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxOutName.Location = new System.Drawing.Point(78, 48);
			this.textBoxOutName.Name = "textBoxOutName";
			this.textBoxOutName.Size = new System.Drawing.Size(121, 19);
			this.textBoxOutName.TabIndex = 0;
			this.textBoxOutName.TabStop = false;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(7, 48);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(58, 20);
			this.label8.TabIndex = 0;
			this.label8.Text = "出力名";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.textBox_seizo);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.textBoxComment);
			this.groupBox1.Controls.Add(this.textBox_naisei);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.textBox_katana);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.comboBox_kotei);
			this.groupBox1.Controls.Add(this.comboBox_buhin);
			this.groupBox1.Location = new System.Drawing.Point(451, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(200, 192);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "カサブランカ情報";
			// 
			// textBox_seizo
			// 
			this.textBox_seizo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_seizo.BackColor = System.Drawing.SystemColors.Control;
			this.textBox_seizo.Location = new System.Drawing.Point(73, 45);
			this.textBox_seizo.Name = "textBox_seizo";
			this.textBox_seizo.Size = new System.Drawing.Size(121, 19);
			this.textBox_seizo.TabIndex = 0;
			this.textBox_seizo.TabStop = false;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(5, 45);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(80, 20);
			this.label10.TabIndex = 0;
			this.label10.Text = "製造番号";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.comboBox_bKubun);
			this.groupBox2.Controls.Add(this.label14);
			this.groupBox2.Controls.Add(this.comboBox_pallet);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.combo_dnc);
			this.groupBox2.Controls.Add(this.comboBox_Dandori);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.textBoxOutName);
			this.groupBox2.Location = new System.Drawing.Point(208, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(237, 151);
			this.groupBox2.TabIndex = 0;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "加工方法";
			// 
			// comboBox_bKubun
			// 
			this.comboBox_bKubun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox_bKubun.FormattingEnabled = true;
			this.comboBox_bKubun.Items.AddRange(new object[] {
            "固定入子",
            "可動入子",
            "スライドコア",
            "直押コア",
            "傾斜コア"});
			this.comboBox_bKubun.Location = new System.Drawing.Point(78, 126);
			this.comboBox_bKubun.MaxDropDownItems = 20;
			this.comboBox_bKubun.Name = "comboBox_bKubun";
			this.comboBox_bKubun.Size = new System.Drawing.Size(121, 20);
			this.comboBox_bKubun.TabIndex = 8;
			this.comboBox_bKubun.Text = "(未設定)";
			this.comboBox_bKubun.SelectedIndexChanged += new System.EventHandler(this.ComboBox_bunri_SelectedIndexChanged);
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(8, 126);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(79, 20);
			this.label14.TabIndex = 7;
			this.label14.Text = "部品種類";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 100);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(79, 20);
			this.label2.TabIndex = 5;
			this.label2.Text = "パレットNo";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonTool
			// 
			this.buttonTool.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonTool.Location = new System.Drawing.Point(464, 257);
			this.buttonTool.Name = "buttonTool";
			this.buttonTool.Size = new System.Drawing.Size(91, 23);
			this.buttonTool.TabIndex = 12;
			this.buttonTool.Text = "工具表出力";
			this.buttonTool.UseVisualStyleBackColor = true;
			this.buttonTool.Visible = false;
			this.buttonTool.Click += new System.EventHandler(this.ButtonTool_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Enabled = false;
			this.buttonOK.Location = new System.Drawing.Point(576, 228);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 8;
			this.buttonOK.Text = "確定";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.label12);
			this.groupBox3.Controls.Add(this.textBox_Z);
			this.groupBox3.Controls.Add(this.label7);
			this.groupBox3.Controls.Add(this.textBox_H);
			this.groupBox3.Controls.Add(this.textBox_Y);
			this.groupBox3.Controls.Add(this.textBox_X);
			this.groupBox3.Controls.Add(this.label11);
			this.groupBox3.Controls.Add(this.label13);
			this.groupBox3.Location = new System.Drawing.Point(208, 170);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(237, 80);
			this.groupBox3.TabIndex = 0;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "部品サイズ";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(117, 41);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(17, 20);
			this.label12.TabIndex = 11;
			this.label12.Text = "Z";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBox_Z
			// 
			this.textBox_Z.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_Z.Enabled = false;
			this.textBox_Z.Location = new System.Drawing.Point(133, 42);
			this.textBox_Z.Name = "textBox_Z";
			this.textBox_Z.Size = new System.Drawing.Size(86, 19);
			this.textBox_Z.TabIndex = 10;
			this.textBox_Z.Text = "0";
			this.textBox_Z.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(10, 39);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(17, 20);
			this.label7.TabIndex = 9;
			this.label7.Text = "H";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBox_H
			// 
			this.textBox_H.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_H.Enabled = false;
			this.textBox_H.Location = new System.Drawing.Point(25, 42);
			this.textBox_H.Name = "textBox_H";
			this.textBox_H.Size = new System.Drawing.Size(86, 19);
			this.textBox_H.TabIndex = 8;
			this.textBox_H.Text = "0";
			this.textBox_H.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBox_Y
			// 
			this.textBox_Y.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_Y.Location = new System.Drawing.Point(133, 17);
			this.textBox_Y.Name = "textBox_Y";
			this.textBox_Y.Size = new System.Drawing.Size(86, 19);
			this.textBox_Y.TabIndex = 7;
			this.textBox_Y.Text = "0";
			this.textBox_Y.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBox_Y.TextChanged += new System.EventHandler(this.TextBox_Y_TextChanged);
			// 
			// textBox_X
			// 
			this.textBox_X.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_X.Location = new System.Drawing.Point(25, 17);
			this.textBox_X.Name = "textBox_X";
			this.textBox_X.Size = new System.Drawing.Size(86, 19);
			this.textBox_X.TabIndex = 6;
			this.textBox_X.Text = "0";
			this.textBox_X.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBox_X.TextChanged += new System.EventHandler(this.TextBox_X_TextChanged);
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(117, 17);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(17, 20);
			this.label11.TabIndex = 0;
			this.label11.Text = "Ｙ";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(10, 17);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(14, 20);
			this.label13.TabIndex = 0;
			this.label13.Text = "Ｘ";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FormNcSet_Buhin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(659, 286);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonTool);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonNc);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(370, 262);
			this.Name = "FormNcSet_Buhin";
			this.Text = "ＴＥＸＡＳ出力";
			this.Shown += new System.EventHandler(this.FormNcSelect_Shown);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxOutName;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button buttonNc;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonTool;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBox_katana;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label13;
		public System.Windows.Forms.ComboBox combo_dnc;
		public System.Windows.Forms.ComboBox comboBox_Dandori;
		public System.Windows.Forms.TextBox textBox_Y;
		public System.Windows.Forms.TextBox textBox_X;
		public System.Windows.Forms.ComboBox comboBox_buhin;
		public System.Windows.Forms.ComboBox comboBox_kotei;
		public System.Windows.Forms.TextBox textBoxComment;
		public System.Windows.Forms.TextBox textBox_naisei;
		public System.Windows.Forms.TextBox textBox_seizo;
		private System.Windows.Forms.Label label10;
		public System.Windows.Forms.Label label2;
		public System.Windows.Forms.ComboBox comboBox_pallet;
		public System.Windows.Forms.TextBox textBox_H;
		private System.Windows.Forms.Label label7;
		public System.Windows.Forms.TextBox textBox_Z;
		private System.Windows.Forms.Label label12;
		public System.Windows.Forms.ComboBox comboBox_bKubun;
		public System.Windows.Forms.Label label14;
	}
}