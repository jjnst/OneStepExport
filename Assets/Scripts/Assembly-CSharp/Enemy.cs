using MoonSharp.Interpreter;

[MoonSharpUserData]
public class Enemy : Cpu
{
	public override void Start()
	{
		base.Start();
		baseLoopDelay = beingObj.baseLoopDelay;
	}

	public override void OnHit(Projectile attackRef)
	{
		base.OnHit(attackRef);
		TriggerAllArtifacts(FTrigger.OnEnemyHit, this);
	}

	public override void AfterHit(Projectile attackRef)
	{
		base.AfterHit(attackRef);
		TriggerAllArtifacts(FTrigger.AfterEnemyHit, this);
	}

	public override void Remove()
	{
		ctrl.EnemyRemoved(this);
		base.Remove();
	}

	protected override bool DeathEffects(bool triggerDeathrattles)
	{
		if (base.DeathEffects(triggerDeathrattles))
		{
			ctrl.lastKilledEnemy = this;
			TriggerAllArtifacts(FTrigger.OnEnemyKill, this);
		}
		return true;
	}

	protected override void CalculateLoopDelay(bool staggerDelay)
	{
		int num = battleGrid.currentEnemies.Count;
		float num2 = 1f;
		foreach (Cpu currentEnemy in battleGrid.currentEnemies)
		{
			if (currentEnemy.beingObj.tags.Contains(Tag.Structure))
			{
				num2 += 0.15f;
				num--;
			}
			else if (currentEnemy.minion)
			{
				num2 += 0.1f;
				num--;
			}
		}
		if (num == 2)
		{
			num2 += 0.2f;
		}
		else if (num == 3)
		{
			num2 += 0.4f;
		}
		else if (num >= 4)
		{
			num2 += 0.6f;
		}
		if (runCtrl.currentHellPassNum > 0)
		{
			num2 *= 0.9f;
		}
		if (ctrl.optCtrl.settingsPane.angelModeEnabled == 1)
		{
			num2 *= 1.2f + ctrl.optCtrl.settingsPane.angelModeCurrentSpeedReduction / 2f;
		}
		if (staggerDelay)
		{
			beingObj.loopDelay = baseLoopDelay * num2;
		}
		else
		{
			beingObj.loopDelay = baseLoopDelay * num2;
		}
	}
}
