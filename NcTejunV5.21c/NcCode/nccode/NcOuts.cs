using System;
using System.Collections.Generic;
using System.Text;

using CamUtil;

namespace NcCode.nccode
{
	/// <summary>
	/// ＮＣデータにより移動する単一の位置情報を保存します。[不変]
	/// </summary>
	public class NcOuts
	{
		/// <summary>移動補間方法 0:早送り 1:直線 2:円弧 3:円弧</summary>
		public int Hokan { get { return m_hokan; } } private readonly int m_hokan;
		/// <summary>移動有無と補正 0:なし 1:工具径補正なしの移動 2:工具径補正ありの移動</summary>
		public readonly int idoa;
		/// <summary>平面指定ＸＹＺベクトル（G17=0,0,1）</summary>
		public readonly Vector3 heimn;

		/// <summary>移動量（中間点の場合も常に加工開始点からの量）</summary>
		public Pals Pls { get { return m_pls; } } private readonly Pals m_pls;
		/// <summary>機械座標値（ミラー、回転、中間点等を含む）</summary>
		public Ichi Cloc { get { return m_cloc; } } private readonly Ichi m_cloc;
		/// <summary>移動先の絶対位置（ＮＣデータ値）</summary>
		public Ichi Ichi { get { return m_ichi; } } private readonly Ichi m_ichi;
		/// <summary>移動先の絶対位置（ミラー、回転、中間点等を含む）</summary>
		public Ichi Ichj { get { return m_ichj; } } private readonly Ichi m_ichj;

		/// <summary>円弧補間の半径</summary>
		public double Rad0 { get { return m_rad0; } } private readonly double m_rad0, m_rad1;
		/// <summary>円弧補間の中心（円弧終点基準）</summary>
		public Vector3 Centr { get { return m_centr; } } private readonly Vector3 m_centr;
		/// <summary>円弧補間の回転角度</summary>
		public double Deg { get { return m_deg; } } private readonly double m_deg;

		/// <summary>クローン</summary>
		public NcOuts Clone() { return (NcOuts)this.MemberwiseClone(); }

		/// <summary>
		/// 初期化コンストラクタです
		/// </summary>
		/// <param name="ichi">初期の座標値</param>
		internal NcOuts(Ichi ichi) {
			m_hokan = 0;
			idoa = 0;
			m_cloc = ichi;
			m_ichj = ichi;
			m_ichi = ichi;

			m_rad0 = m_rad1 = 0.0;
			m_centr = Vector3.v0;
			m_deg = 0.0;
			m_pls = new Pals();
		}
		/// <summary>
		/// out0 の初期化を実行します（コンストラクタ）
		/// </summary>
		/// <param name="src">元のNcOuts</param>
		internal NcOuts(NcOuts src) {
			m_hokan = src.Hokan;
			heimn = src.heimn;
			m_ichi = src.Ichi;
			m_ichj = src.Ichj;
			m_cloc = src.Cloc;
			m_rad0 = src.m_rad0;
			m_rad1 = src.m_rad1;
			m_centr = src.m_centr;
			m_deg = src.m_deg;

			idoa = 0;
			m_pls = new Pals(0);
		}

		/// <summary>
		/// 中間点の作成など新たな座標値を設定します（コンストラクタ）
		/// </summary>
		/// <param name="src">元のNcOuts</param>
		/// <param name="itmp">移動の残量</param>
		internal NcOuts(NcOuts src, Ichi itmp) {
			m_hokan = src.Hokan;
			idoa = src.idoa;
			heimn = src.heimn;
			m_ichi = src.Ichi;
			m_rad0 = src.m_rad0;
			m_rad1 = src.m_rad1;
			m_centr = src.m_centr;
			m_deg = src.m_deg;

			m_ichj = src.Ichj - itmp;
			m_cloc = src.Cloc - itmp;
			m_pls = new Pals(src.Pls.ToIchi(Post.PostData['X'].sdgt) - itmp);
		}

		/// <summary>
		/// 更新するコンストラクタ
		/// </summary>
		/// <param name="src">コピー元</param>
		/// <param name="idopass">移動状態 0:移動 1:ＸＹのみ 2:移動なし 3:無意データ</param>
		/// <param name="lcode"></param>
		/// <param name="mod0"></param>
		/// <param name="p_fsub">マクロ変数</param>
		internal NcOuts(NcOuts src, int idopass, OCode lcode, NcMod mod0, NcMachine.Variable p_fsub) {

			// src は前行を参照したNcOuts(NcOuts src)で作成され、Shokika(){idoa = 0;m_pls = new Pals(0);}を実行している。			

			// mod0の補間をセット
			this.m_hokan = mod0.Hokan;
			// Ｇコードにより現在の平面定義を設定
			switch (mod0.GGroupValue[2].ToInt()) {
			case 17: this.heimn = Vector3.vk; break;
			case 18: this.heimn = Vector3.vj; break;
			case 19: this.heimn = Vector3.vi; break;
			default: throw new Exception("");
			}

			if (idopass >= 2 || mod0.GGroupValue[0].Equals(39)) {
				this.idoa = src.idoa;
				this.m_pls = src.m_pls;
				this.m_cloc = src.m_cloc;
				this.m_ichi = src.m_ichi;
				this.m_ichj = src.m_ichj;
				this.m_rad0 = src.m_rad0;
				this.m_rad1 = src.m_rad1;
				this.m_centr = src.m_centr;
				this.m_deg = src.m_deg;
				return;
			}

			// 新たな座標値（m_ichi, m_centr）を設定
			this.m_ichi = SetIchi(src.Ichi, idopass, lcode, mod0, p_fsub);

			// 円弧補間情報セット
			// 暫定処置あり：チェック後、円弧補間でない場合にVector3.v0を代入すること
			// 暫定処置あり：チェック後、円弧補間半径を計算すること
			this.m_centr = src.m_centr;
			this.m_rad0 = src.m_rad0;
			this.m_rad1 = src.m_rad1;
			if (this.m_hokan > 1)
				this.m_centr = SetCentr(this.m_ichi - src.Ichi, lcode, mod0);

			// ミラー・スケール・回転の処理された座標値（m_ichj, m_centr, m_rad）を計算
			// 間違いがある！！m_radはこの後setcloc()によって再び変更されてる！！
			this.m_ichj = this.m_ichi;
			Mirror(ref this.m_hokan, ref this.m_ichj, ref this.m_centr, ref this.m_rad0, ref this.m_rad1, mod0);

			// 移動量plsを計算
			this.m_cloc = this.m_ichj + NcMachine.Offset(mod0.CoordinatesNo);
			this.m_pls = new Pals(this.m_cloc - src.Cloc);

			// RADIUS DEGREE SET
			this.m_deg = this.m_rad0 = this.m_rad1 = 0.0;
			if (this.m_hokan > 1 && Vector3.Vscal(this.m_centr, this.m_centr) > Post.minim) {
				Vector3 evec = -this.m_centr;
				Vector3 svec = evec - this.m_pls.ToXYZ();
				if (m_pls.ToXYZ() == Vector3.v0)
					this.m_deg = (this.m_hokan == 2 ? -2 * Math.PI : 2 * Math.PI);
				else {
					this.m_deg = _Geometry_Plane.Vangle(svec.ToVector2(mod0.GGroupValue[2].ToInt()), evec.ToVector2(mod0.GGroupValue[2].ToInt()), this.m_hokan);
					if (Math.Min(2 * Math.PI - Math.Abs(this.m_deg), Math.Abs(this.m_deg)) * svec.Abs < 0.001) throw new Exception("微小円弧の処理エラー");
				}
				this.m_rad0 = svec.ToVector2(mod0.GGroupValue[2].ToInt()).Abs;
				this.m_rad1 = evec.ToVector2(mod0.GGroupValue[2].ToInt()).Abs;
			}

			this.idoa = SetIdo(src.idoa, mod0);
			return;
		}
		/// <summary>
		/// 新しい移動位置を計算します
		/// </summary>
		/// <param name="src_ichi"></param>
		/// <param name="idopass">移動状態 0:移動 1:ＸＹのみ 2:移動なし 3:無意データ</param>
		/// <param name="lcode">現在行のデータ</param>
		/// <param name="mod0">現在行のモーダル値</param>
		/// <param name="p_fsub">マクロ変数</param>
		private Ichi SetIchi(Ichi src_ichi, int idopass, OCode lcode, NcMod mod0, NcMachine.Variable p_fsub) {
			Ichi p_ichi;

			if (mod0.GGroupValue[3].Equals(90) || mod0.GGroupValue[3].Equals(91)) { ;}
			else _main.Error.Ncerr(3, "Gcode 3 group error");

			// G92 DATA SET
			if (mod0.GGroupValue[0].Equals(92)) {
				LogOut.CheckCount("NcOuts 131", false, "G92 が設定されました");
				p_ichi = src_ichi;
			}
			// XYZ idou seigyo
			else if (idopass == 0)
				p_ichi = src_ichi.Update(lcode.codeData.XYZ(mod0.GGroupValue[3].ToInt()), lcode.codeData.ABC(mod0.GGroupValue[3].ToInt()));
			else if (idopass == 1)
				p_ichi = src_ichi.Update(lcode.codeData.XY_(mod0.GGroupValue[3].ToInt()), NcZahyo.Null);
			else throw new Exception("プログラムエラー");

			// プローブ位置情報の追加
			if (mod0.GGroupValue[0].Equals(31)) {
				//idoab = 0.75 * idoab;
				LogOut.CheckCount("NcOuts 150", false, "プローブ位置情報の追加");
				p_fsub.SystemSet_PROBE(src_ichi + (0.75 * (p_ichi - src_ichi)));
			}
			return p_ichi;
		}
		/// <summary>
		/// 新しい円弧補間中心を計算します
		/// </summary>
		/// <param name="p_mov">移動量</param>
		/// <param name="lcode">現在行のデータ</param>
		/// <param name="mod0">現在行のモーダル値</param>
		private Vector3 SetCentr(Ichi p_mov, OCode lcode, NcMod mod0) {
			Vector3 p_centr;

			// 円弧補間情報セット
			//Vector3 centr2;	// 円弧中心位置（円弧終点基準）
			_Geometry_Plane.Vector2 end = _Geometry_Plane.Vector2.v0;	// 円弧終点（基準）
			_Geometry_Plane.Vector2 stt = -p_mov.ToXYZ().ToVector2(mod0.GGroupValue[2].ToInt());	// 円弧始点位置（円弧終点基準）
			if (lcode.codeData.CodeCount('R') != 0) {
				_Geometry_Plane.Circle circle;
				double rad = mod0.CodeValue['R'].ToDouble;
				if ((mod0.Hokan == 2 && rad > 0.0) || (mod0.Hokan == 3 && rad < 0.0))
					circle = new _Geometry_Plane.Circle(stt, end, Math.Abs(rad), false);	// 進行方向右側の中心の円
				else
					circle = new _Geometry_Plane.Circle(stt, end, Math.Abs(rad), true);	// 進行方向左側の中心の円
				p_centr = circle.Center.ToVector3(mod0.GGroupValue[2].ToInt(), Vector3.v0);
			}
			else {
				Vector3 IJK = new Vector3(mod0.CodeValue['I'].ToDouble, mod0.CodeValue['J'].ToDouble, mod0.CodeValue['K'].ToDouble);
				p_centr = stt.ToVector3(mod0.GGroupValue[2].ToInt(), Vector3.v0) + IJK;
				switch (mod0.GGroupValue[2].ToInt()) {
				case 17: if (IJK.Z != 0.0) throw new Exception("efdbqheb"); break;
				case 18: if (IJK.Y != 0.0) throw new Exception("efdbqheb"); break;
				case 19: if (IJK.X != 0.0) throw new Exception("efdbqheb"); break;
				}
			}
			return p_centr;
		}

		/// <summary>
		/// ミラー・スケール・回転を処理します
		/// </summary>
		/// <param name="p_hokan"></param>
		/// <param name="p_ichj"></param>
		/// <param name="p_centr"></param>
		/// <param name="p_rad0"></param>
		/// <param name="p_rad1"></param>
		/// <param name="mod0"></param>
		private void Mirror(ref int p_hokan, ref Ichi p_ichj, ref Vector3 p_centr, ref double p_rad0, ref double p_rad1, NcMod mod0) {
			// ////////////////////////////////////
			// ミラーなどを考慮した座標値を作成する
			// (ichj, centrは書き換え)
			// ////////////////////////////////////
			if (p_hokan > 1) {
				switch (mod0.GGroupValue[2].ToInt()) {
				case 17: if (mod0.Pmirror.ToMirrABC.Z * NcMachine.MIRROR.ToMirrABC.Z < 0) p_hokan = p_hokan == 2 ? 3 : 2; break;
				case 18: if (mod0.Pmirror.ToMirrABC.Y * NcMachine.MIRROR.ToMirrABC.Y < 0) p_hokan = p_hokan == 2 ? 3 : 2; break;
				case 19: if (mod0.Pmirror.ToMirrABC.X * NcMachine.MIRROR.ToMirrABC.X < 0) p_hokan = p_hokan == 2 ? 3 : 2; break;
				default: throw new Exception("qwfbqhb");
				}
			}

			// G92 ワーク座標系
			if (mod0.OffsetG92 != Ichi.p0)
				p_ichj = p_ichj + mod0.OffsetG92;

			// PROGRAMABLE MIRROR
			p_ichj = p_ichj.Scaling(mod0.Pmirror.ToVector(), mod0.Pmirror.ToMirrXYZ);
			p_centr = p_centr.Scaling(mod0.Pmirror.ToMirrXYZ);

			// SCALE
			if (mod0.GGroupValue[11].Equals(51)) {
				p_ichj = p_ichj.Scaling(mod0.ScaleCenter.ToXYZ(), mod0.ScaleValue);
				p_centr = p_centr.Scaling(mod0.ScaleValue);
				p_rad0 *= mod0.ScaleValue.Z;
				p_rad1 *= mod0.ScaleValue.Z;
			}

			// ROTATION
			if (mod0.GGroupValue[16].Equals(68)) {
				Ichi cc;
				if (mod0.GGroupValue[11].Equals(51))
					cc = mod0.RotationCenter.Scaling(mod0.ScaleCenter.ToXYZ(), mod0.ScaleValue);
				else
					cc = mod0.RotationCenter;
				p_ichj = p_ichj.Rotation(mod0.RotationPlane, cc.ToXYZ(), Angle3.DtoR(mod0.RotationAngle));
				p_centr = p_centr.Rotation(mod0.RotationPlane, Angle3.DtoR(mod0.RotationAngle));
			}
			// PARAMETER MIRROR
			p_centr = p_centr.Scaling(NcMachine.MIRROR.ToMirrXYZ);
			p_ichj = p_ichj.Scaling(Vector3.v0, NcMachine.MIRROR.ToMirrXYZ);

			if (ProgVersion.Debug) {
				if (mod0.OffsetG92 != Ichi.p0)
					LogOut.CheckCount("NcOuts 452", true, "Ｇ９２の実行");
				if (mod0.Pmirror.X.HasValue || mod0.Pmirror.Y.HasValue || mod0.Pmirror.Z.HasValue)
					LogOut.CheckCount("NcOuts 454", true, "プログラマブルミラーの実行");
				if (NcMachine.MIRROR != NcZahyo.Null)
					LogOut.CheckCount("NcOuts 456", true, "パラメータミラーの実行");
				if (mod0.GGroupValue[11].Equals(51))
					LogOut.CheckCount("NcOuts 458", true, "スケーリングの実行");
				if (mod0.GGroupValue[16].Equals(68))
					LogOut.CheckCount("NcOuts 460", true, "座標回転の実行");
			}
			else {
				if (mod0.OffsetG92 != Ichi.p0)
					throw new Exception("Ｇ９２の実行は未検証です");
				if (mod0.Pmirror.X.HasValue || mod0.Pmirror.Y.HasValue || mod0.Pmirror.Z.HasValue)
					throw new Exception("プログラマブルミラーの実行は未検証です");
				if (NcMachine.MIRROR != NcZahyo.Null)
					throw new Exception("パラメータミラーの実行は未検証です");
				if (mod0.GGroupValue[11].Equals(51))
					throw new Exception("スケーリングの実行は未検証です");
				if (mod0.GGroupValue[16].Equals(68))
					throw new Exception("座標回転の実行は未検証です");
				if (p_centr != this.Centr || p_ichj != this.Ichi)
					throw new Exception("ewfqwfeqfqftgr");
			}

			return;
		}
		/// <summary>
		/// 移動有無と補正情報を計算します
		/// </summary>
		/// <param name="src_idoa"></param>
		/// <param name="mod0"></param>
		private int SetIdo(int src_idoa, NcMod mod0) {
			int p_idoa = src_idoa;

			// idoa data set
			if (this.Hokan > 1 && Math.Abs(this.Deg) > Post.minim) {
				p_idoa = 2;
			}
			else {
				if (!this.Pls.A0 || !this.Pls.B0 || !this.Pls.C0) p_idoa = 1;
				switch (mod0.GGroupValue[2].ToInt()) {
				case 17:
					if (this.Hokan <= 1 && (!this.Pls.X0 || !this.Pls.Y0)) p_idoa = 2;
					else if (!this.Pls.Z0) p_idoa = 1;
					break;
				case 18:
					if (this.Hokan <= 1 && (!this.Pls.Z0 || !this.Pls.X0)) p_idoa = 2;
					else if (!this.Pls.Y0) p_idoa = 1;
					break;
				case 19:
					if (this.Hokan <= 1 && (!this.Pls.Y0 || !this.Pls.Z0)) p_idoa = 2;
					else if (!this.Pls.X0) p_idoa = 1;
					break;
				default: throw new Exception("wkedqwedn");
				}
			}
			return p_idoa;
		}
	}
}
