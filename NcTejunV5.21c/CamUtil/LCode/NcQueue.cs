using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
namespace CamUtil.LCode
{
	/// <summary>
	/// ＮＣデータ情報（NcLineCode）処理行の前後を Queue&gt;NcLineCode&lt; を用いて保存します。
	/// </summary>
	public class NcQueue
	{
		/// <summary>
		/// キューの状態を表します
		/// </summary>
		public enum QUEINFO
		{
			/// <summary>キューへ蓄積中で出力なし</summary>
			NOTFULL = -1,
			/// <summary>入出力正常</summary>
			NORMAL = 0,
			/// <summary>キューが空になり出力行がない（終了）</summary>
			EMPTY = 1
		}

		/// <summary>キューに入れておくＮＣデータの前後それぞれの最大行数。処理後の行数には現在行も含まれます</summary>
		private readonly int queMaxNum;

		/// <summary>処理前のＮＣデータを保存するキュー</summary>
		protected Queue<NcLineQue> ncline_Bef;
		/// <summary>処理後のＮＣデータを保存するキュー</summary>
		protected Queue<NcLineQue> ncline_Aft;

		/// <summary>現在処理中のＮＣデータ行</summary>
		protected NcLineQue ncline_Now;
		/// <summary>最後に取込んだＮＣデータ行</summary>
		private NcLineQue ncCodeLast;

		/// <summary>this[], NcPeek()呼出しで使用可能な最大値。処理前のＮＣデータ参照の行数</summary>
		public int QueueMax { get { return ncline_Bef.Count; } }
		/// <summary>this[], NcPeek()呼出しで使用可能な最小値。１から処理後のＮＣデータ参照の行数を引いた値</summary>
		public int QueueMin { get { return 1 - ncline_Aft.Count; } }

		/// <summary>
		/// 処理前後のＮＣデータを返します。処理前の最大行数は queMaxNum 、処理後は queMaxNum-1 です。
		/// </summary>
		/// <param name="index">処理前後のＮＣデータの位置。０は現在行で QueueMin 以上、 QueueMax 以下</param>
		/// <returns>指定された行のＮＣデータ</returns>
		public NcLineQue this[int index] { get { return NcPeek(index); } }

		/// <summary>ダミーが入っている場合は true</summary>
		private readonly bool dummyData;	// ダミーが入らないように修正が必要（暫定処置）

		/// <summary>
		/// NCSEND2 用に作成します
		/// </summary>
		/// <param name="queMaxNum">保存するＮＣデータの前後それぞれの最大行数</param>
		public NcQueue(int queMaxNum) {
			this.queMaxNum = queMaxNum;
			ncline_Bef = new Queue<NcLineQue>(this.queMaxNum + 1);
			ncline_Aft = new Queue<NcLineQue>(this.queMaxNum + 1);
			ncline_Now = ncCodeLast = null;
			this.dummyData = true;
		}
		/// <summary>
		/// 初期値を指定して NcQueue を作成します
		/// </summary>
		/// <param name="queMaxNum">保存するＮＣデータの前後それぞれの最大行数</param>
		/// <param name="nconly">ＮＣコード以外の情報を消去する場合は true</param>
		/// <param name="apz">最初の工具のＺ座標値</param>
		/// <param name="baseForm">ＮＣの基本フォーマット</param>
		/// <param name="post">ＣＡＭポストの小数点桁数</param>
		/// <param name="commentOutput">コメントを出力する場合は true</param>
		/// <param name="p_regular">既定のフォーマットに変換されたＮＣデータの場合は true</param>
		public NcQueue(int queMaxNum, bool nconly, double[] apz, BaseNcForm baseForm, NcDigit post, bool commentOutput, bool p_regular) {
			this.queMaxNum = queMaxNum;
			ncline_Bef = new Queue<NcLineQue>(this.queMaxNum + 1);
			ncline_Aft = new Queue<NcLineQue>(this.queMaxNum + 1);
			ncline_Now = null;
			ncCodeLast = new NcLineQue(apz, baseForm, post, commentOutput, p_regular);
			this.dummyData = false;
		}

		/// <summary>
		/// ＮＣデータ１行のキューの処理を実行します。入力は ncCodeLast に、出力は ncline_Now に入ります。
		/// </summary>
		/// <param name="ncCode">入力するＮＣデータの情報</param>
		/// <returns>キューの状態（NOTFULL:キューへ蓄積中、NORMAL:入出力正常、EMPTY:キューが空）</returns>
		public QUEINFO NcDeque(NcLineQue ncCode) {
			ncCodeLast = ncCode;
			return NcDeque();
		}
		/// <summary>
		/// ＮＣデータ１行のキューの処理を実行します。入力として ncCodeLast を用い、出力は ncline_Now です。 ncCodeLast は変化しません。
		/// </summary>
		/// <returns>キューの状態（NOTFULL:キューへ蓄積中、NORMAL:入出力正常、EMPTY:キューが空）</returns>
		private QUEINFO NcDeque() {
			if (ncCodeLast != null) {
				ncline_Bef.Enqueue(ncCodeLast);
				if (ncline_Bef.Count <= queMaxNum) {
					ncline_Now = null;
					//ncCodeLast = ncCode;
					return QUEINFO.NOTFULL;
				}
			}

			// 後ろから数えた行数をセットする
			if (ncCodeLast == null && ncline_Bef.Count > 0) {
				if (ncline_Bef.Peek().Lnumb == 0)
					NcLineQue.LnumbSet(ncline_Bef);
			}

			if (ncline_Bef.Count == 0) {
				ncline_Now = null;
				return QUEINFO.EMPTY;
			}
			ncline_Now = ncline_Bef.Dequeue();
			ncline_Aft.Enqueue(ncline_Now);
			if (ncline_Aft.Count > queMaxNum)
				ncline_Aft.Dequeue();
			return QUEINFO.NORMAL;
		}

		/// <summary>
		/// 処理前後のＮＣデータを返します。処理前の最大行数は queMaxNum 、処理後は queMaxNum-1 です。
		/// </summary>
		/// <param name="suf">処理前後のＮＣデータの位置。０は現在行で QueueMin 以上、 QueueMax 以下</param>
		/// <returns>指定された行のＮＣデータ</returns>
		public NcLineQue NcPeek(int suf) {
			if (suf == 0) {
				return ncline_Now;
			}
			else if (suf > 0) {
				int ii = 1;
				foreach (NcLineQue temp in ncline_Bef) {
					if (ii == suf)
						return temp;
					ii++;
				}
				throw new Exception($"NcPeek ERROR suf={suf}  cnout={ncline_Bef.Count}");
			}
			else if (suf < 0) {
				int ii = 1 - ncline_Aft.Count;
				foreach (NcLineQue temp in ncline_Aft) {
					if (ii == suf)
						return temp;
					ii++;
				}
				throw new Exception($"NcPeek ERROR suf={suf}  cnout={ncline_Aft.Count}");
			}
			throw new Exception("NcPeek ERROR suf=" + suf);
		}

		/// <summary>
		/// 指定されたストリームからＮＣデータを取得解析し、前行の情報 ncCodeLast を最新情報に更新する。
		/// その後キューの処理を実行し、処理前キューがいっぱいになり現在行 ncline_Now が出力されるまで繰り返す。
		/// </summary>
		/// <returns>現在行ＮＣデータ</returns>
		public NcLineQue NextLine(StreamReader sr) {
			CamUtil.LCode.NcQueue.QUEINFO iread;
			string readline;

			// ＮＣデータの出力が存在するまで読み込みを繰り返す
			while (true) {
				if (sr.EndOfStream)
					ncCodeLast = null;
				else {
					// 次のＮＣデータ１行の情報NcCodeを作成する
					readline = sr.ReadLine();
					if (Skip(readline, sr)) continue;
					ncCodeLast = ncCodeLast.NextLine(readline, null);
				}

				// キューへの保存と取り出し
				// iread == NOTFULL	: キューへ情報蓄積中
				// iread == EMPTY	: 終了（キューが空）
				// iread == NORMAL	: 正常
				iread = this.NcDeque();
				switch (iread) {
				case CamUtil.LCode.NcQueue.QUEINFO.EMPTY:	// 読み込みの終了（キューが空）
					return null;
				case CamUtil.LCode.NcQueue.QUEINFO.NORMAL:	// 正常
					return ncline_Now;
				case CamUtil.LCode.NcQueue.QUEINFO.NOTFULL:	// キューへ情報蓄積中
					break;
				}
			}
		}

		/// <summary>
		/// 指定されたＮＣデータを解析し、前行の情報 ncCodeLast を最新情報に更新します。
		/// その後キューの処理を実行し、iread == QUEINFO.NORMAL の場合現在行 ncline_Now を出力します
		/// </summary>
		/// <param name="ncLine">次のＮＣデータ行</param>
		/// <param name="moto">同一行の変換履歴を辿るために保存する、前の変換のNcLineCodeへのリンク</param>
		/// <returns>キューの状態（NOTFULL:キューへ蓄積中、NORMAL:入出力正常、EMPTY:キューが空）</returns>
		public QUEINFO NextLine(string ncLine, NcLineQue moto) {
			CamUtil.LCode.NcQueue.QUEINFO iread;

			if (ncLine == null)
				ncCodeLast = null;
			else {
				// 次のＮＣデータ１行の情報NcCodeを作成する
				ncCodeLast = ncCodeLast.NextLine(ncLine, moto);
			}

			// キューへの保存と取り出し
			// iread == NOTFULL	: キューへ情報蓄積中
			// iread == EMPTY	: 終了（キューが空）
			// iread == NORMAL	: 正常
			iread = this.NcDeque();
			return iread;
		}

		/// <summary>
		/// 無意味なＮＣデータを読み飛ばします
		/// </summary>
		/// <param name="readline">ＮＣデータ行</param>
		/// <param name="sr">ストリーム</param>
		/// <returns>読み飛ばす場合 true</returns>
		private bool Skip(string readline, StreamReader sr) {
			// ＮＣデータ読み込み開始時、行の先頭が'%'である行まで読み飛ばす
			if (ncCodeLast.LnumN == 0 && ncCodeLast.Start_End_Code(readline) == false)
				return true;
			// ダミーが入っていたら削除する（ダミーを入れないように修正要）
			if (ncCodeLast.LnumN == 0 && ncCodeLast.Start_End_Code(readline)) {
				if (this.ncline_Bef.Count != 0) {
					if (dummyData)
						this.ncline_Bef.Clear();
					else
						throw new Exception("qwefbqehrbfwqefrvqkerb");
				}
			}
			// ＮＣデータ読み込み終了時
			if (ncCodeLast.LnumN > 0 && ncCodeLast.Start_End_Code(readline))
				sr.ReadToEnd();
			return false;
		}
	}
}
