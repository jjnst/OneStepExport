using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HeroAltButton : NavButton
{
	public bool clickable = true;

	public bool selected = true;

	public BeingObject heroObj;

	public HeroSelectCtrl heroCtrl;

	public HeroButton currentHeroButton;

	public CanvasGroup lockImage;

	public bool locked = false;

	private void Start()
	{
	}

	public override void Up()
	{
		S.I.PlayOnce(btnCtrl.hoverSound);
		if (heroCtrl.unlockedHeroAltButtons.IndexOf(this) > 0)
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroAltButtons[heroCtrl.unlockedHeroAltButtons.IndexOf(this) - 1]);
		}
		else
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroAltButtons[heroCtrl.unlockedHeroAltButtons.Count - 1]);
		}
	}

	public override void Down()
	{
		S.I.PlayOnce(btnCtrl.hoverSound);
		if (heroCtrl.unlockedHeroAltButtons.IndexOf(this) < heroCtrl.unlockedHeroAltButtons.Count - 1)
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroAltButtons[heroCtrl.unlockedHeroAltButtons.IndexOf(this) + 1]);
		}
		else
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroAltButtons[0]);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (locked || !IsHoverable())
		{
			return;
		}
		foreach (HeroAltButton unlockedHeroAltButton in heroCtrl.unlockedHeroAltButtons)
		{
			unlockedHeroAltButton.UnFocus();
		}
		base.OnPointerEnter(eventData);
		heroCtrl.hellPassButton.Hide();
		heroCtrl.ctrl.currentHeroObj = heroObj.Clone();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (!locked)
		{
			base.OnPointerExit(eventData);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!locked && eventData.button == PointerEventData.InputButton.Left)
		{
			OnAcceptPress();
		}
	}

	public void Set(HeroButton theHeroButton, BeingObject theHeroObj, HeroSelectCtrl heroSelectCtrl)
	{
		currentHeroButton = theHeroButton;
		heroCtrl = heroSelectCtrl;
		heroObj = theHeroObj;
		back = theHeroButton;
		if (!SaveDataCtrl.Get(heroObj.beingID, false) && !heroObj.tags.Contains(Tag.Unlock))
		{
			tmpText.color = new Color(0.5f, 0.5f, 0.5f);
			tmpText.text = "??????";
			locked = true;
		}
		else
		{
			heroCtrl.unlockedHeroAltButtons.Add(this);
			locked = false;
			tmpText.color = Color.white;
			tmpText.text = heroObj.title;
			lockImage.alpha = 0f;
		}
	}

	public override void OnAcceptPress()
	{
		if (heroCtrl.CurrentHeroSkinIsUnlocked())
		{
			if (heroCtrl.focusedAltButton != this)
			{
				if (heroCtrl.focusedAltButton != null)
				{
					heroCtrl.focusedAltButton.UnFocus();
				}
				btnCtrl.SetFocus(this);
				heroCtrl.UpdateAltSelectors(true);
			}
			S.I.PlayOnce(btnCtrl.chooseSound);
			heroCtrl.startButton.back = this;
			StartColorCo(U.I.GetColor(UIColor.Pink));
			heroCtrl.FocusHellPassButton(this);
			heroCtrl.ctrl.currentHeroObj = heroObj.Clone();
		}
		else
		{
			heroCtrl.FlashSkinError();
		}
	}

	public override void ChooseZonePress()
	{
		if (heroCtrl.heroDisplay.wepGrid.childCount > 0)
		{
			btnCtrl.SetFocus(heroCtrl.heroDisplay.wepGrid.GetChild(0));
		}
		else if (heroCtrl.heroDisplay.artsGrid.childCount > 0)
		{
			btnCtrl.SetFocus(heroCtrl.heroDisplay.artsGrid.GetChild(0));
		}
		else if (heroCtrl.heroDisplay.spellsGrid.childCount > 0)
		{
			btnCtrl.SetFocus(heroCtrl.heroDisplay.spellsGrid.GetChild(0));
		}
		heroCtrl.changeSkinButton.alpha = 0.4f;
		foreach (HeroItemListCard itemCard in heroCtrl.itemCards)
		{
			itemCard.back = this;
		}
	}

	public override void OnOutfitPress()
	{
		currentHeroButton.OnOutfitPress();
	}

	public override void Focus(int playerNum = 0)
	{
		back = heroCtrl.focusedHeroButton;
		SaveDataCtrl.Set(heroCtrl.focusedHeroButton.heroObj.nameString + "LastAltFocusedIndex", base.transform.GetSiblingIndex());
		heroCtrl.focusedAltButton = this;
		heroCtrl.lastSkinName = SaveDataCtrl.Get(heroObj.beingID + "LastSkinName", "");
		if (heroObj.unlockedAnims.Contains(heroCtrl.lastSkinName))
		{
			heroObj.animName = heroCtrl.lastSkinName;
		}
		if (heroCtrl.currentDisplayedHero != heroObj)
		{
			heroCtrl.DisplayHero(heroObj, currentHeroButton, true);
		}
		heroCtrl.ShowOutfitButton(heroObj);
		heroCtrl.detailsButton.alpha = 1f;
		base.Focus(playerNum);
	}

	public override void UnFocus()
	{
		StartCoroutine(_RemoveColorNextFrame());
		if ((bool)heroCtrl)
		{
			heroCtrl.detailsButton.alpha = 0.1f;
		}
	}

	protected override IEnumerator _RemoveColorNextFrame()
	{
		yield return new WaitForEndOfFrame();
		if ((heroCtrl != null && heroCtrl.currentDisplayedHero != heroObj) || ((bool)btnCtrl.focusedButton && (bool)btnCtrl.focusedButton.GetComponent<HeroButton>()))
		{
			if (locked)
			{
				StartColorCo(new Color(0.5f, 0.5f, 0.5f));
			}
			else
			{
				StartColorCo(Color.white);
			}
		}
	}
}
