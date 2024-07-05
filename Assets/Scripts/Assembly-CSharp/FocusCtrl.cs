using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FocusCtrl : NavPanel
{
	public BrandListCard brandListCardPrefab;

	public BrandListCard[] brandDisplayButtons;

	public List<ListCard> brandListCards;

	public RectTransform focusGrid;

	public int focusNum = 0;

	public int[] savedSelectionIndexes = new int[2];

	public ItemManager itemMan;

	private List<Brand> brandTypes;

	public DeckCtrl deCtrl;

	private void Start()
	{
		itemMan = S.I.itemMan;
		focusGrid.DestroyChildren();
		brandTypes = Enum.GetValues(typeof(Brand)).Cast<Brand>().ToList();
		foreach (Brand brandType in brandTypes)
		{
			BrandListCard brandListCard = UnityEngine.Object.Instantiate(brandListCardPrefab);
			brandListCard.transform.SetParent(focusGrid);
			brandListCard.foCtrl = this;
			brandListCard.displayButton = false;
			brandListCard.parentList = brandListCards;
			brandListCard.SetBrand(brandType);
			brandListCards.Add(brandListCard);
		}
		StartCoroutine(_AnimateCollectionC());
	}

	private IEnumerator _AnimateCollectionC()
	{
		yield return new WaitForEndOfFrame();
		if (slideBody.onScreen)
		{
			float delayTime = 0.03f;
			for (int j = 0; j < brandListCards.Count; j++)
			{
				yield return new WaitForSeconds(delayTime);
				brandListCards[j].anim.SetBool("visible", true);
			}
		}
		else
		{
			for (int i = 0; i < brandListCards.Count; i++)
			{
				brandListCards[i].anim.SetBool("visible", false);
			}
		}
	}

	public override void Open()
	{
		defaultButton = brandListCards[savedSelectionIndexes[focusNum]];
		originButton = brandDisplayButtons[focusNum];
		focusGrid.anchoredPosition = new Vector2(brandDisplayButtons[focusNum].rect.anchoredPosition.x, focusGrid.anchoredPosition.y);
		originButton.tmpText.color = U.I.GetColor(UIColor.Pink);
		base.Open();
		StartCoroutine(_AnimateCollectionC());
	}

	public override void Close()
	{
		StartCoroutine(_AnimateCollectionC());
		base.Close();
	}

	public void SetFocusNum(int num)
	{
		focusNum = num;
		Open();
	}

	public void SetFocusedBrand(Brand brand, int newFocusNum = -1)
	{
		if (newFocusNum >= 0 && newFocusNum < brandDisplayButtons.Length)
		{
			focusNum = newFocusNum;
		}
		brandDisplayButtons[focusNum].SetBrand(brand);
		savedSelectionIndexes[focusNum] = brandTypes.IndexOf(brand);
		S.I.poCtrl.focusLuck = 0;
		for (int i = 0; i < brandDisplayButtons.Length; i++)
		{
			if (brandDisplayButtons[i].brand == Brand.None)
			{
				S.I.poCtrl.focusLuck += 2;
			}
		}
		deCtrl.statsScreen.UpdateStatsText();
		if (brandDisplayButtons[focusNum].brand != 0)
		{
			SaveDataCtrl.Set("LastFocusedBrand", brandDisplayButtons[focusNum].brand);
		}
	}

	public void SetStartingBrands(List<Brand> brands)
	{
		for (int i = 0; i < 2; i++)
		{
			if (brands.Count > i)
			{
				SetFocusedBrand(brands[i], i);
			}
			else
			{
				SetFocusedBrand(Brand.None, i);
			}
		}
	}
}
