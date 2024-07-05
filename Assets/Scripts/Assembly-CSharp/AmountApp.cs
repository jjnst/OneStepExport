using System;
using System.Globalization;
using MoonSharp.Interpreter;
using UnityEngine;

[Serializable]
[MoonSharpUserData]
public class AmountApp
{
	public AmountType type;

	public int initial;

	public float multiplier;

	public float min;

	public float max;

	public AmountApp()
	{
		type = AmountType.Normal;
		initial = 0;
		multiplier = 1f;
		min = -999999f;
		max = 999999f;
	}

	public AmountApp(ref float amountVar, string amountString = "")
	{
		string text = amountString;
		initial = 0;
		multiplier = 1f;
		min = -999999f;
		max = 999999f;
		type = AmountType.Normal;
		string[] array = null;
		if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out amountVar) || string.IsNullOrEmpty(text))
		{
			return;
		}
		if (text.Contains("/"))
		{
			array = text.Split('/');
			float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out max);
			text = array[0];
			if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out amountVar))
			{
				return;
			}
		}
		if (text.Contains("|"))
		{
			array = text.Split('|');
			float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out min);
			text = array[0];
			if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out amountVar))
			{
				return;
			}
		}
		if (text.Contains("*"))
		{
			array = amountString.Split('*');
			text = array[0];
			float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out multiplier);
		}
		if (text.Contains("+"))
		{
			array = text.Split('+');
			int.TryParse(array[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out initial);
			text = array[1];
		}
		if (Enum.IsDefined(typeof(AmountType), text))
		{
			type = (AmountType)Enum.Parse(typeof(AmountType), text);
		}
		else
		{
			Debug.LogWarning("NO AMOUNT TYPE OF " + text);
		}
	}
}
