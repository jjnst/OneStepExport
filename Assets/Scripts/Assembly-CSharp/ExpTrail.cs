using UnityEngine;

public class ExpTrail : SinFollower
{
	private UnlockCtrl unCtrl;

	protected override void ReachedTarget()
	{
		unCtrl.AddExp();
		base.ReachedTarget();
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (target == null)
		{
			DisableSelf();
		}
	}

	public ExpTrail Set(Transform theTarget, UnlockCtrl unCtrl)
	{
		target = theTarget;
		this.unCtrl = unCtrl;
		return this;
	}

	private void OnDisable()
	{
		target = null;
	}
}
