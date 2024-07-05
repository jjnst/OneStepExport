using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace LuaFramework.Tests
{
	public class LuaFrameworkDemo : MonoBehaviour
	{
		public Text console;

		public Text consoleRight;

		public List<BuiltInTypesObject> builtInTypesObjects = new List<BuiltInTypesObject>();

		public Dictionary<string, Enemy> enemies = new Dictionary<string, Enemy>();

		public List<UnityTypesObject> unityTypesObjects = new List<UnityTypesObject>();

		private void Start()
		{
			Script script = new Script();
			script.Globals["defineBuiltInTypesObject"] = new Action<DynValue>(DefineBuiltInTypesObject);
			script.Globals["defineUnityTypesObject"] = new Action<DynValue>(DefineUnityTypesObject);
			script.Globals["defineEnemies"] = new Action<DynValue>(DefineEnemies);
			TextAsset textAsset = Resources.Load<TextAsset>("tests.lua");
			script.DoString(textAsset.text);
			console.text = "#### CLR objects read from Lua file: ####\n\n";
			foreach (BuiltInTypesObject builtInTypesObject in builtInTypesObjects)
			{
				Text text = console;
				text.text = text.text + builtInTypesObject.Debug() + "\n";
			}
			foreach (UnityTypesObject unityTypesObject in unityTypesObjects)
			{
				Text text2 = console;
				text2.text = text2.text + unityTypesObject.Debug() + "\n";
			}
			foreach (KeyValuePair<string, Enemy> enemy in enemies)
			{
				Text text3 = console;
				text3.text = text3.text + "Enemy: " + enemy.Key + "\n" + enemy.Value.Debug() + "\n";
			}
			string text4;
			using (LuaWriter luaWriter = new LuaWriter())
			{
				luaWriter.Write("defineEnemies");
				luaWriter.WriteObject(enemies);
				text4 = luaWriter.ToString();
			}
			consoleRight.text = "#### Lua script generated from the above CLR object: ####\n\n" + text4;
			enemies = new Dictionary<string, Enemy>();
			script.DoString(text4);
			consoleRight.text += "\n\n#### CLR object generated again from the above script, should be equal to the one on the left: ####\n\n";
			foreach (KeyValuePair<string, Enemy> enemy2 in enemies)
			{
				Text text3 = consoleRight;
				text3.text = text3.text + "Enemy: " + enemy2.Key + "\n" + enemy2.Value.Debug() + "\n";
			}
			LuaReader.AddCustomReader(typeof(Attack), delegate(DynValue value)
			{
				Table table = value.Table;
				return new Attack
				{
					power = LuaReader.Read<int>(table.Get(1)),
					cooldown = LuaReader.Read<float>(table.Get(2))
				};
			});
			script.DoString("attack = {5, 3.2}");
			Attack attack = LuaReader.Read<Attack>(script.Globals.Get("attack"));
			Text text5 = consoleRight;
			text5.text = text5.text + "\n\n#### CLR object generated with custom reader: ####\n\nLua:\nattack = {5, 3.2}\n\nResult:\n" + attack.Debug();
		}

		public void DefineBuiltInTypesObject(DynValue luaTable)
		{
			BuiltInTypesObject item = LuaReader.Read<BuiltInTypesObject>(luaTable);
			builtInTypesObjects.Add(item);
		}

		public void DefineUnityTypesObject(DynValue luaTable)
		{
			UnityTypesObject item = LuaReader.Read<UnityTypesObject>(luaTable);
			unityTypesObjects.Add(item);
		}

		public void DefineEnemies(DynValue luaTable)
		{
			enemies = LuaReader.Read<Dictionary<string, Enemy>>(luaTable);
		}
	}
}
