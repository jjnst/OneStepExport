using Rewired;
using UnityEngine;

namespace Kittehface.Framework20
{
	public class TitleScreenHelper : MonoBehaviour
	{
		private enum State
		{
			Initializing = 0,
			InitializationWait = 1,
			ShowTitleScreen = 2,
			PollingForInput = 3,
			SigningIn = 4,
			SignInComplete = 5,
			SignInCanceled = 6,
			SignInError = 7,
			UserDataRequestRead = 8,
			UserDataRequestReadWait = 9,
			UserDataRequestReadComplete = 10,
			AchievementsRequestLoad = 11,
			AchievementsRequestLoadWait = 12,
			AchievementsRequestLoadComplete = 13,
			Completed = 14
		}

		[Header("Settings")]
		[SerializeField]
		private bool allowTitleScreenTransitions = false;

		[Header("Object Groups")]
		[SerializeField]
		private GameObject[] titleScreenObjects;

		[SerializeField]
		private GameObject[] loadingObjects;

		[SerializeField]
		private GameObject[] mainObjects;

		private Profiles.Profile signedInProfile = null;

		private State state = State.Initializing;

		private bool requestShowTitleScreen = false;

		private void Awake()
		{
			if (Platform.initialized)
			{
				Initialize();
				return;
			}
			Platform.OnInitialized += Platform_OnInitialized;
			state = State.InitializationWait;
			DeactivateObjects();
		}

		private void Update()
		{
			switch (state)
			{
			case State.PollingForInput:
				PollForInput();
				break;
			case State.SignInComplete:
				state = State.UserDataRequestRead;
				break;
			case State.SignInCanceled:
				ShowTitleScreen();
				break;
			case State.SignInError:
				ShowTitleScreen();
				break;
			case State.UserDataRequestRead:
				state = State.UserDataRequestReadWait;
				Platform.BeginInitialUserDataRead();
				Platform.OnUserDataReadCompleted += Platform_OnUserDataReadCompleted;
				break;
			case State.UserDataRequestReadComplete:
				state = State.AchievementsRequestLoad;
				break;
			case State.AchievementsRequestLoad:
				state = State.AchievementsRequestLoadWait;
				Platform.BeginInitialAchievementsLoad();
				Platform.OnAchievementsLoadCompleted += Platform_OnAchievementsLoadCompleted;
				break;
			case State.AchievementsRequestLoadComplete:
				state = State.Completed;
				ActivateMainObjects();
				break;
			case State.Completed:
				if (requestShowTitleScreen)
				{
					requestShowTitleScreen = false;
					ShowTitleScreen();
				}
				break;
			case State.SigningIn:
			case State.UserDataRequestReadWait:
			case State.AchievementsRequestLoadWait:
				break;
			}
		}

		public void RequestShowTitleScreen()
		{
			requestShowTitleScreen = true;
		}

		public void StartPollingForInput()
		{
			state = State.PollingForInput;
		}

		private void Initialize()
		{
			state = State.Completed;
			ActivateMainObjects();
		}

		private void PollForInput()
		{
			bool flag = false;
			long? num = null;
			bool isEditor = Platform.isEditor;
			Joystick joystick = null;
			foreach (Joystick joystick2 in ReInput.controllers.Joysticks)
			{
				if (joystick2.GetButtonDown(0))
				{
					Debug.Log("TitleScreenHelper.PollForInput(): Someone hit button 0");
					joystick = joystick2;
					break;
				}
			}
			if (joystick != null && joystick.systemId.HasValue)
			{
				flag = true;
				num = joystick.systemId;
			}
			else if (isEditor)
			{
				for (int i = 0; i < ReInput.players.playerCount; i++)
				{
					Player player = ReInput.players.GetPlayer(i);
					if (player.controllers.Keyboard.isConnected && (player.controllers.Keyboard.GetKeyDown(KeyCode.Return) || player.controllers.Keyboard.GetKeyDown(KeyCode.KeypadEnter)))
					{
						flag = true;
						break;
					}
					if (player.controllers.Mouse.isConnected && player.controllers.Mouse.GetButtonDown(0))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Player systemPlayer = ReInput.players.GetSystemPlayer();
					if (systemPlayer.controllers.Keyboard.isConnected && (systemPlayer.controllers.Keyboard.GetKeyDown(KeyCode.Return) || systemPlayer.controllers.Keyboard.GetKeyDown(KeyCode.KeypadEnter)))
					{
						flag = true;
					}
					if (systemPlayer.controllers.Mouse.isConnected && systemPlayer.controllers.Mouse.GetButtonDown(0))
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				Profiles.OnSignedIn += Profiles_OnSignedIn;
				state = State.SigningIn;
				ActivateLoadingObjects();
				Profiles.RequestSignIn(num);
			}
		}

		private void DeactivateObjects()
		{
			SetObjectsActive(titleScreenObjects, false);
			SetObjectsActive(loadingObjects, false);
			SetObjectsActive(mainObjects, false);
		}

		private void ShowTitleScreen()
		{
			Debug.Log("Showing title screen");
			if (allowTitleScreenTransitions)
			{
				state = State.ShowTitleScreen;
			}
			else
			{
				state = State.PollingForInput;
			}
			signedInProfile = null;
			ActivateTitleObjects();
		}

		private void ActivateTitleObjects()
		{
			SetObjectsActive(loadingObjects, false);
			SetObjectsActive(mainObjects, false);
			SetObjectsActive(titleScreenObjects, true);
		}

		private void ActivateLoadingObjects()
		{
			SetObjectsActive(titleScreenObjects, false);
			SetObjectsActive(mainObjects, false);
			SetObjectsActive(loadingObjects, true);
		}

		private void ActivateMainObjects()
		{
			SetObjectsActive(titleScreenObjects, false);
			SetObjectsActive(loadingObjects, false);
			SetObjectsActive(mainObjects, true);
		}

		private void SetObjectsActive(GameObject[] objects, bool active)
		{
			if (objects != null)
			{
				for (int i = 0; i < objects.Length; i++)
				{
					objects[i].SetActive(active);
				}
			}
		}

		private void Platform_OnInitialized()
		{
			Debug.Log("Platform_OnInitialized()");
			Platform.OnInitialized -= Platform_OnInitialized;
			Initialize();
		}

		private void Profiles_OnSignedIn(Profiles.Profile profile, Profiles.SignInResult result)
		{
			Debug.LogError(string.Concat("Profiles_OnSignedIn( result = ", result, " )"));
			Profiles.OnSignedIn -= Profiles_OnSignedIn;
			switch (result)
			{
			case Profiles.SignInResult.Success:
				state = State.SignInComplete;
				break;
			case Profiles.SignInResult.Canceled:
				state = State.SignInCanceled;
				break;
			default:
				state = State.SignInError;
				break;
			}
		}

		private void Platform_OnUserDataReadCompleted()
		{
			Debug.Log("TitleScreenHelper.Platform_OnUserDataReadCompleted()");
			if (State.UserDataRequestReadWait == state)
			{
				state = State.UserDataRequestReadComplete;
				Platform.OnUserDataReadCompleted -= Platform_OnUserDataReadCompleted;
			}
		}

		private void Platform_OnAchievementsLoadCompleted()
		{
			Debug.Log("TitleScreenHelper.Platform_OnAchievementsLoadCompleted()");
			if (state == State.AchievementsRequestLoadWait)
			{
				state = State.AchievementsRequestLoadComplete;
				Platform.OnAchievementsLoadCompleted -= Platform_OnAchievementsLoadCompleted;
			}
		}
	}
}
