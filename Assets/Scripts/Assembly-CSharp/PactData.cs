using System;

[Serializable]
public struct PactData
{
	public string itemID;

	public bool loaded;

	public string rewardID;

	public int duration;

	public int originalDuration;

	public bool depleted;

	public bool hellPass;

	public PactData(PactObject pactObj)
	{
		itemID = pactObj.itemID;
		loaded = true;
		rewardID = pactObj.rewardID;
		duration = pactObj.duration;
		originalDuration = pactObj.originalDuration;
		depleted = pactObj.depleted;
		hellPass = pactObj.hellPass;
	}

	public PactData(string itemID)
	{
		this.itemID = itemID;
		loaded = false;
		rewardID = "";
		duration = 1;
		originalDuration = 1;
		depleted = false;
		hellPass = false;
	}
}
