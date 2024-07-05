using I2.Loc;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BindButton : NavButton
{
	public TMP_Text actionText;

	public TMP_Text bindingText;

	public Button removeButton;

	public int actionNum;

	public InputAction inputAction;

	public string rewiredControllerID;

	public Image promptImage;

	public int playerNum;

	private OptionCtrl optCtrl;

	private ControlsCtrl conCtrl;

	protected override void Awake()
	{
		base.Awake();
		actionText.text = string.Empty;
		bindingText.text = string.Empty;
	}

	private void Start()
	{
		optCtrl = S.I.optCtrl;
		conCtrl = S.I.conCtrl;
	}

	public void RemoveBinding()
	{
		conCtrl.ClickRemoveBindingButton(inputAction, playerNum);
	}

	public override void OnBackPress()
	{
		base.OnBackPress();
	}

	public override void OnAcceptPress()
	{
		if (!conCtrl.IsRewiredBindingInProgress(playerNum, inputAction))
		{
			conCtrl.ClickBindingButton(inputAction, playerNum);
		}
	}

	protected override void Update()
	{
		string Translation = "";
		if (!LocalizationManager.TryGetTranslation("Controls/Actions_" + inputAction, out Translation))
		{
			Translation = inputAction.ToString();
		}
		if (conCtrl.IsRewiredBindingInProgress(playerNum, inputAction))
		{
			Translation = Translation + " " + ScriptLocalization.Controls.Controls_Listening;
		}
		actionText.text = Translation;
		Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(playerNum);
		bool flag = false;
		if (rewiredPlayer != null)
		{
			if (ControlsCtrl.ControllerIDIsKeyboard(rewiredControllerID))
			{
				bindingText.gameObject.SetActive(true);
				promptImage.gameObject.SetActive(false);
				foreach (ControllerMap allMap in rewiredPlayer.controllers.maps.GetAllMaps(ControllerType.Keyboard))
				{
					foreach (string item in ControlsCtrl.RewiredActions[inputAction])
					{
						ActionElementMap[] elementMapsWithAction = allMap.GetElementMapsWithAction(item);
						int num = 0;
						if (num < elementMapsWithAction.Length)
						{
							ActionElementMap actionElementMap = elementMapsWithAction[num];
							string Translation2 = "";
							if (!LocalizationManager.TryGetTranslation("Controls/controller_default_" + actionElementMap.elementIdentifierName, out Translation2))
							{
								Translation2 = actionElementMap.elementIdentifierName;
							}
							bindingText.text = ScriptLocalization.Controls.Controls_Keyboard + " " + Translation2;
							removeButton.interactable = true;
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
			else
			{
				foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
				{
					if (joystick.hardwareTypeGuid.ToString() == rewiredControllerID)
					{
						foreach (ControllerMap map in rewiredPlayer.controllers.maps.GetMaps(joystick))
						{
							foreach (string item2 in ControlsCtrl.RewiredActions[inputAction])
							{
								InputActionType actionType = InputActionType.Button;
								Rewired.InputAction action = ReInput.mapping.GetAction(item2);
								if (action != null)
								{
									actionType = action.type;
								}
								ActionElementMap[] elementMapsWithAction2 = map.GetElementMapsWithAction(item2);
								foreach (ActionElementMap actionElementMap2 in elementMapsWithAction2)
								{
									bool flag2 = !HelpIconManager.GetMapControllerElementTypeMatchesActionInputType(actionElementMap2.elementType, actionType);
									HelpResources.Type controllerTypeFromGuid = HelpResources.GetControllerTypeFromGuid(rewiredControllerID);
									AxisRange axisRange = actionElementMap2.axisRange;
									if (actionElementMap2.elementType == ControllerElementType.Button)
									{
										axisRange = AxisRange.Positive;
									}
									ControlsCtrl controlsCtrl = S.I.conCtrl;
									Sprite controlSprite = controlsCtrl.GetControlSprite(controllerTypeFromGuid, actionElementMap2.elementIdentifierId, axisRange, actionElementMap2.elementType, controllerTypeFromGuid, actionElementMap2.elementIdentifierId, axisRange, actionElementMap2.elementType);
									bool flag3 = false;
									if (true && controlSprite != null)
									{
										bindingText.gameObject.SetActive(false);
										promptImage.gameObject.SetActive(true);
										promptImage.sprite = controlSprite;
									}
									else
									{
										bindingText.gameObject.SetActive(true);
										promptImage.gameObject.SetActive(false);
										string Translation3 = actionElementMap2.elementIdentifierName;
										if (!LocalizationManager.TryGetTranslation("Controls/controller_default_" + actionElementMap2.elementIdentifierName, out Translation3))
										{
											Translation3 = actionElementMap2.elementIdentifierName;
										}
										if (rewiredControllerID == "cd9718bf-a87a-44bc-8716-60a0def28a9f")
										{
											bindingText.text = "PS4: " + Translation3;
										}
										else if (rewiredControllerID == "71dfe6c8-9e81-428f-a58e-c7e664b7fbed")
										{
											bindingText.text = "PS3: " + Translation3;
										}
										else if (rewiredControllerID == "c3ad3cad-c7cf-4ca8-8c2e-e3df8d9960bb")
										{
											bindingText.text = "PS2: " + Translation3;
										}
										else if (joystick.hardwareName.Length > 20)
										{
											bindingText.text = joystick.hardwareName.Substring(0, 20) + ": " + Translation3;
										}
										else
										{
											bindingText.text = joystick.hardwareName + ": " + Translation3;
										}
									}
									removeButton.interactable = true;
									flag = true;
									if (!flag2)
									{
										break;
									}
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
					if (flag)
					{
						break;
					}
				}
			}
		}
		if (!flag)
		{
			bindingText.text = "";
			removeButton.interactable = false;
			bindingText.gameObject.SetActive(true);
			promptImage.gameObject.SetActive(false);
		}
	}

	public override void OnPointerClick(PointerEventData data)
	{
		conCtrl.ClickBindingButton(inputAction, playerNum);
	}

	public override void Focus(int playerNum = 0)
	{
		canvasGroup.alpha = 1f;
		if ((bool)tmpText && colorOnHover)
		{
			StartColorCo(U.I.GetColor(UIColor.Pink));
		}
		if ((bool)button)
		{
			button.Select();
		}
	}
}
