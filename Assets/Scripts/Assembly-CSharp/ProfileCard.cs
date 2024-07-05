using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProfileCard : NavButton
{
	[SerializeField]
	private TMP_Text playtimeText = null;

	[SerializeField]
	private TMP_Text progressText = null;

	public NavTextfield nameInput = null;

	[SerializeField]
	private Image characterIcon = null;

	[SerializeField]
	private Image brandBackground = null;

	public Image selectedImage;

	public ProfileCtrl profCtrl;

	private int index = 0;

	private Color originalFieldColor = Color.black;

	private string originalName = "";

	[SerializeField]
	private CanvasGroup contentContainer = null;

	[SerializeField]
	private TMP_Text emptySlot = null;

	private bool slotIsEmpty = true;

	protected override void Awake()
	{
		base.Awake();
		originalFieldColor = nameInput.inputField.image.color;
		nameInput.inputField.image.color = Color.clear;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (IsHoverable())
		{
			hovering = true;
			btnCtrl.SetFocus(this);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (btnCtrl.mouseActive)
		{
			base.OnPointerExit(eventData);
		}
	}

	public void SelectThisCard()
	{
		if (slotIsEmpty)
		{
			CreateNewProfile();
			return;
		}
		profCtrl.SelectCard(this);
		originalName = nameInput.inputField.text;
	}

	public void UpdateText()
	{
		index = profCtrl.profileCards.IndexOf(this);
		if (SaveDataCtrl.Get("ProfileCreated" + index, false, true))
		{
			SetSlotEmpty(false);
			nameInput.inputField.text = SaveDataCtrl.Get("ProfileName" + index, "Saffron", true);
			if (index == 0)
			{
				playtimeText.text = Utils.GetFormattedTime(SaveDataCtrl.Get("TotalPlaytime", 0, true));
			}
			else
			{
				playtimeText.text = Utils.GetFormattedTime(SaveDataCtrl.Get("TotalPlaytime" + index, 0, true));
			}
			string empty = string.Empty;
			empty = ((index != 0) ? SaveDataCtrl.Get("LastChosenCharacter" + index, "Saffron", true) : SaveDataCtrl.Get("LastChosenCharacter", "Saffron", true));
			if (profCtrl.charIcons.ContainsKey(empty))
			{
				characterIcon.sprite = profCtrl.charIcons[empty];
			}
			Brand brand = Brand.None;
			brand = ((index != 0) ? SaveDataCtrl.Get("LastFocusedBrand" + index, Brand.None, true) : SaveDataCtrl.Get("LastFocusedBrand", Brand.None, true));
			brandBackground.sprite = profCtrl.deCtrl.spellBackgroundBrands[(int)brand];
		}
		else
		{
			playtimeText.text = string.Empty;
			SetSlotEmpty(true);
		}
		progressText.text = string.Empty;
	}

	public void SetName(string nameString)
	{
		SaveDataCtrl.Set("ProfileName" + index, "Saffron", true);
	}

	public void OpenRename()
	{
		nameInput.inputField.image.color = originalFieldColor;
		nameInput.inputField.text = "";
		nameInput.inputField.interactable = true;
		btnCtrl.SetFocus(nameInput);
	}

	public void EndRename()
	{
		nameInput.inputField.image.color = Color.clear;
		SaveDataCtrl.Set("ProfileName" + index, nameInput.inputField.text, true);
		if (nameInput.inputField.text != originalName)
		{
			SaveDataCtrl.Write(true);
		}
		if (btnCtrl.focusedButton != this)
		{
			btnCtrl.SetFocus(this);
		}
		nameInput.inputField.interactable = false;
	}

	public void OpenDelete()
	{
		if (S.I.currentProfile == index)
		{
			StartCoroutine(FlashSelectedRed());
			Debug.Log("Please select another profile first.");
		}
		else
		{
			if (slotIsEmpty)
			{
				return;
			}
			List<string> list = SaveDataCtrl.Get("SavedProfileData" + index, new List<string>());
			foreach (string item in list)
			{
				SaveDataCtrl.Remove(item);
			}
			SaveDataCtrl.Remove("SavedProfileData" + index);
			SaveDataCtrl.Remove("ProfileName" + index);
			SaveDataCtrl.Remove("ProfileCreated" + index);
			SetSlotEmpty(true);
			SaveDataCtrl.Write(true);
			btnCtrl.SetFocus(this);
		}
	}

	private IEnumerator FlashSelectedRed()
	{
		selectedImage.color = Color.red;
		float flashDuration = 1.5f;
		float currentLerpTime = flashDuration;
		while (currentLerpTime > 0f)
		{
			float t2 = currentLerpTime / flashDuration;
			t2 = Mathf.Sin(t2 * (float)Math.PI * 0.5f);
			selectedImage.color = Color.Lerp(Color.white, Color.red, t2);
			currentLerpTime -= Time.deltaTime;
			yield return null;
		}
	}

	public override void Right()
	{
		if (!slotIsEmpty)
		{
			base.Right();
		}
	}

	public void CreateNewProfile()
	{
		SaveDataCtrl.Set("ProfileCreated" + index, true, true);
		SetSlotEmpty(false);
		OpenRename();
		profCtrl.newProfilesCreated.Add(index);
	}

	private void SetSlotEmpty(bool empty)
	{
		if (empty)
		{
			contentContainer.alpha = 0f;
			canvasGroup.alpha = 0.5f;
			hoverAlpha = 0.5f;
		}
		else
		{
			contentContainer.alpha = 1f;
			hoverAlpha = 1f;
			canvasGroup.alpha = 1f;
		}
		emptySlot.gameObject.SetActive(empty);
		slotIsEmpty = empty;
	}
}
