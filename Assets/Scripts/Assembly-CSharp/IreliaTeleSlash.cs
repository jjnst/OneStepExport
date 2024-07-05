using System.Collections;
using UnityEngine;

public class IreliaTeleSlash : Spell
{
	public int hitboxWidth = 1;

	public int hitboxHeight = 1;

	private BossIrelia bossIrelia;

	public override IEnumerator Cast()
	{
		being.dontInterruptAnim = true;
		bossIrelia = being.GetComponent<BossIrelia>();
		being.mov.SetState(State.Attacking);
		if (bossIrelia.tier >= 2)
		{
			hitboxHeight = 5;
		}
		for (int i = 0; i < 4; i++)
		{
			Tile playerTile = ctrl.currentPlayer.mov.currentTile;
			being.mov.MoveTo(playerTile.x + 1, playerTile.y, true, false);
			being.anim.SetBool("dashing", true);
			bossIrelia.PlayOnce(castSound);
			while (being.mov.state == State.Moving)
			{
				yield return null;
			}
			being.anim.SetBool("dashing", false);
			being.mov.SetState(State.Attacking);
			yield return new WaitForSeconds(warningLength - (float)bossIrelia.tier * 0.025f);
			bossIrelia.swords[i].gameObject.SetActive(false);
			spellObj.P.CreateCustomShot(spellObj, 1, 0, true, duration, damage, 0f, null, castSound.name, hitboxWidth, hitboxHeight);
			spellObj.P.CreateCustomShot(spellObj, 0, 0, true, duration, damage);
			string animString = "slash" + i;
			being.anim.SetTrigger(animString);
			yield return new WaitForSeconds(duration - (float)bossIrelia.tier * 0.025f);
			bossIrelia.swords[i].gameObject.SetActive(true);
		}
		yield return new WaitForSeconds(0.2f);
		being.dontInterruptAnim = false;
		being.mov.MoveTo(being.mov.currentTile.x, being.mov.currentTile.y, false, false);
		while (being.mov.state == State.Moving)
		{
			yield return null;
		}
		being.mov.SetState(State.Attacking);
		yield return new WaitForSeconds(0.05f);
		being.anim.SetTrigger("idle");
		being.mov.SetState(State.Idle);
		yield return new WaitForSeconds(0.5f);
	}
}
