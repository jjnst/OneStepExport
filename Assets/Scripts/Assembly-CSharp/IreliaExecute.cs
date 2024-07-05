using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IreliaExecute : Spell
{
	public float timeBetweenSwords = 0.2f;

	public float fireDelay = 0.2f;

	public int hitboxWidth = 3;

	public int hitboxHeight = 3;

	public float slashWarningLength = 0.3f;

	public List<Vector3> swordPointAdds;

	private BossIrelia bossIrelia;

	public override IEnumerator Cast()
	{
		bossIrelia = being.GetComponent<BossIrelia>();
		for (int j = 0; j < 4; j++)
		{
			bossIrelia.swords[j].ResetDamping();
			bossIrelia.swords[j].target = bossIrelia.swordPoints[j].transform;
			bossIrelia.swordPoints[j].wavering = true;
		}
		being.anim.SetBool("raised", false);
		yield return new WaitForSeconds(0.7f);
		Tile playerTile = ctrl.currentPlayer.mov.currentTile;
		being.mov.MoveTo(playerTile.x + 1, playerTile.y, true, false);
		being.anim.SetBool("dashing", true);
		being.PlayOnce(castSound);
		while (being.mov.state == State.Moving)
		{
			yield return null;
		}
		being.anim.SetBool("dashing", false);
		being.mov.SetState(State.Attacking);
		beingAnim.SetTrigger("fire");
		spellObj.animWarning = "WarningDangerC";
		yield return new WaitForSeconds(0.6f);
		being.anim.SetBool("raised", true);
		List<Vector3> originalSwordAngles = new List<Vector3>();
		List<Projectile> newProjList = new List<Projectile>();
		for (int x2 = 0; x2 < 4; x2++)
		{
			bossIrelia.swords[x2].damping = 0.1f;
			bossIrelia.swordPoints[x2].wavering = false;
			bossIrelia.swordPoints[x2].transform.position += swordPointAdds[x2];
			originalSwordAngles.Add(bossIrelia.swordPoints[x2].transform.localEulerAngles);
			bossIrelia.swordPoints[x2].transform.right = ctrl.currentPlayer.mov.currentTile.transform.position - bossIrelia.swordPoints[x2].transform.position;
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.4f);
		yield return bossIrelia.StartCoroutine(bossIrelia._StartDialogue("Execution"));
		yield return new WaitForSeconds(0.2f);
		being.anim.SetTrigger("throw");
		for (int y2 = 0; y2 < 4; y2++)
		{
			Projectile newMark = spellObj.P.CreateCustomEffect(spellObj, ctrl.currentPlayer.mov.currentTile, 0.5f, null, false, 0f, null, 0, 1f, false, true);
			newMark.transform.rotation = bossIrelia.swordPoints[y2].transform.rotation;
			bossIrelia.swords[y2].target = newMark.transform;
			bossIrelia.swords[y2].ResetDamping();
		}
		yield return new WaitForSeconds(0.06f);
		for (int y = 0; y < 4; y++)
		{
			Projectile newAtk = spellObj.P.CreateCustomShot(spellObj, ctrl.currentPlayer.mov.currentTile, false, 0.1f, damage, 0f, null, castSound.name);
			newProjList.Add(newAtk);
			newAtk.destroyOnHit = true;
			yield return new WaitForSeconds(0.06f);
		}
		being.anim.SetBool("raised", false);
		bossIrelia.swordBox.localPosition = Vector3.zero;
		for (int x = 0; x < 4; x++)
		{
			bossIrelia.swordPoints[x].transform.localEulerAngles = originalSwordAngles[x];
		}
		yield return new WaitForSeconds(1f);
		for (int i = 0; i < 4; i++)
		{
			bossIrelia.swords[i].target = bossIrelia.swordPoints[i].transform;
			bossIrelia.swordPoints[i].wavering = true;
		}
		yield return new WaitForSeconds(0.5f);
		being.mov.MoveTo(being.mov.currentTile.x, being.mov.currentTile.y, false, false);
		while (being.mov.state == State.Moving)
		{
			yield return null;
		}
		being.mov.SetState(State.Attacking);
		yield return new WaitForSeconds(0.05f);
		being.anim.SetTrigger("idle");
		being.mov.SetState(State.Idle);
	}
}
