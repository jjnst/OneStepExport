using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AchievementButton : NavButton
{
	public TMP_Text nameText;

	public TMP_Text descriptionText;

	public TMP_Text counterText;

	public Image icon;

	public Image progressBar;

	public bool unlocked;

	private bool mouseFocus = false;

	public List<AchievementButton> parentList;

	public Scrollbar achievementScrollbar;

	private bool set = false;

	private AchievementData achievementData;

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (IsHoverable())
		{
			hovering = true;
			mouseFocus = true;
			btnCtrl.SetFocus(this);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (btnCtrl.mouseActive)
		{
			mouseFocus = false;
			base.OnPointerExit(eventData);
		}
	}

	public void Set(AchievementData achData, List<AchievementButton> newParentList, Scrollbar newScrollbar)
	{
		achievementData = achData;
		parentList = newParentList;
		achievementScrollbar = newScrollbar;
		nameText.text = LocalizationManager.GetTranslation("AchievementNames/" + achData.AchievementName);
		if (achData.Hidden && !AchievementsCtrl.IsUnlocked(achievementData.AchievementName))
		{
			descriptionText.text = ScriptLocalization.UI.Library_LockedName;
		}
		else
		{
			descriptionText.text = LocalizationManager.GetTranslation("AchievementDescriptions/" + achData.AchievementName);
		}
		counterText.text = string.Empty;
		if ((bool)achData.AchievementImage)
		{
			icon.sprite = Sprite.Create(achData.AchievementImage, new Rect(0f, 0f, achData.AchievementImage.width, achData.AchievementImage.height), new Vector2(0.5f, 0.5f), 100f);
		}
		icon.material = Object.Instantiate(icon.material);
		set = true;
		CheckUnlocked();
	}

	public void CheckUnlocked()
	{
		if (AchievementsCtrl.IsUnlocked(achievementData.AchievementName))
		{
			nameText.color = Color.white;
			icon.material.SetFloat("_GrayScale_Fade_1", 0f);
			icon.color = Color.white;
			progressBar.color = Color.white;
		}
		else
		{
			nameText.color = Color.grey;
			icon.material.SetFloat("_GrayScale_Fade_1", 1f);
			icon.color = Color.grey;
			progressBar.color = Color.grey;
		}
	}

	public override void Focus(int playerNum = 0)
	{
		base.Focus(playerNum);
		if (!mouseFocus)
		{
			float value = 1f - (float)parentList.IndexOf(this) * 1f / (float)Mathf.Clamp(parentList.Count - 1, 1, parentList.Count - 1);
			achievementScrollbar.value = value;
		}
	}

	private void OnDestroy()
	{
		if (set)
		{
			Object.Destroy(icon.material);
		}
	}
}
