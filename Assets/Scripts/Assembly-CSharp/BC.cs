using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using MoonSharp.Interpreter;
using Rewired;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

[MoonSharpUserData]
public class BC : SerializedMonoBehaviour
{
	public BeingObject currentHeroObj;

	public GameObject statusTextPrefab;

	public GameObject statusDisplayPrefab;

	public GameObject floatingSpellPrefab;

	public GameObject dmgTextPrefab;

	public GameObject floatingTextContainerPrefab;

	public GameObject warningTimerPrefab;

	public GameObject lineTracerPrefab;

	public Player currentPlayer;

	public List<Player> currentPlayers = new List<Player>();

	public List<PlayerHalf> playerHalves = new List<PlayerHalf>();

	public List<Being> currentDeadBeings;

	public List<Cpu> currentIdlers;

	public List<Sprite> killedBeingSprites;

	public Being lastKilledEnemy;

	public Being lastKilledStructure;

	public List<Sprite> effectSprites = new List<Sprite>();

	public Dictionary<string, Sprite> effectSpritesDict = new Dictionary<string, Sprite>();

	public GameObject rewardDropPrefab;

	public RuntimeAnimatorController baseCharacterAnim;

	public RuntimeAnimatorController baseMonsterAnim;

	public Transform talkBubblesContainer;

	public Transform floatingTextsContainer;

	public Transform duelDisksContainer;

	public int numBattles;

	public int numBattlesLeft;

	public GameObject hitFXPrefab;

	public Color poisonColor;

	public AudioClip pvpDeathSound;

	public Transform battleUIContainer;

	public Canvas canvas;

	public CanvasGroup lowHealthOverlay;

	public int baseBossTier = 0;

	public int experienceGained = 0;

	public int moneyGained = 0;

	public GameOverPane gameOverPane;

	public GameObject statusEffectPrefab;

	public GameObject statusFlashPrefab;

	public Timerbar timerBarPrefab;

	public GameObject projectilePrefab;

	public float timeDistortionLength = 0f;

	public bool battleEnding = false;

	public bool blockAllSummons = false;

	public string battleRank = "X";

	private static float _gTimeScale = 1f;

	[SerializeField]
	private GState _gameState = GState.Idle;

	public AudioClip battleStartSound;

	public AudioClip winRoundSound;

	public TMP_Text centralMessageText;

	public GameObject centralMessageContainer;

	public float dropMultiplier = 1f;

	public float dropMultiplierAdd;

	public bool perfectBattle = true;

	public CameraScript camCtrl;

	public AudioSource audioSource;

	public static float hasteAmount = 0.05f;

	public static float slowAmount = 0.2f;

	public float fragileMultiplierBase = 1.5f;

	public static float fragileMultiplier = 1.5f;

	public static int frostDmg = 150;

	public static float frostLength = 99999f;

	public static int flameDmg = 10;

	public static int flameTicks = 14;

	public static float flameTickTime = 0.4f;

	public static float poisonMinimum = 0f;

	public static float poisonDuration = 4f;

	public static float poisonBaseDuration = 4f;

	public float shieldDecayBase = 0.25f;

	public static float shieldDecay = 0.25f;

	public AudioClip frostSound;

	public AudioClip trinityCastSound;

	public AudioClip earthboundSound;

	public AudioClip poisonSound;

	public int noHitMoneyBonus = 5;

	public StopWatch stopWatch;

	public StopWatch runStopWatch;

	[NonSerialized]
	public CGCtrl cgCtrl;

	[NonSerialized]
	public ControlsCtrl conCtrl;

	[NonSerialized]
	public CreditsCtrl credCtrl;

	[NonSerialized]
	public DeckCtrl deCtrl;

	[NonSerialized]
	public IdleCtrl idCtrl;

	[NonSerialized]
	public HeroSelectCtrl heCtrl;

	[NonSerialized]
	public ItemManager itemMan;

	[NonSerialized]
	public MainCtrl mainCtrl;

	[NonSerialized]
	public MusicCtrl muCtrl;

	[NonSerialized]
	public PostCtrl poCtrl;

	[NonSerialized]
	public ShopCtrl shopCtrl;

	[NonSerialized]
	public ButtonCtrl btnCtrl;

	[NonSerialized]
	public OptionCtrl optCtrl;

	[NonSerialized]
	public RunCtrl runCtrl;

	[NonSerialized]
	public BGCtrl bgCtrl;

	[NonSerialized]
	public SpawnCtrl sp;

	[NonSerialized]
	public TI ti;

	[NonSerialized]
	public TutorialCtrl tutCtrl;

	public static float playerChronoTime;

	public static float playerChronoScale = 1f;

	public bool bothPvPDead = false;

	public int pvpRequiredWins = 2;

	public int matchesPlayedThisSession = 0;

	public int playerOneSessionWins = 0;

	public int playerTwoSessionWins = 0;

	private UnityEngine.Coroutine co_PvPRoundEnd;

	public int roundCounter = 0;

	public bool pvpMode = false;

	public static float GTimeScale
	{
		get
		{
			return _gTimeScale;
		}
		set
		{
			_gTimeScale = value;
			Time.timeScale = value;
			Time.fixedDeltaTime = value * 0.02f;
			playerChronoScale = value;
		}
	}

	public GState GameState
	{
		get
		{
			return _gameState;
		}
		set
		{
			_gameState = value;
			if (ti.mainBattleGrid != null)
			{
				ti.mainBattleGrid.inBattle = false;
				ti.mainBattleGrid.inMidBattle = false;
			}
			switch (_gameState)
			{
			case GState.Idle:
				StartCoroutine(ShowZoneButtons());
				idCtrl.anim.SetBool("OnScreen", true);
				break;
			case GState.Loot:
				idCtrl.anim.SetBool("OnScreen", true);
				break;
			case GState.Experience:
				idCtrl.anim.SetBool("OnScreen", true);
				break;
			case GState.Unlock:
				idCtrl.anim.SetBool("OnScreen", false);
				break;
			case GState.PreBattle:
				idCtrl.anim.SetBool("OnScreen", false);
				ti.mainBattleGrid.inBattle = true;
				break;
			case GState.Battle:
				idCtrl.anim.SetBool("OnScreen", false);
				ti.mainBattleGrid.inBattle = true;
				ti.mainBattleGrid.inMidBattle = true;
				break;
			case GState.EndBattle:
				idCtrl.anim.SetBool("OnScreen", false);
				ti.mainBattleGrid.inBattle = true;
				break;
			default:
				idCtrl.anim.SetBool("OnScreen", false);
				break;
			}
		}
	}

	private void Awake()
	{
		cgCtrl = S.I.cgCtrl;
		conCtrl = S.I.conCtrl;
		credCtrl = S.I.credCtrl;
		deCtrl = S.I.deCtrl;
		heCtrl = S.I.heCtrl;
		idCtrl = S.I.idCtrl;
		itemMan = S.I.itemMan;
		mainCtrl = S.I.mainCtrl;
		muCtrl = S.I.muCtrl;
		poCtrl = S.I.poCtrl;
		shopCtrl = S.I.shopCtrl;
		btnCtrl = S.I.btnCtrl;
		camCtrl = S.I.camCtrl;
		runCtrl = S.I.runCtrl;
		optCtrl = S.I.optCtrl;
		bgCtrl = S.I.bgCtrl;
		sp = S.I.spCtrl;
		ti = S.I.tiCtrl;
		tutCtrl = S.I.tutCtrl;
		stopWatch = GetComponent<StopWatch>();
		runStopWatch = runCtrl.GetComponent<StopWatch>();
		audioSource = GetComponent<AudioSource>();
		audioSource.outputAudioMixerGroup = S.sfxGroup;
		fragileMultiplier = fragileMultiplierBase;
		shieldDecay = shieldDecayBase;
		foreach (Sprite effectSprite in effectSprites)
		{
			effectSpritesDict[effectSprite.name] = effectSprite;
		}
	}

	private void Start()
	{
		if (S.I.RECORD_MODE)
		{
			StartCoroutine(HideCentralMessage());
		}
	}

	private IEnumerator HideCentralMessage()
	{
		while (true)
		{
			yield return null;
			centralMessageContainer.SetActive(false);
		}
	}

	private void Update()
	{
		playerChronoTime = Time.unscaledDeltaTime;
		playerChronoTime *= playerChronoScale;
	}

	public bool PlayersActive()
	{
		if (!pvpMode)
		{
			if (!currentPlayer || !currentPlayer.isActiveAndEnabled)
			{
				return false;
			}
		}
		else if (currentPlayers.Count > 1)
		{
			foreach (Player currentPlayer in currentPlayers)
			{
				if (!currentPlayer || !currentPlayer.isActiveAndEnabled)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SetupZone(ZoneType zoneType)
	{
		sp.gameObject.SetActive(true);
		ti.gameObject.SetActive(true);
		numBattlesLeft = numBattles;
		StartCoroutine(SetupZoneC(zoneType));
	}

	private IEnumerator SetupZoneC(ZoneType zoneType)
	{
		S.I.camCtrl.CameraChangePos(2);
		foreach (Player player2 in currentPlayers)
		{
			player2.BattleStartReset();
		}
		if ((bool)currentPlayer)
		{
			currentPlayer.duelDisk.CreateDeckSpells();
		}
		if (sp.bossesToSpawn.Count > 0)
		{
			zoneType = ZoneType.Boss;
		}
		if (ti.mainBattleGrid != null)
		{
			ti.mainBattleGrid.ClearField(false, true);
		}
		ti.CreateMainField();
		if (zoneType != ZoneType.Shop && zoneType != ZoneType.DarkShop && zoneType != ZoneType.Campsite)
		{
			U.I.Show(centralMessageContainer);
			centralMessageText.text = ScriptLocalization.UI.Ready;
			yield return new WaitForSeconds(0.1f);
			if (S.I.ANIMATIONS)
			{
				yield return new WaitForSeconds(0.4f);
			}
			if (!currentPlayer && GameState != GState.GameOver)
			{
				sp.CreatePlayers();
				yield return new WaitForSeconds(0.1f);
			}
			else
			{
				if (S.I.scene != GScene.Idle)
				{
					AddControlBlocks(Block.BattleSetup);
					foreach (Player player3 in currentPlayers)
					{
						player3.ApplyStun(false, false, false);
					}
				}
				ti.mainBattleGrid.ClearField();
			}
		}
		else
		{
			foreach (Player player4 in currentPlayers)
			{
				player4.ApplyStun(false, false, false);
			}
		}
		yield return new WaitForSeconds(0.1f);
		foreach (Player player6 in currentPlayers)
		{
			player6.RemoveAllStatuses();
		}
		if (S.I.scene == GScene.Idle || S.I.scene == GScene.SpellLoop)
		{
			foreach (Player player5 in currentPlayers)
			{
				player5.mov.SetState(State.Idle);
			}
			U.I.Hide(centralMessageContainer);
			if (S.I.scene == GScene.SpellLoop)
			{
				sp.SpawnEnemySet();
				StartCoroutine(StartBattle());
			}
			else
			{
				GameState = GState.Idle;
			}
			yield break;
		}
		if (!currentPlayer && GameState != GState.GameOver)
		{
			sp.CreatePlayers();
			yield return new WaitForSeconds(0.1f);
			sp.SpawnEnvironment();
			yield return new WaitForSeconds(0.1f);
		}
		if (GameState == GState.GameOver)
		{
			U.I.Hide(centralMessageContainer);
			yield break;
		}
		if (S.I.scene == GScene.Victory)
		{
			Victory(Ending.PacifistTrue, "LeftWipe");
			yield break;
		}
		if (S.I.scene == GScene.VictoryFalse)
		{
			Victory(Ending.PacifistFalse, "LeftWipe");
			yield break;
		}
		if (S.I.scene == GScene.VictoryEvil)
		{
			Victory(Ending.Genocide, "LeftWipe");
			yield break;
		}
		sp.StartCoroutine(sp.SpawnZoneC(zoneType));
		if (zoneType == ZoneType.Boss && S.I.ANIMATIONS && S.I.scene != GScene.DemoLive)
		{
			yield return new WaitForSeconds(0.6f);
		}
		if (zoneType == ZoneType.Shop || zoneType == ZoneType.DarkShop)
		{
			idCtrl.spellsBtn.tmpText.text = ScriptLocalization.UI.SHOP;
		}
		else
		{
			idCtrl.spellsBtn.tmpText.text = ScriptLocalization.UI.InputAction_Deck;
		}
		if (zoneType != ZoneType.Shop && zoneType != ZoneType.DarkShop && zoneType != ZoneType.Campsite && zoneType != ZoneType.Idle)
		{
			StartCoroutine(StartBattle());
			yield break;
		}
		if (zoneType == ZoneType.Idle)
		{
			muCtrl.PlayIdle();
		}
		foreach (Player player in currentPlayers)
		{
			player.Activate();
		}
	}

	public IEnumerator _BattleAssists(int chance = 3, bool final = false)
	{
		List<Tile> emptyAlliedTiles = ti.mainBattleGrid.Get(new TileApp(Location.Index, Shape.ColumnWide, new List<Pattern>
		{
			Pattern.Random,
			Pattern.Unoccupied
		}, 2), 0, currentPlayer);
		if (final)
		{
			for (int i = emptyAlliedTiles.Count - 1; i >= 0; i--)
			{
				if (ti.mainBattleGrid.currentEnemies.Count > 0 && emptyAlliedTiles[i].y == ti.mainBattleGrid.currentEnemies[0].mov.currentTile.y)
				{
					emptyAlliedTiles.RemoveAt(i);
				}
			}
		}
		List<string> currentAssists = new List<string>(runCtrl.currentRun.assists.Keys);
		foreach (string beingID in currentAssists)
		{
			if (emptyAlliedTiles.Count <= 0 || !runCtrl.currentRun.HasAssist(beingID) || !runCtrl.currentRun.assists[beingID])
			{
				continue;
			}
			string assistName2 = beingID;
			if (assistName2.Contains("Boss"))
			{
				assistName2 = assistName2.Replace("Boss", "");
				assistName2 += "Assist";
				if (final)
				{
					assistName2 += "F";
				}
			}
			int spawnPosNum = UnityEngine.Random.Range(0, emptyAlliedTiles.Count);
			if (sp.beingDictionary.ContainsKey(assistName2) && runCtrl.NextPsuedoRand(0, chance) == 0)
			{
				Being newBeing = sp.PlaceBeing(assistName2, emptyAlliedTiles[spawnPosNum]);
				newBeing.mov.currentTile.SetOccupation(0);
				newBeing.mov.neverOccupy = true;
				emptyAlliedTiles.RemoveAt(spawnPosNum);
				newBeing.transform.rotation = base.transform.rotation;
				yield return new WaitForSeconds(0.5f);
				if (final)
				{
					yield return new WaitForSeconds(0.5f);
				}
			}
		}
	}

	public IEnumerator StartBattle(bool credits = false)
	{
		yield return new WaitForEndOfFrame();
		battleEnding = false;
		perfectBattle = true;
		GameState = GState.PreBattle;
		if (!credits)
		{
			deCtrl.TriggerAllArtifacts(FTrigger.OnBattleStart);
			if (runCtrl.currentZoneDot.type != ZoneType.Treasure)
			{
				yield return StartCoroutine(_BattleAssists());
			}
			if (S.I.ANIMATIONS)
			{
				U.I.Hide(centralMessageContainer);
				if (runCtrl.currentZoneDot.type != ZoneType.Treasure)
				{
					yield return new WaitForSeconds(0.6f);
					U.I.Show(centralMessageContainer);
				}
				audioSource.PlayOneShot(battleStartSound);
				centralMessageText.text = ScriptLocalization.UI.Battle_Start;
				yield return new WaitForSeconds(0f);
			}
			U.I.Hide(centralMessageContainer);
			if (S.I.ANIMATIONS && !S.I.RECORD_MODE)
			{
				yield return new WaitForSeconds(0.5f);
			}
		}
		foreach (Cpu be3 in ti.mainBattleGrid.currentEnemies)
		{
			be3.Activate();
		}
		foreach (Being be2 in ti.mainBattleGrid.currentAllies)
		{
			be2.Activate();
		}
		foreach (Structure be in ti.mainBattleGrid.currentStructures)
		{
			be.Activate();
		}
		RemoveControlBlocks(Block.BattleSetup);
		foreach (Player player in currentPlayers)
		{
			player.ClearQueuedActions();
			player.Undown();
			player.Activate();
		}
		PostCtrl.transitioning = false;
		if (GameState != GState.GameOver)
		{
			GameState = GState.Battle;
		}
		stopWatch.Reset();
		sp.ResetChances();
	}

	public IEnumerator LastBossHitSlow()
	{
		GTimeScale = 0.3f;
		yield return new WaitForSeconds(0.05f);
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.3f);
		}
		GTimeScale = 1f;
	}

	public void EndPvPRound(Player losingPlayer)
	{
		bothPvPDead = false;
		if (co_PvPRoundEnd == null)
		{
			co_PvPRoundEnd = StartCoroutine(_EndPvPRound(losingPlayer));
		}
		else
		{
			bothPvPDead = true;
		}
	}

	private IEnumerator _EndPvPRound(Player losingPlayer)
	{
		AddControlBlocks(Block.PostBattle);
		yield return new WaitForSeconds(1f);
		foreach (Player player2 in currentPlayers)
		{
			player2.RemoveAllStatuses();
		}
		string winnerNameString = "";
		foreach (Player player in currentPlayers)
		{
			if (!(losingPlayer != player) || bothPvPDead)
			{
				continue;
			}
			player.duelDisk.AddWin();
			winnerNameString = player.beingObj.nameString;
			if (player.duelDisk.wins > pvpRequiredWins - 1)
			{
				U.I.Show(centralMessageContainer);
				audioSource.PlayOneShot(winRoundSound);
				centralMessageText.text = ScriptLocalization.UI.Match_Won;
				muCtrl.PauseIntroLoop();
				if (S.I.ANIMATIONS)
				{
					yield return new WaitForSeconds(1f);
				}
				U.I.Hide(centralMessageContainer);
				matchesPlayedThisSession++;
				co_PvPRoundEnd = null;
				Defeat();
				yield break;
			}
		}
		U.I.Show(centralMessageContainer);
		audioSource.PlayOneShot(winRoundSound);
		if (bothPvPDead)
		{
			centralMessageText.text = ScriptLocalization.UI.Battle_Tied;
		}
		else
		{
			centralMessageText.text = ScriptLocalization.UI.Battle_Won;
		}
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(1f);
		}
		U.I.Hide(centralMessageContainer);
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(1f);
		}
		muCtrl.PauseIntroLoop();
		camCtrl.TransitionInHigh("LeftWipe");
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(_SetupPvPRound(winnerNameString));
		co_PvPRoundEnd = null;
	}

	public IEnumerator EndBattle()
	{
		if (battleEnding)
		{
			yield break;
		}
		blockAllSummons = true;
		GameState = GState.EndBattle;
		battleEnding = true;
		yield return new WaitForSeconds(0.4f);
		if (S.I.RECORD_MODE)
		{
			yield return new WaitForSeconds(1.5f);
		}
		ti.mainBattleGrid.ClearField(true, false, false);
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.4f);
		}
		ti.mainBattleGrid.ClearField(true, false, false);
		deCtrl.TriggerAllArtifacts(FTrigger.OnBattleEnd);
		deCtrl.UpdatePacts();
		if (!currentPlayer)
		{
			battleEnding = false;
			yield break;
		}
		foreach (Player player6 in currentPlayers)
		{
			player6.RemoveAllStatuses();
		}
		if (runCtrl.currentRun.coOp)
		{
			if (currentPlayers.Count <= 0)
			{
				battleEnding = false;
				yield break;
			}
			if (currentPlayers.Count < playerHalves.Count)
			{
				for (int i = 0; i < playerHalves.Count; i++)
				{
					if (playerHalves[i].inTempDeath)
					{
						playerHalves[i].RestoreFromTempDeath();
					}
				}
			}
		}
		if (numBattlesLeft > 1)
		{
			foreach (Player player5 in currentPlayers)
			{
				player5.ApplyStun(false, true);
				player5.ClearQueuedActions();
				player5.mov.state = State.Idle;
			}
			if (S.I.ANIMATIONS)
			{
				yield return new WaitForSeconds(0.7f);
				U.I.Show(centralMessageContainer);
				audioSource.PlayOneShot(winRoundSound);
				centralMessageText.text = string.Format("Round {0} Won!", numBattles - numBattlesLeft + 1);
				yield return new WaitForSeconds(1f);
				U.I.Hide(centralMessageContainer);
				foreach (Player player4 in currentPlayers)
				{
					player4.anim.SetBool("dashing", true);
				}
				bgCtrl.MoveBG(0.7f);
				yield return new WaitForSeconds(0.7f);
				foreach (Player player3 in currentPlayers)
				{
					player3.anim.SetBool("dashing", false);
				}
			}
			foreach (Player player2 in currentPlayers)
			{
				player2.RemoveAllStatuses();
				player2.ApplyStun(false);
				player2.ClearQueuedActions();
			}
			AddControlBlocks(Block.BattleIntermission);
			ti.mainBattleGrid.ClearField(true);
			numBattlesLeft--;
			if (S.I.TESTING_MODE)
			{
				sp.StartCoroutine(sp.SpawnZoneC(ZoneType.Battle));
				StartCoroutine(StartBattle());
			}
			else
			{
				sp.StartCoroutine(sp.SpawnZoneC(ZoneType.Battle));
			}
			if (S.I.ANIMATIONS)
			{
				yield return new WaitForSeconds(0.6f);
				centralMessageText.text = string.Format("Round {0} Start!", numBattles - numBattlesLeft + 1);
				U.I.Show(centralMessageContainer);
				yield return new WaitForSeconds(0.5f);
			}
			yield return new WaitForSeconds(0.3f);
			U.I.Hide(centralMessageContainer);
			RemoveControlBlocks(Block.BattleIntermission);
			foreach (Player player in currentPlayers)
			{
				player.ClearQueuedActions();
				player.mov.state = State.Idle;
			}
			foreach (Cpu be3 in ti.mainBattleGrid.currentEnemies)
			{
				be3.Activate();
			}
			foreach (Being be2 in ti.mainBattleGrid.currentAllies)
			{
				be2.Activate();
			}
			foreach (Structure be in ti.mainBattleGrid.currentStructures)
			{
				be.Activate();
			}
		}
		else
		{
			idCtrl.controlsBackDisplay.HideTutorialControlBacklay();
			StartCoroutine(EndZone());
		}
	}

	public void BeingRemove(Being theBeing)
	{
		theBeing.battleGrid.currentBeings.Remove(theBeing);
		theBeing.battleGrid.currentDeadBeings.Add(theBeing);
		if (!currentPlayer || currentPlayer.dead)
		{
			if (theBeing.battleGrid.currentObstacles.Contains(theBeing))
			{
				theBeing.battleGrid.currentObstacles.Remove(theBeing);
			}
			return;
		}
		if (!theBeing.cleared)
		{
			experienceGained += theBeing.beingObj.experience;
			moneyGained += theBeing.beingObj.money;
			if (theBeing.battleGrid.currentObstacles.Contains(theBeing) && !theBeing.minion)
			{
				killedBeingSprites.Add(theBeing.sprite);
			}
			StartCoroutine(SpawnLootEffect(theBeing.beingObj.experience, theBeing.beingObj.money, theBeing.transform.position, DropItems(theBeing.beingObj.rewardList), theBeing.orbs));
		}
		if (!theBeing.battleGrid.currentObstacles.Contains(theBeing))
		{
			return;
		}
		theBeing.battleGrid.currentObstacles.Remove(theBeing);
		if (theBeing.battleGrid.currentObstacles.Count <= 0)
		{
			if (theBeing.lastSpellHit != null && theBeing.lastSpellHit.itemID == "Fadeaway")
			{
				AchievementsCtrl.UnlockAchievement("Buzzer_Beater");
				S.AddSkinUnlock("VioletteJRock");
			}
			StartCoroutine(EndBattle());
			if (theBeing.IsEnemy())
			{
				StartCoroutine(LastBossHitSlow());
			}
		}
	}

	public void AllyRemoved(Ally theAlly)
	{
		theAlly.battleGrid.currentAllies.Remove(theAlly);
		BeingRemove(theAlly);
	}

	public void StructureRemoved(Structure theStructure)
	{
		theStructure.battleGrid.currentStructures.Remove(theStructure);
		BeingRemove(theStructure);
	}

	public void EnemyRemoved(Enemy theEnemy)
	{
		theEnemy.battleGrid.currentEnemies.Remove(theEnemy);
		if (ti.mainBattleGrid != null && theEnemy.battleGrid == ti.mainBattleGrid && (bool)theEnemy.lastHitBy && theEnemy.lastHitBy.player != null)
		{
			IncrementStat("TotalEnemiesKilled");
		}
		lastKilledEnemy = theEnemy;
		BeingRemove(theEnemy);
	}

	private IEnumerator EndZone()
	{
		float battleTime = stopWatch.timeInSeconds;
		if (battleTime < 15f)
		{
			battleRank = "S";
		}
		else if (battleTime < 25f)
		{
			battleRank = "A";
		}
		else if (battleTime < 40f)
		{
			battleRank = "B";
		}
		else if (battleTime < 60f)
		{
			battleRank = "C";
		}
		else if (battleTime < 80f)
		{
			battleRank = "D";
		}
		else
		{
			battleRank = "F";
		}
		bool battleWasFlawless = perfectBattle && runCtrl.currentZoneDot.type != ZoneType.Distress && runCtrl.currentZoneDot.type != ZoneType.Treasure;
		if (S.I.ANIMATIONS)
		{
			muCtrl.PauseIntroLoop();
			yield return new WaitForSeconds(0.8f);
			if (runCtrl.currentZoneDot.type == ZoneType.Boss)
			{
				muCtrl.PlayExecution(runCtrl.currentRun.bossExecutions > 3 && runCtrl.currentRun.assists.Count < 1);
			}
			else
			{
				muCtrl.PlayVictory(battleWasFlawless);
			}
			foreach (Player player2 in currentPlayers)
			{
				player2.mov.BreakChannels();
			}
			if (battleWasFlawless)
			{
				centralMessageText.text = ScriptLocalization.UI.Battle_Won_Flawless;
				moneyGained += noHitMoneyBonus;
				shopCtrl.sera += noHitMoneyBonus;
			}
			else
			{
				centralMessageText.text = ScriptLocalization.UI.Battle_Won;
			}
			if (runCtrl.currentZoneDot.type != ZoneType.Distress && runCtrl.currentZoneDot.type != ZoneType.Treasure)
			{
				U.I.Show(centralMessageContainer);
				yield return new WaitForSeconds(0.11f);
				if (!AchievementsCtrl.IsUnlocked("Plausible_Deniability"))
				{
					bool noWeaponUses = true;
					foreach (Player thePlayer in currentPlayers)
					{
						if (thePlayer.weaponUsesThisBattle > 0 || thePlayer.spellsCastThisBattle.Count > 0)
						{
							noWeaponUses = false;
						}
					}
					if (noWeaponUses)
					{
						AchievementsCtrl.UnlockAchievement("Plausible_Deniability");
					}
				}
			}
			else
			{
				yield return new WaitForSeconds(0.2f);
			}
			yield return new WaitForSeconds(0.4f);
			U.I.Hide(centralMessageContainer);
			deCtrl.statsScreen.UpdateStats();
			yield return new WaitForSeconds(0.5f);
		}
		foreach (Player player in currentPlayers)
		{
			player.RemoveAllStatuses();
		}
		if ((bool)currentPlayer)
		{
			currentPlayer.duelDisk.CreateDeckSpells(false);
		}
		blockAllSummons = false;
		ti.mainBattleGrid.ClearField(true);
		idCtrl.controlsBackDisplay.HideTutorialControlBacklay();
		battleEnding = false;
		if ((bool)currentPlayer && !currentPlayer.dead)
		{
			poCtrl.StartPostBattle();
		}
	}

	public void ChangeScene(int sceneNum)
	{
		S.I.camCtrl.CameraChangePos(sceneNum);
	}

	public List<ItemObject> DropItems(List<string> dropList)
	{
		List<ItemObject> list = new List<ItemObject>();
		foreach (string drop in dropList)
		{
			ItemObject itemObj = itemMan.itemDictionary[drop];
			ListCard listCard = deCtrl.CreatePlayerItem(itemObj, deCtrl.deckScreen.deckGrid, -1, currentPlayer.duelDisk);
			list.Add(listCard.itemObj);
			listCard.anim.SetBool("spawned", false);
		}
		return list;
	}

	public IEnumerator SpawnLootEffect(int enemyExp, int enemyMoney, Vector3 spawnPos, List<ItemObject> droppedList, List<int> orbs)
	{
		int i = 0;
		while (i < enemyExp * 10 / 25 && (bool)currentPlayer)
		{
			RewardDrop droppedExp = SimplePool.Spawn(rewardDropPrefab.gameObject, spawnPos, base.transform.rotation).GetComponent<RewardDrop>();
			droppedExp.ctrl = this;
			droppedExp.target = currentPlayer.transform;
			int expInDrop2 = i;
			if (enemyExp * 10 / 25 - i >= 6)
			{
				i += 6;
				droppedExp.MakeExp(2);
			}
			else if (enemyExp * 10 / 25 - i >= 3)
			{
				i += 3;
				droppedExp.MakeExp(1);
			}
			else
			{
				droppedExp.MakeExp(0);
				i++;
			}
			expInDrop2 = (i - expInDrop2) * 25;
			droppedExp.name = "ExpOf" + expInDrop2;
			droppedExp.transform.parent = base.transform;
			S.I.DespawnAfter(droppedExp.gameObject, 1.5f);
			yield return new WaitForSeconds(0.02f);
		}
		i = 0;
		while (i < enemyMoney && (bool)currentPlayer)
		{
			RewardDrop droppedMoney = SimplePool.Spawn(rewardDropPrefab, spawnPos, base.transform.rotation).GetComponent<RewardDrop>();
			droppedMoney.ctrl = this;
			if (currentPlayer != null)
			{
				droppedMoney.target = currentPlayer.transform;
			}
			else
			{
				droppedMoney.target = idCtrl.moneyTextTarget;
			}
			int moneyInDrop2 = i;
			if (enemyMoney - i >= 25)
			{
				i += 25;
				droppedMoney.MakeMoney(2, 25);
			}
			else if (enemyMoney - i >= 5)
			{
				i += 5;
				droppedMoney.MakeMoney(1, 5);
			}
			else
			{
				droppedMoney.MakeMoney(0, 1);
				i++;
			}
			moneyInDrop2 = i - moneyInDrop2;
			droppedMoney.name = "MoneyOf" + moneyInDrop2;
			droppedMoney.transform.parent = base.transform;
			S.I.DespawnAfter(droppedMoney.gameObject, 1.5f);
			yield return new WaitForSeconds(0.02f);
		}
		foreach (ItemObject itemObj in droppedList)
		{
			if (currentPlayer == null)
			{
				break;
			}
			RewardDrop droppedItem = SimplePool.Spawn(rewardDropPrefab, spawnPos, base.transform.rotation).GetComponent<RewardDrop>();
			droppedItem.ctrl = this;
			droppedItem.MakeItem(itemObj);
			droppedItem.transform.parent = base.transform;
			droppedItem.name = itemObj.itemID + "drop";
			S.I.DespawnAfter(droppedItem.gameObject, 1.5f);
			yield return new WaitForSeconds(0.02f);
		}
		foreach (int orbNum in orbs)
		{
			if (currentPlayer == null)
			{
				break;
			}
			RewardDrop droppedOrb = SimplePool.Spawn(rewardDropPrefab, spawnPos, base.transform.rotation).GetComponent<RewardDrop>();
			droppedOrb.ctrl = this;
			droppedOrb.target = currentPlayer.transform;
			droppedOrb.MakeOrb(poCtrl.AddArtDrop(orbNum));
			droppedOrb.transform.parent = base.transform;
			droppedOrb.name = "Orb drop " + orbNum;
			S.I.DespawnAfter(droppedOrb.gameObject, 1.5f);
			yield return new WaitForSeconds(0.02f);
		}
	}

	private void CreateDroppedItemEffect(ItemObject itemObj, Vector3 spawnPos)
	{
		RewardDrop component = SimplePool.Spawn(rewardDropPrefab, spawnPos, base.transform.rotation).GetComponent<RewardDrop>();
		component.ctrl = this;
		component.MakeItem(itemObj);
		component.transform.parent = base.transform;
		component.name = itemObj.itemID + "drop";
		S.I.DespawnAfter(component.gameObject, 1.5f);
	}

	private void SaveRunHistory()
	{
		RunHistoryData item = new RunHistoryData(sp.beingDictionary[runCtrl.currentRun.beingID].nameString, runCtrl.currentRun.beingID, runCtrl.currentRun.seed, runCtrl.GetZoneText(runCtrl.currentRun.zoneNum));
		List<RunHistoryData> list = SaveDataCtrl.Get("RunHistories", new List<RunHistoryData>());
		if (list.Count > 3)
		{
			list.RemoveAt(0);
		}
		list.Add(item);
		SaveDataCtrl.Set("RunHistories", list);
	}

	public void Victory(Ending endingType, string transitionName)
	{
		StartCoroutine(_Victory(endingType, transitionName));
	}

	private IEnumerator _Victory(Ending endingType, string transitionName)
	{
		AddControlBlocks(Block.GameEnd);
		runStopWatch.Pause();
		SaveRunHistory();
		CheckVictoryAchievements(endingType);
		runCtrl.currentRun.skinUnlocks.Add(currentHeroObj.nameString + "Skin1");
		if (runCtrl.currentRun.genocide)
		{
			runCtrl.currentRun.charUnlocks.Add(currentHeroObj.nameString + "Gen");
		}
		foreach (Player player in currentPlayers)
		{
			credCtrl.preCreditHealthCurrent = player.health.current;
			credCtrl.preCreditHealthMax = player.health.max;
			player.health.SetHealth(99999, 99999);
			player.beingStatsPanel.healthDisplay.transform.localScale = Vector3.zero;
		}
		runCtrl.DeleteRun();
		if ((endingType == Ending.PacifistFalse && S.I.ANIMATIONS) || (endingType == Ending.Genocide && S.I.ANIMATIONS))
		{
			yield return new WaitForSeconds(0.8f);
		}
		if (runCtrl.currentRun.hellPassNum >= 7)
		{
			S.AddSkinUnlock("TerraDark");
		}
		camCtrl.TransitionInHigh(transitionName);
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(1.9f);
		}
		bgCtrl.ChangeBG("black");
		for (int k = ti.mainBattleGrid.currentObstacles.Count - 1; k >= 0; k--)
		{
			ti.mainBattleGrid.currentObstacles[k].FullClean();
		}
		for (int j = ti.mainBattleGrid.currentEnemies.Count - 1; j >= 0; j--)
		{
			ti.mainBattleGrid.currentEnemies[j].FullClean();
		}
		for (int i = ti.mainBattleGrid.currentStructures.Count - 1; i >= 0; i--)
		{
			ti.mainBattleGrid.currentStructures[i].FullClean();
		}
		if ((bool)currentPlayer)
		{
			currentPlayer.duelDisk.CreateDeckSpells(false);
		}
		if (endingType == Ending.PacifistFalse)
		{
			yield return new WaitForSeconds(0.8f);
		}
		U.I.Hide(centralMessageContainer);
		yield return StartCoroutine(cgCtrl._StartScene(endingType));
		cgCtrl.Close();
		credCtrl.StartCredits(endingType);
		while (credCtrl.creditsOngoing)
		{
			yield return null;
		}
		GameState = GState.GameOver;
		currentPlayer.duelDisk.ShowCardRefGrid(false);
		yield return new WaitForSeconds(0.6f);
		idCtrl.deckScreen.Close();
		gameOverPane.Open();
		gameOverPane.SetType(endingType);
		yield return new WaitForSeconds(0.6f);
		camCtrl.TransitionOutHigh("LeftWipe");
	}

	public void Defeat()
	{
		if (GameState != GState.GameOver)
		{
			StartCoroutine(_Defeat());
		}
	}

	private IEnumerator _Defeat()
	{
		SaveRunHistory();
		IncrementStat("TotalDeaths");
		GameState = GState.GameOver;
		runStopWatch.Pause();
		if ((runCtrl.currentRun != null && runCtrl.currentRun.neutral && runCtrl.currentRun.gateDefeated) || (runCtrl.currentRun != null && runCtrl.currentRun.loopNum > 0))
		{
			runCtrl.currentRun.charUnlocks.Add(currentHeroObj.nameString + "Alt");
			CheckVictoryAchievements(Ending.Neutral);
		}
		else if (!pvpMode)
		{
			SaveDataCtrl.Set("CurrentWinStreak", 0);
			SaveDataCtrl.Set(currentHeroObj.nameString + "CurrentWinStreak", 0);
			optCtrl.settingsPane.IncreaseAngelMultiplierMax();
		}
		idCtrl.runCtrl.worldBar.available = false;
		if (S.I.RECORD_MODE)
		{
			yield return new WaitForSeconds(1.6f);
		}
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.6f);
		}
		idCtrl.HideOnwardButton();
		foreach (Player player in currentPlayers)
		{
			player.duelDisk.ShowCardRefGrid(false);
		}
		idCtrl.HideOnwardButton();
		yield return new WaitForSeconds(0.6f);
		if (!pvpMode)
		{
			runCtrl.DeleteRun();
		}
		muCtrl.PlayGameOver(pvpMode);
		idCtrl.deckScreen.Close();
		if (S.I.scene == GScene.DemoLive)
		{
			ti.mainBattleGrid.ClearField();
			StartCoroutine(DemoEndC());
		}
		else
		{
			gameOverPane.Open();
		}
		yield return new WaitForSeconds(0.5f);
		for (int i = ti.mainBattleGrid.currentBeings.Count - 1; i >= 0; i--)
		{
			ti.mainBattleGrid.currentBeings[i].FullClean();
		}
	}

	private void CheckVictoryAchievements(Ending endingType)
	{
		if (runStopWatch.timeInSeconds < SaveDataCtrl.Get("FastestVictory", 0f) || SaveDataCtrl.Get("FastestVictory", 0f) <= 360f)
		{
			SaveDataCtrl.Set("FastestVictory", runStopWatch.timeInSeconds);
		}
		if (runStopWatch.timeInSeconds < SaveDataCtrl.Get(currentHeroObj.nameString + "FastestVictory", 0f) || SaveDataCtrl.Get(currentHeroObj.nameString + "FastestVictory", 0f) <= 360f)
		{
			SaveDataCtrl.Set(currentHeroObj.nameString + "FastestVictory", runStopWatch.timeInSeconds);
		}
		IncrementStat("CurrentWinStreak");
		if (SaveDataCtrl.Get("CurrentWinStreak", 0) > SaveDataCtrl.Get("LongestWinStreak", 0))
		{
			SaveDataCtrl.Set("LongestWinStreak", SaveDataCtrl.Get("CurrentWinStreak", 0));
		}
		if (SaveDataCtrl.Get(currentHeroObj.nameString + "CurrentWinStreak", 0) > SaveDataCtrl.Get(currentHeroObj.nameString + "LongestWinStreak", 0))
		{
			SaveDataCtrl.Set(currentHeroObj.nameString + "LongestWinStreak", SaveDataCtrl.Get(currentHeroObj.nameString + "CurrentWinStreak", 0));
		}
		for (int i = 0; i < runCtrl.currentHellPasses.Count; i++)
		{
			SaveDataCtrl.Set(currentHeroObj.beingID + "HellPassDefeated" + runCtrl.currentHellPasses[i], true);
			switch (endingType)
			{
			case Ending.Genocide:
				SaveDataCtrl.Set(currentHeroObj.beingID + "HellPassDefeatedGenocide" + runCtrl.currentHellPasses[i], true);
				break;
			default:
				if (endingType != 0)
				{
					if (endingType == Ending.Neutral)
					{
						SaveDataCtrl.Set(currentHeroObj.beingID + "HellPassDefeatedNeutral" + runCtrl.currentHellPasses[i], true);
					}
					break;
				}
				goto case Ending.PacifistFalse;
			case Ending.PacifistFalse:
				SaveDataCtrl.Set(currentHeroObj.beingID + "HellPassDefeatedPacifist" + runCtrl.currentHellPasses[i], true);
				break;
			}
		}
		IncrementStat("TotalVictories");
		if (!AchievementsCtrl.IsUnlocked("Ascetic") && !runCtrl.currentRun.artifactTaken)
		{
			AchievementsCtrl.UnlockAchievement("Ascetic");
		}
		if (!AchievementsCtrl.IsUnlocked("Bravely_Default"))
		{
			BeingObject beingObject = sp.heroDictionary[currentPlayer.beingObj.beingID];
			if ((bool)currentPlayer && currentPlayer.duelDisk.deck.Count <= beingObject.deck.Count)
			{
				bool flag = true;
				foreach (ListCard item in currentPlayer.duelDisk.deck)
				{
					if (!currentPlayer.baseBeingObj.deck.Contains(item.itemObj.itemID))
					{
						flag = false;
					}
				}
				if (flag)
				{
					AchievementsCtrl.UnlockAchievement("Bravely_Default");
				}
			}
		}
		if (runCtrl.currentRun.hellPasses.Contains(14))
		{
			AchievementsCtrl.UnlockAchievement("One_Shot");
		}
		if (!AchievementsCtrl.IsUnlocked("All_For_One"))
		{
			bool flag2 = true;
			foreach (BeingObject heroCampaign in sp.heroCampaignList)
			{
				if (SaveDataCtrl.Get(heroCampaign.nameString + "LongestWinStreak", 0) <= 0)
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				AchievementsCtrl.UnlockAchievement("All_For_One");
			}
		}
		if (runCtrl.currentRun.hellPassNum >= runCtrl.unlockedHellPassNum)
		{
			runCtrl.unlockedHellPassNum = runCtrl.currentRun.hellPassNum;
			SaveDataCtrl.Set("UnlockedHellPassNum", runCtrl.unlockedHellPassNum);
		}
		SaveDataCtrl.Write();
	}

	public void Restart(bool flashDark = true, bool skipStartup = false)
	{
		Debug.Log("Restarting...");
		if (S.I.currentProfile == 0)
		{
			SaveDataCtrl.Set("TotalPlaytime", SaveDataCtrl.Get("TotalPlaytime", 0) + runStopWatch.RoundedTimeInSeconds());
		}
		else
		{
			SaveDataCtrl.Set("TotalPlaytime" + S.I.currentProfile, SaveDataCtrl.Get("TotalPlaytime" + S.I.currentProfile, 0) + runStopWatch.RoundedTimeInSeconds(), true);
		}
		SaveDataCtrl.Set(currentHeroObj.nameString + "TotalPlaytime", SaveDataCtrl.Get(currentHeroObj.nameString + "TotalPlaytime", 0) + runStopWatch.RoundedTimeInSeconds());
		runStopWatch.Reset();
		GameState = GState.MainMenu;
		PostCtrl.transitioning = true;
		co_PvPRoundEnd = null;
		StopAllCoroutines();
		runCtrl.StopAllCoroutines();
		camCtrl.designEffects.SetTrigger("hollowDiamondsInstantOut");
		runCtrl.progressBar.anim.SetBool("visible", false);
		poCtrl.StopAllCoroutines();
		shopCtrl.sera = 0;
		runCtrl.xmlReader.prevSetNum = -9999;
		cgCtrl.unCtrl.itemGrids.blocksRaycasts = true;
		if (ti.mainBattleGrid != null)
		{
			foreach (Being currentBeing in ti.mainBattleGrid.currentBeings)
			{
				if ((bool)currentBeing)
				{
					currentBeing.StopSelfAndChildCoroutines();
				}
			}
			foreach (Projectile currentProjectile in ti.mainBattleGrid.currentProjectiles)
			{
				currentProjectile.StopAllCoroutines();
			}
			ti.mainBattleGrid.ClearProjectiles();
			for (int num = ti.mainBattleGrid.currentBeings.Count - 1; num >= 0; num--)
			{
				if ((bool)ti.mainBattleGrid.currentBeings[num])
				{
					ti.mainBattleGrid.currentBeings[num].FullClean();
				}
			}
			ti.mainBattleGrid.currentObstacles.Clear();
			ti.mainBattleGrid.currentBeings.Clear();
			ti.mainBattleGrid.currentEnemies.Clear();
			ti.mainBattleGrid.currentStructures.Clear();
			ti.mainBattleGrid.currentAllies.Clear();
			ti.mainBattleGrid.RestoreTileColor();
		}
		ClearPlayerDecks();
		pvpMode = false;
		gameOverPane.StopAllCoroutines();
		gameOverPane.Close();
		heCtrl.gameMode = GameMode.Solo;
		runCtrl.ResetRun();
		poCtrl.playerLevel = 1;
		poCtrl.experience = 0;
		poCtrl.luck = 0f;
		poCtrl.remainingArtDrops.Clear();
		baseBossTier = 0;
		experienceGained = 0;
		fragileMultiplier = fragileMultiplierBase;
		shieldDecay = shieldDecayBase;
		U.I.Hide(poCtrl.levelPane);
		numBattlesLeft = 0;
		camCtrl.healthOverlay.targetAlpha = 0f;
		battleEnding = false;
		credCtrl.Reset();
		sp.explosionGen.ResetRadius();
		blockAllSummons = false;
		Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
		if (rewiredPlayer != null)
		{
			rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Gameplay");
			rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay");
			rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay2");
		}
		HelpIconManager.SetKeyboardAvailable(0, true);
		idCtrl.spareVoteDisplay.gameObject.SetActive(false);
		idCtrl.executeVoteDisplay.gameObject.SetActive(false);
		S.I.twClient.EndVoting();
		shopCtrl.Close();
		deCtrl.artGrid.DestroyChildren();
		U.I.Hide(centralMessageContainer);
		gameOverPane.diamondBox.SetBool("visible", false);
		for (int num2 = btnCtrl.activeNavPanels.Count - 1; num2 >= 0; num2--)
		{
			btnCtrl.activeNavPanels[num2].Close();
		}
		runCtrl.worldBar.available = false;
		sp.CleanPlayers();
		idCtrl.HideOnwardButton();
		itemMan.defaultEnemyArtList.Clear();
		idCtrl.controlsBackDisplay.HideTutorialControlBacklay();
		mainCtrl.SetSelectorPoint(mainCtrl.startButton);
		PostCtrl.transitioning = false;
		GTimeScale = 1f;
		SettingsPane.masterMixer.SetFloat("musicPitch", 1f);
		SettingsPane.masterMixer.SetFloat("sfxPitch", 1f);
		camCtrl.TransitionOutHigh("Instant");
		gameOverPane.diamondBox.SetBool("visible", false);
		S.I.refCtrl.Hide();
		if (S.I.scene == GScene.DemoLive)
		{
			tutCtrl.ResetTutorialProgress();
			runCtrl.StartDemoLive();
		}
		else if (!skipStartup)
		{
			mainCtrl.Startup(0.1f, flashDark);
		}
	}

	public void ClearPlayerDecks()
	{
		killedBeingSprites.Clear();
		if (ti.mainBattleGrid != null)
		{
			ti.mainBattleGrid.ClearField();
		}
		if (pvpMode)
		{
			foreach (Player currentPlayer in currentPlayers)
			{
				if ((bool)currentPlayer)
				{
					currentPlayer.Clean();
				}
			}
			currentPlayers.Clear();
		}
		deCtrl.DestroyDuelDisks();
		deCtrl.ClearPlayerItems(0);
		deCtrl.ClearPlayerItems(1);
		if (ti.mainBattleGrid != null)
		{
			ti.ResetField(ti.mainBattleGrid, false);
		}
	}

	public Cpu GetEnemy(int enemyIndex)
	{
		return ti.mainBattleGrid.currentEnemies[enemyIndex];
	}

	public void DestroyEnemiesAndStructures(Being doNotKill = null)
	{
		DestroyEnemiesAndStructures(new List<Being> { doNotKill });
	}

	public void DestroyEnemiesAndStructures(List<Being> doNotKill = null)
	{
		List<Cpu> list = new List<Cpu>();
		foreach (Cpu currentEnemy in ti.mainBattleGrid.currentEnemies)
		{
			if (!doNotKill.Contains(currentEnemy))
			{
				list.Add(currentEnemy);
			}
		}
		foreach (Structure currentStructure in ti.mainBattleGrid.currentStructures)
		{
			if (!doNotKill.Contains(currentStructure))
			{
				list.Add(currentStructure);
			}
		}
		foreach (Cpu item in list)
		{
			item.StartDeath(false);
		}
	}

	public void ClickDemoEnd()
	{
		if (ti.mainBattleGrid != null)
		{
			ti.mainBattleGrid.ClearField();
		}
		StartCoroutine(DemoEndC());
		optCtrl.slideBody.Hide();
	}

	public IEnumerator DemoEndC(float endDelay = 0f)
	{
		yield return new WaitForSeconds(endDelay);
		AddControlBlocks(Block.DemoEnd);
		S.I.demoLiveCtrl.Open();
		yield return new WaitForEndOfFrame();
	}

	public IEnumerator ShowZoneButtons()
	{
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.5f);
		}
		idCtrl.onwardBtn.inverse = true;
		idCtrl.ShowOnwardButton();
		yield return new WaitForEndOfFrame();
	}

	public void AddObstacle(Being theBeing)
	{
		if (!theBeing.battleGrid.currentObstacles.Contains(theBeing))
		{
			theBeing.battleGrid.currentObstacles.Add(theBeing);
		}
	}

	public void RemoveObstacle(Being theBeing)
	{
		if (theBeing.battleGrid.currentObstacles.Contains(theBeing))
		{
			theBeing.battleGrid.currentObstacles.Remove(theBeing);
		}
	}

	public void PlaceStatusNoDuration(ItemObject item, Status status)
	{
		Being[] targets = GetTargets(item);
		foreach (Being being in targets)
		{
			being.AddStatus(status, GetAmount(item), 0f, item);
		}
	}

	public void PlaceStatusWithDuration(ItemObject item, Status status)
	{
		Being[] targets = GetTargets(item);
		foreach (Being being in targets)
		{
			being.AddStatus(status, GetAmount(item), GetDuration(item), item);
		}
	}

	public Being[] GetTargets(ItemObject item)
	{
		List<Being> list = new List<Being>();
		if (item.currentApp.target == Target.Default || item.currentApp.target == Target.It)
		{
			if (item.currentApp.fTrigger == FTrigger.OnHit)
			{
				list.Add(item.hitBeing);
			}
			else if (item.currentApp.fTrigger == FTrigger.OnEnemyHit)
			{
				if (item.being.battleGrid.lastTargetHit != null)
				{
					list.Add(item.being.battleGrid.lastTargetHit);
				}
			}
			else if (item.currentApp.fTrigger == FTrigger.OnEnemySpawn)
			{
				list.Add(item.ctrl.sp.lastSpawnedBeing);
			}
			else
			{
				list.Add(item.being);
			}
		}
		else if (item.currentApp.target == Target.Self)
		{
			list.Add(item.being);
		}
		else if (item.currentApp.target == Target.Hit)
		{
			if (item.forwardedTargetHit != null)
			{
				list.Add(item.forwardedTargetHit);
			}
			else if (item.being.battleGrid.lastTargetHit != null)
			{
				list.Add(item.being.battleGrid.lastTargetHit);
			}
		}
		else if (item.currentApp.target == Target.Enemies)
		{
			for (int num = item.being.battleGrid.currentEnemies.Count - 1; num >= 0; num--)
			{
				list.Add(item.being.battleGrid.currentEnemies[num]);
			}
		}
		else if (item.currentApp.target == Target.RandomEnemy)
		{
			if (item.being.battleGrid.currentEnemies.Count > 0)
			{
				list.Add(item.being.battleGrid.currentEnemies[UnityEngine.Random.Range(0, item.being.battleGrid.currentEnemies.Count - 1)]);
			}
		}
		else if (item.currentApp.target == Target.RandomOtherEnemy)
		{
			if (item.being.battleGrid.currentEnemies.Count > 0)
			{
				List<Cpu> list2 = new List<Cpu>(item.being.battleGrid.currentEnemies);
				for (int num2 = list2.Count - 1; num2 >= 0; num2--)
				{
					if (list2[num2] == item.forwardedTargetHit)
					{
						list2.Remove(list2[num2]);
					}
					else if (list2[num2] == item.being.battleGrid.lastTargetHit)
					{
						list2.Remove(list2[num2]);
					}
				}
				if (list2.Count > 0)
				{
					list.Add(list2[UnityEngine.Random.Range(0, list2.Count - 1)]);
				}
			}
		}
		else if (item.currentApp.target == Target.RandomStructure)
		{
			if (item.being.battleGrid.currentStructures.Count > 0)
			{
				list.Add(item.being.battleGrid.currentStructures[UnityEngine.Random.Range(0, item.being.battleGrid.currentStructures.Count - 1)]);
			}
		}
		else if (item.currentApp.target == Target.Player)
		{
			for (int num3 = item.ctrl.currentPlayers.Count - 1; num3 >= 0; num3--)
			{
				list.Add(item.ctrl.currentPlayers[num3]);
			}
		}
		else if (item.currentApp.target == Target.LastSpawned)
		{
			if ((bool)item.being.battleGrid.currentBeings[item.being.battleGrid.currentBeings.Count - 1])
			{
				list.Add(item.being.battleGrid.currentBeings[item.being.battleGrid.currentBeings.Count - 1]);
			}
		}
		else if (item.currentApp.target == Target.LastTargeted)
		{
			list.Add(item.being.lastTargeted);
		}
		else if (item.currentApp.target == Target.LastTargetedGlobal)
		{
			list.Add(item.being.battleGrid.lastTargetedGlobal);
		}
		else if (item.currentApp.target == Target.All)
		{
			for (int num4 = item.being.battleGrid.currentBeings.Count - 1; num4 >= 0; num4--)
			{
				list.Add(item.being.battleGrid.currentBeings[num4]);
			}
		}
		else if (item.currentApp.target == Target.Owner)
		{
			Debug.Log(item.being.parentObj.being.beingObj.nameString);
			list.Add(item.being.parentObj.being);
		}
		else if (item.currentApp.target == Target.Structures)
		{
			for (int num5 = item.being.battleGrid.currentStructures.Count - 1; num5 >= 0; num5--)
			{
				list.Add(item.being.battleGrid.currentStructures[num5]);
			}
		}
		if (list.Count > 0)
		{
			item.being.lastTargeted = list[0];
			item.being.battleGrid.lastTargetedGlobal = list[0];
		}
		return list.ToArray();
	}

	public float GetAmount(ItemObject itemObj)
	{
		return GetAmount(itemObj.currentApp.amountApp, itemObj.currentApp.amount, itemObj.spellObj, itemObj.artObj, itemObj.pactObj);
	}

	public float GetDuration(ItemObject itemObj)
	{
		return GetAmount(itemObj.currentApp.durationApp, itemObj.currentApp.duration, itemObj.spellObj, itemObj.artObj, itemObj.pactObj);
	}

	public float GetAmount(AmountApp amtApp, float inputAmt, SpellObject spellObj = null, ArtifactObject artObj = null, PactObject pactObj = null, bool label = false)
	{
		ItemObject itemObject = spellObj;
		if (itemObject == null)
		{
			itemObject = artObj;
			if (itemObject == null)
			{
				itemObject = pactObj;
			}
		}
		float num = inputAmt;
		if (amtApp == null)
		{
			return num;
		}
		switch (amtApp.type)
		{
		case AmountType.Normal:
			num = inputAmt;
			break;
		case AmountType.BrokenTiles:
		{
			int num3 = 0;
			if (itemObject != null)
			{
				Tile[,] grid3 = itemObject.being.battleGrid.grid;
				foreach (Tile tile3 in grid3)
				{
					if (tile3.type == TileType.Broken)
					{
						num3++;
					}
				}
			}
			else if (ti.mainBattleGrid != null)
			{
				Tile[,] grid4 = ti.mainBattleGrid.grid;
				foreach (Tile tile4 in grid4)
				{
					if (tile4.type == TileType.Broken)
					{
						num3++;
					}
				}
			}
			num = num3;
			break;
		}
		case AmountType.ConsumedSpells:
			if (itemObject != null && (bool)itemObject.being.player)
			{
				num = Mathf.FloorToInt(itemObject.being.player.consumedSpells);
			}
			else if ((bool)currentPlayer)
			{
				num = Mathf.FloorToInt(currentPlayer.consumedSpells);
			}
			break;
		case AmountType.CrackedTiles:
		{
			int num2 = 0;
			if (itemObject != null)
			{
				Tile[,] grid = itemObject.being.battleGrid.grid;
				foreach (Tile tile in grid)
				{
					if (tile.type == TileType.Cracked)
					{
						num2++;
					}
				}
			}
			else if (ti.mainBattleGrid != null)
			{
				Tile[,] grid2 = ti.mainBattleGrid.grid;
				foreach (Tile tile2 in grid2)
				{
					if (tile2.type == TileType.Cracked)
					{
						num2++;
					}
				}
			}
			num = num2;
			break;
		}
		case AmountType.CurrentCardtridges:
			if (itemObject != null && itemObject.being != null && itemObject.being.player != null && itemObject.being.player.duelDisk != null)
			{
				num = itemObject.being.player.duelDisk.currentCardtridges.Count;
			}
			break;
		case AmountType.CurrentMana:
			if (itemObject != null && (bool)itemObject.being.player)
			{
				num = Mathf.FloorToInt(itemObject.being.player.duelDisk.currentMana);
			}
			else if ((bool)currentPlayer)
			{
				num = Mathf.FloorToInt(currentPlayer.duelDisk.currentMana);
			}
			break;
		case AmountType.Damage:
			num = GetAmount(spellObj.damageType, spellObj.damage, spellObj);
			if ((bool)itemObject.being.player)
			{
				num += (float)itemObject.being.player.spellPower;
			}
			num += (float)itemObject.being.GetAmount(Status.SpellPower);
			num += (float)spellObj.tempDamage;
			num += (float)spellObj.permDamage;
			break;
		case AmountType.OriginDamage:
			if (spellObj.originSpell != null)
			{
				num = GetAmount(spellObj.originSpell.damageType, spellObj.originSpell.damage, spellObj.originSpell);
				if ((bool)itemObject.being.player)
				{
					num += (float)itemObject.being.player.spellPower;
				}
				num += (float)itemObject.being.GetAmount(Status.SpellPower);
				num += (float)spellObj.originSpell.tempDamage;
				num += (float)spellObj.originSpell.permDamage;
			}
			break;
		case AmountType.ParentDamage:
			if (spellObj.parentSpell != null)
			{
				num = GetAmount(spellObj.parentSpell.damageType, spellObj.parentSpell.damage, spellObj.parentSpell);
				if ((bool)itemObject.being.player)
				{
					num += (float)itemObject.being.player.spellPower;
				}
				num += (float)itemObject.being.GetAmount(Status.SpellPower);
				num += (float)spellObj.parentSpell.tempDamage;
				num += (float)spellObj.parentSpell.permDamage;
			}
			break;
		case AmountType.DamageDealtThisBattle:
			if (itemObject != null && itemObject.being != null)
			{
				num = itemObject.being.damageDealtThisBattle;
			}
			break;
		case AmountType.Flames:
			if (itemObject != null)
			{
				num = itemObject.being.battleGrid.currentFlames.Count;
			}
			else if (ti.mainBattleGrid != null)
			{
				num = ti.mainBattleGrid.currentFlames.Count;
			}
			break;
		case AmountType.FlowSelf:
		{
			object obj3;
			if (itemObject == null)
			{
				obj3 = null;
			}
			else
			{
				Being being2 = itemObject.being;
				obj3 = (((object)being2 != null) ? being2.GetStatusEffect(Status.Flow) : null);
			}
			num = ((!(UnityEngine.Object)obj3) ? 0f : itemObject.being.GetStatusEffect(Status.Flow).amount);
			break;
		}
		case AmountType.FragileSelf:
		{
			object obj;
			if (itemObject == null)
			{
				obj = null;
			}
			else
			{
				Being being = itemObject.being;
				obj = (((object)being != null) ? being.GetStatusEffect(Status.Fragile) : null);
			}
			num = ((!(UnityEngine.Object)obj) ? 0f : itemObject.being.GetStatusEffect(Status.Fragile).amount);
			break;
		}
		case AmountType.Frost:
		{
			object obj2;
			if (itemObject == null)
			{
				obj2 = null;
			}
			else
			{
				Being hitBeing = itemObject.hitBeing;
				obj2 = (((object)hitBeing != null) ? hitBeing.GetStatusEffect(Status.Frost) : null);
			}
			num = ((!(UnityEngine.Object)obj2) ? 0f : itemObject.hitBeing.GetStatusEffect(Status.Frost).amount);
			break;
		}
		case AmountType.HP:
			if (itemObject != null)
			{
				num = itemObject.being.health.current;
			}
			break;
		case AmountType.HitHP:
			if (itemObject != null && itemObject.being.battleGrid.lastTargetHit != null)
			{
				num = itemObject.being.battleGrid.lastTargetHit.health.current;
			}
			break;
		case AmountType.Infinite:
			num = 999999f;
			break;
		case AmountType.JamsCastThisBattle:
		{
			int num6 = 0;
			if (itemObject.being.battleGrid == ti.mainBattleGrid)
			{
				foreach (Player currentPlayer in currentPlayers)
				{
					num6 += currentPlayer.spellsCastThisBattle.Where((SpellObject t) => t.itemID == "Jam").ToList().Count;
				}
			}
			else if (itemObject.being.battleGrid == ti.refBattleGrid && S.I.refCtrl.genPlayer != null)
			{
				num6 += S.I.refCtrl.genPlayer.spellsCastThisBattle.Where((SpellObject t) => t.itemID == "Jam").ToList().Count;
			}
			num = num6;
			break;
		}
		case AmountType.JamsInDeck:
			if (itemObject != null && (bool)itemObject.being.player)
			{
				num = itemObject.being.player.duelDisk.currentCardtridges.Where((Cardtridge t) => t.spellObj.itemID == "Jam").ToList().Count;
			}
			else if ((bool)this.currentPlayer)
			{
				num = this.currentPlayer.duelDisk.currentCardtridges.Where((Cardtridge t) => t.spellObj.itemID == "Jam").ToList().Count;
			}
			break;
		case AmountType.LastDamageAmount:
			num = itemObject.being.lastDamageAmount;
			break;
		case AmountType.LastTrueDamageAmount:
			num = itemObject.being.lastTrueDamageAmount;
			break;
		case AmountType.ManaRegen:
			if (itemObject != null)
			{
				if ((bool)itemObject.being.player)
				{
					num = itemObject.being.player.manaRegen;
				}
			}
			else if ((bool)this.currentPlayer)
			{
				num = this.currentPlayer.manaRegen;
			}
			break;
		case AmountType.MaxHP:
			if (itemObject != null)
			{
				num = itemObject.being.health.max;
			}
			break;
		case AmountType.MaxMana:
			num = Mathf.FloorToInt(itemObject.being.player.maxMana);
			break;
		case AmountType.MissingHP:
			if (itemObject != null)
			{
				num = itemObject.being.health.max - itemObject.being.health.current;
			}
			break;
		case AmountType.TargetMissingHP:
			if (itemObject != null)
			{
				num = itemObject.being.battleGrid.lastTargetHit.health.max - itemObject.being.battleGrid.lastTargetHit.health.current;
			}
			break;
		case AmountType.ManaCost:
			num = spellObj.mana;
			break;
		case AmountType.Money:
			num = ((heCtrl.gameMode != GameMode.PvP || !itemObject.being.player) ? ((float)shopCtrl.sera) : ((float)itemObject.being.player.beingObj.money));
			break;
		case AmountType.OtherSlotDamage:
		{
			SpellObject spellObject2 = null;
			if (spellObj.castSlotNum == 0 && spellObj.being.player.duelDisk.castSlots[1].cardtridgeFill != null)
			{
				spellObject2 = spellObj.being.player.duelDisk.castSlots[1].cardtridgeFill.spellObj;
			}
			else if (spellObj.castSlotNum != 0 && spellObj.being.player.duelDisk.castSlots[0].cardtridgeFill != null)
			{
				spellObject2 = spellObj.being.player.duelDisk.castSlots[0].cardtridgeFill.spellObj;
			}
			else
			{
				num = 0f;
			}
			if (spellObject2 != null)
			{
				num = ((spellObject2.damageType.type == amtApp.type) ? ((float)spellObject2.CombinedDamage()) : GetAmount(spellObject2.damageType, spellObject2.CombinedDamage(), spellObject2, null, null, true));
			}
			break;
		}
		case AmountType.OtherSlotManaCost:
		{
			SpellObject spellObject = null;
			if (spellObj.castSlotNum == 0 && spellObj.being.player.duelDisk.castSlots[1].cardtridgeFill != null)
			{
				spellObject = spellObj.being.player.duelDisk.castSlots[1].cardtridgeFill.spellObj;
			}
			else if (spellObj.castSlotNum != 0 && spellObj.being.player.duelDisk.castSlots[0].cardtridgeFill != null)
			{
				spellObject = spellObj.being.player.duelDisk.castSlots[0].cardtridgeFill.spellObj;
			}
			else
			{
				num = 0f;
			}
			if (spellObject != null)
			{
				num = ((spellObject.manaType.type == amtApp.type) ? spellObject.mana : GetAmount(spellObject.manaType, spellObject.mana, spellObject));
			}
			break;
		}
		case AmountType.Poison:
			num = 0f;
			if (itemObject != null)
			{
				if (itemObject.forwardedTargetHit != null && (bool)itemObject.forwardedTargetHit.GetStatusEffect(Status.Poison))
				{
					num = itemObject.forwardedTargetHit.GetStatusEffect(Status.Poison).amount;
				}
				else if ((bool)itemObject.being.battleGrid.lastTargetHit && (bool)itemObject.being.battleGrid.lastTargetHit.GetStatusEffect(Status.Poison))
				{
					num = itemObject.being.battleGrid.lastTargetHit.GetStatusEffect(Status.Poison).amount;
				}
			}
			break;
		case AmountType.Shield:
			if ((bool)itemObject.being)
			{
				num = itemObject.being.health.shield;
			}
			break;
		case AmountType.ShieldPreCast:
			num = ((!label) ? ((float)spellObj.preCastShieldAmount) : ((float)itemObject.being.health.shield));
			break;
		case AmountType.ParentShots:
			if (spellObj.parentSpell != null)
			{
				num = GetAmount(spellObj.parentSpell.numShotsType, spellObj.parentSpell.numShots, spellObj.parentSpell);
			}
			break;
		case AmountType.SpellPower:
			num = 0f;
			if ((bool)itemObject.being.GetStatusEffect(Status.SpellPower))
			{
				num += (float)Mathf.RoundToInt(itemObject.being.GetStatusEffect(Status.SpellPower).amount);
			}
			if ((bool)itemObject.being.player && spellObj != null)
			{
				num += (float)itemObject.being.player.spellPower;
			}
			break;
		case AmountType.SpellsCastBattle:
			if (itemObject != null && (bool)itemObject.being.player)
			{
				num = itemObject.being.player.spellsCastThisBattle.Count;
			}
			else if ((bool)this.currentPlayer)
			{
				num = this.currentPlayer.spellsCastThisBattle.Count;
			}
			break;
		case AmountType.Structures:
			if (itemObject != null && (bool)itemObject.being)
			{
				num = itemObject.being.battleGrid.currentStructures.Where((Structure t) => !t.beingObj.tags.Contains(Tag.NotStructure)).ToList().Count;
			}
			break;
		case AmountType.WeaponDamage:
			if (itemObject != null && (bool)itemObject.being.player)
			{
				num = itemObject.being.player.equippedWep.damage;
				if ((bool)itemObject.being.GetStatusEffect(Status.AtkDmg))
				{
					num += (float)Mathf.RoundToInt(itemObject.being.GetStatusEffect(Status.AtkDmg).amount);
				}
				num += (float)itemObject.being.player.atkDmg;
			}
			else if ((bool)this.currentPlayer)
			{
				num = this.currentPlayer.equippedWep.damage;
				if ((bool)itemObject.being.GetStatusEffect(Status.AtkDmg))
				{
					num += (float)Mathf.RoundToInt(itemObject.being.GetStatusEffect(Status.AtkDmg).amount);
				}
				num += (float)itemObject.being.player.atkDmg;
			}
			break;
		case AmountType.Zero:
			num = 0f;
			break;
		default:
			num = 9238191f;
			break;
		}
		num *= amtApp.multiplier;
		num += (float)amtApp.initial;
		if (amtApp.min != -999999f)
		{
			num = Mathf.Clamp(num, amtApp.min, num);
		}
		if (amtApp.max != 999999f)
		{
			num = Mathf.Clamp(num, num, amtApp.max);
		}
		return num;
	}

	public void ActivateEvilHostages()
	{
		runCtrl.currentRun.evilHostages = true;
	}

	public void HostageSaved()
	{
		deCtrl.TriggerAllArtifacts(FTrigger.OnHostageSaved);
		if (runCtrl.currentRun != null)
		{
			IncrementStat("TotalHostagesSaved");
			runCtrl.currentRun.hostagesSavedStreak++;
			if (runCtrl.currentRun.hostagesSavedStreak < 12)
			{
			}
		}
	}

	public void HostageKilled()
	{
		deCtrl.TriggerAllArtifacts(FTrigger.OnHostageKilled);
		runCtrl.currentRun.hostagesSavedStreak = 0;
		runCtrl.currentRun.hostagesKilled++;
	}

	public void IncrementStat(string key, int value = 1)
	{
		SaveDataCtrl.Set(key, SaveDataCtrl.Get(key, 0) + value);
		SaveDataCtrl.Set(currentHeroObj.nameString + key, SaveDataCtrl.Get(currentHeroObj.nameString + key, 0) + value);
	}

	public void AddControlBlocks(Block controlBlock)
	{
		foreach (Player currentPlayer in currentPlayers)
		{
			currentPlayer.AddControlBlock(controlBlock);
		}
	}

	public void RemoveControlBlocks(Block controlBlock)
	{
		foreach (Player currentPlayer in currentPlayers)
		{
			currentPlayer.RemoveControlBlock(controlBlock);
		}
	}

	public void RemoveControlBlocksNextFrame(Block controlBlock)
	{
		StartCoroutine(_SetControllableAfter(controlBlock));
	}

	public IEnumerator _SetControllableAfter(Block controlBlock)
	{
		yield return new WaitForEndOfFrame();
		foreach (Player player in currentPlayers)
		{
			player.RemoveControlBlock(controlBlock);
		}
	}

	public void SetupPvP(ZoneType zoneType)
	{
		pvpMode = true;
		idCtrl.moneyTextBattle.gameObject.SetActive(false);
		sp.gameObject.SetActive(true);
		ti.gameObject.SetActive(true);
		StartCoroutine(_SetupPvP(zoneType));
	}

	public IEnumerator _SetupPvP(ZoneType zoneType)
	{
		matchesPlayedThisSession = 0;
		playerOneSessionWins = 0;
		playerTwoSessionWins = 0;
		ti.CreateMainField();
		PostCtrl.transitioning = false;
		while (S.I.coOpCtrl.controlsProcessing)
		{
			yield return null;
		}
		PostCtrl.transitioning = true;
		camCtrl.TransitionInHigh("LeftWipe");
		StartCoroutine(_RefreshPvP());
	}

	public IEnumerator _RefreshPvP()
	{
		ti.mainBattleGrid.ClearField(false, true);
		numBattlesLeft = numBattles;
		roundCounter = 0;
		ClearPlayerDecks();
		yield return new WaitForSeconds(0.4f);
		S.I.camCtrl.CameraChangePos(2);
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.5f);
		}
		ti.CreateMainField();
		StartCoroutine(_SetupPvPRound());
	}

	private IEnumerator _SetupPvPRound(string winnerNameString = null)
	{
		roundCounter++;
		ti.mainBattleGrid.ClearField(false, true);
		if (deCtrl.duelDisks.Count > 1)
		{
			sp.CleanPlayers();
			yield return new WaitForSeconds(0.1f);
			ti.CreateMainField();
			yield return new WaitForSeconds(0.4f);
		}
		else if (currentPlayers.Count < 1)
		{
			sp.CreatePvPPlayers();
			foreach (DuelDisk duelDisk in deCtrl.duelDisks)
			{
				duelDisk.SetWins(pvpRequiredWins);
			}
			yield return new WaitForSeconds(0.1f);
			sp.SpawnEnvironment();
			yield return new WaitForSeconds(0.1f);
		}
		if (currentPlayers.Count < 1)
		{
			sp.RestorePvPPlayers();
		}
		if (S.I.scene == GScene.PvP)
		{
			foreach (Player player in currentPlayers)
			{
				player.health.SetHealth(10);
			}
			currentPlayers[0].health.SetHealth(currentPlayers[0].health.current += 100, currentPlayers[0].health.current += 100);
		}
		conCtrl.SetActionsToPlayers();
		camCtrl.TransitionOutHigh("LeftWipe");
		PostCtrl.transitioning = false;
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.35f);
		}
		if (winnerNameString == null)
		{
			muCtrl.PlayBattle(UnityEngine.Random.Range(0, 2));
		}
		else
		{
			muCtrl.PlayCharacterTheme(winnerNameString);
		}
		U.I.Show(centralMessageContainer);
		centralMessageText.text = string.Format("{0} {1}", ScriptLocalization.UI.Battle_Round, roundCounter);
		yield return new WaitForSeconds(0.1f);
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(0.35f);
		}
		U.I.Hide(centralMessageContainer);
		PostCtrl.transitioning = false;
		StartCoroutine(StartBattle());
	}
}
