using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using CamUtil;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	/// <summary>
	/// 主型用ＮＣデータの変換。文字列の変換３
	/// </summary>
	class ConvOld_NcRun5 : Conv
	{
		private static Regex regG80 = new Regex("G[78][0-9][A-Z;]");
		private static Regex regG00 = new Regex("G00*[XYZ;]");
		private static Regex regG01F = new Regex("G0*1[A-EG-Z][^F]*;");



		/// <summary>加工機情報</summary>
		NcdTool.Mcn1 mach;

		//sed0s(0, ServerPC.SvrFldrM + @"GENERAL\EDITSTT");
		//a:1:;:s:;.*$:;:
		//a:1: :s: +::g
		//a:1:G83:Q:Q[0-9.]*::
		//a:1:G65P8200:Q:Q[0-9.]*::
		//a:1:G66P8200:Q:Q[0-9.]*::
		//a:3: :s: +::g
		//b:1:G100T:T:
		//c:1:G100T:s:T[0-9]*:T$t:
		//c:1:G100T:s:S[0-9]*:S$s:
		//c:1:G00G90X,G100T,P-1:s:X.*:$x;:
		//c:1:G100T00*[A-Z;],M98P0*6[A-Z;(]:d
		//d:1:^M30;:s:M30;:M99;:
		//e:1:M98P0*6[A-Z;(]:i:G00$c;:
		//e:1:M98P0*6[A-Z;(]:i:$p;:
		//f:1:M98P0*6[A-Z;(]:i:G00$z;:
		//f:1:M98P0*6[A-Z;(]:i:$x;:
		//b:1:G[46][126].*D:s:D[0-9]*:D$d:
		//b:3:G100T:M:M01;
		//g:1:G0*1[A-Z;],,N1:a:G05;:
		//g:1:%,M98P9699,P-3:i:G05;:
		//g:1:%,G65P9699,P-3:i:G05;:
		//g:1:M98P0*6[A-Z;(]:i:G91X0Y0Z0;:
		//g:5:G[78][0-9][A-Z;]:E:KOUSOKU-MODE:
		//h:1:G4[12]:s:[IJKD]-*[0-9.]*::g
		//h:1:G4[012][A-Z;]:s:G4[012]::
		//h:1:^G39[A-Z]:d
		//k:1:G65:K
		//k:1:G66:K

		//sed0s(1, Program.tejun.mach.mcn2.sedf[0]);
		// -- MCC2013 --
		//1:\(:s:\([^)]*\)::g
		//1:^G39[A-Z]:d
		//3:G100:i:#527=11.;:
		//3:G100T:s:U[-0-9.]*K[-0-9.]*W[-0-9.]*S:S:
		//3:G4[12][A-Z;]:s:[IJK][-IJK0-9.]*::
		//3:G40M98P0*6[A-Z;(]:s:G40::
		//3:^[^XY]*Z[0-9],!Z,P-1:m:M100;
		//5:G05:s:;:P10000;:
		//5:M98P0*6[;A-Z(]:i:G05P0;:
		//5:M100,G100,P-4:d
		//5:M100,M98P0*6[A-Z;(],P+1:d
		//5:M100,M98P0*6[A-Z;(],P+2:d
		//6:^O:s:;:($%);:
		// -- MCC2013_G01 追加 --
		//0:G0*1[A-EG-Z][^F]*;:E:
		//6:G00*[XYZ;]:s:;:F16000;:
		//6:G00*[XYZ;]:s:G00*:G01:
		//6:^F:s:^:G01:

		//sed0s(1, ServerPC.SvrFldrM + @"GENERAL\EDITEND");
		//i:4:%,M0*3;,P-1:a:$r:
		//i:4:M0*5;:i:$R:
		//b:6:^;:d

		public ConvOld_NcRun5(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach)
			: base(5, false, null, true, toolList) {

			this.mach = mach;
		}

		public override OutLine ConvExec() {
			NcLineCode txtd = this.NcQue[0];

			// /////////
			// EDITSTT
			// /////////
			//g:5:G[78][0-9][A-Z;]:E:KOUSOKU-MODE:
			if (ncname.Nmgt.Dimn == '3')
				if (mach.ncCode.HSP_ON != null) Sed_g(txtd);		// g

			// /////////
			// Machine
			// /////////
			switch (mach.ID) {
			case Machine.MachID.DMU200P:
			case Machine.MachID.DMU210P:
			case Machine.MachID.DMU210P2:
				//廃止
				//5:M100,G100,P-4:d
				//5:M100,M98P0*6[A-Z;(],P+1:d
				//5:M100,M98P0*6[A-Z;(],P+2:d

				//5:X0Y0,M98P0*6[A-Z;(],P+1:s:(G00)*X0Y0::
				if (txtd.NcLine.IndexOf("X0Y0") >= 0 && this.NcQue[+1].B_p0006)
					txtd.OutLine.Set(Substitute(txtd.OutLine.Txt, new Regex("(G00)*X0Y0"), "", false));

				break;
			case Machine.MachID.MCC2013:
			case Machine.MachID.YBM1218V:
			case Machine.MachID.V77:
				//0:G0*1[A-EG-Z][^F]*;:E:

				//高速加工モードのコードを設定する
				//5:G05:s:;:P10000;:
				//5:M98P0*6[;A-Z(]:i:G05P0;:
				HiSpeed(txtd, false);

				//廃止
				//5:M100,G100,P-4:d
				//5:M100,M98P0*6[A-Z;(],P+1:d
				//5:M100,M98P0*6[A-Z;(],P+2:d

				break;
			case Machine.MachID.MCC3016VG:
			case Machine.MachID.KENSYO:
				//高速加工モードのコードを設定する
				//5:M98P9699:i:G05P0;:
				//5:M98P9699:s:P9699:P8509:
				//5:G05:s:;:P10000;:
				//5:M98P0*6[;A-Z(]:i:G05P0;:
				HiSpeed(txtd, true);

				//廃止
				//5:M100,G100,P-4:d
				//5:M100,M98P0*6[A-Z;(],P+1:d
				//5:M100,M98P0*6[A-Z;(],P+2:d
				break;
			case Machine.MachID.FNC208:
			case Machine.MachID.FNC74:
			case Machine.MachID.MHG_1500:
			case Machine.MachID.YMC430:
				//廃止
				//5:M100,G100,P-4:d
				//5:M100,M98P0*6[A-Z;(],P+1:d
				//5:M100,M98P0*6[A-Z;(],P+2:d
				break;
			case Machine.MachID.D500:
			case Machine.MachID.LineaM:
			//case Machine.machID.SNC106:
			//case Machine.machID.KENSYO_D:
			//case Machine.machID.KENSYO_M:
			//case Machine.machID.KENSYO_Y:
			default:
				throw new Exception("rfqefraefrafdvcafvfre");
			}
			// ----- 共通 -----
			//6:^O:s:;:($%);:　１へ移動
			/*
			if (txtd.ncLine.Length > 0)
				if (txtd.ncLine[0] == 'O')
					txtd.outLine.Set(txtd.outLine.txt, ncoutName.outnam);
			*/

			// ////////////////
			// ----- ３ＤのＧ０１変換 -----
			// ////////////////
			if (mach.ncCode.G01) {
				switch (mach.Axis_Type) {
				case Machine.Machine_Axis_Type.AXIS5_DMU:
					// Conv_DMG_General でＧ０１変換するためここでは実行しない。
					// 将来はここで実施し、統一する
					break;
				default:
					if (ncname.Nmgt.Dimn == '3') {
						// G01行にＦコードがない場合にエラー停止する
						//6:G0*1[A-EG-Z][^F]*;:E:
						if (regG01F.Match(txtd.NcLine + ";").Success)
							throw new Exception("ＮＣデータエラー in ConvOld");

						// ３次元加工のためにG00をG01に変換する
						ChgG01(txtd);
					}
					break;
				}
			}
			/*
			if (mach.G00toG01) {
				// G01行にＦコードがない場合にエラー停止する
				switch (mach.machn.ID) {
				case Machine.machID.MCC3016VG:
					//6:G0*1[A-EG-Z][^F]*;:E:
					if (regG01F.Match(txtd.ncLine + ";").Success)
						throw new Exception("ＮＣデータエラー in ConvOld");
					break;
				case Machine.machID.MCC2013:
				case Machine.machID.YBM1218V:
					break;
				default:
					throw new Exception("qjwehfbqebf");
				}

				// ３次元加工のためにG00をG01に変換する
				switch (mach.machn.ID) {
				case Machine.machID.MCC2013:
				case Machine.machID.MCC3016VG:
					chgG01(txtd, 16000); break;
				case Machine.machID.YBM1218V:
					chgG01(txtd, 10000); break;
				default:
					throw new Exception("qjwehfbqebf");
				}
				if (ncname.nmgt.dimn != '3')
					LogOut.CheckCount("ConvOld_NcRun5 198", this.ncname.nnam);
				// 暫定チェック
				if (Program.tejun.machname != "MCC3016VG_G01" && Program.tejun.machname != "YBM1218V_G01" && Program.tejun.machname != "MCC2013_G01")
					throw new Exception("qjwehfbqebf");
			}
			else {
				if (Machine.G01(mach.machn.ID))
					if (ncname.nmgt.dimn == '3')
						LogOut.CheckCount("ConvOld_NcRun5 206", this.ncname.nnam);
				// 暫定チェック
				if (Program.tejun.machname == "MCC3016VG_G01" || Program.tejun.machname == "YBM1218V_G01" || Program.tejun.machname == "MCC2013_G01")
					throw new Exception("qjwehfbqebf");
			}
			*/

			// /////////
			// EDITEND
			// /////////
			//不要
			//b:6:^;:d

			return txtd.OutLine;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="txtd"></param>
		private void Sed_g(NcLineCode txtd) {

			if (regG80.Match(txtd.NcLine + ";").Success)
				throw new Exception("KOUSOKU-MODE ERROR");

			return;
		}

		/// <summary>
		/// 高速加工モードのコードを設定する
		/// </summary>
		/// <remarks>
		/// 5:M98P9699:i:G05P0;:
		/// 5:M98P9699:s:P9699:P8509:
		/// 5:G05:s:;:P10000;:
		/// 5:M98P0*6[;A-Z(]:i:G05P0;:
		/// </remarks>
		/// <param name="txtd"></param>
		/// <param name="p9699">P9699の前にも設定</param>
		private void HiSpeed(NcLineCode txtd, bool p9699) {

			if (p9699) {
				//5:M98P9699:i:G05P0;:
				if (txtd.NcLine.IndexOf(ClLink5Axis.Kotei) >= 0) {
					LogOut.CheckCount("ConvOld_NcRun5 247", false, this.ncname.Nnam);
					txtd.OutLine.MaeAdd("G05P0");
				}
				//5:M98P9699:s:P9699:P8509:
				if (txtd.NcLine.IndexOf(ClLink5Axis.Kotei) >= 0) {
					LogOut.CheckCount("ConvOld_NcRun5 252", false, this.ncname.Nnam);
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace(ClLink5Axis.Kotei, "M98P8509"));
				}
			}
			else {
				if (txtd.NcLine.IndexOf(ClLink5Axis.Kotei) >= 0)
					LogOut.CheckCount("ConvOld_NcRun5 260", false, this.ncname.Nnam);
			}

			// ConvOld_NcRun1 に統合したためコメントアウト 2016/11/30
			/*
			//5:G05:s:;:P10000;:
			if (txtd.ncLine.IndexOf("G05") >= 0) {
				txtd.outLine.Set(txtd.outLine.txt + "P10000");
			}
			//5:M98P0*6[;A-Z(]:i:G05P0;:
			if (txtd.b_p0006) {
				txtd.outLine.maeAdd("G05P0");
			}
			*/
		}

		/// <summary>
		/// ３次元加工のためにG00をG01に変換する
		/// </summary>
		/// <remarks>
		/// 6:G00*[XYZ;]:s:;:F16000;:	(F10000)
		/// 6:G00*[XYZ;]:s:G00*:G01:
		/// 6:^F:s:^:G01:
		/// </remarks>
		/// <param name="txtd"></param>
		private void ChgG01(NcLineCode txtd) {
			//6:G00*[XYZ;]:s:;:F16000;:
			//6:G00*[XYZ;]:s:;:F10000;:
			if (regG00.Match(txtd.NcLine + ";").Success)
				txtd.OutLine.Set(txtd.OutLine.Txt + "F" + mach.ncCode.RPD.ToString());
			//6:G00*[XYZ;]:s:G00*:G01:
			if (regG00.Match(txtd.NcLine + ";").Success)
				txtd.OutLine.Set(Substitute(txtd.OutLine.Txt, new Regex("G00*"), "G01", false));
			//6:^F:s:^:G01:
			if (txtd.NcLine[0] == 'F')
				txtd.OutLine.Set("G01" + txtd.OutLine.Txt);
		}
	}
}