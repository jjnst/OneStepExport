using System.Collections;
using Kittehface.Framework20;
using Rewired;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputIcon : SerializedMonoBehaviour
{
	public Image icon;

	public Image keyBorder;

	public TMP_Text keyText;

	public ControlsCtrl conCtrl;

	public OptionCtrl optCtrl;

	public InputAction inputAction;

	public Vector3 baseScale;

	private Vector3 targetScale;

	private Vector3 velocity = Vector3.zero;

	public int playerNum = 0;

	public bool useFixedMapping = false;

	private void Awake()
	{
		conCtrl = S.I.conCtrl;
		optCtrl = S.I.optCtrl;
		baseScale = base.transform.localScale;
		targetScale = baseScale;
	}

	private void Start()
	{
		StartCoroutine(DelayedCheck());
	}

	private void OnEnable()
	{
		if (!conCtrl.inputIcons.Contains(this))
		{
			conCtrl.inputIcons.Add(this);
		}
		if (Platform.initialized)
		{
			UpdateDisplay();
		}
		StartCoroutine(DelayedCheck());
		if (S.I.RECORD_MODE)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (base.transform.localScale != targetScale)
		{
			base.transform.localScale = Vector3.SmoothDamp(base.transform.localScale, targetScale, ref velocity, 0.05f, 999f, Time.fixedUnscaledDeltaTime);
		}
	}

	private IEnumerator DelayedCheck()
	{
		yield return new WaitForSecondsRealtime(0.1f);
		while (!Platform.initialized)
		{
			yield return null;
		}
		while (RunCtrl.runProfile == null || RunCtrl.secondaryProfile == null)
		{
			yield return null;
		}
		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		if (playerNum < 0)
		{
			return;
		}
		keyBorder.enabled = false;
		icon.enabled = false;
		keyText.text = "";
		if (!conCtrl)
		{
			conCtrl = S.I.conCtrl;
		}
		string text = conCtrl.playerControllerIDs[playerNum];
		if (playerNum > 1)
		{
			text = conCtrl.playerControllerIDs[2];
		}
		if (ControlsCtrl.ControllerIDIsKeyboard(text))
		{
			SetIconText(inputAction);
			return;
		}
		if (conCtrl.useHelpResourcesForIcons)
		{
			Sprite sprite = null;
			if (ControlsCtrl.RewiredActions.ContainsKey(inputAction))
			{
				foreach (string item in ControlsCtrl.RewiredActions[inputAction])
				{
					int num = -1;
					foreach (Rewired.InputAction action in ReInput.mapping.Actions)
					{
						if (item == action.name)
						{
							num = action.id;
							break;
						}
					}
					if (num >= 0)
					{
						HelpResources.Type controllerTypeFromGuid = HelpResources.GetControllerTypeFromGuid(text);
						HelpResources.Type displayControllerType = controllerTypeFromGuid;
						int elementIdentifierID;
						AxisRange elementAxisRange;
						ControllerElementType elementType;
						KeyCode elementKeyCode;
						int fallbackElementIdentifierID;
						AxisRange fallbackElementAxisRange;
						ControllerElementType fallbackElementType;
						KeyCode fallbackElementKeyCode;
						HelpResources.Type fallbackDisplayControllerType;
						if (HelpIconManager.TryGetElementMapInfo(playerNum, controllerTypeFromGuid, num, Pole.Positive, true, useFixedMapping, out elementIdentifierID, out elementAxisRange, out elementType, out elementKeyCode, out displayControllerType, out fallbackElementIdentifierID, out fallbackElementAxisRange, out fallbackElementType, out fallbackElementKeyCode, out fallbackDisplayControllerType, false))
						{
							sprite = conCtrl.GetControlSprite(displayControllerType, elementIdentifierID, elementAxisRange, elementType, fallbackDisplayControllerType, fallbackElementIdentifierID, fallbackElementAxisRange, fallbackElementType);
						}
					}
					if (sprite != null)
					{
						icon.enabled = true;
						break;
					}
				}
			}
			icon.sprite = sprite;
		}
		if (icon.sprite == null)
		{
			SetIconText(inputAction);
		}
	}

	private void SetIconText(InputAction action)
	{
		Rewired.Player player = ((playerNum <= 1) ? RunCtrl.GetRewiredPlayer(playerNum) : RunCtrl.GetRewiredPlayer());
		if (player == null)
		{
			return;
		}
		bool flag = false;
		foreach (ControllerMap allMap in player.controllers.maps.GetAllMaps(ControllerType.Keyboard))
		{
			foreach (string item in ControlsCtrl.RewiredActions[action])
			{
				ActionElementMap[] elementMapsWithAction = allMap.GetElementMapsWithAction(item);
				int num = 0;
				if (num < elementMapsWithAction.Length)
				{
					ActionElementMap actionElementMap = elementMapsWithAction[num];
					icon.sprite = conCtrl.GetKeyboardSprite(actionElementMap.keyCode);
					if (icon.sprite == null)
					{
						keyText.text = actionElementMap.elementIdentifierName;
						icon.enabled = false;
						keyBorder.enabled = true;
					}
					else
					{
						icon.enabled = true;
					}
					flag = true;
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
	}

	public void FlashDisplay()
	{
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(_FlashDisplay());
		}
	}

	private IEnumerator _FlashDisplay()
	{
		targetScale = baseScale * 1.3f;
		yield return new WaitForSecondsRealtime(0.15f);
		targetScale = baseScale;
	}
}
