using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SettingButton : NavButton
{
	private SettingsPane settingsPane;

	public UnityEvent leftEvent;

	public bool disabled = false;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Left()
	{
		if (left == null)
		{
			if (!functionOnFocus)
			{
				S.I.PlayOnce(btnCtrl.pierceSound);
			}
			leftEvent.Invoke();
		}
	}

	public override void Right()
	{
		if (right == null)
		{
			OnAcceptPress();
		}
	}

	protected override IEnumerator _RemoveColorNextFrame()
	{
		yield return new WaitForEndOfFrame();
		if ((bool)tmpText && colorOnHover && btnCtrl.focusedButton != this)
		{
			if (disabled)
			{
				StartColorCo(Color.grey);
			}
			else
			{
				StartColorCo(Color.white);
			}
		}
	}
}
