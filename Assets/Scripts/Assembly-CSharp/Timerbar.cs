using UnityEngine;
using UnityEngine.UI;

public class Timerbar : MonoBehaviour
{
	public float closeTime = 3f;

	public float closeTimer;

	public bool timedOut = true;

	public bool started = false;

	public Being being;

	public Image image;

	public void Awake()
	{
		base.transform.name = "Timerbar";
		image.fillAmount = 0f;
	}

	public void Set(Being being, Vector3 position, float closeTime)
	{
		this.being = being;
		this.closeTime = closeTime;
		StartTimer();
	}

	public void StartTimer()
	{
		if (!started)
		{
			timedOut = false;
			closeTimer = closeTime + Time.time;
		}
	}

	private void Update()
	{
		if (closeTimer >= Time.time)
		{
			image.fillAmount = 1f - (closeTimer - Time.time) / closeTime;
		}
		else if (!timedOut)
		{
			image.fillAmount = 1f;
			being.StartCoroutine(being.TimeoutC());
			timedOut = true;
		}
	}
}
