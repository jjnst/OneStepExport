using System;
using UnityEngine;

namespace AssetBundles
{
	public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
	{
		private bool m_SentComplete = false;

		public event Action<AsyncOperation> completed;

		public abstract T GetAsset<T>() where T : UnityEngine.Object;

		protected void CompleteOperation(AsyncOperation asyncOp)
		{
			if (!m_SentComplete && this.completed != null)
			{
				this.completed(asyncOp);
			}
		}
	}
}
