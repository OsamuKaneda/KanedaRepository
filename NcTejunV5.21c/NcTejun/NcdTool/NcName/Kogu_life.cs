using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using CamUtil.LCode;

namespace NcdTool.NcName
{
	partial class Kogu
	{
		// //////////////////////////////////////////
		// 工具マッチング後のみに使用可能な正式な値
		// //////////////////////////////////////////

		/// <summary>
		/// ガンドリルで使用しない工具を出力対象から外す
		/// </summary>
		/// <param name="ncnam"></param>
		/// <param name="tcntList"></param>
		static public void DelTool(NcNam ncnam, List<int> tcntList) {
			for (int ii = 0; ii < ncnam.Itdat; ii++) if (!tcntList.Contains(ii)) ncnam.Tdat[ii].m_tsetCHG = null;
		}

		/// <summary>マッチング工具を考慮した正式な加工条件をセットする</summary>
		static public void SetCutting(string tolstName) {
			int err = 0;
			foreach (NcNam ncnam in NcdTool.Tejun.NcList.NcNamsTS(tolstName)) {
				foreach (Kogu skog in ncnam.Tdat)
					// ツールセット未設定の場合は対象外（加工機不整合など）
					if (skog.TsetCHK) {
						//err += skog.RateSetA(NcdTool.Tejun.mach);
						skog.sfRate = new SFRate(skog, NcdTool.Tejun.Mach);
						// 「送り速度最適化」であるが、両立しない「任意指示」が設定されている場合に１を返す
						err += (skog.Tld.XmlT.SmFDC && skog.Tld.XmlT.OPTION()) ? 1 : 0;
					}
			}
			if (err > 0)
				System.Windows.Forms.MessageBox.Show("「送り速度最適化」を実行したＮＣデータに設定されている" + err.ToString() + "個の「回転数・送り速度の任意指示」は無視しました");
		}

		// //////////////
		// 以上、static
		// //////////////




		/// <summary>計算された回転数</summary>
		public int CutSpinRate() { return (int)CutSpinRate(Math.Round(Tld.XmlT.SPIND)); }
		/// <summary>計算された回転数</summary>
		/// <param name="ispin">元の（ＮＣデータ内）回転数</param>
		/// <returns>加工に使用する回転数。SPINタイプを考慮済み</returns>
		public double CutSpinRate(double ispin) {
			double spn = Math.Round(ispin * sfRate.SrataN);
			if (TsetCHG.Hsp == true) {
				if (spn > Tejun.Mach.Hisp + 0.1) throw new Exception("PROGRAM ERROR in max S\n");
				if (spn < Tejun.Mach.Hispmin - 0.1 && spn > 0.0) throw new Exception("PROGRAM ERROR in max S\n");
			}
			else {
				if (spn > Tejun.Mach.Smax + 0.1) throw new Exception("PROGRAM ERROR in max S\n");
				if (spn < Tejun.Mach.Smin - 0.1 && spn > 0.0) throw new Exception("PROGRAM ERROR in max S\n");
			}
			return spn;
		}

		/// <summary>切削送り速度を計算する（fchi[0]の変換値）</summary>
		/// <returns>実送り速度</returns>
		public int CutFeedRate() { return (int)CutFeedRate(Math.Round(Tld.XmlT.FEEDR)); }
		/// <summary>ＮＣデータ出力する切削送り速度を計算する</summary>
		/// <param name="ifeed">元ＮＣデータの送り速度</param>
		/// <returns>実際に加工する送り速度</returns>
		public double CutFeedRate(double ifeed) {
			// m_fratfはm_frataと分離 2015/09/08
			double newfeed = Math.Round(ifeed * sfRate.FrataN, MidpointRounding.AwayFromZero);
			switch (Tejun.Mach.ID) {
			case CamUtil.Machine.MachID.D500:
			case CamUtil.Machine.MachID.LineaM:
				break;
			default:
				if (newfeed < 10) newfeed = 10;
				break;
			}
			if (newfeed > Tejun.Mach.Fmax) newfeed = Math.Round(Tejun.Mach.Fmax);
			return newfeed;
		}
		/// <returns>送り速度自動制御時に設定する送り速度（fratfN のみ）</returns>
		/// <param name="ifeed">元ＮＣデータの送り速度</param>
		/// <returns>実際に加工する送り速度</returns>
		public double CutFeedRateMachine(double ifeed) {
			double newfeed = Math.Round(ifeed * sfRate.fratfN, MidpointRounding.AwayFromZero);
			switch (Tejun.Mach.ID) {
			case CamUtil.Machine.MachID.D500:
			case CamUtil.Machine.MachID.LineaM:
				break;
			default:
				if (newfeed < 10) newfeed = 10;
				break;
			}
			if (newfeed > Tejun.Mach.Fmax) newfeed = Math.Round(Tejun.Mach.Fmax);
			return newfeed;
		}
		/// <summary>元のＮＣデータの加工時間にかける比率（送り速度最適化以外は送り速度比率の逆数）</summary>
		/// <remarks>
		/// 送り速度最適化の場合、切削部は加工機性能による送り速度の比率（fratfN）、アプローチ部など切削部以外には総合送り速度の比率（frataN * fratfN）がかかる。
		/// 一般的に切削部の方が加工時間が長いため、送り速度最適化の場合は加工機性能による送り速度の比率（fratfN）をかける。
		/// </remarks>
		public double CutTimeRate { get { return Tld.XmlT.SmFDC ? 1.0 / sfRate.fratfN : (1.0 / sfRate.FrataN); } }

		/// <summary>工具消耗率（単位％）</summary>
		public double Consumption { get { return Tld.XmlT.NCLEN / Life_max * 100.0; } }
		/// <summary>工具寿命_任意指示・材質考慮（単位mm）</summary>
		public double Life_max { get { return 1000.0 * Tld.LifeDB * (Tld.XmlT.OPTLF ?? ((TsetCHG.life ?? 1.0) * tsetMAT.lratc)); } }
		/// <summary>加工機情報の孔加工リトラクト速度に、工具種類・材質の影響を考慮しかける比率。</summary>
		public double retract_rate { get { return tsetMAT.aretc; } }







		/// <summary>総合回転数比率が1.0でない場合true</summary>
		public bool ChgSpin { get { return Math.Abs(sfRate.SrataN - 1.0) > 0.001; } }
		/// <summary>総合送り速度比率が1.0でない場合true</summary>
		public bool ChgFeed { get { return Math.Abs(sfRate.FrataN - 1.0) > 0.001; } }
		/// <summary>加工機性能送り速度比率が1.0でない場合true</summary>
		public bool ChgFeedMachine { get { return Math.Abs(sfRate.fratfN - 1.0) > 0.001; } }

		/// <summary>ツールセットの決定に関連する工具消耗率、回転速度、送り速度の比率をあらわす</summary>
		public TSetCHG TsetCHG { get { return m_tsetCHG.Value; } }
		private TSetCHG? m_tsetCHG;
		/// <summary>ツールセット・材質に関連する工具消耗率、回転速度、送り速度の比率をあらわす</summary>
		private readonly TSetMAT tsetMAT;
		/// <summary>総合回転速度、総合送り速度、マシンパワー減速の比率をあらわす</summary>
		private SFRate sfRate;

		/// <summary>
		/// 工具マッチング後の正式な切削条件（現在は工具マッチング前の情報で計算可能になっている）
		/// </summary>
		private readonly struct SFRate
		{
			/// <summary>総合回転数比率</summary>
			public readonly double? srata;
			/// <summary>総合送り速度比率</summary>
			public readonly double? frata;
			/// <summary>マシンパワー限界による減速比率</summary>
			public readonly double fratf;

			// /// <summary>エラーの数</summary>
			// public readonly int errCount;

			/// <summary>総合回転数比率（加工機性能を含む）</summary>
			public double SrataN { get { return sratbN * sratfN; } }
			/// <summary>総合送り速度比率（加工機性能を含む）</summary>
			public double FrataN { get { return fratbN * fratfN; } }

			/// <summary>総合回転数比率（加工機性能を除く）</summary>
			public readonly double sratbN;
			/// <summary>総合送り速度比率（加工機性能を除く）</summary>
			public readonly double fratbN;
			/// <summary>加工機性能による回転数比率</summary>
			public readonly double sratfN;
			/// <summary>加工機性能による送り速度比率</summary>
			public readonly double fratfN;

			/// <summary>
			/// 突出し量などによる切削条件比率（srata,frata,fratf）をセットする
			/// </summary>
			/// <param name="skog">ＮＣ工具情報</param>
			/// <param name="mach">加工機</param>
			public SFRate(Kogu skog, Mcn1 mach) {
				// smin		mach min  spind
				// smax		mach max  spind
				// srat1	mach rate spind
				// sratt	tejn rate spind
				// srath	ttsk rate spind
				// srats	hld  rate spind
				// sratc	material  spind
				// srate	ninnisiji spind

				// fmax		mach max  feed
				// frat1	mach rate feed
				// fratt	tejn rate feed
				// frath	ttsk rate feed
				// frats	hld  rate feed
				// fratc	material  feed
				// frate	ninnisiji feed

				//                  ＳＦ設定 手順書 セット決定 材質 セット持替え 加工機能力
				// 工具単位ＳＦ設定  ○                                           ○
				//         検証なし           ○     ○         ○                ○
				// 最適化なしの検証           ○     ○         ○   ○           ○
				//   送り速度最適化           ○                                  ○
				// 

				double ss, ff;
				//this.errCount = 0;

				if (skog.Parent.Jnno.Nknum != null) {
					srata = skog.Parent.Jnno.Nknum.Tdat[skog.Tld.SetJun - 1].sfRate.srata;
					frata = skog.Parent.Jnno.Nknum.Tdat[skog.Tld.SetJun - 1].sfRate.frata;
					fratf = skog.Parent.Jnno.Nknum.Tdat[skog.Tld.SetJun - 1].sfRate.fratf;

					sratbN = skog.Parent.Jnno.Nknum.Tdat[skog.Tld.SetJun - 1].sfRate.sratbN;
					fratbN = skog.Parent.Jnno.Nknum.Tdat[skog.Tld.SetJun - 1].sfRate.fratbN;
					sratfN = skog.Parent.Jnno.Nknum.Tdat[skog.Tld.SetJun - 1].sfRate.sratfN;
					fratfN = skog.Parent.Jnno.Nknum.Tdat[skog.Tld.SetJun - 1].sfRate.fratfN;
					return;
				}

				ss = skog.Tld.XmlT.SPIND;
				ff = skog.Tld.XmlT.FEEDR;

				// 「送り速度最適化」であるが、「任意指示」が設定されている
				//if (skog.tld.xmlT.smFDC && skog.tld.xmlT.OPTION()) errCount = 1;

				if (!skog.Tld.XmlT.SmFDC && skog.Tld.XmlT.OPTION()) {
					// 「送り速度最適化」でなく、ＰＣの任意指示がある場合、任意指示の値を設定
					skog.Tld.XmlT.OPTION(out double? stmp, out double? ftmp);
					ss *= stmp.Value;
					ff *= ftmp.Value;
				}
				else {
					if (skog.Parent.Ncdata.ncInfo.xmlD.SmNAM != null) {
						// ＮＣＳＰＥＥＤの場合、ＸＭＬ内の総合比率を用いる（これはツールセット、材質の比率を含む）
						ss *= skog.Tld.XmlT.SmSPR;
						ff *= skog.Tld.XmlT.SmFDR;
					}
					else {
						// TSET_CHGでの回転と送りを反映 2015/09/08
						ss *= skog.TsetCHG.spin ?? 1.0;
						ff *= skog.TsetCHG.feed ?? 1.0;
						// 材質の影響を反映
						ss *= skog.tsetMAT.sratc;
						ff *= skog.tsetMAT.fratc;
					}

					// 手順書で指定された比率を反映
					ss *= skog.Parent.nmgt.Sratt;
					ff *= skog.Parent.nmgt.Fratt;
				}

				// 新　結果をsratbN, fratbNに反映
				if (skog.Tld.XmlT.SPIND > 0) sratbN = ss / skog.Tld.XmlT.SPIND; else sratbN = 1.0;
				if (skog.Tld.XmlT.FEEDR > 0) fratbN = ff / skog.Tld.XmlT.FEEDR; else fratbN = 1.0;

				//
				// 加工機の能力を反映
				fratf = 1.0;

				//
				// 加工機の最大送り速度を考慮
				if (ff > mach.Fmax) {
					ss = ss * mach.Fmax / ff;
					ff = mach.Fmax;
				}
				if (skog.TsetCHG.Hsp == false) {
					// 加工機の最小回転数を考慮
					if (ss > 0.0 && ss < mach.Smin) {
						ff = ff * mach.Smin / ss;
						ss = mach.Smin;
						if (ff > mach.Fmax) throw new Exception("qkrgfnwqrefjwqrn");
					}
					// 加工機の最大回転数を考慮
					if (ss > mach.Smax) {
						ff = ff * mach.Smax / ss;
						ss = mach.Smax;
					}
					// 加工機の回転数の飛び値を考慮
					/*
					if (mach.mcn2.spind[0] != 0) throw new Exception("暫定チェックのエラー");
					if (mach.mcn2.spind[0] != 0) {
						for (int jj = 1; jj <= mach.mcn2.spind[0]; jj++)
							if (ss <= (mach.mcn2.spind[jj] + mach.mcn2.spind[jj + 1]) / 2) {
								ff = ff * mach.mcn2.spind[jj] / ss;
								ss = mach.mcn2.spind[jj];
								break;
							}
					}
					*/
				}
				else {
					// 加工機の最小回転数を考慮
					if (ss > 0.0 && ss < mach.Hispmin) {
						ff = ff * mach.Hispmin / ss;
						ss = mach.Hispmin;
						if (ff > mach.Fmax) throw new Exception("qkrgfnwqrefjwqrn");
					}
					// 加工機の最大回転数を考慮
					if (ss > mach.Hisp) {
						ff = ff * mach.Hisp / ss;
						ss = mach.Hisp;
					}
				}

				// 結果をm_srata, m_frataに反映
				if (skog.Tld.XmlT.SPIND > 0) srata = ss / skog.Tld.XmlT.SPIND; else srata = 1.0;
				if (skog.Tld.XmlT.FEEDR > 0) frata = ff / skog.Tld.XmlT.FEEDR; else frata = 1.0;

				// 結果をsratfN, fratfNに反映
				if (skog.Tld.XmlT.SPIND > 0) sratfN = ss / (sratbN * skog.Tld.XmlT.SPIND); else sratfN = 1.0;
				if (skog.Tld.XmlT.FEEDR > 0) fratfN = ff / (fratbN * skog.Tld.XmlT.FEEDR); else fratfN = 1.0;
				if (skog.Tld.XmlT.SPIND > 0 && skog.Tld.XmlT.FEEDR > 0)
					if (Math.Abs(sratfN - fratfN) > 0.0001) throw new Exception("回転数、送り速度比率の計算エラー in SFRate");

				return;
			}
		}

		/// <summary>
		/// ツールセット決定に関連する情報である、
		/// テーパ規格名、ホルダー管理区分、スピンドル仕様と、ToolSetCAMの切削条件より優先する、回転数、送り速度、工具消耗率をあらわす。
		/// </summary>
		public readonly struct TSetCHG
		{
			/// <summary>ツールセット名</summary>
			public string Tset_name { get { return toolset.tset_name; } }
			/// <summary>工具の直径</summary>
			public double Tset_diam { get { return toolset.Diam; } }	// ガンドリルで使用する
			/// <summary>工具の名前</summary>
			public string Tset_tool_name { get { return toolset.ToolName; } }
			/// <summary>工具番号固定（in machine_magazine）限定</summary>
			public bool Tset_FixedNumber { get { return toolset.FixedNumber; } }
			/// <summary>切削条件の調整タイプ</summary>
			public string Tset_cond_type { get { return toolset.CondType; } }
			/// <summary>ケーラム工具種類名</summary>
			public string Tset_cutter_type_caelum { get { return toolset.CutterTypeCaelum; } }
			/// <summary>測定子</summary>
			public bool Tset_probe { get { return toolset.Probe; } }
			/// <summary>工具回転方向</summary>
			public string Tset_m0304 { get { return toolset.M0304; } }


			/// <summary>ツールセットＣＡＭと工作機より計算されたツールセット</summary>
			private readonly CamUtil.ToolSetData.ToolSet toolset;

			/// <summary>
			/// テーパ規格名：A100, A63, BT40, BT50, E50（旧テーパ規格区分：A100_1,A100_2,BT50,A63,BT40）
			/// （ホルダー名が決まってもテーパ規格名は決まらない。例えばA100もBT50も同一ホルダー名）
			/// </summary>
			public readonly string hld_type;
			/// <summary>
			/// ホルダー管理区分(A100_A, A63_A, BT40_A, BT50_A, DMU200 など)
			/// 工場内でホルダ管理する区分でありこの情報とホルダー名で実際に管理するホルダー単位が特定される。
			/// １つの工作機で複数持つことができ、優先順位によってのみどのツールセットが採用されるか決まる。
			/// </summary>
			public readonly string hld_knri;
			/// <summary>高速加工モードのツールセット</summary>
			public bool Hsp { get { return spn_type == "HSP"; } }
			/// <summary>
			/// スピンドル仕様（HSP, STD）
			/// </summary>
			public readonly string spn_type;

			/// <summary>TSetCAMに対する回転数比率</summary>
			public readonly double? spin;
			/// <summary>TSetCAMに対する送り速度比率</summary>
			public readonly double? feed;
			/// <summary>TSetCAMに対する寿命比率</summary>
			public readonly double? life;

			public TSetCHG(NcName.NcDataT tld, string mcnName) {
				string tsetname = ToolSetInfo.TSet_Name(tld.XmlT.SNAME, mcnName, out hld_type, out hld_knri, out spn_type);
				// 工具、ホルダー有無
				if (tsetname == null) {
					throw new Exception(String.Format(
						"ツールセットＣＡＭ名:{0}\r\nはこの加工機で使用する工具とホルダーに対応していません。加工対象から除外します。",
						tld.XmlT.SNAME));
				}
				// ＮＣデータ検証
				if (tld.XmlT.SmTSN != null) {
					if (tsetname != tld.XmlT.SmTSN) {
						throw new Exception(String.Format(
							"ツールセット名:{0}\r\nＮＣデータ検証で使用されたツールセット：{1} と異なります。加工対象から除外します。",
							tsetname, tld.XmlT.SmTSN));
					}
				}

				toolset = new CamUtil.ToolSetData.ToolSet(tsetname);
				double?[] aa = ToolSetInfo.GetNewCond(tld.XmlT.SNAME, hld_type);
				spin = aa[0] / tld.XmlT.SPIND;
				feed = aa[1] / tld.XmlT.FEEDR;
				life = aa[2] / tld.LifeDB;

				// 固定番号限定工具の設定
				if (this.toolset.FixedNumber) {
					if (!Tejun.Mach.Mgrs.Exists(tol => tol.Toolsetname == tsetname)) {
						throw new Exception(String.Format(
							"ツールセット名:{0}\r\nこの加工機（{1}）で使用できないツールセットです。加工対象から除外します。",
							tsetname, mcnName));
					}
				}

				return;
			}
		}

		/// <summary>ツールセット・材質に関連する工具消耗率、回転速度、送り速度の比率をあらわす</summary>
		private readonly struct TSetMAT
		{
			/// <summary>材質によるスピンドル回転数比率(set in sdrget)</summary>
			public readonly double sratc;
			/// <summary>材質による送り速度比率(set in sdrget)</summary>
			public readonly double fratc;
			/// <summary>材質による工具消耗比率(set in sdrget)</summary>
			public readonly double lratc;
			/// <summary>材質と工具種類による孔加工リトラクト速度比率(set in sdrget)</summary>
			public readonly double aretc;

			/// <summary>
			/// 工具寿命関連データをセット（工具情報が必要）。tnSetより移動 in 2010/05/31
			/// （Set m_nclen, m_life_max_base, m_mat_life_rate, m_mat_spin_rate, m_mat_feed_rate）
			/// </summary>
			/// <param name="skog">加工条件を作成するＮＣデータ</param>
			public TSetMAT(NcName.Kogu skog) {
				// /////////////////////////////////////////////////////////////////////////
				// m_nclen : 加工長の設定。加工長は「工具消耗率」の計算に使用する
				// 2010/01/19 追加
				// /////////////////////////////////////////////////////////////////////////
				CamUtil.CamNcD.NcInfo ninfo = skog.Parent.Ncdata.ncInfo;
				if (ninfo.xmlD.bunkatsu == false) {
					if (skog.Tld == null)
						throw new Exception("tldのエラーが発生しました");
					if (ninfo.xmlD[skog.Tld.SetJun - 1].NCLEN != skog.Tld.XmlT.NCLEN)
						throw new Exception("qrfqrwerbfwerhbfh");
					if (ninfo.xmlD[skog.Tld.SetJun - 1].NCLEN != skog.Tld.XmlT.NCLEN)
						throw new Exception("qrfqrwerbfwerhbfh");
				}

				// /////////////////////////////////////////////////////////////////////////
				// m_mat_spin_rate, m_mat_feed_rate, m_mat_life_rate : 型材質による切削条件比率
				// /////////////////////////////////////////////////////////////////////////

				// skog.toolset : m_mat_life_rateを仮設定し寿命計算に使用する
				double[] dummy = ToolSetInfo.GetLifeRate(skog.Parent.nggt.zaisGrp, skog.TsetCHG.Tset_cond_type);
				sratc = dummy[0];
				fratc = dummy[1];
				lratc = dummy[2];
				aretc = dummy[3];
			}
		}
	}
}
