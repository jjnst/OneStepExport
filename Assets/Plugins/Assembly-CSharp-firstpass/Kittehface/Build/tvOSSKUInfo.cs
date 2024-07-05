using UnityEngine;

namespace Kittehface.Build
{
	[CreateAssetMenu(fileName = "tvOSSKUInfo", menuName = "KF Framework/SKU Info/tvOS SKU Info", order = 1)]
	public class tvOSSKUInfo : SKUInfo
	{
		private const string XML_CLOUDKIT_CONTAINER_ID = "cloudkitContainerID";

		[Header("tvOS")]
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
				return "tvOSSKUInfo";
			}
		}
	}
}
