using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BossGunner : Boss
{
	public bool attacking = false;

	public bool rush = false;

	public bool quadShot = false;

	public bool uTurn = false;

	public bool minefield = false;

	public bool valleyShot = false;

	public bool skyStrike = false;

	public bool timeShot = false;

	private Vector2 originalColSize;

	public override void Start()
	{
		base.Start();
		originalColSize = col.size;
	}

	protected override void Update()
	{
		base.Update();
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
			if (stage == 0)
			{
				if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE && !normalTestPattern)
				{
					if (rush)
					{
						yield return StartCoroutine(Rush());
					}
					else if (quadShot)
					{
						yield return StartCoroutine(QuadShot());
					}
					else if (minefield)
					{
						yield return StartCoroutine(Minefield());
					}
					else if (uTurn)
					{
						yield return StartCoroutine(UTurn());
					}
					else if (valleyShot)
					{
						yield return StartCoroutine(ValleyShot());
					}
					else if (skyStrike)
					{
						yield return StartCoroutine(SkyStrike());
					}
					else if (timeShot)
					{
						yield return StartCoroutine(_TimeShot());
					}
					yield return new WaitForSecondsRealtime(2f);
					continue;
				}
				if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(Rush());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(QuadShot());
				}
				else
				{
					yield return StartCoroutine(Minefield());
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(QuadShot());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(UTurn());
				}
				else
				{
					yield return StartCoroutine(Minefield());
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(ValleyShot());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(SkyStrike());
				}
				else
				{
					yield return StartCoroutine(_TimeShot());
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
			arger++;
		}
	}

	private IEnumerator Rush()
	{
		dontHitAnim = true;
		float baseDelay = 0.4f;
		for (int i = 1; i <= tier; i++)
		{
			baseDelay -= 0.022f;
		}
		audioSource.PlayOneShot(soundEffects[0]);
		mov.MoveTo(7, ctrl.currentPlayer.mov.currentTile.y);
		yield return new WaitForSeconds(0.2f);
		anim.SetTrigger("specialStart");
		yield return new WaitForSeconds(baseDelay * 2f + 0.1f);
		anim.SetTrigger("specialEnd");
		yield return new WaitForSeconds(0.1f);
		if (!GetStatusEffect(Status.Root))
		{
			mov.MoveTo(0, mov.currentTile.y, true, false);
			anim.SetBool("dashing", true);
			spellObjList[0].StartCast();
			mov.lerpTimeMods.Add(-0.08f);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			spellObjList[1].StartCast();
			anim.SetBool("dashing", true);
			yield return new WaitForSeconds(baseDelay);
			mov.lerpTimeMods.Remove(-0.08f);
			anim.SetBool("dashing", false);
			yield return new WaitForSeconds(baseDelay);
			anim.SetTrigger("charge");
			yield return new WaitForSeconds(baseDelay + 0.1f);
			anim.SetTrigger("release");
			yield return new WaitForSeconds(0.1f);
			mov.MoveToCurrentTile();
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			mov.SetState(State.Attacking);
			yield return new WaitForSeconds(0.05f);
			anim.SetTrigger("idle");
		}
		mov.SetState(State.Idle);
		dontHitAnim = false;
	}

	private IEnumerator _TimeShot()
	{
		dontHitAnim = true;
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.2f);
		anim.SetTrigger("release");
		CastSpellObj("GunnerPush");
		yield return new WaitForSeconds(0.1f);
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.2f);
		anim.SetTrigger("release");
		CastSpellObj("GunnerFlak");
		yield return new WaitForSeconds(0.12f);
		CastSpellObj("GunnerFlak");
		yield return new WaitForSeconds(0.23f);
		CastSpellObj("GunnerBulletTime");
		yield return new WaitForSeconds(2f);
		dontHitAnim = false;
	}

	private IEnumerator SkyStrike()
	{
		yield return StartCoroutine(ResetAnimTriggersC());
		base.dontInterruptAnim = true;
		anim.SetBool("airborne", true);
		col.size = Vector2.zero;
		gunPoint.transform.localPosition += Vector3.up * 160f;
		yield return new WaitForSeconds(1f);
		int j;
		for (j = 0; j <= tier; j++)
		{
			j++;
			spellObjList[6].StartCast();
			yield return new WaitForSeconds(2.1f);
		}
		gunPoint.transform.localPosition = originalGunpointPos;
		col.size = originalColSize;
		anim.SetBool("airborne", false);
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(1f);
	}

	private IEnumerator ValleyShot()
	{
		dontHitAnim = true;
		int y = 0;
		int adjuster = 1;
		float startDelay = 0.5f;
		float delayBeforeShot = 0.2f;
		float delayAfterShot = 0.35f;
		for (int j = 1; j <= tier; j++)
		{
			UnityEngine.Debug.Log(j);
			startDelay -= 0.04f;
			delayBeforeShot -= 0.025f;
			delayAfterShot -= 0.033f;
		}
		if (Utils.RandomBool(2))
		{
			y = 3;
		}
		for (int i = 0; i < 7; i++)
		{
			if (i == 0)
			{
				mov.MoveTo(4, y);
			}
			else
			{
				mov.MoveTo(mov.currentTile.x, y);
			}
			anim.SetTrigger("back");
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			anim.ResetTrigger("release");
			anim.SetTrigger("charge");
			if (i == 0)
			{
				yield return new WaitForSeconds(startDelay);
			}
			yield return new WaitForSeconds(delayBeforeShot);
			anim.SetTrigger("release");
			spellObjList[2].StartCast();
			yield return new WaitForSeconds(0.05f);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			yield return new WaitForSeconds(delayAfterShot);
			switch (y)
			{
			case 3:
				adjuster = -1;
				break;
			case 0:
				adjuster = 1;
				break;
			}
			y += adjuster;
		}
		dontHitAnim = false;
	}

	private IEnumerator QuadShot()
	{
		dontHitAnim = true;
		List<int> randList = Utils.RandomList(4);
		savedTileList = new List<Tile>();
		float timeToTravelMod = 1f;
		int bendingMod = 70;
		float delayBeforeShot = 0.2f;
		float delayAfterShot = 0.35f;
		int numShots2 = 2;
		for (int j = 1; j <= Mathf.Clamp(tier, 0, 2); j++)
		{
			timeToTravelMod -= 0.1f;
			delayBeforeShot -= 0.012f;
			delayAfterShot -= 0.022f;
			numShots2++;
			numShots2 = Mathf.Clamp(numShots2, 2, 4);
		}
		for (int x = 0; x < numShots2; x++)
		{
			spellObjList[3].timeToTravel = timeToTravelMod;
			spellObjList[3].bending = bendingMod;
			savedTileList.Add(battleGrid.grid[Random.Range(0, 4), randList[x]]);
			anim.SetTrigger("throw");
			spellObjList[3].StartCast();
			yield return new WaitForSeconds(0.2f);
			savedTileList.Clear();
			timeToTravelMod += 0.5f;
			bendingMod += 20;
		}
		yield return new WaitForSeconds((float)(4 - numShots2) * 0.2f);
		yield return new WaitForSeconds(0.1f);
		for (int i = 0; i < numShots2; i++)
		{
			if (i == 0)
			{
				mov.MoveTo(4, randList[i]);
			}
			else
			{
				mov.MoveTo(mov.currentTile.x, randList[i]);
			}
			anim.SetBool("dashing", true);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			anim.SetBool("dashing", false);
			yield return new WaitForSeconds(delayBeforeShot);
			anim.SetTrigger("charge");
			spellObjList[2].StartCast();
			anim.SetTrigger("release");
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			yield return new WaitForSeconds(delayAfterShot);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
		}
		yield return new WaitForSeconds(0.1f);
		anim.SetTrigger("idle");
		mov.SetState(State.Idle);
		dontHitAnim = false;
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator Minefield()
	{
		yield return new WaitForSeconds(0.35f);
		anim.SetTrigger("throw");
		SpellObject minefield = GetSpellObj("GunnerMinefield");
		if (tier < 1)
		{
			minefield.numTiles = 12;
		}
		spellObjList[4].StartCast();
		yield return new WaitForSeconds(0.9f);
	}

	private IEnumerator UTurn()
	{
		float waitTime = 0.4f;
		for (int i = 1; i <= tier; i++)
		{
			waitTime -= 0.02f;
		}
		for (int x = 0; x < 1; x++)
		{
			yield return new WaitForSeconds(waitTime + 0.1f);
			if ((bool)GetStatusEffect(Status.Root) || !ti.mainBattleGrid.grid[7, 3].IsOccupiable())
			{
				break;
			}
			mov.MoveTo(7, 3);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			spellObjList[5].StartCast();
			yield return new WaitForSeconds(waitTime + 0.35f);
			dontHitAnim = true;
			dontMoveAnim = true;
			if ((bool)GetStatusEffect(Status.Root))
			{
				break;
			}
			mov.lerpTimeMods.Add(0.2f);
			audioSource.PlayOneShot(soundEffects[1]);
			mov.MoveTo(0, 3, true, false);
			anim.SetBool("dashing", true);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			anim.SetBool("dashing", false);
			dontMoveAnim = false;
			yield return new WaitForSeconds(waitTime);
			if ((bool)GetStatusEffect(Status.Root))
			{
				break;
			}
			mov.lerpTimeMods.Add(-0.1f);
			audioSource.PlayOneShot(soundEffects[1]);
			mov.MoveTo(0, 0, true, false);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			yield return new WaitForSeconds(waitTime);
			if (mov.lerpTimeMods.Contains(-0.1f))
			{
				mov.lerpTimeMods.Remove(-0.1f);
			}
		}
		audioSource.PlayOneShot(soundEffects[1]);
		battleGrid.grid[7, 0].Fix();
		mov.MoveTo(7, 0);
		while (mov.state == State.Moving)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.1f);
		mov.lerpTimeMods.Clear();
		anim.SetTrigger("idle");
		mov.SetState(State.Idle);
		dontHitAnim = false;
		yield return new WaitForSeconds(0.9f);
	}

	protected override void DownEffects()
	{
		gunPoint.transform.localPosition = originalGunpointPos;
		col.size = originalColSize;
		anim.SetBool("airborne", false);
		base.DownEffects();
	}

	public override IEnumerator ExecutePlayerC()
	{
		yield return new WaitForSeconds(0.6f);
		gunPoint.transform.localPosition = originalGunpointPos;
		col.size = originalColSize;
		ResetAnimTriggers();
		while ((bool)ctrl.currentPlayer && ctrl.currentPlayer.downed)
		{
			mov.MoveToTile(battleGrid.Get(new TileApp(Location.Index, Shape.ColumnTwo, new List<Pattern>
			{
				Pattern.PrioritizeMoveable,
				Pattern.Random
			}, 8), 0, this));
			yield return new WaitWhile(() => mov.state == State.Moving);
			yield return new WaitForSeconds(0.2f);
			gunPoint.transform.localPosition = originalGunpointPos;
			col.size = originalColSize;
			yield return legacySpellList[0].StartCoroutine(legacySpellList[0].Cast());
			yield return new WaitForSeconds(0.3f);
		}
		if (ctrl.PlayersActive())
		{
			StartCoroutine(StartLoopC());
		}
	}

	public override IEnumerator Executed()
	{
		if (lastSpellHit != null && lastSpellHit.brand == Brand.Slashfik)
		{
			AchievementsCtrl.UnlockAchievement("Knife_to_a_Gunfight");
			S.AddSkinUnlock("SelicyCatgirl");
		}
		yield return StartCoroutine(_003C_003En__0());
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__0()
	{
		return base.Executed();
	}
}
