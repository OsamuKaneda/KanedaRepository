using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

namespace CamUtil.CamNcD
{
	/// <summary>
	/// 穴加工の基準寸法をデータベースから取得します
	/// </summary>
	public class KijunSize
	{
		/// <summary>周り止めの穴寸法</summary>
		private static DataSet mawarSet = null;
		/// <summary>ヘリカル加工で使用するネジ穴寸法</summary>
		private static DataSet helicSet = null;
		/// <summary>部位ごとの加工許容誤差</summary>
		private static DataTable tolerTbl = null;

		/// <summary>
		/// データベースより周り止めの形状情報を取得します。
		/// </summary>
		/// <param name="progNo">プログラム番号</param>
		/// <param name="diam">ピン径区分</param>
		public static DataRow Mawaridome(string progNo, double diam) {
			bool tblExist = false;
			if (mawarSet == null) mawarSet = new DataSet("MAWARIDOME");
			foreach (DataTable tbl in mawarSet.Tables)
				if (tbl.TableName == progNo) { tblExist = true; break; }

			if (tblExist == false) {
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlCommand com = new SqlCommand() { Connection = connection }) {
					connection.Open();
					com.CommandText = "SELECT * FROM dbo.kijun_MAWARIDOME WHERE [プログラム番号] = @progNo ORDER BY [ピン径区分(以下)]";
					com.Parameters.Add("@progNo", SqlDbType.NChar, 4).Value = progNo;
					using (SqlDataAdapter adapter = new SqlDataAdapter(com)) { adapter.Fill(mawarSet, progNo); }
				}
			}
			foreach (DataRow dRow in mawarSet.Tables[progNo].Rows) {
				if ((double)dRow["ピン径区分(以下)"] < diam - 0.001) continue;
				return dRow;
			}
			throw new Exception("qwjhefbdqwheb");
		}

		/// <summary>
		/// データベースよりヘリカル加工の形状情報を取得します
		/// </summary>
		/// <param name="progNo">プログラム番号</param>
		/// <param name="diam">ネジの呼び径</param>
		public static DataRow HelicalTap(string progNo, double diam) {
			bool tblExist = false;
			if (helicSet == null) helicSet = new DataSet("HELICALTAP");
			foreach (DataTable tbl in helicSet.Tables)
				if (tbl.TableName == progNo) { tblExist = true; break; }

			if (tblExist == false) {
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlCommand com = new SqlCommand() { Connection = connection }) {
					connection.Open();
					com.CommandText = "SELECT * FROM dbo.kijun_HELICALTAP WHERE [プログラム番号] = @progNo ORDER BY [呼び寸法]";
					com.Parameters.Add("@progNo", SqlDbType.NChar, 4).Value = progNo;
					using (SqlDataAdapter adapter = new SqlDataAdapter(com)) { adapter.Fill(helicSet, progNo); }
				}
			}
			foreach (DataRow dRow in helicSet.Tables[progNo].Rows) {
				if (Math.Abs((double)dRow["呼び寸法"] - diam) > 0.001) continue;
				return dRow;
			}
			throw new Exception("qwjhefbdqwheb");
		}

		/// <summary>
		/// データベースより加工許容誤差の情報を取得します。
		/// </summary>
		/// <param name="No"></param>
		/// <returns></returns>
		public static DataRow Tolerance(int No) {
			if (tolerTbl == null) {
				tolerTbl = new DataTable();
				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
				using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.tolerance", connection)) {
					connection.Open();
					adapter.Fill(tolerTbl);
				}
			}
			foreach (DataRow dRow in tolerTbl.Rows) {
				if ((int)dRow["計測部位識別番号"] != No) continue;
				return dRow;
			}
			throw new Exception("qwjhefbdqwheb");
		}
	}
}
