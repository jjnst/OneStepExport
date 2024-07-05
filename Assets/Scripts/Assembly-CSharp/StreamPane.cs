using System.Collections;
using I2.Loc;
using TMPro;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using UnityEngine;
using UnityEngine.UI;

public class StreamPane : NavPanel
{
	public NavTextfield channelNamefield;

	public NavTextfield oAuthTokenfield;

	public Toggle subscribersOnly;

	public Toggle autoconnect;

	public Toggle enableRewardVoting;

	public SliderButton voteDurationSlider;

	public Toggle bossMercyVoting;

	public Toggle chatNotifications;

	public NavButton getTokenButton;

	public NavButton disconnectButton;

	public TMP_Text statusText;

	public Transform rightGrid;

	private string summary;

	private string email;

	private string details;

	private string type;

	public BC ctrl;

	public OptionCtrl optCtrl;

	public TwitchClient twClient;

	private void Start()
	{
		StartCoroutine(c_Connect());
		disconnectButton.down = rightGrid.GetChild(0).GetComponent<UIButton>();
		rightGrid.GetChild(0).GetComponent<UIButton>().up = disconnectButton;
		disconnectButton.up = rightGrid.GetChild(rightGrid.childCount - 1).GetComponent<UIButton>();
		rightGrid.GetChild(rightGrid.childCount - 1).GetComponent<UIButton>().down = disconnectButton;
	}

	private IEnumerator c_Connect()
	{
		statusText.text = string.Format("{0} {1}", ScriptLocalization.UI.Twitch_Status, ScriptLocalization.UI.Twitch_NotConnected);
		if (!SaveDataCtrl.Initialized)
		{
			yield return new WaitUntil(() => SaveDataCtrl.Initialized);
		}
		yield return null;
		statusText.text = string.Format("{0} {1}", ScriptLocalization.UI.Twitch_Status, ScriptLocalization.UI.Twitch_NotConnected);
		disconnectButton.tmpText.text = ScriptLocalization.UI.Twitch_Connect;
		channelNamefield.label.text = ScriptLocalization.UI.Twitch_ChannelName;
		oAuthTokenfield.label.text = ScriptLocalization.UI.Twitch_OAuthToken;
		channelNamefield.inputField.text = SaveDataCtrl.Get("Twitch_LastConnectedChannel", "");
		if (channelNamefield.inputField.text == Secrets.bot_access_token)
		{
			channelNamefield.inputField.text = string.Empty;
		}
		oAuthTokenfield.inputField.text = SaveDataCtrl.Get("Twitch_LastOAuthToken", "");
		enableRewardVoting.isOn = SaveDataCtrl.Get("Twitch_LastEnableVoting", true);
		voteDurationSlider.slider.value = SaveDataCtrl.Get("Twitch_LastVotingDuration", 30f);
		bossMercyVoting.isOn = SaveDataCtrl.Get("Twitch_LastBossMercyVoting", true);
		subscribersOnly.isOn = SaveDataCtrl.Get("Twitch_LastSubOnlyMode", false);
		chatNotifications.isOn = SaveDataCtrl.Get("Twitch_LastChatNotifications", false);
		if (SaveDataCtrl.Get("Twitch_AutoConnect", false))
		{
			StartCoroutine(Connect());
			autoconnect.isOn = SaveDataCtrl.Get("Twitch_AutoConnect", false);
		}
	}

	private void OnConnected(object sender, OnConnectedArgs e)
	{
		statusText.text = string.Format("{0} {1}", ScriptLocalization.UI.Twitch_Status, ScriptLocalization.UI.Twitch_ConnectedToTwitch);
	}

	private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
	{
		statusText.text = string.Format("{0} {1} ({2})", ScriptLocalization.UI.Twitch_Status, ScriptLocalization.UI.Twitch_ConnectedToTwitch, e.Channel);
	}

	private void OnLeftChannel(object sender, OnLeftChannelArgs e)
	{
		statusText.text = string.Format("{0} {1}", ScriptLocalization.UI.Twitch_Status, ScriptLocalization.UI.Twitch_NotConnected);
	}

	public void ConnectToggle()
	{
		if (twClient.client != null && twClient.client.JoinedChannels.Count > 0)
		{
			Debug.Log("Joined channels = " + twClient.client.JoinedChannels.Count);
			foreach (JoinedChannel joinedChannel in twClient.client.JoinedChannels)
			{
				twClient.client.LeaveChannel(joinedChannel.Channel);
			}
			disconnectButton.image.color = U.I.GetColor(UIColor.Pink);
			disconnectButton.tmpText.text = ScriptLocalization.UI.Twitch_Connect;
			channelNamefield.inputField.interactable = true;
			channelNamefield.label.text = ScriptLocalization.UI.Twitch_ChannelName;
			oAuthTokenfield.inputField.interactable = true;
			oAuthTokenfield.label.text = ScriptLocalization.UI.Twitch_OAuthToken;
		}
		else
		{
			StartCoroutine(Connect());
		}
	}

	private IEnumerator Connect()
	{
		if (twClient.client == null)
		{
			if (string.IsNullOrEmpty(channelNamefield.inputField.text))
			{
				SaveDataCtrl.Set("Twitch_AutoConnect", false);
				statusText.text = string.Format("{0} {1} ({2})", ScriptLocalization.UI.Twitch_Status, ScriptLocalization.UI.Twitch_NotConnected, ScriptLocalization.UI.Twitch_PleaseEnterChannelName);
				yield break;
			}
			twClient.Initialize(channelNamefield.inputField.text);
			twClient.client.OnConnected += OnConnected;
			twClient.client.OnJoinedChannel += OnJoinedChannel;
			twClient.client.OnLeftChannel += OnLeftChannel;
		}
		else if (twClient.client.ConnectionCredentials.TwitchOAuth != oAuthTokenfield.inputField.text)
		{
			twClient.Initialize(channelNamefield.inputField.text);
			twClient.client.OnConnected += OnConnected;
			twClient.client.OnJoinedChannel += OnJoinedChannel;
			twClient.client.OnLeftChannel += OnLeftChannel;
		}
		channelNamefield.inputField.interactable = false;
		channelNamefield.label.text = ScriptLocalization.UI.Twitch_ChannelName + " " + ScriptLocalization.UI.Twitch_DisconnectToChange;
		oAuthTokenfield.inputField.interactable = false;
		oAuthTokenfield.label.text = ScriptLocalization.UI.Twitch_OAuthToken + " " + ScriptLocalization.UI.Twitch_DisconnectToChange;
		disconnectButton.canvasGroup.interactable = false;
		disconnectButton.canvasGroup.alpha = 0.5f;
		while (twClient.client == null || !twClient.client.IsConnected)
		{
			disconnectButton.tmpText.text = ScriptLocalization.UI.Twitch_Connecting;
			statusText.text = string.Format("{0} {1} {2}", ScriptLocalization.UI.Twitch_Status, ScriptLocalization.UI.Twitch_ConnectingTo, channelNamefield.inputField.text);
			yield return null;
		}
		SaveDataCtrl.Set("Twitch_LastConnectedChannel", channelNamefield.inputField.text);
		SaveDataCtrl.Set("Twitch_LastOAuthToken", oAuthTokenfield.inputField.text);
		twClient.JoinChannel(channelNamefield.inputField.text);
		UpdateRewardVotingEnabled();
		UpdateVotingDuration();
		UpdateBossMercyVotingEnabled();
		UpdateSubOnlyModeEnabled();
		UpdateChatNotificationsEnabled();
		disconnectButton.tmpText.text = ScriptLocalization.UI.Twitch_Disconnect;
		disconnectButton.canvasGroup.interactable = true;
		disconnectButton.canvasGroup.alpha = 1f;
		disconnectButton.image.color = U.I.GetColor(UIColor.Red);
	}

	public void UpdateRewardVotingEnabled()
	{
		TwitchClient.votingEnabled = enableRewardVoting.isOn;
		SaveDataCtrl.Set("Twitch_LastEnableVoting", enableRewardVoting.isOn);
	}

	public void UpdateVotingDuration()
	{
		TwitchClient.votingDuration = voteDurationSlider.slider.value;
		voteDurationSlider.tmpText.text = string.Format("{0} {1}{2}", ScriptLocalization.UI.Twitch_VoteDuration, voteDurationSlider.slider.value, ScriptLocalization.UI.seconds_shorthand);
		SaveDataCtrl.Set("Twitch_LastVotingDuration", voteDurationSlider.slider.value);
	}

	public void UpdateSubOnlyModeEnabled()
	{
		TwitchClient.subOnlyEnabled = subscribersOnly.isOn;
		SaveDataCtrl.Set("Twitch_LastSubOnlyMode", subscribersOnly.isOn);
	}

	public void UpdateAutoconnectEnabled()
	{
		SaveDataCtrl.Set("Twitch_AutoConnect", autoconnect.isOn);
	}

	public void UpdateChatNotificationsEnabled()
	{
		TwitchClient.chatNotifications = chatNotifications.isOn;
		SaveDataCtrl.Set("Twitch_LastChatNotifications", chatNotifications.isOn);
	}

	public void UpdateBossMercyVotingEnabled()
	{
		TwitchClient.bossMercyVotingEnabled = bossMercyVoting.isOn;
		SaveDataCtrl.Set("Twitch_LastBossMercyVoting", bossMercyVoting.isOn);
	}

	public void OpenOAuthLink()
	{
		Application.OpenURL("https://twitchapps.com/tmi/");
	}

	public override void Close()
	{
		SaveDataCtrl.Write();
		optCtrl.ClosePanel(this);
	}

	public override void CloseBase()
	{
		base.Close();
	}
}
