using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;

namespace CamUtil.LCode
{
	/// <summary>
	/// 簡易なＮＣデータ１行実行時の工具位置・モード・加工長などの情報を作成します。サブプログラム解析、マクロ変数には対応しません。
	/// </summary>
	/// <remarks>
	///	最初にコンストラクタを用いて初期化します
	///		例：NcLineCode ncCode = new NcLineCode(new double[] { 100.0 }, CamUtil.BaseNcForm.GENERAL)
	/// 次にNextLineで指定されたＮＣデータで順次更新します。ncCode自体は変更されない。
	///		例：NcLineCode ncCode2 = ncCode.NextLine(sr.ReadLine());
	/// CamUtil.LCode.NcQueue を用いるとキューに保存された NcLineCode により前後の参照が可能になります
	/// Ｇコードと固定サイクル、カスタムマクロのモーダルは処理していますが、サブプログラムは処理しません。
	/// xyzsf に保存される回転数、送り速度と工具位置はこのＮＣデータ行を実行した最終の情報となり穴底ではありません。
	/// このため、固定サイクル・カスタムマクロの穴底の工具位置はそれぞれのモーダル値にて確認します。
	/// </remarks>
	public partial class NcLineCode
	{
		/// <summary>NcForm.Name == "5AXIS"で早送りとみなす切削送り速度=80000mm/min</summary>
		public const int RF_5AXIS = 80000;

		/// <summary>ＮＣのマクロで使用する引数コード。GLNO は使用できない。</summary>
		public static char[] MACP = "ABCDEFHIJKMPQRSTUVWXYZ".ToCharArray();

		/// <summary>出力時の最小桁数</summary>
		public static int Keta(char cc) { return m_keta[cc - '@']; }
		private static int[] m_keta = new int[] {
		//   ;  A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P  Q  R  S  T  U  V  W  X  Y  Z
			-1, 0, 0, 0, 2, 0, 4, 2, 2, 0, 0, 0, 0, 2, 4, 4, 4, 0, 0, 5, 2, 0, 0, 0, 0, 0, 0 };

		/// <summary>NcLineCode 内のデータが保存されている数値の桁数情報</summary>
		public readonly static NcDigit GeneralDigit = new NcDigit();
		/// <summary>ＮＣデータのＸＹＺ座標値の整数化桁数</summary>
		private static int XyzDigit { get { return NcLineCode.GeneralDigit.Data['X'].sdgt.Value; } }

		/// <summary>固定サイクル、マクロ、モーダルを除く一般のＧコード</summary>
		private static int[] gels_Data ={
			//0,1,2,3,33,
			5,6,7,10,11,12,13,15,16,
			24,25,26,27,
			99,
			//40,41,42,
			43,44,49,
			30,31,
			53,54,28,
			//65,66,67,
			//73,72,80,				// del in 2016/06/24
			247,
			4,36,39,62,440,441,
			//17,18,19,				// del in 2016/06/24
			20,
			//90,91,
			//94,95,					// add 2014/12/24 部品加工機のリジッドタップ加工
			//70,71,79,				// del in 2016/06/24
			29,38,51,98,
			//100,
			105,					// add 2012/07/09 ＶＧの傾斜処理マクロのコール
			102						// add 2014/12/02 部品加工機の加工モード設定（ポスト変更するまでの暫定）
		};

		/// <summary>
		/// ＮＣデータの指定コードの数値に係数をかけます。小数点の有無は元のデータに合わせます。
		/// </summary>
		/// <param name="sline">ＮＣデータの一行</param>
		/// <param name="ncc">ＮＣコード（一文字）</param>
		/// <param name="rate">係数</param>
		/// <returns>１行のＮＣデータ出力</returns>
		public static string NcRateSF(string sline, char ncc, double rate) {
			return NcRateSF(sline, ncc, (double ncvalue) => ncvalue * rate);
		}
		/// <summary>
		/// ＮＣデータの指定コードの数値を元に新しい値を計算します。小数点の有無は元のデータに合わせます。
		/// </summary>
		/// <param name="sline">ＮＣデータの一行</param>
		/// <param name="ncc">ＮＣコード（一文字）</param>
		/// <param name="rset">新ＮＣデータ値の変換式</param>
		/// <returns>１行のＮＣデータ出力</returns>
		public static string NcRateSF(string sline, char ncc, Func<double, double> rset) {
			int[] index = StringCAM.GetNcIndex(sline, ncc)[0];
			int stt = index[0] + 1, len = index[1] - index[0] - 1;
			double sf = rset(Convert.ToDouble(sline.Substring(stt, len)));
			string insstr = sline.Substring(stt, len).Contains(".") ? sf.ToString("0.0###") : ((long)Math.Round(sf)).ToString("0");
			return sline.Remove(stt, len).Insert(stt, insstr);
		}
		/// <summary>
		/// ＮＣデータの指定コードの数値を元に新しい値を計算します。小数点の有無はinteger に従います。
		/// </summary>
		/// <param name="sline">ＮＣデータの一行</param>
		/// <param name="ncc">ＮＣコード（一文字）</param>
		/// <param name="rset">新ＮＣデータ値の変換式</param>
		/// <param name="integer">整数で出力する場合true</param>
		/// <returns>１行のＮＣデータ出力</returns>
		public static string NcRateSF(string sline, char ncc, Func<double, double> rset, bool integer) {
			int[] index = StringCAM.GetNcIndex(sline, ncc)[0];
			int stt = index[0] + 1, len = index[1] - index[0] - 1;
			//string format = sline.Substring(stt, len).Contains(".") ? "0.0" : "0";
			double sf = rset(Convert.ToDouble(sline.Substring(stt, len)));
			string ss = integer ? ((long)Math.Round(sf)).ToString("0") : sf.ToString("0.0###"); // 整数化に従来に合わせるためRoundを使用する
			return sline.Remove(stt, len).Insert(stt, ss);
		}

		/// <summary>
		/// ＮＣデータの指定コードの文字列を指定の文字列で入れ替えます
		/// </summary>
		/// <param name="sline">ＮＣデータの一行</param>
		/// <param name="ncc">ＮＣコード</param>
		/// <param name="text">入れ替える文字列</param>
		/// <returns>１行のＮＣデータ出力</returns>
		public static string NcSetValue(string sline, char ncc, string text) {
			int[] index = StringCAM.GetNcIndex(sline, ncc)[0];
			return sline.Remove(index[0], index[1] - index[0]).Insert(index[0], ncc.ToString() + text);
		}

		/// <summary>
		/// ＮＣデータの指定コードを削除します
		/// </summary>
		/// <param name="sline">ＮＣデータの一行</param>
		/// <param name="ncc">ＮＣコード（一文字）</param>
		/// <returns>１行のＮＣデータ出力</returns>
		public static string NcDelChar(string sline, char ncc) {
			int[] index = StringCAM.GetNcIndex(sline, ncc)[0];
			//return sline.Replace(sline.Substring(jj, kk - jj), "");
			return sline.Replace(sline.Substring(index[0], index[1] - index[0]), "");
		}
		/// <summary>
		/// ＮＣデータの指定コードの前あるいは後に文字列を追加挿入します
		/// </summary>
		/// <param name="sline">ＮＣデータの一行</param>
		/// <param name="ncc">ＮＣコード（一文字）</param>
		/// <param name="text">追加挿入する文字列</param>
		/// <param name="ins">指定コードの前に挿入する場合はtrue、後ろに追加する場合はfalse。</param>
		/// <returns>１行のＮＣデータ出力</returns>
		public static string NcInsertChar(string sline, char ncc, string text, bool ins) {
			int[][] index = StringCAM.GetNcIndex(sline, ncc);
			if (index.Length != 1) throw new Exception("追加挿入すべき場所のＮＣコードが見つかりませんでした。");
			return ins ? sline.Insert(index[0][0], text) : sline.Insert(index[0][1], text);
			
		}
		/// <summary>
		/// ＮＣデータの文字列で指定された複数のコードの前後に文字列を追加挿入します。
		/// 前に挿入する場合は最初に見つかったコード、後ろに追加する場合は最後に見つかったコードが対象となります。
		/// </summary>
		/// <param name="sline">ＮＣデータの一行</param>
		/// <param name="ncc">ＮＣコード（複数）</param>
		/// <param name="text">追加挿入する文字列</param>
		/// <param name="ins">指定コードの前に挿入する場合はtrue、後ろに追加する場合はfalse。</param>
		/// <returns>１行のＮＣデータ出力</returns>
		public static string NcInsertChar(string sline, char[] ncc, string text, bool ins) {
			int[] index = StringCAM.GetNcIndex(sline, ncc);
			if (index == null) throw new Exception("追加挿入すべき場所のＮＣコードが見つかりませんでした。");
			return ins ? sline.Insert(index[0], text) : sline.Insert(index[1], text);
		}

		/// <summary>コメントの開始終了キー（0;ファナック、1:ハイデンハイン）</summary>
		private static string[] comma_Data = { "()", ";" };

		/// <summary>Ｇコードグループ09（固定サイクル）であれば true、そうでなければ false。</summary>
		private static bool G_Fix(int gcode) {
			return (70 <= gcode && gcode < 90);
		}
		/// <summary>主要Ｇコードグループ（01, 02, 03, 05, 07, 09, 12）とG100, G65 以外のエラーとしないＧコード</summary>
		private static bool G_Else(int gcode) {
			foreach (int ii in gels_Data) if (ii == gcode) return true;
			return false;
		}

		/// <summary>コメントの開始キー（１文字目必須）と終了キー（２文字目任意）の１文字か２文字</summary>
		private static string CommStEnd { get { return comma_Data[0]; } }

		// ////////////////////////////////////////////////////
		// 以上、静的
		// ////////////////////////////////////////////////////












		/// <summary>ＮＣデータのフォーマット</summary>
		private readonly BaseNcForm ncForm = BaseNcForm.EMPTY;
		/// <summary>入力ＮＣデータの座標値、桁数（すべて同一のpostを参照するためナロ－コピーで可）</summary>
		public readonly NcDigit post;

		/// <summary>現在のＮＣデータの状況（-1:有意データ前、0:有意データ（最初の%以後）、1:M02,M30以降）</summary>
		public int Start { get; private set; }
		/// <summary>工具ごとのクリアランス高さ</summary>
		private double[] apz;

		/// <summary>この行でのモード指定の有無（G100）	注意：b_g6はtrueにはならない、g6は67である</summary>
		public bool B_g100 { get { return NumList.Exists(num => num.ncChar == 'G' && num.L == 100); } }
		/// <summary>この行でのモード指定の有無（G00,G01,G02,G03,G33）</summary>
		public bool B_g1 { get { return NumList.Exists(num => num.ncChar == 'G' && (num.L <= 3 || num.L == 33)); } }
		/// <summary>この行でのモード指定の有無（G40,G41,G42）</summary>
		public bool B_g4 { get { return NumList.Exists(num => num.ncChar == 'G' && num.L >= 40 && num.L <= 42); } }
		/// <summary>この行でのモード指定の有無（カクタムマクロコード）</summary>
		public bool B_g6 { get { return NumList.Exists(num => num.ncChar == 'G' && num.L >= 65 && num.L <= 67); } }
		/// <summary>この行でのモード指定の有無（固定サイクルコード）</summary>
		public bool B_g8 { get { return NumList.Exists(num => num.ncChar == 'G' && G_Fix((int)num.L)); } }
		/// <summary>この行でのM98P6の指定</summary>
		public bool B_p0006 { get { return SubPro.HasValue ? (SubPro.Value == 6 ? true : false) : false; } }
		//private bool m_b_g100, m_b_g1, m_b_g2, m_b_g4, m_b_g6, m_b_g8, m_b_g9;

		/// <summary>Ｇコードグループ01 移動モード(G00,G01,G02,G03,G33)</summary>
		public short G1 { get; private set; }
		/// <summary>Ｇコードグループ02 平面モード(G17,G18,G19)</summary>
		public short G2 { get; private set; }
		/// <summary>Ｇコードグループ07 オフセットモード(G40,G41,G42)</summary>
		public short G4 { get; private set; }
		/// <summary>Ｇコードグループ05 送り速度モード（毎分G94/毎回転G95）</summary>
		public short G5 { get; private set; }
		/// <summary>Ｇコードグループ12 カクタムマクロコード（G66,G67）</summary>
		public short G6 { get; private set; }
		/// <summary>Ｇコードグループ09 固定サイクルコード（G70～G89）</summary>
		public short G8 { get; private set; }
		/// <summary>Ｇコードグループ03 絶対相対モード(ABSG90,INCG91)</summary>
		public short G9 { get; private set; }

		/// <summary>ＮＣデータ１行のＮＣコードリスト</summary>
		public NumList NumList { get { return m_numList.AsReadOnly; } }
		/// <summary>ＮＣデータ１行のＮＣコードリストnumListを設定するフィールド</summary>
		protected NumList.NumListData m_numList;
		//public CamUtil.RO_List<numCode> numList; private List<numCode> m_numList;



		/// <summary>工具連番（０から）</summary>
		public int Tcnt { get; private set; }
		/// <summary>ＮＣデータ単位の行番号（１から）</summary>
		public long LnumN { get; private set; }
		/// <summary>工具単位の行番号（１から）</summary>
		public long LnumT { get; private set; }

		/// <summary>移動後の位置（X,Y,Z,A,B,C）と（S,F）の値</summary>
		public Xisf Xyzsf { get; private set; }
		/// <summary>固定サイクルのモード情報</summary>
		public CycleMode G8p { get; private set; }
		/// <summary>カスタムマクロのモード情報</summary>
		public MacroMode G6p { get; private set; }
		/// <summary>サブルーチンコールがある場合そのプログラム番号</summary>
		public int? SubPro { get; private set; }
		/// <summary>終了コードM02,M30がある場合 true</summary>
		private bool M02 { get { return NumList.Exists(num => (B_g6 == false || G6 == 67) && num.ncChar == 'M' && (num.L == 2 || num.L == 30)); } }
		//private bool m02;

		/// <summary>ＮＣデータのコメントを除く入力行</summary>
		public string NcLine { get; private set; }

		/// <summary>出力する複数のＮＣデータ行</summary>
		public OutLine OutLine { get; private set; }

		/// <summary>コメントを出力する場合は true、出力しない場合は false。</summary>
		private readonly bool m_commentOutput;
		/// <summary>既定のフォーマットに変換されたＮＣデータの場合は true、ＣＡＭから出力されたままでカスタムマクロがG65からG66に変換される前のＮＣデータの場合は false。</summary>
		private readonly bool regular;

		/// <summary>
		/// 開始行を作成します
		/// </summary>
		/// <param name="p_apz">開始のＺ座標値。工具ごとに指定します</param>
		/// <param name="baseForm">ＮＣの基本フォーマット名（"GENERAL" or "_5AXIS"）</param>
		/// <param name="postInput">入力側のＮＣデータ（ＣＡＭポストなど）の小数点桁数。入力側がＰＴＰの場合はNcDigit.GeneralDigit を用いる</param>
		/// <param name="p_commentOutput">出力時にＮＣデータとともにコメントも出力する場合は true</param>
		/// <param name="p_regular">既定のフォーマットに変換されたＮＣデータの場合は true</param>
		public NcLineCode(double[] p_apz, BaseNcForm baseForm, NcDigit postInput, bool p_commentOutput, bool p_regular) {
			this.ncForm = baseForm;
			this.post = postInput;
			if (ncForm.Id == CamUtil.BaseNcForm.ID.EMPTY) throw new Exception("NcFormが定義できません。");

			this.apz = p_apz;
			this.m_commentOutput = p_commentOutput;
			this.regular = p_regular;

			//ResetLine();
			{
				// g8pの初期化でこの設定はおかしい。new CycleMode(80);に変更すべきでは in 2017/12/26
				this.G8p = this.G8p.ResetLine();
				this.SubPro = null;
				this.m_numList = new NumList.NumListData();
				this.NcLine = null;
				//this.lnumb = 0;
				this.OutLine = new OutLine(m_commentOutput);
			}

			G1 = 0;
			G2 = 17;
			G4 = 40;
			G5 = 94;
			G9 = 90;
			G6 = 67;
			G8 = 80;

			Start = -1;
			Tcnt = -1;
			LnumN = 0;
			LnumT = -1;

			//m_source = null;
			Xyzsf = new Xisf(0);

			// g6p 未設定であるため以下のコードを追加する必要あるが他のコードの検証のため保留 in 2017/12/26
			//this.g6p = new MacroMode(0);
		}
		/// <summary>
		/// ディープコピーによりクローンを作成します。
		/// ディープコピーコンストラクタより約３０％コストが低いためこちらを使用します。
		/// </summary>
		public NcLineCode Clone() {
			//ナローコピーの実行
			NcLineCode clone = (NcLineCode)this.MemberwiseClone();
			// ncLine String(不変)
			// NcForm 値型（メンバーの参照型はStringのみ）
			// xyzsf 値型（メンバーもすべて値型）
			// g8p 値型（メンバーもすべて値型）
			// g6p 値型（メンバーもすべて値型）

			// 以下にimmutable(不変)ではない参照型のオブジェクトのコピーを実行
			clone.apz = this.apz != null ? (double[])this.apz.Clone() : (double[])null;
			clone.OutLine = this.OutLine.Clone();
			clone.m_numList = new NumList.NumListData(this.NumList);
			return clone;
		}
		/// <summary>
		/// ディープコピーを作成します
		/// </summary>
		/// <param name="src"></param>
		protected NcLineCode(NcLineCode src) {
			this.ncForm = src.ncForm;
			this.post = src.post;
			this.Start = src.Start;
			this.G1 = src.G1;
			this.G2 = src.G2;
			this.G4 = src.G4;
			this.G5 = src.G5;
			this.G6 = src.G6;
			this.G8 = src.G8;
			this.G9 = src.G9;
			this.Tcnt = src.Tcnt;
			this.LnumN = src.LnumN;
			this.LnumT = src.LnumT;
			//this.lnumb = src.lnumb;
			this.SubPro = src.SubPro;
			this.NcLine = src.NcLine;
			this.m_commentOutput = src.m_commentOutput;
			this.regular = src.regular;

			this.Xyzsf = src.Xyzsf;
			this.G8p = src.G8p;
			this.G6p = src.G6p;
			//this.m_source = src.m_source;
			this.OutLine = src.OutLine.Clone();

			if (src.apz == null)
				this.apz = null;
			else {
				this.apz = new double[src.apz.Length];
				for (int ii = 0; ii < src.apz.Length; ii++) this.apz[ii] = src.apz[ii];
			}
			this.m_numList = new NumList.NumListData(src.NumList);

			if (!this.Equals(src)) throw new Exception("コピーコンストラクタのエラー");
		}

		/// <summary>
		/// ＣＡＭ出力ＮＣデータを解析し、前行の情報を最新情報に更新する。実行時には前行の情報が入っていること。
		/// もし前後のバッファーNcQueueを用いる場合はNcQueueのNextLineを用いるとバッファリングも同時に実行する
		/// </summary>
		/// <param name="readline">次行のＮＣデータ</param>
		public virtual void NextLine(string readline) {

			if (ncForm.Id == BaseNcForm.ID.EMPTY) throw new Exception("qwefbqerfhb");

			// ///////////////////
			// １行ごとの初期化
			// ///////////////////
			ResetLine();
			if (G6 == 65) {
				this.G6 = 67;
				this.G6p = new MacroMode(0);
			}

			// ライン番号増加
			LnumN++;
			if (LnumT > 0) LnumT++;

			// NcLine, OutLineの作成（コメントの抜き出し）
			Set_Line(readline);

			// 開始終了行の時はstartのセット以外は何もしない
			// numListの作成を追加 in 2016/09/06
			if (Start_End_Code(NcLine)) {
				if (Start < 0)
					Start = 0;  // 有意データ
				m_numList = new NumList.NumListData(new NumCode('%', "", NcDigit.NcDigits.symbol));
				return;
			}

			// //////////////
			// numListの作成
			// //////////////
			try {
				//numLSet_GENERAL(ncLine);
				m_numList = new NumList.NumListData(NcLine, this.post);
			}
			catch (FormatException) {
				MessageBox.Show("ＮＣデータの文字列 '" + NcLine + "' が不正です",
					"NcCode", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
				throw;
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message + " NCDATA=" + NcLine,
					"NcCode", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
				throw;
			}

			// ////////////////////
			// Ｇコードのモーダル値をセット（g1,g2,g9,g4,g5,g6,g8）
			// ////////////////////
			GcodeSet_GENERAL();

			// ////////////////////
			// Ｍコードの処理
			// ////////////////////
			if (B_g6 == false || G6 == 67) {
				bool m98 = false;
				foreach (NumCode num in NumList) {
					if (num.ncChar == 'P' && m98) SubPro = (int)num.L;
					if (num.ncChar == 'M' && num.L == 98) m98 = true;
				}
			}

			// ////////////////////
			// P0006 によるリセット
			// ////////////////////
			if (SubPro.HasValue && SubPro.Value == 6) {
				G8 = 80;
				G6 = 67;
				G4 = 40;
			}

			// ////////////////////
			// 工具交換行の処理
			// ////////////////////
			if (B_g100) ResetTool();

			// ////////////////////
			// サイクルモードと座標値の設定
			// ////////////////////
			Set_Cycle_Xyz();

			// ////////////////////
			// M02,M30による終了設定
			// ////////////////////
			if (M02) Start = 1;

		}

		/// <summary>
		/// 指定コード（Ａ－Ｚ）が行内で存在するかどうかを判断します
		/// </summary>
		/// <param name="cc">存在を判断するＮＣコード</param>
		/// <returns>存在する場合は true</returns>
		public bool B_26(char cc) { return NumList.NcCount(cc) > 0; }

		/// <summary>
		/// 指定したＧコードが存在するかどうかを判断します。
		/// </summary>
		/// <param name="gcode">存在を判断するＧコード</param>
		/// <returns>存在すれば true</returns>
		public bool ExistsG(double gcode) { return NumList.Exists(num => num.ncChar == 'G' && Math.Abs(num.L - gcode) < 0.001); }

		/// <summary>
		/// ＮＣデータ１行に定義されたＮＣコードの情報を出力します
		/// </summary>
		/// <param name="ncchar">情報を得るコード</param>
		/// <returns>コードの情報。ない場合はnum_nullを返します</returns>
		public NumCode Code(char ncchar) { return this.NumList.Code(ncchar); }

		/// <summary>ＮＣデータ内のＸＹＺの座標値をベクトルで出力します。すべてが設定されていない場合はエラーとなります。</summary>
		public CamUtil.Vector3 XYZ {
			get {
				if (B_26('X') && B_26('Y') && B_26('Z')) return new CamUtil.Vector3(Code('X').dblData, Code('Y').dblData, Code('Z').dblData);
				throw new InvalidOperationException("ＸＹＺで未設定のデータがあります。NC=" + NcLine);
			}
		}
		/// <summary>ＮＣデータ内のＩＪＫの座標値をベクトルで出力します。設定されていないコードは0.0となります。</summary>
		public CamUtil.Vector3 IJK {
			get { return new CamUtil.Vector3(B_26('I') ? Code('I').dblData : 0.0, B_26('J') ? Code('J').dblData : 0.0, B_26('K') ? Code('K').dblData : 0.0); }
		}

		/// <summary>
		///	ＮＣデータの開始終了行の判定をします
		/// </summary>
		/// <param name="txt">ＮＣデータ</param>
		/// <returns>開始あるいは終了行（'%'が１文字目）である場合 true。</returns>
		public bool Start_End_Code(string txt) {
			return (txt.Length == 0) ? false : (txt[0] == '%');
		}

		/// <summary>
		/// 初期化。ＮＣデータ１行ごとに実行
		/// </summary>
		private void ResetLine() {

			//m_b_g100 = m_b_g1 = m_b_g2 = m_b_g4 = m_b_g6 = m_b_g8 = m_b_g9 = false;
			//this.b_26 = new setcode(0);
			this.G8p = this.G8p.ResetLine();
			//this.g6p.ResetLIne();
			this.SubPro = null;
			//this.m02 = false;

			this.m_numList = new NumList.NumListData();

			this.NcLine = null;
			//this.m_comment = "";

			//this.lnumb = 0;

			this.OutLine = new OutLine(m_commentOutput);
		}
		/// <summary>
		/// 工具交換時の初期化。G100時にコールされる
		/// </summary>
		private void ResetTool() {

			// 初期化しないもの
			//OutLine;
			//NcLine;
			//comment;
			//m_lnumN;
			//numList.Clear();

			this.Tcnt++;
			this.LnumT = 1;

			if (this.apz == null) {
				Xyzsf = new Xisf(9999.9);
			}
			else {
				if (Tcnt >= this.apz.Length) throw new Exception("qwefbqefhbqh");
				Xyzsf = new Xisf(apz[this.Tcnt]);
			}
			G6p = new MacroMode(0);
			G8p = new CycleMode(G8);

			G1 = 0;
			G2 = 17;
			G4 = 40;
			G5 = 94;
			G9 = 90;
			G6 = 67;
			G8 = 80;
		}
		/// <summary>
		/// NcLine, OutLineの作成
		/// </summary>
		private void Set_Line(string readline) {
			int cstt;
			int cnxt;
			string comm = "";

			if ((cstt = readline.IndexOf(CommStEnd[0])) >= 0) {
				// コメントを閉じる記号がある場合
				if (CommStEnd.Length >= 2) {
					cnxt = readline.IndexOf(CommStEnd[1], cstt + 1);
					comm += readline.Substring(cstt + 1, cnxt - 1 - cstt);
					this.NcLine = readline.Remove(cstt, cnxt + 1 - cstt);
				}
				// コメントを閉じる記号がない場合（以下すべてコメント）
				else {
					comm += readline.Substring(cstt + 1);
					this.NcLine = readline.Substring(0, cstt);
				}
			}
			else
				this.NcLine = readline;

			// コメント
			//m_comment = comm;

			// outLineのセット
			this.OutLine.Set(NcLine, comm);
		}
		private void GcodeSet_GENERAL() {
			foreach (NumCode num in NumList) {
				if (num.ncChar != 'G') continue;
				if (num.decim) continue;

				switch (num.L) {
				case 100:
					break;
				case 0:
				case 1:
				case 2:
				case 3:
				case 33:
					G1 = (short)num.L; break;
				case 17:
				case 18:
				case 19:
					G2 = (short)num.L; break;
				case 90:
				case 91:
					G9 = (short)num.L; break;
				case 40:
				case 41:
				case 42:
					G4 = (short)num.L; break;
				case 94:
				case 95:
					G5 = (short)num.L; break;
				case 65:
				case 66:
				case 67:
					G6 = (short)num.L; break;
				default:
					// G コードの場合
					if (G_Fix((int)num.L)) {
						this.G8 = (short)num.L;
					}
					// その他のＧコード以外の場合
					else if (G_Else((int)num.L) == false)
						throw new Exception("処理できないＧコードG" + num.L + "が見つかりました");
					break;
				}
			}
		}
		private void Set_Cycle_Xyz() {
			if (B_g8) G8p = new CycleMode(G8);
			if (B_g6) G6p = new MacroMode(0);

			// 固定サイクルモード
			if (G8 != 80) {
				G8p = G8p.CycleSetL(NumList, G9, Xyzsf.Z);
				Xyzsf = Xyzsf.Next(ncForm, regular, NumList, G5, G9, (byte)1, (object)G8p);
			}
			// １ショットカスタムマクロ
			// カスタムマクロモード
			else if (G6 != 67) {
				byte mode;
				if (B_g6) {
					G6p = G6p.MacroSet(this.NumList);
					if (G6 == 65) mode = 3; else mode = 4;
				}
				else mode = 2;
				Xyzsf = Xyzsf.Next(ncForm, regular, NumList, G5, G9, mode, (object)G6p);
			}
			// 一般モード
			else {
				Xyzsf = Xyzsf.Next(ncForm, regular, NumList, G5, G9, (byte)0, (object)null);
			}
		}

		/// <summary>このインスタンスと指定した NcLineCode オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj が CamUtil.NcLineCode のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public bool Equals(NcLineCode obj) {
			if (this.ncForm.Id != obj.ncForm.Id) return false;
			//if (this.m_source != obj.m_source) return false;
			if (this.Start != obj.Start) return false;
			if (this.G1 != obj.G1) return false;
			if (this.G2 != obj.G2) return false;
			if (this.G4 != obj.G4) return false;
			if (this.G5 != obj.G5) return false;
			if (this.G6 != obj.G6) return false;
			if (this.G8 != obj.G8) return false;
			if (this.G9 != obj.G9) return false;
			if (this.Tcnt != obj.Tcnt) return false;
			if (this.LnumN != obj.LnumN) return false;
			if (this.LnumT != obj.LnumT) return false;
			//if (this.lnumb != obj.lnumb) return false;
			if (this.SubPro != obj.SubPro) return false;
			if (this.NcLine != obj.NcLine) return false;
			if (this.m_commentOutput != obj.m_commentOutput) return false;
			if (this.regular != obj.regular) return false;

			if ((this.apz == null) != (obj.apz == null)) return false;
			if (this.apz != null) {
				if (this.apz.Length != obj.apz.Length) return false;
				for (int ii = 0; ii < this.apz.Length; ii++) if (this.apz[ii] != obj.apz[ii]) return false;
			}

			if (this.Xyzsf != obj.Xyzsf) return false;
			if (this.G8p != obj.G8p) return false;
			if (this.G6p != obj.G6p) return false;
			if (!this.OutLine.Equals(obj.OutLine)) return false;
			if (this.NumList.Count != obj.NumList.Count) return false;
			for (int ii = 0; ii < this.NumList.Count; ii++) if (!this.NumList[ii].Equals(obj.NumList[ii])) return false;
			return true;
		}
	}
}
