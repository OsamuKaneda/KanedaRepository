namespace MacroUtil
{
	partial class Form1
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
			this.button_Div = new System.Windows.Forms.Button();
			this.button_CRLF = new System.Windows.Forms.Button();
			this.button_16 = new System.Windows.Forms.Button();
			this.button_Test = new System.Windows.Forms.Button();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Div
			// 
			this.button_Div.Location = new System.Drawing.Point(28, 12);
			this.button_Div.Name = "button_Div";
			this.button_Div.Size = new System.Drawing.Size(210, 23);
			this.button_Div.TabIndex = 2;
			this.button_Div.Text = "Ｏ番号単位に分割\r\n";
			this.button_Div.UseVisualStyleBackColor = true;
			this.button_Div.Click += new System.EventHandler(this.Button_Div_Click);
			// 
			// button_CRLF
			// 
			this.button_CRLF.Location = new System.Drawing.Point(28, 41);
			this.button_CRLF.Name = "button_CRLF";
			this.button_CRLF.Size = new System.Drawing.Size(210, 23);
			this.button_CRLF.TabIndex = 3;
			this.button_CRLF.Text = "書式を整える";
			this.button_CRLF.UseVisualStyleBackColor = true;
			this.button_CRLF.Click += new System.EventHandler(this.Button_CRLF_Click);
			// 
			// button_16
			// 
			this.button_16.Location = new System.Drawing.Point(28, 70);
			this.button_16.Name = "button_16";
			this.button_16.Size = new System.Drawing.Size(210, 23);
			this.button_16.TabIndex = 4;
			this.button_16.Text = "ファイルの１６進表示";
			this.button_16.UseVisualStyleBackColor = true;
			this.button_16.Click += new System.EventHandler(this.Button_16_Click);
			// 
			// button_Test
			// 
			this.button_Test.Location = new System.Drawing.Point(28, 99);
			this.button_Test.Name = "button_Test";
			this.button_Test.Size = new System.Drawing.Size(210, 23);
			this.button_Test.TabIndex = 8;
			this.button_Test.Text = "Test";
			this.button_Test.UseVisualStyleBackColor = true;
			this.button_Test.Click += new System.EventHandler(this.Button_Test_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
			this.statusStrip1.Location = new System.Drawing.Point(0, 133);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(265, 22);
			this.statusStrip1.TabIndex = 9;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.AutoSize = false;
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(50, 17);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(265, 155);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.button_Test);
			this.Controls.Add(this.button_16);
			this.Controls.Add(this.button_CRLF);
			this.Controls.Add(this.button_Div);
			this.Name = "Form1";
			this.Text = "マクロプログラム変換";
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button_Div;
		private System.Windows.Forms.Button button_CRLF;
		private System.Windows.Forms.Button button_16;
		private System.Windows.Forms.Button button_Test;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
	}
}

