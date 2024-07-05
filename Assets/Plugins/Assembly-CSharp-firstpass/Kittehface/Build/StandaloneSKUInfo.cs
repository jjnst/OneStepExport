using UnityEngine;

namespace Kittehface.Build
{
	[CreateAssetMenu(fileName = "StandaloneSKUInfo", menuName = "KF Framework/SKU Info/Standalone SKU Info", order = 1)]
	public class StandaloneSKUInfo : SKUInfo
	{
		private const string XML_WINDOWS_EXECUTABLE_NAME = "windowsExeName";

		private const string XML_MAC_APP_NAME = "macAppName";

		private const string XML_LINUX_EXECUTABLE_NAME = "linuxExeName";

		[Header("Standalone")]
		[SerializeField]
		private string windowsExecutableName;

		[SerializeField]
		private string macAppName;

		[SerializeField]
		private string linuxExectuableName;

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

		protected override string XmlSchemaName
		{
			get
			{
				return "StandaloneSKUInfo";
			}
		}
	}
}
