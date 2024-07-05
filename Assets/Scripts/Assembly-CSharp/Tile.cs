using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class Tile : MonoBehaviour
{
	public TileType type;

	public int align = 0;

	public int occupation = 0;

	public int x;

	public int y;

	public Sprite[] tileSprites;

	public Sprite[] tileTypeSprites;

	public float restoreCooldown = 3f;

	private float restoreTime = 0f;

	public bool flashing;

	public bool vacant = true;

	public SpriteRenderer alignSpriteRend;

	public SpriteRenderer typeSpriteRend;

	public SpriteRenderer backSpriteRend;

	public SpriteRenderer shadowSpriteRend;

	public Transform container;

	public Material normalMat;

	public Material enemyAtkMat;

	public Material playerAtkMat;

	public Material healAtkMat;

	public Material occupiedMat;

	public Animator anim;

	public AudioSource audioSource;

	public AudioClip breakSound;

	public List<AudioClip> crackSounds;

	public float flashTime = 0.05f;

	private float lastFlashTime;

	private int atkAlignNum;

	public Projectile flameVisual;

	public float flameTimer = 0f;

	public float flameTicksRemaining = 0f;

	public BC ctrl;

	private BoxCollider2D col;

	public float flickerWarningTime = 1f;

	private int flicker = 0;

	public int colliderBuffer = 32;

	private Collider2D[] m_buffer;

	public BattleGrid battleGrid;

	public SpellObject flameGen;

	public Sprite normalSprite;

	public Sprite crackedSprite;

	public Sprite brokenSprite;

	public Sprite spikedSprite;

	public Sprite noneSprite;

	public Being currentFlameOwner;

	public bool touchedProjectile;

	public bool triggerArts = true;

	private void Awake()
	{
		ctrl = S.I.batCtrl;
		col = GetComponent<BoxCollider2D>();
		m_buffer = new Collider2D[colliderBuffer];
		audioSource.outputAudioMixerGroup = S.sfxGroup;
	}

	public Tile Set(BattleGrid newBattleGrid, int newX, int newY, bool triggerArts)
	{
		triggerArts = false;
		battleGrid = newBattleGrid;
		base.transform.parent = battleGrid.gridContainer.transform;
		x = newX;
		y = newY;
		base.transform.name = "Tile" + x + "-" + y;
		if (x < battleGrid.gridLength / 2)
		{
			SetAlign(1);
		}
		else
		{
			SetAlign(-1);
		}
		backSpriteRend.sortingOrder = 99 - y;
		return this;
	}

	public void Reset()
	{
		SetOccupation(0);
		SetType(TileType.Normal);
		container.transform.position = base.transform.position;
		Extinguish();
		if (x < battleGrid.gridLength / 2)
		{
			SetAlign(1);
		}
		else
		{
			SetAlign(-1);
		}
	}

	private void OnEnable()
	{
		SetType(TileType.Normal);
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Update()
	{
		if (type == TileType.Broken || type == TileType.Cracked)
		{
			if (restoreTime != 0f && Time.time >= restoreTime)
			{
				SetType(TileType.Normal);
			}
			else if (Time.time >= restoreTime - flickerWarningTime && Time.time < restoreTime)
			{
				if (flicker == 0)
				{
					typeSpriteRend.sprite = normalSprite;
					flicker = 1;
				}
				else
				{
					if (type == TileType.Broken)
					{
						typeSpriteRend.sprite = brokenSprite;
					}
					else if (type == TileType.Cracked)
					{
						typeSpriteRend.sprite = crackedSprite;
					}
					flicker = 0;
				}
			}
		}
		if (occupation > 0 && backSpriteRend.sharedMaterial != occupiedMat)
		{
			backSpriteRend.sharedMaterial = occupiedMat;
		}
		touchedProjectile = false;
		int num = Physics2D.OverlapBoxNonAlloc(base.transform.position, col.size, 0f, m_buffer);
		Collider2D[] buffer = m_buffer;
		foreach (Collider2D collider2D in buffer)
		{
			if (collider2D == null)
			{
				break;
			}
			if (!collider2D.gameObject.CompareTag("Projectile"))
			{
				continue;
			}
			Projectile component = collider2D.GetComponent<Projectile>();
			atkAlignNum = component.alignNum;
			touchedProjectile = true;
			if (component.damage < 0)
			{
				if (backSpriteRend.sharedMaterial != healAtkMat)
				{
					backSpriteRend.sharedMaterial = healAtkMat;
				}
			}
			else if (atkAlignNum > 0 && !component.spellObj.hitSelf)
			{
				if (backSpriteRend.sharedMaterial != playerAtkMat)
				{
					backSpriteRend.sharedMaterial = playerAtkMat;
				}
			}
			else if (backSpriteRend.sharedMaterial != enemyAtkMat)
			{
				backSpriteRend.sharedMaterial = enemyAtkMat;
			}
		}
		for (int j = 0; j < m_buffer.Length; j++)
		{
			m_buffer[j] = null;
		}
		if (!touchedProjectile && occupation <= 0 && backSpriteRend.sharedMaterial != normalMat)
		{
			backSpriteRend.sharedMaterial = normalMat;
		}
		if (flameTimer < 0f && flameTicksRemaining > 0f)
		{
			SpawnFlame(currentFlameOwner);
		}
		else if (flameTimer >= 0f)
		{
			flameTimer -= Time.deltaTime;
		}
		if ((bool)flameVisual && flameTimer < 0f && flameTicksRemaining <= 0f)
		{
			Extinguish();
		}
	}

	public void Flame(ItemObject itemObj)
	{
		if (!battleGrid.currentFlames.Contains(this))
		{
			battleGrid.currentFlames.Add(this);
		}
		flameTicksRemaining = BC.flameTicks;
		SpawnFlame(itemObj.being);
	}

	public void Extinguish()
	{
		if (battleGrid.currentFlames.Contains(this))
		{
			battleGrid.currentFlames.Remove(this);
		}
		if ((bool)flameVisual)
		{
			SimplePool.Despawn(flameVisual);
		}
		flameVisual = null;
		flameTimer = 0f;
		flameTicksRemaining = 0f;
	}

	private void SpawnFlame(Being flameOwner)
	{
		if (flameOwner == null || !flameOwner.gameObject.activeInHierarchy)
		{
			flameTicksRemaining = 0f;
			return;
		}
		if (flameGen == null)
		{
			flameGen = ctrl.deCtrl.CreateSpellBase("DefaultFlameGen", flameOwner);
		}
		currentFlameOwner = flameOwner;
		flameGen.being = currentFlameOwner;
		if (flameGen.being == null)
		{
			Extinguish();
			if ((bool)flameGen.spell)
			{
				Object.Destroy(flameGen.spell.gameObject);
			}
			return;
		}
		float flameTickTime = BC.flameTickTime;
		flameGen.hitSelf = true;
		flameGen.hitAllies = true;
		if (flameVisual == null)
		{
			flameVisual = flameGen.P.CreateCustomEffect(flameGen, this, 99f, "FireTile", false, 0f, "fireplace");
		}
		Projectile projectile = flameGen.P.CreateCustomShot(flameGen, this, false, flameTickTime + 0.05f, BC.flameDmg);
		projectile.flame = true;
		projectile.pierceDefense = false;
		projectile.onTouchTile = false;
		projectile.onHitSpell = false;
		projectile.onHitTriggerArts = false;
		flameTimer = BC.flameTickTime;
		flameTicksRemaining -= 1f;
		if (!AchievementsCtrl.IsUnlocked("Watch_The_World_Burn") && battleGrid != ctrl.ti.refBattleGrid)
		{
			float amountVar = 0f;
			if (ctrl.GetAmount(new AmountApp(ref amountVar, "Flames"), 0f) >= 32f)
			{
				AchievementsCtrl.UnlockAchievement("Watch_The_World_Burn");
			}
		}
	}

	public void SetType(TileType newType)
	{
		typeSpriteRend.enabled = true;
		switch (newType)
		{
		case TileType.None:
			backSpriteRend.sprite = noneSprite;
			typeSpriteRend.enabled = false;
			break;
		case TileType.Broken:
			if (occupation > 0)
			{
				newType = TileType.Cracked;
				typeSpriteRend.sprite = crackedSprite;
				PlayOnce(crackSounds[Random.Range(0, crackSounds.Count)]);
			}
			else
			{
				typeSpriteRend.sprite = null;
				if (type != TileType.Broken)
				{
					newType = TileType.Broken;
					backSpriteRend.sprite = brokenSprite;
					PlayOnce(breakSound);
					if (triggerArts && !IsReference())
					{
						ctrl.deCtrl.TriggerAllArtifacts(FTrigger.OnTileBreak);
					}
				}
			}
			if (!AchievementsCtrl.IsUnlocked("Terraform") || !SaveDataCtrl.Get("TerraDreadwyrm", false))
			{
				float amountVar = 0f;
				if (battleGrid != ctrl.ti.refBattleGrid && ctrl.GetAmount(new AmountApp(ref amountVar, "BrokenTiles"), 0f) >= 16f)
				{
					AchievementsCtrl.UnlockAchievement("Terraform");
					S.AddSkinUnlock("TerraDreadwyrm");
				}
			}
			break;
		case TileType.Normal:
			typeSpriteRend.enabled = false;
			backSpriteRend.sprite = normalSprite;
			break;
		case TileType.Spiked:
			typeSpriteRend.sprite = spikedSprite;
			break;
		case TileType.Cracked:
			if (type != TileType.Broken)
			{
				newType = TileType.Cracked;
				typeSpriteRend.sprite = crackedSprite;
				PlayOnce(crackSounds[Random.Range(0, crackSounds.Count)]);
			}
			break;
		}
		type = newType;
	}

	public void SetAlign(int tileAlignNum)
	{
		align = tileAlignNum;
		switch (align)
		{
		case 0:
			alignSpriteRend.sprite = tileSprites[0];
			break;
		case 1:
			alignSpriteRend.sprite = tileSprites[1];
			break;
		case -1:
			alignSpriteRend.sprite = tileSprites[2];
			break;
		}
	}

	public void SetOccupation(int tileOccupiedNum, bool hovering = false)
	{
		occupation = tileOccupiedNum;
		switch (occupation)
		{
		case 0:
		{
			if (type != TileType.Cracked || !(Time.time <= restoreTime) || hovering)
			{
				break;
			}
			bool flag = false;
			foreach (Player currentPlayer in ctrl.currentPlayers)
			{
				if (currentPlayer.mov.endTile == this)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				SetType(TileType.Broken);
				anim.SetTrigger("Break");
			}
			break;
		}
		}
		if (occupation == 0)
		{
			vacant = true;
		}
		else
		{
			vacant = false;
		}
	}

	public bool IsMoveable()
	{
		if (type != 0 && type != TileType.Broken)
		{
			return true;
		}
		return false;
	}

	public bool IsOccupiable()
	{
		if (IsMoveable() && occupation < 1)
		{
			return true;
		}
		return false;
	}

	public void Break(float duration = -1f)
	{
		if (duration == -1f)
		{
			duration = restoreCooldown;
		}
		anim.SetTrigger("Break");
		SetType(TileType.Broken);
		restoreTime = duration + Time.time;
		if (duration == 0f)
		{
			restoreTime = 0f;
		}
	}

	public void Crack(float duration = -1f)
	{
		if (type != TileType.Broken)
		{
			if (duration == -1f)
			{
				duration = restoreCooldown;
			}
			SetType(TileType.Cracked);
			restoreTime = duration + Time.time;
			if (duration == 0f)
			{
				restoreTime = 0f;
			}
		}
	}

	public void Fix()
	{
		if (type != TileType.Normal)
		{
			SetType(TileType.Normal);
			restoreTime = Time.time;
		}
	}

	public bool AlignedTo(int numCheck)
	{
		if (align == numCheck || numCheck == 0 || align == 0)
		{
			return true;
		}
		return false;
	}

	public bool IsReference()
	{
		return ctrl.ti.refBattleGrid != null && battleGrid == ctrl.ti.refBattleGrid;
	}

	public void PlayOnce(AudioClip audioClip)
	{
		audioSource.pitch = Random.Range(0.9f, 1.1f);
		if (IsReference())
		{
			audioSource.PlayOneShot(audioClip, S.I.previewVolumeMultiplier);
		}
		else
		{
			audioSource.PlayOneShot(audioClip);
		}
	}
}
