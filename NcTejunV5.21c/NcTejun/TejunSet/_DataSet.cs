using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace NcTejun.TejunSet
{
	/// <summary>
	/// 加工手順全般の情報を作成する。FormTejunに入力された情報より作成される。
	/// </summary>
	class _DataSet
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public _DataSet() { ;}

		/// <summary>
		/// 手順データのセット
		/// </summary>
		public void TejunSet() {

			// ダイアログを用いてSetMain, SetToolを実行
			StringBuilder errMessage = new StringBuilder();

			//FormCommonDialog frmtmp = new FormCommonDialog("tejun", this, errMessage);
			DialogResult result = DialogResult.OK;
			using (CamUtil.FormCommonDialog frmtmp = new CamUtil.FormCommonDialog("加工手順情報の取得", this.SetMain, errMessage)) {
				Application.DoEvents();
				result = frmtmp.ShowDialog();
			}
			if (result != DialogResult.OK)
				throw new Exception(errMessage.ToString());
			return;
		}

		/// <summary>
		/// 手順データと工具表データを読み込む。TejunSetのShowDialog()により実行される。
		/// 最初の２文字で「手順」と「工具」に分類されたエラーメッセージはFormTejunで処理される。
		/// </summary>
		/// <param name="disp_message">実行時に表示するメッセージ Application.DoEvents(); で更新する</param>
		private void SetMain(Label disp_message)
		{
			// 手順
			try { SetNcdt(disp_message); }
			catch (Exception ex) { throw new Exception("手順" + ex.Message); }
			// 工具
			try { SetTool(disp_message); }
			catch (Exception ex) { throw new Exception("工具" + ex.Message); }

			// /////////////////////////////////////////////////////
			// ガンドリルの孔データ抽出状況の表示
			// /////////////////////////////////////////////////////
			if (NcdTool.Tejun.Mach.ID == CamUtil.Machine.MachID.MHG_1500) {
				string mess = "";
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NcMatch) {
					// 暫定処置
					if (NcTejun.TejunSet.ToolSheet.Match(ncnam.tsheet) == false) throw new Exception("frqbefrqerhfb");
					if (ncnam.nnam == NcdTool.NcName.NcNam.DMY) throw new Exception("frqbefrqerhfb");
					if (ncnam.Holes.tcntList.Count > 0) {
						if (mess.Length > 0) mess += "\r\n\r\n";
						mess += ncnam.nnam + " から以下の孔データを抽出しました\r\n" + ncnam.Holes.ToString();
					}
				}
				if (mess.Length > 0)
					CamUtil.FormMessageBox.Show("ガンドリルの孔情報", "\r\n" + mess);
			}
		}


		/// <summary>
		/// ＮＣデータ情報の読込みとチェック
		/// </summary>
		/// <param name="disp_message">ダイアログに表示するメッセージを返す</param>
		private void SetNcdt(Label disp_message) {

			// ///////////////////////////////////////
			// 加工方向のチェック add in 2016/11/10
			// ///////////////////////////////////////
			disp_message.Text = "加工方向をチェックしています";
			Application.DoEvents();
			bool? tmp = null;
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll) {
				if (tmp == null) tmp = ncnam.nmgt.Omoteura.HasValue;
				if (tmp != ncnam.nmgt.Omoteura.HasValue) throw new Exception("加工方向が設定されていないＮＣデータが存在する。");
				if (ncnam.nmgt.Omoteura.HasValue) {
					foreach (NcdTool.NcName.NcDataT td in ncnam.Ncdata.Tld)
						if (td.XmlT.Keisha) throw new Exception("傾斜加工に加工方向が設定されている。");
				}
			}
			if (tmp.Value) {
				CamUtil.LogOut.CheckCount("Tejun 292", false, NcdTool.Tejun.TejunName + " 手順内加工方向設定");
				switch (NcdTool.Tejun.Mach.ID) {
				case CamUtil.Machine.MachID.DMU200P:
				case CamUtil.Machine.MachID.DMU210P:
				case CamUtil.Machine.MachID.DMU210P2:
					break;
				default:
					MessageBox.Show("加工方向が設定されていますが、ＤＭＧ以外の加工機のため無視されます。");
					break;
				}
			}

			// /////////////////////////////////////////////////////
			// ＮＣデータ情報（NCDATA, TDATA, JNNO）の読込み
			// /////////////////////////////////////////////////////
			disp_message.Text = "ＮＣデータの情報を取得しています";
			Application.DoEvents();
			NcdTool.NcName.NcNam.SetKdata(disp_message);
			CamUtil.LogOut.Warning("加工手順データ");

			// ////////////////////////////////////////////
			// 次元の整合チェック
			// ////////////////////////////////////////////
			Application.DoEvents();
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NotDummy) {
				// 出力値の設定
				switch (ncnam.nmgt.Dimn) {
				case '2':
					if (ncnam.Ncdata.ncInfo.xmlD.CamDimension == 3 && ncnam.Ncdata.ncInfo.xmlD.BaseNcFormat.Id != CamUtil.BaseNcForm.ID.BUHIN)
						MessageBox.Show($"{ncnam.nnam} は{ncnam.Ncdata.ncInfo.xmlD.CamDimension.ToString()}次元であるが加工手順は{ncnam.nmgt.Dimn.ToString()}次元となっている");
					break;
				case '3':
					if (ncnam.Ncdata.ncInfo.xmlD.CamDimension == 2 && ncnam.Ncdata.ncInfo.xmlD.BaseNcFormat.Id != CamUtil.BaseNcForm.ID.BUHIN)
						throw new Exception($"{ncnam.nnam} は{ncnam.Ncdata.ncInfo.xmlD.CamDimension.ToString()}次元であるが加工手順は{ncnam.nmgt.Dimn.ToString()}次元となっている");
					break;
				default: throw new Exception("awefbaqrhfbqerh");
				}
			}

			// ///////////////////////////////////////
			// 移動ミラーのチェック add in 2017/07/18
			// ///////////////////////////////////////
			disp_message.Text = "移動ミラー情報をチェックしています";
			Application.DoEvents();
			switch (NcdTool.Tejun.BaseNcForm.Id) {
			case CamUtil.BaseNcForm.ID.GENERAL:
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll) {
					if (ncnam.nggt.trns != CamUtil.Vector3.v0 || ncnam.nggt.Mirr != CamUtil.NcZahyo.Null || ncnam.nggt.rev) {
						if (ncnam.Ncdata.ncInfo.xmlD.CamDimension == 2)
							foreach (NcdTool.NcName.Kogu skog in ncnam.Tdat)
								if (skog.Tld.XmlT.Keisha) {
									if (CamUtil.ProgVersion.Debug) {
										MessageBox.Show(ncnam.nnam + " ２次元傾斜加工ＮＣデータの移動・ミラーは定義されていません。DEGUBでのみ実行する");
										break;
									}
									else
										throw new Exception(ncnam.nnam + " ２次元傾斜加工ＮＣデータの移動・ミラーは定義されていません。");
								}
					}
				}
				break;
			case CamUtil.BaseNcForm.ID.BUHIN:
				foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll) {
					if (ncnam.nggt.trns != CamUtil.Vector3.v0 || ncnam.nggt.Mirr != CamUtil.NcZahyo.Null || ncnam.nggt.rev) {
						CamUtil.LogOut.CheckCount("_DataSet 190", true, "ＮＣデータの移動・ミラーの設定は、部品加工で未対応のため無視されます。");
						break;
					}
				}
				break;
			default: throw new Exception("gwtgwjtgwjgntwrhyn");
			}

			// ////////////////////////////////////////////////////////////////////
			// 加工機との整合性のチェック（工具参照点のチェックを含む）
			// ////////////////////////////////////////////////////////////////////
			disp_message.Text = "加工機との整合性をチェックしています";
			Application.DoEvents();
			foreach (NcdTool.NcName.NcNam ncsd in NcdTool.Tejun.NcList.NcNamsAll_NcExist)
				foreach (NcdTool.NcName.Kogu skog in ncsd.Tdat) {
					// ツールセット未設定の場合は対象外（加工機不整合など）
					//if (skog.tsetCHG.tset_name == null) continue;
					if (skog.TsetCHK == false) continue;
					NcdTool.Tejun.Mach.KakoKahi(skog);
				}

			// /////////////////////////////////////////////////////
			// ＮＣＳＰＥＥＤの場合、材質と加工機の整合をチェック
			// /////////////////////////////////////////////////////
			if (NcdTool.Tejun.Ncspeed) {
				disp_message.Text = "シミュレーションとの材質と加工機の整合性をチェックしています";
				Application.DoEvents();
				foreach (NcdTool.NcName.NcData ncdat in NcdTool.Tejun.NcList.NcData) {
					if (ncdat.ncInfo != null && ncdat.ncInfo.xmlD.SmNAM != null) continue;
					throw new Exception("一部ＮＣＳＰＥＥＤで検証されていないＮＣデータが存在する");
				}
				//collision = false;
				foreach (NcdTool.NcName.NcNam ncsd in NcdTool.Tejun.NcList.NcNamsAll_NcExist) {
					//if (mach.machn != ncsd.ncdata.ncInfo.xmlD.smMCN) {
					//	MessageBox.Show(ncsd.nnam + "のＮＣＳＰＥＥＤの加工機が整合していない");
					//	collision = true;
					//	break;
					//}
					if (ncsd.nggt.zaisGrp != CamUtil.Material.ZgrpgetPC(ncsd.Ncdata.ncInfo.xmlD.SmMAT)) {
						MessageBox.Show(ncsd.nnam + "のＮＣＳＰＥＥＤの材質名が整合していない");
						//collision = true;
						break;
					}
					for (int ii = 0; ii < ncsd.Itdat; ii++) {
						if (ncsd.Ncdata.ncInfo.xmlD[ii].SmCLC != "OK") {
							MessageBox.Show(ncsd.nnam + "はＮＣＳＰＥＥＤにより干渉が検出されている");
							//collision = true;
							break;
						}
						if (ncsd.Ncdata.ncInfo.xmlD.SmNAM == "NCSPEED" && ncsd.Ncdata.ncInfo.xmlD[ii].Keisha)
							throw new Exception(ncsd.nnam + "は傾斜加工であるがNCSPEEDで検証されている");
					}
				}
				// /////////////////////////////////////////////////////
				// Uerikaの場合、分割のチェックを実施 Add in 2015/07/23
				// /////////////////////////////////////////////////////
				disp_message.Text = "シミュレーションUerikaの分割の整合性をチェックしています";
				Application.DoEvents();
				List<string> orgName = new List<string>();	// 分割されたＮＣデータの元データ
				CamUtil.CamNcD.NcInfo ncinf;
				List<NcdTool.NcName.NcNam>[] bunkList;	// １つの元データから分割された、工具単位ごとのＮＣデータリスト
				int tcnt;

				foreach (NcdTool.NcName.NcNam ncsd in NcdTool.Tejun.NcList.NcNamsAll_NcExist) {
					if (ncsd.Itdat != 1) throw new Exception("efbqfrqerfbqh");
					if (ncsd.Ncdata.ncInfo.xmlD.SmNAM == "EUREKA" && ncsd.Ncdata.ncInfo.xmlD[0].SmLNS)
						if (!orgName.Contains(ncsd.Ncdata.ncInfo.xmlD.CamOriginalNcName))
							orgName.Add(ncsd.Ncdata.ncInfo.xmlD.CamOriginalNcName);
				}
				for (int ii = 0; ii < orgName.Count; ii++) {
					ncinf = new CamUtil.CamNcD.NcInfo(Path.ChangeExtension(CamUtil.ServerPC.FulNcName(orgName[ii]), "xml"));
					bunkList = new List<NcdTool.NcName.NcNam>[ncinf.xmlD.ToolCount];
					tcnt = 0;
					bunkList[tcnt] = new List<NcdTool.NcName.NcNam>();
					foreach (NcdTool.NcName.NcNam ncsd in NcdTool.Tejun.NcList.NcNamsAll_NotDummy) {
						if (orgName[ii] == ncsd.Ncdata.ncInfo.xmlD.CamOriginalNcName) {
							while (ncinf.xmlD[tcnt].SNAME != ncsd.Ncdata.ncInfo.xmlD[0].SNAME) {
								tcnt++;
								bunkList[tcnt] = new List<NcdTool.NcName.NcNam>();
							}
							bunkList[tcnt].Add(ncsd);
						}
					}

					for (tcnt = 0; tcnt < bunkList.Length; tcnt++) {
						switch (bunkList[tcnt].Count) {
						case 0:
							throw new Exception("工具寿命分割情報smLNSエラー");
						case 1:
							//if (bunkList[tcnt][0].ncdata.ncInfo.xmlD[0].smLNS) throw new Exception("工具寿命分割情報smLNSエラー");
							break;
						default:
							foreach (NcdTool.NcName.NcNam ncnam in bunkList[tcnt])
								if (!ncnam.Ncdata.ncInfo.xmlD[0].SmLNS) throw new Exception("工具寿命分割情報smLNSエラー");
							NcdTool.NcSIM.NcSimul.CheckEUREKA(ncinf.xmlD, ncinf.xmlD[tcnt], bunkList[tcnt]);
							break;
						}
					}
				}
			}
			return;
		}
		/// <summary>
		/// 工具表データを読み込む
		/// </summary>
		/// <param name="disp_message"></param>
		private void SetTool(Label disp_message) {

			// /////////////////////////////////////////////////////
			// 工具表のTsetとの関連付けとデータの読込み
			// ユニックスのツールシート読込みとTejunのツールシートリストとの関連付け
			// /////////////////////////////////////////////////////
			disp_message.Text = "工具表の情報を取得しています";
			Application.DoEvents();
			Program.frm1.MakeToolTable();

			// 工具表のテーブルへの読込み
			try {
				foreach (ToolSheet ts in Program.frm1.TsheetList)
					ts.TolstSet();
			}
			catch {
				Program.frm1.TsheetClear();
				throw;
			}

			// ///////////////////////////////////////////////
			// 工具表でエラーが発生した場合、以降を実施しない
			// ///////////////////////////////////////////////
			foreach (ToolSheet ts in Program.frm1.TsheetList)
				if (ts.Tolst_commit == false)
					throw new Exception("工具表でエラーが発生しました。修正してください。");

			disp_message.Text = "工具表を作成しています";
			Application.DoEvents();

			if (Program.frm1.TsheetList.Length > 1) {
				string tall = ""; foreach (ToolSheet ts in Program.frm1.TsheetList) tall += " " + ts.Name;
				CamUtil.LogOut.CheckCount("_DataSet 301", false, "ToolSheet が複数ある手順です " + NcdTool.Tejun.TejunName + tall);
				if (Program.frm1.TsheetList.Length != NcdTool.Tejun.NcList.NcNamsAll.Count)
					CamUtil.LogOut.CheckCount("_DataSet 303", false, "ToolSheet とTSet の数が異なる手順です " + NcdTool.Tejun.TejunName);
			}
			ToolSheet.Matching_TSheet(false, 0, 0, null, 0);

			return;
		}
	}
}
