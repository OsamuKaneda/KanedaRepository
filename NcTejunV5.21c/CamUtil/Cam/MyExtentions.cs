using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamUtil
{
	/// <summary>
	/// 拡張メソッド
	/// </summary>
	public static class MyExtentions
	{
		/// <summary>
		/// 指定した文字列内の文字だけを含むこのインスタンスの先頭からの最大の長さをレポートします。
		/// </summary>
		/// <param name="source">検索される文字列</param>
		/// <param name="str">検査する文字列</param>
		/// <returns>先頭からの長さ</returns>
		public static int Span(this string source, string str) {
			//if( strspn(ddat, "-X0123456789. ") != ddat.Length)
			for (int jj = 0; jj < source.Length; jj++)
				if (str.IndexOf(source[jj]) < 0) return jj;
			return source.Length;
		}
		/// <summary>
		/// 指定した文字列内の文字だけを含むこのインスタンスの指定位置からの最大の長さをレポートします。
		/// </summary>
		/// <param name="source">検索される文字列</param>
		/// <param name="str">検査する文字列</param>
		/// <param name="start">検査を開始する文字列の位置</param>
		/// <returns>指定位置からの長さ</returns>
		public static int Span(this string source, string str, int start) {
			for (int jj = start; jj < source.Length; jj++)
				if (str.IndexOf(source[jj]) < 0) return jj - start;
			return source.Length - start;
		}
		/// <summary>
		/// ＣＳＶファイルの処理のため、カンマで区切られた文字列を部分文字列に分割します。
		/// 先頭とコンマ直後、それと呼応するダブルクォテーションは文字ではなく引用符として扱い、
		/// またその囲まれた内部のカンマでは区切らない点が Split() と異なります。
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static string[] SplitCsv(this string source) {
			List<string> sout = new List<string>();
			char[] chr2 = { '"' };
			int sttChr, endChr = -1;
			string buff;

			do {
				sttChr = endChr + 1;
				if (sttChr == source.Length) { sout.Add(""); break; } // 空の文字列あるいは最後のコンマ
				buff = "";
				if (source[sttChr] == '"') {
					endChr = source.IndexOf('"', sttChr + 1);
					buff = source.Substring(sttChr, (endChr >= 0 ? endChr : source.Length) - sttChr).Trim(chr2);
					sttChr = endChr >= 0 ? endChr + 1 : source.Length;
				}
				endChr = source.IndexOf(',', sttChr);
				sout.Add(buff + source.Substring(sttChr, (endChr >= 0 ? endChr : source.Length) - sttChr));
			} while (endChr >= 0);
			return sout.ToArray();
		}

		/// <summary>
		/// 指定されたキー セレクター関数の区切り条件に従いシーケンスの要素をグループ化します。
		/// 条件を満たした要素が次のグループの先頭になります。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static IEnumerable<IEnumerable<T>> GroupUntil<T>(this IEnumerable<T> source, Func<T, bool> selector) {
			List<T> result = new List<T>();
			foreach (var item in source) {
				if (result.Count > 0) if (selector(item)) {
						yield return result;
						result = new List<T>();
					}
				result.Add(item);
			}
			yield return result;
		}
		/// <summary>
		/// 指定されたキー セレクター関数の変化を区切りにシーケンスの要素をグループ化します。
		/// 同一の値でも連続していなければ別のグループとなるところが GroupBy と異なります。
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static IEnumerable<IEnumerable<T>> GroupUntilChanged<T>(this IEnumerable<T> source, Func<T, object> selector) {
			List<T> result = new List<T>();
			foreach (var item in source) {
				if (result.Count > 0) if (!object.Equals(selector(result[0]), selector(item))) {
						yield return result;
						result = new List<T>();
					}
				result.Add(item);
			}
			yield return result;
		}
	}
}
