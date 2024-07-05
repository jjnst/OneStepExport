using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;

[MoonSharpUserData]
public class SpawnCtrl : MonoBehaviour
{
	public SpriteRenderer aimMarkerPrefab;

	public Sprite aimMarkerSpriteAlt;

	public List<GameObject> allBosses;

	public List<Player> currentPlayers;

	public Being lastSpawnedBeing;

	public BeingStatsPanel beingStatsPanelPrefab;

	public Transform enemyNameGrid;

	public bool forceSpawn = false;

	public Int2 forceSpawnPos;

	public List<int> enemySpawnX;

	public List<int> enemySpawnY;

	public List<int> enemyChance;

	public List<string> enemyToSpawn;

	public List<string> bossesToSpawn;

	public int numEnemiesToSpawn;

	public float debrisChanceBase = 5f;

	public float lootChestChanceBase = 20f;

	public float lootChestChanceAdd = 0f;

	public float healChestChanceBase = 25f;

	public float healChestChanceAdd = 0f;

	public float hostageChanceBase = 50f;

	public float hostageChanceAdd = 0f;

	public float portalChanceBase = 50f;

	public float portalChanceAdd = 0f;

	public float cursedChestChanceBase = 50f;

	public float cursedChestChanceAdd = 0f;

	public List<Int2> playerSpawnPos;

	public List<Int2> pvPlayerSpawnPos;

	public List<Int2> coOpPlayerSpawnPos;

	public TMP_Text enemyNamePrefab;

	public GameObject spawnEffectPrefab;

	public GameObject shadowPrefab;

	public AudioClip hitSoundDefaultTiny;

	public AudioClip hitSoundDefaultLight;

	public AudioClip hitSoundDefault;

	public AudioClip hitSoundDefaultHeavy;

	public AudioClip hitSoundShieldTiny;

	public AudioClip hitSoundShield;

	public AudioClip hitSoundShieldBreak;

	public AudioClip hitSoundPlayerLight;

	public AudioClip hitSoundPlayer;

	public AudioClip hitSoundPlayerHeavy;

	public AudioClip hitSoundShieldPlayer;

	public AudioClip dieSoundPlayer;

	public TalkBox talkBubblePrefab;

	public BossIntroCtrl bossIntroCtrl;

	public int baseHurtboxSize = 20;

	public float dangerZoneHealthMultiplier = 1.2f;

	[NonSerialized]
	public BC ctrl;

	[NonSerialized]
	public IdleCtrl idCtrl;

	[NonSerialized]
	private ItemManager itemMan;

	[NonSerialized]
	public OptionCtrl optCtrl;

	[NonSerialized]
	public DeckCtrl deCtrl;

	[NonSerialized]
	private HeroSelectCtrl heCtrl;

	[NonSerialized]
	private ShopCtrl shopCtrl;

	[NonSerialized]
	private RunCtrl runCtrl;

	[NonSerialized]
	private MusicCtrl muCtrl;

	[NonSerialized]
	private PostCtrl poCtrl;

	[NonSerialized]
	public TI ti;

	public ExplosionGenerator explosionGen;

	public Dictionary<string, BeingObject> beingDictionary;

	public Dictionary<string, BeingObject> heroDictionary;

	public Dictionary<string, BeingObject> structureDictionary;

	public Dictionary<string, GameObject> bossDictionary;

	public List<BeingObject> treasureChests = new List<BeingObject>();

	public List<BeingObject> darkTreasureChests = new List<BeingObject>();

	public List<BeingObject> hazards = new List<BeingObject>();

	public List<BeingObject> hostages = new List<BeingObject>();

	public List<BeingObject> fairies = new List<BeingObject>();

	public PvPSelectCtrl pvpSelectCtrl;

	public List<BeingObject> heroCampaignList;

	public List<BeingObject> heroPvPList;

	public Shader defaultShader;

	public List<Shader> tierShaders;

	public Shader hitShader;

	public float hitFlashLength = 0.07f;

	public Shader secondPlayerShader;

	private Action<BeingObject> cbEnemyObjectCreated;

	private void Awake()
	{
		ctrl = S.I.batCtrl;
		idCtrl = S.I.idCtrl;
		itemMan = S.I.itemMan;
		optCtrl = S.I.optCtrl;
		deCtrl = S.I.deCtrl;
		heCtrl = S.I.heCtrl;
		runCtrl = S.I.runCtrl;
		shopCtrl = S.I.shopCtrl;
		muCtrl = S.I.muCtrl;
		poCtrl = S.I.poCtrl;
		ti = S.I.tiCtrl;
		beingDictionary = new Dictionary<string, BeingObject>(StringComparer.OrdinalIgnoreCase);
		heroDictionary = new Dictionary<string, BeingObject>(StringComparer.OrdinalIgnoreCase);
		structureDictionary = new Dictionary<string, BeingObject>(StringComparer.OrdinalIgnoreCase);
		StartCoroutine(_LoadHeroObjects());
	}

	public IEnumerator _LoadHeroObjects()
	{
		if (!SaveDataCtrl.Initialized)
		{
			yield return new WaitUntil(() => SaveDataCtrl.Initialized);
		}
		if (S.I.EDITION == Edition.Full || S.I.EDITION == Edition.QA)
		{
			while (!SteamManager.Initialized && Time.timeSinceLevelLoad < S.maxLoadTime)
			{
				yield return null;
			}
		}
		CreateBeingObjectPrototypes("Enemies.xml", BeingType.Enemy);
		CreateBeingObjectPrototypes("Structures.xml", BeingType.Structure);
		CreateBeingObjectPrototypes("Heroes.xml", BeingType.Hero);
		CreateBossDictionary();
		SetDefaultHeroes();
	}

	public void SetDefaultHeroes()
	{
		if (S.I.EDITION == Edition.DemoLive)
		{
			ctrl.currentHeroObj = heroDictionary[runCtrl.demoLiveHeroString].Clone();
		}
		else if (S.I.CAMPAIGN_MODE)
		{
			if (heroDictionary.ContainsKey(runCtrl.campaignHeroString))
			{
				ctrl.currentHeroObj = heroDictionary[runCtrl.campaignHeroString].Clone();
			}
			else
			{
				Debug.Log("No Hero Dictionary entry for " + runCtrl.campaignHeroString);
			}
		}
		else if (heroDictionary.ContainsKey(runCtrl.campaignHeroString))
		{
			ctrl.currentHeroObj = heroDictionary[runCtrl.defaultHeroString].Clone();
		}
		else
		{
			Debug.Log("No Hero Dictionary entry for " + runCtrl.defaultHeroString);
		}
	}

	public void CreateHeroObjects()
	{
		CreateBeingObjectPrototypes("Heroes.xml", BeingType.Hero);
	}

	public void CreateBeingObjectPrototypes(string fileName, BeingType type, bool mod = false, string filePath = "")
	{
		XmlTextReader xmlTextReader = null;
		xmlTextReader = ((!mod) ? new XmlTextReader(new StringReader(S.I.xmlReader.GetDataFile(fileName))) : new XmlTextReader(new StringReader(S.I.xmlReader.GetFile(filePath, fileName))));
		if (xmlTextReader.ReadToDescendant("Beings") && xmlTextReader.ReadToDescendant("Being"))
		{
			do
			{
				BeingObject beingObject = new BeingObject();
				beingObject.ReadXmlPrototype(xmlTextReader);
				beingObject.type = type;
				if (mod)
				{
					beingObject.tags.Add(Tag.Unlock);
				}
				switch (type)
				{
				case BeingType.Hero:
					heroDictionary[beingObject.beingID] = beingObject;
					break;
				case BeingType.Structure:
					if (beingObject.tags.Contains(Tag.Ally))
					{
						beingObject.type = BeingType.Ally;
					}
					structureDictionary[beingObject.beingID] = beingObject;
					break;
				}
				if (beingObject.tags.Contains(Tag.Treasure))
				{
					treasureChests.Add(beingObject);
				}
				if (beingObject.tags.Contains(Tag.DarkTreasure))
				{
					darkTreasureChests.Add(beingObject);
				}
				if (beingObject.tags.Contains(Tag.Hostage) && beingObject.tags.Contains(Tag.Distress))
				{
					hostages.Add(beingObject);
				}
				if (beingObject.tags.Contains(Tag.Hazard))
				{
					hazards.Add(beingObject);
				}
				if (beingObject.tags.Contains(Tag.Fairy))
				{
					beingObject.type = BeingType.Fairy;
					fairies.Add(beingObject);
				}
				beingDictionary[beingObject.beingID] = beingObject;
			}
			while (xmlTextReader.ReadToNextSibling("Being"));
		}
		heroCampaignList = heroDictionary.Values.Where((BeingObject t) => t.tags.Contains(Tag.Campaign) && t.tags.Contains(Tag.Default)).ToList();
		heroPvPList = heroDictionary.Values.Where((BeingObject t) => t.tags.Contains(Tag.PvP) && t.tags.Contains(Tag.Default)).ToList();
	}

	public void CreateBossDictionary()
	{
		bossDictionary = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
		foreach (GameObject allBoss in allBosses)
		{
			bossDictionary.Add(allBoss.GetComponent<Boss>().bossID, allBoss);
		}
	}

	public void CreatePlayers()
	{
		deCtrl.DestroyDuelDisks();
		BC.poisonMinimum = 0f;
		BC.poisonDuration = BC.poisonBaseDuration;
		if (heCtrl.gameMode == GameMode.Solo)
		{
			if (S.I.scene == GScene.SpellLoop)
			{
				Player player = SpawnPlayer(beingDictionary[ctrl.currentHeroObj.beingID].Clone(), playerSpawnPos[0].x, playerSpawnPos[0].y, 0, false);
				player.StartCoroutine(player.TestLoop());
			}
			else
			{
				Player player2 = SpawnPlayer(ctrl.currentHeroObj.Clone(), playerSpawnPos[0].x, playerSpawnPos[0].y, 0, true);
				player2.rewiredPlayer = RunCtrl.GetRewiredPlayer();
			}
		}
		else
		{
			ctrl.playerHalves.Clear();
			DuelDisk duelDisk = null;
			PlayerHalf playerHalf = null;
			for (int i = 0; i < 2; i++)
			{
				PlayerHalf component = SpawnPlayer(ctrl.currentHeroObj.Clone(), pvPlayerSpawnPos[0].x + i, pvPlayerSpawnPos[0].y + i, i, true, duelDisk).GetComponent<PlayerHalf>();
				component.rewiredPlayer = RunCtrl.GetRewiredPlayer(i);
				duelDisk = component.duelDisk;
				component.playerHalfNum = i;
				ctrl.playerHalves.Add(component);
				component.baseBeingObj.manaRegen *= 2f;
				if (i == 1)
				{
					component.otherHalf = playerHalf;
					playerHalf.otherHalf = component;
					component.defaultShader = secondPlayerShader;
					component.aimMarker.sprite = aimMarkerSpriteAlt;
					if (runCtrl.LoadedRunExists())
					{
						component.health.SetHealth(runCtrl.loadedRun.currentHealthPlayerTwo, runCtrl.loadedRun.maxHealth);
					}
				}
				playerHalf = component;
			}
		}
		deCtrl.statsScreen.UpdateStats();
	}

	public void CreatePvPPlayers()
	{
		deCtrl.DestroyDuelDisks();
		List<int> list = Utils.RandomList(itemMan.pvpSpellList.Count);
		for (int i = 0; i < 2; i++)
		{
			BeingObject beingObject = pvpSelectCtrl.currentDisplayedHeros[i].Clone();
			for (int j = 0; j < 8; j++)
			{
				beingObject.deck.Add(itemMan.pvpSpellList[list[j]].itemID);
			}
			Player player = SpawnPlayer(beingObject.Clone(), pvPlayerSpawnPos[i].x, pvPlayerSpawnPos[i].y, i, true);
			player.rewiredPlayer = RunCtrl.GetRewiredPlayer(i);
		}
		ctrl.currentPlayers[1].SetAlignNum(-1);
	}

	public void CleanPlayers()
	{
		foreach (Player currentPlayer in ctrl.currentPlayers)
		{
			currentPlayer.FullClean();
		}
		foreach (DuelDisk duelDisk in deCtrl.duelDisks)
		{
			duelDisk.ShowCardRefGrid(false);
		}
		ctrl.currentPlayers.Clear();
		if (ti.mainBattleGrid != null)
		{
			ti.mainBattleGrid.currentBeings.Clear();
			ti.mainBattleGrid.currentAllies.Clear();
		}
	}

	public void RestorePvPPlayers()
	{
		for (int i = 0; i < deCtrl.duelDisks.Count; i++)
		{
			Player player = SpawnPlayer(deCtrl.duelDisks[i].beingObj, pvPlayerSpawnPos[i].x, pvPlayerSpawnPos[i].y, i, true, deCtrl.duelDisks[i]);
			player.beingObj.money = heroDictionary[player.beingObj.beingID].money;
			player.rewiredPlayer = RunCtrl.GetRewiredPlayer(i);
		}
		ctrl.currentPlayers[1].SetAlignNum(-1);
	}

	public Being PlaceBeing(string beingID, Tile tile, int num = 0, bool stun = false, BattleGrid battleGrid = null)
	{
		if (battleGrid == null)
		{
			battleGrid = ti.mainBattleGrid;
		}
		if (tile.type == TileType.None || tile.type == TileType.Broken)
		{
			Debug.Log("Could not spawn " + beingID + ", tile was broken or empty. " + Time.frameCount);
			return null;
		}
		if (!beingDictionary.ContainsKey(beingID))
		{
			Debug.LogError("Being dict doesnt containt a proto for key: " + beingID);
		}
		BeingObject beingObject = beingDictionary[beingID].Clone();
		if (beingObject == null)
		{
			return null;
		}
		if (cbEnemyObjectCreated != null)
		{
			cbEnemyObjectCreated(beingObject);
		}
		return CreateBeing(beingObject, tile.x, tile.y, num, stun, null, true, battleGrid);
	}

	public Being CreateBeing(BeingObject beingObj, float x, float y, int num = 0, bool stun = false, DuelDisk duelDisk = null, bool spawnEffect = true, BattleGrid battleGrid = null, bool skipSoloArtifacts = false)
	{
		if (battleGrid == null)
		{
			battleGrid = ti.mainBattleGrid;
		}
		Tile tile = battleGrid.grid[Mathf.RoundToInt(x), Mathf.RoundToInt(y)];
		Quaternion rotation = base.transform.rotation;
		if (x >= (float)(battleGrid.gridLength / 2))
		{
			rotation *= Quaternion.AngleAxis(180f, Vector3.up);
		}
		if (spawnEffect)
		{
			GameObject obj = UnityEngine.Object.Instantiate(spawnEffectPrefab, tile.transform.position, rotation, base.transform);
			UnityEngine.Object.Destroy(obj, 1f);
		}
		GameObject gameObject = ((!bossDictionary.ContainsKey(beingObj.beingID)) ? new GameObject() : UnityEngine.Object.Instantiate(bossDictionary[beingObj.beingID], tile.transform.position, rotation));
		gameObject.name = beingObj.beingID;
		gameObject.transform.position = tile.transform.position;
		gameObject.transform.rotation = rotation;
		gameObject.transform.SetParent(base.transform, true);
		SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sortingLayerName = "ProjChar";
		spriteRenderer.material.shader = defaultShader;
		string text = beingObj.beingID;
		if (text.Contains("2"))
		{
			text = text.Replace("2", "");
		}
		BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
		boxCollider2D.size = Vector2.one * baseHurtboxSize;
		boxCollider2D.isTrigger = true;
		if (!bossDictionary.ContainsKey(beingObj.beingID))
		{
			if (beingObj.type == BeingType.Enemy)
			{
				gameObject.AddComponent<Enemy>();
			}
			else if (beingObj.type == BeingType.Ally)
			{
				gameObject.AddComponent<Ally>();
			}
			else if (beingObj.type == BeingType.Structure)
			{
				gameObject.AddComponent<Structure>();
			}
			else if (beingObj.type == BeingType.Hero)
			{
				if (heCtrl.gameMode == GameMode.CoOp && battleGrid == ti.mainBattleGrid)
				{
					gameObject.AddComponent<PlayerHalf>();
					if (num == 1)
					{
						skipSoloArtifacts = true;
					}
				}
				else
				{
					gameObject.AddComponent<Player>();
				}
			}
			else
			{
				Debug.LogError("NO/INCORRECT BEING TYPE : beingObj.type");
			}
		}
		Being component = gameObject.GetComponent<Being>();
		Cpu component2 = gameObject.GetComponent<Cpu>();
		component.battleGrid = battleGrid;
		component.hitSoundTiny = hitSoundDefaultTiny;
		component.hitSoundLight = hitSoundDefaultLight;
		component.hitSound = hitSoundDefault;
		component.hitSoundHeavy = hitSoundDefaultHeavy;
		component.hitSoundShieldTiny = hitSoundShieldTiny;
		component.hitSoundShield = hitSoundShield;
		component.hitSoundShieldBreak = hitSoundShieldBreak;
		component.gunPoint = new GameObject().transform;
		component.gunPoint.SetParent(gameObject.transform, true);
		component.gunPoint.localPosition = beingObj.localGunPointPos;
		component.gunPoint.name = "GunPoint";
		component.beingObj = beingObj;
		if (!itemMan.animations.ContainsKey(text) && !itemMan.animations.ContainsKey(beingObj.animName))
		{
			Debug.LogWarning("Animations does not contain key of " + text + " or " + beingObj.animName);
		}
		component.anim = gameObject.AddComponent<Animator>();
		if (string.IsNullOrEmpty(beingObj.animName))
		{
			component.SetAnimatorController(itemMan.GetAnim(text));
		}
		else if (itemMan.animations.ContainsKey(beingObj.animName))
		{
			component.SetAnimatorController(itemMan.GetAnim(beingObj.animName));
		}
		else
		{
			component.sprAnim = gameObject.AddComponent<SpriteAnimator>();
			component.sprAnim.enabled = false;
			component.animOverrider = gameObject.AddComponent<AnimationOverrider>();
			component.animOverrider.Set(component.sprAnim, component.anim, beingObj.animName, itemMan);
			if (string.IsNullOrEmpty(beingObj.animBase))
			{
				component.anim.runtimeAnimatorController = ctrl.baseCharacterAnim;
			}
			else
			{
				component.anim.runtimeAnimatorController = ctrl.baseMonsterAnim;
			}
		}
		component.anim.logWarnings = false;
		component.anim.SetTrigger("spawn");
		component.hitShader = hitShader;
		component.mov = gameObject.AddComponent<Moveable>();
		component.mov.ti = ti;
		component.mov.battleGrid = battleGrid;
		component.mov.lerpTime = beingObj.lerpTime;
		component.mov.baseLerpTime = beingObj.lerpTime;
		component.mov.currentTile = tile;
		component.mov.being = component;
		component.mov.unmoveable = beingObj.unmoveable;
		component.dieSound = ctrl.itemMan.GetAudioClip(beingObj.dieSound);
		component.talkBubble = UnityEngine.Object.Instantiate(talkBubblePrefab, component.transform.position, component.transform.rotation);
		component.talkBubble.Set(component);
		component.shadow = UnityEngine.Object.Instantiate(shadowPrefab, component.transform.position, component.transform.rotation, component.transform);
		if (!beingObj.shadow)
		{
			component.shadow.SetActive(false);
		}
		component.health = gameObject.AddComponent<Health>();
		if ((bool)component2)
		{
			component2.spawnNum = num;
			if (deCtrl.itemMan.spellDictionary.ContainsKey(beingObj.beingID))
			{
				component2.spellObjList.Add(deCtrl.CreateSpellBase(beingObj.beingID, component));
			}
		}
		foreach (string item in beingObj.deck)
		{
			if (!component.GetComponent<Player>())
			{
				component.spellObjList.Add(deCtrl.CreateSpellBase(item, component));
			}
		}
		foreach (string startup in beingObj.startups)
		{
			component.startups.Add(deCtrl.CreateSpellBase(startup, component));
		}
		foreach (string timeout in beingObj.timeouts)
		{
			component.timeouts.Add(deCtrl.CreateSpellBase(timeout, component));
		}
		foreach (string deathrattle in beingObj.deathrattles)
		{
			component.deathrattles.Add(deCtrl.CreateSpellBase(deathrattle, component));
		}
		foreach (string clearSpell in beingObj.clearSpells)
		{
			component.clearSpells.Add(deCtrl.CreateSpellBase(clearSpell, component));
		}
		component.spellAppObj = deCtrl.CreateSpellBase("Default", component);
		foreach (EffectApp efApp in beingObj.efApps)
		{
			component.spellAppObj.efApps.Add(efApp);
		}
		BeingStatsPanel beingStatsPanel = UnityEngine.Object.Instantiate(beingStatsPanelPrefab);
		beingStatsPanel.SetBeing(component);
		component.health.text = beingStatsPanel.healthDisplay;
		beingObj.RegisterOnChangedCallback(OnInstalledObjectChanged);
		component.Setup();
		if ((bool)component.GetComponent<Enemy>())
		{
			CreateEnemy(component2);
		}
		else if ((bool)component.GetComponent<Ally>())
		{
			CreateAlly(component2);
		}
		else if ((bool)component.GetComponent<Structure>())
		{
			CreateStructure(component2);
		}
		else if ((bool)component.GetComponent<Player>())
		{
			CreatePlayer(component, duelDisk);
		}
		foreach (ArtData artifact in beingObj.artifacts)
		{
			if (!skipSoloArtifacts)
			{
				deCtrl.CreateArtifact(artifact, component);
			}
			else if (!itemMan.artDictionary[artifact.itemID].tags.Contains(Tag.Solo))
			{
				deCtrl.CreateArtifact(artifact, component);
			}
		}
		if (stun)
		{
			component.mov.SetState(State.Stunned);
		}
		else
		{
			component.Activate();
		}
		return component;
	}

	private void OnInstalledObjectChanged(BeingObject enemyObj)
	{
		Debug.Log("On installed object changed not implemented");
	}

	public IEnumerator SpawnZoneC(ZoneType zoneType)
	{
		switch (zoneType)
		{
		case ZoneType.Battle:
			muCtrl.PlayBattle();
			ti.mainBattleGrid.currentEnemies.Clear();
			SpawnBattleZone();
			SpawnEnvironment();
			break;
		case ZoneType.Miniboss:
			muCtrl.PlayBattle(1);
			ti.mainBattleGrid.currentEnemies.Clear();
			SpawnBattleZone();
			SpawnEnvironment();
			break;
		case ZoneType.Boss:
			SpawnBoss();
			break;
		case ZoneType.Shop:
			muCtrl.ResumeIntroLoop();
			idCtrl.MakeWorldBarAvailable();
			ctrl.GameState = GState.Idle;
			SpawnShopZone(zoneType);
			break;
		case ZoneType.DarkShop:
			muCtrl.ResumeIntroLoop();
			idCtrl.MakeWorldBarAvailable();
			ctrl.GameState = GState.Idle;
			SpawnShopZone(zoneType);
			break;
		case ZoneType.Danger:
			muCtrl.PlayBattle(1);
			ti.mainBattleGrid.currentEnemies.Clear();
			SpawnHazards();
			SpawnBattleZone();
			SpawnEnvironment();
			break;
		case ZoneType.Distress:
			muCtrl.PlayBattle();
			SpawnDistressZone();
			break;
		case ZoneType.Campsite:
			muCtrl.ResumeIntroLoop();
			idCtrl.MakeWorldBarAvailable();
			ctrl.GameState = GState.Idle;
			SpawnCampsiteZone();
			break;
		case ZoneType.Treasure:
			muCtrl.ResumeIntroLoop();
			SpawnTreasureZone();
			break;
		case ZoneType.Random:
			Debug.LogError("This should never happen? Because random should be set to something else");
			break;
		}
		yield return null;
	}

	public void SpawnBoss(int xPos = -1, int yPos = -1)
	{
		runCtrl.currentZoneDot.type = ZoneType.Boss;
		ti.mainBattleGrid.currentEnemies.Clear();
		List<Tile> list = ti.mainBattleGrid.Get(new TileApp(Location.RandEnemyUnique, Shape.Default, Pattern.Unoccupied), 0);
		int index = runCtrl.NextPsuedoRand(0, list.Count);
		if (xPos == -1)
		{
			xPos = list[index].x;
		}
		if (yPos == -1)
		{
			yPos = list[index].y;
		}
		if (forceSpawn)
		{
			xPos = forceSpawnPos.x;
			yPos = forceSpawnPos.y;
		}
		Boss component = PlaceBeing(bossesToSpawn[0], ti.mainBattleGrid.grid[xPos, yPos], 0, true).GetComponent<Boss>();
		component.ti = ti;
		component.tier = Mathf.RoundToInt(runCtrl.currentRun.worldTierNum / 2);
		if (!runCtrl.currentRun.pacifist)
		{
			muCtrl.PlayCharacterTheme(component.beingObj.nameString);
		}
		list.RemoveAt(index);
	}

	public void SpawnEnvironment()
	{
		if (S.I.dontSpawnAnything)
		{
			return;
		}
		List<Tile> list = ti.mainBattleGrid.Get(new TileApp(Location.RandUnique, Shape.Default, Pattern.Unoccupied), 0, null, null, true);
		if (!ctrl.currentPlayer)
		{
			list.Remove(ti.mainBattleGrid.grid[playerSpawnPos[0].x, playerSpawnPos[0].y]);
		}
		if (heCtrl.gameMode == GameMode.CoOp)
		{
			if (ctrl.currentPlayers.Count > 1)
			{
				list.Remove(ctrl.currentPlayers[1].mov.currentTile);
			}
			else
			{
				list.Remove(ti.mainBattleGrid.grid[coOpPlayerSpawnPos[1].x, coOpPlayerSpawnPos[1].y]);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].x != ctrl.currentPlayer.mov.currentTile.x && list[i].y != ctrl.currentPlayer.mov.currentTile.y && (float)runCtrl.NextPsuedoRand(1, 100) <= debrisChanceBase)
			{
				PlaceBeing("Debris", list[i], 0, true);
			}
		}
		list = ti.mainBattleGrid.Get(new TileApp(Location.RandEnemyUnique, Shape.Default, Pattern.Unoccupied), 0, null, null, true);
		if (!ctrl.currentPlayer)
		{
			list.Remove(ti.mainBattleGrid.GetTile(playerSpawnPos[0]));
		}
		if (heCtrl.gameMode == GameMode.CoOp)
		{
			if (ctrl.currentPlayers.Count > 1)
			{
				list.Remove(ctrl.currentPlayers[1].mov.currentTile);
			}
			else
			{
				list.Remove(ti.mainBattleGrid.GetTile(coOpPlayerSpawnPos[1]));
			}
		}
		int num = 0;
		if (runCtrl.currentRun != null && runCtrl.currentRun.hellPasses.Contains(2))
		{
			num = 10;
		}
		for (int j = 0; j < list.Count; j++)
		{
			if ((float)runCtrl.NextPsuedoRand(1, 100 - num * 2) <= debrisChanceBase && poCtrl.CombinedLuck() > (float)runCtrl.NextPsuedoRand(10, 50 - num))
			{
				List<BeingObject> beingsWithTags = GetBeingsWithTags(runCtrl.currentWorld.tags);
				if (beingsWithTags.Count > 0)
				{
					int index = runCtrl.NextPsuedoRand(0, beingsWithTags.Count);
					PlaceBeing(beingsWithTags[index].beingID, list[j], 0, true);
				}
			}
		}
	}

	public List<BeingObject> GetBeingsWithTags(List<Tag> tags)
	{
		List<BeingObject> list = new List<BeingObject>();
		foreach (BeingObject value in structureDictionary.Values)
		{
			if (Utils.SharesTags(tags, value.tags))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public void SpawnBattleZone()
	{
		if (S.I.TESTING_MODE)
		{
			SpawnStaticEnemies();
			S.I.dontSpawnAnything = true;
		}
		else
		{
			SpawnEnemySet();
		}
		if (S.I.dontSpawnAnything)
		{
			return;
		}
		if ((float)runCtrl.NextPsuedoRand(1, 100) < lootChestChanceBase + lootChestChanceAdd || S.I.spawnChest)
		{
			string beingID = "Chestcommon";
			int num = runCtrl.NextPsuedoRand(0, 102);
			if (num > 100)
			{
				beingID = "Chestlegendary";
			}
			else if (num > 75)
			{
				beingID = "Chestepic";
			}
			else if (num > 50)
			{
				beingID = "Chestrare";
			}
			SpawnCpu(beingID);
		}
		if (((float)runCtrl.NextPsuedoRand(1, 100) < healChestChanceBase + healChestChanceAdd || S.I.spawnHealChest) && ctrl.currentPlayer.health.max != ctrl.currentPlayer.health.current)
		{
			SpawnCpu("Chestheal");
		}
		if ((float)runCtrl.NextPsuedoRand(1, 100) < hostageChanceBase + hostageChanceAdd || S.I.spawnHostage)
		{
			SpawnCpu("HostageBattle");
			if (runCtrl.currentRun.evilHostages && runCtrl.NextPsuedoRand(1, 100) < 50)
			{
				SpawnCpu("HostageEvil");
			}
		}
	}

	public void SpawnDistressZone()
	{
		int num = 1;
		if (ctrl.currentPlayer.mov.currentTile.y == 1)
		{
			num = 2;
		}
		if (Utils.RandomBool(3))
		{
			if (num == 1)
			{
				num = 3;
			}
			PlaceBeing("Rapidcannon", ti.mainBattleGrid.grid[6, num - 2], 0, true);
			SpawnRandomHostage(0, num - 2, 1);
			PlaceBeing("Rapidcannon", ti.mainBattleGrid.grid[6, num], 0, true);
			SpawnRandomHostage(0, num, 2);
		}
		else if (Utils.RandomBool(2))
		{
			PlaceBeing("Chargecannon", ti.mainBattleGrid.grid[6, num], 0, true);
			SpawnRandomHostage(0, num, 1);
		}
		else
		{
			PlaceBeing("Exploder", ti.mainBattleGrid.grid[5, num], 0, true);
			SpawnRandomHostage(4, num, 1);
		}
	}

	private void SpawnRandomHostage(int x, int y, int spawnNum)
	{
		string randomHostageID = GetRandomHostageID();
		if (beingDictionary[randomHostageID].type == BeingType.Enemy && x < ti.mainBattleGrid.gridLength / 2)
		{
			x = Mathf.Clamp(x, 4, ti.mainBattleGrid.gridLength);
		}
		PlaceBeing(randomHostageID, ti.mainBattleGrid.grid[x, y], spawnNum, true);
	}

	private string GetRandomHostageID()
	{
		if (!runCtrl.currentRun.evilHostages)
		{
			return hostages[runCtrl.NextPsuedoRand(0, hostages.Count)].beingID;
		}
		if (runCtrl.NextPsuedoRand(1, 100) < 50)
		{
			return hostages[runCtrl.NextPsuedoRand(0, hostages.Count)].beingID;
		}
		return "HostageEvil";
	}

	public void SpawnHazards()
	{
		if (S.I.dontSpawnAnything)
		{
			return;
		}
		SpawnSerapiles(true);
		List<Tile> list = ti.mainBattleGrid.Get(new TileApp(Location.RandUnique, Shape.Default, Pattern.Unoccupied), 0, null, null, true);
		for (int i = 0; i < list.Count; i++)
		{
			if ((bool)ctrl.currentPlayer || list[i].x != playerSpawnPos[0].x || list[i].y != playerSpawnPos[0].y)
			{
				int index = runCtrl.NextPsuedoRand(0, hazards.Count);
				PlaceBeing(hazards[index].beingID, list[i], 0, true);
				break;
			}
		}
	}

	public void SpawnSerapiles(bool timed)
	{
		if (S.I.EDITION == Edition.DemoLive)
		{
			return;
		}
		string beingID = "Serapile";
		if (timed)
		{
			beingID = "SerapileTimed";
		}
		for (int i = 0; i < 3; i++)
		{
			if (i <= 1 || poCtrl.CombinedLuck() > (float)runCtrl.NextPsuedoRand(40, 70))
			{
				PlaceBeing(beingID, ti.mainBattleGrid.Get(new TileApp(Location.Index, Shape.Column, Pattern.Unoccupied, 8), 0, null, null, true)[0], 0, true);
			}
		}
	}

	public void SpawnEnemySet()
	{
		List<Tile> list = ti.mainBattleGrid.Get(new TileApp(Location.RandEnemyUnique, Shape.Default, Pattern.Unoccupied), 0, null, null, true);
		List<int> list2 = new List<int>();
		float num = poCtrl.DifficultyLuck();
		foreach (string item in enemyToSpawn)
		{
			list2.Add(0);
		}
		if (list2.Count > 0)
		{
			while (num >= 10f)
			{
				num -= 10f;
				list2 = list2.OrderBy((int w) => w).ToList();
				list2[0]++;
			}
		}
		for (int i = 0; i < enemyToSpawn.Count && (runCtrl.currentRun.worldTierNum >= 2 || !S.I.CAMPAIGN_MODE || S.I.EDITION == Edition.DemoLive || runCtrl.currentHellPassNum >= 1 || i < 2); i++)
		{
			int index = runCtrl.NextPsuedoRand(0, list.Count);
			int num2 = list[index].x;
			int num3 = list[index].y;
			if (forceSpawn && i == 0)
			{
				num2 = forceSpawnPos.x;
				num3 = forceSpawnPos.y;
			}
			if (S.I.RECORD_MODE)
			{
				if (enemySpawnX.Count > i)
				{
					num2 = enemySpawnX[i];
				}
				if (enemySpawnY.Count > i)
				{
					num3 = enemySpawnY[i];
					Debug.Log(num2 + " " + num3);
				}
			}
			string text = enemyToSpawn[i];
			string s = text.Substring(text.Length - 1);
			int result = 1;
			if (int.TryParse(s, out result))
			{
				text = text.Remove(text.Length - 1);
			}
			else
			{
				result = 1;
			}
			if (!beingDictionary.ContainsKey(text))
			{
				Debug.LogError("Being Dictionary does not contain key for " + text);
			}
			BeingObject beingObject = beingDictionary[text];
			int num4 = 0;
			if (S.I.scene != GScene.DemoLive && S.I.CAMPAIGN_MODE)
			{
				if (runCtrl.currentRun.worldTierNum > 1 && list2.Count == 1 && list2[0] == 0)
				{
					list2[0] = 1;
				}
				num4 += list2[i];
			}
			if (runCtrl.currentRun.dark)
			{
				num4++;
			}
			int num5 = Mathf.Clamp(result + num4, 0, 4);
			text += num5;
			if (!beingDictionary.ContainsKey(text))
			{
				text = enemyToSpawn[i];
				num5 = 1;
			}
			Being being = PlaceBeing(text, ti.mainBattleGrid.grid[num2, num3], i, true);
			being.beingObj.experience = Mathf.RoundToInt((float)beingObject.experience + (float)(beingObject.experience * (num5 - 1)) / 3f);
			being.beingObj.money = Mathf.RoundToInt((float)beingObject.money + (float)(beingObject.money * (num5 - 1)) / 3f);
			if (num5 - 2 >= 0)
			{
				being.defaultShader = tierShaders[num5 - 2];
			}
			list.RemoveAt(index);
		}
	}

	private Cpu SpawnCpu(string beingID)
	{
		List<Tile> list = ti.mainBattleGrid.Get(new TileApp(Location.RandEnemyUnique, Shape.Default, Pattern.Unoccupied), 0, null, null, true);
		if (list.Count > 0)
		{
			int index = runCtrl.NextPsuedoRand(0, list.Count);
			return PlaceBeing(beingID, ti.mainBattleGrid.grid[list[index].x, list[index].y], 0, true).GetComponent<Cpu>();
		}
		return null;
	}

	public void SpawnCampsiteZone()
	{
		SpawnCpu("Campfire").Activate();
		SpawnCpu("Bunny").Activate();
		if (runCtrl.currentRun.HasAssist("BossGunner"))
		{
			SpawnCpu("GunnerCampfire").Activate();
		}
		if (runCtrl.currentRun.HasAssist("BossShiso"))
		{
			SpawnCpu("ShisoCampfire").Activate();
		}
	}

	public void SpawnTreasureZone()
	{
		SpawnSerapiles(false);
		bool flag = false;
		if (runCtrl.currentRun.dark)
		{
			for (int i = 0; i < 2; i++)
			{
				if (i == 0 || poCtrl.DifficultyLuck() > (float)runCtrl.NextPsuedoRand(50, 100))
				{
					if (!flag)
					{
						int index = runCtrl.NextPsuedoRand(0, darkTreasureChests.Count);
						SpawnCpu(darkTreasureChests[index].beingID).Activate();
					}
					else
					{
						int index2 = runCtrl.NextPsuedoRand(0, treasureChests.Count);
						SpawnCpu(treasureChests[index2].beingID).Activate();
					}
				}
			}
			return;
		}
		for (int j = 0; j < 2; j++)
		{
			if (j == 0 || poCtrl.DifficultyLuck() > (float)runCtrl.NextPsuedoRand(50, 100))
			{
				int index3 = runCtrl.NextPsuedoRand(0, treasureChests.Count);
				SpawnCpu(treasureChests[index3].beingID).Activate();
			}
		}
	}

	public void SpawnShopZone(ZoneType zoneType)
	{
		if (runCtrl.currentRun.dark)
		{
			zoneType = ZoneType.DarkShop;
		}
		shopCtrl.SetShopkeeper(SpawnCpu("BossShopkeeper").GetComponent<Boss>(), zoneType, true);
		shopCtrl.currentShopkeeper.Activate();
	}

	public void CreateAlly(Cpu newCpu)
	{
		newCpu.mov.currentTile.SetOccupation(1, newCpu.mov.hovering);
		Ally component = newCpu.GetComponent<Ally>();
		newCpu.battleGrid.currentAllies.Add(component);
		newCpu.battleGrid.currentBeings.Add(component);
	}

	public void CreateStructure(Cpu newCpu)
	{
		newCpu.mov.currentTile.SetOccupation(2, newCpu.mov.hovering);
		Structure component = newCpu.GetComponent<Structure>();
		component.SetAlignNum(0);
		if (component.beingObj.mustDestroy)
		{
			newCpu.battleGrid.currentObstacles.Add(component);
		}
		newCpu.battleGrid.currentStructures.Add(component);
		newCpu.battleGrid.currentBeings.Add(component);
		component.health.SetHealth(component.beingObj.health);
	}

	public void CreatePlayer(Being newBeing, DuelDisk duelDisk = null)
	{
		Player component = newBeing.GetComponent<Player>();
		component.player = component;
		component.transform.name += ctrl.currentPlayers.Count;
		component.mov.currentTile.SetOccupation(1, newBeing.mov.hovering);
		component.mov.overrideMovement = newBeing.beingObj.overrideMovement;
		component.SetAlignNum(1);
		if (beingDictionary.ContainsKey(newBeing.beingObj.animName))
		{
			newBeing.gunPoint.localPosition = beingDictionary[newBeing.beingObj.animName].localGunPointPos;
			newBeing.mov.overrideMovement = beingDictionary[newBeing.beingObj.animName].overrideMovement;
			newBeing.beingObj.petAnim = beingDictionary[newBeing.beingObj.animName].petAnim;
			newBeing.beingObj.tauntAnim = beingDictionary[newBeing.beingObj.animName].tauntAnim;
		}
		component.hitSoundLight = hitSoundPlayerLight;
		component.hitSound = hitSoundPlayer;
		component.hitSoundHeavy = hitSoundPlayerHeavy;
		component.hitSoundShield = hitSoundShieldPlayer;
		component.dieSound = dieSoundPlayer;
		component.gracePeriodDuration = component.beingObj.gracePeriod;
		SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate(aimMarkerPrefab, Vector3.right * 160f, base.transform.rotation);
		spriteRenderer.transform.SetParent(component.transform, false);
		component.aimMarker = spriteRenderer;
		if (optCtrl.settingsPane.aimMarkerEnabled == 0 || runCtrl.currentHellPasses.Contains(3))
		{
			component.aimMarker.color = Color.clear;
		}
		if (component.equippedWep == null)
		{
			deCtrl.EquipWep(component.beingObj.weapon, component);
		}
		component.basicCooldown = component.beingObj.basicCooldown;
		component.health.SetHealth(component.beingObj.health, component.beingObj.maxHealthBase);
		if (S.I.scene != GScene.Idle)
		{
			component.mov.SetState(State.Stunned);
		}
	}

	public Player SpawnPlayer(BeingObject heroObj, int spawnX, int spawnY, int num, bool stun, DuelDisk duelDisk = null, BattleGrid battleGrid = null, bool reference = false)
	{
		Player component = CreateBeing(heroObj.Clone(), spawnX, spawnY, num, true, duelDisk, true, battleGrid).GetComponent<Player>();
		component.baseBeingObj = heroObj.Clone();
		if (!reference)
		{
			ctrl.currentPlayer = component;
			ctrl.currentPlayers.Add(component);
			component.battleGrid.currentAllies.Add(component);
			component.battleGrid.currentBeings.Add(component);
		}
		if (duelDisk == null)
		{
			component.duelDisk = deCtrl.CreatePlayerDuelDisk(component, reference);
			if (!reference)
			{
				deCtrl.SetupPlayerDeck(component);
			}
		}
		else
		{
			duelDisk.AddPlayer(component);
		}
		if (!reference)
		{
			deCtrl.deckScreen.foCtrl.SetStartingBrands(component.beingObj.startingBrands);
			if (heCtrl.gameMode == GameMode.PvP)
			{
				deCtrl.CreatePlayerItems(component, num);
			}
			else
			{
				deCtrl.CreatePlayerItems(component, 0, num);
			}
		}
		if (num < 1 || heCtrl.gameMode == GameMode.PvP)
		{
			component.duelDisk.Setup(ctrl.pvpMode, component);
		}
		deCtrl.statsScreen.UpdateStats(component);
		if (runCtrl.LoadedRunExists() && !reference)
		{
			runCtrl.currentRun.removals = runCtrl.loadedRun.removals;
			runCtrl.currentRun.upgraders = runCtrl.loadedRun.upgraders;
			shopCtrl.sera = runCtrl.loadedRun.sera;
			component.health.SetHealth(runCtrl.loadedRun.currentHealth, runCtrl.loadedRun.maxHealth);
			for (int i = 0; i < 2; i++)
			{
				runCtrl.foCtrl.focusNum = i;
				runCtrl.foCtrl.SetFocusedBrand(runCtrl.currentRun.focuses[i]);
			}
		}
		if (ctrl.optCtrl.settingsPane.angelModeEnabled == 1)
		{
			component.CreateStatusDisplay(Status.Blessed);
		}
		return component;
	}

	public void CreateEnemy(Cpu newCpu)
	{
		newCpu.mov.currentTile.SetOccupation(1, newCpu.mov.hovering);
		Enemy component = newCpu.GetComponent<Enemy>();
		component.SetAlignNum(-1);
		component.battleGrid.currentEnemies.Add(component);
		newCpu.battleGrid.currentBeings.Add(component);
		newCpu.battleGrid.currentObstacles.Add(component);
		TMP_Text tMP_Text = UnityEngine.Object.Instantiate(enemyNamePrefab);
		tMP_Text.transform.SetParent(enemyNameGrid, true);
		tMP_Text.text = component.beingObj.localizedName;
		component.nameText = tMP_Text;
		if (runCtrl.currentRun == null)
		{
			component.health.SetHealth(component.beingObj.health);
			return;
		}
		if (newCpu.beingObj.scaleHP)
		{
			int num = component.beingObj.health + component.beingObj.health / 8 * runCtrl.currentRun.worldTierNum;
			num *= Mathf.FloorToInt(dangerZoneHealthMultiplier);
			num += Mathf.RoundToInt((float)num * 0.33f * (float)runCtrl.currentRun.loopNum * 10f) / 10;
			newCpu.beingObj.defense += runCtrl.currentRun.loopNum * 2;
			component.health.SetHealth(num);
		}
		else
		{
			component.health.SetHealth(component.beingObj.health);
		}
		int num2 = 0;
		if (poCtrl.DifficultyLuck() > 25f)
		{
			num2++;
		}
		if (poCtrl.DifficultyLuck() > 35f)
		{
			num2++;
		}
		if (runCtrl.currentRun.hellPasses.Contains(1))
		{
			num2++;
		}
		if (runCtrl.currentRun.hellPasses.Contains(6))
		{
			num2++;
		}
		if ((bool)runCtrl.currentZoneDot && runCtrl.currentZoneDot.type == ZoneType.Distress)
		{
			num2 = 0;
		}
		if (!component.beingObj.tags.Contains(Tag.Structure))
		{
			for (int i = 0; i < num2; i++)
			{
				component.buffs.Add(deCtrl.CreateArtifact(itemMan.buffList[runCtrl.NextPsuedoRand(0, itemMan.buffList.Count)].itemID, component));
			}
			for (int j = 0; j < runCtrl.currentRun.loopNum; j++)
			{
				component.buffs.Add(deCtrl.CreateArtifact(itemMan.loopMutationList[runCtrl.NextPsuedoRand(0, itemMan.loopMutationList.Count)].itemID, component));
			}
		}
		foreach (ArtifactObject defaultEnemyArt in itemMan.defaultEnemyArtList)
		{
			component.buffs.Add(deCtrl.CreateArtifact(defaultEnemyArt.itemID, component));
		}
		lastSpawnedBeing = component;
		if (newCpu.battleGrid == ti.mainBattleGrid)
		{
			deCtrl.TriggerAllArtifacts(FTrigger.OnEnemySpawn);
		}
	}

	public void ResetChances()
	{
		lootChestChanceAdd = 0f;
		healChestChanceAdd = 0f;
	}

	public void RegisterEnemyObjectCreated(Action<BeingObject> callbackfunc)
	{
		cbEnemyObjectCreated = (Action<BeingObject>)Delegate.Combine(cbEnemyObjectCreated, callbackfunc);
	}

	public void UnregisterEnemyObjectCreated(Action<BeingObject> callbackfunc)
	{
		cbEnemyObjectCreated = (Action<BeingObject>)Delegate.Remove(cbEnemyObjectCreated, callbackfunc);
	}

	public void SpawnStaticEnemies()
	{
		ti.mainBattleGrid.currentEnemies.Clear();
		for (int i = 0; i < enemyToSpawn.Count; i++)
		{
			PlaceBeing(enemyToSpawn[i], ti.mainBattleGrid.grid[enemySpawnX[i], enemySpawnY[i]], i, true);
		}
	}

	private void SpawnRandomEnemies()
	{
		List<Tile> list = ti.mainBattleGrid.Get(new TileApp(Location.RandEnemyUnique, Shape.Default, Pattern.Unoccupied), 0, null, null, true);
		for (int i = 0; i < numEnemiesToSpawn; i++)
		{
			int num = -1;
			for (int j = 0; j < enemyToSpawn.Count; j++)
			{
				int num2 = runCtrl.NextPsuedoRand(1, 10);
				if (num2 <= enemyChance[j])
				{
					num = j;
					break;
				}
				num++;
			}
			int index = runCtrl.NextPsuedoRand(0, list.Count);
			PlaceBeing(enemyToSpawn[num], ti.mainBattleGrid.grid[list[index].x, list[index].y], i, true);
			list.RemoveAt(index);
		}
	}
}
