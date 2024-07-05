using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[MoonSharpUserData]
public class FloatingText : MonoBehaviour
{
	public float animDelay;

	private float startTime;

	public FloatingTextContainer container;

	public TMP_Text textBox;

	public Image image;

	public TextType triggerName;

	public Animator anim;

	private BC ctrl;

	private void Start()
	{
		startTime = Time.time;
	}

	private void Update()
	{
		if (animDelay < Time.time - startTime)
		{
			anim.SetTrigger(triggerName.ToString());
		}
	}

	public FloatingText Set(FloatingTextContainer parentFollow, BC batCtrl, string name, string text = "none", Sprite sprite = null)
	{
		ctrl = batCtrl;
		anim.SetTrigger("show");
		base.transform.SetParent(parentFollow.transform, false);
		base.transform.localPosition = Vector3.zero;
		base.transform.name = name;
		textBox.text = text;
		container = parentFollow;
		if (sprite != null)
		{
			image.sprite = sprite;
		}
		return this;
	}

	public FloatingText SetHealth(float yAddOffset, float waitTime, TextType type)
	{
		container.yOffset = yAddOffset;
		animDelay = waitTime;
		triggerName = type;
		return this;
	}
}
