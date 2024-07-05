using System;

[Serializable]
public struct RunHistoryData
{
	public int day;

	public int month;

	public int year;

	public string beingName;

	public string beingID;

	public string seed;

	public string zone;

	public RunHistoryData(string theBeingName, string theBeingID, string theSeed, string theZone)
	{
		day = DateTime.Now.Day;
		month = DateTime.Now.Month;
		year = DateTime.Now.Year;
		beingName = theBeingName;
		beingID = theBeingID;
		seed = theSeed;
		zone = theZone;
	}
}
