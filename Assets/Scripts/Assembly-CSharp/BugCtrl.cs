using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class BugCtrl : NavPanel
{
	public TMP_InputField summaryField;

	public TMP_Dropdown typeDropdown;

	public TMP_InputField detailsField;

	public TMP_InputField emailField;

	private string summary;

	private string email;

	private string details;

	private string type;

	[SerializeField]
	private string BASE_URL = "https://docs.google.com/forms/d/e/1FAIpQLSfoAyUkVTxyrU2TfpnzaUNZm6O-yQwG0_-fOilKE3hnVGS0hw/formResponse";

	public BC ctrl;

	public OptionCtrl optCtrl;

	public void Send()
	{
		if (summaryField.text.Length < 1)
		{
			summaryField.placeholder.GetComponent<TMP_Text>().text = "Please enter Summary here*";
			return;
		}
		summary = summaryField.text;
		type = typeDropdown.options[typeDropdown.value].text;
		details = detailsField.text;
		email = emailField.text;
		StartCoroutine(Post(summary, type, details, email));
	}

	private IEnumerator Post(string summary, string type, string details, string email)
	{
		WWWForm form = new WWWForm();
		form.AddField("entry.1283702508", summary);
		form.AddField("entry.1903941663", type);
		form.AddField("entry.1203610476", details);
		form.AddField("entry.1296624029", email);
		form.AddField("entry.572484581", S.I.vrCtrl.currentVersion);
		string filePath = Path.Combine(Environment.GetEnvironmentVariable("AppData"), "..", "LocalLow", Application.companyName, Application.productName, "output_log.txt");
		string outputLog = "";
		try
		{
			outputLog = File.ReadAllText(filePath);
			outputLog = outputLog.Substring(outputLog.Length - 5000);
		}
		catch (Exception)
		{
		}
		string spellList = "";
		string artList = "";
		if ((bool)ctrl.currentPlayer)
		{
			foreach (ListCard spellListCard in ctrl.currentPlayer.duelDisk.deck)
			{
				spellList = spellList + ", " + spellListCard.spellObj.itemID;
			}
			foreach (ArtifactObject spellObj in ctrl.currentPlayer.artObjs)
			{
				artList = artList + ", " + spellObj.itemID;
			}
		}
		form.AddField("entry.2073363910", outputLog);
		form.AddField("entry.1528211346", spellList);
		form.AddField("entry.1700066836", artList);
		string sysInfo = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}", SystemInfo.operatingSystem, SystemInfo.deviceType, SystemInfo.processorType, SystemInfo.processorFrequency, SystemInfo.systemMemorySize, SystemInfo.graphicsDeviceVendor, SystemInfo.graphicsDeviceVersion, SystemInfo.graphicsMemorySize, SystemInfo.deviceUniqueIdentifier);
		form.AddField("entry.1112774434", sysInfo);
		using (UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form))
		{
			yield return www.SendWebRequest();
			if (www.isNetworkError || www.isHttpError)
			{
				summaryField.text = www.error;
				detailsField.text = "Sorry, report could not be sent, please email OneStepFromEden@gmail.com instead";
			}
			else
			{
				summaryField.text = "";
				detailsField.text = "Report sent! Thank you!";
			}
		}
	}

	public override void Close()
	{
		optCtrl.ClosePanel(this);
	}

	public override void CloseBase()
	{
		base.Close();
	}
}
