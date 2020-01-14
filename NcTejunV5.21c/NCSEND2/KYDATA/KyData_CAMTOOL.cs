using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using CamUtil;

namespace NCSEND2.KYDATA
{
	/// <summary>
	/// CAMTOOL の加工情報[不変]
	/// </summary>
	internal class KyData_CAMTOOL : KyData
	{
		/// <summary>
		/// コンストラクタ（ＣＳＶ）
		/// </summary>
		/// <param name="fnam">フルファイル名</param>
		public KyData_CAMTOOL(string fnam) {
			this.CSV = true;
			this.FulName = fnam;
			this.nsgt2 = null;
			this.nsgt1 = new List<string[]>();
			this.OrgZ = 100.0;

			using (StreamReader csvf = new StreamReader(Path.ChangeExtension(fnam, ".csv"), Encoding.Default)) {
				string[] stmp;
				//00ファイル名,01切削モード,02ツールセット名,03工具径,04刃先R,05工具種別,06工具名,07ホルダ名,
				//08突出し,09回転,10送り,11ピッチ（XY）,12ピッチ（Z),13仕上げ代,14クリアランスプレーン,
				//15-,16切削距離,17加工時間,18-,19最小Z,20最大Z,21Comment
				while (csvf.EndOfStream == false) {
					stmp = Sub(fnam, csvf.ReadLine(), 21);
					if (stmp[0] != "") nsgt1.Add(stmp);
				}
				if (nsgt1.Count == 0)
					throw new Exception(Path.GetFileName(fnam) + "の工具単位の加工情報が作成されていません");
				for (int ii = 0; ii < nsgt1.Count; ii++)
					if (OrgZ < Convert.ToDouble(nsgt1[ii][20]))
						OrgZ = Convert.ToDouble(nsgt1[ii][20]);
			}
		}

		public override string GetStringItem(ItemNcdt item) {
			switch (item) {
			case ItemNcdt.ProcessName:
				return Path.GetFileName(Path.GetDirectoryName(FulName));
			case ItemNcdt.PartOperationName:
				return Path.GetFileNameWithoutExtension(nsgt1[0][1]);
			case ItemNcdt.ProductsName:
				return Path.GetFileName(Path.GetDirectoryName(FulName));
			case ItemNcdt.CoordinatesName:
				return Path.GetFileName(Path.GetDirectoryName(FulName));
			case ItemNcdt.PostProcessorName:
				return PostProcessor.GetPost(CamSystem.CAMTOOL, "G0").Name;
			case ItemNcdt.PostProcessorVersion:
				return null;
			case ItemNcdt.machineHeadName:
				return null;
			case ItemNcdt.OriginXYZ:
				return "0,0," + OrgZ.ToString();
			case ItemNcdt.camMaterial:
				return "STEEL";
			default: throw new Exception("");
			}
		}
		public override string GetStringItem(ItemTool item, NCINFO.TSetCAM tset) {
			switch (item) {
			case ItemTool.ToolSetName:
				return nsgt1[0][2];
			case ItemTool.ToolTypeCam:
				return nsgt1[0][5];
			case ItemTool.ProcessDiscrimination:
				return tset.KouteiType;
			case ItemTool.ToolDiameter:
				return nsgt1[0][3];
			case ItemTool.ToolCornerRadius:
				return nsgt1[0][4];
			case ItemTool.ToolDiameter_in:
				return null;
			default: throw new Exception("");
			}
		}
		// 00ファイル名, 01切削モード, 02ツールセット名, 03工具径, 04刃先R, 05工具種別, 06工具名, 07ホルダ名, 08突出し,
		// 09回転, 10送り, 11ピッチ（XY）, 12ピッチ（Z), 13仕上げ代, 14クリアランスプレーン, 15-, 16切削距離, 17加工時間,
		// 18-, 19最小Z, 20最大Z, 21コメント,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
		public override string GetStringItem(ItemKotei item, NCINFO.TSetCAM tset, int index) {
			double ff;
			switch (item) {
			case ItemKotei.Name:
				return nsgt1[index][1];
			case ItemKotei.Type:
				return null;
			case ItemKotei.Class:
				return null;
			case ItemKotei.CuttingDirection:
				return null;
			case ItemKotei.MachiningMethod:
				return null;
			case ItemKotei.NcCycle:
				return null;
			case ItemKotei.Tolerance:
				return null;
			case ItemKotei.PickZ:
				ff = Convert.ToDouble(nsgt1[index][12].IndexOf('(') < 0 ? nsgt1[index][12] : nsgt1[index][12].Substring(0, nsgt1[index][12].IndexOf('(')));
				return ff.ToString();
			case ItemKotei.PickX:
				ff = Convert.ToDouble(nsgt1[index][11].IndexOf('(') < 0 ? nsgt1[index][11] : nsgt1[index][12].Substring(0, nsgt1[index][11].IndexOf('(')));
				return ff.ToString();
			case ItemKotei.WallThicknessZ:
				return nsgt1[index][13];
			case ItemKotei.WallThicknessX:
				return nsgt1[index][13];
			case ItemKotei.FeedLength:
				return nsgt1[index][16];
			case ItemKotei.NonFeedLength:
				return "0.0";
			case ItemKotei.FeedTime:
				if ((int)Math.Floor(Convert.ToDouble(nsgt1[index][10]) + 0.5) != 1000.0)
					throw new Exception("ＣＡＭＴＯＯＬの送り速度は1000mm/min固定です");
				string[] ftime_s = nsgt1[index][17].Split(new char[] { ':' });
				double ftime_d =
					Convert.ToInt32(ftime_s[0]) * 60.0 +
					Convert.ToInt32(ftime_s[1]) +
					Convert.ToInt32(ftime_s[2]) / 60.0;
				ff = ftime_d * Convert.ToDouble(nsgt1[index][10]) / tset.Feedrate;
				return ff.ToString();
			case ItemKotei.NonFeedTime:
				return "0.0";
			case ItemKotei.CuttingFeedRate_CSV:
				return nsgt1[index][10];
			case ItemKotei.SpindleSpeed_CSV:
				return nsgt1[index][9];
			case ItemKotei.ToolUsableLength:
				return nsgt1[index][8];
			case ItemKotei.ToolReferencePoint:
				return nsgt1[index][15];
			case ItemKotei.PostComment:
				return (nsgt1[index].Length > 21) ? nsgt1[index][21] : null;
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
				return nsgt1[index][14];
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
		public override string TSName(int index) { return nsgt1[index][2]; }
		/// <summary>次元を求める</summary>
		public override string Dimension { get { return "3"; } }
		/// <summary>倒れ補正量のリスト</summary>
		public override IEnumerable<double?> TaoreList { get { return this.nsgt1.Select(ss => TaoreHoseiRyo(ss[13])); } }


		/// <summary>工具単位に分解する</summary>
		public override KyData[] Divide_Tool() { return new KyData[] { this }; }
		/// <summary>軸単位に分解する</summary>
		public override KyData[] Divide_Axis() { return new KyData[] { this }; }
	}
}
