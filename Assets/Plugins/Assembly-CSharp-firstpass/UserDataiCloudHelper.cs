using System;
using System.Collections.Generic;
using Kittehface.Framework20;
using UnityEngine;

public class UserDataiCloudHelper : MonoBehaviour
{
	public const string ICLOUD_INFO_UUID_KEY = "iCloud_UUID";

	public const string ICLOUD_INFO_DATETIME_KEY = "iCloud_DateTime";

	public const string ICLOUD_INFO_DEVICE_KEY = "iCloud_Device";

	public const string ICLOUD_INFO_DEVICE_FALLBACK_KEY = "iCloud_Device_Fallback";

	private List<UserData.File> files = new List<UserData.File>();

	private static string _deviceUUID = null;

	private static UserDataiCloudHelper instance = null;

	public static string deviceUUID
	{
		get
		{
			return _deviceUUID;
		}
	}

	public static UserDataiCloudHelper Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject("UserDataiCloudHelper");
				instance = gameObject.AddComponent<UserDataiCloudHelper>();
			}
			return instance;
		}
	}

	public static event UserData.RequestReconciliation OnRequestReconciliation;

	public static void StartMonitoring(UserData.File file)
	{
		if (file != null)
		{
		}
	}

	public static void StopMonitoring(UserData.File file)
	{
		if (file != null)
		{
		}
	}

	public static void WriteWithiCloudInfo(UserData.File file)
	{
		file.Write();
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		string text = deviceUUID;
		Debug.Log("UserDataiCloudHelper.Awake: UUID [" + text + "]", this);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void UserDataFile_OnRequestReconciliation(UserData.ReconciliationEventArgs eventArgs)
	{
		if (!files.Contains(eventArgs.file))
		{
			return;
		}
		if (eventArgs.localFile != null)
		{
			if (eventArgs.localFile.Contains("iCloud_UUID") && eventArgs.remoteFile.Contains("iCloud_UUID"))
			{
				string text = eventArgs.localFile.Get<string>("iCloud_UUID");
				string text2 = eventArgs.remoteFile.Get<string>("iCloud_UUID");
				if (!string.IsNullOrEmpty(text) && text == text2)
				{
					if (eventArgs.operationType == UserData.ReconciliationOperationType.Load)
					{
						if (deviceUUID == text)
						{
							Debug.Log("UserDataiCloudHelper.UserDataFile_OnRequestReconciliation: [" + eventArgs.operationType.ToString() + "], [" + eventArgs.onlineState.ToString() + "], UUIDs match in save files and match local device (" + deviceUUID + "), reconcile local", this);
							eventArgs.file.Reconcile(UserData.ReconciliationResolutionType.Local);
						}
						else
						{
							Debug.Log("UserDataiCloudHelper.UserDataFile_OnRequestReconciliation: [" + eventArgs.operationType.ToString() + "], [" + eventArgs.onlineState.ToString() + "], UUIDs match in save files but do not match local device (file: " + text + ", device: " + deviceUUID + "), reconcile remote", this);
							eventArgs.file.Reconcile(UserData.ReconciliationResolutionType.Remote);
						}
						return;
					}
					if (deviceUUID == text)
					{
						Debug.Log("UserDataiCloudHelper.UserDataFile_OnRequestReconciliation: [" + eventArgs.operationType.ToString() + "], [" + eventArgs.onlineState.ToString() + "], UUIDs match (file: " + text + ", device: " + deviceUUID + "), reconcile local", this);
						eventArgs.file.Reconcile(UserData.ReconciliationResolutionType.Local);
						return;
					}
					if (eventArgs.localFile.Contains("iCloud_DateTime") && eventArgs.remoteFile.Contains("iCloud_DateTime") && eventArgs.localFile.Get<DateTime>("iCloud_DateTime") >= eventArgs.remoteFile.Get<DateTime>("iCloud_DateTime"))
					{
						Debug.Log("UserDataiCloudHelper.UserDataFile_OnRequestReconciliation: [" + eventArgs.operationType.ToString() + "], [" + eventArgs.onlineState.ToString() + "], UUIDs match (file: " + text + ", device: " + deviceUUID + "), local data is newer, reconcile local", this);
						eventArgs.file.Reconcile(UserData.ReconciliationResolutionType.Local);
						return;
					}
				}
			}
		}
		else if (eventArgs.remoteFile.Contains("iCloud_UUID"))
		{
			string text3 = deviceUUID;
			string text4 = eventArgs.remoteFile.Get<string>("iCloud_UUID");
			if (!string.IsNullOrEmpty(text3) && text3 == text4)
			{
				if (eventArgs.onlineState == UserData.ReconciliationOnlineState.Online)
				{
					Debug.Log("UserDataiCloudHelper.UserDataFile_OnRequestReconciliation: [" + eventArgs.operationType.ToString() + "], [" + eventArgs.onlineState.ToString() + "], UUIDs match (no local file defined) (" + deviceUUID + "), reconcile local", this);
					eventArgs.file.Reconcile(UserData.ReconciliationResolutionType.Local);
					return;
				}
				if (eventArgs.remoteFile.Contains("iCloud_DateTime") && DateTime.UtcNow >= eventArgs.remoteFile.Get<DateTime>("iCloud_DateTime"))
				{
					Debug.Log("UserDataiCloudHelper.UserDataFile_OnRequestReconciliation: [" + eventArgs.operationType.ToString() + "], [" + eventArgs.onlineState.ToString() + "], UUIDs match (no local file defined) (" + deviceUUID + "), local data is newer, reconcile local", this);
					eventArgs.file.Reconcile(UserData.ReconciliationResolutionType.Local);
					return;
				}
			}
		}
		if (UserDataiCloudHelper.OnRequestReconciliation != null)
		{
			UserDataiCloudHelper.OnRequestReconciliation(eventArgs);
		}
		else if (eventArgs.operationType == UserData.ReconciliationOperationType.Load && eventArgs.onlineState == UserData.ReconciliationOnlineState.Offline)
		{
			Debug.Log("UserDataiCloudHelper.UserDataFile_OnRequestReconciliation: [" + eventArgs.operationType.ToString() + "], [" + eventArgs.onlineState.ToString() + "], auto-reconcile remote", this);
			eventArgs.file.Reconcile(UserData.ReconciliationResolutionType.Remote);
		}
		else
		{
			Debug.Log("UserDataiCloudHelper.UserDataFile_OnRequestReconciliation: [" + eventArgs.operationType.ToString() + "], [" + eventArgs.onlineState.ToString() + "], auto-reconcile local", this);
			eventArgs.file.Reconcile(UserData.ReconciliationResolutionType.Local);
		}
	}

	private void UserDataFile_OnUnmountCompleted(UserData.File file, UserData.Result result)
	{
		StopMonitoring(file);
	}
}
