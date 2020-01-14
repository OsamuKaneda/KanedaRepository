using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace NcTejun
{
	/// <summary>
	/// 工程管理情報
	/// </summary>
	class CasaData : IDisposable
	{
		/// <summary>ＳＱＬサーバより読込んだ工程管理のデータセット</summary>
		static public DataSet Casa;

		static private string connectionString = "Data Source=nt0040np;Initial Catalog=InputCasa;User ID=TEXAS;Password=dnc";

		/// <summary>
		/// 工程管理ＤＢにて、使用可能な内製管理番号の一覧を取得する。表示するデータは[内製管理番号]、[製造番号]、[子注番]、[型管理番号]、[金型名]。
		/// </summary>
		static public void DataSet() {
			Casa = new DataSet("Casa");

			using (SqlConnection connection = new SqlConnection(CasaData.connectionString))
			using (SqlCommand com = new SqlCommand() { Connection = connection }) {
				connection.Open();
				com.CommandText = "dbo.tjn_JNoList";
				com.CommandType = CommandType.StoredProcedure;
				using (SqlDataAdapter adapter = new SqlDataAdapter(com)) { adapter.Fill(Casa, "DBJNO"); }
			}
		}





		/// <summary>加工実績としての使用可否（部品、工程なし）</summary>
		public bool KATA_CODE_ARI { get { return KATA_CODE == null ? false : true; } }
		/// <summary>製造番号</summary>
		public string SEIBAN;
		/// <summary>型管理番号</summary>
		public string KATA_CODE;
		/// <summary>金型名</summary>
		public string KATA_NAME;

		public DataTable DBORD;
		public DataTable DBRTG;

		/// <summary>内製管理番号</summary>
		private readonly string casJNo;
		/// <summary>工程管理システムの加工機名</summary>
		private readonly string casMach;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="j_NO">内製管理番号</param>
		/// <param name="casName">工程管理システムの加工機名</param>
		public CasaData(string j_NO, string casName) {
			casJNo = j_NO;
			casMach = casName;
			// 金型情報の設定
			SEIBAN = null;
			KATA_CODE = null;
			KATA_NAME = null;
			foreach (DataRow dRow in Casa.Tables["DBJNO"].Rows)
				if (j_NO == (string)dRow["J_NO"]) {
					SEIBAN = dRow.Field<string>("SEIBAN");
					KATA_CODE = (string)dRow["KATA_CODE"];
					KATA_NAME = (string)dRow["KATA_NAME"];
					break;
				}
			if (KATA_CODE_ARI == false) {
				if (NcdTool.Tejun.Mach.Log_kado && CamUtil.ProgVersion.Debug == false)
					throw new Exception("製造番号（" + NcdTool.Tejun.Seba + "）はカサブランカに存在しません");
				else
					MessageBox.Show("製造番号（" + NcdTool.Tejun.Seba + "）はカサブランカに存在しません");
				return;
			}

			using (SqlConnection connection = new SqlConnection(CasaData.connectionString)) {

				connection.Open();
				// ///////////////////////////////////////////////////////////////////////////
				// 工程管理ＤＢにて、所属する作業セルの数から実績収集対象加工機か否かを調べる
				// ///////////////////////////////////////////////////////////////////////////
				using (var com = new SqlCommand() { Connection = connection }) {
					com.CommandText = "dbo.tjn_CELCOUNT";
					com.CommandType = CommandType.StoredProcedure;
					com.Parameters.Add(new SqlParameter("@MACH", casName));
					int ii = (int)com.ExecuteScalar();
					if ( ii == 0) {
						// 加工機がどのグループにも属していない場合（＝レートがなく型費に計上しない加工機。YBM430など）
						//whereString1 = whereString2 = null;
						KATA_CODE = null;   // 存在しない製造番号と同等に扱う
					}
				}
				if (KATA_CODE_ARI == false) return;

				// //////////////////////////////////////////////////////////////
				// 工程管理ＤＢにて、型製作工程が未登録の場合工程の初期化を実施
				// //////////////////////////////////////////////////////////////
				using (var com = new SqlCommand() { Connection = connection }) {
					com.CommandText = "dbo.tjn_KATAORDER_J";
					com.CommandType = CommandType.StoredProcedure;
					com.Parameters.Add(new SqlParameter("@S_JNO", j_NO));
					com.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// 工程管理ＤＢより部品コード、部品名のリストを取得する
		/// </summary>
		public void SetDBORD() {
			DBORD = new DataTable();
			if (KATA_CODE_ARI == false) return;
			using (SqlConnection connection = new SqlConnection(CasaData.connectionString))
			using (SqlCommand com = new SqlCommand() { Connection = connection }) {
				connection.Open();
				com.CommandText = "dbo.tjn_DBORD";
				com.CommandType = CommandType.StoredProcedure;
				com.Parameters.Add("@SEIB", SqlDbType.VarChar, 20).Value = casJNo;
				com.Parameters.Add("@MACH", SqlDbType.VarChar, 20).Value = casMach;
				using (SqlDataAdapter adapter = new SqlDataAdapter(com)) { adapter.Fill(DBORD); }
			}
		}

		/// <summary>
		/// 工程管理ＤＢより工程ＮＯ、工程名のリストを取得する
		/// </summary>
		/// <param name="buhin_code"></param>
		public void SetDBRTG(string buhin_code) {
			DBRTG = new DataTable();
			if (KATA_CODE_ARI == false) return;
			using (SqlConnection connection = new SqlConnection(CasaData.connectionString))
			using (SqlCommand com = new SqlCommand() { Connection = connection }) {
				connection.Open();
				com.CommandText = "dbo.tjn_DBRTG";
				com.CommandType = CommandType.StoredProcedure;
				com.Parameters.Add("@SEIB", SqlDbType.VarChar, 20).Value = casJNo;
				com.Parameters.Add("@MACH", SqlDbType.VarChar, 20).Value = casMach;
				com.Parameters.Add("@BHNC", SqlDbType.VarChar, 20).Value = buhin_code;
				using (SqlDataAdapter adapter = new SqlDataAdapter(com)) { adapter.Fill(DBRTG); }
			}
		}

		public void Dispose() {
			if (DBORD != null) { DBORD.Dispose(); DBORD = null; }
			if (DBRTG != null) { DBRTG.Dispose(); DBRTG = null; }
		}
	}
}