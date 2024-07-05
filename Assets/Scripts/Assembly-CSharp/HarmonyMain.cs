using System.Reflection;
using HarmonyLib;

public class HarmonyMain
{
	public static void LoadMod(string DLLPath, string modName)
	{
		Harmony harmony = new Harmony("com." + modName + ".patch");
		harmony.PatchAll(Assembly.LoadFrom(DLLPath));
	}
}
