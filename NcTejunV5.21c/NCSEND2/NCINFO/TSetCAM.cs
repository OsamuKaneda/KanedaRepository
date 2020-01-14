using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCSEND2.NCINFO
{
	/// <summary>
	/// ツールセットＣＡＭ情報[不変]
	/// </summary>
	public class TSetCAM : CamUtil.ToolSetData.TSetCAM
	{
		/// <summary>データベースで設定された回転数</summary>
		public double Spin { get { return (double)dRowCAM["spin"]; } }
		/// <summary>データベースで設定された送り速度</summary>
		public double Feedrate { get { return (double)dRowCAM["feedrate"]; } }
		/// <summary>データベースで設定された送り速度２</summary>
		public double Feedrate2 { get { return (double)dRowCAM["feedrate2"]; } }

		/// <summary>ツールセットＣＡＭに関連付けられたユニックスの加工情報ファイル登録工具名</summary>
		public string Tnam_kinf { get { return (string)dRowCAM["tool_name_kinf"]; } }
		/// <summary>ツールセットＣＡＭに関連付けられた仮ツールセットの工具情報</summary>
		public readonly CamUtil.ToolSetData.ToolSet toolsetTemp;

		internal TSetCAM(string tsCAM) : base(tsCAM) {
			toolsetTemp = new CamUtil.ToolSetData.ToolSet((string)dRowCAM["tset_name"]);
		}
	}
}
