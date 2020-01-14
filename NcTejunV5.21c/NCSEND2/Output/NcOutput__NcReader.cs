using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CamUtil;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace NCSEND2.Output
{
	/// <summary>
	/// ＮＣデータの前処理をＣＡＭシステムごとに実施する
	/// </summary>
	abstract partial class NcOutput : IDisposable
	{
		/// <summary>
		/// ＮＣデータが不完全な部分を補完しNcLineCodeで理解できるようにフォーマットを整える
		/// </summary>
		public class NcReader : StreamReader
		{
			/// <summary>ＮＣデータ読込み状況（0:初期値, 1:有意データ）</summary>
			protected short wnc;
			protected NCINFO.NcInfoCam ncd;

			protected NcReader(NCINFO.NcInfoCam ncd) : base(ncd.FullNameCam) {
				this.wnc = 0;
				this.ncd = ncd;
			}
		}

		/// <summary>
		/// ＮＣデータが不完全な部分を補完しNcLineCodeで理解できるようにフォーマットを整える
		/// </summary>
		public class NcReader_Default : NcReader
		{
			public NcReader_Default(NCINFO.NcInfoCam ncd) : base(ncd) { }

			/// <summary>
			/// ＮＣデータの読込み。NcLineCodeで理解できるようにフォーマットを整える
			/// </summary>
			/// <returns></returns>
			public override string ReadLine() {
				string ddat;

				if (wnc > 0)
					return base.ReadLine();
				while (true) {
					ddat = base.ReadLine();
					if (ddat == null)
						throw new Exception("加工開始終了記号である'%'が存在しない。");
					if (ddat.IndexOf("%") == 0) {
						wnc++;
						return ddat;
					}
				}
			}
		}

		/// <summary>
		/// ＮＣデータが不完全な部分を補完しNcLineCodeで理解できるようにフォーマットを整える
		/// </summary>
		public class NcReader_WorkNC : NcReader
		{
			public NcReader_WorkNC(NCINFO.NcInfoCam ncd) : base(ncd) { }

			/// <summary>
			/// ＮＣデータの読込み。NcLineCodeで理解できるようにフォーマットを整える
			/// </summary>
			/// <returns></returns>
			public override string ReadLine() {
				string ddat;
				// wnc : 入力行の行番号（0から）
				switch (wnc) {
				case 0:
					ddat = base.ReadLine();
					break;
				case 1:
					ddat = "O0001";
					break;
				case 2:
					// G100の行を作成する
					ddat = "G100T99S" + ncd.NcdtInfo[0].Spidle;
					break;
				case 3:
					ddat = "G00G90" +
						"X" + ((int)ncd.xmlD.OriginX * 1000).ToString() +
						"Y" + ((int)ncd.xmlD.OriginY * 1000).ToString();
					break;
				default:
					return base.ReadLine();
				}
				wnc++;
				return ddat;
			}
		}

		/// <summary>
		/// ＮＣデータが不完全な部分を補完しNcLineCodeで理解できるようにフォーマットを整える
		/// </summary>
		public class NcReader_CADmeisterKDK : NcReader
		{
			protected Queue<string> que;

			public NcReader_CADmeisterKDK(NCINFO.NcInfoCam ncd) : base(ncd) { this.que = new Queue<string>(); }

			/// <summary>
			/// ＮＣデータの読込み。NcLineCodeで理解できるようにフォーマットを整える
			/// </summary>
			/// <returns></returns>
			public override string ReadLine() {
				string ddat;

				if (wnc == 0) wnc = 1;
				if (que.Count > 0)
					return que.Dequeue();

				ddat = base.ReadLine();
				// カスタムマクロ内のＸＹを次行に移動
				if (ddat.IndexOf("G66") >= 0) {
					if (ddat.IndexOf("X") < 0 || ddat.IndexOf("Y") < 0)
						throw new Exception("wefqgfrvqgfrqrvfg");
					int[] se = StringCAM.GetNcIndex(ddat, new char[] { 'X', 'Y' });
					que.Enqueue(ddat.Substring(se[0], se[1] - se[0]));
					ddat = ddat.Remove(se[0], se[1] - se[0]);
					throw new Exception("未検証です。凍結解除時にチェック願います。");
				}
				return ddat;
			}
		}

		/// <summary>
		/// 部品加工の前処理。ＮＣデータの工程間の切れ目で工具交換コードを挿入する
		/// </summary>
		public class NcReader_Buhin : NcReader
		{
			/// <summary>ＮＣデータ情報</summary>
			private KYDATA.KyData[] kydTool;

			/// <summary>出力するために保存されたＮＣデータ</summary>
			private Queue<string> ncQue;

			/// <summary>工具連番０から</summary>
			private int tCount;
			/// <summary>工程連番０から</summary>
			private int kCount;
			/// <summary>G100基準の行番号０から</summary>
			private int lCount;
			/// <summary>G100から３行のＮＣデータ</summary>
			private string[] g100 = new string[3];

			public NcReader_Buhin(NCINFO.NcInfoCam ncd) : base(ncd) {
				this.ncQue = new Queue<string>();
				this.tCount = this.kCount = this.lCount = -1;
			}

			/// <summary>
			/// ＮＣデータの読込み。NcLineCodeで理解できるようにフォーマットを整える
			/// </summary>
			/// <returns></returns>
			public override string ReadLine() {
				string ddat;

				// '%' まで読み飛ばす
				if (wnc == 0)
					while (true) {
						ddat = base.ReadLine();
						if (ddat == null)
							throw new Exception("加工開始終了記号である'%'が存在しない。");
						if (ddat.IndexOf("%") == 0) {
							wnc++;
							return ddat;
						}
					}

				// キューにデータがある場合（工具分割時のみ実行される）
				if (ncQue.Count > 0)
					return ncQue.Dequeue();

				// １行の読み込みとG100の処理
				ddat = base.ReadLine();
				if (ddat.IndexOf("G100T") == 0) {
					tCount++;
					kCount = -1;
					lCount = -1;
					if (CamUtil.ProgVersion.NotTrialVersion2) {
						kydTool = ((KYDATA.KyData_Tebis)ncd.ncKydList[tCount]).Divide_Tool(false);
						if (kydTool.Length != 1) throw new Exception("frbehrfb");
					}
					else
						kydTool = ((KYDATA.KyData_Tebis)ncd.ncKydList[tCount]).Divide_Tool(true);
				}
				// 工具分割とは無関係のデータの場合
				if (kydTool == null) return ddat;
				if (kydTool.Length == 1) return ddat;

				// 工具分割のためのG100行などの保存と工程数のチェック
				lCount++;
				if (lCount < 3) { g100[lCount] = ddat; }
				if (ddat.IndexOf("M98P9306") == 0) {
					kCount++;
					if (kCount + 1 != kydTool.Sum(kyd => kyd.CountKotei)) throw new Exception("工程数が異常");
				}

				// 工具分割候補位置ではない場合
				if (ddat.IndexOf("M98P9017") != 0) return ddat;

				// ///////////////////////////////////////
				// 以下は工具分割候補位置(M98P9017)の場合
				// ///////////////////////////////////////
				kCount++;

				// 工具交換するしないの判断と出力コードのセット
				bool tchg = false;
				for (int ii = 1; ii <= kydTool.Length; ii++) {
					int kotei = kydTool.Take(ii).Sum(kyd => kyd.CountKotei);
					if (kotei == kCount + 1) tchg = true;
					if (kotei >= kCount + 1) break;
					if (ii == kydTool.Length) throw new Exception("qerfvqerfv");
				}
				if (tchg) {
					// 工具交換コードの挿入
					ncQue.Enqueue("M98P9306");
					ncQue.Enqueue(g100[0]); // G100
					ncQue.Enqueue(g100[1]); // G65P9376
				}
				else {
					// そのままであるが、以下の工具軸方向の更新は必須！
					ncQue.Enqueue(ddat);
				}

				// 工具軸方向を更新する
				ddat = base.ReadLine();
				if (ddat.IndexOf("G65P8700") == 0) {
					g100[2] = ddat;
					ddat = base.ReadLine();
					ncQue.Enqueue(g100[2]); // G65P8700
				}
				else {
					if (tchg) ncQue.Enqueue(g100[2]); // G65P8700 工具交換コード挿入の場合は以前の工具軸情報を出力する
				}
				if (ddat.IndexOf("G102") != 0) throw new Exception("argfaergfb");
				ncQue.Enqueue(ddat); // G102

				return ncQue.Dequeue();
			}
		}

		public void Dispose() {
			if (sr != null) { sr.Dispose(); sr = null; }
		}
	}
}
