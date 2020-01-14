using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamUtil.LCode
{
	/// <summary>
	/// NcNextLine(), passLength.PassLength() を１つのクラスにまとめる。まだ未使用
	/// </summary>
	public class LCode
	{
		/// <summary>ＮＣデータのフォーマット</summary>
		private readonly BaseNcForm ncForm = BaseNcForm.EMPTY;
		/// <summary>入力ＮＣデータの座標値、桁数</summary>
		private readonly NcDigit post;
		/// <summary>工具ごとのクリアランス高さ</summary>
		private readonly double[] apz;
		/// <summary>コメントを出力する場合は true、出力しない場合は false。</summary>
		private readonly bool commentOutput;
		/// <summary>既定のフォーマットに変換されたＮＣデータの場合は true、ＣＡＭから出力されたままでカスタムマクロがG65からG66に変換される前のＮＣデータの場合は false。</summary>
		private readonly bool regular;

		/// <summary></summary>
		private readonly bool passKeisan;
		/// <summary></summary>
		public NcLineCode.NcDist passLength;

		/// <summary></summary>
		public NcLineCode outCode;

		/// <summary>
		/// NcLineCode, NcLineCode.NcDist を計算する LCode を作成する。加工長、加工時間は計算しない
		/// </summary>
		/// <param name="p_apz">開始のＺ座標値。工具ごとに指定します</param>
		/// <param name="baseForm">ＮＣの基本フォーマット名（"GENERAL" or "_5AXIS"）</param>
		/// <param name="p_post">ＣＡＭポストの小数点桁数</param>
		/// <param name="p_commentOutput">出力時にＮＣデータとともにコメントも出力する場合は true</param>
		/// <param name="p_regular">既定のフォーマットに変換されたＮＣデータの場合は true</param>
		public LCode(double[] p_apz, BaseNcForm baseForm, NcDigit p_post, bool p_commentOutput, bool p_regular) {
			this.apz = p_apz;
			this.ncForm = baseForm;
			this.post = p_post;
			this.commentOutput = p_commentOutput;
			this.regular = p_regular;
			this.passKeisan = false;
			this.outCode = new NcLineCode(p_apz, baseForm, p_post, p_commentOutput, p_regular);
			this.passLength = new NcLineCode.NcDist();
		}
		/// <summary>
		/// NcLineCode, NcLineCode.NcDist を計算する LCode を作成する。加工長、加工時間も計算する
		/// </summary>
		/// <param name="p_apz">開始のＺ座標値。工具ごとに指定します</param>
		/// <param name="baseForm">ＮＣの基本フォーマット名（"GENERAL" or "_5AXIS"）</param>
		/// <param name="p_post">ＣＡＭポストの小数点桁数</param>
		/// <param name="p_commentOutput">出力時にＮＣデータとともにコメントも出力する場合は true</param>
		/// <param name="p_regular">既定のフォーマットに変換されたＮＣデータの場合は true</param>
		/// <param name="feedrate">ＮＣデータ内の切削送り速度。加工時の送りではないので注意。加工時間の計算不要の場合はnullで可</param>
		/// <param name="eulerXYZ">座標回転</param>
		public LCode(double[] p_apz, BaseNcForm baseForm, NcDigit p_post, bool p_commentOutput, bool p_regular, double feedrate, Angle3[] eulerXYZ) {
			this.apz = p_apz;
			this.ncForm = baseForm;
			this.post = p_post;
			this.commentOutput = p_commentOutput;
			this.regular = p_regular;
			this.passKeisan = true;
			this.outCode = new NcLineCode(p_apz, baseForm, p_post, p_commentOutput, p_regular);
			this.passLength = new NcLineCode.NcDist(feedrate, eulerXYZ);
		}

		/// <summary>
		/// 新たな１行のＮＣデータで NcLineCode, NcLineCode.NcDist を更新する
		/// </summary>
		/// <param name="readline">ＮＣデータ１行の文字列</param>
		public void NextLine(string readline) {
			outCode.NextLine(readline);
			if (passKeisan) passLength.PassLength(outCode);
		}
	}
}
