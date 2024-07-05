using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LuaFramework.Tests
{
	public static class TestsDebug
	{
		public static string Debug(this BuiltInTypesObject o)
		{
			string text = "Built-in types test object:\n";
			text = text + "myBool: " + o.myBool + "\n";
			text = text + "myInt: " + o.myInt + "\n";
			text = text + "myFloat: " + o.myFloat + "\n";
			text = text + "myString: " + o.myString + "\n";
			text = text + "myByte: " + o.myByte + "\n";
			text = text + "myDecimal: " + o.myDecimal + "\n";
			text = text + "myIntArray: " + o.myIntArray.Debug() + "\n";
			if (o.myDictionaryListArray != null)
			{
				text += "myDictionaryListArray:\n";
				foreach (KeyValuePair<string, List<int[]>> item in o.myDictionaryListArray)
				{
					text = text + "  Key: " + item.Key + ", Values List: \n";
					foreach (int[] item2 in item.Value)
					{
						text = text + "    Array: " + item2.Debug() + ",\n";
					}
				}
			}
			return text;
		}

		public static string Debug(this UnityTypesObject o)
		{
			string text = "Unity types test object:\n";
			text = string.Concat(text, "myColor: ", o.myColor, "\n");
			text = string.Concat(text, "myColor32: ", o.myColor32, "\n");
			text = string.Concat(text, "myRect: ", o.myRect, "\n");
			text = string.Concat(text, "myVector2: ", o.myVector2, "\n");
			text = string.Concat(text, "myVector3: ", o.myVector3, "\n");
			text = string.Concat(text, "myVector4: ", o.myVector4, "\n");
			if (o.myVector3Array != null)
			{
				text += "myVector3Array:\n";
				text = o.myVector3Array.Aggregate(text, (string current, Vector3 vector3) => string.Concat(current, "  ", vector3, "\n"));
			}
			if (o.myColorList != null)
			{
				text += "myColorList:\n";
				text = o.myColorList.Aggregate(text, (string current, Color color) => string.Concat(current, "  ", color, "\n"));
			}
			if (o.myRectDictionary != null)
			{
				text += "myRectDictionary:\n";
				text = o.myRectDictionary.Aggregate(text, (string current, KeyValuePair<string, Rect> pair) => string.Concat(current, "  ", pair.Key, ": ", pair.Value, "\n"));
			}
			return text;
		}

		public static string Debug(this Enemy enemy)
		{
			string text = "";
			text = text + "  Health: " + enemy.health + "\n";
			text = text + "  Flying: " + enemy.flying + "\n";
			if (enemy.attacks != null)
			{
				text += "  Attacks:\n";
				text = enemy.attacks.Aggregate(text, (string s1, KeyValuePair<string, Attack> pair) => s1 + "    " + pair.Key + ": " + pair.Value.Debug() + ",\n");
			}
			return text;
		}

		public static string Debug(this Attack attack)
		{
			string text = "{";
			return text + "Power: " + attack.power + ", Cooldown: " + attack.cooldown + "}";
		}

		public static string Debug(this int[] array)
		{
			string text = "{";
			for (int i = 0; i < array.Length; i++)
			{
				text += array[i];
				if (i < array.Length - 1)
				{
					text += ", ";
				}
			}
			return text + "}";
		}
	}
}
