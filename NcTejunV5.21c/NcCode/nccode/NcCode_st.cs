using System;
using System.Collections.Generic;
using System.Text;

namespace NcCode.nccode
{
	/// <summary>
	/// 手順で設定された移動ミラーを入力時に処理します。NCSPEEDで使用するために新たに作成されました。 in 2010/06/22[不変]
	/// </summary>
	public class Transp_Mirror
	{
		/// <summary>移動ベクトル</summary>
		public readonly CamUtil.Vector3 m_ido;
		/// <summary>ミラーベクトル</summary>
		public readonly CamUtil.NcZahyo mirr;
		//private string m_mir_x, m_mir_y;
		/// <summary>反転有無</summary>
		public readonly bool rev;

		/// <summary>唯一のコンストラクタ</summary>
		public Transp_Mirror(CamUtil.Vector3 ido, CamUtil.NcZahyo mirr, bool rev) {
			this.m_ido = ido;
			this.mirr = mirr;
			this.rev = rev;
		}
	}

	/// <summary>
	/// サブプログラムの呼出し深さ情報を保存します。
	/// </summary>
	public readonly struct Ncdep
	{
		// /////////////////////////////
		// 以下は演算子のオーバーロード
		// /////////////////////////////

		/// <summary>呼出し深さの==演算子</summary>
		public static bool operator ==(Ncdep c1, Ncdep c2) { return c1.Equals(c2); }
		/// <summary>呼出し深さの!=演算子</summary>
		public static bool operator !=(Ncdep c1, Ncdep c2) { return !c1.Equals(c2); }




		/// <summary>固定サイクルプログラムの深さ</summary>
		public readonly int subf;
		/// <summary>マクロプログラムの深さ</summary>
		public readonly int subm;
		/// <summary>一般サブプログラムの深さ</summary>
		public readonly int subs;
		/// <summary>読み込みした行での固定サイクルモード</summary>
		public readonly CamUtil.LCode.Gcode gg09;
		/// <summary>読み込みした行でのカスタムマクロモード</summary>
		public readonly CamUtil.LCode.Gcode gg12;

		/// <summary>サブの合計深さ（メインは０）</summary>
		public int Depth { get { return subf + subm + subs; } }

		/// <summary>コンストラクタ（初期化new）</summary>
		internal Ncdep(int dummy) {
			subf = subm = subs = 0;
			this.gg09 = new CamUtil.LCode.Gcode(CamUtil.LCode.Gcode.ggroup[9]);
			this.gg12 = new CamUtil.LCode.Gcode(CamUtil.LCode.Gcode.ggroup[12]);
		}
		/// <summary>コンストラクタ</summary>
		internal Ncdep(CamUtil.LCode.Gcode gg09, CamUtil.LCode.Gcode gg12) {
			subf = subm = subs = 0;
			this.gg09 = gg09;
			this.gg12 = gg12;
		}
		internal Ncdep(int subf, int subm, int subs, CamUtil.LCode.Gcode gg09, CamUtil.LCode.Gcode gg12) {
			this.subf = subf;
			this.subm = subm;
			this.subs = subs;
			this.gg09 = gg09;
			this.gg12 = gg12;
		}
		/// <summary>固定サイクルコールの更新</summary>
		public Ncdep UpDateFix(int subf, CamUtil.LCode.Gcode gg09) { return new Ncdep(subf, this.subm, this.subs, gg09, this.gg12); }
		/// <summary>マクロコールの更新</summary>
		public Ncdep UpDateMac(int subm, CamUtil.LCode.Gcode gg12) { return new Ncdep(this.subf, subm, this.subs, this.gg09, gg12); }
		/// <summary>サブプロコールの更新</summary>
		public Ncdep UpDateSub(int subs) { return new Ncdep(this.subf, this.subm, subs, this.gg09, this.gg12); }

		/// <summary>このインスタンスと指定した ncdep オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj が ncdep のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public override bool Equals(object obj) { return Equals((Ncdep)obj); }
		private bool Equals(Ncdep obj) {
			if (this.subf != obj.subf) return false;
			if (this.subm != obj.subm) return false;
			if (this.subs != obj.subs) return false;
			if (!this.gg09.Equals(obj.gg09)) return false;
			if (!this.gg12.Equals(obj.gg12)) return false;
			return true;
		}
		/// <summary>このインスタンスのハッシュ コードを返します。</summary>
		/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
		public override int GetHashCode() { return subf ^ subm ^ subs ^ gg09.GetHashCode() ^ gg12.GetHashCode(); }
	}
}
