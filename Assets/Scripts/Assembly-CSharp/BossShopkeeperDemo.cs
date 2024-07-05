using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BossShopkeeperDemo : Boss
{
	public float[] orbitalLerpTime;

	public bool attacking = false;

	public bool diagStraight = false;

	public bool pullSlam = false;

	public bool summonSniper = false;

	public bool crossBlast = false;

	public bool orbitalBlast = false;

	public TileApp breakLocation;

	public List<string> killLines;

	public bool showIntro = false;

	public override void Start()
	{
		base.Start();
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
		PlayerPrefs.GetInt("ShopkeeperIntro");
		yield return new WaitForSeconds(0.5f);
		if (S.I.EDITION == Edition.DemoLive && S.I.ANIMATIONS && !introPlayed)
		{
			introPlayed = true;
			talkBubble.Show();
			yield return new WaitForSeconds(0.1f);
			talkBubble.AnimateText("Someone's a fast learner!");
			yield return new WaitForSeconds(1.7f);
			talkBubble.AnimateText("Unfortunately for you");
			yield return new WaitForSeconds(1.3f);
			talkBubble.AnimateText("This is the end of the demo.");
			yield return new WaitForSeconds(1.6f);
			talkBubble.AnimateText("So...");
			yield return new WaitForSeconds(1.4f);
			talkBubble.AnimateText("Lets");
			yield return new WaitForSeconds(0.6f);
			talkBubble.AnimateText("Have");
			yield return new WaitForSeconds(0.6f);
			talkBubble.AnimateText("Some");
			yield return new WaitForSeconds(0.6f);
			talkBubble.AnimateText("FUN!");
			yield return new WaitForSeconds(0.8f);
			talkBubble.Hide();
			base.dontInterruptAnim = true;
		}
		anim.runtimeAnimatorController = ctrl.itemMan.GetAnim("Shopcreeper");
		int arger = 0;
		while (true)
		{
			LoopStart();
			if (stage == 0)
			{
				if (diagStraight)
				{
					yield return StartCoroutine(DiagStraight(true));
				}
				else if (pullSlam)
				{
					yield return StartCoroutine(PullSlam());
				}
				else if (summonSniper)
				{
					yield return StartCoroutine(Summon(2));
				}
				else if (crossBlast)
				{
					yield return StartCoroutine(CrossBlast());
				}
				else if (orbitalBlast)
				{
					yield return StartCoroutine(RandomBreakOrbital(breakLocation));
				}
				else if (Utils.RandomBool(3))
				{
					if (Utils.RandomBool(2))
					{
						yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.Base, Shape.Column, Pattern.All, 5)));
					}
					else
					{
						yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.Base, Shape.Horizontal)));
					}
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(PullSlam());
				}
				else
				{
					yield return StartCoroutine(CrossBlast());
				}
				yield return new WaitForEndOfFrame();
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(DiagStraight());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(CrossBlast());
				}
				else if (Utils.RandomBool(2))
				{
					if (Utils.RandomBool(2))
					{
						yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.Square)));
					}
					else
					{
						yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.Index, Shape.Cross, Pattern.All, 19), false));
					}
				}
				else
				{
					yield return StartCoroutine(Diamonds(2));
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(DiagStraight(true));
				}
				else if (Utils.RandomBool(3))
				{
					if (Utils.RandomBool(2))
					{
						yield return StartCoroutine(RandomBreakOrbital(new TileApp(Location.BotLeftTopRightTwo)));
					}
					else
					{
						yield return StartCoroutine(Diamonds(2));
					}
				}
				else
				{
					int summonAmount = 2;
					yield return StartCoroutine(Summon(summonAmount));
				}
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

	private IEnumerator FinalBeam()
	{
		spellObjList[0].StartCast();
		yield return new WaitForSeconds(1f);
	}

	private IEnumerator FinalWarning()
	{
		CastSpellObj("TerraBlaster");
		yield return new WaitForSeconds(1f);
	}

	private IEnumerator DiagStraight(bool triple = false)
	{
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
				if (upDownSpellNum == 1)
				{
					CastSpellObj("RevaBeamUp");
				}
				else
				{
					CastSpellObj("RevaBeamDown");
				}
			}
			if (tier > 2)
			{
				CastSpellObj("RevaBeamBot");
				CastSpellObj("RevaBeamTop");
			}
			yield return new WaitForSeconds(0.5f);
			anim.SetTrigger("release");
			yield return new WaitForSeconds(GetDelay(0.5f, 0.1f));
		}
	}

	private IEnumerator PullSlam()
	{
		anim.SetTrigger("specialStart");
		spellObjList[6].StartCast();
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("specialEnd");
		mov.MoveTo(4, 2);
		yield return new WaitForSeconds(0.7f);
		base.dontInterruptAnim = true;
		anim.SetTrigger("channel");
		anim.SetTrigger("charge");
		yield return new WaitForSeconds(0.2f);
		ctrl.camCtrl.Shake(1);
		for (int i = 4; i <= 7; i++)
		{
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.All, i), 0, this));
			spellObjList[7].StartCast();
			ctrl.camCtrl.Shake(0);
			yield return new WaitForSeconds(0.45f);
		}
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
	}

	private IEnumerator Summon(int amount = 1)
	{
		yield return new WaitForSeconds(1f);
		for (int i = 0; i < amount; i++)
		{
			spellObjList[8].StartCast();
			base.dontInterruptAnim = true;
			anim.SetTrigger("specialStart");
			yield return new WaitForSeconds(0.5f);
			anim.SetTrigger("specialEnd");
			yield return new WaitForSeconds(0.4f);
			base.dontInterruptAnim = false;
			yield return new WaitForSeconds(0.3f);
			mov.PatrolRandomEmpty();
			yield return new WaitUntil(() => mov.state == State.Idle);
		}
	}

	private IEnumerator RandomBreakOrbital(TileApp tilePattern, bool hitRandom = true)
	{
		dontHitAnim = true;
		float randAdd = Random.Range(0f, 0.4f);
		anim.SetTrigger("specialStart");
		anim.SetTrigger("specialEnd");
		legacySpellList[0].StartCoroutine(legacySpellList[0].GetComponent<BreakerOrbitals>().Cast(orbitalLerpTime[tier], 7f));
		yield return new WaitForSeconds(0.5f + randAdd);
		savedTileList = new List<Tile>(battleGrid.Get(tilePattern, 0, this));
		anim.SetTrigger("specialStart");
		anim.SetTrigger("specialEnd");
		savedTileList = new List<Tile>(battleGrid.Get(tilePattern, 0, this));
		CastSpellObj("TerraPattern");
		dontHitAnim = false;
		yield return new WaitForSeconds(6f);
	}

	private IEnumerator CrossBlast()
	{
		dontHitAnim = true;
		float baseDelay = 1.7f - 0.05f * (float)tier;
		yield return new WaitForSeconds(baseDelay);
		int[] indexes = new int[4] { 18, 19, 10, 11 };
		int indexNum = indexes[Random.Range(0, indexes.Length)];
		anim.SetTrigger("specialStart");
		savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.CrossAnti, Pattern.All, indexNum), 0, this));
		CastSpellObj("ShopkeeperPattern");
		yield return new WaitForSeconds(0.4f);
		anim.SetTrigger("specialEnd");
		yield return new WaitForSeconds(baseDelay);
		if (tier < 2)
		{
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.Column, Pattern.All, indexNum), 0, this));
		}
		else
		{
			StartCoroutine(HitRandomDiagonals(3));
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.Column, Pattern.All, indexNum), 0, this));
		}
		anim.SetTrigger("channel");
		anim.SetTrigger("charge");
		CastSpellObj("ShopkeeperBlaster");
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
		CastSpellObj("ShopkeeperBlaster");
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
			mov.MoveToRandom();
			yield return new WaitForSeconds(baseDelay / 2f);
		}
		savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.ColumnWide, Pattern.All, indexNum), 0, this));
		CastSpellObj("ShopkeeperBlaster");
		if (tier < 2)
		{
			yield return new WaitForSeconds(baseDelay + 0.1f);
		}
		else
		{
			yield return new WaitForSeconds(baseDelay / 2f);
			mov.MoveToRandom();
			yield return new WaitForSeconds(baseDelay / 2f);
		}
		savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Index, Shape.HorizontalWide, Pattern.All, indexNum), 0, this));
		CastSpellObj("ShopkeeperBlaster");
		dontHitAnim = false;
		anim.SetTrigger("release");
		yield return new WaitForSeconds(1.9f);
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
		int diamondNum2 = tier;
		diamondNum2 = Mathf.Clamp(diamondNum2, 1, 2);
		List<Tile> tempTileList = new List<Tile>();
		savedTileList.Clear();
		GetSpellObj("TerraDiamond").warningDuration = 0.8f;
		GetSpellObj("TerraDiamond").castDelay = 0.8f;
		for (int i = 0; i < amount; i++)
		{
			for (int x = 0; x < diamondNum2; x++)
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
			if (tier > 1)
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
		anim.runtimeAnimatorController = ctrl.itemMan.GetAnim("Shopcreeper");
		base.DownEffects();
		AddInvince(50f);
	}

	public override IEnumerator Executed()
	{
		yield return new WaitForSeconds(0.1f);
		if (S.I.scene == GScene.DemoLive)
		{
			ctrl.idCtrl.runCtrl.GoToNextZone(ctrl.runCtrl.currentZoneDot.nextDots[0]);
		}
		else
		{
			yield return StartCoroutine(_003C_003En__0());
		}
	}

	public override IEnumerator DownTalkL()
	{
		AddInvince(50f);
		talkBubble.Show();
		yield return new WaitForSeconds(0.1f);
		talkBubble.AnimateText("Haha...");
		yield return new WaitForSeconds(0.6f);
		talkBubble.AnimateText("Y'know what?");
		yield return new WaitForSeconds(1.6f);
		talkBubble.AnimateText("I'll let HER deal with you.");
		yield return new WaitForSeconds(1.7f);
		talkBubble.Hide();
		yield return new WaitForSeconds(0.5f);
		ti.mainBattleGrid.ClearField(true, false, false);
		ctrl.StopAllCoroutines();
		BC.GTimeScale = 1f;
		runCtrl.StartZone(runCtrl.currentRun.zoneNum, runCtrl.currentZoneDot.nextDots[0]);
	}

	public override IEnumerator ExecutePlayerC()
	{
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

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__0()
	{
		return base.Executed();
	}
}
