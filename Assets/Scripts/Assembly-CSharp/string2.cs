using System;

[Serializable]
public struct string2
{
	public string trigger;

	public string effect;

	public string2(string xString, string yString)
	{
		trigger = xString;
		effect = yString;
	}
}
