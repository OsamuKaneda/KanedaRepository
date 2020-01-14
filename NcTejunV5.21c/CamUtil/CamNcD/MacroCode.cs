using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;

namespace CamUtil.CamNcD
{
	/// <summary>
	/// 固定サイクル、カスタムマクロのＮＣコード情報を提供します。
	/// </summary>
	public readonly struct MacroCode
	{
		/// <summary>加工と移動・ミラーを定義</summary>
		public enum KAKO
		{
			/// <summary>加工ありで、移動・ミラーの対象であるが未検証</summary>
			K_CHK,
			/// <summary>加工ありで、移動・ミラーの対象（xlist，zlistが対象）</summary>
			K_TRU,
			/// <summary>測定などで、動作を追跡しない（移動・ミラーの定義なし）</summary>
			K_FLS,
			/// <summary>動作なし（移動・ミラーの変化なし）</summary>
			K_NON,
		}
		/// <summary>'W'を用いてミラー時のアップカット、ダウンカットを制御するマクロのmacrowの位置。無い場合は -1</summary>
		static public int MacroIndex(string mCode) {
			for (int ii = 0; ii < macrow.Length; ii++) if (mCode == macrow[ii].ToString("0000")) return ii;
			return -1;
		}
		/// <summary>'W'を用いてミラー時のアップカット、ダウンカットを制御するマクロのリスト</summary>
		static private int[] macrow = new int[] { 8025, 8080, 8082, 8090, 8100, 8110, 8120, 8401, 8402 };
		//static private int[] macrow = new int[] { 8020, 8025, 8030, 8080, 8082, 8090, 8100, 8110, 8120, 8401, 8402 };

		/// <summary>固定サイクルやカクタムマクロ以外の一般部ＮＣデータ</summary>
		static public MacroCode NORM_NC = new MacroCode(0);
		/// <summary>マクロコードのテーブル</summary>
		static private DataTable mCode = null;





		/// <summary>加工移動の区分</summary>
		public readonly KAKO kako;
		/// <summary>加工移動し、移動・ミラーの対象とするマクロ</summary>
		public bool Ido { get { return (kako == KAKO.K_TRU || kako == KAKO.K_CHK); } }

		/// <summary>引数のリスト</summary>
		public readonly string hlist;
		/// <summary>省略可能リスト</summary>
		public readonly string olist;
		/// <summary>XY軸に関連するコード。XY軸のミラー移動時に使用</summary>
		public readonly string xlist;
		/// <summary>Ｚ軸に関連するコード。Ｚ軸の移動時に使用</summary>
		public readonly string zlist;
		/// <summary>マクロの説明</summary>
		public readonly string mess;

		/// <summary>一般ＮＣデータ用コンストラクタ</summary>
		private MacroCode(int dummy) {
			kako = KAKO.K_TRU;
			hlist = StringCAM.ABC0;
		mess = "一般ＮＣデータ";
			olist = StringCAM.ABC0;
			xlist = "XYIJ";
			zlist = "Z";
		}
		/// <summary>固定サイクル/カスタムマクロ用コンストラクタ</summary>
		public MacroCode(BaseNcForm bnForm, int progNo) {

			// 固定サイクル
			if (progNo < 90) {
				switch (progNo) {
				case 73: kako = KAKO.K_CHK; hlist = "XYZQRF"; mess = "G73 固定サイクル"; break;
				case 81: kako = KAKO.K_CHK; hlist = "XYZRF"; mess = "G81 固定サイクル"; break;
				case 82: kako = KAKO.K_CHK; hlist = "XYZPRF"; mess = "G82 固定サイクル"; break;
				case 83: kako = KAKO.K_CHK; hlist = "XYZQRF"; mess = "G83 固定サイクル"; break;
				case 84: kako = KAKO.K_CHK; hlist = "XYZRF"; mess = "G84 固定サイクル"; break;
				case 86: kako = KAKO.K_CHK; hlist = "XYZRF"; mess = "G86 固定サイクル"; break;
				default: throw new Exception("固定サイクル番号" + progNo.ToString() + " は使用できません。４");
				}
				olist = "ZQRF";
				xlist = "XY";
				zlist = "ZR";
				return;
			}

			if (MacroCode.mCode == null) {
				MacroCode.mCode = new DataTable("MacroCode");
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.macro_code", connection)) {
					connection.Open();
					adapter.Fill(MacroCode.mCode);
				}
			}
			foreach (DataRow dRow in MacroCode.mCode.Rows) {
				//if (bnForm.Id != ((string)dRow["BaseForm"] == "BU" ? BaseNcForm.ID._5AXIS : BaseNcForm.ID.GENERAL)) continue;
				if (bnForm.Code != (string)dRow["BaseForm"]) continue;
				if (progNo != Convert.ToInt32((string)dRow["ProgramNo"])) continue;

				switch ((string)dRow["Kako"]) {
				case "CHK": kako = KAKO.K_CHK; break;	// 移動・ミラーの処理が未チェック
				case "FLS": kako = KAKO.K_FLS; break;	// 移動・ミラーの処理が定義できない
				case "TRU": kako = KAKO.K_TRU; break;	// 移動・ミラーの処理が定義されている
				case "NON": kako = KAKO.K_NON; break;	// 移動動作なし
				case "ERR": throw new Exception("P" + progNo.ToString() + " は使用できないマクロです。４");
				default: throw new Exception("Program ERROR");
				}
				hlist = (string)dRow["CodeList"];
				olist = (string)dRow["Optional"];
				xlist = (string)dRow["XAxis"];
				zlist = (string)dRow["ZAxis"];
				mess = (string)dRow["Message"];
				return;
			}
			throw new Exception("P" + progNo.ToString() + " は使用できないマクロです。４");

		}
	}
}
