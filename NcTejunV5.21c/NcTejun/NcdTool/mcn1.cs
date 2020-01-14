using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.IO;
using CamUtil;

namespace NcdTool
{
	/// <summary>
	/// この加工に使用する加工機情報[不変]
	/// </summary>
	class Mcn1
	{
		/// <summary>データベースより習得した加工機情報</summary>
		private readonly MCN mcnDB;
		/// <summary>カサブランカ登録名</summary>
		public string CasMachineName { get { return (string)mcnDB.machRow["casMachineName"]; } }
		/// <summary>ＤＮＣ装置の名前。"TEXAS"かそうでないかを主に区分する</summary>
		public string DncName { get { return (string)mcnDB.machRow["DNC名"]; } }

		/// <summary>１工具１ＮＣデータの場合true</summary>
		public bool Toool_nc {
			get {
				switch ((string)mcnDB.machRow["DNC名"]) {
				case Machine.DNCName.TEXAS:
				case Machine.DNCName.DM10:
				case Machine.DNCName.HP:
					return true;
				case Machine.DNCName.AOI:
				case Machine.DNCName.CIMX:
					return false;
				default:
					throw new Exception("ＤＮＣの名称が未登録です。");
				}
			}
		}

		/// <summary>稼働実績を収集する</summary>
		public bool Log_kado {
			get {
				if ((string)mcnDB.machRow["DNC名"] == Machine.DNCName.TEXAS && this.ID != Machine.MachID.YMC430)
					return true;
				else
					return false;
			}
		}

		/// <summary>ＮＣデータ出力名の拡張子</summary>
		public string Suffix { get { return Machine.Suffix(this.ID); } }
		/// <summary>加工機ＩＤより５軸加工機のタイプを出力する</summary>
		public Machine.Machine_Axis_Type Axis_Type { get { return Machine.Axis_Type_st(this.ID); } }
		/// <summary>５面加工（工具軸指定）に使用する加工機はtrue</summary>
		public bool Milling_5Faces { get { return Machine.Milling_5Faces_st(this.ID); } }
		///// <summary>出力可能な加工機名であれば true</summary>
		public bool CheckOutput {
			get {
				foreach (string ss in Machine.MachList) if (ss == this.name) return true;
				return false;
			}
		}

		/// <summary>出力先のフォルダー名</summary>
		public string Dir_PTR { get { return CamUtil.ServerPC.SvrName + @"\h\usr9\TEXAS\NCDATA\" + this.name + @"\"; } }

		/// <summary>加工機の性能のレベル</summary>
		public string Performance { get { return (string)mcnDB.machRow["性能"]; } }
		// /// <summary>ＲＦＩＤによる工具管理の有無</summary>
		// public bool rfid { get { return (bool)mcnDB.machRow["RFID有無"]; } }
		/// <summary>工具測定方式</summary>
		public Machine.ToolMeasureType ToolMeasureType {
			get {
				switch ((string)mcnDB.machRow["ToolMeasureType"]) {
				case "接触式":
					return CamUtil.Machine.ToolMeasureType.接触式;
				case "レーザー":
					return CamUtil.Machine.ToolMeasureType.レーザー;
				case "画像式":
					return CamUtil.Machine.ToolMeasureType.画像式;
				default:
					throw new Exception("aerfbqfnzq");
				}
			}
		}
		///// <summary>固定工具番号の刃具のリスト</summary>
		//public List<MCN.magazine> Magazine_fix { get { return mcnDB.Magazine_fix; } }

		// /////////////////
		// クーラント
		// /////////////////
		/// <summary>クーラント水外部あり</summary>
		private bool CoolantWO { get { return (string)mcnDB.machRow["S_WTR_OUT"] != "0"; } }
		/// <summary>クーラント水内部あり</summary>
		private bool CoolantWI { get { return (string)mcnDB.machRow["S_WTR_IN"] != "0"; } }
		/// <summary>クーラントエア外部あり</summary>
		private bool CoolantAO { get { return (string)mcnDB.machRow["S_AIR_OUT"] != "0"; } }
		/// <summary>クーラントエア内部あり</summary>
		private bool CoolantAI { get { return (string)mcnDB.machRow["S_AIR_IN"] != "0"; } }

		/// <summary>
		/// クーラントのコード番号。ＤＭＵ系はＭコードの番号で、その他はテキサスに渡すコード番号でそれぞれの加工機内マクロでＭコードを設定
		/// </summary>
		/// <param name="coolID">クーラントを設定するID</param>
		/// <param name="mess">クーラントが設定された値を出力する。[0]はツールセットＣＡＭの初期値、[1]は最終決定値</param>
		/// <returns></returns>
		public int CoolCodeNo(ToolSetData.Coolant.ID coolID, string[] mess) {
			if (mess[0] == null) { mess[0] = ToolSetData.Coolant.Message(coolID); }	// 初期のクーラント名
			mess[1] = ToolSetData.Coolant.Message(coolID);                          // 最終のクーラント名

			switch (coolID) {
			case ToolSetData.Coolant.ID.waterOut:               // 外部クーラント（水）
				if (CoolantWO) {
					return this.Dmu
						? Convert.ToInt32(((string)mcnDB.machRow["WATER_OUT"]).Substring(1))    // クーラントのＭコード番号
						: ToolSetData.Coolant.TexasCoolantNo(ToolSetData.Coolant.ID.waterOut);  // テキサス用クーラント番号
				}
				else {
					throw new Exception(this.name + "では外部クーラント（水）を使用できません。");
				}
			case ToolSetData.Coolant.ID.waterIn:                // 内部クーラント（水）
				if (CoolantWI) {
					return this.Dmu
						? Convert.ToInt32(((string)mcnDB.machRow["WATER_IN"]).Substring(1))     // クーラントのＭコード番号
						: ToolSetData.Coolant.TexasCoolantNo(ToolSetData.Coolant.ID.waterIn);   // テキサス用クーラント番号
				}
				else {
					return CoolCodeNo(ToolSetData.Coolant.ID.waterOut, mess);
				}
			case ToolSetData.Coolant.ID.airOut:                 // 外部クーラント（エア）
				if (CoolantAO) {
					return this.Dmu
						? Convert.ToInt32(((string)mcnDB.machRow["AIR_OUT"]).Substring(1))      // クーラントのＭコード番号
						: ToolSetData.Coolant.TexasCoolantNo(ToolSetData.Coolant.ID.airOut);    // テキサス用クーラント番号
				}
				else {
					return CoolCodeNo(ToolSetData.Coolant.ID.waterOut, mess);
				}
			case ToolSetData.Coolant.ID.airIn:                  // 内部クーラント（エア）
				if (CoolantAI) {
					return this.Dmu
						? Convert.ToInt32(((string)mcnDB.machRow["AIR_IN"]).Substring(1))       // クーラントのＭコード番号
						: ToolSetData.Coolant.TexasCoolantNo(ToolSetData.Coolant.ID.airIn);     // テキサス用クーラント番号
				}
				else {
					return CoolCodeNo(ToolSetData.Coolant.ID.waterIn, mess);
				}
			case ToolSetData.Coolant.ID.nonCool:                // クーラントオフ
				return this.Dmu
					? Convert.ToInt32(((string)mcnDB.machRow["NON_COOL"]).Substring(1))			// クーラントのＭコード番号
					: ToolSetData.Coolant.TexasCoolantNo(ToolSetData.Coolant.ID.nonCool);		// テキサス用クーラント番号
			default: throw new Exception("wearfqaerf");
			}
		}


		/// <summary>加工機のテーパ規格名の有無</summary>
		public bool Taper_exist(string taper_std_name) {
			return mcnDB.taper_set.Contains(taper_std_name);
		}
		/// <summary>テーパ規格名ごとのホルダー管理区分名</summary>
		public string Holder_kubun(string taper_std_name) {
			return mcnDB.taper_set.Kubun(taper_std_name);
		}



		/// <summary>加工機名称</summary>
		public readonly string name;
		/// <summary>加工機ID</summary>
		public readonly Machine.MachID ID;
		/// <summary>ＤＭＵの加工機の場合true</summary>
		public bool Dmu { get { return ID == Machine.MachID.DMU200P || ID == Machine.MachID.DMU210P || ID == Machine.MachID.DMU210P2; } }

		/// <summary>早送り速度ＸＹ</summary>
		public int Rapx {
			get {
				// 実行速度として５０％を入れておく
				if (this.Dmu)
					return Convert.ToInt32(mcnDB.machRow["rapx"]) / 2;
				else
					return Convert.ToInt32(mcnDB.machRow["rapx"]);
			}
		}
		/// <summary>早送り速度Ｚ</summary>
		public int Rapz {
			get {
				// 実行速度として５０％を入れておく
				if (this.Dmu)
					return Convert.ToInt32(mcnDB.machRow["rapz"]) / 2;
				else
					return Convert.ToInt32(mcnDB.machRow["rapz"]);
			}
		}
		/// <summary>最大切削送り速度</summary>
		public double Fmax { get { return Convert.ToDouble(mcnDB.machRow["fmax"]); } }
		/*
		/// <summary>1=高速加工モードあり</summary>
		public int hikm { get { return (Convert.ToInt32(mcnDB.machRow["hikm"]) & 1); } }
		*/
		/// <summary>マガジン数</summary>
		public int Nmgz { get { return Convert.ToInt32(mcnDB.machRow["nmgz"]); } }
		/// <summary>工具番号フリー</summary>
		public bool FreeTNo { get { return Convert.ToBoolean(mcnDB.machRow["FreeTNo"]); } }
		/// <summary>工具交換時間</summary>
		public int Tctim { get { return Convert.ToInt32(mcnDB.machRow["tctim"]); } }

		/// <summary>スピンドル回転最大値</summary>
		public double Smax { get { return Convert.ToDouble(mcnDB.machRow["smax"]); } }
		/// <summary>スピンドル回転最小値</summary>
		public double Smin { get { return Convert.ToDouble(mcnDB.machRow["smin"]); } }
		/// <summary>スピンドル回転最大値（高速）</summary>
		public int Hisp { get { return Convert.ToInt32(mcnDB.machRow["hispmax"]); } }
		/// <summary>スピンドル回転最小値（高速）</summary>
		public int Hispmin { get { return Convert.ToInt32(mcnDB.machRow["hispmin"]); } }
		/// <summary>最大トルク(Nm)</summary>
		public double? Torque { get { return mcnDB.machRow.Field<double?>("torque"); } }

		/// <summary>常時セット工具のリスト</summary>
		public RO_Collection<Tool.Tol> Mgrs { get { return m_mgrs.AsReadOnly; } } private readonly RO_Collection<Tool.Tol>.InnerList m_mgrs;


		/// <summary>ＮＣコード</summary>
		public readonly NcCode ncCode;

		/// <summary>加工精度番号</summary>
		public int ScaleNo(string accuracy) { return mcnDB.scaleNo.First(scl => scl.accuracy == accuracy).scale_no; }

		/// <summary>メモリ内にあるマクロプログラムリスト</summary>
		public string[] MemMacro { get { return m_memMacro.ToArray(); } }
		private readonly List<string> m_memMacro;

		/// <summary>
		/// コンストラクタ(mcnget)
		/// </summary>
		/// <param name="tjnmachn">加工手順の加工機設定名</param>
		public Mcn1(string tjnmachn) {

			m_mgrs = new RO_Collection<Tool.Tol>.InnerList();

			this.name = tjnmachn;
			this.ID = Machine.GetmachID(tjnmachn);
			//this.G00toG01 = tjnmachn.Contains("_G01");

			// ＳＱＬサーバ情報内の加工機情報（テーパ規格、クーラントコード）を設定する
			mcnDB = new MCN(tjnmachn);

			if (this.CheckOutput) {
				ncCode = new NcCode(mcnDB.machRow);
			}

			// /////////////////////////////////////
			// 固定工具の設定
			// /////////////////////////////////////
			if (m_mgrs.Count != 0) throw new Exception("kaebfhaebrh");
			for (int jj = 0; jj < mcnDB.Magazine_fix.Count; jj++) {
				Tool.Tol tol = new Tool.Tol(mcnDB.Magazine_fix[jj].Tno, mcnDB.Magazine_fix[jj].Tset_name, tjnmachn);
				//m_mgrs.Add(tol);
				m_mgrs.Add(tol);
			}

			// メモリ内にあるマクロプログラムリストを取得する
			switch (this.ID) {
			case Machine.MachID.D500:
			case Machine.MachID.LineaM:
				break;
			default:
				//メモリ内にあるマクロプログラムリストを取得す
				m_memMacro = new List<string>();
				foreach (string stmp in Directory.GetFiles(CamUtil.ServerPC.SvrFldrE + tjnmachn)) {
					if (Path.GetFileName(stmp).Length == 5)
						if (Path.GetFileName(stmp).IndexOf("OG") == 0 || Path.GetFileName(stmp).IndexOf("P") == 0)
							m_memMacro.Add(Path.GetFileName(stmp));
				}
				break;
			}

			return;
		}

		/// <summary>
		/// この加工機で加工可能なＮＣデータであるかチェックする
		/// </summary>
		/// <param name="skog"></param>
		public void KakoKahi(NcName.Kogu skog) {

			if (skog.TsetCHG.Tset_probe && skog.Tld.XmlT.parent.CamSystemID.Name == CamSystem.Tebis)
				return;

			// 主型加工の場合
			switch (Tejun.BaseNcForm.Id) {
			case BaseNcForm.ID.GENERAL:

				// ５軸対応加工機
				if (skog.Tld.XmlT.Keisha) {
					if ((int)mcnDB.machRow["numAxis"] < 5) throw new Exception("傾斜加工に未対応の加工機です");
				}

				// ２次元加工の場合は工具リファレンス点は必ず"先端"である
				if (skog.Tld.XmlT.parent.CamDimension == 2 && skog.Tld.XmlT.TRTIP == false) {
					throw new Exception("次元と工具リファレンス点の組み合わせが異常");
				}

				// ３次元加工の場合で傾斜加工（同時５軸を含む）の場合は工具リファレンス点は必ず"中心"である
				//if (skog.tld.xmlT.parent.camDimension == 3 && skog.tld.xmlT.TRTIP == true && skog.tld.xmlT.keisha) {
				if (skog.Tld.XmlT.parent.CamDimension == 3 && skog.Tld.XmlT.TRTIP == true && (skog.Tld.XmlT.Keisha || skog.Tld.XmlT.SimultaneousAxisControll)) {
					throw new Exception("傾斜角度と次元と工具リファレンス点の組み合わせが異常");
				}

				// ３次元加工の場合で傾斜なしの加工の場合は工具リファレンス点"先端"だが一部"中心"も認める
				//if (skog.tld.xmlT.parent.camDimension == 3 && skog.tld.xmlT.TRTIP == false && !skog.tld.xmlT.keisha) {
				if (skog.Tld.XmlT.parent.CamDimension == 3 && skog.Tld.XmlT.TRTIP == false && !(skog.Tld.XmlT.Keisha || skog.Tld.XmlT.SimultaneousAxisControll)) {

					// "中心"で対応しているのはＤＭＧのみである
					switch (this.Axis_Type) {
					case Machine.Machine_Axis_Type.AXIS5_DMU:	// 出力されたリファレンス点の情報のみで刃先補正を実施している
						break;
					case Machine.Machine_Axis_Type.AXIS5_VG:	// G105のマクロでは、刃先補正ありで回転角度が０の場合をエラーとしている
						throw new Exception("傾斜加工以外で工具リファレンス点が'中心'の加工はＶＧでは未対応です");
					default:						// リファレンス点の情報が出力されていない（コーナーＲはテキサスに出力されている）
						throw new Exception("工具リファレンス点が'中心'の加工はこの設備では未対応です");
					}
				}
				break;

			// 部品加工の場合
			case BaseNcForm.ID.BUHIN:

				if (skog.Tld.XmlT.Keisha) {
					// "5AXIS"の工具リファレンス点は工具タイプにより決定され、ＮＣデータの"G100"でＭコードで区分されるためチェック不要
					// "M0" : 刃削補正しない、　"M1" : 刃先補正する
				}
				break;

			default: throw new Exception("qfqefr");
			}
			return;
		}


		/// <summary>
		/// ＮＣデータのフォーマット情報
		/// </summary>
		public readonly struct NcCode
		{
			/// <summary>true:絶対座標系 false:相対座標系 null:元のＮＣデータに従う（マクロ展開時相対となる場合がある）</summary>
			public bool? ZSK { get { return machRow.Field<bool?>("nccodeZSK"); } }
			/// <summary>小数点なしの場合の座標値設定単位の逆数（10000, 1000, 200, 100 など）</summary>
			public int DGT2 { get { return (int)machRow["nccodeDGT2"]; } }
			/// <summary>true:座標値小数点あり false:座標値小数点なし（マクロコール時は不明） null:元のＮＣデータに従う</summary>
			public bool? DEC { get { return machRow.Field<bool?>("nccodeDEC"); } }
			/// <summary>true:シーケンス番号あり false:シーケンス番号あり null:元のＮＣデータに従う</summary>
			public bool? SEQ { get { return machRow.Field<bool?>("nccodeSEQ"); } }
			/// <summary>true:ＸＹ座標値補間 false:ＸＹ座標値補間なし</summary>
			public bool XYH { get { return (bool)machRow["nccodeXYH"]; } }
			/// <summary>Ｄコード番号のＴ番号に対する加算値</summary>
			public int DIN { get { return (int)machRow["nccodeDIN"]; } }
			/// <summary>ドウェル時Ｐコードの時間単位（秒）</summary>
			public double TIM { get { return (double)machRow["nccodeTIM"]; } }

			/// <summary>機械番号。この加工機用のＮＣデータか確認する</summary>
			public int? MNO { get { return machRow.Field<int?>("nccodeMNO"); } }
			/// <summary>機械番号を代入するマクロ変数の番号</summary>
			public int? MAC { get { return machRow.Field<int?>("nccodeMAC"); } }
			/// <summary>ＮＣデータに挿入する高速加工モードのコード ＯＮ</summary>
			public string HSP_ON { get { return machRow.Field<string>("nccodeHSP_ON"); } }
			/// <summary>ＮＣデータに挿入する高速加工モードのコード ＯＦＦ</summary>
			public string HSP_OFF { get { return machRow.Field<string>("nccodeHSP_OFF"); } }
			/// <summary>３次元加工時にＧ０１のみで加工するか</summary>
			public bool G01 { get { return (bool)machRow["nccodeG01"]; } }
			/// <summary>３次元加工時にＧ０１のみで加工する場合の早送り速度</summary>
			public int RPD { get { return machRow.Field<int?>("nccodeRapidF") ?? (int)machRow.Field<double?>("fmax").Value; } }
			/// <summary>Ｍ０４（逆回転）に対応している加工機か</summary>
			public bool M04 { get { return (bool)machRow["nccodeM04"]; } }
			/// <summary>孔加工でのリトラクト速度。工具種類や材質による比率がかけられて実際の値となる。</summary>
			public int RetractFeed { get { return (int)machRow["nccodeRET"]; } }

			private readonly DataRow machRow;
			public NcCode(DataRow dRow) { this.machRow = dRow; }
		}

		/// <summary>
		/// ＤＢより取得する情報[不変]
		/// </summary>
		private class MCN
		{
			// //////////////
			// テーパ規格関連
			// //////////////
			public readonly Taper taper_set;
			/// <summary>
			/// テーパ規格のリスト
			/// </summary>
			public readonly struct Taper
			{
				/// <summary>テーパの規格名（A100,A63,BT40,BT50）</summary>
				private readonly List<string> m_taper_std_name;
				/// <summary>ホルダーの管理区分（A100_A,BT40_A,BT50_A,DMU200,DMU_E50,FH100）</summary>
				private readonly List<string> m_holder_kubun;

				public Taper(string machn, DataTable table) {
					m_taper_std_name = table.AsEnumerable().Where(dRow => machn == (string)dRow["設備名"]).Select(dRow => (string)dRow["テーパ規格名"]).ToList();
					m_holder_kubun = table.AsEnumerable().Where(dRow => machn == (string)dRow["設備名"]).Select(dRow => (string)dRow["ホルダー管理区分"]).ToList();
				}
				/// <summary>
				/// 指定したテーパの規格名が存在するか
				/// </summary>
				/// <param name="taper_std_name">テーパの規格名</param>
				/// <returns>存在の有無</returns>
				public bool Contains(string taper_std_name) {
					return m_taper_std_name.Contains(taper_std_name);
				}
				/// <summary>
				/// 指定したテーパの規格名のホルダー管理区分名
				/// </summary>
				/// <param name="taper_std_name">テーパの規格名</param>
				/// <returns>存在の有無</returns>
				public string Kubun(string taper_std_name) {
					return m_holder_kubun[m_taper_std_name.IndexOf(taper_std_name)];
				}
			}

			/// <summary>固定工具番号の刃具のリスト</summary>
			public readonly List<Magazine> Magazine_fix;
			/// <summary>
			/// 固定工具番号の刃具情報
			/// </summary>
			public readonly struct Magazine
			{
				private readonly string m_tset_name, m_tset_ID;
				private readonly int m_tno;

				/// <summary>ツールセット名</summary>
				public string Tset_name { get { return m_tset_name; } }
				/// <summary>ツールセットＩＤ</summary>
				public string Tset_ID { get { return m_tset_ID; } }
				/// <summary>工具番号</summary>
				public int Tno { get { return m_tno; } }

				public Magazine(string tset_name, string tset_ID, int tno) {
					this.m_tset_name = tset_name;
					this.m_tset_ID = tset_ID;
					this.m_tno = tno;
				}
			}

			/// <summary>加工精度情報</summary>
			public readonly IEnumerable<Scale> scaleNo;
			/// <summary>
			/// 加工精度情報
			/// </summary>
			public readonly struct Scale
			{
				public readonly string accuracy;
				public readonly int scale_no;
				public Scale(string acc, int no) { this.accuracy = acc; this.scale_no = no; }
			}

			/// <summary>加工機dbo.NcConv_Machのデータ</summary>
			internal readonly DataRow machRow;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="machn"></param>
			public MCN(string machn) {

				DataSet MSetInfo = new DataSet("MSetInfo");

				using (SqlConnection connection = new SqlConnection(CamUtil.ServerPC.connectionString)) {
					connection.Open();
					using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_Mach", connection)) {
						adapter.Fill(MSetInfo, "NcConv_Mach");
					}
					using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_Mach_taper", connection)) {
						adapter.Fill(MSetInfo, "NcConv_Mach_taper");
					}
					using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_Magazine", connection)) {
						adapter.Fill(MSetInfo, "NcConv_Magazine");
					}
					using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM dbo.NcConv_Mach_scale", connection)) {
						adapter.Fill(MSetInfo, "NcConv_Mach_scale");
					}
				}

				// 「テーパ規格区分」を取得する
				// クーラントコード(ncCode)をセットする
				// ＤＮＣ名（dncName）を取得する
				machRow = MSetInfo.Tables["NcConv_Mach"].AsEnumerable().FirstOrDefault(dRow => machn == dRow.Field<string>("設備名"));
				if (machRow == null) throw new Exception("fgasfgvsdfvsfdvs");

				if (Convert.ToInt32(machRow["nmgz"]) < 1)
					throw new Exception("magazine suu over flow." + Convert.ToInt32(machRow["nmgz"]).ToString());

				// 「新テーパ規格区分」を取得する
				taper_set = new Taper(machn, MSetInfo.Tables["NcConv_Mach_taper"]);

				// マガジン内固定工具番号を取得する
				Magazine_fix = MSetInfo.Tables["NcConv_Magazine"].AsEnumerable()
					.Where(dRow => machn == (string)dRow["machine_name"])
					.Select(dRow => new MCN.Magazine((string)dRow["tset_name"], (string)dRow["tset_ID"], (Int16)dRow["tool_number"])).ToList();

				// 加工精度名-加工精度Noを取得する
				scaleNo = MSetInfo.Tables["NcConv_Mach_scale"].AsEnumerable()
					.Where(dRow => machn == (string)dRow["machine_name"])
					.Select(dRow => new Scale((string)dRow["accuracy"], (Int32)dRow["scale_no"]));
				return;
			}
		}
	}
}
