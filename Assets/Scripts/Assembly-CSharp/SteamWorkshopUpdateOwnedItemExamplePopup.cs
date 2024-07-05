using System;
using System.IO;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using UnityEngine;

public class SteamWorkshopUpdateOwnedItemExamplePopup : MonoBehaviour
{
	private void Start()
	{
		SteamMainBase<SteamWorkshopMain>.Instance.IsDebugLogEnabled = true;
		uMyGUI_PopupManager.Instance.ShowPopup("loading");
	}

	private void OnOwnedItemListLoaded(WorkshopItemListEventArgs p_itemListArgs)
	{
		uMyGUI_PopupManager.Instance.HidePopup("loading");
		if (p_itemListArgs.IsError)
		{
			return;
		}
		if (p_itemListArgs.ItemList.Items.Count > 0)
		{
			string[] array = new string[p_itemListArgs.ItemList.Items.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = p_itemListArgs.ItemList.Items[i].Name;
			}
			((uMyGUI_PopupDropdown)uMyGUI_PopupManager.Instance.ShowPopup("dropdown")).SetEntries(array).SetOnSelected(delegate(int p_selectedIndex)
			{
				OnOwnedItemSelectedForUpdate(p_itemListArgs.ItemList.Items[p_selectedIndex]);
			}).SetText("Select Item", "Select the item, which you want to update");
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("No Items Found", "Cannot find any items to update, it seems that you haven't uploaded any items yet.").ShowButton("ok");
		}
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
