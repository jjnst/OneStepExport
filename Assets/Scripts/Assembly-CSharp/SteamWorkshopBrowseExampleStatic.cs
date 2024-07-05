using System.IO;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using UnityEngine;

public class SteamWorkshopBrowseExampleStatic : MonoBehaviour
{
	private void Start()
	{
		SteamMainBase<SteamWorkshopMain>.Instance.IsDebugLogEnabled = true;
		if (SteamWorkshopUIBrowse.Instance == null)
		{
			string text = "SteamWorkshopBrowseExampleStatic: you have no SteamWorkshopUIBrowse in this scene! Please drag an drop the 'SteamWorkshopItemBrowser' prefab from 'LapinerTools/Steam/Workshop' into your Canvas object!";
			Debug.LogError(text);
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Error", text);
			return;
		}
		SteamWorkshopUIBrowse.Instance.OnPlayButtonClick += delegate(WorkshopItemEventArgs p_itemArgs)
		{
			string text2 = "\n";
			try
			{
				string[] files = Directory.GetFiles(p_itemArgs.Item.InstalledLocalFolder);
				for (int i = 0; i < files.Length; i++)
				{
					text2 = text2 + files[i] + "\n";
				}
			}
			catch
			{
				text2 += "not found!";
			}
			Debug.Log("ON PLAYED HERE");
			string text3 = string.Concat("Name: ", p_itemArgs.Item.Name, "\nPublished File Id: ", p_itemArgs.Item.SteamNative.m_nPublishedFileId, "\nLocal Folder: ", p_itemArgs.Item.InstalledLocalFolder, "\n", text2);
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Item Played", "Load your Steam Workshop item here (e.g. could be a new level for your game)\n" + text3).ShowButton("ok");
		};
	}
}
