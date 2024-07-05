using System.Collections;
using UnityEngine;

public class BossGate : Boss
{
	public bool attacking = false;

	public bool summonTop;

	public bool summonBot;

	public bool summonBoth;

	public bool summonCannons;

	public bool tracker;

	private TileApp topTurretTile;

	private TileApp botTurretTile;

	private bool inPosition = false;

	public Vector2 turretHitbox;

	public Shader gateHitShader;

	public override void Start()
	{
		hitShader = gateHitShader;
		base.Start();
		mov.MoveTo(7, 1);
		spriteRend.sortingOrder = -1;
		col.size = new Vector2(sp.baseHurtboxSize * 4, sp.baseHurtboxSize * 5);
		col.offset = Vector2.right * sp.baseHurtboxSize * 0.3f + Vector2.up * sp.baseHurtboxSize * 0.66f;
		topTurretTile = new TileApp(Location.Index, Shape.Default, Pattern.All, 23);
		botTurretTile = new TileApp(Location.Index, Shape.Default, Pattern.All, 7);
		shadow.SetActive(false);
		AchievementsCtrl.UnlockAchievement("One_Step_From_Eden");
	}

	protected override void Update()
	{
		base.Update();
		spriteRend.sharedMaterial.SetColor("_TintRGBA_Color_1", Color.clear);
		foreach (Cpu currentEnemy in battleGrid.currentEnemies)
		{
			currentEnemy.mov.forced = true;
			if (currentEnemy != this && currentEnemy.col.size != turretHitbox)
			{
				currentEnemy.col.size = turretHitbox;
			}
		}
		if (inPosition)
		{
			mov.forced = true;
		}
	}

	protected override void AddTierToNameText()
	{
	}

	public override void StartAction()
	{
		mov.PatrolRandom();
	}

	public override IEnumerator Loop()
	{
		int arger = 0;
		while (true)
		{
			LoopStart();
			inPosition = true;
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			if (stage == 0)
			{
				if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
				{
					if (summonTop)
					{
						yield return StartCoroutine(SummonTop());
					}
					else if (summonBot)
					{
						yield return StartCoroutine(SummonBot());
					}
					else if (summonBoth)
					{
						yield return StartCoroutine(SummonBoth());
					}
					else if (summonCannons)
					{
						yield return StartCoroutine(SummonCannons());
					}
					else if (tracker)
					{
						yield return StartCoroutine(Tracker());
					}
					yield return new WaitForSecondsRealtime(3f);
					continue;
				}
				if (Utils.RandomBool(2) && battleGrid.Get(topTurretTile, 1, this)[0].occupation == 0)
				{
					yield return StartCoroutine(SummonTop());
				}
				else if (battleGrid.Get(botTurretTile, 1, this)[0].occupation == 0)
				{
					yield return StartCoroutine(SummonBot());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(SummonTop());
				}
				else
				{
					yield return StartCoroutine(SummonBot());
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(SummonCannons());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(Tracker());
				}
				else
				{
					yield return StartCoroutine(SummonBoth());
				}
			}
			else if (stage == 2)
			{
				yield return StartCoroutine(SummonBoth());
				yield return StartCoroutine(Tracker());
				yield return StartCoroutine(SummonCannons());
			}
			yield return new WaitForSeconds(loopDelay);
			StageCheck();
			arger++;
		}
	}

	private int GetRandomTurretInt()
	{
		return Random.Range(0, 4);
	}

	private IEnumerator SummonTop()
	{
		yield return new WaitForSeconds(1.5f);
		savedTileList = battleGrid.Get(topTurretTile, 1, this);
		FixSavedTile();
		spellObjList[GetRandomTurretInt()].StartCast();
		yield return new WaitForSeconds(5 - stage);
	}

	private IEnumerator SummonBot()
	{
		yield return new WaitForSeconds(0.5f);
		savedTileList = battleGrid.Get(botTurretTile, 1, this);
		FixSavedTile();
		spellObjList[GetRandomTurretInt()].StartCast();
		yield return new WaitForSeconds(5 - stage);
	}

	private IEnumerator SummonBoth()
	{
		yield return new WaitForSeconds(0.5f);
		savedTileList = battleGrid.Get(topTurretTile, 1, this);
		FixSavedTile();
		spellObjList[GetRandomTurretInt()].StartCast();
		yield return new WaitForSeconds(1.5f);
		savedTileList = battleGrid.Get(botTurretTile, 1, this);
		FixSavedTile();
		spellObjList[GetRandomTurretInt()].StartCast();
		yield return new WaitForSeconds(3 - stage);
	}

	private IEnumerator SummonTopGround()
	{
		yield return new WaitForSeconds(1.5f);
		savedTileList = battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.Moveable, 31), 1, this);
		FixSavedTile();
		CastSpellObj("GateSummonCannon");
		yield return new WaitForSeconds(3 - stage);
	}

	private IEnumerator SummonBotGround()
	{
		yield return new WaitForSeconds(1.5f);
		savedTileList = battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.Moveable, 15), 1, this);
		FixSavedTile();
		CastSpellObj("GateSummonCannon");
		yield return new WaitForSeconds(3 - stage);
	}

	private IEnumerator SummonCannons()
	{
		yield return new WaitForSeconds(0.5f);
		savedTileList = battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.Moveable, 31), 1, this);
		CastSpellObj("GateSummonCannon");
		yield return new WaitForSeconds(2f);
		savedTileList = battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.Moveable, 15), 1, this);
		FixSavedTile();
		CastSpellObj("GateSummonCannon");
		yield return new WaitForSeconds(4 - stage);
	}

	private IEnumerator Tracker()
	{
		legacySpellList[0].StartCoroutine(legacySpellList[0].Cast());
		yield return new WaitForSeconds(1f);
	}

	private void FixSavedTile()
	{
		if (savedTileList.Count > 0)
		{
			savedTileList[0].Fix();
		}
	}

	public override IEnumerator ExecutePlayerC()
	{
		yield return new WaitForSeconds(0.2f);
		ResetAnimTriggers();
		yield return StartCoroutine(SummonCannons());
		yield return new WaitForSeconds(0.3f);
		yield return new WaitForSeconds(3f);
		if (ctrl.PlayersActive())
		{
			StartCoroutine(StartLoopC());
		}
	}

	public override IEnumerator DownC(bool destroyStructures = true, bool showZoneButtons = true)
	{
		if (destroyStructures)
		{
			ctrl.DestroyEnemiesAndStructures(this);
		}
		ClearProjectiles();
		invinceFlash = false;
		legacySpellList[0].GetComponent<LaserTracker>().CancelSpell();
		runCtrl.currentRun.gateDefeated = true;
		if (S.I.ANIMATIONS)
		{
			for (int i = 0; i < 9; i++)
			{
				sp.explosionGen.spawnRadius = 100;
				int explosionSize = 2;
				if (Utils.RandomBool(2))
				{
					explosionSize = 9;
					ctrl.camCtrl.Shake(3);
				}
				sp.explosionGen.CreateExplosionString(explosionSize, base.transform.position, base.transform.rotation, this);
				S.I.PlayOnce(explosionSound);
				ctrl.camCtrl.Shake(2);
				yield return new WaitForSeconds(0.5f);
				if (destroyStructures)
				{
					ctrl.DestroyEnemiesAndStructures(this);
				}
			}
			ctrl.camCtrl.TransitionInHigh("WhiteExplosion");
			yield return new WaitForSeconds(1.2f);
			ctrl.camCtrl.TransitionOutHigh("WhiteExplosion");
			AddInvince(9999.7f);
			downed = true;
			dontMoveAnim = true;
			base.dontInterruptAnim = false;
			anim.SetTrigger("down");
			ctrl.camCtrl.Shake(3);
			health.current = 1;
			DestroySpells();
			RemoveAllStatuses();
			yield return new WaitForSeconds(0.1f);
			RemoveAllStatuses();
			yield return new WaitForSeconds(0.6f);
		}
		endGameOnExecute = false;
		RemoveAllStatuses();
		ctrl.DestroyEnemiesAndStructures(this);
		health.deathTriggered = false;
		col.enabled = true;
		yield return new WaitForSeconds(0.6f);
		beingObj.mustDestroy = false;
		ctrl.RemoveObstacle(this);
		sp.explosionGen.ResetRadius();
		battleGrid.FixAllTiles();
		Being execSerif = sp.PlaceBeing("BossSerif", battleGrid.Get(new TileApp(Location.Index, Shape.ColumnTwo, Pattern.Moveable, 6), 0, this)[0], 0, true);
		execSerif.GetComponent<BossSerif>().GateExecution(this);
	}

	public override void ExecutePlayer()
	{
	}

	protected override void DownEffects()
	{
		base.DownEffects();
	}

	public override IEnumerator DownTalkC()
	{
		yield return new WaitForSeconds(0.1f);
	}

	public override IEnumerator _Mercy()
	{
		yield return StartCoroutine(ExecutePlayerC());
	}
}
