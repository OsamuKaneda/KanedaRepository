using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	partial class Conv_DMG : Conv
	{
		private string[] CycleDef(ref double baseZ, NcLineCode.CycleMode g8p2, double clrp) {
			if (g8p2['R'].Set == false || g8p2['Z'].Set == false)
				throw new Exception("固定サイクルエラー in cycleDef");

			string[] stmp = new string[2];
			// ;DWELL TIME AT DEPTH
			double Q211 = g8p2['P'].Set ? g8p2['P'].D / 1000.0 : 0.0;
			double fukasa = g8p2['R'].D - g8p2['Z'].D;
			double r_ten = g8p2['R'].D;
			baseZ = r_ten;
			if (clrp - r_ten < 0) throw new Exception("クリアランスプレーン設定エラー in cycleDef");

			switch (g8p2.progNo) {
			case 73:
				// Ｇ２０３はＧ７３以外（Ｇ８３など）には使用しないこと。Ｇ２０５を使用
				// セットアップが０のため、Ｒ点移動後の再位置決めクリアランス０となる
				stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(r_ten);
				stmp[1] = "G203"
					+ " Q200=" + "0"						// ;SET-UP CLEARANCE
					+ " Q201=" + Sign_num(-fukasa)			// ;DEPTH
					+ " Q206=" + g8p2['F'].L				// ;FEED RATE FOR PLNGNG
					+ " Q202=" + Sign_num(g8p2['Q'].D)		// ;PLUNGING DEPTH
					+ " Q210=" + "0"
					+ " Q203=" + Sign_num(r_ten)			// ;SURFACE COORDINATE
					+ " Q204=Q204"							// ;2ND SET-UP CLEARANCE
					+ " Q212=" + "0"						// ;DECREMENT
					+ " Q213=" + Sign_num(1 + Math.Floor(fukasa / g8p2['Q'].D))
					+ " Q205=" + "0"						// ;MIN. PLUNGING DEPTH
					+ " Q211=" + Q211						// ;DWELL TIME AT DEPTH
					+ " Q208=" + "1000"						// ;RETRACTION FEED RATE
					+ " Q256=" + "0.1";						// ;DIST FOR CHIP BRKNG
				break;
			case 81:
			case 82:
				stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(r_ten);
				stmp[1] = "G201"
					+ " Q200=" + "0"						// ;SET-UP CLEARANCE
					+ " Q201=" + Sign_num(-fukasa)			// ;DEPTH
					+ " Q206=" + g8p2['F'].L				// ;FEED RATE FOR PLNGNG
					+ " Q211=" + Q211						// ;DWELL TIME AT DEPTH
					+ " Q208=" + "5000"						// ;RETRACTION FEED RATE
					+ " Q203=" + Sign_num(r_ten)			// ;SURFACE COORDINATE
					+ " Q204=Q204";						// ;2ND SET-UP CLEARANCE
				break;
			case 83:
				stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(r_ten);
				stmp[1] = "G205"
					+ " Q200=" + "0"						// ;SET-UP CLEARANCE
					+ " Q201=" + Sign_num(-fukasa)			// ;DEPTH
					+ " Q206=" + g8p2['F'].L				// ;FEED RATE FOR PLNGNG
					+ " Q202=" + Sign_num(g8p2['Q'].D)		// ;PLUNGING DEPTH
					+ " Q203=" + Sign_num(r_ten)			// ;SURFACE COORDINATE
					+ " Q204=Q204"							// ;2ND SET-UP CLEARANCE
					+ " Q212=" + "0"						// ;DECREMENT
					+ " Q205=" + "0"						// ;MIN. PLUNGING DEPTH
					+ " Q258=" + "0.5"						// ;UPPER ADV STOP DIST
					+ " Q259=" + "1.0"						// ;LOWER ADV STOP DIST
					+ " Q257=" + "0"						// ;DEPTH FOR CHIP BRKNG
					+ " Q256=" + "0.1"						// ;DIST FOR CHIP BRKNG
					+ " Q211=" + Q211						// ;DWELL TIME AT DEPTH
					+ " Q379=" + "0"						// ;STARTING POINT
					+ " Q253=" + "200";                     // ;F PRE-POSITIONING
				if (mach.ID == CamUtil.Machine.MachID.DMU210P || mach.ID == CamUtil.Machine.MachID.DMU210P2) {
					int retfeed = (int)Math.Round(mach.ncCode.RetractFeed * NcoutName.Skog.retract_rate);
					stmp[1] += $" Q208=+{retfeed:d} Q395=+0";           // ;RETRACTION FEED RATE, ;DEPTH REFERENCE
				}
				break;
			case 84:
				stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(r_ten);
				stmp[1] = "G206"
					+ " Q200=" + "0"						// ;SET-UP CLEARANCE
					+ " Q201=" + Sign_num(-fukasa)			// ;DEPTH
					+ " Q206=" + g8p2['F'].L				// ;FEED RATE FOR PLNGNG
					+ " Q211=" + Q211						// ;DWELL TIME AT DEPTH
					+ " Q203=" + Sign_num(r_ten)			// ;SURFACE COORDINATE
					+ " Q204=Q204";							// ;2ND SET-UP CLEARANCE
				break;
			case 86:	// ボーリング
				stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(r_ten);
				stmp[1] = "G202"
					+ " Q200=" + "0"						// ;SET-UP CLEARANCE
					+ " Q201=" + Sign_num(-fukasa)			// ;DEPTH
					+ " Q206=" + g8p2['F'].L				// ;FEED RATE FOR PLNGNG
					+ " Q211=" + Q211						// ;DWELL TIME AT DEPTH
					+ " Q208=" + "5000"						// ;RETRACTION FEED RATE
					+ " Q203=" + Sign_num(r_ten)			// ;SURFACE COORDINATE
					+ " Q204=Q204"							// ;2ND SET-UP CLEARANCE
					+ " Q214=" + "0"
					+ " Q336=" + "0";
				break;
			default:
				Exception aa = new Exception("G81");
				break;
			}
			if (fukasa <= 0.0) {
				throw new Exception("マクロ深さエラー");
			}
			return stmp;
		}
		private string[] MacroDef(NcLineCode txtd, ref double baseZ, string progNo, NcLineCode.MacroMode g6p, double clrp) {
			string[] stmp = new string[2];
			double fukasa;

			switch (progNo) {
			case "8900":  // 多段孔加工（１段目）
				if (g6p['Z'].Set == false || g6p['Q'].Set == false || g6p['R'].Set == false || g6p['F'].Set == false)
					throw new Exception("マクロエラー in NcConv_DMG_cycle");

				{
					double r_ten;
					double plunge;
					double chipBr;
					fukasa = g6p['R'].D - g6p['Z'].D;
					r_ten = g6p['R'].D;
					plunge = g6p['Q'].D * 3.0;
					chipBr = g6p['Q'].D * 1.01;
					baseZ = r_ten;
					if (clrp - r_ten < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");

					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(r_ten);
					stmp[1] = "G205"	// UNIVERSAL PECKING
						+ " Q200=" + "0"						// ;SET-UP CLEARANCE
						+ " Q201=" + Sign_num(-fukasa)			// ;DEPTH
						+ " Q206=" + g6p['F'].D			// ;FEED RATE FOR PLNGNG
						+ " Q202=" + plunge						// ;PLUNGING DEPTH
						+ " Q203=" + Sign_num(r_ten)			// ;SURFACE COORDINATE
						+ " Q204=Q204"							// ;2ND SET-UP CLEARANCE
						+ " Q212=" + "0"						// ;DECREMENT
						+ " Q205=" + "0"						// ;MIN. PLUNGING DEPTH
						+ " Q258=" + "1.0"						// ;UPPER ADV STOP DIST
						+ " Q259=" + "1.0"						// ;LOWER ADV STOP DIST
						+ " Q257=" + chipBr						// ;DEPTH FOR CHIP BRKNG
						+ " Q256=" + "0.1"						// ;DIST FOR CHIP BRKNG
						+ " Q211=" + "1"						// ;DWELL TIME AT DEPTH
						+ " Q379=" + "0"						// ;STARTING POINT
						+ " Q253=" + "200";                     // ;F PRE-POSITIONING
					if (mach.ID == CamUtil.Machine.MachID.DMU210P || mach.ID == CamUtil.Machine.MachID.DMU210P2) {
						int retfeed = (int)Math.Round(mach.ncCode.RetractFeed * NcoutName.Skog.retract_rate);
						stmp[1] += $" Q208=+{retfeed:d} Q395=+0";           // ;RETRACTION FEED RATE, ;DEPTH REFERENCE
					}
				}
				break;
			case "8700":  // 多段孔加工（２段目以降）
			case "8200":  // 深孔ドリル加工（２段目以降）(G83)
				if (g6p['Z'].Set == false || g6p['Q'].Set == false || g6p['K'].Set == false || g6p['R'].Set == false || g6p['F'].Set == false)
					throw new Exception("マクロエラー");

				{
					double r_ten;
					double k_fukasa;
					double plunge;
					double chipBr;
					fukasa = g6p['R'].D - g6p['Z'].D;
					k_fukasa = g6p['R'].D - g6p['K'].D;
					r_ten = g6p['R'].D;
					if (progNo == "8700") {
						plunge = g6p['Q'].D * 3.0;
						chipBr = g6p['Q'].D * 1.01;
					}
					else {
						plunge = g6p['Q'].D;
						chipBr = 0;
					}
					baseZ = r_ten;
					if (clrp - r_ten < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");

					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(r_ten);
					stmp[1] = "G205"			// UNIVERSAL PECKING
						+ " Q200=" + "0"					// ;SET-UP CLEARANCE
						+ " Q201=" + Sign_num(-fukasa)		// ;DEPTH
						+ " Q206=" + g6p['F'].D				// ;FEED RATE FOR PLNGNG
						+ " Q202=" + plunge					// ;PLUNGING DEPTH
						+ " Q203=" + Sign_num(r_ten)		// ;SURFACE COORDINATE
						+ " Q204=Q204"						// ;2ND SET-UP CLEARANCE
						+ " Q212=" + "0"					// ;DECREMENT
						+ " Q205=" + "0"					// ;MIN. PLUNGING DEPTH
						+ " Q258=" + "0.5"					// ;UPPER ADV STOP DIST
						+ " Q259=" + "1.0"					// ;LOWER ADV STOP DIST
						+ " Q257=" + chipBr					// ;DEPTH FOR CHIP BRKNG
						+ " Q256=" + "0.1"					// ;DIST FOR CHIP BRKNG
						+ " Q211=" + "1"					// ;DWELL TIME AT DEPTH
						+ " Q379=" + k_fukasa				// ;STARTING POINT
						+ " Q253=" + "200";                 // ;F PRE-POSITIONING
					if (mach.ID == CamUtil.Machine.MachID.DMU210P || mach.ID == CamUtil.Machine.MachID.DMU210P2) {
						int retfeed = (int)Math.Round(mach.ncCode.RetractFeed * NcoutName.Skog.retract_rate);
						stmp[1] += $" Q208=+{retfeed:d} Q395=+0";           // ;RETRACTION FEED RATE, ;DEPTH REFERENCE
					}
				}
				break;
			case "8400":  // リジッドタップ
				if (TolInfo.Toolset.MeasTypeDMG == 'F')
					throw new Exception(
						"工具計測タイプＦにはこのP8400サイクルは使用できません in macroDef");
				if (g6p['Z'].Set == false || g6p['R'].Set == false || g6p['F'].Set == false)
					throw new Exception("マクロエラー in macroDef");

				{
					double r_ten;
					fukasa = g6p['R'].D - g6p['Z'].D;
					r_ten = g6p['R'].D;
					double pitch = Math.Round(20.0 * g6p['F'].D / Index.spin) / 20.0;
					// 刃長のチェック。シャンクが逃がしてあるためチェック不要
					baseZ = r_ten;
					if (clrp - r_ten < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");

					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(r_ten);
					stmp[1] = "G207"
						+ " Q200=" + "0"						// ;SET-UP CLEARANCE
						+ " Q201=" + Sign_num(-fukasa)			// ;DEPTH
						+ " Q239=" + Sign_num(pitch)			// ;PITCH
						+ " Q203=" + Sign_num(r_ten)			// ;SURFACE COORDINATE
						+" Q204=Q204";							// ;2ND SET-UP CLEARANCE
				}
				break;
			case "8401":  // ヘリカルＲＣタップ
				if (TolInfo.Toolset.MeasTypeDMG == 'F')
					throw new Exception("工具計測タイプＦにはこのP8401サイクルは使用できません in macroDef");
				if (g6p['E'].Set == false || g6p['R'].Set == false || g6p['W'].Set == true)
					throw new Exception("マクロエラー");

				{
					double surfcoord;
					double kdiam, pitch, feed_m, setupc;
					double dhosei;

					DataRow dRow = CamUtil.CamNcD.KijunSize.HelicalTap(progNo, g6p['E'].D);
					kdiam = pitch = fukasa = setupc = feed_m = 0.0;
					dhosei = 0.0;
					kdiam = (double)dRow["27基準径"];
					dhosei = (double)dRow["基準径補正値"];
					pitch = (double)dRow["29ピッチ"];
					fukasa = (double)dRow["30基準孔深さ"];
					setupc = (double)dRow["R点高さ"];
					// マクロにＦコードがないためＮＣデータの条件をセットする2016/03/24
					feed_m = this.NcoutName.Skog.Tld.XmlT.FEEDR;

					if (fukasa < 1) {
						throw new Exception("wefbwreh");
					}
					// 刃長のチェック
					if (TolInfo.Toolset.Hacho != 0.0 && fukasa + pitch > TolInfo.Toolset.Hacho) {
						MessageBox.Show(
							$"工具：{TolInfo.Toolset.ToolName}(刃長：{TolInfo.Toolset.Hacho})(深さ：{((double)(fukasa + pitch)).ToString("0.0")}) で刃長が不足している可能性があります。",
							"G66P8401",
							MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					surfcoord = g6p['R'].D - setupc;
					if (clrp - surfcoord < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");

					baseZ = surfcoord;
					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(surfcoord);
					stmp[1] = "G262"
						// 工具径補正値は工具テーブルで設定するように変更
						// 工具径補正値はここで設定するように再度変更 20011/11/04
						+ " Q335=" + (kdiam + 2 * dhosei)		// ;NOMINAL DIAMETER
						//+ " Q335=" + kdiam						// ;NOMINAL DIAMETER
						+ " Q239=" + Sign_num(pitch)			// ;PITCH
						+ " Q201=" + Sign_num(-fukasa - pitch)	// ;DEPTH
						+ " Q355=" + "0"						// ;THREADS PER STEP
						+ " Q253=" + "1000"						// ;FEED FOR PRE-POSITIONING
						+ " Q351=" + "+1"						// ;CLIMB OR UP-CUT
						+ " Q200=" + setupc						// ;SET-UP CLEARANCE
						+ " Q203=" + Sign_num(surfcoord)		// ;SURFACE COORDINATE
						+ " Q204=Q204"							// ;2ND SET-UP CLEARANCE
						+ " Q207=" + feed_m;					// ;FEED FOR MILLING(切刃)
				}
				break;
			case "8402":  // ヘリカル並目/細目タップ
				if (TolInfo.Toolset.MeasTypeDMG == 'F')
					throw new Exception("工具計測タイプＦにはこのP8400サイクルは使用できません in macroDef");
				if (g6p['E'].Set == false || g6p['R'].Set == false || g6p['W'].Set == true)
					throw new Exception("マクロP8402のコードエラー" + txtd.NcLine + " in macroDef");

				{
					double surfcoord;
					double kdiam, pitch, feed_m, setupc;
					double dhosei;

					DataRow dRow = CamUtil.CamNcD.KijunSize.HelicalTap(progNo, g6p['E'].D);
					kdiam = pitch = fukasa = setupc = feed_m = 0.0;
					dhosei = 0.0;

					kdiam = (double)dRow["27基準径"];
					dhosei = (double)dRow["基準径補正値"];
					pitch = (double)dRow["29ピッチ"];
					fukasa = (double)dRow["30基準孔深さ"];
					setupc = (double)dRow["R点高さ"];
					// マクロにＦコードがないためＮＣデータの条件をセットする2016/03/24
					feed_m = this.NcoutName.Skog.Tld.XmlT.FEEDR;

					if (fukasa < 1)
						throw new Exception("wefbwreh");
					// 刃長のチェック
					if (TolInfo.Toolset.Hacho != 0.0 && fukasa + pitch > TolInfo.Toolset.Hacho) {
						MessageBox.Show(
							$"工具：{TolInfo.Toolset.ToolName}(刃長：{TolInfo.Toolset.Hacho})(深さ：{((double)(fukasa + pitch)).ToString("0.0")}) で刃長が不足している可能性があります。",
							"G66P8402",
							MessageBoxButtons.OK, MessageBoxIcon.Warning);
					}
					surfcoord = g6p['R'].D - setupc;
					baseZ = surfcoord;
					if (clrp - surfcoord < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");

					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(surfcoord);
					stmp[1] =  "G262"
						// 工具径補正値は工具テーブルで設定するように変更
						// 工具径補正値はここで設定するように再度変更 20011/11/04
						+ " Q335=" + (kdiam + 2 * dhosei)		// ;NOMINAL DIAMETER
						//+ " Q335=" + kdiam						// ;NOMINAL DIAMETER
						+ " Q239=" + Sign_num(pitch)			// ;PITCH
						+ " Q201=" + Sign_num(-fukasa - pitch)	// ;DEPTH
						+ " Q355=" + "0"						// ;THREADS PER STEP
						+ " Q253=" + "1000"						// ;FEED FOR PRE-POSITIONING
						+ " Q351=" + "+1"						// ;CLIMB OR UP-CUT
						+ " Q200=" + setupc						// ;SET-UP CLEARANCE
						+ " Q203=" + Sign_num(surfcoord)		// ;SURFACE COORDINATE
						+ " Q204=Q204"							// ;2ND SET-UP CLEARANCE
						+ " Q207=" + feed_m;					// ;FEED FOR MILLING(切刃)
				}
				break;
			/*
			case "8030":	// 円内粗仕上げ（ケーラムのみ）
				if (tolInfo.toolset.meas_type_DMG == 'F')
					throw new Exception("工具計測タイプＦにはこのP8030サイクルは使用できません in macroDef");
				if (g6p['I'].set == false || g6p['Z'].set == false || g6p['D'].set == false || g6p['E'].set == false || g6p['F'].set == false || g6p['W'].set == true)
					throw new Exception("マクロエラー in macroDef");

				{
					double g6pI;
					double g6pF;
					double g6pC;
					double g6pQ;
					g6pI = g6p['I'].d;
					g6pF = g6p['F'].d;
					g6pC = g6p['C'].set ? g6p['C'].d : 0.0;
					g6pQ = g6p['Q'].set ? g6p['Q'].d : g6pC - g6p['Z'].d;

					fukasa = g6pC - g6p['Z'].d;
					baseZ = g6pC;
					if (clrp - g6pC < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");

					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + sign_num(g6pC);
					stmp[1] =  "G252"
						+ " Q215=" + "1"					// ;MACHINING OPERATION
						+ " Q223=" + sign_num(g6pI)			// ;CIRCLE DIAMETER
						+ " Q368=" + "0"					// ;ALLOWANCE FOR SIDE
						+ " Q207=" + sign_num(g6pF)			// ;FEED RATE FOR MILLING
						+ " Q351=" + "+1"					// ;CLIMB OR UP-CUT
						+ " Q201=" + sign_num(-fukasa)		// ;DEPTH
						+ " Q202=" + sign_num(g6pQ)			// ;PLUNGING DEPTH
						+ " Q369=" + "0"					// ;ALLOWANCE FOR FLOOR
						+ " Q206=" + sign_num(g6pF)			// ;FEED RATE FOR PLUNGING
						+ " Q338=" + "0"					// ;INFEED FOR FINISHING
						+ " Q200=" + "5"					// ;SET-UP CLEARANCE
						+ " Q203=" + sign_num(g6pC)			// ;SURFACE COORDINATE
						+ " Q204=Q204"						// ;2ND SET-UP CLEARANCE
						+ " Q370=" + "1.2"					// ;TOOL PATH OVERLAP(*RADIUS)
						+ " Q366=" + "0"					// ;PLUNGING
						+ " Q385=" + sign_num(g6pF);		// ;FEED RATE FOR FINISHING
				}
				break;
			*/
			case "8010":  // 周り止め
			case "8013":  // 周り止め
			case "8015":  // 周り止め
			case "8011":  // 周り止め
			case "8014":  // 周り止め
			case "8016":  // 周り止め add in 2019/06/12
			case "8019":  // 周り止め add in 2019/06/12
				if (g6p['U'].Set == false || g6p['I'].Set == false || g6p['F'].Set == false)
					throw new Exception("マクロエラー in macroDef");
				//
				{
					double g6pI;
					double g6pF;
					double g6pC;
					double Q218 = 0.0;      // ;SLOT LENGTH
					double Q219 = 0.0;      // ;SLOT WIDTH
											//double Q374;			// ;ANGLE OF ROTATION
											//double Q207;			// ;FEED RATE FOR MILLING
											//double Q201;			// ;DEPTH
					double Q202 = 0.0;      // ;PLUNGING DEPTH
					double Q206 = 0.0;      // ;FEED RATE FOR PLUNGING
					double Q338 = 0.0;      // ;LENGTH FOR RAPID    (INFEED FOR FINISHING)
					double Q200 = 0.0;      // ;SET-UP CLEARANCE
											//double Q203;			// ;SURFACE COORDINATE
											//double Q204;			// ;2ND SET-UP CLEARANCE
					double Q385 = 0.0;      // ;FEED RATE FOR RAPID (FEED RATE FOR FINISHING)

					g6pI = g6p['I'].D;  // Q224;ANGLE OF ROTATION
					g6pF = g6p['F'].D;  // Q207;FEED RATE FOR MILLING
					g6pC = g6p['C'].Set ? g6p['C'].D : 0.0; // Q203;SURFACE COORDINATE

					Q200 = 5.0;
					fukasa = 0.0;
					DataRow dRow = CamUtil.CamNcD.KijunSize.Mawaridome(progNo, g6p['U'].D);
					Q218 = (double)dRow["パス長さ"];
					Q219 = (double)dRow["パス幅"];
					Q338 = (double)dRow["アプローチ長さ"];
					fukasa = (double)dRow["深さ"];
					Q202 = (double)dRow["プランジ量"];
					Q206 = (double)dRow["アプローチ速度Z"];
					if (Q206 == 0)
						Q206 = g6pF;
					Q385 = (double)dRow["アプローチ速度XY"];
					if (Q385 == 0)
						Q385 = g6pF;
					// 加工基準のシフト
					g6pC = g6pC - (double)dRow["加工基準深さ"];
					Q200 = Q200 + (double)dRow["加工基準深さ"];
					if (fukasa < 1) {
						throw new Exception("wefbwreh in macroDef");
					}
					// G253
					// Q215=+0 Q218=+60 Q219=+10 Q368=+0 Q374=+90 Q367=+0 Q207=+500 Q351=+1 Q201=-20
					// Q202=+6 Q369=+0 Q206=+150 Q338=+20 Q200=+5 Q203=+0 Q204=+50 Q366=+1 Q385=+200*
					// X-500 Y+400*
					// % TNC:\TG\PROG\MAWARIDOME.H
					baseZ = g6pC;
					if (clrp - g6pC < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");

					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(g6pC);
					stmp[1] = "G253"
						+ " Q215=" + "1"                    // ;MACHINING OPERATION
						+ " Q218=" + Q218                   // ;SLOT LENGTH
						+ " Q219=" + Q219                   // ;SLOT WIDTH
						+ " Q368=" + "0"                    // ;ALLOWANCE FOR SIDE
						+ " Q374=" + Sign_num(g6pI)         // ;ANGLE OF ROTATION
						+ " Q367=" + "1"                    // ;SLOT POSITION
						+ " Q207=" + Sign_num(g6pF)         // ;FEED RATE FOR MILLING
						+ " Q351=" + "+1"                   // ;CLIMB OR UP-CUT
						+ " Q201=" + Sign_num(-fukasa)      // ;DEPTH
						+ " Q202=" + Q202                   // ;PLUNGING DEPTH
						+ " Q369=" + "0"                    // ;ALLOWANCE FOR FLOOR
						+ " Q206=" + Q206                   // ;FEED RATE FOR PLUNGING
						+ " Q338=" + Q338                   // ;INFEED FOR FINISHING
						+ " Q200=" + Q200                   // ;SET-UP CLEARANCE
						+ " Q203=" + Sign_num(g6pC)         // ;SURFACE COORDINATE
						+ " Q204=Q204"                      // ;2ND SET-UP CLEARANCE
						+ " Q366=" + "0"                    // ;PLUNGING
						+ " Q385=" + Q385;                  // ;FEED RATE FOR FINISHING
				}
				break;
			/*
			case "8046":  // エアブロー
				if (g6p['Z'].set == false || g6p['R'].set == false || g6p['F'].set == false)
					throw new Exception("P8046 マクロエラー in macroDef");

				{
					double r_ten;
					fukasa = g6p['R'].d - g6p['Z'].d;
					r_ten = g6p['R'].d;
					double g6pF = g6p['F'].d;	// Q206;FEED RATE FOR PLNGNG
					baseZ = r_ten;
					if (clrp - r_ten < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");

					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + sign_num(r_ten);
					stmp[1] = "G201"
						+ " Q200=" + "0"						// ;SET-UP CLEARANCE
						+ " Q201=" + sign_num(-fukasa)			// ;DEPTH
						+ " Q206=" + g6pF						// ;FEED RATE FOR PLNGNG
						+ " Q211=" + "0"						// ;DWELL TIME AT DEPTH
						+ " Q208=" + "0"						// ;RETRACTION FEED RATE
						+ " Q203=" + sign_num(r_ten)			// ;SURFACE COORDINATE
						+" Q204=Q204";							// ;2ND SET-UP CLEARANCE
				}
				break;
			*/
			/*
			case "8280":  // ガンドリル
				// 未使用メッセージ
				CamUtil.LogOut.CheckCount("Conv_DMG_cycle 501", "P8280はチェックされていません。使用する場合は藤本まで連絡ください。");
				MessageBox.Show("P8280はチェックされていません。使用する場合は藤本まで連絡ください。");
				if (g6p['Z'].set == false || g6p['K'].set == false || g6p['R'].set == false || g6p['F'].set == false)
					throw new Exception("P8280 マクロエラー");

				{
					double r_ten;
					double plunge;
					fukasa = g6p['R'].d - g6p['Z'].d;
					plunge = g6p['Q'].set ? g6p['Q'].d : fukasa;
					r_ten = g6p['R'].d;
					double k_fukasa = g6p['R'].d - g6p['K'].d;
					double g6pF = g6p['F'].d;	// Q206;FEED RATE FOR PLNGNG
					baseZ = r_ten;
					if (clrp - r_ten < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");

					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + sign_num(r_ten);
					stmp[1] = "G205"
						+ " Q200=" + "0"						// ;SET-UP CLEARANCE
						+ " Q201=" + sign_num(-fukasa)			// ;DEPTH
						+ " Q206=" + g6pF						// ;FEED RATE FOR PLNGNG
						+ " Q202=" + sign_num(plunge)			// ;PLUNGING DEPTH
						+ " Q203=" + sign_num(r_ten)			// ;SURFACE COORDINATE
						+ " Q204=Q204"							// ;2ND SET-UP CLEARANCE
						+ " Q212=" + "0"						// ;DECREMENT
						+ " Q205=" + "0"						// ;MIN. PLUNGING DEPTH
						+ " Q258=" + "5.0"						// ;UPPER ADV STOP DIST
						+ " Q259=" + "5.0"						// ;LOWER ADV STOP DIST
						+ " Q257=" + "0"						// ;DEPTH FOR CHIP BRKNG
						+ " Q256=" + "0.1"						// ;DIST FOR CHIP BRKNG
						+ " Q211=" + "0.5"						// ;DWELL TIME AT DEPTH
						+ " Q379=" + k_fukasa					// ;STARTING POINT
						+ " Q253=" + "700";						// ;F PRE-POSITIONING
					if (mach.machn.ID == CamUtil.Machine.machID.DMU210P || mach.machn.ID == CamUtil.Machine.machID.DMU210P2) {
						int retfeed = (int)Math.Round(mach.ncCode.RetractFeed * NcoutName.Skog.retract_rate);
						stmp[1] += $" Q208=+{retfeed:d} Q395=+0";           // ;RETRACTION FEED RATE, ;DEPTH REFERENCE
					}
				}
				break;
			*/
			case "8290":  // 超鋼ドリル
				if (g6p['Z'].Set == false || g6p['K'].Set == false || g6p['R'].Set == false || g6p['F'].Set == false)
					throw new Exception("P8290 マクロエラー");

				{
					double k_srf;				// 上面の絶対座標値
					double k_fukasa;			// 上面からの加工開始深さ（正の値）
					double r_inc = 3.0;
					double g6pF = g6p['F'].D;
					double r_ten = g6p['R'].D;	// Ｒ点

					k_srf = r_ten - r_inc;
					fukasa = k_srf - g6p['Z'].D;

					// Kの値は他のマクロと異なり上面からの相対座標であるので変更 2011/09/03
					//k_fukasa = k_srf - g6p['K'].d;
					k_fukasa = - g6p['K'].D;

					if (fukasa <= 0.0 || k_fukasa <= 0.0)
						throw new Exception("awfqehfbwerfbwehr");
					if (fukasa <= k_fukasa)
						throw new Exception("awfqehfbwerfbwehr");
					if (clrp - r_ten < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");


					baseZ = k_srf;
					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(k_srf);
					stmp[1] = "G203"
						+ " Q200=" + r_inc						// ;SET-UP CLEARANCE
						+ " Q201=" + Sign_num(-fukasa)			// ;DEPTH
						+ " Q206=" + g6pF						// ;FEED RATE FOR PLNGNG
						+ " Q202=" + k_fukasa					// ;PLUNGING DEPTH
						+ " Q210=" + "2.0"						// ;DWELL TIME AT TOP
						+ " Q203=" + Sign_num(k_srf)			// ;SURFACE COORDINATE
						+ " Q204=Q204"							// ;2ND SET-UP CLEARANCE
						+ " Q212=" + "0"						// ;DECREMENT
						+ " Q213=" + "0"						// ;BREAKS
						+ " Q205=" + "0"						// ;MIN. PLUNGING DEPTH
						+ " Q211=" + "2.0"						// ;DWELL TIME AT DEPTH
						+ " Q208=" + "500"						// ;RETRACTION FEED RATE
						+ " Q256=" + "0.2";						// ;DIST. FOR CHIP BRKNG
				}
				break;
			default:
				throw new Exception("program ERROR in macroDef");
			}
			if (fukasa <= 0.0) {
				throw new Exception("マクロ深さエラー in macroDef");
			}
			return stmp;
		}
		// /////////////////////////////
		// ＸＹ座標値を必要とするサイクル
		// /////////////////////////////
		private string[] MacroDef(ref double baseZ, string progNo, NcLineCode.MacroMode g6p, double clrp, NcLineCode.Xisf xyzsf) {
			string[] stmp = new string[2];
			double fukasa;

			switch (progNo) {
			//case "8020":	// 円内仕上げ（ケーラムのみ）
			case "8025":	// スプルーロック
				if (TolInfo.Toolset.MeasTypeDMG == 'F')
					throw new Exception("工具計測タイプＦにはこのP8020,P8025サイクルは使用できません in macroDef");
				if (g6p['I'].Set == false || g6p['Z'].Set == false || g6p['D'].Set == false || g6p['E'].Set == false || g6p['F'].Set == false || g6p['W'].Set == true)
					throw new Exception("マクロエラー in macroDef");

				{
					double g6pI;
					double g6pF;
					double g6pC;
					double g6pQ;
					g6pI = g6p['I'].D;
					g6pF = g6p['F'].D;
					g6pC = g6p['C'].Set ? g6p['C'].D : 0.0;
					g6pQ = g6p['Q'].Set ? g6p['Q'].D : g6pC - g6p['Z'].D;

					fukasa = g6pC - g6p['Z'].D;

					baseZ = g6pC;
					if (clrp - g6pC < 0) throw new Exception("クリアランスプレーン設定エラー in macroDef");

					stmp[0] = "D02 Q204 P01 +Q1754 P02 " + Sign_num(g6pC);
					stmp[1] = "G214"
						+ " Q200=" + "5"					// ;SET-UP CLEARANCE
						+ " Q201=" + Sign_num(-fukasa)		// ;DEPTH
						+ " Q206=" + Sign_num(g6pF)			// ;FEED RATE FOR PLNGING
						+ " Q202=" + Sign_num(g6pQ)			//
						//+ " Q207=" + "10"					// ;FEED RATE FOR MILLING
						+ " Q207=" + Sign_num(g6pF)			// ;FEED RATE FOR MILLING
						+ " Q203=" + Sign_num(g6pC)			// ;SURFACE COORDINATE
						+ " Q204=Q204"						// ;2ND SET-UP CLEARANCE
						+ " Q216=" + Sign_num(xyzsf.X)
						+ " Q217=" + Sign_num(xyzsf.Y)
						+ " Q222=" + "0"
						+ " Q223=" + Sign_num(g6pI);		// ;SURFACE COORDINATE
				}
				break;
			default:
				throw new Exception("未登録のカスタムマクロ " + progNo + " が使用された in macroDef");
			}
			if (fukasa <= 0.0) throw new Exception("マクロ深さエラー in macroDef");
			return stmp;
		}
	}
}
