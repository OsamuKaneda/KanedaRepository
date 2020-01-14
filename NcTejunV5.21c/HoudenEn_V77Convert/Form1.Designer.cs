namespace HoudenEn_V77Convert
{
	partial class Form1
	{
		/// <summary>
		/// 必要なデザイナー変数です。
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

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent() {
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.button_OK = new System.Windows.Forms.Button();
			this.button_CANCEL = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox_filename = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// button_OK
			// 
			this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_OK.Location = new System.Drawing.Point(235, 67);
			this.button_OK.Name = "button_OK";
			this.button_OK.Size = new System.Drawing.Size(75, 23);
			this.button_OK.TabIndex = 2;
			this.button_OK.Text = "変換実行";
			this.button_OK.UseVisualStyleBackColor = true;
			this.button_OK.Click += new System.EventHandler(this.Button_OK_Click);
			// 
			// button_CANCEL
			// 
			this.button_CANCEL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_CANCEL.Location = new System.Drawing.Point(316, 67);
			this.button_CANCEL.Name = "button_CANCEL";
			this.button_CANCEL.Size = new System.Drawing.Size(75, 23);
			this.button_CANCEL.TabIndex = 0;
			this.button_CANCEL.Text = "キャンセル";
			this.button_CANCEL.UseVisualStyleBackColor = true;
			this.button_CANCEL.Click += new System.EventHandler(this.Button_CANCEL_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 40);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(69, 12);
			this.label1.TabIndex = 3;
			this.label1.Text = "csvファイル名";
			// 
			// textBox_filename
			// 
			this.textBox_filename.Location = new System.Drawing.Point(98, 33);
			this.textBox_filename.Name = "textBox_filename";
			this.textBox_filename.Size = new System.Drawing.Size(295, 19);
			this.textBox_filename.TabIndex = 1;
			this.textBox_filename.Text = "ダブルクリック、ドロップ可";
			this.textBox_filename.DoubleClick += new System.EventHandler(this.TextBox_filename_DoubleClick);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(381, 12);
			this.label2.TabIndex = 4;
			this.label2.Text = "変換するＮＣデータのフォルダー内にCSVファイルを作成し、それを選択してください";
			// 
			// Form1
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(403, 102);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBox_filename);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button_CANCEL);
			this.Controls.Add(this.button_OK);
			this.Name = "Form1";
			this.Text = "ＮＣデータ変換";
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_CANCEL;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_filename;
		private System.Windows.Forms.Label label2;
	}
}

