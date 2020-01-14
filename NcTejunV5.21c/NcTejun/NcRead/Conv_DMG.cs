using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using CamUtil;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	partial class Conv_DMG : Conv
	{
		/// <summary>加工機情報</summary>
		private NcdTool.Mcn1 mach;

		// 傾斜軸の角度（string）
		int keishaNo;
		// 傾斜角度の設定
		private RotationAxis rotSP;

		/// <summary>ＮＣデータの変換仕様（Conv_Generalで用いる）</summary>
		protected Spec spec_stt, spec_end;

		public Conv_DMG(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach)
			: base(10, false, new double[] { toolList[0].Skog.Tld.XmlT.TlAPCHZ }, false, toolList) {
			this.mach = mach;

			this.keishaNo = -1;
			this.rotSP = null;

			spec_stt = new Spec("start");
			spec_end = new Spec("end");
			this.g01ari = false;
		}

		public override OutLine ConvExec() {
			//this.txtd = txtd;
			NcLineQue txtd = this.NcQue.NcPeek(0);

			// 移動範囲データの作成
			if (txtd.LnumT >= 0) NcoutName.ncdist.PassLength(txtd);

			// ＮＣデータ変換
			Convert_General(txtd, spec_stt);
			Convert_DMG(txtd);
			Convert_General(txtd, spec_end);

			return txtd.OutLine;
		}

		private void Convert_DMG(NcLineQue txtd) {
			// ＤＭＧ仕様への変換
			if (txtd.OutLine.Txt.Length > 0) {
				if (txtd.OutLine.Txt[0] == '%') {
					Fn_dmg_name(txtd, NcoutName.OutnamNEW);
				}
				if (txtd.B_g8 && txtd.G8 == 80)
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G80", ""));
				if (txtd.B_g6 && txtd.G6 == 67)
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G67", ""));
				// 工具格納処理
				if (txtd.B_p0006) {
					// 工具格納前に計測マクロをコール
					if (Keisoku)
						txtd.OutLine.MaeAdd("% TNC:\\TG\\PROG\\POINTMEAS.H");
					if (txtd.OutLine.Txt.IndexOf("M98P0006") != 0) throw new Exception("afqfvafvaf");
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("M98P0006", "% TNC:\\TG\\PROG\\P0006.H"));
					if (keishaNo != NcoutName.Skog.Tld.XmlT.MachiningAxisList.Length - 1)
						throw new Exception("軸傾斜角度の数がＸＭＬとＮＣデータで一致しない。");
				}

				// 工程間の切れ目処理（工具軸変更）
				if (txtd.NcLine.IndexOf(ClLink5Axis.ChgAX) == 0) {
					bool a0 = NcoutName.Skog.Tld.XmlT.MachiningAxisList.Any(ang => ang.ToVector() == Vector3.v0);
					if (Index.dimensionAXIS == 2) {
						if (a0) {
							//２次元加工でのG0ポスト工具軸変更を伴う工程間接続（３軸含む）
							CamUtil.LogOut.CheckCount("Conv_DMG 098", true, NcoutName.Outnam + " " + Program._2d_kot_t00.mess);
						}
						else {
							//２次元加工でのG0ポスト工具軸変更を伴う工程間接続（傾斜のみ）
							// 2019/10/02 DMU210P にてトライ実施。マクロのオフセットの間違い修正により加工完了
							// ただし、実際には座標系を変えなかったことと他の設備ではマクロ修正を伴うことから立会いは継続する
							CamUtil.LogOut.CheckCount("Conv_DMG 103", true, NcoutName.Outnam + " " + Program._2d_kot_t11.mess);
						}
					}
					else {
						if (a0) {
							//３次元加工でのG0ポスト工具軸変更を伴う工程間接続（３軸含む）
							CamUtil.LogOut.CheckCount("Conv_DMG 110", true, NcoutName.Outnam + " " + Program._3d_kot_t00.mess);
						}
						else
							CamUtil.LogOut.CheckCount("Conv_DMG 103", false, "工具軸変更を伴う工程間接続（３次元加工、傾斜のみ）in " + NcoutName.Outnam);
					}
					NcLineCode ntmp = this.NcQue.NcPeek(+1);
					if (ntmp.B_26('X') == false || ntmp.B_26('Y') == false || ntmp.B_26('Z') == true)
						throw new Exception("工程間移動後のＮＣデータに異常があります。");

					txtd.OutLine.Set("% TNC:\\TG\\PROG\\MBMAX_Z.H");
					// 工具軸設定のＮＣデータ挿入
					Fn_Rot(txtd);
					// クーラント処理
					txtd.OutLine.MaeAdd("M09");
					txtd.OutLine.AtoAdd("G00 G90" +
						" X" + ntmp.Xyzsf.X.ToString("0.0##") +
						" Y" + ntmp.Xyzsf.Y.ToString("0.0##"));
					txtd.OutLine.AtoAdd("M+Q1702 ; COOLANT");
				}
				// 工程間の切れ目処理（工具軸同一）
				if (txtd.NcLine.IndexOf(ClLink5Axis.Kotei) == 0) {
					if (Index.dimensionAXIS == 2) {
						if (NcoutName.Skog.Tld.XmlT.Keisha == false) {
							//２次元加工でのG0ポスト工程間接続（３軸）
							CamUtil.LogOut.CheckCount("Conv_DMG 136", true, NcoutName.Outnam + " " + Program._2d_kot_s00.mess);
						}
					}
					else {
						if (NcoutName.Skog.Tld.XmlT.Keisha == false && NcoutName.Skog.Tld.XmlT.SimultaneousAxisControll == false) {
							//３次元加工でのG0ポスト工程間接続（３軸）
							CamUtil.LogOut.CheckCount("Conv_DMG 146", true, NcoutName.Outnam + " " + Program._3d_kot_s00.mess);
						}
					}
					NcLineCode aa = this.NcQue.NcPeek(+1);
					if (aa.B_26('X') == false || aa.B_26('Y') == false || aa.B_26('Z') == true)
						throw new Exception("工程間移動後のＮＣデータに異常があります。");
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace(ClLink5Axis.Kotei, "% TNC:\\TG\\PROG\\MBMAX_Z.H"));
					// 直交軸の移動
					if (Index.dimensionAXIS == 2) Choko(txtd);
				}
			}

			// Ｇ１００の処理(G100T01)
			if (txtd.B_g100 == true) {
				Fn_dmg_g100(txtd);
				keishaNo = -1;
				Fn_Rot(txtd);
			}
			// 固定サイクルモード
			// カスタムマクロモード
			else if (txtd.G8 != 80 || txtd.G6 != 67) {
				if (Index.dimensionAXIS != 2)
					throw new Exception("ＮＣデータの次元が間違っています。 in NcConv_DMG");
				if (NcoutName.DimensionG01 != 2) {
					throw new Exception("３次元加工モードでサイクルは使用できません。" +
						"「加工手順」にて２次元加工モード('D 2D'の行を挿入)に変更してください in NcConv_DMG");
				}
				Fn_cycle(txtd);
			}
			// 一般モード
			else {
				if (Keisoku) {
					// 計測の場合、一般の移動情報は不要
					if (txtd.OutLine.Txt.Length > 0) {
						if (txtd.OutLine.Txt[0] != '%')
							txtd.OutLine.Set("");
					}
				}
				// Ｇ１００の次の行
				else if (keishaNo >= 0 && this.NcQue.NcPeek(-1).B_g100 == true) {
					if (txtd.NcLine != "G00G90X0Y0" && txtd.NcLine != "G00G90X0Y0;") {
						throw new Exception("2debehdbqweh");
					}
					if (rotSP.Rot) {
						int m130 = this.NcQue.NcPeek(-1).OutLine.NcLineOut().IndexOf("M130");
						switch (Index.dimensionAXIS) {
						case 2:
							if (m130 < 0) throw new Exception("2debehdbqweh");
							txtd.OutLine.Set("");
							break;
						case 3:
							if (m130 >= 0) throw new Exception("2debehdbqweh");
							txtd.OutLine.Set("G00G90");
							break;
						default:
							throw new Exception("awfaefrbh");
						}
					}
				}
				else {
					// 工具径補正のチェック
					Fn_diam_offset();

					// 円弧補間
					if (txtd.G1 == 2 || txtd.G1 == 3) {
						// 円弧補間のチェック
						Fn_cir_chk();
						// 円弧補間のコード変換
						Fn_dmg_g02(txtd);
					}
				}
			}

			// クーラントＯＮＯＦＦの挿入（Ｚ移動時）
			if (!Keisoku) Fn_clnt(txtd);

			// M01の挿入（ＤＭＧ）
			if (NcoutName.Smch.m01) {
				if (txtd.Lnumb == -3)
					txtd.OutLine.MaeAdd("M01");
			}

			// M30の削除（dncオフの処置）
			// M30の削除（メモリ運転にすぐに切り替えられるようにＤＮＣでもM30を消しておく）
			if (txtd.Lnumb == -2)
				txtd.OutLine.Set(txtd.OutLine.Txt.Replace("M30", ""));
		}


		/// <summary>
		/// ＤＭＧ用工具交換
		/// </summary>
		/// <param name="txtd"></param>
		protected void Fn_dmg_g100(NcLineCode txtd) {
			int pos = txtd.OutLine.Txt.IndexOf("G100");
			txtd.OutLine.Set(txtd.OutLine.Txt.Substring(0, pos));

			// JOKEN.TAB に入力されるデータ（ＤＮＣ運転時はＤＮＣシステムより、メモリ運転時は加工機のプログラムより）
			// ＤＮＣの場合は以下の項目はＮＣデータの整合チェックのためのみに使用する
			// Q40,Q41-Q45,Q02 の工具の情報は TOOL_INFO.TAB にも入力される（ＤＮＣ、メモリ共通）
			//txtd.OutLine_Add.Add("D00 Q00 P01 3" + "      ;Version");
			txtd.OutLine.AtoAdd("D00 Q1710 P01 4" + "      ;Version");
			txtd.OutLine.AtoAdd("D00 Q1740 P01 " + TolInfo.Toolset.ID + "   ;FTN NUMBER");
			txtd.OutLine.AtoAdd("D00 Q1741 P01 " + Index.tno.ToString("000") + "    ;TOOL NO.");
			txtd.OutLine.AtoAdd("D00 Q1742 P01 " + Index.type.ToString("00") + "     ;TOOL TYPE");
			txtd.OutLine.AtoAdd("D00 Q1743 P01 " + TolInfo.Min_length.ToString("000.00") + " ;TOTAL LENGTH");
			txtd.OutLine.AtoAdd("D00 Q1744 P01 " + TolInfo.Toolset.Ex_diam_Index.ToString("00.00") + "  ;TOOL DIAMETER");
			txtd.OutLine.AtoAdd("D00 Q1745 P01 " + TolInfo.Toolset.Crad.ToString("00.00") + "  ;TOOL CORNER RADIUS");
			txtd.OutLine.AtoAdd("D00 Q1756 P01 " + Index.next.ToString("000") + "    ;NEXT TOOL NO.");
			if (TolInfo.Toolset.M0304 != "M04")
				txtd.OutLine.AtoAdd("D00 Q1764 P01 " + Index.spin.ToString("00000") + "  ;SPIN");
			else
				txtd.OutLine.AtoAdd("D00 Q1764 P01 " + Index.spin.ToString("-00000") + "  ;SPIN");
			txtd.OutLine.AtoAdd("D00 Q1766 P01 " + Index.toth.ToString("0") + "      ;");
			txtd.OutLine.AtoAdd("D00 Q1767 P01 " + Index.refpoint.ToString("0") + "      ;NC REFERENCE POINT");
			txtd.OutLine.AtoAdd("D00 Q1768 P01 " + Index.coordinate.ToString("0") + "      ;COODINATE MODE(2:FRONT 3:REAR)");
			txtd.OutLine.AtoAdd("D00 Q1769 P01 " + IndexMain.progress + "      ;DANDORI CHECK(1:OMOTE 2:URA)");
			txtd.OutLine.AtoAdd("D00 Q1746 P01 " + Index.sgi1.ToString("0") + "      ;SGI1");
			txtd.OutLine.AtoAdd("D00 Q1747 P01 " + Index.sgi2.ToString("0") + "      ;SGI2");
			// ＴＥＸＡＳはRATEを整数として受け取るので整数に丸める
			//txtd.OutLine_Add.Add("D00 Q65 P01 " + joken.index.life_rate.ToString("000.00") + " ;RATE");
			txtd.OutLine.AtoAdd("D00 Q1765 P01 " + Index.Life_rate.ToString("000") + "    ;RATE");
			txtd.OutLine.AtoAdd("D00 Q1761 P01 " + Index.atime.ToString("000") + "    ;Time");
			txtd.OutLine.AtoAdd("D00 Q1762 P01 " + Index.coolant.ToString("0") + "      ;Coolant");
			txtd.OutLine.AtoAdd("D00 Q1763 P01 " + Index.dimensionAXIS.ToString("0") + "      ;Dimension");

			// TOOL_INFO.TAB に入力されるデータ（ＤＮＣ、メモリ共通）
			txtd.OutLine.AtoAdd("D00 Q1748 P01 " + TolInfo.Toolset.TolLmax.ToString("00.00") + "  ;Lmax");
			txtd.OutLine.AtoAdd("D00 Q1749 P01 " + TolInfo.Toolset.TolDmax.ToString("0.00") + "   ;Dmax");
			txtd.OutLine.AtoAdd("D00 Q1750 P01 " + TolInfo.Toolset.TolDmin.ToString("0.00") + "   ;Dmin");
			txtd.OutLine.AtoAdd("D00 Q1751 P01 " + TolInfo.TsetCAM.Tol_L_After.ToString("0.00") + "   ;qL_after");
			txtd.OutLine.AtoAdd("D00 Q1752 P01 " + TolInfo.TsetCAM.Tol_D_After.ToString("0.00") + "   ;qD_after");
			txtd.OutLine.AtoAdd("D00 Q1753 P01 " + TolInfo.Min_length.ToString("000.00") + " ;MIN_LENGTH");
			txtd.OutLine.AtoAdd("D00 Q1754 P01 " + Index.clearance_plane.ToString("000.00") + " ;CLEARANCE_PLANE");// ２次元加工のＺの逃げの位置に使用
			txtd.OutLine.AtoAdd("D00 Q1755 P01 " + (Index.hosei_r ?? (double)0.0).ToString("00.000") + " ;RADIUS_HOSEI");
			txtd.OutLine.AtoAdd("D00 Q1757 P01 " + (Index.hosei_l ?? (double)0.0).ToString("00.000") + " ;LENGTH_HOSEI");
			txtd.OutLine.AtoAdd("D00 Q1759 P01 " + Index.simultaneous.ToString("0") + " ;DOJI5JIKU");

			txtd.OutLine.AtoAdd("% TNC:\\TG\\PROG\\JOKEN.H");
			//if (dnc == true)
			//	txtd.outLine.atoAdd("% TNC:\\TG\\PROG\\G100_TOOLMEAS.H");
			//else
			txtd.OutLine.AtoAdd("% TNC:\\TG\\PROG\\G100.H");
		}

		/// <summary>
		/// 傾斜角度の設定と２次元加工での直交軸移動の設定
		/// </summary>
		/// <param name="txtd"></param>
		private void Fn_Rot(NcLineCode txtd) {

			// 
			//rotSP = new NcTejun.NcRead.Rot_Axis(30.0, 30.0, 0.0);
			//rotSP = new RotationAxis(RotationAxis.JIKU.Euler_XYZ, joken.skog.tld.nsgt1k.ax);
			keishaNo++;
			// ＣＡＭで設定された回転
			rotSP = new RotationAxis(NcoutName.Skog.Tld.XmlT[keishaNo].MachiningAxisAngle);

			// ＮＣとＸＭＬの回転角度比較
			if (!txtd.B_g100) {
				Angle3 ncd_ang, csv_ang;
				if (ncname.Ncdata.ncInfo.xmlD.SmNAM != null) {
					if (ncname.Ncdata.ncInfo.xmlD.CamDimension == 2)
						csv_ang = rotSP.Euler_ZXZ();
					else
						csv_ang = rotSP.DMU_BC();
				}
				else
					csv_ang = rotSP.Euler_XYZ();
				ncd_ang = new Angle3(csv_ang.Jiku, txtd.NcLine.Replace(ClLink5Axis.ChgAX, "").Replace(";", ""));
				if (!Angle3.MachineEquals(csv_ang, ncd_ang, 3))
					throw new Exception(
						"ポストのエラー：ＮＣデータとＣＳＶの軸回転コードが同一ではない。" +
						csv_ang.ToString() + " , " + ncd_ang.ToString());
			}

			// 傾斜でない場合は何もしない
			// 11 の時は必要だ！！（2013/06/17 衝突）
			//if (!rotSP.rot) return;

			switch (Index.coordinate) {
			case 11:
				break;
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
				return;
			case 0:
			case 1:
			default:
				throw new Exception("");
			}

			// 計測の場合は加工前の動きは不要
			if (Keisoku) return;

			Angle3 ax;
			if (Index.dimensionAXIS == 3) {
				//% TNC:\TG\PROG\G100.H
				//D00 Q02 P01 +28.432 
				//D00 Q03 P01 -100.156 
				//% TNC:\TG\PROG\BC_AXIS_SET.H
				//G00 G90 X-450.816 Y+95.649
				//Z+311.418
				//G00 Y+135.32 Z+202.423 M+Q1702
				//G01 Y+137.03 Z+197.725 F1080
				//G01X-450.651 Y+137.262 Z+197.797 F2160
				ax = rotSP.DMU_BC();
				txtd.OutLine.AtoAdd("D00 Q02 P01 " + ax.DegB.ToString("0.0##"));
				txtd.OutLine.AtoAdd("D00 Q03 P01 " + ax.DegC.ToString("0.0##"));
				txtd.OutLine.AtoAdd("% TNC:\\TG\\PROG\\BC_AXIS_SET.H");
			}
			else {
				//D00 Q01 P01 +12. 
				//D00 Q02 P01 +0. 
				//D00 Q03 P01 -90. 
				//% TNC:\TG\PROG\BC_AXIS_SET.H
				//G00 G90 X+488.752 Y+388. M130
				//Z+128.935 M130
				//G00 X-388. Y+504.879 Z+24.5
				ax = rotSP.SPATIAL();
				txtd.OutLine.AtoAdd("D00 Q01 P01 " + ax.DegA.ToString("0.0##"));
				txtd.OutLine.AtoAdd("D00 Q02 P01 " + ax.DegB.ToString("0.0##"));
				txtd.OutLine.AtoAdd("D00 Q03 P01 " + ax.DegC.ToString("0.0##"));
				txtd.OutLine.AtoAdd("% TNC:\\TG\\PROG\\BC_AXIS_SET.H");
				// 直交軸の移動
				if (rotSP.Rot) Choko(txtd);
			}
			return;
		}
		/// <summary>
		/// 直交軸の移動
		/// </summary>
		/// <param name="txtd"></param>
		private void Choko(NcLineCode txtd) {
			if (Index.dimensionAXIS != 2) throw new Exception("awefqwerfhb");
			NcLineCode next = this.NcQue.NcPeek(+1);
			if (next.NcLine == "G00G90X0Y0" || next.NcLine == "G00G90X0Y0;")
				next = this.NcQue.NcPeek(+2);

			// 直行軸のＸＹ動き（M130）
			Vector3 stPos_next = next.Xyzsf.ToXYZ();
			if (next.B_26('X') == false || next.B_26('Y') == false || next.B_26('Z') == true)
				throw new Exception("aegbaegsvgshvsrvf");
			Vector3 stPos_work = new Vector3(stPos_next.X, stPos_next.Y, NcoutName.Skog.Tld.XmlT[keishaNo].ClearancePlane);
			Vector3 stPos_model = rotSP.Transform(RotationAxis.TRANSFORM.FeatureToWork, stPos_work);
			txtd.OutLine.AtoAdd("G00 G90" +
				" X" + stPos_model.X.ToString("0.0##") +
				" Y" + stPos_model.Y.ToString("0.0##") + " M130");
			txtd.OutLine.AtoAdd(
				"Z" + stPos_model.Z.ToString("0.0##") + " M130");

			// 工具が垂直に近い場合（３０度以上）のみメッセージを出力するように変更（0.001 → 0.500）
			if (stPos_model.Z < 20.0 && Math.Abs(rotSP.ToolDir().Z) > 0.500)
				MessageBox.Show(
					$"ＮＣデータ：{ncname.Nnam} 工具：{TolInfo.Toolset.tset_name}でアプローチ点と金型上面の距離が２０mm以下です。\n" +
					$"距離 = {stPos_model.Z:f1} in X = {stPos_work.X.ToString()}   Y = {stPos_work.Y.ToString()} Z = {stPos_work.Z.ToString()}",
					"金型上面との距離確認", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
		}

		// プログラム名と単位（％）
		private void Fn_dmg_name(NcLineCode txtd, string outname) {
			if (txtd.LnumN == 1)
				txtd.OutLine.Set("% " + outname + " G71");
			else
				txtd.OutLine.Set("");
		}

		// 工具径補正のチェック
		protected void Fn_diam_offset() {
			if (Index.dimensionAXIS != 2 && Index.type == 6) {
				//MessageBox.Show("検討用ＮＣデータを作成するため、一時的にチェックを外す 2011/12/20");
				throw new Exception(
					$"{ncname.Nnam}の仕上げ用工具({NcoutName.tolInfo.Toolset.ToolName})はＤＭＧの３次元加工モードでは使用できません。NCMNにおいて'2D'の指定をして使用してください in NcConv_DMG");
			}
		}
		// 円弧補間の次元チェック
		protected void Fn_cir_chk() {
			if (Index.dimensionAXIS != 2) {
				throw new Exception(
					$"{ncname.Nnam}においてＤＭＧの３次元加工モードにかかわらず円弧補間が使用されました。円弧補間を使用する場合は２次元加工モードに変更（NCMNにおいて'2D'とする）してください in NcConv_DMG");
			}
		}
		// ＤＭＧ仕様への変換（G02,G03のI,Jコード変換）
		private void Fn_dmg_g02(NcLineQue txtd) {
			if (txtd.B_26('I') || txtd.B_26('J')) {

				Vector3 cntr = txtd.Xyzsf.PreToXYZ() + txtd.IJK;
				if (txtd.G2 != 17) throw new Exception("qjefrbqbfr");
				// 半径のコメント追加
				txtd.OutLine.Addcomment("R=" + txtd.IJK.ToVector2(17).Abs.ToString("0.000"));
				// 省略されたＩＪコードを追加
				if (txtd.B_26('I') == false) txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'J', "I0", true));
				if (txtd.B_26('J') == false) txtd.OutLine.Set(NcLineCode.NcInsertChar(txtd.OutLine.Txt, 'I', "J0", false));
				// ＩＪコードを絶対値化
				txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'I', cntr.X.ToString("0.0###")));
				txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'J', cntr.Y.ToString("0.0###")));

				// Ｇ０２、Ｇ０３の入力（円弧補間Ｒ指定後に必要なためすべて挿入）
				txtd.AddG123();

			}
			else if (txtd.B_26('R')) {
				// Ｇ０２、Ｇ０３の入力（円弧補間ＩＪ指定後にたぶん必要なため）
				txtd.AddG123();
			}
			else
				throw new Exception("円弧補間エラー LINE=" + txtd.OutLine.Txt + " in NcConv_DMG");
		}

		// クーラントＯＮＯＦＦの挿入（G01の直前）
		protected virtual void Fn_clnt(NcLineQue txtd) {
			if (txtd.LnumT == 4) {
				if (TolInfo.Toolset.ToolName == "AIRBLOW")	// AIRBLOW COOLANT OFF
					return;
				txtd.OutLine.MaeInsert("M+Q1702 ; COOLANT");
			}
			if (txtd.Lnumb == -4) {
				if (txtd.G8 == 80 && txtd.G6 == 67 && txtd.G1 == 0)
					txtd.OutLine.MaeAdd("M09");
				else
					txtd.OutLine.AtoAdd("M09");
			}
		}

		// 孔加工のサイクル定義とサイクルの実行
		private void Fn_cycle(NcLineCode txtd) {
			bool def = false;
			double baseZ = 0.0;

			if (txtd.G8 != 80) {
				if (txtd.B_g8) {
					def = true;
					if (txtd.G8p['L'].Set) {
						if (txtd.G8p['L'].L != 0)
							throw new Exception("固定サイクルの'L'は'L0'以外未対応です");
					}
				}
				else {
					if (txtd.G8p['Z'].Set) if (txtd.G8p['Z'].B) def = true;
					if (txtd.G8p['P'].Set) if (txtd.G8p['P'].B) def = true;
					if (txtd.G8p['Q'].Set) if (txtd.G8p['Q'].B) def = true;
					if (txtd.G8p['R'].Set) if (txtd.G8p['R'].B) def = true;
					if (txtd.G8p['I'].Set) if (txtd.G8p['I'].B) def = true;
					if (txtd.G8p['J'].Set) if (txtd.G8p['J'].B) def = true;
					if (txtd.G8p['F'].Set) if (txtd.G8p['F'].B) def = true;
					if (txtd.G8p['L'].Set) if (txtd.G8p['L'].B) {
						// 現在のGENERALの仕様にはないのでエラーとし、以下は未対応である
						throw new Exception("固定サイクルの'L'はサイクル設定行以外未対応です");
					}
				}
				// ///////////////////////
				// ＮＣデータのサイクル設定
				// ///////////////////////
				if (def) {
					// /////////////////////////////////////
					// 前のNcReadと比較するための暫定処置
					// /////////////////////////////////////
					//txtd.outLine.maeAdd("; " + txtd.ncLine);
					txtd.OutLine.MaeAdd("; " + txtd.NcLine + ";");
					// /////////////////////////////////////
					if (txtd.Xyzsf.Fi != txtd.G8p['F'].L) {
						throw new Exception("awedfawedf");
					}
					string[] stmp = CycleDef(ref baseZ, txtd.G8p, Index.clearance_plane);
					//txtd.OutLine_Ins.Add("D02 Q99 P01 Q1703 P02 " + sign_num(baseZ));
					txtd.OutLine.MaeAdd(stmp[0]);
					txtd.OutLine.MaeAdd(stmp[1]);
				}
				// ////////////////
				// 孔加工位置の出力
				// ////////////////
				if (txtd.B_26('X') || txtd.B_26('Y')) {
					if (txtd.G8p['L'].Set && txtd.G8p['L'].B && txtd.G8p['L'].L == 0)
						txtd.OutLine.Set("");
					else {
						txtd.OutLine.Set(
							"X" + Sign_num(txtd.Xyzsf.X) +
							"Y" + Sign_num(txtd.Xyzsf.Y) +
							"M99");
					}
				}
			}
			else if (txtd.G6 == 66 || txtd.G6 == 65) {
				string progNo = txtd.G6p.ProgNo.ToString("0000");

				// W1.0 によりアップカット、ダウンカットを逆にするマクロ
				// ただし実際に使用しているのは8401,8402の２つのみである
				// ８０１３などの周り止めはW1.0がなくアップカットになるようだがよいのか？
				if (CamUtil.CamNcD.MacroCode.MacroIndex(progNo) >= 0) {
					// ＝＝＝＝Ｘ軸のミラー＝＝＝＝
					// Ｘ座標値の＋－を反転
					// Ｇ４１、Ｇ４２を入れ替え
					// Ｇ０２、Ｇ０３を入れ替え
					// 円弧中心Ｉ座標値の＋－反転（Ｒはそのまま）
					// カスタムマクロの W0.0、W1.0 を入れ替え（W0.0 は省略可能）
					//
					if (txtd.G6p['W'].Set)
						throw new Exception("アップカット指定W1.0はDMGでは対応していません");
				}
				else {
					if (txtd.G6p['W'].Set)
						throw new Exception("P" + progNo + " にはアップカット指示 W1.0は使用できません in NcConv_DMG");
				}
				switch (progNo) {
				// 孔加工パターン
				case "8900":	// 多段孔加工（１段目）
				case "8700":	// 多段孔加工（２段目以降）
				case "8200":	// 深孔ドリル加工（２段目以降）(G83)
				case "8400":	// リジッドタップ
				case "8402":	// ヘリカル並目タップ
					//case "8030":	// 円内粗仕上げ
					if (txtd.B_g6) {
						// /////////////////////////////////////
						// 前のNcReadと比較するための暫定処置
						// /////////////////////////////////////
						//txtd.outLine.maeAdd("; " + txtd.ncLine);
						txtd.OutLine.MaeAdd("; " + txtd.NcLine + ";");
						string[] stmp = MacroDef(txtd, ref baseZ, progNo, txtd.G6p, Index.clearance_plane);
						txtd.OutLine.MaeAdd(stmp[0]);
						txtd.OutLine.Set(stmp[1]);
						//txtd.OutLine_Ins.Add("D02 Q99 P01 Q1703 P02 " + sign_num(baseZ));
					}
					else if (txtd.B_26('X') || txtd.B_26('Y')) {
						txtd.OutLine.Set(
							"X" + Sign_num(txtd.Xyzsf.X) +
							"Y" + Sign_num(txtd.Xyzsf.Y) +
							"M99");
						// サブコール後、なぜかG01モードになることへの対応
						// こちらは本当に必要か、要チェック
						txtd.OutLine.AtoAdd("G00G90");
					}
					else {
						throw new Exception("qqqqq1");
					}
					break;

				// 特殊加工パターン
				case "8401":	// ヘリカルＲＣタップ
				case "8010":	// 周り止め
				case "8013":	// 周り止め
				case "8015":	// 周り止め
				case "8011":	// 周り止め
				case "8014":    // 周り止め
				case "8016":    // 周り止め add in 2019/06/12
				case "8019":    // 周り止め add in 2019/06/12
				//case "8046":	// エアブロー
				//case "8280":	// ガンドリル
				case "8290":	// 超鋼ドリル
					if (txtd.B_g6) {
						// /////////////////////////////////////
						// 前のNcReadと比較するための暫定処置
						// /////////////////////////////////////
						//txtd.outLine.maeAdd("; " + txtd.ncLine);
						txtd.OutLine.MaeAdd("; " + txtd.NcLine + ";");
						string[] stmp = MacroDef(txtd, ref baseZ, progNo, txtd.G6p, Index.clearance_plane);
						txtd.OutLine.MaeAdd(stmp[0]);
						txtd.OutLine.Set(stmp[1]);
						//txtd.OutLine_Ins.Add("D02 Q99 P01 Q1703 P02 " + sign_num(baseZ));
					}
					else if (txtd.B_26('X') || txtd.B_26('Y')) {
						txtd.OutLine.Set(
							"G00" +
							"X" + Sign_num(txtd.Xyzsf.X) +
							"Y" + Sign_num(txtd.Xyzsf.Y));
						switch (progNo) {
						case "8401":	// ヘリカルＲＣタップ
							txtd.OutLine.AtoAdd("% TNC:\\TG\\PROG\\MAC_HELICALTAP.H");
							break;
						case "8010":	// 周り止め
						case "8013":	// 周り止め
						case "8015":	// 周り止め
						case "8011":	// 周り止め
						case "8014":    // 周り止め
						case "8016":    // 周り止め add in 2019/06/12
						case "8019":    // 周り止め add in 2019/06/12
							txtd.OutLine.AtoAdd("% TNC:\\TG\\PROG\\MAC_MAWARIDOME.H");
							break;
						//case "8046":	// エアブロー
						//	txtd.outLine.atoAdd("% TNC:\\TG\\PROG\\MAC_AIRBLOW.H");
						//	break;
						//case "8280":	// ガンドリル
						//	txtd.outLine.atoAdd("% TNC:\\TG\\PROG\\MAC_GUNDRILL.H");
						//	break;
						case "8290":	// 超鋼ドリル
							txtd.OutLine.AtoAdd("% TNC:\\TG\\PROG\\MAC_LONGDRILL.H");
							break;
						}
						// サブコール後、なぜかG01モードになることへの対応
						txtd.OutLine.AtoAdd("G00G90");
					}
					else {
						throw new Exception("qqqqq2");
					}
					break;

				// 個別分類パターン
				// 工具測定区分が"F"以外を前提に使用する
				case "8025":	// スプルーロック
					//case "8020":	// 円内仕上げ（ケーラムのみ）
					if (txtd.B_g6) {
						// /////////////////////////////////////
						// 前のNcReadと比較するための暫定処置
						// /////////////////////////////////////
						//txtd.outLine.maeAdd("; " + txtd.ncLine);
						txtd.OutLine.MaeAdd("; " + txtd.NcLine + ";");
						txtd.OutLine.Set("");
					}
					else if (txtd.B_26('X') || txtd.B_26('Y')) {
						string[] stmp = MacroDef(ref baseZ, progNo, txtd.G6p, Index.clearance_plane, txtd.Xyzsf);
						//txtd.OutLine_Ins.Add("D02 Q99 P01 Q1703 P02 " + sign_num(baseZ));
						txtd.OutLine.MaeAdd(stmp[0]);
						txtd.OutLine.MaeAdd(stmp[1]);
						txtd.OutLine.Set(
							"X" + Sign_num(txtd.Xyzsf.X) +
							"Y" + Sign_num(txtd.Xyzsf.Y) +
							"M99");
					}
					else {
						throw new Exception("qqqqq3");
					}
					break;
				//
				//　形状測定
				case "8755":
					Vector3 Wxyz = new Vector3(txtd.G6p['X'].D, txtd.G6p['Y'].D, txtd.G6p['Z'].D);	// 測定面上点（ワーク）
					Vector3 Wmen = new Vector3(txtd.G6p['I'].D, txtd.G6p['J'].D, txtd.G6p['K'].D);	//面法線ベクトル（ワーク）
					Vector3 Wcnr = Wxyz + Wmen.Unit(TolInfo.Toolset.Diam / 2.0);	// 測定時プローブ中心（ワーク）
					Vector3 Mcnr = rotSP.Transform(RotationAxis.TRANSFORM.FeatureToWork, Wcnr);	// 測定時プローブ中心（モデル）
					Vector3 Mmen = rotSP.Transform(RotationAxis.TRANSFORM.FeatureToWork, Wmen);	// 面法線ベクトル（モデル）
					int mtyp;
					switch ((int)txtd.G6p['Q'].D) {
					case 0:	// ポイント
					case 1:	// ポケットの幅
					case 2:	// ボスの厚さ
						mtyp = (int)txtd.G6p['Q'].D;
						break;
					case 5:	// 穴径
						mtyp = 3;
						break;
					case 3:
					case 4:
					default:
						throw new Exception("測定種類=" + ((int)txtd.G6p['Q'].D).ToString() + "はDMU200Pでは対応していない測定方法です。");
					}
					txtd.OutLine.MaeAdd("D00 Q01 P01 " + this.rotSP.SPATIAL().DegA.ToString("0.000#"));
					txtd.OutLine.MaeAdd("D00 Q02 P01 " + this.rotSP.SPATIAL().DegB.ToString("0.000#"));
					txtd.OutLine.MaeAdd("D00 Q03 P01 " + this.rotSP.SPATIAL().DegC.ToString("0.000#"));
					txtd.OutLine.MaeAdd("D00 Q24 P01 " + Mcnr.X.ToString("0.0##"));
					txtd.OutLine.MaeAdd("D00 Q25 P01 " + Mcnr.Y.ToString("0.0##"));
					txtd.OutLine.MaeAdd("D00 Q26 P01 " + Mcnr.Z.ToString("0.0##"));
					txtd.OutLine.MaeAdd("D00 Q09 P01 " + Mmen.Unit().X.ToString("0.0###"));
					txtd.OutLine.MaeAdd("D00 Q10 P01 " + Mmen.Unit().Y.ToString("0.0###"));
					txtd.OutLine.MaeAdd("D00 Q11 P01 " + Mmen.Unit().Z.ToString("0.0###"));
					txtd.OutLine.MaeAdd("D00 Q12 P01 " + txtd.G6p['S'].D.ToString("0"));
					txtd.OutLine.MaeAdd("D00 Q13 P01 " + txtd.G6p['R'].D.ToString("0.0##"));
					txtd.OutLine.MaeAdd("D00 Q14 P01 " + mtyp.ToString());
					txtd.OutLine.Set("% TNC:\\TG\\PROG\\MeasuringPoint.H");
					break;
				//
				// 8020,8030はハイデンハインのサイクルを使用できるが
				// 仕上げ用工具をフライスに使用するために、工具径を基本０と
				// している関係で、工具径を参照して自動的にオフセットする
				// これらのサイクルの使用は中止した。2007/09 藤本
				//
				//case "8020":	// 円内仕上げ（マクロ展開する）
				//case "8025":	// スプルーロック（マクロ展開する）
				//case "8030":	// 円内粗仕上げ（マクロ展開する）
				case "8080":	// 面取り（マクロ展開する）
				case "8082":	// 面取り（マクロ展開する）
				case "8085":	// 面取り（マクロ展開する）
				default:
					throw new Exception(
						"未登録のカスタムマクロ P" + progNo + " が使用された in NcConv_DMG");
				}
			}
			else {
				throw new Exception("固定サイクルorマクロにおけるプログラムエラー in NcConv_DMG");
			}
		}

		private string Sign_num(double d) {
			string txt = d.ToString("0.###");
			if (txt[0] != '-')
				txt = "+" + txt;
			return txt;
		}
	}
}
