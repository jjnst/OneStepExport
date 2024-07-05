using System.Collections;
using AssetBundles;
using I2.Loc;
using Kittehface.Build;
using Kittehface.Framework20;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
	private const string STARTUPS_PREF_KEY = "Startups";

	public CanvasGroup loadingBar;

	public Image loadSymbol;

	public AudioMixer masterAudioMixerReference;

	public AudioMixerGroup masterSfxGroup;

	public AudioMixerGroup masterMusicGroup;

	public Platform kfFrameworkPlatformPrefab;

	public Animator anim;

	private void Start()
	{
		Application.targetFrameRate = 60;
		SettingsPane.masterMixer = masterAudioMixerReference;
		S.sfxGroup = masterSfxGroup;
		S.musicGroup = masterMusicGroup;
		bool flag = false;
		foreach (IResourceManager_Bundles mBundleManager in ResourceManager.pInstance.mBundleManagers)
		{
			if (mBundleManager is I2LocalizationAssetBundleLoader)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ResourceManager.pInstance.mBundleManagers.Add(new I2LocalizationAssetBundleLoader());
		}
		StartCoroutine(LoadAsynchronously());
	}

	private IEnumerator LoadAsynchronously()
	{
		yield return null;
		AssetBundleManager.SetSourceAssetBundleDirectory("AssetBundles");
		yield return AssetBundleManager.Initialize();
		yield return LoadAssetBundle("localization");
		RunCtrl.assetBundleManagerInitialized = true;
		AssetBundleLoadAssetOperation op = AssetBundleManager.LoadAssetAsync("rewired", "InputManager", typeof(GameObject));
		yield return op;
		Object.Instantiate(op.GetAsset<GameObject>());
		Platform.OnRequestPlatformMetadata += Platform_OnRequestPlatformMetadata;
		Profiles.OnActivated += Profiles_OnActivated;
		AssetBundleLoadAssetOperation op2 = AssetBundleManager.LoadAssetAsync("achievements", "AchievementsCtrl", typeof(GameObject));
		yield return op2;
		AchievementsCtrl achievementsCtrl = Object.Instantiate(op2.GetAsset<GameObject>()).GetComponent<AchievementsCtrl>();
		Object.Instantiate(kfFrameworkPlatformPrefab);
		while (!Platform.initialized)
		{
			yield return null;
		}
		if (RunCtrl.secondaryProfile == null)
		{
			Profiles.RequestDummyProfile("");
			while (RunCtrl.secondaryProfile == null)
			{
				yield return null;
			}
		}
		Profiles.OnActivated -= Profiles_OnActivated;
		yield return SaveDataCtrl.Initialize();
		yield return achievementsCtrl.Initialize();
		yield return LoadAssetBundle("animations");
		yield return LoadAssetBundle("sounds");
		yield return LoadAssetBundle("sprites");
		AssetBundleLoadOperation assetBundleLoadOperation = AssetBundleManager.LoadLevelAsync("main", "Main", false, false);
		float minimumPreWaitTime = 4f;
		while (minimumPreWaitTime > Time.timeSinceLevelLoad)
		{
			if (Input.anyKeyDown)
			{
				anim.SetTrigger("skip");
			}
			loadingBar.alpha = Mathf.Clamp01(Time.timeSinceLevelLoad / minimumPreWaitTime);
			yield return null;
		}
		float minimumWaitTime = 24f;
		while (!assetBundleLoadOperation.IsDone())
		{
			Mathf.Clamp01(assetBundleLoadOperation.GetProgress() / 0.9f);
			if (Input.anyKeyDown)
			{
				anim.SetTrigger("skip");
			}
			if ((assetBundleLoadOperation.GetProgress() >= 0.9f || minimumWaitTime < Time.timeSinceLevelLoad) && (minimumWaitTime < Time.timeSinceLevelLoad || anim.GetCurrentAnimatorStateInfo(0).IsName("LoadingScreen3")))
			{
				assetBundleLoadOperation.AllowSceneActivation();
				break;
			}
			yield return null;
		}
	}

	private void Platform_OnRequestPlatformMetadata()
	{
		SKUInfo skuInfo = RunCtrl.skuInfo;
		if (skuInfo != null && skuInfo is SteamSKUInfo)
		{
			Platform.SetPlatformMetadata(((SteamSKUInfo)skuInfo).GameID);
		}
	}

	private void Profiles_OnActivated(Profiles.Profile profile)
	{
		if (RunCtrl.runProfile == null)
		{
			Debug.Log("Set runProfile");
			RunCtrl.runProfile = profile;
		}
		else if (RunCtrl.secondaryProfile == null)
		{
			Debug.Log("Set secondaryProfile");
			RunCtrl.secondaryProfile = profile;
		}
	}

	private IEnumerator LoadAssetBundle(string assetBundleName)
	{
		string error;
		if (AssetBundleManager.GetLoadedAssetBundle(assetBundleName, out error) == null)
		{
			AssetBundleManager.LoadAssetBundle(assetBundleName);
			while (AssetBundleManager.GetLoadedAssetBundle(assetBundleName, out error) == null)
			{
				yield return null;
			}
		}
	}
}
