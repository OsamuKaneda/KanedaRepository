using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace CamUtil
{
	/// <summary>
	/// 材質による加工条件を調整します。
	/// </summary>
	static public class Material
	{
		static DataSet mat;

		static Material() {
			mat = new DataSet("Material");

			using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString))
			using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_material_name", connection)) {
				connection.Open();
				adapter.Fill(mat, "material_name");
			}
		}

		/// <summary>
		/// ＤＢの材質名よりＤＢの材質グループ名を求める（ＰＣ版）
		/// </summary>
		/// <param name="wzais">材質名</param>
		/// <returns>材質グループ名</returns>
		static public string ZgrpgetPC(string wzais) {

			foreach (DataRow dRow in mat.Tables["material_name"].Rows) {
				if ((string)dRow["material_name"] != wzais)
					continue;
				return (string)dRow["material_grp"];
			}
			throw new Exception(wzais + " は存在しない");
		}
	}
}
