using System;
using AssetBundles;
using I2.Loc;
using TMPro;
using UnityEngine;

public class I2LocalizationAssetBundleLoader : IResourceManager_Bundles
{
	public UnityEngine.Object LoadFromBundle(string path, Type assetType)
	{
		string error;
		if (assetType == typeof(TMP_FontAsset))
		{
			return AssetBundleManager.LoadAsset<UnityEngine.Object>("fonts", path, out error);
		}
		return AssetBundleManager.LoadAsset<UnityEngine.Object>("localization", path, out error);
	}
}
