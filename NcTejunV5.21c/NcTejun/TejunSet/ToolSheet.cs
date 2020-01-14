using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Drawing.Printing;
using CamUtil;

// 01 2006/09/22 ７８７行目、ソートされていない場合、無限ループとなる
// 02 2006/09/22 保存時、ソートして出力しないと変

// 00 存在しない工具のホルダー突出しに関するメッセージがある（～選択されました）
// 00 不要なメッセージがある（～not need）
// 00 foreachで削除した項目が選択される
// 00 ソートされると設定したToolTipsが消える
// 00 追加した場合の色が設定されない
// 00

namespace NcTejun.TejunSet
{
	using Tols = RO_Collection<NcdTool.Tool.Tol>;
	using Told = RO_Collection<NcdTool.Tool.Tol>.InnerList;

	/// <summary>
	/// 工具表を表示・修正するフォーム
	/// </summary>
	partial class ToolSheet : Form
	{
		/// <summary>工具表名が工具表とのマッチングを実行したかを表す</summary>
		static public bool Match(string tsheetName) {
			foreach (NcTejun.TejunSet.ToolSheet tsht in NcTejun.Program.frm1.TsheetList) if (tsheetName == tsht.TolstName) return (tsht.Tols.Count > 0);
			throw new Exception("kqwjefdqefqe");
		}

		/// <summary>工具表名から工具表フォームのインスタンスを得る</summary>
		/// <param name="tsheetName"></param>
		/// <returns></returns>
		static public ToolSheet SelToolSheet(string tsheetName) {
			foreach (ToolSheet tsht in NcTejun.Program.frm1.TsheetList) if (tsheetName == tsht.TolstName) return tsht;
			throw new Exception("工具表" + tsheetName + " は存在しません");
		}

		/// <summary>
		/// 全ての工具表とＮＣデータとのマッチングを実行
		/// </summary>
		/// <remarks>
		/// ToolSheet.Matching_TSheet		すべての工具表と関連するTSetのループ
		///		ToolSheet.Matching			すべてのＮＣデータを処理し工具表を作成
		///			TMatch.Divide			ＮＣデータの自動分割
		///			Tol.tnadds				マッチする工具の検索
		///			Tol						マッチする工具がない場合に工具を作成する
		///			kohoSheetSet()			TMatch.MatchT の作成
		///			new TMatch.MatchK		マッチング情報作成
		///			TMatch.BunkData			分割情報の作成
		///			TMatch.MatchT.SortTsk()	同一ツールセットが複数追加された場合に、突出し量により使用工具を配分する
		///			TMatch.MatchK.MatchK2.Set_Tool()	各ＮＣデータの工具へのリンクが作成され、工具番号などが決定する
		///		ToolSheet.Matching_TS		この工具表に追加された工具の設定
		/// </remarks>
		/// <param name="addtool">不足分を工具表に追加</param>
		/// <param name="initS">以降のシートの工具番号を再設定する</param>
		/// <param name="initT">以下を使用不可とする工具の番号</param>
		/// <param name="tsheet">対象となる工具表。すべて間場合はnull</param>
		/// <param name="initK">以降の加工順の工具番号を再設定する</param>
		/// <returns>変更の有無 変更あり:true</returns>
		static public void Matching_TSheet(bool addtool, int initS, int initT, ToolSheet tsheet, int initK) {
			foreach (ToolSheet tsht in Program.frm1.TsheetList) {
				if (tsheet == null) {
					// 最初の手順確定時に実行される全ての工具表の情報抽出の実行
					if (tsht.TolstData.RowsCount == 0) continue;
					if (initS != 0 || initT != 0 || initK != 0) throw new Exception("");
				}
				else {
					// 個々の工具表の更新時に実行される
					if (tsheet.TolstName != tsht.TolstName) continue;
				}

				// 初期工具リストの作成
				tsht.m_tols = new Told(tsht.TolstData.TolSet(initS, initK));

				// （toolSheet.stinc : 工具を追加する場合の基準シート番号）
				if (tsht.Stinc > 0) {
					initS = tsht.Stinc;
					initT = NcdTool.Tejun.Mach.Nmgz;
				}

				// /////////////////////////////////////////////////
				// ＮＣデータと工具のマッチング（不足工具を調べる）
				// /////////////////////////////////////////////////
				tsht.Matching(initS, initT);
				Application.DoEvents();

				// //////////////////////
				// 追加された工具の設定
				// //////////////////////
				tsht.Matching_TS(addtool);
				Application.DoEvents();

				// 次の工具情報取得のためのリセット
				initS = initT = initK = 0;
				tsht.m_stinc = 0;

				// 保存状態のセット
				if (tsheet == null)
					tsht.Tolst_save = tsht.TolstData.ChgShomo ? false : true;
			}
			// 工具単位の分割情報のセット add in 2014/10/31
			NcdTool.NcName.NcNam.BunkSet();
			// 次工具番号をセットする（全てのtsetの計算終了後に実行する）
			NcdTool.TMatch.MatchK.MatchK2.Set_NextToolNo();
			// Ｏ番号をセットする（全てのtsetの計算終了後に実行する）
			NcdTool.TMatch.MatchK.MatchK2.Set_Onum();
			// メッセージの表示
			Application.DoEvents();
			CamUtil.LogOut.Warning("toolsheetAdd");
		}

		// //////////////
		// 以上、static
		// //////////////






		/// <summary>工具表の名前（==""は工具表なしの場合）</summary>
		public string TolstName { get { return TolstData.TableName; } }
		/// <summary>このTSetで使用する工具一覧</summary>
		public Tols Tols { get { return m_tols.AsReadOnly; } } private Told m_tols;

		/// <summary>
		/// 工具追加のときの基準シート番号
		/// 一般には０であるが、追加の時に'S'を指定することにより次の番号にすることが可能
		/// </summary>
		public int Stinc { get { return m_stinc; } }
		private int m_stinc = 0;

		/// <summary>工具表のデータテーブル</summary>
		public NcdTool.Tool.TSData TolstData { get { return m_tolstData; } } private NcdTool.Tool.TSData m_tolstData;

		/// <summary>関連するＮＣデータの数</summary>
		public int NcNamsCount { get { return NcdTool.Tejun.NcList.NcNamsTS(this.TolstName).Count; } }

		///// <summary>工具表で設定された親の手順の製造番号（V3より追加）</summary>
		//public string ktejunSeba { get { return tolstData.ktejunSeba; } }
		///// <summary>工具表で設定された親の手順名</summary>
		//public string ktejunName { get { return tolstData.ktejunName; } }


		// ////////////////////////////////////////
		// 以下は工具表の変更の状況を表わすプロパティ
		// ////////////////////////////////////////

		/// <summary>UNIXツールシートの上書き保存回数</summary>
		public int HenkoNumb { get { return m_henkoNumb; } }
		private int m_henkoNumb = 0;

		/// <summary>ツールシートの元のユニックスからの変更の有無（再読込みのみでfalseに戻る）</summary>
		public bool HenkoFile { get { return m_henkoFile; } }
		//public bool henkoFile { get { if (m_rireki.rirekiNow <= 1) return false; else return true; } }
		private bool m_henkoFile = false;

		/// <summary>上書き保存されている場合true</summary>
		public bool Tolst_save {
			get { return !上書き保存SToolStripButton.Enabled; }
			private set {
				上書き保存SToolStripButton.Enabled = !value;
				開くOtoolStripButton.Enabled = !value;
				if (!value)
					m_henkoFile = true;
			}
		}
		/// <summary>工具表が確定している場合true</summary>
		public bool Tolst_commit {
			get { return !toolStripButton_Decision.Enabled; }
			private set {
				bool set = toolStripButton_Decision.Enabled;
				toolStripButton_Decision.Enabled = !value;
				toolStripButton_Cancel.Enabled = !value;
				// value==trueの場合、工具表にエラーがなく確定したことをForm1に知らせる
				if (set == value) Program.frm1.CheckedChanged();
			}
		}

		/// <summary>
		/// データグリッドビューの行/セルが確定していれば true。
		/// dataGridView1_CellValueChanged の Validate() によりセルが確定したら行を確定するようにしている。
		/// </summary>
		public bool DgvCommit { get { return !dataGridView1.IsCurrentRowDirty; } }
		
		// /// <summary>操作履歴を保存するクラス</summary>
		// internal Rireki rireki { get { return m_rireki; } }
		// private Rireki m_rireki;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="tsetname">ツールセット名</param>
		/// <param name="count">ツールセットの順序（＞＝０）</param>
		internal ToolSheet(string tsetname, int count) {
			InitializeComponent();

			// フォームの作成
			//m_tsForm = new FormTjnText();
			//m_tsForm.Text = "ツールシート名：" + tolstName;

			this.Text = "工具表： " + tsetname;
			m_tolstData = new NcdTool.Tool.TSData(tsetname);
			m_tols = new Told();

			//tolstData.RowChanged += new DataRowChangeEventHandler(tolstData_RowChanged);

			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(50 + 10 * count, 100 + 30 * count);

			dataGridView1.DataSource = TolstData;
			dataGridView1.ShowCellToolTips = true;

			// データグリッドビューのコラムのセット
			ColumnsSet();

			// 工具番号手動設定
			if (NcdTool.Tejun.BaseNcForm.Id == CamUtil.BaseNcForm.ID.BUHIN)
				toolStripButton_TMoSet.Enabled = false;

			toolStripButton_Shomo.Enabled = false;
		}

		public void CloseAtOnse() {
			this.FormClosing -= this.FormToolSheet_FormClosing;
			Close();
		}

		private void Matching_TS(bool addtool) {

			// /////////////////////////
			// 工具データテーブルの更新
			// /////////////////////////
			int fusoku = 0;	// 不足工具の本数
			int misiyo = 0;	// 未使用工具の本数

			// 追加工具をDataに反映(addtool == true)
			fusoku = TolstData.TableUpDate(this, addtool);
			// tmod の変更
			NcdTool.Tool.Tol.TmodSet(this, addtool);

			// 過不足のメッセージ表示
			if (TolstName != "" && TolstData.RowsCount > 0) {
				foreach (NcdTool.Tool.Tol sad in this.Tols) {
					if (sad.Consumption_new == 0.0 && sad.NoMatch) misiyo++;
				}
				string mess = "";
				if (fusoku > 0)
					mess += "加工で使用するがこの工具表内では未登録の工具が存在します。本数：" + fusoku.ToString() + "\n";
				if (misiyo > 0)
					mess += "この工具表内にＮＣデータで使用していない工具が存在します。本数：" + misiyo.ToString() + "\n";
				if (mess.Length > 0)
					MessageBox.Show(mess, "工具マッチング情報", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			// 消耗率をデータに反映（新マッチングの情報を用いる）
			if (TolstData.Shomo(this.TolstName)) {
				// 消耗率のリストが変更された場合
				switch (NcdTool.Tejun.Mach.ID) {
				case Machine.MachID.D500:
				case Machine.MachID.LineaM:
					MessageBox.Show("消耗率のデータが変更されました。");
					break;
				}
			}
			// 「消耗率」などをテーブルに表示する
			TolstData.TableNcInfo(this);

			// ソートする
			TolstData.DefaultSort();

			// 確定する
			TolstData.Decision();
			Tolst_commit = true;
		}

		/// <summary>
		/// ＰＣ or ユニックスのツールシートデータの取り込み
		/// ＰＣ or ユニックス　→　データテーブル
		/// </summary>
		internal void TolstSet() {

			TolstData.Clear();

			// 工具表の名前がない場合
			if (TolstName == "") return;

			TolstData.TolstSet_PC(TolstName);
			return;
		}

		/// <summary>
		/// ＮＣデータと工具のマッチングの設定(Ｏ番号も)
		/// </summary>
		/// <param name="initS">以降のシートの工具番号を再設定する</param>
		/// <param name="initT">以下を使用不可とする工具の番号</param>
		private void Matching(int initS, int initT) {
			NcdTool.TMatch.MatchK preMtch;
			NcdTool.NcName.Kogu preKogu;
			int snoMax = 0;		// 最大シート番号

			// 新マッチングのクリア
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsTS(this.TolstName))
				foreach (NcdTool.NcName.Kogu skog in ncnam.Tdat)
					skog.matchK = new NcdTool.TMatch.MatchK[0];

			// １シート内の工具の情報
			NcdTool.TMatch.SheetData shtData = new NcdTool.TMatch.SheetData(NcdTool.Tejun.Mach.Nmgz, initS, initT);

			// ＮＣデータのループ開始
			preMtch = null;		// 直前にマッチングした工具情報
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsTS(this.TolstName)) {

				// 工具表を最初に参照するＮＣデータの場合、マッチさせるシート番号をセットする
				if (ncnam.nmgt.Tsn.HasValue) {
					shtData.ShokiSet(ncnam.nmgt.Tsn.Value, this.Tols.ToArray());
				}

				preKogu = null;		// 直前にマッチングした工具情報（ＮＣ単位）
				foreach (NcdTool.NcName.Kogu skog in ncnam.Tdat) {
					// ツールセット未設定の場合は対象外（加工機不整合など）
					//if (skog.tsetCHG.tset_name == null) continue;
					if (skog.TsetCHK == false) continue;

					// ////////////////////////////////////////////////////////////////////
					// ＮＣデータ自動分割の処理(D500, LineaM)
					// ////////////////////////////////////////////////////////////////////
					NcdTool.TMatch.Divide div;
					switch (NcdTool.Tejun.Mach.ID) {
					case Machine.MachID.D500:
					case Machine.MachID.LineaM:
						if (skog.Consumption > 100.0) {
							div = new NcdTool.TMatch.Divide(skog, skog.Tld.XmlT.SimultaneousAxisControll);
							// 結果的に分割されなかった場合 ADD 2015/03/16
							if (div.divData.Count == 1) div = new NcdTool.TMatch.Divide(skog);
						}
						else
							div = new NcdTool.TMatch.Divide(skog);
						break;
					case Machine.MachID.MHG_1500:
						// ガンドリル加工に変換し加工長などを計算
						div = new NcdTool.TMatch.Divide(ncnam, skog);
						break;
					default:
						div = new NcdTool.TMatch.Divide(skog);
						break;
					}
					if (div.divData.Count == 0) throw new Exception("efbqhfrbqrbhqbrefh");

					// ////////////////////////////////////////////////////////////////////
					// マッチングの処理（NcName.Kogu の matchK, Tol の matchT を作成）
					// ////////////////////////////////////////////////////////////////////
					NcdTool.TMatch.MatchK.TSRule rule;
					NcdTool.Tool.Tol koho;
					skog.matchK = new NcdTool.TMatch.MatchK[div.divData.Count];
					for (int ii = 0; ii < div.divData.Count; ii++) {
						// ＮＣデータの工具ごとの分割の設定／ＮＣデータのＭ０１，Ｍ１００の挿入の設定
						rule = new NcdTool.TMatch.MatchK.TSRule(skog, preKogu, NcdTool.Tejun.Mach.Toool_nc);

						if (ncnam.Jnno.Nknum == null) {
							// 工具表を検索し一致した工具を得る
							koho = NcdTool.Tool.Tol.Tnadds(this.Tols.ToArray(), shtData.ShtNo_T, skog, div.divData[ii]);
							// 次のシートにマッチする工具があるか調べる
							if (koho == null) {
								koho = NcdTool.Tool.Tol.Tnadds(this.Tols.ToArray(), shtData.ShtNo_T + 1, skog, div.divData[ii]);
							}

							// 候補の追加された工具がルールに適合しない場合は適合する工具も追加する
							if (koho != null)
								if (koho.Tmod == '2' && rule.Match(koho, null) == false) {
									if (ProgVersion.Debug) {
										MessageBox.Show("test : 候補の追加された工具がルールに適合しない場合は適合する工具も追加する");
										koho = new NcdTool.Tool.Tol(skog, koho.matchT, rule.iRule_bun, rule.iRule_m01, NcdTool.Tejun.Mach);
									}
									CamUtil.LogOut.CheckCount("ToolSheet 393", false,
										$"ルールに適合しない工具が存在する{NcdTool.Tejun.TejunName} : {skog.Parent.nnam} {koho.Toolsetname}");
								}
							// 工具表にない新しい工具を作成（唯一）
							if (koho == null)
								koho = new NcdTool.Tool.Tol(skog, null, rule.iRule_bun, rule.iRule_m01, NcdTool.Tejun.Mach);

							// 工具kohoが使用可能なシートに移動し、そのシートを更新する（シートNo:shtData.shtNo_N）
							NcdTool.Tool.Tol preTol = NcdTool.Tool.Tol.Tnadds(this.Tols.ToArray(), shtData.ShtNo_T - 1, skog, div.divData[ii]);	// ひとつ前のシート番号と整合させる場合に使用
							NcdTool.TMatch.MatchT mtmp = shtData.KohoSheetSet(koho, skog, preTol, this.Tols.ToArray());

							// 新規の工具はmatchTをセット
							if (koho.matchT == null) koho.matchT = mtmp;
							// 新しい工具は工具表に追加（唯一）
							if (!this.Tols.Contains(koho)) this.m_tols.Add(koho);
						}
						else {
							// 参照ＮＣデータと同一の仮の工具
							koho = ncnam.Jnno.Nknum.Tdat[skog.TNo].matchK[ii].K2.ttmp;
						}

						// 工具マッチング情報作成
						skog.matchK[ii] = new NcdTool.TMatch.MatchK(ii, skog, div.divData[ii], preMtch, koho, shtData.ShtNo_N, rule);

						// ＮＣデータの情報追加（唯一）
						koho.matchT.Add_Nc(skog.matchK[ii]);

						// 今回の各情報を保存
						preMtch = skog.matchK[ii];
						preKogu = skog;
						snoMax = skog.matchK[ii].SnumN;	// 最大シート番号を保存する
					}
				}
			}

			// ////////////////////
			// マッチング後の処理
			// ////////////////////

			// 工具寿命の任意指示使用のメッセージ
			StringBuilder lifeSet = new StringBuilder();
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.NcNamsTS(this.TolstName))
				foreach (NcdTool.NcName.Kogu skog in ncnam.Tdat) {
					if (skog.TsetCHK && skog.Tld.XmlT.OPTLF.HasValue)
						lifeSet.Append(String.Format("\r\n{0,-12} {1,-14} : {2,5:F1}m -> {3,5:F1}m",
							skog.Parent.nnam, skog.TsetCHG.Tset_name,
							skog.Tld.LifeDB, skog.Tld.LifeDB * skog.Tld.XmlT.OPTLF.Value));
				}
			if (lifeSet.Length > 0) {
				FormMessageBox.Show(
					"工具寿命の任意指示",
					"以下のＮＣデータで最大加工長にＤＢと異なる値が使用されました\n" + lifeSet.ToString(), 500, 300);
			}

			// 工具シートが複数となった場合のメッセージ
			switch (NcdTool.Tejun.Mach.ID) {
			case Machine.MachID.D500:
			case Machine.MachID.LineaM:
				if (snoMax > 1)
					MessageBox.Show("この工具表の工具本数が４０本を超えます。", "工具マッチング", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				break;
			}

			// 同一ツールセットが複数追加された場合に、突出し量により使用工具を配分する
			NcdTool.TMatch.MatchT.SortTsk(this.Tols.ToArray());

			// TMatch.MatchK.K2 を設定する
			//（ここで各ＮＣデータの工具へのリンクが作成され、工具番号などが決定する）
			NcdTool.TMatch.MatchK.MatchK2.Set_Tool(this.TolstName, this.Tols.ToArray());

			// ＮＣデータとマッチングしなかった番号未設定工具の番号設定
			List<NcdTool.Tool.Tol> noMatchTol = new List<NcdTool.Tool.Tol>();
			foreach (NcdTool.Tool.Tol sad in this.Tols)
				if (sad.NoMatch && sad.Tmod == '1') {
					noMatchTol.Add(sad);
					//if (sad.rnum > 0) throw new Exception("	edbqedbh");
				}
			if (noMatchTol.Count > 0) {
				shtData = new NcdTool.TMatch.SheetData(NcdTool.Tejun.Mach.Nmgz, 0, 0);
				shtData.ShokiSet(0, this.Tols.ToArray());
				while (noMatchTol.Count > 0) {
					noMatchTol[0].matchT = shtData.KohoSheetSet(noMatchTol[0], null, null, this.Tols.ToArray());
					noMatchTol.RemoveAt(0);
				}
			}

			// 工具番号順にソート（無くてもよい）
			NcdTool.Tool.Tol.sortMethod = 0;
			this.m_tols.Sort();

			/* 移動
			// 工具単位の分割情報のセット add in 2014/10/31
			foreach (NcdTool.NcName.NcNam ncnam in NcdTool.Tejun.NcList.ncNamsTS(this.tolstName))
				ncnam.bunkData = new NcdTool.TMatch.BunkData(ncnam);
			*/

			// 新マッチングの工具への反映
			foreach (NcdTool.Tool.Tol tol in this.Tols) tol.Set_Match();

			// *******************************************************************
			// 新たにここで非標準のツールセットを設定する（工具には影響させない）
			// *******************************************************************
			foreach (NcdTool.Tool.Tol tol in this.Tols) {
				if (tol.matchT == null) continue;
				if (tol.matchT.Tset.ToutMatch(tol.matchT.Umax)) continue;
				// 上記最小突出し量が標準より長い場合は、非標準のツールセットを作成する
				tol.matchT.SetTemporary(tol.matchT.Umax);
			}

			// ////////////////////////
			// 最終の切削条件を計算する
			// ////////////////////////
			NcdTool.NcName.NcNam.SetCutting(this.TolstName);

			return;
		}
	}
}