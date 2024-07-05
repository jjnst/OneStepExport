using System;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public static class Utils
{
	public static string Translate(string key)
	{
		return LocalizationManager.GetTranslation(key);
	}

	public static bool SharesTags(List<Tag> tagsOne, List<Tag> tagsTwo)
	{
		foreach (Tag item in tagsOne)
		{
			if (tagsTwo.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public static List<int> RandomList(int length, bool world = false, bool seeded = true)
	{
		RunCtrl runCtrl = S.I.runCtrl;
		List<int> list = new List<int>();
		for (int i = 0; i < length; i++)
		{
			list.Add(i);
		}
		List<int> list2 = new List<int>();
		for (int j = 0; j < length; j++)
		{
			int num = 0;
			num = ((!world) ? ((runCtrl.pseudoRandom == null || !seeded) ? UnityEngine.Random.Range(0, list.Count) : runCtrl.NextPsuedoRand(0, list.Count)) : runCtrl.NextWorldRand(0, list.Count));
			list2.Add(list[num]);
			list.RemoveAt(num);
		}
		return list2;
	}

	public static bool RandomBool(int maxRandomRange)
	{
		if (UnityEngine.Random.Range(0, maxRandomRange) == 0)
		{
			return true;
		}
		return false;
	}

	public static int GetNextWrappedIndex<T>(IList<T> collection, int currentIndex)
	{
		if (collection.Count < 1)
		{
			return 0;
		}
		return (currentIndex + 1) % collection.Count;
	}

	public static int GetPreviousWrappedIndex<T>(IList<T> collection, int currentIndex)
	{
		if (collection.Count < 1)
		{
			return 0;
		}
		if (currentIndex - 1 < 0)
		{
			return collection.Count - 1;
		}
		return (currentIndex - 1) % collection.Count;
	}

	public static string GetFormattedTime(float timeInSeconds)
	{
		int num = Mathf.RoundToInt(timeInSeconds);
		int num2 = num / 3600;
		int num3 = num % 3600 / 60;
		float num4 = Mathf.Round(timeInSeconds % 3600f % 60f * 100f) / 100f;
		return string.Format("{0}{1} {2}{3} {4}{5}", num2, ScriptLocalization.UI.hours_shorthand, num3, ScriptLocalization.UI.minutes_shorthand, num4, ScriptLocalization.UI.seconds_shorthand);
	}

	public static Brand GetBrand(string brandName)
	{
		if (Enum.IsDefined(typeof(Brand), brandName))
		{
			return (Brand)Enum.Parse(typeof(Brand), brandName);
		}
		Debug.LogError("Invalid Brand: " + brandName);
		return Brand.None;
	}
}
