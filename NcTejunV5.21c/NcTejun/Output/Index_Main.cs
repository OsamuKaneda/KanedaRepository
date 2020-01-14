using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;

namespace NcTejun.Output
{
	/// <summary>
	/// テキサスのインデックスファイルの全体の情報[不変]
	/// </summary>
	class Index_Main
	{
		/// <summary>カサブランカ_受注ＮＯ（J_NO）</summary>
		public readonly string order_no;
		/// <summary>カサブランカ_型ＮＯ（KATA_CODE）</summary>
		public readonly string parts_no;
		/// <summary>カサブランカ_型名称（KATA_NAME）-> テキサスには使用しない</summary>
		public readonly string kataName;
		/// <summary>カサブランカ_工程ＮＯ（KOUTEI_NO）</summary>
		public readonly string step_no;
		/// <summary>カサブランカ_部品ＮＯ（BUHIN_CODE）</summary>
		public readonly string process_no;

		/// <summary>ＤＮＣ手順のファイル名（受注ＮＯ_部品名）</summary>
		public readonly string workName;
		/// <summary>自由に設定可能 コメントとする（初期値：手順名）</summary>
		public readonly string setupName;
		/// <summary>自由に設定可能 工程名とする（work:）（KOUTEI_COMM）</summary>
		public readonly string comment;

		/// <summary>段取り方法 0:立置きorDMG以外 1:平置き（表が上） 2:平置き（裏が上）</summary>
		public readonly string progress;
		/// <summary>加工方向（0:表面, 1:裏面, 2:正面, 3:背面, 4:右側面, 5:左側面）</summary>
		public readonly int kakoHoko;

		/// <summary>カサブランカ_部品名称（BUHIN_NAME）</summary>
		public readonly string buhinName;
		/// <summary>カサブランカ_工程名称（KOUTEI_COMM）</summary>
		public readonly string koutei_comm;


		// /////////////
		// その他の情報
		// /////////////
		// /////////////////////////////
		// 以下はデータとしては出力しない
		// /////////////////////////////

		/// <summary>ＤＭＣ運転の可否</summary>
		public readonly bool dnc;

		/// <summary>部品加工機でのベースの高さ</summary>
		public readonly double baseHeight;
		/// <summary>部品加工機でのパレット番号</summary>
		public readonly int pallet;

		/// <summary>素材のサイズＸ（部品のみで使用）</summary>
		public readonly double mold_X;
		/// <summary>素材のサイズＹ（部品のみで使用）</summary>
		public readonly double mold_Y;
		/// <summary>素材上面の測定高さ</summary>
		public readonly double Height;

		/// <summary>固定可動の区分（固定の場合true）</summary>
		public readonly bool kotkad;





		
		/// <summary>
		/// 出力設定フォームから作成する（主型）
		/// </summary>
		/// <param name="frm"></param>
		public Index_Main(FormNcSet_Texas frm) {
			order_no = "";
			parts_no = "";
			kataName = "";
			process_no = "";
			step_no = "";
			// 製造番号がカサブランカに存在する場合
			if (frm.Casa.KATA_CODE_ARI) {
				//カサブランカの型情報（ncOutput.indexMain.casaDataの一部）の設定
				order_no = NcdTool.Tejun.Seba;
				parts_no = frm.Casa.KATA_CODE;
				kataName = frm.Casa.KATA_NAME;
				process_no = frm.comboBox_buhin.SelectedValue.ToString();
				buhinName = frm.comboBox_buhin.Text;
				//process_no = (string)Casa.Tables["DBORD"].Rows[frm.comboBox_buhin.SelectedIndex]["BUHIN_CODE"];
				//buhinName = (string)Casa.Tables["DBORD"].Rows[frm.comboBox_buhin.SelectedIndex]["BUHIN_NAME"];
				step_no = frm.comboBox_kotei.SelectedValue.ToString();
				koutei_comm = frm.comboBox_kotei.Text;
				//koutei_comm = (string)Casa.Tables["DBRTG"].Rows[frm.comboBox_kotei.SelectedIndex]["KOUTEI_COMM"];
				workName =
					frm.textBox_naisei.Text + "_" +
					frm.comboBox_buhin.SelectedValue.ToString() + "_" +
					frm.comboBox_kotei.SelectedValue.ToString();
				setupName = frm.textBoxComment.Text;
				comment = frm.comboBox_kotei.Text;
			}
			else {
				workName = frm.textBox_naisei.Text;
				setupName = frm.textBoxComment.Text;
				comment = frm.textBoxComment.Text;
			}



			// ＤＮＣ運転可否のセット
			//ncOutput.indexMain.dncSet(frm.combo_dnc.Text);
			if (frm.combo_dnc.Text == "ＤＮＣ運転")
				this.dnc = true;
			else if (frm.combo_dnc.Text == "メモリ運転")
				this.dnc = false;
			else {
				throw new Exception("");
			}

			// 段取り方法
			if (NcdTool.Tejun.Mach.Milling_5Faces == false) {
				progress = "0";
			}
			else {
				// 段取り方法
				switch (frm.comboBox_Dandori.SelectedIndex) {
				case 0:	// 平置き（表が上）固定型タイプ
					kotkad = true;
					progress = "1"; break;
				case 1:	// 平置き（表が上）可動型タイプ
					kotkad = false;
					progress = "1"; break;
				case 2:	// 平置き（裏が上）固定型タイプ
					kotkad = true;
					progress = "2"; break;
				case 3:	// 平置き（裏が上）可動型タイプ
					kotkad = false;
					progress = "2"; break;
				default: throw new Exception("");
				}

				// 加工方向
				// comboBox_Hoko, kakoHoko : 0=表面, 1=裏面, 2=正面, 3=背面, 4=右側面, 5=左側面
				kakoHoko = frm.comboBox_Hoko.SelectedIndex;
				// 金型のサイズ
				mold_X = 0.0;
				mold_Y = 0.0;
			}
		}

		/// <summary>
		/// 出力設定フォームから作成する（部品）
		/// </summary>
		/// <param name="frm"></param>
		public Index_Main(FormNcSet_Buhin frm) {
			order_no = "";
			parts_no = "";
			kataName = "";
			process_no = "";
			step_no = "";
			// 製造番号がカサブランカに存在する場合
			if (frm.Casa.KATA_CODE_ARI) {
				//カサブランカの型情報（ncOutput.indexMain.casaDataの一部）の設定
				order_no = NcdTool.Tejun.Seba;
				parts_no = frm.Casa.KATA_CODE;
				kataName = frm.Casa.KATA_NAME;
				process_no = frm.comboBox_buhin.SelectedValue.ToString();
				//buhinName = frm.comboBox_buhin.Text;
				step_no = frm.comboBox_kotei.SelectedValue.ToString();
				koutei_comm = frm.comboBox_kotei.Text;
				workName =
					frm.textBox_naisei.Text + "_" +
					frm.comboBox_buhin.SelectedValue.ToString() + "_" +
					frm.comboBox_kotei.SelectedValue.ToString();
				setupName = frm.textBoxComment.Text;
				buhinName = frm.comboBox_bKubun.Text;
				comment = frm.comboBox_kotei.Text;
			}
			else {
				workName = frm.textBox_naisei.Text;
				setupName = frm.textBoxComment.Text;
				comment = frm.textBoxComment.Text;
			}

			// ＤＮＣ運転可否のセット
			//ncOutput.indexMain.dncSet(frm.combo_dnc.Text);
			if (frm.combo_dnc.Text == "ＤＮＣ運転")
				this.dnc = true;
			else if (frm.combo_dnc.Text == "メモリ運転")
				this.dnc = false;
			else {
				throw new Exception("");
			}

			// ＸＹ基準
			kotkad = true;
			// 加工方向
			kakoHoko = 99;

			// 段取り方法
			//progress = frm.comboBox_Dandori.SelectedIndex.ToString();
			progress = frm.comboBox_Dandori.Text;

			// 部品加工のベースの厚さ
			baseHeight = (double)frm.comboBox_Dandori.SelectedValue;

			// パレット番号
			pallet = Convert.ToInt32(frm.comboBox_pallet.Text);

			// 部品のサイズ
			mold_X = Convert.ToDouble(frm.textBox_X.Text);
			mold_Y = Convert.ToDouble(frm.textBox_Y.Text);
			Height = Convert.ToDouble(frm.textBox_H.Text);
		}

		/// <summary>
		/// すでに出力されたインデックスファイルから作成する
		/// </summary>
		/// <param name="sr"></param>
		public Index_Main(System.IO.StreamReader sr) {
			string stmp;
			while (!sr.EndOfStream) {
				stmp = sr.ReadLine();
				if (stmp.IndexOf("program") == 0)
					break;
				else if (stmp.IndexOf("work-name:") == 0)
					workName = stmp.Substring(10);
				else if (stmp.IndexOf("setup-name:") == 0)
					setupName = stmp.Substring(11);
				else if (stmp.IndexOf("order-no:") == 0)
					order_no = stmp.Substring(9);		// 受注ＮＯ
				else if (stmp.IndexOf("parts-no:") == 0)
					parts_no = stmp.Substring(9);		// 型ＮＯ
				else if (stmp.IndexOf("step-no:") == 0)
					step_no = stmp.Substring(8);		// 工程ＮＯ
				else if (stmp.IndexOf("process-no:") == 0)
					process_no = stmp.Substring(11);	// 部品ＮＯ
				else if (stmp.IndexOf("work:") == 0)
					comment = stmp.Substring(5);
				else if (stmp.IndexOf("progress:") == 0)
					progress = stmp.Substring(9);
			}
		}

	}
}
