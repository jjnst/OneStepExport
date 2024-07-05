using System.Collections;
using UnityEngine;

public class IreliaMultiSlash : Spell
{
	public int dashDistance = 1;

	public int hitboxWidth = 1;

	public int hitboxHeight = 1;

	private BossIrelia bossIrelia;

	public override IEnumerator Cast()
	{
		bossIrelia = being.GetComponent<BossIrelia>();
		being.mov.SetState(State.Attacking);
		bossIrelia.dontHitAnim = true;
		bool zagging = false;
		if (bossIrelia.tier > 2)
		{
			hitboxHeight = 5;
		}
		for (int i = 0; i < 4; i++)
		{
			if (i == 0)
			{
				if (bossIrelia.tier > 0 && Utils.RandomBool(3))
				{
					zagging = true;
				}
				if (Utils.RandomBool(2))
				{
					being.mov.MoveTo(4, 2, true, false);
				}
				else
				{
					being.mov.MoveTo(4, 1, true, false);
				}
			}
			else
			{
				int yMov = 0;
				if (zagging)
				{
					yMov = ((being.mov.startTile.y < being.battleGrid.gridHeight / 2) ? 1 : (-1));
				}
				being.mov.Move(1, yMov, true, false);
			}
			while (being.mov.state == State.Moving)
			{
				yield return null;
			}
			if (i == 0)
			{
				yield return new WaitForSeconds(0.1f);
			}
			being.mov.SetState(State.Attacking);
			yield return new WaitForSeconds(0.03f);
			bossIrelia.swords[i].gameObject.SetActive(false);
			spellObj.P.CreateCustomShot(spellObj, 1, 0, true, duration, damage, 0f, null, castSound.name, hitboxWidth, hitboxHeight);
			spellObj.P.CreateCustomShot(spellObj, 0, 0, true, duration, damage);
			string animString = "slash" + i;
			being.anim.SetTrigger(animString);
			yield return new WaitForSeconds(0.16f - (float)bossIrelia.tier * 0.013f);
			bossIrelia.swords[i].gameObject.SetActive(true);
		}
		yield return new WaitForSeconds(0.2f);
		being.anim.SetTrigger("back");
		being.mov.MoveTo(being.mov.currentTile.x, being.mov.currentTile.y, false, false);
		while (being.mov.state == State.Moving)
		{
			yield return null;
		}
		being.mov.SetState(State.Attacking);
		yield return new WaitForSeconds(0.05f);
		bossIrelia.dontHitAnim = false;
		being.mov.SetState(State.Idle);
	}
}
