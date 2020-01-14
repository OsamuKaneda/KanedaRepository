using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using CamUtil;

namespace NCSEND2.KYDATA
{
	/// <summary>
	/// Dynavbista2Dの加工情報[不変]
	/// </summary>
	internal class KyData_Dynavista2D : KyData
	{
		/// <summary>Dynavista2Dの*.logファイル内にあるTarget情報 ADD in 2013/04/04</summary>
		public readonly string Target;

		/// <summary>
		/// コンストラクタ（ＣＳＶ）
		/// </summary>
		/// <param name="fnam">フルファイル名</param>
		/// <param name="originTextbox">テキストボックスで指示されたクリアランスＺ高さ</param>
		public KyData_Dynavista2D(string fnam, double originTextbox) {
			this.CSV = true;
			this.FulName = fnam;
			this.nsgt2 = null;
			this.nsgt1 = new List<string[]>();
			this.OrgZ = originTextbox;
			this.Target = null;

			// 000 工具交換名, 001アタッチメント名, 002 ツールセット名, 003 TCコメント, 004 工具タイプ, 005 ボールタイプ, 006工具番号, 007 工具径, 008 工具全長, 009 有効切削長,
			// 010 長さ, 011 本体直径, 012 刃先コーナ半径, 013 切削角度, 014 入り口直径, 015 テーパ角度, 016 面取り直径１, 017 長さ１, 018 工具角度, 019 先端角度,
			// 020 先端半径, 021 先端長さ, 022 最大直径, 023 最小直径, 024 切削しない直径, 025 工具刃先長さ, 026 外側の直径, 027 非切削直径, 028 面取り直径２, 029 長さ２,
			// 030 切削角度２, 031 直径の呼び径の場所, 032 上部コーナ半径, 033 -, 034 -, 035 -, 036 -, 037 -, 038 -, 039 -,
			// 040 工程名, 041 工程タイプ, 042 切削時間, 043 加工時間, 044 工具首下長, 045 最短首下長, 046 工程コメント, 047 -, 048 -, 049 -,
			// 050 工程区分, 051 加工開始段, 052 加工終了段, 053 切込回数, 054 残し量壁面, 055 残し量壁面２, 056 残し量壁面３, 057 残し量壁面４, 058 残し量壁面５, 059 残し量底面,
			// 060 残し量底面２, 061 残し量底面３, 062 残し量底面４, 063 残し量底面５, 064 工具軸, 065 工具方向X, 066 工具方向Y, 067 工具方向Z, 068 -, 069 -,
			// 070 -, 071 -, 072 -, 073 -, 074 -, 075 径ピッチタイプ, 076 径ピッチ量, 077 径開始ピッチ有無, 078 径開始ピッチ量, 079 径終了ピッチ有無,
			// 080 径終了ピッチ量, 080 軸ピッチ量, 080 軸開始ピッチ有無, 080 軸開始ピッチ量, 080 軸終了ピッチ有無, 080 軸終了ピッチ量, 080 軸ピッチ固定処理, 080 -, 080 -, 080 -,
			// 090 走査方向, 090 加工方向, 090 加工開始点タイプ, 090 開始点X, 090 開始点Y, 090 開始点Z, 090 加工順序, 090 R点タイプ, 090 R点X, 090 R点Y,
			// 100 R点Z, 100 相対値／最小高さ, 100 センターもみ深さ, 100 穴加工動作タイプ, 100 固定サイクル, 100 マクロ名, 100 ドゥエル（P), 100 切り込み／シフト量（Q), 100 干渉余裕量, 100 穴底補正量,
			// 110 切削開始終了動作, 110 底面一周処理, 110 面取り量, 110 穴直径, 110 テーパー角度, 110 コーナー加工タイプ, 110 コーナー部認識値, 110 コーナー部延長量, 110 スキャンタイプ, 110 スキャン角度,
			// 120 進行方向, 120 突きパターン, 120 溝深さ, 120 溝幅, 120 トレランス, 120 -, 120 -, 120 -, 120 -, 120 -,
			// 130 工具パス延長タイプ, 130 延長量, 130 円弧処理フラグ, 130 角度, 130 円弧半径, 130 ダウン穴加工順序, 130 作成限定タイプ, 130 下限高さ, 130 上限高さ, 130 周囲加工,
			// 140 工具パス端末後退量,コーナー過負荷対応タイプ,コーナー角度,コーナーピッチ（比率）,工具パス接続,逃げ動作タイプ,高さ,長さ,引きずり長さ,ドゥエルタイプ,
			// 150 ドゥエル時間,端末逃がしタイプ,端末逃がし量,接近タイプ,ピッチ量（比率）,-,-,-,-,-,
			// 160 径補正,寿命値（距離）量,寿命値（時間）,超過比率,分割タイプ,保証加工タイプ,W軸制御,制御量,-,-,
			// 170 クーラントタイプ,減速処理フラグ,比率,-,-,-,-,-,-,-,
			// 180 送り速度自動計算, 181 アプローチ, 182 機械加工, 183 リトラクト, 184 単位, 185 スピンドル速度自動計算, 186 スピンドル出力, 187 機械加工, 188 単位, 189 -,
			// 190 ローカル安全面指示,定義軸,タイプ,余裕量／高さ,復帰動作,-,-,-,-,-,
			// 200 層内ピックタイプ,ピック高さ／余裕量,ピック限界距離,層間ピックタイプ,ピック高さ／余裕量,ピック限界距離,-,-,-,-,
			// 210 速度タイプ,付加位置タイプ,通過点指示フラグ,通過点X,通過点Y,通過点Z,-,-,-,-,
			// 220 アプローチタイプ,角度,長さ／半径／ピッチ,深さ／高さ,直線部比率,速度（直接）,速度（間接）,軸方向高さ,軸方向速度（直接）,軸方向速度（間接）,
			// 230 リトラクトタイプ,角度,長さ／半径／ピッチ,深さ／高さ,直線部比率,速度（直接）,速度（間接）,軸方向高さ,軸方向速度（直接）,軸方向速度（間接）,
			// 240 刃具名,ホルダ名,-,-,-,-,-,-,-,-,
			// 250 -,-,-,-,-
			using (StreamReader csvf = new StreamReader(Path.ChangeExtension(fnam, ".csv"), Encoding.Default)) {
				StreamReader logf;
				string ddat;

				csvf.ReadLine();
				nsgt2 = csvf.ReadLine().SplitCsv();
				csvf.ReadLine();
				while (csvf.EndOfStream == false)
					nsgt1.Add(Sub(fnam, csvf.ReadLine(), 255));
				if (nsgt1.Count == 0)
					throw new Exception(Path.GetFileName(fnam) + "の工具単位の加工情報が作成されていません");
				logf = new StreamReader(Path.ChangeExtension(fnam, ".log"), Encoding.Default);
				while (logf.EndOfStream == false) {
					ddat = logf.ReadLine();
					if (ddat.IndexOf("Target") == 0) {
						this.Target = ddat.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[2];
						break;
					}
				}
				logf.Close();
			}
		}

		/// <summary>単一工具である KyData の工程情報の整合性をチェックする</summary>
		public void CheckKotei(List<string> tuError, List<NCINFO.NcInfoCam.MessageData> messdata, NCINFO.TSetCAM tset) {
			// ＝＝＝＝＝突出し量の設定＝＝＝＝＝
			// もしＣＡＭで算出されていない場合は、ＤＢの値を用いる
			// 算出されている場合は、最小突出し量としてＣＡＭのデータを用いる
			// 焼きバメの場合の考慮はＤＢとDynavistaでは同じなため不要
			// ＤＢの突出し量よりも短い場合はエラーとする
			foreach (string[] ff in this.nsgt1) {
				if (ff[45] == "-" || ff[45] == "") {
					if (tuError.Contains(FulName + " " + tset.tscamName) != true) {
						NCINFO.NcInfoCam.MessageData aa = new NCINFO.NcInfoCam.MessageData(
							"NON_TSK",
							Path.GetFileNameWithoutExtension(FulName),
							$"突出し量が算出されていないため、ＤＢの突出し量を使用します。({tset.tscamName})");
						messdata.Add(aa);
						tuError.Add(FulName + " " + tset.tscamName);
					}
				}
				else {
					if (tset.toolsetTemp.ToutMatch(Convert.ToDouble(ff[45])) == false) {
						NCINFO.NcInfoCam.MessageData aa = new NCINFO.NcInfoCam.MessageData(
							"TSKLONG",
							Path.GetFileNameWithoutExtension(FulName),
							$"{tset.tscamName}のＣＳＶの突き出し量（{ff[45]}）がツールセットＤＢの突き出し量（{tset.toolsetTemp.ToutLength.ToString("0")}）より長い");
						messdata.Add(aa);
					}
				}
			}
		}

		public override string GetStringItem(ItemNcdt item) {
			switch (item) {
			case ItemNcdt.ProcessName:
				return Path.GetFileNameWithoutExtension(nsgt2[0]);
			case ItemNcdt.PartOperationName:
				return nsgt2[1];
			case ItemNcdt.ProductsName:
				return nsgt2[2];
			case ItemNcdt.CoordinatesName:
				return this.Target;
			case ItemNcdt.PostProcessorName:
				return PostProcessor.GetPost(CamSystem.Dynavista2D, nsgt2[4]).Name;
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
		public override string GetStringItem(ItemTool item, NCINFO.TSetCAM tset) {
			switch (item) {
			case ItemTool.ToolSetName:
				return tset.tscamName;
			case ItemTool.ToolTypeCam:
				return nsgt1[0][4];
			case ItemTool.ProcessDiscrimination:
				return tset.KouteiType;
			case ItemTool.ToolDiameter:
				return tset.toolsetTemp.Ex_diam_Dynavista(nsgt1[0]).ToString();
			case ItemTool.ToolCornerRadius:
				return nsgt1[0][12] == "-" ? "0.0" : nsgt1[0][12];
			case ItemTool.ToolDiameter_in:
				return nsgt1[0][16] != "-" ? nsgt1[0][7] : null;
			default: throw new Exception("");
			}
		}
		public override string GetStringItem(ItemKotei item, NCINFO.TSetCAM tset, int index) {
			double ff;
			switch (item) {
			case ItemKotei.Name:
				return nsgt1[index][40];
			case ItemKotei.Type:
				return nsgt1[index][41];
			case ItemKotei.Class:
				return nsgt1[index][50];
			case ItemKotei.CuttingDirection:
				return null;
			case ItemKotei.MachiningMethod:
				return (nsgt1[index][46] != "記述なし") ? nsgt1[index][46] : null;
			case ItemKotei.NcCycle:
				if (nsgt1[index][103] == "固定サイクル") return nsgt1[index][104];
				if (nsgt1[index][103] == "ユーザサイクル") return nsgt1[index][105];
				return null;
			case ItemKotei.Tolerance:
				return nsgt1[index][124];
			case ItemKotei.PickZ:
				return nsgt1[index][81] == "-" ? "0" : nsgt1[index][81];
			case ItemKotei.PickX:
				return nsgt1[index][76] == "-" ? "0" : nsgt1[index][76];
			case ItemKotei.WallThicknessZ:
				return nsgt1[index][59] == "-" ? null : nsgt1[index][59];
			case ItemKotei.WallThicknessX:
				return nsgt1[index][54] == "-" ? null : nsgt1[index][54];
			case ItemKotei.FeedLength:
				ff = Convert.ToDouble(nsgt1[index][42]) * Convert.ToDouble(nsgt1[index][182]);
				return ff.ToString();
			case ItemKotei.NonFeedLength:
				ff = (Convert.ToDouble(nsgt1[index][43]) - Convert.ToDouble(nsgt1[index][42])) * 60000;
				return ff.ToString();
			case ItemKotei.FeedTime:
				return nsgt1[index][42];
			case ItemKotei.NonFeedTime:
				ff = (Convert.ToDouble(nsgt1[index][43]) - Convert.ToDouble(nsgt1[index][42])) * 60000 / CamUtil.LCode.NcLineCode.NcDist.RapidFeed;
				return ff.ToString();
			case ItemKotei.CuttingFeedRate_CSV:
				return nsgt1[index][182];
			case ItemKotei.SpindleSpeed_CSV:
				return nsgt1[index][187];
			case ItemKotei.ToolUsableLength:
				if (nsgt1[index][45] == "-" || nsgt1[index][45] == "")
					return tset.toolsetTemp.ToutLength.ToString();
				else
					return nsgt1[index][45];
			case ItemKotei.ToolReferencePoint:
				return "Tip";
			case ItemKotei.PostComment:
				return null;

			case ItemKotei.Approach:
				return nsgt1[index][181];
			case ItemKotei.Retract:
				return nsgt1[index][183];
			default: throw new Exception("");
			}
		}
		public override string GetStringItem(ItemAxis item, int index) {
			switch (item) {
			case ItemAxis.AxisControlledMotion:
				return null;
			case ItemAxis.ClearancePlane:
				return this.OrgZ.ToString();
			case ItemAxis.AxisType:
				return Angle3.JIKU_Name(Angle3.JIKU.Null);
			case ItemAxis.AxisAngle:
				return "A0.B0.C0.";
			default: throw new Exception("");
			}
		}
		/// <summary>1 ＮＣデータ出力日</summary>
		public override DateTime CamOutputDate { get { return CamOutputDateSub(Convert.ToDateTime(nsgt2[9])); } }
		/// <summary>ＣＡＭで指定されたToolSetCAM の名称</summary>
		public override string TSName(int index) { return nsgt1[index][2].Replace('*', 'x'); }
		/// <summary>倒れ補正量のリスト</summary>
		public override IEnumerable<double?> TaoreList {
			get { return this.nsgt1.Select(ss => (ss[54] != ss[59] || ss[54] == "-") ? (double?)null : TaoreHoseiRyo(ss[54])); }
		}


		/// <summary>工具単位に分解する</summary>
		public override KyData[] Divide_Tool() {
			List<KyData_Dynavista2D> divide = new List<KyData_Dynavista2D>();
			KyData_Dynavista2D tebis;

			if (this.nsgt1.Any(ss => ss[0].Length == 0)) throw new Exception("ＣＳＶエラー");
			// 連続する工具交換名nsgt1[][0]ごとにグループ化する
			IEnumerable<IEnumerable<string[]>> nsgtList = this.nsgt1.GroupUntilChanged(ss => ss[0]);
			foreach (IEnumerable<string[]> ss in nsgtList) {
				tebis = (KyData_Dynavista2D)this.MemberwiseClone();
				tebis.nsgt1 = ss.ToList();
				divide.Add(tebis);
			}
			return divide.ToArray();
		}
		/// <summary>軸単位に分解する</summary>
		public override KyData[] Divide_Axis() { return Divide_Tool(); }

		/// <summary>次元を求める</summary>
		public override string Dimension { get { return "2"; } }
	}
}
