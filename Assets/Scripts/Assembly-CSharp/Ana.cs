using System.Collections.Generic;
using UnityEngine;

public class Ana : MonoBehaviour
{
	private string currentVersion;

	private string lifetime_battles;

	private string finished_zones;

	private string itemID;

	private string rarity;

	private string type;

	private void Awake()
	{
		currentVersion = S.I.vrCtrl.currentVersion;
	}

	public static void CustomEvent(string key, Dictionary<string, object> data)
	{
	}

	public void SendLootPick(int finished_zones, string beingID, ItemObject itemObj, int deck_size)
	{
	}

	public void SendLevelUpPick(int finished_zones, string beingID, ItemObject itemObj, int deck_size)
	{
	}

	public void SendUpgradePick(int finished_zones, string beingID, ItemObject itemObj, int deck_size)
	{
	}

	public void SendRemovePick(int finished_zones, string beingID, ItemObject itemObj, int deck_size)
	{
	}
}
