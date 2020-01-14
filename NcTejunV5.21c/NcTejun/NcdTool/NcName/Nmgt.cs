using System;
using System.Collections.Generic;
using System.Text;

using CamUtil;
using System.Windows.Forms;

namespace NcdTool.NcName
{
	/// <summary>
	/// 加工手順データ内の情報を保存するためのストラクチャ。最終的にはNcNam内に保存される。
	/// </summary>
	class Nmgt
	{
		// ///////////////////////////////////////
		// １ショット値（otClear にてリセットする）
		// ///////////////////////////////////////
		/// <summary>最初のＮＣデータが使用する工具表のシート番号。ディフォルトは１。（これにより工具表作成時に処理を行う）</summary>
		internal int? Tsn { get; private set; }

		/// <summary>コメント、出力シートの区切りに用いる(nullは区切り無し</summary>
		internal string Comnt { get; private set; }

		/// <summary>Ｏ番号の初期値</summary>
		internal int? OnumS { get { return m_osnum?[0]; } }
		/// <summary>Ｏ番号の増分値）</summary>
		internal int? OnumI { get { return m_osnum?[1]; } }
		private int[] m_osnum;  /* O No. START,INC NUMBER */


		// ///////////////////////////////////////
		// モーダル値
		// ///////////////////////////////////////
		private string tolst;   // 外から参照はしない

		/// <summary>ＮＣデータ検証のためのワークと冶具の設定ファイル名</summary>
		internal string Wrkjg { get; private set; }

		/// <summary>手順書で設定された回転速度比率</summary>
		public double Sratt { get; private set; }

		/// <summary>手順書で設定された送り速度比率</summary>
		public double Fratt { get; private set; }
		/// <summary>
		/// ２Ｄ、３Ｄの区分でG00のG01への変換と加工時間算出方法に使用する
		/// （座標設定＋工具参照点はncInfo.xmlD.camDimensionで、工具径補正の有無は両者で決定する）
		/// </summary>
		public char Dimn { get; private set; }

		/// <summary>KAKO BUHIN SUU</summary>
		public int Wnum { get; private set; }

		/// <summary>同一ツールシートの繰り返し使用数（普通は０）</summary>
		public int Trept { get; private set; }

		/// <summary>
		/// 加工方向　0=表面, 1=裏面, 2=正面, 3=背面, 4=右側面, 5=左側面
		/// </summary>
		public short? Omoteura { get; private set; }

		// そのままgmodをコピーするために追加
		/// <summary>材質名</summary>
		public string SetZais { get; private set; }

		// コンストラクタ
		public Nmgt(int dummy) {
			Tsn = null;
			// 最初はコメントを入れる
			Comnt = "";
			m_osnum = null;

			Trept = 0;
			this.tolst = null;

			Sratt = 1.0;
			Fratt = 1.0;
			Dimn = '2';
			Wnum = 1;
			SetZais = null;
			Wrkjg = null;

			Omoteura = null;
		}

		/// <summary>
		/// 完全コピーするコンストラクタ
		/// </summary>
		internal Nmgt(Nmgt src) {
			this.Tsn = src.Tsn;
			this.Comnt = src.Comnt;
			if (src.m_osnum == null) this.m_osnum = null;
			else {
				this.m_osnum = new int[2]; this.m_osnum[0] = src.m_osnum[0]; this.m_osnum[1] = src.m_osnum[1];
			}
			this.tolst = src.tolst;
			this.Wrkjg = src.Wrkjg;
			this.Sratt = src.Sratt;
			this.Fratt = src.Fratt;
			this.Dimn = src.Dimn;
			this.Wnum = src.Wnum;
			this.Trept = src.Trept;
			this.Omoteura = src.Omoteura;
			this.SetZais = src.SetZais;
		}


		/// <summary>
		/// １ショットの設定をクリア
		/// </summary>
		public void OtClear() {
			Tsn = null;
			Comnt = null;
			m_osnum = null;
		}

		/// <summary>
		/// 手順の１行の情報によるgmodの更新
		/// </summary>
		/// <param name="ddat">手順１行の文字列</param>
		public void Set_Tejun(string ddat) {
			string stmp;

			string[] ff = ddat.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			if (ff.Length == 1)
				stmp = "";
			else
				stmp = ff[1];

			switch (ddat[0]) {
			case 'M':
				Wrkjg = null;
				break;
			case 'T':
				if (ff.Length > 2) {
					Tsn = Convert.ToInt32(ff[2]);
				}
				else
					Tsn = 1;
				if (tolst != stmp)
					Trept = 0;
				else {
					if (this.Tsn == 1)
						Trept++;
				}
				tolst = stmp;
				break;
			case 'C':   // 手順書の分割位置
				Comnt = stmp;
				break;
			case 'D':
				Dimn = stmp[0];
				if (Dimn != '2' && Dimn != '3')
					throw new Exception("H (dimension sitei) error");
				break;
			case 'Z':
				SetZais = stmp;
				// 登録された材質かチェックする
				Material.ZgrpgetPC(stmp);
				break;
			case 'O':
				m_osnum = new int[2];
				m_osnum[0] = Convert.ToInt32(stmp) % 10000;
				m_osnum[1] = 1;
				if (ff.Length > 2) {
					if (Convert.ToInt32(ff[2]) > 0)
						m_osnum[1] = Convert.ToInt32(ff[2]);
				}
				break;
			case 'S':
				Sratt = Convert.ToDouble(stmp);
				break;
			case 'F':
				Fratt = Convert.ToDouble(stmp);
				break;
			case 'W':
				if (stmp.Length == 0)
					throw new Exception("ワークジグのファイル名が不正。");
				Wrkjg = stmp;
				break;
			case 'V':
				// 新規追加分
				Omoteura = Convert.ToInt16(stmp);
				if (Omoteura < 0 || Omoteura > 5)
					throw new Exception("加工方向V は、0=表面, 1=裏面, 2=正面, 3=背面, 4=右側面, 5=左側面 です。");
				break;
			default:
				CamUtil.LogOut.warn.AppendLine("手順の行 '" + ddat + "' は認識できません。");
				break;
			}
		}
		public bool Equals(Nmgt obj) {
			if (this.Tsn != obj.Tsn) return false;
			if (this.Comnt != obj.Comnt) return false;
			if (this.m_osnum == null && obj.m_osnum != null) return false;
			if (this.m_osnum != null) {
				if (obj.m_osnum == null) return false;
				if (this.m_osnum[0] != obj.m_osnum[0]) return false;
				if (this.m_osnum[1] != obj.m_osnum[1]) return false;
			}
			if (this.tolst != obj.tolst) return false;
			if (this.Wrkjg != obj.Wrkjg) return false;
			if (this.Sratt != obj.Sratt) return false;
			if (this.Fratt != obj.Fratt) return false;
			if (this.Dimn != obj.Dimn) return false;
			if (this.Wnum != obj.Wnum) return false;
			if (this.Trept != obj.Trept) return false;
			if (this.Omoteura != obj.Omoteura) return false;
			if (this.SetZais != obj.SetZais) return false;
			return true;
		}
	}
}
