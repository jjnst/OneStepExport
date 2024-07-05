using UnityEngine;

namespace Kittehface.Build
{
	[CreateAssetMenu(fileName = "iOSSKUInfo", menuName = "KF Framework/SKU Info/iOS SKU Info", order = 1)]
	public class iOSSKUInfo : SKUInfo
	{
		private const string XML_CLOUDKIT_CONTAINER_ID = "cloudkitContainerID";

		[Header("iOS")]
		[SerializeField]
		private string cloudkitContainerID;

		[SerializeField]
		public string CloudKitContainerID
		{
			get
			{
				return cloudkitContainerID;
			}
		}

		protected override string XmlSchemaName
		{
			get
			{
				return "iOSSKUInfo";
			}
		}
	}
}
