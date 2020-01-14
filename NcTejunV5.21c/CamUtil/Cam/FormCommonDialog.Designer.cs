namespace CamUtil
{
	partial class FormCommonDialog
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
			this.label_message = new System.Windows.Forms.Label();
			this.CANCEL = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label_message
			// 
			this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.label_message.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.label_message.Location = new System.Drawing.Point(0, 0);
			this.label_message.Margin = new System.Windows.Forms.Padding(0);
			this.label_message.Name = "label_message";
			this.label_message.Size = new System.Drawing.Size(366, 50);
			this.label_message.TabIndex = 0;
			this.label_message.Text = "加工手順";
			this.label_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CANCEL
			// 
			this.CANCEL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CANCEL.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CANCEL.Location = new System.Drawing.Point(279, 53);
			this.CANCEL.Name = "CANCEL";
			this.CANCEL.Size = new System.Drawing.Size(75, 23);
			this.CANCEL.TabIndex = 1;
			this.CANCEL.TabStop = false;
			this.CANCEL.Text = "CANCEL";
			this.CANCEL.UseVisualStyleBackColor = true;
			this.CANCEL.Click += new System.EventHandler(this.CANCEL_Click);
			// 
			// FormCommonDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(366, 82);
			this.Controls.Add(this.CANCEL);
			this.Controls.Add(this.label_message);
			this.Name = "FormCommonDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "加工手順情報の取得";
			this.Shown += new System.EventHandler(this.FormFormTejunSet_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button CANCEL;
		private System.Windows.Forms.Label label_message;


	}
}