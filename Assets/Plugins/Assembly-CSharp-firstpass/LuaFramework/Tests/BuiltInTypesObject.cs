using System.Collections.Generic;

namespace LuaFramework.Tests
{
	public class BuiltInTypesObject
	{
		public bool myBool { get; set; }

		public int myInt { get; set; }

		public float myFloat { get; set; }

		public string myString { get; set; }

		public byte myByte { get; set; }

		public decimal myDecimal { get; set; }

		public int[] myIntArray { get; set; }

		public Dictionary<string, List<int[]>> myDictionaryListArray { get; set; }
	}
}
