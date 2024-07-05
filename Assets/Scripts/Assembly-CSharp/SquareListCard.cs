using System.Collections;
using UnityEngine;

public class SquareListCard : ListCard
{
	public Sprite checkmark;

	private bool holdingRemove = false;

	public override void Up()
	{
		base.Up();
	}

	public override void Down()
	{
		base.Down();
	}

	public override void Left()
	{
		if (!useParentTransformNav)
		{
			return;
		}
		if (base.transform.GetSiblingIndex() > 0)
		{
			btnCtrl.SetFocus(parentList[base.transform.GetSiblingIndex() - 1]);
		}
		else if ((parentList == deCtrl.artCardList || parentList == deCtrl.pactCardList) && base.transform.GetSiblingIndex() == 0)
		{
			if (deCtrl.ctrl.currentPlayer.duelDisk.deck.Count > 0)
			{
				btnCtrl.SetFocus(deCtrl.ctrl.currentPlayer.duelDisk.deck[0]);
			}
			else if (deCtrl.deckScreen.foCtrl.brandDisplayButtons.Length > 1)
			{
				btnCtrl.SetFocus(deCtrl.deckScreen.foCtrl.brandDisplayButtons[1]);
			}
		}
	}

	public override void Right()
	{
		if (!useParentTransformNav)
		{
			return;
		}
		if (base.transform.GetSiblingIndex() < parentList.Count - 1)
		{
			btnCtrl.SetFocus(parentList[base.transform.GetSiblingIndex() + 1]);
		}
		else if ((parentList == deCtrl.artCardList || parentList == deCtrl.pactCardList) && base.transform.GetSiblingIndex() == parentList.Count - 1)
		{
			if (deCtrl.ctrl.currentPlayer.duelDisk.deck.Count > 0)
			{
				btnCtrl.SetFocus(deCtrl.ctrl.currentPlayer.duelDisk.deck[0]);
			}
			else if (deCtrl.deckScreen.foCtrl.brandDisplayButtons.Length != 0)
			{
				btnCtrl.SetFocus(deCtrl.deckScreen.foCtrl.brandDisplayButtons[0]);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!(btnCtrl.focusedButton == this))
		{
			return;
		}
		if (holdingRemove)
		{
			if (pactObj == null || (pactObj != null && !pactObj.hellPass))
			{
				if (deckScreen.removeSpellFill.fillAmount < 1f)
				{
					deckScreen.removeSpellFill.fillAmount += Time.deltaTime / btnCtrl.holdDuration;
				}
				else
				{
					if ((pactObj == null || (pactObj != null && !pactObj.hellPass)) && itemObj.type != ItemType.Wep)
					{
						deckScreen.RemoveArtifact(this);
					}
					deckScreen.removeSpellFill.fillAmount = 0f;
				}
			}
			holdingRemove = false;
		}
		else if (deckScreen.removeSpellFill.fillAmount > 0f)
		{
			deckScreen.removeSpellFill.fillAmount -= Time.deltaTime / btnCtrl.holdDuration;
		}
	}

	public override void OnAcceptPress()
	{
	}

	public ListCard SetPact(PactObject pactObj, Transform parentTransform, int siblingIndex = -1, bool setSpawned = false)
	{
		SetItem(pactObj, parentTransform, siblingIndex);
		base.name = " ListCard " + siblingIndex + " - " + pactObj.nameString;
		bgImage.sprite = pactObj.sprite;
		base.pactObj = pactObj;
		anim.SetBool("spawned", setSpawned);
		parentList = deCtrl.pactCardList;
		if (pactObj.pact != null)
		{
			pactObj.pact.listCard = this;
		}
		return this;
	}

	public override void Focus(int playerNum = 0)
	{
		anim.SetBool("spawned", true);
		deCtrl.displayCard.ShowDisplayCard(rect, itemObj);
		deCtrl.statsScreen.artCursor.SetTarget(rect);
		base.Focus(playerNum);
	}

	public void Finish()
	{
		StartCoroutine(_Finish());
	}

	private IEnumerator _Finish()
	{
		deCtrl.pactCardList.Remove(this);
		anim.SetBool("finished", true);
		bgImage.sprite = checkmark;
		yield return new WaitForSeconds(1.5f);
		if (hovering)
		{
			if ((bool)deCtrl.displayCard)
			{
				deCtrl.displayCard.Hide();
			}
			if ((bool)deCtrl.displayCardAlt)
			{
				deCtrl.displayCardAlt.Hide();
			}
		}
		Object.Destroy(base.gameObject);
	}

	public override void UnFocus()
	{
		base.UnFocus();
		deCtrl.statsScreen.artCursor.Hide();
		if ((bool)deCtrl.displayCard)
		{
			deCtrl.displayCard.Hide();
		}
		if ((bool)deCtrl.displayCardAlt)
		{
			deCtrl.displayCardAlt.Hide();
		}
	}

	public override void RemoveSpellHold()
	{
		holdingRemove = true;
	}
}
