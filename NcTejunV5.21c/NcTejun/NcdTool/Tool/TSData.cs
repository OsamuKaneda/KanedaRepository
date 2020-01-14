using System;
using System.Collections.Generic;
using System.Data;

namespace NcdTool.Tool
{
	/// <summary>
	/// 工具表フォームToolSheetに表示されるテーブル
	/// </summary>
	partial class TSData : DataTable
	{
		/// <summary>基本ソート方法</summary>
		static string defaultSort = "加工順";

		/// <summary>
		/// 消耗率を保存するストラクチャー
		/// </summary>
		public readonly struct ItemData
		{
			public readonly int ID;			// 工具ＤＢへのリンクＩＤ Add in 2015/01/23
			public readonly string tjn_name;
			public readonly string ncd_name;
			public readonly int tdno;
			public readonly int snum;	// 不要
			public readonly int tnum;	// 不要
			public readonly double data;
			public ItemData(int id, string tjn_name, string ncd_name, int tdno, int snum, int tnum, double data) {
				this.ID = id;
				this.tjn_name = tjn_name;
				this.ncd_name = ncd_name;
				this.tdno = tdno;
				this.snum = snum;
				this.tnum = tnum;
				this.data = data;
			}
			public ItemData(int id, int jj, System.Xml.XPath.XPathNavigator xpathNavigator) {
				System.Xml.XPath.XPathNodeIterator nodes;
				this.ID = id;
				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + id.ToString() + "]/Consumption[" + jj.ToString() + "]/TejunName");
				nodes.MoveNext();
				this.tjn_name = nodes.Current.Value;
				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + id.ToString() + "]/Consumption[" + jj.ToString() + "]/NcName");
				nodes.MoveNext();
				this.ncd_name = nodes.Current.Value;
				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + id.ToString() + "]/Consumption[" + jj.ToString() + "]/NcToolCount");
				nodes.MoveNext();
				this.tdno = nodes.Current.ValueAsInt;
				nodes = xpathNavigator.Select("/ToolSheet/Tool[" + id.ToString() + "]/Consumption[" + jj.ToString() + "]/Data");
				nodes.MoveNext();
				this.data = nodes.Current.ValueAsDouble;
				this.snum = 0;
				this.tnum = 0;
			}
		}

		/// <summary>
		/// ＰＣ保存の工具表の消耗率
		/// </summary>
		private struct ConsumptList : IEnumerable<ItemData>, System.Collections.IEnumerable
		{
			private List<ItemData> itemData;

			//public ItemData this[int index] { get { return itemData[index]; } }

			/// <summary>消耗率のデータ数（手順名単位）</summary>
			public int Count(string tjnName) {
				int ii = 0;
				foreach (ItemData itmp in itemData) if (tjnName == itmp.tjn_name) ii++;
				return ii;
			}
			/// <summary>保存されている消耗率の手順名（同じ手順名を除く）。ない場合はnull</summary>
			public string TejunNames {
				get {
					List<string> aa = new List<string>();
					foreach (ItemData item in itemData)
						if (!aa.Contains(item.tjn_name) && Tejun.TejunName != item.tjn_name)
							aa.Add(item.tjn_name);
					if (aa.Count == 0) return null;
					string bb = "";
					foreach (string ss in aa) bb += " " + ss;
					return bb.Substring(1);
				}
			}

			/// <summary>消耗率のデータのクリア</summary>
			public void Clear() { itemData.Clear(); }
			/// <summary>消耗率のデータ追加</summary>
			public void Add(ItemData item) { itemData.Add(item); }

			/// <summary>指定されたＩＤの消耗率を削除する</summary>
			public void Remove(int id) {
				int ii = 0;
				while (ii < itemData.Count) {
					if (id == itemData[ii].ID)
						itemData.RemoveAt(ii);
					else
						ii++;
				}
			}
			/// <summary>指定された手順の消耗率を削除する</summary>
			public void Remove(string tjnName) {
				int ii = 0;
				while (ii < itemData.Count) {
					if (tjnName == itemData[ii].tjn_name)
						itemData.RemoveAt(ii);
					else
						ii++;
				}
			}
			/// <summary>指定されたＩＤを持ちこの手順以外のデータを積算</summary>
			public double Consumpt(int id) {
				double dd = 0.0;
				foreach (ItemData item in itemData)
					if (item.ID == id && Tejun.TejunName != item.tjn_name)
						dd += item.data;
				return dd;
			}

			/// <summary>同一データの有無</summary>
			public bool Same(ItemData itdata) {
				foreach (ItemData item in itemData) {
					if (itdata.ID != item.ID) continue;
					if (itdata.tjn_name != item.tjn_name) continue;
					if (itdata.ncd_name != item.ncd_name) continue;
					if (itdata.tdno != item.tdno) continue;
					//if (itdata.snum != item.snum) continue;
					//if (itdata.tnum != item.tnum) continue;
					if (Math.Abs(itdata.data - item.data) > 0.001) continue;
					return true;
				}
				return false;
			}


			public ConsumptList(int dummy) {
				itemData = new List<ItemData>();
			}

			// //////////////////////
			// IEnumerator<ItemData>
			// //////////////////////
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return new IE<ItemData>(itemData); }
			IEnumerator<ItemData> IEnumerable<ItemData>.GetEnumerator() { return new IE<ItemData>(itemData); }
			private struct IE<U> : IEnumerator<ItemData>
			{
				private List<ItemData> item;
				private int position;
				public IE(List<ItemData> array) { item = array; position = -1; }
				Object System.Collections.IEnumerator.Current { get { try { return item[position]; } catch (IndexOutOfRangeException) { throw new InvalidOperationException(); } } }
				ItemData IEnumerator<ItemData>.Current { get { try { return item[position]; } catch { return default; } } }
				void System.Collections.IEnumerator.Reset() { position = -1; }
				bool System.Collections.IEnumerator.MoveNext() { position++; return position < item.Count; }
				void IDisposable.Dispose() { ;}
			}
		}

		// /////////////
		// 以上 static
		// /////////////




		/// <summary>工具表で設定された親の手順の製造番号（V3より追加）</summary>
		public string KtejunSeba { get { return m_ktejunSeba; } }
		private string m_ktejunSeba;
		/// <summary>工具表で設定された親の手順名</summary>
		public string KtejunName { get { return m_ktejunName; } }
		private string m_ktejunName;

		/// <summary>今までに使用した最大のＩＤ番号</summary>
		private int addIDNo;

		// ///////////
		// Rowsの隠蔽
		// ///////////
		/// <summary>工具データテーブル</summary>
		public new DataRowCollection Rows { private get { return base.Rows; } set { ; } }
		/// <summary>工具データテーブル数（DataRowState.Deletedも含む）</summary>
		public int RowsCount { get { return Rows.Count; } }
		/// <summary>工具データテーブルの追加</summary>
		public void RowsAdd(DataRow dRow) {
			//dRow["ID"] = MaxID() + 1;
			dRow["加工長"] = 0.0;
			dRow["使用回数"] = 0;
			dRow["ＮＣ限定"] = DBNull.Value;
			dRow["加工順"] = DBNull.Value;
			dRow["対応ＮＣ"] = DBNull.Value;
			dRow["ID"] = ++addIDNo;
			Rows.Add(dRow);
		}
		/// <summary>工具データテーブルのクリア</summary>
		public new void Clear() { base.Clear(); sumptLst.Clear(); }


		// ///////////
		// 消耗率
		// ///////////
		/// <summary>ＰＣ保存の工具シート情報の消耗率データ</summary>
		private ConsumptList sumptLst;
		/// <summary>ＰＣ保存の工具シート情報の消耗率データのクリア</summary>
		public void ConsumptClear() { sumptLst.Clear(); }
		/// <summary>消耗率のデータの変更の有無</summary>
		public bool ChgShomo { get { return m_chgShomo; } }
		private bool m_chgShomo;

		/// <summary>指示された加工順以降の工具を消去</summary>
		/// <param name="kjun">基準位置</param>
		public void Delete(int kjun) {
			foreach (DataRow dRow in this.Rows)
				if (kjun <= this.KakoJun(dRow)) {
					//sumptLst.Remove((int)dRow["シートNo"], (int)dRow["工具No"]);
					dRow.Delete();
				}
		}



		/// <summary>
		/// コンストラクタ（唯一）
		/// </summary>
		/// <param name="tolstName"></param>
		public TSData(string tolstName)
			: base(tolstName) {
			sumptLst = new ConsumptList(0);
			m_ktejunSeba = null;
			m_ktejunName = null;
			addIDNo = 0;
			m_chgShomo = false;
			// データテーブルのコラムのセット
			SetColumns();
		}

		/// <summary>
		/// データテーブルのデータを読込む（新規作成含む）
		/// </summary>
		/// <param name="tolstName">工具表名</param>
		public void TolstSet_PC(string tolstName) {
			// 工具表ファイルのオープン
			//System.IO.FileInfo fi = new System.IO.FileInfo(Program.frm1.tjnDir + "\\" + tolstName + ".tol");
			System.IO.FileInfo newfi = new System.IO.FileInfo(Tejun.TolDir + "\\" + tolstName + ".tol");

			if (newfi.Exists) {
				// ///////////////
				// 新工具表の作成
				// ///////////////
				ItemSetPC(true, newfi.FullName);
				if (sumptLst.TejunNames != null) {
					//DialogResult aaa = MessageBox.Show(
					//	"以下の手順の工具消耗データが入力されました。クリアする場合は工具表の「全消耗クリア」をクリックしてください。\n" + sumptLst.TejunNames,
					//	"消耗率の利用", MessageBoxButtons.OK, MessageBoxIcon.Question);

					// 使用する消耗データの設定
					List<string> rmTejun = new List<string>();
					TSFormLife ttmp = new TSFormLife(rmTejun, sumptLst.TejunNames, KtejunName);
					ttmp.ShowDialog();
					foreach (string stmp in rmTejun) sumptLst.Remove(stmp);
				}
			}
			else {
				// 新規作成
				if (NcTejun.Output.ToolSheetDB.TSHEET_HEAD_ID(Tejun.Seba, tolstName, out string tjnName) > 0) {
					if (tjnName != NcdTool.Tejun.TejunName)
						throw new Exception(
							$"新規に作成される 製造番号：{Tejun.Seba} 工具表：{tolstName} はすでに手順：{tjnName} によってデータベースに登録済みです。製造番号、工具表名のいずれかを変更してください。");
				}
				System.Windows.Forms.MessageBox.Show("工具表：" + tolstName + " はＰＣサーバー内に存在しません。空の工具表を作成します。");
				m_ktejunSeba = Tejun.Seba;
				m_ktejunName = Tejun.TejunName;
			}
			return;
		}

		/// <summary>初期設定値でソートする</summary>
		public void DefaultSort() { this.DefaultView.Sort = defaultSort; }

		/// <summary>
		/// この手順の消耗率を保存時のためにデータに追加しておく
		/// </summary>
		/// <param name="tolstname"></param>
		/// <returns>変更の有無 変更あり:true</returns>
		public bool Shomo(string tolstname) {
			ItemData item;
			int cnt = 0;

			// /////////////////////////////////////
			// すべて同一データであるかチェックする
			// /////////////////////////////////////
			if (sumptLst.Count(Tejun.TejunName) > 0) {
				foreach (NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsTS(tolstname)) {
					for (int kk = 0; kk < ncnam.Itdat; kk++)
						for (int ll = 0; ll < ncnam.Tdat[kk].matchK.Length; ll++) {
							if (ncnam.Tdat[kk].matchK[ll].K2.Tlgn.Tmod != '0')
								continue;
							item = new ItemData(
								(int)ncnam.Tdat[kk].matchK[ll].K2.Tlgn.DRowLink["ID"],
								Tejun.TejunName, ncnam.nnam, kk, 0, 0,
								ncnam.Tdat[kk].matchK[ll].divData.Consumption);

							if (!sumptLst.Same(item)) {
								cnt = -1; break;
							}
							cnt++;
						}
					if (cnt < 0) break;
				}
			}

			if (cnt > 0 && cnt == sumptLst.Count(Tejun.TejunName))
				// 消耗率データに変更なし
				this.m_chgShomo = false;
			else {
				// 消耗率データを新たに作成
				this.m_chgShomo = true;
				sumptLst.Remove(Tejun.TejunName);
				foreach (NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsTS(tolstname))
					for (int kk = 0; kk < ncnam.Itdat; kk++)
						for (int ll = 0; ll < ncnam.Tdat[kk].matchK.Length; ll++) {
							if (ncnam.Tdat[kk].matchK[ll].K2.Tlgn.DRowLink == null)
								continue;	// ツールシートなしの場合、不足工具がある場合
							item = new ItemData(
								(int)ncnam.Tdat[kk].matchK[ll].K2.Tlgn.DRowLink["ID"],
								Tejun.TejunName, ncnam.nnam, kk, 0, 0,
								ncnam.Tdat[kk].matchK[ll].divData.Consumption);

							sumptLst.Add(item);
						}
			}
			// 追加の場合はfalse
			return this.m_chgShomo;
		}


		/// <summary>
		/// データテーブルのコラムのセット
		/// </summary>
		private void SetColumns() {
			// DataTableにnullをセットするとDBNull.Valueとなる
			DataColumn dclm;

			// ADD in 2015/01/23
			dclm = this.Columns.Add("ID", typeof(Int32));
			dclm.AllowDBNull = false;
			dclm.DefaultValue = DBNull.Value;

			// /////////////
			// 保存対象情報
			// /////////////
			dclm = this.Columns.Add("ツールセット", typeof(String));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = DBNull.Value;

			// TEMP
			dclm = this.Columns.Add("tol", typeof(Tol));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = null;
			dclm = this.Columns.Add("変更", typeof(char));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = DBNull.Value;

			// /////////////
			// 保存対象情報
			// /////////////
			dclm = this.Columns.Add("シートNo", typeof(Int32));
			dclm.AllowDBNull = false;
			dclm.DefaultValue = 0;
			dclm = this.Columns.Add("工具No", typeof(Int32));
			dclm.AllowDBNull = false;
			dclm.DefaultValue = 0;
			dclm = this.Columns.Add("高速Ｓ", typeof(bool));
			dclm.AllowDBNull = false;
			dclm.DefaultValue = false;
			dclm = this.Columns.Add("分割", typeof(bool));
			dclm.AllowDBNull = false;
			dclm.DefaultValue = false;
			dclm = this.Columns.Add("M01", typeof(bool));
			dclm.AllowDBNull = false;
			dclm.DefaultValue = false;

			// TEMP
			dclm = this.Columns.Add("工具名PC", typeof(String));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = DBNull.Value;

			// /////////////
			// 保存対象情報
			// /////////////
			dclm = this.Columns.Add("ＮＣ限定", typeof(String));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = null;
			dclm = this.Columns.Add("突出し量PC", typeof(double));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = DBNull.Value;

			// TEMP
			dclm = this.Columns.Add("ホルダ１", typeof(String));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = DBNull.Value;

			// /////////////
			// 保存対象情報（consumptより ADD 2014/11/18）
			// /////////////
			dclm = this.Columns.Add("消耗率", typeof(double));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = DBNull.Value;

			// TEMP
			dclm = this.Columns.Add("加工長", typeof(double));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = DBNull.Value;
			dclm = this.Columns.Add("使用回数", typeof(Int32));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = DBNull.Value;
			dclm = this.Columns.Add("加工順", typeof(String));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = DBNull.Value;
			dclm = this.Columns.Add("対応ＮＣ", typeof(String));
			dclm.AllowDBNull = true;
			dclm.DefaultValue = DBNull.Value;
			// 新規追加 2016/02/05
			dclm = this.Columns.Add("標準セット", typeof(Boolean));
			dclm.AllowDBNull = false;
			dclm.DefaultValue = false;
		}

		/// <summary>
		/// 変更を確定
		/// </summary>
		public void Decision() {

			// すべてが挿入の場合のみ「変更」をクリアする2010/09/07
			bool ALL = true;
			foreach (DataRow dRow in this.Rows) {
				if (dRow.RowState == DataRowState.Deleted)
					continue;
				if (dRow["変更"] == DBNull.Value) {
					ALL = false;
					break;
				}
				if ((char)dRow["変更"] != 'A') {
					ALL = false;
					break;
				}
			}
			foreach (DataRow dRow in this.Rows) {
				if (dRow.RowState == DataRowState.Deleted)
					continue;
				if (dRow["変更"] != DBNull.Value) {
					if (ALL) {
						dRow["変更"] = DBNull.Value;
					}
					else {
						if ((char)dRow["変更"] != 'A') dRow["変更"] = DBNull.Value;
					}
				}
			}
			AcceptChanges();
		}

		/// <summary>
		/// 加工順の数値を取り出す。加工順がない場合は０を返す
		/// </summary>
		/// <param name="dRow"></param>
		/// <returns></returns>
		public int KakoJun(DataRow dRow) {
			// ["加工順"] = "006 010 012"
			if (dRow["加工順"] == DBNull.Value) return 0;	// 加工に使用しない工具
			string[] stmp = ((string)dRow["加工順"]).Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			return Convert.ToInt32(stmp[0]);
		}

		/// <summary>
		/// 工具表を加工順にすべて表示する情報
		/// </summary>
		internal readonly struct TS_Edit_Data
		{
			public readonly string ncname;
			public readonly Tol tol;
			public string Tsname { get { return tol.Toolsetname; } }
			///// <summary>元の値（シート番号）</summary>
			//public int snumA { get { return tol.rnum; } }
			///// <summary>元の値（工具番号）</summary>
			//public int tnumA { get { return tol.unum; } }

			/// <summary>変更する値（シート番号）</summary>
			public readonly int snumN;
			/// <summary>変更する値（工具番号）</summary>
			public readonly int tnumN;

			/// <summary>実際のマッチングのシミュレーションをするためのＮＣ限定</summary>
			public readonly List<string> gent;

			public TS_Edit_Data(NcName.Kogu skog, System.Windows.Forms.DataGridViewRow dRow) {

				if (skog.Parent.nnam != (string)dRow.Cells["ＮＣデータ"].Value)
					throw new Exception("awefvqeg");
				ncname = skog.Parent.nnam;

				if (skog.matchK[0].K2.Tlgn.Toolsetname != (string)dRow.Cells["ツールセット"].Value)
					throw new Exception("awefvqeg");
				tol = skog.matchK[0].K2.Tlgn;

				snumN = Convert.ToInt32((string)dRow.Cells["シートNo"].Value);
				tnumN = Convert.ToInt32((string)dRow.Cells["工具No"].Value);

				// 積算値の初期化
				gent = new List<string>();
			}
		}
	}
}
