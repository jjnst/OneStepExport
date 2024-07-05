public class Weapon : Spell
{
	public float timeBetweenShots = 0.1f;

	public override void Reset()
	{
		itemType = ItemType.Wep;
	}
}
