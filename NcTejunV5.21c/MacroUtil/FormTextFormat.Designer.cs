namespace MacroUtil
{
	partial class FormDivode
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
			this.checkBox_SP = new System.Windows.Forms.CheckBox();
			this.checkBoxLF = new System.Windows.Forms.CheckBox();
			this.checkBoxOdd = new System.Windows.Forms.CheckBox();
			this.checkBox_CR = new System.Windows.Forms.CheckBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// checkBox_SP
			// 
			this.checkBox_SP.AutoSize = true;
			this.checkBox_SP.Location = new System.Drawing.Point(12, 34);
			this.checkBox_SP.Name = "checkBox_SP";
			this.checkBox_SP.Size = new System.Drawing.Size(148, 16);
			this.checkBox_SP.TabIndex = 10;
			this.checkBox_SP.Text = "前後のスペースを削除する";
			this.checkBox_SP.UseVisualStyleBackColor = true;
			// 
			// checkBoxLF
			// 
			this.checkBoxLF.AutoSize = true;
			this.checkBoxLF.Location = new System.Drawing.Point(12, 78);
			this.checkBoxLF.Name = "checkBoxLF";
			this.checkBoxLF.Size = new System.Drawing.Size(171, 16);
			this.checkBoxLF.TabIndex = 13;
			this.checkBoxLF.Text = "最後の行に改行コードを入れる";
			this.checkBoxLF.UseVisualStyleBackColor = true;
			this.checkBoxLF.Visible = false;
			// 
			// checkBoxOdd
			// 
			this.checkBoxOdd.AutoSize = true;
			this.checkBoxOdd.Checked = true;
			this.checkBoxOdd.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxOdd.Location = new System.Drawing.Point(12, 56);
			this.checkBoxOdd.Name = "checkBoxOdd";
			this.checkBoxOdd.Size = new System.Drawing.Size(215, 16);
			this.checkBoxOdd.TabIndex = 12;
			this.checkBoxOdd.Text = "スペースを追加して各行を奇数文字数に";
			this.checkBoxOdd.UseVisualStyleBackColor = true;
			// 
			// checkBox_CR
			// 
			this.checkBox_CR.AutoSize = true;
			this.checkBox_CR.Location = new System.Drawing.Point(12, 12);
			this.checkBox_CR.Name = "checkBox_CR";
			this.checkBox_CR.Size = new System.Drawing.Size(155, 16);
			this.checkBox_CR.TabIndex = 11;
			this.checkBox_CR.Text = "改行コードを CR-LF にする";
			this.checkBox_CR.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Location = new System.Drawing.Point(167, 131);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 14;
			this.buttonCancel.Text = "CANCEL";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Location = new System.Drawing.Point(248, 131);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 15;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// FormTextFormat
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(335, 161);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.checkBoxLF);
			this.Controls.Add(this.checkBoxOdd);
			this.Controls.Add(this.checkBox_CR);
			this.Controls.Add(this.checkBox_SP);
			this.Name = "FormTextFormat";
			this.Text = "FormTextFormat";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox checkBox_SP;
		private System.Windows.Forms.CheckBox checkBoxLF;
		private System.Windows.Forms.CheckBox checkBoxOdd;
		private System.Windows.Forms.CheckBox checkBox_CR;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
	}
}