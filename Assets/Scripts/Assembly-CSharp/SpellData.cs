using System;
using System.Collections.Generic;

[Serializable]
public struct SpellData
{
	public string itemID;

	public float damage;

	public int permDamage;

	public List<Enhancement> enhancements;

	public SpellData(SpellObject spellObj)
	{
		itemID = spellObj.itemID;
		damage = spellObj.damage;
		permDamage = spellObj.permDamage;
		enhancements = new List<Enhancement>(spellObj.enhancements);
	}

	public SpellData(string itemID)
	{
		this.itemID = itemID;
		damage = 0f;
		permDamage = 0;
		enhancements = new List<Enhancement>();
	}
}
