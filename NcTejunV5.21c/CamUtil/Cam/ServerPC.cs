using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using System.IO;
using System.Windows.Forms;

namespace CamUtil
{
	/// <summary>
	/// ＰＣサーバ内の情報を提供します。
	/// </summary>
	static public class ServerPC
	{
		/// <summary>サーバ名</summary>
		public const string SvrName = @"\\nt0040np";

		/// <summary>サーバのフォルダー名（ＮＣデータ）</summary>
		public static string SvrFldrN { get { return SvrName + @"\h\usr9\ASDM\PTP\"; } }
		/// <summary>サーバのフォルダー名（加工情報データ）</summary>
		public static string SvrFldrC { get { return SvrName + @"\h\usr9\ASDM\CamInfo\"; } }

		/// <summary>加工機情報のディレクトリ/usr1/ASDM/CAMCTL/NCSPC/</summary>
		public static string SvrFldrM { get { return SvrName + @"\h\usr9\ASDM\CAMCTL\NCSPC\"; } }
		/// <summary>加工機マクロのディレクトリ\usr9\ASDM\CAMCTL\NCMEM\</summary>
		public static string SvrFldrE { get { return SvrName + @"\h\usr9\ASDM\CAMCTL\NCMEM\"; } }
		/// <summary>サーバのフォルダー名（NCSPEEDデータ旧）</summary>
		public static string SvrFldrS { get { return SvrName + @"\h\usr9\ASDM\NCSPEED\"; } }
		/// <summary>サーバのフォルダー名（NCSPEEDデータ計算後）</summary>
		public static string SvrFldrSedt { get { return SvrName + @"\h\usr9\ASDM\NCSPEED_edit\"; } }
		/// <summary>サーバのフォルダー名（NCSPEEDデータ計算前）</summary>
		public static string SvrFldrSorg { get { return SvrName + @"\h\usr9\ASDM\NCSPEED_org\"; } }

		/// <summary> 加工情報ＤＢへの接続 </summary>
		public static string connectionString = "Data Source=nt0040np;Initial Catalog=加工情報;User ID=cadceus;Password=3933";
		//public static string connectionString = "Data Source=nt0040np;Initial Catalog=加工情報;Integrated Security=SSPI;";

		/// <summary> 最新の実行ファイルが保存されているフォルダー </summary>
		public static string ExecFldr = @"\\nt0040np\tgsite_data\Program\Files\ＣＡＭ最新ソフト\";




		/// <summary>検証済手順のフォルダー名</summary>
		public static string EDT { get { return Path.GetFileName(SvrFldrSedt.TrimEnd(new char[] { '\\' })); } }
		/// <summary>未検証手順のフォルダー名</summary>
		public static string ORG { get { return Path.GetFileName(SvrFldrSorg.TrimEnd(new char[] { '\\' })); } }

		/// <summary>
		/// ＮＣデータ名に絶対パスを付ける
		/// </summary>
		/// <param name="ncname">ＮＣデータ名</param>
		/// <returns>ＮＣデータをＰＣに保存するフルパス名</returns>
		public static string FulNcName(string ncname) {
			return
				SvrFldrN +
				(tempfolder.Length >= 2 ? tempfolder : Path.GetFileNameWithoutExtension(ncname).Substring(1, 2).ToUpper()) +
				@"\" + ncname;
		}
		/// <summary> ＮＣデータをチェックする場合に使用するPTPの下のフォルダー名 </summary>
		public static void TempFolder() {
			tempfolder = "";
			string ss = "";
			CamUtil.FormInputBox aa = new FormInputBox("PTP直下のフォルダー名（２文字以上）", "フォルダー任意", "", null, null);
			while (ss != null && ss.Length < 2) ss = aa.ShowDialog();
			aa.Close();
			if (ss != null) tempfolder = ss;
		}
		private static string tempfolder = "";


		/// <summary>
		/// 手順書のファイル名からフルパス名を作成する
		/// </summary>
		public static string TejunName(string org_edt, string tjnname) {
			string TejunName;
			if (tjnname[1] != 'Y' && (tjnname[0] == 'D' || tjnname[0] == 'L'))
				TejunName = Path.GetFileNameWithoutExtension(tjnname).Substring(1, 2).ToUpper() + "_" + tjnname.Substring(0, 1) + "\\" + tjnname;
			else
				TejunName = Path.GetFileNameWithoutExtension(tjnname).Substring(1, 2).ToUpper() + "\\" + tjnname;

			switch (org_edt) {
			case "EDT": return ServerPC.SvrFldrSedt + TejunName;
			case "ORG": return ServerPC.SvrFldrSorg + TejunName;
			default: throw new Exception("crt_opn_imp ERROR");
			}
		}

		/// <summary>
		/// ＰＴＰ直下の新たなＮＣデータ保存用フォルダーを月度のチェック実施のうえ作成します。
		/// </summary>
		/// <param name="outn"></param>
		public static void CreateDirectory(string outn) {
			string fldName = Path.GetDirectoryName(ServerPC.FulNcName(outn));
			if (Directory.Exists(fldName))
				return;

			int imoji, inmon;
			// [0]     A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P  Q  R  S  T  U  V  W  X  Y  Z
			// imoji   0  0  1  1  2  2  3  3  4  4  5  5  6  6  7  7  8  8  9  9 10 10 11 11 12 12
			// month JanJanFebFebMarMarAprAprMayMayJunJunJulJulAugAugSepSepOctOctNovNovDecDec
			imoji = StringCAM.ABC0.IndexOf(Path.GetFileName(fldName)[0]) / 2;
			inmon = DateTime.Now.Month - 4;
			if (inmon < 0) inmon += 12;

			if (imoji < 12) {
				if (imoji == inmon) {
					DialogResult result = MessageBox.Show(
						"新たな金型のためのフォルダー '" + Path.GetFileName(fldName) + "' を作成します。",
						"フォルダー作成", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else {
					DialogResult result = MessageBox.Show(
						"ＮＣデータ名の付け方の規則に従っていません。フォルダー '" + Path.GetFileName(fldName) + "' を作成しますか？\n" +
						"（NCSEND2の'送信'コラム内の出力名は編集可能です。検討用は２文字目を'Z'にしてください）",
						"フォルダー作成", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
					if (result == DialogResult.Cancel)
						throw new Exception();
				}
			}
			Directory.CreateDirectory(fldName);
			return;
		}

		/// <summary>
		/// 部品加工でＰＴＰに保存する場合に指定したファイル名の先頭に追加する識別名を制御します。
		/// </summary>
		public readonly struct PTPName
		{
			/// <summary>識別文字列の長さ in _5AXIS</summary>
			private const int addFileNameLength = 5;
			/// <summary>ＮＣデータの出力ファイル名（ＮＣデータ識別文字列とそれに続く"_"を除いたもの）</summary>
			public static string FileNameTrim(BaseNcForm bncf, string fname) {
				switch (bncf.Id) {
				case BaseNcForm.ID.BUHIN:
					return (fname[addFileNameLength] == '_') ? fname.Substring(addFileNameLength + 1) : fname.Substring(addFileNameLength);
				default: return fname;
				}
			}

			// 以上 static


			/// <summary>ＮＣデータ識別文字</summary>
			private readonly string name;
			/// <summary>識別文字の入力有無</summary>
			public bool NameExist { get { return name != null; } }
			/// <summary>ＮＣデータに追加する識別文字</summary>
			public string AddName { get { return name.Length == 0 ? "" : name + "_"; } }

			/// <summary>
			/// 部品であるかを指定してPTPNameを作成します
			/// </summary>
			/// <param name="buhin">部品加工の場合はtrue</param>
			public PTPName(bool buhin) {
				string dir;
				DialogResult result0;

				if (buhin) {
					while (true) {
						FormInputBox aa = new FormInputBox("ＮＣデータを識別する英字５文字を入力", "NcName", "", 400, 200);
						name = aa.ShowDialog();
						aa.Close();
						Application.DoEvents();
						if (name == null) return; // CANCEL 選択時

						// 追加するＮＣデータの先頭文字列をチェックする
						if (name.Length != addFileNameLength) continue;
						if (Char.IsLetter(name, 0) != true) continue;
						if (Char.IsLetter(name, 1) != true) continue;
						if (Char.IsLetter(name, 2) != true) continue;
						if (Char.IsLetter(name, 3) != true) continue;
						if (Char.IsLetter(name, 4) != true && Char.IsDigit(name, 4) != true) continue;
						//if (Char.ToUpper(m_name[1]) != 'Y') continue;

						name = name.ToUpper();
						dir = Path.GetDirectoryName(ServerPC.FulNcName(name));
						if (Directory.Exists(dir) != true) break;
						if (Directory.GetFiles(dir, name + "*", SearchOption.TopDirectoryOnly).Length == 0) break;
						result0 = MessageBox.Show("この文字で始まるＮＣデータがすでに存在します", "ＮＣデータファイル名", MessageBoxButtons.OKCancel);
						if (result0 == DialogResult.OK) break;
					}
				}
				else name = "";
			}

			/// <summary>
			/// ファイル名の先頭がＮＣデータ識別文字と一致するかをチェックする
			/// </summary>
			/// <param name="fname">ファイル名</param>
			/// <returns></returns>
			public bool CheckName(string fname) {
				if (name == "") return true;
				if (name == fname.Remove(addFileNameLength)) return true;
				return false;
			}
		}
	}
}