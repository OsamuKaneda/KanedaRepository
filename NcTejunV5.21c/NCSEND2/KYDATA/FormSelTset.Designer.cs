namespace NCSEND2.KYDATA
{
	partial class FormSelTset
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
			System.Windows.Forms.Button buttonCANCEL;
			this.buttonOK = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.SET_ALL = new System.Windows.Forms.Button();
			buttonCANCEL = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// buttonCANCEL
			// 
			buttonCANCEL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			buttonCANCEL.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			buttonCANCEL.Font = new System.Drawing.Font("MS UI Gothic", 11.25F);
			buttonCANCEL.Location = new System.Drawing.Point(495, 195);
			buttonCANCEL.Name = "buttonCANCEL";
			buttonCANCEL.Size = new System.Drawing.Size(70, 23);
			buttonCANCEL.TabIndex = 5;
			buttonCANCEL.Text = "CANCEL";
			buttonCANCEL.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.buttonOK.Location = new System.Drawing.Point(419, 195);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(70, 23);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.AutoScroll = true;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(578, 186);
			this.panel1.TabIndex = 4;
			// 
			// SET_ALL
			// 
			this.SET_ALL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SET_ALL.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.SET_ALL.Location = new System.Drawing.Point(284, 195);
			this.SET_ALL.Name = "SET_ALL";
			this.SET_ALL.Size = new System.Drawing.Size(108, 23);
			this.SET_ALL.TabIndex = 6;
			this.SET_ALL.Text = "最優先を設定";
			this.SET_ALL.UseVisualStyleBackColor = true;
			this.SET_ALL.Click += new System.EventHandler(this.SET_ALL_Click);
			// 
			// FormSelTset
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(581, 222);
			this.ControlBox = false;
			this.Controls.Add(this.SET_ALL);
			this.Controls.Add(buttonCANCEL);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.buttonOK);
			this.Name = "FormSelTset";
			this.Text = "SELECT";
			this.Shown += new System.EventHandler(this.FormSelTset_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button SET_ALL;
	}
}