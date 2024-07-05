using System;

[Serializable]
public class StatusStack
{
	public float amount;

	public float duration;

	public float maxDuration;

	public ItemObject source;

	public StatusStack(float amt, float dur, float maxDur, ItemObject itemSource)
	{
		amount = amt;
		duration = dur;
		maxDuration = maxDur;
		source = itemSource;
	}
}
