using System;
using UnityEngine;

[Serializable]
public struct SpriteAnimationClip
{
	public float keyFrameDuration;

	public float[] keyFramePositions;

	public Sprite[] keyFrameSprites;

	public bool loop;

	public SpriteAnimationClip(float theKeyFrameLength, float[] theNums, Sprite[] theSprites, bool loop)
	{
		keyFrameDuration = theKeyFrameLength;
		keyFramePositions = theNums;
		keyFrameSprites = theSprites;
		this.loop = loop;
	}
}
