using System.Collections.Generic;
using System.Globalization;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInner : MonoBehaviour
{
	public struct FTriggerEffects
	{
		public FTrigger fTrigger;

		public List<EffectApp> efApps;

		public FTriggerEffects(FTrigger fTrig, List<EffectApp> efAppList)
		{
			fTrigger = fTrig;
			efApps = efAppList;
		}
	}

	public Image background;

	public Image border;

	public Image brandBackground;

	public TMP_Text nameText;

	public TMP_Text mana;

	public Image image;

	public Image cardFrame;

	private TMP_Text currentDescription = null;

	public TMP_Text description;

	public TMP_Text descriptionReadable;

	public TMP_Text descriptionAlt;

	public TMP_Text flavorText;

	public TMP_Text damage;

	public TMP_Text numOfShots;

	public CanvasGroup cardBack;

	public Transform mechTooltipGrid;

	public CanvasGroup brandBox;

	public Image brandImage;

	public TMP_Text brandText;

	public RectTransform rect;

	public CardTrail cardTrail;

	public HitDisplay hitDisplay;

	public CanvasGroup canvasGroup;

	public PriceText priceText;

	public VoteDisplay voteDisplay;

	public BoxCollider2D col;

	public ItemObject itemObj;

	public PostCtrl poCtrl;

	public DeckCtrl deCtrl;

	private float startingDistance;

	private float mechDetailsSpacing = 5f;

	private Vector3 leftMechDetailsPosition;

	private Vector3 rightMechDetailsPosition;

	public void Awake()
	{
		deCtrl = S.I.deCtrl;
		poCtrl = S.I.poCtrl;
		rect = GetComponent<RectTransform>();
		rightMechDetailsPosition = new Vector3(rect.sizeDelta.x / 2f + mechDetailsSpacing + 33f, rect.sizeDelta.y / 3f - 2f, 0f);
		leftMechDetailsPosition = new Vector3(rightMechDetailsPosition.x * -1f, rightMechDetailsPosition.y - 2f, 0f);
	}

	private void OnDisable()
	{
		if (priceText != null)
		{
			priceText.gameObject.SetActive(false);
		}
	}

	public void SetRarity(int rarityNum)
	{
		if (itemObj.type == ItemType.Spell)
		{
			border.sprite = deCtrl.spellRarityBorders[rarityNum];
			background.sprite = deCtrl.spellBackgrounds[rarityNum];
			brandBackground.sprite = deCtrl.spellBackgroundBrands[(int)itemObj.brand];
		}
		else if (itemObj.type == ItemType.Art)
		{
			border.sprite = deCtrl.artRarityBorders[rarityNum];
		}
		else if (itemObj.type == ItemType.Wep)
		{
			background.sprite = deCtrl.weaponBackground;
			brandBackground.sprite = deCtrl.blankBorder;
			border.sprite = deCtrl.blankBorder;
		}
	}

	public void CreateCardTrail(Transform newTarget)
	{
		cardTrail.TrailTo(newTarget);
		mechTooltipGrid.DestroyChildren();
	}

	private void SetCurrentDescription()
	{
		if (S.I.readabilityModeEnabled && descriptionReadable != null)
		{
			currentDescription = descriptionReadable;
			description.gameObject.SetActive(false);
			TMP_Text tMP_Text = flavorText;
			if ((object)tMP_Text != null)
			{
				tMP_Text.gameObject.SetActive(false);
			}
		}
		else
		{
			currentDescription = description;
			TMP_Text tMP_Text2 = descriptionReadable;
			if ((object)tMP_Text2 != null)
			{
				tMP_Text2.gameObject.SetActive(false);
			}
			TMP_Text tMP_Text3 = flavorText;
			if ((object)tMP_Text3 != null)
			{
				tMP_Text3.gameObject.SetActive(true);
			}
		}
		currentDescription.gameObject.SetActive(true);
	}

	public void ApplyItemAttributesToCard(ItemObject itemObj)
	{
		SetCurrentDescription();
		mechTooltipGrid.DestroyChildren();
		ApplyBaseAttributesToCard(itemObj);
		if (itemObj.type == ItemType.Art)
		{
			ApplyArtifactAttributesToCard(itemObj.artObj);
		}
		else if (itemObj.type == ItemType.Spell)
		{
			ApplySpellAttributesToCard(itemObj.spellObj);
		}
		else if (itemObj.type == ItemType.Wep)
		{
			ApplySpellAttributesToCard(itemObj.spellObj);
		}
		else if (itemObj.type == ItemType.Pact)
		{
			ApplyPactAttributesToCard(itemObj.pactObj);
		}
	}

	private void ApplySpellAttributesToCard(SpellObject spellObj)
	{
		currentDescription.text = deCtrl.ParseDescription(spellObj, spellObj.itemID);
		string Translation = itemObj.flavor;
		LocalizationManager.TryGetTranslation("SpellFlavors/" + itemObj.itemID, out Translation);
		if (S.modsInstalled && string.IsNullOrEmpty(Translation))
		{
			Translation = itemObj.flavor;
		}
		flavorText.text = Translation;
		if (spellObj.manaType.type != 0)
		{
			mana.text = "X";
		}
		else
		{
			mana.text = spellObj.mana.ToString();
		}
		if (spellObj.enhancements.Contains(Enhancement.ManaRe))
		{
			mana.text = U.I.Colorify(mana.text, UIColor.EnhancementMana);
		}
		if (spellObj.damageType.type == AmountType.Normal || spellObj.damageType.type == AmountType.Zero)
		{
			float num = spellObj.damage;
			if (spellObj.damage < 0f)
			{
				num *= -1f;
			}
			damage.text = num.ToString();
		}
		else
		{
			damage.text = string.Empty;
			if (spellObj.damageType.initial > 0)
			{
				damage.text = spellObj.damageType.initial + "+";
			}
			if (spellObj.damageType.multiplier > 1f)
			{
				TMP_Text tMP_Text = damage;
				tMP_Text.text = tMP_Text.text + "<size=80%>" + spellObj.damageType.multiplier + "</size>×";
			}
			else
			{
				damage.text = "X";
			}
		}
		if (spellObj.enhancements.Contains(Enhancement.DamagePlus) || spellObj.enhancements.Contains(Enhancement.Overload) || spellObj.enhancements.Contains(Enhancement.Mini))
		{
			damage.text = U.I.Colorify(damage.text, UIColor.EnhancementMana);
		}
		string text = "";
		int num2 = Mathf.RoundToInt(spellObj.numShots);
		if (spellObj.numShotsType.type == AmountType.Normal)
		{
			text = num2.ToString();
		}
		else
		{
			if (spellObj.numShotsType.type == AmountType.Infinite)
			{
				num2 = 9999;
			}
			text = "X";
		}
		if (itemObj.brand == Brand.None)
		{
			brandBox.alpha = 0f;
		}
		else
		{
			brandBox.alpha = 0.9f;
			brandImage.sprite = deCtrl.brandSprites[(int)itemObj.brand];
			brandImage.color = Color.white;
			brandText.text = LocalizationManager.GetTranslation("BrandNames/" + itemObj.brand);
			brandText.color = Color.white;
		}
		if (num2 > 1 || spellObj.numShotsType.type != 0)
		{
			if (num2 > 999)
			{
				numOfShots.text = "<size=300%>∞</size>";
			}
			else
			{
				numOfShots.text = string.Format("x{0}", text);
			}
			if (spellObj.enhancements.Contains(Enhancement.Shot) || spellObj.enhancements.Contains(Enhancement.ExtendedMag))
			{
				numOfShots.text = U.I.Colorify(numOfShots.text, UIColor.EnhancementMana);
			}
		}
		else
		{
			numOfShots.text = "";
		}
		if (spellObj.HasParam("numOfWaves"))
		{
			int num3 = int.Parse(spellObj.Param("numOfWaves"));
			if (num3 > 999)
			{
				numOfShots.text = "<size=300%>∞</size>";
			}
			else if (num3 > 1)
			{
				numOfShots.text += string.Format("x{0}", num3);
			}
		}
	}

	private void ApplyArtifactAttributesToCard(ArtifactObject artObj)
	{
		currentDescription.text = deCtrl.ParseDescription(artObj, artObj.itemID);
		string Translation = itemObj.flavor;
		LocalizationManager.TryGetTranslation("ArtFlavors/" + itemObj.itemID, out Translation);
		if (S.modsInstalled && string.IsNullOrEmpty(Translation))
		{
			Translation = itemObj.flavor;
		}
		flavorText.text = Translation;
	}

	private void ApplyPactAttributesToCard(PactObject pactObj)
	{
		currentDescription.text = deCtrl.ParseDescription(pactObj, pactObj.itemID) ?? "";
		if (pactObj.originalDuration > 0)
		{
			currentDescription.text += string.Format("({0} {1}/{2})", ScriptLocalization.UI.Pact_Battles, pactObj.originalDuration - pactObj.duration, pactObj.originalDuration);
		}
		if (pactObj.tags.Contains(Tag.Hell))
		{
			descriptionAlt.text = string.Empty;
		}
		else
		{
			descriptionAlt.text = deCtrl.ParseDescription(pactObj, pactObj.rewardID);
		}
		image.sprite = pactObj.sprite;
	}

	public void ApplyBrandAttributesToCard(Brand brand)
	{
		SetCurrentDescription();
		currentDescription.text = LocalizationManager.GetTranslation("BrandDescriptions/" + brand);
	}

	private void ApplyBaseAttributesToCard(ItemObject itemObj)
	{
		this.itemObj = itemObj;
		cardTrail.itemObj = itemObj;
		if (LocalizationManager.CurrentLanguage == "Turkish")
		{
			nameText.text = itemObj.nameString.ToUpper(new CultureInfo("tr-TR"));
		}
		else
		{
			nameText.text = itemObj.nameString.ToUpper();
		}
		image.sprite = itemObj.sprite;
		if (string.IsNullOrEmpty(itemObj.description))
		{
			itemObj.description = "";
		}
		currentDescription.text = itemObj.description;
		flavorText.text = itemObj.flavor;
		SetRarity(itemObj.rarity);
		canvasGroup.alpha = 1f;
	}

	public void SetPriceText(PriceText newPriceText)
	{
		priceText = newPriceText;
		newPriceText.transform.SetParent(base.transform, true);
	}

	public void SetVoteDisplay(VoteDisplay newVoteDisplay, int optionNum, TwitchClient twClient)
	{
		voteDisplay = newVoteDisplay;
		voteDisplay.twClient = twClient;
		newVoteDisplay.transform.SetParent(base.transform, true);
	}

	public void CreateMechTooltip(RectTransform itemDisplay, bool listCard = false)
	{
		if (base.transform.position.x < -20f)
		{
			mechTooltipGrid.transform.localPosition = rightMechDetailsPosition;
		}
		else
		{
			mechTooltipGrid.transform.localPosition = leftMechDetailsPosition;
		}
		mechTooltipGrid.DestroyChildren();
		if (itemObj == null)
		{
			return;
		}
		List<FTrigger> list = new List<FTrigger>();
		List<Effect> list2 = new List<Effect>();
		for (int num = itemObj.efApps.Count - 1; num >= 0; num--)
		{
			FTrigger fTrigger = itemObj.efApps[num].fTrigger;
			if (!list.Contains(fTrigger) && deCtrl.triggerTooltips.Contains(fTrigger))
			{
				MechTooltipInstance(itemObj.efApps[num], fTrigger.ToString());
				list.Add(fTrigger);
			}
			Effect effect = itemObj.efApps[num].effect;
			if (!list2.Contains(effect) && deCtrl.effectTooltips.Contains(effect))
			{
				MechTooltipInstance(itemObj.efApps[num], effect.ToString());
				list2.Add(effect);
			}
		}
		for (int num2 = itemObj.efApps.Count - 1; num2 >= 0; num2--)
		{
			Effect effect2 = itemObj.efApps[num2].effect;
			if (effect2 == Effect.AddToDeck || effect2 == Effect.AddToDiscard || effect2 == Effect.CreateSpell || effect2 == Effect.EquipWep)
			{
				if (itemObj.efApps[num2].value != "ThisSpell")
				{
					ItemObject itemObject = S.I.itemMan.itemDictionary[itemObj.efApps[num2].value];
					itemObject.being = itemObj.being;
					itemObject.ctrl = itemObj.ctrl;
					deCtrl.displayCardAlt.ShowDisplayCardAlt(itemDisplay, itemObject, true, mechTooltipGrid.childCount, listCard);
				}
			}
			else if (effect2 == Effect.Jam)
			{
				deCtrl.displayCardAlt.ShowDisplayCardAlt(itemDisplay, S.I.itemMan.itemDictionary[effect2.ToString()], true, mechTooltipGrid.childCount, listCard);
			}
			else if (base.transform.position.x < -100f)
			{
				mechTooltipGrid.transform.localPosition = rightMechDetailsPosition;
			}
			else
			{
				mechTooltipGrid.transform.localPosition = leftMechDetailsPosition;
			}
		}
		if (deCtrl.displayCardAlt.positionedOnRight)
		{
			mechTooltipGrid.transform.localPosition = rightMechDetailsPosition;
		}
	}

	private void MechTooltipInstance(EffectApp efApp, string dictKey)
	{
		MechDetails mechDetails = Object.Instantiate(deCtrl.mechTooltip, mechTooltipGrid);
		mechDetails.cardInner = this;
		mechDetails.FillTra(efApp, deCtrl.ctrl.effectSpritesDict[dictKey], dictKey);
	}

	public static string FirstCharToUpper(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		return char.ToUpper(s[0]) + s.Substring(1);
	}

	public static string FirstCharToLower(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		return char.ToLower(s[0]) + s.Substring(1);
	}
}
