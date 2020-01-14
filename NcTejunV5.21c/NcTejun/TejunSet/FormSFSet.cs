using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using CamUtil;

namespace NcTejun.TejunSet
{
	/// <summary>
	/// 工具寿命、回転数、送り速度を個別に設定する
	/// </summary>
	partial class FormSFSet : Form
	{
		private List<St_boxData> textBox;
		private struct St_boxData
		{
			public TextBox NCD;
			public TextBox TOL;
			public TextBox OrgL;
			public TextBox OrgS;
			public TextBox OrgF;
			public TextBox NowL;
			public TextBox NowS;
			public TextBox NowF;
			public ComboBox RatL;
			public ComboBox RatS;
			public ComboBox RatF;
			public St_boxData(int dummy) {
				NCD = new TextBox();
				TOL = new TextBox();
				OrgL = new TextBox();
				OrgS = new TextBox();
				OrgF = new TextBox();
				NowL = new TextBox();
				NowS = new TextBox();
				NowF = new TextBox();

				string[] items = new string[] { "0.500", "0.600", "0.700", "0.800", "0.900", "1.000", "1.100", "1.200", "1.300", "1.400", "1.500" };

				RatL = new ComboBox() { MaxDropDownItems = items.Length };
				RatL.Items.AddRange(items);

				RatS = new ComboBox() { MaxDropDownItems = items.Length };
				RatS.Items.AddRange(items);

				RatF = new ComboBox() { MaxDropDownItems = items.Length };
				RatF.Items.AddRange(items);
			}
		}

		internal FormSFSet(List<NcdTool.NcName.NcData> ndList)
		{
			InitializeComponent();

			textBox = new List<St_boxData>();

			bool not_all = false;
			bool saiteki = false;

			int ii = -1;
			foreach (NcdTool.NcName.NcData nd in ndList) {
				// ＸＭＬ情報がない場合
				if (nd.ncInfo == null) {
					not_all = true;
					continue;
				}

				foreach (NcdTool.NcName.NcDataT nt in nd.Tld) {
					// ＮＣＳＰＥＥＤ最適化の場合はできない
					if (nd.ncInfo.xmlD.SmNAM != null && nt.XmlT.SmFDC && nt.XmlT.OPTION() == false) {
						saiteki = true;
						continue;
					}
					ii++;
					textBox.Add(TbSet(ii, nd, nt));
				}
			}
			if (not_all) {
				MessageBox.Show(
					"NCSEND2で出力されていないＮＣデータ、古いバージョンのＮＣデータはリストから除きました。", "FormSFSet",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else if (saiteki) {
				MessageBox.Show(
					"ＮＣＳＰＥＥＤで最適化されたＮＣデータはリストから除きました。", "FormSFSet",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			if (textBox.Count == 0)
				button_OK.Enabled = false;

		}
		private St_boxData TbSet(int icnt, NcdTool.NcName.NcData nd, NcdTool.NcName.NcDataT nt)
		{
			int Y = 22 + 26 * icnt;

			St_boxData boxData = new St_boxData(0);

			Font font = new Font(
				"MS UI Gothic", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(128)));
			// 
			// textBox1（ＮＣデータ名）
			// 
			boxData.NCD.Tag = nd;
			boxData.NCD.Font = font;
			boxData.NCD.Location = new Point(label_NCD.Location.X + 3, Y);
			boxData.NCD.Name = "NCD";
			boxData.NCD.Size = new Size(label_NCD.Size.Width - 3, 20);
			boxData.NCD.TabStop = false;
			boxData.NCD.TabIndex = 0;
			boxData.NCD.Text = nd.nnam;
			boxData.NCD.ReadOnly = true;
			boxData.NCD.BackColor = Color.FromName("Control");
			if (nt.SetJun != 1) boxData.NCD.Visible = false;
			// 
			// textBox2（工具名）
			// 
			boxData.TOL.Tag = nt;
			boxData.TOL.Font = font;
			boxData.TOL.Location = new Point(label_TOOL.Location.X + 3, Y);
			boxData.TOL.Name = "TOL";
			boxData.TOL.Size = new Size(label_TOOL.Size.Width - 3, 20);
			boxData.TOL.TabStop = false;
			boxData.TOL.TabIndex = 0;
			boxData.TOL.Text = nt.XmlT.SNAME;
			boxData.TOL.ReadOnly = true;
			boxData.TOL.BackColor = Color.FromName("Control");
			// 
			// 初期値Ｌ
			// 
			boxData.OrgL.Font = font;
			boxData.OrgL.Location = new Point(label_OrgL.Location.X + 3, Y);
			boxData.OrgL.Name = "OrgL";
			boxData.OrgL.Size = new Size(label_OrgL.Size.Width - 3, 20);
			boxData.OrgL.TabStop = false;
			boxData.OrgL.TabIndex = 0;
			boxData.OrgL.TextAlign = HorizontalAlignment.Right;
			boxData.OrgL.Text = nt.LifeDB.ToString("0");

			boxData.OrgL.Tag = boxData.OrgL.Text;
			boxData.OrgL.ReadOnly = true;
			boxData.OrgL.BackColor = Color.FromName("Control");
			// 
			// 現在値Ｌ
			// 
			boxData.NowL.Tag = boxData;
			boxData.NowL.Font = font;
			boxData.NowL.Location = new Point(label_NowL.Location.X + 3, Y);
			boxData.NowL.Name = "NowL";
			boxData.NowL.Size = new Size(label_NowL.Size.Width - 3, 20);
			boxData.NowL.TabIndex = 1 + 3 * icnt;
			boxData.NowL.TextAlign = HorizontalAlignment.Right;
			boxData.NowL.TextChanged += new EventHandler(FormSFSet_TextChanged_L);
			// 
			// 比率Ｌ
			// 
			boxData.RatL.Tag = boxData;
			boxData.RatL.Font = font;
			boxData.RatL.Location = new Point(label_RatL.Location.X + 3, Y);
			boxData.RatL.Name = "RatL";
			boxData.RatL.Size = new Size(label_RatL.Size.Width - 3, 20);
			boxData.RatL.TabStop = false;
			boxData.RatL.TabIndex = 0;
			boxData.RatL.TextChanged += new System.EventHandler(FormSFSet_TextChanged_L);

			// 
			// 初期値Ｓ
			// 
			boxData.OrgS.Font = font;
			boxData.OrgS.Location = new Point(label_OrgS.Location.X + 3, Y);
			boxData.OrgS.Name = "OrgS";
			boxData.OrgS.Size = new Size(label_OrgS.Size.Width - 3, 20);
			boxData.OrgS.TabStop = false;
			boxData.OrgS.TabIndex = 0;
			boxData.OrgS.TextAlign = HorizontalAlignment.Right;
			// ADD in 2010/10/28
			if (nd.ncInfo.xmlD.SmNAM != null && nt.XmlT.SmFDC == false)
				boxData.OrgS.Text = (nt.XmlT.SPIND * nt.XmlT.SmSPR).ToString("0");
			else
				boxData.OrgS.Text = nt.XmlT.SPIND.ToString("0");

			boxData.OrgS.Tag = boxData.OrgS.Text;
			boxData.OrgS.ReadOnly = true;
			boxData.OrgS.BackColor = Color.FromName("Control");
			// 
			// 現在値Ｓ
			// 
			boxData.NowS.Tag = boxData;
			boxData.NowS.Font = font;
			boxData.NowS.Location = new Point(label_NowS.Location.X + 3, Y);
			boxData.NowS.Name = "NowS";
			boxData.NowS.Size = new Size(label_NowS.Size.Width - 3, 20);
			boxData.NowS.TabIndex = 2 + 3 * icnt;
			boxData.NowS.TextAlign = HorizontalAlignment.Right;
			boxData.NowS.TextChanged += new EventHandler(FormSFSet_TextChanged_S);
			//if (nt.nsgt1.ninniSetPC == false) {
			//		boxData.TB_es.ReadOnly = true;
			//		boxData.TB_es.BackColor = Color.FromName("Control");
			//	}
			// 
			// 比率Ｓ
			// 
			boxData.RatS.Tag = boxData;
			boxData.RatS.Font = font;
			boxData.RatS.Location = new Point(label_RatS.Location.X + 3, Y);
			boxData.RatS.Name = "RatS";
			boxData.RatS.Size = new Size(label_RatS.Size.Width - 3, 20);
			boxData.RatS.TabStop = false;
			boxData.RatS.TabIndex = 0;
			boxData.RatS.TextChanged += new System.EventHandler(FormSFSet_TextChanged_S);

			// 
			// 初期値Ｆ
			// 
			boxData.OrgF.Font = font;
			boxData.OrgF.Location = new Point(label_OrgF.Location.X + 3, Y);
			boxData.OrgF.Name = "OrgF";
			boxData.OrgF.Size = new Size(label_OrgF.Size.Width - 3, 20);
			boxData.OrgF.TabStop = false;
			boxData.OrgF.TabIndex = 0;
			boxData.OrgF.TextAlign = HorizontalAlignment.Right;
			// ADD in 2010/10/28
			if (nd.ncInfo.xmlD.SmNAM != null && nt.XmlT.SmFDC == false)
				boxData.OrgF.Text = (nt.XmlT.FEEDR * nt.XmlT.SmFDR).ToString("0");
			else
				boxData.OrgF.Text = nt.XmlT.FEEDR.ToString("0");

			boxData.OrgF.Tag = boxData.OrgF.Text;
			boxData.OrgF.ReadOnly = true;
			boxData.OrgF.BackColor = Color.FromName("Control");
			// 
			// 現在値Ｆ
			// 
			boxData.NowF.Tag = boxData;
			boxData.NowF.Font = font;
			boxData.NowF.Location = new Point(label_NowF.Location.X + 3, Y);
			boxData.NowF.Name = "NowF";
			boxData.NowF.Size = new Size(label_NowF.Size.Width - 3, 20);
			boxData.NowF.TabIndex = 3 + 3 * icnt;
			boxData.NowF.TextAlign = HorizontalAlignment.Right;
			boxData.NowF.TextChanged += new EventHandler(FormSFSet_TextChanged_F);
			//if (nt.nsgt1.ninniSetPC == false) {
			//		boxData.TB_ef.ReadOnly = true;
			//		boxData.TB_ef.BackColor = Color.FromName("Control");
			//	}
			// 
			// 比率Ｆ
			// 
			boxData.RatF.Tag = boxData;
			boxData.RatF.Font = font;
			boxData.RatF.Location = new Point(label_RatF.Location.X + 3, Y);
			boxData.RatF.Name = "RatF";
			boxData.RatF.Size = new Size(label_RatF.Size.Width - 3, 20);
			boxData.RatF.TabStop = false;
			boxData.RatF.TabIndex = 0;
			boxData.RatF.TextChanged += new System.EventHandler(FormSFSet_TextChanged_F);

			// データセット
			boxData.NowL.Text = nt.LninnXML.ToString("0");
			boxData.NowS.Text = nt.SninnXML.ToString("0");
			boxData.NowF.Text = nt.FninnXML.ToString("0");


			// チェック
			//if (boxData.TB_ss.Text != boxData.TB_es.Text || boxData.TB_sf.Text != boxData.TB_ef.Text) {
			//	MessageBox.Show("awfbqehfrbqh");
			//}

			this.panel1.Controls.Add(boxData.NCD);
			this.panel1.Controls.Add(boxData.TOL);
			this.panel1.Controls.Add(boxData.OrgL);
			this.panel1.Controls.Add(boxData.NowL);
			this.panel1.Controls.Add(boxData.RatL);
			this.panel1.Controls.Add(boxData.OrgS);
			this.panel1.Controls.Add(boxData.NowS);
			this.panel1.Controls.Add(boxData.RatS);
			this.panel1.Controls.Add(boxData.OrgF);
			this.panel1.Controls.Add(boxData.NowF);
			this.panel1.Controls.Add(boxData.RatF);

			return boxData;
		}




		// //////////////////
		// 以下はイベント処理
		// //////////////////

		/// <summary>
		/// 寿命長の現在値を変更した場合
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormSFSet_TextChanged_L(object sender, EventArgs e) {
			St_boxData bd;
			int org, now;
			double rat;

			if (((Control)sender).Text.Length == 0) {
				((Control)sender).BackColor = Color.OrangeRed;
				return;
			}
			if (((Control)sender).Text.Span("0123456789.") != ((Control)sender).Text.Length) {
				((Control)sender).BackColor = Color.OrangeRed;
				return;
			}

			bd = (St_boxData)((Control)sender).Tag;
			org = Convert.ToInt32(bd.OrgL.Tag);
			if (((Control)sender).Name == "NowL") {
				if (((TextBox)sender).Text.IndexOf('.') >= 0) {
					((TextBox)sender).BackColor = Color.OrangeRed;
					return;
				}
				now = Convert.ToInt32(((TextBox)sender).Text);
				rat = RateSet(now, org);
				bd.RatL.TextChanged -= new System.EventHandler(FormSFSet_TextChanged_L);
				bd.RatL.Text = (org == now) ? "" : rat.ToString("0.000");
				bd.RatL.TextChanged += new System.EventHandler(FormSFSet_TextChanged_L);
			}
			else if (((Control)sender).Name == "RatL") {
				rat = Convert.ToDouble(((ComboBox)sender).Text);
				now = (int)Math.Round(org * rat);
				bd.NowL.TextChanged -= new System.EventHandler(FormSFSet_TextChanged_L);
				bd.NowL.Text = now.ToString();
				bd.NowL.TextChanged += new System.EventHandler(FormSFSet_TextChanged_L);
			}
			else
				throw new Exception("qwejfbqerbf");
			if (org == now)
				bd.NowL.BackColor = Color.White;
			else
				bd.NowL.BackColor = Color.Bisque;
		}
		/// <summary>
		/// 回転数の現在値を変更した場合
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormSFSet_TextChanged_S(object sender, EventArgs e) {
			St_boxData bd;
			int org, now;
			double rat;

			if (((Control)sender).Name == "NowS") {
				if (((TextBox)sender).Text.Length == 0) {
					((TextBox)sender).BackColor = Color.OrangeRed;
					return;
				}
				if (((TextBox)sender).Text.Span("0123456789") != ((TextBox)sender).Text.Length) {
					((TextBox)sender).BackColor = Color.OrangeRed;
					return;
				}
				bd = (St_boxData)((TextBox)sender).Tag;
				org = Convert.ToInt32(bd.OrgS.Tag);
				now = Convert.ToInt32(((TextBox)sender).Text);
				rat = RateSet(now, org);
				bd.RatS.TextChanged -= new System.EventHandler(FormSFSet_TextChanged_S);
				bd.RatS.Text = NotNinni(bd) ? "" : rat.ToString("0.000");
				bd.RatS.TextChanged += new System.EventHandler(FormSFSet_TextChanged_S);
			}
			else if (((Control)sender).Name == "RatS") {
				if (((ComboBox)sender).Text.Length == 0) {
					((ComboBox)sender).BackColor = Color.OrangeRed;
					return;
				}
				if (((ComboBox)sender).Text.Span("0123456789.") != ((ComboBox)sender).Text.Length) {
					((ComboBox)sender).BackColor = Color.OrangeRed;
					return;
				}
				bd = (St_boxData)((ComboBox)sender).Tag;
				org = Convert.ToInt32(bd.OrgS.Tag);
				rat = Convert.ToDouble(((ComboBox)sender).Text);
				now = (int)Math.Round(org * rat);
				bd.NowS.TextChanged -= new System.EventHandler(FormSFSet_TextChanged_S);
				bd.NowS.Text = now.ToString();
				bd.NowS.TextChanged += new System.EventHandler(FormSFSet_TextChanged_S);
			}
			else {
				bd = new St_boxData();
				throw new Exception("qwejfbqerbf");
			}
			if (NotNinni(bd)) {
				bd.NowS.BackColor = Color.White;
				bd.NowF.BackColor = Color.White;
			}
			else {
				bd.NowS.BackColor = Color.Bisque;
				bd.NowF.BackColor = Color.Bisque;
			}
		}
		/// <summary>
		/// 送り速度の現在値を変更した場合
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FormSFSet_TextChanged_F(object sender, EventArgs e) {
			St_boxData bd;
			int org, now;
			double rat;

			if (((Control)sender).Name == "NowF") {
				if (((TextBox)sender).Text.Length == 0) {
					((TextBox)sender).BackColor = Color.OrangeRed;
					return;
				}
				if (((TextBox)sender).Text.Span("0123456789") != ((TextBox)sender).Text.Length) {
					((TextBox)sender).BackColor = Color.OrangeRed;
					return;
				}
				bd = (St_boxData)((TextBox)sender).Tag;
				org = Convert.ToInt32(bd.OrgF.Tag);
				now = Convert.ToInt32(((TextBox)sender).Text);
				rat = RateSet(now, org);
				bd.RatF.TextChanged -= new System.EventHandler(FormSFSet_TextChanged_F);
				bd.RatF.Text = NotNinni(bd) ? "" : rat.ToString("0.000");
				bd.RatF.TextChanged += new System.EventHandler(FormSFSet_TextChanged_F);
			}
			else if (((Control)sender).Name == "RatF") {
				if (((ComboBox)sender).Text.Length == 0) {
					((ComboBox)sender).BackColor = Color.OrangeRed;
					return;
				}
				if (((ComboBox)sender).Text.Span("0123456789.") != ((ComboBox)sender).Text.Length) {
					((ComboBox)sender).BackColor = Color.OrangeRed;
					return;
				}
				bd = (St_boxData)((ComboBox)sender).Tag;
				org = Convert.ToInt32(bd.OrgF.Tag);
				rat = Convert.ToDouble(((ComboBox)sender).Text);
				now = (int)Math.Round(org * rat);
				bd.NowF.TextChanged -= new System.EventHandler(FormSFSet_TextChanged_F);
				bd.NowF.Text = now.ToString();
				bd.NowF.TextChanged += new System.EventHandler(FormSFSet_TextChanged_F);
			}
			else {
				bd = new St_boxData();
				throw new Exception("qwejfbqerbf");
			}
			if (NotNinni(bd)) {
				bd.NowS.BackColor = Color.White;
				bd.NowF.BackColor = Color.White;
			}
			else {
				bd.NowS.BackColor = Color.Bisque;
				bd.NowF.BackColor = Color.Bisque;
			}
		}
		/// <summary>比率欄に表示する値を計算</summary>
		private double RateSet(int now, int org) {
			double rat;
			rat = Convert.ToDouble(now) / Convert.ToDouble(org);
			if ((int)Math.Round(org * Math.Round(rat, 1)) == now)
				rat = Math.Round(rat, 1);
			else
				rat = Math.Round(rat, 3);
			return rat;
		}
		/// <summary>任意指示ではない場合にtrue</summary>
		private bool NotNinni(St_boxData bd) {
			if (bd.NowS.Text != "" && Convert.ToInt32(bd.OrgS.Tag) != Convert.ToInt32(bd.NowS.Text))
				return false;
			if (bd.NowF.Text != "" && Convert.ToInt32(bd.OrgF.Tag) != Convert.ToInt32(bd.NowF.Text))
				return false;
			if ((bd.NowS.Text == "" || bd.NowF.Text == "") && ((NcdTool.NcName.NcDataT)bd.TOL.Tag).XmlT.OPTION() == true)
				return false;
			return true;
		}

		/// <summary>
		/// ＯＫによりデータを更新する
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_OK_Click(object sender, EventArgs e) {
			NcdTool.NcName.NcData nd;
			NcdTool.NcName.NcDataT nt;

			bool save = false;
			foreach (St_boxData tbArray in textBox) {
				nd = ((NcdTool.NcName.NcData)tbArray.NCD.Tag);
				nt = ((NcdTool.NcName.NcDataT)tbArray.TOL.Tag);

				// 回転数、送り速度のセット
				if (
				nt.SninnXML.ToString("0") != tbArray.NowS.Text ||
				nt.FninnXML.ToString("0") != tbArray.NowF.Text ||
				(nt.SninnXML.ToString("0") == tbArray.OrgS.Text && nt.FninnXML.ToString("0") == tbArray.OrgF.Text && nt.XmlT.OPTFD != null)) {
					// if文の最後の行は暫定処置（1.0のまま任指示の場合に実行して任指示を消去させる）
					save = true;
					nd.ncInfo.XmlEditSF(nt.SetJun,
						RoundN(Convert.ToInt32(tbArray.NowS.Text), nt.XmlT.SPIND),
						RoundN(Convert.ToInt32(tbArray.NowF.Text), nt.XmlT.FEEDR));
				}

				// 工具寿命長のセット
				if (nt.LninnXML.ToString("0") != tbArray.NowL.Text) {
					save = true;
					nd.ncInfo.XmlEditLife(nt.SetJun,
						RoundN(Convert.ToInt32(tbArray.NowL.Text), nt.LifeDB));
				}
				// ＸＭＬが修正された場合保存する
				if (nt.SetJun == nd.Tld.Length && save) {
					try { nd.ncInfo.XmlSave(System.IO.Path.ChangeExtension(nd.fulnamePC, ".xml")); }
					catch (Exception ex) {
						MessageBox.Show(ex.Message, "NcInfoCam", MessageBoxButtons.OK, MessageBoxIcon.Error);
						Application.Exit();
					}
					save = false;
				}
			}
		}
		/// <summary>
		/// 整数の商を分母との積の結果が元の分子と同一になる最小桁数＋１で表す
		/// </summary>
		/// <param name="bunsi">分子</param>
		/// <param name="bunbo">分母</param>
		/// <returns></returns>
		private double RoundN(int bunsi, double bunbo) {
			for (int ii = 0; ii < 20; ii++) {
				if (bunsi == (int)Math.Round(bunbo * Math.Round(bunsi / bunbo, ii)))
					return Math.Round(bunsi / bunbo, ii + 1);
			}
			throw new Exception("qrgfwbrhb");
		}

	}
}