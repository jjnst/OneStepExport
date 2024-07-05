using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Kittehface.Framework20;
using UnityEngine;

public class SaveDataCtrl : MonoBehaviour
{
	private static readonly byte[] SaltBytes = new byte[16]
	{
		120, 49, 54, 110, 101, 57, 103, 110, 101, 107,
		115, 102, 108, 120, 114, 111
	};

	private static readonly byte[] P1Bytes = new byte[20]
	{
		166, 88, 213, 96, 237, 20, 254, 231, 109, 0,
		182, 18, 110, 219, 11, 100, 109, 63, 161, 31
	};

	private static readonly byte[] P2Bytes = new byte[20]
	{
		243, 8, 230, 19, 191, 87, 201, 146, 11, 85,
		239, 126, 40, 176, 82, 60, 55, 15, 209, 88
	};

	private static readonly UserData.FileDefinition SaveDataFileDefinition = new UserData.FileDefinition("SaveData", true, true, false, true, new UserData.FileDefinition.EncryptionDefinition(SaltBytes, P1Bytes, P2Bytes), new UserData.FileDefinition.SwitchDefinition("SaveData", 1048576L), new UserData.FileDefinition.PS4Definition(null, null, "One Step From Eden", null, null, null, "Media/StreamingAssets/SaveIconPS4.png", 1048576L));

	private static SaveDataCtrl instance = null;

	private bool initialized = false;

	private bool initializing = false;

	private UserData.File saveDataFile = null;

	private bool saveEnabled = true;

	private bool handleExistingWrite = false;

	private bool writeRequested = false;

	public static SaveDataCtrl Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("SaveDataCtrl");
				instance = gameObject.AddComponent<SaveDataCtrl>();
			}
			return instance;
		}
	}

	public static bool Initialized
	{
		get
		{
			return Instance.initialized;
		}
	}

	public static IEnumerator Initialize()
	{
		UserData.VerbosityLevel = VerbosityLevel.Warning;
		Achievements.VerbosityLevel = VerbosityLevel.Warning;
		yield return Instance.StartCoroutine(Instance.InitializeCoroutine());
	}

	public static void Set<T>(string key, T value, bool noProfile = false)
	{
		if (!Initialized)
		{
			Debug.LogError("Can't set [" + key + "] when SaveDataCtrl isn't initialized!");
			return;
		}
		if (!noProfile && S.I.currentProfile > 0)
		{
			key += S.I.currentProfile;
		}
		List<string> list = Instance.saveDataFile.Get("SavedProfileData" + S.I.currentProfile, new List<string>());
		if (!list.Contains(key))
		{
			list.Add(key);
			Instance.saveDataFile.Set("SavedProfileData" + S.I.currentProfile, list, UserData.WriteMode.Deferred);
		}
		if (Instance.saveDataFile != null)
		{
			Instance.saveDataFile.Set(key, value, UserData.WriteMode.Deferred);
		}
		else
		{
			Debug.LogError("Can't set [" + key + "], save data file is invalid!", Instance);
		}
	}

	public static T Get<T>(string key, T defaultValue = default(T), bool noProfile = false)
	{
		if (!Initialized)
		{
			Debug.LogError("Can't get [" + key + "] when SaveDataCtrl isn't initialized!");
			return defaultValue;
		}
		if (!noProfile && S.I.currentProfile > 0)
		{
			key += S.I.currentProfile;
		}
		if (Instance.saveDataFile != null)
		{
			return Instance.saveDataFile.Get(key, defaultValue);
		}
		Debug.LogError("Can't get [" + key + "], save data file is invalid!", Instance);
		return defaultValue;
	}

	public static void Remove(string key, bool noProfile = false)
	{
		if (!Initialized)
		{
			Debug.LogError("Can't get [" + key + "] when SaveDataCtrl isn't initialized!");
		}
		if (!noProfile && S.I.currentProfile > 0)
		{
			key += S.I.currentProfile;
		}
		if (Instance.saveDataFile != null)
		{
			Instance.saveDataFile.Remove(key);
		}
		else
		{
			Debug.LogError("Can't remove [" + key + "], save data file is invalid!", Instance);
		}
	}

	public static void Write(bool forceRetrySave = false)
	{
		if (!Initialized)
		{
			Debug.LogError("Can't write when SaveDataCtrl isn't initialized!");
		}
		else if (Instance.saveEnabled || forceRetrySave)
		{
			if (Instance.handleExistingWrite)
			{
				Instance.writeRequested = true;
			}
			else if (Instance.saveDataFile != null)
			{
				Instance.saveDataFile.OnWriteCompleted += Instance.File_OnExistingWriteCompleted;
				Instance.handleExistingWrite = true;
				Instance.saveDataFile.Write();
			}
			else
			{
				Debug.LogError("Can't write, save data file is invalid!", Instance);
			}
		}
	}

	public static void SetAchievementsFile()
	{
		if (Instance.saveDataFile != null)
		{
			Achievements.SetAchievementsFile(RunCtrl.runProfile, Instance.saveDataFile);
		}
		else
		{
			Debug.LogError("Trying to set achievements file before the file is set up!");
		}
	}

	public static void RemoveAll()
	{
		if (!Initialized)
		{
			Debug.LogError("Can't remove all user data when SaveDataCtrl isn't initialized!");
		}
		else if (Instance.saveDataFile != null)
		{
			Instance.saveDataFile.RemoveAll();
		}
		else
		{
			Debug.LogError("Can't remove all user data, save data file is invalid!", Instance);
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
			Object.Destroy(base.gameObject);
			return;
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private IEnumerator InitializeCoroutine()
	{
		if (!initialized && !initializing)
		{
			initializing = true;
			UserData.RegisterDataType(typeof(List<ArtData>));
			UserData.RegisterDataType(typeof(List<Brand>));
			UserData.RegisterDataType(typeof(Dictionary<string, bool>));
			UserData.RegisterDataType(typeof(List<List<Enhancement>>));
			UserData.RegisterDataType(typeof(List<string>));
			UserData.RegisterDataType(typeof(List<int>));
			if (RunCtrl.runProfile != null)
			{
				UserData.OnFileMounted += UserData_OnFileMounted;
				UserData.Mount(RunCtrl.runProfile, null, SaveDataFileDefinition);
			}
			else
			{
				initialized = true;
				Debug.LogError("Cannot initialize SaveDataCtrl until a Profile has been created!", this);
			}
			while (!initialized)
			{
				yield return null;
			}
			initializing = false;
		}
	}

	private void UserData_OnFileMounted(UserData.File file, UserData.Result result)
	{
		UserData.OnFileMounted -= UserData_OnFileMounted;
		if (result.IsSuccess())
		{
			saveDataFile = file;
			file.OnReadCompleted += File_OnReadCompleted;
			file.OnUnmountCompleted += File_OnUnmountCompleted;
			file.Read();
		}
		else
		{
			file.Unmount();
			initialized = true;
		}
	}

	private void File_OnReadCompleted(UserData.File file, UserData.Result result)
	{
		file.OnReadCompleted -= File_OnReadCompleted;
		if (result.IsSuccess())
		{
			if (result.Contains(UserData.Result.FileNotFound))
			{
				file.OnWriteCompleted += File_OnNewWriteCompleted;
				file.Set("Test", 0);
				file.Write();
			}
			else
			{
				initialized = true;
			}
		}
		else if (result.Contains(UserData.Result.FileNotFound))
		{
			file.OnWriteCompleted += File_OnNewWriteCompleted;
			file.Set("Test", 0);
			file.Write();
		}
		else if (result.Contains(UserData.Result.CorruptData))
		{
			UserDataErrorHelper.OnCorruptDataDialogComplete += UserDataErrorHelper_OnCorruptDataDialogComplete;
			if (!UserDataErrorHelper.OpenCorruptDataDialog(file.owner, file, UserDataErrorHelper.CorruptDataMessage.Corrupt))
			{
				UserDataErrorHelper.OnCorruptDataDialogComplete -= UserDataErrorHelper_OnCorruptDataDialogComplete;
				initialized = true;
			}
		}
		else
		{
			initialized = true;
		}
	}

	private void File_OnNewWriteCompleted(UserData.File file, UserData.Result result)
	{
		file.OnWriteCompleted -= File_OnNewWriteCompleted;
		if (result.IsFailure())
		{
			saveEnabled = false;
			if (result.Contains(UserData.Result.NoFreeSpace))
			{
				UserDataErrorHelper.OnNoSaveSpaceDialogComplete += UserDataErrorHelper_OnNoSaveSpaceDialogComplete;
				if (UserDataErrorHelper.OpenNoSaveSpaceDialog(file.owner, file, UserDataErrorHelper.NoSaveSpaceMessage.CanContinue))
				{
					return;
				}
				UserDataErrorHelper.OnNoSaveSpaceDialogComplete -= UserDataErrorHelper_OnNoSaveSpaceDialogComplete;
			}
			initialized = true;
		}
		else
		{
			initialized = true;
			saveEnabled = true;
		}
	}

	private void UserDataErrorHelper_OnNoSaveSpaceDialogComplete(Profiles.Profile profile, UserData.File file, bool success)
	{
		UserDataErrorHelper.OnNoSaveSpaceDialogComplete -= UserDataErrorHelper_OnNoSaveSpaceDialogComplete;
		UserDataErrorHelper.OnUserMessageDialogComplete += UserDataErrorHelper_OnRetrySaveUserMessageDialogComplete;
		if (!UserDataErrorHelper.OpenUserMessageDialog(profile, LocalizationManager.GetTranslation("UI/save_data_retry_save_ps4"), UserDataErrorHelper.UserDataOperationType.Save, UserDataErrorHelper.UserResponseType.YesNo, file))
		{
			UserDataErrorHelper.OnUserMessageDialogComplete -= UserDataErrorHelper_OnRetrySaveUserMessageDialogComplete;
			initialized = true;
		}
	}

	private void UserDataErrorHelper_OnRetrySaveUserMessageDialogComplete(Profiles.Profile profile, UserDataErrorHelper.UserResponse userResponse, bool success)
	{
		UserDataErrorHelper.OnUserMessageDialogComplete -= UserDataErrorHelper_OnRetrySaveUserMessageDialogComplete;
		if (success && userResponse == UserDataErrorHelper.UserResponse.Yes)
		{
			saveDataFile.OnWriteCompleted += File_OnNewWriteCompleted;
			saveDataFile.Write();
		}
		else
		{
			initialized = true;
		}
	}

	private void File_OnExistingWriteCompleted(UserData.File file, UserData.Result result)
	{
		file.OnWriteCompleted -= File_OnExistingWriteCompleted;
		if (result.IsFailure())
		{
			saveEnabled = false;
			if (result.Contains(UserData.Result.NoFreeSpace))
			{
				UserDataErrorHelper.OnNoSaveSpaceDialogComplete += UserDataErrorHelper_OnExistingNoSaveSpaceDialogComplete;
				if (UserDataErrorHelper.OpenNoSaveSpaceDialog(file.owner, file, UserDataErrorHelper.NoSaveSpaceMessage.CanContinue))
				{
					return;
				}
				UserDataErrorHelper.OnNoSaveSpaceDialogComplete -= UserDataErrorHelper_OnExistingNoSaveSpaceDialogComplete;
			}
		}
		else
		{
			saveEnabled = true;
		}
		handleExistingWrite = false;
		if (writeRequested)
		{
			saveDataFile.OnWriteCompleted += File_OnExistingWriteCompleted;
			handleExistingWrite = true;
			saveDataFile.Write();
			writeRequested = false;
		}
	}

	private void UserDataErrorHelper_OnExistingNoSaveSpaceDialogComplete(Profiles.Profile profile, UserData.File file, bool success)
	{
		UserDataErrorHelper.OnNoSaveSpaceDialogComplete -= UserDataErrorHelper_OnExistingNoSaveSpaceDialogComplete;
		handleExistingWrite = false;
		if (writeRequested)
		{
			saveDataFile.OnWriteCompleted += File_OnExistingWriteCompleted;
			handleExistingWrite = true;
			saveDataFile.Write();
			writeRequested = false;
		}
	}

	private void File_OnUnmountCompleted(UserData.File file, UserData.Result result)
	{
		file.OnUnmountCompleted -= File_OnUnmountCompleted;
		if (file == saveDataFile)
		{
			saveDataFile = null;
		}
	}

	private void UserDataErrorHelper_OnCorruptDataDialogComplete(Profiles.Profile profile, UserData.File file, UserDataErrorHelper.UserResponse userResponse, bool success)
	{
		UserDataErrorHelper.OnCorruptDataDialogComplete -= UserDataErrorHelper_OnCorruptDataDialogComplete;
		UserDataErrorHelper.OnUserMessageDialogComplete += UserDataErrorHelper_OnCorruptDataDeleteUserMessageDialogComplete;
		if (!UserDataErrorHelper.OpenUserMessageDialog(profile, LocalizationManager.GetTranslation("UI/save_data_corrupt_message_ps4"), UserDataErrorHelper.UserDataOperationType.Load, UserDataErrorHelper.UserResponseType.Ok, file))
		{
			UserDataErrorHelper.OnUserMessageDialogComplete -= UserDataErrorHelper_OnCorruptDataDeleteUserMessageDialogComplete;
			initialized = true;
		}
	}

	private void UserDataErrorHelper_OnCorruptDataDeleteUserMessageDialogComplete(Profiles.Profile profile, UserDataErrorHelper.UserResponse userResponse, bool success)
	{
		UserDataErrorHelper.OnUserMessageDialogComplete -= UserDataErrorHelper_OnCorruptDataDeleteUserMessageDialogComplete;
		saveDataFile.OnDeleteCompleted += File_OnCorruptDataDeleteCompleted;
		saveDataFile.Delete();
	}

	private void File_OnCorruptDataDeleteCompleted(UserData.File file, UserData.Result result)
	{
		file.OnDeleteCompleted -= File_OnCorruptDataDeleteCompleted;
		if (result.IsSuccess())
		{
			file.OnWriteCompleted += File_OnNewWriteCompleted;
			file.Set("Test", 0);
			file.Write();
		}
		else
		{
			initialized = true;
		}
	}
}
