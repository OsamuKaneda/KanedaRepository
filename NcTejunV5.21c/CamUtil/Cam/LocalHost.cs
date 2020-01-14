using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using System.IO;

namespace CamUtil
{
	/// <summary>
	/// 実行しているＰＣの情報を提供するクラスです。
	/// </summary>
	public static class LocalHost
	{
		/// <summary>ＰＣ名</summary>
		public static string Name { get { return m_Name; } }
		private static string m_Name;
		/// <summary>ＩＰアドレス</summary>
		public static string IPAddress { get { return m_IPAddress; } }
		private static string m_IPAddress;
		/// <summary>ホームディレクトリ</summary>
		public static string Homedir { get { return m_homedir; } }
		private static string m_homedir = "";
		/// <summary>テンポラリディレクトリ</summary>
		public static string Tempdir { get { return m_tempdir; } }
		private static string m_tempdir = "";
		/// <summary>ＮＣデータの基本ディレクトリ（最後の\なし）</summary>
		public static string Ncdtdir { get { return m_ncdtdir; } }
		private static string m_ncdtdir;

		/// <summary>
		/// ホスト名、ローカルＩＰアドレスの取得
		/// </summary>
		public static void LocalHostSet() {
			m_Name = System.Net.Dns.GetHostName();
			System.Net.IPHostEntry ipentry = System.Net.Dns.GetHostEntry(m_Name);
			foreach (System.Net.IPAddress ipa in ipentry.AddressList) {
				if (ipa.ToString().Length <= 4) continue;
				if (ipa.ToString()[3] == '.') {
					m_IPAddress = ipa.ToString();
					return;
				}
			}

			// ////////////////////////////////////////////
			// 完全なＩＰアドレスの取得（エラー処理で使用）
			// ////////////////////////////////////////////
			string mess = "";
			foreach (System.Net.NetworkInformation.NetworkInterface nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()) {
				mess += "----";
				mess += String.Format("名称： {0}\n", nic.Name);
				mess += String.Format("説明： {0}\n", nic.Description);
				mess += String.Format("タイプ： {0}\n", nic.NetworkInterfaceType);

				System.Net.NetworkInformation.IPInterfaceProperties ip = nic.GetIPProperties();
				if (ip != null) {
					foreach (System.Net.NetworkInformation.UnicastIPAddressInformation addr in ip.UnicastAddresses)
						mess += String.Format("    IPアドレス： {0}\n", addr.Address.ToString());

					if (nic.Supports(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)) {
						System.Net.NetworkInformation.IPv4InterfaceProperties ipv4 = ip.GetIPv4Properties();
						if (ipv4 != null)
							mess += String.Format("    IPv4 MTU： {0:d}\n", ipv4.Mtu);
					}

					if (nic.Supports(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)) {
						System.Net.NetworkInformation.IPv6InterfaceProperties ipv6 = ip.GetIPv6Properties();
						if (ipv6 != null)
							mess += String.Format("    IPv6 MTU： {0}\n", ipv6.Mtu);
					}
				}
			}
			throw new Exception(mess);
		}

		/// <summary>
		/// 一般に使用するホーム＆ローカルディレクトリ名を設定
		/// </summary>
		public static void HomeTempDirSet() {
			string dirName;
			if (ProgVersion.Debug) {
				dirName = @"C:\ncd\TGProgram";
				if (!Directory.Exists(dirName))
					throw new Exception("指定されたフォルダ（" + dirName + "）が存在しません");
			}
			else
				dirName = Directory.GetCurrentDirectory();

			m_homedir = dirName + @"\";
			m_ncdtdir = Directory.GetParent(dirName).FullName;
			m_tempdir = dirName + @"\temp";
			if (!Directory.Exists(m_tempdir))
				Directory.CreateDirectory(m_tempdir);
		}
	}
}
