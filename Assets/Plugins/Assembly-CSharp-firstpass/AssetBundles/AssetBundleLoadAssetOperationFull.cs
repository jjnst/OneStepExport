using System;
using UnityEngine;

namespace AssetBundles
{
	public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
	{
		protected string m_AssetBundleName;

		protected string m_AssetName;

		protected string m_DownloadingError;

		protected Type m_Type;

		protected AssetBundleRequest m_Request = null;

		public AssetBundleLoadAssetOperationFull(string bundleName, string assetName, Type type)
		{
			m_AssetBundleName = bundleName;
			m_AssetName = assetName;
			m_Type = type;
		}

		public override T GetAsset<T>()
		{
			if (m_Request != null && m_Request.isDone)
			{
				if (m_Type == typeof(GameObject) || m_Type == typeof(Component) || m_Type.IsSubclassOf(typeof(Component)))
				{
					if (m_Request.asset != null && m_Request.asset is GameObject && typeof(T) != typeof(GameObject))
					{
						return ((GameObject)m_Request.asset).GetComponent<T>();
					}
					return m_Request.asset as T;
				}
				UnityEngine.Object[] allAssets = m_Request.allAssets;
				foreach (UnityEngine.Object @object in allAssets)
				{
					if (@object.GetType() == typeof(T) || @object.GetType().IsSubclassOf(typeof(T)))
					{
						return (T)@object;
					}
				}
			}
			return null;
		}

		public override bool Update()
		{
			if (m_Request != null)
			{
				return false;
			}
			LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
			if (loadedAssetBundle != null)
			{
				if (m_Type == typeof(GameObject) || m_Type == typeof(Component) || m_Type.IsSubclassOf(typeof(Component)))
				{
					m_Request = loadedAssetBundle.m_AssetBundle.LoadAssetAsync(m_AssetName, typeof(GameObject));
				}
				else
				{
					m_Request = loadedAssetBundle.m_AssetBundle.LoadAssetWithSubAssetsAsync(m_AssetName, m_Type);
				}
				m_Request.completed += delegate(AsyncOperation asyncOp)
				{
					CompleteOperation(asyncOp);
				};
				return false;
			}
			if (m_DownloadingError != null)
			{
				CompleteOperation(null);
			}
			return true;
		}

		public override bool IsDone()
		{
			if (m_Request == null && m_DownloadingError != null)
			{
				AssetBundleManager.Log(AssetBundleManager.LogType.Error, m_DownloadingError);
				return true;
			}
			return m_Request != null && m_Request.isDone;
		}
	}
}
