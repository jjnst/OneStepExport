using System.Collections.Generic;

namespace LuaFramework.Tests
{
	public class Enemy
	{
		public int health { get; set; }

		public bool flying { get; set; }

		public Dictionary<string, Attack> attacks { get; set; }
	}
}
