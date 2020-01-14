using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	class Conv_Buhin_Aaxis : Conv
	{
		/// <summary>加工機情報</summary>
		private NcdTool.Mcn1 mach;

		public Conv_Buhin_Aaxis(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach)
			: base(10, false, null, true, toolList) {
			this.mach = mach;
		}

		public override OutLine ConvExec() {
			NcLineCode txtd = this.NcQue[0];

			if (ncname.Ncdata.ncInfo.xmlD.PostProcessor.Id == CamUtil.PostProcessor.ID.MES_BEF_BU) {

				//================================
				//Ａ軸補正の不具合修正 2006.07.28
				//Ａ軸補正のための追加 2006.04.17
				//================================
				if (txtd.OutLine.Txt.IndexOf("G65P8730") == 0)
					if (mach.ID == CamUtil.Machine.MachID.LineaM)
						txtd.OutLine.Set(txtd.OutLine.Txt + MomentSet());
				if (txtd.OutLine.Txt.IndexOf("G65P8700") == 0)
					txtd.OutLine.Set("");
			}

			return txtd.OutLine;
		}

		// //////////// //
		//    軸補正    //
		// //////////// //

		//横幅 Format(SWork.Sheets(WS_csv).Range(CSV_SZX).Value, "0.0")
		//奥行き Format(SWork.Sheets(WS_csv).Range(CSV_SZY).Value, "0.0")
		//高さ Format(SWork.Sheets(WS_csv).Range(CSV_SZZ).Value, "0.0")
		//ベースタイプ UserForm1.BaseType.List(UserForm1.BaseType.ListIndex, 0) & "(" & UserForm1.BaseType.Value & ")"

		//    BaseType.List(2, 0) = "主入子０７２タイプ"
		//    BaseType.List(2, 1) = 72#
		//    BaseType.AddItem
		//    BaseType.List(3, 0) = "主入子１５０タイプ"
		//    BaseType.List(3, 1) = 150#
		//    BaseType.AddItem

		private string MomentSet() {

			int weight;			//Kg
			int length;			//mm

			double VL;			//mm3*mm
			double Volume_Wrk;	//mm3
			double Length_Wrk;	//mm
			double Volume_Leg;	//mm3
			double leg_length = IndexMain.baseHeight;	// ベースの高さ(mm)

			if (Math.Abs(leg_length - 72.0) <= 0.1) {
				Volume_Leg = (40.0 / 2) * (40.0 / 2) * 3.14 * leg_length * 4;
				VL = Volume_Leg * (leg_length / 2);
			}
			else if (Math.Abs(leg_length - 150.0) <= 0.1) {
				Volume_Leg = (30.0 / 2) * (30.0 / 2) * 3.14 * leg_length * 4;
				VL = Volume_Leg * (leg_length / 2);
			}
			else
				return "";

			double CSV_SZX = IndexMain.mold_X;				//素材横幅
			double CSV_SZY = IndexMain.mold_Y;				//素材奥行き
			double CSV_SZZ = IndexMain.Height - leg_length;	//素材高さ

			Volume_Wrk = CSV_SZX * CSV_SZY * CSV_SZZ;
			Length_Wrk = leg_length + (CSV_SZZ / 2);

			VL = VL + Volume_Wrk * Length_Wrk;
			weight = (int)Math.Round(7.8 * (Volume_Wrk + Volume_Leg) / 1000 / 1000);
			length = (int)Math.Round(VL / (Volume_Wrk + Volume_Leg));

			return "W" + weight.ToString() + ".D" + length.ToString() + ".";
		}
	}
}
