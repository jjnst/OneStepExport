using System.Collections;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class Artifact : Item
{
	public ListCard listCard;

	public ArtifactObject artObj;

	protected override IEnumerator EffectRoutine(DynValue result)
	{
		while ((bool)base.gameObject)
		{
			try
			{
				result.Coroutine.Resume(artObj);
			}
			catch (ScriptRuntimeException e)
			{
				Debug.LogError(e.DecoratedMessage);
			}
			if (result.Coroutine.State == CoroutineState.Dead)
			{
				break;
			}
			yield return null;
		}
	}
}
