using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssetBundles
{
	public class AssetBundleManager : MonoBehaviour
	{
		public enum LogMode
		{
			All = 0,
			JustErrors = 1
		}

		public enum LogType
		{
			Info = 0,
			Warning = 1,
			Error = 2
		}

		public delegate string OverrideBaseDownloadingURLDelegate(string bundleName);

		private static LogMode m_LogMode = LogMode.JustErrors;

		private static string m_BaseDownloadingURL = "";

		private static string[] m_ActiveVariants = new string[0];

		private static AssetBundleManifest m_AssetBundleManifest = null;

		private static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

		private static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();

		private static List<string> m_DownloadingBundles = new List<string>();

		private static List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation>();

		private static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();

		public static LogMode logMode
		{
			get
			{
				return m_LogMode;
			}
			set
			{
				m_LogMode = value;
			}
		}

		public static string BaseDownloadingURL
		{
			get
			{
				return m_BaseDownloadingURL;
			}
			set
			{
				m_BaseDownloadingURL = value;
			}
		}

		public static string[] ActiveVariants
		{
			get
			{
				return m_ActiveVariants;
			}
			set
			{
				m_ActiveVariants = value;
			}
		}

		public static AssetBundleManifest AssetBundleManifestObject
		{
			set
			{
				m_AssetBundleManifest = value;
			}
		}

		public static event OverrideBaseDownloadingURLDelegate overrideBaseDownloadingURL;

		public static void Log(LogType logType, string text)
		{
			if (logType == LogType.Error)
			{
				Debug.LogError("[AssetBundleManager] " + text);
			}
			else if (m_LogMode == LogMode.All && logType == LogType.Warning)
			{
				Debug.LogWarning("[AssetBundleManager] " + text);
			}
			else if (m_LogMode == LogMode.All)
			{
				Debug.Log("[AssetBundleManager] " + text);
			}
		}

		private static string GetStreamingAssetsPath()
		{
			if (Application.isEditor)
			{
				return "file://" + Environment.CurrentDirectory.Replace("\\", "/");
			}
			if (Application.isMobilePlatform || Application.isConsolePlatform)
			{
				return Application.streamingAssetsPath;
			}
			return "file://" + Application.streamingAssetsPath;
		}

		public static void SetSourceAssetBundleDirectory(string relativePath)
		{
			string text = GetStreamingAssetsPath();
			if (!text.EndsWith("/") && !text.EndsWith("\\"))
			{
				text += "/";
			}
			BaseDownloadingURL = text + relativePath;
		}

		public static void SetSourceAssetBundleURL(string absolutePath)
		{
			if (!absolutePath.EndsWith("/"))
			{
				absolutePath += "/";
			}
			BaseDownloadingURL = absolutePath + Utility.GetPlatformName() + "/";
		}

		public static void SetDevelopmentAssetBundleServer()
		{
			TextAsset textAsset = Resources.Load("AssetBundleServerURL") as TextAsset;
			string text = ((textAsset != null) ? textAsset.text.Trim() : null);
			if (text == null || text.Length == 0)
			{
				Log(LogType.Error, "Development Server URL could not be found.");
			}
			else
			{
				SetSourceAssetBundleURL(text);
			}
		}

		public static LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
		{
			if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
			{
				return null;
			}
			LoadedAssetBundle value = null;
			m_LoadedAssetBundles.TryGetValue(assetBundleName, out value);
			if (value == null)
			{
				return null;
			}
			string[] value2 = null;
			if (!m_Dependencies.TryGetValue(assetBundleName, out value2))
			{
				return value;
			}
			string[] array = value2;
			foreach (string key in array)
			{
				if (m_DownloadingErrors.TryGetValue(key, out error))
				{
					return null;
				}
				LoadedAssetBundle value3;
				m_LoadedAssetBundles.TryGetValue(key, out value3);
				if (value3 == null)
				{
					return null;
				}
			}
			return value;
		}

		public static bool IsAssetBundleDownloaded(string assetBundleName)
		{
			return m_LoadedAssetBundles.ContainsKey(assetBundleName);
		}

		public static AssetBundleLoadManifestOperation Initialize()
		{
			return Initialize(Utility.GetPlatformName());
		}

		public static AssetBundleLoadManifestOperation Initialize(string manifestAssetBundleName)
		{
			GameObject target = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
			UnityEngine.Object.DontDestroyOnLoad(target);
			LoadAssetBundle(manifestAssetBundleName, true);
			AssetBundleLoadManifestOperation assetBundleLoadManifestOperation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
			m_InProgressOperations.Add(assetBundleLoadManifestOperation);
			return assetBundleLoadManifestOperation;
		}

		public static void LoadAssetBundle(string assetBundleName)
		{
			LoadAssetBundle(assetBundleName, false);
		}

		protected static void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest)
		{
			Log(LogType.Info, "Loading Asset Bundle " + (isLoadingAssetBundleManifest ? "Manifest: " : ": ") + assetBundleName);
			if (!isLoadingAssetBundleManifest && m_AssetBundleManifest == null)
			{
				Log(LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
			}
			else if (!LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest) && !isLoadingAssetBundleManifest)
			{
				LoadDependencies(assetBundleName);
			}
		}

		protected static string GetAssetBundleBaseDownloadingURL(string bundleName)
		{
			if (AssetBundleManager.overrideBaseDownloadingURL != null)
			{
				Delegate[] invocationList = AssetBundleManager.overrideBaseDownloadingURL.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					OverrideBaseDownloadingURLDelegate overrideBaseDownloadingURLDelegate = (OverrideBaseDownloadingURLDelegate)invocationList[i];
					string text = overrideBaseDownloadingURLDelegate(bundleName);
					if (text != null)
					{
						return text;
					}
				}
			}
			return m_BaseDownloadingURL;
		}

		protected static bool UsesExternalBundleVariantResolutionMechanism(string baseAssetBundleName)
		{
			return false;
		}

		protected static string RemapVariantName(string assetBundleName)
		{
			string[] allAssetBundlesWithVariant = m_AssetBundleManifest.GetAllAssetBundlesWithVariant();
			string text = assetBundleName.Split('.')[0];
			if (UsesExternalBundleVariantResolutionMechanism(text))
			{
				return text;
			}
			int num = int.MaxValue;
			int num2 = -1;
			for (int i = 0; i < allAssetBundlesWithVariant.Length; i++)
			{
				string[] array = allAssetBundlesWithVariant[i].Split('.');
				string text2 = array[0];
				string value = array[1];
				if (!(text2 != text))
				{
					int num3 = Array.IndexOf(m_ActiveVariants, value);
					if (num3 == -1)
					{
						num3 = 2147483646;
					}
					if (num3 < num)
					{
						num = num3;
						num2 = i;
					}
				}
			}
			if (num == 2147483646)
			{
				Log(LogType.Warning, "Ambigious asset bundle variant chosen because there was no matching active variant: " + allAssetBundlesWithVariant[num2]);
			}
			if (num2 != -1)
			{
				return allAssetBundlesWithVariant[num2];
			}
			return assetBundleName;
		}

		protected static bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest)
		{
			LoadedAssetBundle value = null;
			m_LoadedAssetBundles.TryGetValue(assetBundleName, out value);
			if (value != null)
			{
				value.m_ReferencedCount++;
				return true;
			}
			if (m_DownloadingBundles.Contains(assetBundleName))
			{
				return true;
			}
			string text = GetAssetBundleBaseDownloadingURL(assetBundleName);
			if (text.ToLower().StartsWith("odr://"))
			{
				new ApplicationException("Can't load bundle " + assetBundleName + " through ODR: this Unity version or build target doesn't support it.");
			}
			else if (text.ToLower().StartsWith("res://"))
			{
				new ApplicationException("Can't load bundle " + assetBundleName + " through asset catalog: this Unity version or build target doesn't support it.");
			}
			else
			{
				if (!text.EndsWith("/"))
				{
					text += "/";
				}
				string text2 = text + assetBundleName;
				Log(LogType.Info, "Preparing to load AssetBundle [" + assetBundleName + "] at path [" + text2 + "]");
				if (isLoadingAssetBundleManifest)
				{
					Log(LogType.Info, "Performing manifest load through WWW");
					WWW www = new WWW(text2);
					m_InProgressOperations.Add(new AssetBundleDownloadFromWebOperation(assetBundleName, www));
				}
				else
				{
					if (text2.StartsWith("file://"))
					{
						text2 = text2.Substring(7);
					}
					Log(LogType.Info, "Loading [" + assetBundleName + "] with AssetBundle.LoadFromFileAsync using path [" + text2 + "]");
					m_InProgressOperations.Add(new AssetBundleCreateOperation(assetBundleName, AssetBundle.LoadFromFileAsync(text2)));
				}
			}
			m_DownloadingBundles.Add(assetBundleName);
			return false;
		}

		protected static void LoadDependencies(string assetBundleName)
		{
			Log(LogType.Info, "Loading dependencies for bundle [" + assetBundleName + "]");
			if (m_AssetBundleManifest == null)
			{
				Log(LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
				return;
			}
			string[] allDependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
			Log(LogType.Info, "Found [" + allDependencies.Length + "] dependencies for [" + assetBundleName + "]");
			if (allDependencies.Length != 0)
			{
				for (int i = 0; i < allDependencies.Length; i++)
				{
					allDependencies[i] = RemapVariantName(allDependencies[i]);
				}
				m_Dependencies.Add(assetBundleName, allDependencies);
				for (int j = 0; j < allDependencies.Length; j++)
				{
					LoadAssetBundleInternal(allDependencies[j], false);
				}
			}
		}

		public static void UnloadAssetBundle(string assetBundleName)
		{
			assetBundleName = RemapVariantName(assetBundleName);
			UnloadAssetBundleInternal(assetBundleName);
			UnloadDependencies(assetBundleName);
		}

		protected static void UnloadDependencies(string assetBundleName)
		{
			string[] value = null;
			if (m_Dependencies.TryGetValue(assetBundleName, out value))
			{
				string[] array = value;
				foreach (string assetBundleName2 in array)
				{
					UnloadAssetBundleInternal(assetBundleName2);
				}
				m_Dependencies.Remove(assetBundleName);
			}
		}

		protected static void UnloadAssetBundleInternal(string assetBundleName)
		{
			string error;
			LoadedAssetBundle loadedAssetBundle = GetLoadedAssetBundle(assetBundleName, out error);
			if (loadedAssetBundle != null && --loadedAssetBundle.m_ReferencedCount == 0)
			{
				loadedAssetBundle.OnUnload();
				m_LoadedAssetBundles.Remove(assetBundleName);
				Log(LogType.Info, assetBundleName + " has been unloaded successfully");
			}
		}

		private void Update()
		{
			int num = 0;
			while (num < m_InProgressOperations.Count)
			{
				AssetBundleLoadOperation assetBundleLoadOperation = m_InProgressOperations[num];
				if (logMode == LogMode.All && assetBundleLoadOperation is AssetBundleCreateOperation)
				{
					AssetBundleCreateOperation assetBundleCreateOperation = assetBundleLoadOperation as AssetBundleCreateOperation;
					float num2 = assetBundleCreateOperation.PercentageComplete();
					if (num2 != assetBundleCreateOperation.lastPercentage)
					{
						assetBundleCreateOperation.lastPercentage = num2;
						Log(LogType.Info, "Processing [" + assetBundleCreateOperation.assetBundleName + "], [" + num2 * 100f + "%] complete");
					}
				}
				if (assetBundleLoadOperation.Update())
				{
					num++;
					continue;
				}
				m_InProgressOperations.RemoveAt(num);
				ProcessFinishedOperation(assetBundleLoadOperation);
			}
		}

		private void ProcessFinishedOperation(AssetBundleLoadOperation operation)
		{
			AssetBundleDownloadOperation assetBundleDownloadOperation = operation as AssetBundleDownloadOperation;
			if (assetBundleDownloadOperation != null)
			{
				Log(LogType.Info, "Finished download operation for [" + assetBundleDownloadOperation.assetBundleName + "]");
				if (string.IsNullOrEmpty(assetBundleDownloadOperation.error))
				{
					m_LoadedAssetBundles.Add(assetBundleDownloadOperation.assetBundleName, assetBundleDownloadOperation.assetBundle);
				}
				else
				{
					string value = string.Format("Failed downloading bundle {0} from {1}: {2}", assetBundleDownloadOperation.assetBundleName, assetBundleDownloadOperation.GetSourceURL(), assetBundleDownloadOperation.error);
					m_DownloadingErrors.Add(assetBundleDownloadOperation.assetBundleName, value);
				}
				m_DownloadingBundles.Remove(assetBundleDownloadOperation.assetBundleName);
			}
		}

		public static AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, Type type)
		{
			Log(LogType.Info, "Loading " + assetName + " from " + assetBundleName + " bundle");
			AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = null;
			assetBundleName = RemapVariantName(assetBundleName);
			LoadAssetBundle(assetBundleName);
			assetBundleLoadAssetOperation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);
			m_InProgressOperations.Add(assetBundleLoadAssetOperation);
			return assetBundleLoadAssetOperation;
		}

		public static AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive, bool allowSceneActivation = true)
		{
			Log(LogType.Info, "Loading " + levelName + " from " + assetBundleName + " bundle");
			AssetBundleLoadOperation assetBundleLoadOperation = null;
			assetBundleName = RemapVariantName(assetBundleName);
			LoadAssetBundle(assetBundleName);
			assetBundleLoadOperation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive, allowSceneActivation);
			m_InProgressOperations.Add(assetBundleLoadOperation);
			return assetBundleLoadOperation;
		}

		public static AsyncOperation LoadSceneAsync(string assetBundleName, string sceneName, bool isAdditive, bool allowSceneActivation = true)
		{
			Log(LogType.Info, "Loading scene " + sceneName + " from " + assetBundleName + " bundle");
			AsyncOperation asyncOperation = null;
			assetBundleName = RemapVariantName(assetBundleName);
			string error;
			LoadedAssetBundle loadedAssetBundle = GetLoadedAssetBundle(assetBundleName, out error);
			if (loadedAssetBundle != null)
			{
				asyncOperation = SceneManager.LoadSceneAsync(sceneName, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
				asyncOperation.allowSceneActivation = allowSceneActivation;
			}
			else
			{
				Log(LogType.Error, "AssetBundle [" + assetBundleName + "] for scene [" + sceneName + "] not loaded!");
			}
			return asyncOperation;
		}

		public static T[] LoadAllAssets<T>(string assetBundleName, out string error) where T : UnityEngine.Object
		{
			error = null;
			LoadedAssetBundle loadedAssetBundle = GetLoadedAssetBundle(assetBundleName, out error);
			if (loadedAssetBundle == null)
			{
				return new T[0];
			}
			T[] array = loadedAssetBundle.m_AssetBundle.LoadAllAssets<T>();
			if (array.Length == 0)
			{
				error = "Could not find assets";
			}
			return array;
		}

		public static T LoadAsset<T>(string assetBundleName, string assetName, out string error) where T : UnityEngine.Object
		{
			error = null;
			LoadedAssetBundle loadedAssetBundle = GetLoadedAssetBundle(assetBundleName, out error);
			if (loadedAssetBundle == null)
			{
				return null;
			}
			if (typeof(T) == typeof(GameObject) || typeof(T) == typeof(Component) || typeof(T).IsSubclassOf(typeof(Component)))
			{
				GameObject gameObject = loadedAssetBundle.m_AssetBundle.LoadAsset<GameObject>(assetName);
				if (gameObject != null && (object)gameObject != null && typeof(T) != typeof(GameObject))
				{
					return gameObject.GetComponent<T>();
				}
				return gameObject as T;
			}
			UnityEngine.Object[] array = loadedAssetBundle.m_AssetBundle.LoadAssetWithSubAssets(assetName);
			UnityEngine.Object[] array2 = array;
			foreach (UnityEngine.Object @object in array2)
			{
				if (@object.GetType() == typeof(T) || @object.GetType().IsSubclassOf(typeof(T)))
				{
					return (T)@object;
				}
			}
			error = "Could not find matching type";
			return null;
		}
	}
}
