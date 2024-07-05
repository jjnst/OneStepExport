using System.Collections;
using System.Text.RegularExpressions;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DemoEndCtrl : MonoBehaviour
{
	public TMP_InputField emailField;

	public TMP_Text errorMessage;

	private string email;

	public Animator thanksPane;

	public TMP_Text battleTime;

	public Image thanksImage;

	public TMP_Text topText;

	public TMP_Text totalPlaytimeText;

	public TMP_Text skText;

	public TMP_Text reText;

	public TMP_Text seText;

	public TMP_Text ngText;

	public TMP_Text saffThankText;

	public TMP_Text seedText;

	public GameObject itchIoButton;

	public GameObject gameJoltButton;

	public GameObject steamButton;

	public UIButton restartButton;

	public StopWatch totalPlaytimeWatch;

	private BC ctrl;

	private ButtonCtrl btnCtrl;

	public const string MatchEmailPattern = "^(([\\w-]+\\.)+[\\w-]+|([a-zA-Z]{1}|[\\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\\w-]+\\.)+[a-zA-Z]{2,4})$";

	public const string MatchTenSeedPattern = "^[0-9]*$";

	public void Start()
	{
		seedText.text = S.I.runCtrl.worldBar.seedText.text;
		ctrl = S.I.batCtrl;
		btnCtrl = S.I.btnCtrl;
		thanksPane.SetBool("OnScreen", SaveDataCtrl.Get("SaidThanks", false));
		totalPlaytimeWatch = GetComponent<StopWatch>();
		totalPlaytimeWatch.minutes = SaveDataCtrl.Get("totalMinutes", 0);
		gameJoltButton.SetActive(false);
		itchIoButton.SetActive(false);
		errorMessage.text = "";
		topText.text = ScriptLocalization.UI.ty_for_playing;
	}

	public void ClickSubmit()
	{
		if (Regex.IsMatch(emailField.text, "^[0-9]*$") && emailField.text.Length > 0)
		{
			errorMessage.text = "Seeding next run.";
			return;
		}
		if (emailField.text.Length < 1)
		{
			emailField.placeholder.GetComponent<TMP_Text>().text = "Please enter Email here*";
			return;
		}
		email = emailField.text;
		if (Regex.IsMatch(email, "^(([\\w-]+\\.)+[\\w-]+|([a-zA-Z]{1}|[\\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\\w-]+\\.)+[a-zA-Z]{2,4})$"))
		{
			StartCoroutine(PostToMailChimp(email));
		}
		else
		{
			errorMessage.text = "Sorry, email is invalid";
		}
	}

	private IEnumerator PostToMailChimp(string email)
	{
		WWWForm form = new WWWForm();
		form.AddField("EMAIL", email);
		form.AddField("b_f8c8adb53c026f72ec529c596_e94a752f28", "");
		using (UnityWebRequest www = UnityWebRequest.Post("https://onestepfromeden.us16.list-manage.com/subscribe/post?u=f8c8adb53c026f72ec529c596&amp;id=e94a752f28", form))
		{
			yield return www.SendWebRequest();
			if (www.isNetworkError || www.isHttpError)
			{
				errorMessage.text = www.error;
				yield break;
			}
			errorMessage.text = "Success! Thank you!";
			ClearFields();
		}
	}

	public void ShowThanks()
	{
		thanksPane.SetBool("OnScreen", true);
		SaveDataCtrl.Set("SaidThanks", true);
	}

	public void UpdateStats()
	{
		btnCtrl.SetFocus(restartButton);
		totalPlaytimeWatch.Pause();
		totalPlaytimeText.text = string.Format("{0}: {1}h {2}m", ScriptLocalization.UI.total_playtime, totalPlaytimeWatch.Hours(), totalPlaytimeWatch.Minutes());
		SaveDataCtrl.Set("totalMinutes", totalPlaytimeWatch.minutes);
		battleTime.text = ScriptLocalization.UI.Run_time + ": " + ctrl.runStopWatch.FormattedTime();
		ctrl.runStopWatch.Pause();
		if (SaveDataCtrl.Get("BonusSK", false))
		{
			skText.color = Color.white;
		}
		if (SaveDataCtrl.Get("BonusRe", false))
		{
			reText.color = Color.white;
		}
		if (SaveDataCtrl.Get("BonusNG", false))
		{
			ngText.color = Color.white;
		}
		ClearFields();
	}

	private void ClearFields()
	{
		emailField.text = "";
	}
}
