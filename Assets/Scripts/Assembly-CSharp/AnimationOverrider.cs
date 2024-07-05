using UnityEngine;

public class AnimationOverrider : MonoBehaviour
{
	public SpriteAnimator sprAnim;

	public Animator anim;

	public string controllerName;

	public string currentClipName;

	public ItemManager itemMan;

	private void Start()
	{
	}

	private void Update()
	{
		string text = anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Replace("character", string.Empty);
		if (currentClipName != text || !sprAnim.enabled)
		{
			if (itemMan.spriteAnimClips.ContainsKey(controllerName + text))
			{
				sprAnim.enabled = true;
				sprAnim.AssignClip(itemMan.GetClip(controllerName + text));
			}
			else
			{
				sprAnim.enabled = false;
			}
			currentClipName = text;
		}
	}

	public void Set(SpriteAnimator spriteAnimator, Animator animator, string baseName, ItemManager itemManager)
	{
		sprAnim = spriteAnimator;
		sprAnim.lateUpdate = true;
		anim = animator;
		controllerName = baseName;
		itemMan = itemManager;
	}
}
