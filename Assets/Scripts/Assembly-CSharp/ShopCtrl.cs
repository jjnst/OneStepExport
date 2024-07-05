using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[MoonSharpUserData]
public class ShopCtrl : NavPanel
{
	private enum Emotion
	{
		BePatient = 0,
		Buy = 1,
		BuyService = 2,
		DonateBlood = 3,
		Intro = 4,
		OutOfRemovers = 5,
		Poke = 6,
		Restock = 7,
		SoldOut = 8,
		YouNeedMoreHP = 9,
		YouNeedMoreMoney = 10
	}

	public int baseRefillCost = 30;

	public int refillCost;

	public int refillAdd = 0;

	public int refillInterval = 10;

	public TMP_Text refillCostText;

	public NavButton refillButton;

	public int removeCost;

	public TMP_Text removeCostText;

	public NavButton removeButton;

	public TMP_Text upgradeCostText;

	public int baseUpgradeCost;

	public int upgradeCost;

	public int upgradeInterval = 20;

	public NavButton upgradeButton;

	public int donateStartingValue = 20;

	public int donateCurrentValue = 0;

	public float donateValueDeprecationRate = 0.8f;

	public int donateHealthAmount = 100;

	public NavButton donateButton;

	public TMP_Text donateValueText;

	public TMP_Text donateHealthText;

	public float emoteDuration = 1f;

	public AudioClip createShopCardSound;

	public AudioClip buyShopCardSound;

	public AudioClip buyShopCardHealthSound;

	public AudioClip donateBloodSound;

	public BossShopkeeper currentShopkeeper;

	public List<ItemObject> potentialSpellOptions = new List<ItemObject>();

	public List<ItemObject> potentialArtOptions = new List<ItemObject>();

	public List<ChoiceCard> currentShopOptions;

	public List<ChoiceCard> currentSpellOptions;

	public List<ChoiceCard> currentArtOptions;

	public List<PactObject> pactChallengeOptions = new List<PactObject>();

	public List<ChoiceCard> currentPactOptions = new List<ChoiceCard>();

	public Dictionary<string, Sprite> shopkeeperSplashes;

	public Transform artGrid;

	public Transform spellGrid;

	public Transform pactGrid;

	public TalkBox talkBox;

	public PriceText priceTextPrefab;

	public int rarityCostbase;

	public int rarityCostMultiplier;

	public Image storeBG;

	public Sprite defaultStoreBG;

	public Sprite darkStoreBG;

	public Image shopkeeperSplash;

	public Sprite defaultShopkeeperSplash;

	public Sprite darkShopkeeperSplash;

	public ZoneType shopZoneType;

	private bool removePurchased = false;

	public int opensThisZone = 0;

	private int Sera = 0;

	public bool selfMode = false;

	private UnityEngine.Coroutine co_Emote;

	private BC ctrl;

	private DeckCtrl deCtrl;

	private IdleCtrl idCtrl;

	private ItemManager itemMan;

	private ReferenceCtrl refCtrl;

	private RunCtrl runCtrl;

	public static bool transitioning = false;

	private bool animatingOptions = false;

	public int sera
	{
		get
		{
			return Sera;
		}
		set
		{
			Sera = value;
			ctrl.idCtrl.moneyText.text = string.Format("{0}", Sera);
			ctrl.idCtrl.moneyTextBattle.text = ctrl.idCtrl.moneyText.text;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		ctrl = S.I.batCtrl;
		deCtrl = S.I.deCtrl;
		idCtrl = S.I.idCtrl;
		itemMan = S.I.itemMan;
		refCtrl = S.I.refCtrl;
		runCtrl = S.I.runCtrl;
	}

	public override void Open()
	{
		if (((bool)currentShopkeeper && !currentShopkeeper.downed && !currentShopkeeper.rudeShown) || selfMode)
		{
			S.I.PlayOnce(S.I.idCtrl.menuSlideSound);
			StartCoroutine(_Open());
		}
	}

	private IEnumerator _Open()
	{
		content.SetActive(true);
		ctrl.AddControlBlocks(Block.ShopPanel);
		transitioning = true;
		if (selfMode)
		{
			refillCost = 140;
			yield return new WaitForSeconds(deCtrl.deckScreen.introDelay);
		}
		else
		{
			refillCost = baseRefillCost;
			if ((bool)currentShopkeeper)
			{
				currentShopkeeper.mov.MoveTo(ctrl.currentPlayer.mov.currentTile.x + 1, ctrl.currentPlayer.mov.currentTile.y, true, false, true, false, false, false);
				currentShopkeeper.anim.SetBool("dashing", true);
				while (currentShopkeeper.mov.state == State.Moving)
				{
					yield return null;
				}
				currentShopkeeper.anim.SetBool("dashing", false);
			}
			yield return new WaitForSeconds(deCtrl.deckScreen.introDelay);
			if ((bool)currentShopkeeper)
			{
				currentShopkeeper.anim.SetTrigger("charge");
			}
			if (currentShopkeeper == null || currentShopkeeper.downed)
			{
				Close();
				yield break;
			}
		}
		refillCostText.text = (refillCost + refillAdd).ToString();
		removeCostText.text = removeCost.ToString();
		upgradeCostText.text = (upgradeCost + upgradeInterval * runCtrl.currentRun.shopUpgradesPurchased).ToString();
		if (currentShopOptions.Count > 0)
		{
			Emote(Emotion.Intro);
			StartCoroutine(AnimateShopOptions());
		}
		else
		{
			Emote(Emotion.SoldOut);
		}
		if (ctrl.currentPlayer.duelDisk.deck.Count > 0)
		{
			upgradeButton.right = ctrl.currentPlayer.duelDisk.deck[0];
			removeButton.right = ctrl.currentPlayer.duelDisk.deck[0];
		}
		else
		{
			upgradeButton.right = deCtrl.deckScreen.foCtrl.brandDisplayButtons[0];
			removeButton.right = deCtrl.deckScreen.foCtrl.brandDisplayButtons[0];
		}
		_003C_003En__0();
		originButton = deCtrl.deckScreen.originButton;
		transitioning = false;
		RefreshButtonMapping();
		opensThisZone++;
		if (opensThisZone > 29 && (bool)currentShopkeeper)
		{
			currentShopkeeper.StartConvert(true);
		}
	}

	private string LocalizationKey()
	{
		if (selfMode)
		{
			return "Self";
		}
		if (shopZoneType == ZoneType.DarkShop)
		{
			return "Dark";
		}
		return "";
	}

	private void Emote(Emotion emotion)
	{
		talkBox.AnimateRandomLine("Shopkeeper/Shop_" + emotion.ToString() + LocalizationKey());
		shopkeeperSplash.sprite = shopkeeperSplashes[LocalizationKey() + emotion];
		if (co_Emote != null)
		{
			StopCoroutine(co_Emote);
			co_Emote = null;
		}
		co_Emote = StartCoroutine(_Emote());
	}

	private IEnumerator _Emote()
	{
		yield return new WaitForSeconds(emoteDuration);
		shopkeeperSplash.sprite = shopkeeperSplashes[LocalizationKey() + Emotion.Intro];
		co_Emote = null;
	}

	private void SetListCardDown(ListCard listCard, int index)
	{
		if (index <= 6)
		{
			if (currentArtOptions.Count > 0)
			{
				listCard.down = currentArtOptions[0];
			}
			else if (currentSpellOptions.Count > 0)
			{
				listCard.down = currentSpellOptions[0];
			}
		}
		else if (index <= 9)
		{
			if (currentSpellOptions.Count > 0)
			{
				listCard.down = currentSpellOptions[0];
			}
		}
		else if (index <= 15)
		{
			if (currentPactOptions.Count > 0)
			{
				listCard.down = currentPactOptions[0];
			}
			else
			{
				listCard.down = removeButton;
			}
		}
		else if (index <= 16)
		{
			if (ctrl.currentPlayer.duelDisk.deck.Count > 0)
			{
				listCard.down = ctrl.currentPlayer.duelDisk.deck[0];
			}
		}
		else
		{
			SetListCardDown(listCard, index - 16);
		}
	}

	private void SetPactCardNav(ListCard listCard, int index)
	{
		if (deCtrl.artCardList.Count > index)
		{
			listCard.up = deCtrl.artCardList[index];
			listCard.up.down = listCard;
		}
		SetListCardDown(listCard, index);
	}

	public override void Close()
	{
		StartCoroutine(_CloseShop());
	}

	private IEnumerator _CloseShop()
	{
		if (!currentShopkeeper && !selfMode)
		{
			yield break;
		}
		_003C_003En__1();
		if (btnCtrl.IsActivePanel(deCtrl.deckScreen))
		{
			deCtrl.deckScreen.Close();
		}
		transitioning = true;
		foreach (ChoiceCard shopOption in currentShopOptions)
		{
			shopOption.anim.SetBool("OnScreen", false);
		}
		if ((bool)currentShopkeeper)
		{
			currentShopkeeper.anim.SetTrigger("release");
			yield return new WaitForSeconds(0.4f);
			if ((bool)currentShopkeeper && !currentShopkeeper.downed)
			{
				currentShopkeeper.mov.MoveTo(currentShopkeeper.mov.currentTile.x, currentShopkeeper.mov.currentTile.y, false, false);
				while (currentShopkeeper.mov.state == State.Moving)
				{
					yield return null;
				}
			}
		}
		if ((bool)ctrl.currentPlayer)
		{
			ctrl.RemoveControlBlocks(Block.ShopPanel);
		}
		transitioning = false;
	}

	public IEnumerator AnimateShopOptions()
	{
		yield return new WaitWhile(() => animatingOptions);
		animatingOptions = true;
		foreach (ChoiceCard shopOption in currentShopOptions)
		{
			yield return new WaitForSeconds(0.06f);
			if ((bool)shopOption.anim)
			{
				shopOption.anim.SetBool("OnScreen", true);
			}
			if (shopOption.cardInner.priceText != null)
			{
				shopOption.cardInner.priceText.gameObject.SetActive(true);
			}
		}
		animatingOptions = false;
	}

	public void LoadShopOptions(Run loadedRun)
	{
		ClearShopLists();
		foreach (string shopOption in loadedRun.shopOptions)
		{
			if (itemMan.itemDictionary[shopOption].type == ItemType.Art)
			{
				CreateShopCard(itemMan.artDictionary[shopOption].Clone(), artGrid, currentArtOptions);
			}
			else if (itemMan.itemDictionary[shopOption].type == ItemType.Spell)
			{
				CreateShopCard(itemMan.spellDictionary[shopOption].Clone(), spellGrid, currentSpellOptions);
			}
			else if (itemMan.itemDictionary[shopOption].type == ItemType.Pact)
			{
				CreateShopCard(itemMan.pactDictionary[shopOption].ClonePact().SetReward(itemMan.pactDictionary[loadedRun.shopPactRewards[currentPactOptions.Count]].ClonePact()), pactGrid, currentPactOptions);
			}
		}
		RefreshButtonMapping();
	}

	private void ClearShopLists()
	{
		potentialSpellOptions.Clear();
		potentialArtOptions.Clear();
		currentArtOptions.Clear();
		currentPactOptions.Clear();
		currentSpellOptions.Clear();
		currentShopOptions.Clear();
		spellGrid.DestroyChildren();
		artGrid.DestroyChildren();
		pactGrid.DestroyChildren();
	}

	public IEnumerator CreateShopOptions()
	{
		yield return new WaitWhile(() => animatingOptions);
		ClearShopLists();
		S.I.PlayOnce(createShopCardSound);
		int rarityBonus = 15;
		if (shopZoneType == ZoneType.DarkShop)
		{
			rarityBonus = 20;
		}
		if (deCtrl.artCardList.Count < 48)
		{
			List<ItemObject> currentArtItems = new List<ItemObject>();
			List<ArtifactObject> baseRewardsList = itemMan.baseArtList;
			if (ctrl.currentPlayer.health.current == ctrl.currentPlayer.health.max)
			{
				baseRewardsList = baseRewardsList.Where((ArtifactObject t) => !t.tags.Contains(Tag.Heal)).ToList();
			}
			currentArtItems.Add(baseRewardsList[runCtrl.NextPsuedoRand(0, baseRewardsList.Count)].Clone());
			foreach (ArtifactObject itemKey2 in itemMan.unobtainedNonBaseArts)
			{
				potentialArtOptions.Add(itemKey2);
			}
			List<int> randomArtBase = new List<int>();
			for (int i4 = 0; i4 < potentialArtOptions.Count; i4++)
			{
				randomArtBase.Add(i4);
			}
			for (int i3 = 0; i3 < 2; i3++)
			{
				int randArt = runCtrl.NextPsuedoRand(0, randomArtBase.Count);
				currentArtItems.AddRange(itemMan.GetItems(ctrl.poCtrl.GenerateRewardValue(rarityBonus), 1, ItemType.Art, true, Brand.None, currentArtItems));
				randomArtBase.RemoveAt(randArt);
			}
			for (int i2 = 0; i2 < currentArtItems.Count; i2++)
			{
				CreateShopCard(currentArtItems[i2], artGrid, currentArtOptions);
			}
		}
		List<ItemObject> currentSpellItems = new List<ItemObject>();
		foreach (SpellObject itemKey in itemMan.playerSpellList)
		{
			potentialSpellOptions.Add(itemKey);
		}
		List<int> randomSpellBase = new List<int>();
		for (int n = 0; n < potentialSpellOptions.Count; n++)
		{
			randomSpellBase.Add(n);
		}
		for (int m = 0; m < 2; m++)
		{
			int rand = runCtrl.NextPsuedoRand(0, randomSpellBase.Count);
			currentSpellItems.AddRange(itemMan.GetItems(ctrl.poCtrl.GenerateRewardValue(rarityBonus), 1, ItemType.Spell, true, Brand.None, currentSpellItems));
			randomSpellBase.RemoveAt(rand);
		}
		for (int l = 0; l < currentSpellItems.Count; l++)
		{
			CreateShopCard(currentSpellItems[l], spellGrid, currentSpellOptions);
		}
		pactChallengeOptions = new List<PactObject>(itemMan.pactChallengeList);
		List<PactObject> pactRewardOptions = new List<PactObject>(itemMan.pactRewardList);
		List<int> randomBaseC = new List<int>();
		for (int k = 0; k < pactChallengeOptions.Count; k++)
		{
			randomBaseC.Add(k);
		}
		List<int> randomBaseR = new List<int>();
		for (int j = 0; j < pactRewardOptions.Count; j++)
		{
			randomBaseR.Add(j);
		}
		for (int i = 0; i < 2; i++)
		{
			int randC = runCtrl.NextPsuedoRand(0, randomBaseC.Count);
			int randR = runCtrl.NextPsuedoRand(0, randomBaseR.Count);
			CreateShopCard(pactChallengeOptions[randomBaseC[randC]].ClonePact().SetReward(pactRewardOptions[randomBaseR[randR]]), pactGrid, currentPactOptions);
			randomBaseC.RemoveAt(randC);
		}
		RefreshButtonMapping();
		potentialSpellOptions.Clear();
		potentialArtOptions.Clear();
		yield return new WaitForEndOfFrame();
	}

	private ChoiceCard CreateShopCard(ItemObject itemObj, Transform parentGrid, List<ChoiceCard> parentList)
	{
		ChoiceCard choiceCard = deCtrl.CreateNewChoiceCard(itemObj, parentGrid);
		choiceCard.shopCard = true;
		choiceCard.shopCtrl = this;
		choiceCard.refCtrl = refCtrl;
		choiceCard.btnCtrl = btnCtrl;
		if (itemObj.pactObj == null)
		{
			Vector3 vector = new Vector3(-19f, choiceCard.cardInner.rect.sizeDelta.y / 2f * spellGrid.localScale.y - 3f, 0f);
			Vector3 vector2 = new Vector3(-19f, choiceCard.cardInner.rect.sizeDelta.y / 2f * spellGrid.localScale.y - 2f, 0f);
			if (S.I.readabilityModeEnabled)
			{
				vector = new Vector3(15f, choiceCard.cardInner.rect.sizeDelta.y / 2f * spellGrid.localScale.y - 12f, 0f);
				vector2 = new Vector3(15f, choiceCard.cardInner.rect.sizeDelta.y / 2f * spellGrid.localScale.y - 10f, 0f);
			}
			if (choiceCard.cardInner.priceText == null)
			{
				PriceText priceText = Object.Instantiate(priceTextPrefab, choiceCard.cardInner.transform.position - vector, base.transform.rotation);
				choiceCard.cardInner.SetPriceText(priceText);
			}
			else
			{
				choiceCard.cardInner.priceText.gameObject.SetActive(true);
			}
			if (choiceCard.cardInner.voteDisplay != null)
			{
				choiceCard.cardInner.voteDisplay.gameObject.SetActive(false);
			}
			if (choiceCard.itemObj.artObj != null)
			{
				choiceCard.cardInner.priceText.transform.position = choiceCard.cardInner.transform.position - vector2;
			}
			int num = Mathf.RoundToInt(rarityCostbase * 2 + itemObj.rarity * rarityCostMultiplier);
			if (itemObj.type == ItemType.Spell)
			{
				num = Mathf.RoundToInt((float)rarityCostbase + (float)(itemObj.rarity * rarityCostMultiplier) * 0.75f);
			}
			if (shopZoneType == ZoneType.DarkShop)
			{
				num = Mathf.RoundToInt(num * 2);
			}
			if (runCtrl.currentRun.hellPasses.Contains(12))
			{
				num = Mathf.RoundToInt((float)num * 1.15f);
			}
			if (selfMode)
			{
				num = 0;
			}
			choiceCard.cardInner.priceText.amount.text = string.Format("{0}", num);
			choiceCard.cardInner.priceText.Set(shopZoneType);
			choiceCard.price = num;
		}
		else
		{
			choiceCard.price = 0;
		}
		parentList.Add(choiceCard);
		choiceCard.parentList = parentList;
		currentShopOptions.Add(choiceCard);
		return choiceCard;
	}

	public void TryBuyItem(ChoiceCard cardToBuy)
	{
		if (!animatingOptions)
		{
			if (selfMode)
			{
				BuyWithMoney(cardToBuy);
			}
			else if (shopZoneType == ZoneType.Shop)
			{
				BuyWithMoney(cardToBuy);
			}
			else if (shopZoneType == ZoneType.DarkShop)
			{
				BuyWithHealth(cardToBuy);
			}
		}
	}

	public void ModifySera(int amount)
	{
		sera += amount;
		sera = Mathf.Clamp(sera, 0, sera);
	}

	public void BuyWithMoney(ChoiceCard cardToBuy)
	{
		if (sera >= cardToBuy.price)
		{
			sera -= cardToBuy.price;
			BuyCard(cardToBuy);
			S.I.PlayOnce(buyShopCardSound);
			if (cardToBuy.itemObj.type == ItemType.Pact)
			{
				Emote(Emotion.BuyService);
			}
			else
			{
				Emote(Emotion.Buy);
			}
		}
		else
		{
			YouNeedMoreMoney();
		}
	}

	public void BuyWithHealth(ChoiceCard cardToBuy)
	{
		if (ctrl.currentPlayer.health.current >= cardToBuy.price)
		{
			foreach (Player currentPlayer in ctrl.currentPlayers)
			{
				currentPlayer.health.ModifyHealth(-cardToBuy.price);
			}
			BuyCard(cardToBuy);
			if (cardToBuy.itemObj.type != ItemType.Pact || cardToBuy.itemObj.HasEffect(Effect.Damage))
			{
				Emote(Emotion.Buy);
				S.I.Flash(UIColor.RedLight);
				S.I.PlayOnce(buyShopCardHealthSound);
			}
			else
			{
				S.I.PlayOnce(buyShopCardSound);
				Emote(Emotion.BuyService);
			}
		}
		else
		{
			Emote(Emotion.YouNeedMoreHP);
		}
	}

	private void BuyCard(ChoiceCard cardToBuy)
	{
		cardToBuy.GetThisCard();
		if (cardToBuy.itemObj.type == ItemType.Pact)
		{
			currentPactOptions.Remove(cardToBuy);
			if (currentPactOptions.Count > 0)
			{
				btnCtrl.SetFocus(currentPactOptions[0]);
			}
			else
			{
				btnCtrl.SetFocus(refillButton);
			}
		}
		else if (cardToBuy.itemObj.type == ItemType.Spell)
		{
			currentSpellOptions.Remove(cardToBuy);
		}
		else if (cardToBuy.itemObj.type == ItemType.Art)
		{
			currentArtOptions.Remove(cardToBuy);
		}
		currentShopOptions.Remove(cardToBuy);
		if (currentShopOptions.Count > 0)
		{
			btnCtrl.SetFocus(currentShopOptions[0]);
		}
		else
		{
			btnCtrl.SetFocus(refillButton);
		}
		deCtrl.deckScreen.UpdateUpgraderCountText();
		deCtrl.deckScreen.UpdateRemoverCountText();
		RefreshButtonMapping();
	}

	public void SetShopkeeper(Boss newShopKeeper, ZoneType zoneType, bool createShopOptions)
	{
		opensThisZone = 0;
		refillAdd = 0;
		donateCurrentValue = donateStartingValue;
		donateValueText.text = "+" + donateCurrentValue;
		donateHealthText.text = "-" + donateHealthAmount;
		removePurchased = false;
		removeButton.canvasGroup.alpha = 1f;
		removeButton.hoverAlpha = 1f;
		shopZoneType = zoneType;
		if (zoneType == ZoneType.DarkShop)
		{
			storeBG.sprite = darkStoreBG;
			shopkeeperSplash.sprite = darkShopkeeperSplash;
		}
		else
		{
			storeBG.sprite = defaultStoreBG;
			shopkeeperSplash.sprite = defaultShopkeeperSplash;
		}
		if ((bool)newShopKeeper)
		{
			newShopKeeper.RemoveAllStatuses();
			newShopKeeper.ti = ctrl.ti;
			currentShopkeeper = newShopKeeper.GetComponent<BossShopkeeper>();
		}
		if (createShopOptions)
		{
			StartCoroutine(CreateShopOptions());
		}
	}

	public void UpdateDonateValueText()
	{
		donateValueText.text = "+" + donateCurrentValue;
	}

	public void PokeShopKeeper()
	{
		Emote(Emotion.Poke);
	}

	public void ClickRemoveBtn()
	{
		if (removePurchased)
		{
			Emote(Emotion.OutOfRemovers);
			S.I.PlayOnce(btnCtrl.lockedSound);
		}
		else if (sera >= removeCost)
		{
			if (!selfMode)
			{
				removePurchased = true;
				removeButton.canvasGroup.alpha = 0.4f;
				removeButton.hoverAlpha = 0.4f;
			}
			ModifyRemovalCount(1);
			S.I.PlayOnce(buyShopCardSound);
			sera -= removeCost;
			Emote(Emotion.BuyService);
		}
		else
		{
			YouNeedMoreMoney();
		}
	}

	public void ModifyRemovalCount(int amount)
	{
		runCtrl.currentRun.removals += amount;
		deCtrl.deckScreen.UpdateRemoverCountText();
	}

	public void ClickUpgradeBtn()
	{
		if (sera >= upgradeCost + upgradeInterval * runCtrl.currentRun.shopUpgradesPurchased)
		{
			S.I.PlayOnce(buyShopCardSound);
			ModifyUpgraderCount(1);
			sera -= upgradeCost + upgradeInterval * runCtrl.currentRun.shopUpgradesPurchased;
			runCtrl.currentRun.shopUpgradesPurchased++;
			upgradeCostText.text = (upgradeCost + upgradeInterval * runCtrl.currentRun.shopUpgradesPurchased).ToString();
			Emote(Emotion.BuyService);
		}
		else
		{
			YouNeedMoreMoney();
		}
	}

	public IEnumerator FocusNextFrame(UIButton theButton)
	{
		yield return new WaitForEndOfFrame();
		btnCtrl.SetFocus(theButton);
	}

	private void YouNeedMoreMoney()
	{
		Emote(Emotion.YouNeedMoreMoney);
		S.I.PlayOnce(btnCtrl.lockedSound);
	}

	public void ModifyUpgraderCount(int amount)
	{
		runCtrl.currentRun.upgraders += amount;
		deCtrl.deckScreen.UpdateUpgraderCountText();
	}

	public void ClickDonateBtn()
	{
		if (ctrl.currentPlayer.health.current > donateHealthAmount)
		{
			foreach (Player currentPlayer in ctrl.currentPlayers)
			{
				currentPlayer.health.ModifyHealth(-donateHealthAmount);
			}
			sera += donateCurrentValue;
			donateCurrentValue = Mathf.FloorToInt((float)donateCurrentValue * donateValueDeprecationRate);
			donateCurrentValue = Mathf.Clamp(donateCurrentValue, 1, donateStartingValue);
			runCtrl.currentRun.shopDonateValue = donateCurrentValue;
			donateValueText.text = "+" + donateCurrentValue;
			donateHealthText.text = "-" + donateHealthAmount;
			Emote(Emotion.DonateBlood);
			S.I.PlayOnce(buyShopCardHealthSound);
			S.I.Flash(UIColor.RedLight);
		}
		else
		{
			Emote(Emotion.YouNeedMoreHP);
		}
	}

	public void Refill()
	{
		if (!animatingOptions)
		{
			if (sera >= refillCost + refillAdd)
			{
				StartCoroutine(RefillC());
				sera -= refillCost + refillAdd;
				refillAdd += refillInterval;
				refillCostText.text = (refillCost + refillAdd).ToString();
			}
			else
			{
				YouNeedMoreMoney();
			}
		}
		else
		{
			Emote(Emotion.BePatient);
		}
	}

	private IEnumerator RefillC()
	{
		Emote(Emotion.Restock);
		yield return StartCoroutine(CreateShopOptions());
		yield return new WaitForSeconds(0.2f);
		StartCoroutine(AnimateShopOptions());
		RefreshButtonMapping();
	}

	public void RefreshButtonMapping()
	{
		foreach (ListCard artCard in deCtrl.artCardList)
		{
			SetListCardDown(artCard, artCard.transform.GetSiblingIndex());
		}
		foreach (ListCard pactCard in deCtrl.pactCardList)
		{
			SetPactCardNav(pactCard, pactCard.transform.GetSiblingIndex());
		}
		foreach (ListCard item in ctrl.currentPlayer.duelDisk.deck)
		{
			if (currentPactOptions.Count > 0)
			{
				item.left = currentPactOptions[0];
			}
			else
			{
				item.left = upgradeButton;
			}
		}
		if (currentPactOptions.Count > 0)
		{
			refillButton.up = currentPactOptions[currentPactOptions.Count - 1];
			upgradeButton.up = currentPactOptions[currentPactOptions.Count - 1];
		}
		else if (deCtrl.artCardList.Count > 0)
		{
			refillButton.up = deCtrl.artCardList[deCtrl.artCardList.Count - 1];
			upgradeButton.up = deCtrl.artCardList[deCtrl.artCardList.Count - 1];
		}
		SetLeftToSpellsOrArtBot(refillButton);
		SetLeftToSpellsOrArtBot(donateButton);
		for (int i = 0; i < currentPactOptions.Count; i++)
		{
			if (ctrl.currentPlayer.duelDisk.deck.Count > 0)
			{
				currentPactOptions[i].right = ctrl.currentPlayer.duelDisk.deck[0];
			}
			else
			{
				currentPactOptions[i].right = deCtrl.deckScreen.foCtrl.brandDisplayButtons[0];
			}
			if (i == 0)
			{
				if (deCtrl.artCardList.Count > 0)
				{
					if (deCtrl.artCardList.Count <= 12)
					{
						currentPactOptions[i].up = deCtrl.artCardList[deCtrl.artCardList.Count - 1];
					}
					else
					{
						currentPactOptions[i].up = deCtrl.artCardList[12];
					}
				}
				else if (deCtrl.pactCardList.Count > 0)
				{
					if (deCtrl.pactCardList.Count <= 12)
					{
						currentPactOptions[i].up = deCtrl.pactCardList[deCtrl.pactCardList.Count - 1];
					}
					else
					{
						currentPactOptions[i].up = deCtrl.pactCardList[12];
					}
				}
				if (currentPactOptions.Count <= 1)
				{
					currentPactOptions[i].down = refillButton;
				}
			}
			else
			{
				currentPactOptions[i].down = refillButton;
			}
			SetLeftToSpellsOrArtTop(currentPactOptions[i]);
		}
		for (int j = 0; j < currentSpellOptions.Count; j++)
		{
			if (currentArtOptions.Count > 0)
			{
				if (j == 0)
				{
					currentSpellOptions[j].left = currentArtOptions[0];
				}
				else
				{
					currentSpellOptions[j].left = currentArtOptions[currentArtOptions.Count - 1];
				}
			}
			else
			{
				currentSpellOptions[j].left = null;
			}
			if (j == 0)
			{
				if (deCtrl.artCardList.Count > 0)
				{
					currentSpellOptions[j].up = deCtrl.artCardList[deCtrl.artCardList.Count - 1];
					if (deCtrl.artCardList.Count <= 8)
					{
						currentSpellOptions[j].up = deCtrl.artCardList[deCtrl.artCardList.Count - 1];
					}
					else
					{
						currentSpellOptions[j].up = deCtrl.artCardList[8];
					}
				}
				if (currentPactOptions.Count > 0)
				{
					currentSpellOptions[j].right = currentPactOptions[0];
				}
				else if (ctrl.currentPlayer.duelDisk.deck.Count > 0)
				{
					currentSpellOptions[j].right = ctrl.currentPlayer.duelDisk.deck[0];
				}
				else
				{
					currentSpellOptions[j].right = deCtrl.deckScreen.foCtrl.brandDisplayButtons[0];
				}
			}
			else
			{
				currentSpellOptions[j].right = refillButton;
				if (currentArtOptions.Count > 0)
				{
					currentSpellOptions[j].left = currentArtOptions[currentArtOptions.Count - 1];
				}
			}
		}
		for (int k = 0; k < currentArtOptions.Count; k++)
		{
			switch (k)
			{
			case 0:
				if (deCtrl.artCardList.Count > 0)
				{
					if (deCtrl.artCardList.Count <= 5)
					{
						currentArtOptions[k].up = deCtrl.artCardList[deCtrl.artCardList.Count - 1];
					}
					else
					{
						currentArtOptions[k].up = deCtrl.artCardList[5];
					}
				}
				if (currentSpellOptions.Count > 0)
				{
					currentArtOptions[k].right = currentSpellOptions[0];
				}
				else if (currentPactOptions.Count > 0)
				{
					currentArtOptions[k].right = currentPactOptions[0];
				}
				else if (ctrl.currentPlayer.duelDisk.deck.Count > 0)
				{
					currentArtOptions[k].right = ctrl.currentPlayer.duelDisk.deck[0];
				}
				else
				{
					currentArtOptions[k].right = deCtrl.deckScreen.foCtrl.brandDisplayButtons[0];
				}
				break;
			case 1:
				if (currentSpellOptions.Count > 0)
				{
					currentArtOptions[k].right = currentSpellOptions[currentSpellOptions.Count - 1];
				}
				else if (currentPactOptions.Count > 0)
				{
					currentArtOptions[k].right = currentPactOptions[currentPactOptions.Count - 1];
				}
				else if (ctrl.currentPlayer.duelDisk.deck.Count > 0)
				{
					currentArtOptions[k].right = ctrl.currentPlayer.duelDisk.deck[0];
				}
				else
				{
					currentArtOptions[k].right = deCtrl.deckScreen.foCtrl.brandDisplayButtons[0];
				}
				break;
			default:
				if (currentSpellOptions.Count > 0)
				{
					currentArtOptions[k].right = currentSpellOptions[currentSpellOptions.Count - 1];
				}
				else
				{
					currentArtOptions[k].right = refillButton;
				}
				break;
			}
		}
	}

	private void SetLeftToSpellsOrArtTop(UIButton navButton)
	{
		if (currentSpellOptions.Count > 0)
		{
			navButton.left = currentSpellOptions[0];
		}
		else if (currentArtOptions.Count > 0)
		{
			navButton.left = currentArtOptions[0];
		}
		else
		{
			navButton.left = null;
		}
	}

	private void SetLeftToSpellsOrArtBot(UIButton navButton)
	{
		if (currentSpellOptions.Count > 0)
		{
			navButton.left = currentSpellOptions[currentSpellOptions.Count - 1];
		}
		else if (currentArtOptions.Count > 0)
		{
			navButton.left = currentArtOptions[currentArtOptions.Count - 1];
		}
		else
		{
			navButton.left = null;
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.Open();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__1()
	{
		base.Close();
	}
}
