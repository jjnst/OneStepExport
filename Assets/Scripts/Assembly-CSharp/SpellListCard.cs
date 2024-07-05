using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellListCard : ListCard, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	public Transform originalParent = null;

	public Transform parentToReturnTo = null;

	public Transform placeholderParent = null;

	private GameObject placeholder = null;

	public AudioClip pickUpSound;

	public AudioClip dropSound;

	public int originalListNum;

	public RectTransform container;

	public LayoutElement layoutElement;

	public CanvasGroup upgradeContainer;

	public TMP_Text upgradeCount;

	public Animator upgradeCountAnim;

	private bool holdingUpgrade = false;

	private bool holdingRemove = false;

	private bool mouseFocus = false;

	public void Start()
	{
		deckScreen.CalculateGridHeight(base.transform.parent);
	}

	protected override void Update()
	{
		base.Update();
		if (!(btnCtrl.focusedButton == this))
		{
			return;
		}
		if (holdingUpgrade)
		{
			if (btnCtrl.activeNavPanels.Contains(S.I.poCtrl))
			{
				holdingUpgrade = false;
				return;
			}
			if (deckScreen.upgradeSpellFill.fillAmount < 1f)
			{
				if ((bool)deCtrl.ctrl.currentPlayer && deCtrl.ctrl.GameState != GState.GameOver)
				{
					deckScreen.upgradeSpellFill.fillAmount += Time.deltaTime / btnCtrl.holdDuration;
				}
			}
			else
			{
				UpgradeSpell();
				deckScreen.upgradeSpellFill.fillAmount = 0f;
			}
			holdingUpgrade = false;
		}
		else if (deckScreen.upgradeSpellFill.fillAmount > 0f)
		{
			deckScreen.upgradeSpellFill.fillAmount -= Time.deltaTime / btnCtrl.holdDuration;
		}
		if (holdingRemove)
		{
			if (deckScreen.removeSpellFill.fillAmount < 1f)
			{
				deckScreen.removeSpellFill.fillAmount += Time.deltaTime / btnCtrl.holdDuration;
			}
			else
			{
				RemoveSpell();
				deckScreen.removeSpellFill.fillAmount = 0f;
			}
			holdingRemove = false;
		}
		else if (deckScreen.removeSpellFill.fillAmount > 0f)
		{
			deckScreen.removeSpellFill.fillAmount -= Time.deltaTime / btnCtrl.holdDuration;
		}
	}

	public override void Up()
	{
		if (parentList.IndexOf(this) > 0)
		{
			btnCtrl.SetFocus(parentList[parentList.IndexOf(this) - 1]);
		}
		else
		{
			btnCtrl.SetFocus(deckScreen.foCtrl.brandDisplayButtons[0]);
		}
	}

	public override void Down()
	{
		if (parentList.IndexOf(this) < parentList.Count - 1)
		{
			btnCtrl.SetFocus(parentList[parentList.IndexOf(this) + 1]);
		}
		else
		{
			btnCtrl.SetFocus(parentList[0]);
		}
	}

	public override void Left()
	{
		base.Left();
		if ((bool)left && !left.GetComponent<SquareListCard>())
		{
			deCtrl.displayCard.Hide();
		}
	}

	public override void Right()
	{
		if (deCtrl.artGrid.childCount > 0)
		{
			btnCtrl.SetFocus(deCtrl.artGrid.GetChild(0).gameObject);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (IsHoverable() && !deckScreen.busy && deckScreen.open)
		{
			if (itemObj != null)
			{
				hovering = true;
			}
			mouseFocus = true;
			btnCtrl.SetFocus(this);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (deCtrl.btnCtrl.mouseActive)
		{
			mouseFocus = false;
			base.OnPointerExit(eventData);
		}
	}

	public override void OnAcceptPress()
	{
	}

	public override void OnBackPress()
	{
	}

	public void UpgradeSpell()
	{
		if ((bool)deCtrl.ctrl.currentPlayer && deCtrl.ctrl.GameState != GState.GameOver)
		{
			UpgradeThisCard();
		}
	}

	public override void UpgradeSpellHold()
	{
		holdingUpgrade = true;
	}

	public override void RemoveSpellHold()
	{
		if (!placeholder)
		{
			holdingRemove = true;
		}
	}

	public void RemoveSpell()
	{
		deckScreen.RemoveCard(this);
	}

	public override void Focus(int playerNum = 0)
	{
		base.Focus(playerNum);
		S.I.PlayOnce(btnCtrl.flipSound);
		container.position += Vector3.left * 2f;
		deCtrl.statsScreen.artCursor.Hide();
		deCtrl.displayCard.ShowDisplayCard(rect, itemObj);
		if (!mouseFocus && base.transform.parent == deckScreen.deckGrid)
		{
			float value = 1f - (float)parentList.IndexOf(this) * 1f / (float)Mathf.Clamp(duelDisk.deck.Count - 1, 1, duelDisk.deck.Count - 1);
			deckScreen.deckScrollbar.value = value;
		}
		deckScreen.spellCursor.SetTarget(rect);
		deckScreen.CalculateGridHeight(base.transform.parent);
		if ((bool)deckScreen.runCtrl.ctrl.currentPlayer)
		{
			UpdateUpgraderCostText();
			anim.SetBool("focused", true);
		}
	}

	public override void UnFocus()
	{
		base.UnFocus();
		container.position += Vector3.left * -2f;
		deckScreen.spellCursor.Hide();
		if ((bool)deCtrl && (bool)deCtrl.displayCard)
		{
			deCtrl.displayCardAlt.Hide();
		}
		anim.SetBool("focused", false);
	}

	public void UpdateUpgraderCostText()
	{
		upgradeCount.text = string.Format("{0}x", spellObj.enhancements.Count + 1);
		if (deckScreen.runCtrl.currentRun.upgraders < spellObj.enhancements.Count + 1)
		{
			upgradeContainer.alpha = 0.3f;
		}
		else
		{
			upgradeContainer.alpha = 1f;
		}
	}

	public void UpgradeThisCard(bool useUpgrader = true)
	{
		if (deckScreen.busy || deckScreen.upgradeInProgress || btnCtrl.activeNavPanels.Contains(S.I.poCtrl))
		{
			return;
		}
		holdingWeapon = false;
		if (itemObj.type == ItemType.Spell)
		{
			S.I.refCtrl.Hide();
		}
		if (useUpgrader)
		{
			if (deCtrl.runCtrl.currentRun.upgraders < spellObj.enhancements.Count + 1)
			{
				S.I.PlayOnce(btnCtrl.lockedSound);
				upgradeCountAnim.SetTrigger("FlashRed");
				deckScreen.upgraderAnimator.SetTrigger("FlashRed");
				return;
			}
			deCtrl.runCtrl.currentRun.upgraders -= spellObj.enhancements.Count + 1;
			deckScreen.UpdateUpgraderCountText();
		}
		StartCoroutine(_UpgradeCard());
	}

	private IEnumerator _UpgradeCard()
	{
		btnCtrl.RemoveFocus();
		deckScreen.upgradeInProgress = true;
		deckScreen.busy = true;
		bgImage.material = new Material(deCtrl.upgradeMat);
		CardInner cardInner = deCtrl.displayCard.cardInner;
		cardInner.background.material = new Material(deCtrl.upgradeMat);
		S.I.PlayOnce(deckScreen.upgradeSound);
		float value = 0f;
		float currentVelocity = 0f;
		float sDamping = 0.35f;
		deCtrl.displayCard.dissolving = true;
		while (value < 0.8f)
		{
			value = Mathf.SmoothDamp(value, 1f, ref currentVelocity, sDamping, float.PositiveInfinity);
			cardInner.background.material.SetFloat("_Destroyer_Value_1", value);
			bgImage.material.SetFloat("_Destroyer_Value_1", value);
			cardInner.canvasGroup.alpha = 1f - value * 1.5f;
			yield return null;
		}
		if (itemObj.type == ItemType.Spell)
		{
			S.I.refCtrl.Hide();
		}
		deCtrl.displayCard.dissolving = false;
		deckScreen.busy = false;
		Object.Destroy(cardInner.background.material);
		Object.Destroy(bgImage.material);
		deCtrl.displayCard.Hide();
		deCtrl.displayCardAlt.Hide();
		FinalizeUpgrade();
	}

	public void FinalizeUpgrade()
	{
		int siblingIndex = parentList.IndexOf(this);
		deCtrl.RemoveCardFromDeck(this, false);
		if (deCtrl.runCtrl.worldBar.open)
		{
			deCtrl.runCtrl.worldBar.Close();
		}
		deCtrl.ctrl.poCtrl.StartUpgrade(spellObj, siblingIndex);
		if (duelDisk.currentDeck.Count < 1)
		{
			deckScreen.spellCursor.Hide();
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!deckScreen.busy)
		{
			originalParent = base.transform.parent;
			originalListNum = parentList.IndexOf(this);
			placeholder = new GameObject("Placeholder");
			LayoutElement layoutElement = placeholder.AddComponent<LayoutElement>();
			LayoutElement component = GetComponent<LayoutElement>();
			layoutElement.preferredWidth = component.preferredWidth;
			layoutElement.minHeight = component.minHeight;
			layoutElement.preferredHeight = component.preferredHeight;
			layoutElement.flexibleWidth = 0f;
			layoutElement.flexibleHeight = 0f;
			S.I.PlayOnce(pickUpSound);
			placeholder.transform.SetSiblingIndex(parentList.IndexOf(this));
			parentToReturnTo = base.transform.parent;
			placeholderParent = parentToReturnTo;
			base.transform.SetParent(deckScreen.transform);
			GetComponent<CanvasGroup>().blocksRaycasts = false;
			GetComponent<CanvasGroup>().alpha = 0.6f;
			if (placeholderParent.name == "DeckGrid")
			{
				base.transform.rotation = Quaternion.AngleAxis(2f, Vector3.forward);
			}
			deckScreen.CalculateGridHeight(parentToReturnTo);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (deckScreen.busy || placeholder == null)
		{
			return;
		}
		if (btnCtrl.focusedButton != this)
		{
			btnCtrl.SetFocus(this);
		}
		deckScreen.spellCursor.SetTarget(rect);
		deCtrl.displayCard.SetCard(spellObj);
		deCtrl.displayCard.Hide();
		deCtrl.displayCardAlt.Hide();
		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(deCtrl.ctrl.idCtrl.canvas.transform as RectTransform, Input.mousePosition, deCtrl.ctrl.idCtrl.canvas.worldCamera, out localPoint);
		base.transform.position = deCtrl.ctrl.idCtrl.canvas.transform.TransformPoint(localPoint);
		if (placeholder.transform.parent != placeholderParent)
		{
			placeholder.transform.SetParent(placeholderParent);
		}
		int num = parentToReturnTo.childCount;
		if (placeholderParent.name == "CollectionGrid")
		{
			for (int i = 0; i < placeholderParent.childCount; i++)
			{
				if (base.transform.position.y > placeholderParent.GetChild(i).position.y)
				{
					num = i;
					if (placeholder.transform.GetSiblingIndex() < num)
					{
						num--;
					}
					placeholder.transform.SetSiblingIndex(i);
					break;
				}
			}
		}
		else if (placeholderParent.name == "DeckGrid")
		{
			for (int j = 0; j < placeholderParent.childCount; j++)
			{
				if (base.transform.position.y > placeholderParent.GetChild(j).position.y)
				{
					num = j;
					if (placeholder.transform.GetSiblingIndex() < num)
					{
						num--;
					}
					placeholder.transform.SetSiblingIndex(j);
					break;
				}
			}
		}
		else if (parentToReturnTo.name == "RemoveZone")
		{
			Debug.Log("Over sell zone");
		}
		placeholder.transform.SetSiblingIndex(num);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (deckScreen.busy || placeholder == null)
		{
			return;
		}
		deckScreen.CalculateGridHeight(parentToReturnTo);
		base.transform.SetParent(parentToReturnTo);
		base.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
		base.transform.rotation = Quaternion.AngleAxis(0f, Vector3.forward);
		GetComponent<CanvasGroup>().blocksRaycasts = true;
		Object.Destroy(placeholder);
		int siblingIndex = placeholder.transform.GetSiblingIndex() - 1;
		ListCard listCard = null;
		if (originalParent != parentToReturnTo)
		{
			deckScreen.CalculateGridHeight(originalParent);
			S.I.PlayOnce(dropSound);
			if (parentToReturnTo.name == "RemoveZone")
			{
				if (deCtrl.runCtrl.currentRun.removals < 1)
				{
					S.I.PlayOnce(dropSound);
					deCtrl.RemoveCardFromList(this);
					listCard = deCtrl.CreateSpellCard(spellObj, originalParent, siblingIndex, duelDisk);
				}
				else
				{
					RemoveThisCard();
				}
			}
			deCtrl.deckScreen.UpdateInfo();
		}
		else
		{
			S.I.PlayOnce(dropSound);
			deCtrl.RemoveCardFromList(this);
			listCard = deCtrl.CreateSpellCard(spellObj, originalParent, siblingIndex, duelDisk);
		}
		if ((bool)listCard)
		{
			deckScreen.spellCursor.SetTarget(listCard.rect);
		}
		GetComponent<CanvasGroup>().alpha = 1f;
		deckScreen.UpdateButtonNavs();
		deCtrl.ctrl.shopCtrl.RefreshButtonMapping();
	}
}
