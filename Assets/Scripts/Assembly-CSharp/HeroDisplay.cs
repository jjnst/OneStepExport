using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeroDisplay : MonoBehaviour
{
	public HeroItemListCard startingItemPrefab;

	public RectTransform spellsGrid;

	public RectTransform artsGrid;

	public RectTransform wepGrid;

	public TMP_Text heroDescription;

	public void SetHeroItems(BeingObject beingObj, ref List<HeroItemListCard> itemCards, DeckCtrl deCtrl, HeroSelectCtrl heCtrl, PvPSelectCtrl pvPSelectCtrl = null)
	{
		foreach (string item in beingObj.deck)
		{
			HeroItemListCard heroItemListCard = Object.Instantiate(startingItemPrefab);
			heroItemListCard.SetHeroSpell(item, this);
			itemCards.Add(heroItemListCard);
			heroItemListCard.heroCtrl = heCtrl;
		}
		spellsGrid.sizeDelta = new Vector2(startingItemPrefab.rect.sizeDelta.x * (float)beingObj.deck.Count, startingItemPrefab.rect.sizeDelta.y);
		artsGrid.DestroyChildren();
		foreach (ArtData artifact in beingObj.artifacts)
		{
			HeroItemListCard heroItemListCard2 = Object.Instantiate(startingItemPrefab);
			heroItemListCard2.SetHeroArtifact(artifact.itemID, this);
			itemCards.Add(heroItemListCard2);
			heroItemListCard2.heroCtrl = heCtrl;
		}
		artsGrid.sizeDelta = new Vector2(startingItemPrefab.rect.sizeDelta.x * (float)beingObj.artifacts.Count, startingItemPrefab.rect.sizeDelta.y);
		wepGrid.DestroyChildren();
		HeroItemListCard heroItemListCard3 = Object.Instantiate(startingItemPrefab);
		heroItemListCard3.SetHeroWeapon(beingObj.weapon, this);
		itemCards.Add(heroItemListCard3);
		heroItemListCard3.heroCtrl = heCtrl;
	}
}
