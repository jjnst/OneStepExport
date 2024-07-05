using System;
using System.Collections.Generic;
using MEC;
using MoonSharp.Interpreter;
using SonicBloom.Koreo;
using UnityEngine;

[MoonSharpUserData]
public class Projectile : MonoBehaviour
{
	public int damage = 0;

	public bool destroyOnHit = false;

	public Being target;

	public List<StatusApp> statusList = new List<StatusApp>();

	public Being being;

	public int alignNum = 0;

	public bool onHitShake = true;

	public bool onHitTriggerArts = true;

	public bool onHitSpell = true;

	public bool onTouchTile = true;

	public bool pierceShield = false;

	public bool pierceDefense = false;

	private int highestX;

	private int lowestX;

	public Moveable mov;

	public Animator anim;

	public SpriteAnimator sprAnim;

	public float lerpDelay = 1f;

	public SpriteRenderer spriteRenderer;

	public AudioSource audioSource;

	public string hitSound;

	public string hitAnim;

	public bool flame = false;

	public bool arcing = false;

	public LineTracer lineTracer;

	public BoxCollider2D col;

	public Rigidbody2D rBody;

	public SpellObject spellObj;

	public Spell spell;

	public GameObject warningTimer;

	public BattleGrid battleGrid;

	public BC ctrl;

	private float despawnTime = 0f;

	private void Update()
	{
		if (despawnTime != 0f && despawnTime < Time.time)
		{
			SimplePool.Despawn(this);
		}
	}

	private void OnEnable()
	{
		mov.offset = Vector2.zero;
		spell = null;
		col.offset = Vector2.zero;
		Timing.RunCoroutine(Loop().CancelWith(base.gameObject));
	}

	private void OnDisable()
	{
		battleGrid.currentProjectiles.Remove(this);
		if ((bool)warningTimer)
		{
			SimplePool.Despawn(warningTimer);
			warningTimer = null;
		}
		StopAllCoroutines();
		if ((bool)being)
		{
			being.activeProjectiles.Remove(this);
		}
		anim.runtimeAnimatorController = null;
		col.size = new Vector2(2f, 2f);
		col.offset = Vector2.zero;
		col.enabled = true;
		rBody.velocity = Vector2.zero;
		mov.lerpTime = 0.1f;
		mov.offset = Vector2.zero;
		hitSound = null;
		hitAnim = null;
		flame = false;
		arcing = false;
		damage = 0;
		spriteRenderer.sprite = null;
		rBody.velocity = Vector3.zero;
		anim.enabled = true;
		sprAnim.enabled = true;
		onHitShake = true;
		onHitTriggerArts = true;
		onHitSpell = true;
		onTouchTile = true;
		spell = null;
		spriteRenderer.sprite = null;
		target = null;
		mov.state = State.Idle;
		mov.hovering = false;
		mov.movement = MovPattern.None;
		spriteRenderer.color = Color.white;
		statusList.Clear();
		base.transform.localScale = Vector3.one;
		if ((bool)lineTracer)
		{
			SimplePool.Despawn(lineTracer.gameObject);
			lineTracer = null;
		}
	}

	public void StartLoop(Being playerScript)
	{
		Timing.RunCoroutine(Loop().CancelWith(base.gameObject));
	}

	public void SetToGunPoint(GunPointSetting gunPointSettingOverride = GunPointSetting.Blank)
	{
		GunPointSetting gunPointSetting = spellObj.gunPointSetting;
		if (gunPointSettingOverride != 0)
		{
			gunPointSetting = gunPointSettingOverride;
		}
		if (gunPointSetting == GunPointSetting.Horizontal)
		{
			mov.offset = new Vector3(being.gunPoint.localPosition.x * (float)being.FacingDirection(), 0f, 0f);
		}
		if (gunPointSetting == GunPointSetting.Vertical)
		{
			mov.offset = new Vector3(0f, being.gunPoint.localPosition.y, 0f);
			col.offset += new Vector2(0f, 0f - being.gunPoint.localPosition.y);
		}
		if (gunPointSetting == GunPointSetting.Both)
		{
			mov.offset = new Vector3(being.gunPoint.localPosition.x * (float)being.FacingDirection(), being.gunPoint.localPosition.y, 0f);
			col.offset += new Vector2(0f, 0f - being.gunPoint.localPosition.y);
		}
		base.transform.position = base.transform.position + mov.offset;
	}

	public void CreateLineTracer(string color)
	{
		lineTracer = SimplePool.Spawn(ctrl.lineTracerPrefab, base.transform.position, base.transform.rotation, base.transform).GetComponent<LineTracer>();
		lineTracer.Set(being.gunPoint, base.transform, color);
	}

	public void SetVelocity(float shotVelocity, Direction direction)
	{
		float x = 0f;
		float y = 0f;
		float num = 0.625f;
		float num2 = 1.18f;
		num2 = 1f;
		switch (direction)
		{
		case Direction.Up:
			y = 1f;
			shotVelocity *= num;
			break;
		case Direction.Right:
			x = 1f;
			break;
		case Direction.Down:
			y = -1f;
			shotVelocity *= num;
			break;
		case Direction.Left:
			x = -1f;
			break;
		case Direction.UpRight:
			x = 1f;
			y = num;
			shotVelocity *= num2;
			break;
		case Direction.UpLeft:
			x = -1f;
			y = num;
			shotVelocity *= num2;
			break;
		case Direction.DownRight:
			x = 1f;
			y = 0f - num;
			shotVelocity *= num2;
			break;
		case Direction.DownLeft:
			x = -1f;
			y = 0f - num;
			shotVelocity *= num2;
			break;
		case Direction.Forward:
			x = being.FacingDirection();
			break;
		case Direction.DownForward:
			x = being.FacingDirection();
			y = 0f - num;
			shotVelocity *= num2;
			break;
		case Direction.UpForward:
			x = being.FacingDirection();
			y = num;
			shotVelocity *= num2;
			break;
		case Direction.Backward:
			x = -being.FacingDirection();
			break;
		case Direction.DownBackward:
			x = -being.FacingDirection();
			y = 0f - num;
			shotVelocity *= num2;
			break;
		case Direction.UpBackward:
			x = -being.FacingDirection();
			y = num;
			shotVelocity *= num2;
			break;
		}
		rBody.velocity = base.transform.TransformDirection(new Vector3(x, y, 0f) * shotVelocity);
	}

	public void SetY(float yChange)
	{
		yChange = UnityEngine.Random.Range(0f - yChange, yChange);
		base.transform.position += new Vector3(0f, yChange, 0f);
		col.offset += new Vector2(0f, 0f - yChange);
	}

	public IEnumerator<float> Loop()
	{
		yield return float.NegativeInfinity;
		if (mov.movement == MovPattern.None)
		{
			yield break;
		}
		while ((bool)base.gameObject)
		{
			mov.Move(mov.movement);
			yield return Timing.WaitForSeconds(lerpDelay);
			yield return Timing.WaitUntilTrue(() => mov.state == State.Idle);
		}
	}

	public void Arc(Vector3 startPos, Vector3 endPos, int bendingY, float timeToTravel)
	{
		if (timeToTravel > 0f)
		{
			arcing = true;
			base.transform.localScale = new Vector3(-1f, 1f, 1f);
			switch (spellObj.arcType)
			{
			case ArcType.Normal:
				Timing.RunCoroutine(_ArcY(startPos, endPos, bendingY, timeToTravel).CancelWith(base.gameObject));
				break;
			case ArcType.Wild:
				Timing.RunCoroutine(_Wild(startPos, endPos, bendingY, timeToTravel, false).CancelWith(base.gameObject));
				break;
			case ArcType.WildBi:
				Timing.RunCoroutine(_Wild(startPos, endPos, bendingY, timeToTravel, true).CancelWith(base.gameObject));
				break;
			case ArcType.CastSlot:
				Timing.RunCoroutine(_CastSlot(startPos, endPos, bendingY, timeToTravel).CancelWith(base.gameObject));
				break;
			}
		}
	}

	private IEnumerator<float> _ArcY(Vector3 startPos, Vector3 endPos, int bendingY, float timeToTravel)
	{
		float timeStamp = Time.time;
		int i = 0;
		Vector3 zero = Vector3.zero;
		while (Time.time < timeStamp + timeToTravel)
		{
			if ((double)Time.timeScale < 0.1 && timeStamp != Time.time)
			{
				yield return float.NegativeInfinity;
				continue;
			}
			Vector3 currentPos;
			if (i == 0)
			{
				i++;
				currentPos = Vector3.Lerp(startPos, endPos, (Time.time + Time.deltaTime - timeStamp) / timeToTravel);
				currentPos.y += (float)bendingY * Mathf.Sin(Mathf.Clamp01((Time.time + Time.deltaTime - timeStamp) / timeToTravel) * (float)Math.PI);
				base.transform.right = startPos - currentPos;
			}
			else
			{
				currentPos = Vector3.Lerp(startPos, endPos, (Time.time - timeStamp) / timeToTravel);
				currentPos.y += (float)bendingY * Mathf.Sin(Mathf.Clamp01((Time.time - timeStamp) / timeToTravel) * (float)Math.PI);
				base.transform.right = base.transform.position - currentPos;
			}
			base.transform.position = currentPos;
			yield return float.NegativeInfinity;
		}
	}

	private IEnumerator<float> _Wild(Vector3 startPos, Vector3 endPos, int bending, float timeToTravel, bool sinX)
	{
		float currentLerpTime = 0f;
		float sinMax = bending;
		float frequency2 = 4f;
		float freqVariance = 3f;
		float u = Time.deltaTime;
		Vector2 yVariance = new Vector2(-1.5f, 1.5f);
		float endYVariance = 0f;
		Vector2 xVariance = new Vector2(-1.5f, 1.5f);
		float endXVariance = 0f;
		float endYOffset = 0f;
		float endXOffset = 0f;
		float yVarFactor = UnityEngine.Random.Range(yVariance.x, yVariance.y);
		float endYVarFactor = UnityEngine.Random.Range(0f - endYVariance, endYVariance);
		float xVarFactor = UnityEngine.Random.Range(xVariance.x, xVariance.y);
		float endXVarFactor = UnityEngine.Random.Range(0f - endXVariance, endXVariance);
		frequency2 += UnityEngine.Random.Range(0f - freqVariance, freqVariance);
		float timeStamp = Time.time;
		while (Time.time < timeStamp + timeToTravel)
		{
			if ((double)Time.timeScale < 0.1 && timeStamp != Time.time)
			{
				yield return float.NegativeInfinity;
				continue;
			}
			currentLerpTime += Time.deltaTime;
			if (currentLerpTime > timeToTravel)
			{
				currentLerpTime = timeToTravel;
			}
			float t = currentLerpTime / timeToTravel;
			Mathf.Sin(t * (float)Math.PI * 0.5f);
			float easeIn2 = currentLerpTime / timeToTravel;
			easeIn2 = 1f - Mathf.Cos(easeIn2 * (float)Math.PI * 0.5f);
			float sinSize = Mathf.Lerp(sinMax, 0f, easeIn2);
			float x;
			if (sinX)
			{
				float xPositer = Mathf.Lerp(startPos.x, endPos.x, easeIn2);
				x = Mathf.Sin(u * frequency2) * sinSize * xVarFactor + xPositer + endXOffset + endXVarFactor;
			}
			else
			{
				x = Mathf.Lerp(startPos.x, endPos.x, easeIn2);
			}
			float yPositer = Mathf.Lerp(startPos.y, endPos.y, easeIn2);
			float y = Mathf.Sin(u * frequency2) * sinSize * yVarFactor + yPositer + endYOffset + endYVarFactor;
			u += Time.deltaTime;
			base.transform.right = base.transform.position - new Vector3(x, y, base.transform.position.z);
			base.transform.position = new Vector3(x, y, base.transform.position.z);
			yield return float.NegativeInfinity;
		}
	}

	private IEnumerator<float> _CastSlot(Vector3 startPos, Vector3 endPos, int bendingY, float timeToTravel)
	{
		endPos = ctrl.currentPlayer.duelDisk.castSlots[0].transform.position;
		float timeStamp = Time.time;
		while (Time.time < timeStamp + timeToTravel)
		{
			if ((double)Time.timeScale < 0.1 && timeStamp != Time.time)
			{
				yield return float.NegativeInfinity;
				continue;
			}
			Vector3 currentPos = Vector3.Lerp(startPos, endPos, (Time.time - timeStamp) / timeToTravel);
			currentPos.x += 0f * Mathf.Sin(Mathf.Clamp01((Time.time - timeStamp) / timeToTravel) * (float)Math.PI);
			currentPos.y += (float)bendingY * Mathf.Sin(Mathf.Clamp01((Time.time - timeStamp) / timeToTravel) * (float)Math.PI);
			currentPos.z += 0f * Mathf.Sin(Mathf.Clamp01((Time.time - timeStamp) / timeToTravel) * (float)Math.PI);
			base.transform.right = base.transform.position - currentPos;
			base.transform.position = currentPos - Vector3.forward * 0.1f;
			base.transform.localScale = Vector3.one * 3f * (Time.time - timeStamp) / timeToTravel;
			yield return float.NegativeInfinity;
		}
	}

	public virtual IEnumerator<float> PositionCheck()
	{
		yield return Timing.WaitForSeconds(1f);
	}

	public void Despawn(float lifeTime = 0f)
	{
		if (lifeTime == 0f)
		{
			SimplePool.Despawn(base.gameObject);
		}
		else
		{
			despawnTime = Time.time + lifeTime;
		}
	}

	public virtual void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Tile") && onTouchTile && (bool)spell)
		{
			spell.spellObj.TouchTile(other.gameObject.GetComponent<Tile>());
		}
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

	public void StartWarningTimer(float duration, bool danger)
	{
		if (base.isActiveAndEnabled)
		{
			SpriteRenderer component = SimplePool.Spawn(ctrl.warningTimerPrefab, base.transform.position, base.transform.rotation, ctrl.transform).GetComponent<SpriteRenderer>();
			if (danger)
			{
				component.color = Color.red;
			}
			else
			{
				component.color = Color.green;
			}
			component.transform.localScale = Vector3.one * 1f;
			Timing.RunCoroutine(ScaleTimer(component, duration).CancelWith(base.gameObject));
			warningTimer = component.gameObject;
			SimplePool.Despawn(component.gameObject, duration);
		}
	}

	private IEnumerator<float> ScaleTimer(SpriteRenderer warningTimer, float duration)
	{
		float maxDuration = duration;
		Color originalColor = warningTimer.color;
		while (duration > 0f)
		{
			warningTimer.transform.localScale = Vector3.one * 0.9f + Vector3.one * 1f * duration / maxDuration;
			float t2 = duration / maxDuration;
			t2 *= t2;
			warningTimer.color = Color.Lerp(originalColor, Color.clear, t2);
			yield return float.NegativeInfinity;
			duration -= Time.deltaTime;
		}
		base.transform.parent = ctrl.transform;
		warningTimer.transform.localScale = Vector3.zero;
	}

	public void StartRhythmTimer(float duration, bool danger, Korevent evt, Koreography koreo, BossViolette boss)
	{
		if (base.isActiveAndEnabled)
		{
			SpriteRenderer component = SimplePool.Spawn(ctrl.warningTimerPrefab, base.transform.position, base.transform.rotation, ctrl.transform).GetComponent<SpriteRenderer>();
			if (danger)
			{
				component.color = Color.red;
			}
			else
			{
				component.color = Color.green;
			}
			component.transform.localScale = Vector3.one * 1f;
			Timing.RunCoroutine(ScaleRhythmTimer(component, duration, evt, koreo, boss).CancelWith(base.gameObject));
			warningTimer = component.gameObject;
		}
	}

	private IEnumerator<float> ScaleRhythmTimer(SpriteRenderer warningTimer, float duration, Korevent evt, Koreography koreo, BossViolette boss)
	{
		Color originalColor = warningTimer.color;
		float distanceFromFinalSample = 0f;
		while (distanceFromFinalSample >= 0f)
		{
			float samplesPerUnit = (float)boss.SampleRate * evt.warningTime;
			distanceFromFinalSample = (float)(-1 * (boss.DelayedSampleTime - evt.StartSample)) / samplesPerUnit;
			warningTimer.transform.localScale = Vector3.one * 0.9f + Vector3.one * distanceFromFinalSample * boss.noteScale;
			warningTimer.color = Color32.Lerp(originalColor, Color.clear, distanceFromFinalSample * distanceFromFinalSample);
			yield return float.NegativeInfinity;
		}
		Despawn();
		warningTimer.transform.localScale = Vector3.zero;
		base.transform.parent = ctrl.transform;
	}

	public bool IsReference()
	{
		return ctrl.ti.refBattleGrid != null && battleGrid == ctrl.ti.refBattleGrid;
	}
}
