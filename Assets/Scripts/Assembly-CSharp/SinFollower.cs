using System;
using UnityEngine;
using UnityEngine.UI;

public class SinFollower : MonoBehaviour
{
	public float lerpTime = 0.6f;

	protected float currentLerpTime;

	[HideInInspector]
	public float startX;

	public float sinMax = 50f;

	public float baseFrequency = 5f;

	public float frequency = 9f;

	public float freqVariance = 3f;

	public Vector2 yVariance = new Vector2(-0.5f, 1.5f);

	public float endYVariance = 18f;

	public float endYOffset = 0f;

	protected float yVarFactor;

	protected float endYVarFactor;

	protected float u = 0f;

	public SpriteRenderer spriteRenderer;

	public ParticleSystem particleSys;

	public Image image;

	public Transform target;

	protected bool trailing = false;

	public bool reachedTarget = false;

	protected Vector3 startingPosition;

	public float despawnDelay = 0f;

	public BC ctrl;

	protected bool showedAnim = false;

	private bool disabling = false;

	protected float distance = 0f;

	protected virtual void Awake()
	{
		ctrl = S.I.batCtrl;
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		Reset();
		Calculate();
	}

	public void Calculate()
	{
		yVarFactor = UnityEngine.Random.Range(yVariance.x, yVariance.y);
		endYVarFactor = UnityEngine.Random.Range(0f - endYVariance, endYVariance);
		frequency += UnityEngine.Random.Range(0f - freqVariance, freqVariance);
	}

	public virtual void Reset()
	{
		u = 0f;
		startingPosition = base.transform.position;
		startX = base.transform.position.x;
		currentLerpTime = 0f;
		reachedTarget = false;
		trailing = false;
		showedAnim = false;
		target = null;
		disabling = false;
		frequency = baseFrequency;
		if ((bool)particleSys)
		{
			ParticleSystem.MainModule main = particleSys.main;
			main.startSpeed = 10f;
			particleSys.Play();
		}
		if ((bool)image)
		{
			image.enabled = true;
		}
	}

	protected virtual void LateUpdate()
	{
		if (!reachedTarget && (bool)target)
		{
			distance = Vector2.Distance(base.transform.position, target.position);
			if (distance >= 2f)
			{
				Move();
			}
			else
			{
				ReachedTarget();
			}
		}
	}

	protected virtual void ReachedTarget()
	{
		reachedTarget = true;
		DisableSelf();
	}

	public void Move()
	{
		currentLerpTime += Time.deltaTime;
		if (currentLerpTime > lerpTime)
		{
			currentLerpTime = lerpTime;
		}
		float num = currentLerpTime / lerpTime;
		num = Mathf.Sin(num * (float)Math.PI * 0.5f);
		float num2 = currentLerpTime / lerpTime;
		num2 = 1f - Mathf.Cos(num2 * (float)Math.PI * 0.5f);
		float num3 = Mathf.Lerp(sinMax, 0f, num2);
		float x = Mathf.Lerp(startX, target.position.x, num2);
		float num4 = Mathf.Lerp(startingPosition.y, target.position.y, num2);
		float y = Mathf.Sin(u * frequency) * num3 * yVarFactor + num4 + endYOffset + endYVarFactor;
		base.transform.position = new Vector3(x, y, base.transform.position.z);
		u += Time.deltaTime;
	}

	public virtual void DisableSelf()
	{
		if (!disabling)
		{
			disabling = true;
			if ((bool)spriteRenderer)
			{
				spriteRenderer.sprite = null;
			}
			if ((bool)image)
			{
				image.enabled = false;
			}
			if ((bool)particleSys)
			{
				ParticleSystem.MainModule main = particleSys.main;
				main.startSpeed = 40f;
				particleSys.Emit(15);
				particleSys.Stop();
			}
			SimplePool.Despawn(base.gameObject, despawnDelay);
		}
	}
}
