using System;
using System.Collections.Generic;
using System.Text;

namespace CamUtil.LCode
{
	/// <summary>
	/// NcLineCode をNcQueueで使用するための派生クラスです。
	/// </summary>
	public class NcLineQue : NcLineCode
	{
		/// <summary>
		/// 処理前のキューに入った NcLineQue に後ろから数えた行数 lnumb をセットします。
		/// </summary>
		/// <param name="que"></param>
		static public void LnumbSet(Queue<NcLineQue> que) {
			int ii = -que.Count;
			foreach (NcLineQue temp in que) temp.Lnumb = ii++;
		}
		/////////////////
		// 以上 static //
		/////////////////



		/// <summary>このデータの元となったNcLineQue（Convで連続変換する場合に使用）add in 2016/05/17</summary>
		public NcLineQue source;

		/// <summary>後ろから数えたマイナス値の行数</summary>
		/// <remarks>キューに入れられた後、ファイルエンドとなった時に処理前の行に設定されます。
		/// 最終行が -1で以下 -2, -3 となります。未設定時は０です。</remarks>
		public int Lnumb { get; private set; }

		/// <summary>
		/// 開始行を作成します
		/// </summary>
		/// <param name="p_apz">開始のＺ座標値。工具ごとに指定します</param>
		/// <param name="baseForm">ＮＣの基本フォーマット名（"GENERAL" or "_5AXIS"）</param>
		/// <param name="post">ＣＡＭポストの小数点桁数</param>
		/// <param name="p_commentOutput">出力時にＮＣデータとともにコメントも出力する場合は true</param>
		/// <param name="p_regular">既定のフォーマットに変換されたＮＣデータの場合は true</param>
		public NcLineQue(double[] p_apz, BaseNcForm baseForm, NcDigit post, bool p_commentOutput, bool p_regular)
			: base(p_apz, baseForm, post, p_commentOutput, p_regular) {
			this.source = null;
			this.Lnumb = 0;
		}

		/// <summary>
		/// 前行の情報を元に、指定されたＮＣデータの１行を解析し作成します。元のインスタンスは変更されません。
		/// </summary>
		/// <param name="readline">ＮＣデータの１行</param>
		/// <param name="link">同一行の変換履歴を辿るために保存する、前の変換のNcLineQueへのリンク</param>
		public NcLineQue NextLine(string readline, NcLineQue link) {
			NcLineQue next = (NcLineQue)this.Clone();
			next.source = link;
			next.Lnumb = 0;
			next.NextLine(readline);
			return next;
		}

		/// <summary>
		/// 行頭にG01, G02, G03 を挿入したNcLineCodeを作成します。すでにそれらのＧコードが存在する場合と早送りモードの場合はエラーとなります。
		/// </summary>
		/// <remarks>連続変換するためには[不変]ではなく、インスタンスを変更しなければなりません</remarks>
		public void AddG123() {
			if (B_g1 || G1 == 0) return;
			List<NumCode> aaa = new List<NumCode>();
			aaa.Add(new NumCode('G', "0" + G1.ToString(), post.Data['G']));
			foreach (NumCode num in this.NumList) aaa.Add(num);
			m_numList = new NumList.NumListData(aaa);
			OutLine.Set("G" + G1.ToString("00") + OutLine.Txt);
		}

		/// <summary>このインスタンスと指定した NcLineQue オブジェクトが同じ値を表しているかどうかを示す値を返します。</summary>
		/// <param name="obj">このインスタンスと比較するオブジェクト。</param>
		/// <returns>obj が CamUtil.NcLineQue のインスタンスで、このインスタンスの値に等しい場合は true。それ以外の場合は false。</returns>
		public bool Equals(NcLineQue obj) {
			if (this.Lnumb != obj.Lnumb) return false;
			return base.Equals(obj);
		}
	}
}
