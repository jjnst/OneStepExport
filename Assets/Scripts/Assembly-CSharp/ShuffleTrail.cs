public class ShuffleTrail : SinFollower
{
	public Cardtridge cardtridge;

	protected override void ReachedTarget()
	{
		if (cardtridge != null)
		{
			ShowCardtridge();
		}
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

	public ShuffleTrail Set(Cardtridge theCardtridge)
	{
		target = theCardtridge.transform;
		cardtridge = theCardtridge;
		return this;
	}

	private void ShowCardtridge()
	{
		cardtridge.visible = true;
		cardtridge.anim.SetTrigger("RefLoad");
		showedAnim = true;
	}

	private void OnDisable()
	{
		target = null;
		cardtridge = null;
	}
}
