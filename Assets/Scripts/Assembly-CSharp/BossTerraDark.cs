using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MEC;
using UnityEngine;

public class BossTerraDark : Boss
{
	public bool attacking = false;

	public bool darkStorm;

	public bool darkDrain;

	public bool darkZigZag;

	public bool darkSweepers;

	public bool darkUltima;

	public bool darkBreaker;

	public bool darkConvergence;

	public bool lastScene = false;

	public AudioClip finalDeathExplosionSound;

	private bool finalDowned = false;

	private Being gateProp;

	public override void Start()
	{
		base.Start();
		ctrl.RemoveObstacle(this);
		if (runCtrl.currentRun.HasAssist("BossTerra"))
		{
			runCtrl.currentRun.assists.Remove("BossTerra");
		}
		if (mov.currentTile != battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.All, 21), 1, this)[0])
		{
			mov.TeleportToTile(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.Moveable, 21), 1, this)[0]);
		}
		gateProp = ctrl.sp.PlaceBeing("GateProp", ti.mainBattleGrid.grid[7, 1], 0, true);
		gateProp.shadow.SetActive(false);
	}

	protected override void Update()
	{
		base.Update();
		if ((bool)gateProp)
		{
			gateProp.mov.forced = true;
		}
	}

	public override void StartAction()
	{
		mov.PatrolRandom();
	}

	protected override void AddTierToNameText()
	{
	}

	public override IEnumerator Loop()
	{
		if (!introPlayed)
		{
			if (S.I.ANIMATIONS)
			{
				base.dontInterruptAnim = true;
				AddInvince(3.7f);
				base.transform.right = Vector3.zero - Vector3.left;
				anim.SetTrigger("charge");
				anim.SetTrigger("channel");
				S.I.PlayOnce(soundEffects[0]);
				yield return new WaitForSeconds(2f);
				anim.SetTrigger("release");
				CastSpellObj("DarkMeteor");
				yield return new WaitForSeconds(1f);
				ctrl.muCtrl.PlayCharacterTheme(beingObj.nameString);
				yield return new WaitForSeconds(0.5f);
				base.transform.right = Vector3.left;
				yield return StartCoroutine(_StartDialogue("Intro"));
				base.dontInterruptAnim = false;
			}
			else
			{
				base.transform.right = Vector3.zero - Vector3.left;
				CastSpellObj("DarkMeteor");
				yield return new WaitForSeconds(0.5f);
				base.transform.right = Vector3.left;
			}
		}
		ctrl.RemoveObstacle(this);
		int arger = 0;
		while (true)
		{
			LoopStart();
			if (stage == 0)
			{
				if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
				{
					if (darkStorm)
					{
						yield return StartCoroutine(_DarkStorm());
					}
					if (darkDrain)
					{
						yield return StartCoroutine(_DarkDrain());
					}
					else if (darkZigZag)
					{
						yield return StartCoroutine(_DarkZigZag());
					}
					else if (darkSweepers)
					{
						yield return StartCoroutine(_DarkSweepers());
					}
					else if (darkUltima)
					{
						yield return StartCoroutine(_DarkUltima());
					}
					else if (darkBreaker)
					{
						yield return StartCoroutine(_DarkBreaker());
					}
					else if (darkConvergence)
					{
						yield return StartCoroutine(_DarkConvergence(1));
					}
					yield return new WaitForSecondsRealtime(3f);
					continue;
				}
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(_DarkStorm());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(_DarkDrain());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_DarkConvergence(0));
				}
				else
				{
					yield return StartCoroutine(_DarkUltima());
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(_DarkStorm());
				}
				else if (Utils.RandomBool(4))
				{
					yield return StartCoroutine(_DarkUltima());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_DarkConvergence(1));
				}
				else
				{
					yield return StartCoroutine(_DarkSweepers());
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(4))
				{
					yield return StartCoroutine(_DarkUltima());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(_DarkConvergence(2));
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_DarkSweepers());
				}
				else
				{
					yield return StartCoroutine(_DarkBreaker());
				}
			}
			while (loopDelay > 0f)
			{
				loopDelay -= Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			for (int i = 0; i < 3; i++)
			{
				mov.PatrolRandomEmpty();
				yield return new WaitUntil(() => mov.state == State.Idle);
				yield return new WaitForSeconds(beingObj.movementDelay);
			}
			StageCheck();
			arger++;
		}
	}

	private IEnumerator _DarkStorm()
	{
		yield return new WaitForSeconds(0.5f);
		base.dontInterruptAnim = true;
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		spellObjList[0].StartCast();
		yield return new WaitForSeconds(4.9f);
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(0.4f);
	}

	private IEnumerator _DarkDrain()
	{
		base.dontInterruptAnim = true;
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		int shots = 0;
		while (ctrl.currentPlayer.duelDisk.currentMana >= 1f)
		{
			ctrl.currentPlayer.duelDisk.currentMana -= 0.5f;
			spellObjList[1].StartCast();
			yield return new WaitForSeconds(0.2f);
			shots++;
			if (shots > 8)
			{
				break;
			}
		}
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(0.65f);
	}

	private IEnumerator _DarkZigZag()
	{
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.5f);
		anim.SetTrigger("release");
		savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Front, Shape.Column), 4, this));
		spellObjList[2].StartCast();
		yield return new WaitForEndOfFrame();
	}

	private IEnumerator _DarkSweepers()
	{
		ResetAnimTriggers();
		yield return new WaitForSeconds(0.5f);
		base.dontInterruptAnim = true;
		spellObjList[3].StartCast();
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(5f);
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(0.5f);
		mov.SetState(State.Idle);
	}

	private IEnumerator _DarkUltima()
	{
		yield return new WaitForSeconds(0.15f);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.RandOtherUnique), 0, this)[0], true, false, true, false);
		CastSpellObj("DarkUltimaWarning");
		yield return new WaitForSeconds(0.3f);
		base.dontInterruptAnim = true;
		for (int i = 0; i < 3; i++)
		{
			anim.SetTrigger("charge");
			anim.SetTrigger("release");
			yield return new WaitForSeconds(0.2f);
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.End), 0, this));
			spellObjList[6].StartCast();
			yield return new WaitForSeconds(0.45f);
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.End, Shape.O), 0, this));
			spellObjList[6].StartCast();
			yield return new WaitForSeconds(0.45f);
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.End, Shape.OBig), 0, this));
			spellObjList[6].StartCast();
			yield return new WaitForSeconds(0.45f);
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.End, Shape.OBigger), 0, this));
			spellObjList[6].StartCast();
			if (i < 2)
			{
				mov.MoveToTile(battleGrid.Get(new TileApp(Location.RandOtherUnique), 0, this)[0], true, false, true, false);
				yield return new WaitWhile(() => mov.state == State.Moving);
			}
			yield return new WaitForSeconds(0.1f);
		}
		base.dontInterruptAnim = false;
		mov.MoveTo(mov.currentTile.x, mov.currentTile.y, true, false);
		yield return new WaitForSeconds(0.3f);
	}

	private IEnumerator _DarkConvergence(int attackType)
	{
		yield return new WaitForSeconds(0.3f);
		base.dontInterruptAnim = true;
		yield return new WaitForSeconds(0.3f);
		for (int i = 0; i < 6; i++)
		{
			CastSpellObj("DarkConvergeWarning");
			anim.SetTrigger("specialStart");
			anim.SetTrigger("specialEnd");
			switch (attackType)
			{
			case 0:
				if (i % 2 == 0)
				{
					CastSpellObj("DarkConvergeCross");
					yield return new WaitForSeconds(0.6f);
				}
				else
				{
					CastSpellObj("DarkConvergeX");
					yield return new WaitForSeconds(0.6f);
				}
				break;
			case 1:
				CastSpellObj("DarkConverge");
				yield return new WaitForSeconds(1.7f);
				if (i >= 3)
				{
					i += 2;
				}
				break;
			case 2:
				CastSpellObj("DarkConvergeSwirl");
				yield return new WaitForSeconds(1.8f);
				i++;
				break;
			}
			mov.MoveToRandom();
		}
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(0.4f);
	}

	private IEnumerator _DarkBreaker()
	{
		base.dontInterruptAnim = true;
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.3f);
		for (int i = 0; i < 6; i++)
		{
			spellObjList[4].StartCast();
			yield return new WaitForSeconds(0.4f);
		}
		anim.SetTrigger("release");
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("throw");
		spellObjList[5].StartCast();
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(0.6f);
	}

	public override IEnumerator ExecutePlayerC()
	{
		base.dontInterruptAnim = true;
		yield return new WaitForSeconds(0.2f);
		ResetAnimTriggers();
		yield return new WaitForSeconds(0.4f);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.HalfField, Shape.ColumnWide, Pattern.Moveable), 1, ctrl.currentPlayer));
		yield return new WaitWhile(() => mov.state == State.Moving);
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		yield return StartCoroutine(_StartDialogue("Execution"));
		yield return new WaitForSeconds(0.2f);
		yield return StartCoroutine(ResetAnimTriggersC());
		anim.SetTrigger("release");
		CastSpellObj("DarkExecution");
		yield return new WaitForSeconds(5f);
		base.dontInterruptAnim = false;
		if ((bool)ctrl.currentPlayer)
		{
			StartCoroutine(StartLoopC());
		}
	}

	protected override void LastWord()
	{
		StartCoroutine(_StartDialogue("Death"));
	}

	public override IEnumerator DownC(bool destroyStructures = true, bool showZoneButtons = false)
	{
		if (!lastScene)
		{
			lastScene = true;
			ctrl.DestroyEnemiesAndStructures(this);
			invinceFlash = false;
			AddInvince(5.7f);
			dontMoveAnim = true;
			base.dontInterruptAnim = false;
			dontExecutePlayer = true;
			anim.SetTrigger("revive");
			ctrl.camCtrl.Shake(3);
			health.current = 1;
			battleGrid.FixAllTiles();
			RemoveAllStatuses();
			yield return new WaitForSeconds(0.1f);
			base.transform.right = Vector3.left;
			RemoveAllStatuses();
			mov.MoveToTile(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.Moveable, 15), 1, this));
			yield return new WaitForSeconds(0.6f);
			RemoveAllStatuses();
			yield return new WaitForSeconds(0.1f);
			health.ModifyHealth(500);
			anim.SetTrigger("back");
			yield return new WaitForSeconds(0.2f);
			foreach (Player thePlayer5 in ctrl.currentPlayers)
			{
				thePlayer5.undownForAssist = false;
				thePlayer5.ApplyStun(false);
				thePlayer5.AddControlBlock(Block.Fake);
			}
			anim.SetTrigger("charge");
			anim.SetTrigger("channel");
			while ((bool)ctrl.currentPlayer && !ctrl.currentPlayer.downed)
			{
				CastSpellObj("DarkWave");
				yield return new WaitForSeconds(0.5f);
			}
			anim.SetTrigger("release");
			ctrl.currentPlayer.mov.MoveTo(mov.currentTile.x - 5, mov.currentTile.y, false, true, false, false, false, true, true, false);
			yield return new WaitForSeconds(0.3f);
			yield return StartCoroutine(_StartDialogue("Finisher"));
			health.SetHealth(500);
			RemoveAllStatuses();
			RemoveAllBuffs();
			foreach (Player thePlayer4 in ctrl.currentPlayers)
			{
				thePlayer4.dontInterruptAnim = false;
			}
			foreach (Player thePlayer3 in ctrl.currentPlayers)
			{
				thePlayer3.RemoveControlBlock(Block.Fake);
				thePlayer3.RemoveAllStatuses();
			}
			anim.SetTrigger("charge");
			anim.SetTrigger("channel");
			S.I.PlayOnce(soundEffects[0]);
			yield return new WaitForSeconds(0.1f);
			yield return new WaitForSeconds(2f);
			anim.SetTrigger("release");
			CastSpellObj("DarkMeteorFinal");
			battleGrid.FixAllTiles();
			if (ctrl.currentPlayers.Count > 1)
			{
				ctrl.currentPlayer.AddInvince(3.5f);
			}
			yield return new WaitForSeconds(2.5f);
			battleGrid.FixAllTiles();
			if ((bool)ctrl.currentPlayer)
			{
				yield return StartCoroutine(_StartDialogue("Blocked"));
				yield return new WaitForSeconds(0.4f);
				anim.SetTrigger("charge");
				anim.SetTrigger("channel");
				S.I.PlayOnce(soundEffects[0]);
				SetInvince(0f);
				yield return ctrl.StartCoroutine(ctrl._BattleAssists(1, true));
			}
			health.deathTriggered = false;
			col.enabled = true;
			yield return new WaitForSeconds(6f);
			if (!finalDowned)
			{
				while ((bool)ctrl.currentPlayer)
				{
					Damage(1000, true, true);
					yield return new WaitForSeconds(1.5f);
				}
			}
		}
		else
		{
			foreach (Player thePlayer2 in ctrl.currentPlayers)
			{
				thePlayer2.RemoveAllStatuses();
			}
			finalDowned = true;
			StartCoroutine(_003C_003En__0(false, true));
			foreach (Structure cpu in battleGrid.currentStructures)
			{
				if (cpu.beingObj.beingID == "Turrethazelex")
				{
					cpu.Damage(400);
				}
			}
			AddInvince(3f);
			foreach (Player thePlayer in ctrl.currentPlayers)
			{
				thePlayer.Undown();
			}
		}
		health.deathTriggered = false;
		col.enabled = true;
	}

	public override IEnumerator DownTalkC()
	{
		yield return new WaitForSeconds(1f);
		yield return StartCoroutine(_003C_003En__1());
	}

	public override IEnumerator<float> _DeathFinal()
	{
		inDeathSequence = true;
		anim.runtimeAnimatorController = ctrl.itemMan.GetAnim("TerraDarkDown");
		LastWord();
		for (int i = 1; i < 6; i++)
		{
			sp.explosionGen.CreateExplosionString(i, base.transform.position, base.transform.rotation, this);
			S.I.PlayOnce(explosionSound);
			ctrl.camCtrl.Shake(2);
			yield return Timing.WaitForSeconds(0.35f);
		}
		S.I.PlayOnce(explosionSound);
		ctrl.camCtrl.Shake(2);
		ctrl.Victory(Ending.PacifistFalse, "WhiteExplosion");
		yield return Timing.WaitForSeconds(0.2f);
		S.I.PlayOnce(explosionSound);
		yield return Timing.WaitForSeconds(0.5f);
		S.I.PlayOnce(finalDeathExplosionSound);
		Remove();
		yield return Timing.WaitForSeconds(0.2f);
		inDeathSequence = false;
		spriteRend.enabled = false;
		foreach (Player thePlayer in ctrl.currentPlayers)
		{
			thePlayer.ApplyStun(false);
		}
		AchievementsCtrl.UnlockAchievement("Terrable");
		base.gameObject.SetActive(false);
	}

	protected override IEnumerator SpareC(ZoneDot nextZoneDot)
	{
		anim.runtimeAnimatorController = ctrl.itemMan.GetAnim("TerraDarkSpare");
		foreach (Player thePlayer in ctrl.currentPlayers)
		{
			thePlayer.ApplyStun(false);
		}
		AddInvince(99f);
		anim.SetTrigger("toIdle");
		talkBubble.Hide();
		ctrl.muCtrl.PauseIntroLoop();
		runCtrl.currentRun.AddAssist(beingObj);
		yield return new WaitForSeconds(0.3f);
		yield return StartCoroutine(_StartDialogue("Spare"));
		anim.SetTrigger("back");
		yield return new WaitForSeconds(0.3f);
		base.transform.right = Vector3.zero - Vector3.left;
		yield return new WaitForSeconds(0.5f);
		anim.SetBool("dashing", true);
		yield return new WaitForSeconds(0.4f);
		AchievementsCtrl.UnlockAchievement("Happily_Ever_After");
		ctrl.Victory(Ending.PacifistTrue, "LeftWipe");
	}

	protected override void DownEffects()
	{
		base.DownEffects();
	}

	public override IEnumerator _Mercy()
	{
		yield return StartCoroutine(ExecutePlayerC());
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__0(bool destroyStructures, bool showZoneButtons)
	{
		return base.DownC(destroyStructures, showZoneButtons);
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__1()
	{
		return base.DownTalkC();
	}
}
