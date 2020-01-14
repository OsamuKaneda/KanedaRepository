using System;
using System.Collections.Generic;
using System.Text;

namespace NcCode.nccode
{
	/// <summary>
	/// 加工する設備のボタンのオン/オフなどの情報を管理します。
	/// </summary>
	/// <remarks>
	/// １．入力するＮＣデータの仕様はPostとし明確に区分したい。2008/9/25
	/// ２．シミュレーションの場合は加工機の情報とＮＣデータ仕様は一致する2010/04/08
	/// </remarks>
	internal static class NcMachine
	{
		/// <summary>パラメータ2401にセット可能な場合はtrue</summary>
		private static bool set2401;

		// ////////////////////
		// 加工機のスイッチ関連
		// ////////////////////
		/// <summary>オプショナルスキップ（OFF:false ON:true）</summary>
		static public bool oskip = true;
		/// <summary>オプショナルストップ（OFF:false ON:true）</summary>
		static public bool ostop = true;
		/// <summary>ミラースイッチＸＹＺ（OFF:false ON:true）</summary>
		static public bool[] mmirs ={ false, false, false };

		// ////////////////
		// 工具マガジン関連
		// ////////////////
		//public const int TOOLMAX = 300;
		/// <summary>工具交換するＭコード番号</summary>
		static public int tchg;

		// ///////////////
		// 時定数関連
		// ///////////////
		/// <summary>時定数1620</summary>
		static public double t0;
		/// <summary>時定数1622</summary>
		static public double t1;
		/// <summary>時定数1825</summary>
		static public double t2;
		/// <summary>時定数1827</summary>
		static public double th;
		/// <summary>時定数による停止誤差</summary>
		static public double[] gosa = new double[20];

		/// <summary>Ｇコードの読み替えリスト</summary>
		public static List<double[]> sgc = new List<double[]>();

		// /////////////////////
		// マシンパラメータ関連
		// /////////////////////
		/// <summary>パラメータのデータ番号のリスト。同じ位置のparaaに情報が保存される</summary>
		static private int[] parad ={   0,
		  12,  12,  12,1004,1220,1220,1220,1221,1221,1221,
		1222,1222,1222,1223,1223,1223,1224,1224,1224,1225,
		1225,1225,1226,1226,1226,1240,1240,1240,1241,1241,
		1241,1242,1242,1242,1243,1243,1243,1400,1420,1420,
		1420,1420,1420,1420,1422,1422,1422,1422,1422,1422,
		1451,1452,1453,1454,1455,1456,1457,1458,1459,1493,
		1620,1622,1825,1827,2400,2401,6200,6210,6211,6400,
		6410,6411,6421,6421,6421,7000,7050,7051,7052,7053,
		7054,7055,7056,7057,7058,7059,7611};
		/// <summary>パラメータのデータ</summary>
		static private int?[] paraa = new int?[parad.Length];

		/// <summary>パラメータミラー情報（null:OFF, 0:ON）</summary>
		static public CamUtil.NcZahyo MIRROR {
			get {
				return new CamUtil.NcZahyo(
					(paraa[mirIndex + 0].Value & 0x01) == 0 ? (double?)null : 0,
					(paraa[mirIndex + 1].Value & 0x01) == 0 ? (double?)null : 0,
					(paraa[mirIndex + 2].Value & 0x01) == 0 ? (double?)null : 0);
			}
		}
		/// <summary>座標系設定</summary>
		static public CamUtil.Ichi Offset(int g54) {
			return new CamUtil.Ichi(
				new CamUtil.Vector3(
					NcMachine.ParaData(1220, 0) + NcMachine.ParaData(1221 + g54, 0),
					NcMachine.ParaData(1220, 1) + NcMachine.ParaData(1221 + g54, 1),
					NcMachine.ParaData(1220, 2) + NcMachine.ParaData(1221 + g54, 2)
				),
				new CamUtil.Vector3(
					NcMachine.ParaData(1220, 0) + NcMachine.ParaData(1221 + g54, 3),
					NcMachine.ParaData(1220, 1) + NcMachine.ParaData(1221 + g54, 4),
					NcMachine.ParaData(1220, 2) + NcMachine.ParaData(1221 + g54, 5)
				),
				Post.PostData['X'].sdgt);
		}
		/// <summary>ミラーのパラメータ１２の保存位置</summary>
		private static readonly int mirIndex;


		// ///////////////
		// その他
		// ///////////////
		/// <summary>
		/// 工具交換時間
		/// </summary>
		static public double ttim;

		/// <summary>
		/// Ｇコードマクロのリスト
		/// </summary>
		public static List<int[]> GCodeMacro = new List<int[]>();
		private static void SetGCodeMacro() {
			GCodeMacro.Clear();
			int[] item;
			for (int jj = 7050; jj <= 7059; jj++)
				if (ParaValue(jj, 0)) {
					item = new int[3];
					item[0] = Math.Abs(ParaData(jj, 0));
					item[1] = 9010 + jj - 7050;
					if (ParaData(jj, 0) < 0) {
						if ((NcMachine.ParaData(7000, 0) & 8) == 0)
							item[2] = 1;
						else
							item[2] = 2;
					}
					else {
						item[2] = 0;
					}
					GCodeMacro.Add(item);
				}
		}

		static NcMachine() {
			for (int ii = 0; ii < paraa.Length; ii++)
				paraa[ii] = null;
			paraa[Parag(1004, 0)] = 0;
			mirIndex = Array.IndexOf<int>(parad, 12);
		}

		/// <summary>
		/// 加工機パラメータの設定
		/// </summary>
		/// <param name="mID"></param>
		static public void ParaInit(CamUtil.Machine.MachID mID)
		{
			// /////////////////////////
			// MCNPARAM のデータをセット
			// /////////////////////////
			tchg = 6;
			ttim = 0.0;
			sgc.Clear();
			// /////////////////////////
			// NCOPARAM のデータをセット
			// /////////////////////////
			// 以下のパラメータは FANUC 11M がベース
			ParaSet(1004, 0, 0);        // #0==0,#1==0:最小設定単位0.001(1,0:最小設定単位0.01), #2==1:入力単位を10倍の値にする
			ParaSet(2400, 0, 0);        // #1==0:小数点なしＦコード単位1mm/min(1:0.1mm/min)
			set2401 = true;	// para2401 の変更を許可する
			ParaSet(2401, 0, 2);
			set2401 = false;// para2401 の変更を禁止する
			// #0==0:電源投入時G00モード(1:G01), #1==0:電源投入時G91モード(1:G90), #2==0,#3==0:電源投入時G49モード(0,1:G43, 1,0:G44)
			// #4==0:電源投入時G94モード(1:G95), #5==0:電源投入時G17モード(1:G18)
			for (int ii = 0; ii < 3; ii++) ParaSet(1220, ii, 0);    // 各軸の外部ワーク原点オフセット量
			for (int ii = 0; ii < 3; ii++) ParaSet(1221, ii, 0);    // ワーク座標系１のワーク原点オフセット量
			for (int ii = 0; ii < 3; ii++) ParaSet(1222, ii, 0);    // ワーク座標系２のワーク原点オフセット量
			for (int ii = 0; ii < 3; ii++) ParaSet(1223, ii, 0);    // ワーク座標系３のワーク原点オフセット量
			for (int ii = 0; ii < 3; ii++) ParaSet(1224, ii, 0);    // ワーク座標系４のワーク原点オフセット量
			for (int ii = 0; ii < 3; ii++) ParaSet(1225, ii, 0);    // ワーク座標系５のワーク原点オフセット量
			for (int ii = 0; ii < 3; ii++) ParaSet(1226, ii, 0);    // ワーク座標系６のワーク原点オフセット量
			for (int ii = 0; ii < 3; ii++) ParaSet(1240, ii, 0);    // 第１リファレンス点の機械座標系での座標値
			for (int ii = 0; ii < 3; ii++) ParaSet(1241, ii, 0);    // 第２リファレンス点の機械座標系での座標値
			for (int ii = 0; ii < 3; ii++) ParaSet(1242, ii, 0);    // 第３リファレンス点の機械座標系での座標値
			for (int ii = 0; ii < 3; ii++) ParaSet(1243, ii, 0);    // 第４リファレンス点の機械座標系での座標値
			ParaSet(1400, 0, 16);       // #4==0:非直線補間型位置決め(1:直線補間型位置決め)
										//NcMachine.paraSet(1400, 0, 0, null);		// #4==0:非直線補間型位置決め(1:直線補間型位置決め)
			ParaSet(1420, 0, 15001);    // 各軸の早送り速度
			ParaSet(1420, 1, 15001);
			ParaSet(1420, 2, 15002);
			for (int ii = 3; ii < 6; ii++) ParaSet(1420, ii, 15000);
			for (int ii = 0; ii < 6; ii++) ParaSet(1422, ii, 15000);    // 各軸の最大切削送り速度
			for (int jj = 0; jj < 9; jj++) ParaSet(1451 + jj, 0, 0);    // Ｆ１桁送り速度
			ParaSet(1620, 0, 160);  // 軸毎の直線形早送り加減速の時定数
			ParaSet(1622, 0, 40);   // 軸毎の切削送り加減速の時定数
			ParaSet(1825, 0, 3000); // 軸毎のサーボループゲイン
			ParaSet(1827, 0, 10);   // 軸毎のインポジションの幅
			ParaSet(6200, 0, 0);    // #0==0:固定サイクルの穴あけ軸は常にＺ軸(1:プログラムで選択された軸)
			ParaSet(6210, 0, 1000); // 固定サイクルG73のもどり量
			ParaSet(6211, 0, 1500); // 固定サイクルG83のクリアランス量
			for (int ii = 0; ii < 3; ii++) ParaSet(12, ii, 2);  // #0==0:ミラーイメージオフ(1:オン), #1==0:スケーリング無効(1:有効)
			ParaSet(6400, 0, 6);    // #0==0:座標回転G68の回転角度の指令Rは常に絶対値(1:モードによる), #1==0:スケーリングG51の倍率単位は0.00001(1:0.001), #2==0:座標回転G68の回転角度の単位は0.00001deg(1:0.001deg)
			ParaSet(7611, 0, 0);    // #4==0:スケーリングG51は等方P6410(1:異方P6421)
			ParaSet(6410, 0, 5000); // スケーリングG51の倍率
			for (int ii = 0; ii < 3; ii++) ParaSet(6421, ii, 2000); // スケーリングG51の異方倍率
			ParaSet(6411, 0, 180000);   // 座標回転G68の回転角度
			ParaSet(7000, 0, 0);        // #3==0:ユーザＧコードモーダル呼出しは移動後呼出し(1:毎ブロック呼出し)
			ParaSet(7050, 0, 100);      // プログラム番号9010のカスタムマクロを呼出すＧコード
										//for (int jj = 0; jj < 9; jj++) NcMachine.paraSet(7051 + jj, 0, null, null);
			for (int ii = 0; ii < 3; ii++) {
				if (NcMachine.mmirs[ii] == false)
					ParaSet(12, ii, NcMachine.ParaData(12, ii) & ~0x01);
				else
					ParaSet(12, ii, NcMachine.ParaData(12, ii) | 0x01);
			}

			// "G105の設定"追加 2017/10/26
			if (mID == CamUtil.Machine.MachID.MCC3016VG)
				ParaSet(7055, 0, 105);      // プログラム番号9015のカスタムマクロを呼出すＧコード
			// Ｇコードマクロ情報の設定
			SetGCodeMacro();
		}
		/// <summary>
		/// G10 によるパラメータの設定
		/// </summary>
		/// <param name="tcode"></param>
		static public void G10ParaSet(OCode tcode) {
			if (tcode.codeData.CodeCount('P') == 0)
				ParaSet(tcode.codeData.CodeData('N').ToInt, 0, tcode.codeData.CodeData('R').ToInt);
			else
				ParaSet(tcode.codeData.CodeData('N').ToInt, tcode.codeData.CodeData('P').ToInt, tcode.codeData.CodeData('R').ToInt);
			CamUtil.LogOut.CheckCount("NcMachine 232", false, "G10 によるパラメータの設定" + tcode.codeData.NcText);
			// Ｇコードマクロ情報の設定
			SetGCodeMacro();
		}

		/// <summary>
		/// ＮＣのパラメータとsdgt（小数点なしの場合の単位）の初期設定
		/// </summary>
		/// <param name="codn">パラメータＮｏ</param>
		/// <param name="codp">サブパラメータＮｏ</param>
		/// <param name="codd">入力データ</param>
		private static void ParaSet(int codn, int codp, int codd)
		{
			int ii, jj;
			double rtmp;

			switch (codn) {
			case 1004:
				paraa[Parag(codn, 0)] = codd;
				if ((ParaData(1004, 0) & 0x03) == 0)
					jj = 1000;
				else if ((ParaData(1004, 0) & 0x03) == 1)
					jj = 100;
				else if ((ParaData(1004, 0) & 0x03) == 2)
					jj = 10000;
				else {
					_main.Error.Ncerr(2, "PARAM 1004 0,1bit error");
					return;
				}
				Post.ZahyoSet(jj);
				break;
			case 2400:
				paraa[Parag(codn, 0)] = codd;
				if ((ParaData(2400, 0) & 0x01) != 0) {
					_main.Error.Ncerr(2, "PARAM 2400  0bit error");
					return;
				}
				if ((ParaData(2400, 0) & 0x02) == 0) {
					Post.FcodeSet(1);		/* F */
				}
				else {
					Post.FcodeSet(10);		/* F */
					_main.Error.Ncerr(2, "PARAM 2400  1bit error");
					return;
				}
				if ((ParaData(2400, 0) & 0x04) != 0) {
					_main.Error.Ncerr(2, "PARAM 2400  2bit error");
					return;
				}
				if ((ParaData(2400, 0) & 0x20) != 0) {
					_main.Error.Ncerr(2, "PARAM 2400  5bit error");
					return;
				}
				break;
			case 2401:
				if (set2401 == false) throw new Exception("加工中はパラメータ2401を変更できません。");
				paraa[Parag(codn, 0)] = codd;
				if ((ParaData(2401, 0) & 0x04) != 0 || (ParaData(2401, 0) & 0x08) != 0) {
					_main.Error.Ncerr(2, "PARAM 2401  2,3bit error");
					return;
				}
				if ((ParaData(2401, 0) & 0x10) != 0) /* Fcode mode       */ {
					Post.FcodeSet(100);		/* F */
					_main.Error.Ncerr(2, "PARAM 2401  4bit error");
					return;
				}
				break;
			case 1451:
			case 1452:
			case 1453:
			case 1454:
			case 1455:
			case 1456:
			case 1457:
			case 1458:
			case 1459:
				if (Post.PostData['R'].axis) {
					paraa[Parag(codn, 0)] = Post.PostData['F'].sdgt * codd;
				}
				else {
					paraa[Parag(codn, 0)] = (Post.PostData['F'].sdgt * codd) / Post.PostData['R'].sdgt;
				}
				if (ParaData(codn, 0) < 0) {
					_main.Error.Ncerr(2, "PARAM " + codn.ToString() + " error");
					return;
				}
				break;
			case 1493:
				paraa[Parag(codn, 0)] = codd;
				break;
			case 6200:
				paraa[Parag(codn, 0)] = codd;
				if ((ParaData(6200, 0) & 0x01) != 0) {
					_main.Error.Ncerr(2, "PARAM 6200  0bit error");
					return;
				}
				break;
			case 12:
				paraa[Parag(codn, codp)] = codd;
				break;
			default:
				paraa[Parag(codn, codp)] = codd;
				if (codn == 6400)
					if ((ParaData(6400, 0) & 0x04) == 0)
						throw new Exception("バグである可能性大");
				break;
			}
			if (ParaValue(1622, 0) && ParaValue(1825, 0) && (codn == 1622 || codn == 1825)) {
				t1 = ParaData(1622, 0) / 1000.0;
				t2 = 100.0 / ParaData(1825, 0);
				if (t1 > Post.minim)
					for (ii = 0; ii < 20; ii++) {
						rtmp = (t1 + t2) * ii / 2.0;
						gosa[ii] = (t1 * t1 * Math.Exp(-rtmp / t1)
							 - t2 * t2 * Math.Exp(-rtmp / t2)) / (t1 - t2);
					}
			}
			else if (codn == 1620)
				t0 = ParaData(1620, 0) / 1000.0;
			else if (codn == 1827)
				th = ParaData(1827, 0) / 1000.0;

			return;
		}

		/// <summary>
		/// パラメータ値の定義の有無をチェック
		/// </summary>
		/// <param name="itmp">パラメータのデータ番号</param>
		/// <param name="jtmp">パラメータのサブ番号(0, 1, 2)</param>
		/// <returns></returns>
		static public bool ParaValue(int itmp, int jtmp)
		{
			return paraa[Parag(itmp, jtmp)].HasValue;
		}

		/// <summary>各軸の早送り速度(mm/min)</summary>
		/// <returns></returns>
		static public int[] ParaData1420() {
			return new int[] { ParaData(1420, 0), ParaData(1420, 1), ParaData(1420, 2), ParaData(1420, 3), ParaData(1420, 4), ParaData(1420, 5) };
		}
		/// <summary>各軸の最大切削送り速度(mm/min)</summary>
		static public int[] ParaData1422() {
			return new int[] { ParaData(1422, 0), ParaData(1422, 1), ParaData(1422, 2), ParaData(1422, 3), ParaData(1422, 4), ParaData(1422, 5) };
		}

		/// <summary>
		/// パラメータの値を返す
		/// </summary>
		/// <param name="itmp">パラメータのデータ番号</param>
		/// <param name="jtmp">パラメータのサブ番号(0, 1, 2)</param>
		/// <returns>パラメータ値</returns>
		static public int ParaData(int itmp, int jtmp)
		{
			int ii = Parag(itmp, jtmp);
			if (paraa[ii].HasValue)
				return (int)paraa[ii];
			else
				_main.Error.Ncerr(3, "PARAMETER SET ERROR #" + itmp.ToString("0000"));
			return -1;
		}

		/// <summary>
		/// パラメータparaaの値の保管位置を示す
		/// </summary>
		/// <param name="itmp">パラメータのデータ番号</param>
		/// <param name="jtmp">パラメータのサブ番号(0, 1, 2)</param>
		/// <returns>パラメータの値の保管場所</returns>
		static private int Parag(int itmp, int jtmp)
		{
			try { return Array.IndexOf<int>(parad, itmp) + jtmp; }
			catch {
				_main.Error.Ncerr(3, "PARAMETER NUMBER ERROR #" + itmp.ToString("0000"));
				throw new Exception();
			}
		}

		/// <summary>
		/// マクロ変数値（local : 0-33、global : 100-6000）
		/// </summary>
		public class Variable
		{
			/// <summary>コモン変数の最大値</summary>
			const int mVALMAX = 6000;
			/// <summary>ローカル変数の最大値</summary>
			public const int MMAX = 33;		/* macro local value */

			/// <summary>ローカル変数を保存する場所</summary>
			private readonly double?[] ltemp;
			/// <summary>コモン変数を保存する場所</summary>
			private double?[] stemp;

			/// <summary>
			/// コモン変数の初期化
			/// </summary>
			private void Init() {
				for (int ii = 0; ii <= 120; ii += 20)
					for (int jj = 5201 + ii; jj <= 5206 + ii; jj++)
						stemp[jj - 400] = 0.0;
				for (int jj = 2000; jj <= 2400; jj++)
					stemp[jj - 400] = 0.0;

				stemp[3007 - 400] = 0.0;

				for (int ii = 4000; ii <= 4200; ii += 200) {
					// Ｇコードの初期値を入力
					for (int jj = 0; jj < 21; jj++)
						stemp[ii + jj - 400] = CamUtil.LCode.Gcode.ggroup[jj];
					// ＮＣコードの初期値を入力
					stemp[ii + 102 - 400] = 0.0; /* B */
					stemp[ii + 107 - 400] = 0.0; /* D */
					stemp[ii + 108 - 400] = 0.0; /* E */
					stemp[ii + 109 - 400] = 0.0; /* F */
					stemp[ii + 111 - 400] = 0.0; /* H */
					stemp[ii + 113 - 400] = 0.0; /* M */
					stemp[ii + 114 - 400] = 0.0; /* N */
					stemp[ii + 115 - 400] = 0.0; /* O */
					stemp[ii + 119 - 400] = 0.0; /* S */
					stemp[ii + 120 - 400] = 0.0; /* T */
				}
				for (int ii = 0; ii <= 40; ii += 40) {
					stemp[ii + 5001 - 400] = 0.0; /* X */
					stemp[ii + 5002 - 400] = 0.0; /* Y */
					stemp[ii + 5003 - 400] = 0.0; /* Z */
					stemp[ii + 5004 - 400] = 0.0; /* A */
					stemp[ii + 5005 - 400] = 0.0; /* B */
					stemp[ii + 5006 - 400] = 0.0; /* C */
				}
				stemp[3000 - 400] = null;
				stemp[3011 - 400] = 0.0;
				stemp[3012 - 400] = 0.0;

				// 初期位置はNcOutsによって初期化される
				stemp[5001 - 400] = 999999.0;
				stemp[5002 - 400] = 999999.0;
				stemp[5003 - 400] = 999999.0;
				stemp[5004 - 400] = 999999.0;
				stemp[5005 - 400] = 999999.0;
				stemp[5006 - 400] = 999999.0;

				// 加工機内の設定値
				VariSet(500, 1.0);
				VariSet(501, 0.0);
				VariSet(502, 0.0);
				VariSet(503, 54.0);
				VariSet(504, 0.0);
				VariSet(505, 100.0);
				VariSet(506, 0.0);
				VariSet(507, 1.0);
				VariSet(508, 6.0);
				VariSet(509, 150.0);
				VariSet(510, 1.0);
				VariSet(511, 4.0);
				VariSet(512, 0.0);
				VariSet(513, 0.0);
				VariSet(514, 0.0);
				VariSet(515, -516.55);
				VariSet(829, 0.5);
			}

			/// <summary>コモン変数のセット</summary>
			/// <param name="num">コモン変数番号</param>
			/// <param name="value">データ</param>
			public void VariSet(int num, double? value) {
				if (100 <= num && num <= 199) { stemp[num - 100] = value; return; }
				if (500 <= num && num <= 999) { stemp[num - 400] = value; return; }
				if (num == 3000) { stemp[num - 400] = value; return; }
				if (5201 <= num && num <= 5340) { stemp[num - 400] = value; return; }
				throw new Exception("書込みできないマクロ変数(#" + num + ")への代入が実行された");
			}

			/// <summary>コモン変数の取出し</summary>
			/// <param name="num">コモン変数番号</param>
			/// <returns></returns>
			public double? VariData(int num) {
				if (100 <= num && num <= 199) return stemp[num - 100];
				if (500 <= num && num <= 999) return stemp[num - 400];
				if (num == 3000) return stemp[num - 400];
				if (1000 <= num && num <= mVALMAX) if (stemp[num - 400].HasValue) return stemp[num - 400];
				throw new Exception("未定義のマクロ変数(#" + num + ")への参照がが実行された");
			}



			/// <summary>コンストラクタ（ローカル変数コモン変数を初期化）</summary>
			public Variable() {
				ltemp = new double?[MMAX + 1];
				stemp = new double?[mVALMAX - 400];
				Init();	// コモン変数の初期化
			}
			/// <summary>コンストラクタ（ローカル変数をすべてnullとして初期化。コモン変数は共有）</summary>
			public Variable(Variable src) {
				ltemp = new double?[MMAX + 1];
				stemp = src.stemp;
			}

			/// <summary>
			/// 直前のブロックの終点位置ABC（ワーク座標系）
			/// </summary>
			public CamUtil.Ichi WorkIchi {
				get {
					return new CamUtil.Ichi(
						new CamUtil.Vector3(this.VariData(5001).Value, this.VariData(5002).Value, this.VariData(5003).Value),
						new CamUtil.Vector3(this.VariData(5004).Value, this.VariData(5005).Value, this.VariData(5006).Value), Post.PostData['X'].sdgt);
				}
			}

			/// <summary>
			/// マクロ変数値のget/set
			/// </summary>
			/// <param name="num"></param>
			/// <returns></returns>
			public double? this[int num] {
				set {
					if (1 <= num && num <= MMAX) { ltemp[num] = value; return; }
					this.VariSet(num, value);
				}
				get {
					if (num == 0) return ltemp[0];
					if (1 <= num && num <= MMAX) return ltemp[num];
					return this.VariData(num);
				}
			}

			/// <summary>
			/// 現在の機械座標系の位置を設定する
			/// </summary>
			/// <param name="out0"></param>
			internal void SystemSet_IDO(NcOuts out0) {
				stemp[5021 - 400] = out0.Cloc.X;
				stemp[5022 - 400] = out0.Cloc.Y;
				stemp[5023 - 400] = out0.Cloc.Z;
				stemp[5041 - 400] = out0.Ichi.X;
				stemp[5042 - 400] = out0.Ichi.Y;
				stemp[5043 - 400] = out0.Ichi.Z;
			}
			/// <summary>
			/// 直前のブロックのモーダル情報を設定する
			/// </summary>
			/// <param name="out0"></param>
			internal void SystemSet_COPY(NcOuts out0) {
				for (int ii = 1; ii <= 21; ii++)
					stemp[4000 + ii - 400] = stemp[4200 + ii - 400];	/* G */
				stemp[4102 - 400] = stemp[4302 - 400];	/* B */
				stemp[4107 - 400] = stemp[4307 - 400];	/* D */
				stemp[4108 - 400] = stemp[4308 - 400];	/* E */
				stemp[4109 - 400] = stemp[4309 - 400];	/* F */
				stemp[4111 - 400] = stemp[4311 - 400];	/* H */
				stemp[4113 - 400] = stemp[4313 - 400];	/* M */
				stemp[4114 - 400] = stemp[4314 - 400];	/* N */
				stemp[4115 - 400] = stemp[4315 - 400];	/* O */
				stemp[4119 - 400] = stemp[4319 - 400];	/* S */
				stemp[4120 - 400] = stemp[4320 - 400];	/* T */
				stemp[5001 - 400] = out0.Ichi.X;	/* X */
				stemp[5002 - 400] = out0.Ichi.Y;	/* Y */
				stemp[5003 - 400] = out0.Ichi.Z;	/* Z */
				stemp[5004 - 400] = out0.Ichi.A;	/* A */
				stemp[5005 - 400] = out0.Ichi.B;	/* B */
				stemp[5006 - 400] = out0.Ichi.C;	/* C */
				// 計測マクロのために、ダミーで追加 2010/09/09
				stemp[5061 - 400] = out0.Ichi.X;	/* X */
				stemp[5062 - 400] = out0.Ichi.Y;	/* Y */
				stemp[5063 - 400] = out0.Ichi.Z;	/* Z */
			}

			/// <summary>
			/// システム変数へのセット（Ｇコードグループ）
			/// </summary>
			/// <param name="ichar"></param>
			/// <param name="gcode"></param>
			internal void SystemSet(int ichar, CamUtil.LCode.Gcode gcode) {
				stemp[ichar + 4200 - 400] = gcode.ToDouble();
			}
			/// <summary>
			/// プローブの位置情報の設定
			/// </summary>
			/// <param name="data"></param>
			internal void SystemSet_PROBE(CamUtil.Ichi data) {
				stemp[0 + 5060 - 400] = data.X;
				stemp[1 + 5060 - 400] = data.Y;
				stemp[2 + 5060 - 400] = data.Z;
				stemp[3 + 5060 - 400] = data.A;
				stemp[4 + 5060 - 400] = data.B;
				stemp[5 + 5060 - 400] = data.C;
			}
			/// <summary>
			/// 
			/// </summary>
			/// <param name="data"></param>
			internal void SystemSet_PROBE(double data) {
				stemp[3007 - 400] = data;
			}

			/// <summary>ＮＣデータ１行の情報（OCode）による変数の設定</summary>
			/// <param name="ocode"></param>
			public void SystemSet_NCCODE(OCode ocode) {
				CamUtil.LCode.Gcode codeG;
				char ncchar;

				// /////////////////
				// 代入の実行
				// /////////////////
				foreach (CodeD codeD in ocode.codeData) {
					if (codeD.ncChar == '#' && codeD.SetVariable) {
						this[codeD.macVNo.Value] = codeD.macData;
						break;
					}
				}

				// //////////////////////////////
				// Ｇコードのグループの数値を設定
				// //////////////////////////////
				for (int ii = 0; ii <= 21; ii++) {
					if ((codeG = ocode.Gg[ii]).Gst) {
						if (codeG.MacroCall) continue;
						this.SystemSet(codeG.group, codeG);
					}
				}

				// //////////////////////////////
				// ＮＣデータコードの数値を設定
				// //////////////////////////////
				foreach (CodeD codeD in ocode.codeData) {
					ncchar = codeD.ncChar;
					if (codeD.ncChar == ';') continue;
					if (codeD.ncChar == '/') continue;
					if (codeD.ncChar == '(') continue;
					if (codeD.ncChar == '%') continue;
					if (codeD.ncChar == '#') continue;

					if (ocode.Gg[0].Equals(4) && ncchar == 'X') continue; /* G04 で X の場合 */
		
					if (ocode.Gg[0].Equals(65) || ocode.subdep1.gg12.Equals(66.1) || (ocode.Gst12 && ocode.subdep1.gg12.Equals(66))) {
						switch (ncchar) {
						case 'N':
						case 'O':
							break;
						default:
							continue;
						}
					}
					switch (ncchar) {
					case 'B':
					case 'D':
					case 'E':
					case 'H':
					case 'M':
					case 'N':
					case 'O':
					case 'S':
					case 'T':
						stemp[4300 + codeD.MacroNo - 400] = codeD.ToDouble;
						break;
					case 'F':
						if (ocode.nrmFeed == true) {
							stemp[4300 + codeD.MacroNo - 400] = codeD.ToDouble;
						}
						else {
							System.Windows.Forms.MessageBox.Show("Ｆ一桁送りの設定です");
							stemp[4300 + codeD.MacroNo - 400] = (double)ParaData((int)codeD.ToDouble + 1450, 0) / Post.PostData[ncchar].sdgt;
						}
						break;
					}
				}
			}
		}
	}
}
