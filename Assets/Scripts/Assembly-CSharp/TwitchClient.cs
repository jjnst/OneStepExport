using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Unity;
using UnityEngine;

public class TwitchClient : MonoBehaviour
{
	public TwitchApi twitchApi;

	public Client client;

	private string channel_name = "thomasmoonkang";

	public static bool twitchConnected = false;

	public static bool votingEnabled = false;

	public static float votingDuration = 20f;

	public static bool subOnlyEnabled = false;

	public static bool chatNotifications = false;

	public static bool bossMercyVotingEnabled = false;

	public bool votingOpen = false;

	public List<VoteDisplay> voteDisplays = new List<VoteDisplay>();

	public List<VoteDisplay> orderedVoteDisplays = new List<VoteDisplay>();

	public Dictionary<string, int> voteDictionary = new Dictionary<string, int>();

	private void Start()
	{
		Application.runInBackground = true;
		if (twitchApi == null)
		{
			twitchApi = GetComponent<TwitchApi>();
		}
	}

	public void Initialize(string bot_name, string oAuthToken = "")
	{
		bot_name = ((!string.IsNullOrEmpty(bot_name) && !string.IsNullOrEmpty(oAuthToken)) ? bot_name.ToLowerInvariant() : Secrets.bot_name);
		if (string.IsNullOrEmpty(oAuthToken))
		{
			oAuthToken = Secrets.bot_access_token;
		}
		twitchApi.InitializeApi(oAuthToken);
		ConnectionCredentials credentials = new ConnectionCredentials(bot_name, oAuthToken);
		client = new Client();
		client.Initialize(credentials);
		client.OnMessageReceived += OnMessageReceived;
		client.OnConnected += OnConnected;
		client.OnJoinedChannel += OnJoinedChannel;
		client.OnLeftChannel += OnLeftChannel;
		client.Connect();
	}

	public bool VotingOnline()
	{
		return client != null && votingEnabled && twitchConnected;
	}

	public bool BossMercyVotingOnline()
	{
		return client != null && bossMercyVotingEnabled && twitchConnected;
	}

	public void JoinChannel(string newChannelName)
	{
		if (!string.IsNullOrEmpty(newChannelName))
		{
			channel_name = newChannelName.ToLowerInvariant();
			client.JoinChannel(channel_name);
		}
	}

	private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
	{
		twitchConnected = true;
	}

	private void OnLeftChannel(object sender, OnLeftChannelArgs e)
	{
		twitchConnected = false;
	}

	private void OnConnected(object sender, OnConnectedArgs e)
	{
	}

	private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
	{
		string text = e.ChatMessage.Message;
		string displayName = e.ChatMessage.DisplayName;
		if (!votingOpen || voteDisplays.Count <= 0 || (subOnlyEnabled && !e.ChatMessage.IsSubscriber))
		{
			return;
		}
		int result = -999;
		int.TryParse(text, out result);
		if (!text.Contains("#") && (result < 0 || result >= 99))
		{
			return;
		}
		if (text.Contains("#"))
		{
			text = text.Replace("#", string.Empty);
		}
		int result2 = -1;
		if (!int.TryParse(text, out result2))
		{
			result2 = -1;
		}
		if (result2 < 0 || result2 >= 99)
		{
			return;
		}
		if (voteDictionary.ContainsKey(displayName))
		{
			voteDisplays[voteDictionary[displayName]].Decrement();
		}
		voteDictionary[displayName] = result2;
		if (voteDisplays.Count <= result2)
		{
			return;
		}
		voteDisplays[result2].Increment();
		orderedVoteDisplays = voteDisplays.OrderByDescending((VoteDisplay t) => t.votes).ToList();
		int votes = orderedVoteDisplays[0].votes;
		foreach (VoteDisplay orderedVoteDisplay in orderedVoteDisplays)
		{
			orderedVoteDisplay.Highlight(votes);
		}
	}

	public void StartVoting()
	{
		voteDictionary.Clear();
		votingOpen = true;
		if (chatNotifications && client != null && client.JoinedChannels.Count > 0)
		{
			client.SendMessage(client.JoinedChannels[0], string.Format("{0} #{1}-{2}", ScriptLocalization.UI.Vote_Now, 0, voteDisplays.Count - 1));
		}
	}

	public void EndVoting()
	{
		if (!chatNotifications || votingOpen)
		{
		}
		votingOpen = false;
		voteDictionary.Clear();
	}
}
