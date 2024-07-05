using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class BossSaffron : Boss
{
	private enum MoveTo
	{
		None = 0,
		Random = 1,
		RowFront = 2,
		RowBack = 3,
		RowRandom = 4,
		FourAway = 5,
		CenterSquare = 6
	}

	public bool attacking = false;

	public bool antiDmg = false;

	public bool pullGun = false;

	public bool tier1 = false;

	public bool tier2 = false;

	public bool tier3 = false;

	public bool tier4 = false;

	public bool useTestIndex = false;

	private int spellsCastTier = 1;

	public int spellCastAmount = 5;

	public Dictionary<string, SpellObject> patternSpells;

	public UIFollow spellIconPrefab;

	public UIFollow spellIconFollow;

	public Image spellIcon;

	public Cardtridge cardtridgeRef;

	public Sprite emptySprite;

	private bool reviveTriggered = false;

	private bool doingReviveProcess = false;

	private int spellIndex = 0;

	private Queue<int> spellIndexQueue = new Queue<int>();

	public override void Start()
	{
		base.Start();
		List<int> list = Utils.RandomList(ctrl.itemMan.saffronBossSpellList.Count - 1);
		patternSpells = new Dictionary<string, SpellObject>();
		spellIconFollow = Object.Instantiate(spellIconPrefab, ctrl.battleUIContainer);
		spellIconFollow.following = base.transform;
		cardtridgeRef = spellIconFollow.transform.GetChild(0).GetComponent<Cardtridge>();
		cardtridgeRef.refCard = true;
		cardtridgeRef.canvas.sortingOrder = 2;
		cardtridgeRef.image.fillAmount = 1f;
		cardtridgeRef.imageOverlay.fillAmount = 0f;
		cardtridgeRef.imageOverback.fillAmount = 0f;
		SetCardtridgeSprite(emptySprite);
	}

	private void AddPatternSpell(string spellId)
	{
		patternSpells[spellId] = ctrl.deCtrl.CreateSpellBase(spellId, this);
		switch (spellId)
		{
		default:
			if (!(spellId == "Warpath"))
			{
				break;
			}
			goto case "BowSnipe";
		case "BowSnipe":
		case "Ragnarok":
		case "Cryokinesis":
		case "Blackout":
			patternSpells[spellId].damage = patternSpells[spellId].damage * 0.5f;
			break;
		}
		if (spellId == "Undertow" || spellId == "Tsunami")
		{
			patternSpells[spellId].animWarning = "WarningDangerC";
			patternSpells[spellId].warningDuration = 0.4f;
			patternSpells[spellId].castDelay = 0.4f;
			if (ctrl.optCtrl.settingsPane.angelModeEnabled == 1)
			{
				patternSpells[spellId].warningDuration = 0.55f;
				patternSpells[spellId].castDelay = 0.55f;
			}
		}
	}

	private void SetCardtridgeSprite(Sprite sprite)
	{
		cardtridgeRef.image.sprite = sprite;
		cardtridgeRef.anim.SetTrigger("RefLoad");
	}

	protected override void Update()
	{
		base.Update();
	}

	public override void StartAction()
	{
	}

	public override IEnumerator Loop()
	{
		yield return StartCoroutine(_StartDialogue("Intro"));
		int arger = 0;
		while (true)
		{
			LoopStart();
			if (stage == 0)
			{
				if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
				{
					if (antiDmg)
					{
						yield return StartCoroutine(_AntiDmg());
					}
					else if (pullGun)
					{
						yield return StartCoroutine(_PullGun());
					}
					else if (tier1)
					{
						yield return StartCoroutine(_Tier1());
					}
					else if (tier2)
					{
						yield return StartCoroutine(_Tier2());
					}
					else if (tier3)
					{
						yield return StartCoroutine(_Tier3());
					}
					else if (tier4)
					{
						yield return StartCoroutine(_Tier4());
					}
					yield return new WaitForSecondsRealtime(2f);
					continue;
				}
				yield return StartCoroutine(_TierCast());
			}
			else if (stage == 1)
			{
				yield return StartCoroutine(_TierCast());
			}
			else if (stage == 2)
			{
				yield return StartCoroutine(_TierCast());
			}
			yield return new WaitForSeconds(loopDelay);
			if (Utils.RandomBool(2))
			{
				SetCardtridgeSprite(GetSpellObj("Thunder").sprite);
				for (int i = 0; i < 3; i++)
				{
					mov.PatrolRandomEmpty();
					yield return new WaitUntil(() => mov.state == State.Idle);
					yield return new WaitForSeconds(beingObj.movementDelay);
					foreach (Player thePlayer in ctrl.currentPlayers)
					{
						if (ctrl.PlayersActive() && thePlayer.mov.currentTile.x == mov.currentTile.x - 4 && thePlayer.mov.currentTile.y == mov.currentTile.y)
						{
							yield return StartCoroutine(_Thunder());
							yield return new WaitForSeconds(0.3f);
							break;
						}
					}
				}
				if (Utils.RandomBool(2))
				{
					for (int j = 0; j < 3; j++)
					{
						mov.Move(1, 0);
						yield return new WaitUntil(() => mov.state == State.Idle);
						foreach (Player thePlayer3 in ctrl.currentPlayers)
						{
							if (ctrl.PlayersActive() && thePlayer3.mov.currentTile.x == mov.currentTile.x - 4 && thePlayer3.mov.currentTile.y == mov.currentTile.y)
							{
								yield return StartCoroutine(_Thunder());
								break;
							}
						}
						mov.Move(-1, 0);
						yield return new WaitUntil(() => mov.state == State.Idle);
						foreach (Player thePlayer2 in ctrl.currentPlayers)
						{
							if (ctrl.PlayersActive() && thePlayer2.mov.currentTile.x == mov.currentTile.x - 4 && thePlayer2.mov.currentTile.y == mov.currentTile.y)
							{
								yield return StartCoroutine(_Thunder());
								break;
							}
						}
					}
				}
			}
			if (tier > 3 && Utils.RandomBool(2))
			{
				SetCardtridgeSprite(GetSpellObj("Focus").sprite);
				yield return new WaitForSeconds(0.2f);
				CastSpellObj("Focus");
				SetCardtridgeSprite(emptySprite);
			}
			SetCardtridgeSprite(emptySprite);
			if (ctrl.currentPlayer.spellsCastThisBattle.Count > spellsCastTier * spellCastAmount)
			{
				spellsCastTier++;
				yield return StartCoroutine(_AntiDmg());
			}
			StageCheck();
			arger++;
		}
	}

	private void SetTestIndex(int cases)
	{
		if (useTestIndex)
		{
			spellIndex++;
			if (spellIndex >= cases)
			{
				spellIndex = 0;
			}
		}
		else
		{
			spellIndex = Random.Range(0, cases);
			while (spellIndexQueue.Contains(spellIndex))
			{
				spellIndex = Random.Range(0, cases);
			}
		}
		spellIndexQueue.Enqueue(spellIndex);
		if (spellIndexQueue.Count > 2)
		{
			spellIndexQueue.Dequeue();
		}
	}

	private IEnumerator _TierCast()
	{
		if (tier == 0)
		{
			yield return StartCoroutine(_Tier1());
		}
		else if (tier == 1)
		{
			yield return StartCoroutine(_Tier2());
		}
		else if (tier == 2)
		{
			yield return StartCoroutine(_Tier3());
		}
		else if (tier == 3)
		{
			yield return StartCoroutine(_Tier4());
		}
		else if (tier >= 4)
		{
			for (int i = 4; i <= tier; i++)
			{
				yield return StartCoroutine(_Tier4());
				yield return new WaitForSeconds(0.5f);
			}
		}
	}

	private IEnumerator _Tier1()
	{
		SetTestIndex(8);
		switch (spellIndex)
		{
		case 0:
			yield return StartCoroutine(SpellPattern(new List<string> { "KineticWave", "Thunder" }, MoveTo.RowFront));
			break;
		case 1:
			yield return StartCoroutine(SpellPattern(new List<string> { "Cryokinesis", "Frostbolt" }, MoveTo.RowBack));
			break;
		case 2:
			yield return StartCoroutine(SpellPattern(new List<string> { "StepSlash" }, MoveTo.FourAway));
			break;
		case 3:
			yield return StartCoroutine(SpellPattern(new List<string> { "Sweeper" }, MoveTo.None));
			break;
		case 4:
			yield return StartCoroutine(SpellPattern(new List<string> { "PekayFire", "Firewall" }, MoveTo.RowBack));
			break;
		case 5:
			yield return StartCoroutine(SpellPattern(new List<string> { "BlackoutSaffron" }, MoveTo.None));
			break;
		case 6:
			yield return StartCoroutine(SpellPattern(new List<string> { "KineticWave", "OrbitalBeam" }, MoveTo.RowFront));
			break;
		case 7:
			yield return StartCoroutine(SpellPattern(new List<string> { "Glitterbomb", "CounterStrike" }, MoveTo.CenterSquare));
			break;
		}
	}

	private IEnumerator _Tier2()
	{
		SetTestIndex(9);
		switch (spellIndex)
		{
		case 0:
			yield return StartCoroutine(SpellPattern(new List<string> { "Align", "Thunder" }, MoveTo.RowFront));
			break;
		case 1:
			yield return StartCoroutine(SpellPattern(new List<string> { "Cryokinesis", "FrostBarrage" }, MoveTo.RowBack));
			break;
		case 2:
			yield return StartCoroutine(SpellPattern(new List<string> { "StepSlash" }, MoveTo.FourAway));
			break;
		case 3:
			yield return StartCoroutine(SpellPattern(new List<string> { "BombToss", "Sweeper" }, MoveTo.None));
			break;
		case 4:
			yield return StartCoroutine(SpellPattern(new List<string> { "Pull", "Flurry" }, MoveTo.RowFront));
			break;
		case 5:
			yield return StartCoroutine(SpellPattern(new List<string> { "PekayFire", "Firewall" }, MoveTo.RowBack));
			break;
		case 6:
			yield return StartCoroutine(SpellPattern(new List<string> { "IonCannon", "Glitterbomb" }, MoveTo.CenterSquare));
			break;
		case 7:
			yield return StartCoroutine(SpellPattern(new List<string> { "Boomerang", "PekayFire" }, MoveTo.RowRandom));
			break;
		case 8:
			yield return StartCoroutine(SpellPattern(new List<string> { "BlackoutSaffron" }, MoveTo.None));
			break;
		}
	}

	private IEnumerator _Tier3()
	{
		SetTestIndex(9);
		switch (spellIndex)
		{
		case 0:
			yield return StartCoroutine(SpellPattern(new List<string> { "Align", "MagicClaw" }, MoveTo.RowFront));
			break;
		case 1:
			yield return StartCoroutine(SpellPattern(new List<string> { "Cryokinesis", "FrostBarrage" }, MoveTo.RowBack));
			break;
		case 2:
			yield return StartCoroutine(SpellPattern(new List<string> { "Circuit", "StepSlash" }, MoveTo.FourAway));
			break;
		case 3:
			yield return StartCoroutine(SpellPattern(new List<string> { "ShardToss", "Leech" }, MoveTo.None));
			break;
		case 4:
			yield return StartCoroutine(SpellPattern(new List<string> { "Pull", "Shotgun", "KineticWave" }, MoveTo.RowFront));
			break;
		case 5:
			yield return StartCoroutine(SpellPattern(new List<string> { "Fracture", "Fissure", "MissMeShield" }, MoveTo.RowFront));
			break;
		case 6:
			yield return StartCoroutine(SpellPattern(new List<string> { "SwordLineSaffron", "LaserTriple" }, MoveTo.RowFront));
			break;
		case 7:
			yield return StartCoroutine(SpellPattern(new List<string> { "BombToss", "SumTurretlaser" }, MoveTo.RowRandom));
			break;
		case 8:
			yield return StartCoroutine(SpellPattern(new List<string> { "PekayFire", "FireWall" }, MoveTo.FourAway));
			break;
		}
	}

	private IEnumerator _Tier4()
	{
		SetTestIndex(12);
		switch (spellIndex)
		{
		case 0:
			yield return StartCoroutine(SpellPattern(new List<string> { "Align", "Ragnarok", "MagicClaw" }, MoveTo.RowFront));
			break;
		case 1:
			yield return StartCoroutine(SpellPattern(new List<string> { "IceNeedle", "FrostBarrage", "Hockey" }, MoveTo.RowBack, 0.3f));
			break;
		case 2:
			yield return StartCoroutine(SpellPattern(new List<string> { "Align", "Warpath" }, MoveTo.FourAway));
			break;
		case 3:
			yield return StartCoroutine(SpellPattern(new List<string> { "BombToss", "Flamberge" }, MoveTo.FourAway, 0.3f));
			break;
		case 4:
			yield return StartCoroutine(SpellPattern(new List<string> { "Pull", "Shotgun", "Tsunami" }, MoveTo.RowFront));
			break;
		case 5:
			yield return StartCoroutine(SpellPattern(new List<string> { "Blink", "Flurry", "Undertow" }, MoveTo.RowFront));
			break;
		case 6:
			yield return StartCoroutine(SpellPattern(new List<string> { "SwordLineSaffron", "LaserTriple" }, MoveTo.RowFront));
			break;
		case 7:
			yield return StartCoroutine(SpellPattern(new List<string> { "Hellfire", "BlessingOfSusano" }, MoveTo.RowRandom, 0.4f));
			break;
		case 8:
			yield return StartCoroutine(SpellPattern(new List<string> { "SwordLineSaffron", "BowSnipe" }, MoveTo.FourAway));
			break;
		case 9:
			yield return StartCoroutine(SpellPattern(new List<string> { "PekayFire", "FireWall", "Wildfire" }, MoveTo.FourAway, 0.4f));
			break;
		case 10:
			yield return StartCoroutine(SpellPattern(new List<string> { "PowerSaws", "Leech" }, MoveTo.CenterSquare));
			break;
		case 11:
			yield return StartCoroutine(SpellPattern(new List<string> { "CrossturretToss", "FateShield" }, MoveTo.CenterSquare));
			break;
		}
	}

	private IEnumerator SpellPattern(List<string> spells, MoveTo moveTo, float endingDelay = 0f)
	{
		float baseDelay = 1.4f;
		for (int j = 1; j <= tier; j++)
		{
			baseDelay -= 0.03f;
			if (ctrl.optCtrl.settingsPane.angelModeEnabled == 1)
			{
				baseDelay += 0.33f * ctrl.optCtrl.settingsPane.angelModeCurrentSpeedReduction;
			}
		}
		for (int k = 0; k < spells.Count; k++)
		{
			if (!patternSpells.ContainsKey(spells[k]))
			{
				AddPatternSpell(spells[k]);
			}
		}
		SetCardtridgeSprite(patternSpells[spells[0]].sprite);
		yield return new WaitForSeconds(baseDelay * 0.6f);
		List<Tile> theTileList = null;
		switch (moveTo)
		{
		case MoveTo.Random:
			theTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Player, Shape.Row, new List<Pattern>
			{
				Pattern.Reverse,
				Pattern.Moveable
			}), 0, this));
			break;
		case MoveTo.RowFront:
			theTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Player, Shape.Row, new List<Pattern>
			{
				Pattern.Reverse,
				Pattern.Moveable
			}), 0, this));
			break;
		case MoveTo.RowBack:
			theTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Player, Shape.Row, new List<Pattern> { Pattern.Moveable }), 0, this));
			break;
		case MoveTo.FourAway:
			theTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Player, Shape.Behind, new List<Pattern> { Pattern.Moveable }, 4), 0, this));
			if (theTileList.Count < 1)
			{
				theTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Player, Shape.Behind, new List<Pattern> { Pattern.Moveable }, 5), 0, this));
			}
			break;
		case MoveTo.CenterSquare:
			theTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Base, Shape.Square, new List<Pattern>
			{
				Pattern.Random,
				Pattern.Moveable
			}, 3), 0, this));
			break;
		}
		if (theTileList != null && theTileList != null && theTileList.Count > 0)
		{
			mov.MoveToTile(theTileList[0]);
		}
		for (int i = 0; i < spells.Count; i++)
		{
			if (i == 0)
			{
				yield return new WaitForSeconds(baseDelay * 0.35f);
				if (spells[0] == "StepSlash")
				{
					yield return new WaitForSeconds(baseDelay * 0.3f);
				}
			}
			else
			{
				SetCardtridgeSprite(patternSpells[spells[i]].sprite);
				yield return new WaitForSeconds(baseDelay * 0.35f + (float)i * baseDelay * 0.15f);
				if (tier == 3)
				{
					yield return new WaitForSeconds(baseDelay * 0.2f);
				}
			}
			anim.SetTrigger("spellCast");
			patternSpells[spells[i]].StartCast();
			SetCardtridgeSprite(emptySprite);
		}
		if (tier < 2)
		{
			yield return new WaitForSeconds(baseDelay * 0.4f);
		}
		else
		{
			yield return new WaitForSeconds(baseDelay * 0.2f);
		}
		float moveCooldown = 0f;
		if (tier < 3)
		{
			yield break;
		}
		while (endingDelay > 0f)
		{
			endingDelay -= Time.deltaTime;
			moveCooldown -= Time.deltaTime;
			if (moveCooldown < 0f)
			{
				moveCooldown = 0.4f;
				mov.PatrolRandomEmpty();
			}
		}
	}

	private IEnumerator _AntiDmg()
	{
		base.dontInterruptAnim = true;
		anim.SetTrigger("specialStart");
		mov.Move(1, 0);
		yield return new WaitForSeconds(0.2f);
		CastSpellObj("SaffronSummonPlushie");
		yield return new WaitForSeconds(0.4f);
		anim.SetTrigger("specialEnd");
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator _PullGun()
	{
		float baseDelay = 1.1f;
		for (int i = 1; i <= tier; i++)
		{
			baseDelay -= 0.1f;
		}
		yield return new WaitForSeconds(baseDelay);
		SetCardtridgeSprite(ctrl.itemMan.GetSprite("Pull"));
		base.dontInterruptAnim = true;
		mov.MoveTo(battleGrid.gridLength / 2, ctrl.currentPlayer.mov.currentTile.y);
		anim.SetBool("dashing", true);
		while (mov.state == State.Moving)
		{
			yield return null;
		}
		anim.SetBool("dashing", false);
		yield return new WaitForSeconds(baseDelay / 4f);
		anim.SetTrigger("charge");
		CastSpellObj("SaffronPull");
		SetCardtridgeSprite(ctrl.itemMan.GetSprite("Shotgun"));
		yield return new WaitForSeconds(baseDelay / 2f);
		CastSpellObj("SaffronShotgun");
		SetCardtridgeSprite(emptySprite);
		anim.SetTrigger("release");
		yield return new WaitForSeconds(baseDelay);
		base.dontInterruptAnim = false;
	}

	private IEnumerator _Thunder()
	{
		anim.SetTrigger("specialStart");
		yield return new WaitForSeconds(0.4f - 0.02f * (float)tier);
		CastSpellObj("Thunder");
		anim.SetTrigger("specialEnd");
	}

	private IEnumerator DeathRevive()
	{
		doingReviveProcess = true;
		PauseSpells();
		mov.SetState(State.Idle);
		ResetAnimTriggers();
		yield return null;
		yield return null;
		anim.SetTrigger("die");
		BC.GTimeScale = 0.2f;
		yield return new WaitForSeconds(0.15f);
		BC.GTimeScale = 1f;
		reviveTriggered = true;
		col.enabled = false;
		health.ModifyHealth(Mathf.Clamp(100 * tier, 200, 600));
		if (health.current > 0)
		{
			anim.SetTrigger("revive");
			CreateHitFX(Status.Resurrect);
			PlayOnce("misc_shimmer");
			SetInvince(1f);
			health.deathTriggered = false;
			col.enabled = true;
			doingReviveProcess = false;
			yield return new WaitForSeconds(0.3f);
			PlayOnce("light_spell");
			mov.SetState(State.Idle);
			yield return new WaitForSeconds(0.9f);
		}
	}

	public override void StartDeath(bool triggerDeathrattles = true)
	{
		if (!downed && !reviveTriggered)
		{
			StartCoroutine(DeathRevive());
		}
		else
		{
			base.StartDeath(true);
		}
	}

	private void EmergencyRevive()
	{
		BC.GTimeScale = 1f;
		health.ModifyHealth(Mathf.Clamp(100 * tier, 200, 600));
		ResetAnimTriggers();
		CreateHitFX(Status.Resurrect);
		PlayOnce("misc_shimmer");
		SetInvince(1f);
		reviveTriggered = true;
		doingReviveProcess = false;
		health.deathTriggered = false;
		col.enabled = true;
		mov.SetState(State.Idle);
	}

	public override IEnumerator ExecutePlayerC()
	{
		if (doingReviveProcess)
		{
			EmergencyRevive();
		}
		yield return new WaitForSeconds(0.2f);
		ResetAnimTriggers();
		yield return new WaitForSeconds(0.4f);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.HalfField), 1, ctrl.currentPlayer));
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return new WaitForSeconds(0.4f);
		yield return StartCoroutine(_StartDialogue("Execution"));
		anim.SetTrigger("specialStart");
		SetCardtridgeSprite(ctrl.itemMan.GetSprite("Ragnarok"));
		yield return new WaitForSeconds(0.4f);
		anim.SetTrigger("specialEnd");
		anim.SetTrigger("charge");
		CastSpellObj("SaffronAutoRag");
		SetCardtridgeSprite(emptySprite);
		yield return new WaitForSeconds(0.2f);
		if (Random.Range(0, 3) == 0)
		{
			anim.SetTrigger("release");
		}
		else
		{
			anim.SetTrigger("taunt");
		}
		yield return new WaitForSeconds(0.8f);
		if (ctrl.PlayersActive())
		{
			StartCoroutine(StartLoopC());
		}
	}

	public override IEnumerator _Mercy()
	{
		if (doingReviveProcess)
		{
			EmergencyRevive();
		}
		yield return StartCoroutine(_003C_003En__0());
	}

	public override void Clean()
	{
		base.Clean();
		if ((bool)spellIconFollow)
		{
			Object.Destroy(spellIconFollow.gameObject);
		}
	}

	public override void Clear(bool overrideDeathSequence = false)
	{
		if ((bool)spellIconFollow)
		{
			Object.Destroy(spellIconFollow.gameObject);
		}
		base.Clear(overrideDeathSequence);
	}

	protected override void DownEffects()
	{
		if ((bool)spellIconFollow)
		{
			Object.Destroy(spellIconFollow.gameObject);
		}
		base.DownEffects();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__0()
	{
		return base._Mercy();
	}
}
