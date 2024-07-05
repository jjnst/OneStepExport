using System.Collections;
using System.Collections.Generic;
using SonicBloom.Koreo;
using UnityEngine;

public class VioletteSequence : Spell
{
	public int hitboxWidth = 1;

	public int hitboxHeight = 1;

	private bool shootNext = false;

	private int currentPathRow;

	public List<Projectile> muLerpers = new List<Projectile>();

	protected override void Start()
	{
		base.Start();
		Koreographer.Instance.RegisterForEvents("Shoot", Shoot);
	}

	public override IEnumerator Cast()
	{
		if (being.mov.state != 0)
		{
			yield break;
		}
		being.mov.SetState(State.Attacking);
		int i = 0;
		Debug.Log("start looping");
		while (i < 60)
		{
			if (shootNext)
			{
				shootNext = false;
				Debug.Log("SHOTS FIRED");
				StartCoroutine(MusicPath());
				i++;
			}
			else
			{
				yield return new WaitForEndOfFrame();
			}
		}
		yield return new WaitForSeconds(0.2f);
		being.anim.SetTrigger("idle");
		being.mov.SetState(State.Idle);
	}

	public void Shoot(KoreographyEvent evt)
	{
		shootNext = true;
	}

	private IEnumerator MusicPath()
	{
		foreach (Projectile muLerp in muLerpers)
		{
			muLerp.mov.Forward();
		}
		int nonInt = Random.Range(0, 4);
		while (Mathf.Abs(nonInt - currentPathRow) > 1)
		{
			nonInt = Random.Range(0, 4);
		}
		for (int i = 0; i < 4; i++)
		{
			if (i != nonInt && i != currentPathRow)
			{
				Projectile orbitalBot = spellObj.P.CreateCustomLerper(spellObj, being.battleGrid.grid[6, i], MovPattern.None, 0.1f, 0f, false, 20f, 50, animShot.name, castSound.name);
				muLerpers.Add(orbitalBot);
			}
		}
		currentPathRow = nonInt;
		yield return new WaitForEndOfFrame();
	}

	private IEnumerator MusicBlast()
	{
		being.anim.SetTrigger("spellCast");
		being.anim.SetBool("raised", true);
		being.anim.SetTrigger("throw");
		being.PlayOnce(castSound);
		Debug.Log(bulletSpeed);
		being.spellObjList[1].StartCast();
		being.mov.MoveToRandom();
		yield return new WaitForSeconds(duration);
	}
}
