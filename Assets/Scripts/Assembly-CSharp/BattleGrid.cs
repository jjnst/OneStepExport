using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class BattleGrid
{
	public Tile[,] grid;

	public int gridLength;

	public int gridHeight;

	public GameObject gridContainer;

	private List<Tile> tempTiles = new List<Tile>();

	private List<Tile> locationTiles = new List<Tile>();

	private List<Tile> finalTiles = new List<Tile>();

	public List<Being> currentObstacles = new List<Being>();

	public List<Being> currentAllies = new List<Being>();

	public List<Cpu> currentEnemies = new List<Cpu>();

	public List<Structure> currentStructures = new List<Structure>();

	public List<Being> currentBeings = new List<Being>();

	public List<Being> currentDeadBeings = new List<Being>();

	public List<Projectile> currentProjectiles = new List<Projectile>();

	public List<Tile> currentFlames = new List<Tile>();

	public Overlay darknessOverlay;

	public BC ctrl;

	public Being lastTargetHit = null;

	public Tile lastTargetHitTile = null;

	public Being lastTargetedGlobal = null;

	public bool inBattle = false;

	public bool inMidBattle = false;

	public BattleGrid(int x, int y, BC batCtrl)
	{
		ctrl = batCtrl;
		grid = new Tile[x, y];
		gridLength = x;
		gridHeight = y;
	}

	public void ClearTileColor()
	{
		Tile[,] array = grid;
		foreach (Tile tile in array)
		{
			tile.alignSpriteRend.color = Color.clear;
			tile.backSpriteRend.color = Color.clear;
			tile.shadowSpriteRend.color = Color.clear;
		}
	}

	public void RestoreTileColor()
	{
		Tile[,] array = grid;
		foreach (Tile tile in array)
		{
			tile.alignSpriteRend.color = Color.white;
			tile.backSpriteRend.color = Color.white;
			tile.shadowSpriteRend.color = Color.white;
		}
	}

	public IEnumerator FadeOutField()
	{
		List<Tile> tileList = new List<Tile>();
		for (int x = 0; x < gridLength; x++)
		{
			for (int y = 0; y < gridHeight; y++)
			{
				tileList.Add(grid[x, y]);
			}
		}
		foreach (Tile tile in tileList)
		{
			tile.anim.SetTrigger("Fade");
			yield return null;
		}
	}

	public IEnumerator AnimateField()
	{
		for (int x = 0; x < gridLength; x++)
		{
			for (int y = 0; y < gridHeight; y++)
			{
				grid[x, y].anim.SetTrigger("MoveDown");
				if (y == 3)
				{
					yield return null;
				}
			}
		}
	}

	public void Darkness(float duration, float transitionTime)
	{
		darknessOverlay.Flash(duration, transitionTime);
	}

	public void FixAllTiles()
	{
		Tile[,] array = grid;
		foreach (Tile tile in array)
		{
			tile.Fix();
		}
	}

	public Tile GetTileAt(int x, int y)
	{
		return grid[x, y];
	}

	public List<Tile> LocalCross(Tile centerTile)
	{
		List<Tile> list = new List<Tile>();
		for (int i = -1; i <= 1; i++)
		{
			if (centerTile.y - i < gridHeight && centerTile.y - i >= 0)
			{
				list.Add(grid[centerTile.x, centerTile.y - i]);
			}
			if (i != 0 && centerTile.x - i < gridLength && centerTile.x - i >= 0)
			{
				list.Add(grid[centerTile.x - i, centerTile.y]);
			}
		}
		return list;
	}

	public bool TileExists(int x, int y)
	{
		if (x > gridLength - 1 || y > gridHeight - 1 || x < 0 || y < 0)
		{
			return false;
		}
		return true;
	}

	public Tile TileClosestExisting(int x, int y)
	{
		if (x > gridLength - 1)
		{
			x = gridLength - 1;
		}
		else if (x < 0)
		{
			x = 0;
		}
		if (y > gridHeight - 1)
		{
			y = gridHeight - 1;
		}
		else if (y < 0)
		{
			y = 0;
		}
		return grid[x, y];
	}

	public List<Tile> Randomize(List<Tile> tileList, bool seed)
	{
		List<Tile> list = new List<Tile>();
		List<int> list2 = Utils.RandomList(tileList.Count, false, seed);
		for (int i = 0; i < tileList.Count; i++)
		{
			list.Add(tileList[list2[i]]);
		}
		return list;
	}

	public void SetSerif()
	{
		for (int i = 0; i < gridLength; i++)
		{
			for (int j = 0; j < gridHeight; j++)
			{
				grid[i, j].SetAlign(0);
			}
		}
	}

	public void ExtinguishFlames()
	{
		for (int num = currentFlames.Count - 1; num >= 0; num--)
		{
			currentFlames[num].Extinguish();
		}
	}

	public void ClearField(bool excludeHealingProjectiles = false, bool overrideDeathSequences = false, bool clearDeadBeings = true)
	{
		ClearProjectiles(excludeHealingProjectiles);
		for (int num = currentStructures.Count - 1; num >= 0; num--)
		{
			currentStructures[num].Clear(overrideDeathSequences);
		}
		for (int num2 = currentEnemies.Count - 1; num2 >= 0; num2--)
		{
			currentEnemies[num2].Clear(overrideDeathSequences);
		}
		if (!clearDeadBeings)
		{
			return;
		}
		for (int num3 = currentDeadBeings.Count - 1; num3 >= 0; num3--)
		{
			if (currentDeadBeings[num3] != null)
			{
				currentDeadBeings[num3].Clean();
			}
		}
		currentDeadBeings.Clear();
	}

	public void ClearProjectiles(bool excludeHealing = false)
	{
		if (excludeHealing)
		{
			for (int num = currentProjectiles.Count - 1; num >= 0; num--)
			{
				if (!currentProjectiles[num].spellObj.HasEffect(Effect.HealBattle) && !currentProjectiles[num].spellObj.HasEffect(Effect.Heal) && !currentProjectiles[num].spellObj.HasEffect(Effect.Money) && currentProjectiles[num].being.player == null)
				{
					SimplePool.Despawn(currentProjectiles[num].gameObject);
				}
			}
		}
		else
		{
			for (int num2 = currentProjectiles.Count - 1; num2 >= 0; num2--)
			{
				SimplePool.Despawn(currentProjectiles[num2].gameObject);
			}
		}
		ExtinguishFlames();
	}

	public bool InBattle()
	{
		return inBattle;
	}

	public bool InMidBattle()
	{
		return inMidBattle;
	}

	public Tile GetTile(Int2 inter)
	{
		return grid[inter.x, inter.y];
	}

	public List<Tile> Get(TileApp tileApp, int num = 1, Being being = null, ItemObject itemObj = null, bool seed = false)
	{
		return Get(tileApp.location, tileApp.shape, num, being, tileApp.pattern, tileApp.locationPattern, tileApp.position, tileApp.tile, itemObj, seed);
	}

	public List<Tile> Get(Location loc, Shape shape, int num = 1, Being being = null, List<Pattern> pats = null, List<Pattern> locPats = null, int position = 1, Tile tile = null, ItemObject itemObj = null, bool seed = false)
	{
		int num2 = 0;
		int num3 = 0;
		if ((bool)being)
		{
			num2 = being.alignNum;
			num3 = being.FacingDirection();
		}
		int num4 = gridLength / 4 - gridLength / 4 * num3;
		int num5 = gridLength / 4 * 3 - gridLength / 4 * num3;
		int num6 = gridLength / 4 - gridLength / 4 * -num3;
		int num7 = gridLength / 4 * 3 - gridLength / 4 * -num3;
		int num8 = Mathf.RoundToInt((float)(gridLength / 2) - 0.5f + (float)num3 / 2f);
		finalTiles = new List<Tile>();
		tempTiles = new List<Tile>();
		locationTiles = new List<Tile>();
		switch (loc)
		{
		case Location.Above:
			locationTiles.Add(being.TileLocal(0, position));
			break;
		case Location.Base:
			locationTiles.Add(grid[Mathf.RoundToInt((float)(gridLength - 1) / 2f - (float)(gridLength - 1) / 2f * (float)num3 + (float)(position * num3)), Mathf.Clamp(position, 0, gridHeight - 1)]);
			break;
		case Location.BaseZero:
			locationTiles.Add(grid[position, 0]);
			break;
		case Location.Behind:
			locationTiles.Add(being.TileLocal(-position));
			break;
		case Location.BehindAll:
		{
			int x = being.mov.endTile.x;
			int num19 = (gridLength - 1 + (gridLength - 1) * -being.FacingDirection()) / 2 + x * being.FacingDirection();
			for (int num20 = 0; num20 < num19; num20++)
			{
				locationTiles.Add(grid[x - (num19 - num20) * being.FacingDirection(), being.mov.endTile.y]);
			}
			break;
		}
		case Location.Beings:
			foreach (Being currentBeing in currentBeings)
			{
				AddTileToList(locationTiles, currentBeing.mov.endTile);
			}
			break;
		case Location.BotLeftTopRight:
			locationTiles.Add(grid[0, 0]);
			locationTiles.Add(grid[3, 3]);
			break;
		case Location.BotLeftTopRightTwo:
			locationTiles.Add(grid[0, 0]);
			locationTiles.Add(grid[0, 1]);
			locationTiles.Add(grid[3, 3]);
			locationTiles.Add(grid[3, 2]);
			break;
		case Location.BotLeftTopRightSquares:
			locationTiles.Add(grid[0, 0]);
			locationTiles.Add(grid[0, 1]);
			locationTiles.Add(grid[1, 0]);
			locationTiles.Add(grid[1, 1]);
			locationTiles.Add(grid[3, 3]);
			locationTiles.Add(grid[3, 2]);
			locationTiles.Add(grid[2, 3]);
			locationTiles.Add(grid[2, 2]);
			break;
		case Location.BrokenTiles:
		{
			Tile[,] array4 = grid;
			foreach (Tile tile3 in array4)
			{
				if (tile3.type == TileType.Broken)
				{
					locationTiles.Add(tile3);
				}
			}
			break;
		}
		case Location.CrackedTiles:
		{
			Tile[,] array3 = grid;
			foreach (Tile tile2 in array3)
			{
				if (tile2.type == TileType.Cracked)
				{
					locationTiles.Add(tile2);
				}
			}
			break;
		}
		case Location.Current:
			AddTileToList(locationTiles, being.mov.currentTile);
			break;
		case Location.CurrentPlayer:
			if ((bool)ctrl.currentPlayer)
			{
				locationTiles.Add(ctrl.currentPlayer.mov.currentTile);
			}
			else if ((bool)being)
			{
				locationTiles.Add(being.TileLocal(gridLength / 2));
			}
			break;
		case Location.End:
			AddTileToList(locationTiles, being.mov.endTile);
			break;
		case Location.Enemies:
			foreach (Cpu currentEnemy in currentEnemies)
			{
				AddTileToList(locationTiles, currentEnemy.mov.endTile);
			}
			break;
		case Location.Flame:
			foreach (Tile currentFlame in currentFlames)
			{
				locationTiles.Add(currentFlame);
			}
			break;
		case Location.Front:
			locationTiles.Add(being.TileLocal(position));
			break;
		case Location.FrontAll:
		{
			int num40 = being.mov.endTile.x;
			int num41 = (gridLength - 1 + (gridLength - 1) * being.FacingDirection()) / 2 + num40 * being.FacingDirection() * -1;
			for (int num42 = 0; num42 < num41; num42++)
			{
				num40 += being.FacingDirection();
				locationTiles.Add(grid[num40, being.mov.endTile.y]);
			}
			break;
		}
		case Location.Frosted:
			foreach (Being currentBeing2 in currentBeings)
			{
				if ((bool)currentBeing2.GetStatusEffect(Status.Frost))
				{
					AddTileToList(locationTiles, currentBeing2.mov.endTile);
				}
			}
			break;
		case Location.FurthestEnemy:
		{
			List<Tile> list7 = new List<Tile>();
			foreach (Cpu currentEnemy2 in currentEnemies)
			{
				AddTileToList(list7, currentEnemy2.mov.endTile);
			}
			if (list7.Count > 0)
			{
				list7 = list7.OrderBy((Tile e) => e.GetComponent<Tile>().x * -being.FacingDirection()).ToList();
				locationTiles.Add(list7[0]);
			}
			break;
		}
		case Location.HalfField:
			locationTiles.Add(being.TileLocal(gridLength / 2));
			break;
		case Location.Index:
		{
			int num37 = 1;
			for (int num38 = 0; num38 < gridHeight; num38++)
			{
				for (int num39 = 0; num39 < gridLength; num39++)
				{
					if (num37 == position)
					{
						locationTiles.Add(grid[num39, num38]);
					}
					num37++;
				}
			}
			break;
		}
		case Location.LastKilledEnemy:
			AddTileToList(locationTiles, ctrl.lastKilledEnemy.mov.endTile);
			break;
		case Location.LastKilledStructure:
			AddTileToList(locationTiles, ctrl.lastKilledStructure.mov.endTile);
			break;
		case Location.LastTargetedGlobal:
			if ((bool)lastTargetedGlobal)
			{
				locationTiles.Add(lastTargetedGlobal.mov.currentTile);
			}
			break;
		case Location.LastTargetHit:
			if ((bool)lastTargetHitTile)
			{
				locationTiles.Add(lastTargetHitTile);
			}
			else
			{
				AddTileToList(locationTiles, being.mov.endTile);
			}
			break;
		case Location.LastHitBy:
			if ((bool)being.lastHitBy)
			{
				AddTileToList(locationTiles, being.lastHitBy.mov.endTile);
			}
			break;
		case Location.LastHitByOther:
			if ((bool)being.lastHitByOther)
			{
				AddTileToList(locationTiles, being.lastHitByOther.mov.endTile);
			}
			break;
		case Location.LastSpawned:
			if ((bool)currentBeings[currentBeings.Count - 1])
			{
				AddTileToList(locationTiles, currentBeings[currentBeings.Count - 1].mov.endTile);
			}
			break;
		case Location.LastTileTouched:
			AddTileToList(locationTiles, itemObj.spellObj.touchedTile);
			break;
		case Location.LastTileTouchedParent:
			AddTileToList(locationTiles, itemObj.parentSpell.touchedTile);
			break;
		case Location.NearestEnemy:
		{
			List<Tile> list2 = new List<Tile>();
			foreach (Cpu currentEnemy3 in currentEnemies)
			{
				AddTileToList(list2, currentEnemy3.mov.endTile);
			}
			if (list2.Count > 0)
			{
				list2 = list2.OrderBy((Tile e) => e.GetComponent<Tile>().x * being.FacingDirection()).ToList();
				locationTiles.Add(list2[0]);
			}
			break;
		}
		case Location.NearestOther:
		{
			List<Tile> list = new List<Tile>();
			foreach (Being currentBeing3 in currentBeings)
			{
				if (currentBeing3.mov.endTile.x >= num6 && currentBeing3.mov.endTile.x < num7)
				{
					AddTileToList(list, currentBeing3.mov.endTile);
				}
			}
			if (list.Count > 0)
			{
				list = list.OrderBy((Tile e) => e.GetComponent<Tile>().x * being.FacingDirection()).ToList();
				locationTiles.Add(list[0]);
			}
			break;
		}
		case Location.Owner:
			if (itemObj != null && (bool)itemObj.being && itemObj.being.parentObj != null && (bool)itemObj.being.parentObj.being)
			{
				AddTileToList(locationTiles, itemObj.being.parentObj.being.mov.currentTile);
			}
			break;
		case Location.ParentSpellTargetHit:
		{
			SpellObject parentSpell = itemObj.parentSpell;
			if ((bool)((parentSpell != null) ? parentSpell.hitBeing : null))
			{
				locationTiles.Add(itemObj.parentSpell.hitTile);
			}
			else
			{
				locationTiles.Add(being.mov.endTile);
			}
			break;
		}
		case Location.Player:
			if (ctrl.currentPlayers.Count > 1)
			{
				locationTiles.Add(ctrl.currentPlayers[Random.Range(0, ctrl.currentPlayers.Count)].mov.currentTile);
			}
			else if ((bool)ctrl.currentPlayer)
			{
				locationTiles.Add(ctrl.currentPlayer.mov.currentTile);
			}
			else if ((bool)being)
			{
				locationTiles.Add(being.TileLocal(gridLength / 2));
			}
			break;
		case Location.PlayerBotLeft:
		{
			List<Tile> list6 = new List<Tile>();
			list6.Add(grid[0, 0]);
			list6.Add(grid[0, 1]);
			list6.Add(grid[1, 0]);
			list6.Add(grid[1, 1]);
			for (int num36 = 0; num36 < num; num36++)
			{
				Tile item4 = list6[Random.Range(0, list6.Count)];
				locationTiles.Add(item4);
				list6.Remove(item4);
				if (list6.Count < 1)
				{
					break;
				}
			}
			break;
		}
		case Location.PlayerBotRight:
		{
			List<Tile> list5 = new List<Tile>();
			list5.Add(grid[2, 0]);
			list5.Add(grid[2, 1]);
			list5.Add(grid[3, 0]);
			list5.Add(grid[3, 1]);
			for (int num35 = 0; num35 < num; num35++)
			{
				Tile item3 = list5[Random.Range(0, list5.Count)];
				locationTiles.Add(item3);
				list5.Remove(item3);
				if (list5.Count < 1)
				{
					break;
				}
			}
			break;
		}
		case Location.PlayerTopLeft:
		{
			List<Tile> list4 = new List<Tile>();
			list4.Add(grid[0, 2]);
			list4.Add(grid[0, 3]);
			list4.Add(grid[1, 2]);
			list4.Add(grid[1, 3]);
			for (int num32 = 0; num32 < num; num32++)
			{
				Tile item2 = list4[Random.Range(0, list4.Count)];
				locationTiles.Add(item2);
				list4.Remove(item2);
				if (list4.Count < 1)
				{
					break;
				}
			}
			break;
		}
		case Location.PlayerTopRight:
		{
			List<Tile> list3 = new List<Tile>();
			list3.Add(grid[2, 2]);
			list3.Add(grid[2, 3]);
			list3.Add(grid[3, 2]);
			list3.Add(grid[3, 3]);
			for (int num31 = 0; num31 < num; num31++)
			{
				Tile item = list3[Random.Range(0, list3.Count)];
				locationTiles.Add(item);
				list3.Remove(item);
				if (list3.Count < 1)
				{
					break;
				}
			}
			break;
		}
		case Location.Poisoned:
			foreach (Being currentBeing4 in currentBeings)
			{
				if ((bool)currentBeing4.GetStatusEffect(Status.Poison))
				{
					AddTileToList(locationTiles, currentBeing4.mov.endTile);
				}
			}
			break;
		case Location.RandEnemy:
			foreach (Cpu currentEnemy4 in currentEnemies)
			{
				AddTileToList(tempTiles, currentEnemy4.mov.endTile);
			}
			locationTiles = Randomize(tempTiles, seed);
			break;
		case Location.RandEnemyUnique:
		{
			for (int num27 = gridLength / 2; num27 < gridLength; num27++)
			{
				for (int num28 = 0; num28 < gridHeight; num28++)
				{
					tempTiles.Add(grid[num27, num28]);
				}
			}
			locationTiles = Randomize(tempTiles, seed);
			break;
		}
		case Location.RandOther:
		{
			for (int num26 = 0; num26 < num; num26++)
			{
				locationTiles.Add(grid[Random.Range(num6, num7), Random.Range(0, gridHeight)]);
			}
			break;
		}
		case Location.RandOtherUnique:
		{
			for (int num24 = num6; num24 < num7; num24++)
			{
				for (int num25 = 0; num25 < gridHeight; num25++)
				{
					tempTiles.Add(grid[num24, num25]);
				}
			}
			locationTiles = Randomize(tempTiles, seed);
			break;
		}
		case Location.RandAllied:
		{
			for (int num23 = 0; num23 < num; num23++)
			{
				locationTiles.Add(grid[Random.Range(num4, num5), Random.Range(0, gridHeight)]);
			}
			break;
		}
		case Location.RandAlliedUnique:
		{
			for (int num21 = num4; num21 < num5; num21++)
			{
				for (int num22 = 0; num22 < gridHeight; num22++)
				{
					tempTiles.Add(grid[num21, num22]);
				}
			}
			locationTiles = Randomize(tempTiles, seed);
			break;
		}
		case Location.RandPlayerUnique:
		{
			for (int num17 = 0; num17 < 4; num17++)
			{
				for (int num18 = 0; num18 < gridHeight; num18++)
				{
					tempTiles.Add(grid[num17, num18]);
				}
			}
			locationTiles = Randomize(tempTiles, seed);
			break;
		}
		case Location.Rand:
		{
			for (int num16 = 0; num16 < num; num16++)
			{
				locationTiles.Add(grid[Random.Range(0, gridLength), Random.Range(0, gridHeight)]);
			}
			break;
		}
		case Location.RandUnique:
		{
			for (int num14 = 0; num14 < gridLength; num14++)
			{
				for (int num15 = 0; num15 < gridHeight; num15++)
				{
					tempTiles.Add(grid[num14, num15]);
				}
			}
			locationTiles = Randomize(tempTiles, seed);
			break;
		}
		case Location.Saved:
			locationTiles = new List<Tile>(being.savedTileList);
			break;
		case Location.SpellTargetHit:
			if ((bool)itemObj.hitBeing)
			{
				locationTiles.Add(itemObj.hitTile);
			}
			else
			{
				AddTileToList(locationTiles, being.mov.endTile);
			}
			break;
		case Location.SpiralCounterClockwise:
		{
			int[] array = new int[16]
			{
				0, 1, 2, 3, 3, 3, 3, 2, 1, 0,
				0, 0, 1, 2, 2, 1
			};
			int[] array2 = new int[16]
			{
				0, 0, 0, 0, 1, 2, 3, 3, 3, 3,
				2, 1, 1, 1, 2, 2
			};
			for (int num13 = 0; num13 < array.Length; num13++)
			{
				locationTiles.Add(grid[array[num13], array2[num13]]);
			}
			break;
		}
		case Location.Square:
			locationTiles.Add(grid[num6 + 1, 1]);
			locationTiles.Add(grid[num6 + 2, 1]);
			locationTiles.Add(grid[num6 + 1, 2]);
			locationTiles.Add(grid[num6 + 2, 2]);
			break;
		case Location.Structures:
			foreach (Structure currentStructure in currentStructures)
			{
				if (!currentStructure.beingObj.tags.Contains(Tag.NotStructure))
				{
					AddTileToList(locationTiles, currentStructure.mov.endTile);
				}
			}
			break;
		case Location.Tile:
			locationTiles.Add(tile);
			break;
		case Location.SweeperColumnBotRight:
		{
			for (int n = num8; n >= 0 && n <= 7; n += num3)
			{
				if (n % 2 == 0)
				{
					for (int num11 = 3; num11 >= 0; num11--)
					{
						if (locationTiles.Count < num || num == 0)
						{
							locationTiles.Add(grid[n, num11]);
						}
					}
					continue;
				}
				for (int num12 = 0; num12 <= 3; num12++)
				{
					if (locationTiles.Count < num || num == 0)
					{
						locationTiles.Add(grid[n, num12]);
					}
				}
			}
			break;
		}
		case Location.SweeperColumnFront:
		{
			int num10 = (gridLength - 1 + (gridLength - 1) * num3) / 2 + being.mov.currentTile.x * num3 * -1;
			for (int k = being.mov.currentTile.x + num3; k <= num10 && k >= 0; k += num3)
			{
				if (k % 2 == 0)
				{
					for (int l = 3; l >= 0; l--)
					{
						if (locationTiles.Count < num || num == 0)
						{
							locationTiles.Add(grid[k, l]);
						}
					}
					continue;
				}
				for (int m = 0; m <= 3; m++)
				{
					if (locationTiles.Count < num || num == 0)
					{
						locationTiles.Add(grid[k, m]);
					}
				}
			}
			break;
		}
		case Location.SweeperColumnTopLeft:
		{
			int num9 = num6;
			while (num9 >= 0 && num9 <= 7)
			{
				if (num9 % 2 == 0)
				{
					for (int i = 3; i >= 0; i--)
					{
						if (locationTiles.Count < num || num == 0)
						{
							locationTiles.Add(grid[num9, i]);
						}
					}
				}
				else
				{
					for (int j = 0; j <= 3; j++)
					{
						if (locationTiles.Count < num || num == 0)
						{
							locationTiles.Add(grid[num9, j]);
						}
					}
				}
				num9 -= num3;
			}
			break;
		}
		}
		locationTiles = ApplyTilePattern(locationTiles, locPats, being, seed);
		if (shape != 0)
		{
			foreach (Tile locationTile in locationTiles)
			{
				switch (shape)
				{
				case Shape.Above:
					if (TileExists(locationTile.x, locationTile.y + 1))
					{
						finalTiles.Add(grid[locationTile.x, locationTile.y + 1]);
					}
					break;
				case Shape.AboveBelow:
					if (TileExists(locationTile.x, locationTile.y + 1))
					{
						finalTiles.Add(grid[locationTile.x, locationTile.y + 1]);
					}
					if (TileExists(locationTile.x, locationTile.y - 1))
					{
						finalTiles.Add(grid[locationTile.x, locationTile.y - 1]);
					}
					break;
				case Shape.Adjacent:
					if (TileExists(locationTile.x, locationTile.y + 1))
					{
						tempTiles.Add(grid[locationTile.x, locationTile.y + 1]);
					}
					if (TileExists(locationTile.x, locationTile.y - 1))
					{
						tempTiles.Add(grid[locationTile.x, locationTile.y - 1]);
					}
					if (TileExists(locationTile.x + 1, locationTile.y))
					{
						tempTiles.Add(grid[locationTile.x + 1, locationTile.y]);
					}
					if (TileExists(locationTile.x - 1, locationTile.y))
					{
						tempTiles.Add(grid[locationTile.x - 1, locationTile.y]);
					}
					finalTiles = Randomize(tempTiles, seed);
					break;
				case Shape.Behind:
					if ((bool)being)
					{
						finalTiles.Add(TileClosestExisting(locationTile.x - position * num3, locationTile.y));
					}
					else
					{
						finalTiles.Add(TileClosestExisting(locationTile.x - position, locationTile.y));
					}
					break;
				case Shape.BehindAll:
				{
					int num64 = locationTile.x;
					int num65 = (gridLength - 1 + (gridLength - 1) * -num3) / 2 + num64 * num3;
					for (int num66 = 0; num66 < num65; num66++)
					{
						num64 -= num3;
						finalTiles.Add(grid[num64, locationTile.y]);
					}
					break;
				}
				case Shape.Below:
					if (TileExists(locationTile.x, locationTile.y - 1))
					{
						finalTiles.Add(grid[locationTile.x, locationTile.y - 1]);
					}
					break;
				case Shape.Bot:
					finalTiles.Add(grid[locationTile.x, 0]);
					break;
				case Shape.Box:
				{
					int[] array21 = new int[9] { 0, 1, 1, 1, 0, -1, -1, -1, 0 };
					int[] array22 = new int[9] { 0, 1, 0, -1, -1, -1, 0, 1, 1 };
					for (int num83 = 0; num83 < array21.Length; num83++)
					{
						if (TileExists(locationTile.x + array21[num83], locationTile.y + array22[num83]))
						{
							finalTiles.Add(grid[locationTile.x + array21[num83], locationTile.y + array22[num83]]);
						}
					}
					break;
				}
				case Shape.Column:
				{
					for (int num74 = gridHeight - 1; num74 >= 0; num74--)
					{
						for (int num75 = 0; num75 < gridLength; num75++)
						{
							if (num75 == locationTile.x)
							{
								finalTiles.Add(grid[num75, num74]);
							}
						}
					}
					break;
				}
				case Shape.ColumnAnti:
				{
					for (int num70 = gridHeight - 1; num70 >= 0; num70--)
					{
						for (int num71 = num6; num71 < num7; num71++)
						{
							if (num71 != locationTile.x)
							{
								finalTiles.Add(grid[num71, num70]);
							}
						}
					}
					break;
				}
				case Shape.ColumnSmall:
					finalTiles.Add(locationTile);
					if (TileExists(locationTile.x, locationTile.y + 1))
					{
						finalTiles.Add(grid[locationTile.x, locationTile.y + 1]);
					}
					if (TileExists(locationTile.x, locationTile.y - 1))
					{
						finalTiles.Add(grid[locationTile.x, locationTile.y - 1]);
					}
					break;
				case Shape.ColumnTwo:
				{
					for (int num96 = gridHeight - 1; num96 >= 0; num96--)
					{
						for (int num97 = 0; num97 < gridLength; num97++)
						{
							if (num97 == locationTile.x)
							{
								if (TileExists(num97, num96))
								{
									finalTiles.Add(grid[num97, num96]);
								}
								if (TileExists(num97 + num3, num96))
								{
									finalTiles.Add(grid[num97 + num3, num96]);
								}
							}
						}
					}
					break;
				}
				case Shape.ColumnWide:
				{
					for (int num90 = gridHeight - 1; num90 >= 0; num90--)
					{
						for (int num91 = 0; num91 < gridLength; num91++)
						{
							if (num91 == locationTile.x)
							{
								if (TileExists(num91 + num3, num90))
								{
									finalTiles.Add(grid[num91 + num3, num90]);
								}
								if (TileExists(num91, num90))
								{
									finalTiles.Add(grid[num91, num90]);
								}
								if (TileExists(num91 - num3, num90))
								{
									finalTiles.Add(grid[num91 - num3, num90]);
								}
							}
						}
					}
					break;
				}
				case Shape.Cone:
				{
					int[] array17 = new int[16]
					{
						1, 2, 2, 2, 3, 3, 3, 3, 3, 4,
						4, 4, 4, 4, 4, 4
					};
					int[] array18 = new int[16]
					{
						0, 0, 1, -1, 0, 1, -1, 2, -2, 0,
						1, -1, 2, -2, 3, -3
					};
					for (int num81 = 0; num81 < array17.Length && (num81 < num || num == 0); num81++)
					{
						if (TileExists(locationTile.x + array17[num81] * num3, locationTile.y + array18[num81]))
						{
							finalTiles.Add(grid[locationTile.x + array17[num81] * num3, locationTile.y + array18[num81]]);
						}
					}
					break;
				}
				case Shape.ConeDouble:
				{
					int[] array11 = new int[16]
					{
						1, 2, 2, 2, 3, 3, 3, 3, 3, 4,
						4, 4, 4, 4, 4, 4
					};
					int[] array12 = new int[16]
					{
						0, 0, 1, -1, 0, 1, -1, 2, -2, 0,
						1, -1, 2, -2, 3, -3
					};
					for (int num76 = 0; num76 < array11.Length && num76 < num / 2; num76++)
					{
						if (TileExists(locationTile.x + array11[num76], locationTile.y + array12[num76]))
						{
							finalTiles.Add(grid[locationTile.x + array11[num76], locationTile.y + array12[num76]]);
						}
						if (TileExists(locationTile.x + array11[num76] * -1, locationTile.y + array12[num76]))
						{
							finalTiles.Add(grid[locationTile.x + array11[num76] * -1, locationTile.y + array12[num76]]);
						}
					}
					break;
				}
				case Shape.CrossAnti:
				{
					int num60 = 1;
					if (locationTile.x >= gridLength / 2)
					{
						num60 = -1;
					}
					for (int num61 = 0; num61 < gridLength; num61++)
					{
						for (int num62 = 0; num62 < gridHeight; num62++)
						{
							if (grid[num61, num62].align == num60 && num61 != locationTile.x && num62 != locationTile.y)
							{
								finalTiles.Add(grid[num61, num62]);
							}
						}
					}
					break;
				}
				case Shape.Cross:
				{
					int num57 = 1;
					if (locationTile.x >= gridLength / 2)
					{
						num57 = -1;
					}
					for (int num58 = 0; num58 < gridLength; num58++)
					{
						for (int num59 = 0; num59 < gridHeight; num59++)
						{
							if (grid[num58, num59].align == num57 && (num58 == locationTile.x || num59 == locationTile.y))
							{
								finalTiles.Add(grid[num58, num59]);
							}
						}
					}
					break;
				}
				case Shape.CrossFull:
				{
					for (int num53 = 0; num53 < gridLength; num53++)
					{
						for (int num54 = 0; num54 < gridHeight; num54++)
						{
							if (num53 == locationTile.x || num54 == locationTile.y)
							{
								finalTiles.Add(grid[num53, num54]);
							}
						}
					}
					break;
				}
				case Shape.CrossMed:
				{
					finalTiles = LocalCross(locationTile);
					for (int num44 = -2; num44 <= 2; num44++)
					{
						if (Mathf.Abs(num44) > 1)
						{
							if (locationTile.y - num44 < gridHeight && locationTile.y - num44 >= 0)
							{
								finalTiles.Add(grid[locationTile.x, locationTile.y - num44]);
							}
							if (num44 != 0 && locationTile.x - num44 < gridLength && locationTile.x - num44 >= 0)
							{
								finalTiles.Add(grid[locationTile.x - num44, locationTile.y]);
							}
						}
					}
					break;
				}
				case Shape.CrossSmall:
					finalTiles = LocalCross(locationTile);
					break;
				case Shape.DiagonalBotLeft:
				{
					for (int num86 = 3; num86 >= 0; num86--)
					{
						if (TileExists(locationTile.x - num86, locationTile.y - num86))
						{
							finalTiles.Add(grid[locationTile.x - num86, locationTile.y - num86]);
						}
					}
					for (int num87 = 1; num87 < 4; num87++)
					{
						if (TileExists(locationTile.x + num87, locationTile.y + num87))
						{
							finalTiles.Add(grid[locationTile.x + num87, locationTile.y + num87]);
						}
					}
					break;
				}
				case Shape.DiagonalTopLeft:
				{
					for (int num77 = 3; num77 >= 0; num77--)
					{
						if (TileExists(locationTile.x - num77, locationTile.y + num77))
						{
							finalTiles.Add(grid[locationTile.x - num77, locationTile.y + num77]);
						}
					}
					for (int num78 = 1; num78 < 4; num78++)
					{
						if (TileExists(locationTile.x + num78, locationTile.y - num78))
						{
							finalTiles.Add(grid[locationTile.x + num78, locationTile.y - num78]);
						}
					}
					break;
				}
				case Shape.DiagonalTopRight:
				{
					for (int num68 = 3; num68 >= 0; num68--)
					{
						if (TileExists(locationTile.x + num68, locationTile.y + num68))
						{
							finalTiles.Add(grid[locationTile.x + num68, locationTile.y + num68]);
						}
					}
					for (int num69 = 1; num69 < 4; num69++)
					{
						if (TileExists(locationTile.x - num69, locationTile.y - num69))
						{
							finalTiles.Add(grid[locationTile.x - num69, locationTile.y - num69]);
						}
					}
					break;
				}
				case Shape.Diamond:
					if (TileExists(locationTile.x, locationTile.y + 1))
					{
						finalTiles.Add(grid[locationTile.x, locationTile.y + 1]);
					}
					if (TileExists(locationTile.x + 1, locationTile.y))
					{
						finalTiles.Add(grid[locationTile.x + 1, locationTile.y]);
					}
					if (TileExists(locationTile.x, locationTile.y - 1))
					{
						finalTiles.Add(grid[locationTile.x, locationTile.y - 1]);
					}
					if (TileExists(locationTile.x - 1, locationTile.y))
					{
						finalTiles.Add(grid[locationTile.x - 1, locationTile.y]);
					}
					break;
				case Shape.Even:
				{
					int x2 = locationTile.x;
					int num50 = (gridLength - 1 + (gridLength - 1) * num3) / 2 + x2 * num3 * -1;
					int num51 = x2;
					for (int num52 = 1; num52 < num50; num52 += 2)
					{
						num51 += num3 * 2;
						finalTiles.Add(grid[num51, locationTile.y]);
					}
					break;
				}
				case Shape.Front:
					if ((bool)being)
					{
						finalTiles.Add(TileClosestExisting(locationTile.x + position * num3, locationTile.y));
					}
					else
					{
						finalTiles.Add(TileClosestExisting(locationTile.x + position, locationTile.y));
					}
					break;
				case Shape.FrontAll:
				{
					int num98 = locationTile.x;
					int num99 = (gridLength - 1 + (gridLength - 1) * num3) / 2 + num98 * num3 * -1;
					for (int num100 = 0; num100 < num99; num100++)
					{
						num98 += num3;
						finalTiles.Add(grid[num98, locationTile.y]);
					}
					break;
				}
				case Shape.Horizontal:
				{
					for (int num94 = num6; num94 < num7; num94++)
					{
						for (int num95 = 0; num95 < gridHeight; num95++)
						{
							if (num95 == locationTile.y)
							{
								finalTiles.Add(grid[num94, num95]);
							}
						}
					}
					break;
				}
				case Shape.HorizontalAnti:
				{
					for (int num92 = num6; num92 < num7; num92++)
					{
						for (int num93 = 0; num93 < gridHeight; num93++)
						{
							if (num93 != locationTile.y)
							{
								finalTiles.Add(grid[num92, num93]);
							}
						}
					}
					break;
				}
				case Shape.HorizontalTwo:
				{
					for (int num88 = num6; num88 < num7; num88++)
					{
						for (int num89 = 0; num89 < gridHeight; num89++)
						{
							if ((num89 == locationTile.y || num89 == locationTile.y + 1) && TileExists(num88, num89))
							{
								finalTiles.Add(grid[num88, num89]);
							}
						}
					}
					break;
				}
				case Shape.HorizontalWide:
				{
					for (int num84 = num6; num84 < num7; num84++)
					{
						for (int num85 = 0; num85 < gridHeight; num85++)
						{
							if (num85 >= locationTile.y - 1 && num85 <= locationTile.y + 1)
							{
								finalTiles.Add(grid[num84, num85]);
							}
						}
					}
					break;
				}
				case Shape.O:
				{
					int[] array19 = new int[8] { 1, 1, 1, 0, -1, -1, -1, 0 };
					int[] array20 = new int[8] { 1, 0, -1, -1, -1, 0, 1, 1 };
					for (int num82 = 0; num82 < array19.Length; num82++)
					{
						if (TileExists(locationTile.x + array19[num82], locationTile.y + array20[num82]))
						{
							finalTiles.Add(grid[locationTile.x + array19[num82], locationTile.y + array20[num82]]);
						}
					}
					break;
				}
				case Shape.OBig:
				{
					int[] array15 = new int[16]
					{
						2, 2, 2, 2, 2, 1, 0, -1, -2, -2,
						-2, -2, -2, -1, 0, 1
					};
					int[] array16 = new int[16]
					{
						2, 1, 0, -1, -2, -2, -2, -2, -2, -1,
						0, 1, 2, 2, 2, 2
					};
					for (int num80 = 0; num80 < array15.Length; num80++)
					{
						if (TileExists(locationTile.x + array15[num80], locationTile.y + array16[num80]))
						{
							finalTiles.Add(grid[locationTile.x + array15[num80], locationTile.y + array16[num80]]);
						}
					}
					break;
				}
				case Shape.OBigger:
				{
					int[] array13 = new int[24]
					{
						3, 3, 3, 3, 3, 3, 3, 2, 1, 0,
						-1, -2, -3, -3, -3, -3, -3, -3, -3, -2,
						-1, 0, 1, 2
					};
					int[] array14 = new int[24]
					{
						3, 2, 1, 0, -1, -2, -3, -3, -3, -3,
						-3, -3, -3, -2, -1, 0, 1, 2, 3, 3,
						3, 3, 3, 3
					};
					for (int num79 = 0; num79 < array13.Length; num79++)
					{
						if (TileExists(locationTile.x + array13[num79], locationTile.y + array14[num79]))
						{
							finalTiles.Add(grid[locationTile.x + array13[num79], locationTile.y + array14[num79]]);
						}
					}
					break;
				}
				case Shape.RhythmCross:
				{
					List<Tile> list8 = LocalCross(locationTile);
					for (int num72 = 0; num72 < gridHeight; num72++)
					{
						for (int num73 = 0; num73 < 4; num73++)
						{
							finalTiles.Add(grid[num73, num72]);
						}
					}
					foreach (Tile item5 in list8)
					{
						if (finalTiles.Contains(item5))
						{
							finalTiles.Remove(item5);
						}
					}
					finalTiles.Add(locationTile);
					break;
				}
				case Shape.Ring:
				{
					int[] array9 = new int[12]
					{
						0, 1, 2, 3, 3, 3, 3, 2, 1, 0,
						0, 0
					};
					int[] array10 = new int[12]
					{
						0, 0, 0, 0, 1, 2, 3, 3, 3, 3,
						2, 1
					};
					if (locationTile.x >= gridLength / 2)
					{
						array9 = new int[12]
						{
							7, 6, 5, 4, 4, 4, 4, 5, 6, 7,
							7, 7
						};
						array10 = new int[12]
						{
							3, 3, 3, 3, 2, 1, 0, 0, 0, 0,
							1, 2
						};
					}
					for (int num67 = 0; num67 < array9.Length; num67++)
					{
						finalTiles.Add(grid[array9[num67], array10[num67]]);
					}
					break;
				}
				case Shape.Row:
				{
					for (int num63 = gridLength - 1; num63 >= 0; num63--)
					{
						finalTiles.Add(grid[num63, locationTile.y]);
					}
					break;
				}
				case Shape.RowSmall:
					if (TileExists(locationTile.x + 1, locationTile.y))
					{
						finalTiles.Add(grid[locationTile.x + 1, locationTile.y]);
					}
					if (TileExists(locationTile.x - 1, locationTile.y))
					{
						finalTiles.Add(grid[locationTile.x - 1, locationTile.y]);
					}
					finalTiles.Add(locationTile);
					break;
				case Shape.RowWide:
				{
					for (int num55 = 0; num55 < gridLength; num55++)
					{
						for (int num56 = 0; num56 < gridHeight; num56++)
						{
							if (num56 >= locationTile.y - 1 && num56 <= locationTile.y + 1)
							{
								finalTiles.Add(grid[num55, num56]);
							}
						}
					}
					break;
				}
				case Shape.Square:
					finalTiles.Add(grid[locationTile.x + 1, 1]);
					finalTiles.Add(grid[locationTile.x + 2, 1]);
					finalTiles.Add(grid[locationTile.x + 1, 2]);
					finalTiles.Add(grid[locationTile.x + 2, 2]);
					break;
				case Shape.Top:
					finalTiles.Add(grid[locationTile.x, gridHeight - 1]);
					break;
				case Shape.X:
				{
					for (int num46 = 0; num46 < gridHeight; num46++)
					{
						int num47 = locationTile.x - locationTile.y + num46;
						if (num47 >= 0 && num47 < gridLength)
						{
							finalTiles.Add(grid[num47, num46]);
						}
					}
					for (int num48 = gridHeight - 1; num48 >= 0; num48--)
					{
						int num49 = locationTile.x - num48 + locationTile.y;
						if (num49 >= 0 && num49 < gridLength)
						{
							finalTiles.Add(grid[num49, num48]);
						}
					}
					break;
				}
				case Shape.XSmall:
				{
					int[] array7 = new int[5] { 0, 1, 1, -1, -1 };
					int[] array8 = new int[5] { 0, 1, -1, 1, -1 };
					for (int num45 = 0; num45 < array8.Length; num45++)
					{
						if (TileExists(locationTile.x + array7[num45], locationTile.y + array8[num45]))
						{
							finalTiles.Add(grid[locationTile.x + array7[num45], locationTile.y + array8[num45]]);
						}
					}
					break;
				}
				case Shape.ZBig:
				{
					int[] array5 = new int[10] { 0, 1, 2, 3, 2, 1, 0, 1, 2, 3 };
					int[] array6 = new int[10] { 0, 0, 0, 0, -1, -2, -3, -3, -3, -3 };
					for (int num43 = 0; num43 < array6.Length; num43++)
					{
						if (TileExists(locationTile.x + array5[num43], locationTile.y + array6[num43]))
						{
							finalTiles.Add(grid[locationTile.x + array5[num43], locationTile.y + array6[num43]]);
						}
					}
					break;
				}
				default:
					finalTiles = locationTiles;
					break;
				}
			}
		}
		else
		{
			finalTiles = locationTiles;
		}
		finalTiles = ApplyTilePattern(finalTiles, pats, being, seed);
		if (finalTiles.Count > num && num > 0)
		{
			finalTiles.RemoveRange(num, finalTiles.Count - num);
		}
		return finalTiles;
	}

	private List<Tile> ApplyTilePattern(List<Tile> theTiles, List<Pattern> patterns, Being being, bool seed)
	{
		if (patterns == null)
		{
			return theTiles;
		}
		using (List<Pattern>.Enumerator enumerator = patterns.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case Pattern.Unoccupied:
				{
					for (int num6 = theTiles.Count - 1; num6 >= 0; num6--)
					{
						if (theTiles[num6].occupation > 0)
						{
							theTiles.RemoveAt(num6);
						}
					}
					break;
				}
				case Pattern.Occupied:
				{
					for (int num2 = theTiles.Count - 1; num2 >= 0; num2--)
					{
						if (theTiles[num2].occupation < 1)
						{
							theTiles.RemoveAt(num2);
						}
					}
					break;
				}
				case Pattern.PrioritizeOccupied:
					theTiles = theTiles.Where((Tile w) => w.occupation > 0).Union(theTiles.Where((Tile w) => w.occupation <= 0)).ToList();
					break;
				case Pattern.PrioritizeUnoccupied:
					theTiles = theTiles.Where((Tile w) => w.occupation <= 0).Union(theTiles.Where((Tile w) => w.occupation > 0)).ToList();
					break;
				case Pattern.PrioritizeMoveable:
				{
					for (int num4 = theTiles.Count - 1; num4 >= 0; num4--)
					{
						if (!being.mov.GetMoveableTile(theTiles[num4].x, theTiles[num4].y))
						{
							theTiles.Add(theTiles[num4]);
							theTiles.RemoveAt(num4);
						}
					}
					break;
				}
				case Pattern.Moveable:
				{
					for (int num = theTiles.Count - 1; num >= 0; num--)
					{
						if (!being.mov.GetMoveableTile(theTiles[num].x, theTiles[num].y))
						{
							theTiles.RemoveAt(num);
						}
					}
					break;
				}
				case Pattern.Allied:
				{
					if (!(being != null))
					{
						break;
					}
					for (int num7 = theTiles.Count - 1; num7 >= 0; num7--)
					{
						if (theTiles[num7].align != being.alignNum && theTiles[num7].align != 0)
						{
							theTiles.RemoveAt(num7);
						}
					}
					break;
				}
				case Pattern.InverseLocal:
				{
					for (int num5 = theTiles.Count - 1; num5 >= 0; num5--)
					{
						if (theTiles[num5].x < 4)
						{
							theTiles[num5] = grid[Mathf.Abs(theTiles[num5].x - 3), Mathf.Abs(theTiles[num5].y - 3)];
						}
						else
						{
							theTiles[num5] = grid[Mathf.Abs(gridLength + 3 - theTiles[num5].x), Mathf.Abs(theTiles[num5].y - 3)];
						}
					}
					break;
				}
				case Pattern.Unbroken:
				{
					for (int num3 = theTiles.Count - 1; num3 >= 0; num3--)
					{
						if (theTiles[num3].type == TileType.Broken)
						{
							theTiles.RemoveAt(num3);
						}
					}
					break;
				}
				case Pattern.ExcludeLastTargetHit:
					if (theTiles.Contains(lastTargetHitTile))
					{
						theTiles.Remove(lastTargetHitTile);
					}
					break;
				case Pattern.ExcludeSelf:
					if (theTiles.Contains(being.mov.currentTile))
					{
						theTiles.Remove(being.mov.currentTile);
					}
					break;
				case Pattern.Random:
					theTiles = Randomize(theTiles, seed);
					break;
				case Pattern.Reverse:
					theTiles.Reverse();
					break;
				}
			}
		}
		return theTiles;
	}

	private void AddTileToList(List<Tile> tileList, Tile tile)
	{
		if (tile != null)
		{
			tileList.Add(tile);
		}
	}
}
