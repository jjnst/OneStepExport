using System.Collections.Generic;
using UnityEngine;

public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
	public AnimationClip this[string name]
	{
		get
		{
			return Find((KeyValuePair<AnimationClip, AnimationClip> x) => x.Key.name.Equals(name)).Value;
		}
		set
		{
			int num = FindIndex((KeyValuePair<AnimationClip, AnimationClip> x) => x.Key.name.Equals(name));
			if (num != -1)
			{
				base[num] = new KeyValuePair<AnimationClip, AnimationClip>(base[num].Key, value);
			}
		}
	}

	public AnimationClipOverrides(int capacity)
		: base(capacity)
	{
	}
}
