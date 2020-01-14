using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace NcdTool
{
	/// <summary>
	/// データベース内の工具の情報[不変]
	/// </summary>
	class ToolSetInfo
	{
		// //////////////////////////////////////////////
		// 静的変数（加工機決定後にDataSetによりセットする）
		// //////////////////////////////////////////////

		/// <summary>ＳＱＬサーバより読込んだデータセット</summary>
		static private DataSet tSetInfo;



		/// <summary>工具番号の開始値の初期設定値</summary>
		static public int TnoStart { get { return Tejun.Mach.Dmu ? 200 : 0; } }

		// 静的ＥＮＤ
		// /////////////////////////////////////////////////

		// ////////////////////////////////////////////////////////////////////
		// 静的メソッド
		// ////////////////////////////////////////////////////////////////////
		static public void DataSet() {
			tSetInfo = new DataSet("TSetInfo");

			//using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString)) {
			//	connection.Open();
			//	string queryString = "SELECT * FROM dbo.NcConv_ToolSet";
			//	SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
			//	adapter.Fill(TSetInfo, "NcConv_ToolSet");
			//}

			//using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString)) {
			//	connection.Open();
			//	string queryString = "SELECT * FROM dbo.NcConv_TSet_CAM";
			//	SqlDataAdapter adapter = new SqlDataAdapter(queryString, connection);
			//	adapter.Fill(TSetInfo, "NcConv_TSet_CAM");
			//}
			using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString)) {
				connection.Open();
				using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_TSet_CHG", connection)) {
					adapter.Fill(tSetInfo, "NcConv_TSet_CHG");
				}
				using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_cond_material", connection)) {
					adapter.Fill(tSetInfo, "NcConv_cond_material");
				}
			}

			SelToolset.Set_SelToolset();
			GetTaper.Set_GetTaper();
			GetSpinType.Set_GetSpinType();
		}

		/// <summary>
		/// ツールセットとホルダー管理区分を求める 2009/07/10
		/// </summary>
		private readonly struct SelToolset
		{
			static public SqlConnection connection;
			static public SqlDataAdapter adapter;
			static public SqlParameter param_tsetCam;
			static public SqlParameter param_machine;
			static public void Set_SelToolset() {
				connection = new SqlConnection(CamUtil.ServerPC.connectionString);
				adapter = new SqlDataAdapter("dbo.SelToolset", connection);
				adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
				adapter.SelectCommand.Parameters.Add("@tsetCam", SqlDbType.VarChar, 30);
				adapter.SelectCommand.Parameters.Add("@machine", SqlDbType.VarChar, 10);
				param_tsetCam = adapter.SelectCommand.Parameters[0];
				param_machine = adapter.SelectCommand.Parameters[1];
				connection.Close();
			}
		}

		/// <summary>
		/// ホルダー管理区分を求める 2009/09/01
		/// </summary>
		private readonly struct GetTaper
		{
			static public SqlConnection connection;
			static public SqlDataAdapter adapter;
			static public SqlParameter param_toolset;
			static public SqlParameter param_machine;
			static public void Set_GetTaper() {
				connection = new SqlConnection(CamUtil.ServerPC.connectionString);
				adapter = new SqlDataAdapter("dbo.GetTaper", connection);
				adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
				adapter.SelectCommand.Parameters.Add("@toolset", SqlDbType.VarChar, 30);
				adapter.SelectCommand.Parameters.Add("@machine", SqlDbType.VarChar, 10);
				param_toolset = adapter.SelectCommand.Parameters[0];
				param_machine = adapter.SelectCommand.Parameters[1];
				connection.Close();
			}
		}

		/// <summary>
		///スピンドル区分（標準、高速）を求める 2016/03/01
		/// </summary>
		private readonly struct GetSpinType
		{
			static public SqlConnection connection;
			static public SqlDataAdapter adapter;
			static public SqlParameter param_hldname;
			static public SqlParameter param_machine;
			static public void Set_GetSpinType() {
				connection = new SqlConnection(CamUtil.ServerPC.connectionString);
				adapter = new SqlDataAdapter("dbo.GetSpinType", connection);
				adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
				adapter.SelectCommand.Parameters.Add("@hldname", SqlDbType.VarChar, 15);
				adapter.SelectCommand.Parameters.Add("@machine", SqlDbType.VarChar, 10);
				param_hldname = adapter.SelectCommand.Parameters[0];
				param_machine = adapter.SelectCommand.Parameters[1];
				connection.Close();
			}
		}

		/// <summary>
		/// ツールセットＣＡＭ名と加工機名からツールセット名を決定する
		/// </summary>
		/// <param name="tsetCAM">ツールセットＣＡＭ名</param>
		/// <param name="mchName">加工機名</param>
		/// <returns>ツールセット名</returns>
		static public string TSet_Name(string tsetCAM, string mchName) {
			string tsetname = TSet_Name(tsetCAM, mchName, out string hld_typ, out string hld_knr, out string spn_typ);
			if (tsetname == null)
				throw new Exception($"加工機{mchName}において、ツールセットＣＡＭ{tsetCAM}に対応する工具、ホルダーの組み合わせは存在しません。");
			return tsetname;
		}
		/// <summary>
		/// ツールセットＣＡＭ名と加工機名からツールセット名を決定する
		/// </summary>
		/// <param name="tsetCAM">ツールセットＣＡＭ名</param>
		/// <param name="mchName">加工機名</param>
		/// <param name="hld_typ">ホルダータイプ</param>
		/// <param name="hld_knr">ホルダー管理区域名</param>
		/// <param name="spn_typ">スピンドル仕様</param>
		/// <returns>ツールセット名。組み合わせが存在しない場合はnullを返す</returns>
		static public string TSet_Name(string tsetCAM, string mchName, out string hld_typ, out string hld_knr, out string spn_typ) {
			string tsetname = null;

			// ///////////////////////////////////////////////////////////////
			// ストアドプロシージャによりツールセットとホルダー管理区分を求める
			// ///////////////////////////////////////////////////////////////
			using (DataTable selToolset = new DataTable()) {
				SelToolset.param_tsetCam.Value = tsetCAM;
				SelToolset.param_machine.Value = mchName;
				SelToolset.adapter.Fill(selToolset);

				if (selToolset.Rows.Count == 1) {
					tsetname = (string)selToolset.Rows[0]["ツールセット名"];
					hld_typ = (string)selToolset.Rows[0]["テーパ規格名"];
					hld_knr = (string)selToolset.Rows[0]["ホルダー管理区分"];
					spn_typ = (string)selToolset.Rows[0]["スピンドル仕様"];
				}
				else {
					hld_typ = null;
					hld_knr = null;
					spn_typ = null;
				}
			}
			return tsetname;
		}

		/// <summary>
		/// ツールセット名と加工機名からホルダー管理名を取得する（ツールセットＣＡＭ名が不明の場合）
		/// これは未使用。ホルダー管理をしていない現在ではホルダー管理名をこのholder_kanriテーブルから取得すべきでない。
		/// </summary>
		/// <param name="tsetName">ツールセット名</param>
		/// <param name="mchName">加工機名</param>
		/// <returns>ホルダー管理名</returns>
		static public string Get_HolderKanri(string tsetName, string mchName) {
			string taper = null, kanri = null;

			// /////////////////////////////////////
			// ストアドプロシージャにより
			// /////////////////////////////////////
			using (DataTable getTaper = new DataTable()) {
				GetTaper.param_toolset.Value = tsetName;
				GetTaper.param_machine.Value = mchName;
				GetTaper.adapter.Fill(getTaper);

				if (getTaper.Rows.Count == 1) {
					taper = (string)getTaper.Rows[0]["テーパ規格名"];
					kanri = (string)getTaper.Rows[0]["ホルダー管理区分"];
					return kanri;
				}
			}
			throw new Exception($"ツールセット{tsetName} は設備{mchName} で使用できません");
		}

		/// <summary>
		/// ホルダー名と加工機名からスピンドルタイプ名を取得する（"STD" or "HSP"）
		/// </summary>
		/// <param name="toolset">ツールセット</param>
		/// <param name="mchName">加工機名</param>
		/// <returns>高速スピンドルの場合true</returns>
		static public bool Get_HolderType(CamUtil.ToolSetData.ToolSet toolset, string mchName) {

			// /////////////////////////////////////
			// ストアドプロシージャにより
			// /////////////////////////////////////
			using (DataTable getSpintype = new DataTable()) {
				GetSpinType.param_hldname.Value = toolset.HolderName;
				GetSpinType.param_machine.Value = mchName;
				GetSpinType.adapter.Fill(getSpintype);

				if (getSpintype.Rows.Count == 1)
					return (string)getSpintype.Rows[0]["スピンドル仕様"] == "HSP";
			}
			throw new Exception($"ツールセット{toolset.tset_name}のホルダーは加工機{mchName}では使用できない設定になっています。");
		}

		/// <summary>
		/// ツールセットＣＡＭ名とテーパ規格名から新たな回転数、送り速度、寿命を得る(spin, feed, life)
		/// </summary>
		/// <param name="tsetCAM">ツールセットＣＡＭ名</param>
		/// <param name="hld_typ">テーパ規格名</param>
		/// <returns>spin,feed,life</returns>
		static public double?[] GetNewCond(string tsetCAM, string hld_typ) {
			return tSetInfo.Tables["NcConv_TSet_CHG"].AsEnumerable()
					.Where(dRow => tsetCAM == (string)dRow["tset_name_CAM"])
					.Where(dRow => hld_typ == (string)dRow["taper_std_name"])
					.Select(dRow => new double?[] { dRow.Field<double?>("spin"), dRow.Field<double?>("feedrate"), dRow.Field<double?>("life_max") })
					.First();
		}

		/// <summary>
		/// ツールセットと材質から回転数、送り速度、寿命の調整比率を得る(spin, feed, life)
		/// </summary>
		/// <param name="material">材質名</param>
		/// <param name="cond_type">切削条件調整タイプ名</param>
		/// <returns>spin,feed,life</returns>
		static public double[] GetLifeRate(string material, string cond_type) {
			double[] fout = tSetInfo.Tables["NcConv_cond_material"].AsEnumerable()
				.Where(dRow => material == (string)dRow["material_group"])
				.Where(dRow => cond_type == (string)dRow["cutter_cond_type"])
				.Select(dRow => new double[] { (double)dRow["spin"], (double)dRow["feed"], (double)dRow["life"], (double)dRow["retfeed"] })
				.FirstOrDefault();
			if (fout == null) {
				MessageBox.Show($"ＤＢの材質名：{material}、工具の切削条件タイプ：{cond_type} が存在しません。材質による切削条件の変更なしで処理します。");
				fout = new double[] { 1.0, 1.0, 1.0 };
			}
			return fout;
		}

		/// <summary>
		/// ツールセットＣＡＭ情報[不変]
		/// </summary>
		public class TSetCAM : CamUtil.ToolSetData.TSetCAM
		{
			internal TSetCAM(string tsCAM) : base(tsCAM) { }
		}

		// 静的メソッド END
		// ////////////////////////////////////////////////////////////////////







		/// <summary>ツーリングの情報(ＤＢのtoolsetより)</summary>
		public CamUtil.ToolSetData.ToolSet Toolset { get { return tol.Toolset; } }
		/// <summary>加工条件の情報(ＤＢのtoolsetCAMより)</summary>
		public TSetCAM TsetCAM { get { return m_tsetCAM; } }
		private readonly TSetCAM m_tsetCAM;

		/// <summary>最小突出し量を考慮したツールセット全長</summary>
		public double Min_length { get { return Toolset.HolderLength + tol.Ttsk; } }

		// /////////////////////////////////////////////////////////////////////////////
		// 追加 2015/08/28
		// /////////////////////////////////////////////////////////////////////////////
		private readonly Tool.Tol tol;

		// ///////////////////////////////////////////////////////////////////////////////
		// 複数の工具を同一工具番号とした場合の代表ツールセット（matchT.tsetを使用する）
		// ///////////////////////////////////////////////////////////////////////////////
		/// <summary>代表ツールセット名</summary>
		public string DA_tset_name { get { return tol.matchT.Tset.tset_name; } }
		/// <summary>代表ツールセットＩＤ（非標準の場合は"0500"以下）</summary>
		public string DA_ID { get { return tol.matchT.Tset.ID; } }
		/// <summary>代表元になったツールセット名（フリーのツールセットの場合）</summary>
		public string DA_tset_base_name { get { return tol.matchT.Tset.TsetBaseName; } }
		/// <summary>代表突出し量</summary>
		public int DA_tout_length { get { return tol.matchT.Tset.ToutLength; } }


		/// <summary>
		/// コンストラクタ
		/// 「ツールセットＣＡＭ」と工具表内のマッチした「工具」から、工具総合情報 TolInfo を作成
		/// 切削条件は「ツールセットＣＡＭ」より取得
		/// </summary>
		/// <param name="tsCAM">ツールセットＣＡＭ名</param>
		/// <param name="tol">マッチした工具情報（ツールセット情報はそのまま使用する）</param>
		public ToolSetInfo(string tsCAM, Tool.Tol tol) {

			this.tol = tol;
			if (tol.Toolset == null)
				throw new Exception("wqaefbqrefhqb");
			//this.toolset = tol.toolset;

			// 加工条件にかかわる情報を作成
			this.m_tsetCAM = new TSetCAM(tsCAM);

		}
	}
}
