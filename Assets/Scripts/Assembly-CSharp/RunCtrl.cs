using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using I2.Loc;
using Kittehface.Build;
using Kittehface.Framework20;
using MoonSharp.Interpreter;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

[MoonSharpUserData]
public class RunCtrl : MonoBehaviour
{
	public string testWorldName;

	public int testZoneNum;

	public int testWorldTierNum;

	public string testBattleName;

	public string demoWorldName;

	public string demoLiveWorldName;

	[ShowInInspector]
	public Run currentRun = null;

	public ZoneDot currentZoneDot;

	public string zoneBackground;

	[ShowInInspector]
	public Run loadedRun = null;

	public bool useTestOrder = false;

	public List<Tag> testOrder;

	public float slideTime;

	[ShowInInspector]
	public Dictionary<string, World> worlds;

	[ShowInInspector]
	public World currentWorld;

	public int stageNum = 0;

	public string defaultHeroString;

	public string campaignHeroString;

	public string demoHeroString;

	public string demoLiveHeroString;

	public string localizedWorldName;

	public int currentHellPassNum = 0;

	public int unlockedHellPassNum = -1;

	public List<int> currentHellPasses = new List<int>();

	public XMLReader xmlReader;

	public WorldBar worldBar;

	public ProgressBar progressBar;

	public string testSeed;

	public bool useRandomSeed;

	public System.Random pseudoRandom;

	public System.Random pseudoRandomWorld;

	public BC ctrl;

	public BGCtrl bgCtrl;

	public ButtonCtrl btnCtrl;

	public CameraScript camCtrl;

	public DiscordCtrl disCtrl;

	public HeroSelectCtrl heCtrl;

	public IdleCtrl idCtrl;

	public FocusCtrl foCtrl;

	public MainCtrl mainCtrl;

	public MusicCtrl muCtrl;

	public PostCtrl poCtrl;

	public ShopCtrl shopCtrl;

	public SpawnCtrl spCtrl;

	public TutorialCtrl tutCtrl;

	public ItemManager itemMan;

	public Platform kfFrameworkPlatformPrefab;

	public int savedBossKills = 0;

	public int tutorialBossKills = 0;

	public bool doSave;

	public bool doLoad;

	public bool doDelete;

	private static SKUInfo _skuInfo = null;

	private static bool skuInfoSet = false;

	public static bool assetBundleManagerInitialized = false;

	public static Profiles.Profile runProfile;

	public static Profiles.Profile secondaryProfile;

	public static SKUInfo skuInfo
	{
		get
		{
			if (!skuInfoSet)
			{
				skuInfoSet = true;
				_skuInfo = Resources.Load<SKUInfo>("skuInfo");
			}
			return _skuInfo;
		}
	}

	private void Awake()
	{
		bool flag = false;
		foreach (IResourceManager_Bundles mBundleManager in ResourceManager.pInstance.mBundleManagers)
		{
			if (mBundleManager is I2LocalizationAssetBundleLoader)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ResourceManager.pInstance.mBundleManagers.Add(new I2LocalizationAssetBundleLoader());
		}
		Profiles.OnActivated += Profiles_OnActivated;
		Profiles.OnDeactivated += Profiles_OnDeactivated;
		StartCoroutine(_SetDefaultPrefs());
	}

	private IEnumerator _SetDefaultPrefs()
	{
		yield return new WaitUntil(() => SaveDataCtrl.Initialized);
		if (S.I.EDITION == Edition.Full || S.I.EDITION == Edition.QA)
		{
			while (!SteamManager.Initialized && Time.timeSinceLevelLoad < S.maxLoadTime)
			{
				yield return null;
			}
		}
		unlockedHellPassNum = SaveDataCtrl.Get("UnlockedHellPassNum", -1);
		if (S.I.EDITION == Edition.Dev || S.I.EDITION == Edition.QA)
		{
			unlockedHellPassNum = 17;
		}
		savedBossKills = SaveDataCtrl.Get("SavedBossKills", 0);
		tutorialBossKills = SaveDataCtrl.Get("TutorialBossKills", 0);
	}

	private void GenerateSeeds(string predefinedSeed = "")
	{
		if (!string.IsNullOrEmpty(predefinedSeed))
		{
			currentRun.seed = predefinedSeed;
			currentRun.seedWasPredefined = true;
		}
		else if (useRandomSeed)
		{
			currentRun.seed = Mathf.Abs(Environment.TickCount).ToString();
		}
		else if (testSeed != null)
		{
			currentRun.seed = testSeed;
		}
		pseudoRandom = new System.Random(currentRun.seed.GetHashCode());
		pseudoRandomWorld = new System.Random(currentRun.seed.GetHashCode());
		worldBar.seedText.text = ScriptLocalization.UI.Worldbar_Seed + " " + currentRun.seed;
	}

	private void Update()
	{
		if (doSave)
		{
			doSave = false;
			SaveRun();
		}
		if (doLoad)
		{
			doLoad = false;
			CreateRunFromSave(loadedRun);
		}
		if (doDelete)
		{
			doDelete = false;
			DeleteRun();
		}
	}

	public void StartRefresh()
	{
		StartCoroutine(Start());
	}

	private IEnumerator Start()
	{
		camCtrl.cameraPane.color = S.I.GetFlashColor(UIColor.BlueDark);
		BC.GTimeScale = 1f;
		if (!ReInput.isReady)
		{
			AssetBundleLoadAssetOperation op2 = AssetBundleManager.LoadAssetAsync("rewired", "InputManager", typeof(GameObject));
			yield return op2;
			UnityEngine.Object.Instantiate(op2.GetAsset<GameObject>());
		}
		Platform platform = UnityEngine.Object.FindObjectOfType<Platform>();
		AchievementsCtrl achievementsCtrl = UnityEngine.Object.FindObjectOfType<AchievementsCtrl>();
		if (achievementsCtrl == null)
		{
			AssetBundleLoadAssetOperation op = AssetBundleManager.LoadAssetAsync("achievements", "AchievementsCtrl", typeof(GameObject));
			yield return op;
			achievementsCtrl = UnityEngine.Object.Instantiate(op.GetAsset<GameObject>()).GetComponent<AchievementsCtrl>();
		}
		if (platform == null)
		{
			Platform.OnRequestPlatformMetadata += Platform_OnRequestPlatformMetadata;
			UnityEngine.Object.Instantiate(kfFrameworkPlatformPrefab, Vector3.zero, Quaternion.identity);
		}
		while (!Platform.initialized)
		{
			yield return null;
		}
		if (secondaryProfile == null)
		{
			Profiles.RequestDummyProfile("");
			while (secondaryProfile == null)
			{
				yield return null;
			}
		}
		Profiles.OnActivated -= Profiles_OnActivated;
		UserInput.SetUserCount(1);
		ControllerDisconnectCtrl.trackingConfigurationChange = false;
		if (!SaveDataCtrl.Initialized)
		{
			yield return SaveDataCtrl.Initialize();
		}
		if (!AchievementsCtrl.Initialized)
		{
			yield return achievementsCtrl.Initialize();
		}
		yield return xmlReader.XMLtoTestSceneData();
		camCtrl.cameraPane.color = S.I.GetFlashColor(UIColor.Clear);
		if (S.I.EDITION == Edition.DemoLive)
		{
			StartDemoLive();
		}
		else if (S.I.scene == GScene.MainMenu)
		{
			mainCtrl.Startup();
		}
		else if (S.I.scene == GScene.HeroSelect)
		{
			mainCtrl.OpenPanel(heCtrl);
		}
		else if (S.I.scene == GScene.PvP)
		{
			mainCtrl.ClickPvP();
		}
		else
		{
			StartCoroutine(_EnterTestMode());
		}
	}

	private void OnDestroy()
	{
		Profiles.OnActivated -= Profiles_OnActivated;
		Profiles.OnDeactivated -= Profiles_OnDeactivated;
	}

	public void StartDemoLive()
	{
		ctrl.sp.CreateHeroObjects();
		ctrl.sp.CreateBossDictionary();
		ctrl.currentHeroObj = ctrl.sp.heroDictionary[demoLiveHeroString];
		idCtrl.moneyTextBattle.gameObject.SetActive(false);
		camCtrl.cameraPane.color = S.I.GetFlashColor(UIColor.BlueDark);
		S.I.CameraStill(UIColor.Clear);
		camCtrl.TransitionInHigh("LeftWipe");
		StartCampaign(false);
	}

	public void ResetRun()
	{
		currentRun = null;
	}

	public int NextPsuedoRand(int min, int max)
	{
		currentRun.currentPsuedoGen++;
		return pseudoRandom.Next(min, max);
	}

	public int NextWorldRand(int min, int max)
	{
		currentRun.currentWorldGen++;
		return pseudoRandomWorld.Next(min, max);
	}

	public void CreateNewRun(int zoneNum, int worldTierNum, bool campaign, string seed = "")
	{
		currentRun = new Run("Run");
		currentRun.beingID = ctrl.currentHeroObj.beingID;
		currentRun.animName = ctrl.currentHeroObj.animName;
		GenerateSeeds(seed);
		GenerateRandomWorldList(campaign);
		currentRun.zoneNum = zoneNum;
		currentRun.worldTierNum = worldTierNum;
		currentRun.hellPassNum = currentHellPassNum;
		currentRun.hellPasses = new List<int>(currentHellPasses);
		idCtrl.heroNameText.text = ctrl.currentHeroObj.localizedName;
		idCtrl.heroLevelText.text = string.Format(ScriptLocalization.UI.TopNav_LevelShort + " {0}", 1);
		if (heCtrl.gameMode == GameMode.CoOp)
		{
			currentRun.coOp = true;
		}
		ctrl.deCtrl.deckScreen.ResetValues();
	}

	public void CreateNewRunFromLoaded(Run run)
	{
		currentRun = new Run(run);
		pseudoRandom = new System.Random(currentRun.seed.GetHashCode());
		pseudoRandomWorld = new System.Random(currentRun.seed.GetHashCode());
		for (int i = 0; i < currentRun.lastPsuedoGenOrigin; i++)
		{
			NextPsuedoRand(0, 1);
		}
		for (int j = 0; j < currentRun.lastWorldGenOrigin; j++)
		{
			NextWorldRand(0, 1);
		}
		worlds = new Dictionary<string, World>();
		xmlReader.XMLtoGetWorlds(xmlReader.GetDataFile("Zones.xml"));
		progressBar.Set();
		currentWorld = worlds[currentRun.worldName];
		idCtrl.heroNameText.text = ctrl.currentHeroObj.localizedName;
		idCtrl.heroLevelText.text = string.Format(ScriptLocalization.UI.TopNav_LevelShort + " {0}", run.playerLevel);
		worldBar.GenerateWorldBar(currentWorld.numZones);
		currentZoneDot = worldBar.currentZoneDots[loadedRun.zoneIndex];
	}

	public bool LoadedRunExists()
	{
		if (loadedRun != null && loadedRun.runName != "nullRun" && !ctrl.pvpMode)
		{
			return true;
		}
		return false;
	}

	public void LoopRun()
	{
		currentRun.visitedWorldNames.Clear();
		currentRun.loopNum++;
		GenerateRandomWorldList(true);
		currentRun.Loop();
		if (ctrl.currentPlayer.beingObj.tags.Contains(Tag.Shopkeeper))
		{
			currentRun.yamiObtained = true;
		}
		ResetWorld(currentWorld.nameString);
		StartZone(currentRun.zoneNum, currentZoneDot, true);
		if (!AchievementsCtrl.IsUnlocked("Back_to_the_Past"))
		{
			AchievementsCtrl.UnlockAchievement("Back_to_the_Past");
		}
		if (currentRun.loopNum > SaveDataCtrl.Get("MostLoops", 0))
		{
			SaveDataCtrl.Set("MostLoops", currentRun.loopNum);
		}
		if (currentRun.loopNum > SaveDataCtrl.Get(ctrl.currentHeroObj.nameString + "MostLoops", 0))
		{
			SaveDataCtrl.Set(ctrl.currentHeroObj.nameString + "MostLoops", currentRun.loopNum);
		}
	}

	private void GenerateRandomWorldList(bool campaign)
	{
		if (!S.modsInstalled)
		{
			worlds = new Dictionary<string, World>();
		}
		xmlReader.XMLtoGetWorlds(xmlReader.GetDataFile("Zones.xml"));
		if (campaign)
		{
			List<string> list = new List<string>(worlds.Keys);
			List<string> list2 = new List<string>(list);
			foreach (string item in list2)
			{
				if (Utils.SharesTags(ctrl.currentHeroObj.tags, worlds[item].tags))
				{
					list.Remove(worlds[item].nameString);
				}
			}
			list2 = new List<string>(list);
			foreach (string item2 in list2)
			{
				if (!worlds[item2].tags.Contains(Tag.Pool))
				{
					list.Remove(worlds[item2].nameString);
				}
			}
			if (useTestOrder)
			{
				list2 = new List<string>(list);
				foreach (string item3 in list2)
				{
					if (!Utils.SharesTags(testOrder, worlds[item3].tags) && !worlds[item3].tags.Contains(Tag.Ending))
					{
						list.Remove(worlds[item3].nameString);
					}
				}
			}
			for (int num = list.Count; num > 0; num--)
			{
				int index = NextPsuedoRand(0, list.Count);
				currentRun.unvisitedWorldNames.Add(list[index]);
				list.Remove(list[index]);
			}
			progressBar.Set();
			currentRun.worldName = currentRun.unvisitedWorldNames[0];
			currentRun.visitedWorldNames.Add(currentRun.unvisitedWorldNames[0]);
			currentRun.unvisitedWorldNames.RemoveAt(0);
		}
		else if (S.I.scene == GScene.DemoLive)
		{
			currentRun.worldName = demoLiveWorldName;
		}
		else
		{
			currentRun.worldName = testWorldName;
		}
		currentWorld = worlds[currentRun.worldName];
	}

	public void StartCampaign(bool loadRun, string seed = "")
	{
		if (loadRun)
		{
			CreateRunFromSave(loadedRun);
			return;
		}
		ctrl.runStopWatch.Reset();
		ctrl.runStopWatch.Resume();
		DeleteRun();
		CreateNewRun(0, 0, true, seed);
		ResetWorld(currentWorld.nameString);
		StartZone(currentRun.zoneNum, currentZoneDot, true);
	}

	public void StartPvP()
	{
		ctrl.runStopWatch.Reset();
		ctrl.runStopWatch.Resume();
		CreateNewRun(0, 0, true);
		ResetWorld(currentWorld.nameString, 1);
		bgCtrl.ChangeBG(currentWorld.background);
		muCtrl.PauseIntroLoop();
		worldBar.UpdateLocation(currentZoneDot);
		worldBar.detailPanel.gameObject.SetActive(false);
		currentZoneDot.type = ZoneType.PvP;
		ctrl.SetupPvP(currentZoneDot.type);
	}

	public void ResetWorld(string worldName, int numZones = -1)
	{
		World world = worlds[worldName];
		currentRun.dark = false;
		currentRun.zoneNum = 0;
		currentWorld = worlds[world.nameString];
		if (S.I.EDITION == Edition.DemoLive)
		{
			currentWorld = worlds["Arcadia"];
		}
		if (numZones == -1)
		{
			numZones = currentWorld.numZones;
		}
		worldBar.GenerateWorldBar(numZones);
		currentZoneDot = worldBar.currentZoneDots[0];
	}

	public void StartZone(int zoneNum, ZoneDot zoneDot, bool newRun = false)
	{
		S.I.PlayOnce(idCtrl.startZoneSound);
		zoneDot.pastDot = currentZoneDot;
		zoneDot.ColorNextLines();
		if (zoneNum > 0)
		{
			currentZoneDot.KeepColorFor(zoneDot);
		}
		else if (zoneNum == 0)
		{
			currentZoneDot.ColorNextLines();
		}
		currentZoneDot = zoneDot;
		zoneBackground = string.Empty;
		stageNum = CalculateStageNum();
		xmlReader.XMLtoZoneData(currentWorld, stageNum, zoneDot.type);
		Debug.Log("Entering World " + currentRun.worldName + " - Zone " + zoneNum + " StageNum: " + stageNum);
		if (!newRun)
		{
			ctrl.IncrementStat("TotalZones");
		}
		currentRun.finishedZones++;
		currentRun.zoneNum++;
		poCtrl.luck += poCtrl.luckBonusRate;
		currentZoneDot = zoneDot;
		StartCoroutine(StartZoneProcess(zoneNum, newRun));
	}

	public void UpdateDiscordData()
	{
		int num = 0;
		if (currentRun != null)
		{
			num = currentRun.zoneNum;
		}
		int num2 = 1200;
		string text = "";
		string detailText = "";
		if ((bool)ctrl.currentPlayer)
		{
			text = ctrl.currentPlayer.beingObj.localizedName;
			detailText = string.Format("Playing {0}", text);
			num2 = ctrl.currentPlayer.health.current;
			detailText = detailText + " " + num2 + "hp";
		}
		string stateText = string.Format("{2} {0} - {1}", currentRun.worldTierNum + 1, num, currentWorld.nameString);
		disCtrl.RefreshActivityData(stateText, detailText, text);
	}

	public int CalculateStageNum()
	{
		int num = 0;
		num = ((currentRun.zoneNum > 0) ? ((currentRun.zoneNum <= 2) ? 1 : ((currentRun.zoneNum > 4) ? 2 : 2)) : 0);
		if (savedBossKills < 2)
		{
			num = 0;
		}
		if (currentRun.worldTierNum < 2)
		{
			num = Mathf.Clamp(num, 0, 1);
		}
		float num2 = (float)NextPsuedoRand(0, 100) + poCtrl.CombinedLuck();
		if (num2 > 125f)
		{
			num++;
		}
		if (currentRun.worldTierNum > 2 && num == 0)
		{
			num++;
		}
		if (S.I.EDITION == Edition.DemoLive)
		{
			num = currentRun.zoneNum;
		}
		return num;
	}

	public void ShowControlsPanel(int zoneNum, bool battle)
	{
		if (currentRun.worldTierNum == 0 && zoneNum <= 1 && tutorialBossKills < 1 && S.I.CAMPAIGN_MODE && S.I.SHOW_TUTORIAL)
		{
			idCtrl.controlsBackDisplay.ShowControlsPanel(battle);
		}
		else if (S.I.EDITION == Edition.DemoLive && currentRun.worldTierNum == 0 && zoneNum <= 2 && S.I.SHOW_TUTORIAL)
		{
			idCtrl.controlsBackDisplay.ShowControlsPanel(battle);
		}
	}

	private IEnumerator StartZoneProcess(int zoneNum, bool newRun = false)
	{
		muCtrl.PauseIntroLoop();
		camCtrl.CameraChangePos(2, true);
		PostCtrl.transitioning = true;
		foreach (Player player2 in ctrl.currentPlayers)
		{
			if (player2.health.current > 0 && !player2.dead)
			{
				player2.anim.SetBool("dashing", true);
			}
		}
		if (zoneNum == 0 || newRun)
		{
			progressBar.anim.SetBool("visible", true);
			bgCtrl.MoveBG();
			if (S.I.ANIMATIONS && !S.I.RECORD_MODE)
			{
				if (!newRun)
				{
					yield return new WaitForSeconds(0.5f);
				}
				progressBar.canvas.sortingOrder = camCtrl.canvas.sortingOrder;
				ctrl.AddControlBlocks(Block.NextWorld);
				if (!newRun)
				{
					camCtrl.TransitionInHigh("LeftWipe");
				}
				yield return new WaitForSeconds(0.4f);
				progressBar.SetLocation(currentRun.worldTierNum, currentWorld.iconName, newRun);
				ctrl.centralMessageText.text = LocalizationManager.GetTranslation("UI/Approaching_" + currentWorld.background);
				U.I.Show(ctrl.centralMessageContainer);
				camCtrl.designEffects.SetTrigger("hollowDiamonds");
				yield return new WaitForSeconds(0.7f);
				if (newRun)
				{
					itemMan.LoadItemData();
				}
				yield return new WaitForSeconds(0.3f);
				U.I.Hide(ctrl.centralMessageContainer);
				camCtrl.designEffects.SetTrigger("hollowDiamondsOut");
				yield return new WaitForSeconds(0.6f);
				ctrl.RemoveControlBlocks(Block.NextWorld);
			}
			else if (newRun)
			{
				yield return new WaitForSeconds(0.1f);
				itemMan.LoadItemData();
			}
			if (currentRun.dark)
			{
				bgCtrl.ChangeBG(currentWorld.background + "_dark");
			}
			else
			{
				bgCtrl.ChangeBG(currentWorld.background);
			}
			progressBar.anim.SetBool("visible", false);
			camCtrl.TransitionOutHigh("LeftWipe");
			if (S.I.scene == GScene.DemoLive || (!SaveDataCtrl.Get("TutorialOverlayShown", false) && S.I.SHOW_TUTORIAL))
			{
				yield return new WaitForSeconds(0.3f);
				tutCtrl.StartTutorialOv();
				while (tutCtrl.tutorialOvsInProgress)
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
		SetZoneText(zoneNum);
		ctrl.idCtrl.moneyTextBattle.gameObject.SetActive(true);
		ShowControlsPanel(zoneNum, true);
		if (S.I.CAMPAIGN_MODE)
		{
			muCtrl.PauseIntroLoop();
		}
		worldBar.UpdateLocation(currentZoneDot);
		if (!string.IsNullOrEmpty(zoneBackground))
		{
			bgCtrl.ChangeBG(zoneBackground);
		}
		else if (currentRun.dark)
		{
			bgCtrl.ChangeBG(currentWorld.background + "_dark");
		}
		else
		{
			bgCtrl.ChangeBG(currentWorld.background);
		}
		ctrl.SetupZone(currentZoneDot.type);
		if (S.I.RECORD_MODE && btnCtrl.hideUICounter == 0)
		{
			btnCtrl.CycleHiddenUI();
		}
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.9f);
		}
		foreach (Player player in ctrl.currentPlayers)
		{
			player.anim.SetBool("dashing", false);
		}
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.4f);
		}
		if (S.I.scene == GScene.Idle)
		{
			idCtrl.MakeWorldBarAvailable();
		}
		if (currentZoneDot.type == ZoneType.Idle)
		{
			U.I.Hide(ctrl.centralMessageContainer);
			ctrl.GameState = GState.Idle;
			idCtrl.MakeWorldBarAvailable();
		}
		PostCtrl.transitioning = false;
		UpdateDiscordData();
	}

	public void GoToNextZone(ZoneDot zoneDot)
	{
		if (currentRun.zoneNum >= currentWorld.numZones)
		{
			GoToNextWorld(ref zoneDot);
		}
		StartZone(currentRun.zoneNum, zoneDot);
	}

	private void GoToNextWorld(ref ZoneDot zoneDot)
	{
		Debug.Log("GOING TO NEXT WORLD " + zoneDot.worldName + " cur zone num : " + currentRun.zoneNum + "  and the zone= " + currentWorld.numZones + " BossExecs= " + currentRun.bossExecutions + " unvis world nums:" + currentRun.unvisitedWorldNames.Count);
		currentRun.worldTierNum++;
		int num = currentRun.visitedWorldNames.Count;
		if (currentRun.shopkeeperKilled)
		{
			num++;
		}
		if (currentRun.unvisitedWorldNames.Count > 0)
		{
			currentRun.visitedWorldNames.Add(zoneDot.worldName);
			currentRun.unvisitedWorldNames.Remove(zoneDot.worldName);
		}
		else if (S.I.BETA)
		{
			zoneDot.worldName = "Beta";
		}
		else if (currentRun.bossExecutions == 0)
		{
			zoneDot.worldName = "Pacifist";
			currentRun.pacifist = true;
		}
		else if (currentRun.bossExecutions < num)
		{
			zoneDot.worldName = "Normal";
			currentRun.neutral = true;
		}
		else if (currentRun.bossExecutions >= num)
		{
			currentRun.genocide = true;
			zoneDot.worldName = "Genocide";
		}
		progressBar.transform.SetParent(Camera.main.transform);
		currentRun.worldName = zoneDot.worldName;
		ResetWorld(zoneDot.worldName);
		currentRun.zoneNum = 0;
		zoneDot = worldBar.currentZoneDots[0];
	}

	public void GoToDarkZone(ZoneDot zoneDot)
	{
		currentRun.dark = true;
		StartZone(currentRun.zoneNum, zoneDot);
	}

	public void ChangeZone(string worldName, int zoneNum, string zoneMessage)
	{
		S.I.PlayOnce(idCtrl.menuSlideSound);
		idCtrl.deckScreen.slideBody.Hide();
		idCtrl.deckScreen.AnimateCollection();
		StartAltZone(worldName, zoneNum, zoneMessage);
	}

	public void ChangeWorld(string worldName)
	{
		currentWorld.numZones = 4;
		Debug.Log("World Num - " + worldName);
		currentRun.worldName = worldName;
		bgCtrl.ChangeBG(currentWorld.background);
		ResetWorld(currentWorld.nameString);
		xmlReader.XMLtoZoneData(currentWorld, 0);
		SetZoneText();
		currentRun.zoneNum = 0;
		currentZoneDot = worldBar.currentZoneDots[0];
		worldBar.UpdateLocation(currentZoneDot);
	}

	private void SetZoneText(int zoneNum = 0)
	{
		worldBar.zoneText.text = GetZoneText(zoneNum);
		if (currentRun.dark)
		{
			worldBar.zoneText.text = worldBar.zoneText.text + " " + ScriptLocalization.UI.Dark;
		}
		worldBar.title.text = localizedWorldName;
	}

	public string GetZoneText(int zoneNum = 0)
	{
		string text = "";
		if (currentRun.loopNum > 0)
		{
			text = currentRun.loopNum + "x";
		}
		if (!LocalizationManager.TryGetTranslation("BeingNames/" + currentWorld.nameString, out localizedWorldName))
		{
			localizedWorldName = currentWorld.nameString;
		}
		return string.Format("{3}{0} - {1} ({2})", currentRun.worldTierNum + 1, zoneNum, localizedWorldName, text);
	}

	public void StartAltZone(string worldName, int zoneNum, string zoneMessage)
	{
		currentWorld = worlds[worldName];
		S.I.PlayOnce(idCtrl.startZoneSound);
		Debug.Log("Entering Alt World " + worldName + " - Zone " + zoneNum);
		SetZoneText();
		xmlReader.XMLtoZoneData(currentWorld, 0);
		StartCoroutine(StartAltZoneProcess(zoneNum, zoneMessage));
	}

	private IEnumerator StartAltZoneProcess(int zoneNum, string zoneMessage)
	{
		ctrl.ti.mainBattleGrid.ClearField();
		PostCtrl.transitioning = true;
		muCtrl.PauseIntroLoop();
		yield return new WaitForSeconds(0.3f);
		bgCtrl.ChangeBG(currentWorld.background);
		U.I.Show(ctrl.centralMessageContainer);
		ctrl.centralMessageText.text = LocalizationManager.GetTranslation("UI/Approaching_" + currentWorld.background);
		yield return new WaitForSeconds(1.4f);
		U.I.Hide(ctrl.centralMessageContainer);
		yield return new WaitForSeconds(0.3f);
		ctrl.SetupZone(currentZoneDot.type);
		PostCtrl.transitioning = false;
	}

	private void CheckDemoSettings()
	{
		if (!S.I.ANIMATIONS || !S.I.SHOW_TUTORIAL || poCtrl.smallTiers || S.I.DEVELOPER_TOOLS || S.I.CONSOLE || !useRandomSeed || spCtrl.forceSpawn || !useRandomSeed || S.I.RECORD_MODE || S.I.dontSpawnAnything)
		{
			Debug.LogError("DEMO IS NOT PROPERLY SET! DEMO IS NOT PROPERLY SET!");
		}
	}

	private void CheckDemoLiveSettings()
	{
		if (!S.I.ANIMATIONS || ctrl.currentHeroObj.health < 1000 || ctrl.currentHeroObj.weapon != "CampaignGun" || !S.I.SHOW_TUTORIAL || poCtrl.smallTiers || demoLiveHeroString != "SaffronDemoLive" || S.I.DEVELOPER_TOOLS || !S.I.CONSOLE || !useRandomSeed || spCtrl.forceSpawn || S.I.RECORD_MODE || S.I.dontSpawnAnything)
		{
			Debug.LogError("DEMO IS NOT PROPERLY SET! DEMO IS NOT PROPERLY SET!");
		}
	}

	public bool SaveRun()
	{
		if (!ctrl.currentPlayer)
		{
			return false;
		}
		Run run = new Run(currentRun);
		Player player = ctrl.currentPlayers[0];
		foreach (ArtifactObject artObj in player.artObjs)
		{
			artObj.dead = true;
		}
		foreach (Cpu currentPet in player.currentPets)
		{
			currentPet.parentObj.artObj.currentValue = currentPet.health.current;
			currentPet.parentObj.artObj.maxValue = currentPet.health.max;
			currentPet.parentObj.artObj.dead = false;
		}
		List<ArtData> list = new List<ArtData>();
		foreach (ListCard artCard in ctrl.deCtrl.artCardList)
		{
			list.Add(new ArtData(artCard.artObj));
		}
		run.artifactData = list;
		List<SpellData> list2 = new List<SpellData>();
		foreach (ListCard item in player.duelDisk.deck)
		{
			list2.Add(new SpellData(item.spellObj));
		}
		run.spellData = list2;
		List<PactData> list3 = new List<PactData>();
		foreach (PactObject pactObj in player.pactObjs)
		{
			list3.Add(new PactData(pactObj));
		}
		run.pactData = list3;
		List<string> list4 = new List<string>();
		List<string> list5 = new List<string>();
		foreach (ChoiceCard currentShopOption in shopCtrl.currentShopOptions)
		{
			list4.Add(currentShopOption.itemObj.itemID);
			if (currentShopOption.itemObj.type == ItemType.Pact)
			{
				list5.Add(currentShopOption.itemObj.pactObj.rewardID);
			}
		}
		run.shopOptions = list4;
		run.shopPactRewards = list5;
		run.shopRefillAdd = shopCtrl.refillAdd;
		run.currentHealth = player.health.current;
		Debug.Log("saving first player health as " + player.health.current);
		if (ctrl.playerHalves != null && ctrl.playerHalves.Count > 1)
		{
			Debug.Log("saving second player health as " + ctrl.playerHalves[1].health.current);
			run.currentHealthPlayerTwo = ctrl.playerHalves[1].health.current;
		}
		run.currentHealth = player.health.current;
		run.maxHealth = player.health.max;
		worldBar.currentZoneDots.IndexOf(currentZoneDot);
		run.experience = poCtrl.experience;
		run.playerLevel = poCtrl.playerLevel;
		run.sera = shopCtrl.sera;
		run.runTime = ctrl.runStopWatch.timeInSeconds;
		List<Brand> list6 = new List<Brand>();
		for (int i = 0; i < foCtrl.brandDisplayButtons.Length; i++)
		{
			list6.Add(foCtrl.brandDisplayButtons[i].brand);
		}
		List<string> list7 = new List<string>();
		foreach (Sprite killedBeingSprite in ctrl.killedBeingSprites)
		{
			if (killedBeingSprite != null)
			{
				list7.Add(killedBeingSprite.name);
			}
		}
		run.killedSprites = new List<string>(list7);
		run.focuses = new List<Brand>(list6);
		run.zoneIndex = worldBar.currentZoneDots.IndexOf(currentZoneDot);
		run.lastPsuedoGenOrigin = run.currentPsuedoGen;
		run.permanentLuck = poCtrl.permanentLuck;
		run.Save();
		SaveDataCtrl.Set("SavedRunExists", true);
		SaveDataCtrl.Write(true);
		Debug.Log("Run Saved");
		return true;
	}

	public void DeleteRun()
	{
		SaveDataCtrl.Set("SavedRunExists", false);
		SaveDataCtrl.Write();
	}

	public bool LoadRun()
	{
		if (DoesSaveGameExist())
		{
			loadedRun = new Run().Load();
			Debug.Log("loading the saved run " + loadedRun.beingID);
		}
		return DoesSaveGameExist();
	}

	public bool DoesSaveGameExist()
	{
		Debug.Log("Save data exists?: " + SaveDataCtrl.Get("SavedRunExists", false));
		return SaveDataCtrl.Get("SavedRunExists", false);
	}

	public void CreateRunFromSave(Run newLoadedRun)
	{
		if (spCtrl.heroDictionary.ContainsKey(newLoadedRun.beingID))
		{
			if (spCtrl.heroDictionary.ContainsKey(newLoadedRun.beingID))
			{
				ctrl.currentHeroObj = spCtrl.heroDictionary[newLoadedRun.beingID].Clone();
			}
			else
			{
				Debug.LogError("Hero Dictionary does not contain " + newLoadedRun.beingID);
			}
			ctrl.currentHeroObj.animName = newLoadedRun.animName;
			ctrl.optCtrl.heroSplash.sprite = itemMan.GetSprite(ctrl.currentHeroObj.splashSprite);
		}
		ctrl.currentHeroObj.artifacts.Clear();
		foreach (ArtData artifactDatum in newLoadedRun.artifactData)
		{
			ctrl.currentHeroObj.artifacts.Add(artifactDatum);
		}
		ctrl.currentHeroObj.deck.Clear();
		foreach (SpellData spellDatum in newLoadedRun.spellData)
		{
			ctrl.currentHeroObj.deck.Add(spellDatum.itemID);
		}
		foreach (string killedSprite in newLoadedRun.killedSprites)
		{
			if (itemMan.sprites.ContainsKey(killedSprite))
			{
				ctrl.killedBeingSprites.Add(itemMan.sprites[killedSprite]);
			}
		}
		ctrl.runStopWatch.Set(newLoadedRun.runTime);
		CreateNewRunFromLoaded(newLoadedRun);
		currentHellPassNum = newLoadedRun.hellPassNum;
		currentHellPasses = new List<int>(newLoadedRun.hellPasses);
		poCtrl.experience = newLoadedRun.experience;
		poCtrl.playerLevel = newLoadedRun.playerLevel;
		poCtrl.luck = (float)(newLoadedRun.finishedZones - 1) * poCtrl.luckBonusRate;
		poCtrl.permanentLuck = newLoadedRun.permanentLuck;
		if (newLoadedRun.coOp)
		{
			heCtrl.gameMode = GameMode.CoOp;
		}
		else
		{
			heCtrl.gameMode = GameMode.Solo;
			Rewired.Player rewiredPlayer = GetRewiredPlayer();
			if (rewiredPlayer != null)
			{
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Gameplay");
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay");
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay2");
			}
			HelpIconManager.SetKeyboardAvailable(0, true);
		}
		worldBar.seedText.text = ScriptLocalization.UI.Worldbar_Seed + " " + currentRun.seed;
		currentZoneDot.type = ZoneType.Idle;
		StartZone(currentRun.zoneNum, currentZoneDot, true);
		currentRun.finishedZones = newLoadedRun.finishedZones;
		currentRun.zoneNum = newLoadedRun.zoneNum;
	}

	private IEnumerator _EnterTestMode()
	{
		yield return new WaitUntil(() => SaveDataCtrl.Initialized);
		if (S.I.EDITION == Edition.Full || S.I.EDITION == Edition.QA)
		{
			while (!SteamManager.Initialized && Time.timeSinceLevelLoad < S.maxLoadTime)
			{
				yield return null;
			}
		}
		ctrl.optCtrl.stillInStartup = false;
		CreateNewRun(testZoneNum, testWorldTierNum, S.I.CAMPAIGN_MODE);
		if (S.I.CO_OP)
		{
			heCtrl.gameMode = GameMode.CoOp;
		}
		if (S.I.scene == GScene.GameOver)
		{
			ResetWorld(currentWorld.nameString);
			ctrl.Defeat();
			yield break;
		}
		if (S.I.scene == GScene.SpellLoop)
		{
			ResetWorld("Automatia");
			xmlReader.XMLtoZoneData(currentWorld, 0);
			PostCtrl.transitioning = false;
			ctrl.SetupZone(ZoneType.Battle);
			yield break;
		}
		if (S.I.scene == GScene.Idle)
		{
			ResetWorld(currentWorld.nameString);
			PostCtrl.transitioning = false;
			currentZoneDot = worldBar.currentZoneDots[0];
			currentRun.zoneNum++;
			ctrl.SetupZone(ZoneType.Battle);
			idCtrl.MakeWorldBarAvailable();
			currentZoneDot.ColorNextLines();
			worldBar.UpdateLocation(currentZoneDot);
			yield break;
		}
		if (S.I.TESTING_MODE)
		{
			ResetWorld(currentWorld.nameString);
			xmlReader.XMLtoBattlesData(testBattleName);
			ctrl.SetupZone(ZoneType.Battle);
			yield break;
		}
		if (S.I.scene == GScene.DemoLive)
		{
			ResetWorld(demoLiveWorldName);
			camCtrl.cameraPane.color = S.I.GetFlashColor(UIColor.Black);
			S.I.CameraStill(UIColor.Clear);
			bgCtrl.ChangeBG(currentWorld.background);
			ctrl.currentHeroObj = spCtrl.heroDictionary[demoLiveHeroString];
			xmlReader.XMLtoZoneData(currentWorld, 0);
			PostCtrl.transitioning = false;
			StartZone(0, currentZoneDot, true);
			CheckDemoLiveSettings();
			yield break;
		}
		ResetWorld(currentWorld.nameString);
		currentRun.zoneNum = testZoneNum;
		currentZoneDot.type = S.I.testZoneType;
		if (S.I.testZoneType == ZoneType.Pacifist || S.I.testZoneType == ZoneType.Normal || S.I.testZoneType == ZoneType.Genocide)
		{
			currentRun.visitedWorldNames.Clear();
			currentRun.unvisitedWorldNames.Clear();
			ResetWorld(currentWorld.nameString);
			currentRun.worldTierNum = 8;
			currentRun.zoneNum = currentWorld.numZones - 1;
			if (S.I.testZoneType == ZoneType.Pacifist)
			{
				currentRun.bossExecutions = 0;
			}
			else if (S.I.testZoneType == ZoneType.Normal)
			{
				currentRun.bossExecutions = 1;
				currentRun.visitedWorldNames.Add("Selicy");
				currentRun.visitedWorldNames.Add("Selicy");
			}
			else if (S.I.testZoneType == ZoneType.Genocide)
			{
				currentRun.bossExecutions = 8;
			}
			S.I.testZoneType = ZoneType.Campsite;
			currentZoneDot = worldBar.currentZoneDots[worldBar.currentZoneDots.Count - 3];
			currentZoneDot.type = S.I.testZoneType;
		}
		if (S.I.scene == GScene.CoOp)
		{
			heCtrl.gameMode = GameMode.CoOp;
		}
		StartZone(currentRun.zoneNum, currentZoneDot, true);
	}

	private void Platform_OnRequestPlatformMetadata()
	{
		SKUInfo sKUInfo = skuInfo;
		if (sKUInfo != null && sKUInfo is SteamSKUInfo)
		{
			Platform.SetPlatformMetadata(((SteamSKUInfo)sKUInfo).GameID);
		}
	}

	private void Profiles_OnActivated(Profiles.Profile profile)
	{
		if (runProfile == null)
		{
			Debug.Log("Set runProfile");
			runProfile = profile;
		}
		else if (secondaryProfile == null)
		{
			Debug.Log("Set secondaryProfile");
			secondaryProfile = profile;
		}
	}

	private void Profiles_OnDeactivated(Profiles.Profile profile)
	{
		if (secondaryProfile == profile)
		{
			secondaryProfile = null;
		}
	}

	private IEnumerator _ReloadMain()
	{
		S.I.CameraStill(UIColor.BlueDark);
		btnCtrl.transitioning = true;
		AssetBundleLoadOperation assetBundleLoadOperation = AssetBundleManager.LoadLevelAsync("main", "Main", false);
		float startTime = Time.unscaledTime;
		while (!assetBundleLoadOperation.IsDone())
		{
			Mathf.Clamp01(assetBundleLoadOperation.GetProgress() / 0.9f);
			if (assetBundleLoadOperation.GetProgress() >= 1f && startTime + 1f < Time.unscaledTime)
			{
				assetBundleLoadOperation.AllowSceneActivation();
			}
			yield return null;
		}
	}

	private void Platform_OnSystemPause(bool shouldPause)
	{
	}

	public static Rewired.Player GetRewiredPlayer(int playerNum = 0)
	{
		if (!Platform.initialized || (playerNum == 0 && runProfile == null) || (playerNum == 1 && secondaryProfile == null))
		{
			return null;
		}
		switch (playerNum)
		{
		case 0:
			return UserInput.GetRewiredPlayer(runProfile);
		case 1:
			return UserInput.GetRewiredPlayer(secondaryProfile);
		default:
			return null;
		}
	}
}
