using System;
using System.Collections.Generic;
using System.Text;

using CamUtil;

namespace NcdTool
{
	/// <summary>
	/// 加工手順データ情報を保存する[不変]
	/// </summary>
	class Tejun
	{
		/// <summary>ツールセットのリスト</summary>
		static internal Tejun NcList { get; private set; }

		/// <summary>ＣＡＭから出力されたＮＣデータのフォーマット名。手順内で同一とする in 2015/02/09</summary>
		static internal BaseNcForm BaseNcForm { get { return m_baseNcForm; } }
		static private BaseNcForm m_baseNcForm;

		/// <summary>加工手順名</summary>
		static public string TejunName { get; private set; }
		/// <summary>ユーザID</summary>
		static public int Uid { get; private set; }
		/// <summary>製造番号</summary>
		static public string Seba { get; private set; }
		/// <summary>加工機情報</summary>
		static internal Mcn1 Mach { get; private set; }
		/// <summary>加工予定日</summary>
		static public DateTime Kdate { get; private set; }
		/// <summary>手順書データが検証済みであれば（NCSPEED_editフォルダー内） true</summary>
		static public bool Ncspeed { get; private set; }
		/// <summary>工具表データの存在するディレクトリ</summary>
		static public string TolDir { get { return System.IO.Path.GetDirectoryName(m_tjnDir); } }
		/// <summary>手順書データの存在するディレクトリ</summary>
		static public string TjnDir { get { return m_tjnDir; } }
		static private string m_tjnDir;

		/// <summary>手順のクリア</summary>
		static public void Set() {
			NcList = new Tejun();
			m_baseNcForm = BaseNcForm.EMPTY;

			TejunName = null;
			Uid = 0;
			Seba = null;
			//machname = null;
			Mach = null;
			Kdate = DateTime.MinValue;
			Ncspeed = false;
			m_tjnDir = null;
		}

		/// <summary>設定</summary>
		/// <param name="p_TejunText"></param>
		/// <param name="p_tejunName"></param>
		/// <param name="p_uid"></param>
		/// <param name="p_seba"></param>
		/// <param name="p_machname"></param>
		/// <param name="p_kdate"></param>
		/// <param name="p_ncspeed"></param>
		/// <param name="tjnName"></param>
		static public void Set(List<string> p_TejunText, string p_tejunName, string p_uid, string p_seba, string p_machname, string p_kdate, bool p_ncspeed, string tjnName) {
			TejunName = p_tejunName;
			Uid = Convert.ToInt32(p_uid);
			Seba = p_seba;
			//machname = p_machname;
			Mach = new Mcn1(p_machname);
			m_baseNcForm = Machine.BNcForm(Mach.ID);

			Kdate = Convert.ToDateTime(p_kdate);
			Ncspeed = p_ncspeed;
			m_tjnDir = tjnName;

			NcList = null;	// コンストラクタ内で使用できないようにする
			NcList = new Tejun(p_TejunText.ToArray());
			if (NcList.NcNamsAll.Count == 0) throw new Exception("ＮＣデータの数が０です。");
		}

		// /////////////
		// 以上 static
		// /////////////





		/// <summary>総合材質名（連名）</summary>
		public string Mzais {
			get {
				string sout = "";
				List<string> zais = new List<string>();
				foreach (NcName.NcNam ncnam in NcNamsAll_NcExist) {
					if (zais.Contains(ncnam.nmgt.SetZais) == false) {
						zais.Add(ncnam.nmgt.SetZais);
						sout += ncnam.nmgt.SetZais;
					}
				}
				return sout;
			}
		}
		/// <summary>ＣＡＭで作成されたＮＣデータの情報</summary>
		internal List<NcName.NcData> NcData {
			get {
				List<NcName.NcData> ncdt = new List<NcName.NcData>();
				foreach (NcName.NcNam nnam in m_ncNams) {
						if (nnam.Ncdata == null) continue;
						if (!ncdt.Contains(nnam.Ncdata)) ncdt.Add(nnam.Ncdata);
					}
				return ncdt;
			}
		}

		/// <summary>手順で設定されたＮＣの情報_ＮＣ出力するデータ限定</summary>
		internal List<NcName.NcNam> NcNamsAll_NcOutput { get { return m_ncNams.FindAll(NcName.Zopt.NcOutput.Nctoks); } }
		/// <summary>手順で設定されたＮＣの情報_ＮＣデータが存在するデータ限定（itdat.HasValue の場合、 ncsd.itdat == 0 と同等）</summary>
		internal List<NcName.NcNam> NcNamsAll_NcExist { get { return m_ncNams.FindAll(NcName.Zopt.NcExist.Nctoks); } }
		/// <summary>手順で設定されたＮＣの情報_ＮＣデータがダミーではないデータ限定</summary>
		internal List<NcName.NcNam> NcNamsAll_NotDummy { get { return m_ncNams.FindAll(NcName.Zopt.NotDummy.Nctoks); } }
		/// <summary>工具表とのマッチングが実施されたデータ限定（工具表の工具と関連付けの有無ではない）</summary>
		internal List<NcName.NcNam> NcNamsAll_NcMatch { get { return m_ncNams.FindAll(NcName.Zopt.NcMatch.Nctoks); } }

		/// <summary>手順で設定されたＮＣの情報_ツールシート名限定</summary>
		public List<NcName.NcNam> NcNamsTS(string tolstname) {
			return m_ncNams.FindAll(ncnam => ncnam.tsheet == tolstname);
		}
		/// <summary>手順で設定されたＮＣの情報_ツールシート名限定_ＮＣデータが存在するデータ限定（itdat.HasValue の場合、 ncsd.itdat == 0 と同等）</summary>
		public List<NcName.NcNam> NcNamsTS_NcExist(string tolstname) {
			return NcNamsAll_NcExist.FindAll(ncnam => ncnam.tsheet == tolstname);
		}


		/// <summary>手順で設定されたＮＣの情報_全て</summary>
		internal List<NcName.NcNam> NcNamsAll { get { return m_ncNams; } }
		private readonly List<NcName.NcNam> m_ncNams;

		/// <summary>手順で設定された工具表の名称リスト。同一名称が複数含まれることもある</summary>
		public string[] TsNameList { get { return m_TsNameList.ToArray(); } }
		private string TsNameLast { get { return m_TsNameList.Count > 0 ? m_TsNameList[m_TsNameList.Count - 1] : null; } }
		private readonly List<string> m_TsNameList;

		/// <summary>
		/// 空のデータを作成するコンストラクタ
		/// </summary>
		private Tejun() {
			this.m_ncNams = new List<NcName.NcNam>();
			this.m_TsNameList = new List<string>();
		}

		/// <summary>
		/// 手順データの各行から作成するコンストラクタ
		/// </summary>
		/// <param name="tejunText">手順データ</param>
		private Tejun(string[] tejunText) {
			bool uSet, bSet, mSet;
			uSet = bSet = mSet = false;
			string stmp;
			NcdTool.NcName.Nmgt gmodN = new NcdTool.NcName.Nmgt(0);

			this.m_ncNams = new List<NcName.NcNam>();
			this.m_TsNameList = new List<string>();

			bool loop = true;
			foreach (string ddat in tejunText) {
				if (loop == false)
					break;
				//MessageBox.Show(ddat);

				string[] ff = ddat.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (ff.Length == 0)
					continue;
				if (ff.Length == 1)
					stmp = null;
				else
					stmp = ff[1];

				switch (ddat[0]) {
				case 'K':	// 加工予定日 Add in 2012/11/15
				case 'I':
				case 'B':
				case 'D':
				case 'Z':
				case 'O':
				//case 'G':
				//case 'L':
				case 'S':
				case 'F':
				case 'W':
				//case 'Y':	// 内容不明。今は未使用
				case 'V':	// 表の加工：０、裏の加工：１ add 2008/02/19
				case 'M':
				case 'T':
				//case 'P':
				//case 'p':
				case 'N':
				case 'n':
				case 'X':
					if (stmp == null)
						throw new Exception("tejun data line '" + ddat + "' ERROR");
					break;
				case 'C':	// 手順書の分割位置
				case 'E':
					break;
				default:
					LogOut.warn.AppendLine("手順の行 '" + ddat + "' は認識できません。");
					break;
				}

				switch (ddat[0]) {
				case 'M':
					if (mSet)
						throw new Exception("加工機が再設定されました");
					mSet = true;
					break;
				case 'T':
					if (mSet == false)
						throw new Exception("ツールシート名を設定する前に加工機を決定してください");
					if (m_TsNameList.Count == 1)
						if (TsNameLast == "" && this.m_ncNams.Count != 0)
							throw new Exception("ツールシート名をＮＣデータの途中で設定することはできません");
					if (m_TsNameList.Count == 0 || TsNameLast != stmp)
						m_TsNameList.Add(stmp);
					break;
				}

				switch (ddat[0]) {
				case 'I':
					if (uSet) {
						throw new Exception("user name double set error");
					}
					uSet = true;
					break;
				case 'B':
					if (bSet) {
						throw new Exception("seibann double set error");
					}
					bSet = true;
					break;
				case 'D':
				case 'Z':
				case 'O':
				//case 'G':
				//case 'L':
				case 'S':
				case 'F':
				case 'W':
				//case 'Y':
				case 'V':	// 加工方向　0=表面, 1=裏面, 2=正面, 3=背面, 4=右側面, 5=左側面
				case 'C':	// 手順書の分割位置
				case 'M':
				case 'T':
					// gmodの情報保存とＮＣデータの設定
					gmodN.Set_Tejun(ddat);
					break;
				case 'N':
				case 'n':
					if (uSet == false)
						throw new Exception("ＮＣデータを設定する前にユーザ名を決定してください");
					if (bSet == false)
						throw new Exception("ＮＣデータを設定する前に製造番号を決定してください");
					if (mSet == false)
						throw new Exception("ＮＣデータを設定する前に加工機を決定してください");
					// ダミーの工具表を作成
					if (m_TsNameList.Count == 0) {
						m_TsNameList.Add("");
						gmodN.Set_Tejun("T");
					}
					// 一般ＮＣデータの設定
					this.m_ncNams.Add(new NcdTool.NcName.NcNam(gmodN, ff, TsNameLast, this.NcNamsAll));
					gmodN.OtClear();

					break;
				case 'E':
					loop = false;
					break;
				}
			}
			return;
		}
	}
}
