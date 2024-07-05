using System.Collections.Generic;
using Kittehface.Framework20;
using Rewired;
using UnityEngine;

public class ControllerDisconnectCtrl : MonoBehaviour
{
	private SlideBody slideBody = null;

	private Profiles.Profile currentDisconnectProfile = null;

	public static bool controllerDisconnectInProgress { get; private set; }

	public static bool trackingConfigurationChange { get; set; }

	private void Awake()
	{
		UserInput.OnControllerDisconnected += UserInput_OnControllerDisconnected;
		UserInput.OnControllerConfigurationChanged += UserInput_OnControllerConfigurationChanged;
		slideBody = GetComponent<SlideBody>();
	}

	private void OnDestroy()
	{
		UserInput.OnControllerDisconnected -= UserInput_OnControllerDisconnected;
		UserInput.OnControllerConfigurationChanged -= UserInput_OnControllerConfigurationChanged;
	}

	private void Update()
	{
		if (controllerDisconnectInProgress && currentDisconnectProfile == null)
		{
			List<Profiles.Profile> pendingDisconnectProfiles = UserInput.GetPendingDisconnectProfiles();
			if (pendingDisconnectProfiles != null && pendingDisconnectProfiles.Count > 0)
			{
				currentDisconnectProfile = pendingDisconnectProfiles[0];
			}
			else
			{
				slideBody.Hide();
				controllerDisconnectInProgress = false;
			}
		}
		if (controllerDisconnectInProgress && currentDisconnectProfile != null)
		{
			foreach (Joystick joystick in ReInput.controllers.Joysticks)
			{
				if (joystick.GetButtonUp(0))
				{
					UserInput.ReconnectJoystick(currentDisconnectProfile, joystick);
					currentDisconnectProfile = null;
					break;
				}
			}
		}
		if (controllerDisconnectInProgress && currentDisconnectProfile != null && ReInput.controllers.Keyboard != null && (ReInput.controllers.Keyboard.GetKeyUp(KeyCode.Space) || ReInput.controllers.Keyboard.GetKeyUp(KeyCode.Escape)))
		{
			Rewired.Player rewiredPlayer = UserInput.GetRewiredPlayer(currentDisconnectProfile);
			if (rewiredPlayer != null)
			{
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Gameplay");
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay");
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay2");
			}
			if (currentDisconnectProfile == RunCtrl.runProfile)
			{
				HelpIconManager.SetKeyboardAvailable(0, true);
			}
			else if (currentDisconnectProfile == RunCtrl.secondaryProfile)
			{
				HelpIconManager.SetKeyboardAvailable(1, true);
			}
			UserInput.ClearJoystickDisconnection(currentDisconnectProfile);
			currentDisconnectProfile = null;
		}
	}

	private void UserInput_OnControllerConfigurationChanged()
	{
	}

	private void UserInput_OnControllerDisconnected(Profiles.Profile profile)
	{
		if (S.I.heCtrl.gameMode != 0)
		{
			controllerDisconnectInProgress = true;
			if (!S.I.conCtrl.IsRewiredBindingInProgress(0) && !S.I.conCtrl.IsRewiredBindingInProgress(1) && !S.I.btnCtrl.activeNavPanels.Contains(S.I.optCtrl) && !S.I.coOpCtrl.controlsProcessing)
			{
				S.I.optCtrl.Open();
			}
			slideBody.Show();
		}
	}
}
