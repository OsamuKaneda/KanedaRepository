using System;
using System.Collections.Generic;
using System.Text;

using CamUtil;

namespace NcCode.nccode
{
	/// <summary>
	/// ＮＣデータ１行のＮＣコードを、設定順に保存した CodeD のリストで表します。
	/// </summary>
	/// <remarks>
	/// codedList に関して、Add(), Clear() はコンストラクタのみで使用しているが、RemoveAt(), Insert() はその他で使用しているため[不変]ではありません。
	/// </remarks>
	public sealed class CodeDList : IEnumerable<CodeD>, System.Collections.IEnumerable
	{
		#region IEnumerable メンバ
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return codedList.GetEnumerator();
		}
		#endregion

		IEnumerator<CodeD> IEnumerable<CodeD>.GetEnumerator() {
			return codedList.GetEnumerator();
		}


		// ///////////
		// パラメータ
		// ///////////

		/// <summary>ＮＣデータ１行を解析した場合に設定される１行の文字列をあらわします</summary>
		public string NcText { get { return m_NcText; } }
		private readonly string m_NcText;

		/// <summary>ＮＣのコード情報を行内指定順に保存されています</summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public CodeD this[int index] { get { return codedList[index]; } }
		private readonly List<CodeD> codedList;

		/// <summary>実際に格納されている要素の数です</summary>
		public int Count { get { return codedList.Count; } }

		/// <summary>ＮＣコードごとのＮＣデータ１行内での指定数です（A:1 B:2 C:3 D:4 ... ）</summary>
		/// <param name="ncc">ＮＣコード</param>
		public int CodeCount(char ncc) { return codedList.FindAll(cd => cd.ncChar == ncc).Count; }

		/// <summary>コード別にＮＣデータ内容を返します</summary>
		/// <param name="ncc">ＮＣコード</param>
		public CodeD CodeData(char ncc) {
			switch (ncc) {
			case '/':	// DWELL
				return this.xDwell ? codedList.FindLast(cd => cd.ncChar == 'X') : null;
			case '(':	// MACRO
				return codedList.FindLast(cd => cd.ncChar == '#');
			case 'X':
				return this.xDwell ? null : codedList.FindLast(cd => cd.ncChar == 'X');
			default:
				return codedList.FindLast(cd => cd.ncChar == ncc);
			}
		}

		/// <summary>この行がＸを用いたドウェルを表す場合にtrueとなります</summary>
		private readonly bool xDwell;

		/// <summary>XYZの座標値の設定状況を返します</summary>
		/// <param name="g3">この座標値があるＮＣデータのＧコードグループ３の値（90 or 91）です。移動の設定値を得る場合に指定します</param>
		/// <returns></returns>
		public NcZahyo XYZ(int? g3) {
			if (g3.HasValue)
				// 移動の位置座標値を得る
				return new NcZahyo((short)g3,
					CodeCount('X') > 0 ? CodeData('X').ToDouble : (double?)null,
					CodeCount('Y') > 0 ? CodeData('Y').ToDouble : (double?)null,
					CodeCount('Z') > 0 ? CodeData('Z').ToDouble : (double?)null);
			else
				// プログラマブルミラーの基準座標値を得る
				return new NcZahyo(
					CodeCount('X') > 0 ? CodeData('X').ToDouble : (double?)null,
					CodeCount('Y') > 0 ? CodeData('Y').ToDouble : (double?)null,
					CodeCount('Z') > 0 ? CodeData('Z').ToDouble : (double?)null);
		}
		/// <summary>XY_の座標値の設定状況です</summary>
		/// <param name="g3">この座標値があるＮＣデータのＧコードグループ３の値（90 or 91）です</param>
		public NcZahyo XY_(int g3) {
			return new NcZahyo((short)g3,
				CodeCount('X') > 0 ? CodeData('X').ToDouble : (double?)null,
				CodeCount('Y') > 0 ? CodeData('Y').ToDouble : (double?)null,
				(double?)null);
		}
		/// <summary>ABCの座標値の設定状況</summary>
		public NcZahyo ABC(int g3) {
			return new NcZahyo((short)g3,
				CodeCount('A') > 0 ? CodeData('A').ToDouble : (double?)null,
				CodeCount('B') > 0 ? CodeData('B').ToDouble : (double?)null,
				CodeCount('C') > 0 ? CodeData('C').ToDouble : (double?)null);
		}

		// ///////////////
		// コンストラクタ
		// ///////////////

		/// <summary>
		/// List&gt;CodeD&lt;からOCodeに保存されるCodeDListを作成します。G04の処理はここで実行されます。
		/// </summary>
		/// <param name="list"></param>
		internal CodeDList(List<CodeD> list) {
			bool g04 = false;

			codedList = new List<CodeD>();
			m_NcText = "";
			xDwell = false;

			foreach (CodeD item in list) {
				m_NcText += item.String();
				//";ABCDEFGHIJKLMNOPQRSTUVWXYZ/(%#"
				switch (item.ncChar) {
				case ';':
				case '/':
				case '(':
				case '%':
					break;
				case '#':	// マクロ
					break;
				default:
					if (item.ncChar == 'G' && item.ToInt_OK && item.ToInt == 4) g04 = true;
					if (item.ncChar == 'X' && g04) {
						//idat[27] = (short)codedList.Count;	// Ｘを用いたドウェルの設定
						xDwell = true;
					}
					break;
				}
				codedList.Add(item.Clone());
			}
		}

		/// <summary>
		/// ＮＣデータ１行を解析し、ＮＣデータそのままのCodeDのリストを作成します。これを元にOCodeが作成されます。
		/// </summary>
		/// <param name="ddat">ＮＣデータ行</param>
		/// <param name="fsub">使用する変数値</param>
		internal CodeDList(string ddat, NcMachine.Variable fsub) {
			m_NcText = ddat;
			xDwell = false;
			codedList = GetList(ddat, fsub);
		}
		private List<CodeD> GetList(string ddat, NcMachine.Variable fsub) {
			List<CodeD> codedList = new List<CodeD>();
			string[] ncList = CamUtil.StringCAM.SplitNcCode(ddat);
			St_vsw v2;
			bool macroLine = false;
			bool loopEnd = false;
			short epoi;

			foreach (string ss in ncList) {
				epoi = 1;
				switch (ss[0]) {
				case ';':
					codedList.Add(new CodeD(ss[0], ss.Trim(), null));
					break;
				case '/':
				case '%':
				case '(':
					codedList.Add(new CodeD(ss[0], ss, null));
					break;
				case '#':   // マクロ変数への代入
							// 代入する値の計算や代入はnccod1の時に実行する
					{
						// マクロ変数を取得
						int jtmp;
						v2 = new St_vsw(ss, ref epoi, '\0', fsub);
						if (epoi < 0 || v2.Sswbl == false) throw new Exception("awerfbwerfhb");
						jtmp = (int)Math.Floor(0.5 + v2.Rdt);
						if (jtmp < 0 || jtmp == 2000 || (jtmp >= 1000 && jtmp <= 1035)) throw new Exception("qwedfqwevg");
						while (ss[epoi] == ' ') epoi++;

						// マクロ代入値を取得
						if (ss[epoi] != '=') throw new Exception("ＮＣデータコードエラー " + ddat);
						epoi++;
						v2 = new St_vsw(ss + ";", ref epoi, 0, '\0', fsub);
						if (epoi < 0) throw new Exception("wedfbqwefhbh");

						if (v2.Sswbl)
							codedList.Add(new CodeD(ss, jtmp, v2.Rdt));
						else
							codedList.Add(new CodeD(ss, jtmp, (double?)null));
					}
					break;
				default:
					// NORMAL NCDATA
					if (Char.IsUpper(ss[1]) == false) {
						v2 = new St_vsw(ss, ref epoi, ss[0], fsub);
						codedList.Add(new CodeD(v2, ss, macroLine));
						if (ss[0] == 'G') {
							if (codedList[codedList.Count - 1].Equals(65.0)) macroLine = true;
							if (codedList[codedList.Count - 1].Equals(66.0)) macroLine = true;
							if (codedList[codedList.Count - 1].Equals(66.1)) macroLine = true;
						}
					}
					// MACRO STATMENT
					else {
						while (epoi < ss.Length && Char.IsLetter(ss[epoi])) epoi++;
						switch (ss.Substring(0, epoi)) {
						case "THEN":
							codedList.Add(new CodeD('#', ss, null));
							break;
						case "IF":
						case "WHILE":
							v2 = new St_vsw(ss, ref epoi, '\0', fsub);
							codedList.Add(new CodeD('#', ss, v2.bdt.Value ? -1 : 0));
							break;
						case "GOTO":
						case "END":
						case "DO":
							v2 = new St_vsw(ss, ref epoi, '\0', fsub);
							if (epoi >= 0 && v2.Sswbl) {
								int itmp = (int)Math.Floor(0.5 + v2.Rdt);
								codedList.Add(new CodeD('#', ss, itmp));
							}
							else
								loopEnd = true;
							break;
						case "BPRNT":
						case "DPRNT":
						case "POPEN":
						case "PCLOS":
						default:
							loopEnd = true;
							break;
						}
					}
					break;
				}
				if (loopEnd) break;
			}
			return codedList;
		}

		// ///////////
		// メソッド
		// ///////////

		/// <summary>要素を新しい配列にコピーします</summary>
		internal CodeD[] ToArray() { return codedList.ToArray(); }

		/// <summary>末尾に要素を追加します。（内部処理用の単純追加）</summary>
		private void Add(CodeD item) {
			codedList.Add(item);
		}

		/// <summary>
		/// 指定したインデックスの位置にある要素を入れ替えます
		/// </summary>
		/// <param name="index">インデックス</param>
		/// <param name="item">入れ替える要素</param>
		private void Replace(int index, CodeD item) {
			codedList.RemoveAt(index);
			codedList.Insert(index, item);
		}

		/// <summary>このインスタンスと指定した CodeDList オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public bool Equals(CodeDList obj) {
			if (this.codedList.Count != obj.codedList.Count) return false;
			for (int ii = 0; ii < this.codedList.Count; ii++)
				if (!this.codedList[ii].Equals(obj.codedList[ii])) return false;
			if (this.m_NcText != obj.m_NcText) return false;
			if (this.xDwell != obj.xDwell) return false;

			return true;
		}
	}
}
