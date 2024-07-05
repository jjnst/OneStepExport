using System;
using UnityEngine;

namespace AssetBundles
{
	public class AssetBundleDownloadFromWebOperation : AssetBundleDownloadOperation
	{
		private WWW m_WWW;

		private string m_Url;

		protected override bool downloadIsDone
		{
			get
			{
				return m_WWW == null || m_WWW.isDone;
			}
		}

		public AssetBundleDownloadFromWebOperation(string assetBundleName, WWW www)
			: base(assetBundleName)
		{
			if (www == null)
			{
				throw new ArgumentNullException("www");
			}
			m_Url = www.url;
			m_WWW = www;
		}

		protected override void FinishDownload()
		{
			base.error = m_WWW.error;
			if (string.IsNullOrEmpty(base.error))
			{
				AssetBundle assetBundle = m_WWW.assetBundle;
				if (assetBundle == null)
				{
					base.error = string.Format("{0} is not a valid asset bundle.", base.assetBundleName);
				}
				else
				{
					base.assetBundle = new LoadedAssetBundle(m_WWW.assetBundle);
				}
				m_WWW.Dispose();
				m_WWW = null;
			}
		}

		public override string GetSourceURL()
		{
			return m_Url;
		}
	}
}
