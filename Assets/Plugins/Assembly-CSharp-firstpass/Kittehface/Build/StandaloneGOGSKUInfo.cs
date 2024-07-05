using UnityEngine;

namespace Kittehface.Build
{
	[CreateAssetMenu(fileName = "StandaloneGOGSKUInfo", menuName = "KF Framework/SKU Info/Standalone GOG SKU Info", order = 1)]
	public class StandaloneGOGSKUInfo : SKUInfo
	{
		private const string XML_WINDOWS_EXECUTABLE_NAME = "windowsExeName";

		private const string XML_MAC_APP_NAME = "macAppName";

		private const string XML_LINUX_EXECUTABLE_NAME = "linuxExeName";

		private const string XML_CLIENT_ID = "clientID";

		private const string XML_CLIENT_SECRET = "clientSecret";

		[Header("Standalone GOG")]
		[SerializeField]
		private string windowsExecutableName;

		[SerializeField]
		private string macAppName;

		[SerializeField]
		private string linuxExectuableName;

		[SerializeField]
		private string clientID;

		[SerializeField]
		private string clientSecret;

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

		public string ClientID
		{
			get
			{
				return clientID;
			}
		}

		public string ClientSecret
		{
			get
			{
				return clientSecret;
			}
		}

		protected override string XmlSchemaName
		{
			get
			{
				return "StandaloneGOGSKUInfo";
			}
		}

		public string[] GetClientIDs()
		{
			return new string[2] { clientID, clientSecret };
		}
	}
}
