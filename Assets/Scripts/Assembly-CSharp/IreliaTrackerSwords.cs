using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IreliaTrackerSwords : Spell
{
	public List<Projectile> trackers;

	public float timeBetweenSwords;

	public float timeBetweenAttacks;

	private BossIrelia bossIrelia;

	public List<Projectile> shots;

	public AudioClip dashSound;

	protected override void Awake()
	{
		base.Awake();
		if (S.I.scene == GScene.DemoLive)
		{
			warningLength = 0.43f;
			timeBetweenSwords = 0.26f;
			timeBetweenAttacks = 0.11f;
		}
	}

	private IEnumerator Trackers()
	{
		while ((bool)ctrl.currentPlayer)
		{
			if ((bool)trackers[0])
			{
				trackers[0].transform.position = new Vector3(ctrl.currentPlayer.transform.position.x, trackers[0].transform.position.y, trackers[0].transform.position.z);
			}
			if ((bool)trackers[1])
			{
				trackers[1].transform.position = new Vector3(trackers[1].transform.position.x, ctrl.currentPlayer.transform.position.y, trackers[1].transform.position.z);
			}
			if ((bool)trackers[2])
			{
				trackers[2].transform.position = new Vector3(ctrl.currentPlayer.transform.position.x, trackers[2].transform.position.y, trackers[2].transform.position.z);
			}
			if ((bool)trackers[3])
			{
				trackers[3].transform.position = new Vector3(trackers[3].transform.position.x, ctrl.currentPlayer.transform.position.y, trackers[3].transform.position.z);
			}
			yield return null;
		}
	}

	public override IEnumerator Cast()
	{
		bossIrelia = being.GetComponent<BossIrelia>();
		being.dontHitAnim = true;
		being.mov.SetState(State.Attacking);
		if (bossIrelia.tier > 2)
		{
			warningLength = 0.4f;
		}
		if (bossIrelia.tier > 3)
		{
			warningLength = 0.34f;
		}
		yield return new WaitForSeconds(0.3f);
		List<int> randList = Utils.RandomList(4);
		StartCoroutine(Trackers());
		being.anim.SetBool("raised", true);
		for (int x = 0; x < 4; x++)
		{
			if (randList[x] == 0)
			{
				trackers[randList[x]] = spellObj.P.CreateLerperEffect(spellObj, being.battleGrid.grid[ctrl.currentPlayer.mov.currentTile.x, 0], MovPattern.None, 0f, 0f, false, duration, null, castSound.name);
				trackers[randList[x]].transform.localEulerAngles = Vector3.forward * 90f;
			}
			else if (randList[x] == 1)
			{
				trackers[randList[x]] = spellObj.P.CreateLerperEffect(spellObj, being.battleGrid.grid[0, ctrl.currentPlayer.mov.currentTile.y], MovPattern.None, 0f, 0f, false, duration, null, castSound.name);
				trackers[randList[x]].transform.localEulerAngles = Vector3.zero;
			}
			else if (randList[x] == 2)
			{
				trackers[randList[x]] = spellObj.P.CreateLerperEffect(spellObj, being.battleGrid.grid[ctrl.currentPlayer.mov.currentTile.x, 3], MovPattern.None, 0f, 0f, false, duration, null, castSound.name);
				trackers[randList[x]].transform.localEulerAngles = Vector3.back * 90f;
			}
			else
			{
				trackers[randList[x]] = spellObj.P.CreateLerperEffect(spellObj, being.battleGrid.grid[3, ctrl.currentPlayer.mov.currentTile.y], MovPattern.None, 0f, 0f, false, duration, null, castSound.name);
				trackers[randList[x]].transform.localEulerAngles = Vector3.forward * 180f;
			}
			bossIrelia.swords[randList[x]].target = trackers[randList[x]].transform;
			yield return new WaitForSeconds(timeBetweenSwords);
		}
		yield return new WaitForSeconds(0.8f - 0.02f * (float)bossIrelia.tier);
		shots.Clear();
		for (int j = 0; j < 4; j++)
		{
			int xPos = trackers[randList[j]].mov.currentTile.x;
			int yPos = trackers[randList[j]].mov.currentTile.y;
			if (randList[j] % 2 == 0)
			{
				xPos = ctrl.currentPlayer.mov.currentTile.x;
			}
			else
			{
				yPos = ctrl.currentPlayer.mov.currentTile.y;
			}
			spellObj.P.CreateCustomEffect(spellObj, xPos, yPos, 0.3f, animMarkS, false, 0f, castSound.name);
			Projectile newSword = spellObj.P.CreateCustomShot(spellObj, xPos, yPos, false, duration, damage, 0f, null, shotSound.name);
			SimplePool.Despawn(trackers[randList[j]].gameObject);
			trackers[randList[j]] = null;
			bossIrelia.swords[randList[j]].target = newSword.transform;
			if (randList[j] == 0)
			{
				newSword.transform.localEulerAngles = Vector3.forward * 90f;
			}
			else if (randList[j] == 1)
			{
				newSword.transform.localEulerAngles = Vector3.zero;
			}
			else if (randList[j] == 2)
			{
				newSword.transform.localEulerAngles = Vector3.back * 90f;
			}
			else
			{
				newSword.transform.localEulerAngles = Vector3.forward * 180f;
			}
			shots.Add(newSword);
			yield return new WaitForSeconds(warningLength - 0.01f * (float)bossIrelia.tier);
			being.anim.SetTrigger("throw");
			float speedMult = 1f;
			if (randList[j] % 2 == 0)
			{
				speedMult = 0.62f;
			}
			S.I.PlayOnce(shotSound);
			bossIrelia.swords[j].damping = 0f;
			newSword.rBody.velocity = base.transform.TransformDirection(GetDir(randList[j]) * bulletSpeed * speedMult);
			yield return new WaitForSeconds(timeBetweenAttacks - 0.008f * (float)bossIrelia.tier);
			StartCoroutine(StopAfter(newSword));
		}
		yield return new WaitForSeconds(0.4f);
		if (bossIrelia.tier > 2)
		{
			for (int i = 0; i < 4; i++)
			{
				yield return new WaitForSeconds(warningLength - 0.01f * (float)bossIrelia.tier);
				S.I.PlayOnce(shotSound);
				being.anim.SetTrigger("throw");
				float speedMult2 = 1f;
				if (randList[i] % 2 == 0)
				{
					speedMult2 = 0.62f;
				}
				shots[i].rBody.velocity = base.transform.TransformDirection(-GetDir(randList[i]) * bulletSpeed * speedMult2);
				yield return new WaitForSeconds(timeBetweenAttacks - 0.008f * (float)bossIrelia.tier);
			}
		}
		StopCoroutine(Trackers());
		yield return new WaitForSeconds(0.5f);
		being.anim.SetBool("raised", false);
		for (int r = 0; r < 4; r++)
		{
			bossIrelia.swords[r].ResetDamping();
			bossIrelia.swords[r].target = bossIrelia.swordPoints[r].transform;
		}
		being.dontHitAnim = false;
		being.mov.SetState(State.Idle);
	}

	private IEnumerator StopAfter(Projectile proj)
	{
		yield return new WaitForSeconds(0.35f);
		proj.rBody.velocity = Vector3.zero;
	}
}
