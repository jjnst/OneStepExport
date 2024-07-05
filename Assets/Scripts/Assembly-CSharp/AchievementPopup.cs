using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopup : MonoBehaviour
{
	public TMP_Text unlockedText;

	public TMP_Text nameText;

	public Image icon;

	public float displayLength = 5f;

	public float fadeDelay = 1f;

	public CanvasGroup canvasGroup;

	public Animator anim;

	private void Start()
	{
	}

	public void Set(AchievementData achData, int popupGridChildCount)
	{
		unlockedText.text = ScriptLocalization.UI.Achievement_Unlocked;
		nameText.text = LocalizationManager.GetTranslation("AchievementNames/" + achData.AchievementName);
		Debug.Log("setting and showing achievement popup for " + achData.AchievementName + " created popups: " + popupGridChildCount);
		StartCoroutine(_HideAfter((float)(popupGridChildCount - 1) * 1.5f));
	}

	private IEnumerator _HideAfter(float introDelay)
	{
		yield return new WaitForSeconds(introDelay);
		anim.SetBool("visible", true);
		yield return new WaitForSeconds(displayLength);
		anim.SetBool("visible", false);
		yield return new WaitForSeconds(fadeDelay);
		Object.Destroy(base.gameObject);
	}
}
