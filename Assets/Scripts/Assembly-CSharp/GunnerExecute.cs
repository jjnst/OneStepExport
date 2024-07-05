using System.Collections;
using UnityEngine;

public class GunnerExecute : Spell
{
	public float timeBetweenSwords = 0.2f;

	public float fireDelay = 0.2f;

	public int hitboxWidth = 3;

	public int hitboxHeight = 3;

	private BossGunner bossGunner;

	public override IEnumerator Cast()
	{
		bossGunner = being.GetComponent<BossGunner>();
		yield return new WaitForSeconds(0.7f);
		being.anim.SetBool("dashing", true);
		being.PlayOnce(castSound);
		while (being.mov.state == State.Moving)
		{
			yield return null;
		}
		being.anim.SetBool("dashing", false);
		being.mov.SetState(State.Attacking);
		spellObj.animWarning = "WarningDangerC";
		if ((bool)ctrl.currentPlayer)
		{
			bossGunner.savedTileList = being.battleGrid.Get(new TileApp(Location.Front), 1, ctrl.currentPlayer);
		}
		being.anim.SetTrigger("throw");
		bossGunner.spellObjList[7].StartCast();
		yield return new WaitForSeconds(0.9f);
		being.anim.SetTrigger("throw");
		bossGunner.spellObjList[8].StartCast();
		yield return new WaitForSeconds(0.4f);
		yield return bossGunner.StartCoroutine(bossGunner._StartDialogue("Execution"));
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
		yield return new WaitForSeconds(3.3f);
	}
}
