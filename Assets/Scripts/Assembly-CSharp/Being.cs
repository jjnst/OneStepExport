using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MEC;
using MoonSharp.Interpreter;
using Sirenix.OdinInspector;
using UnityEngine;

[MoonSharpUserData]
public class Being : MonoBehaviour
{
	public Sprite sprite;

	[HideInInspector]
	public BC ctrl;

	[HideInInspector]
	public DeckCtrl deCtrl;

	[HideInInspector]
	public RunCtrl runCtrl;

	[HideInInspector]
	public SpawnCtrl sp;

	public int alignNum = 1;

	public Transform gunPoint;

	public GameObject shadow;

	[HideInInspector]
	public AudioSource audioSource;

	public List<StatusEffect> statusEffectList = new List<StatusEffect>();

	[HideInInspector]
	public BeingStatsPanel beingStatsPanel;

	public List<StatusDisplay> statusDisplays = new List<StatusDisplay>();

	[HideInInspector]
	public Moveable mov;

	[HideInInspector]
	public Health health;

	[HideInInspector]
	public Animator anim;

	[HideInInspector]
	public Shader hitShader;

	public Shader defaultShader;

	[HideInInspector]
	public float flashLength = 0.001f;

	[HideInInspector]
	public float flashTime;

	[HideInInspector]
	public SpriteRenderer spriteRend;

	public BoxCollider2D col;

	public List<Projectile> activeProjectiles = new List<Projectile>();

	[HideInInspector]
	public int incomingDamage = 0;

	public AudioClip hitSoundTiny;

	public AudioClip hitSoundLight;

	public AudioClip hitSound;

	public AudioClip hitSoundHeavy;

	public AudioClip hitSoundShieldTiny;

	public AudioClip hitSoundShield;

	public AudioClip hitSoundShieldBreak;

	public AudioClip dieSound;

	public AudioClip explosionSound;

	public SpellObject lastSpellHit;

	public SpellObject theSpellCast;

	public TalkBox talkBubble;

	public Being lastHitBy;

	public Being lastHitByOther;

	public Being lastTargeted;

	public int lastDamageAmount;

	public int lastTrueDamageAmount;

	public bool cleared = false;

	public bool dead = false;

	public float invinceTime = 0f;

	private bool _dontInterruptAnim;

	public bool dontHitAnim = false;

	public bool dontMoveAnim = false;

	public bool dontInterruptChannelAnim = false;

	public bool hitAnimationActive = false;

	[ShowInInspector]
	public List<ArtifactObject> artObjs = new List<ArtifactObject>();

	[ShowInInspector]
	public List<ArtifactObject> buffs = new List<ArtifactObject>();

	[ShowInInspector]
	public List<PactObject> pactObjs = new List<PactObject>();

	[ShowInInspector]
	public List<SpellObject> spellObjList = new List<SpellObject>();

	[ShowInInspector]
	public List<SpellObject> startups = new List<SpellObject>();

	[ShowInInspector]
	public List<SpellObject> timeouts = new List<SpellObject>();

	[ShowInInspector]
	public List<SpellObject> deathrattles = new List<SpellObject>();

	[ShowInInspector]
	public List<SpellObject> clearSpells = new List<SpellObject>();

	[ShowInInspector]
	public List<SpellObject> currentSpellObjs = new List<SpellObject>();

	[ShowInInspector]
	public SpellObject spellAppObj;

	public List<Cpu> currentPets = new List<Cpu>();

	public List<GameObject> legacyDeckList = new List<GameObject>();

	[ReadOnly]
	public List<Spell> legacySpellList = new List<Spell>();

	public bool inDeathSequence = false;

	public bool minion = false;

	public ItemObject parentObj;

	private UnityEngine.Coroutine co_HitAnimReset;

	public List<Tile> savedTileList = new List<Tile>();

	[ShowInInspector]
	public BeingObject beingObj;

	public Player player;

	public int healthBeforeHit = 0;

	public bool deathrattlesTriggered = false;

	public Color invinceColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 160);

	public bool shieldDefense = false;

	public int damageDealtThisBattle = 0;

	public int stepsTakenThisBattle = 0;

	public bool invinceFlash = true;

	public float gracePeriodDuration = 0f;

	public float lastGracePeriod = 0f;

	private bool removeThisStatus;

	private List<StatusEffect> seRemovalQueue = new List<StatusEffect>();

	public List<int> orbs = new List<int>();

	private AnimatorControllerParameter[] animParams;

	public List<LineRenderer> lineRends = new List<LineRenderer>();

	public BattleGrid battleGrid;

	public bool pet = false;

	public float baseLoopDelay = 0f;

	public SpriteAnimator sprAnim;

	public AnimationOverrider animOverrider;

	public Dictionary<Status, int> statusesRemovedThisFramePos = new Dictionary<Status, int>();

	public bool dontInterruptAnim
	{
		get
		{
			return _dontInterruptAnim;
		}
		set
		{
			_dontInterruptAnim = value;
		}
	}

	public void Awake()
	{
		ctrl = S.I.batCtrl;
		deCtrl = S.I.deCtrl;
		runCtrl = S.I.runCtrl;
		sp = S.I.spCtrl;
	}

	public void SetAlignNum(int newAlignNum)
	{
		alignNum = newAlignNum;
		mov.alignNum = newAlignNum;
	}


	public virtual void Setup()
	{
		mov = GetComponent<Moveable>();
		health = GetComponent<Health>();
		anim = GetComponent<Animator>();
		col = GetComponent<BoxCollider2D>();
		spriteRend = GetComponent<SpriteRenderer>();
		sprite = spriteRend.sprite;
		audioSource = base.gameObject.AddComponent<AudioSource>();
		audioSource.outputAudioMixerGroup = S.sfxGroup;
		defaultShader = spriteRend.sharedMaterial.shader;
		flashLength = sp.hitFlashLength;
		animParams = anim.parameters;
	}

	public virtual void Start()
	{
		explosionSound = S.I.explosionSound;
	}

	public virtual void Activate()
	{
		mov.state = State.Idle;
		Startup();
	}

	protected virtual void Update()
	{
		if (base.gameObject == null)
		{
			return;
		}
		if (flashTime > 0f)
		{
			flashTime -= Time.unscaledDeltaTime;
		}
		else if (spriteRend.sharedMaterial.shader != defaultShader)
		{
			spriteRend.sharedMaterial.shader = defaultShader;
		}
		if (invinceTime >= Time.time && invinceFlash)
		{
			if (spriteRend.color != invinceColor)
			{
				spriteRend.color = invinceColor;
			}
			else
			{
				spriteRend.color = Color.white;
			}
		}
		else if (spriteRend.color != Color.white)
		{
			spriteRend.color = Color.white;
		}
		if (mov.state == State.Idle)
		{
			anim.SetTrigger("toIdle");
		}
		CalculateStatus();
		TriggerArtifacts(FTrigger.While);
		if (battleGrid.InMidBattle())
		{
			TriggerWhileBattle();
			TriggerArtifacts(FTrigger.WhileIdle);
			TriggerArtifacts(FTrigger.WhileHPBelow);
			TriggerArtifacts(FTrigger.WhileShieldBelow);
		}
	}

	protected virtual void TriggerWhileBattle()
	{
		TriggerArtifacts(FTrigger.WhileBattle);
	}

	public virtual void SetAnimatorController(RuntimeAnimatorController newAnimatorController)
	{
		anim.runtimeAnimatorController = newAnimatorController;
	}

	public bool IsReference()
	{
		return ctrl.ti.refBattleGrid != null && battleGrid == ctrl.ti.refBattleGrid;
	}

	public void AddTimerBar(float closeTime = 1f)
	{
		Timerbar timerbar = UnityEngine.Object.Instantiate(ctrl.timerBarPrefab, beingStatsPanel.transform.position + Vector3.up * 1.5f, base.transform.rotation, beingStatsPanel.transform);
		timerbar.Set(this, beingObj.localTimerPos, closeTime);
	}

	public void AddStatus(string statusTypeString, float amount = 0f, float duration = 0f)
	{
		if (Enum.IsDefined(typeof(Status), statusTypeString))
		{
			AddStatus((Status)Enum.Parse(typeof(Status), statusTypeString), amount, duration);
		}
	}

	public virtual void AddStatus(Status statusType, float amount = 0f, float duration = 0f, ItemObject source = null)
	{
		bool flag = false;
		int num = statusEffectList.Count - 1;
		while (num >= 0 && statusEffectList.Count > num)
		{
			StatusEffect statusEffect = statusEffectList[num];
			if (statusType == statusEffect.status)
			{
				flag = true;
				string plusser = " +";
				if (amount < 0f)
				{
					plusser = " ";
				}
				switch (statusType)
				{
				case Status.AtkDmg:
					SetStatusStack(statusEffect, amount, duration, plusser, true, source);
					break;
				case Status.Chrono:
					if (battleGrid == ctrl.ti.mainBattleGrid)
					{
						statusEffect.maxDuration = duration * amount;
						statusEffect.duration = duration * amount;
						Time.timeScale = amount;
						statusEffect.amount = amount;
						statusEffect.SetText(statusEffect.amount);
						SettingsPane.masterMixer.SetFloat("musicPitch", Mathf.Clamp(amount, 0.1f, 1f));
						SettingsPane.masterMixer.SetFloat("sfxPitch", Mathf.Clamp(amount, 0.1f, 1f));
					}
					else
					{
						statusEffect.maxDuration = duration;
						statusEffect.duration = duration;
					}
					break;
				case Status.Defense:
					SetStatusStack(statusEffect, amount, duration, plusser, true, source);
					break;
				case Status.Flow:
					statusEffect.duration = 9999f;
					statusEffect.maxDuration = duration;
					if (amount < 1f)
					{
						amount = 1f;
					}
					statusEffect.amount += amount;
					break;
				case Status.Fragile:
					statusEffect.maxDuration = duration;
					statusEffect.duration = 9999f;
					if (amount < 1f)
					{
						amount = 1f;
					}
					statusEffect.amount += amount;
					break;
				case Status.Frost:
					statusEffect.duration = BC.frostLength;
					statusEffect.maxDuration = statusEffect.duration;
					if (amount < 1f)
					{
						amount = 1f;
					}
					CreateHitFX(statusType);
					statusEffect.amount += amount;
					if ((bool)statusEffect.display.anim)
					{
						statusEffect.display.anim.SetInteger("frost", Mathf.RoundToInt(statusEffect.amount));
					}
					if (statusEffect.amount >= 3f)
					{
						CreateHitFX(statusType, true);
						Damage(BC.frostDmg, true, true);
						PlayOnce(ctrl.frostSound);
						statusEffect.SetText(statusEffect.amount);
						statusEffect.amount -= 3f;
						if (statusEffect.amount < 1f)
						{
							statusEffect.duration = 0.5f;
						}
						else
						{
							statusEffect.display = CreateStatusDisplay(statusType);
						}
						TriggerAllArtifacts(FTrigger.OnFrost, this);
					}
					break;
				case Status.Haste:
					statusEffect.duration = duration;
					break;
				case Status.Link:
					statusEffect.maxDuration = duration;
					statusEffect.duration = duration;
					break;
				case Status.ManaRegen:
					SetStatusStack(statusEffect, amount, duration, plusser, true, source);
					break;
				case Status.MaxMana:
					SetStatusStack(statusEffect, amount, duration, plusser, true, source);
					break;
				case Status.Poison:
					statusEffect.maxDuration = BC.poisonDuration;
					statusEffect.duration = BC.poisonDuration;
					statusEffect.amount += amount;
					CreateHitFX(statusType);
					break;
				case Status.Reflect:
					statusEffect.maxDuration = duration;
					statusEffect.duration = duration;
					statusEffect.amount = amount;
					if ((bool)statusEffect.display.anim)
					{
						statusEffect.display.anim.SetTrigger("spawn");
					}
					break;
				case Status.Root:
					statusEffect.duration = duration;
					statusEffect.amount = 0f;
					CreateStatusText(LocalizationManager.GetTranslation("MechKeys/" + statusEffect.status) + "!");
					break;
				case Status.Slow:
					statusEffect.duration = duration;
					statusEffect.amountText.text = string.Empty;
					break;
				case Status.SpellPower:
					SetStatusStack(statusEffect, amount, duration, plusser, true, source);
					break;
				case Status.Shield:
					statusEffect.maxDuration = duration;
					statusEffect.duration = 9999f;
					if ((amount > 5f || health.shield == 0) && (bool)statusEffect.display.anim)
					{
						statusEffect.display.anim.SetTrigger("spawn");
					}
					health.ModifyShield(Mathf.RoundToInt(amount));
					amount = 0f;
					break;
				case Status.ShieldExte:
					statusEffect.maxDuration = duration;
					statusEffect.duration = 9999f;
					AddStatus(Status.Shield, statusEffect.amount);
					break;
				case Status.Trinity:
					statusEffect.maxDuration = duration;
					statusEffect.duration = 9999f;
					if (amount < 1f)
					{
						amount = 1f;
					}
					statusEffect.amount += amount;
					if (statusEffect.amount >= 4f)
					{
						statusEffect.amount = 3f;
					}
					if ((bool)statusEffect.display.anim)
					{
						statusEffect.display.anim.SetInteger("trinity", Mathf.RoundToInt(statusEffect.amount));
					}
					break;
				case Status.Stun:
					if (statusEffect.duration < duration)
					{
						statusEffect.duration = duration;
						statusEffect.maxDuration = statusEffect.duration;
					}
					foreach (Transform item in base.transform)
					{
						if ((bool)item.GetComponent<Spell>())
						{
							item.GetComponent<Spell>().StopAllCoroutines();
						}
					}
					CreateStatusText(LocalizationManager.GetTranslation("MechKeys/" + statusEffect.status) + "!");
					break;
				}
				statusEffect.amount = Mathf.Clamp(statusEffect.amount, -999999f, 999999f);
				if (statusEffect.amount > 0f)
				{
					statusEffect.SetText(statusEffect.amount);
				}
				if ((bool)statusEffect.icon && !statusEffect.icon.enabled)
				{
					statusEffect.icon.enabled = true;
					statusEffect.iconBackground.enabled = true;
				}
			}
			num--;
		}
		if (!flag)
		{
			StatusDisplay statusDisplay = null;
			string text = " +";
			if (amount < 0f)
			{
				text = " ";
			}
			switch (statusType)
			{
			case Status.Stun:
				ApplyStun();
				break;
			case Status.AtkDmg:
				if (duration >= 1f)
				{
					CreateStatusText(LocalizationManager.GetTranslation("MechKeys/" + statusType) + text + amount);
				}
				amount = Mathf.FloorToInt(amount);
				break;
			case Status.Chrono:
				if (duration > 1f)
				{
					CreateHitFX(statusType);
				}
				if (battleGrid == ctrl.ti.mainBattleGrid)
				{
					Time.timeScale = amount;
					duration *= amount;
					SettingsPane.masterMixer.SetFloat("musicPitch", Mathf.Clamp(amount, 0.1f, 1f));
					SettingsPane.masterMixer.SetFloat("sfxPitch", Mathf.Clamp(amount, 0.1f, 1f));
				}
				break;
			case Status.Defense:
				if (duration >= 1f)
				{
					CreateStatusText(LocalizationManager.GetTranslation("MechKeys/" + statusType) + text + amount);
				}
				break;
			case Status.Flow:
				duration = 9999f;
				if (amount < 1f)
				{
					amount = 1f;
				}
				break;
			case Status.ManaRegen:
				if (amount == 0f)
				{
					return;
				}
				break;
			case Status.MaxMana:
				if (amount == 0f)
				{
					return;
				}
				break;
			case Status.Poison:
				if (amount == 0f)
				{
					return;
				}
				duration = BC.poisonDuration;
				CreateHitFX(statusType);
				break;
			case Status.SpellPower:
				if (duration >= 1f)
				{
					CreateStatusText(LocalizationManager.GetTranslation("MechKeys/" + statusType) + text + amount);
				}
				amount = Mathf.FloorToInt(amount);
				break;
			case Status.Shield:
				if (amount < 0f)
				{
					return;
				}
				duration = 9999f;
				health.ModifyShield(Mathf.RoundToInt(amount));
				amount = 0f;
				statusDisplay = CreateStatusDisplay(statusType);
				break;
			case Status.ShieldExte:
				duration = 9999f;
				AddStatus(Status.Shield, amount);
				amount = 0f;
				col.size = new Vector2(30f, 50f);
				statusDisplay = CreateStatusDisplay(statusType);
				break;
			case Status.Link:
				statusDisplay = CreateStatusDisplay(statusType);
				break;
			case Status.Fragile:
				duration = 9999f;
				if (amount < 1f)
				{
					amount = 1f;
				}
				statusDisplay = CreateStatusDisplay(statusType);
				break;
			case Status.Frost:
				duration = BC.frostLength;
				CreateHitFX(statusType);
				statusDisplay = CreateStatusDisplay(statusType);
				if ((bool)statusDisplay.anim)
				{
					statusDisplay.anim.SetInteger("frost", Mathf.RoundToInt(amount));
				}
				if (amount < 1f)
				{
					amount = 1f;
				}
				if (amount >= 3f)
				{
					CreateHitFX(statusType, true);
					Damage(BC.frostDmg, true, true);
					PlayOnce(ctrl.frostSound);
					amount -= 3f;
					if (amount < 1f)
					{
						duration = 0.5f;
					}
					TriggerAllArtifacts(FTrigger.OnFrost, this);
				}
				break;
			case Status.Haste:
				mov.lerpTime -= BC.hasteAmount;
				break;
			case Status.Reflect:
				statusDisplay = CreateStatusDisplay(statusType);
				statusDisplay.spriteRend.sortingOrder = 1;
				break;
			case Status.Root:
				CreateStatusText(LocalizationManager.GetTranslation("MechKeys/" + statusType) + "!");
				statusDisplay = CreateStatusDisplay(statusType);
				break;
			case Status.Slow:
				mov.lerpTime += BC.slowAmount;
				break;
			case Status.Trinity:
				duration = 9999f;
				Mathf.Clamp(amount, 1f, 4f);
				statusDisplay = CreateStatusDisplay(statusType);
				if ((bool)statusDisplay.anim)
				{
					statusDisplay.anim.SetInteger("trinity", Mathf.RoundToInt(amount));
				}
				if (amount < 1f)
				{
					amount = 1f;
				}
				break;
			}
			statusEffectList.Add(SimplePool.Spawn(ctrl.statusEffectPrefab, base.transform.position, ctrl.transform.rotation).GetComponent<StatusEffect>().Set(this, statusType, duration, amount, statusDisplay, source));
		}
		switch (statusType)
		{
		case Status.Poison:
			TriggerAllArtifacts(FTrigger.OnPoison, this);
			break;
		case Status.Frost:
			TriggerAllArtifacts(FTrigger.OnFrostStack, this);
			break;
		}
	}

	private void SetStatusStack(StatusEffect se, float amount, float duration, string plusser, bool durationMinimum, ItemObject source)
	{
		se.statusStacks.Add(new StatusStack(amount, duration, duration, source));
		if (se.duration < duration)
		{
			se.maxDuration = duration;
			se.duration = duration;
		}
		se.amount += amount;
		if (durationMinimum && duration >= 1f)
		{
			CreateStatusText(LocalizationManager.GetTranslation("MechKeys/" + se.status) + plusser + amount);
		}
		else if (!durationMinimum)
		{
			CreateStatusText(LocalizationManager.GetTranslation("MechKeys/" + se.status) + plusser + amount);
		}
		se.SetText(se.amount);
		deCtrl.statsScreen.UpdateStats(player);
		if (se.amount == 0f)
		{
			se.duration = 0f;
		}
	}

	public StatusDisplay CreateStatusDisplay(Status status)
	{
		StatusDisplay component = SimplePool.Spawn(ctrl.statusDisplayPrefab, base.transform.position, ctrl.transform.rotation).GetComponent<StatusDisplay>();
		component.Set(this, status);
		return component;
	}

	public void RemoveStatus(Status statusToRemove)
	{
		for (int num = statusEffectList.Count - 1; num >= 0; num--)
		{
			if (statusEffectList[num].status == statusToRemove)
			{
				RemoveStatusEffect(statusEffectList[num]);
			}
		}
	}

	public void RemoveStatus(string seString)
	{
		if (Enum.IsDefined(typeof(Status), seString))
		{
			RemoveStatus((Status)Enum.Parse(typeof(Status), seString));
		}
	}

	public virtual void RemoveAllStatuses()
	{
		for (int num = statusEffectList.Count - 1; num >= 0; num--)
		{
			if (statusEffectList[num].status == Status.Haste)
			{
				mov.lerpTime += BC.hasteAmount;
			}
			else if (statusEffectList[num].status == Status.Slow)
			{
				mov.lerpTime -= BC.slowAmount;
			}
			RemoveStatusEffect(statusEffectList[num]);
		}
		health.SetShield(0);
		foreach (Cpu currentPet in currentPets)
		{
			currentPet.RemoveAllStatuses();
		}
	}

	public virtual void RemoveStatusEffect(StatusEffect se)
	{
		statusesRemovedThisFramePos[se.status] = se.transform.GetSiblingIndex();
		StartCoroutine(RemoveStatusRemoveNextFrame(se.status));
		if ((bool)se.display)
		{
			SimplePool.Despawn(se.display.gameObject);
		}
		if (se.status == Status.Chrono)
		{
			Time.timeScale = 1f;
			SettingsPane.masterMixer.SetFloat("musicPitch", 1f);
			SettingsPane.masterMixer.SetFloat("sfxPitch", 1f);
		}
		statusEffectList.Remove(se);
		if (!beingStatsPanel)
		{
			return;
		}
		foreach (Transform item in beingStatsPanel.statusEffectsBox)
		{
			if (item.GetComponent<StatusEffect>().status == se.status)
			{
				SimplePool.Despawn(item.gameObject);
			}
		}
	}

	public void RemoveStatusFromItem(ItemObject itemObj, string statusName)
	{
		Status status = (Status)Enum.Parse(typeof(Status), statusName);
		StatusEffect statusEffect = GetStatusEffect(status);
		if (!statusEffect)
		{
			return;
		}
		foreach (StatusStack statusStack in statusEffect.statusStacks)
		{
			if (statusStack.source == itemObj)
			{
				statusStack.duration = 0f;
				if (statusEffect.statusStacks.Count <= 1)
				{
					statusEffect.duration = 0f;
				}
			}
		}
	}

	private IEnumerator RemoveStatusRemoveNextFrame(Status statusToRemove)
	{
		yield return null;
		statusesRemovedThisFramePos.Remove(statusToRemove);
	}

	public void CalculateStatus()
	{
		for (int num = statusEffectList.Count - 1; num >= 0; num--)
		{
			StatusEffect statusEffect = statusEffectList[num];
			removeThisStatus = true;
			if (statusEffect == null)
			{
				return;
			}
			if (statusEffect.duration >= 0f)
			{
				if (statusEffect.creationFrame < Time.frameCount)
				{
					statusEffect.duration -= Time.deltaTime;
				}
				switch (statusEffect.status)
				{
				case Status.AtkDmg:
					CalculateStatusStack(statusEffect);
					break;
				case Status.Defense:
					CalculateStatusStack(statusEffect);
					break;
				case Status.Flow:
					if (statusEffect.amount < 1f)
					{
						statusEffect.duration = -1f;
					}
					statusEffect.SetText(statusEffect.amount);
					break;
				case Status.Fragile:
					statusEffect.SetText(statusEffect.amount);
					break;
				case Status.MaxMana:
					CalculateStatusStack(statusEffect);
					break;
				case Status.ManaRegen:
					CalculateStatusStack(statusEffect);
					break;
				case Status.Shield:
					if (health.shield <= 0)
					{
						statusEffect.duration = -1f;
						if ((bool)statusEffect.display && (bool)statusEffect.display.gameObject)
						{
							SimplePool.Despawn(statusEffect.display.gameObject);
						}
						if ((bool)GetStatusEffect(Status.ShieldExte))
						{
							GetStatusEffect(Status.ShieldExte).duration = -1f;
						}
					}
					else
					{
						health.UpdateHealthText();
					}
					statusEffect.amountText.text = string.Empty;
					break;
				case Status.SpellPower:
					CalculateStatusStack(statusEffect);
					break;
				case Status.Reflect:
					statusEffect.SetText(statusEffect.amount);
					break;
				}
				if (statusEffect.duration / Time.timeScale > 0.1f || statusEffect.maxDuration / Time.timeScale > 0.1f)
				{
					statusEffect.icon.fillAmount = statusEffect.duration / statusEffect.maxDuration;
				}
				else if (statusEffect.duration > 0f && statusEffect.maxDuration / Time.timeScale > 0.1f)
				{
					statusEffect.icon.fillAmount = 1f;
				}
				else
				{
					statusEffect.icon.fillAmount = 0f;
				}
			}
			else
			{
				switch (statusEffect.status)
				{
				case Status.Stun:
					if (mov.state == State.Moving)
					{
						statusEffect.duration = 0f;
					}
					else
					{
						mov.SetState(State.Idle);
					}
					break;
				case Status.Chrono:
					Time.timeScale = 1f;
					break;
				case Status.Poison:
					lastSpellHit = null;
					Damage(Mathf.RoundToInt(statusEffect.amount), true, true);
					PlayOnce(ctrl.poisonSound);
					CreateHitFX(statusEffect.status, true);
					statusEffect.amount = Mathf.FloorToInt(statusEffect.amount / 2f);
					if (statusEffect.amount < BC.poisonMinimum)
					{
						statusEffect.amount = BC.poisonMinimum;
					}
					if (statusEffect.amount >= 10f)
					{
						statusEffect.duration = BC.poisonDuration;
						removeThisStatus = false;
						statusEffect.SetText(statusEffect.amount);
					}
					TriggerAllArtifacts(FTrigger.OnPoisonDmg, this);
					break;
				case Status.Slow:
					mov.lerpTime -= BC.slowAmount;
					break;
				case Status.ShieldExte:
					col.size = Vector2.one * sp.baseHurtboxSize;
					break;
				case Status.Haste:
					mov.lerpTime += BC.hasteAmount;
					break;
				}
				if (removeThisStatus)
				{
					seRemovalQueue.Add(statusEffect);
				}
			}
			if (statusEffectList.Count < 1)
			{
				break;
			}
		}
		for (int num2 = seRemovalQueue.Count - 1; num2 >= 0; num2--)
		{
			RemoveStatusEffect(seRemovalQueue[num2]);
		}
		if (seRemovalQueue.Count > 0)
		{
			deCtrl.statsScreen.UpdateStats(player);
		}
		seRemovalQueue.Clear();
		if (!(spriteRend.sharedMaterial.shader == defaultShader))
		{
			return;
		}
		if ((bool)GetStatusEffect(Status.Poison))
		{
			if (spriteRend.sharedMaterial.GetColor("_TintRGBA_Color_1") != ctrl.poisonColor)
			{
				spriteRend.sharedMaterial.SetColor("_TintRGBA_Color_1", ctrl.poisonColor);
			}
		}
		else if (spriteRend.sharedMaterial.GetColor("_TintRGBA_Color_1") != Color.clear)
		{
			spriteRend.sharedMaterial.SetColor("_TintRGBA_Color_1", Color.clear);
		}
	}

	public void CalculateStatusStack(StatusEffect se)
	{
		for (int num = se.statusStacks.Count - 1; num >= 0; num--)
		{
			if (se.statusStacks[num].duration <= 0f)
			{
				se.amount -= se.statusStacks[num].amount;
				se.statusStacks.Remove(se.statusStacks[num]);
				se.SetText(se.amount);
				deCtrl.statsScreen.UpdateStats(player);
			}
			else
			{
				se.statusStacks[num].duration -= Time.deltaTime;
			}
		}
		deCtrl.statsScreen.UpdateStats(player);
	}

	public bool HasStatusEffect(Status status)
	{
		if (GetStatusEffect(status) == null)
		{
			return false;
		}
		return true;
	}

	public StatusEffect GetStatusEffect(Status status)
	{
		for (int num = statusEffectList.Count - 1; num >= 0; num--)
		{
			if (status == statusEffectList[num].status)
			{
				return statusEffectList[num];
			}
		}
		return null;
	}

	public virtual void ApplyStun(bool showText = true, bool endIdle = false, bool stopDashing = true)
	{
		StopSelfAndChildCoroutines();
		hitAnimationActive = false;
		mov.SetState(State.Stunned);
		base.transform.position = mov.currentTile.transform.position;
		mov.startTile = mov.currentTile;
		mov.endTile = mov.currentTile;
		mov.MoveTo(mov.currentTile.x, mov.currentTile.y, false, true, false, false, endIdle, true, true, false);
		ResetAnimTriggers(stopDashing);
		if (showText)
		{
			CreateStatusText("Stunned!");
		}
	}

	public FloatingText CreateSpellText(SpellObject thisSpell, float duration = 0.5f)
	{
		return CreateFloatText(ctrl.floatingSpellPrefab, thisSpell.nameString, -20, 65, duration, thisSpell.sprite);
	}

	public void CreateStatusText(string text, float duration = 0.5f)
	{
		if (!S.I.RECORD_MODE)
		{
			CreateFloatText(ctrl.statusTextPrefab, text, 0, 5, duration);
		}
	}

	public FloatingText CreateFloatText(GameObject textPrefab, string amount = "EMPTY", int xOffset = 0, int yOffset = 0, float duration = 1.5f, Sprite sprite = null)
	{
		if (S.I.RECORD_MODE && textPrefab != ctrl.dmgTextPrefab)
		{
			return null;
		}
		return SimplePool.Spawn(textPrefab, Vector3.zero, ctrl.transform.rotation).GetComponent<FloatingText>().Set(SimplePool.Spawn(ctrl.floatingTextContainerPrefab).GetComponent<FloatingTextContainer>().SetContainer(xOffset, yOffset, base.transform, false, ctrl.canvas, duration, ctrl), ctrl, "FloatingText", amount, sprite);
	}

	public virtual void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Projectile"))
		{
			Projectile component = other.GetComponent<Projectile>();
			if (((component.spellObj != null || component.alignNum == alignNum) && (!component.spellObj.hitAllies || component.alignNum != alignNum || !(component.spellObj.being != this)) && (!component.spellObj.hitEnemies || component.alignNum != -alignNum) && (!component.spellObj.hitEnemies || component.alignNum != 0 || alignNum == 0) && (!component.spellObj.hitSelf || !(component.being == this)) && (!component.spellObj.hitStructures || alignNum != 0)) || (pet && (component.spellObj.HasEffect(Effect.HealBattle) || component.spellObj.HasEffect(Effect.Heal) || component.spellObj.HasEffect(Effect.Shield) || component.spellObj.HasEffect(Effect.ShieldSet) || component.spellObj.HasEffect(Effect.Trinity) || component.spellObj.HasEffect(Effect.Mana))) || (!component.spellObj.hitStructures && alignNum == 0 && component.spellObj.being != this) || (!component.spellObj.hitSelf && component.being == this))
			{
				return;
			}
			lastHitBy = component.being;
			if (component.being != this)
			{
				lastHitByOther = component.being;
			}
			lastSpellHit = component.spellObj;
			if (component.onHitSpell || component.onHitTriggerArts || component.flame)
			{
				battleGrid.lastTargetHit = this;
				battleGrid.lastTargetHitTile = mov.endTile;
			}
			if (!string.IsNullOrEmpty(component.hitSound))
			{
				PlayOnce(component.hitSound);
			}
			if (!string.IsNullOrEmpty(component.hitAnim))
			{
				component.spellObj.P.CreateCustomEffect(component.spellObj, mov.currentTile.x, mov.currentTile.y, 0f, component.hitAnim);
			}
			if (Time.time >= lastGracePeriod + gracePeriodDuration || component.flame)
			{
				if (!component.flame && component.damage > 0)
				{
					lastGracePeriod = Time.time;
				}
				HitAmount(component.damage, component.onHitTriggerArts, component.onHitShake, component, component.pierceDefense, component.pierceShield);
			}
			if (component.onHitSpell && (bool)component.spell && component.spell.spellObj != null)
			{
				component.spell.spellObj.OnHit(this);
			}
			if (component.statusList.Count > 0 && col.enabled)
			{
				for (int i = 0; i < component.statusList.Count; i++)
				{
					AddStatus(component.statusList[i].type, component.statusList[i].amount, component.statusList[i].duration);
				}
			}
			if (alignNum != component.alignNum)
			{
				component.target = this;
			}
			if (component.destroyOnHit)
			{
				component.Despawn();
			}
		}
		else if (other.gameObject.CompareTag("Tile"))
		{
			Tile component2 = other.gameObject.GetComponent<Tile>();
			if (component2.type == TileType.Spiked)
			{
				HitAmount(5);
			}
		}
	}

	public virtual void HitAmount(int damage, bool onHitTriggerArts = false, bool onHitShake = false, Projectile attackRef = null, bool pierceDefense = false, bool pierceShield = false, bool link = false)
	{
		lastDamageAmount = damage;
		if (damage != 0)
		{
			if (damage < 0)
			{
				Heal(-damage);
			}
			else
			{
				if (onHitShake)
				{
					ctrl.camCtrl.Shake(0);
				}
				StatusEffect statusEffect = GetStatusEffect(Status.Reflect);
				if ((bool)statusEffect)
				{
					SpellObject spellObject = ReflectShot();
					spellObject.damage = damage;
					spellObject.StartCast();
					statusEffect.amount -= 1f;
					if ((bool)statusEffect.display.anim)
					{
						statusEffect.display.anim.SetTrigger("knockback");
					}
					if (statusEffect.amount < 1f)
					{
						RemoveStatusEffect(statusEffect);
					}
					return;
				}
				if ((bool)attackRef && attackRef.flame)
				{
					TriggerAllArtifacts(FTrigger.OnFlameHit, this);
				}
				StatusEffect statusEffect2 = GetStatusEffect(Status.Fragile);
				if (onHitTriggerArts)
				{
					OnHit(attackRef);
				}
				if (statusEffect2 != null && attackRef != null && !attackRef.spellObj.tags.Contains(Tag.Drone) && !attackRef.flame)
				{
					damage = Mathf.FloorToInt((float)damage * BC.fragileMultiplier);
					statusEffect2.amount -= 1f;
					if (statusEffect2.amount < 1f)
					{
						RemoveStatusEffect(statusEffect2);
					}
				}
				Damage(damage, pierceDefense, pierceShield, false, ((object)attackRef != null) ? attackRef.spellObj : null);
				if (onHitTriggerArts)
				{
					AfterHit(attackRef);
				}
			}
		}
		if (!onHitTriggerArts)
		{
			return;
		}
		for (int num = battleGrid.currentBeings.Count - 1; num >= 0; num--)
		{
			Being being = battleGrid.currentBeings[num];
			if ((bool)being.GetStatusEffect(Status.Link) && being != this && GetStatusEffect(Status.Reflect) == null && !link)
			{
				being.HitAmount(damage, onHitTriggerArts, false, null, false, false, true);
			}
		}
	}

	public virtual SpellObject ReflectShot()
	{
		return ctrl.deCtrl.CreateSpellBase("ReflectShot", this);
	}

	public virtual void OnHit(Projectile attackRef)
	{
		int forwardedHitAmount = 0;
		if (attackRef != null)
		{
			forwardedHitAmount = attackRef.damage;
			attackRef.being.TriggerArtifacts(FTrigger.OnAnyHit, this, forwardedHitAmount);
		}
		TriggerArtifacts(FTrigger.OnTakeHit, this, forwardedHitAmount);
		if ((bool)attackRef && attackRef.spellObj.tags.Contains(Tag.Weapon))
		{
			attackRef.being.TriggerArtifacts(FTrigger.OnWeaponHit, this, forwardedHitAmount);
		}
	}

	public virtual void AfterHit(Projectile attackRef)
	{
		int forwardedHitAmount = 0;
		if (attackRef != null)
		{
			forwardedHitAmount = attackRef.damage;
			attackRef.being.TriggerArtifacts(FTrigger.AfterAnyHit, this, forwardedHitAmount);
		}
		TriggerArtifacts(FTrigger.AfterTakeHit, this, forwardedHitAmount);
		if ((bool)attackRef && (bool)attackRef.being)
		{
			attackRef.being.TriggerArtifacts(FTrigger.OnLandHit, this, forwardedHitAmount);
		}
	}

	public virtual void Heal(int amount)
	{
		health.ModifyHealth(amount);
		CreateHitFX(Status.Normal, true, -amount);
	}

	public virtual void Damage(int amount, bool pierceDefense = false, bool pierceShield = false, bool pierceInvince = false, ItemObject itemObj = null)
	{
		if ((invinceTime >= Time.time && !pierceInvince) || this == null || ActivateSavior(amount))
		{
			return;
		}
		int current = health.current;
		int shield = health.shield;
		if (!pierceDefense)
		{
			if (amount > health.shield)
			{
				if (amount - health.shield > beingObj.defense)
				{
					amount -= beingObj.defense;
					if ((bool)GetStatusEffect(Status.Defense))
					{
						amount -= Mathf.RoundToInt(GetStatusEffect(Status.Defense).amount);
					}
					if (amount <= health.shield)
					{
						amount = health.shield + 1;
					}
				}
				else
				{
					amount = health.shield + Mathf.Clamp(amount - health.shield - beingObj.defense, 1, amount);
				}
			}
			else if (shieldDefense)
			{
				amount -= beingObj.defense;
				if ((bool)GetStatusEffect(Status.Defense))
				{
					amount -= Mathf.RoundToInt(GetStatusEffect(Status.Defense).amount);
				}
			}
		}
		if (amount < 1)
		{
			amount = 1;
		}
		lastDamageAmount = amount;
		CreateHitFX(Status.Normal, false, amount);
		healthBeforeHit = health.current;
		if (pierceShield)
		{
			lastTrueDamageAmount = amount;
		}
		else
		{
			lastTrueDamageAmount = amount - health.shield;
			if (lastTrueDamageAmount < 0)
			{
				lastTrueDamageAmount = 0;
			}
		}
		health.ModifyHealth(-amount, pierceShield);
		if (shield > 0 && health.shield <= 0)
		{
			PlayOnce(hitSoundShieldBreak);
		}
		TriggerArtifacts(FTrigger.OnTakeDmg, this, amount);
		TriggerArtifacts(FTrigger.OnHPBelow);
		if ((health.shield <= 0 && current > health.current) || (health.shield <= 0 && health.current <= 1))
		{
			if ((bool)anim && !anim.GetBool("downed") && !dontInterruptAnim && !dontInterruptChannelAnim && !dontHitAnim && mov.state != State.Attacking)
			{
				if (co_HitAnimReset != null)
				{
					StopCoroutine(co_HitAnimReset);
					co_HitAnimReset = null;
				}
				if (co_HitAnimReset == null)
				{
					co_HitAnimReset = StartCoroutine(HitAnimReset());
				}
				StartCoroutine(HitAnimReset());
				anim.SetTrigger("takeDmg");
			}
			if (amount <= 1)
			{
				PlayOnce(hitSoundTiny);
			}
			else if (amount < 11)
			{
				PlayOnce(hitSoundLight);
			}
			else if (amount < 100)
			{
				PlayOnce(hitSound);
			}
			else
			{
				PlayOnce(hitSoundHeavy);
			}
		}
		else if (health.shield > 0)
		{
			if (pierceShield)
			{
				if (amount <= 1)
				{
					PlayOnce(hitSoundTiny);
				}
				else if (amount < 11)
				{
					PlayOnce(hitSoundLight);
				}
				else if (amount < 100)
				{
					PlayOnce(hitSound);
				}
				else
				{
					PlayOnce(hitSoundHeavy);
				}
			}
			else
			{
				if ((bool)GetStatusEffect(Status.Shield).display && (bool)GetStatusEffect(Status.Shield).display.anim)
				{
					GetStatusEffect(Status.Shield).display.anim.SetTrigger("knockback");
				}
				if (amount <= 1)
				{
					PlayOnce(hitSoundShieldTiny);
				}
				else
				{
					PlayOnce(hitSoundShield);
				}
			}
		}
		if (itemObj != null)
		{
			itemObj.being.damageDealtThisBattle += amount;
		}
		spriteRend.sharedMaterial.shader = hitShader;
		flashTime = flashLength;
	}

	private IEnumerator HitAnimReset()
	{
		hitAnimationActive = true;
		yield return new WaitForSeconds(0.25f);
		hitAnimationActive = false;
	}

	public void SetInvince(float duration)
	{
		invinceTime = duration + Time.time;
	}

	public void AddInvince(float duration)
	{
		if (duration > 0f && invinceTime < Time.time)
		{
			invinceTime = Time.time;
		}
		invinceTime += duration;
	}

	public void AddCappedInvince(float duration)
	{
		float num = invinceTime - Time.time;
		if (num > 0f && num < duration)
		{
			duration -= num;
			AddInvince(duration);
		}
		else if (num <= 0f)
		{
			AddInvince(duration);
		}
	}

	public IEnumerator<float> WaitUntilIdle(State untilState = State.Idle)
	{
		while (mov.state != untilState)
		{
			yield return float.NegativeInfinity;
		}
	}

	public virtual void CreateHitFX(Status damageType = Status.Normal, bool pop = false, int damage = 0)
	{
		CreateHitFX(damageType.ToString(), pop, damage);
	}

	public virtual void CreateHitFX(string damageType, bool pop = false, int damage = 0)
	{
		int num = UnityEngine.Random.Range(-10, 10);
		int num2 = UnityEngine.Random.Range(-15, 10);
		if (pop)
		{
			num = 0;
			num2 = 0;
		}
		SimplePool.Spawn(ctrl.hitFXPrefab, base.transform.position + new Vector3(num, num2, 0f), base.transform.rotation).GetComponent<HitFX>().Play(damageType, pop, damage, ctrl.transform);
	}

	public void Startup()
	{
		foreach (SpellObject startup in startups)
		{
			startup.StartCast();
		}
		startups.Clear();
	}

	public IEnumerator TimeoutC()
	{
		if (beingObj.timeoutAnim)
		{
			anim.SetTrigger("timeout");
		}
		foreach (SpellObject timeout in timeouts)
		{
			timeout.StartCast();
			yield return null;
		}
		yield return new WaitForSeconds(1f);
	}

	public virtual void StartDeath(bool triggerDeathrattles = true)
	{
		dontInterruptAnim = true;
		DeathEffects(triggerDeathrattles);
		if (deathrattlesTriggered)
		{
			Timing.RunCoroutine(_DeathFinal().CancelWith(base.gameObject));
		}
	}

	protected virtual bool DeathEffects(bool triggerDeathrattles)
	{
		if (!triggerDeathrattles)
		{
			deathrattlesTriggered = true;
		}
		if ((bool)col)
		{
			col.enabled = false;
		}
		AddInvince(1f);
		if (spellAppObj != null)
		{
			spellAppObj.Trigger(FTrigger.OnDeath);
		}
		if (!deathrattlesTriggered)
		{
			anim.SetTrigger("die");
			Timing.RunCoroutine(DeathrattlesC().CancelWith(base.gameObject));
			return false;
		}
		deathrattles.Clear();
		TriggerArtifacts(FTrigger.OnDeath);
		S.I.PlayOnce(dieSound, IsReference());
		ctrl.camCtrl.Shake(1);
		sprite = spriteRend.sprite;
		if (lastSpellHit != null && !lastSpellHit.being.dead)
		{
			lastSpellHit.Trigger(FTrigger.Execute, false, this);
		}
		StopSelfAndChildCoroutines();
		return true;
	}

	protected IEnumerator<float> DeathrattlesC()
	{
		int frameCount = Time.frameCount;
		inDeathSequence = true;
		int i = deathrattles.Count - 1;
		while (i >= 0 && i < deathrattles.Count && deathrattles[i] != null)
		{
			deathrattles[i].StartCast();
			yield return float.NegativeInfinity;
			i--;
		}
		deathrattles.Clear();
		if (beingObj.deathDelay > 0f)
		{
			yield return Timing.WaitForSeconds(beingObj.deathDelay);
		}
		deathrattlesTriggered = true;
		inDeathSequence = false;
		StartDeath();
	}

	public void DropItems()
	{
		ctrl.StartCoroutine(ctrl.SpawnLootEffect(beingObj.experience, beingObj.money, base.transform.position, ctrl.DropItems(beingObj.rewardList), new List<int>(orbs)));
		beingObj.rewardList.Clear();
		orbs.Clear();
	}

	public void StopSelfAndChildCoroutines()
	{
		StopAllCoroutines();
		foreach (Transform item in base.transform)
		{
			Item component = item.GetComponent<Item>();
			if (component != null && (component.itemObj == null || (component.itemObj.spellObj != null && component.itemObj.spellObj.destroyOnDeath)))
			{
				component.StopEverything();
			}
		}
	}

	public int GetAmount(Status status)
	{
		if ((bool)GetStatusEffect(status))
		{
			return Mathf.RoundToInt(GetStatusEffect(status).amount);
		}
		return 0;
	}

	public virtual void Clear(bool overrideDeathSequence = false)
	{
		if (!inDeathSequence || overrideDeathSequence)
		{
			StopSelfAndChildCoroutines();
			if (clearSpells.Count > 0)
			{
				Timing.RunCoroutine(ClearSpellsC().CancelWith(base.gameObject));
				return;
			}
			beingObj.experience = 0;
			beingObj.money = 0;
			orbs.Clear();
			beingObj.rewardList.Clear();
			spriteRend.enabled = false;
			shadow.SetActive(false);
			cleared = true;
			Remove();
		}
	}

	private IEnumerator<float> ClearSpellsC()
	{
		inDeathSequence = true;
		anim.SetTrigger("fire");
		List<SpellObject> currentClearSpells = new List<SpellObject>(clearSpells);
		foreach (SpellObject spell in currentClearSpells)
		{
			if (spell != null)
			{
				spell.StartCast();
				yield return float.NegativeInfinity;
			}
		}
		clearSpells.Clear();
		yield return Timing.WaitForSeconds(beingObj.clearDelay);
		if (beingObj.clearAnim)
		{
			anim.SetTrigger("clear");
			yield return Timing.WaitForSeconds(0.14f);
		}
		inDeathSequence = false;
		Clear();
	}

	public virtual IEnumerator<float> _DeathFinal()
	{
		inDeathSequence = true;
		sp.explosionGen.CreateExplosionString(1, base.transform.position, base.transform.rotation, this);
		S.I.PlayOnce(explosionSound, IsReference());
		if ((bool)talkBubble)
		{
			talkBubble.Fade();
		}
		yield return float.NegativeInfinity;
		Remove();
		yield return Timing.WaitForSeconds(0.2f);
		inDeathSequence = false;
		spriteRend.enabled = false;
		shadow.SetActive(false);
		yield return Timing.WaitForSeconds(0.6f);
		if ((bool)talkBubble)
		{
			UnityEngine.Object.Destroy(talkBubble.gameObject);
		}
	}

	public virtual void Remove()
	{
		if ((bool)col)
		{
			col.enabled = false;
		}
		dead = true;
		mov.currentTile.SetOccupation(0, mov.hovering);
		RemoveAllStatuses();
		statusEffectList.Clear();
		ClearProjectiles();
		DestroySpells();
		DestroyArtifacts();
		if ((bool)beingStatsPanel)
		{
			UnityEngine.Object.Destroy(beingStatsPanel.gameObject);
		}
		for (int num = statusDisplays.Count - 1; num >= 0; num--)
		{
			if ((bool)statusDisplays[num])
			{
				UnityEngine.Object.Destroy(statusDisplays[num].gameObject);
			}
		}
		statusDisplays.Clear();
	}

	public void RemoveBuff()
	{
		if (buffs.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, buffs.Count);
			artObjs.Remove(buffs[index]);
			UnityEngine.Object.Destroy(buffs[index].art.gameObject);
			buffs.RemoveAt(index);
		}
	}

	public void RemoveAllBuffs()
	{
		while (buffs.Count > 0)
		{
			RemoveBuff();
		}
	}

	public void ClearProjectiles()
	{
		for (int num = activeProjectiles.Count - 1; num >= 0; num--)
		{
			if (!activeProjectiles[num].spellObj.HasEffect(Effect.HealBattle) && !activeProjectiles[num].spellObj.HasEffect(Effect.Heal) && activeProjectiles[num].spellObj.destroyOnDeath && (!activeProjectiles[num].CompareTag("Effect") || activeProjectiles[num].arcing || activeProjectiles[num].warningTimer != null))
			{
				SimplePool.Despawn(activeProjectiles[num]);
			}
		}
	}

	public virtual void TriggerAllArtifacts(FTrigger fTrigger, Being forwardedTargetHit = null, int forwardedHitAmount = 0)
	{
		deCtrl.TriggerAllArtifacts(fTrigger, battleGrid, forwardedTargetHit, forwardedHitAmount);
	}

	public void PauseSpells()
	{
		for (int num = currentSpellObjs.Count - 1; num >= 0; num--)
		{
			if (currentSpellObjs[num].destroyOnDeath && (bool)currentSpellObjs[num].spell)
			{
				currentSpellObjs[num].spell.StopEverything();
			}
		}
	}

	public void DestroySpells()
	{
		for (int num = currentSpellObjs.Count - 1; num >= 0; num--)
		{
			if (currentSpellObjs[num].destroyOnDeath && (bool)currentSpellObjs[num].spell)
			{
				currentSpellObjs[num].spell.StopEverything();
				UnityEngine.Object.Destroy(currentSpellObjs[num].spell.gameObject);
				currentSpellObjs.RemoveAt(num);
			}
		}
	}

	public void DestroyArtifacts()
	{
		for (int num = artObjs.Count - 1; num >= 0; num--)
		{
			if (artObjs != null)
			{
				UnityEngine.Object.Destroy(artObjs[num].art.gameObject);
				artObjs.RemoveAt(num);
			}
		}
	}

	public virtual void Clean()
	{
		mov.currentTile.SetOccupation(0, mov.hovering);
		if ((bool)talkBubble)
		{
			UnityEngine.Object.Destroy(talkBubble.gameObject);
		}
		if ((bool)beingStatsPanel)
		{
			UnityEngine.Object.Destroy(beingStatsPanel.gameObject);
		}
		if ((bool)this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void FullClean()
	{
		Clear(true);
		Clean();
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(spriteRend.material);
	}

	public bool HasParameter(string paramName)
	{
		if ((bool)anim)
		{
			AnimatorControllerParameter[] parameters = anim.parameters;
			foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
			{
				if (animatorControllerParameter.name == paramName)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddOrb(int rarity = -1)
	{
		orbs.Add(rarity);
	}

	public Tile TileLocal(int x, int y = 0)
	{
		return battleGrid.TileClosestExisting(mov.endTile.x + x * FacingDirection(), mov.endTile.y + y);
	}

	public bool IsEnemy()
	{
		if (alignNum <= 0 && player == null)
		{
			return true;
		}
		return false;
	}

	public int FacingDirection()
	{
		return Mathf.RoundToInt(base.transform.TransformDirection(Vector3.right).x);
	}

	protected virtual bool ActivateSavior(int damage)
	{
		return false;
	}

	public virtual void MustBeInBattleWarning()
	{
	}

	public void ResetAnimTriggers(bool resetDashing = true)
	{
		anim.ResetTrigger("charge");
		anim.SetTrigger("release");
		anim.ResetTrigger("specialStart");
		anim.SetTrigger("specialEnd");
		anim.ResetTrigger("channel");
		anim.ResetTrigger("throw");
		anim.ResetTrigger("fire");
		anim.ResetTrigger("undown");
		anim.SetBool("raised", false);
		if (resetDashing)
		{
			anim.SetBool("dashing", false);
		}
		anim.SetBool("airborne", false);
		StartCoroutine(FinishAnimTriggers());
	}

	public IEnumerator ResetAnimTriggersC()
	{
		ResetAnimTriggers();
		yield return StartCoroutine(FinishAnimTriggers());
	}

	private IEnumerator FinishAnimTriggers()
	{
		yield return null;
		anim.ResetTrigger("specialEnd");
		anim.ResetTrigger("release");
	}

	public virtual void Reset()
	{
		ResetAnimTriggers();
		RemoveAllStatuses();
		if (mov.state != 0)
		{
			mov.state = State.Idle;
		}
		mov.MoveToCurrentTile();
	}

	public void PlayOnce(string audioName)
	{
		AudioClip audioClip = deCtrl.itemMan.GetAudioClip(audioName);
		PlayOnce(deCtrl.itemMan.GetAudioClip(audioName));
	}

	public void PlayOnce(AudioClip audioClip)
	{
		audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
		if (IsReference())
		{
			audioSource.PlayOneShot(audioClip, S.I.previewVolumeMultiplier);
		}
		else
		{
			audioSource.PlayOneShot(audioClip);
		}
	}

	public void TriggerArtifacts(FTrigger fTrigger, Being forwardedTargetHit = null, int forwardedHitAmount = 0)
	{
		for (int num = artObjs.Count - 1; num >= 0; num--)
		{
			if (!artObjs[num].depleted)
			{
				artObjs[num].Trigger(fTrigger, false, forwardedTargetHit, forwardedHitAmount);
			}
		}
		for (int num2 = pactObjs.Count - 1; num2 >= 0; num2--)
		{
			if (!pactObjs[num2].depleted)
			{
				pactObjs[num2].Trigger(fTrigger);
			}
		}
	}
}
