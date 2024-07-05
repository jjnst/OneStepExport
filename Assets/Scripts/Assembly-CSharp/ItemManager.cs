using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using AssetBundles;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;

[MoonSharpUserData]
public class ItemManager : MonoBehaviour
{
	public Dictionary<string, ItemObject> itemDictionary;

	public Dictionary<string, SpellObject> spellDictionary;

	public Dictionary<string, ArtifactObject> artDictionary;

	public Dictionary<string, PactObject> pactDictionary;

	public List<ArtifactObject> baseArtList;

	public List<ArtifactObject> nonBaseArtList;

	public List<ArtifactObject> playerFullArtList;

	public List<ArtifactObject> playerNonDuplicateNonBaseArtList;

	public List<ArtifactObject> minibossRewards;

	public List<ArtifactObject> bossRewards;

	public List<ArtifactObject> buffList;

	public List<ArtifactObject> loopMutationList;

	public List<ArtifactObject> defaultEnemyArtList = new List<ArtifactObject>();

	public List<ItemObject> playerItemList;

	public List<SpellObject> playerSpellList;

	public List<SpellObject> playerFullSpellList;

	public List<SpellObject> unobtainedPlayerSpellList;

	public List<SpellObject> cpuSpellList;

	public List<SpellObject> weaponList;

	public List<SpellObject> pvpSpellList;

	public Dictionary<Brand, List<SpellObject>> brandSpellLists = new Dictionary<Brand, List<SpellObject>>();

	public List<SpellObject> saffronBossSpellList = new List<SpellObject>();

	public List<PactObject> pactList = new List<PactObject>();

	public List<PactObject> pactChallengeList = new List<PactObject>();

	public List<PactObject> pactRewardList = new List<PactObject>();

	public List<PactObject> hellPasses = new List<PactObject>();

	public List<ArtifactObject> unobtainedNonBaseArts;

	public List<ItemObject> unlocks = new List<ItemObject>();

	public Dictionary<string, RuntimeAnimatorController> animations = new Dictionary<string, RuntimeAnimatorController>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

	private List<string> animClipNames = new List<string>();

	private Dictionary<string, List<Sprite>> animClipSprites = new Dictionary<string, List<Sprite>>(StringComparer.OrdinalIgnoreCase);

	public bool animClipSpritesGenerated = false;

	public Dictionary<string, SpriteAnimationClip> spriteAnimClips = new Dictionary<string, SpriteAnimationClip>(StringComparer.OrdinalIgnoreCase);

	private Dictionary<string, AudioClip> allAudioClips = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, string> allScripts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public List<Brand> brandTypes;

	private ModCtrl modCtrl;

	private RunCtrl runCtrl;

	private UnlockCtrl unCtrl;

	private XMLReader xmlReader;

	private void Awake()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
		modCtrl = S.I.modCtrl;
		runCtrl = S.I.runCtrl;
		unCtrl = S.I.unCtrl;
		brandTypes = Enum.GetValues(typeof(Brand)).Cast<Brand>().ToList();
		Dictionary<string, string> scriptToCodeMap = new Dictionary<string, string>();
		Script.DefaultOptions.ScriptLoader = new UnityAssetsScriptLoader(scriptToCodeMap);
		string error;
		RuntimeAnimatorController[] array = AssetBundleManager.LoadAllAssets<RuntimeAnimatorController>("animations", out error);
		if (!string.IsNullOrEmpty(error))
		{
			Debug.Log(error);
		}
		RuntimeAnimatorController[] array2 = array;
		foreach (RuntimeAnimatorController runtimeAnimatorController in array2)
		{
			animations[runtimeAnimatorController.name] = runtimeAnimatorController;
		}
		RuntimeAnimatorController[] array3 = AssetBundleManager.LoadAllAssets<RuntimeAnimatorController>("sprites", out error);
		if (!string.IsNullOrEmpty(error))
		{
			Debug.Log(error);
		}
		RuntimeAnimatorController[] array4 = array3;
		foreach (RuntimeAnimatorController runtimeAnimatorController2 in array4)
		{
			animations[runtimeAnimatorController2.name] = runtimeAnimatorController2;
		}
		Sprite[] array5 = AssetBundleManager.LoadAllAssets<Sprite>("sprites", out error);
		if (!string.IsNullOrEmpty(error))
		{
			Debug.Log(error);
		}
		Sprite[] array6 = array5;
		foreach (Sprite sprite in array6)
		{
			sprites[sprite.name] = sprite;
		}
		AudioClip[] array7 = AssetBundleManager.LoadAllAssets<AudioClip>("sounds", out error);
		if (!string.IsNullOrEmpty(error))
		{
			Debug.Log(error);
		}
		AudioClip[] array8 = array7;
		foreach (AudioClip audioClip in array8)
		{
			allAudioClips[audioClip.name] = audioClip;
		}
	}

	public SpriteAnimationClip GetClip(string clipName)
	{
		if (!spriteAnimClips.ContainsKey(clipName))
		{
			Debug.LogError("No animation Clip with name: " + clipName);
			return default(SpriteAnimationClip);
		}
		return spriteAnimClips[clipName];
	}

	public RuntimeAnimatorController GetAnim(string animName)
	{
		if (!animations.ContainsKey(animName))
		{
			Debug.LogWarning("No Runtime Animator Controller with name: " + animName);
			return null;
		}
		return animations[animName];
	}

	public Sprite GetSprite(string spriteName)
	{
		if (!sprites.ContainsKey(spriteName))
		{
			return sprites["Placeholder"];
		}
		return sprites[spriteName];
	}

	public AudioClip GetAudioClip(string soundName)
	{
		if (string.IsNullOrEmpty(soundName) && !allAudioClips.ContainsKey(soundName))
		{
			return null;
		}
		if (!allAudioClips.ContainsKey(soundName))
		{
			Debug.LogError("No audio clip with name: " + soundName);
		}
		return allAudioClips[soundName];
	}

	public void LoadItemData()
	{
		Debug.Log("RELOADING ITEM DATA " + Time.frameCount);
		unCtrl.UpdateUnlockLevel();
		unlocks.Clear();
		saffronBossSpellList.Clear();
		LoadEffectsLua();
		itemDictionary = new Dictionary<string, ItemObject>(StringComparer.OrdinalIgnoreCase);
		CreateSpellObjectPrototypes();
		CreateArtifactObjectPrototypes();
		CreatePactObjectPrototypes();
		string text = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", "SeraCannon,Explosion,Salamander,PetDragon,BackBurner,Flamberge,Brushfire,MoltenCore,FrostMail,Frostbite,", "IceShield,Fimbulveter,Icing,SnowBoots,ColdPressedJuice,MissMeShield,HardenedShields,FateShield,BloodShield,Acupuncture,", "MasonJar,GripTape,Scavenge,CassidyScarf,CollectRing,Ninjutsu,ShadowToxin,ImmuneSystem,Monsoon,Venoshock,", "Backstab,ResistantStrain,Twoxin,ChaosFrag,Thorn,DartFrog,Vaccine,UnicornBomb,Flechette,Sidewinder,", "Amalgam,PetKitty,PocketSand,Echo,PhasePlates,FlintShot,BladeRain,Bladeskrieg,ChargeRing,Excalibur,", "BeamCrystals,SumGrail,Adamantium,Circuit,Cynet,LastLetter,LifeSword,StepFury,Resonate,RockTomb,", "SpellShield,Stasis,SoulFire,Salvage,Lilac,Daffodil,SpellthiefLicense,BlueberryJam,Tessellate,", "Stoplight,TriForce,Skipper,Sunshine,TimeStop,CandyWrapper,Gambit,Mother,Trifrost,DuelDisk,", "Goldfish,PetFox,Blink,Transcendence,AmbientBurst,Unleash,Vivisection,Midnight,Wonder,GoldenCat,", "Curacao,BoltAction,Trident,Inverter,Jackhammer,Paragon,Sleight,Haven,Sunder,StormSaw,Overload");
		string[] array = text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (itemDictionary.ContainsKey(array[i]))
			{
				unlocks.Add(itemDictionary[array[i]]);
				unlocks[i].unlockLevel = 1 + (i + (i + 1) % 2) / 2;
			}
			else
			{
				Debug.LogError("No item named " + array[i]);
			}
		}
		CreateCustomRuntimeAnimatorControllers();
		GenerateAnimClips();
		baseArtList = artDictionary.Values.Where((ArtifactObject t) => t.tags.Contains(Tag.Base)).ToList();
		if (S.I.scene == GScene.DemoLive)
		{
			playerFullSpellList = spellDictionary.Values.Where((SpellObject t) => t.tags.Contains(Tag.Player) && t.tags.Contains(Tag.Live)).ToList();
			playerSpellList = spellDictionary.Values.Where((SpellObject t) => t.tags.Contains(Tag.Player) && t.tags.Contains(Tag.Live)).ToList();
			playerFullArtList = artDictionary.Values.Where((ArtifactObject t) => t.tags.Contains(Tag.Player) && !t.tags.Contains(Tag.Cpu) && t.tags.Contains(Tag.Demo) && !t.tags.Contains(Tag.Starting)).ToList();
			nonBaseArtList = artDictionary.Values.Where((ArtifactObject t) => !t.tags.Contains(Tag.Base) && t.tags.Contains(Tag.Demo) && !t.tags.Contains(Tag.Cpu)).ToList();
			buffList = artDictionary.Values.Where((ArtifactObject t) => t.tags.Contains(Tag.Buff)).ToList();
		}
		else
		{
			playerFullSpellList = spellDictionary.Values.Where((SpellObject t) => t.tags.Contains(Tag.Player)).ToList();
			playerSpellList = playerFullSpellList.Where((SpellObject t) => t.unlockLevel <= unCtrl.currentUnlockLevel).ToList();
			playerFullArtList = artDictionary.Values.Where((ArtifactObject t) => t.tags.Contains(Tag.Player) && !t.tags.Contains(Tag.Cpu) && !t.tags.Contains(Tag.Starting)).ToList();
			nonBaseArtList = playerFullArtList.Where((ArtifactObject t) => !t.tags.Contains(Tag.Base) && t.unlockLevel <= unCtrl.currentUnlockLevel).ToList();
			buffList = artDictionary.Values.Where((ArtifactObject t) => t.tags.Contains(Tag.Buff)).ToList();
			loopMutationList = artDictionary.Values.Where((ArtifactObject t) => t.tags.Contains(Tag.LoopMutation)).ToList();
		}
		if (runCtrl.ctrl.currentHeroObj != null)
		{
			if (!runCtrl.ctrl.currentHeroObj.tags.Contains(Tag.Attack))
			{
				nonBaseArtList = nonBaseArtList.Where((ArtifactObject t) => !t.tags.Contains(Tag.Attack)).ToList();
			}
			bossRewards = nonBaseArtList.Where((ArtifactObject t) => t.tags.Contains(Tag.Boss)).ToList();
			minibossRewards = nonBaseArtList.Where((ArtifactObject t) => t.tags.Contains(Tag.Miniboss)).ToList();
			nonBaseArtList = nonBaseArtList.Where((ArtifactObject t) => !t.tags.Contains(Tag.Boss) && !t.tags.Contains(Tag.Miniboss)).ToList();
			List<Tag> list = new List<Tag>
			{
				Tag.Saffron,
				Tag.Selicy,
				Tag.Shiso,
				Tag.Terra,
				Tag.Hazel,
				Tag.Violette,
				Tag.Gunner,
				Tag.Reva
			};
			foreach (Tag charTag in list)
			{
				if (!runCtrl.ctrl.currentHeroObj.tags.Contains(charTag))
				{
					nonBaseArtList = nonBaseArtList.Where((ArtifactObject t) => !t.tags.Contains(charTag)).ToList();
				}
			}
		}
		playerNonDuplicateNonBaseArtList = nonBaseArtList.Where((ArtifactObject t) => !t.tags.Contains(Tag.NoDuplicate)).ToList();
		foreach (Brand brand in brandTypes)
		{
			brandSpellLists[brand] = playerFullSpellList.Where((SpellObject t) => t.brand == brand).ToList();
		}
		cpuSpellList = spellDictionary.Values.Where((SpellObject t) => t.tags.Contains(Tag.Cpu)).ToList();
		weaponList = spellDictionary.Values.Where((SpellObject t) => t.tags.Contains(Tag.Weapon)).ToList();
		pvpSpellList = spellDictionary.Values.Where((SpellObject t) => t.tags.Contains(Tag.PvP)).ToList();
		unobtainedPlayerSpellList = new List<SpellObject>(playerSpellList);
		unobtainedNonBaseArts = new List<ArtifactObject>(nonBaseArtList);
		if (S.I.scene != GScene.DemoLive)
		{
			string[] names = Enum.GetNames(typeof(Effect));
			foreach (string spriteName in names)
			{
				GetSprite(spriteName);
			}
			string[] names2 = Enum.GetNames(typeof(FTrigger));
			foreach (string spriteName2 in names2)
			{
				GetSprite(spriteName2);
			}
		}
	}

	private void LoadEffectsLua()
	{
		if (xmlReader == null)
		{
			xmlReader = S.I.xmlReader;
		}
		EffectActions effectActions = new EffectActions(xmlReader.GetDataFilePath("Spells.lua"));
		effectActions.AddScript(xmlReader.GetDataFilePath("Lib.lua"));
		effectActions.AddScript(xmlReader.GetDataFilePath("Effects.lua"));
		effectActions.AddScript(xmlReader.GetDataFilePath("Artifacts.lua"));
		foreach (string luaMod in modCtrl.luaMods)
		{
			effectActions.AddScript(luaMod);
		}
	}

	private void CreateSpellObjectPrototypes()
	{
		spellDictionary = new Dictionary<string, SpellObject>(StringComparer.OrdinalIgnoreCase);
		ReadSpellFile(xmlReader.GetDataFile("Spells.xml"));
	}

	private void ReadSpellFile(string xmlString)
	{
		bool flag = false;
		Dictionary<Effect, int> dictionary = new Dictionary<Effect, int>();
		if (flag)
		{
			foreach (Effect value in Enum.GetValues(typeof(Effect)))
			{
				dictionary[value] = 0;
			}
		}
		string s = xmlReader.GetDataFile("Spells.xml");
		for (int i = -1; i < modCtrl.spellMods.Count; i++)
		{
			if (i != -1)
			{
				s = modCtrl.spellMods[i];
			}
			XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(s));
			int num = 0;
			if (!xmlTextReader.ReadToDescendant("Spells") || !xmlTextReader.ReadToDescendant("Spell"))
			{
				continue;
			}
			do
			{
				num++;
				SpellObject spellObject = new SpellObject();
				spellObject.ReadXmlPrototype(xmlTextReader);
				spellObject.type = ItemType.Spell;
				spellObject.spellObj = spellObject;
				if (spellObject.tags.Contains(Tag.SaffronBoss))
				{
					saffronBossSpellList.Add(spellObject);
				}
				if (spellObject.tags.Contains(Tag.Weapon))
				{
					spellObject.type = ItemType.Wep;
				}
				if (spellObject.tags.Contains(Tag.Cpu) && !sprites.ContainsKey(spellObject.itemID))
				{
					spellObject.sprite = null;
				}
				else
				{
					spellObject.sprite = GetSprite(spellObject.itemID);
				}
				if (flag)
				{
					foreach (EffectApp efApp in spellObject.efApps)
					{
						if (spellObject.tags.Contains(Tag.Player))
						{
							dictionary[efApp.effect]++;
						}
					}
				}
				spellDictionary[spellObject.itemID] = spellObject;
				itemDictionary[spellObject.itemID] = spellObject;
			}
			while (xmlTextReader.ReadToNextSibling("Spell"));
		}
		if (!flag)
		{
			return;
		}
		string text = "";
		foreach (Effect key in dictionary.Keys)
		{
			text = text + key.ToString() + "\t" + dictionary[key] + "\n";
		}
		Debug.Log(text);
	}

	private void CreateArtifactObjectPrototypes()
	{
		artDictionary = new Dictionary<string, ArtifactObject>(StringComparer.OrdinalIgnoreCase);
		string s = xmlReader.GetDataFile("Artifacts.xml");
		for (int i = -1; i < modCtrl.artMods.Count; i++)
		{
			if (i != -1)
			{
				s = modCtrl.artMods[i];
			}
			XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(s));
			int num = 0;
			if (xmlTextReader.ReadToDescendant("Artifacts") && xmlTextReader.ReadToDescendant("Artifact"))
			{
				do
				{
					num++;
					ArtifactObject artifactObject = new ArtifactObject();
					artifactObject.ReadXmlPrototype(xmlTextReader);
					artifactObject.sprite = GetSprite(artifactObject.itemID);
					artifactObject.type = ItemType.Art;
					artifactObject.artObj = artifactObject;
					artDictionary[artifactObject.itemID] = artifactObject;
					itemDictionary[artifactObject.itemID] = artifactObject;
				}
				while (xmlTextReader.ReadToNextSibling("Artifact"));
			}
		}
	}

	private void CreatePactObjectPrototypes()
	{
		pactChallengeList.Clear();
		pactRewardList.Clear();
		hellPasses.Clear();
		pactDictionary = new Dictionary<string, PactObject>(StringComparer.OrdinalIgnoreCase);
		string s = xmlReader.GetDataFile("Pacts.xml");
		for (int i = -1; i < modCtrl.pactMods.Count; i++)
		{
			if (i != -1)
			{
				s = modCtrl.pactMods[i];
			}
			XmlTextReader xmlTextReader = new XmlTextReader(new StringReader(s));
			int num = 0;
			if (!xmlTextReader.ReadToDescendant("Pacts") || !xmlTextReader.ReadToDescendant("Pact"))
			{
				continue;
			}
			do
			{
				num++;
				PactObject pactObject = new PactObject();
				pactObject.ReadXmlPrototype(xmlTextReader);
				pactObject.sprite = GetSprite(pactObject.itemID);
				pactObject.pactObj = pactObject;
				pactObject.type = ItemType.Pact;
				if (pactObject.tags.Contains(Tag.Challenge))
				{
					pactChallengeList.Add(pactObject);
				}
				else if (pactObject.tags.Contains(Tag.Reward))
				{
					pactRewardList.Add(pactObject);
				}
				else if (pactObject.tags.Contains(Tag.Hell))
				{
					hellPasses.Add(pactObject);
				}
				pactDictionary[pactObject.itemID] = pactObject;
				itemDictionary[pactObject.itemID] = pactObject;
			}
			while (xmlTextReader.ReadToNextSibling("Pact"));
		}
	}

	private void CreateCustomRuntimeAnimatorControllers()
	{
	}

	private void GenerateAnimClips()
	{
		foreach (SpellObject value in spellDictionary.Values)
		{
			animClipNames.Add(value.animWarning);
			animClipNames.Add(value.animObj);
			animClipNames.Add(value.animCast);
			animClipNames.Add(value.animShot);
			animClipNames.Add(value.animBlast);
			animClipNames.Add(value.animHit);
		}
		animClipNames = animClipNames.Where((string t) => !string.IsNullOrEmpty(t)).Distinct().ToList();
		GenerateAnimClipSprites();
		foreach (string animClipName in animClipNames)
		{
			CreateAnimClip(animClipName, true, 0.05f, animClipSprites[animClipName].ToArray());
		}
	}

	private void GenerateAnimClipSprites()
	{
		if (animClipSpritesGenerated)
		{
			return;
		}
		string error;
		Sprite[] array = AssetBundleManager.LoadAllAssets<Sprite>("sprites", out error);
		if (!string.IsNullOrEmpty(error))
		{
			Debug.Log(error);
		}
		foreach (string animClipName in animClipNames)
		{
			animClipSprites[animClipName] = new List<Sprite>();
		}
		string pattern = "^[A-Za-z0-9]+";
		Sprite[] array2 = array;
		foreach (Sprite sprite in array2)
		{
			string key = Regex.Match(sprite.name, pattern).ToString();
			if (animClipSprites.ContainsKey(key))
			{
				animClipSprites[key].Add(sprite);
			}
		}
		animClipSpritesGenerated = true;
	}

	public void CreateAnimClip(string name, bool loop, float keyFrameLength, Sprite[] sprites)
	{
		if (sprites.Length != 0)
		{
			if (keyFrameLength > 10f)
			{
				keyFrameLength /= 1000f;
			}
			List<float> list = new List<float>();
			for (int i = 0; i < sprites.Length; i++)
			{
				list.Add(i);
			}
			spriteAnimClips[name] = new SpriteAnimationClip(keyFrameLength, list.ToArray(), sprites.ToArray(), loop);
		}
	}

	public void AddDefaultEnemyArt(string itemID)
	{
		if (artDictionary.ContainsKey(itemID))
		{
			defaultEnemyArtList.Add(artDictionary[itemID]);
		}
		else
		{
			Debug.LogError("NO ARTIFACT FOR " + itemID);
		}
	}

	public List<string> GetItemIDs(int rarity = -1, int amount = -1, ItemType itemType = ItemType.Item)
	{
		List<string> list = new List<string>();
		switch (itemType)
		{
		case ItemType.Art:
			foreach (ArtifactObject unobtainedNonBaseArt in unobtainedNonBaseArts)
			{
				if (unobtainedNonBaseArt.rarity == rarity || rarity == -1)
				{
					list.Add(unobtainedNonBaseArt.itemID);
				}
			}
			break;
		case ItemType.Spell:
			foreach (SpellObject playerSpell in playerSpellList)
			{
				if (playerSpell.rarity == rarity || rarity == -1)
				{
					list.Add(playerSpell.itemID);
				}
			}
			break;
		default:
			foreach (ItemObject value in itemDictionary.Values)
			{
				if (value.rarity == rarity || rarity == -1)
				{
					list.Add(value.itemID);
				}
			}
			break;
		}
		List<string> list2 = new List<string>();
		if (amount > 0)
		{
			for (int i = 0; i < amount; i++)
			{
				int index = runCtrl.NextPsuedoRand(0, list.Count);
				if (list.Count <= 1)
				{
					index = 0;
				}
				list2.Add(list[index]);
				list.RemoveAt(index);
			}
		}
		return list2;
	}

	public List<ItemObject> GetUnlocks(int unlockLevel)
	{
		return unlocks.Where((ItemObject t) => t.unlockLevel == unlockLevel).ToList();
	}

	public ItemObject GetRandomSpell()
	{
		return GetItems(-1, 1, ItemType.Spell)[0];
	}

	public List<ItemObject> GetItems(int rarity = -1, int amountNeeded = -1, ItemType itemType = ItemType.Item, bool unique = false, Brand brand = Brand.None, List<ItemObject> bannedList = null)
	{
		List<ItemObject> list = new List<ItemObject>();
		switch (itemType)
		{
		case ItemType.Art:
			rarity = Mathf.Clamp(rarity, 0, 3);
			if (unobtainedNonBaseArts.Where((ArtifactObject t) => t.rarity == rarity).ToList().Count >= amountNeeded)
			{
				foreach (ArtifactObject item in unobtainedNonBaseArts.Where((ArtifactObject t) => t.rarity == rarity))
				{
					list.Add(item.Clone());
				}
				break;
			}
			foreach (ArtifactObject item2 in playerNonDuplicateNonBaseArtList.Where((ArtifactObject t) => t.rarity == rarity))
			{
				list.Add(item2.Clone());
			}
			break;
		case ItemType.Spell:
			if (brand != 0)
			{
				foreach (SpellObject item3 in brandSpellLists[brand])
				{
					if (item3.rarity == rarity || rarity == -1)
					{
						if (!unique)
						{
							list.Add(item3.Clone());
						}
						else if (unobtainedPlayerSpellList.Contains(item3))
						{
							list.Add(item3.Clone());
						}
					}
				}
				break;
			}
			if (unique && brand == Brand.None)
			{
				foreach (SpellObject unobtainedPlayerSpell in unobtainedPlayerSpellList)
				{
					if (unobtainedPlayerSpell.rarity == rarity || rarity == -1)
					{
						list.Add(unobtainedPlayerSpell.Clone());
					}
				}
				break;
			}
			foreach (SpellObject playerSpell in playerSpellList)
			{
				if (playerSpell.rarity == rarity || rarity == -1)
				{
					list.Add(playerSpell.Clone());
				}
			}
			break;
		default:
			foreach (ItemObject value in itemDictionary.Values)
			{
				if (value.rarity == rarity || rarity == -1)
				{
					list.Add(value);
				}
			}
			break;
		}
		List<string> bannedItemIDs = new List<string>();
		if (bannedList != null)
		{
			foreach (ItemObject banned in bannedList)
			{
				List<ItemObject> list2 = new List<ItemObject>();
				foreach (ItemObject item4 in list)
				{
					if (item4.itemID == banned.itemID)
					{
						list2.Add(item4);
					}
				}
				foreach (ItemObject item5 in list2)
				{
					list.Remove(item5);
				}
				bannedItemIDs.Add(banned.itemID);
			}
		}
		if (list.Count < amountNeeded)
		{
			if (itemType == ItemType.Art)
			{
				if (playerNonDuplicateNonBaseArtList.Where((ArtifactObject t) => t.rarity == rarity && !bannedItemIDs.Contains(t.itemID)).ToList().Count >= amountNeeded)
				{
					foreach (ArtifactObject item6 in playerNonDuplicateNonBaseArtList.Where((ArtifactObject t) => t.rarity == rarity && !bannedItemIDs.Contains(t.itemID)))
					{
						list.Add(item6.Clone());
					}
				}
			}
			else if (unobtainedPlayerSpellList.Where((SpellObject t) => (t.rarity == rarity && !bannedItemIDs.Contains(t.itemID)) || (rarity == -1 && !bannedItemIDs.Contains(t.itemID))).ToList().Count >= amountNeeded - list.Count)
			{
				foreach (SpellObject item7 in unobtainedPlayerSpellList.Where((SpellObject t) => (t.rarity == rarity && !bannedItemIDs.Contains(t.itemID)) || (rarity == -1 && !bannedItemIDs.Contains(t.itemID))))
				{
					list.Add(item7.Clone());
				}
			}
			else if (rarity - 1 >= 0 && unobtainedPlayerSpellList.Where((SpellObject t) => t.rarity == rarity - 1 && !bannedItemIDs.Contains(t.itemID)).ToList().Count >= amountNeeded - list.Count)
			{
				Debug.Log("Getting spell from a lower rarity list " + (rarity - 1));
				foreach (SpellObject item8 in unobtainedPlayerSpellList.Where((SpellObject t) => t.rarity == rarity - 1 && !bannedItemIDs.Contains(t.itemID)))
				{
					list.Add(item8.Clone());
				}
			}
			else if (rarity - 2 >= 0 && unobtainedPlayerSpellList.Where((SpellObject t) => t.rarity == rarity - 2 && !bannedItemIDs.Contains(t.itemID)).ToList().Count >= amountNeeded - list.Count)
			{
				Debug.Log("Getting spell from a lower rarity list " + (rarity - 2));
				foreach (SpellObject item9 in unobtainedPlayerSpellList.Where((SpellObject t) => t.rarity == rarity - 2 && !bannedItemIDs.Contains(t.itemID)))
				{
					list.Add(item9.Clone());
				}
			}
			else if (unobtainedPlayerSpellList.Where((SpellObject t) => !bannedItemIDs.Contains(t.itemID)).ToList().Count >= amountNeeded - list.Count)
			{
				Debug.Log("Getting spell from any rarity list (potential duplicate");
				foreach (SpellObject item10 in unobtainedPlayerSpellList.Where((SpellObject t) => !bannedItemIDs.Contains(t.itemID)))
				{
					list.Add(item10.Clone());
				}
			}
			else if (playerSpellList.Where((SpellObject t) => (t.rarity == rarity && !bannedItemIDs.Contains(t.itemID)) || (rarity == -1 && !bannedItemIDs.Contains(t.itemID))).ToList().Count >= amountNeeded - list.Count)
			{
				foreach (SpellObject item11 in playerSpellList.Where((SpellObject t) => (t.rarity == rarity && !bannedItemIDs.Contains(t.itemID)) || (rarity == -1 && !bannedItemIDs.Contains(t.itemID))))
				{
					Debug.Log("Getting spell from all player spell list (potential duplicate");
					list.Add(item11.Clone());
				}
			}
			else
			{
				list.Add(spellDictionary["Ragnarok"]);
			}
		}
		List<ItemObject> list3 = new List<ItemObject>();
		if (amountNeeded > 0)
		{
			for (int i = 0; i < amountNeeded; i++)
			{
				int index = runCtrl.NextPsuedoRand(0, list.Count);
				if (list.Count == 0)
				{
					return list3;
				}
				list3.Add(list[index]);
				list.RemoveAt(index);
			}
		}
		return list3;
	}
}
