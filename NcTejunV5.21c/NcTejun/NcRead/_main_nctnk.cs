using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using NcCode.nccode;

namespace NcTejun.NcRead

{
	/// <summary>
	/// マクロ展開するメインクラス
	/// </summary>
	internal class _main_nctnk : NcCode._main, IDisposable
	{
		/// <summary>出力するStringBuilderクラス</summary>
		protected List<string> ncWrite = null;

		/// <summary>ＮＣデータの検索フォルダーの順</summary>
		public override string[] Ncdir {
			get { return m_ncdir; }
			protected set { m_ncdir = value; }
		}
		private string[] m_ncdir ={
			CamUtil.ServerPC.SvrFldrM,
			CamUtil.ServerPC.SvrFldrE};

		/// <summary>
		/// マクロコールを展開します。展開時は"GENERAL"のマクロコードを用います。
		/// </summary>
		/// <param name="program_name">ＮＣデータ名</param>
		/// <param name="mach">加工機情報</param>
		/// <param name="ncwrite">出力先のインスタンス</param>
		/// <param name="xyz">初期のＸＹＺ位置</param>
		/// <param name="mac_name">展開するマクロ名</param>
		public _main_nctnk(string program_name, NcdTool.Mcn1 mach, List<string> ncwrite, CamUtil.Vector3 xyz, string mac_name)
			: base(program_name, NcCode._main.mcGENERAL, mach.ID) {

			// メインＮＣデータ設定
			MainNcdSet("CALL", null, xyz, mac_name);

			this.ncWrite = ncwrite;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nout"></param>
		/// <param name="mod"></param>
		/// <param name="lcode"></param>
		public override void Ncoutm(NcOuts nout, NcMod mod, OCode lcode) {
			int jj, sg, sm;
			string chout = "";

			sg = 0;
			sm = 0;

			// ////////////
			// 初期化中
			if (ncWrite == null) return;
			// ////////////

			if (lcode.Gg[0].Equals(65))
				return;
			if (lcode.subdep1.subm == 0 && mod.GGroupValue[12].Equals(66.1))
				return;
			foreach (CodeD ctmp in lcode.codeData) {
				//kk = ctmp.stj;
				switch (ctmp.ncChar) {
				case ';':
					if (chout.Length != 0)
						chout += ctmp.ncChar + "\n";
					break;
				case 'G':
					sg++;
					if (sg == 1)
						for (jj = 0; jj < lcode.Gg.Count; jj++)
							if (lcode.Gg[jj].Gst == true) {
								if (mod.GGroupValue[jj].Equals(66) || mod.GGroupValue[jj].Equals(67))
									return;
								chout += mod.GGroupValue[jj].ToStringAuto();
							}
					break;
				case 'M':
					sm++;
					if (lcode.Mst[sm] == 99 || lcode.Mst[sm] == 2)
						return;
					chout += ctmp.ncChar;
					chout += lcode.Mst[sm].ToString("0");
					break;
				case 'N':
					break;
				case 'O':
					return;
				case '/':
				case '(':
				case '%':
				case '#':
					break;
				case 'F':
				case 'P':
					chout += ctmp.ncChar;
					chout += ctmp.ToInt.ToString("0000");
					break;
				case 'D':
					chout += ctmp.ncChar;
					chout += ctmp.ToStringAuto();
					break;
				default:
					chout += ctmp.ncChar;
					chout += ctmp.ToString("0.0");
					break;
				}
			}
			if (chout.Length != 0)
				ncWrite.Add(chout);
			return;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Ncoute() {
			//NcOuts nout = ncReader.tout;
			base.WriteEnd();
		}

		/// <summary>
		/// ＮＣデータ解析時におけるエラーの処理を実行
		/// </summary>
		/// <param name="errn">エラー番号</param>
		/// <param name="errc">コメント</param>
		public override void Ncerr(int errn, string errc) {
			switch (errn) {
			case 0: /* check data print */
				return;
			case 1: /* warning data print */
				return;
			case 2: /* error data print */
				return;
			default: /* fatal error */
				MessageBox.Show(errc + " code=" + errn, "ncerr",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
				throw new Exception();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Close()
		{
			throw new Exception("The method or operation is not implemented.");
		}
		public void Dispose() {
			if (m_swNcCheck != null) { m_swNcCheck.Dispose(); m_swNcCheck = null; }
		}
	}
}
