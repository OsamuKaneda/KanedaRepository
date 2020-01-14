using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using CamUtil;

namespace NCSEND2.KYDATA
{
	/// <summary>
	/// TEBISの加工情報[不変]
	/// </summary>
	internal class KyData_Tebis : KyData
	{
		/// <summary>マシンヘッドの情報をnsgt1から取得する</summary>
		private static string PostCode31(string[] code) {
			switch (code[1]) {
			case "5AXIS":
			case "Keijyo-Keisoku":
			case "Mae-Keisoku":
				return code[31] == "Head" ? null : code[31];
			default:
				return code[30];
			}
		}

		/// <summary>
		/// コンストラクタ（ＣＳＶ）
		/// </summary>
		/// <param name="fnam">フルファイル名</param>
		public KyData_Tebis(string fnam) {
			this.CSV = true;
			this.FulName = fnam;
			this.nsgt2 = null;
			this.nsgt1 = new List<string[]>();
			this.OrgZ = 100.0;

			// 00 バージョン,  01 ポスト名, 02 TS_Date$, 03 PP_CADNAME$, 04 PP_FILE$, 05 PP_FIRST_Z,
			// 06 PP_TOOL_MAG, 07 WZ_Name$ 08 WZ_Type$, 09 WZ_Diameter, 10 WZ_CornerRad, 11 WZ_TotalLen, 12 WZ_Length,
			// 13 WZ_UseableLen, 14 WZ_Comment$, 15 NC_ToolRef$, 16 NC_Function$, 17 NC_Material$, 18 NC_Cooling,
			// 19 NC_Speed, 20 NC_FeedPath, 21 NC_Thickness, 22 NC_Overmeasure, 23 NC_StepZ$, 24 NC_UserStep, 25 NC_Cycle$,
			// 26 NC_Tolerance, 27 Angle, 28 NC_Fraeslaenge, 29 CuttingTime, 30 MacjneName, 31 Comment
			using (StreamReader csvf = new StreamReader(Path.ChangeExtension(fnam, ".csv"), Encoding.Default)) {
				while (csvf.EndOfStream == false)
					nsgt1.Add(Sub(fnam, csvf.ReadLine(), 31));
				if (nsgt1.Count == 0)
					throw new Exception(Path.GetFileName(fnam) + "の工具単位の加工情報が作成されていません");
				// オリジンＺの作成（2009/06/30 ＢＴＵの工具分割での不具合対応）
				for (int ii = 0; ii < nsgt1.Count; ii++)
					if (OrgZ < Convert.ToDouble(nsgt1[ii][5]))
						OrgZ = Convert.ToDouble(nsgt1[ii][5]);
			}
			// ポストプロセッサーのチェック
			{
				// ポストプロセッサーの同一チェック
				for (int jj = 1; jj < nsgt1.Count; jj++) {
					if (nsgt1[0][1] != nsgt1[jj][1]) throw new Exception("ポストプロセッサー名がＮＣデータ内で同一ではない。");
					if (nsgt1[0][0] != nsgt1[jj][0]) throw new Exception("ポストプロセッサーバージョンがＮＣデータ内で同一ではない。");
					if (KyData_Tebis.PostCode31(nsgt1[0]) != KyData_Tebis.PostCode31(nsgt1[jj])) throw new Exception("マシンヘッドがＮＣデータ内で同一ではない。");
				}
				// ポストプロセッサーとマシンヘッドの組み合わせチェック
				// まだポストプロセッサーの調整がされていないため一部保留 in 2018/03/05
				MachineHead.CheckPost(KyData_Tebis.PostCode31(nsgt1[0]), PostProcessor.GetPost(CamSystem.Tebis, nsgt1[0][1]));
			}
			// 次元のチェック
			foreach (string[] ss in nsgt1) {
				if (ss[16].Length == 0) {
					NCINFO.TSetCAM tset2 = new NCINFO.TSetCAM(ss[7]);
					if (tset2.toolsetTemp.Probe == false) {
						System.Windows.Forms.MessageBox.Show(Path.GetFileName(FulName) + "には空の工程名が存在します。加工は２次元に設定しました");
						break;
					}
				}
			}
		}

		/// <summary>単一工具である KyData の工程情報の整合性をチェックする</summary>
		/// <param name="messData"></param>
		public System.Windows.Forms.DialogResult CheckKotei(List<NCINFO.NcInfoCam.MessageData> messData) {
			for (int ii = 1; ii < nsgt1.Count; ii++) if (this.TSName(0) != this.TSName(ii)) throw new Exception("");
			NCINFO.TSetCAM tset = new NCINFO.TSetCAM(this.TSName(0));
			string dim = this.Dimension;

			// 加工情報をチェック
			foreach (string[] ff in this.nsgt1) {
				if (dim != this.DimensionSub(ff))
					throw new Exception("２次元と３次元の工程が含まれている");

				if (ff[16].Length == 0) {
					// 計測データの処理
					if (tset.toolsetTemp.Probe == false) {
						throw new Exception("ＣＳＶに加工法の情報がない");
					}
					if (ff[15] != "Center" && ff[15] != "Tip")
						throw new Exception("計測データの工具参照点が\"Center\"でも\"Tip\"でもない");
				}

				double bbb = Convert.ToDouble(ff[12].Substring(1)) - tset.toolsetTemp.HolderLength;
				if (tset.toolsetTemp.ToutMatch(bbb) == false) {
					NCINFO.NcInfoCam.MessageData aa = new NCINFO.NcInfoCam.MessageData(
						"TSKLONG",
						Path.GetFileNameWithoutExtension(FulName),
						$"{tset.tscamName}のＣＳＶの突出し量（{bbb.ToString("0.0")}）がツールセットＤＢの突出し量（{tset.toolsetTemp.ToutLength.ToString("0")}）より長い");
					messData.Add(aa);
				}
			}

			// 残し量が異なる場合のメッセージを表示し、DialogResult.OK あるいは DialogResult.Cancel を返す Add in 2017/01/10
			PostProcessor pname = PostProcessor.GetPost(CamSystem.Tebis, nsgt1[0][1]);
			if (pname.Id == PostProcessor.ID.GEN_OM && dim == "3") {
				double? zz, xx;
				List<double[]> wallThicknessList3 = new List<double[]>();
				for (int jj = 0; jj < nsgt1.Count; jj++) {
					zz = this.GetDoubleItem(KYDATA.ItemKotei.WallThicknessZ, tset, jj);
					xx = this.GetDoubleItem(KYDATA.ItemKotei.WallThicknessX, tset, jj);
					if (zz.HasValue || xx.HasValue) {
						if (wallThicknessList3.Exists(dd => Math.Abs(dd[0] - zz.Value) <= 0.001 && Math.Abs(dd[1] - xx.Value) <= 0.001) == false)
							wallThicknessList3.Add(new double[] { zz ?? 0.0, xx ?? 0.0 });
					}
				}
				if (wallThicknessList3.Count > 1) {
					string form =
						$"ＮＣデータ{Path.GetFileName(FulName)}の工具{TSName(0)}は以下のように異なる加工残し量のパスが接続されています。" +
						"このままＮＣデータを作成する場合は'ＯＫ'を、中止する場合は'キャンセル'を選択してください。\n";
					for (int ii = 0; ii < wallThicknessList3.Count; ii++) {
						form += "  [CL" + (ii + 1).ToString() + "]";
						if (wallThicknessList3.Exists(dd => Math.Abs(dd[0] - dd[1]) > 0.001) == false)
							form += String.Format("XYZ={0:f3}", wallThicknessList3[ii][0]);
						else
							form += String.Format("XY={0:f3} Z={1:f3}", wallThicknessList3[ii][1], wallThicknessList3[ii][0]);
					}
					return System.Windows.Forms.MessageBox.Show(form, "接続されたパスの残し量",
						System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Warning, System.Windows.Forms.MessageBoxDefaultButton.Button2);
				}
			}
			return System.Windows.Forms.DialogResult.OK;
		}

		public override string GetStringItem(ItemNcdt item) {
			switch (item) {
			case ItemNcdt.ProcessName:
				return Path.GetFileNameWithoutExtension(nsgt1[0][3]);
			case ItemNcdt.PartOperationName:
				return Path.GetFileNameWithoutExtension(nsgt1[0][3]);
			case ItemNcdt.ProductsName:
				return Path.GetFileNameWithoutExtension(nsgt1[0][3]);
			case ItemNcdt.CoordinatesName:
				return Path.GetFileNameWithoutExtension(nsgt1[0][3]);
			case ItemNcdt.PostProcessorName:
				return PostProcessor.GetPost(CamSystem.Tebis, nsgt1[0][1]).Name;
			case ItemNcdt.PostProcessorVersion:
				return nsgt1[0][0];
			case ItemNcdt.machineHeadName:
				return KyData_Tebis.PostCode31(nsgt1[0]);
			case ItemNcdt.OriginXYZ:
				return "0,0," + OrgZ.ToString();
			case ItemNcdt.camMaterial:
				return nsgt1[0][17] != "" ? nsgt1[0][17] : "STEEL";
			default: throw new Exception("");
			}
		}
		public override string GetStringItem(ItemTool item, NCINFO.TSetCAM tset) {
			switch (item) {
			case ItemTool.ToolSetName:
				return tset.tscamName;
			case ItemTool.ToolTypeCam:
				return nsgt1[0][8];
			case ItemTool.ProcessDiscrimination:
				return tset.KouteiType;
			case ItemTool.ToolDiameter:
				return nsgt1[0][9].Substring(1);
			case ItemTool.ToolCornerRadius:
				return nsgt1[0][10].Substring(1);
			case ItemTool.ToolDiameter_in:
				return null;
			default: throw new Exception("");
			}
		}

		// 00バージョン名,		01ポスト名,		02出力日			03ＣＡＤ名		04出力ファイル名,
		// 05クリアランス高さ	06工具No,		07セット名,			08工具タイプ,	09工具径,
		// 10コーナーＲ,		11工具全長,		12工具ホルダー全長,	13加工可能深さ,	14コメント,
		// 15データ点,			16加工法,		17材料,				18クーラント,	19回転数
		// 20送り速度			21				22残し量			23Ｚピッチ		24ピッチ
		// 25サイクル			26トレランス	27工具軸			28加工長		29加工時間
		// (30Ｚ最大値)
		// 30ヘッド名			31コメント

		// V1					(5AXIS)			TS_Date$			PP_CADNAME$		PP_FILE$
		// PP_FIRST_Z			PP_TOOL_MAG		WZ_Name$			WZ_Type$		D WZ_Diameter
		// R (WZ_CornerRad)		L WZ_TotalLen	H WZ_Length			WZ_UseableLen	WZ_Comment$
		// NC_ToolRef$			NC_Function$,	NC_Material,		$PP_FLUID		NC_Speed
		// NC_FeedPath			NC_Thickness	NC_Overmeasure		NC_StepZ$		NC_UserStep
		// NC_Cycle$			NC_Tolerance	B PP_C C PP_A		NC_Fraeslaenge	(NC_Time/60.0)
		// (PP_29)
		// NC_Machine$
		public override string GetStringItem(ItemKotei item, NCINFO.TSetCAM tset, int index) {
			double ff;
			switch (item) {
			case ItemKotei.Name:
				return nsgt1[index][16].Length != 0 ? nsgt1[index][16] : "Measure";
			case ItemKotei.Type:
				return nsgt1[index][16].Length != 0 ? nsgt1[index][16] : null;
			case ItemKotei.Class:
				return nsgt1[index][14];
			case ItemKotei.CuttingDirection:
				return null;
			case ItemKotei.MachiningMethod:
				return nsgt1[index][14];
			case ItemKotei.NcCycle:
				return nsgt1[index][25];
			case ItemKotei.Tolerance:
				return nsgt1[index][26];
			case ItemKotei.PickZ:
				// "0.00"に変更する（今は従来との比較のためこのまま2010/07/15）
				return nsgt1[index][23] == "NONE" ? "0.0" : nsgt1[index][23];
			case ItemKotei.PickX:
				return nsgt1[index][24];
			case ItemKotei.WallThicknessZ:
				return nsgt1[index][22];
			case ItemKotei.WallThicknessX:
				return nsgt1[index][22];
			case ItemKotei.FeedLength:
				ff = Convert.ToDouble(nsgt1[index][28]) * 1000.0;
				return ff.ToString();
			case ItemKotei.NonFeedLength:
				return "0.0";
			case ItemKotei.FeedTime:
				if (nsgt1[index][29].Length < 7)
					throw new Exception("ＣＳＶファイル加工時間のフォーマットが異常");
				int lenH = nsgt1[index][29].Length - 8;
				double feed =
					Convert.ToDouble(nsgt1[index][29].Substring(0, 2 + lenH)) * 60 +
					Convert.ToDouble(nsgt1[index][29].Substring(3 + lenH, 2)) +
					Convert.ToDouble(nsgt1[index][29].Substring(6 + lenH, 2)) / 60;
				ff = feed * Convert.ToDouble(nsgt1[index][20]) / tset.Feedrate;
				return ff.ToString();
			case ItemKotei.NonFeedTime:
				return "0.0";
			case ItemKotei.CuttingFeedRate_CSV:
				if (nsgt1[index][20] == "0" && tset.toolsetTemp.Probe == false)
					throw new Exception(tset.toolsetTemp.ToolName + "の送り速度が設定されていない。");
				return nsgt1[index][20];    // 送り速度のチェック
			case ItemKotei.SpindleSpeed_CSV:
				return nsgt1[index][19];    // 回転数のチェック
			case ItemKotei.ToolUsableLength:
				ff = Convert.ToDouble(nsgt1[index][12].Substring(1)) - tset.toolsetTemp.HolderLength;
				return ff.ToString();
			case ItemKotei.ToolReferencePoint:
				return nsgt1[index][15];
			case ItemKotei.PostComment:
				return (nsgt1[index].Length > 31) ? nsgt1[index][31] : null;
			case ItemKotei.Approach:
				return null;
			case ItemKotei.Retract:
				return null;
			default: throw new Exception("");
			}
		}
		public override string GetStringItem(ItemAxis item, int index) {
			PostProcessor pname = PostProcessor.GetPost(CamSystem.Tebis, nsgt1[index][1]);
			if (pname.Id == PostProcessor.ID.CPC_D500_BU && ProgVersion.NotTrialVersion1)
				throw new Exception("まだポストプロセッサ'" + pname.Code + "'には対応していません。");
			switch (item) {
			case ItemAxis.AxisControlledMotion:
				if (pname.Id == PostProcessor.ID.CPC_DMU_OM) return "true";
				if (pname.Id == PostProcessor.ID.CPC_D500_BU) return "true";
				return "false";
			case ItemAxis.ClearancePlane:
				return nsgt1[index][5];
			case ItemAxis.AxisType:
				if (pname.Id == PostProcessor.ID.CPC_DMU_OM) return Angle3.JIKU_Name(Angle3.JIKU.DMU_BC);
				if (pname.Id == PostProcessor.ID.CPC_D500_BU) return Angle3.JIKU_Name(Angle3.JIKU.D500_AC);
				return Angle3.JIKU_Name(Angle3.JIKU.Euler_XYZ);
			case ItemAxis.AxisAngle:
				if (pname.Id == PostProcessor.ID.CPC_DMU_OM) {
					if (nsgt1[index][27] != "A0.B0.C0.") throw new Exception("Tebis ＤＭＵ同時５軸加工の開始軸角度が０ではない。" + nsgt1[index][27]);
					return nsgt1[index][27];
				}
				if (pname.Id == PostProcessor.ID.CPC_D500_BU) {
					if (nsgt1[index][27] + "C0." != "A0.B0.C0.") throw new Exception("Tebis Ｄ５００同時５軸加工の開始軸角度が０ではない。" + nsgt1[index][27]);
					return nsgt1[index][27] + "C0.";
				}
				return nsgt1[index][27];
			default: throw new Exception("");
			}
		}
		/// <summary>1 ＮＣデータ出力日</summary>
		public override DateTime CamOutputDate {
			get {
				return CamOutputDateSub(Convert.ToDateTime(nsgt1[0][2].Substring(3, 3) + nsgt1[0][2].Substring(0, 3) + nsgt1[0][2].Substring(6, 4)));
			}
		}
		/// <summary>ＣＡＭで指定されたToolSetCAM の名称</summary>
		public override string TSName(int index) { return nsgt1[index][7]; }
		/// <summary>倒れ補正量のリスト</summary>
		public override IEnumerable<double?> TaoreList { get { return this.nsgt1.Select(ss => TaoreHoseiRyo(ss[22])); } }


		/// <summary>工具単位に分解する</summary>
		public override KyData[] Divide_Tool() {
			if (CamUtil.ProgVersion.NotTrialVersion2) return Divide_Tool(false);
			return this.Buhin ? Divide_Tool(true) : Divide_Tool(false);
		}
		/// <summary>
		/// 工具単位に分解する。倒れ補正ごとにも分解する場合はdivTaoreをTrueに設定する
		/// </summary>
		/// <param name="divTaore">True の場合倒れ補正ごとの分解も実行する</param>
		/// <returns></returns>
		public KyData[] Divide_Tool(bool divTaore) {
			List<KyData_Tebis> divide = new List<KyData_Tebis>();
			KyData_Tebis tebis;
			IEnumerable<IEnumerable<string[]>> nsgtList;

			if (divTaore) {
				if (this.nsgt1.GroupBy(ss => new { name = ss[7], taore = TaoreHoseiRyo(ss[22]) }).Count() !=
					this.nsgt1.GroupBy(ss => new { name = ss[7], taore = TaoreHoseiRyo(ss[22]) != null }).Count()
				) throw new Exception("同一工具で倒れ補正量が異なる");
				// 連続するツールセットと倒れ補正有無ごとにグループ化する
				nsgtList = this.nsgt1.GroupUntilChanged(ss => new { name = ss[7], taore = TaoreHoseiRyo(ss[22]) });
			}
			else {
				// 連続するツールセットごとにグループ化する
				nsgtList = this.nsgt1.GroupUntilChanged(ss => ss[7]);
			}
			foreach (IEnumerable<string[]> ss in nsgtList) {
				tebis = (KyData_Tebis)this.MemberwiseClone();
				tebis.nsgt1 = ss.ToList();
				divide.Add(tebis);
			}
			return divide.ToArray();
		}
		/// <summary>軸単位に分解する</summary>
		public override KyData[] Divide_Axis() {
			List<KyData_Tebis> divide = new List<KyData_Tebis>();
			KyData_Tebis tebis;

			if (this.nsgt1.Any(ss => ss[27].Length == 0)) throw new Exception("工具軸名がＣＳＶに定義されていない");
			// 連続する工具軸ごとにグループ化する
			IEnumerable<IEnumerable<string[]>> nsgtList = this.nsgt1.GroupUntilChanged(ss => ss[27]);
			foreach (IEnumerable<string[]> ss in nsgtList) {
				tebis = (KyData_Tebis)this.MemberwiseClone();
				tebis.nsgt1 = ss.ToList();
				divide.Add(tebis);
			}
			return divide.ToArray();
		}

		/// <summary>次元を求める</summary>
		public override string Dimension { get { return DimensionSub(nsgt1[0]); } }
		/// <summary>
		/// Tebisの次元を求める
		/// </summary>
		/// <returns></returns>
		private string DimensionSub(string[] ss) {
			switch (ss[16]) {
			case "MPlan":
			case "MCont":
			case "MContR":
			case "MBore":
			case "MFeat":
			case "": // ＭＰ７００の場合
				return "2";
			default:
				return "3";
			}
		}
	}
}
