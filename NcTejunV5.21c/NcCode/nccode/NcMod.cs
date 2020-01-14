using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CamUtil;

//#pragma warning disable 1591
namespace NcCode.nccode
{
	/// <summary>
	/// ＮＣデータのモーダル値を保持します。
	/// </summary>
	public class NcMod
	{
		/// <summary>各ＮＣコードの値を保存するリストを作成するために使用します</summary>
		private const string ncCodeList = "@" + StringCAM.ABC0;

		/// <summary>
		/// 使用している工具番号を保存する。tno[0]は常に0であり、tno[1]から追加する。
		/// nctim でのみ使用し、ＮＣデータ行とは無関係である。
		/// </summary>
		private static List<int> toolNoList;

		/// <summary>固定サイクル登録名の最初の文字列</summary>
		private static readonly string fixString = "OG";





		/// <summary>工具番号</summary>
		public int ToolNo { get; private set; }

		/// <summary>保存されている工具番号toolNoListへのポインタ</summary>
		public int Tptr { get; private set; }
		/// <summary>工具番号の現在の使用数</summary>
		public int ToolCount { get { return NcMod.toolNoList.Count - 1; } }

		/// <summary>各ＮＣコードの値（FはＦ一桁送りを考慮した値）</summary>
		internal RO_ListChar<CodeD> CodeValue { get { return m_CodeValue.AsReadOnly; } } private RO_ListChar<CodeD>.InnerArray m_CodeValue;
		//private CodeD[] agList;	// m_ag と m_ag2 を同一にするためのみに使用している。消去する。

		/// <summary>各Ｇコードグループの値</summary>
		public RO_Collection<CamUtil.LCode.Gcode> GGroupValue { get { return m_GGroupValue.AsReadOnly; } } private RO_Collection<CamUtil.LCode.Gcode>.InnerArray m_GGroupValue;

		/// <summary>現在行の補間モード（G00,G01,G02,G03）</summary>
		public int Hokan { get; private set; }
		/// <summary>座標系番号（0:G54）</summary>
		public int CoordinatesNo { get; private set; }
		/// <summary>パラメータの設定モード（ＯＮ：５０）</summary>
		public int G10 { get; private set; }
		/// <summary>マクロ文、マクロ定義文とM98,M99（false:出力対象のＮＣ文）</summary>
		public bool Mcro { get; private set; }
		/// <summary>メインＮＣデータの行番号</summary>
		public int NcLineNo { get; private set; }


		/// <summary>サブをオープンするための情報。NcRW.nextPro.readInfoに情報を渡すため、ここでは変更点のみを出力する ADD in 2007/10/04</summary>
		internal Fmsub subInfo;
		/// <summary>サブプログラムの呼び出し方法（0:メイン(無し) 1:固定サイクル 2:マクロ 3:通常サブ）</summary>
		public short Subk { get { return subInfo.subk; } }

		/// <summary>停止時間（DWELL,M06）</summary>
		public double Lost { get; private set; }

		/// <summary>プログラマブルミラーのＯＮＯＦＦ（==null:ミラーＯＦＦ else:ミラーＯＮ）</summary>
		public NcZahyo Pmirror { get; private set; }

		/// <summary>ＸＹＺのスケーリング値</summary>
		public Vector3 ScaleValue { get; private set; }
		/// <summary>スケーリングの中心座標値</summary>
		public Ichi ScaleCenter { get { return m_scalz; } } private Ichi m_scalz;

		/// <summary>座標回転平面</summary>
		public int RotationPlane { get; private set; }
		/// <summary>座標回転角度（deg）</summary>
		public double RotationAngle { get; private set; }
		/// <summary>座標回転中心座標値</summary>
		public Ichi RotationCenter { get { return m_rotz; } } private Ichi m_rotz;

		/// <summary>G92 による座標オフセットＸＹＺ</summary>
		public Ichi OffsetG92 { get; private set; }

		/// <summary>
		/// 繰り返し数（１ショットであり、モーダル値であるsubInfo.iloopとは異なる）
		/// ido.subkを削除可能にするために新たに設ける 2017/09/29
		/// </summary>
		public int? LoopNo { get; private set; }



		/// <summary>
		/// 初期値を代入するコンストラクタ
		/// </summary>
		internal NcMod() {
			NcMod.toolNoList = new List<int>(51) { 0 };

			Hokan = 0;
			Mcro = false;	/* mcro!=0 -- macro statment */

			//agList = new CodeD[27];
			//m_ag = new RO_Collection<CodeD>.InnerArray(agList);
			m_CodeValue = new RO_ListChar<CodeD>.InnerArray(NcMod.ncCodeList);

			m_GGroupValue = new RO_Collection<CamUtil.LCode.Gcode>.InnerArray(CamUtil.LCode.Gcode.GGRPNUM);
			for (int ii = 0; ii < m_GGroupValue.Length; ii++)
				m_GGroupValue[ii] = new CamUtil.LCode.Gcode(CamUtil.LCode.Gcode.ggroup[ii]);

			CoordinatesNo = 54 - 54;
			NcLineNo = 0;
			G10 = -1;
			ToolNo = Tptr = 0;

			subInfo = new Fmsub(0, null, null);

			// 以下はNcMod_gでのみ使用する
			Lost = 0.0;
			m_scalz = new Ichi(Vector3.v0, Post.PostData['X'].sdgt);
			m_rotz = new Ichi(Vector3.v0, Post.PostData['X'].sdgt);

			Pmirror = NcZahyo.Null;
			ScaleValue = new Vector3(1.0, 1.0, 1.0);
			RotationPlane = 0;
			RotationAngle = 0.0;
			OffsetG92 = new Ichi(Vector3.v0, Vector3.v0, Post.PostData['X'].sdgt);
			LoopNo = null;
		}
		/// <summary>
		/// ディープコピーによりクローンを作成します。
		/// ディープコピーコンストラクタより約３０％コストが低いためこちらを使用します。
		/// </summary>
		internal NcMod Clone() {
			//ナローコピーの実行
			NcMod clone = (NcMod)this.MemberwiseClone();

			// 以下に完全値型ではないオブジェクトのコピーを実行
			clone.subInfo = new Fmsub(this.subInfo);
			clone.m_GGroupValue = new RO_Collection<CamUtil.LCode.Gcode>.InnerArray(26);
			for (int ii = 0; ii < clone.m_GGroupValue.Length; ii++) { clone.m_GGroupValue[ii] = this.GGroupValue[ii]; }
			clone.m_CodeValue = new RO_ListChar<CodeD>.InnerArray(NcMod.ncCodeList);
			foreach (char cc in NcMod.ncCodeList.ToCharArray()) { if (this.m_CodeValue[cc] != null) { clone.m_CodeValue[cc] = this.m_CodeValue[cc].Clone(); } }

			return clone;
		}

		/// <summary>
		/// Ｇグループ初期値をパラメータ2401により設定する
		/// </summary>
		/// <param name="paras2401">params(2401,0)</param>
		internal void Params2401(int paras2401) {
			if ((paras2401 & 0x01) == 0) /* hokan houhou     */
				m_GGroupValue[1] = new CamUtil.LCode.Gcode(0);
			else
				m_GGroupValue[1] = new CamUtil.LCode.Gcode(1);
			if ((paras2401 & 0x02) == 0) /* abs-inc mode     */
				m_GGroupValue[3] = new CamUtil.LCode.Gcode(91);
			else
				m_GGroupValue[3] = new CamUtil.LCode.Gcode(90);
			if ((paras2401 & 0x04) == 0 && (paras2401 & 0x08) == 0)
				// kougu-chou hosei
				m_GGroupValue[8] = new CamUtil.LCode.Gcode(49);
			if ((paras2401 & 0x10) == 0) /* Fcode mode       */
				m_GGroupValue[5] = new CamUtil.LCode.Gcode(94);
			else
				m_GGroupValue[5] = new CamUtil.LCode.Gcode(95);
			if ((paras2401 & 0x20) == 0) /* heimen  sitei    */
				m_GGroupValue[2] = new CamUtil.LCode.Gcode(17);
			else
				m_GGroupValue[2] = new CamUtil.LCode.Gcode(18);
		}


		/// <summary>
		/// 現在読込みするＮＣデータのモード（固定サイクル、カスタムマクロ、サブプログラム）の状況を保持します。
		/// 処理中の行の情報は ncmod にて保存されます。
		/// </summary>
		internal readonly struct Fmsub
		{
			/// <summary>サブ呼出し方法でキャンセルを含む設定行でのみセットされる（0:メイン(無し) 1:固定サイクル 2:マクロ 3:通常サブ）</summary>
			public readonly short subk;
			/// <summary>繰り返し回数</summary>
			public readonly int? iloop;
			/// <summary>ＮＣのファイル名</summary>
			public readonly string ncnam;

			/// <summary>固定サイクル、カクタムマクロを呼び出す時のローカル変数初期値のリスト</summary>
			internal readonly List<FsubT> fsubt;

			/// <summary></summary>
			internal Fmsub(short p_subk, int? p_iloop, string p_ncnam) {
				this.subk = p_subk;
				this.iloop = p_iloop;
				this.ncnam = p_ncnam;
				this.fsubt = new List<NcMod.Fmsub.FsubT>();
			}
			/// <summary></summary>
			internal Fmsub(short p_subk, int? p_iloop, string p_ncnam, List<NcMod.Fmsub.FsubT> p_fsubt) {
				this.subk = p_subk;
				this.iloop = p_iloop;
				this.ncnam = p_ncnam;
				this.fsubt = new List<NcMod.Fmsub.FsubT>();
				if (p_fsubt != null)
					foreach (NcMod.Fmsub.FsubT ft in p_fsubt)
						this.fsubt.Add(ft);
			}
			/// <summary>
			/// ディープコピーするコンストラクタ
			/// </summary>
			/// <param name="src"></param>
			internal Fmsub(Fmsub src) {
				this.subk = src.subk;
				this.iloop = src.iloop;
				this.ncnam = src.ncnam;
				this.fsubt = new List<FsubT>();
				foreach (FsubT ft in src.fsubt) { this.fsubt.Add(ft); }
			}

			/// <summary>新たな引数で変数代入リストを更新する（まだ未使用）</summary>
			internal Fmsub SetFsub(List<FsubT> p_fsubt) {
				bool set;
				List<FsubT> ftmp = new List<FsubT>();

				switch (this.subk) {
				case 1: // 固定サイクルの場合は以前の引数に追加する
					foreach (FsubT ft1 in this.fsubt) {
						set = false;
						foreach (FsubT ft2 in p_fsubt) {
							if (ft1.varia == ft2.varia) {
								ftmp.Add(ft2);
								p_fsubt.Remove(ft2);
								set = true;
								break;
							}
						}
						if (set) continue;
						ftmp.Add(ft1);
					}
					foreach (FsubT ft2 in p_fsubt) ftmp.Add(ft2);
					return new Fmsub(this.subk, this.iloop, this.ncnam, ftmp);
				case 2: // マクロ(G66.1)の場合は引数を差し換える
					return new Fmsub(this.subk, this.iloop, this.ncnam, p_fsubt);
				default:
					throw new Exception("qerfbqrb");
				}
			}

			/// <summary>このインスタンスと指定した Fmsub オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
			/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
			/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
			public bool Equals(Fmsub obj) {
				if (this.subk != obj.subk) return false;
				if (this.iloop != obj.iloop) return false;
				if (this.ncnam != obj.ncnam) return false;
				if (this.fsubt.Count != obj.fsubt.Count) return false;
				for (int ii = 0; ii < this.fsubt.Count; ii++)
					if (!this.fsubt[ii].Equals(obj.fsubt[ii])) return false;
				return true;
			}

			/// <summary>固定サイクル、カクタムマクロを呼び出す時のローカル変数初期値</summary>
			internal readonly struct FsubT
			{
				/// <summary>代入する変数の番号</summary>
				public readonly int varia;
				/// <summary>代入する値</summary>
				public readonly double value;
				/// <summary>
				/// コンストラクタ
				/// </summary>
				/// <param name="p_varia">変数の番号</param>
				/// <param name="p_value">値</param>
				public FsubT(int p_varia, double p_value) {
					this.varia = p_varia;
					this.value = p_value;
				}
			}
		}

		/// <summary>
		/// 現在行の処理事項を決定するための情報を保存します。
		/// </summary>
		internal readonly struct Ido
		{
			/// <summary>移動状態 0:移動 1:ＸＹのみ 2:移動なし 3:無意データ</summary>
			public readonly int pass;
			/// <summary>データ内容（0:一般データ 1: 2:）</summary>
			public readonly int norm;
			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="dummy">無意味</param>
			public Ido(int dummy) {
				this.pass = 3;
				this.norm = 0;
			}
			/// <summary>
			/// コンストラクタ
			/// </summary>
			/// <param name="pass">移動状態</param>
			/// <param name="norm">データ内容</param>
			public Ido(int pass, int norm) {
				this.pass = pass;
				this.norm = norm;
			}
		}

		/// <summary>
		/// ncmod の設定
		/// </summary>
		/// <param name="tcode">ncModの元となるＮＣデータ行の情報</param>
		/// <param name="ido">実行後に実施すべき動作を決定する情報</param>
		/// <param name="p0006">M98P0006(ＮＣデータ終了処理)をコードを読まずに処理(ncgen)</param>
		/// <param name="sdep"></param>
		/// <param name="p_fsub">使用している変数リスト</param>
		/// <returns>エラーの有無</returns>
		internal int NcCod2(OCode tcode, ref Ido ido, bool p0006, Ncdep sdep, NcMachine.Variable p_fsub) {
			int ii, ptrn;
			double rtmp;

			ido = new Ido(3, 0);
			LoopNo = null;

			Mcro = false;
			Lost = 0.0;
			if (tcode.g10 >= -1)
				G10 = tcode.g10;

			if (sdep.Depth == 0) {
				NcLineNo = tcode.ncln;
			}

			// Ｇコードグループの設定
			m_GGroupValue[0] = new CamUtil.LCode.Gcode(0);
			for (ii = 0; ii < tcode.Gg.Count; ii++)
				if (tcode.Gg[ii].Gst)
					m_GGroupValue[ii] = tcode.Gg[ii];

			Hokan = GGroupValue[1].ToInt();

			if (!GGroupValue[14].Equals(54.1))
				CoordinatesNo = GGroupValue[14].ToInt() - 54;
			if (tcode.Gg[11].Gst && GGroupValue[11].Equals(50))
				ScaleValue = new Vector3(1.0, 1.0, 1.0);
			if (tcode.Gg[16].Gst && GGroupValue[16].Equals(69)) {
				RotationPlane = 0;
				RotationAngle = 0.0;
			}

			// subInfoの初期化 add in 2008/07/23
			subInfo = new Fmsub(0, null, null);

			// PARAMETER INPUT
			if (G10 == 0) {
				if (tcode.codeData.CodeCount('L') == 0 || tcode.codeData.CodeData('L').ToInt != 50) {
					_main.Error.Ncerr(1, "G10L" + tcode.codeData.CodeCount('L') + " ignored.");
					G10 = -1;
					return -1;
				}
				G10 = 50;
				ido = new NcMod.Ido(ido.pass, 2);
				return 0;
			}
			else if (G10 == 50) {
				//System.Windows.Forms.MessageBox.Show("G10パラメータの設定＝" + tcode.cod[14].ToString());
				if (tcode.codeData.CodeCount('N') != 0 && tcode.codeData.CodeCount('R') != 0) {
					NcMachine.G10ParaSet(tcode);
				}
				ido = new NcMod.Ido(ido.pass, 2);
				return 0;
			}


			if (tcode.codeData.CodeCount('N') != 0)
				m_CodeValue['N'] = tcode.codeData.CodeData('N').Clone(); /* Ncode */
			if (tcode.codeData.CodeCount('O') != 0)
				m_CodeValue['O'] = tcode.codeData.CodeData('O').Clone(); /* Ocode */

			ptrn = tcode.codeData.Count(cd => "ABCDEFGHIJKLM..PQRSTUVWXYZ".IndexOf(cd.ncChar) >= 0);    // YUUI DATA 有無
			if (ptrn == 0 && tcode.codeData.CodeCount('#') != 0) {
				Mcro = true;
				ido = new NcMod.Ido(ido.pass, 2);
				return 0;
			}

			ptrn = tcode.codeData.Count(cd => "ABCDEFGHIJKLM..PQRSTUVWXYZ".IndexOf(cd.ncChar) >= 0);    // YUUI DATA 有無
			if (tcode.codeData.CodeCount('N') > 1) ptrn++;    // ２つめの'N'は有意データである

			// //////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// マクロの設定行に 繰り返し数L, プログラム番号ではないP のコードがあるか調べる（今までに出現はない） 2019/06/25
			// //////////////////////////////////////////////////////////////////////////////////////////////////////////////
			if (tcode.Gst12 || GGroupValue[0].Equals(65)) {
				if (tcode.codeData.CodeCount('L') != 0 || (tcode.Subc != 0 && tcode.codeData.CodeCount('P') != 0)) {
					LogOut.CheckCount("NcMod 395", true, "マクロ設定行に 繰り返し数L, プログラム番号ではないP のコードが使用された。動作チェックを実施のこと nc=" + tcode.nctx);
				}
			}
			// カスタムマクロ（１ショット）処理
			if ((sdep.gg12.Equals(66.1) && ptrn > 0) || GGroupValue[0].Equals(65)) {
				Mcro = true;
				ido = new NcMod.Ido(ido.pass, 1);
				if (tcode.Gst12 || GGroupValue[0].Equals(65)) {
					if (tcode.codeData.CodeCount('L') != 0) m_CodeValue['L'] = tcode.codeData.CodeData('L').Clone();
					if (tcode.Subc != 0)
						if (tcode.codeData.CodeCount('P') != 0) m_CodeValue['P'] = tcode.codeData.CodeData('P').Clone();
				}
				SetMac(tcode, sdep);
				return 0;
			}

			// カスタムマクロ（モーダル設定行）処理
			if (tcode.Gst12) {
				if (sdep.gg12.Equals(66)) {
					Mcro = true;
					ido = new NcMod.Ido(ido.pass, 1);
					if (tcode.Gst12 || GGroupValue[0].Equals(65)) {
						if (tcode.codeData.CodeCount('L') != 0) m_CodeValue['L'] = tcode.codeData.CodeData('L').Clone();
						if (tcode.Subc != 0)
							if (tcode.codeData.CodeCount('P') != 0) m_CodeValue['P'] = tcode.codeData.CodeData('P').Clone();
					}
					SetMac(tcode, sdep);
					LoopNo = 0;
					return 0;
				}
				if (sdep.gg12.Equals(67)) {
					Mcro = true;
					ido = new NcMod.Ido(ido.pass, 1);
					LoopNo = 0;
					return 0;
				}
			}

			// G92の設定
			if (GGroupValue[0].Equals(92)) {
				LogOut.CheckCount("NcMod 539", false, "G92 が設定されました");
				OffsetG92 = p_fsub.WorkIchi.Update(tcode.codeData.XYZ(90), tcode.codeData.ABC(90));

				//g92 -= new Ichi(p_fsub.WorkXYZ, p_fsub.WorkABC, Post.sdgt[24]);
				OffsetG92 -= p_fsub.WorkIchi;
			}

			// PROGRAMABLE MIRROR
			if (tcode.Gg[18].Gst) {
				if (GGroupValue[16].Equals(68) || GGroupValue[11].Equals(51))
					_main.Error.Ncerr(3, "PROGRAMABLE MIRROR ERROR");
				ido = new Ido(2, 1);
				if (GGroupValue[18].Equals(51.1)) {
					Pmirror = tcode.codeData.XYZ(null);
					LogOut.CheckCount("NcMod 552", false, "G51.1 プログラマブルミラーが設定されました");
				}
				else if (GGroupValue[18].Equals(50.1)) {
					Pmirror = NcZahyo.Null;
				}
				return 0;
			}

			// SCALING
			if (tcode.Gg[11].Gst && GGroupValue[11].Equals(51)) {
				ido = new Ido(2, 1);
				if ((NcMachine.ParaData(6400, 0) & 0x02) == 0)
					rtmp = 0.00001;
				else
					rtmp = 0.001;
				if (tcode.codeData.CodeCount('P') != 0) {
					ScaleValue = new Vector3(
						((NcMachine.ParaData(12, 0) & 0x02) == 2) ? tcode.codeData.CodeData('P').ToInt * rtmp : 1.0,
						((NcMachine.ParaData(12, 1) & 0x02) == 2) ? tcode.codeData.CodeData('P').ToInt * rtmp : 1.0,
						((NcMachine.ParaData(12, 2) & 0x02) == 2) ? tcode.codeData.CodeData('P').ToInt * rtmp : 1.0);
				}
				else if ((NcMachine.ParaData(7611, 0) & 0x10) == 0) {
					ScaleValue = new Vector3(
						((NcMachine.ParaData(12, 0) & 0x02) == 2) ? NcMachine.ParaData(6410, 0) * rtmp : 1.0,
						((NcMachine.ParaData(12, 1) & 0x02) == 2) ? NcMachine.ParaData(6410, 0) * rtmp : 1.0,
						((NcMachine.ParaData(12, 2) & 0x02) == 2) ? NcMachine.ParaData(6410, 0) * rtmp : 1.0);
				}
				else {
					ScaleValue = new Vector3(
						NcMachine.ParaData(6421, 0) * rtmp,
						NcMachine.ParaData(6421, 1) * rtmp,
						NcMachine.ParaData(6421, 2) * rtmp);
				}

				LogOut.CheckCount("NcMod 611", false, "G51 スケーリングが設定されました");
				m_scalz = p_fsub.WorkIchi.Update(tcode.codeData.XYZ(90), NcZahyo.Null);

				m_scalz = ScaleCenter.Scaling(Pmirror.ToVector(), Pmirror.ToMirrXYZ);
				return 0;
			}

			// ZAHYOU KAITEN
			if (tcode.Gg[16].Gst && GGroupValue[16].Equals(68)) {
				ido = new Ido(2, 1);
				if ((NcMachine.ParaData(6400, 0) & 0x04) == 0)
					rtmp = 0.00001;
				else
					rtmp = 0.001;
				RotationPlane = GGroupValue[2].ToInt();
				if (tcode.codeData.CodeCount('R') != 0) {
					if ((NcMachine.ParaData(6400, 0) & 0x01) == 0 || GGroupValue[3].Equals(90))
						RotationAngle = tcode.codeData.CodeData('R').ToInt * rtmp;
					else
						RotationAngle += tcode.codeData.CodeData('R').ToInt * rtmp;
				}
				else
					RotationAngle = NcMachine.ParaData(6411, 0) * rtmp;

				LogOut.CheckCount("NcMod 657", false, "G68 座標回転が設定されました");
				m_rotz = p_fsub.WorkIchi.Update(tcode.codeData.XYZ(90), NcZahyo.Null);

				m_rotz = RotationCenter.Scaling(Pmirror.ToVector(), Pmirror.ToMirrXYZ);

				switch (RotationPlane) {
				case 17: RotationAngle *= Pmirror.ToMirrABC.Z; break;
				case 18: RotationAngle *= Pmirror.ToMirrABC.Y; break;
				case 19: RotationAngle *= Pmirror.ToMirrABC.X; break;
				}
				return 0;
			}

			// DWELL
			if (GGroupValue[0].Equals(4)) {
				ido = new Ido(2, 1);
				if (tcode.codeData.CodeCount('P') != 0)			/* P */
					Lost += tcode.codeData.CodeData('P').ToDouble / 1000.0;	// "/1000.0" add in 2008/07/24
				else if (tcode.codeData.CodeCount('X') != 0) {	/* X */
					Lost += tcode.codeData.CodeData('/').ToDouble;
				}
				return 0;
			}

			// costumn macro2 2line ikou
			if (sdep.gg12.Equals(66)) {
				if (tcode.codeData.CodeCount('X') + tcode.codeData.CodeCount('Y') + tcode.codeData.CodeCount('Z') > 0) {
					ido = new Ido(0, 1);
					if (LoopNo.HasValue) throw new Exception("efbqwefh");
					return 0;
				}
				else {
					LogOut.CheckCount("NcMod 588", false, "移動を伴わないカスタムマクロ呼出しが見つかりました。" + tcode.nctx);
					LoopNo = 0;
					// 空のＮＣデータはここで処理するように変更 2011/08/09
					if (tcode.codeData.Count == 1) {
						ido = new Ido(2, 1);
						return 0;
					}
				}
			}

			// fixed sycle
			if (sdep.gg09.Equals(80)) {
				if (tcode.Gst09)
					LoopNo = 0;	// G80;
			}
			else {
				// 注意！！孔加工データQPIJKは、孔あけ動作が行われるXYZRのある行で指令された場合のみ、モーダルなデータとして記憶される
				if (tcode.codeData.CodeCount('X') + tcode.codeData.CodeCount('Y') + tcode.codeData.CodeCount('Z') + tcode.codeData.CodeCount('R') > 0) {
					ido = new Ido(1, 1);	// なぜ、passが1なのか不明。調査要 2017/10/02

					if (tcode.codeData.CodeCount('F') != 0) {
						if (tcode.nrmFeed == true) m_CodeValue['F'] = tcode.codeData.CodeData('F').Clone();
						else {
							System.Windows.Forms.MessageBox.Show("Ｆ一桁送りの設定です");
							m_CodeValue['F'] = new CodeD('F', false, (double)NcMachine.ParaData((int)tcode.codeData.CodeData('F').ToDouble + 1450, 0), Post.PostData['F'].sdgt);
						}
					}
					SetFix(tcode, sdep, p_fsub);
					LoopNo = (tcode.codeData.CodeCount('L') != 0) ? (int)tcode.codeData.CodeData('L').ToDouble : 1;

					if ((NcMachine.ParaData(6200, 0) & 0xF0) == 0)
						Hokan = 0;
					else {
						System.Windows.Forms.MessageBox.Show("標準の設定ではない");
					}
					return 0;
				}
				else {
					LogOut.CheckCount("NcMod 824", false, "移動を伴わない固定サイクル呼出しが見つかりました。" + tcode.nctx);
					LoopNo = 0;
					// 空のＮＣデータはここで処理するように変更 2011/08/09
					// 以下はたぶん固定サイクル内の空白行でエラーとなったための暫定処置と思われるが、今はなぜ必要か不明。調査要。21017/10/02
					if (tcode.codeData.Count == 1) {
						ido = new Ido(2, 1);
						return 0;
					}
				}
			}

			// sonota code shori
			SetNrm(tcode, sdep);
			if (tcode.Mst[0] != 0) {
				ptrn = tcode.codeData.Count(cd => "ABCDEFGHIJK.....QRSTUVWXYZ".IndexOf(cd.ncChar) >= 0);    // YUUI DATA 有無
				if (tcode.Mst[1] == 2) {
					if (sdep.Depth != 0) _main.Error.Ncerr(3, "M02 exists in SUB");
				}
				else if (tcode.Mst[1] == 30) {
					if (sdep.Depth != 0) _main.Error.Ncerr(3, "M30 exists in SUB");
				}
				else if (tcode.Mst[1] == 98) {
					ido = new NcMod.Ido(ido.pass, ido.norm);
					if (ptrn == 0) Mcro = true;
					// ncgenの場合はM98P0006の処理をコードを読まずにここで実行する
					if (p0006) P0006(tcode);
					LoopNo = (tcode.codeData.CodeCount('P') != 0 && tcode.codeData.CodeCount('L') != 0) ? (int)tcode.codeData.CodeData('L').ToDouble : 1;
				}
				else if (tcode.Mst[1] == 99) {
					if (sdep.Depth == 0) _main.Error.Ncerr(3, "M99 exists in MAIN");
					if (ptrn == 0) Mcro = true;
				}
			}
			ptrn = tcode.codeData.Count(cd => "ABCDEF.HIJK.....QR.TUVWXYZ".IndexOf(cd.ncChar) >= 0);    // YUUI DATA 有無
			if (ptrn > 0 || GGroupValue[0].Equals(39))
				ido = new Ido(0, ido.norm);
			else if (tcode.codeData.CodeCount('G') != 0)
				ido = new Ido(2, ido.norm);
			return 0;
		}
		/// <summary>
		/// マクロのモード処理
		/// </summary>
		/// <param name="lcode"></param>
		/// <param name="sdep"></param>
		private void SetMac(OCode lcode, Ncdep sdep)
		{
			// 新規モーダルと１ショットの処理
			if (lcode.Gst12 || GGroupValue[0].Equals(65)) {
				int ilp = lcode.codeData.CodeCount('L') != 0 ? lcode.codeData.CodeData('L').ToInt : 1;
				string subn = lcode.Subc == 0 ? ("O" + lcode.codeData.CodeData('P').ToDouble.ToString("0000")) : ("O" + lcode.Subc.ToString("0000"));
				subInfo = new Fmsub(2, ilp, subn, null);
			}

			int ijk = 3;
			int nn = 0;
			foreach (CodeD ftmp in lcode.codeData) {
				switch (ftmp.ncChar) {
				case 'G':
				case 'L':
				case 'N':
				case 'O':
				case 'P':
					break;
				case 'I':
				case 'J':
				case 'K':
					for (int ii = 1; ; ii++)
						if (ijk < (ftmp.ncChar - 'H') + 3 * ii) { ijk = (ftmp.ncChar - 'H') + 3 * ii; break; }
					subInfo.fsubt.Add(new Fmsub.FsubT(ijk, ftmp.ToDouble)); break;
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
				case 'H':
				case 'M':
				case 'Q':
				case 'R':
				case 'S':
				case 'T':
				case 'U':
				case 'V':
				case 'W':
				case 'X':
				case 'Y':
				case 'Z':
					subInfo.fsubt.Add(new Fmsub.FsubT(ftmp.MacroNo, ftmp.ToDouble)); break;
				}
				// G66.1 の２回目以降の処理
				if (lcode.Gst12 == false && GGroupValue[12].Equals(66.1)) {
					switch (ftmp.ncChar) {
					case 'G':
					case 'L':
					case 'P':
						subInfo.fsubt.Add(new Fmsub.FsubT(ftmp.MacroNo, ftmp.ToDouble)); break;
					case 'N':
						nn++; if (nn == 1) break;
						subInfo.fsubt.Add(new Fmsub.FsubT(ftmp.MacroNo, ftmp.ToDouble)); break;
					}
				}
			}
			return;
		}

		/// <summary>
		/// 固定サイクルの処理
		/// </summary>
		/// <param name="lcode"></param>
		/// <param name="sdep"></param>
		/// <param name="p_fsub"></param>
		private void SetFix(OCode lcode, Ncdep sdep, NcMachine.Variable p_fsub)
		{
			// モーダル開始の処理
			if (lcode.Gg[9].Gst) {
				string subn;
				subn = fixString + sdep.gg09.ToString("000");

				subInfo = new Fmsub(1, null, subn, null);

				// //////////////////////////////////////////////////////////////////////
				// 以下の３つは固定サイクルの動作を決定するGENERAL内のマクロ文で使用する
				// //////////////////////////////////////////////////////////////////////
				// 第３軸ブロック終点位置
				subInfo.fsubt.Add(new Fmsub.FsubT(33, (double)p_fsub[5003]));
				// 固定サイクルG73 のもどり量
				subInfo.fsubt.Add(new Fmsub.FsubT(30, (double)NcMachine.ParaData(6210, 0) / Post.PostData['R'].sdgt / ScaleValue.Z));
				// 固定サイクルG83 のクリアランス
				subInfo.fsubt.Add(new Fmsub.FsubT(31, (double)NcMachine.ParaData(6211, 0) / Post.PostData['R'].sdgt / ScaleValue.Z));
			}

			foreach(CodeD codeD in lcode.codeData){
				if (Char.IsUpper(codeD.ncChar)) {
					if (lcode.codeData.CodeCount(codeD.ncChar) > 1) {
						if (codeD.ncChar != 'G') LogOut.CheckCount("", false, "１行に複数のＮＣコードが設定された" + lcode.codeData.NcText);
						if (codeD != lcode.codeData.CodeData(codeD.ncChar)) continue;
					}
					switch (codeD.ncChar) {
					case 'G':
					case 'N':
					case 'O':
					case 'X':
					case 'Y':
						break;
					case 'M':
					case 'R':
					case 'Z':
					case 'K':
					case 'P':
						subInfo.fsubt.Add(new Fmsub.FsubT(codeD.MacroNo, codeD.ToDouble));
						break;
					case 'F':
						subInfo.fsubt.Add(new Fmsub.FsubT(codeD.MacroNo, CodeValue['F'].ToDouble));
						break;
					case 'I':
					case 'J':
					case 'Q':
						subInfo.fsubt.Add(new Fmsub.FsubT(codeD.MacroNo, codeD.ToDouble / ScaleValue.Z));
						break;
					}
				}
			}
			return;
		}
		/// <summary>
		/// 非サイクルモード時の各コードのモード設定
		/// </summary>
		/// <param name="lcode"></param>
		/// <param name="sdep"></param>
		private void SetNrm(OCode lcode, Ncdep sdep)
{
			int ii;

			if (lcode.codeData.CodeCount('D') != 0)
				m_CodeValue['D'] = lcode.codeData.CodeData('D').Clone();		/* Dcode */
			if (lcode.codeData.CodeCount('F') != 0) {	    /* Fcode */
				if (lcode.nrmFeed == true)
					m_CodeValue['F'] = lcode.codeData.CodeData('F').Clone();
				else if (!GGroupValue[1].Equals(0)) {
					m_CodeValue['F'] = new CodeD('F', false, (double)NcMachine.ParaData((int)lcode.codeData.CodeData('F').ToDouble + 1450, 0), Post.PostData['F'].sdgt);
					System.Windows.Forms.MessageBox.Show("Ｆ一桁送りの設定です");
				}
			}
			if (lcode.codeData.CodeCount('H') != 0)
				m_CodeValue['H'] = lcode.codeData.CodeData('H').Clone(); /* Hcode */
			if (lcode.codeData.CodeCount('I') != 0)
				m_CodeValue['I'] = lcode.codeData.CodeData('I').Clone(); /* Icode */
			else
				m_CodeValue['I'] = new CodeD('I', false, 0, Post.PostData['I'].sdgt);
			if (lcode.codeData.CodeCount('J') != 0)
				m_CodeValue['J'] = lcode.codeData.CodeData('J').Clone(); /* Jcode */
			else
				m_CodeValue['J'] = new CodeD('J', false, 0, Post.PostData['J'].sdgt);
			if (lcode.codeData.CodeCount('K') != 0)
				m_CodeValue['K'] = lcode.codeData.CodeData('K').Clone(); /* Kcode */
			else
				m_CodeValue['K'] = new CodeD('K', false, 0, Post.PostData['K'].sdgt);
			if (lcode.codeData.CodeCount('R') != 0)
				m_CodeValue['R'] = lcode.codeData.CodeData('R').Clone(); /* Rcode */
			if (lcode.codeData.CodeCount('S') != 0)
				m_CodeValue['S'] = lcode.codeData.CodeData('S').Clone(); /* Scode */
			if (lcode.codeData.CodeCount('T') != 0)               /* Tcode */
				m_CodeValue['T'] = lcode.codeData.CodeData('T').Clone();
			if ((lcode.codeData.CodeCount('T') != 0 && NcMachine.tchg < 0) ||
				(lcode.Mst[0] != 0 && lcode.Mst[1] == NcMachine.tchg)) {
				ii = ToolNo;
				ToolNo = m_CodeValue['T'].ToInt;
				m_CodeValue['T'] = new CodeD('T', false, (double)ii, Post.PostData['T'].sdgt);

				if (ToolNo != 0) {
					NcMod.toolNoList.Add(ToolNo);
					_main.sdst.ToolAdd();
					Tptr = NcMod.toolNoList.Count - 1;
				}
				else
					Tptr = 0;
				Lost += NcMachine.ttim;
			}

			// SUB PROGRAM CALL がある場合、その情報の設定(subInfo)
			// ない場合は subInfo.iloop == 0
			// （従来はここからサブのＮＣデータをコールしていた）
			if (lcode.Mst[0] != 0 && lcode.Mst[1] == 98) /* call subprogram */ {
				string subn;
				if (lcode.codeData.CodeCount('P') != 0) {
					subn = "O" + (lcode.codeData.CodeData('P').ToDouble).ToString("0000");
				}
				else {
					subn = "O" + ((int)lcode.codeData.CodeData('L').ToDouble).ToString("0000");
				}

				subInfo = new Fmsub(3, null, subn, null);
			}

			// hayaokuri mode
			if (GGroupValue[0].Equals(28) || GGroupValue[0].Equals(29))
				Hokan = 0;

			return;
		}
		/// <summary>
		/// １工具の加工終了処理M98P0006の動作をプログラムで実行
		/// </summary>
		/// <param name="tcode">ncModの元となるＮＣデータ行の情報</param>
		private void P0006(OCode tcode) {
			if (tcode.codeData.CodeCount('P') != 0 || tcode.codeData.CodeData('P').ToInt == 6) {
				m_GGroupValue[1] = new CamUtil.LCode.Gcode(0);
				m_GGroupValue[9] = new CamUtil.LCode.Gcode(80);
				m_GGroupValue[7] = new CamUtil.LCode.Gcode(40);
			}
		}

		/// <summary>このインスタンスと指定した NcMod オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj がこのインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public bool Equals(NcMod obj) {
			if (this.Hokan != obj.Hokan) return false;
			if (this.Mcro != obj.Mcro) return false;

			if (this.CoordinatesNo != obj.CoordinatesNo) return false;
			if (this.NcLineNo != obj.NcLineNo) return false;
			if (this.G10 != obj.G10) return false;
			if (this.ToolNo != obj.ToolNo) return false;
			if (this.Tptr != obj.Tptr) return false;

			if (this.Lost != obj.Lost) return false;

			if (this.Pmirror != obj.Pmirror) return false;
			if (this.ScaleValue != obj.ScaleValue) return false;
			if (this.RotationPlane != obj.RotationPlane) return false;
			if (this.RotationAngle != obj.RotationAngle) return false;
			if (this.m_scalz != obj.m_scalz) return false;
			if (this.m_rotz != obj.m_rotz) return false;
			if (this.OffsetG92 != obj.OffsetG92) return false;
			if (this.LoopNo != obj.LoopNo) return false;

			if (!this.subInfo.Equals(obj.subInfo)) return false;
			for (int ii = 0; ii < this.m_GGroupValue.Length; ii++)
				if (!this.m_GGroupValue[ii].Equals(obj.m_GGroupValue[ii])) return false;
			foreach (char cc in NcMod.ncCodeList)
				if (!CodeD.Equals(this.m_CodeValue[cc], obj.m_CodeValue[cc])) return false;

			return true;
		}
	}
}
