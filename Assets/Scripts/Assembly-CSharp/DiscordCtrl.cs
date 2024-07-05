using System;
using Discord;
using UnityEngine;

public class DiscordCtrl : MonoBehaviour
{
	public global::Discord.Discord discord;

	public ActivityManager activityManager;

	public Activity activity;

	private void Start()
	{
		try
		{
			discord = new global::Discord.Discord(493148662059499520L, 1uL);
		}
		catch (Exception)
		{
			Debug.Log("No Discord Available");
			discord = null;
		}
		if (discord == null)
		{
			return;
		}
		activityManager = discord.GetActivityManager();
		activity = new Activity
		{
			State = "",
			Details = "In Main Menu",
			Timestamps = 
			{
				Start = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()
			},
			Assets = 
			{
				LargeImage = "icon_1024x1024"
			},
			Instance = true
		};
		activityManager.UpdateActivity(activity, delegate(Result res)
		{
			if (res != 0)
			{
			}
		});
	}

	private void Update()
	{
		if (discord != null)
		{
			discord.RunCallbacks();
		}
	}

	public void RefreshActivityData(string stateText, string detailText, string characterName)
	{
		if (discord == null)
		{
			return;
		}
		string largeText = "";
		if (!string.IsNullOrEmpty(characterName))
		{
			largeText = "Playing as " + characterName;
		}
		activity = new Activity
		{
			State = stateText,
			Details = detailText,
			Assets = 
			{
				LargeImage = "icon_1024x1024",
				LargeText = largeText
			},
			Instance = true
		};
		activityManager.UpdateActivity(activity, delegate(Result result)
		{
			if (result != 0)
			{
			}
		});
	}
}
