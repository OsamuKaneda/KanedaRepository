using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;

namespace CamUtil
{
	/// <summary>
	/// すべての加工機の情報を持つクラスです。
	/// </summary>
	static public class Machine
	{
		/// <summary>加工機ＩＤリスト</summary>
		public enum MachID
		{
			/// <summary>未設定の工具</summary>
			NULL,
			///// <summary></summary>
			//BTU_14_4AX,
			/// <summary></summary>
			D500,
			/// <summary></summary>
			DMU200P,
			/// <summary></summary>
			DMU210P,
			/// <summary></summary>
			DMU210P2,
			/// <summary></summary>
			FNC208,
			/// <summary></summary>
			FNC74,
			/// <summary></summary>
			KENSYO,
			///// <summary></summary>
			//KENSYO_D,
			///// <summary></summary>
			//KENSYO_M,
			///// <summary></summary>
			//KENSYO_Y,
			/// <summary></summary>
			LineaM,
			/// <summary></summary>
			MCC2013,
			/// <summary></summary>
			MCC3016VG,
			///// <summary></summary>
			//MCD1513,
			/// <summary></summary>
			MHG_1500,
			///// <summary></summary>
			//SNC106,
			/// <summary></summary>
			V77,
			/// <summary></summary>
			YBM1218V,
			/// <summary></summary>
			YMC430,
		}
		/// <summary>加工機名より加工機ＩＤを取得する</summary>
		static public MachID GetmachID(string machname) {
			switch (machname) {
			//case "BTU-14_4AX": return machID.BTU_14_4AX;
			case "D500": return MachID.D500;
			case "DMU200P": return MachID.DMU200P;
			case "DMU210P": return MachID.DMU210P;
			case "DMU210P2": return MachID.DMU210P2;
			case "FNC208": return MachID.FNC208;
			case "FNC74": return MachID.FNC74;
			case "KENSYO": return MachID.KENSYO;
			//case "KENSYO_D": return machID.KENSYO_D;
			//case "KENSYO_M": return machID.KENSYO_M;
			//case "KENSYO_Y": return machID.KENSYO_Y;
			case "LineaM": return MachID.LineaM;
			case "MCC2013": return MachID.MCC2013;
			case "MCC3016VG": return MachID.MCC3016VG;
			//case "MCD1513": return machID.MCD1513;
			case "MHG-1500": return MachID.MHG_1500;
			//case "SNC106": return machID.SNC106;
			case "V77": return MachID.V77;
			case "YBM1218V": return MachID.YBM1218V;
			case "YMC430": return MachID.YMC430;
			default: return MachID.NULL;
			}
		}

		/// <summary>
		/// ５面加工（工具軸指定）に使用する加工機はtrue
		/// </summary>
		/// <param name="machID"></param>
		/// <returns></returns>
		static public bool Milling_5Faces_st(Machine.MachID machID) {
			switch (machID) {
			case MachID.DMU200P:
			case MachID.DMU210P:
			case MachID.DMU210P2:
				return true;
			default:
				return false;
			}
		}

		/// <summary>ＮＣデータ出力名の拡張子</summary>
		static public string Suffix(Machine.MachID machID) {
			switch (machID) {
			case Machine.MachID.DMU200P:
			case Machine.MachID.DMU210P:
			case Machine.MachID.DMU210P2:
				return ".I";
			case Machine.MachID.D500:
			case Machine.MachID.LineaM:
				return ".ncd";
			case Machine.MachID.MCC2013:
			case Machine.MachID.MCC3016VG:
			default:
				return "";
			}
		}

		/// <summary>加工機よりベースＮＣフォーマットを出力する</summary>
		static public BaseNcForm BNcForm(Machine.MachID machID) {
			switch (machID) {
			case Machine.MachID.D500:
			case Machine.MachID.LineaM:
				return BaseNcForm.BUHIN;
			default:
				return BaseNcForm.GENERAL;
			}
		}

		/// <summary>
		/// 軸加工機の軸タイプ
		/// </summary>
		public enum Machine_Axis_Type
		{
			/// <summary>部品加工の軸タイプ</summary>
			AXIS5_BUHIN,
			/// <summary>ＤＭＵのハイデンハイン仕様の軸タイプ</summary>
			AXIS5_DMU,
			/// <summary></summary>
			AXIS5_VG,
			/// <summary>３軸仕様の加工機</summary>
			AXIS3
		}
		/// <summary>
		/// 加工機ＩＤより５軸加工機のタイプを出力する
		/// </summary>
		/// <param name="machID"></param>
		/// <returns></returns>
		static public Machine_Axis_Type Axis_Type_st(Machine.MachID machID) {
			switch (machID) {
			case MachID.LineaM:
			case MachID.D500:
				return Machine_Axis_Type.AXIS5_BUHIN;
			case MachID.DMU200P:
			case MachID.DMU210P:
			case MachID.DMU210P2:
				return Machine_Axis_Type.AXIS5_DMU;
			case MachID.MCC3016VG:
				return Machine_Axis_Type.AXIS5_VG;
			default:
				return Machine_Axis_Type.AXIS3;
			}
		}

		/// <summary>G73の戻り量（NcCode/NcMachineとの整合要） add in 2012/06/26</summary>
		public const double para6210 = 1.0;
		/// <summary>G83のクリアランス量（NcCode/NcMachineとの整合要） add in 2012/06/26</summary>
		public const double para6211 = 1.5;

		/// <summary>登録された加工機の名称</summary>
		public static string[] MachList { get { return machn.ToArray(); } }
		private static readonly List<string> machn;

		/// <summary>
		/// 工具計測方式
		/// </summary>
		public enum ToolMeasureType
		{
			/// <summary></summary>
			接触式,
			/// <summary></summary>
			レーザー,
			/// <summary></summary>
			画像式
		}

		/// <summary>ＤＮＣ装置の名前</summary>
		public readonly struct DNCName
		{
			/// <summary></summary>
			public const string TEXAS = "TEXAS";
			/// <summary></summary>
			public const string AOI = "AOI";
			/// <summary></summary>
			public const string CIMX = "CIMX";
			/// <summary>LineaM用</summary>
			public const string DM10 = "DM10";
			/// <summary>MHG-1500用</summary>
			public const string HP = "HP";
		}

		/// <summary>コンストラクタ</summary>
		static Machine() {

			using (DataTable Mach = new DataTable("Mach")) {
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_Mach ORDER BY [group]", connection)) {
					connection.Open();
					adapter.Fill(Mach);
				}

				machn = new List<string>();
				foreach (DataRow dRow in Mach.Rows) {
					if (dRow["未使用"] != DBNull.Value)
						if ((bool)dRow["未使用"] == true) continue;
					if (GetmachID((string)dRow["設備名"]) == MachID.NULL) {
						System.Windows.Forms.MessageBox.Show("データベース内の設備" + (string)dRow["設備名"] + " はＮＣデータ出力設備として未設定です。");
						continue;
					}
					machn.Add((string)dRow["設備名"]);
				}
			}
			return;
		}

		// 演算子のオーバーロード

		/*
		/// <summary>==演算子</summary>
		public static bool operator ==(Machine c1, Machine c2) { return c1.m_ID == c2.m_ID; }
		/// <summary>!=演算子</summary>
		public static bool operator !=(Machine c1, Machine c2) { return c1.m_ID != c2.m_ID; }
		*/
	}
}
