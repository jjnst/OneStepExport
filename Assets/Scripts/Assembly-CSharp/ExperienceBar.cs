using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBar : MonoBehaviour
{
	public Image expBarFill;

	public TMP_Text levelText;

	public TMP_Text experienceText;

	public TMP_Text lowerLevelText;

	public TMP_Text higherLevelText;

	public TMP_Text experiencePortionText;

	public Animator anim;

	public float targetFill;

	private float fillVelocity = 0f;

	private void Start()
	{
	}

	private void Update()
	{
		if (expBarFill.fillAmount != targetFill)
		{
			expBarFill.fillAmount = Mathf.SmoothDamp(expBarFill.fillAmount, targetFill, ref fillVelocity, 0.1f, 999f, Time.deltaTime);
		}
	}

	public void UpdateBar(float fillAmount)
	{
		targetFill = fillAmount;
	}

	public void SetTargetToMax()
	{
		targetFill = 1f;
	}

	public void EmptyBar()
	{
		expBarFill.fillAmount = 0f;
		targetFill = 0f;
	}
}
