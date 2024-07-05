using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderButton : NavButton
{
	public Slider slider;

	public int changeAmount = 1;

	protected override void Awake()
	{
		base.Awake();
		if ((bool)GetComponent<Slider>())
		{
			slider = GetComponent<Slider>();
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

	public override void Left()
	{
		slider.value -= changeAmount;
		if (holdingDown)
		{
			slider.value -= changeAmount;
		}
	}

	public override void Right()
	{
		slider.value += changeAmount;
		if (holdingDown)
		{
			slider.value += changeAmount;
		}
	}

	public override void Focus(int playerNum = 0)
	{
		base.Focus(0);
		canvasGroup.alpha = 1f;
		if ((bool)slider)
		{
			slider.Select();
		}
	}

	public override void UnFocus()
	{
		base.UnFocus();
		EventSystem.current.SetSelectedGameObject(null);
	}
}
