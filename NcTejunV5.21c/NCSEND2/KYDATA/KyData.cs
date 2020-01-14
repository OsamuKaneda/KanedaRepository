using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Text.RegularExpressions;
using CamUtil;

namespace NCSEND2.KYDATA
{
	/// <summary>ＮＣデータの情報</summary>
	public enum ItemNcdt {
		/// <summary>0 プロセス名（車両部品名として使用 etc. MTO123AGRL）</summary>
		ProcessName,
		/// <summary>ケーラムは加工グループ名として使用</summary>
		PartOperationName,
		/// <summary>金型の部品名（編集名として使用する）</summary>
		ProductsName,
		/// <summary>未使用（CADmeister Dynavista2D のみ設定あり）</summary>
		CoordinatesName,
		/// <summary>  ポストプロセッサ名</summary>
		PostProcessorName,
		/// <summary>  ポストプロセッサのバージョン</summary>
		PostProcessorVersion,
		/// <summary>  ＣＡＭで干渉チェックに使用されたマシンヘッド名（Tebis）</summary>
		machineHeadName,
		/// <summary>  加工原点ＸＹＺ</summary>
		OriginXYZ,
		/*
		/// <summary>1 ＮＣデータ出力日</summary>
		camOutputDateTime,
		*/
		/// <summary>1 ＮＣデータ作成時の材質設定</summary>
		camMaterial,
	}
	/// <summary>工具の情報（工具単位）</summary>
	public enum ItemTool {
		/// <summary>1 ツールセットＣＡＭ名</summary>
		ToolSetName,
		/// <summary>1 ＣＡＭシステムの工具種類</summary>
		ToolTypeCam,
		/// <summary>  工程区分（NCSPEED用）</summary>
		ProcessDiscrimination,
		/// <summary>工具の外径</summary>
		ToolDiameter,
		/// <summary>工具のコーナー半径</summary>
		ToolCornerRadius,
		/// <summary>工具の内径</summary>
		ToolDiameter_in,
	}
	/// <summary>工程の情報（工程単位）</summary>
	public enum ItemKotei {
		/// <summary>1 工程名（ＸＭＬ必須）</summary>
		Name,
		/// <summary>  工程タイプ</summary>
		Type,
		/// <summary>  工程区分</summary>
		Class,
		/// <summary>1 加工方向</summary>
		CuttingDirection,
		/// <summary>1 加工法</summary>
		MachiningMethod,
		/// <summary>  工程のＮＣサイクル名</summary>
		NcCycle,
		/// <summary>1 トレランス（すべて未指定、工程間不一致の場合 null）</summary>
		Tolerance,
		/// <summary>1 Ｚピック（すべて未指定、工程間不一致の場合 null）</summary>
		PickZ,
		/// <summary>1 XYピック（すべて未指定、工程間不一致の場合 null）</summary>
		PickX,
		/// <summary>1 Ｚ残し量（すべて未指定、工程間不一致の場合 null）</summary>
		WallThicknessZ,
		/// <summary>1 XY残し量（すべて未指定、工程間不一致の場合 null）</summary>
		WallThicknessX,
		/// <summary>1 加工長mm</summary>
		FeedLength,
		/// <summary>早送り移動長さmm</summary>
		NonFeedLength,
		/// <summary>1 加工時間min</summary>
		FeedTime,
		/// <summary>  早送り時間min</summary>
		NonFeedTime,
		/// <summary>1 ＮＣデータの送り速度mm/min（==NCSEND2実行時のＤＢの値）</summary>
		CuttingFeedRate_CSV,
		/// <summary>1 ＮＣデータの回転数min-1   （==NCSEND2実行時のＤＢの値）</summary>
		SpindleSpeed_CSV,
		/// <summary>工具の首下の長さ（有効長）</summary>
		ToolUsableLength,
		/// <summary>工具のＮＣデータ参照点（先端tipあるいはボール中心center）</summary>
		ToolReferencePoint,
		/// <summary>  ＮＣデータ出力時に入力されたコメント（Tebis）</summary>
		PostComment,
		/// <summary>アプローチの送り速度</summary>
		Approach,
		/// <summary>リトラクトの送り速度</summary>
		Retract,
	}
	/// <summary>回転軸の情報</summary>
	public enum ItemAxis
	{
		/// <summary></summary>
		AxisControlledMotion,
		/// <summary></summary>
		ClearancePlane,
		/// <summary></summary>
		AxisType,
		/// <summary></summary>
		AxisAngle,
	}

	abstract internal class KyData {
		/// <summary>
		/// 加工要領書を読み込むためのKyData クラスを作成する
		/// </summary>
		/// <param name="dnam">加工要領書のあるディレクトリ名</param>
		/// <param name="camSystem">ＣＡＭシステム名</param>
		/// <param name="originTextbox">テキストボックスで指示されたクリアランスＺ高さ（Dynavvista2D でのみ使用）</param>
		/// <returns></returns>
		internal static List<KyData> CreateKakoYoryo(string dnam, CamSystem camSystem, double? originTextbox) {
			List<KyData> KyDList = new List<KyData>();
			KyData kyD;

			// テキスト内の加工情報を保存
			switch (camSystem.Name) {
			case CamSystem.Tebis:
				foreach (string stmp in Directory.GetFiles(dnam)) {
					if (Path.GetExtension(stmp) == "" || Path.GetExtension(stmp) == ".ncd") {
						if (!File.Exists(Path.ChangeExtension(stmp, "csv"))) continue;
						KyDList.Add(new KyData_Tebis(stmp));
					}
				}
				break;
			case CamSystem.Dynavista2D:
				foreach (string stmp in Directory.GetFiles(dnam)) {
					if (Path.GetExtension(stmp) == "" || Path.GetExtension(stmp) == ".ncd") {
						KyDList.Add(new KyData_Dynavista2D(stmp, originTextbox.Value));
					}
				}
				break;
			case CamSystem.CAMTOOL:
				foreach (string stmp in Directory.GetFiles(dnam)) {
					if (Path.GetExtension(stmp) != ".csv") continue;
					KyDList.AddRange(new KyData_CAMTOOL(stmp).Divide(""));
				}
				break;
			case CamSystem.CAMTOOL_5AXIS:
				foreach (string stmp in Directory.GetFiles(dnam)) {
					if (Path.GetExtension(stmp) != ".csv") continue;
					KyDList.AddRange(new KyData_CAMTOOL_5AXIS(stmp).Divide(""));
				}
				break;
			case CamSystem.CADmeisterKDK:
				foreach (string stmp in Directory.GetFiles(dnam)) {
					if (Path.GetExtension(stmp) != ".csv") continue;
					KyDList.AddRange(new KyData_CADmeisterKDK(stmp).Divide(".DNC"));
				}
				break;
			case CamSystem.CADCEUS:
				foreach (string stmp in Directory.GetFiles(dnam + @"\nc")) {
					if (File.Exists(dnam + @"\lst\" + Path.GetFileName(stmp))) {
						KyDList.Add(new KyData_CADCEUS(stmp));
					}
				}
				break;
			case CamSystem.WorkNC:
			//case CamSystem.WorkNC_5AXIS:
				foreach (string stmp in Directory.GetFiles(dnam, "outil*.jou")) {
					kyD = new KyData_WorkNC(stmp, new double[] { 0.0, 0.0, 100.0 });
					if (kyD.FulName == null) continue;
					if (PostProcessor.GetPost(kyD.GetStringItem(ItemNcdt.PostProcessorName)).Id == PostProcessor.ID.NULL)
						throw new Exception("ＮＣデータ：" + Path.GetFileName(kyD.FulName) + " はポストプロセスが実行されていないため出力できません。");
					KyDList.Add(kyD);
				}
				break;
			default:
				throw new Exception("qwefvqbewfrbqrefhb");
			}

			// ツールセットＣＡＭを選択し決定する
			if (KyDList.Count > 0) {
				switch (camSystem.Name) {
				case CamSystem.CADmeisterKDK:
				case CamSystem.CADCEUS:
					FormSelTset selTset = new FormSelTset(camSystem.Name, KyDList);
					System.Windows.Forms.DialogResult result = selTset.ShowDialog();
					if (result == System.Windows.Forms.DialogResult.Cancel) {
						throw new Exception("agfbahefbgah");
					}
					break;
				}
			}

			return KyDList;
		}

		/// <summary>倒れ補正として処理する最小値</summary>
		private static double TaoreHoseiHanni = -0.1;
		protected static double? TaoreHoseiRyo(string offset) {
			if (offset == null) return null;
			double dd = Math.Round(Convert.ToDouble(offset), 3);
			return (dd >= 0.0 || dd <= TaoreHoseiHanni) ? (double?)null : dd;
		}




		/// <summary>ＣＳＶのデータより作成された KyData の場合</summary>
		public bool CSV { get; protected set; }

		/// <summary>部品加工か否か</summary>
		public bool Buhin {
			get {
				return (this is KyData_Tebis)
					? PostProcessor.GetPost(CamSystem.Tebis, this.nsgt1[0][1]).BaseForm.Id == BaseNcForm.ID.BUHIN
					: false;
			}
		}

		/// <summary>ファイル内の工具数</summary>
		public int CountTool { get { return CSV ? this.Divide_Tool().Length : 1; } }
		/// <summary>ファイル内の工程数</summary>
		public int CountKotei { get { return CSV ? nsgt1.Count : 1; } }
		/// <summary>加工要領書のフルファイル名</summary>
		public string FulName { get; protected set; }
		/// <summary>ＣＡＭで指定されたToolSetCAM の名称</summary>
		abstract public string TSName(int index);
		/// <summary>ＣＡＭの次元</summary>
		abstract public string Dimension { get; }
		/// <summary>最大クリアランスプレーン</summary>
		public double MaxCLR() {
			double max = Double.MinValue;
			for (int ii = 0; ii < this.CountKotei; ii++)
				if (max > GetDoubleItem(KYDATA.ItemAxis.ClearancePlane, ii).Value)
					max = GetDoubleItem(KYDATA.ItemAxis.ClearancePlane, ii).Value;
			return max;
		}

		abstract public string GetStringItem(ItemNcdt item);
		abstract public string GetStringItem(ItemTool item, NCINFO.TSetCAM tset);
		abstract public string GetStringItem(ItemKotei item, NCINFO.TSetCAM tset, int index);
		abstract public string GetStringItem(ItemAxis item, int index);

		public double? GetDoubleItem(ItemTool item, NCINFO.TSetCAM tset) {
			string ss = GetStringItem(item, tset);
			return ss == null ? (double?)null : Convert.ToDouble(ss);
		}
		public double? GetDoubleItem(ItemKotei item, NCINFO.TSetCAM tset, int index) {
			string ss = GetStringItem(item, tset, index);
			return ss == null ? (double?)null : Convert.ToDouble(ss);
		}
		public double? GetDoubleItem(ItemAxis item, int index) {
			string ss = GetStringItem(item, index);
			return ss == null ? (double?)null : Convert.ToDouble(ss);
		}

		/// <summary>1 ＮＣデータ出力日</summary>
		abstract public DateTime CamOutputDate { get; }

		/// <summary>加工開始点のＺ高さ</summary>
		protected double OrgZ;

		/// <summary>倒れ補正量のリスト</summary>
		abstract public IEnumerable<double?> TaoreList { get; }

		// ////////////////////////////////////////////////////////////////////////////
		// 加工要領がＣＳＶファイルの場合に使用するフィールド
		// ////////////////////////////////////////////////////////////////////////////
		/// <summary>ＮＣデータ全体の情報を保存するリスト</summary>
		protected string[] nsgt2 = null;
		/// <summary>各加工工程の情報を保存するリスト</summary>
		protected List<string[]> nsgt1 = null;





		protected DateTime CamOutputDateSub(DateTime? item) {
			DateTime ncOutDate;
			FileInfo fileInfo = new FileInfo(this.FulName);

			if (item == null) {
				ncOutDate = fileInfo.LastWriteTime;
			}
			else {
				ncOutDate = item.Value;
				if (ncOutDate == fileInfo.LastWriteTime.Date)
					ncOutDate = fileInfo.LastWriteTime;
				else if (ncOutDate == fileInfo.LastWriteTime.AddMinutes(5.0).Date)
					ncOutDate = fileInfo.LastWriteTime;
				else if (ncOutDate == fileInfo.LastWriteTime.AddMinutes(-5.0).Date)
					ncOutDate = fileInfo.LastWriteTime;
			}
			return ncOutDate;
		}

		protected string[] Sub(string fnam, string ddat, int icheck) {
			string[] rrr = ddat.SplitCsv();
			if (rrr.Length < icheck)
				throw new Exception($"ＣＳＶファイル{Path.ChangeExtension(fnam, ".csv")}に不正な行（{ddat}.....）があります。");
			return rrr;
		}

		/// <summary>
		/// １工程（ＣＳＶ１行）ごとに分解する。情報は元と共有している
		/// </summary>
		/// <param name="suff">フルファイル名作成の後ろに追加する文字列</param>
		/// <returns>分割された新たなKyData</returns>
		public KyData[] Divide(string suff) {
			KyData[] divide;
			if (this.CSV) {
				divide = new KyData[this.nsgt1.Count];
				for (int nsgtNo = 0; nsgtNo < this.nsgt1.Count; nsgtNo++) {
					if (this.nsgt1[nsgtNo][0].Length == 0) {
						LogOut.CheckCount("IKyData2 168", false, "ＣＳＶ先頭の文字列の長さが０ " + FulName);
						continue;
					}
					divide[nsgtNo] = (KyData)this.MemberwiseClone();
					divide[nsgtNo].FulName = Path.GetDirectoryName(FulName) + "\\" + this.nsgt1[nsgtNo][0] + suff;
					divide[nsgtNo].nsgt1 = new List<string[]>();
					divide[nsgtNo].nsgt1.Add(this.nsgt1[nsgtNo]);
				}
			}
			else
				divide = new KyData[] { this };
			return divide;
		}
		/// <summary>工具単位に分解する</summary>
		abstract public KyData[] Divide_Tool();
		/// <summary>軸単位に分解する</summary>
		abstract public KyData[] Divide_Axis();
		// //////////////////////////////
		// 以下はＣＳＶの表示関連
		// //////////////////////////////

		/// <summary>ＣＳＶの最大コラム数1</summary>
		public int CountColumn1() { return CSV ? nsgt1.Max(ss => ss.Length) : 0; }
		/// <summary>ＣＳＶの最大コラム数2</summary>
		public int CountColumn2() { return (CSV && nsgt2 != null) ? nsgt2.Length : 0; }
		//public int CountColumn2() { return (!CSV) ? 0 : (nsgt2 == null ? 0 : nsgt2.Length); }
		/// <summary>
		/// リストビューアイテムを取得する
		/// </summary>
		/// <param name="columnNo1"></param>
		/// <param name="columnNo2"></param>
		/// <returns></returns>
		public System.Windows.Forms.ListViewItem[] LVItems(int columnNo1, int columnNo2) {
			System.Windows.Forms.ListViewItem[] itemList = new System.Windows.Forms.ListViewItem[nsgt1.Count];
			List<string> items;
			for (int ii = 0; ii < nsgt1.Count; ii++) {
				items = new List<string>();
				items.Add(Path.GetFileName(FulName));
				if (nsgt2 != null) foreach (string ss in nsgt2) items.Add(ss);
				while (items.Count < columnNo2) items.Add("");
				foreach (string ss in nsgt1[ii]) items.Add(ss);
				itemList[ii] = new System.Windows.Forms.ListViewItem(items.ToArray());
			}
			return itemList;
		}
	}
}
