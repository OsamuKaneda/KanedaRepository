using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.IO;
using CamUtil;

namespace NcTejun.Output
{
	/// <summary>
	/// ＮＣデータ、手順書、工具表の出力の情報を作成する
	/// FormTexasSetで作成される
	/// </summary>
	partial class NcOutput
	{
		/// <summary>出力するフォルダー名</summary>
		public static string Dir_PTR { get { return NcdTool.Tejun.Mach.Dir_PTR; } }

		/// <summary>工具チェック用ＮＣデータの出力行の正規表現。プローブ(J)は含まれない。</summary>
		private static System.Text.RegularExpressions.Regex regex =
			new System.Text.RegularExpressions.Regex("G100T[0-9][0-9]*[A-IK-Z]");






		/// <summary>出力するフォルダー名</summary>
		public string lfldr;

		/// <summary>ＮＣ出力対象のリスト</summary>
		public NcOutList ncoutList = null;

		// （ADD in 2013/02/13）
		/// <summary>
		/// Ｚ方向以外の加工方向の有無
		/// true  = TEBIS-G0傾斜がある
		/// false = TEBIS以外のＣＡＭとTEBIS-G0の傾斜なしのみ
		/// </summary>
		public readonly bool keisha;

		/// <summary>手順書で加工方向を指示している場合はtrue（DMG以外は無視）</summary>
		public readonly bool? tjnHoko;

		/// <summary>同時５軸加工が含まれる場合はtrue</summary>
		public readonly bool doji5ax;


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public NcOutput() {
			//　２次元ＣＡＭで出力され、加工面を指定して加工するＮＣデータ
			bool SelKakoFace = false;
			//　５軸ＣＡＭで出力され、傾斜角が０でないＮＣデータ
			bool keisha_tmp = false;

			Program.MiKensho._sk_angle0 = Program.MiKensho._2d_angle0 = Program.MiKensho._3d_angle0 = false;
			Program.MiKensho._sk_keisha = Program.MiKensho._2d_keisha = Program.MiKensho._3d_keisha = false;
			tjnHoko = null;
			doji5ax = false;

			// ///////////////////////////
			// 全ＮＣ出力対象のリストの作成
			// ///////////////////////////
			ncoutList = new NcOutList();

			// ///////////////////////////
			// 傾斜加工のチェック
			// ///////////////////////////
			//progNo = 0;
			foreach (NcToolL tolL in ncoutList.Tl) {

				// 加工方向を手順で設定しているか
				if (tjnHoko == null) tjnHoko = tolL.Ncnam.nmgt.Omoteura.HasValue;
				if (tjnHoko != tolL.Ncnam.nmgt.Omoteura.HasValue) throw new Exception("efhbqwebfrh");
				// 同時５軸加工か
				if (tolL.Skog.Tld.XmlT.SimultaneousAxisControll) doji5ax = true;

				if (tolL.Smch.K2.Tlgn.Toolset.Probe && tolL.Ncnam.Ncdata.ncInfo.xmlD.CamSystemID.Name == CamSystem.Tebis) {
					// ＤＭＧでは測定ポイント毎に傾斜角度を設定する使用のため、傾斜無しの測定でも傾斜加工とする必要あり
					keisha_tmp = true;
				}
				else if (tolL.Skog.Tld.XmlT.Keisha || tolL.Skog.Tld.XmlT.SimultaneousAxisControll) {
					keisha_tmp = true;
				}
				else {
					switch (tolL.Ncnam.Ncdata.ncInfo.xmlD.CamSystemID.Name) {
					case CamSystem.Tebis:
					case CamSystem.CAMTOOL:
					case CamSystem.CAMTOOL_5AXIS:
					//case CamSystem.WorkNC_5AXIS:
						break;
					case CamSystem.WorkNC:
						switch (tolL.Ncnam.Ncdata.ncInfo.xmlD.PostProcessor.Id) {
						case PostProcessor.ID.gousei5:
							break;
						case PostProcessor.ID.GOUSEI:
						case PostProcessor.ID.GENERAL:
							SelKakoFace = true;
							break;
						default: throw new Exception("qjrefbqhrb");
						}
						break;
					default:
						SelKakoFace = true;
						break;
					}
				}

				// 未使用である傾斜加工の種類を設定
				if (tolL.Ncnam.Ncdata.ncInfo.xmlD.CamSystemID.Name == CamSystem.Tebis) {
					if (tolL.Skog.Tld.XmlT.Keisha) {
						if (tolL.Smch.K2.Tlgn.Toolset.Probe)
							Program.MiKensho._sk_keisha = true;
						else if (tolL.Ncnam.Ncdata.ncInfo.xmlD.CamDimension == 2)
							Program.MiKensho._2d_keisha = true;
						else
							Program.MiKensho._3d_keisha = true;
					}
					else {
						if (tolL.Smch.K2.Tlgn.Toolset.Probe)
							Program.MiKensho._sk_angle0 = true;
						else if (tolL.Ncnam.Ncdata.ncInfo.xmlD.CamDimension == 2)
							Program.MiKensho._2d_angle0 = true;
						else
							Program.MiKensho._3d_angle0 = true;
					}
				}
			}

			// ////////////////////
			// 傾斜角度の最終設定
			// ////////////////////
			// iTNC540、傾斜、測定
			if (keisha_tmp) {
				// テービス以外のＣＡＭあり（傾斜なし）
				if (SelKakoFace)
					MessageBox.Show("工具軸設定の無い３軸加工のＮＣデータは上面を加工するものとして処理します。");
				if (tjnHoko.Value) throw new Exception("ewqajfbqefvbeqfhb");
				keisha = true;
			}
			else
				keisha = false;

			// １工具１ＮＣデータの場合、全ての出力名が異なっているかチェックする
			if (NcdTool.Tejun.Mach.Toool_nc) {
				List<string> onamList = new List<string>();
				string Onam;
				foreach (NcToolL ntmp in ncoutList.Tl) {
					if (ntmp.Skog.matchK.Length > 1)
						Onam = ntmp.Outnam + ntmp.MatchNo.ToString("_00");
					else
						Onam = ntmp.Outnam;
					if (onamList.Contains(Onam))
						throw new Exception(Onam + "は複数存在する");
					onamList.Add(Onam);
				}
			}
		}

		/// <summary>
		/// 同じ工具番号のＮＣデータ数を数える
		/// </summary>
		/// <param name="skogx"></param>
		/// <param name="matchNo"></param>
		/// <returns></returns>
		private int TnumNumber(NcdTool.NcName.Kogu skogx, int matchNo) {
			int number = 0;
			int tnum = skogx.matchK[matchNo].K2.Tnum;
			int snum = skogx.matchK[matchNo].SnumN;
			foreach (NcToolL nclist in ncoutList.Tl) {
				if (nclist.Smch.K2.Tnum != tnum)
					continue;
				if (nclist.Smch.SnumN != snum)
					continue;
				number++;
			}
			return number;
		}

		/// <summary>
		/// テキサスの場合にFormNcSet設定後にindexを作成する
		/// </summary>
		public void NcOutputSet() {
			// メッセージ出力用
			List<string> messList = new List<string>();

			// ＤＭＧ工具管理データ作成の準備
			Index.ListTnoTable.ToolMaxNo(ncoutList.Tl);

			// ///////////////////////////////////////////////////
			// ＤＭＧのテキサス情報のセットと工具本数の最終セット
			// ///////////////////////////////////////////////////
			// ncoutListのＪＯＫＥＮ情報作成
			for (int ii = 0; ii < ncoutList.Tj.Count; ii++) {
				NcToolL next = null;
				for (int kk = ii + 1; kk < ncoutList.Tj.Count; kk++) {
					if (ncoutList.Tj[kk].nknum == null && ncoutList.Tj[kk].tNodeChecked == false) continue;
					if (ncoutList.Tj[kk].nknum != null && ncoutList.Tj[kk].nknum.tNodeChecked == false) continue;
					next = ncoutList.Tj[kk];
					break;
				}
				if (ncoutList.Tj[ii].Skog.Output) {
					string[] mess = new string[2];  // クーラントの設定値。[0]初期値、[1]最終決定値
					ncoutList.Tj[ii].index = new Index(ncoutList.Tj[ii], NcdTool.Tejun.Mach, this.keisha, this.tjnHoko.Value, next, mess);
					if (mess[0] != mess[1] && ncoutList.Tj[ii].tolInfo.TsetCAM.tscamName != "AIRBLOW_B") {
						string ss = String.Format("{0}のＤＢ情報により、クーラントが'{1}'の工具はクーラントを'{2}'に変更して出力します", NcdTool.Tejun.Mach.name, mess[0], mess[1]);
						if (messList.Contains(ss) == false) messList.Add(ss);
					}
				}
			}
			// メッセージの表示
			if (messList.Count > 0) {
				string mess = "";
				foreach (string ss in messList) mess += ss + "\r\n";
				FormMessageBox.Show("クーラントの変更", mess, 800, 150, "HGｺﾞｼｯｸM", (float)10.5);
			}
		}
	}
}
