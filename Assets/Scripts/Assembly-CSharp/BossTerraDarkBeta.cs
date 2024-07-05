using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTerraDarkBeta : Boss
{
	public bool attacking = false;

	public bool darkStorm;

	public bool darkDrain;

	public bool darkZigZag;

	public bool darkSweepers;

	public bool darkUltima;

	public bool darkBreaker;

	private bool saidLine = false;

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
		SetInvince(200f);
		int arger = 0;
		while (true)
		{
			LoopStart();
			if (stage == 0)
			{
				if (darkStorm)
				{
					yield return StartCoroutine(_DarkStorm());
				}
				if (darkDrain)
				{
					yield return StartCoroutine(_DarkDrain());
				}
				else if (darkZigZag)
				{
					yield return StartCoroutine(_DarkZigZag());
				}
				else if (darkSweepers)
				{
					yield return StartCoroutine(_DarkSweepers());
				}
				else if (darkUltima)
				{
					yield return StartCoroutine(_DarkUltima());
				}
				else if (darkBreaker)
				{
					yield return StartCoroutine(_DarkBreaker());
				}
				else if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(_DarkStorm());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_DarkDrain());
				}
				else
				{
					yield return StartCoroutine(_DarkUltima());
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_DarkUltima());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_DarkDrain());
				}
				else
				{
					yield return StartCoroutine(_DarkSweepers());
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(3))
				{
					yield return StartCoroutine(_DarkUltima());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(_DarkSweepers());
				}
				else
				{
					yield return StartCoroutine(_DarkBreaker());
				}
			}
			while (loopDelay > 0f)
			{
				loopDelay -= Time.deltaTime;
				yield return new WaitForEndOfFrame();
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

	private IEnumerator _DarkStorm()
	{
		yield return new WaitForSeconds(0.5f);
		base.dontInterruptAnim = true;
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.4f);
		spellObjList[0].StartCast();
		yield return new WaitForSeconds(4.9f);
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(0.2f);
		yield return StartCoroutine(_Instakill());
	}

	private IEnumerator _DarkDrain()
	{
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.5f);
		int i = 0;
		while (ctrl.currentPlayer.duelDisk.currentMana >= 1f)
		{
			i++;
			ctrl.currentPlayer.duelDisk.currentMana -= 0.5f;
			spellObjList[1].StartCast();
			yield return new WaitForSeconds(0.2f);
			if (i > 10)
			{
				break;
			}
		}
		anim.SetTrigger("release");
		yield return StartCoroutine(_Instakill());
	}

	private IEnumerator _Instakill()
	{
		if (!saidLine)
		{
			saidLine = true;
			yield return new WaitForSeconds(talkBubble.AnimateText("I'm gonna Ragna-ROCK your world."));
		}
		while ((bool)ctrl.currentPlayer)
		{
			anim.SetTrigger("charge");
			anim.SetTrigger("channel");
			yield return new WaitForSeconds(0.3f);
			spellObjList[7].StartCast();
			yield return new WaitForSeconds(0.3f);
			ctrl.currentPlayer.Damage(999999, true, true, true);
			yield return new WaitForSeconds(0.5f);
		}
		anim.SetTrigger("release");
	}

	private IEnumerator _DarkZigZag()
	{
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.5f);
		anim.SetTrigger("release");
		savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.Front, Shape.Column), 4, this));
		spellObjList[2].StartCast();
		yield return new WaitForEndOfFrame();
	}

	private IEnumerator _DarkSweepers()
	{
		yield return new WaitForSeconds(0.5f);
		base.dontInterruptAnim = true;
		spellObjList[3].StartCast();
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(2.7f);
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
		mov.SetState(State.Idle);
	}

	private IEnumerator _DarkUltima()
	{
		yield return new WaitForSeconds(0.3f);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.RandOtherUnique), 0, this)[0], true, false);
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.8f);
		anim.SetTrigger("release");
		for (int i = 0; i < 3; i++)
		{
			if (i > 0)
			{
				anim.SetTrigger("charge");
				anim.SetTrigger("release");
			}
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.End), 0, this));
			spellObjList[6].StartCast();
			yield return new WaitForSeconds(0.45f);
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.End, Shape.O), 0, this));
			spellObjList[6].StartCast();
			yield return new WaitForSeconds(0.45f);
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.End, Shape.OBig), 0, this));
			spellObjList[6].StartCast();
			yield return new WaitForSeconds(0.45f);
			savedTileList = new List<Tile>(battleGrid.Get(new TileApp(Location.End, Shape.OBigger), 0, this));
			spellObjList[6].StartCast();
			if (i < 2)
			{
				mov.MoveToTile(battleGrid.Get(new TileApp(Location.RandOtherUnique), 0, this)[0], true, false);
				yield return new WaitWhile(() => mov.state == State.Moving);
			}
			yield return new WaitForSeconds(0.1f);
		}
		mov.MoveTo(mov.currentTile.x, mov.currentTile.y, true, false);
		yield return new WaitForSeconds(0.2f);
		yield return StartCoroutine(_Instakill());
	}

	private IEnumerator _DarkBreaker()
	{
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(0.3f);
		for (int i = 0; i < 6; i++)
		{
			spellObjList[4].StartCast();
			yield return new WaitForSeconds(0.4f);
		}
		yield return new WaitForSeconds(0.3f);
		anim.SetTrigger("release");
		spellObjList[5].StartCast();
		yield return new WaitForEndOfFrame();
	}

	public override IEnumerator ExecutePlayerC()
	{
		yield return StartCoroutine(_Instakill());
	}

	protected override void LastWord()
	{
		talkBubble.AnimateText("NO-");
	}

	public override IEnumerator DownC(bool destroyStructures = true, bool showZoneButtons = true)
	{
		yield return StartCoroutine(_Instakill());
	}

	protected override void DownEffects()
	{
		SetInvince(200f);
		base.DownEffects();
	}

	public override IEnumerator _Mercy()
	{
		yield return StartCoroutine(ExecutePlayerC());
	}
}
