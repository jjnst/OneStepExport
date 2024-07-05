using System.Collections.Generic;
using System.Xml;

public class World
{
	public string nameString;

	public string background;

	public string iconName;

	public string transition;

	public string[] tagStrings = new string[0];

	public List<Tag> tags = new List<Tag>();

	public int numZones;

	public int numStages;

	public int id;

	public XmlNode node;
}
