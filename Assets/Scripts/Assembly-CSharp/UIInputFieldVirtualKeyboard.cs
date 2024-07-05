using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_InputField))]
public class UIInputFieldVirtualKeyboard : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, ISubmitHandler
{
	[SerializeField]
	private bool showKeyboardOnFocus = false;

	private bool localInputCaptureInProgress = false;

	private TMP_InputField inputField = null;

	private bool shouldActivate = false;

	public static bool inputCaptureInProgress { get; private set; }

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			TryActivateKeyboard();
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		if (!inputField.isFocused)
		{
			TryActivateKeyboard();
		}
	}

	public void TryActivateKeyboard()
	{
		if (inputField.IsActive() && inputField.IsInteractable())
		{
			shouldActivate = true;
		}
	}

	protected void Awake()
	{
		inputField = GetComponent<TMP_InputField>();
		inputField.onSelect.AddListener(OnSelect);
	}

	protected void OnDestroy()
	{
	}

	private void Update()
	{
		if (Application.isPlaying && !inputCaptureInProgress && !localInputCaptureInProgress && !ControllerDisconnectCtrl.controllerDisconnectInProgress && !S.I.btnCtrl.transitioning && !PostCtrl.transitioning && !S.I.conCtrl.IsRewiredBindingInProgress(0) && !S.I.conCtrl.IsRewiredBindingInProgress(1) && !S.I.consoleView.viewContainer.activeSelf && inputField.isFocused)
		{
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
			if (rewiredPlayer != null && rewiredPlayer.GetButtonDown("Accept"))
			{
				TryActivateKeyboard();
			}
		}
	}

	private void LateUpdate()
	{
		if (shouldActivate)
		{
			shouldActivate = false;
			ActivateKeyboard();
		}
	}

	private void ActivateKeyboard()
	{
		string text = inputField.text;
		bool multiLine = inputField.multiLine;
		int num = ((inputField.characterLimit > 0) ? inputField.characterLimit : 20);
		Debug.Log("[" + base.name + "] Launch keyboard", this);
	}

	private void OnSelect(string text)
	{
		if (showKeyboardOnFocus)
		{
			TryActivateKeyboard();
		}
	}

	private void OnVirtualKeyboardComplete(string text)
	{
		inputField.text = text;
		if (inputField.isFocused)
		{
			bool restoreOriginalTextOnEscape = inputField.restoreOriginalTextOnEscape;
			inputField.restoreOriginalTextOnEscape = false;
			inputField.DeactivateInputField();
			inputField.restoreOriginalTextOnEscape = restoreOriginalTextOnEscape;
		}
		else
		{
			inputField.onEndEdit.Invoke(text);
		}
	}

	private void OnViritualKeyboardCanceled()
	{
		if (inputField.isFocused)
		{
			inputField.DeactivateInputField();
		}
	}
}
