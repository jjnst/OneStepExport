using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HeroButton : NavButton
{
	public bool clickable = true;

	public bool selected = true;

	public bool locked = true;

	public CanvasGroup lockImage;

	public BeingObject heroObj;

	public HeroSelectCtrl heroCtrl;

	public int lastAltFocusedIndex = 0;

	public bool alt = false;

	protected override void Awake()
	{
		base.Awake();
	}

	public void Set(HeroSelectCtrl heCtrl, BeingObject theHeroObj)
	{
		heroCtrl = heCtrl;
		base.transform.SetParent(heCtrl.heroGrid, false);
		heroObj = theHeroObj;
		btnCtrl = heCtrl.btnCtrl;
		tmpText.text = theHeroObj.localizedName;
		if (!SaveDataCtrl.Get(heroObj.beingID, false) && !heroObj.tags.Contains(Tag.Unlock))
		{
			tmpText.color = new Color(0.5f, 0.5f, 0.5f);
			locked = true;
			return;
		}
		tmpText.color = Color.white;
		locked = false;
		heroCtrl.unlockedHeroButtons.Add(this);
		lockImage.alpha = 0f;
	}

	public override void Up()
	{
		S.I.PlayOnce(btnCtrl.hoverSound);
		if (heroCtrl.unlockedHeroButtons.IndexOf(this) > 0)
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroButtons[heroCtrl.unlockedHeroButtons.IndexOf(this) - 1]);
		}
		else
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroButtons[heroCtrl.unlockedHeroButtons.Count - 1]);
		}
	}

	public override void Down()
	{
		S.I.PlayOnce(btnCtrl.hoverSound);
		if (heroCtrl.unlockedHeroButtons.IndexOf(this) < heroCtrl.unlockedHeroButtons.Count - 1)
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroButtons[heroCtrl.unlockedHeroButtons.IndexOf(this) + 1]);
		}
		else
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroButtons[0]);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (locked || !IsHoverable())
		{
			return;
		}
		foreach (HeroButton heroButton in heroCtrl.heroButtons)
		{
			heroButton.UnFocus();
		}
		base.OnPointerEnter(eventData);
		heroCtrl.hellPassButton.Hide();
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

	public override void OnAcceptPress()
	{
		if (heroCtrl.focusedHeroButton != this)
		{
			if (heroCtrl.focusedHeroButton != null)
			{
				heroCtrl.focusedHeroButton.UnFocus();
			}
			btnCtrl.SetFocus(this);
			heroCtrl.UpdateHeroSelectors();
		}
		S.I.PlayOnce(btnCtrl.chooseSound);
		heroCtrl.startButton.back = this;
		StartColorCo(U.I.GetColor(UIColor.Pink));
		SaveDataCtrl.Set("LastHeroChosenIndex", base.transform.GetSiblingIndex());
		lastAltFocusedIndex = SaveDataCtrl.Get(heroObj.nameString + "LastAltFocusedIndex", 0);
		if (lastAltFocusedIndex < heroCtrl.heroAltButtonGrid.childCount)
		{
			heroCtrl.FocusAltButtons(this, lastAltFocusedIndex);
		}
		else
		{
			heroCtrl.FocusAltButtons(this, 0);
		}
		heroCtrl.heroAnimator.SetTrigger("spellCast");
		heroCtrl.heroAnimator.SetTrigger("toIdle");
		heroCtrl.UpdateAltSelectors(true);
	}

	public override void Focus(int playerNum = 0)
	{
		heroCtrl.CreateHeroAltButtons(heroObj, this);
		lastAltFocusedIndex = SaveDataCtrl.Get(heroObj.nameString + "LastAltFocusedIndex", 0);
		if (lastAltFocusedIndex < heroCtrl.heroAltButtonGrid.childCount && heroCtrl.unlockedHeroAltButtons.Count > lastAltFocusedIndex)
		{
			heroObj = heroCtrl.unlockedHeroAltButtons[lastAltFocusedIndex].heroObj;
			if (lastAltFocusedIndex > 0)
			{
				alt = true;
			}
			else
			{
				alt = false;
			}
		}
		heroCtrl.ctrl.currentHeroObj = heroObj;
		heroCtrl.focusedHeroButton = this;
		heroCtrl.lastSkinName = SaveDataCtrl.Get(heroCtrl.ctrl.currentHeroObj.beingID + "LastSkinName", "");
		if (heroCtrl.ctrl.currentHeroObj.unlockedAnims.Contains(heroCtrl.lastSkinName))
		{
			heroCtrl.ctrl.currentHeroObj.animName = heroCtrl.lastSkinName;
		}
		if (heroCtrl.currentDisplayedHero == null || heroCtrl.currentDisplayedHero.beingID != heroCtrl.ctrl.currentHeroObj.beingID)
		{
			heroCtrl.DisplayHero(heroCtrl.ctrl.currentHeroObj, this, alt, true);
			heroCtrl.heroAnimator.SetTrigger("spawn");
		}
		base.Focus(playerNum);
	}

	public override void UnFocus()
	{
		base.UnFocus();
	}

	public override void OnOutfitPress()
	{
		if (heroObj.allAnims.Count > 1)
		{
			S.I.PlayOnce(btnCtrl.chooseSound);
			int num = heroObj.allAnims.IndexOf(heroObj.animName) + 1;
			if (num >= heroObj.allAnims.Count)
			{
				num = 0;
			}
			heroObj.animName = heroObj.allAnims[num];
			heroCtrl.lastSkinName = heroObj.animName;
			SaveDataCtrl.Set(heroObj.beingID + "LastSkinName", heroObj.animName);
			heroCtrl.SetHeroAnimator(heroObj.animName);
			heroCtrl.heroAnimator.SetTrigger("spawn");
			if (heroCtrl.ctrl.currentHeroObj.unlockedAnims.Contains(heroCtrl.lastSkinName))
			{
				heroCtrl.ctrl.currentHeroObj.animName = heroCtrl.lastSkinName;
			}
		}
	}

	protected override IEnumerator _RemoveColorNextFrame()
	{
		yield return new WaitForEndOfFrame();
		if (heroCtrl != null && heroCtrl.currentDisplayedHero != heroObj)
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
