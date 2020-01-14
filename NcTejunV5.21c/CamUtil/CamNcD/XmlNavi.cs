using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using System.Text.RegularExpressions;

namespace CamUtil.CamNcD
{
	/// <summary>
	/// ＸＭＬのデータを参照します。[不変]
	/// </summary>
	public class XmlNavi
	{
		/// <summary>
		/// 加工情報のスキーマ情報を保存する
		/// </summary>
		public readonly struct SchemaVersion
		{
			/// <summary>最新のスキーマバージョン名</summary>
			private static readonly string newVersion = "V16";
			/// <summary>使用可能な最も古いスキーマバージョン番号</summary>
			private static readonly int oldVersion = 9;

			/// <summary>最新スキーマ</summary>
			public static SchemaVersion Latest() { return new SchemaVersion(newVersion); }

			/// <summary>スキーマバージョン名のフォーマット</summary>
			private static Regex regVer = new Regex("^V[0-9][0-9]$");

			/// <summary>
			/// xmlDocの設定（エラーハンドリング、ナビゲータ設定、スキーマによる検証）
			/// </summary>
			static public XPathNavigator XmlCheck(XmlDocument xmlDoc1) {
				// エラーの設定
				xmlDoc1.Validate(new ValidationEventHandler(ValidationEventHandler));
				// ナビゲータの作成
				XPathNavigator xpathNavigator1 = xmlDoc1.CreateNavigator();
				// スキーマによる検証の設定
				if (xpathNavigator1.SchemaInfo.Validity != XmlSchemaValidity.Valid) {
					xmlDoc1.Save("tmp.xml");
					throw new Exception(
						"スキーマ検証でエラーが見つかりました。  " +
						"エラーとなったＸＭＬデータは " + Directory.GetCurrentDirectory() + "\\tmp.xml に保存しました。");
				}
				return xpathNavigator1;
			}

			static private void ValidationEventHandler(object sender, ValidationEventArgs e) {
				switch (e.Severity) {
				case XmlSeverityType.Error:
					Console.WriteLine("Error: {0}", e.Message);
					break;
				case XmlSeverityType.Warning:
					Console.WriteLine("Warning {0}", e.Message);
					break;
				}
			}

			/////////////////
			// 以上 static //
			/////////////////



			/// <summary>読込んだスキーマのバージョン名（"V04","V05","V06",...）</summary>
			public readonly string Name;
			/// <summary>読込んだスキーマのバージョン番号</summary>
			public readonly Byte Numb;

			/// <summary>バージョン名より作成するコンストラクタ</summary>
			/// <param name="versionName"></param>
			public SchemaVersion(string versionName) {
				// バージョン名のチェック
				if (!regVer.IsMatch(versionName)) throw new Exception("qwekfqefnqjen");

				Name = versionName;
				Numb = Convert.ToByte(versionName.Substring(1));
				if (Numb < oldVersion)
					throw new Exception("使用できない古いＮＣデータが選択されました。NCSEND2を再度実行してください。");
			}

			/// <summary>ＸＭＬファイルより作成するコンストラクタ</summary>
			/// <param name="xd"></param>
			public SchemaVersion(XmlDocument xd) {
				// バージョン名の取出し
				XPathNavigator xpathNavigator = xd.CreateNavigator();
				XPathNodeIterator nodes = xpathNavigator.Select("/NcInfo/NcInfoVersion");
				nodes.MoveNext();

				Name = nodes.Current.Value;
				Numb = Convert.ToByte(nodes.Current.Value.Substring(1));
				if (Numb < oldVersion)
					throw new Exception("使用できない古いＮＣデータが選択されました。NCSEND2を再度実行してください。");
			}

			/// <summary>スキーマのバージョンが指定のバージョンより古いか等しい場合はtrue</summary>
			public bool Older(string versionName) {
				// バージョン名のチェック
				if (!regVer.IsMatch(versionName)) throw new Exception("qwekfqefnqjen");

				byte versionNo = Convert.ToByte(versionName.Substring(1));
				if (versionNo < oldVersion) LogOut.CheckCount("XmlNavi 101", false, "versionNoが最も古いバージョン未満のため常にfalseとなります");
				return Numb <= versionNo;
			}

			/// <summary>定義言語スキーマの生成</summary>
			public XmlSchemaSet SchemaSet() {
				XmlSchemaSet xml = new XmlSchemaSet();
				xml.Add(null, ServerPC.SvrFldrC + "NcInfo" + this.Name + ".xsd");
				xml.Compile();
				return xml;
			}
		}



		/// <summary>ＮＣＭＮによる工具分割がある場合true（工具単位情報は使用不可）</summary>
		public readonly bool bunkatsu;

		/// <summary>ＸＭＬナビ</summary>
		private readonly XPathNavigator xpathNavigator;
		//protected XPathNodeIterator nodes;

		/// <summary>ＸＭＬスキーマバージョン</summary>
		private readonly SchemaVersion ncinfoSchemaVer;

		/// <summary>工具交換回数（注意：ＸＭＬの工具数でＵＮＩＸにて事前分割されたものは含まない）</summary>
		public int ToolCount {
			get {
				if (xpathNavigator == null) return 0;
				return xpathNavigator.Select("/NcInfo/Tool").Count;
			}
		}
		//public int ToolCount { get { return m_ToolCount; } }
		//private int m_ToolCount;

		/// <summary>
		/// 工具単位のＮＣデータのＸＭＬ情報
		/// </summary>
		/// <param name="tcnt"></param>
		/// <returns></returns>
		public Tool this[int tcnt] {
			get {
				if (bunkatsu)
					throw new InvalidOperationException("ＮＣＭＮにて工具分割されているため参照できない。");
				if (tcnt < 0)
					throw new InvalidOperationException("工具Ｎｏが０以下");
				if (tcnt >= ToolCount)
					throw new InvalidOperationException("工具の最大値を超えた");
				return tdd[tcnt];
			}
		}
		private readonly Tool[] tdd;



		/// <summary>
		/// XPathNavigatorの作成（唯一のコンストラクター）
		/// </summary>
		/// <param name="xmlDoc1"></param>
		/// <param name="ncinfoSchemaVer1"></param>
		public XmlNavi(XmlDocument xmlDoc1, string ncinfoSchemaVer1) {
			// xmlDocの設定（エラーハンドリング、ナビゲータ設定、スキーマによる検証）
			xpathNavigator = SchemaVersion.XmlCheck(xmlDoc1);

			ncinfoSchemaVer = new SchemaVersion(ncinfoSchemaVer1);

			// 工具数ごとの工具情報の読み込みストラクチャー作成
			tdd = new Tool[xpathNavigator.Select("/NcInfo/Tool").Count];
			for (int ii = 0; ii < tdd.Length; ii++)
				tdd[ii] = new Tool(this, ii, ncinfoSchemaVer);

			bunkatsu = false;

			return;
		}




		/// <summary>部品名</summary>
		public string BUHIN_NAME { get { return ProcessName; } }

		/// <summary>編集名</summary>
		public string HENSHU_NAME { get { return CoordinatesName ?? ProductsName; } }

		/// <summary>0 ファイル名 Path.GetFileName(FULNM)</summary>
		public string CamDataName { get { return (string)DtGetSub("/NcInfo/NcData/camDataName"); } }
		/// <summary>  検証前のファイル名</summary>
		public string CamOriginalNcName { get { return (string)DtGetSub("/NcInfo/NcData/camOriginalNcName"); } }

		/// <summary>0 ＣＡＭシステム名 ADD 2011/10/10</summary>
		public CamSystem CamSystemID { get { return new CamSystem((string)DtGetSub("/NcInfo/NcData/camSystemName")); } }

		/// <summary>1 ＮＣデータ出力日</summary>
		public DateTime CamOutputDateTime { get { return (DateTime)DtGetSub("/NcInfo/NcData/camOutputDateTime"); } }

		/// <summary>0 プロセス名（車両部品名として使用 etc. MTO123AGRL）</summary>
		public string ProcessName { get { return (string)DtGetSub("/NcInfo/NcData/ProcessName"); } }

		/// <summary>金型の部品名（編集名として使用する）</summary>
		public string ProductsName { get { return (string)DtGetSub("/NcInfo/NcData/ProductsName"); } }

		/// <summary>未使用（CADmeister Dynavista2D のみ設定あり）</summary>
		public string CoordinatesName { get { return (string)DtGetSub("/NcInfo/NcData/CoordinatesName"); } }

		/// <summary>0 ＮＣデータのフルネーム</summary>
		private string CamDataFullName {
			get {
				string localPath = (string)DtGetSub("/NcInfo/NcData/camDataUri");
				int indx = localPath.IndexOf('\\', 2);
				if (LocalHost.IPAddress == localPath.Substring(2, indx - 2))
					return localPath.Substring(indx + 1);
				else
					return localPath;
			}
		}
		/// <summary>0 フォルダーのフルネーム</summary>
		private string CamFolderName { get { return Path.GetDirectoryName(CamDataFullName); } }
		/// <summary>0 Path.GetFileNameWithoutExtension(FULNM)</summary>
		public string CamBaseName { get { return Path.GetFileNameWithoutExtension(CamDataFullName); } }

		/// <summary>1 ＮＣデータ作成時の材質設定</summary>
		public string CamMaterial { get { return (string)DtGetSub("/NcInfo/NcData/camMaterial"); } }

		/// <summary>  加工原点Ｘ</summary>
		public double OriginX { get { return (double)DtGetSub("/NcInfo/NcData/OriginX"); } }
		/// <summary>  加工原点Ｙ</summary>
		public double OriginY { get { return (double)DtGetSub("/NcInfo/NcData/OriginY"); } }
		/// <summary>  加工原点Ｚ</summary>
		public double OriginZ { get { return (double)DtGetSub("/NcInfo/NcData/OriginZ"); } }
		/// <summary>  材料のサイズ。Ｚの値は取り付け冶具も含む</summary>
		public double[] MaterialSize {
			get {
				if (ncinfoSchemaVer.Older("V14")) {
					return null;
				}
				else {
					return new double[] {
						(double)DtGetSub("/NcInfo/NcData/MaterialSizeX"),
						(double)DtGetSub("/NcInfo/NcData/MaterialSizeY"),
						(double)DtGetSub("/NcInfo/NcData/MaterialSizeZ") };
				}
			}
		}

		/// <summary>  ＣＡＭの次元。工具傾斜時の座標系設定方法と加工法を区分する。（２or３）</summary>
		public int CamDimension { get { return (int)DtGetSub("/NcInfo/NcData/camDimension"); } }

		// 2007.08.30
		/// <summary>  ポストプロセッサ</summary>
		public PostProcessor PostProcessor {
			get {
				if (ncinfoSchemaVer.Older("V14")) {
					return PostProcessor.GetPost(CamSystemID.Name, (string)DtGetSub("/NcInfo/NcData/PostProcessorName"));
				}
				else if(ncinfoSchemaVer.Older("V15")) {
					return PostProcessor.GetPost(PostProcessor.ChangeName((string)DtGetSub("/NcInfo/NcData/PostProcessorName")));
				}
				else {
					return PostProcessor.GetPost((string)DtGetSub("/NcInfo/NcData/PostProcessorName"));
				}
			}
		}
		/// <summary>  ベースＮＣデータフォーマット</summary>
		public BaseNcForm BaseNcFormat { get { return PostProcessor.BaseForm; } }
		/// <summary>  ポストプロセッサのバージョン</summary>
		public string PostProcessorVersion { get { return (string)DtGetSub("/NcInfo/NcData/PostProcessorVersion"); } }
		/// <summary>  ＣＡＭで干渉チェックに使用されたマシンヘッド名（Tebis）</summary>
		public string MHEAD { get { return (string)DtGetSub("/NcInfo/NcData/MachineHeadName"); } }




		// ////////////////////
		// V03 2010.04.27 ADD
		// ////////////////////
		/// <summary>0 シミュレーションシステム名</summary>
		public string SmNAM {
			get {
				string aaa = (string)DtGetSub("/NcInfo/NcData/simSystemName");
				if (aaa != null) return aaa;
				if ((string)DtGetSub("/NcInfo/NcData/simMaterial") == null)
					return aaa;
				throw new InvalidOperationException("シミュレーションシステム名simSystemNameが入力されていない");
			}
		}
		//public string smNAM { get { return (string)DtGetSub("/NcInfo/NcData/simSystemName", "only"); } }
		/// <summary>検証出力の材質</summary>
		public string SmMAT { get { return (string)DtGetSub("/NcInfo/NcData/simMaterial"); } }
		/// <summary>検証出力の工作機</summary>
		public string SmMCN { get { return (string)DtGetSub("/NcInfo/NcData/simMachine"); } }

		// ADD in 2013/06/04

		/// <summary>1 クリアランス面のＮＣデータ最大値</summary>
		public double NcAPCHZ { get { return this.tdd.Select(tol => tol.TlAPCHZ).Max(); } }
		/// <summary>1 クリアランス面のＮＣデータのリスト</summary>
		public double[] NcClrPlaneList { get { return this.tdd.Select(tol => tol.TlAPCHZ).ToArray(); } }
		/// <summary>
		/// クリアランス面の整合性チェック 熊崎さんの要望により追加 in 2018/05/28
		/// </summary>
		/// <returns>エラーなし：null、エラーの場合：ＮＣデータ名、ツールセット名を出力</returns>
		public string ClpCheck() {
			RotationAxis rotSP;
			if (this.BaseNcFormat.Id != BaseNcForm.ID.GENERAL) return null;		// 素材上面基準以外は数値でのチェックはできない
			if (this.CamDimension != 2) return null;							// ３次元加工は高さ自由とする
			for (int ii = 0; ii < this.ToolCount; ii++)
				for (int jj = 0; jj < this[ii].AxisCount; jj++) {
					rotSP = new RotationAxis(this[ii][jj].MachiningAxisAngle);
					if (rotSP.ToolDir().Z > 0.999)
						if (this[ii][jj].ClearancePlane < 20.0)					// ２次元加工は２０mm以上に限定する
							return String.Format("CLP={0:f2}", this[ii][jj].ClearancePlane);
				}
			return null;
		}

		private object DtGetSub(string xPath) {
			Uri uri;
			XmlTypeCode xmltypeCode;

			XPathExpression query = xpathNavigator.Compile(xPath);
			XPathNodeIterator nodes = xpathNavigator.Select(query);

			if (nodes.Count == 0) return null;
			if (nodes.Count != 1) {
				throw new Exception(
					"nodes.Countは１ではない"
					+ " type = " + nodes.Current.ValueType.Name
						+ " typecode = " + Type.GetTypeCode(nodes.Current.ValueType).ToString()
					+ " string = " + xPath);
			}
			nodes.MoveNext();
			xmltypeCode = nodes.Current.SchemaInfo.SchemaType.Datatype.TypeCode;

			switch (xmltypeCode) {
			case XmlTypeCode.String:
				return (object)nodes.Current.Value;
			case XmlTypeCode.Int:
				return (object)nodes.Current.ValueAsInt;
			case XmlTypeCode.Double:
				return (object)nodes.Current.ValueAsDouble;
			case XmlTypeCode.DateTime:
				return (object)nodes.Current.ValueAsDateTime;
			case XmlTypeCode.Boolean:
				return (object)nodes.Current.ValueAsBoolean;
			case XmlTypeCode.AnyUri:
				uri = new Uri(nodes.Current.Value);
				// uri.LocalPathはフォルダー名に"#"が含まれると以降を無視するため、uri.OriginalStringに変更 2016/01/20
				//return (object)uri.LocalPath;
				return (object)uri.OriginalString.Replace("file:", "").Replace("/", @"\");
			default:
				throw new Exception(
					"処理できないタイプ(only)"
					+ " xmltypeCode = " + xmltypeCode.ToString()
					+ " type = " + nodes.Current.ValueType.Name
					+ " string = " + xPath);
			}
		}


		/// <summary>
		/// 工具単位のＸＭＬ情報の出力ストラクチャー[不変]
		/// </summary>
		public class Tool
		{
			/// <summary>工具の番号（０から）</summary>
			private readonly int tcnt;
			/// <summary>親のXmlNavi</summary>
			public readonly XmlNavi parent;

			// ******** //
			//   工具   //
			// ******** //
			/// <summary>1 ツールセットＣＡＭ名</summary>
			public string SNAME { get { return (string)DtGetSub("/ToolData/ToolSetName"); } }
			/// <summary>1 ＣＡＭシステムの工具種類</summary>
			public string TTYPE { get { return (string)DtGetSub("/ToolData/ToolTypeCam"); } }

			/// <summary>ツールセット名（ＮＣＳＰＥＥＤ）V03 2010.04.27 ADD</summary>
			public string SmTSN { get { return (string)DtGetSub("/ToolData/simToolSetName"); } }
			/// <summary>  工程区分（NCSPEED用）</summary>
			public string KOTEI { get { return (string)DtGetSub("/ToolData/ProcessDiscrimination"); } }

			// ******** //
			// 任意指示 //
			// ******** //

			/// <summary>
			/// 加工条件の任意指示(SPIN, FEED)の有無
			/// </summary>
			public bool OPTION() {
				return OPTION(out double? spin, out double? feed);
			}
			/// <summary>
			/// 加工条件の任意指示(SPIN, FEED)
			/// </summary>
			/// <param name="spin"></param>
			/// <param name="feed"></param>
			public bool OPTION(out double? spin, out double? feed) {
				double? dS;
				double? dF;
				if (parent.ncinfoSchemaVer.Older("V10")) {
					dS = (double?)DtGetSub("/OptionalSpinSet");
					dF = (double?)DtGetSub("/OptionalFeedSet");
				}
				else {
					dS = (double?)DtGetSub("/OptionalSpFdSet/Spin");
					dF = (double?)DtGetSub("/OptionalSpFdSet/Feed");
				}
				if (dS.HasValue || dF.HasValue) {
					if (!dS.HasValue || !dF.HasValue) throw new Exception("XML Optional ERROR");
					if (Math.Abs(dS.Value - 1.0) >= 0.01 || Math.Abs(dF.Value - 1.0) >= 0.01) {
						spin = dS;
						feed = dF;
						return true;
					}
				}
				spin = feed = null;
				return false;
			}
			/// <summary>  加工条件の任意指示（ＳＰＩＮ）</summary>
			public double? OPTSP {
				get {
					if (parent.ncinfoSchemaVer.Older("V10")) {
						return (double?)DtGetSub("/OptionalSpinSet");
					}
					else {
						return (double?)DtGetSub("/OptionalSpFdSet/Spin");
					}
				}
			}
			/// <summary>  加工条件の任意指示（ＦＥＥＤ）</summary>
			public double? OPTFD {
				get {
					if (parent.ncinfoSchemaVer.Older("V10")) {
						return (double?)DtGetSub("/OptionalFeedSet");
					}
					else {
						return (double?)DtGetSub("/OptionalSpFdSet/Feed");
					}
				}
			}



			/// <summary>  加工条件の任意指示（ＬＩＦＥ）V04以降</summary>
			public double? OPTLF { get { return (double?)DtGetSub("/OptionalLifeSet"); } }

			/// <summary>  突出し量の任意指示</summary>
			internal double? OPTUL { get { return (double?)DtGetSub("/OptionalULenSet"); } }

			// ******** //
			//  工具軸  //
			// ******** //

			/// <summary>工具軸</summary>
			public TAxis this[int iaxis] { get { return axisd[iaxis]; } }
			private readonly TAxis[] axisd;

			/// <summary>工具軸：工具の工具軸数</summary>
			public int AxisCount { get { return axisd.Length; } }

			/// <summary>工具軸：傾斜加工か</summary>
			public bool Keisha { get { return MaxMachiningAxis.ToVector().Abs > 0.0; } }

			/// <summary>工具軸：同時５軸か</summary>
			public bool SimultaneousAxisControll { get { return this.axisd.Any(ax => ax.SimultaneousAxisControll); } }

			/// <summary>1 クリアランス面の工具データ最大値</summary>
			public double TlAPCHZ { get { return this.axisd.Select(ax => ax.ClearancePlane).Max(); } }
			/// <summary>クリアランス面のリスト</summary>
			public double[] ClrPlaneList { get { return this.axisd.Select(ax => ax.ClearancePlane).ToArray(); } }
			/// <summary>絶対値が最大の軸傾斜角</summary>
			public Angle3 MaxMachiningAxis { get { return this.axisd.Select(ax => ax.MachiningAxisAngle).OrderBy(ax => ax.Abs).ThenBy(ax => ax.A).ThenBy(ax => ax.B).ThenBy(ax => ax.C).Last(); } }
			/// <summary>軸傾斜角のリスト</summary>
			public Angle3[] MachiningAxisList { get { return this.axisd.Select(ax => ax.MachiningAxisAngle).ToArray(); } }

			// ******** //
			//   工程   //
			// ******** //
			/// <summary>1 加工長mm</summary>
			public double NCLEN { get { return (double)DtGetSub("/kotei/FeedLength"); } }
			/// <summary>早送り移動長さmm</summary>
			internal double? RPLEN { get { return (double?)DtGetSub("/kotei/NonFeedLength"); } }
			/// <summary>1 加工時間min</summary>
			public double NCTIM { get { return (double)DtGetSub("/kotei/FeedTime"); } }
			/// <summary>  早送り時間min</summary>
			public double NOFDT { get { return (double)DtGetSub("/kotei/NonFeedTime"); } }
			/// <summary>  実切削加工時間min</summary>
			public double? CuttingTime { get { return (double?)DtGetSub("/kotei/CuttingTime"); } }
			/// <summary>2 突出し量</summary>
			public double TULEN { get { return (double)DtGetSub("/kotei/ToolUsableLength"); } }

			// V05 add in 2012/07/17
			/// <summary>加工領域最大</summary>
			public Vector3? MaxXYZ {
				get {
					if (DtGetSub("/kotei/MaxXYZ/X") == null)
						return null;
					else
						return new Vector3(
							(double)DtGetSub("/kotei/MaxXYZ/X"),
							(double)DtGetSub("/kotei/MaxXYZ/Y"),
							(double)DtGetSub("/kotei/MaxXYZ/Z"));
				}
			}
			/// <summary>加工領域最小</summary>
			public Vector3? MinXYZ {
				get {
					if (DtGetSub("/kotei/MinXYZ/X") == null)
						return null;
					else
						return new Vector3(
							(double)DtGetSub("/kotei/MinXYZ/X"),
							(double)DtGetSub("/kotei/MinXYZ/Y"),
							(double)DtGetSub("/kotei/MinXYZ/Z"));
				}
			}

			/// <summary>1 Ｚピック（すべて未指定、工程間不一致の場合 null）</summary>
			public double? ZPICK { get { return (double?)DtGetSub("/kotei/PickZ"); } }
			/// <summary>1 XYピック（すべて未指定、工程間不一致の場合 null）</summary>
			public double? XPICK { get { return (double?)DtGetSub("/kotei/PickX"); } }
			/// <summary>1 残し量（すべて未指定、工程間不一致の場合 null）</summary>
			public double? NOKOS { get { return (double?)DtGetSub("/kotei/WallThicknessX"); } }
			/// <summary>1 トレランス（すべて未指定、工程間不一致の場合 null）</summary>
			public double? TOLER { get { return (double?)DtGetSub("/kotei/Tolerance"); } }

			/// <summary>1 ＮＣデータの回転数min-1   （==NCSEND2実行時のＤＢの値）</summary>
			public double SPIND { get { return (double)DtGetSub("/kotei/SpindleSpeed"); } }
			/// <summary>1 ＮＣデータの送り速度mm/min（==NCSEND2実行時のＤＢの値）</summary>
			public double FEEDR { get { return (double)DtGetSub("/kotei/CuttingFeedRate"); } }

			/// <summary>1 アプローチ速度（未指定の場合、工程間不一致の場合 null）</summary>
			public double? APRFED {
				get {
					if (parent.ncinfoSchemaVer.Older("V09"))
						return (double?)DtGetSub("/kotei/FeedElse/FeedRate[1]/speed");
					else
						return (double?)DtGetSub("/kotei/FeedElse/FeedRate[1]/@speed");
				}
			}

			// 2006.04.14
			/// <summary>1 加工方向</summary>
			public string CTDIR { get { return (string)DtGetSub("/kotei/CuttingDirection"); } }
			/// <summary>1 加工法</summary>
			public string METHD { get { return (string)DtGetSub("/kotei/MachiningMethod"); } }
			/// <summary>1 工程名（ＸＭＬ必須）</summary>
			public string KOTNM { get { return (string)DtGetSub("/kotei/Name"); } }

			// 2006.06.28
			/// <summary>  工程タイプ</summary>
			public string KTYPE { get { return (string)DtGetSub("/kotei/Type"); } }
			/// <summary>  工程区分</summary>
			public string KCLSS { get { return (string)DtGetSub("/kotei/Class"); } }
			/// <summary>  工程のＮＣサイクル名</summary>
			public string KCYCL { get { return (string)DtGetSub("/kotei/NcCycle"); } }

			// 2007.08.20
			// 2007.10.01 ADD

			/// <summary>  ＮＣデータのリファレンス点が刃先=Tipならtrue、工具中心=Centerならfalse</summary>
			public bool TRTIP { get { return (string)DtGetSub("/kotei/ToolReferencePoint") == "Tip"; } }
			/// <summary>  ＮＣデータ出力時に入力されたコメント（Tebis）</summary>
			public string CMMNT { get { return (string)DtGetSub("/kotei/PostComment"); } }

			/// <summary>ＸＭＬ切削送りに対する総合比率（ToolsetCAM,材質,加工機）V03 2010.04.27 ADD</summary>
			public double SmFDR { get { return (double)DtGetSub("/kotei/CuttingFeedRatio"); } }
			/// <summary>ＸＭＬ回転数に対する総合比率（ToolsetCAM,材質,加工機）V03 2010.04.27 ADD</summary>
			public double SmSPR { get { return (double)DtGetSub("/kotei/SpindleSpeedRatio"); } }

			// ＮＣデータの移動情報　XmlNavi.ClMoving より移動 in 2019/08/08

			/// <summary>  ＮＣデータの移動情報 倒れ補正のための面オフセット量（in ＣＡＭ）</summary>
			public double ClMv_Offset { get { return SetDist("Offset") ?? 0.0; } }
			/// <summary>  ＮＣデータの移動情報 ＣＬ移動済み量 Ｘ軸方向（in NCSPEED）</summary>
			public double ClMv_X_axis { get { return SetDist("X_axis") ?? 0.0; } }
			/// <summary>  ＮＣデータの移動情報 ＣＬ移動済み量 Ｙ軸方向（in NCSPEED）</summary>
			public double ClMv_Y_axis { get { return SetDist("Y_axis") ?? 0.0; } }
			/// <summary>  ＮＣデータの移動情報 ＣＬ移動済み量 Ｚ軸方向（in NCSPEED）</summary>
			public double ClMv_Z_axis { get { return SetDist("Z_axis") ?? 0.0; } }
			private double? SetDist(string qq) {
				return parent.ncinfoSchemaVer.Older("V09") ? null : (double?)DtGetSub("/kotei/ClMovingDistance/" + qq);
			}


			// ******** //
			//   検証   //
			// ******** //
			/// <summary>コリジョンのチェック結果V03 2010.04.27 ADD</summary>
			public string SmCLC { get { return (string)DtGetSub("/Simulate/CollisionCheck"); } }
			/// <summary>検証内容_ツールセット選択V03 2010.04.27 ADD</summary>
			public bool SmTSS { get { return (bool?)DtGetSub("/Optimize/ToolingNcSelect") ?? false; } }
			/// <summary>検証内容_ツールセット最適化</summary>
			public bool SmTSB { get { return (bool?)DtGetSub("/Optimize/ToolingNcSeparate") ?? false; } }
			/// <summary>検証内容_送り速度最適化</summary>
			public bool SmFDC { get { return (bool?)DtGetSub("/Optimize/FeedControl") ?? false; } }
			/// <summary>検証内容_エアカット削除</summary>
			public bool SmACD { get { return (bool?)DtGetSub("/Optimize/AirCutDelete") ?? false; } }
			/// <summary>検証内容_工具寿命分割</summary>
			public bool SmLNS { get { return (bool?)DtGetSub("/Optimize/LifeNcSeparate") ?? false; } }





			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="p_parent"></param>
			/// <param name="p_tcnt"></param>
			/// <param name="p_ncinfoSchemaVer"></param>
			public Tool(XmlNavi p_parent, int p_tcnt, SchemaVersion p_ncinfoSchemaVer) {
				this.tcnt = p_tcnt;
				this.parent = p_parent;

				// 工具軸関連の設定
				int axCount = p_parent.xpathNavigator.Select("/NcInfo/Tool[" + ((int)(p_tcnt + 1)).ToString() + "]/Axis").Count;
				axisd = new TAxis[axCount];
				for (int ii = 0; ii < axCount; ii++)
					axisd[ii] = new TAxis(this, p_ncinfoSchemaVer, p_tcnt, ii);
			}

			private object DtGetSub(string xPath_s) {
				return this.parent.DtGetSub("/NcInfo/Tool[" + ((int)(this.tcnt + 1)).ToString() + "]" + xPath_s);
			}
		}



		/// <summary>
		/// 工具軸単位のストラクチャ
		/// </summary>
		public readonly struct TAxis
		{
			private readonly Tool parent;
			private readonly string xPath;

			/// <summary>５軸同時制御</summary>
			public bool SimultaneousAxisControll {
				get {
					XPathExpression query = parent.parent.xpathNavigator.Compile(xPath + "/AxisControlledMotion");
					XPathNodeIterator nodes = parent.parent.xpathNavigator.Select(query);
					if (nodes.Count != 1) throw new InvalidOperationException("afaevfrvgv");
					nodes.MoveNext();
					if (nodes.Current.Value == "simultaneous") return true;
					if (nodes.Current.Value == "indexed") return false;
					throw new InvalidOperationException("ＸＭＬの'Tool/Axis/AxisControlledMotion'の値が'indexed'でも'simultaneous'でもない。");
				}
			}

			/// <summary>1 クリアランスプレーン</summary>
			public double ClearancePlane {
				get {
					XPathExpression query = parent.parent.xpathNavigator.Compile(xPath + "/ClearancePlane");
					XPathNodeIterator nodes = parent.parent.xpathNavigator.Select(query);
					if (nodes.Count != 1) throw new InvalidOperationException("afaevfrvgv");
					nodes.MoveNext();
					return nodes.Current.ValueAsDouble;
				}
			}

			/// <summary>  傾斜軸の回転角</summary>
			public Angle3 MachiningAxisAngle {
				get {
					if (parent.parent.ncinfoSchemaVer.Older("V09")) {
						Angle3.JIKU jiku;
						switch (parent.parent.CamSystemID.Name) {
						case CamSystem.Tebis:
							switch (parent.parent.PostProcessor.Id) {
							case PostProcessor.ID.GEN_OM:
								jiku = Angle3.JIKU.Euler_XYZ; break;
							case PostProcessor.ID.GEN_BU:
							case PostProcessor.ID.MES_AFT_BU:
							case PostProcessor.ID.MES_BEF_BU:
								jiku = Angle3.JIKU.Euler_XYZ; break;
							default:
								throw new InvalidOperationException("efbqfrbqahreb");
							}
							break;
						case CamSystem.CAMTOOL_5AXIS:
							switch (parent.parent.CamDimension) {
							case 2: jiku = Angle3.JIKU.Euler_ZXZ; break;
							case 3: jiku = Angle3.JIKU.MCCVG_AC; break;
							default: throw new InvalidOperationException("efbqfrbqahreb");
							}
							break;
						default: jiku = Angle3.JIKU.Null; break;
						}
						switch (jiku) {
						case Angle3.JIKU.Euler_XYZ:
						case Angle3.JIKU.Euler_ZXZ:
						case Angle3.JIKU.MCCVG_AC:
						case Angle3.JIKU.Null:
							return new Angle3(jiku, MachiningAxis);
						case Angle3.JIKU.Spatial:
						case Angle3.JIKU.DMU_BC:
							return new Angle3(jiku, PlaneSpatial);
						default: throw new InvalidOperationException("efdbqehb");
						}
					}
					else {
						string axis = null;
						Angle3.JIKU type = Angle3.JIKU.Null;
						if (0 == 0) {
							XPathExpression query = parent.parent.xpathNavigator.Compile(xPath + "/MachiningAxis");
							XPathNodeIterator nodes = parent.parent.xpathNavigator.Select(query);
							if (nodes.Count != 1) throw new InvalidOperationException("qwefbqwhferbh");
							XPathNavigator navi = parent.parent.xpathNavigator.SelectSingleNode(query);

							navi.MoveToFirstAttribute();
							if (navi.Name != "type") throw new InvalidOperationException("qawrefbqarebh");
							type = Angle3.JIKU_Type(navi.Value);

							navi.MoveToNextAttribute();
							if (navi.Name != "axis") throw new InvalidOperationException("qawrefbqarebh");
							axis = navi.Value;
						}
						return new Angle3(type, axis);
					}
				}
			}

			/// <summary>  ハイデンハインの空間角、DMU200PのＢ軸Ｃ軸</summary>
			internal string PlaneSpatial {
				get {
					XPathExpression query = parent.parent.xpathNavigator.Compile(xPath + "/PlaneSpatial");
					XPathNodeIterator nodes = parent.parent.xpathNavigator.Select(query);
					if (nodes.Count != 1) return null;
					nodes.MoveNext();
					string ss = nodes.Current.Value;
					switch (parent.parent.CamDimension) {
					case 2:
						if (ss.IndexOf('A') < 0) throw new Exception("ハイデンハインの空間角フォーマットエラー");
						if (ss.IndexOf('B') < 0) throw new Exception("ハイデンハインの空間角フォーマットエラー");
						if (ss.IndexOf('C') < 0) throw new Exception("ハイデンハインの空間角フォーマットエラー");
						break;
					case 3:
						// 空間角SPATIAL ではなくDMU200PのＢ軸とＣ軸が出力されている
						if (ss.IndexOf('A') >= 0) throw new Exception("ハイデンハインのＢＣ軸フォーマットエラー");
						if (ss.IndexOf('B') < 0) throw new Exception("ハイデンハインのＢＣ軸フォーマットエラー");
						if (ss.IndexOf('C') < 0) throw new Exception("ハイデンハインのＢＣ軸フォーマットエラー");
						break;
					default: throw new Exception("awefaerfvregfvg");
					}
					return ss;
				}
			}
			/// <summary>  傾斜軸の回転角（Tebis標準仕様 = ＸＹＺオイラー角）</summary>
			internal string MachiningAxis {
				get {
					XPathExpression query = parent.parent.xpathNavigator.Compile(xPath + "/MachiningAxis");
					XPathNodeIterator nodes = parent.parent.xpathNavigator.Select(query);
					if (nodes.Count != 1) return null;
					nodes.MoveNext();
					return nodes.Current.Value;
				}
			}
			/// <summary>  オイラー角</summary>
			public string EulerAngleZXZ {
				get {
					XPathExpression query = parent.parent.xpathNavigator.Compile(xPath + "/EulerAngleZXZ");
					XPathNodeIterator nodes = parent.parent.xpathNavigator.Select(query);
					if (nodes.Count != 1) return null;
					string ss = "";
					query = parent.parent.xpathNavigator.Compile(xPath + "/EulerAngleZXZ/A");
					nodes = parent.parent.xpathNavigator.Select(query);
					nodes.MoveNext();
					ss += "A" + nodes.Current.Value;
					query = parent.parent.xpathNavigator.Compile(xPath + "/EulerAngleZXZ/B");
					nodes = parent.parent.xpathNavigator.Select(query);
					nodes.MoveNext();
					ss += "B" + nodes.Current.Value;
					query = parent.parent.xpathNavigator.Compile(xPath + "/EulerAngleZXZ/C");
					nodes = parent.parent.xpathNavigator.Select(query);
					nodes.MoveNext();
					ss += "C" + nodes.Current.Value;
					if (parent.parent.CamDimension != 2)
						throw new InvalidOperationException("awefaerfvregfvg");
					return ss;
				}
			}
			/// <summary>  ＶＧのＡ軸Ｃ軸</summary>
			public string RotaryAxisAngle {
				get {
					XPathExpression query = parent.parent.xpathNavigator.Compile(xPath + "/RotaryAxisAngle");
					XPathNodeIterator nodes = parent.parent.xpathNavigator.Select(query);
					if (nodes.Count != 1) return null;
					string ss = "";
					query = parent.parent.xpathNavigator.Compile(xPath + "/RotaryAxisAngle/A");
					nodes = parent.parent.xpathNavigator.Select(query);
					nodes.MoveNext();
					ss += "A" + nodes.Current.Value;
					query = parent.parent.xpathNavigator.Compile(xPath + "/RotaryAxisAngle/B");
					nodes = parent.parent.xpathNavigator.Select(query);
					nodes.MoveNext();
					if (nodes.Current.Value != "0") throw new InvalidOperationException("awefaerfvregfvg");
					query = parent.parent.xpathNavigator.Compile(xPath + "/RotaryAxisAngle/C");
					nodes = parent.parent.xpathNavigator.Select(query);
					nodes.MoveNext();
					ss += "C" + nodes.Current.Value;
					if (parent.parent.CamDimension == 2) throw new InvalidOperationException("awefaerfvregfvg");
					return ss;
				}
			}
			/// <summary>
			/// 
			/// </summary>
			/// <param name="tool"></param>
			/// <param name="p_ncinfoSchemaVer"></param>
			/// <param name="tcnt"></param>
			/// <param name="ii"></param>
			public TAxis(Tool tool, SchemaVersion p_ncinfoSchemaVer, int tcnt, int ii) {
				this.parent = tool;
				this.xPath = "/NcInfo/Tool[" + ((int)(tcnt + 1)).ToString() + "]/Axis[" + ((int)(ii + 1)).ToString() + "]";
			}
		}
	}
}
