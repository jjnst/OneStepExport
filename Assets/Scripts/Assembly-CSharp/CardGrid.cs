using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardGrid : MonoBehaviour
{
	public Transform belt;

	public Image shuffleTimer;

	public Transform shuffleSpinner;

	public TMP_Text cardCounter;

	public int maxBeltLength = 0;

	public float lerpTime = 0.2f;

	public void UpdateCounter(int queued, int current)
	{
		cardCounter.text = queued + "/" + current;
	}

	public void MoveTo(Vector3 position, bool castMove = false)
	{
		StartCoroutine(MoveCardGrid(position, castMove));
	}

	private IEnumerator MoveCardGrid(Vector3 position, bool castMove = false)
	{
		Vector3 startPos = base.transform.position;
		float currentLerpTime = 0f;
		while (currentLerpTime < lerpTime)
		{
			yield return new WaitForEndOfFrame();
			currentLerpTime += Time.deltaTime;
			if (currentLerpTime > lerpTime)
			{
				currentLerpTime = lerpTime;
			}
			float t2 = currentLerpTime / lerpTime;
			t2 = Mathf.Sin(t2 * (float)Math.PI * 0.5f);
			base.transform.position = Vector3.Lerp(startPos, position, t2);
			if (castMove)
			{
				RectTransform beltRect = belt.GetComponent<RectTransform>();
				float y = beltRect.sizeDelta.y;
				float beltLength = 360f - (base.transform.localPosition.y + 96f) * 2f;
				if (beltLength > (float)maxBeltLength)
				{
					beltLength = maxBeltLength;
				}
				beltRect.sizeDelta = new Vector2(beltRect.sizeDelta.x, beltLength);
			}
		}
	}
}
