using System;
using System.Collections.Generic;
using Rewired;
using Steamworks;
using UnityEngine;

namespace Kittehface.Framework20
{
	public class Profiles
	{
		public class Profile
		{
			private bool deactivateInProgress = false;

			private bool canDeactivate = false;

			private string dummyDisplayName;

			public bool isEmulated { get; protected set; }

			public bool isDummy { get; protected set; }

			public long? LoginJoystickSystemID { get; set; }

			protected Profile()
			{
			}

			protected Profile(bool isDummy, string dummyDisplayName = null)
			{
				this.isDummy = isDummy;
				this.dummyDisplayName = dummyDisplayName;
			}

			public virtual void Deactivate()
			{
				if (!deactivateInProgress && ActiveProfiles.Contains(this))
				{
					deactivateInProgress = true;
					canDeactivate = false;
					if (Profiles.OnWillDeactivate != null)
					{
						Profiles.OnWillDeactivate(this);
					}
					if (Profiles.OnLateWillDeactivate != null)
					{
						Profiles.OnLateWillDeactivate(this);
					}
					Platform.OnUpdate += Platform_OnUpdate;
					Platform.OnLateUpdate += Platform_OnLateUpdate;
				}
			}

			public virtual string GetDisplayName()
			{
				if (isDummy)
				{
					return dummyDisplayName;
				}
				return null;
			}

			private void Platform_OnUpdate()
			{
				if (deactivateInProgress)
				{
					if (canDeactivate && !Platform.IsInCriticalSection())
					{
						if (Profiles.OnDeactivated != null)
						{
							Profiles.OnDeactivated(this);
						}
						ActiveProfiles.Remove(this);
						Platform.OnUpdate -= Platform_OnUpdate;
					}
				}
				else
				{
					Platform.OnUpdate -= Platform_OnUpdate;
				}
			}

			private void Platform_OnLateUpdate()
			{
				canDeactivate = true;
				Platform.OnLateUpdate -= Platform_OnLateUpdate;
			}
		}

		private class ProfilesImpl
		{
			public static ProfilesImpl GetProfilesImpl()
			{
				return new SteamProfilesImpl();
			}

			protected ProfilesImpl()
			{
			}

			public virtual void Initialize()
			{
			}

			public virtual void RequestSignIn(params object[] args)
			{
				if (Profiles.OnSignedIn != null)
				{
					Profiles.OnSignedIn(null, SignInResult.Error);
				}
			}

			public virtual void RequestDummyProfile(string displayName, Controller desiredController)
			{
			}
		}

		public enum SignInResult
		{
			Success = 0,
			Canceled = 1,
			Error = 2
		}

		public class SteamProfile : Profile
		{
			public SteamProfile()
			{
				Debug.Log("Constructed SteamProfile.");
				if (Platform.isEditor)
				{
					base.isEmulated = true;
				}
			}

			public SteamProfile(bool isDummy = false, string dummyDisplayName = null)
				: base(isDummy, dummyDisplayName)
			{
			}

			public override string GetDisplayName()
			{
				if (base.isDummy)
				{
					return base.GetDisplayName();
				}
				if (base.isEmulated)
				{
					return "Emulated Profile";
				}
				if (Platform.SteamInitialized)
				{
					return SteamFriends.GetPersonaName();
				}
				return "";
			}

			public CSteamID GetSteamID()
			{
				if (base.isDummy || !Platform.SteamInitialized)
				{
					return CSteamID.Nil;
				}
				return SteamUser.GetSteamID();
			}
		}

		private class SteamProfilesImpl : ProfilesImpl
		{
			public override void Initialize()
			{
				initialized = true;
			}

			public override void RequestSignIn(params object[] args)
			{
				long? loginJoystickSystemID = (long?)args[0];
				Profile profile = new SteamProfile();
				profile.LoginJoystickSystemID = loginJoystickSystemID;
				ActivateProfile(profile);
				if (Profiles.OnSignedIn != null)
				{
					Profiles.OnSignedIn(profile, SignInResult.Success);
				}
			}

			public override void RequestDummyProfile(string displayName, Controller desiredController)
			{
				Profile profile = new SteamProfile(true, displayName);
				if (desiredController != null && desiredController.type == ControllerType.Joystick)
				{
					profile.LoginJoystickSystemID = ((Joystick)desiredController).systemId;
				}
				ActivateProfile(profile);
			}
		}

		private static ProfilesImpl implementation;

		private static bool initialized = false;

		public static List<Profile> ActiveProfiles = new List<Profile>();

		public static event Action<Profile> OnActivated;

		public static event Action<Profile> OnDeactivated;

		public static event Action<Profile> OnWillDeactivate;

		public static event Action<Profile> OnLateWillDeactivate;

		public static event Action<Profile, SignInResult> OnSignedIn;

		public static event Action<Profile> OnSignedOut;

		public static event Action OnSignInCanceled;

		public static event Action OnSignInError;

		public static void Initialize()
		{
			if (!initialized)
			{
				implementation = ProfilesImpl.GetProfilesImpl();
				implementation.Initialize();
			}
		}

		public static void RequestSignIn(params object[] args)
		{
			if (!initialized)
			{
				throw new InvalidOperationException("Profiles.RequestSignIn() cannot be called before Profiles has been initialized.");
			}
			implementation.RequestSignIn(args);
		}

		public static void RequestDummyProfile(string displayName, Controller desiredController = null)
		{
			if (!initialized)
			{
				throw new InvalidOperationException("Profiles.RequestDummyProfile() cannot be called before Profiles has been initialized.");
			}
			implementation.RequestDummyProfile(displayName, desiredController);
		}

		private static void ActivateProfile(Profile profile)
		{
			if (!ActiveProfiles.Contains(profile))
			{
				ActiveProfiles.Add(profile);
				if (Profiles.OnActivated != null)
				{
					Profiles.OnActivated(profile);
				}
			}
		}

		public static bool ActivateSteamUser()
		{
			Profile profile = new SteamProfile();
			ActivateProfile(profile);
			return true;
		}
	}
}
