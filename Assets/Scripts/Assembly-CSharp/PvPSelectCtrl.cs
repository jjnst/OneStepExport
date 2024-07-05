using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Rewired;
using UnityEngine;

public class PvPSelectCtrl : NavPanel
{
	public Transform heroGrid;

	public List<BeingObject> currentDisplayedHeros;

	public List<HeroDisplay> heroDisplays;

	public HeroItemListCard startingItemPrefab;

	public List<HeroItemListCard> itemCards = new List<HeroItemListCard>();

	public List<HeroSplash> heroSplashes;

	public List<Animator> heroAnimators;

	public List<SpriteRenderer> heroSprites;

	public List<PvPHeroButton> heroButtons;

	public List<PvPHeroButton> unlockedHeroButtons;

	public List<int> lastChosenHeroIndexes;

	public PvPHeroButton[] focusedHeroButtons = new PvPHeroButton[2];

	public PvPHeroButton heroBtnPrefab;

	public AudioClip startSound;

	public NavButton startButton;

	public List<GameObject> allHeroes;

	public List<Sprite> heroSplashSprites;

	public FlexSelector heroGridPlayerOneLeftSelector;

	public FlexSelector heroGridPlayerTwoRightSelector;

	public float offsetAdd = 20f;

	public float[] selectorOffsetAdd = new float[2];

	public List<CanvasGroup> changeSkinButtons;

	public List<CanvasGroup> changeSkinLocks;

	public float selectorGap = 6f;

	public bool hideSelectors = true;

	public Canvas canvas;

	public BC ctrl;

	public CameraScript camCtrl;

	public DeckCtrl deCtrl;

	public HeroSelectCtrl heCtrl;

	public ItemManager itemMan;

	private MainCtrl mainCtrl;

	private OptionCtrl optCtrl;

	public RunCtrl runCtrl;

	public SpawnCtrl spCtrl;

	private bool campaignStarted = false;

	public bool tournamentMode = false;

	protected override void Awake()
	{
		base.Awake();
		ctrl = S.I.batCtrl;
		camCtrl = S.I.camCtrl;
		deCtrl = S.I.deCtrl;
		itemMan = S.I.itemMan;
		mainCtrl = S.I.mainCtrl;
		optCtrl = S.I.optCtrl;
		runCtrl = S.I.runCtrl;
		spCtrl = S.I.spCtrl;
		foreach (Sprite heroSplashSprite in heroSplashSprites)
		{
			itemMan.sprites[heroSplashSprite.name] = heroSplashSprite;
		}
	}

	private void Update()
	{
		if (btnCtrl.IsActivePanel(this))
		{
			if ((bool)btnCtrl.focusedButton)
			{
				heroGridPlayerOneLeftSelector.rightTarget = focusedHeroButtons[0].transform;
				heroGridPlayerOneLeftSelector.offset = focusedHeroButtons[0].transform.position.x + (focusedHeroButtons[0].tmpText.textBounds.size.x / 2f + selectorGap + selectorOffsetAdd[0]);
			}
			if ((bool)btnCtrl.playerTwoFocusedButton)
			{
				heroGridPlayerTwoRightSelector.leftTarget = focusedHeroButtons[1].transform;
				heroGridPlayerTwoRightSelector.offset = focusedHeroButtons[1].transform.position.x + (focusedHeroButtons[1].tmpText.textBounds.size.x / 2f + selectorGap + selectorOffsetAdd[1]);
			}
		}
	}

	public void EnableTournamentMode()
	{
		if (!tournamentMode)
		{
			tournamentMode = true;
			CreateHeroButtons();
			btnCtrl.SetFocus(heroButtons[0]);
			btnCtrl.SetFocus(heroButtons[0], 1);
		}
	}

	public override void Open()
	{
		base.Open();
		optCtrl.quitButtonEnabled = true;
		CreateHeroButtons();
		for (int i = 0; i < 2; i++)
		{
			btnCtrl.SetFocus(heroButtons[Mathf.Clamp(lastChosenHeroIndexes[i], 0, heroButtons.Count - 1)], i);
		}
		S.I.muCtrl.PlayPvPSelect();
		runCtrl.currentHellPassNum = 0;
		for (int j = 0; j < selectorOffsetAdd.Length; j++)
		{
			selectorOffsetAdd[j] = offsetAdd;
			heroAnimators[j].SetTrigger("spawn");
		}
	}

	public void DisplayHero(int playerIndex, BeingObject thisHero, PvPHeroButton focusedHeroButton, bool alt = false, bool heroButton = false)
	{
		HeroDisplay heroDisplay = heroDisplays[playerIndex];
		currentDisplayedHeros[playerIndex] = thisHero;
		focusedHeroButton.heroObjs[playerIndex] = thisHero;
		if (S.modsInstalled)
		{
			foreach (BeingObject value in ctrl.sp.beingDictionary.Values)
			{
				if (value.nameString == thisHero.nameString && value.tags.Contains(Tag.Skin) && !thisHero.allAnims.Contains(value.beingID))
				{
					thisHero.allAnims.Add(value.beingID);
					thisHero.unlockedAnims.Add(value.beingID);
				}
			}
		}
		SetHeroAnimator(thisHero.animName, playerIndex);
		focusedHeroButton.selected = true;
		heroDisplay.spellsGrid.DestroyChildren();
		itemCards.Clear();
		heroDisplay.SetHeroItems(thisHero, ref itemCards, deCtrl, null, this);
		StartCoroutine(ShowListCards(heroDisplay.spellsGrid));
		StartCoroutine(ShowListCards(heroDisplay.artsGrid));
		StartCoroutine(ShowListCards(heroDisplay.wepGrid));
		if (heroButton)
		{
			heroDisplay.heroDescription.text = "";
		}
		heroSplashes[playerIndex].ChangeSplash(thisHero);
		ShowOutfitButton(playerIndex, thisHero);
	}

	public void ShowOutfitButton(int playerIndex, BeingObject heroObj)
	{
		if (heroObj.allAnims.Count > 1)
		{
			changeSkinButtons[playerIndex].alpha = 1f;
		}
		else
		{
			changeSkinButtons[playerIndex].alpha = 0.8f;
		}
	}

	public void UpdateSplash(int playerNum)
	{
		if (ctrl.sp.heroDictionary.ContainsKey(currentDisplayedHeros[playerNum].animName))
		{
			if (ctrl.sp.beingDictionary[currentDisplayedHeros[playerNum].animName].splashSprite != null)
			{
				if (ctrl.sp.beingDictionary[currentDisplayedHeros[playerNum].animName].splashSprite != heroSplashes[playerNum].shownHeroSplash)
				{
					heroSplashes[playerNum].ChangeSplash(ctrl.sp.beingDictionary[currentDisplayedHeros[playerNum].animName]);
				}
			}
			else if (currentDisplayedHeros[playerNum].splashSprite != heroSplashes[playerNum].shownHeroSplash)
			{
				heroSplashes[playerNum].ChangeSplash(currentDisplayedHeros[playerNum]);
			}
		}
		else if (currentDisplayedHeros[playerNum].splashSprite != heroSplashes[playerNum].shownHeroSplash)
		{
			heroSplashes[playerNum].ChangeSplash(currentDisplayedHeros[playerNum]);
		}
	}

	private IEnumerator ShowListCards(Transform grid)
	{
		yield return new WaitForEndOfFrame();
		foreach (Transform card in grid)
		{
			card.GetComponent<Animator>().SetBool("spawned", true);
			yield return new WaitForSeconds(0.1f);
		}
	}

	public void CreateHeroButtons()
	{
		heroGrid.DestroyChildren();
		heroButtons.Clear();
		unlockedHeroButtons.Clear();
		for (int i = 0; i < spCtrl.heroPvPList.Count; i++)
		{
			PvPHeroButton component = Object.Instantiate(heroBtnPrefab, base.transform.position + Vector3.back, base.transform.rotation).GetComponent<PvPHeroButton>();
			component.Set(this, spCtrl.heroPvPList[i]);
			heroButtons.Add(component);
		}
		StartCoroutine(ShowHeroButtons(heroGrid));
		DisplayHero(0, heroButtons[0].heroObjs[0], heroButtons[0], false, true);
		DisplayHero(1, heroButtons[0].heroObjs[1], heroButtons[0], false, true);
	}

	private IEnumerator ShowHeroButtons(Transform grid)
	{
		yield return new WaitForEndOfFrame();
		foreach (Transform card in grid)
		{
			card.GetComponent<Animator>().SetBool("visible", true);
			yield return new WaitForSeconds(0.05f);
		}
	}

	private IEnumerator HideHeroButtons()
	{
		yield return new WaitForEndOfFrame();
		foreach (PvPHeroButton heroButton in heroButtons)
		{
			if (!focusedHeroButtons.Contains(heroButton))
			{
				heroButton.GetComponent<Animator>().SetBool("visible", false);
				yield return new WaitForSeconds(0.05f);
			}
		}
	}

	public void SetHeroAnimator(string key, int playerNum = -1)
	{
		if (playerNum == -1)
		{
			playerNum = btnCtrl.lastInputPlayerIndex;
		}
		Animator animator = heroAnimators[playerNum];
		animator.runtimeAnimatorController = itemMan.GetAnim(key);
		if (itemMan.animations.ContainsKey(key))
		{
			animator.GetComponent<AnimationOverrider>().enabled = false;
			animator.GetComponent<SpriteAnimator>().enabled = false;
			animator.runtimeAnimatorController = itemMan.GetAnim(key);
		}
		else
		{
			animator.GetComponent<AnimationOverrider>().enabled = true;
			animator.GetComponent<AnimationOverrider>().Set(animator.GetComponent<SpriteAnimator>(), animator, key, itemMan);
			animator.runtimeAnimatorController = ctrl.baseCharacterAnim;
		}
		UpdateSplash(playerNum);
		SetSkinColor(playerNum);
	}

	private void SetSkinColor(int playerNum)
	{
		if (!currentDisplayedHeros[playerNum].unlockedAnims.Contains(currentDisplayedHeros[playerNum].animName))
		{
			if (heroSprites[playerNum].material.GetColor("_TintRGBA_Color_1") != Color.black)
			{
				heroSprites[playerNum].material.SetColor("_TintRGBA_Color_1", Color.black);
				heroSprites[playerNum].material.SetFloat("_SpriteFade", ctrl.heCtrl.lockedSpriteTransparency);
				changeSkinLocks[playerNum].alpha = 1f;
			}
		}
		else if (heroSprites[playerNum].material.GetColor("_TintRGBA_Color_1") != Color.clear)
		{
			heroSprites[playerNum].material.SetColor("_TintRGBA_Color_1", Color.clear);
			heroSprites[playerNum].material.SetFloat("_SpriteFade", 1f);
			changeSkinLocks[playerNum].alpha = 0f;
		}
	}

	public bool CurrentHeroSkinIsUnlocked(int playerNum)
	{
		return currentDisplayedHeros[playerNum].unlockedAnims.Contains(currentDisplayedHeros[playerNum].animName);
	}

	public void FlashSkinError(int playerNum)
	{
		StartCoroutine(c_FlashSkinError(playerNum));
	}

	private IEnumerator c_FlashSkinError(int playerNum)
	{
		if (heroSprites[playerNum].material.GetColor("_TintRGBA_Color_1") != ctrl.heCtrl.errorSpriteColor)
		{
			heroSprites[playerNum].material.SetColor("_TintRGBA_Color_1", ctrl.heCtrl.errorSpriteColor);
			heroSprites[playerNum].material.SetFloat("_SpriteFade", ctrl.heCtrl.lockedSpriteTransparency);
		}
		S.I.PlayOnce(btnCtrl.lockedSound);
		yield return new WaitForSeconds(0.2f);
		SetSkinColor(playerNum);
	}

	public void StartCampaign()
	{
		bool flag = true;
		for (int i = 0; i < currentDisplayedHeros.Count; i++)
		{
			if (!currentDisplayedHeros[i].unlockedAnims.Contains(currentDisplayedHeros[i].animName))
			{
				flag = false;
				FlashSkinError(i);
			}
		}
		if (flag)
		{
			StartCoroutine(_StartCampaign());
		}
	}

	private IEnumerator _StartCampaign()
	{
		if (!campaignStarted && !(btnCtrl.focusedButton != startButton) && !(btnCtrl.playerTwoFocusedButton != startButton))
		{
			optCtrl.quitButtonEnabled = false;
			mainCtrl.transitioning = true;
			PostCtrl.transitioning = true;
			btnCtrl.RemoveFocus();
			campaignStarted = true;
			anim.SetBool("visible", false);
			S.I.PlayOnce(startSound);
			camCtrl.TransitionInHigh("LeftWipe");
			StartCoroutine(HideHeroButtons());
			yield return new WaitForSeconds(0.8f);
			ctrl.GameState = GState.PvPSetup;
			btnCtrl.RemoveFocus();
			runCtrl.StartPvP();
			yield return new WaitForSeconds(0.9f);
			mainCtrl.transitioning = false;
			campaignStarted = false;
			_003C_003En__0();
			mainCtrl.ForceClose();
		}
	}

	public override void Close()
	{
		if (campaignStarted)
		{
			return;
		}
		base.Close();
		if (btnCtrl.activeNavPanels.Contains(mainCtrl))
		{
			return;
		}
		Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
		if (rewiredPlayer != null)
		{
			rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Gameplay");
			rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay");
			rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay2");
		}
		HelpIconManager.SetKeyboardAvailable(0, true);
		S.I.heCtrl.gameMode = GameMode.Solo;
		foreach (HeroSplash heroSplash in heroSplashes)
		{
			heroSplash.Hide();
		}
		S.I.muCtrl.TransitionTo(S.I.muCtrl.titleTrack);
		camCtrl.cameraPane.color = S.I.GetFlashColor(UIColor.BlueDark);
		S.I.CameraStill(UIColor.Clear);
		mainCtrl.Open();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.Close();
	}
}
