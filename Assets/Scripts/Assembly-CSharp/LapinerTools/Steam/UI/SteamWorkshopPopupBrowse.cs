using LapinerTools.uMyGUI;
using UnityEngine;

namespace LapinerTools.Steam.UI
{
	public class SteamWorkshopPopupBrowse : uMyGUI_Popup
	{
		[SerializeField]
		protected SteamWorkshopUIBrowse m_browseUI;

		public SteamWorkshopUIBrowse BrowseUI
		{
			get
			{
				return m_browseUI;
			}
		}

		public SteamWorkshopPopupBrowse()
		{
			DestroyOnHide = true;
		}
	}
}
