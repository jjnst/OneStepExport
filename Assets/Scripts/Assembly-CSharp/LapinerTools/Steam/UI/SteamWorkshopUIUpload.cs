using System;
using System.Collections;
using System.IO;
using I2.Loc;
using LapinerTools.Steam.Data;
using LapinerTools.uMyGUI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace LapinerTools.Steam.UI
{
	public class SteamWorkshopUIUpload : MonoBehaviour
	{
		protected static SteamWorkshopUIUpload s_instance;

		[SerializeField]
		protected int ICON_WIDTH = 512;

		[SerializeField]
		protected int ICON_HEIGHT = 512;

		[SerializeField]
		protected TMP_InputField NAME_INPUT = null;

		[SerializeField]
		protected TMP_InputField DESCRIPTION_INPUT = null;

		[SerializeField]
		protected RawImage ICON = null;

		[SerializeField]
		protected Button SCREENSHOT_BUTTON = null;

		[SerializeField]
		protected Button UPLOAD_BUTTON = null;

		[SerializeField]
		protected bool m_improveNavigationFocus = true;

		protected bool m_isUploading = false;

		protected UnityWebRequest m_pendingImageDownload = null;

		protected WorkshopItemUpdate m_itemData = new WorkshopItemUpdate();

		public static SteamWorkshopUIUpload Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = UnityEngine.Object.FindObjectOfType<SteamWorkshopUIUpload>();
				}
				return s_instance;
			}
		}

		public event Action<string> OnNameSet;

		public event Action<string> OnDescriptionSet;

		public event Action<string> OnIconFilePathSet;

		public event Action<Texture2D> OnIconTextureSet;

		public event Action<WorkshopItemUpdateEventArgs> OnStartedUpload;

		public event Action<WorkshopItemUpdateEventArgs> OnFinishedUpload;

		public virtual void SetItemData(WorkshopItemUpdate p_itemData)
		{
			m_itemData = ((p_itemData != null) ? p_itemData : new WorkshopItemUpdate());
			if (m_itemData.Name == null)
			{
				m_itemData.Name = "";
			}
			if (m_itemData.Description == null)
			{
				m_itemData.Description = "";
			}
			m_itemData.IconPath = p_itemData.IconPath;
			if (NAME_INPUT != null)
			{
				NAME_INPUT.text = m_itemData.Name;
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: SetItemData: NAME_INPUT is not set in inspector!");
			}
			if (DESCRIPTION_INPUT != null)
			{
				if (!string.IsNullOrEmpty(m_itemData.Description))
				{
					StartCoroutine(SetDescriptionSafe(m_itemData.Description));
				}
				else
				{
					DESCRIPTION_INPUT.text = "";
				}
			}
			Debug.Log(m_itemData.Name + " is the set name");
		}

		protected virtual void Start()
		{
			SteamMainBase<SteamWorkshopMain>.Instance.OnUploaded += ShowSuccessMessage;
			SteamMainBase<SteamWorkshopMain>.Instance.OnError += ShowErrorMessage;
			if (NAME_INPUT != null)
			{
				NAME_INPUT.onEndEdit.AddListener(OnEditName);
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: NAME_INPUT is not set in inspector!");
			}
			if (DESCRIPTION_INPUT != null)
			{
				DESCRIPTION_INPUT.onEndEdit.AddListener(OnEditDescription);
			}
			if (UPLOAD_BUTTON != null)
			{
				UPLOAD_BUTTON.onClick.AddListener(OnUploadButtonClick);
			}
			else
			{
				Debug.LogError("SteamWorkshopUIUpload: UPLOAD_BUTTON is not set in inspector!");
			}
		}

		protected virtual void LateUpdate()
		{
			if (!m_improveNavigationFocus)
			{
				return;
			}
			EventSystem current = EventSystem.current;
			if (current != null && (current.currentSelectedGameObject == null || !current.currentSelectedGameObject.activeInHierarchy))
			{
				if (current.firstSelectedGameObject != null && current.firstSelectedGameObject.activeInHierarchy)
				{
					current.SetSelectedGameObject(current.firstSelectedGameObject);
				}
				else if (NAME_INPUT != null)
				{
					NAME_INPUT.Select();
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (ICON != null)
			{
				UnityEngine.Object.Destroy(ICON.texture);
			}
			if (m_pendingImageDownload != null)
			{
				m_pendingImageDownload.Dispose();
				m_pendingImageDownload = null;
			}
			if (SteamMainBase<SteamWorkshopMain>.IsInstanceSet)
			{
				SteamMainBase<SteamWorkshopMain>.Instance.OnUploaded -= ShowSuccessMessage;
				SteamMainBase<SteamWorkshopMain>.Instance.OnError -= ShowErrorMessage;
			}
		}

		protected virtual void OnEditName(string p_name)
		{
			m_itemData.Name = p_name;
			InvokeEventHandlerSafely(this.OnNameSet, p_name);
		}

		protected virtual void OnEditDescription(string p_description)
		{
			m_itemData.Description = p_description;
			InvokeEventHandlerSafely(this.OnDescriptionSet, p_description);
		}

		protected virtual void OnScreenshotButtonClick()
		{
			if (string.IsNullOrEmpty(m_itemData.ContentPath))
			{
				m_itemData.ContentPath = Path.Combine(ModCtrl.YOUR_MODS_PATH, m_itemData.Name);
			}
			string iconFilePath = Path.Combine(m_itemData.ContentPath, "workshopIcon.png");
			SteamMainBase<SteamWorkshopMain>.Instance.RenderIcon(Camera.main, ICON_WIDTH, ICON_HEIGHT, iconFilePath, delegate(Texture2D p_renderedIcon)
			{
				m_itemData.IconPath = iconFilePath;
				InvokeEventHandlerSafely(this.OnIconFilePathSet, iconFilePath);
				if (ICON != null)
				{
					ICON.texture = p_renderedIcon;
					InvokeEventHandlerSafely(this.OnIconTextureSet, p_renderedIcon);
				}
				else
				{
					Debug.LogError("SteamWorkshopUIUpload: OnScreenshotButtonClick: ICON is not set in inspector!");
				}
			});
		}

		protected virtual void OnUploadButtonClick()
		{
			if (string.IsNullOrEmpty(m_itemData.Name))
			{
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Invalid Item Name", "Please give your item a non-empty name!").ShowButton("ok");
				return;
			}
			if (string.IsNullOrEmpty(m_itemData.ContentPath))
			{
				m_itemData.ContentPath = Path.Combine(ModCtrl.YOUR_MODS_PATH, m_itemData.Name);
			}
			if (!Directory.Exists(m_itemData.ContentPath))
			{
				Directory.CreateDirectory(m_itemData.ContentPath);
			}
			string contents = "Save your item/level/mod data here.\nIt does not need to be a text file. Any file type is supported (binary, images, etc...).\nYou can save multiple files, Steam items are folders (not single files).\n";
			File.WriteAllText(Path.Combine(m_itemData.ContentPath, "ItemData.txt"), contents);
			string iconFilePath = Path.Combine(m_itemData.ContentPath, "workshopIcon.png");
			if (string.IsNullOrEmpty(m_itemData.IconPath))
			{
				SteamMainBase<SteamWorkshopMain>.Instance.SaveIcon(S.I.itemMan.GetSprite("ModIcon"), "Icon", iconFilePath, delegate
				{
					m_itemData.IconPath = iconFilePath;
					InvokeEventHandlerSafely(this.OnIconFilePathSet, iconFilePath);
				});
			}
			Debug.Log("Uploading content from: " + m_itemData.ContentPath);
			WorkshopItemUpdate workshopItemUpdate = new WorkshopItemUpdate();
			workshopItemUpdate.ContentPath = m_itemData.ContentPath;
			workshopItemUpdate.Name = m_itemData.Name;
			workshopItemUpdate.Description = m_itemData.Description;
			workshopItemUpdate.IconPath = m_itemData.IconPath;
			Instance.SetItemData(workshopItemUpdate);
			m_isUploading = true;
			StartCoroutine(ShowUploadProgress());
			SteamMainBase<SteamWorkshopMain>.Instance.Upload(m_itemData, null);
			if (this.OnStartedUpload != null)
			{
				this.OnStartedUpload(new WorkshopItemUpdateEventArgs
				{
					Item = m_itemData
				});
			}
		}

		protected virtual void ShowSuccessMessage(WorkshopItemUpdateEventArgs p_successArgs)
		{
			m_isUploading = false;
			if (!p_successArgs.IsError && p_successArgs.Item != null)
			{
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("upload")).SetText(ScriptLocalization.UI.Mods_ModUploaded, "'" + p_successArgs.Item.Name + "' " + ScriptLocalization.UI.Mods_WasSucessfullyUploaded).ShowButton("ok");
			}
			S.I.modCtrl.uploadPane.originButton = S.I.modCtrl.newModButton;
			S.I.modCtrl.uploadPane.Close();
			S.I.modCtrl.updatePane.Close();
			if (this.OnFinishedUpload != null)
			{
				this.OnFinishedUpload(p_successArgs);
			}
		}

		protected virtual void ShowErrorMessage(LapinerTools.Steam.Data.ErrorEventArgs p_errorArgs)
		{
			m_isUploading = false;
			((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText("Steam Error", p_errorArgs.ErrorMessage).ShowButton("ok");
		}

		protected virtual void InvokeEventHandlerSafely<T>(Action<T> p_handler, T p_data)
		{
			try
			{
				if (p_handler != null)
				{
					p_handler(p_data);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat("SteamWorkshopUIUpload: your event handler (", p_handler.Target, " - System.Action<", typeof(T), ">) has thrown an excepotion!\n", ex));
			}
		}

		protected virtual IEnumerator ShowUploadProgress()
		{
			while (m_itemData != null && m_isUploading)
			{
				float progress = SteamMainBase<SteamWorkshopMain>.Instance.GetUploadProgress(m_itemData);
				((uMyGUI_PopupText)uMyGUI_PopupManager.Instance.ShowPopup("text")).SetText(ScriptLocalization.UI.Mods_UploadingMod, "<size=32>" + (int)(progress * 100f) + "%</size>");
				yield return new WaitForSeconds(0.2f);
			}
			uMyGUI_PopupManager.Instance.HidePopup("text");
		}

		protected virtual IEnumerator SetDescriptionSafe(string p_description)
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			if (DESCRIPTION_INPUT != null)
			{
				DESCRIPTION_INPUT.text = p_description;
			}
		}

		protected virtual IEnumerator LoadIcon(string p_filePath)
		{
			if (string.IsNullOrEmpty(p_filePath))
			{
				yield break;
			}
			using (m_pendingImageDownload = UnityWebRequestTexture.GetTexture("file:///" + p_filePath))
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
					if (ICON != null)
					{
						ICON.texture = DownloadHandlerTexture.GetContent(m_pendingImageDownload);
					}
				}
				else
				{
					Debug.LogError("SteamWorkshopUIUpload: LoadIcon: could not load icon at '" + p_filePath + "'\n" + m_pendingImageDownload.error);
				}
				m_pendingImageDownload = null;
			}
		}
	}
}
