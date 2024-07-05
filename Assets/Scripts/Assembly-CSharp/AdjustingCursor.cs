using UnityEngine;
using UnityEngine.UI;

public class AdjustingCursor : Follower
{
	private Vector3 sizeVelocity;

	public RectTransform targetRect;

	public Image image;

	public AdjustingCursor antiCursor;

	public RectTransform rect;

	public bool adjustToTarget = true;

	private bool scalingToTarget = false;

	public bool onScreen = false;

	public void SetTarget(RectTransform newTarget)
	{
		scalingToTarget = true;
		target = newTarget;
		targetRect = newTarget;
		if (!onScreen)
		{
			onScreen = true;
			if ((bool)antiCursor)
			{
				antiCursor.Hide();
			}
			anim.SetBool("OnScreen", true);
		}
	}

	public void Hide()
	{
		anim.SetBool("OnScreen", false);
		onScreen = false;
	}

	protected override void Update()
	{
		base.Update();
		if (scalingToTarget && adjustToTarget && (bool)targetRect)
		{
			Vector3 vector = new Vector3(Mathf.RoundToInt(targetRect.sizeDelta.x * targetRect.localScale.x * targetRect.parent.localScale.x), Mathf.RoundToInt(targetRect.sizeDelta.y * targetRect.localScale.y * targetRect.parent.localScale.y), 1f);
			if (rect.sizeDelta.x != vector.x || rect.sizeDelta.y != vector.y)
			{
				rect.sizeDelta = vector;
			}
			else
			{
				scalingToTarget = false;
			}
		}
	}
}
