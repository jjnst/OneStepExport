using System;
using System.Collections;
using UnityEngine;

public class UISlide : MonoBehaviour
{
	public Vector3 offScreenPos;

	public Vector3 onScreenPos;

	private float currentLerpTime;

	private bool onScreen = false;

	public float lerpTime = 0.5f;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void MoveToggle()
	{
		if (onScreen)
		{
			StartCoroutine(Move(onScreenPos, offScreenPos));
		}
		else
		{
			StartCoroutine(Move(offScreenPos, onScreenPos));
		}
		onScreen = !onScreen;
	}

	public void MoveOnScreen()
	{
		onScreen = true;
		StartCoroutine(Move(offScreenPos, onScreenPos));
	}

	public void MoveOffScreen()
	{
		onScreen = false;
		StartCoroutine(Move(onScreenPos, offScreenPos));
	}

	private IEnumerator Move(Vector3 startPos, Vector3 endPos)
	{
		currentLerpTime = 0f;
		while (base.transform.position != endPos)
		{
			currentLerpTime += Time.deltaTime;
			if (currentLerpTime > lerpTime)
			{
				currentLerpTime = lerpTime;
			}
			float t2 = currentLerpTime / lerpTime;
			t2 = Mathf.Sin(t2 * (float)Math.PI * 0.5f);
			base.transform.position = Vector3.Lerp(startPos, endPos, t2);
			yield return new WaitForEndOfFrame();
		}
	}
}
