using UnityEngine;

public class StopWatch : MonoBehaviour
{
	public float timeInSeconds;

	public float minutes;

	public float seconds;

	public float milliseconds;

	public bool paused = false;

	private void Update()
	{
		if (paused)
		{
			return;
		}
		if (milliseconds >= 60f)
		{
			seconds += 1f;
			if (seconds >= 60f)
			{
				minutes += 1f;
				seconds -= 60f;
			}
			milliseconds -= 60f;
		}
		timeInSeconds += Time.unscaledDeltaTime;
		milliseconds += Time.unscaledDeltaTime * 60f;
	}

	public void Set(float secondsPassed)
	{
		milliseconds = secondsPassed * 60f % 60f;
		seconds = Mathf.FloorToInt(secondsPassed % 60f);
		minutes = Mathf.FloorToInt(secondsPassed / 60f);
		timeInSeconds = secondsPassed;
	}

	public void Reset()
	{
		minutes = 0f;
		seconds = 0f;
		milliseconds = 0f;
		timeInSeconds = 0f;
	}

	public void Pause()
	{
		paused = true;
	}

	public void Resume()
	{
		paused = false;
	}

	public string FormattedTime()
	{
		string text = "";
		if (milliseconds <= 10f)
		{
			text = string.Format("{0}:{1}:0{2}", minutes, seconds, (int)milliseconds);
			if (seconds <= 10f)
			{
				text = string.Format("{0}:0{1}:0{2}", minutes, seconds, (int)milliseconds);
			}
		}
		else
		{
			text = string.Format("{0}:{1}:{2}", minutes, seconds, (int)milliseconds);
			if (seconds <= 10f)
			{
				text = string.Format("{0}:0{1}:{2}", minutes, seconds, (int)milliseconds);
			}
		}
		return text;
	}

	public string Hours()
	{
		string text = "";
		return string.Format("{0}", Mathf.FloorToInt(minutes / 60f));
	}

	public string Minutes()
	{
		string text = "";
		return string.Format("{0}", minutes % 60f);
	}

	public int RoundedTimeInSeconds()
	{
		return Mathf.RoundToInt(timeInSeconds);
	}
}
