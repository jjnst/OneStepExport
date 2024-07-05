using UnityEngine;

namespace Kittehface.Build
{
	[CreateAssetMenu(fileName = "SwitchSKUInfo", menuName = "KF Framework/SKU Info/Switch SKU Info", order = 1)]
	public class SwitchSKUInfo : SKUInfo
	{
		private const string XML_PACKAGE_NAME = "packageName";

		[Header("Switch")]
		[SerializeField]
		private string packageName;

		public string PackageName
		{
			get
			{
				return packageName;
			}
		}

		protected override string XmlSchemaName
		{
			get
			{
				return "SwitchSKUInfo";
			}
		}
	}
}
