using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PvPHeroButton : NavButton
{
	public bool clickable = true;

	public bool selected = true;

	public bool locked = true;

	public CanvasGroup lockImage;

	public List<BeingObject> heroObjs;

	public PvPSelectCtrl heroCtrl;

	public int lastAltIndex = 0;

	public bool alt = false;

	public int assignment = -1;

	protected override void Awake()
	{
		base.Awake();
	}

	public void Set(PvPSelectCtrl heCtrl, BeingObject theHeroObj)
	{
		heroCtrl = heCtrl;
		base.transform.SetParent(heCtrl.heroGrid, false);
		heroObjs = new List<BeingObject>();
		for (int i = 0; i < 2; i++)
		{
			heroObjs.Add(theHeroObj.Clone());
		}
		btnCtrl = heCtrl.btnCtrl;
		tmpText.text = theHeroObj.localizedName;
		if (!SaveDataCtrl.Get(heroObjs[0].beingID, false) && !heroObjs[0].tags.Contains(Tag.Unlock) && !heCtrl.tournamentMode)
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
			btnCtrl.SetFocus(heroCtrl.unlockedHeroButtons[heroCtrl.unlockedHeroButtons.IndexOf(this) - 1], btnCtrl.lastInputPlayerIndex);
		}
		else
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroButtons[heroCtrl.unlockedHeroButtons.Count - 1], btnCtrl.lastInputPlayerIndex);
		}
	}

	public override void Down()
	{
		S.I.PlayOnce(btnCtrl.hoverSound);
		if (heroCtrl.unlockedHeroButtons.IndexOf(this) < heroCtrl.unlockedHeroButtons.Count - 1)
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroButtons[heroCtrl.unlockedHeroButtons.IndexOf(this) + 1], btnCtrl.lastInputPlayerIndex);
		}
		else
		{
			btnCtrl.SetFocus(heroCtrl.unlockedHeroButtons[0], btnCtrl.lastInputPlayerIndex);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (locked || !IsHoverable())
		{
			return;
		}
		foreach (PvPHeroButton heroButton in heroCtrl.heroButtons)
		{
			heroButton.UnFocus();
		}
		base.OnPointerEnter(eventData);
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
			btnCtrl.lastInputPlayerIndex = 0;
			OnAcceptPress();
		}
	}

	public override void OnAcceptPress()
	{
		if (heroCtrl.CurrentHeroSkinIsUnlocked(btnCtrl.lastInputPlayerIndex))
		{
			S.I.PlayOnce(btnCtrl.chooseSound);
			if (btnCtrl.lastInputPlayerIndex == 0)
			{
				heroCtrl.startButton.back = this;
			}
			else if (btnCtrl.lastInputPlayerIndex == 1)
			{
				heroCtrl.startButton.playerTwoBack = this;
			}
			StartColorCo(U.I.GetColor(UIColor.Pink));
			heroCtrl.heroAnimators[btnCtrl.lastInputPlayerIndex].SetTrigger("spellCast");
			heroCtrl.heroAnimators[btnCtrl.lastInputPlayerIndex].SetTrigger("toIdle");
			btnCtrl.SetFocus(heroCtrl.startButton, btnCtrl.lastInputPlayerIndex);
			heroCtrl.selectorOffsetAdd[btnCtrl.lastInputPlayerIndex] = 0f;
			heroCtrl.changeSkinButtons[btnCtrl.lastInputPlayerIndex].alpha = 0.5f;
			heroCtrl.lastChosenHeroIndexes[btnCtrl.lastInputPlayerIndex] = base.transform.GetSiblingIndex();
		}
		else
		{
			heroCtrl.FlashSkinError(btnCtrl.lastInputPlayerIndex);
		}
	}

	public override void Focus(int playerNum = 0)
	{
		base.Focus(playerNum);
		if ((bool)heroCtrl)
		{
			heroCtrl.focusedHeroButtons[playerNum] = this;
			if (heroCtrl.currentDisplayedHeros[playerNum] != heroObjs[playerNum])
			{
				heroCtrl.currentDisplayedHeros[playerNum] = heroObjs[playerNum];
				heroCtrl.DisplayHero(playerNum, heroObjs[playerNum], this, alt, true);
				heroCtrl.heroAnimators[playerNum].SetTrigger("spawn");
			}
			heroCtrl.selectorOffsetAdd[playerNum] = heroCtrl.offsetAdd;
			if (heroObjs != null && heroObjs.Count > btnCtrl.lastInputPlayerIndex)
			{
				heroCtrl.ShowOutfitButton(btnCtrl.lastInputPlayerIndex, heroObjs[btnCtrl.lastInputPlayerIndex]);
			}
		}
	}

	public override void UnFocus()
	{
		base.UnFocus();
	}

	public override void OnShufflePress()
	{
		if (heroObjs[btnCtrl.lastInputPlayerIndex].allAnims.Count > 1)
		{
			S.I.PlayOnce(btnCtrl.chooseSound);
			int num = heroObjs[btnCtrl.lastInputPlayerIndex].allAnims.IndexOf(heroObjs[btnCtrl.lastInputPlayerIndex].animName) + 1;
			if (num >= heroObjs[btnCtrl.lastInputPlayerIndex].allAnims.Count)
			{
				num = 0;
			}
			heroObjs[btnCtrl.lastInputPlayerIndex].animName = heroObjs[btnCtrl.lastInputPlayerIndex].allAnims[num];
			heroCtrl.SetHeroAnimator(heroObjs[btnCtrl.lastInputPlayerIndex].animName);
			heroCtrl.heroAnimators[btnCtrl.lastInputPlayerIndex].SetTrigger("spawn");
		}
	}

	protected override IEnumerator _RemoveColorNextFrame()
	{
		yield return new WaitForEndOfFrame();
		if (this == btnCtrl.focusedButton || this == btnCtrl.playerTwoFocusedButton)
		{
			yield break;
		}
		PvPHeroButton[] focusedHeroButtons = heroCtrl.focusedHeroButtons;
		foreach (PvPHeroButton heroButton in focusedHeroButtons)
		{
			if (heroButton == this)
			{
				yield break;
			}
		}
		int playerInt = 0;
		if (heroCtrl != null && heroCtrl.currentDisplayedHeros[playerInt] != heroObjs[playerInt])
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
