using System;
using System.Collections.Generic;
using System.Text;

namespace NcCode.nccode
{
	/// <summary>
	/// ＮＣデータ１行により移動する複数の位置情報を保存します。
	/// </summary>
	internal class NcOut
	{
		/// <summary>関連するlcode,modの保存位置</summary>
		public int imac;

		/// <summary>最終位置と中間点の最大数</summary>
		public const int CHUU = 4;
		/// <summary>ＮＣデータ１行により発生する加工機の最終位置情報</summary>
		public NcOuts Out0 { get { return m_out0; } private set { m_out0 = value; } }
		private NcOuts m_out0;
		/// <summary>ＮＣデータ１行により発生する加工機の中間位置情報</summary>
		private List<NcOuts> outn;

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="ichi">初期の位置</param>
		public NcOut(CamUtil.Ichi ichi) {
			imac = 0;
			m_out0 = new NcOuts(ichi);
			outn = new List<NcOuts>();
		}

		/// <summary>
		/// 前行の情報を用いて新規ＮＣデータ行のモード情報を作成する
		/// </summary>
		/// <param name="src"></param>
		/// <param name="mcro"></param>
		public NcOut(NcOut src, bool mcro) {
			imac = -1;
			//m_out0 = new NcOuts(src.m_out0);
			//m_out0 = new NcOuts(out0, 0);
			m_out0 = new NcOuts(src.m_out0);
			outn = new List<NcOuts>();
			if (src.outn.Count != 0) {
				for (int ii = 0; ii < src.outn.Count; ii++)
					outn.Add(src.outn[ii].Clone());
			}
			if (mcro)
				outn.Clear();
		}

		//public ncout this[int index] { get { return Nout[index]; } set { Nout[index] = value; } }
		/// <summary>
		/// 加工機の位置情報（0:最終位置、else:中間点）
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public NcOuts this[int index] {
			get {
				if (index == 0) return Out0;
				if (index - 1 < outn.Count) return outn[index - 1];
				return null;
			}
		}

		/// <summary>
		/// 加工機の位置を算出する
		/// </summary>
		/// <param name="idopass">移動状態 0:移動 1:ＸＹのみ 2:移動なし 3:無意データ</param>
		/// <param name="lcode"></param>
		/// <param name="mod0"></param>
		/// <param name="p_fsub">マクロ変数</param>
		public void Ncxyz(int idopass, OCode lcode, NcMod mod0, NcMachine.Variable p_fsub)
		{
			if (idopass > 2) {
				outn.Clear();
				return;
			}

			// ////////////////////////////////////////
			// 「不変化」するためのチェックに使用する
			// ////////////////////////////////////////
			m_out0 = new NcOuts(m_out0, idopass, lcode, mod0, p_fsub);
			outn.Clear();

			if (idopass >= 2) return;
			if (mod0.GGroupValue[0].Equals(39)) return;
			Chukan();
		}

		/// <summary>
		/// 早送り中間点の作成
		/// </summary>
		private void Chukan()
		{
			if (m_out0.Hokan != 0 || m_out0.idoa == 0 || (NcMachine.ParaData(1400, 0) & 0x10) != 0)
				return;

			// 早送りの非直線処理 new
			List<CamUtil.Pals> newCHU = m_out0.Pls.HiChokusen(NcMachine.ParaData1420());
			foreach (CamUtil.Pals ich in newCHU) {
				NcOuts ntmp = new NcOuts(m_out0, ich.ToIchi(Post.PostData['X'].sdgt));
				outn.Insert(0, ntmp);
			}
			return;
		}
	}
}
