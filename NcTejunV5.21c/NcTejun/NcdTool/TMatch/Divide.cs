using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using CamUtil.LCode;

namespace NcdTool.TMatch
{
	/// <summary>
	/// 工具の寿命分割情報を作成
	/// </summary>
	class Divide
	{
		/// <summary>分割最小寿命長</summary>
		public const double ClMin = 0.90;
		/// <summary>分割最大寿命長</summary>
		public const double ClMax = 1.05;
		/// <summary>分割エラー寿命長</summary>
		public const double ClErr = 1.20;
		/// <summary>分割確定位置</summary>
		public const double ClSet = 0.995;

		/// <summary>
		/// 分割情報
		/// </summary>
		public readonly struct DivData
		{
			/// <summary>工程開始番号</summary>
			public readonly int kotStt;
			/// <summary>工程終了番号。分割なしの場合はInt32.MaxValue</summary>
			public readonly int kotEnd;
			/// <summary>消耗割合％</summary>
			public double Consumption { get { return ncdist.G01 / lifemax * 100; } }

			/// <summary>ＣＬ内の分割行位置。ＣＬ内分割がない場合はNULL</summary>
			public readonly int? divPoint;

			/// <summary>加工長・加工時間ストラクチャー</summary>
			public readonly NcLineCode.NcDist ncdist;
			/// <summary>工具寿命長(mm)</summary>
			public readonly double lifemax;

			/// <summary>
			/// 分割しない場合
			/// </summary>
			/// <param name="skog"></param>
			/// <param name="passlength">加工長の情報。nullの場合はＸＭＬ情報より作成する</param>
			public DivData(NcName.Kogu skog, NcLineCode.NcDist? passlength) {
				this.kotStt = 0;
				this.kotEnd = Int32.MaxValue;	// 分割なしの場合のチェックに使用している
				if (passlength == null)
					this.ncdist = new NcLineCode.NcDist(skog.Tld.XmlT.FEEDR, skog.Tld.XmlT, Tejun.Mach.Rapx);
				else
					this.ncdist = passlength.Value;
				this.lifemax = skog.Life_max;
				this.divPoint = null;
			}
			/// <summary>
			/// 分割した場合の設定
			/// </summary>
			/// <param name="p_kotStt"></param>
			/// <param name="p_kotEnd"></param>
			/// <param name="p_ncdist"></param>
			/// <param name="p_lifemax"></param>
			/// <param name="p_divPoint"></param>
			public DivData(int p_kotStt, int p_kotEnd, NcLineCode.NcDist p_ncdist, double p_lifemax, int? p_divPoint) {
				this.kotStt = p_kotStt;
				this.kotEnd = p_kotEnd;
				this.ncdist = p_ncdist;
				this.lifemax = p_lifemax;
				this.divPoint = p_divPoint;
			}
		}







		/// <summary>分割情報</summary>
		public List<DivData> divData;

		/// <summary>
		/// コンストラクタ１　分割しない工具単位ＮＣデータ用
		/// </summary>
		/// <param name="skog"></param>
		public Divide(NcName.Kogu skog) {
			divData = new List<DivData>();
			if (Tejun.Ncspeed && skog.Tld.XmlT.parent.CamDimension == 2) {
				// ///////////////////////////////////////////////////////////
				// ncspeedがG91を認識しないことによる加工長の異常値を修正する
				// ///////////////////////////////////////////////////////////
				NcLineCode.NcDist passLength = new NcLineCode.NcDist(skog.Tld.XmlT.FEEDR, skog.Tld.XmlT.MachiningAxisList);
				using (StreamReader sr = new StreamReader(skog.Parent.Ncdata.fulnamePC)) {
					NcLineCode txtd = new NcLineCode(skog.Parent.Ncdata.ncInfo.xmlD.NcClrPlaneList, Tejun.BaseNcForm, NcLineCode.GeneralDigit, false, true);
					NcSIM.RegForm regf = new NcSIM.RegForm(skog.Parent);
					string ddat;
					while (!sr.EndOfStream) {
						ddat = regf.Conv(sr.ReadLine());
						if (ddat != null) {
							txtd.NextLine(ddat);
							if (txtd.Tcnt == 0) passLength.PassLength(txtd);
						}
					}
				}
				divData.Add(new DivData(skog, passLength));

				// ＮＣＳＰＥＥＤの計算誤差が大きいものを表示。マクロ作成などにより誤差を減らせるか（誤差１５分以上、かつ１．２５倍以上の場合）
				double G01 = skog.Tld.XmlT.NCTIM;	// Ｇ０１加工時間
				if (Math.Abs(G01 - divData[0].ncdist.FeedTime) > 15.0 && Math.Abs(Math.Log10(G01 / divData[0].ncdist.FeedTime)) > Math.Log10(1.25)) {
					string ss = String.Format("{0:yyyy/MM/dd HH:mm} {1,-8} {2,-14} JUN={3,3:d} NCSPEED_G01TIM={4:f0} NCDIST_G01TIME={5:f0}",
						DateTime.Now, Tejun.TejunName, skog.Parent.nnam, skog.kakoJun, G01, divData[0].ncdist.FeedTime);
					if (CamUtil.ProgVersion.Debug) System.Windows.Forms.MessageBox.Show("ＮＣＳＰＥＥＤの計算誤差が大きいものを表示 " + ss);
					//StreamWriter sw = new StreamWriter(CamUtil.ServerPC.SvrFldrC + "Log_BUNKATSU.txt", true);
					//sw.WriteLine(ss);
					//sw.Close();
				}
			}
			else
				divData.Add(new DivData(skog, null));
		}

		/// <summary>
		/// コンストラクタ２　分割する工具単位ＮＣデータ用
		/// </summary>
		/// <remarks>
		/// １工程で工具寿命のClMax倍以下の場合はClMaxを超えないように複数工程をまとめて分割する。
		/// １工程で工具寿命のClMax倍を超える場合は工程内で工具寿命ごとに分割する（同時５軸加工を除く）。
		/// １工程で工具寿命のClErr倍を超える場合はエラーとする（同時５軸加工）。
		/// </remarks>
		/// <param name="skog"></param>
		/// <param name="doji5"></param>
		public Divide(NcName.Kogu skog, bool doji5) {
			divData = new List<DivData>();
			NcLineCode.NcDist dtmp;

			if (skog.KoteiList.Length < 1) throw new Exception("aefrbqfbqhfrbqherf");

			// 分割データ作成
			int istt = 0, iend;
			NcLineCode.NcDist dkot = new NcLineCode.NcDist(skog.Tld.XmlT.FEEDR, skog.Tld.XmlT.MachiningAxisList);
			while (istt < skog.KoteiList.Length) {
				for (iend = istt; iend < skog.KoteiList.Length; iend++) {
					dtmp = skog.KoteiList[iend];

					if (dkot.G01 > 0.0 && dkot.G01 + dtmp.G01 > ClMax * skog.Life_max) {
						// 工程間での分割
						divData.Add(new DivData(istt, iend - 1, dkot, skog.Life_max, null));
						dkot = new NcLineCode.NcDist(skog.Tld.XmlT.FEEDR, skog.Tld.XmlT.MachiningAxisList);
						istt = iend;	// 今の工程から
						break;
					}
					if (CamUtil.ProgVersion.NotTrialVersion1) {
						if (doji5) CamUtil.LogOut.CheckCount("Divide 156", false, "同時５軸フラッグエラー");
						if (dtmp.G01 > ClMax * skog.Life_max) {
							// 工程内で複数に分割し端数の加工長を出力
							dkot = Bun(skog, istt, iend, dtmp.G01 + dkot.G01, dkot);
							continue;	// 次の工程へ
						}
					}
					else {
						if (doji5) {
							CamUtil.LogOut.CheckCount("Divide 165", false, "部品加工の同時５軸の分割実行");
							if (dtmp.G01 > ClErr * skog.Life_max)
								throw new Exception(
									String.Format("同時５軸加工での工具寿命分割エラー。" +
									"１工程で加工長が{0:f0}mmであり、工具寿命{1:f0}mmの{2:f2}倍を超えています。 NC = {3} TOOL = {4}",
									dtmp.G01, skog.Life_max, ClErr, skog.Parent.nnam, skog.TsetCHG.Tset_name));
						}
						else {
							if (dtmp.G01 > ClMax * skog.Life_max) {
								// 工程内で複数に分割し端数の加工長を出力
								dkot = Bun(skog, istt, iend, dtmp.G01 + dkot.G01, dkot);
								continue;	// 工程内で分割した余りdkotを残し、次の工程へ
							}
						}
					}
					dkot.Add(dtmp);
				}
				// 最後の出力
				if (iend == skog.KoteiList.Length) {
					if (istt == iend) istt = iend - 1;
					divData.Add(new DivData(istt, iend - 1, dkot, skog.Life_max, null));
					istt = iend;
				}
			}

			// ＣＬ内分割の場合は結果をログに保存
			if (divData.Count > 1) {
				for (int ii = 0; ii < divData.Count; ii++)
					if (divData[ii].divPoint != null)
						CamUtil.LogOut.CheckOutput(CamUtil.LogOut.FNAM.BUHINBUNK, Tejun.TejunName, Tejun.Mach.name,
							$"{skog.Parent.nnam,-14} JUN={skog.kakoJun,3:d} STT={divData[ii].kotStt,3:d} END={divData[ii].kotStt,3:d} LINE={divData[ii].divPoint,6:d}");
			}
			return;
		}

		/// <summary>
		/// コンストラクタ３　ガンドリル用ＮＣデータ用
		/// </summary>
		/// <param name="ncnam"></param>
		/// <param name="skog"></param>
		public Divide(NcName.NcNam ncnam, NcName.Kogu skog) {
			divData = new List<DivData>();
			if (skog.Tld.SetJun - 1 == ncnam.Holes.tcntList[0].tcnt) {
				ncnam.Holes.ConvToFile(ncnam.Ncdata.fulnamePC, null);
			}
			NcLineCode.NcDist passLength = new NcLineCode.NcDist(skog.Tld.XmlT.FEEDR, skog.Tld.XmlT.MachiningAxisList);
			using (StreamReader sr = new StreamReader(ncnam.Holes.OutName)) {
				NcLineCode txtd = new NcLineCode(skog.Parent.Ncdata.ncInfo.xmlD.NcClrPlaneList, Tejun.BaseNcForm, NcLineCode.GeneralDigit, false, true);
				string ddat;
				while (!sr.EndOfStream) {
					ddat = sr.ReadLine();
					if (ddat != null) {
						txtd.NextLine(ddat);
						if (txtd.Tcnt == skog.Tld.SetJun - 1) passLength.PassLength(txtd);
					}
				}
			}
			if (skog.Tld.SetJun - 1 == ncnam.Holes.tcntList[ncnam.Holes.tcntList.Count - 1].tcnt)
				File.Delete(ncnam.Holes.OutName);

			// 孔１つに付３０秒の待ち時間を追加する
			passLength.AddDwell(30.0 * ncnam.Holes.HoleNo(skog.Tld.SetJun - 1));

			divData.Add(new DivData(skog, passLength));
			return;
		}

		/// <summary>
		/// 工程内での分割位置の設定処理
		/// </summary>
		/// <param name="skog"></param>
		/// <param name="istt">工程開始番号</param>
		/// <param name="kcnt">工程終了番号</param>
		/// <param name="cllen">残りの加工長さ＋分割する工程の加工長</param>
		/// <param name="nokori">前の工程での残り加工長</param>
		/// <returns>前の工程で最後の工具交換後の残り加工長</returns>
		private NcLineCode.NcDist Bun(NcName.Kogu skog, int istt, int kcnt, double cllen, NcLineCode.NcDist nokori) {
			int know;
			NcLineCode.NcDist passLength;
			double lifemax = skog.Life_max;

			// tcnt:分割する工具の加工順（＞＝０）
			int tcnt;
			tcnt = skog.TNo;
			if (tcnt >= skog.Parent.Itdat)
				throw new Exception("qwefbqhrebfh");

			// 出力ＮＣデータの情報
			NcLineCode txtd = new NcLineCode(skog.Parent.Ncdata.ncInfo.xmlD.NcClrPlaneList, Tejun.BaseNcForm, NcLineCode.GeneralDigit, false, true);

			// 加工長の情報
			passLength = new NcLineCode.NcDist(skog.Tld.XmlT.FEEDR, skog.Tld.XmlT.MachiningAxisList);
			passLength.Add(nokori);

			// 分割数
			int bsuu = (int)Math.Ceiling(cllen / lifemax - (ClMax - 1.0));
			int bnow = 1;

			know = 0;

			//行数と分割までの加工長と送りモードの保存
			long BUNlno = 0; NcLineCode.NcDist BUNlen = new NcLineCode.NcDist(); int BUNg1 = 1;

			using (StreamReader sr = new StreamReader(skog.Parent.Ncdata.fulnamePC)) {
				while (!sr.EndOfStream) {
					txtd.NextLine(sr.ReadLine());
					if (txtd.B_g100) {
						if (txtd.Tcnt > tcnt) break;
					}
					if (txtd.Tcnt < tcnt) continue;

					// M98P9017（ＣＬの切れ目）
					// M98P9306（工具終了処理）
					if (txtd.NcLine.IndexOf("M98P9017") >= 0 || txtd.NcLine.IndexOf("M98P9306") >= 0) {
						know++;
						if (know > kcnt) {
							// 最後の分割位置がなく工程終了位置まで来た場合
							if (bnow < bsuu) {
								Check(skog, passLength.G01);
								bsuu = bnow;
							}
							break;
						}
					}
					if (know < kcnt) continue;

					if (bnow < bsuu && passLength.G01 > ClMin * lifemax) {

						// 分離する場所と優先順位
						bool bunri = false;
						// １．Ｇ００での移動（送り速度が最大送り速度80000mm/min）
						if (txtd.G1 == 0 || txtd.Xyzsf.Fi == NcLineCode.RF_5AXIS) bunri = true;
						// ２．送り速度の指定があり、前行が切削送りでその送りが切削送りでない場合
						if (BUNg1 == 1 && txtd.B_26('F') && txtd.Xyzsf.Fi != skog.Tld.XmlT.FEEDR && txtd.Xyzsf.PreFi == skog.Tld.XmlT.FEEDR) bunri = true;

						if (bunri && txtd.B_g8 == false && txtd.G8 == 80 && txtd.B_g6 == false && txtd.G6 == 67) {
							if (BUNlno == 0 || Math.Abs(passLength.G01 - lifemax) < Math.Abs(BUNlen.G01 - lifemax)) {
								BUNlen = passLength;
								BUNlno = txtd.LnumN;
								if (txtd.G1 == 0 || txtd.Xyzsf.Fi == NcLineCode.RF_5AXIS)
									BUNg1 = 0;
								else
									BUNg1 = 1;
							}
							// 工具交換位置設定
							if (passLength.G01 > ClSet * lifemax) {
								Check(skog, BUNlen.G01);
								divData.Add(new DivData(bnow == 1 ? istt : kcnt, kcnt, BUNlen, lifemax, (int)BUNlno));
								bnow++;
								BUNlno = 0; BUNlen = new NcLineCode.NcDist(); BUNg1 = 1;
								passLength = new NcLineCode.NcDist(skog.Tld.XmlT.FEEDR, skog.Tld.XmlT.MachiningAxisList);
							}
						}
					}
					passLength.PassLength(txtd);
				}
			}

			// 結果として最後の分割位置がＣＬの終わりとなった場合 2015/07/02
			if (passLength.G01 < lifemax * 0.005) {
				passLength.Add(divData[divData.Count - 1].ncdist);
				divData.RemoveAt(divData.Count - 1);
			}

			return passLength;
		}

		/// <summary>
		/// エラーのチェック
		/// </summary>
		/// <param name="skog"></param>
		/// <param name="leng">加工長</param>
		private void Check(NcName.Kogu skog, double leng) {
			if (leng / skog.Life_max > 1.05) {
				System.Windows.Forms.MessageBox.Show(
					$"{skog.Parent.nnam} {skog.TsetCHG.Tset_name} はＣＬ分割で寿命超過しました（ 寿命に対する加工長の割合 = {(leng / skog.Life_max * 100).ToString("0.00")}% ）");
			}
		}
	}
}
