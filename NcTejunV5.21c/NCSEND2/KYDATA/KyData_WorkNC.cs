using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Text.RegularExpressions;
using CamUtil;

namespace NCSEND2.KYDATA
{
	/// <summary>
	/// CADCEUS, WorkNC の加工要領書内の情報（１ＮＣデータ１工具が前提）
	/// </summary>
	internal class KyData_WorkNC : KyData
	{
		private StKyData stKyData;

		/// <summary>
		/// ＷＯＲＫＮＣnewのＮＣデータ情報の取得
		/// </summary>
		/// <param name="fnam">フルファイル名</param>
		/// <param name="homep"></param>
		public KyData_WorkNC(string fnam, double[] homep) {
			this.CSV = false;
			stKyData = new StKyData(fnam, homep);
			this.FulName = stKyData.NC_NAME;
		}

		public override string GetStringItem(ItemNcdt item) {
			switch (item) {
			case ItemNcdt.ProcessName:
				return stKyData.PROJECT;
			case ItemNcdt.PartOperationName:
				return stKyData.KAHO_HO[0];
			case ItemNcdt.ProductsName:
				return stKyData.PRODUCT;
			case ItemNcdt.CoordinatesName:
				return stKyData.PRODUCT;
			case ItemNcdt.PostProcessorName:
				return stKyData.POST.Name;
			case ItemNcdt.PostProcessorVersion:
				return null;
			case ItemNcdt.machineHeadName:
				return null;
			case ItemNcdt.OriginXYZ:
				return stKyData.ORIGIN[0] + "," + stKyData.ORIGIN[1] + "," + stKyData.ORIGIN[2];
			case ItemNcdt.camMaterial:
				return "STEEL";
			default: throw new Exception("");
			}
		}
		public override string GetStringItem(ItemTool item, NCINFO.TSetCAM tset) {
			switch (item) {
			case ItemTool.ToolSetName:
				return stKyData.TSETCAM;
			case ItemTool.ToolTypeCam:
				return stKyData.TOOL[0];
			case ItemTool.ProcessDiscrimination:
				return tset.KouteiType;
			case ItemTool.ToolDiameter:
				return stKyData.Diam.ToString();
			case ItemTool.ToolCornerRadius:
				return stKyData.Crad.ToString();
			case ItemTool.ToolDiameter_in:
				return null;
			default: throw new Exception("");
			}
		}
		public override string GetStringItem(ItemKotei item, NCINFO.TSetCAM tset, int index) {
			double ff;
			switch (item) {
			case ItemKotei.Name:
				return stKyData.PRODUCT;
			case ItemKotei.Type:
				return stKyData.METHOD ?? null;
			case ItemKotei.Class:
				return null;
			case ItemKotei.CuttingDirection:
				return stKyData.KAHO_HO[1];
			case ItemKotei.MachiningMethod:
				return stKyData.KAHO_HO[2];
			case ItemKotei.NcCycle:
				return null;
			case ItemKotei.Tolerance:
				return stKyData.TOLERAN ?? null;
			case ItemKotei.PickZ:
				return stKyData.PITCH[1];
			case ItemKotei.PickX:
				return stKyData.PITCH[0];
			case ItemKotei.WallThicknessZ:
				return stKyData.NOKOSI[1];
			case ItemKotei.WallThicknessX:
				return stKyData.NOKOSI[1];
			case ItemKotei.FeedLength:
				return stKyData.FEEDCUT[0];
			case ItemKotei.NonFeedLength:
				return stKyData.FEEDRPD[0];
			case ItemKotei.FeedTime:
				ff = Convert.ToDouble(stKyData.FEEDCUT[1]) * Convert.ToDouble(stKyData.FEED) / tset.Feedrate;
				return ff.ToString();
			case ItemKotei.NonFeedTime:
				ff = Convert.ToDouble(stKyData.FEEDRPD[1])
					- Convert.ToDouble(stKyData.FEEDRPD[0]) / Convert.ToDouble(stKyData.FEEDRPD[2])
					+ Convert.ToDouble(stKyData.FEEDRPD[0]) / CamUtil.LCode.NcLineCode.NcDist.RapidFeed;
				return ff.ToString();
			case ItemKotei.CuttingFeedRate_CSV:
				return stKyData.FEED;
			case ItemKotei.SpindleSpeed_CSV:
				ff = tset.Spin;
				return ff.ToString();
			case ItemKotei.ToolUsableLength:
				return tset.toolsetTemp.ToutLength.ToString();
			case ItemKotei.ToolReferencePoint:
				return stKyData.REFP;
			case ItemKotei.PostComment:
				return null;
			case ItemKotei.Approach:
				return stKyData.APR_SPD;
			case ItemKotei.Retract:
				return null;
			default: throw new Exception("");
			}
		}
		public override string GetStringItem(ItemAxis item, int index) {
			switch (item) {
			case ItemAxis.AxisControlledMotion:
				return null;
			case ItemAxis.ClearancePlane:
				return stKyData.ORIGIN[3];
			case ItemAxis.AxisType:
				if (Convert.ToDouble(stKyData.ROTARY[1]) != 0.0) throw new Exception("nawfjknwfjanrfqar");
				return (Convert.ToDouble(stKyData.ROTARY[0]) != 0.0 || Convert.ToDouble(stKyData.ROTARY[2]) != 0.0) 
					? Angle3.JIKU_Name(Angle3.JIKU.MCCVG_AC) 
					: Angle3.JIKU_Name(Angle3.JIKU.Null);
			case ItemAxis.AxisAngle:
				if (Convert.ToDouble(stKyData.ROTARY[1]) != 0.0) throw new Exception("nawfjknwfjanrfqar");
				return (Convert.ToDouble(stKyData.ROTARY[0]) != 0.0 || Convert.ToDouble(stKyData.ROTARY[2]) != 0.0) 
					? "A" + stKyData.ROTARY[0] + "B0.C" + stKyData.ROTARY[2] 
					: "A0.B0.C0.";
			default: throw new Exception("");
			}
		}
		/// <summary>1 ＮＣデータ出力日</summary>
		public override DateTime CamOutputDate { get { return CamOutputDateSub(this.stKyData.DATE); } }
		/// <summary>ＣＡＭで指定されたToolSetCAM の名称</summary>
		public override string TSName(int index) { return stKyData.TSETCAM; }
		/// <summary>次元を求める</summary>
		public override string Dimension {
			get {
				NCINFO.TSetCAM tset = new NCINFO.TSetCAM(stKyData.TSETCAM);
				if (tset.toolsetTemp.ToolFormType == "FMIL" || tset.toolsetTemp.MeasType == 'F')
					return "2";
				else
					return "3";
			}
		}
		/// <summary>倒れ補正量のリスト</summary>
		public override IEnumerable<double?> TaoreList { get { return new List<double?> { TaoreHoseiRyo(stKyData.NOKOSI[1]) }; } }


		/// <summary>工具単位に分解する</summary>
		public override KyData[] Divide_Tool() { return new KyData[] { this }; }
		/// <summary>軸単位に分解する</summary>
		public override KyData[] Divide_Axis() { return new KyData[] { this }; }

		struct StKyData
		{
			/// <summary>工具の直径</summary>
			public double Diam {
				get {
					if (Convert.ToDouble(TOOL[1]) >= 0.001)
						return Convert.ToDouble(TOOL[1]);
					else
						return 2.0 * Convert.ToDouble(TOOL[2]);
				}
			}
			/// <summary>工具のコーナーＲ</summary>
			public double Crad { get { return Convert.ToDouble(TOOL[3]); } }

			public string PRODUCT;
			public DateTime DATE;
			public string[] ORIGIN;
			public string[] TOOL;
			public string[] KAHO_HO;
			public string[] PITCH;
			public string[] NOKOSI;
			public string TOLERAN;
			public string NC_NAME;
			public string[] APRTDST;
			public string FEED;
			public string APR_SPD;
			public string SPN_SPD;  // ADD in 2009/8/21
									//private string[] TL_VCT;

			// ADD 2006/06/12
			public string TSETCAM;
			public string PROJECT;
			public string METHOD;
			/// <summary>早送り（[0]:LENGTH(mm), [1]:TIME(min)）</summary>
			public string[] FEEDRPD;
			/// <summary>切削送り（[0]:LENGTH(mm), [1]:TIME(min)）</summary>
			public string[] FEEDCUT;
			/// <summary>工具参照点（"Tip" or "Center"）</summary>
			public string REFP;

			// ADD in 2016/06/22
			/// <summary>旋回軸の角度</summary>
			public string[] ROTARY;
			// ADD in 2017/10/07
			/// <summary>ポスト名</summary>
			public PostProcessor POST;

			// /////////
			// 表示
			// ////////
			public void Show() {
				List<string> jou1 = LsOut();

				string qqq = "";
				qqq += "==========================\n";
				foreach (string qaz in jou1)
					qqq += qaz + "\n";
				qqq += "==========================";
				System.Windows.Forms.MessageBox.Show(qqq);

				//tmpa = `grep "^ *FEED " tmp$$`
				Regex re = new Regex("^ *FEED ");
				qqq = null;
				foreach (string qaz in jou1) {
					if (re.IsMatch(qaz) == false) continue;
					qqq = qaz;
				}
				if (qqq == null) {
					System.Windows.Forms.MessageBox.Show("postprocessor not execute");
					return;
				}
			}

			/// <summary>
			/// ストリングリストの出力
			/// </summary>
			/// <returns></returns>
			private List<string> LsOut() {
				string org;
				if (ORIGIN.Length == 4)
					org = $"{ORIGIN[0]} {ORIGIN[1]} {ORIGIN[2]} {ORIGIN[3]}";
				else
					org = $"{ORIGIN[0]} {ORIGIN[1]} {ORIGIN[2]}";

				List<string> sout = new List<string> {
					$"PRODUCT {PRODUCT}",
					$"   DATE {DATE.ToString()}",
					$" ORIGIN {org}",
					$"   TOOL {TOOL[0]} {TOOL[1] } {TOOL[2]} {TOOL[3]}",
					$"KAKO-HO {KAHO_HO[0]} {KAHO_HO[1]} {KAHO_HO[2]}",
					$"  PITCH {PITCH[0]} {PITCH[1]}",
					$" NOKOSI {NOKOSI[0]} {NOKOSI[1]}",
					$"TOLERAN {TOLERAN}",
					$"NC-NAME {NC_NAME}",
					$"APRTDST {APRTDST[0]} {APRTDST[1]}",
					$"   FEED {FEED}",
					$"APR-SPD {APR_SPD}",
					$"SPN-SPD {SPN_SPD}",
					$" ROTARY {ROTARY[0]} {ROTARY[1]} {ROTARY[2]}",
					$"PROJECT {PROJECT}",
					$"TSETCAM {TSETCAM}",
					$" METHOD {METHOD}",
					$"FEEDRPD {FEEDRPD[0]} {FEEDRPD[1]} {FEEDRPD[2]}",
					$"FEEDCUT {FEEDCUT[0]} {FEEDCUT[1]}",
					$"   REFP {REFP}"
				};
				return sout;
			}

			/// <summary>
			/// ＷＯＲＫＮＣnewのＮＣデータ情報の取得
			/// </summary>
			/// <param name="fnam">フルファイル名</param>
			/// <param name="homep"></param>
			public StKyData(string fnam, double[] homep) {
				string fulkata = Path.GetDirectoryName(fnam);			// ワークゾーンのファイル名
				string ncname = Path.GetFileNameWithoutExtension(fnam);	// ＮＣ名

				NC_NAME = null;
				APRTDST = null;
				FEED = null;
				APR_SPD = null;
				SPN_SPD = null;
				TSETCAM = null;
				PROJECT = null;
				METHOD = null;
				FEEDRPD = null;
				FEEDCUT = null;
				REFP = null;
				ROTARY = null;

				System.CodeDom.Compiler.TempFileCollection tmpFile = null;

				//###################
				//# jounal data get #
				//###################
				FileInfo fileInfo = new FileInfo(fulkata + @"\" + ncname + ".jou");
				using (StreamReader sr_x = new StreamReader(fulkata + @"\" + ncname + ".par", Encoding.Default))
				using (StreamReader sr_n = new StreamReader(fulkata + @"\" + ncname + ".jou", Encoding.Default)) {
					string[] tmptmp;
					string ddat;
					string[] tmpa, tmpb, tmpc;
					Regex re;

					// ///////////////////
					// ポスト名の取得 add in 2017/10/07
					// ///////////////////
					using (StreamReader sr_i = new StreamReader(fulkata + @"\" + ncname + ".inf", Encoding.Default)) {
						ddat = sr_i.ReadToEnd();
						if (ddat.IndexOf("m7 gousei5") >= 0) {
							POST = PostProcessor.GetPost(CamSystem.WorkNC, "gousei5");
						}
						else if (ddat.IndexOf("m7 GOUSEI") >= 0) {
							POST = PostProcessor.GetPost(CamSystem.WorkNC, "GOUSEI");
						}
						else {
							POST = PostProcessor.GetPost(PostProcessor.ID.NULL);
						}
					}

					List<string> niv = new List<string>();
					if (File.Exists(fulkata + @"\" + ncname + ".niv")) {
						using (StreamReader sr_y = new StreamReader(fulkata + @"\" + ncname + ".niv", Encoding.Default)) {
							if (sr_y.EndOfStream == false) sr_y.ReadToEnd();
							sr_y.BaseStream.Seek(0, SeekOrigin.Begin);
							while (sr_y.EndOfStream == false) {
								ddat = sr_y.ReadLine();
								tmptmp = ddat.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
								niv.Add(Convert.ToDouble(tmptmp[2]).ToString("0.00##"));
							}
						}
					}

					// ///////////////////
					// 有意情報の抽出(tmpz)
					// ///////////////////
					// 一時ファイルの準備
					tmpFile = new System.CodeDom.Compiler.TempFileCollection(CamUtil.LocalHost.Tempdir, false);
					using (StreamWriter sw = new StreamWriter(tmpFile.AddExtension("tmpz"), false, Encoding.Default)) {
						re = new Regex("^\\| *Toolpath No");
						if (sr_n.EndOfStream == false) sr_n.ReadToEnd();
						sr_n.BaseStream.Seek(0, SeekOrigin.Begin);
						bool tmp1 = false;
						while (!sr_n.EndOfStream) {
							ddat = sr_n.ReadLine();
							if (re.IsMatch(ddat)) tmp1 = true;
							if (tmp1)
								sw.WriteLine(ddat);
						}
					}

					using (StreamReader sr_z = new StreamReader(tmpFile.BasePath + ".tmpz", Encoding.Default)) {

						//#
						//##################
						//# data print out #
						//##################
						//#
						// PRODUCT
						ddat = SrchLine(" Project num", sr_z, true);
						tmpa = ddat
							.Substring(ddat.IndexOf("Project number") + 14)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						PRODUCT = tmpa[1] + "." + ncname.Substring(5);

						// DATE
						ddat = SrchLine("Date :", sr_z, true);
						tmpa = ddat
							.Substring(ddat.IndexOf("Date :") + 6)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						DATE = Convert.ToDateTime(tmpa[1] + " " + tmpa[2] + " " + DateTime.Now.Year + " " + tmpa[3]);
						if (DATE > DateTime.Now)
							DATE = Convert.ToDateTime(tmpa[1] + " " + tmpa[2] + " " + (DateTime.Now.Year - 1) + " " + tmpa[3]);
						// 年が入っていない日付に対応する
						while (DATE > fileInfo.LastWriteTime.AddDays(1)) DATE = DATE.AddYears(-1);

						// ORIGIN
						ORIGIN = new string[4];
						ORIGIN[0] = homep[0].ToString("0.00");
						ORIGIN[1] = homep[1].ToString("0.00");
						ORIGIN[2] = homep[2].ToString("0.00");
						//ORIGIN[3] = homep[2].ToString("0.00");

						// TOOL
						tmpa = SrchLine(" cutter  *Radius ", sr_z, true)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						if (tmpa[1] == "FLATEND") {
							TOOL = new string[4];
							TOOL[0] = tmpa[1];
							TOOL[1] = "0.00";
							TOOL[2] = tmpa[5];
							TOOL[3] = tmpa[9];
						}
						else if (tmpa[1] == "BALLEND") {
							TOOL = new string[4];
							TOOL[0] = tmpa[1];
							TOOL[1] = "0.00";
							TOOL[2] = tmpa[5];
							TOOL[3] = tmpa[5];
						}
						else {
							TOOL = new string[4];
							TOOL[0] = tmpa[1];
							TOOL[1] = "0.00";
							TOOL[2] = tmpa[5];
							TOOL[3] = "0.00";
						}

						// KAHO_HO
						string kakoho = null;
						tmpb = null; tmpc = null;
						ddat = SrchLine("^\\| *Toolpath No ", sr_z, true);
						ddat = ddat.Substring(ddat.IndexOf("Toolpath No") + 11);
						ddat = ddat.Substring(0, ddat.LastIndexOf("|"));
						tmptmp = ddat.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						kakoho = tmptmp[1];
						for (int ii = 2; ii < tmptmp.Length; ii++)
							kakoho += "-" + tmptmp[ii];
						ddat = SrchLine("Cutting Dir\\. ", sr_z, false);
						tmpb = ddat
							.Substring(ddat.IndexOf("Cutting Dir.") + 12)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						tmpc = SrchLine(":  34 ", sr_x, false)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						tmpc[0] = Convert.ToDouble(tmpc[0]).ToString("0");
						KAHO_HO = new string[3];
						KAHO_HO[0] = kakoho;
						KAHO_HO[1] = tmpb[1];
						KAHO_HO[2] = tmpc[0];

						// PITCH
						ddat = SrchLine("^.*Z Step *:", sr_z, false);
						ddat = ddat.Substring(ddat.IndexOf("Z Step") + 6);
						tmpb = ddat.Substring(ddat.IndexOf(":") + 1)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						if (tmpb[0] == "0.00" && niv.Count == 1)
							tmpb = niv.ToArray();
						if (kakoho == "Z-level-finish") {
							tmpa = new string[1];
							tmpa[0] = tmpb[0];
						}
						else {
							//tmpa = `grep "^.*Stepover :" tmpz$$ |sed 's/^.*Stepover ://'`
							ddat = SrchLine("^.*Stepover :", sr_z, false);
							tmpa = ddat
								.Substring(ddat.IndexOf("Stepover :") + 10)
								.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						}
						PITCH = new string[2];
						PITCH[0] = tmpa[0];
						PITCH[1] = tmpb[0];

						// NOKOSI
						ddat = SrchLine("^.*Allowance :", sr_z, false);
						tmpa = ddat
							.Substring(ddat.IndexOf("Allowance :") + 11)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						NOKOSI = new string[2];
						NOKOSI[0] = tmpa[0];
						NOKOSI[1] = tmpa[0];

						// TOLERAN
						ddat = SrchLine("^.*Tolerance *:", sr_z, false);
						ddat = ddat.Substring(ddat.IndexOf("Tolerance") + 9);
						tmpa = ddat.Substring(ddat.IndexOf(":") + 1)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						TOLERAN = tmpa[0];

						// NC_NAME
						tmpa = SrchLine("^\\| Filename : ", sr_z, false)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						if (tmpa.Length != 0) {
							NC_NAME = fulkata + @"\" + tmpa[3];

							// APRTDST
							ddat = SrchLine("^\\| Approach *:", sr_z, false)
								.Substring(10);
							tmpa = ddat.Substring(ddat.IndexOf(":") + 1)
							   .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							//set tmpb = `grep "^.*Retract Dist. *:" tmpz$$ |sed 's/^.*Retract Dist. *://'`
							ddat = SrchLine("^.*Retract Dist. *:", sr_z, false);
							ddat = ddat.Substring(ddat.IndexOf("Retract Dist.") + 13);
							tmpb = ddat.Substring(ddat.IndexOf(":") + 1)
							   .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							APRTDST = new string[2];
							APRTDST[0] = tmpa[0];
							APRTDST[1] = tmpb[0];

							// FEED
							ddat = SrchLine("^.*Cutting Feed. *:", sr_z, false);
							ddat = ddat.Substring(ddat.IndexOf("Cutting Feed.") + 13);
							tmpa = ddat.Substring(ddat.IndexOf(":") + 1)
							   .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							FEED = tmpa[0];

							// APR_SPD
							ddat = SrchLine("^.*Approach Feed. *:", sr_z, false);
							ddat = ddat.Substring(ddat.IndexOf("Approach Feed.") + 14);
							tmpa = ddat.Substring(ddat.IndexOf(":") + 1)
							   .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							APR_SPD = tmpa[0];

							// SPN_SPD
							ddat = SrchLine("^.*Spindle Sp. *:", sr_z, false);
							ddat = ddat.Substring(ddat.IndexOf("Spindle Sp.") + 11);
							tmpa = ddat.Substring(ddat.IndexOf(":") + 1)
							   .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							SPN_SPD = tmpa[0];

							// PROJECT 製品名　add 2009/06/11
							int count = -1;
							re = new Regex("Workzone informations");
							if (sr_z.EndOfStream == false) sr_z.ReadToEnd();
							sr_z.BaseStream.Seek(0, SeekOrigin.Begin);
							tmpa = null;
							while (sr_z.EndOfStream == false) {
								ddat = sr_z.ReadLine();
								if (count >= 0) count++;
								else if (re.IsMatch(ddat)) count = 0;
								if (count != 2) continue;
								tmpa = ddat.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
								break;
							}
							PROJECT = tmpa[1];

							// TSETCAM ツールセットＣＡＭ名　add 2009/06/11
							tmpa = new string[1];
							ddat = SrchLine("^ *82 :", sr_x, true);
							re = new Regex("^ *82 :");
							tmpa[0] = ddat.Substring(re.Match(ddat).Length);
							if (tmpa[0].Length <= 2) {
								ddat = SrchLine("^ *69 :", sr_x, true);
								re = new Regex("^ *69 :");
								tmpa[0] = ddat.Substring(re.Match(ddat).Length);
							}
							TSETCAM = Path.GetFileName(tmpa[0]).Replace(".hld", "");

							// METHOD 加工法　add 2009/06/11
							ddat = SrchLine("Method", sr_z, true);
							tmpa = ddat
								.Substring(ddat.IndexOf("Method") + 6)
								.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							METHOD = tmpa[1];

							// FEEDRPD 加工長、加工時間（ＲＡＰＩＤ）　add 2009/06/11
							ddat = SrchLine("Rapid feedrate", sr_z, true);
							tmpa = ddat
								.Substring(ddat.IndexOf("Rapid feedrate") + 14)
								.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							FEEDRPD = new string[3];
							FEEDRPD[0] = tmpa[1];
							FEEDRPD[1] = tmpa[4];
							tmpc = SrchLine(":  25 ", sr_x, false)
								.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							FEEDRPD[2] = tmpc[0];

							// FEEDCUT 加工長、加工時間（ＣＵＴＴＩＮＧ）　add 2009/06/11
							ddat = SrchLine("General feedrate", sr_z, true);
							tmpa = ddat
								.Substring(ddat.IndexOf("General feedrate") + 16)
								.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							FEEDCUT = new string[2];
							FEEDCUT[0] = tmpa[1];
							FEEDCUT[1] = tmpa[4];

							// REFP 工具参照点　add 2009/06/11
							ddat = SrchLine("Point directed", sr_z, true);
							tmpa = ddat
								.Substring(ddat.IndexOf("Point directed") + 14)
								.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							switch (tmpa[1]) {
							case "TIP":
								REFP = "Tip";
								if (POST.Id == CamUtil.PostProcessor.ID.gousei5) throw new Exception("５軸ポストで工具参照点が先端となっている");
								break;
							case "CENTER":
								REFP = "Center";
								if (POST.Id == CamUtil.PostProcessor.ID.GOUSEI) throw new Exception("３軸ポストで工具参照点が中心となっている");
								break;
							default:
								throw new Exception("工具参照点が'TIP'でも'CENTER'でもない。");
							}

							// CLR クリアランスプレーン　add 2016/06/22
							ddat = SrchLine("Toolpath bounds for this file", sr_z, true);
							ddat = sr_z.ReadLine();
							ddat = sr_z.ReadLine();
							tmpa = ddat
								.Substring(ddat.IndexOf("ZMAX") + 4)
								.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							ORIGIN[3] = tmpa[1];

							// ROTARY 旋回軸　add 2016/06/22
							ddat = SrchLine("Rotary Axis Angle A B C", sr_z, true);
							tmpa = ddat
								.Substring(ddat.IndexOf("A B C") + 5)
								.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							ROTARY = new string[3];
							ROTARY[0] = tmpa[1];
							ROTARY[1] = tmpa[2];
							ROTARY[2] = tmpa[3];
							if (ROTARY[0] != "0.00" || ROTARY[1] != "0.00" || ROTARY[2] != "0.00")
								if (POST.Id == CamUtil.PostProcessor.ID.GOUSEI) throw new Exception("３軸加工であるが工具回転軸がすべて０ではない。");
						}
					}
				}
				if (tmpFile != null) tmpFile.Delete();
				return;
			}

			private static string SrchLine(string re_string, StreamReader sr, bool first) {
				string tmpa = "";
				string ddat;
				Regex re = new Regex(re_string);
				if (sr.EndOfStream == false) sr.ReadToEnd();
				sr.BaseStream.Seek(0, SeekOrigin.Begin);
				while (sr.EndOfStream == false) {
					ddat = sr.ReadLine();
					if (!re.IsMatch(ddat)) continue;
					tmpa = ddat;
					if (first) break;
				}
				return tmpa;
			}
		}
	}
}
