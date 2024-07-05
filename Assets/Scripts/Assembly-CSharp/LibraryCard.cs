using I2.Loc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LibraryCard : UICard
{
	public LibraryCtrl libCtrl;

	public LayoutElement layoutElement;

	private int cardsPerRow = 5;

	public bool hidden = true;

	private void OnEnable()
	{
		rect.localScale = Vector3.one * 0.82f;
	}

	public override void Up()
	{
		cardsPerRow = libCtrl.GetCardsPerRow();
		if (up == null)
		{
			if (base.transform.GetSiblingIndex() > cardsPerRow - 1)
			{
				btnCtrl.SetFocus(base.transform.parent.GetChild(base.transform.GetSiblingIndex() - cardsPerRow));
			}
			else
			{
				btnCtrl.SetFocus(base.transform.parent.GetChild(base.transform.parent.childCount + base.transform.GetSiblingIndex() - cardsPerRow));
			}
		}
		UpdateScrollbar();
	}

	public override void Down()
	{
		cardsPerRow = libCtrl.GetCardsPerRow();
		if (down == null)
		{
			if (base.transform.GetSiblingIndex() < base.transform.parent.childCount - cardsPerRow)
			{
				btnCtrl.SetFocus(base.transform.parent.GetChild(base.transform.GetSiblingIndex() + cardsPerRow));
			}
			else
			{
				btnCtrl.SetFocus(base.transform.parent.GetChild(base.transform.GetSiblingIndex() + cardsPerRow - base.transform.parent.childCount));
			}
		}
		UpdateScrollbar();
	}

	public override void Left()
	{
		cardsPerRow = libCtrl.GetCardsPerRow();
		if (left == null)
		{
			if (base.transform.GetSiblingIndex() > 0)
			{
				btnCtrl.SetFocus(base.transform.parent.GetChild(base.transform.GetSiblingIndex() - 1));
				UpdateScrollbar();
			}
		}
		else
		{
			base.Left();
		}
	}

	public override void Right()
	{
		cardsPerRow = libCtrl.GetCardsPerRow();
		if (right == null)
		{
			if (base.transform.GetSiblingIndex() < base.transform.parent.childCount - 1)
			{
				btnCtrl.SetFocus(base.transform.parent.GetChild(base.transform.GetSiblingIndex() + 1));
				UpdateScrollbar();
			}
		}
		else
		{
			base.Right();
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (IsHoverable())
		{
			btnCtrl.SetFocus(this);
		}
	}

	private void UpdateScrollbar()
	{
		libCtrl.scrollbarTargetValue = 1f - (float)(btnCtrl.focusedButton.transform.GetSiblingIndex() / cardsPerRow) / ((float)(base.transform.parent.childCount / cardsPerRow) * 1f) - 0.028f * (float)btnCtrl.focusedButton.transform.GetSiblingIndex() / (float)base.transform.parent.childCount * 1f;
	}

	public override void OnAcceptPress()
	{
	}

	public override void OnBackPress()
	{
		if (itemObj.type == ItemType.Spell)
		{
			btnCtrl.SetFocus(libCtrl.librarySpellButton);
		}
		else if (itemObj.type == ItemType.Art)
		{
			btnCtrl.SetFocus(libCtrl.libraryArtButton);
		}
	}

	protected override void Update()
	{
		if (hidden)
		{
			if (CardIsOnScreen())
			{
				Reveal();
			}
			return;
		}
		SetTargetSize();
		if ((bool)cardInner)
		{
			cardInner.gameObject.SetActive(true);
			Scale(cardInner.rect);
		}
		if (!CardIsOnScreen())
		{
			SimplePool.Despawn(cardInner.gameObject);
			anim.SetBool("OnScreen", false);
			cardInner = null;
			hidden = true;
		}
	}

	private bool CardIsOnScreen()
	{
		if (base.transform.position.y > libCtrl.transform.position.y - 200f && base.transform.position.y < libCtrl.transform.position.y + 200f)
		{
			return true;
		}
		return false;
	}

	public void SetHidden(ItemObject theItemObj)
	{
		itemObj = theItemObj;
		hidden = true;
	}

	public void Reveal()
	{
		hidden = false;
		SetCard(itemObj);
		anim.SetBool("OnScreen", true);
		if (S.I.itemMan.unlocks.Contains(itemObj) && itemObj.unlockLevel > S.I.unCtrl.currentUnlockLevel)
		{
			cardInner.nameText.text = ScriptLocalization.UI.Library_LockedName;
			if (S.I.readabilityModeEnabled)
			{
				cardInner.descriptionReadable.text = ScriptLocalization.UI.Library_UnlockedAtLevel + " " + itemObj.unlockLevel;
			}
			else
			{
				cardInner.description.text = ScriptLocalization.UI.Library_UnlockedAtLevel + " " + itemObj.unlockLevel;
			}
			cardInner.canvasGroup.alpha = libCtrl.lockedCardsTransparency;
		}
	}

	public override void Focus(int playerNum = 0)
	{
		if (hidden)
		{
			SetCard(itemObj);
		}
		else
		{
			PreviewFocus();
		}
		S.I.PlayOnce(btnCtrl.hoverSound);
	}

	public override void UnFocus()
	{
		holdingWeapon = false;
		if ((bool)cardInner)
		{
			cardInner.mechTooltipGrid.DestroyChildren();
			deCtrl.displayCardAlt.Hide();
		}
	}
}
