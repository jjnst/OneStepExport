using UnityEngine.EventSystems;

public class ShopButton : NavButton
{
	public DeckCtrl deCtrl;

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		deCtrl.statsScreen.artCursor.Hide();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (deCtrl.statsScreen.artCursor.target != base.transform)
		{
			deCtrl.statsScreen.artCursor.SetTarget(rect);
		}
		if (!deCtrl.statsScreen.artCursor.onScreen)
		{
			deCtrl.statsScreen.artCursor.anim.SetBool("OnScreen", true);
		}
	}

	public override void Focus(int playerNum = 0)
	{
		deCtrl.statsScreen.artCursor.SetTarget(rect);
	}

	public override void UnFocus()
	{
		deCtrl.statsScreen.artCursor.Hide();
	}
}
