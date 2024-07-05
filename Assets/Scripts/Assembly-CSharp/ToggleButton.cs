using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleButton : NavButton
{
	public Toggle toggle;

	protected override void Awake()
	{
		base.Awake();
		if ((bool)GetComponent<Toggle>())
		{
			toggle = GetComponent<Toggle>();
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (IsHoverable())
		{
			hovering = true;
			if (setFocusOnHover && btnCtrl.focusedButton != this)
			{
				S.I.PlayOnce(btnCtrl.hoverSound);
				btnCtrl.SetFocus(this);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (hovering)
		{
			canvasGroup.alpha = 1f;
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (btnCtrl.focusedButton != this)
			{
				btnCtrl.SetFocus(this);
			}
			S.I.PlayOnce(btnCtrl.pierceSound);
		}
	}

	public override void OnAcceptPress()
	{
		base.OnAcceptPress();
		toggle.isOn = !toggle.isOn;
	}
}
