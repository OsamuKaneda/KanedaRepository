namespace NcTejun.TejunSet
{
	partial class ToolSheet
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ToolSheet));
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripButton_Decision = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton_Cancel = new System.Windows.Forms.ToolStripButton();
			this.開くOtoolStripButton = new System.Windows.Forms.ToolStripButton();
			this.上書き保存SToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.印刷PToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButton_TMoSet = new System.Windows.Forms.ToolStripButton();
			this.行追加toolStripButton = new System.Windows.Forms.ToolStripButton();
			this.切り取りUToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
			this.再作成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.不足工具追加ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.工具番号整理ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.選択工具以降ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.選択工具以降シート分割ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripButton_Shomo = new System.Windows.Forms.ToolStripButton();
			this.コピーCToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.貼り付けPToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.元に戻すtoolStripButton = new System.Windows.Forms.ToolStripButton();
			this.やり直しtoolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.ヘルプLToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.helpProvider1 = new System.Windows.Forms.HelpProvider();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.toolStrip1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToResizeRows = false;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(0, 25);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.RowTemplate.Height = 21;
			this.dataGridView1.Size = new System.Drawing.Size(792, 283);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellValueChanged);
			this.dataGridView1.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridView1_ColumnHeaderMouseClick);
			this.dataGridView1.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridView1_DataError);
			this.dataGridView1.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.DataGridView1_UserAddedRow);
			this.dataGridView1.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.DataGridView1_UserDeletedRow);
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.toolStripButton_Decision,
			this.toolStripButton_Cancel,
			this.開くOtoolStripButton,
			this.上書き保存SToolStripButton,
			this.印刷PToolStripButton,
			this.toolStripSeparator,
			this.toolStripButton_TMoSet,
			this.行追加toolStripButton,
			this.切り取りUToolStripButton,
			this.toolStripSplitButton1,
			this.toolStripButton_Shomo,
			this.コピーCToolStripButton,
			this.貼り付けPToolStripButton,
			this.元に戻すtoolStripButton,
			this.やり直しtoolStripButton,
			this.toolStripSeparator1,
			this.ヘルプLToolStripButton});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(792, 25);
			this.toolStrip1.TabIndex = 1;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripButton_Decision
			// 
			this.toolStripButton_Decision.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripButton_Decision.Enabled = false;
			this.toolStripButton_Decision.ForeColor = System.Drawing.Color.Magenta;
			this.toolStripButton_Decision.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_Decision.Image")));
			this.toolStripButton_Decision.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton_Decision.Name = "toolStripButton_Decision";
			this.toolStripButton_Decision.Size = new System.Drawing.Size(72, 22);
			this.toolStripButton_Decision.Text = "工具表確定";
			this.toolStripButton_Decision.ToolTipText = "工具表に対して行われた変更をコミットします";
			this.toolStripButton_Decision.Click += new System.EventHandler(this.ToolStripButton_Decision_Click);
			// 
			// toolStripButton_Cancel
			// 
			this.toolStripButton_Cancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripButton_Cancel.Enabled = false;
			this.toolStripButton_Cancel.ForeColor = System.Drawing.Color.Magenta;
			this.toolStripButton_Cancel.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_Cancel.Image")));
			this.toolStripButton_Cancel.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton_Cancel.Name = "toolStripButton_Cancel";
			this.toolStripButton_Cancel.Size = new System.Drawing.Size(36, 22);
			this.toolStripButton_Cancel.Text = "取消";
			this.toolStripButton_Cancel.ToolTipText = "工具表に対して行われた「確定」以降の変更を取消します";
			this.toolStripButton_Cancel.Click += new System.EventHandler(this.ToolStripButton_Cancel_Click);
			// 
			// 開くOtoolStripButton
			// 
			this.開くOtoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.開くOtoolStripButton.Enabled = false;
			this.開くOtoolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("開くOtoolStripButton.Image")));
			this.開くOtoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.開くOtoolStripButton.Name = "開くOtoolStripButton";
			this.開くOtoolStripButton.RightToLeftAutoMirrorImage = true;
			this.開くOtoolStripButton.Size = new System.Drawing.Size(23, 22);
			this.開くOtoolStripButton.Text = "開くOtoolStripButton";
			this.開くOtoolStripButton.ToolTipText = "全ての変更を取消し、再度保存データを読込みます";
			this.開くOtoolStripButton.Click += new System.EventHandler(this.開くOtoolStripButton_Click);
			// 
			// 上書き保存SToolStripButton
			// 
			this.上書き保存SToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.上書き保存SToolStripButton.Enabled = false;
			this.上書き保存SToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("上書き保存SToolStripButton.Image")));
			this.上書き保存SToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.上書き保存SToolStripButton.Name = "上書き保存SToolStripButton";
			this.上書き保存SToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.上書き保存SToolStripButton.Text = "上書き保存(&S)";
			this.上書き保存SToolStripButton.Click += new System.EventHandler(this.上書き保存SToolStripButton_Click);
			// 
			// 印刷PToolStripButton
			// 
			this.印刷PToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.印刷PToolStripButton.Enabled = false;
			this.印刷PToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("印刷PToolStripButton.Image")));
			this.印刷PToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.印刷PToolStripButton.Name = "印刷PToolStripButton";
			this.印刷PToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.印刷PToolStripButton.Text = "印刷(&P)...";
			// 
			// toolStripSeparator
			// 
			this.toolStripSeparator.Name = "toolStripSeparator";
			this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButton_TMoSet
			// 
			this.toolStripButton_TMoSet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripButton_TMoSet.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_TMoSet.Image")));
			this.toolStripButton_TMoSet.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton_TMoSet.Name = "toolStripButton_TMoSet";
			this.toolStripButton_TMoSet.Size = new System.Drawing.Size(108, 22);
			this.toolStripButton_TMoSet.Text = "工具番号手動設定";
			this.toolStripButton_TMoSet.Click += new System.EventHandler(this.ToolStripButton_TMoSet_Click);
			// 
			// 行追加toolStripButton
			// 
			this.行追加toolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.行追加toolStripButton.Image = global::NcTejun.Properties.Resources.行の挿入;
			this.行追加toolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.行追加toolStripButton.Name = "行追加toolStripButton";
			this.行追加toolStripButton.Size = new System.Drawing.Size(23, 22);
			this.行追加toolStripButton.Text = "行追加toolStripButton";
			this.行追加toolStripButton.ToolTipText = "行追加";
			this.行追加toolStripButton.Click += new System.EventHandler(this.行追加toolStripButton_Click);
			// 
			// 切り取りUToolStripButton
			// 
			this.切り取りUToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.切り取りUToolStripButton.Image = global::NcTejun.Properties.Resources.行の削除;
			this.切り取りUToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.切り取りUToolStripButton.Name = "切り取りUToolStripButton";
			this.切り取りUToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.切り取りUToolStripButton.Text = "切り取り(&U)";
			this.切り取りUToolStripButton.ToolTipText = "行削除(U)";
			this.切り取りUToolStripButton.Click += new System.EventHandler(this.切り取りUToolStripButton_Click);
			// 
			// toolStripSplitButton1
			// 
			this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.再作成ToolStripMenuItem,
			this.不足工具追加ToolStripMenuItem,
			this.工具番号整理ToolStripMenuItem});
			this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
			this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButton1.Name = "toolStripSplitButton1";
			this.toolStripSplitButton1.Size = new System.Drawing.Size(96, 22);
			this.toolStripSplitButton1.Text = "編集メニュー";
			// 
			// 再作成ToolStripMenuItem
			// 
			this.再作成ToolStripMenuItem.Name = "再作成ToolStripMenuItem";
			this.再作成ToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
			this.再作成ToolStripMenuItem.Text = "再作成";
			this.再作成ToolStripMenuItem.Click += new System.EventHandler(this.再作成ToolStripMenuItem_Click);
			// 
			// 不足工具追加ToolStripMenuItem
			// 
			this.不足工具追加ToolStripMenuItem.Name = "不足工具追加ToolStripMenuItem";
			this.不足工具追加ToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
			this.不足工具追加ToolStripMenuItem.Text = "不足工具追加";
			this.不足工具追加ToolStripMenuItem.Click += new System.EventHandler(this.不足工具追加ToolStripMenuItem_Click);
			// 
			// 工具番号整理ToolStripMenuItem
			// 
			this.工具番号整理ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.選択工具以降ToolStripMenuItem,
			this.選択工具以降シート分割ToolStripMenuItem});
			this.工具番号整理ToolStripMenuItem.Name = "工具番号整理ToolStripMenuItem";
			this.工具番号整理ToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
			this.工具番号整理ToolStripMenuItem.Text = "工具番号整理";
			// 
			// 選択工具以降ToolStripMenuItem
			// 
			this.選択工具以降ToolStripMenuItem.Name = "選択工具以降ToolStripMenuItem";
			this.選択工具以降ToolStripMenuItem.Size = new System.Drawing.Size(260, 22);
			this.選択工具以降ToolStripMenuItem.Text = "選択工具を基準に以降 リナンバー";
			this.選択工具以降ToolStripMenuItem.ToolTipText = "選択している工具の工具番号を基準に、それ以降使用する工具の番号を使用順に整理します";
			this.選択工具以降ToolStripMenuItem.Click += new System.EventHandler(this.選択工具以降ToolStripMenuItem_Click);
			// 
			// 選択工具以降シート分割ToolStripMenuItem
			// 
			this.選択工具以降シート分割ToolStripMenuItem.Name = "選択工具以降シート分割ToolStripMenuItem";
			this.選択工具以降シート分割ToolStripMenuItem.Size = new System.Drawing.Size(260, 22);
			this.選択工具以降シート分割ToolStripMenuItem.Text = "選択工具以降シート分割し再作成";
			this.選択工具以降シート分割ToolStripMenuItem.ToolTipText = "選択している工具以降を消去しシートを分割した工具を新たに追加します";
			this.選択工具以降シート分割ToolStripMenuItem.Click += new System.EventHandler(this.選択工具以降シート分割ToolStripMenuItem_Click);
			// 
			// toolStripButton_Shomo
			// 
			this.toolStripButton_Shomo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripButton_Shomo.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_Shomo.Image")));
			this.toolStripButton_Shomo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton_Shomo.Name = "toolStripButton_Shomo";
			this.toolStripButton_Shomo.Size = new System.Drawing.Size(84, 22);
			this.toolStripButton_Shomo.Text = "全消耗クリア";
			this.toolStripButton_Shomo.Click += new System.EventHandler(this.ToolStripButton_Shomo_Click);
			// 
			// コピーCToolStripButton
			// 
			this.コピーCToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.コピーCToolStripButton.Enabled = false;
			this.コピーCToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("コピーCToolStripButton.Image")));
			this.コピーCToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.コピーCToolStripButton.Name = "コピーCToolStripButton";
			this.コピーCToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.コピーCToolStripButton.Text = "コピー(&C)";
			// 
			// 貼り付けPToolStripButton
			// 
			this.貼り付けPToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.貼り付けPToolStripButton.Enabled = false;
			this.貼り付けPToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("貼り付けPToolStripButton.Image")));
			this.貼り付けPToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.貼り付けPToolStripButton.Name = "貼り付けPToolStripButton";
			this.貼り付けPToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.貼り付けPToolStripButton.Text = "貼り付け(&P)";
			// 
			// 元に戻すtoolStripButton
			// 
			this.元に戻すtoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.元に戻すtoolStripButton.Enabled = false;
			this.元に戻すtoolStripButton.Image = global::NcTejun.Properties.Resources.元に戻す;
			this.元に戻すtoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.元に戻すtoolStripButton.Name = "元に戻すtoolStripButton";
			this.元に戻すtoolStripButton.Size = new System.Drawing.Size(23, 22);
			this.元に戻すtoolStripButton.Text = "元に戻すtoolStripButton";
			this.元に戻すtoolStripButton.ToolTipText = "元に戻す(Ctrl+Z)";
			this.元に戻すtoolStripButton.Click += new System.EventHandler(this.元に戻すtoolStripButton_Click);
			// 
			// やり直しtoolStripButton
			// 
			this.やり直しtoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.やり直しtoolStripButton.Enabled = false;
			this.やり直しtoolStripButton.Image = global::NcTejun.Properties.Resources.やり直し;
			this.やり直しtoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.やり直しtoolStripButton.Name = "やり直しtoolStripButton";
			this.やり直しtoolStripButton.Size = new System.Drawing.Size(23, 22);
			this.やり直しtoolStripButton.Text = "やり直しtoolStripButton";
			this.やり直しtoolStripButton.ToolTipText = "やり直し(Ctrl+Y)";
			this.やり直しtoolStripButton.Click += new System.EventHandler(this.やり直しtoolStripButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// ヘルプLToolStripButton
			// 
			this.ヘルプLToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ヘルプLToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("ヘルプLToolStripButton.Image")));
			this.ヘルプLToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ヘルプLToolStripButton.Name = "ヘルプLToolStripButton";
			this.ヘルプLToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.ヘルプLToolStripButton.Text = "ヘルプ(&L)";
			this.ヘルプLToolStripButton.Click += new System.EventHandler(this.ヘルプLToolStripButton_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.toolStripStatusLabel1});
			this.statusStrip1.Location = new System.Drawing.Point(0, 308);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(792, 23);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(134, 18);
			this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
			// 
			// ToolSheet
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(792, 331);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.toolStrip1);
			this.Name = "ToolSheet";
			this.ShowInTaskbar = false;
			this.Text = "FormToolSheet";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormToolSheet_FormClosing);
			this.Shown += new System.EventHandler(this.ToolSheet_Shown);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton 上書き保存SToolStripButton;
		private System.Windows.Forms.ToolStripButton 印刷PToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
		private System.Windows.Forms.ToolStripButton 切り取りUToolStripButton;
		private System.Windows.Forms.ToolStripButton コピーCToolStripButton;
		private System.Windows.Forms.ToolStripButton 貼り付けPToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton ヘルプLToolStripButton;
		private System.Windows.Forms.ToolStripButton 元に戻すtoolStripButton;
		private System.Windows.Forms.ToolStripButton 行追加toolStripButton;
		private System.Windows.Forms.ToolStripButton やり直しtoolStripButton;
		private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
		private System.Windows.Forms.ToolStripMenuItem 不足工具追加ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 工具番号整理ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 選択工具以降ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 選択工具以降シート分割ToolStripMenuItem;
		private System.Windows.Forms.HelpProvider helpProvider1;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.ToolStripMenuItem 再作成ToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton toolStripButton_Decision;
		private System.Windows.Forms.ToolStripButton 開くOtoolStripButton;
		private System.Windows.Forms.ToolStripButton toolStripButton_Cancel;
		private System.Windows.Forms.ToolStripButton toolStripButton_TMoSet;
		private System.Windows.Forms.ToolStripButton toolStripButton_Shomo;

	}
}