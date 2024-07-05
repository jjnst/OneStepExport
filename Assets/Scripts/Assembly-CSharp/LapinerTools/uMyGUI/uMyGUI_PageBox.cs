using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LapinerTools.uMyGUI
{
	public class uMyGUI_PageBox : MonoBehaviour
	{
		[SerializeField]
		private Button m_previousButton = null;

		[SerializeField]
		private Button m_nextButton = null;

		[SerializeField]
		private Button m_pageButton = null;

		[SerializeField]
		private int m_pageCount = 1;

		[SerializeField]
		private int m_maxPageBtnCount = 9;

		[SerializeField]
		private int m_selectedPage = 0;

		private RectTransform m_rectTransform = null;

		private RectTransform m_pageButtonTransform;

		private int m_offset = 0;

		private List<Button> m_pageButtons = new List<Button>();

		public int PageCount
		{
			get
			{
				return m_pageCount;
			}
			set
			{
				SetPageCount(value);
			}
		}

		public int MaxPageBtnCount
		{
			get
			{
				return m_maxPageBtnCount;
			}
			set
			{
				m_maxPageBtnCount = value;
				SetPageCount(PageCount);
			}
		}

		public int SelectedPage
		{
			get
			{
				return m_selectedPage;
			}
			set
			{
				SelectPage(value);
			}
		}

		public RectTransform RTransform
		{
			get
			{
				return (m_rectTransform != null) ? m_rectTransform : (m_rectTransform = GetComponent<RectTransform>());
			}
		}

		private RectTransform PageButtonTransform
		{
			get
			{
				return (m_pageButtonTransform != null || m_pageButton == null) ? m_pageButtonTransform : (m_pageButtonTransform = m_pageButton.GetComponent<RectTransform>());
			}
		}

		public event Action<int> OnPageSelected;

		public void SetPageCount(int p_newPageCount)
		{
			m_pageCount = Mathf.Max(1, p_newPageCount);
			if (p_newPageCount <= 1)
			{
				base.gameObject.SetActive(false);
			}
			else if (m_pageButton != null)
			{
				base.gameObject.SetActive(true);
				UpdateUI();
			}
			else
			{
				Debug.LogError("uMyGUI_PageBox: SetPageCount: m_pageButton must be set in the inspector!");
			}
		}

		public void SelectPageAndCenterOffset(int p_selectedPage)
		{
			m_offset = Mathf.Min(m_pageCount - m_maxPageBtnCount, Mathf.Max(0, p_selectedPage - 1 - m_maxPageBtnCount / 2));
			SelectPage(p_selectedPage);
		}

		public void SelectPage(int p_selectedPage)
		{
			int num = Mathf.Clamp(p_selectedPage, 0, m_pageCount);
			bool flag = num != m_selectedPage;
			m_selectedPage = num;
			UpdateUI();
			if (flag && this.OnPageSelected != null)
			{
				this.OnPageSelected(p_selectedPage);
			}
		}

		public void UpdateUI()
		{
			Clear();
			int num = Mathf.Min(m_pageCount, m_maxPageBtnCount);
			float size = GetWidth(m_previousButton) + GetWidth(m_nextButton) + GetWidth(m_pageButton) * (float)num;
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
			int num2 = Mathf.Max(0, m_selectedPage - m_maxPageBtnCount);
			int num3 = Mathf.Max(0, Mathf.Min(m_pageCount - m_maxPageBtnCount, m_selectedPage - 1));
			if (num2 - 1 >= m_offset)
			{
				m_offset = num2;
			}
			else if (num3 + 1 <= m_offset)
			{
				m_offset = num3;
			}
			SetText(m_pageButton, (1 + m_offset).ToString());
			SetOnClick(m_pageButton, 1 + m_offset);
			for (int i = 2; i <= num; i++)
			{
				Button button = UnityEngine.Object.Instantiate(m_pageButton);
				RectTransform component = button.GetComponent<RectTransform>();
				component.SetParent(PageButtonTransform.parent, true);
				component.localScale = PageButtonTransform.localScale;
				component.localPosition = PageButtonTransform.localPosition + Vector3.right * (i - 1) * GetWidth(m_pageButton);
				SetText(button, (i + m_offset).ToString());
				SetOnClick(button, i + m_offset);
				m_pageButtons.Add(button);
			}
			for (int j = 0; j < m_pageButtons.Count; j++)
			{
				int num4 = j + 1 + m_offset;
				m_pageButtons[j].enabled = num4 != m_selectedPage;
			}
			if (m_nextButton != null)
			{
				m_nextButton.GetComponent<RectTransform>().localPosition = PageButtonTransform.localPosition + Vector3.right * num * GetWidth(m_pageButton);
			}
		}

		private void Start()
		{
			SetPageCount(m_pageCount);
			if (m_previousButton != null)
			{
				m_previousButton.onClick.AddListener(delegate
				{
					SelectPageAndCenterOffset(Mathf.Max(1, m_selectedPage - 1));
				});
			}
			if (m_nextButton != null)
			{
				m_nextButton.onClick.AddListener(delegate
				{
					SelectPageAndCenterOffset(m_selectedPage + 1);
				});
			}
		}

		private void SetText(Button p_button, string p_text)
		{
			TMP_Text componentInChildren = p_button.GetComponentInChildren<TMP_Text>();
			if (componentInChildren != null)
			{
				componentInChildren.text = p_text;
			}
		}

		private void SetOnClick(Button p_button, int p_pageNumber)
		{
			p_button.onClick.RemoveAllListeners();
			p_button.onClick.AddListener(delegate
			{
				SelectPage(p_pageNumber);
			});
		}

		private float GetWidth(Button p_button)
		{
			return (p_button != null) ? GetWidth(p_button.GetComponent<RectTransform>()) : 0f;
		}

		private float GetWidth(RectTransform p_rTransform)
		{
			return (p_rTransform != null) ? (p_rTransform.rect.xMax - p_rTransform.rect.xMin) : 0f;
		}

		private void Clear()
		{
			for (int i = 0; i < m_pageButtons.Count; i++)
			{
				if (m_pageButtons[i] != null && m_pageButtons[i] != m_pageButton)
				{
					UnityEngine.Object.Destroy(m_pageButtons[i].gameObject);
				}
			}
			m_pageButtons.Clear();
			m_pageButtons.Add(m_pageButton);
		}
	}
}
