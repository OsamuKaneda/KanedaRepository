using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using CamUtil;
using CamUtil.CamNcD;

namespace NCSEND2.NCINFO
{
	partial class NcInfoCam : NcInfo
	{
		/// <summary>ＣＡＭの次元。"2" OR "3"</summary>
		protected string Dim {
			set { if (value == "2" || value == "3") m_dim = value; else throw new Exception("dim Program Error in NcInfoCam"); }
			get { if (m_dim == "2" || m_dim == "3") return m_dim; else throw new Exception("dim Program Error in NcInfoCam"); }
		}
		private string m_dim;
		/// <summary>ＮＣデータ内の工具本数（ＸＭＬ作成時に使用する）</summary>
		protected int ToolCountTemp;

		// //////////////////////////////////
		// 各種ノードの追加メソッド
		// //////////////////////////////////

		// ノードの追加（XmlDeclaration）
		protected XmlNode AddXmlDeclaration() {
			XmlNode child = xmlDoc.CreateXmlDeclaration("1.0", "Shift_JIS", null);
			xmlDoc.AppendChild((XmlNode)child);
			return xmlDoc.LastChild;
		}
		// ノードの追加（XmlElement simpleType）
		protected XmlNode AddXmlElementsimpleType(XmlNode xmlNode, string localname, string value, string isNull) {
			switch (Form1.SelCamSystem.Name) {
			// ＴＥＢＩＳの場合
			case CamUtil.CamSystem.Tebis:
				if (value == "NONE") value = "";
				break;
			// ＤＹＮＡＶＩＳＴＡの場合
			case CamUtil.CamSystem.Dynavista2D:
				if (value == "-") value = "";
				break;
			}

			if (value == "") {
				if (isNull == "") return null;
				value = isNull;
			}

			XmlNode child = xmlDoc.CreateElement(localname);
			xmlNode.AppendChild((XmlNode)child);
			XmlNode child2 = xmlDoc.CreateTextNode(value);
			xmlNode.LastChild.AppendChild((XmlNode)child2);
			return xmlNode.LastChild;
		}
		// ノードの追加（XmlElement complexType）
		protected XmlNode AddXmlElementcomplexType(XmlNode xmlNode, string localname) {
			XmlNode child = xmlDoc.CreateElement(localname);
			xmlNode.AppendChild((XmlNode)child);
			return xmlNode.LastChild;
		}
		// ノードの追加（Attribute）
		private XmlNode AddAttribute(XmlNode xmlNode, string localname, string value) {
			XmlAttribute child = xmlDoc.CreateAttribute(localname);
			child.Value = value;
			xmlNode.Attributes.Append(child);
			return xmlNode.Attributes[localname];
		}

		// /////////////////////////
		// ＸＭＬノードの階層の表示
		// /////////////////////////
		protected void NodePrint() {
			NodePrintsub((XmlNode)xmlDoc, 0);
		}
		private void NodePrintsub(XmlNode xmlNode, int spc) {
			const int tabsp = 2;
			//Display the contents of the child nodes.
			for (int ii = 0; ii < spc; ii++) Console.Write(" ");
			Console.WriteLine(
				$"type ={xmlNode.NodeType}  name ={xmlNode.LocalName}  ns ={xmlNode.NamespaceURI}  count ={xmlNode.ChildNodes.Count.ToString()}  value={xmlNode.Value}");
			if (xmlNode.Attributes != null)
				for (int jj = 0; jj < xmlNode.Attributes.Count; jj++) {
					for (int ii = 0; ii < spc + tabsp; ii++) Console.Write(" ");
					Console.WriteLine(
						$"type ={xmlNode.Attributes[jj].NodeType}  name ={xmlNode.Attributes[jj].LocalName}  value={xmlNode.Attributes[jj].Value}");
				}


			// 子ノードの処理
			for (int ii = 0; ii < xmlNode.ChildNodes.Count; ii++) {
				NodePrintsub(xmlNode.ChildNodes[ii], spc + tabsp);
			}
		}

		/// <summary>
		/// ＸＭＬデータの設定（NcData）
		/// </summary>
		/// <param name="kyd2"></param>
		protected void AddXmlNode_NcInfo(KYDATA.KyData kyd2) {

			XmlNode tmpNode1, tmpNode2;

			string[] orgn = kyd2.GetStringItem(KYDATA.ItemNcdt.OriginXYZ).Split(new char[] { ',' });

			/**********************************************************************
			 * AddXmlElementsimpleTypeに "NONE"を代入すると、本当に何も代入されない
			 * ようである。注意！！
			***********************************************************************/

			if (kyd2.GetStringItem(KYDATA.ItemNcdt.ProcessName) == null) throw new Exception("ＸＭＬ用出力用の文字がnullである");
			if (kyd2.GetStringItem(KYDATA.ItemNcdt.PartOperationName) == null) throw new Exception("ＸＭＬ用出力用の文字がnullである");
			if (kyd2.GetStringItem(KYDATA.ItemNcdt.ProductsName) == null) throw new Exception("ＸＭＬ用出力用の文字がnullである");
			if (kyd2.GetStringItem(KYDATA.ItemNcdt.PostProcessorName) == null) throw new Exception("ＸＭＬ用出力用の文字がnullである");
			if (kyd2.GetStringItem(KYDATA.ItemNcdt.camMaterial) == null) throw new Exception("ＸＭＬ用出力用の文字がnullである");
			if (orgn.Length < 3) throw new Exception("ＸＭＬ用出力用の文字がnullである");
			if (orgn[0] == null || orgn[1] == null || orgn[2] == null) throw new Exception("ＸＭＬ用出力用の文字がnullである");
			//if (cord == null) throw new Exception("");
			//if (pver == null) throw new Exception("");
			//if (head == null) throw new Exception("");

			tmpNode1 = xmlDoc.SelectSingleNode("NcInfo");
			tmpNode1 = xmlDoc.LastChild;

			AddXmlElementsimpleType(tmpNode1, "NcInfoVersion", ncinfoSchemaVer.Name, "");
			if (ProgVersion.Debug)
				AddXmlElementsimpleType(tmpNode1, "XmlOutputDateTime", XmlConvert.ToString(Convert.ToDateTime("2020/1/1 8:30"), XmlDateTimeSerializationMode.Local), "");
			else
				AddXmlElementsimpleType(tmpNode1, "XmlOutputDateTime", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Local), "");
			{
				tmpNode2 = AddXmlElementcomplexType(tmpNode1, "NcData");
				AddXmlElementsimpleType(tmpNode2, "camDataName", Path.GetFileNameWithoutExtension(FullNameCam), "");
				AddXmlElementsimpleType(tmpNode2, "camDataUri", "file://" + LocalHost.IPAddress + "/" + CamUtil.UniversalName.GetUniversalName(FullNameCam).Replace('\\', '/'), "");
				AddXmlElementsimpleType(tmpNode2, "camSystemName", Form1.SelCamSystem.Name, "");

				AddXmlElementsimpleType(tmpNode2, "ProcessName", strLengthErr.MaxRemove(kyd2.GetStringItem(KYDATA.ItemNcdt.ProcessName), "ProcsName"), "");
				AddXmlElementsimpleType(tmpNode2, "PartOperationName", kyd2.GetStringItem(KYDATA.ItemNcdt.PartOperationName), "");
				AddXmlElementsimpleType(tmpNode2, "ProductsName", kyd2.GetStringItem(KYDATA.ItemNcdt.ProductsName), "");
				AddXmlElementsimpleType(tmpNode2, "CoordinatesName", strLengthErr.MaxRemove(kyd2.GetStringItem(KYDATA.ItemNcdt.CoordinatesName), "CoordName"), "");
				AddXmlElementsimpleType(tmpNode2, "PostProcessorName", kyd2.GetStringItem(KYDATA.ItemNcdt.PostProcessorName), "");
				if (kyd2.GetStringItem(KYDATA.ItemNcdt.PostProcessorVersion) != null) AddXmlElementsimpleType(tmpNode2, "PostProcessorVersion", kyd2.GetStringItem(KYDATA.ItemNcdt.PostProcessorVersion), "");
				if (kyd2.GetStringItem(KYDATA.ItemNcdt.machineHeadName) != null) AddXmlElementsimpleType(tmpNode2, "MachineHeadName", kyd2.GetStringItem(KYDATA.ItemNcdt.machineHeadName), "");
				AddXmlElementsimpleType(tmpNode2, "OriginX", orgn[0], "");
				AddXmlElementsimpleType(tmpNode2, "OriginY", orgn[1], "");
				AddXmlElementsimpleType(tmpNode2, "OriginZ", orgn[2], "");
				AddXmlElementsimpleType(tmpNode2, "camDimension", Dim, "");
				AddXmlElementsimpleType(tmpNode2, "camOutputDateTime", XmlConvert.ToString(kyd2.CamOutputDate, XmlDateTimeSerializationMode.Local), "");
				AddXmlElementsimpleType(tmpNode2, "camMaterial", kyd2.GetStringItem(KYDATA.ItemNcdt.camMaterial), "");
			}
		}

		/// <summary>
		/// ＸＭＬに保存する工具の情報
		/// </summary>
		protected readonly struct ToolD
		{
			private readonly string toolSetName;
			private readonly string toolTypeCam;
			private readonly string processDiscrimination;

			// 以下はチェックのみに使用
			public readonly double ToolDiameter;
			private readonly double toolCornerRadius;
			//private string HolderName;
			private readonly double? toolDiameter_in;   // 工具内径

			/// <summary>
			/// 唯一のコンストラクタ
			/// </summary>
			/// <param name="kyd2"></param>
			/// <param name="tset"></param>
			public ToolD(KYDATA.KyData kyd2, NCINFO.TSetCAM tset) {
				this.toolSetName = kyd2.GetStringItem(KYDATA.ItemTool.ToolSetName, tset);
				this.toolTypeCam = kyd2.GetStringItem(KYDATA.ItemTool.ToolTypeCam, tset);
				this.processDiscrimination = kyd2.GetStringItem(KYDATA.ItemTool.ProcessDiscrimination, tset);

				this.ToolDiameter = kyd2.GetDoubleItem(KYDATA.ItemTool.ToolDiameter, tset).Value;
				this.toolCornerRadius = kyd2.GetDoubleItem(KYDATA.ItemTool.ToolCornerRadius, tset).Value;
				this.toolDiameter_in = kyd2.GetDoubleItem(KYDATA.ItemTool.ToolDiameter_in, tset);
			}



			public NcInfoCam.LvmMess ErrorCheck(NcInfoCam.LvmMess m_LvmMess0, TSetCAM tset) {
				/*
					ヘリカルSS1(2)チップのノーズRのCAM上登録値と実物が異なっていることが判明しました(下記表)。
					直径	実物	工具DB		主型CAM		部品CAM
					φ12	R0.3	R0.3 ○		R0.3 ○		R0.3 ○
					φ16	R0.3	R1.0 ×		R1.0 ×		R0.8 ×
					φ20	R0.3	R1.0 ×		R1.0 ×		R0.3 ○
					φ25	R0.3	R1.0 ×		R1.0 ×		-
					φ30	R0.3	R1.0 ×		R1.0 ×		-
				*/

				if (Math.Abs(ToolDiameter - tset.toolsetTemp.Diam) > 0.001) {
					return new NcInfoCam.LvmMess(
						NcInfoCam.LVM.エラー,
						$"工具径が一致しない CAM={ToolDiameter.ToString("#.00")} DB={tset.toolsetTemp.Diam.ToString("#.00")}");
				}
				if (Math.Abs(toolCornerRadius - tset.toolsetTemp.Crad) > 0.001) {
					return new NcInfoCam.LvmMess(
						NcInfoCam.LVM.エラー,
						$"工具のコーナー半径が一致しない CAM={toolCornerRadius.ToString("#.00")} DB={tset.toolsetTemp.Crad.ToString("#.00")}");
				}
				switch (tset.toolsetTemp.CutterTypeCaelum) {
				case "STY":
				case "CDR":
				case "DRL":
				case "MIL":
				case "CSN":
				case "SSN":
				case "SNK":
				case "TAP":
				case "BOR":
				case "REM":
					break;
				default:
					return new NcInfoCam.LvmMess(
						NcInfoCam.LVM.エラー,
						"ケーラム工具タイプが異常値=" + tset.toolsetTemp.CutterTypeCaelum);
				}

				// ＣＡＭシステムごとのエラーチェック
				switch (Form1.SelCamSystem.Name) {
				case CamUtil.CamSystem.Dynavista2D:
					if (toolDiameter_in.HasValue) {
						if (tset.toolsetTemp.Diam_in == null) throw new Exception("jaerbgfqwerhbh");
						if (Math.Abs(toolDiameter_in.Value - tset.toolsetTemp.Diam_in.Value) > 0.001) {
							return new NcInfoCam.LvmMess(
								NcInfoCam.LVM.エラー,
								"面取り工具の先端径が一致しない");
						}
					}
					break;
				default:
					break;
				}

				return m_LvmMess0;
			}

			public void Output(NcInfoCam ncInfo, XmlNode tmpNode3) {
				ncInfo.AddXmlElementsimpleType(tmpNode3, "ToolSetName", toolSetName, "");
				ncInfo.AddXmlElementsimpleType(tmpNode3, "ToolTypeCam", toolTypeCam, "");
				ncInfo.AddXmlElementsimpleType(tmpNode3, "ProcessDiscrimination", processDiscrimination, "");
			}
		}

		/// <summary>
		/// ＸＭＬに保存する工程の情報
		/// </summary>
		protected struct Kotei
		{
			// first
			private readonly string m_Name;
			private readonly string m_Type;
			private readonly string m_Class;
			private readonly string m_CuttingDirection;
			private readonly string m_MachiningMethod;
			private readonly string m_NcCycle;
			private readonly string m_PostComment;
			// max
			private readonly double m_Tolerance;
			private readonly double m_PickZ;
			private readonly double m_PickX;
			private readonly double m_ToolUsableLength;
			// sum
			private readonly double m_FeedLength;
			private readonly double m_NonFeedLength;
			private readonly double m_FeedTime;
			private readonly double m_NonFeedTime;
			// same
			private readonly double? m_CuttingFeedRate_CSV;
			private readonly double? m_SpindleSpeed_CSV;
			private readonly string m_ToolReferencePoint;
			// DB
			private readonly double m_CuttingFeedRate;
			private readonly double m_SpindleSpeed;
			// else
			private readonly double? m_WallThicknessZ;
			private readonly double? m_WallThicknessX;
			private readonly bool taoreHosei;
			private readonly List<FeedElse> feedelse;
			/// <summary></summary>
			private readonly struct FeedElse
			{
				public readonly string type, fix, speed;
				public FeedElse(string t, string f, string s) { type = t; fix = f; speed = s; }
			}

			public Kotei(KYDATA.KyData[] ff, ToolD told, TSetCAM tset) {
				IEnumerable<int> ii;
				if (ff.Length == 0) throw new Exception("");
				if ((ii = ff.Select(kyd2 => (int)Math.Round(kyd2.GetDoubleItem(KYDATA.ItemKotei.CuttingFeedRate_CSV, tset, 0).Value)).Distinct()).Count() != 1)
					throw new Exception($"工程間で基準送り速度が異なる {ii.First().ToString()}mm/min, {ii.Last().ToString()}mm/min");
				if ((ii = ff.Select(kyd2 => (int)Math.Round(kyd2.GetDoubleItem(KYDATA.ItemKotei.SpindleSpeed_CSV, tset, 0).Value)).Distinct()).Count() != 1)
					throw new Exception($"工程間で回転数が異なる {ii.First().ToString()}min-1, {ii.Last().ToString()}min-1");
				if (ff.Select(kyd2 => kyd2.GetStringItem(KYDATA.ItemKotei.ToolReferencePoint, tset, 0)).Distinct().Count() != 1)
					throw new Exception("ToolReferencePoint Error");

				// first
				m_Name = ff[0].GetStringItem(KYDATA.ItemKotei.Name, tset, 0);
				m_Type = ff[0].GetStringItem(KYDATA.ItemKotei.Type, tset, 0);
				m_Class = ff[0].GetStringItem(KYDATA.ItemKotei.Class, tset, 0);
				m_CuttingDirection = ff[0].GetStringItem(KYDATA.ItemKotei.CuttingDirection, tset, 0);
				m_MachiningMethod = ff[0].GetStringItem(KYDATA.ItemKotei.MachiningMethod, tset, 0);
				m_NcCycle = ff[0].GetStringItem(KYDATA.ItemKotei.NcCycle, tset, 0);
				m_PostComment = ff[0].GetStringItem(KYDATA.ItemKotei.PostComment, tset, 0);

				// max
				m_Tolerance = ff.Select(kyd2 => kyd2.GetDoubleItem(KYDATA.ItemKotei.Tolerance, tset, 0) ?? 0.0).Max();
				m_PickZ = ff.Select(kyd2 => kyd2.GetDoubleItem(KYDATA.ItemKotei.PickZ, tset, 0) ?? 0.0).Max();
				m_PickX = ff.Select(kyd2 => kyd2.GetDoubleItem(KYDATA.ItemKotei.PickX, tset, 0) ?? 0.0).Max();
				// NCSPEEDでは首下長が工具半径以下であるとエラーとなるため、ここで余裕をみて工具半径の１．５倍を最小値として設定する
				// （坂本からの要望 2012/12/04）
				m_ToolUsableLength = ff.Select(kyd2 => kyd2.GetDoubleItem(KYDATA.ItemKotei.ToolUsableLength, tset, 0).Value).Max();
				m_ToolUsableLength = NcInfo.ToolULength(m_ToolUsableLength, told.ToolDiameter);

				// sum
				m_FeedLength = ff.Select(kyd2 => kyd2.GetDoubleItem(KYDATA.ItemKotei.FeedLength, tset, 0).Value).Sum();
				m_NonFeedLength = ff.Select(kyd2 => kyd2.GetDoubleItem(KYDATA.ItemKotei.NonFeedLength, tset, 0).Value).Sum();
				m_FeedTime = ff.Select(kyd2 => kyd2.GetDoubleItem(KYDATA.ItemKotei.FeedTime, tset, 0).Value).Sum();
				m_NonFeedTime = ff.Select(kyd2 => kyd2.GetDoubleItem(KYDATA.ItemKotei.NonFeedTime, tset, 0).Value).Sum();

				// same
				m_CuttingFeedRate_CSV = ff[0].GetDoubleItem(KYDATA.ItemKotei.CuttingFeedRate_CSV, tset, 0).Value;
				m_SpindleSpeed_CSV = ff[0].GetDoubleItem(KYDATA.ItemKotei.SpindleSpeed_CSV, tset, 0).Value;
				m_ToolReferencePoint = ff[0].GetStringItem(KYDATA.ItemKotei.ToolReferencePoint, tset, 0);

				// 直接ＤＢ条件にするように変更 2011/10/11
				m_CuttingFeedRate = tset.Feedrate;
				m_SpindleSpeed = tset.Spin;

				// else
				if (ff.Any(kyd => kyd.TaoreList.First().HasValue) && ff[0].Dimension == "3") {
					// いずれかが倒れ補正範囲であれば、同一の値以外はエラーとする in 2019/04/05
					m_WallThicknessZ = ff[0].GetDoubleItem(KYDATA.ItemKotei.WallThicknessZ, tset, 0);
					m_WallThicknessX = ff[0].GetDoubleItem(KYDATA.ItemKotei.WallThicknessX, tset, 0);
					taoreHosei = true;
					List<double?> taore = ff.Select(kyd => kyd.TaoreList.First()).Distinct().ToList();
					if (taore.Count != 1) {
						if (CamUtil.ProgVersion.Debug)
							MessageBox.Show($"debug : 工程間で倒れ補正の値が異なる {taore[0] ?? 0.0:f3}, {taore[1] ?? 0.0:f3}");
						else
							throw new Exception($"工程間で倒れ補正の値が異なる {taore[0] ?? 0.0:f3}, {taore[1] ?? 0.0:f3}");
					}
				}
				else {
					// 倒れ補正ではないため絶対値が最大の値を代入する
					m_WallThicknessZ = null;
					m_WallThicknessX = null;
					taoreHosei = false;
					foreach (KYDATA.KyData kyd in ff) {
						if (kyd.GetDoubleItem(KYDATA.ItemKotei.WallThicknessZ, tset, 0).HasValue)
							if (Math.Abs(m_WallThicknessZ ?? 0.0) <= Math.Abs(kyd.GetDoubleItem(KYDATA.ItemKotei.WallThicknessZ, tset, 0).Value))
								m_WallThicknessZ = kyd.GetDoubleItem(KYDATA.ItemKotei.WallThicknessZ, tset, 0).Value;
						if (kyd.GetDoubleItem(KYDATA.ItemKotei.WallThicknessX, tset, 0).HasValue)
							if (Math.Abs(m_WallThicknessX ?? 0.0) <= Math.Abs(kyd.GetDoubleItem(KYDATA.ItemKotei.WallThicknessX, tset, 0).Value))
								m_WallThicknessX = kyd.GetDoubleItem(KYDATA.ItemKotei.WallThicknessX, tset, 0).Value;
					}
				}

				// ////////////////////////////
				// 送り速度のリストを作成する
				// ////////////////////////////
				feedelse = new List<Kotei.FeedElse>();
				foreach (KYDATA.KyData kyd2 in ff) {
					string ss;
					if ((ss = kyd2.GetStringItem(KYDATA.ItemKotei.Approach, tset, 0)) != null) {
						Kotei.FeedElse ftmp = new FeedElse("approach", "false", ss);
						if (feedelse.All(feed => !feed.Equals(ftmp))) feedelse.Add(ftmp);
					}
					if ((ss = kyd2.GetStringItem(KYDATA.ItemKotei.Retract, tset, 0)) != null) {
						Kotei.FeedElse ftmp = new FeedElse("retract", "false", ss);
						if (feedelse.All(feed => !feed.Equals(ftmp))) feedelse.Add(ftmp);
					}
				}
				if (feedelse.Count == 0) feedelse = null;
			}

			public void Output(NcInfoCam ncInfo, XmlNode tmpNode3, double clearancePlane, CamUtil.StringLengthDB strLengthErr, bool buhin) {
				ncInfo.AddXmlElementsimpleType(tmpNode3, "Name", m_Name, "");
				if (m_Type != null)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "Type", m_Type, "");
				if (m_Class != null)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "Class", m_Class, "");
				if (m_CuttingDirection != null)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "CuttingDirection", m_CuttingDirection, "");
				if (m_MachiningMethod != null)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "MachiningMethod", m_MachiningMethod, "");
				if (m_NcCycle != null)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "NcCycle", m_NcCycle, "");
				if (m_Tolerance != 0.0)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "Tolerance", m_Tolerance.ToString(), "");

				if (m_PickZ != 0.0)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "PickZ", m_PickZ.ToString(), "");
				if (m_PickX != 0.0)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "PickX", m_PickX.ToString(), "");
				if (m_WallThicknessZ != null)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "WallThicknessZ", m_WallThicknessZ.Value.ToString(), "");
				if (m_WallThicknessX != null)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "WallThicknessX", m_WallThicknessX.Value.ToString(), "");

				ncInfo.AddXmlElementsimpleType(tmpNode3, "ClearancePlane", clearancePlane.ToString("0.0##"), "");

				ncInfo.AddXmlElementsimpleType(tmpNode3, "FeedLength", Math.Round(m_FeedLength, 3).ToString(), "");
				ncInfo.AddXmlElementsimpleType(tmpNode3, "NonFeedLength", Math.Round(m_NonFeedLength, 3).ToString(), "");
				ncInfo.AddXmlElementsimpleType(tmpNode3, "FeedTime", Math.Round(m_FeedTime, 3).ToString(), "");
				ncInfo.AddXmlElementsimpleType(tmpNode3, "NonFeedTime", Math.Round(m_NonFeedTime, 3).ToString(), "");

				ncInfo.AddXmlElementsimpleType(tmpNode3, "CuttingFeedRate", m_CuttingFeedRate.ToString(), "");
				ncInfo.AddXmlElementsimpleType(tmpNode3, "SpindleSpeed", m_SpindleSpeed.ToString(), "");

				ncInfo.AddXmlElementsimpleType(tmpNode3, "ToolUsableLength", m_ToolUsableLength.ToString(), "");
				ncInfo.AddXmlElementsimpleType(tmpNode3, "ToolReferencePoint", m_ToolReferencePoint, "");
				if (m_PostComment != null)
					ncInfo.AddXmlElementsimpleType(tmpNode3, "PostComment", strLengthErr.MaxRemove(m_PostComment, "CamCommnt"), "");
				if (ncInfo.ncinfoSchemaVer.Older("V09")) {; }
				else {
					// 倒れ補正のためのオフセットの場合、Offsetにその値を入力する（部品 or １ＮＣデータ１工具のみ対応）
					if (taoreHosei) {
						if (buhin || ncInfo.ToolCountTemp == 1) {
							XmlNode tmpNode4 = ncInfo.AddXmlElementcomplexType(tmpNode3, "ClMovingDistance");
							ncInfo.AddXmlElementsimpleType(tmpNode4, "Offset", ((double)m_WallThicknessX.Value).ToString("0.000"), "");
						}
					}
				}
				if (feedelse != null) {
					XmlNode tmpNode4, tmpNode5;
					tmpNode4 = ncInfo.AddXmlElementcomplexType(tmpNode3, "FeedElse");
					for (int ii = 0; ii < feedelse.Count; ii++) {
						tmpNode5 = ncInfo.AddXmlElementcomplexType(tmpNode4, "FeedRate");
						if (ncInfo.ncinfoSchemaVer.Older("V09")) {
							ncInfo.AddAttribute(tmpNode5, "type", feedelse[ii].type);
							ncInfo.AddAttribute(tmpNode5, "fix", feedelse[ii].fix);
							ncInfo.AddXmlElementsimpleType(tmpNode5, "speed", feedelse[ii].speed, "");
							//	break;
						}
						else {
							ncInfo.AddAttribute(tmpNode5, "type", feedelse[ii].type);
							ncInfo.AddAttribute(tmpNode5, "fix", feedelse[ii].fix);
							ncInfo.AddAttribute(tmpNode5, "speed", feedelse[ii].speed);
						}
					}
				}
			}
		}

		/// <summary>
		/// ＸＭＬに保存する工具軸の情報
		/// </summary>
		protected struct Axis
		{
			private readonly bool? m_AxisControlledMotion;
			private readonly double m_ClearancePlane;    // max

			private readonly Angle3.JIKU m_AxisType;
			private readonly string m_AxisAngle;         // same

			// 最大値
			public double ClearancePlane { get { return m_ClearancePlane; } }

			public Axis(KYDATA.KyData[] ff) {
				if (ff.Length == 0) throw new Exception("wergfwegrfbwreh");
				if (ff.Select(kyD => Angle3.JIKU_Type(kyD.GetStringItem(KYDATA.ItemAxis.AxisType, 0))).Distinct().Count() != 1) throw new Exception("AxisTypeNo");
				if (ff.Select(kyD => kyD.GetStringItem(KYDATA.ItemAxis.AxisAngle, 0)).Distinct().Count() != 1) throw new Exception("AxisAngle");
				if (ff.Select(kyD => kyD.GetStringItem(KYDATA.ItemAxis.AxisControlledMotion, 0) == "true").Distinct().Count() != 1) throw new Exception("AxisControlledMotion");
				m_ClearancePlane = ff.Select(kyD => kyD.GetDoubleItem(KYDATA.ItemAxis.ClearancePlane, 0).Value).Max();
				m_AxisType = Angle3.JIKU_Type(ff[0].GetStringItem(KYDATA.ItemAxis.AxisType, 0));
				m_AxisAngle = ff[0].GetStringItem(KYDATA.ItemAxis.AxisAngle, 0);
				m_AxisControlledMotion = ff[0].GetStringItem(KYDATA.ItemAxis.AxisControlledMotion, 0) == "true";
			}

			public void Output(NcInfoCam ncInfo, XmlNode tmpNode3) {
				ncInfo.AddXmlElementsimpleType(tmpNode3, "AxisControlledMotion",
					m_AxisControlledMotion.HasValue ? (m_AxisControlledMotion.Value ? "simultaneous" : "indexed") : "indexed", "");
				ncInfo.AddXmlElementsimpleType(tmpNode3, "ClearancePlane", ClearancePlane.ToString("0.0##"), "");

				XmlNode tmpNode4;
				if (ncInfo.ncinfoSchemaVer.Older("V09")) {
					switch (m_AxisType) {
					case Angle3.JIKU.Euler_XYZ:
					case Angle3.JIKU.Euler_ZXZ:
					case Angle3.JIKU.MCCVG_AC:
					case Angle3.JIKU.Null:
						ncInfo.AddXmlElementsimpleType(tmpNode3, "MachiningAxis", m_AxisAngle, "");
						break;
					case Angle3.JIKU.Spatial:
					case Angle3.JIKU.DMU_BC:
						ncInfo.AddXmlElementsimpleType(tmpNode3, "PlaneSpatial", m_AxisAngle, "");
						break;
					default: throw new Exception("efqbfaherfqefrq");
					}
				}
				else if (ncInfo.ncinfoSchemaVer.Older("V13")) {
					tmpNode4 = ncInfo.AddXmlElementcomplexType(tmpNode3, "MachiningAxis");
					switch (m_AxisType) {
					case Angle3.JIKU.Euler_XYZ:
						ncInfo.AddAttribute(tmpNode4, "type", "Euler_XYZ"); break;
					case Angle3.JIKU.Euler_ZXZ:
						ncInfo.AddAttribute(tmpNode4, "type", "Euler_ZXZ"); break;
					case Angle3.JIKU.MCCVG_AC:
						ncInfo.AddAttribute(tmpNode4, "type", "MCCVG_AC"); break;
					case Angle3.JIKU.Spatial:
						ncInfo.AddAttribute(tmpNode4, "type", "Spatial"); break;
					case Angle3.JIKU.DMU_BC:
						ncInfo.AddAttribute(tmpNode4, "type", "DMU_BC"); break;
					case Angle3.JIKU.Null:
						ncInfo.AddAttribute(tmpNode4, "type", "Null"); break;
					default:
						throw new Exception("efqbfaherfqefrq");
					}
					ncInfo.AddAttribute(tmpNode4, "axis", m_AxisAngle);
				}
				else {
					tmpNode4 = ncInfo.AddXmlElementcomplexType(tmpNode3, "MachiningAxis");
					ncInfo.AddAttribute(tmpNode4, "type", Angle3.JIKU_Name(m_AxisType));
					ncInfo.AddAttribute(tmpNode4, "axis", m_AxisAngle);
				}
			}
			public bool Equals(Axis obj) {
				if (this.m_AxisControlledMotion != obj.m_AxisControlledMotion) return false;
				if (this.m_ClearancePlane != obj.m_ClearancePlane) return false;
				if (this.m_AxisType != obj.m_AxisType) return false;
				if (this.m_AxisAngle != obj.m_AxisAngle) return false;
				return true;
			}
		}
	}
}
