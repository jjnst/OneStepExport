using System;
using Rewired;
using UnityEngine;

public class HelpResources : MonoBehaviour
{
	public enum Type
	{
		None = 0,
		XboxOne = 1,
		PS4 = 2,
		SwitchHandheld = 3,
		SwitchDualJoycon = 4,
		SwitchJoyconLeft = 5,
		SwitchJoyconRight = 6,
		SwitchPro = 7,
		Steam = 8,
		Keyboard = 9,
		Mouse = 10,
		Joystick = 11
	}

	public enum SpecialIcon
	{
		None = 0,
		LeftStickHorizontal = 1,
		LeftStickVertical = 2,
		RightStickHorizontal = 3,
		RightStickVertical = 4,
		DPadHorizontal = 5,
		DPadVertical = 6,
		Bumpers = 7,
		Triggers = 8,
		LeftStickUp = 9,
		LeftStickDown = 10,
		LeftStickLeft = 11,
		LeftStickRight = 12,
		RightStickUp = 13,
		RightStickDown = 14,
		RightStickLeft = 15,
		RightStickRight = 16
	}

	public static readonly Guid REWIRED_DEFAULT_JOYSTICK_GUID = new Guid("83b427e4-086f-47f3-bb06-be266abd1ca5");

	public static readonly Guid REWIRED_XBOXONE_JOYSTICK_GUID = new Guid("19002688-7406-4f4a-8340-8d25335406c8");

	public static readonly Guid REWIRED_PS4_JOYSTICK_GUID = new Guid("cd9718bf-a87a-44bc-8716-60a0def28a9f");

	public static readonly Guid REWIRED_SWITCH_HANDHELD_GUID = new Guid("1fbdd13b-0795-4173-8a95-a2a75de9d204");

	public static readonly Guid REWIRED_SWITCH_DUAL_JOYCON_GUID = new Guid("521b808c-0248-4526-bc10-f1d16ee76bf1");

	public static readonly Guid REWIRED_SWITCH_JOYCON_LEFT_GUID = new Guid("3eb01142-da0e-4a86-8ae8-a15c2b1f2a04");

	public static readonly Guid REWIRED_SWITCH_JOYCON_RIGHT_GUID = new Guid("605dc720-1b38-473d-a459-67d5857aa6ea");

	public static readonly Guid REWIRED_SWITCH_PRO_GUID = new Guid("7bf3154b-9db8-4d52-950f-cd0eed8a5819");

	public static readonly Guid REWIRED_STEAM_JOYSTICK_GUID = new Guid("2694f4b9-9d84-4f55-9ee8-78fbba744b7d");

	public const int JOYSTICK_CONTROLLER_BUTTON_COUNT = 20;

	public const int XBOX_ONE_CONTROLLER_BUTTON_COUNT = 20;

	public const int PS4_CONTROLLER_BUTTON_COUNT = 22;

	public const int SWITCH_HANDHELD_BUTTON_COUNT = 22;

	public const int SWITCH_DUAL_JOYCON_BUTTON_COUNT = 22;

	public const int SWITCH_JOYCON_LEFT_BUTTON_COUNT = 13;

	public const int SWITCH_JOYCON_RIGHT_BUTTON_COUNT = 13;

	public const int SWITCH_PRO_CONTROLLER_BUTTON_COUNT = 22;

	public const int STEAM_CONTROLLER_BUTTON_COUNT = 20;

	private const int CONTROLLER_AXIS_ELEMENTS = 6;

	private const int SWITCH_AXIS_ELEMENTS = 4;

	private const int SWITCH_JOYCON_AXIS_ELEMENTS = 2;

	private const int MOUSE_BUTTON_COUNT = 3;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultJoystickIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultXboxOneIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultPS4Icon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultSwitchHandheldIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultSwitchDualJoyconIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultSwitchJoyconLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultSwitchJoyconRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultSwitchProIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultSteamIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultKeyboardIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite defaultMouseIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite[] joystickIcons = new Sprite[20];

	[HideInInspector]
	[SerializeField]
	private Sprite[] xboxOneIcons = new Sprite[20];

	[HideInInspector]
	[SerializeField]
	private Sprite[] ps4Icons = new Sprite[22];

	[HideInInspector]
	[SerializeField]
	private Sprite[] switchHandheldIcons = new Sprite[22];

	[HideInInspector]
	[SerializeField]
	private Sprite[] switchDualJoyconIcons = new Sprite[22];

	[HideInInspector]
	[SerializeField]
	private Sprite[] switchJoyconLeftIcons = new Sprite[13];

	[HideInInspector]
	[SerializeField]
	private Sprite[] switchJoyconRightIcons = new Sprite[13];

	[HideInInspector]
	[SerializeField]
	private Sprite[] switchProIcons = new Sprite[22];

	[HideInInspector]
	[SerializeField]
	private Sprite[] steamIcons = new Sprite[20];

	[HideInInspector]
	[SerializeField]
	private Sprite[] keyboardIcons = new Sprite[Enum.GetValues(typeof(KeyCode)).Length];

	[HideInInspector]
	[SerializeField]
	private Sprite[] mouseIcons = new Sprite[3];

	[HideInInspector]
	[SerializeField]
	private Sprite joystickLeftStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickLeftStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickRightStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickRightStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickDPadVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickDPadHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickBumpersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickTriggersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickLeftStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickLeftStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickLeftStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickLeftStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickRightStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickRightStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickRightStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite joystickRightStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneLeftStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneLeftStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneRightStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneRightStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneDPadVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneDPadHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneBumpersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneTriggersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneLeftStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneLeftStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneLeftStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneLeftStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneRightStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneRightStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneRightStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite xboxOneRightStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4LeftStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4LeftStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4RightStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4RightStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4DPadVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4DPadHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4BumpersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4TriggersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4LeftStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4LeftStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4LeftStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4LeftStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4RightStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4RightStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4RightStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite ps4RightStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldLeftStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldLeftStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldRightStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldRightStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldDPadVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldDPadHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldBumpersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldTriggersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldLeftStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldLeftStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldLeftStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldLeftStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldRightStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldRightStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldRightStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchHandheldRightStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconLeftStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconLeftStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconRightStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconRightStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconDPadVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconDPadHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconBumpersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconTriggersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconLeftStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconLeftStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconLeftStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconLeftStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconRightStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconRightStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconRightStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchDualJoyconRightStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconLeftLeftStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconLeftLeftStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconLeftBumpersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconLeftLeftStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconLeftLeftStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconLeftLeftStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconLeftLeftStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconRightRightStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconRightRightStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconRightBumpersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconRightRightStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconRightRightStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconRightRightStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchJoyconRightRightStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProLeftStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProLeftStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProRightStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProRightStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProDPadVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProDPadHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProBumpersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProTriggersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProLeftStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProLeftStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProLeftStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProLeftStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProRightStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProRightStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProRightStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite switchProRightStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamLeftStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamLeftStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamRightStickVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamRightStickHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamDPadVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamDPadHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamBumpersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamTriggersIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamLeftStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamLeftStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamLeftStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamLeftStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamRightStickUpIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamRightStickDownIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamRightStickLeftIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite steamRightStickRightIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite keyboardWASDVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite keyboardWASDHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite keyboardArrowVerticalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite keyboardArrowHorizontalIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite keyboardControlIcon = null;

	[HideInInspector]
	[SerializeField]
	private Sprite keyboardShiftIcon = null;

	public static Type GetControllerTypeFromGuid(string guid)
	{
		return GetControllerTypeFromGuid(new Guid(guid));
	}

	public static Type GetControllerTypeFromGuid(Guid guid)
	{
		if (guid == REWIRED_XBOXONE_JOYSTICK_GUID)
		{
			return Type.XboxOne;
		}
		if (guid == REWIRED_PS4_JOYSTICK_GUID)
		{
			return Type.PS4;
		}
		if (guid == REWIRED_SWITCH_HANDHELD_GUID)
		{
			return Type.SwitchHandheld;
		}
		if (guid == REWIRED_SWITCH_DUAL_JOYCON_GUID)
		{
			return Type.SwitchDualJoycon;
		}
		if (guid == REWIRED_SWITCH_JOYCON_LEFT_GUID)
		{
			return Type.SwitchJoyconLeft;
		}
		if (guid == REWIRED_SWITCH_JOYCON_RIGHT_GUID)
		{
			return Type.SwitchJoyconRight;
		}
		if (guid == REWIRED_SWITCH_PRO_GUID)
		{
			return Type.SwitchPro;
		}
		if (guid == REWIRED_STEAM_JOYSTICK_GUID)
		{
			return Type.Steam;
		}
		return Type.Joystick;
	}

	public static Guid GetControllerHardwareType(Type controllerType)
	{
		switch (controllerType)
		{
		case Type.XboxOne:
			return REWIRED_XBOXONE_JOYSTICK_GUID;
		case Type.PS4:
			return REWIRED_PS4_JOYSTICK_GUID;
		case Type.SwitchHandheld:
			return REWIRED_SWITCH_HANDHELD_GUID;
		case Type.SwitchDualJoycon:
			return REWIRED_SWITCH_DUAL_JOYCON_GUID;
		case Type.SwitchJoyconLeft:
			return REWIRED_SWITCH_JOYCON_LEFT_GUID;
		case Type.SwitchJoyconRight:
			return REWIRED_SWITCH_JOYCON_RIGHT_GUID;
		case Type.SwitchPro:
			return REWIRED_SWITCH_PRO_GUID;
		case Type.Steam:
			return REWIRED_STEAM_JOYSTICK_GUID;
		case Type.Joystick:
			return REWIRED_DEFAULT_JOYSTICK_GUID;
		default:
			return Guid.Empty;
		}
	}

	public static SpecialIcon GetSpecialIconForElement(Type controllerType, int elementIdentifierID, AxisRange elementAxisRange, ControllerElementType elementType, KeyCode elementKeyCode)
	{
		switch (controllerType)
		{
		case Type.Joystick:
			if (elementType == ControllerElementType.Axis)
			{
				if (elementIdentifierID == 0)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickRight;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickLeft;
					default:
						return SpecialIcon.LeftStickHorizontal;
					}
				}
				if (elementIdentifierID == 1)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickUp;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickDown;
					default:
						return SpecialIcon.LeftStickVertical;
					}
				}
				if (elementIdentifierID == 2)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickRight;
					case AxisRange.Negative:
						return SpecialIcon.RightStickLeft;
					default:
						return SpecialIcon.RightStickHorizontal;
					}
				}
				if (elementIdentifierID == 3)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickUp;
					case AxisRange.Negative:
						return SpecialIcon.RightStickDown;
					default:
						return SpecialIcon.RightStickVertical;
					}
				}
				if ((elementIdentifierID == 4 || elementIdentifierID == 5) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Triggers;
				}
			}
			else
			{
				if ((elementIdentifierID == 16 || elementIdentifierID == 18) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadVertical;
				}
				if ((elementIdentifierID == 17 || elementIdentifierID == 19) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadHorizontal;
				}
				if ((elementIdentifierID == 10 || elementIdentifierID == 11) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Bumpers;
				}
			}
			break;
		case Type.XboxOne:
			if (elementType == ControllerElementType.Axis)
			{
				if (elementIdentifierID == 0)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickRight;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickLeft;
					default:
						return SpecialIcon.LeftStickHorizontal;
					}
				}
				if (elementIdentifierID == 1)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickUp;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickDown;
					default:
						return SpecialIcon.LeftStickVertical;
					}
				}
				if (elementIdentifierID == 2)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickRight;
					case AxisRange.Negative:
						return SpecialIcon.RightStickLeft;
					default:
						return SpecialIcon.RightStickHorizontal;
					}
				}
				if (elementIdentifierID == 3)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickUp;
					case AxisRange.Negative:
						return SpecialIcon.RightStickDown;
					default:
						return SpecialIcon.RightStickVertical;
					}
				}
				if ((elementIdentifierID == 4 || elementIdentifierID == 5) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Triggers;
				}
			}
			else
			{
				if ((elementIdentifierID == 16 || elementIdentifierID == 18) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadVertical;
				}
				if ((elementIdentifierID == 17 || elementIdentifierID == 19) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadHorizontal;
				}
				if ((elementIdentifierID == 10 || elementIdentifierID == 11) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Bumpers;
				}
			}
			break;
		case Type.PS4:
			if (elementType == ControllerElementType.Axis)
			{
				if (elementIdentifierID == 0)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickRight;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickLeft;
					default:
						return SpecialIcon.LeftStickHorizontal;
					}
				}
				if (elementIdentifierID == 1)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickUp;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickDown;
					default:
						return SpecialIcon.LeftStickVertical;
					}
				}
				if (elementIdentifierID == 2)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickRight;
					case AxisRange.Negative:
						return SpecialIcon.RightStickLeft;
					default:
						return SpecialIcon.RightStickHorizontal;
					}
				}
				if (elementIdentifierID == 3)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickUp;
					case AxisRange.Negative:
						return SpecialIcon.RightStickDown;
					default:
						return SpecialIcon.RightStickVertical;
					}
				}
				if ((elementIdentifierID == 4 || elementIdentifierID == 5) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Triggers;
				}
			}
			else
			{
				if ((elementIdentifierID == 18 || elementIdentifierID == 20) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadVertical;
				}
				if ((elementIdentifierID == 19 || elementIdentifierID == 21) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadHorizontal;
				}
				if ((elementIdentifierID == 10 || elementIdentifierID == 11) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Bumpers;
				}
			}
			break;
		case Type.SwitchHandheld:
			if (elementType == ControllerElementType.Axis)
			{
				switch (elementIdentifierID)
				{
				case 0:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickRight;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickLeft;
					default:
						return SpecialIcon.LeftStickHorizontal;
					}
				case 1:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickUp;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickDown;
					default:
						return SpecialIcon.LeftStickVertical;
					}
				case 2:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickRight;
					case AxisRange.Negative:
						return SpecialIcon.RightStickLeft;
					default:
						return SpecialIcon.RightStickHorizontal;
					}
				case 3:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickUp;
					case AxisRange.Negative:
						return SpecialIcon.RightStickDown;
					default:
						return SpecialIcon.RightStickVertical;
					}
				}
			}
			else
			{
				if ((elementIdentifierID == 10 || elementIdentifierID == 11) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Triggers;
				}
				if ((elementIdentifierID == 18 || elementIdentifierID == 20) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadVertical;
				}
				if ((elementIdentifierID == 19 || elementIdentifierID == 21) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadHorizontal;
				}
				if ((elementIdentifierID == 8 || elementIdentifierID == 9) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Bumpers;
				}
			}
			break;
		case Type.SwitchDualJoycon:
			if (elementType == ControllerElementType.Axis)
			{
				switch (elementIdentifierID)
				{
				case 0:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickRight;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickLeft;
					default:
						return SpecialIcon.LeftStickHorizontal;
					}
				case 1:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickUp;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickDown;
					default:
						return SpecialIcon.LeftStickVertical;
					}
				case 2:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickRight;
					case AxisRange.Negative:
						return SpecialIcon.RightStickLeft;
					default:
						return SpecialIcon.RightStickHorizontal;
					}
				case 3:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickUp;
					case AxisRange.Negative:
						return SpecialIcon.RightStickDown;
					default:
						return SpecialIcon.RightStickVertical;
					}
				}
			}
			else
			{
				if ((elementIdentifierID == 10 || elementIdentifierID == 11) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Triggers;
				}
				if ((elementIdentifierID == 18 || elementIdentifierID == 20) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadVertical;
				}
				if ((elementIdentifierID == 19 || elementIdentifierID == 21) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadHorizontal;
				}
				if ((elementIdentifierID == 8 || elementIdentifierID == 9) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Bumpers;
				}
			}
			break;
		case Type.SwitchJoyconLeft:
			if (elementType == ControllerElementType.Axis)
			{
				switch (elementIdentifierID)
				{
				case 0:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickRight;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickLeft;
					default:
						return SpecialIcon.LeftStickHorizontal;
					}
				case 1:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickUp;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickDown;
					default:
						return SpecialIcon.LeftStickVertical;
					}
				}
			}
			else if ((elementIdentifierID == 6 || elementIdentifierID == 7) && elementAxisRange == AxisRange.Full)
			{
				return SpecialIcon.Bumpers;
			}
			break;
		case Type.SwitchJoyconRight:
			if (elementType == ControllerElementType.Axis)
			{
				switch (elementIdentifierID)
				{
				case 0:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickRight;
					case AxisRange.Negative:
						return SpecialIcon.RightStickLeft;
					default:
						return SpecialIcon.RightStickHorizontal;
					}
				case 1:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickUp;
					case AxisRange.Negative:
						return SpecialIcon.RightStickDown;
					default:
						return SpecialIcon.RightStickVertical;
					}
				}
			}
			else if ((elementIdentifierID == 6 || elementIdentifierID == 7) && elementAxisRange == AxisRange.Full)
			{
				return SpecialIcon.Bumpers;
			}
			break;
		case Type.SwitchPro:
			if (elementType == ControllerElementType.Axis)
			{
				switch (elementIdentifierID)
				{
				case 0:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickRight;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickLeft;
					default:
						return SpecialIcon.LeftStickHorizontal;
					}
				case 1:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickUp;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickDown;
					default:
						return SpecialIcon.LeftStickVertical;
					}
				case 2:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickRight;
					case AxisRange.Negative:
						return SpecialIcon.RightStickLeft;
					default:
						return SpecialIcon.RightStickHorizontal;
					}
				case 3:
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickUp;
					case AxisRange.Negative:
						return SpecialIcon.RightStickDown;
					default:
						return SpecialIcon.RightStickVertical;
					}
				}
			}
			else
			{
				if ((elementIdentifierID == 10 || elementIdentifierID == 11) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Triggers;
				}
				if ((elementIdentifierID == 18 || elementIdentifierID == 20) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadVertical;
				}
				if ((elementIdentifierID == 19 || elementIdentifierID == 21) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadHorizontal;
				}
				if ((elementIdentifierID == 8 || elementIdentifierID == 9) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Bumpers;
				}
			}
			break;
		case Type.Steam:
			if (elementType == ControllerElementType.Axis)
			{
				if (elementIdentifierID == 0)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickRight;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickLeft;
					default:
						return SpecialIcon.LeftStickHorizontal;
					}
				}
				if (elementIdentifierID == 1)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.LeftStickUp;
					case AxisRange.Negative:
						return SpecialIcon.LeftStickDown;
					default:
						return SpecialIcon.LeftStickVertical;
					}
				}
				if (elementIdentifierID == 2)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickRight;
					case AxisRange.Negative:
						return SpecialIcon.RightStickLeft;
					default:
						return SpecialIcon.RightStickHorizontal;
					}
				}
				if (elementIdentifierID == 3)
				{
					switch (elementAxisRange)
					{
					case AxisRange.Positive:
						return SpecialIcon.RightStickUp;
					case AxisRange.Negative:
						return SpecialIcon.RightStickDown;
					default:
						return SpecialIcon.RightStickVertical;
					}
				}
				if ((elementIdentifierID == 4 || elementIdentifierID == 5) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Triggers;
				}
			}
			else
			{
				if ((elementIdentifierID == 16 || elementIdentifierID == 18) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadVertical;
				}
				if ((elementIdentifierID == 17 || elementIdentifierID == 19) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.DPadHorizontal;
				}
				if ((elementIdentifierID == 10 || elementIdentifierID == 11) && elementAxisRange == AxisRange.Full)
				{
					return SpecialIcon.Bumpers;
				}
			}
			break;
		case Type.Keyboard:
			if ((elementKeyCode == KeyCode.W || elementKeyCode == KeyCode.S) && elementAxisRange == AxisRange.Full)
			{
				return SpecialIcon.LeftStickVertical;
			}
			if ((elementKeyCode == KeyCode.A || elementKeyCode == KeyCode.D) && elementAxisRange == AxisRange.Full)
			{
				return SpecialIcon.LeftStickHorizontal;
			}
			if ((elementKeyCode == KeyCode.UpArrow || elementKeyCode == KeyCode.DownArrow) && elementAxisRange == AxisRange.Full)
			{
				return SpecialIcon.DPadVertical;
			}
			if ((elementKeyCode == KeyCode.LeftArrow || elementKeyCode == KeyCode.RightArrow) && elementAxisRange == AxisRange.Full)
			{
				return SpecialIcon.DPadHorizontal;
			}
			if ((elementKeyCode == KeyCode.LeftControl || elementKeyCode == KeyCode.RightControl) && elementAxisRange == AxisRange.Full)
			{
				return SpecialIcon.Bumpers;
			}
			if ((elementKeyCode == KeyCode.LeftShift || elementKeyCode == KeyCode.RightShift) && elementAxisRange == AxisRange.Full)
			{
				return SpecialIcon.Triggers;
			}
			break;
		}
		return SpecialIcon.None;
	}

	public Sprite GetJoystickIcon(int elementIdentifierID, ControllerElementType elementType)
	{
		return GetJoystickIcon(elementIdentifierID, elementType, defaultJoystickIcon);
	}

	public Sprite GetJoystickIcon(int elementIdentifierID, ControllerElementType elementType, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (elementType)
		{
		case ControllerElementType.Axis:
			if (elementIdentifierID >= 0 && elementIdentifierID < 6)
			{
				sprite = joystickIcons[elementIdentifierID];
			}
			break;
		case ControllerElementType.Button:
			if (elementIdentifierID >= 0 && elementIdentifierID < 20)
			{
				sprite = joystickIcons[elementIdentifierID];
			}
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSpecialJoystickIcon(SpecialIcon specialIcon)
	{
		return GetSpecialJoystickIcon(specialIcon, defaultJoystickIcon);
	}

	public Sprite GetSpecialJoystickIcon(SpecialIcon specialIcon, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (specialIcon)
		{
		case SpecialIcon.LeftStickHorizontal:
			sprite = joystickLeftStickHorizontalIcon;
			break;
		case SpecialIcon.LeftStickVertical:
			sprite = joystickLeftStickVerticalIcon;
			break;
		case SpecialIcon.RightStickHorizontal:
			sprite = joystickRightStickHorizontalIcon;
			break;
		case SpecialIcon.RightStickVertical:
			sprite = joystickRightStickVerticalIcon;
			break;
		case SpecialIcon.DPadHorizontal:
			sprite = joystickDPadHorizontalIcon;
			break;
		case SpecialIcon.DPadVertical:
			sprite = joystickDPadVerticalIcon;
			break;
		case SpecialIcon.Bumpers:
			sprite = joystickBumpersIcon;
			break;
		case SpecialIcon.Triggers:
			sprite = joystickTriggersIcon;
			break;
		case SpecialIcon.LeftStickUp:
			sprite = joystickLeftStickUpIcon;
			break;
		case SpecialIcon.LeftStickDown:
			sprite = joystickLeftStickDownIcon;
			break;
		case SpecialIcon.LeftStickLeft:
			sprite = joystickLeftStickLeftIcon;
			break;
		case SpecialIcon.LeftStickRight:
			sprite = joystickLeftStickRightIcon;
			break;
		case SpecialIcon.RightStickUp:
			sprite = joystickRightStickUpIcon;
			break;
		case SpecialIcon.RightStickDown:
			sprite = joystickRightStickDownIcon;
			break;
		case SpecialIcon.RightStickLeft:
			sprite = joystickRightStickLeftIcon;
			break;
		case SpecialIcon.RightStickRight:
			sprite = joystickRightStickRightIcon;
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetXboxOneIcon(int elementIdentifierID, ControllerElementType elementType)
	{
		return GetXboxOneIcon(elementIdentifierID, elementType, defaultXboxOneIcon);
	}

	public Sprite GetXboxOneIcon(int elementIdentifierID, ControllerElementType elementType, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (elementType)
		{
		case ControllerElementType.Axis:
			if (elementIdentifierID >= 0 && elementIdentifierID < 6)
			{
				sprite = xboxOneIcons[elementIdentifierID];
			}
			break;
		case ControllerElementType.Button:
			if (elementIdentifierID >= 0 && elementIdentifierID < 20)
			{
				sprite = xboxOneIcons[elementIdentifierID];
			}
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSpecialXboxOneIcon(SpecialIcon specialIcon)
	{
		return GetSpecialXboxOneIcon(specialIcon, defaultXboxOneIcon);
	}

	public Sprite GetSpecialXboxOneIcon(SpecialIcon specialIcon, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (specialIcon)
		{
		case SpecialIcon.LeftStickHorizontal:
			sprite = xboxOneLeftStickHorizontalIcon;
			break;
		case SpecialIcon.LeftStickVertical:
			sprite = xboxOneLeftStickVerticalIcon;
			break;
		case SpecialIcon.RightStickHorizontal:
			sprite = xboxOneRightStickHorizontalIcon;
			break;
		case SpecialIcon.RightStickVertical:
			sprite = xboxOneRightStickVerticalIcon;
			break;
		case SpecialIcon.DPadHorizontal:
			sprite = xboxOneDPadHorizontalIcon;
			break;
		case SpecialIcon.DPadVertical:
			sprite = xboxOneDPadVerticalIcon;
			break;
		case SpecialIcon.Bumpers:
			sprite = xboxOneBumpersIcon;
			break;
		case SpecialIcon.Triggers:
			sprite = xboxOneTriggersIcon;
			break;
		case SpecialIcon.LeftStickUp:
			sprite = xboxOneLeftStickUpIcon;
			break;
		case SpecialIcon.LeftStickDown:
			sprite = xboxOneLeftStickDownIcon;
			break;
		case SpecialIcon.LeftStickLeft:
			sprite = xboxOneLeftStickLeftIcon;
			break;
		case SpecialIcon.LeftStickRight:
			sprite = xboxOneLeftStickRightIcon;
			break;
		case SpecialIcon.RightStickUp:
			sprite = xboxOneRightStickUpIcon;
			break;
		case SpecialIcon.RightStickDown:
			sprite = xboxOneRightStickDownIcon;
			break;
		case SpecialIcon.RightStickLeft:
			sprite = xboxOneRightStickLeftIcon;
			break;
		case SpecialIcon.RightStickRight:
			sprite = xboxOneRightStickRightIcon;
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetPS4Icon(int elementIdentifierID, ControllerElementType elementType)
	{
		return GetPS4Icon(elementIdentifierID, elementType, defaultPS4Icon);
	}

	public Sprite GetPS4Icon(int elementIdentifierID, ControllerElementType elementType, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (elementType)
		{
		case ControllerElementType.Axis:
			if (elementIdentifierID >= 0 && elementIdentifierID < 6)
			{
				sprite = ps4Icons[elementIdentifierID];
			}
			break;
		case ControllerElementType.Button:
			if (elementIdentifierID >= 0 && elementIdentifierID < 22)
			{
				sprite = ps4Icons[elementIdentifierID];
			}
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSpecialPS4Icon(SpecialIcon specialIcon)
	{
		return GetSpecialPS4Icon(specialIcon, defaultPS4Icon);
	}

	public Sprite GetSpecialPS4Icon(SpecialIcon specialIcon, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (specialIcon)
		{
		case SpecialIcon.LeftStickHorizontal:
			sprite = ps4LeftStickHorizontalIcon;
			break;
		case SpecialIcon.LeftStickVertical:
			sprite = ps4LeftStickVerticalIcon;
			break;
		case SpecialIcon.RightStickHorizontal:
			sprite = ps4RightStickHorizontalIcon;
			break;
		case SpecialIcon.RightStickVertical:
			sprite = ps4RightStickVerticalIcon;
			break;
		case SpecialIcon.DPadHorizontal:
			sprite = ps4DPadHorizontalIcon;
			break;
		case SpecialIcon.DPadVertical:
			sprite = ps4DPadVerticalIcon;
			break;
		case SpecialIcon.Bumpers:
			sprite = ps4BumpersIcon;
			break;
		case SpecialIcon.Triggers:
			sprite = ps4TriggersIcon;
			break;
		case SpecialIcon.LeftStickUp:
			sprite = ps4LeftStickUpIcon;
			break;
		case SpecialIcon.LeftStickDown:
			sprite = ps4LeftStickDownIcon;
			break;
		case SpecialIcon.LeftStickLeft:
			sprite = ps4LeftStickLeftIcon;
			break;
		case SpecialIcon.LeftStickRight:
			sprite = ps4LeftStickRightIcon;
			break;
		case SpecialIcon.RightStickUp:
			sprite = ps4RightStickUpIcon;
			break;
		case SpecialIcon.RightStickDown:
			sprite = ps4RightStickDownIcon;
			break;
		case SpecialIcon.RightStickLeft:
			sprite = ps4RightStickLeftIcon;
			break;
		case SpecialIcon.RightStickRight:
			sprite = ps4RightStickRightIcon;
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSwitchHandheldIcon(int elementIdentifierID, ControllerElementType elementType)
	{
		return GetSwitchHandheldIcon(elementIdentifierID, elementType, defaultSwitchHandheldIcon);
	}

	public Sprite GetSwitchHandheldIcon(int elementIdentifierID, ControllerElementType elementType, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (elementType)
		{
		case ControllerElementType.Axis:
			if (elementIdentifierID >= 0 && elementIdentifierID < 4)
			{
				sprite = switchHandheldIcons[elementIdentifierID];
			}
			break;
		case ControllerElementType.Button:
			if (elementIdentifierID >= 0 && elementIdentifierID < 22)
			{
				sprite = switchHandheldIcons[elementIdentifierID];
			}
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSpecialSwitchHandheldIcon(SpecialIcon specialIcon)
	{
		return GetSpecialSwitchHandheldIcon(specialIcon, defaultSwitchHandheldIcon);
	}

	public Sprite GetSpecialSwitchHandheldIcon(SpecialIcon specialIcon, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (specialIcon)
		{
		case SpecialIcon.LeftStickHorizontal:
			sprite = switchHandheldLeftStickHorizontalIcon;
			break;
		case SpecialIcon.LeftStickVertical:
			sprite = switchHandheldLeftStickVerticalIcon;
			break;
		case SpecialIcon.RightStickHorizontal:
			sprite = switchHandheldRightStickHorizontalIcon;
			break;
		case SpecialIcon.RightStickVertical:
			sprite = switchHandheldRightStickVerticalIcon;
			break;
		case SpecialIcon.DPadHorizontal:
			sprite = switchHandheldDPadHorizontalIcon;
			break;
		case SpecialIcon.DPadVertical:
			sprite = switchHandheldDPadVerticalIcon;
			break;
		case SpecialIcon.Bumpers:
			sprite = switchHandheldBumpersIcon;
			break;
		case SpecialIcon.Triggers:
			sprite = switchHandheldTriggersIcon;
			break;
		case SpecialIcon.LeftStickUp:
			sprite = switchHandheldLeftStickUpIcon;
			break;
		case SpecialIcon.LeftStickDown:
			sprite = switchHandheldLeftStickDownIcon;
			break;
		case SpecialIcon.LeftStickLeft:
			sprite = switchHandheldLeftStickLeftIcon;
			break;
		case SpecialIcon.LeftStickRight:
			sprite = switchHandheldLeftStickRightIcon;
			break;
		case SpecialIcon.RightStickUp:
			sprite = switchHandheldRightStickUpIcon;
			break;
		case SpecialIcon.RightStickDown:
			sprite = switchHandheldRightStickDownIcon;
			break;
		case SpecialIcon.RightStickLeft:
			sprite = switchHandheldRightStickLeftIcon;
			break;
		case SpecialIcon.RightStickRight:
			sprite = switchHandheldRightStickRightIcon;
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSwitchDualJoyconIcon(int elementIdentifierID, ControllerElementType elementType)
	{
		return GetSwitchDualJoyconIcon(elementIdentifierID, elementType, defaultSwitchDualJoyconIcon);
	}

	public Sprite GetSwitchDualJoyconIcon(int elementIdentifierID, ControllerElementType elementType, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (elementType)
		{
		case ControllerElementType.Axis:
			if (elementIdentifierID >= 0 && elementIdentifierID < 4)
			{
				sprite = switchDualJoyconIcons[elementIdentifierID];
			}
			break;
		case ControllerElementType.Button:
			if (elementIdentifierID >= 0 && elementIdentifierID < 22)
			{
				sprite = switchDualJoyconIcons[elementIdentifierID];
			}
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSpecialSwitchDualJoyconIcon(SpecialIcon specialIcon)
	{
		return GetSpecialSwitchDualJoyconIcon(specialIcon, defaultSwitchDualJoyconIcon);
	}

	public Sprite GetSpecialSwitchDualJoyconIcon(SpecialIcon specialIcon, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (specialIcon)
		{
		case SpecialIcon.LeftStickHorizontal:
			sprite = switchDualJoyconLeftStickHorizontalIcon;
			break;
		case SpecialIcon.LeftStickVertical:
			sprite = switchDualJoyconLeftStickVerticalIcon;
			break;
		case SpecialIcon.RightStickHorizontal:
			sprite = switchDualJoyconRightStickHorizontalIcon;
			break;
		case SpecialIcon.RightStickVertical:
			sprite = switchDualJoyconRightStickVerticalIcon;
			break;
		case SpecialIcon.DPadHorizontal:
			sprite = switchDualJoyconDPadHorizontalIcon;
			break;
		case SpecialIcon.DPadVertical:
			sprite = switchDualJoyconDPadVerticalIcon;
			break;
		case SpecialIcon.Bumpers:
			sprite = switchDualJoyconBumpersIcon;
			break;
		case SpecialIcon.Triggers:
			sprite = switchDualJoyconTriggersIcon;
			break;
		case SpecialIcon.LeftStickUp:
			sprite = switchDualJoyconLeftStickUpIcon;
			break;
		case SpecialIcon.LeftStickDown:
			sprite = switchDualJoyconLeftStickDownIcon;
			break;
		case SpecialIcon.LeftStickLeft:
			sprite = switchDualJoyconLeftStickLeftIcon;
			break;
		case SpecialIcon.LeftStickRight:
			sprite = switchDualJoyconLeftStickRightIcon;
			break;
		case SpecialIcon.RightStickUp:
			sprite = switchDualJoyconRightStickUpIcon;
			break;
		case SpecialIcon.RightStickDown:
			sprite = switchDualJoyconRightStickDownIcon;
			break;
		case SpecialIcon.RightStickLeft:
			sprite = switchDualJoyconRightStickLeftIcon;
			break;
		case SpecialIcon.RightStickRight:
			sprite = switchDualJoyconRightStickRightIcon;
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSwitchJoyconLeftIcon(int elementIdentifierID, ControllerElementType elementType)
	{
		return GetSwitchJoyconLeftIcon(elementIdentifierID, elementType, defaultSwitchJoyconLeftIcon);
	}

	public Sprite GetSwitchJoyconLeftIcon(int elementIdentifierID, ControllerElementType elementType, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (elementType)
		{
		case ControllerElementType.Axis:
			if (elementIdentifierID >= 0 && elementIdentifierID < 2)
			{
				sprite = switchJoyconLeftIcons[elementIdentifierID];
			}
			break;
		case ControllerElementType.Button:
			if (elementIdentifierID >= 0 && elementIdentifierID < 13)
			{
				sprite = switchJoyconLeftIcons[elementIdentifierID];
			}
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSpecialSwitchJoyconLeftIcon(SpecialIcon specialIcon)
	{
		return GetSpecialSwitchJoyconLeftIcon(specialIcon, defaultSwitchJoyconLeftIcon);
	}

	public Sprite GetSpecialSwitchJoyconLeftIcon(SpecialIcon specialIcon, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (specialIcon)
		{
		case SpecialIcon.LeftStickHorizontal:
			sprite = switchJoyconLeftLeftStickHorizontalIcon;
			break;
		case SpecialIcon.LeftStickVertical:
			sprite = switchJoyconLeftLeftStickVerticalIcon;
			break;
		case SpecialIcon.RightStickHorizontal:
			sprite = switchJoyconLeftLeftStickHorizontalIcon;
			break;
		case SpecialIcon.RightStickVertical:
			sprite = switchJoyconLeftLeftStickVerticalIcon;
			break;
		case SpecialIcon.Bumpers:
			sprite = switchJoyconLeftBumpersIcon;
			break;
		case SpecialIcon.LeftStickUp:
			sprite = switchJoyconLeftLeftStickUpIcon;
			break;
		case SpecialIcon.LeftStickDown:
			sprite = switchJoyconLeftLeftStickDownIcon;
			break;
		case SpecialIcon.LeftStickLeft:
			sprite = switchJoyconLeftLeftStickLeftIcon;
			break;
		case SpecialIcon.LeftStickRight:
			sprite = switchJoyconLeftLeftStickRightIcon;
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSwitchJoyconRightIcon(int elementIdentifierID, ControllerElementType elementType)
	{
		return GetSwitchJoyconRightIcon(elementIdentifierID, elementType, defaultSwitchJoyconRightIcon);
	}

	public Sprite GetSwitchJoyconRightIcon(int elementIdentifierID, ControllerElementType elementType, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (elementType)
		{
		case ControllerElementType.Axis:
			if (elementIdentifierID >= 0 && elementIdentifierID < 2)
			{
				sprite = switchJoyconRightIcons[elementIdentifierID];
			}
			break;
		case ControllerElementType.Button:
			if (elementIdentifierID >= 0 && elementIdentifierID < 13)
			{
				sprite = switchJoyconRightIcons[elementIdentifierID];
			}
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSpecialSwitchJoyconRightIcon(SpecialIcon specialIcon)
	{
		return GetSpecialSwitchJoyconRightIcon(specialIcon, defaultSwitchJoyconRightIcon);
	}

	public Sprite GetSpecialSwitchJoyconRightIcon(SpecialIcon specialIcon, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (specialIcon)
		{
		case SpecialIcon.LeftStickHorizontal:
			sprite = switchJoyconRightRightStickHorizontalIcon;
			break;
		case SpecialIcon.LeftStickVertical:
			sprite = switchJoyconRightRightStickVerticalIcon;
			break;
		case SpecialIcon.RightStickHorizontal:
			sprite = switchJoyconRightRightStickHorizontalIcon;
			break;
		case SpecialIcon.RightStickVertical:
			sprite = switchJoyconRightRightStickVerticalIcon;
			break;
		case SpecialIcon.Bumpers:
			sprite = switchJoyconRightBumpersIcon;
			break;
		case SpecialIcon.RightStickUp:
			sprite = switchJoyconRightRightStickUpIcon;
			break;
		case SpecialIcon.RightStickDown:
			sprite = switchJoyconRightRightStickDownIcon;
			break;
		case SpecialIcon.RightStickLeft:
			sprite = switchJoyconRightRightStickLeftIcon;
			break;
		case SpecialIcon.RightStickRight:
			sprite = switchJoyconRightRightStickRightIcon;
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSwitchProIcon(int elementIdentifierID, ControllerElementType elementType)
	{
		return GetSwitchProIcon(elementIdentifierID, elementType, defaultSwitchProIcon);
	}

	public Sprite GetSwitchProIcon(int elementIdentifierID, ControllerElementType elementType, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (elementType)
		{
		case ControllerElementType.Axis:
			if (elementIdentifierID >= 0 && elementIdentifierID < 4)
			{
				sprite = switchProIcons[elementIdentifierID];
			}
			break;
		case ControllerElementType.Button:
			if (elementIdentifierID >= 0 && elementIdentifierID < 22)
			{
				sprite = switchProIcons[elementIdentifierID];
			}
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSpecialSwitchProIcon(SpecialIcon specialIcon)
	{
		return GetSpecialSwitchProIcon(specialIcon, defaultSwitchProIcon);
	}

	public Sprite GetSpecialSwitchProIcon(SpecialIcon specialIcon, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (specialIcon)
		{
		case SpecialIcon.LeftStickHorizontal:
			sprite = switchProLeftStickHorizontalIcon;
			break;
		case SpecialIcon.LeftStickVertical:
			sprite = switchProLeftStickVerticalIcon;
			break;
		case SpecialIcon.RightStickHorizontal:
			sprite = switchProRightStickHorizontalIcon;
			break;
		case SpecialIcon.RightStickVertical:
			sprite = switchProRightStickVerticalIcon;
			break;
		case SpecialIcon.DPadHorizontal:
			sprite = switchProDPadHorizontalIcon;
			break;
		case SpecialIcon.DPadVertical:
			sprite = switchProDPadVerticalIcon;
			break;
		case SpecialIcon.Bumpers:
			sprite = switchProBumpersIcon;
			break;
		case SpecialIcon.Triggers:
			sprite = switchProTriggersIcon;
			break;
		case SpecialIcon.LeftStickUp:
			sprite = switchProLeftStickUpIcon;
			break;
		case SpecialIcon.LeftStickDown:
			sprite = switchProLeftStickDownIcon;
			break;
		case SpecialIcon.LeftStickLeft:
			sprite = switchProLeftStickLeftIcon;
			break;
		case SpecialIcon.LeftStickRight:
			sprite = switchProLeftStickRightIcon;
			break;
		case SpecialIcon.RightStickUp:
			sprite = switchProRightStickUpIcon;
			break;
		case SpecialIcon.RightStickDown:
			sprite = switchProRightStickDownIcon;
			break;
		case SpecialIcon.RightStickLeft:
			sprite = switchProRightStickLeftIcon;
			break;
		case SpecialIcon.RightStickRight:
			sprite = switchProRightStickRightIcon;
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSteamIcon(int elementIdentifierID, ControllerElementType elementType)
	{
		return GetSteamIcon(elementIdentifierID, elementType, defaultSteamIcon);
	}

	public Sprite GetSteamIcon(int elementIdentifierID, ControllerElementType elementType, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (elementType)
		{
		case ControllerElementType.Axis:
			if (elementIdentifierID >= 0 && elementIdentifierID < 6)
			{
				sprite = steamIcons[elementIdentifierID];
			}
			break;
		case ControllerElementType.Button:
			if (elementIdentifierID >= 0 && elementIdentifierID < 20)
			{
				sprite = steamIcons[elementIdentifierID];
			}
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSpecialSteamIcon(SpecialIcon specialIcon)
	{
		return GetSpecialSteamIcon(specialIcon, defaultSteamIcon);
	}

	public Sprite GetSpecialSteamIcon(SpecialIcon specialIcon, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (specialIcon)
		{
		case SpecialIcon.LeftStickHorizontal:
			sprite = steamLeftStickHorizontalIcon;
			break;
		case SpecialIcon.LeftStickVertical:
			sprite = steamLeftStickVerticalIcon;
			break;
		case SpecialIcon.RightStickHorizontal:
			sprite = steamRightStickHorizontalIcon;
			break;
		case SpecialIcon.RightStickVertical:
			sprite = steamRightStickVerticalIcon;
			break;
		case SpecialIcon.DPadHorizontal:
			sprite = steamDPadHorizontalIcon;
			break;
		case SpecialIcon.DPadVertical:
			sprite = steamDPadVerticalIcon;
			break;
		case SpecialIcon.Bumpers:
			sprite = steamBumpersIcon;
			break;
		case SpecialIcon.Triggers:
			sprite = steamTriggersIcon;
			break;
		case SpecialIcon.LeftStickUp:
			sprite = steamLeftStickUpIcon;
			break;
		case SpecialIcon.LeftStickDown:
			sprite = steamLeftStickDownIcon;
			break;
		case SpecialIcon.LeftStickLeft:
			sprite = steamLeftStickLeftIcon;
			break;
		case SpecialIcon.LeftStickRight:
			sprite = steamLeftStickRightIcon;
			break;
		case SpecialIcon.RightStickUp:
			sprite = steamRightStickUpIcon;
			break;
		case SpecialIcon.RightStickDown:
			sprite = steamRightStickDownIcon;
			break;
		case SpecialIcon.RightStickLeft:
			sprite = steamRightStickLeftIcon;
			break;
		case SpecialIcon.RightStickRight:
			sprite = steamRightStickRightIcon;
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetKeyboardIcon(KeyCode keycode)
	{
		return GetKeyboardIcon(keycode, defaultKeyboardIcon);
	}

	public Sprite GetKeyboardIcon(KeyCode keycode, Sprite defaultIcon)
	{
		Sprite sprite = keyboardIcons[(int)keycode];
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetSpecialKeyboardIcon(SpecialIcon specialIcon)
	{
		return GetSpecialKeyboardIcon(specialIcon, defaultKeyboardIcon);
	}

	public Sprite GetSpecialKeyboardIcon(SpecialIcon specialIcon, Sprite defaultIcon)
	{
		Sprite sprite = null;
		switch (specialIcon)
		{
		case SpecialIcon.LeftStickHorizontal:
			sprite = keyboardWASDHorizontalIcon;
			break;
		case SpecialIcon.LeftStickVertical:
			sprite = keyboardWASDVerticalIcon;
			break;
		case SpecialIcon.RightStickHorizontal:
			sprite = keyboardArrowHorizontalIcon;
			break;
		case SpecialIcon.RightStickVertical:
			sprite = keyboardArrowVerticalIcon;
			break;
		case SpecialIcon.DPadHorizontal:
			sprite = keyboardArrowHorizontalIcon;
			break;
		case SpecialIcon.DPadVertical:
			sprite = keyboardArrowVerticalIcon;
			break;
		case SpecialIcon.Bumpers:
			sprite = keyboardControlIcon;
			break;
		case SpecialIcon.Triggers:
			sprite = keyboardShiftIcon;
			break;
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}

	public Sprite GetMouseIcon(int mouseButton)
	{
		return GetMouseIcon(mouseButton, defaultMouseIcon);
	}

	public Sprite GetMouseIcon(int mouseButton, Sprite defaultIcon)
	{
		Sprite sprite = null;
		if (mouseButton >= 0 && mouseButton < 3)
		{
			sprite = mouseIcons[mouseButton];
		}
		if (sprite == null)
		{
			sprite = defaultIcon;
		}
		return sprite;
	}
}
