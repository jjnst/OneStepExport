using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using I2.Loc;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class BeingObject
{
	public string nameString;

	public string localizedName;

	private string[] tagStrings = new string[0];

	public List<Tag> tags = new List<Tag>();

	public BeingType type = BeingType.None;

	public int health = 99999999;

	public float lerpTime = 0.1f;

	public int experience = 0;

	public int money = 0;

	public MovPattern movementPattern;

	public int movements;

	public float movementDelay = 0.5f;

	public float startDelay = 1f;

	public float baseLoopDelay = 5f;

	public float loopDelay = 5f;

	public int defense = 0;

	public string animName = "";

	public string animBase = "";

	public bool scaleHP = true;

	public bool shadow = true;

	public bool overrideMovement = true;

	public bool unmoveable = false;

	public bool petAnim = false;

	public bool tauntAnim = false;

	public List<string> allAnims = new List<string>();

	public List<string> unlockedAnims = new List<string>();

	public bool stagger = false;

	public bool mustDestroy;

	public float deathDelay;

	public float clearDelay;

	public string dieSound = "";

	public string vocSynth = "";

	public int lethality;

	public string description;

	public string flavor;

	public string title;

	public string weapon;

	public int maxHealthBase;

	public int maxHealthCurrent;

	public int maxMana;

	public float manaRegen;

	public float basicCooldown;

	public float shuffleTime;

	public float gracePeriod;

	public string splashSprite;

	public int spellPower;

	public List<ArtData> artifacts = new List<ArtData>();

	public List<Brand> startingBrands = new List<Brand>();

	public bool chargeAnim;

	public bool timeoutAnim;

	public bool clearAnim;

	public List<string> startups = new List<string>();

	public List<string> deck = new List<string>();

	public List<string> rewardList = new List<string>();

	public List<string> timeouts = new List<string>();

	public List<string> deathrattles = new List<string>();

	public List<string> clearSpells = new List<string>();

	public List<EffectApp> efApps = new List<EffectApp>();

	private Action<BeingObject> cbOnChanged;

	public string beingID { get; protected set; }

	public Vector3 localGunPointPos { get; protected set; }

	public Vector3 localTimerPos { get; protected set; }

	public BeingObject()
	{
	}

	protected BeingObject(BeingObject other)
	{
		beingID = other.beingID;
		nameString = other.nameString;
		localizedName = other.localizedName;
		tags = other.tags;
		type = other.type;
		health = other.health;
		lerpTime = other.lerpTime;
		experience = other.experience;
		money = other.money;
		movementPattern = other.movementPattern;
		movements = other.movements;
		movementDelay = other.movementDelay;
		startDelay = other.startDelay;
		baseLoopDelay = other.baseLoopDelay;
		loopDelay = other.loopDelay;
		localGunPointPos = other.localGunPointPos;
		localTimerPos = other.localTimerPos;
		defense = other.defense;
		animName = other.animName;
		animBase = other.animBase;
		scaleHP = other.scaleHP;
		shadow = other.shadow;
		overrideMovement = other.overrideMovement;
		unmoveable = other.unmoveable;
		petAnim = other.petAnim;
		tauntAnim = other.tauntAnim;
		allAnims = new List<string>(other.allAnims);
		unlockedAnims = new List<string>(other.unlockedAnims);
		chargeAnim = other.chargeAnim;
		clearAnim = other.clearAnim;
		timeoutAnim = other.timeoutAnim;
		stagger = other.stagger;
		mustDestroy = other.mustDestroy;
		deathDelay = other.deathDelay;
		clearDelay = other.clearDelay;
		dieSound = other.dieSound;
		vocSynth = other.vocSynth;
		lethality = other.lethality;
		description = other.description;
		flavor = other.flavor;
		title = other.title;
		weapon = other.weapon;
		maxHealthBase = other.maxHealthBase;
		maxHealthCurrent = other.maxHealthCurrent;
		maxMana = other.maxMana;
		manaRegen = other.manaRegen;
		basicCooldown = other.basicCooldown;
		shuffleTime = other.shuffleTime;
		gracePeriod = other.gracePeriod;
		spellPower = other.spellPower;
		splashSprite = other.splashSprite;
		artifacts = new List<ArtData>(other.artifacts);
		startingBrands = new List<Brand>(other.startingBrands);
		startups = new List<string>(other.startups);
		deck = new List<string>(other.deck);
		deathrattles = new List<string>(other.deathrattles);
		clearSpells = new List<string>(other.clearSpells);
		efApps = new List<EffectApp>(other.efApps);
		timeouts = new List<string>(other.timeouts);
		rewardList = new List<string>(other.rewardList);
	}

	public virtual BeingObject Clone()
	{
		return new BeingObject(this);
	}

	public void RegisterOnChangedCallback(Action<BeingObject> callbackFunc)
	{
		cbOnChanged = (Action<BeingObject>)Delegate.Combine(cbOnChanged, callbackFunc);
	}

	public void UnregisterOnChangedCallback(Action<BeingObject> callbackFunc)
	{
		cbOnChanged = (Action<BeingObject>)Delegate.Remove(cbOnChanged, callbackFunc);
	}

	public void ReadXmlPrototype(XmlReader reader_parent)
	{
		beingID = reader_parent.GetAttribute("beingID");
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
				nameString = xmlReader.ReadContentAsString();
				if (!LocalizationManager.TryGetTranslation("BeingNames/" + nameString, out localizedName) && !LocalizationManager.TryGetTranslation("BeingNames/" + beingID, out localizedName))
				{
					localizedName = nameString;
				}
				break;
			case "Tags":
			{
				xmlReader.Read();
				tagStrings = xmlReader.ReadContentAsString().Replace(" ", string.Empty).Split(',');
				string[] array = tagStrings;
				foreach (string text4 in array)
				{
					if (!string.IsNullOrEmpty(text4))
					{
						if (Enum.IsDefined(typeof(Tag), text4))
						{
							tags.Add((Tag)Enum.Parse(typeof(Tag), text4));
						}
						else
						{
							Debug.LogWarning("Invalid Tag: " + text4 + " for " + nameString);
						}
					}
				}
				break;
			}
			case "Description":
				xmlReader.Read();
				if (!LocalizationManager.TryGetTranslation("HeroDescriptions/" + beingID, out description))
				{
					description = xmlReader.ReadContentAsString();
				}
				break;
			case "Flavor":
				xmlReader.Read();
				if (!LocalizationManager.TryGetTranslation("HeroFlavors/" + beingID, out flavor))
				{
					flavor = xmlReader.ReadContentAsString();
				}
				break;
			case "Title":
				xmlReader.Read();
				if (!LocalizationManager.TryGetTranslation("HeroTitles/" + beingID, out title))
				{
					title = xmlReader.ReadContentAsString();
				}
				break;
			case "Weapon":
				xmlReader.Read();
				weapon = xmlReader.ReadContentAsString();
				break;
			case "Health":
				xmlReader.Read();
				health = xmlReader.ReadContentAsInt();
				break;
			case "MaxHealth":
				xmlReader.Read();
				maxHealthBase = xmlReader.ReadContentAsInt();
				break;
			case "Experience":
				xmlReader.Read();
				if (!int.TryParse(xmlReader.ReadContentAsString(), out experience))
				{
					experience = 0;
				}
				break;
			case "Money":
				xmlReader.Read();
				if (!int.TryParse(xmlReader.ReadContentAsString(), out money))
				{
					money = 0;
				}
				break;
			case "MaxMana":
				xmlReader.Read();
				if (!int.TryParse(xmlReader.ReadContentAsString(), out maxMana))
				{
					maxMana = 0;
				}
				break;
			case "BasicCooldown":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out basicCooldown))
				{
					basicCooldown = 0f;
				}
				break;
			case "ManaRegen":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out manaRegen))
				{
					manaRegen = 0f;
				}
				break;
			case "ShuffleTime":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out shuffleTime))
				{
					shuffleTime = 0f;
				}
				break;
			case "GracePeriod":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out gracePeriod))
				{
					gracePeriod = 0f;
				}
				break;
			case "SpellPower":
				xmlReader.Read();
				if (!int.TryParse(xmlReader.ReadContentAsString(), out spellPower))
				{
					spellPower = 0;
				}
				break;
			case "MovPattern":
			{
				xmlReader.Read();
				string text5 = "";
				text5 = xmlReader.ReadContentAsString();
				if (text5 != "")
				{
					movementPattern = (MovPattern)Enum.Parse(typeof(MovPattern), text5);
				}
				break;
			}
			case "Movements":
				xmlReader.Read();
				if (!int.TryParse(xmlReader.ReadContentAsString(), out movements))
				{
					movements = 0;
				}
				break;
			case "MovementDelay":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out movementDelay))
				{
					movementDelay = 0f;
				}
				break;
			case "StartDelay":
				xmlReader.Read();
				startDelay = float.Parse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture);
				break;
			case "LoopDelay":
				xmlReader.Read();
				loopDelay = float.Parse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture);
				baseLoopDelay = loopDelay;
				break;
			case "Defense":
				xmlReader.Read();
				if (!int.TryParse(xmlReader.ReadContentAsString(), out defense))
				{
					defense = 0;
				}
				break;
			case "LerpTime":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out lerpTime))
				{
					lerpTime = 0.1f;
				}
				break;
			case "Startup":
			{
				xmlReader.Read();
				string text3 = xmlReader.ReadContentAsString();
				if (text3 != "")
				{
					startups.Add(text3);
				}
				break;
			}
			case "Deck":
			{
				xmlReader.Read();
				string text2 = xmlReader.ReadContentAsString();
				if (!string.IsNullOrEmpty(text2))
				{
					deck.Add(text2);
				}
				break;
			}
			case "Artifacts":
			{
				xmlReader.Read();
				string text = xmlReader.ReadContentAsString();
				if (!string.IsNullOrEmpty(text))
				{
					artifacts.Add(new ArtData(text));
				}
				break;
			}
			case "Timeout":
				xmlReader.Read();
				timeouts.Add(xmlReader.ReadContentAsString());
				break;
			case "Deathrattle":
				xmlReader.Read();
				deathrattles.Add(xmlReader.ReadContentAsString());
				break;
			case "ClearSpell":
				xmlReader.Read();
				clearSpells.Add(xmlReader.ReadContentAsString());
				break;
			case "Stagger":
				xmlReader.Read();
				if (!bool.TryParse(xmlReader.ReadContentAsString(), out stagger))
				{
					stagger = false;
				}
				break;
			case "MustDestroy":
				xmlReader.Read();
				if (!bool.TryParse(xmlReader.ReadContentAsString(), out mustDestroy))
				{
					mustDestroy = false;
				}
				break;
			case "DeathDelay":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out deathDelay))
				{
					deathDelay = 0f;
				}
				break;
			case "ClearDelay":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out clearDelay))
				{
					clearDelay = 0f;
				}
				break;
			case "Stats":
				ReadXmlStats(xmlReader);
				break;
			}
		}
	}

	public void ReadXmlStats(XmlReader reader)
	{
		for (int i = 0; i < reader.AttributeCount; i++)
		{
			reader.MoveToAttribute(i);
			switch (reader.Name)
			{
			case "localGunPointPos":
			{
				string[] array = reader.Value.Split(',');
				float result = 0f;
				float result2 = 0f;
				float.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result);
				if (array.Length > 1)
				{
					float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result2);
				}
				localGunPointPos = new Vector3(result, result2, 0f);
				break;
			}
			case "localTimerPos":
			{
				string[] array3 = reader.Value.Split(',');
				float result3 = 0f;
				float result4 = 0f;
				float.TryParse(array3[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result3);
				if (array3.Length > 1)
				{
					float.TryParse(array3[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result4);
				}
				localTimerPos = new Vector3(result3, result4, 0f);
				break;
			}
			case "animName":
				if (animName == "")
				{
					animName = beingID;
				}
				animName = reader.Value;
				allAnims.Add(animName);
				unlockedAnims.Add(animName);
				break;
			case "animBase":
				animName = reader.Value;
				break;
			case "scaleHP":
				if (!bool.TryParse(reader.Value, out scaleHP))
				{
					scaleHP = true;
				}
				break;
			case "shadow":
				if (!bool.TryParse(reader.Value, out shadow))
				{
					shadow = true;
				}
				break;
			case "overrideMovement":
				if (!bool.TryParse(reader.Value, out overrideMovement))
				{
					overrideMovement = true;
				}
				break;
			case "unmoveable":
				if (!bool.TryParse(reader.Value, out unmoveable))
				{
					unmoveable = false;
				}
				break;
			case "petAnim":
				if (!bool.TryParse(reader.Value, out petAnim))
				{
					petAnim = false;
				}
				break;
			case "tauntAnim":
				if (!bool.TryParse(reader.Value, out tauntAnim))
				{
					tauntAnim = false;
				}
				break;
			case "altAnims":
			{
				if (string.IsNullOrEmpty(reader.Value))
				{
					break;
				}
				allAnims.AddRange(reader.Value.Replace(" ", string.Empty).Split(','));
				for (int k = 0; k < allAnims.Count; k++)
				{
					if ((SaveDataCtrl.Get(string.Format("{0}Skin{1}", nameString, k), false) || SaveDataCtrl.Get(allAnims[k], false)) && !unlockedAnims.Contains(allAnims[k]))
					{
						unlockedAnims.Add(allAnims[k]);
					}
				}
				break;
			}
			case "chargeAnim":
				if (!bool.TryParse(reader.Value, out chargeAnim))
				{
					chargeAnim = false;
				}
				break;
			case "clearAnim":
				if (!bool.TryParse(reader.Value, out clearAnim))
				{
					clearAnim = false;
				}
				break;
			case "timeoutAnim":
				if (!bool.TryParse(reader.Value, out timeoutAnim))
				{
					timeoutAnim = false;
				}
				break;
			case "dieSound":
				if (dieSound == "")
				{
					dieSound = "mark_02";
				}
				dieSound = reader.Value;
				break;
			case "vocSynth":
				vocSynth = reader.Value;
				break;
			case "splashSprite":
				if (splashSprite == "")
				{
					splashSprite = beingID;
				}
				splashSprite = reader.Value;
				break;
			case "startingBrands":
			{
				tagStrings = reader.Value.Replace(" ", string.Empty).Split(',');
				string[] array2 = tagStrings;
				foreach (string text in array2)
				{
					if (!string.IsNullOrEmpty(text))
					{
						if (Enum.IsDefined(typeof(Brand), text))
						{
							startingBrands.Add((Brand)Enum.Parse(typeof(Brand), text));
						}
						else
						{
							Debug.LogError("Invalid Brand: " + text + " for " + nameString);
						}
					}
				}
				break;
			}
			case "lethality":
				if (!int.TryParse(reader.Value, out lethality))
				{
					lethality = 100;
				}
				break;
			}
		}
	}
}
