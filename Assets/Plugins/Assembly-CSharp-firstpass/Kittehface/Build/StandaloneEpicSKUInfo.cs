using UnityEngine;

namespace Kittehface.Build
{
	[CreateAssetMenu(fileName = "StandaloneEpicSKUInfo", menuName = "KF Framework/SKU Info/Standalone Epic SKU Info", order = 1)]
	public class StandaloneEpicSKUInfo : SKUInfo
	{
		private const string XML_WINDOWS_EXECUTABLE_NAME = "windowsExeName";

		private const string XML_MAC_APP_NAME = "macAppName";

		private const string XML_LINUX_EXECUTABLE_NAME = "linuxExeName";

		private const string XML_SAVE_SUBDIRECTORY = "saveSubdirectory";

		[Header("Standalone Epic")]
		[SerializeField]
		private string windowsExecutableName;

		[SerializeField]
		private string macAppName;

		[SerializeField]
		private string linuxExectuableName;

		[SerializeField]
		private string saveSubdirectory;

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

		public string SaveSubdirectory
		{
			get
			{
				return saveSubdirectory;
			}
		}

		protected override string XmlSchemaName
		{
			get
			{
				return "StandaloneEpicSKUInfo";
			}
		}
	}
}
