using System.Collections;
using System.Collections.Generic;
using Kittehface.Build;
using Kittehface.Framework20;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class AchievementsCtrl : MonoBehaviour
{
	public AchievementPopup achievementPrefab;

	public RectTransform achievementPopupGrid;

	[SerializeField]
	private List<AchievementData> achievementData = null;

	private static bool initialized = false;

	private static bool initializing = false;

	private bool achievementsLoaded = false;

	private int achievementGetCount = 0;

	private Dictionary<string, bool> unlockedAchievements = new Dictionary<string, bool>();

	private static AchievementsCtrl instance = null;

	public static AchievementsCtrl Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("AchievementsCtrl");
				instance = gameObject.AddComponent<AchievementsCtrl>();
			}
			return instance;
		}
	}

	public static bool Initialized
	{
		get
		{
			return initialized;
		}
	}

	public static void UnlockAchievement(string achievementID)
	{
		if (initialized)
		{
			if (!S.modsInstalled)
			{
				Achievements.UnlockAchievement(RunCtrl.runProfile, achievementID);
			}
		}
		else
		{
			Debug.LogError("Trying to unlock achievement [" + achievementID + "] when AchievementsCtrl not initialized!");
		}
	}

	public static bool IsUnlocked(string achievementID)
	{
		if (initialized)
		{
			if (Instance.unlockedAchievements.ContainsKey(achievementID))
			{
				return Instance.unlockedAchievements[achievementID];
			}
			return false;
		}
		Debug.LogError("Trying to check achievement [" + achievementID + "] when AchievementsCtrl not initialized!");
		return false;
	}

	public IEnumerator Initialize()
	{
		if (initialized || initializing)
		{
			yield break;
		}
		initializing = true;
		if (RunCtrl.runProfile != null)
		{
			SaveDataCtrl.SetAchievementsFile();
			achievementsLoaded = false;
			achievementGetCount = 0;
			Achievements.OnAchievementsLoaded += Achievements_OnAchievementsLoaded;
			Achievements.LoadAchievements(RunCtrl.runProfile, achievementData);
			while (!achievementsLoaded && achievementGetCount != achievementData.Count)
			{
				yield return null;
			}
		}
		initializing = false;
		initialized = true;
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Object.DontDestroyOnLoad(base.gameObject);
		Achievements.VerbosityLevel = VerbosityLevel.Info;
		Platform.OnRequestAchievementsMetadata += Platform_OnRequestAchievementsMetadata;
		Achievements.OnAchievementUnlocked += Achievements_OnAchievementUnlocked;
	}

	private void Platform_OnRequestAchievementsMetadata()
	{
		SKUInfo skuInfo = RunCtrl.skuInfo;
		if (skuInfo != null && skuInfo is SteamSKUInfo)
		{
			Achievements.SetAchievementsMetadata((ulong)((SteamSKUInfo)skuInfo).GameID);
		}
	}

	private void Achievements_OnAchievementsLoaded(Profiles.Profile profile, bool success)
	{
		if (profile != RunCtrl.runProfile)
		{
			return;
		}
		Achievements.OnAchievementsLoaded -= Achievements_OnAchievementsLoaded;
		achievementsLoaded = true;
		if (achievementData.Count <= 0)
		{
			return;
		}
		Achievements.OnAchievementRetrieved += Achievements_OnAchievementRetrieved;
		foreach (AchievementData achievementDatum in achievementData)
		{
			Achievements.GetAchievement(profile, achievementDatum.AchievementName);
		}
	}

	private void Achievements_OnAchievementRetrieved(Profiles.Profile profile, IAchievement achievement)
	{
		if (profile != RunCtrl.runProfile)
		{
			return;
		}
		achievementGetCount++;
		if (achievement != null)
		{
			if (unlockedAchievements.ContainsKey(achievement.id))
			{
				unlockedAchievements[achievement.id] = achievement.completed;
			}
			else
			{
				unlockedAchievements.Add(achievement.id, achievement.completed);
			}
		}
		if (achievementGetCount == achievementData.Count)
		{
			Achievements.OnAchievementRetrieved -= Achievements_OnAchievementRetrieved;
		}
	}

	private void Achievements_OnAchievementUnlocked(Profiles.Profile profile, IAchievement achievement)
	{
		if (achievement != null)
		{
			if (unlockedAchievements.ContainsKey(achievement.id))
			{
				unlockedAchievements[achievement.id] = achievement.completed;
			}
			else
			{
				unlockedAchievements.Add(achievement.id, achievement.completed);
			}
		}
		Debug.Log("UNLOCK ACHIEVEMENT " + achievement.id);
	}

	public List<AchievementData> GetAchievementData()
	{
		return new List<AchievementData>(achievementData);
	}
}
