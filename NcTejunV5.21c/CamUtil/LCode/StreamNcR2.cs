using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace CamUtil.LCode
{
	/// <summary>
	/// 変換仕様NcConvertのリストを使用してＮＣデータを連続的に変換します。
	/// </summary>
	public class StreamNcR2 : StreamReader
	{
		/// <summary>入力ＮＣデータをチェックする</summary>
		public delegate void NcFormCheck(ref int iget, string ddat);

		/// <summary>ＮＣデータの標準行末コード</summary>
		static readonly char[] stdEndCode = new char[] { ';' };




		/// <summary>変換仕様のリスト</summary>
		private INcConvert[] ncConvs;

		/// <summary>各変換後の出力ＮＣデータのリスト</summary>
		private readonly OutLine[] outLines;

		/// <summary>ＮＣ行の変換履歴を保存します</summary>
		public NcLineCode[] lastConv;

		/// <summary>ＮＣデータの行数（進行状況の表示に使用 ADD in 2016/07/29）</summary>
		public int MaxLineNo { get { return m_maxLineNo; } }
		private readonly int m_maxLineNo;
		/// <summary>ＮＣデータの表示間隔（進行状況の表示に使用 ADD in 2016/07/29）</summary>
		public int Interval { get { return m_interval; } }
		private readonly int m_interval;

		/// <summary>ＮＣデータのチェックの実行</summary>
		private readonly NcFormCheck nctchk;
		private int iget;

		/// <summary>
		/// 唯一のコンストラクタです。
		/// </summary>
		/// <param name="path">ＮＣデータのファイル名</param>
		/// <param name="p_nctchk">入力されたＮＣデータのチェック</param>
		/// <param name="aa">変換仕様のリスト</param>
		public StreamNcR2(string path, NcFormCheck p_nctchk, params INcConvert[] aa)
			: base(path, Encoding.Default) {
			this.ncConvs = aa;
			//this.endCode = new char[] { endcode };
			this.lastConv = new NcLineCode[this.ncConvs.Length];
			this.outLines = new OutLine[this.ncConvs.Length];

			m_maxLineNo = File.ReadAllLines(path).Length;
			m_interval = Math.Min(5000, 1 + m_maxLineNo / 45);
			nctchk = p_nctchk;
			iget = 0;
		}

		/// <summary>
		/// ＮＣデータの変換リストを連続処理します
		/// </summary>
		/// <returns>すべての変換後の最終ＮＣデータ行</returns>
		public override string ReadLine() {
			int imax = ncConvs.Length - 1;
			string outl = ReadLine(imax);

			// 変換チェック用データの作成
			NcLineQue moto2 = ncConvs[imax].NcQue[0];
			for (int jj = imax; jj >= 0; jj--) {
				this.lastConv[jj] = moto2;
				if (moto2 != null) moto2 = moto2.source;
			}

			// 最終出力
			return outl;
		}
	
		/// <summary>
		/// ＮＣデータの変換とキューの処理を実行します。StreamNcR.ReadLineと自身から呼び出されます。
		/// </summary>
		/// <remarks>
		/// １つの変換で複数の行が出力された場合は outLines[index].outputNo を用いて１行ずつ処理します。
		/// 最終変換から実行され、再帰的に前の変換仕様を呼出して出力されるＮＣデータ１行を取得します。
		/// </remarks>
		/// <param name="index">処理する変換仕様のインデックス</param>
		/// <returns>変換されたＮＣデータ行</returns>
		private string ReadLine(int index) {
			NcQueue.QUEINFO iread = NcQueue.QUEINFO.NORMAL;
			string readline;

			// ＮＣデータが出力されるまで読み込みを繰り返す
			while (true) {
				if (iread == NcQueue.QUEINFO.NORMAL)
					if (outLines[index] != null && outLines[index].OutputNo < outLines[index].Count)
						return outLines[index][outLines[index].OutputNo];	// 変換したデータの一行取出し。無くなるまでここから取り出す

				// それぞれ、ファイルストリームあるいは前の出力からデータを取出し、
				// 変換後自分のキューへ保存する
				if (index == 0) {
					readline = base.ReadLine();						// ファイルから読込み
					if (readline != null) {
						if (nctchk != null)
							nctchk(ref iget, readline);
						readline = readline.TrimEnd(stdEndCode);
					}
				}
				else
					readline = ReadLine(index - 1);					// 再帰呼び出し

				// --- 新たなＮＣデータの解析と保存 ---
				iread = ncConvs[index].NcQue.NextLine(readline, index > 0 ? ncConvs[index - 1].NcQue[0] : null);
				switch (iread) {
				case NcQueue.QUEINFO.NOTFULL:				// キューへ情報蓄積中。繰り返す
					break;
				case NcQueue.QUEINFO.EMPTY:				// 出力データなし。
					//outLines[index] = new NcLineCode.OutLine();
					outLines[index] = null;
					return null;
				case NcQueue.QUEINFO.NORMAL:
					outLines[index] = ncConvs[index].ConvExec().Clone();	// 一行の変換の実行
					break;
				}
			}
		}
	}
}
