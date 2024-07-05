using UnityEngine;
using UnityEngine.UI;

public class SelectorBar : MonoBehaviour
{
	private Image image;

	public RectTransform rect;

	public float fillTarget = 0f;

	public float damping = 0.1f;

	private float currentVelocity;

	private void Awake()
	{
		image = GetComponent<Image>();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (image.fillAmount != fillTarget)
		{
			image.fillAmount = Mathf.SmoothDamp(image.fillAmount, fillTarget, ref currentVelocity, damping, float.PositiveInfinity, Time.unscaledDeltaTime);
		}
	}
}
