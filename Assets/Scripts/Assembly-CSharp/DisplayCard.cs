using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayCard : UICard
{
	private DeckScreen deckScreen;

	public bool positionedOnRight = false;

	public bool dissolving = false;

	protected override void Start()
	{
		base.Start();
		anim.SetBool("visible", true);
		deckScreen = deCtrl.deckScreen;
	}

	public void Hide()
	{
		anim.SetBool("OnDisplay", false);
		if ((bool)cardInner)
		{
			cardInner.gameObject.layer = 2;
		}
	}

	private IEnumerator ShowBrandAfterDissolve(RectTransform itemDisplay, Brand brand)
	{
		while (dissolving)
		{
			yield return null;
		}
		ShowDisplayCard(itemDisplay, brand);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
	}

	public virtual void ShowDisplayCard(RectTransform itemDisplay, ItemObject theItemObj, bool second = false)
	{
		SetCard(theItemObj);
		base.transform.SetParent(deCtrl.deckScreen.transform);
		if (!(cardInner == null))
		{
			if (cardInner.voteDisplay != null)
			{
				cardInner.voteDisplay.gameObject.SetActive(false);
			}
			if (cardInner.voteDisplay != null)
			{
				cardInner.voteDisplay.gameObject.SetActive(false);
			}
			float num = cardInner.rect.sizeDelta.y / 2f;
			float num2 = itemDisplay.localPosition.y + itemDisplay.parent.position.y;
			float num3 = 0f;
			float num4 = 0f;
			if (itemDisplay.transform.parent == deckScreen.deckGrid)
			{
				num3 = 50f;
				num4 = -104f;
			}
			else if (itemDisplay.transform.parent == deCtrl.artGrid)
			{
				num3 = num2 - 10f;
				num4 = -46f;
			}
			else if (itemDisplay.transform.parent == S.I.poCtrl.choiceCardGrid)
			{
				num3 = num2 + num;
				num4 = -46f;
			}
			else if (itemDisplay.transform.parent == S.I.shopCtrl.pactGrid)
			{
				num3 = num2 + num;
				num4 = -1026f;
			}
			else if (itemDisplay.transform.parent == deCtrl.pactGrid)
			{
				num3 = num2 - 10f;
				num4 = -1026f;
			}
			else if (itemDisplay.transform.parent == deCtrl.deckScreen.transform)
			{
				num3 = num2 - 10f;
				num4 = -46f;
			}
			else
			{
				num3 = 9050f;
				num4 = -46f;
			}
			if (num2 > num3 - num)
			{
				num2 = num3 - num;
			}
			else if (num2 < num4 + num)
			{
				num2 = num4 + num;
			}
			PositionDisplay(itemDisplay, num2, second);
		}
	}

	public virtual void ShowDisplayCard(RectTransform itemDisplay, Brand brand)
	{
		if (dissolving)
		{
			StartCoroutine(ShowBrandAfterDissolve(itemDisplay, brand));
			return;
		}
		SetCard(brand);
		if (itemDisplay.parent == deCtrl.deckScreen.foCtrl.focusGrid)
		{
			base.transform.SetParent(deCtrl.deckScreen.foCtrl.transform);
		}
		else
		{
			base.transform.SetParent(deCtrl.deckScreen.transform);
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = cardInner.rect.sizeDelta.y / 2f;
		float num4 = itemDisplay.position.y;
		num = 100f;
		num2 = -114f;
		if (num4 > num - num3)
		{
			num4 = num - num3;
		}
		else if (num4 < num2 + num3)
		{
			num4 = num2 + num3;
		}
		PositionDisplay(itemDisplay, num4);
	}

	public virtual void ShowDisplayCardAlt(RectTransform itemDisplay, ItemObject theItemObj, bool second = false, int tooltipCount = 1, bool listCard = false)
	{
		SetCard(theItemObj);
		base.transform.SetParent(deCtrl.deckScreen.transform);
		float num = cardInner.rect.sizeDelta.y / 2f;
		float num2 = itemDisplay.localPosition.y + itemDisplay.parent.position.y;
		float num3 = 0f;
		float num4 = 0f;
		if (itemDisplay.transform.parent == deckScreen.deckGrid)
		{
			num3 = 70f;
			num4 = -104f;
		}
		else if (itemDisplay.transform.parent == deCtrl.artGrid)
		{
			num3 = num2 - 10f;
			num4 = -46f;
		}
		else if (itemDisplay.transform.parent == S.I.poCtrl.choiceCardGrid)
		{
			num3 = num2 + num;
			num4 = -46f;
		}
		else if (itemDisplay.transform.parent == S.I.shopCtrl.pactGrid)
		{
			num3 = num2 - 10f;
			num4 = -46f;
		}
		else if ((bool)itemDisplay.parent.parent.GetComponent<HeroDisplay>())
		{
			num3 = num2 + num;
			num4 = -1026f;
		}
		else
		{
			num3 = 1010f;
			num4 = -46f;
		}
		if (num2 > num3 - num)
		{
			num2 = num3 - num;
		}
		else if (num2 < num4 + num)
		{
			num2 = num4 + num;
		}
		PositionDisplay(itemDisplay, num2, second, tooltipCount);
		cardInner.transform.position = itemDisplay.transform.position;
		cardInner.cardTrail.ExpandTo(base.transform, listCard);
	}

	public void PositionDisplay(RectTransform itemDisplay, float yPos, bool second = false, int tooltipCount = 1)
	{
		positionedOnRight = false;
		Vector3 one = Vector3.one;
		float num = 135f * itemDisplay.GetComponent<UIButton>().targetSize;
		float num2 = 70f * defaultSize;
		float num3 = itemDisplay.sizeDelta.x * defaultSize;
		if (itemDisplay.parent == deckScreen.deckGrid)
		{
			one = new Vector3(itemDisplay.position.x - (num3 - 1f), yPos, 0f);
			if (second)
			{
				if (tooltipCount > 0)
				{
					one -= Vector3.right * num;
				}
				else
				{
					one -= Vector3.right * num3;
				}
			}
		}
		else if (itemDisplay.parent == deCtrl.artGrid || itemDisplay.parent == deCtrl.pactGrid || itemDisplay.parent == S.I.shopCtrl.pactGrid)
		{
			one = new Vector3(itemDisplay.position.x + num3 + 30f, yPos, 0f);
			if (second)
			{
				if (tooltipCount > 0)
				{
					one -= Vector3.right * num;
				}
				else
				{
					one -= Vector3.right * num2;
				}
			}
		}
		else if (itemDisplay.parent == S.I.poCtrl.choiceCardGrid)
		{
			one = new Vector3(itemDisplay.position.x, yPos, 0f);
			if (second)
			{
				if (tooltipCount > 0)
				{
					one -= Vector3.right * num;
				}
				else
				{
					one -= Vector3.right * num2;
				}
			}
		}
		else if ((bool)itemDisplay.parent.parent.GetComponent<HeroDisplay>())
		{
			float num4 = Camera.main.orthographicSize * Camera.main.aspect;
			one = new Vector3(itemDisplay.position.x + num3 + 20f, yPos - 52f, 0f);
			if (one.x > num4 - num3 - 20f)
			{
				one = new Vector3(itemDisplay.position.x - num3 - 20f, yPos - 52f, 0f);
			}
		}
		else
		{
			one = new Vector3(itemDisplay.position.x - num3 - 25f, yPos, 0f);
		}
		if (second)
		{
			if (one.x < -150f)
			{
				one = ((tooltipCount <= 0) ? new Vector3(one.x + num, one.y, one.z) : new Vector3(one.x + num * 2f, one.y, one.z));
				positionedOnRight = true;
			}
			else if ((bool)itemDisplay.parent.parent.GetComponent<HeroDisplay>())
			{
				one = new Vector3(one.x - num + 2f, one.y, one.z);
			}
		}
		base.transform.position = one;
	}
}
