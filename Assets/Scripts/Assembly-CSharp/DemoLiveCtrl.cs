using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class DemoLiveCtrl : NavPanel
{
	public TMP_InputField emailField;

	public TMP_Text errorMessage;

	private string email;

	public Animator thanksPane;

	public TMP_Text battleTime;

	public TMP_Text topText;

	public UIButton reportBugButton;

	public UIButton quitButton;

	public UIButton restartButton;

	public UIButton endDemoButton;

	public StopWatch totalPlaytimeWatch;

	public string mailChimpTabID = "b_f8c8adb53c026f72ec529c596_e94a752f28";

	public string mailChimpPostURL;

	private BC ctrl;

	public const string MatchEmailPattern = "^(([\\w-]+\\.)+[\\w-]+|([a-zA-Z]{1}|[\\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\\w-]+\\.)+[a-zA-Z]{2,4})$";

	public void Start()
	{
		if (S.I.EDITION == Edition.DemoLive)
		{
			ctrl = S.I.batCtrl;
			btnCtrl = S.I.btnCtrl;
			errorMessage.text = "";
			topText.text = "";
			topText.text = ScriptLocalization.UI.ty_for_playing;
			reportBugButton.gameObject.SetActive(false);
			quitButton.gameObject.SetActive(false);
			restartButton.gameObject.SetActive(true);
			endDemoButton.gameObject.SetActive(true);
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	public void ClickSubmit()
	{
		if (emailField.text.Length < 1)
		{
			emailField.placeholder.GetComponent<TMP_Text>().text = "Please enter Email here*";
			return;
		}
		email = emailField.text;
		if (Regex.IsMatch(email, "^(([\\w-]+\\.)+[\\w-]+|([a-zA-Z]{1}|[\\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\\w-]+\\.)+[a-zA-Z]{2,4})$"))
		{
			ShowThanks();
			StartCoroutine(PostToMailChimp(email));
		}
		else
		{
			errorMessage.text = "Sorry, email is invalid";
		}
	}

	public void WinMessage()
	{
		topText.text = "YOU WIN!";
	}

	private IEnumerator PostToMailChimp(string email)
	{
		WWWForm form = new WWWForm();
		form.AddField("EMAIL", email);
		form.AddField(mailChimpTabID, "");
		using (UnityWebRequest www = UnityWebRequest.Post(mailChimpPostURL, form))
		{
			yield return www.SendWebRequest();
			if (www.isNetworkError || www.isHttpError || true)
			{
				errorMessage.text = www.error;
				string streamingAssetsPath = Application.streamingAssetsPath;
				string filePath = Path.Combine(streamingAssetsPath, "LocalEmails.txt");
				if (!File.Exists(filePath))
				{
					File.WriteAllText(filePath, "");
				}
				File.WriteAllText(filePath, File.ReadAllText(filePath) + ", " + email);
				errorMessage.text = "Saved! Thank you!";
				ClearFields();
			}
			else
			{
				errorMessage.text = "Success! Thank you!";
				ClearFields();
			}
		}
	}

	public void ShowThanks()
	{
		thanksPane.SetBool("OnScreen", true);
		SaveDataCtrl.Set("SaidThanks", true);
	}

	public void UpdateStats()
	{
		thanksPane.SetBool("OnScreen", SaveDataCtrl.Get("SaidThanks", false));
		totalPlaytimeWatch.minutes = SaveDataCtrl.Get("totalMinutes", 0f);
		totalPlaytimeWatch.Pause();
		SaveDataCtrl.Set("totalMinutes", totalPlaytimeWatch.minutes * 1f);
		battleTime.text = ScriptLocalization.UI.Run_time + ": " + ctrl.runStopWatch.FormattedTime();
		ctrl.runStopWatch.Pause();
		ClearFields();
	}

	private void ClearFields()
	{
		emailField.text = "";
	}

	public override void Open()
	{
		base.Open();
		UpdateStats();
		S.I.muCtrl.ResumeIntroLoop();
	}

	public void RestartDemo()
	{
		if (S.I.EDITION == Edition.DemoLive)
		{
			StartCoroutine(_RestartScene());
		}
	}

	private IEnumerator _RestartScene()
	{
		S.I.CameraStill(UIColor.BlueDark);
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(1);
		float startTime = Time.unscaledTime;
		while (!asyncOperation.isDone)
		{
			Mathf.Clamp01(asyncOperation.progress / 0.9f);
			if (asyncOperation.progress >= 1f && startTime + 1f < Time.unscaledTime)
			{
				asyncOperation.allowSceneActivation = true;
			}
			yield return null;
		}
	}
}
