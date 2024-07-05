using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public UIButton up = null;

	public UIButton down = null;

	public UIButton left = null;

	public UIButton right = null;

	public UIButton link = null;

	public UIButton back = null;

	public UIButton playerTwoBack = null;

	public Animator anim;

	public TMP_Text tmpText;

	public Image image;

	protected Button button;

	public UnityEvent onAcceptPress;

	public ButtonCtrl btnCtrl;

	public float defaultSize = 1f;

	public float downSize = 1.08f;

	public float hoverSize = 1.1f;

	public float damping = 0.05f;

	protected Vector3 currentVelocity;

	public float hoverAlpha = 1f;

	public bool hovering = false;

	public bool holdingDown = false;

	public float targetSize = 1f;

	public RectTransform rect;

	public CanvasGroup canvasGroup;

	public bool hoverable = true;

	public List<UIButton> navList;

	public bool useParentTransformNav = false;

	public bool setFocusOnHover = true;

	public bool functionOnFocus = false;

	public bool holdToAccept = false;

	public float holdDurationOverride = 0f;

	public Image holdCircle;

	public bool holding = false;

	protected virtual void Awake()
	{
		btnCtrl = S.I.btnCtrl;
		if (anim == null)
		{
			anim = GetComponent<Animator>();
		}
		rect = GetComponent<RectTransform>();
		if ((bool)GetComponent<Button>())
		{
			button = GetComponent<Button>();
		}
		if ((bool)GetComponent<CanvasGroup>())
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}
		else
		{
			canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		}
		UnFocus();
	}

	protected virtual void Update()
	{
		SetTargetSize();
		Scale(rect);
		if (!holdToAccept)
		{
			return;
		}
		if (holding || holdingDown)
		{
			if (holdCircle.fillAmount < 1f)
			{
				if (holdDurationOverride > 0f)
				{
					holdCircle.fillAmount += Time.deltaTime / holdDurationOverride;
				}
				else
				{
					holdCircle.fillAmount += Time.deltaTime / btnCtrl.holdDuration;
				}
			}
			else
			{
				OnAcceptPress();
				holdCircle.fillAmount = 0f;
			}
			holding = false;
		}
		else if (holdCircle.fillAmount > 0f)
		{
			holdCircle.fillAmount -= Time.deltaTime / btnCtrl.holdDuration;
		}
	}

	protected void SetTargetSize()
	{
		if ((hovering && btnCtrl.mouseActive) || btnCtrl.focusedButton == this)
		{
			if (holdingDown)
			{
				targetSize = downSize;
			}
			else
			{
				targetSize = hoverSize;
			}
		}
		else
		{
			targetSize = defaultSize;
		}
	}

	protected void Scale(RectTransform scalable)
	{
		if ((bool)scalable && scalable.localScale.x != targetSize)
		{
			Vector3 localScale = Vector3.SmoothDamp(scalable.localScale, Vector3.one * targetSize, ref currentVelocity, damping, 999f, Time.unscaledDeltaTime);
			scalable.localScale = localScale;
		}
	}

	public virtual void Up()
	{
		if ((bool)up && up.gameObject.activeInHierarchy)
		{
			S.I.PlayOnce(btnCtrl.hoverSound);
			btnCtrl.SetFocus(up);
		}
		else if (navList != null && navList.Count > 0)
		{
			S.I.PlayOnce(btnCtrl.hoverSound);
			if (navList.IndexOf(this) > 0)
			{
				btnCtrl.SetFocus(navList[navList.IndexOf(this) - 1]);
			}
			else
			{
				btnCtrl.SetFocus(navList[navList.Count - 1]);
			}
		}
		else
		{
			if (!useParentTransformNav)
			{
				return;
			}
			int siblingIndex = base.transform.GetSiblingIndex();
			int num = siblingIndex - 1;
			if (num < 0)
			{
				num = base.transform.parent.childCount - 1;
			}
			while (num != siblingIndex && !base.transform.parent.GetChild(num).gameObject.activeInHierarchy)
			{
				num--;
				if (num < 0)
				{
					num = base.transform.parent.childCount - 1;
				}
			}
			if (base.transform.parent.GetChild(num) != base.transform)
			{
				S.I.PlayOnce(btnCtrl.hoverSound);
			}
			btnCtrl.SetFocus(base.transform.parent.GetChild(num));
		}
	}

	public virtual void Down()
	{
		if ((bool)down && down.gameObject.activeInHierarchy)
		{
			S.I.PlayOnce(btnCtrl.hoverSound);
			btnCtrl.SetFocus(down);
		}
		else if (navList != null && navList.Count > 0)
		{
			S.I.PlayOnce(btnCtrl.hoverSound);
			if (navList.IndexOf(this) < navList.Count - 1)
			{
				btnCtrl.SetFocus(navList[navList.IndexOf(this) + 1]);
			}
			else
			{
				btnCtrl.SetFocus(navList[0]);
			}
		}
		else
		{
			if (!useParentTransformNav)
			{
				return;
			}
			int siblingIndex = base.transform.GetSiblingIndex();
			int num = siblingIndex + 1;
			if (num >= base.transform.parent.childCount)
			{
				num = 0;
			}
			while (num != siblingIndex && !base.transform.parent.GetChild(num).gameObject.activeInHierarchy)
			{
				num++;
				if (num >= base.transform.parent.childCount)
				{
					num = 0;
				}
			}
			if (base.transform.parent.GetChild(num) != base.transform)
			{
				S.I.PlayOnce(btnCtrl.hoverSound);
			}
			btnCtrl.SetFocus(base.transform.parent.GetChild(num));
		}
	}

	public virtual void Left()
	{
		if ((bool)left && left.gameObject.activeInHierarchy)
		{
			S.I.PlayOnce(btnCtrl.hoverSound);
			btnCtrl.SetFocus(left);
		}
	}

	public virtual void Right()
	{
		if ((bool)right && right.gameObject.activeInHierarchy)
		{
			S.I.PlayOnce(btnCtrl.hoverSound);
			btnCtrl.SetFocus(right);
		}
	}

	public bool IsHoverable()
	{
		if (btnCtrl.mouseActive && hoverable && btnCtrl.NoTransitions())
		{
			return true;
		}
		return false;
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (IsHoverable())
		{
			hovering = true;
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		hovering = false;
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (btnCtrl.focusedButton != this && setFocusOnHover)
			{
				btnCtrl.SetFocus(this);
			}
			if (button == null || button.onClick.GetPersistentEventCount() <= 0)
			{
				OnAcceptPress();
			}
			else
			{
				S.I.PlayOnce(btnCtrl.pierceSound);
			}
		}
	}

	public virtual void OnPointerDown(PointerEventData data)
	{
		holdingDown = true;
	}

	public virtual void OnPointerUp(PointerEventData data)
	{
		holdingDown = false;
	}

	public virtual void OnAcceptPress()
	{
		if (holdToAccept && (bool)holdCircle && holdCircle.fillAmount < 1f)
		{
			return;
		}
		if ((bool)holdCircle)
		{
			holdCircle.fillAmount = 0f;
		}
		if (!functionOnFocus)
		{
			S.I.PlayOnce(btnCtrl.pierceSound);
		}
		bool flag = false;
		for (int i = 0; i < onAcceptPress.GetPersistentEventCount(); i++)
		{
			if (onAcceptPress.GetPersistentTarget(i) != null)
			{
				onAcceptPress.Invoke();
				flag = true;
			}
		}
		if (!flag)
		{
			if ((bool)button && button.onClick != null && button.onClick.GetPersistentEventCount() > 0)
			{
				button.onClick.Invoke();
			}
			else if ((bool)GetComponent<EventTrigger>())
			{
				GetComponent<EventTrigger>().OnPointerClick(null);
			}
			holding = false;
			holdingDown = false;
		}
	}

	public virtual void OnAcceptHold()
	{
		if (holdToAccept)
		{
			holding = true;
		}
	}

	public void RemoveDirectionalNavigation()
	{
		up = null;
		down = null;
		right = null;
		left = null;
	}

	public virtual void OnBackPress()
	{
		if (btnCtrl.lastInputPlayerIndex == 0)
		{
			if ((bool)back)
			{
				btnCtrl.SetFocus(back, btnCtrl.lastInputPlayerIndex);
			}
		}
		else if (btnCtrl.lastInputPlayerIndex == 1 && (bool)playerTwoBack)
		{
			btnCtrl.SetFocus(playerTwoBack, btnCtrl.lastInputPlayerIndex);
		}
	}

	public virtual void OnWeaponPress()
	{
	}

	public virtual void OnWeaponRelease()
	{
	}

	public virtual void OnWeaponHold()
	{
	}

	public virtual void OnShufflePress()
	{
	}

	public virtual void OnOutfitPress()
	{
	}

	public virtual void RemoveSpellPress()
	{
	}

	public virtual void RemoveSpellHold()
	{
	}

	public virtual void UpgradeSpellPress()
	{
	}

	public virtual void UpgradeSpellHold()
	{
	}

	public virtual void ChooseZonePress()
	{
	}

	public virtual void Focus(int playerNum = 0)
	{
		if (functionOnFocus)
		{
			OnAcceptPress();
		}
	}

	public virtual void UnFocus()
	{
	}
}
