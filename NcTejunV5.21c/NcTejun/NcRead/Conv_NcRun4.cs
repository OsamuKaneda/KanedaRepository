using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using CamUtil;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	/// <summary>
	/// 主型用ＮＣデータの変換。一般変換（ミラー、移動....）
	/// </summary>
	class Conv_NcRun4 : Conv
	{
		/// <summary>アプローチ、リトラクトなどの判断</summary>
		protected enum AprRet
		{
			NULL = -1,	// 未設定
			RAPID = 0,	// 早送り
			APR = 1,	// アプローチ
			CUT = 2,	// 加工
			RTR = 3		// リトラクト
		}

		/// <summary>出力ＮＣデータの行数</summary>
		protected int start0;

		/// <summary>平行移動量</summary>
		private Vector3 trns;

		/// <summary>加工機情報</summary>
		private readonly NcdTool.Mcn1 mach;
		/// <summary>GENERALのＮＣコードの数値情報</summary>
		private readonly NcDigit post;

		/// <summary>
		/// Ｇコードグループ１の直前のコード（G00,G01,G02,G03...）
		/// </summary>
		private short g1mod;


		public Conv_NcRun4(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach)
			: base(5, false, null, true, toolList) {

			this.mach = mach;
			this.post = new NcDigit();
			start0 = -2;
			trns = ncname.Nggt.trns;
		}

		public override OutLine ConvExec() {
			NcLineCode txtd = this.NcQue[0];

			List<NcLineCode.NumCode> codeData = new List<NcLineCode.NumCode>();
			foreach (NcLineCode.NumCode num in txtd.NumList) codeData.Add(num);
			NcLineCode.NumCode numd;

			string chout;
			bool inc;	// 相対座標系の場合true。
			bool zahyo;	// X, Y, Z, I, J などが座標値の場合true。（カスタムマクロでは座標値ではない場合もある）

			// 円弧補間でのＸＹ座標値追加
			if (mach.ncCode.XYH) codeData = Xymake(txtd, codeData);

			// ミラーと移動の処理
			if (ncname.Nggt.Mirr != NcZahyo.Null)
				LogOut.CheckCount("Conv_NcRun4 066", false, "ミラーＸＹ実行");
			if (trns != Vector3.v0)
				LogOut.CheckCount("Conv_NcRun4 068", false, "移動実行");

			// アブソリュート/インクリメンタルの設定
			if ((txtd.G9 == 91 && mach.ncCode.ZSK == null) || (mach.ncCode.ZSK.HasValue && mach.ncCode.ZSK.Value == false))
				inc = true;
			else
				inc = false;

			// /////////////////////
			// 出力テキストの作成
			// /////////////////////
			chout = "";
			// 一般ＮＣ
			if (txtd.G6 == 67 && txtd.G8 == 80) {
				// チェックミラー移動の可否 G100 とその次行のG00G90X0Y0 は移動ミラーはしない 2018/04/24
				zahyo = txtd.LnumT != 1 && txtd.LnumT != 2;
				if (txtd.B_g100 && zahyo) throw new Exception("wfvwrvwrvewrgvewrgv");
				// ミラー移動後の座標値設定
				NcLineCode.Xisf xyz2 = txtd.Xyzsf.Mirror(ncname.Nggt.Mirr).Transfer(trns);
				if (xyz2.Dist.X != 0.0 && txtd.B_26('X') == false) throw new Exception("qfebqhfrbh");
				if (xyz2.Dist.Y != 0.0 && txtd.B_26('Y') == false) throw new Exception("qfebqhfrbh");
				if (xyz2.Dist.Z != 0.0 && txtd.B_26('Z') == false) throw new Exception("qfebqhfrbh");

				foreach (NcLineCode.NumCode cdtmp in codeData) {
					numd = cdtmp;
					/*
					// ＮＣコードのセット（ドウェルのＸは３１とする）
					ic = numd.NccharNo; // NC DATA CODE
					if (txtd.ExistsG(4) && ic == 24) ic = 31;
					*/

					if (numd.ncChar == '%' && start0 == -2) { ;}
					else if (numd.ncChar == '%') start0 = -2;

					switch (numd.ncChar) {
					case 'G':
						if ((mach.ncCode.ZSK.HasValue && mach.ncCode.ZSK.Value == true) && numd.L == 91) numd = new NcLineCode.NumCode('G', "90", post.Data['G']);
						if ((mach.ncCode.ZSK.HasValue && mach.ncCode.ZSK.Value == false) && numd.L == 90) numd = new NcLineCode.NumCode('G', "91", post.Data['G']);

						// ミラーで変化するＧコードの処理
						if (ncname.Nggt.Mirr.ToMirrABC.Z < 0) {
							numd = numd.GCodeMirror(post);
						}

						chout += Txmake(txtd, numd, numd.dblData);
						break;
					case 'X':
						if (txtd.ExistsG(4)) {
							chout += Txmake(txtd, numd, numd.dblData);
						}
						else {
							chout += Txmake(txtd, numd, zahyo ? (inc ? xyz2.Dist.X : xyz2.X) : numd.dblData);
						}
						break;
					case 'Y':
						chout += Txmake(txtd, numd, zahyo ? (inc ? xyz2.Dist.Y : xyz2.Y) : numd.dblData); break;
					case 'Z':
						chout += Txmake(txtd, numd, zahyo ? (inc ? xyz2.Dist.Z : xyz2.Z) : numd.dblData); break;
					case 'R':
						chout += Txmake(txtd, numd, numd.dblData); break;
					case 'I':
						chout += Txmake(txtd, numd, (zahyo && ncname.Nggt.Mirr.X.HasValue) ? -numd.dblData : numd.dblData); break;
					case 'J':
						chout += Txmake(txtd, numd, (zahyo && ncname.Nggt.Mirr.Y.HasValue) ? -numd.dblData : numd.dblData); break;
					case 'K':
						chout += Txmake(txtd, numd, numd.dblData); break;
					case '(':
					case '#':
						chout += numd.ncChar.ToString() + numd.S;
						break;
					case ';':
					case '/':
					case '%':
						chout += Txmake(txtd, numd, 0.0);
						break;
					default:
						chout += Txmake(txtd, numd, numd.dblData);
						break;
					}
				}
			}
			// 固定サイクル・カスタムマクロ
			else {
				CamUtil.CamNcD.MacroCode mCode;
				double numdd;

				if (txtd.G8 != 80)
					mCode = new CamUtil.CamNcD.MacroCode(NcdTool.Tejun.BaseNcForm, txtd.G8);
				else
					mCode = new CamUtil.CamNcD.MacroCode(NcdTool.Tejun.BaseNcForm, txtd.G6p.ProgNo);

				// チェックミラー移動の可否
				zahyo = false;
				if (ncname.Nggt.Mirr != NcZahyo.Null || trns != Vector3.v0) {
					switch (mCode.kako) {
					case CamUtil.CamNcD.MacroCode.KAKO.K_TRU:
						zahyo = true; break;
					case CamUtil.CamNcD.MacroCode.KAKO.K_NON:
						zahyo = false; break;
					case CamUtil.CamNcD.MacroCode.KAKO.K_FLS:
						throw new Exception("FATAL" + mCode.mess + "はＮＣデータのミラー処理、移動処理の対象外です。");
					case CamUtil.CamNcD.MacroCode.KAKO.K_CHK:
						zahyo = true;
						if (ProgVersion.Debug)
							LogOut.CheckCount("Conv_NcRun4 155", true, mCode.mess + "はミラー処理、移動処理のチェックがされていません。");
						else
							throw new Exception("FATAL" + mCode.mess + "はミラー処理、移動処理のチェックがされていません。");
						break;
					}
				}

				// チェックinc
				if (inc)
					switch (mCode.kako) {
					case CamUtil.CamNcD.MacroCode.KAKO.K_TRU:
					case CamUtil.CamNcD.MacroCode.KAKO.K_FLS:
					case CamUtil.CamNcD.MacroCode.KAKO.K_CHK:
						throw new Exception("FATAL固定サイクル・カスタムマクロのインクリメンタル指令は動作確認されていません。プログラム管理者までご蓮核ください。");
					case CamUtil.CamNcD.MacroCode.KAKO.K_NON:
						LogOut.CheckCount("Conv_NcRun4 170", true, mCode.mess + "がインクリメンタル指令で実行された。");
						break;
					}

				foreach (NcLineCode.NumCode cdtmp in codeData) {
					numd = cdtmp;
					if (txtd.ExistsG(4) && numd.ncChar == 'X') throw new Exception("wedbqwedbh");
					if (numd.ncChar == '%') throw new Exception("wedbqwedbh");

					numdd = numd.dblData;
					if (zahyo) {
						if (mCode.xlist.IndexOf(numd.ncChar) >= 0) {
							switch (numd.ncChar) {
							case 'X':
								numdd = numd.dblData * (ncname.Nggt.Mirr.ToMirrXYZ.X) + trns.X; break;
							case 'Y':
								numdd = numd.dblData * (ncname.Nggt.Mirr.ToMirrXYZ.Y) + trns.Y; break;
							case 'I':
								numdd = numd.dblData * (ncname.Nggt.Mirr.ToMirrXYZ.X); break;
							case 'J':
								numdd = numd.dblData * (ncname.Nggt.Mirr.ToMirrXYZ.Y); break;
							default: throw new Exception("qejrfbqh");
							}
						}
						if (mCode.zlist.IndexOf(numd.ncChar) >= 0)
							numdd = numd.dblData + trns.Z;
					}

					// ;ABCDEFGHIJKLMNOPQRSTUVWXYZ/(%#
					switch (numd.ncChar) {
					case '(':
					case '#':
						chout += numd.ncChar.ToString() + numd.S; break;
					case ';':
					case '/':
					case '%':
						chout += Txmake(txtd, numd, 0.0); break;
					default:
						chout += Txmake(txtd, numd, numdd); break;
					}
				}
			}
			g1mod = txtd.G1;

			txtd.OutLine.Set(chout);
			return txtd.OutLine;
		}

		/// <summary>
		/// 円弧補間のＸＹ座標値を追加する
		/// </summary>
		/// <param name="txtd"></param>
		/// <param name="codeInput"></param>
		/// <returns></returns>
		private List<NcLineCode.NumCode> Xymake(NcLineCode txtd, List<NcLineCode.NumCode> codeInput) {
			int jj;
			int xy;
			char[] hei2 = new char[2];

			List<NcLineCode.NumCode> codeOutput = new List<NcLineCode.NumCode>();

			if ((txtd.G1 == 0 || txtd.G1 == 1) || txtd.ExistsG(39)) {
				foreach (NcLineCode.NumCode ctmp in codeInput)
					codeOutput.Add(ctmp);
				return codeOutput;
			}

			xy = 0;
			switch (txtd.G2) {
			case 17: hei2[0] = 'X'; hei2[1] = 'Y'; break;
			case 18: hei2[0] = 'Z'; hei2[1] = 'X'; break;
			case 19: hei2[0] = 'Y'; hei2[1] = 'Z'; break;
			default: throw new Exception("qehfcbqebh");
			}
			foreach (NcLineCode.NumCode ctmp in codeInput) {
				if (ctmp.ncChar == hei2[0] && (xy & 1) == 0) xy += 1;			// hei[0](Ｘ)座標があれば１をプラス
				if (ctmp.ncChar == hei2[1] && (xy & 2) == 0) xy += 2;			// hei[1](Ｙ)座標があれば２をプラス
				if (ctmp.ncChar == hei2[0] - 15 && (xy & 4) == 0) xy += 4;	// hei[0]-15(Ｉ)座標があれば４をプラス
				if (ctmp.ncChar == hei2[1] - 15 && (xy & 4) == 0) xy += 4;	// hei[1]-15(Ｊ)座標があれば４をプラス
			}

			//NcLineCode.numCode aa;
			NcLineCode.NumCode bb;
			foreach (NcLineCode.NumCode ctmp in codeInput)
				codeOutput.Add(ctmp);

			// ===== Ｙ座標がない =====
			if ((xy & 6) == 4) {
				for (jj = 0; jj < codeOutput.Count; jj++) {
					if (codeOutput[jj].ncChar == hei2[0] - 15 || codeOutput[jj].ncChar == hei2[1] - 15 || Char.IsLetter(codeOutput[jj].ncChar) == false)
						break;
				}

				switch (hei2[1]) {
				case 'X': bb = new NcLineCode.NumCode(hei2[1], txtd.G9 == 91 ? "0" : txtd.Xyzsf.Xi.ToString(), post.Data[hei2[1]]); break;
				case 'Y': bb = new NcLineCode.NumCode(hei2[1], txtd.G9 == 91 ? "0" : txtd.Xyzsf.Yi.ToString(), post.Data[hei2[1]]); break;
				case 'Z': bb = new NcLineCode.NumCode(hei2[1], txtd.G9 == 91 ? "0" : txtd.Xyzsf.Zi.ToString(), post.Data[hei2[1]]); break;
				default: throw new Exception("aerfwerjfn");
				}
				codeOutput.Insert(jj, bb);
			}
			// ===== Ｘ座標がない =====
			if ((xy & 5) == 4) {
				for (jj = 0; jj < codeOutput.Count; jj++) {
					if (codeOutput[jj].ncChar == hei2[1])
						break;
				}

				switch (hei2[0]) {
				case 'X': bb = new NcLineCode.NumCode(hei2[0], txtd.G9 == 91 ? "0" : txtd.Xyzsf.Xi.ToString(), post.Data[hei2[0]]); break;
				case 'Y': bb = new NcLineCode.NumCode(hei2[0], txtd.G9 == 91 ? "0" : txtd.Xyzsf.Yi.ToString(), post.Data[hei2[0]]); break;
				case 'Z': bb = new NcLineCode.NumCode(hei2[0], txtd.G9 == 91 ? "0" : txtd.Xyzsf.Zi.ToString(), post.Data[hei2[0]]); break;
				default: throw new Exception("aerfwerjfn");
				}
				codeOutput.Insert(jj, bb);
			}
			return codeOutput;
		}

		/// <summary>
		/// 出力ＮＣ文字列を作成する
		/// </summary>
		/// <param name="txtd"></param>
		/// <param name="numd"></param>
		/// <param name="ido">実際に設定された移動量</param>
		/// <returns></returns>
		protected string Txmake(NcLineCode txtd, NcLineCode.NumCode numd, double ido) {
			string ch1, ch2;	// ch2はチェック用
			string chsout;

			int jj;
			//string form;

			chsout = numd.ncChar.ToString();

			//if ((Char.IsUpper(numd.ncchar) == false) != (numd.nccharNo < 1 || numd.nccharNo > 26)) throw new Exception("jewfdbqwebfdqfdqewhb");
			if (!Char.IsUpper(numd.ncChar)) {
				return chsout;
			}

			if (!txtd.B_g100 && !txtd.B_g6) {
				if (numd.ncChar == 'P')	// Ｐコード（ドウェル）
					if (txtd.ExistsG(4) || txtd.G8 != 80)
						ido = Math.Round(ido / 1000.0 / mach.ncCode.TIM);
			}

			// //////////////////
			// 数値の文字数の設定
			// //////////////////

			// 出力フォーマット
			NcDigit machDigit = new NcDigit(mach.ID, mach.ncCode.DGT2);

			int jissuu = 0;
			if (txtd.B_g100) {
				if (numd.decim == true) jissuu = 1;
				// Ｇ１００で小数点付きが可能な機械
				switch (mach.ID) {
				case Machine.MachID.YBM1218V:
					switch (numd.ncChar) {
					case 'G':
					case 'H':
					case 'L':
					case 'M':
					case 'N':
					case 'O':
					case 'P':
					case 'S':
					case 'T':
						break;
					default:
						if (machDigit.Data[numd.ncChar].axis)
							jissuu = 1;
						else
							jissuu = 1000;
						break;
					}
					break;
				case Machine.MachID.YMC430:
					// ユニックスの設定に合わせた 2015/11/10
					if (machDigit.Data[numd.ncChar].axis)
						jissuu = 1;
					break;
				}
			}
			else if (txtd.B_g6 && txtd.G6 != 67) {
				if (numd.decim == true) jissuu = 1;
			}
			else {
				if (numd.decim == true && (mach.ncCode.DEC == null || mach.ncCode.DEC.Value == true)) {
					jissuu = 1;
				}
				if ((mach.ncCode.DEC.HasValue && mach.ncCode.DEC.Value == true) && machDigit.Data[numd.ncChar].axis) jissuu = 1;

			}

			if (numd.ncChar == 'F') {
				;
			}
			else {
				// marume ndgt[0]==1:1000,==10:100,==5:200
				if (machDigit.Data[numd.ncChar].axis && machDigit.Data[numd.ncChar].sdgt.Value != 1000)
					System.Windows.Forms.MessageBox.Show("ＮＣコードの小数点以下の桁数をチェックのこと");
			}

			if (jissuu > 0) {
				//form = "0." + "000000".Substring(0, jj);

				if (machDigit.Data[numd.ncChar].axis) {
					double marume = machDigit.Data[numd.ncChar].sdgt.Value;
					ido = Math.Round(ido * marume) / marume;
					ch2 = ido.ToString("0.0##");
				}
				else {
					ch2 = (ido / jissuu).ToString("0.0##");
				}
				ch1 = ch2;
			}
			else {

				if (machDigit.Data[numd.ncChar].axis) {
					double marume = machDigit.Data[numd.ncChar].sdgt.Value;
					ido = Math.Round(ido * marume) / marume;
					ch1 = ((int)Math.Round(ido * marume)).ToString();
				}
				else
					ch1 = ((int)ido).ToString();

				// G00以外の"0"の場合はそのままとする 2016/12/01
				// S00000でエラーとなるためＰのみに限定する 2016/12/08
				if (numd.ncChar != 'P' || ch1 != "0")
					if ((jj = NcLineCode.Keta(numd.ncChar) - ch1.Length) > 0)
						ch1 = "000000".Substring(0, jj) + ch1;
			}
			chsout += ch1;
			return chsout;
		}
	}
}
