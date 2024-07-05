using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

namespace LapinerTools.Steam.Data
{
	public class WorkshopItemUpdate
	{
		public class SteamNativeData
		{
			public PublishedFileId_t m_nPublishedFileId { get; set; }

			public UGCUpdateHandle_t m_uploadHandle { get; set; }

			public EItemUpdateStatus m_lastValidUpdateStatus { get; set; }

			public SteamNativeData()
			{
				m_nPublishedFileId = PublishedFileId_t.Invalid;
				m_uploadHandle = UGCUpdateHandle_t.Invalid;
				m_lastValidUpdateStatus = EItemUpdateStatus.k_EItemUpdateStatusInvalid;
			}

			public SteamNativeData(PublishedFileId_t p_nPublishedFileId)
			{
				m_nPublishedFileId = p_nPublishedFileId;
				m_uploadHandle = UGCUpdateHandle_t.Invalid;
				m_lastValidUpdateStatus = EItemUpdateStatus.k_EItemUpdateStatusInvalid;
			}
		}

		public string Name { get; set; }

		public string Description { get; set; }

		public string IconPath { get; set; }

		public string ContentPath { get; set; }

		public string ChangeNote { get; set; }

		public List<string> Tags { get; set; }

		public int Priority { get; set; }

		public int ModVersion { get; set; }

		public string GameVersion { get; set; }

		public SteamNativeData SteamNative { get; set; }

		public WorkshopItemUpdate()
		{
			SteamNative = new SteamNativeData();
			ChangeNote = "Initial version";
			Tags = new List<string>();
		}

		public WorkshopItemUpdate(WorkshopItem p_existingItem, List<string> tagsToSet = null)
		{
			if (p_existingItem.SteamNative != null)
			{
				Name = p_existingItem.Name;
				Description = p_existingItem.Description;
				ContentPath = p_existingItem.InstalledLocalFolder;
				SteamNative = new SteamNativeData(p_existingItem.SteamNative.m_nPublishedFileId);
				ChangeNote = "";
				Tags = new List<string>();
				Debug.Log("Update: " + p_existingItem.InstalledLocalFolder);
				if (!string.IsNullOrEmpty(ContentPath))
				{
					string text = Path.Combine(ContentPath, "workshopIcon.png");
					if (File.Exists(text))
					{
						IconPath = text;
					}
				}
			}
			else
			{
				SteamNative = new SteamNativeData();
				ChangeNote = "Initial version";
				Tags = new List<string>();
			}
		}

		public WorkshopItemUpdate(PublishedFileId_t p_existingPublishedFileId)
		{
			SteamNative = new SteamNativeData(p_existingPublishedFileId);
			ChangeNote = "";
			Tags = new List<string>();
		}
	}
}
