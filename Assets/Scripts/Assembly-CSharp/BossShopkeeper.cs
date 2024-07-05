using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BossShopkeeper : Boss
{
	public bool attacking = false;

	public bool deadspot;

	public bool clockwiseBlast;

	public bool spinBlast;

	public bool walls;

	public bool shards;

	public bool diagWave;

	public bool pushSlam;

	public bool testingMode;

	public bool rudeShown = false;

	private bool deadspotDone = false;

	private bool causeOfOpens = false;

	public override void Start()
	{
		loopDelay = 0f;
		base.Start();
		healthStages[0] = health.max - 100;
		ctrl.RemoveObstacle(this);
		RemoveAllBuffs();
	}

	protected override void Update()
	{
		base.Update();
		if (stage >= 1 && !rudeShown)
		{
			ctrl.shopCtrl.currentShopkeeper = null;
			ctrl.shopCtrl.Close();
		}
	}

	public override void StartAction()
	{
		mov.PatrolRandom();
	}

	public override IEnumerator Loop()
	{
		if (!testingMode)
		{
			if (S.I.ANIMATIONS)
			{
				yield return new WaitForSeconds(2f);
			}
			yield return _StartDialogue("Intro");
		}
		while (true)
		{
			if (stage == 0)
			{
				if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
				{
					if (testingMode)
					{
						anim.runtimeAnimatorController = ctrl.itemMan.GetAnim("Shopcreeper");
						if (deadspot)
						{
							yield return StartCoroutine(Deadspot());
						}
						else if (clockwiseBlast)
						{
							yield return StartCoroutine(ClockwiseBlast());
						}
						else if (spinBlast)
						{
							yield return StartCoroutine(SpinBlast());
						}
						else if (walls)
						{
							yield return StartCoroutine(Walls());
						}
						else if (shards)
						{
							yield return StartCoroutine(Shards());
						}
						else if (diagWave)
						{
							yield return StartCoroutine(DiagWave());
						}
						else if (pushSlam)
						{
							yield return StartCoroutine(PushSlam());
						}
					}
					yield return new WaitForSecondsRealtime(2f);
					continue;
				}
				yield return new WaitForEndOfFrame();
			}
			else if (stage == 1)
			{
				if (!rudeShown)
				{
					yield return StartCoroutine(_Convert());
				}
				if (Utils.RandomBool(4))
				{
					yield return StartCoroutine(ClockwiseBlast());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(Walls());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(PushSlam());
				}
				else
				{
					yield return StartCoroutine(DiagWave());
				}
			}
			else if (stage == 2)
			{
				if (!rudeShown)
				{
					yield return StartCoroutine(_Convert());
				}
				if (!deadspotDone)
				{
					deadspotDone = true;
					yield return StartCoroutine(Deadspot());
					yield return new WaitForSeconds(1.1f);
				}
				if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(SpinBlast());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(Shards());
				}
				else
				{
					yield return StartCoroutine(DiagWave());
				}
			}
			if (stage != 0)
			{
				yield return new WaitForSeconds(loopDelay);
				for (int i = 0; i < 3; i++)
				{
					mov.PatrolRandomEmpty();
					yield return new WaitUntil(() => mov.state == State.Idle);
					yield return new WaitForSeconds(beingObj.movementDelay);
				}
			}
			StageCheck();
		}
	}

	public void StartConvert(bool tooManyOpens = false)
	{
		causeOfOpens = tooManyOpens;
		health.SetHealth(health.max, health.max - 100);
	}

	private IEnumerator _Convert()
	{
		introPlayed = true;
		rudeShown = true;
		base.dontInterruptAnim = true;
		ctrl.perfectBattle = true;
		RemoveAllStatuses();
		ctrl.runCtrl.worldBar.Close();
		ctrl.idCtrl.HideOnwardButton();
		runCtrl.worldBar.available = false;
		ctrl.currentPlayer.duelDisk.CreateDeckSpells();
		foreach (Player thePlayer in ctrl.currentPlayers)
		{
			thePlayer.Reset();
		}
		SetInvince(2f);
		health.SetHealth(health.max, health.max - 100);
		health.ModifyHealth(9999);
		ctrl.shopCtrl.Close();
		if (runCtrl.currentRun.shopkeeperDowned)
		{
			yield return StartCoroutine(_StartDialogue("Serious"));
			for (int j = 0; j < 20 + tier; j++)
			{
				buffs.Add(deCtrl.CreateArtifact(ctrl.itemMan.buffList[runCtrl.NextPsuedoRand(0, ctrl.itemMan.buffList.Count)].itemID, this));
			}
		}
		else if (causeOfOpens)
		{
			yield return new WaitForSeconds(talkBubble.AnimateRandomLine("Shopkeeper/ConvertOpen") - 0.9f);
		}
		else
		{
			spellObjList[1].StartCast();
			yield return new WaitForSeconds(talkBubble.AnimateRandomLine("Shopkeeper/Convert") - 0.9f);
		}
		ctrl.shopCtrl.Close();
		for (int i = 0; i < 4 + tier; i++)
		{
			buffs.Add(deCtrl.CreateArtifact(ctrl.itemMan.buffList[runCtrl.NextPsuedoRand(0, ctrl.itemMan.buffList.Count)].itemID, this));
		}
		yield return new WaitForSeconds(0.3f);
		ctrl.shopCtrl.Close();
		Vector3 oldGunpointPos = gunPoint.transform.position;
		gunPoint.transform.position = new Vector3(12f, 49f, 0f);
		gunPoint.transform.position = oldGunpointPos;
		ctrl.GameState = GState.PreBattle;
		deCtrl.TriggerAllArtifacts(FTrigger.OnBattleStart);
		yield return new WaitForSeconds(1.1f);
		S.I.muCtrl.PlayCharacterTheme(beingObj.nameString);
		base.dontInterruptAnim = false;
		yield return StartCoroutine(ResetAnimTriggersC());
		anim.runtimeAnimatorController = ctrl.itemMan.GetAnim("Shopcreeper");
		ctrl.GameState = GState.Battle;
	}

	private IEnumerator Deadspot()
	{
		ResetAnimTriggers();
		base.dontInterruptAnim = true;
		mov.MoveToTile(battleGrid.Randomize(battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.Moveable, 3), 0, this), false));
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return new WaitForSeconds(0.35f);
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		savedTileList.Clear();
		savedTileList.AddRange(battleGrid.Get(new TileApp(Location.FrontAll, Shape.Column, Pattern.Random), 0, this));
		savedTileList.Remove(battleGrid.Get(new TileApp(Location.Front), 1, this)[0]);
		yield return new WaitForSeconds(0.4f);
		CastSpellObj("ShopkeeperDeadspot");
		yield return new WaitForSeconds(3.5f);
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
	}

	private IEnumerator ClockwiseBlast()
	{
		yield return new WaitForSeconds(0.35f);
		base.dontInterruptAnim = true;
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		int patternNum = 0;
		for (int i = 0; i < 12; i++)
		{
			savedTileList.Clear();
			switch (patternNum)
			{
			case 0:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Index, Shape.ColumnTwo, Pattern.All, 26), 0, this));
				break;
			case 1:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Index, Shape.HorizontalWide, Pattern.All, 26), 0, this));
				break;
			case 2:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Index, Shape.ColumnTwo, Pattern.All, 4), 0, this));
				break;
			case 3:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Index, Shape.HorizontalWide, Pattern.All, 4), 0, this));
				break;
			}
			patternNum++;
			if (patternNum > 3)
			{
				patternNum = 0;
			}
			CastSpellObj("ShopkeeperClockwiseBlast");
			yield return new WaitForSeconds(0.3f);
		}
		yield return new WaitForSeconds(0.5f);
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
	}

	private IEnumerator SpinBlast()
	{
		yield return new WaitForSeconds(0.35f);
		base.dontInterruptAnim = true;
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		int patternNum = 0;
		for (int i = 0; i < 8; i++)
		{
			savedTileList.Clear();
			switch (patternNum)
			{
			case 0:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.Column, Pattern.All, 4), 0, this));
				break;
			case 1:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.DiagonalTopLeft, Pattern.All, 4), 0, this));
				break;
			case 2:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.Horizontal, Pattern.All, 4), 0, this));
				break;
			case 3:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.DiagonalTopRight, Pattern.Reverse, 4), 0, this));
				break;
			case 4:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.Column, Pattern.Reverse, 4), 0, this));
				break;
			case 5:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.DiagonalTopLeft, Pattern.Reverse, 4), 0, this));
				break;
			case 6:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.Horizontal, Pattern.Reverse, 4), 0, this));
				break;
			case 7:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.DiagonalTopRight, Pattern.All, 4), 0, this));
				break;
			}
			patternNum++;
			CastSpellObj("ShopkeeperClockwiseBlast");
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(0.5f);
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
	}

	private IEnumerator Walls()
	{
		yield return new WaitForSeconds(0.35f);
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		savedTileList.Clear();
		savedTileList.AddRange(battleGrid.Get(new TileApp(Location.SpiralCounterClockwise), 15, this));
		CastSpellObj("ShopkeeperWalls");
		yield return new WaitForSeconds(0.4f);
		savedTileList = battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.All, 18), 0, this);
		CastSpellObj("ShopkeeperMeteor");
		yield return new WaitForSeconds(3.8f);
		anim.SetTrigger("release");
		yield return new WaitForSeconds(1.2f);
	}

	private IEnumerator Shards()
	{
		mov.MoveToTile(battleGrid.Randomize(battleGrid.Get(new TileApp(Location.Index, Shape.Column, Pattern.Moveable, 7), 0, this), false));
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return new WaitForSeconds(0.35f);
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		savedTileList.Clear();
		if (Utils.RandomBool(2))
		{
			savedTileList.AddRange(battleGrid.Get(new TileApp(Location.HalfField, Shape.Ring), 12, this));
		}
		else
		{
			savedTileList.AddRange(battleGrid.Get(new TileApp(Location.HalfField, Shape.Ring, Pattern.Reverse), 0, this));
		}
		GetSpellObj("ShopkeeperShards").efApps[0].amount = 300 + stage * 100;
		CastSpellObj("ShopkeeperShards");
		yield return new WaitForSeconds(0.4f);
		anim.SetTrigger("release");
		yield return new WaitForSeconds(4.4f);
		yield return new WaitForSeconds(1.9f + (float)stage * 0.7f);
	}

	private IEnumerator DiagWave()
	{
		ResetAnimTriggers();
		base.dontInterruptAnim = true;
		mov.MoveToTile(battleGrid.Randomize(battleGrid.Get(new TileApp(Location.Base, Shape.Row, Pattern.Moveable, 2), 0, this), false));
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return new WaitForSeconds(0.35f);
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		for (int i = 1; i <= mov.currentTile.x; i++)
		{
			savedTileList.Clear();
			savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Front, Shape.DiagonalTopLeft, Pattern.Reverse, i), 0, this));
			CastSpellObj("ShopkeeperClockwiseBlast");
			yield return new WaitForSeconds(0.05f);
			savedTileList.Clear();
			savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Front, Shape.DiagonalBotLeft, Pattern.All, i), 0, this));
			CastSpellObj("ShopkeeperClockwiseBlast");
			yield return new WaitForSeconds(0.3f);
		}
		yield return new WaitForSeconds(0.5f);
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator PushSlam()
	{
		base.dontInterruptAnim = true;
		float baseDelay = 0.5f;
		for (int j = 0; j <= tier; j++)
		{
			baseDelay -= 0.04f;
		}
		for (int i = 0; i < 5; i++)
		{
			SpellObject pushSpell = GetSpellObj("ShopkeeperPush");
			EffectApp pushApp = pushSpell.GetEffect(Effect.Move);
			GetSpellObj("ShopkeeperSmash");
			int direction = Random.Range(0, 4);
			switch (direction)
			{
			case 0:
				pushApp.amount = 0f;
				pushApp.duration = 1f;
				pushSpell.shotVelocity = 0f;
				pushSpell.shotVelocityY = 400f;
				pushSpell.spawnOffset = new Vector2(0f, -1f);
				savedTileList = battleGrid.Get(new TileApp(Location.Base, Shape.Horizontal, Pattern.All, 0), 0, this);
				break;
			case 1:
				pushApp.amount = 0f;
				pushApp.duration = -1f;
				pushSpell.shotVelocity = 0f;
				pushSpell.shotVelocityY = -400f;
				pushSpell.spawnOffset = new Vector2(0f, 1f);
				savedTileList = battleGrid.Get(new TileApp(Location.Base, Shape.Horizontal, Pattern.All, 3), 0, this);
				break;
			case 2:
				pushApp.amount = 1f;
				pushApp.duration = 0f;
				pushSpell.shotVelocity = 600f;
				pushSpell.shotVelocityY = 0f;
				pushSpell.spawnOffset = new Vector2(-1f, 0f);
				savedTileList = battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.All, 4), 0, this);
				break;
			case 3:
				pushApp.amount = -1f;
				pushApp.duration = 0f;
				pushSpell.shotVelocity = -600f;
				pushSpell.shotVelocityY = 0f;
				pushSpell.spawnOffset = new Vector2(1f, 0f);
				savedTileList = battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.All, 7), 0, this);
				break;
			}
			CastSpellObj("ShopkeeperPush");
			anim.SetTrigger("throw");
			yield return new WaitForSeconds(0.29f);
			switch (direction)
			{
			case 0:
				savedTileList = battleGrid.Get(new TileApp(Location.Base, Shape.HorizontalTwo, Pattern.All, 2), 0, this);
				break;
			case 1:
				savedTileList = battleGrid.Get(new TileApp(Location.Base, Shape.HorizontalTwo, Pattern.All, 0), 0, this);
				break;
			case 2:
				savedTileList = battleGrid.Get(new TileApp(Location.Index, Shape.ColumnTwo, Pattern.All, 2), 0, this);
				break;
			case 3:
				savedTileList = battleGrid.Get(new TileApp(Location.Index, Shape.ColumnTwo, Pattern.All, 4), 0, this);
				break;
			}
			CastSpellObj("ShopkeeperSmash");
			yield return new WaitForSeconds(0.5f);
			anim.SetTrigger("throw");
			yield return new WaitForSeconds(0.18f);
		}
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(0.3f);
	}

	public override IEnumerator ExecutePlayerC()
	{
		if (!rudeShown)
		{
			if ((bool)ctrl.currentPlayer)
			{
				ctrl.currentPlayer.Damage(1, true, true, true);
			}
			yield break;
		}
		anim.SetTrigger("release");
		yield return new WaitForSeconds(0.2f);
		ResetAnimTriggers();
		yield return new WaitForSeconds(0.4f);
		while (ctrl.PlayersActive() && ctrl.currentPlayer.downed)
		{
			savedTileList = battleGrid.Get(new TileApp(Location.Player), 0, this);
			yield return StartCoroutine(_StartDialogue("Execution"));
			yield return new WaitForSeconds(0.3f);
			anim.SetTrigger("throw");
			CastSpellObj("ShopkeeperExecute");
			yield return new WaitForSeconds(2.2f);
		}
		if (ctrl.PlayersActive())
		{
			StartCoroutine(StartLoopC());
		}
	}

	public override IEnumerator DownC(bool destroyStructures = true, bool showZoneButtons = true)
	{
		if (rudeShown)
		{
			yield return StartCoroutine(_003C_003En__0(true, true));
			yield break;
		}
		yield return StartCoroutine(_Convert());
		health.deathTriggered = false;
		yield return StartCoroutine(Loop());
	}

	protected override void DownEffects()
	{
		if (rudeShown)
		{
			ctrl.shopCtrl.Close();
			ctrl.idCtrl.MakeWorldBarAvailable(true);
			runCtrl.currentRun.charUnlocks.Add(beingObj.nameString);
			runCtrl.currentRun.shopkeeperDowned = true;
			base.DownEffects();
			base.dontInterruptAnim = true;
			dontHitAnim = true;
			dontMoveAnim = true;
		}
	}

	protected override void LastWord()
	{
		runCtrl.currentRun.shopkeeperKilled = true;
		runCtrl.currentRun.yamiObtained = true;
		base.LastWord();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__0(bool destroyStructures, bool showZoneButtons)
	{
		return base.DownC(destroyStructures, showZoneButtons);
	}
}
