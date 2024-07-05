using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsCtrl : NavPanel
{
	public List<Sprite> emojis;

	public Image background;

	public Image emojiIcon;

	public Image emojiIconDemo;

	public TMP_Text textSectionPrefab;

	public GameObject doubleTextSectionPrefab;

	public Image imageSectionPrefab;

	public GameObject beingTextPrefab;

	public Transform scrollGrid;

	public RectTransform scrollGridRect;

	public TextAsset kickstarterNames;

	public TextAsset kickstarterAmounts;

	public Animator logo;

	private int emojiCounter = 0;

	public int creditNum = 0;

	public int creditImageNum = 0;

	public float scrollSpeed = 20f;

	private float multiplier = 1f;

	private float fightMultiplier = 1f;

	public Dictionary<string, Sprite> creditImages = new Dictionary<string, Sprite>();

	public List<CreditBeingText> currentCreditBeings = new List<CreditBeingText>();

	public List<string> missedCredits = new List<string>();

	public int creditBeingsLeftAlive = 0;

	public int spawnedCredits = 0;

	public int totalNumCredits = 0;

	public float beingCreditsScrollSpeed = 40f;

	private float beingCreditsSpawnInterval = 0f;

	private string creditsKey = "Credits/Credits";

	private string creditsDescriptionKey = "Description";

	private bool skippingFrame = false;

	public GameObject skipButton;

	public float holdLength = 0f;

	public bool creditsOngoing = false;

	public float kickstarterSkipHoldLength = 1.2f;

	public BC ctrl;

	public MusicCtrl muCtrl;

	public OptionCtrl optCtrl;

	public SpawnCtrl sp;

	public UnlockCtrl unCtrl;

	private bool skipCurrentCredits = false;

	public int preCreditHealthCurrent = 1000;

	public int preCreditHealthMax = 1000;

	public TMP_Text currentTextSection;

	protected override void Awake()
	{
		base.Awake();
		beingCreditsSpawnInterval = 60f / beingCreditsScrollSpeed * 60f / 150f;
	}

	public override void Open()
	{
		base.Open();
		originButton = null;
	}

	private void Update()
	{
		if (skippingFrame || skipCurrentCredits)
		{
			if (skipCurrentCredits)
			{
				multiplier = 60f;
				fightMultiplier += 0.5f;
			}
			else
			{
				skippingFrame = false;
				multiplier = 5f * (1f + holdLength * 2f);
				fightMultiplier = 10f * (1f + holdLength);
			}
			holdLength += Time.deltaTime;
			if (!S.I.ANIMATIONS)
			{
				multiplier = 40f;
			}
		}
		else
		{
			fightMultiplier = 1f;
			multiplier = 1f;
			holdLength = 0f;
		}
		int num = currentCreditBeings.Count - 1;
		while (num >= 0 && !(currentCreditBeings[num].being == null))
		{
			currentCreditBeings[num].being.transform.position += Vector3.up * Time.deltaTime * beingCreditsScrollSpeed * fightMultiplier;
			if (!currentCreditBeings[num].being.dead)
			{
				if (currentCreditBeings[num].being.transform.position.y > 200f)
				{
					missedCredits.Add(currentCreditBeings[num].creditText.text);
					currentCreditBeings[num].being.Clear();
					currentCreditBeings[num].being.Clean();
					currentCreditBeings.Remove(currentCreditBeings[num]);
				}
			}
			else
			{
				currentCreditBeings[num].being.Clean();
				currentCreditBeings.Remove(currentCreditBeings[num]);
			}
			num--;
		}
	}

	public void StartCredits(Ending endingType)
	{
		Open();
		StartCoroutine(_StartCredits(endingType));
	}

	private void CreateDualCreditSectionWithHeader(string headerText = "", string leftColText = "", string rightColText = "", string subtitle = "", bool endPadding = true)
	{
		TMP_Text tMP_Text = Object.Instantiate(textSectionPrefab, scrollGrid);
		tMP_Text.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 0f);
		tMP_Text.text = headerText;
		if (!string.IsNullOrEmpty(subtitle))
		{
			tMP_Text.text = tMP_Text.text + "<br><size=80%><color=#FFFFFF97>" + subtitle + "</color></size><br><br>";
		}
		else
		{
			tMP_Text.text += "<br><br>";
		}
		if (string.IsNullOrEmpty(leftColText) || string.IsNullOrEmpty(rightColText))
		{
			return;
		}
		leftColText = leftColText.Replace("% ", "%");
		string[] array = leftColText.Split('%');
		rightColText = rightColText.Replace("% ", "%");
		string[] array2 = rightColText.Split('%');
		for (int i = 0; i < array.Length; i++)
		{
			TMP_Text[] componentsInChildren = Object.Instantiate(doubleTextSectionPrefab, scrollGrid).GetComponentsInChildren<TMP_Text>();
			TMP_Text component = doubleTextSectionPrefab.GetComponent<TMP_Text>();
			componentsInChildren[0].text = "<color=#FFFFFF97>" + array[i] + "</color>";
			componentsInChildren[1].text = "<color=#FFFFFF97>" + array[i] + "</color>";
			componentsInChildren[2].text = "<color=#FFFFFF>" + array2[i] + "</color>";
			if (endPadding && i == array.Length - 1)
			{
				componentsInChildren[0].text += "<br><br><br>";
			}
		}
	}

	private IEnumerator _StartCredits(Ending endingType)
	{
		unCtrl.itemGrids.blocksRaycasts = false;
		Reset();
		skipButton.SetActive(false);
		U.I.Hide(ctrl.centralMessageContainer);
		ctrl.idCtrl.moneyTextBattle.gameObject.SetActive(false);
		ctrl.runCtrl.worldBar.detailPanel.gameObject.SetActive(false);
		PostCtrl.transitioning = false;
		creditsOngoing = true;
		muCtrl.PlayCredits(endingType);
		yield return new WaitForEndOfFrame();
		while (true)
		{
			if (creditNum == 13)
			{
				currentTextSection = null;
				string locCreditNames = "";
				string locCreditDescriptions = "";
				for (int j = 0; j < 12; j++)
				{
					locCreditNames += LocalizationManager.GetTranslation("Credits/LocCredits" + j);
					locCreditDescriptions += LocalizationManager.GetTranslation("Credits/LocCredits" + j + "Description");
					if (j < 11)
					{
						locCreditNames += "%";
						locCreditDescriptions += "%";
					}
				}
				CreateDualCreditSectionWithHeader(ScriptLocalization.Credits.Riotloc, locCreditDescriptions, locCreditNames, ScriptLocalization.Credits.Localization);
				locCreditNames = "";
				locCreditDescriptions = "";
				for (int l = 12; l < 13; l++)
				{
					locCreditNames += LocalizationManager.GetTranslation("Credits/LocCredits" + l);
					locCreditDescriptions += LocalizationManager.GetTranslation("Credits/LocCredits" + l + "Description");
				}
				CreateDualCreditSectionWithHeader(ScriptLocalization.Credits.Kakehashi_Games, locCreditDescriptions, locCreditNames, ScriptLocalization.Credits.Localization);
				locCreditNames = "";
				locCreditDescriptions = "";
				for (int k = 13; k < 14; k++)
				{
					locCreditNames += LocalizationManager.GetTranslation("Credits/LocCredits" + k);
					locCreditDescriptions += LocalizationManager.GetTranslation("Credits/LocCredits" + k + "Description");
				}
				CreateDualCreditSectionWithHeader(ScriptLocalization.Credits.BADA_GAMES, locCreditDescriptions, locCreditNames, ScriptLocalization.Credits.Localization);
				currentTextSection = null;
				CreateDualCreditSectionWithHeader(ScriptLocalization.Credits.HumbleBundle, ScriptLocalization.Credits.HumbleRoles, ScriptLocalization.Credits.HumbleNames, ScriptLocalization.Credits.Publishing);
				currentTextSection = null;
				CreateDualCreditSectionWithHeader(ScriptLocalization.Credits.Maple_Whispering, ScriptLocalization.Credits.MapleRoles, ScriptLocalization.Credits.MapleNames, ScriptLocalization.Credits.ChinaPublishing, false);
			}
			if (creditNum == 10)
			{
				currentTextSection = null;
				Object.Instantiate(imageSectionPrefab, scrollGrid).sprite = creditImages[endingType.ToString()];
				creditImageNum++;
			}
			if (creditNum == 16)
			{
				currentTextSection = null;
				CreateDualCreditSectionWithHeader(ScriptLocalization.Credits.Cameos, ScriptLocalization.Credits.CameosNames, ScriptLocalization.Credits.CameosOwners, "", false);
			}
			if (currentTextSection == null)
			{
				currentTextSection = Object.Instantiate(textSectionPrefab, scrollGrid);
				currentTextSection.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 0f);
				currentTextSection.text = "<br><br>";
			}
			List<string> lines = GetNextLine();
			if (lines.Count < 1)
			{
				break;
			}
			for (int i = 0; i < 2; i++)
			{
				if (i == 0)
				{
					currentTextSection.text += lines[i];
				}
				else if (lines.Count > 1 && i == 1)
				{
					TMP_Text tMP_Text = currentTextSection;
					tMP_Text.text = tMP_Text.text + "<br><size=80%><color=#FFFFFF97>" + lines[i] + "</color></size>";
				}
			}
			currentTextSection.text += "<br><br><br>";
		}
		yield return new WaitForEndOfFrame();
		float timer = 0f;
		int creditLimit = 150;
		if (endingType == Ending.Genocide)
		{
			creditLimit = 200;
		}
		while (scrollGridRect.localPosition.y - scrollGridRect.sizeDelta.y < (float)creditLimit)
		{
			timer += Time.deltaTime;
			if (timer > 4f && !skipButton.activeSelf)
			{
				skipButton.SetActive(true);
			}
			scrollGrid.transform.position += Vector3.up * Time.deltaTime * scrollSpeed * multiplier;
			yield return null;
		}
		StartCoroutine(StartKickstarterCredits());
	}

	public void SkipHold()
	{
		skipCurrentCredits = true;
	}

	public void SkipCurrentCredits()
	{
		skipCurrentCredits = true;
	}

	public void ChangeEmoji()
	{
		emojiCounter++;
		if (emojiCounter >= emojis.Count)
		{
			emojiCounter = 0;
		}
		emojiIcon.sprite = emojis[emojiCounter];
		emojiIconDemo.sprite = emojis[emojiCounter];
	}

	public List<string> GetNextLine()
	{
		List<string> list = new List<string>();
		creditNum++;
		if (LocalizationManager.GetTranslation(creditsKey + creditNum) != null)
		{
			list.Add(LocalizationManager.GetTranslation(creditsKey + creditNum));
		}
		if (LocalizationManager.GetTranslation(creditsKey + creditNum + creditsDescriptionKey) != null)
		{
			list.Add(LocalizationManager.GetTranslation(creditsKey + creditNum + creditsDescriptionKey));
		}
		return list;
	}

	public IEnumerator StartKickstarterCredits()
	{
		creditBeingsLeftAlive = 0;
		skipCurrentCredits = false;
		PostCtrl.transitioning = false;
		unCtrl.itemGrids.blocksRaycasts = true;
		missedCredits.Clear();
		skipButton.SetActive(true);
		ctrl.idCtrl.moneyTextBattle.gameObject.SetActive(true);
		foreach (Player player in ctrl.currentPlayers)
		{
			player.AddInvince(9999f);
			player.invinceFlash = false;
		}
		if ((bool)ctrl.currentPlayer)
		{
			ctrl.currentPlayer.duelDisk.CreateDeckSpells();
		}
		ctrl.ti.mainBattleGrid.ClearTileColor();
		ctrl.camCtrl.TransitionOutHigh("LeftWipe");
		background.enabled = false;
		originButton = null;
		List<string> kickstarterNameList = new List<string>(kickstarterNames.ToString().Split(','));
		List<string> kickstarterAmountList = new List<string>(kickstarterAmounts.ToString().Split(','));
		totalNumCredits = kickstarterNameList.Count;
		ctrl.GameState = GState.Battle;
		StartCoroutine(ctrl.StartBattle(true));
		if ((bool)ctrl.currentPlayer)
		{
			ctrl.RemoveControlBlocks(Block.GameEnd);
		}
		muCtrl.PauseIntroLoop();
		yield return new WaitForSeconds(1f);
		U.I.Show(ctrl.centralMessageContainer);
		ctrl.audioSource.PlayOneShot(ctrl.battleStartSound);
		ctrl.centralMessageText.text = ScriptLocalization.UI.Battle_SpecialThanks;
		yield return new WaitForSeconds(1f);
		U.I.Hide(ctrl.centralMessageContainer);
		muCtrl.PlayCreditsKickstarter();
		holdLength = 0f;
		int alternator = 4;
		while (kickstarterNameList.Count > 1)
		{
			yield return null;
			if (ctrl.currentPlayers.Count < 1)
			{
				break;
			}
			for (int i = 0; i < 2; i++)
			{
				alternator = ((alternator != 4) ? 4 : 7);
				int randomKickstarterNum = Random.Range(0, kickstarterNameList.Count);
				Being newBeing = sp.CreateBeing(sp.beingDictionary["CreditText"].Clone(), alternator, 0f, 0, false, null, false);
				newBeing.transform.localPosition += Vector3.down * 120f;
				newBeing.mov.currentTile.SetOccupation(0, newBeing.mov.hovering);
				newBeing.mov.forced = true;
				newBeing.health.SetHealth(kickstarterAmountList[randomKickstarterNum]);
				newBeing.shadow.SetActive(false);
				currentCreditBeings.Add(SimplePool.Spawn(beingTextPrefab, newBeing.beingStatsPanel.transform.position, newBeing.beingStatsPanel.transform.rotation, newBeing.beingStatsPanel.transform).GetComponent<CreditBeingText>().Set(kickstarterNameList[randomKickstarterNum], newBeing));
				ctrl.ti.mainBattleGrid.currentStructures.Remove(newBeing.GetComponent<Structure>());
				kickstarterNameList.RemoveAt(randomKickstarterNum);
				kickstarterAmountList.RemoveAt(randomKickstarterNum);
				spawnedCredits++;
			}
			if (holdLength > kickstarterSkipHoldLength)
			{
				kickstarterNameList.Clear();
				kickstarterAmountList.Clear();
				break;
			}
			yield return new WaitForSeconds(beingCreditsSpawnInterval / fightMultiplier);
		}
		float currentWaitTime = 0f;
		while (currentWaitTime < 3f)
		{
			currentWaitTime += Time.deltaTime * fightMultiplier;
			yield return null;
		}
		ctrl.idCtrl.moneyTextBattle.gameObject.SetActive(false);
		foreach (Player player3 in ctrl.currentPlayers)
		{
			player3.health.SetHealth(preCreditHealthCurrent, preCreditHealthMax);
		}
		ctrl.AddControlBlocks(Block.GameEnd);
		ctrl.camCtrl.TransitionInHigh("LeftWipe");
		skipButton.SetActive(false);
		muCtrl.PauseIntroLoop();
		ctrl.ti.mainBattleGrid.ClearField(false, true);
		creditBeingsLeftAlive = currentCreditBeings.Count;
		currentCreditBeings.Clear();
		unCtrl.itemGrids.blocksRaycasts = false;
		foreach (Player player2 in ctrl.currentPlayers)
		{
			player2.RemoveAllStatuses();
			player2.ApplyStun(false);
			player2.ClearQueuedActions();
		}
		yield return new WaitForSeconds(1f);
		logo.SetBool("visible", true);
		logo.SetBool("still", true);
		yield return new WaitForSeconds(2f);
		logo.SetBool("visible", false);
		yield return new WaitForSeconds(3f);
		unCtrl.itemGrids.blocksRaycasts = true;
		Close();
		creditsOngoing = false;
	}

	public void Reset()
	{
		logo.SetBool("visible", false);
		totalNumCredits = 0;
		missedCredits.Clear();
		spawnedCredits = 0;
		creditNum = 0;
		creditImageNum = 0;
		scrollGrid.DestroyChildren();
		scrollGrid.localPosition = Vector3.down * 145f;
		skipCurrentCredits = false;
	}
}
