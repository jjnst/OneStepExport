using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.Data.Internal;
using Steamworks;
using UnityEngine;

namespace LapinerTools.Steam
{
	public class SteamWorkshopMain : SteamMainBase<SteamWorkshopMain>
	{
		private uint m_reqPage = 0u;

		private WorkshopItemList m_reqItemList = null;

		public Dictionary<PublishedFileId_t, WorkshopItem> m_items = new Dictionary<PublishedFileId_t, WorkshopItem>();

		public Dictionary<PublishedFileId_t, WorkshopItem> m_activeItems = new Dictionary<PublishedFileId_t, WorkshopItem>();

		private List<PublishedFileId_t> m_downloadingItems = new List<PublishedFileId_t>();

		private WorkshopItemUpdate m_uploadItemData = null;

		private Texture2D m_renderedTexture = null;

		[SerializeField]
		[Tooltip("Controls the item list sorting. See also OnItemListLoaded and GetItemList.")]
		private WorkshopSortMode m_sorting = new WorkshopSortMode();

		[SerializeField]
		[Tooltip("This search filter is applied to the item list. See also OnItemListLoaded and GetItemList.")]
		private string m_searchText = "";

		[SerializeField]
		[Tooltip("This tag filter is applied to the item list. See also SearchMatchAnyTag, OnItemListLoaded and GetItemList.")]
		private List<string> m_searchTags = new List<string>();

		[SerializeField]
		[Tooltip("Should the items filtered by SearchTags just need to have one required tag (true), or all of them (false). See also OnItemListLoaded and GetItemList.")]
		private bool m_searchMatchAnyTag = true;

		[SerializeField]
		[Tooltip("Set this property to true if you want your UI to respond faster, but sacrifice up-to-dateness. Disabled by default.")]
		private bool m_isSteamCacheEnabled = false;

		public WorkshopSortMode Sorting
		{
			get
			{
				return m_sorting;
			}
			set
			{
				m_sorting = value;
			}
		}

		public string SearchText
		{
			get
			{
				return m_searchText;
			}
			set
			{
				m_searchText = value;
			}
		}

		public List<string> SearchTags
		{
			get
			{
				return m_searchTags;
			}
			set
			{
				m_searchTags = value;
			}
		}

		public bool SearchMatchAnyTag
		{
			get
			{
				return m_searchMatchAnyTag;
			}
			set
			{
				m_searchMatchAnyTag = value;
			}
		}

		public bool IsSteamCacheEnabled
		{
			get
			{
				return m_isSteamCacheEnabled;
			}
			set
			{
				m_isSteamCacheEnabled = value;
			}
		}

		public event Action<WorkshopItemListEventArgs> OnItemListLoaded;

		public event Action<WorkshopItemEventArgs> OnSubscribed;

		public event Action<WorkshopItemEventArgs> OnUnsubscribed;

		public event Action<WorkshopItemEventArgs> OnAddedFavorite;

		public event Action<WorkshopItemEventArgs> OnRemovedFavorite;

		public event Action<WorkshopItemEventArgs> OnVoted;

		public event Action<WorkshopItemEventArgs> OnInstalled;

		public event Action<WorkshopItemUpdateEventArgs> OnUploaded;

		public bool GetItemList(uint p_page, Action<WorkshopItemListEventArgs> p_onItemListLoaded, EWorkshopSource sortSource)
		{
			if (p_page == 0)
			{
				LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = new LapinerTools.Steam.Data.ErrorEventArgs("Page (p_page parameter) must be greater 0, but was '" + p_page + "'!");
				InvokeEventHandlerSafely(p_onItemListLoaded, new WorkshopItemListEventArgs(errorEventArgs));
				HandleError("GetItemList: failed! ", errorEventArgs);
				return false;
			}
			lock (m_lock)
			{
				if (m_reqItemList != null)
				{
					return false;
				}
				if (SteamManager.Initialized)
				{
					m_reqItemList = new WorkshopItemList();
					m_reqItemList.Page = p_page;
					m_reqItemList.PagesItemsFavorited = 0u;
					m_reqItemList.PagesItemsVoted = 0u;
					m_pendingRequests.Clear<GetUserItemVoteResult_t>();
					SetSingleShotEventHandler("OnItemListLoaded", ref this.OnItemListLoaded, p_onItemListLoaded);
					Debug.Log("Getting item list: " + sortSource);
					switch (sortSource)
					{
					case EWorkshopSource.SUBSCRIBED:
						QueryAllItems(sortSource);
						break;
					case EWorkshopSource.PUBLIC:
						QueryPublishedItems(1u);
						break;
					case EWorkshopSource.OWNED:
						QueryOwnedSubscribedItems(sortSource);
						break;
					}
					return true;
				}
				LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs2 = LapinerTools.Steam.Data.ErrorEventArgs.CreateSteamNotInit();
				InvokeEventHandlerSafely(p_onItemListLoaded, new WorkshopItemListEventArgs(errorEventArgs2));
				HandleError("GetItemList: failed! ", errorEventArgs2);
				return false;
			}
		}

		public bool GetSubbedItemList(uint p_page, Action<WorkshopItemListEventArgs> p_onItemListLoaded)
		{
			if (p_page == 0)
			{
				LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = new LapinerTools.Steam.Data.ErrorEventArgs("Page (p_page parameter) must be greater 0, but was '" + p_page + "'!");
				InvokeEventHandlerSafely(p_onItemListLoaded, new WorkshopItemListEventArgs(errorEventArgs));
				HandleError("GetItemList: failed! ", errorEventArgs);
				return false;
			}
			lock (m_lock)
			{
				if (m_reqItemList != null)
				{
					return false;
				}
				if (SteamManager.Initialized)
				{
					m_reqItemList = new WorkshopItemList();
					m_reqItemList.Page = p_page;
					m_reqItemList.PagesItemsFavorited = 0u;
					m_reqItemList.PagesItemsVoted = 0u;
					m_pendingRequests.Clear<GetUserItemVoteResult_t>();
					SetSingleShotEventHandler("OnItemListLoaded", ref this.OnItemListLoaded, p_onItemListLoaded);
					QuerySubscribedItems(1u);
					return true;
				}
				LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs2 = LapinerTools.Steam.Data.ErrorEventArgs.CreateSteamNotInit();
				InvokeEventHandlerSafely(p_onItemListLoaded, new WorkshopItemListEventArgs(errorEventArgs2));
				HandleError("GetItemList: failed! ", errorEventArgs2);
				return false;
			}
		}

		public bool Subscribe(WorkshopItem p_item, Action<WorkshopItemEventArgs> p_onSubscribed)
		{
			return Subscribe(p_item.SteamNative.m_nPublishedFileId, p_onSubscribed);
		}

		public bool Subscribe(PublishedFileId_t p_fileId, Action<WorkshopItemEventArgs> p_onSubscribed)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnSubscribed" + p_fileId, ref this.OnSubscribed, p_onSubscribed);
				Execute<RemoteStorageSubscribePublishedFileResult_t>(SteamUGC.SubscribeItem(p_fileId), OnSubscribeCallCompleted);
				return true;
			}
			LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = LapinerTools.Steam.Data.ErrorEventArgs.CreateSteamNotInit();
			InvokeEventHandlerSafely(p_onSubscribed, new WorkshopItemEventArgs(errorEventArgs));
			HandleError("Subscribe: failed! ", errorEventArgs);
			return false;
		}

		public bool Unsubscribe(WorkshopItem p_item, Action<WorkshopItemEventArgs> p_onUnsubscribed)
		{
			return Unsubscribe(p_item.SteamNative.m_nPublishedFileId, p_onUnsubscribed);
		}

		public bool Unsubscribe(PublishedFileId_t p_fileId, Action<WorkshopItemEventArgs> p_onUnsubscribed)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnUnsubscribed" + p_fileId, ref this.OnUnsubscribed, p_onUnsubscribed);
				Execute<RemoteStorageUnsubscribePublishedFileResult_t>(SteamUGC.UnsubscribeItem(p_fileId), OnUnsubscribeCallCompleted);
				return true;
			}
			LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = LapinerTools.Steam.Data.ErrorEventArgs.CreateSteamNotInit();
			InvokeEventHandlerSafely(p_onUnsubscribed, new WorkshopItemEventArgs(errorEventArgs));
			HandleError("Unsubscribe: failed! ", errorEventArgs);
			return false;
		}

		public bool AddFavorite(WorkshopItem p_item, Action<WorkshopItemEventArgs> p_onAddedFavorite)
		{
			return AddFavorite(p_item.SteamNative.m_nPublishedFileId, p_onAddedFavorite);
		}

		public bool AddFavorite(PublishedFileId_t p_fileId, Action<WorkshopItemEventArgs> p_onAddedFavorite)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnAddedFavorite" + p_fileId, ref this.OnAddedFavorite, p_onAddedFavorite);
				Execute<UserFavoriteItemsListChanged_t>(SteamUGC.AddItemToFavorites(SteamUtils.GetAppID(), p_fileId), OnFavoriteChangeCallCompleted);
				return true;
			}
			LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = LapinerTools.Steam.Data.ErrorEventArgs.CreateSteamNotInit();
			InvokeEventHandlerSafely(p_onAddedFavorite, new WorkshopItemEventArgs(errorEventArgs));
			HandleError("AddFavorite: failed! ", errorEventArgs);
			return false;
		}

		public bool RemoveFavorite(WorkshopItem p_item, Action<WorkshopItemEventArgs> p_onRemovedFavorite)
		{
			return RemoveFavorite(p_item.SteamNative.m_nPublishedFileId, p_onRemovedFavorite);
		}

		public bool RemoveFavorite(PublishedFileId_t p_fileId, Action<WorkshopItemEventArgs> p_onRemovedFavorite)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnRemovedFavorite" + p_fileId, ref this.OnRemovedFavorite, p_onRemovedFavorite);
				Execute<UserFavoriteItemsListChanged_t>(SteamUGC.RemoveItemFromFavorites(SteamUtils.GetAppID(), p_fileId), OnFavoriteChangeCallCompleted);
				return true;
			}
			LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = LapinerTools.Steam.Data.ErrorEventArgs.CreateSteamNotInit();
			InvokeEventHandlerSafely(p_onRemovedFavorite, new WorkshopItemEventArgs(errorEventArgs));
			HandleError("RemoveFavorite: failed! ", errorEventArgs);
			return false;
		}

		public bool Vote(WorkshopItem p_item, bool p_isUpVote, Action<WorkshopItemEventArgs> p_onVoted)
		{
			return Vote(p_item.SteamNative.m_nPublishedFileId, p_isUpVote, p_onVoted);
		}

		public bool Vote(PublishedFileId_t p_fileId, bool p_isUpVote, Action<WorkshopItemEventArgs> p_onVoted)
		{
			if (SteamManager.Initialized)
			{
				SetSingleShotEventHandler("OnVoted" + p_fileId, ref this.OnVoted, p_onVoted);
				Execute<SetUserItemVoteResult_t>(SteamUGC.SetUserItemVote(p_fileId, p_isUpVote), OnVoteCallCompleted);
				return true;
			}
			LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = LapinerTools.Steam.Data.ErrorEventArgs.CreateSteamNotInit();
			InvokeEventHandlerSafely(p_onVoted, new WorkshopItemEventArgs(errorEventArgs));
			HandleError("Vote: failed! ", errorEventArgs);
			return false;
		}

		public float GetDownloadProgress(WorkshopItem p_item)
		{
			return GetDownloadProgress(p_item.SteamNative.m_nPublishedFileId);
		}

		public float GetDownloadProgress(PublishedFileId_t p_fileId)
		{
			if (SteamManager.Initialized)
			{
				EItemState itemState = (EItemState)SteamUGC.GetItemState(p_fileId);
				if (IsDownloading(itemState))
				{
					ulong punBytesDownloaded;
					ulong punBytesTotal;
					if (SteamUGC.GetItemDownloadInfo(p_fileId, out punBytesDownloaded, out punBytesTotal) && punBytesTotal != 0)
					{
						return (float)punBytesDownloaded / (float)punBytesTotal;
					}
					return 0f;
				}
				if (IsInstalled(itemState))
				{
					return 1f;
				}
			}
			return 0f;
		}

		public float GetUploadProgress(WorkshopItemUpdate p_itemUpdate)
		{
			if (SteamManager.Initialized && p_itemUpdate.SteamNative.m_uploadHandle != UGCUpdateHandle_t.Invalid)
			{
				ulong punBytesProcessed;
				ulong punBytesTotal;
				EItemUpdateStatus itemUpdateProgress = SteamUGC.GetItemUpdateProgress(p_itemUpdate.SteamNative.m_uploadHandle, out punBytesProcessed, out punBytesTotal);
				if (itemUpdateProgress != 0)
				{
					p_itemUpdate.SteamNative.m_lastValidUpdateStatus = itemUpdateProgress;
				}
				switch (itemUpdateProgress)
				{
				case EItemUpdateStatus.k_EItemUpdateStatusPreparingConfig:
					return 0f;
				case EItemUpdateStatus.k_EItemUpdateStatusPreparingContent:
					return ((punBytesTotal != 0) ? ((float)punBytesProcessed / (float)punBytesTotal) : 0f) * 0.1f;
				case EItemUpdateStatus.k_EItemUpdateStatusUploadingContent:
					return ((punBytesTotal != 0) ? ((float)punBytesProcessed / (float)punBytesTotal) : 0f) * 0.65f + 0.1f;
				case EItemUpdateStatus.k_EItemUpdateStatusUploadingPreviewFile:
					return ((punBytesTotal != 0) ? ((float)punBytesProcessed / (float)punBytesTotal) : 0f) * 0.15f + 0.75f;
				case EItemUpdateStatus.k_EItemUpdateStatusCommittingChanges:
					return ((punBytesTotal != 0) ? ((float)punBytesProcessed / (float)punBytesTotal) : 0f) * 0.1f + 0.9f;
				}
				if (p_itemUpdate.SteamNative.m_lastValidUpdateStatus != 0)
				{
					return 1f;
				}
			}
			return 0f;
		}

		public bool Upload(WorkshopItemUpdate p_itemData, Action<WorkshopItemUpdateEventArgs> p_onUploaded, bool afterCreation = false)
		{
			if (SteamManager.Initialized)
			{
				Debug.Log("icon path was " + p_itemData.IconPath);
				bool flag = false;
				if (!string.IsNullOrEmpty(p_itemData.ContentPath))
				{
					string[] files = Directory.GetFiles(p_itemData.ContentPath);
					string[] array = files;
					foreach (string objA in array)
					{
						if (!object.Equals(objA, p_itemData.IconPath))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = new LapinerTools.Steam.Data.ErrorEventArgs("No content to upload found! WorkshopItemUpdate.ContentPath is set to '" + p_itemData.ContentPath + "'!");
					InvokeEventHandlerSafely(p_onUploaded, new WorkshopItemUpdateEventArgs(errorEventArgs));
					HandleError("Upload: failed! ", errorEventArgs);
					return false;
				}
				m_uploadItemData = p_itemData;
				if (m_uploadItemData.SteamNative.m_nPublishedFileId == PublishedFileId_t.Invalid)
				{
					Debug.Log("CREATING NEW ITEM");
					SetSingleShotEventHandler("OnUploaded", ref this.OnUploaded, p_onUploaded);
					Execute<CreateItemResult_t>(SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeFirst), OnCreateItemCompleted);
				}
				else
				{
					Debug.Log("UPDATING EXISTING ITEM");
					if (!string.IsNullOrEmpty(m_uploadItemData.ContentPath))
					{
						Debug.Log("streaming to " + m_uploadItemData.ContentPath + " | " + m_uploadItemData.IconPath);
						using (FileStream stream = new FileStream(Path.Combine(m_uploadItemData.ContentPath, "WorkshopItemInfo.xml"), FileMode.Create))
						{
							new XmlSerializer(typeof(WorkshopItemInfo)).Serialize(stream, new WorkshopItemInfo
							{
								PublishedFileId = m_uploadItemData.SteamNative.m_nPublishedFileId.m_PublishedFileId,
								Name = m_uploadItemData.Name,
								Description = m_uploadItemData.Description,
								IconFileName = ((!string.IsNullOrEmpty(m_uploadItemData.IconPath)) ? Path.GetFileName(m_uploadItemData.IconPath) : ""),
								Tags = ((m_uploadItemData.Tags != null) ? m_uploadItemData.Tags.ToArray() : new string[0]),
								Priority = m_uploadItemData.Priority,
								ModVersion = m_uploadItemData.ModVersion,
								GameVersion = S.I.vrCtrl.currentVersion
							});
						}
					}
					UGCUpdateHandle_t uGCUpdateHandle_t = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), m_uploadItemData.SteamNative.m_nPublishedFileId);
					m_uploadItemData.SteamNative.m_uploadHandle = uGCUpdateHandle_t;
					bool flag2 = false;
					flag2 = !afterCreation || SteamUGC.SetItemVisibility(uGCUpdateHandle_t, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
					bool flag3 = !string.IsNullOrEmpty(m_uploadItemData.Name) && SteamUGC.SetItemTitle(uGCUpdateHandle_t, m_uploadItemData.Name);
					bool flag4 = !string.IsNullOrEmpty(m_uploadItemData.IconPath) && SteamUGC.SetItemPreview(uGCUpdateHandle_t, m_uploadItemData.IconPath);
					bool flag5 = !string.IsNullOrEmpty(m_uploadItemData.ContentPath) && SteamUGC.SetItemContent(uGCUpdateHandle_t, m_uploadItemData.ContentPath);
					Debug.LogError("Setting x tags " + m_uploadItemData.Tags.Count);
					bool flag6 = m_uploadItemData.Tags != null && m_uploadItemData.Tags.Count > 0 && SteamUGC.SetItemTags(uGCUpdateHandle_t, m_uploadItemData.Tags);
					if (!flag2)
					{
						HandleError("Upload: ", new LapinerTools.Steam.Data.ErrorEventArgs("Could not set item visibility to 'public'!"));
					}
					if (!string.IsNullOrEmpty(m_uploadItemData.Name) && !flag3)
					{
						HandleError("Upload: ", new LapinerTools.Steam.Data.ErrorEventArgs("Could not set item title to '" + m_uploadItemData.Name + "'!"));
					}
					if (!string.IsNullOrEmpty(m_uploadItemData.IconPath) && !flag4)
					{
						HandleError("Upload: ", new LapinerTools.Steam.Data.ErrorEventArgs("Could not set item icon path to '" + m_uploadItemData.IconPath + "'!"));
					}
					if (!string.IsNullOrEmpty(m_uploadItemData.ContentPath) && !flag5)
					{
						HandleError("Upload: ", new LapinerTools.Steam.Data.ErrorEventArgs("Could not set item content path to '" + m_uploadItemData.ContentPath + "'!"));
					}
					if (m_uploadItemData.Tags != null && m_uploadItemData.Tags.Count > 0 && !flag6)
					{
						HandleError("Upload: ", new LapinerTools.Steam.Data.ErrorEventArgs("Could not set item tags!"));
					}
					if (base.IsDebugLogEnabled)
					{
						Debug.Log("Upload: starting...");
					}
					SetSingleShotEventHandler("OnUploaded", ref this.OnUploaded, p_onUploaded);
					Execute<SubmitItemUpdateResult_t>(SteamUGC.SubmitItemUpdate(uGCUpdateHandle_t, p_itemData.ChangeNote), OnItemUpdateCompleted);
				}
				return true;
			}
			LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs2 = LapinerTools.Steam.Data.ErrorEventArgs.CreateSteamNotInit();
			InvokeEventHandlerSafely(p_onUploaded, new WorkshopItemUpdateEventArgs(errorEventArgs2));
			HandleError("Upload: failed! ", errorEventArgs2);
			return false;
		}

		public void RenderIcon(Camera p_camera, int p_width, int p_height, string p_saveToFilePath, Action<Texture2D> p_onRenderIconCompleted)
		{
			StartCoroutine(RenderIconRoutine(p_camera, p_width, p_height, p_saveToFilePath, true, p_onRenderIconCompleted));
		}

		public void RenderIcon(Camera p_camera, int p_width, int p_height, string p_saveToFilePath, bool p_keepTextureReference, Action<Texture2D> p_onRenderIconCompleted)
		{
			StartCoroutine(RenderIconRoutine(p_camera, p_width, p_height, p_saveToFilePath, p_keepTextureReference, p_onRenderIconCompleted));
		}

		public WorkshopItemUpdate GetItemUpdateFromFolder(string p_itemContentFolderPath)
		{
			WorkshopItemUpdate workshopItemUpdate = null;
			string text = Path.Combine(p_itemContentFolderPath, "WorkshopItemInfo.xml");
			if (File.Exists(text))
			{
				try
				{
					using (FileStream stream = new FileStream(text, FileMode.Open))
					{
						WorkshopItemInfo workshopItemInfo = new XmlSerializer(typeof(WorkshopItemInfo)).Deserialize(stream) as WorkshopItemInfo;
						workshopItemUpdate = new WorkshopItemUpdate(new PublishedFileId_t(workshopItemInfo.PublishedFileId))
						{
							Name = workshopItemInfo.Name,
							Description = workshopItemInfo.Description,
							ContentPath = p_itemContentFolderPath,
							Priority = workshopItemInfo.Priority,
							ModVersion = workshopItemInfo.ModVersion + 1,
							GameVersion = S.I.vrCtrl.currentVersion
						};
						if (!string.IsNullOrEmpty(workshopItemInfo.IconFileName))
						{
							string text2 = Path.Combine(p_itemContentFolderPath, workshopItemInfo.IconFileName);
							if (File.Exists(text2))
							{
								workshopItemUpdate.IconPath = text2;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("SteamWorkshopMain: GetItemUpdateFromFolder: could not parse item info at '" + text + "'!\n" + ex.Message);
				}
			}
			else
			{
				Debug.LogError("SteamWorkshopMain: GetItemUpdateFromFolder: could not find item info at '" + text + "'!");
			}
			return workshopItemUpdate;
		}

		protected override void LateUpdate()
		{
			lock (m_lock)
			{
				for (int num = m_downloadingItems.Count - 1; num >= 0; num--)
				{
					PublishedFileId_t publishedFileId_t = m_downloadingItems[num];
					WorkshopItem value;
					if (m_items.TryGetValue(publishedFileId_t, out value))
					{
						EItemState itemState = (EItemState)SteamUGC.GetItemState(publishedFileId_t);
						if (itemState != value.SteamNative.m_itemState || IsInstalled(itemState))
						{
							value.SteamNative.m_itemState = itemState;
							value.IsInstalled = IsInstalled(itemState);
							value.IsDownloading = IsDownloading(itemState);
							value.IsUpdateNeeded = IsUpdateNeeded(itemState);
							if (value.IsInstalled)
							{
								DateTime installedTimestamp = DateTime.MinValue;
								ulong punSizeOnDisk;
								string pchFolder;
								uint punTimeStamp;
								if (SteamUGC.GetItemInstallInfo(publishedFileId_t, out punSizeOnDisk, out pchFolder, 260u, out punTimeStamp))
								{
									installedTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(punTimeStamp).ToLocalTime();
								}
								value.InstalledLocalFolder = pchFolder;
								value.InstalledSizeOnDisk = punSizeOnDisk;
								value.InstalledTimestamp = installedTimestamp;
								m_downloadingItems.RemoveAt(num);
								if (base.IsDebugLogEnabled)
								{
									Debug.Log(string.Concat("SteamWorkshopMain: item installed ", publishedFileId_t, (this.OnInstalled != null) ? " (will notify)" : " (no listeners)"));
								}
								if (this.OnInstalled != null)
								{
									InvokeEventHandlerSafely(this.OnInstalled, new WorkshopItemEventArgs(value));
									ClearSingleShotEventHandlers("OnInstalled" + publishedFileId_t, ref this.OnInstalled);
								}
							}
						}
					}
					else
					{
						m_downloadingItems.RemoveAt(num);
					}
				}
				int num2 = m_pendingRequests.Count<GetUserItemVoteResult_t>();
				base.LateUpdate();
				int num3 = m_pendingRequests.Count<GetUserItemVoteResult_t>();
				if (num2 > 0 && num3 == 0)
				{
					QueryAllItems();
				}
				if (base.IsDebugLogEnabled && Time.frameCount % 300 == 0 && m_downloadingItems.Count > 0)
				{
					Debug.Log("Pending downloads left: " + m_downloadingItems.Count);
				}
			}
		}

		private void OnDestroy()
		{
			if (m_renderedTexture != null)
			{
				UnityEngine.Object.Destroy(m_renderedTexture);
				m_renderedTexture = null;
			}
		}

		private void OnAvailableItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
		{
			if (!CheckAndLogResult<SteamUGCQueryCompleted_t, WorkshopItemListEventArgs>("OnAvailableItemsCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnItemListLoaded", ref this.OnItemListLoaded))
			{
				return;
			}
			lock (m_lock)
			{
				m_reqItemList.PagesItems = GetPageCount(p_callback);
				for (uint num = 0u; num < p_callback.m_unNumResultsReturned; num++)
				{
					SteamUGCDetails_t pDetails;
					if (!SteamUGC.GetQueryUGCResult(p_callback.m_handle, num, out pDetails))
					{
						continue;
					}
					WorkshopItem item = ParseItem(p_callback.m_handle, num, pDetails);
					if (!item.IsSubscribed)
					{
						continue;
					}
					if (m_sorting.SOURCE != EWorkshopSource.OWNED || item.IsOwned)
					{
						m_reqItemList.Items.Add(item);
					}
					m_items[item.SteamNative.m_nPublishedFileId] = item;
					if (SaveDataCtrl.Get(item.SteamNative.m_nPublishedFileId.ToString(), false))
					{
						item.IsActive = SaveDataCtrl.Get(item.SteamNative.m_nPublishedFileId.ToString(), false);
						m_activeItems[item.SteamNative.m_nPublishedFileId] = item;
					}
					string text = "\n";
					if (item.IsActive)
					{
						try
						{
							string[] files = Directory.GetFiles(item.InstalledLocalFolder);
							for (int i = 0; i < files.Length; i++)
							{
								text = text + files[i] + "\n";
							}
						}
						catch
						{
							text += "not found!";
							if (m_items.TryGetValue(item.SteamNative.m_nPublishedFileId, out item))
							{
								EItemState itemState = (EItemState)SteamUGC.GetItemState(item.SteamNative.m_nPublishedFileId);
								item.SteamNative.m_itemState = itemState;
								item.IsInstalled = IsInstalled(itemState);
								item.IsDownloading = IsDownloading(itemState);
								item.IsUpdateNeeded = IsUpdateNeeded(itemState);
								if (item.IsInstalled && SteamUGC.DownloadItem(item.SteamNative.m_nPublishedFileId, true))
								{
									if (base.IsDebugLogEnabled)
									{
										Debug.Log("OnAddItemTo m_Items Completed: started download for missing item " + item.SteamNative.m_nPublishedFileId);
									}
									if (!m_downloadingItems.Contains(item.SteamNative.m_nPublishedFileId))
									{
										m_downloadingItems.Add(item.SteamNative.m_nPublishedFileId);
									}
									itemState = (EItemState)SteamUGC.GetItemState(item.SteamNative.m_nPublishedFileId);
									item.SteamNative.m_itemState = itemState;
									item.IsInstalled = IsInstalled(itemState);
									item.IsDownloading = IsDownloading(itemState);
									item.IsUpdateNeeded = IsUpdateNeeded(itemState);
								}
								else if (base.IsDebugLogEnabled)
								{
									Debug.Log("OnAddItemTo m_Items Completed: item is correctly installed " + item.SteamNative.m_nPublishedFileId);
								}
							}
						}
					}
					item.IsFavorited = m_reqItemList.ItemsFavorited.Where((WorkshopItem flvl) => flvl.SteamNative.m_nPublishedFileId == item.SteamNative.m_nPublishedFileId).FirstOrDefault() != null;
					WorkshopItem workshopItem = m_reqItemList.ItemsVoted.Where((WorkshopItem flvl) => flvl.SteamNative.m_nPublishedFileId == item.SteamNative.m_nPublishedFileId).FirstOrDefault();
					if (workshopItem != null)
					{
						item.IsVotedUp = workshopItem.IsVotedUp;
						item.IsVotedDown = workshopItem.IsVotedDown;
						item.IsVoteSkipped = workshopItem.IsVoteSkipped;
					}
				}
				if (this.OnItemListLoaded != null)
				{
					InvokeEventHandlerSafely(this.OnItemListLoaded, new WorkshopItemListEventArgs
					{
						ItemList = m_reqItemList
					});
					ClearSingleShotEventHandlers("OnItemListLoaded", ref this.OnItemListLoaded);
					if (base.IsDebugLogEnabled)
					{
						Debug.Log("OnAvailableItemsCallCompleted: loaded " + m_reqItemList.Items.Count + " items from page " + m_reqItemList.Page + ", " + m_reqItemList.ItemsFavorited.Count + " favorited by user, " + m_reqItemList.ItemsVoted.Count + " voted by user");
					}
				}
				m_reqItemList = null;
				m_pendingRequests.Clear<GetUserItemVoteResult_t>();
			}
		}

		private void OnOwnedSubscribedItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
		{
			if (!CheckAndLogResult<SteamUGCQueryCompleted_t, WorkshopItemListEventArgs>("OnOwnedSubscribedCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnItemListLoaded", ref this.OnItemListLoaded))
			{
				return;
			}
			lock (m_lock)
			{
				m_reqItemList.PagesItems = GetPageCount(p_callback);
				for (uint num = 0u; num < p_callback.m_unNumResultsReturned; num++)
				{
					SteamUGCDetails_t pDetails;
					if (!SteamUGC.GetQueryUGCResult(p_callback.m_handle, num, out pDetails))
					{
						continue;
					}
					WorkshopItem item = ParseItem(p_callback.m_handle, num, pDetails);
					if (!item.IsSubscribed)
					{
						continue;
					}
					if (item.IsOwned)
					{
						m_reqItemList.Items.Add(item);
					}
					m_items[item.SteamNative.m_nPublishedFileId] = item;
					if (SaveDataCtrl.Get(item.SteamNative.m_nPublishedFileId.ToString(), false))
					{
						item.IsActive = SaveDataCtrl.Get(item.SteamNative.m_nPublishedFileId.ToString(), false);
						m_activeItems[item.SteamNative.m_nPublishedFileId] = item;
					}
					string text = "\n";
					if (item.IsActive)
					{
						try
						{
							string[] files = Directory.GetFiles(item.InstalledLocalFolder);
							for (int i = 0; i < files.Length; i++)
							{
								text = text + files[i] + "\n";
							}
						}
						catch
						{
							text += "not found!";
							if (m_items.TryGetValue(item.SteamNative.m_nPublishedFileId, out item))
							{
								EItemState itemState = (EItemState)SteamUGC.GetItemState(item.SteamNative.m_nPublishedFileId);
								item.SteamNative.m_itemState = itemState;
								item.IsInstalled = IsInstalled(itemState);
								item.IsDownloading = IsDownloading(itemState);
								item.IsUpdateNeeded = IsUpdateNeeded(itemState);
								if (item.IsInstalled && SteamUGC.DownloadItem(item.SteamNative.m_nPublishedFileId, true))
								{
									if (base.IsDebugLogEnabled)
									{
										Debug.Log("OnAddItemTo m_Items Completed: started download for missing item " + item.SteamNative.m_nPublishedFileId);
									}
									if (!m_downloadingItems.Contains(item.SteamNative.m_nPublishedFileId))
									{
										m_downloadingItems.Add(item.SteamNative.m_nPublishedFileId);
									}
									itemState = (EItemState)SteamUGC.GetItemState(item.SteamNative.m_nPublishedFileId);
									item.SteamNative.m_itemState = itemState;
									item.IsInstalled = IsInstalled(itemState);
									item.IsDownloading = IsDownloading(itemState);
									item.IsUpdateNeeded = IsUpdateNeeded(itemState);
								}
								else if (base.IsDebugLogEnabled)
								{
									Debug.Log("OnAddItemTo m_Items Completed: item is correctly installed " + item.SteamNative.m_nPublishedFileId);
								}
							}
						}
					}
					item.IsFavorited = m_reqItemList.ItemsFavorited.Where((WorkshopItem flvl) => flvl.SteamNative.m_nPublishedFileId == item.SteamNative.m_nPublishedFileId).FirstOrDefault() != null;
					WorkshopItem workshopItem = m_reqItemList.ItemsVoted.Where((WorkshopItem flvl) => flvl.SteamNative.m_nPublishedFileId == item.SteamNative.m_nPublishedFileId).FirstOrDefault();
					if (workshopItem != null)
					{
						item.IsVotedUp = workshopItem.IsVotedUp;
						item.IsVotedDown = workshopItem.IsVotedDown;
						item.IsVoteSkipped = workshopItem.IsVoteSkipped;
					}
				}
				if (this.OnItemListLoaded != null)
				{
					InvokeEventHandlerSafely(this.OnItemListLoaded, new WorkshopItemListEventArgs
					{
						ItemList = m_reqItemList
					});
					ClearSingleShotEventHandlers("OnItemListLoaded", ref this.OnItemListLoaded);
					if (base.IsDebugLogEnabled)
					{
						Debug.Log("OnAvailableItemsCallCompleted: loaded " + m_reqItemList.Items.Count + " items from page " + m_reqItemList.Page + ", " + m_reqItemList.ItemsFavorited.Count + " favorited by user, " + m_reqItemList.ItemsVoted.Count + " voted by user");
					}
				}
				m_reqItemList = null;
				m_pendingRequests.Clear<GetUserItemVoteResult_t>();
			}
		}

		private void OnFavoriteItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
		{
			if (!CheckAndLogResult<SteamUGCQueryCompleted_t, WorkshopItemListEventArgs>("OnFavoriteItemsCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnItemListLoaded", ref this.OnItemListLoaded))
			{
				return;
			}
			lock (m_lock)
			{
				if (m_reqItemList.PagesItemsFavorited == 0)
				{
					m_reqItemList.PagesItemsFavorited = GetPageCount(p_callback);
				}
				for (uint num = 0u; num < p_callback.m_unNumResultsReturned; num++)
				{
					SteamUGCDetails_t pDetails;
					if (SteamUGC.GetQueryUGCResult(p_callback.m_handle, num, out pDetails))
					{
						WorkshopItem workshopItem = ParseItem(p_callback.m_handle, num, pDetails);
						m_reqItemList.ItemsFavorited.Add(workshopItem);
						m_items[workshopItem.SteamNative.m_nPublishedFileId] = workshopItem;
						workshopItem.IsFavorited = true;
					}
				}
				if (m_reqPage >= m_reqItemList.PagesItemsFavorited)
				{
					if (SteamUser.BLoggedOn())
					{
						QueryVotedItems(1u);
						return;
					}
					QueryAllItems();
					HandleError("OnFavoriteItemsCallCompleted: user is offline, user votes will not be loaded! ", LapinerTools.Steam.Data.ErrorEventArgs.Create(EResult.k_EResultNotLoggedOn));
				}
				else
				{
					QueryFavoritedItems(m_reqPage + 1);
				}
			}
		}

		private void OnSubscribedItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
		{
			if (!CheckAndLogResult<SteamUGCQueryCompleted_t, WorkshopItemListEventArgs>("OnSubscribedItemsCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnItemListLoaded", ref this.OnItemListLoaded))
			{
				return;
			}
			lock (m_lock)
			{
				if (m_reqItemList.PagesItemsFavorited == 0)
				{
					m_reqItemList.PagesItemsFavorited = GetPageCount(p_callback);
				}
				for (uint num = 0u; num < p_callback.m_unNumResultsReturned; num++)
				{
					SteamUGCDetails_t pDetails;
					if (SteamUGC.GetQueryUGCResult(p_callback.m_handle, num, out pDetails))
					{
						WorkshopItem workshopItem = ParseItem(p_callback.m_handle, num, pDetails);
						m_reqItemList.ItemsFavorited.Add(workshopItem);
						m_items[workshopItem.SteamNative.m_nPublishedFileId] = workshopItem;
						workshopItem.IsFavorited = true;
					}
				}
				if (m_reqPage >= m_reqItemList.PagesItemsFavorited)
				{
					if (SteamUser.BLoggedOn())
					{
						QueryVotedItems(1u);
						return;
					}
					QueryAllItems();
					HandleError("OnFavoriteItemsCallCompleted: user is offline, user votes will not be loaded! ", LapinerTools.Steam.Data.ErrorEventArgs.Create(EResult.k_EResultNotLoggedOn));
				}
				else
				{
					QuerySubscribedItems(m_reqPage + 1);
				}
			}
		}

		private void OnPublishedItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
		{
			if (!CheckAndLogResult<SteamUGCQueryCompleted_t, WorkshopItemListEventArgs>("OnPublishedItemsCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnItemListLoaded", ref this.OnItemListLoaded))
			{
				return;
			}
			lock (m_lock)
			{
				if (m_reqItemList.PagesItemsFavorited == 0)
				{
					m_reqItemList.PagesItemsFavorited = GetPageCount(p_callback);
				}
				for (uint num = 0u; num < p_callback.m_unNumResultsReturned; num++)
				{
					SteamUGCDetails_t pDetails;
					if (SteamUGC.GetQueryUGCResult(p_callback.m_handle, num, out pDetails))
					{
						WorkshopItem workshopItem = ParseItem(p_callback.m_handle, num, pDetails);
						m_reqItemList.ItemsFavorited.Add(workshopItem);
						m_items[workshopItem.SteamNative.m_nPublishedFileId] = workshopItem;
						workshopItem.IsFavorited = true;
					}
				}
				if (m_reqPage >= m_reqItemList.PagesItemsFavorited)
				{
					if (SteamUser.BLoggedOn())
					{
						QueryVotedItems(1u);
						return;
					}
					QueryAllItems();
					HandleError("OnFavoriteItemsCallCompleted: user is offline, user votes will not be loaded! ", LapinerTools.Steam.Data.ErrorEventArgs.Create(EResult.k_EResultNotLoggedOn));
				}
				else
				{
					QueryPublishedItems(m_reqPage + 1);
				}
			}
		}

		private void OnOwnedItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
		{
			if (!CheckAndLogResult<SteamUGCQueryCompleted_t, WorkshopItemListEventArgs>("OnOwnedItemsCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnItemListLoaded", ref this.OnItemListLoaded))
			{
				return;
			}
			lock (m_lock)
			{
				if (m_reqItemList.PagesItemsFavorited == 0)
				{
					m_reqItemList.PagesItemsFavorited = GetPageCount(p_callback);
				}
				for (uint num = 0u; num < p_callback.m_unNumResultsReturned; num++)
				{
					SteamUGCDetails_t pDetails;
					if (SteamUGC.GetQueryUGCResult(p_callback.m_handle, num, out pDetails))
					{
						WorkshopItem workshopItem = ParseItem(p_callback.m_handle, num, pDetails);
						m_reqItemList.ItemsFavorited.Add(workshopItem);
						m_items[workshopItem.SteamNative.m_nPublishedFileId] = workshopItem;
						workshopItem.IsFavorited = true;
					}
				}
				if (m_reqPage >= m_reqItemList.PagesItemsFavorited)
				{
					if (SteamUser.BLoggedOn())
					{
						QueryVotedItems(1u);
						return;
					}
					QueryAllItems(EWorkshopSource.OWNED);
					HandleError("OnFavoriteItemsCallCompleted: user is offline, user votes will not be loaded! ", LapinerTools.Steam.Data.ErrorEventArgs.Create(EResult.k_EResultNotLoggedOn));
				}
				else
				{
					QueryOwnedItems(m_reqPage + 1);
				}
			}
		}

		private void OnVotedItemsCallCompleted(SteamUGCQueryCompleted_t p_callback, bool p_bIOFailure)
		{
			if (!CheckAndLogResult<SteamUGCQueryCompleted_t, WorkshopItemListEventArgs>("OnVotedItemsCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnItemListLoaded", ref this.OnItemListLoaded))
			{
				return;
			}
			lock (m_lock)
			{
				if (m_reqItemList.PagesItemsVoted == 0)
				{
					m_reqItemList.PagesItemsVoted = GetPageCount(p_callback);
				}
				if (m_reqPage < m_reqItemList.PagesItemsVoted)
				{
					QueryVotedItems(m_reqPage + 1);
				}
				for (uint num = 0u; num < p_callback.m_unNumResultsReturned; num++)
				{
					SteamUGCDetails_t pDetails;
					if (SteamUGC.GetQueryUGCResult(p_callback.m_handle, num, out pDetails))
					{
						WorkshopItem item = ParseItem(p_callback.m_handle, num, pDetails);
						m_reqItemList.ItemsVoted.Add(item);
						m_items[item.SteamNative.m_nPublishedFileId] = item;
						item.IsFavorited = m_reqItemList.ItemsFavorited.Where((WorkshopItem flvl) => flvl.SteamNative.m_nPublishedFileId == item.SteamNative.m_nPublishedFileId).FirstOrDefault() != null;
						Execute<GetUserItemVoteResult_t>(SteamUGC.GetUserItemVote(pDetails.m_nPublishedFileId), OnUserVoteCallCompleted);
					}
				}
				if (m_pendingRequests.Count<GetUserItemVoteResult_t>() == 0)
				{
					if (base.IsDebugLogEnabled)
					{
						Debug.Log("OnVotedItemsCallCompleted - no user votes found");
					}
					QueryAllItems();
				}
				else if (base.IsDebugLogEnabled)
				{
					Debug.Log("OnVotedItemsCallCompleted - started vote requests: " + m_pendingRequests.Count<GetUserItemVoteResult_t>());
				}
			}
		}

		private void OnUserVoteCallCompleted(GetUserItemVoteResult_t p_callback, bool p_bIOFailure)
		{
			if (CheckAndLogResultNoEvent<GetUserItemVoteResult_t>("OnUserVoteCallCompleted", p_callback.m_eResult, p_bIOFailure))
			{
				lock (m_lock)
				{
					WorkshopItem workshopItem = m_reqItemList.ItemsVoted.Where((WorkshopItem flvl) => flvl.SteamNative.m_nPublishedFileId == p_callback.m_nPublishedFileId).FirstOrDefault();
					if (workshopItem != null)
					{
						workshopItem.IsVotedUp = p_callback.m_bVotedUp;
						workshopItem.IsVotedDown = p_callback.m_bVotedDown;
						workshopItem.IsVoteSkipped = p_callback.m_bVoteSkipped;
					}
				}
			}
			lock (m_lock)
			{
				if (m_pendingRequests.Count<GetUserItemVoteResult_t>() == 0)
				{
					QueryAllItems();
				}
			}
		}

		private void OnSubscribeCallCompleted(RemoteStorageSubscribePublishedFileResult_t p_callback, bool p_bIOFailure)
		{
			if (!CheckAndLogResult<RemoteStorageSubscribePublishedFileResult_t, WorkshopItemEventArgs>("OnSubscribeCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnSubscribed" + p_callback.m_nPublishedFileId, ref this.OnSubscribed))
			{
				return;
			}
			lock (m_lock)
			{
				WorkshopItem value;
				if (m_items.TryGetValue(p_callback.m_nPublishedFileId, out value))
				{
					value.IsSubscribed = true;
					EItemState itemState = (EItemState)SteamUGC.GetItemState(p_callback.m_nPublishedFileId);
					value.SteamNative.m_itemState = itemState;
					value.IsInstalled = IsInstalled(itemState);
					value.IsDownloading = IsDownloading(itemState);
					value.IsUpdateNeeded = IsUpdateNeeded(itemState);
					if ((value.IsUpdateNeeded || !value.IsInstalled) && SteamUGC.DownloadItem(p_callback.m_nPublishedFileId, true))
					{
						if (base.IsDebugLogEnabled)
						{
							Debug.Log("OnSubscribeCallCompleted: started download for " + p_callback.m_nPublishedFileId);
						}
						if (!m_downloadingItems.Contains(p_callback.m_nPublishedFileId))
						{
							m_downloadingItems.Add(p_callback.m_nPublishedFileId);
						}
						itemState = (EItemState)SteamUGC.GetItemState(p_callback.m_nPublishedFileId);
						value.SteamNative.m_itemState = itemState;
						value.IsInstalled = IsInstalled(itemState);
						value.IsDownloading = IsDownloading(itemState);
						value.IsUpdateNeeded = IsUpdateNeeded(itemState);
					}
					else if (base.IsDebugLogEnabled)
					{
						Debug.Log("OnSubscribeCallCompleted: subscribed to already installed item " + p_callback.m_nPublishedFileId);
					}
					if (this.OnSubscribed != null)
					{
						InvokeEventHandlerSafely(this.OnSubscribed, new WorkshopItemEventArgs(value));
						ClearSingleShotEventHandlers("OnSubscribed" + p_callback.m_nPublishedFileId, ref this.OnSubscribed);
					}
				}
				else
				{
					LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = new LapinerTools.Steam.Data.ErrorEventArgs("Could not find item!");
					HandleError("OnSubscribeCallCompleted: failed! ", errorEventArgs);
					if (this.OnSubscribed != null)
					{
						CallSingleShotEventHandlers("OnSubscribed" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorEventArgs), ref this.OnSubscribed);
					}
				}
			}
		}

		private void OnUnsubscribeCallCompleted(RemoteStorageUnsubscribePublishedFileResult_t p_callback, bool p_bIOFailure)
		{
			if (!CheckAndLogResult<RemoteStorageUnsubscribePublishedFileResult_t, WorkshopItemEventArgs>("OnUnsubscribeCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnUnsubscribed" + p_callback.m_nPublishedFileId, ref this.OnUnsubscribed))
			{
				return;
			}
			lock (m_lock)
			{
				WorkshopItem value;
				if (m_items.TryGetValue(p_callback.m_nPublishedFileId, out value))
				{
					value.IsSubscribed = false;
					EItemState itemState = (EItemState)SteamUGC.GetItemState(p_callback.m_nPublishedFileId);
					value.SteamNative.m_itemState = itemState;
					value.IsInstalled = IsInstalled(itemState);
					value.IsDownloading = IsDownloading(itemState);
					value.IsUpdateNeeded = IsUpdateNeeded(itemState);
					if (this.OnUnsubscribed != null)
					{
						InvokeEventHandlerSafely(this.OnUnsubscribed, new WorkshopItemEventArgs(value));
						ClearSingleShotEventHandlers("OnUnsubscribed" + p_callback.m_nPublishedFileId, ref this.OnUnsubscribed);
					}
				}
				else
				{
					LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = new LapinerTools.Steam.Data.ErrorEventArgs("Could not find subscribed item!");
					HandleError("OnUnsubscribeCallCompleted: failed! ", errorEventArgs);
					if (this.OnUnsubscribed != null)
					{
						CallSingleShotEventHandlers("OnUnsubscribed" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorEventArgs), ref this.OnUnsubscribed);
					}
				}
			}
		}

		private void OnFavoriteChangeCallCompleted(UserFavoriteItemsListChanged_t p_callback, bool p_bIOFailure)
		{
			WorkshopItem value;
			m_items.TryGetValue(p_callback.m_nPublishedFileId, out value);
			if (CheckAndLogResultNoEvent<UserFavoriteItemsListChanged_t>("OnFavoriteChangeCallCompleted", p_callback.m_eResult, p_bIOFailure))
			{
				lock (m_lock)
				{
					if (value != null)
					{
						value.IsFavorited = p_callback.m_bWasAddRequest;
						if (value.IsFavorited)
						{
							if (this.OnAddedFavorite != null)
							{
								InvokeEventHandlerSafely(this.OnAddedFavorite, new WorkshopItemEventArgs(value));
								ClearSingleShotEventHandlers("OnAddedFavorite" + p_callback.m_nPublishedFileId, ref this.OnAddedFavorite);
							}
						}
						else if (this.OnRemovedFavorite != null)
						{
							InvokeEventHandlerSafely(this.OnRemovedFavorite, new WorkshopItemEventArgs(value));
							ClearSingleShotEventHandlers("OnRemovedFavorite" + p_callback.m_nPublishedFileId, ref this.OnRemovedFavorite);
						}
					}
					else
					{
						LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = new LapinerTools.Steam.Data.ErrorEventArgs("Could not find changed item!");
						HandleError("OnFavoriteChangeCallCompleted: failed! ", errorEventArgs);
						if (this.OnAddedFavorite != null)
						{
							CallSingleShotEventHandlers("OnAddedFavorite" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorEventArgs), ref this.OnAddedFavorite);
						}
						if (this.OnRemovedFavorite != null)
						{
							CallSingleShotEventHandlers("OnRemovedFavorite" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorEventArgs), ref this.OnRemovedFavorite);
						}
					}
					return;
				}
			}
			LapinerTools.Steam.Data.ErrorEventArgs p_errorEventArgs = LapinerTools.Steam.Data.ErrorEventArgs.Create(p_callback.m_eResult);
			if (this.OnAddedFavorite != null)
			{
				CallSingleShotEventHandlers("OnAddedFavorite" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(p_errorEventArgs), ref this.OnAddedFavorite);
			}
			if (this.OnRemovedFavorite != null)
			{
				CallSingleShotEventHandlers("OnRemovedFavorite" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(p_errorEventArgs), ref this.OnRemovedFavorite);
			}
		}

		private void OnVoteCallCompleted(SetUserItemVoteResult_t p_callback, bool p_bIOFailure)
		{
			if (!CheckAndLogResult<SetUserItemVoteResult_t, WorkshopItemEventArgs>("OnVoteCallCompleted", p_callback.m_eResult, p_bIOFailure, "OnVoted" + p_callback.m_nPublishedFileId, ref this.OnVoted))
			{
				return;
			}
			lock (m_lock)
			{
				WorkshopItem value;
				if (m_items.TryGetValue(p_callback.m_nPublishedFileId, out value))
				{
					value.IsVotedUp = p_callback.m_bVoteUp;
					value.IsVotedDown = !p_callback.m_bVoteUp;
					value.IsVoteSkipped = false;
					if (this.OnVoted != null)
					{
						InvokeEventHandlerSafely(this.OnVoted, new WorkshopItemEventArgs(value));
						ClearSingleShotEventHandlers("OnVoted" + p_callback.m_nPublishedFileId, ref this.OnVoted);
					}
				}
				else
				{
					LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = new LapinerTools.Steam.Data.ErrorEventArgs("Could not find voted item!");
					HandleError("OnVoteCallCompleted: failed! ", errorEventArgs);
					if (this.OnVoted != null)
					{
						CallSingleShotEventHandlers("OnVoted" + p_callback.m_nPublishedFileId, new WorkshopItemEventArgs(errorEventArgs), ref this.OnVoted);
					}
				}
			}
		}

		private void OnCreateItemCompleted(CreateItemResult_t p_callback, bool p_bIOFailure)
		{
			if (p_callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				Application.OpenURL("https://steamcommunity.com/workshop/workshoplegalagreement/");
				LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = LapinerTools.Steam.Data.ErrorEventArgs.CreateWorkshopLegalAgreement();
				HandleError("OnCreateItemCompleted: failed! ", errorEventArgs);
				if (this.OnUploaded != null)
				{
					CallSingleShotEventHandlers("OnUploaded", new WorkshopItemUpdateEventArgs(errorEventArgs), ref this.OnUploaded);
				}
			}
			else if (CheckAndLogResult<SetUserItemVoteResult_t, WorkshopItemUpdateEventArgs>("OnCreateItemCompleted", p_callback.m_eResult, p_bIOFailure, "OnUploaded", ref this.OnUploaded))
			{
				m_uploadItemData.SteamNative.m_nPublishedFileId = p_callback.m_nPublishedFileId;
				Upload(m_uploadItemData, null, true);
			}
		}

		private void OnItemUpdateCompleted(SubmitItemUpdateResult_t p_callback, bool p_bIOFailure)
		{
			if (p_callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				Application.OpenURL("https://steamcommunity.com/workshop/workshoplegalagreement/");
				LapinerTools.Steam.Data.ErrorEventArgs errorEventArgs = LapinerTools.Steam.Data.ErrorEventArgs.CreateWorkshopLegalAgreement();
				HandleError("OnItemUpdateCompleted: failed! ", errorEventArgs);
				if (this.OnUploaded != null)
				{
					CallSingleShotEventHandlers("OnUploaded", new WorkshopItemUpdateEventArgs(errorEventArgs), ref this.OnUploaded);
				}
			}
			else if (CheckAndLogResult<SetUserItemVoteResult_t, WorkshopItemUpdateEventArgs>("OnItemUpdateCompleted (" + m_uploadItemData.Name + ")", p_callback.m_eResult, p_bIOFailure, "OnUploaded", ref this.OnUploaded) && this.OnUploaded != null)
			{
				InvokeEventHandlerSafely(this.OnUploaded, new WorkshopItemUpdateEventArgs
				{
					Item = m_uploadItemData
				});
				ClearSingleShotEventHandlers("OnUploaded", ref this.OnUploaded);
			}
		}

		private WorkshopItem ParseItem(UGCQueryHandle_t p_handle, uint p_indexInHandle, SteamUGCDetails_t p_itemDetails)
		{
			string friendPersonaName = SteamFriends.GetFriendPersonaName(new CSteamID(p_itemDetails.m_ulSteamIDOwner));
			ulong pStatValue;
			if (!SteamUGC.GetQueryUGCStatistic(p_handle, p_indexInHandle, EItemStatistic.k_EItemStatistic_NumFavorites, out pStatValue))
			{
				pStatValue = 0uL;
			}
			ulong pStatValue2;
			if (!SteamUGC.GetQueryUGCStatistic(p_handle, p_indexInHandle, EItemStatistic.k_EItemStatistic_NumSubscriptions, out pStatValue2))
			{
				pStatValue2 = 0uL;
			}
			string pchURL;
			if (!SteamUGC.GetQueryUGCPreviewURL(p_handle, p_indexInHandle, out pchURL, 1024u))
			{
				pchURL = "";
			}
			bool isSubscribed = (SteamUGC.GetItemState(p_itemDetails.m_nPublishedFileId) & 1) != 0;
			EItemState itemState = (EItemState)SteamUGC.GetItemState(p_itemDetails.m_nPublishedFileId);
			DateTime installedTimestamp = DateTime.MinValue;
			ulong punSizeOnDisk;
			string pchFolder;
			uint punTimeStamp;
			if (SteamUGC.GetItemInstallInfo(p_itemDetails.m_nPublishedFileId, out punSizeOnDisk, out pchFolder, 260u, out punTimeStamp))
			{
				installedTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(punTimeStamp).ToLocalTime();
			}
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(p_itemDetails.m_rtimeUpdated).ToLocalTime();
			return new WorkshopItem
			{
				SteamNative = new WorkshopItem.SteamNativeData(p_itemDetails.m_nPublishedFileId)
				{
					m_details = p_itemDetails,
					m_itemState = itemState
				},
				Name = p_itemDetails.m_rgchTitle,
				Description = p_itemDetails.m_rgchDescription,
				OwnerName = friendPersonaName,
				LastUpdated = dateTime.ToShortDateString(),
				IsOwned = (p_itemDetails.m_ulSteamIDOwner == SteamUser.GetSteamID().m_SteamID),
				PreviewImageURL = pchURL,
				VotesUp = p_itemDetails.m_unVotesUp,
				VotesDown = p_itemDetails.m_unVotesDown,
				Subscriptions = pStatValue2,
				Favorites = pStatValue,
				IsSubscribed = isSubscribed,
				IsInstalled = IsInstalled(itemState),
				IsDownloading = IsDownloading(itemState),
				IsUpdateNeeded = IsUpdateNeeded(itemState),
				InstalledLocalFolder = pchFolder,
				InstalledSizeOnDisk = punSizeOnDisk,
				InstalledTimestamp = installedTimestamp
			};
		}

		private bool IsInstalled(EItemState p_itemState)
		{
			return (p_itemState & EItemState.k_EItemStateInstalled) == EItemState.k_EItemStateInstalled && (p_itemState & EItemState.k_EItemStateDownloading) != EItemState.k_EItemStateDownloading && (p_itemState & EItemState.k_EItemStateDownloadPending) != EItemState.k_EItemStateDownloadPending;
		}

		private bool IsDownloading(EItemState p_itemState)
		{
			return (p_itemState & EItemState.k_EItemStateDownloading) == EItemState.k_EItemStateDownloading || (p_itemState & EItemState.k_EItemStateDownloadPending) == EItemState.k_EItemStateDownloadPending;
		}

		private bool IsUpdateNeeded(EItemState p_itemState)
		{
			return (p_itemState & EItemState.k_EItemStateNeedsUpdate) == EItemState.k_EItemStateNeedsUpdate;
		}

		private uint GetPageCount(SteamUGCQueryCompleted_t p_callback)
		{
			if (p_callback.m_unTotalMatchingResults != 0)
			{
				return (uint)Mathf.Ceil((float)p_callback.m_unTotalMatchingResults / 50f);
			}
			return 1u;
		}

		private void QueryFavoritedItems(uint p_page)
		{
			if (base.IsDebugLogEnabled)
			{
				Debug.Log("QueryFavoritedItems page " + p_page);
			}
			m_reqPage = p_page;
			UGCQueryHandle_t handle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Favorited, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppId_t.Invalid, SteamUtils.GetAppID(), m_reqPage);
			if (!m_isSteamCacheEnabled)
			{
				SteamUGC.SetAllowCachedResponse(handle, 0u);
			}
			Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), OnFavoriteItemsCallCompleted);
		}

		private void QuerySubscribedItems(uint p_page)
		{
			if (base.IsDebugLogEnabled)
			{
				Debug.Log("QuerySubscribedItems page " + p_page);
			}
			m_reqPage = p_page;
			UGCQueryHandle_t handle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Subscribed, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppId_t.Invalid, SteamUtils.GetAppID(), m_reqPage);
			if (!m_isSteamCacheEnabled)
			{
				SteamUGC.SetAllowCachedResponse(handle, 0u);
			}
			Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), OnSubscribedItemsCallCompleted);
		}

		private void QueryPublishedItems(uint p_page)
		{
			if (base.IsDebugLogEnabled)
			{
				Debug.Log("QueryPublishedItems page " + p_page);
			}
			m_reqPage = p_page;
			UGCQueryHandle_t handle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppId_t.Invalid, SteamUtils.GetAppID(), m_reqPage);
			if (!m_isSteamCacheEnabled)
			{
				SteamUGC.SetAllowCachedResponse(handle, 0u);
			}
			Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), OnPublishedItemsCallCompleted);
		}

		private void QueryOwnedItems(uint p_page)
		{
			if (base.IsDebugLogEnabled)
			{
				Debug.Log("QueryOwnedItems page " + p_page);
			}
			m_reqPage = p_page;
			UGCQueryHandle_t handle = SteamUGC.CreateQueryAllUGCRequest(m_sorting.MODE, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, AppId_t.Invalid, SteamUtils.GetAppID(), m_reqPage);
			if (m_searchText != null && !string.IsNullOrEmpty(m_searchText.Trim()))
			{
				SteamUGC.SetSearchText(handle, m_searchText);
			}
			if (m_searchTags != null && m_searchTags.Count > 0)
			{
				SteamUGC.SetMatchAnyTag(handle, m_searchMatchAnyTag);
				for (int i = 0; i < m_searchTags.Count; i++)
				{
					SteamUGC.AddRequiredTag(handle, m_searchTags[i]);
				}
			}
			if (!m_isSteamCacheEnabled)
			{
				SteamUGC.SetAllowCachedResponse(handle, 0u);
			}
			Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), OnOwnedItemsCallCompleted);
		}

		private void QueryVotedItems(uint p_page)
		{
			if (base.IsDebugLogEnabled)
			{
				Debug.Log("QueryVotedItems page " + p_page);
			}
			m_reqPage = p_page;
			UGCQueryHandle_t handle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_VotedOn, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, AppId_t.Invalid, SteamUtils.GetAppID(), m_reqPage);
			if (!m_isSteamCacheEnabled)
			{
				SteamUGC.SetAllowCachedResponse(handle, 0u);
			}
			Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), OnVotedItemsCallCompleted);
		}

		private void QueryAllItems(EWorkshopSource sortSource = EWorkshopSource.PUBLIC)
		{
			m_sorting.SOURCE = sortSource;
			lock (m_lock)
			{
				m_reqPage = m_reqItemList.Page;
			}
			if (base.IsDebugLogEnabled)
			{
				Debug.Log(string.Concat("QueryAllItems from ", m_sorting.SOURCE, " page ", m_reqPage));
			}
			UGCQueryHandle_t handle;
			if (m_sorting.SOURCE != EWorkshopSource.SUBSCRIBED)
			{
				handle = SteamUGC.CreateQueryAllUGCRequest(m_sorting.MODE, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, AppId_t.Invalid, SteamUtils.GetAppID(), m_reqPage);
				if (m_searchText != null && !string.IsNullOrEmpty(m_searchText.Trim()))
				{
					SteamUGC.SetSearchText(handle, m_searchText);
				}
				if (m_searchTags != null && m_searchTags.Count > 0)
				{
					SteamUGC.SetMatchAnyTag(handle, m_searchMatchAnyTag);
					for (int i = 0; i < m_searchTags.Count; i++)
					{
						SteamUGC.AddRequiredTag(handle, m_searchTags[i]);
					}
				}
			}
			else
			{
				uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
				Debug.Log("Sub count: " + numSubscribedItems);
				if (numSubscribedItems == 0)
				{
					lock (m_lock)
					{
						m_reqItemList.PagesItems = 0u;
						if (this.OnItemListLoaded != null)
						{
							InvokeEventHandlerSafely(this.OnItemListLoaded, new WorkshopItemListEventArgs
							{
								ItemList = m_reqItemList
							});
							ClearSingleShotEventHandlers("OnItemListLoaded", ref this.OnItemListLoaded);
							if (base.IsDebugLogEnabled)
							{
								Debug.Log("QueryAllItems: no subscribed items");
							}
						}
						m_reqItemList = null;
						m_pendingRequests.Clear<GetUserItemVoteResult_t>();
						return;
					}
				}
				PublishedFileId_t[] pvecPublishedFileID = new PublishedFileId_t[numSubscribedItems];
				numSubscribedItems = Math.Min(numSubscribedItems, SteamUGC.GetSubscribedItems(pvecPublishedFileID, numSubscribedItems));
				handle = SteamUGC.CreateQueryUGCDetailsRequest(pvecPublishedFileID, numSubscribedItems);
			}
			if (!m_isSteamCacheEnabled)
			{
				SteamUGC.SetAllowCachedResponse(handle, 0u);
			}
			Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), OnAvailableItemsCallCompleted);
		}

		private void QueryOwnedSubscribedItems(EWorkshopSource sortSource = EWorkshopSource.PUBLIC)
		{
			m_sorting.SOURCE = EWorkshopSource.SUBSCRIBED;
			Debug.Log("Sort source: " + sortSource);
			lock (m_lock)
			{
				m_reqPage = m_reqItemList.Page;
			}
			if (base.IsDebugLogEnabled)
			{
				Debug.Log(string.Concat("QueryAllItems from ", m_sorting.SOURCE, " page ", m_reqPage));
			}
			UGCQueryHandle_t handle;
			if (m_sorting.SOURCE != EWorkshopSource.SUBSCRIBED)
			{
				Debug.LogError("Not sub: " + m_sorting.SOURCE);
				handle = SteamUGC.CreateQueryAllUGCRequest(m_sorting.MODE, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, AppId_t.Invalid, SteamUtils.GetAppID(), m_reqPage);
				if (m_searchText != null && !string.IsNullOrEmpty(m_searchText.Trim()))
				{
					SteamUGC.SetSearchText(handle, m_searchText);
				}
				if (m_searchTags != null && m_searchTags.Count > 0)
				{
					SteamUGC.SetMatchAnyTag(handle, m_searchMatchAnyTag);
					for (int i = 0; i < m_searchTags.Count; i++)
					{
						SteamUGC.AddRequiredTag(handle, m_searchTags[i]);
					}
				}
			}
			else
			{
				uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
				Debug.Log("Sub count: " + numSubscribedItems);
				if (numSubscribedItems == 0)
				{
					lock (m_lock)
					{
						m_reqItemList.PagesItems = 0u;
						if (this.OnItemListLoaded != null)
						{
							InvokeEventHandlerSafely(this.OnItemListLoaded, new WorkshopItemListEventArgs
							{
								ItemList = m_reqItemList
							});
							ClearSingleShotEventHandlers("OnItemListLoaded", ref this.OnItemListLoaded);
							if (base.IsDebugLogEnabled)
							{
								Debug.Log("QueryAllItems: no subscribed items");
							}
						}
						m_reqItemList = null;
						m_pendingRequests.Clear<GetUserItemVoteResult_t>();
						return;
					}
				}
				PublishedFileId_t[] pvecPublishedFileID = new PublishedFileId_t[numSubscribedItems];
				numSubscribedItems = Math.Min(numSubscribedItems, SteamUGC.GetSubscribedItems(pvecPublishedFileID, numSubscribedItems));
				handle = SteamUGC.CreateQueryUGCDetailsRequest(pvecPublishedFileID, numSubscribedItems);
			}
			if (!m_isSteamCacheEnabled)
			{
				SteamUGC.SetAllowCachedResponse(handle, 0u);
			}
			Execute<SteamUGCQueryCompleted_t>(SteamUGC.SendQueryUGCRequest(handle), OnOwnedSubscribedItemsCallCompleted);
		}

		public void SaveIcon(Sprite sprite, string fileName, string filePath, Action<Texture2D> p_onRenderIconCompleted)
		{
			StartCoroutine(_SaveIcon(sprite, fileName, filePath, p_onRenderIconCompleted));
		}

		private IEnumerator _SaveIcon(Sprite sprite, string fileName, string filePath, Action<Texture2D> p_onRenderIconCompleted)
		{
			yield return new WaitForEndOfFrame();
			if (m_renderedTexture != null)
			{
				UnityEngine.Object.Destroy(m_renderedTexture);
			}
			m_renderedTexture = new Texture2D((int)sprite.textureRect.width, (int)sprite.textureRect.height);
			Color[] pixels = sprite.texture.GetPixels(0, 0, (int)sprite.textureRect.width, (int)sprite.textureRect.height);
			m_renderedTexture.SetPixels(pixels);
			m_renderedTexture.Apply();
			if (!string.IsNullOrEmpty(filePath))
			{
				string directory = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
				File.WriteAllBytes(filePath, m_renderedTexture.EncodeToPNG());
				if (base.IsDebugLogEnabled)
				{
					Debug.Log("RenderIconRoutine saved icon to '" + filePath + "'");
				}
			}
			if (p_onRenderIconCompleted != null)
			{
				p_onRenderIconCompleted(m_renderedTexture);
			}
		}

		private IEnumerator RenderIconRoutine(Camera p_camera, int p_width, int p_height, string p_saveToFilePath, bool p_keepTextureReference, Action<Texture2D> p_onRenderIconCompleted)
		{
			yield return new WaitForEndOfFrame();
			Rect originalPixelRect = p_camera.pixelRect;
			if ((float)p_width > originalPixelRect.width || (float)p_height > originalPixelRect.height)
			{
				Debug.LogError("SteamWorkshopUIUpload: RenderIconRoutine: cannot render icon in given resolution (" + p_width + "," + p_height + "), because it exceeds the current camera's resolution (" + originalPixelRect.width + "," + originalPixelRect.height + ")!");
				p_width = (int)Mathf.Min(p_width, originalPixelRect.width);
				p_height = (int)Mathf.Min(p_height, originalPixelRect.height);
			}
			Rect pixelRect = (p_camera.pixelRect = new Rect(0f, 0f, p_width, p_height));
			p_camera.Render();
			p_camera.pixelRect = originalPixelRect;
			if (m_renderedTexture != null)
			{
				UnityEngine.Object.Destroy(m_renderedTexture);
			}
			m_renderedTexture = new Texture2D(p_width, p_height, TextureFormat.RGB24, false, true);
			m_renderedTexture.ReadPixels(pixelRect, 0, 0, false);
			m_renderedTexture.Apply(false);
			if (!string.IsNullOrEmpty(p_saveToFilePath))
			{
				string directory = Path.GetDirectoryName(p_saveToFilePath);
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
				File.WriteAllBytes(p_saveToFilePath, m_renderedTexture.EncodeToPNG());
				if (base.IsDebugLogEnabled)
				{
					Debug.Log("RenderIconRoutine saved icon to '" + p_saveToFilePath + "'");
				}
			}
			if (p_onRenderIconCompleted != null)
			{
				p_onRenderIconCompleted(m_renderedTexture);
			}
			if (!p_keepTextureReference)
			{
				m_renderedTexture = null;
			}
		}
	}
}
