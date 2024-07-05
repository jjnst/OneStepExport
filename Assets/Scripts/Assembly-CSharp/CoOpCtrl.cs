using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using I2.Loc;
using Kittehface.Framework20;
using Rewired;
using TMPro;
using UnityEngine;

public class CoOpCtrl : NavPanel
{
	private enum Platform
	{
		Editor = 0,
		PC = 1,
		Switch = 2,
		PS4 = 3
	}

	private Platform platform = Platform.PC;

	private BC ctrl;

	private ControlsCtrl conCtrl;

	public TMP_Text instructionText;

	public List<DeviceDisplay> deviceDisplays;

	public AudioClip setSound;

	private Controller previousActiveController;

	private Controller player1SelectedController = null;

	private Controller player2SelectedController = null;

	private float savedTimeScale = 0f;

	public float fillRate = 0.02f;

	public bool controlsProcessing = false;

	public bool aborted = false;

	private bool controllerConfigurationChanged;

	private bool previouslyTransitioning = false;

	public bool closing = false;

	private void Start()
	{
		ctrl = S.I.batCtrl;
		conCtrl = S.I.conCtrl;
	}

	private void Update()
	{
		if (controlsProcessing && (platform == Platform.Editor || platform == Platform.PC))
		{
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
			if (Input.GetMouseButtonDown(1))
			{
				ClickBack();
			}
			if ((rewiredPlayer != null && rewiredPlayer.GetButtonDown("Back")) || ReInput.players.SystemPlayer.GetButtonDown("Back"))
			{
				Abort();
			}
		}
	}

	public override void Open()
	{
		base.Open();
		btnCtrl.RemoveFocus();
		StartCoroutine(_ControlsProcess());
	}

	private IEnumerator _ControlsProcess()
	{
		aborted = false;
		controlsProcessing = true;
		previouslyTransitioning = btnCtrl.transitioning;
		btnCtrl.transitioning = false;
		if (platform == Platform.Editor || platform == Platform.PC || platform == Platform.PS4)
		{
			player1SelectedController = null;
			player2SelectedController = null;
			ReInput.ControllerPreDisconnectEvent += ReInput_ControllerPreDisconnectEvent;
		}
		for (int i = 0; i < 2; i++)
		{
			deviceDisplays[i].deviceText.text = "";
			deviceDisplays[i].fillIcon.fillAmount = 0f;
			deviceDisplays[i].fillIcon.color = Color.white;
			deviceDisplays[i].promptButton.gameObject.SetActive(false);
			deviceDisplays[i].promptButton.playerNum = i;
		}
		if (S.I.heCtrl.gameMode == GameMode.PvP)
		{
			title.text = ScriptLocalization.UI.Controls_pvp_Controls;
		}
		else
		{
			title.text = ScriptLocalization.UI.Controls_CO_OP_Controls;
		}
		savedTimeScale = Time.timeScale;
		Time.timeScale = 0f;
		if (platform == Platform.Editor || platform == Platform.PC)
		{
			Rewired.Player rewiredPlayer0 = RunCtrl.GetRewiredPlayer();
			Rewired.Player rewiredPlayer3 = RunCtrl.GetRewiredPlayer(1);
			while (rewiredPlayer0 == null || rewiredPlayer3 == null)
			{
				rewiredPlayer0 = RunCtrl.GetRewiredPlayer();
				rewiredPlayer3 = RunCtrl.GetRewiredPlayer(1);
				yield return null;
			}
			rewiredPlayer0.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Gameplay");
			rewiredPlayer0.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay");
			rewiredPlayer0.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay2");
			rewiredPlayer3.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Gameplay");
			rewiredPlayer3.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay");
			rewiredPlayer3.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay2");
			HelpIconManager.SetKeyboardAvailable(0, true);
			HelpIconManager.SetKeyboardAvailable(1, true);
			UserInput.ClearJoystickDisconnection(RunCtrl.runProfile);
			UserInput.ClearJoystickDisconnection(RunCtrl.secondaryProfile);
			yield return null;
			deviceDisplays[0].promptButton.gameObject.SetActive(true);
			instructionText.text = ScriptLocalization.UI.Controls_P1_HoldAnyButton;
			conCtrl.UpdateControlDisplays(0);
			while (!ButtonHeld(0, rewiredPlayer0))
			{
				yield return null;
			}
			instructionText.text = ScriptLocalization.UI.Controls_P1_DeviceSet;
			deviceDisplays[0].promptButton.gameObject.SetActive(false);
			if (player1SelectedController != null && player1SelectedController.type == ControllerType.Joystick)
			{
				yield return null;
				rewiredPlayer0.controllers.maps.SetMapsEnabled(false, ControllerType.Keyboard, "Gameplay");
				rewiredPlayer0.controllers.maps.SetMapsEnabled(false, ControllerType.Keyboard, "UIGameplay");
				rewiredPlayer0.controllers.maps.SetMapsEnabled(false, ControllerType.Keyboard, "UIGameplay2");
				HelpIconManager.SetKeyboardAvailable(0, false);
			}
			yield return new WaitForSecondsRealtime(0.5f);
			deviceDisplays[1].promptButton.gameObject.SetActive(true);
			deviceDisplays[1].promptButton.useFixedMapping = false;
			instructionText.text = ScriptLocalization.UI.Controls_P2_HoldAnyButton;
			conCtrl.UpdateControlDisplays(1);
			while (!ButtonHeld(1, rewiredPlayer3))
			{
				yield return null;
			}
			instructionText.text = ScriptLocalization.UI.Controls_P2_DeviceSet;
			deviceDisplays[1].promptButton.gameObject.SetActive(false);
			if (player2SelectedController != null && player2SelectedController.type == ControllerType.Joystick)
			{
				yield return null;
				rewiredPlayer3.controllers.maps.SetMapsEnabled(false, ControllerType.Keyboard, "Gameplay");
				rewiredPlayer3.controllers.maps.SetMapsEnabled(false, ControllerType.Keyboard, "UIGameplay");
				rewiredPlayer3.controllers.maps.SetMapsEnabled(false, ControllerType.Keyboard, "UIGameplay2");
				HelpIconManager.SetKeyboardAvailable(1, false);
			}
			yield return new WaitForSecondsRealtime(0.3f);
		}
		else if (platform == Platform.PS4)
		{
			Profiles.OnActivated += Profiles_OnActivated;
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
			while (rewiredPlayer == null)
			{
				rewiredPlayer = RunCtrl.GetRewiredPlayer();
				yield return null;
			}
			yield return null;
			instructionText.gameObject.SetActive(false);
			deviceDisplays[0].fillIcon.fillAmount = 1f;
			deviceDisplays[0].fillIcon.color = Color.green;
			deviceDisplays[0].promptButton.gameObject.SetActive(false);
			Controller p1Controller = rewiredPlayer.controllers.GetLastActiveController();
			conCtrl.SetController(0, p1Controller);
			conCtrl.UpdateControlDisplays(0);
			player1SelectedController = p1Controller;
			yield return new WaitForSecondsRealtime(0.5f);
			deviceDisplays[1].promptButton.gameObject.SetActive(true);
			deviceDisplays[1].promptButton.useFixedMapping = true;
			conCtrl.UpdateControlDisplays(1);
			while (!ButtonHeldPS4(1))
			{
				yield return null;
			}
			deviceDisplays[1].promptButton.gameObject.SetActive(false);
			Profiles.RequestSignIn((long)(player2SelectedController as Joystick).unityId);
			while (RunCtrl.secondaryProfile == null)
			{
				yield return null;
			}
			Profiles.OnActivated -= Profiles_OnActivated;
			yield return new WaitForSecondsRealtime(0.3f);
		}
		else if (platform == Platform.Switch)
		{
			ControllerDisconnectCtrl.trackingConfigurationChange = false;
			UserInput.OnControllerConfigurationChanged += UserInput_OnControllerConfigurationChanged;
			controllerConfigurationChanged = false;
			yield return new WaitForSecondsRealtime(0.5f);
			UserInput.SetUserCount(1, 2);
			Rewired.Player rewiredPlayer2 = RunCtrl.GetRewiredPlayer();
			Rewired.Player rewiredPlayer4 = RunCtrl.GetRewiredPlayer(1);
			while (rewiredPlayer2 == null || rewiredPlayer4 == null)
			{
				rewiredPlayer2 = RunCtrl.GetRewiredPlayer();
				rewiredPlayer4 = RunCtrl.GetRewiredPlayer(1);
				yield return null;
			}
			if (!UserInput.RequestControllerConnection())
			{
				UserInput.OnControllerConfigurationChanged -= UserInput_OnControllerConfigurationChanged;
				Abort();
				yield break;
			}
			while (!controllerConfigurationChanged)
			{
				yield return null;
			}
			yield return null;
			if (rewiredPlayer2.controllers.joystickCount < 1 || rewiredPlayer4.controllers.joystickCount < 1)
			{
				UserInput.OnControllerConfigurationChanged -= UserInput_OnControllerConfigurationChanged;
				Abort();
				yield break;
			}
			Controller player1Controller = rewiredPlayer2.controllers.GetLastActiveController(ControllerType.Joystick);
			Controller player2Controller = rewiredPlayer4.controllers.GetLastActiveController(ControllerType.Joystick);
			if (player1Controller == null)
			{
				player1Controller = rewiredPlayer2.controllers.Joysticks[0];
			}
			if (player2Controller == null)
			{
				player2Controller = rewiredPlayer4.controllers.Joysticks[0];
			}
			conCtrl.SetController(0, player1Controller);
			conCtrl.SetController(1, player2Controller);
			UserInput.OnControllerConfigurationChanged -= UserInput_OnControllerConfigurationChanged;
			ControllerDisconnectCtrl.trackingConfigurationChange = true;
		}
		TranisitionClose(true);
	}

	private void Profiles_OnActivated(Profiles.Profile profile)
	{
		if (RunCtrl.runProfile == null)
		{
			UnityEngine.Debug.Log("Set runProfile");
			RunCtrl.runProfile = profile;
		}
		else if (RunCtrl.secondaryProfile == null)
		{
			UnityEngine.Debug.Log("Set secondaryProfile");
			RunCtrl.secondaryProfile = profile;
		}
	}

	private void UserInput_OnControllerConfigurationChanged()
	{
		controllerConfigurationChanged = true;
	}

	private void ReInput_ControllerPreDisconnectEvent(ControllerStatusChangedEventArgs args)
	{
		if ((player1SelectedController != null && args.controller == player1SelectedController) || (player2SelectedController != null && args.controller == player2SelectedController))
		{
			Abort();
		}
	}

	private bool ButtonHeldPS4(int playerNum)
	{
		bool flag = true;
		foreach (Controller controller in ReInput.controllers.Controllers)
		{
			bool flag2 = false;
			if (controller.type != ControllerType.Joystick || controller == player1SelectedController || controller == player2SelectedController || !controller.GetButton(0) || (controller != previousActiveController && previousActiveController != null))
			{
				continue;
			}
			if (previousActiveController == null)
			{
				deviceDisplays[playerNum].promptButton.playerNum = playerNum;
				deviceDisplays[playerNum].promptButton.UpdateDisplay();
				deviceDisplays[playerNum].promptButton.FlashDisplay();
			}
			flag = false;
			previousActiveController = controller;
			deviceDisplays[playerNum].deviceText.text = controller.name;
			deviceDisplays[playerNum].fillIcon.fillAmount += fillRate;
			break;
		}
		if (flag)
		{
			previousActiveController = null;
			deviceDisplays[playerNum].deviceText.text = ScriptLocalization.UI.Controls_ListeningForDevice;
			deviceDisplays[playerNum].fillIcon.fillAmount = 0f;
			return false;
		}
		if (deviceDisplays[playerNum].fillIcon.fillAmount >= 1f)
		{
			deviceDisplays[playerNum].fillIcon.color = Color.green;
			conCtrl.SetController(playerNum, previousActiveController);
			conCtrl.UpdateControlDisplays(playerNum);
			switch (playerNum)
			{
			case 0:
				player1SelectedController = previousActiveController;
				break;
			case 1:
				player2SelectedController = previousActiveController;
				break;
			}
			S.I.PlayOnce(setSound);
			return true;
		}
		return false;
	}

	private bool ButtonHeld(int playerNum, Rewired.Player player)
	{
		bool flag = true;
		if (!player.GetButton("Menu"))
		{
			foreach (Controller controller in ReInput.controllers.Controllers)
			{
				bool flag2 = false;
				if (controller.type == ControllerType.Joystick && (controller == player1SelectedController || controller == player2SelectedController))
				{
					continue;
				}
				if (controller.type == ControllerType.Joystick)
				{
					flag2 = controller.GetButton(0);
				}
				else if (controller.type == ControllerType.Keyboard)
				{
					flag2 = ((Keyboard)controller).GetKey(KeyCode.Space);
				}
				if (!flag2 || (controller != previousActiveController && previousActiveController != null))
				{
					continue;
				}
				if (previousActiveController == null)
				{
					if (controller.type == ControllerType.Joystick)
					{
						if (player.controllers.ContainsController(controller))
						{
							deviceDisplays[playerNum].promptButton.playerNum = playerNum;
						}
						else if (playerNum == 0)
						{
							deviceDisplays[playerNum].promptButton.playerNum = 1;
						}
						else
						{
							deviceDisplays[playerNum].promptButton.playerNum = 0;
						}
					}
					else
					{
						deviceDisplays[playerNum].promptButton.playerNum = playerNum;
					}
					deviceDisplays[playerNum].promptButton.UpdateDisplay();
					deviceDisplays[playerNum].promptButton.FlashDisplay();
				}
				flag = false;
				previousActiveController = controller;
				if (controller.name == "Keyboard")
				{
					deviceDisplays[playerNum].deviceText.text = ScriptLocalization.Controls.controller_Keyboard;
				}
				else
				{
					deviceDisplays[playerNum].deviceText.text = controller.name;
				}
				deviceDisplays[playerNum].fillIcon.fillAmount += fillRate;
				break;
			}
		}
		if (flag)
		{
			previousActiveController = null;
			deviceDisplays[playerNum].deviceText.text = ScriptLocalization.UI.Controls_ListeningForDevice;
			deviceDisplays[playerNum].fillIcon.fillAmount = 0f;
			return false;
		}
		if (deviceDisplays[playerNum].fillIcon.fillAmount >= 1f)
		{
			deviceDisplays[playerNum].fillIcon.color = Color.green;
			if (previousActiveController.type != 0 && previousActiveController.type != ControllerType.Mouse)
			{
				player.controllers.AddController(previousActiveController, true);
			}
			conCtrl.SetController(playerNum, previousActiveController);
			conCtrl.UpdateControlDisplays(playerNum);
			switch (playerNum)
			{
			case 0:
				player1SelectedController = previousActiveController;
				break;
			case 1:
				player2SelectedController = previousActiveController;
				break;
			}
			S.I.PlayOnce(setSound);
			return true;
		}
		return false;
	}

	public override void ClickBack()
	{
		if (!closing)
		{
			Abort();
		}
	}

	public void Abort()
	{
		StopAllCoroutines();
		Time.timeScale = savedTimeScale;
		if (platform == Platform.Editor || platform == Platform.PC)
		{
			player1SelectedController = null;
			player2SelectedController = null;
			if (controlsProcessing)
			{
				ReInput.ControllerPreDisconnectEvent -= ReInput_ControllerPreDisconnectEvent;
			}
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
			Rewired.Player rewiredPlayer2 = RunCtrl.GetRewiredPlayer(1);
			if (rewiredPlayer != null)
			{
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Gameplay");
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay");
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay2");
			}
			if (rewiredPlayer2 != null)
			{
				rewiredPlayer2.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Gameplay");
				rewiredPlayer2.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay");
				rewiredPlayer2.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay2");
			}
			HelpIconManager.SetKeyboardAvailable(0, true);
			HelpIconManager.SetKeyboardAvailable(1, true);
			UserInput.ClearJoystickDisconnection(RunCtrl.runProfile);
			if (RunCtrl.secondaryProfile != null)
			{
				UserInput.ClearJoystickDisconnection(RunCtrl.secondaryProfile);
			}
		}
		else if (platform == Platform.PS4)
		{
			Profiles.OnActivated -= Profiles_OnActivated;
			player1SelectedController = null;
			player2SelectedController = null;
			if (controlsProcessing)
			{
				ReInput.ControllerPreDisconnectEvent -= ReInput_ControllerPreDisconnectEvent;
			}
			if (RunCtrl.secondaryProfile != null)
			{
				RunCtrl.secondaryProfile.Deactivate();
			}
		}
		else if (platform == Platform.Switch)
		{
			UserInput.SetUserCount(1);
			ControllerDisconnectCtrl.trackingConfigurationChange = false;
		}
		aborted = true;
		controlsProcessing = false;
		btnCtrl.transitioning = previouslyTransitioning;
		TranisitionClose(false);
	}

	public void TranisitionClose(bool setActionsToPlayers)
	{
		StartCoroutine(_TransitionClose(setActionsToPlayers));
	}

	private IEnumerator _TransitionClose(bool setActionsToPlayers)
	{
		if (closing)
		{
			yield break;
		}
		if (setActionsToPlayers)
		{
			conCtrl.SetActionsToPlayers();
		}
		closing = true;
		anim.SetBool("visible", false);
		yield return null;
		while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
		Time.timeScale = savedTimeScale;
		if (platform == Platform.Editor || platform == Platform.PC || platform == Platform.PS4)
		{
			player1SelectedController = null;
			player2SelectedController = null;
			if (controlsProcessing)
			{
				ReInput.ControllerPreDisconnectEvent -= ReInput_ControllerPreDisconnectEvent;
			}
		}
		controlsProcessing = false;
		btnCtrl.transitioning = previouslyTransitioning;
		closing = false;
		_003C_003En__0();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.Close();
	}
}
