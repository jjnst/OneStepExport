using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class ItemObject
{
	public string nameString;

	public string shortName;

	protected string[] tagStrings = new string[0];

	public List<Tag> tags = new List<Tag>();

	public List<FTrigger> triggerTags = new List<FTrigger>();

	public List<Effect> effectTags = new List<Effect>();

	public string description = "";

	public Sprite sprite;

	public int rarity;

	public Brand brand;

	public string flavor;

	public ItemType type;

	public BC ctrl;

	public DeckCtrl deCtrl;

	public SpawnCtrl spCtrl;

	public Being being;

	public Animator beingAnim;

	public List<TileApp> tileApps = new List<TileApp>();

	public List<EffectApp> efApps = new List<EffectApp>();

	public EffectApp currentApp;

	public Dictionary<string, string> paramDictionary = new Dictionary<string, string>();

	public Item item;

	public SpellObject spellObj;

	public ArtifactObject artObj;

	public SpellObject generatedSpell;

	public SpellObject originSpell;

	public SpellObject parentSpell;

	public PactObject pactObj;

	public Being hitBeing;

	public Tile hitTile;

	public Being forwardedTargetHit;

	public bool alwaysUseGameTime = false;

	public int unlockLevel = -1;

	private bool checkFailed = false;

	public string itemID { get; protected set; }

	public void SetItemAtts(ItemObject other)
	{
		itemID = other.itemID;
		nameString = other.nameString;
		tags = other.tags;
		description = other.description;
		flavor = other.flavor;
		sprite = other.sprite;
		rarity = other.rarity;
		brand = other.brand;
		type = other.type;
		effectTags = new List<Effect>(other.effectTags);
		triggerTags = new List<FTrigger>(other.triggerTags);
		efApps = new List<EffectApp>();
		foreach (EffectApp efApp in other.efApps)
		{
			efApps.Add(efApp.Clone());
		}
		paramDictionary = new Dictionary<string, string>(other.paramDictionary);
		int num = 3;
		if (!string.IsNullOrEmpty(other.nameString))
		{
			shortName = Regex.Replace(other.nameString, "(?<!^)[aeiouaeiou]", string.Empty);
			if (shortName.Length > num)
			{
				shortName = shortName.Substring(0, num);
			}
		}
	}

	public float GetDeltaTime()
	{
		if (being == ctrl.currentPlayer)
		{
			return BC.playerChronoTime;
		}
		return Time.deltaTime;
	}

	public void Trigger(FTrigger fTrigger, bool doublecast = false, Being hitBeing = null, int forwardedHitDamage = 0)
	{
		if (!HasTrigger(fTrigger))
		{
			return;
		}
		forwardedTargetHit = hitBeing;
		foreach (EffectApp efApp in efApps)
		{
			if (fTrigger != efApp.fTrigger)
			{
				continue;
			}
			if (currentApp != null && currentApp.frameTriggered == Time.frameCount && currentApp.fTrigger == fTrigger)
			{
				break;
			}
			checkFailed = false;
			if (efApp.checks != null)
			{
				using (List<Check>.Enumerator enumerator2 = efApp.checks.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						switch (enumerator2.Current)
						{
						case Check.AmountOver:
							if (ctrl.GetAmount(efApp.amountApp, efApp.amount, spellObj, artObj) <= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj))
							{
								checkFailed = true;
							}
							break;
						case Check.Fragile:
							if (!being.GetStatusEffect(Status.Fragile))
							{
								checkFailed = true;
							}
							break;
						case Check.NotFragile:
							if ((bool)being.GetStatusEffect(Status.Fragile))
							{
								checkFailed = true;
							}
							break;
						case Check.Enemy:
							if ((bool)hitBeing && hitBeing.alignNum != -1)
							{
								checkFailed = true;
							}
							break;
						case Check.EnemyOrHostage:
							if ((bool)hitBeing && hitBeing.alignNum != -1 && !hitBeing.beingObj.tags.Contains(Tag.Hostage))
							{
								checkFailed = true;
							}
							break;
						case Check.EnemyOrStructure:
							if ((bool)hitBeing && hitBeing.alignNum != -1 && hitBeing.alignNum != 0)
							{
								checkFailed = true;
							}
							break;
						case Check.DamageOver:
							if ((bool)hitBeing && (float)forwardedHitDamage <= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj))
							{
								checkFailed = true;
							}
							break;
						case Check.HasStatusFromThis:
						{
							Status status3 = (Status)Enum.Parse(typeof(Status), efApp.checkValue);
							if (!being.GetStatusEffect(status3))
							{
								checkFailed = true;
								break;
							}
							bool flag = false;
							foreach (StatusStack statusStack in being.GetStatusEffect(status3).statusStacks)
							{
								if (statusStack.source == this)
								{
									flag = true;
								}
							}
							checkFailed = !flag;
							break;
						}
						case Check.HitHPOver:
							if ((bool)hitBeing && (float)hitBeing.health.current <= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj))
							{
								checkFailed = true;
							}
							break;
						case Check.HitHPUnder:
							if ((bool)hitBeing && (float)hitBeing.health.current >= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj))
							{
								checkFailed = true;
							}
							break;
						case Check.Hostage:
							if ((bool)hitBeing && !hitBeing.beingObj.tags.Contains(Tag.Hostage))
							{
								checkFailed = true;
							}
							break;
						case Check.HPOver:
							if ((bool)being && (float)being.health.current <= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj))
							{
								checkFailed = true;
							}
							break;
						case Check.HPUnder:
							if ((bool)being && (float)being.health.current >= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj))
							{
								checkFailed = true;
							}
							break;
						case Check.Neutral:
							if ((bool)hitBeing && hitBeing.alignNum != 0 && !hitBeing.beingObj.tags.Contains(Tag.Structure))
							{
								checkFailed = true;
							}
							break;
						case Check.ManaCostOver:
							if ((bool)being && (bool)being.player && being.player.spellToCast != null && being.player.spellToCast.mana <= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj))
							{
								checkFailed = true;
							}
							break;
						case Check.ManaCostUnder:
							if ((bool)being && (bool)being.player && being.player.spellToCast != null && being.player.spellToCast.mana >= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj))
							{
								checkFailed = true;
							}
							break;
						case Check.Never:
							checkFailed = true;
							break;
						case Check.Flow:
							if (!ctrl.currentPlayer.GetStatusEffect(Status.Flow))
							{
								checkFailed = true;
							}
							break;
						case Check.NotFlow:
							if ((bool)ctrl.currentPlayer.GetStatusEffect(Status.Flow))
							{
								checkFailed = true;
							}
							break;
						case Check.NoMinion:
							if ((bool)hitBeing && hitBeing.minion)
							{
								checkFailed = true;
							}
							break;
						case Check.NotDrone:
							if ((bool)hitBeing && hitBeing.lastSpellHit != null && hitBeing.lastSpellHit.tags.Contains(Tag.Drone))
							{
								checkFailed = true;
							}
							break;
						case Check.NotWeaponOrDrone:
							if (((bool)hitBeing && hitBeing.lastSpellHit != null && hitBeing.lastSpellHit.tags.Contains(Tag.Weapon)) || ((bool)hitBeing && hitBeing.lastSpellHit != null && hitBeing.lastSpellHit.tags.Contains(Tag.Drone)))
							{
								checkFailed = true;
							}
							break;
						case Check.PlayerWasHit:
							if (being.battleGrid.lastTargetHit.player == null)
							{
								checkFailed = true;
							}
							break;
						case Check.PlayerWasNotHit:
							if (being.battleGrid.lastTargetHit.player != null)
							{
								checkFailed = true;
							}
							break;
						case Check.ShieldUnder:
							if ((bool)being && (float)being.health.shield >= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj))
							{
								checkFailed = true;
							}
							break;
						case Check.SpellID:
							if ((bool)hitBeing && hitBeing.lastSpellHit != null && hitBeing.lastSpellHit.itemID != efApp.checkValue)
							{
								checkFailed = true;
							}
							break;
						case Check.SpellToCastID:
							if ((bool)being && (bool)being.player && being.player.spellToCast != null && being.player.spellToCast.itemID != efApp.checkValue)
							{
								checkFailed = true;
							}
							break;
						case Check.SpellToCastIsNotWeapon:
							if ((bool)being && (bool)being.player && being.player.spellToCast != null && being.player.spellToCast.type == ItemType.Wep)
							{
								checkFailed = true;
							}
							break;
						case Check.StatusOver:
							if (Enum.IsDefined(typeof(Status), efApp.checkValue))
							{
								Status status2 = (Status)Enum.Parse(typeof(Status), efApp.checkValue);
								if (((bool)hitBeing && !hitBeing.GetStatusEffect(status2)) || ((bool)hitBeing && (bool)hitBeing.GetStatusEffect(status2) && hitBeing.GetStatusEffect(status2).amount <= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj)))
								{
									checkFailed = true;
								}
							}
							break;
						case Check.StatusUnder:
							if (Enum.IsDefined(typeof(Status), efApp.checkValue))
							{
								Status status = (Status)Enum.Parse(typeof(Status), efApp.checkValue);
								if ((bool)hitBeing && (bool)hitBeing.GetStatusEffect(status) && hitBeing.GetStatusEffect(status).amount >= ctrl.GetAmount(efApp.checkAmountApp, efApp.checkAmount, spellObj, artObj))
								{
									checkFailed = true;
								}
							}
							break;
						case Check.TouchedTileNotBroken:
							if (spellObj != null && spellObj.touchedTile.type == TileType.Broken)
							{
								checkFailed = true;
							}
							break;
						case Check.TrinityCast:
							if (spellObj != null && !spellObj.trinityCasted)
							{
								checkFailed = true;
							}
							break;
						case Check.NotTrinityCast:
							if (spellObj != null && spellObj.trinityCasted)
							{
								checkFailed = true;
							}
							break;
						case Check.LastSpell:
						{
							if (spellObj.being.player.duelDisk.queuedCardtridges.Count > 0)
							{
								checkFailed = true;
							}
							int num = 0;
							foreach (CastSlot castSlot in spellObj.being.player.duelDisk.castSlots)
							{
								if ((bool)castSlot.cardtridgeFill)
								{
									num++;
								}
							}
							if (num > 1)
							{
								checkFailed = true;
							}
							break;
						}
						}
					}
				}
				if (checkFailed)
				{
					continue;
				}
			}
			if (doublecast && efApp.effect == Effect.DoubleCast)
			{
				break;
			}
			if (fTrigger == FTrigger.WhileIdle)
			{
				if (!(efApp.timer >= efApp.triggerCooldown))
				{
					efApp.timer += Time.deltaTime;
					if ((bool)ctrl.currentPlayer && ctrl.currentPlayer.mov.state != 0)
					{
						efApp.timer = 0f;
					}
					continue;
				}
				efApp.timer = 0f;
			}
			if (fTrigger == FTrigger.WhileBattle || fTrigger == FTrigger.Hold || fTrigger == FTrigger.While)
			{
				if (!(efApp.timer >= efApp.triggerCooldown))
				{
					efApp.timer += Time.deltaTime;
					continue;
				}
				efApp.timer = 0f;
			}
			if (fTrigger == FTrigger.WhileHPBelow)
			{
				if (!((float)being.health.current < efApp.triggerAmount))
				{
					break;
				}
				if (!(efApp.timer >= efApp.triggerCooldown))
				{
					efApp.timer += Time.deltaTime;
					continue;
				}
				efApp.timer = 0f;
			}
			if (fTrigger == FTrigger.WhileManaBelow)
			{
				if (!(being.player.duelDisk.currentMana < efApp.triggerAmount))
				{
					break;
				}
				if (!(efApp.timer >= efApp.triggerCooldown))
				{
					efApp.timer += Time.deltaTime;
					continue;
				}
				efApp.timer = 0f;
			}
			if (fTrigger == FTrigger.WhileShieldBelow)
			{
				if (!((float)being.health.shield < efApp.triggerAmount))
				{
					break;
				}
				if (!(efApp.timer >= efApp.triggerCooldown))
				{
					efApp.timer += Time.deltaTime;
					continue;
				}
				efApp.timer = 0f;
			}
			if ((fTrigger == FTrigger.OnHPBelow && (!((float)being.healthBeforeHit > efApp.triggerAmount) || !((float)being.health.current < efApp.triggerAmount))) || (fTrigger == FTrigger.OnManaBelow && (!(ctrl.currentPlayer.manaBeforeSpellCast > efApp.triggerAmount) || !(being.player.duelDisk.currentMana < efApp.triggerAmount))))
			{
				break;
			}
			if (!(UnityEngine.Random.Range(0f, 1f) > efApp.chance))
			{
				currentApp = efApp;
				currentApp.frameTriggered = Time.frameCount;
				EffectActions.CallFunctionWithItem(efApp.effect.ToString(), this);
				if (fTrigger == FTrigger.Flow || fTrigger == FTrigger.Execute)
				{
					being.beingStatsPanel.CreateImageFlash(fTrigger.ToString());
				}
				if (artObj != null && artObj.art.listCard != null && (efApp.triggerCooldown == 0f || efApp.triggerCooldown > 1f))
				{
					artObj.art.listCard.anim.SetTrigger("cast");
				}
				currentApp = null;
			}
		}
	}

	public void ReadXmlParams(XmlReader reader)
	{
		for (int i = 0; i < reader.AttributeCount; i++)
		{
			reader.MoveToAttribute(i);
			string value = reader.Value;
			paramDictionary.Add(reader.Name, value);
		}
	}

	public void SetParam(string paramName, string paramValue)
	{
		paramDictionary[paramName] = paramValue;
	}

	public string Param(string paramName)
	{
		if (!paramDictionary.ContainsKey(paramName))
		{
			Debug.LogError(itemID + " paramDictionary does not contain key: " + paramName);
		}
		return paramDictionary[paramName];
	}

	public bool HasParam(string paramName)
	{
		return paramDictionary.ContainsKey(paramName);
	}

	public TileApp ReadXmlLocation(XmlReader reader, bool effApp = false)
	{
		Location loc = Location.Current;
		Shape shap = Shape.Default;
		List<Pattern> list = new List<Pattern>();
		List<Pattern> list2 = new List<Pattern>();
		int position = 1;
		string text = "";
		for (int i = 0; i < reader.AttributeCount; i++)
		{
			reader.MoveToAttribute(i);
			text = reader.Value;
			if (reader.Name.Contains("Shape"))
			{
				if (!string.IsNullOrEmpty(text))
				{
					shap = (Shape)Enum.Parse(typeof(Shape), text);
				}
			}
			else if (reader.Name.Contains("Pattern"))
			{
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				tagStrings = reader.ReadContentAsString().Replace(" ", string.Empty).Split(',');
				if (reader.Name == "LocationPattern")
				{
					string[] array = tagStrings;
					foreach (string text2 in array)
					{
						if (Enum.IsDefined(typeof(Pattern), text2))
						{
							list2.Add((Pattern)Enum.Parse(typeof(Pattern), text2));
						}
						else
						{
							Debug.LogError("Invalid Pattern: " + text2 + " for " + nameString);
						}
					}
					continue;
				}
				string[] array2 = tagStrings;
				foreach (string text3 in array2)
				{
					if (Enum.IsDefined(typeof(Pattern), text3))
					{
						list.Add((Pattern)Enum.Parse(typeof(Pattern), text3));
					}
					else
					{
						Debug.LogError("Invalid Pattern: " + text3 + " for " + nameString);
					}
				}
			}
			else if (reader.Name.Contains("Position"))
			{
				if (!string.IsNullOrEmpty(text))
				{
					position = int.Parse(text);
				}
			}
			else if (effApp && reader.Name == "Location" && !string.IsNullOrEmpty(text))
			{
				loc = (Location)Enum.Parse(typeof(Location), text);
			}
		}
		if (!effApp)
		{
			reader.Read();
			text = reader.ReadContentAsString();
			if (!string.IsNullOrEmpty(text))
			{
				loc = (Location)Enum.Parse(typeof(Location), text);
			}
		}
		return new TileApp(loc, shap, list, position, null, list2);
	}

	public bool HasTrigger(FTrigger FTrigger)
	{
		foreach (EffectApp efApp in efApps)
		{
			if (efApp.fTrigger == FTrigger)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect(Effect effect)
	{
		foreach (EffectApp efApp in efApps)
		{
			if (efApp.effect == effect)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCheck(Check check)
	{
		foreach (EffectApp efApp in efApps)
		{
			if (efApp.checks != null && efApp.checks.Contains(check))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAmount(AmountType amountType)
	{
		foreach (EffectApp efApp in efApps)
		{
			if (efApp.amountApp != null && efApp.amountApp.type == amountType)
			{
				return true;
			}
		}
		if (spellObj != null && ((spellObj.numShotsType != null && spellObj.numShotsType.type == amountType) || (spellObj.damageType != null && spellObj.damageType.type == amountType)))
		{
			return true;
		}
		return false;
	}

	public EffectApp GetValue(string value)
	{
		foreach (EffectApp efApp in efApps)
		{
			if (efApp.value == value)
			{
				return efApp;
			}
		}
		return null;
	}

	public EffectApp GetEffect(Effect effect, int index = 0)
	{
		int num = 0;
		foreach (EffectApp efApp in efApps)
		{
			if (efApp.effect == effect)
			{
				if (index <= num)
				{
					return efApp;
				}
				num++;
			}
		}
		return null;
	}

	public List<EffectApp> GetEffects(Effect effect)
	{
		List<EffectApp> list = new List<EffectApp>();
		foreach (EffectApp efApp in efApps)
		{
			if (efApp.effect == effect)
			{
				list.Add(efApp);
			}
		}
		return list;
	}

	public List<Tile> Get(int tiles = 1, int tileAppNum = 0)
	{
		return being.battleGrid.Get(tileApps[tileAppNum], tiles, being, this);
	}

	public List<Tile> Get(TileApp tileApp, int tiles = 1)
	{
		return being.battleGrid.Get(tileApp, tiles, being, this);
	}
}
