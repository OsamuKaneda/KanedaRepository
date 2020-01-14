using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.IO;

namespace NcTejun.Output
{
	/// <summary>
	/// テキサスのインデクスファイルの工具単位情報（ＤＭＧ用の追加）[不変]
	/// </summary>
	class Index
	{
		/// <summary>インデックスファイル用の情報</summary>
		static public Index_Main IndexMain { get { return m_indexMain; } }
		/// <summary>インデックスファイル用の情報の作成（部品）</summary>
		static public void SetMain_BU(FormNcSet_Buhin frm) { m_indexMain = new Index_Main(frm); }
		/// <summary>インデックスファイル用の情報の作成（主型）</summary>
		static public void SetMain_OM(FormNcSet_Texas frm) { m_indexMain = new Index_Main(frm); }
		static private Index_Main m_indexMain = null;





		/// <summary>
		/// 消耗率（単位：％）
		/// ＴＥＸＡＳはRATEを整数として受け取るので整数に丸める
		/// また、工具数で分割するときは切り捨てとする
		/// </summary>
		public double Life_rate {
			get {
				// １％以下は四捨五入とする
				if (m_life_rate < 1.0) return Math.Round(m_life_rate);
				// １％以上は切り捨てる
				return Math.Floor(m_life_rate);
			}
		}
		private readonly double m_life_rate;

		/// <summary>工具番号</summary>
		public readonly int tno;
		/// <summary>工具径補正番号</summary>
		public readonly int h;
		/// <summary>次の工具番号</summary>
		public readonly int next;

		/// <summary>工具交換時間を含めた加工時間（自動工具分割考慮）</summary>
		public readonly double atime;
		/// <summary>実切削加工時間（自動工具分割考慮）</summary>
		public readonly double rtime;

		/// <summary>回転数、送り速度の値</summary>
		public readonly int spin, feed;

		public readonly int type;
		public readonly int toth;

		/// <summary>
		/// 加工方向
		/// 0:自由（立置き）、1:自由（平置き）、11:自由（平置き,上面基準）、
		/// 2:表加工、3:裏加工、4:正面、5:背面、6:右側面、7:左側面
		/// </summary>
		public readonly int coordinate;
		/// <summary>工具の参照点（0:工具先端  1:工具中心）</summary>
		public readonly int refpoint;

		/// <summary>加工モード（ＤＭＧ以外はsgi1のみ使用））</summary>
		public readonly int sgi1;
		public readonly int sgi2;
		/// <summary>クーラント仕様</summary>
		public readonly int coolant;

		/// <summary>クリアランスプレーン（Ｚ）</summary>
		public readonly double clearance_plane;

		// 常時セットの工具であればtrue（加工機に依存する）add 2013/03/15
		public readonly bool perm_tool;

		// 追加 2016/04/08

		/// <summary>工具半径補正量</summary>
		public readonly double? hosei_r;
		/// <summary>工具長さ補正量</summary>
		public readonly double? hosei_l;

		/// <summary>加工手順の次元設定値＝座標系設定に使用する</summary>
		public readonly int dimensionAXIS;
		/// <summary>同時５軸ではない場合は０</summary>
		public readonly int simultaneous;

		/// <summary>
		/// 唯一のコンストラクタ
		/// </summary>
		/// <param name="ncoutName"></param>
		/// <param name="mach"></param>
		/// <param name="keisha"></param>
		/// <param name="tjnHoko"></param>
		/// <param name="ncoutNext"></param>
		/// <param name="mess">クーラントが設定された値を出力する。[0]はツールセットＣＡＭの初期値、[1]は最終決定値</param>
		public Index(NcOutput.NcToolL ncoutName, NcdTool.Mcn1 mach, bool keisha, bool tjnHoko, NcOutput.NcToolL ncoutNext, string[] mess) {
			// 工具交換含めた加工時間の計算
			if (ncoutName.Smch.Atim > 0.0) {
				this.atime = ncoutName.Smch.Atim;
				if (mach.ID == CamUtil.Machine.MachID.D500 || mach.ID == CamUtil.Machine.MachID.LineaM) {
					this.atime *= ncoutName.tolInfo.TsetCAM.Gensoku;
				}
				this.atime += mach.Tctim / 60.0;
				if (this.atime.ToString("0") == "0")
					this.atime = 1.0;
			}
			else
				this.atime = 0.0;

			// 実切削加工時間の計算
			this.rtime = ncoutName.Smch.Rtim;

			// /////////////////////////////////
			// 工具番号と径補正番号
			// /////////////////////////////////
			if (ncoutName.Skog == null)
				throw new Exception("すべてskogを作成すること");
			if (ncoutName.Skog.matchK[0] == null)
				throw new Exception("すべてtMatchを設定すること");

			// ＤＮＣ運転
			// ＲＦＩＤ無し（Ｅ５０）
			// １．ＲＦＩＤなしの工具を工具管理からはずす。（工具不足も考慮しない）
			// ２．加工時はインデックスファイルの工具番号（ｔ）をそのまま用いる。
			// ３．ＲＦＩＤなし工具の判断基準は工具番号（t）が２０１以上。

			// ftn番号（参考値）とlife_rateの決定（標準工具A100と共用）
			m_life_rate = ncoutName.Smch.divData.Consumption;

			// 工具番号の設定
			{
				perm_tool = ncoutName.Smch.K2.Tlgn.Perm_tool;
				// 工具表が存在しかつメモリ運転の場合（ＤＭＧの場合、リザーブ工具以外は２００をプラスする）
				tno = TnoSet2(ncoutName.Smch.K2);
				next = ncoutNext != null ? TnoSet2(ncoutNext.Smch.K2) : 0;
				h = tno + 50;
			}

			this.spin = ncoutName.Skog.CutSpinRate();
			this.feed = ncoutName.Skog.CutFeedRate();

			switch (mach.ID) {
			case CamUtil.Machine.MachID.DMU200P:
			case CamUtil.Machine.MachID.DMU210P:
			case CamUtil.Machine.MachID.DMU210P2:
				this.type = Convert.ToInt32(ncoutName.tolInfo.Toolset.MeasTypeDMG) - 0x40;
				break;
			default:
				this.type = Convert.ToInt32(ncoutName.tolInfo.Toolset.MeasType) - 0x40;
				break;
			}

			// 刃数？
			this.toth = 0;

			// 加工方向
			switch (NcdTool.Tejun.BaseNcForm.Id) {
			case CamUtil.BaseNcForm.ID.GENERAL:
			case CamUtil.BaseNcForm.ID.BUHIN:
				// 手順書での加工方向設定（現在未実施）
				if (tjnHoko) {
					CamUtil.LogOut.CheckCount("Index 177", false, "tjnHoko==True");
					if (keisha) throw new Exception("qerfbqerfbh");
					this.coordinate = ncoutName.Ncnam.nmgt.Omoteura.Value + 2;
				}
				else if (keisha)
					this.coordinate = 11;   // 上面基準の傾斜加工
				else
					this.coordinate = IndexMain.kakoHoko + 2;
				if (this.coordinate < 2) throw new Exception("qregbqegrbwhergbwre");
				break;
			default:
				throw new Exception("wfwfebwhjbfqre");
			}

			// CLEARANCE PLANE
			this.clearance_plane = ncoutName.Skog.Tld.XmlT.TlAPCHZ;
			if (this.coordinate == 0 || this.coordinate == 1 || this.coordinate == 11) {
				;
			}
			else {
				// ケーラム出力のデータなどを調査するまでの暫定処置 2014/03/03
				if (this.clearance_plane < 19.9) {
					CamUtil.LogOut.CheckCount("Index 201", false,
						$"{ncoutName.Ncnam.nnam}({ncoutName.Ncnam.Ncdata.ncInfo.xmlD.CamSystemID.Name}) " +
						$"暫定として19.9以下のclearance_plane={this.clearance_plane:0}を100.0に変更した。");
					this.clearance_plane = 100.0;
				}
			}


			// ＤＭＧのプローブの場合、加工機の測定マクロ側で刃先の補正が
			// 行われるため、全て先端（０）とする
			// その他ＶＧのプローブの場合は元々先端である（これは間違いでは？？2013/04/19）
			if (ncoutName.tolInfo.Toolset.ToolName == "TS640")
				this.refpoint = 0;
			else
				this.refpoint = (ncoutName.Skog.Tld.XmlT.TRTIP ? 0 : 1);

			// 加工精度の番号を取得
			this.sgi1 = mach.ScaleNo(ncoutName.tolInfo.TsetCAM.Accuracy);
			if (mach.Dmu) {
				this.sgi2 = 3;
			}
			else {
				this.sgi2 = 0;  // sgi2は使用しない
			}
			this.coolant = mach.CoolCodeNo(ncoutName.tolInfo.TsetCAM.coolant.id, mess);
			switch (NcdTool.Tejun.BaseNcForm.Id) {
			case CamUtil.BaseNcForm.ID.GENERAL:
				if (ncoutName.DimensionG01 != 2) {
					if (this.type == 6) {
						throw new Exception(
							$"{ncoutName.Ncnam.nnam}の仕上げ用工具({ncoutName.tolInfo.Toolset.ToolName})はＤＭＧの３次元加工モードでは使用できません。NCMNにおいて'2D'の指定をして使用してください。");
					}
					if (ncoutName.tolInfo.Toolset.Probe)
						throw new Exception(
							"プローブサイクルは２Ｄ加工モードで行う必要があります。" +
							"NCMNにおいて'2D'の指定をして使用してください。");
				}
				break;
			}

			// 工具径補正量
			if (ncoutName.tolInfo.TsetCAM.KouteiType == "タップ" && mach.Dmu == false)
				hosei_r = (double?)null;            // DMU200P以外のタップの径補正は消去する
			else
				hosei_r = ncoutName.tolInfo.TsetCAM.RadRevision;
			// 工具長補正量
			hosei_l = ncoutName.Ncnam.nggt.ToolLengthHosei.ValueHosei(ncoutName.Skog);
			if (hosei_l == 0.0)
				hosei_l = ncoutName.tolInfo.TsetCAM.LenRevision;
			// 次元（加工軸の設定）
			dimensionAXIS = ncoutName.Ncnam.Ncdata.ncInfo.xmlD.CamDimension;
			// 同時５軸
			simultaneous = ncoutName.Skog.Tld.XmlT.SimultaneousAxisControll ? 1 : 0;
		}

		/// <summary>工具番号の開始初期値を考慮した工具番号を決めます</summary>
		/// <param name="k2"></param>
		/// <returns>工具番号</returns>
		private int TnoSet2(NcdTool.TMatch.MatchK.MatchK2 k2) { return k2.Tlgn.Perm_tool ? k2.Tnum : k2.Tnum + NcdTool.ToolSetInfo.TnoStart; }

		/// <summary>
		/// Ｘプラス側で加工する場合は+1、Ｘマイナス側で加工する場合は-1を返す
		/// </summary>
		public int XPLUSMINUS() {
			if (IndexMain.kotkad) {
				// 固定型
				switch (IndexMain.progress) {
				case "1":
					// 1:平置き（表が上）
					switch (this.coordinate) {
					case 1: return +1;
					case 11: return +1;
					case 2: return +1;
					case 4: return +1;
					case 5: return -1;
					case 6: return +1;
					case 7: return -1;
					case 3:
					default:
						throw new Exception("wefarfre");
					}
				case "2":
					// 2:平置き（裏が上）
					switch (this.coordinate) {
					case 1: return -1;
					case 11: return -1;
					case 3: return -1;
					case 4: return -1;
					case 5: return +1;
					case 6: return +1;
					case 7: return -1;
					case 2:
					default:
						throw new Exception("wefarfre");
					}
				default: throw new Exception("awefqberfqwebrhfb");
				}
			}
			else {
				// 可動型
				switch (IndexMain.progress) {
				case "1":
					// 1:平置き（表が上）
					switch (this.coordinate) {
					case 1: return -1;
					case 11: return -1;
					case 2: return -1;
					case 4: return -1;
					case 5: return +1;
					case 6: return +1;
					case 7: return -1;
					case 3:
					default:
						throw new Exception("wefarfre");
					}
				case "2":
					// 2:平置き（裏が上）
					switch (this.coordinate) {
					case 1: return +1;
					case 11: return +1;
					case 3: return +1;
					case 4: return +1;
					case 5: return -1;
					case 6: return +1;
					case 7: return -1;
					case 2:
					default:
						throw new Exception("wefarfre");
					}
				default: throw new Exception("awefqberfqwebrhfb");
				}
			}
		}
		
		internal class ListTnoTable
		{
			/// <summary>工具表無しの場合のＥ５０とメモリ運転時の開始工具番号－１（ >=200 ）</summary>
			static private int startTno;

			/// <summary>
			/// メモリ運転時における自動設定の工具番号開始位置を決める
			/// </summary>
			static public void ToolMaxNo(List<NcOutput.NcToolL> nList) {

				startTno = NcdTool.ToolSetInfo.TnoStart;

				// 部分的に工具表のないものも考慮し、自動設定の工具番号開始位置を決める
				foreach (NcOutput.NcToolL nout in nList) {
					if (nout.Smch.K2.Tlgn.Perm_tool) {
						if (startTno < nout.Smch.K2.Tnum)
							startTno = nout.Smch.K2.Tnum;
					}
					else {
						if (startTno < nout.Smch.K2.Tnum + NcdTool.ToolSetInfo.TnoStart)
							startTno = nout.Smch.K2.Tnum + NcdTool.ToolSetInfo.TnoStart;
					}
				}
			}
		}
	}
}
