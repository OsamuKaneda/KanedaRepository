using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using System.IO;
using CamUtil.CamNcD;


namespace NCSEND2
{
	static class LView
	{
		/// <summary></summary>
		private readonly struct MList
		{
			public readonly string text;
			public readonly int Width;
			public readonly HorizontalAlignment Alignment;
			public readonly string icon;
			public MList(string text, int width, HorizontalAlignment alignment, string icon) {
				this.text = text;
				this.Width = width;
				this.Alignment = alignment;
				this.icon = icon;
			}
		}

		// /////////////////////////////////////////
		// リストビューに表示する情報
		// この順序で表示する（コラムヘッダーと同時に順序入れ替え可能）
		// /////////////////////////////////////////
		private static readonly int lV_MAX = (int)LV.LVMAX;
		private static readonly int lT_MAX = (int)LT.LTMAX;
		private enum LV : int
		{
			CHECK,  // 送信
			SMODE,  // 状態
			FNAME,  // ファイル名
			//OUTNM,  // 出力名
			LMODI,  // 更新日時
			FSIZE,  // サイズ
			TCCNT,  // 工具交換数
			CLNUM,  // ＣＬ数
			SNAME,  // セット名
			TNAME,  // 工具名
			TTYPE,  // 工具種類
			TULEN,  // 突出し量
			HNAME,  // ホルダー名
			ZPICK,  //Ｚピック
			XPICK,  // XYピック
			NOKOS,  // 残し量
			TOLER,	// トレランス
			NCTIM,	// 加工時間
			SPIND,	// 回転数
			FEEDR,	// 送り速度
			NCLEN,	// 加工長（単位変更 mm→m 2008/01/22）
			// 2006.04.01 add by Fujimoto
			TLIFE,	// 工具寿命
			// 2007.09.24 add by Fujimoto
			TBUNK,	// 工具分割数
			//
			TTYPB,	// ケーラム工具種類
			SPINB,	// 回転数（ベース）
			FEEDB,	// 送り速度（ベース）
			COOLB,	// クーラント
			LVMAX
		}
		// /////////////////////////////////////////
		// リストビューに表示する情報（工具単位）
		// この順序で表示する（コラムヘッダーと同時に順序入れ替え可能）
		// /////////////////////////////////////////
		private enum LT : int
		{
			FNAME,  // ファイル名
			CLNUM,  // ＣＬ数
			SNAME,  // セット名
			TNAME,  // 工具名
			TTYPE,  // 工具種類
			TULEN,  // 突出し量
			HNAME,  // ホルダー名
			ZPICK,  //Ｚピック
			XPICK,  // XYピック
			NOKOS,  // 残し量
			TOLER,  // トレランス
			NCTIM,  // 加工時間
			SPIND,  // 回転数
			FEEDR,  // 送り速度
			NCLEN,  // 加工長（単位変更 mm→m 2008/01/22）
			// 2006.04.01 add by Fujimoto
			TLIFE,	// 工具寿命
			// 2007.09.24 add by Fujimoto
			TBUNK,	// 工具分割数
			//
			TTYPB,  // ケーラム工具種類
			SPINB,  // 回転数（ベース）
			FEEDB,  // 送り速度（ベース）
			COOLB,	// クーラント
			LTMAX
		}

		// ///////////////////////////////////////////
		// コラムヘッダーのセット（ＮＣデータ単位リスト）
		// ///////////////////////////////////////////
		public static void ColumnHeaderSet(ListView lView1) {
			ListView.ColumnHeaderCollection clmHeader = new ListView.ColumnHeaderCollection(lView1);
			System.Collections.ArrayList MListV = new System.Collections.ArrayList();

			//MessageBox.Show("Array=" + MListV.Count);
			//MessageBox.Show("SNAME=" + LV.SNAME.ToString());

			//MListV.Insert((int)LV.CHECK, new MList("送信",             300, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LV.CHECK, new MList("送信", 1200, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LV.SMODE, new MList("状態", 750, HorizontalAlignment.Center, ""));
			MListV.Insert((int)LV.FNAME, new MList("ファイル名", 30, HorizontalAlignment.Left, "arwup"));
			//MListV.Insert((int)LV.OUTNM, new MList("出力名",             0, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LV.LMODI, new MList("更新日時", 1750, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LV.FSIZE, new MList("サイズ", 30, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.TCCNT, new MList("工具数", 30, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.CLNUM, new MList("ＣＬ数", 30, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.SNAME, new MList("セット名", 30, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LV.TNAME, new MList("工具名", 1400, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LV.TTYPE, new MList("工具種類", 30, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LV.TULEN, new MList("突出し量", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.HNAME, new MList("ホルダー名", 1300, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LV.ZPICK, new MList("Ｚピック", 800, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.XPICK, new MList("XYピック", 800, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.NOKOS, new MList("残し量", 800, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.TOLER, new MList("トレランス", 30, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.NCTIM, new MList("加工時間min", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.SPIND, new MList("回転数", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.FEEDR, new MList("送り速度", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.NCLEN, new MList("加工長m", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.TLIFE, new MList("工具寿命m", 1050, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.TBUNK, new MList("工具本数", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.TTYPB, new MList("ケーラム工具種類", 30, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LV.SPINB, new MList("回転数（ベース）", 30, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.FEEDB, new MList("送り速度（ベース）", 30, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LV.COOLB, new MList("クーラント", 30, HorizontalAlignment.Left, ""));

			//lV_MAX = MListV.Count;

			foreach (object oo in MListV)
				clmHeader.Add(((MList)oo).text, ((MList)oo).Width / 15, ((MList)oo).Alignment);
			return;
		}
		// ///////////////////////////////////////////
		// コラムヘッダーのセット（工具単位リスト）
		// ///////////////////////////////////////////
		public static void ColumnHeaderSet2(ListView lView1) {
			ListView.ColumnHeaderCollection clmHeader = new ListView.ColumnHeaderCollection(lView1);
			System.Collections.ArrayList MListV = new System.Collections.ArrayList();

			MListV.Insert((int)LT.FNAME, new MList("ファイル名", 1300, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.CLNUM, new MList("ＣＬ数", 30, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.SNAME, new MList("セット名", 30, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LT.TNAME, new MList("工具名", 1400, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LT.TTYPE, new MList("工具種類", 30, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LT.TULEN, new MList("突出し量", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.HNAME, new MList("ホルダー名", 1300, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LT.ZPICK, new MList("Ｚピック", 800, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.XPICK, new MList("XYピック", 800, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.NOKOS, new MList("残し量", 800, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.TOLER, new MList("トレランス", 30, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.NCTIM, new MList("加工時間min", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.SPIND, new MList("回転数", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.FEEDR, new MList("送り速度", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.NCLEN, new MList("加工長m", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.TLIFE, new MList("工具寿命m", 1050, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.TBUNK, new MList("工具本数", 1000, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.TTYPB, new MList("ケーラム工具種類", 30, HorizontalAlignment.Left, ""));
			MListV.Insert((int)LT.SPINB, new MList("回転数（ベース）", 30, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.FEEDB, new MList("送り速度（ベース）", 30, HorizontalAlignment.Right, ""));
			MListV.Insert((int)LT.COOLB, new MList("クーラント", 30, HorizontalAlignment.Left, ""));

			//lT_MAX = MListV.Count;

			foreach (object oo in MListV)
				clmHeader.Add(((MList)oo).text, ((MList)oo).Width / 15, ((MList)oo).Alignment);
			return;
		}

		// //////////////////////////////////
		// リストビューアイテム（ファイル単位）
		// //////////////////////////////////
		public class ListViewItemFile : ListViewItem
		{
			private System.Collections.ArrayList limArray =
				new System.Collections.ArrayList();

			// コンストラクタ
			public ListViewItemFile(NCINFO.NcInfoCam ncd) {
				// Name は Items のインデックスとして使用されるため変更不可
				this.Name = Path.GetFileNameWithoutExtension(ncd.FullNameCam);

				if (ncd.OutName != null)
					this.Text = ncd.OutName;
				this.UseItemStyleForSubItems = false;
				this.Tag = NCINFO.NcInfoCam.LVM.未設定;

				for (int jj = 1; jj <= LView.lV_MAX; jj++)
					this.SubItems.Add("");

				this.SubItems[(int)LV.TCCNT].Text = ncd.ToolCount.ToString();
				this.SubItems[(int)LV.LMODI].Text = ncd.LastWriteTime.ToString();
				this.SubItems[(int)LV.FSIZE].Text = ncd.FSize.ToString();

				// ソートに用いるためＣＳＶデータがエラーでも"FNAME"は必要
				this.SubItems[(int)LV.FNAME].Text =
					Path.GetFileNameWithoutExtension(ncd.FullNameCam);

				if (ncd.ToolCount <= 0)
					return;
				if (ncd.xmlD == null)
					return;

				// //////////////////////////////////////
				// 工具単位のリストビューアイテムの作成
				// //////////////////////////////////////
				for (int tcnt = 0; tcnt < ncd.ToolCount; tcnt++)
					limArray.Add(new ListViewItemTool(tcnt, ncd));

				if (ncd.ToolCount != 1) {
					double clnum = 0.0;
					double nclen = 0.0;
					double nctim = 0.0;
					for (int ii = 0; ii < limArray.Count; ii++) {
						clnum += Convert.ToDouble(((ListViewItem)limArray[ii]).SubItems[(int)LT.CLNUM].Text);
						nclen += Convert.ToDouble(((ListViewItem)limArray[ii]).SubItems[(int)LT.NCLEN].Text);
						nctim += Convert.ToDouble(((ListViewItem)limArray[ii]).SubItems[(int)LT.NCTIM].Text);
					}
					this.SubItems[(int)LV.CLNUM].Text = clnum.ToString();
					this.SubItems[(int)LV.NCLEN].Text = nclen.ToString();
					this.SubItems[(int)LV.NCTIM].Text = nctim.ToString();
				}
				else {
					this.SubItems[(int)LV.CLNUM].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.CLNUM].Text;
					this.SubItems[(int)LV.NCLEN].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.NCLEN].Text;
					this.SubItems[(int)LV.NCTIM].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.NCTIM].Text;

					this.SubItems[(int)LV.SNAME].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.SNAME].Text;
					this.SubItems[(int)LV.TNAME].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.TNAME].Text;
					this.SubItems[(int)LV.TTYPE].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.TTYPE].Text;
					this.SubItems[(int)LV.TULEN].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.TULEN].Text;
					this.SubItems[(int)LV.HNAME].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.HNAME].Text;
					this.SubItems[(int)LV.ZPICK].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.ZPICK].Text;
					this.SubItems[(int)LV.XPICK].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.XPICK].Text;
					this.SubItems[(int)LV.NOKOS].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.NOKOS].Text;
					this.SubItems[(int)LV.TOLER].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.TOLER].Text;
					this.SubItems[(int)LV.SPIND].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.SPIND].Text;
					this.SubItems[(int)LV.FEEDR].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.FEEDR].Text;
					this.SubItems[(int)LV.TTYPB].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.TTYPB].Text;
					this.SubItems[(int)LV.SPINB].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.SPINB].Text;
					this.SubItems[(int)LV.FEEDB].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.FEEDB].Text;
					this.SubItems[(int)LV.COOLB].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.COOLB].Text;
					this.SubItems[(int)LV.TLIFE].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.TLIFE].Text;
					this.SubItems[(int)LV.TBUNK].Text = ((ListViewItem)limArray[0]).SubItems[(int)LT.TBUNK].Text;
				}
			}
			public void LVModeSet(NCINFO.NcInfoCam ncd) {
				this.Tag = ncd.SMOD0;
				this.ToolTipText = ncd.ERRM0;

				switch ((NCINFO.NcInfoCam.LVM)this.Tag) {
				case NCINFO.NcInfoCam.LVM.未送信:
					this.Checked = true;
					this.SubItems[(int)LV.SMODE].Text = "未送信";
					this.SubItems[(int)LV.SMODE].ForeColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x00, 0xFF);
					break;
				case NCINFO.NcInfoCam.LVM.送信済:
					this.Checked = false;
					this.SubItems[(int)LV.SMODE].Text = "送信済";
					this.SubItems[(int)LV.SMODE].ForeColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
					break;
				case NCINFO.NcInfoCam.LVM.再送要:
					this.Checked = true;
					this.SubItems[(int)LV.SMODE].Text = "再送要";
					this.SubItems[(int)LV.SMODE].ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0x00, 0xFF);
					break;
				case NCINFO.NcInfoCam.LVM.未確認:
					this.Checked = false;
					this.SubItems[(int)LV.SMODE].Text = "未確認";
					this.SubItems[(int)LV.SMODE].ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
					break;
				case NCINFO.NcInfoCam.LVM.出力名:
					this.Checked = false;
					this.SubItems[(int)LV.SMODE].Text = "出力名";
					this.SubItems[(int)LV.SMODE].ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
					break;
				case NCINFO.NcInfoCam.LVM.エラー:
					this.Checked = false;
					this.SubItems[(int)LV.SMODE].Text = "エラー";
					this.SubItems[(int)LV.SMODE].ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
					break;
				default:
					this.Checked = false;
					this.SubItems[(int)LV.SMODE].Text = "未定義";
					this.SubItems[(int)LV.SMODE].ForeColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
					break;
				}
			}
			public void LVNcLenSet(NCINFO.NcInfoCam ncd) {
				double nclen, nctim;
				double nclenSum = 0.0, nctimSum = 0.0;
				string tlbun = "";

				for (int jj = 0; jj < ncd.ToolCount; jj++) {
					nclen = ncd.xmlD[jj].NCLEN / 1000.0;
					nctim = ncd.xmlD[jj].NCTIM;
					tlbun = "";
					if (ncd.sqlDB[jj].LifeMaxBase > 0.01)
						tlbun = (ncd.xmlD[jj].NCLEN / ncd.sqlDB[jj].LifeMaxBase / 1000).ToString("0.00");
					((ListViewItem)this.limArray[jj]).SubItems[(int)LT.TULEN].Text = ncd.xmlD[jj].TULEN.ToString("##0");
					((ListViewItem)this.limArray[jj]).SubItems[(int)LT.NCLEN].Text = nclen.ToString("F2");
					((ListViewItem)this.limArray[jj]).SubItems[(int)LT.NCTIM].Text = nctim.ToString("F2");
					((ListViewItem)this.limArray[jj]).SubItems[(int)LT.TBUNK].Text = tlbun;
					nclenSum += nclen;
					nctimSum += nctim;
				}
				this.SubItems[(int)LV.NCLEN].Text = nclenSum.ToString("F2");
				this.SubItems[(int)LV.NCTIM].Text = nctimSum.ToString("F2");
				if (ncd.ToolCount == 1) {
					this.SubItems[(int)LV.TULEN].Text = ncd.xmlD[0].TULEN.ToString("##0");
					this.SubItems[(int)LV.TBUNK].Text = tlbun;
				}
			}
		}

		// //////////////////////////////
		// リストビューアイテム（工具単位）
		// //////////////////////////////
		public class ListViewItemTool : ListViewItem
		{
			// コンストラクタ
			public ListViewItemTool(int tcnt, NCINFO.NcInfoCam ncd)
			{
				string BasName = Path.GetFileNameWithoutExtension(ncd.FullNameCam);
				if (ncd.xmlD != null)
					if (Path.GetFileNameWithoutExtension(ncd.FullNameCam) != ncd.xmlD.CamBaseName)
						throw new Exception("aerfwergfwberfbh");
				this.UseItemStyleForSubItems = false;
				//object otmp;

				// サブアイテム固有名（ファイル名）
				this.Name = "";
				this.Text = "";

				for (int jj = 1; jj <= lT_MAX; jj++)
					this.SubItems.Add("");

				this.SubItems[(int)LT.FNAME].Text = ncd.xmlD.CamDataName;
				this.SubItems[(int)LT.CLNUM].Text = "1";
				this.SubItems[(int)LT.SNAME].Text = ncd.xmlD[tcnt].SNAME;
				this.SubItems[(int)LT.TNAME].Text = ncd.sqlDB[tcnt].toolsetTemp.ToolName;
				double dtmp = Math.Ceiling(ncd.xmlD[tcnt].TULEN);
				this.SubItems[(int)LT.TULEN].Text = dtmp.ToString("##0");
				this.SubItems[(int)LT.HNAME].Text = ncd.sqlDB[tcnt].toolsetTemp.HolderName;

				this.SubItems[(int)LT.TTYPE].Text = ncd.xmlD[tcnt].TTYPE;
				this.SubItems[(int)LT.ZPICK].Text = ncd.xmlD[tcnt].ZPICK != null ? ncd.xmlD[tcnt].ZPICK.ToString() : "不定";
				this.SubItems[(int)LT.XPICK].Text = ncd.xmlD[tcnt].XPICK != null ? ncd.xmlD[tcnt].XPICK.ToString() : "不定";
				this.SubItems[(int)LT.NOKOS].Text = ncd.xmlD[tcnt].NOKOS != null ? ncd.xmlD[tcnt].NOKOS.ToString() : "不定";
				this.SubItems[(int)LT.TOLER].Text = ncd.xmlD[tcnt].TOLER != null ? ncd.xmlD[tcnt].TOLER.ToString() : "不定";

				this.SubItems[(int)LT.SPIND].Text = ncd.sqlDB[tcnt].Spin.ToString();
				this.SubItems[(int)LT.FEEDR].Text = ncd.sqlDB[tcnt].Feedrate.ToString();

				this.SubItems[(int)LT.NCLEN].Text = (ncd.xmlD[tcnt].NCLEN / 1000.0).ToString("F2");
				this.SubItems[(int)LT.NCTIM].Text = ncd.xmlD[tcnt].NCTIM.ToString("F2");

				this.SubItems[(int)LT.TTYPB].Text = ncd.sqlDB[tcnt].toolsetTemp.CutterTypeCaelum;
				this.SubItems[(int)LT.TLIFE].Text = ncd.sqlDB[tcnt].LifeMaxBase.ToString("F2");
				if (ncd.sqlDB[tcnt].LifeMaxBase > 0.01)
					// TNUNK（単位 本数）、NCLEN（単位 ｍｍ）、TLIFE（単位 ｍ）
					this.SubItems[(int)LT.TBUNK].Text =
						(ncd.xmlD[tcnt].NCLEN / ncd.sqlDB[tcnt].LifeMaxBase / 1000).ToString("0.00");
				else
					this.SubItems[(int)LT.TBUNK].Text = "";
			}
		}
	}
}
