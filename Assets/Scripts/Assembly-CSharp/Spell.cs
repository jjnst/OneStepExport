using System;
using System.Collections;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class Spell : Item
{
	public SpellObject spellObj;

	public RuntimeAnimatorController animMark;

	public string animMarkS;

	public RuntimeAnimatorController animShot;

	public AudioClip castSound;

	public AudioClip shotSound;

	public int numTiles = 1;

	public float bulletSpeed = 1000f;

	public int damage = 10;

	public int numOfShots = 1;

	public float duration = 3f;

	public float warningLength = 0.6f;

	public float timeMirror = 0f;

	public float deltaTimeMirror = 0f;

	[NonSerialized]
	public GameObject projectilePrefab;

	[NonSerialized]
	public Animator beingAnim;

	public float castTime = 0f;

	public virtual void Reset()
	{
		itemType = ItemType.Spell;
	}

	protected virtual void Start()
	{
		projectilePrefab = ctrl.projectilePrefab;
	}

	public void StartCast()
	{
		Debug.Log("This is a Legacy CAST!! Should cast from Spell Obj instead");
	}

	public virtual IEnumerator Cast()
	{
		yield return null;
	}

	public Vector3 GetDir(int i)
	{
		switch (i)
		{
		case 0:
			return Vector3.up;
		case 1:
			return Vector3.left;
		case 2:
			return Vector3.down;
		case 3:
			return Vector3.right;
		default:
			return Vector3.up;
		}
	}

	protected override IEnumerator EffectRoutine(DynValue result)
	{
		while ((bool)base.gameObject)
		{
			try
			{
				result.Coroutine.Resume(spellObj);
			}
			catch (ScriptRuntimeException e)
			{
				Debug.LogError(e.DecoratedMessage);
			}
			if (result.Coroutine.State == CoroutineState.Dead)
			{
				break;
			}
			yield return null;
		}
	}

	public IEnumerator DelayedDoublecast(int doublecastCounter)
	{
		yield return new WaitForSeconds((float)doublecastCounter * 0.2f);
		while (being.mov.state != 0)
		{
			yield return new WaitForSeconds(0.2f);
		}
		spellObj.StartCast(true, doublecastCounter);
	}

	public void CreateShotAfter(SpellObject spellObject, Tile tile, float delay, GunPointSetting gunPointSetting, int pelletIndex = 0, float pelletInterval = 1f)
	{
		StartCoroutine(CreateShotAfterC(spellObject, tile, delay, gunPointSetting, pelletIndex, pelletInterval));
	}

	private IEnumerator CreateShotAfterC(SpellObject spellData, Tile tile, float delay, GunPointSetting gunPointSetting, int pelletIndex, float pelletInterval)
	{
		float timer = 0f;
		while (timer < delay)
		{
			timer = TickTimer(timer);
			yield return null;
		}
		if ((bool)being)
		{
			spellData.P.CreateShot(spellData, tile, false, pelletIndex, pelletInterval).SetToGunPoint(gunPointSetting);
			if (spellData.fireAnimLate)
			{
				being.anim.SetTrigger("fire");
			}
		}
	}

	public void CreateLerperAfter(SpellObject spellObject, Tile tile, float delay, GunPointSetting gunPointSetting)
	{
		StartCoroutine(CreateLerperAfterC(spellObject, tile, delay, gunPointSetting));
	}

	private IEnumerator CreateLerperAfterC(SpellObject spellData, Tile tile, float delay, GunPointSetting gunPointSetting)
	{
		float timer = 0f;
		while (timer < delay)
		{
			timer = TickTimer(timer);
			yield return null;
		}
		spellData.P.CreateLerper(spellData, tile).SetToGunPoint(gunPointSetting);
		yield return null;
	}

	public void CreateObjAfter(SpellObject spellObject, Tile tile, float delay, float duration = 0f, int pelletIndex = 0, float pelletInterval = 1f)
	{
		StartCoroutine(CreateObjAfterC(spellObject, tile, delay, duration, 0f, pelletIndex, pelletInterval));
	}

	private IEnumerator CreateObjAfterC(SpellObject spellData, Tile tile, float delay, float duration, float speed = 0f, int pelletIndex = 0, float pelletInterval = 1f)
	{
		float timer = 0f;
		while (timer < delay)
		{
			timer = TickTimer(timer);
			yield return null;
		}
		spellData.P.CreateAnimObj(spellData, tile, duration, false, speed, pelletIndex, pelletInterval);
		yield return null;
	}

	public void CreateBlastEffectAfter(SpellObject spellObject, Tile tile, float delay, GunPointSetting gunPointSetting)
	{
		StartCoroutine(CreateBlastEffectAfterC(spellObject, tile, delay, gunPointSetting));
	}

	private IEnumerator CreateBlastEffectAfterC(SpellObject spellData, Tile tile, float delay, GunPointSetting gunPointSetting, float speed = 0f)
	{
		float timer = 0f;
		while (timer < delay)
		{
			timer = TickTimer(timer);
			yield return null;
		}
		if ((bool)being)
		{
			Projectile newProj = spellData.P.CreateBlastEffect(spellData, tile, false, speed);
			if ((bool)newProj)
			{
				newProj.SetToGunPoint(gunPointSetting);
			}
		}
		yield return null;
	}

	public void Timestamp(string message)
	{
		Debug.Log(message + " " + Time.time + " frame: " + Time.frameCount);
	}
}
