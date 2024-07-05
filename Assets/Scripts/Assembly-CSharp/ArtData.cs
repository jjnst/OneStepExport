using System;

[Serializable]
public struct ArtData
{
	public string itemID;

	public bool loaded;

	public bool depleted;

	public bool dead;

	public float currentValue;

	public float maxValue;

	public int atkDmg;

	public int defense;

	public int maxMana;

	public float manaRegen;

	public int spellPower;

	public float shuffleTime;

	public ArtData(ArtifactObject artObj)
	{
		itemID = artObj.itemID;
		loaded = true;
		depleted = artObj.depleted;
		dead = artObj.dead;
		currentValue = artObj.currentValue;
		maxValue = artObj.maxValue;
		atkDmg = artObj.atkDmg;
		defense = artObj.defense;
		maxMana = artObj.maxMana;
		manaRegen = artObj.manaRegen;
		spellPower = artObj.spellPower;
		shuffleTime = artObj.shuffleTime;
	}

	public ArtData(string itemID)
	{
		this.itemID = itemID;
		loaded = false;
		depleted = false;
		dead = false;
		currentValue = 0f;
		maxValue = 0f;
		atkDmg = 0;
		defense = 0;
		maxMana = 0;
		manaRegen = 0f;
		spellPower = 0;
		shuffleTime = 0f;
	}
}
