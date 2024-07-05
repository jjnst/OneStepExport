using System;
using MoonSharp.Interpreter;

[Serializable]
[MoonSharpUserData]
public class StatusApp
{
	public Status type;

	public float amount;

	public float duration;

	public FTrigger origin;

	public StatusApp(Status stat, float amt, float dur, FTrigger fTrigger = FTrigger.None)
	{
		type = stat;
		amount = amt;
		duration = dur;
		origin = fTrigger;
	}
}
