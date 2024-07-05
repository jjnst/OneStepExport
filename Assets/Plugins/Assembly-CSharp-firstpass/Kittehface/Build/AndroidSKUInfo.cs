using UnityEngine;

namespace Kittehface.Build
{
	[CreateAssetMenu(fileName = "AndroidSKUInfo", menuName = "KF Framework/SKU Info/Android SKU Info", order = 1)]
	public class AndroidSKUInfo : SKUInfo
	{
		private const string XML_APK_NAME = "apkName";

		[Header("Android")]
		[SerializeField]
		private string apkName;

		public string APKName
		{
			get
			{
				return apkName;
			}
		}

		protected override string XmlSchemaName
		{
			get
			{
				return "AndroidSKUInfo";
			}
		}
	}
}
