namespace Test
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
			this.button_Refer = new System.Windows.Forms.Button();
			this.button_Matrix = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button_Refer
			// 
			this.button_Refer.Location = new System.Drawing.Point(26, 25);
			this.button_Refer.Name = "button_Refer";
			this.button_Refer.Size = new System.Drawing.Size(140, 23);
			this.button_Refer.TabIndex = 0;
			this.button_Refer.Text = "参照型test";
			this.button_Refer.UseVisualStyleBackColor = true;
			this.button_Refer.Click += new System.EventHandler(this.Button_Refer_Click);
			// 
			// button_Matrix
			// 
			this.button_Matrix.Location = new System.Drawing.Point(26, 55);
			this.button_Matrix.Name = "button_Matrix";
			this.button_Matrix.Size = new System.Drawing.Size(140, 23);
			this.button_Matrix.TabIndex = 1;
			this.button_Matrix.Text = "マトリックス";
			this.button_Matrix.UseVisualStyleBackColor = true;
			this.button_Matrix.Click += new System.EventHandler(this.Button_Matrix_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.button_Matrix);
			this.Controls.Add(this.button_Refer);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button button_Refer;
		private System.Windows.Forms.Button button_Matrix;
	}
}