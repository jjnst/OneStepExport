using System.Collections.Generic;
using SonicBloom.Koreo;

public class Korevent : KoreographyEvent
{
	public string value;

	public int measurePos;

	public int measureNum;

	public int patternNum;

	public float warningTime;

	public bool triggered = false;

	public bool speakerWarningShown = false;

	public List<Tile> tiles = new List<Tile>();

	public Korevent(KoreographyEvent theEvent)
	{
		base.StartSample = theEvent.StartSample;
		base.EndSample = theEvent.EndSample;
		base.Payload = theEvent.Payload;
	}

	public Korevent(int startingSample, int endingSample, float warnTime, int patNum)
	{
		base.StartSample = startingSample;
		base.EndSample = endingSample;
		warningTime = warnTime;
		patternNum = patNum;
	}
}
