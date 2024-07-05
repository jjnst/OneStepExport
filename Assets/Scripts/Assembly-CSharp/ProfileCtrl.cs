using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

public class ProfileCtrl : NavPanel
{
	public DeckCtrl deCtrl;

	public List<ProfileCard> profileCards;

	private int openedProfile = 0;

	public List<int> newProfilesCreated = new List<int>();

	public Dictionary<string, Sprite> charIcons = new Dictionary<string, Sprite>();

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Open()
	{
		openedProfile = S.I.currentProfile;
		defaultButton = profileCards[openedProfile];
		base.Open();
		UpdateCards();
	}

	public override void Close()
	{
		base.Close();
		S.I.mainCtrl.Open();
		if (openedProfile != S.I.currentProfile || newProfilesCreated.Contains(S.I.currentProfile))
		{
			SaveDataCtrl.Set("CurrentProfile", S.I.currentProfile, true);
			SaveDataCtrl.Write(true);
			Debug.Log("saved with " + S.I.currentProfile);
			S.I.optCtrl.settingsPane.StartCoroutine(S.I.optCtrl.settingsPane._RestartScene());
		}
		S.I.mainCtrl.profileButton.tmpText.text = ScriptLocalization.UI.MainMenu_Profile + " (" + SaveDataCtrl.Get("ProfileName" + S.I.currentProfile, "Saffron", true) + ")";
	}

	private void UpdateCards()
	{
		SelectCard(profileCards[openedProfile]);
		foreach (ProfileCard profileCard in profileCards)
		{
			profileCard.UpdateText();
		}
	}

	public void SelectCard(ProfileCard selectedCard)
	{
		foreach (ProfileCard profileCard in profileCards)
		{
			if (profileCard != selectedCard)
			{
				profileCard.selectedImage.color = Color.clear;
			}
		}
		selectedCard.selectedImage.color = Color.white;
		S.I.currentProfile = profileCards.IndexOf(selectedCard);
	}
}
