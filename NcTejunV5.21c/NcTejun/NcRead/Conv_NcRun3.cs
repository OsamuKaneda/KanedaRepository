using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using CamUtil;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	/// <summary>
	/// 主型用ＮＣデータの変換。文字列の変換２
	/// </summary>
	class Conv_NcRun3 : Conv
	{
		private static Regex regG41 = new Regex("G4[12][A-Z;]");
		private static Regex regG40 = new Regex("G40M98P0*6[A-Z;]");
		private static readonly Regex regM03 = new Regex("M0*3");
		private static readonly Regex regM05 = new Regex("M0*5");

		private static Regex regUVW = new Regex("U[-0-9.]*V[-0-9.]*W[0-9.]*S");
		private static Regex regUKW = new Regex("U[-0-9.]*K[-0-9.]*W[-0-9.]*S");



		/// <summary>加工機情報</summary>
		NcdTool.Mcn1 mach;
		/// <summary>マクロ番号</summary>
		int mcroNo;
		/// <summary>加工機番号</summary>
		int machNo;

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

		public Conv_NcRun3(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach)
			: base(5, false, null, true, toolList) {

			this.mach = mach;

			if (mach.ncCode.MAC.HasValue) {
				mcroNo = mach.ncCode.MAC.Value;
				machNo = mach.ncCode.MNO.Value;
			}
			else mcroNo = machNo = 0;
		}

		public override OutLine ConvExec() {
			NcLineCode txtd = this.NcQue[0];

			// /////////
			// EDITSTT
			// /////////
			//a:3: :s: +::g
			if (txtd.OutLine.Txt.IndexOf(' ') >= 0)
				txtd.OutLine.Set(txtd.OutLine.Txt.Replace(" ", ""));
			//b:3:G100T:M:M01;
			if (txtd.B_g100) {
				if (NcoutName.Smch.m01)
					txtd.OutLine.MaeAdd("M01");
			}

			// /////////
			// Machine
			// /////////
			if (txtd.B_g100) {
				// 設備番号の追加
				//3:G100:i:#527=XX.;:
				if (machNo > 0)
					txtd.OutLine.MaeAdd("#" + mcroNo.ToString() + "=" + machNo.ToString() + ".");

				// Ｇ１００コードの修正
				if (regUVW.Match(txtd.NcLine).Success || regUKW.Match(txtd.NcLine).Success)
					LogOut.CheckCount("Conv_NcRun3 146", false, "G100 UVW,UKW Exists");
				switch (mach.ID) {
				case Machine.MachID.DMU200P:
				case Machine.MachID.DMU210P:
				case Machine.MachID.DMU210P2:
				case Machine.MachID.YBM1218V:
				case Machine.MachID.FNC208:
				case Machine.MachID.FNC74:
				case Machine.MachID.YMC430:
					//3:G100T:s:U[-0-9.]*V[-0-9.]*W[0-9.]*S:S:
					txtd.OutLine.Set(Substitute(txtd.OutLine.Txt, regUVW, "S", false));
					break;
				case Machine.MachID.MCC2013:
				case Machine.MachID.MCC3016VG:
				case Machine.MachID.V77:
				case Machine.MachID.KENSYO:
					//3:G100T:s:U[-0-9.]*K[-0-9.]*W[-0-9.]*S:S:
					txtd.OutLine.Set(Substitute(txtd.OutLine.Txt, regUKW, "S", false));
					break;
				case Machine.MachID.MHG_1500:
					break;
				case Machine.MachID.D500:
				case Machine.MachID.LineaM:
				default:
					throw new Exception("afvafvfvafvaf"); ;
				}
			}

			switch (mach.ID) {
			case Machine.MachID.MCC3016VG:
			case Machine.MachID.KENSYO:
				// B に小数点を入れる
				//3:G100T:s:([B][0-9][0-9]*)([0-9][0-9][0-9]S):$0.$1:
				//3:G100T:s:([B])([0-9][0-9][0-9]S):$00.$1:
				//3:G100T:s:([B])([0-9][0-9]S):$00.0$1:
				//3:G100T:s:([B])([0-9]S):$00.00$1:
				if (txtd.B_g100) {
					if (txtd.NcLine.IndexOf('B') >= 0) {
						txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, 'B', txtd.NumList.Code('B').dblData.ToString("0.000")));
					}
				}
				//9:
				//3:%,G100T,P-1:s:X0Y0::
				if (NcQue.QueueMin <= -1 && NcQue[-1].B_g100)
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("X0Y0", ""));

				//3:%,G100T,P-2:s:^:G105:
				if (NcQue.QueueMin <= -2 && NcQue[-2].B_g100)
					txtd.OutLine.Set("G105" + txtd.OutLine.Txt);

				//3:G0*1[A-Z;],M98P9699,P-1:E:
				if (txtd.B_g1 && txtd.G1 == 1) if (NcQue[-1].NcLine.IndexOf(ClLink5Axis.Kotei) >= 0)
						throw new Exception("ＮＣデータエラー in Conv");

				//3:%,M98P9699,P-1:s:G00*::
				//3:%,M98P9699,P-1:s:^:G105:
				if (NcQue.QueueMin <= -1 && NcQue[-1].NcLine.IndexOf(ClLink5Axis.Kotei) >= 0)
					txtd.OutLine.Set("G105" + txtd.OutLine.Txt.Replace("G00", ""));

				break;
			}

			// 工具径オフセットの設定
			// 3:G4[12][A-Z;]:s:[IJK][-IJK0-9.]*::
			// 3:G40M98P0*6[A-Z;(]:s:G40::
			OffsetD(txtd);
			//3:^[^XY]*Z[0-9],!Z,P-1:m:M100; 廃止

			// /////////
			// EDITEND
			// /////////
			//if (mach.mcn2.cool[0] != 0) sed_i(txtd);	// i

			return txtd.OutLine;
		}

		/// <summary>
		/// 工具径オフセットの設定
		/// </summary>
		/// <remarks>
		/// 3:G4[12][A-Z;]:s:[IJK][-IJK0-9.]*::
		/// 3:G40M98P0*6[A-Z;(]:s:G40::
		/// </remarks>
		/// <param name="txtd"></param>
		private void OffsetD(NcLineCode txtd) {

			//3:G4[12][A-Z;]:s:[IJK][-IJK0-9.]*::
			//P8025で使用している
			if (regG41.Match(txtd.NcLine + ";").Success) {
				if (txtd.B_26('I')) txtd.OutLine.Set(NcLineCode.NcDelChar(txtd.OutLine.Txt, 'I'));
				if (txtd.B_26('J')) txtd.OutLine.Set(NcLineCode.NcDelChar(txtd.OutLine.Txt, 'J'));
				if (txtd.B_26('K')) txtd.OutLine.Set(NcLineCode.NcDelChar(txtd.OutLine.Txt, 'K'));
			}

			//3:G40M98P0*6[A-Z;(]:s:G40::
			if (regG40.Match(txtd.NcLine + ";").Success) {
				txtd.OutLine.Set(Substitute(txtd.OutLine.Txt, new Regex("G40"), "", false));
				LogOut.CheckCount("Conv_NcRun3 286", false, "G40M98P0006 Exists");
			}
		}
	}
}