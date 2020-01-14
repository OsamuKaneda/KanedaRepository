using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;

using System.IO;
using CamUtil;


namespace NcdTool.Tool
{
	partial class TSData : DataTable
	{
		/// <summary>
		/// ツールシートの保存（データテーブル　→　ＰＣ or ユニックス）
		/// </summary>
		/// <returns></returns>
		public bool SaveTolst() {

			if (Tejun.TjnDir == null) {
				MessageBox.Show("手順データを先に保存してください。");
				return false;
			}
			// 保存確認
			if (KtejunName != null && KtejunName != Tejun.TejunName) {
				DialogResult aa;
				aa = MessageBox.Show($"{this.TableName}は手順{KtejunName}で作成された工具表です。上書きしてもいいですか？",
					"ツールシートの保存", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				if (aa != DialogResult.OK) return false;
			}
			// 一時ファイルの準備
			System.CodeDom.Compiler.TempFileCollection tmpFile =
				new System.CodeDom.Compiler.TempFileCollection(LocalHost.Tempdir, false);
			string tmpFName = tmpFile.BasePath + ".tmp";

			// データテーブル　→　ＰＣのＸＭＬ
			DataToXmlPC(tmpFName);
			// ＰＣのＸＭＬ　→　ＰＣ
			if (!Directory.Exists(Tejun.TolDir))
				Directory.CreateDirectory(Tejun.TolDir);
			if (File.Exists(Tejun.TolDir + "\\" + this.TableName + ".tol"))
				File.Delete(Tejun.TolDir + "\\" + this.TableName + ".tol");
			File.Move(tmpFName, Tejun.TolDir + "\\" + this.TableName + ".tol");
			return true;
		}

		/// <summary>
		/// データテーブル　→　ＰＣのＸＭＬ変換
		/// </summary>
		/// <returns></returns>
		private void DataToXmlPC(string fnam) {
			System.Xml.XmlDocument xmlDoc;
			System.Xml.XmlNode tmpNode1, tmpNode2, tmpNode3;
			string ncinfoSchemaVer = "V03";

			if (ProgVersion.Debug)
				MessageBox.Show("オリジナル手順名の製造番号に関して暫定処置がされている。修正のこと");
			if (KtejunSeba == null || KtejunName == null)
				throw new Exception("qwefrbgvqyerfvl");

			// 保存するときは工具番号順にする
			string sort = DefaultView.Sort;
			DefaultView.Sort = "シートNo,工具No";

			xmlDoc = new System.Xml.XmlDocument();
			// xmlSchema  xmlSchemaSet の準備
			xmlDoc.Schemas = new System.Xml.Schema.XmlSchemaSet();
			xmlDoc.Schemas.Add(null, ServerPC.SvrFldrC + "ToolSheet" + ncinfoSchemaVer + ".xsd");
			xmlDoc.Schemas.Compile();

			// 最初の子ノードの作成
			xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "Shift_JIS", null));
			xmlDoc.AppendChild(xmlDoc.CreateElement("ToolSheet"));

			tmpNode1 = xmlDoc.LastChild;
			tmpNode1.AppendChild(xmlDoc.CreateElement("ToolSheetVersion"));
			tmpNode1.LastChild.AppendChild(xmlDoc.CreateTextNode(ncinfoSchemaVer));

			tmpNode1.AppendChild(xmlDoc.CreateElement("OriginalSeizoNo"));
			tmpNode1.LastChild.AppendChild(xmlDoc.CreateTextNode(KtejunSeba));
			tmpNode1.AppendChild(xmlDoc.CreateElement("OriginalTejunName"));
			tmpNode1.LastChild.AppendChild(xmlDoc.CreateTextNode(KtejunName));

			tmpNode1.AppendChild(xmlDoc.CreateElement("XmlOutputDateTime"));
			tmpNode1.LastChild.AppendChild(xmlDoc.CreateTextNode(
				System.Xml.XmlConvert.ToString(DateTime.Now, System.Xml.XmlDateTimeSerializationMode.Local)));
			// 工具表データ
			foreach (DataRowView dView in this.DefaultView) {
				tmpNode1.AppendChild(xmlDoc.CreateElement("Tool"));
				tmpNode2 = tmpNode1.LastChild;
				tmpNode2.AppendChild(xmlDoc.CreateElement("SheetNo"));
				tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode(((int)dView.Row["シートNo"]).ToString()));
				tmpNode2.AppendChild(xmlDoc.CreateElement("ToolNo"));
				tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode(((int)dView.Row["工具No"]).ToString()));
				tmpNode2.AppendChild(xmlDoc.CreateElement("Division"));
				tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode(((bool)dView.Row["分割"] ? "true" : "false")));
				tmpNode2.AppendChild(xmlDoc.CreateElement("M001"));
				tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode(((bool)dView.Row["M01"] ? "true" : "false")));
				tmpNode2.AppendChild(xmlDoc.CreateElement("M100"));
				//tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode(((bool)dView.Row["M100"] ? "true" : "false")));
				tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode("false"));
				tmpNode2.AppendChild(xmlDoc.CreateElement("HighSpeedSpindle"));
				//tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode(((bool)dView.Row["高速Ｓ"] ? "true" : "false")));
				tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode("false"));
				if (dView.Row["ツールセット"] != DBNull.Value) {
					tmpNode2.AppendChild(xmlDoc.CreateElement("ToolSetName"));
					tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode(((string)dView.Row["ツールセット"])));
				}
				if (dView.Row["突出し量PC"] != DBNull.Value) {
					tmpNode2.AppendChild(xmlDoc.CreateElement("ToolUsableLength"));
					tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode(((double)dView.Row["突出し量PC"]).ToString("#0.000")));
				}
				if (dView.Row["ＮＣ限定"] != DBNull.Value) {
					foreach (string stmp in ((string)dView.Row["ＮＣ限定"]).Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries)) {
						tmpNode2.AppendChild(xmlDoc.CreateElement("Reserved"));
						tmpNode2.LastChild.AppendChild(xmlDoc.CreateTextNode(stmp));
					}
				}
				// 消耗データ
				foreach (ItemData item in this.sumptLst) {
					if ((int)dView.Row["ID"] != item.ID) continue;
					tmpNode2.AppendChild(xmlDoc.CreateElement("Consumption"));
					tmpNode3 = tmpNode2.LastChild;
					tmpNode3.AppendChild(xmlDoc.CreateElement("TejunName"));
					tmpNode3.LastChild.AppendChild(xmlDoc.CreateTextNode(item.tjn_name));
					tmpNode3.AppendChild(xmlDoc.CreateElement("NcName"));
					tmpNode3.LastChild.AppendChild(xmlDoc.CreateTextNode(item.ncd_name));
					tmpNode3.AppendChild(xmlDoc.CreateElement("NcToolCount"));
					tmpNode3.LastChild.AppendChild(xmlDoc.CreateTextNode(item.tdno.ToString()));
					tmpNode3.AppendChild(xmlDoc.CreateElement("Data"));
					tmpNode3.LastChild.AppendChild(xmlDoc.CreateTextNode(item.data.ToString("0.000")));
				}
			}
			// スキーマ検証と保存
			System.Xml.Schema.ValidationEventHandler validation;
			validation = new System.Xml.Schema.ValidationEventHandler(SchemaValidationHandler);
			System.Xml.XPath.XPathNavigator navigator = xmlDoc.CreateNavigator();
			xmlDoc.Validate(validation);
			if (navigator.SchemaInfo.Validity != System.Xml.Schema.XmlSchemaValidity.Valid) {
				xmlDoc.Save("tool.xml");
				throw new Exception(navigator.SchemaInfo.Validity.ToString());
			}
			xmlDoc.Save(fnam);

			DefaultView.Sort = sort;

		}

		/// <summary>
		/// ＰＣのＸＭＬデータ　→　データテーブル変換
		/// </summary>
		/// <param name="pc"></param>
		/// <param name="fulname">ＰＣの工具表から取得した文字列</param>
		private void ItemSetPC(bool pc, string fulname) {

			DataRow workRow;
			ItemData workSmp;
			int iCount, jCount;

			sumptLst.Clear();
			addIDNo = 0;

			System.Xml.XmlDocument xmlDoc;
			System.Xml.Schema.XmlSchemaSet xmlSchemaSet;
			string toolshetSchemaVer;
			System.Xml.XPath.XPathNodeIterator nodes;
			System.Xml.XPath.XPathNavigator xpathNavigator;

			// ＸＭＬドキュメントとナビゲータの作成
			xmlDoc = new System.Xml.XmlDocument();
			xmlDoc.Load(fulname);
			xpathNavigator = xmlDoc.CreateNavigator();

			// ＸＭＬバージョン情報の取得
			nodes = xpathNavigator.Select("/ToolSheet/ToolSheetVersion");
			nodes.MoveNext();
			toolshetSchemaVer = nodes.Current.Value;

			// ＸＭＬスキーマ（セット）の作成
			xmlSchemaSet = new System.Xml.Schema.XmlSchemaSet();
			xmlSchemaSet.Add(null, ServerPC.SvrFldrC + "ToolSheet" + toolshetSchemaVer + ".xsd");
			xmlSchemaSet.Compile();
			xmlDoc.Schemas = xmlSchemaSet;

			// オリジナル手順の製造番号の取得
			switch (toolshetSchemaVer) {
			case "V01":
			case "V02":
				break;
			default:
				nodes = xpathNavigator.Select("/ToolSheet/OriginalSeizoNo");
				nodes.MoveNext();
				m_ktejunSeba = nodes.Current.Value;
				break;
			}

			// オリジナル手順名の取得
			nodes = xpathNavigator.Select("/ToolSheet/OriginalTejunName");
			nodes.MoveNext();
			m_ktejunName = nodes.Current.Value;

			// 作成日の取得
			nodes = xpathNavigator.Select("/ToolSheet/XmlOutputDateTime");
			nodes.MoveNext();
			DateTime outdate = nodes.Current.ValueAsDateTime;

			// ツールセット情報の取得
			nodes = xpathNavigator.Select("/ToolSheet/Tool");
			iCount = nodes.Count;
			for (int ii = 1; ii <= iCount; ii++) {
				workRow = this.NewRow();
				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + ii.ToString() + "]/SheetNo");
				nodes.MoveNext();
				workRow["シートNo"] = nodes.Current.ValueAsInt;
				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + ii.ToString() + "]/ToolNo");
				nodes.MoveNext();
				workRow["工具No"] = nodes.Current.ValueAsInt;
				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + ii.ToString() + "]/Division");
				nodes.MoveNext();
				workRow["分割"] = nodes.Current.ValueAsBoolean;
				if (Tejun.Mach.Toool_nc) workRow["分割"] = true;
				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + ii.ToString() + "]/M001");
				nodes.MoveNext();
				workRow["M01"] = nodes.Current.ValueAsBoolean;
				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + ii.ToString() + "]/ToolSetName");
				if (nodes.Count == 1) {
					nodes.MoveNext();
					workRow["ツールセット"] = nodes.Current.Value;
				}
				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + ii.ToString() + "]/ToolUsableLength");
				if (nodes.Count == 1) {
					nodes.MoveNext();
					workRow["突出し量PC"] = nodes.Current.ValueAsDouble;
				}

				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + ii.ToString() + "]/Reserved");
				jCount = nodes.Count;
				string gent = "";
				for (int jj = 1; jj <= jCount; jj++) {
					nodes = xpathNavigator.Select("/ToolSheet/Tool[" + ii.ToString() + "]/Reserved[" + jj.ToString() + "]");
					nodes.MoveNext();
					gent += " " + nodes.Current.Value;
				}
				if (gent.Length > 0)
					workRow["ＮＣ限定"] = gent.Substring(1);

				// ＮＣ消耗情報の取得 V02, V03....
				if (toolshetSchemaVer == "V01") { throw new Exception("工具表データが未対応の古いバージョンです。"); }
				else {
					nodes = xpathNavigator.Select("/ToolSheet/Tool[" + ii.ToString() + "]/Consumption");
					jCount = nodes.Count;
					for (int jj = 1; jj <= jCount; jj++) {
						workSmp = new ItemData(ii, jj, xpathNavigator);
						this.sumptLst.Add(workSmp);
					}
				}

				workRow["変更"] = DBNull.Value;
				workRow["ID"] = addIDNo = ii;
				this.Rows.Add(workRow);
			}

			// ＰＣからの変換後
			this.AcceptChanges();
			return;
		}
	}
}
