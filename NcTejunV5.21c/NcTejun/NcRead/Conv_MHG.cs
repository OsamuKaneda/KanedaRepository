using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	/// <summary>
	/// ガンドリル用ＮＣデータ変換仕様
	/// </summary>
	class Conv_MHG : Conv
	{
		public Conv_MHG(List<Output.NcOutput.NcToolL> toolList)
			: base(10, false, null, true, toolList) {
		}

		public override OutLine ConvExec() {
			NcLineCode txtd = this.NcQue[0];


			if (txtd.Tcnt < 0) {
				// Ｏ番号のコメントの追加
				if (txtd.B_26('O')) {
					if (NcQue[1].LnumT != 1) throw new Exception("qefbqfrbqhb");
					//txtd.outLine.Addcomment("D" + ((double)(ncQue[1].numList[2].d/1000.0)).ToString("0.0#"));
					txtd.OutLine.Addcomment("D" + TolInfo.Toolset.Diam.ToString("0.0#"));
				}
			}
			else {
				switch (txtd.LnumT) {
				case 1:
					txtd.OutLine.Set("S" + NcoutName.Skog.CutSpinRate().ToString("0000"));
					txtd.OutLine.CommOut = false;
					break;
				default:
					if (txtd.G8 != 80 && txtd.B_g8) {
						if (txtd.NcLine.IndexOf('F') < 0) throw new Exception("ガンドリルの穴あけサイクルのフォーマットが異常　" + txtd.NcLine);
						txtd.OutLine.Set(NcLineCode.NcSetValue(txtd.NcLine, 'F', NcoutName.Skog.CutFeedRate().ToString("00.0")) + "M13");
					}
					else if (txtd.G8 != 80)
						txtd.OutLine.Set(txtd.NcLine + "M13");
					else if (txtd.B_g8)
						txtd.OutLine.Set("");
					else if (txtd.B_p0006)
						txtd.OutLine.Set("G80");
					break;
				}
			}
			return txtd.OutLine;
		}
	}
}
