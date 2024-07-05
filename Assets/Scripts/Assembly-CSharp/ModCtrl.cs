using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using I2.Loc;
using LapinerTools.Steam;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.UI;
using LapinerTools.uMyGUI;
using TMPro;
using UnityEngine;

public class ModCtrl : NavPanel
{
	public static string YOUR_MODS_PATH = "";

	public static string MODS_PATH = "";

	private const string WORKSHOP_URL = "https://steamcommunity.com/workshop/browse/?appid=960690";

	private const string MOD_URL_BASE = "https://steamcommunity.com/sharedfiles/filedetails/?id=";

	private ItemManager itemMan;

	private SpawnCtrl spCtrl;

	public XMLReader xmlReader;

	public FlexSelector selector;

	public UploadPane uploadPane;

	public UpdatePane updatePane;

	public NavButton installButton;

	public NavButton newModButton;

	public NavButton updateModButton;

	public ToggleButton installModsOnStartupCheckbox;

	public GameObject workshopBlackout;

	public MyLog myLog;

	public TMP_Text logText;

	public List<string> luaMods = new List<string>();

	public List<string> artMods = new List<string>();

	public List<string> spellMods = new List<string>();

	public List<string> pactMods = new List<string>();

	public bool installModsAfterOpen = false;

	public GameObject uiBrowseGO;

	public GameObject uiUploadGO;

	public GameObject uiUpdateGO;

	public bool processing = false;

	private SteamWorkshopUIBrowse uiBrowse;

	private SteamWorkshopUIUpload uiUpload;

	private SteamWorkshopUIUpload uiUpdate;

	private void Start()
	{
		itemMan = S.I.itemMan;
		spCtrl = S.I.spCtrl;
		YOUR_MODS_PATH = Path.Combine(Application.streamingAssetsPath, "YourMods");
		MODS_PATH = Path.Combine(Application.streamingAssetsPath, "Mods");
		uiBrowse = uiBrowseGO.GetComponent<SteamWorkshopUIBrowse>();
		uiUpload = uiUploadGO.GetComponent<SteamWorkshopUIUpload>();
		uiUpdate = uiUpdateGO.GetComponent<SteamWorkshopUIUpload>();
		installButton.down = newModButton;
		content.SetActive(true);
		SteamWorkshopUIBrowse.Instance.OnDownloadButtonClick += delegate(WorkshopItemEventArgs p_itemArgs)
		{
			string text2 = "\n";
			try
			{
				string[] files2 = Directory.GetFiles(p_itemArgs.Item.InstalledLocalFolder);
				for (int j = 0; j < files2.Length; j++)
				{
					text2 = text2 + files2[j] + "\n";
				}
			}
			catch
			{
				text2 += "not found!";
			}
			p_itemArgs.Item.IsActive = true;
			SaveDataCtrl.Set(p_itemArgs.Item.SteamNative.m_nPublishedFileId.ToString(), p_itemArgs.Item.IsActive);
			Debug.Log("saved " + p_itemArgs.Item.SteamNative.m_nPublishedFileId.ToString() + " as " + p_itemArgs.Item.IsActive);
			string message2 = string.Concat("Name: ", p_itemArgs.Item.Name, "\nPublished File Id: ", p_itemArgs.Item.SteamNative.m_nPublishedFileId, "\nLocal Folder: ", p_itemArgs.Item.InstalledLocalFolder, "\n", text2);
			Debug.Log(message2);
		};
		SteamWorkshopUIBrowse.Instance.OnPlayButtonClick += delegate(WorkshopItemEventArgs p_itemArgs)
		{
			string text = "\n";
			try
			{
				string[] files = Directory.GetFiles(p_itemArgs.Item.InstalledLocalFolder);
				for (int i = 0; i < files.Length; i++)
				{
					text = text + files[i] + "\n";
				}
			}
			catch
			{
				text += "not found!";
			}
			p_itemArgs.Item.IsActive = false;
			SaveDataCtrl.Set(p_itemArgs.Item.SteamNative.m_nPublishedFileId.ToString(), p_itemArgs.Item.IsActive);
			SaveDataCtrl.Write();
			string message = string.Concat("Name: ", p_itemArgs.Item.Name, "\nPublished File Id: ", p_itemArgs.Item.SteamNative.m_nPublishedFileId, "\nLocal Folder: ", p_itemArgs.Item.InstalledLocalFolder, "\n", text);
			Debug.Log(message);
		};
		content.SetActive(false);
	}

	private void Update()
	{
		if (myLog.enabled)
		{
			logText.text = myLog.myLog;
		}
		if (slideBody.onScreen && (bool)btnCtrl.focusedButton)
		{
			selector.rightTarget = btnCtrl.focusedButton.transform;
		}
	}

	private void BlackoutWorkshop()
	{
		workshopBlackout.SetActive(true);
		installButton.RemoveDirectionalNavigation();
		installButton.down = installModsOnStartupCheckbox;
		installModsOnStartupCheckbox.up = installButton;
	}

	private IEnumerator _OpenMenuC()
	{
		installModsOnStartupCheckbox.toggle.isOn = SaveDataCtrl.Get("InstallModsOnStartup", false);
		if (SteamManager.Initialized)
		{
			workshopBlackout.SetActive(false);
			LoadItems();
		}
		else
		{
			BlackoutWorkshop();
		}
		myLog.enabled = true;
		yield return new WaitForEndOfFrame();
		if (installModsAfterOpen)
		{
			installModsAfterOpen = false;
			InstallMods();
		}
	}

	public void LoadItems()
	{
		uiBrowse.LoadItems(1, EWorkshopSource.SUBSCRIBED);
	}

	public void SetInstallOnOpen()
	{
		installModsAfterOpen = true;
	}

	public override void Open()
	{
		installButton.tmpText.text = ScriptLocalization.UI.Mods_InstallMods;
		base.Open();
		selector.width = 400f;
		StartCoroutine(_OpenMenuC());
	}

	public override void Close()
	{
		SaveDataCtrl.Write();
		selector.width = 20f;
		uMyGUI_PopupManager.Instance.HidePopup("dropdown");
		base.Close();
		S.I.mainCtrl.Open();
		myLog.enabled = false;
	}

	public void OpenModPage(string modFileID)
	{
		Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=" + modFileID);
	}

	public void OpenWorkshopPage()
	{
		Application.OpenURL("https://steamcommunity.com/workshop/browse/?appid=960690");
	}

	public void InstallMods()
	{
		StartCoroutine(_InstallMods());
	}

	public IEnumerator _InstallMods()
	{
		if (processing)
		{
			yield break;
		}
		float startTime = Time.time;
		S.modsInstalled = true;
		processing = true;
		installButton.tmpText.text = "(" + ScriptLocalization.UI.Mods_Installing + ")";
		luaMods.Clear();
		artMods.Clear();
		spellMods.Clear();
		pactMods.Clear();
		string[] dirs = Directory.GetDirectories(MODS_PATH);
		bool atLeastOneModInstalled = false;
		List<string> prioritizedLocalMods = new List<string>(dirs.OrderByDescending((string dir) => GetPriority(new DirectoryInfo(Path.Combine(MODS_PATH, dir)).GetFiles(), Path.Combine(MODS_PATH, dir))).ToList());
		using (List<string>.Enumerator enumerator = prioritizedLocalMods.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				string modsDir = Path.Combine(path2: enumerator.Current, path1: MODS_PATH);
				DirectoryInfo info = new DirectoryInfo(modsDir);
				Debug.Log("Installing " + info.Name + " at " + modsDir);
				FileInfo[] fileInfo = info.GetFiles();
				atLeastOneModInstalled = true;
				_InstallTheseMods(fileInfo, modsDir);
			}
		}
		if (SteamManager.Initialized)
		{
			List<WorkshopItem> prioritizedWorkshopMods = new List<WorkshopItem>(SteamMainBase<SteamWorkshopMain>.Instance.m_activeItems.Values.OrderByDescending((WorkshopItem wsItem) => GetPriority(new DirectoryInfo(wsItem.InstalledLocalFolder).GetFiles(), wsItem.InstalledLocalFolder)).ToList());
			foreach (WorkshopItem wsItem2 in prioritizedWorkshopMods)
			{
				if (wsItem2.IsActive)
				{
					Debug.Log("Installing " + wsItem2.Name + " at " + wsItem2.InstalledLocalFolder);
					DirectoryInfo wsInfo = new DirectoryInfo(wsItem2.InstalledLocalFolder);
					FileInfo[] wsFileInfo = wsInfo.GetFiles();
					atLeastOneModInstalled = true;
					_InstallTheseMods(wsFileInfo, wsItem2.InstalledLocalFolder);
				}
			}
		}
		S.modsInstalled = atLeastOneModInstalled;
		if (!atLeastOneModInstalled)
		{
			Debug.Log("NO MODS INSTALLED");
		}
		else
		{
			Debug.Log("Mods installed in " + (Time.time - startTime) + " seconds");
		}
		yield return new WaitForSeconds(0f);
		itemMan.LoadItemData();
		installButton.tmpText.text = "(" + ScriptLocalization.UI.Mods_InstallComplete + "!~)";
		yield return new WaitForSeconds(0.6f);
		installButton.tmpText.text = ScriptLocalization.UI.Mods_InstallMods;
		processing = false;
	}

	public float GetPriority(FileInfo[] fileInfo, string modsDir)
	{
		float result = 0f;
		foreach (FileInfo fileInfo2 in fileInfo)
		{
			if (fileInfo2.Name.Contains("WorkshopItemInfo.xml"))
			{
				result = xmlReader.XMLToModPriority(modsDir, fileInfo2.FullName);
			}
		}
		return result;
	}

	public void _InstallTheseMods(FileInfo[] fileInfo, string modsDir)
	{
		itemMan.animClipSpritesGenerated = false;
		foreach (FileInfo fileInfo2 in fileInfo)
		{
			if (fileInfo2.Name.Contains(".meta"))
			{
				continue;
			}
			if (fileInfo2.Name.Contains("AnimInfo.xml"))
			{
				xmlReader.StartCoroutine(xmlReader.XMLtoAnimationInfo(fileInfo2.DirectoryName, fileInfo2.FullName));
			}
			if (fileInfo2.Name.Contains(".png"))
			{
				LoadPlayerUI(fileInfo2);
			}
			else
			{
				switch (fileInfo2.Name)
				{
				case "Artifacts.lua":
					luaMods.Add(xmlReader.GetFilePath(modsDir, fileInfo2.FullName));
					break;
				case "Artifacts.xml":
					artMods.Add(xmlReader.GetFile(modsDir, fileInfo2.FullName));
					break;
				case "Effects.lua":
					luaMods.Add(xmlReader.GetFilePath(modsDir, fileInfo2.FullName));
					break;
				case "Enemies.xml":
					spCtrl.CreateBeingObjectPrototypes(fileInfo2.Name, BeingType.Enemy, true, modsDir);
					break;
				case "Heroes.xml":
					spCtrl.CreateBeingObjectPrototypes(fileInfo2.Name, BeingType.Hero, true, modsDir);
					break;
				case "Lib.lua":
					luaMods.Add(xmlReader.GetFilePath(modsDir, fileInfo2.FullName));
					break;
				case "Pacts.xml":
					pactMods.Add(xmlReader.GetFile(modsDir, fileInfo2.FullName));
					break;
				case "Spells.lua":
					luaMods.Add(xmlReader.GetFilePath(modsDir, fileInfo2.FullName));
					break;
				case "Spells.xml":
					spellMods.Add(xmlReader.GetFile(modsDir, fileInfo2.FullName));
					break;
				case "Structures.xml":
					spCtrl.CreateBeingObjectPrototypes(fileInfo2.Name, BeingType.Structure, true, modsDir);
					break;
				case "Zones.xml":
					xmlReader.XMLtoGetWorlds(xmlReader.GetFile(modsDir, fileInfo2.FullName));
					break;
				default:
					if (fileInfo2.Name.Contains(".lua"))
					{
						luaMods.Add(xmlReader.GetFilePath(modsDir, fileInfo2.FullName));
					}
					break;
				}
			}
			if (fileInfo2.Name.Contains(".dll"))
			{
				Debug.Log("Installing " + xmlReader.GetFilePath(modsDir, fileInfo2.FullName));
				HarmonyMain.LoadMod(xmlReader.GetFilePath(modsDir, fileInfo2.FullName), fileInfo2.Name);
			}
		}
	}

	private void LoadPlayerUI(FileInfo theFile)
	{
		if (!theFile.Name.Contains(".meta"))
		{
			itemMan.sprites[Path.GetFileNameWithoutExtension(theFile.FullName)] = LoadNewSprite(theFile.FullName);
		}
	}

	public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 1f, SpriteMeshType spriteType = SpriteMeshType.FullRect)
	{
		Texture2D texture2D = LoadTexture(FilePath);
		if (texture2D == null)
		{
			Debug.LogError("Texture at " + FilePath + " was UNABLE to be loaded! IT will be missing.");
			return null;
		}
		return Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0u, spriteType);
	}

	public Texture2D LoadTexture(string FilePath)
	{
		if (File.Exists(FilePath))
		{
			byte[] data = File.ReadAllBytes(FilePath);
			Texture2D texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			texture2D.filterMode = FilterMode.Point;
			if (texture2D.LoadImage(data))
			{
				return texture2D;
			}
		}
		Debug.LogError("Failed to load sprite at " + FilePath + ". Is the sprite missing or is there a typo?");
		return null;
	}

	public void InstallModsOnStartToggle()
	{
		SaveDataCtrl.Set("InstallModsOnStartup", installModsOnStartupCheckbox.toggle.isOn);
	}

	public void OpenUpdatePanel()
	{
		uMyGUI_PopupManager.Instance.ShowPopup("loading");
		SteamMainBase<SteamWorkshopMain>.Instance.GetItemList(1u, OnOwnedItemListLoaded, EWorkshopSource.OWNED);
	}

	public void OnCloseUpdatePanel()
	{
		SteamMainBase<SteamWorkshopMain>.Instance.GetItemList(1u, delegate
		{
		}, EWorkshopSource.SUBSCRIBED);
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
			}).SetText(ScriptLocalization.UI.Mods_SelectMod, ScriptLocalization.UI.Mods_WhichModUpdate);
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText(ScriptLocalization.UI.Mods_NoModsFound, ScriptLocalization.UI.Mods_PleaseCreateMod).ShowButton("ok");
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
		Debug.Log("Item's published fileid " + workshopItemUpdate.SteamNative.m_nPublishedFileId);
		string path = Path.Combine(workshopItemUpdate.ContentPath, "ItemData.txt");
		if (File.Exists(path))
		{
			Debug.Log("UPDATING ITEMDATA.TXT in " + workshopItemUpdate.ContentPath);
			Debug.Log(File.ReadAllText(path));
			File.AppendAllText(path, string.Format("{0}{1}", Environment.NewLine, "Update - " + DateTime.Now));
			Debug.Log(File.ReadAllText(path));
			updatePane.Open();
			updatePane.SetItem(p_item, workshopItemUpdate);
			uiUpdate.SetItemData(workshopItemUpdate);
		}
		else
		{
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Not Installed", "This item is subscribed, but not installed. Please sync local files in Steam!").ShowButton("ok", Start);
		}
	}
}
