using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class HeroSelectCtrl : NavPanel
{
	public TMP_Text heroName;

	public TMP_Text heroDescription;

	public TMP_Text heroFlavor;

	public Transform heroGrid;

	public BeingObject currentDisplayedHero;

	public TMP_InputField seedInput;

	public HeroDisplay heroDisplay;

	public HeroItemListCard startingItemPrefab;

	public List<HeroItemListCard> itemCards = new List<HeroItemListCard>();

	public AudioClip startSound;

	public NavButton startButton;

	public HeroButton heroBtnPrefab;

	public HeroSplash heroSplash;

	public Animator heroAnimator;

	public SpriteRenderer heroSprite;

	public List<GameObject> allHeroes;

	public List<HeroButton> heroButtons;

	public List<HeroButton> unlockedHeroButtons;

	public HeroAltButton heroAltButtonPrefab;

	public List<HeroAltButton> unlockedHeroAltButtons;

	public RectTransform heroAltButtonGrid;

	public List<Sprite> heroSplashSprites;

	public FlexSelector heroGridLeftSelector;

	public FlexSelector heroGridRightSelector;

	public FlexSelector heroGridVerticalSelector;

	public FlexSelector altGridLeftSelector;

	public FlexSelector altGridRightSelector;

	public FlexSelector hellButtonRightSelector;

	public FlexSelector hellButtonVerticalSelector;

	public FlexSelector startButtonLeftSelector;

	public CanvasGroup detailsButton;

	public CanvasGroup changeSkinButton;

	public CanvasGroup changeSkinLock;

	public CanvasGroup previewButton;

	public Color errorSpriteColor;

	public float lockedSpriteTransparency = 0.7f;

	public HeroButton focusedHeroButton;

	public UIButton focusedAltButton;

	public HellPassButton hellPassButton;

	public float selectorGap = 6f;

	public bool hideSelectors = true;

	public bool multiplayer = false;

	public GameMode gameMode;

	public Canvas canvas;

	public BC ctrl;

	public CameraScript camCtrl;

	public DeckCtrl deCtrl;

	public ItemManager itemMan;

	private MainCtrl mainCtrl;

	private OptionCtrl optCtrl;

	public RunCtrl runCtrl;

	public SpawnCtrl spCtrl;

	private int frameAltButtonsWereCreated = 0;

	public bool campaignStarted = false;

	public int lastHeroChosenIndex = 0;

	public string lastSkinName;

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
		if (!btnCtrl.IsActivePanel(this) || !btnCtrl.focusedButton)
		{
			return;
		}
		if (btnCtrl.focusedButton.transform.IsChildOf(heroGrid))
		{
			UpdateHeroSelectors();
		}
		else if (btnCtrl.focusedButton.transform.IsChildOf(heroAltButtonGrid))
		{
			UpdateAltSelectors(false);
		}
		else if (btnCtrl.focusedButton == hellPassButton)
		{
			UpdateHellPassSelectors(false);
		}
		else if (btnCtrl.focusedButton == startButton)
		{
			if (string.IsNullOrEmpty(hellPassButton.description.text) || !altGridRightSelector.fill)
			{
				hellPassButton.SetLinePath(null);
				UpdateHellPassSelectors(true);
				focusedAltButton.back = focusedHeroButton;
				btnCtrl.SetFocus(hellPassButton);
				hellPassButton.back = focusedAltButton;
				btnCtrl.SetFocus(startButton);
				startButton.back = hellPassButton;
			}
			hellButtonRightSelector.leftTarget = hellPassButton.transform;
			hellButtonRightSelector.offset = hellPassButton.rect.sizeDelta.x / 2f * hellPassButton.rect.localScale.x + selectorGap / 2f;
			hellButtonRightSelector.width = 130f + hellButtonRightSelector.offset;
			if (!hellButtonRightSelector.fill)
			{
				hellButtonRightSelector.Fill();
			}
			hellButtonVerticalSelector.startTarget = hellButtonRightSelector.botRightPoint.transform;
			hellButtonVerticalSelector.endTarget = startButtonLeftSelector.topLeftPoint.transform;
			startButtonLeftSelector.rightTarget = startButton.transform;
			startButtonLeftSelector.offset = startButton.tmpText.textBounds.size.x / 2f + selectorGap;
			startButtonLeftSelector.width = -1f * (hellButtonRightSelector.botRightPoint.transform.position.x - startButton.transform.position.x + startButtonLeftSelector.offset);
		}
		if ((bool)btnCtrl.focusedButton.GetComponent<HeroItemListCard>())
		{
			if (btnCtrl.focusedButton.GetComponent<HeroItemListCard>().itemObj.type != ItemType.Art)
			{
				previewButton.alpha = 1f;
			}
			else
			{
				previewButton.alpha = 0.1f;
			}
		}
		else if (previewButton.alpha != 0.1f)
		{
			previewButton.alpha = 0.1f;
		}
	}

	public void UpdateHeroSelectors()
	{
		heroGridLeftSelector.rightTarget = focusedHeroButton.transform;
		heroGridLeftSelector.offset = focusedHeroButton.rect.sizeDelta.x / 2f * focusedHeroButton.rect.localScale.x + selectorGap - 2f;
		if (altGridLeftSelector.fill)
		{
			altGridLeftSelector.Empty();
		}
		if (altGridRightSelector.fill)
		{
			altGridRightSelector.Empty();
		}
		if (startButtonLeftSelector.fill)
		{
			startButtonLeftSelector.Empty();
		}
	}

	public void UpdateAltSelectors(bool instant)
	{
		if (!(focusedAltButton == null) && !(focusedAltButton.transform.parent != heroAltButtonGrid) && frameAltButtonsWereCreated != Time.frameCount)
		{
			if (startButtonLeftSelector.fill)
			{
				startButtonLeftSelector.Empty();
			}
			heroGridLeftSelector.offset = focusedHeroButton.rect.sizeDelta.x / 2f * focusedHeroButton.rect.localScale.x + selectorGap - 2f;
			heroGridRightSelector.leftTarget = focusedHeroButton.transform;
			heroGridRightSelector.offset = focusedHeroButton.tmpText.textBounds.size.x - focusedHeroButton.rect.sizeDelta.x + selectorGap + 28f;
			heroGridRightSelector.Fill();
			if (instant)
			{
				heroGridRightSelector.UpdateFlex();
			}
			heroGridVerticalSelector.startTarget = heroGridRightSelector.botRightPoint.transform;
			heroGridVerticalSelector.endTarget = altGridLeftSelector.topLeftPoint.transform;
			altGridLeftSelector.rightTarget = focusedAltButton.transform;
			altGridLeftSelector.offset = focusedAltButton.tmpText.textBounds.size.x / 2f + selectorGap;
			heroGridRightSelector.width = heroAltButtonGrid.transform.position.x - heroGridRightSelector.transform.position.x - 35f;
			altGridLeftSelector.width = heroAltButtonGrid.transform.position.x - heroGridRightSelector.transform.position.x - heroGridRightSelector.width - altGridLeftSelector.offset + 1f;
			altGridRightSelector.Empty();
		}
	}

	public void UpdateHellPassSelectors(bool instant)
	{
		if (!heroGridRightSelector.fill)
		{
			heroGridRightSelector.Fill();
		}
		altGridRightSelector.leftTarget = focusedAltButton.transform;
		altGridRightSelector.offset = focusedAltButton.tmpText.textBounds.size.x / 2f * focusedAltButton.rect.localScale.x + selectorGap;
		altGridRightSelector.width = 40f + altGridRightSelector.offset * hellPassButton.rect.localScale.x - focusedAltButton.tmpText.textBounds.size.x / 2f;
		if (!altGridRightSelector.fill)
		{
			altGridRightSelector.Fill();
		}
		if (instant)
		{
			altGridRightSelector.UpdateFlex();
		}
		hellPassButton.transform.position = altGridRightSelector.botRightPoint.position + Vector3.right * (selectorGap / 2f + hellPassButton.rect.sizeDelta.x / 2f * hellPassButton.rect.localScale.x + (float)hellPassButton.ExtraTriangleOffset());
		if (startButtonLeftSelector.fill)
		{
			startButtonLeftSelector.Empty();
		}
	}

	public override void Open()
	{
		ctrl.GameState = GState.HeroSelect;
		optCtrl.quitButtonEnabled = true;
		spCtrl.CreateHeroObjects();
		base.Open();
		hellPassButton.Hide();
		CreateHeroButtons();
		startButton.UnFocus();
		lastHeroChosenIndex = SaveDataCtrl.Get("LastHeroChosenIndex", 0);
		if (lastHeroChosenIndex >= heroButtons.Count)
		{
			lastHeroChosenIndex = 0;
		}
		btnCtrl.SetFocus(heroButtons[lastHeroChosenIndex]);
		DisplayHero(heroButtons[lastHeroChosenIndex].heroObj, heroButtons[lastHeroChosenIndex]);
		detailsButton.alpha = 0.1f;
		heroGridRightSelector.Reset();
		altGridLeftSelector.Reset();
		altGridRightSelector.Reset();
		startButtonLeftSelector.Reset();
	}

	public void CreateHeroButtons()
	{
		heroGrid.DestroyChildren();
		heroButtons.Clear();
		unlockedHeroButtons.Clear();
		for (int i = 0; i < spCtrl.heroCampaignList.Count; i++)
		{
			HeroButton component = Object.Instantiate(heroBtnPrefab, base.transform.position + Vector3.back, base.transform.rotation).GetComponent<HeroButton>();
			component.Set(this, spCtrl.heroCampaignList[i]);
			heroButtons.Add(component);
		}
	}

	public void SetHeroAnimator(string key)
	{
		heroAnimator.runtimeAnimatorController = itemMan.GetAnim(key);
		UpdateSplash();
		if (itemMan.animations.ContainsKey(key))
		{
			heroAnimator.GetComponent<AnimationOverrider>().enabled = false;
			heroAnimator.GetComponent<SpriteAnimator>().enabled = false;
			heroAnimator.runtimeAnimatorController = itemMan.GetAnim(key);
		}
		else
		{
			heroAnimator.GetComponent<AnimationOverrider>().enabled = true;
			heroAnimator.GetComponent<AnimationOverrider>().Set(heroAnimator.GetComponent<SpriteAnimator>(), heroAnimator, key, itemMan);
			heroAnimator.runtimeAnimatorController = ctrl.baseCharacterAnim;
		}
		SetSkinColor();
	}

	public bool CurrentHeroSkinIsUnlocked()
	{
		if (currentDisplayedHero != null)
		{
			return currentDisplayedHero.unlockedAnims.Contains(currentDisplayedHero.animName);
		}
		return false;
	}

	public void DisplayHero(BeingObject thisHero, HeroButton focusedHeroButton, bool alt = false, bool heroButton = false)
	{
		if (heroButton)
		{
			CreateHeroAltButtons(thisHero, focusedHeroButton);
		}
		currentDisplayedHero = thisHero;
		foreach (HeroButton heroButton2 in heroButtons)
		{
			heroButton2.selected = false;
		}
		focusedHeroButton.heroObj = thisHero;
		focusedHeroButton.alt = alt;
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
		SetHeroAnimator(thisHero.animName);
		focusedHeroButton.selected = true;
		heroDisplay.spellsGrid.DestroyChildren();
		itemCards.Clear();
		heroDisplay.SetHeroItems(thisHero, ref itemCards, deCtrl, this);
		StartCoroutine(ShowListCards(heroDisplay.spellsGrid));
		StartCoroutine(ShowListCards(heroDisplay.artsGrid));
		StartCoroutine(ShowListCards(heroDisplay.wepGrid));
		heroName.text = thisHero.localizedName;
		if (heroButton)
		{
			if (alt || thisHero.beingID != thisHero.nameString)
			{
				if (spCtrl.heroDictionary.ContainsKey(thisHero.nameString))
				{
					heroDescription.text = spCtrl.heroDictionary[thisHero.nameString].description;
				}
			}
			else
			{
				heroDescription.text = thisHero.description;
			}
		}
		heroFlavor.text = thisHero.flavor;
		optCtrl.heroSplash.sprite = itemMan.GetSprite(thisHero.splashSprite);
		ShowOutfitButton(thisHero);
	}

	public void UpdateSplash()
	{
		if (ctrl.sp.heroDictionary.ContainsKey(currentDisplayedHero.animName))
		{
			if (ctrl.sp.beingDictionary[currentDisplayedHero.animName].splashSprite != null)
			{
				if (ctrl.sp.beingDictionary[currentDisplayedHero.animName].splashSprite != heroSplash.shownHeroSplash)
				{
					heroSplash.ChangeSplash(ctrl.sp.beingDictionary[currentDisplayedHero.animName]);
				}
			}
			else if (currentDisplayedHero.splashSprite != heroSplash.shownHeroSplash)
			{
				heroSplash.ChangeSplash(currentDisplayedHero);
			}
		}
		else if (currentDisplayedHero.splashSprite != heroSplash.shownHeroSplash)
		{
			heroSplash.ChangeSplash(currentDisplayedHero);
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

	private void SetSkinColor()
	{
		if (!currentDisplayedHero.unlockedAnims.Contains(currentDisplayedHero.animName))
		{
			if (heroSprite.material.GetColor("_TintRGBA_Color_1") != Color.black)
			{
				heroSprite.material.SetColor("_TintRGBA_Color_1", Color.black);
				heroSprite.material.SetFloat("_SpriteFade", lockedSpriteTransparency);
				changeSkinLock.alpha = 1f;
			}
		}
		else if (heroSprite.material.GetColor("_TintRGBA_Color_1") != Color.clear)
		{
			heroSprite.material.SetColor("_TintRGBA_Color_1", Color.clear);
			heroSprite.material.SetFloat("_SpriteFade", 1f);
			changeSkinLock.alpha = 0f;
		}
	}

	public void FlashSkinError()
	{
		StartCoroutine(c_FlashSkinError());
	}

	private IEnumerator c_FlashSkinError()
	{
		if (heroSprite.material.GetColor("_TintRGBA_Color_1") != errorSpriteColor)
		{
			heroSprite.material.SetColor("_TintRGBA_Color_1", errorSpriteColor);
			heroSprite.material.SetFloat("_SpriteFade", lockedSpriteTransparency);
		}
		S.I.PlayOnce(btnCtrl.lockedSound);
		yield return new WaitForSeconds(0.2f);
		SetSkinColor();
	}

	public void CreateHeroAltButtons(BeingObject thisHero, HeroButton theHeroButton)
	{
		unlockedHeroAltButtons.Clear();
		heroAltButtonGrid.DestroyChildren();
		heroAltButtonGrid.DetachChildren();
		frameAltButtonsWereCreated = Time.frameCount;
		List<BeingObject> list = HeroAltList(thisHero.nameString);
		for (int i = 0; i < list.Count; i++)
		{
			HeroAltButton heroAltButton = Object.Instantiate(heroAltButtonPrefab, base.transform.position, base.transform.rotation, heroAltButtonGrid);
			heroAltButton.Set(theHeroButton, list[i], this);
		}
	}

	public List<BeingObject> HeroAltList(string heroNameString)
	{
		List<BeingObject> source = spCtrl.heroDictionary.Values.Where((BeingObject t) => t.nameString == heroNameString && t.tags.Contains(Tag.Campaign)).ToList();
		return source.OrderBy((BeingObject t) => t.beingID).ToList();
	}

	public void FocusAltButtons(HeroButton clickedButton, int lastAltIndex)
	{
		focusedHeroButton = clickedButton;
		if (lastAltIndex >= heroAltButtonGrid.childCount)
		{
			lastAltIndex = 0;
		}
		btnCtrl.SetFocus(heroAltButtonGrid.GetChild(lastAltIndex).GetComponent<UIButton>());
	}

	public void FocusHellPassButton(UIButton altButton)
	{
		hellPassButton.back = altButton;
		hellPassButton.listCard.anim.SetBool("spawned", true);
		btnCtrl.SetFocus(hellPassButton);
	}

	public void FocusStartButton()
	{
		btnCtrl.SetFocus(startButton);
	}

	public void StartCampaign(bool loadRun)
	{
		if (btnCtrl.focusedButton != startButton && !loadRun)
		{
			btnCtrl.SetFocus(startButton);
		}
		else if (loadRun || CurrentHeroSkinIsUnlocked())
		{
			StartCoroutine(_StartCampaign(loadRun));
		}
		else
		{
			FlashSkinError();
		}
	}

	private IEnumerator _StartCampaign(bool loadRun)
	{
		if (!campaignStarted)
		{
			mainCtrl.quitButtonEnabled = false;
			optCtrl.quitButtonEnabled = false;
			mainCtrl.transitioning = true;
			ctrl.idCtrl.moneyTextBattle.gameObject.SetActive(false);
			runCtrl.currentHellPassNum = hellPassButton.displayedHellPassNum;
			PostCtrl.transitioning = true;
			btnCtrl.RemoveFocus();
			if (loadRun && !spCtrl.heroDictionary.ContainsKey(runCtrl.loadedRun.beingID))
			{
				loadRun = false;
				ctrl.currentHeroObj = spCtrl.heroDictionary["Saffron"].Clone();
			}
			if (!loadRun)
			{
				runCtrl.loadedRun = null;
			}
			campaignStarted = true;
			anim.SetBool("visible", false);
			S.I.PlayOnce(startSound);
			camCtrl.TransitionInHigh("LeftWipe");
			yield return new WaitForSeconds(0.7f);
			string seed = seedInput.text;
			if (loadRun)
			{
				runCtrl.CreateRunFromSave(runCtrl.loadedRun);
				ctrl.shopCtrl.sera = runCtrl.loadedRun.sera;
			}
			else
			{
				runCtrl.StartCampaign(loadRun, seed);
				ctrl.shopCtrl.sera = ctrl.currentHeroObj.money;
				seedInput.text = string.Empty;
			}
			yield return new WaitForSeconds(0.5f);
			mainCtrl.transitioning = false;
			HideSelectors();
			_003C_003En__0();
			mainCtrl.ForceClose();
			campaignStarted = false;
			SaveDataCtrl.Set("LastChosenCharacter", ctrl.currentHeroObj.nameString);
			UnityEngine.Debug.Log("Starting Campaign as " + ctrl.currentHeroObj.beingID + " Loaded run: " + loadRun);
		}
	}

	public override void Close()
	{
		if (campaignStarted)
		{
			return;
		}
		btnCtrl.RemoveFocus();
		HideSelectors();
		base.Close();
		if (!btnCtrl.activeNavPanels.Contains(mainCtrl))
		{
			heroSplash.Hide();
			if (!mainCtrl.startedUp)
			{
				mainCtrl.Startup(0f, false, false);
			}
			else
			{
				mainCtrl.Open();
			}
		}
	}

	public void ShowOutfitButton(BeingObject heroObj)
	{
		if (heroObj.allAnims.Count > 1)
		{
			changeSkinButton.alpha = 1f;
		}
		else
		{
			changeSkinButton.alpha = 0.8f;
		}
	}

	private void HideSelectors()
	{
		heroGridLeftSelector.Empty();
		heroGridRightSelector.Empty();
		heroGridVerticalSelector.Empty();
		altGridLeftSelector.Empty();
		altGridRightSelector.Empty();
		hellButtonRightSelector.Empty();
		hellButtonVerticalSelector.Empty();
		startButtonLeftSelector.Empty();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.Close();
	}
}
