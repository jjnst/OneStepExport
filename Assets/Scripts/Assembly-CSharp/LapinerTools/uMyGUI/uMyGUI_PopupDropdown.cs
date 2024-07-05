using System;
using System.Collections;
using UnityEngine;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_PopupDropdown : uMyGUI_PopupText
	{
		[SerializeField]
		protected uMyGUI_Dropdown m_dropdown;

		public NavPanel navPanel;

		protected Action<int> m_onSelected;

		public override void Show()
		{
			base.Show();
			if (m_dropdown != null)
			{
				m_dropdown.Select(-1);
			}
			navPanel.Open();
			navPanel.btnCtrl.RemoveFocus();
			navPanel.originButton = S.I.modCtrl.updateModButton;
		}

		public uMyGUI_PopupDropdown ShowEntries()
		{
			m_dropdown.ShowEntries();
			return this;
		}

		public override void Hide()
		{
			navPanel.Close();
			base.Hide();
			if (m_dropdown != null && m_onSelected != null)
			{
				m_dropdown.OnSelected -= m_onSelected;
				m_onSelected = null;
			}
		}

		public virtual uMyGUI_PopupDropdown SetEntries(string[] p_entries)
		{
			if (m_dropdown != null)
			{
				m_dropdown.Entries = p_entries;
			}
			StartCoroutine(ShowEntriesNext());
			return this;
		}

		public virtual uMyGUI_PopupDropdown SetOnSelected(Action<int> p_onSelected)
		{
			if (m_dropdown != null)
			{
				m_onSelected = p_onSelected;
				m_dropdown.OnSelected += p_onSelected;
			}
			return this;
		}

		private IEnumerator ShowEntriesNext()
		{
			yield return null;
			ShowEntries();
		}
	}
}
