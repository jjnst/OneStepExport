using UnityEngine.EventSystems;

public class TopNavButton : NavButton
{
	private void Start()
	{
		btnCtrl = S.I.btnCtrl;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (IsHoverable())
		{
			hovering = true;
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		hovering = false;
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button != 0)
		{
		}
	}
}
