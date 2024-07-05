using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "KF Framework/Achievement Data", order = 1)]
public class AchievementData : ScriptableObject
{
	[SerializeField]
	private string achievementName;

	[SerializeField]
	private bool hidden;

	[SerializeField]
	private string localizationNameKey;

	[SerializeField]
	private string localizationDescriptionKey;

	[SerializeField]
	private string localizationAchievedDescriptionKey;

	[SerializeField]
	private Texture2D achievementImage;

	[SerializeField]
	private string steamID;

	[SerializeField]
	private string xboxOneID;

	[SerializeField]
	private int ps4ID;

	[SerializeField]
	private string uwpID;

	[SerializeField]
	private string gameCenterID;

	[SerializeField]
	private string androidID;

	[SerializeField]
	private string gogID;

	public string AchievementName
	{
		get
		{
			return achievementName;
		}
	}

	public bool Hidden
	{
		get
		{
			return hidden;
		}
	}

	public string LocalizationNameKey
	{
		get
		{
			return localizationNameKey;
		}
	}

	public string LocalizationDescriptionKey
	{
		get
		{
			return localizationDescriptionKey;
		}
	}

	public string LocalizationAchievedDescriptionKey
	{
		get
		{
			return localizationAchievedDescriptionKey;
		}
	}

	public Texture2D AchievementImage
	{
		get
		{
			return achievementImage;
		}
	}

	public string SteamID
	{
		get
		{
			return steamID;
		}
	}

	public string XboxOneID
	{
		get
		{
			return xboxOneID;
		}
	}

	public int PS4ID
	{
		get
		{
			return ps4ID;
		}
	}

	public string UWPID
	{
		get
		{
			return uwpID;
		}
	}

	public string GameCenterID
	{
		get
		{
			return gameCenterID;
		}
	}

	public string AndroidID
	{
		get
		{
			return androidID;
		}
	}

	public string GOGID
	{
		get
		{
			return gogID;
		}
	}
}
