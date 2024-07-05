using System;
using UnityEngine;

namespace AssetBundles
{
	public class LoadedAssetBundle
	{
		public AssetBundle m_AssetBundle;

		public int m_ReferencedCount;

		internal event Action unload;

		internal void OnUnload()
		{
			m_AssetBundle.Unload(false);
			if (this.unload != null)
			{
				this.unload();
			}
		}

		public LoadedAssetBundle(AssetBundle assetBundle)
		{
			m_AssetBundle = assetBundle;
			m_ReferencedCount = 1;
		}
	}
}
