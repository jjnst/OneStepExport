using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

[Serializable]
[MoonSharpUserData]
public class TileApp
{
	public Location location;

	public List<Pattern> locationPattern;

	public Shape shape;

	public List<Pattern> pattern;

	public int position;

	public Tile tile;

	public TileApp(Location loc, Shape shap, List<Pattern> pat, int position = 1, Tile tile = null, List<Pattern> locPattern = null)
	{
		location = loc;
		locationPattern = locPattern;
		shape = shap;
		pattern = pat;
		this.position = position;
		this.tile = tile;
	}

	public TileApp(Location loc, Shape shap = Shape.Default, Pattern pat = Pattern.All, int position = 1, Tile tile = null, Pattern locPattern = Pattern.All)
	{
		location = loc;
		if (locPattern != 0)
		{
			locationPattern = new List<Pattern> { locPattern };
		}
		shape = shap;
		if (pat != 0)
		{
			pattern = new List<Pattern> { pat };
		}
		this.position = position;
		this.tile = tile;
	}
}
