namespace NcTejun.Output
{
	partial class FormNcSet_NonTexas
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
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonTool = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonNc = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.textBoxOutName = new System.Windows.Forms.TextBox();
			this.checkBox_minZ = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(310, 242);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 14;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonTool
			// 
			this.buttonTool.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonTool.Location = new System.Drawing.Point(167, 242);
			this.buttonTool.Name = "buttonTool";
			this.buttonTool.Size = new System.Drawing.Size(134, 23);
			this.buttonTool.TabIndex = 12;
			this.buttonTool.TabStop = false;
			this.buttonTool.Text = "工具表/手順書出力";
			this.buttonTool.UseVisualStyleBackColor = true;
			this.buttonTool.Visible = false;
			this.buttonTool.Click += new System.EventHandler(this.ButtonTool_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Location = new System.Drawing.Point(310, 213);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 13;
			this.buttonOK.Text = "確定";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// buttonNc
			// 
			this.buttonNc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonNc.Location = new System.Drawing.Point(10, 242);
			this.buttonNc.Name = "buttonNc";
			this.buttonNc.Size = new System.Drawing.Size(91, 23);
			this.buttonNc.TabIndex = 10;
			this.buttonNc.TabStop = false;
			this.buttonNc.Text = "ＮＣデータ出力";
			this.buttonNc.UseVisualStyleBackColor = true;
			this.buttonNc.Visible = false;
			this.buttonNc.Click += new System.EventHandler(this.ButtonNc_Click);
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label8.Location = new System.Drawing.Point(229, 9);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(42, 20);
			this.label8.TabIndex = 16;
			this.label8.Text = "出力名";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxOutName
			// 
			this.textBoxOutName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxOutName.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxOutName.Location = new System.Drawing.Point(277, 12);
			this.textBoxOutName.Name = "textBoxOutName";
			this.textBoxOutName.Size = new System.Drawing.Size(100, 19);
			this.textBoxOutName.TabIndex = 0;
			this.textBoxOutName.TabStop = false;
			// 
			// checkBox_minZ
			// 
			this.checkBox_minZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBox_minZ.AutoSize = true;
			this.checkBox_minZ.Enabled = false;
			this.checkBox_minZ.Location = new System.Drawing.Point(231, 44);
			this.checkBox_minZ.Name = "checkBox_minZ";
			this.checkBox_minZ.Size = new System.Drawing.Size(146, 16);
			this.checkBox_minZ.TabIndex = 17;
			this.checkBox_minZ.Text = "工具表に加工深さを表示";
			this.checkBox_minZ.UseVisualStyleBackColor = true;
			// 
			// FormNcSet_NonTexas
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(389, 268);
			this.Controls.Add(this.checkBox_minZ);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textBoxOutName);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonTool);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonNc);
			this.Name = "FormNcSet_NonTexas";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected System.Windows.Forms.Button buttonCancel;
		protected System.Windows.Forms.Button buttonTool;
		protected System.Windows.Forms.Button buttonOK;
		protected System.Windows.Forms.Button buttonNc;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textBoxOutName;
		private System.Windows.Forms.CheckBox checkBox_minZ;
	}
}