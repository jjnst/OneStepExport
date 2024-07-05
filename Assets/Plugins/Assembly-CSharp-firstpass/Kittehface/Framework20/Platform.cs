using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Steamworks;
using UnityEngine;

namespace Kittehface.Framework20
{
	public class Platform : MonoBehaviour
	{
		private class PlatformImpl
		{
			public static PlatformImpl GetPlatformImpl()
			{
				return new SteamPlatformImpl();
			}

			protected PlatformImpl()
			{
			}

			public virtual void SetPlatformMetadata(object metadata)
			{
			}

			public virtual void Awake()
			{
				if (Platform.OnSystemLanguageSet != null)
				{
					Platform.OnSystemLanguageSet();
				}
			}

			public virtual void OnDestroy()
			{
			}

			public virtual void OnEnable()
			{
			}

			public virtual void OnDisable()
			{
			}

			public virtual void Update()
			{
			}

			public virtual void LateUpdate()
			{
			}

			public virtual void OnApplicationPause(bool pause)
			{
			}

			public virtual Language GetLanguage()
			{
				return (Language)Application.systemLanguage;
			}
		}

		private class SteamPlatformImpl : PlatformImpl
		{
			private enum InitializationState
			{
				Startup = 0,
				RequestPlatformMetadata = 1,
				InitializeSteam = 2,
				ProfileInitialize = 3,
				UserInputInitialize = 4,
				ProfileActivate = 5,
				UserDataInitialize = 6,
				UserDataInitializeWait = 7,
				AchievementsInitialize = 8,
				UserDataRequestRead = 9,
				UserDataRequestReadWait = 10,
				AchievementsRequestLoad = 11,
				AchievementsRequestLoadWait = 12,
				NotifyListeners = 13,
				Complete = 14
			}

			private SteamAPIWarningMessageHook_t steamAPIWarningMessageCallback;

			private AppId_t appID;

			private bool appIDSet = false;

			private InitializationState initializationState = InitializationState.Startup;

			public override void Awake()
			{
				initializationState = InitializationState.Startup;
				Profiles.OnActivated += Profile_OnActivated;
				UserData.OnInitialized += UserData_OnInitialized;
				if (!Packsize.Test())
				{
					Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", instance);
				}
				if (!DllCheck.Test())
				{
					Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", instance);
				}
			}

			public override void OnEnable()
			{
				if (steamInitialized && steamAPIWarningMessageCallback == null)
				{
					steamAPIWarningMessageCallback = SteamClient_OnWarningMessage;
					SteamClient.SetWarningMessageHook(steamAPIWarningMessageCallback);
				}
			}

			public override void OnDestroy()
			{
				Profiles.OnActivated -= Profile_OnActivated;
				Achievements.Destroy();
				UserData.Shutdown();
				if (steamInitialized)
				{
					SteamAPI.Shutdown();
				}
			}

			public override void Update()
			{
				if (steamInitialized && (!isEditor || RunSteamInEditor))
				{
					SteamAPI.RunCallbacks();
				}
				switch (initializationState)
				{
				case InitializationState.Startup:
					initializationState = InitializationState.RequestPlatformMetadata;
					break;
				case InitializationState.RequestPlatformMetadata:
					if (Platform.OnRequestPlatformMetadata != null)
					{
						Platform.OnRequestPlatformMetadata();
					}
					initializationState = InitializationState.InitializeSteam;
					break;
				case InitializationState.InitializeSteam:
					if (!isEditor || RunSteamInEditor)
					{
						if (!appIDSet)
						{
							Debug.LogError("Platform.SteamPlatformImpl(): No app ID metadata set, cannot start Steam!", instance);
							Application.Quit();
							break;
						}
						try
						{
							if (SteamAPI.RestartAppIfNecessary(appID))
							{
								Application.Quit();
								break;
							}
						}
						catch (DllNotFoundException ex)
						{
							Debug.LogError("Platform.SteamPlatformImpl(): Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex, instance);
							Application.Quit();
							break;
						}
						steamInitialized = SteamAPI.Init();
						if (!steamInitialized)
						{
							Debug.LogError("Platform.SteamPlatformImpl(): SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", instance);
						}
						else
						{
							if (steamAPIWarningMessageCallback == null)
							{
								steamAPIWarningMessageCallback = SteamClient_OnWarningMessage;
								SteamClient.SetWarningMessageHook(steamAPIWarningMessageCallback);
							}
							string currentGameLanguage = SteamApps.GetCurrentGameLanguage();
							Language systemLanguage;
							if (TryGetSystemLanguageFromSteamLanguage(currentGameLanguage, out systemLanguage))
							{
								Platform.systemLanguage = systemLanguage;
							}
						}
					}
					if (Platform.OnSystemLanguageSet != null)
					{
						Platform.OnSystemLanguageSet();
					}
					initializationState = InitializationState.ProfileInitialize;
					break;
				case InitializationState.ProfileInitialize:
					Profiles.Initialize();
					initializationState = InitializationState.UserInputInitialize;
					break;
				case InitializationState.UserInputInitialize:
					UserInput.Initialize();
					initializationState = InitializationState.ProfileActivate;
					break;
				case InitializationState.ProfileActivate:
					if (Profiles.ActivateSteamUser())
					{
						Debug.Log("Platform:Awake(): Successfully activated standalone user.");
					}
					else
					{
						Debug.LogWarning("Platform:Awake(): Error activating standalone user.");
					}
					initializationState = InitializationState.UserDataInitialize;
					break;
				case InitializationState.UserDataInitialize:
					initializationState = InitializationState.UserDataInitializeWait;
					UserData.Initialize();
					break;
				case InitializationState.AchievementsInitialize:
					initializationState = InitializationState.UserDataRequestRead;
					Achievements.Initialize();
					if (Platform.OnRequestAchievementsMetadata != null)
					{
						Platform.OnRequestAchievementsMetadata();
					}
					break;
				case InitializationState.UserDataRequestRead:
					initializationState = InitializationState.UserDataRequestReadWait;
					BeginInitialUserDataRead();
					break;
				case InitializationState.UserDataRequestReadWait:
					lock (pendingUserDataReadsLock)
					{
						if (pendingUserDataReads.Count == 0)
						{
							Debug.Log("PLATFORM: All file reads complete, proceeding with initialization!");
							initializationState = InitializationState.AchievementsRequestLoad;
						}
						break;
					}
				case InitializationState.AchievementsRequestLoad:
					initializationState = InitializationState.AchievementsRequestLoadWait;
					BeginInitialAchievementsLoad();
					break;
				case InitializationState.AchievementsRequestLoadWait:
					lock (pendingAchievementsLoadsLock)
					{
						if (pendingAchievementsLoads.Count == 0)
						{
							Debug.Log("PLATFORM: All Achievements loads complete, proceeding with initialization!");
							initializationState = InitializationState.NotifyListeners;
						}
						break;
					}
				case InitializationState.NotifyListeners:
					initialized = true;
					if (Platform.OnInitialized != null)
					{
						Platform.OnInitialized();
					}
					initializationState = InitializationState.Complete;
					break;
				case InitializationState.UserDataInitializeWait:
					break;
				}
			}

			public override void SetPlatformMetadata(object metadata)
			{
				if (metadata is uint)
				{
					appID = new AppId_t((uint)metadata);
					appIDSet = true;
				}
			}

			private void Profile_OnActivated(Profiles.Profile profile)
			{
				Debug.Log("SteamPlatformImpl:Profile_OnActivated(): Profile activated: \n" + profile.ToString());
			}

			private void UserData_OnInitialized()
			{
				if (InitializationState.UserDataInitialize == initializationState || InitializationState.UserDataInitializeWait == initializationState)
				{
					initializationState = InitializationState.AchievementsInitialize;
				}
				else
				{
					Debug.LogError("Platform:UserData_OnInitialized(): Unexpected UserData:OnInitialized event!");
				}
			}

			private void SteamClient_OnWarningMessage(int severity, StringBuilder debugText)
			{
				Debug.LogWarning(debugText);
			}

			private bool TryGetSystemLanguageFromSteamLanguage(string steamLanguage, out Language systemLanguage)
			{
				systemLanguage = Language.Unknown;
				steamLanguage = (string.IsNullOrEmpty(steamLanguage) ? "" : steamLanguage.ToLowerInvariant());
				switch (steamLanguage)
				{
				case "arabic":
					systemLanguage = Language.Arabic;
					break;
				case "brazilian":
					systemLanguage = Language.Portuguese;
					break;
				case "bulgarian":
					systemLanguage = Language.Bulgarian;
					break;
				case "czech":
					systemLanguage = Language.Czech;
					break;
				case "danish":
					systemLanguage = Language.Danish;
					break;
				case "dutch":
					systemLanguage = Language.Dutch;
					break;
				case "english":
					systemLanguage = Language.English;
					break;
				case "finnish":
					systemLanguage = Language.Finnish;
					break;
				case "french":
					systemLanguage = Language.French;
					break;
				case "german":
					systemLanguage = Language.German;
					break;
				case "greek":
					systemLanguage = Language.Greek;
					break;
				case "hungarian":
					systemLanguage = Language.Hungarian;
					break;
				case "italian":
					systemLanguage = Language.Italian;
					break;
				case "japanese":
					systemLanguage = Language.Japanese;
					break;
				case "koreana":
					systemLanguage = Language.Korean;
					break;
				case "norwegian":
					systemLanguage = Language.Norwegian;
					break;
				case "polish":
					systemLanguage = Language.Polish;
					break;
				case "portuguese":
					systemLanguage = Language.Portuguese;
					break;
				case "romanian":
					systemLanguage = Language.Romanian;
					break;
				case "russian":
					systemLanguage = Language.Russian;
					break;
				case "schinese":
					systemLanguage = Language.ChineseSimplified;
					break;
				case "spanish":
					systemLanguage = Language.Spanish;
					break;
				case "swedish":
					systemLanguage = Language.Swedish;
					break;
				case "tchinese":
					systemLanguage = Language.ChineseTraditional;
					break;
				case "thai":
					systemLanguage = Language.Thai;
					break;
				case "turkish":
					systemLanguage = Language.Turkish;
					break;
				case "ukrainian":
					systemLanguage = Language.Ukrainian;
					break;
				}
				return systemLanguage != Language.Unknown;
			}
		}

		private static object criticalSectionLock = new object();

		private static int criticalSectionReferenceCount = 0;

		private static List<object> criticalSectionReferences = new List<object>();

		private static bool performingInitialAchievementsLoad = false;

		private static List<object> pendingAchievementsLoads = new List<object>();

		private static object pendingAchievementsLoadsLock = new object();

		private static bool performingInitialUserDataRead = false;

		private static List<object> pendingUserDataReads = new List<object>();

		private static object pendingUserDataReadsLock = new object();

		[Header("iOS")]
		[SerializeField]
		private bool useiOSPrime31GameCenter = false;

		[Header("Language")]
		[SerializeField]
		private bool overrideSystemLanguage = false;

		[SerializeField]
		private Language overrideLanguage = Language.English;

		private static PlatformImpl implementation;

		private static Platform instance;

		[Header("Standalone Game Center")]
		[SerializeField]
		private bool useGameCenter = true;

		[SerializeField]
		private bool useiCloud = true;

		[SerializeField]
		private bool useStandaloneOSXPrime31GameCenter = false;

		[Header("Standalonge GOG")]
		[SerializeField]
		private bool useGOGGalaxy = false;

		private static bool steamInitialized = false;

		[Header("tvOS")]
		[SerializeField]
		private bool usetvOSPrime31GameCenter = false;

		public static bool isEditor { get; private set; }

		public static Thread mainThread { get; private set; }

		public static Language systemLanguage { get; private set; }

		public static bool initialized { get; private set; }

		public static bool RunSteamInEditor
		{
			get
			{
				return false;
			}
		}

		public static bool SteamInitialized
		{
			get
			{
				return steamInitialized;
			}
		}

		public static event Action<List<object>> OnRequestAchievementsLoad;

		public static event Action OnAchievementsLoadCompleted;

		public static event Action<List<object>> OnRequestUserDataRead;

		public static event Action OnUserDataReadCompleted;

		public static event Action OnInitialized;

		public static event Action OnUpdate;

		public static event Action OnLateUpdate;

		public static event Action<bool> OnSystemPause;

		public static event Action OnRequestPlatformMetadata;

		public static event Action OnRequestAchievementsMetadata;

		public static event Action OnRequestRichPresenceMetadata;

		public static event Action OnRequestUserDataMetadata;

		public static event Action OnSystemLanguageSet;

		public static bool IsInCriticalSection()
		{
			lock (criticalSectionLock)
			{
				return criticalSectionReferenceCount > 0 || criticalSectionReferences.Count > 0;
			}
		}

		public static void EnterCriticalSection(object criticalSectionReference = null)
		{
			lock (criticalSectionLock)
			{
				if (criticalSectionReference == null)
				{
					int num = criticalSectionReferenceCount;
					criticalSectionReferenceCount++;
					bool flag = num == 0 && criticalSectionReferenceCount > 0;
				}
				else if (criticalSectionReferences.Contains(criticalSectionReference))
				{
					bool flag = false;
				}
				else
				{
					criticalSectionReferences.Add(criticalSectionReference);
					bool flag = true;
				}
			}
		}

		public static void ExitCriticalSection(object criticalSectionReference = null)
		{
			lock (criticalSectionLock)
			{
				if (criticalSectionReference == null)
				{
					int num = criticalSectionReferenceCount;
					criticalSectionReferenceCount--;
					if (criticalSectionReferenceCount < 0)
					{
						criticalSectionReferenceCount = 0;
					}
					bool flag = num > 0 && criticalSectionReferenceCount == 0;
				}
				else if (criticalSectionReferences.Contains(criticalSectionReference))
				{
					bool flag = true;
					criticalSectionReferences.Remove(criticalSectionReference);
				}
				else
				{
					bool flag = false;
				}
			}
		}

		private void UpdateCriticalSection()
		{
			lock (criticalSectionLock)
			{
				bool flag = criticalSectionReferenceCount > 0 || criticalSectionReferences.Count > 0;
			}
		}

		public static void BeginInitialAchievementsLoad()
		{
			lock (pendingAchievementsLoadsLock)
			{
				if (Platform.OnRequestAchievementsLoad != null)
				{
					Platform.OnRequestAchievementsLoad(pendingAchievementsLoads);
				}
				Debug.Log("Platform.BeginInitialAchievementsLoad(): Received " + pendingAchievementsLoads.Count + " requests for Achievements loads!");
			}
			performingInitialAchievementsLoad = true;
		}

		private void UpdateInitialAchievementsLoads()
		{
			if (!performingInitialAchievementsLoad)
			{
				return;
			}
			lock (pendingAchievementsLoadsLock)
			{
				if (pendingAchievementsLoads.Count == 0)
				{
					performingInitialAchievementsLoad = false;
					if (Platform.OnAchievementsLoadCompleted != null)
					{
						Platform.OnAchievementsLoadCompleted();
					}
				}
			}
		}

		public static void BeginInitialUserDataRead()
		{
			lock (pendingUserDataReadsLock)
			{
				if (Platform.OnRequestUserDataRead != null)
				{
					Platform.OnRequestUserDataRead(pendingUserDataReads);
				}
				Debug.Log("Platform.BeginInitialUserDataRead(): Received " + pendingUserDataReads.Count + " requests for UserData reads!");
			}
			performingInitialUserDataRead = true;
		}

		private void UpdateInitialUserDataReads()
		{
			if (!performingInitialUserDataRead)
			{
				return;
			}
			lock (pendingUserDataReadsLock)
			{
				if (pendingUserDataReads.Count == 0)
				{
					performingInitialUserDataRead = false;
					if (Platform.OnUserDataReadCompleted != null)
					{
						Platform.OnUserDataReadCompleted();
					}
				}
			}
		}

		private void Awake()
		{
			if ((object)instance == null)
			{
				instance = this;
				implementation = PlatformImpl.GetPlatformImpl();
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
				mainThread = Thread.CurrentThread;
				isEditor = Application.isEditor;
				if (!overrideSystemLanguage)
				{
					systemLanguage = implementation.GetLanguage();
				}
				else
				{
					systemLanguage = overrideLanguage;
				}
				implementation.Awake();
			}
			else
			{
				Debug.LogError("Multiple instances of Platform created!");
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		private void OnEnable()
		{
			implementation.OnEnable();
		}

		private void OnDisable()
		{
			implementation.OnDisable();
		}

		private void Update()
		{
			implementation.Update();
			if (Platform.OnUpdate != null)
			{
				Platform.OnUpdate();
			}
			UpdateInitialUserDataReads();
			UpdateInitialAchievementsLoads();
		}

		private void LateUpdate()
		{
			implementation.LateUpdate();
			if (Platform.OnLateUpdate != null)
			{
				Platform.OnLateUpdate();
			}
			UpdateCriticalSection();
		}

		private void OnApplicationPause(bool pause)
		{
			implementation.OnApplicationPause(pause);
		}

		private void OnDestroy()
		{
			if ((object)instance == this)
			{
				implementation.OnDestroy();
			}
		}

		public static void SetPlatformMetadata(object metadata)
		{
			implementation.SetPlatformMetadata(metadata);
		}

		public static void NotifyUserDataReadCompleted(object obj)
		{
			lock (pendingUserDataReadsLock)
			{
				pendingUserDataReads.Remove(obj);
			}
		}

		public static void NotifyAchievementsLoadCompleted(object obj)
		{
			lock (pendingAchievementsLoadsLock)
			{
				pendingAchievementsLoads.Remove(obj);
			}
		}
	}
}
