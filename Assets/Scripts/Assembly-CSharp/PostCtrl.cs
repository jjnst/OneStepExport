using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using I2.Loc;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;

[MoonSharpUserData]
public class PostCtrl : NavPanel
{
	public GameObject postBattlePanel;

	public TMP_Text victoryRankText;

	public TMP_Text timerText;

	public TMP_Text chooseOneText;

	public TMP_Text levelUpAnnounceText;

	public TMP_Text upgradeAnnounceText;

	public List<ItemObject> lootList;

	public Transform choiceCardGrid;

	public Vector3 choiceCardGridReadabilityScale = Vector3.one;

	private Vector3 choiceCardGridOriginalScale = Vector3.one;

	public int experience;

	public int playerLevel;

	public List<int> experienceTiers;

	public List<int> experienceTiersTest;

	public bool smallTiers;

	public int maxArtifactLimit = 96;

	public float luck = 0f;

	public float luckBonusRate = 1f;

	public int focusLuck = 0;

	public int permanentLuck = 0;

	public int baseAmountOfLootDrops = 3;

	public int addAmountOfLootDrops = 4;

	public int levelUpExtraOptions = 2;

	private int totalExpGain;

	private int currentExp;

	private int levelsGained;

	private float currentDisplayedExp;

	public NavButton continueBtn;

	public VoteDisplay continueVoteDisplay;

	public List<UIButton> rewardCardList;

	public AudioClip expUpSound;

	public AudioClip levelUpSound;

	public AudioClip createLootSound;

	public AudioClip createRewardSound;

	public AudioClip skipOptionSound;

	public AudioClip chooseOptionSound;

	public Animator previewButton;

	public GameObject lootStatsPane;

	public GameObject levelPane;

	public ExperienceBar expBar;

	public TMP_Text moneyGainedText;

	public ParticleSystem particleSys;

	public ParticleSystem particleSysCircle;

	public InputIcon continueSkipInputIcon;

	public InputIcon continueAcceptInputIcon;

	private BC ctrl;

	private DeckCtrl deCtrl;

	private FocusCtrl foCtrl;

	private IdleCtrl idCtrl;

	private ReferenceCtrl refCtrl;

	private RunCtrl runCtrl;

	private ItemManager itemMan;

	private TutorialCtrl tutCtrl;

	public XMLReader xmlReader;

	private List<Brand> focusedList = new List<Brand>();

	public List<int> remainingBlessings = new List<int>();

	public List<int> remainingArtDrops = new List<int>();

	public SpellObject originalUpgradeCard;

	public int originalUpgradeCardIndex;

	private bool deckScreenWasOpen = false;

	public static bool transitioning = false;

	public RewardType currentRewardType;

	private bool showOnwardButtonAfter = false;

	private RewardType endingRewardType = RewardType.Loot;

	public float showFocusDelay = 0f;

	protected override void Awake()
	{
		base.Awake();
		ctrl = S.I.batCtrl;
		deCtrl = S.I.deCtrl;
		idCtrl = S.I.idCtrl;
		foCtrl = S.I.foCtrl;
		itemMan = S.I.itemMan;
		refCtrl = S.I.refCtrl;
		runCtrl = S.I.runCtrl;
		tutCtrl = S.I.tutCtrl;
		choiceCardGridOriginalScale = choiceCardGrid.localScale;
		if (S.I.CAMPAIGN_MODE || S.I.scene == GScene.DemoLive)
		{
			playerLevel = 1;
			for (int i = 0; i < 100; i++)
			{
				if (smallTiers)
				{
					experienceTiers.Add(i * 10);
				}
				else if (S.I.scene == GScene.DemoLive)
				{
					experienceTiers.Add(i * 35);
				}
				else
				{
					experienceTiers.Add(i * 50);
				}
			}
		}
		else
		{
			experienceTiers = new List<int>(experienceTiersTest);
		}
		lootList = new List<ItemObject>();
		continueBtn.gameObject.SetActive(false);
		continueVoteDisplay.gameObject.SetActive(false);
	}

	private void Start()
	{
		previewButton.SetBool("visible", false);
	}

	public void StartPostBattle()
	{
		Open();
		idCtrl.heroLevelText.text = string.Format(ScriptLocalization.UI.TopNav_LevelShort + " {0}", playerLevel);
		remainingBlessings.Clear();
		if (!runCtrl.currentRun.seedWasPredefined)
		{
			for (int i = 0; i < SaveDataCtrl.Get("blessings", 0); i++)
			{
				remainingBlessings.Add(0);
			}
		}
		timerText.text = "Time: " + ctrl.stopWatch.FormattedTime();
		victoryRankText.text = string.Format("Rank: {0}", ctrl.battleRank);
		if (runCtrl.currentZoneDot.type == ZoneType.Boss)
		{
			StartLootOptions(RewardType.BossSpell);
		}
		else
		{
			StartLootOptions(RewardType.Loot);
		}
	}

	public void StartLootOptions(RewardType rewardType)
	{
		foreach (Player currentPlayer in ctrl.currentPlayers)
		{
			currentPlayer.anim.SetBool("dashing", false);
			currentPlayer.ApplyStun(false);
			currentPlayer.ClearQueuedActions();
			currentPlayer.RemoveAllStatuses();
		}
		endingRewardType = rewardType;
		ctrl.AddControlBlocks(Block.PostBattle);
		ctrl.GameState = GState.Loot;
		ctrl.camCtrl.TransitionInLow(runCtrl.currentWorld.transition);
		GenerateLootOptions(rewardType);
	}

	public void GenerateLootOptions(RewardType rewardType)
	{
		ClearAndHideCards();
		List<ItemObject> list = new List<ItemObject>();
		if (remainingBlessings.Count > 0)
		{
			list = itemMan.GetItems(remainingBlessings[remainingBlessings.Count - 1], 2, ItemType.Art, true);
			remainingBlessings.RemoveAt(remainingBlessings.Count - 1);
			rewardType = RewardType.Blessing;
		}
		else if (remainingArtDrops.Count > 0)
		{
			list = itemMan.GetItems(remainingArtDrops[remainingArtDrops.Count - 1], 1, ItemType.Art, true);
			remainingArtDrops.RemoveAt(remainingArtDrops.Count - 1);
			rewardType = RewardType.ArtDrop;
		}
		else if (runCtrl.currentZoneDot.type == ZoneType.Miniboss)
		{
			list = itemMan.GetItems(GenerateRewardValue(5), 1, ItemType.Art, true);
			List<ArtifactObject> list2 = itemMan.minibossRewards;
			if (ctrl.currentPlayer.health.current == ctrl.currentPlayer.health.max)
			{
				list2 = list2.Where((ArtifactObject t) => !t.tags.Contains(Tag.Heal)).ToList();
			}
			list.Add(list2[runCtrl.NextPsuedoRand(0, list2.Count)].Clone());
		}
		else if (runCtrl.currentZoneDot.type == ZoneType.Danger)
		{
			list = GetSpells();
			foreach (ItemObject item in list)
			{
				if (item.type == ItemType.Spell)
				{
					GenerateEnhancement(item.spellObj, false, null, 1);
				}
			}
		}
		else
		{
			switch (rewardType)
			{
			case RewardType.BossSpell:
				list = GetSpells(15, 2);
				break;
			case RewardType.BossArt:
				list = itemMan.GetItems(GenerateRewardValue(15, 1), 2, ItemType.Art, true);
				list.Add(itemMan.bossRewards[runCtrl.NextPsuedoRand(0, itemMan.bossRewards.Count)].Clone());
				break;
			default:
				list = GetSpells();
				foreach (ItemObject item2 in list)
				{
					if (item2.type == ItemType.Spell)
					{
						GenerateEnhancement(item2.spellObj);
					}
				}
				break;
			}
		}
		StartCoroutine(StartOptions(list, rewardType));
	}

	public float CombinedLuck()
	{
		return luck + (float)focusLuck + (float)permanentLuck;
	}

	public float DifficultyLuck()
	{
		return luck + (float)permanentLuck;
	}

	private List<ItemObject> GetSpells(int luckBonus = 0, int minRarity = 0)
	{
		focusedList.Clear();
		List<ItemObject> list = new List<ItemObject>();
		while (list.Count < baseAmountOfLootDrops + addAmountOfLootDrops)
		{
			Brand brand = Brand.None;
			if (runCtrl.NextPsuedoRand(0, 6) == 0 && foCtrl.brandDisplayButtons[0].brand != 0)
			{
				brand = foCtrl.brandDisplayButtons[0].brand;
				focusedList.Add(foCtrl.brandDisplayButtons[0].brand);
			}
			else if (runCtrl.NextPsuedoRand(0, 6) == 0 && foCtrl.brandDisplayButtons[1].brand != 0)
			{
				brand = foCtrl.brandDisplayButtons[1].brand;
				focusedList.Add(foCtrl.brandDisplayButtons[1].brand);
			}
			else
			{
				focusedList.Add(Brand.None);
			}
			list.AddRange(itemMan.GetItems(GenerateRewardValue(luckBonus, minRarity), 1, ItemType.Spell, true, brand, list));
			if (focusedList[focusedList.Count - 1] != 0 && list[list.Count - 1].brand != focusedList[focusedList.Count - 1])
			{
				focusedList.RemoveAt(focusedList.Count - 1);
				focusedList.Add(Brand.None);
			}
		}
		return list;
	}

	private void Update()
	{
		if (showFocusDelay >= 0f)
		{
			showFocusDelay -= Time.deltaTime;
		}
		if (!open)
		{
			return;
		}
		if (S.I.readabilityModeEnabled)
		{
			if (choiceCardGrid.localScale != choiceCardGridReadabilityScale)
			{
				choiceCardGrid.localScale = choiceCardGridReadabilityScale;
			}
		}
		else if (choiceCardGrid.localScale != choiceCardGridOriginalScale)
		{
			choiceCardGrid.localScale = choiceCardGridOriginalScale;
		}
	}

	public void EndLoot(RewardType rewardType, bool skipped)
	{
		StartCoroutine(EndOptions(rewardType, skipped));
		continueBtn.tmpText.text = ScriptLocalization.UI.CONTINUE;
		continueBtn.gameObject.SetActive(false);
	}

	private IEnumerator StartExperienceSeq()
	{
		transitioning = true;
		previewButton.SetBool("visible", false);
		if (ctrl.GameState == GState.GameOver)
		{
			transitioning = false;
			yield break;
		}
		ctrl.GameState = GState.Experience;
		totalExpGain = ctrl.experienceGained;
		currentDisplayedExp = currentExp * 10;
		U.I.Hide(lootStatsPane);
		U.I.Show(levelPane);
		expBar.levelText.text = playerLevel.ToString();
		levelsGained = 0;
		currentExp = experience;
		expBar.lowerLevelText.text = playerLevel.ToString();
		expBar.higherLevelText.text = (playerLevel + 1).ToString();
		int finalExp = currentExp + ctrl.experienceGained;
		currentDisplayedExp = currentExp * 10;
		float finalDisplayedExp = finalExp * 10;
		UpdateExpBar();
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.3f);
		}
		float expSound = 0f;
		float currentVelocity = 0f;
		while (currentDisplayedExp < finalDisplayedExp)
		{
			float newSize = Mathf.SmoothDamp(currentDisplayedExp, finalDisplayedExp, ref currentVelocity, 0.3f);
			if (currentDisplayedExp == (float)Mathf.RoundToInt(newSize))
			{
				currentDisplayedExp += 1f;
			}
			else
			{
				currentDisplayedExp = Mathf.RoundToInt(newSize);
			}
			if (finalDisplayedExp - currentDisplayedExp < 1f)
			{
				currentDisplayedExp = finalDisplayedExp;
			}
			currentExp = Mathf.RoundToInt(currentDisplayedExp * 0.1f);
			float expLeft = finalDisplayedExp - currentDisplayedExp;
			expSound += 0.5f;
			if (expLeft < 50f)
			{
				expSound -= 0.25f;
			}
			if (expSound >= 1f)
			{
				S.I.PlayOnce(expUpSound);
				expSound = 0f;
			}
			UpdateExpBar();
			if (currentExp >= experienceTiers[playerLevel])
			{
				currentExp -= experienceTiers[playerLevel];
				finalExp -= experienceTiers[playerLevel];
				currentDisplayedExp -= experienceTiers[playerLevel] * 10;
				finalDisplayedExp -= (float)(experienceTiers[playerLevel] * 10);
				playerLevel++;
				levelsGained++;
				experience = 0;
				currentVelocity = 0f;
				expSound = 0f;
				U.I.Show(levelUpAnnounceText);
				levelUpAnnounceText.text = ScriptLocalization.UI.LEVEL_UP_;
				expBar.levelText.text = playerLevel.ToString();
				S.I.PlayOnce(levelUpSound);
				idCtrl.heroLevelText.text = string.Format(ScriptLocalization.UI.TopNav_LevelShort + " {0}", playerLevel);
				expBar.lowerLevelText.text = playerLevel.ToString();
				expBar.higherLevelText.text = (playerLevel + 1).ToString();
				deCtrl.statsScreen.UpdateStats();
				expBar.SetTargetToMax();
				while ((double)expBar.expBarFill.fillAmount < 0.99)
				{
					expSound += 0.5f;
					if (expSound >= 1f)
					{
						S.I.PlayOnce(expUpSound);
						expSound = 0f;
					}
					yield return null;
				}
				particleSys.Play();
				particleSysCircle.Play();
				if (S.I.ANIMATIONS)
				{
					yield return new WaitForSeconds(0.1f);
				}
				U.I.Hide(levelUpAnnounceText);
				if (S.I.ANIMATIONS)
				{
					yield return new WaitForSeconds(0.1f);
				}
				expBar.EmptyBar();
			}
			yield return null;
		}
		ctrl.experienceGained = 0;
		ctrl.moneyGained = 0;
		currentExp = finalExp;
		experience = currentExp;
		UpdateExpBar();
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (levelsGained > 0)
		{
			StartLevelUpOptions();
		}
		else
		{
			continueBtn.gameObject.SetActive(true);
			continueSkipInputIcon.gameObject.SetActive(false);
			continueAcceptInputIcon.gameObject.SetActive(true);
			continueBtn.holdToAccept = false;
			btnCtrl.SetFocus(continueBtn);
		}
		transitioning = false;
	}

	public void UpdateExpBar()
	{
		expBar.UpdateBar(currentDisplayedExp * 1f / ((float)experienceTiers[playerLevel] * 1f * 10f));
		expBar.experienceText.text = string.Format("+{0}", totalExpGain);
		expBar.experiencePortionText.text = string.Format("{0}/{1}", currentExp, experienceTiers[playerLevel]);
	}

	public void StartLevelUpOptions()
	{
		lootList.Clear();
		List<ItemObject> list = new List<ItemObject>();
		List<ArtifactObject> list2 = itemMan.baseArtList;
		if (ctrl.currentPlayer.health.current == ctrl.currentPlayer.health.max)
		{
			list2 = list2.Where((ArtifactObject t) => !t.tags.Contains(Tag.Heal)).ToList();
		}
		list.Add(list2[runCtrl.NextPsuedoRand(0, list2.Count)].Clone());
		list.AddRange(itemMan.GetItems(GenerateRewardValue(), levelUpExtraOptions, ItemType.Art, true));
		levelsGained--;
		StartCoroutine(StartOptions(list, RewardType.LevelUp));
	}

	public void EndLevelUpOptions(bool skipped)
	{
		if (!transitioning)
		{
			StartCoroutine(EndOptions(RewardType.LevelUp, skipped));
		}
	}

	public IEnumerator StartOptions(List<ItemObject> finalOptions, RewardType rewardType, int siblingIndex = -1)
	{
		transitioning = true;
		deckScreenWasOpen = false;
		if (deCtrl.deckScreen.slideBody.onScreen)
		{
			deckScreenWasOpen = true;
		}
		if (deCtrl.deckScreen.open)
		{
			deCtrl.deckScreen.Close();
		}
		currentRewardType = rewardType;
		if (rewardType == RewardType.Blessing)
		{
			SaveDataCtrl.Set("blessings", 0);
			SaveDataCtrl.Write();
		}
		continueSkipInputIcon.gameObject.SetActive(true);
		continueAcceptInputIcon.gameObject.SetActive(false);
		yield return new WaitForSeconds(0.3f);
		if (ctrl.currentPlayer == null)
		{
			Close();
			yield break;
		}
		switch (rewardType)
		{
		case RewardType.Loot:
			moneyGainedText.text = "+" + ctrl.moneyGained;
			moneyGainedText.transform.parent.GetComponent<Animator>().SetTrigger("show");
			U.I.Show(lootStatsPane);
			break;
		case RewardType.ArtDrop:
			U.I.Show(lootStatsPane);
			break;
		}
		lootList = finalOptions;
		bool noMoreRoomForArtifacts = false;
		if (lootList.Count > 0 && lootList[0].type == ItemType.Art && deCtrl.artCardList.Count >= maxArtifactLimit)
		{
			noMoreRoomForArtifacts = true;
			lootList.Clear();
		}
		CreateRewardCards(rewardType, siblingIndex);
		StartCoroutine(ShowCards(choiceCardGrid));
		if (rewardType == RewardType.Upgrade)
		{
			U.I.Show(upgradeAnnounceText);
			upgradeAnnounceText.text = ScriptLocalization.UI.UPGRADE;
			yield return null;
			U.I.Hide(upgradeAnnounceText);
		}
		yield return new WaitForSeconds(0.01f);
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.3f);
		}
		idCtrl.ShowOnwardButton();
		showFocusDelay = 0.4f;
		if (deCtrl.deckScreen.open)
		{
			deCtrl.deckScreen.Close();
		}
		if (rewardCardList.Count > 0)
		{
			continueBtn.holdToAccept = true;
			btnCtrl.SetFocus(rewardCardList[0]);
		}
		else
		{
			continueBtn.holdToAccept = false;
			btnCtrl.SetFocus(continueBtn);
		}
		previewButton.SetBool("visible", false);
		if (noMoreRoomForArtifacts)
		{
			chooseOneText.text = ScriptLocalization.UI.Rewards_NoMoreRoomArtifacts;
		}
		else
		{
			switch (rewardType)
			{
			case RewardType.Blessing:
				chooseOneText.text = ScriptLocalization.UI.Rewards_BlessingFound;
				break;
			case RewardType.ArtDrop:
				chooseOneText.text = ScriptLocalization.UI.Rewards_ArtifactFound;
				break;
			default:
				if (finalOptions.Count > 0 && finalOptions[0].type == ItemType.Spell)
				{
					chooseOneText.text = ScriptLocalization.UI.Rewards_ChooseOneSpell;
					previewButton.SetBool("visible", true);
				}
				else
				{
					chooseOneText.text = ScriptLocalization.UI.Rewards_ChooseOneArtifact;
				}
				break;
			}
		}
		if (S.I.twClient.VotingOnline() && rewardCardList.Count > 0)
		{
			chooseOneText.text = string.Empty;
			S.I.twClient.StartVoting();
			StartCoroutine(c_StartVoteTimer(TwitchClient.votingDuration));
		}
		U.I.Show(chooseOneText);
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.1f);
		}
		idCtrl.MakeWorldBarAvailable(false, false);
		continueBtn.gameObject.SetActive(true);
		if (rewardType == RewardType.Upgrade)
		{
			continueBtn.tmpText.text = ScriptLocalization.UI.SKIP_UPGRADES;
		}
		else
		{
			continueBtn.tmpText.text = ScriptLocalization.UI.SKIP_REWARDS;
		}
		transitioning = false;
	}

	private IEnumerator c_StartVoteTimer(float timerLength)
	{
		float endTime = Time.time + timerLength;
		int lastDisplayedTimeRemaining = 0;
		while (endTime > Time.time && S.I.twClient.votingOpen)
		{
			if (lastDisplayedTimeRemaining != Mathf.RoundToInt(endTime - Time.time))
			{
				lastDisplayedTimeRemaining = Mathf.RoundToInt(endTime - Time.time);
				chooseOneText.text = string.Format("{0} {1}{2} {3}", ScriptLocalization.UI.VOTE, lastDisplayedTimeRemaining, ScriptLocalization.UI.seconds_shorthand, ScriptLocalization.UI.Twitch_TimeLeft);
			}
			yield return null;
		}
		if (S.I.twClient.votingOpen)
		{
			S.I.twClient.EndVoting();
		}
		chooseOneText.text = string.Format(ScriptLocalization.UI.VOTING_ENDED);
	}

	private IEnumerator EndOptions(RewardType rewardType = RewardType.Loot, bool skipped = false)
	{
		if (transitioning)
		{
			yield break;
		}
		continueVoteDisplay.gameObject.SetActive(false);
		S.I.twClient.EndVoting();
		U.I.Hide(chooseOneText);
		transitioning = true;
		if (skipped)
		{
			S.I.PlayOnce(skipOptionSound);
		}
		else
		{
			S.I.PlayOnce(chooseOptionSound);
		}
		foreach (Transform child in choiceCardGrid.transform)
		{
			yield return new WaitForSeconds(0.05f);
			child.GetComponent<Animator>().SetBool("OnScreen", false);
		}
		yield return new WaitForSeconds(0.1f);
		ClearAndHideCards();
		yield return new WaitForSeconds(0.01f);
		transitioning = false;
		int num;
		switch (rewardType)
		{
		case RewardType.LevelUp:
			if (levelsGained > 0)
			{
				StartLevelUpOptions();
				yield break;
			}
			U.I.Hide(levelPane);
			EndPostBattle();
			yield break;
		case RewardType.Upgrade:
			StartCoroutine(_Close(true));
			deCtrl.deckScreen.upgradeInProgress = false;
			yield break;
		default:
			num = ((rewardType == RewardType.ArtDrop) ? 1 : 0);
			break;
		case RewardType.Blessing:
			num = 1;
			break;
		}
		if (num != 0)
		{
			GenerateLootOptions(endingRewardType);
		}
		else if (rewardType == RewardType.BossSpell)
		{
			GenerateLootOptions(RewardType.BossArt);
		}
		else
		{
			StartCoroutine(StartExperienceSeq());
		}
	}

	public void ClearAndHideCards()
	{
		choiceCardGrid.DestroyChildren();
		lootList.Clear();
		rewardCardList.Clear();
		deCtrl.displayCardAlt.Hide();
	}

	public void HoldContinue()
	{
		if (S.I.holdToSkipEnabled && choiceCardGrid.childCount > 0)
		{
			continueBtn.OnAcceptHold();
		}
		else
		{
			ClickContinue();
		}
	}

	public void ClickContinue()
	{
		if (transitioning || !continueBtn.gameObject.activeSelf)
		{
			return;
		}
		btnCtrl.RemoveFocus();
		continueBtn.gameObject.SetActive(false);
		continueBtn.tmpText.color = Color.white;
		if (currentRewardType == RewardType.LevelUp || ctrl.GameState == GState.Experience)
		{
			EndLevelUpOptions(true);
		}
		else if (currentRewardType == RewardType.Upgrade)
		{
			if ((bool)ctrl.currentPlayer)
			{
				deCtrl.CreatePlayerItem(originalUpgradeCard, deCtrl.deckScreen.deckGrid, originalUpgradeCardIndex, ctrl.currentPlayer.duelDisk);
				EndLoot(RewardType.Upgrade, true);
			}
		}
		else
		{
			EndLoot(currentRewardType, true);
		}
	}

	public void EndPostBattle()
	{
		continueBtn.gameObject.SetActive(false);
		S.I.muCtrl.PauseIntroLoop();
		Close();
	}

	public override void Close()
	{
		StartCoroutine(_Close());
		deCtrl.deckScreen.upgradeInProgress = false;
	}

	private IEnumerator _Close(bool upgrade = false)
	{
		U.I.Hide(chooseOneText);
		if (runCtrl.currentRun == null)
		{
			_003C_003En__0();
			ctrl.camCtrl.TransitionOutLow("FlowerThree");
			yield break;
		}
		transitioning = true;
		if (!upgrade)
		{
			if (!SaveDataCtrl.Get("TutorialPathShown", false) || (S.I.scene == GScene.DemoLive && runCtrl.currentRun.zoneNum == 1))
			{
				tutCtrl.StartTutorialPath();
				while (tutCtrl.tutorialPathsInProgress)
				{
					yield return new WaitForEndOfFrame();
				}
				ctrl.RemoveControlBlocks(Block.Tutorial);
			}
			runCtrl.SaveRun();
		}
		yield return new WaitForSeconds(0.4f);
		if (ctrl.deCtrl.deckScreen.open)
		{
			ctrl.deCtrl.deckScreen.Close();
		}
		if (!upgrade)
		{
			ctrl.camCtrl.TransitionOutLow(runCtrl.currentWorld.transition);
		}
		else
		{
			ctrl.camCtrl.TransitionOutLow("Upgrade");
		}
		_003C_003En__0();
		ctrl.GameState = GState.Idle;
		foreach (Player player in ctrl.currentPlayers)
		{
			player.mov.SetState(State.Idle);
			player.ClearQueuedActions();
			player.Undown();
		}
		ctrl.RemoveControlBlocks(Block.PostBattle);
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.2f);
		}
		if (!upgrade)
		{
			runCtrl.ShowControlsPanel(runCtrl.currentRun.zoneNum, false);
			S.I.muCtrl.PlayIdle();
		}
		else if (deckScreenWasOpen)
		{
			transitioning = true;
			idCtrl.HideOnwardButton();
			yield return new WaitForEndOfFrame();
			idCtrl.onwardBtn.inverse = false;
			idCtrl.ShowOnwardButton();
			transitioning = false;
			deCtrl.deckScreen.Open();
			deckScreenWasOpen = false;
		}
		else if (showOnwardButtonAfter)
		{
			idCtrl.ShowOnwardButton();
		}
		idCtrl.MakeWorldBarAvailable();
		refCtrl.Hide();
		transitioning = false;
	}

	private void CreateRewardCards(RewardType rewardType, int siblingIndex = -1)
	{
		if (S.I.EDITION != Edition.Dev || ctrl.GameState == GState.Loot)
		{
		}
		if (S.I.twClient.VotingOnline())
		{
			S.I.twClient.voteDisplays.Clear();
			continueVoteDisplay.gameObject.SetActive(true);
			continueVoteDisplay.Reset(0);
			S.I.twClient.voteDisplays.Add(continueVoteDisplay);
		}
		for (int i = 0; i < lootList.Count; i++)
		{
			ChoiceCard choiceCard = deCtrl.CreateNewChoiceCard(lootList[i], choiceCardGrid);
			choiceCard.siblingIndex = siblingIndex;
			choiceCard.rewardType = rewardType;
			choiceCard.poCtrl = this;
			choiceCard.refCtrl = refCtrl;
			if (S.I.twClient.VotingOnline())
			{
				if (choiceCard.cardInner.voteDisplay == null)
				{
					int num = 0;
					if (choiceCard.itemObj.type == ItemType.Spell)
					{
						num = 6;
					}
					VoteDisplay newVoteDisplay = UnityEngine.Object.Instantiate(deCtrl.voteDisplayPrefab, choiceCard.cardInner.transform.position - new Vector3(0f, (choiceCard.cardInner.rect.sizeDelta.y / 2f + (float)num) * choiceCardGrid.localScale.y, 0f), base.transform.rotation);
					choiceCard.cardInner.SetVoteDisplay(newVoteDisplay, i, S.I.twClient);
				}
				else
				{
					choiceCard.cardInner.voteDisplay.gameObject.SetActive(true);
				}
				choiceCard.cardInner.voteDisplay.Reset(i + 1);
				S.I.twClient.voteDisplays.Add(choiceCard.cardInner.voteDisplay);
			}
			else if (choiceCard.cardInner.voteDisplay != null)
			{
				choiceCard.cardInner.voteDisplay.gameObject.SetActive(false);
			}
			if (rewardType == RewardType.Loot && choiceCard.itemObj.type == ItemType.Spell)
			{
				for (int j = 0; j < foCtrl.brandDisplayButtons.Length; j++)
				{
					if (foCtrl.brandDisplayButtons[j].brand == choiceCard.itemObj.brand)
					{
						choiceCard.FocusGlow();
						break;
					}
				}
			}
			rewardCardList.Add(choiceCard);
		}
		if (rewardCardList.Count > 0)
		{
			continueBtn.left = rewardCardList[0];
			continueBtn.up = rewardCardList[0];
			continueBtn.right = rewardCardList[0];
		}
		if (rewardCardList.Count > 1)
		{
			continueBtn.up = rewardCardList[1];
		}
		if (rewardCardList.Count > 2)
		{
			continueBtn.right = rewardCardList[2];
		}
	}

	private IEnumerator ShowCards(Transform parentGrid)
	{
		yield return new WaitForEndOfFrame();
		foreach (Transform child in parentGrid)
		{
			yield return new WaitForSeconds(0.15f);
			S.I.PlayOnce(createRewardSound);
			if ((bool)child)
			{
				child.GetComponent<Animator>().SetBool("OnScreen", true);
			}
		}
	}

	public int AddArtDrop(int rarity = -1)
	{
		if (rarity == -1)
		{
			rarity = GenerateRewardValue();
		}
		remainingArtDrops.Add(rarity);
		return rarity;
	}

	public int GenerateRewardValue(int bonus = 0, int minimumRarity = 0)
	{
		int num = 0;
		if (S.I.EDITION == Edition.DemoLive)
		{
			num += 35;
		}
		num += bonus;
		float num2 = Mathf.Clamp(CombinedLuck(), 0f, 100f) / 70f;
		float num3 = 67f + -36f * num2;
		float num4 = 26f + 9f * num2;
		float num5 = 7f + 18f * num2;
		float num6 = 1f + 5f * num2;
		float num7 = -1f + 4f * num2;
		float f = num3 + num4 + num5 + num6 + num7;
		float num8 = runCtrl.NextPsuedoRand(num, Mathf.RoundToInt(f));
		int num9 = -1;
		if (num8 <= num3)
		{
			num9 = 0;
		}
		else if (num8 <= num3 + num4)
		{
			num9 = 1;
		}
		else if (num8 <= num3 + num4 + num5)
		{
			num9 = 2;
		}
		else if (num8 <= num3 + num4 + num5 + num6)
		{
			num9 = 3;
		}
		else if (num8 <= num3 + num4 + num5 + num6 + num7)
		{
			num9 = 4;
		}
		if (num9 < minimumRarity)
		{
			num9 = minimumRarity;
		}
		if (S.I.EDITION == Edition.DemoLive && num9 > 3)
		{
			num9 = 3;
		}
		return num9;
	}

	public void StartUpgrade(SpellObject spellObj, int siblingIndex)
	{
		ClearAndHideCards();
		Open();
		showOnwardButtonAfter = idCtrl.onwardBtn.onScreen;
		if (idCtrl.onwardBtn.onScreen)
		{
			idCtrl.HideOnwardButton();
		}
		deCtrl.TriggerAllArtifacts(FTrigger.OnUpgradeSpell);
		ctrl.camCtrl.TransitionInLow("Upgrade");
		ctrl.AddControlBlocks(Block.PostBattle);
		List<ItemObject> list = new List<ItemObject>();
		originalUpgradeCard = spellObj;
		originalUpgradeCardIndex = siblingIndex;
		for (int i = 0; i < 3; i++)
		{
			list.Add(spellObj.Clone());
		}
		List<Enhancement> list2 = new List<Enhancement>();
		foreach (SpellObject item in list)
		{
			int count = item.enhancements.Count;
			GenerateEnhancement(item, true, list2, 1);
			if (count < item.enhancements.Count)
			{
				list2.Add(item.enhancements[item.enhancements.Count - 1]);
			}
		}
		StartCoroutine(StartOptions(list, RewardType.Upgrade, siblingIndex));
	}

	public void EndUpgradeOptions(bool skipped)
	{
		if (!transitioning)
		{
			StartCoroutine(EndOptions(RewardType.Upgrade, skipped));
		}
	}

	private void GenerateEnhancement(SpellObject spellObj, bool upgrade = false, List<Enhancement> bannedEnhancements = null, int baseAmount = 0)
	{
		List<Enhancement> list = Enum.GetValues(typeof(Enhancement)).Cast<Enhancement>().ToList();
		float num = (float)runCtrl.NextPsuedoRand(0, 30) + CombinedLuck();
		int num2 = baseAmount;
		int count = spellObj.enhancements.Count;
		if (!upgrade)
		{
			if (num > 55f)
			{
				num2++;
			}
			if (num > 70f)
			{
				num2++;
			}
		}
		spellObj.nameString += "<color=#3fc380>";
		for (int i = 0; i < num2; i++)
		{
			List<int> list2 = Utils.RandomList(list.Count, true);
			for (int j = 0; j < list.Count; j++)
			{
				if (bannedEnhancements == null || !bannedEnhancements.Contains(list[list2[j]]))
				{
					EnhanceSpell(spellObj, list[list2[j]]);
					if (spellObj.enhancements.Count > count)
					{
						count = spellObj.enhancements.Count;
						break;
					}
				}
			}
		}
		spellObj.nameString += "</color>";
	}

	public void EnhanceSpell(SpellObject spellObj, Enhancement enhancement)
	{
		switch (enhancement)
		{
		case Enhancement.Critical:
			if (spellObj.damage > 0f && !spellObj.hitSelf && !spellObj.HasEffect(Effect.Summon) && spellObj.hitEnemies)
			{
				spellObj.nameString += U.I.Colorify(" J", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnHit, 0f, 0f, Effect.CastSpell, Target.Hit, 0f, 0f, "HitCritical", 0.25f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.DamagePlus:
			if (spellObj.damage >= 30f && spellObj.damage <= 80f && !spellObj.HasEffect(Effect.Summon))
			{
				spellObj.nameString += U.I.Colorify(" A", UIColor.Enhancement);
				spellObj.damage += 20f;
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Double:
			if (spellObj.GetNumEnhancements(enhancement) < 2 && !spellObj.tags.Contains(Tag.NoDoublecast) && !spellObj.effectTags.Contains(Effect.Channel) && !spellObj.effectTags.Contains(Effect.Chrono))
			{
				spellObj.nameString += U.I.Colorify(" D", UIColor.Enhancement);
				spellObj.mana += Mathf.Clamp(spellObj.mana - 1f, 1f, 2f);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.DoubleCast, Target.Self, 0f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.ExtendedMag:
			if (spellObj.numShots >= 4f && spellObj.numShots < 20f && spellObj.GetNumEnhancements(enhancement) < 1 && !spellObj.effectTags.Contains(Effect.Channel) && spellObj.numShotsType.type == AmountType.Normal)
			{
				spellObj.nameString += U.I.Colorify(" X", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.Jam, Target.Self, 1f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.numShots += 4f;
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Flame:
			if (!spellObj.HasEffect(Effect.Flame) && !spellObj.hitSelf && !spellObj.HasEffect(Effect.Summon) && spellObj.hitEnemies)
			{
				spellObj.nameString += U.I.Colorify(" F", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnHit, 0f, 0f, Effect.Flame, Target.Hit, 0f, 0f, spellObj.itemID, 0.25f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Flex:
			if (!spellObj.HasEffect(Effect.Consume) && spellObj.GetValue("ThisSpell") == null && spellObj.GetEffect(Effect.Redeck) == null && !spellObj.tags.Contains(Tag.NoFlex))
			{
				spellObj.nameString += U.I.Colorify(" L", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.Consume, Target.Default, 0f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.MaxHPChangeBattle, Target.Self, 5f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.HealBattle, Target.Self, 5f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Flow:
			if (spellObj.GetNumEnhancements(enhancement) < 1 && !spellObj.HasEffect(Effect.FlowStack) && !spellObj.tags.Contains(Tag.NoFlow))
			{
				spellObj.nameString += U.I.Colorify(" E", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.FlowStack, Target.Self, 1f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Fragile:
			if (spellObj.GetNumEnhancements(enhancement) < 1 && !spellObj.HasEffect(Effect.Fragile) && spellObj.shotDuration > 0f && spellObj.numShots > 0f && spellObj.numShots < 12f && !spellObj.HasParam("numOfWaves") && !spellObj.HasEffect(Effect.Summon))
			{
				spellObj.nameString += U.I.Colorify(" R", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnHit, 0f, 0f, Effect.Fragile, Target.Hit, 1f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Frost:
			if (!spellObj.HasEffect(Effect.Frost) && spellObj.numShots <= 4f && !spellObj.HasParam("numOfWaves") && !spellObj.HasEffect(Effect.Summon) && !spellObj.hitSelf && spellObj.hitEnemies && !spellObj.HasEffect(Effect.AddToDiscard))
			{
				spellObj.nameString += U.I.Colorify(" G", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnHit, 0f, 0f, Effect.Frost, Target.Hit, 1f, 0f, spellObj.itemID, 0.25f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.KunaiPlus:
			if (spellObj.HasEffect(Effect.AddToDeck) && spellObj.GetValue("Kunai") != null && spellObj.GetNumEnhancements(enhancement) < 1)
			{
				spellObj.nameString += U.I.Colorify(" K", UIColor.Enhancement);
				spellObj.GetValue("Kunai").amount += 1f;
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Heal:
			if (!spellObj.HasEffect(Effect.HealBattle) || !(spellObj.GetEffect(Effect.HealBattle).amount > 5f) || !spellObj.hitSelf)
			{
				break;
			}
			spellObj.nameString += U.I.Colorify(" H", UIColor.Enhancement);
			foreach (EffectApp effect in spellObj.GetEffects(Effect.HealBattle))
			{
				effect.amount += effect.amount;
			}
			spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.Fragile, Target.Self, 2f, 0f, spellObj.itemID, 1f, 0f));
			spellObj.enhancements.Add(enhancement);
			break;
		case Enhancement.ManaRe:
			if (spellObj.mana > 1f && spellObj.GetNumEnhancements(enhancement) < 2 && spellObj.manaType.type == AmountType.Normal)
			{
				spellObj.nameString += U.I.Colorify(" M", UIColor.Enhancement);
				spellObj.mana = Mathf.Clamp(spellObj.mana -= 1f, 0f, 99f);
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Mini:
			if (spellObj.mana > 2f && spellObj.damage > 20f && spellObj.GetNumEnhancements(enhancement) < 2 && !spellObj.HasEffect(Effect.Summon) && spellObj.GetNumEnhancements(enhancement) < 1)
			{
				spellObj.nameString += U.I.Colorify(" I", UIColor.Enhancement);
				spellObj.mana = 1f;
				spellObj.damage = Mathf.RoundToInt(spellObj.damage / 2f);
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Overload:
			if (!spellObj.HasEffect(Effect.Consume) && spellObj.GetNumEnhancements(enhancement) < 1 && spellObj.damage > 1f && !spellObj.HasEffect(Effect.Summon))
			{
				spellObj.nameString += U.I.Colorify(" O", UIColor.Enhancement);
				spellObj.damage *= 2f;
				spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.Consume, Target.Default, 0f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Pierce:
			if (spellObj.destroyOnHit && spellObj.shotVelocity > 10f && !spellObj.tags.Contains(Tag.NoPierce))
			{
				spellObj.nameString += U.I.Colorify(" C", UIColor.Enhancement);
				spellObj.destroyOnHit = false;
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Poison:
			if (spellObj.numShots > 1f && spellObj.numShots < 6f && spellObj.damage > 1f && !spellObj.HasEffect(Effect.Summon) && !spellObj.HasEffect(Effect.Poison))
			{
				spellObj.nameString += U.I.Colorify(" P", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnHit, 0f, 0f, Effect.Poison, Target.Hit, 10f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.PoisonPlus:
			if (!spellObj.HasEffect(Effect.Poison) || spellObj.HasEffect(Effect.Summon) || !(spellObj.GetEffect(Effect.Poison).amount >= 10f))
			{
				break;
			}
			spellObj.nameString += U.I.Colorify(" Q", UIColor.Enhancement);
			foreach (EffectApp effect2 in spellObj.GetEffects(Effect.Poison))
			{
				effect2.amount += Mathf.Round(effect2.amount * 0.25f);
			}
			spellObj.enhancements.Add(enhancement);
			break;
		case Enhancement.Refund:
			if (spellObj.mana > 0f)
			{
				float amountVar = 1f;
				spellObj.nameString += U.I.Colorify(" Y", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.Mana, Target.Self, 1f, 0f, null, 1f, 0f, null, 1f, null, "", new AmountApp(ref amountVar, "ManaCost*0.5")));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Sharpened:
			if (spellObj.damage >= 60f && !spellObj.HasEffect(Effect.Summon) && spellObj.numShots < 6f)
			{
				spellObj.nameString += U.I.Colorify(" N", UIColor.Enhancement);
				spellObj.damage += 40f;
				spellObj.efApps.Add(new EffectApp(FTrigger.OnHit, 0f, 0f, Effect.PowerUp, Target.Default, -10f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Shield:
			if (!spellObj.HasEffect(Effect.Shield) && !spellObj.tags.Contains(Tag.NoShield))
			{
				spellObj.nameString += U.I.Colorify(" S", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.Shield, Target.Self, 10f, 0f, "", 1f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.ShieldPlus:
			if (!spellObj.HasEffect(Effect.Shield) || spellObj.HasEffect(Effect.Summon) || !(spellObj.GetEffect(Effect.Shield).amount >= 10f) || spellObj.tags.Contains(Tag.NoShieldPlus) || (spellObj.GetEffect(Effect.Shield).amountApp != null && spellObj.GetEffect(Effect.Shield).amountApp.type != 0))
			{
				break;
			}
			spellObj.nameString += U.I.Colorify(" U", UIColor.Enhancement);
			foreach (EffectApp effect3 in spellObj.GetEffects(Effect.Shield))
			{
				effect3.amount += 20f;
			}
			spellObj.enhancements.Add(enhancement);
			break;
		case Enhancement.Shot:
			if (spellObj.numShots >= 3f && spellObj.numShots < 20f && spellObj.GetNumEnhancements(enhancement) < 1 && !spellObj.effectTags.Contains(Effect.Channel) && spellObj.numShotsType.type == AmountType.Normal)
			{
				spellObj.nameString += U.I.Colorify(" V", UIColor.Enhancement);
				spellObj.numShots += 2f;
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Splash:
			if (spellObj.damage >= 5f && !spellObj.hitSelf && !spellObj.HasEffect(Effect.Summon))
			{
				spellObj.nameString += U.I.Colorify(" W", UIColor.Enhancement);
				spellObj.efApps.Add(new EffectApp(FTrigger.OnHit, 0f, 0f, Effect.CastSpell, Target.Default, 0f, 0f, "HitSplash", 1f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		case Enhancement.Strafe:
		{
			for (int num = spellObj.efApps.Count - 1; num >= 0; num--)
			{
				if (spellObj.efApps[num].effect == Effect.Anchor && !spellObj.tags.Contains(Tag.NoStrafe))
				{
					spellObj.nameString += U.I.Colorify(" T", UIColor.Enhancement);
					spellObj.anchor = false;
					spellObj.efApps.RemoveAt(num);
					spellObj.enhancements.Add(enhancement);
				}
			}
			break;
		}
		case Enhancement.SummonHP:
			if (!spellObj.HasEffect(Effect.Summon) || spellObj.GetNumEnhancements(enhancement) >= 1)
			{
				break;
			}
			spellObj.nameString += U.I.Colorify(" B", UIColor.Enhancement);
			foreach (EffectApp efApp in spellObj.efApps)
			{
				if (efApp.effect == Effect.Summon)
				{
					efApp.amount *= 2f;
				}
			}
			spellObj.enhancements.Add(enhancement);
			break;
		case Enhancement.TriSlash:
			if (spellObj.spellActions.Contains("StepSlash") && spellObj.GetNumEnhancements(enhancement) < 1 && spellObj.damage >= 10f)
			{
				spellObj.nameString += U.I.Colorify(" Z", UIColor.Enhancement);
				spellObj.damage = Mathf.RoundToInt(spellObj.damage * 0.4f);
				spellObj.numShots *= 3f;
				if (spellObj.timeBetweenShots < 0.08f)
				{
					spellObj.timeBetweenShots = 0.08f;
				}
				spellObj.efApps.Add(new EffectApp(FTrigger.OnCast, 0f, 0f, Effect.Trinity, Target.Default, 1f, 0f, spellObj.itemID, 1f, 0f));
				spellObj.enhancements.Add(enhancement);
			}
			break;
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.Close();
	}
}
