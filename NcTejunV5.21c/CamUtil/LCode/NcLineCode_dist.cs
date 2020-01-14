using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.Data.SqlClient;

namespace CamUtil.LCode
{
	public partial class NcLineCode
	{
		/// <summary>ＮＣデータの移動距離、加工時間と存在範囲の情報をＮＣデータを元に積算し保存します</summary>
		public struct NcDist
		{
			/// <summary>早送り速度のＸＭＬ標準値</summary>
			public const double RapidFeed = 12000.0;

			// /////////////////////////////
			// 以下は演算子のオーバーロード
			// /////////////////////////////

			/// <summary>３Ｄベクトルの==演算子</summary>
			public static bool operator ==(NcLineCode.NcDist c1, NcLineCode.NcDist c2) { return c1.Equals(c2); }
			/// <summary>３Ｄベクトルの!=演算子</summary>
			public static bool operator !=(NcLineCode.NcDist c1, NcLineCode.NcDist c2) { return !c1.Equals(c2); }

			// ////////////////////////
			// 以上静的
			// ////////////////////////







			/// <summary>加工長mm（G00）</summary>
			public double G00 { get { return m_G00X + m_G00Z; } }
			/// <summary>加工長mm（G01.G02,G03）</summary>
			public double G01 { get { return m_G01; } }

			/// <summary>加工長mm（G00, XY平面移動）</summary>
			private double m_G00X;
			/// <summary>加工長mm（G00, Z軸移動）</summary>
			private double m_G00Z;
			/// <summary>加工長mm（G01.G02,G03）</summary>
			private double m_G01;

			/// <summary>最大ＸＹＺ位置</summary>
			public Vector3? Max { get { return m_max; } } private Vector3? m_max;
			/// <summary>最大ＸＹＺ位置を更新します</summary>
			public void MaxAdd(Vector3? value) { if (value.HasValue) m_max = m_max == null ? value : Vector3.Max(m_max.Value, value.Value); }
			/// <summary>最小ＸＹＺ位置</summary>
			public Vector3? Min { get { return m_min; } } private Vector3? m_min;
			/// <summary>最小ＸＹＺ位置を更新します</summary>
			public void MinAdd(Vector3? value) { if (value.HasValue) m_min = m_min == null ? value : Vector3.Min(m_min.Value, value.Value); }


			/// <summary>切削送りの加工時間(min)</summary>
			public double CuttingTime { get { return m_TimC; } }
			/// <summary>直線円弧補間時の加工時間(min)</summary>
			public double FeedTime { get { return m_TimC + m_TimN; } }
			/// <summary>標準早送り速度での位置決めと滞留時間(min)</summary>
			public double NonFeedTime() { return NonFeedTime(RapidFeed, RapidFeed); }
			/// <summary>指定早送り速度での位置決めと滞留時間(min)</summary>
			public double NonFeedTime(double rapx, double rapz) { return m_G00X / rapx + m_G00Z / rapz + m_dwell; }



			/// <summary>切削送り速度(mm/min)。切削送り加工時間の計算不要の場合はnullとします</summary>
			private int? frate;
			/// <summary>切削送り加工時間(min)</summary>
			private double m_TimC;
			/// <summary>直線円弧補間時の非切削加工時間(min)。切削送り速度でない加工時間です。</summary>
			private double m_TimN;
			/// <summary>滞留時間(min)</summary>
			private double m_dwell;

			/// <summary>傾斜加工の情報リスト</summary>
			private readonly CamUtil.RotationAxis[] rotAxis;
			/// <summary>最大孔深さ（型上面～孔底の距離）現在、算出のみでその値は未使用です</summary>
			private double depth;
			/*
			/// <summary>座標回転角度のリスト</summary>
			private Angle3[] MachineAxisList;
			*/
			/// <summary>傾斜角の連番（初期値０）</summary>
			private int keishaNo;


			/// <summary>
			/// ＮＣデータによって積算するための初期値で作成します
			/// </summary>
			/// <param name="feedrate">ＮＣデータ内の切削送り速度。加工時の送りではないので注意。加工時間の計算不要の場合はnullで可</param>
			/// <param name="eulerXYZ"></param>
			public NcDist(double? feedrate, Angle3[] eulerXYZ) {
				this.m_G00X = 0.0;
				this.m_G00Z = 0.0;
				this.m_G01 = 0.0;
				this.m_max = null;
				this.m_min = null;

				this.frate = feedrate.HasValue ? (int)Math.Round(feedrate.Value) : (int?)null;
				this.m_TimC = 0.0;
				this.m_TimN = 0.0;
				this.m_dwell = 0.0;

				rotAxis = new RotationAxis[eulerXYZ.Length];
				for (int ii = 0; ii < eulerXYZ.Length; ii++)
					rotAxis[ii] = new RotationAxis(eulerXYZ[ii]);
				keishaNo = 0;
				depth = 0.0;
			}

			/// <summary>
			/// ＸＭＬ情報からの結果を作成します（ADD in 2015/01/20）
			/// </summary>
			/// <param name="feedrate">ＮＣデータ内の切削送り速度。加工時の送りではないので注意</param>
			/// <param name="xml">ＸＭＬデータ</param>
			/// <param name="rapx">早送り速度ＸＹ</param>
			public NcDist(double feedrate, CamNcD.XmlNavi.Tool xml, double rapx) {
				double? ftimerate = null;
				if (xml.CuttingTime.HasValue)
					ftimerate = xml.CuttingTime.Value > xml.NCTIM ? xml.CuttingTime.Value / xml.NCTIM : xml.NCTIM / xml.CuttingTime.Value;

				// NCSPEEDで寿命分割された場合、早送り加工時間は更新されるが早送り加工長は更新されない
				if (xml.SmLNS) {
					this.m_G00X = xml.NOFDT * rapx;
					this.m_dwell = 0.0;
					this.m_TimC = xml.NCTIM;
					this.m_G01 = xml.NCLEN;				// 加工長の設定
					this.m_TimN = xml.NCTIM - this.m_TimC;
				}
				// NCSPEEDの場合は、G91を認識しないことによりxml.NCLEN, xml.NCTIMが異常値となることがある
				//else if (xml.smCLC != null && xml.CuttingTime.HasValue && ftimerate > 8.0) {
				//	this.m_G00X = (xml.RPLEN.HasValue) ? xml.RPLEN.Value : 0.0;
				//	this.m_dwell = Math.Max(0.0, xml.NOFDT - m_G00X / RapidFeed);
				//	this.m_TimC = xml.CuttingTime.Value;
				//	this.m_G01 = xml.CuttingTime.Value * feedrate;
				//	this.m_TimN = xml.CuttingTime.Value;
				//}
				else {
					this.m_G00X = xml.RPLEN ?? 0.0;
					this.m_dwell = Math.Max(0.0, xml.NOFDT - m_G00X / RapidFeed);
					this.m_TimC = (xml.CuttingTime.HasValue && xml.CuttingTime.Value <= xml.NCTIM) ? xml.CuttingTime.Value : xml.NCTIM;
					this.m_G01 = xml.NCLEN;				// 加工長の設定
					this.m_TimN = xml.NCTIM - this.m_TimC;
				}
				this.m_G00Z = 0.0;
				this.frate = (int)Math.Round(feedrate);

				this.m_max = xml.MaxXYZ;
				this.m_min = xml.MinXYZ;

				rotAxis = new RotationAxis[xml.MachiningAxisList.Length];
				for (int ii = 0; ii < xml.MachiningAxisList.Length; ii++)
					rotAxis[ii] = new RotationAxis(xml.MachiningAxisList[ii]);
				keishaNo = 0;
				depth = 0.0;
			}


			/// <summary>
			/// 他の NcDist 情報で加工長、移動範囲、加工時間を追加更新します
			/// </summary>
			/// <param name="nd"></param>
			public void Add(NcDist nd) {
				this.m_G00X += nd.m_G00X;
				this.m_G00Z += nd.m_G00Z;
				this.m_G01 += nd.m_G01;
				this.MaxAdd(nd.m_max);
				this.MinAdd(nd.m_min);
				this.m_TimC += nd.m_TimC;
				this.m_TimN += nd.m_TimN;
				this.m_dwell += nd.m_dwell;
				//this.frate
				//keishaNo
				//rotAxis
				this.depth = Math.Max(depth, nd.depth);
			}

			/// <summary>
			/// 滞留時間（秒）を積算します。ガンドリルの孔加工の穴底での停止時間などに用います。
			/// </summary>
			/// <param name="dwell">停止時間（秒）</param>
			public void AddDwell(double dwell) {
				m_dwell += dwell / 60.0;
			}

			/// <summary>
			/// 計算したＮＣデータの存在範囲データをＺ方向にシフトします。工具先端補正などに用います。
			/// </summary>
			/// <param name="tvec">Ｚシフト量</param>
			public void ShiftZ(double tvec) {
				m_max += new Vector3(0.0, 0.0, tvec);
				m_min += new Vector3(0.0, 0.0, tvec);
			}

			/// <summary>
			/// ＮＣデータ１行の情報 NcLineCode を用いて移動距離、移動範囲、加工時間などを積算します
			/// </summary>
			/// <returns></returns>
			public void PassLength(NcLineCode nlCode) {

				// カスタムマクロ
				if (nlCode.G6 != 67) {

					// G66 は X, Y がないと加工しない
					if (nlCode.G6 == 66 && nlCode.B_26('X') != true && nlCode.B_26('Y') != true) return;

					// マクロプログラムごとの加工長の計算
					if (nlCode.G6 == 65)
						this.UserMacroSet65(nlCode);
					else
						this.UserMacroSet66(nlCode);
				}
				// 固定サイクル
				else if (nlCode.G8 != 80) {

					// 固定サイクルはXYZRのいずれかがあると穴あけ動作する(XYのみと勘違いをしていた) 2013/03/07
					//if (nlCode.b_26['X'] != true && nlCode.b_26['Y'] != true)
					if (nlCode.B_26('X') != true && nlCode.B_26('Y') != true && nlCode.B_26('Z') != true && nlCode.B_26('R') != true)
						return;

					// イニシャル点でのＸＹ移動距離
					if (Math.Abs(nlCode.Xyzsf.Z - nlCode.Xyzsf.PreZ) >= 0.001) throw new Exception("qwefbqwrhfbwhr");
					this.m_G00X += ((Vector3)(nlCode.Xyzsf.ToXYZ() - nlCode.Xyzsf.PreToXYZ())).Abs;
					this.MinMax(nlCode.Xyzsf.ToXYZ());
					// サイクルごとの加工長の計算
					this.FixCycleSet(nlCode);
				}
				else if (nlCode.SubPro.HasValue) {
					switch (nlCode.SubPro.Value) {
					case 9698:
						keishaNo++;
						//rotAxis = new RotationAxis(MachineAxisList[keishaNo]);
						break;
					case 9699:
						break;
					}
				}
				// 直線補間、円弧補間
				else {
					Vector3 cntr;
					short g17 = nlCode.G2;

					short g123 = nlCode.G1;
					switch (nlCode.ncForm.Id) {
					case BaseNcForm.ID.BUHIN:
						if (nlCode.G1 == 1 && nlCode.Xyzsf.Fi == RF_5AXIS) g123 = 0;    // 部品加工８０ｍに対応 in 2014/12/24
						break;
					}

					switch (g123) {
					case 0:
						cntr = nlCode.Xyzsf.ToXYZ() - nlCode.Xyzsf.PreToXYZ();
						if (Math.Abs(cntr.Z) > cntr.Abs / Math.Sqrt(2.0))
							this.m_G00Z += Math.Abs(cntr.Z);
						else
							this.m_G00X += cntr.Abs;
						this.MinMax(nlCode.Xyzsf.ToXYZ());
						break;
					case 1:
						this.AddG01(((Vector3)(nlCode.Xyzsf.ToXYZ() - nlCode.Xyzsf.PreToXYZ())).Abs, nlCode.Xyzsf.F);
						this.MinMax(nlCode.Xyzsf.ToXYZ());
						break;
					case 2:
					case 3:
						if (g17 != 17) throw new Exception("まだ、G18, G19 の円弧補間に対応していません");
						cntr = nlCode.Center();
						Vector3 sttv = (nlCode.Xyzsf.PreToXYZ().ToVector2(g17) - cntr.ToVector2(g17)).ToVector3(g17, Vector3.v0);
						Vector3 endv = (nlCode.Xyzsf.ToXYZ().ToVector2(g17) - cntr.ToVector2(g17)).ToVector3(g17, Vector3.v0);
						double rad = nlCode.Radius();
						double ang = nlCode.Angle(cntr);

						if (!nlCode.B_26('R')) {
							if (Math.Abs(rad - sttv.Abs) > 0.00001) throw new Exception("akergfnearjfnj");
							if (Math.Abs(rad - endv.Abs) > 0.002)
								LogOut.CheckCount("NcLineCode_dist 343", false, $"円錐補間  r={rad:f}  r={endv.Abs:f}");
							rad = Math.Max(rad, endv.Abs);
						}

						// ///////////
						// distの設定
						// ///////////
						this.AddG01(CamUtil.Vector3.Length(rad * ang, cntr.Z - nlCode.Xyzsf.Z), nlCode.Xyzsf.F);

						// ///////////
						// 最大最小の設定
						// ///////////
						Vector3[] xyMxn2 = new Vector3[4];
						xyMxn2[0] = new Vector3(rad, 0.0, 0.0);
						xyMxn2[1] = new Vector3(0.0, rad, 0.0);
						xyMxn2[2] = new Vector3(-rad, 0.0, 0.0);
						xyMxn2[3] = new Vector3(0.0, -rad, 0.0);
						for (int ii = 0; ii < 4; ii++) {
							if (Math.Abs(ang) < 2.0 * Math.PI) {
								if (new Ichi(xyMxn2[ii] + cntr, 1000) == new Ichi(sttv.Unit(rad) + cntr, 1000))
									continue;   // 始点位置との誤差がIchi.Gosa以下
								if (new Ichi(xyMxn2[ii] + cntr, 1000) == new Ichi(endv.Unit(rad) + cntr, 1000))
									continue;   // 終点位置との誤差がIchi.Gosa以下
												// ２つの平面ベクトル（円弧補間始点と円周上の点）の回転角度を求める
								Double ang2 = _Geometry_Plane.Vangle(sttv.ToVector2(g17), xyMxn2[ii].ToVector2(g17), g123);
								if (Math.Abs(ang2) > Math.Abs(ang))
									continue;   // 終点までの角度の方が小さい
							}
							this.MinMax(xyMxn2[ii] + cntr);
						}
						this.MinMax(nlCode.Xyzsf.ToXYZ());

						break;
					default:
						throw new Exception("kqfqrji");
					}

					// ドウェル Add in 2016/0524
					if (nlCode.ExistsG(4)) {
						foreach (NumCode num in nlCode.NumList) {
							if (num.ncChar == 'P') { m_dwell += num.L / 1000.0 / 60.0; break; }
							if (num.ncChar == 'X') { m_dwell += num.dblData / 60.0; break; }
						}
					}
				}
			}
			/// <summary>
			/// 固定サイクルの積算値を計算します
			/// </summary>
			/// <returns></returns>
			private void FixCycleSet(NcLineCode nlCode) {
				int lnum;

				switch (nlCode.G8) {
				case 73:
					lnum = (int)Math.Floor((nlCode.G8p['R'].D - nlCode.G8p['Z'].D) / nlCode.G8p['Q'].D);
					if (lnum < 0) throw new Exception("wefrqbwhbfahfrh");
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G8p['R'].D);
					this.AddG01(Machine.para6210 * lnum + Math.Abs(nlCode.G8p['R'].D - nlCode.G8p['Z'].D), nlCode.Xyzsf.F);
					this.m_G00Z += Machine.para6210 * lnum;
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G8p['Z'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G8p['Z'].D));
					break;
				case 81:
				case 82:
				case 86:
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G8p['R'].D);
					this.AddG01(Math.Abs(nlCode.G8p['R'].D - nlCode.G8p['Z'].D), nlCode.Xyzsf.F);
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G8p['Z'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G8p['Z'].D));
					if (nlCode.G8 == 82)
						this.m_dwell += nlCode.G8p['P'].L / 1000.0 / 60.0;
					break;
				case 83:
					lnum = (int)Math.Ceiling((nlCode.G8p['R'].D - nlCode.G8p['Z'].D) / nlCode.G8p['Q'].D) - 1;
					if (lnum < 0) throw new Exception("wefrqbwhbfahfrh");
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G8p['R'].D);
					this.AddG01(Machine.para6211 * lnum + Math.Abs(nlCode.G8p['R'].D - nlCode.G8p['Z'].D), nlCode.Xyzsf.F);
					this.m_G00Z += 2 * ((lnum + 1) * lnum / 2) * nlCode.G8p['Q'].D - lnum * Machine.para6211;
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G8p['Z'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G8p['Z'].D));
					break;
				case 84:
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G8p['R'].D);
					this.AddG01(2 * Math.Abs(nlCode.G8p['R'].D - nlCode.G8p['Z'].D), nlCode.Xyzsf.F);
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G8p['R'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G8p['Z'].D));
					break;
				default:
					throw new Exception("固定サイクル G" + nlCode.G8.ToString() + " は使用できません");
				}
			}
			/// <summary>
			/// G65マクロの積算値を計算します
			/// </summary>
			/// <returns></returns>
			private void UserMacroSet65(NcLineCode nlCode) {
				switch (nlCode.ncForm.Id) {
				case BaseNcForm.ID.GENERAL:
					switch (nlCode.G6p.ProgNo) {
					case 8755:	// 測定
						// イニシャル点でのＸＹ移動距離
						this.m_G00X += ((Vector3)(nlCode.Xyzsf.ToXYZ() - nlCode.Xyzsf.PreToXYZ())).Abs;
						this.MinMax(nlCode.Xyzsf.ToXYZ());

						Vector3 kvec = new Vector3(nlCode.G6p['I'].D, nlCode.G6p['J'].D, nlCode.G6p['K'].D) * nlCode.G6p['R'].D;

						// //////////////////////
						// 暫定処置　2015/05/28
						// //////////////////////
						double? ftmp = nlCode.Xyzsf.F ?? nlCode.G6p['F'].D;
						this.AddG01(Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['Z'].D), ftmp, false);
						this.AddG01(kvec.Abs, 50.0, false);
						this.AddG01(kvec.Abs, ftmp, false);
						// //////////////////////
						// //////////////////////

						this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['Z'].D);
						this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['Z'].D));
						this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['Z'].D) - kvec);
						break;
					default:
						throw new Exception(nlCode.G6p.ProgName + " は使用できないマクロです。１");
					}
					break;
				case BaseNcForm.ID.BUHIN:
					switch (nlCode.G6p.ProgNo) {
					case 8101:	// 部品加工用マクロ（クリアランスプレーンに移動）
					case 8102:	// 部品加工用マクロ（加工モード設定）
					case 8103:	// 部品加工用マクロ（クーラント設定）
					case 8700:  // 部品加工用マクロ（座標系の設定）
					case 8730:	// 部品加工用マクロ（座標系のクリア）
					case 8750:	// 部品加工用マクロ（後測定：形状測定開始）
					case 8759:	// 部品加工用マクロ（後測定：形状測定終了）
					case 9376:	// 部品加工用マクロ（工具名をマクロ変数に入力する）
						break;
					case 8755:	// 測定
						this.AddG01(75.0, 500.0);
						break;
					case 9345:	// 部品加工用マクロ（前測定）
					case 9346:	// 部品加工用マクロ（前測定）
						this.AddG01(200.0, 500.0);
						break;
					case 8104:	// 部品加工用マクロ（ヘリカル/円錐補間）
						if (nlCode.G2 != 17) { throw new Exception("wjefqwegh"); }
						// 回転中心
						Vector3 cntr = nlCode.Xyzsf.PreToXYZ() + new Vector3(
							nlCode.G6p['I'].Set ? nlCode.G6p['I'].D : 0.0, nlCode.G6p['J'].Set ? nlCode.G6p['J'].D : 0.0, 0.0);
						// 回転半径（開始終了）
						double rad0 = Vector3.Length((nlCode.Xyzsf.PreX - cntr.X), (nlCode.Xyzsf.PreY - cntr.Y));
						double rad1 = Vector3.Length((nlCode.Xyzsf.X - cntr.X), (nlCode.Xyzsf.Y - cntr.Y));
						// 回転角度
						_Geometry_Plane.Vector2 v0 = (nlCode.Xyzsf.PreToXYZ() - cntr).ToVector2(17);
						_Geometry_Plane.Vector2 v1 = (nlCode.Xyzsf.ToXYZ() - cntr).ToVector2(17);
						if (Math.Abs(_Geometry_Plane.Vvect(v0, v1)) > 0.001) throw new Exception("始終点が一致しない");
						double angl = 2.0 * Math.PI * nlCode.G6p['H'].D;

						this.MinMax(cntr + new Vector3(Math.Max(rad0, rad1), 0.0, 0.0));
						this.MinMax(cntr + new Vector3(0.0, Math.Max(rad0, rad1), 0.0));
						this.MinMax(cntr + new Vector3(-Math.Max(rad0, rad1), 0.0, 0.0));
						this.MinMax(cntr + new Vector3(0.0, -Math.Max(rad0, rad1), 0.0));
						this.AddG01(CamUtil.Vector3.Length((rad0 + rad1) / 2.0 * angl, cntr.Z - nlCode.G6p['Z'].D), nlCode.Xyzsf.F);
						this.MinMax(nlCode.Xyzsf.ToXYZ());
						break;
					case 8105:	// 部品加工用マクロ（リジッドタップ）
						// イニシャル点でのＸＹ移動距離
						this.m_G00X += ((Vector3)(nlCode.Xyzsf.ToXYZ() - nlCode.Xyzsf.PreToXYZ())).Abs;
						this.MinMax(nlCode.Xyzsf.ToXYZ());

						// //////////////////////
						// 暫定処置　2015/06/01 2015/07/02
						// //////////////////////
						double? ftmp = nlCode.Xyzsf.F.HasValue && nlCode.Xyzsf.F != RF_5AXIS ? nlCode.Xyzsf.F * nlCode.Xyzsf.S : nlCode.G6p['F'].D * nlCode.Xyzsf.S;
						this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['R'].D);
						this.AddG01(2 * Math.Abs(nlCode.G6p['R'].D - nlCode.G6p['Z'].D), ftmp);
						this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['R'].D);

						this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['Z'].D));
						break;
					default:
						throw new Exception(nlCode.G6p.ProgName + " は使用できないマクロです。２");
					}
					break;
				default:
					throw new Exception("efdwedfheu");
				}
			}
			/// <summary>
			/// G66マクロの積算値を計算します
			/// </summary>
			/// <returns></returns>
			private void UserMacroSet66(NcLineCode nlCode) {
				double rad, kiri;
				int lnum;
				double clearz, S126, stt;
				double radmax, radinc;

				// イニシャル点でのＸＹ移動距離
				if (Math.Abs(nlCode.Xyzsf.Z - nlCode.Xyzsf.PreZ) > 0.001) throw new Exception("qwefbqwrhfbwhr");
				this.m_G00X += ((Vector3)(nlCode.Xyzsf.ToXYZ() - nlCode.Xyzsf.PreToXYZ())).Abs;
				this.MinMax(nlCode.Xyzsf.ToXYZ());

				switch (nlCode.G6p.ProgNo) {
				case 8010:  // 周り止め
				case 8013:  // 周り止め
				case 8015:  // 周り止め
				case 8011:  // 周り止め（コアピン・スリーブピン逃がし部加工用 ADD 2007.01.25）
				case 8014:  // 周り止め（コアピン・スリーブピン逃がしあり専用 ADD 2007.01.25）
				case 8016:  // 周り止め（コアピン・スリーブピン逃がし部加工用 ADD 2019.06.12）
				case 8019:  // 周り止め（コアピン・スリーブピン逃がしあり専用 ADD 2019.06.12）
					clearz = 5.0;
					DataRow mawaridome = CamUtil.CamNcD.KijunSize.Mawaridome(nlCode.G6p.ProgNo.ToString(), nlCode.G6p['U'].D);

					// アプローチの無い P8011, P8016 はすべて切削送りで加工するため以下のように設定する
					double? appx = (double)mawaridome["アプローチ速度XY"] > 0 ? (double)mawaridome["アプローチ速度XY"] : nlCode.Xyzsf.F;
					double? appz = (double)mawaridome["アプローチ速度Z"] > 0 ? (double)mawaridome["アプローチ速度Z"] : nlCode.Xyzsf.F;

					lnum = (int)Math.Ceiling((double)mawaridome["深さ"] / (double)mawaridome["プランジ量"]);
					this.m_G00Z += nlCode.Xyzsf.Z - (nlCode.G6p['C'].D + clearz);
					this.AddG01(clearz + (double)mawaridome["加工基準深さ"], 100.0, false);
					this.AddG01((double)mawaridome["深さ"], appz, false);
					this.AddG01(lnum * (double)mawaridome["パス幅"], appx, false);
					this.AddG01(lnum * (double)mawaridome["アプローチ長さ"] * 2, appx, false);
					this.AddG01(lnum * ((double)mawaridome["パス長さ"] - (double)mawaridome["アプローチ長さ"]) * 2, nlCode.Xyzsf.F);
					this.AddG01(lnum * (double)mawaridome["パス幅"], nlCode.Xyzsf.F);
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - (nlCode.G6p['C'].D - (double)mawaridome["加工基準深さ"] - (double)mawaridome["深さ"]));
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['C'].D - (double)mawaridome["加工基準深さ"] - (double)mawaridome["深さ"]));
					break;
				//case 8020:  // 円内側面仕上げ加工
				case 8025:  // スプルーロック
					clearz = 5.0;
					if (nlCode.G6p['Q'].Set) {
						if (nlCode.G6p['C'].Set)
							lnum = (int)Math.Ceiling((nlCode.G6p['C'].D - nlCode.G6p['Z'].D) / nlCode.G6p['Q'].D);
						else
							lnum = (int)Math.Ceiling((0.0 - nlCode.G6p['Z'].D) / nlCode.G6p['Q'].D);
					}
					else lnum = 1;
					rad = (nlCode.G6p['I'].D - nlCode.G6p['E'].D) / 2.0;
					this.m_G00Z += nlCode.Xyzsf.Z - (nlCode.G6p['C'].D + clearz);
					this.AddG01(clearz, nlCode.Xyzsf.F, false);
					this.AddG01(lnum * CirApproach(rad), nlCode.Xyzsf.F, false);	// アプローチＸＹ
					this.AddG01(lnum * (rad * Math.PI * 2), nlCode.Xyzsf.F);	// 加工
					this.AddG01(lnum * CirApproach(rad), nlCode.Xyzsf.F, false);	// リトラクトＸＹ
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['Z'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X + rad, nlCode.Xyzsf.Y + rad, nlCode.G6p['Z'].D));
					this.MinMax(new Vector3(nlCode.Xyzsf.X - rad, nlCode.Xyzsf.Y - rad, nlCode.G6p['Z'].D));
					break;
				/*
				case 8030:  // 円内側面荒加工
					clearz = 5.0;
					if (nlCode.g6p['Q'].set)
						lnum = (int)Math.Ceiling(((nlCode.g6p['C'].set ? nlCode.g6p['C'].d : 0.0) - nlCode.g6p['Z'].d) / nlCode.g6p['Q'].d);
					else
						lnum = 1;

					this.m_G00Z += nlCode.xyzsf.Z - (nlCode.g6p['C'].d + clearz);
					this.AddG01(clearz, nlCode.xyzsf.F, false);

					double radmin, radmax, radinc;
					int lnumr;
					if (nlCode.g6p['R'].set)
						radmin = (nlCode.g6p['R'].d - nlCode.g6p['E'].d) / 2.0 - 1.0;
					else
						radmin = 0.6 * nlCode.g6p['E'].d;
					radmax = (nlCode.g6p['I'].d - nlCode.g6p['E'].d) / 2.0;
					radinc = nlCode.g6p['E'].d * 0.6;
					if (radmin <= radinc)
						radmin = radinc;
					lnumr = (int)Math.Ceiling((radmax - radmin) / radinc);
					if (lnumr <= 0) {
						radmin = radmax;
						lnumr = 1;
					}
					//dist.G01] = (radmin + (radmin + radinc) + (radmin + radinc * 2) + .....) * 2.0 * Math.PI;
					this.AddG01(lnum * ((radmin * lnumr + radinc * ((0 + (lnumr - 1)) * lnumr / 2)) * 2.0 * Math.PI), nlCode.xyzsf.F);
					this.AddG01(lnum * (radmax * 2.0 * Math.PI), nlCode.xyzsf.F);
					this.AddG01(2.0 * radmax, nlCode.xyzsf.F, false);	// アプローチリトラクトＸＹ
					this.m_G00Z += Math.Abs(nlCode.xyzsf.Z - nlCode.g6p['Z'].d);
					this.MinMax(new Vector3(nlCode.xyzsf.X + radmax, nlCode.xyzsf.Y + radmax, nlCode.g6p['Z'].d));
					this.MinMax(new Vector3(nlCode.xyzsf.X - radmax, nlCode.xyzsf.Y - radmax, nlCode.g6p['Z'].d));
					break;
				*/
				/*
				case 8046:  // エアブロー
					this.AddG01(Math.Abs(nlCode.xyzsf.Z - nlCode.g6p['R'].d), 1000.0, false);
					this.m_dwell += 3.0 / 60.0;
					this.AddG01(Math.Abs(nlCode.g6p['R'].d - nlCode.g6p['Z'].d), nlCode.xyzsf.F, false);
					this.m_G00Z += Math.Abs(nlCode.xyzsf.Z - nlCode.g6p['Z'].d);
					this.MinMax(new Vector3(nlCode.xyzsf.X, nlCode.xyzsf.Y, nlCode.g6p['Z'].d));
					break;
				*/
				case 8080:	// 面取り
					if (nlCode.G6p['I'].D <= 12.0) throw new Exception("awefqbfhb");
					else if (nlCode.G6p['I'].D <= 20.0) {
						S126 = Math.Round((nlCode.G6p['I'].D - 6.0) / 2.0, 1);
						this.m_G00Z += nlCode.Xyzsf.Z - nlCode.G6p['C'].D;
						this.AddG01(S126, nlCode.Xyzsf.F * 0.5, true);
						this.m_G00Z += S126 + Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['C'].D);
						this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['C'].D - S126));
					}
					else if (nlCode.G6p['I'].D <= 360.0) {
						if (nlCode.G6p['I'].D <= 50.0) {
							S126 = 4.0;
							rad = nlCode.G6p['I'].D / 2.0 - 6.5;
						}
						else {
							S126 = 5.0;
							rad = nlCode.G6p['I'].D / 2.0 - 7.5;
						}
						clearz = 5.0;
						this.m_G00Z += nlCode.Xyzsf.Z - (nlCode.G6p['C'].D + clearz);
						this.AddG01(clearz, nlCode.Xyzsf.F, false);
						this.AddG01(S126, nlCode.Xyzsf.F, false);
						this.AddG01(Vector3.Length(3.0, rad - 3.0), nlCode.Xyzsf.F * 3.0);
						this.AddG01(3.0 * Math.PI / 2.0, nlCode.Xyzsf.F, false);
						this.AddG01(rad * Math.PI * 2.0, nlCode.Xyzsf.F);
						this.AddG01(3.0 * Math.PI / 2.0, nlCode.Xyzsf.F, false);
						this.m_G00Z += S126 + Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['C'].D);
						this.m_G00X += Vector3.Length(3.0, rad - 3.0);
						this.MinMax(new Vector3(nlCode.Xyzsf.X + rad, nlCode.Xyzsf.Y + rad, nlCode.G6p['C'].D - S126));
						this.MinMax(new Vector3(nlCode.Xyzsf.X - rad, nlCode.Xyzsf.Y - rad, nlCode.G6p['C'].D - S126));
					}
					else throw new Exception("awefqbfhb");
					break;
				case 8082:  // 面取り
					if (nlCode.G6p['I'].D <= 12.0) throw new Exception("awefqbfhb");
					else if (nlCode.G6p['I'].D <= 20.0) {
						S126 = 2.0;
						rad = nlCode.G6p['I'].D / 2.0 - 2.5;
					}
					else if (nlCode.G6p['I'].D <= 50.0) {
						S126 = 2.5;
						rad = nlCode.G6p['I'].D / 2.0 - 2.5;
					}
					else if (nlCode.G6p['I'].D <= 360.0) {
						S126 = 3.0;
						rad = nlCode.G6p['I'].D / 2.0 - 3.0;
					}
					else throw new Exception("awefqbfhb");
					clearz = 5.0;
					this.m_G00Z += nlCode.Xyzsf.Z - (nlCode.G6p['C'].D + clearz);
					this.AddG01(clearz, nlCode.Xyzsf.F, false);
					this.AddG01(S126, nlCode.Xyzsf.F, false);
					this.AddG01(Vector3.Length(3.0, rad - 3.0), nlCode.Xyzsf.F * 3.0);
					this.AddG01(3.0 * Math.PI / 2.0, nlCode.Xyzsf.F, false);
					this.AddG01(rad * Math.PI * 2.0, nlCode.Xyzsf.F);
					this.AddG01(3.0 * Math.PI / 2.0, nlCode.Xyzsf.F, false);
					this.m_G00Z += S126 + Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['C'].D);
					this.m_G00X += Vector3.Length(3.0, rad - 3.0);
					this.MinMax(new Vector3(nlCode.Xyzsf.X + rad, nlCode.Xyzsf.Y + rad, nlCode.G6p['C'].D - S126));
					this.MinMax(new Vector3(nlCode.Xyzsf.X - rad, nlCode.Xyzsf.Y - rad, nlCode.G6p['C'].D - S126));
					break;
				case 8090:	// ヘリカル加工
					stt = nlCode.G6p['R'].Set ? nlCode.G6p['R'].D : nlCode.G6p['C'].D;
					lnum = (int)Math.Ceiling((stt - nlCode.G6p['Z'].D) / nlCode.G6p['Q'].D);
					rad = (nlCode.G6p['A'].D - nlCode.G6p['E'].D) / 2.0;
					this.m_G00Z += nlCode.Xyzsf.Z - stt;
					this.AddG01(CirApproach(rad), nlCode.Xyzsf.F, false);	// アプローチＸＹ
					this.AddG01(Vector3.Length(stt - nlCode.G6p['Z'].D, lnum * rad * Math.PI * 2.0), nlCode.Xyzsf.F);
					this.AddG01(CirApproach(rad), nlCode.Xyzsf.F, false);	// リトラクトＸＹ
					this.AddG01(CirApproach(rad), nlCode.Xyzsf.F, false);	// アプローチＸＹ
					this.AddG01(rad * Math.PI * 2.0, nlCode.Xyzsf.F);
					this.AddG01(CirApproach(rad), nlCode.Xyzsf.F, false);	// リトラクトＸＹ
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['Z'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X + rad, nlCode.Xyzsf.Y + rad, nlCode.G6p['Z'].D));
					this.MinMax(new Vector3(nlCode.Xyzsf.X - rad, nlCode.Xyzsf.Y - rad, nlCode.G6p['Z'].D));
					break;
				// 固定型ノズルテーパ孔ヘリカル加工
				case 8100:
					// 半径方向の切込み量
					kiri = nlCode.G6p['E'].D / 2.0 * 0.8;
					// 最大径
					radmax = (nlCode.G6p['A'].D - nlCode.G6p['E'].D) / 2.0;
					// 深さ方向のループ回数
					lnum = (int)Math.Ceiling((nlCode.G6p['C'].D - nlCode.G6p['Z'].D) / nlCode.G6p['Q'].D);
					// アプローチＺ
					this.AddG01(nlCode.Xyzsf.Z - nlCode.G6p['C'].D, 1000.0, false);
					// ヘリカル加工（ヘリカルの半径は中央値で近似する）
					radinc = 0.0;	// 半径中央値への差分
					if (nlCode.G6p['U'].Set)
						radinc = Math.Tan(nlCode.G6p['U'].D * Math.PI / 180.0) * (nlCode.G6p['C'].D - nlCode.G6p['Z'].D) / 2.0;
					for (int ii = 1; ii <= (int)Math.Ceiling(radmax / kiri); ii++) {
						rad = Math.Min(kiri * ii, radmax) - radinc;
						this.AddG01(CirApproach(rad + radinc), nlCode.Xyzsf.F, false);	// アプローチ
						// 距離 = Vector3.Length(Z, 2*PI*rad*lnum)
						this.AddG01(Vector3.Length(nlCode.G6p['C'].D - nlCode.G6p['Z'].D, 2.0 * Math.PI * rad * lnum), nlCode.Xyzsf.F);
						this.AddG01((rad - radinc) * Math.PI * 2.0, nlCode.Xyzsf.F);	// 円弧底面
						this.AddG01(CirApproach(rad - radinc), nlCode.Xyzsf.F, false);	// リトラクト
					}
					// リトラクトＺ
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['Z'].D);

					this.MinMax(new Vector3(nlCode.Xyzsf.X + radmax, nlCode.Xyzsf.Y + radmax, nlCode.G6p['Z'].D));
					this.MinMax(new Vector3(nlCode.Xyzsf.X - radmax, nlCode.Xyzsf.Y - radmax, nlCode.G6p['Z'].D));
					break;
				// ヘリカル加工
				case 8110:
				case 8120:
					kiri = 0.0;
					switch (nlCode.G6p.ProgNo) {
					case 8110:
						switch ((int)nlCode.G6p['E'].D) {
						case 40:
							switch ((int)nlCode.G6p['J'].D) {
							case 1: kiri = 1.5; break;
							case 2: kiri = 1.0; break;
							case 3: kiri = 0.8; break;
							}
							break;
						case 35:
							switch ((int)nlCode.G6p['J'].D) {
							case 1: kiri = 1.5; break;
							case 2: kiri = 1.0; break;
							case 3: kiri = 0.4; break;
							}
							break;
						case 26:
							switch ((int)nlCode.G6p['J'].D) {
							case 1: kiri = 1.2; break;
							case 2: kiri = 0.8; break;
							case 3: kiri = 0.15; break;
							}
							break;
						case 21:
							switch ((int)nlCode.G6p['J'].D) {
							case 1: kiri = 0.8; break;
							case 2: kiri = 0.4; break;
							case 3: kiri = 0.4; break;
							}
							break;
						}
						break;
					case 8120:
						switch ((int)nlCode.G6p['E'].D) {
						case 40:
							switch ((int)nlCode.G6p['J'].D) {
							case 1: kiri = 1.3; break;
							case 2: kiri = 1.0; break;
							case 3: kiri = 1.0; break;
							}
							break;
						case 35:
							switch ((int)nlCode.G6p['J'].D) {
							case 1: kiri = 1.2; break;
							case 2: kiri = 1.0; break;
							case 3: kiri = 0.4; break;
							}
							break;
						case 26:
							switch ((int)nlCode.G6p['J'].D) {
							case 1: kiri = 1.0; break;
							case 2: kiri = 0.6; break;
							case 3: kiri = 0.15; break;
							}
							break;
						case 21:
							switch ((int)nlCode.G6p['J'].D) {
							case 1: kiri = 0.6; break;
							case 2: kiri = 0.5; break;
							case 3: kiri = 0.5; break;
							}
							break;
						}
						break;
					}
					if (kiri == 0.0) throw new Exception("qwefqrfqherb");
					lnum = (int)Math.Ceiling((nlCode.G6p['C'].D - nlCode.G6p['Z'].D) / kiri);
					rad = (nlCode.G6p['A'].D - nlCode.G6p['E'].D) / 2.0;
					this.AddG01(nlCode.Xyzsf.Z - nlCode.G6p['C'].D, 1000.0, false);
					this.AddG01(CirApproach(rad), nlCode.Xyzsf.F, false);	// アプローチＸＹ
					this.AddG01(Vector3.Length(nlCode.G6p['C'].D - nlCode.G6p['Z'].D, lnum * rad * Math.PI * 2.0), nlCode.Xyzsf.F);
					this.AddG01(CirApproach(rad), nlCode.Xyzsf.F, false);	// リトラクトＸＹ
					this.AddG01(CirApproach(rad), nlCode.Xyzsf.F, false);	// アプローチＸＹ
					this.AddG01(rad * Math.PI * 2.0, nlCode.Xyzsf.F);
					this.AddG01(CirApproach(rad), nlCode.Xyzsf.F, false);	// リトラクトＸＹ
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['Z'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X + rad, nlCode.Xyzsf.Y + rad, nlCode.G6p['Z'].D));
					this.MinMax(new Vector3(nlCode.Xyzsf.X - rad, nlCode.Xyzsf.Y - rad, nlCode.G6p['Z'].D));
					break;
				case 8200:  // 深孔ドリル加工（２段目以降）
					clearz = 1.0;
					lnum = (int)Math.Floor((nlCode.G6p['K'].D - nlCode.G6p['Z'].D) / nlCode.G6p['Q'].D);
					this.m_G00Z += nlCode.Xyzsf.Z - nlCode.G6p['R'].D;
					this.AddG01(nlCode.G6p['R'].D - nlCode.G6p['K'].D, 200.0, false);
					this.AddG01((nlCode.G6p['K'].D - nlCode.G6p['Z'].D) + lnum * clearz, nlCode.Xyzsf.F);
					// 上下移動は
					//   Σ(n=0,lnum-1) (R-K+nQ)+(R-K+nQ-cl) =
					// = Σ(n=0,lnum-1) 2nQ+2R-2K-cl
					// = 2Q(lnum*(lnum-1)/2) + (2R-2K-cl)*lnum
					this.m_G00Z += nlCode.G6p['Q'].D * (lnum * (lnum - 1)) + (2 * nlCode.G6p['R'].D - 2 * nlCode.G6p['K'].D - clearz) * lnum;
					//this.m_G00Z += 2 * ((lnum + 1) * lnum / 2) * nlCode.g6p['Q'].d + 2 * (nlCode.g6p['R'].d - nlCode.g6p['K'].d) - lnum * clearz;
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['Z'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['Z'].D));
					break;
				case 8290:	// 超鋼ドリルマクロ
					clearz = 3.0;
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['R'].D);

					// 送り速度の間違いを修正 nlCode.xyzsf.F * 0.25 -> nlCode.xyzsf.F
					//this.AddG01(clearz - nlCode.g6p['K'].d, nlCode.xyzsf.F * 0.25, false);
					this.AddG01(clearz - nlCode.G6p['K'].D, nlCode.Xyzsf.F, false);	// 案内孔底への移動であるため切削送りであるが切削ではない

					this.m_dwell += 2.0 / 60.0;
					this.AddG01((nlCode.G6p['R'].D + (nlCode.G6p['K'].D - clearz)) - nlCode.G6p['Z'].D, nlCode.Xyzsf.F);
					this.AddG01(1.0, nlCode.Xyzsf.F, false);
					this.m_dwell += 2.0 / 60.0;
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['Z'].D) - 1.0;
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['Z'].D));
					break;
				case 8400:  // リジッドタップ
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['R'].D);
					this.AddG01(2 * Math.Abs(nlCode.G6p['R'].D - nlCode.G6p['Z'].D), nlCode.Xyzsf.F);
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['R'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['Z'].D));
					break;
				case 8401:  // ヘリカルタップ
				case 8402:  // ヘリカルタップ
					DataRow helicaltap = CamUtil.CamNcD.KijunSize.HelicalTap(nlCode.G6p.ProgNo.ToString(), nlCode.G6p['E'].D);
					rad = ((double)helicaltap["27基準径"] - (double)helicaltap["31工具先端径"]) / 2.0;
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['R'].D);
					this.m_G00Z += 3.0 + (double)helicaltap["30基準孔深さ"] + (double)helicaltap["29ピッチ"];
					double rtmp = Vector3.Length(rad * Math.PI * 2.0, (double)helicaltap["29ピッチ"]);
					double feed;
					feed = this.frate.HasValue ? this.frate.Value * rad / ((double)helicaltap["27基準径"] / 2) : 0.0;
					this.AddG01(rtmp, feed, true);		// 実切削
					feed = this.frate.HasValue ? this.frate.Value * rad / ((double)helicaltap["27基準径"] - rad) : 0.0;
					this.AddG01(rtmp / 1.5, feed, false);	// アプローチ＆リトラクト（概算）
					this.m_G00Z += 3.0 + (double)helicaltap["30基準孔深さ"];
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['R'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['R'].D - 3.0 - (double)helicaltap["30基準孔深さ"] - (double)helicaltap["29ピッチ"]));
					break;
				case 8900:  // 多段孔加工（１段目）
				case 9999:  // テスト用マクロ
					clearz = 1.0;
					lnum = (int)Math.Floor((nlCode.G6p['R'].D - nlCode.G6p['Z'].D) / nlCode.G6p['Q'].D / 3.0);
					this.m_G00Z += nlCode.Xyzsf.Z - nlCode.G6p['R'].D;
					this.AddG01((nlCode.G6p['R'].D - nlCode.G6p['Z'].D) + lnum * clearz, nlCode.Xyzsf.F);
					this.m_dwell += lnum * 3 * 0.2 / 60;
					// (1*Q*3+cl)+(1*Q*3) + (2*Q*3+cl)+(2*Q*3) + (3*Q*3+1)+(3*Q*3) + .... +(lnum*Q*3+cl)+(lnum*Q*3)
					// Σ(n=1,lnum) (n*Q*3+cl)+(n*Q*3)
					// Σ(n=1,lnum) (n*Q*3*2+cl)
					// ((Q*3*2)+(lnum*Q*3*2))*lnum/2 + lnum*cl
					// 2*(Q*3)(1+lnum)*lnum/2 + lnum*cl
					this.m_G00Z += 2 * ((lnum + 1) * lnum / 2) * nlCode.G6p['Q'].D * 3 + lnum * clearz;
					//this.m_G00Z += 2 * ((lnum + 1) * lnum / 2) * nlCode.g6p['Q'].d - lnum * clearz;
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['Z'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['Z'].D));
					break;
				case 8700:  // 多段孔加工（２段目以降）
					clearz = 1.0;
					lnum = (int)Math.Floor((nlCode.G6p['K'].D - nlCode.G6p['Z'].D) / nlCode.G6p['Q'].D / 3.0);
					this.m_G00Z += nlCode.Xyzsf.Z - nlCode.G6p['R'].D;
					this.AddG01(nlCode.G6p['R'].D - nlCode.G6p['K'].D, 200.0, false);
					this.m_G00Z += 2 * (nlCode.G6p['R'].D - nlCode.G6p['K'].D);
					this.AddG01((nlCode.G6p['K'].D - nlCode.G6p['Z'].D) + lnum * clearz, nlCode.Xyzsf.F);
					this.m_dwell += lnum * 3 * 0.2 / 60;
					// (1*Q*3+(R-K))+(1*Q*3+(R-K-1)) + .... + (lnum*Q*3+(R-K))+(lnum*Q*3+(R-K-1))
					// Σ(n=1,lnum) (n*Q*3+(R-K))+(n*Q*3+(R-K-1))
					// Σ(n=1,lnum) (n*Q*3)*2+(R-K)*2-1)
					// (Q*3*2+lnum*Q*3*2)*lnum/2 + lnum*(2*R-2*K-1)
					// 2*(1+lnum)*lnum/2*(Q*3) + 2*lnum*(R-K)-lnum
					this.m_G00Z += 2 * ((lnum + 1) * lnum / 2) * nlCode.G6p['Q'].D * 3 + 2 * lnum * (nlCode.G6p['R'].D - nlCode.G6p['K'].D) - lnum * clearz;
					//this.m_G00Z += 2 * ((lnum + 1) * lnum / 2) * nlCode.g6p['Q'].d + 2 * (nlCode.g6p['R'].d - nlCode.g6p['K'].d) - lnum * clearz;
					this.m_G00Z += Math.Abs(nlCode.Xyzsf.Z - nlCode.G6p['Z'].D);
					this.MinMax(new Vector3(nlCode.Xyzsf.X, nlCode.Xyzsf.Y, nlCode.G6p['Z'].D));
					break;
				/*
				case 8280:  // ガンドリル
					clearz = 5.0;
					if (nlCode.g6p['Q'].set) {
						lnum = (int)Math.Floor((nlCode.g6p['K'].d - nlCode.g6p['Z'].d) / nlCode.g6p['Q'].d);
						if (lnum < 1) lnum = 1;
					}
					else lnum = 1;
					this.m_G00Z += nlCode.xyzsf.Z - nlCode.g6p['R'].d;
					this.m_dwell += 0.5 / 60;
					this.AddG01(nlCode.g6p['R'].d - nlCode.g6p['K'].d, 200.0, false);
					this.m_dwell += 0.5 / 60;
					this.AddG01((nlCode.g6p['K'].d - nlCode.g6p['Z'].d) + lnum * clearz, nlCode.xyzsf.F);
					this.m_dwell += lnum * (0.5 + 0.2) / 60;
					this.AddG01(((lnum + 1) * lnum / 2) * nlCode.g6p['Q'].d, 1000.0, false);
					this.AddG01(((lnum + 1) * lnum / 2) * nlCode.g6p['Q'].d - lnum * clearz, 700.0, false);
					this.m_G00Z += Math.Abs(nlCode.xyzsf.Z - nlCode.g6p['K'].d);
					this.MinMax(new Vector3(nlCode.xyzsf.X, nlCode.xyzsf.Y, nlCode.g6p['Z'].d));
					break;
				*/
				case 8085:	// 面取り
				default:
					throw new Exception(nlCode.G6p.ProgName + " は使用できないマクロです。３");
				}
			}
			// 直線オフセット開始と円弧アプローチの距離
			private double CirApproach(double radius) {
				return radius / 2 * Math.Sqrt(2.0) + radius * Math.PI / 4.0;
			}
			/// <summary>
			/// 新たな座標値で最大最小の座標値を更新します
			/// </summary>
			/// <param name="xyz"></param>
			private void MinMax(Vector3 xyz) {
				MaxAdd(xyz);
				MinAdd(xyz);
				// ////////////
				// depthの計算
				// ////////////
				if (rotAxis[keishaNo].Rot) {
					Vector3 tvec = rotAxis[keishaNo].ToolDir();	// 工具軸ベクトル
					if (Math.Abs(tvec.Z) > 0.0001) {
						// Ｚ＝０平面との交点 w0
						Vector3 w0 = new Vector3(
							xyz.X - tvec.X * xyz.Z / tvec.Z,
							xyz.Y - tvec.Y * xyz.Z / tvec.Z,
							0.0);
						w0 = rotAxis[keishaNo].Transform(RotationAxis.TRANSFORM.WorkToFeature, w0);
						depth = Math.Max(depth, w0.Z - xyz.Z);
					}
				}
				else
					depth = Math.Max(depth, Min.HasValue ? -Min.Value.Z : Double.MinValue);
			}
			/// <summary>
			/// 送り速度を指定して直線円弧補間の加工長と加工時間を積算します
			/// </summary>
			/// <param name="kyori">加工長</param>
			/// <param name="feed2">送り速度</param>
			private void AddG01(double kyori, double? feed2) {
				AddG01(kyori, feed2, feed2.HasValue && this.frate.HasValue ? Math.Abs(feed2.Value - this.frate.Value) < 1.0 : false);
			}
			/// <summary>
			/// 送り速度と切削加工か否かを指定して直線円弧補間の加工長と加工時間を積算します
			/// </summary>
			/// <param name="kyori">加工長</param>
			/// <param name="feed2">送り速度</param>
			/// <param name="cutting">切削加工の場合はtrue</param>
			private void AddG01(double kyori, double? feed2, bool cutting) {
				if (kyori < 0.0) {
					System.Windows.Forms.DialogResult result =
						System.Windows.Forms.MessageBox.Show("孔あけ加工で、加工開始点より終了点が上にある箇所が見つかりました。続行しますか", "DRILING ERROR",
							System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Question);
					if (result != System.Windows.Forms.DialogResult.OK)
						throw new Exception("孔あけ加工で、加工開始点より終了点が上にある箇所が見つかりました。");
				}
				this.m_G01 += kyori;

				if (this.frate.HasValue) {
					if (!feed2.HasValue) {
						feed2 = 10.0;
					}
					if (feed2.Value <= 0.0)
						throw new Exception("qdfcqhwdfcbqhdcbqhdcqhdcbh");
					if (cutting)
						this.m_TimC += kyori / feed2.Value;
					else
						this.m_TimN += kyori / feed2.Value;
				}
			}


			/// <summary>このインスタンスと指定した NcDist オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
			/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
			/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
			public override bool Equals(object obj) { return Equals((NcLineCode.NcDist)obj); }
			private bool Equals(NcLineCode.NcDist obj) {
				if (this.m_G00X != obj.m_G00X) return false;
				if (this.m_G00Z != obj.m_G00Z) return false;
				if (this.m_G01 != obj.m_G01) return false;
				if (this.m_max != obj.m_max) return false;
				if (this.m_min != obj.m_min) return false;
				if (this.m_TimC != obj.m_TimC) return false;
				if (this.m_TimN != obj.m_TimN) return false;
				if (this.m_dwell != obj.m_dwell) return false;
				if (this.depth != obj.depth) return false;
				return true;
			}
			/// <summary>このインスタンスのハッシュ コードを返します。</summary>
			/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
			public override int GetHashCode() {
				return
					m_G00X.GetHashCode() ^
					m_G00Z.GetHashCode() ^
					m_G01.GetHashCode() ^
					m_max.GetHashCode() ^
					m_min.GetHashCode() ^
					m_TimC.GetHashCode() ^
					m_TimN.GetHashCode() ^
					m_dwell.GetHashCode() ^
					depth.GetHashCode();
			}
		}

		
		/// <summary>
		/// 円弧補間NcLineCodeの工具軌跡回転中心点を計算します
		/// </summary>
		/// <returns>円弧補間の工具軌跡回転中心座標値</returns>
		public Vector3 Center() {
			Vector3 cntr;
			Vector3 svec = Xyzsf.PreToXYZ();
			Vector3 evec = Xyzsf.ToXYZ().ToVector2(G2).ToVector3(G2, svec);

			// 円弧補間のＲ指定の場合
			if (B_26('R')) {
				double rad = Code('R').dblData;
				CamUtil._Geometry_Plane.Circle circle;
				if ((G1 == 2 && rad > 0.0) || (G1 == 3 && rad < 0.0))
					circle = new _Geometry_Plane.Circle(svec.ToVector2(G2), evec.ToVector2(G2), Math.Abs(rad), false);		// 進行方向右側の中心の円
				else
					circle = new _Geometry_Plane.Circle(svec.ToVector2(G2), evec.ToVector2(G2), Math.Abs(rad), true);		// 進行方向左側の中心の円
				cntr = circle.Center.ToVector3(G2, svec);
				// チェック
				if (Math.Abs(((Vector3)(evec - cntr)).Abs - Math.Abs(rad)) > 0.001) throw new Exception("awefbqerhfbqwrh");
				if (Math.Abs(((Vector3)(svec - cntr)).Abs - Math.Abs(rad)) > 0.001) throw new Exception("awefbqerhfbqwrh");
			}
			// 円弧補間のＩＪＫ指定の場合
			else {
				cntr = svec + IJK;
				// チェック
				switch (G2) {
				case 17: if (B_26('K')) throw new Exception("wjefqwegh"); break;
				case 18: if (B_26('J')) throw new Exception("wjefqwegh"); break;
				case 19: if (B_26('I')) throw new Exception("wjefqwegh"); break;
				}
				if (!B_26('I') && !B_26('J') && !B_26('K')) throw new Exception("半径が０の円弧補間が見つかりました。処理を中止します");
			}
			return cntr;
		}

		/// <summary>
		/// 円弧補間NcLineCodeの工具軌跡半径を計算します
		/// </summary>
		/// <returns>円弧補間の工具軌跡半径</returns>
		public double Radius() {
			double rad;
			// 円弧補間のＲ指定の場合
			if (B_26('R')) {
				rad = Math.Abs(Code('R').dblData);
			}
			// 円弧補間のＩＪＫ指定の場合
			else {
				rad = this.IJK.ToVector2(G2).Abs;
			}
			return rad;
		}

		/// <summary>
		/// ヘリカル加工を含む円弧補間NcLineCodeの回転角度を計算します（反時計回りG03を正とします）
		/// </summary>
		/// <param name="cntr">回転中心座標値</param>
		/// <returns>円弧補間の回転角度（単位：radian）</returns>
		public double Angle(Vector3 cntr) {
			double ang;

			// １回転以下の角度の計算
			Vector3 svec = Xyzsf.PreToXYZ() - cntr;
			Vector3 evec = Xyzsf.ToXYZ() - cntr;
			if (Xyzsf.Dist.Abs == 0.0)
				ang = (G1 == 2 ? -2 * Math.PI : 2 * Math.PI);
			else {
				ang = _Geometry_Plane.Vangle(svec.ToVector2(G2), evec.ToVector2(G2), G1);
				if (Math.Min(2 * Math.PI - Math.Abs(ang), Math.Abs(ang)) * svec.Abs < 0.001) throw new Exception("微小円弧の処理エラー");
			}

			// 回転回数の計算
			long nn;	// 回転の回数
			switch (ncForm.Id) {
			case BaseNcForm.ID.GENERAL:			// FANUCの仕様
				if (B_26('L'))
					nn = Code('L').L;	// FANUMの場合端数は切り上げ
				else if (B_26('Q'))
					throw new Exception("未対応");
				else
					nn = 1;
				break;
			case BaseNcForm.ID.BUHIN:				// MELDASの仕様
				if (B_26('L')) {
					nn = Code('P').L;
					if (Math.PI * 2 - Math.Abs(ang) > 0.001)
						nn++;					// MELDASの場合端数は切り捨てとなるため調整
				}
				else
					nn = 1;
				break;
			default: throw new Exception("qkfewqewfh");
			}

			ang += (G1 == 2 ? -1 : 1) * (nn - 1) * Math.PI * 2;	// 回転角度の総計
			if (nn > 1)
				LogOut.CheckCount("NcLineCode_dist 1138", false, "１回転以上のヘリカル加工");

			return ang;
		}
	}
}
