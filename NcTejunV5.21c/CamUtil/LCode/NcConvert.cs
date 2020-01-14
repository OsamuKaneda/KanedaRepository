using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil.LCode
{
	/// <summary>
	/// StreamNcR2で使用するＮＣデータ変換のインターフェースです。前後のＮＣデータを保存するプロパティとＮＣデータを変換するメソッドを公開します。
	/// </summary>
	public interface INcConvert
	{
		/// <summary>ＮＣデータが保存されているバッファー</summary>
		NcQueue NcQue { get; }

		/// <summary>
		/// ＮＣデータを変換するメソッドです
		/// </summary>
		/// <returns>複数行のＮＣデータを出力する情報を返します</returns>
		OutLine ConvExec();
	}
}
