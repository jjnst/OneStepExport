using UnityEngine;

namespace Kittehface.Build
{
	[CreateAssetMenu(fileName = "PS4SKUInfo", menuName = "KF Framework/SKU Info/PS4 SKU Info", order = 1)]
	public class PS4SKUInfo : SKUInfo
	{
		private const string XML_CONTENT_ID = "contentId";

		private const string XML_PARAMS_PATH = "paramsPath";

		private const string XML_APP_VERSION = "appVersion";

		private const string XML_MASTER_VERSION = "masterVersion";

		private const string XML_NP_TITLE_DATA_PATH = "nptitleDataPath";

		private const string XML_NP_TITLE_SECRET = "nptitleSecret";

		private const string XML_LOCALIZED_ICONS_PATH = "localizedIconsPath";

		private const string XML_LOCALIZED_SPLASH_SCREENS_PATH = "localizedSplashScreensPath";

		private const string XML_BACKGROUND_IMAGE_PATH = "backgroundImagePath";

		private const string XML_DEFAULT_SPLASH_SCREEN_PATH = "defaultSplashScreenPath";

		private const string XML_PRONUNCIATION_XML_PATH = "pronunciationXmlPath";

		private const string XML_PRONUNCIATION_SIG_PATH = "pronunciationSigPath";

		private const string XML_TROPHY_PATH = "trophyPath";

		private const string XML_CHANGE_INFO_PATH = "changeInfoPath";

		private const string XML_DEFAULT_NP_AGE_RESTRICTION = "defaultNPAgeRestriction";

		[Header("PS4")]
		[SerializeField]
		private string contentId;

		[SerializeField]
		private string paramsPath;

		[SerializeField]
		private string appVersion;

		[SerializeField]
		private string masterVersion;

		[SerializeField]
		private string nptitleDataPath;

		[SerializeField]
		private string nptitleSecret;

		[SerializeField]
		private string localizedIconsPath;

		[SerializeField]
		private string localizedSplashScreensPath;

		[SerializeField]
		private string backgroundImagePath;

		[SerializeField]
		private string defaultSplashScreenPath;

		[SerializeField]
		private string pronunciationXmlPath;

		[SerializeField]
		private string pronunciationSigPath;

		[SerializeField]
		private string trophyPath;

		[SerializeField]
		private string changeInfoPath;

		[SerializeField]
		private int defaultNPAgeRestriction = 0;

		public string ContentId
		{
			get
			{
				return contentId;
			}
		}

		public string ParamsPath
		{
			get
			{
				return paramsPath;
			}
		}

		public string AppVersion
		{
			get
			{
				return appVersion;
			}
		}

		public string MasterVersion
		{
			get
			{
				return masterVersion;
			}
		}

		public string NptitleDataPath
		{
			get
			{
				return nptitleDataPath;
			}
		}

		public string NptitleSecret
		{
			get
			{
				return nptitleSecret;
			}
		}

		public string LocalizedIconsPath
		{
			get
			{
				return localizedIconsPath;
			}
		}

		public string LocalizedSplashScreensPath
		{
			get
			{
				return localizedSplashScreensPath;
			}
		}

		public string BackgroundImagePath
		{
			get
			{
				return backgroundImagePath;
			}
		}

		public string DefaultSplashScreenPath
		{
			get
			{
				return defaultSplashScreenPath;
			}
		}

		public string PronunciationXmlPath
		{
			get
			{
				return pronunciationXmlPath;
			}
		}

		public string PronunciationSigPath
		{
			get
			{
				return pronunciationSigPath;
			}
		}

		public string TrophyPath
		{
			get
			{
				return trophyPath;
			}
		}

		public string ChangeInfoPath
		{
			get
			{
				return changeInfoPath;
			}
		}

		public int DefaultNPAgeRestriction
		{
			get
			{
				return defaultNPAgeRestriction;
			}
		}

		protected override string XmlSchemaName
		{
			get
			{
				return "PS4SKUInfo";
			}
		}
	}
}
