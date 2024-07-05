using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using I2.Loc;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class SpellObject : ItemObject
{
	public float mana;

	public AmountApp manaType = new AmountApp();

	public float originalMana;

	public string animWarning;

	public string animCast;

	public string animObj;

	public string animBlast;

	public string animShot;

	public string animHit;

	public string introSound;

	public string warningSound;

	public string castSound;

	public string shotSound;

	public string hitSound;

	public float damage;

	public float originalDamage;

	public int numTiles;

	public float numShots;

	public AmountApp numShotsType = new AmountApp();

	public float timeBetweenShots;

	public float warningDuration;

	public float shotVelocity;

	public float shotVelocityY;

	public float shotDuration;

	public float shotDelay;

	public float castDelay;

	public float castDuration;

	public float blastDuration;

	public float cooldown;

	public GunPointSetting gunPointSetting;

	public float yVariance;

	public bool consume;

	public bool flow;

	public bool trinityCast;

	public bool destroyOnHit;

	public bool onHitTriggerArts = true;

	public int dashDistance = 0;

	public int dashHeight = 0;

	public int slashLocation = 0;

	public float introDelay = 0f;

	public int hitboxWidth = 1;

	public int hitboxHeight = 1;

	public Vector2 hitboxOffset;

	public Vector2 spawnOffset = Vector2.zero;

	public bool effectLayer;

	public float recoveryTime;

	public bool faceVelocity;

	public int bending;

	public float timeToTravel;

	public ArcType arcType = ArcType.Normal;

	public FireLoop fireLoop = FireLoop.Normal;

	public List<string> spellActions = new List<string>();

	public List<string> timeOutActions = new List<string>();

	public List<StatusApp> statusApps = new List<StatusApp>();

	public string enhancementCode;

	public List<Enhancement> enhancements = new List<Enhancement>();

	public AmountApp damageType = new AmountApp();

	public bool pierceDefense;

	public bool pierceShield;

	public bool hitAllies;

	public bool hitEnemies;

	public bool hitSelf;

	public bool hitStructures;

	public bool anchor = false;

	public bool channel = false;

	public bool interrupt = true;

	public bool backfire = false;

	public bool fireAnim = true;

	public bool fireAnimLate = false;

	public string lineTracer;

	public bool destroyOnDeath = true;

	public Cardtridge cardtridge;

	public Spell spell;

	public ProjectileFactory P;

	public Tile touchedTile;

	public int castSlotNum;

	public int preCastShieldAmount = 0;

	public int tempDamage = 0;

	public int permDamage = 0;

	public bool trinityCasted = false;

	public List<Tile> tiles = new List<Tile>();

	public Projectile proj;

	private int doublecastCounter = 0;

	public SpellObject()
	{
	}

	protected SpellObject(SpellObject other)
	{
		SetItemAtts(other);
		mana = other.mana;
		manaType = other.manaType;
		animObj = other.animObj;
		animCast = other.animCast;
		animBlast = other.animBlast;
		animShot = other.animShot;
		animHit = other.animHit;
		animWarning = other.animWarning;
		introSound = other.introSound;
		castSound = other.castSound;
		shotSound = other.shotSound;
		hitSound = other.hitSound;
		warningSound = other.warningSound;
		tileApps = new List<TileApp>(other.tileApps);
		damage = other.damage;
		numTiles = other.numTiles;
		numShots = other.numShots;
		numShotsType = other.numShotsType;
		timeBetweenShots = other.timeBetweenShots;
		shotVelocity = other.shotVelocity;
		shotVelocityY = other.shotVelocityY;
		shotDuration = other.shotDuration;
		shotDelay = other.shotDelay;
		castDelay = other.castDelay;
		castDuration = other.castDuration;
		blastDuration = other.blastDuration;
		warningDuration = other.warningDuration;
		cooldown = other.cooldown;
		anchor = other.anchor;
		channel = other.channel;
		interrupt = other.interrupt;
		backfire = other.backfire;
		lineTracer = other.lineTracer;
		destroyOnDeath = other.destroyOnDeath;
		introDelay = other.introDelay;
		fireAnim = other.fireAnim;
		fireAnimLate = other.fireAnimLate;
		gunPointSetting = other.gunPointSetting;
		yVariance = other.yVariance;
		consume = other.consume;
		flow = other.flow;
		trinityCast = other.trinityCast;
		spellActions = new List<string>(other.spellActions);
		timeOutActions = new List<string>(other.timeOutActions);
		statusApps = new List<StatusApp>(other.statusApps);
		enhancements = new List<Enhancement>(other.enhancements);
		damageType = other.damageType;
		pierceDefense = other.pierceDefense;
		pierceShield = other.pierceShield;
		hitAllies = other.hitAllies;
		hitEnemies = other.hitEnemies;
		hitSelf = other.hitSelf;
		hitStructures = other.hitStructures;
		destroyOnHit = other.destroyOnHit;
		onHitTriggerArts = other.onHitTriggerArts;
		effectLayer = other.effectLayer;
		dashDistance = other.dashDistance;
		dashHeight = other.dashHeight;
		slashLocation = other.slashLocation;
		hitboxWidth = other.hitboxWidth;
		hitboxHeight = other.hitboxHeight;
		hitboxOffset = other.hitboxOffset;
		spawnOffset = other.spawnOffset;
		recoveryTime = other.recoveryTime;
		faceVelocity = other.faceVelocity;
		bending = other.bending;
		timeToTravel = other.timeToTravel;
		arcType = other.arcType;
		fireLoop = other.fireLoop;
		tempDamage = other.tempDamage;
		permDamage = other.permDamage;
		spellObj = this;
	}

	public SpellObject Clone()
	{
		return new SpellObject(this);
	}

	public SpellObject Set(Being theBeing, bool transferTempDamage = false)
	{
		if (theBeing == null)
		{
			return null;
		}
		being = theBeing;
		ctrl = being.ctrl;
		deCtrl = being.deCtrl;
		P = deCtrl.projectileFactory;
		spCtrl = S.I.spCtrl;
		beingAnim = being.anim;
		Spell spell = new GameObject().AddComponent<Spell>();
		spell.name = base.itemID;
		spell.transform.position = being.transform.position;
		spell.transform.rotation = deCtrl.transform.rotation;
		spell.transform.SetParent(being.transform, true);
		spell.being = being;
		spell.spellObj = spellObj;
		spell.itemObj = spellObj;
		this.spell = spell;
		item = spell;
		if (!transferTempDamage)
		{
			tempDamage = 0;
			if (generatedSpell != null)
			{
				generatedSpell.tempDamage = 0;
			}
		}
		return this;
	}

	public virtual void PlayerCast()
	{
		StartCast();
		foreach (CastSlot castSlot in being.player.duelDisk.castSlots)
		{
			castSlot.UpdateGlow();
		}
	}

	public void StartCast(bool doublecast = false, int doubleNum = 0, bool triggerFlow = true)
	{
		trinityCasted = false;
		spellObj.numShots = ctrl.GetAmount(spellObj.numShotsType, spellObj.numShots, spellObj);
		bool flag = false;
		if ((bool)being.GetStatusEffect(Status.Flow) && !tags.Contains(Tag.Weapon) && triggerFlow)
		{
			flag = true;
			being.GetStatusEffect(Status.Flow).amount -= 1f;
		}
		if (!doublecast)
		{
			doublecastCounter = 0;
		}
		else if (doubleNum == 0)
		{
			doublecastCounter++;
		}
		if (doublecastCounter > 1 && doubleNum == 0)
		{
			if ((bool)spell)
			{
				spell.StartCoroutine(spell.DelayedDoublecast(doublecastCounter));
			}
			return;
		}
		preCastShieldAmount = being.health.shield;
		Trigger(FTrigger.OnCast, doublecast);
		if (spellActions.Count > 0)
		{
			EffectActions.CallFunctionsWithItem(spellActions, this);
		}
		if ((bool)being.GetStatusEffect(Status.Trinity))
		{
			StatusEffect statusEffect = being.GetStatusEffect(Status.Trinity);
			if (statusEffect.amount >= 3f)
			{
				if (trinityCast)
				{
					trinityCasted = true;
					Trigger(FTrigger.TrinityCast);
					statusEffect.amount -= 3f;
					being.TriggerArtifacts(FTrigger.OnTrinityCast);
					if (statusEffect.amount < 1f)
					{
						statusEffect.duration = 0.2f;
					}
					being.PlayOnce(ctrl.trinityCastSound);
					if ((bool)statusEffect.display)
					{
						statusEffect.display.anim.SetTrigger("pop");
					}
				}
				else
				{
					statusEffect.amount = 3f;
				}
			}
		}
		if (flag)
		{
			Trigger(FTrigger.Flow, doublecast);
			being.TriggerArtifacts(FTrigger.OnFlow);
			if (flow)
			{
				being.PlayOnce(ctrl.earthboundSound);
			}
		}
	}

	public int CombinedDamage()
	{
		return Mathf.RoundToInt(damage + (float)tempDamage + (float)permDamage);
	}

	public int GetNumEnhancements(Enhancement enhancement = Enhancement.All)
	{
		if (enhancement == Enhancement.All)
		{
			return enhancements.Count;
		}
		int num = 0;
		foreach (Enhancement enhancement2 in spellObj.enhancements)
		{
			if (enhancement2 == enhancement)
			{
				num++;
			}
		}
		return num;
	}

	public void OnHit(Being hitBeing)
	{
		base.hitBeing = hitBeing;
		hitTile = hitBeing.mov.endTile;
		Trigger(FTrigger.OnHit, false, hitBeing);
	}

	public void TouchTile(Tile touchedTile)
	{
		this.touchedTile = touchedTile;
		Trigger(FTrigger.TouchTile);
	}

	public void TimeOut()
	{
		if (timeOutActions.Count > 0)
		{
			EffectActions.CallFunctionsWithItem(timeOutActions, this);
		}
	}

	public void RegisterSpellAction(string luaFunctionName)
	{
		spellActions.Add(luaFunctionName);
	}

	public void UnregisterSpellAction(string luaFunctionName)
	{
		spellActions.Remove(luaFunctionName);
	}

	public void RegisterTimeOutAction(string luaFunctionName)
	{
		timeOutActions.Add(luaFunctionName);
	}

	public void UnregisterTimeOutAction(string luaFunctionName)
	{
		timeOutActions.Remove(luaFunctionName);
	}

	public List<int> RandomList(int length)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < length; i++)
		{
			list.Add(i);
		}
		List<int> list2 = new List<int>();
		for (int j = 0; j < length; j++)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}

	public Vector3 GetDir(int i)
	{
		switch (i)
		{
		case 0:
			return Vector3.up;
		case 1:
			return Vector3.left;
		case 2:
			return Vector3.down;
		case 3:
			return Vector3.right;
		default:
			return Vector3.up;
		}
	}

	public Vector3 RoundedPosition(Vector3 thisVector)
	{
		return new Vector3(Mathf.RoundToInt(thisVector.x), Mathf.RoundToInt(thisVector.y), Mathf.RoundToInt(thisVector.z));
	}

	public void AddStatusApp(Status status)
	{
		statusApps.Add(new StatusApp(status, ctrl.GetAmount(currentApp.amountApp, currentApp.amount, this), 0f, currentApp.fTrigger));
	}

	public void ResetToOriginal()
	{
		mana = originalMana;
		damage = originalDamage;
	}

	public void RemoveEffect(Effect effect)
	{
		for (int num = spellObj.efApps.Count - 1; num >= 0; num--)
		{
			if (spellObj.efApps[num].effect == effect)
			{
				spellObj.efApps.RemoveAt(num);
			}
		}
	}

	public void ClearSpellFlowStatusApps()
	{
		for (int num = statusApps.Count - 1; num >= 0; num--)
		{
			if (statusApps[num].origin == FTrigger.Flow)
			{
				statusApps.Remove(statusApps[num]);
			}
		}
	}

	public int CalculatedSortingShots()
	{
		int num = Mathf.RoundToInt(spellObj.numShots);
		if (spellObj.numShotsType.type != 0)
		{
			num = ((spellObj.numShotsType.type != AmountType.Infinite) ? 999 : 99999);
		}
		else if (spellObj.HasParam("numOfWaves"))
		{
			num = Mathf.Clamp(num, 1, num);
			num *= int.Parse(spellObj.Param("numOfWaves"));
		}
		return num;
	}

	public void ReadXmlPrototype(XmlReader reader_parent)
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
				if (!LocalizationManager.TryGetTranslation("SpellNames/" + base.itemID, out nameString))
				{
					nameString = xmlReader.ReadContentAsString();
					if (tags.Contains(Tag.Player))
					{
						Debug.LogWarning("No localization for spell " + base.itemID + " name");
					}
				}
				break;
			case "Tags":
			{
				xmlReader.Read();
				tagStrings = xmlReader.ReadContentAsString().Replace(" ", string.Empty).Split(',');
				string[] array2 = tagStrings;
				foreach (string text2 in array2)
				{
					if (Enum.IsDefined(typeof(Tag), text2))
					{
						tags.Add((Tag)Enum.Parse(typeof(Tag), text2));
					}
					else
					{
						Debug.LogError("Invalid Tag: " + text2 + " for " + nameString);
					}
				}
				break;
			}
			case "Action":
				xmlReader.Read();
				RegisterSpellAction(xmlReader.ReadContentAsString());
				break;
			case "Description":
				if (S.modsInstalled)
				{
					xmlReader.Read();
					description = xmlReader.ReadContentAsString();
				}
				break;
			case "MetaDISABLED":
				xmlReader.Read();
				if (string.IsNullOrEmpty(description) && !LocalizationManager.TryGetTranslation("SpellDescriptions/" + base.itemID, out description))
				{
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
				if (!int.TryParse(xmlReader.ReadContentAsString(), out rarity))
				{
					rarity = -1;
				}
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
			case "Mana":
			{
				xmlReader.Read();
				string amountString3 = xmlReader.ReadContentAsString();
				manaType = new AmountApp(ref mana, amountString3);
				originalMana = mana;
				break;
			}
			case "Damage":
			{
				xmlReader.Read();
				string amountString2 = xmlReader.ReadContentAsString();
				damageType = new AmountApp(ref damage, amountString2);
				originalDamage = damage;
				break;
			}
			case "Tiles":
				xmlReader.Read();
				if (!int.TryParse(xmlReader.ReadContentAsString(), out numTiles))
				{
					numTiles = 0;
				}
				break;
			case "Location":
				tileApps.Add(ReadXmlLocation(xmlReader));
				break;
			case "Shots":
			{
				xmlReader.Read();
				string amountString = xmlReader.ReadContentAsString();
				numShotsType = new AmountApp(ref numShots, amountString);
				break;
			}
			case "TimeBetweenShots":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out timeBetweenShots))
				{
					timeBetweenShots = 0.1f;
				}
				break;
			case "ShotVelocity":
			{
				xmlReader.Read();
				string text = xmlReader.ReadContentAsString();
				if (text.Contains(","))
				{
					float result = 0f;
					float result2 = 0f;
					string[] array = text.Split(',');
					float.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result);
					if (array.Length > 1)
					{
						float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result2);
					}
					shotVelocity = result;
					shotVelocityY = result2;
				}
				else if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out shotVelocity))
				{
					shotVelocity = 0f;
				}
				break;
			}
			case "ShotDuration":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out shotDuration))
				{
					shotDuration = 0f;
				}
				break;
			case "ShotDelay":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out shotDelay))
				{
					shotDelay = 0f;
				}
				break;
			case "CastDelay":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out castDelay))
				{
					castDelay = 0f;
				}
				break;
			case "CastDuration":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out castDuration))
				{
					castDuration = 0f;
				}
				break;
			case "BlastDuration":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out blastDuration))
				{
					blastDuration = 0f;
				}
				break;
			case "WarningDuration":
				xmlReader.Read();
				if (!float.TryParse(xmlReader.ReadContentAsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out warningDuration))
				{
					warningDuration = 0f;
				}
				break;
			case "DestroyOnHit":
				xmlReader.Read();
				if (!bool.TryParse(xmlReader.ReadContentAsString(), out destroyOnHit))
				{
					destroyOnHit = false;
				}
				break;
			case "Stats":
				ReadXmlStats(xmlReader);
				break;
			case "Params":
				ReadXmlParams(xmlReader);
				break;
			case "OnCast":
				EffectApp.AddTo(xmlReader, this, efApps);
				break;
			case "OnHit":
				EffectApp.AddTo(xmlReader, this, efApps);
				break;
			case "TouchTile":
				EffectApp.AddTo(xmlReader, this, efApps);
				break;
			case "Flow":
				EffectApp.AddTo(xmlReader, this, efApps);
				flow = true;
				break;
			case "Hold":
				EffectApp.AddTo(xmlReader, this, efApps);
				break;
			case "TrinityCast":
				EffectApp.AddTo(xmlReader, this, efApps);
				trinityCast = true;
				break;
			case "Execute":
				EffectApp.AddTo(xmlReader, this, efApps);
				break;
			case "HitAllies":
				xmlReader.Read();
				if (!bool.TryParse(xmlReader.ReadContentAsString(), out hitAllies))
				{
					hitAllies = false;
				}
				break;
			case "HitEnemies":
				xmlReader.Read();
				if (!bool.TryParse(xmlReader.ReadContentAsString(), out hitEnemies))
				{
					hitEnemies = false;
				}
				break;
			case "HitSelf":
				xmlReader.Read();
				if (!bool.TryParse(xmlReader.ReadContentAsString(), out hitSelf))
				{
					hitSelf = false;
				}
				break;
			case "HitStructures":
				xmlReader.Read();
				if (!bool.TryParse(xmlReader.ReadContentAsString(), out hitStructures))
				{
					hitStructures = false;
				}
				break;
			}
		}
	}

	public void ReadXmlStats(XmlReader reader)
	{
		for (int i = 0; i < reader.AttributeCount; i++)
		{
			reader.MoveToAttribute(i);
			if (reader.Value == "")
			{
				continue;
			}
			switch (reader.Name)
			{
			case "animCast":
				animCast = reader.Value;
				break;
			case "animObj":
				animObj = reader.Value;
				break;
			case "animBlast":
				animBlast = reader.Value;
				break;
			case "animShot":
				animShot = reader.Value;
				break;
			case "animHit":
				animHit = reader.Value;
				break;
			case "animWarning":
				animWarning = reader.Value;
				break;
			case "introSound":
				introSound = reader.Value;
				break;
			case "castSound":
				castSound = reader.Value;
				break;
			case "shotSound":
				shotSound = reader.Value;
				break;
			case "hitSound":
				hitSound = reader.Value;
				break;
			case "warningSound":
				warningSound = reader.Value;
				break;
			case "cooldown":
				cooldown = float.Parse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
				break;
			case "setToGunPoint":
			{
				string value3 = reader.Value;
				if (Enum.IsDefined(typeof(GunPointSetting), value3))
				{
					gunPointSetting = (GunPointSetting)Enum.Parse(typeof(GunPointSetting), value3);
				}
				break;
			}
			case "yVariance":
				if (!float.TryParse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out yVariance))
				{
					yVariance = 0f;
				}
				break;
			case "dashDistance":
				if (!int.TryParse(reader.Value, out dashDistance))
				{
					dashDistance = 0;
				}
				break;
			case "dashHeight":
				if (!int.TryParse(reader.Value, out dashHeight))
				{
					dashHeight = 0;
				}
				break;
			case "slashLocation":
				slashLocation = int.Parse(reader.Value);
				break;
			case "lineTracer":
				lineTracer = reader.Value;
				break;
			case "effectLayer":
				if (!bool.TryParse(reader.Value, out effectLayer))
				{
					effectLayer = true;
				}
				break;
			case "destroyOnDeath":
				if (!bool.TryParse(reader.Value, out destroyOnDeath))
				{
					destroyOnDeath = true;
				}
				break;
			case "fireAnim":
				if (!bool.TryParse(reader.Value, out fireAnim))
				{
					fireAnim = false;
				}
				break;
			case "fireAnimLate":
				if (!bool.TryParse(reader.Value, out fireAnimLate))
				{
					fireAnimLate = false;
				}
				break;
			case "introDelay":
				introDelay = float.Parse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
				break;
			case "hitboxWidth":
				hitboxWidth = int.Parse(reader.Value);
				break;
			case "hitboxHeight":
				hitboxHeight = int.Parse(reader.Value);
				break;
			case "hitboxOffsetX":
				hitboxOffset.x = float.Parse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
				break;
			case "hitboxOffsetY":
				hitboxOffset.y = float.Parse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
				break;
			case "spawnOffsetX":
				spawnOffset.x = float.Parse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
				break;
			case "spawnOffsetY":
				spawnOffset.y = float.Parse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
				break;
			case "recoveryTime":
				recoveryTime = float.Parse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
				break;
			case "onHitTriggerArts":
				if (!bool.TryParse(reader.Value, out onHitTriggerArts))
				{
					onHitTriggerArts = true;
				}
				break;
			case "faceVelocity":
				if (!bool.TryParse(reader.Value, out faceVelocity))
				{
					faceVelocity = false;
				}
				break;
			case "bending":
				bending = int.Parse(reader.Value);
				break;
			case "timeToTravel":
				timeToTravel = float.Parse(reader.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
				break;
			case "pierceDefense":
				if (!bool.TryParse(reader.Value, out pierceDefense))
				{
					pierceDefense = true;
				}
				break;
			case "pierceShield":
				if (!bool.TryParse(reader.Value, out pierceShield))
				{
					pierceShield = true;
				}
				break;
			case "arcType":
			{
				string value2 = reader.Value;
				if (Enum.IsDefined(typeof(ArcType), value2))
				{
					arcType = (ArcType)Enum.Parse(typeof(ArcType), value2);
				}
				else
				{
					Debug.LogError("Invalid Arc Type for " + base.itemID);
				}
				break;
			}
			case "fireLoop":
			{
				string value = reader.Value;
				if (Enum.IsDefined(typeof(FireLoop), value))
				{
					fireLoop = (FireLoop)Enum.Parse(typeof(FireLoop), value);
				}
				else
				{
					Debug.LogError("Invalid Fire Loop type " + base.itemID);
				}
				break;
			}
			}
		}
	}
}
