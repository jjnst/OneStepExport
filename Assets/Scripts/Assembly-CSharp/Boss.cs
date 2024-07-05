using System.Collections;
using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;

public class Boss : Enemy
{
	public string bossID;

	public Sprite introSprite;

	public string introDescription;

	public Color introBGColor;

	public AudioClip battleTheme;

	public List<int> healthStages;

	public List<AudioClip> soundEffects;

	protected float loopDelay = 5f;

	public int stage = 0;

	public int tier = 0;

	public bool downed = false;

	public bool introPlayed = false;

	public TI ti;

	private Coroutine co_Dialogue;

	protected bool dontExecutePlayer = false;

	public bool endGameOnExecute = true;

	protected Vector3 originalGunpointPos;

	public bool executionPlayed = false;

	public int testTier = 0;

	public bool normalTestPattern = false;

	private bool attemptedToExecute = false;

	public override void Start()
	{
		originalGunpointPos = gunPoint.transform.localPosition;
		tier = ctrl.baseBossTier;
		for (int i = 0; i < runCtrl.currentRun.worldTierNum; i += 2)
		{
			tier++;
		}
		if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
		{
			tier = testTier;
		}
		tier = Mathf.Clamp(tier, 0, 6);
		beingObj.defense += Mathf.FloorToInt(tier / 2);
		AddTierToNameText();
		Debug.Log("WorldTierNum: " + runCtrl.currentRun.worldTierNum + " Calculated Boss Tier:" + tier);
		if (ctrl.optCtrl.settingsPane.angelModeEnabled == 1)
		{
			loopDelay = beingObj.loopDelay - 0.01f * (float)tier;
			beingObj.movementDelay -= 0.005f * (float)tier;
			beingObj.lerpTime *= 1f + ctrl.optCtrl.settingsPane.angelModeCurrentSpeedReduction / 2f;
		}
		else
		{
			loopDelay = beingObj.loopDelay - 0.03f * (float)tier;
			beingObj.movementDelay -= 0.015f * (float)tier;
		}
		healthStages.Clear();
		health.max += Mathf.RoundToInt((float)health.max * 0.1f * (float)runCtrl.currentRun.worldTierNum);
		health.max = Mathf.RoundToInt(health.max / 100) * 100;
		health.current = health.max;
		foreach (ArtifactObject buff in buffs)
		{
			if (buff.itemID == "HellRegen")
			{
				float amountVar = Mathf.Clamp(5 * runCtrl.currentRun.worldTierNum, 5, 30);
				buff.GetEffect(Effect.HealBattle).amount = amountVar;
				buff.GetEffect(Effect.HealBattle).amountApp = new AmountApp(ref amountVar);
			}
		}
		for (int num = 2; num >= 0; num--)
		{
			if (num > 0)
			{
				healthStages.Add(health.current / 3 * num + 100);
			}
			else
			{
				healthStages.Add(health.current / 3 * num);
			}
		}
		if (S.I.EDITION == Edition.DemoLive)
		{
			beingObj.lethality = 100;
		}
		base.Start();
	}

	protected virtual void AddTierToNameText()
	{
		if (tier > 0)
		{
			TMP_Text tMP_Text = nameText;
			tMP_Text.text = tMP_Text.text + " " + (tier + 1);
		}
	}

	public virtual void StageCheck()
	{
		for (int num = healthStages.Count - 1; num >= 0; num--)
		{
			if (health.current > healthStages[num])
			{
				stage = num;
			}
		}
	}

	public virtual void LoopStart()
	{
		if (!ctrl.PlayersActive())
		{
			StopSelfAndChildCoroutines();
		}
	}

	public override void StartDeath(bool triggerDeathrattles = true)
	{
		ClearProjectiles();
		if (downed)
		{
			if (deathrattles.Count > 0 || !deathrattlesTriggered)
			{
				if (!inDeathSequence)
				{
					anim.SetTrigger("die");
					Timing.RunCoroutine(DeathrattlesC().CancelWith(base.gameObject));
				}
			}
			else
			{
				StartCoroutine(Executed());
			}
		}
		else
		{
			DownEffects();
			ApplyStun(false);
			StartCoroutine(DownC());
		}
	}

	public void Spare(ZoneDot nextZoneDot)
	{
		ApplyStun(false, true);
		invinceFlash = false;
		AddInvince(9f);
		base.dontInterruptAnim = false;
		dontInterruptChannelAnim = false;
		dontHitAnim = false;
		ctrl.RemoveObstacle(this);
		ctrl.IncrementStat("TotalSpares");
		mov.lerpTimeMods.Clear();
		Debug.Log("SPARING " + beingObj.nameString);
		StartCoroutine(SpareC(nextZoneDot));
		deCtrl.UpdatePacts();
		deCtrl.TriggerAllArtifacts(FTrigger.OnBossSpare);
	}

	protected virtual IEnumerator SpareC(ZoneDot nextZoneDot)
	{
		anim.SetTrigger("toIdle");
		talkBubble.Hide();
		ctrl.muCtrl.PauseIntroLoop();
		runCtrl.currentRun.AddAssist(beingObj);
		runCtrl.progressBar.SetBossFate();
		foreach (Player thePlayer in ctrl.currentPlayers)
		{
			thePlayer.RemoveStatus(Status.Poison);
		}
		ResetAnimTriggers();
		EndVoting();
		yield return new WaitForSeconds(0.3f);
		yield return StartCoroutine(_StartDialogue("Spare"));
		deCtrl.TriggerAllArtifacts(FTrigger.OnBattleEnd);
		yield return new WaitForSeconds(0.6f);
		anim.SetTrigger("throw");
		if (!runCtrl.currentRun.hellPasses.Contains(4))
		{
			ctrl.deCtrl.CreateSpellBase("BossHealShot", this).StartCast();
		}
		else
		{
			ctrl.deCtrl.CreateSpellBase("BossHealShotHell", this).StartCast();
		}
		ctrl.deCtrl.CreateSpellBase("BossHealSelf", this).StartCast();
		yield return new WaitForSeconds(0.3f);
		yield return new WaitForSeconds(1.7f);
		if (inDeathSequence)
		{
			yield break;
		}
		foreach (Player player in ctrl.currentPlayers)
		{
			player.RemoveAllStatuses();
			player.ApplyStun(false);
			player.ClearQueuedActions();
		}
		ctrl.idCtrl.runCtrl.GoToNextZone(nextZoneDot);
	}

	public virtual IEnumerator Executed()
	{
		runCtrl.progressBar.SetBossFate();
		EndVoting();
		ctrl.IncrementStat("TotalExecutions");
		runCtrl.currentRun.RemoveAssist(beingObj);
		if (endGameOnExecute)
		{
			ctrl.AddObstacle(this);
		}
		LastWord();
		ctrl.runCtrl.worldBar.Close();
		ctrl.idCtrl.HideOnwardButton();
		runCtrl.worldBar.available = false;
		foreach (Player thePlayer in ctrl.currentPlayers)
		{
			thePlayer.RemoveStatus(Status.Poison);
		}
		yield return new WaitForEndOfFrame();
		deCtrl.TriggerAllArtifacts(FTrigger.OnBossKill);
		talkBubble.Fade();
		runCtrl.currentRun.bossExecutions++;
		DeathEffects(false);
		Timing.RunCoroutine(_DeathFinal().CancelWith(base.gameObject));
	}

	protected virtual void LastWord()
	{
		talkBubble.StopAllCoroutines();
		talkBubble.InstantRandomLine(beingObj.nameString + "/Death");
		DownEffects();
	}

	protected virtual void DownEffects()
	{
		PauseSpells();
		foreach (Player currentPlayer in ctrl.currentPlayers)
		{
			currentPlayer.ApplyStun(false, true);
			currentPlayer.ClearProjectiles();
			currentPlayer.ClearQueuedActions();
			currentPlayer.RemoveControlBlock(Block.Fake);
		}
		battleGrid.ExtinguishFlames();
		base.dontInterruptAnim = false;
		dontInterruptChannelAnim = false;
		dontHitAnim = false;
		S.I.Flash();
		ctrl.StartCoroutine(ctrl.LastBossHitSlow());
		ResetAnimTriggers();
		RemoveAllBuffs();
		PlayOnce("bossdown_deep");
		runCtrl.savedBossKills++;
		SaveDataCtrl.Set("SavedBossKills", runCtrl.savedBossKills);
		runCtrl.tutorialBossKills++;
		SaveDataCtrl.Set("TutorialBossKills", runCtrl.tutorialBossKills);
		if (runCtrl.currentRun.worldTierNum > 1)
		{
			runCtrl.currentRun.charUnlocks.Add(beingObj.nameString);
		}
		ctrl.GameState = GState.EndBattle;
		CreateHitFX("down", true);
		SaveDataCtrl.Set("Beat_" + beingObj.beingID, 1);
		Ana.CustomEvent("beat_" + beingObj.beingID, new Dictionary<string, object>
		{
			{
				"lifetime_battles",
				SaveDataCtrl.Get("LifetimeBattles", 0)
			},
			{
				"time_elapsed",
				ctrl.runStopWatch.FormattedTime()
			}
		});
	}

	public virtual void ExecutePlayer()
	{
		ClearProjectiles();
		ctrl.DestroyEnemiesAndStructures(this);
		if (!downed && !dontExecutePlayer)
		{
			base.dontInterruptAnim = false;
			dontInterruptChannelAnim = false;
			dontHitAnim = false;
			ApplyStun(false, true);
			mov.lerpTimeMods.Clear();
			battleGrid.FixAllTiles();
			anim.SetTrigger("toIdle");
			if (!attemptedToExecute && Random.Range(1, 101) > beingObj.lethality + runCtrl.currentRun.hostagesKilled + runCtrl.currentRun.worldTierNum + runCtrl.currentRun.bossExecutions * 2)
			{
				StartCoroutine(_Mercy());
				return;
			}
			attemptedToExecute = true;
			StartCoroutine(ExecutePlayerC());
		}
	}

	public virtual IEnumerator _Mercy()
	{
		ClearProjectiles();
		DestroySpells();
		ctrl.DestroyEnemiesAndStructures(this);
		ctrl.muCtrl.PauseIntroLoop();
		ctrl.GameState = GState.EndBattle;
		yield return new WaitForSeconds(0.9f);
		ctrl.DestroyEnemiesAndStructures(this);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.HalfField), 1, ctrl.currentPlayer)[0]);
		yield return new WaitWhile(() => mov.state == State.Moving);
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(_StartDialogue("Mercy"));
		yield return new WaitForSeconds(0.35f);
		anim.SetTrigger("throw");
		ctrl.deCtrl.CreateSpellBase("BossMercyHealShot", this).StartCast();
		yield return new WaitForSeconds(0.8f);
		runCtrl.currentRun.AddAssist(beingObj);
		health.deathTriggered = false;
		col.enabled = true;
		ctrl.RemoveObstacle(this);
		yield return new WaitForSeconds(0.6f);
		anim.SetTrigger("back");
		yield return new WaitForSeconds(0.1f);
		foreach (Player thePlayer in ctrl.currentPlayers)
		{
			thePlayer.Undown();
		}
		deCtrl.UpdatePacts();
		if (S.I.scene != GScene.DemoLive)
		{
			ctrl.idCtrl.MakeWorldBarAvailable();
			ctrl.idCtrl.ShowOnwardButton();
		}
		yield return new WaitForEndOfFrame();
		Clear();
	}

	public virtual IEnumerator ExecutePlayerC()
	{
		yield return new WaitForEndOfFrame();
		Debug.Log("Execute player placeholder");
		yield return StartCoroutine(_StartDialogue("Execution"));
		yield return new WaitForSeconds(0.5f);
		if (ctrl.PlayersActive())
		{
			StartCoroutine(StartLoopC());
		}
	}

	private void EndVoting()
	{
		ctrl.idCtrl.spareVoteDisplay.gameObject.SetActive(false);
		ctrl.idCtrl.executeVoteDisplay.gameObject.SetActive(false);
		S.I.twClient.EndVoting();
	}

	public IEnumerator _StartDialogue(string key)
	{
		if (co_Dialogue != null)
		{
			StopCoroutine(co_Dialogue);
		}
		co_Dialogue = StartCoroutine(_Dialogue(key));
		yield return co_Dialogue;
	}

	private IEnumerator _Dialogue(string key)
	{
		if (key == "Intro")
		{
			if (introPlayed)
			{
				yield break;
			}
			introPlayed = true;
		}
		else if (key == "Execution")
		{
			if (executionPlayed)
			{
				yield break;
			}
			executionPlayed = true;
		}
		string chosenDialogue = talkBubble.GetRandomLine(beingObj.nameString + "/" + key + ctrl.currentPlayer.beingObj.nameString);
		if (string.IsNullOrEmpty(chosenDialogue) || Utils.RandomBool(3))
		{
			chosenDialogue = talkBubble.GetRandomLine(beingObj.nameString + "/" + key);
		}
		yield return new WaitForSeconds(talkBubble.AnimateText(chosenDialogue));
	}

	public float GetDelay(float baseDelay, float reductionPerTier)
	{
		for (int i = 1; i <= tier; i++)
		{
			baseDelay -= reductionPerTier;
		}
		return baseDelay;
	}

	public virtual IEnumerator DownC(bool destroyStructures = true, bool showZoneButtons = true)
	{
		if (destroyStructures)
		{
			ctrl.DestroyEnemiesAndStructures(this);
		}
		battleGrid.ClearProjectiles(true);
		S.I.muCtrl.PauseIntroLoop();
		AddInvince(1.6f);
		downed = true;
		dontMoveAnim = true;
		base.dontInterruptAnim = false;
		dontInterruptChannelAnim = false;
		anim.SetTrigger("down");
		ctrl.camCtrl.Shake(3);
		health.current = 1;
		RemoveAllStatuses();
		yield return new WaitForSeconds(0.1f);
		RemoveAllStatuses();
		foreach (Player thePlayer2 in ctrl.currentPlayers)
		{
			thePlayer2.RemoveStatus(Status.Poison);
			thePlayer2.RemoveAllStatuses();
		}
		if (tier >= 3)
		{
			AchievementsCtrl.UnlockAchievement("Defeat_" + beingObj.nameString);
		}
		if (ctrl.currentPlayer.health.current == 1)
		{
			AchievementsCtrl.UnlockAchievement("Not_Even_Close_Baby");
			S.AddSkinUnlock("ShisoCowboy");
		}
		yield return new WaitForSeconds(0.6f);
		battleGrid.ExtinguishFlames();
		RemoveAllStatuses();
		foreach (Player thePlayer in ctrl.currentPlayers)
		{
			thePlayer.RemoveStatus(Status.Poison);
		}
		if (destroyStructures)
		{
			ctrl.DestroyEnemiesAndStructures(this);
		}
		health.deathTriggered = false;
		col.enabled = true;
		if ((bool)GetComponent<Boss>() && S.I.scene != GScene.DemoLive && showZoneButtons)
		{
			ctrl.idCtrl.MakeWorldBarAvailable(true);
			ctrl.idCtrl.ShowOnwardButton();
		}
		if (S.I.scene != GScene.DemoLive)
		{
			StartCoroutine(DownTalkC());
		}
		else if (S.I.scene == GScene.DemoLive)
		{
			StartCoroutine(DownTalkL());
		}
		if (S.I.twClient.BossMercyVotingOnline())
		{
			S.I.twClient.voteDisplays.Clear();
			ctrl.idCtrl.spareVoteDisplay.gameObject.SetActive(true);
			ctrl.idCtrl.spareVoteDisplay.Reset(0);
			S.I.twClient.voteDisplays.Add(ctrl.idCtrl.spareVoteDisplay);
			ctrl.idCtrl.executeVoteDisplay.gameObject.SetActive(true);
			ctrl.idCtrl.executeVoteDisplay.Reset(1);
			S.I.twClient.voteDisplays.Add(ctrl.idCtrl.executeVoteDisplay);
			S.I.twClient.StartVoting();
		}
	}

	public virtual IEnumerator DownTalkC()
	{
		if (ctrl.perfectBattle)
		{
			yield return StartCoroutine(_StartDialogue("Flawless"));
		}
		else
		{
			yield return StartCoroutine(_StartDialogue("Downed"));
		}
	}

	public virtual IEnumerator DownTalkL()
	{
		talkBubble.Show();
		yield return new WaitForSeconds(0.1f);
		talkBubble.AnimateText("It's Saturday NIIIIGHT!");
		yield return new WaitForSeconds(1.8f);
		talkBubble.Hide();
	}
}
