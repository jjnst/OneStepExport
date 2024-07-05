using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class NavPanel : SerializedMonoBehaviour
{
	public UIButton defaultButton;

	public UIButton backButton;

	public UIButton originButton;

	public SlideBody slideBody;

	public Animator anim;

	public TMP_Text title;

	public GameObject content;

	public ButtonCtrl btnCtrl;

	public bool open = false;

	public bool deactivateContentOnClose = false;

	public bool disableBackButton = false;

	public float closeDeactivationDelay = 0f;

	private Coroutine co_DeactivateAfterMoving;

	protected virtual void Awake()
	{
		btnCtrl = S.I.btnCtrl;
		anim = GetComponent<Animator>();
		if ((bool)anim)
		{
			anim.updateMode = AnimatorUpdateMode.UnscaledTime;
		}
		if ((bool)content)
		{
			content.SetActive(false);
		}
	}

	public virtual void Open()
	{
		if ((bool)content)
		{
			content.SetActive(true);
		}
		if ((bool)btnCtrl.focusedButton)
		{
			originButton = btnCtrl.focusedButton;
		}
		if ((bool)slideBody)
		{
			slideBody.Show();
		}
		if ((bool)defaultButton)
		{
			btnCtrl.SetFocus(defaultButton);
		}
		if ((bool)anim)
		{
			anim.SetBool("visible", true);
		}
		open = true;
		btnCtrl.AddActivePanel(this);
	}

	public virtual void ClickBack()
	{
		if (!disableBackButton)
		{
			Close();
		}
	}

	public virtual void Close()
	{
		if ((bool)slideBody)
		{
			slideBody.Hide();
		}
		if ((bool)originButton)
		{
			btnCtrl.SetFocus(originButton);
			originButton = null;
		}
		else
		{
			btnCtrl.RemoveFocus();
		}
		if ((bool)content && deactivateContentOnClose && co_DeactivateAfterMoving == null)
		{
			StartCoroutine(_DeactivateContentAfterClose());
		}
		if ((bool)anim)
		{
			anim.SetBool("visible", false);
		}
		open = false;
		btnCtrl.RemoveActivePanel(this);
	}

	public virtual void CloseBase()
	{
		Close();
	}

	public virtual void Toggle()
	{
		if (btnCtrl.IsActivePanel(this))
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	private IEnumerator _DeactivateContentAfterClose()
	{
		while (slideBody.moving)
		{
			yield return null;
		}
		if (closeDeactivationDelay > 0f)
		{
			yield return new WaitForSeconds(closeDeactivationDelay);
		}
		if (!slideBody.onScreen)
		{
			content.SetActive(false);
		}
		co_DeactivateAfterMoving = null;
	}
}
