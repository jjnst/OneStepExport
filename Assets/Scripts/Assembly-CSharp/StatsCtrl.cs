using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsCtrl : NavPanel
{
	public NavButton showAchievementsButton;

	public TMP_Text statsText;

	public TMP_Text achievementsText;

	public TMP_Text runHistoryText;

	public Transform charGrid;

	public CanvasGroup canvasGroup;

	public HeroSplash heroSplash;

	public CanvasGroup statsDisplay;

	public Transform achievementsDisplay;

	public Transform achievementsGrid;

	public Scrollbar achievementsScrollbar;

	public AchievementButton achievementButtonPrefab;

	private List<AchievementButton> achievementButtons = new List<AchievementButton>();

	public HeroSelectCtrl heCtrl;

	public Animator containerAnim;

	private void Start()
	{
		statsDisplay.gameObject.SetActive(true);
		if (achievementsGrid.childCount < 10)
		{
			achievementsGrid.DestroyChildren();
		}
		heroSplash.Hide();
	}

	public override void Open()
	{
		base.Open();
		canvasGroup.blocksRaycasts = true;
		achievementsText.text = ScriptLocalization.UI.Stats_Achievements;
		StartCoroutine(OpenMenuC(charGrid));
		ShowAchievements();
	}

	private IEnumerator OpenMenuC(Transform parentGrid)
	{
		yield return new WaitForEndOfFrame();
		foreach (Transform child in parentGrid)
		{
			yield return new WaitForSecondsRealtime(0.05f);
			if ((bool)child)
			{
				child.GetComponent<Animator>().SetBool("visible", true);
			}
		}
	}

	private void Update()
	{
		if (slideBody.onScreen && btnCtrl.focusedButton == showAchievementsButton && !achievementsDisplay.gameObject.activeSelf)
		{
			ShowAchievements();
		}
	}

	public void ShowAchievements(bool focusFirstAchievement = false)
	{
		achievementsDisplay.gameObject.SetActive(true);
		statsDisplay.alpha = 0f;
		statsDisplay.blocksRaycasts = false;
		statsDisplay.interactable = false;
		if (achievementsGrid.childCount < 1)
		{
			foreach (AchievementData achievementDatum in AchievementsCtrl.Instance.GetAchievementData())
			{
				AchievementButton achievementButton = Object.Instantiate(achievementButtonPrefab, achievementsGrid);
				achievementButtons.Add(achievementButton);
				achievementButton.left = showAchievementsButton;
				achievementButton.back = showAchievementsButton;
				achievementButton.Set(achievementDatum, achievementButtons, achievementsScrollbar);
			}
		}
		else
		{
			foreach (AchievementButton achievementButton2 in achievementButtons)
			{
				achievementButton2.CheckUnlocked();
			}
		}
		if (focusFirstAchievement && achievementButtons.Count > 0)
		{
			btnCtrl.SetFocus(achievementButtons[0]);
		}
		showAchievementsButton.right = achievementButtons[0];
		heroSplash.Hide();
		StartCoroutine(_ShowAchievementButtons());
		StartCoroutine(_SetScrollbar());
	}

	private IEnumerator _ShowAchievementButtons()
	{
		foreach (AchievementButton achButton in achievementButtons)
		{
			achButton.anim.SetBool("visible", true);
			float time = Time.time;
			yield return new WaitWhile(() => time + 0.05f > Time.time);
		}
	}

	private IEnumerator _SetScrollbar()
	{
		yield return new WaitForEndOfFrame();
		achievementsScrollbar.value = 1f;
	}

	public void ShowOverallStats()
	{
		containerAnim.SetTrigger("Show");
		statsDisplay.alpha = 1f;
		statsDisplay.blocksRaycasts = true;
		statsDisplay.interactable = true;
		achievementsDisplay.gameObject.SetActive(false);
		heroSplash.Hide();
		title.text = ScriptLocalization.UI.Stats_Statistics;
		StringBuilder stringBuilder = new StringBuilder();
		if (S.I.currentProfile == 0)
		{
			AppendLine(stringBuilder, ScriptLocalization.UI.Stats_Playtime, Utils.GetFormattedTime(SaveDataCtrl.Get("TotalPlaytime", 0)), true);
		}
		else
		{
			AppendLine(stringBuilder, ScriptLocalization.UI.Stats_Playtime, Utils.GetFormattedTime(SaveDataCtrl.Get("TotalPlaytime" + S.I.currentProfile, 0)), true);
		}
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_Victories, SaveDataCtrl.Get("TotalVictories", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_Deaths, SaveDataCtrl.Get("TotalDeaths", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_Zones, SaveDataCtrl.Get("TotalZones", 0));
		stringBuilder.AppendLine();
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_BossesSpared, SaveDataCtrl.Get("TotalSpares", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_BossesExecuted, SaveDataCtrl.Get("TotalExecutions", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_EnemiesKilled, SaveDataCtrl.Get("TotalEnemiesKilled", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_HostagesSaved, SaveDataCtrl.Get("TotalHostagesSaved", 0));
		stringBuilder.AppendLine();
		int currentUnlockLevel = SaveDataCtrl.Get("UnlockLevel", 0);
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_SpellsUnlocked, string.Format("{0}/{1}", S.I.itemMan.unlocks.Where((ItemObject t) => t.unlockLevel <= currentUnlockLevel && t.type == ItemType.Spell).ToList().Count, S.I.itemMan.unlocks.Where((ItemObject t) => t.type == ItemType.Spell).ToList().Count));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_ArtifactsUnlocked, string.Format("{0}/{1}", S.I.itemMan.unlocks.Where((ItemObject t) => t.unlockLevel <= currentUnlockLevel && t.type == ItemType.Art).ToList().Count, S.I.itemMan.unlocks.Where((ItemObject t) => t.type == ItemType.Art).ToList().Count));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_FastestVictory, Utils.GetFormattedTime(SaveDataCtrl.Get("FastestVictory", 0f)));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_LongestWinStreak, SaveDataCtrl.Get("LongestWinStreak", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_CurrentWinStreak, SaveDataCtrl.Get("CurrentWinStreak", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_MostLoops, SaveDataCtrl.Get("MostLoops", 0));
		stringBuilder.AppendLine();
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_DamageDealt, SaveDataCtrl.Get("TotalDamageDealt", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_StepsTaken, SaveDataCtrl.Get("TotalStepsTaken", 0));
		statsText.text = stringBuilder.ToString();
		StringBuilder stringBuilder2 = new StringBuilder();
		List<RunHistoryData> list = SaveDataCtrl.Get("RunHistories", new List<RunHistoryData>());
		for (int num = list.Count - 1; num >= 0; num--)
		{
			string Translation = "Default";
			LocalizationManager.TryGetTranslation("HeroTitles/" + list[num].beingID, out Translation);
			if (string.IsNullOrEmpty(Translation) && heCtrl.spCtrl.beingDictionary.ContainsKey(list[num].beingID))
			{
				Translation = heCtrl.spCtrl.beingDictionary[list[num].beingID].title;
			}
			AppendLine(stringBuilder2, string.Format("{0}/{1}/{2} {3} - {4}", list[num].month, list[num].day, list[num].year, S.I.spCtrl.heroDictionary[list[num].beingName].localizedName, Translation));
			AppendLine(stringBuilder2, "<br>" + ScriptLocalization.UI.Worldbar_Seed + " " + list[num].seed + " | " + list[num].zone + "<br><br>");
		}
		runHistoryText.text = stringBuilder2.ToString();
	}

	public void ShowCharacterStats(string charName)
	{
		containerAnim.SetTrigger("Show");
		statsDisplay.alpha = 1f;
		statsDisplay.blocksRaycasts = true;
		statsDisplay.interactable = true;
		achievementsDisplay.gameObject.SetActive(false);
		heroSplash.ChangeSplash(S.I.spCtrl.heroDictionary[charName]);
		title.text = S.I.spCtrl.heroDictionary[charName].localizedName;
		StringBuilder stringBuilder = new StringBuilder();
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_Playtime, Utils.GetFormattedTime(SaveDataCtrl.Get(charName + "TotalPlaytime", 0)), true);
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_Victories, SaveDataCtrl.Get(charName + "TotalVictories", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_Deaths, SaveDataCtrl.Get(charName + "TotalDeaths", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_Zones, SaveDataCtrl.Get(charName + "TotalZones", 0));
		stringBuilder.AppendLine();
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_BossesSpared, SaveDataCtrl.Get(charName + "TotalSpares", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_BossesExecuted, SaveDataCtrl.Get(charName + "TotalExecutions", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_EnemiesKilled, SaveDataCtrl.Get(charName + "TotalEnemiesKilled", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_HostagesSaved, SaveDataCtrl.Get(charName + "TotalHostagesSaved", 0));
		stringBuilder.AppendLine();
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_FastestVictory, Utils.GetFormattedTime(SaveDataCtrl.Get(charName + "FastestVictory", 0f)));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_LongestWinStreak, SaveDataCtrl.Get(charName + "LongestWinStreak", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_CurrentWinStreak, SaveDataCtrl.Get(charName + "CurrentWinStreak", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_MostLoops, SaveDataCtrl.Get(charName + "MostLoops", 0));
		stringBuilder.AppendLine();
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_DamageDealt, SaveDataCtrl.Get(charName + "TotalDamageDealt", 0));
		AppendLine(stringBuilder, ScriptLocalization.UI.Stats_StepsTaken, SaveDataCtrl.Get(charName + "TotalStepsTaken", 0));
		statsText.text = stringBuilder.ToString();
		runHistoryText.text = string.Empty;
	}

	private void AppendLine(StringBuilder sb, string title, string amount, bool first = false)
	{
		string arg = "\n";
		if (first)
		{
			arg = string.Empty;
		}
		sb.AppendFormat("{0}<alpha=#AA>{1}<alpha=#FF>: {2}", arg, title, amount);
	}

	private void AppendLine(StringBuilder sb, string title, int amount, bool first = false)
	{
		AppendLine(sb, title, amount.ToString(), first);
	}

	private void AppendLine(StringBuilder sb, string content)
	{
		sb.AppendFormat(content);
	}

	public override void Close()
	{
		base.Close();
		S.I.mainCtrl.Open();
		canvasGroup.blocksRaycasts = false;
	}
}
