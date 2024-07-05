using Kittehface.Framework20;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class HelpIcon : MonoBehaviour
{
	[SerializeField]
	private int playerNumber = -1;

	[SerializeField]
	private Image helpIconImage;

	public string action;

	public Pole axisContribution;

	[SerializeField]
	private bool useFullAxis = false;

	[SerializeField]
	private bool hideIfNotAssigned = true;

	[SerializeField]
	private HelpResources resources = null;

	[SerializeField]
	private GameObject[] hideIfNotAssignedAdditional = null;

	private static HelpResources sharedResources = null;

	private string lastAction;

	private Pole lastAxisContribution;

	private int actionID = -1;

	private bool actionHasFullAxis = false;

	private bool initialized = false;

	private HelpResources.Type controllerType = HelpResources.Type.Joystick;

	private static HelpResources GetSharedResources(HelpResources resourcesPrefab)
	{
		if (sharedResources == null)
		{
			sharedResources = Object.Instantiate(resourcesPrefab);
		}
		return sharedResources;
	}

	private void OnEnable()
	{
		if (helpIconImage == null)
		{
			helpIconImage = GetComponent<Image>();
		}
		if (helpIconImage != null)
		{
			helpIconImage.enabled = false;
		}
		initialized = false;
		Refresh();
	}

	private void Update()
	{
		Refresh();
	}

	private void Refresh()
	{
		bool flag = false;
		if (!Platform.initialized)
		{
			return;
		}
		if (action != lastAction)
		{
			lastAction = action;
			actionHasFullAxis = false;
			flag = true;
			foreach (Rewired.InputAction action in ReInput.mapping.Actions)
			{
				if (this.action == action.name)
				{
					actionID = action.id;
					if (action.type == InputActionType.Axis && useFullAxis)
					{
						actionHasFullAxis = true;
					}
					break;
				}
			}
		}
		if (axisContribution != lastAxisContribution)
		{
			lastAxisContribution = axisContribution;
			flag = true;
		}
		if (actionID == -1)
		{
			if (!flag || !hideIfNotAssigned)
			{
				return;
			}
			if (helpIconImage != null)
			{
				helpIconImage.enabled = false;
			}
			if (hideIfNotAssignedAdditional != null)
			{
				for (int i = 0; i < hideIfNotAssignedAdditional.Length; i++)
				{
					hideIfNotAssignedAdditional[i].SetActive(false);
				}
			}
			return;
		}
		HelpIconManager.Instance.Refresh();
		HelpResources.Type type = HelpIconManager.GetControllerType(playerNumber);
		if (!(!initialized || type != controllerType || flag))
		{
			return;
		}
		initialized = true;
		controllerType = type;
		HelpResources.Type displayControllerType = type;
		int elementIdentifierID;
		AxisRange elementAxisRange;
		ControllerElementType elementType;
		KeyCode elementKeyCode;
		int fallbackElementIdentifierID;
		AxisRange fallbackElementAxisRange;
		ControllerElementType fallbackElementType;
		KeyCode fallbackElementKeyCode;
		HelpResources.Type fallbackDisplayControllerType;
		if (HelpIconManager.TryGetElementMapInfo(playerNumber, type, actionID, axisContribution, false, false, out elementIdentifierID, out elementAxisRange, out elementType, out elementKeyCode, out displayControllerType, out fallbackElementIdentifierID, out fallbackElementAxisRange, out fallbackElementType, out fallbackElementKeyCode, out fallbackDisplayControllerType, false))
		{
			if (helpIconImage != null)
			{
				helpIconImage.enabled = true;
			}
			if (hideIfNotAssignedAdditional != null)
			{
				for (int j = 0; j < hideIfNotAssignedAdditional.Length; j++)
				{
					hideIfNotAssignedAdditional[j].SetActive(true);
				}
			}
			if (!(helpIconImage != null))
			{
				return;
			}
			bool flag2 = false;
			if (actionHasFullAxis)
			{
				HelpResources.SpecialIcon specialIconForElement = HelpResources.GetSpecialIconForElement(displayControllerType, elementIdentifierID, elementAxisRange, elementType, elementKeyCode);
				if (specialIconForElement == HelpResources.SpecialIcon.None)
				{
					specialIconForElement = HelpResources.GetSpecialIconForElement(fallbackDisplayControllerType, fallbackElementIdentifierID, fallbackElementAxisRange, fallbackElementType, fallbackElementKeyCode);
				}
				if (specialIconForElement != 0)
				{
					switch (displayControllerType)
					{
					case HelpResources.Type.Joystick:
						helpIconImage.sprite = GetSharedResources(resources).GetSpecialJoystickIcon(specialIconForElement);
						flag2 = true;
						break;
					case HelpResources.Type.XboxOne:
						helpIconImage.sprite = GetSharedResources(resources).GetSpecialXboxOneIcon(specialIconForElement);
						flag2 = true;
						break;
					case HelpResources.Type.PS4:
						helpIconImage.sprite = GetSharedResources(resources).GetSpecialPS4Icon(specialIconForElement);
						flag2 = true;
						break;
					case HelpResources.Type.SwitchHandheld:
						helpIconImage.sprite = GetSharedResources(resources).GetSpecialSwitchHandheldIcon(specialIconForElement);
						useFullAxis = true;
						break;
					case HelpResources.Type.SwitchDualJoycon:
						helpIconImage.sprite = GetSharedResources(resources).GetSpecialSwitchDualJoyconIcon(specialIconForElement);
						useFullAxis = true;
						break;
					case HelpResources.Type.SwitchJoyconLeft:
						helpIconImage.sprite = GetSharedResources(resources).GetSpecialSwitchJoyconLeftIcon(specialIconForElement);
						useFullAxis = true;
						break;
					case HelpResources.Type.SwitchJoyconRight:
						helpIconImage.sprite = GetSharedResources(resources).GetSpecialSwitchJoyconRightIcon(specialIconForElement);
						useFullAxis = true;
						break;
					case HelpResources.Type.SwitchPro:
						helpIconImage.sprite = GetSharedResources(resources).GetSpecialSwitchProIcon(specialIconForElement);
						useFullAxis = true;
						break;
					case HelpResources.Type.Steam:
						helpIconImage.sprite = GetSharedResources(resources).GetSpecialSteamIcon(specialIconForElement);
						useFullAxis = true;
						break;
					case HelpResources.Type.Keyboard:
						helpIconImage.sprite = GetSharedResources(resources).GetSpecialKeyboardIcon(specialIconForElement);
						flag2 = true;
						break;
					}
				}
			}
			if (flag2)
			{
				return;
			}
			switch (displayControllerType)
			{
			case HelpResources.Type.Joystick:
				helpIconImage.sprite = GetSharedResources(resources).GetJoystickIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.XboxOne:
				helpIconImage.sprite = GetSharedResources(resources).GetXboxOneIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.PS4:
				helpIconImage.sprite = GetSharedResources(resources).GetPS4Icon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.SwitchHandheld:
				helpIconImage.sprite = GetSharedResources(resources).GetSwitchHandheldIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.SwitchDualJoycon:
				helpIconImage.sprite = GetSharedResources(resources).GetSwitchDualJoyconIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.SwitchJoyconLeft:
				helpIconImage.sprite = GetSharedResources(resources).GetSwitchJoyconLeftIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.SwitchJoyconRight:
				helpIconImage.sprite = GetSharedResources(resources).GetSwitchJoyconRightIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.SwitchPro:
				helpIconImage.sprite = GetSharedResources(resources).GetSwitchProIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.Steam:
				helpIconImage.sprite = GetSharedResources(resources).GetSteamIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.Keyboard:
				helpIconImage.sprite = GetSharedResources(resources).GetKeyboardIcon(elementKeyCode, null);
				break;
			case HelpResources.Type.Mouse:
				helpIconImage.sprite = GetSharedResources(resources).GetMouseIcon(elementIdentifierID, null);
				break;
			}
			if (helpIconImage.sprite == null)
			{
				switch (fallbackDisplayControllerType)
				{
				case HelpResources.Type.Joystick:
					helpIconImage.sprite = GetSharedResources(resources).GetJoystickIcon(fallbackElementIdentifierID, fallbackElementType);
					break;
				case HelpResources.Type.XboxOne:
					helpIconImage.sprite = GetSharedResources(resources).GetXboxOneIcon(fallbackElementIdentifierID, fallbackElementType);
					break;
				case HelpResources.Type.PS4:
					helpIconImage.sprite = GetSharedResources(resources).GetPS4Icon(fallbackElementIdentifierID, fallbackElementType);
					break;
				case HelpResources.Type.SwitchHandheld:
					helpIconImage.sprite = GetSharedResources(resources).GetSwitchHandheldIcon(fallbackElementIdentifierID, fallbackElementType);
					break;
				case HelpResources.Type.SwitchDualJoycon:
					helpIconImage.sprite = GetSharedResources(resources).GetSwitchDualJoyconIcon(fallbackElementIdentifierID, fallbackElementType);
					break;
				case HelpResources.Type.SwitchJoyconLeft:
					helpIconImage.sprite = GetSharedResources(resources).GetSwitchJoyconLeftIcon(fallbackElementIdentifierID, fallbackElementType);
					break;
				case HelpResources.Type.SwitchJoyconRight:
					helpIconImage.sprite = GetSharedResources(resources).GetSwitchJoyconRightIcon(fallbackElementIdentifierID, fallbackElementType);
					break;
				case HelpResources.Type.SwitchPro:
					helpIconImage.sprite = GetSharedResources(resources).GetSwitchProIcon(fallbackElementIdentifierID, fallbackElementType);
					break;
				case HelpResources.Type.Steam:
					helpIconImage.sprite = GetSharedResources(resources).GetSteamIcon(fallbackElementIdentifierID, fallbackElementType);
					break;
				case HelpResources.Type.Keyboard:
					helpIconImage.sprite = GetSharedResources(resources).GetKeyboardIcon(fallbackElementKeyCode);
					break;
				case HelpResources.Type.Mouse:
					helpIconImage.sprite = GetSharedResources(resources).GetMouseIcon(fallbackElementIdentifierID);
					break;
				}
			}
		}
		else
		{
			if (!hideIfNotAssigned)
			{
				return;
			}
			if (helpIconImage != null)
			{
				helpIconImage.enabled = false;
			}
			if (hideIfNotAssignedAdditional != null)
			{
				for (int k = 0; k < hideIfNotAssignedAdditional.Length; k++)
				{
					hideIfNotAssignedAdditional[k].SetActive(false);
				}
			}
		}
	}
}
