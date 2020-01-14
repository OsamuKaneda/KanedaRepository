using System;
using System.Text;
using System.Windows.Forms;

using System.IO;
using CamUtil;

namespace HoudenEn_V77Convert
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>変換するＮＣデータとエクセル形式の手順データが存在するフォルダー名</summary>
		private string csvDirectory;

		/// <summary>
		/// 
		/// </summary>
		public Form1() {
			InitializeComponent();

			// ローカルホスト名、ＩＰアドレスの取得
			CamUtil.LocalHost.LocalHostSet();

			csvDirectory = "";
		}

		private void TextBox_filename_DoubleClick(object sender, EventArgs e) {
			csvDirectory = "";
			textBox_filename.Text = "";

			openFileDialog1 = new OpenFileDialog();
			openFileDialog1.ShowDialog();

			if (Path.GetExtension(openFileDialog1.FileName) == ".csv") {
				textBox_filename.Text = Path.GetFileName(openFileDialog1.FileName);
				csvDirectory = Path.GetDirectoryName(openFileDialog1.FileName);
			}
		}

		/// <summary>ＣＳＶファイルのドラッグ＆ドロップ</summary>
		private void Form1_DragEnter(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;
		}

		/// <summary>ＣＳＶファイルのドラッグ＆ドロップ</summary>
		private void Form1_DragDrop(object sender, DragEventArgs e) {
			DataObject dObject = new DataObject(DataFormats.FileDrop, e.Data.GetData(DataFormats.FileDrop));
			if (dObject.GetFileDropList().Count != 1) return;

			string stmp = dObject.GetFileDropList()[0];
			csvDirectory = "";
			textBox_filename.Text = "";

			if (Path.GetExtension(stmp) == ".csv" || Path.GetExtension(stmp) == ".CSV") {
				textBox_filename.Text = Path.GetFileName(stmp);
				csvDirectory = Path.GetDirectoryName(stmp);
			}
		}

		private void Button_OK_Click(object sender, EventArgs e) {
			int count = 0;
			string[] data;

			// 直接テキストボックスにファイル名を入力した場合
			if (csvDirectory == "") {
				if (!File.Exists(textBox_filename.Text)) {
					MessageBox.Show("指定のファイルは存在しません", "", MessageBoxButtons.OK);
					return;
				}
				if (Path.GetExtension(textBox_filename.Text) != ".csv" && Path.GetExtension(textBox_filename.Text) != ".CSV") {
					MessageBox.Show("指定のファイルはCSVファイルではありません", "", MessageBoxButtons.OK);
					return;
				}
				//変換するＮＣデータとエクセル形式の手順データが存在するフォルダー名
				csvDirectory = Path.GetDirectoryName(textBox_filename.Text);
			}

			using (StreamWriter sw_pro = new StreamWriter(csvDirectory + "\\_MAIN"))
			using (StreamReader srcsv = new StreamReader(csvDirectory + "\\" + Path.GetFileName(textBox_filename.Text), Encoding.Default)) {
				sw_pro.WriteLine("%");
				sw_pro.WriteLine("O0001");
				while (true) {
					if (srcsv.EndOfStream) break;
					data = srcsv.ReadLine().SplitCsv();
					if (data[0].Replace(" ", "") == "") break;
					if (!File.Exists(csvDirectory + "\\" + data[46])) continue;

					switch (data[6]) {
					case "ﾎﾞｰﾙ":
					case "ﾌﾗｯﾄ":
					case "ﾌﾞﾙﾉｰｽﾞ":
						break;
					default:
						throw new Exception("未設定の工具タイプです");
					}

					count++;
					NcConvert(
						csvDirectory + "\\" + data[46],                 // ＮＣデータ名
						data[6] == "ﾎﾞｰﾙ" ? 'B' : 'E',                  // 工具種類（ ﾎﾞｰﾙ, ﾌﾗｯﾄ, ﾌﾞﾙﾉｰｽﾞ ）
						Convert.ToDouble(data[3].Replace("φ", "")), // 工具径
						Convert.ToDouble(data[32]),                     // 突出し
						Convert.ToDouble(data[4].Replace("R", "")),     // コーナーＲ
						Convert.ToInt32(data[7]),                       // 工具番号
						count + 1000);                                  // Ｏ番号

					// メインプログラムへの出力 add in 2018/03/15
					sw_pro.WriteLine("(+INC C:\\NcData\\{0}.dnc +)", Path.ChangeExtension(data[46], "dnc"));
					if (count == 1)
						sw_pro.WriteLine("(+INC C:\\NcData\\掃除\\cleantec.dnc +)");
				}
			}
			// ログを保存する
			CamUtil.LogOut.CheckOutput(CamUtil.LogOut.FNAM.NCSENDLOG, CamUtil.LocalHost.Name, "HodenEN_V77",
				$"{Path.GetFileName(textBox_filename.Text)} {CamUtil.LocalHost.IPAddress} {csvDirectory}");

			MessageBox.Show(count.ToString() + " 個のＮＣデータを変換しました。");
		}

		private void Button_CANCEL_Click(object sender, EventArgs e) {
			this.Close();
		}

		/// <summary>
		/// １ＮＣデータの変換
		/// </summary>
		/// <param name="fnam">ＮＣデータのフルパス名</param>
		/// <param name="type">工具種類（ ﾎﾞｰﾙ, ﾌﾗｯﾄ, ﾌﾞﾙﾉｰｽﾞ ）</param>
		/// <param name="diam">工具径</param>
		/// <param name="tsuk">突出し</param>
		/// <param name="crad">コーナーＲ</param>
		/// <param name="tlno">工具番号</param>
		/// <param name="onum">Ｏ番号</param>
		private void NcConvert(string fnam, char type, double diam, double tsuk, double crad, int tlno, int onum) {
			string outLine;

			using (StreamWriter ncsw = new StreamWriter(Path.ChangeExtension(fnam, "dnc"), false, Encoding.Default))
			using (CamUtil.LCode.StreamNcR2 ncsr = new CamUtil.LCode.StreamNcR2(fnam, null, (CamUtil.LCode.INcConvert)new Conv_V77(type, diam, onum))) {

				while (true) {
					outLine = ncsr.ReadLine();
					if (outLine == null)
						break;
					if (outLine != "")
						ncsw.WriteLine(outLine);
					Application.DoEvents();
				}
			}
		}
	}

	class Conv_V77 : CamUtil.LCode.INcConvert
	{
		/// <summary>工具タイプ</summary>
		private readonly string toolType;
		/// <summary>工具直径</summary>
		private readonly double toolDiam;
		/// <summary>Ｏ番号</summary>
		private double oNumber;

		/// <summary>ＮＣデータが保存されているバッファー</summary>
		public CamUtil.LCode.NcQueue NcQue { get { return m_ncQue; } }
		private readonly CamUtil.LCode.NcQueue m_ncQue;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">工具のタイプ</param>
		/// <param name="diam">工具直径</param>
		/// <param name="onum">Ｏ番号</param>
		public Conv_V77(char type, double diam, int onum) {
			this.toolType = type.ToString();
			this.toolDiam = diam;
			this.oNumber = onum;
			this.m_ncQue = new CamUtil.LCode.NcQueue(10, true, new double[] { 100.0 }, CamUtil.BaseNcForm.GRAPHITE, CamUtil.LCode.NcLineCode.GeneralDigit, true, true);
		}

		public CamUtil.LCode.OutLine ConvExec() {
			CamUtil.LCode.NcLineQue txtd = this.NcQue[0];
			switch (txtd.LnumN) {
			case 1:
			case 2:
			case 3:
				if (txtd.NcLine.Replace("%", "").Length != 0) throw new Exception("ＮＣデータエラー");
				break;
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
				txtd.OutLine.Set("");
				txtd.OutLine.CommOut = false;
				break;
			case 14:
				if (txtd.LnumN == 14 && txtd.NcLine != "M03" && txtd.NcLine != "M04") throw new Exception("ＮＣデータエラー");
				txtd.OutLine.MaeAdd("O" + oNumber.ToString());
				txtd.OutLine.MaeAdd("#527=12.");
				txtd.OutLine.Set("G100" + NcQue[-8].NcLine + toolType + (toolDiam * 1000.0).ToString() + NcQue[-1].NcLine);
				txtd.OutLine.AtoAdd("G00G90X0Y0");
				txtd.OutLine.AtoAdd("Z" + NcQue[-4].Xyzsf.Z.ToString("0.0"));
				break;
			default:
				switch (txtd.Lnumb) {
				case -9:
					if (txtd.Lnumb == -9 && txtd.NcLine != "M09") throw new Exception("ＮＣデータエラー");
					txtd.OutLine.Set("M98P6");
					txtd.OutLine.AtoAdd("M30");
					break;
				case -8:
				case -7:
				case -6:
				case -5:
				case -4:
				case -3:
				case -2:
					txtd.OutLine.Set("");
					txtd.OutLine.CommOut = false;
					break;
				}
				break;
			}
			return txtd.OutLine;
		}
	}
}
