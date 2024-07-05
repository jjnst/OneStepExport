using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IreliaRainSlash : Spell
{
	public float timeBetweenSwords = 0.2f;

	public float fireDelay = 0.2f;

	public int hitboxWidth = 3;

	public int hitboxHeight = 3;

	public float slashWarningLength = 0.3f;

	public List<Vector3> swordPointAdds;

	private BossIrelia bossIrelia;

	protected override void Awake()
	{
		base.Awake();
		if (S.I.scene == GScene.DemoLive)
		{
			warningLength = 0.5f;
			slashWarningLength = 0.3f;
		}
	}

	public override IEnumerator Cast()
	{
		bossIrelia = being.GetComponent<BossIrelia>();
		bossIrelia.dontHitAnim = true;
		being.mov.SetState(State.Attacking);
		spellObj.animWarning = "WarningDangerC";
		yield return new WaitForSeconds(0.3f);
		List<Projectile> swordList = new List<Projectile>();
		List<Tile> tileList = new List<Tile>
		{
			being.battleGrid.Get(new TileApp(Location.PlayerBotLeft), 1, being)[0],
			being.battleGrid.Get(new TileApp(Location.PlayerBotRight), 1, being)[0],
			being.battleGrid.Get(new TileApp(Location.PlayerTopLeft), 1, being)[0],
			being.battleGrid.Get(new TileApp(Location.PlayerTopRight), 1, being)[0]
		};
		List<int> randList = Utils.RandomList(4);
		bossIrelia.swordBox.position = new Vector3(70f, 50f, 0f);
		List<Vector3> originalSwordAngles = new List<Vector3>();
		List<Projectile> newProjList = new List<Projectile>();
		for (int x2 = 0; x2 < 4; x2++)
		{
			bossIrelia.swords[x2].damping = 0.1f;
			bossIrelia.swordPoints[x2].wavering = false;
			bossIrelia.swordPoints[x2].transform.position += swordPointAdds[x2];
			originalSwordAngles.Add(bossIrelia.swordPoints[x2].transform.localEulerAngles);
			bossIrelia.swordPoints[x2].transform.right = tileList[randList[x2]].transform.position - bossIrelia.swordPoints[x2].transform.position;
		}
		being.anim.SetBool("raised", true);
		yield return new WaitForSeconds(0.3f);
		for (int y = 0; y < 4; y++)
		{
			spellObj.P.CreateWarning(spellObj, tileList[randList[y]], warningLength);
			yield return new WaitForSeconds(warningLength);
			being.anim.SetTrigger("throw");
			Projectile newAtk = spellObj.P.CreateCustomShot(spellObj, tileList[randList[y]], false, 3f, damage, 0f, null, castSound.name);
			newProjList.Add(newAtk);
			newAtk.destroyOnHit = false;
			swordList.Add(newAtk);
			Projectile newMark = spellObj.P.CreateCustomEffect(spellObj, tileList[randList[y]], 3f, null, false, 0f, null, 0, 1f, false, true);
			newMark.transform.rotation = bossIrelia.swordPoints[y].transform.rotation;
			bossIrelia.swords[y].target = newMark.transform;
			bossIrelia.swords[y].ResetDamping();
			yield return new WaitForSeconds(timeBetweenSwords);
		}
		being.anim.SetBool("raised", false);
		bossIrelia.swordBox.localPosition = Vector3.zero;
		for (int x = 0; x < 4; x++)
		{
			bossIrelia.swordPoints[x].transform.localEulerAngles = originalSwordAngles[x];
		}
		yield return new WaitForSeconds(fireDelay);
		being.PlayOnce(shotSound);
		for (int i = 0; i < 4; i++)
		{
			Tile atkTile = tileList[randList[i]];
			being.mov.MoveTo(atkTile.x, atkTile.y, true, false);
			being.anim.SetBool("dashing", true);
			being.PlayOnce(castSound);
			while (being.mov.state == State.Moving)
			{
				yield return null;
			}
			being.anim.SetBool("dashing", false);
			bossIrelia.swords[i].target = bossIrelia.swordPoints[i].transform;
			bossIrelia.swordPoints[i].wavering = true;
			newProjList[i].Despawn();
			being.mov.SetState(State.Attacking);
			yield return new WaitForSeconds(slashWarningLength - 0.011f * (float)bossIrelia.tier);
			bossIrelia.swords[i].gameObject.SetActive(false);
			spellObj.P.CreateCustomShot(spellObj, 0, 0, true, duration, damage, 0f, "Slash3x3SpinBlue", castSound.name, hitboxWidth, hitboxHeight);
			string animString = "slash" + i;
			being.anim.SetTrigger(animString);
			yield return new WaitForSeconds(duration - 0.011f * (float)bossIrelia.tier);
			bossIrelia.swords[i].gameObject.SetActive(true);
		}
		yield return new WaitForSeconds(0.3f - 0.011f * (float)bossIrelia.tier);
		being.mov.MoveTo(being.mov.currentTile.x, being.mov.currentTile.y, false, false);
		while (being.mov.state == State.Moving)
		{
			yield return null;
		}
		being.mov.SetState(State.Attacking);
		yield return new WaitForSeconds(0.05f);
		being.anim.SetTrigger("idle");
		being.mov.SetState(State.Idle);
		bossIrelia.dontHitAnim = false;
		yield return new WaitForSeconds(0.3f - 0.015f * (float)bossIrelia.tier);
	}
}
