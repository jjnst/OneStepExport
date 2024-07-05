using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;

public class EffectActions
{
	private static EffectActions _Instance;

	private Script myLuaScript;

	private List<Script> scripts = new List<Script>();

	public EffectActions(string rawLuaCode)
	{
		UserData.RegisterAssembly();
		UserData.RegisterType<Transform>();
		UserData.RegisterType<Cardtridge>();
		UserData.RegisterType<Vector3>();
		UserData.RegisterType<List<Being>>();
		UserData.RegisterType<List<Cpu>>();
		UserData.RegisterType<Animator>();
		UserData.RegisterType<Quaternion>();
		UserData.RegisterType<Rigidbody2D>();
		UserData.RegisterType<BoxCollider2D>();
		UserData.RegisterType<Time>();
		UserData.RegisterType<State>();
		UserData.RegisterType<Status>();
		UserData.RegisterType<Direction>();
		UserData.RegisterType<FTrigger>();
		UserData.RegisterType<Effect>();
		UserData.RegisterType<ItemType>();
		UserData.RegisterType<GunPointSetting>();
		UserData.RegisterType<TalkBox>();
		UserData.RegisterType<RunCtrl>();
		UserData.RegisterType<DuelDisk>();
		UserData.RegisterType<EffectApp>();
		UserData.RegisterType<Target>();
		UserData.RegisterType<StatusApp>();
		UserData.RegisterType<AmountApp>();
		UserData.RegisterType<DeckScreen>();
		UserData.RegisterType<HeroProfile>();
		UserData.RegisterType<FireLoop>();
		UserData.RegisterType<Player>();
		UserData.RegisterType<List<Tile>>();
		UserData.RegisterType<List<TileApp>>();
		UserData.RegisterType<List<SpellObject>>();
		UserData.RegisterType<List<string>>();
		UserData.RegisterType<ShopCtrl>();
		UserData.RegisterType<Block>();
		UserData.RegisterType<BattleGrid>();
		UserData.RegisterType<CastSlot>();
		UserData.RegisterType<SpellObject>();
		UserData.RegisterType<ArtifactObject>();
		UserData.RegisterType<Spell>();
		UserData.RegisterType<Artifact>();
		UserData.RegisterType<Tile>();
		UserData.RegisterType<Projectile>();
		UserData.RegisterType<ProjectileFactory>();
		UserData.RegisterType<Moveable>();
		UserData.RegisterType<BeingObject>();
		UserData.RegisterType<Enemy>();
		UserData.RegisterType<Structure>();
		UserData.RegisterType<BC>();
		UserData.RegisterType<DeckCtrl>();
		UserData.RegisterType<PostCtrl>();
		UserData.RegisterType<ItemManager>();
		UserData.RegisterType<TileApp>();
		UserData.RegisterType<Health>();
		UserData.RegisterType<StatsScreen>();
		UserData.RegisterType<TI>();
		UserData.RegisterType<Being>();
		_Instance = this;
		myLuaScript = new Script();
		myLuaScript.Options.DebugPrint = delegate(string s)
		{
			Debug.Log(s);
		};
		myLuaScript.Options.ScriptLoader = new FileSystemScriptLoader();
		myLuaScript.DoFile(rawLuaCode);
		_Instance.myLuaScript.Globals["State"] = UserData.CreateStatic<State>();
		_Instance.myLuaScript.Globals["Status"] = UserData.CreateStatic<Status>();
		_Instance.myLuaScript.Globals["FireLoop"] = UserData.CreateStatic<FireLoop>();
		_Instance.myLuaScript.Globals["FTrigger"] = UserData.CreateStatic<FTrigger>();
		_Instance.myLuaScript.Globals["Effect"] = UserData.CreateStatic<Effect>();
		_Instance.myLuaScript.Globals["Direction"] = UserData.CreateStatic<Direction>();
		_Instance.myLuaScript.Globals["ItemType"] = UserData.CreateStatic<ItemType>();
		_Instance.myLuaScript.Globals["Target"] = UserData.CreateStatic<Target>();
		_Instance.myLuaScript.Globals["Block"] = UserData.CreateStatic<Block>();
		_Instance.myLuaScript.Globals["GunPointSetting"] = UserData.CreateStatic<GunPointSetting>();
		_Instance.myLuaScript.Globals["BC"] = UserData.CreateStatic<BC>();
	}

	public void AddScript(string scriptPath)
	{
		myLuaScript.DoFile(scriptPath);
	}

	public static DynValue ReturnDyn(string fnName)
	{
		return DynValue.NewClosure((Closure)_Instance.myLuaScript.Globals[fnName]);
	}

	public static void CallFunctionsWithItem(List<string> functionNames, ItemObject itemObj)
	{
		foreach (string functionName in functionNames)
		{
			CallFunctionWithItem(functionName, itemObj);
		}
	}

	public static void CallFunctionWithItem(string fn, ItemObject itemObj)
	{
		object obj = _Instance.myLuaScript.Globals[fn];
		if (obj == null)
		{
			if (fn != "")
			{
				Debug.LogError("'" + fn + "' is not a LUA Function.");
			}
		}
		else
		{
			itemObj.item.StartEffectRoutine(_Instance.myLuaScript.CreateCoroutine(obj));
		}
	}
}
