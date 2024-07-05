using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using SonicBloom.Koreo;
using SonicBloom.Koreo.Players;
using UnityEngine;

public class BossViolette : Boss
{
	public bool attacking = false;

	public bool sequence = false;

	public bool randomBeat = false;

	public bool speakerDrop = false;

	public bool spotlights = false;

	public bool notePath = false;

	public bool notePathInfinite = false;

	public bool sheetMusic = false;

	private ViolettePath viPath;

	public bool noteInQueue = false;

	public List<Structure> currentSpeakers;

	public Koreography playingKoreo;

	public List<Korevent> events = new List<Korevent>();

	public List<Korevent> measures = new List<Korevent>();

	private double bpm;

	private BGCtrl bgCtrl;

	private MusicCtrl muCtrl;

	[EventID]
	public string eventID;

	public float noteTime = 1f;

	public float noteScale = 1f;

	public int pendingEventIdw = 0;

	public int pendingEventIda = 0;

	public int pendingEventIdm = 0;

	public int pendingEventIdma = 0;

	private float leadInTimeLeft;

	private float timeLeftToPlay;

	private double measureLength = 0.0;

	private int measureSampleLength = 0;

	private int currentMeasureNum = 0;

	private int currentHalfMeasureNum = 0;

	public int notePathMeasureDuration = 8;

	public bool playingSheetMusic = false;

	public bool reset = false;

	private bool playVisualizer = true;

	private KoreographyEvent currentEvt;

	public int testPat = 0;

	private int xNum = 1;

	public int SampleRate
	{
		get
		{
			return playingKoreo.SampleRate;
		}
	}

	public int DelayedSampleTime
	{
		get
		{
			return playingKoreo.GetLatestSampleTime() - (int)(muCtrl.audioSource.pitch * leadInTimeLeft * (float)SampleRate);
		}
	}

	public override void Start()
	{
		base.Start();
		bgCtrl = S.I.bgCtrl;
		muCtrl = S.I.muCtrl;
		muCtrl.Play(battleTheme);
		muCtrl.Stop();
		viPath = legacySpellList[1].GetComponent<ViolettePath>();
		muCtrl.GetComponent<SimpleMusicPlayer>().LoadSong(playingKoreo);
		muCtrl.GetComponent<SimpleMusicPlayer>().Pause();
		StartCoroutine(StopMusic());
		eventID = playingKoreo.GetEventIDs()[0];
		Koreographer.Instance.RegisterForEvents(eventID, Shoot);
		testPat = 0;
	}

	private IEnumerator StopMusic()
	{
		yield return null;
		yield return null;
		muCtrl.StopIntroLoop();
	}

	public override IEnumerator Loop()
	{
		if (!introPlayed)
		{
			yield return StartCoroutine(_StartDialogue("Intro"));
			InitializeLeadIn();
		}
		if (downed)
		{
			StartCoroutine(StopMusic());
			yield break;
		}
		int arger = 0;
		while (true)
		{
			LoopStart();
			if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE && !normalTestPattern)
			{
				if (!sequence)
				{
					if (notePath)
					{
						yield return StartCoroutine(_NotePath());
					}
					else if (speakerDrop)
					{
						if (currentSpeakers.Count >= 2)
						{
							yield return new WaitForSeconds(1f);
							continue;
						}
						yield return StartCoroutine(_SpeakerDrop());
					}
					else if (sheetMusic)
					{
						yield return StartCoroutine(_SheetMusic());
					}
				}
				yield return new WaitForSecondsRealtime(2f);
				continue;
			}
			if (stage == 0)
			{
				if (testPat == 0)
				{
					yield return StartCoroutine(_NotePath());
					testPat++;
				}
				else if (Utils.RandomBool(2) && currentSpeakers.Count == 0)
				{
					yield return StartCoroutine(_NotePath());
				}
				else if (Utils.RandomBool(2) && currentSpeakers.Count == 0)
				{
					yield return StartCoroutine(_SpeakerDrop());
				}
				else
				{
					yield return StartCoroutine(_SheetMusic());
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(2) && currentSpeakers.Count == 0)
				{
					yield return StartCoroutine(_NotePath());
				}
				else if (Utils.RandomBool(3) && currentSpeakers.Count == 0)
				{
					yield return StartCoroutine(_SpeakerDrop());
				}
				else
				{
					yield return StartCoroutine(_SheetMusic());
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(2) && currentSpeakers.Count == 0)
				{
					yield return StartCoroutine(_NotePath());
				}
				else if (Utils.RandomBool(3) && currentSpeakers.Count == 0)
				{
					yield return StartCoroutine(_SpeakerDrop());
				}
				else
				{
					yield return StartCoroutine(_SheetMusic());
				}
			}
			while (loopDelay > 0f)
			{
				loopDelay -= Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			for (int i = 0; i < 3; i++)
			{
				while (!noteInQueue)
				{
					yield return null;
				}
				mov.PatrolRandomEmpty();
				yield return new WaitUntil(() => mov.state == State.Idle);
			}
			StageCheck();
			arger++;
		}
	}

	private void InitializeLeadIn()
	{
		float num = 0.5f;
		if (num > 0f)
		{
			leadInTimeLeft = num;
			timeLeftToPlay = num - Koreographer.Instance.EventDelayInSeconds;
			return;
		}
		muCtrl.audioSource.time = 0f - num;
		if (!downed)
		{
			muCtrl.Resume();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (leadInTimeLeft > 0f)
		{
			leadInTimeLeft = Mathf.Max(leadInTimeLeft - Time.unscaledDeltaTime, 0f);
		}
		if (timeLeftToPlay > 0f)
		{
			timeLeftToPlay -= Time.unscaledDeltaTime;
			if (!(timeLeftToPlay <= 0f))
			{
				return;
			}
			muCtrl.audioSource.time = 0f - timeLeftToPlay;
			if (!downed)
			{
				muCtrl.Resume();
			}
			playingKoreo = Koreographer.Instance.GetKoreographyAtIndex(0);
			KoreographyTrackBase trackByID = playingKoreo.GetTrackByID(eventID);
			List<KoreographyEvent> allEvents = trackByID.GetAllEvents();
			foreach (KoreographyEvent item in allEvents)
			{
				events.Add(new Korevent(item));
			}
			bpm = playingKoreo.GetBPM(DelayedSampleTime);
			measureLength = 1.0 / (bpm / 60.0) * 4.0 / 2.0;
			measureSampleLength = Mathf.RoundToInt((float)SampleRate * (float)measureLength);
			int num = 0;
			int num2 = 0;
			while (num * measureSampleLength < events[events.Count - 1].EndSample + 100000)
			{
				if (num2 > 2)
				{
					num2 = 0;
				}
				measures.Add(new Korevent(num * measureSampleLength, num * measureSampleLength, noteTime, num2));
				num2++;
				num++;
				if (num > 9999)
				{
					break;
				}
			}
			num = 0;
			int num3 = 1;
			foreach (Korevent @event in events)
			{
				if (num < measures.Count && measures[num].StartSample < @event.StartSample)
				{
					num3 = 1;
					num++;
				}
				@event.warningTime = noteTime;
				@event.measureNum = num;
				@event.measurePos = num3;
				num3++;
			}
			timeLeftToPlay = 0f;
			return;
		}
		if (reset)
		{
			if (DelayedSampleTime > events[pendingEventIdw].StartSample)
			{
				{
					foreach (Korevent event2 in events)
					{
						event2.triggered = false;
						event2.speakerWarningShown = false;
						event2.tiles.Clear();
					}
					return;
				}
			}
			reset = false;
			playingSheetMusic = false;
			viPath.StopPathing();
			for (int num4 = activeProjectiles.Count - 1; num4 >= 0; num4--)
			{
				activeProjectiles[num4].Despawn();
			}
			viPath.ResetSong();
		}
		if (!downed)
		{
			CheckVisualizer();
			CheckSpawnWarning();
			CheckSpawnBomb();
		}
	}

	private int GetSpawnSampleOffset(float warningTime)
	{
		double num = warningTime;
		return (int)(num * (double)SampleRate);
	}

	private void CheckVisualizer()
	{
		if (measures.Count <= currentMeasureNum || !playVisualizer)
		{
			return;
		}
		if (DelayedSampleTime > measures[currentMeasureNum].StartSample)
		{
			bgCtrl.visualizer.SetTrigger("burst");
			currentMeasureNum++;
			foreach (Structure currentSpeaker in currentSpeakers)
			{
				currentSpeaker.anim.SetTrigger("fire");
			}
		}
		if (DelayedSampleTime <= measures[currentHalfMeasureNum].StartSample + measureSampleLength / 2)
		{
			return;
		}
		bgCtrl.visualizer.SetTrigger("expand");
		currentHalfMeasureNum++;
		foreach (Structure currentSpeaker2 in currentSpeakers)
		{
			currentSpeaker2.anim.SetTrigger("fire");
		}
	}

	private void CheckSpawnWarning()
	{
		for (int i = 0; i < 10; i++)
		{
			int num = pendingEventIdw + i;
			if (num >= events.Count || events[num].StartSample >= DelayedSampleTime + GetSpawnSampleOffset(events[num].warningTime))
			{
				continue;
			}
			if (pendingEventIdw + i > events.Count)
			{
				return;
			}
			if (events[pendingEventIdw].triggered)
			{
				pendingEventIdw++;
				continue;
			}
			if (!this || downed)
			{
				return;
			}
			events[num].triggered = true;
			viPath.CreateWarningPath();
			WarnSheetMusic(events[num]);
			if (i == 0)
			{
				pendingEventIdw++;
			}
		}
		if (pendingEventIdm < measures.Count && measures[pendingEventIdm].StartSample < DelayedSampleTime + GetSpawnSampleOffset(measures[pendingEventIdm].warningTime))
		{
			WarnSpeakers();
			pendingEventIdm++;
			if (pendingEventIdm >= measures.Count)
			{
				pendingEventIdm = 0;
			}
		}
		if (pendingEventIdw >= events.Count)
		{
			pendingEventIdw = 0;
		}
	}

	private void CheckSpawnBomb()
	{
		while (pendingEventIda < events.Count && events[pendingEventIda].StartSample < DelayedSampleTime)
		{
			if (pendingEventIda < 1)
			{
				pendingEventIda++;
				return;
			}
			if (!this || downed)
			{
				return;
			}
			if (events[pendingEventIda].tiles.Count > 0)
			{
				savedTileList = new List<Tile>(events[pendingEventIda].tiles);
				if (savedTileList.Count > 0 && (bool)spellObjList[3].spell)
				{
					spellObjList[3].StartCast();
				}
			}
			viPath.CreateShotPath();
			pendingEventIda++;
			if (pendingEventIda >= events.Count)
			{
				reset = true;
				pendingEventIdw = 0;
				pendingEventIda = 0;
				pendingEventIdm = 0;
				pendingEventIdma = 0;
				currentMeasureNum = 0;
				currentHalfMeasureNum = 0;
			}
		}
		while (pendingEventIdma < measures.Count && measures[pendingEventIdma].StartSample < DelayedSampleTime)
		{
			if (tier < 2 && pendingEventIdma % 2 == 0)
			{
				pendingEventIdma++;
				break;
			}
			foreach (Structure currentSpeaker in currentSpeakers)
			{
				if (!currentSpeaker.dead && measures[pendingEventIdma].speakerWarningShown)
				{
					currentSpeaker.spellObjList[measures[pendingEventIdma].patternNum].StartCast();
				}
			}
			pendingEventIdma++;
			if (pendingEventIdma >= events.Count)
			{
				pendingEventIdma = 0;
			}
		}
	}

	public override void StartAction()
	{
		mov.PatrolRandom();
	}

	private IEnumerator _NotePath()
	{
		yield return new WaitForSeconds(0.5f);
		bool savedPerfectBattle = ctrl.perfectBattle;
		SpellObject punishSpell = spellObjList[6];
		if (ctrl.currentPlayers.Count > 1)
		{
			punishSpell = GetSpellObj("ViPathPunishCoOp");
		}
		while (true)
		{
			if (pendingEventIdw + 1 >= events.Count)
			{
				yield break;
			}
			if (events[pendingEventIdw + 1].measurePos == 1 && events[pendingEventIdw + 1].measureNum % 2 != 0)
			{
				break;
			}
			yield return null;
		}
		events[pendingEventIdw + 1].warningTime = 1f;
		base.dontInterruptAnim = true;
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		while (events[pendingEventIdw].measurePos > 1 || events[pendingEventIdw].measureNum % 2 == 0)
		{
			yield return null;
		}
		int pendingEventIdw2 = pendingEventIdw;
		int startMeasure = events[pendingEventIdw].measureNum;
		int timesFollowedViolettePath = SaveDataCtrl.Get("TimesFollowedViolettePath", 0);
		if (timesFollowedViolettePath < 2)
		{
			talkBubble.AnimateText(LocalizationManager.GetTranslation("Violette/PathStart1"));
		}
		viPath.StartPathing();
		int infinite = 0;
		if (notePathInfinite)
		{
			infinite = 9999;
		}
		float moveCooldown = 0f;
		float moveBase = 1.6f;
		while (startMeasure + notePathMeasureDuration + infinite > events[pendingEventIdw].measureNum)
		{
			if (tier > 0)
			{
				moveCooldown += Time.deltaTime;
				if (moveCooldown > moveBase)
				{
					moveCooldown -= moveBase;
					mov.PatrolRandomEmpty();
				}
			}
			if (events[pendingEventIdw].measureNum < startMeasure - notePathMeasureDuration)
			{
				break;
			}
			yield return null;
		}
		viPath.StopPathing();
		punishSpell.GetEffect(Effect.Damage).amount = viPath.notesPlayed * int.Parse(punishSpell.Param("damagePerNote"));
		if (tier < 2)
		{
			punishSpell.GetEffect(Effect.Damage).amount *= 0.75f;
		}
		if (timesFollowedViolettePath < 2 && ctrl.currentPlayers.Count < 2 && (bool)ctrl.currentPlayer && (float)ctrl.currentPlayer.health.shield < (float)(viPath.notesPlayed * int.Parse(punishSpell.Param("damagePerNote"))) * 0.5f)
		{
			yield return new WaitForSeconds(0.3f);
			yield return StartCoroutine(_StartDialogue("PathEnd"));
		}
		else
		{
			SaveDataCtrl.Set("TimesFollowedViolettePath", timesFollowedViolettePath + 1);
			yield return new WaitForSeconds(1.01f);
		}
		anim.SetTrigger("release");
		if (punishSpell.GetEffect(Effect.Damage).amount > 0f)
		{
			punishSpell.StartCast();
		}
		yield return new WaitForSeconds(0.5f);
		ctrl.perfectBattle = savedPerfectBattle;
		loopDelay = 0f;
		base.dontInterruptAnim = false;
		yield return new WaitForSeconds(0.5f);
	}

	public void Shoot(KoreographyEvent evt)
	{
		if ((bool)this && !downed)
		{
			noteInQueue = true;
			currentEvt = evt;
			StartCoroutine(TurnOffNoteInQueueC());
		}
	}

	private IEnumerator TurnOffNoteInQueueC()
	{
		yield return new WaitForEndOfFrame();
		noteInQueue = false;
	}

	private IEnumerator _SpeakerDrop()
	{
		yield return new WaitForSeconds(1.2f);
		anim.SetTrigger("throw");
		spellObjList[4].StartCast();
		yield return new WaitForSeconds(4.2f);
	}

	private IEnumerator _SheetMusic()
	{
		while (timeLeftToPlay > 0f)
		{
			yield return null;
		}
		while (events[pendingEventIdw].measurePos > 1 || events[pendingEventIdw].measureNum % 2 == 0)
		{
			yield return null;
		}
		base.dontInterruptAnim = true;
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		int pendingEventIdw2 = pendingEventIdw;
		int startMeasure = events[pendingEventIdw].measureNum;
		xNum = 1;
		playingSheetMusic = true;
		float moveCooldown = 0f;
		float moveBase = 1.6f;
		if (tier > 2)
		{
			moveBase = 0.8f;
		}
		while (startMeasure + 8 > events[pendingEventIdw].measureNum && events[pendingEventIdw].measureNum >= startMeasure)
		{
			if (tier > 0)
			{
				moveCooldown += Time.deltaTime;
				if (moveCooldown > moveBase)
				{
					moveCooldown -= moveBase;
					mov.PatrolRandomEmpty();
					if (tier > 3)
					{
						CastSpellObj("ViFollowHorizontal");
					}
					else
					{
						CastSpellObj("ViFollow");
					}
				}
			}
			yield return null;
		}
		playingSheetMusic = false;
		yield return new WaitForSeconds(1f);
		loopDelay = 0f;
		anim.SetTrigger("release");
		base.dontInterruptAnim = false;
	}

	private void UpdateSpeakerList()
	{
		currentSpeakers.Clear();
		foreach (Structure currentStructure in battleGrid.currentStructures)
		{
			if (currentStructure.beingObj.beingID == "ViSpeaker")
			{
				currentSpeakers.Add(currentStructure);
			}
		}
	}

	private void WarnSpeakers()
	{
		UpdateSpeakerList();
		foreach (Structure currentSpeaker in currentSpeakers)
		{
			if (tier < 2 && pendingEventIdm % 2 == 0)
			{
				break;
			}
			measures[pendingEventIdm].speakerWarningShown = true;
			if (measures[pendingEventIdm].patternNum == 0)
			{
				foreach (Tile item in battleGrid.Get(new TileApp(Location.Tile, Shape.O, Pattern.All, 1, currentSpeaker.mov.currentTile), 0, currentSpeaker))
				{
					viPath.CreateWarning(item, measures[pendingEventIdm]);
				}
			}
			else if (measures[pendingEventIdm].patternNum == 1)
			{
				foreach (Tile item2 in battleGrid.Get(new TileApp(Location.Tile, Shape.ConeDouble, Pattern.ExcludeSelf, 1, currentSpeaker.mov.currentTile), 8, currentSpeaker))
				{
					viPath.CreateWarning(item2, measures[pendingEventIdm]);
				}
			}
			else
			{
				if (measures[pendingEventIdm].patternNum != 2)
				{
					continue;
				}
				foreach (Tile item3 in battleGrid.Get(currentSpeaker.spellObjList[measures[pendingEventIdm].patternNum].tileApps[0], 0, currentSpeaker))
				{
					viPath.CreateWarning(item3, measures[pendingEventIdm]);
				}
			}
		}
	}

	private void WarnSheetMusic(Korevent evt)
	{
		if (!playingSheetMusic)
		{
			return;
		}
		string textValue = evt.GetTextValue();
		List<int> list = Utils.RandomList(4);
		int num = 0;
		switch (textValue)
		{
		case "a":
			num = 0;
			break;
		case "a#":
			num = 1;
			break;
		case "c":
			num = 2;
			break;
		case "c#":
			num = 3;
			break;
		case "f":
			num = 0;
			break;
		case "f#":
			num = 1;
			break;
		case "g#":
			num = 3;
			break;
		default:
			num = Random.Range(0, 4);
			break;
		}
		evt.tiles = new List<Tile>();
		evt.tiles.Add(battleGrid.grid[4 - xNum, num]);
		list.Remove(num);
		num -= 2;
		if (num < 0)
		{
			num += 4;
		}
		evt.tiles.Add(battleGrid.grid[4 - xNum, num]);
		list.Remove(num);
		if (tier > 1)
		{
			num = list[Random.Range(0, list.Count)];
			evt.tiles.Add(battleGrid.grid[4 - xNum, num]);
			list.Remove(num);
		}
		if (tier > 3)
		{
			num = list[Random.Range(0, list.Count)];
			evt.tiles.Add(battleGrid.grid[4 - xNum, num]);
			list.Remove(num);
		}
		foreach (Tile tile in evt.tiles)
		{
			viPath.CreateWarning(tile, evt);
		}
		xNum++;
		if (xNum > 4)
		{
			xNum = 1;
		}
	}

	protected override void DownEffects()
	{
		base.DownEffects();
		playingSheetMusic = false;
		viPath.StopPathing();
		muCtrl.Stop();
		muCtrl.StopIntroLoop();
		StartCoroutine(StopMusic());
	}

	public override void Clear(bool overrideDeathSequence = false)
	{
		muCtrl.Stop();
		base.Clear();
	}

	public override void ExecutePlayer()
	{
		playVisualizer = false;
		viPath.StopPathing();
		playingSheetMusic = false;
		base.ExecutePlayer();
	}

	public override IEnumerator ExecutePlayerC()
	{
		yield return new WaitForSeconds(0.6f);
		viPath.StopPathing();
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.HalfField), 1, ctrl.currentPlayer)[0]);
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return new WaitForSeconds(0.3f);
		yield return StartCoroutine(_StartDialogue("Execution"));
		yield return new WaitForSeconds(0.3f);
		CastSpellObj("ViExecute");
		anim.SetTrigger("charge");
		anim.SetTrigger("channel");
		yield return new WaitForSeconds(1.9f);
		anim.SetTrigger("release");
		yield return new WaitForSeconds(0.8f);
		if ((bool)ctrl.currentPlayer)
		{
			StartCoroutine(StartLoopC());
		}
	}
}
