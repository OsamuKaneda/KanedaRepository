using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using CamUtil;

namespace NCSEND2.KYDATA
{
	/// <summary>
	/// TEBIS, Dynavista2D, CAMTOOL の加工情報[不変]
	/// </summary>
	internal class KyData_CADmeisterKDK : KyData
	{
		/// <summary>CADmeisterKでツールセットＣＡＭの手動設定のデータを保存する add in 2013/2/28</summary>
		public string tsetCamName;

		/// <summary>
		/// コンストラクタ（ＣＳＶ）
		/// </summary>
		/// <param name="fnam">フルファイル名</param>
		public KyData_CADmeisterKDK(string fnam) {
			this.CSV = true;
			this.FulName = fnam;
			this.nsgt2 = null;
			this.nsgt1 = new List<string[]>();
			this.OrgZ = 100.0;
			this.tsetCamName = null;

			using (StreamReader csvf = new StreamReader(Path.ChangeExtension(fnam, ".csv"), Encoding.Default)) {

				//00"ＷＳ名",01"ＯＢＪ名",02"座標系",03"工作機械名",04"マガジン名",
				//05"工具原点Ｘ",06"工具原点Ｙ",07"工具原点Ｚ",08"経路間移動高さ",
				//09"工具交換設定状態",10"シーケンス番号設定状態",11"出力モード設定状態",
				//12"任意軸経路内移動設定状態",13"コーナ加減速設定状態",14"ＮＣデータ分割設定状態",15"円弧近似設定状態",
				//16"型番名",17"工具基準位置",18"板厚",19"材質名",20"加工技術ファイル名",21"ワーク座標系番号",
				//22"高速加工制御",23"初期プログラム原点Ｘ",24"初期プログラム原点Ｙ",25"初期プログラム原点Ｚ",26"総加工時間",

				csvf.ReadLine();
				nsgt2 = csvf.ReadLine().SplitCsv();
				for (int ii = 0; ii < nsgt2.Length; ii++)
					if (nsgt2[ii].Length > 3)
						if (nsgt2[ii].Substring(0, 2) == "=\"" && nsgt2[ii].Substring(nsgt2[ii].Length - 1) == "\"")
							nsgt2[ii] = nsgt2[ii].Substring(2, nsgt2[ii].Length - 3);
				csvf.ReadLine();
				while (csvf.EndOfStream == false) {
					string[] stmp = Sub(fnam, csvf.ReadLine(), 21);
					for (int ii = 0; ii < stmp.Length; ii++)
						if (stmp[ii].Length > 3)
							if (stmp[ii].Substring(0, 2) == "=\"" && stmp[ii].Substring(stmp[ii].Length - 1) == "\"")
								stmp[ii] = stmp[ii].Substring(2, stmp[ii].Length - 3);
					nsgt1.Add(stmp);
				}
				if (nsgt1.Count == 0)
					throw new Exception(Path.GetFileName(fnam) + "の工具単位の加工情報が作成されていません");
				OrgZ = Convert.ToDouble(nsgt2[7]);
			}
		}

		public override string GetStringItem(ItemNcdt item) {
			switch (item) {
			case ItemNcdt.ProcessName:
				return nsgt2[1];
			case ItemNcdt.PartOperationName:
				return nsgt2[0];
			case ItemNcdt.ProductsName:
				return nsgt2[16];
			case ItemNcdt.CoordinatesName:
				return nsgt2[2];
			case ItemNcdt.PostProcessorName:
				return PostProcessor.GetPost(CamSystem.CADmeisterKDK, nsgt2[3]).Name;
			case ItemNcdt.PostProcessorVersion:
				return "V0.0";
			case ItemNcdt.machineHeadName:
				return null;
			case ItemNcdt.OriginXYZ:
				return nsgt2[5] + "," + nsgt2[6] + "," + nsgt2[7];
			case ItemNcdt.camMaterial:
				return "STEEL";
			default: throw new Exception("");
			}
		}
		// 					, , , , Convert.ToDouble(csvd[0][15]), null);

		public override string GetStringItem(ItemTool item, NCINFO.TSetCAM tset) {
			switch (item) {
			case ItemTool.ToolSetName:
				return tset.tscamName;
			case ItemTool.ToolTypeCam:
				return nsgt1[0][6];
			case ItemTool.ProcessDiscrimination:
				return tset.KouteiType;
			case ItemTool.ToolDiameter:
				string tDiam = nsgt1[0][7];
				switch (nsgt1[0][5]) {
				case "TP-1x2L": tDiam = "21.0"; break;
				case "TP-1x2S": tDiam = "21.0"; break;
				case "TP-1x4L": tDiam = "13.2"; break;
				case "TP-1x4S": tDiam = "13.2"; break;
				case "TP-1x8L": tDiam = "9.7"; break;
				case "TP-1x8S": tDiam = "9.7"; break;
				case "TP-3x4L": tDiam = "26.4"; break;
				case "TP-3x4S": tDiam = "26.4"; break;
				case "TP-3x8S": tDiam = "16.7"; break;
				case "TP-3x8L": tDiam = "16.7"; break;
				}
				return tDiam;
			case ItemTool.ToolCornerRadius:
				return nsgt1[0][15];
			case ItemTool.ToolDiameter_in:
				return null;
			default: throw new Exception("");
			}
		}

		//00"ＮＣデータ名",01"アタッチメント名",02"主回転角度",03"従属回転角度",
		//04"Ｔ番号",05"工具名",06"工具タイプ",07"工具径",08"カッタ名",09"工具全長",10"有効長",11"シャンク直径",12"刃数",
		//13"ホルダ名",14"首下長さ",15"刃先コーナ半径",16"テーパ角度",17"工具先端直径",
		//18"加工タイプ",19"経路名",20"区分",21"工具基準位置",22"領域名",23"コメント",24"クーラント",
		//25"切削速度",26"送り速度",27"ＰＦ速度",28"接近送り速度",29"離脱送り速度",30"減速送り速度",31"回転数",
		//32"主軸回転方向",33"トレランス",34"残し量",35"径方向ピッチ",36"軸方向ピッチ",37"経路間移動タイプ",
		//38"経路間移動高さ",39"移動高さ判別距離",40"経路内移動タイプ",41"経路内移動高さ",
		//42"ピックフィードタイプ",43"ピックフィード高さ",44"減速開始高さ",45"Ｈ軸角度",46"Ｖ軸角度",
		//47"切削長",48"早送り長",49"切削時間",50"早送り時間",51"加工時間",52"データ長",
		//53"動作範囲最小Ｘ",54"動作範囲最大Ｘ",55"動作範囲最小Ｙ",56"動作範囲最大Ｙ",57"動作範囲最小Ｚ",="動作範囲最大Ｚ",
		//="経路内最大送り速度",="経路内最大回転数",="Ｄ番号",="Ｈ番号",="径補正方法",="製品名",="補正量",="島補正量",
		//="穴はみ出し量",="切込開始",="切込終了",="輪郭ピッチ",="プログラム番号",="サイクル名",="穴数",="ネジピッチ",
		//="工具先端角度",="側端オフセット量",="サブプログラムファイル名",="サブプログラム番号",="ワーク座標系番号",="出力座標系名",
		public override string GetStringItem(ItemKotei item, NCINFO.TSetCAM tset, int index) {
			double ff;
			switch (item) {
			case ItemKotei.Name:
				return nsgt1[index][19];
			case ItemKotei.Type:
				return nsgt1[index][18];
			case ItemKotei.Class:
				return nsgt1[index][20];
			case ItemKotei.CuttingDirection:
				return null;
			case ItemKotei.MachiningMethod:
				return null;
			case ItemKotei.NcCycle:
				return null;
			case ItemKotei.Tolerance:
				return nsgt1[index][33];
			case ItemKotei.PickZ:
				return (Convert.ToDouble(nsgt1[index][36]) < 9999.0) ? nsgt1[index][36] : null;
			case ItemKotei.PickX:
				return (Convert.ToDouble(nsgt1[index][35]) < 9999.0) ? nsgt1[index][35] : null;
			case ItemKotei.WallThicknessZ:
				return (Convert.ToDouble(nsgt1[index][34]) < 9999.0) ? nsgt1[index][34] : null;
			case ItemKotei.WallThicknessX:
				return (Convert.ToDouble(nsgt1[index][34]) < 9999.0) ? nsgt1[index][34] : null;
			case ItemKotei.FeedLength:
				ff = 1000.0 * Convert.ToDouble(nsgt1[index][47]);
				return ff.ToString();
			case ItemKotei.NonFeedLength:
				ff = 1000.0 * Convert.ToDouble(nsgt1[index][48]);
				return ff.ToString();
			case ItemKotei.FeedTime:
				return nsgt1[index][49];
			case ItemKotei.NonFeedTime:
				ff = 1000.0 * Convert.ToDouble(nsgt1[index][48]) / CamUtil.LCode.NcLineCode.NcDist.RapidFeed;
				return ff.ToString();
			case ItemKotei.CuttingFeedRate_CSV:
				return nsgt1[index][26];
			case ItemKotei.SpindleSpeed_CSV:
				return nsgt1[index][31];
			case ItemKotei.ToolUsableLength:
				// 突出し量
				// CSVの「動作範囲最小Ｚ」より計算していたがカスタムマクロでは出力できないため、
				// 加工長計算時に設定することとし、ここでは標準の突出し量を代入しておく
				//ToolUsableLength = 5.0 - Convert.ToDouble(ff[57]);
				return nsgt1[index][14];
			case ItemKotei.ToolReferencePoint:
				if (nsgt1[index][21] == "TOP") return "Tip";
				throw new Exception("afqrfqhefbaerhbh");
			case ItemKotei.PostComment:
				return null;
			case ItemKotei.Approach:
				return null;
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
				return OrgZ.ToString();
			case ItemAxis.AxisType:
				return Angle3.JIKU_Name(Angle3.JIKU.Null);
			case ItemAxis.AxisAngle:
				return "A0.B0.C0.";
			default: throw new Exception("");
			}
		}
		/// <summary>1 ＮＣデータ出力日</summary>
		public override DateTime CamOutputDate { get { return CamOutputDateSub(null); } }
		/// <summary>ＣＡＭで指定されたToolSetCAM の名称</summary>
		public override string TSName(int index) { return tsetCamName; }
		/// <summary>次元を求める</summary>
		public override string Dimension { get { return "2"; } }
		/// <summary>倒れ補正量のリスト</summary>
		public override IEnumerable<double?> TaoreList { get { return this.nsgt1.Select(ss => TaoreHoseiRyo(ss[34])); } }


		/// <summary>工具単位に分解する</summary>
		public override KyData[] Divide_Tool() { return new KyData[] { this }; }
		/// <summary>軸単位に分解する</summary>
		public override KyData[] Divide_Axis() { return new KyData[] { this }; }

		// ///////////////////////////////////////////////////////////
		// ツールセットを決定するために使用する情報 add in 2019/05/07
		// ///////////////////////////////////////////////////////////
		/// <summary>工具名</summary>
		public string CsvToolName { get { return nsgt1[0][5].Replace('x', '*'); } }
		/// <summary>ホルダー名</summary>
		public string CsvHoldName { get { return nsgt1[0][13]; } }
		/// <summary>突き出し量</summary>
		public string CsvTsukiRyo { get { return nsgt1[0][14]; } }
	}
}
