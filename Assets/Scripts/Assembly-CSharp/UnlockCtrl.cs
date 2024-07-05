using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using I2.Loc;
using TMPro;
using UnityEngine;

public class UnlockCtrl : NavPanel
{
	public ExperienceBar expBar;

	private BC ctrl;

	private DeckCtrl deCtrl;

	private IdleCtrl idCtrl;

	private PostCtrl poCtrl;

	private ItemManager itemMan;

	public GameOverPane gameOverPane;

	public List<string> charUnlocks = new List<string>();

	public List<ChoiceCard> hiddenUnlocks = new List<ChoiceCard>();

	public List<ChoiceCard> shownUnlocks = new List<ChoiceCard>();

	public Transform cardGrid;

	public static bool transitioning = false;

	public int currentExp = 0;

	public int unlockExp = 4;

	public int currentUnlockLevel = 0;

	public int totalNumUnlocks;

	public CharacterCard characterCardPrefab;

	public TMP_Text levelsToAddText;

	public TMP_Text unlockDescriptionText;

	public TMP_Text unlockNoteText;

	public TMP_Text unlocksRemainingText;

	public AudioClip openSound;

	public AudioClip revealSound;

	public AudioClip levelUpSound;

	public AudioClip expSound;

	public AudioClip closeSound;

	public ExpTrail expParticlePrefab;

	public ParticleSystem particleSys;

	private bool showingExpBar = false;

	public float baseExpTickDuration = 0.1f;

	private float expTickDuration = 0.1f;

	public int totalExp = 0;

	public CanvasGroup itemGrids;

	public CanvasGroup moneyTextBattle;

	private GameMode lastGameMode = GameMode.Solo;

	private CameraScript camCtrl;

	private HeroSelectCtrl heCtrl;

	private MainCtrl mainCtrl;

	private RunCtrl runCtrl;

	private SpawnCtrl spCtrl;

	private bool closing = false;

	public int levelsToAdd = 0;

	protected override void Awake()
	{
		base.Awake();
		camCtrl = S.I.camCtrl;
		ctrl = S.I.batCtrl;
		deCtrl = S.I.deCtrl;
		heCtrl = S.I.heCtrl;
		idCtrl = S.I.idCtrl;
		mainCtrl = S.I.mainCtrl;
		poCtrl = S.I.poCtrl;
		runCtrl = S.I.runCtrl;
		spCtrl = S.I.spCtrl;
		itemMan = S.I.itemMan;
		StartCoroutine(_SetDefaultPrefs());
	}

	private IEnumerator _SetDefaultPrefs()
	{
		yield return new WaitUntil(() => SaveDataCtrl.Initialized);
		SaveDataCtrl.Set("Saffron", true);
		UpdateUnlockLevel();
		if (S.I.UNLOCK_ALL_CHARS)
		{
			UnlockAll();
		}
		else if (S.I.EDITION != Edition.Dev && S.I.EDITION != Edition.QA)
		{
			SaveDataCtrl.Set("SaffronTest", false);
		}
	}

	public void UnlockAll()
	{
		SaveDataCtrl.Set("Saffron", true);
		SaveDataCtrl.Set("SaffronAlt", true);
		SaveDataCtrl.Set("SaffronGen", true);
		SaveDataCtrl.Set("SaffronTest", true);
		SaveDataCtrl.Set("Hazel", true);
		SaveDataCtrl.Set("HazelAlt", true);
		SaveDataCtrl.Set("Gunner", true);
		SaveDataCtrl.Set("GunnerAlt", true);
		SaveDataCtrl.Set("Reva", true);
		SaveDataCtrl.Set("RevaAlt", true);
		SaveDataCtrl.Set("Shiso", true);
		SaveDataCtrl.Set("ShisoAlt", true);
		SaveDataCtrl.Set("Violette", true);
		SaveDataCtrl.Set("VioletteAlt", true);
		SaveDataCtrl.Set("Selicy", true);
		SaveDataCtrl.Set("SelicyAlt", true);
		SaveDataCtrl.Set("Terra", true);
		SaveDataCtrl.Set("TerraAlt", true);
		SaveDataCtrl.Set("Shopkeeper", true);
		SaveDataCtrl.Set("SaffronWitch", true);
		SaveDataCtrl.Set("SaffronLegacy", true);
		SaveDataCtrl.Set("SaffronLea", true);
		SaveDataCtrl.Set("GunnerBuccaneer", true);
		SaveDataCtrl.Set("ShisoCowboy", true);
		SaveDataCtrl.Set("SelicyCatgirl", true);
		SaveDataCtrl.Set("SelicyHoliday", true);
		SaveDataCtrl.Set("TerraDreadwyrm", true);
		SaveDataCtrl.Set("RevaCyber", true);
		SaveDataCtrl.Set("VioletteJRock", true);
		SaveDataCtrl.Set("HazelPriestess", true);
		SaveDataCtrl.Set("HazelQueen", false);
		SaveDataCtrl.Set("ShopcreeperLegacy", true);
		SaveDataCtrl.Set("TerraDark", true);
		SaveDataCtrl.Set("HazelPvP", true);
		SaveDataCtrl.Set("ShisoPvP", true);
		SaveDataCtrl.Set("TerraPvP", true);
		SaveDataCtrl.Set("ViolettePvP", true);
		SaveDataCtrl.Set("ShopkeeperPvP", true);
	}

	public void StartUnlocks()
	{
		StartCoroutine(_Unlocks());
	}

	public void UpdateUnlockLevel()
	{
		currentUnlockLevel = SaveDataCtrl.Get("UnlockLevel", 0);
	}

	public void PreOpen()
	{
		cardGrid.DestroyChildren();
		slideBody.Show();
		S.I.PlayOnce(openSound);
		expBar.anim.SetBool("visible", true);
		anim.SetBool("visible", true);
		unlockDescriptionText.text = "";
		unlockNoteText.text = "";
		totalNumUnlocks = itemMan.unlocks[itemMan.unlocks.Count - 1].unlockLevel;
		if (!ctrl.pvpMode)
		{
			levelsToAdd = poCtrl.playerLevel;
			if (S.I.EDITION == Edition.Dev)
			{
				levelsToAdd = 18;
			}
		}
		else
		{
			levelsToAdd = ctrl.matchesPlayedThisSession;
			ctrl.matchesPlayedThisSession = 0;
		}
		levelsToAddText.text = string.Format(ScriptLocalization.UI.TopNav_LevelShort + " {0}", levelsToAdd);
		UpdateUnlockLevel();
		if (currentUnlockLevel > 2 && !SaveDataCtrl.Get("SaffronAlt", false))
		{
			AddCharCardUnlock("SaffronAlt");
		}
		if (currentUnlockLevel > 5 && !SaveDataCtrl.Get("Reva", false))
		{
			AddCharCardUnlock("Reva");
		}
		if (currentUnlockLevel > 14 && SaveDataCtrl.Get("Hazel", false) && !SaveDataCtrl.Get("HazelQueen", false))
		{
			AddCharCardUnlock("HazelQueen", true);
		}
		currentExp = SaveDataCtrl.Get("UnlockExp", 0);
		UpdateLevelText();
		defaultButton.tmpText.text = ScriptLocalization.UI.CONTINUE;
		expBar.UpdateBar((float)currentExp * 1f / (float)unlockExp * 1f);
		gameOverPane.diamondBox.SetTrigger("next");
		itemGrids.alpha = 0f;
		moneyTextBattle.alpha = 0f;
		lastGameMode = heCtrl.gameMode;
	}

	public override void Open()
	{
		base.Open();
	}

	public override void Close()
	{
		if (!closing && ctrl.GameState != GState.Unlock)
		{
			StartCoroutine(_Close());
		}
	}

	private IEnumerator _Close()
	{
		closing = true;
		gameOverPane.diamondBox.SetTrigger("next");
		S.I.PlayOnce(closeSound);
		expBar.anim.SetBool("visible", false);
		expBar.anim.SetBool("Mid", false);
		S.I.muCtrl.PauseIntroLoop();
		foreach (ChoiceCard card in shownUnlocks)
		{
			card.anim.SetBool("OnScreen", false);
			card.anim.SetBool("back", false);
			if (!mainCtrl.startedUp)
			{
				yield return new WaitForSeconds(0.1f);
			}
		}
		anim.SetBool("visible", false);
		if (!mainCtrl.startedUp)
		{
			yield return new WaitForSeconds(0.4f);
			ctrl.Restart(false, true);
		}
		gameOverPane.diamondBox.SetBool("visible", false);
		cardGrid.DestroyChildren();
		if (!mainCtrl.startedUp)
		{
			camCtrl.CameraChangePos(0, true);
			base.transform.position = camCtrl.transform.position;
		}
		yield return new WaitForSeconds(0.1f);
		itemGrids.alpha = 1f;
		moneyTextBattle.alpha = 1f;
		if (!mainCtrl.startedUp)
		{
			_003C_003En__0();
			yield return new WaitForSeconds(0.1f);
			if (lastGameMode == GameMode.Solo || lastGameMode == GameMode.CoOp)
			{
				S.I.muCtrl.PlayTitle();
				heCtrl.gameMode = lastGameMode;
				heCtrl.Open();
			}
			else
			{
				mainCtrl.Startup(0.1f, false);
			}
		}
		closing = false;
	}

	private void Update()
	{
		if (currentExp < unlockExp)
		{
			return;
		}
		if (itemMan.GetUnlocks(currentUnlockLevel + 1).Count > 0 && hiddenUnlocks.Count < 4)
		{
			defaultButton.tmpText.text = ScriptLocalization.UI.Unlock_Reveal_Reward;
			currentExp -= unlockExp;
			List<ItemObject> unlocks = itemMan.GetUnlocks(currentUnlockLevel + 1);
			foreach (ItemObject item in unlocks)
			{
				ChoiceCard choiceCard = deCtrl.CreateNewChoiceCard(item, cardGrid);
				choiceCard.rewardType = RewardType.Unlock;
				choiceCard.parentList = hiddenUnlocks;
				choiceCard.anim.SetBool("back", true);
				choiceCard.cardInner.cardBack.alpha = 1f;
				hiddenUnlocks.Add(choiceCard);
				if (choiceCard.cardInner.voteDisplay != null)
				{
					choiceCard.cardInner.voteDisplay.gameObject.SetActive(false);
				}
			}
			particleSys.Play();
			currentUnlockLevel++;
			expBar.levelText.text = currentUnlockLevel.ToString();
			expBar.SetTargetToMax();
			SaveDataCtrl.Set("UnlockLevel", currentUnlockLevel);
			SaveDataCtrl.Write();
			S.I.PlayOnce(levelUpSound);
		}
		else
		{
			currentExp = unlockExp - 1;
		}
	}

	private IEnumerator _Unlocks()
	{
		shownUnlocks.Clear();
		Open();
		PostCtrl.transitioning = false;
		expBar.UpdateBar(currentExp / unlockExp);
		UpdateLevelText();
		totalExp = levelsToAdd;
		showingExpBar = true;
		expTickDuration = baseExpTickDuration;
		yield return new WaitForSeconds(0.2f);
		string storedCharUnlockStringData = SaveDataCtrl.Get("StoredCharIDUnlocks", "");
		if (!string.IsNullOrEmpty(storedCharUnlockStringData))
		{
			string[] storedCharUnlockStrings = storedCharUnlockStringData.Replace(" ", string.Empty).Split(',');
			SaveDataCtrl.Set("StoredCharIDUnlocks", "");
			string[] array = storedCharUnlockStrings;
			foreach (string charID in array)
			{
				if (!string.IsNullOrEmpty(charID) && AddCharCardUnlock(charID))
				{
					yield return new WaitForSeconds(0.3f);
				}
			}
		}
		string storedSkinUnlockStringData = SaveDataCtrl.Get("StoredSkinIDUnlocks", "");
		if (!string.IsNullOrEmpty(storedSkinUnlockStringData))
		{
			string[] storedSkinUnlockStrings = storedSkinUnlockStringData.Replace(" ", string.Empty).Split(',');
			SaveDataCtrl.Set("StoredSkinIDUnlocks", "");
			string[] array2 = storedSkinUnlockStrings;
			foreach (string charID4 in array2)
			{
				if (!string.IsNullOrEmpty(charID4) && AddCharCardUnlock(charID4, true))
				{
					yield return new WaitForSeconds(0.3f);
				}
			}
		}
		foreach (string charID3 in runCtrl.currentRun.charUnlocks)
		{
			if (!string.IsNullOrEmpty(charID3) && AddCharCardUnlock(charID3))
			{
				yield return new WaitForSeconds(0.3f);
			}
		}
		foreach (string charID2 in runCtrl.currentRun.skinUnlocks)
		{
			if (!string.IsNullOrEmpty(charID2) && AddCharCardUnlock(charID2, true))
			{
				yield return new WaitForSeconds(0.3f);
			}
		}
		if (currentUnlockLevel < totalNumUnlocks)
		{
			for (int i = 0; i < levelsToAdd; i++)
			{
				ExpTrail newExpParticle = SimplePool.Spawn(expParticlePrefab.gameObject, levelsToAddText.transform.position, base.transform.rotation, base.transform).GetComponent<ExpTrail>();
				newExpParticle.target = expBar.levelText.transform;
				newExpParticle.Set(expBar.levelText.transform, this);
				levelsToAddText.text = string.Format("{0} {1}", ScriptLocalization.UI.TopNav_LevelShort, levelsToAdd - 1 - i);
				if (expBar.targetFill != 1f)
				{
					UpdateLevelText();
				}
				yield return new WaitForSeconds(expTickDuration);
			}
		}
		else
		{
			totalExp = 0;
			expBar.UpdateBar(1f);
		}
		while (totalExp > 0)
		{
			yield return new WaitForSeconds(expTickDuration);
		}
		SaveDataCtrl.Set("UnlockExp", currentExp);
		expBar.anim.SetBool("Mid", true);
		yield return new WaitForSeconds(0.2f);
		yield return new WaitForSeconds(0.1f);
		showingExpBar = false;
	}

	public void AddExp()
	{
		S.I.PlayOnce(expSound);
		if (currentExp == 0)
		{
			expBar.EmptyBar();
		}
		currentExp++;
		totalExp--;
		UpdateLevelText();
	}

	private void UpdateLevelText()
	{
		expBar.UpdateBar((float)currentExp * 1f / (float)unlockExp * 1f);
		if (currentUnlockLevel >= totalNumUnlocks)
		{
			expBar.levelText.text = currentUnlockLevel + "/" + currentUnlockLevel;
		}
		else
		{
			expBar.levelText.text = currentUnlockLevel.ToString();
		}
		unlocksRemainingText.text = string.Format(ScriptLocalization.UI.Unlock_Unlocks_Remaining + " {0}", Mathf.Clamp(totalNumUnlocks - currentUnlockLevel - 1, 0, totalNumUnlocks));
	}

	private bool AddCharCardUnlock(string charID, bool skin = false)
	{
		if (string.IsNullOrEmpty(charID))
		{
			return false;
		}
		if (hiddenUnlocks.Count >= 5)
		{
			if (!skin)
			{
				string text = SaveDataCtrl.Get("StoredCharIDUnlocks", "");
				text = ((!string.IsNullOrEmpty(text)) ? (text + ", " + charID) : charID);
				SaveDataCtrl.Set("StoredCharIDUnlocks", text);
			}
			else
			{
				string text2 = SaveDataCtrl.Get("StoredSkinIDUnlocks", "");
				text2 = ((!string.IsNullOrEmpty(text2)) ? (text2 + ", " + charID) : charID);
				SaveDataCtrl.Set("StoredSkinIDUnlocks", text2);
			}
			return false;
		}
		string text3 = charID;
		if (skin && charID.Contains("Skin"))
		{
			List<string> list = new List<string>(charID.Replace("Skin", ",").Split(','));
			string key = list[0];
			if (list.Count < 2)
			{
				return false;
			}
			int num = int.Parse(list[1]);
			if (!spCtrl.heroDictionary.ContainsKey(key))
			{
				return false;
			}
			BeingObject beingObject = spCtrl.heroDictionary[key];
			if (beingObject.allAnims.Count <= num)
			{
				return false;
			}
			text3 = beingObject.allAnims[num];
		}
		if (SaveDataCtrl.Get(charID, false) || SaveDataCtrl.Get(text3, false) || !spCtrl.heroDictionary.ContainsKey(text3))
		{
			return false;
		}
		foreach (CharacterCard hiddenUnlock in hiddenUnlocks)
		{
			if (hiddenUnlock.beingObj.beingID == text3)
			{
				return false;
			}
		}
		CharacterCard characterCard2 = Object.Instantiate(characterCardPrefab);
		characterCard2.transform.SetParent(cardGrid);
		characterCard2.beingObj = spCtrl.heroDictionary[text3];
		characterCard2.charAnim.runtimeAnimatorController = itemMan.GetAnim(characterCard2.beingObj.animName);
		characterCard2.charRend.color = Color.clear;
		UnityEngine.Debug.Log("Unlocking " + text3);
		SaveDataCtrl.Set(text3, true);
		SaveDataCtrl.Set(characterCard2.beingObj.nameString + "PvP", true);
		characterCard2.cardInner.nameText.text = characterCard2.beingObj.localizedName;
		characterCard2.cardInner.flavorText.text = characterCard2.beingObj.description;
		characterCard2.deCtrl = deCtrl;
		characterCard2.parentList = hiddenUnlocks;
		characterCard2.rewardType = RewardType.Unlock;
		characterCard2.anim.SetBool("back", true);
		characterCard2.cardInner.cardBack.alpha = 1f;
		if (charID.Contains("Alt") || charID.Contains("Gen") || skin)
		{
			characterCard2.alternate = true;
			TMP_Text nameText = characterCard2.cardInner.nameText;
			nameText.text = nameText.text + " - " + characterCard2.beingObj.title;
			characterCard2.cardInner.flavorText.text = characterCard2.beingObj.flavor;
		}
		hiddenUnlocks.Add(characterCard2);
		S.I.PlayOnce(levelUpSound);
		return true;
	}

	public void Continue()
	{
		if (hiddenUnlocks.Count > 0 && totalExp <= 0)
		{
			ShowNextUnlock();
		}
		else if (!showingExpBar)
		{
			ctrl.GameState = GState.MainMenu;
			Close();
			SaveDataCtrl.Write();
		}
		else
		{
			expTickDuration = 0.03f;
		}
	}

	public void ShowNextUnlock(int i = 0)
	{
		if (i >= hiddenUnlocks.Count)
		{
			i = 0;
		}
		if (hiddenUnlocks.Count > 0)
		{
			hiddenUnlocks[i].anim.SetBool("OnScreen", true);
			hiddenUnlocks[i].cardInner.cardBack.alpha = 0f;
			if (hiddenUnlocks[i].itemObj == null)
			{
				CharacterCard component = hiddenUnlocks[i].GetComponent<CharacterCard>();
				component.charRend.color = Color.white;
				if (component.beingObj.tags.Contains(Tag.Skin))
				{
					unlockDescriptionText.text = ScriptLocalization.UI.Unlock_New_Outfit;
				}
				else if (component.alternate)
				{
					unlockDescriptionText.text = ScriptLocalization.UI.Unlock_New_Starting_Kit;
				}
				else
				{
					unlockDescriptionText.text = ScriptLocalization.UI.Unlock_New_Character;
				}
			}
			else if (hiddenUnlocks[i].itemObj.type == ItemType.Spell)
			{
				unlockDescriptionText.text = ScriptLocalization.UI.Unlock_New_Spell;
			}
			else if (hiddenUnlocks[i].itemObj.type == ItemType.Art)
			{
				unlockDescriptionText.text = ScriptLocalization.UI.Unlock_New_Artifact;
			}
			shownUnlocks.Add(hiddenUnlocks[i]);
			hiddenUnlocks.RemoveAt(i);
			S.I.PlayOnce(revealSound);
			unlockNoteText.text = ScriptLocalization.UI.Unlock_Unlocked_For_Future;
		}
		if (hiddenUnlocks.Count <= 0)
		{
			defaultButton.tmpText.text = ScriptLocalization.UI.CONTINUE;
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.Close();
	}
}
