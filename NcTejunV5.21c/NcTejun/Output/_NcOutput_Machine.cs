using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.IO;
using CamUtil;

namespace NcTejun.Output
{
	partial class NcOutput
	{
		/// <summary>イケールとの干渉チェックでのクリアランス値</summary>
		private const double clearance = 1.0;

		/// <summary>
		/// テキサス用ＮＣデータ変換
		/// </summary>
		/// <param name="disp_message"></param>
		/// <param name="mach"></param>
		/// <param name="sw_tck_tno">工具チェックリストに出力した工具リスト</param>
		/// <param name="sw_ind"></param>
		/// <param name="sw_pro"></param>
		/// <param name="sw_tck"></param>
		private void NcOut_TEXAS(Label disp_message, NcdTool.Mcn1 mach, List<int> sw_tck_tno, StreamWriter sw_ind, StreamWriter sw_pro, StreamWriter sw_tck) {

			// インデックスファイルとメインファイル内で使用するプログラムの通し番号
			int pb_ind = 0;

			foreach (NcToolL ncoutName in ncoutList.Tj) {

				Application.DoEvents();
				if (ncoutName.nknum == null && ncoutName.tNodeChecked == false) continue;
				if (ncoutName.nknum != null && ncoutName.nknum.tNodeChecked == false) continue;

				if (ncoutName.tolInfo.Toolset.tset_name == null)
					MessageBox.Show("対応するツールセットが存在しない : " +
						ncoutName.Smch.K2.Tlgn.Toolset.ToolName + " " +
						ncoutName.Smch.K2.Tlgn.Toolset.HolderName + " " +
						ncoutName.Smch.K2.Tlgn.Ttsk);

				// フルファイル名
				string fulName;
				switch (NcdTool.Tejun.BaseNcForm.Id) {
				case BaseNcForm.ID.GENERAL:
				case BaseNcForm.ID.BUHIN:
					fulName = folder + @"\unix\" + ncoutName.Outnam;
					break;
				default:
					throw new Exception("erdbqebdh");
				}
				// ＮＣデータファイルが存在しない場合
				if (ncoutName.nknum == null && !File.Exists(fulName)) {
					MessageBox.Show(
						fulName + " ファイルが存在しません。以降、中止します", "NcOut_TEXAS",
						MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;
				}

				// メッセージの出力
				disp_message.Text = $"{mach.name}仕様への変換中({ncoutName.OutnamNEW})";
				Application.DoEvents();

				// インデックスファイル出力(2)
				pb_ind++;
				// ncoutName.progNoと同じになるとは限らない
				// if (pb_ind != ncoutName.progNo) throw new Exception("arefbreqahfbqreafqew");
				if (Index.IndexMain.dnc)
					IndexFileOutput2(ncoutName, mach, sw_ind, pb_ind);
				// メインファイルの出力
				if (sw_pro != null)
					MainFileOutput(ncoutName.OutnamNEW + mach.Suffix, ncoutName, sw_pro, pb_ind);
				Application.DoEvents();

				// /////////////////////////////
				// ＮＣデータの読み込み書き込み
				// /////////////////////////////
				if (ncoutName.nknum == null) {
					List<NcToolL> toolList = new List<NcToolL>();
					toolList.Add(ncoutName);
					// ＮＣデータの変換と書込み
					string outfulName = MachineConv(toolList, fulName, mach);

					// テンポラリディレクトリのファイルであれば削除（自動分割の場合）
					if (fulName.IndexOf(LocalHost.Tempdir) == 0) {
						if (mach.Dmu == false)
							throw new Exception("wefarefqbreg");
						File.Delete(fulName);
					}
					// 工具チェックファイルの出力（工具単位）
					if (sw_tck != null) TCKFileOutput(outfulName, sw_tck_tno, mach, sw_tck);
				}
			}
			// ///////////////////////////////
			// 自動追加のＮＣデータを作成する
			// ///////////////////////////////
			string progName;
			switch (mach.Axis_Type) {
			case Machine.Machine_Axis_Type.AXIS5_DMU:
				// 掃除ＮＣデータの作成
				progName = "PROPELLER";
				pb_ind++;
				IndexFileOutput3(progName + ".I", sw_ind, sw_pro, pb_ind);
				// 終了ＮＣデータの作成
				progName = "TOOL_CALL_179";
				pb_ind++;
				IndexFileOutput3(progName + ".I", sw_ind, sw_pro, pb_ind);
				if (doji5ax) {
					// 暖機運転（Ｃ軸）データの作成
					progName = "DANKI_C";
					pb_ind++;
					IndexFileOutput3(progName + ".I", sw_ind, sw_pro, pb_ind);
				}
				break;
			case Machine.Machine_Axis_Type.AXIS5_BUHIN:
				ToolSetData.ToolSet mp700 = new ToolSetData.ToolSet("MP700_B");
				progName = "end_processing.ncd";
				pb_ind++;
				string form = "";
				foreach (string ss in File.ReadAllLines(ServerPC.SvrFldrC + Path.ChangeExtension("prog_" + progName, "idx"))) form += ss;
				form = form.Replace("\\r", "\r").Replace("\\n", "\n");
				sw_ind.Write(form, pb_ind.ToString(), progName, (mp700.ToutLength + mp700.HolderLength).ToString("0.00"));
				File.WriteAllLines(folder + @"\" + progName, File.ReadAllLines(
					ServerPC.SvrFldrC + Path.GetFileNameWithoutExtension("prog_" + progName) + "_" + mach.name + ".ncx"));
				break;
			}
		}

		/// <summary>
		/// 非テキサス用ＮＣデータ変換
		/// </summary>
		/// <param name="disp_message"></param>
		/// <param name="mach"></param>
		/// <param name="sw_tck_tno">工具チェックリストに出力した工具リスト</param>
		/// <param name="sw_pro"></param>
		/// <param name="sw_tck"></param>
		private void NcOut_NONTEXAS(Label disp_message, NcdTool.Mcn1 mach, List<int> sw_tck_tno, StreamWriter sw_pro, StreamWriter sw_tck) {

			// インデックスファイルとメインファイル内で使用するプログラムの通し番号
			int pb_ind = 0;

			for (int cntF = 0; cntF < ncoutList.OF_Count(); cntF++) {
				List<NcOutput.NcToolL> toolList = ncoutList.ListInFile(cntF);
				Application.DoEvents();

				int ii = 0;
				while (ii < toolList.Count) {
					if (toolList[ii].nknum == null && toolList[ii].tNodeChecked == false) {
						toolList.RemoveAt(ii);
						continue;
					}
					if (toolList[ii].nknum != null && toolList[ii].nknum.tNodeChecked == false) {
						toolList.RemoveAt(ii);
						continue;
					}
					ii++;
				}
				if (toolList.Count == 0) continue;

				// フルファイル名
				string fulName;
				if (NcdTool.Tejun.BaseNcForm.Id == BaseNcForm.ID.GENERAL)
					fulName = folder + @"\unix\" + toolList[0].Outnam;
				else
					fulName = toolList[0].Ncnam.Ncdata.fulnamePC;
				if (toolList[0].nknum == null && !File.Exists(fulName)) {
					MessageBox.Show(
						fulName + " ファイルが存在しません。以降、中止します", "NcOut_NONTEXAS",
						MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;
				}

				// 出力名のチェック
				foreach (NcToolL ncoutName in toolList) {
					if (toolList[0].OutnamNEW != ncoutName.OutnamNEW)
						throw new Exception("wefdqwebfhbh");
				}

				disp_message.Text = $"{mach.name}仕様への変換中({toolList[0].OutnamNEW})";
				Application.DoEvents();

				// メインファイルの出力
				pb_ind++;
				// 出力しないＮＣデータがある場合、cntF + 1 と同じにならない
				//if (pb_ind != cntF + 1) throw new Exception("arefbreqahfbqreafqew");
				if (sw_pro != null)
					MainFileOutput(toolList[0].OutnamNEW, null, sw_pro, pb_ind);
				Application.DoEvents();

				// /////////////////////////////
				// ＮＣデータの読み込み書き込み
				// /////////////////////////////
				if (toolList[0].nknum == null) {
					string outfulName = MachineConv(toolList, fulName, mach);

					// 工具チェックファイルの出力（工具単位）
					if (sw_tck != null) TCKFileOutput(outfulName, sw_tck_tno, mach, sw_tck);
				}
			}
		}

		/// <summary>
		/// ＮＣデータの変換と出力
		/// </summary>
		/// <param name="toolList"></param>
		/// <param name="fulName">ＮＣデータのフルファイル名</param>
		/// <param name="mach">加工機仕様</param>
		/// <returns>出力名</returns>
		private string MachineConv(List<NcToolL> toolList, string fulName, NcdTool.Mcn1 mach) {
			string outfulName;

			// ストリームリーダー設定
			StreamReader ncsr = null;
			try {
				switch (mach.Axis_Type) {
				case Machine.Machine_Axis_Type.AXIS5_DMU:
					//ncsr = new NcRead.StreamNcR_DMG(fulName, toolList[0]);
					ncsr = new CamUtil.LCode.StreamNcR2(fulName, null,
						(CamUtil.LCode.INcConvert)new NcRead.Conv_DMG(toolList, mach));

					// ncdist : 横孔加工の加工範囲の検証用に使用する。工具軸はすべて０とすること
					if (NcdTool.Tejun.BaseNcForm.Id == BaseNcForm.ID.GENERAL) {
						Angle3[] angs = new Angle3[toolList[0].Skog.Tld.XmlT.MachiningAxisList.Length];
						for (int ii = 0; ii < angs.Length; ii++) angs[ii] = Angle3.a0;
						toolList[0].ncdist = new CamUtil.LCode.NcLineCode.NcDist(null, angs);
					}
					break;
				case Machine.Machine_Axis_Type.AXIS5_VG:
					//ncsr = new NcRead.StreamNcR_VG(fulName, toolList[0]);
					ncsr = new CamUtil.LCode.StreamNcR2(fulName, null,
						(CamUtil.LCode.INcConvert)new NcRead.Conv_VG(toolList, mach));
					break;
				//case Machine.machID.BTU_14_4AX:
				//	//ncsr = new NcRead.StreamNcR_BTU(fulName, toolList);
				//	ncsr = null;
				//	break;
				case Machine.Machine_Axis_Type.AXIS5_BUHIN:
					ncsr = new CamUtil.LCode.StreamNcR2(fulName, null,
						(CamUtil.LCode.INcConvert)new NcRead.Conv_Buhin(toolList, mach),
						(CamUtil.LCode.INcConvert)new NcRead.Conv_Buhin_Aaxis(toolList, mach));
					break;
				default:
					if (mach.ID == Machine.MachID.MHG_1500) {
						//
						// さらにHoles.ConvToFile()でMHG_1500用に変換する（実際に必要な元のＮＣデータの情報はＯ番号のみ）
						string outname = toolList[0].Ncnam.Holes.ConvToFile(fulName, toolList[0].TdatNo);
						//
						ncsr = new CamUtil.LCode.StreamNcR2(LocalHost.Tempdir + "\\" + outname, null,
							(CamUtil.LCode.INcConvert)new NcRead.Conv_MHG(toolList));
						//ncsr = new CamUtil.LCode.StreamNcR2(fulName, ';',
						//	(CamUtil.LCode.NcConvert)new NcRead.Conv_MHG(toolList, toolList[0].ncnam.holes, toolList[0].tdatNo));
					}
					else {
						if (mach.CheckOutput == false)
							throw new Exception("この加工機は出力できません。");
						//ncsr = new NcRead.StreamNcR_General(fulName, toolList, mach.machn, mach.performance == "HI");
						ncsr = new CamUtil.LCode.StreamNcR2(fulName, null,
							(CamUtil.LCode.INcConvert)new NcRead.Conv_Normal(toolList, mach));
					}
					break;
				}

				// ストリームライター設定
				outfulName = folder + @"\" + toolList[0].OutnamNEW + mach.Suffix;

				// ＮＣデータファイル出力
				StreamWriter ncsw = null;
				try {
					ncsw = new StreamWriter(outfulName);
					string outLine;
					while (true) {
						// バッファがいっぱいになって出力があるまでデータを読込む
						outLine = ncsr.ReadLine();
						if (outLine == null)
							break;

						// TrimEndは不要
						//if (outLine.TrimEnd(((CamUtil.LCode.StreamNcR2)ncsr).endCode).Length != outLine.Length) throw new Exception("qewjfbqrfb");
						//outLine = outLine.TrimEnd(((CamUtil.LCode.StreamNcR2)ncsr).endCode);

						if (outLine != "")
							ncsw.WriteLine(outLine);
						Application.DoEvents();
					}
				}
				finally {
					ncsw?.Dispose();
					ncsw = null;
				}
			}
			finally {
				ncsr?.Dispose();
				ncsr = null;
			}

			// 変換元のデータの削除
			if (NcdTool.Tejun.BaseNcForm.Id == BaseNcForm.ID.GENERAL)
				File.Delete(folder + @"\unix\" + toolList[0].Outnam);

			// チェック
			switch (mach.ID) {
			case Machine.MachID.DMU200P:
			case Machine.MachID.DMU210P:
			case Machine.MachID.DMU210P2:
				// イケールとの干渉チェック
				//if (((Output.Joken_Texas)toolList[0].joken).indexMain.progress == "0")
				//	NcDataCheck0(toolList[0]);

				if (Index.IndexMain.progress != "0") {
					switch (toolList[0].index.coordinate) {
					case 2:
					case 3:
						NcDataCheck2(toolList[0]);	// Ｘの＋－のチェック
						break;
					case 4:
					case 5:
					case 6:
					case 7:
						NcDataCheck1(toolList[0]);	// テーブルとの干渉チェック
						NcDataCheck2(toolList[0]);	// Ｘの＋－のチェック
						break;
					}
				}
				break;
			}

			return outfulName;
		}

		/// <summary>
		/// 工具チェック用ＮＣデータの工具単位情報セット
		/// </summary>
		/// <param name="outfulName"></param>
		/// <param name="sw_tck_tno"></param>
		/// <param name="mach"></param>
		/// <param name="sw_tck"></param>
		private void TCKFileOutput(string outfulName, List<int> sw_tck_tno, NcdTool.Mcn1 mach, StreamWriter sw_tck) {
			string ddat;
			int tNo;
			int str0, str1;

			//% FQAK11 G71
			//N1 D00 Q1710 P01 4      ;Version
			//N2 D00 Q1740 P01 7401   ;FTN NUMBER
			//N3 D00 Q1741 P01 201    ;TOOL NO.
			//N4 D00 Q1742 P01 02     ;TOOL TYPE
			//N5 D00 Q1743 P01 225.00 ;TOTAL LENGTH
			//N6 D00 Q1744 P01 10.00  ;TOOL DIAMETER
			//N7 D00 Q1745 P01 01.00  ;TOOL CORNER RADIUS
			//N8 D00 Q1756 P01 202    ;NEXT TOOL NO.
			//N9 D00 Q1764 P01 03200  ;SPIN
			//N10 D00 Q1766 P01 0      ;
			//N11 D00 Q1767 P01 0      ;NC REFERENCE POINT
			//N12 D00 Q1768 P01 1      ;COODINATE MODE(2:FRONT 3:REAR)
			//N13 D00 Q1769 P01 4      ;DANDORI CHECK(1:OMOTE 2:URA)
			//N14 D00 Q1746 P01 3      ;SGI1
			//N15 D00 Q1747 P01 3      ;SGI2
			//N16 D00 Q1765 P01 019    ;RATE
			//N17 D00 Q1761 P01 014    ;Time
			//N18 D00 Q1762 P01 8      ;Coolant
			//N19 D00 Q1763 P01 2      ;Dimension
			//N20 D00 Q1748 P01 50.00  ;Lmax
			//N21 D00 Q1749 P01 0.10   ;Dmax
			//N22 D00 Q1750 P01 0.10   ;Dmin
			//N23 D00 Q1751 P01 0.10   ;qL_after
			//N24 D00 Q1752 P01 0.15   ;qD_after
			//N25 D00 Q1753 P01 225.00 ;MIN_LENGTH
			//N26 D00 Q1754 P01 100.00 ;CLEARANCE_PLANE
			//N27 D00 Q1755 P01 00.000 ;RADIUS_HOSEI
			//N28 D00 Q1757 P01 00.000 ;LENGTH_HOSEI
			//N29 % TNC:\TG\PROG\JOKEN.H
			//N30 % TNC:\TG\PROG\G100.H

			using (StreamReader sr = new StreamReader(outfulName)) {
				if (mach.Dmu) {
					string TNO_LINE = "D00 Q1741 P01 ";
					while (sr.EndOfStream == false) {
						ddat = sr.ReadLine();
						if (ddat[0] == '%') continue;

						if (ddat.IndexOf(TNO_LINE) >= 0) {
							str0 = ddat.IndexOf(TNO_LINE) + TNO_LINE.Length;
							str1 = ddat.IndexOf(";", str0);
							if (str1 < 0) str1 = ddat.Length;
							tNo = Convert.ToInt32(ddat.Substring(str0, str1 - str0));
							if (sw_tck_tno.Contains(tNo) == true)
								break;
							sw_tck_tno.Add(tNo);
						}
						sw_tck.WriteLine(ddat);
						if (ddat.IndexOf(@"TNC:\TG\PROG\G100") >= 0)
							break;
					}
				}
				else {
					while (sr.EndOfStream == false) {
						ddat = sr.ReadLine();
						if (!regex.IsMatch(ddat)) continue;
						//if (ddat.IndexOf("G100T") < 0) continue;

						str0 = ddat.IndexOf('T') + 1;
						str1 = ddat.IndexOfAny("ABCDEFHIJKLMNOPQRSUVWXYZ;".ToCharArray());
						tNo = Convert.ToInt32(ddat.Substring(str0, str1 - str0));
						if (sw_tck_tno.Contains(tNo) == false) {
							sw_tck_tno.Add(tNo);
							if (mach.ID == Machine.MachID.LineaM || mach.ID == Machine.MachID.D500)
								sw_tck.WriteLine(ddat.Replace("M0", "M90").Replace("M1", "M90"));
							else
								sw_tck.WriteLine(ddat);
						}
						if (NcdTool.Tejun.Mach.Toool_nc)
							break;
					}
				}
			}
		}

		/// <summary>
		/// インデックスファイル、メインファイルの工具単位情報を出力する
		/// </summary>
		/// <param name="ncoutName"></param>
		/// <param name="mach"></param>
		/// <param name="sw_ind"></param>
		/// <param name="pb_ind"></param>
		private void IndexFileOutput2(NcToolL ncoutName, NcdTool.Mcn1 mach, StreamWriter sw_ind, int pb_ind) {
			string programName = ncoutName.OutnamNEW + mach.Suffix;

			sw_ind.WriteLine("program" + pb_ind.ToString() + ":" + programName);
			sw_ind.Write("(" + "type=" + "1");
			sw_ind.Write("," + "ftn=" + ncoutName.tolInfo.Toolset.ID);

			// ＴＥＸＡＳはRATEを整数として受け取るので整数に丸める
			//ind_sw.Write("," + "rate=" + joken.index.life_rate.ToString("0.00"));
			switch (mach.ID) {
			case Machine.MachID.LineaM:
			case Machine.MachID.D500:
				sw_ind.Write("," + "rate=" + ncoutName.index.Life_rate.ToString("0000"));
				sw_ind.Write("," + "time=" + ncoutName.index.atime.ToString("000"));
				break;
			default:
				sw_ind.Write("," + "rate=" + ncoutName.index.Life_rate.ToString("0"));
				sw_ind.Write("," + "time=" + ncoutName.index.atime.ToString("0"));
				break;
			}
			sw_ind.WriteLine(")");
			sw_ind.Write("(" + "t=" + ncoutName.index.tno.ToString("00"));
			//if (Program.tejun.mach.machn == Machine.DMU200P)
			//	sw_ind.Write("," + "h=" + joken.index.h.ToString("00"));
			if (ncoutName.tolInfo.Toolset.M0304 != "M04")
				sw_ind.Write("," + "spin=" + ncoutName.index.spin.ToString("00000"));
			else
				sw_ind.Write("," + "spin=" + ncoutName.index.spin.ToString("-00000"));
			sw_ind.Write("," + "feed=" + ncoutName.index.feed.ToString("00000"));
			sw_ind.Write(",tnam=" + ncoutName.tolInfo.Toolset.ToolName);

			if (mach.Dmu) {
				if (ncoutName.index.next > 0)
					sw_ind.Write("," + "q=" + ncoutName.index.next.ToString("00"));
				sw_ind.Write("," + "ttype=" + ncoutName.index.type.ToString());
				sw_ind.Write("," + "diam=" + ncoutName.tolInfo.Toolset.Ex_diam_Index.ToString("0.00"));
				sw_ind.Write("," + "crad=" + ncoutName.tolInfo.Toolset.Crad.ToString("0.0##"));
				//sw_ind.Write("," + "toth=" + joken.index.toth.ToString());
				//sw_ind.Write("," + "length=" + joken.index.length.ToString("0.00"));
				//sw_ind.Write("," + "refpoint=" + joken.index.refpoint.ToString("0"));
				//sw_ind.Write("," + "coordinate=" + joken.index.coordinate.ToString("0"));
				//sw_ind.Write("," + "dandori=" + joken.indexMain.progress);
				sw_ind.Write(",Lmax=" + ncoutName.tolInfo.Toolset.TolLmax.ToString("00.00"));
				sw_ind.Write(",qL_after=" + ncoutName.tolInfo.TsetCAM.Tol_L_After.ToString("0.00"));
				sw_ind.Write(",qD_after=" + ncoutName.tolInfo.TsetCAM.Tol_D_After.ToString("0.00"));
				sw_ind.Write(",cl_plane=" + ncoutName.index.clearance_plane.ToString("000.00"));// ２次元加工のＺの逃げの位置に使用
				sw_ind.Write(",sgi1=" + ncoutName.index.sgi1.ToString("0"));
				sw_ind.Write(",sgi2=" + ncoutName.index.sgi2.ToString("0"));
			}
			else {
				switch (mach.ID) {
				case Machine.MachID.D500:
				case Machine.MachID.LineaM:
					if (ncoutName.Ncnam.Ncdata.ncInfo.xmlD.PostProcessor.Id == PostProcessor.ID.MES_AFT_BU) {
						sw_ind.Write(",g100=100");
						sw_ind.Write(",p6=7");
					}
					else {
						sw_ind.Write(",g100=100");
						sw_ind.Write(",p6=6");
					}
					break;
				default:
					if (ncoutName.tolInfo.Toolset.Probe) {
						// 計測の場合
						//sw_ind.Write(",g100=101");
						sw_ind.Write(",g100=100");
						sw_ind.Write(",p6=7");
					}
					else {
						sw_ind.Write(",g100=100");
						sw_ind.Write(",p6=6");
					}
					break;
				}
				sw_ind.Write("," + "scal=" + ncoutName.index.sgi1.ToString("0"));
			}
			if (ncoutName.index.hosei_r.HasValue)
				sw_ind.Write("," + "r_hosei=" + ncoutName.index.hosei_r.Value.ToString("0.0###"));
			if (ncoutName.index.hosei_l.HasValue)
				sw_ind.Write("," + "l_hosei=" + ncoutName.index.hosei_l.Value.ToString("0.0###"));

			sw_ind.Write("," + "coolant=" + ncoutName.index.coolant.ToString("0"));
			sw_ind.Write("," + "dimension=" + ncoutName.index.dimensionAXIS.ToString("0"));
			if (mach.ID == Machine.MachID.D500 || mach.ID == Machine.MachID.LineaM)
				sw_ind.Write("," + "mirror=0");
			else
				sw_ind.Write("," + "mirror=2");
			if (mach.ID == Machine.MachID.D500 || mach.ID == Machine.MachID.LineaM) {
				sw_ind.Write("," + "told_min=" + (-ncoutName.tolInfo.Toolset.TolDmin).ToString("0.0##"));
				sw_ind.Write("," + "told_max=" + ncoutName.tolInfo.Toolset.TolDmax.ToString("0.0##"));
				sw_ind.Write("," + "tlen_min=" + ncoutName.tolInfo.Min_length.ToString("0.00"));
			}
			else {
				sw_ind.Write("," + "told_min=" + ncoutName.tolInfo.Toolset.TolDmin.ToString("0.0##"));
				sw_ind.Write("," + "told_max=" + ncoutName.tolInfo.Toolset.TolDmax.ToString("0.0##"));
				sw_ind.Write("," + "tlen_min=" + ncoutName.tolInfo.Min_length.ToString("0"));
			}
			switch (mach.ID) {
			case Machine.MachID.LineaM:
			case Machine.MachID.D500:
				/*
				・軸方向のシフト量
					①　ＤＢ[cutter_form].[径測定識別記号] が"A"あるいは"D"の場合は
					　　プログラム	1.0 + (直径)/2.0 * tan(30)
						D500		1.0 + (直径)/2.0 * tan(30)
						LineaM		0.2 + (直径)/2.0 * tan(30)
					②　ＤＢ[cutter_form].[刃具形状タイプ] が"RMIL"あるいは"EMIL"あるいは"FMIL"の場合は
					　　プログラム	1.0 + (コーナーＲ)
						D500		IF (直径) >= 10.0 THEN 4.0 ELSE 2.0
						LineaM		1.0 + (コーナーＲ)
					③　ＤＢ[cutter_form].[径測定識別記号] が"B"の場合は
					　　プログラム	1.0 + (直径)/2.0
						D500		IF (直径) > 2.0 THEN 1.0 + (直径)/2.0 ELSE 1.0(D500は必ず1.0以上)
						LineaM		0.2 + (直径)/2.0
					④　ＤＢ[cutter_form].[径測定識別記号] が"Q"の場合は
						プログラム	5.0
					　　D500		6.0（径の測定はしない）
						LineaM		5.0（径を測定している？）
					⑤　ＤＢ[cutter_form].[径測定識別記号] が"R"の場合は
						プログラム	5.0
						D500		6.0
						LineaM		3.0
					⑥　ＤＢ[cutter_form].[径測定識別記号] が"V", "J"の場合は径は測らない
				・参考	刃具形状タイプ"RMIL"はラットであり径測定識別記号は"B"である
				*/
				// r_shift は使用されている
				sw_ind.Write("," + "r_shift=" + ncoutName.tolInfo.Toolset.Shift_r(mach.ToolMeasureType).ToString("0.0##"));
				// l_shift はまだ使用されず、各加工機のカスタムマクロで計算されている。（上記コメント参照）
				double? l_shift = ncoutName.tolInfo.Toolset.Shift_l();
				if (l_shift.HasValue) sw_ind.Write("," + "l_shift=" + l_shift.Value.ToString("0.0##"));
				break;
			default:
				sw_ind.Write("," + "r_shift=" + ncoutName.tolInfo.Toolset.Shift_r(mach.ToolMeasureType).ToString("0.0##"));
				break;
			}

			switch (mach.ID) {
			case Machine.MachID.DMU200P:
			case Machine.MachID.DMU210P:
			case Machine.MachID.DMU210P2:
				break;
			case Machine.MachID.LineaM:
			case Machine.MachID.D500:
				sw_ind.Write("," + "cradius=" + ncoutName.tolInfo.Toolset.Crad.ToString("0.000"));
				sw_ind.Write("," + "ftn2=" + ncoutName.tolInfo.Toolset.ID);
				sw_ind.Write("," + "time2=" + ncoutName.index.atime.ToString("000"));
				sw_ind.Write("," + "doji5=" + ncoutName.index.simultaneous.ToString("0"));
				break;
			default:
				sw_ind.Write("," + "cradius=" + ncoutName.tolInfo.Toolset.Crad.ToString("0.0##"));
				//稼働時間自動収集のために追加 2014/09/29
				sw_ind.Write("," + "ftn2=" + ncoutName.tolInfo.Toolset.ID);
				sw_ind.Write("," + "time2=" + ncoutName.index.atime.ToString("0"));
				//ＳＬ値のために追加 2015/01/28
				if (ncoutName.tolInfo.Toolset.Torque.HasValue && mach.Torque.HasValue)
					sw_ind.Write("," + "sl=" + Math.Ceiling(ncoutName.tolInfo.Toolset.Torque.Value / mach.Torque.Value * 100.0).ToString("00"));
				else
					sw_ind.Write("," + "sl=0");
				// 加工後の精度追加 2017/12/14
				sw_ind.Write(",qL_after=" + ncoutName.tolInfo.TsetCAM.Tol_L_After.ToString("0.00"));
				sw_ind.Write(",qD_after=" + ncoutName.tolInfo.TsetCAM.Tol_D_After.ToString("0.00"));
				break;
			}
			sw_ind.WriteLine(")");
		}
		private void MainFileOutput(string progName, NcToolL ncoutName, StreamWriter sw_pro, int pb_ind) {
			switch (NcdTool.Tejun.Mach.ID) {
			// //////////////////////
			// HAIDENHEINのメインファイル
			// //////////////////////
			case Machine.MachID.DMU200P:
			case Machine.MachID.DMU210P:
			case Machine.MachID.DMU210P2:
				// ＮＣデータのコール文挿入
				sw_pro.Write("N{0,-2:d} % {1}", pb_ind.ToString(), progName);
				if (ncoutName != null)
					sw_pro.Write("\t; T{0:000} {1} {2:000}min", ncoutName.index.tno, ncoutName.tolInfo.Toolset.ToolName, ncoutName.index.atime);
				sw_pro.WriteLine();
				break;
			// //////////////////////
			// μDMSのメインファイル
			// //////////////////////
			case Machine.MachID.V77:
			//case Machine.machID.SNC106:
				// %
				// O0001
				// (+INC C:\NcData\070b_j002-cc_ed05l_01.dnc +)
				// (+INC C:\NcData\掃除\cleantec.dnc +)
				// (+INC C:\NcData\070b_j002-cc_ed05l_02.dnc +)
				// (+INC C:\NcData\070b_j002-cc_ed05l_03.dnc +)
				// M30
				// %
				sw_pro.WriteLine("(+INC C:\\NcData\\{0}.dnc +)", progName);
				if (pb_ind == 1)
					sw_pro.WriteLine("(+INC C:\\NcData\\掃除\\cleantec.dnc +)");
				break;
			default: throw new Exception("qjewfbqhreb");
			}
		}

		/// <summary>
		/// 自動追加のインデックスファイル、メインファイル、ＮＣデータの作成
		/// </summary>
		/// <param name="progName"></param>
		/// <param name="sw_ind"></param>
		/// <param name="sw_pro"></param>
		/// <param name="pb_ind"></param>
		private void IndexFileOutput3(string progName, StreamWriter sw_ind, StreamWriter sw_pro, int pb_ind) {
			string form;
			// INDEX FILE
			if (Index.IndexMain.dnc) {
				form = "";
				foreach (string ss in File.ReadAllLines(ServerPC.SvrFldrC + Path.ChangeExtension("prog_" + progName, "idx"))) form += ss;
				form = form.Replace("\\r", "\r").Replace("\\n", "\n");
				//MessageBox.Show(String.Format(form, pb_ind.ToString(), progName + ".I"));
				sw_ind.Write(form, pb_ind.ToString(), progName);
			}
			// MAIN FILE
			if (sw_pro != null) {
				sw_pro.WriteLine("N{0,-2:d} % {1}", pb_ind.ToString(), progName);
			}
			// NC DATA
			StreamWriter ncsw2 = new StreamWriter(folder + @"\" + progName);
			foreach (string ss in File.ReadAllLines(ServerPC.SvrFldrC + Path.ChangeExtension("prog_" + progName, "ncx")))
				ncsw2.WriteLine(ss, Path.GetFileNameWithoutExtension(progName), Index.IndexMain.progress);
			ncsw2.Close();
			return;
		}

		/// <summary>
		/// 終了ＮＣデータのインデックスファイル、メインファイル（ＤＭＧのみ）
		/// </summary>
		/// <param name="progName"></param>
		/// <param name="sw_ind"></param>
		/// <param name="pb_ind"></param>
		private void IndexFileOutput2(string progName, StreamWriter sw_ind, int pb_ind) {
			string programName = progName + ".I";
			sw_ind.WriteLine("program" + pb_ind.ToString() + ":" + programName);
			sw_ind.Write("(" + "type=" + "1");
			sw_ind.Write("," + "ftn=0510");
			sw_ind.Write("," + "rate=0");
			sw_ind.Write("," + "time=0");
			sw_ind.WriteLine(")");

			sw_ind.Write("(" + "t=179");
			//sw_ind.Write("," + "h=179");
			sw_ind.Write("," + "spin=1000");
			sw_ind.Write("," + "feed=1000");
			sw_ind.Write(",tnam=GAUGE-TOOL");
			sw_ind.Write("," + "q=0");
			sw_ind.Write("," + "type=10");
			sw_ind.Write("," + "diam=12.00");
			sw_ind.Write("," + "crad=0.0");
			//sw_ind.Write("," + "toth=0");
			//sw_ind.Write("," + "length=130.00");
			//sw_ind.Write("," + "refpoint=0");
			//sw_ind.Write("," + "coordinate=0");
			//sw_ind.Write("," + "dandori=1");
			sw_ind.Write(",Lmax=50.0");
			sw_ind.Write(",qL_after=0.01");
			sw_ind.Write(",qD_after=0.01");
			sw_ind.Write(",cl_plane=500.00");
			sw_ind.Write("," + "sgi1=3");
			sw_ind.Write("," + "sgi2=3");
			sw_ind.Write("," + "r_hosei=0.0");
			sw_ind.Write("," + "l_hosei=0.0");
			sw_ind.Write("," + "coolant=9");
			sw_ind.Write("," + "dimension=2");
			sw_ind.Write("," + "mirror=2");
			sw_ind.Write("," + "told_min=0.01");
			sw_ind.Write("," + "told_max=0.01");
			sw_ind.Write("," + "tlen_min=10");
			sw_ind.Write("," + "r_shift=0.0");
			sw_ind.WriteLine(")");
		}

		/// <summary>
		/// テーブルとの干渉チェック（側面加工）
		/// </summary>
		private void NcDataCheck1(NcToolL ncoutName) {
			// Ｙ座標値のチェック
			if (ncoutName.ncdist.Min.Value.Y < -30.0) {
				MessageBox.Show("テーブルと干渉します。" +
					" NCNAME=" + ncoutName.Ncnam.nnam +
					" TOOLNAME=" + ncoutName.tolInfo.Toolset.ToolName +
					" Y_Min=" + ncoutName.ncdist.Min.Value.Y.ToString(),
					"テーブルとの干渉チェック", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				if (ProgVersion.Debug)
					MessageBox.Show("debug:テーブルと干渉します。");
				else
					throw new Exception("テーブルと干渉します。");
			}
		}
		/// <summary>
		/// Ｘの＋－のチェック
		/// </summary>
		private void NcDataCheck2(NcToolL ncoutName) {
			if (ncoutName.index.XPLUSMINUS() > 0) {
				if (ncoutName.ncdist.Min.Value.X + ncoutName.ncdist.Max.Value.X < 0.0 || ncoutName.ncdist.Min.Value.X < -10.0) {
					DialogResult result = MessageBox.Show(
						$"{ncoutName.Ncnam.nnam}の{ncoutName.Skog.Tld.SetJun.ToString()}番目の工具（{ncoutName.Smch.K2.Tlgn.Toolset.ToolName}）の" +
						$"最小Ｘ加工位置（X={ncoutName.ncdist.Min.Value.X}）は型から外れている可能性があります。",
						"NcOut_DMU200P", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
					if (result == DialogResult.Cancel)
						throw new Exception("Ｘ座標値の＋－チェックエラー");
				}
			}
			else {
				if (ncoutName.ncdist.Min.Value.X + ncoutName.ncdist.Max.Value.X > 0.0 || ncoutName.ncdist.Max.Value.X > 10.0) {
					DialogResult result = MessageBox.Show(
						$"{ncoutName.Ncnam.nnam}の{ncoutName.Skog.Tld.SetJun.ToString()}番目の工具（{ncoutName.Smch.K2.Tlgn.Toolset.ToolName}）の" +
						$"最大Ｘ加工位置（X={ncoutName.ncdist.Max.Value.X}）は型から外れている可能性があります。",
						"NcOut_DMU200P", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
					if (result == DialogResult.Cancel)
						throw new Exception("Ｘ座標値の＋－チェックエラー");
				}
			}
		}
	}
}
