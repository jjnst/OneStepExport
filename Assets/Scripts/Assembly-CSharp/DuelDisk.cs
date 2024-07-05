using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[MoonSharpUserData]
public class DuelDisk : MonoBehaviour
{
	public List<ListCard> deck;

	public List<Spell> currentDeck;

	public Player player;

	public List<Player> players = new List<Player>();

	public BeingObject beingObj;

	public float shuffleTime = 1f;

	public float nextShuffleTimeModifier = 0f;

	public Transform battleCardPanel;

	public List<DiskReference> diskRefs = new List<DiskReference>();

	public RectTransform cardGridCover;

	public DiskReference diskRefPrefab;

	public Transform castSlotsGrid;

	public List<CastSlot> castSlots;

	public List<Cardtridge> queuedCardtridges;

	public List<Cardtridge> currentCardtridges;

	public Transform cardtridgeSlotContainer;

	public GameObject winParticlePrefab;

	private List<Cardtridge> shuffleQueue = new List<Cardtridge>();

	public CardGrid cardGrid;

	public int clipSize;

	private float remainingShuffleTime = 0f;

	public bool shuffling = false;

	public bool manualShuffleDisabled = false;

	public int shufflesThisBattle = 0;

	private float quickQueueDuration = 0.01f;

	public FillBar manaBar;

	public FillBar healthBar;

	public List<SpellObject> temporarySpells = new List<SpellObject>();

	public float currentMana;

	public TMP_Text nameText;

	public Transform winsGrid;

	public CanvasGroup winIconPrefab;

	public Transform winsBackgroundGrid;

	public Transform winBackgroundIconPrefab;

	public Image profilePic;

	public bool matchDeckOrder = false;

	public int wins = 0;

	public BC ctrl;

	public DeckCtrl deCtrl;

	public StatsScreen statsScreen;

	private void Awake()
	{
		winsGrid.DestroyChildren();
		winsBackgroundGrid.DestroyChildren();
	}

	private void Update()
	{
		if (healthBar.gameObject.activeSelf && (bool)player)
		{
			healthBar.UpdateBar(player.health.current, player.health.max);
		}
		if (player.manaRegen >= 0f && currentMana < (float)player.maxMana)
		{
			currentMana += Time.deltaTime * player.manaRegen;
		}
		else if (player.manaRegen < 0f && currentMana > 0f)
		{
			currentMana += Time.deltaTime * player.manaRegen;
		}
		currentMana = Mathf.Clamp(currentMana, 0f, player.maxMana);
		manaBar.UpdateBar(currentMana, player.maxMana);
	}

	public void AddWin()
	{
		wins++;
		CanvasGroup canvasGroup = UnityEngine.Object.Instantiate(winIconPrefab, winsGrid);
		if (winsGrid.position.x < 0f)
		{
			canvasGroup.transform.SetAsFirstSibling();
		}
		SinFollower component = SimplePool.Spawn(winParticlePrefab, diskRefs[0].transform.position, base.transform.rotation).GetComponent<SinFollower>();
		component.target = canvasGroup.transform;
		StartCoroutine(WaitForExpParticle(canvasGroup, component));
	}

	private IEnumerator WaitForExpParticle(CanvasGroup winIcon, SinFollower expParticle)
	{
		winIcon.alpha = 0f;
		while (!expParticle.reachedTarget)
		{
			yield return null;
		}
		winIcon.alpha = 1f;
	}

	public void SetWins(int requiredWins)
	{
		wins = 0;
		winsGrid.DestroyChildren();
		winsBackgroundGrid.DestroyChildren();
		for (int i = 0; i < requiredWins; i++)
		{
			UnityEngine.Object.Instantiate(winBackgroundIconPrefab, winsBackgroundGrid);
		}
	}

	public void Setup(bool pvpMode, Player thePlayer)
	{
		nameText.gameObject.SetActive(pvpMode);
		healthBar.gameObject.SetActive(pvpMode);
		healthBar.maxLines = 1;
		foreach (Transform item in castSlotsGrid)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		castSlots.Clear();
		if (pvpMode && diskRefs.Count > 0)
		{
			foreach (DiskReference diskRef2 in diskRefs)
			{
				UnityEngine.Object.Destroy(diskRef2.gameObject);
			}
			diskRefs.Clear();
		}
		DiskReference diskRef = CreateDiskRef(thePlayer);
		for (int i = 0; i < 2; i++)
		{
			castSlots.Add(UnityEngine.Object.Instantiate(deCtrl.castSlotPrefab).GetComponent<CastSlot>().SetRef(this, castSlotsGrid, UnityEngine.Object.Instantiate(deCtrl.cardtridgePrefab, new Vector3(10000f, 10000f, 0f), base.transform.rotation).GetComponent<Cardtridge>(), i, diskRef));
		}
		CreateDeckSpells();
		for (int j = 0; j < 2; j++)
		{
			castSlots[j].inputIcon.playerNum = deCtrl.duelDisks.IndexOf(this);
		}
	}

	private DiskReference CreateDiskRef(Player thePlayer)
	{
		DiskReference diskReference = UnityEngine.Object.Instantiate(diskRefPrefab, base.transform.position, base.transform.rotation, base.transform);
		diskReference.cardSlotsReferenceGrid.DestroyChildren();
		diskReference.follower.following = thePlayer.transform;
		diskRefs.Add(diskReference);
		return diskReference;
	}

	public void AddPlayer(Player newPlayer)
	{
		newPlayer.duelDisk = this;
		if (player == null)
		{
			player = newPlayer;
		}
		else
		{
			players.Add(newPlayer);
			CreateDiskRef(newPlayer);
			castSlots[1].cardtridgeRef.transform.SetParent(diskRefs[diskRefs.Count - 1].cardSlotsReferenceGrid);
			castSlots[1].inputIcon.playerNum = 1;
			castSlots[1].inputIcon.UpdateDisplay();
			castSlots[1].player = newPlayer;
			diskRefs[0].cardSlotsReferenceGridRect.sizeDelta = diskRefs[0].cardSlotsReferenceGridRect.sizeDelta / 2f;
			diskRefs[1].cardSlotsReferenceGridRect.sizeDelta = diskRefs[1].cardSlotsReferenceGridRect.sizeDelta / 2f;
			diskRefs[0].cardSlotsReferenceGrid.transform.localPosition -= Vector3.right * (diskRefs[0].cardSlotsReferenceGridRect.sizeDelta.x / 2f - 1f);
			diskRefs[1].cardSlotsReferenceGrid.transform.localPosition += Vector3.right * (diskRefs[1].cardSlotsReferenceGridRect.sizeDelta.x / 2f - 1f);
		}
		ShowCardRefGrid(true);
	}

	private void OnDestroy()
	{
		foreach (CastSlot castSlot in castSlots)
		{
			if ((bool)castSlot.cardtridgeFill)
			{
				UnityEngine.Object.Destroy(castSlot.cardtridgeFill.gameObject);
			}
		}
	}

	public void CreateDeckSpells(bool audio = true)
	{
		if (player == null)
		{
			return;
		}
		StopAllCoroutines();
		shuffling = false;
		deCtrl.deckScreen.dirty = false;
		foreach (Spell item in currentDeck)
		{
			if ((bool)item)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		foreach (Cardtridge currentCardtridge in currentCardtridges)
		{
			if ((bool)currentCardtridge && (bool)currentCardtridge.gameObject)
			{
				SimplePool.Despawn(currentCardtridge.gameObject);
			}
		}
		foreach (Cardtridge queuedCardtridge in queuedCardtridges)
		{
			if ((bool)queuedCardtridge && (bool)queuedCardtridge.gameObject)
			{
				SimplePool.Despawn(queuedCardtridge.gameObject);
			}
		}
		foreach (SpellObject temporarySpell in temporarySpells)
		{
			if ((bool)temporarySpell.cardtridge)
			{
				SimplePool.Despawn(temporarySpell.cardtridge.gameObject);
			}
		}
		foreach (CastSlot castSlot in castSlots)
		{
			if ((bool)castSlot.cardtridgeFill)
			{
				SimplePool.Despawn(castSlot.cardtridgeFill.gameObject);
			}
			castSlot.Empty();
		}
		currentDeck.Clear();
		currentCardtridges.Clear();
		queuedCardtridges.Clear();
		DestroyTemporarySpells();
		clipSize = 0;
		for (int i = 0; i < deck.Count; i++)
		{
			currentDeck.Add(deck[i].spellObj.Set(player).spell);
			CreateCardtridge(currentDeck[currentDeck.Count - 1].spellObj, true);
			clipSize++;
		}
		if (deck.Count <= 1 && (bool)player && !player.IsReference() && !deCtrl.deckScreen.upgradeInProgress)
		{
			AchievementsCtrl.UnlockAchievement("Solo");
			S.AddSkinUnlock("GunnerBuccaneer");
		}
		shufflesThisBattle = 0;
		foreach (Player player in players)
		{
			player.RemoveAllStatuses();
			player.health.SetShield(0);
			statsScreen.UpdateStats(player);
		}
		StartCoroutine(ShuffleDeck(false, audio));
	}

	public void AddLiveSpell(ItemObject itemObj, string itemID, Being being, bool discard, bool front)
	{
		deCtrl.deckScreen.dirty = true;
		if (currentCardtridges.Count <= deCtrl.cardtridgeLimit)
		{
			Cardtridge cardtridge = null;
			cardtridge = ((itemObj == null || itemObj.type != 0 || !(itemID == "ThisSpell")) ? CreateCardtridge(deCtrl.CreateSpellBase(itemID, being, false), discard, front) : CreateCardtridge(itemObj.spellObj, discard, front));
			cardtridge.CreateShuffleTrail(diskRefs[0].transform.position, ctrl.transform);
		}
	}

	public Cardtridge CreateCardtridge(SpellObject spellObj, bool discard = false, bool front = false)
	{
		int num = 0;
		for (int i = 0; i < queuedCardtridges.Count; i++)
		{
			num++;
		}
		currentCardtridges.Add(SimplePool.Spawn(deCtrl.cardtridgePrefab, Vector3.one * 1000f, base.transform.rotation).GetComponent<Cardtridge>().Set(spellObj, this));
		if (shuffleQueue.Count > 0)
		{
			shuffleQueue.Add(currentCardtridges[currentCardtridges.Count - 1]);
		}
		else if (!discard)
		{
			if (!front)
			{
				QueueCardtridge(currentCardtridges[currentCardtridges.Count - 1]);
			}
			else if (front)
			{
				SwapCardtridge(currentCardtridges[currentCardtridges.Count - 1]);
			}
		}
		else
		{
			currentCardtridges[currentCardtridges.Count - 1].transform.SetParent(ctrl.transform);
		}
		cardGrid.UpdateCounter(queuedCardtridges.Count, currentCardtridges.Count);
		if (currentCardtridges.Count >= 200 && !player.IsReference())
		{
			AchievementsCtrl.UnlockAchievement("Hoarder");
		}
		return currentCardtridges[currentCardtridges.Count - 1];
	}

	public void SwapCardtridge(Cardtridge cardtridge)
	{
		QueueCardtridge(cardtridge, true);
		LaunchSlot(0, false);
		QueueCardtridge(castSlots[0].cardtridgeFill, true);
		cardGrid.MoveTo(cardGrid.transform.parent.position - Vector3.up * queuedCardtridges[0].transform.localPosition.y, true);
	}

	public void QueueCardtridge(Cardtridge cardtridge, bool front = false)
	{
		if (shuffling || queuedCardtridges.Contains(cardtridge) || cardtridge == null)
		{
			return;
		}
		if (front)
		{
			queuedCardtridges.Insert(0, cardtridge);
		}
		else
		{
			queuedCardtridges.Add(cardtridge);
		}
		clipSize++;
		cardtridge.Reset(cardGrid.transform, cardGrid.transform.position + new Vector3(0f, cardtridge.height * (clipSize - queuedCardtridges.Count + queuedCardtridges.IndexOf(cardtridge)), 0f));
		cardtridge.CreateShuffleTrail(diskRefs[0].transform.position, ctrl.transform);
		if (queuedCardtridges.Count == 1)
		{
			cardGrid.MoveTo(cardGrid.transform.parent.position - Vector3.up * queuedCardtridges[0].transform.localPosition.y, true);
		}
		for (int i = 0; i < castSlots.Count; i++)
		{
			if (castSlots[i].cardtridgeFill == null)
			{
				LoadSlot(i);
			}
		}
	}

	public void LaunchSlot(int slotNum, bool forceConsume, FloatingText spellText = null)
	{
		if (shuffling)
		{
			return;
		}
		castSlots[slotNum].Launch(forceConsume, spellText);
		if (queuedCardtridges.Count > 0)
		{
			for (int i = 0; i < castSlots.Count; i++)
			{
				if (castSlots[i].cardtridgeFill == null && queuedCardtridges.Count > 0)
				{
					LoadSlot(i);
				}
			}
		}
		else
		{
			bool flag = true;
			foreach (CastSlot castSlot in castSlots)
			{
				if (castSlot.spellObj != null || castSlot.cardtridgeFill != null)
				{
					flag = false;
				}
			}
			if (flag)
			{
				StartCoroutine(ShuffleDeck());
			}
		}
		cardGrid.UpdateCounter(queuedCardtridges.Count, currentCardtridges.Count);
	}

	public void LoadSlot(int slotNum)
	{
		if (queuedCardtridges.Count <= 0 || (diskRefs.Count >= 2 && !diskRefs[slotNum].gameObject.activeSelf))
		{
			return;
		}
		foreach (CastSlot castSlot in castSlots)
		{
			if (castSlot.cardtridgeFill == queuedCardtridges[0])
			{
				castSlot.Empty();
			}
		}
		queuedCardtridges[0].spellObj.castSlotNum = slotNum;
		castSlots[slotNum].Load(queuedCardtridges[0]);
		queuedCardtridges.RemoveAt(0);
		cardGrid.UpdateCounter(queuedCardtridges.Count, currentCardtridges.Count);
		if (queuedCardtridges.Count > 0)
		{
			cardGrid.MoveTo(cardGrid.transform.parent.position - Vector3.up * queuedCardtridges[0].transform.localPosition.y, true);
		}
	}

	private IEnumerator _RequeueCards(float queueDuration, float finalLoadSlotDuration, bool audio)
	{
		clipSize = 0;
		int startingQueueSize = shuffleQueue.Count;
		int count = shuffleQueue.Count;
		while (shuffleQueue.Count > 0)
		{
			int predictedShuffleQueueSize = Mathf.Clamp(Mathf.RoundToInt((float)startingQueueSize * (remainingShuffleTime - finalLoadSlotDuration) / (queueDuration - finalLoadSlotDuration)), 0, shuffleQueue.Count);
			if (S.I.scene == GScene.SpellLoop)
			{
				predictedShuffleQueueSize = 0;
			}
			while (shuffleQueue.Count > predictedShuffleQueueSize)
			{
				clipSize++;
				int cardIndex = UnityEngine.Random.Range(0, shuffleQueue.Count);
				if (S.I.MATCH_DECK_ORDER || matchDeckOrder)
				{
					cardIndex = 0;
				}
				shuffleQueue[cardIndex].RemoveFromShuffleQueue(shuffleQueue, queuedCardtridges, cardGrid, diskRefs[0].transform.position, ctrl.transform);
			}
			yield return null;
		}
		if (!player.IsReference() && audio)
		{
			S.I.PlayOnce(deCtrl.shuffleEndSound);
		}
		for (int i = 0; i < castSlots.Count; i++)
		{
			castSlots[i].Empty();
			LoadSlot(i);
			yield return new WaitForSeconds(deCtrl.loadSlotDuration);
		}
		if (shufflesThisBattle > 0)
		{
			player.TriggerAllArtifacts(FTrigger.OnReshuffleEnd);
		}
		yield return null;
		shuffling = false;
		remainingShuffleTime = 0f;
		shufflesThisBattle++;
	}

	public IEnumerator ShuffleDeck(bool manual = false, bool audio = true)
	{
		if (currentCardtridges.Count < 1)
		{
			yield break;
		}
		while (shuffling)
		{
			yield return null;
		}
		shuffling = true;
		shuffleQueue = new List<Cardtridge>(currentCardtridges);
		if (!player.IsReference() && audio)
		{
			S.I.PlayOnce(deCtrl.shuffleSound);
		}
		float calculatedShuffleTime = shuffleTime;
		float shuffleStaleness = Mathf.Clamp((float)shufflesThisBattle * deCtrl.shuffleStalenessMultiplier, 0f, deCtrl.shuffleStalenessCap - nextShuffleTimeModifier);
		if (manual)
		{
			calculatedShuffleTime *= 0.8f;
		}
		calculatedShuffleTime += nextShuffleTimeModifier;
		calculatedShuffleTime += shuffleStaleness;
		nextShuffleTimeModifier = 0f;
		if (shufflesThisBattle > 0)
		{
			player.TriggerAllArtifacts(FTrigger.OnReshuffle);
		}
		cardGrid.MoveTo(cardGrid.transform.parent.position + new Vector3(0f, currentCardtridges[0].height * 2, 0f));
		cardGrid.shuffleTimer.fillAmount = 0f;
		cardGrid.shuffleSpinner.gameObject.SetActive(true);
		foreach (DiskReference diskRef3 in diskRefs)
		{
			diskRef3.shuffler.fillAmount = 0f;
			diskRef3.shuffler.gameObject.SetActive(true);
			diskRef3.shuffleBorder.gameObject.SetActive(true);
		}
		float currentLerpTime = 0f;
		bool requeueStarted = false;
		float finalLoadSlotDuration = deCtrl.loadSlotDuration * 2f;
		float queueDuration = (float)shuffleQueue.Count * deCtrl.requeueTime + finalLoadSlotDuration;
		if (ctrl.GameState == GState.Idle || shufflesThisBattle < 1)
		{
			calculatedShuffleTime = 0.1f;
			queueDuration = (float)shuffleQueue.Count * quickQueueDuration + finalLoadSlotDuration;
		}
		calculatedShuffleTime += queueDuration;
		calculatedShuffleTime = (remainingShuffleTime = Mathf.Clamp(calculatedShuffleTime, 0.001f, calculatedShuffleTime));
		while (remainingShuffleTime > 0f || !requeueStarted)
		{
			cardGrid.shuffleTimer.fillAmount = 1f - remainingShuffleTime / calculatedShuffleTime;
			foreach (DiskReference diskRef in diskRefs)
			{
				diskRef.shuffler.fillAmount = 1f - remainingShuffleTime / calculatedShuffleTime;
			}
			currentLerpTime += Time.deltaTime * 2f;
			float t2 = currentLerpTime;
			t2 = Mathf.Sin(t2 * (float)Math.PI) + 1.6f;
			if (remainingShuffleTime < queueDuration && !requeueStarted)
			{
				StartCoroutine(_RequeueCards(queueDuration, finalLoadSlotDuration, audio));
				requeueStarted = true;
			}
			cardGrid.shuffleSpinner.Rotate(Vector3.forward * 500f * t2 * Time.deltaTime);
			remainingShuffleTime -= Time.deltaTime;
			yield return null;
		}
		cardGrid.shuffleTimer.fillAmount = 0f;
		cardGrid.shuffleSpinner.gameObject.SetActive(false);
		foreach (DiskReference diskRef2 in diskRefs)
		{
			diskRef2.shuffleBorder.gameObject.SetActive(false);
			diskRef2.shuffler.fillAmount = 0f;
		}
		int beltLength = currentCardtridges.Count * currentCardtridges[0].height * 2 - 24;
		cardGrid.maxBeltLength = beltLength;
		if (beltLength > 264)
		{
			beltLength = 264;
		}
		if (shufflesThisBattle > 0)
		{
			foreach (Player thePlayer in ctrl.currentPlayers)
			{
				thePlayer.health.ModifyShield(Mathf.RoundToInt((float)(-thePlayer.health.shield) * BC.shieldDecay));
			}
		}
		cardGrid.belt.GetComponent<RectTransform>().sizeDelta = new Vector2(cardGrid.belt.GetComponent<RectTransform>().sizeDelta.x, beltLength);
	}

	public void PvPMode(int duelDiskCount)
	{
		healthBar.gameObject.SetActive(true);
		if (duelDiskCount > 0)
		{
			FlipAll();
		}
		manaBar.Resize(70);
	}

	private void FlipAll()
	{
		FlipPos(nameText.transform);
		nameText.alignment = TextAlignmentOptions.TopRight;
		FlipPos(winsGrid);
		winsGrid.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
		FlipPos(winsBackgroundGrid);
		winsBackgroundGrid.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
		FlipPos(battleCardPanel);
		FlipPos(castSlotsGrid);
		FlipPos(cardGrid.transform.parent);
		FlipPos(cardGridCover);
		cardGridCover.transform.localScale = new Vector3(-1f, 1f, 1f);
		FlipPos(manaBar.transform);
		FlipPos(manaBar.gem);
		FlipPos(manaBar.barContainer);
		manaBar.Flip();
		FlipPos(healthBar.transform);
		FlipPos(healthBar.gem);
		FlipPos(healthBar.barContainer);
		FlipPos(cardGrid.cardCounter.transform);
		healthBar.Flip();
	}

	private void FlipPos(Transform transformToFlip)
	{
		transformToFlip.localPosition = new Vector3(transformToFlip.localPosition.x * -1f, transformToFlip.localPosition.y, transformToFlip.localPosition.z);
	}

	public void ShowCardRefGrid(bool show, int num = -1)
	{
		for (int i = 0; i < diskRefs.Count; i++)
		{
			if (num < 0 || num == i)
			{
				diskRefs[i].gameObject.SetActive(show);
			}
		}
	}

	public void DestroyTemporarySpells()
	{
		for (int num = temporarySpells.Count - 1; num >= 0; num--)
		{
			if ((bool)temporarySpells[num].spell)
			{
				if (temporarySpells[num].parentSpell != null && temporarySpells[num].parentSpell.generatedSpell != null)
				{
					temporarySpells[num].parentSpell.generatedSpell = null;
				}
				UnityEngine.Object.Destroy(temporarySpells[num].spell.gameObject);
			}
		}
		temporarySpells.Clear();
	}

	public void ManualShuffle()
	{
		if (shuffling)
		{
			return;
		}
		foreach (Cardtridge queuedCardtridge in queuedCardtridges)
		{
			queuedCardtridge.Eject();
		}
		foreach (CastSlot castSlot in castSlots)
		{
			if ((bool)castSlot.cardtridgeFill)
			{
				castSlot.cardtridgeFill.Eject();
			}
			castSlot.Empty();
		}
		queuedCardtridges.Clear();
		StartCoroutine(ShuffleDeck(true));
	}

	public void Clear()
	{
		deck.Clear();
	}
}
