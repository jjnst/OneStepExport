using I2.Loc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrandListCard : ListCard
{
	public Brand brand;

	public FocusCtrl foCtrl;

	public bool displayButton = true;

	public void SetBrand(Brand newBrand)
	{
		brand = newBrand;
		image.sprite = deCtrl.brandSprites[(int)newBrand];
		tmpText.text = LocalizationManager.GetTranslation("BrandNames/" + brand);
	}

	public override void Up()
	{
		if (up == null)
		{
			if (parentList != null && parentList.Count > 0)
			{
				if (base.transform.GetSiblingIndex() > 0)
				{
					btnCtrl.SetFocus(parentList[base.transform.GetSiblingIndex() - 1]);
				}
				else
				{
					btnCtrl.SetFocus(parentList[parentList.Count - 1]);
				}
			}
		}
		else
		{
			base.Up();
		}
	}

	public override void Down()
	{
		if (down == null)
		{
			if (parentList != null && parentList.Count > 0)
			{
				if (base.transform.GetSiblingIndex() < parentList.Count - 1)
				{
					btnCtrl.SetFocus(parentList[base.transform.GetSiblingIndex() + 1]);
				}
				else
				{
					btnCtrl.SetFocus(parentList[0]);
				}
			}
		}
		else
		{
			base.Down();
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (IsHoverable() && !deckScreen.busy && deckScreen.open)
		{
			btnCtrl.SetFocus(this);
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if ((eventData == null || eventData.button == PointerEventData.InputButton.Left) && !displayButton)
		{
			OnAcceptPress();
		}
	}

	public override void Focus(int playerNum = 0)
	{
		StartColorCo(U.I.GetColor(UIColor.Pink));
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(ShowDetails());
			S.I.PlayOnce(btnCtrl.flipSound);
			base.Focus(playerNum);
			deCtrl.statsScreen.artCursor.Hide();
			deCtrl.displayCard.ShowDisplayCard(rect, brand);
		}
	}

	public override void OnWeaponPress()
	{
	}

	public override void UnFocus()
	{
		StartColorCo(Color.white);
		if ((bool)deCtrl.displayCard)
		{
			deCtrl.displayCard.Hide();
		}
	}

	public override void OnAcceptPress()
	{
		if (!GetComponent<Button>())
		{
			foCtrl.SetFocusedBrand(brand);
			foCtrl.Close();
		}
		else
		{
			base.OnAcceptPress();
		}
	}
}
