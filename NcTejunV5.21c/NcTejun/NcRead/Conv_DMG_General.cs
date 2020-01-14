using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	partial class Conv_DMG : Conv
	{
		/// <summary>ＮＣデータの変換仕様を表すストラクチャ</summary>
		protected readonly struct Spec
		{
			/// <summary>変換する位置（最初=true、最後false）</summary>
			public readonly bool start;
			/// <summary>シーケンスNo	有無 （0:無し）</summary>
			public readonly int sequ;
			/// <summary>小数点の有無（無し）</summary>
			public readonly bool decm;
			/// <summary>プラス記号挿入	有無</summary>
			public readonly bool plus;
			/// <summary>行の末尾のコード（無し）</summary>
			public readonly string demi;
			/// <summary>Ｆコードのある行に"G01,2,3"を挿入	有無</summary>
			public readonly bool fg01;
			/// <summary>Ｆコードに比率を掛ける</summary>
			public readonly bool fedr;
			/// <summary>"3D"の時、G00をG01に変換	有無</summary>
			public readonly bool g0_1;
			/// <summary>コメントを追加する</summary>
			public readonly bool comm;
			/// <summary>文字列の変換</summary>
			public readonly List<StrConv> strConvList;
			/// <summary></summary>
			public readonly struct StrConv
			{
				public readonly string str1;	// 変換する文字列
				public readonly string str2;	// 変換後の文字列
				public StrConv(string str1, string str2) {
					this.str1 = str1;
					this.str2 = str2;
				}
			}
			/// <summary>文字列の削除[不変]</summary>
			public readonly List<StrDell> strDellList;
			public readonly struct StrDell
			{
				public readonly string str;	// この文字列が存在する行を削除する
				public StrDell(string str) {
					this.str = str;
				}
			}
			public Spec(string sttend) {
				// 初期化
				demi = "";
				fedr = false;
				strDellList = new List<StrDell>();
				strConvList = new List<StrConv>();

				switch (sttend) {
				case "start":
					start = true;
					decm = true;
					plus = true;
					g0_1 = true;
					fg01 = true;
					sequ = 0;
					comm = false;
					break;
				case "end":
					start = false;
					decm = false;
					plus = false;
					g0_1 = false;
					fg01 = false;
					sequ = 1;
					comm = true;
					//strConvList.Add(new strConv("M98P0006", "% TNC:\\TG\\PROG\\P0006.H"));
					strDellList.Add(new Spec.StrDell("G49"));
					strDellList.Add(new Spec.StrDell("O"));
					break;
				default: throw new Exception("qwefbqehrb");
				}
			}
		}

		/// <summary>ＮＣデータにＧ０１が１度でも存在したらtrue</summary>
		private bool g01ari;

		private void Convert_General(NcLineQue txtd, Spec spec) {

			if (spec.start) {
				// ＮＣデータのチェック
				NcLineCheck(txtd);
				// 行末コード(;)の削除
				txtd.OutLine.Set(txtd.OutLine.Txt.Replace(";", ""));
				// G17,18,19の削除
				txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G17", "").Replace("G18", "").Replace("G19", ""));
				// 不要な工具径補正コードの削除（安全のため）
				if (txtd.B_g4) {
					if (TolInfo.Toolset.MeasTypeDMG == 'F') Fn_g41g42Dcod(txtd); else Fn_g41g42del(txtd);
				}
			}

			// 小数点を挿入する
			if (spec.decm) Fn_decm(txtd);
			// ＋記号を挿入する
			if (spec.plus) Fn_plus(txtd);
			// G00をG01に変換
			if (spec.g0_1) Fn_g0_1(txtd);

			// Ｇ１００の処理(G100T01)
			if (txtd.B_g100 == true) { }
			// 固定サイクルモード
			// カスタムマクロモード
			else if (txtd.G8 != 80 || txtd.G6 != 67) { }
			// 一般モード
			else {
				// Ｆコードのある行に"G01,2,3"を挿入
				if (spec.fg01) if (txtd.B_26('F')) txtd.AddG123();
				// G41,G42 の処理
				if (txtd.B_g4 && txtd.G4 != 40) { }
			}

			// 行削除
			for (int ii = 0; ii < spec.strDellList.Count; ii++)
				if (txtd.NcLine.IndexOf(spec.strDellList[ii].str) >= 0)
					txtd.OutLine.Set("");
			// 文字列変換
			for (int ii = 0; ii < spec.strConvList.Count; ii++)
				txtd.OutLine.Set(txtd.OutLine.Txt.Replace(spec.strConvList[ii].str1, spec.strConvList[ii].str2));
			// 行末コードを変換する
			if (spec.demi.Length != 0) Fn_demi(txtd, spec.demi);
			// シーケンスNoを追加する
			if (spec.sequ > 0) Fn_sequ(txtd);
			// コメントを追加する
			if (spec.comm) Fn_comm(txtd);
		}

		// シーケンスNoを追加する
		protected void Fn_sequ(NcLineCode txtd) {
			// "%"単独行の場合と最初の行は追加しない
			if (txtd.NcLine == "%" || txtd.LnumN == 1)
				return;

			txtd.OutLine.Sequence(ref sequence);
		}

		// G00 を G01 に変換する（G01行にFコードも挿入する）
		// 1 %
		// 2 O0001
		// 3 G100
		// 4 G00G90X0Y0
		// 5 X100Y100
		// 6 G01(G00)Z10??
		//
		// -4 G00Z100
		// -3 M98P0006
		// -2 M30
		// -1 %
		protected void Fn_g0_1(NcLineCode txtd) {
			if (NcoutName.DimensionG01 != 3) return;

			//if (txtd.lnumN <= 4)
			//	return;
			if (txtd.B_g1 && txtd.G1 > 0)
				g01ari = true;
			else if (g01ari == false)
				return;

			if (txtd.B_g1) {
				if (txtd.G1 == 0) {
					txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G00", "G01"));
					txtd.OutLine.Set(txtd.OutLine.Txt + " F" + mach.ncCode.RPD.ToString());
				}
				else if (txtd.B_26('F') == false) {
					//txtd.outLine.Set(txtd.outLine.txt + " F" + txtd.xyzsf.Fi);
					txtd.OutLine.Set(txtd.OutLine.Txt + " F" + (int)NcoutName.Skog.CutFeedRate((double)txtd.Xyzsf.Fi));
					if (txtd.Xyzsf.Fi == 0)
						System.Windows.Forms.MessageBox.Show(
							"NCSPEEDのＮＣデータ寿命分割の仕様などで送り速度が指示されていない箇所があります。" +
							"アプローチ動作を最小送り速度（10mm/min）で処理します。");
				}
			}
			else {
				if (txtd.B_26('F') == true && txtd.G1 == 0)
					txtd.OutLine.Set("G01 " + txtd.OutLine.Txt);
			}
		}

		// 小数点を挿入する
		protected void Fn_decm(NcLineCode txtd) {
			//int inds, inde;
			foreach (char cc in "ABCXYZ".ToCharArray()) {
				if (txtd.B_26(cc)) {
					txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.OutLine.Txt, cc, txtd.Code(cc).dblData.ToString("0.0###")));
				}
			}
		}

		// ＋記号を挿入する
		protected void Fn_plus(NcLineCode txtd) {
			int cnxt;
			cnxt = 0;
			while ((cnxt = txtd.OutLine.Txt.IndexOfAny("ABCIJKUVWXYZ".ToCharArray(), cnxt)) >= 0) {
				cnxt++;
				while (txtd.OutLine.Txt[cnxt] == ' ')
					cnxt++;
				if (txtd.OutLine.Txt[cnxt] == '-')
					continue;
				txtd.OutLine.Set(
					txtd.OutLine.Txt.Substring(0, cnxt) + "+" + txtd.OutLine.Txt.Substring(cnxt));
			}
		}

		/// <summary>
		/// Dxxコードを削除する
		/// </summary>
		/// <param name="txtd"></param>
		protected void Fn_g41g42Dcod(NcLineCode txtd) {
			if (txtd.B_26('D')) {
				txtd.OutLine.Set(NcLineCode.NcDelChar(txtd.OutLine.Txt, 'D'));
			}
		}

		/// <summary>
		/// G41,G42,Dxx のコードを削除する
		/// </summary>
		/// <param name="txtd"></param>
		protected void Fn_g41g42del(NcLineCode txtd) {

			if (!errCheck.Contains("G41G42 " + ncname.Nnam))
				errCheck.Add("G41G42 " + ncname.Nnam);
			// Ｇコードの削除
			if (txtd.OutLine.Txt.IndexOf("G" + txtd.G4.ToString()) < 0) throw new Exception("qwejfbqeh");
			txtd.OutLine.Set(txtd.OutLine.Txt.Replace("G" + txtd.G4.ToString(), ""));
			// Ｄコードの削除
			switch (txtd.G4) {
			case 40:
				break;
			case 41:
			case 42:
				txtd.OutLine.Set(NcLineCode.NcDelChar(txtd.OutLine.Txt, 'D'));
				break;
			}
		}

		// 行末コードを挿入する
		protected void Fn_demi(NcLineCode txtd, string demi) {
			if (txtd.OutLine.Txt.Length == 0)
				return;
			txtd.OutLine.AddString(demi);
		}

		// コメントを追加する
		protected void Fn_comm(NcLineCode txtd) {
			if (txtd.OutLine.Comment == "") return;
			if (txtd.OutLine.Txt.Length > 0)
				txtd.OutLine.Set(txtd.OutLine.Txt + "\t; " + txtd.OutLine.Comment);
		}

		/// <summary>
		/// ＮＣデータ行のチェック
		/// </summary>
		protected virtual void NcLineCheck(NcLineQue txtd) {
			int ii = 0;
			switch (txtd.LnumN) {
			case 1:
				if (txtd.NcLine.IndexOf("%") != 0) ii = 1;
				break;
			case 2:
				if (txtd.NcLine.IndexOf("O") != 0) ii = 2;
				break;
			}
			switch(txtd.LnumT) {
			case 1:
				if (txtd.B_g100 != true) ii = 3;
				break;
			case 2:
				if (txtd.NcLine.IndexOf("G00G90") != 0) ii = 4;
				if (txtd.B_26('X') != true || txtd.B_26('Y') != true) ii = 4;
				break;
			case 3:
				if (txtd.B_26('X') != true || txtd.B_26('Y') != true) ii = 5;
				break;
			}
			switch (txtd.Lnumb) {
			case -3:
				if (!txtd.B_p0006) ii = -3;
				break;
			case -2:
				if (txtd.NcLine.IndexOf("M30") != 0) ii = -2;
				break;
			case -1:
				if (txtd.NcLine.IndexOf("%") != 0) ii = -1;
				break;
			}
			if (ii != 0) {
				throw new Exception(
					$"ＮＣデータ({NcoutName.OutnamNEW}）の{txtd.LnumN}行（{txtd.NcLine}) でＮＣデータのフォーマットエラーが見つかりました code={ii}\n処理を中断しました。");
			}
			// G41,G42 の処理
			if (txtd.B_g4 && txtd.G4 == 41) {
				CamUtil.LogOut.CheckCount("Conv_DMG_General 329", false, "G41はＮＣデータ仕様に現在ありません。アップカットが必要な場合はプログラム修正しますので藤本までご連絡ください.。");
				throw new Exception("G41はＮＣデータ仕様に現在ありません。アップカットが必要な場合はプログラム修正しますので藤本までご連絡ください.。");
			}
			// G80 の処理
			if (txtd.B_g8 || txtd.G8 != 80) {
				if (txtd.B_26('L'))
						if (txtd.Code('L').L != 0)
							throw new Exception("固定サイクル'L' の'L0'以外は未対応です");
			}
		}
	}
}
