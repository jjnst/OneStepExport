public class Ally : Cpu
{
	public Being owner;

	public override void Start()
	{
		base.Start();
	}

	public override void OnHit(Projectile attackRef)
	{
		base.OnHit(attackRef);
		TriggerAllArtifacts(FTrigger.OnAllyHit, this);
	}

	public override void AfterHit(Projectile attackRef)
	{
		base.AfterHit(attackRef);
		TriggerAllArtifacts(FTrigger.AfterAllyHit, this);
	}

	public override void Remove()
	{
		ctrl.AllyRemoved(this);
		if (owner != null && owner.currentPets.Contains(this))
		{
			owner.currentPets.Remove(this);
			battleGrid.currentDeadBeings.Add(this);
		}
		base.Remove();
	}
}
