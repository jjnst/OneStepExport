using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using I2.Loc;
using Kittehface.Framework20;
using Rewired;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsCtrl : NavPanel
{
	public static readonly Dictionary<InputAction, List<string>> RewiredActions = new Dictionary<InputAction, List<string>>
	{
		{
			InputAction.Weapon,
			new List<string> { "Weapon" }
		},
		{
			InputAction.Accept,
			new List<string> { "Accept" }
		},
		{
			InputAction.Back,
			new List<string> { "Back" }
		},
		{
			InputAction.Shuffle,
			new List<string> { "Shuffle" }
		},
		{
			InputAction.FireOne,
			new List<string> { "FireOne" }
		},
		{
			InputAction.FireTwo,
			new List<string> { "FireTwo" }
		},
		{
			InputAction.RemoveSpell,
			new List<string> { "RemoveSpell" }
		},
		{
			InputAction.UpgradeSpell,
			new List<string> { "UpgradeSpell" }
		},
		{
			InputAction.Outfit,
			new List<string> { "Outfit" }
		},
		{
			InputAction.Move_Left,
			new List<string> { "Gameplay_Left", "UI_Left" }
		},
		{
			InputAction.Move_Right,
			new List<string> { "Gameplay_Right", "UI_Right" }
		},
		{
			InputAction.Move_Up,
			new List<string> { "Gameplay_Up", "UI_Up" }
		},
		{
			InputAction.Move_Down,
			new List<string> { "Gameplay_Down", "UI_Down" }
		},
		{
			InputAction.Menu,
			new List<string> { "Menu" }
		},
		{
			InputAction.TopNav,
			new List<string> { "TopNav" }
		},
		{
			InputAction.ChooseZone,
			new List<string> { "ChooseZone" }
		},
		{
			InputAction.SignIn,
			new List<string> { "SignIn" }
		}
	};

	private const string PLAYER_CONTROLLER_ID_SAVE_KEY = "PlayerControllerID{0}";

	private const string PLAYER_REWIRED_KEYBOARD_GAMEPLAY_BINDING_SAVE_KEY = "PlayerRewiredKeyboardGameplayBindings{0}";

	private const string PLAYER_REWIRED_KEYBOARD_UI_BINDING_SAVE_KEY = "PlayerRewiredKeyboardUIBindings{0}";

	private const string PLAYER_REWIRED_KEYBOARD_UI_GAMEPLAY_BINDING_SAVE_KEY = "PlayerRewiredKeyboardUIGameplayBindings{0}";

	private const string PLAYER_REWIRED_KEYBOARD_MENU_BINDING_SAVE_KEY = "PlayerRewiredKeyboardMenuBindings{0}";

	private const string PLAYER_REWIRED_KEYBOARD_UI_GAMEPLAY2_BINDING_SAVE_KEY = "PlayerRewiredKeyboardUIGameplay2Bindings{0}";

	private const string PLAYER_REWIRED_JOYSTICK_GAMEPLAY_BINDING_SAVE_KEY = "PlayerRewiredJoystickGameplayBindings_{0}_{1}";

	private const string PLAYER_REWIRED_JOYSTICK_UI_BINDING_SAVE_KEY = "PlayerRewiredJoystickUIBindings_{0}_{1}";

	private const string PLAYER_REWIRED_JOYSTICK_UI_GAMEPLAY_BINDING_SAVE_KEY = "PlayerRewiredJoystickUIGameplayBindings_{0}_{1}";

	private const string PLAYER_REWIRED_JOYSTICK_MENU_BINDING_SAVE_KEY = "PlayerRewiredJoystickMenuBindings_{0}_{1}";

	private const string PLAYER_REWIRED_JOYSTICK_UI_GAMEPLAY2_BINDING_SAVE_KEY = "PlayerRewiredJoystickUIGameplay2Bindings_{0}_{1}";

	public const string REWIRED_PS4_CONTROLLER_GUID = "cd9718bf-a87a-44bc-8716-60a0def28a9f";

	public const string REWIRED_PS3_CONTROLLER_GUID = "71dfe6c8-9e81-428f-a58e-c7e664b7fbed";

	public const string REWIRED_PS2_CONTROLLER_GUID = "c3ad3cad-c7cf-4ca8-8c2e-e3df8d9960bb";

	public const string REWIRED_SWITCH_HANDLEHELD_CONTROLLER_GUID = "1fbdd13b-0795-4173-8a95-a2a75de9d204";

	public const string REWIRED_SWITCH_DUALJOYCON_CONTROLLER_GUID = "521b808c-0248-4526-bc10-f1d16ee76bf1";

	public const string REWIRED_SWITCH_JOYCONLEFT_CONTROLLER_GUID = "3eb01142-da0e-4a86-8ae8-a15c2b1f2a04";

	public const string REWIRED_SWITCH_JOYCONRIGHT_CONTROLLER_GUID = "605dc720-1b38-473d-a459-67d5857aa6ea";

	public const string REWIRED_SWITCH_PRO_CONTROLLER_GUID = "7bf3154b-9db8-4d52-950f-cd0eed8a5819";

	[HideInInspector]
	public string[] playerControllerIDs = new string[3];

	private string currentBindingControllerID = null;

	private int currentBindingPlayerNum = 0;

	private InputAction[] playerBindingAction = new InputAction[2];

	private bool[] playerBindingActionSet = new bool[2];

	private bool[] requestClearPlayerBindingAction = new bool[2];

	private bool buttonHeldAtBindingStart = false;

	private bool buttonHeldAtBindingEnd = false;

	private List<UIButton> bindButtonList = new List<UIButton>();

	public Scrollbar scrollbar;

	public BindButton bindButtonPrefab;

	public Transform bindButtonGrid;

	public List<TMP_Text> deviceTexts;

	public List<string> deviceOptions = new List<string>();

	public int[] currentDeviceNums = new int[2];

	[HideInInspector]
	public string[] playerDisplayControllerIDs = new string[2];

	public List<List<string>> controllerIDList = new List<List<string>>();

	public Dictionary<string, string> controllerNameMap = new Dictionary<string, string>();

	public string[] disconnectedActiveDevices = new string[2];

	public string[] disconnectedActiveControllerIDs = new string[2];

	public List<NavButton> playerControlsButtons;

	public NavButton resetMultiplayerDevicesButton;

	public GameObject[] player2Objects;

	public GameObject[] deviceObjects;

	public List<InputIcon> inputIcons;

	public SlideBody disconnectedPane;

	private BC ctrl;

	private HeroSelectCtrl heCtrl;

	public IdleCtrl idCtrl;

	private MusicCtrl muCtrl;

	private OptionCtrl optCtrl;

	private TutorialCtrl tutCtrl;

	private bool transitioning = false;

	public Coroutine co_reconnect;

	public bool searchingForReconnection = false;

	public bool iconFlashEnabled = false;

	[ShowInInspector]
	public HelpResources helpResourcesPrefab;

	public bool useHelpResourcesForIcons = false;

	private HelpResources helpResourcesInstance = null;

	private InputManager rewiredInputManager = null;

	private bool controlsCtrlStarted = false;

	private Array inputActionsArray;

	private bool menuOpen = false;

	private Guid lastActiveJoystickType = Guid.Empty;

	private float lastActiveJoystickTimeS = -1f;

	private float lastActiveKeyboardTimeS = -1f;

	protected override void Awake()
	{
		base.Awake();
		inputActionsArray = Enum.GetValues(typeof(InputAction));
		if (useHelpResourcesForIcons)
		{
			helpResourcesInstance = UnityEngine.Object.Instantiate(helpResourcesPrefab);
		}
		controllerIDList.Add(new List<string>());
		controllerIDList.Add(new List<string>());
		UserInput.OnControllerConfigurationChanged += UserInput_OnControllerConfigurationChanged;
		SetNavController(null);
	}

	private IEnumerator Start()
	{
		ctrl = S.I.batCtrl;
		heCtrl = S.I.heCtrl;
		muCtrl = S.I.muCtrl;
		optCtrl = S.I.optCtrl;
		tutCtrl = S.I.tutCtrl;
		while (!Platform.initialized)
		{
			yield return null;
		}
		while (RunCtrl.runProfile == null || RunCtrl.secondaryProfile == null)
		{
			yield return null;
		}
		yield return new WaitUntil(() => SaveDataCtrl.Initialized);
		S.I.currentProfile = SaveDataCtrl.Get("CurrentProfile", 0, true);
		Debug.Log("First load " + S.I.currentProfile);
		LoadBindings(0);
		LoadBindings(1);
		RefreshControllers(null);
		LoadSavedController(0, SaveDataCtrl.Get("PlayerControllerID0", string.Empty));
		LoadSavedController(1, SaveDataCtrl.Get("PlayerControllerID1", string.Empty));
		controlsCtrlStarted = true;
	}

	private void OnEnable()
	{
		ReInput.ControllerConnectedEvent += Rewired_ControllerConnectedEvent;
		ReInput.ControllerPreDisconnectEvent += Rewired_ControllerPreDisconnectEvent;
	}

	private void OnDisable()
	{
		ReInput.ControllerConnectedEvent -= Rewired_ControllerConnectedEvent;
		ReInput.ControllerPreDisconnectEvent -= Rewired_ControllerPreDisconnectEvent;
	}

	private void OnDestroy()
	{
		UserInput.OnControllerConfigurationChanged -= UserInput_OnControllerConfigurationChanged;
	}

	public override void Open()
	{
		base.Open();
		menuOpen = true;
		RefreshControllers(null);
		ShowPlayerBindings(0, false);
		resetMultiplayerDevicesButton.gameObject.SetActive(heCtrl.gameMode != GameMode.Solo);
		if (resetMultiplayerDevicesButton.gameObject.activeInHierarchy)
		{
			playerControlsButtons[1].right = resetMultiplayerDevicesButton;
		}
		else
		{
			playerControlsButtons[1].right = null;
		}
		bool active = controllerIDList[1].Count > 0;
		bool flag = false;
		for (int i = 0; i < player2Objects.Length; i++)
		{
			if (flag)
			{
				bool flag2 = false;
				for (int j = 0; j < deviceObjects.Length; j++)
				{
					if (deviceObjects[j] == player2Objects[i])
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					continue;
				}
			}
			player2Objects[i].SetActive(active);
		}
		HelpResources.Type controllerType = HelpIconManager.GetControllerType(0);
		if (controllerType == HelpResources.Type.Keyboard || controllerType == HelpResources.Type.Mouse || controllerType == HelpResources.Type.None)
		{
			SetCurrentDevice(0, null);
		}
		else
		{
			SetCurrentDevice(0, HelpIconManager.GetControllerHardwareType(0).ToString());
		}
	}

	private void Update()
	{
		if (!controlsCtrlStarted)
		{
			return;
		}
		if (buttonHeldAtBindingEnd)
		{
			buttonHeldAtBindingEnd = ReInput.controllers.GetAnyButton();
		}
		if (TryUpdatedBinding())
		{
			return;
		}
		HelpIconManager.Instance.Refresh();
		Controller controller = ReInput.controllers.GetLastActiveController(ControllerType.Joystick);
		Controller controller2 = ReInput.controllers.GetLastActiveController(ControllerType.Keyboard);
		if (controller != null)
		{
			if (lastActiveJoystickType == controller.hardwareTypeGuid && controller.GetLastTimeActive() <= lastActiveJoystickTimeS)
			{
				controller = null;
			}
			if (controller != null)
			{
				lastActiveJoystickType = controller.hardwareTypeGuid;
				lastActiveJoystickTimeS = controller.GetLastTimeActive();
			}
		}
		if (controller2 != null)
		{
			if (controller2.GetLastTimeActive() <= lastActiveKeyboardTimeS)
			{
				controller2 = null;
			}
			if (controller2 != null)
			{
				lastActiveKeyboardTimeS = controller2.GetLastTimeActive();
			}
		}
		if (controller != null)
		{
			FlashIcons(controller);
			if (heCtrl.gameMode == GameMode.Solo)
			{
				Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
				if (rewiredPlayer != null && !rewiredPlayer.controllers.ContainsController(controller))
				{
					rewiredPlayer.controllers.AddController(controller, true);
					UserInput.ClearJoystickDisconnection(RunCtrl.runProfile);
					LoadBindings(controller, 0);
				}
			}
			optCtrl.HideAndCountdownToIdleVid();
		}
		else if (controller2 != null)
		{
			FlashIcons(controller2);
			if (heCtrl.gameMode == GameMode.Solo)
			{
				Rewired.Player rewiredPlayer2 = RunCtrl.GetRewiredPlayer();
				if (rewiredPlayer2 != null && !rewiredPlayer2.controllers.ContainsController(controller2))
				{
					rewiredPlayer2.controllers.AddController(controller2, false);
					LoadBindings(controller2, 0);
				}
			}
			optCtrl.HideAndCountdownToIdleVid();
		}
		string text = null;
		HelpResources.Type controllerType = HelpIconManager.GetControllerType(0);
		if (controllerType != HelpResources.Type.Keyboard && controllerType != HelpResources.Type.Mouse && controllerType != 0)
		{
			text = HelpIconManager.GetControllerHardwareType(0).ToString();
		}
		if (playerControllerIDs[0] != text)
		{
			SetController(0, text);
			UpdateControlDisplays(0);
		}
		if (playerControllerIDs[2] != text)
		{
			SetNavController(text);
		}
		if (heCtrl.gameMode == GameMode.CoOp || heCtrl.gameMode == GameMode.PvP)
		{
			string text2 = null;
			HelpResources.Type controllerType2 = HelpIconManager.GetControllerType(1);
			if (controllerType2 != HelpResources.Type.Keyboard && controllerType2 != HelpResources.Type.Mouse && controllerType2 != 0)
			{
				text2 = HelpIconManager.GetControllerHardwareType(1).ToString();
			}
			if (playerControllerIDs[1] != text2)
			{
				SetController(1, text2);
				UpdateControlDisplays(1);
			}
		}
		if (Input.GetMouseButtonDown(0))
		{
			optCtrl.HideAndCountdownToIdleVid();
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < requestClearPlayerBindingAction.Length; i++)
		{
			if (requestClearPlayerBindingAction[i])
			{
				requestClearPlayerBindingAction[i] = false;
				playerBindingActionSet[i] = false;
			}
		}
	}

	public Controller AnyDeviceWasPressedOnListener(ControllerType controllerType)
	{
		switch (controllerType)
		{
		case ControllerType.Joystick:
			foreach (Joystick joystick in ReInput.controllers.Joysticks)
			{
				if (joystick.GetAnyButtonDown())
				{
					return joystick;
				}
			}
			break;
		case ControllerType.Keyboard:
			if (ReInput.controllers.Keyboard.GetAnyButtonDown())
			{
				return ReInput.controllers.Keyboard;
			}
			break;
		}
		return null;
	}

	private void LoadSavedController(int playerNum, string controllerID)
	{
		bool flag = false;
		if (!ControllerIDIsKeyboard(controllerID))
		{
			currentDeviceNums[playerNum] = controllerIDList[playerNum].IndexOf(controllerID);
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(playerNum);
			if (currentDeviceNums[playerNum] > 0 && rewiredPlayer != null)
			{
				foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
				{
					if (joystick.hardwareTypeGuid.ToString() == controllerID)
					{
						flag = true;
						SetController(playerNum, joystick.hardwareTypeGuid.ToString());
						SetCurrentDevice(playerNum, joystick.hardwareTypeGuid.ToString());
						break;
					}
				}
			}
		}
		if (!flag)
		{
			currentDeviceNums[playerNum] = 0;
			SetController(playerNum, (string)null);
			SetCurrentDevice(playerNum, null);
		}
	}

	public void RefreshControllers(Controller excludeController)
	{
		for (int i = 0; i < controllerIDList.Count; i++)
		{
			controllerIDList[i].Clear();
		}
		controllerNameMap.Clear();
		for (int j = 0; j < controllerIDList.Count; j++)
		{
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(j);
			controllerIDList[j].Add("Keyboard");
			if (!controllerNameMap.ContainsKey("Keyboard"))
			{
				controllerNameMap.Add("Keyboard", ScriptLocalization.UI.Controls_Keyboard);
			}
			if (rewiredPlayer != null)
			{
				foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
				{
					if (joystick == excludeController)
					{
						continue;
					}
					string text = joystick.hardwareTypeGuid.ToString();
					if (!controllerIDList[j].Contains(text))
					{
						controllerIDList[j].Add(text);
						if (!controllerNameMap.ContainsKey(text))
						{
							controllerNameMap.Add(text, joystick.hardwareName);
						}
					}
				}
			}
			if (j == 0 && controllerIDList[j].Count == 0)
			{
				if (rewiredInputManager == null)
				{
					rewiredInputManager = UnityEngine.Object.FindObjectOfType<InputManager>();
				}
				if (!(rewiredInputManager != null))
				{
				}
			}
			if (currentDeviceNums[j] >= controllerIDList[j].Count)
			{
				currentDeviceNums[j] = 0;
			}
			if (currentDeviceNums[j] < controllerIDList[j].Count)
			{
				deviceTexts[j].text = GetDeviceName(j);
			}
		}
	}

	private string GetDeviceName(int playerNum)
	{
		string Translation = controllerNameMap[controllerIDList[playerNum][currentDeviceNums[playerNum]]];
		if (!LocalizationManager.TryGetTranslation("Controls/controller_" + controllerNameMap[controllerIDList[playerNum][currentDeviceNums[playerNum]]], out Translation))
		{
			Translation = controllerNameMap[controllerIDList[playerNum][currentDeviceNums[playerNum]]];
		}
		return Translation;
	}

	public void ClickDeviceButton(int playerNum)
	{
		currentDeviceNums[playerNum] = Utils.GetNextWrappedIndex(controllerIDList[playerNum], currentDeviceNums[playerNum]);
		deviceTexts[playerNum].text = GetDeviceName(playerNum);
		bool flag = false;
		Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(playerNum);
		if (currentDeviceNums[playerNum] > 0 && rewiredPlayer != null)
		{
			foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
			{
				if (joystick.hardwareTypeGuid.ToString() == controllerIDList[playerNum][currentDeviceNums[playerNum]])
				{
					flag = true;
					SetController(playerNum, joystick.hardwareTypeGuid.ToString());
					SetCurrentDevice(playerNum, joystick.hardwareTypeGuid.ToString());
					break;
				}
			}
		}
		if (!flag)
		{
			SetController(playerNum, (string)null);
			SetCurrentDevice(playerNum, null);
		}
		ShowPlayerBindings(playerNum, false);
	}

	public void SetController(int playerNum, Controller controller)
	{
		if (controller == null)
		{
			SetController(playerNum, (string)null);
		}
		else if (controller.type == ControllerType.Joystick)
		{
			SetController(playerNum, controller.hardwareTypeGuid.ToString());
		}
		else
		{
			SetController(playerNum, (string)null);
		}
	}

	private void SetController(int playerNum, string controllerHardwareType)
	{
		LoadBindings(playerNum);
		playerControllerIDs[playerNum] = controllerHardwareType;
	}

	private void SetNavController(string controllerHardwareType)
	{
		playerControllerIDs[2] = controllerHardwareType;
		UpdateControlDisplays(2);
	}

	private void SetCurrentDevice(int playerNum, string controllerHardwareType)
	{
		if (ControllerIDIsKeyboard(controllerHardwareType))
		{
			currentDeviceNums[playerNum] = 0;
		}
		else
		{
			currentDeviceNums[playerNum] = controllerIDList[playerNum].IndexOf(controllerHardwareType);
			if (currentDeviceNums[playerNum] < 0)
			{
				currentDeviceNums[playerNum] = 0;
			}
		}
		if (currentDeviceNums[playerNum] == 0)
		{
			SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerControllerID{0}", playerNum), string.Empty);
		}
		else
		{
			SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerControllerID{0}", playerNum), controllerIDList[playerNum][currentDeviceNums[playerNum]]);
		}
		deviceTexts[playerNum].text = GetDeviceName(playerNum);
	}

	public void SaveBindings()
	{
		int mapCategoryId = ReInput.mapping.GetMapCategoryId("Gameplay");
		int mapCategoryId2 = ReInput.mapping.GetMapCategoryId("UI");
		int mapCategoryId3 = ReInput.mapping.GetMapCategoryId("UIGameplay");
		int mapCategoryId4 = ReInput.mapping.GetMapCategoryId("UIGameplay2");
		int mapCategoryId5 = ReInput.mapping.GetMapCategoryId("Menu");
		for (int i = 0; i < 2; i++)
		{
			string item = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerControllerID{0}", i), "");
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(i);
			if (rewiredPlayer == null)
			{
				continue;
			}
			PlayerSaveData saveData = rewiredPlayer.GetSaveData(true);
			List<string> list = new List<string>();
			list.Add(item);
			for (int j = 0; j < list.Count; j++)
			{
				if (ControllerIDIsKeyboard(list[j]))
				{
					KeyboardMapSaveData[] keyboardMapSaveData = saveData.keyboardMapSaveData;
					bool flag = false;
					bool flag2 = false;
					bool flag3 = false;
					bool flag4 = false;
					bool flag5 = false;
					KeyboardMapSaveData[] array = keyboardMapSaveData;
					foreach (KeyboardMapSaveData keyboardMapSaveData2 in array)
					{
						if (keyboardMapSaveData2.categoryId == mapCategoryId && !flag)
						{
							SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredKeyboardGameplayBindings{0}", i), keyboardMapSaveData2.map.ToXmlString());
							flag = true;
						}
						else if (keyboardMapSaveData2.categoryId == mapCategoryId2 && !flag2)
						{
							SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredKeyboardUIBindings{0}", i), keyboardMapSaveData2.map.ToXmlString());
							flag2 = true;
						}
						else if (keyboardMapSaveData2.categoryId == mapCategoryId3 && !flag3)
						{
							SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredKeyboardUIGameplayBindings{0}", i), keyboardMapSaveData2.map.ToXmlString());
							flag3 = true;
						}
						else if (keyboardMapSaveData2.categoryId == mapCategoryId4 && !flag4)
						{
							SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredKeyboardUIGameplay2Bindings{0}", i), keyboardMapSaveData2.map.ToXmlString());
							flag4 = true;
						}
						else if (keyboardMapSaveData2.categoryId == mapCategoryId5 && !flag5)
						{
							SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredKeyboardMenuBindings{0}", i), keyboardMapSaveData2.map.ToXmlString());
							flag5 = true;
						}
						if (flag && flag2 && flag3 && flag5)
						{
							break;
						}
					}
					continue;
				}
				JoystickMapSaveData[] joystickMapSaveData = saveData.joystickMapSaveData;
				bool flag6 = false;
				bool flag7 = false;
				bool flag8 = false;
				bool flag9 = false;
				bool flag10 = false;
				JoystickMapSaveData[] array2 = joystickMapSaveData;
				foreach (JoystickMapSaveData joystickMapSaveData2 in array2)
				{
					if (joystickMapSaveData2.categoryId == mapCategoryId && joystickMapSaveData2.joystickHardwareTypeGuid.ToString() == list[j] && !flag6)
					{
						SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredJoystickGameplayBindings_{0}_{1}", list[j], i), joystickMapSaveData2.map.ToXmlString());
						flag6 = true;
					}
					else if (joystickMapSaveData2.categoryId == mapCategoryId2 && joystickMapSaveData2.joystickHardwareTypeGuid.ToString() == list[j] && !flag7)
					{
						SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredJoystickUIBindings_{0}_{1}", list[j], i), joystickMapSaveData2.map.ToXmlString());
						flag7 = true;
					}
					else if (joystickMapSaveData2.categoryId == mapCategoryId3 && joystickMapSaveData2.joystickHardwareTypeGuid.ToString() == list[j] && !flag8)
					{
						SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredJoystickUIGameplayBindings_{0}_{1}", list[j], i), joystickMapSaveData2.map.ToXmlString());
						flag8 = true;
					}
					else if (joystickMapSaveData2.categoryId == mapCategoryId4 && joystickMapSaveData2.joystickHardwareTypeGuid.ToString() == list[j] && !flag9)
					{
						SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredJoystickUIGameplay2Bindings_{0}_{1}", list[j], i), joystickMapSaveData2.map.ToXmlString());
						flag9 = true;
					}
					else if (joystickMapSaveData2.categoryId == mapCategoryId5 && joystickMapSaveData2.joystickHardwareTypeGuid.ToString() == list[j] && !flag10)
					{
						SaveDataCtrl.Set(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredJoystickMenuBindings_{0}_{1}", list[j], i), joystickMapSaveData2.map.ToXmlString());
						flag10 = true;
					}
					if (flag6 && flag7 && flag8 && flag10)
					{
						break;
					}
				}
			}
		}
		SaveDataCtrl.Write(true);
		UpdateControlDisplays(0);
		UpdateControlDisplays(1);
		UpdateControlDisplays(2);
	}

	public void LoadBindings(int playerNum)
	{
		string text = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerControllerID{0}", playerNum), string.Empty);
		Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(playerNum);
		if (rewiredPlayer == null)
		{
			return;
		}
		if (ControllerIDIsKeyboard(text))
		{
			LoadBindings(rewiredPlayer.controllers.Keyboard, playerNum);
			return;
		}
		foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
		{
			if (joystick.hardwareTypeGuid.ToString() == text)
			{
				LoadBindings(joystick, playerNum);
				break;
			}
		}
	}

	public void LoadBindings(Controller controller, int forcePlayerNum = -1)
	{
		if (controller == null)
		{
			return;
		}
		int mapCategoryId = ReInput.mapping.GetMapCategoryId("Gameplay");
		int mapCategoryId2 = ReInput.mapping.GetMapCategoryId("UI");
		int mapCategoryId3 = ReInput.mapping.GetMapCategoryId("UIGameplay");
		int mapCategoryId4 = ReInput.mapping.GetMapCategoryId("UIGameplay2");
		int mapCategoryId5 = ReInput.mapping.GetMapCategoryId("Menu");
		Rewired.Player player = null;
		int num = 0;
		if (forcePlayerNum > -1)
		{
			num = forcePlayerNum;
			player = RunCtrl.GetRewiredPlayer(num);
		}
		else
		{
			for (int i = 0; i < ReInput.players.playerCount; i++)
			{
				Rewired.Player player2 = ReInput.players.GetPlayer(i);
				if (player2.controllers.ContainsController(controller))
				{
					player = player2;
					break;
				}
			}
			if (player != null)
			{
				if (player == RunCtrl.GetRewiredPlayer())
				{
					num = 0;
				}
				else
				{
					if (player != RunCtrl.GetRewiredPlayer(1))
					{
						return;
					}
					num = 1;
				}
			}
		}
		if (player == null)
		{
			return;
		}
		if (controller.type == ControllerType.Keyboard)
		{
			string text = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredKeyboardGameplayBindings{0}", num), string.Empty);
			string text2 = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredKeyboardUIBindings{0}", num), string.Empty);
			string text3 = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredKeyboardUIGameplayBindings{0}", num), string.Empty);
			string text4 = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredKeyboardUIGameplay2Bindings{0}", num), string.Empty);
			string text5 = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredKeyboardMenuBindings{0}", num), string.Empty);
			player.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
			if (!string.IsNullOrEmpty(text))
			{
				List<ControllerMap> list = new List<ControllerMap>();
				foreach (ControllerMap item in player.controllers.maps.GetAllMapsInCategory(mapCategoryId))
				{
					if (item.controllerType == ControllerType.Keyboard)
					{
						list.Add(item);
					}
				}
				if (list.Count > 0)
				{
					foreach (ControllerMap item2 in list)
					{
						player.controllers.maps.RemoveMap(ControllerType.Keyboard, 0, item2.id);
					}
					player.controllers.maps.AddMapFromXml(ControllerType.Keyboard, 0, text);
				}
			}
			if (!string.IsNullOrEmpty(text2))
			{
				List<ControllerMap> list2 = new List<ControllerMap>();
				foreach (ControllerMap item3 in player.controllers.maps.GetAllMapsInCategory(mapCategoryId2))
				{
					if (item3.controllerType == ControllerType.Keyboard)
					{
						list2.Add(item3);
					}
				}
				if (list2.Count > 0)
				{
					foreach (ControllerMap item4 in list2)
					{
						player.controllers.maps.RemoveMap(ControllerType.Keyboard, 0, item4.id);
					}
					player.controllers.maps.AddMapFromXml(ControllerType.Keyboard, 0, text2);
				}
			}
			if (!string.IsNullOrEmpty(text3))
			{
				List<ControllerMap> list3 = new List<ControllerMap>();
				foreach (ControllerMap item5 in player.controllers.maps.GetAllMapsInCategory(mapCategoryId3))
				{
					if (item5.controllerType == ControllerType.Keyboard)
					{
						list3.Add(item5);
					}
				}
				if (list3.Count > 0)
				{
					foreach (ControllerMap item6 in list3)
					{
						player.controllers.maps.RemoveMap(ControllerType.Keyboard, 0, item6.id);
					}
					player.controllers.maps.AddMapFromXml(ControllerType.Keyboard, 0, text3);
				}
			}
			if (!string.IsNullOrEmpty(text4))
			{
				List<ControllerMap> list4 = new List<ControllerMap>();
				foreach (ControllerMap item7 in player.controllers.maps.GetAllMapsInCategory(mapCategoryId4))
				{
					if (item7.controllerType == ControllerType.Keyboard)
					{
						list4.Add(item7);
					}
				}
				if (list4.Count > 0)
				{
					foreach (ControllerMap item8 in list4)
					{
						player.controllers.maps.RemoveMap(ControllerType.Keyboard, 0, item8.id);
					}
					player.controllers.maps.AddMapFromXml(ControllerType.Keyboard, 0, text4);
				}
			}
			if (string.IsNullOrEmpty(text5))
			{
				return;
			}
			List<ControllerMap> list5 = new List<ControllerMap>();
			foreach (ControllerMap item9 in player.controllers.maps.GetAllMapsInCategory(mapCategoryId5))
			{
				if (item9.controllerType == ControllerType.Keyboard)
				{
					list5.Add(item9);
				}
			}
			if (list5.Count <= 0)
			{
				return;
			}
			foreach (ControllerMap item10 in list5)
			{
				player.controllers.maps.RemoveMap(ControllerType.Keyboard, 0, item10.id);
			}
			player.controllers.maps.AddMapFromXml(ControllerType.Keyboard, 0, text5);
		}
		else
		{
			if (controller.type != ControllerType.Joystick)
			{
				return;
			}
			int joystickLayoutId = ReInput.mapping.GetJoystickLayoutId("Default");
			string text6 = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredJoystickGameplayBindings_{0}_{1}", controller.hardwareTypeGuid.ToString(), num), string.Empty);
			string text7 = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredJoystickUIBindings_{0}_{1}", controller.hardwareTypeGuid.ToString(), num), string.Empty);
			string text8 = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredJoystickUIGameplayBindings_{0}_{1}", controller.hardwareTypeGuid.ToString(), num), string.Empty);
			string text9 = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredJoystickUIGameplay2Bindings_{0}_{1}", controller.hardwareTypeGuid.ToString(), num), string.Empty);
			string text10 = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerRewiredJoystickMenuBindings_{0}_{1}", controller.hardwareTypeGuid.ToString(), num), string.Empty);
			foreach (Joystick joystick in player.controllers.Joysticks)
			{
				if (joystick.hardwareTypeGuid == controller.hardwareTypeGuid)
				{
					player.controllers.maps.LoadMap(ControllerType.Joystick, joystick.id, mapCategoryId, joystickLayoutId);
					player.controllers.maps.LoadMap(ControllerType.Joystick, joystick.id, mapCategoryId2, joystickLayoutId);
					player.controllers.maps.LoadMap(ControllerType.Joystick, joystick.id, mapCategoryId3, joystickLayoutId);
					player.controllers.maps.LoadMap(ControllerType.Joystick, joystick.id, mapCategoryId4, joystickLayoutId);
					player.controllers.maps.LoadMap(ControllerType.Joystick, joystick.id, mapCategoryId5, joystickLayoutId);
				}
			}
			if (!string.IsNullOrEmpty(text6))
			{
				List<ControllerMap> list6 = new List<ControllerMap>();
				foreach (ControllerMap item11 in player.controllers.maps.GetAllMapsInCategory(mapCategoryId))
				{
					if (item11.controllerType == ControllerType.Joystick && item11.hardwareGuid == controller.hardwareTypeGuid)
					{
						list6.Add(item11);
					}
				}
				if (list6.Count > 0)
				{
					foreach (ControllerMap item12 in list6)
					{
						player.controllers.maps.RemoveMap(ControllerType.Joystick, item12.controllerId, item12.id);
					}
					foreach (Joystick joystick2 in player.controllers.Joysticks)
					{
						if (joystick2.hardwareTypeGuid == controller.hardwareTypeGuid)
						{
							player.controllers.maps.AddMapFromXml(ControllerType.Joystick, joystick2.id, text6);
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(text7))
			{
				List<ControllerMap> list7 = new List<ControllerMap>();
				foreach (ControllerMap item13 in player.controllers.maps.GetAllMapsInCategory(mapCategoryId2))
				{
					if (item13.controllerType == ControllerType.Joystick && item13.hardwareGuid == controller.hardwareTypeGuid)
					{
						list7.Add(item13);
					}
				}
				if (list7.Count > 0)
				{
					foreach (ControllerMap item14 in list7)
					{
						player.controllers.maps.RemoveMap(ControllerType.Joystick, item14.controllerId, item14.id);
					}
					foreach (Joystick joystick3 in player.controllers.Joysticks)
					{
						if (joystick3.hardwareTypeGuid == controller.hardwareTypeGuid)
						{
							player.controllers.maps.AddMapFromXml(ControllerType.Joystick, joystick3.id, text7);
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(text8))
			{
				List<ControllerMap> list8 = new List<ControllerMap>();
				foreach (ControllerMap item15 in player.controllers.maps.GetAllMapsInCategory(mapCategoryId3))
				{
					if (item15.controllerType == ControllerType.Joystick && item15.hardwareGuid == controller.hardwareTypeGuid)
					{
						list8.Add(item15);
					}
				}
				if (list8.Count > 0)
				{
					foreach (ControllerMap item16 in list8)
					{
						player.controllers.maps.RemoveMap(ControllerType.Joystick, item16.controllerId, item16.id);
					}
					foreach (Joystick joystick4 in player.controllers.Joysticks)
					{
						if (joystick4.hardwareTypeGuid == controller.hardwareTypeGuid)
						{
							player.controllers.maps.AddMapFromXml(ControllerType.Joystick, joystick4.id, text8);
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(text9))
			{
				List<ControllerMap> list9 = new List<ControllerMap>();
				foreach (ControllerMap item17 in player.controllers.maps.GetAllMapsInCategory(mapCategoryId4))
				{
					if (item17.controllerType == ControllerType.Joystick && item17.hardwareGuid == controller.hardwareTypeGuid)
					{
						list9.Add(item17);
					}
				}
				if (list9.Count > 0)
				{
					foreach (ControllerMap item18 in list9)
					{
						player.controllers.maps.RemoveMap(ControllerType.Joystick, item18.controllerId, item18.id);
					}
					foreach (Joystick joystick5 in player.controllers.Joysticks)
					{
						if (joystick5.hardwareTypeGuid == controller.hardwareTypeGuid)
						{
							player.controllers.maps.AddMapFromXml(ControllerType.Joystick, joystick5.id, text9);
						}
					}
				}
			}
			if (string.IsNullOrEmpty(text10))
			{
				return;
			}
			List<ControllerMap> list10 = new List<ControllerMap>();
			foreach (ControllerMap item19 in player.controllers.maps.GetAllMapsInCategory(mapCategoryId5))
			{
				if (item19.controllerType == ControllerType.Joystick && item19.hardwareGuid == controller.hardwareTypeGuid)
				{
					list10.Add(item19);
				}
			}
			if (list10.Count <= 0)
			{
				return;
			}
			foreach (ControllerMap item20 in list10)
			{
				player.controllers.maps.RemoveMap(ControllerType.Joystick, item20.controllerId, item20.id);
			}
			foreach (Joystick joystick6 in player.controllers.Joysticks)
			{
				if (joystick6.hardwareTypeGuid == controller.hardwareTypeGuid)
				{
					player.controllers.maps.AddMapFromXml(ControllerType.Joystick, joystick6.id, text10);
				}
			}
		}
	}

	public void ResetBindings()
	{
		int num = currentBindingPlayerNum;
		Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(num);
		if (rewiredPlayer != null)
		{
			if (ControllerIDIsKeyboard(currentBindingControllerID))
			{
				rewiredPlayer.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
			}
			else
			{
				int mapCategoryId = ReInput.mapping.GetMapCategoryId("Gameplay");
				int mapCategoryId2 = ReInput.mapping.GetMapCategoryId("UI");
				int mapCategoryId3 = ReInput.mapping.GetMapCategoryId("UIGameplay");
				int mapCategoryId4 = ReInput.mapping.GetMapCategoryId("UIGameplay2");
				int mapCategoryId5 = ReInput.mapping.GetMapCategoryId("Menu");
				int joystickLayoutId = ReInput.mapping.GetJoystickLayoutId("Default");
				foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
				{
					if (joystick.hardwareTypeGuid.ToString() == currentBindingControllerID)
					{
						rewiredPlayer.controllers.maps.LoadMap(ControllerType.Joystick, joystick.id, mapCategoryId, joystickLayoutId);
						rewiredPlayer.controllers.maps.LoadMap(ControllerType.Joystick, joystick.id, mapCategoryId2, joystickLayoutId);
						rewiredPlayer.controllers.maps.LoadMap(ControllerType.Joystick, joystick.id, mapCategoryId3, joystickLayoutId);
						rewiredPlayer.controllers.maps.LoadMap(ControllerType.Joystick, joystick.id, mapCategoryId4, joystickLayoutId);
						rewiredPlayer.controllers.maps.LoadMap(ControllerType.Joystick, joystick.id, mapCategoryId5, joystickLayoutId);
					}
				}
			}
		}
		SaveBindings();
		bool flag = false;
		string text = SaveDataCtrl.Get(string.Format(CultureInfo.InvariantCulture, "PlayerControllerID{0}", num), string.Empty);
		if (!ControllerIDIsKeyboard(text) && rewiredPlayer != null)
		{
			foreach (Joystick joystick2 in rewiredPlayer.controllers.Joysticks)
			{
				if (text == joystick2.hardwareTypeGuid.ToString())
				{
					flag = true;
					SetController(num, joystick2.hardwareTypeGuid.ToString());
					SetCurrentDevice(num, joystick2.hardwareTypeGuid.ToString());
					break;
				}
			}
		}
		if (!flag)
		{
			SetController(num, (string)null);
			SetCurrentDevice(num, null);
		}
		ShowPlayerBindings(num, false);
	}

	public static bool ControllerIDIsKeyboard(string controllerID)
	{
		if (string.IsNullOrEmpty(controllerID))
		{
			return true;
		}
		if (ReInput.controllers.Keyboard != null)
		{
			return controllerID == ReInput.controllers.Keyboard.hardwareTypeGuid.ToString();
		}
		return false;
	}

	public static bool ControllerIDIsPlaystationController(string controllerID)
	{
		return controllerID == "cd9718bf-a87a-44bc-8716-60a0def28a9f" || controllerID == "71dfe6c8-9e81-428f-a58e-c7e664b7fbed" || controllerID == "c3ad3cad-c7cf-4ca8-8c2e-e3df8d9960bb";
	}

	public static bool ControllerIDIsSwitchController(string controllerID)
	{
		int result;
		switch (controllerID)
		{
		default:
			result = ((controllerID == "7bf3154b-9db8-4d52-950f-cd0eed8a5819") ? 1 : 0);
			break;
		case "1fbdd13b-0795-4173-8a95-a2a75de9d204":
		case "521b808c-0248-4526-bc10-f1d16ee76bf1":
		case "3eb01142-da0e-4a86-8ae8-a15c2b1f2a04":
		case "605dc720-1b38-473d-a459-67d5857aa6ea":
			result = 1;
			break;
		}
		return (byte)result != 0;
	}

	private void Rewired_ControllerConnectedEvent(ControllerStatusChangedEventArgs args)
	{
		Debug.Log("Controller was attached: " + args.name + " " + Time.frameCount);
		if (heCtrl.gameMode != 0)
		{
			for (int i = 0; i < disconnectedActiveControllerIDs.Length; i++)
			{
				if (disconnectedActiveControllerIDs[i] == args.controller.hardwareTypeGuid.ToString())
				{
					SetController(i, args.controller.hardwareTypeGuid.ToString());
					disconnectedActiveControllerIDs[i] = "";
				}
			}
		}
		RefreshControllers(null);
	}

	private void Rewired_ControllerPreDisconnectEvent(ControllerStatusChangedEventArgs args)
	{
		Debug.Log("Controller was detached: " + args.name);
		if (heCtrl.gameMode == GameMode.Solo && args.controller.hardwareTypeGuid.ToString() == playerControllerIDs[0])
		{
			SetController(0, (string)null);
			UpdateControlDisplays(0);
		}
		RefreshControllers(args.controller);
	}

	private IEnumerator _ActiveDeviceDisconnected()
	{
		float originalTimeScale = BC.GTimeScale;
		searchingForReconnection = true;
		disconnectedPane.Show();
		BC.GTimeScale = 0f;
		while (searchingForReconnection)
		{
			yield return null;
		}
		disconnectedPane.Hide();
		BC.GTimeScale = originalTimeScale;
		co_reconnect = null;
	}

	public Sprite GetKeyboardSprite(KeyCode keycode)
	{
		return helpResourcesInstance.GetKeyboardIcon(keycode);
	}

	public Sprite GetControlSprite(HelpResources.Type controllerType, int elementIdentifierID, AxisRange elementAxisRange, ControllerElementType elementType, HelpResources.Type fallbackControllerType, int fallbackElementIdentifierID, AxisRange fallbackElementAxisRange, ControllerElementType fallbackElementType)
	{
		Sprite sprite = null;
		HelpResources.SpecialIcon specialIconForElement = HelpResources.GetSpecialIconForElement(controllerType, elementIdentifierID, elementAxisRange, elementType, KeyCode.None);
		if (specialIconForElement == HelpResources.SpecialIcon.None)
		{
			specialIconForElement = HelpResources.GetSpecialIconForElement(fallbackControllerType, fallbackElementIdentifierID, fallbackElementAxisRange, fallbackElementType, KeyCode.None);
		}
		if (specialIconForElement != 0)
		{
			switch (controllerType)
			{
			case HelpResources.Type.Joystick:
				sprite = helpResourcesInstance.GetSpecialJoystickIcon(specialIconForElement);
				break;
			case HelpResources.Type.XboxOne:
				sprite = helpResourcesInstance.GetSpecialXboxOneIcon(specialIconForElement);
				break;
			case HelpResources.Type.PS4:
				sprite = helpResourcesInstance.GetSpecialPS4Icon(specialIconForElement);
				break;
			case HelpResources.Type.SwitchHandheld:
				sprite = helpResourcesInstance.GetSpecialSwitchHandheldIcon(specialIconForElement);
				break;
			case HelpResources.Type.SwitchDualJoycon:
				sprite = helpResourcesInstance.GetSpecialSwitchDualJoyconIcon(specialIconForElement);
				break;
			case HelpResources.Type.SwitchJoyconLeft:
				sprite = helpResourcesInstance.GetSpecialSwitchJoyconLeftIcon(specialIconForElement);
				break;
			case HelpResources.Type.SwitchJoyconRight:
				sprite = helpResourcesInstance.GetSpecialSwitchJoyconRightIcon(specialIconForElement);
				break;
			case HelpResources.Type.SwitchPro:
				sprite = helpResourcesInstance.GetSpecialSwitchProIcon(specialIconForElement);
				break;
			case HelpResources.Type.Steam:
				sprite = helpResourcesInstance.GetSpecialSteamIcon(specialIconForElement);
				break;
			case HelpResources.Type.Keyboard:
				sprite = helpResourcesInstance.GetSpecialKeyboardIcon(specialIconForElement);
				break;
			}
		}
		if (sprite == null)
		{
			switch (controllerType)
			{
			case HelpResources.Type.Joystick:
				sprite = helpResourcesInstance.GetJoystickIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.XboxOne:
				sprite = helpResourcesInstance.GetXboxOneIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.PS4:
				sprite = helpResourcesInstance.GetPS4Icon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.SwitchHandheld:
				sprite = helpResourcesInstance.GetSwitchHandheldIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.SwitchDualJoycon:
				sprite = helpResourcesInstance.GetSwitchDualJoyconIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.SwitchJoyconLeft:
				sprite = helpResourcesInstance.GetSwitchJoyconLeftIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.SwitchJoyconRight:
				sprite = helpResourcesInstance.GetSwitchJoyconRightIcon(elementIdentifierID, elementType, null);
				break;
			case HelpResources.Type.SwitchPro:
				sprite = helpResourcesInstance.GetSwitchProIcon(elementIdentifierID, elementType, null);
				break;
			}
		}
		if (sprite == null)
		{
			switch (fallbackControllerType)
			{
			case HelpResources.Type.Joystick:
				sprite = helpResourcesInstance.GetJoystickIcon(fallbackElementIdentifierID, fallbackElementType);
				break;
			case HelpResources.Type.XboxOne:
				sprite = helpResourcesInstance.GetXboxOneIcon(fallbackElementIdentifierID, fallbackElementType);
				break;
			case HelpResources.Type.PS4:
				sprite = helpResourcesInstance.GetPS4Icon(fallbackElementIdentifierID, fallbackElementType);
				break;
			case HelpResources.Type.SwitchHandheld:
				sprite = helpResourcesInstance.GetSwitchHandheldIcon(fallbackElementIdentifierID, fallbackElementType);
				break;
			case HelpResources.Type.SwitchDualJoycon:
				sprite = helpResourcesInstance.GetSwitchDualJoyconIcon(fallbackElementIdentifierID, fallbackElementType);
				break;
			case HelpResources.Type.SwitchJoyconLeft:
				sprite = helpResourcesInstance.GetSwitchJoyconLeftIcon(fallbackElementIdentifierID, fallbackElementType);
				break;
			case HelpResources.Type.SwitchJoyconRight:
				sprite = helpResourcesInstance.GetSwitchJoyconRightIcon(fallbackElementIdentifierID, fallbackElementType);
				break;
			case HelpResources.Type.SwitchPro:
				sprite = helpResourcesInstance.GetSwitchProIcon(fallbackElementIdentifierID, fallbackElementType);
				break;
			}
		}
		return sprite;
	}

	public void UpdateControlDisplays(int playerNum)
	{
		if (playerNum == 0 || playerNum == 2)
		{
			idCtrl.controlsBackDisplay.UpdateControlOverlay(ControllerIDIsKeyboard(playerControllerIDs[playerNum]));
		}
		UpdateInputIcons();
	}

	private void UpdateInputIcons()
	{
		int num = 0;
		foreach (InputIcon inputIcon in inputIcons)
		{
			if ((bool)inputIcon && inputIcon.isActiveAndEnabled)
			{
				inputIcon.UpdateDisplay();
			}
			num++;
		}
	}

	private void FlashIcons(Controller controller)
	{
		int num = 2;
		int num2 = 2;
		Rewired.Player player = RunCtrl.GetRewiredPlayer();
		Rewired.Player player2 = RunCtrl.GetRewiredPlayer(1);
		Rewired.Player player3 = ReInput.players.GetSystemPlayer();
		if (player != null && controller != null && player.controllers.ContainsController(controller))
		{
			num = 0;
		}
		else if (player2 != null && controller != null && player2.controllers.ContainsController(controller))
		{
			num = 1;
		}
		iconFlashEnabled = true;
		if (btnCtrl.activeInputFields.Count > 0 || !iconFlashEnabled)
		{
			return;
		}
		if (controller == null || controller.type != 0)
		{
			switch (num)
			{
			case 0:
				player2 = null;
				player3 = null;
				break;
			case 1:
				player = null;
				player3 = null;
				break;
			}
		}
		foreach (object item in inputActionsArray)
		{
			bool flag = false;
			if (player != null)
			{
				foreach (string item2 in RewiredActions[(InputAction)item])
				{
					flag |= player.GetButtonDown(item2);
				}
			}
			if (player2 != null)
			{
				foreach (string item3 in RewiredActions[(InputAction)item])
				{
					flag |= player2.GetButtonDown(item3);
				}
			}
			if (player3 != null)
			{
				foreach (string item4 in RewiredActions[(InputAction)item])
				{
					flag |= player3.GetButtonDown(item4);
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (InputIcon inputIcon in inputIcons)
			{
				if ((bool)inputIcon && inputIcon.isActiveAndEnabled && (InputAction)item == inputIcon.inputAction && (num == inputIcon.playerNum || num2 == inputIcon.playerNum))
				{
					inputIcon.FlashDisplay();
				}
			}
		}
	}

	public void ClickShowPlayerBindings(int playerNum)
	{
		ShowPlayerBindings(playerNum, true);
	}

	public void ShowPlayerBindings(int playerNum, bool focusBinding)
	{
		CancelBinding();
		currentBindingPlayerNum = playerNum;
		currentBindingControllerID = playerControllerIDs[playerNum];
		foreach (NavButton playerControlsButton in playerControlsButtons)
		{
			playerControlsButton.image.color = U.I.GetColor(UIColor.White);
		}
		playerControlsButtons[playerNum].image.color = U.I.GetColor(UIColor.Pink);
		SetCurrentDevice(playerNum, currentBindingControllerID);
		StartCoroutine(CreateBindingButtons(currentBindingControllerID, playerNum, focusBinding));
	}

	public IEnumerator CreateBindingButtons(string controllerID, int playerNum, bool focusBinding)
	{
		while (transitioning)
		{
			yield return null;
		}
		transitioning = true;
		Array actions = Enum.GetValues(typeof(InputAction));
		int actionCount = actions.Length;
		bindButtonGrid.DestroyChildren();
		bindButtonList.Clear();
		playerControlsButtons[0].tmpText.color = Color.white;
		playerControlsButtons[1].tmpText.color = Color.white;
		Rewired.Player player = RunCtrl.GetRewiredPlayer(playerNum);
		for (int i = 0; i < actionCount; i++)
		{
			InputAction action = (InputAction)actions.GetValue(i);
			if (action == InputAction.SignIn)
			{
				continue;
			}
			if (player != null)
			{
				bool isAvailable = false;
				foreach (string rewiredAction in RewiredActions[action])
				{
					Rewired.InputAction inputAction = ReInput.mapping.GetAction(rewiredAction);
					InputMapCategory mapCategory = GetMapCategoryForAction(inputAction);
					if (ControllerIDIsKeyboard(currentBindingControllerID))
					{
						foreach (ControllerMap map2 in player.controllers.maps.GetAllMaps(ControllerType.Keyboard))
						{
							if (map2.categoryId == mapCategory.id)
							{
								isAvailable = true;
								break;
							}
						}
						continue;
					}
					foreach (ControllerMap map in player.controllers.maps.GetAllMaps(ControllerType.Joystick))
					{
						if (map.categoryId == mapCategory.id)
						{
							isAvailable = true;
							break;
						}
					}
				}
				if (!isAvailable)
				{
					continue;
				}
			}
			BindButton newBindBtn = UnityEngine.Object.Instantiate(bindButtonPrefab);
			newBindBtn.transform.SetParent(bindButtonGrid);
			newBindBtn.navList = bindButtonList;
			newBindBtn.playerNum = playerNum;
			newBindBtn.back = playerControlsButtons[playerNum];
			bindButtonList.Add(newBindBtn);
			newBindBtn.inputAction = action;
			newBindBtn.rewiredControllerID = controllerID;
		}
		if (btnCtrl.IsActivePanel(this) && focusBinding)
		{
			btnCtrl.SetFocus(bindButtonList[0]);
		}
		foreach (BindButton bindBtn in bindButtonList)
		{
			bindBtn.anim.SetBool("visible", true);
			float time = Time.unscaledTime;
			yield return new WaitWhile(() => time + 0.01f > Time.unscaledTime);
		}
		scrollbar.value = 1f;
		transitioning = false;
	}

	public bool IsRewiredBindingInProgress(int playerNum)
	{
		return playerBindingActionSet[playerNum];
	}

	public bool IsRewiredBindingInProgress(int playerNum, InputAction action)
	{
		return playerBindingActionSet[playerNum] && playerBindingAction[playerNum] == action;
	}

	public void ClickBindingButton(InputAction action, int playerNum)
	{
		if (!buttonHeldAtBindingEnd)
		{
			CancelBinding();
			buttonHeldAtBindingStart = ReInput.controllers.GetAnyButton();
			playerBindingAction[playerNum] = action;
			playerBindingActionSet[playerNum] = true;
			requestClearPlayerBindingAction[playerNum] = false;
		}
	}

	public void ClickRemoveBindingButton(InputAction action, int playerNum)
	{
		CancelBinding();
		int mapCategoryId = ReInput.mapping.GetMapCategoryId("Gameplay");
		int mapCategoryId2 = ReInput.mapping.GetMapCategoryId("UI");
		int mapCategoryId3 = ReInput.mapping.GetMapCategoryId("UIGameplay");
		int mapCategoryId4 = ReInput.mapping.GetMapCategoryId("UIGameplay2");
		int mapCategoryId5 = ReInput.mapping.GetMapCategoryId("Menu");
		Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(playerNum);
		if (rewiredPlayer == null)
		{
			return;
		}
		if (ControllerIDIsKeyboard(currentBindingControllerID))
		{
			foreach (ControllerMap item in rewiredPlayer.controllers.maps.GetMapsInCategory(ControllerType.Keyboard, 0, mapCategoryId))
			{
				foreach (string item2 in RewiredActions[action])
				{
					item.DeleteElementMapsWithAction(item2);
				}
			}
			foreach (ControllerMap item3 in rewiredPlayer.controllers.maps.GetMapsInCategory(ControllerType.Keyboard, 0, mapCategoryId2))
			{
				foreach (string item4 in RewiredActions[action])
				{
					item3.DeleteElementMapsWithAction(item4);
				}
			}
			foreach (ControllerMap item5 in rewiredPlayer.controllers.maps.GetMapsInCategory(ControllerType.Keyboard, 0, mapCategoryId3))
			{
				foreach (string item6 in RewiredActions[action])
				{
					item5.DeleteElementMapsWithAction(item6);
				}
			}
			foreach (ControllerMap item7 in rewiredPlayer.controllers.maps.GetMapsInCategory(ControllerType.Keyboard, 0, mapCategoryId4))
			{
				foreach (string item8 in RewiredActions[action])
				{
					item7.DeleteElementMapsWithAction(item8);
				}
			}
			foreach (ControllerMap item9 in rewiredPlayer.controllers.maps.GetMapsInCategory(ControllerType.Keyboard, 0, mapCategoryId5))
			{
				foreach (string item10 in RewiredActions[action])
				{
					item9.DeleteElementMapsWithAction(item10);
				}
			}
		}
		else
		{
			foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
			{
				if (!(joystick.hardwareTypeGuid.ToString() == currentBindingControllerID))
				{
					continue;
				}
				foreach (ControllerMap item11 in rewiredPlayer.controllers.maps.GetMapsInCategory(ControllerType.Joystick, joystick.id, mapCategoryId))
				{
					foreach (string item12 in RewiredActions[action])
					{
						item11.DeleteElementMapsWithAction(item12);
					}
				}
				foreach (ControllerMap item13 in rewiredPlayer.controllers.maps.GetMapsInCategory(ControllerType.Joystick, joystick.id, mapCategoryId2))
				{
					foreach (string item14 in RewiredActions[action])
					{
						item13.DeleteElementMapsWithAction(item14);
					}
				}
				foreach (ControllerMap item15 in rewiredPlayer.controllers.maps.GetMapsInCategory(ControllerType.Joystick, joystick.id, mapCategoryId3))
				{
					foreach (string item16 in RewiredActions[action])
					{
						item15.DeleteElementMapsWithAction(item16);
					}
				}
				foreach (ControllerMap item17 in rewiredPlayer.controllers.maps.GetMapsInCategory(ControllerType.Joystick, joystick.id, mapCategoryId4))
				{
					foreach (string item18 in RewiredActions[action])
					{
						item17.DeleteElementMapsWithAction(item18);
					}
				}
				foreach (ControllerMap item19 in rewiredPlayer.controllers.maps.GetMapsInCategory(ControllerType.Joystick, joystick.id, mapCategoryId5))
				{
					foreach (string item20 in RewiredActions[action])
					{
						item19.DeleteElementMapsWithAction(item20);
					}
				}
			}
		}
		SaveBindings();
	}

	private bool TryUpdatedBinding()
	{
		InputAction key = InputAction.Weapon;
		bool flag = false;
		int num = 0;
		for (int i = 0; i < playerBindingAction.Length; i++)
		{
			if (playerBindingActionSet[i])
			{
				key = playerBindingAction[i];
				flag = true;
				num = i;
				break;
			}
		}
		if (flag)
		{
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(num);
			if (rewiredPlayer == null)
			{
				CancelBinding();
				return true;
			}
			bool flag2 = false;
			if (ControllerIDIsKeyboard(currentBindingControllerID))
			{
				flag2 = rewiredPlayer.controllers.Keyboard != null;
			}
			else
			{
				foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
				{
					if (joystick.hardwareTypeGuid.ToString() == currentBindingControllerID)
					{
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				CancelBinding();
				return true;
			}
			if (buttonHeldAtBindingStart)
			{
				buttonHeldAtBindingStart = ReInput.controllers.GetAnyButton();
			}
			if (buttonHeldAtBindingStart)
			{
				return true;
			}
			if (ControllerIDIsKeyboard(currentBindingControllerID))
			{
				ControllerPollingInfo pollingInfo;
				if (PollKeyboardForAssignment(out pollingInfo))
				{
					foreach (ControllerMap allMap in rewiredPlayer.controllers.maps.GetAllMaps(ControllerType.Keyboard))
					{
						foreach (string item in RewiredActions[key])
						{
							Rewired.InputAction action = ReInput.mapping.GetAction(item);
							int id = action.id;
							InputMapCategory mapCategoryForAction = GetMapCategoryForAction(action);
							if (allMap.categoryId != mapCategoryForAction.id)
							{
								continue;
							}
							if (mapCategoryForAction.name == "Gameplay")
							{
								InputLayout keyboardLayout = ReInput.mapping.GetKeyboardLayout(allMap.layoutId);
								if (num == 0)
								{
									if (keyboardLayout.name == "Player1")
									{
										continue;
									}
								}
								else if (keyboardLayout.name != "Player1")
								{
									continue;
								}
							}
							ElementAssignment elementAssignment = new ElementAssignment(ControllerType.Keyboard, pollingInfo.elementType, pollingInfo.elementIdentifierId, GetAxisRange(pollingInfo, id), pollingInfo.keyboardKey, ModifierKeyFlags.None, id, Pole.Positive, false, -1);
							ActionElementMap[] elementMapsWithAction = allMap.GetElementMapsWithAction(id);
							int num2 = 0;
							if (num2 < elementMapsWithAction.Length)
							{
								ActionElementMap actionElementMap = elementMapsWithAction[num2];
								elementAssignment.invert = actionElementMap.invert;
								elementAssignment.elementMapId = actionElementMap.id;
							}
							ElementAssignmentConflictCheck conflictCheck = new ElementAssignmentConflictCheck(rewiredPlayer.id, ControllerType.Keyboard, rewiredPlayer.controllers.Keyboard.id, allMap.id, pollingInfo.elementType, pollingInfo.elementIdentifierId, GetAxisRange(pollingInfo, id), pollingInfo.keyboardKey, ModifierKeyFlags.None, id, Pole.Positive, elementAssignment.invert, elementAssignment.elementMapId);
							conflictCheck.controllerMapCategoryId = allMap.categoryId;
							if (rewiredPlayer.controllers.conflictChecking.DoesElementAssignmentConflict(conflictCheck))
							{
								rewiredPlayer.controllers.conflictChecking.RemoveElementAssignmentConflicts(conflictCheck);
							}
							allMap.ReplaceOrCreateElementMap(elementAssignment);
						}
					}
					buttonHeldAtBindingEnd = true;
					SaveBindings();
					CancelBinding();
				}
			}
			else
			{
				foreach (Joystick joystick2 in rewiredPlayer.controllers.Joysticks)
				{
					if (!(joystick2.hardwareTypeGuid.ToString() == currentBindingControllerID))
					{
						continue;
					}
					ControllerPollingInfo info = rewiredPlayer.controllers.polling.PollControllerForFirstElementDown(ControllerType.Joystick, joystick2.id);
					if (!info.success)
					{
						continue;
					}
					foreach (Joystick joystick3 in rewiredPlayer.controllers.Joysticks)
					{
						if (!(joystick3.hardwareTypeGuid.ToString() == currentBindingControllerID))
						{
							continue;
						}
						foreach (ControllerMap map in rewiredPlayer.controllers.maps.GetMaps(joystick3))
						{
							foreach (string item2 in RewiredActions[key])
							{
								Rewired.InputAction action2 = ReInput.mapping.GetAction(item2);
								int id2 = action2.id;
								InputMapCategory mapCategoryForAction2 = GetMapCategoryForAction(action2);
								if (map.categoryId == mapCategoryForAction2.id)
								{
									ElementAssignment elementAssignment2 = new ElementAssignment(ControllerType.Joystick, info.elementType, info.elementIdentifierId, GetAxisRange(info, id2), info.keyboardKey, ModifierKeyFlags.None, id2, Pole.Positive, false, -1);
									ActionElementMap[] elementMapsWithAction2 = map.GetElementMapsWithAction(id2);
									int num3 = 0;
									if (num3 < elementMapsWithAction2.Length)
									{
										ActionElementMap actionElementMap2 = elementMapsWithAction2[num3];
										elementAssignment2.invert = actionElementMap2.invert;
										elementAssignment2.elementMapId = actionElementMap2.id;
									}
									ElementAssignmentConflictCheck conflictCheck2 = new ElementAssignmentConflictCheck(rewiredPlayer.id, ControllerType.Joystick, joystick3.id, map.id, info.elementType, info.elementIdentifierId, GetAxisRange(info, id2), info.keyboardKey, ModifierKeyFlags.None, id2, Pole.Positive, elementAssignment2.invert, elementAssignment2.elementMapId);
									conflictCheck2.controllerMapCategoryId = map.categoryId;
									if (rewiredPlayer.controllers.conflictChecking.DoesElementAssignmentConflict(conflictCheck2))
									{
										rewiredPlayer.controllers.conflictChecking.RemoveElementAssignmentConflicts(conflictCheck2);
									}
									map.ReplaceOrCreateElementMap(elementAssignment2);
								}
							}
						}
					}
					buttonHeldAtBindingEnd = true;
					SaveBindings();
					CancelBinding();
					break;
				}
			}
			return true;
		}
		return false;
	}

	private static AxisRange GetAxisRange(ControllerPollingInfo info, int actionID)
	{
		if (!info.success)
		{
			return AxisRange.Positive;
		}
		AxisRange result = AxisRange.Positive;
		if (info.elementType == ControllerElementType.Axis)
		{
			Rewired.InputAction action = ReInput.mapping.GetAction(actionID);
			result = ((action.type != 0) ? ((info.axisPole == Pole.Positive) ? AxisRange.Positive : AxisRange.Negative) : AxisRange.Full);
		}
		return result;
	}

	private static InputMapCategory GetMapCategoryForAction(Rewired.InputAction action)
	{
		InputCategory actionCategory = ReInput.mapping.GetActionCategory(action.categoryId);
		string text = actionCategory.name;
		if (text == "UI" && action.name == "Menu")
		{
			text = "Menu";
		}
		else if (text == "UI" && (action.name == "UpgradeSpell" || action.name == "RemoveSpell" || action.name == "ChooseZone" || action.name == "TopNav" || action.name == "StartMultiplayerGame"))
		{
			text = "UIGameplay";
		}
		else if (text == "UI" && action.name == "Outfit")
		{
			text = "UIGameplay2";
		}
		return ReInput.mapping.GetMapCategory(text);
	}

	private bool PollKeyboardForAssignment(out ControllerPollingInfo pollingInfo)
	{
		int num = 0;
		ControllerPollingInfo controllerPollingInfo = default(ControllerPollingInfo);
		ControllerPollingInfo controllerPollingInfo2 = default(ControllerPollingInfo);
		ModifierKeyFlags modifierKeyFlags = ModifierKeyFlags.None;
		foreach (ControllerPollingInfo item in ReInput.controllers.Keyboard.PollForAllKeys())
		{
			KeyCode keyboardKey = item.keyboardKey;
			if (keyboardKey == KeyCode.AltGr)
			{
				continue;
			}
			if (Keyboard.IsModifierKey(item.keyboardKey))
			{
				if (num == 0)
				{
					controllerPollingInfo2 = item;
				}
				modifierKeyFlags |= Keyboard.KeyCodeToModifierKeyFlags(keyboardKey);
				num++;
			}
			else if (controllerPollingInfo.keyboardKey == KeyCode.None)
			{
				controllerPollingInfo = item;
			}
		}
		if (controllerPollingInfo.keyboardKey != 0)
		{
			pollingInfo = controllerPollingInfo;
			return true;
		}
		if (num > 0 && num == 1)
		{
			pollingInfo = controllerPollingInfo2;
			return true;
		}
		pollingInfo = controllerPollingInfo;
		return false;
	}

	private void CancelBinding()
	{
		for (int i = 0; i < requestClearPlayerBindingAction.Length; i++)
		{
			requestClearPlayerBindingAction[i] = true;
		}
	}

	public override void Close()
	{
		CancelBinding();
		menuOpen = false;
		optCtrl.ClosePanel(this);
	}

	public override void CloseBase()
	{
		base.Close();
	}

	public void SetActionsToPlayers()
	{
		UpdateControlDisplays(0);
		UpdateControlDisplays(1);
	}

	private void UserInput_OnControllerConfigurationChanged()
	{
		if (menuOpen)
		{
			CancelBinding();
			RefreshControllers(null);
		}
		List<Guid> list = new List<Guid>();
		for (int i = 0; i < 2; i++)
		{
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer(i);
			if (rewiredPlayer == null)
			{
				continue;
			}
			list.Clear();
			LoadBindings(rewiredPlayer.controllers.Keyboard, i);
			foreach (Joystick joystick in rewiredPlayer.controllers.Joysticks)
			{
				if (!list.Contains(joystick.hardwareTypeGuid))
				{
					list.Add(joystick.hardwareTypeGuid);
					LoadBindings(joystick, i);
				}
			}
		}
		if (!menuOpen)
		{
			return;
		}
		if (currentBindingPlayerNum > 0)
		{
			Rewired.Player rewiredPlayer2 = RunCtrl.GetRewiredPlayer(currentBindingPlayerNum);
			if (rewiredPlayer2 != null)
			{
				if (rewiredPlayer2.controllers.joystickCount == 0)
				{
					currentBindingPlayerNum = 0;
				}
			}
			else
			{
				currentBindingPlayerNum = 0;
			}
		}
		ShowPlayerBindings(currentBindingPlayerNum, false);
	}
}
