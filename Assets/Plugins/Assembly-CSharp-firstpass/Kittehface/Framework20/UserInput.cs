using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace Kittehface.Framework20
{
	public class UserInput
	{
		private class SteamUserInputImpl : UserInputImpl
		{
			private UserInputImpl_Default defaultImplementation = new UserInputImpl_Default();

			public override void ReconnectJoystick(Profiles.Profile profile, Joystick joystick)
			{
				defaultImplementation.ReconnectJoystick(profile, joystick);
			}

			public override void ClearJoystickDisconnection(Profiles.Profile profile)
			{
				defaultImplementation.ClearJoystickDisconnection(profile);
			}

			public override void ActivateProfile(Profiles.Profile profile)
			{
				defaultImplementation.ActivateProfile(profile);
			}

			public override void Rewired_OnControllerConnected(ControllerStatusChangedEventArgs args)
			{
				defaultImplementation.Rewired_OnControllerConnected(args);
			}

			public override void Rewired_OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
			{
				defaultImplementation.Rewired_OnControllerPreDisconnect(args);
			}
		}

		public delegate void ControllerConnectedAction(Profiles.Profile profile, int controllerId);

		public delegate void ControllerDisconnectedAction(Profiles.Profile profile);

		public delegate void ProfileControllersReadyAction(Profiles.Profile profile);

		private class Data
		{
			public Player player;

			public int playerIndex;

			public bool disconnectInProgress;

			public Data()
			{
			}

			public Data(Player player, int playerIndex)
			{
				this.player = player;
				this.playerIndex = playerIndex;
			}

			public bool OwnsJoystick(int controllerID)
			{
				if (player == null)
				{
					return false;
				}
				for (int i = 0; i < player.controllers.Joysticks.Count; i++)
				{
					if (player.controllers.Joysticks[i].id == controllerID)
					{
						return true;
					}
				}
				return false;
			}
		}

		private class UserInputImpl
		{
			public static UserInputImpl GetUserInputImpl()
			{
				return new SteamUserInputImpl();
			}

			protected UserInputImpl()
			{
			}

			public virtual void Initialize()
			{
			}

			public virtual void SetUserCount(int minUserCount, int maxUserCount)
			{
			}

			public virtual void ActivateProfile(Profiles.Profile profile)
			{
			}

			public virtual void DeactivateProfile(Profiles.Profile profile)
			{
			}

			public virtual bool RequestControllerConnection()
			{
				return false;
			}

			public virtual void ReconnectJoystick(Profiles.Profile profile, Joystick joystick)
			{
			}

			public virtual void ClearJoystickDisconnection(Profiles.Profile profile)
			{
			}

			public virtual string GetControllerSummary()
			{
				return "";
			}

			public virtual void Rewired_OnControllerConnected(ControllerStatusChangedEventArgs args)
			{
			}

			public virtual void Rewired_OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
			{
			}
		}

		private class UserInputImpl_Default
		{
			public void ReconnectJoystick(Profiles.Profile profile, Joystick joystick)
			{
				Profiles.Profile profile2 = null;
				foreach (KeyValuePair<Profiles.Profile, Data> profileDatum in profileData)
				{
					if (profileDatum.Value.OwnsJoystick(joystick.id) && profileDatum.Key != profile)
					{
						if (!profileDatum.Value.disconnectInProgress)
						{
							profile2 = profileDatum.Key;
							profileDatum.Value.disconnectInProgress = true;
						}
						break;
					}
				}
				if (!profileData.ContainsKey(profile))
				{
					return;
				}
				Data data = profileData[profile];
				if (!data.player.controllers.ContainsController(joystick))
				{
					if (data.player.controllers.joystickCount > 0 && data.player.controllers.joystickCount >= ReInput.configuration.maxJoysticksPerPlayer)
					{
						RemoveOldestJoystick(data.player, null);
					}
					data.disconnectInProgress = false;
					data.player.controllers.AddController(joystick, true);
					if (profile2 != null && UserInput.OnControllerDisconnected != null)
					{
						UserInput.OnControllerDisconnected(profile2);
					}
					if (UserInput.OnControllerConfigurationChanged != null)
					{
						UserInput.OnControllerConfigurationChanged();
					}
				}
			}

			public void ClearJoystickDisconnection(Profiles.Profile profile)
			{
				if (profileData.ContainsKey(profile))
				{
					profileData[profile].disconnectInProgress = false;
				}
			}

			public void ActivateProfile(Profiles.Profile profile)
			{
				Data data = profileData[profile];
				bool flag = false;
				while (data.player.controllers.joystickCount > ReInput.configuration.maxJoysticksPerPlayer)
				{
					RemoveOldestJoystick(data.player, null);
				}
				if (profile.LoginJoystickSystemID.HasValue)
				{
					foreach (Joystick joystick in ReInput.controllers.Joysticks)
					{
						if (joystick.systemId != profile.LoginJoystickSystemID)
						{
							continue;
						}
						Profiles.Profile profile2 = null;
						foreach (KeyValuePair<Profiles.Profile, Data> profileDatum in profileData)
						{
							if (profileDatum.Value.OwnsJoystick(joystick.id) && profileDatum.Key != profile)
							{
								if (!profileDatum.Value.disconnectInProgress)
								{
									profile2 = profileDatum.Key;
									profileDatum.Value.disconnectInProgress = true;
								}
								break;
							}
						}
						if (!data.player.controllers.ContainsController(joystick))
						{
							if (data.player.controllers.joystickCount > 0 && data.player.controllers.joystickCount >= ReInput.configuration.maxJoysticksPerPlayer)
							{
								RemoveOldestJoystick(data.player, null);
							}
							flag = true;
							data.player.controllers.AddController(joystick, true);
							if (profile2 != null && UserInput.OnControllerDisconnected != null)
							{
								UserInput.OnControllerDisconnected(profile2);
							}
						}
						break;
					}
				}
				data.disconnectInProgress = false;
				if (flag && UserInput.OnControllerConfigurationChanged != null)
				{
					UserInput.OnControllerConfigurationChanged();
				}
				if (UserInput.OnProfileControllersReady != null)
				{
					UserInput.OnProfileControllersReady(profile);
				}
			}

			public void Rewired_OnControllerConnected(ControllerStatusChangedEventArgs args)
			{
				int controllerId = args.controllerId;
				foreach (KeyValuePair<Profiles.Profile, Data> profileDatum in profileData)
				{
					if (profileDatum.Value.OwnsJoystick(controllerId))
					{
						profileDatum.Value.disconnectInProgress = false;
						if (UserInput.OnControllerConnected != null)
						{
							UserInput.OnControllerConnected(profileDatum.Key, controllerId);
						}
						if (UserInput.OnControllerConfigurationChanged != null)
						{
							UserInput.OnControllerConfigurationChanged();
						}
						break;
					}
				}
			}

			public void Rewired_OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
			{
				foreach (KeyValuePair<Profiles.Profile, Data> profileDatum in profileData)
				{
					if (!profileDatum.Value.OwnsJoystick(args.controllerId))
					{
						continue;
					}
					if (!profileDatum.Value.disconnectInProgress)
					{
						profileDatum.Value.disconnectInProgress = true;
						if (UserInput.OnControllerDisconnected != null)
						{
							UserInput.OnControllerDisconnected(profileDatum.Key);
						}
						if (UserInput.OnControllerConfigurationChanged != null)
						{
							UserInput.OnControllerConfigurationChanged();
						}
					}
					break;
				}
			}

			private void RemoveOldestJoystick(Player player, Controller desiredRemoveJoystick)
			{
				while (player.controllers.joystickCount > 0 && player.controllers.joystickCount > ReInput.configuration.maxJoysticksPerPlayer - 1)
				{
					Controller controller = null;
					if (desiredRemoveJoystick != null && player.controllers.ContainsController(desiredRemoveJoystick))
					{
						controller = desiredRemoveJoystick;
						desiredRemoveJoystick = null;
					}
					if (controller == null)
					{
						float num = -1f;
						foreach (Joystick joystick in player.controllers.Joysticks)
						{
							if (num == -1f || joystick.GetLastTimeActive() < num)
							{
								controller = joystick;
								num = joystick.GetLastTimeActive();
							}
						}
					}
					if (controller != null)
					{
						player.controllers.RemoveController(controller);
						continue;
					}
					break;
				}
			}
		}

		private static UserInputImpl implementation;

		private static Dictionary<Profiles.Profile, Data> profileData = new Dictionary<Profiles.Profile, Data>();

		public static bool initialized { get; private set; }

		public static int userCountMin { get; private set; }

		public static int userCountMax { get; private set; }

		public static event ControllerConnectedAction OnControllerConnected;

		public static event ControllerDisconnectedAction OnControllerDisconnected;

		public static event ProfileControllersReadyAction OnProfileControllersReady;

		public static event Action OnControllerConfigurationChanged;

		public static void Initialize()
		{
			if (initialized)
			{
				return;
			}
			initialized = true;
			foreach (Player player in ReInput.players.Players)
			{
				player.isPlaying = false;
			}
			implementation = UserInputImpl.GetUserInputImpl();
			userCountMin = 1;
			userCountMax = 1;
			Profiles.OnActivated += Profile_OnActivated;
			Profiles.OnDeactivated += Profile_OnDeactivated;
			ReInput.ControllerConnectedEvent += Rewired_OnJoystickConnected;
			ReInput.ControllerPreDisconnectEvent += Rewired_OnJoystickPreDisconnect;
			implementation.Initialize();
		}

		public static Player GetRewiredPlayer(Profiles.Profile profile)
		{
			if (profileData.ContainsKey(profile))
			{
				return profileData[profile].player;
			}
			return null;
		}

		public static float GetAxis(Profiles.Profile profile, string action)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetAxis(action);
			}
			return 0f;
		}

		public static float GetAxis(Profiles.Profile profile, int actionID)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetAxis(actionID);
			}
			return 0f;
		}

		public static float GetAxisRaw(Profiles.Profile profile, string action)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetAxisRaw(action);
			}
			return 0f;
		}

		public static float GetAxisRaw(Profiles.Profile profile, int actionID)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetAxisRaw(actionID);
			}
			return 0f;
		}

		public static float GetAxisPrev(Profiles.Profile profile, string action)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetAxisPrev(action);
			}
			return 0f;
		}

		public static float GetAxisPrev(Profiles.Profile profile, int actionID)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetAxisPrev(actionID);
			}
			return 0f;
		}

		public static float GetAxisRawPrev(Profiles.Profile profile, string action)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetAxisRawPrev(action);
			}
			return 0f;
		}

		public static float GetAxisRawPrev(Profiles.Profile profile, int actionID)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetAxisRawPrev(actionID);
			}
			return 0f;
		}

		public static bool GetButtonDown(Profiles.Profile profile, string action)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetButtonDown(action);
			}
			return false;
		}

		public static bool GetButtonDown(Profiles.Profile profile, int actionID)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetButtonDown(actionID);
			}
			return false;
		}

		public static bool GetButton(Profiles.Profile profile, string action)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetButton(action);
			}
			return false;
		}

		public static bool GetButton(Profiles.Profile profile, int actionID)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetButton(actionID);
			}
			return false;
		}

		public static bool GetButtonUp(Profiles.Profile profile, string action)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetButtonUp(action);
			}
			return false;
		}

		public static bool GetButtonUp(Profiles.Profile profile, int actionID)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.GetButtonUp(actionID);
			}
			return false;
		}

		public static void SetUserCount(int userCount)
		{
			SetUserCount(userCount, userCount);
		}

		public static void SetUserCount(int userCountMin, int userCountMax)
		{
			if (userCountMin < 0 || userCountMax < 0)
			{
				throw new ArgumentException("UserInput.SetUserCount: User count cannot be less than zero.");
			}
			if (userCountMax < userCountMin)
			{
				throw new ArgumentException("UserInput.SetUserCount: Maxmum user count must be equal to or greater than minimum user count!");
			}
			if (UserInput.userCountMin != userCountMin || UserInput.userCountMax != userCountMax)
			{
				implementation.SetUserCount(userCountMin, userCountMax);
				UserInput.userCountMin = userCountMin;
				UserInput.userCountMax = userCountMax;
			}
		}

		public static Player.ControllerHelper GetControllers(Profiles.Profile profile)
		{
			if (profileData.ContainsKey(profile) && profileData[profile].player != null)
			{
				return profileData[profile].player.controllers;
			}
			return null;
		}

		public static bool RequestControllerConnection()
		{
			return implementation.RequestControllerConnection();
		}

		public static void ReconnectJoystick(Profiles.Profile profile, Joystick joystick)
		{
			implementation.ReconnectJoystick(profile, joystick);
		}

		public static void ClearJoystickDisconnection(Profiles.Profile profile)
		{
			implementation.ClearJoystickDisconnection(profile);
		}

		public static Profiles.Profile GetJoystickProfile(Joystick joystick)
		{
			if (joystick != null)
			{
				foreach (KeyValuePair<Profiles.Profile, Data> profileDatum in profileData)
				{
					if (profileDatum.Value.OwnsJoystick(joystick.id))
					{
						return profileDatum.Key;
					}
				}
			}
			return null;
		}

		public static string GetControllerSummary()
		{
			return implementation.GetControllerSummary();
		}

		public static List<Profiles.Profile> GetPendingDisconnectProfiles()
		{
			List<Profiles.Profile> list = new List<Profiles.Profile>();
			foreach (KeyValuePair<Profiles.Profile, Data> profileDatum in profileData)
			{
				if (profileDatum.Value.disconnectInProgress)
				{
					list.Add(profileDatum.Key);
				}
			}
			return list;
		}

		private static void Profile_OnActivated(Profiles.Profile profile)
		{
			bool flag = false;
			for (int i = 0; i < ReInput.players.playerCount; i++)
			{
				bool flag2 = false;
				foreach (Data value in profileData.Values)
				{
					if (value.playerIndex == i)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					continue;
				}
				Data data = new Data(ReInput.players.GetPlayer(i), i);
				profileData.Add(profile, data);
				data.player.isPlaying = true;
				foreach (Joystick joystick in ReInput.controllers.Joysticks)
				{
					bool flag3 = false;
					foreach (Player player in ReInput.players.Players)
					{
						if (player.controllers.ContainsController(joystick))
						{
							flag3 = true;
							break;
						}
					}
					if (!flag3)
					{
						ReInput.controllers.AutoAssignJoystick(joystick);
					}
				}
				flag = true;
				break;
			}
			if (!flag)
			{
				Debug.LogError("Could not find available Rewired player for new profile");
			}
			else
			{
				implementation.ActivateProfile(profile);
			}
		}

		private static void Profile_OnDeactivated(Profiles.Profile profile)
		{
			if (profileData.ContainsKey(profile))
			{
				profileData[profile].player.isPlaying = false;
				implementation.DeactivateProfile(profile);
				profileData.Remove(profile);
			}
		}

		private static void Rewired_OnJoystickConnected(ControllerStatusChangedEventArgs args)
		{
			implementation.Rewired_OnControllerConnected(args);
		}

		private static void Rewired_OnJoystickPreDisconnect(ControllerStatusChangedEventArgs args)
		{
			implementation.Rewired_OnControllerPreDisconnect(args);
		}
	}
}
