using System;
using System.Runtime.InteropServices;//Marshalのために追加

namespace CamUtil
{
	/// <summary>
	/// NativeMethods（アンマネージまとめるクラス）
	/// </summary>
	internal static class NativeMethods
	{
		/// <summary>
		/// WNetGetUniversalNameをインポートする
		/// </summary>
		/// <param name="lpLocalPath">ネットワーク資源のパス</param>
		/// <param name="dwInfoLevel">情報のレベル</param>
		/// <param name="lpBuffer">名前バッファ</param>
		/// <param name="lpBufferSize">バッファのサイズ</param>
		/// <returns></returns>
		[DllImport("mpr.dll", CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.U4)]
		internal static extern int WNetGetUniversalName(
			string lpLocalPath,
			[MarshalAs(UnmanagedType.U4)] int dwInfoLevel,
			IntPtr lpBuffer,
			[MarshalAs(UnmanagedType.U4)] ref int lpBufferSize
		);
	}
	/// <summary>
	/// ネットワークドライブ名をUNC（Universal Naming Convention）名に変換する
	/// </summary>
	static public class UniversalName
	{
		// dwInfoLevelに指定するパラメータ
		// lpBuffer パラメータが指すバッファで受け取る構造体の種類を次のいずれかで指定

		const int universalNameInfoLevel = 0x00000001;
		const int remoteNameInfoLevel = 0x00000002; //こちらは、テストしていない


		 // lpBufferで受け取る構造体
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		struct UNIVERSAL_NAME_INFO
		{
			public string lpUniversalName;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		struct _REMOTE_NAME_INFO  //こちらは、テストしていない
		{
			readonly string lpUniversalName;
			readonly string lpConnectionName;
			readonly string lpRemainingPath;
		}

		// エラーコード一覧
		// WNetGetUniversalName固有のエラーコード
		//   http://msdn.microsoft.com/ja-jp/library/cc447067.aspx
		// System Error Codes (0-499)
		//   http://msdn.microsoft.com/en-us/library/windows/desktop/ms681382(v=vs.85).aspx

		const int error_NO = 0;
		const int error_NOT_SUPPORTED = 50;
		const int error_MORE_DATA = 234;
		const int error_BAD_DEVICE = 1200;
		const int error_CONNECTION_UNAVAIL = 1201;
		const int error_NO_NET_OR_BAD_PATH = 1203;
		const int error_EXTENDED_ERROR = 1208;
		const int error_NO_NETWORK = 1222;
		const int error_NOT_CONNECTED = 2250;


		/// <summary>
		/// UNC変換ロジック本体
		/// </summary>
		/// <param name="path_src"></param>
		/// <returns></returns>
		static public string GetUniversalName(string path_src) {
			string unc_path_dest = path_src; //解決できないエラーが発生した場合は、入力されたパスをそのまま戻す
			int size = 1;

			// 前処理
			// 意図的に、ERROR_MORE_DATAを発生させて、必要なバッファ・サイズ(size)を取得する。

			//1バイトならば、確実にERROR_MORE_DATAが発生するだろうという期待。
			IntPtr lp_dummy = Marshal.AllocCoTaskMem(size);

			//サイズ取得をトライ
			int apiRetVal = NativeMethods.WNetGetUniversalName(path_src, universalNameInfoLevel, lp_dummy, ref size);

			//ダミーを解放
			Marshal.FreeCoTaskMem(lp_dummy);


			// UNC変換処理
			switch (apiRetVal) {
			case error_MORE_DATA:
				//受け取ったバッファ・サイズ(size)で再度メモリ確保
				IntPtr lpBufUniversalNameInfo = Marshal.AllocCoTaskMem(size);

				//UNCパスへの変換を実施する。
				apiRetVal = NativeMethods.WNetGetUniversalName(path_src, universalNameInfoLevel, lpBufUniversalNameInfo, ref size);

				//UNIVERSAL_NAME_INFOを取り出す。
				UNIVERSAL_NAME_INFO a = (UNIVERSAL_NAME_INFO)Marshal.PtrToStructure(lpBufUniversalNameInfo, typeof(UNIVERSAL_NAME_INFO));

				//バッファを解放する
				Marshal.FreeCoTaskMem(lpBufUniversalNameInfo);

				if (apiRetVal == error_NO) {
					//UNCに変換したパスを返す
					unc_path_dest = a.lpUniversalName;
				}
				else {
					//MessageBox.Show(path_src +"ErrorCode:" + apiRetVal.ToString());
				}
				break;
			case error_BAD_DEVICE:		//すでにUNC名(\\servername\test)
			case error_NOT_CONNECTED:	//ローカル・ドライブのパス(C:\test)
				//MessageBox.Show(path_src +"\nErrorCode:" + apiRetVal.ToString());
				break;
			default:
				//MessageBox.Show(path_src + "\nErrorCode:" + apiRetVal.ToString());
				break;
			}

			return unc_path_dest;
		}
	}
}
