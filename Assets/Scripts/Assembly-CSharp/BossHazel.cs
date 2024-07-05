using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHazel : Boss
{
	public bool attacking = false;

	public bool summonTurret;

	public bool slowMissiles;

	public bool blockTrap;

	public bool sideburn;

	public bool blastShield;

	public bool beamCrystals;

	public bool shockwave;

	public bool scene;

	public override void Start()
	{
		base.Start();
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
					if (scene)
					{
						switch (arger)
						{
						case 0:
							yield return StartCoroutine(Shockwave());
							yield return StartCoroutine(BlockTrap());
							mov.Move(1, 0);
							yield return new WaitUntil(() => mov.state == State.Idle);
							yield return new WaitForSeconds(beingObj.movementDelay);
							mov.Move(0, -1);
							yield return new WaitUntil(() => mov.state == State.Idle);
							yield return new WaitForSeconds(beingObj.movementDelay);
							yield return StartCoroutine(SummonTurret());
							break;
						case 2:
							yield return StartCoroutine(SummonTurret());
							break;
						case 3:
							yield return StartCoroutine(Shockwave());
							break;
						case 4:
							yield return StartCoroutine(SummonTurret());
							break;
						case 5:
							yield return StartCoroutine(Slowmissiles());
							break;
						case 6:
							yield return StartCoroutine(Shockwave());
							break;
						default:
							if (arger == 1)
							{
								yield return StartCoroutine(Shockwave());
							}
							break;
						case 1:
							break;
						}
					}
					else if (summonTurret)
					{
						yield return StartCoroutine(SummonTurret());
					}
					else if (slowMissiles)
					{
						yield return StartCoroutine(Slowmissiles());
					}
					else if (blockTrap)
					{
						yield return StartCoroutine(BlockTrap());
					}
					else if (sideburn)
					{
						yield return StartCoroutine(Sideburn());
					}
					else if (blastShield)
					{
						yield return StartCoroutine(BlastShield());
					}
					else if (beamCrystals)
					{
						yield return StartCoroutine(BeamCrystals());
					}
					else if (shockwave)
					{
						yield return StartCoroutine(Shockwave());
					}
					yield return new WaitForSecondsRealtime(3f);
					continue;
				}
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(SummonTurret());
				}
				else if (Utils.RandomBool(2) && battleGrid.currentStructures.Count < 2)
				{
					yield return StartCoroutine(Slowmissiles());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(Sideburn());
				}
				else
				{
					yield return StartCoroutine(Shockwave());
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(SummonTurret(2));
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(BlockTrap());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(Sideburn());
				}
				else
				{
					yield return StartCoroutine(Shockwave());
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(3) && battleGrid.currentStructures.Count < 2)
				{
					yield return StartCoroutine(Slowmissiles());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(Sideburn(2));
				}
				else if (Utils.RandomBool(2))
				{
					if (Utils.RandomBool(2))
					{
						yield return StartCoroutine(BlastShield());
					}
					else
					{
						yield return StartCoroutine(BeamCrystals());
					}
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(BlockTrap());
				}
				else
				{
					yield return StartCoroutine(Shockwave());
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

	private IEnumerator SummonTurret(int amount = 1)
	{
		dontHitAnim = true;
		amount += tier / 2;
		for (int i = 0; i < amount; i++)
		{
			anim.SetTrigger("throw");
			spellObjList[0].StartCast();
			yield return new WaitForSeconds(0.5f);
			mov.Move(-1, 0);
			yield return new WaitForSeconds(0.5f);
		}
		dontHitAnim = false;
		yield return new WaitForSeconds(2f);
		if (battleGrid.currentStructures.Count >= 2)
		{
			yield return StartCoroutine(Empower());
		}
	}

	private IEnumerator Slowmissiles(int amount = 1)
	{
		dontHitAnim = true;
		mov.Move(-1, 0);
		while (mov.state == State.Moving)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		amount += tier / 2;
		for (int i = 0; i < amount; i++)
		{
			foreach (EffectApp efApp in spellObjList[1].efApps)
			{
				if (efApp.value == "Slowmissile")
				{
					efApp.amount = 40f;
				}
			}
			anim.SetTrigger("throw");
			spellObjList[1].StartCast();
			yield return new WaitForSeconds(1.3f);
		}
		dontHitAnim = false;
		yield return new WaitForSeconds(2f);
	}

	private IEnumerator BlockTrap()
	{
		yield return new WaitForSeconds(0.5f);
		GetSpellObj("HazelBlockTrap").numTiles = 12 - Mathf.Clamp(4 - tier, 0, 3);
		CastSpellObj("HazelBlockTrap");
		anim.SetTrigger("throw");
		savedTileList.Clear();
		StartCoroutine(BeamCrystals());
		yield return new WaitForSeconds(2.8f);
		for (int i = 7; i >= 3; i--)
		{
			if (tier <= 1 || S.I.RECORD_MODE)
			{
				savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.All, i), 0, this));
			}
			else
			{
				if (i == 4 && tier > 2)
				{
					savedTileList.Clear();
					savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.All, 6), 0, this));
					savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.All, 5), 0, this));
				}
				savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.All, i), 0, this));
				if (i == 4 && tier > 2)
				{
					savedTileList.AddRange(battleGrid.Get(new TileApp(Location.Base, Shape.Column, Pattern.All, 3), 0, this));
					i--;
				}
				if (i <= 5 && tier == 2)
				{
					savedTileList.RemoveRange(0, 4);
				}
				yield return new WaitForSeconds(0.1f);
			}
			anim.SetTrigger("throw");
			spellObjList[4].StartCast();
			ctrl.camCtrl.Shake(0);
			yield return new WaitForSeconds(0.4f);
			if (tier > 1)
			{
				mov.PatrolRandomEmpty();
			}
			if (S.I.RECORD_MODE)
			{
				yield return new WaitForSeconds(0.1f);
			}
			else
			{
				yield return new WaitForSeconds(0.8f - 0.02f * (float)tier);
			}
		}
		yield return new WaitForSeconds(0.3f);
	}

	private IEnumerator Sideburn(int amount = 1)
	{
		dontHitAnim = true;
		amount += tier / 2;
		for (int i = 0; i < amount; i++)
		{
			anim.SetTrigger("throw");
			spellObjList[5].StartCast();
			yield return new WaitForSeconds(0.7f);
		}
		dontHitAnim = false;
		yield return new WaitForSeconds(1.3f);
	}

	private IEnumerator BlastShield()
	{
		mov.MoveTo(7, ctrl.currentPlayer.mov.currentTile.y);
		yield return new WaitForSeconds(0.5f);
		if (tier == 0)
		{
			GetSpellObj("HazelSummonBlastShields").numTiles = 2;
		}
		else if (tier == 1)
		{
			GetSpellObj("HazelSummonBlastShields").numTiles = 3;
		}
		spellObjList[6].StartCast();
		yield return new WaitForSeconds(2f);
	}

	private IEnumerator BeamCrystals()
	{
		yield return new WaitForSeconds(0.3f);
		mov.Move(-1, 0);
		yield return new WaitForSeconds(0.4f);
		CastSpellObj("HazelSummonBeamCrystals");
		yield return new WaitForSeconds(1f);
	}

	private IEnumerator Shockwave()
	{
		dontHitAnim = true;
		float baseDelay = 0.5f;
		for (int j = 1; j <= tier; j++)
		{
			baseDelay -= 0.03f;
		}
		for (int i = 0; i <= tier; i++)
		{
			spellObjList[8].timeBetweenShots = 0.3f - (float)tier * 0.03f;
			anim.SetTrigger("charge");
			yield return new WaitForSeconds(baseDelay);
			spellObjList[8].StartCast();
			anim.SetTrigger("release");
			yield return new WaitForSeconds(baseDelay);
		}
		dontHitAnim = false;
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator Empower()
	{
		if (tier > 0)
		{
			yield return new WaitForSeconds(0.5f);
			GetSpellObj("HazelEmpower").GetEffect(Effect.SpellPowerBattle).amount = tier * 10;
			GetSpellObj("HazelEmpower").GetEffect(Effect.Shield).amount = 20 + tier * 10;
			CastSpellObj("HazelEmpower");
			yield return new WaitForSeconds(0.8f);
		}
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
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.HalfField, Shape.ColumnWide, Pattern.Moveable), 1, ctrl.currentPlayer));
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return new WaitForSeconds(0.4f);
		mov.Move(-1, 0);
		yield return StartCoroutine(_StartDialogue("Execution"));
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("throw");
		battleGrid.FixAllTiles();
		CastSpellObj("HazelSummonHex");
		yield return new WaitForSeconds(3f);
		anim.SetTrigger("throw");
		yield return new WaitForSeconds(1f);
		if (ctrl.PlayersActive())
		{
			StartCoroutine(StartLoopC());
		}
	}
}
