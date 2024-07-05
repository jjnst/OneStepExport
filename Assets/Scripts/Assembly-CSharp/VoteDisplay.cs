using TMPro;
using UnityEngine;

public class VoteDisplay : MonoBehaviour
{
	public TwitchClient twClient;

	public TMP_Text tmpText;

	public int optionNum = 0;

	public int votes = 0;

	public void Reset(int newOptionNum)
	{
		votes = 0;
		optionNum = newOptionNum;
		UpdateDisplay(false);
		if (twClient == null)
		{
			twClient = S.I.twClient;
		}
	}

	public void Increment()
	{
		votes++;
	}

	public void Decrement()
	{
		votes--;
	}

	private void UpdateDisplay(bool highlight)
	{
		string arg = "0";
		if (votes > 0)
		{
			arg = Mathf.RoundToInt((float)votes / ((float)twClient.voteDictionary.Count * 1f) * 100f).ToString();
		}
		tmpText.text = string.Format("#{0}: {1}({2}%)", optionNum, votes, arg);
		if (highlight)
		{
			tmpText.text = "<b>" + U.I.GetColorStarter(UIColor.Effect) + tmpText.text + "</color></b>";
		}
	}

	public void Highlight(int highestVoteNum)
	{
		if (highestVoteNum <= votes)
		{
			UpdateDisplay(true);
		}
		else
		{
			UpdateDisplay(false);
		}
	}
}
