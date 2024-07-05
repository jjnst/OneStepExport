using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Overlay : MonoBehaviour
{
	public CanvasGroup canvasGroup;

	public Image image;

	public float targetAlpha;

	public float smoothTime = 0.1f;

	private float velocity = 0f;

	private void Start()
	{
	}

	private void Update()
	{
		if (Mathf.Abs(canvasGroup.alpha - targetAlpha) > 0.01f)
		{
			canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, targetAlpha, ref velocity, smoothTime, 999f, Time.unscaledDeltaTime);
		}
	}

	public void Flash(float duration, float speed)
	{
		StartCoroutine(_Flash(duration, speed));
	}

	public IEnumerator _Flash(float duration, float transitionRate)
	{
		smoothTime = transitionRate;
		targetAlpha = 1f;
		yield return new WaitForSeconds(duration);
		targetAlpha = 0f;
	}
}
