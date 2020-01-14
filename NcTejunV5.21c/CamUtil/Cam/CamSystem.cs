using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace CamUtil
{
	/// <summary>
	///	使用するＮＣデータの形式名であるベースＮＣデータフォーマットを設定します。データベースのツールセットの区分にも使用されます。
	///	現在は GENERAL, 5AXIS, GRAPHITE の３種類です。
	/// </summary>
	public readonly struct BaseNcForm
	{
		/// <summary>未設定</summary>
		static public BaseNcForm EMPTY = new BaseNcForm(ID.EMPTY, "EMPTY", null);
		/// <summary>GENERAL</summary>
		static public BaseNcForm GENERAL;
		/// <summary>5AXIS</summary>
		static public BaseNcForm BUHIN;
		/// <summary>グラファイト電極加工</summary>
		static public BaseNcForm GRAPHITE;

		/// <summary>ＩＤ番号</summary>
		public enum ID
		{
			/// <summary></summary>
			EMPTY,
			/// <summary>西溝口工場で決められた一般的ＮＣデータ形式のＮＣデータフォーマット</summary>
			GENERAL,
			// /// <summary>Tebisで使用するハイデンハイン専用のＮＣデータフォーマット</summary>
			// iTNC530,
			/// <summary>Tebisで使用する部品加工専用のＮＣデータフォーマット</summary>
			BUHIN,
			/// <summary>グラファイト電極加工専用のＮＣデータフォーマット</summary>
			GRAPHITE,
		}

		static BaseNcForm() {
			using (DataTable camBase = new DataTable("CamBaseNcFormat")) {
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.CamBaseNcFormat", connection)) {
					connection.Open();
					adapter.Fill(camBase);
				}
				foreach (DataRow dRow in camBase.Rows) {
					switch ((string)dRow["BaseNcFormName"]) {
					case "BUHIN":
						BUHIN = new BaseNcForm(ID.BUHIN, (string)dRow["BaseNcFormName"], (string)dRow["Code"]); break;
					case "GENERAL":
						GENERAL = new BaseNcForm(ID.GENERAL, (string)dRow["BaseNcFormName"], (string)dRow["Code"]); break;
					case "GRAPHITE":
						GRAPHITE = new BaseNcForm(ID.GRAPHITE, (string)dRow["BaseNcFormName"], (string)dRow["Code"]); break;
					}
				}
			}
		}

		/// <summary>ＮＣデータフォーマットID</summary>
		public readonly ID Id;
		/// <summary>ＮＣデータフォーマット名</summary>
		public readonly string Name;
		/// <summary>データベースで使用する区分コード</summary>
		public readonly string Code;

		/// <summary>ベースＮＣデータフォーマット名から作成するコンストラクタ</summary>
		private BaseNcForm(ID id, string name, string code) {
			Id = id;
			Name = name;
			Code = code;
		}
	}

	/// <summary>
	/// ＣＡＭシステムの種類を設定します。
	/// </summary>
	public readonly struct CamSystem
	{
		/// <summary></summary>
		public const string Empty = "Empty";
		/// <summary></summary>
		public const string Tebis = "Tebis";
		/// <summary></summary>
		public const string Dynavista2D = "Dynavista2D";
		/// <summary></summary>
		public const string WorkNC = "WorkNC";
		/*
		/// <summary></summary>
		public const string WorkNC_5AXIS = "WorkNC_5AXIS";
		*/
		/// <summary></summary>
		public const string CADCEUS = "CADCEUS";
		/// <summary></summary>
		public const string CAMTOOL = "CAMTOOL";
		/// <summary>５軸対応CAMTOOL in 2015/07/03</summary>
		public const string CAMTOOL_5AXIS = "CAMTOOL_5AXIS";
		/*
		/// <summary></summary>
		public const string Caelum = "Caelum";
		*/
		/// <summary></summary>
		public const string CADmeisterKDK = "CADmeisterKDK";

		/// <summary>使用可能なCamSystemのリスト</summary>
		public static List<CamSystem> CamSystems = new List<CamSystem>() { new CamSystem("Empty", "Empty", 2, false, 0, false) };

		static CamSystem() {
			using (DataTable camSys = new DataTable("CamSystem")) {
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.CamSystem", connection)) {
					connection.Open();
					adapter.Fill(camSys);
				}
				CamSystems.AddRange(camSys.AsEnumerable().Where(dRow => (bool)dRow["使用"])
					.Select(dRow => new CamSystem(
						(string)dRow["CAMシステム名"], (string)dRow["表示名"], (byte)dRow["次元"],
						(bool)dRow["NCSEND2表示"], (int)dRow["表示順"], (bool)dRow["TSET有無"])));
			}
			CamSystems.Sort((aa, bb) => aa.SortNo - bb.SortNo);
			return;
		}
		////////////////
		// 以上 statc //
		////////////////



		/// <summary>ＣＡＭのシステム名</summary>
		public readonly string Name;

		// 以下ＤＢ化に伴い新規作成
		/// <summary>ＣＡＭのニックネーム</summary>
		public readonly string NickName;
		/// <summary>ＣＡＭの次元</summary>
		public readonly byte Dimension;
		/// <summary>NCSENDへの表示</summary>
		public readonly bool Ncsend;
		/// <summary>表示順</summary>
		public readonly int SortNo;
		/// <summary>ツールセット情報の有無</summary>
		public readonly bool Tset;

		/// <summary>
		/// ＣＡＭのシステム名から作成するコンストラクタ
		/// </summary>
		/// <param name="name"></param>
		/// <param name="nickname"></param>
		/// <param name="dim"></param>
		/// <param name="ncsend"></param>
		/// <param name="sortno"></param>
		/// <param name="tset"></param>
		private CamSystem(string name, string nickname, byte dim, bool ncsend, int sortno, bool tset) {
			this.Name = name;
			this.NickName = nickname;
			this.Dimension = dim;
			this.Ncsend = ncsend;
			this.SortNo = sortno;
			this.Tset = tset;
		}
		/// <summary>
		/// ＣＡＭのシステム名から作成するコンストラクタ
		/// </summary>
		/// <param name="name"></param>
		public CamSystem(string name) {
			this = CamSystems.Find(cam => cam.Name == name);
			if (this.Name == null) throw new Exception(name + "は未登録のＣＡＭシステム名です");
		}

		/// <summary>
		/// ＣＡＭシステムのニックネーム
		/// コンボボックスで使用しているのでToStringのオーバーライド必要
		/// </summary>
		/// <returns></returns>
		public override string ToString() { throw new Exception("ToString() は使用できません"); }
	}

	/// <summary>
	/// ポストプロセッサの種類を設定します
	/// </summary>
	public readonly struct PostProcessor
	{
		/// <summary>ポストを決定するＣＡＭのコードです</summary>
		public readonly struct PostProcessorCode
		{
			/// <summary>ポストを決定するＣＡＭシステム名</summary>
			public readonly string CamSystemName;
			/// <summary>ポストを決定するＣＡＭのポストコード</summary>
			public readonly string PostCode;
			/// <summary>決定したポストID</summary>
			public readonly PostProcessor.ID PostID;
			/// <summary>ＤＢより作成します</summary>
			public PostProcessorCode(string camSystemName, string postProcessorCode, string postProcessorName) {
				this.CamSystemName = camSystemName;
				this.PostCode = postProcessorCode;
				this.PostID = PostProcessor.ToID(postProcessorName);
			}
		}
		/// <summary>ＩＤ番号</summary>
		public enum ID
		{
			/// <summary>Tebis 主型用固定５軸</summary>
			GEN_OM,
			/// <summary>Tebis ＤＭＵ用同時５軸</summary>
			CPC_DMU_OM,
			/// <summary>Tebis 部品用固定５軸</summary>
			GEN_BU,
			/// <summary>Tebis D500用同時５軸</summary>
			CPC_D500_BU,
			/// <summary>Tebis 部品用形状測定（後計測）</summary>
			MES_AFT_BU,
			/// <summary>Tebis 部品用基準測定（前計測）</summary>
			MES_BEF_BU,
			///// <summary>Tebis</summary>
			//iTNC530,
			///// <summary>Tebis</summary>
			//G0_ABS_4AXIS_BTU,
			/// <summary>Caelum, CADCEUS</summary>
			GENERAL,
			/// <summary>Dynavista2D</summary>
			HDNC2210_FNC208,
			/// <summary>Dynavista2D</summary>
			MCC2013_MCD1513,
			/// <summary>CAMTOOL</summary>
			TG3X,
			/// <summary>CAMTOOL_5AXIS</summary>
			TG5X,
			/// <summary>CAMTOOL_5AXIS</summary>
			TG2ZXZ,
			/// <summary>CAMTOOL_5AXIS</summary>
			TG5Xauto,
			/// <summary>CADmeisterKDK</summary>
			FANUC_TG,
			/// <summary>WorkNC ５軸</summary>
			gousei5,
			/// <summary>WorkNC ３軸</summary>
			GOUSEI,
			/// <summary>未設定</summary>
			NULL,
		}
		internal static ID ToID(string name) {
			switch (name) {
			case "CPC_D500_BU": return ID.CPC_D500_BU;
			case "CPC_DMU_OM": return ID.CPC_DMU_OM;
			case "GEN_BU": return ID.GEN_BU;
			case "GEN_OM": return ID.GEN_OM;
			case "MES_AFT_BU": return ID.MES_AFT_BU;
			case "MES_BEF_BU": return ID.MES_BEF_BU;
			case "TG3X": return ID.TG3X;

			case "GENERAL": return ID.GENERAL;
			case "HDNC2210_FNC208": return ID.HDNC2210_FNC208;
			case "MCC2013_MCD1513": return ID.MCC2013_MCD1513;
			case "TG5X": return ID.TG5X;
			case "TG2ZXZ": return ID.TG2ZXZ;
			case "TG5Xauto": return ID.TG5Xauto;
			case "FANUC_TG": return ID.FANUC_TG;
			case "gousei5": return ID.gousei5;
			case "GOUSEI": return ID.GOUSEI;
			case "NULL":
			default:
				return ID.NULL;
			}
		}
		/// <summary>XML Version V15(2019/04/15 12:00 ～ 2019/05/06 12:00)に作成されたＸＭＬのポスト名を変換する</summary>
		public static string ChangeName(string name) {
			switch (name) {
			case "G0_ABS_2AXIS": return "GEN_OM";
			case "_5AX_DMU": return "CPC_DMU_OM";
			case "_5AXIS": return "GEN_BU";
			case "_5AX_D500": return "CPC_D500_BU";
			case "Keijyo_Keisoku": return "MES_AFT_BU";
			case "Mae_Keisoku": return "MES_BEF_BU";
			case "G0": return "TG3X";
			default: return name;
			}
		}

		/// <summary>ポストＩＤに対応するポストポロセッサを返します</summary>
		/// <param name="postProcessorID"></param>
		/// <returns></returns>
		public static PostProcessor GetPost(PostProcessor.ID postProcessorID) { return postProcessors.Find(pp => pp.Id == postProcessorID); }
		/// <summary>ポスト名に対応するポストポロセッサを返します</summary>
		/// <param name="postProcessorName"></param>
		/// <returns></returns>
		public static PostProcessor GetPost(string postProcessorName) { return postProcessors.Find(pp => pp.Id == ToID(postProcessorName)); }
		/// <summary>ポストコードに対応するポストポロセッサを返します</summary>
		/// <param name="camSystemName"></param>
		/// <param name="postProcessorCode"></param>
		public static PostProcessor GetPost(string camSystemName, string postProcessorCode) {
			// ポストの間違いによる暫定処置
			if (postProcessorCode == "004 D500 TOYODA GOSEI_CPC ") {
				LogOut.CheckZantei("D500 の同時５軸のポストの間違いあり");
				postProcessorCode = "004 D500 TOYODA GOSEI_CPC";
			}
			else
			if (postProcessorCode == "004 D500 TOYODA GOSEI_CPC") {
				LogOut.CheckZantei("D500 の同時５軸のポストの間違いは修正された");
			}
			PostProcessorCode code = postProcessorCodes.Find(pp => pp.CamSystemName == camSystemName && pp.PostCode == postProcessorCode);
			if (code.CamSystemName == null) throw new Exception($"ＣＡＭシステム名：{camSystemName}, ポストコード：{postProcessorCode} の組み合わせは未登録です。");
			return postProcessors.Find(pp => pp.Id == code.PostID);
		}

		private static List<PostProcessor> postProcessors = new List<PostProcessor>();
		private static List<PostProcessorCode> postProcessorCodes = new List<PostProcessorCode>();

		static PostProcessor() {
			using (DataSet camSys = new DataSet("PostProcessor")) {
				camSys.Tables.Add("CAMPostProcessor");
				camSys.Tables.Add("CAMPostProcessorCode");
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlDataAdapter adapter1 = new SqlDataAdapter("SELECT * FROM dbo.CAMPostProcessor", connection))
				using (SqlDataAdapter adapter2 = new SqlDataAdapter("SELECT * FROM dbo.CAMPostProcessorCode", connection)) {
					connection.Open();
					adapter1.Fill(camSys.Tables["CAMPostProcessor"]);
					adapter2.Fill(camSys.Tables["CAMPostProcessorCode"]);
				}

				postProcessorCodes.AddRange(camSys.Tables["CAMPostProcessorCode"].AsEnumerable().Where(dRow1 => (bool)dRow1["使用"])
					.Select(dRow1 => new PostProcessorCode(
						(string)dRow1["CAMシステム名"], (string)dRow1["ポストプロセッサコード"], (string)dRow1["ポストプロセッサ名"])));
				postProcessors.AddRange(camSys.Tables["CAMPostProcessor"].AsEnumerable()
					.Where(dRow2 => postProcessorCodes.Any(pCode => pCode.PostID == PostProcessor.ToID((string)dRow2["ポストプロセッサ名"])))
					.Select(dRow2 => new PostProcessor((string)dRow2["ポストプロセッサ名"],
						(string)dRow2["CAMシステム名"], (string)dRow2["BaseNcFormat"], (string)dRow2["軸回転タイプ名"], (bool)dRow2["軸回転同時制御"])));
			}
			return;
		}

		/// <summary>ポストポロセッサ識別番号</summary>
		public readonly ID Id;
		/// <summary>ポストポロセッサ名</summary>
		public readonly string Name;
		/// <summary>CamSystem</summary>
		public readonly CamSystem CamSys;
		/// <summary>BaseNcForm</summary>
		public readonly BaseNcForm BaseForm;
		/// <summary>軸回転タイプ</summary>
		public readonly string AxisType;
		/// <summary>軸回転同時制御</summary>
		public readonly bool AxisDoji;

		/// <summary>DUMMY</summary>
		public readonly string Code;

		/// <summary>
		/// データベースから作成
		/// </summary>
		/// <param name="camSystemName"></param>
		/// <param name="postProcessorName"></param>
		/// <param name="baseNcFormName"></param>
		/// <param name="axisType"></param>
		/// <param name="axisDoji"></param>
		/// <returns></returns>
		private PostProcessor(string postProcessorName, string camSystemName, string baseNcFormName, string axisType, bool axisDoji) {
			this.Id = PostProcessor.ToID(postProcessorName);
			this.Name = postProcessorName;
			this.CamSys = new CamSystem(camSystemName);
			switch (baseNcFormName) {
			case "GENERAL": this.BaseForm = BaseNcForm.GENERAL; break;
			case "BUHIN": this.BaseForm = BaseNcForm.BUHIN; break;
			case "GRAPHITE": this.BaseForm = BaseNcForm.GRAPHITE; break;
			default:throw new Exception("qergfnqwergn");
			}
			this.AxisType = axisType;
			this.AxisDoji = axisDoji;
			this.Code = "dummy";
		}
	}

	/// <summary>
	/// マシンヘッドの情報を管理します。現在使用しているＣＡＭシステムは CamSystem.Tebis のみです。
	/// </summary>
	public readonly struct MachineHead
	{
		private static List<MachineHead> machineHeads;

		static MachineHead() {
			using (DataTable camMH = new DataTable("CamMachineHead")) {
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.CamMachineHead", connection)) {
					connection.Open();
					adapter.Fill(camMH);
				}
				machineHeads = camMH.AsEnumerable().Where(dRow => (bool)dRow["使用"])
					.Select(dRow => new MachineHead(dRow.Field<string>("マシンヘッド名"), dRow.Field<string>("ポストプロセッサ名"), dRow.Field<string>("設備名"))).ToList();
			}
			return;
		}

		/// <summary>ポストプロセッサとの整合</summary>
		static public void CheckPost(string head, PostProcessor post) {
			if (head == null) return;
			if (head == "") return;
			if (machineHeads.Exists(mh => mh.headName == head &&  mh.postID == post.Id) == false)
				throw new Exception($"マシンヘッド名：{head}, ポストプロセッサ名：{post.Name} は使用できない組み合わせです。");
			return;
		}
		/// <summary>加工機との整合</summary>
		static public void CheckMachine(string head, PostProcessor post, string machName) {
			if (head == null) return;
			if (head == "") return;
			// 加工機IDがNULLの場合はすべての加工機に適用
			if (machineHeads.Exists(mh => mh.headName == head && mh.machID == Machine.MachID.NULL)) return;

			Machine.MachID id = Machine.GetmachID(machName);
			if (machineHeads.Exists(mh => mh.headName == head && mh.machID == id) == false) {
				string ss = $"マシンヘッド名：{head}, 設備名：{machName} は使用できない組み合わせです。";
				if (ProgVersion.Debug) {
					System.Windows.Forms.MessageBox.Show("debug : " + ss);
					return;
				}
				throw new Exception(ss);
			}
		}


		/// <summary>マシンヘッド名</summary>
		private readonly string headName;
		/// <summary>ポストプロセッサID</summary>
		private readonly PostProcessor.ID postID;
		/// <summary>設備ID</summary>
		private readonly Machine.MachID machID;
		private MachineHead(string headName, string postName, string machName) {
			this.headName = headName;
			this.postID = PostProcessor.ToID(postName);
			this.machID = machName == null ? Machine.MachID.NULL : Machine.GetmachID(machName);
			if (machName != null && this.machID == Machine.MachID.NULL) throw new Exception("設備名：" + machName + " は登録されていません。");
		}
	}
}
