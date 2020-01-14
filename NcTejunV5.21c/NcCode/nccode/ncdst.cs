using System;
using System.Collections.Generic;
using System.Text;

using CamUtil;

namespace NcCode.nccode
{
	/// <summary>
	/// 複数工具を持つＮＣデータ全体の加工長・加工時間の情報を保存します。
	/// </summary>
	internal struct NcDst
	{
		/// <summary>加工長の積算値</summary>
		public double CuttingL {
			get { return data.FeedAll_xyz; }
		}

		/// <summary>ＮＣデータ全体の積算値（加工時間、加工長）</summary>
		private Nctol data;
		/// <summary>工具単位ごとの積算値（加工時間、加工長）のリスト</summary>
		private List<Nctol> tol;

		/// <summary>不明</summary>
		public double Mfeed { get { return m_mfeed; } }
		private double m_sfeed, m_efeed, m_mfeed;
		/// <summary>ＮＣデータの曲がり角度</summary>
		public double Vdeg { get { return m_vdeg; } }
		private double m_vdeg;

		/// <summary>送り区分（0:早送り　1:ＸＹ切削送り　2:Ｚ切削送り）</summary>
		private enum Kub
		{
			/// <summary>早送り</summary>
			rapid = 0,
			/// <summary>切削送りＸＹ</summary>
			feedx = 1,
			/// <summary>切削送りＺ</summary>
			feedz = 2
		}

		/// <summary>
		/// 早送り、XY切削送り、Ｚ切削送りごとの加工長、加工時間を保存します。
		/// </summary>
		private readonly struct Nctol
		{
			/// <summary>早送り</summary>
			private readonly Sub rapid;
			/// <summary>切削送りＸＹ</summary>
			private readonly Sub feedx;
			/// <summary>切削送りＺ</summary>
			private readonly Sub feedz;


			/// <summary>
			/// Kubごとの加工長、加工時間情報
			/// </summary>
			private readonly struct Sub
			{
				/// <summary>出現数</summary>
				public readonly int cnt;
				/// <summary>積算距離</summary>
				public readonly double xyz;
				/// <summary>積算時間</summary>
				public readonly double tim;

				public Sub(int cnt, double xyz, double tim) {
					this.cnt = cnt;
					this.xyz = xyz;
					this.tim = tim;
				}
				private Sub(Sub moto, double xyz, double tim) {
					this = moto;
					this.cnt++;
					this.xyz += xyz;
					this.tim += tim;
				}
				public Sub Add(double xyz, double tim) {
					return new Sub(this, xyz, tim);
				}
			}

			public Nctol(int dummy) {
				rapid = new Sub(0, 0.0, 0.0);
				feedx = new Sub(0, 0.0, 0.0);
				feedz = new Sub(0, 0.0, 0.0);
			}
			public Nctol(Nctol moto, Kub kub, double xyz, double tim) {
				this = moto;
				switch (kub) {
				case Kub.rapid: rapid = rapid.Add(xyz, tim); break;
				case Kub.feedx: feedx = feedx.Add(xyz, tim); break;
				case Kub.feedz: feedz = feedz.Add(xyz, tim); break;
				default: throw new Exception("qwefnqhewr");
				}
			}

			public int Rapid_cnt { get { return rapid.cnt; } }
			public double Rapid_xyz { get { return rapid.xyz; } }
			public double Rapid_tim { get { return rapid.tim; } }

			public int FeedXY_cnt { get { return feedx.cnt; } }
			public double FeedXY_xyz { get { return feedx.xyz; } }
			public double FeedXY_tim { get { return feedx.tim; } }

			public int FeedAll_cnt { get { return feedx.cnt + feedz.cnt; } }
			public double FeedAll_xyz { get { return feedx.xyz + feedz.xyz; } }
			public double FeedAll_tim { get { return feedx.tim + feedz.tim; } }

			public int All_cnt { get { return Rapid_cnt + FeedAll_cnt; } }
			public double All_xyz { get { return Rapid_xyz + FeedAll_xyz; } }
			public double All_tim { get { return Rapid_tim + FeedAll_tim; } }

			/// <summary>
			/// ＮＣデータ移動の距離・時間の積算（dataを廃止するまでの暫定処置としてミュータブルとする）
			/// </summary>
			/// <param name="kub">移動区分</param>
			/// <param name="xyz">移動距離</param>
			/// <param name="tim">移動時間</param>
			public Nctol Add(Kub kub, double xyz, double tim) {
				return new Nctol(this, kub, xyz, tim);
			}
		}

		/// <summary>
		/// 初期化コンストラクタ
		/// </summary>
		/// <param name="dummy"></param>
		public NcDst(int dummy) {
			data = new Nctol(0);
			tol = new List<Nctol>();
			tol.Add(new Nctol(0));
			m_sfeed = 0.0;
			m_efeed = 0.0;
			m_mfeed = 0.0;
			m_vdeg = 0.0;
		}

		/// <summary>
		/// 新しい工具のための積算情報の追加
		/// </summary>
		public void ToolAdd() {
			tol.Add(new Nctol(0));
		}

		/// <summary>
		/// 新しい移動情報による積算情報の更新
		/// </summary>
		/// <param name="tmpo">tmpo[0]:加工終了点、tmpo[1]:加工開始点、tmpo[2]:終了の次の加工点</param>
		/// <param name="p_fsub">マクロ変数</param>
		/// <param name="tmpm0"></param>
		/// <returns></returns>
		public int NcDist(NcOuts[] tmpo, NcMod tmpm0, NcMachine.Variable p_fsub)
		{
			int[] minf = new int[3];
			SubDst outdst = new SubDst();
			//void setdist();

			// los time plas
			if (tmpo[0].idoa == 0) {
				if (tmpm0.Lost > Post.minim) {
					data = data.Add(Kub.rapid, 0.0, tmpm0.Lost);
					tol[tmpm0.Tptr] = tol[tmpm0.Tptr].Add(Kub.rapid, 0.0, tmpm0.Lost);
				}
				return 0;
			}
			// ido time
			outdst = new SubDst(tmpo, tmpm0, this.m_efeed, p_fsub);

			data = data.Add(outdst.kub, outdst.distd, outdst.timinc);
			tol[tmpm0.Tptr] = tol[tmpm0.Tptr].Add(outdst.kub, outdst.distd, outdst.timinc);

			m_sfeed = m_efeed;
			m_mfeed = outdst.feed2 * 60.0;
			m_efeed = outdst.timefd;
			m_vdeg = outdst.timdeg;

			return 0;
		}

		/// <summary>
		/// ＮＣデータ１回（ＮＣデータ１行で２回もありうる）の移動情報を作成します。
		/// </summary>
		private readonly struct SubDst
		{
			/// <summary>送り区分（0:早送り　1:ＸＹ切削送り　2:Ｚ切削送り）</summary>
			public readonly Kub kub;
			/// <summary>機械座標系での移動量</summary>
			public readonly double distd;
			public readonly double distn;
			//public readonly double distr;
			/// <summary>加工時間[sec]</summary>
			public readonly double timinc;
			/// <summary>送り速度[mm/sec]（feed2は最大切削速度を考慮）</summary>
			public readonly double feed1;
			/// <summary>送り速度[mm/sec]（feed2は最大切削速度を考慮）</summary>
			public readonly double feed2;

			public readonly double timefd;
			public readonly double timdeg;

			/// <summary>
			/// 移動の距離・時間・速度などの情報作成
			/// </summary>
			/// <param name="tmpo">tmpo[0]:加工点の位置（計算対象点）、tmpo[1]:移動前の位置、tmpo[2]:移動先の位置</param>
			/// <param name="tmpm0"></param>
			/// <param name="efeed"></param>
			/// <param name="p_fsub">マクロ変数</param>
			public SubDst(NcOuts[] tmpo, NcMod tmpm0, double efeed, NcMachine.Variable p_fsub) {
				int stst;
				double rtmp, stmp;

				Vector3 ttmp;

				Vector3 utmp;
				Vector3[] vvec = new Vector3[2];
				vvec[0] = Vector3.v0;
				vvec[1] = Vector3.v0;

				if (tmpo[2] == null ||
					tmpo[2].idoa == 0 ||
					tmpo[2].Hokan == 0 ||
					tmpm0.GGroupValue[15].Equals(61) ||
					tmpm0.GGroupValue[0].Equals(9))
					stst = 0;
				else
					stst = 1;

				// 移動を伴う場合、その移動情報をシステム変数に設定
				p_fsub.SystemSet_IDO(tmpo[0]);

				if (tmpo[0].Hokan != 0 && stst != 0) {
					utmp = tmpo[0].heimn;
					switch (tmpo[0].Hokan) {
					case 1:
						vvec[0] = tmpo[0].Pls.ToXYZ();
						break;
					case 2:
						ttmp = -tmpo[0].Centr;
						vvec[0] = Vector3.Vvect(ttmp, utmp);
						break;
					case 3:
						ttmp = -tmpo[0].Centr;
						vvec[0] = Vector3.Vvect(utmp, ttmp);
						break;
					}
					utmp = tmpo[2].heimn;
					switch (tmpo[2].Hokan) {
					case 1:
						vvec[1] = tmpo[2].Pls.ToXYZ();
						break;
					case 2:
						ttmp = tmpo[0].Cloc.ToXYZ() - (tmpo[2].Cloc.ToXYZ() + tmpo[2].Centr);
						vvec[1] = Vector3.Vvect(ttmp, utmp);
						break;
					case 3:
						ttmp = tmpo[0].Cloc.ToXYZ() - (tmpo[2].Cloc.ToXYZ() + tmpo[2].Centr);
						vvec[1] = Vector3.Vvect(utmp, ttmp);
						break;
					}
				}


				// dist & time
				switch (tmpo[0].Hokan) {
				case 0:
					ttmp = tmpo[0].Cloc.ToXYZ() - tmpo[1].Cloc.ToXYZ();
					distd = ttmp.Abs;

					// 最も移動時間のかかる軸の計算
					if (tmpo[0].Pls.Xyzabc0) throw new Exception("移動する軸がありません");
					SdgtNo jiku = tmpo[0].Pls.MaxTimeAxis(NcMachine.ParaData1420(), true);

					//distn = Math.Abs(tmpo[0].pls[jiku]);	// 中間点の場合plsは直前からの距離ではない
					distn = Math.Abs((tmpo[0].Cloc - tmpo[1].Cloc)[jiku]);  // 中間点の場合plsは直前からの距離ではない
					feed1 = NcMachine.ParaData1420()[(int)jiku] / 60.0 / Post.PostData['F'].sdgt;

					kub = Kub.rapid;
					break;
				case 1:
					ttmp = tmpo[0].Cloc.ToXYZ() - tmpo[1].Cloc.ToXYZ();

					//m_distd = Math.Sqrt(Vector.vscal(ttmp, ttmp));
					distd = ttmp.Abs;
					distn = tmpo[0].Pls.ToXYZ().Abs;

					//distr = distn;
					feed1 = tmpm0.CodeValue['F'].ToDouble / 60.0;
					if (tmpo[0].Pls.X0 && tmpo[0].Pls.Y0 && (tmpo[0].Pls.Z0 || tmpo[0].Pls.Z > 0))
						kub = Kub.feedz;
					else
						kub = Kub.feedx;
					break;
				case 2:
				case 3:
					distd = Math.Abs(tmpo[0].Rad0 * tmpo[0].Deg);
					distn = distd + tmpo[0].Pls.ToABC().Abs;

					//distr = distn;
					feed1 = tmpm0.CodeValue['F'].ToDouble / 60.0;
					if (distn > Post.minim) {
						kub = Kub.feedx;
					}
					else {
						kub = Kub.feedz;
					}
					break;
				default:
					throw new Exception("qefbqfrebh");
				}

				// saidai sokudo kouryo
				feed2 = feed1;
				// 移動あり、直線円弧補間、移動する軸あり（一周の円弧などの場合を除く）
				if (distn >= Post.minim && tmpo[0].Hokan != 0 && tmpo[0].Pls.Xyzabc0 == false) {
					//CamUtil.LogOut.CheckCount("ncdst 395", "最大送り速度制限のチェック");
					double mintim = tmpo[0].Pls.Min_Time(NcMachine.ParaData1422());		// 秒
					if (mintim > distn / feed1) {
						CamUtil.LogOut.CheckCount("ncdst 416", false, "最大送り速度制限で送り速度が低下しました");
						feed2 = distn / mintim;
					}
				}

				if (tmpm0.Tptr == 0 && kub != Kub.rapid && tmpm0.ToolCount != 0)
					_main.Error.Ncerr(3, "feed rate cut with empty tool");

				timinc = 0.0;
				timdeg = 0.0;
				timefd = 0.0;
				if (feed2 < Post.minim) {
					LogOut.CheckCount("ncdst 412", false, $"feed rate is 0(line={tmpm0.NcLineNo:d}:{tmpo[0].Ichi.ToXYZ().ToString()}) を2017/09/14まで出力していた。");
				}
				else if (distn >= Post.minim) {
					if (kub != Kub.rapid && stst != 0) {
						timefd = feed2 * 60.0;
						// magari kakudo keisann
						rtmp = Vector3.Vscal(vvec[0], vvec[1]);
						stmp = vvec[0].Abs * vvec[1].Abs;
						if (stmp > 0.0) {
							rtmp /= stmp;
							if (rtmp > 1.0)
								rtmp = 1.0;
							else if (rtmp < -1.0)
								rtmp = -1.0;
							timdeg = Math.Acos(rtmp);
						}
					}
					timinc = SetTim(kub, distn, feed2, efeed, stst);
					//timinc = timinc * distr / distn;
				}
			}

			private double SetTim(Kub kub, double rxyz, double feed1, double startf, int stst)
			{
				int ii;
				double rtmp, stmp, rout;

				if (kub == Kub.rapid) {
					if (startf / 60.0 > Post.minim)
						_main.Error.Ncerr(3, "rapid feed start not 0");

					if (rxyz > NcMachine.t0 * feed1)
						rout = rxyz / feed1 + NcMachine.t0;
					else
						rout = Math.Sqrt(4.0 * rxyz * NcMachine.t0 / feed1);

					// inposition
					if (rxyz > NcMachine.t0 * feed1)
						rtmp = NcMachine.t2 * Math.Log(2.0 * feed1 * NcMachine.t2 * NcMachine.t2 / NcMachine.t0 / NcMachine.th) - 2 * NcMachine.t2;
					else
						rtmp = NcMachine.t2 * Math.Log(2.0 * Math.Sqrt(rxyz * feed1 / NcMachine.t0)
							* NcMachine.t2 * NcMachine.t2 / NcMachine.t0 / NcMachine.th) - 2 * NcMachine.t2;
					rout += rtmp;
				}
				else {
					if (startf / 60.0 < Post.minim)
						rout = rxyz / feed1 + (NcMachine.t1 + NcMachine.t2);
					else
						rout = rxyz / feed1;

					if (stst == 0) {
						stmp = NcMachine.th / feed1;
						rtmp = 0.0;
						for (ii = 0; ii < 20; ii++)
							if (stmp > NcMachine.gosa[ii]) {
								if (ii != 0)
									rtmp = ((NcMachine.t1 + NcMachine.t2) * (ii - 1) / 2.0) +
							  (NcMachine.gosa[ii - 1] - stmp) /
							  (NcMachine.gosa[ii - 1] - NcMachine.gosa[ii]) *
							  ((NcMachine.t1 + NcMachine.t2) / 2.0) - (NcMachine.t1 + NcMachine.t2);
								break;
							}

						if (rtmp < 0.0)
							rtmp = 0.0;
						rout += rtmp;
					}
				}
				return rout;
			}
		}
	}
}
