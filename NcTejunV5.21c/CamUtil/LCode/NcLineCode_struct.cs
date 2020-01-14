using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil.LCode
{
	public partial class NcLineCode
	{
		/// <summary>
		/// ＮＣコード１件の情報を表します。小数点以下桁数はdigitList固定です。[不変]
		/// </summary>
		public class NumCode
		{
			/// <summary>ＮＣのコード文字</summary>
			public readonly char ncChar;

			/// <summary>マクロ変数への代入文（ncchar=='#'）の場合、その変数番号。その他の場合は０</summary>
			private readonly int macVNo;

			/// <summary>入力ＮＣデータが小数点付きの場合は true</summary>
			public readonly bool decim;

			/// <summary>ＮＣコードの数値部分のそのままの文字列。数値部分がない場合は空の文字列</summary>
			public string S { get { return strData.Length > 1 ? strData.Substring(1) : ""; } }
			/// <summary>ＮＣのコード全体の文字列</summary>
			private readonly string strData;

			/// <summary>ＮＣのコードの数値（実数）</summary>
			public readonly double dblData;

			/// <summary>ＮＣのコード整数化可否</summary>
			private bool ToInt_OK { get { return NcLineCode.GeneralDigit.Data[ncChar].axis || Math.Floor(dblData) == Math.Ceiling(dblData); } }

			/// <summary>ＮＣのコードの整数化した数値を出力します</summary>
			public long L {
				get {
					if (!ToInt_OK) throw new InvalidOperationException("qjefbqrhb");	// ADD in 2016/06/15
					if (NcLineCode.GeneralDigit.Data[ncChar].axis)
						return (long)Math.Round(dblData * NcLineCode.GeneralDigit.Data[ncChar].sdgt.Value);
					else
						return (long)Math.Round(dblData);
				}
			}

			/// <summary>ディープコピー（==ナローコピー）を作成します</summary>
			/// <returns>コピー</returns>
			public NumCode Clone() { return (NumCode)this.MemberwiseClone(); }

			/// <summary>
			/// ＮＣのコードとテキストから作成します。単一文字の %, ; や マクロ代入文 # も処理します
			/// </summary>
			/// <param name="p_ncchar">ＮＣコードの文字</param>
			/// <param name="ntxt">ＮＣコードの数値文字列</param>
			/// <param name="posts">ＮＣコード数値の桁数の処理方法</param>
			public NumCode(char p_ncchar, string ntxt, NcDigit.NcDigits posts) {
				this.ncChar = p_ncchar;
				this.strData = p_ncchar.ToString() + ntxt;
				//this.axis = digitList.Data[p_ncchar].axis;
				this.decim = false;
				this.dblData = 0.0;
				this.macVNo = 0;

				if (p_ncchar == '%') {
					return;
				}
				if (p_ncchar == ';') {
					throw new Exception("存在しないはずのＮＣコード';'が見つかりました。");
				}

				if (p_ncchar == '#') {
					string[] ff = ntxt.Split(new char[] { '=' });
					macVNo = Convert.ToInt32(ff[0]);
					ntxt = ff[1];
				}
				if (ntxt[0] == '#') {
					// マクロ変数は数値として処理しない
					;
				}
				else {
					decim = (ntxt.IndexOf('.') >= 0);
					if (decim)
						dblData = Convert.ToDouble(ntxt);
					else {
						if (posts.axis) {
							dblData = Convert.ToInt32(ntxt) / (double)posts.sdgt.Value;
						}
						else {
							dblData = (double)Convert.ToInt32(ntxt);
						}
					}
				}
			}

			/// <summary>
			/// ミラーしたＧコードを作成します
			/// G02 - G03 ,6 G41 - G42
			/// </summary>
			/// <param name="post">ＣＡＭから出力されたＮＣデータの数値桁数の処理方法</param>
			/// <returns></returns>
			public NumCode GCodeMirror(NcDigit post) {
				if (this.ncChar != 'G')
					throw new Exception("Ｇコードミラー処理でＧコード以外のデータを変更しようとしている");

				string ss = this.S;
				if (this.ToInt_OK) {
					switch (this.L) {
					case 2: ss = "3"; break;
					case 3: ss = "2"; break;
					case 41: ss = "42"; break;
					case 42: ss = "41"; break;
					}
				}
				return new NumCode('G', ss, post.Data['G']);
			}

			/// <summary>このインスタンスと指定した numCode オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
			/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
			/// <returns>obj が numCode のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
			public bool Equals(NumCode obj) {
				//if (this.axis != obj.axis) return false;
				if (this.ncChar != obj.ncChar) return false;
				if (this.macVNo != obj.macVNo) return false;
				if (this.decim != obj.decim) return false;
				if (this.strData != obj.strData) return false;
				if (this.dblData != obj.dblData) return false;
				return true;
			}
			/*
			/// <summary>このインスタンスのハッシュ コードを返します。</summary>
			/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
			public override int GetHashCode() {
				return axis.GetHashCode() ^ ncchar.GetHashCode() ^ macVNo ^ decim.GetHashCode() ^ sdat.GetHashCode() ^ ddat.GetHashCode();
			}
			*/
		}

		/// <summary>
		/// ＮＣデータの位置と回転送りの情報を保存します
		/// </summary>
		public readonly struct Xisf
		{

			// /////////////////////////////
			// 演算子のオーバーロード
			// /////////////////////////////
			/// <summary>ＮＣデータの位置と回転送りの情報の＝＝演算子</summary>
			public static bool operator ==(Xisf c1, Xisf c2) { return c1.Equals(c2); }
			/// <summary>ＮＣデータの位置と回転送りの情報の！＝演算子</summary>
			public static bool operator !=(Xisf c1, Xisf c2) { return !c1.Equals(c2); }




			/// <summary>送りモード現在（毎分/毎回転）</summary>
			private readonly short g94;
			/// <summary>送りモード直前（毎分/毎回転）</summary>
			private readonly short preG94;

			/// <summary>回転数</summary>
			public readonly double S;
			/// <summary>送り速度現在</summary>
			public readonly double? F;
			/// <summary>送り速度直前</summary>
			public readonly double? preF;

			/// <summary>移動する位置</summary>
			private readonly Ichi xyza;
			/// <summary>移動する距離と方向</summary>
			private readonly Pals ipls;

			/// <summary>移動ベクトル</summary>
			public Vector3 Dist { get { return ipls.ToXYZ(); } }

			/// <summary>
			/// 各メンバーの実数値を出力します
			/// </summary>
			/// <param name="nc">ＮＣのキャラクタコード</param>
			/// <returns>各実数値</returns>
			public double this[char nc] {
				get {
					switch (nc) {
					case 'X': return xyza.X;
					case 'Y': return xyza.Y;
					case 'Z': return xyza.Z;
					case 'A': return xyza.A;
					case 'B': return xyza.B;
					case 'C': return xyza.C;
					default: throw new InvalidOperationException("ergbtrhbh");
					}
				}
			}

			/// <summary>最終到達位置Ｘ</summary>
			public double X { get { return xyza.X; } }
			/// <summary>最終到達位置Ｙ</summary>
			public double Y { get { return xyza.Y; } }
			/// <summary>最終到達位置Ｚ</summary>
			public double Z { get { return xyza.Z; } }
			/// <summary>最終到達位置Ａ</summary>
			public double A { get { return xyza.A; } }
			/// <summary>最終到達位置Ｂ</summary>
			public double B { get { return xyza.B; } }
			/// <summary>最終到達位置Ｃ</summary>
			public double C { get { return xyza.C; } }

			/// <summary>整数化X</summary>
			public long Xi { get { return xyza.Xi; } }
			/// <summary>整数化Y</summary>
			public long Yi { get { return xyza.Yi; } }
			/// <summary>整数化Z</summary>
			public long Zi { get { return xyza.Zi; } }
			/// <summary>整数化A</summary>
			public long Ai { get { return xyza.Ai; } }
			/// <summary>整数化B</summary>
			public long Bi { get { return xyza.Bi; } }
			/// <summary>整数化C</summary>
			public long Ci { get { return xyza.Ci; } }
			/// <summary>整数化S</summary>
			public long Si { get { return (int)Math.Round(S); } }
			/// <summary>整数化F 送り未設定の場合は０とする 2015/09/24</summary>
			public long Fi {
				get {
					if (g94 != 94) throw new InvalidOperationException("毎回転送りモードでは整数化できません");
					return F.HasValue ? (int)Math.Round(F.Value) : 0;
				}
			}

			/// <summary>直前到達位置Ｘ</summary>
			public double PreX { get { return xyza.X - ipls.X; } }
			/// <summary>直前到達位置Ｙ</summary>
			public double PreY { get { return xyza.Y - ipls.Y; } }
			/// <summary>直前到達位置Ｚ</summary>
			public double PreZ { get { return xyza.Z - ipls.Z; } }
			/// <summary>直前到達位置Ａ</summary>
			public double PreA { get { return xyza.A - ipls.A; } }
			/// <summary>直前到達位置Ｂ</summary>
			public double PreB { get { return xyza.B - ipls.B; } }
			/// <summary>直前到達位置Ｃ</summary>
			public double PreC { get { return xyza.C - ipls.C; } }
			/// <summary>整数化F 送り未設定の場合は０とします</summary>
			public long PreFi {
				get {
					if (preG94 != 94) throw new InvalidOperationException("毎回転送りモードでは整数化できません");
					return preF.HasValue ? (int)Math.Round(preF.Value) : 0;
				}
			}

			/// <summary>
			/// 全メンバーを初期化して作成します
			/// </summary>
			/// <param name="apz">アプローチＺの値</param>
			internal Xisf(double apz) {
				g94 = preG94 = 94;
				S = 0.0;
				F = preF = null;
				xyza = new Ichi(new Vector3(0.0, 0.0, apz), Vector3.v0, NcLineCode.XyzDigit);
				ipls = new Pals(new Ichi(Vector3.v0, Vector3.v0, NcLineCode.XyzDigit));
			}
			/// <summary>
			/// 新たな位置、回転数、送り速度データより作成します
			/// </summary>
			private Xisf(Xisf moto, short g94, double? spin, double? feed, Ichi next) {
				this.preG94 = moto.g94;
				this.preF = moto.F;

				this.g94 = g94;
				this.S = spin ?? moto.S;
				this.F = feed ?? moto.F;

				this.ipls = new Pals(next - moto.xyza);
				this.xyza = next;
				if (next != moto.xyza + ipls.ToIchi(NcLineCode.XyzDigit)) throw new Exception("qwfedbqwfedbh");
			}
			/// <summary>
			/// 座標系移動を反映した xisf を作成します
			/// </summary>
			private Xisf(Xisf moto, Vector3 trans) {
				this = moto;
				this.xyza += trans;
			}
			/// <summary>
			/// ミラーを反映した xisf を作成します
			/// </summary>
			private Xisf(Xisf moto, NcZahyo mirr) {
				this = moto;
				this.xyza = this.xyza.Scaling(Vector3.v0, mirr.ToMirrXYZ);
				this.ipls = this.ipls.Scaling(mirr);
			}


			/// <summary>
			/// ＮＣデータコードのリストからＧ９モードを考慮し xisf を作成します
			/// </summary>
			/// <param name="ncForm">ＮＣデータのフォーマット</param>
			/// <param name="regular">ＰＴＰの正式フォーマットのＮＣデータの場合 true</param>
			/// <param name="numList">ＮＣデータコードのリスト</param>
			/// <param name="g5">Ｇ５グループ（送り速度モード）の値</param>
			/// <param name="g9">Ｇ９グループ（ＡＢＳ/ＩＮＣ）の値</param>
			/// <param name="cyclemode">サイクル値（0:一般、1:固定サイクル、2:カスタムマクロモード内[Ｇコードの設定なし]、3:G65セット、4:カスタムマクロモードのセット）</param>
			/// <param name="cycle">固定サイクルあるいはマクロ呼出しサイクルの情報</param>
			internal Xisf Next(BaseNcForm ncForm, bool regular, NumList numList, short g5, short g9, byte cyclemode, object cycle) {
				bool ido;
				double? nextS = null;
				double? nextF = null;
				Ichi nextP;
				CamUtil.CamNcD.MacroCode mCode;

				if (g5 != 94 && g5 != 95) throw new Exception("qk3efqrfn");
				switch (cyclemode) {
				case 0:	// 一般
					ido = true;
					foreach (NumCode numtmp in numList) {
						switch (numtmp.ncChar) {
						case 'F': nextF = numtmp.dblData; break;
						case 'S': nextS = numtmp.dblData; break;
						}
					}
					break;
				case 1:	// 固定サイクル
					mCode = new CamUtil.CamNcD.MacroCode(ncForm, ((CycleMode)cycle).progNo);
					ido = mCode.Ido;

					// 固定サイクルの場合、Ｘ，Ｙ，Ｚ，Ｒのいずれも存在しない場合動作しない
					//if (bb_26('X') != true && bb_26('Y') != true && bb_26('Z') != true && bb_26('R') != true) ido = false;
					if (numList.NcCount('X') + numList.NcCount('Y') + numList.NcCount('Z') + numList.NcCount('R') == 0) ido = false;
					if (((CycleMode)cycle)['F'].Set) nextF = ((CycleMode)cycle)['F'].D;
					break;
				case 2:	// G66 カスタムマクロモード内[Ｇコードの設定なし]
					mCode = new CamUtil.CamNcD.MacroCode(ncForm, ((MacroMode)cycle).ProgNo);
					ido = mCode.Ido;
					// カスタムマクロの場合、Ｘ，Ｙのいずれも存在しない場合動作しない
					//if (bb_26('X') != true && bb_26('Y') != true) ido = false;
					if (numList.NcCount('X') + numList.NcCount('Y') == 0) ido = false;
					for (char ii = 'A'; ii <= 'Z'; ii++)
						if (ii != 'X' && ii != 'Y' && numList.NcCount(ii) > 0) throw new Exception("モーダル行にＸＹ以外のコードが見つかりました");
					break;
				case 3:	// G65１ショットマクロ
					mCode = new CamUtil.CamNcD.MacroCode(ncForm, ((MacroMode)cycle).ProgNo);
					ido = mCode.Ido;
					switch (ncForm.Id) {
					case BaseNcForm.ID.GENERAL:
						if (ido) {
							// ///////////////////////////////////////
							// Ｇ６６に変換される前のＮＣデータの場合
							// ///////////////////////////////////////
							if (regular) throw new Exception("Ｇ６５コールエラー" + mCode.mess);
							if (((MacroMode)cycle)['F'].Set) nextF = ((MacroMode)cycle)['F'].D;
						}
						break;
					case BaseNcForm.ID.BUHIN:
						if (ido) {
							switch (((MacroMode)cycle).ProgNo) {
							case 8104:	// 部品加工用マクロ（ヘリカル/円錐補間）
								// 円弧補間で孔加工でないため、nextR, nextZをセットしない
								break;
							case 8105:	// 部品加工用マクロ（リジッドタップ）
								break;
							default:
								throw new Exception("Ｇ６５コールエラー" + mCode.mess);
							}
							if (((MacroMode)cycle)['F'].Set) nextF = ((MacroMode)cycle)['F'].D;
						}
						break;
					case BaseNcForm.ID.GRAPHITE:
						break;
					default:
						throw new Exception("efdwedfheu");
					}
					break;
				case 4:	// G66 カスタムマクロモードのセット
					mCode = new CamUtil.CamNcD.MacroCode(ncForm, ((MacroMode)cycle).ProgNo);
					ido = false;
					// カスタムマクロの設定行でＺ軸の移動を機能させるため以下を追加 at 2017/07/17
					if (((MacroMode)cycle)['F'].Set) nextF = ((MacroMode)cycle)['F'].D;
					break;
				default:
					throw new Exception("サイクルモードの値が異常 in NcLineCode.xisf.Set");
				}

				// ////////////////////////
				// 移動先位置 nextP の計算
				// ////////////////////////
				if (ido == false) {
					nextP = xyza;
				}
				else {
					//Ichi testP;
					NcZahyo xyz = new NcZahyo(g9,
						numList.NcCount('X') > 0 ? numList.Code('X').dblData : (double?)null,
						numList.NcCount('Y') > 0 ? numList.Code('Y').dblData : (double?)null,
						numList.NcCount('Z') > 0 ? numList.Code('Z').dblData : (double?)null);
					NcZahyo abc = new NcZahyo(g9,
						numList.NcCount('A') > 0 ? numList.Code('A').dblData : (double?)null,
						numList.NcCount('B') > 0 ? numList.Code('B').dblData : (double?)null,
						numList.NcCount('C') > 0 ? numList.Code('C').dblData : (double?)null);

					switch (cyclemode) {
					case 0:
						nextP = xyza.Update(xyz, abc);
						break;
					case 3:
						// G66サイクル終了後はイニシャル点に復帰するため、穴底のＺは代入しない（P8104を除く）
						if (((MacroMode)cycle).ProgNo != 8104) xyz = new NcZahyo(g9, xyz.X, xyz.Y, null);
						nextP = xyza.Update(xyz, NcZahyo.Null);
						break;
					default:
						// 固定サイクル終了後はイニシャル点に復帰するため、穴底のＺは代入しない
						xyz = new NcZahyo(g9, xyz.X, xyz.Y, null);
						nextP = xyza.Update(xyz, NcZahyo.Null);
						break;
					}
				}
				return new Xisf(this, g5, nextS, nextF, nextP);
			}

			/// <summary>最終到達位置ＸＹＺのストラクチャVector3 を出力します</summary>
			public Vector3 ToXYZ() { return xyza.ToXYZ(); }

			/// <summary>最終到達位置ＡＢＣのストラクチャVector3 を出力します</summary>
			public Vector3 ToABC() { return xyza.ToABC(); }

			/// <summary>直前到達位置のストラクチャVector3 を出力します</summary>
			public Vector3 PreToXYZ() { return (xyza - ipls.ToIchi(NcLineCode.XyzDigit)).ToXYZ(); }

			/// <summary>座標系ミラー処理後の座標値を出力します</summary>
			public Xisf Mirror(NcZahyo mirr) { return new Xisf(this, mirr); }

			/// <summary>座標系移動処理後の座標値を出力します</summary>
			public Xisf Transfer(Vector3 trans) { return new Xisf(this, trans); }

			/// <summary>このインスタンスと指定した xisf オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
			/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
			/// <returns>obj が xisf のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
			public override bool Equals(object obj) { return Equals((Xisf)obj); }
			private bool Equals(Xisf obj) {
				if (this.g94 != obj.g94) return false;
				if (this.preG94 != obj.preG94) return false;
				if (this.S != obj.S) return false;
				if (this.F != obj.F) return false;
				if (this.preF != obj.preF) return false;
				if (this.xyza != obj.xyza) return false;
				if (this.ipls != obj.ipls) return false;
				return true;
			}
			/// <summary>このインスタンスのハッシュ コードを返します。</summary>
			/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
			public override int GetHashCode() {
				return g94 ^ preG94 ^ S.GetHashCode() ^ F.GetHashCode() ^ preF.GetHashCode() ^ xyza.GetHashCode() ^ ipls.GetHashCode();
			}
		}

		/// <summary>
		/// 固定サイクルのモーダル値を表します
		/// </summary>
		public readonly struct CycleMode
		{
			// /////////////////////////////
			// 演算子のオーバーロード
			// /////////////////////////////
			/// <summary>固定サイクルの＝＝演算子</summary>
			public static bool operator ==(CycleMode c1, CycleMode c2) { return c1.Equals(c2); }
			/// <summary>固定サイクルの！＝演算子</summary>
			public static bool operator !=(CycleMode c1, CycleMode c2) { return !c1.Equals(c2); }




			/// <summary>固定サイクル番号（80, 81, 82, 83, 84, 86, 73）</summary>
			public readonly short progNo;
			/// <summary>固定サイクルデータ（Z, P, Q, R, I, J, F, L）</summary>
			private readonly ModeSub z, p, q, r, i, j, f, l;

			/// <summary>
			/// 固定サイクルの個々のコードを表します
			/// </summary>
			public readonly struct ModeSub
			{
				// /////////////////////////////
				// 演算子のオーバーロード
				// /////////////////////////////
				/// <summary>位置座標値の＝＝演算子。誤差がIchi.Gosa以内</summary>
				public static bool operator ==(CycleMode.ModeSub c1, CycleMode.ModeSub c2) { return c1.Equals(c2); }
				/// <summary>位置座標値の！＝演算子。誤差がIchi.Gosa以上</summary>
				public static bool operator !=(CycleMode.ModeSub c1, CycleMode.ModeSub c2) { return !c1.Equals(c2); }




				/// <summary>この情報が定義済の場合 true</summary>
				public bool Set { get { return m_d.HasValue; } }
				/// <summary>この行で設定された場合 true</summary>
				public bool B { get { if (m_d.HasValue) return m_b; throw new InvalidOperationException("固定サイクル呼出しの引数が不足している。"); } }
				private readonly bool m_b;
				/// <summary>コードの値。未定義の場合はエラーとなります</summary>
				public double D { get { if (m_d.HasValue) return m_d.Value; throw new InvalidOperationException("固定サイクル呼出しの引数が不足している。"); } }
				private readonly double? m_d;
				/// <summary>整数値で出力されるコード（P, F, L）の場合は true、そうでなければ false。</summary>
				private readonly bool pfl;

				/// <summary>コードの値を整数値で出力します。整数値で出力しないコード（Z, Q, R, I, J）の場合はエラーとなります。</summary>
				public long L {
					get {
						if (!pfl) throw new InvalidOperationException("P, F, L 以外のコードを整数化できません。");
						if (Math.Abs(m_d.Value - Math.Round(m_d.Value)) > 0.0001)
							throw new InvalidOperationException("小数点以下の有効な数値が存在します");
						return (long)Math.Round(m_d.Value);
					}
				}

				/// <summary>各プロパティ値により作成します</summary>
				/// <param name="data">設定の有無</param>
				/// <param name="value">コードの値</param>
				/// <param name="pfl">整数値</param>
				private ModeSub(bool data, double? value, bool pfl) {
					m_b = data;
					m_d = value;
					this.pfl = pfl;
				}
				/// <summary>
				/// 未定義のデータを作成します
				/// </summary>
				/// <param name="pfl"></param>
				public ModeSub(bool pfl) {
					m_b = false;
					m_d = null;
					this.pfl = pfl;
				}
				/// <summary>
				/// ＮＣコードの値を設定して新たな ModeSub を作成します
				/// </summary>
				/// <param name="value"></param>
				public ModeSub ModeSetL(double value) { return new ModeSub(true, value, this.pfl); }

				/// <summary>
				/// 次の行の情報とするため、この行で未設定として新たな ModeSub を作成します
				/// </summary>
				public ModeSub RLine() { return new ModeSub(false, this.m_d, this.pfl); }

				/// <summary>このインスタンスと指定した CycleMode.ModeSub オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
				/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
				/// <returns>obj が CycleMode.ModeSub のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
				public override bool Equals(object obj) { return Equals((CycleMode.ModeSub)obj); }
				private bool Equals(CycleMode.ModeSub obj) { return m_b == obj.m_b && m_d == obj.m_d && pfl == obj.pfl; }
				/// <summary>このインスタンスのハッシュ コードを返します。</summary>
				/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
				public override int GetHashCode() { return m_b.GetHashCode() ^ m_d.GetHashCode() ^ pfl.GetHashCode(); }
			}

			/// <summary>未定義のデータを作成します</summary>
			/// <param name="gCode">固定サイクルのＧコード</param>
			internal CycleMode(short gCode) {
				//m_g8p = new CycleModeSub[PQRIJ.Length];
				progNo = gCode;
				z = new ModeSub(false);
				p = new ModeSub(true);
				q = new ModeSub(false);
				r = new ModeSub(false);
				i = new ModeSub(false);
				j = new ModeSub(false);
				f = new ModeSub(true);
				l = new ModeSub(true);
			}
			/// <summary>前行の情報から、次行処理のためこの行で未設定なコードとして作成します</summary>
			/// <param name="src">前行の CycleMode</param>
			private CycleMode(CycleMode src) {
				progNo = src.progNo;
				z = src.z.RLine();
				p = src.p.RLine();
				q = src.q.RLine();
				r = src.r.RLine();
				i = src.i.RLine();
				j = src.j.RLine();
				f = src.f.RLine();
				l = src.l.RLine();
			}

			/// <summary>ＮＣデータコードのリストから元の CycleMode を元に作成します</summary>
			/// <param name="moto">元の CycleMode</param>
			/// <param name="numList">ＮＣデータコードのリスト</param>
			/// <param name="g9">Ｇコードグループ03 絶対相対モード(ABSG90,INCG91)</param>
			/// <param name="clp">穴加工のクリアランス高さ</param>
			private CycleMode(CycleMode moto, NumList numList, short g9, double clp) {
				this = moto;

				double rten = double.MaxValue, zten = double.MaxValue;
				if (g9 == 90) {
					foreach (NumCode num in numList) if (num.ncChar == 'R') { rten = num.dblData; break; }
					foreach (NumCode num in numList) if (num.ncChar == 'Z') { zten = num.dblData; break; }
				}
				else {
					foreach (NumCode num in numList) if (num.ncChar == 'R') { rten = clp + num.dblData; break; }
					foreach (NumCode num in numList) if (num.ncChar == 'Z') { zten = rten + num.dblData; break; }
					throw new Exception("固定サイクルのインクリメンタル指令は動作確認されていません。プログラム管理者までご連絡下さい");
				}

				foreach (NumCode num in numList) {
					switch (num.ncChar) {
					case 'Z': z = z.ModeSetL(zten); break;
					case 'R': r = r.ModeSetL(rten); break;
					case 'Q': q = q.ModeSetL(num.dblData); break;
					case 'I': i = i.ModeSetL(num.dblData); break;
					case 'J': j = j.ModeSetL(num.dblData); break;
					case 'P': p = p.ModeSetL(num.dblData); break;
					case 'F': f = f.ModeSetL(num.dblData); break;
					case 'L': l = l.ModeSetL(num.dblData); break;
					default: break;
					}
				}
			}

			/// <summary>
			/// このインスタンスを元に、ＮＣデータコードのリストから新たな CycleMode を作成します
			/// </summary>
			/// <param name="numList">ＮＣデータコードのリスト</param>
			/// <param name="g9">Ｇコードグループ03 絶対相対モード(ABSG90,INCG91)</param>
			/// <param name="clp">穴加工のクリアランス高さ</param>
			/// <returns>新たに作成された CycleMode</returns>
			public CycleMode CycleSetL(NumList numList, short g9, double clp) { return new CycleMode(this, numList, g9, clp); }

			/// <summary>
			/// このインスタンスを元に、次行処理のためこの行で未設定なコードとして新たな CycleMode を作成します
			/// </summary>
			/// <returns>新たに作成された CycleMode</returns>
			public CycleMode ResetLine() { return new CycleMode(this); }

			/// <summary>
			/// インデクサ
			/// </summary>
			/// <param name="nc">情報を取り出すＮＣコード</param>
			/// <returns>指定したＮＣコードを表す ModeSub</returns>
			public ModeSub this[char nc] {
				get {
					switch (nc) {
					case 'Z': return z;
					case 'P': return p;
					case 'Q': return q;
					case 'R': return r;
					case 'I': return i;
					case 'J': return j;
					case 'F': return f;
					case 'L': return l;
					default:
						throw new InvalidOperationException("fefwefnjw");
					}
				}
			}

			/// <summary>このインスタンスと指定した CycleMode オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
			/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
			/// <returns>obj が CycleMode のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
			public override bool Equals(object obj) { return Equals((CycleMode)obj); }
			private bool Equals(CycleMode obj) {
				if (this.progNo != obj.progNo) return false;
				if (this.z != obj.z) return false;
				if (this.p != obj.p) return false;
				if (this.q != obj.q) return false;
				if (this.r != obj.r) return false;
				if (this.i != obj.i) return false;
				if (this.j != obj.j) return false;
				if (this.f != obj.f) return false;
				if (this.l != obj.l) return false;
				return true;
			}
			/// <summary>このインスタンスのハッシュ コードを返します。</summary>
			/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
			public override int GetHashCode() {
				return progNo ^
					z.GetHashCode() ^ p.GetHashCode() ^ q.GetHashCode() ^ r.GetHashCode() ^
					i.GetHashCode() ^ j.GetHashCode() ^ f.GetHashCode() ^ l.GetHashCode();
			}
		}

		/// <summary>
		/// カスタムマクロのモーダル値を保存します
		/// </summary>
		public readonly struct MacroMode
		{
			// /////////////////////////////
			// 演算子のオーバーロード
			// /////////////////////////////

			/// <summary>カスタムマクロのモーダル値の＝＝演算子</summary>
			public static bool operator ==(MacroMode c1, MacroMode c2) { return c1.Equals(c2); }
			/// <summary>カスタムマクロのモーダル値の！＝演算子</summary>
			public static bool operator !=(MacroMode c1, MacroMode c2) { return !c1.Equals(c2); }




			/// <summary>プログラム名</summary>
			public string ProgName { get { return "P" + m_progNo.Value.ToString("0000"); } }
			/// <summary>プログラム番号</summary>
			public int ProgNo { get { return m_progNo.Value; } }
			private readonly int? m_progNo;
			/// <summary>繰り返し回数</summary>
			public int Repeat { get { return m_repeat; } }
			private readonly int m_repeat;

			private readonly ModeSub a, b, c, d, e, f, h, i, j, k, m, p, q, r, s, t, u, v, w, x, y, z;
			/// <summary>個々のカスタムマクロのＮＣコードを表します</summary>
			public readonly struct ModeSub
			{
				// /////////////////////////////
				// 演算子のオーバーロード
				// /////////////////////////////

				/// <summary>カスタムマクロのモーダル値の＝＝演算子</summary>
				public static bool operator ==(MacroMode.ModeSub c1, MacroMode.ModeSub c2) { return c1.Equals(c2); }
				/// <summary>カスタムマクロのモーダル値の！＝演算子</summary>
				public static bool operator !=(MacroMode.ModeSub c1, MacroMode.ModeSub c2) { return !c1.Equals(c2); }




				/// <summary>定義済みの場合は true</summary>
				public bool Set { get { return m_d.HasValue; } }
				/// <summary>ＮＣデータの値。未定義の場合はエラーとなります</summary>
				public double D { get { if (m_d.HasValue) return m_d.Value; throw new InvalidOperationException("カスタムマクロ呼出しの引数'" + cc.ToString() + "'が不足している。"); } }
				private readonly double? m_d;
				/// <summary>ＮＣコード</summary>
				private readonly char cc;

				/// <summary>未定義として作成します</summary>
				internal ModeSub(char code) { cc = code; m_d = null; }
				/// <summary>ＮＣコードの値で定義して作成します</summary>
				private ModeSub(char code, double p_d) { cc = code; m_d = p_d; }

				/// <summary>このインスタンスを元にＮＣコードの値を定義して作成します</summary>
				/// <param name="d">ＮＣコードの値</param>
				internal ModeSub ModeSet(double d) { return new ModeSub(this.cc, d); }

				/// <summary>このインスタンスと指定した MacroMode.ModeSub オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
				/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
				/// <returns>obj が MacroMode.ModeSub のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
				public override bool Equals(object obj) { return Equals((MacroMode.ModeSub)obj); }
				private bool Equals(MacroMode.ModeSub obj) { return this.m_d == obj.m_d && this.cc == obj.cc; }
				/// <summary>このインスタンスのハッシュ コードを返します。</summary>
				/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
				public override int GetHashCode() { return this.m_d.GetHashCode() ^ this.cc.GetHashCode(); }
			}

			/// <summary>
			/// すべてのコードが未定義として作成します
			/// </summary>
			/// <param name="dummy">ダミーです</param>
			internal MacroMode(int dummy) {
				m_progNo = null;
				m_repeat = 0;

				a = new ModeSub('A'); b = new ModeSub('B'); c = new ModeSub('C');
				d = new ModeSub('D'); e = new ModeSub('E'); f = new ModeSub('F'); h = new ModeSub('H');
				i = new ModeSub('I'); j = new ModeSub('J'); k = new ModeSub('K');
				m = new ModeSub('M'); p = new ModeSub('P'); q = new ModeSub('Q');
				r = new ModeSub('R'); s = new ModeSub('S'); t = new ModeSub('T');
				u = new ModeSub('U'); v = new ModeSub('V'); w = new ModeSub('W');
				x = new ModeSub('X'); y = new ModeSub('Y'); z = new ModeSub('Z');
			}

			/// <summary>
			/// ＮＣコードのリストから作成します
			/// </summary>
			/// <param name="moto">元の情報</param>
			/// <param name="numList">ＮＣコードのリスト</param>
			private MacroMode(MacroMode moto, NumList numList) {
				this = moto;
				foreach (NumCode num in numList) {
					switch (num.ncChar) {
					case 'A': a = a.ModeSet(num.dblData); break;
					case 'B': b = b.ModeSet(num.dblData); break;
					case 'C': c = c.ModeSet(num.dblData); break;
					case 'D': d = d.ModeSet(num.dblData); break;
					case 'E': e = e.ModeSet(num.dblData); break;
					case 'F': f = f.ModeSet(num.dblData); break;
					case 'G': break;
					case 'H': h = h.ModeSet(num.dblData); break;
					case 'I': i = i.ModeSet(num.dblData); break;
					case 'J': j = j.ModeSet(num.dblData); break;
					case 'K': k = k.ModeSet(num.dblData); break;
					case 'L': m_repeat = (int)num.dblData; break;
					case 'M': m = m.ModeSet(num.dblData); break;
					case 'N': break;
					case 'O': break;
					case 'P': if (m_progNo == null) m_progNo = (int)num.dblData; else p = p.ModeSet(num.dblData); break;
					case 'Q': q = q.ModeSet(num.dblData); break;
					case 'R': r = r.ModeSet(num.dblData); break;
					case 'S': s = s.ModeSet(num.dblData); break;
					case 'T': t = t.ModeSet(num.dblData); break;
					case 'U': u = u.ModeSet(num.dblData); break;
					case 'V': v = v.ModeSet(num.dblData); break;
					case 'W': w = w.ModeSet(num.dblData); break;
					case 'X': x = x.ModeSet(num.dblData); break;
					case 'Y': y = y.ModeSet(num.dblData); break;
					case 'Z': z = z.ModeSet(num.dblData); break;
					case ';': break;
					default:
						throw new Exception("fefwefnjw");
					}
				}
			}

			/// <summary>
			/// このインスタンスを元にＮＣコードのリストから新たに MacroMode を作成します
			/// </summary>
			/// <param name="numList">ＮＣコードのリスト</param>
			/// <returns>新たに作成した MacroMode</returns>
			public MacroMode MacroSet(NumList numList) { return new MacroMode(this, numList); }

			/// <summary>
			/// インデクサ
			/// </summary>
			/// <param name="nc">情報を取り出すＮＣコード</param>
			/// <returns>指定したＮＣコードを表す ModeSub</returns>
			public ModeSub this[char nc] {
				get {
					//return m_g6p[ABC.IndexOf(nc)];
					switch (nc) {
					case 'A': return a;
					case 'B': return b;
					case 'C': return c;
					case 'D': return d;
					case 'E': return e;
					case 'F': return f;
					case 'H': return h;
					case 'I': return i;
					case 'J': return j;
					case 'K': return k;
					case 'M': return m;
					case 'P': return p;
					case 'Q': return q;
					case 'R': return r;
					case 'S': return s;
					case 'T': return t;
					case 'U': return u;
					case 'V': return v;
					case 'W': return w;
					case 'X': return x;
					case 'Y': return y;
					case 'Z': return z;
					default:
						throw new InvalidOperationException("fefwefnjw");
					}
				}
			}

			/// <summary>このインスタンスと指定した MacroMode オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
			/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
			/// <returns>obj が MacroMode のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
			public override bool Equals(object obj) { return Equals((MacroMode)obj); }
			private bool Equals(MacroMode obj) {
				if (this.m_progNo != obj.m_progNo) return false;
				if (this.m_repeat != obj.m_repeat) return false;
				foreach (char cc in NcLineCode.MACP)
					if (this[cc] != obj[cc]) return false;
				return true;
			}
			/// <summary>このインスタンスのハッシュ コードを返します。</summary>
			/// <returns>32 ビット符号付き整数ハッシュ コード。</returns>
			public override int GetHashCode() {
				int mm = m_progNo.GetHashCode() ^ m_repeat;
				foreach (char cc in NcLineCode.MACP)
					mm ^= this[cc].GetHashCode();
				return mm;
			}
		}
	}
}
