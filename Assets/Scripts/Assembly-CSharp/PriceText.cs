using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PriceText : MonoBehaviour
{
	public TMP_Text amount;

	public Image currencyIcon;

	public Sprite moneyIcon;

	public Sprite healthIcon;

	public RectTransform rect;

	private void Awake()
	{
		rect = GetComponent<RectTransform>();
	}

	public void Set(ZoneType zoneType)
	{
		switch (zoneType)
		{
		case ZoneType.Shop:
			currencyIcon.sprite = moneyIcon;
			break;
		case ZoneType.DarkShop:
			currencyIcon.sprite = healthIcon;
			break;
		}
		rect.localScale = Vector3.one;
	}
}
