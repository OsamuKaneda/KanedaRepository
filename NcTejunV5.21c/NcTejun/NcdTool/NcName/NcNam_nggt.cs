using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.IO;
using CamUtil;
using CamUtil.CamNcD;

namespace NcdTool.NcName
{
	partial class NcNam
	{
		/// <summary>加工手順でＮＣに設定された情報</summary>
		public readonly struct St_nggt
		{
			/// <summary>ＣＬの移動指示</summary>
			public readonly Vector3 trns;

			/// <summary>原点でミラー処理するNcZahyo値。ミラー基準を返す。しない場合はnull</summary>
			public NcZahyo Mirr { get { return new NcZahyo(mirX ? 0.0 : (double?)null, mirY ? 0.0 : (double?)null, (double?)null); } }
			private readonly bool mirX, mirY;

			/// <summary>加工サイクルのマクロ反転</summary>
			public readonly bool rev;

			/// <summary>材質グループ名</summary>
			public readonly string zaisGrp;

			/// <summary>工具長補正量</summary>
			public readonly LHosei ToolLengthHosei;
			public struct LHosei
			{
				/// <summary>工具長補正なしを取得または設定します。ただし、false を設定することはできません。</summary>
				public bool Zero {
					get { return auto == false && hosei == 0.0; }
					set { if (value == false) throw new Exception("工具長補正エラー"); auto = false; hosei = 0.0; }
				}
				/// <summary>j自動工具長補正か否かを取得または設定します</summary>
				public bool Auto {
					get { return auto; }
					set { auto = value; if (value) hosei = 0.0; }
				}
				/// <summary>j工具長補正量を取得または設定します。工具長補正なしの場合、0.0を取得または設定します。自動の場合はエラーとなります</summary>
				public double ValueHosei() { if (auto) throw new Exception("工具長補正エラー"); return hosei; }
				/// <summary>j工具長補正量を取得または設定します。工具長補正なしの場合、0.0を取得または設定します。</summary>
				public double ValueHosei(Kogu skog) { return auto ? (skog.Tld.XmlT.ClMv_Offset == 0.0 ? 0.0 : hosei - skog.Tld.XmlT.ClMv_Offset) : hosei; }
				/// <summary>j工具長補正値を加える</summary>
				public void Add(double value) { hosei += value; }

				private bool auto;
				private double hosei;
			}

			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="ff">手順のＮＣ設定行</param>
			/// <param name="setZais">材質名</param>
			public St_nggt(string[] ff, string setZais) {
				zaisGrp = Material.ZgrpgetPC(setZais);

				// ////////////////////////////////////////////
				// 以下はミラー、反転、移動、工具長補正の設定
				// ////////////////////////////////////////////
				rev = false;
				trns = Vector3.v0;
				mirX = mirY = false;
				//ToolLengthHosei = 0.0;
				ToolLengthHosei = new LHosei() { Zero = true };

				int ii, jj;
				char ctmp;
				string stmp;

				ii = 0;
				jj = 1;
				while (++jj < ff.Length) {
					stmp = ff[jj];
					if (stmp == "ON" || stmp == "MIR") {
						if (ii > 1)
							continue;
						if (ii == 0) mirX = true;
						if (ii == 1) mirY = true;
						ii++;
					}
					else if (stmp == "OFF") {
						if (ii > 1)
							continue;
						ii++;
					}
					else if (stmp[0] == 'X' || stmp[0] == 'Y' || stmp[0] == 'Z') {
						ctmp = stmp[0];
						if (stmp.Length == 1) {
							//stmp = strtok(NULL, " \t");
							jj++;
							stmp = ff[jj];
						}
						else
							stmp = stmp.Substring(1);
						switch (ctmp) {
						case 'X': trns = new Vector3(trns.X + Convert.ToDouble(stmp), trns.Y, trns.Z); break;
						case 'Y': trns = new Vector3(trns.X, trns.Y + Convert.ToDouble(stmp), trns.Z); break;
						case 'Z': trns = new Vector3(trns.X, trns.Y, trns.Z + Convert.ToDouble(stmp)); break;
						}
					}
					else if (stmp == "REV") {
						rev = true;
					}
					else if (stmp[0] == 'L') {
						if (stmp.Length == 1) {
							jj++;
							stmp = ff[jj];
						}
						else
							stmp = stmp.Substring(1);
						if (stmp == "AUTO") {
							//ToolLengthHosei = 0.0;
							ToolLengthHosei.Auto = true;
						}
						else {
							//ToolLengthHosei += Convert.ToDouble(stmp);
							ToolLengthHosei.Add(Convert.ToDouble(stmp));
						}
					}
					else {
						throw new Exception("NC MIRROR DATA(" + stmp + ") ERROR.");
					}
				}
			}

			/// <summary>このインスタンスと指定した st_nggt オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
			/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
			/// <returns>obj が CamUtil.Machine のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
			private bool Equals(St_nggt obj) {
				if (this.trns != obj.trns) return false;
				if (this.mirX != obj.mirX) return false;
				if (this.mirY != obj.mirY) return false;
				if (this.rev != obj.rev) return false;
				if (this.zaisGrp != obj.zaisGrp) return false;
				if (LHosei.Equals(this.ToolLengthHosei, obj.ToolLengthHosei) == false) return false;
				//if (this.m_sorg0 != obj.m_sorg0) return false;
				//if (this.m_sorg1 != obj.m_sorg1) return false;
				//if (this.m_sorg2 != obj.m_sorg2) return false;
				//if (this.m_sorg3 != obj.m_sorg3) return false;
				return true;
			}
			/*
			/// <summary>このインスタンスのハッシュ コードを返します。</summary>
			/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
			public override int GetHashCode() {
				return
					this.m_sorg0.GetHashCode() ^ this.m_sorg1.GetHashCode() ^ this.m_sorg2.GetHashCode() ^ this.m_sorg3.GetHashCode() ^
					this.nmod.GetHashCode() ^ this.m_trns.GetHashCode() ^ this.mirX.GetHashCode() ^ this.mirY.GetHashCode() ^
					this.rev.GetHashCode() ^ this.m_zaisGrp.GetHashCode() ^ m_ToolLengthHosei.GetHashCode();
			}
			*/
		}
	}
}
