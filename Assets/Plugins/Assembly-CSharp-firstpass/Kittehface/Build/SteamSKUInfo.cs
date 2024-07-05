using Steamworks;
using UnityEngine;

namespace Kittehface.Build
{
	[CreateAssetMenu(fileName = "SteamSKUInfo", menuName = "KF Framework/SKU Info/Steam SKU Info", order = 1)]
	public class SteamSKUInfo : SKUInfo
	{
		private const string XML_GAME_ID = "gameID";

		private const string XML_WINDOWS_EXECUTABLE_NAME = "windowsExeName";

		private const string XML_MAC_APP_NAME = "macAppName";

		private const string XML_LINUX_EXECUTABLE_NAME = "linuxExeName";

		[Header("Steam")]
		[SerializeField]
		private uint gameID;

		[SerializeField]
		private string windowsExecutableName;

		[SerializeField]
		private string macAppName;

		[SerializeField]
		private string linuxExectuableName;

		private AppId_t appID = AppId_t.Invalid;

		public uint GameID
		{
			get
			{
				return gameID;
			}
		}

		public string WindowsExecutableName
		{
			get
			{
				return windowsExecutableName;
			}
		}

		public string MacAppName
		{
			get
			{
				return macAppName;
			}
		}

		public string LinuxExecutableName
		{
			get
			{
				return linuxExectuableName;
			}
		}

		public AppId_t AppID
		{
			get
			{
				if (appID == AppId_t.Invalid)
				{
					appID = new AppId_t(gameID);
				}
				return appID;
			}
		}

		protected override string XmlSchemaName
		{
			get
			{
				return "SteamSKUInfo";
			}
		}
	}
}
