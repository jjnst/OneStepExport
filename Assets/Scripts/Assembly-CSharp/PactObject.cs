using System;
using System.Globalization;
using System.Xml;
using I2.Loc;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class PactObject : ArtifactObject
{
	public Pact pact;

	public int duration;

	public int originalDuration;

	public string rewardID;

	public bool hellPass = false;

	public PactObject()
	{
	}

	public override void Deplete()
	{
		if ((bool)pact && (bool)pact.listCard)
		{
			pact.listCard.bgImage.color = Color.gray;
		}
		depleted = true;
	}

	public virtual PactObject ClonePact()
	{
		return new PactObject(this);
	}

	protected PactObject(PactObject other)
	{
		SetItemAtts(other);
		duration = other.duration;
		originalDuration = other.originalDuration;
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
		pactObj = this;
	}

	public PactObject SetPact(Being theBeing, PactData pactData, bool isHellPass = false)
	{
		being = theBeing;
		ctrl = being.ctrl;
		deCtrl = being.deCtrl;
		spCtrl = ctrl.sp;
		beingAnim = being.anim;
		deCtrl.activePacts.Add(pactObj);
		theBeing.pactObjs.Add(pactObj);
		if (pactData.loaded)
		{
			duration = pactData.duration;
			originalDuration = pactData.originalDuration;
			depleted = pactData.depleted;
			if (!string.IsNullOrEmpty(pactData.rewardID))
			{
				SetReward(ctrl.itemMan.pactDictionary[pactData.rewardID]);
			}
		}
		else
		{
			originalDuration = duration;
		}
		GameObject gameObject = new GameObject(base.itemID);
		gameObject.transform.position = being.transform.position;
		gameObject.transform.rotation = deCtrl.transform.rotation;
		gameObject.transform.SetParent(being.transform, true);
		Pact pact = gameObject.AddComponent<Pact>();
		pact.being = being;
		pact.pactObj = pactObj;
		pact.itemObj = pactObj;
		hellPass = isHellPass;
		this.pact = pact;
		item = pact;
		Trigger(FTrigger.OnEquip);
		return this;
	}

	public void FinishPact()
	{
		being.pactObjs.Remove(this);
		ctrl.deCtrl.activePacts.Remove(this);
		pact.listCard.Finish();
		deCtrl.statsScreen.UpdateStats(being.player);
	}

	public PactObject SetReward(PactObject rewardPact)
	{
		originalDuration = duration;
		efApps.AddRange(rewardPact.efApps);
		rewardID = rewardPact.itemID;
		return this;
	}

	public override void ReadXmlPrototype(XmlReader reader_parent)
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
				if (!LocalizationManager.TryGetTranslation("PactNames/" + base.itemID, out nameString))
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
			case "Duration":
				xmlReader.Read();
				if (!int.TryParse(xmlReader.ReadContentAsString(), out duration))
				{
					duration = 0;
				}
				break;
			case "Amount":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out amount))
				{
					amount = 0f;
				}
				break;
			}
		}
	}
}
