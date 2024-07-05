using System.Collections;
using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;

public class BossSerif : Boss
{
	public TMP_FontAsset fontAsset;

	public bool attacking = false;

	public bool loopThroughAll;

	private int loopNum = 0;

	public bool slashDash;

	public bool lightbombRain;

	public bool lightbombPath;

	public bool alternatingWaves;

	public bool swordRows;

	public bool swordFlurry;

	public bool rainDash;

	public bool glitterStorm;

	public bool spellKiller;

	public bool sweeper;

	public bool rainSweeper;

	public bool excaliburSlash;

	public bool offSwords;

	private int timesCastFlurry = 0;

	private bool lastScene = false;

	public Vector3 sweeperGunpointPos;

	private Player currentPlayer;

	private Cpu bossGate;

	private string savedPlayerWepString;

	private bool ignoreFacing = false;

	private bool stopAllFacing = false;

	public override void Start()
	{
		base.Start();
		ti = S.I.tiCtrl;
		currentPlayer = ctrl.currentPlayer;
		talkBubble.textBox.font = fontAsset;
	}

	protected override void Update()
	{
		base.Update();
		if (health.shield <= 0)
		{
			anim.SetBool("shield", false);
			ctrl.RemoveObstacle(this);
			col.size = Vector2.one * sp.baseHurtboxSize;
		}
		foreach (Player currentPlayer in ctrl.currentPlayers)
		{
			if (!currentPlayer || stopAllFacing)
			{
				continue;
			}
			if (currentPlayer.mov.currentTile.x > mov.currentTile.x)
			{
				if (!ignoreFacing)
				{
					base.transform.right = Vector3.zero - Vector3.left;
				}
				currentPlayer.transform.right = Vector3.zero - Vector3.right;
				foreach (Cpu currentPet in currentPlayer.currentPets)
				{
					currentPet.transform.right = Vector3.zero - Vector3.right;
				}
			}
			else
			{
				if (currentPlayer.mov.currentTile.x >= mov.currentTile.x)
				{
					continue;
				}
				if (!ignoreFacing)
				{
					base.transform.right = Vector3.zero - Vector3.right;
				}
				currentPlayer.transform.right = Vector3.zero - Vector3.left;
				foreach (Cpu currentPet2 in currentPlayer.currentPets)
				{
					currentPet2.transform.right = Vector3.zero - Vector3.left;
				}
			}
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
		ctrl.RemoveObstacle(this);
		if (!introPlayed)
		{
			AddInvince(6f);
			for (int j = 0; j < runCtrl.currentRun.loopNum; j++)
			{
				AddStatus(Status.SpellPower, 99f, 9999f);
				AddStatus(Status.Defense, 9f, 9999f);
			}
			base.dontInterruptAnim = true;
			yield return new WaitForSeconds(0.5f);
			mov.MoveToTile(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.Moveable, 21), 1, this));
			if (S.I.ANIMATIONS)
			{
				yield return new WaitForSeconds(0.7f);
				yield return StartCoroutine(_StartDialogue("Intro"));
				anim.SetTrigger("specialStart");
				yield return new WaitForSeconds(0.3f);
				anim.SetTrigger("channel");
				S.I.PlayOnce(soundEffects[0]);
				yield return new WaitForSeconds(0.7f);
				anim.SetTrigger("specialEnd");
			}
			else
			{
				introPlayed = true;
			}
			CastSpellObj("SerifTileChange");
			yield return new WaitForSeconds(0.3f);
			battleGrid.SetSerif();
			yield return new WaitForSeconds(1.3f);
			base.dontInterruptAnim = false;
		}
		int arger = 0;
		while (true)
		{
			LoopStart();
			if (stage == 0)
			{
				if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
				{
					if (loopThroughAll)
					{
						loopNum++;
						if (loopNum > 12)
						{
							loopNum = 1;
						}
					}
					if (slashDash || loopNum == 1)
					{
						yield return StartCoroutine(SlashDash());
					}
					else if (lightbombRain || loopNum == 2)
					{
						yield return StartCoroutine(LightbombRain());
					}
					else if (lightbombPath || loopNum == 3)
					{
						yield return StartCoroutine(LightbombPath());
					}
					else if (alternatingWaves || loopNum == 4)
					{
						yield return StartCoroutine(AlternatingWaves());
					}
					else if (swordRows || loopNum == 5)
					{
						yield return StartCoroutine(SwordRows());
					}
					else if (swordFlurry || loopNum == 6)
					{
						yield return StartCoroutine(SwordFlurry());
					}
					else if (rainDash || loopNum == 7)
					{
						yield return StartCoroutine(RainDash());
					}
					else if (glitterStorm || loopNum == 8)
					{
						yield return StartCoroutine(GlitterStorm());
					}
					else if (spellKiller || loopNum == 9)
					{
						yield return StartCoroutine(SpellEjector());
					}
					else if (sweeper || loopNum == 10)
					{
						yield return StartCoroutine(Sweeper());
					}
					else if (rainSweeper || loopNum == 11)
					{
						yield return StartCoroutine(RainSweeper());
					}
					else if (excaliburSlash || loopNum == 12)
					{
						yield return StartCoroutine(ExcaliburSlash());
					}
					else if (offSwords || loopNum == 13)
					{
						yield return StartCoroutine(OffSwords());
					}
					yield return new WaitForSecondsRealtime(2f);
					continue;
				}
				if (Utils.RandomBool(4))
				{
					yield return StartCoroutine(SlashDash());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(RainDash());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(AlternatingWaves());
				}
				else
				{
					yield return StartCoroutine(SwordRows());
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(4))
				{
					yield return StartCoroutine(ExcaliburSlash());
				}
				else if (Utils.RandomBool(3))
				{
					if (Utils.RandomBool(2))
					{
						yield return StartCoroutine(OffSwords());
					}
					else
					{
						yield return StartCoroutine(Sweeper());
					}
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(LightbombRain());
				}
				else if ((bool)player && (bool)player.duelDisk && player.duelDisk.currentCardtridges != null)
				{
					yield return StartCoroutine(SpellEjector());
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(4))
				{
					yield return StartCoroutine(LightbombPath());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(SwordFlurry());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(RainSweeper());
				}
				else
				{
					yield return StartCoroutine(GlitterStorm());
				}
			}
			yield return new WaitForSeconds(loopDelay);
			for (int i = 0; i < 3; i++)
			{
				mov.PatrolRandomEmpty();
				yield return new WaitUntil(() => mov.state == State.Idle);
				yield return new WaitForSeconds(beingObj.movementDelay);
			}
			StageCheck();
			if (loopThroughAll)
			{
				loopNum++;
				if (loopNum > 12)
				{
					loopNum = 1;
				}
			}
			arger++;
		}
	}

	private IEnumerator RainDash()
	{
		yield return StartCoroutine(LightbombRain());
		yield return StartCoroutine(SlashDash());
	}

	private IEnumerator RainSweeper()
	{
		yield return StartCoroutine(LightbombRain());
		yield return StartCoroutine(Sweeper());
	}

	private IEnumerator SlashDash()
	{
		ignoreFacing = true;
		float waitTime = 0.4f;
		for (int i = 1; i <= stage; i++)
		{
			waitTime -= 0.03f;
		}
		yield return MoveToEdge(1, 2);
		CastSpellObj("SerifCircuitWarning");
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(waitTime + 0.2f);
		CastSpellObj("SerifCircuit");
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("release");
		yield return new WaitForSeconds(0.1f);
		dontHitAnim = true;
		mov.lerpTimeMods.Add(0.3f);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.Front, Shape.Column, Pattern.Moveable, 5), 1, this));
		SetDashTrue();
		yield return new WaitWhile(() => mov.state == State.Moving);
		anim.SetBool("dashing", false);
		yield return new WaitForSeconds(waitTime);
		mov.lerpTimeMods.Remove(0.3f);
		anim.SetTrigger("idle");
		mov.SetState(State.Idle);
		dontHitAnim = false;
		yield return new WaitForSeconds(0.4f);
		ignoreFacing = false;
	}

	private IEnumerator LightbombRain()
	{
		dontHitAnim = true;
		List<int> randList = Utils.RandomList(4);
		savedTileList = new List<Tile>();
		int numShots3 = 3;
		numShots3 += stage;
		numShots3 = Mathf.Clamp(numShots3, 2, 4);
		for (int x = 0; x < numShots3; x++)
		{
			savedTileList.Add(battleGrid.grid[Random.Range(1, 6), randList[x]]);
			anim.SetTrigger("throw");
			CastSpellObj("SerifLightbombToss");
			yield return new WaitForSeconds(0.3f);
			savedTileList.Clear();
		}
		yield return new WaitForSeconds(0.1f);
		dontHitAnim = false;
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator LightbombPath()
	{
		ignoreFacing = true;
		yield return MoveToEdge();
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.4f);
		anim.SetTrigger("release");
		yield return new WaitForSeconds(0.4f);
		CastSpellObj("SerifLightbombPath");
		yield return new WaitForSeconds(0.3f);
		mov.Move(0, 3);
		yield return new WaitWhile(() => mov.state == State.Moving);
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("release");
		CastSpellObj("SerifLightbombPath");
		yield return new WaitForSeconds(0.7f);
		anim.SetTrigger("throw");
		CastSpellObj("SerifWave");
		CastSpellObj("SerifCircuit");
		yield return new WaitForSeconds(0.3f);
		mov.MoveTo(mov.currentTile.x, 2);
		CastSpellObj("SerifCircuit");
		yield return new WaitForSeconds(2.4f);
		ignoreFacing = false;
	}

	private IEnumerator AlternatingWaves()
	{
		ignoreFacing = true;
		int numShots = 5;
		yield return MoveToEdge();
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.4f);
		CastSpellObj("SerifPush");
		anim.SetTrigger("release");
		List<int> randThree = Utils.RandomList(3);
		for (int x = 0; x < numShots; x++)
		{
			if (x % 3 == 0)
			{
				randThree.AddRange(Utils.RandomList(3));
			}
			savedTileList.Add(battleGrid.grid[mov.currentTile.x, randThree[x]]);
			mov.MoveToTile(savedTileList[x]);
			SetDashTrue();
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.1f);
			anim.SetBool("dashing", false);
			anim.SetTrigger("throw");
			CastSpellObj("SerifWave");
			yield return new WaitForSeconds(0.5f - (float)stage * 0.05f);
		}
		savedTileList.Clear();
		yield return new WaitForSeconds(0.4f);
		ignoreFacing = false;
	}

	private IEnumerator SwordRows()
	{
		dontHitAnim = true;
		Utils.RandomList(4);
		savedTileList = new List<Tile>();
		yield return MoveToEdge(0, 1);
		yield return new WaitForSeconds(0.4f);
		CastSpellObj("SerifPush");
		savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Front, Shape.ColumnWide, new List<Pattern>
		{
			Pattern.Random,
			Pattern.PrioritizeUnoccupied
		}, 4), 1, this));
		anim.SetTrigger("throw");
		CastSpellObj("SerifLightbombToss");
		yield return new WaitForSeconds(1.2f);
		savedTileList.Clear();
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.2f);
		anim.SetTrigger("release");
		SpellObject swordRows = GetSpellObj("SerifSwordRows");
		swordRows.numTiles = 4;
		for (int i = 0; i < 4; i++)
		{
			if (i > 0)
			{
				anim.SetTrigger("charge");
				anim.SetTrigger("release");
			}
			CastSpellObj("SerifSwordRows");
			swordRows.numTiles--;
			yield return new WaitForSeconds(0.8f - (float)i * 0.2f);
		}
		dontHitAnim = false;
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator SwordFlurry()
	{
		yield return MoveToEdge(0, 1);
		yield return new WaitForSeconds(0.35f);
		CastSpellObj("SerifPush");
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.3f);
		if (timesCastFlurry > 1)
		{
			CastSpellObj("SerifOffSwordsTop");
		}
		timesCastFlurry++;
		for (int i = 0; i < 4; i++)
		{
			savedTileList.Clear();
			savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Front, Shape.Column, Pattern.Random), 4, this));
			CastSpellObj("SerifSwordFlurry");
			yield return new WaitForSeconds(0.9f);
		}
		yield return new WaitForSeconds(0.6f);
		anim.SetTrigger("release");
	}

	private IEnumerator GlitterStorm()
	{
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		CastSpellObj("SerifGlitterStorm");
		yield return new WaitForSeconds(4.5f);
		anim.SetTrigger("release");
	}

	private IEnumerator SpellEjector()
	{
		yield return new WaitForSeconds(0.2f);
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("release");
		anim.SetTrigger("throw");
		yield return new WaitForSeconds(0.1f);
		CastSpellObj("SerifSpellEjector");
		yield return new WaitForSeconds(0.3f);
	}

	private IEnumerator Sweeper()
	{
		yield return MoveToEdge(0, 1);
		ResetAnimTriggers();
		yield return new WaitForSeconds(0.35f);
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.2f);
		CastSpellObj("SerifCircuit");
		int numWaves = 1;
		if (stage > 0)
		{
			numWaves = 2;
		}
		gunPoint.transform.localPosition = sweeperGunpointPos;
		for (int i = 0; i < numWaves; i++)
		{
			yield return new WaitForSeconds(0.1f);
			savedTileList = battleGrid.Get(new TileApp(Location.SweeperColumnFront, Shape.Default, Pattern.All, 0), 0, this);
			CastSpellObj("SerifSweeper");
			yield return new WaitForSeconds(0.1f);
			savedTileList = battleGrid.Get(new TileApp(Location.SweeperColumnFront, Shape.Default, Pattern.Reverse, 0), 0, this);
			CastSpellObj("SerifSweeper");
			yield return new WaitForSeconds(0.6f);
		}
		CastSpellObj("SerifCircuit");
		yield return new WaitForSeconds(3f);
		anim.SetTrigger("release");
		gunPoint.transform.localPosition = originalGunpointPos;
	}

	private IEnumerator ExcaliburSlash()
	{
		yield return new WaitForSeconds(0.3f);
		CastSpellObj("SerifExcalibur");
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.Current, Shape.Adjacent, Pattern.Moveable, 0), 0, ctrl.currentPlayer));
		SetDashTrue();
		yield return new WaitWhile(() => mov.state == State.Moving);
		anim.SetBool("dashing", false);
		yield return new WaitForSeconds(0.1f);
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.7f);
		anim.SetTrigger("release");
		CastSpellObj("SerifExcaliburSlash");
		yield return new WaitForSeconds(0.55f);
	}

	private IEnumerator OffSwords()
	{
		yield return MoveToEdge(0, 1);
		yield return new WaitForSeconds(0.35f);
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.2f);
		CastSpellObj("SerifOffSwordsTop");
		CastSpellObj("SerifOffSwordsBot");
		yield return new WaitForSeconds(3f);
		anim.SetTrigger("release");
		gunPoint.transform.localPosition = originalGunpointPos;
	}

	private void SetDashTrue()
	{
		PlayOnce(soundEffects[0]);
		anim.SetBool("dashing", true);
	}

	private IEnumerator MoveToEdge(int distanceFromEdge = 0, int yAxis = 0)
	{
		if (mov.currentTile.x >= battleGrid.gridLength / 2)
		{
			if (distanceFromEdge == 0)
			{
				base.transform.right = Vector3.zero - Vector3.left;
			}
			if (battleGrid.grid[7 - distanceFromEdge, yAxis].IsOccupiable())
			{
				mov.MoveTo(7 - distanceFromEdge, yAxis);
			}
			else
			{
				mov.MoveToTile(battleGrid.Get(new TileApp(Location.Index, Shape.Column, Pattern.Moveable, 8 - distanceFromEdge), 1, this));
			}
		}
		else
		{
			if (distanceFromEdge == 0)
			{
				base.transform.right = Vector3.zero - Vector3.right;
			}
			if (battleGrid.grid[distanceFromEdge, yAxis].IsOccupiable())
			{
				mov.MoveTo(distanceFromEdge, yAxis);
			}
			else
			{
				mov.MoveToTile(battleGrid.Get(new TileApp(Location.Index, Shape.Column, Pattern.Moveable, 1 + distanceFromEdge), 1, this));
			}
		}
		SetDashTrue();
		yield return new WaitWhile(() => mov.state == State.Moving);
		anim.SetBool("dashing", false);
		if (mov.currentTile.x >= battleGrid.gridLength / 2)
		{
			base.transform.right = Vector3.zero - Vector3.right;
		}
		else
		{
			base.transform.right = Vector3.zero - Vector3.left;
		}
	}

	public override IEnumerator DownC(bool destroyStructures = true, bool showZoneButtons = false)
	{
		ignoreFacing = false;
		if (!lastScene)
		{
			lastScene = true;
			ctrl.muCtrl.PauseIntroLoop();
			if ((bool)bossGate)
			{
				new List<Cpu> { this, bossGate };
			}
			else
			{
				ctrl.DestroyEnemiesAndStructures(this);
			}
			AddInvince(3.7f);
			dontMoveAnim = true;
			base.dontInterruptAnim = false;
			dontExecutePlayer = true;
			ctrl.camCtrl.Shake(3);
			health.current = 1;
			if ((bool)ctrl.currentPlayer)
			{
				ctrl.currentPlayer.downed = true;
			}
			RemoveAllStatuses();
			yield return new WaitForSeconds(0.1f);
			RemoveAllStatuses();
			mov.MoveToTile(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.Moveable, 14), 1, this));
			yield return new WaitForSeconds(0.6f);
			RemoveAllStatuses();
			yield return new WaitForSeconds(0.1f);
			health.ModifyHealth(3000);
			anim.SetTrigger("back");
			yield return StartCoroutine(_StartDialogue("Downed"));
			yield return new WaitForSeconds(0.6f);
			if (!runCtrl.currentRun.yamiObtained)
			{
				invinceFlash = false;
				AddInvince(99f);
				yield return StartCoroutine(_LoopRun(false));
				AddInvince(99f);
			}
			else
			{
				currentPlayer.duelDisk.currentCardtridges.Clear();
				currentPlayer.duelDisk.currentDeck.Clear();
				savedPlayerWepString = ctrl.currentPlayer.equippedWep.itemID;
				ctrl.currentPlayer.equippedWep = deCtrl.CreateSpellBase("Yami", ctrl.currentPlayer);
				for (int i = 0; i < 12; i++)
				{
					currentPlayer.duelDisk.AddLiveSpell(null, "Yami", ctrl.currentPlayer, true, false);
				}
				CastSpellObj("SerifDarkPierce");
				GetSpellObj("SerifDarkPierceEffect").being = currentPlayer;
				yield return new WaitForSeconds(0.1f);
				CastSpellObj("SerifDarkPierceEffect");
				currentPlayer.duelDisk.ManualShuffle();
				yield return new WaitForSeconds(0.6f);
				health.deathTriggered = false;
				col.enabled = true;
				SetInvince(0f);
				yield return new WaitForSeconds(0.6f);
				yield return StartCoroutine(_LoopRun(true));
			}
			yield return new WaitForSeconds(0.3f);
		}
		else
		{
			dontMoveAnim = true;
			dontHitAnim = true;
			base.dontInterruptAnim = true;
			CastSpellObj("SerifBurn");
			anim.SetTrigger("down");
			yield return new WaitForSeconds(0.6f);
			StartCoroutine(_ContinuousShake(3f));
			yield return StartCoroutine(_StartDialogue("Burning"));
			yield return new WaitForSeconds(0.1f);
			downed = true;
			dontMoveAnim = true;
			dontHitAnim = true;
			base.dontInterruptAnim = true;
		}
		health.deathTriggered = false;
		col.enabled = true;
	}

	private IEnumerator _LoopRun(bool yami)
	{
		yield return StartCoroutine(_StartDialogue("Reset"));
		yield return new WaitForSeconds(0.4f);
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.7f);
		CastSpellObj("SerifTimeReverse");
		anim.SetTrigger("release");
		yield return new WaitForSeconds(0.4f);
		ctrl.camCtrl.TransitionInHigh("WhiteExplosion");
		yield return new WaitForSeconds(1f);
		for (int j = 0; j < ctrl.currentPlayers.Count; j++)
		{
			ctrl.currentPlayers[j].ApplyStun(false);
			ctrl.currentPlayers[j].RemoveAllStatuses();
		}
		yield return new WaitForSeconds(1.5f);
		if (yami)
		{
			ctrl.currentPlayer.equippedWep = deCtrl.CreateSpellBase(savedPlayerWepString, ctrl.currentPlayer);
		}
		ctrl.DestroyEnemiesAndStructures(this);
		battleGrid.FixAllTiles();
		stopAllFacing = true;
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < ctrl.currentPlayers.Count; i++)
		{
			ctrl.currentPlayers[i].mov.MoveTo(i, i, false, true, true, true, false, true, true);
			ctrl.currentPlayers[i].transform.right = Vector3.zero - Vector3.left;
			foreach (Cpu pet in ctrl.currentPlayers[i].currentPets)
			{
				pet.transform.right = Vector3.zero - Vector3.left;
			}
		}
		runCtrl.LoopRun();
	}

	private IEnumerator _ContinuousShake(float duration)
	{
		duration += Time.time;
		while (Time.time < duration)
		{
			ctrl.camCtrl.Shake(2);
			yield return new WaitForSeconds(0.5f);
		}
	}

	public override IEnumerator<float> _DeathFinal()
	{
		inDeathSequence = true;
		sp.explosionGen.CreateExplosionString(3, base.transform.position, base.transform.rotation, this);
		S.I.PlayOnce(explosionSound);
		anim.SetTrigger("die");
		GetSpellObj("SerifDarkPierceEffect").being = currentPlayer;
		CastSpellObj("SerifDarkPierceEffect");
		yield return Timing.WaitForSeconds(0.9f);
		talkBubble.Fade();
		Remove();
		stopAllFacing = true;
		yield return Timing.WaitForSeconds(0.1f);
		for (int i = 0; i < ctrl.currentPlayers.Count; i++)
		{
			ctrl.currentPlayers[i].transform.right = Vector3.zero - Vector3.left;
		}
		inDeathSequence = false;
		spriteRend.enabled = false;
		ctrl.currentPlayer.equippedWep = deCtrl.CreateSpellBase(savedPlayerWepString, ctrl.currentPlayer);
		AchievementsCtrl.UnlockAchievement("Sans-Serif");
		ctrl.Victory(Ending.Genocide, "WhiteExplosion");
		base.gameObject.SetActive(false);
	}

	public override IEnumerator ExecutePlayerC()
	{
		ignoreFacing = false;
		yield return new WaitForSeconds(0.2f);
		ignoreFacing = false;
		ResetAnimTriggers();
		yield return new WaitForSeconds(0.4f);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.Index, Shape.Column, Pattern.Moveable, 4), 1, ctrl.currentPlayer));
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return new WaitForSeconds(0.4f);
		yield return StartCoroutine(_StartDialogue("Execution"));
		yield return new WaitForSeconds(0.9f);
		anim.SetTrigger("throw");
		CastSpellObj("SerifExecution");
		yield return new WaitForSeconds(5f);
		if ((bool)ctrl.currentPlayer)
		{
			StartCoroutine(StartLoopC());
		}
	}

	protected override void DownEffects()
	{
		base.DownEffects();
	}

	public void GateExecution(BossGate ogBossGate)
	{
		StopAllCoroutines();
		bossGate = ogBossGate;
		StartCoroutine(_NormalExecution());
	}

	private IEnumerator _NormalExecution()
	{
		ignoreFacing = false;
		ctrl.RemoveObstacle(this);
		ResetAnimTriggers();
		AddStatus(Status.SpellPower, 999f, 9999f);
		AddStatus(Status.Defense, 99f, 9999f);
		invinceFlash = false;
		health.SetHealth(99999, 99999);
		yield return new WaitForSeconds(0.75f);
		yield return StartCoroutine(_StartDialogue("NormalExecution"));
		yield return new WaitForSeconds(0.9f);
		if ((bool)ctrl.currentPlayer)
		{
			mov.MoveToTile(battleGrid.Get(new TileApp(Location.Player, Shape.Front), 0, ctrl.currentPlayer), true, false, true, false, true, false);
		}
		SetDashTrue();
		yield return new WaitWhile(() => mov.state == State.Moving);
		anim.SetBool("dashing", false);
		while ((bool)ctrl.currentPlayer && ctrl.currentPlayer.isActiveAndEnabled)
		{
			yield return new WaitForSeconds(0.2f);
			anim.SetTrigger("charge");
			yield return new WaitForSeconds(0.7f);
			CastSpellObj("SerifGateExecution");
			yield return new WaitForSeconds(0.2f);
			anim.SetTrigger("release");
			yield return new WaitForSeconds(1.35f);
			yield return new WaitForSeconds(1.5f);
			if ((bool)ctrl.currentPlayer && ctrl.currentPlayer.downed)
			{
				yield return new WaitForSeconds(0.6f);
				anim.SetTrigger("charge");
				yield return new WaitForSeconds(0.2f);
				CastSpellObj("SerifExecution");
				anim.SetTrigger("release");
				yield return new WaitForSeconds(6.2f);
			}
			if ((bool)ctrl.currentPlayer && !ctrl.currentPlayer.dead)
			{
				yield return StartCoroutine(_StartDialogue("ExecuteAgain"));
				yield return new WaitForSeconds(0.2f);
				while ((bool)ctrl.currentPlayer && ctrl.currentPlayer.isActiveAndEnabled)
				{
					anim.SetTrigger("charge");
					yield return new WaitForSeconds(0.2f);
					CastSpellObj("SerifExecution");
					anim.SetTrigger("release");
					yield return new WaitForSeconds(0.6f);
				}
				yield return new WaitForSeconds(6.2f);
			}
		}
		ignoreFacing = true;
		base.transform.right = Vector3.zero - Vector3.left;
		yield return StartCoroutine(_StartDialogue("NormalExecutionAfter"));
	}

	public override IEnumerator _Mercy()
	{
		yield return StartCoroutine(ExecutePlayerC());
	}
}
