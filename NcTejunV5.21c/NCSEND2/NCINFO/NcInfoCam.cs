using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using CamUtil;
using CamUtil.CamNcD;

namespace NCSEND2.NCINFO
{
	/// <summary>
	/// ＮＣＳＥＮＤ２で使用する詳細なＮＣデータ１ファイル＆複数工具の情報（NC, XML, ToolSsetCam）
	/// </summary>
	partial class NcInfoCam : NcInfo
	{
		/// <summary>部品加工の場合に使用するファイル名の先頭５文字</summary>
		static public ServerPC.PTPName BuhinName;
		/// <summary>データ作成順</summary>
		static public int CompareName(NCINFO.NcInfoCam ncd1, NCINFO.NcInfoCam ncd2) { return (String.Compare(ncd1.FullNameCam, ncd2.FullNameCam)); }
		/// <summary>データ作成順</summary>
		static public int CompareTime(NCINFO.NcInfoCam ncd1, NCINFO.NcInfoCam ncd2) { return (DateTime.Compare(ncd1.LastWriteTime, ncd2.LastWriteTime)); }

		/// <summary>データ出力時に作成されるテキストファイルの順 ADD in 2015/07/16</summary>
		static public int CompareText(NCINFO.NcInfoCam ncd1, NCINFO.NcInfoCam ncd2) { return ncd1.tNo - ncd2.tNo; }
		/// <summary>
		/// 部品加工のＴＸＴファイルの内容からＮＣデータ名のリストを取得する
		/// </summary>
		/// <param name="list"></param>
		/// <param name="txtNam">テキストファイル名</param>
		/// <returns>ＮＣデータリスト</returns>
		static public void TextSortSet(List<NcInfoCam> list, string txtNam) {
			int no = 0;
			string ddat, fnam;
			using (StreamReader ftxt = new StreamReader(txtNam)) {
				while (!ftxt.EndOfStream) {
					ddat = ftxt.ReadLine();
					if (ddat.IndexOf("PUTNC") < 0 || ddat.IndexOf("[") < 0) continue;
					no += 10;
					fnam = ddat.Substring(13, ddat.IndexOf('[') - 13).TrimEnd(new char[] { ' ' });
					for (int ii = 0; ii < list.Count; ii++)
						if (fnam == Path.GetFileNameWithoutExtension(list[ii].FullNameCam)) {
							list[ii].tNo = no;
							break;
						}
				}
			}
			return;
		}
		private int tNo;





		protected List<MessageData> messdata;
		/// <summary>
		/// NcInfoCAMのワーニングメッセージを保管する
		/// </summary>
		public readonly struct MessageData
		{
			public readonly string messType;
			public readonly string fileName;
			public readonly string mess;
			public MessageData(string mType, string fName, string mStrg) {
				this.messType = mType;
				this.fileName = fName;
				this.mess = mStrg;
			}
		}

		/// <summary>ファイルフルパス名</summary>
		public string FullNameCam { get { return m_FullNameCam; } }
		private readonly string m_FullNameCam;         // ファイルフルパス名

		/// <summary>最終更新日（==xmlD.LMIDI,==xmlD.camOutputDateTime）</summary>
		public DateTime LastWriteTime { get { return m_LastWriteTime; } }
		private readonly DateTime m_LastWriteTime;  // 最終更新日


		/// <summary>ＮＣデータのファイルサイズ</summary>
		public long FSize { get { return m_Size; } }
		private readonly long m_Size;

		/// <summary>工具交換回数（注意：ＸＭＬの工具数でＵＮＩＸにて事前分割されたものは含まない）（==xml.ToolCount）</summary>
		public int ToolCount { get { return xmlD != null ? xmlD.ToolCount : 0; } }

		// 2008/11/20 追加
		/// <summary>以前出力したデータがある場合、そのデータのフルネーム(NC or XML)</summary>
		public string fulfName = null;

		// その他（DtGetで使用）

		/// <summary>出力名（作成されるＮＣデータの名前）</summary>
		public string OutName { get { return m_outName; } set { m_outName = value; } }
		private string m_outName;
		/// <summary>状態（工具分割無し）</summary>
		public LVM SMOD0 { get { return m_LvmMess0.m_Mode; } }
		/// <summary>エラーメッセージ（工具分割無し）</summary>
		public string ERRM0 { get { return m_LvmMess0.m_ErrorMessage; } }

		protected LvmMess m_LvmMess0;
		/// <summary>データの状態</summary>
		public readonly struct LvmMess
		{
			public readonly LVM m_Mode;          // データの状態
			public readonly string m_ErrorMessage;     // エラーメッセージ
			public LvmMess(LVM lvm, string mess) {
				m_Mode = lvm;
				m_ErrorMessage = mess;
			}
		}

		/// <summary>
		/// 加工情報のＮＣデータ内の条件（ＤＢ条件とは一致しない。ＤＢに合わせるため変換する）
		/// 出力するＸＭＬとは異なる cf. xmlDoc(xmlD)
		/// </summary>
		public List<SFCInfo> NcdtInfo;

		/// <summary>
		/// ＮＣデータ内の加工条件
		/// </summary>
		public readonly struct SFCInfo
		{
			public readonly double Spidle;
			public readonly double Feedrt;
			public readonly string Coolnt;
			/// <summary>
			/// ＮＣデータ内の加工条件
			/// </summary>
			/// <param name="spin">回転数</param>
			/// <param name="feed">送り速度</param>
			/// <param name="cool">クーラント</param>
			public SFCInfo(double spin, double feed, string cool) {
				Spidle = spin;
				Feedrt = feed;
				Coolnt = cool;
			}
		}

		/// <summary>
		/// ＳＱＬＤＢ標準条件 AddXmlNode内で追加する
		/// </summary>
		public List<TSetCAM> sqlDB;

		// その他
		protected List<string> tuError = new List<string>();           // 突出し量未設定エラー

		// 状態ナンバー（=lim.tag）
		public enum LVM : int
		{
			未設定 = 0,
			未送信 = 1,
			送信済 = 2,
			再送要 = 3,
			未確認 = 4,
			出力名 = 5,
			エラー = 9
		}

		/// <summary>文字数エラーの設定</summary>
		protected StringLengthDB strLengthErr;

		/// <summary>工程間での工具分割前のリスト。ＮＣデータと一致する in 2019/08/05</summary>
		public KYDATA.KyData[] ncKydList;

		/// <summary>
		/// コンストラクタ（唯一）
		/// </summary>
		/// <param name="camSystem"></param>
		/// <param name="stdt"></param>
		/// <param name="pMessData"></param>
		/// <param name="p_strLengthErr"></param>
		public NcInfoCam(CamSystem camSystem, KYDATA.KyData stdt, List<MessageData> pMessData, StringLengthDB p_strLengthErr) {

			// 最初の子ノードの作成
			xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "Shift_JIS", null));
			xmlDoc.AppendChild(xmlDoc.CreateElement("NcInfo"));

			// ワーニングの表示用リスト
			this.messdata = pMessData;

			// 文字数エラーの設定
			this.strLengthErr = p_strLengthErr;

			NcdtInfo = new List<SFCInfo>();
			sqlDB = new List<TSetCAM>();

			// subset
			//subSet(fnam);
			this.m_FullNameCam = stdt.FulName;
			if (m_FullNameCam == null)
				throw new Exception("は存在しません");
			FileInfo fi1 = new FileInfo(m_FullNameCam);
			if (!fi1.Exists)
				throw new Exception("ＮＣデータ" + m_FullNameCam + "は存在しません");
			this.m_LastWriteTime = fi1.LastWriteTime;
			this.m_Size = fi1.Length;
			this.m_LvmMess0 = new LvmMess(LVM.未設定, "");

			if (stdt.Buhin)
				this.m_outName = BuhinName.AddName + Path.GetFileNameWithoutExtension(m_FullNameCam);
			else
				this.m_outName = BuhinName.AddName + Path.GetFileNameWithoutExtension(m_FullNameCam).ToUpper();


			try {
				TSetCAM tset;
				this.ToolCountTemp = stdt.CountTool;
				this.Dim = stdt.Dimension;

				// ＸＭＬ情報を作成
				AddXmlNode_NcInfo(stdt);

				// 工具単位情報
				foreach (KYDATA.KyData tList in stdt.Divide_Tool()) {
					// ＳＱＬの工具情報を取得
					tset = new TSetCAM(tList.TSName(0));
					sqlDB.Add(tset);
					// ＮＣデータの切削条件をセット
					NcdtInfo.Add(new SFCInfo(
						tList.GetDoubleItem(KYDATA.ItemKotei.SpindleSpeed_CSV, tset, 0).Value,
						tList.GetDoubleItem(KYDATA.ItemKotei.CuttingFeedRate_CSV, tset, 0).Value, "DUMMY"));

					// 工程のチェック
					switch (camSystem.Name) {
					case CamSystem.Tebis:
						if (((KYDATA.KyData_Tebis)tList).CheckKotei(messdata) != DialogResult.OK)
							throw new Exception("接続されたパスで残し量が異なる");
						break;
					case CamSystem.Dynavista2D:
						((KYDATA.KyData_Dynavista2D)tList).CheckKotei(this.tuError, messdata, tset);
						break;
					}

					// ＸＭＬ情報を作成
					AddXmlNode_ToolSet(tList, tset, stdt.Buhin);
				}
				// 工程間での工具分割に使用するncKydListを作成 in 2019/08/05
				if (stdt.Buhin) {
					ncKydList = ((KYDATA.KyData_Tebis)stdt).Divide_Tool(false);
					if (ncKydList.Length != NcdtInfo.Count)
						System.Windows.Forms.MessageBox.Show($"ＮＣ={Path.GetFileName(m_FullNameCam)} は倒れ補正情報により工具分割されました。");
				}
				else
					ncKydList = null;
			}
			catch (IOException) {
				MessageBox.Show(
					$"ファイル '{Path.ChangeExtension(FullNameCam, ".csv")}' は別のプロセスで使用中でアクセスできません。\nもう一度実行してください。",
					"NcInfoCam",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
				throw;
			}
			catch (Exception ex) {
				//MessageBox.Show(ex.Message);
				m_LvmMess0 = new LvmMess(LVM.エラー, ex.Message);
				return;
			}

			// //////////////////////
			// ＬＶＩＥＷ情報のセット
			// //////////////////////
			SetElse();

		}

		/// <summary>
		/// １工具のＸＭＬノードを追加
		/// </summary>
		/// <param name="kyd2"></param>
		/// <param name="tset"></param>
		/// <param name="buhin"></param>
		private void AddXmlNode_ToolSet(KYDATA.KyData kyd2, TSetCAM tset, bool buhin) {
			XmlNode tmpNode1, tmpNode2, tmpNode3;

			/**********************************************************************
			 * AddXmlElementsimpleTypeに "NONE"を代入すると、本当に何も代入されない
			 * ようである。注意！！
			***********************************************************************/

			tmpNode1 = xmlDoc.SelectSingleNode("NcInfo");
			int dimension = Convert.ToInt32(xmlDoc.SelectSingleNode("NcInfo/NcData/camDimension").InnerText);
			ToolD toold = new ToolD(kyd2, tset);
			double maxCLR = -9999.99;
			tmpNode2 = AddXmlElementcomplexType(tmpNode1, "Tool");

			// /////
			// 工具
			// /////
			tmpNode3 = AddXmlElementcomplexType(tmpNode2, "ToolData");
			if (m_LvmMess0.m_Mode != LVM.エラー) m_LvmMess0 = toold.ErrorCheck(m_LvmMess0, tset);
			toold.Output(this, tmpNode3);

			// ////////
			// 工具軸
			// ////////
			Axis ax;
			foreach (KYDATA.KyData ff in kyd2.Divide_Axis()) {
				ax = new Axis(ff.Divide(""));
				tmpNode3 = AddXmlElementcomplexType(tmpNode2, "Axis");
				ax.Output(this, tmpNode3);
				maxCLR = Math.Max(maxCLR, ax.ClearancePlane);
			}
			if (maxCLR < -9999) throw new Exception("afrbahfbafrabrb");

			// /////
			// 工程
			// /////
			Kotei kotei;
			// 新工程情報出力
			tmpNode3 = AddXmlElementcomplexType(tmpNode2, "kotei");
			kotei = new Kotei(kyd2.Divide(""), toold, tset);
			kotei.Output(this, tmpNode3, maxCLR, strLengthErr, buhin);

			return;
		}

		/// <summary>
		/// 共通のその他の情報のセット
		/// </summary>
		protected void SetElse() {

			// チェック出力
			//MessageBox.Show(Path.GetFileNameWithoutExtension(m_FullNameCam) + ".xml を出力");
			//xmlDoc.Save(Path.GetFileNameWithoutExtension(m_FullNameCam) + ".xml");

			// スキーマによる検証とXPath の作成と工具交換数m_ToolCountのセット
			try { xmlD = new XmlNavi(xmlDoc, ncinfoSchemaVer.Name); }
			catch (Exception ex) {
				MessageBox.Show(ex.Message, "NcInfoCam", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
				throw;
			}

			if (ToolCount != ToolCountTemp) throw new Exception("ToolCount Program Error in NcInfoCam");
			// 工具交換数m_ToolCountのチェック
			if (ToolCount != sqlDB.Count) {
				m_LvmMess0 = new LvmMess(LVM.エラー, "m_ToolCount と sqlDB が整合しない");
				return;
			}

			// ///////////////////////////////////////////
			// 工具毎の出力名設定と送信先ファイルの確認
			// ///////////////////////////////////////////
			try { CheckOutName(); }
			catch (Exception ex) {
				m_LvmMess0 = new LvmMess(LVM.エラー, ex.Message);
				return;
			}

			// ///////////////////////////////////////////
			// クリアランス高さのチェック
			// ///////////////////////////////////////////
			string ss = this.xmlD.ClpCheck();
			if (ss != null) {
				m_LvmMess0 = new LvmMess(LVM.エラー, "クリアランス高さが２０mm未満です " + ss);
				return;
			}

			// /////////////////////////////////////////////////////////////
			// ＤＢ切削条件とＣＡＭ切削条件の整合チェック（メッセージのみ）
			// /////////////////////////////////////////////////////////////
			switch (Form1.SelCamSystem.Name) {
			case CamSystem.CADmeisterKDK:
				for (int tcnt = 0; tcnt < NcdtInfo.Count; tcnt++) {
					if (NcdtInfo[tcnt].Spidle != 1000) {
						MessageBox.Show(
							sqlDB[tcnt].toolsetTemp.ToolName + "の回転数が" +
							" S" + NcdtInfo[tcnt].Spidle.ToString() + " でありＮＣデータ仕様の１０００ではない");
					}
					if (NcdtInfo[tcnt].Feedrt != 1000) {
						MessageBox.Show(
							sqlDB[tcnt].toolsetTemp.ToolName + "の送り速度が" +
							" F" + NcdtInfo[tcnt].Feedrt.ToString() + " でありＮＣデータ仕様の１０００ではない");
					}
				}
				break;
			}
		}

		/// <summary>
		/// 出力名の設定とモードの設定
		/// </summary>
		public void CheckOutName() {
			if (m_LvmMess0.m_Mode == LVM.エラー)
				return;
			if (m_LvmMess0.m_Mode == LVM.未確認)
				return;

			m_LvmMess0 = new LvmMess(LVM.未設定, "");

			// ////////////////
			// 送信要否を調べる
			// ////////////////

			if (m_outName.Length < 3) {
				m_LvmMess0 = new LvmMess(LVM.出力名, "出力名の文字数は３文字以上です。出力名をクリックして変更してください。");
				return;
			}
			if (!Char.IsLetter(m_outName[0])) {
				m_LvmMess0 = new LvmMess(LVM.出力名, "出力名の先頭の文字はアルファベットのみです。出力名をクリックして変更してください。");
				return;
			}
			if (!Char.IsLetter(m_outName[1])) {
				m_LvmMess0 = new LvmMess(LVM.出力名, "出力名の２番目の文字はアルファベットのみです。出力名をクリックして変更してください。");
				return;
			}
			if (!Char.IsLetter(m_outName[2])) {
				m_LvmMess0 = new LvmMess(LVM.出力名, "出力名の３番目の文字はアルファベットのみです。出力名をクリックして変更してください。");
				return;
			}
			if (NcInfoCam.BuhinName.CheckName(m_outName) != true) {
				m_LvmMess0 = new LvmMess(LVM.出力名, "出力名の先頭文字部分が指定の文字列ではない。出力名をクリックして変更してください。");
				return;
			}
			string ss = CamUtil.StringLengthDB.Moji_NcOutName(this.xmlD.BaseNcFormat, ServerPC.PTPName.FileNameTrim(this.xmlD.BaseNcFormat, m_outName), ToolCount);
			if (ss.Length > 0) {
				m_LvmMess0 = new LvmMess(LVM.出力名, ss);
				return;
			}

			// //////////////////////////////////
			// ２番目の文字の意味
			//  4  5  6  7  8  9 10 11 12  1  2  3
			// AB CD EF GH IJ KL MN OP QR ST UV WX
			// //////////////////////////////////


			// ＮＣデータの存在の確認
			{
				// ＰＣからＮＣデータを検索する
				string fn = CamUtil.ServerPC.FulNcName(m_outName + ".ncd");
				if (File.Exists(fn)) {
					fulfName = Path.ChangeExtension(fn, null);
					DateTime ncdate = File.GetLastWriteTime(fulfName + ".ncd");
					if (ncdate > this.m_LastWriteTime)
						this.m_LvmMess0 = new LvmMess(LVM.送信済, this.m_LvmMess0.m_ErrorMessage);
					else
						this.m_LvmMess0 = new LvmMess(LVM.再送要, this.m_LvmMess0.m_ErrorMessage);
				}
			}
			if (m_LvmMess0.m_Mode == LVM.未設定)
				this.m_LvmMess0 = new LvmMess(LVM.未送信, this.m_LvmMess0.m_ErrorMessage);
			return;
		}
	}
}
