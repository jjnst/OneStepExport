using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionCtrl : MonoBehaviour
{
	public string currentVersion;

	public TMP_Text currentVersionText;

	public TMP_Text currentVersionQAText;

	public TMP_Text updateAvailableText;

	public GameObject updateButton;

	public BuildVersionSetter buildVer;

	private string latestVersionString = "";

	private void Awake()
	{
		string text = "";
		if (S.I.scene == GScene.DemoLive)
		{
			text += "Live ";
		}
		else if (S.I.EDITION == Edition.Beta)
		{
			text = text + "Beta v" + currentVersion;
		}
		else if (S.I.EDITION == Edition.Full)
		{
			text += currentVersion;
		}
		else if (S.I.EDITION == Edition.QA)
		{
			text = text + "QA v" + currentVersion;
		}
		else if (S.I.EDITION == Edition.Dev)
		{
			text = text + "Dev v" + currentVersion;
		}
		text += buildVer.buildDate;
		if (S.I.RECORD_MODE || S.I.EDITION == Edition.Dev)
		{
			text = "";
		}
		currentVersionText.text = text;
		currentVersionQAText.text = text;
		if (S.I.EDITION != Edition.QA)
		{
			currentVersionQAText.gameObject.SetActive(false);
		}
		updateButton.SetActive(false);
		if (S.I.scene != GScene.DemoLive)
		{
		}
	}

	public void ClickUpdate()
	{
		Report("update_clicked", latestVersionString);
	}

	public void Report(string eventName, string updatingToVersion)
	{
		Ana.CustomEvent(eventName, new Dictionary<string, object>
		{
			{ "current_version", currentVersion },
			{ "updating_to", updatingToVersion },
			{
				"edition",
				S.I.EDITION.ToString()
			},
			{
				"lifetime_battles",
				SaveDataCtrl.Get("LifetimeBattles", 0)
			},
			{
				"time_time_elapsed",
				Time.time
			},
			{
				"device_unique_id",
				SystemInfo.deviceUniqueIdentifier
			}
		});
	}
}
