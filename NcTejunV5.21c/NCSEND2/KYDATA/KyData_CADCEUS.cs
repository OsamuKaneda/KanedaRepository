using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Text.RegularExpressions;
using CamUtil;

namespace NCSEND2.KYDATA
{
	/// <summary>
	/// CADCEUS, WorkNC の加工要領書内の情報（１ＮＣデータ１工具が前提）
	/// </summary>
	internal class KyData_CADCEUS : KyData
	{
		private StKyData stKyData;

		/// <summary>
		/// ＣＡＤＣＥＵＳのＮＣデータ情報の取得
		/// </summary>
		/// <param name="fnam">フルファイル名</param>
		public KyData_CADCEUS(string fnam) {
			this.CSV = false;
			this.FulName = fnam;
			this.stKyData = new StKyData(fnam);
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

		/// <summary>TSETCAM名をセットする</summary>
		public void Set_TSETCAM(string name) { stKyData.TSETCAM = name; }


		// ///////////////////////////////////////////////////////////
		// ツールセットを決定するために使用する情報 add in 2019/05/07
		// ///////////////////////////////////////////////////////////
		/// <summary>工具直径</summary>
		public string CsvToolDiam { get { return stKyData.TOOL[1]; } }
		/// <summary>コーナー半径</summary>
		public string CsvToolRadi { get { return stKyData.TOOL[3]; } }

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
			public string[] TOOL;		// 工具名、工具直径、工具半径、コーナー半径
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
					$"   TOOL {TOOL[0]} {TOOL[1]} {TOOL[2]} {TOOL[3]}",
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
			/// ＣＡＤＣＥＵＳのＮＣデータ情報の取得
			/// </summary>
			/// <param name="fnam">フルファイル名</param>
			public StKyData(string fnam) {
				string fulkata = Path.GetDirectoryName(Path.GetDirectoryName(fnam));	// ワークゾーンのファイル名
				string ncname = Path.GetFileNameWithoutExtension(fnam);                 // ＮＣ名

				TOLERAN = null;

				//string fhst = "cd-01";
				StreamReader sr_x = null;

				string[] tmptmp;
				string ddat;
				string[] tmpa, tmpb;

				//POST = new PostProcessor("GENERAL");
				POST = PostProcessor.GetPost(CamSystem.CADCEUS, "GENERAL");

				//####################
				//# journal data get #
				//####################
				//remsh ${fhst} cat $kata/lst/$ncname:t > tmpz$$
				//cat tmpz$$ > qazqaz
				using (StreamReader sr_z = new StreamReader(fulkata + @"\lst\" + ncname, Encoding.Default)) {
					//set tmpa = `grep "  load 'path" tmpz$$ |tail -1 |sed "s/'/ /g" |sed 's:/: :g'`
					tmpa = SrchLine("  load 'path", sr_z, false)
						.Replace('\'', ' ').Replace('/', ' ')
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

					//remsh ${fhst} cat $kata/lst/$tmpa[4] > tmpx$$
					//cat tmpx$$ >> qazqaz
					if (File.Exists(fulkata + @"\lst\" + tmpa[3]))
						sr_x = new StreamReader(fulkata + @"\lst\" + tmpa[3], Encoding.Default);

					//tmp_x = sr_x.tmpFName;
					PRODUCT = tmpa[3];

					//set tmpa = `tail -1 tmpz$$ |awk '{print $6}'`
					tmpa = new string[1];
					tmptmp = SrchLine("END OF ", sr_z, false)
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					if (tmptmp.Length > 5)
						if (tmptmp[5].IndexOf("/") > 0)
							tmpa[0] = tmptmp[5];
					//set tmpa = `echo $tmpa |sed 's:/: :g' |awk '{printf"%s/%s/%s\n",$3,$1,$2}'`
					if (tmpa[0] != null) {
						tmptmp = tmpa[0].Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						tmpa[0] = tmptmp[2] + "/" + tmptmp[0] + "/" + tmptmp[1];
						DATE = Convert.ToDateTime(tmpa[0]);

					}
					else {
						DATE = DateTime.MinValue;
					}

					//set tmpa = `grep "     point qcmpnt" tmpz$$ |tail -1`
					tmpa = SrchLine("     point qcmpnt", sr_z, false)
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					//set tmpb = `grep "     plane qcmpln000001 " tmpz$$ |tail -1`
					tmpb = SrchLine("     plane qcmpln000001 ", sr_z, false)
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					if (tmpb.Length == 7 && tmpb[6].Span("0123456789.") == tmpb[6].Length) {
						ORIGIN = new string[4];
						ORIGIN[0] = tmpa[5];
						ORIGIN[1] = tmpa[6];
						ORIGIN[2] = tmpa[7];
						ORIGIN[3] = tmpb[6];
					}
					else {
						ORIGIN = new string[3];
						ORIGIN[0] = tmpa[5];
						ORIGIN[1] = tmpa[6];
						ORIGIN[2] = tmpa[7];
					}

					//set tmpa = `grep "^  *TYPE  =" tmpz$$ |tail -1`
					tmpa = SrchLine("^  *TYPE  =", sr_z, false)
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					//set tmpb = `grep "^  *DIAMET  =" tmpz$$ |tail -1`
					tmpb = SrchLine("^  *DIAMET  =", sr_z, false)
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					//echo "   TOOL $tmpa[3] $tmpb[3] 0.00  $tmpb[6]"
					TOOL = new string[4];
					TOOL[0] = tmpa[2];
					TOOL[1] = tmpb[2];
					TOOL[2] = "0.00";
					TOOL[3] = tmpb[5];

					KAHO_HO = new string[3];
					KAHO_HO[0] = "---";
					KAHO_HO[1] = "0";
					KAHO_HO[2] = "0";

					//if ( ! -z tmpx$$ ) then
					if (sr_x != null) {
						//set tmpa = `grep "^  *[0-9]*  *pitch " tmpx$$`
						tmpa = SrchLine("^  *[0-9]*  *pitch ", sr_x, false)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						if (tmpa.Length >= 3) {
							PITCH = new string[2];
							PITCH[0] = "0";
							PITCH[1] = tmpa[2];
						}
						else {
							//set tmpa = `grep "^  *[0-9]* .*scan.*pitch " tmpx$$ |sed 's/.*pitch//'`
							tmpa = new string[0];
							ddat = SrchLine("^  *[0-9]* .*scan.*pitch ", sr_x, false);
							if (ddat.Length > 0)
								tmpa = ddat.Substring(ddat.IndexOf("pitch") + 5)
									.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
							if (tmpa.Length == 1) {
								PITCH = new string[2];
								PITCH[0] = tmpa[0];
								PITCH[1] = "0";
							}
							else {
								PITCH = new string[2];
								PITCH[0] = "0";
								PITCH[1] = "0";
							}
						}
						//set tmpa = `grep " [0-9]* *work " tmpx$$`
						tmpa = SrchLine(" [0-9]* *work ", sr_x, false)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						if (tmpa.Length >= 5) {
							if (tmpa[4] == "nocut" && tmpa.Length >= 7) {
								NOKOSI = new string[2];
								NOKOSI[0] = tmpa[6];
								NOKOSI[1] = tmpa[6];
							}
							else {
								NOKOSI = new string[2];
								NOKOSI[0] = tmpa[4];
								NOKOSI[1] = tmpa[4];
							}
						}
						else {
							NOKOSI = new string[2];
							NOKOSI[0] = "0";
							NOKOSI[1] = "0";
						}
						//set tmpa = `grep " 3 *tolera" tmpx$$`
						tmpa = SrchLine(" 3 *tolera", sr_x, false)
							.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						if (tmpa.Length >= 3) {
							TOLERAN = tmpa[2];
						}
					}
					else {
						PITCH = new string[2];
						PITCH[0] = "0";
						PITCH[1] = "0";
						NOKOSI = new string[2];
						NOKOSI[0] = "0";
						NOKOSI[1] = "0";
					}

					NC_NAME = fulkata + @"\nc\" + ncname;
					APRTDST = new string[2];
					APRTDST[0] = "0";
					APRTDST[1] = "0";

					//set tmpa = `grep "     feed " tmpz$$ |tail -1 |sed 's/\.00*//g'`
					tmpa = SrchLine("     feed ", sr_z, false)
						.Replace(".000000", "")
						.Replace(".00000", "")
						.Replace(".0000", "")
						.Replace(".000", "")
						.Replace(".00", "")
						.Replace(".0", "")
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					FEED = tmpa[2];
					APR_SPD = tmpa[3];
					SPN_SPD = "0";  // 情報なし

					//set tmpa = `grep "^ *TL-VCT " tmpz$$ |tail -1 |sed 's/\.//g'`

					//set tmpa = `grep "     vector qcmvec" tmpz$$ |tail -1 |sed 's/\.//g'`
					tmpa = SrchLine("     vector qcmvec", sr_z, false)
						.Replace(" 0.000000", " 00")
						.Replace(".000000", "00")
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					tmpb = SrchLine("^ *TL-VCT ", sr_z, false)
						.Replace(".", "")
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					// 工具軸が"00 00 100"であるかチェック
					if (tmpa.Length == 8) {
						if (tmpa[5] != "00" || tmpa[6] != "00" || tmpa[7] != "100")
							throw new Exception("nawfjknwfjanrfqar");
					}
					else {
						if (tmpb[3] != "00" || tmpb[6] != "00" || tmpb[9] != "100")
							throw new Exception("nawfjknwfjanrfqar");
					}
					ROTARY = new string[3];
					ROTARY[0] = ROTARY[1] = ROTARY[2] = "0.";

					// ////////////////////////////////////////////




					// 新規 2006/06/19
					tmpa = SrchLine(" partno ", sr_z, true)
						.Replace("'", " ")
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					PROJECT = tmpa[2];
					PROJECT = Path.GetFileName(fulkata);    // change in 2012/07/10

					TSETCAM = null;
					METHOD = null;

					FEEDRPD = new string[3];    // [0]:LENGTH(mm), [1]:TIME(min), [1]:RATE(mm/min)
					FEEDRPD[0] = FEEDRPD[1] = "0";
					FEEDRPD[2] = "8000";
					tmpa = SrchLine("CUTTING LENGTH", sr_z, true)
						.Replace("=", " ")
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					tmpb = SrchLine(" CUTTING TIME\\(1\\)", sr_z, true)
						.Replace("=", " ")
						.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					FEEDCUT = new string[2];    // [0]:LENGTH(mm), [1]:TIME(min)
					FEEDCUT[0] = ((double)(Convert.ToDouble(tmpa[7]) * 1000.0)).ToString();
					FEEDCUT[1] = tmpb[7];
					REFP = "Tip";

					//stop:
					if (sr_x != null) sr_x.Close();
				}
				//File.Delete(tmp_x);
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
