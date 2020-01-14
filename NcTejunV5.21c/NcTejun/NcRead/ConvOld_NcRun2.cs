using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;
using CamUtil.LCode;

namespace NcTejun.NcRead
{
	/// <summary>
	/// 主型用ＮＣデータの変換。マクロ展開
	/// </summary>
	class ConvOld_NcRun2 : Conv, IDisposable
	{
		/// <summary>加工機情報</summary>
		NcdTool.Mcn1 mach;

		/// <summary>マクロ展開のクラス</summary>
		_main_nctnk nctnk;
		/// <summary>出力</summary>
		List<string> nextNcRead;


		public ConvOld_NcRun2(List<Output.NcOutput.NcToolL> toolList, NcdTool.Mcn1 mach, _main_nctnk p_nctnk)
			: base(5, false, null, true, toolList) {

			this.mach = mach;
			this.nctnk = p_nctnk;
		}

		public override OutLine ConvExec() {
			NcLineCode txtd = this.NcQue[0];

			if (txtd.B_g6 && txtd.G6 != 67) {
				string mac_name = "P" + txtd.G6p.ProgNo.ToString("0000");
				foreach (string stmp in mach.MemMacro)
					if (stmp == mac_name) {
						// メモリ内にマクロが存在するため、マクロ展開不要
						nctnk = null;
						return txtd.OutLine;
					}
				if (mac_name == "P8400") {
					CamUtil.LogOut.CheckCount("ConvOld_NcRun2 042", false, NcdTool.Tejun.TejunName + " P8400マクロが未登録の加工機でのリジッドタップ加工にはまだ対応できていません。");
					throw new Exception("P8400マクロが未登録の加工機でのリジッドタップ加工にはまだ対応できていません。");
				}
				nextNcRead = new List<string>();
				nctnk = new _main_nctnk(ncname.Nnam, mach, nextNcRead, txtd.Xyzsf.ToXYZ(), mac_name);
				nctnk.WriteLine("%;");
			}
			if (nctnk == null) return txtd.OutLine;

			nctnk.WriteLine(txtd.NcLine + ";");
			if (txtd.G6 == 65 || txtd.G6 == 67) {
				nctnk.WriteLine("M02;");
				nctnk.WriteLine("%;");
				nctnk = null;
			}
			foreach (string stmp in nextNcRead)
				txtd.OutLine.AtoAdd(stmp.Replace(";\n", ""));
			nextNcRead.Clear();
			txtd.OutLine.Set("");
			return txtd.OutLine;

		}
		public void Dispose() {
			if (nctnk != null) { nctnk.Dispose(); nctnk = null; }
		}
	}
}
