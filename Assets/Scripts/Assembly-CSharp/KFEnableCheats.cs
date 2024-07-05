using Rewired;
using UnityEngine;

public class KFEnableCheats : MonoBehaviour
{
	public static bool isUnlocked = false;

	private string[] actionSequence = new string[14]
	{
		"Cheat_Up", "Cheat_Up", "Cheat_Up", "Cheat_Down", "Cheat_Down", "Cheat_Down", "Cheat_Left", "Cheat_Right", "Cheat_Left", "Cheat_Right",
		"Cheat_Left", "Cheat_Right", "Cheat_L", "Cheat_R"
	};

	private int sequencePos = 0;

	private void Awake()
	{
	}

	private void Update()
	{
		if (isUnlocked)
		{
			return;
		}
		if (!S.I.btnCtrl.activeNavPanels.Contains(S.I.optCtrl))
		{
			sequencePos = 0;
			return;
		}
		Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
		if (!rewiredPlayer.GetAnyButtonUp())
		{
			return;
		}
		string actionName = actionSequence[sequencePos];
		if (rewiredPlayer.GetButtonUp(actionName))
		{
			sequencePos++;
			if (sequencePos >= actionSequence.Length)
			{
				UnlockCheats();
			}
		}
		else
		{
			sequencePos = 0;
		}
	}

	private void UnlockCheats()
	{
		Debug.Log("KFEnableCheats: Cheats now unlocked!");
		isUnlocked = true;
	}
}
