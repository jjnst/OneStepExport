using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
	public SpriteAnimationClip clip;

	public int currentKeyFrame = 0;

	public SpriteRenderer spriteRend;

	public bool lateUpdate = false;

	private float timer = 0f;

	private void OnEnable()
	{
		ResetAnim();
	}

	private void Awake()
	{
		spriteRend = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		Step();
	}

	private void LateUpdate()
	{
		if (lateUpdate)
		{
			spriteRend.sprite = clip.keyFrameSprites[currentKeyFrame];
		}
	}

	private void Step()
	{
		if (timer >= (clip.keyFramePositions[currentKeyFrame] + 1f) * clip.keyFrameDuration)
		{
			currentKeyFrame++;
			if (currentKeyFrame >= clip.keyFramePositions.Length)
			{
				currentKeyFrame = clip.keyFramePositions.Length - 1;
				if (!(timer > clip.keyFramePositions[currentKeyFrame] * clip.keyFrameDuration) || !clip.loop)
				{
					timer += Time.deltaTime;
					return;
				}
				ResetAnim();
			}
			spriteRend.sprite = clip.keyFrameSprites[currentKeyFrame];
		}
		timer += Time.deltaTime;
	}

	private void ResetAnim()
	{
		currentKeyFrame = 0;
		timer = 0f;
	}

	public void AssignClip(SpriteAnimationClip clipToAssign)
	{
		ResetAnim();
		clip = clipToAssign;
		spriteRend.sprite = clip.keyFrameSprites[currentKeyFrame];
	}
}
