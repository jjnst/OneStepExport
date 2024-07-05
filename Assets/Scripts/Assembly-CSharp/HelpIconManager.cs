using System;
using System.Collections.Generic;
using Kittehface.Framework20;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using UnityEngine;

public class HelpIconManager : MonoBehaviour
{
	[SerializeField]
	private int playerCount = 2;

	private bool refreshThisFrame = false;

	private InputManager rewiredInputManager = null;

	private bool hasFixedControllerType = false;

	private bool hasLimitedControllerTypes = false;

	private bool controllerRemappingAllowed = true;

	private HelpResources.Type defaultControllerType = HelpResources.Type.Joystick;

	private Guid defaultControllerHardwareType = HelpResources.REWIRED_DEFAULT_JOYSTICK_GUID;

	private List<HelpResources.Type> allowedControllerTypes = new List<HelpResources.Type>();

	private List<HelpResources.Type> controllerTypes = new List<HelpResources.Type>();

	private List<Guid> controllerHardwareTypes = new List<Guid>();

	private List<bool> keyboardAvailable = new List<bool>();

	private static HelpIconManager instance = null;

	public static HelpIconManager Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("HelpIconManager");
				instance = gameObject.AddComponent<HelpIconManager>();
			}
			return instance;
		}
	}

	public static HelpResources.Type GetControllerType(int playerNumber)
	{
		if (playerNumber == Instance.playerCount)
		{
			playerNumber = 0;
		}
		else
		{
			if (playerNumber < 0)
			{
				return HelpResources.Type.None;
			}
			if (playerNumber >= Instance.controllerTypes.Count)
			{
				return HelpResources.Type.None;
			}
		}
		return Instance.controllerTypes[playerNumber];
	}

	public static Guid GetControllerHardwareType(int playerNumber)
	{
		if (playerNumber == Instance.playerCount)
		{
			playerNumber = 0;
		}
		else
		{
			if (playerNumber < 0)
			{
				return Guid.Empty;
			}
			if (playerNumber >= Instance.controllerTypes.Count)
			{
				return Guid.Empty;
			}
		}
		return Instance.controllerHardwareTypes[playerNumber];
	}

	public static void SetKeyboardAvailable(int playerNumber, bool available)
	{
		if (playerNumber >= 0 && playerNumber < Instance.keyboardAvailable.Count)
		{
			if (available != Instance.keyboardAvailable[playerNumber])
			{
				Instance.refreshThisFrame = false;
			}
			Instance.keyboardAvailable[playerNumber] = available;
		}
	}

	public static bool TryGetElementMapInfo(int playerNumber, HelpResources.Type controllerType, int actionID, Pole axisContribution, bool preferMatchActionInputType, bool getFixedMapping, out int elementIdentifierID, out AxisRange elementAxisRange, out ControllerElementType elementType, out KeyCode elementKeyCode, out HelpResources.Type displayControllerType, out int fallbackElementIdentifierID, out AxisRange fallbackElementAxisRange, out ControllerElementType fallbackElementType, out KeyCode fallbackElementKeyCode, out HelpResources.Type fallbackDisplayControllerType, bool verbose)
	{
		if (verbose)
		{
			Debug.Log(string.Concat("HelpIconManager.TryGetElementMapInfo( ", playerNumber, ", ", controllerType, ", ", actionID, ", ", axisContribution, ", ", preferMatchActionInputType.ToString(), ")"));
		}
		if (playerNumber == Instance.playerCount)
		{
			playerNumber = 0;
		}
		else if (playerNumber < 0 || playerNumber >= Instance.controllerTypes.Count)
		{
			elementIdentifierID = -1;
			elementAxisRange = AxisRange.Positive;
			elementType = ControllerElementType.Button;
			elementKeyCode = KeyCode.None;
			displayControllerType = controllerType;
			fallbackElementIdentifierID = -1;
			fallbackElementAxisRange = AxisRange.Positive;
			fallbackElementType = ControllerElementType.Button;
			fallbackElementKeyCode = KeyCode.None;
			fallbackDisplayControllerType = displayControllerType;
			return false;
		}
		if (!Instance.hasFixedControllerType && Instance.controllerRemappingAllowed && !getFixedMapping)
		{
			InputMapCategory existingMapCategory = null;
			Guid existingMapHardwareType = Guid.Empty;
			displayControllerType = controllerType;
			if (verbose)
			{
				Debug.Log("HelpIconManager.TryGetElementMapInfo: Check player");
			}
			ActionElementMap existingElementMap = null;
			existingElementMap = GetBestElementMap(GetPlayerInput(playerNumber), controllerType, existingElementMap, actionID, axisContribution, preferMatchActionInputType, ref displayControllerType, ref existingMapCategory, ref existingMapHardwareType, verbose);
			ActionElementMap actionElementMap = existingElementMap;
			fallbackDisplayControllerType = displayControllerType;
			if (verbose)
			{
				Debug.Log("HelpIconManager.TryGetElementMapInfo: Check system player");
			}
			existingElementMap = GetBestElementMap(ReInput.players.GetSystemPlayer(), controllerType, existingElementMap, actionID, axisContribution, preferMatchActionInputType, ref displayControllerType, ref existingMapCategory, ref existingMapHardwareType, verbose);
			if (existingElementMap != null)
			{
				elementIdentifierID = existingElementMap.elementIdentifierId;
				if (existingElementMap.elementType == ControllerElementType.Axis)
				{
					elementAxisRange = existingElementMap.axisRange;
				}
				else
				{
					elementAxisRange = AxisRange.Positive;
				}
				elementType = existingElementMap.elementType;
				elementKeyCode = existingElementMap.keyCode;
				if (actionElementMap != null)
				{
					fallbackElementIdentifierID = actionElementMap.elementIdentifierId;
					if (actionElementMap.elementType == ControllerElementType.Axis)
					{
						fallbackElementAxisRange = actionElementMap.axisRange;
					}
					else
					{
						fallbackElementAxisRange = AxisRange.Positive;
					}
					fallbackElementType = actionElementMap.elementType;
					fallbackElementKeyCode = actionElementMap.keyCode;
				}
				else
				{
					fallbackElementIdentifierID = -1;
					fallbackElementAxisRange = AxisRange.Positive;
					fallbackElementType = ControllerElementType.Button;
					fallbackElementKeyCode = KeyCode.None;
				}
				if (verbose)
				{
					Debug.Log(string.Concat("HelpIconManager.TryGetElementMapInfo: return [", elementIdentifierID, ", ", elementAxisRange, ", ", elementType, ", ", elementKeyCode, ", ", fallbackElementIdentifierID, ", ", fallbackElementAxisRange, ", ", fallbackElementType, ", ", fallbackElementKeyCode, "]"));
				}
				return true;
			}
			elementIdentifierID = -1;
			elementAxisRange = AxisRange.Positive;
			elementType = ControllerElementType.Button;
			elementKeyCode = KeyCode.None;
			fallbackElementIdentifierID = -1;
			fallbackElementAxisRange = AxisRange.Positive;
			fallbackElementType = ControllerElementType.Button;
			fallbackElementKeyCode = KeyCode.None;
			return false;
		}
		Instance.Refresh();
		displayControllerType = Instance.controllerTypes[playerNumber];
		fallbackDisplayControllerType = displayControllerType;
		elementIdentifierID = -1;
		elementAxisRange = AxisRange.Positive;
		elementType = ControllerElementType.Button;
		elementKeyCode = KeyCode.None;
		fallbackElementIdentifierID = -1;
		fallbackElementAxisRange = AxisRange.Positive;
		fallbackElementType = ControllerElementType.Button;
		fallbackElementKeyCode = KeyCode.None;
		if (Instance.rewiredInputManager != null && Instance.rewiredInputManager.userData != null)
		{
			Rewired.Data.UserData userData = Instance.rewiredInputManager.userData;
			Guid guid = Guid.Empty;
			switch (Instance.controllerTypes[playerNumber])
			{
			case HelpResources.Type.Joystick:
				guid = HelpResources.REWIRED_DEFAULT_JOYSTICK_GUID;
				break;
			case HelpResources.Type.XboxOne:
				guid = HelpResources.REWIRED_XBOXONE_JOYSTICK_GUID;
				break;
			case HelpResources.Type.PS4:
				guid = HelpResources.REWIRED_PS4_JOYSTICK_GUID;
				break;
			case HelpResources.Type.SwitchHandheld:
				guid = HelpResources.REWIRED_SWITCH_HANDHELD_GUID;
				break;
			case HelpResources.Type.SwitchDualJoycon:
				guid = HelpResources.REWIRED_SWITCH_DUAL_JOYCON_GUID;
				break;
			case HelpResources.Type.SwitchJoyconLeft:
				guid = HelpResources.REWIRED_SWITCH_JOYCON_LEFT_GUID;
				break;
			case HelpResources.Type.SwitchJoyconRight:
				guid = HelpResources.REWIRED_SWITCH_JOYCON_RIGHT_GUID;
				break;
			case HelpResources.Type.SwitchPro:
				guid = HelpResources.REWIRED_SWITCH_PRO_GUID;
				break;
			case HelpResources.Type.Steam:
				guid = HelpResources.REWIRED_STEAM_JOYSTICK_GUID;
				break;
			}
			if (guid != Guid.Empty)
			{
				Rewired.InputAction action = ReInput.mapping.GetAction(actionID);
				bool flag = false;
				foreach (InputMapCategory mapCategory in ReInput.mapping.MapCategories)
				{
					foreach (InputLayout item in ReInput.mapping.MapLayouts(ControllerType.Joystick))
					{
						ControllerMap_Editor joystickMap = userData.GetJoystickMap(mapCategory.id, guid, item.id);
						if (joystickMap == null)
						{
							continue;
						}
						foreach (ActionElementMap actionElementMap2 in joystickMap.ActionElementMaps)
						{
							if (actionElementMap2.actionId == actionID && actionElementMap2.axisContribution == axisContribution && (!flag || (preferMatchActionInputType && !GetMapControllerElementTypeMatchesActionInputType(elementType, action.type) && GetMapControllerElementTypeMatchesActionInputType(actionElementMap2.elementType, action.type))))
							{
								elementIdentifierID = actionElementMap2.elementIdentifierId;
								if (actionElementMap2.elementType == ControllerElementType.Axis)
								{
									elementAxisRange = actionElementMap2.axisRange;
								}
								else
								{
									elementAxisRange = AxisRange.Positive;
								}
								elementType = actionElementMap2.elementType;
								elementKeyCode = actionElementMap2.keyCode;
								fallbackElementIdentifierID = elementIdentifierID;
								fallbackElementAxisRange = elementAxisRange;
								fallbackElementType = elementType;
								fallbackElementKeyCode = elementKeyCode;
								flag = true;
							}
						}
					}
				}
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Refresh()
	{
		if (refreshThisFrame || !Platform.initialized || !ReInput.isReady)
		{
			return;
		}
		refreshThisFrame = true;
		if (rewiredInputManager == null)
		{
			rewiredInputManager = UnityEngine.Object.FindObjectOfType<InputManager>();
		}
		if (rewiredInputManager == null)
		{
			Debug.LogError("Could not find Rewired InputManager!", this);
		}
		if (hasFixedControllerType)
		{
			return;
		}
		Rewired.Player systemPlayer = ReInput.players.GetSystemPlayer();
		for (int i = 0; i < controllerTypes.Count; i++)
		{
			Rewired.Player playerInput = GetPlayerInput(i);
			Controller controller = null;
			float num = 0f;
			if (systemPlayer != null && i == 0)
			{
				foreach (Controller controller2 in systemPlayer.controllers.Controllers)
				{
					if ((controller2.type == ControllerType.Keyboard || controller2.type == ControllerType.Joystick || controller2.type == ControllerType.Mouse) && (i < 0 || i >= keyboardAvailable.Count || (controller2.type != 0 && controller2.type != ControllerType.Mouse) || keyboardAvailable[i]) && (!hasLimitedControllerTypes || ((controller2.type != ControllerType.Mouse || allowedControllerTypes.Contains(HelpResources.Type.Mouse)) && (controller2.type != 0 || allowedControllerTypes.Contains(HelpResources.Type.Keyboard)))))
					{
						float lastTimeActive = controller2.GetLastTimeActive();
						if (controller == null || lastTimeActive > num || (lastTimeActive == num && controller != null && (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse) && controller2.type == ControllerType.Joystick))
						{
							controller = controller2;
							num = lastTimeActive;
						}
					}
				}
			}
			if (playerInput != null)
			{
				foreach (Controller controller3 in playerInput.controllers.Controllers)
				{
					if ((controller3.type == ControllerType.Keyboard || controller3.type == ControllerType.Joystick || controller3.type == ControllerType.Mouse) && (i < 0 || i >= keyboardAvailable.Count || (controller3.type != 0 && controller3.type != ControllerType.Mouse) || keyboardAvailable[i]) && (!hasLimitedControllerTypes || ((controller3.type != ControllerType.Mouse || allowedControllerTypes.Contains(HelpResources.Type.Mouse)) && (controller3.type != 0 || allowedControllerTypes.Contains(HelpResources.Type.Keyboard)))))
					{
						float lastTimeActive2 = controller3.GetLastTimeActive();
						if (controller == null || lastTimeActive2 > num || (lastTimeActive2 == num && controller != null && (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse) && controller3.type == ControllerType.Joystick))
						{
							controller = controller3;
							num = lastTimeActive2;
						}
					}
				}
			}
			if (controller == null)
			{
				continue;
			}
			if (controller.type == ControllerType.Mouse)
			{
				SetControllerType(i, HelpResources.Type.Mouse, Guid.Empty);
			}
			else if (controller.type == ControllerType.Keyboard)
			{
				SetControllerType(i, HelpResources.Type.Keyboard, Guid.Empty);
			}
			else if (controller.type == ControllerType.Joystick)
			{
				Joystick joystick = (Joystick)controller;
				if (joystick.hardwareTypeGuid == HelpResources.REWIRED_PS4_JOYSTICK_GUID)
				{
					SetControllerType(i, HelpResources.Type.PS4, joystick.hardwareTypeGuid);
				}
				else if (joystick.hardwareTypeGuid == HelpResources.REWIRED_XBOXONE_JOYSTICK_GUID)
				{
					SetControllerType(i, HelpResources.Type.XboxOne, joystick.hardwareTypeGuid);
				}
				else if (joystick.hardwareTypeGuid == HelpResources.REWIRED_SWITCH_HANDHELD_GUID)
				{
					SetControllerType(i, HelpResources.Type.SwitchHandheld, joystick.hardwareTypeGuid);
				}
				else if (joystick.hardwareTypeGuid == HelpResources.REWIRED_SWITCH_DUAL_JOYCON_GUID)
				{
					SetControllerType(i, HelpResources.Type.SwitchDualJoycon, joystick.hardwareTypeGuid);
				}
				else if (joystick.hardwareTypeGuid == HelpResources.REWIRED_SWITCH_JOYCON_LEFT_GUID)
				{
					SetControllerType(i, HelpResources.Type.SwitchJoyconLeft, joystick.hardwareTypeGuid);
				}
				else if (joystick.hardwareTypeGuid == HelpResources.REWIRED_SWITCH_JOYCON_RIGHT_GUID)
				{
					SetControllerType(i, HelpResources.Type.SwitchJoyconRight, joystick.hardwareTypeGuid);
				}
				else if (joystick.hardwareTypeGuid == HelpResources.REWIRED_SWITCH_PRO_GUID)
				{
					SetControllerType(i, HelpResources.Type.SwitchPro, joystick.hardwareTypeGuid);
				}
				else if (joystick.hardwareTypeGuid == HelpResources.REWIRED_STEAM_JOYSTICK_GUID)
				{
					SetControllerType(i, HelpResources.Type.Steam, joystick.hardwareTypeGuid);
				}
				else if (controller.name.IndexOf("ps3", StringComparison.InvariantCultureIgnoreCase) >= 0 || controller.name.IndexOf("ps4", StringComparison.InvariantCultureIgnoreCase) >= 0 || controller.name.IndexOf("playstation", StringComparison.InvariantCultureIgnoreCase) >= 0 || controller.name.IndexOf("sony", StringComparison.InvariantCultureIgnoreCase) >= 0 || controller.name.IndexOf("dualshock", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					SetControllerType(i, HelpResources.Type.PS4, joystick.hardwareTypeGuid);
				}
				else if (controller.name.IndexOf("steam", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					SetControllerType(i, HelpResources.Type.Steam, joystick.hardwareTypeGuid);
				}
				else
				{
					SetControllerType(i, HelpResources.Type.Joystick, joystick.hardwareTypeGuid);
				}
			}
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		hasFixedControllerType = false;
		hasLimitedControllerTypes = false;
		controllerRemappingAllowed = true;
		for (int i = 0; i < playerCount; i++)
		{
			controllerTypes.Add(HelpResources.Type.Joystick);
			controllerHardwareTypes.Add(HelpResources.REWIRED_DEFAULT_JOYSTICK_GUID);
			keyboardAvailable.Add(true);
		}
	}

	private void Update()
	{
		Refresh();
	}

	private void LateUpdate()
	{
		refreshThisFrame = false;
	}

	private void SetControllerType(int playerNumber, HelpResources.Type controllerType, Guid controllerHardwareType)
	{
		if (playerNumber < 0 || playerNumber >= controllerTypes.Count)
		{
			return;
		}
		if (hasLimitedControllerTypes)
		{
			if (allowedControllerTypes.Contains(controllerType))
			{
				controllerTypes[playerNumber] = controllerType;
				controllerHardwareTypes[playerNumber] = controllerHardwareType;
				if (controllerType == HelpResources.Type.Keyboard && playerNumber >= 0 && playerNumber < keyboardAvailable.Count)
				{
					keyboardAvailable[playerNumber] = true;
				}
			}
			else
			{
				controllerTypes[playerNumber] = defaultControllerType;
				controllerHardwareTypes[playerNumber] = defaultControllerHardwareType;
				if (defaultControllerType == HelpResources.Type.Keyboard && playerNumber >= 0 && playerNumber < keyboardAvailable.Count)
				{
					keyboardAvailable[playerNumber] = true;
				}
			}
		}
		else
		{
			controllerTypes[playerNumber] = controllerType;
			controllerHardwareTypes[playerNumber] = controllerHardwareType;
			if (controllerType == HelpResources.Type.Keyboard && playerNumber >= 0 && playerNumber < keyboardAvailable.Count)
			{
				keyboardAvailable[playerNumber] = true;
			}
		}
	}

	private static Rewired.Player GetPlayerInput(int playerNumber)
	{
		return RunCtrl.GetRewiredPlayer(playerNumber);
	}

	private static ActionElementMap GetBestElementMap(Rewired.Player input, HelpResources.Type controllerType, ActionElementMap existingElementMap, int actionID, Pole axisContribution, bool preferMatchActionInputType, ref HelpResources.Type displayControllerType, ref InputMapCategory existingMapCategory, ref Guid existingMapHardwareType, bool verbose)
	{
		if (verbose)
		{
			Debug.Log(string.Concat("HelpIconManager.GetBestElementMap( ", (input != null) ? "player input" : "null", ", ", controllerType, ", ", (existingElementMap != null) ? "existing element map" : "null", ", ", actionID, ", ", axisContribution, ", ", displayControllerType, ", ", (existingMapCategory != null) ? (existingMapCategory.name + " (assignable " + existingMapCategory.userAssignable + ")") : "null", ", ", existingMapHardwareType, ")"));
		}
		if (input == null)
		{
			return existingElementMap;
		}
		Rewired.InputAction action = ReInput.mapping.GetAction(actionID);
		ActionElementMap actionElementMap = existingElementMap;
		InputMapCategory inputMapCategory = existingMapCategory;
		ControllerType controllerType2 = ControllerType.Custom;
		ControllerType controllerType3 = ControllerType.Custom;
		ControllerElementType elementType = ControllerElementType.Button;
		Guid guid = existingMapHardwareType;
		bool flag = existingMapCategory != null;
		Guid guid2 = Guid.Empty;
		switch (controllerType)
		{
		case HelpResources.Type.XboxOne:
		case HelpResources.Type.PS4:
		case HelpResources.Type.SwitchHandheld:
		case HelpResources.Type.SwitchDualJoycon:
		case HelpResources.Type.SwitchJoyconLeft:
		case HelpResources.Type.SwitchJoyconRight:
		case HelpResources.Type.SwitchPro:
		case HelpResources.Type.Steam:
		case HelpResources.Type.Joystick:
			controllerType2 = ControllerType.Joystick;
			guid2 = HelpResources.GetControllerHardwareType(controllerType);
			break;
		case HelpResources.Type.Keyboard:
			controllerType2 = ControllerType.Keyboard;
			break;
		case HelpResources.Type.Mouse:
			controllerType2 = ControllerType.Mouse;
			break;
		}
		if (verbose)
		{
			Debug.Log(string.Concat("HelpIconManager.GetBestElementMap: controller type [", controllerType2, "], hardware type [", guid2, "], action [", action.name, "]"));
		}
		foreach (ControllerMap allMap in input.controllers.maps.GetAllMaps(controllerType2))
		{
			InputMapCategory mapCategory = ReInput.mapping.GetMapCategory(allMap.categoryId);
			if (verbose)
			{
				Debug.Log(string.Concat("HelpIconManager.GetBestElementMap: map [", allMap.name, ", ", allMap.id, "], category [", mapCategory.name, " (assignable ", mapCategory.userAssignable.ToString(), ")], map hardware type [", allMap.hardwareGuid, "], controller hardware type [", allMap.controller.hardwareTypeGuid, "]"));
			}
			foreach (ActionElementMap allMap2 in allMap.AllMaps)
			{
				if (allMap2.actionId != actionID || allMap2.axisContribution != axisContribution)
				{
					continue;
				}
				if (verbose)
				{
					string text = string.Concat("HelpIconManager.GetBestElementMap: element map [", allMap2.id, "], type [", allMap2.elementType, "], action ID [", allMap2.actionId, "]\n");
					text = string.Concat(text, "Existing element map [", (actionElementMap != null) ? "exists" : "null", "], category [", (inputMapCategory != null) ? (inputMapCategory.name + " (assignable " + inputMapCategory.userAssignable + ")") : "null", "], map hardware type [", guid, "], using existing [", flag.ToString(), "]");
					Debug.Log(text);
				}
				if (actionElementMap == null || (mapCategory.userAssignable && !inputMapCategory.userAssignable) || (flag && (mapCategory.userAssignable || !existingMapCategory.userAssignable)) || (preferMatchActionInputType && !GetMapControllerElementTypeMatchesActionInputType(elementType, action.type) && GetMapControllerElementTypeMatchesActionInputType(allMap2.elementType, action.type) && (guid == Guid.Empty || guid2 == Guid.Empty || guid2 == allMap.hardwareGuid || controllerType2 != ControllerType.Joystick)) || (guid == Guid.Empty && allMap.hardwareGuid != Guid.Empty && controllerType2 == ControllerType.Joystick) || (guid != Guid.Empty && guid2 != Guid.Empty && guid != guid2 && guid2 == allMap.hardwareGuid && controllerType2 == ControllerType.Joystick))
				{
					actionElementMap = allMap2;
					inputMapCategory = mapCategory;
					controllerType3 = controllerType2;
					elementType = allMap2.elementType;
					guid = ((controllerType2 == ControllerType.Joystick) ? allMap.hardwareGuid : Guid.Empty);
					flag = false;
					if (verbose)
					{
						Debug.Log("HelpIconManager.GetBestElementMap: updating to better map");
					}
				}
			}
		}
		if (actionElementMap == null && (controllerType2 == ControllerType.Keyboard || controllerType2 == ControllerType.Mouse))
		{
			ControllerType controllerType4 = ((controllerType2 == ControllerType.Keyboard) ? ControllerType.Mouse : ControllerType.Keyboard);
			foreach (ControllerMap allMap3 in input.controllers.maps.GetAllMaps(controllerType4))
			{
				InputMapCategory mapCategory2 = ReInput.mapping.GetMapCategory(allMap3.categoryId);
				foreach (ActionElementMap allMap4 in allMap3.AllMaps)
				{
					if (allMap4.actionId == actionID && allMap4.axisContribution == axisContribution && (actionElementMap == null || (mapCategory2.userAssignable && !inputMapCategory.userAssignable) || (flag && (mapCategory2.userAssignable || !existingMapCategory.userAssignable))))
					{
						actionElementMap = allMap4;
						inputMapCategory = mapCategory2;
						controllerType3 = controllerType4;
						displayControllerType = ((controllerType4 == ControllerType.Keyboard) ? HelpResources.Type.Keyboard : HelpResources.Type.Mouse);
						guid = Guid.Empty;
						flag = false;
					}
				}
			}
		}
		if (controllerType2 == ControllerType.Mouse)
		{
			foreach (ControllerMap allMap5 in input.controllers.maps.GetAllMaps(ControllerType.Keyboard))
			{
				InputMapCategory mapCategory3 = ReInput.mapping.GetMapCategory(allMap5.categoryId);
				foreach (ActionElementMap allMap6 in allMap5.AllMaps)
				{
					if (allMap6.actionId == actionID && allMap6.axisContribution == axisContribution && (actionElementMap == null || (mapCategory3.userAssignable && !inputMapCategory.userAssignable) || (controllerType3 == ControllerType.Mouse && (mapCategory3.userAssignable || !inputMapCategory.userAssignable)) || (flag && (mapCategory3.userAssignable || !existingMapCategory.userAssignable))))
					{
						actionElementMap = allMap6;
						inputMapCategory = mapCategory3;
						controllerType3 = ControllerType.Keyboard;
						displayControllerType = HelpResources.Type.Keyboard;
						guid = Guid.Empty;
						flag = false;
					}
				}
			}
		}
		existingMapCategory = inputMapCategory;
		existingMapHardwareType = guid;
		return actionElementMap;
	}

	public static bool GetMapControllerElementTypeMatchesActionInputType(ControllerElementType elementType, InputActionType actionType)
	{
		switch (elementType)
		{
		case ControllerElementType.Axis:
			return actionType == InputActionType.Axis;
		case ControllerElementType.Button:
			return actionType == InputActionType.Button;
		default:
			return false;
		}
	}
}
