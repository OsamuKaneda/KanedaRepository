namespace NcTejun.TejunSet
{
	partial class FormSFSet
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
			this.button_OK = new System.Windows.Forms.Button();
			this.button_CANCEL = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label_RatL = new System.Windows.Forms.Label();
			this.label_RatS = new System.Windows.Forms.Label();
			this.label_RatF = new System.Windows.Forms.Label();
			this.label_NowL = new System.Windows.Forms.Label();
			this.label_OrgL = new System.Windows.Forms.Label();
			this.label_NowF = new System.Windows.Forms.Label();
			this.label_NowS = new System.Windows.Forms.Label();
			this.label_OrgF = new System.Windows.Forms.Label();
			this.label_OrgS = new System.Windows.Forms.Label();
			this.label_TOOL = new System.Windows.Forms.Label();
			this.label_NCD = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_OK
			// 
			this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_OK.Location = new System.Drawing.Point(817, 301);
			this.button_OK.Name = "button_OK";
			this.button_OK.Size = new System.Drawing.Size(75, 23);
			this.button_OK.TabIndex = 0;
			this.button_OK.Text = "OK";
			this.button_OK.UseVisualStyleBackColor = true;
			this.button_OK.Click += new System.EventHandler(this.Button_OK_Click);
			// 
			// button_CANCEL
			// 
			this.button_CANCEL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_CANCEL.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_CANCEL.Location = new System.Drawing.Point(898, 301);
			this.button_CANCEL.Name = "button_CANCEL";
			this.button_CANCEL.Size = new System.Drawing.Size(75, 23);
			this.button_CANCEL.TabIndex = 1;
			this.button_CANCEL.Text = "CANCEL";
			this.button_CANCEL.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.AutoScroll = true;
			this.panel1.Controls.Add(this.label_RatL);
			this.panel1.Controls.Add(this.label_RatS);
			this.panel1.Controls.Add(this.label_RatF);
			this.panel1.Controls.Add(this.label_NowL);
			this.panel1.Controls.Add(this.label_OrgL);
			this.panel1.Controls.Add(this.label_NowF);
			this.panel1.Controls.Add(this.label_NowS);
			this.panel1.Controls.Add(this.label_OrgF);
			this.panel1.Controls.Add(this.label_OrgS);
			this.panel1.Controls.Add(this.label_TOOL);
			this.panel1.Controls.Add(this.label_NCD);
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(994, 290);
			this.panel1.TabIndex = 0;
			// 
			// label_RatL
			// 
			this.label_RatL.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_RatL.Location = new System.Drawing.Point(430, 6);
			this.label_RatL.Name = "label_RatL";
			this.label_RatL.Size = new System.Drawing.Size(60, 13);
			this.label_RatL.TabIndex = 5;
			this.label_RatL.Text = "比率(Ｌ)";
			this.label_RatL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_RatS
			// 
			this.label_RatS.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_RatS.Location = new System.Drawing.Point(668, 6);
			this.label_RatS.Name = "label_RatS";
			this.label_RatS.Size = new System.Drawing.Size(60, 13);
			this.label_RatS.TabIndex = 4;
			this.label_RatS.Text = "比率(S)";
			this.label_RatS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_RatF
			// 
			this.label_RatF.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_RatF.Location = new System.Drawing.Point(906, 6);
			this.label_RatF.Name = "label_RatF";
			this.label_RatF.Size = new System.Drawing.Size(60, 13);
			this.label_RatF.TabIndex = 3;
			this.label_RatF.Text = "比率(F)";
			this.label_RatF.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_NowL
			// 
			this.label_NowL.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_NowL.Location = new System.Drawing.Point(347, 6);
			this.label_NowL.Name = "label_NowL";
			this.label_NowL.Size = new System.Drawing.Size(80, 13);
			this.label_NowL.TabIndex = 2;
			this.label_NowL.Text = "現在値(L)";
			this.label_NowL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_OrgL
			// 
			this.label_OrgL.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_OrgL.Location = new System.Drawing.Point(264, 6);
			this.label_OrgL.Name = "label_OrgL";
			this.label_OrgL.Size = new System.Drawing.Size(80, 13);
			this.label_OrgL.TabIndex = 1;
			this.label_OrgL.Text = "初期値(L)";
			this.label_OrgL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_NowF
			// 
			this.label_NowF.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_NowF.Location = new System.Drawing.Point(823, 6);
			this.label_NowF.Name = "label_NowF";
			this.label_NowF.Size = new System.Drawing.Size(80, 13);
			this.label_NowF.TabIndex = 0;
			this.label_NowF.Text = "現在値(F)";
			this.label_NowF.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_NowS
			// 
			this.label_NowS.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_NowS.Location = new System.Drawing.Point(585, 6);
			this.label_NowS.Name = "label_NowS";
			this.label_NowS.Size = new System.Drawing.Size(80, 13);
			this.label_NowS.TabIndex = 0;
			this.label_NowS.Text = "現在値(S)";
			this.label_NowS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_OrgF
			// 
			this.label_OrgF.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_OrgF.Location = new System.Drawing.Point(740, 6);
			this.label_OrgF.Name = "label_OrgF";
			this.label_OrgF.Size = new System.Drawing.Size(80, 13);
			this.label_OrgF.TabIndex = 0;
			this.label_OrgF.Text = "初期値(F)";
			this.label_OrgF.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_OrgS
			// 
			this.label_OrgS.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_OrgS.Location = new System.Drawing.Point(502, 6);
			this.label_OrgS.Name = "label_OrgS";
			this.label_OrgS.Size = new System.Drawing.Size(80, 13);
			this.label_OrgS.TabIndex = 0;
			this.label_OrgS.Text = "初期値(S)";
			this.label_OrgS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_TOOL
			// 
			this.label_TOOL.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_TOOL.Location = new System.Drawing.Point(112, 6);
			this.label_TOOL.Name = "label_TOOL";
			this.label_TOOL.Size = new System.Drawing.Size(140, 13);
			this.label_TOOL.TabIndex = 0;
			this.label_TOOL.Text = "工具名";
			this.label_TOOL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_NCD
			// 
			this.label_NCD.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_NCD.Location = new System.Drawing.Point(6, 6);
			this.label_NCD.Name = "label_NCD";
			this.label_NCD.Size = new System.Drawing.Size(100, 13);
			this.label_NCD.TabIndex = 0;
			this.label_NCD.Text = "ＮＣデータ名";
			this.label_NCD.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FormSFSet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(998, 329);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.button_CANCEL);
			this.Controls.Add(this.button_OK);
			this.Name = "FormSFSet";
			this.Text = "FormSFSet";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_CANCEL;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label_NCD;
		private System.Windows.Forms.Label label_TOOL;
		private System.Windows.Forms.Label label_OrgS;
		private System.Windows.Forms.Label label_OrgF;
		private System.Windows.Forms.Label label_NowF;
		private System.Windows.Forms.Label label_NowS;
		private System.Windows.Forms.Label label_OrgL;
		private System.Windows.Forms.Label label_RatF;
		private System.Windows.Forms.Label label_NowL;
		private System.Windows.Forms.Label label_RatS;
		private System.Windows.Forms.Label label_RatL;
	}
}