using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Data.SqlClient;

namespace NcTejun.Output
{
	partial class ToolSheetDB
	{
		const string connectionString = "Data Source=nt0040np;Initial Catalog=TexasDB;User ID=cadceus;Password=3933";

		/// <summary>
		/// TOOL_SHEET_HEADに追加する
		/// </summary>
		/// <param name="tsheet"></param>
		/// <param name="lineCount">工具表に追加する行数</param>
		/// <returns></returns>
		static private DataRow TS_Head(TejunSet.ToolSheet tsheet, int lineCount) {
			DataRow dRow;
			string year_2 = DateTime.Now.Year.ToString().Substring(2);

			using (SqlConnection connection = new SqlConnection(ToolSheetDB.connectionString)) {
				connection.Open();
				using (SqlCommand command = new SqlCommand("dbo.tg_TOOLHEADui", connection)) {
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add(new SqlParameter("@TSHEET_NAME", tsheet.TolstName));
					command.Parameters.Add(new SqlParameter("@YEAR_2", year_2));
					command.Parameters.Add(new SqlParameter("@PSHEET_NAME", tsheet.TolstData.KtejunName));
					command.Parameters.Add(new SqlParameter("@J_NO", tsheet.TolstData.KtejunSeba));
					if (FormNcSet.casaData == null || FormNcSet.casaData.parts_no == "") {
						command.Parameters.Add(new SqlParameter("@KATA_CODE", "XXXX"));
						command.Parameters.Add(new SqlParameter("@BUHIN_CODE", "001"));
						command.Parameters.Add(new SqlParameter("@KOUTEI_NO", "001"));
						command.Parameters.Add(new SqlParameter("@KATA_NAME", "DUMMY"));
						command.Parameters.Add(new SqlParameter("@BUHIN_NAME", "DUMMY"));
						command.Parameters.Add(new SqlParameter("@KOUTEI_COMM", "切削"));
					}
					else {
						command.Parameters.Add(new SqlParameter("@KATA_CODE", FormNcSet.casaData.parts_no));
						command.Parameters.Add(new SqlParameter("@BUHIN_CODE", FormNcSet.casaData.process_no));
						command.Parameters.Add(new SqlParameter("@KOUTEI_NO", FormNcSet.casaData.step_no));
						command.Parameters.Add(new SqlParameter("@KATA_NAME", FormNcSet.casaData.kataName));
						command.Parameters.Add(new SqlParameter("@BUHIN_NAME", FormNcSet.casaData.buhinName));
						command.Parameters.Add(new SqlParameter("@KOUTEI_COMM", FormNcSet.casaData.koutei_comm));
					}
					double dtmp = 0.0;
					command.Parameters.Add(new SqlParameter("@THICKNESS", dtmp));
					command.Parameters.Add(new SqlParameter("@MATERIAL_NAME", NcdTool.Tejun.NcList.Mzais));
					command.Parameters.Add(new SqlParameter("@MACHINE_NAME", NcdTool.Tejun.Mach.name));
					command.Parameters.Add(new SqlParameter("@SHAIN_NAME", NcdTool.Tejun.Uid.ToString()));
					command.Parameters.Add(new SqlParameter("@KAKO_START", NcdTool.Tejun.Kdate.ToShortDateString()));
					command.Parameters.Add(new SqlParameter("@LINE_MAX", lineCount));
					command.ExecuteNonQuery();
				}
				// 設定情報 dRow の作成
				using (var command = new SqlCommand() { Connection = connection })
				using (DataTable TOOL_SHEET_HEAD = new DataTable()) {
					command.CommandText = "SELECT * FROM dbo.TOOL_SHEET_HEAD WHERE J_NO = @SEBA AND TSHEET_NAME = @NAME";
					command.Parameters.Add("@SEBA", SqlDbType.VarChar, 20).Value = tsheet.TolstData.KtejunSeba;
					command.Parameters.Add("@NAME", SqlDbType.VarChar, 16).Value = tsheet.TolstName;
					using (SqlDataAdapter adapter = new SqlDataAdapter(command)) { adapter.Fill(TOOL_SHEET_HEAD); }
					dRow = TOOL_SHEET_HEAD.Rows[0];
				}
			}
			return dRow;
		}

		/// <summary>
		/// TOOL_SHEETに追加する
		/// </summary>
		/// <param name="dRow_HEAD"></param>
		/// <param name="tsList"></param>
		/// <returns></returns>
		static private void TS_Tool(DataRow dRow_HEAD, List<Tsdata> tsList) {

			using (SqlConnection connection = new SqlConnection(ToolSheetDB.connectionString))
			using (SqlDataAdapter adapter = new SqlDataAdapter()) {

				// //////////////////////////////
				// 工具表のアダプター作成
				// //////////////////////////////

				// ＳＥＬＥＣＴコマンド
				adapter.SelectCommand = new SqlCommand($"SELECT * FROM dbo.TOOL_SHEET WHERE TSHEET_ID = {(int)dRow_HEAD["TSHEET_ID"]:d}", connection);

				// ＩＮＳＥＲＴコマンド
				adapter.InsertCommand = new SqlCommand(
					"INSERT TOOL_SHEET " +
					"( [TSHEET_ID]" +
					" ,[J_NO] ,[TSHEET_NAME]" +
					" ,[line_no] ,[magazine_no] ,[tool_no] ,[toolset_ID]" +
					" ,[cutter_name] ,[holder_name] ,[length] ,[total_length] ,[length_min]" +
					" ,[cutter_diameter] ,[cutter_type_name] ,[holder_type_name] ,[stock_kubun] ,[depth]" +
					" ,[cutting_time] ,[comment] ,[nc_number] ,[nc_name] ,[permanent_tool]" +
					" ,[hosei_D] ,[spin_speed] ,[feedrate] ,[tooth_length] ,[consume_rate])" +
					"VALUES ( " +
					" @b01" +
					",@bb1 ,@bb2" +
					",@b02 ,@b03 ,@b04 ,@b05 " +
					",@b06 ,@b07 ,@b08 ,@b09 ,@b10 " +
					",@b11 ,@b12 ,@b13 ,@b14 ,@b15 " +
					",@b16 ,@b17 ,@b18 ,@b19 ,@b20 " +
					",@b21 ,@b22 ,@b23 ,@b24 ,@b25 )", connection);

				adapter.InsertCommand.Parameters.Add("@b01", SqlDbType.Int, 0, "TSHEET_ID");
				adapter.InsertCommand.Parameters.Add("@bb1", SqlDbType.VarChar, 20, "J_NO");
				adapter.InsertCommand.Parameters.Add("@bb2", SqlDbType.VarChar, 16, "TSHEET_NAME");

				adapter.InsertCommand.Parameters.Add("@b02", SqlDbType.SmallInt, 0, "line_no");
				adapter.InsertCommand.Parameters.Add("@b03", SqlDbType.SmallInt, 0, "magazine_no");
				adapter.InsertCommand.Parameters.Add("@b04", SqlDbType.SmallInt, 0, "tool_no");
				adapter.InsertCommand.Parameters.Add("@b05", SqlDbType.Char, 4, "toolset_ID");
				adapter.InsertCommand.Parameters.Add("@b06", SqlDbType.VarChar, 15, "cutter_name");
				adapter.InsertCommand.Parameters.Add("@b07", SqlDbType.VarChar, 15, "holder_name");
				adapter.InsertCommand.Parameters.Add("@b08", SqlDbType.Float, 0, "length");
				adapter.InsertCommand.Parameters.Add("@b09", SqlDbType.Float, 0, "total_length");
				adapter.InsertCommand.Parameters.Add("@b10", SqlDbType.Float, 0, "length_min");
				adapter.InsertCommand.Parameters.Add("@b11", SqlDbType.Float, 0, "cutter_diameter");
				adapter.InsertCommand.Parameters.Add("@b12", SqlDbType.NVarChar, 7, "cutter_type_name");
				adapter.InsertCommand.Parameters.Add("@b13", SqlDbType.NVarChar, 7, "holder_type_name");
				adapter.InsertCommand.Parameters.Add("@b14", SqlDbType.VarChar, 6, "stock_kubun");
				adapter.InsertCommand.Parameters.Add("@b15", SqlDbType.Float, 0, "depth");
				adapter.InsertCommand.Parameters.Add("@b16", SqlDbType.Int, 0, "cutting_time");
				adapter.InsertCommand.Parameters.Add("@b17", SqlDbType.NVarChar, 100, "comment");
				adapter.InsertCommand.Parameters.Add("@b18", SqlDbType.SmallInt, 0, "nc_number");
				adapter.InsertCommand.Parameters.Add("@b19", SqlDbType.VarChar, 100, "nc_name");
				adapter.InsertCommand.Parameters.Add("@b20", SqlDbType.Bit, 0, "permanent_tool");
				adapter.InsertCommand.Parameters.Add("@b21", SqlDbType.SmallInt, 0, "hosei_D");
				adapter.InsertCommand.Parameters.Add("@b22", SqlDbType.Int, 0, "spin_speed");
				adapter.InsertCommand.Parameters.Add("@b23", SqlDbType.Int, 0, "feedrate");
				adapter.InsertCommand.Parameters.Add("@b24", SqlDbType.Float, 0, "tooth_length");
				adapter.InsertCommand.Parameters.Add("@b25", SqlDbType.Float, 0, "consume_rate");
				//parameter_i.SourceVersion = DataRowVersion.Original;

				// ＤＥＬＥＴＥコマンド
				/*	delete 2018/07/16
				adapter.DeleteCommand = new SqlCommand("DELETE TOOL_SHEET WHERE [TSHEET_ID] = @b50", connection);
				adapter.DeleteCommand.Parameters.Add("@b50", SqlDbType.Int, 0, "TSHEET_ID");
				*/

				// //////////////////////////////
				// 追加する工具表ＤＢをdTableに作成
				// //////////////////////////////
				DataTable dTable = new DataTable();
				adapter.Fill(dTable);
				if (dTable.Rows.Count != 0) throw new Exception("qwjefbqhwebf");
				short lno = 0;
				DataRow dRow;
				foreach (Tsdata tsd in tsList) {
					dRow = dTable.NewRow();
					lno++;

					dRow["TSHEET_ID"] = (int)dRow_HEAD["TSHEET_ID"];
					// ADD in 2010/10/22
					dRow["TSHEET_NAME"] = (string)dRow_HEAD["TSHEET_NAME"];
					dRow["J_NO"] = (string)dRow_HEAD["J_NO"];

					dRow["line_no"] = lno;
					dRow["magazine_no"] = (short)tsd.snum;
					dRow["tool_no"] = (short)tsd.tnum;
					dRow["toolset_ID"] = tsd.toolset_ID;
					dRow["cutter_name"] = tsd.Tset.ToolName;
					dRow["holder_name"] = tsd.Tset.HolderName;
					dRow["length"] = (float)tsd.Tset.ToutLength;            //SqlDbType.Float
					dRow["total_length"] = tsd.Tset.TsetLength; //SqlDbType.Float
					dRow["length_min"] = tsd.ttsk;
					dRow["cutter_diameter"] = tsd.Tset.Diam;    //SqlDbType.Float
					dRow["cutter_type_name"] = tsd.Tset.CutterTypeName;
					dRow["holder_type_name"] = tsd.Tset.HolderTypeName;
					if (tsd.hld_knri != null) dRow["stock_kubun"] = tsd.hld_knri;
					if (tsd.zchi.HasValue) dRow["depth"] = tsd.zchi.Value;
					dRow["cutting_time"] = tsd.Ntim;        //SqlDbType.Int
					dRow["comment"] = "";
					dRow["nc_number"] = tsd.nnum;           //SqlDbType.SmallInt
					dRow["nc_name"] = tsd.Nnam(5);
					//dRow["permanent_tool"] = Program.tejun.mach.perm_tool(tsd.tnum, tsd.tset.tset_name);
					dRow["permanent_tool"] = tsd.perm_tool;
					// ADDD in 2012/11/14
					dRow["hosei_D"] = tsd.Dnum;
					if (tsd.schi.HasValue) dRow["spin_speed"] = tsd.schi.Value;
					if (tsd.fchi.HasValue) dRow["feedrate"] = tsd.fchi.Value;
					dRow["tooth_length"] = tsd.Tset.Hacho;
					dRow["consume_rate"] = tsd.Consume_rate;
					dTable.Rows.Add(dRow);
				}

				// //////////////////////////////
				// ＳＱＬデータベースに追加する
				// //////////////////////////////
				adapter.Update(dTable);
			}
		}

		/// <summary>
		/// 製造番号と工具表名の組み合わせがTOOL_SHEET_HEADに存在する場合はＩＤ番号を、存在しない場合は０返す。
		/// </summary>
		/// <param name="seiban">製造番号</param>
		/// <param name="tsname">工具表名</param>
		/// <returns></returns>
		static public int TSHEET_HEAD_ID(string seiban, string tsname) {
			return TSHEET_HEAD_ID(seiban, tsname, out string tjnN);
		}
		/// <summary>
		/// 製造番号と工具表名の組み合わせがTOOL_SHEET_HEADに存在する場合はＩＤ番号を、存在しない場合は０返す。
		/// </summary>
		/// <param name="seiban">製造番号</param>
		/// <param name="tsname">工具表名</param>
		/// <param name="tjnN">作成された手順名</param>
		/// <returns></returns>
		static public int TSHEET_HEAD_ID(string seiban, string tsname, out string tjnN) {
			using (SqlConnection connection = new SqlConnection(ToolSheetDB.connectionString))
			using (var com = new SqlCommand() { Connection = connection })
			using (DataTable TOOL_SHEET_HEAD = new DataTable()) {
				connection.Open();
				com.CommandText = "SELECT TSHEET_ID, PSHEET_NAME FROM dbo.TOOL_SHEET_HEAD WHERE J_NO = @SEI AND TSHEET_NAME = @TSN";
				com.Parameters.Add("@SEI", SqlDbType.VarChar, 20).Value = seiban;
				com.Parameters.Add("@TSN", SqlDbType.VarChar, 16).Value = tsname;
				using (SqlDataAdapter adapter = new SqlDataAdapter(com)) { adapter.Fill(TOOL_SHEET_HEAD); }
				tjnN = TOOL_SHEET_HEAD.Rows.Count > 0 ? (string)TOOL_SHEET_HEAD.Rows[0]["PSHEET_NAME"] : null;
				return TOOL_SHEET_HEAD.Rows.Count > 0 ? (int)TOOL_SHEET_HEAD.Rows[0]["TSHEET_ID"] : 0;
			}
		}
	}
}