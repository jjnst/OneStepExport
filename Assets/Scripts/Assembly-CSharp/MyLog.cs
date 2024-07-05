using System.Collections;
using TMPro;
using UnityEngine;

public class MyLog : MonoBehaviour
{
	public string myLog;

	private Queue myLogQueue = new Queue();

	public TMP_Text ui_log;

	public bool getLog = false;

	public bool showGUI = false;

	private void Start()
	{
	}

	private void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	public void Toggle()
	{
		getLog = !getLog;
		if (getLog)
		{
			if (!ui_log.gameObject.activeSelf)
			{
				ui_log.gameObject.SetActive(true);
			}
		}
		else
		{
			ui_log.gameObject.SetActive(false);
		}
	}

	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		if (!getLog)
		{
			return;
		}
		myLog = logString;
		string text = string.Concat("\n [", type, "] : ", myLog);
		myLogQueue.Enqueue(text);
		if (type == LogType.Exception)
		{
			text = "\n" + stackTrace;
			myLogQueue.Enqueue(text);
		}
		myLog = string.Empty;
		foreach (string item in myLogQueue)
		{
			myLog += item;
		}
		ui_log.text += text;
		if (ui_log.text.Length > 10000)
		{
			ui_log.text.Remove(0, 3000);
		}
	}

	private void OnGUI()
	{
		if (showGUI)
		{
			GUILayout.Label(myLog);
		}
	}
}
