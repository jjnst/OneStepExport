using UnityEngine;

namespace Kittehface.Build
{
	[CreateAssetMenu(fileName = "XboxOneSKUInfo", menuName = "KF Framework/SKU Info/Xbox One SKU Info", order = 1)]
	public class XboxOneSKUInfo : SKUInfo
	{
		private const string XML_MANIFEST_PATH = "manifestPath";

		private const string XML_TITLE_ID = "titleID";

		private const string XML_SERVICE_CONFIG_ID = "serviceConfigID";

		[Header("Xbox One")]
		[SerializeField]
		private string manifestPath;

		[SerializeField]
		private uint titleID;

		[SerializeField]
		private string serviceConfigID;

		public string ManifestPath
		{
			get
			{
				return manifestPath;
			}
		}

		public uint TitleID
		{
			get
			{
				return titleID;
			}
		}

		public string ServiceConfigID
		{
			get
			{
				return serviceConfigID;
			}
		}

		protected override string XmlSchemaName
		{
			get
			{
				return "XboxOneSKUInfo";
			}
		}
	}
}
