using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.IO;
using CamUtil;

namespace NcdTool.NcSIM
{
	class NcSimul
	{

		/// <summary>
		/// ユーリカでの分割チェック（傾斜角度とクリアランス面）
		/// </summary>
		/// <param name="ninf">元のＸＭＬの情報（ＮＣ）</param>
		/// <param name="tinf">元のＸＭＬの情報（工具）</param>
		/// <param name="bunkList">寿命分割されたＮＣデータのリスト</param>
		public static void CheckEUREKA(CamUtil.CamNcD.XmlNavi ninf, CamUtil.CamNcD.XmlNavi.Tool tinf, List<NcName.NcNam> bunkList) {
			int axisCnt;
			Angle3 angZXZ;
			CamUtil.LCode.NcQueue ncQue;

			string form1 = "ＮＣ名 : {0}  クリアランス面エラー ORIGINAL : {1}  {2,-3} : {3}";
			string form2 = "ＮＣ名 : {0}  工具軸角度設定エラー ORIGINAL : {1}  {2,-3} : {3}";

			double[] clpList = tinf.ClrPlaneList;
			Angle3[] angList = tinf.MachiningAxisList;
			RotationAxis rot;

			axisCnt = 0;
			foreach (NcName.NcNam ncnam in bunkList) {
				using (StreamReader sr = new StreamReader(ncnam.Ncdata.fulnamePC)) {
					ncQue = new CamUtil.LCode.NcQueue(5, false, ncnam.Ncdata.ncInfo.xmlD.NcClrPlaneList, Tejun.BaseNcForm, CamUtil.LCode.NcLineCode.GeneralDigit, true, true);
					ncQue.NextLine(sr);
					for (int ii = 0; ii < ncnam.Ncdata.ncInfo.xmlD[0].AxisCount; ii++) {
						while (ncQue[0].NcLine.IndexOf("M30") != 0 && ncQue[0].NcLine.IndexOf(ClLink5Axis.Start_G) != 0 && ncQue[0].NcLine.IndexOf(ClLink5Axis.ChgAX_G) != 0) ncQue.NextLine(sr);
						if (ncQue[0].NcLine.IndexOf("M30") == 0) throw new Exception("ＮＣ名 : " + ncnam.nnam + "  P9698 の数が異常");
						// ///////////////////////////
						// 最初のＮＣデータの先頭
						// ///////////////////////////
						if (ii == 0 && ncnam == bunkList[0]) {
							if (ncQue[0].NcLine.IndexOf(ClLink5Axis.Start_G) != 0) throw new Exception("ＮＣ名 : " + ncnam.nnam + "  P9697 が存在しない");
							// 大きい小さいが反対であったので変更 in 2015/08/20
							// ２次元加工のみに限定 in 2016/05/24
							if (ninf.CamDimension == 2 && clpList[axisCnt] - ncQue[+2].Xyzsf['Z'] > 0.0)
								throw new Exception(String.Format(form1, ncnam.nnam, clpList[axisCnt].ToString(), "NC", ncQue[+2].Xyzsf['Z'].ToString()));
						}
						// ///////////////////////////
						// 分割されたＮＣデータの先頭
						// ///////////////////////////
						else if (ii == 0) {
							if (ncQue[0].NcLine.IndexOf(ClLink5Axis.Start_G) != 0) throw new Exception("ＮＣ名 : " + ncnam.nnam + "  P9697 が存在しない");
							if (angList[axisCnt].ToString(4) != ncnam.Ncdata.ncInfo.xmlD[0][ii].MachiningAxisAngle.ToString(4))
								axisCnt++;	// 工具軸の変更位置で分割された
							if (Math.Abs(clpList[axisCnt] - ncQue[+2].Xyzsf['Z']) > 0.001)
								throw new Exception(String.Format(form1, ncnam.nnam, clpList[axisCnt].ToString(), "NC", ncQue[2].Xyzsf['Z'].ToString()));
						}
						// ///////////////////////////
						// 工具軸角度変更位置
						// ///////////////////////////
						else {
							if (ncQue[0].NcLine.IndexOf(ClLink5Axis.ChgAX_G) != 0) throw new Exception("ＮＣ名 : " + ncnam.nnam + "  P9697 の数が異常");
							// ２次元加工のみに限定 in 2015/10/15
							if (ninf.CamDimension == 2 && clpList[axisCnt] - ncQue[-1].Xyzsf['Z'] < 0.0)
								throw new Exception(String.Format(form1, ncnam.nnam, clpList[axisCnt].ToString(), "NC", ncQue[-1].Xyzsf['Z'].ToString()));
							axisCnt++;
							if (clpList[axisCnt] - ncQue[+2].Xyzsf['Z'] < 0.0)
								throw new Exception(String.Format(form1, ncnam.nnam, clpList[axisCnt].ToString(), "NC", ncQue[+2].Xyzsf['Z'].ToString()));
						}
						// クリアランスの検証：ＸＭＬ内
						if (Math.Abs(clpList[axisCnt] - ncnam.Ncdata.ncInfo.xmlD[0][ii].ClearancePlane) > 0.001)
							throw new Exception(String.Format(form1, ncnam.nnam, clpList[axisCnt].ToString(), "XML", ncnam.Ncdata.ncInfo.xmlD[0][ii].ClearancePlane));
						// 工具軸の検証： 元のオイラーＸＹＺでの比較（ＸＭＬ内）
						if (angList[axisCnt].ToString(4) != ncnam.Ncdata.ncInfo.xmlD[0][ii].MachiningAxisAngle.ToString(4))
							throw new Exception(String.Format(form2, ncnam.nnam, angList[axisCnt].ToString(4), "XML", ncnam.Ncdata.ncInfo.xmlD[0][ii].MachiningAxisAngle.ToString()));
						// 工具軸の検証： 元のオイラーＸＹＺでの比較（ＮＣコメント欄）
						//		- 分割された最初の角度部分はコメント欄が空になっているため検証不能 2016/01/15
						if (ncQue[0].OutLine.Comment.Length > 0)
							if (angList[axisCnt].ToString(3) != new Angle3(angList[axisCnt].Jiku, ncQue[0].OutLine.Comment).ToString(3))
								throw new Exception(String.Format(form2, ncnam.nnam, angList[axisCnt].ToString(3), "NC", ncQue[0].OutLine.Comment));
						// 工具軸の検証： ユーリカのオイラーＺＸＺでの比較（ＮＣライン）
						rot = new RotationAxis(angList[axisCnt]);
						angZXZ = ncnam.Ncdata.ncInfo.xmlD.CamDimension == 2 ? rot.Euler_ZXZ() : rot.DMU_BC();
						if (angZXZ.ToString(3) != new Angle3(angZXZ.Jiku, ncQue[0].NcLine.Substring(8)).ToString(3))
							throw new Exception(String.Format(form2, ncnam.nnam, angZXZ.ToString(3), "NC", ncQue[0].NcLine.Substring(8)));
						ncQue.NextLine(sr);
					}
					while (ncQue[0].NcLine.IndexOf("M30") != 0 && ncQue[0].NcLine.IndexOf(ClLink5Axis.Start_G) != 0 && ncQue[0].NcLine.IndexOf(ClLink5Axis.ChgAX_G) != 0) ncQue.NextLine(sr);
					if (ncQue[0].NcLine.IndexOf("M30") != 0) throw new Exception("ＮＣ名 : " + ncnam.nnam + "  P9698 の数が異常");
					// ///////////////////////////
					// 分割されたＮＣデータの末尾
					// ///////////////////////////
					if (ncnam != bunkList[bunkList.Count - 1]) {
						if (ninf.CamDimension == 2 && Math.Abs(clpList[axisCnt] - ncQue[-1].Xyzsf['Z']) > 0.001)
							throw new Exception(String.Format(form1, ncnam.nnam, clpList[axisCnt].ToString(), "NC", ncQue[-1].Xyzsf['Z'].ToString()));
					}
					// ///////////////////////////
					// 最後のＮＣデータの末尾
					// ///////////////////////////
					else {
						// 大きい小さいが反対であったので変更 in 2015/08/20
						// ２次元加工のみに限定 in 2015/10/15
						if (ninf.CamDimension == 2 && clpList[axisCnt] - ncQue[-1].Xyzsf['Z'] > 0.0)
							throw new Exception(String.Format(form1, ncnam.nnam, clpList[axisCnt].ToString(), "NC", ncQue[-1].Xyzsf['Z'].ToString()));
					}
				}
			}
		}




		List<string> tjnString;
		string dirn;
		char trGrp;
		public List<string> ncdCount;

		public DialogResult NcSimulSet(List<string> p_tjnString, string dirn) {
			DialogResult result;
			StringBuilder trGroup;

			this.tjnString = p_tjnString;
			this.dirn = dirn;
			trGroup = new StringBuilder();
			ncdCount = new List<string>();

			// /////////////////////////////////
			// ＮＣＳＰＥＥＤ用データ設定の実行
			// /////////////////////////////////
			FormNcSpeed frmTjnG = new FormNcSpeed(trGroup);
			result = frmTjnG.ShowDialog();
			if (result != DialogResult.OK) return result;
			trGrp = trGroup[0];

			// /////////////////////////////////
			// ＮＣＳＰＥＥＤ用データ変換の実行
			// /////////////////////////////////
			StringBuilder errMessage = new StringBuilder();
			//FormCommonDialog frmDia = new FormCommonDialog("NCSPEED", this, errMessage);
			using (FormCommonDialog frmDia = new FormCommonDialog("ＮＣシミュレーション実行用データ作成", this.Exec, errMessage)) {
				result = frmDia.ShowDialog();
			}
			if (result != DialogResult.OK)
				MessageBox.Show(errMessage.ToString(), "NcSimul", MessageBoxButtons.OK, MessageBoxIcon.Error);

			// ///////////////
			// ログを保存する
			// ///////////////
			string move = "";
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsAll_NotDummy) {
				if (ncnam.nggt.trns.X != 0.0 && move.IndexOf("X") < 0) move += "X";
				if (ncnam.nggt.trns.Y != 0.0 && move.IndexOf("Y") < 0) move += "Y";
				if (ncnam.nggt.trns.Z != 0.0 && move.IndexOf("Z") < 0) move += "Z";
				if (ncnam.nggt.ToolLengthHosei.Zero != true && move.IndexOf("L") < 0) move += "L";
			}
			CamUtil.LogOut.CheckOutput(CamUtil.LogOut.FNAM.NCSPEEDLG, Tejun.TejunName, Tejun.Mach.name, "move=" + move);

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="disp_message">実行時に表示するメッセージ Application.DoEvents(); で更新する</param>
		public void Exec(Label disp_message) {

			// //////////////////////////////////////////////
			// 手順書・ＸＭＬ・ＮＣデータを工具単位に変換
			// //////////////////////////////////////////////

			int NcNo = -1;
			NcName.NcNam ncd;

			using (StreamWriter sw = new StreamWriter(dirn + "\\Tejun")) {
				bool existT = false, existW = false;
				foreach (string str in this.tjnString) {
					if (str.IndexOf("T ") == 0) existT = true;
					if (str.IndexOf("W ") == 0) existW = true;
				}

				foreach (string str in this.tjnString) {
					switch (str[0]) {
					case 'D':
						sw.WriteLine(str);
						if (existT == false) {
							sw.WriteLine("T " + Path.GetFileName(dirn));
							existT = true;
						}
						if (existW == false) {
							sw.WriteLine("W " + Path.GetFileName(dirn));
							existW = true;
						}
						break;
					case 'n':
						NcNo++;
						break;
					case 'N':
						NcNo++;
						ncd = NcdTool.Tejun.NcList.NcNamsAll[NcNo];
						if (str.IndexOf(ncd.nnam) != 2)
							throw new Exception("aergfaerfhnh");

						// ＮＣデータの出力
						if (NcName.Zopt.NcOutput.Nctoks(ncd)) {
							// ツーリンググループを追加
							if (ncdCount.Count == 0)
								sw.WriteLine("X " + trGrp);

							int TlNo;
							string nnam;
							Angle3[][] angle;

							//TlNo = 0; foreach (string str1 in ncdCount) if (str1 == ncd.nnam) TlNo++;
							TlNo = ncd.Jnno.ClStartNo - 1;

							// /////////////////
							// ＮＣデータの作成
							// /////////////////
							disp_message.Text = ncd.nnam + "を変換しています。";
							Application.DoEvents();

							// 回転軸の回転角を設定する
							angle = ncd.Tdat.Select(kog => kog.Tld.XmlT.MachiningAxisList).ToArray();
							// 標準の送り速度を設定する
							double[] feedrate = ncd.Tdat.Select(kog => kog.Tld.XmlT.FEEDR).ToArray();
							// 標準の送り速度を設定する
							bool[] simult = ncd.Tdat.Select(kog => kog.Tld.XmlT.SimultaneousAxisControll).ToArray();

							using (_main_ncspd main = new _main_ncspd(
								ncd.nnam, disp_message, Directory.GetCurrentDirectory(), ncd.Ncdata.ncInfo.xmlD.CamDimension, angle, feedrate, simult, ncd.nggt)) {

								Application.DoEvents();
								try { main.ReadLine_File(); }
								catch {
									sw.Flush();
									sw.Close();
									throw;
								}
								Application.DoEvents();
							}

							// ///////////////////////////////////////////
							// ＮＣデータの分離移動
							// ///////////////////////////////////////////
							if (ncd.Itdat == 1) {
								if (ncd.Jnno.SameNc == false)
									File.Move(ncd.nnam, dirn + "\\" + ncd.nnam);
								else
									File.Move(ncd.nnam, dirn + "\\" + ncd.nnam + (TlNo + 1).ToString("_000"));
							}
							else
								Bunri(ncd.nnam, ncd.Itdat.Value, dirn, TlNo);

							// ///////////////////////////////////////////
							// 手順書／ＸＭＬの作成
							// ///////////////////////////////////////////
							for (int ii = 0; ii < ncd.Itdat; ii++) {
								TlNo++;
								if (ncd.Itdat == 1 && ncd.Jnno.SameNc == false)
									nnam = ncd.nnam;
								else
									nnam = ncd.nnam + TlNo.ToString("_000");

								if (ncd.Tdat[ii].Output == false) {
									File.Delete(dirn + "\\" + nnam);
									continue;
								}

								// 手順書への出力（移動、ミラーの情報はＮＣデータに反映済みなので出力しない。工具長補正量の情報は入力できない）
								sw.WriteLine("N " + nnam);

								// ＸＭＬの作成
								try {
									System.Xml.XmlDocument ncsDoc;
									ncsDoc = XmlExport(ncd, ii, angle[ii]);
									ncsDoc.Save(dirn + "\\" + nnam + ".xml");
									ncdCount.Add(ncd.nnam);
								}
								catch (Exception ex) {
									MessageBox.Show(ex.Message + "  エラーとなったＸＭＬデータは temp\\tmp.xml に保存しました。", "NcSimul",
										MessageBoxButtons.OK, MessageBoxIcon.Error);
									Application.Exit();
								}
							}
						}
						break;
					default:
						sw.WriteLine(str);
						break;
					}
				}
				if (NcdTool.Tejun.NcList.NcNamsAll.Count != NcNo + 1)
					throw new Exception("aergbaherbgh");
			}

			// ワーク形状のＩＧＥＳデータとＳＴＬデータをコピー
			if (NcdTool.Tejun.NcList.NcNamsAll.Count > 0) {
				string igsName;
				igsName = Path.ChangeExtension(Path.GetFileName(dirn), ".igs");
				if (File.Exists(ServerPC.SvrFldrS + "work\\" + igsName))
					File.Copy(ServerPC.SvrFldrS + "work\\" + igsName, dirn + "\\" + igsName);
				igsName = Path.ChangeExtension(Path.GetFileName(dirn), ".stl");
				if (File.Exists(ServerPC.SvrFldrS + "work\\" + igsName))
					File.Copy(ServerPC.SvrFldrS + "work\\" + igsName, dirn + "\\" + igsName);
			}
		}

		/// <summary>
		/// ＮＣデータの工具単位の分割
		/// </summary>
		/// <param name="nnam">ＮＣデータ名</param>
		/// <param name="itdat">工具数</param>
		/// <param name="dirn">保存するフォルダー</param>
		/// <param name="stNo">開始連番</param>
		/// <returns></returns>
		private void Bunri(string nnam, int itdat, string dirn, int stNo) {
			string key1 = "G100T";
			string ddat, tmpFile;
			StreamReader fp;
			StreamWriter fpw = null;

			try { fp = new StreamReader(nnam); }
			catch { fp = null; }
			if (fp == null)
				throw new Exception(nnam + " can not open.");

			int tno = -1;
			while (fp.EndOfStream != true) {
				ddat = fp.ReadLine();
				if (ddat.IndexOf(key1) >= 0) {
					tno++;
					stNo++;
					if (tno != 0) {
						fpw.WriteLine("M30");
						fpw.WriteLine("%");
						fpw.Close();
					}
					tmpFile = dirn + "\\" + nnam + stNo.ToString("_000");
					fpw = new StreamWriter(tmpFile);
					fpw.WriteLine("%");
					fpw.WriteLine("O0001");
				}
				if (tno >= 0)
					fpw.WriteLine(ddat);
			}
			fp.Close();
			fpw.Close();
			return;
		}
		/// <summary>
		/// ＸＭＬデータのコピー（NcSpeedで使用）
		/// </summary>
		/// <param name="ncd"></param>
		/// <param name="tNo"></param>
		/// <param name="angle"></param>
		private System.Xml.XmlDocument XmlExport(NcName.NcNam ncd, int tNo, Angle3[] angle) {
			System.Xml.XmlNode elm;
			System.Xml.XmlNode elm2;

			// １工具のＸＭＬの取出し
			System.Xml.XmlDocument docn = ncd.Ncdata.ncInfo.XmlExport(tNo);

			// オリジナルのＮＣデータ名（＝ＸＭＬ名）を挿入
			elm = docn.CreateNode(System.Xml.XmlNodeType.Element, "camOriginalNcName", null);
			elm.InnerText = ncd.Ncdata.nnam;
			docn.SelectSingleNode("/NcInfo/NcData").InsertAfter(elm, docn.SelectSingleNode("/NcInfo/NcData/camDataName"));

			// 工具軸角度を入力
			for (int ii = 0; ii < angle.Length; ii++) {
				RotationAxis rota = new RotationAxis(angle[ii]);
				if (ncd.Ncdata.ncInfo.xmlD.CamDimension == 2) {
					elm = docn.CreateNode(System.Xml.XmlNodeType.Element, "EulerAngleZXZ", null);
					elm2 = docn.CreateNode(System.Xml.XmlNodeType.Element, "A", null);
					elm2.InnerText = rota.Euler_ZXZ().DegA.ToString("0.0####");
					elm.AppendChild(elm2);
					elm2 = docn.CreateNode(System.Xml.XmlNodeType.Element, "B", null);
					elm2.InnerText = rota.Euler_ZXZ().DegB.ToString("0.0####");
					elm.AppendChild(elm2);
					elm2 = docn.CreateNode(System.Xml.XmlNodeType.Element, "C", null);
					elm2.InnerText = rota.Euler_ZXZ().DegC.ToString("0.0####");
					elm.AppendChild(elm2);
				}
				else {
					elm = docn.CreateNode(System.Xml.XmlNodeType.Element, "RotaryAxisAngle", null);
					elm2 = docn.CreateNode(System.Xml.XmlNodeType.Element, "B", null);
					elm2.InnerText = rota.DMU_BC().DegB.ToString("0.0####");
					elm.AppendChild(elm2);
					elm2 = docn.CreateNode(System.Xml.XmlNodeType.Element, "C", null);
					elm2.InnerText = rota.DMU_BC().DegC.ToString("0.0####");
					elm.AppendChild(elm2);
				}
				docn.SelectSingleNode("/NcInfo/Tool/Axis[" + (ii + 1).ToString() + "]")
					.InsertAfter(elm, docn.SelectSingleNode("/NcInfo/Tool/Axis[" + (ii + 1).ToString() + "]/MachiningAxis"));
			}

			// ＣＬ移動量を入力
			if (ncd.Ncdata.ncInfo.ncinfoSchemaVer.Older("V09")) { ;}
			else {
				if (ncd.Itdat == 1 && (ncd.nggt.trns != Vector3.v0 || ncd.nggt.ToolLengthHosei.Zero != true)) {
					if (docn.SelectSingleNode("/NcInfo/Tool/kotei").SelectNodes("ClMovingDistance").Count == 0) {
						docn.SelectSingleNode("/NcInfo/Tool/kotei").InsertAfter(
							docn.CreateNode(System.Xml.XmlNodeType.Element, "ClMovingDistance", null),
							docn.SelectSingleNode("/NcInfo/Tool/kotei/ToolReferencePoint"));
					}
					elm = docn.SelectSingleNode("/NcInfo/Tool/kotei/ClMovingDistance");

					if (ncd.nggt.trns.X != 0.0) {
						elm2 = docn.CreateNode(System.Xml.XmlNodeType.Element, "X_axis", null);
						elm2.InnerText = ncd.nggt.trns.X.ToString("0.000");
						elm.InsertAfter(elm2, elm.LastChild);
					}
					if (ncd.nggt.trns.Y != 0.0) {
						elm2 = docn.CreateNode(System.Xml.XmlNodeType.Element, "Y_axis", null);
						elm2.InnerText = ncd.nggt.trns.Y.ToString("0.000");
						elm.InsertAfter(elm2, elm.LastChild);
					}
					if (ncd.nggt.trns.Z != 0.0) {
						elm2 = docn.CreateNode(System.Xml.XmlNodeType.Element, "Z_axis", null);
						elm2.InnerText = ncd.nggt.trns.Z.ToString("0.000");
						elm.InsertAfter(elm2, elm.LastChild);
					}
					if (ncd.nggt.ToolLengthHosei.Zero != true) {
						throw new Exception("工具長補正はシミュレーションでは機能しないため許可されていません。");
						/*
						elm2 = docn.CreateNode(System.Xml.XmlNodeType.Element, "T_axis", null);
						elm2.InnerText = ncd.nggt.ToolLengthHoseiNew.ValueHosei().ToString("0.000");
						elm.InsertAfter(elm2, elm.LastChild);
						*/
					}
				}
			}

			// スキーマによる検証
			CamUtil.CamNcD.XmlNavi.SchemaVersion.XmlCheck(docn);

			return docn;
		}
	}
}
