using System.Collections.Generic;
using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePane : NavPanel
{
	public TMP_InputField nameInput;

	public ModCtrl modCtrl;

	public List<string> tags;

	public List<NavButton> tagButtons;

	public Transform tagButtonGrid;

	public NavButton tagButtonPrefab;

	public Color selectedTagColor;

	public Color unselectedTagColor;

	public List<string> selectedTags = new List<string>();

	private string[] allTags = new string[10] { "Characters", "Enemies", "Bosses", "Spells", "Artifacts", "Pacts", "Gameplay", "Modifiers", "Tools", "QoL" };

	private WorkshopItemUpdate currentItemUpdate;

	private void Start()
	{
		tagButtonGrid.DestroyChildren();
		string[] array = allTags;
		foreach (string text in array)
		{
			NavButton newTagButton = Object.Instantiate(tagButtonPrefab, tagButtonGrid);
			newTagButton.tmpText.text = text;
			tagButtons.Add(newTagButton);
			newTagButton.GetComponent<Button>().onClick.AddListener(delegate
			{
				ToggleTag(newTagButton);
			});
		}
	}

	public override void Close()
	{
		uMyGUI_PopupManager.Instance.HidePopup("loading");
		modCtrl.OnCloseUpdatePanel();
		base.Close();
	}

	public override void Open()
	{
		base.Open();
		nameInput.text = string.Empty;
	}

	public void SetItem(WorkshopItem itemToUpdate, WorkshopItemUpdate itemUpdate)
	{
		foreach (NavButton tagButton in tagButtons)
		{
			SetButtonAsUnselected(tagButton);
		}
		currentItemUpdate = itemUpdate;
		string[] array = itemToUpdate.SteamNative.m_details.m_rgchTags.Split(',');
		string[] array2 = array;
		foreach (string text in array2)
		{
			foreach (NavButton tagButton2 in tagButtons)
			{
				if (text == tagButton2.tmpText.text)
				{
					SetButtonAsSelected(tagButton2);
					currentItemUpdate.Tags.Add(text);
				}
			}
		}
		Debug.LogError("Item tags = " + itemToUpdate.SteamNative.m_details.m_rgchTags);
	}

	public void ToggleTag(NavButton theTagButton)
	{
		if (currentItemUpdate.Tags.Contains(theTagButton.tmpText.text))
		{
			currentItemUpdate.Tags.Remove(theTagButton.tmpText.text);
			SetButtonAsUnselected(theTagButton);
		}
		else
		{
			currentItemUpdate.Tags.Add(theTagButton.tmpText.text);
			SetButtonAsSelected(theTagButton);
		}
	}

	private void SetButtonAsSelected(NavButton theTagButton)
	{
		theTagButton.image.color = selectedTagColor;
	}

	private void SetButtonAsUnselected(NavButton theTagButton)
	{
		theTagButton.image.color = unselectedTagColor;
	}
}
