using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CamUtil
{
	/// <summary>
	/// ＮＣデータの文字列に使用するクラスです。
	/// </summary>
	public static class StringCAM
	{
		/// <summary>大文字のアルファベット</summary>
		public const string ABC0 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		/// <summary>ＮＣで使用するコード。大文字アルファベットと ;(%\n の３０文字。２番目以降の検出用</summary>
		private static readonly char[] ncCHAR0 = ";ABCDEFGHIJKLMNOPQRSTUVWXYZ(%\n".ToCharArray();
		/// <summary>ＮＣで使用するコード。大文字アルファベットと ;(%#\n の３１文字。２番目以降の検出用（シーケンス番号後）</summary>
		private static readonly char[] ncCHAR1 = ";ABCDEFGHIJKLMNOPQRSTUVWXYZ(%#\n".ToCharArray();
		/// <summary>ＮＣで使用するコード。大文字アルファベットと ;(%#/ の３１文字。最初の検出用</summary>
		private static readonly string ncCHARS = ";ABCDEFGHIJKLMNOPQRSTUVWXYZ(%#/";
		private static readonly string trimString = " \t\n";

		/// <summary>ＮＣのマクロ文のリスト</summary>
		private static readonly string[] macroCode = { "IF", "GOTO", "THEN", "WHILE", "DO", "END", "BPRNT", "DPRNT", "POPEN", "PCLOS" };
		/// <summary>ＮＣのマクロ文で、英文字で表される演算子と関数のリスト</summary>
		private static readonly string[] functionList = {
			"OR", "XOR", "AND", "MOD", "EQ", "NE", "GT", "LT", "GE", "LE",
			"SIN", "COS", "TAN", "SQRT", "ABS", "BIN", "BCD", "ROUND", "FIX", "FUP", "ACOS", "ASIN", "ATAN", "LN", "EXP"};

		/// <summary>
		/// ＮＣデータに含まれるＮＣコードの位置リストを出力する。最終データにはＮＣデータの文字数が入る。
		/// </summary>
		/// <param name="ncLine"></param>
		/// <returns>ＮＣコードの位置リスト。cf. "N1 G01 X100;"   { {0, 3}, {3, 7}, {7, 11}, {11, 12} }</returns>
		private static int[][] GetNcIndexList(string ncLine) {
			int cstt = 0, cnxt;

			// ＮＣコードの位置リストをここに作成する。最後は文字列長。cf. "N1 G01 X100;"   { 0, 3, 7, 11, 12 }
			List<int> indexList = new List<int>();

			// 最初の空白などを読み飛ばす
			while (cstt < ncLine.Length) {
				if (trimString.IndexOf(ncLine[cstt]) >= 0) { cstt++; continue; }
				// オプショナルスキップ'/'、マクロ代入文'#'も検出する
				if (ncCHARS.IndexOf(ncLine[cstt]) < 0) { throw new Exception("ＮＣデータにはないコードが検出された。NC = " + ncLine); }
				break;
			}

			while (cstt >= 0 && cstt < ncLine.Length) {
				if (ncLine[cstt] == '\n') {
					LogOut.CheckCount("StringCAM 049", false, "ＮＣデータ内に改行コードが見つかりました NC = " + ncLine);
					cstt++; continue;
				}
				// マクロ文の処理
				if (ncLine[cstt] == '#' || ncLine.Span(ABC0, cstt) > 1) {
					string mac;
					if (ncLine[cstt] == '#')
						mac = "#";
					else {
						mac = ncLine.Substring(cstt, ncLine.Span(ABC0, cstt));
						// ケーラムで使われた紙テープに透かし文字を入れるコード。空行として処理する
						if (mac == "PARTNO") return new int[0][];
						if (macroCode.Any(ss => ss == mac) == false) throw new Exception("マクロの開始文ではない文字 " + mac + " が検出された。");
					}
					if (mac == "THEN")
						cnxt = ncLine.IndexOf('#', cstt + mac.Length);       // マクロ代入文'#'を検出する
					else {
						cnxt = ncLine.IndexOfAny(ncCHAR0, cstt + mac.Length);
						// 検出された英文字がマクロ文の演算子・関数であれば、区切り文字とせずさらに検索する
						while (cnxt >= 0 && functionList.Any(ss => ss == ncLine.Substring(cnxt, ncLine.Span(ABC0, cnxt))))
							cnxt = ncLine.IndexOfAny(ncCHAR0, cnxt + ncLine.Span(ABC0, cnxt));
					}
				}
				// 一般文の処理
				else {
					if (ncLine[cstt] == '(') {
						// '('の場合は対応する')'まで１つの区切りとする
						cnxt = ncLine.IndexOf(')', cstt);
						if (cnxt < 0) throw new Exception("'('に対応する')'が見つかりませんでした。NC = " + ncLine);
						cnxt++;
					}
					else cnxt = cstt + 1;
					if (cnxt < ncLine.Length) {
						switch (ncLine[cstt]) {
						case '/':
						case '%':
						case 'O':
						case 'N':
							cnxt = ncLine.IndexOfAny(ncCHAR1, cnxt);    // マクロ代入文'#'も検出する
							break;
						default:
							cnxt = ncLine.IndexOfAny(ncCHAR0, cnxt);
							break;
						}
					}
				}
				indexList.Add(cstt);
				cstt = cnxt;
			}
			indexList.Add(ncLine.Length);
			return indexList.Zip(indexList.Skip(1), (item1, item2) => new int[] { item1, item2 }).ToArray();
		}
		/// <summary>
		/// ＮＣコード単位に分部分文字列を格納する文字列配列を返す。
		/// </summary>
		/// <param name="ncLine"></param>
		/// <returns></returns>
		public static string[] SplitNcCode(string ncLine) {
			return GetNcIndexList(ncLine).Select(item => ncLine.Substring(item[0], item[1] - item[0])).ToArray();
		}
		/// <summary>
		/// 指定したＮＣコードの開始位置と終了位置＋１の２つの整数値のリストを返す。ＮＣコードが存在しない場合は長さ０の配列を返す
		/// </summary>
		/// <param name="ncLine"></param>
		/// <param name="ncc"></param>
		/// <returns></returns>
		public static int[][] GetNcIndex(string ncLine, char ncc) {
			return GetNcIndexList(ncLine).Where(item => ncc == ncLine[item[0]]).ToArray();
		}
		/// <summary>
		/// 複数のＮＣコードの最初の開始位置と最後の終了位置＋１の２つの整数値を返す。連続不連続は問わない。ＮＣコードが存在しない場合はnullを返す。
		/// </summary>
		/// <param name="ncLine"></param>
		/// <param name="ncc"></param>
		/// <returns></returns>
		public static int[] GetNcIndex(string ncLine, char[] ncc) {
			int[][] indexList = GetNcIndexList(ncLine).Where(item => ncc.Any(cc => cc == ncLine[item[0]])).ToArray();
			return indexList.Length > 0 ? new int[] { indexList.First()[0], indexList.Last()[1] } : null;
		}
		/// <summary>
		/// ＮＣデータから指定のＮＣコードのリストを取り出す。指定のＮＣコードがない場合はnullを返す
		/// </summary>
		/// <param name="ncLine"></param>
		/// <param name="ncc"></param>
		/// <returns></returns>
		public static string[] GetNcCode(string ncLine, char ncc) {
			int[][] indexList = GetNcIndex(ncLine, ncc);
			if (indexList.Length == 0) return null;
			return indexList.Select(value => ncLine.Substring(value[0], value[1] - value[0])).ToArray();
		}
	}
}
