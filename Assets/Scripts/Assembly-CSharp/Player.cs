using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using I2.Loc;
using MEC;
using MoonSharp.Interpreter;
using Rewired;
using UnityEngine;

[MoonSharpUserData]
public class Player : Being
{
	public BeingObject baseBeingObj;

	public float basicCooldown = 1f;

	public float basicCooldownTime = 0f;

	public int atkDmg;

	public SpellObject equippedWep;

	public DuelDisk duelDisk;

	public Transform petSlot;

	public int maxMana;

	public float manaRegen;

	public float startingMana = 1f;

	public float manaBeforeSpellCast = 0f;

	public List<GameObject> tempDeckList;

	public bool actionQueued = false;

	public AudioClip castSound;

	public int spellPower;

	private AnimatorOverrideController overrideController;

	private List<KeyValuePair<AnimationClip, AnimationClip>> overrides;

	public InputAction queuedAction;

	public Rewired.Player rewiredPlayer;

	public bool controllable = true;

	public List<Block> controlBlocks = new List<Block>();

	public SpriteRenderer aimMarker;

	public bool downed = false;

	public bool wrap = false;

	public List<SpellObject> spellsCastThisBattle = new List<SpellObject>();

	public int weaponUsesThisBattle = 0;

	public int consumedSpells = 0;

	[NonSerialized]
	public FloatingText lastSpellText;

	public float chronoTime;

	private int currentHealthInt = 0;

	public bool undownForAssist = true;

	public bool petSnap = false;

	private float timeSinceLastNeedMoreMana = 0f;

	public ItemObject wrapObj;

	public ItemObject manualShuffleDisabledObj;

	public SpellObject spellToCast;

	private float holdTimer = 0f;

	private float holdStartDelay = 0.2f;

	private float holdInputDelay = 0.15f;

	private bool holding = false;

	private Direction holdDirection = Direction.None;

	public virtual void BattleStartReset()
	{
		ctrl.IncrementStat("TotalDamageDealt", damageDealtThisBattle);
		damageDealtThisBattle = 0;
		ctrl.IncrementStat("TotalStepsTaken", stepsTakenThisBattle);
		stepsTakenThisBattle = 0;
		weaponUsesThisBattle = 0;
		duelDisk.currentMana = startingMana;
		spellsCastThisBattle.Clear();
		consumedSpells = 0;
		basicCooldownTime = -999f;
	}

	public override void Reset()
	{
		base.Reset();
		BattleStartReset();
		RemoveControlBlock(Block.Fake);
		ClearQueuedActions();
	}

	public override void SetAnimatorController(RuntimeAnimatorController newAnimatorController)
	{
		base.SetAnimatorController(newAnimatorController);
		overrideController = newAnimatorController as AnimatorOverrideController;
		overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
		overrideController.GetOverrides(overrides);
	}

	public override void Start()
	{
		base.Start();
		basicCooldownTime = -999f;
		anim.updateMode = AnimatorUpdateMode.UnscaledTime;
	}

	protected override void Update()
	{
		chronoTime = BC.playerChronoTime;
		anim.speed = BC.playerChronoScale;
		base.Update();
		TriggerAllArtifacts(FTrigger.WhileManaBelow);
		foreach (CastSlot castSlot in duelDisk.castSlots)
		{
			if (castSlot.player == this)
			{
				castSlot.TriggerHold();
			}
		}
		foreach (CastSlot castSlot2 in duelDisk.castSlots)
		{
			if (castSlot2.player == this)
			{
				castSlot2.PUpdate();
			}
		}
		if (!IsReference())
		{
			if (health.current + health.shield < 100 && !ctrl.pvpMode)
			{
				ctrl.camCtrl.healthOverlay.targetAlpha = 1f - (float)(health.current + health.shield) / 100f;
			}
			else
			{
				ctrl.camCtrl.healthOverlay.targetAlpha = 0f;
			}
			if (currentHealthInt != health.current)
			{
				currentHealthInt = health.current;
				ctrl.idCtrl.idleHealthText.text = currentHealthInt + "/" + health.max;
			}
		}
		if (controlBlocks.Count < 1 && rewiredPlayer != null && !ControllerDisconnectCtrl.controllerDisconnectInProgress && !UIInputFieldVirtualKeyboard.inputCaptureInProgress)
		{
			if (rewiredPlayer.GetButton("Shuffle") && !deCtrl.deckScreen.slideBody.onScreen && ctrl.btnCtrl.IsActivePanel(ctrl.poCtrl))
			{
				ctrl.poCtrl.HoldContinue();
			}
			if (rewiredPlayer.GetButtonDown("Gameplay_Left"))
			{
				Timing.RunCoroutine(_QueueAction(InputAction.Move_Left));
			}
			else if (rewiredPlayer.GetButtonDown("Gameplay_Right"))
			{
				Timing.RunCoroutine(_QueueAction(InputAction.Move_Right));
			}
			else if (rewiredPlayer.GetButtonDown("Gameplay_Up"))
			{
				Timing.RunCoroutine(_QueueAction(InputAction.Move_Up));
			}
			else if (rewiredPlayer.GetButtonDown("Gameplay_Down"))
			{
				Timing.RunCoroutine(_QueueAction(InputAction.Move_Down));
			}
			else if (rewiredPlayer.GetButtonDown("Weapon"))
			{
				Timing.RunCoroutine(_QueueAction(InputAction.Weapon));
			}
			else if (rewiredPlayer.GetButtonDown("FireOne"))
			{
				Timing.RunCoroutine(_QueueAction(InputAction.FireOne));
			}
			else if (rewiredPlayer.GetButtonDown("FireTwo"))
			{
				Timing.RunCoroutine(_QueueAction(InputAction.FireTwo));
			}
			else if (rewiredPlayer.GetButton("Weapon"))
			{
				if (!actionQueued && mov.state == State.Idle)
				{
					Timing.RunCoroutine(_QueueAction(InputAction.Weapon));
				}
			}
			else if (rewiredPlayer.GetButton("Gameplay_Left"))
			{
				if (CheckHold(Direction.Left))
				{
					Timing.RunCoroutine(_QueueAction(InputAction.Move_Left));
				}
			}
			else if (rewiredPlayer.GetButton("Gameplay_Right"))
			{
				if (CheckHold(Direction.Right))
				{
					Timing.RunCoroutine(_QueueAction(InputAction.Move_Right));
				}
			}
			else if (rewiredPlayer.GetButton("Gameplay_Up"))
			{
				if (CheckHold(Direction.Up))
				{
					Timing.RunCoroutine(_QueueAction(InputAction.Move_Up));
				}
			}
			else if (rewiredPlayer.GetButton("Gameplay_Down"))
			{
				if (CheckHold(Direction.Down))
				{
					Timing.RunCoroutine(_QueueAction(InputAction.Move_Down));
				}
			}
			else
			{
				holdTimer = 0f;
				holding = false;
			}
			if (rewiredPlayer.GetButtonDown("Shuffle"))
			{
				ctrl.camCtrl.Shake(0);
				if (duelDisk.shuffling)
				{
					lastSpellText = CreateFloatText(ctrl.statusTextPrefab, string.Format(ScriptLocalization.UI.Deck_is_shuffling), -20, 65, 0.5f);
				}
				else
				{
					if (duelDisk.manualShuffleDisabled)
					{
						lastSpellText = CreateFloatText(ctrl.statusTextPrefab, string.Format(ScriptLocalization.UI.Manual_shuffle_disabled), -20, 65, 0.5f);
						return;
					}
					duelDisk.ManualShuffle();
				}
			}
			if (rewiredPlayer.GetButtonDown("RemoveSpell"))
			{
				petSnap = !petSnap;
				if (rewiredPlayer.GetButtonDown("Shuffle"))
				{
					petSnap = !petSnap;
					if (HasOverrideAnim("taunt"))
					{
						anim.SetTrigger("taunt");
					}
				}
				else if (petSnap)
				{
					if (currentPets.Count > 0)
					{
						base.transform.position = base.transform.position - Vector3.forward * 1E-05f;
						if (HasOverrideAnim("pet"))
						{
							anim.SetTrigger("pet");
						}
					}
					for (int i = 0; i < currentPets.Count; i++)
					{
						currentPets[i].beingStatsPanel.healthDisplayCanvasGroup.alpha = 0f;
					}
				}
				else if (!petSnap)
				{
					for (int j = 0; j < currentPets.Count; j++)
					{
						currentPets[j].beingStatsPanel.healthDisplayCanvasGroup.alpha = 1f;
					}
				}
			}
			if (petSnap)
			{
				for (int k = 0; k < currentPets.Count; k++)
				{
					if (currentPets[k].mov.currentTile != mov.endTile && currentPets[k].mov.state == State.Idle)
					{
						currentPets[k].mov.MoveToTile(mov.endTile, false, false, false, false);
					}
				}
			}
		}
		basicCooldownTime -= chronoTime;
	}

	private bool CheckHold(Direction newDirection)
	{
		if (ctrl.optCtrl.settingsPane.holdMovementEnabled == 0)
		{
			return false;
		}
		holdTimer += Time.unscaledDeltaTime;
		if (holdDirection != newDirection)
		{
			holdDirection = newDirection;
			holdTimer = 0f;
			holding = false;
		}
		else if ((holdTimer > holdStartDelay || holding) && holdTimer > holdInputDelay)
		{
			holdTimer = 0f;
			holding = true;
			return true;
		}
		return false;
	}

	public override void AddStatus(Status statusType, float amount = 0f, float duration = 0f, ItemObject source = null)
	{
		base.AddStatus(statusType, amount, duration, source);
		deCtrl.statsScreen.UpdateStats(this);
		duelDisk.castSlots[0].StatusUpdateGlow(statusType);
		duelDisk.castSlots[1].StatusUpdateGlow(statusType);
	}

	public void AddControlBlock(Block blockName)
	{
		controlBlocks.Add(blockName);
	}

	public void RemoveControlBlock(Block blockName)
	{
		while (controlBlocks.Contains(blockName))
		{
			controlBlocks.Remove(blockName);
		}
	}

	public void RemoveControlBlockNextFrame(Block blockName)
	{
		StartCoroutine(_RemoveControlBlockNextFrame(blockName));
	}

	private IEnumerator _RemoveControlBlockNextFrame(Block blockName)
	{
		yield return new WaitForEndOfFrame();
		RemoveControlBlock(blockName);
	}

	private bool HasOverrideAnim(string animString)
	{
		if (overrides == null)
		{
			return false;
		}
		foreach (KeyValuePair<AnimationClip, AnimationClip> @override in overrides)
		{
			if (@override.Value != null && @override.Value.name.Contains(animString))
			{
				return true;
			}
		}
		return false;
	}

	public void QueueAction(InputAction action)
	{
		Timing.RunCoroutine(_QueueAction(action));
	}

	private IEnumerator<float> _QueueAction(InputAction action)
	{
		if (mov.state != 0 && mov.state != State.Channeling)
		{
			if (actionQueued)
			{
				if (queuedAction != InputAction.FireOne && queuedAction != InputAction.FireTwo && queuedAction != InputAction.Weapon)
				{
					queuedAction = action;
				}
				yield break;
			}
			queuedAction = action;
			actionQueued = true;
			yield return Timing.WaitUntilTrue(() => mov.state == State.Idle || mov.state == State.Channeling);
			action = queuedAction;
			actionQueued = false;
		}
		int num;
		switch (action)
		{
		case InputAction.Move_Up:
			if (wrap)
			{
				if (battleGrid.TileExists(mov.currentTile.x, mov.currentTile.y + 1))
				{
					Move(0, 1);
				}
				else
				{
					mov.TeleportTo(mov.currentTile.x, 0);
				}
			}
			else
			{
				Move(0, 1);
			}
			yield break;
		case InputAction.Move_Right:
			if (base.transform.rotation.y == 0f)
			{
				Move(1, 0);
			}
			else
			{
				Move(-1, 0);
			}
			yield break;
		case InputAction.Move_Down:
			if (wrap)
			{
				if (battleGrid.TileExists(mov.currentTile.x, mov.currentTile.y - 1))
				{
					Move(0, -1);
				}
				else
				{
					mov.TeleportTo(mov.currentTile.x, battleGrid.gridHeight - 1);
				}
			}
			else
			{
				Move(0, -1);
			}
			yield break;
		case InputAction.Move_Left:
			if (base.transform.rotation.y == 0f)
			{
				Move(-1, 0);
			}
			else
			{
				Move(1, 0);
			}
			yield break;
		case InputAction.Weapon:
			num = ((!PostCtrl.transitioning) ? 1 : 0);
			break;
		default:
			num = 0;
			break;
		}
		if (num != 0)
		{
			CastWeapon();
		}
		else if (action == InputAction.FireOne && !PostCtrl.transitioning)
		{
			CastSpell(0);
		}
		else if (action == InputAction.FireTwo && !PostCtrl.transitioning)
		{
			CastSpell(1);
		}
	}

	public void CastWeapon()
	{
		if (!(basicCooldownTime + equippedWep.cooldown < 0f))
		{
			return;
		}
		int num = CalculateManaCost(equippedWep);
		if (duelDisk.currentMana >= (float)num)
		{
			weaponUsesThisBattle++;
			spellToCast = equippedWep;
			TriggerAllArtifacts(FTrigger.OnWeaponCast);
			manaBeforeSpellCast = duelDisk.currentMana;
			duelDisk.currentMana -= num;
			TriggerArtifacts(FTrigger.OnManaBelow);
			if ((bool)anim)
			{
				anim.SetTrigger("spellCast");
				anim.ResetTrigger("toIdle");
			}
			equippedWep.PlayerCast();
			basicCooldownTime = basicCooldown;
			if (equippedWep.channel)
			{
				anim.SetTrigger("channel");
			}
			spellToCast = null;
		}
		else if (duelDisk.currentMana < (float)num && !S.I.RECORD_MODE && timeSinceLastNeedMoreMana < Time.time - 0.2f)
		{
			float num2 = (float)num - duelDisk.currentMana;
			Mathf.Clamp(num2, 0.1f, num2);
			timeSinceLastNeedMoreMana = Time.time;
			lastSpellText = CreateFloatText(ctrl.statusTextPrefab, string.Format(ScriptLocalization.UI.NeedMoreMana + "({0})", Mathf.Clamp(num2, 0.1f, num2).ToString("f1")), -20, 65, 0.5f);
		}
	}

	public virtual void Move(int x, int y)
	{
		for (int i = 0; i < currentPets.Count; i++)
		{
			if (currentPets[i].mov.state == State.Idle)
			{
				if (i > 0)
				{
					currentPets[i].mov.MoveToTile(currentPets[i - 1].mov.startTile, false, false, false, false);
				}
				else
				{
					currentPets[i].mov.MoveToTile(mov.startTile, false, false, false, false);
				}
			}
		}
		mov.Move(x, y);
	}

	public override void Damage(int amount, bool pierceDefense = false, bool pierceShield = false, bool pierceInvince = false, ItemObject itemObj = null)
	{
		if (ctrl.optCtrl.settingsPane.angelModeEnabled == 1 && !pierceInvince && itemObj != null && (itemObj.being == null || itemObj.being != this))
		{
			amount = Mathf.RoundToInt((float)amount - (float)amount * ctrl.optCtrl.settingsPane.angelModeCurrentDamageReduction);
		}
		base.Damage(amount, pierceDefense, pierceShield, pierceInvince, itemObj);
	}

	private int CalculateManaCost(SpellObject spellObj)
	{
		spellObj.mana = ctrl.GetAmount(spellObj.manaType, spellObj.mana, spellObj);
		spellObj.numShots = ctrl.GetAmount(spellObj.numShotsType, spellObj.numShots, spellObj);
		Mathf.Clamp(spellObj.mana, 0f, spellObj.mana);
		return Mathf.RoundToInt(spellObj.mana);
	}

	protected virtual int GetSlotNum(int theSlotNum)
	{
		if (duelDisk.castSlots[theSlotNum].spellObj == null)
		{
			for (int i = 0; i < duelDisk.castSlots.Count; i++)
			{
				if (duelDisk.castSlots[i].spellObj != null)
				{
					theSlotNum = i;
					break;
				}
			}
		}
		return theSlotNum;
	}

	public void CastSpell(int slotNum, int manaOverride = -1, bool consumeOverride = false)
	{
		if (slotNum >= duelDisk.castSlots.Count)
		{
			return;
		}
		if (lastSpellText != null)
		{
			SimplePool.Despawn(lastSpellText.gameObject);
		}
		if (duelDisk.shuffling)
		{
			lastSpellText = CreateFloatText(ctrl.statusTextPrefab, string.Format(ScriptLocalization.UI.Deck_is_shuffling), -20, 65, 0.5f);
			return;
		}
		slotNum = GetSlotNum(slotNum);
		if (duelDisk.castSlots[slotNum].cardtridgeFill == null)
		{
			lastSpellText = CreateFloatText(ctrl.statusTextPrefab, ScriptLocalization.UI.NoMoreSpells, -20, 65, 0.5f);
			return;
		}
		spellToCast = duelDisk.castSlots[slotNum].cardtridgeFill.spellObj;
		int num = CalculateManaCost(spellToCast);
		if (manaOverride >= 0)
		{
			num = manaOverride;
		}
		if (duelDisk.currentMana >= (float)num)
		{
			spellToCast.being = this;
			spellToCast.beingAnim = anim;
			spellToCast.spell.being = this;
			anim.SetTrigger("spellCast");
			anim.ResetTrigger("toIdle");
			spellToCast.castSlotNum = slotNum;
			audioSource.PlayOneShot(castSound);
			manaBeforeSpellCast = duelDisk.currentMana;
			duelDisk.currentMana -= num;
			TriggerArtifacts(FTrigger.OnManaBelow);
			lastSpellText = CreateSpellText(spellToCast);
			TriggerArtifacts(FTrigger.OnSpellCast);
			TriggerAllArtifacts(FTrigger.OnPlayerSpellCast);
			if (spellToCast.itemID == "Jam")
			{
				TriggerArtifacts(FTrigger.OnJamCast);
			}
			theSpellCast = spellToCast;
			spellToCast.PlayerCast();
			spellsCastThisBattle.Add(spellToCast);
			duelDisk.LaunchSlot(slotNum, consumeOverride, lastSpellText);
			if (spellToCast.backfire)
			{
				for (int i = 0; i < duelDisk.castSlots.Count; i++)
				{
					if (i != slotNum)
					{
						duelDisk.LaunchSlot(i, true);
					}
				}
			}
			if (spellToCast.channel)
			{
				anim.SetTrigger("channel");
			}
			foreach (Cpu currentPet in currentPets)
			{
				currentPet.StartAction();
			}
			if (!AchievementsCtrl.IsUnlocked("Disguised_Toast") && !player.IsReference())
			{
				float amountVar = 0f;
				if (ctrl.GetAmount(new AmountApp(ref amountVar, "JamsCastThisBattle"), 0f, spellToCast) >= 10f)
				{
					AchievementsCtrl.UnlockAchievement("Disguised_Toast");
				}
			}
			spellToCast = null;
		}
		else if (duelDisk.currentMana < (float)num)
		{
			float num2 = (float)num - duelDisk.currentMana;
			Mathf.Clamp(num2, 0.1f, num2);
			if (num > maxMana)
			{
				lastSpellText = CreateFloatText(ctrl.statusTextPrefab, ScriptLocalization.UI.Max_Mana_too_low, -20, 65, 0.5f);
				duelDisk.currentMana -= 1f;
				duelDisk.LaunchSlot(slotNum, consumeOverride, lastSpellText);
			}
			else
			{
				lastSpellText = CreateFloatText(ctrl.statusTextPrefab, string.Format(ScriptLocalization.UI.NeedMoreMana + "({0})", Mathf.Clamp(num2, 0.1f, num2).ToString("f1")), -20, 65, 0.5f);
			}
		}
	}

	public override void OnHit(Projectile attackRef)
	{
		base.OnHit(attackRef);
		if ((bool)attackRef && (bool)attackRef.being && attackRef.being != this && invinceTime <= Time.time)
		{
			ctrl.perfectBattle = false;
		}
		ctrl.camCtrl.Shake(1);
		TriggerAllArtifacts(FTrigger.OnPlayerHit, this);
	}

	public override void AfterHit(Projectile attackRef)
	{
		base.AfterHit(attackRef);
		TriggerAllArtifacts(FTrigger.AfterPlayerHit, this);
	}

	public override void ApplyStun(bool showText = true, bool endIdle = false, bool stopDashing = true)
	{
		actionQueued = false;
		base.ApplyStun(showText, endIdle, stopDashing);
	}

	public override void RemoveStatusEffect(StatusEffect se)
	{
		base.RemoveStatusEffect(se);
		duelDisk.castSlots[0].StatusUpdateGlow(se.status);
		duelDisk.castSlots[1].StatusUpdateGlow(se.status);
	}

	public override void StartDeath(bool triggerDeathrattles = true)
	{
		StopSelfAndChildCoroutines();
		string text = "";
		if (sp.bossesToSpawn.Count > 0)
		{
			text = sp.bossesToSpawn[0];
		}
		if (downed)
		{
			Timing.RunCoroutine(DeathSequence());
			return;
		}
		if (spellAppObj != null)
		{
			spellAppObj.Trigger(FTrigger.OnDeath);
		}
		if (deathrattles.Count > 0 && triggerDeathrattles)
		{
			Timing.RunCoroutine(DeathrattlesC());
		}
		else if (battleGrid.currentEnemies.Count > 0 && (bool)battleGrid.currentEnemies[0].GetComponent<Boss>() && !battleGrid.currentEnemies[0].GetComponent<Boss>().downed)
		{
			StartDownC();
		}
		else
		{
			Timing.RunCoroutine(DeathSequence());
		}
	}

	protected virtual void StartDownC()
	{
		Timing.RunCoroutine(DownC());
	}

	protected IEnumerator<float> DownC()
	{
		AddControlBlock(Block.Downed);
		anim.ResetTrigger("undown");
		base.dontInterruptAnim = true;
		dontMoveAnim = true;
		col.enabled = false;
		anim.SetBool("dashing", false);
		AddInvince(1.4f);
		downed = true;
		anim.SetTrigger("down");
		ctrl.camCtrl.Shake(3);
		sprite = spriteRend.sprite;
		health.current = 1;
		if (lastSpellHit != null)
		{
			lastSpellHit.Trigger(FTrigger.Execute);
		}
		RemoveAllStatuses();
		mov.MoveTo(mov.currentTile.x, mov.currentTile.y, false, true, false, false, true, true, true);
		mov.currentTile.Extinguish();
		yield return Timing.WaitForSeconds(0.6f);
		RemoveAllStatuses();
		mov.currentTile.Extinguish();
		base.dontInterruptAnim = false;
		health.deathTriggered = false;
		col.enabled = true;
		if (battleGrid.currentEnemies.Count > 0 && (bool)battleGrid.currentEnemies[0].GetComponent<Boss>())
		{
			CallBossExecute();
		}
		else
		{
			StartCoroutine(DeathSequence());
		}
	}

	protected virtual void CallBossExecute()
	{
		battleGrid.currentEnemies[0].GetComponent<Boss>().ExecutePlayer();
	}

	protected IEnumerator<float> DeathSequence()
	{
		base.dontInterruptAnim = true;
		anim.SetBool("dashing", false);
		if (!ctrl.pvpMode && battleGrid != ctrl.ti.refBattleGrid)
		{
			S.I.muCtrl.PauseIntroLoop();
			if (ctrl.shopCtrl.slideBody.onScreen)
			{
				ctrl.shopCtrl.Close();
			}
		}
		mov.MoveTo(mov.currentTile.x, mov.currentTile.y, false, true, false, false, false, true, true);
		anim.SetTrigger("die");
		if (beingObj.animName == "Reva")
		{
			CreateHitFX("BossReva", true);
		}
		AddControlBlock(Block.Dead);
		if (ctrl.pvpMode)
		{
			S.I.PlayOnce(ctrl.pvpDeathSound, IsReference());
			BC.GTimeScale = 0.1f;
			yield return Timing.WaitForSeconds(0.15f);
		}
		else
		{
			BC.GTimeScale = 0.2f;
			yield return Timing.WaitForSeconds(0.2f);
		}
		BC.GTimeScale = 1f;
		if ((bool)col)
		{
			col.enabled = false;
		}
		base.dontInterruptAnim = false;
		TriggerArtifacts(FTrigger.OnDeath, this);
		TriggerAllArtifacts(FTrigger.OnPlayerDeath, this);
		ClearQueuedActions();
		yield return float.NegativeInfinity;
		if (health.current > 0)
		{
			anim.SetTrigger("revive");
			deathrattlesTriggered = false;
			CreateHitFX(Status.Resurrect);
			PlayOnce("misc_shimmer");
			yield return Timing.WaitForSeconds(0.3f);
			ClearQueuedActions();
			PlayOnce("light_spell");
			RemoveControlBlock(Block.Dead);
			RemoveAllStatuses();
			Undown();
			health.deathTriggered = false;
			mov.SetState(State.Idle);
			col.enabled = true;
			SetInvince(1f);
			S.I.muCtrl.ResumeIntroLoop();
		}
		else
		{
			if ((bool)lastHitBy)
			{
				ctrl.gameOverPane.killedByName = lastHitBy.beingObj.localizedName;
			}
			base.dontInterruptAnim = true;
			if (deathrattles.Count > 0)
			{
				_003C_003En__0(true);
			}
			else if ((bool)player && (bool)player.gameObject)
			{
				Timing.RunCoroutine(_DeathFinal().CancelWith(base.gameObject));
			}
			base.dontInterruptAnim = false;
			deCtrl.statsScreen.UpdateStats(this);
		}
	}

	public override void RemoveAllStatuses()
	{
		base.RemoveAllStatuses();
		deCtrl.statsScreen.UpdateStats(this);
	}

	public override void MustBeInBattleWarning()
	{
		CreateStatusText(ScriptLocalization.UI.Must_be_in_battle);
	}

	public override IEnumerator<float> _DeathFinal()
	{
		inDeathSequence = true;
		sp.explosionGen.CreateExplosionString(1, base.transform.position, base.transform.rotation, this);
		yield return float.NegativeInfinity;
		battleGrid.currentAllies.Remove(this);
		battleGrid.currentBeings.Remove(this);
		duelDisk.ShowCardRefGrid(false);
		Remove();
		if (ctrl.currentPlayers.Contains(this))
		{
			ctrl.currentPlayers.Remove(this);
		}
		yield return Timing.WaitForSeconds(0.2f);
		inDeathSequence = false;
		if (ctrl.pvpMode)
		{
			ctrl.EndPvPRound(this);
		}
		else if (!IsReference() && ctrl.GameState != GState.MainMenu)
		{
			ctrl.Defeat();
		}
		ctrl.IncrementStat("TotalDamageDealt", damageDealtThisBattle);
		ctrl.IncrementStat("TotalStepsTaken", stepsTakenThisBattle);
		currentPets.Clear();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public IEnumerator TestLoop()
	{
		yield return new WaitUntil(() => mov.state == State.Idle);
		Move(1, 0);
		while (true)
		{
			yield return new WaitUntil(() => mov.state == State.Idle);
			yield return new WaitForSeconds(0.2f);
			if (duelDisk.queuedCardtridges.Count < 1)
			{
				if (duelDisk.shuffling)
				{
					lastSpellText = CreateFloatText(ctrl.statusTextPrefab, string.Format(ScriptLocalization.UI.Deck_is_already_shuffling), -20, 65, 0.5f);
				}
				else
				{
					duelDisk.ManualShuffle();
				}
			}
			else
			{
				duelDisk.currentMana = 100f;
				UnityEngine.Debug.Log(duelDisk.queuedCardtridges[0].spellObj.itemID);
				CastSpell(0);
			}
			yield return new WaitForSeconds(0.2f);
		}
	}

	protected override void TriggerWhileBattle()
	{
		if (!player.downed)
		{
			TriggerArtifacts(FTrigger.WhileBattle);
		}
	}

	public void ClearQueuedActions()
	{
		actionQueued = false;
	}

	protected override bool ActivateSavior(int damage)
	{
		if (battleGrid == ctrl.ti.refBattleGrid)
		{
			return false;
		}
		if ((bool)TileLocal(1))
		{
			TileLocal(1).Fix();
		}
		if (health.current + health.shield <= damage && runCtrl.currentRun.HasAssist("BossReva") && runCtrl.currentRun.assists["BossReva"] && TileLocal(1).IsOccupiable())
		{
			if (runCtrl.currentZoneDot.type != ZoneType.Boss || downed)
			{
				RevaAssist(damage);
				return true;
			}
		}
		else if (health.current + health.shield <= damage && runCtrl.currentRun.pacifist && battleGrid.currentEnemies.Count > 0 && (bool)battleGrid.currentEnemies[0].GetComponent<BossTerraDark>() && battleGrid.currentEnemies[0].GetComponent<BossTerraDark>().lastScene && runCtrl.currentRun.HasAssist("BossSaffron") && TileLocal(1).IsOccupiable() && beingObj != null && beingObj.nameString == "Reva")
		{
			if (runCtrl.currentZoneDot.type != ZoneType.Boss || downed)
			{
				SaffronAssist(damage);
				return true;
			}
		}
		else if (health.current + health.shield <= damage && runCtrl.currentRun.pacifist && battleGrid.currentEnemies.Count > 0 && (bool)battleGrid.currentEnemies[0].GetComponent<BossTerraDark>() && battleGrid.currentEnemies[0].GetComponent<BossTerraDark>().lastScene && runCtrl.currentRun.HasAssist("BossReva") && TileLocal(1).IsOccupiable() && (runCtrl.currentZoneDot.type != ZoneType.Boss || downed))
		{
			RevaAssist(damage, true);
			return true;
		}
		return false;
	}

	public virtual void Undown()
	{
		RemoveControlBlock(Block.Downed);
		RemoveControlBlock(Block.Fake);
		if (((bool)anim && anim.HasState(0, 0) && anim.GetCurrentAnimatorStateInfo(0).IsName("down")) || anim.GetCurrentAnimatorStateInfo(0).IsName("downed"))
		{
			anim.SetTrigger("undown");
		}
		dontMoveAnim = false;
		downed = false;
		mov.SetState(State.Idle);
	}

	private void RevaAssist(int damage, bool final = false)
	{
		runCtrl.currentRun.assists["BossReva"] = false;
		anim.SetTrigger("flinch");
		S.I.Flash();
		string beingID = "RevaSaveAssist";
		if (runCtrl.currentRun.pacifist && (bool)battleGrid.currentEnemies[0].GetComponent<BossTerraDark>() && battleGrid.currentEnemies[0].GetComponent<BossTerraDark>().lastScene)
		{
			beingID = "RevaSaveAssistF";
		}
		Being being = ctrl.sp.PlaceBeing(beingID, battleGrid.grid[mov.currentTile.x + 1, mov.currentTile.y]);
		being.transform.rotation = base.transform.rotation;
		PlayOnce("bell_holy");
		being.anim.SetTrigger("assist");
		being.transform.position = being.transform.position - Vector3.forward * 1E-05f;
		if (!final)
		{
			being.spriteRend.sortingOrder = 1;
		}
		being.HitAmount(damage, false, true);
		if (undownForAssist)
		{
			Undown();
		}
		mov.currentTile.Fix();
		AddInvince(3.5f);
	}

	private void SaffronAssist(int damage)
	{
		runCtrl.currentRun.assists["BossSaffron"] = false;
		anim.SetTrigger("flinch");
		S.I.Flash();
		Being being = ctrl.sp.PlaceBeing("SaffronSaveAssistF", battleGrid.grid[mov.currentTile.x + 1, mov.currentTile.y]);
		being.transform.rotation = base.transform.rotation;
		PlayOnce("bell_holy");
		being.anim.SetTrigger("flinch");
		being.HitAmount(damage, false, true);
		if (undownForAssist)
		{
			Undown();
		}
		mov.currentTile.Fix();
		AddInvince(3.5f);
	}

	public void BattleStartAssist()
	{
	}

	public void CheckArts()
	{
		if (artObjs.Count <= 0)
		{
			return;
		}
		if (wrapObj != null && !artObjs.Contains(wrapObj.artObj))
		{
			wrap = false;
			wrapObj = null;
		}
		if (manualShuffleDisabledObj != null)
		{
			if (!artObjs.Contains(manualShuffleDisabledObj.artObj))
			{
				duelDisk.manualShuffleDisabled = false;
				manualShuffleDisabledObj = null;
			}
		}
		else
		{
			duelDisk.manualShuffleDisabled = false;
		}
	}

	public void AddWrap(ItemObject itemObj)
	{
		wrap = true;
		wrapObj = itemObj;
	}

	public void DisableManualShuffle(ItemObject itemObj)
	{
		duelDisk.manualShuffleDisabled = true;
		manualShuffleDisabledObj = itemObj;
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0(bool triggerDeathrattles)
	{
		base.StartDeath(triggerDeathrattles);
	}
}
