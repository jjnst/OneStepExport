using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChoiceCard : UICard
{
	public ShopCtrl shopCtrl;

	public PostCtrl poCtrl;

	public List<ChoiceCard> parentList;

	public RewardType rewardType;

	public int price = 0;

	public int siblingIndex = -1;

	public bool testMode = false;

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (!IsHoverable())
		{
			return;
		}
		if (shopCard && !deCtrl.deckScreen.foCtrl.slideBody.onScreen)
		{
			hovering = true;
			deCtrl.displayCard.Hide();
			deCtrl.statsScreen.artCursor.SetTarget(cardInner.rect);
		}
		else if (rewardType == RewardType.Unlock)
		{
			hovering = true;
			if (!deCtrl.deckScreen.slideBody.onScreen && cardInner.cardBack.alpha < 1f)
			{
				base.OnPointerEnter(eventData);
			}
		}
		else if (!deCtrl.deckScreen.slideBody.onScreen)
		{
			base.OnPointerEnter(eventData);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (shopCard)
		{
			deCtrl.statsScreen.artCursor.Hide();
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if ((eventData == null || eventData.button == PointerEventData.InputButton.Left) && IsHoverable())
		{
			if (shopCard && !deCtrl.deckScreen.foCtrl.slideBody.onScreen)
			{
				ChooseThisCard();
			}
			else if (rewardType == RewardType.Unlock)
			{
				S.I.unCtrl.ShowNextUnlock(parentList.IndexOf(this));
			}
			else if (!deCtrl.deckScreen.slideBody.onScreen)
			{
				ChooseThisCard();
			}
		}
	}

	public override void Up()
	{
		if ((bool)shopCtrl)
		{
			if (up == null)
			{
				if (parentList.IndexOf(this) > 0)
				{
					btnCtrl.SetFocus(parentList[parentList.IndexOf(this) - 1]);
				}
			}
			else
			{
				base.Up();
			}
		}
		else
		{
			base.Up();
		}
	}

	public override void Down()
	{
		if ((bool)shopCtrl)
		{
			if (down == null)
			{
				if (parentList.IndexOf(this) < parentList.Count - 1)
				{
					btnCtrl.SetFocus(parentList[parentList.IndexOf(this) + 1]);
				}
			}
			else
			{
				base.Down();
			}
		}
		else
		{
			base.Down();
		}
	}

	public override void Left()
	{
		if ((bool)shopCtrl)
		{
			base.Left();
		}
		else if (base.transform.GetSiblingIndex() > 0)
		{
			btnCtrl.SetFocus(base.transform.parent.GetChild(base.transform.GetSiblingIndex() - 1));
		}
	}

	public override void Right()
	{
		if ((bool)shopCtrl)
		{
			base.Right();
		}
		else if (base.transform.GetSiblingIndex() < base.transform.parent.childCount - 1)
		{
			btnCtrl.SetFocus(base.transform.parent.GetChild(base.transform.GetSiblingIndex() + 1));
		}
	}

	public override void OnAcceptPress()
	{
		ChooseThisCard();
	}

	public override void OnBackPress()
	{
	}

	public override void OnShufflePress()
	{
		if ((bool)shopCtrl)
		{
			shopCtrl.PokeShopKeeper();
		}
	}

	protected override void Update()
	{
		if (!testMode)
		{
			SetTargetSize();
			Scale(cardInner.rect);
		}
	}

	public override void Focus(int playerNum = 0)
	{
		S.I.PlayOnce(btnCtrl.hoverSound);
		cardInner.CreateMechTooltip(rect);
		PreviewFocus();
		if (shopCard)
		{
			deCtrl.statsScreen.artCursor.SetTarget(cardInner.rect);
		}
		refCtrl.Hide();
	}

	public override void UnFocus()
	{
		holdingWeapon = false;
		if ((bool)cardInner)
		{
			cardInner.mechTooltipGrid.DestroyChildren();
			if (deCtrl != null)
			{
				deCtrl.displayCardAlt.Hide();
			}
			if (itemObj != null && (itemObj.type == ItemType.Spell || itemObj.type == ItemType.Wep))
			{
				refCtrl.Hide();
			}
		}
		if (shopCard)
		{
			deCtrl.statsScreen.artCursor.Hide();
		}
	}

	public void FocusGlow()
	{
		cardInner.brandImage.color = U.I.GetColor(UIColor.Focus);
		cardInner.brandText.color = U.I.GetColor(UIColor.Focus);
	}

	private void ChooseThisCard()
	{
		if (PostCtrl.transitioning || chosen)
		{
			return;
		}
		if (itemObj != null && (itemObj.type == ItemType.Spell || itemObj.type == ItemType.Wep))
		{
			refCtrl.Hide();
		}
		if ((bool)shopCtrl)
		{
			shopCtrl.TryBuyItem(this);
		}
		else
		{
			if (!poCtrl || !(poCtrl.showFocusDelay <= 0f))
			{
				return;
			}
			GetThisCard();
			switch (rewardType)
			{
			case RewardType.Loot:
				poCtrl.EndLoot(rewardType, false);
				if (itemObj.type == ItemType.Spell)
				{
					deCtrl.TriggerAllArtifacts(FTrigger.OnChooseSpell);
				}
				S.I.ana.SendLootPick(deCtrl.runCtrl.currentRun.finishedZones, deCtrl.ctrl.currentPlayer.beingObj.beingID, itemObj, deCtrl.ctrl.currentPlayer.duelDisk.deck.Count);
				break;
			case RewardType.ArtDrop:
				poCtrl.EndLoot(rewardType, false);
				break;
			case RewardType.Blessing:
				poCtrl.EndLoot(rewardType, false);
				break;
			case RewardType.BossSpell:
				poCtrl.EndLoot(rewardType, false);
				if (itemObj.type == ItemType.Spell)
				{
					deCtrl.TriggerAllArtifacts(FTrigger.OnChooseSpell);
				}
				S.I.ana.SendLootPick(deCtrl.runCtrl.currentRun.finishedZones, deCtrl.ctrl.currentPlayer.beingObj.beingID, itemObj, deCtrl.ctrl.currentPlayer.duelDisk.deck.Count);
				break;
			case RewardType.BossArt:
				poCtrl.EndLoot(rewardType, false);
				S.I.ana.SendLootPick(deCtrl.runCtrl.currentRun.finishedZones, deCtrl.ctrl.currentPlayer.beingObj.beingID, itemObj, deCtrl.ctrl.currentPlayer.duelDisk.deck.Count);
				break;
			case RewardType.LevelUp:
				poCtrl.EndLevelUpOptions(false);
				S.I.ana.SendLevelUpPick(deCtrl.runCtrl.currentRun.finishedZones, deCtrl.ctrl.currentPlayer.beingObj.beingID, itemObj, deCtrl.ctrl.currentPlayer.duelDisk.deck.Count);
				break;
			case RewardType.Upgrade:
				if (itemObj.spellObj.enhancements.Count >= 4 && !AchievementsCtrl.IsUnlocked("Eggs_in_a_Basket"))
				{
					AchievementsCtrl.UnlockAchievement("Eggs_in_a_Basket");
				}
				deCtrl.ctrl.camCtrl.Effect("Upgrade", base.transform.position);
				poCtrl.EndUpgradeOptions(false);
				S.I.ana.SendUpgradePick(deCtrl.runCtrl.currentRun.finishedZones, deCtrl.ctrl.currentPlayer.beingObj.beingID, itemObj, deCtrl.ctrl.currentPlayer.duelDisk.deck.Count);
				break;
			}
		}
	}

	public void GetThisCard()
	{
		ListCard listCard = deCtrl.CreatePlayerItem(itemObj, deCtrl.deckScreen.deckGrid, siblingIndex, deCtrl.ctrl.currentPlayer.duelDisk);
		Transform rectTransform = listCard.transform;
		hoverable = false;
		Debug.Log("Getting " + itemObj.itemID);
		if (itemObj.type == ItemType.Spell)
		{
			if (deCtrl.deckScreen.slideBody.onScreen)
			{
				deCtrl.deckScreen.deckScrollbar.value = 0f;
			}
			else
			{
				rectTransform = deCtrl.ctrl.currentPlayer.duelDisk.cardGrid.shuffleTimer.rectTransform;
			}
			if (deCtrl.itemMan.unobtainedPlayerSpellList.Contains(deCtrl.itemMan.spellDictionary[itemObj.itemID]))
			{
				deCtrl.itemMan.unobtainedPlayerSpellList.Remove(deCtrl.itemMan.spellDictionary[itemObj.itemID]);
			}
			if (itemObj.spellObj.rarity >= 4)
			{
				AchievementsCtrl.UnlockAchievement("Calamity");
				deCtrl.runCtrl.currentRun.skinUnlocks.Add("SaffronLea");
			}
			if (!AchievementsCtrl.IsUnlocked("Arch_Sage") || !SaveDataCtrl.Get("SaffronLegacy", false))
			{
				bool flag = true;
				foreach (Brand brandType in deCtrl.itemMan.brandTypes)
				{
					bool flag2 = false;
					foreach (ListCard item in deCtrl.ctrl.currentPlayer.duelDisk.deck)
					{
						if (item.spellObj.brand == brandType || brandType == Brand.None)
						{
							flag2 = true;
						}
					}
					if (!flag2)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					AchievementsCtrl.UnlockAchievement("Arch_Sage");
					deCtrl.runCtrl.currentRun.skinUnlocks.Add("SaffronLegacy");
				}
			}
		}
		else if (itemObj.type == ItemType.Art)
		{
			deCtrl.runCtrl.currentRun.artifactTaken = true;
			listCard.anim.SetBool("spawned", false);
		}
		else if (itemObj.type == ItemType.Pact)
		{
			listCard.anim.SetBool("spawned", false);
			if (deCtrl.activePacts.Count >= 8 + deCtrl.runCtrl.currentRun.hellPasses.Count)
			{
				AchievementsCtrl.UnlockAchievement("Soulless");
			}
			if (listCard.pactObj.duration == 0)
			{
				listCard.anim.SetBool("spawned", true);
				listCard.pactObj.FinishPact();
			}
		}
		if ((bool)deCtrl.ctrl.currentPlayer)
		{
			deCtrl.statsScreen.UpdateStatsText(deCtrl.ctrl.currentPlayer);
		}
		cardInner.CreateCardTrail(rectTransform);
		deCtrl.deckScreen.UpdateButtonNavs();
		chosen = true;
		deCtrl.displayCardAlt.Hide();
	}
}
