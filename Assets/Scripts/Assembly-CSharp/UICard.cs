using UnityEngine;
using UnityEngine.EventSystems;

public class UICard : UIButton
{
	public CardInner cardInner;

	public ItemObject itemObj;

	public DeckCtrl deCtrl;

	public ReferenceCtrl refCtrl;

	public bool enablePreview = false;

	public bool chosen = false;

	public bool shopCard = false;

	public bool holdingWeapon = false;

	public bool libraryCard = false;

	private float readabilityScale = 1.15f;

	protected override void Awake()
	{
		base.Awake();
		if (S.I.readabilityModeEnabled)
		{
			defaultSize *= readabilityScale;
			hoverSize *= readabilityScale;
		}
	}

	protected virtual void Start()
	{
		deCtrl = S.I.deCtrl;
	}

	private void OnEnable()
	{
		rect.localScale = Vector3.one * 0.85f;
	}

	public void SetCard(ItemObject itemObj)
	{
		this.itemObj = itemObj;
		if (cardInner != null)
		{
			SimplePool.Despawn(cardInner.gameObject);
		}
		if (itemObj != null)
		{
			if (itemObj.type == ItemType.Art)
			{
				cardInner = SimplePool.Spawn(deCtrl.cardInnerArtPrefab, Vector3.zero, base.transform.rotation).GetComponent<CardInner>();
			}
			else if (itemObj.type == ItemType.Spell)
			{
				cardInner = SimplePool.Spawn(deCtrl.cardInnerSpellPrefab, Vector3.zero, base.transform.rotation).GetComponent<CardInner>();
			}
			else if (itemObj.type == ItemType.Wep)
			{
				cardInner = SimplePool.Spawn(deCtrl.cardInnerSpellPrefab, Vector3.zero, base.transform.rotation).GetComponent<CardInner>();
			}
			else if (itemObj.type == ItemType.Pact)
			{
				cardInner = SimplePool.Spawn(deCtrl.cardInnerPactPrefab, Vector3.zero, base.transform.rotation).GetComponent<CardInner>();
			}
			cardInner.transform.SetParent(base.transform, false);
			anim.SetBool("OnDisplay", true);
			cardInner.ApplyItemAttributesToCard(itemObj);
		}
	}

	public void SetCard(Brand brand)
	{
		if (cardInner != null)
		{
			SimplePool.Despawn(cardInner.gameObject);
		}
		cardInner = SimplePool.Spawn(deCtrl.cardInnerBrandPrefab, Vector3.zero, base.transform.rotation).GetComponent<CardInner>();
		cardInner.transform.SetParent(base.transform, false);
		anim.SetBool("OnDisplay", true);
		cardInner.ApplyBrandAttributesToCard(brand);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (IsHoverable())
		{
			btnCtrl.SetFocus(this);
			cardInner.CreateMechTooltip(rect);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if ((bool)cardInner && (bool)cardInner.mechTooltipGrid)
		{
			cardInner.mechTooltipGrid.DestroyChildren();
		}
		deCtrl.displayCardAlt.Hide();
	}

	public override void OnWeaponPress()
	{
		if (!enablePreview)
		{
			return;
		}
		if (!chosen && (itemObj.type == ItemType.Spell || itemObj.type == ItemType.Wep))
		{
			if (refCtrl.autoPreview && !shopCard && !libraryCard)
			{
				refCtrl.Hide();
			}
			else
			{
				refCtrl.DisplaySpell(itemObj.spellObj, cardInner);
			}
		}
		holdingWeapon = true;
	}

	public override void OnWeaponRelease()
	{
		if (!enablePreview)
		{
			return;
		}
		holdingWeapon = false;
		if (itemObj.type == ItemType.Spell || itemObj.type == ItemType.Wep)
		{
			if (refCtrl.autoPreview && !shopCard && !libraryCard)
			{
				refCtrl.DisplaySpell(itemObj.spellObj, cardInner);
			}
			else
			{
				refCtrl.Hide();
			}
		}
	}

	public override void OnWeaponHold()
	{
		if (!enablePreview)
		{
			return;
		}
		if (!holdingWeapon && (itemObj.type == ItemType.Spell || itemObj.type == ItemType.Wep))
		{
			if (refCtrl.autoPreview && !shopCard && !libraryCard)
			{
				refCtrl.Hide();
			}
			else
			{
				refCtrl.DisplaySpell(itemObj.spellObj, cardInner);
			}
		}
		holdingWeapon = true;
	}

	public void PreviewFocus()
	{
		if (refCtrl.autoPreview && !shopCard && !libraryCard && !holdingWeapon && (itemObj.type == ItemType.Spell || itemObj.type == ItemType.Wep))
		{
			refCtrl.DisplaySpell(itemObj.spellObj, cardInner);
		}
	}
}
