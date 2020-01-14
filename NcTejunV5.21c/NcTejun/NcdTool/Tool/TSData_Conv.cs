using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;

using System.IO;
using CamUtil;

namespace NcdTool.Tool
{
	/// <summary>
	/// 工具表を表示・修正するフォーム
	/// </summary>
	partial class TSData : DataTable
	{
		static void SchemaValidationHandler(object sender, System.Xml.Schema.ValidationEventArgs e) {
			switch (e.Severity) {
			case System.Xml.Schema.XmlSeverityType.Error:
				Console.WriteLine("Schema Validation Error: {0}", e.Message);
				break;
			case System.Xml.Schema.XmlSeverityType.Warning:
				Console.WriteLine("Schema Validation Warning: {0}", e.Message);
				break;
			}
		}





		/// <summary>
		/// データテーブル　→　工具リスト　変換（"tol"にもセット）
		/// </summary>
		/// <param name="initS">以降の工具をT-1とする</param>
		/// <param name="initK">以降の工具をT-1とする</param>
		/// <returns></returns>
		public Tol[] TolSet(int initS, int initK) {
			List<Tol> tolList = new List<Tol>();
			Tol tol;
			char tmod;

			// 新マッチングの場合は工具番号の大小を考慮するため不要
			string sort = DefaultView.Sort;
			DefaultView.Sort = "シートNo,工具No";

			if (initK == 0) {
				foreach (DataRowView dView in this.DefaultView) {
					if (dView.Row.RowState == System.Data.DataRowState.Deleted) continue;

					//this.Add(new Tol());
					//this[this.Count - 1].tolSet(dRow, this.Count);
					tmod = '0';
					if ((int)dView.Row["工具No"] < 0)
						tmod = '1';
					tol = new Tol(tmod, dView.Row, tolList.Count, sumptLst.Consumpt((int)dView.Row["ID"]), Tejun.Mach.name);
					tolList.Add(tol);
					dView.Row["tol"] = tol;
				}
			}
			else {
				List<int> ariNo = new List<int>();
				foreach (DataRowView dView in this.DefaultView)
					if (initS == (int)dView.Row["シートNo"])
						if (initK >= this.KakoJun(dView.Row)) {
							// 使用している工具番号のリストを作成する
							ariNo.Add((int)dView.Row["工具No"]);
						}

				foreach (DataRowView dView in this.DefaultView) {
					if (dView.Row.RowState == System.Data.DataRowState.Deleted) continue;

					tmod = '0';
					if ((int)dView.Row["工具No"] < 0)
						tmod = '1';
					else {
						if (initK < this.KakoJun(dView.Row)) {
							if (initS == (int)dView.Row["シートNo"]) {
								if (!ariNo.Contains((int)dView.Row["工具No"]))
									tmod = '1';
							}
							else
								tmod = '1';
						}
					}
					tol = new Tol(tmod, dView.Row, tolList.Count, sumptLst.Consumpt((int)dView.Row["ID"]), Tejun.Mach.name);
					tolList.Add(tol);
					dView.Row["tol"] = tol;
				}
			}
			DefaultView.Sort = sort;
			return tolList.ToArray();
		}




		/// <summary>
		/// 工具クラス(tols)　→　データテーブル　変換
		/// </summary>
		/// <param name="tsht">変換元工具リスト</param>
		/// <param name="add">不足工具を追加する</param>
		/// <returns>不足工具数</returns>
		public int TableUpDate(NcTejun.TejunSet.ToolSheet tsht, bool add) {
			int fusoku = 0;

			// データテーブル情報への書き込み
			foreach (Tol sad in tsht.Tols)
				if ((add && sad.Addtool) || sad.Tmod != '2')
					TableUpDate_sub(sad);

			// データテーブル情報へ追加
			foreach (Tol sad in tsht.Tols)
				if (sad.Tmod == '2') {
					if ((add && sad.Addtool)) {
						sad.DRowLink["ID"] = ++addIDNo;
						this.Rows.Add(sad.DRowLink);
					}
					else
						fusoku++;
				}

			// tolsからの変換後
			this.AcceptChanges();
			//m_rireki.Rireki_Save(tolstData.Copy());	// 変更履歴の追加

			return fusoku;
		}
		private void TableUpDate_sub(Tol sad) {
			switch (sad.Tmod) {
			case '0':
				if ((int)sad.DRowLink["工具No"] < 0) {
					sad.DRowLink["シートNo"] = sad.Rnum;
					sad.DRowLink["工具No"] = sad.Unum;
					if (sad.DRowLink["変更"] == DBNull.Value)
						sad.DRowLink["変更"] = 'R';
				}
				//if (sad.dRowLink["突出し量UX"] == DBNull.Value)
				//	if (sad.dRowLink["変更"] == DBNull.Value)
				//		sad.dRowLink["変更"] = 'R';
				//if (sad.dRowLink["ホルダ１"] == DBNull.Value)
				//	if (sad.dRowLink["変更"] == DBNull.Value)
				//		sad.dRowLink["変更"] = 'R';
				break;
			case '1':
				if (sad.DRowLink["変更"] == DBNull.Value)
					sad.DRowLink["変更"] = 'R';
				sad.DRowLink["シートNo"] = sad.Rnum;
				sad.DRowLink["工具No"] = sad.Unum;
				break;
			case '2':
				sad.DRowLink = this.NewRow();
				sad.DRowLink["ID"] = 0;
				sad.DRowLink["tol"] = sad;
				sad.DRowLink["変更"] = 'A';
				sad.DRowLink["シートNo"] = sad.Rnum;
				sad.DRowLink["分割"] = sad.Bnum;
				sad.DRowLink["工具No"] = sad.Unum;
				sad.DRowLink["M01"] = sad.EdtM001;
				//sad.dRowLink["M100"] = sad.edtM100;
				//sad.dRowLink["工具名UX"] = sad.tnam_kinf;
				sad.DRowLink["工具名PC"] = sad.Toolset.ToolName;
				sad.DRowLink["ＮＣ限定"] = sad.NnamS;
				break;
			}
			// 共通
			if (sad.Toolset == null) {
				sad.DRowLink["ツールセット"] = DBNull.Value;
				sad.DRowLink["工具名PC"] = DBNull.Value;
				//sad.dRowLink["突出し補正量"] = 0.0;
			}
			else {
				sad.DRowLink["ツールセット"] = sad.Toolsetname;
				sad.DRowLink["工具名PC"] = sad.Toolset.ToolName;
				//sad.dRowLink["突出し補正量"] = sad.tsk_hosei;
			}
			sad.DRowLink["高速Ｓ"] = sad.Hsp;
			sad.DRowLink["突出し量PC"] = ColumnDataSet(sad.Ttsk);
			//sad.dRowLink["突出し量UX"] = ColumnDataSet(sad.ttsk_unix);
			sad.DRowLink["ホルダ１"] = ColumnDataSet(sad.Toolset.HolderName);
			//sad.dRowLink["突出し量１"] = ColumnDataSet(sad.htsk[0]);
			//sad.dRowLink["ホルダ２"] = ColumnDataSet(sad.hldr[1]);
			//sad.dRowLink["突出し量２"] = ColumnDataSet(sad.htsk[1]);
			if (sad.Ttsk <= sad.Toolset.ToutLength)
				sad.DRowLink["標準セット"] = true;
			else
				sad.DRowLink["標準セット"] = false;

			return;
		}


	
		/// <summary>
		/// 工具クラスの「消耗率」などをテーブルに表示する
		/// </summary>
		public void TableNcInfo(NcTejun.TejunSet.ToolSheet tsht) {

			// データテーブル情報への書き込み
			foreach (Tol sad in tsht.Tols) {
				if (sad.DRowLink == null)
					continue;
				sad.DRowLink["消耗率"] = sad.Consumption_new / 100.0;
				sad.DRowLink["加工長"] = 0.0;
				sad.DRowLink["加工順"] = DBNull.Value;
				sad.DRowLink["対応ＮＣ"] = DBNull.Value;
				sad.DRowLink["使用回数"] = 0;
			}
			foreach (NcName.NcNam nnam in NcdTool.Tejun.NcList.NcNamsTS(tsht.TolstName))
				foreach (NcName.Kogu skog in nnam.Tdat) {
					Tol sad;
					foreach (TMatch.MatchK mtchk in skog.matchK) {
						sad = mtchk.K2.Tlgn;
						if (sad.DRowLink == null)
							continue;
						sad.DRowLink["加工長"] = ((double)sad.DRowLink["加工長"] + mtchk.divData.ncdist.G01 / 1000.0).ToString("0.00");
						sad.DRowLink["使用回数"] = (int)sad.DRowLink["使用回数"] + 1;
						if (sad.DRowLink["加工順"] != DBNull.Value)
							sad.DRowLink["加工順"] += " ";
						sad.DRowLink["加工順"] += skog.kakoJun.ToString("000");
						if (sad.DRowLink["対応ＮＣ"] != DBNull.Value)
							sad.DRowLink["対応ＮＣ"] += " ";
						sad.DRowLink["対応ＮＣ"] += skog.Parent.nnam;
					}
				}
		}




		/// <summary>
		/// 加工順ＮＣデータ一覧データ　→　データテーブル変換（個別）
		/// </summary>
		/// <param name="tse">ＰＣの工具表から取得した文字列</param>
		public bool ItemSetTNoSet(TS_Edit_Data tse) {

			DataRow workRow;
			string ss;

			workRow = this.NewRow();
			workRow["シートNo"] = tse.snumN;
			workRow["工具No"] = tse.tnumN;
			workRow["分割"] = tse.tol.Bnum;
			workRow["M01"] = tse.tol.EdtM001;
			//workRow["M100"] = tse.tol.edtM100;
			//workRow["高速Ｓ"] = tse.tol.bnum == 2 ? true : false;
			workRow["ツールセット"] = tse.tol.Toolsetname;
			workRow["工具名PC"] = tse.tol.Toolset.ToolName;

			ss = "";
			foreach (string stmp in tse.gent) ss += " " + stmp;
			if (ss.Length == 0)
				workRow["ＮＣ限定"] = DBNull.Value;
			else
				workRow["ＮＣ限定"] = ss.Substring(1);

			workRow["突出し量PC"] = tse.tol.Ttsk;
			workRow["変更"] = DBNull.Value;
			workRow["ID"] = ++addIDNo;
			this.Rows.Add(workRow);

			return true;
		}


	
		private object ColumnDataSet(string value) {
			if (value == null)
				return DBNull.Value;
			return value;
		}
		private object ColumnDataSet(double value) {
			if (value < 0.0001)
				return DBNull.Value;
			return value;
		}
	}
}