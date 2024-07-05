using System.Collections.Generic;

namespace Kittehface.Framework20
{
	public static class AppleDeviceModel
	{
		private const string IPadPrefix = "iPad";

		private const string IPhonePrefix = "iPhone";

		private const string IPodPrefix = "iPod";

		private const string AppleTvPrefix = "AppleTV";

		private const string IMacPrefix = "iMac";

		private const string IMacProPrefix = "iMacPro";

		private const string MacBookPrefix = "MacBook";

		private const string MacBookAirPrefix = "MacBookAir";

		private const string MacBookProPrefix = "MacBookPro";

		private const string MacMiniPrefix = "Macmini";

		private const string MacProPrefix = "MacPro";

		private static Dictionary<string, string> IOSModels = new Dictionary<string, string>
		{
			{ "iPad1,1", "iPad" },
			{ "iPad2,1", "iPad 2" },
			{ "iPad2,2", "iPad 2" },
			{ "iPad2,3", "iPad 2" },
			{ "iPad2,4", "iPad 2" },
			{ "iPad2,5", "iPad mini" },
			{ "iPad2,6", "iPad mini" },
			{ "iPad2,7", "iPad mini" },
			{ "iPad3,1", "iPad (3rd generation)" },
			{ "iPad3,2", "iPad (3rd generation)" },
			{ "iPad3,3", "iPad (3rd generation)" },
			{ "iPad3,4", "iPad (4th generation)" },
			{ "iPad3,5", "iPad (4th generation)" },
			{ "iPad3,6", "iPad (4th generation)" },
			{ "iPad4,1", "iPad Air" },
			{ "iPad4,2", "iPad Air" },
			{ "iPad4,3", "iPad Air" },
			{ "iPad4,4", "iPad mini 2" },
			{ "iPad4,5", "iPad mini 2" },
			{ "iPad4,6", "iPad mini 2" },
			{ "iPad4,7", "iPad mini 3" },
			{ "iPad4,8", "iPad mini 3" },
			{ "iPad4,9", "iPad mini 3" },
			{ "iPad5,1", "iPad mini 4" },
			{ "iPad5,2", "iPad mini 4" },
			{ "iPad5,3", "iPad Air 2" },
			{ "iPad5,4", "iPad Air 2" },
			{ "iPad6,3", "iPad Pro 9.7\"" },
			{ "iPad6,4", "iPad Pro 9.7\"" },
			{ "iPad6,7", "iPad Pro 12.9\"" },
			{ "iPad6,8", "iPad Pro 12.9\"" },
			{ "iPad6,11", "iPad (5th generation)" },
			{ "iPad6,12", "iPad (5th generation)" },
			{ "iPad7,1", "iPad Pro 12.9\" (2nd generation)" },
			{ "iPad7,2", "iPad Pro 12.9\" (2nd generation)" },
			{ "iPad7,3", "iPad Pro 10.5\"" },
			{ "iPad7,4", "iPad Pro 10.5\"" },
			{ "iPad7,5", "iPad (6th generation)" },
			{ "iPad7,6", "iPad (6th generation)" },
			{ "iPad8,1", "iPad Pro 11\"" },
			{ "iPad8,2", "iPad Pro 11\"" },
			{ "iPad8,3", "iPad Pro 11\"" },
			{ "iPad8,4", "iPad Pro 11\"" },
			{ "iPad8,5", "iPad Pro 12.9\" (3rd generation)" },
			{ "iPad8,6", "iPad Pro 12.9\" (3rd generation)" },
			{ "iPad8,7", "iPad Pro 12.9\" (3rd generation)" },
			{ "iPad8,8", "iPad Pro 12.9\" (3rd generation)" },
			{ "iPad11,1", "iPad mini (5th generation)" },
			{ "iPad11,2", "iPad mini (5th generation)" },
			{ "iPad11,3", "iPad Air (3rd generation)" },
			{ "iPad11,4", "iPad Air (3rd generation)" },
			{ "iPhone1,1", "iPhone" },
			{ "iPhone1,2", "iPhone 3G" },
			{ "iPhone2,1", "iPhone 3GS" },
			{ "iPhone3,1", "iPhone 4" },
			{ "iPhone3,2", "iPhone 4" },
			{ "iPhone3,3", "iPhone 4" },
			{ "iPhone4,1", "iPhone 4S" },
			{ "iPhone5,1", "iPhone 5" },
			{ "iPhone5,2", "iPhone 5" },
			{ "iPhone5,3", "iPhone 5c" },
			{ "iPhone5,4", "iPhone 5c" },
			{ "iPhone6,1", "iPhone 5s" },
			{ "iPhone6,2", "iPhone 5s" },
			{ "iPhone7,1", "iPhone 6 Plus" },
			{ "iPhone7,2", "iPhone 6" },
			{ "iPhone8,1", "iPhone 6s" },
			{ "iPhone8,2", "iPhone 6s Plus" },
			{ "iPhone8,4", "iPhone SE" },
			{ "iPhone9,1", "iPhone 7" },
			{ "iPhone9,2", "iPhone 7 Plus" },
			{ "iPhone9,3", "iPhone 7" },
			{ "iPhone9,4", "iPhone 7 Plus" },
			{ "iPhone10,1", "iPhone 8" },
			{ "iPhone10,2", "iPhone 8 Plus" },
			{ "iPhone10,3", "iPhone X" },
			{ "iPhone10,4", "iPhone 8" },
			{ "iPhone10,5", "iPhone 8 Plus" },
			{ "iPhone10,6", "iPhone X" },
			{ "iPhone11,2", "iPhone XS" },
			{ "iPhone11,6", "iPhone XS Max" },
			{ "iPhone11,8", "iPhone XR" },
			{ "iPhone12,1", "iPhone 11" },
			{ "iPhone12,3", "iPhone 11 Pro" },
			{ "iPhone12,5", "iPhone 11 Pro Max" },
			{ "iPod1,1", "iPod touch" },
			{ "iPod2,1", "iPod touch (2nd generation)" },
			{ "iPod3,1", "iPod touch (3rd generation)" },
			{ "iPod4,1", "iPod touch (4th generation)" },
			{ "iPod5,1", "iPod touch (5th generation)" },
			{ "iPod7,1", "iPod touch (6th generation)" },
			{ "iPod9,1", "iPod touch (7th generation)" }
		};

		private static Dictionary<string, string> AppleTvModels = new Dictionary<string, string>
		{
			{ "AppleTV1,1", "Apple TV" },
			{ "AppleTV2,1", "Apple TV (2nd generation)" },
			{ "AppleTV3,1", "Apple TV (3rd generation)" },
			{ "AppleTV3,2", "Apple TV (3rd generation)" },
			{ "AppleTV5,3", "Apple TV HD" },
			{ "AppleTV6,2", "Apple TV 4K" }
		};

		private static Dictionary<string, string> MacModels = new Dictionary<string, string>
		{
			{ "iMac,1", "iMac G3" },
			{ "iMac10,1", "iMac (Late 2009)" },
			{ "iMac11,1", "iMac (Late 2009)" },
			{ "iMac11,2", "iMac (Mid-2010)" },
			{ "iMac11,3", "iMac (Mid-2010)" },
			{ "iMac12,1", "iMac (2011)" },
			{ "iMac12,2", "iMac (2011)" },
			{ "iMac13,1", "iMac" },
			{ "iMac13,2", "iMac (Late 2012)" },
			{ "iMac14,1", "iMac (Late 2013)" },
			{ "iMac14,2", "iMac (Late 2013)" },
			{ "iMac14,3", "iMac (Late 2013)" },
			{ "iMac14,4", "iMac (Mid-2014)" },
			{ "iMac15,1", "iMac" },
			{ "iMac16,1", "iMac (Late 2015)" },
			{ "iMac16,2", "iMac (Late 2015)" },
			{ "iMac17,1", "iMac (Late 2015)" },
			{ "iMac18,1", "iMac (Mid-2017)" },
			{ "iMac18,2", "iMac (Mid-2017)" },
			{ "iMac18,3", "iMac (Mid-2017)" },
			{ "iMac19,1", "iMac (2019)" },
			{ "iMac19,2", "iMac (2019)" },
			{ "iMacPro1,1", "iMac Pro (Late 2017)" },
			{ "MacBook7,1", "MacBook (Mid-2010)" },
			{ "MacBook8,1", "MacBook (Early 2015)" },
			{ "MacBook9,1", "MacBook (Early 2016)" },
			{ "MacBook10,1", "MacBook (Mid-2017)" },
			{ "MacBookAir1,1", "MacBook Air" },
			{ "MacBookAir2,1", "MacBook Air" },
			{ "MacBookAir3,1", "MacBook Air (Late 2010)" },
			{ "MacBookAir3,2", "MacBook Air (Late 2010)" },
			{ "MacBookAir4,1", "MacBook Air (Mid-2011)" },
			{ "MacBookAir4,2", "MacBook Air (Mid-2011)" },
			{ "MacBookAir5,1", "MacBook Air (Mid-2012)" },
			{ "MacBookAir5,2", "MacBook Air (Mid-2012)" },
			{ "MacBookAir6,1", "MacBook Air" },
			{ "MacBookAir6,2", "MacBook Air" },
			{ "MacBookAir7,1", "MacBook Air (Early 2015)" },
			{ "MacBookAir7,2", "MacBook Air" },
			{ "MacBookAir8,1", "MacBook Air (Late 2018)" },
			{ "MacBookPro6,1", "MacBook Pro (Mid-2010)" },
			{ "MacBookPro6,2", "MacBook Pro (Mid-2010)" },
			{ "MacBookPro7,1", "MacBook Pro (Mid-2010)" },
			{ "MacBookPro8,1", "MacBook Pro (2011)" },
			{ "MacBookPro8,2", "MacBook Pro (2011)" },
			{ "MacBookPro8,3", "MacBook Pro (2011)" },
			{ "MacBookPro9,1", "MacBook Pro (Mid-2012)" },
			{ "MacBookPro9,2", "MacBook Pro (Mid-2012)" },
			{ "MacBookPro10,1", "MacBook Pro" },
			{ "MacBookPro10,2", "MacBook Pro" },
			{ "MacBookPro11,1", "MacBook Pro" },
			{ "MacBookPro11,2", "MacBook Pro" },
			{ "MacBookPro11,3", "MacBook Pro" },
			{ "MacBookPro11,4", "MacBook Pro (Mid-2015)" },
			{ "MacBookPro11,5", "MacBook Pro (Mid-2015)" },
			{ "MacBookPro12,1", "MacBook Pro (Early 2015)" },
			{ "MacBookPro13,1", "MacBook Pro (Late 2016)" },
			{ "MacBookPro13,2", "MacBook Pro (Late 2016)" },
			{ "MacBookPro13,3", "MacBook Pro (Late 2016)" },
			{ "MacBookPro14,1", "MacBook Pro (Mid-2017)" },
			{ "MacBookPro14,2", "MacBook Pro (Mid-2017)" },
			{ "MacBookPro14,3", "MacBook Pro (Mid-2017)" },
			{ "MacBookPro15,1", "MacBook Pro" },
			{ "MacBookPro15,2", "MacBook Pro" },
			{ "MacBookPro15,3", "MacBook Pro" },
			{ "Macmini1,1", "Mac mini" },
			{ "Macmini2,1", "Mac mini" },
			{ "Macmini3,1", "Mac mini (2009)" },
			{ "Macmini4,1", "Mac mini (Mid-2010)" },
			{ "Macmini5,1", "Mac mini (Mid-2011)" },
			{ "Macmini5,2", "Mac mini (Mid-2011)" },
			{ "Macmini5,3", "Mac mini (Mid-2011)" },
			{ "Macmini6,1", "Mac mini (Late 2012)" },
			{ "Macmini6,2", "Mac mini (Late 2012)" },
			{ "Macmini7,1", "Mac mini (Late 2014)" },
			{ "Macmini8,1", "Mac mini (Late 2018)" },
			{ "MacPro1,1", "Mac Pro" },
			{ "MacPro2,1", "Mac Pro" },
			{ "MacPro3,1", "Mac Pro (2008)" },
			{ "MacPro4,1", "Mac Pro (2009)" },
			{ "MacPro5,1", "Mac Pro" },
			{ "MacPro6,1", "Mac Pro (Late 2013)" }
		};

		public static string GetModelNameFromModelId(string modelId, string fallbackName = null)
		{
			if (modelId == null)
			{
				return fallbackName;
			}
			if (IOSModels.ContainsKey(modelId))
			{
				return IOSModels[modelId];
			}
			if (AppleTvModels.ContainsKey(modelId))
			{
				return AppleTvModels[modelId];
			}
			if (MacModels.ContainsKey(modelId))
			{
				return MacModels[modelId];
			}
			if (modelId.StartsWith("iPad"))
			{
				return "iPad";
			}
			if (modelId.StartsWith("iPhone"))
			{
				return "iPhone";
			}
			if (modelId.StartsWith("iPod"))
			{
				return "iPod";
			}
			if (modelId.StartsWith("AppleTV"))
			{
				return "Apple TV";
			}
			if (modelId.StartsWith("iMac"))
			{
				if (modelId.StartsWith("iMacPro"))
				{
					return "iMac Pro";
				}
				return "iMac";
			}
			if (modelId.StartsWith("MacBook"))
			{
				if (modelId.StartsWith("MacBookAir"))
				{
					return "MacBook Air";
				}
				if (modelId.StartsWith("MacBookPro"))
				{
					return "MacBook Pro";
				}
				return "MacBook";
			}
			if (modelId.StartsWith("Macmini"))
			{
				return "Mac mini";
			}
			if (modelId.StartsWith("MacPro"))
			{
				return "Mac Pro";
			}
			if (!string.IsNullOrEmpty(fallbackName))
			{
				return fallbackName;
			}
			return modelId;
		}

		public static string GetGenericModelNameForPlatform()
		{
			return null;
		}
	}
}
