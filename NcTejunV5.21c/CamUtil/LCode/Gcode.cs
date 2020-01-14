using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil.LCode
{
	/// <summary>
	/// Ｇコードを保存するストラクチャです。
	/// </summary>
	public readonly struct Gcode
	{
		/// <summary
		/// >sg[][,]に保存するＧコードの単位。
		/// これ以下の小数があってもsgと一致しないのでエラーとなる。このため、ＮＣからの変換もpostの単位ではなくこれを使用する 2019/03/27
		/// </summary>
		public const int unt0 = 10;
		/// <summary>Ｇコードグループの数</summary>
		public const int GGRPNUM = 26;

		/// <summary>Ｇグループの初期状態。</summary>
		public static readonly double[] ggroup = new double[GGRPNUM] {
		//  0  1  2     3  4     5  6     7     8  9
			0, 0, 17.0, 0, 22.0, 0, 21.0, 40.0, 0, 80.0,
		//  0     1     2     3     4     5     6     7     8     9
			98.0, 50.0, 67.0, 97.0, 54.0, 64.0, 69.0, 15.0, 50.1, 40.1,
		//  0  1  2  3  4     5
			0, 0, 0, 0, 25.0, 13.1};

		private static int SetItem(double value) {
			if (value * unt0 - Math.Floor(value * unt0) > 0.00001) {
				throw new Exception($"小数点以下{((int)Math.Log10(unt0) + 1).ToString()}桁以上に数値が設定されたＧコードは処理できません。");
			}
			return (int)Math.Round(value * unt0);
		}

		/// <summary>Ｇコード番号より、Ｇコードグループ番号と処理内容を出力する</summary>
		/// <param name="ignum">Ｇコードの値</param>
		/// <param name="group">Ｇコードグループ番号</param>
		/// <returns>Ｇコードの処理区分  0:正常に処理 1:Ｇコードを無視する 2:その行を無視する 3:処理不能エラー</returns>
		static private int Ncgset(double ignum, out byte group) {
			int? isw = null;
			group = 0;
			// モーダルＧコードの検索
			for (byte ii = 1; ii < sg.Length; ii++)
				for (int jj = 0; sg[ii][jj, 0] >= 0; jj++) {
					if (sg[ii][jj, 1] == (int)(ignum * unt0)) {
						isw = sg[ii][jj, 0];
						group = ii;
					}
				}
			// １ショットＧコードの検索
			if (isw == null) {
				for (int jj = 0; sg[0][jj, 0] >= 0; jj++) {
					if (sg[0][jj, 1] == (int)(ignum * unt0)) {
						isw = sg[0][jj, 0];
					}
				}
			}
			return isw ?? 1;
		}

		/// <summary>
		/// Ｇコードグループのメンバーとその処理を決定する
		/// Gコードのグループ番号、処理区分、とコード番号
		/// 処理区分＝0:正常に処理 1:Ｇコードを無視する 2:その行を無視する 3:処理不能エラー
		/// </summary>
		static private int[][,] sg = new int[GGRPNUM][,] {
			new int[,] {	/* GROUP 00 */
				{0,  40}, /* DWELL */
				{0,  50}, /* KOUSOKU MODE ON */
				{0,  90}, /* ECACT STOP */
				{0, 100}, /* DATA SETTEI */
				{0, 110}, /* DATA SETTEI CANCEL */
				{0, 280}, /* JIDO REFERENCE */
				{0, 290}, /* JIDO REFERENCE */
				{0, 310}, /* SKIP KONOU */
				{0, 390}, /* corner endko-hokan */
				{0, 650}, /* custum macro */
				{0, 920}, /* work zahyou henkou */
				{1, 270},
				{1, 300},
				{1, 301},
				{1, 311},
				{1, 312},
				{1, 313},
				{1, 314},
				{1, 370}, /* auto tool length hosei */
				{1, 380},
				{1, 450}, /* tool position offset */
				{1, 460}, /* tool position offset */
				{1, 470}, /* tool position offset */
				{1, 480}, /* tool position offset */
				{1, 600}, /* 1 houkou ichi gime */
				{2,  51},
				{2, 530}, /* machine coodinate or sakiyomi stop 2015/02/06 */
				{3,  70},
				{3,  71},
				{3, 101},
				{3, 103},
				{3, 106},
				{3, 113},
				{3, 520},
				{3, 653},
				{3, 721},
				{3, 722},
				{3, 811},
				{3, 921},
				{-1, -1}},
			new int[,] {	/* GROUP 01 */
				{0,   0},
				{0,  10},
				{0,  20},
				{0,  30},
				{3,  21},
				{3,  31},
				{3,  22},
				{3,  32},
				{3,  23},
				{3,  33},
				{3,  61},
				{3, 330},
				{-1, -1}},
			new int[,] {	/* GROUP 02 */
				{0, 170},
				{0, 180},
				{0, 190},
				{-1, -1}},
			new int[,] {	/* GROUP 03 */
				{0, 900},
				{0, 910},
				{-1, -1}},
			new int[,] {	/* GROUP 04 */
				{2, 220}, /* stored stroke check */
				{2, 230}, /* stored stroke check */
				{-1, -1}},
			new int[,] {	/* GROUP 05 */
				{0, 940}, /* okuri seigyo */
				{3, 930}, /* inverse time okuri */
				{3, 950}, /* mai kaiten okuri */
				{-1, -1}},
			new int[,] {	/* GROUP 06 */
				{0, 210}, /* METRIC */
				{3, 200},
				{-1, -1}},
			new int[,] {	/* GROUP 07 */
				{0, 400}, /* KOUGU KEI HOSEI OFF */
				{0, 410}, /* 2D offset only */
				{0, 420},
				{-1, -1}},
			new int[,] {	/* GROUP 08 */
				{0, 490}, /* KOUGU CHOU HOSEI CANCEL */
				{0, 430}, /* tool length hosei */
				{0, 440}, /* tool length hosei */
				{-1, -1}},
			new int[,] {	/* GROUP 09 */
				{0, 800}, /* kotei sycle */
				{0, 730}, /* kotei sycle */
				{0, 740}, /* kotei sycle */
				{0, 760}, /* kotei sycle */
				{0, 810}, /* kotei sycle */
				{0, 820}, /* kotei sycle */
				{0, 830}, /* kotei sycle */
				{0, 840}, /* kotei sycle */
				{0, 842}, /* kotei sycle */
				{0, 843}, /* kotei sycle */
				{0, 850}, /* kotei sycle */
				{0, 860}, /* kotei sycle */
				{0, 870}, /* kotei sycle */
				{0, 880}, /* kotei sycle */
				{0, 890}, /* kotei sycle */
				{-1, -1}},
			new int[,] {	/* GROUP 10 */
				{0, 980}, /* kotei cycle initial */
				{0, 990}, /* kotei cycle initial */
				{-1, -1}},
			new int[,] {	/* GROUP 11 */
				{0, 500}, /* SCALING CANCEL */
				{0, 510}, /* SCALING */
				{-1, -1}},
			new int[,] {	/* GROUP 12 */
				{0, 670}, /* custum macro */
				{0, 660}, /* custum macro */
				{0, 661}, /* custum macro */
				{-1, -1}},
			new int[,] {	/* GROUP 13 */
				{0, 970}, /* syuu sokudo ittei seigyo CANCEL */
				{3, 960},
				{-1, -1}},
			new int[,] {	/* GROUP 14 */
				{0, 540}, /* work zahyou kei */
				{0, 550}, /* work zahyou kei */
				{0, 560}, /* work zahyou kei */
				{0, 570}, /* work zahyou kei */
				{0, 580}, /* work zahyou kei */
				{0, 590}, /* work zahyou kei */
				{2, 541}, /* work zahyou kei */
				{-1, -1}},
			new int[,] {	/* GROUP 15 */
				{0, 610}, /* exact stop mode */
				{0, 640}, /* SESSAKU MODE */
				{1, 620}, /* sessaku mode */
				{1, 630}, /* sessaku mode */
				{-1, -1}},
			new int[,] {	/* GROUP 16 */
				{0, 680}, /* ZAHYOU KAITEN */
				{0, 690}, /* ZAHYOU KAITEN CANCEL */
				{-1, -1}},
			new int[,] {	/* GROUP 17 */
				{0, 150}, /* KYOKU ZAHYOU CANCEL */
				{3, 160},
				{-1, -1}},
			new int[,] {	/* GROUP 18 */
				{0, 501}, /* MIRROR IMAGE CANCEL */
				{0, 511}, /* MIRROR IMAGE SET */
				{-1, -1}},
			new int[,] {	/* GROUP 19 */
				{0, 401}, /* HOUSEN SEIGYO CANCEL */
				{1, 411}, /* hou-sen vector seigyo */
				{1, 421}, /* hou-sen vector seigyo */
				{-1, -1}},
			new int[,] {	/* GROUP 20 */
				{-1, -1}},
			new int[,] {	/* GROUP 21 */
				{-1, -1}},
			new int[,] {	/* GROUP 22 */
				{-1, -1}},
			new int[,] {	/* GROUP 23 */
				{-1, -1}},
			new int[,] {	/* GROUP 24 */
				{2, 250}, /* shu-jiku sokudo hendou kenshutsu */
				{2, 260}, /* shu-jiku sokudo hendou kenshutsu */
				{-1, -1}},
			new int[,] {	/* GROUP 25 */
				{0, 131}, /* KYOKU ZAHYOU CANCEL */
				{3, 121},
				{-1, -1}}
		};






		/// <summary>Ｇコード設定の有無</summary>
		public bool Gst { get { return item.HasValue; } }
		/// <summary>Ｇコード番号。Post.sdgt[7]倍となる</summary>
		private readonly int? item;
		/// <summary>Ｇコードマクロ呼出しの場合はtrue</summary>
		public bool MacroCall { get { return macName > 0; } }


		/// <summary>Ｇコードの処理区分　0:正常に処理 1:Ｇコードを無視する 2:その行を無視する 3:処理不能エラー</summary>
		public readonly int gsw;
		/// <summary>Ｇコードのグループ番号</summary>
		public readonly byte group;
		/// <summary>Ｇコードマクロのプログラム番号（G100の場合は9010）</summary>
		public readonly int macName;
		/// <summary>ＧコードマクロのＧコード番号（G100の場合は100）未使用</summary>
		public readonly int? macCode;

		/// <summary>Ｇコードの読み替え、処理区分とグループに対応するコンストラクタ</summary>
		/// <param name="value">Ｇコード設定値</param>
		/// <param name="yomikae">Ｇコード読替えリスト</param>
		/// <param name="gCodeMacro">
		/// Ｇコード呼び出しマクロのリスト。Ｇコード、マクロ番号と呼出し方法のリストを与える
		/// [0]:Ｇコード番号(etc.100)、[1]:マクロ番号(etc.9010)、[2]:0=単純呼出し(G65) 1=モーダル呼出し移動指令(G66) 2:モーダル呼出し毎ブロック(G66.1)
		/// </param>
		public Gcode(double value, List<double[]> yomikae, List<int[]> gCodeMacro) {
			// Ｇコードの読み替え
			for (int ii = 0; ii < yomikae.Count; ii++)
				if (Math.Abs(value - yomikae[ii][0]) * unt0 < 0.1) {
					value = yomikae[ii][1];
					break;
				}

			// Ｇコードマクロの検索（整数の場合）
			if ((int)Math.Floor(value) * unt0 == (int)Math.Floor(value * unt0)) {
				int ival = (int)Math.Floor(value);

				foreach (int[] ii in gCodeMacro) {
					if (ival == ii[0]) {
						gsw = 0;
						macCode = ii[0];
						macName = ii[1];
						switch (ii[2]) {
						case 0: group = 0; item = SetItem(65.0); break;
						case 1: group = 12; item = SetItem(66.0); break;
						case 2: group = 12; item = SetItem(66.1); break;
						default: throw new Exception("qwefbqwehfbqh");
						}
						return;
					}
				}

				if (value == 100) throw new Exception("G100が一般のＧコードとして処理されます。");
				if (value == 105) throw new Exception("G105が一般のＧコードとして処理されます。");
			}

			macName = 0;
			macCode = null;
			item = SetItem(value);

			// Ｇコードグループ、Ｇコード処理区分の取得
			gsw = Gcode.Ncgset(value, out group);

			// エラーの処理
			switch (gsw) {
			case 0:break;   // 正常
			case 1: LogOut.CheckCount("Gcode 312", true, "Gcode G" + value.ToString("00.0") + " のＧコードは処理されません"); break;	// Ｇコードを無視
			case 2: LogOut.CheckCount("Gcode 313", true, "Gcode G" + value.ToString("00.0") + " の１行は処理されません"); break;	// １行を無視
			case 3: throw new Exception("Gcode G" + value.ToString("00.0") + " エラー。");	// エラー
			}
		}



		/// <summary>Ｇコード番号でＧコードを作成します</summary>
		/// <param name="value">Ｇコード設定値</param>
		public Gcode(double value)
		{
			item = SetItem(value);
			// Ｇコードグループ、Ｇコード処理区分の取得
			gsw = Gcode.Ncgset(value, out group);
			macName = 0;
			macCode = null;
		}

		/// <summary>Ｇコードを実数化して出力します</summary>
		public double ToDouble() {
			if (item.HasValue == false) throw new Exception("Ｇコードが設定されていない");
			return (double)item.Value / unt0;
		}
		/// <summary>整数化の可否を判断します</summary>
		public bool ToInt_OK() { return item.Value == (int)(Math.Round(ToDouble()) * unt0); }
		/// <summary>整数値のＧコードを出力します。小数点付になる場合はエラーです</summary>
		public int ToInt() {
			double tmp = Math.Round(ToDouble());
			if ((int)Math.Round(tmp * unt0) != item.Value) throw new Exception("整数値のＧコード以外が呼び出された");
			return (int)tmp;
		}

		/// <summary>実数Ｇコードと比較します</summary>
		public bool Equals(double value)
		{
			if (item.HasValue == false) return false;
			return item.Value == (int)Math.Round(value * unt0);
		}
		/// <summary>このインスタンスと指定した Gcode オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public bool Equals(Gcode obj) {
			if (this.item != obj.item) return false;
			if (this.gsw != obj.gsw) return false;
			if (this.group != obj.group) return false;
			if (this.macName != obj.macName) return false;
			if (this.macCode != obj.macCode) return false;
			return true;
		}
		/// <summary></summary>
		public override string ToString() { return item.Value.ToString(); }
		/// <summary></summary>
		public string ToString(string format) { return item.Value.ToString(format); }
		/// <summary>
		/// Ｇコードを元のＮＣデータに復元して出力する
		/// </summary>
		/// <returns></returns>
		public string ToStringAuto() {
			string form = this.ToInt_OK() ? "00" : "00." + new string('0', (int)Math.Round(Math.Log10(unt0)));
			if (this.ToInt_OK()) { if (form != "00") throw new Exception("新しいformのチェックでエラー"); }
			else { if (form != "00.0") throw new Exception("新しいformのチェックでエラー"); }
			return "G" + this.ToDouble().ToString(form);
		}
	}
}
