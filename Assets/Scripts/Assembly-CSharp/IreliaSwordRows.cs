using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IreliaSwordRows : Spell
{
	public float timeBetweenSwords = 0f;

	public float fireDelay = 0.2f;

	public float lastDelay = 0.7f;

	private BossIrelia bossIrelia;

	protected override void Awake()
	{
		base.Awake();
		if (S.I.scene == GScene.DemoLive)
		{
			fireDelay = 0.4f;
			lastDelay = 0.45f;
		}
	}

	public override IEnumerator Cast()
	{
		bossIrelia = being.GetComponent<BossIrelia>();
		being.dontHitAnim = true;
		being.mov.SetState(State.Attacking);
		being.anim.SetBool("raised", true);
		yield return new WaitForSeconds(0.3f);
		List<int> rowList = new List<int>();
		List<Projectile> swordList = new List<Projectile>();
		int colNum = being.mov.currentTile.x + 1;
		if (being.mov.currentTile.x == 7)
		{
			colNum = 6;
		}
		for (int i = 0; i < 4; i++)
		{
			rowList.Add(i);
		}
		for (int y = 0; y < 4; y++)
		{
			int yRow = rowList[Random.Range(0, rowList.Count)];
			if (y == 3)
			{
				yield return new WaitForSeconds(lastDelay - (float)bossIrelia.tier * 0.012f);
			}
			Projectile newAtk = spellObj.P.CreateCustomShot(spellObj, colNum, yRow, false, duration, damage, 0f, null, castSound.name);
			if (bossIrelia.tier > 1)
			{
				newAtk.destroyOnHit = false;
			}
			else
			{
				newAtk.destroyOnHit = true;
			}
			newAtk.SetToGunPoint();
			swordList.Add(newAtk);
			rowList.Remove(yRow);
			bossIrelia.swords[y].target = newAtk.transform;
			yield return new WaitForSeconds(timeBetweenSwords);
		}
		yield return new WaitForSeconds(fireDelay - (float)bossIrelia.tier * 0.021f);
		being.audioSource.PlayOneShot(shotSound);
		for (int x2 = 0; x2 < 4; x2++)
		{
			if (x2 == 3)
			{
				yield return new WaitForSeconds(lastDelay - (float)bossIrelia.tier * 0.021f);
				being.audioSource.PlayOneShot(shotSound);
			}
			if ((bool)swordList[x2])
			{
				being.anim.SetTrigger("throw");
				bossIrelia.swords[x2].damping = 0f;
				swordList[x2].rBody.velocity = base.transform.TransformDirection(Vector3.right * bulletSpeed);
			}
		}
		if (bossIrelia.tier > 1)
		{
			yield return new WaitForSeconds(0.35f);
			being.audioSource.PlayOneShot(shotSound);
			for (int x = 0; x < 4; x++)
			{
				if (x == 3)
				{
					yield return new WaitForSeconds(lastDelay - (float)bossIrelia.tier * 0.021f);
					being.audioSource.PlayOneShot(shotSound);
				}
				if ((bool)swordList[x])
				{
					being.anim.SetTrigger("throw");
					bossIrelia.swords[x].damping = 0f;
					swordList[x].rBody.velocity = base.transform.TransformDirection(-Vector3.right * bulletSpeed);
				}
			}
		}
		yield return new WaitForSeconds(0.9f - (float)bossIrelia.tier * 0.05f);
		being.anim.SetBool("raised", false);
		for (int r = 0; r < 4; r++)
		{
			bossIrelia.swords[r].ResetDamping();
			bossIrelia.swords[r].target = bossIrelia.swordPoints[r].transform;
		}
		being.dontHitAnim = false;
		being.mov.SetState(State.Idle);
	}
}
