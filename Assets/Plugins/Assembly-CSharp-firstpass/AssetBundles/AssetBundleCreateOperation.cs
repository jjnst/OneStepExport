using System;
using UnityEngine;

namespace AssetBundles
{
	public class AssetBundleCreateOperation : AssetBundleDownloadOperation
	{
		private AssetBundleCreateRequest request = null;

		public float lastPercentage = -1f;

		protected override bool downloadIsDone
		{
			get
			{
				return request == null || request.isDone;
			}
		}

		public AssetBundleCreateOperation(string assetBundleName, AssetBundleCreateRequest request)
			: base(assetBundleName)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}
			this.request = request;
		}

		protected override void FinishDownload()
		{
			AssetBundle assetBundle = request.assetBundle;
			if (assetBundle == null)
			{
				base.error = string.Format("{0} is not a valid asset bundle.", base.assetBundleName);
			}
			else
			{
				base.assetBundle = new LoadedAssetBundle(assetBundle);
			}
		}

		public float PercentageComplete()
		{
			if (request != null)
			{
				return request.progress;
			}
			return 0f;
		}

		public override string GetSourceURL()
		{
			return base.assetBundleName;
		}
	}
}
