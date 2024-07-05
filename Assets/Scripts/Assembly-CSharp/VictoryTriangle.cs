using UnityEngine;
using UnityEngine.UI;

public class VictoryTriangle : MonoBehaviour
{
	public Image background;

	public Image genocideLine;

	public Image pacifistLine;

	public Image neutralLine;

	public void Hide()
	{
		background.color = Color.clear;
		genocideLine.color = Color.clear;
		pacifistLine.color = Color.clear;
		neutralLine.color = Color.clear;
	}

	public void Show(string beingID, int hellPassNum)
	{
		background.color = Color.white;
		if (SaveDataCtrl.Get(beingID + "HellPassDefeatedGenocide" + hellPassNum, false))
		{
			genocideLine.color = Color.white;
		}
		else
		{
			genocideLine.color = Color.clear;
		}
		if (SaveDataCtrl.Get(beingID + "HellPassDefeatedPacifist" + hellPassNum, false))
		{
			pacifistLine.color = Color.white;
		}
		else
		{
			pacifistLine.color = Color.clear;
		}
		if (SaveDataCtrl.Get(beingID + "HellPassDefeatedNeutral" + hellPassNum, false))
		{
			neutralLine.color = Color.white;
		}
		else
		{
			neutralLine.color = Color.clear;
		}
	}

	public bool Valid(string beingID, int hellPassNum)
	{
		if (SaveDataCtrl.Get(beingID + "HellPassDefeated" + hellPassNum, false))
		{
			return true;
		}
		return false;
	}
}
