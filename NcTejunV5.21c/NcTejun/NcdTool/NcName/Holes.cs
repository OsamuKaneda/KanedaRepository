using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using CamUtil.LCode;

namespace NcdTool.NcName
{
	/// <summary>
	/// ＮＣデータ内の複数の工具を使用して加工する穴加工情報を作成する。
	/// 一般加工機で複数工具を使用して加工するＮＣデータをガンドリル加工機用ＮＣデータに変換する場合に使用する
	/// </summary>
	class Holes
	{
		/// <summary>出力ファイル名</summary>
		public string OutName { get { return "temp_" + ncnam.nnam; } }

		/// <summary>ＮＣデータ情報</summary>
		NcName.NcNam ncnam;

		/// <summary>孔径単位の孔情報リスト</summary>
		public List<ToolD> tcntList;
		/// <summary>孔径単位の孔情報[不変]</summary>
		public class ToolD
		{
			/// <summary>工具連番</summary>
			public readonly int tcnt;
			/// <summary>イニシャル点高さ</summary>
			public readonly double init;
			/// <summary>Ｒ点高さ</summary>
			public readonly double rten;
			/// <summary>最小深さの送り速度</summary>
			public readonly double feed;
			/// <summary>最大深さの工具径</summary>
			public readonly double diam;

			public ToolD(int tcnt, double init, double rten, double feed, double diam) {
				this.tcnt = tcnt;
				this.init = init;
				this.rten = rten;
				this.feed = feed;
				this.diam = diam;
			}
			public ToolD InitMax(double p_init) { return new ToolD(tcnt, Math.Max(init, p_init), rten, feed, diam); }
			public ToolD RtenMax(double p_rten) { return new ToolD(tcnt, init, Math.Max(rten, p_rten), feed, diam); }
		}

		/// <summary>穴情報のリスト</summary>
		private List<Hole> holeList;
		/// <summary>個々の穴情報</summary>
		private class Hole
		{
			/// <summary>工具径が等しいかチェック</summary>
			static public bool DiamSame(Hole h1, Hole h2) {
				if (Math.Abs(h1.diam - h2.diam) <= 0.01) return true; else return false;
			}
			/// <summary>工具径 h1.diam > h2.diam でイニシャル点、Ｒ点が異なる場合、チェック項目表示</summary>
			static public void Chek(Hole h1, Hole h2) {
				if (h1.diam - h2.diam <= 0.01) return;
				if ((h1.init - 0.01 > h2.init || h1.rten - 0.01 > h2.rten)) {
					string F = "000.00";
					System.Windows.Forms.MessageBox.Show(
						$"同一穴位置の異なる穴径をチェックしてください。 X={h2.xyz.X.ToString()} Y={h2.xyz.Y.ToString()}\n" +
						$"Z={h1.xyz.Z.ToString(F)} D={h1.diam.ToString("00.00")} init={h1.init.ToString(F)} rten={h1.rten.ToString(F)}\n" +
						$"Z={h2.xyz.Z.ToString(F)} D={h2.diam.ToString("00.00")} init={h2.init.ToString(F)} rten={h2.rten.ToString(F)}");
				}
			}



			/// <summary>工具連番のリスト</summary>
			public List<int> tcnt;

			/// <summary>加工位置と深さ</summary>
			public CamUtil.Vector3 xyz;
			/// <summary>イニシャル点高さ</summary>
			private double init;
			/// <summary>Ｒ点高さ</summary>
			private double rten;

			/// <summary>最小深さの送り速度</summary>
			private readonly double feed;

			/// <summary>最大深さのＮＣデータ行番号</summary>
			public long lineNo;
			/// <summary>最大深さの工具径</summary>
			private double diam;

			public Hole(NcLineCode txtd, double r, double z, double f, double d) {
				this.tcnt = new List<int>() { txtd.Tcnt };

				this.xyz = new CamUtil.Vector3(txtd.Xyzsf.X, txtd.Xyzsf.Y, z);
				this.init = txtd.Xyzsf.Z;
				this.rten = r;

				this.lineNo = txtd.LnumN;
				this.feed = f;
				this.diam = d;
				return;
			}

			/// <summary>
			/// より深い穴、同一穴位置、同一穴径の場合にtcnt, lineNo を更新する
			/// </summary>
			/// <param name="anan"></param>
			public void Add(Hole anan) {
				if (anan.xyz.Z > xyz.Z) return;

				if (Math.Abs(this.diam - anan.diam) > 0.01) {
					throw new Exception("リストのRemoveとAddで対応したため不要");
				}
				this.tcnt.Add(anan.tcnt[0]);
				this.xyz = anan.xyz;
				this.lineNo = anan.lineNo;
				this.diam = anan.diam;
				return;
			}

			/// <summary>
			/// 工具情報を作成する
			/// </summary>
			public ToolD CreateToolD() {
				return new ToolD(tcnt[0], init, rten, feed, diam);
			}
			/// <summary>
			/// 工具情報を更新する
			/// </summary>
			/// <param name="tool"></param>
			public void SetToolD(ToolD tool) {
				if (tool.tcnt != this.tcnt[0]) throw new Exception("qjfrbqhre");
				tool = tool.InitMax(this.init);
				tool = tool.RtenMax(this.rten);
				if (tool.feed != this.feed) throw new Exception("qjfrbqhre");
				if (tool.diam != this.diam) throw new Exception("qjfrbqhre");
			}
		}

		/// <summary>
		/// 唯一のコンストラクタ
		/// </summary>
		/// <param name="p_ncnam">ＮＣデータ情報</param>
		public Holes(NcName.NcNam p_ncnam) {
			this.ncnam = p_ncnam;
			this.holeList = new List<Hole>();
			this.tcntList = new List<ToolD>();

			using (StreamReader sr = new StreamReader(ncnam.Ncdata.fulnamePC)) {
				NcLineCode txtd = new NcLineCode(ncnam.Ncdata.ncInfo.xmlD.NcClrPlaneList, Tejun.BaseNcForm, CamUtil.LCode.NcLineCode.GeneralDigit, false, true);
				string ddat;
				NcSIM.RegForm regf = new NcSIM.RegForm(ncnam);

				Hole hole, anan;

				while (!sr.EndOfStream) {
					ddat = regf.Conv(sr.ReadLine());
					if (ddat == null) continue;

					txtd.NextLine(ddat);
					if (txtd.Tcnt < 0) continue;
					//if (ncnam.tdat[txtd.tcnt].tsetCHG.tset_name == null) continue;
					if (ncnam.Tdat[txtd.Tcnt].TsetCHK == false) continue;

					// ///////////////////////////////////////////////////////////////////
					// 穴あけ動作がある場合、同一穴位置の穴リストであるHoleListを更新する
					// ///////////////////////////////////////////////////////////////////
					if ((anan = AnaAke(txtd)) != null) {
						hole = SameXY(anan.xyz);    // 同一穴位置のHoleList内の穴データを抽出
						if (hole != null) {
							if (anan.xyz.Z <= hole.xyz.Z) {
								if (Hole.DiamSame(hole, anan))
									hole.Add(anan);
								else {
									// より深い穴の径が大きい場合、上面から加工するＮＣデータである → 孔加工を初期化
									// より深い穴の径が小さい場合、ザグリありでその小さい穴の上面から加工する → 孔加工を初期化
									holeList.Remove(hole);
									holeList.Add(anan);
									// チェック表示
									Hole.Chek(hole, anan);
								}
							}
						}
						else {
							holeList.Add(anan);
						}
					}
				}
			}

			// ///////////////////////////////////////////////////////////////
			// ＮＣデータを出力する（穴リストの最初の工具）工具連番を抽出する
			// ///////////////////////////////////////////////////////////////
			foreach (Hole htmp in holeList) SettcntList(htmp);

			// ////////////////////////////////////////
			// 使用しない工具の設定（toolset==null）
			// ////////////////////////////////////////
			List<int> ilist = new List<int>();
			foreach (ToolD tool in this.tcntList) ilist.Add(tool.tcnt);
			NcName.Kogu.DelTool(ncnam, ilist);

			return;
		}

		/// <summary>
		/// 最初に穴を加工する工具連番を抽出する（tcnt[0]は最初のショート穴）
		/// </summary>
		/// <param name="hole"></param>
		private void SettcntList(Hole hole) {
			ToolD tool;
			if ((tool = ToolDFind(hole.tcnt[0])) == null) {
				// 新規穴径
				tcntList.Add(hole.CreateToolD());
			}
			else {
				// 穴径チェック
				hole.SetToolD(tool);
			}
		}

		/// <summary>
		/// ＮＣデータ出力の工具情報を出力
		/// </summary>
		/// <param name="tcnt">工具連番</param>
		/// <returns></returns>
		public ToolD ToolDFind(int tcnt) {
			return tcntList.Find(tol => tol.tcnt == tcnt);
		}

		/// <summary>
		/// 穴あけ動作の有無をチェックし、穴加工であればその穴情報を出力する
		/// </summary>
		/// <param name="txtd"></param>
		/// <returns></returns>
		private Hole AnaAke(NcLineCode txtd) {
			if (txtd.G8 != 80) {
				switch (txtd.G8) {
				case 84:
					break;
				default:
					// 固定サイクルはXYZRのいずれかがあると穴あけ動作する
					if (txtd.B_26('X') || txtd.B_26('Y') || txtd.B_26('Z') || txtd.B_26('R'))
						return new Hole(txtd, txtd.G8p['R'].D, txtd.G8p['Z'].D, txtd.G8p['F'].D, ncnam.Tdat[txtd.Tcnt].TsetCHG.Tset_diam);
					break;
				}
			}
			else if (txtd.G6 == 66) {
				switch (txtd.G6p.ProgNo) {
				case 8200:  // 深孔ドリル加工（２段目以降）
				case 8290:	// 超鋼ドリルマクロ
				case 8900:  // 多段孔加工（１段目）
				case 8700:  // 多段孔加工（２段目以降）
				//case 8280:  // ガンドリル
					// G66 は X, Y がないと加工しない
					if (txtd.B_26('X') || txtd.B_26('Y'))
						return new Hole(txtd, txtd.G6p['R'].D, txtd.G6p['Z'].D, txtd.G6p['F'].D, ncnam.Tdat[txtd.Tcnt].TsetCHG.Tset_diam);
					break;
				}
			}
			return null;
		}

		/// <summary>
		/// HoleListからＸＹの座標値が同一のHoleを返す
		/// </summary>
		/// <param name="xyz"></param>
		/// <returns></returns>
		private Hole SameXY(CamUtil.Vector3 xyz) {
			foreach (Hole hole in holeList) {
				if (Math.Abs(hole.xyz.X - xyz.X) < 0.1 && Math.Abs(hole.xyz.Y - xyz.Y) < 0.1) {
					if (Math.Abs(hole.xyz.X - xyz.X) > 0.005 || Math.Abs(hole.xyz.Y - xyz.Y) > 0.005)
						throw new Exception(String.Format("孔の中心座標の誤差エラー X{0:F3} Y{1:F3}   X{2:F3} Y{3:F3}", hole.xyz.X, hole.xyz.Y, xyz.X, xyz.Y));
					return hole;
				}
			}
			return null;
		}

		/// <summary>
		/// ＮＣデータの変換とファイルへの出力
		/// </summary>
		/// <param name="fname">変換の元となるＮＣデータ名（tno!=nullの場合は単一の工具単位ＮＣデータとする）</param>
		/// <param name="tno">null:空のデータも含めすべての工具を出力 else:加工する指定の工具のみを出力</param>
		/// <returns>出力ファイル名</returns>
		public string ConvToFile(string fname, int? tno) {
			string outLine;
			bool g100;
			int tcnt;
			using (StreamReader ncsr = new StreamReader(fname))
			using (StreamWriter ncsw = new StreamWriter(OutName)) {
				// ＮＣデータファイル出力
				tcnt = -1;
				g100 = false;
				while (!ncsr.EndOfStream) {
					outLine = ncsr.ReadLine();
					if (outLine.IndexOf("G100T") == 0) {
						g100 = true;
						tcnt++;
						ncsw.WriteLine(outLine);
						foreach (string stmp in DataSet(tno ?? tcnt)) ncsw.WriteLine(stmp);
					}
					else if (outLine.IndexOf("M98P0006") == 0) {
						g100 = false;
						ncsw.WriteLine(outLine);
					}
					else if (g100 == false) {
						ncsw.WriteLine(outLine);
					}
				}
			}
			return OutName;
		}

		/// <summary>
		/// ガンドリルデータを１工具分出力する
		/// </summary>
		/// <param name="tcnt">工具連番</param>
		public List<string> DataSet(int tcnt) {
			double ZZ;
			List<string> dSet = new List<string>();
			ToolD tool = ToolDFind(tcnt);

			foreach (Hole hole in holeList) {
				if (hole.tcnt[0] != tcnt) continue;

				// ドリルの先端１６０度の補正をする
				ZZ = hole.xyz.Z + tool.diam / 2.0 * Math.Tan(10.0 * Math.PI / 180.0);

				if (dSet.Count == 0) {
					dSet.Add("G90G54");
					dSet.Add(String.Format("G01Z{0:0.00}F1000", tool.init));
					dSet.Add(String.Format("G86X{0:0.00}Y{1:0.00}Z{2:0.00}R{3:0.00#}F{4:0.0}", hole.xyz.X, hole.xyz.Y, ZZ, tool.rten, tool.feed));
				}
				else {
					dSet.Add(String.Format("X{0:0.00}Y{1:0.00}Z{2:0.00}", hole.xyz.X, hole.xyz.Y, ZZ));
				}
			}
			if (dSet.Count > 0) {
				dSet.Add("G80");
				dSet.Add(String.Format("G00Z{0:0.00}", tool.init));
			}
			return dSet;
		}

		public override string ToString() {
			string ss = "";
			int inum;
			//double? ds, df;
			double feed, dep0, dep1;
			foreach (ToolD tool in tcntList) {
				inum = 0;
				dep0 = -9999.9;
				dep1 = +9999.9;
				feed = ncnam.Tdat[tool.tcnt].CutFeedRate();
				foreach (Hole hole in holeList) {
					if (hole.tcnt[0] != tool.tcnt) continue;
					if (dep0 < hole.xyz.Z) dep0 = hole.xyz.Z;
					if (dep1 > hole.xyz.Z) dep1 = hole.xyz.Z;
					inum++;
				}
				if (ss.Length > 0) ss += "\r\n";
				ss += String.Format("  孔径 : {0,5:F2}  I点 : {1,3:F0}  R点 : {2,3:F0}  送り速度 : {3,3:F0}  孔数 : {4,3:D}  孔深さ : {5,7:F2} ～ {6,7:F2}",
					tool.diam, tool.init, tool.rten, feed, inum, dep0, dep1);
				if (feed > 50.0)
					ss += "\r\n＝＝＝＝＝ 注意！送り速度が標準値より大きい ＝＝＝＝＝";
			}
			return ss;
		}

		/// <summary>
		/// 指定工具連番（０から）の穴数を出力する
		/// </summary>
		/// <param name="tcnt"></param>
		/// <returns></returns>
		public int HoleNo(int tcnt) {
			int inum = 0;
			foreach (Hole hole in holeList) if (hole.tcnt[0] == tcnt) inum++;
			return inum;
		}
	}
}
