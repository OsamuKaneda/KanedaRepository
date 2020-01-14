namespace NcTejun.TejunSet
{
	partial class FormTejun
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTejun));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.CommittoolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
			this.toolStrip_Tname = new System.Windows.Forms.ToolStripTextBox();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.toolStrip_user = new System.Windows.Forms.ToolStripTextBox();
			this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
			this.toolStrip_seba = new System.Windows.Forms.ToolStripTextBox();
			this.toolStrip_seba2 = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
			this.toolStrip_machn = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
			this.toolStrip_kdate = new System.Windows.Forms.ToolStripTextBox();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			this.新規作成NToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.開くOToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.上書き保存SToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.印刷PToolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButton_Insert = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton_Remove = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton_Up = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton_Dn = new System.Windows.Forms.ToolStripButton();
			this.コピーCToolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.貼り付けPToolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
			this.ヘルプLToolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButton_srchNC = new System.Windows.Forms.ToolStripButton();
			this.toolStrip_SFSet = new System.Windows.Forms.ToolStripButton();
			this.toolStrip_Taore = new System.Windows.Forms.ToolStripDropDownButton();
			this.toolStrip_TaoreL = new System.Windows.Forms.ToolStripButton();
			this.toolStrip_TaoreZ = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
			this.toolStripMenuItem_Shokichi = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.toolStrip2.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.AllowItemReorder = true;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CommittoolStripButton,
            this.toolStripSeparator8,
            this.toolStripLabel4,
            this.toolStrip_Tname,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.toolStrip_user,
            this.toolStripSeparator9,
            this.toolStripLabel2,
            this.toolStrip_seba,
            this.toolStrip_seba2,
            this.toolStripSeparator10,
            this.toolStripLabel3,
            this.toolStrip_machn,
            this.toolStripSeparator15,
            this.toolStripLabel5,
            this.toolStrip_kdate});
			this.toolStrip1.Location = new System.Drawing.Point(0, 25);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(801, 26);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// CommittoolStripButton
			// 
			this.CommittoolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.CommittoolStripButton.ForeColor = System.Drawing.Color.Magenta;
			this.CommittoolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("CommittoolStripButton.Image")));
			this.CommittoolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.CommittoolStripButton.Name = "CommittoolStripButton";
			this.CommittoolStripButton.Size = new System.Drawing.Size(60, 23);
			this.CommittoolStripButton.Text = "手順確定";
			this.CommittoolStripButton.Click += new System.EventHandler(this.CommittoolStripButton_Click);
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(6, 26);
			// 
			// toolStripLabel4
			// 
			this.toolStripLabel4.Name = "toolStripLabel4";
			this.toolStripLabel4.Size = new System.Drawing.Size(44, 23);
			this.toolStripLabel4.Text = "手順名";
			// 
			// toolStrip_Tname
			// 
			this.toolStrip_Tname.Name = "toolStrip_Tname";
			this.toolStrip_Tname.Size = new System.Drawing.Size(90, 26);
			this.toolStrip_Tname.Leave += new System.EventHandler(this.ToolStrip_Tname_Leave);
			this.toolStrip_Tname.TextChanged += new System.EventHandler(this.ToolStripName_TextChanged);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(68, 23);
			this.toolStripLabel1.Text = "作成者番号";
			// 
			// toolStrip_user
			// 
			this.toolStrip_user.Name = "toolStrip_user";
			this.toolStrip_user.Size = new System.Drawing.Size(40, 26);
			this.toolStrip_user.TextChanged += new System.EventHandler(this.UserName_TextChanged);
			// 
			// toolStripSeparator9
			// 
			this.toolStripSeparator9.Name = "toolStripSeparator9";
			this.toolStripSeparator9.Size = new System.Drawing.Size(6, 26);
			// 
			// toolStripLabel2
			// 
			this.toolStripLabel2.Name = "toolStripLabel2";
			this.toolStripLabel2.Size = new System.Drawing.Size(72, 23);
			this.toolStripLabel2.Text = "内製管理No";
			// 
			// toolStrip_seba
			// 
			this.toolStrip_seba.Name = "toolStrip_seba";
			this.toolStrip_seba.Size = new System.Drawing.Size(50, 26);
			this.toolStrip_seba.TextChanged += new System.EventHandler(this.ToolStrip_seba_TextChanged);
			// 
			// toolStrip_seba2
			// 
			this.toolStrip_seba2.AutoSize = false;
			this.toolStrip_seba2.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F);
			this.toolStrip_seba2.Name = "toolStrip_seba2";
			this.toolStrip_seba2.Size = new System.Drawing.Size(12, 20);
			// 
			// toolStripSeparator10
			// 
			this.toolStripSeparator10.Name = "toolStripSeparator10";
			this.toolStripSeparator10.Size = new System.Drawing.Size(6, 26);
			// 
			// toolStripLabel3
			// 
			this.toolStripLabel3.Name = "toolStripLabel3";
			this.toolStripLabel3.Size = new System.Drawing.Size(56, 23);
			this.toolStripLabel3.Text = "加工機名";
			// 
			// toolStrip_machn
			// 
			this.toolStrip_machn.MaxDropDownItems = 20;
			this.toolStrip_machn.Name = "toolStrip_machn";
			this.toolStrip_machn.Size = new System.Drawing.Size(110, 26);
			this.toolStrip_machn.TextChanged += new System.EventHandler(this.ToolStrip_machn_TextChanged);
			// 
			// toolStripSeparator15
			// 
			this.toolStripSeparator15.Name = "toolStripSeparator15";
			this.toolStripSeparator15.Size = new System.Drawing.Size(6, 26);
			// 
			// toolStripLabel5
			// 
			this.toolStripLabel5.Name = "toolStripLabel5";
			this.toolStripLabel5.Size = new System.Drawing.Size(68, 23);
			this.toolStripLabel5.Text = "加工予定日";
			// 
			// toolStrip_kdate
			// 
			this.toolStrip_kdate.BackColor = System.Drawing.SystemColors.Window;
			this.toolStrip_kdate.Name = "toolStrip_kdate";
			this.toolStrip_kdate.ReadOnly = true;
			this.toolStrip_kdate.Size = new System.Drawing.Size(73, 26);
			this.toolStrip_kdate.Click += new System.EventHandler(this.ToolStrip_kdate_Click);
			this.toolStrip_kdate.TextChanged += new System.EventHandler(this.ToolStrip_kdate_TextChanged);
			// 
			// dataGridView1
			// 
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(0, 51);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.RowTemplate.Height = 21;
			this.dataGridView1.Size = new System.Drawing.Size(801, 349);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1_CellValueChanged);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Location = new System.Drawing.Point(0, 400);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(801, 22);
			this.statusStrip1.TabIndex = 3;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStrip2
			// 
			this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新規作成NToolStripButton,
            this.開くOToolStripButton,
            this.上書き保存SToolStripButton,
            this.印刷PToolStripButton1,
            this.toolStripSeparator12,
            this.toolStripButton_Insert,
            this.toolStripButton_Remove,
            this.toolStripButton_Up,
            this.toolStripButton_Dn,
            this.コピーCToolStripButton1,
            this.貼り付けPToolStripButton1,
            this.toolStripSeparator13,
            this.ヘルプLToolStripButton1,
            this.toolStripSeparator11,
            this.toolStripButton_srchNC,
            this.toolStrip_SFSet,
            this.toolStrip_Taore,
            this.toolStripSeparator16,
            this.toolStripDropDownButton1});
			this.toolStrip2.Location = new System.Drawing.Point(0, 0);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Size = new System.Drawing.Size(801, 25);
			this.toolStrip2.TabIndex = 4;
			this.toolStrip2.Text = "toolStrip2";
			// 
			// 新規作成NToolStripButton
			// 
			this.新規作成NToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.新規作成NToolStripButton.Enabled = false;
			this.新規作成NToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("新規作成NToolStripButton.Image")));
			this.新規作成NToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.新規作成NToolStripButton.Name = "新規作成NToolStripButton";
			this.新規作成NToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.新規作成NToolStripButton.Text = "新規作成(&N)";
			// 
			// 開くOToolStripButton
			// 
			this.開くOToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.開くOToolStripButton.Enabled = false;
			this.開くOToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("開くOToolStripButton.Image")));
			this.開くOToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.開くOToolStripButton.Name = "開くOToolStripButton";
			this.開くOToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.開くOToolStripButton.Text = "開く(&O)";
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
			// 印刷PToolStripButton1
			// 
			this.印刷PToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.印刷PToolStripButton1.Enabled = false;
			this.印刷PToolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("印刷PToolStripButton1.Image")));
			this.印刷PToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.印刷PToolStripButton1.Name = "印刷PToolStripButton1";
			this.印刷PToolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.印刷PToolStripButton1.Text = "印刷(&P)";
			// 
			// toolStripSeparator12
			// 
			this.toolStripSeparator12.Name = "toolStripSeparator12";
			this.toolStripSeparator12.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButton_Insert
			// 
			this.toolStripButton_Insert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton_Insert.Image = global::NcTejun.Properties.Resources.行の挿入;
			this.toolStripButton_Insert.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton_Insert.Name = "toolStripButton_Insert";
			this.toolStripButton_Insert.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton_Insert.Text = "挿入";
			this.toolStripButton_Insert.ToolTipText = "選択行の前に空の行を挿入します";
			this.toolStripButton_Insert.Click += new System.EventHandler(this.ToolStripButton_Insert_Click);
			// 
			// toolStripButton_Remove
			// 
			this.toolStripButton_Remove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton_Remove.Image = global::NcTejun.Properties.Resources.行の削除;
			this.toolStripButton_Remove.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton_Remove.Name = "toolStripButton_Remove";
			this.toolStripButton_Remove.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton_Remove.Text = "切り取り(&U)";
			this.toolStripButton_Remove.ToolTipText = "選択行を切り取ります";
			this.toolStripButton_Remove.Click += new System.EventHandler(this.ToolStripButton_Remove_Click);
			// 
			// toolStripButton_Up
			// 
			this.toolStripButton_Up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton_Up.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_Up.Image")));
			this.toolStripButton_Up.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton_Up.Name = "toolStripButton_Up";
			this.toolStripButton_Up.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton_Up.Text = "toolStripButton1";
			this.toolStripButton_Up.ToolTipText = "選択行を上に移動します";
			this.toolStripButton_Up.Click += new System.EventHandler(this.ToolStripButton_Up_Click);
			// 
			// toolStripButton_Dn
			// 
			this.toolStripButton_Dn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton_Dn.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_Dn.Image")));
			this.toolStripButton_Dn.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton_Dn.Name = "toolStripButton_Dn";
			this.toolStripButton_Dn.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton_Dn.Text = "toolStripButton2";
			this.toolStripButton_Dn.ToolTipText = "選択行を下に移動します";
			this.toolStripButton_Dn.Click += new System.EventHandler(this.ToolStripButton_Down_Click);
			// 
			// コピーCToolStripButton1
			// 
			this.コピーCToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.コピーCToolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("コピーCToolStripButton1.Image")));
			this.コピーCToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.コピーCToolStripButton1.Name = "コピーCToolStripButton1";
			this.コピーCToolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.コピーCToolStripButton1.Text = "コピー(&C)";
			this.コピーCToolStripButton1.ToolTipText = "行のコピー(C)";
			this.コピーCToolStripButton1.Click += new System.EventHandler(this.コピーCToolStripButton1_Click);
			// 
			// 貼り付けPToolStripButton1
			// 
			this.貼り付けPToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.貼り付けPToolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("貼り付けPToolStripButton1.Image")));
			this.貼り付けPToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.貼り付けPToolStripButton1.Name = "貼り付けPToolStripButton1";
			this.貼り付けPToolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.貼り付けPToolStripButton1.Text = "貼り付け(&P)";
			this.貼り付けPToolStripButton1.ToolTipText = "行の貼り付け(P)";
			this.貼り付けPToolStripButton1.Click += new System.EventHandler(this.貼り付けPToolStripButton1_Click);
			// 
			// toolStripSeparator13
			// 
			this.toolStripSeparator13.Name = "toolStripSeparator13";
			this.toolStripSeparator13.Size = new System.Drawing.Size(6, 25);
			// 
			// ヘルプLToolStripButton1
			// 
			this.ヘルプLToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ヘルプLToolStripButton1.Enabled = false;
			this.ヘルプLToolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("ヘルプLToolStripButton1.Image")));
			this.ヘルプLToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ヘルプLToolStripButton1.Name = "ヘルプLToolStripButton1";
			this.ヘルプLToolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.ヘルプLToolStripButton1.Text = "ヘルプ(&L)";
			// 
			// toolStripSeparator11
			// 
			this.toolStripSeparator11.Name = "toolStripSeparator11";
			this.toolStripSeparator11.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButton_srchNC
			// 
			this.toolStripButton_srchNC.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripButton_srchNC.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_srchNC.Image")));
			this.toolStripButton_srchNC.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton_srchNC.Name = "toolStripButton_srchNC";
			this.toolStripButton_srchNC.Size = new System.Drawing.Size(132, 22);
			this.toolStripButton_srchNC.Text = "ＮＣデータ検索・挿入";
			this.toolStripButton_srchNC.ToolTipText = "ＮＣデータを検索し結果を手順書に挿入します";
			this.toolStripButton_srchNC.Click += new System.EventHandler(this.ToolStripButton_srchNC_Click);
			// 
			// toolStrip_SFSet
			// 
			this.toolStrip_SFSet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStrip_SFSet.Enabled = false;
			this.toolStrip_SFSet.Image = ((System.Drawing.Image)(resources.GetObject("toolStrip_SFSet.Image")));
			this.toolStrip_SFSet.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStrip_SFSet.Name = "toolStrip_SFSet";
			this.toolStrip_SFSet.Size = new System.Drawing.Size(99, 22);
			this.toolStrip_SFSet.Text = "工具単位SF設定";
			this.toolStrip_SFSet.ToolTipText = "工具ごとに寿命長・回転数・送り速度を設定します";
			this.toolStrip_SFSet.Click += new System.EventHandler(this.ToolStrip_SFSet_Click);
			// 
			// toolStrip_Taore
			// 
			this.toolStrip_Taore.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStrip_Taore.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStrip_TaoreL,
            this.toolStrip_TaoreZ,
            this.toolStripButton3});
			this.toolStrip_Taore.Enabled = false;
			this.toolStrip_Taore.Image = ((System.Drawing.Image)(resources.GetObject("toolStrip_Taore.Image")));
			this.toolStrip_Taore.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStrip_Taore.Name = "toolStrip_Taore";
			this.toolStrip_Taore.Size = new System.Drawing.Size(81, 22);
			this.toolStrip_Taore.Text = "倒れ式など";
			this.toolStrip_Taore.ToolTipText = "倒れ式など";
			// 
			// toolStrip_TaoreL
			// 
			this.toolStrip_TaoreL.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStrip_TaoreL.Image = ((System.Drawing.Image)(resources.GetObject("toolStrip_TaoreL.Image")));
			this.toolStrip_TaoreL.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStrip_TaoreL.Name = "toolStrip_TaoreL";
			this.toolStrip_TaoreL.Size = new System.Drawing.Size(84, 22);
			this.toolStrip_TaoreL.Text = "Ｌの補正入力";
			this.toolStrip_TaoreL.ToolTipText = "倒れと隅取りペンシル加工を工具長Ｌで補正する";
			this.toolStrip_TaoreL.Click += new System.EventHandler(this.ToolStrip_HoseiL_Click);
			// 
			// toolStrip_TaoreZ
			// 
			this.toolStrip_TaoreZ.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStrip_TaoreZ.Image = ((System.Drawing.Image)(resources.GetObject("toolStrip_TaoreZ.Image")));
			this.toolStrip_TaoreZ.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStrip_TaoreZ.Name = "toolStrip_TaoreZ";
			this.toolStrip_TaoreZ.Size = new System.Drawing.Size(84, 22);
			this.toolStrip_TaoreZ.Text = "Ｚの補正入力";
			this.toolStrip_TaoreZ.ToolTipText = "倒れと隅取りペンシル加工をＮＣデータＺ移動で補正する";
			this.toolStrip_TaoreZ.Click += new System.EventHandler(this.ToolStrip_HoseiZ_Click);
			// 
			// toolStripButton3
			// 
			this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
			this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton3.Name = "toolStripButton3";
			this.toolStripButton3.Size = new System.Drawing.Size(96, 22);
			this.toolStripButton3.Text = "送り減速の設定";
			this.toolStripButton3.ToolTipText = "送り減速の設定";
			this.toolStripButton3.Click += new System.EventHandler(this.ToolStrip_PUPZ_Click);
			// 
			// toolStripSeparator16
			// 
			this.toolStripSeparator16.Name = "toolStripSeparator16";
			this.toolStripSeparator16.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripDropDownButton1
			// 
			this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Shokichi});
			this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
			this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
			this.toolStripDropDownButton1.Size = new System.Drawing.Size(93, 22);
			this.toolStripDropDownButton1.Text = "カスタマイズ";
			// 
			// toolStripMenuItem_Shokichi
			// 
			this.toolStripMenuItem_Shokichi.Name = "toolStripMenuItem_Shokichi";
			this.toolStripMenuItem_Shokichi.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItem_Shokichi.Text = "手順書初期値登録(個人別)";
			this.toolStripMenuItem_Shokichi.Click += new System.EventHandler(this.ToolStripMenuItem_Shokichi_Click);
			// 
			// FormTejun
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(801, 422);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.toolStrip2);
			this.Controls.Add(this.statusStrip1);
			this.Name = "FormTejun";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "FormTejun";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTejun_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormTejun_FormClosed);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.toolStrip2.ResumeLayout(false);
			this.toolStrip2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
		private System.Windows.Forms.ToolStripLabel toolStripLabel2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
		private System.Windows.Forms.ToolStripLabel toolStripLabel3;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStripButton 新規作成NToolStripButton;
		private System.Windows.Forms.ToolStripButton 開くOToolStripButton;
		private System.Windows.Forms.ToolStripButton 上書き保存SToolStripButton;
		private System.Windows.Forms.ToolStripButton 印刷PToolStripButton1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
		private System.Windows.Forms.ToolStripButton toolStripButton_Insert;
		private System.Windows.Forms.ToolStripButton toolStripButton_Remove;
		private System.Windows.Forms.ToolStripButton コピーCToolStripButton1;
		private System.Windows.Forms.ToolStripButton 貼り付けPToolStripButton1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
		private System.Windows.Forms.ToolStripButton ヘルプLToolStripButton1;
		private System.Windows.Forms.ToolStripLabel toolStripLabel4;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton toolStripButton_srchNC;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
		private System.Windows.Forms.ToolStripButton toolStripButton_Up;
		private System.Windows.Forms.ToolStripButton toolStripButton_Dn;
		private System.Windows.Forms.ToolStripButton toolStrip_SFSet;
		private System.Windows.Forms.ToolStripButton CommittoolStripButton;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.ToolStripTextBox toolStrip_user;
		private System.Windows.Forms.ToolStripComboBox toolStrip_machn;
		private System.Windows.Forms.ToolStripTextBox toolStrip_seba;
		private System.Windows.Forms.ToolStripTextBox toolStrip_Tname;
		private System.Windows.Forms.ToolStripLabel toolStripLabel5;
		private System.Windows.Forms.ToolStripTextBox toolStrip_kdate;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
		private System.Windows.Forms.ToolStripComboBox toolStrip_seba2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
		private System.Windows.Forms.ToolStripDropDownButton toolStrip_Taore;
		private System.Windows.Forms.ToolStripButton toolStrip_TaoreL;
		private System.Windows.Forms.ToolStripButton toolStrip_TaoreZ;
		private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Shokichi;
		private System.Windows.Forms.ToolStripButton toolStripButton3;
	}
}