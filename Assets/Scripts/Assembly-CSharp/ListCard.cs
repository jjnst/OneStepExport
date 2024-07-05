using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListCard : NavButton
{
	public TMP_Text nameText;

	public Image bgImage;

	public List<ListCard> parentList;

	public SpellObject spellObj;

	public ArtifactObject artObj;

	public ItemObject itemObj;

	public PactObject pactObj;

	public DeckCtrl deCtrl;

	public ReferenceCtrl refCtrl;

	public float detailsShowTime = 0.4f;

	protected DeckScreen deckScreen;

	public bool showDisplayCard = true;

	public DuelDisk duelDisk;

	public bool holdingWeapon = false;

	private Coroutine showDetailsCo;

	protected override void Awake()
	{
		deCtrl = S.I.deCtrl;
		deckScreen = deCtrl.deckScreen;
		base.Awake();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (itemObj != null && IsHoverable())
		{
			hovering = true;
			StartShowDetails();
			if (showDisplayCard)
			{
				deCtrl.displayCard.ShowDisplayCard(rect, itemObj);
			}
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		deCtrl.displayCard.Hide();
		deCtrl.displayCardAlt.Hide();
		if (showDetailsCo != null)
		{
			StopCoroutine(showDetailsCo);
		}
	}

	public override void OnPointerClick(PointerEventData data)
	{
	}

	public override void Focus(int playerNum = 0)
	{
		StartShowDetails();
	}

	private void StartShowDetails()
	{
		if (showDetailsCo == null)
		{
			showDetailsCo = StartCoroutine(ShowDetails());
			return;
		}
		showDetailsCo = null;
		showDetailsCo = StartCoroutine(ShowDetails());
	}

	protected IEnumerator ShowDetails()
	{
		if ((bool)deCtrl.displayCard.cardInner)
		{
			deCtrl.displayCard.cardInner.mechTooltipGrid.DestroyChildren();
		}
		yield return new WaitForSeconds(detailsShowTime);
		if ((hovering || btnCtrl.focusedButton == this) && showDisplayCard)
		{
			deCtrl.displayCard.cardInner.CreateMechTooltip(rect, true);
		}
		showDetailsCo = null;
	}

	protected void SetItem(ItemObject itemObj, Transform parentTransform, int siblingIndex = -1)
	{
		base.name = " ListCard " + siblingIndex + " - " + itemObj.nameString;
		base.transform.SetParent(parentTransform, false);
		base.transform.SetSiblingIndex(siblingIndex);
		btnCtrl = S.I.btnCtrl;
		deCtrl = S.I.deCtrl;
		refCtrl = S.I.refCtrl;
		this.itemObj = itemObj;
	}

	public void SetDeckSpell(SpellObject newSpellObj, Transform parentTransform, int siblingIndex, DuelDisk theDuelDisk = null)
	{
		SetItem(newSpellObj, parentTransform, siblingIndex);
		if ((bool)nameText)
		{
			nameText.text = newSpellObj.nameString;
		}
		if ((bool)theDuelDisk)
		{
			duelDisk = theDuelDisk;
		}
		image.sprite = newSpellObj.sprite;
		newSpellObj.spellObj = newSpellObj;
		spellObj = newSpellObj;
	}

	public ListCard SetArt(ArtifactObject artObj, Transform parentTransform, int siblingIndex = -1, bool setSpawned = false)
	{
		SetItem(artObj, parentTransform, siblingIndex);
		base.name = " ListCard " + siblingIndex + " - " + artObj.nameString;
		bgImage.sprite = artObj.sprite;
		artObj.artObj = artObj;
		this.artObj = artObj;
		anim.SetBool("spawned", setSpawned);
		parentList = deCtrl.artCardList;
		if (artObj.art != null)
		{
			artObj.art.listCard = this;
			if (artObj.depleted)
			{
				artObj.Deplete();
			}
		}
		return this;
	}

	public void SetWeapon(SpellObject weaponObj)
	{
		SetItem(weaponObj, base.transform.parent);
		image.sprite = weaponObj.sprite;
		weaponObj.spellObj = weaponObj;
		spellObj = weaponObj;
	}

	public override void UnFocus()
	{
		holdingWeapon = false;
		if ((bool)refCtrl && refCtrl.onScreen)
		{
			refCtrl.Hide();
		}
	}

	public override void OnWeaponPress()
	{
		if (itemObj.type == ItemType.Spell || itemObj.type == ItemType.Wep)
		{
			refCtrl.DisplaySpell(itemObj.spellObj, deCtrl.displayCard.cardInner);
		}
		else
		{
			refCtrl.Hide();
		}
		holdingWeapon = true;
	}

	public override void OnWeaponRelease()
	{
		holdingWeapon = false;
		if ((bool)refCtrl && refCtrl.onScreen)
		{
			refCtrl.Hide();
		}
	}

	public override void OnWeaponHold()
	{
		if (itemObj != null && !holdingWeapon && (itemObj.type == ItemType.Spell || itemObj.type == ItemType.Wep))
		{
			refCtrl.DisplaySpell(itemObj.spellObj, deCtrl.displayCard.cardInner);
		}
		holdingWeapon = true;
	}

	public void RemoveThisCard(bool useRemover = true)
	{
		if (deckScreen.busy)
		{
			return;
		}
		holdingWeapon = false;
		if (itemObj.type == ItemType.Spell)
		{
			S.I.refCtrl.Hide();
		}
		else if (itemObj.type == ItemType.Pact && itemObj.pactObj.hellPass)
		{
			return;
		}
		if (useRemover)
		{
			if (deCtrl.runCtrl.currentRun.removals < 1)
			{
				S.I.PlayOnce(btnCtrl.lockedSound);
				deckScreen.removerAnimator.SetTrigger("FlashRed");
				return;
			}
			deCtrl.runCtrl.currentRun.removals--;
			deckScreen.UpdateRemoverCountText();
		}
		StartCoroutine(_RemoveCard());
	}

	private IEnumerator _RemoveCard()
	{
		btnCtrl.RemoveFocus();
		S.I.PlayOnce(deckScreen.removeSound);
		deckScreen.busy = true;
		bgImage.material = new Material(deCtrl.dissolveMat);
		deCtrl.displayCard.cardInner.background.material = new Material(deCtrl.dissolveMat);
		Material savedMaterial = deCtrl.displayCard.cardInner.background.material;
		float value = 0f;
		float currentVelocity = 0f;
		float sDamping = 0.35f;
		deCtrl.displayCard.dissolving = true;
		while (value < 0.8f)
		{
			value = Mathf.SmoothDamp(value, 1f, ref currentVelocity, sDamping);
			deCtrl.displayCard.cardInner.background.material.SetFloat("_Destroyer_Value_1", value);
			bgImage.material.SetFloat("_Destroyer_Value_1", value);
			deCtrl.displayCard.cardInner.canvasGroup.alpha = 1f - value;
			yield return null;
		}
		deckScreen.busy = false;
		Object.Destroy(savedMaterial);
		Object.Destroy(bgImage.material);
		deCtrl.displayCard.dissolving = false;
		FinalizeRemoval();
	}

	public void FinalizeRemoval()
	{
		deCtrl.displayCard.Hide();
		deCtrl.displayCardAlt.Hide();
		if (parentList.IndexOf(this) > 0)
		{
			btnCtrl.SetFocus(parentList[parentList.IndexOf(this) - 1]);
		}
		else if (parentList.IndexOf(this) < parentList.Count - 1)
		{
			btnCtrl.SetFocus(parentList[parentList.IndexOf(this) + 1]);
		}
		else if (deCtrl.artGrid.childCount > 0)
		{
			btnCtrl.SetFocus(deCtrl.artGrid.GetChild(deCtrl.artGrid.childCount - 1).gameObject);
		}
		else
		{
			btnCtrl.SetFocus(deckScreen.foCtrl.brandDisplayButtons[0]);
		}
		Debug.Log("Removing " + itemObj.itemID);
		if (itemObj.type == ItemType.Spell)
		{
			deCtrl.RemoveCardFromDeck(this, true);
			if (duelDisk.currentDeck.Count < 1)
			{
				deckScreen.spellCursor.Hide();
			}
			deckScreen.CalculateGrid();
			duelDisk.CreateDeckSpells();
			S.I.ana.SendRemovePick(deCtrl.runCtrl.currentRun.finishedZones, deCtrl.ctrl.currentPlayer.beingObj.beingID, itemObj, duelDisk.deck.Count);
		}
		else if (itemObj.type == ItemType.Art)
		{
			deCtrl.RemoveArtifactCard(this);
		}
		else if (itemObj.type == ItemType.Pact)
		{
			deCtrl.RemoveArtifactCard(this);
		}
		deCtrl.deckScreen.UpdateButtonNavs();
		if (itemObj.type == ItemType.Art || itemObj.type == ItemType.Pact)
		{
			if (deCtrl.artCardList.Count > 0)
			{
				deCtrl.statsScreen.artCursor.SetTarget(deCtrl.artCardList[0].rect);
			}
			else if (deCtrl.pactCardList.Count > 0)
			{
				deCtrl.statsScreen.artCursor.SetTarget(deCtrl.pactCardList[0].rect);
			}
			if (deCtrl.artCardList.Count > 0)
			{
				btnCtrl.SetFocus(deCtrl.artCardList[0]);
			}
			else
			{
				btnCtrl.SetFocus(deckScreen.foCtrl.brandDisplayButtons[0]);
			}
		}
	}
}
