using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil
{
	/// <summary>
	/// 加工機の座標軸（工具の向き）回転角度を計算します。位置ベクトル（座標値）の回転方向でないので注意します[不変]
	/// </summary>
	public class RotationAxis
	{
		/// <summary>最大誤差（０．０００１度換算）のラジアン値を返します</summary>
		private static readonly double gosa = 0.00005 * Math.PI / 180.0;

		/// <summary>ゼロとみなす数値（最大誤差以下）の場合 true、そうでない場合 false。</summary>
		/// <param name="rotang">判定する角度（ラジアン）</param>
		/// <returns>ゼロ判定</returns>
		static private bool Zero(double rotang) {
			if (Math.Abs(rotang) < gosa)
				return true;
			else return false;
		}

		/// <summary>
		/// 座標系の変換方向（ワーク座標⇔フィーチャー[傾斜]座標）を表します
		/// </summary>
		public enum TRANSFORM
		{
			/// <summary>ワーク座標からフィーチャ（傾斜）座標へ</summary>
			WorkToFeature,
			/// <summary>フィーチャ（傾斜）座標からワーク座標へ</summary>
			FeatureToWork
		}

		/// <summary>
		/// 任意軸Ｎの周りの列ベクトル用の回転マトリックス（３Ｘ３）を作成します
		/// </summary>
		/// <param name="n">回転軸ベクトル</param>
		/// <param name="angl">軸周りの回転角度(radian)</param>
		/// <returns>３Ｘ３回転行列</returns>
		static public Matrix RotMatrix(Vector3 n, double angl) {
			// 任意ベクトル回りの回転公式（列ベクトル用）
			double[,] dmat = new double[3, 3];
			n = n.Unit();
			// out[0,0] out[0,1] out[0,2]
			// out[1,0] out[1,1] out[1,2]
			// out[2,0] out[2,1] out[2,2]
			// 任意ベクトル回りの回転公式（列ベクトルの場合）
			// |VxVx(1-cosθ)+   cosθ  VyVx(1-cosθ)-Vz*sinθ  VzVx(1-cosθ)+Vy*sinθ| |x|
			// |VxVy(1-cosθ)+Vz*sinθ  VyVy(1-cosθ)+   cosθ  VzVy(1-cosθ)-Vx*sinθ|*|y|
			// |VxVz(1-cosθ)-Vy*sinθ  VyVz(1-cosθ)+Vx*sinθ  VzVz(1-cosθ)+   cosθ| |z|
			dmat[0, 0] = n.X * n.X + (1 - n.X * n.X) * Math.Cos(angl);
			dmat[1, 1] = n.Y * n.Y + (1 - n.Y * n.Y) * Math.Cos(angl);
			dmat[2, 2] = n.Z * n.Z + (1 - n.Z * n.Z) * Math.Cos(angl);
			dmat[0, 1] = n.X * n.Y * (1 - Math.Cos(angl)) - n.Z * Math.Sin(angl);
			dmat[1, 0] = n.X * n.Y * (1 - Math.Cos(angl)) + n.Z * Math.Sin(angl);
			dmat[1, 2] = n.Y * n.Z * (1 - Math.Cos(angl)) - n.X * Math.Sin(angl);
			dmat[2, 1] = n.Y * n.Z * (1 - Math.Cos(angl)) + n.X * Math.Sin(angl);
			dmat[0, 2] = n.Z * n.X * (1 - Math.Cos(angl)) + n.Y * Math.Sin(angl);
			dmat[2, 0] = n.Z * n.X * (1 - Math.Cos(angl)) - n.Y * Math.Sin(angl);
			return new Matrix(dmat);
		}

		/// <summary>
		/// Ｘ軸周りの回転マトリックス（３Ｘ３）=== 列ベクトルの場合 ===
		/// </summary>
		/// <param name="angl">軸周りの回転角度(radian)</param>
		/// <returns>３Ｘ３回転行列</returns>
		static public Matrix RotMatrix_X(double angl) {
			return new Matrix(new double[,] {
				{ 1.0, 0.0,             0.0            },
				{ 0.0, Math.Cos(angl), -Math.Sin(angl) },
				{ 0.0, Math.Sin(angl),  Math.Cos(angl) }
			});
		}
		/// <summary>
		/// Ｙ軸周りの回転マトリックス（３Ｘ３）=== 列ベクトルの場合 ===
		/// </summary>
		/// <param name="angl">軸周りの回転角度(radian)</param>
		/// <returns>３Ｘ３回転行列</returns>
		static public Matrix RotMatrix_Y(double angl) {
			return new Matrix(new double[,] {
				{  Math.Cos(angl), 0.0, Math.Sin(angl) },
				{  0.0,            1.0, 0.0            },
				{ -Math.Sin(angl), 0.0, Math.Cos(angl) }
			});
		}
		/// <summary>
		/// Ｚ軸周りの回転マトリックス（３Ｘ３）=== 列ベクトルの場合 ===
		/// </summary>
		/// <param name="angl">軸周りの回転角度(radian)</param>
		/// <returns>３Ｘ３回転行列</returns>
		static public Matrix RotMatrix_Z(double angl) {
			return new Matrix(new double[,] {
				{ Math.Cos(angl), -Math.Sin(angl), 0.0 },
				{ Math.Sin(angl),  Math.Cos(angl), 0.0 },
				{ 0.0,             0.0,            1.0 }
			});
		}

		// /////////////////////////////////////////////
		// 以上 static
		// /////////////////////////////////////////////






		/// <summary>単位四元数で座標軸（工具）の回転情報を保存します。位置ベクトルの回転と共役の関係となります。</summary>
		private readonly Quaternion.Unit quat;

		/// <summary>加工軸回転があれば true、無ければ false</summary>
		public bool Rot { get { return !Zero(quat.Angle); } }

		/// <summary>
		/// ３つの回転軸の回転角度から作成します
		/// </summary>
		/// <param name="abc"></param>
		public RotationAxis(Angle3 abc) {

			switch (abc.Jiku) {
			case Angle3.JIKU.Euler_XYZ:
				// |      cosB*cosC                -cosB*sinC                 sinB|
				// | sinA*sinB*cosC+cosA*sinC -sinA*sinB*sinC+cosA*cosC -sinA*cosB|
				// |-cosA*sinB*cosC+sinA*sinC  cosA*sinB*sinC+sinA*cosC  cosA*cosB|
				// 転置行列 = 逆行列
				// | cosB*cosC  sinA*sinB*cosC+cosA*sinC -cosA*sinB*cosC+sinA*sinC|
				// |-cosB*sinC -sinA*sinB*sinC+cosA*cosC  cosA*sinB*sinC+sinA*cosC|
				// | sinB      -sinA*cosB                 cosA*cosB               |
				// A=-A, B=-B, C=-C
				// | cosB*cosC  sinA*sinB*cosC-cosA*sinC  cosA*sinB*cosC+sinA*sinC|
				// | cosB*sinC  sinA*sinB*sinC+cosA*cosC  cosA*sinB*sinC-sinA*cosC|
				// |-sinB       sinA*cosB                 cosA*cosB               |
				//
				// | cosB*cosC  sinA*sinB*cosC-cosA*sinC  cosA*sinB*cosC+sinA*sinC|
				// | cosB*sinC  sinA*sinB*sinC+cosA*cosC  cosA*sinB*sinC-sinA*cosC| 比較 SPATIAL
				// |-sinB       sinA*cosB                 cosA*cosB               |

				//rmat = RotMatrix_X(abc.A);
				//rmat = RotMatrix(rmat * Vector3.vj, abc.B) * rmat;
				//rmat = RotMatrix(rmat * Vector3.vk, abc.C) * rmat;

				// オイラー系の座標回転は積を逆順にする
				quat = Quaternion.Unit.RotQuat_X(abc.A) * Quaternion.Unit.RotQuat_Y(abc.B) * Quaternion.Unit.RotQuat_Z(abc.C);
				// 従来の計算式でチェック
				{
					Quaternion.Unit tmpq;
					tmpq = Quaternion.Unit.RotQuat(abc.A, Vector3.vi);
					tmpq = Quaternion.Unit.RotQuat(abc.B, tmpq.Rotation(Vector3.vj)) * tmpq;
					tmpq = Quaternion.Unit.RotQuat(abc.C, tmpq.Rotation(Vector3.vk)) * tmpq;
					if (this.quat != tmpq) throw new Exception("Euler_XYZ と Spatial の計算チェックでエラー");
				}
				break;
			case Angle3.JIKU.Spatial:
				//rmat = RotMatrix_Z(abc.C) * RotMatrix_Y(abc.B) * RotMatrix_X(abc.A);
				quat = Quaternion.Unit.RotQuat_Z(abc.C) * Quaternion.Unit.RotQuat_Y(abc.B) * Quaternion.Unit.RotQuat_X(abc.A);
				break;
			case Angle3.JIKU.DMU_BC:
				//rmat = RotMatrix_Z(abc.C) * RotMatrix(new Vector3(0.0, Math.Sqrt(0.5), Math.Sqrt(0.5)), abc.B);
				quat = Quaternion.Unit.RotQuat_Z(abc.C) * Quaternion.Unit.RotQuat(abc.B, new Vector3(0.0, Math.Sqrt(0.5), Math.Sqrt(0.5)));
				break;
			case Angle3.JIKU.Euler_ZXZ:
				//  | cosA*cosC-sinA*cosB*sinC  -cosA*sinC-sinA*cosB*cosC   sinA*sinB|
				//  | sinA*cosC+cosA*cosB*sinC  -sinA*sinC+cosA*cosB*cosC  -cosA*sinB|
				//  |                sinB*sinC                  sinB*cosC        cosB|
				// 展開１
				//  | cosA  -sinA  0| |cosC       -sinC          0 |
				// =| sinA   cosA  0|*|cosB*sinC   cosB*cosC  -sinB| 
				//  | 0      0     1| |sinB*sinC   sinB*cosC   cosB|
				// 展開２
				//  | cosA  -sinA  0| |1  0      0   | |cosC  -sinC  0|
				// =| sinA   cosA  0|*|0  cosB  -sinB|*|sinC   cosC  0|
				//  | 0      0     1| |0  sinB   cosB| |0      0     1|
				//  Ｚ軸の回転　　　　Ｘ軸の回転       Ｚ軸の回転

				//rmat = RotMatrix_Z(abc.A);
				//rmat = RotMatrix(rmat * Vector3.vi, abc.B) * rmat;
				//rmat = RotMatrix(rmat * Vector3.vk, abc.C) * rmat;

				// オイラー系の座標回転は積を逆順にする
				quat = Quaternion.Unit.RotQuat_Z(abc.A) * Quaternion.Unit.RotQuat_X(abc.B) * Quaternion.Unit.RotQuat_Z(abc.C);
				// 従来の計算式でチェック
				{
					Quaternion.Unit tmpq;
					tmpq = Quaternion.Unit.RotQuat(abc.A, Vector3.vk);
					tmpq = Quaternion.Unit.RotQuat(abc.B, tmpq.Rotation(Vector3.vi)) * tmpq;
					tmpq = Quaternion.Unit.RotQuat(abc.C, tmpq.Rotation(Vector3.vk)) * tmpq;
					if (this.quat != tmpq) throw new Exception("Euler_ZXZ と Spatial の計算チェックでエラー");
				}
				break;
			case Angle3.JIKU.MCCVG_AC:
				//rmat = RotMatrix_Z(abc.C);
				//rmat = RotMatrix(rmat * Vector3.vi, abc.A) * rmat;

				// オイラー系の座標回転は積を逆順にする
				quat = Quaternion.Unit.RotQuat_Z(abc.C) * Quaternion.Unit.RotQuat_X(abc.A);
				// 従来の計算式でチェック
				{
					Quaternion.Unit tmpq;
					tmpq = Quaternion.Unit.RotQuat(abc.C, Vector3.vk);
					tmpq = Quaternion.Unit.RotQuat(abc.A, tmpq.Rotation(Vector3.vi)) * tmpq;
					if (this.quat != tmpq) throw new Exception("MCCVG_AC の計算チェックでエラー");
				}
				break;
			case Angle3.JIKU.D500_AC:
				// SpatialXYZ で abc.B == 0
				quat = Quaternion.Unit.RotQuat_Z(abc.C) * Quaternion.Unit.RotQuat_X(abc.A);
				// ZXZで検証する
				{
					Quaternion.Unit quat2;
					quat2 = Quaternion.Unit.RotQuat(abc.C, Vector3.vk);
					quat2 = Quaternion.Unit.RotQuat(abc.A, quat2.Rotation(Vector3.vi)) * quat2;
					if (quat != quat2) throw new Exception("D500_AC軸のZXZとの類似性検証でエラー。");
				}
				break;
			case Angle3.JIKU.Null:
				if (abc.ToVector() != Vector3.v0) throw new Exception("febqfqherfb");
				//rmat = new Matrix(3);
				quat = Quaternion.Unit.RotQuat(0.0, Vector3.vk);
				break;
			default:
				throw new Exception("awefbaqfrbh");
			}
			//System.Windows.Forms.MessageBox.Show("四元数 = " + quat.ToString());
		}
		/// <summary>
		/// １つの回転軸と回転角度から作成します
		/// </summary>
		/// <param name="ang">軸周りの回転角度(radian)</param>
		/// <param name="vec">回転軸ベクトル</param>
		public RotationAxis(double ang, Vector3 vec) {
			//rmat = RotMatrix(vec, ang);
			quat = Quaternion.Unit.RotQuat(ang, vec);
		}


		/*
		/// <summary>
		/// ＢＴＵ－１４の回転軸の角度を求める（ > -180 AND &lt;= 180）
		/// </summary>
		/// <returns>回転角度</returns>
		public Angle3 BTU14_4AX() {
			//Angle3 ax = Angle3.a0;
			//
			// 加工平面内の座標回転
			//
			// | cosB  sinA*sinB  cosA*sinB| | cosC  sinC  0|
			// |  0      cosA       -sinA  | |-sinC  cosC  0|
			// |-sinB  sinA*cosB  cosA*cosB| |   0     0   1|
			// =
			// | cosB*cosC-sinA*sinB*sinC   cosB*sinC+sinA*sinB*cosC  cosA*sinB|
			// |      -cosA*sinC                 cosA*cosC             -sinA   |
			// |-sinB*cosC-sinA*cosB*sinC  -sinB*sinC+sinA*cosB*cosC  cosA*cosB|

			if (this.abc.jiku != Angle3.JIKU.Euler_XYZ)
				throw new Exception("x-y-z系オイラー角で作成されていない");

			if (Zero(abc.A) || Zero(abc.B)) {
				return abc;
				//ax['A'] = abc.A;
				//ax['B'] = abc.B;
				//ax['C'] = abc.C;
			}
			else {
				throw new Exception(
					"Ａ軸＝" + abc.degA.ToString("0.000") +
					" Ｂ軸＝" + abc.degB.ToString("0.000") +
					" 工具軸の２度振りには対応していません。");
			}
		}
		*/

		/// <summary>
		/// SPATIAL回転角を出力します
		/// </summary>
		/// <returns>SPATIAL角</returns>
		public Angle3 SPATIAL() {
			// 空間角の３つの値より回転マトリックスを作成
			// 回転Ｂ             回転Ａ
			// | CosB 0  SinB| |1 0      0  |   | CosB  SinA*SinB  CosA*SinB|
			// | 0    1  0   |*|0 CosA -SinA| = | 0     CosA      -SinA     |
			// |-SinB 0  CosB| |0 SinA  CosA|   |-SinB  SinA*CosB  CosA*CosB|
			// 回転Ｃ
			// |CosC -SinC 0| |CosB   SinA*SinB  CosA*SinB|   | CosB*CosC  SinA*SinB*CosC-CosA*SinC  CosA*SinB*CosC+SinA*SinC|
			// |SinC  CosC 0|*|0      CosA      -SinA     | = | CosB*SinC  SinA*SinB*SinC+CosA*CosC  CosA*SinB*SinC-SinA*CosC|
			// |0     0    1| |-SinB  SinA*CosB  CosA*CosB|   |-SinB       SinA*CosB                 CosA*CosB               |
			Angle3 ax = new Angle3(Angle3.JIKU.Spatial, Vector3.v0);
			double cosZ, sinZ;
			Matrix rmat = quat.ToMatrix();

			sinZ = -rmat[2, 0];
			cosZ = Vector3.Length(rmat[2, 1], rmat[2, 2]);
			ax = ax.SetB(Math.Atan2(sinZ, cosZ));

			if (Zero(ax.B - Math.PI / 2.0)) {
				// Ｂ軸が９０度の場合Ｃ軸を０としても成立する角度Ａが存在する（Ａ－Ｃが決定する）
				ax = ax.SetC(0.0).SetA(Math.Atan2(rmat[0, 1], rmat[1, 1]));
			}
			else if (Zero(ax.B + Math.PI / 2.0)) {
				// Ｂ軸が－９０度の場合Ｃ軸を０としても成立する角度Ａが存在する（Ａ＋Ｃが決定する）
				ax = ax.SetC(0.0).SetA(Math.Atan2(-rmat[0, 1], rmat[1, 1]));
			}
			else {
				ax = ax
					.SetC(Math.Atan2(rmat[1, 0] / Math.Cos(ax.B), rmat[0, 0] / Math.Cos(ax.B)))
					.SetA(Math.Atan2(rmat[2, 1] / Math.Cos(ax.B), rmat[2, 2] / Math.Cos(ax.B)));
			}
			// 実際に回転してチェックする
			RotationAxis rtmp = new RotationAxis(ax);
			if (this.quat != rtmp.quat) throw new Exception("回転チェックでエラー");
			return ax;
		}

		/// <summary>
		/// X-Y-Zオイラー角を出力します（未使用）
		/// </summary>
		/// <returns>X-Y-Zオイラー角</returns>
		public Angle3 Euler_XYZ() {
			// [横, 縦]とする
			//rmat[0,0] rmat[0,1] rmat[0,2]
			//rmat[1,0] rmat[1,1] rmat[1,2]
			//rmat[2,0] rmat[2,1] rmat[2,2]
			// 任意ベクトル回りの回転公式（列ベクトルの場合）
			// |VxVx(1-cosθ)+   cosθ  VyVx(1-cosθ)-Vz*sinθ  VzVx(1-cosθ)+Vy*sinθ| |x|
			// |VxVy(1-cosθ)+Vz*sinθ  VyVy(1-cosθ)+   cosθ  VzVy(1-cosθ)-Vx*sinθ|*|y|
			// |VxVz(1-cosθ)-Vy*sinθ  VyVz(1-cosθ)+Vx*sinθ  VzVz(1-cosθ)+   cosθ| |z|
			//
			// Ｘ軸回りに回転したＹ軸回り（Vx=0, Vy=cosA, Vz=sinA）        Ｘ軸回り
			// |   cosB       -sinA*sinB             cosA*sinB          | |1   0     0 |
			// |  sinA*sinB  cosAcosA(1-cosB)+cosB cosAsinA(1-cosB)     | |0 cosA -sinA|
			// | -cosA*sinB  cosAsinA(1-cosB)      sinAsinA(1-cosB)+cosB| |0 sinA  cosA|
			// =
			// | cosB        0       sinB   |
			// | sinA*sinB  cosA  -sinA*cosB|
			// |-cosA*sinB  sinA   cosA*cosB|
			//
			// ＸＹ軸で回転したＺ軸回り（Vx=sinB, Vy=-sinAcosB, Vz=cosAcosB）    Ｘ軸Ｙ軸回転
			// |   sBsB(1-cC)+cC       -sAcBsB(1-cC)-cAcBsC  cAcBsB(1-cC)-sAcBsC| | cB     0      sB|
			// |-sAcBsB(1-cC)+cAcBsC  sAsAcBcB(1-cC)+cC     -cAsAcBcB(1-cC)-sBsC| | sA*sB  cA -sA*cB|
			// | cAcBsB(1-cC)+sAcBsC -cAsAcBcB(1-cC)+sB*sC   cAcAcBcB(1-cC)+cC  | |-cA*sB  sA  cA*cB|
			// =
			// | cB*cC          -cB*sC            sB   |
			// | sA*sB*cC+cA*sC -sA*sB*sC+cA*cC  -sA*cB|
			// |-cA*sB*cC+sA*sC  cA*sB*sC+sA*cC   cA*cB|

			Angle3 ax = new Angle3(Angle3.JIKU.Euler_XYZ, Vector3.v0);
			double cosZ, sinZ;
			Matrix rmat = quat.ToMatrix();

			sinZ = rmat[0, 2];
			cosZ = Vector3.Length(rmat[1, 2], rmat[2, 2]);
			ax = ax.SetB(Math.Atan2(sinZ, cosZ));
			if (Zero(ax.B - Math.PI / 2.0)) {
				// | 0         0         1|
				// | sin(A+C)  cos(A+C)  0|
				// |-cos(A+C)  sin(A+C)  0|
				ax = ax.SetC(0.0).SetB(Math.PI / 2.0);
				cosZ = rmat[1, 1];
				sinZ = rmat[2, 1];
				ax = ax.SetA(Math.Atan2(sinZ, cosZ));
				if (ax.A > Math.PI / 2.0) {
					ax = ax.SetA(ax.A - Math.PI).SetC(Math.PI);
				}
				if (ax.A < -Math.PI / 2.0) {
					ax = ax.SetA(ax.A + Math.PI).SetC(-Math.PI);
				}
			}
			else if (Zero(ax.B + Math.PI / 2.0)) {
				// |  0         0        -1|
				// | -sin(A-C)  cos(A-C)  0|
				// |  cos(A-C)  sin(A-C)  0|
				ax = ax.SetC(0.0).SetB(-Math.PI / 2.0);
				cosZ = rmat[1, 1];
				sinZ = rmat[2, 1];
				ax = ax.SetA(Math.Atan2(sinZ, cosZ));
				if (ax.A > Math.PI / 2.0) {
					ax = ax.SetA(ax.A - Math.PI).SetC(-Math.PI);
				}
				if (ax.A < -Math.PI / 2.0) {
					ax = ax.SetA(ax.A + Math.PI).SetC(Math.PI);
				}
			}
			else {
				// Ａを求める
				cosZ = +rmat[2, 2] / Math.Cos(ax.B);
				sinZ = -rmat[1, 2] / Math.Cos(ax.B);
				ax = ax.SetA(Math.Atan2(sinZ, cosZ));

				// Ｃを求める
				cosZ = +rmat[0, 0] / Math.Cos(ax.B);
				sinZ = -rmat[0, 1] / Math.Cos(ax.B);
				ax = ax.SetC(Math.Atan2(sinZ, cosZ));
			}
			// 実際に回転してチェックする
			RotationAxis rtmp = new RotationAxis(ax);
			if (this.quat != rtmp.quat) throw new Exception("回転チェックでエラー");
			return ax;
		}

		/// <summary>
		/// Z-X-Zオイラー角を出力します（-π/2 ≤ A ≤ π/2,-π ≤ B ≤ π,-π ≤ C ≤ π）
		/// </summary>
		/// <remarks>
		/// ファナックの傾斜面加工指令の座標系指定角度（G68.2X0Y0Z0I[degA]J[degB]K[degC]）。
		/// 条件１．if (B==0) A=0 （C!=0）。
		/// 条件２．if (B!=0) -π/2≤A≤π/2。
		/// </remarks>
		/// <returns>Z-X-Zオイラー角</returns>
		public Angle3 Euler_ZXZ() {
			// [横, 縦]とする
			// rmat[0,0] rmat[0,1] rmat[0,2]
			// rmat[1,0] rmat[1,1] rmat[1,2]
			// rmat[2,0] rmat[2,1] rmat[2,2]
			// 任意ベクトル回りの回転公式
			// VxVx(1-cosθ)+   cosθ  VyVx(1-cosθ)-Vz*sinθ  VzVx(1-cosθ)+Vy*sinθ
			// VxVy(1-cosθ)+Vz*sinθ  VyVy(1-cosθ)+   cosθ  VzVy(1-cosθ)-Vx*sinθ
			// VxVz(1-cosθ)-Vy*sinθ  VyVz(1-cosθ)+Vx*sinθ  VzVz(1-cosθ)+   cosθ
			//
			// 　Ｚ軸回りに回転したＸ軸回り（Vx=cosA, Vy=sinA, Vz=0）       Ｚ軸回り
			// | cosAcosA(1-cosB)+cosB  cosAsinA(1-cosB)       sinA*sinB| | cosA -sinA  0|
			// | cosAsinA(1-cosB)       sinAsinA(1-cosB)+cosB -cosA*sinB| | sinA  cosA  0|
			// |-sinA*sinB              cosA*sinB              cosB     | |  0     0    1|
			// =
			// | cosA  -sinA*cosB   sinA*sinB|
			// | sinA   cosA*cosB  -cosA*sinB|
			// | 0      sinB        cosB     |
			//
			//   ＺＸ軸で回転したＺ軸回り（Vx=sinA*sinB, Vy=-cosA*sinB, Vz=cosB）                                            Ｘ軸Ｙ軸回転
			// | sinAsinAsinBsinB(1-cosC)+cosC      -cosAsinAsinBsinB(1-cosC)-cosBsinC  sinAcosBsinB(1-cosC)-cosAsinBsinC| | cosA  -sinA*cosB   sinA*sinB|
			// |-cosAsinAsinBsinB(1-cosC)+cosB*sinC  cosAcosAsinBsinB(1-cosC)+cosC     -cosAcosBsinB(1-cosC)-sinAsinBsinC| | sinA   cosA*cosB  -cosA*sinB|
			// | sinAcosBsinB(1-cosC)+cosAsinBsinC  -cosAcosBsinB(1-cosC)+sinAsinBsinC  cosBcosB(1-cosC)+cosC            | | 0      sinB        cosB     |
			// =
			// | cosA*cosC-sinA*cosB*sinC  -cosA*sinC-sinA*cosB*cosC  sinA*sinB|
			// | sinA*cosC+cosA*cosB*sinC  -sinA*sinC+cosA*cosB*cosC -cosA*sinB|
			// |       sinB*sinC                sinB*cosC             cosB     |
			//
			// 上記式は、 B = -B、 A = A + π、 C = C + πを代入しても成り立つ。
			// よって、Ｂは正負とも成立する
			//
			Angle3 ax = new Angle3(Angle3.JIKU.Euler_ZXZ, Vector3.v0);
			double cosZ, sinZ;
			Matrix rmat = quat.ToMatrix();

			//cosZ = rmat_new[2, 2];
			// sinZ は正負どちらもＯＫ
			//sinZ = Vector3.Length(rmat_new[2, 0], rmat_new[2, 1]);
			//ax['B'] = SetAng(sinZ, cosZ);
			ax = ax.SetB(Math.Acos(rmat[2, 2]));
			if (Zero(ax.B)) {
				// | cos(A+C) -sin(A+C)  0|
				// | sin(A+C)  cos(A+C)  0|
				// |   0         0       1|
				cosZ = rmat[0, 0];
				sinZ = rmat[1, 0];
				ax = new Angle3(ax.Jiku, new Vector3(0.0, 0.0, Math.Atan2(sinZ, cosZ)));
			}
			else {
				if (!Zero(Math.Asin(-rmat[1, 2] / Math.Sin(ax.B)))) {
					// Ａが-π/2≤θ≤π/2 の範囲になるようにＢの正負を決める
					if (-rmat[1, 2] / Math.Sin(ax.B) < 0.0) {
						ax = ax.SetB(-ax.B);
					}
					sinZ = +rmat[0, 2] / Math.Sin(ax.B);
					cosZ = -rmat[1, 2] / Math.Sin(ax.B);
				}
				else {
					// cosＡが０の場合Ｃがより０に近いようにＢの正負を決める
					if (rmat[2, 1] / Math.Sin(ax.B) < 0.0) {
						ax = ax.SetB(-ax.B);
					}
					sinZ = +rmat[0, 2] / Math.Sin(ax.B);
					cosZ = 0.0;
				}
				// Ａを求める（Ａは-π/2≤θ≤π/2）
				if (cosZ < 0.0)
					throw new Exception("waefknqaerfnqaerjh");
				ax = ax.SetA(Math.Atan2(sinZ, cosZ));
				// Ｃを求める -π≤θ≤π
				cosZ = rmat[2, 1] / Math.Sin(ax.B);
				sinZ = rmat[2, 0] / Math.Sin(ax.B);
				ax = ax.SetC(Math.Atan2(sinZ, cosZ));
			}
			// 実際に回転してチェックする
			RotationAxis rtmp = new RotationAxis(ax);
			if (this.quat != rtmp.quat) throw new Exception("回転チェックでエラー");
			return ax;
		}

		/// <summary>
		/// DMU200Pの３Ｄ固定５軸用（工具軸のみ）ＢＣ軸を出力します
		/// </summary>
		/// <returns>DMU200PのＢＣ軸</returns>
		public Angle3 DMU_BC() {
			// 任意ベクトル回りの回転公式
			// VxVx(1-cosθ)+   cosθ  VyVx(1-cosθ)-Vz*sinθ  VzVx(1-cosθ)+Vy*sinθ
			// VxVy(1-cosθ)+Vz*sinθ  VyVy(1-cosθ)+   cosθ  VzVy(1-cosθ)-Vx*sinθ
			// VxVz(1-cosθ)-Vy*sinθ  VyVz(1-cosθ)+Vx*sinθ  VzVz(1-cosθ)+   cosθ
			//
			//  C軸の回転              B軸の回転 Vx=0 Vy=SQ Vz=SQ (SQ = Sqrt(0.5))
			// |Cos(C) -Sin(C)  0.0| | Cos(B)     -Sin(B)*SQ       Sin(B)*SQ     |
			// |Sin(C)  Cos(C)  0.0| | Sin(B)*SQ   0.5*(1+Cos(B))  0.5*(1-Cos(B))|
			// |0.0     0.0     1.0| |-Sin(B)*SQ   0.5*(1-Cos(B))  0.5*(1+Cos(B))|
			// =
			//  Cos(B)*Cos(C) - Sin(B)*Sin(C)*SQ  -Sin(B)*Cos(C)*SQ - 0.5*(1+Cos(B))*Sin(C)  Sin(B)*Cos(C)*SQ - 0.5*(1-Cos(B))*Sin(C)
			//  Cos(B)*Sin(C) + Sin(B)*Cos(C)*SQ  -Sin(B)*Sin(C)*SQ + 0.5*(1+Cos(B))*Cos(C)  Sin(B)*Sin(C)*SQ + 0.5*(1-Cos(B))*Cos(C)
			// -Sin(B)*SQ                          0.5*(1-Cos(B))                            0.5*(1+Cos(B))

			Angle3 ax = new Angle3(Angle3.JIKU.DMU_BC, Vector3.v0);
			double cosZ, sinZ;
			Matrix rmat = quat.ToMatrix();

			// ///////////////////////////////////////////////////
			// 工具軸の方向のみの問題のため、３列目のみで計算する
			// ///////////////////////////////////////////////////

			// rmat[2,2] = 0.5*(1+Cos(B))
			double dtmp = rmat[2, 2] / 0.5 - 1;
			if (Math.Abs(dtmp) > 1.0 + gosa) throw new Exception("計算不能です");
			if (dtmp > 1.0) dtmp = 1.0;
			if (dtmp < -1.0) dtmp = -1.0;
			ax = ax.SetB(Math.Acos(dtmp));
			if (Zero(ax.B)) {
				ax = ax.SetC(0.0);
			}
			else {
				// rmat[0,2] = Sin(B)*Cos(C)*Sqrt(0.5) - 0.5*(1-Cos(B))*Sin(C)
				// rmat[1,2] = Sin(B)*Sin(C)*Sqrt(0.5) + 0.5*(1-Cos(B))*Cos(C)
				double AA = Math.Sin(ax.B) * Math.Sqrt(0.5);
				double BB = 0.5 * (1 - Math.Cos(ax.B));
				// rmat[0,2] = AA*Cos(C) - BB*Sin(C)
				// rmat[1,2] = BB*Cos(C) + AA*Sin(C) 
				cosZ = (BB * rmat[1, 2] + AA * rmat[0, 2]) / (AA * AA + BB * BB);
				sinZ = (AA * rmat[1, 2] - BB * rmat[0, 2]) / (AA * AA + BB * BB);
				ax = ax.SetC(Math.Atan2(sinZ, cosZ));
			}
			// 実際に回転してチェックする
			RotationAxis rtmp = new RotationAxis(ax);
			for (int ii = 0; ii < 3; ii++)
				if (Math.Abs(rtmp.quat.ToMatrix()[ii, 2] - rmat[ii, 2]) > gosa) throw new Exception("qwefbqerfbrh");
			return ax;
		}

		/// <summary>
		/// MCC3016VGの３Ｄ固定５軸用（工具軸のみ）ＡＣ軸を出力します
		/// </summary>
		/// <returns>MCC3016VGのＡＣ軸</returns>
		public Angle3 MCCVG_AC() {
			// | cosC  -cosA*sinC   sinA*sinC|
			// | sinC   cosA*cosC  -sinA*cosC|
			// | 0      sinA        cosA     |
			Angle3 ax = Euler_ZXZ();
			if (Math.Cos(ax.B) < -gosa) throw new Exception("工具軸が反対向きになっています。計算不能です");
			return new Angle3(Angle3.JIKU.MCCVG_AC, new Vector3(ax.B, 0.0, ax.A));
		}

		/// <summary>
		/// Ｄ５００の３Ｄ固定５軸用（工具軸のみ）ＡＣ軸を出力します
		/// </summary>
		/// <returns>SPATIAL角</returns>
		public Angle3 D500_AC() {
			// 回転Ｃ           回転Ａ
			// |CosC -SinC  0| |1  0     0   |   |CosC -CosA*SinC  SinA*SinC|
			// |SinC  CosC  0|*|0  CosA -SinA| = |SinC  CosA*CosC -SinA*CosC|
			// |0     0     1| |0  SinA  CosA|   |0     SinA       CosA     |
			//
			Angle3 ax = new Angle3(Angle3.JIKU.D500_AC, Vector3.v0);
			Matrix rmat = quat.ToMatrix();

			// ///////////////////////////////////////////////////
			// 工具軸の方向のみの問題のため、３列目のみで計算する
			// ///////////////////////////////////////////////////
			if (rmat[2, 2] < -gosa) throw new Exception("工具軸が反対向きになっています。計算不能です");
			ax = ax.SetA(-Math.Acos(rmat[2, 2]));	// D500では主にＡ軸マイナス側を使用する
			if (!Zero(ax.A)) {
				ax = ax.SetC(Math.Atan2(rmat[0, 2] / Math.Sin(ax.A), -rmat[1, 2] / Math.Sin(ax.A)));
			}
			// 実際に回転してチェックする
			RotationAxis rtmp = new RotationAxis(ax);
			for (int ii = 0; ii < 3; ii++)
				if (Math.Abs(rtmp.quat.ToMatrix()[ii, 2] - rmat[ii, 2]) > gosa) throw new Exception("回転チェックでエラー");
			return ax;
		}

		/// <summary>
		/// 任意軸回りの回転を表す四元数を行列表現（W, X, Y, Z）で出力します
		/// </summary>
		/// <returns>四元数（４行１列の行列。１行目が実数部）</returns>
		public Matrix General() {

			// 行列と四元数の変換をチェック
			{
				Quaternion.Unit unit = new Quaternion.Unit(quat.ToMatrix());
				if (this.quat != unit) throw new Exception("変換チェックでエラー");
			}
			return new Matrix(quat.ToQuaternion().ToDoubleArray());
		}


		/// <summary>
		/// 指示された位置の座標系変換（Work->Feature座標系, Feature->Work座標系）後の座標値を出力します
		/// </summary>
		/// <param name="tf">変換方向</param>
		/// <param name="value">位置情報</param>
		/// <returns>変換後の座標値</returns>
		public Vector3 Transform(TRANSFORM tf, Vector3 value) {
			switch (tf) {
			case TRANSFORM.WorkToFeature:	// quat は座標系の回転を表すため共役で実行します
				return quat.Conjugation().Rotation(value);
			case TRANSFORM.FeatureToWork:	// quat は座標系の回転を表すため逆回転はそのままです
				return quat.Rotation(value);
			default:
				throw new Exception("aerfbaerb");
			}
		}

		/// <summary>工具軸方向（Ｚ＋）を出力します</summary>
		/// <returns>工具軸ベクトル</returns>
		public Vector3 ToolDir() { return Transform(TRANSFORM.FeatureToWork, Vector3.vk); }
	}
}
