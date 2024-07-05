using System.Collections.Generic;
using MEC;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class PlayerHalf : Player
{
	public int playerHalfNum = 0;

	public PlayerHalf otherHalf;

	public bool bossExecuteCalled = false;

	public bool inTempDeath = false;

	public override void BattleStartReset()
	{
		base.BattleStartReset();
		bossExecuteCalled = false;
	}

	protected override void Update()
	{
		base.Update();
	}

	public override void Setup()
	{
		base.Setup();
		health.onSetMax += SyncHealth;
	}

	private void OnDisable()
	{
		health.onSetMax -= SyncHealth;
	}

	private void SyncHealth()
	{
		if ((bool)otherHalf && otherHalf.health.max != health.max)
		{
			otherHalf.health.SetMax(health.max);
		}
	}

	public override void Move(int x, int y)
	{
		for (int i = 0; i < currentPets.Count; i++)
		{
			if (i > 0)
			{
				currentPets[i].mov.MoveToTile(currentPets[i - 1].mov.startTile, false, false, false, false);
			}
			else
			{
				currentPets[i].mov.MoveToTile(mov.startTile, false, false, false, false);
			}
		}
		if (mov.currentTile == otherHalf.mov.currentTile && !otherHalf.inTempDeath)
		{
			mov.currentTile.SetOccupation(1);
		}
		if (TileLocal(x, y) == otherHalf.mov.currentTile)
		{
			mov.Move(x, y, false, true, false, false, true, true, this);
		}
		else
		{
			mov.Move(x, y);
		}
		if (!otherHalf.inTempDeath)
		{
			otherHalf.mov.currentTile.SetOccupation(1);
		}
	}

	protected override int GetSlotNum(int theSlotNum)
	{
		theSlotNum = playerHalfNum;
		return theSlotNum;
	}

	public void StartFinalDeath()
	{
		if (!inDeathSequence)
		{
			ApplyStun(false);
			base.dontInterruptAnim = true;
			inDeathSequence = true;
			otherHalf.anim.SetTrigger("die");
			Timing.RunCoroutine(_DeathFinal().CancelWith(base.gameObject));
		}
	}

	protected override void StartDownC()
	{
		if (!otherHalf.inTempDeath)
		{
			Timing.RunCoroutine(DeathSequence());
		}
		else
		{
			Timing.RunCoroutine(DownC());
		}
	}

	public override void Undown()
	{
		base.Undown();
		bossExecuteCalled = false;
		if (otherHalf.downed)
		{
			otherHalf.Undown();
		}
	}

	public void RestoreFromTempDeath()
	{
		inTempDeath = false;
		duelDisk.ShowCardRefGrid(true, playerHalfNum);
		battleGrid.currentAllies.Add(this);
		battleGrid.currentBeings.Add(this);
		if ((bool)col)
		{
			col.enabled = true;
		}
		anim.enabled = true;
		spriteRend.enabled = true;
		mov.currentTile.SetOccupation(1, mov.hovering);
		health.text.enabled = true;
		shadow.SetActive(true);
		aimMarker.enabled = true;
		if (!ctrl.currentPlayers.Contains(this))
		{
			ctrl.currentPlayers.Insert(playerHalfNum, this);
		}
		health.SetHealth(100, health.max);
		anim.SetTrigger("revive");
		deathrattlesTriggered = false;
		CreateHitFX(Status.Resurrect);
		PlayOnce("misc_shimmer");
		ClearQueuedActions();
		RemoveControlBlock(Block.Dead);
		RemoveAllStatuses();
		Undown();
		health.deathTriggered = false;
		mov.SetState(State.Idle);
	}

	public void TempRemove()
	{
		if ((bool)col)
		{
			col.enabled = false;
		}
		if (ctrl.currentPlayers.Contains(this))
		{
			ctrl.currentPlayers.Remove(this);
			if (ctrl.currentPlayer == this)
			{
				ctrl.currentPlayer = otherHalf;
			}
		}
		mov.currentTile.SetOccupation(0, mov.hovering);
		anim.enabled = false;
		spriteRend.enabled = false;
		RemoveAllStatuses();
		statusEffectList.Clear();
		for (int num = statusDisplays.Count - 1; num >= 0; num--)
		{
			if ((bool)statusDisplays[num])
			{
				Object.Destroy(statusDisplays[num].gameObject);
			}
		}
		statusDisplays.Clear();
		beingStatsPanel.gameObject.SetActive(true);
		health.text.enabled = false;
		shadow.SetActive(false);
		aimMarker.enabled = false;
		currentPets.Clear();
	}

	public override IEnumerator<float> _DeathFinal()
	{
		inDeathSequence = true;
		sp.explosionGen.CreateExplosionString(1, base.transform.position, base.transform.rotation, this);
		yield return float.NegativeInfinity;
		battleGrid.currentAllies.Remove(this);
		battleGrid.currentBeings.Remove(this);
		duelDisk.ShowCardRefGrid(false, playerHalfNum);
		beingStatsPanel.gameObject.SetActive(false);
		if ((bool)duelDisk.castSlots[playerHalfNum].cardtridgeFill)
		{
			duelDisk.castSlots[playerHalfNum].cardtridgeFill.Eject();
		}
		duelDisk.castSlots[playerHalfNum].Empty();
		inTempDeath = true;
		TempRemove();
		if (!otherHalf.inTempDeath)
		{
			inDeathSequence = false;
			yield break;
		}
		dead = true;
		yield return Timing.WaitForSeconds(0.2f);
		inDeathSequence = false;
		if (playerHalfNum == 0)
		{
			if (ctrl.pvpMode)
			{
				ctrl.EndPvPRound(this);
			}
			else
			{
				ctrl.Defeat();
			}
		}
		if (!ctrl.pvpMode)
		{
			ctrl.Defeat();
		}
		Object.Destroy(base.gameObject);
	}

	public override void Heal(int amount)
	{
		base.Heal(amount);
		if ((bool)otherHalf)
		{
			otherHalf.health.ModifyHealth(health.current - otherHalf.health.current);
		}
	}

	protected override void CallBossExecute()
	{
		if (!bossExecuteCalled)
		{
			bossExecuteCalled = true;
			otherHalf.bossExecuteCalled = true;
			battleGrid.currentEnemies[0].GetComponent<Boss>().ExecutePlayer();
		}
	}
}
