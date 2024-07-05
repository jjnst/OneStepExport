using TMPro;
using UnityEngine.EventSystems;

public class NavTextfield : UIButton
{
	public TMP_InputField inputField;

	public TMP_Text label;

	public bool disableVerticalNav = false;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update()
	{
		base.Update();
		if (hovering)
		{
			canvasGroup.alpha = 1f;
		}
	}

	public override void Left()
	{
	}

	public override void Right()
	{
	}

	public override void Up()
	{
		if (!disableVerticalNav)
		{
			base.Up();
		}
	}

	public override void Down()
	{
		if (!disableVerticalNav)
		{
			base.Down();
		}
	}

	public override void OnBackPress()
	{
	}

	public override void OnAcceptPress()
	{
	}

	public override void Focus(int playerNum = 0)
	{
		canvasGroup.alpha = 1f;
		if (inputField.interactable && (bool)inputField)
		{
			inputField.Select();
		}
	}

	public override void UnFocus()
	{
		canvasGroup.alpha = 0.7f;
		EventSystem.current.SetSelectedGameObject(null);
	}
}
