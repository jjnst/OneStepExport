using UnityEngine;

public class HeroItemListCard : ListCard
{
	public HeroSelectCtrl heroCtrl;

	public PvPSelectCtrl pvpSelectCtrl;

	public HeroDisplay heroDisplay;

	public void SetHeroSpell(string itemID, HeroDisplay newHeroDisplay)
	{
		SetDeckSpell(deCtrl.itemMan.spellDictionary[itemID], newHeroDisplay.spellsGrid, -1);
		heroDisplay = newHeroDisplay;
	}

	public void SetHeroArtifact(string itemID, HeroDisplay newHeroDisplay)
	{
		SetArt(deCtrl.itemMan.artDictionary[itemID], newHeroDisplay.artsGrid);
		heroDisplay = newHeroDisplay;
	}

	public void SetHeroWeapon(string itemID, HeroDisplay newHeroDisplay)
	{
		SetDeckSpell(deCtrl.itemMan.spellDictionary[itemID], newHeroDisplay.wepGrid, -1);
		heroDisplay = newHeroDisplay;
	}

	public override void Up()
	{
		if (base.transform.parent == heroDisplay.spellsGrid)
		{
			if (heroDisplay.wepGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.wepGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.wepGrid.childCount - 1)));
			}
			else if (heroDisplay.artsGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.artsGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.artsGrid.childCount - 1)));
			}
		}
		else if (base.transform.parent == heroDisplay.artsGrid)
		{
			if (heroDisplay.spellsGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.spellsGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.spellsGrid.childCount - 1)));
			}
			else if (heroDisplay.wepGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.wepGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.wepGrid.childCount - 1)));
			}
		}
		else if (base.transform.parent == heroDisplay.wepGrid)
		{
			if (heroDisplay.artsGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.artsGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.artsGrid.childCount - 1)));
			}
			else if (heroDisplay.spellsGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.spellsGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.spellsGrid.childCount - 1)));
			}
		}
	}

	public override void Down()
	{
		if (base.transform.parent == heroDisplay.spellsGrid)
		{
			if (heroDisplay.artsGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.artsGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.artsGrid.childCount - 1)));
			}
			else if (heroDisplay.wepGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.wepGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.wepGrid.childCount - 1)));
			}
		}
		else if (base.transform.parent == heroDisplay.artsGrid)
		{
			if (heroDisplay.wepGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.wepGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.wepGrid.childCount - 1)));
			}
			else if (heroDisplay.spellsGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.spellsGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.spellsGrid.childCount - 1)));
			}
		}
		else if (base.transform.parent == heroDisplay.wepGrid)
		{
			if (heroDisplay.spellsGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.spellsGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.spellsGrid.childCount - 1)));
			}
			else if (heroDisplay.artsGrid.childCount > 0)
			{
				btnCtrl.SetFocus(heroDisplay.artsGrid.GetChild(Mathf.Clamp(base.transform.GetSiblingIndex(), 0, heroDisplay.artsGrid.childCount - 1)));
			}
		}
	}

	public override void Left()
	{
		if (base.transform.GetSiblingIndex() > 0)
		{
			btnCtrl.SetFocus(base.transform.parent.GetChild(base.transform.GetSiblingIndex() - 1));
		}
	}

	public override void Right()
	{
		if (base.transform.GetSiblingIndex() < base.transform.parent.childCount - 1)
		{
			btnCtrl.SetFocus(base.transform.parent.GetChild(base.transform.GetSiblingIndex() + 1));
		}
	}

	public override void OnAcceptPress()
	{
	}

	public override void ChooseZonePress()
	{
		if ((bool)heroCtrl)
		{
			foreach (Transform item in heroCtrl.heroAltButtonGrid)
			{
				if (item.GetComponent<HeroAltButton>().heroObj == heroCtrl.currentDisplayedHero)
				{
					btnCtrl.SetFocus(item);
				}
			}
			return;
		}
		if (!pvpSelectCtrl)
		{
			return;
		}
		foreach (Transform item2 in pvpSelectCtrl.heroGrid)
		{
			if (item2.GetComponent<HeroAltButton>().heroObj == pvpSelectCtrl.currentDisplayedHeros[btnCtrl.lastInputPlayerIndex])
			{
				btnCtrl.SetFocus(item2);
			}
		}
	}

	public override void Focus(int playerNum = 0)
	{
		anim.SetBool("spawned", true);
		deCtrl.displayCard.ShowDisplayCard(rect, itemObj);
		deCtrl.statsScreen.artCursor.SetTarget(rect);
		base.Focus(playerNum);
	}

	public override void UnFocus()
	{
		base.UnFocus();
		deCtrl.statsScreen.artCursor.Hide();
		if ((bool)deCtrl.displayCard)
		{
			deCtrl.displayCard.Hide();
		}
		if ((bool)deCtrl.displayCardAlt)
		{
			deCtrl.displayCardAlt.Hide();
		}
		GetComponent<CanvasGroup>().alpha = 0.5f;
	}
}
