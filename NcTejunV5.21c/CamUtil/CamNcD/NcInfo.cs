using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

namespace CamUtil.CamNcD
{
	/// <summary>
	/// ＮＣデータ個々の情報を管理します。
	/// </summary>
	public class NcInfo
	{
		/// <summary>
		/// NCSPEEDでは首下長が工具半径以下であるとエラーとなるため、
		/// ここで余裕をみて工具半径の１．５倍を最小値として設定する
		/// （坂本からの要望 2012/12/04）
		/// </summary>
		/// <param name="length">指定された突出し量</param>
		/// <param name="diameter">工具直径</param>
		/// <returns>ToolUsableLengthに設定する値</returns>
		protected static double ToolULength(double length, double diameter) {
			return Math.Max(length, 1.5 * diameter / 2.0);
		}



		// ///////////
		// 以上　静的
		// ///////////


		/// <summary>ＸＭＬドキュメントデータ</summary>
		protected readonly XmlDocument xmlDoc;

		/// <summary>読込んだスキーマのバージョン（"V04","V05","V06",...）</summary>
		public readonly XmlNavi.SchemaVersion ncinfoSchemaVer;

		/// <summary>ＸＭＬ内のデータ</summary>
		public XmlNavi xmlD;




		/// <summary>
		/// コンストラクタ（NcInfoCAMで使用）
		/// </summary>
		public NcInfo() {
			// ＸＭＬの初期化
			xmlDoc = new XmlDocument();
			// スキーマの設定
			ncinfoSchemaVer = XmlNavi.SchemaVersion.Latest();
			xmlDoc.Schemas = ncinfoSchemaVer.SchemaSet();
		}
		/// <summary>
		/// コンストラクタ（スキーマによる検証とXPath の作成まで実行）
		/// </summary>
		/// <param name="ncFullName"></param>
		public NcInfo(string ncFullName) {

			// ＸＭＬファイルの読み込み
			xmlDoc = new XmlDocument();
			xmlDoc.Load(Path.ChangeExtension(ncFullName, ".xml"));

			// スキーマの設定
			ncinfoSchemaVer = new XmlNavi.SchemaVersion(xmlDoc);
			xmlDoc.Schemas = ncinfoSchemaVer.SchemaSet();

			// XPath の作成とスキーマによる検証
			xmlD = new XmlNavi(xmlDoc, ncinfoSchemaVer.Name);

			return;
		}






		/// <summary>
		/// ＸＭＬデータの上書き（スキーマ検証しない）
		/// </summary>
		/// <param name="fulname">保存するファイル名</param>
		public void XmlSave(string fulname) {
			xmlDoc.Save(fulname);
		}

		/// <summary>
		/// ＸＭＬデータの加工長、移動範囲の変更
		/// </summary>
		public void XmlEditFeed(int tcnt, LCode.NcLineCode.NcDist kyori) {
			int setJun = tcnt + 1;

			XmlNode xoya, xtul, nodeG00, nodeG01, nodeMax, nodeMin, nodeX, nodeY, nodeZ;

			xoya = xmlDoc.SelectSingleNode("/NcInfo/Tool[" + setJun.ToString() + "]/kotei");
			xtul = xoya.SelectSingleNode("ToolUsableLength");

			// 加工長の設定
			nodeG01 = xoya.SelectSingleNode("FeedLength");
			nodeG01.InnerText = kyori.G01.ToString("0.0");

			if (xoya.SelectNodes("NonFeedLength").Count == 0)
				nodeG00 = xoya.InsertAfter(xmlDoc.CreateElement("NonFeedLength"), nodeG01);
			else
				nodeG00 = xoya.SelectSingleNode("NonFeedLength");
			nodeG00.InnerText = kyori.G00.ToString("0.0");

			// 移動範囲の設定
			if (kyori.Max.HasValue) {
				if (xoya.SelectNodes("MaxXYZ").Count == 0) {
					nodeMax = xoya.InsertAfter(xmlDoc.CreateElement("MaxXYZ"), xtul);
					nodeX = nodeMax.AppendChild(xmlDoc.CreateElement("X"));
					nodeY = nodeMax.AppendChild(xmlDoc.CreateElement("Y"));
					nodeZ = nodeMax.AppendChild(xmlDoc.CreateElement("Z"));
				}
				else {
					nodeMax = xoya.SelectSingleNode("MaxXYZ");
					nodeX = nodeMax.SelectSingleNode("X");
					nodeY = nodeMax.SelectSingleNode("Y");
					nodeZ = nodeMax.SelectSingleNode("Z");
				}
				nodeX.InnerText = kyori.Max.Value.X.ToString("0.000");
				nodeY.InnerText = kyori.Max.Value.Y.ToString("0.000");
				nodeZ.InnerText = kyori.Max.Value.Z.ToString("0.000");
			}
			else
				nodeMax = xtul;

			if (kyori.Min.HasValue) {
				if (xoya.SelectNodes("MinXYZ").Count == 0) {
					nodeMin = xoya.InsertAfter(xmlDoc.CreateElement("MinXYZ"), nodeMax);
					nodeX = nodeMin.AppendChild(xmlDoc.CreateElement("X"));
					nodeY = nodeMin.AppendChild(xmlDoc.CreateElement("Y"));
					nodeZ = nodeMin.AppendChild(xmlDoc.CreateElement("Z"));
				}
				else {
					nodeMin = xoya.SelectSingleNode("MinXYZ");
					nodeX = nodeMin.SelectSingleNode("X");
					nodeY = nodeMin.SelectSingleNode("Y");
					nodeZ = nodeMin.SelectSingleNode("Z");
				}
				nodeX.InnerText = kyori.Min.Value.X.ToString("0.000");
				nodeY.InnerText = kyori.Min.Value.Y.ToString("0.000");
				nodeZ.InnerText = kyori.Min.Value.Z.ToString("0.000");
			}

			// ナビゲータの作成と各種設定（スキーマは検証される）
			xmlD = new XmlNavi(xmlDoc, ncinfoSchemaVer.Name);

			return;
		}

		/// <summary>
		/// ＸＭＬデータのテキスト変更
		/// </summary>
		public void XmlEdit(string xpath, string text) {
			// xpath = "/NcInfo/NcData/CamDataName"
			XmlNode node;
			node = xmlDoc.SelectSingleNode(xpath);
			node.InnerText = text;
			CamUtil.CamNcD.XmlNavi.SchemaVersion.XmlCheck(xmlDoc);
		}

		/// <summary>
		/// ＸＭＬデータの加工時間の変更
		/// </summary>
		public void XmlEditTime(int tcnt, LCode.NcLineCode.NcDist kyori) {
			int setJun = tcnt + 1;

			XmlNode xoya, nodeTim;
			xoya = xmlDoc.SelectSingleNode("/NcInfo/Tool[" + setJun.ToString() + "]/kotei");

			nodeTim = xoya.SelectSingleNode("FeedTime");
			nodeTim.InnerText = kyori.FeedTime.ToString("0.00");
			nodeTim = xoya.SelectSingleNode("NonFeedTime");
			nodeTim.InnerText = kyori.NonFeedTime().ToString("0.00");
			nodeTim = xoya.SelectSingleNode("CuttingTime");
			if (nodeTim == null)
				nodeTim = xoya.InsertAfter(xmlDoc.CreateElement("CuttingTime"), xoya.SelectSingleNode("NonFeedTime"));
			nodeTim.InnerText = kyori.CuttingTime.ToString("0.00");
		}

		/// <summary>
		/// ＸＭＬデータの工具有効長（突出し量）の変更
		/// </summary>
		public void XmlEditULength(int tcnt, double uLength, double diam, bool by_force) {
			int setJun = tcnt + 1;

			XmlNode xoya1, xoya2, nodeUL, nodeOP;
			xoya1 = xmlDoc.SelectSingleNode("/NcInfo/Tool[" + setJun.ToString() + "]");
			xoya2 = xmlDoc.SelectSingleNode("/NcInfo/Tool[" + setJun.ToString() + "]/kotei");

			uLength = NcInfo.ToolULength(uLength, diam);
			nodeUL = xoya2.SelectSingleNode("ToolUsableLength");
			if (by_force) {
				if (xoya1.SelectNodes("OptionalULenSet").Count == 1)
					nodeOP = xoya1.SelectSingleNode("OptionalULenSet");
				else
					nodeOP = xoya1.InsertAfter(xmlDoc.CreateElement("OptionalULenSet"),
						xmlDoc.SelectSingleNode("/NcInfo/Tool[" + setJun.ToString() + "]/ToolData"));
				nodeOP.InnerText = uLength.ToString("0.0");
				nodeUL.InnerText = uLength.ToString("0.0");
			}
			else if (xoya1.SelectNodes("OptionalULenSet").Count == 0)
				nodeUL.InnerText = uLength.ToString("0.0");
		}

		/// <summary>
		/// ＸＭＬデータの回転数任意指示、送り速度任意指示の変更
		/// </summary>
		public void XmlEditSF(int setJun, double valueS, double valueF) {

			XmlNode xoya, next, nodeS, nodeF;
			int iS, iF;

			xoya = xmlDoc.SelectSingleNode("/NcInfo/Tool[" + setJun.ToString() + "]");

			if (ncinfoSchemaVer.Older("V10")) {
				iS = xoya.SelectNodes("OptionalSpinSet").Count;
				iF = xoya.SelectNodes("OptionalFeedSet").Count;
				if (iS != iF) throw new Exception("qwejfbqerfb");
				if (iS == 0) {
					next = xmlDoc.SelectSingleNode("/NcInfo/Tool[" + setJun.ToString() + "]/ToolData");
					nodeS = xoya.InsertAfter(xmlDoc.CreateElement("OptionalSpinSet"), next);
					nodeF = xoya.InsertAfter(xmlDoc.CreateElement("OptionalFeedSet"), nodeS);
				}
				else {
					nodeS = xoya.SelectSingleNode("OptionalSpinSet");
					nodeF = xoya.SelectSingleNode("OptionalFeedSet");
				}
				if (valueS == 1.0 && valueF == 1.0) {
					xoya.RemoveChild(nodeS);
					xoya.RemoveChild(nodeF);
				}
				else {
					nodeS.InnerText = valueS.ToString();
					nodeF.InnerText = valueF.ToString();
				}
			}
			else {
				iS = xoya.SelectNodes("OptionalSpFdSet").Count;
				if (iS == 0) {
					next = xoya.SelectSingleNode("ToolData");
					next = xoya.InsertAfter(xmlDoc.CreateElement("OptionalSpFdSet"), next);
					nodeS = next.AppendChild(xmlDoc.CreateElement("Spin"));
					nodeF = next.AppendChild(xmlDoc.CreateElement("Feed"));
				}
				else {
					next = xoya.SelectSingleNode("OptionalSpFdSet");
					nodeS = next.SelectSingleNode("Spin");
					nodeF = next.SelectSingleNode("Feed");
				}
				if (valueS == 1.0 && valueF == 1.0) {
					xoya.RemoveChild(next);
				}
				else {
					nodeS.InnerText = valueS.ToString();
					nodeF.InnerText = valueF.ToString();
				}
			}

			// ナビゲータの作成と各種設定（スキーマは検証される）
			xmlD = new XmlNavi(xmlDoc, ncinfoSchemaVer.Name);

			return;
		}
		/// <summary>
		/// ＸＭＬデータの寿命長任意指示の変更
		/// </summary>
		public void XmlEditLife(int setJun, double valueL) {

			XmlNode xoya, next, nodeL;
			int iS, iF, iL;

			xoya = xmlDoc.SelectSingleNode("/NcInfo/Tool[" + setJun.ToString() + "]");

			if (ncinfoSchemaVer.Older("V10")) {
				iS = xoya.SelectNodes("OptionalSpinSet").Count;
				iF = xoya.SelectNodes("OptionalFeedSet").Count;
				iL = xoya.SelectNodes("OptionalLifeSet").Count;
				if (iS != iF) throw new Exception("qwejfbqerfb");
				if (iL == 0) {
					if (iS == 0)
						next = xmlDoc.SelectSingleNode("/NcInfo/Tool[" + setJun.ToString() + "]/ToolData");
					else
						next = xoya.SelectSingleNode("OptionalFeedSet");
					nodeL = xoya.InsertAfter(xmlDoc.CreateElement("OptionalLifeSet"), next);
				}
				else
					nodeL = xoya.SelectSingleNode("OptionalLifeSet");
			}
			else {
				iS = xoya.SelectNodes("OptionalSpFdSet").Count;
				iL = xoya.SelectNodes("OptionalLifeSet").Count;
				if (iL == 0) {
					if (iS == 0)
						next = xoya.SelectSingleNode("ToolData");
					else
						next = xoya.SelectSingleNode("OptionalSpFdSet");
					nodeL = xoya.InsertAfter(xmlDoc.CreateElement("OptionalLifeSet"), next);
				}
				else {
					nodeL = xoya.SelectSingleNode("OptionalLifeSet");
				}
			}

			if (valueL == 1.0)
				xoya.RemoveChild(nodeL);
			else
				nodeL.InnerText = valueL.ToString();

			// ナビゲータの作成と各種設定（スキーマは検証される）
			xmlD = new XmlNavi(xmlDoc, ncinfoSchemaVer.Name);

			return;
		}

		/// <summary>
		/// ＸＭＬデータの１工具情報の取出し
		/// </summary>
		/// <param name="tNo"></param>
		public System.Xml.XmlDocument XmlExport(int tNo) {
			System.Xml.XmlDocument xmlDoc1 = this.xmlDoc;
			System.Xml.XmlDocument docn = new System.Xml.XmlDocument(xmlDoc1.NameTable);

			docn.Schemas = xmlDoc1.Schemas;
			docn.LoadXml("<?xml version=\"1.0\" encoding=\"Shift_JIS\"?><NcInfo></NcInfo>");

			docn.DocumentElement.AppendChild(
				docn.ImportNode(xmlDoc1.SelectSingleNode("/NcInfo/NcInfoVersion"), true));
			docn.DocumentElement.AppendChild(
				docn.ImportNode(xmlDoc1.SelectSingleNode("/NcInfo/XmlOutputDateTime"), true));
			docn.DocumentElement.AppendChild(
				docn.ImportNode(xmlDoc1.SelectSingleNode("/NcInfo/NcData"), true));
			// 指定された１つの工具情報を挿入
			docn.DocumentElement.AppendChild(
				docn.ImportNode(xmlDoc1.SelectSingleNode("/NcInfo/Tool[" + (tNo + 1).ToString() + "]"), true));
			// スキーマによる検証
			CamUtil.CamNcD.XmlNavi.SchemaVersion.XmlCheck(docn);
			return docn;
		}

		/// <summary>
		/// ＸＭＬデータの素材寸法入力
		/// </summary>
		public void XmlInsertMaterialSize(CamUtil.Vector3 value) {

			XmlNode xoya, next, nodeX, nodeY, nodeZ;

			xoya = xmlDoc.SelectSingleNode("/NcInfo/NcData");

			if (ncinfoSchemaVer.Older("V14")) {
				;
			}
			else {
				next = xoya.SelectSingleNode("OriginZ");
				nodeX = xoya.InsertAfter(xmlDoc.CreateElement("MaterialSizeX"), next);
				nodeY = xoya.InsertAfter(xmlDoc.CreateElement("MaterialSizeY"), nodeX);
				nodeZ = xoya.InsertAfter(xmlDoc.CreateElement("MaterialSizeZ"), nodeY);
				nodeX.InnerText = value.X.ToString("0.0##");
				nodeY.InnerText = value.Y.ToString("0.0##");
				nodeZ.InnerText = value.Z.ToString("0.0##");
			}

			// ナビゲータの作成と各種設定（スキーマは検証される）
			xmlD = new XmlNavi(xmlDoc, ncinfoSchemaVer.Name);

			return;
		}
	}
}
