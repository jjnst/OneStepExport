using System.Collections;
using UnityEngine;

public class BreakerOrbitals : Spell
{
	public int distance = 1;

	private int endX;

	public IEnumerator Cast(float lerpTimeX, float duration)
	{
		being.mov.SetState(State.Attacking);
		being.anim.SetTrigger("fire");
		yield return new WaitForSeconds(castTime);
		int highestX = 0;
		int lowestX = being.battleGrid.gridLength - 1;
		for (int x = 0; x < being.battleGrid.gridLength; x++)
		{
			if (being.battleGrid.grid[x, 0].align == being.alignNum * -1)
			{
				if (x > highestX)
				{
					highestX = x;
				}
				if (x < lowestX)
				{
					lowestX = x;
				}
			}
		}
		spellObj.P.CreateCustomEffect(spellObj, being.battleGrid.grid[highestX, being.battleGrid.grid.GetLength(1) - 1], 0.5f, "WarningDangerC", false, 0f, "mark_01");
		spellObj.P.CreateCustomEffect(spellObj, being.battleGrid.grid[lowestX, 0], 0.5f, "WarningDangerC", false, 0f, "mark_01");
		yield return new WaitForSeconds(0.5f);
		Projectile orbitalTop = spellObj.P.CreateCustomLerper(spellObj, being.battleGrid.grid[highestX, being.battleGrid.grid.GetLength(1) - 1], MovPattern.CClockwiseOtherEdges, lerpTimeX, bulletSpeed, false, duration, damage, animShot.name, castSound.name);
		orbitalTop.mov.direction = Direction.Left;
		orbitalTop.mov.hovering = false;
		Projectile orbitalBot = spellObj.P.CreateCustomLerper(spellObj, being.battleGrid.grid[lowestX, 0], MovPattern.CClockwiseOtherEdges, lerpTimeX, bulletSpeed, false, duration, damage, animShot.name, castSound.name);
		orbitalBot.mov.direction = Direction.Right;
		orbitalBot.mov.hovering = false;
		being.mov.SetState(State.Idle);
	}
}
