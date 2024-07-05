using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossShielder : Boss
{
	public float[] orbitalLerpTime;

	public bool attacking = false;

	public bool summonIntoBigShield = false;

	public bool bigShield = false;

	public bool diagBeam = false;

	public bool diagStraight = false;

	public bool reflectOnly = false;

	public bool reflectArtillery = false;

	public bool shielderSmash = false;

	public bool scene;

	public override void Start()
	{
		base.Start();
		for (int i = 1; i <= tier; i++)
		{
			deCtrl.CreateArtifact("Eschaton", this);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (health.shield <= 0)
		{
			anim.SetBool("shield", false);
			col.size = Vector2.one * sp.baseHurtboxSize;
		}
	}

	public override void StartAction()
	{
		mov.PatrolRandom();
	}

	public override IEnumerator Loop()
	{
		yield return StartCoroutine(_StartDialogue("Intro"));
		int arger = 0;
		while (true)
		{
			LoopStart();
			if (scene)
			{
				tier = 2;
				yield return new WaitForSeconds(1.5f);
				yield return StartCoroutine(RevaSmash());
				mov.Move(-1, 0);
				yield return new WaitUntil(() => mov.state == State.Idle);
				yield return new WaitForSeconds(beingObj.movementDelay);
				yield return StartCoroutine(SummonBigShieldThrow());
				yield return StartCoroutine(DiagStraight());
			}
			else if (stage == 0)
			{
				if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
				{
					if (summonIntoBigShield)
					{
						yield return StartCoroutine(SummonBigShieldThrow());
					}
					else if (bigShield)
					{
						yield return StartCoroutine(BigShieldThrow());
					}
					else if (diagBeam)
					{
						yield return StartCoroutine(DiagBeam(true));
					}
					else if (diagStraight)
					{
						yield return StartCoroutine(DiagStraight(true));
					}
					else if (reflectArtillery)
					{
						yield return StartCoroutine(RevaArtillery());
					}
					else if (reflectOnly)
					{
						yield return StartCoroutine(Reflect());
					}
					else if (shielderSmash)
					{
						yield return StartCoroutine(RevaSmash(true));
					}
					yield return new WaitForSecondsRealtime(2f);
					continue;
				}
				if (Utils.RandomBool(4) && battleGrid.currentEnemies.Count <= 1)
				{
					yield return StartCoroutine(Summon());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(RevaSmash());
				}
				else
				{
					yield return StartCoroutine(BigShieldThrow());
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(4))
				{
					yield return StartCoroutine(RevaArtillery());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(DiagBeam());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(SummonBigShieldThrow());
				}
				else
				{
					yield return StartCoroutine(DiagStraight());
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(SummonBigShieldThrow());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(RevaSmash());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(DiagBeam(true));
				}
				else
				{
					yield return StartCoroutine(DiagStraight(true));
				}
			}
			yield return new WaitForSeconds(loopDelay);
			for (int i = 0; i < 2; i++)
			{
				mov.PatrolRandomEmpty();
				yield return new WaitUntil(() => mov.state == State.Idle);
				yield return new WaitForSeconds(beingObj.movementDelay);
			}
			StageCheck();
			arger++;
		}
	}

	private IEnumerator SummonBigShieldThrow()
	{
		if (battleGrid.currentEnemies.Count <= 1)
		{
			yield return StartCoroutine(Summon());
		}
		yield return StartCoroutine(BigShieldThrow());
	}

	private IEnumerator BigShieldThrow()
	{
		yield return StartCoroutine(BigShield());
		float baseDelay = 0.3f;
		for (int j = 1; j <= tier; j++)
		{
			baseDelay -= 0.02f;
		}
		yield return new WaitForSeconds(baseDelay + 0.4f);
		dontMoveAnim = true;
		for (int i = 0; i < 4; i++)
		{
			mov.PatrolRandomEmptyVertical();
			yield return new WaitUntil(() => mov.state == State.Idle);
			yield return new WaitForSeconds(baseDelay);
			if (health.shield <= 0)
			{
				dontMoveAnim = false;
				yield break;
			}
		}
		dontMoveAnim = false;
		if (health.shield > 0)
		{
			yield return StartCoroutine(BigRevaThrow());
		}
	}

	private IEnumerator BigShield()
	{
		yield return new WaitForSeconds(0.1f);
		spellObjList[7].efApps[0].amount = 200 + 50 * tier;
		spellObjList[7].StartCast();
		yield return null;
		if (health.shield > 0)
		{
			col.size = new Vector2(30f, 50f);
			anim.SetBool("shield", true);
		}
	}

	private IEnumerator BigRevaThrow()
	{
		spellObjList[0].shotVelocity += tier * 10;
		anim.SetTrigger("throw");
		yield return new WaitForSeconds(0.35f - 0.05f * (float)tier);
		spellObjList[0].StartCast();
		yield return new WaitForSeconds(0.9f - 0.1f * (float)tier);
	}

	private IEnumerator Summon()
	{
		SpellObject revaSum = GetSpellObj("RevaSummon");
		foreach (EffectApp efApp in revaSum.efApps)
		{
			if (efApp.effect == Effect.Summon)
			{
				if (tier == 1)
				{
					efApp.amount = 80f;
				}
				else if (tier == 2)
				{
					efApp.amount = 100f;
					efApp.value = "Slasher2";
				}
				else if (tier == 3)
				{
					efApp.amount = 140f;
					efApp.value = "Slasher2";
				}
				else if (tier > 3)
				{
					efApp.amount = 160f;
					efApp.value = "Slasher3";
				}
			}
		}
		if (mov.currentTile.x > 6)
		{
			mov.Move(1, 0);
		}
		while (mov.state == State.Moving)
		{
			yield return null;
		}
		for (int i = 0; i < 2; i++)
		{
			anim.SetTrigger("charge");
			spellObjList[6].StartCast();
			yield return new WaitForSeconds(0.5f);
			anim.SetTrigger("release");
			yield return new WaitForSeconds(1.5f);
			if (tier < 4)
			{
				break;
			}
		}
	}

	private IEnumerator DiagBeam(bool four = false)
	{
		float baseDelay = 0.3f;
		for (int j = 1; j <= tier; j++)
		{
			baseDelay -= 0.05f;
		}
		dontHitAnim = true;
		int yPos = Random.Range(1, 3);
		mov.MoveTo(4, yPos);
		while (mov.state == State.Moving)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.1f);
		anim.SetTrigger("charge");
		spellObjList[1].StartCast();
		spellObjList[2].StartCast();
		if (four)
		{
			spellObjList[8].StartCast();
			spellObjList[9].StartCast();
		}
		for (int i = 2; i <= tier; i++)
		{
			yield return new WaitForSeconds(0.3f);
			mov.MoveTo(mov.currentTile.x, ctrl.currentPlayer.mov.currentTile.y);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.1f);
			CastSpellObj("RevaBoomerang");
		}
		yield return new WaitForSeconds(0.5f);
		anim.SetTrigger("release");
		yield return new WaitForSeconds(0.5f);
		dontHitAnim = false;
	}

	private IEnumerator DiagStraight(bool triple = false)
	{
		if (tier > 1)
		{
			spellObjList[3].damage += 2f;
			if (triple)
			{
				spellObjList[4].damage += 2f;
				spellObjList[5].damage += 2f;
			}
		}
		for (int r = 0; r < 3; r++)
		{
			yield return new WaitForSeconds(0.3f);
			int yPos = Random.Range(0, 4);
			int xPos = Random.Range(4, 6);
			if (triple)
			{
				yPos = Random.Range(1, 3);
				xPos = 5;
			}
			mov.MoveTo(xPos, yPos);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.1f);
			anim.SetTrigger("charge");
			CastSpellObj("RevaBoomerang");
			if (triple && tier > 0)
			{
				CastSpellObj("RevaBeamUp");
				CastSpellObj("RevaBeamDown");
			}
			else
			{
				int upDownSpellNum = Random.Range(1, 2);
				spellObjList[upDownSpellNum].StartCast();
			}
			if (tier > 2)
			{
				spellObjList[8].StartCast();
				spellObjList[9].StartCast();
			}
			yield return new WaitForSeconds(0.5f);
			anim.SetTrigger("release");
			yield return new WaitForSeconds(GetDelay(0.5f, 0.1f));
		}
	}

	private IEnumerator Reflect()
	{
		anim.SetTrigger("charge");
		CastSpellObj("RevaReflectWarning");
		yield return new WaitForSeconds(0.5f);
		anim.SetTrigger("release");
		spellObjList[3].StartCast();
		yield return new WaitForSeconds(3.5f);
	}

	private IEnumerator RevaArtillery()
	{
		anim.SetTrigger("charge");
		CastSpellObj("RevaReflectWarning");
		yield return new WaitForSeconds(0.5f);
		anim.SetTrigger("release");
		spellObjList[3].StartCast();
		yield return new WaitForSeconds(0.6f);
		savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.RandAlliedUnique), 3, this));
		spellObjList[4].StartCast();
		yield return new WaitForSeconds(1.3f);
		foreach (Tile tile in savedTileList)
		{
			mov.MoveTo(tile.x, tile.y);
			yield return new WaitForSeconds(0.8f);
		}
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator RevaSmash(bool castDiag = false)
	{
		float baseDelay = 0.5f;
		for (int j = 1; j <= tier; j++)
		{
			baseDelay -= 0.04f;
		}
		if (tier >= 2)
		{
			castDiag = false;
			anim.SetTrigger("throw");
			spellObjList[10].StartCast();
			yield return new WaitForSeconds(0.6f);
		}
		int yPos = Random.Range(1, 3);
		mov.MoveTo(4, yPos);
		while (mov.state == State.Moving)
		{
			yield return null;
		}
		base.dontInterruptAnim = true;
		anim.SetTrigger("specialStart");
		yield return new WaitForSeconds(0.55f);
		ctrl.camCtrl.Shake(1);
		int waveNum = 0;
		int x2;
		for (x2 = 0; x2 <= tier; x2++)
		{
			x2++;
			for (int i = battleGrid.gridLength - mov.currentTile.x; i <= 7; i++)
			{
				savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.All, i), 0, this));
				spellObjList[5].StartCast();
				ctrl.camCtrl.Shake(0);
				yield return new WaitForSeconds(baseDelay);
				if (tier > 2)
				{
					switch (waveNum)
					{
					case 0:
						if (tier > 3)
						{
							CastSpellObj("RevaBeamUp");
						}
						break;
					case 1:
						CastSpellObj("RevaBoomerang");
						break;
					case 2:
						if (tier > 4)
						{
							CastSpellObj("RevaBeamDown");
						}
						break;
					}
				}
				waveNum++;
			}
		}
		yield return new WaitForSeconds(0.4f);
		anim.SetTrigger("specialEnd");
		base.dontInterruptAnim = false;
	}

	public override SpellObject ReflectShot()
	{
		return ctrl.deCtrl.CreateSpellBase("ReflectShotTall", this).Set(this);
	}

	protected override void DownEffects()
	{
		base.DownEffects();
	}

	public override IEnumerator ExecutePlayerC()
	{
		base.dontInterruptAnim = false;
		dontMoveAnim = false;
		yield return new WaitForSeconds(0.2f);
		ResetAnimTriggers();
		yield return new WaitForSeconds(0.4f);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.HalfField), 1, ctrl.currentPlayer));
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return StartCoroutine(_StartDialogue("Execution"));
		while ((bool)ctrl.currentPlayer && ctrl.currentPlayer.downed)
		{
			yield return new WaitForSeconds(0.3f);
			yield return StartCoroutine(RevaSmash());
			yield return new WaitForSeconds(0.6f);
		}
		if (ctrl.PlayersActive())
		{
			StartCoroutine(StartLoopC());
		}
	}
}
