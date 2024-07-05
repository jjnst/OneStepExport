using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using UnityEngine;

public class SteamWorkshopTagsExample : MonoBehaviour
{
	[SerializeField]
	public string[] TAGS = new string[4] { "TAG1", "TAG2", "TAG3", "TAG4" };

	private List<string> m_tagsToUse = new List<string>();

	private void Start()
	{
		SteamMainBase<SteamWorkshopMain>.Instance.IsDebugLogEnabled = true;
		SteamMainBase<SteamWorkshopMain>.Instance.SearchMatchAnyTag = true;
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0f, Screen.height - 28, Screen.width, 28f));
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Browse With Tags", GUILayout.Height(28f)))
		{
			if (SteamWorkshopUIBrowse.Instance != null)
			{
				SteamWorkshopUIBrowse.Instance.LoadItems(1);
			}
			else
			{
				((SteamWorkshopPopupBrowse)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_browse")).BrowseUI.OnPlayButtonClick += delegate(WorkshopItemEventArgs p_itemArgs)
				{
					((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Item Played", "Item Name: " + p_itemArgs.Item.Name + "\nFor further item details check SteamWorkshopBrowseExamplePopup or SteamWorkshopBrowseExampleStatic classes.").ShowButton("ok");
				};
			}
		}
		if (GUILayout.Button("Upload With Tags", GUILayout.Height(40f)))
		{
			string text = Path.Combine(Application.persistentDataPath, "DummyItemContentFolder" + DateTime.Now.Ticks);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			string contents = "Save your item/level/mod data here.\nIt does not need to be a text file. Any file type is supported (binary, images, etc...).\nYou can save multiple files, Steam items are folders (not single files).\n";
			File.WriteAllText(Path.Combine(text, "ItemData.txt"), contents);
			WorkshopItemUpdate workshopItemUpdate = new WorkshopItemUpdate();
			workshopItemUpdate.ContentPath = text;
			workshopItemUpdate.Tags = m_tagsToUse;
			SteamWorkshopPopupUpload steamWorkshopPopupUpload = (SteamWorkshopPopupUpload)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_upload");
			steamWorkshopPopupUpload.UploadUI.SetItemData(workshopItemUpdate);
			steamWorkshopPopupUpload.UploadUI.OnFinishedUpload += delegate(WorkshopItemUpdateEventArgs p_args)
			{
				if (!p_args.IsError && p_args.Item != null)
				{
					((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Item Uploaded", "Item '" + p_args.Item.Name + "' was successfully uploaded!\nTags: " + p_args.Item.Tags.Aggregate((string tag1, string tag2) => tag1 + ", " + tag2) + "\nIt can take a long time for this new level to arrive in the Steam Workshop listing, sometimes longer than an hour! Be patient...").ShowButton("ok");
				}
			};
		}
		for (int i = 0; i < TAGS.Length; i++)
		{
			bool flag = m_tagsToUse.Contains(TAGS[i]);
			bool flag2 = GUILayout.Toggle(flag, TAGS[i]);
			if (flag2 && !flag)
			{
				m_tagsToUse.Add(TAGS[i]);
			}
			if (!flag2 && flag)
			{
				m_tagsToUse.Remove(TAGS[i]);
			}
			SteamMainBase<SteamWorkshopMain>.Instance.SearchTags = m_tagsToUse;
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
