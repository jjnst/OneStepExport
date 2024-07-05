using System;
using System.Collections;
using LapinerTools.Steam.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LapinerTools.Steam.UI
{
	public class SteamWorkshopItemNode : MonoBehaviour, IScrollHandler, IEventSystemHandler
	{
		public class ItemDataSetEventArgs : EventArgsBase
		{
			public WorkshopItem ItemData { get; set; }

			public SteamWorkshopItemNode ItemUI { get; set; }
		}

		public class SendMessageInitData
		{
			public WorkshopItem Item { get; set; }
		}

		protected SendMessageInitData m_data = null;

		[SerializeField]
		protected TMP_Text m_textName;

		[SerializeField]
		protected TMP_Text m_textDescription;

		[SerializeField]
		protected TMP_Text m_textOwnerName;

		[SerializeField]
		protected TMP_Text m_textLastUpdated;

		[SerializeField]
		protected TMP_Text m_textVotes;

		[SerializeField]
		protected Button m_btnVotesUp;

		[SerializeField]
		protected Button m_btnVotesUpActive;

		[SerializeField]
		protected Button m_btnVotesDown;

		[SerializeField]
		protected Button m_btnVotesDownActive;

		[SerializeField]
		protected TMP_Text m_textFavorites;

		[SerializeField]
		protected Button m_btnFavorites;

		[SerializeField]
		protected Button m_btnFavoritesActive;

		[SerializeField]
		protected TMP_Text m_textSubscriptions;

		[SerializeField]
		protected TMP_Text m_textDownloadProgress;

		[SerializeField]
		protected Button m_btnSubscriptions;

		[SerializeField]
		protected Button m_btnSubscriptionsActive;

		[SerializeField]
		protected Button m_btnExplorer;

		[SerializeField]
		protected Button m_btnIconLink;

		[SerializeField]
		protected Button m_btnLink;

		[SerializeField]
		protected RawImage m_image;

		[SerializeField]
		protected Image m_selectionImage;

		[SerializeField]
		protected Button m_btnDownload;

		[SerializeField]
		protected Button m_btnPlay;

		[SerializeField]
		protected Button m_btnDelete;

		[SerializeField]
		protected bool m_useExplicitNavigation = true;

		[SerializeField]
		protected bool m_improveNavigationFocus = true;

		protected ScrollRect m_parentScroller = null;

		protected UnityWebRequest m_pendingImageDownload = null;

		protected bool isDestroyed = false;

		public RawImage Image
		{
			get
			{
				return m_image;
			}
		}

		public virtual void uMyGUI_TreeBrowser_InitNode(object p_data)
		{
			if (p_data is SendMessageInitData)
			{
				SteamMainBase<SteamWorkshopMain>.Instance.OnInstalled -= OnItemInstalled;
				SteamMainBase<SteamWorkshopMain>.Instance.OnInstalled += OnItemInstalled;
				m_data = (SendMessageInitData)p_data;
				if (m_image != null && m_image.texture == null && m_pendingImageDownload == null)
				{
					StartCoroutine(DownloadPreview(m_data.Item.PreviewImageURL));
				}
				if (m_textName != null)
				{
					m_textName.text = m_data.Item.Name;
				}
				if (m_textDescription != null)
				{
					m_textDescription.text = m_data.Item.Description;
				}
				if (m_textOwnerName != null)
				{
					m_textOwnerName.text = "by " + m_data.Item.OwnerName;
				}
				if (m_textLastUpdated != null)
				{
					m_textLastUpdated.text = m_data.Item.LastUpdated;
				}
				if (m_textVotes != null)
				{
					m_textVotes.text = ((int)(m_data.Item.VotesUp - m_data.Item.VotesDown)).ToString();
				}
				if (m_textFavorites != null)
				{
					m_textFavorites.text = m_data.Item.Favorites.ToString();
				}
				if (m_textSubscriptions != null)
				{
					m_textSubscriptions.text = m_data.Item.Subscriptions.ToString();
				}
				if (m_btnFavorites != null && m_btnFavoritesActive != null)
				{
					m_btnFavorites.gameObject.SetActive(!m_data.Item.IsFavorited);
					m_btnFavoritesActive.gameObject.SetActive(m_data.Item.IsFavorited);
				}
				if (m_btnSubscriptions != null && m_btnSubscriptionsActive != null)
				{
					m_btnSubscriptions.gameObject.SetActive(!m_data.Item.IsSubscribed);
					m_btnSubscriptionsActive.gameObject.SetActive(m_data.Item.IsSubscribed);
				}
				if (m_btnVotesUp != null && m_btnVotesUpActive != null)
				{
					m_btnVotesUp.gameObject.SetActive(!m_data.Item.IsVotedUp);
					m_btnVotesUpActive.gameObject.SetActive(m_data.Item.IsVotedUp);
				}
				if (m_btnVotesDown != null && m_btnVotesDownActive != null)
				{
					m_btnVotesDown.gameObject.SetActive(!m_data.Item.IsVotedDown);
					m_btnVotesDownActive.gameObject.SetActive(m_data.Item.IsVotedDown);
				}
				if (m_btnDownload != null)
				{
					m_btnDownload.gameObject.SetActive(!m_data.Item.IsActive);
				}
				if (m_btnPlay != null)
				{
					m_btnPlay.gameObject.SetActive(m_data.Item.IsActive && m_data.Item.IsInstalled && !m_data.Item.IsDownloading);
				}
				if (m_btnDelete != null)
				{
				}
				if (m_btnExplorer != null)
				{
					m_btnExplorer.gameObject.SetActive(true);
				}
				if (m_btnLink != null)
				{
					m_btnLink.gameObject.SetActive(true);
				}
				if (m_useExplicitNavigation)
				{
					SetNavigationTargetsHorizontal(new Selectable[11]
					{
						m_btnDelete, m_btnVotesUp, m_btnVotesUpActive, m_btnVotesDown, m_btnVotesDownActive, m_btnFavorites, m_btnFavoritesActive, m_btnSubscriptions, m_btnSubscriptionsActive, m_btnPlay,
						m_btnDownload
					});
					StartCoroutine(SetNavigationTargetsVertical());
				}
				if (m_textDownloadProgress != null)
				{
					m_textDownloadProgress.gameObject.SetActive(m_data.Item.IsDownloading);
				}
				if (m_data.Item.IsDownloading)
				{
					StartCoroutine(ShowDownloadProgress());
				}
				SteamWorkshopUIBrowse.Instance.InvokeOnItemDataSet(m_data.Item, this);
			}
			else
			{
				Debug.LogError("SteamWorkshopItemNode: uMyGUI_TreeBrowser_InitNode: expected p_data to be a SteamWorkshopItemNode.SendMessageInitData! p_data: " + p_data);
			}
		}

		public virtual void OnScroll(PointerEventData data)
		{
			if (m_parentScroller == null)
			{
				m_parentScroller = GetComponentInParent<ScrollRect>();
			}
			if (!(m_parentScroller == null))
			{
				m_parentScroller.OnScroll(data);
			}
		}

		public virtual void Select()
		{
			if (m_btnDownload != null && m_btnDownload.gameObject.activeSelf)
			{
				m_btnDownload.Select();
			}
			else if (m_btnPlay != null && m_btnPlay.gameObject.activeSelf)
			{
				m_btnPlay.Select();
			}
		}

		protected virtual void Start()
		{
			if (m_btnFavorites != null && m_btnFavoritesActive != null)
			{
				m_btnFavorites.onClick.AddListener(AddFavorite);
				m_btnFavoritesActive.onClick.AddListener(RemovedFavorite);
			}
			if (m_btnSubscriptions != null && m_btnSubscriptionsActive != null)
			{
				m_btnSubscriptions.onClick.AddListener(Subscribe);
				m_btnSubscriptionsActive.onClick.AddListener(Unsubscribe);
			}
			if (m_btnVotesUp != null && m_btnVotesUpActive != null)
			{
				m_btnVotesUp.onClick.AddListener(VoteUp);
			}
			if (m_btnVotesDown != null && m_btnVotesDownActive != null)
			{
				m_btnVotesDown.onClick.AddListener(VoteDown);
			}
			if (m_btnDownload != null)
			{
				m_btnDownload.onClick.AddListener(OnDownloadBtn);
			}
			if (m_btnPlay != null)
			{
				m_btnPlay.onClick.AddListener(OnPlayBtn);
			}
			if (m_btnDelete != null)
			{
				m_btnDelete.onClick.AddListener(Unsubscribe);
			}
			if (m_btnExplorer != null)
			{
				m_btnExplorer.onClick.AddListener(OnExplorerBtn);
			}
			if (m_btnLink != null)
			{
				m_btnLink.onClick.AddListener(OnLinkBtn);
			}
			if (m_btnIconLink != null)
			{
				m_btnIconLink.onClick.AddListener(OnLinkBtn);
			}
		}

		protected virtual void OnDestroy()
		{
			isDestroyed = true;
			if (m_image != null)
			{
				UnityEngine.Object.Destroy(m_image.texture);
			}
			if (m_pendingImageDownload != null)
			{
				m_pendingImageDownload.Dispose();
				m_pendingImageDownload = null;
			}
			if (SteamMainBase<SteamWorkshopMain>.IsInstanceSet)
			{
				SteamMainBase<SteamWorkshopMain>.Instance.OnInstalled -= OnItemInstalled;
			}
		}

		protected virtual void OnDownloadBtn()
		{
			if (m_data != null)
			{
				SteamWorkshopUIBrowse.Instance.InvokeOnDownloadButtonClick(m_data.Item);
				uMyGUI_TreeBrowser_InitNode(new SendMessageInitData
				{
					Item = m_data.Item
				});
			}
		}

		protected virtual void OnPlayBtn()
		{
			if (m_data != null)
			{
				SteamWorkshopUIBrowse.Instance.InvokeOnPlayButtonClick(m_data.Item);
				uMyGUI_TreeBrowser_InitNode(new SendMessageInitData
				{
					Item = m_data.Item
				});
			}
		}

		protected virtual void Subscribe()
		{
			if (m_data != null)
			{
				SteamMainBase<SteamWorkshopMain>.Instance.Subscribe(m_data.Item, OnItemUpdated(m_btnSubscriptionsActive));
				SteamWorkshopUIBrowse.Instance.InvokeOnSubscribeButtonClick(m_data.Item);
			}
		}

		protected virtual void Unsubscribe()
		{
			if (m_data != null)
			{
				SteamMainBase<SteamWorkshopMain>.Instance.Unsubscribe(m_data.Item, OnItemUpdated(m_btnSubscriptions));
				SteamWorkshopUIBrowse.Instance.InvokeOnUnsubscribeButtonClick(m_data.Item);
			}
		}

		protected virtual void AddFavorite()
		{
			if (m_data != null)
			{
				SteamMainBase<SteamWorkshopMain>.Instance.AddFavorite(m_data.Item, OnItemUpdated(m_btnFavoritesActive));
				SteamWorkshopUIBrowse.Instance.InvokeOnAddFavoriteButtonClick(m_data.Item);
			}
		}

		protected virtual void RemovedFavorite()
		{
			if (m_data != null)
			{
				SteamMainBase<SteamWorkshopMain>.Instance.RemoveFavorite(m_data.Item, OnItemUpdated(m_btnFavorites));
				SteamWorkshopUIBrowse.Instance.InvokeOnRemoveFavoriteButtonClick(m_data.Item);
			}
		}

		protected virtual void VoteUp()
		{
			if (m_data != null)
			{
				SteamMainBase<SteamWorkshopMain>.Instance.Vote(m_data.Item, true, OnItemUpdated(m_btnVotesUpActive));
				SteamWorkshopUIBrowse.Instance.InvokeOnVoteUpButtonClick(m_data.Item);
			}
		}

		protected virtual void VoteDown()
		{
			if (m_data != null)
			{
				SteamMainBase<SteamWorkshopMain>.Instance.Vote(m_data.Item, false, OnItemUpdated(m_btnVotesDownActive));
				SteamWorkshopUIBrowse.Instance.InvokeOnVoteDownButtonClick(m_data.Item);
			}
		}

		protected virtual void OnExplorerBtn()
		{
			if (m_data != null)
			{
				Application.OpenURL("file://" + m_data.Item.InstalledLocalFolder);
			}
		}

		protected virtual void OnLinkBtn()
		{
			if (m_data != null)
			{
				Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=" + m_data.Item.SteamNative.m_nPublishedFileId);
				Debug.Log(string.Concat(m_data.Item.PreviewImageURL, " ", m_data.Item.SteamNative.m_nPublishedFileId, " ", m_data.Item.Name, " ", m_data.Item.OwnerName));
			}
		}

		protected virtual void OnItemInstalled(WorkshopItemEventArgs p_itemArgs)
		{
			OnItemUpdated(m_btnPlay)(p_itemArgs);
		}

		protected virtual Action<WorkshopItemEventArgs> OnItemUpdated(Selectable p_focusWhenDone)
		{
			return delegate(WorkshopItemEventArgs p_itemArgs)
			{
				if (!isDestroyed && m_data != null && !p_itemArgs.IsError && m_data.Item.SteamNative.m_nPublishedFileId == p_itemArgs.Item.SteamNative.m_nPublishedFileId)
				{
					uMyGUI_TreeBrowser_InitNode(new SendMessageInitData
					{
						Item = p_itemArgs.Item
					});
					if (m_improveNavigationFocus && p_focusWhenDone != null)
					{
						p_focusWhenDone.Select();
					}
				}
			};
		}

		protected virtual void SetNavigationTargetsHorizontal(Selectable[] p_horizontalNavOrder)
		{
			for (int i = 0; i < p_horizontalNavOrder.Length; i++)
			{
				Selectable selectable = p_horizontalNavOrder[i];
				if (!(selectable != null))
				{
					continue;
				}
				Navigation navigation = selectable.navigation;
				navigation.mode = Navigation.Mode.Explicit;
				for (int num = i - 1; num >= 0; num--)
				{
					Selectable selectable2 = p_horizontalNavOrder[num];
					if (selectable2 != null && selectable2.gameObject.activeSelf)
					{
						navigation.selectOnLeft = selectable2;
						break;
					}
				}
				for (int j = i + 1; j < p_horizontalNavOrder.Length; j++)
				{
					Selectable selectable3 = p_horizontalNavOrder[j];
					if (selectable3 != null && selectable3.gameObject.activeSelf)
					{
						navigation.selectOnRight = selectable3;
						break;
					}
				}
				selectable.navigation = navigation;
			}
		}

		protected virtual void SetNavigationTargetsVertical(Selectable p_current, Selectable[] p_verticalNavOrder)
		{
			if (p_current == null || !p_current.gameObject.activeSelf)
			{
				return;
			}
			for (int i = 0; i < p_verticalNavOrder.Length; i++)
			{
				Selectable selectable = p_verticalNavOrder[i];
				if (!(selectable != null) || i < 0)
				{
					continue;
				}
				Navigation navigation = selectable.navigation;
				navigation.mode = Navigation.Mode.Explicit;
				for (int num = i - 1; num >= 0; num--)
				{
					Selectable selectable2 = p_verticalNavOrder[num];
					if (selectable2 != null && selectable2.gameObject.activeSelf)
					{
						navigation.selectOnUp = selectable2;
						break;
					}
				}
				for (int j = i + 1; j < p_verticalNavOrder.Length; j++)
				{
					Selectable selectable3 = p_verticalNavOrder[j];
					if (selectable3 != null && selectable3.gameObject.activeSelf)
					{
						navigation.selectOnDown = selectable3;
						break;
					}
				}
				selectable.navigation = navigation;
			}
		}

		protected virtual IEnumerator SetNavigationTargetsVertical()
		{
			yield return new WaitForEndOfFrame();
			if (!(base.transform.parent != null))
			{
				yield break;
			}
			SteamWorkshopItemNode[] allNodes = base.transform.parent.GetComponentsInChildren<SteamWorkshopItemNode>();
			int selfIndex = Array.IndexOf(allNodes, this);
			if (selfIndex >= 0)
			{
				SteamWorkshopItemNode self = allNodes[selfIndex];
				SteamWorkshopItemNode up = ((selfIndex > 0) ? allNodes[selfIndex - 1] : null);
				SteamWorkshopItemNode down = ((selfIndex < allNodes.Length - 1) ? allNodes[selfIndex + 1] : null);
				SetNavigationTargetsVertical(self.m_btnDelete, new Selectable[3]
				{
					up ? up.m_btnDelete : null,
					self.m_btnDelete,
					down ? down.m_btnDelete : null
				});
				SetNavigationTargetsVertical(self.m_btnVotesUp, new Selectable[5]
				{
					up ? up.m_btnVotesUp : null,
					up ? up.m_btnVotesUpActive : null,
					self.m_btnVotesUp,
					down ? down.m_btnVotesUp : null,
					down ? down.m_btnVotesUpActive : null
				});
				SetNavigationTargetsVertical(self.m_btnVotesUpActive, new Selectable[5]
				{
					up ? up.m_btnVotesUp : null,
					up ? up.m_btnVotesUpActive : null,
					self.m_btnVotesUpActive,
					down ? down.m_btnVotesUp : null,
					down ? down.m_btnVotesUpActive : null
				});
				SetNavigationTargetsVertical(self.m_btnVotesDown, new Selectable[5]
				{
					up ? up.m_btnVotesDown : null,
					up ? up.m_btnVotesDownActive : null,
					self.m_btnVotesDown,
					down ? down.m_btnVotesDown : null,
					down ? down.m_btnVotesDownActive : null
				});
				SetNavigationTargetsVertical(self.m_btnVotesDownActive, new Selectable[5]
				{
					up ? up.m_btnVotesDown : null,
					up ? up.m_btnVotesDownActive : null,
					self.m_btnVotesDownActive,
					down ? down.m_btnVotesDown : null,
					down ? down.m_btnVotesDownActive : null
				});
				SetNavigationTargetsVertical(self.m_btnFavorites, new Selectable[5]
				{
					up ? up.m_btnFavorites : null,
					up ? up.m_btnFavoritesActive : null,
					self.m_btnFavorites,
					down ? down.m_btnFavorites : null,
					down ? down.m_btnFavoritesActive : null
				});
				SetNavigationTargetsVertical(self.m_btnFavoritesActive, new Selectable[5]
				{
					up ? up.m_btnFavorites : null,
					up ? up.m_btnFavoritesActive : null,
					self.m_btnFavoritesActive,
					down ? down.m_btnFavorites : null,
					down ? down.m_btnFavoritesActive : null
				});
				SetNavigationTargetsVertical(self.m_btnSubscriptions, new Selectable[5]
				{
					up ? up.m_btnSubscriptions : null,
					up ? up.m_btnSubscriptionsActive : null,
					self.m_btnSubscriptions,
					down ? down.m_btnSubscriptions : null,
					down ? down.m_btnSubscriptionsActive : null
				});
				SetNavigationTargetsVertical(self.m_btnSubscriptionsActive, new Selectable[5]
				{
					up ? up.m_btnSubscriptions : null,
					up ? up.m_btnSubscriptionsActive : null,
					self.m_btnSubscriptionsActive,
					down ? down.m_btnSubscriptions : null,
					down ? down.m_btnSubscriptionsActive : null
				});
				SetNavigationTargetsVertical(self.m_btnPlay, new Selectable[5]
				{
					up ? up.m_btnPlay : null,
					up ? up.m_btnDownload : null,
					self.m_btnPlay,
					down ? down.m_btnPlay : null,
					down ? down.m_btnDownload : null
				});
				SetNavigationTargetsVertical(self.m_btnDownload, new Selectable[5]
				{
					up ? up.m_btnPlay : null,
					up ? up.m_btnDownload : null,
					self.m_btnDownload,
					down ? down.m_btnPlay : null,
					down ? down.m_btnDownload : null
				});
				if (selfIndex == 0 || selfIndex == allNodes.Length - 1)
				{
					yield return new WaitForEndOfFrame();
					SetAutomaticNavigation(m_btnDelete);
					SetAutomaticNavigation(m_btnVotesUp);
					SetAutomaticNavigation(m_btnVotesUpActive);
					SetAutomaticNavigation(m_btnVotesDown);
					SetAutomaticNavigation(m_btnVotesDownActive);
					SetAutomaticNavigation(m_btnFavorites);
					SetAutomaticNavigation(m_btnFavoritesActive);
					SetAutomaticNavigation(m_btnSubscriptions);
					SetAutomaticNavigation(m_btnSubscriptionsActive);
					SetAutomaticNavigation(m_btnPlay);
					SetAutomaticNavigation(m_btnDownload);
				}
			}
		}

		protected virtual void SetAutomaticNavigation(Selectable p_selectable)
		{
			if (p_selectable != null)
			{
				Navigation navigation = p_selectable.navigation;
				navigation.mode = Navigation.Mode.Automatic;
				p_selectable.navigation = navigation;
			}
		}

		protected virtual IEnumerator ShowDownloadProgress()
		{
			while (m_data != null && m_data.Item.IsDownloading)
			{
				if (m_textDownloadProgress != null)
				{
					m_textDownloadProgress.gameObject.SetActive(true);
					m_textDownloadProgress.text = (int)(SteamMainBase<SteamWorkshopMain>.Instance.GetDownloadProgress(m_data.Item) * 100f) + "%";
				}
				yield return new WaitForSeconds(0.4f);
			}
		}

		protected virtual IEnumerator DownloadPreview(string p_URL)
		{
			if (string.IsNullOrEmpty(p_URL))
			{
				yield break;
			}
			using (m_pendingImageDownload = UnityWebRequestTexture.GetTexture(p_URL))
			{
				yield return m_pendingImageDownload.SendWebRequest();
				if (m_pendingImageDownload == null)
				{
					yield break;
				}
				if (m_pendingImageDownload.isNetworkError || m_pendingImageDownload.isHttpError)
				{
					Debug.Log(m_pendingImageDownload.error);
				}
				else if (m_pendingImageDownload.isDone && string.IsNullOrEmpty(m_pendingImageDownload.error))
				{
					if (m_image != null)
					{
						m_image.texture = DownloadHandlerTexture.GetContent(m_pendingImageDownload);
					}
				}
				else
				{
					Debug.LogError("SteamWorkshopItemNode: DownloadPreview: could not load preview image at '" + p_URL + "'\n" + m_pendingImageDownload.error);
				}
				m_pendingImageDownload = null;
			}
		}
	}
}
