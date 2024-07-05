using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavButton : UIButton
{
	public bool colorOnHover = true;

	private float colorTransitionDuration = 0.4f;

	private Coroutine co_GoToColor;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();
		if (hovering)
		{
			canvasGroup.alpha = hoverAlpha;
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

	public override void Focus(int playerNum = 0)
	{
		if ((bool)canvasGroup)
		{
			canvasGroup.alpha = hoverAlpha;
		}
		else
		{
			Debug.LogWarning("Button does not have a canvasGroup");
		}
		if ((bool)tmpText && colorOnHover)
		{
			StartColorCo(U.I.GetColor(UIColor.Pink));
		}
		if ((bool)button)
		{
			button.Select();
		}
		if (functionOnFocus)
		{
			OnAcceptPress();
		}
	}

	public override void UnFocus()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(_RemoveColorNextFrame());
		}
	}

	protected virtual IEnumerator _RemoveColorNextFrame()
	{
		yield return new WaitForEndOfFrame();
		if ((bool)tmpText && colorOnHover && btnCtrl.focusedButton != this)
		{
			StartColorCo(Color.white);
		}
	}

	protected void StartColorCo(Color color)
	{
		if (co_GoToColor != null)
		{
			StopCoroutine(co_GoToColor);
			co_GoToColor = null;
		}
		if (base.gameObject.activeInHierarchy)
		{
			co_GoToColor = StartCoroutine(_GoToColor(color));
		}
	}

	public void GoToColor(UIColor color)
	{
		StartColorCo(U.I.GetColor(color));
	}

	protected virtual IEnumerator _GoToColor(Color color)
	{
		if (!(tmpText == null))
		{
			float currentLerpTime = 0f;
			while (tmpText.color != color)
			{
				currentLerpTime += Time.unscaledDeltaTime;
				float t2 = currentLerpTime / colorTransitionDuration;
				t2 = Mathf.Sin(t2 * (float)Math.PI * 0.5f);
				tmpText.color = Color.Lerp(tmpText.color, color, t2);
				yield return null;
			}
			co_GoToColor = null;
		}
	}
}
