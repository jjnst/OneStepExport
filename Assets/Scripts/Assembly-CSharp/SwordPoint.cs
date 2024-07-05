using System;
using UnityEngine;

public class SwordPoint : MonoBehaviour
{
	public bool wavering = true;

	public Vector3 baseLocalPos;

	private float currentLerpTime = 0f;

	private float waverTime = 1.3f;

	private Vector3 targetPos;

	private Vector3 originalPos;

	public float delay;

	private float delayTimer = 0f;

	private void Awake()
	{
		baseLocalPos = base.transform.localPosition;
		SetTargetPos();
	}

	private void Update()
	{
		if (delay > delayTimer)
		{
			delayTimer += Time.deltaTime;
		}
		else
		{
			if (!wavering)
			{
				return;
			}
			if (base.transform.localPosition != targetPos)
			{
				currentLerpTime += Time.deltaTime;
				if (currentLerpTime > waverTime)
				{
					currentLerpTime = waverTime;
				}
				float num = currentLerpTime / waverTime;
				num = Mathf.Sin(num * (float)Math.PI * 0.5f);
				base.transform.localPosition = Vector3.Lerp(originalPos, targetPos, num);
			}
			else
			{
				SetTargetPos();
				delayTimer = 0f;
				delay = UnityEngine.Random.Range(0.04f, 0.1f);
			}
		}
	}

	public void SetTargetPos()
	{
		currentLerpTime = 0f;
		originalPos = base.transform.localPosition;
		if (base.transform.localPosition.y >= baseLocalPos.y)
		{
			targetPos = baseLocalPos + new Vector3(0f, -1f, 0f);
		}
		else
		{
			targetPos = baseLocalPos + new Vector3(0f, 1f, 0f);
		}
	}
}
