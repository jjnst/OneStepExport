using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssetBundles
{
	public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
	{
		protected string m_AssetBundleName;

		protected string m_LevelName;

		protected bool m_IsAdditive;

		protected string m_DownloadingError;

		protected AsyncOperation m_Request;

		protected bool m_AllowSceneActivation;

		public AssetBundleLoadLevelOperation(string assetbundleName, string levelName, bool isAdditive, bool allowSceneActivation = true)
		{
			m_AssetBundleName = assetbundleName;
			m_LevelName = levelName;
			m_IsAdditive = isAdditive;
			m_AllowSceneActivation = allowSceneActivation;
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
				m_Request = SceneManager.LoadSceneAsync(m_LevelName, m_IsAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
				m_Request.allowSceneActivation = m_AllowSceneActivation;
				return false;
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

		public override float GetProgress()
		{
			if (m_Request != null)
			{
				return m_Request.progress;
			}
			return 0f;
		}

		public override void AllowSceneActivation()
		{
			m_AllowSceneActivation = true;
			if (m_Request != null)
			{
				m_Request.allowSceneActivation = true;
			}
		}
	}
}
