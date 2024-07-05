using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HitDisplay : MonoBehaviour
{
	public GameObject tileRefPrefab;

	private List<Image> tileRefs = new List<Image>();

	private void Awake()
	{
		base.transform.DestroyChildren();
	}

	public void Show(SpellObject spellObj)
	{
		while (tileRefs.Count < 32)
		{
			tileRefs.Add(SimplePool.Spawn(tileRefPrefab, base.transform.position, base.transform.rotation, base.transform).GetComponent<Image>());
		}
		CalculateHit(spellObj);
	}

	private void CalculateHit(SpellObject spellObj)
	{
		foreach (Image tileRef in tileRefs)
		{
			tileRef.color = Color.white;
		}
		List<Tile> list = spellObj.Get(Mathf.RoundToInt(spellObj.numTiles));
		foreach (Tile item in list)
		{
			tileRefs[item.x + item.y * 8].color = Color.red;
		}
		tileRefs[spellObj.being.mov.currentTile.x + spellObj.being.mov.currentTile.y * 8].color = Color.yellow;
	}
}
