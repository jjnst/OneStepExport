using UnityEngine;

public class RewardDrop : SinFollower
{
	public Color expColor;

	public Sprite[] expSprites;

	public Color moneyColor;

	public Sprite[] moneySprites;

	public Color[] orbColors;

	public Sprite[] orbSprites;

	public Color spellColor;

	public Sprite dropSpellSprite;

	public Color artColor;

	public Sprite dropArtSprite;

	public Color wepColor;

	public Sprite dropWepSprite;

	public Color healthColor;

	public Sprite dropHealthSprite;

	public ItemType itemType;

	private float savedBaseVariance = 0f;

	private float savedBaseOffset = 0f;

	private int moneyPayload = 0;

	protected override void Awake()
	{
		savedBaseVariance = endYVariance;
		savedBaseOffset = endYOffset;
	}

	protected override void LateUpdate()
	{
		if (itemType == ItemType.Art)
		{
			base.LateUpdate();
		}
		else
		{
			if (reachedTarget)
			{
				return;
			}
			if (!target)
			{
				DisableSelf();
				return;
			}
			if ((bool)target && base.transform.position.x != target.position.x)
			{
				Move();
				return;
			}
			reachedTarget = true;
			DisableSelf();
			if (moneyPayload > 0)
			{
				S.I.shopCtrl.ModifySera(moneyPayload);
			}
		}
	}

	public override void DisableSelf()
	{
		if (itemType == ItemType.Art)
		{
			target.GetComponent<Animator>().SetBool("spawned", true);
		}
		base.DisableSelf();
	}

	public override void Reset()
	{
		base.Reset();
		itemType = ItemType.Item;
		endYVariance = savedBaseVariance;
		endYOffset = savedBaseOffset;
	}

	public void MakeExp(int num)
	{
		spriteRenderer.sprite = expSprites[num];
		ParticleSystem.MainModule main = particleSys.main;
		main.startColor = expColor;
	}

	public void MakeMoney(int num, int newMoneyPayload)
	{
		spriteRenderer.sprite = moneySprites[num];
		ParticleSystem.MainModule main = particleSys.main;
		main.startColor = moneyColor;
		endYVariance = 0f;
		endYOffset = 0f;
		moneyPayload = newMoneyPayload;
	}

	public void MakeItem(ItemObject itemObj)
	{
		Reset();
		itemType = itemObj.type;
		switch (itemType)
		{
		case ItemType.Spell:
			MakeSpell();
			break;
		case ItemType.Art:
			MakeArt(itemObj);
			break;
		case ItemType.Wep:
			MakeWep();
			break;
		}
	}

	public void MakeOrb(int num)
	{
		spriteRenderer.sprite = orbSprites[0];
		ParticleSystem.MainModule main = particleSys.main;
		main.startColor = orbColors[num];
	}

	public void MakeSpell()
	{
		spriteRenderer.sprite = dropSpellSprite;
		ParticleSystem.MainModule main = particleSys.main;
		main.startColor = spellColor;
	}

	public void MakeArt(ItemObject itemObj)
	{
		target = itemObj.artObj.art.listCard.transform;
		spriteRenderer.sprite = dropArtSprite;
		ParticleSystem.MainModule main = particleSys.main;
		main.startColor = artColor;
		endYVariance = 0f;
		endYOffset = 0f;
		Calculate();
	}

	public void MakeWep()
	{
		spriteRenderer.sprite = dropWepSprite;
		ParticleSystem.MainModule main = particleSys.main;
		main.startColor = wepColor;
	}

	public void MakeHealth()
	{
		spriteRenderer.sprite = dropHealthSprite;
		ParticleSystem.MainModule main = particleSys.main;
		main.startColor = healthColor;
	}
}
