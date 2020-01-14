using System;
using System.Collections.Generic;
using System.Text;

using CamUtil;
using CamUtil.LCode;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace NCSEND2.Output
{
	/// <summary>
	/// ＮＣデータ、ＮＣデータの情報（XML）、ユニックス用の情報を出力し、所定のフォルダーに保存する
	/// ＣＡＭシステムごとに派生クラスを設ける
	/// </summary>
	abstract partial class NcOutput : IDisposable
	{
		/// <summary>Ｇ以外のアルファベット大文字の配列</summary>
		protected static char[] ABC_G = "ABCDEFHIJKLMNOPQRSTUVWXYZ".ToCharArray();
		/// <summary>小数点以下の桁数</summary>
		public static short decimalNum;
		private static System.Globalization.CultureInfo cultureUS = new System.Globalization.CultureInfo("en-US");

		/// <summary>
		/// 各ＣＡＭシステムから出力されたＮＣデータを共通フォーマットに変換するNcOutput クラスを作成する
		/// </summary>
		/// <param name="camSystem">ＣＡＭシステム情報</param>
		/// <param name="ncd">ＮＣデータ１ファイルの情報</param>
		/// <param name="kataZ">クリアランス面の値（CADmeisterKDK でのみ使用）</param>
		/// <returns>NcOutput クラス</returns>
		public static NcOutput Factory(CamSystem camSystem, NCINFO.NcInfoCam ncd, TextBox kataZ) {
			switch (camSystem.Name) {
			case CamSystem.Tebis:
				switch (ncd.xmlD.BaseNcFormat.Id) {
				case BaseNcForm.ID.GENERAL:
					return new NcOutput_Tebis_OM(new NcReader_Default(ncd), ncd);
				case BaseNcForm.ID.BUHIN:
					return new NcOutput_Tebis_BU(new NcReader_Buhin(ncd), ncd);
				default: throw new Exception("awefqbfrhh");
				}
			case CamSystem.Dynavista2D:
				return new NcOutput_Dynavista2D(new NcReader_Default(ncd), ncd);
			case CamSystem.CAMTOOL:
				return new NcOutput_CAMTOOL(new NcReader_Default(ncd) , ncd);
			case CamSystem.CAMTOOL_5AXIS:
				return new NcOutput_CAMTOOL_5AXIS(new NcReader_Default(ncd), ncd);
			case CamSystem.CADCEUS:
				return new NcOutput_CADCEUS(new NcReader_Default(ncd), ncd);
			case CamSystem.WorkNC:
				switch (ncd.xmlD.PostProcessor.Id) {
				case PostProcessor.ID.GOUSEI:
					return new NcOutput_WorkNC(new NcReader_WorkNC(ncd), ncd);
				case PostProcessor.ID.gousei5:
					return new NcOutput_WorkNC_5AXIS(new NcReader_WorkNC(ncd), ncd);
				default: throw new Exception("frebqrfh");
				}
			/*
			case CamSystem.WorkNC_5AXIS:
				return new NcOutput_WorkNC_5AXIS(new NcReader_WorkNC(ncd), ncd);
			*/
			/*
			case CamSystem.Caelum:
				return new NcOutput_Caelum(ncd, Convert.ToDouble(kataZ.Text));
			case CamSystem.CADmeisterK:
				return new NcOutput_CADmeisterK(ncData, comboBox1.Text == "STEEL", sosin);
			*/
			case CamSystem.CADmeisterKDK:
				return new NcOutput_CADmeisterK(new NcReader_CADmeisterKDK(ncd) , ncd, Convert.ToDouble(kataZ.Text));
			default:
				throw new Exception("このＣＡＭソフトは未登録です。in NcOutput.Factory()");
			}
		}

		// 以上 static




		/// <summary>ベースＮＣフォーマット</summary>
		protected BaseNcForm bname;
		/// <summary>ＣＡＭのポスト仕様</summary>
		protected NcDigit post;

		/// <summary>ＮＣデータ情報</summary>
		protected NCINFO.NcInfoCam ncd;
		/// <summary>送信の可否</summary>
		protected bool sosin;
		/// <summary>ユニックスへの送信</summary>
		protected bool sendToUnix;
		/// <summary>
		/// ３次元加工でもＸＭＬの更新を実行する。
		/// 参考：NonFeedLen==0 &amp;&amp; NonFeedTime==0 &amp;&amp; CuttingTime==null の場合、エアーカット比は出力されない。
		/// </summary>
		protected bool xmlUpdate;

		/// <summary>入力ＮＣデータの情報を保存するキュー</summary>
		protected NcQueue ncQue;

		/// <summary>アプローチＺの工具単位リスト</summary>
		private double[] aprzd;

		/// <summary>最後にストリームから読み出した行から作成したNcCode</summary>
		protected NcLineQue ncCodeLast;

		/// <summary>突出し量の設定に利用する金型上面の高さ</summary>
		protected double kataZ;

		protected NcReader sr;

		/// <summary>コンストラクタ</summary>
		/// <param name="p_sr">前処理とともにＮＣデータを読む</param>
		/// <param name="p_ncd">出力するＮＣデータの情報</param>
		protected NcOutput(NcReader p_sr, NCINFO.NcInfoCam p_ncd) {
			this.sr = p_sr;
			this.ncd = p_ncd;
			this.bname = p_ncd.xmlD.BaseNcFormat;
			this.post = new NcDigit(p_ncd.xmlD.PostProcessor);
		}

		/// <summary>
		/// 加工機共通の条件設定
		/// </summary>
		/// <param name="steel"></param>
		/// <param name="sosin">データをＰＴＰに取込む場合 true</param>
		/// <param name="sendToUnix">データをユニックスに送信する場合 true</param>
		/// <param name="xmlUpdate">ＸＭＬを２次元加工以外でも更新する場合</param>
		public void JokenSet(bool steel, bool sosin, bool sendToUnix, bool xmlUpdate) {

			this.sosin = sosin;
			this.sendToUnix = sendToUnix;
			this.xmlUpdate = xmlUpdate;

			decimalNum = 3;

			// Ｚホームポシションのセット
			aprzd = new double[this.ncd.ToolCount];
			for (int ii = 0; ii < this.ncd.ToolCount; ii++)
				aprzd[ii] = this.ncd.xmlD[ii].TlAPCHZ;

			if (
				this is NcOutput_CADCEUS ||
				this is NcOutput_CADmeisterK ||
				this is NcOutput_CAMTOOL ||
				this is NcOutput_CAMTOOL_5AXIS ||
				this is NcOutput_Dynavista2D ||
				//this is NcOutput_Tebis ||
				this is NcOutput_Tebis_BU ||
				this is NcOutput_WorkNC ||
				this is NcOutput_WorkNC_5AXIS
				) {
				LogOut.CheckZantei("NcDequeへのダミー出力とNcQueue.dummyDataを廃止する");
				if (this is NcOutput_CADCEUS || this is NcOutput_CADmeisterK || this is NcOutput_CAMTOOL_5AXIS) {
					if (CamUtil.ProgVersion.Debug == false) LogOut.CheckError("新しい方法で入力用 NcLine の初期設定を実施する。");
				}
				ncQue = new NcQueue(10);
				// 入力用 NcLine の初期設定
				ncCodeLast = new NcLineQue(aprzd, bname, post, false, false);
				// 後のループ制御のために初期化データ（無意味）を１つ出力する
				ncQue.NcDeque(ncCodeLast);
			}
			else {
				ncQue = new NcQueue(10, false, aprzd, bname, post, false, false);
			}
		}

		/// <summary>
		/// １ＮＣデータファイルの情報を出力します
		/// </summary>
		/// <param name="disp_message">実行時に表示するメッセージ Application.DoEvents(); で更新する</param>
		public virtual void FtpPut(Label disp_message)
		{

			//List<NcLineCode.NcDist> kyoriList = new List<NcLineCode.NcDist>();

			string outn = ncd.OutName;
			if (this.sosin)
				ServerPC.CreateDirectory(outn);


			// /////////////////////////////////////////
			// ＮＣデータを作成
			// /////////////////////////////////////////
			{
				disp_message.Text = "ＮＣデータの作成";
				Application.DoEvents();
				NcLineCode.NcDist passLength;
				using (StreamWriter swn = new StreamWriter(outn + ".ncd", false)) {
					for (int tcnt = 0; tcnt < ncd.ToolCount; tcnt++) {
						Application.DoEvents();
						try {
							passLength = NcConvertN(tcnt, swn, aprzd[tcnt]);
							if (ncd.xmlD.CamDimension == 2 || xmlUpdate) {
								// 工具中心のＮＣデータの場合、工具半径補正する
								if (ncd.xmlD[tcnt].TRTIP == false)
									passLength.ShiftZ(-ncd.sqlDB[tcnt].toolsetTemp.Crad);
								// ＸＭＬの加工長の修正とＸＹＺ移動範囲の挿入
								ncd.XmlEditFeed(tcnt, passLength);
								// 加工時間の設定
								ncd.XmlEditTime(tcnt, passLength);
								// 工具突き出し量の修正
								switch (Form1.SelCamSystem.Name) {
								//case CamSystem.Caelum:
								case CamSystem.CADmeisterKDK:
									ncd.XmlEditULength(tcnt,
										this.kataZ - passLength.Min.Value.Z + 5.0,
										ncd.sqlDB[tcnt].toolsetTemp.Diam, false);
									break;
								}
							}
						}
						catch (Exception ex) {
							sr.Close();
							throw new Exception($"ＮＣデータ:{Path.GetFileName(ncd.FullNameCam)} ツールセット:{ncd.sqlDB[tcnt].tscamName} エラー\n{ex.Message}");
						}
					}
					Application.DoEvents();
				}
			}

			// /////////////////////////////////////////
			// ＸＭＬファイルの作成（tempに保存）
			// /////////////////////////////////////////
			disp_message.Text = "ＸＭＬファイルの作成";
			Application.DoEvents();
			ncd.XmlSave(outn + ".xml");


			// ///////////////////////////
			// 送信しない場合はここで終了
			// ///////////////////////////
			if (this.sosin == false) return;


			// //////////////////////////////////////////////////
			// ＮＣデータのＰＣサーバーへの送信処理
			// //////////////////////////////////////////////////
			disp_message.Text = "ＰＣにデータを送信";
			Application.DoEvents();
			// ＰＣのＮＣデータとＸＭＬデータのセット
			{
				// 不要なＮＣデータやＸＭＬがＰＣにある場合、ここで消去
				if (File.Exists(ServerPC.FulNcName(outn + ".ncd")))
					File.Delete(ServerPC.FulNcName(outn + ".ncd"));
				if (File.Exists(ServerPC.FulNcName(outn + ".xml")))
					File.Delete(ServerPC.FulNcName(outn + ".xml"));
				File.Move(outn + ".ncd", ServerPC.FulNcName(outn + ".ncd"));
				File.Move(outn + ".xml", ServerPC.FulNcName(outn + ".xml"));
			}
			Application.DoEvents();
		}

		/// <summary>
		/// ＮＣデータ１工具分をReadNcd()を用いて読込み、修正を加えてストリームtfooに出力する。ＣＡＭシステム毎にオーバーライドされる。
		/// </summary>
		/// <param name="tcnt">工具順の数</param>
		/// <param name="tfoo">ＮＣデータの出力先</param>
		/// <param name="aprzd">加工原点Ｚ</param>
		/// <returns>パスの移動距離</returns>
		protected abstract NcLineCode.NcDist NcConvertN(int tcnt, StreamWriter tfoo, double aprzd);

	}
}
