using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BossShiso : Boss
{
	public bool attacking = false;

	public bool steal;

	public bool layTraps;

	public bool shuffler;

	public bool teleshot;

	public bool crossshot;

	public bool fan;

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
		yield return StartCoroutine(_StartDialogue("Intro"));
		int arger = 0;
		while (true)
		{
			LoopStart();
			if (stage == 0)
			{
				if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
				{
					if (steal)
					{
						yield return StartCoroutine(_Steal());
					}
					else if (layTraps)
					{
						yield return StartCoroutine(_LayTraps());
					}
					else if (shuffler)
					{
						yield return StartCoroutine(_Shuffler());
					}
					else if (teleshot)
					{
						yield return StartCoroutine(_Teleshot());
					}
					else if (crossshot)
					{
						yield return StartCoroutine(_Crossshot());
					}
					else if (fan)
					{
						yield return StartCoroutine(_Fan());
					}
					yield return new WaitForSecondsRealtime(3f);
					continue;
				}
				if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_Steal());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_Shuffler());
				}
				else
				{
					yield return StartCoroutine(_LayTraps());
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_Teleshot());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_Crossshot());
				}
				else
				{
					yield return StartCoroutine(_Shuffler());
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(_LayTraps());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_Crossshot());
				}
				else
				{
					yield return StartCoroutine(_Shuffler());
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

	private IEnumerator _Steal()
	{
		float delayTime = 0.33f;
		for (int j = 0; j <= tier; j++)
		{
			delayTime -= 0.03f;
		}
		foreach (EffectApp efApp in spellObjList[0].efApps)
		{
			if (efApp.effect == Effect.Jam)
			{
				efApp.amount = Mathf.Clamp(tier, 1, 2);
			}
		}
		for (int i = 0; i <= tier && i <= 2; i++)
		{
			mov.MoveToRandom();
			yield return new WaitUntil(() => mov.state == State.Idle);
			anim.SetBool("dashing", true);
			CastSpellObj("ShisoStealWarning");
			yield return new WaitForSeconds(delayTime + 0.2f);
			spellObjList[0].StartCast();
			yield return new WaitForSeconds(0.3f);
			yield return new WaitUntil(() => mov.state == State.Idle);
			anim.SetBool("dashing", false);
		}
	}

	private IEnumerator _LayTraps()
	{
		float baseDelay = 0.5f;
		for (int i = 0; i <= tier; i++)
		{
			baseDelay -= 0.02f;
		}
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.Index, Shape.Square, Pattern.Moveable, 22), 1, this));
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("throw");
		if (tier > 2)
		{
			GetSpellObj("ShisoLayTraps").numTiles = 3;
		}
		spellObjList[1].StartCast();
		yield return new WaitForSeconds(0.5f);
		List<List<Tile>> movingToTiles = new List<List<Tile>>();
		int numWaves = Mathf.Clamp(2 + tier, 0, 4);
		Player theCatchedPlayer = null;
		for (int j = 0; j < numWaves; j++)
		{
			movingToTiles.Add(battleGrid.Get(new TileApp(Location.Current, Shape.Adjacent, Pattern.Moveable), j, this));
			SpellObject theSpellObj = null;
			if (movingToTiles[j].Count > 0)
			{
				if (mov.currentTile.x - movingToTiles[j][0].x == 1)
				{
					theSpellObj = spellObjList[3];
				}
				else if (mov.currentTile.x - movingToTiles[j][0].x == -1)
				{
					theSpellObj = spellObjList[4];
				}
				else if (mov.currentTile.y - movingToTiles[j][0].y == -1)
				{
					theSpellObj = spellObjList[5];
				}
				else if (mov.currentTile.y - movingToTiles[j][0].y == 1)
				{
					theSpellObj = spellObjList[6];
				}
				mov.MoveToTile(movingToTiles[j]);
				yield return new WaitForSeconds(baseDelay + 0.2f);
				foreach (Player thePlayer3 in ctrl.currentPlayers)
				{
					if ((bool)thePlayer3 && (bool)thePlayer3.GetStatusEffect(Status.Root))
					{
						theCatchedPlayer = thePlayer3;
						break;
					}
				}
				if ((bool)theCatchedPlayer)
				{
					break;
				}
				anim.SetTrigger("throw");
				theSpellObj.StartCast();
				yield return new WaitForSeconds(baseDelay);
				foreach (Player thePlayer2 in ctrl.currentPlayers)
				{
					if ((bool)thePlayer2 && (bool)thePlayer2.GetStatusEffect(Status.Root))
					{
						theCatchedPlayer = thePlayer2;
						break;
					}
				}
				if ((bool)theCatchedPlayer)
				{
					break;
				}
			}
			if (j == numWaves - 1)
			{
				yield return new WaitForSeconds(0.2f);
			}
			foreach (Player thePlayer in ctrl.currentPlayers)
			{
				if ((bool)thePlayer && (bool)thePlayer.GetStatusEffect(Status.Root))
				{
					theCatchedPlayer = thePlayer;
					break;
				}
			}
			if ((bool)theCatchedPlayer)
			{
				break;
			}
		}
		if (theCatchedPlayer != null)
		{
			dontHitAnim = true;
			mov.MoveTo(theCatchedPlayer.mov.currentTile.x + 1, theCatchedPlayer.mov.currentTile.y, true, false);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			anim.SetTrigger("charge");
			yield return new WaitForSeconds(0.6f);
			spellObjList[7].StartCast();
			anim.SetTrigger("release");
			if (tier > 2)
			{
				yield return new WaitForSeconds(0.2f);
				anim.SetTrigger("charge");
				yield return new WaitForSeconds(0.1f);
				spellObjList[7].StartCast();
				anim.SetTrigger("release");
			}
			if (tier > 3)
			{
				yield return new WaitForSeconds(0.2f);
				anim.SetTrigger("charge");
				yield return new WaitForSeconds(0.1f);
				spellObjList[7].StartCast();
				anim.SetTrigger("release");
			}
			yield return new WaitForSeconds(0.4f);
			mov.MoveToCurrentTile();
			dontHitAnim = false;
		}
	}

	private IEnumerator _Jammer()
	{
		loopDelay = 7f;
		yield return new WaitForSeconds(0.5f);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.Player, Shape.Adjacent), 0, this)[0], true, false);
		spellObjList[2].StartCast();
		yield return new WaitForSeconds(2.5f);
		for (int i = 7; i >= 4; i--)
		{
			savedTileList = battleGrid.Get(new TileApp(Location.Base, Shape.Column), i, this);
			spellObjList[7].StartCast();
			ctrl.camCtrl.Shake(0);
			yield return new WaitForSeconds(1f);
		}
		yield return new WaitForSeconds(0.4f);
	}

	private IEnumerator _Shuffler()
	{
		base.dontInterruptAnim = true;
		float baseDelay = 0.4f;
		for (int i = 0; i <= tier; i++)
		{
			baseDelay -= 0.05f;
		}
		anim.SetTrigger("charge");
		CastSpellObj("ShisoShufflerWarning");
		yield return new WaitForSeconds(baseDelay + 0.5f);
		spellObjList[2].StartCast();
		anim.SetTrigger("release");
		yield return new WaitForSeconds(baseDelay * 2f);
		base.dontInterruptAnim = false;
	}

	private IEnumerator _Teleshot()
	{
		dontHitAnim = true;
		int[] x = new int[4] { -2, 2, -2, 2 };
		int[] y = new int[4];
		int tileInt = 0;
		float baseDelay = 0.45f;
		for (int j = 0; j <= tier; j++)
		{
			baseDelay -= 0.02f;
		}
		for (int i = 0; i <= tier + 1; i++)
		{
			if (tileInt > 3)
			{
				tileInt = 0;
			}
			mov.MoveToTile(ctrl.currentPlayer.TileLocal(x[tileInt], y[tileInt]), true, false);
			if (x[tileInt] < 0)
			{
				base.transform.right = Vector3.zero - Vector3.left;
			}
			else
			{
				base.transform.right = Vector3.zero - Vector3.right;
			}
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			if (tier > 1)
			{
				spellObjList[8].timeToTravel = baseDelay;
				spellObjList[8].warningDuration = baseDelay;
				spellObjList[8].StartCast();
			}
			anim.SetTrigger("charge");
			yield return new WaitForSeconds(baseDelay);
			spellObjList[7].StartCast();
			anim.SetTrigger("release");
			yield return new WaitForSeconds(baseDelay);
			tileInt++;
		}
		mov.MoveToCurrentTile();
		base.transform.right = Vector3.left;
		while (mov.state == State.Moving)
		{
			yield return null;
		}
		yield return new WaitForSeconds(baseDelay);
		dontHitAnim = false;
	}

	private IEnumerator _Crossshot()
	{
		dontHitAnim = true;
		float baseDelay = 0.45f;
		for (int j = 0; j <= tier; j++)
		{
			baseDelay -= 0.015f;
		}
		for (int i = 0; i <= tier + 1; i++)
		{
			mov.MoveToTile(battleGrid.Get(new TileApp(Location.Player, Shape.Adjacent), 0, this)[0], true, false);
			while (mov.state == State.Moving)
			{
				yield return null;
			}
			SpellObject crossShotSpell = GetSpellObj("ShisoCrossshot");
			if (tier >= 3)
			{
				crossShotSpell = GetSpellObj("ShisoCrossshotFull");
			}
			anim.SetTrigger("specialStart");
			crossShotSpell.timeToTravel = baseDelay;
			crossShotSpell.warningDuration = baseDelay;
			crossShotSpell.StartCast();
			yield return new WaitForSeconds(baseDelay);
			anim.SetTrigger("specialEnd");
			yield return new WaitForSeconds(baseDelay - 0.1f);
		}
		mov.MoveToCurrentTile();
		while (mov.state == State.Moving)
		{
			yield return null;
		}
		yield return new WaitForSeconds(baseDelay);
		dontHitAnim = false;
	}

	private IEnumerator _Fan()
	{
		dontHitAnim = true;
		float baseDelay = 0.45f;
		for (int i = 0; i <= tier; i++)
		{
			baseDelay -= 0.015f;
		}
		mov.Move(-1, 0);
		while (mov.state == State.Moving)
		{
			yield return null;
		}
		SpellObject shotSpell = GetSpellObj("ShisoFan");
		anim.SetTrigger("charge");
		shotSpell.StartCast();
		anim.SetTrigger("release");
		while (mov.state == State.Moving)
		{
			yield return null;
		}
		yield return new WaitForSeconds(baseDelay);
		dontHitAnim = false;
	}

	protected override void DownEffects()
	{
		base.DownEffects();
		base.transform.right = Vector3.left;
	}

	public override IEnumerator ExecutePlayerC()
	{
		yield return new WaitForSeconds(0.2f);
		ResetAnimTriggers();
		base.transform.right = Vector3.left;
		yield return new WaitForSeconds(0.4f);
		mov.MoveToTile(ctrl.currentPlayer.TileLocal(2), true, false);
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return new WaitForSeconds(0.4f);
		anim.SetTrigger("charge");
		yield return StartCoroutine(_StartDialogue("Execution"));
		while ((bool)ctrl.currentPlayer && ctrl.currentPlayer.downed)
		{
			yield return new WaitForSeconds(0.4f);
			spellObjList[7].StartCast();
			anim.SetTrigger("release");
			yield return new WaitForSeconds(2f);
			if ((bool)ctrl.currentPlayer && ctrl.currentPlayer.downed)
			{
				mov.MoveToTile(ctrl.currentPlayer.TileLocal(2), true, false);
				yield return new WaitWhile(() => mov.state == State.Moving);
				yield return new WaitForSeconds(0.4f);
			}
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
