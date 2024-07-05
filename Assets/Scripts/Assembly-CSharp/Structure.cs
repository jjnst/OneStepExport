public class Structure : Cpu
{
	public override void Start()
	{
		base.Start();
		if (!mov.neverOccupy)
		{
			mov.currentTile.SetOccupation(2, mov.hovering);
		}
	}

	public override void OnHit(Projectile attackRef)
	{
		base.OnHit(attackRef);
		if (!beingObj.tags.Contains(Tag.NotStructure))
		{
			TriggerAllArtifacts(FTrigger.OnStructureHit, this);
		}
	}

	public override void AfterHit(Projectile attackRef)
	{
		base.AfterHit(attackRef);
		if (!beingObj.tags.Contains(Tag.NotStructure))
		{
			TriggerAllArtifacts(FTrigger.AfterStructureHit, this);
		}
	}

	public override void Remove()
	{
		ctrl.StructureRemoved(this);
		base.Remove();
	}

	protected override bool DeathEffects(bool triggerDeathrattles)
	{
		if (base.DeathEffects(triggerDeathrattles) && !beingObj.tags.Contains(Tag.NotStructure))
		{
			ctrl.lastKilledStructure = this;
			TriggerAllArtifacts(FTrigger.OnStructureKill, this);
		}
		if (beingObj.beingID == "Turretsd" && !AchievementsCtrl.IsUnlocked("Desecration") && !IsReference())
		{
			AchievementsCtrl.UnlockAchievement("Desecration");
		}
		if (lastSpellHit != null && lastSpellHit.itemID == "Devour" && beingObj.tags.Contains(Tag.Hostage))
		{
			AchievementsCtrl.UnlockAchievement("Cannibal");
		}
		return true;
	}
}
