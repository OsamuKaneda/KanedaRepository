using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;

namespace CamUtil
{
	/// <summary>
	/// ツールセットＣＡＭの情報を管理します。
	/// </summary>
	static public class ToolSetData
	{
		static private DataSet tsDB = null;

		static ToolSetData() {
			tsDB = new DataSet();

			using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString)) {
				connection.Open();
				SqlDataAdapter adapter;

				using (adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_TSet_CAM", connection)) {
					tsDB.Tables.Add("TSet_CAM");
					adapter.Fill(tsDB.Tables["TSet_CAM"]);
				}
				using (adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_ToolSet", connection)) {
					tsDB.Tables.Add("ToolSet");
					adapter.Fill(tsDB.Tables["ToolSet"]);
				}
				using (adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_TSet_CHG", connection)) {
					tsDB.Tables.Add("TSet_CHG");
					adapter.Fill(tsDB.Tables["TSet_CHG"]);
				}
			}
			TSetTemp.Set_TSetTemp();
		}

		/// <summary>
		/// 非標準ツールセットの作成用のテーブル 2008/11/14
		/// </summary>
		private readonly struct TSetTemp
		{
			static public SqlConnection connection;
			static public SqlDataAdapter adapter;

			static public void Set_TSetTemp() {
				connection = new SqlConnection(CamUtil.ServerPC.connectionString);
				connection.Open();
				adapter = new SqlDataAdapter("SELECT * FROM dbo.toolset_Temp", connection);
				adapter.FillSchema(tsDB, SchemaType.Source, "toolset_Temp");

				// ＵＰＤＡＴＥコマンド
				SqlParameter parameter;
				adapter.UpdateCommand = new SqlCommand(
				   "UPDATE toolset_Temp " +
				   "SET [ツールセット名] = @toolset_name " +
				   "   ,[刃具精度管理名] = @tool_name " +
				   "   ,[ホルダ名] = @holder_name " +
				   "   ,[突出し] = @tout_length " +
				   "   ,[突出し補正量] = @tout_revision " +
				   "   ,[刃具外径許容誤差(＋)] = @tol_Dmax " +
				   "   ,[刃具外径許容誤差(－)] = @tol_Dmin " +
				   "   ,[突出し量許容誤差(＋)] = @tol_Lmax " +
				   "   ,[磨耗割合閾値] = @sikiichi " +
				   "   ,[作成日] = @date_create " +
				   "   ,[利用日] = @date_update " +
				   "   ,[利用回数] = @numb_update " +
				   "WHERE [ツールセットID] = @ToolSetID", connection);
				parameter = adapter.UpdateCommand.Parameters.Add("@toolset_name", SqlDbType.VarChar, 30);
				parameter.SourceColumn = "ツールセット名";
				parameter = adapter.UpdateCommand.Parameters.Add("@tool_name", SqlDbType.VarChar, 15);
				parameter.SourceColumn = "刃具精度管理名";
				parameter = adapter.UpdateCommand.Parameters.Add("@holder_name", SqlDbType.VarChar, 15);
				parameter.SourceColumn = "ホルダ名";
				parameter = adapter.UpdateCommand.Parameters.Add("@tout_length", SqlDbType.Float);
				parameter.SourceColumn = "突出し";
				parameter = adapter.UpdateCommand.Parameters.Add("@tout_revision", SqlDbType.Float);
				parameter.SourceColumn = "突出し補正量";
				parameter = adapter.UpdateCommand.Parameters.Add("@tol_Dmax", SqlDbType.Float);
				parameter.SourceColumn = "刃具外径許容誤差(＋)";
				parameter = adapter.UpdateCommand.Parameters.Add("@tol_Dmin", SqlDbType.Float);
				parameter.SourceColumn = "刃具外径許容誤差(－)";
				parameter = adapter.UpdateCommand.Parameters.Add("@tol_Lmax", SqlDbType.Float);
				parameter.SourceColumn = "突出し量許容誤差(＋)";
				parameter = adapter.UpdateCommand.Parameters.Add("@sikiichi", SqlDbType.Float);
				parameter.SourceColumn = "磨耗割合閾値";
				parameter = adapter.UpdateCommand.Parameters.Add("@date_create", SqlDbType.DateTime);
				parameter.SourceColumn = "作成日";
				parameter = adapter.UpdateCommand.Parameters.Add("@date_update", SqlDbType.DateTime);
				parameter.SourceColumn = "利用日";
				parameter = adapter.UpdateCommand.Parameters.Add("@numb_update", SqlDbType.Int);
				parameter.SourceColumn = "利用回数";
				parameter = adapter.UpdateCommand.Parameters.Add("@ToolSetID", SqlDbType.Char, 4);
				parameter.SourceColumn = "ツールセットID";
				parameter.SourceVersion = DataRowVersion.Original;
				connection.Close();
			}
		}


		/// <summary>
		/// エンドミルの直径とコーナー半径より適合するツールセットＣＡＭ名を抽出する
		/// </summary>
		/// <param name="diam"></param>
		/// <param name="crad"></param>
		/// <param name="bnf">加工対象区分（GENERAL, 5AXIS, GRAPHITE）</param>
		/// <returns></returns>
		static public List<DataRow> TSetCAM_List(double diam, double crad, BaseNcForm bnf) {
			List<string> toolSet = new List<string>();
			List<DataRow> tsetCam = new List<DataRow>();

			toolSet.AddRange(tsDB.Tables["ToolSet"].AsEnumerable()
				.Where(dRow => (string)dRow["caelum_type"] == "MIL" || (string)dRow["caelum_type"] == "SSN" || (string)dRow["caelum_type"] == "CSN")
				.Where(dRow => Math.Abs((double)dRow["diam"] - diam) <= 0.01)
				.Where(dRow => Math.Abs((double)dRow["crad"] - crad) <= 0.01)
				.Select(dRow => (string)dRow["tset_name"]));

			tsetCam.AddRange(tsDB.Tables["TSet_CAM"].AsEnumerable()
				.Where(dRow => (string)dRow["kako_taisho"] == bnf.Code)
				.Where(dRow => toolSet.Contains((string)dRow["tset_name"])));
			return tsetCam;
		}
		/// <summary>
		/// ケーラム工具名より適合するツールセットＣＡＭ名を抽出する（TSetCAM_List）
		/// </summary>
		/// <param name="tnameUNIX"></param>
		/// <param name="bnf">加工対象区分（GENERAL, 5AXIS, GRAPHITE）</param>
		/// <returns></returns>
		static public List<DataRow> TSetCAM_List(string tnameUNIX, BaseNcForm bnf) {
			List<DataRow> tsetCam = tsDB.Tables["TSet_CAM"].AsEnumerable()
				.Where(dRow => dRow.Field<string>("tool_name_kinf") == tnameUNIX)
				.Where(dRow => dRow.Field<string>("kako_taisho") == bnf.Code).ToList();
			return tsetCam;
		}

		/// <summary>
		/// 刃具精度管理名とホルダー名と突出し量より適合するツールセットＣＡＭ名を抽出する
		/// </summary>
		/// <param name="tnam"></param>
		/// <param name="hnam"></param>
		/// <param name="tsuk"></param>
		/// <param name="bnf">加工対象区分（GENERAL, 5AXIS, GRAPHITE）</param>
		/// <returns></returns>
		static public List<DataRow> TSetCAM_List(string tnam, string hnam, double tsuk, BaseNcForm bnf) {
			List<string> tsetNam = new List<string>();
			List<string> tcamNam = new List<string>();
			List<DataRow> tsetCam = new List<DataRow>();

			tsetNam.AddRange(tsDB.Tables["ToolSet"].AsEnumerable()
				.Where(dRow => (string)dRow["tool_name"] == tnam)
				.Where(dRow => (string)dRow["holder_name"] == hnam)
				.Where(dRow => (double)dRow["tout_length"] >= tsuk)
				.Select(dRow => (string)dRow["tset_name"]));

			tcamNam.AddRange(tsDB.Tables["TSet_CHG"].AsEnumerable()
				.Where(dRow => tsetNam.Contains((string)dRow["tset_name"]))
				.Select(dRow => (string)dRow["tset_name_CAM"]).Distinct());

			tsetCam.AddRange(tsDB.Tables["TSet_CAM"].AsEnumerable()
				.Where(dRow => (string)dRow["kako_taisho"] == bnf.Code)
				.Where(dRow => tcamNam.Contains((string)dRow["tset_name_CAM"])));
			return tsetCam;
		}






		/// <summary>
		/// ツールセットＣＡＭ情報[不変]
		/// </summary>
		abstract public class TSetCAM
		{
			/// <summary>ツールセットＣＡＭの名前</summary>
			public readonly string tscamName;

			/// <summary>ツールセットＣＡＭのデータ</summary>
			protected readonly DataRow dRowCAM;

			/// <summary>クーラントのコード</summary>
			public readonly Coolant coolant;

			/// <summary>
			/// 工具半径補正量
			/// （M20,M24,PTのタップとスプルーロックに使用）ADD in 2007/11/03
			/// （刃具倒れ式の数値にも使用）2009/03 ＶＧより
			/// </summary>
			public double? RadRevision { get { return dRowCAM.Field<double?>("r_revision"); } }
			/// <summary>工具長さ補正量（測定再加工用） ADD in 2009/02/02</summary>
			public double? LenRevision { get { return dRowCAM.Field<double?>("l_revision"); } }

			/// <summary>精度区分。加工モードの選択に使用する（ARA,CHU,SIA1,SIA2など）</summary>
			public string Accuracy { get { return (string)dRowCAM["accuracy"]; } }
			/// <summary>工具寿命のベースの値</summary>
			public double LifeMaxBase { get { return (double)dRowCAM["life_max"]; } }
			/// <summary>工具長の磨耗量の最大値</summary>
			public double Tol_L_After { get { return (double)dRowCAM["quarityL_after"]; } }
			/// <summary>工具径の磨耗量の最大値</summary>
			public double Tol_D_After { get { return (double)dRowCAM["quarityD_after"]; } }
			/// <summary>加工の減速比率(add 2014/12/11)</summary>
			public double Gensoku { get { return (double)dRowCAM["gensoku"]; } }


			// NCSENDのToolsetInfoと統合するために追加 2010/09/14

			/// <summary>工程区分【ＸＭＬに代入される】Add 2009/05/15 </summary>
			public string KouteiType { get { return (string)dRowCAM["koutei"]; } }


			/// <summary>
			/// ツールセットＣＡＭ名から作成するコンストラクタ（ツールセット情報は仮）
			/// </summary>
			/// <param name="tsCAM">ツールセットＣＡＭ名</param>
			public TSetCAM(string tsCAM) {
				tscamName = tsCAM;
				dRowCAM = tsDB.Tables["TSet_CAM"].AsEnumerable().Where(dRow => tsCAM == (string)dRow["tset_name_CAM"]).FirstOrDefault();
				if (dRowCAM == null) throw new Exception(tsCAM + "はデータベースに存在しないツールセット（ＣＡＭ）です");

				coolant = new Coolant((string)dRowCAM["cool"]);

				return;
			}
		}


		/// <summary>
		/// ツールセットの情報[不変]
		/// </summary>
		public class ToolSet : ICloneable
		{
			private readonly DataRow dRowTS;

			/// <summary>クローン</summary>
			public object Clone() { return this.MemberwiseClone(); }


			// /////////////////////
			// ツールセット基本情報
			// /////////////////////
			/// <summary>ツールセット名</summary>
			public readonly string tset_name;
			/// <summary>ツールセットＩＤ（非標準の場合は"0500"以下）</summary>
			public readonly string ID;
			/// <summary>元になったツールセット名（フリーのツールセットの場合）</summary>
			public string TsetBaseName {
				get {
					if (Convert.ToInt32(ID) <= 500) return m_tset_base_name;
					return tset_name;
				}
			}
			/// <summary>元になったツールセット名（フリーのツールセットの場合）</summary>
			protected readonly string m_tset_base_name;
			/// <summary>突出し量の整合（切り上げて大小比較）</summary>
			public bool ToutMatch(double length) { return ((int)Math.Ceiling(length) <= ToutLength); }
			/// <summary>突出し量</summary>
			public int ToutLength { get { return (int)Math.Ceiling(m_tout_length); } }
			/// <summary>突出し量</summary>
			protected readonly double m_tout_length;


			// /////////////////////
			// ツールセット情報
			// /////////////////////
			/// <summary>突出し補正量</summary>
			public double ToutHosei { get { return (double)dRowTS["tout_revision"]; } }
			/// <summary>突出し量許容誤差</summary>
			public double TolLmax { get { return (double)dRowTS["tol_Lmax"]; } }
			/// <summary>刃具径許容誤差</summary>
			public double TolDmax { get { return (double)dRowTS["tol_Dmax"]; } }
			/// <summary>刃具径許容誤差</summary>
			public double TolDmin { get { return (double)dRowTS["tol_Dmin"]; } }
			/// <summary>工具番号固定（in machine_magazine）限定</summary>
			public bool FixedNumber { get { return (bool)dRowTS["FixedNumber"]; } }


			// /////////////
			// 刃具精度管理
			// /////////////
			/// <summary>刃具精度管理工具名</summary>
			public string ToolName { get { return (string)dRowTS["tool_name"]; } }
			// 工具表に表示する工具を特定するために追加 2010/09/16
			/// <summary>工具の径精度（＋側）</summary>
			public double QuarityDiam_p { get { return (double)dRowTS["quarityDiam_p"]; } }
			/// <summary>工具の径精度（－側）</summary>
			public double QuarityDiam_m { get { return (double)dRowTS["quarityDiam_m"]; } }


			// /////////
			// 刃具種類
			// /////////
			/// <summary>工具形状名</summary>
			public string ToolFormName { get { return (string)dRowTS["tool_form_name"]; } }
			/// <summary>工具形状タイプ</summary>
			public string ToolFormType { get { return (string)dRowTS["tool_form_type"]; } }
			/// <summary>工具長計測時シフト固定値</summary>
			public double? MeasShift { get { return dRowTS.Field<double?>("measure_shift"); } }
			/// <summary>ケーラム工具種類名</summary>
			public string CutterTypeCaelum { get { return (string)dRowTS["caelum_type"]; } }
			/// <summary>切削条件（寿命含む）調整のタイプ</summary>
			public string CondType { get { return (string)dRowTS["condition_type"]; } }
			/// <summary>工具表に表示する工具種類の名称(add 2010/01/21)</summary>
			public string CutterTypeName { get { return (string)dRowTS["cutter_type_name"]; } }
			/// <summary>測定子の場合true(add 2013/04/10)</summary>
			public bool Probe { get { return CutterTypeCaelum == "STY"; } }
			/// <summary>工具回転方向 M04:reverse (add 2014/12/09)</summary>
			public string M0304 { get { return dRowTS.Field<string>("m0304"); } }

			/// <summary>工具計測タイプ</summary>
			public char MeasType { get { return ((string)dRowTS["measure_type"])[0]; } }
			/// <summary>工具計測タイプ（DMG）</summary>
			public char MeasTypeDMG {
				get {
					switch (ToolFormType) {
					case "TAP":     // タップ
						return ((string)dRowTS["measure_type"]).Replace('R', 'Q')[0];
					default:
						return ((string)dRowTS["measure_type"])[0];
					}
				}
			}
			/// <summary>工具計測タイプ（部品加工機）(add 2014/12/09)</summary>
			/// <remarks>
			/// １．「刃具形状タイプ」が RMIL(ラットエンドミル) の場合、「部品の径測定識別記号」は 'E' になる
			///			ただし、ＤＢの「径測定識別記号」は 'B' でなければならない
			/// ２．「刃具形状タイプ」が EMIL(フラットエンドミル)、FMIL(フルバック) の場合、「部品の径測定識別記号」は 'F' になる
			///			ただし、ＤＢの「径測定識別記号」は 'E' あるいは 'F' でなければならない
			/// ３．「刃具形状タイプ」が MEN(面取り)、CTAP(テーパタップ) の場合、「部品の径測定識別記号」は 'V' になる
			///			ただし、ＤＢの「径測定識別記号」は 'Q' でなければならない
			/// ４．その他の「刃具形状タイプ」の場合はＤＢの「径測定識別記号」がそのまま「部品の径測定識別記号」になる
			///			ただし、ＤＢの「径測定識別記号」は 'E' あるいは 'F' であってはならない。
			/// </remarks>
			public char MeasType5AXIS {
				get {
					switch (ToolFormType) {
					case "RMIL":        // ラットエンドミル
						if (((string)dRowTS["measure_type"])[0] != 'B')
							throw new InvalidOperationException("ツールセットＤＢの刃具形状タイプエラー");
						return 'E';
					case "EMIL":        // フラットエンドミル
					case "FMIL":        // フルバック
						if (((string)dRowTS["measure_type"])[0] != 'E' && ((string)dRowTS["measure_type"])[0] != 'F')
							throw new InvalidOperationException("ツールセットＤＢの刃具形状タイプエラー");
						return 'F';
					case "MEN":         // 面取り
					case "CTAP":        // テーパタップ
						if (((string)dRowTS["measure_type"])[0] != 'Q')
							throw new InvalidOperationException("ツールセットＤＢの刃具形状タイプエラー");
						return 'V';
					default:
						if (((string)dRowTS["measure_type"])[0] == 'E' || ((string)dRowTS["measure_type"])[0] == 'F')
							throw new InvalidOperationException("ツールセットＤＢの刃具形状タイプエラー");
						return ((string)dRowTS["measure_type"])[0];
					}
				}
			}


			// /////////
			// 刃具形状
			// /////////
			/// <summary>工具直径</summary>
			public double Diam { get { return (double)dRowTS["diam"]; } }
			/// <summary>コーナー半径</summary>
			public double Crad { get { return (double)dRowTS["crad"]; } }
			/// <summary>工具内径（ＲＣテーパタップの先端径に使用）</summary>
			public double? Diam_in { get { return dRowTS.Field<double?>("diam_in"); } }
			/// <summary>刃長（ADD in 2007/11/04）</summary>
			public double Hacho { get { return dRowTS.Field<double?>("hacho") ?? 0.0; } }
			/// <summary>シャンク径(add 2010/09/03)</summary>
			public double Diam_shank { get { return (double)dRowTS["diam_shank"]; } }
			/// <summary>最大許容トルク(add 2015/02/03)</summary>
			public double? Torque { get { return dRowTS.Field<double?>("torque"); } }

			// /////////
			// ホルダー
			// /////////
			// NCSEND2 のtollsetINfoにあわせて追加 2010/09/10（近いうちに統合すること）
			/// <summary>ホルダー名</summary>
			public string HolderName { get { return (string)dRowTS["holder_name"]; } }
			/// <summary>ホルダー長さ</summary>
			public double HolderLength { get { return (double)dRowTS["holder_length"]; } }
			/// <summary>ツールセット全長</summary>
			public double TsetLength { get { return m_tout_length + (double)dRowTS["holder_length"]; } }
			/// <summary>ホルダー径</summary>
			public double HolderDiameter {
				get {
					return
					   Math.Max(
					   (double)dRowTS["holder_diam1"], Math.Max(
					   (double)dRowTS["holder_diam2"], Math.Max(
					   (double)dRowTS["holder_diam3"], Math.Max(
					   (double)dRowTS["holder_diam4"], Math.Max(
					   (double)dRowTS["holder_diam5"], Math.Max(
					   (double)dRowTS["holder_diam6"], Math.Max(
					   (double)dRowTS["holder_diam7"], Math.Max(
					   (double)dRowTS["holder_diam8"], Math.Max(
					   (double)dRowTS["holder_diam9"],
					   (double)dRowTS["holder_diamA"]))))))))
					   );
				}
			}
			/// <summary>工具表に表示するホルダーの名称(add 2010/01/21)</summary>
			public string HolderTypeName { get { return (string)dRowTS["holder_type_name"]; } }
			/// <summary>対応する最小シャンク径(add 2010/09/03)</summary>
			public double HolderShankMin { get { return (double)dRowTS["shankD_min"]; } }
			/// <summary>対応する最大シャンク径(add 2010/09/03)</summary>
			public double HolderShankMax { get { return (double)dRowTS["shankD_max"]; } }

			/// <summary>
			/// 標準の工具情報を作成する
			/// </summary>
			/// <param name="tsname"></param>
			public ToolSet(string tsname) {
				dRowTS = tsDB.Tables["ToolSet"].AsEnumerable().Where(dRow => tsname == (string)dRow["tset_name"]).FirstOrDefault();
				if (dRowTS == null) throw new Exception("wefbwqarhbfqha");

				tset_name = (string)dRowTS["tset_name"];
				ID = (string)dRowTS["tset_ID"];
				m_tout_length = (double)dRowTS["tout_length"];
				m_tset_base_name = null;
			}
			/// <summary>
			/// 非標準の工具情報を作成する
			/// </summary>
			/// <param name="tsname"></param>
			/// <param name="ttsk"></param>
			public ToolSet(string tsname, double ttsk) {
				dRowTS = tsDB.Tables["ToolSet"].AsEnumerable().Where(dRow1 => tsname == (string)dRow1["tset_name"]).FirstOrDefault();
				if (dRowTS == null) throw new Exception("wefbwqarhbfqha");
				if (ttsk <= (double)dRowTS["tout_length"]) throw new Exception("wefbwqarhbfqha");

				DataRow dRow = NonStdIdSet(Math.Ceiling(ttsk), null);
				m_tset_base_name = tsname;
				tset_name = (string)dRow["ツールセット名"];
				ID = (string)dRow["ツールセットID"];
				m_tout_length = (double)dRow["突出し"];
			}



			// ////////////////////////////////////////////////////////
			// 以下は工具径、コーナー半径の特殊処理に対応するため設定
			// 
			// MENCUT100, MENCUT250（TYPE D:直径 N:内径 R:コーナー半径）
			// TOLDAT, LIST.KINF ....	DRL D10.0 N2.0 R0.0
			//	（正しい値）			DRL D24.4 N7.0 R0.0
			//
			// ケーラムＣＡＭ登録....	(TAP) D 2.0 N0.0 R0.0
			// （＝加工要領書）			(TAP) D 7.0 N0.0 R0.0
			//
			// ＫＤＡＴＡ....			DRL D10.0 N0.0 R4.0
			//	（Ｉコードのため）		DRL D24.4 N0.0 R8.7
			//
			// /////////////////////////////////////////////////////////

			internal const string mencut080 = "MENCUT080";
			internal const string mencut100 = "MENCUT100";
			internal const string mencut250 = "MENCUT250";
			internal const string taper_1_1H = "TP-1*1H";
			internal const string taper_1_2H = "TP-1*23*4H";
			internal const string taper_1_4H = "TP-1*43*8H";
			internal const string taper_1_8H = "TP-1*8H";
			internal const string taper_1_8 = "TP-1*8";
			internal const string taper_1_4 = "TP-1*4";
			internal const string taper_3_8 = "TP-3*8";
			internal const string taper_1_2 = "TP-1*2";
			internal const string taper_3_4 = "TP-3*4";
			internal const string taper_1_1 = "TP-1*1";

			/// <summary>加工機にわたす（インデックスファイル用）工具径を出力する（diam_inが存在する時はdiam_inとなる）</summary>
			public double Ex_diam_Index { get { return Diam_in ?? Diam; } }

			/// <summary>Dynavista内部のごまかし工具径を調整する</summary>
			public double Ex_diam_Dynavista(string[] ff) {
				if (ff[16] != "-") {
					return Convert.ToDouble(ff[16]);    // 面取り工具の場合
				}
				else {
					switch (ToolName) {
					case taper_1_8:
					case taper_1_4:
					case taper_3_8:
					case taper_1_2:
					case taper_3_4:
					case taper_1_1:
					case taper_1_8H:
					case taper_1_4H:
					case taper_1_2H:
					case taper_1_1H:
						return Diam;
					default:
						return Convert.ToDouble(ff[7]);
					}
				}
			}

			// ///////////////////
			// G100のＮＣ行の作成
			// ///////////////////	
			/// <summary>径測定識別記号と計測直径（Ｄコード）を作成する</summary>
			public string DCode { get { return MeasType.ToString() + (Ex_diam_Index * 1000).ToString("0"); } }
			/// <summary>接触式の工具長計測のシフトコード（Ｉコード）を作成する</summary>
			public string ICode {
				get {
					double ShiftR;
					ShiftR = Shift_r(Machine.ToolMeasureType.接触式);
					if (ShiftR > 2.5)
						return "I" + (ShiftR * 1000).ToString("0");
					else
						return "";
				}
			}

			/// <summary>
			/// 工具長計測のシフト値を出力する
			/// </summary>
			/// <param name="mType">工具測定方式</param>
			/// <returns>工具長計測のシフト値</returns>
			public double Shift_r(Machine.ToolMeasureType mType) {

				//工具長測定時の径方向シフト量
				//○接触式
				//　①ＤＢに値ある：径測定識別記号が"EFV"であればその値、そうでなければ０
				//　②内径がある　：径測定識別記号が"EFVQ"であれば（[内径]/2.0-[コーナーR]）、そうでなければ０
				//　③その他の場合：径測定識別記号が"EFV"であれば（[工具径]/2.0-[コーナーR]）、そうでなければ０
				//　また、結果として出力されたシフト量が2.5以下であれば出力しない
				//○レーザー
				//　①ＤＢに値ある：その値
				//　②その他の場合：径測定識別記号が"EF"で工具径が１０以上であれば（[工具径]/2.0-[コーナーR]-0.5）、
				//　　そうでなければ０
				//

				switch (mType) {
				case CamUtil.Machine.ToolMeasureType.接触式:
					if (MeasShift.HasValue) {
						switch (MeasType) {
						case 'E':
						case 'F':
						case 'V':
							return MeasShift.Value;
						default:
							return 0.0;
						}
					}
					else if (Diam_in.HasValue) {
						switch (MeasType) {
						case 'E':
						case 'F':
						case 'V':
						case 'Q':
							return (Diam_in.Value / 2.0) - Crad;
						default:
							return 0.0;
						}
					}
					else {
						switch (MeasType) {
						case 'E':
						case 'F':
						case 'V':
							return (Diam / 2.0) - Crad;
						default:
							return 0.0;
						}
					}
				case CamUtil.Machine.ToolMeasureType.レーザー:
				case CamUtil.Machine.ToolMeasureType.画像式:
					if (MeasShift.HasValue) {
						return MeasShift.Value;
					}
					else if (Diam >= 10.0) {
						switch (MeasType) {
						case 'E':
						case 'F':
							return (Diam / 2.0) - Crad - 0.5;
						default:
							return 0.0;
						}
					}
					else
						return 0.0;
				default:
					throw new Exception("esfgvwerf");
				}
			}

			/// <summary>
			/// 工具径計測の工具軸方向シフト値を出力する
			/// </summary>
			/// <returns>工具径計測のシフト値</returns>
			public double? Shift_l() {
				switch (MeasType5AXIS) {
				case 'A':
				case 'D':
					return 1.0 + (Diam / 2.0) * Math.Tan(30.0 * Math.PI / 180.0);
				case 'B':
					return 1.0 + (Diam / 2.0);
				case 'E':
				case 'F':
					return 1.0 + Crad;
				case 'Q':
				case 'R':
					return 5.0;
				default:
					return (double?)null;
				}
			}

			/// <summary>
			/// 非標準のツールセットを流用あるいは作成する
			/// </summary>
			/// <param name="length">ツールセット全長。事前に丸めておくこと</param>
			/// <param name="exceptID">流用を除外するＩＤのリスト</param>
			private DataRow NonStdIdSet(double length, List<int> exceptID) {
				DataRow dRow = null;
				string aaa;

				TSetTemp.adapter.Fill(tsDB, "toolset_Temp");
				dRow = tsDB.Tables["toolset_Temp"].AsEnumerable()
					.Where(dRow1 => this.ToolName == (string)dRow1["刃具精度管理名"])
					.Where(dRow1 => this.HolderName == (string)dRow1["ホルダ名"])
					.Where(dRow1 => length <= (double)dRow1["突出し"] && length >= (double)dRow1["突出し"] * 0.8)
					.Where(dRow1 => exceptID == null || exceptID.Contains(Convert.ToInt32(dRow1["ツールセットID"])) == false)
					.FirstOrDefault();
				if (dRow != null) {
					// ///////////////////////////////////////
					// 以前作成した非標準のツールセットを流用
					// ///////////////////////////////////////

					// 現在のＤＢの値で更新しておく 2012/08/30
					dRow["突出し補正量"] = this.ToutHosei;
					dRow["突出し量許容誤差(＋)"] = this.TolLmax;
					dRow["刃具外径許容誤差(＋)"] = this.TolDmax;
					dRow["刃具外径許容誤差(－)"] = this.TolDmin;

					if (dRow["利用回数"] == DBNull.Value)
						dRow["利用回数"] = 2;
					else {
						if ((DateTime)dRow["利用日"] < DateTime.Now.AddDays(-3))
							dRow["利用回数"] = (int)dRow["利用回数"] + 1;
					}
					if (((string)dRow["ツールセット名"]).IndexOf("FREETOOLSET") == 0) {
						aaa = this.ToolName + "+" + this.HolderName + "_" + ((double)dRow["突出し"]).ToString("0");
						if (aaa.Length <= 30) dRow["ツールセット名"] = aaa;
					}
					dRow["利用日"] = DateTime.Now;
				}
				else {
					// ///////////////////////////////////////////////////////
					// 新たな非標準のツールセットを作成（最も古いものを変更）
					// ///////////////////////////////////////////////////////

					// 最も古い非標準ツールセットを探す
					TSetTemp.adapter.Fill(tsDB, "toolset_Temp");
					dRow = tsDB.Tables["toolset_Temp"].AsEnumerable().OrderBy(dRow1 => (DateTime)dRow1["利用日"]).First();
					if (((DateTime)dRow["利用日"]).AddMonths(1) > DateTime.Now)
						LogOut.CheckCount("ToolSetData 719", false, "非標準のツールセットの数が不足しつつあります");

					// 最も古い非標準ツールセットを変更し新たなツールセットを作成する。
					aaa = this.ToolName + "+" + this.HolderName + "_" + length.ToString("0");
					if (aaa.Length <= 30)
						dRow["ツールセット名"] = aaa;
					else
						dRow["ツールセット名"] = "FREETOOLSET" + ((string)dRow["ツールセットID"]).Substring(1);
					dRow["刃具精度管理名"] = this.ToolName;
					dRow["ホルダ名"] = this.HolderName;
					dRow["突出し"] = length;
					dRow["突出し補正量"] = this.ToutHosei;
					dRow["突出し量許容誤差(＋)"] = this.TolLmax;
					dRow["刃具外径許容誤差(＋)"] = this.TolDmax;
					dRow["刃具外径許容誤差(－)"] = this.TolDmin;
					dRow["磨耗割合閾値"] = 5;
					dRow["作成日"] = DateTime.Now;
					dRow["利用日"] = DateTime.Now;
					dRow["利用回数"] = 1;
				}
				// データベースの更新
				TSetTemp.adapter.Update(tsDB, "toolset_Temp");
				return dRow;
			}
		}

		/// <summary>
		/// クーラント情報
		/// </summary>
		public readonly struct Coolant
		{
			/// <summary>
			/// クーラント区分ＩＤ
			/// </summary>
			public enum ID
			{
				/// <summary>クーラント　水油、外部</summary>
				waterOut,
				/// <summary>クーラント　水油、内部</summary>
				waterIn,
				/// <summary>クーラント　エア、外部</summary>
				airOut,
				/// <summary>クーラント　エア、内部</summary>
				airIn,
				/// <summary>クーラント　無し</summary>
				nonCool,
			}
			/// <summary>
			/// テキサス出力クーラントナンバー
			/// </summary>
			/// <param name="coolID"></param>
			/// <returns></returns>
			public static int TexasCoolantNo(ID coolID) {
				switch (coolID) {
				case ID.waterOut: return 4;     // 外部クーラント（水）
				case ID.waterIn: return 2;      // 内部クーラント（水）
				case ID.airOut: return 1;       // 外部クーラント（エア）
				case ID.airIn: return 6;        // 内部クーラント（エア）
				case ID.nonCool: return 3;      // クーラントオフ
				default: throw new Exception("wearfqaerf");
				}
			}
			/// <summary>
			/// 各クーラントの区分名称
			/// </summary>
			/// <param name="coolID"></param>
			/// <returns></returns>
			public static string Message(ID coolID) {
				switch (coolID) {
				case ID.waterOut: return "外部（水）";
				case ID.waterIn: return "内部（水）";
				case ID.airOut: return "外部（エア）";
				case ID.airIn: return "内部（エア）";
				case ID.nonCool: return "オフ";
				default: throw new Exception("wearfqaerf");
				}
			}




			/// <summary>クーラント機能ID（"WATER_OUT", "WATER_IN", ....）</summary>
			public readonly ID id;
			/// <summary>クーラント機能名（"WATER_OUT", "WATER_IN", ....）</summary>
			public readonly string name;

			/// <summary>唯一のコンストラクタ</summary>
			public Coolant(string p_name) {
				switch (p_name) {
				case "WATER_OUT": this.id = ID.waterOut; break;
				case "WATER_IN": this.id = ID.waterIn; break;
				case "AIR_OUT": this.id = ID.airOut; break;
				case "AIR_IN": this.id = ID.airIn; break;
				case "NON_COOL": this.id = ID.nonCool; break;
				default: throw new Exception("");
				}
				this.name = p_name;
			}

			/// <summary>クーラントＩＤ（部品加工のＮＣデータ用）</summary>
			public string CoolCode_BuhinNC() {
				switch (this.id) {
				case ID.waterOut: return "W08.";
				case ID.waterIn: return "W12.";
				case ID.nonCool: return "";
				default: throw new Exception("qerfbqherfbh");
				}
			}
		}
	}
}
