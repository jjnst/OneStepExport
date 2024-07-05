using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class DeckCtrl : MonoBehaviour
{
	public int minDeckSize = 4;

	public int maxDeckSize = 100;

	public float requeueTime = 0.03f;

	public float loadSlotDuration = 0.1f;

	public int cardtridgeLimit = 9999;

	public DeckScreen deckScreen;

	public SpellListCard spellListCardPrefab;

	public StatsScreen statsScreen;

	public Transform artGrid;

	public Transform playerTwoArtGrid;

	public List<ListCard> artCardList;

	public List<PactObject> activePacts = new List<PactObject>();

	public Transform pactGrid;

	public List<ListCard> pactCardList;

	public GameObject artDisplay;

	public Sprite cardWepBG;

	public GameObject cardtridgePrefab;

	public CastSlot castSlotPrefab;

	public GameObject choiceCardPrefab;

	public GameObject cardInnerSpellPrefab;

	public GameObject cardInnerArtPrefab;

	public GameObject cardInnerPactPrefab;

	public GameObject cardInnerBrandPrefab;

	public VoteDisplay voteDisplayPrefab;

	public DisplayCard displayCardPrefab;

	public DisplayCard displayCard;

	public DisplayCard displayCardAlt;

	public MechDetails mechTooltip;

	public GameObject artListCardPrefab;

	public AudioClip shuffleSound;

	public AudioClip shuffleEndSound;

	public AudioClip sellSound;

	public Camera referenceCam;

	public Sprite[] brandSprites;

	public List<RuntimeAnimatorController> wAnims;

	public List<Effect> effectTooltips;

	public List<FTrigger> triggerTooltips;

	public List<string> highlightedMechs;

	public List<Effect> generatedDescriptionEffects;

	public Transform cardTrailContainer;

	public Material dissolveMat;

	public Material upgradeMat;

	public Transform iconHolder;

	public SpriteRenderer testIconDisplay;

	public Sprite blankBorder;

	public List<Sprite> spellRarityBorders;

	public List<Sprite> spellBackgrounds;

	public List<Sprite> spellBackgroundBrands;

	public Sprite weaponBackground;

	public List<Sprite> artRarityBorders;

	public List<Sprite> artBackgrounds;

	public ButtonCtrl btnCtrl;

	public BC ctrl;

	public ControlsCtrl conCtrl;

	public ItemManager itemMan;

	public OptionCtrl optCtrl;

	public RunCtrl runCtrl;

	public DuelDisk duelDiskPrefab;

	public float shuffleStalenessMultiplier;

	public float shuffleStalenessCap;

	public int deckRegenLimit = 4;

	public List<DuelDisk> duelDisks = new List<DuelDisk>();

	private Action<SpellObject> cbSpellObjectCreated;

	public ProjectileFactory projectileFactory;

	private SpellObject spellObjTemp;

	private void Awake()
	{
		projectileFactory = new ProjectileFactory();
		FillShows();
	}

	private void Start()
	{
		if (!displayCard)
		{
			displayCard = UnityEngine.Object.Instantiate(displayCardPrefab, deckScreen.transform.position, deckScreen.transform.rotation, deckScreen.transform);
		}
		if (!displayCardAlt)
		{
			displayCardAlt = UnityEngine.Object.Instantiate(displayCardPrefab, deckScreen.transform.position, deckScreen.transform.rotation, deckScreen.transform);
			displayCardAlt.defaultSize *= 0.9f;
		}
		DestroyDuelDisks();
	}

	public ListCard CreatePlayerItem(ItemObject itemObj, Transform parentTransform, int siblingIndex, DuelDisk duelDisk)
	{
		if (itemObj.type == ItemType.Spell)
		{
			return CreateSpellCard(itemObj.spellObj, parentTransform, siblingIndex, duelDisk);
		}
		if (itemObj.type == ItemType.Art)
		{
			return CreateNewPlayerArtifactCard(CreateArtifact(itemObj.itemID, duelDisk.player), siblingIndex, duelDisk);
		}
		if (itemObj.type == ItemType.Wep)
		{
			return CreateSpellCard(itemObj.spellObj, parentTransform, siblingIndex, duelDisk);
		}
		if (itemObj.type == ItemType.Pact)
		{
			return CreatePact(itemObj.pactObj, siblingIndex, duelDisk);
		}
		return null;
	}

	public SpellObject CreateSpellBase(string itemID, Being being, bool addToCurrentObjs = true)
	{
		if (!itemMan.spellDictionary.ContainsKey(itemID))
		{
			Debug.LogError("Spell dict doesnt contain a proto for key: " + itemID);
		}
		SpellObject spellObject = CreateSpellBase(itemMan.spellDictionary[itemID], being, addToCurrentObjs);
		if (spellObject == null)
		{
			Debug.LogError("There was no spellObj");
			return null;
		}
		if (cbSpellObjectCreated != null)
		{
			cbSpellObjectCreated(spellObject);
		}
		if (addToCurrentObjs)
		{
			being.currentSpellObjs.Add(spellObject);
		}
		else if ((bool)being.player)
		{
			being.player.duelDisk.temporarySpells.Add(spellObject);
		}
		return spellObject;
	}

	public SpellObject CreateSpellBase(SpellObject spellObj, Being being, bool addToCurrentObjs = true, bool transferTempDamage = false)
	{
		SpellObject spellObject = spellObj.Clone().Set(being, true);
		if (spellObject == null)
		{
			Debug.LogError("There was no spellObj");
			return null;
		}
		if (cbSpellObjectCreated != null)
		{
			cbSpellObjectCreated(spellObject);
		}
		if (addToCurrentObjs)
		{
			being.currentSpellObjs.Add(spellObject);
		}
		else if ((bool)being.player)
		{
			being.player.duelDisk.temporarySpells.Add(spellObject);
		}
		return spellObject;
	}

	public ListCard CreateSpellCard(SpellObject spellObj, Transform parentTransform, int siblingIndex, DuelDisk duelDisk)
	{
		int num = siblingIndex;
		if (siblingIndex < parentTransform.childCount && deckScreen.deckMarkers.Count > 0 && siblingIndex != -1)
		{
			siblingIndex += Mathf.FloorToInt(siblingIndex / 5);
		}
		if (siblingIndex == -1 || siblingIndex > parentTransform.childCount)
		{
			siblingIndex = duelDisk.deck.Count + Mathf.FloorToInt(duelDisk.deck.Count / 5);
		}
		if (num == -1 || num > duelDisk.deck.Count)
		{
			num = duelDisk.deck.Count;
		}
		ListCard listCard = UnityEngine.Object.Instantiate(spellListCardPrefab);
		listCard.SetDeckSpell(spellObj, parentTransform, siblingIndex, duelDisk);
		if (itemMan.unobtainedPlayerSpellList.Contains(itemMan.spellDictionary[spellObj.itemID]))
		{
			itemMan.unobtainedPlayerSpellList.Remove(itemMan.spellDictionary[spellObj.itemID]);
		}
		if (parentTransform == deckScreen.deckGrid)
		{
			listCard.parentList = duelDisk.deck;
			duelDisk.deck.Insert(num, listCard);
		}
		if (deckScreen.slideBody.onScreen)
		{
			listCard.anim.SetBool("visible", true);
		}
		else
		{
			listCard.anim.SetBool("visible", false);
		}
		deckScreen.CalculateGrid();
		return listCard;
	}

	public ArtifactObject CreateArtifact(string itemID, Being being)
	{
		return CreateArtifact(new ArtData(itemID), being);
	}

	public ArtifactObject CreateArtifact(ArtData artData, Being being)
	{
		if (!itemMan.artDictionary.ContainsKey(artData.itemID))
		{
			Debug.LogError("Art dict doesnt contain a proto for key: " + artData.itemID);
		}
		return itemMan.artDictionary[artData.itemID].Clone().Set(being, artData);
	}

	public ListCard CreateNewPlayerArtifactCard(ArtifactObject artObj, int siblingIndex, DuelDisk duelDisk, int pvpNum = 0)
	{
		if (itemMan.unobtainedNonBaseArts.Contains(itemMan.artDictionary[artObj.itemID]))
		{
			itemMan.unobtainedNonBaseArts.Remove(itemMan.artDictionary[artObj.itemID]);
		}
		string text = "";
		foreach (ArtifactObject unobtainedNonBaseArt in itemMan.unobtainedNonBaseArts)
		{
			text = text + " " + unobtainedNonBaseArt.itemID;
		}
		if (artObj == null)
		{
			return new ListCard();
		}
		Transform transform = artGrid;
		if (pvpNum == 1)
		{
			transform = playerTwoArtGrid;
		}
		if (siblingIndex == -1)
		{
			siblingIndex = transform.childCount;
		}
		artCardList.Add(UnityEngine.Object.Instantiate(artListCardPrefab).GetComponent<ListCard>().SetArt(artObj, transform, siblingIndex, true));
		if (ctrl.heCtrl.gameMode == GameMode.CoOp && ctrl.currentPlayers.Count > 1 && !artObj.tags.Contains(Tag.Solo))
		{
			CreateArtifact(artObj.itemID, ctrl.currentPlayers[1]);
			statsScreen.UpdateStats(ctrl.currentPlayers[ctrl.currentPlayers.Count - 1]);
		}
		statsScreen.UpdateStats(duelDisk.player);
		return artCardList[artCardList.Count - 1];
	}

	public ListCard CreatePact(PactObject pactObj, int siblingIndex, DuelDisk duelDisk)
	{
		return CreatePact(new PactData(pactObj), siblingIndex, duelDisk);
	}

	public ListCard CreatePact(PactData pactData, int siblingIndex, DuelDisk duelDisk)
	{
		if (!itemMan.pactDictionary.ContainsKey(pactData.itemID))
		{
			Debug.LogError("Art dict doesnt contain a proto for key: " + pactData.itemID);
		}
		if (siblingIndex == -1)
		{
			siblingIndex = pactGrid.childCount;
		}
		PactObject pactObject = itemMan.pactDictionary[pactData.itemID].ClonePact();
		pactObject.SetPact(duelDisk.player, pactData, pactData.hellPass);
		pactCardList.Add(UnityEngine.Object.Instantiate(artListCardPrefab).GetComponent<SquareListCard>().SetPact(pactObject, pactGrid, siblingIndex, true));
		statsScreen.UpdateStats(duelDisk.player);
		return pactCardList[pactCardList.Count - 1];
	}

	public void RemoveCardFromList(ListCard cardToRemove)
	{
		cardToRemove.parentList.Remove(cardToRemove);
		UnityEngine.Object.Destroy(cardToRemove.gameObject);
	}

	public void RemoveCardFromDeck(ListCard cardToRemove, bool triggerOnRemoveSpell)
	{
		S.I.PlayOnce(sellSound);
		if (triggerOnRemoveSpell)
		{
			TriggerAllArtifacts(FTrigger.OnRemoveSpell);
		}
		RemoveCardFromList(cardToRemove);
	}

	public void RemoveArtifactCard(ListCard cardToRemove)
	{
		S.I.PlayOnce(sellSound);
		foreach (ArtifactObject artObj in ctrl.currentPlayer.artObjs)
		{
			if (artObj.HasEffect(Effect.AlterCard))
			{
				SpellObject spellObject = itemMan.spellDictionary[artObj.GetEffect(Effect.AlterCard).value];
				spellObject.ResetToOriginal();
			}
		}
		cardToRemove.parentList.Remove(cardToRemove);
		int index = 0;
		int num = 0;
		for (int i = 0; i < ctrl.currentPlayers.Count; i++)
		{
			if (ctrl.currentPlayers[i].artObjs.Contains(cardToRemove.artObj))
			{
				index = ctrl.currentPlayers[i].artObjs.IndexOf(cardToRemove.artObj);
				ctrl.currentPlayers[i].artObjs.Remove(cardToRemove.artObj);
				num = i;
				ctrl.currentPlayers[i].CheckArts();
			}
		}
		for (int j = 0; j < ctrl.currentPlayers.Count; j++)
		{
			if (j != num)
			{
				ctrl.currentPlayers[j].artObjs.RemoveAt(index);
			}
		}
		if (cardToRemove.itemObj.type == ItemType.Art)
		{
			UnityEngine.Object.Destroy(cardToRemove.artObj.art);
		}
		else if (cardToRemove.itemObj.type == ItemType.Pact)
		{
			cardToRemove.itemObj.being.pactObjs.Remove(cardToRemove.itemObj.pactObj);
			ctrl.deCtrl.activePacts.Remove(cardToRemove.itemObj.pactObj);
			cardToRemove.itemObj.pactObj.pact.listCard.Finish();
			statsScreen.UpdateStats();
			UnityEngine.Object.Destroy(cardToRemove.pactObj.pact);
		}
		if (cardToRemove.artObj.HasEffect(Effect.PoisonSetMinimum))
		{
			BC.poisonMinimum = 0f;
		}
		if (cardToRemove.artObj.HasEffect(Effect.PoisonMultiplyDuration))
		{
			BC.poisonDuration /= cardToRemove.artObj.GetEffect(Effect.PoisonMultiplyDuration).amount;
		}
		UnityEngine.Object.Destroy(cardToRemove.gameObject);
		foreach (ArtifactObject artObj2 in ctrl.currentPlayer.artObjs)
		{
			if (artObj2.HasEffect(Effect.AlterCard))
			{
				artObj2.Trigger(FTrigger.OnEquip);
			}
		}
		foreach (Player currentPlayer in ctrl.currentPlayers)
		{
			foreach (Cpu currentPet in currentPlayer.currentPets)
			{
				if (!currentPlayer.artObjs.Contains(currentPet.parentObj))
				{
					currentPet.StartDeath(false);
				}
			}
			statsScreen.UpdateStats(currentPlayer);
			statsScreen.UpdateStatsText(currentPlayer);
		}
	}

	public void TriggerAllArtifacts(FTrigger fTrigger, BattleGrid theBattleGrid = null, Being forwardedTargetHit = null, int forwardedHitAmount = 0)
	{
		if (theBattleGrid == null)
		{
			theBattleGrid = ctrl.ti.mainBattleGrid;
		}
		if (theBattleGrid == null)
		{
			return;
		}
		foreach (Being currentBeing in theBattleGrid.currentBeings)
		{
			currentBeing.TriggerArtifacts(fTrigger, forwardedTargetHit, forwardedHitAmount);
		}
	}

	public void UpdatePacts()
	{
		for (int num = activePacts.Count - 1; num >= 0; num--)
		{
			activePacts[num].duration--;
			if (activePacts[num].duration < 1)
			{
				activePacts[num].FinishPact();
			}
		}
	}

	public void ClearPlayerItems(int pvpNum)
	{
		switch (pvpNum)
		{
		case 0:
			deckScreen.ClearAll();
			artGrid.DestroyChildren();
			pactGrid.DestroyChildren();
			pactCardList.Clear();
			artCardList.Clear();
			activePacts.Clear();
			{
				foreach (DuelDisk duelDisk in duelDisks)
				{
					duelDisk.Clear();
				}
				break;
			}
		case 1:
			playerTwoArtGrid.DestroyChildren();
			break;
		}
	}

	public void DestroyDuelDisks()
	{
		for (int num = duelDisks.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(duelDisks[num].gameObject);
		}
		duelDisks.Clear();
	}

	public DuelDisk CreatePlayerDuelDisk(Player player, bool reference = false)
	{
		DuelDisk duelDisk = UnityEngine.Object.Instantiate(duelDiskPrefab, ctrl.duelDisksContainer.position, ctrl.duelDisksContainer.rotation, ctrl.duelDisksContainer);
		duelDisk.beingObj = player.beingObj;
		duelDisk.deCtrl = this;
		duelDisk.ctrl = ctrl;
		duelDisk.statsScreen = statsScreen;
		duelDisk.player = player;
		duelDisk.nameText.text = player.beingObj.localizedName;
		if (reference)
		{
			duelDisk.transform.position = S.I.refCtrl.transform.position;
			duelDisk.transform.SetParent(S.I.refCtrl.transform);
			return duelDisk;
		}
		if (ctrl.pvpMode)
		{
			duelDisk.PvPMode(duelDisks.Count);
		}
		ctrl.gameOverPane.playerSplashes[duelDisks.Count].sprite = itemMan.GetSprite(player.beingObj.splashSprite);
		duelDisks.Add(duelDisk);
		return duelDisk;
	}

	public void SetupPlayerDeck(Player thePlayer)
	{
		thePlayer.duelDisk.manualShuffleDisabled = false;
		S.I.shopCtrl.sera = thePlayer.beingObj.money;
	}

	public void CreatePlayerItems(Player player, int pvpNum = 0, int coOpNum = 0)
	{
		BeingObject beingObj = player.beingObj;
		if (coOpNum == 0)
		{
			ClearPlayerItems(pvpNum);
			if (S.I.scene == GScene.SpellLoop)
			{
				int num = 0;
				foreach (SpellObject playerSpell in itemMan.playerSpellList)
				{
					if (!playerSpell.tags.Contains(Tag.SkipTest))
					{
						CreateSpellCard(playerSpell, deckScreen.deckGrid, num, player.duelDisk);
						num++;
					}
				}
			}
			else
			{
				List<string> list = new List<string>();
				if (S.I.FULL_DECK)
				{
					foreach (SpellObject playerSpell2 in itemMan.playerSpellList)
					{
						list.Add(playerSpell2.itemID);
					}
				}
				else
				{
					foreach (string item in beingObj.deck)
					{
						list.Add(item);
					}
				}
				for (int i = 0; i < list.Count; i++)
				{
					if (itemMan.spellDictionary.ContainsKey(list[i]))
					{
						ListCard listCard = CreateSpellCard(itemMan.spellDictionary[list[i]].Clone(), deckScreen.deckGrid, i, player.duelDisk);
						if (runCtrl.LoadedRunExists())
						{
							if (runCtrl.loadedRun.spellData[i].enhancements.Count > 0)
							{
								foreach (Enhancement enhancement in runCtrl.loadedRun.spellData[i].enhancements)
								{
									S.I.poCtrl.EnhanceSpell(listCard.spellObj, enhancement);
								}
								listCard.nameText.text = listCard.spellObj.nameString;
							}
							listCard.spellObj.damage = runCtrl.loadedRun.spellData[i].damage;
							listCard.spellObj.permDamage = runCtrl.loadedRun.spellData[i].permDamage;
						}
						Debug.Log("Getting Spell " + listCard.spellObj.itemID);
					}
					else
					{
						Debug.LogError("\"" + list[i] + "\" does not exist in Spell Dictionary.");
					}
				}
			}
			if (S.I.FULL_ARTS)
			{
				for (int j = 0; j < itemMan.baseArtList.Count; j++)
				{
					CreateNewPlayerArtifactCard(CreateArtifact(itemMan.baseArtList[j].itemID, player), j, player.duelDisk);
				}
				for (int k = 0; k < itemMan.nonBaseArtList.Count; k++)
				{
					if (!itemMan.nonBaseArtList[k].tags.Contains(Tag.SkipTest))
					{
						CreateNewPlayerArtifactCard(CreateArtifact(itemMan.nonBaseArtList[k].itemID, player), k, player.duelDisk, ctrl.sp.currentPlayers.Count - 1);
					}
				}
			}
			else
			{
				for (int l = 0; l < player.artObjs.Count; l++)
				{
					CreateNewPlayerArtifactCard(player.artObjs[l], l, player.duelDisk, pvpNum);
					Debug.Log("Getting Artifact " + player.artObjs[l].itemID);
				}
			}
			if (!runCtrl.LoadedRunExists())
			{
				for (int m = 0; m < ctrl.runCtrl.currentRun.hellPasses.Count; m++)
				{
					if (ctrl.runCtrl.currentRun.hellPasses[m] != 0)
					{
						if (itemMan.hellPasses.Count > ctrl.runCtrl.currentRun.hellPasses[m])
						{
							CreatePact(itemMan.hellPasses[ctrl.runCtrl.currentRun.hellPasses[m]].ClonePact(), -1, player.duelDisk).pactObj.hellPass = true;
							Debug.Log("Getting Hell Pass " + itemMan.hellPasses[ctrl.runCtrl.currentRun.hellPasses[m]].itemID);
						}
						else
						{
							Debug.LogWarning("create more hell passes " + ctrl.runCtrl.currentRun.hellPasses[m]);
						}
					}
				}
			}
			if (runCtrl.LoadedRunExists() && runCtrl.loadedRun.pactData.Count > 0)
			{
				foreach (PactData pactDatum in runCtrl.loadedRun.pactData)
				{
					CreatePact(pactDatum, -1, player.duelDisk);
				}
			}
		}
		ctrl.shopCtrl.selfMode = player.beingObj.tags.Contains(Tag.Shopkeeper);
		if (!player.beingObj.tags.Contains(Tag.Shopkeeper))
		{
			return;
		}
		if (runCtrl.LoadedRunExists())
		{
			if (coOpNum == 0)
			{
				ctrl.shopCtrl.LoadShopOptions(runCtrl.loadedRun);
				ctrl.shopCtrl.SetShopkeeper(null, ZoneType.Shop, false);
				ctrl.shopCtrl.donateCurrentValue = runCtrl.loadedRun.shopDonateValue;
				ctrl.shopCtrl.UpdateDonateValueText();
				ctrl.shopCtrl.refillAdd = runCtrl.loadedRun.shopRefillAdd;
			}
		}
		else
		{
			ctrl.shopCtrl.SetShopkeeper(null, ZoneType.Shop, true);
			runCtrl.currentRun.yamiObtained = true;
			if (runCtrl.currentHellPassNum >= 14)
			{
				ctrl.shopCtrl.sera = 100;
			}
		}
	}

	public ChoiceCard CreateNewChoiceCard(ItemObject itemObj, Transform parent)
	{
		ChoiceCard component = SimplePool.Spawn(choiceCardPrefab).GetComponent<ChoiceCard>();
		component.transform.SetParent(parent, false);
		component.deCtrl = this;
		component.SetCard(itemObj);
		component.btnCtrl = btnCtrl;
		return component;
	}

	public void RegisterSpellObjectCreated(Action<SpellObject> callbackfunc)
	{
		cbSpellObjectCreated = (Action<SpellObject>)Delegate.Combine(cbSpellObjectCreated, callbackfunc);
	}

	public void UnregisterSpellObjectCreated(Action<SpellObject> callbackfunc)
	{
		cbSpellObjectCreated = (Action<SpellObject>)Delegate.Remove(cbSpellObjectCreated, callbackfunc);
	}

	public void EquipWep(string weaponID, Player thePlayer = null)
	{
		if (thePlayer != null)
		{
			thePlayer.equippedWep = CreateSpellBase(weaponID, thePlayer);
			if (!thePlayer.IsReference())
			{
				deckScreen.weaponListCard.SetWeapon(thePlayer.equippedWep);
			}
			return;
		}
		foreach (Player currentPlayer in ctrl.currentPlayers)
		{
			currentPlayer.equippedWep = CreateSpellBase(weaponID, currentPlayer);
			if (!currentPlayer.IsReference())
			{
				deckScreen.weaponListCard.SetWeapon(currentPlayer.equippedWep);
			}
		}
	}

	private string LocalizedSentenceEnding()
	{
		return SettingsPane.localizedFullStop;
	}

	public string ParseDescription(ItemObject itemObj, string translationID)
	{
		string text = "";
		if (itemObj.type == ItemType.Spell || itemObj.type == ItemType.Wep)
		{
			text = ((S.modsInstalled && !string.IsNullOrEmpty(itemObj.description)) ? itemObj.description : (LocalizationManager.GetTranslation("SpellDescriptions/" + translationID) + LocalizedSentenceEnding()));
			foreach (EffectApp efApp in itemObj.efApps)
			{
				if (efApp.fTrigger == FTrigger.OnCast && generatedDescriptionEffects.Contains(efApp.effect))
				{
					string translation = LocalizationManager.GetTranslation("MechSnippets/" + efApp.effect);
					translation = ParseEffectApp(translation, efApp);
					if (!text.Contains(translation))
					{
						text = text + translation + LocalizedSentenceEnding();
					}
				}
			}
		}
		else if (itemObj.type == ItemType.Art)
		{
			text = ((S.modsInstalled && !string.IsNullOrEmpty(itemObj.description)) ? itemObj.description : (LocalizationManager.GetTranslation("ArtDescriptions/" + translationID) + LocalizedSentenceEnding()));
			if (text.Contains("efApp.spellPower"))
			{
				text = text.Replace("efApp.spellPower", "<b>" + itemObj.artObj.spellPower + "</b>");
			}
		}
		else if (itemObj.type == ItemType.Pact)
		{
			text = ((!S.modsInstalled || !string.IsNullOrEmpty(LocalizationManager.GetTranslation("PactDescriptions/" + translationID))) ? (LocalizationManager.GetTranslation("PactDescriptions/" + translationID) + LocalizedSentenceEnding()) : ((!itemMan.pactDictionary.ContainsKey(translationID)) ? itemObj.description : itemMan.pactDictionary[translationID].description));
		}
		if (string.IsNullOrEmpty(text))
		{
			text = itemObj.description;
		}
		text = text.Replace("<hg>", "<b>" + U.I.GetColorStarter(UIColor.Effect));
		text = text.Replace("</hg>", "</color></b>");
		if (S.modsInstalled)
		{
			text = text.Replace("%hg%", "<b>" + U.I.GetColorStarter(UIColor.Effect));
			text = text.Replace("%/hg%", "</color></b>");
		}
		if (text.Contains("sp.PermDamage") && itemObj.spellObj != null)
		{
			text = text.Replace("sp.PermDamage", U.I.Colorify("(" + itemObj.spellObj.permDamage + ")", UIColor.Effect));
		}
		foreach (string highlightedMech in highlightedMechs)
		{
			if (text.Contains("ef." + highlightedMech))
			{
				if (string.IsNullOrEmpty(LocalizationManager.GetTranslation("MechKeys/" + highlightedMech)))
				{
					Debug.LogError("NO LOCALIZATION FOR " + highlightedMech);
				}
				text = text.Replace("ef." + highlightedMech, "<b>" + U.I.Colorify(LocalizationManager.GetTranslation("MechKeys/" + highlightedMech), UIColor.Effect) + "</b>");
			}
		}
		if (itemObj.type == ItemType.Spell && itemObj.spellObj.enhancements.Count > 0)
		{
			string text2 = "";
			foreach (Enhancement enhancement in itemObj.spellObj.enhancements)
			{
				text2 = text2 + " (" + LocalizationManager.GetTranslation("Enhancements/" + enhancement) + ") ";
			}
			text += U.I.Colorify(text2, UIColor.Enhancement);
		}
		return text;
	}

	public string ParseEffectApp(string effDescription, EffectApp efApp = null, bool effect = true)
	{
		if (effDescription == null)
		{
			effDescription = string.Empty;
		}
		if (!string.IsNullOrEmpty(efApp.value))
		{
			effDescription = ((LocalizationManager.GetTranslation("SpellNames/" + efApp.value) != null) ? effDescription.Replace("efApp.value", LocalizationManager.GetTranslation("SpellNames/" + efApp.value)) : ((LocalizationManager.GetTranslation("BeingNames/" + efApp.value) == null) ? effDescription.Replace("efApp.value", efApp.value) : effDescription.Replace("efApp.value", LocalizationManager.GetTranslation("BeingNames/" + efApp.value))));
		}
		float num = efApp.amount;
		if (num == 0f)
		{
			num = 1f;
		}
		if (string.IsNullOrEmpty(effDescription))
		{
			Debug.LogWarning(string.Concat("description is null for ", efApp.fTrigger, " ", efApp.effect));
		}
		if (effDescription.Contains("efApp.amount"))
		{
			effDescription = ((efApp.amountApp == null || efApp.amountApp.multiplier == 1f) ? effDescription.Replace("efApp.amount", "<b>" + Mathf.Abs(num) + "</b>") : effDescription.Replace("efApp.amount", "<b>x</b>"));
		}
		effDescription = effDescription.Replace("<hg>", "<b>" + U.I.GetColorStarter(UIColor.Effect));
		effDescription = effDescription.Replace("</hg>", "</color></b>");
		effDescription = effDescription.Replace("efApp.current", efApp.current.ToString());
		effDescription = effDescription.Replace("efApp.limit", efApp.limit.ToString());
		effDescription = effDescription.Replace("efApp.chance", (efApp.chance * 100f).ToString());
		effDescription = effDescription.Replace("efApp.triggerAmount", efApp.triggerAmount.ToString());
		effDescription = effDescription.Replace("efApp.triggerCooldown", efApp.triggerCooldown.ToString());
		Target target = efApp.target;
		if (efApp.target == Target.Default)
		{
			target = Target.Hit;
		}
		effDescription = ((LocalizationManager.GetTranslation("Target/" + target) == null) ? effDescription.Replace("efApp.target", target.ToString()) : effDescription.Replace("efApp.target", LocalizationManager.GetTranslation("Target/" + target)));
		string text = efApp.fTrigger.ToString();
		if (LocalizationManager.GetTranslation("MechKeys/" + text) != null)
		{
			text = LocalizationManager.GetTranslation("MechKeys/" + text);
		}
		effDescription = effDescription.Replace("efApp.fTrigger", "<b>" + text + "</b>");
		string text2 = efApp.effect.ToString();
		if (LocalizationManager.GetTranslation("MechKeys/" + text2) != null)
		{
			text2 = LocalizationManager.GetTranslation("MechKeys/" + text2);
		}
		effDescription = effDescription.Replace("efApp.fTrigger", "<b>" + U.I.Colorify(text, UIColor.Effect) + "</b>");
		effDescription = effDescription.Replace("efApp.effect", "<b>" + U.I.Colorify(text2, UIColor.Effect) + "</b>");
		effDescription = effDescription.Replace("BC.hasteAmount", optCtrl.settingsPane.Percentify(Mathf.FloorToInt(BC.hasteAmount * 1000f)));
		effDescription = effDescription.Replace("BC.flameTotalDamage", Mathf.FloorToInt(BC.flameDmg * BC.flameTicks).ToString());
		effDescription = effDescription.Replace("BC.flameDuration", Mathf.FloorToInt((float)BC.flameTicks * BC.flameTickTime).ToString());
		effDescription = effDescription.Replace("BC.fragileMultiplier", BC.fragileMultiplier + "x");
		effDescription = effDescription.Replace("BC.shieldDecay", optCtrl.settingsPane.Percentify(BC.shieldDecay * 100f));
		effDescription = effDescription.Replace("BC.frostDmg", BC.frostDmg.ToString());
		effDescription = effDescription.Replace("BC.poisonDuration", BC.poisonDuration.ToString());
		return effDescription;
	}

	private void FillShows()
	{
		effectTooltips.AddRange(new List<Effect>
		{
			Effect.Anchor,
			Effect.AtkDmg,
			Effect.AtkDmgBattle,
			Effect.Backfire,
			Effect.Channel,
			Effect.Consume,
			Effect.Defense,
			Effect.DefenseBattle,
			Effect.Flame,
			Effect.FlowSet,
			Effect.FlowStack,
			Effect.Fragile,
			Effect.Frost,
			Effect.Haste,
			Effect.Link,
			Effect.Luck,
			Effect.ManaRegen,
			Effect.ManaRegenBattle,
			Effect.Pet,
			Effect.Poison,
			Effect.Reflect,
			Effect.Removal,
			Effect.Root,
			Effect.Shield,
			Effect.Slow,
			Effect.SpellPower,
			Effect.SpellPowerBattle,
			Effect.Summon,
			Effect.TileBreak,
			Effect.TileCrack,
			Effect.TriggerFlow,
			Effect.Trinity,
			Effect.Upgrader
		});
		triggerTooltips.AddRange(new List<FTrigger>
		{
			FTrigger.Flow,
			FTrigger.TrinityCast,
			FTrigger.OnConsume,
			FTrigger.OnFlow,
			FTrigger.OnPoison,
			FTrigger.OnPoisonDmg
		});
		highlightedMechs.AddRange(new List<string>
		{
			"AtkDmg", "AtkDmgBattle", "Backfire", "Consume", "DefenseBattle", "Defense", "FlowStack", "Flow", "Frost", "Flames",
			"Flame", "Fragile", "Frost", "Haste", "Heal", "Jam", "Link", "Luck", "ManaRegen", "Mana",
			"MaxMana", "MaxHPChange", "Money", "OnFlow", "Poison", "Reflect", "Removal", "Root", "Shield", "ShuffleTime",
			"Shuffle", "SpellPower", "TileBreak", "TileCrack", "Trinity", "Upgrader"
		});
		generatedDescriptionEffects.AddRange(new List<Effect>
		{
			Effect.Anchor,
			Effect.Consume,
			Effect.FlowStack,
			Effect.Jam
		});
	}
}
