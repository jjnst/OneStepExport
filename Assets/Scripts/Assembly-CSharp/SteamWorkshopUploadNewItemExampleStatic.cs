using System;
using System.IO;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using UnityEngine;

public class SteamWorkshopUploadNewItemExampleStatic : MonoBehaviour
{
	private void Start()
	{
		SteamMainBase<SteamWorkshopMain>.Instance.IsDebugLogEnabled = true;
		if (SteamWorkshopUIUpload.Instance == null)
		{
			string text = "SteamWorkshopUploadNewItemExampleStatic: you have no SteamWorkshopUIUpload in this scene! Please drag an drop the 'SteamWorkshopItemUpload' prefab from 'LapinerTools/Steam/Workshop' into your Canvas object!";
			Debug.LogError(text);
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Error", text);
			return;
		}
		string text2 = Path.Combine(Path.Combine(Application.streamingAssetsPath, "Mods"), "DummyItemContentFolder" + DateTime.Now.Ticks);
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		string contents = "Save your item/level/mod data here.\nIt does not need to be a text file. Any file type is supported (binary, images, etc...).\nYou can save multiple files, Steam items are folders (not single files).\n";
		File.WriteAllText(Path.Combine(text2, "ItemData.txt"), contents);
		WorkshopItemUpdate workshopItemUpdate = new WorkshopItemUpdate();
		workshopItemUpdate.ContentPath = text2;
		SteamWorkshopUIUpload.Instance.SetItemData(workshopItemUpdate);
	}

	private void OnOwnedItemSelectedForUpdate(WorkshopItem p_item)
	{
		uMyGUI_PopupManager.Instance.HidePopup("dropdown");
		if (!p_item.IsInstalled)
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Not Installed", "This item is not installed. Please subscribe this item first!").ShowButton("ok", Start);
			return;
		}
		WorkshopItemUpdate workshopItemUpdate = new WorkshopItemUpdate(p_item);
		string path = Path.Combine(workshopItemUpdate.ContentPath, "ItemData.txt");
		if (File.Exists(path))
		{
			File.AppendAllText(path, "\nUpdate - " + DateTime.Now);
			((SteamWorkshopPopupUpload)uMyGUI_PopupManager.Instance.ShowPopup("steam_ugc_upload")).UploadUI.SetItemData(workshopItemUpdate);
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Not Installed", "This item is subscribed, but not installed. Please sync local files in Steam!").ShowButton("ok", Start);
		}
	}
}
