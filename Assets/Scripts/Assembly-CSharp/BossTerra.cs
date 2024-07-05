using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTerra : Boss
{
	public float[] orbitalLerpTime;

	public bool randomBreakOrbital;

	public bool randomBreakOrbitalAlt;

	public bool crossBlast;

	public bool followBreak;

	public bool followBreakOrbital;

	public bool revolverRain;

	public bool diamonds;

	public TileApp breakLocation;

	public override void StartAction()
	{
		mov.PatrolRandom();
	}

	public override IEnumerator Loop()
	{
		yield return StartCoroutine(_StartDialogue("Intro"));
		while (true)
		{
			if (stage == 0)
			{
				if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
				{
					if (randomBreakOrbital)
					{
						yield return StartCoroutine(RandomBreakOrbital(breakLocation));
					}
					else if (randomBreakOrbitalAlt)
					{
						yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.BotLeftTopRightTwo)));
					}
					else if (crossBlast)
					{
						yield return StartCoroutine(CrossBlast());
					}
					else if (followBreak)
					{
						yield return StartCoroutine(FollowBreak());
					}
					else if (followBreakOrbital)
					{
						yield return StartCoroutine(FollowBreakOrbital());
					}
					else if (revolverRain)
					{
						yield return StartCoroutine(RevolverRain());
					}
					else if (diamonds)
					{
						yield return StartCoroutine(Diamonds(4));
					}
					yield return new WaitForSecondsRealtime(3f);
					continue;
				}
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(CrossBlast());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.Base, Shape.Column, Pattern.All, 5)));
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.Base, Shape.Horizontal, Pattern.All, 2)));
				}
				else
				{
					yield return StartCoroutine(Diamonds(2));
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(2))
				{
					if (Utils.RandomBool(2))
					{
						yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.Index, Shape.Cross, Pattern.All, 19), false));
					}
					else
					{
						yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.Square)));
					}
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(FollowBreak());
				}
				else
				{
					yield return StartCoroutine(Diamonds(3));
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.BotLeftTopRightTwo)));
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(FollowBreakOrbital());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(RevolverRain());
				}
				else
				{
					yield return StartCoroutine(Diamonds(4));
				}
			}
			yield return new WaitForSeconds(loopDelay);
			for (int i = 0; i < 10; i++)
			{
				mov.MoveToRandom();
				yield return new WaitUntil(() => mov.state == State.Idle);
				yield return new WaitForSeconds(beingObj.movementDelay);
			}
			StageCheck();
		}
	}

	private IEnumerator RandomBreakOrbital(TileApp tilePattern, bool hitRandom = true)
	{
		dontHitAnim = true;
		float randAdd = Random.Range(0f, 0.4f);
		anim.SetTrigger("specialStart");
		anim.SetTrigger("specialEnd");
		legacySpellList[0].StartCoroutine(legacySpellList[0].GetComponent<BreakerOrbitals>().Cast(orbitalLerpTime[Mathf.Clamp(tier, 0, orbitalLerpTime.Length - 1)], 7f));
		yield return new WaitForSeconds(0.5f + randAdd);
		savedTileList = new List<Tile>(battleGrid.Get(tilePattern, 0, this));
		anim.SetTrigger("specialStart");
		anim.SetTrigger("specialEnd");
		savedTileList = new List<Tile>(battleGrid.Get(tilePattern, 0, this));
		spellObjList[2].StartCast();
		dontHitAnim = false;
		if (tier > 1)
		{
			if (hitRandom)
			{
				if (tier > 2)
				{
					StartCoroutine(HitRandomColumns(4));
				}
				else if (tier > 3)
				{
					StartCoroutine(HitRandomColumns(5));
				}
				else if (tier > 4)
				{
					StartCoroutine(HitRandomColumns(6));
				}
				else
				{
					StartCoroutine(HitRandomColumns(2));
				}
			}
			for (int i = 0; i < 4; i++)
			{
				yield return new WaitForSeconds(1f);
				mov.MoveToRandom();
			}
			yield return new WaitForSeconds(2f);
		}
		else
		{
			yield return new WaitForSeconds(6f);
		}
	}

	private IEnumerator CrossBlast()
	{
		dontHitAnim = true;
		float baseDelay = 1.4f - 0.05f * (float)tier;
		int[] indexes = new int[4] { 18, 19, 10, 11 };
		int indexNum = indexes[Random.Range(0, indexes.Length)];
		anim.SetTrigger("specialStart");
		savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.CrossAnti, Pattern.All, indexNum), 0, this));
		spellObjList[2].StartCast();
		anim.SetTrigger("specialEnd");
		yield return new WaitForSeconds(baseDelay + 0.4f);
		if (tier < 2)
		{
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.Column, Pattern.All, indexNum), 0, this));
		}
		else
		{
			StartCoroutine(HitRandomDiagonals(3));
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.Column, Pattern.All, indexNum), 0, this));
		}
		anim.SetTrigger("charge");
		anim.SetTrigger("release");
		spellObjList[1].StartCast();
		if (tier < 2)
		{
			yield return new WaitForSeconds(baseDelay);
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.Horizontal, Pattern.All, indexNum), 0, this));
		}
		else
		{
			yield return new WaitForSeconds(baseDelay / 2f);
			mov.MoveToRandom();
			yield return new WaitForSeconds(baseDelay / 2f);
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.Horizontal, Pattern.All, indexNum), 0, this));
		}
		anim.SetTrigger("charge");
		anim.SetTrigger("release");
		spellObjList[1].StartCast();
		if (tier < 2)
		{
			yield return new WaitForSeconds(baseDelay + 0.1f);
		}
		else
		{
			if (tier > 2)
			{
				StartCoroutine(HitRandomDiagonals(3));
			}
			yield return new WaitForSeconds(baseDelay / 2f);
			yield return new WaitForSeconds(baseDelay / 2f);
		}
		savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.ColumnWide, Pattern.All, indexNum), 0, this));
		anim.SetTrigger("charge");
		anim.SetTrigger("release");
		spellObjList[1].StartCast();
		if (tier < 2)
		{
			yield return new WaitForSeconds(baseDelay + 0.1f);
		}
		else
		{
			yield return new WaitForSeconds(baseDelay / 2f);
			yield return new WaitForSeconds(baseDelay / 2f);
		}
		savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.HorizontalWide, Pattern.All, indexNum), 0, this));
		anim.SetTrigger("charge");
		anim.SetTrigger("release");
		spellObjList[1].StartCast();
		dontHitAnim = false;
		yield return new WaitForSeconds(1.9f);
	}

	private IEnumerator HitRandomColumns(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			int patternNum = Random.Range(0, 8);
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
			CastSpellObj("TerraBlaster");
			yield return new WaitForSeconds(0.7f);
		}
	}

	private IEnumerator HitRandomDiagonals(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			int patternNum = Random.Range(0, 4);
			savedTileList.Clear();
			switch (patternNum)
			{
			case 0:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.DiagonalTopLeft, Pattern.All, 4), 0, this));
				break;
			case 1:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.DiagonalTopRight, Pattern.Reverse, 4), 0, this));
				break;
			case 2:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.DiagonalTopLeft, Pattern.Reverse, 4), 0, this));
				break;
			case 3:
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Player, Shape.DiagonalTopRight, Pattern.All, 4), 0, this));
				break;
			}
			CastSpellObj("TerraBlaster");
			yield return new WaitForSeconds(0.7f);
		}
	}

	private IEnumerator FollowBreak()
	{
		dontHitAnim = true;
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		spellObjList[0].numShots = 6 + tier * 2;
		Mathf.Clamp(spellObjList[0].numShots, 0f, 14f);
		spellObjList[0].StartCast();
		yield return new WaitForSeconds(0.6f * spellObjList[0].numShots);
		anim.SetTrigger("release");
		dontHitAnim = false;
		yield return new WaitForSeconds(1f);
	}

	private IEnumerator FollowBreakOrbital()
	{
		dontHitAnim = true;
		spellObjList[0].numShots = 4 + tier;
		Debug.Log(spellObjList[0].numShots);
		spellObjList[0].StartCast();
		legacySpellList[0].StartCoroutine(legacySpellList[0].GetComponent<BreakerOrbitals>().Cast(orbitalLerpTime[Mathf.Clamp(tier, 0, orbitalLerpTime.Length - 1)], 6f));
		yield return new WaitForSeconds(0.6f * spellObjList[0].numShots + 2f);
		dontHitAnim = false;
		yield return new WaitForSeconds(1.5f);
	}

	private IEnumerator RevolverRain()
	{
		float baseDelay = 0.5f;
		for (int j = 1; j <= tier; j++)
		{
			baseDelay -= 0.04f;
		}
		for (int i = 0; i < (health.max - health.current) / 200 + 10; i++)
		{
			anim.SetTrigger("throw");
			spellObjList[3].StartCast();
			yield return new WaitForSeconds(baseDelay);
			if (i > 8)
			{
				break;
			}
		}
		yield return new WaitForSeconds(1f);
	}

	private IEnumerator Diamonds(int amount)
	{
		dontHitAnim = true;
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		savedTileList.Clear();
		savedTileList.Add(battleGrid.Get(new TileApp(Location.Index), 0, this)[0]);
		savedTileList.Add(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.All, 4), 0, this)[0]);
		savedTileList.Add(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.All, 25), 0, this)[0]);
		savedTileList.Add(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.All, 28), 0, this)[0]);
		CastSpellObj("TerraPattern");
		yield return new WaitForSeconds(0.9f);
		int diamondNum = tier;
		diamondNum = Mathf.Clamp(diamondNum, 1, 2);
		List<Tile> tempTileList = new List<Tile>();
		savedTileList.Clear();
		if (tier < 3 && amount > 2)
		{
			amount = 2;
		}
		for (int i = 0; i < amount; i++)
		{
			for (int x = 0; x < diamondNum; x++)
			{
				if (tempTileList.Count < 1)
				{
					tempTileList.Add(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.All, 18), 0, this)[0]);
					tempTileList.Add(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.All, 19), 0, this)[0]);
					tempTileList.Add(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.All, 26), 0, this)[0]);
					tempTileList.Add(battleGrid.Get(new TileApp(Location.Index, Shape.Default, Pattern.All, 27), 0, this)[0]);
				}
				savedTileList.Clear();
				savedTileList.Add(tempTileList[Random.Range(0, tempTileList.Count)]);
				tempTileList.Remove(savedTileList[0]);
				CastSpellObj("TerraDiamond");
			}
			yield return new WaitForSeconds(1.1f);
			if (tier > 1 && diamondNum < 2)
			{
				mov.MoveToRandom();
			}
		}
		yield return new WaitForSeconds(0.9f);
		anim.SetTrigger("release");
		dontHitAnim = false;
	}

	protected override void DownEffects()
	{
		base.DownEffects();
	}

	public override IEnumerator ExecutePlayerC()
	{
		yield return new WaitForSeconds(0.2f);
		ResetAnimTriggers();
		yield return new WaitForSeconds(0.4f);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.HalfField), 1, ctrl.currentPlayer));
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return new WaitForSeconds(0.2f);
		yield return StartCoroutine(_StartDialogue("Execution"));
		while (ctrl.PlayersActive() && ctrl.currentPlayer.downed)
		{
			anim.SetTrigger("charge");
			anim.SetTrigger("channel");
			yield return new WaitForSeconds(0.3f);
			CastSpellObj("TerraExecute");
			yield return new WaitForSeconds(0.4f);
			anim.SetTrigger("release");
			yield return new WaitForSeconds(1.1f);
		}
		if (ctrl.PlayersActive())
		{
			StartCoroutine(StartLoopC());
		}
	}
}
