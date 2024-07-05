using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using MoonSharp.Interpreter;
using UnityEngine;

[Serializable]
[MoonSharpUserData]
public class EffectApp
{
	public FTrigger fTrigger;

	public float triggerAmount;

	public float triggerCooldown;

	public Effect effect;

	public Target target;

	public float amount;

	public AmountApp amountApp;

	public TileApp tileApp;

	public float duration;

	public AmountApp durationApp;

	public string value;

	public float chance;

	public float limit;

	public float current;

	public float timer = 0f;

	public List<Check> checks;

	public float checkAmount;

	public AmountApp checkAmountApp;

	public string checkValue;

	public int frameTriggered = 0;

	public EffectApp(FTrigger fTrig, float trigAmt, float trigCD, Effect sEffect, Target ftarget, float amt, float dur, string val, float chance, float limit, List<Check> checks = null, float checkAmount = 1f, AmountApp checkAmtApp = null, string checkValue = "", AmountApp amtApp = null, TileApp tileApp = null, AmountApp durApp = null)
	{
		fTrigger = fTrig;
		triggerAmount = trigAmt;
		triggerCooldown = trigCD;
		effect = sEffect;
		target = ftarget;
		amount = amt;
		amountApp = amtApp;
		this.tileApp = tileApp;
		duration = dur;
		durationApp = durApp;
		value = val;
		this.chance = chance;
		this.limit = limit;
		this.checks = checks;
		this.checkAmount = checkAmount;
		checkAmountApp = checkAmtApp;
		this.checkValue = checkValue;
	}

	public EffectApp Clone()
	{
		return new EffectApp(fTrigger, triggerAmount, triggerCooldown, effect, target, amount, duration, value, chance, limit, checks, checkAmount, checkAmountApp, checkValue, amountApp, tileApp, durationApp);
	}

	public static void AddTo(XmlReader reader, ItemObject itemObj, List<EffectApp> appList, string fTriggerName = null, string sEffectName = null)
	{
		float result = 0f;
		float.TryParse(reader.GetAttribute("triggerAmount"), NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		float result2 = 0f;
		float.TryParse(reader.GetAttribute("triggerCooldown"), NumberStyles.Float, CultureInfo.InvariantCulture, out result2);
		float amountVar = 1f;
		AmountApp amtApp = new AmountApp(ref amountVar, reader.GetAttribute("amount"));
		float amountVar2 = 0f;
		AmountApp durApp = new AmountApp(ref amountVar2, reader.GetAttribute("duration"));
		string val = reader.GetAttribute("value");
		if (string.IsNullOrEmpty(val))
		{
			val = "";
		}
		string attribute = reader.GetAttribute("target");
		Target ftarget = Target.Default;
		if (string.IsNullOrEmpty(attribute))
		{
			attribute = "";
		}
		else
		{
			ftarget = (Target)Enum.Parse(typeof(Target), attribute);
		}
		float result3 = 1f;
		float.TryParse(reader.GetAttribute("chance"), NumberStyles.Float, CultureInfo.InvariantCulture, out result3);
		if (result3 == 0f)
		{
			result3 = 1f;
		}
		float result4 = 0f;
		float.TryParse(reader.GetAttribute("limit"), NumberStyles.Float, CultureInfo.InvariantCulture, out result4);
		string text = fTriggerName;
		if (string.IsNullOrEmpty(fTriggerName))
		{
			text = reader.Name;
		}
		float amountVar3 = 1f;
		AmountApp checkAmtApp = null;
		List<Check> list = null;
		string attribute2 = reader.GetAttribute("check");
		if (!string.IsNullOrEmpty(attribute2))
		{
			list = new List<Check>();
			checkAmtApp = new AmountApp(ref amountVar3, reader.GetAttribute("checkAmount"));
			string[] array = reader.GetAttribute("check").Replace(" ", string.Empty).Split(',');
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (!string.IsNullOrEmpty(text2))
				{
					if (Enum.IsDefined(typeof(Check), text2))
					{
						list.Add((Check)Enum.Parse(typeof(Check), text2));
					}
					else
					{
						Debug.LogError("Invalid Check: " + text2);
					}
				}
			}
		}
		string text3 = reader.GetAttribute("checkValue");
		if (string.IsNullOrEmpty(text3))
		{
			text3 = "";
		}
		TileApp tileApp = null;
		if (reader.GetAttribute("Location") != null)
		{
			tileApp = itemObj.ReadXmlLocation(reader, true);
		}
		if (text == "")
		{
			return;
		}
		FTrigger fTrigger = (FTrigger)Enum.Parse(typeof(FTrigger), text);
		reader.Read();
		string text4 = sEffectName;
		if (string.IsNullOrEmpty(sEffectName))
		{
			text4 = reader.ReadContentAsString();
		}
		if (!(text4 == ""))
		{
			Effect effect = (Effect)Enum.Parse(typeof(Effect), text4);
			if (!itemObj.effectTags.Contains(effect))
			{
				itemObj.effectTags.Add(effect);
			}
			if (!itemObj.triggerTags.Contains(fTrigger))
			{
				itemObj.triggerTags.Add(fTrigger);
			}
			appList.Add(new EffectApp(fTrigger, result, result2, effect, ftarget, amountVar, amountVar2, val, result3, result4, list, amountVar3, checkAmtApp, text3, amtApp, tileApp, durApp));
		}
	}
}
