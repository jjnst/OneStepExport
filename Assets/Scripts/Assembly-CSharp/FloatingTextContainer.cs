using UnityEngine;

public class FloatingTextContainer : UIFollow
{
	public BC ctrl;

	private void OnEnable()
	{
		foreach (Transform item in base.transform)
		{
			SimplePool.Despawn(item.gameObject);
		}
	}

	public FloatingTextContainer SetContainer(float newXOffset, float newYOffset, Transform following, bool runOnce, Canvas newParent, float duration, BC ctrl)
	{
		Set(newXOffset, newYOffset, following, false, ctrl.floatingTextsContainer);
		SimplePool.Despawn(base.gameObject, duration);
		return this;
	}
}
