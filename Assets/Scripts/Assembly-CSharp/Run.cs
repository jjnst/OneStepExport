using System;
using System.Collections.Generic;
using System.Xml.Schema;

[Serializable]
public class Run
{
	public string runName = string.Empty;

	public string worldName = string.Empty;

	public string seed = string.Empty;

	public bool seedWasPredefined = false;

	public string beingID = string.Empty;

	public string animName = string.Empty;

	public int zoneNum = 0;

	public int zoneIndex = 0;

	public int worldTierNum = 0;

	public int currentPsuedoGen = 0;

	public int lastPsuedoGenOrigin = 0;

	public int currentWorldGen = 0;

	public int lastWorldGenOrigin = 0;

	public List<string> visitedWorldNames = new List<string>();

	public List<string> unvisitedWorldNames = new List<string>();

	public int hellPassNum = 0;

	public List<int> hellPasses = new List<int>();

	public bool dark = false;

	public bool revaSaved = false;

	public bool yamiObtained = false;

	public bool coOp = false;

	public Dictionary<string, bool> assists = new Dictionary<string, bool>();

	public List<string> charUnlocks = new List<string>();

	public List<string> skinUnlocks = new List<string>();

	public List<string> shopOptions = new List<string>();

	public List<string> shopPactRewards = new List<string>();

	public int shopRefillAdd = 0;

	public int shopUpgradesPurchased = 0;

	public int shopDonateValue = 0;

	public bool evilHostages = false;

	public int hostagesSavedStreak = 0;

	public int hostagesKilled = 0;

	public int finishedZones = 0;

	public int bossExecutions = 0;

	public bool shopkeeperDowned = false;

	public bool shopkeeperKilled = false;

	public int sera = 0;

	public int removals = 0;

	public int upgraders = 0;

	public int permanentLuck = 0;

	public bool artifactTaken = false;

	public bool genocide = false;

	public bool neutral = false;

	public bool pacifist = false;

	public float runTime = 0f;

	public List<string> killedSprites = new List<string>();

	public List<Brand> focuses = new List<Brand>();

	public List<ArtData> artifactData = new List<ArtData>();

	public List<SpellData> spellData = new List<SpellData>();

	public List<PactData> pactData = new List<PactData>();

	public int currentHealth = 0;

	public int currentHealthPlayerTwo = 0;

	public int maxHealth = 0;

	public int experience = 0;

	public int playerLevel = 0;

	public bool gateDefeated = false;

	public int loopNum = 0;

	public Run()
	{
		runName = "nullRun";
	}

	public Run(string name)
	{
		runName = name;
	}

	public Run Load()
	{
		runName = SaveDataCtrl.Get("runName", string.Empty);
		worldName = SaveDataCtrl.Get("runWorldName", string.Empty);
		seed = SaveDataCtrl.Get("runSeed", string.Empty);
		seedWasPredefined = SaveDataCtrl.Get("runSeedWasPredefined", false);
		beingID = SaveDataCtrl.Get("runBeingID", string.Empty);
		animName = SaveDataCtrl.Get("runAnimName", string.Empty);
		zoneNum = SaveDataCtrl.Get("runZoneNum", 0);
		zoneIndex = SaveDataCtrl.Get("runZoneIndex", 0);
		worldTierNum = SaveDataCtrl.Get("runWorldTierNum", 0);
		lastPsuedoGenOrigin = SaveDataCtrl.Get("runLastPsuedoGenOrigin", 0);
		lastWorldGenOrigin = SaveDataCtrl.Get("runLastWorldGenOrigin", 0);
		visitedWorldNames = new List<string>(SaveDataCtrl.Get("runVisitedWorldNames", new List<string>()));
		unvisitedWorldNames = new List<string>(SaveDataCtrl.Get("runUnvisitedWorldNames", new List<string>()));
		hellPassNum = SaveDataCtrl.Get("runHellPassNum", 0);
		hellPasses = SaveDataCtrl.Get("runHellPasses", new List<int>());
		dark = SaveDataCtrl.Get("runDark", false);
		revaSaved = SaveDataCtrl.Get("runRevaSaved", false);
		yamiObtained = SaveDataCtrl.Get("runYamiObtained", false);
		coOp = SaveDataCtrl.Get("runCoOp", false);
		assists = new Dictionary<string, bool>(SaveDataCtrl.Get("runAssists", new Dictionary<string, bool>()));
		removals = SaveDataCtrl.Get("runRemovals", 0);
		upgraders = SaveDataCtrl.Get("runUpgraders", 0);
		permanentLuck = SaveDataCtrl.Get("runPermanentLuck", 0);
		artifactTaken = SaveDataCtrl.Get("runArtifactTaken", false);
		charUnlocks = new List<string>(SaveDataCtrl.Get("runCharUnlocks", new List<string>()));
		skinUnlocks = new List<string>(SaveDataCtrl.Get("runSkinUnlocks", new List<string>()));
		killedSprites = new List<string>(SaveDataCtrl.Get("runKilledSprites", new List<string>()));
		focuses = new List<Brand>(SaveDataCtrl.Get("runFocuses", new List<Brand>()));
		evilHostages = SaveDataCtrl.Get("runEvilHostages", false);
		hostagesSavedStreak = SaveDataCtrl.Get("hostagesSavedStreak", 0);
		hostagesKilled = SaveDataCtrl.Get("hostagesKilled", 0);
		finishedZones = SaveDataCtrl.Get("runFinishedZones", 0);
		bossExecutions = SaveDataCtrl.Get("runBossExecutions", 0);
		shopkeeperDowned = SaveDataCtrl.Get("shopkeeperDowned", false);
		shopkeeperKilled = SaveDataCtrl.Get("shopkeeperKilled", false);
		genocide = SaveDataCtrl.Get("genocide", false);
		pacifist = SaveDataCtrl.Get("pacifist", false);
		artifactData = new List<ArtData>(SaveDataCtrl.Get("runArtifactData", new List<ArtData>()));
		spellData = new List<SpellData>(SaveDataCtrl.Get("runSpellData", new List<SpellData>()));
		pactData = new List<PactData>(SaveDataCtrl.Get("runPactData", new List<PactData>()));
		shopOptions = new List<string>(SaveDataCtrl.Get("runShopOptions", new List<string>()));
		shopPactRewards = new List<string>(SaveDataCtrl.Get("runShopPactRewards", new List<string>()));
		shopRefillAdd = SaveDataCtrl.Get("runShopRefillAdd", 0);
		shopUpgradesPurchased = SaveDataCtrl.Get("runShopUpgradesPurchased", 0);
		shopDonateValue = SaveDataCtrl.Get("runShopDonateValue", 0);
		currentHealth = SaveDataCtrl.Get("runCurrentHealth", 0);
		currentHealthPlayerTwo = SaveDataCtrl.Get("runCurrentHealthPlayerTwo", 0);
		maxHealth = SaveDataCtrl.Get("runMaxHealth", 0);
		experience = SaveDataCtrl.Get("runExperience", 0);
		playerLevel = SaveDataCtrl.Get("runPlayerLevel", 0);
		sera = SaveDataCtrl.Get("runSera", 0);
		runTime = SaveDataCtrl.Get("runTime", 0f);
		gateDefeated = SaveDataCtrl.Get("runGateDefeated", false);
		loopNum = SaveDataCtrl.Get("loopNum", 0);
		return this;
	}

	public void Save()
	{
		SaveDataCtrl.Set("runName", runName);
		SaveDataCtrl.Set("runWorldName", worldName);
		SaveDataCtrl.Set("runSeed", seed);
		SaveDataCtrl.Set("runSeedWasPredefined", seedWasPredefined);
		SaveDataCtrl.Set("runBeingID", beingID);
		SaveDataCtrl.Set("runAnimName", animName);
		SaveDataCtrl.Set("runZoneNum", zoneNum);
		SaveDataCtrl.Set("runZoneIndex", zoneIndex);
		SaveDataCtrl.Set("runWorldTierNum", worldTierNum);
		SaveDataCtrl.Set("runLastPsuedoGenOrigin", lastPsuedoGenOrigin);
		SaveDataCtrl.Set("runLastWorldGenOrigin", lastWorldGenOrigin);
		SaveDataCtrl.Set("runVisitedWorldNames", visitedWorldNames);
		SaveDataCtrl.Set("runUnvisitedWorldNames", unvisitedWorldNames);
		SaveDataCtrl.Set("runHellPassNum", hellPassNum);
		SaveDataCtrl.Set("runHellPasses", hellPasses);
		SaveDataCtrl.Set("runDark", dark);
		SaveDataCtrl.Set("runRevaSaved", revaSaved);
		SaveDataCtrl.Set("runYamiObtained", yamiObtained);
		SaveDataCtrl.Set("runCoOp", coOp);
		SaveDataCtrl.Set("runAssists", assists);
		SaveDataCtrl.Set("runRemovals", removals);
		SaveDataCtrl.Set("runUpgraders", upgraders);
		SaveDataCtrl.Set("runPermanentLuck", permanentLuck);
		SaveDataCtrl.Set("runArtifactTaken", artifactTaken);
		SaveDataCtrl.Set("runCharUnlocks", charUnlocks);
		SaveDataCtrl.Set("runSkinUnlocks", skinUnlocks);
		SaveDataCtrl.Set("runKilledSprites", killedSprites);
		SaveDataCtrl.Set("runFocuses", focuses);
		SaveDataCtrl.Set("runEvilHostages", evilHostages);
		SaveDataCtrl.Set("hostagesSavedStreak", hostagesSavedStreak);
		SaveDataCtrl.Set("hostagesKilled", hostagesKilled);
		SaveDataCtrl.Set("runFinishedZones", finishedZones);
		SaveDataCtrl.Set("runBossExecutions", bossExecutions);
		SaveDataCtrl.Set("shopkeeperDowned", shopkeeperDowned);
		SaveDataCtrl.Set("shopkeeperKilled", shopkeeperKilled);
		SaveDataCtrl.Set("genocide", genocide);
		SaveDataCtrl.Set("neutral", neutral);
		SaveDataCtrl.Set("pacifist", pacifist);
		SaveDataCtrl.Set("runArtifactData", artifactData);
		SaveDataCtrl.Set("runSpellData", spellData);
		SaveDataCtrl.Set("runPactData", pactData);
		SaveDataCtrl.Set("runShopOptions", shopOptions);
		SaveDataCtrl.Set("runShopPactRewards", shopPactRewards);
		SaveDataCtrl.Set("runShopRefillAdd", shopRefillAdd);
		SaveDataCtrl.Set("runShopUpgradesPurchased", shopUpgradesPurchased);
		SaveDataCtrl.Set("runShopDonateValue", shopDonateValue);
		SaveDataCtrl.Set("runCurrentHealth", currentHealth);
		SaveDataCtrl.Set("runCurrentHealthPlayerTwo", currentHealthPlayerTwo);
		SaveDataCtrl.Set("runMaxHealth", maxHealth);
		SaveDataCtrl.Set("runExperience", experience);
		SaveDataCtrl.Set("runPlayerLevel", playerLevel);
		SaveDataCtrl.Set("runSera", sera);
		SaveDataCtrl.Set("runTime", runTime);
		SaveDataCtrl.Set("runGateDefeated", gateDefeated);
		SaveDataCtrl.Set("loopNum", loopNum);
	}

	public Run(Run other)
	{
		runName = other.runName;
		worldName = other.worldName;
		seed = other.seed;
		seedWasPredefined = other.seedWasPredefined;
		beingID = other.beingID;
		animName = other.animName;
		zoneNum = other.zoneNum;
		zoneIndex = other.zoneIndex;
		worldTierNum = other.worldTierNum;
		lastPsuedoGenOrigin = other.lastPsuedoGenOrigin;
		lastWorldGenOrigin = other.lastWorldGenOrigin;
		visitedWorldNames = new List<string>(other.visitedWorldNames);
		unvisitedWorldNames = new List<string>(other.unvisitedWorldNames);
		hellPassNum = other.hellPassNum;
		hellPasses = other.hellPasses;
		dark = other.dark;
		revaSaved = other.revaSaved;
		yamiObtained = other.yamiObtained;
		coOp = other.coOp;
		assists = new Dictionary<string, bool>(other.assists);
		removals = other.removals;
		upgraders = other.upgraders;
		permanentLuck = other.permanentLuck;
		artifactTaken = other.artifactTaken;
		charUnlocks = new List<string>(other.charUnlocks);
		skinUnlocks = new List<string>(other.skinUnlocks);
		killedSprites = new List<string>(other.killedSprites);
		focuses = new List<Brand>(other.focuses);
		evilHostages = other.evilHostages;
		hostagesSavedStreak = other.hostagesSavedStreak;
		hostagesKilled = other.hostagesKilled;
		finishedZones = other.finishedZones;
		bossExecutions = other.bossExecutions;
		shopkeeperDowned = other.shopkeeperDowned;
		shopkeeperKilled = other.shopkeeperKilled;
		shopUpgradesPurchased = other.shopUpgradesPurchased;
		shopDonateValue = other.shopDonateValue;
		genocide = other.genocide;
		neutral = other.neutral;
		pacifist = other.pacifist;
		gateDefeated = other.gateDefeated;
		loopNum = other.loopNum;
		hellPassNum = other.hellPassNum;
		hellPasses = other.hellPasses;
	}

	public void SetupRun()
	{
	}

	public void AddAssist(BeingObject beingObj)
	{
		if (!assists.ContainsKey(beingObj.beingID))
		{
			assists[beingObj.beingID] = true;
		}
	}

	public bool HasAssist(string bossName)
	{
		return assists.ContainsKey(bossName);
	}

	public void RemoveAssist(BeingObject beingObj)
	{
		if (assists.ContainsKey(beingObj.beingID))
		{
			assists.Remove(beingObj.beingID);
		}
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void Loop()
	{
		zoneNum = 0;
		bossExecutions = 0;
		pacifist = false;
		neutral = false;
		genocide = false;
		yamiObtained = false;
		shopkeeperKilled = false;
		assists.Clear();
	}
}
