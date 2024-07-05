using System.Collections;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class Pact : Item
{
	public SquareListCard listCard;

	public PactObject pactObj;

	protected override IEnumerator EffectRoutine(DynValue result)
	{
		while ((bool)base.gameObject)
		{
			try
			{
				result.Coroutine.Resume(pactObj);
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
