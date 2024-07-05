using System;
using System.Globalization;
using System.Xml;
using I2.Loc;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class ArtifactObject : ItemObject
{
	public float amount = 0f;

	public int atkDmg = 0;

	public int spellPower = 0;

	public int defense = 0;

	public int maxHP = 0;

	public int maxMana = 0;

	public int startingManaChange = 0;

	public float manaRegen = 0f;

	public float invulLength = 0f;

	public float shuffleTime = 0f;

	public float dropMultiplierAdd = 0f;

	public int lootDropsAdd = 0;

	public float chestSpawnRateAdd = 0f;

	public float healSpawnRateAdd = 0f;

	public bool depleted = false;

	public bool dead = false;

	public float currentValue = 0f;

	public float maxValue = 0f;

	public Artifact art;

	public string spell;

	public ArtifactObject()
	{
	}

	public virtual ArtifactObject Clone()
	{
		return new ArtifactObject(this);
	}

	public virtual void Deplete()
	{
		if ((bool)art && (bool)art.listCard)
		{
			art.listCard.bgImage.color = Color.gray;
		}
		depleted = true;
	}

	public virtual void Replete()
	{
		if ((bool)art && (bool)art.listCard)
		{
			art.listCard.bgImage.color = Color.white;
		}
		depleted = false;
	}

	protected ArtifactObject(ArtifactObject other)
	{
		SetItemAtts(other);
		amount = other.amount;
		maxHP = other.maxHP;
		maxMana = other.maxMana;
		startingManaChange = other.startingManaChange;
		manaRegen = other.manaRegen;
		atkDmg = other.atkDmg;
		spellPower = other.spellPower;
		defense = other.defense;
		invulLength = other.invulLength;
		shuffleTime = other.shuffleTime;
		dropMultiplierAdd = other.dropMultiplierAdd;
		lootDropsAdd = other.lootDropsAdd;
		spell = other.spell;
		artObj = other.artObj;
	}

	public void Reset()
	{
		Debug.LogError("were resttgin");
		amount = 0f;
		maxHP = 0;
		maxMana = 0;
		startingManaChange = 0;
		manaRegen = 0f;
		atkDmg = 0;
		spellPower = 0;
		defense = 0;
		invulLength = 0f;
		shuffleTime = 0f;
		dropMultiplierAdd = 0f;
		lootDropsAdd = 0;
		spell = null;
	}

	public ArtifactObject Set(Being theBeing, ArtData artData)
	{
		being = theBeing;
		ctrl = being.ctrl;
		deCtrl = being.deCtrl;
		spCtrl = ctrl.sp;
		beingAnim = being.anim;
		artObj = this;
		being.artObjs.Add(artObj);
		GameObject gameObject = new GameObject(base.itemID);
		gameObject.transform.position = being.transform.position;
		gameObject.transform.rotation = deCtrl.transform.rotation;
		gameObject.transform.SetParent(being.transform, true);
		Artifact artifact = gameObject.AddComponent<Artifact>();
		artifact.being = being;
		artifact.artObj = artObj;
		artifact.itemObj = artObj;
		art = artifact;
		item = artifact;
		dead = artData.dead;
		currentValue = artData.currentValue;
		maxValue = artData.maxValue;
		if (artData.depleted)
		{
			Deplete();
		}
		Trigger(FTrigger.OnEquip);
		if (!artData.loaded)
		{
			artData = new ArtData(this);
		}
		atkDmg = artData.atkDmg;
		defense = artData.defense;
		maxMana = artData.maxMana;
		manaRegen = artData.manaRegen;
		spellPower = artData.spellPower;
		shuffleTime = artData.shuffleTime;
		return this;
	}

	public virtual void ReadXmlPrototype(XmlReader reader_parent)
	{
		base.itemID = reader_parent.GetAttribute("itemID");
		XmlReader xmlReader = reader_parent.ReadSubtree();
		while (xmlReader.Read())
		{
			if (xmlReader.IsEmptyElement)
			{
				continue;
			}
			switch (xmlReader.Name)
			{
			case "Name":
				xmlReader.Read();
				if (!LocalizationManager.TryGetTranslation("ArtNames/" + base.itemID, out nameString))
				{
					nameString = xmlReader.ReadContentAsString();
				}
				break;
			case "Tags":
			{
				xmlReader.Read();
				tagStrings = xmlReader.ReadContentAsString().Replace(" ", string.Empty).Split(',');
				if (string.IsNullOrEmpty(tagStrings[0]))
				{
					break;
				}
				string[] array = tagStrings;
				foreach (string text in array)
				{
					if (Enum.IsDefined(typeof(Tag), text))
					{
						tags.Add((Tag)Enum.Parse(typeof(Tag), text));
					}
					else
					{
						Debug.LogError("Invalid Tag: " + text + " for " + nameString);
					}
				}
				break;
			}
			case "App":
				EffectApp.AddTo(xmlReader, this, efApps, xmlReader.GetAttribute("trigger"), xmlReader.GetAttribute("effect"));
				break;
			case "Description":
				if (S.modsInstalled)
				{
					xmlReader.Read();
					description = xmlReader.ReadContentAsString();
				}
				break;
			case "Flavor":
				if (S.modsInstalled)
				{
					xmlReader.Read();
					flavor = xmlReader.ReadContentAsString();
				}
				break;
			case "Rarity":
				xmlReader.Read();
				rarity = xmlReader.ReadContentAsInt();
				break;
			case "Brand":
			{
				xmlReader.Read();
				string value = xmlReader.ReadContentAsString();
				if (Enum.IsDefined(typeof(Brand), value))
				{
					brand = (Brand)Enum.Parse(typeof(Brand), value);
				}
				break;
			}
			case "Amount":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out amount))
				{
					amount = 0f;
				}
				break;
			case "Params":
				ReadXmlParams(xmlReader);
				break;
			}
		}
	}
}
