using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Sirenix.OdinInspector;
using UnityEngine;

[MoonSharpUserData]
public class TI : SerializedMonoBehaviour
{
	public GameObject tile;

	public BattleGrid mainBattleGrid;

	public BattleGrid refBattleGrid;

	[NonSerialized]
	public static string[,] fieldStringGrid = new string[8, 4];

	public static int tileHeight = 25;

	public static int tileWidth = 40;

	public int tileCrackChanceInverse = 40;

	public int fieldNum = 0;

	public BC ctrl;

	public ReferenceCtrl refCtrl;

	public SpawnCtrl sp;

	public Overlay darknessOverlayPrefab;

	private void Awake()
	{
	}

	public void CreateMainField(int newBlocksNum = -1, int newCornersNum = -1)
	{
		StartCoroutine(_CreateMainField());
	}

	private IEnumerator _CreateMainField(int newBlocksNum = -1, int newCornersNum = -1)
	{
		List<Being> persistentBeings = new List<Being>();
		persistentBeings.AddRange(ctrl.currentPlayers);
		if (mainBattleGrid == null)
		{
			mainBattleGrid = new BattleGrid(8, 4, ctrl);
			mainBattleGrid.gridContainer = new GameObject("Tilefield");
			mainBattleGrid.gridContainer.transform.position = base.transform.position;
			mainBattleGrid.gridContainer.transform.parent = base.transform;
			mainBattleGrid.darknessOverlay = UnityEngine.Object.Instantiate(darknessOverlayPrefab, base.transform.position, base.transform.rotation, base.transform);
			int fieldHeight = -18;
			mainBattleGrid.gridContainer.transform.position = new Vector3(-20 * (mainBattleGrid.gridLength - 1), base.transform.position.y + (float)(fieldHeight * (mainBattleGrid.gridHeight - 1 + 2)) + 25f, mainBattleGrid.gridContainer.transform.position.z);
			CreateTileField(mainBattleGrid);
		}
		else
		{
			ResetField(mainBattleGrid, true);
		}
		ImpactTileField(mainBattleGrid, persistentBeings);
		foreach (Being being in persistentBeings)
		{
			being.mov.SetTile(being.mov.currentTile);
		}
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(mainBattleGrid.AnimateField());
	}

	public void CreateRefField()
	{
		if (refBattleGrid == null)
		{
			refBattleGrid = new BattleGrid(8, 4, ctrl);
			refBattleGrid.gridContainer = new GameObject("RefTilefield");
			refBattleGrid.gridContainer.transform.position = Vector3.up * (refCtrl.transform.position.y - 65f) + Vector3.left * 140f;
			refBattleGrid.gridContainer.transform.parent = base.transform;
			refBattleGrid.darknessOverlay = UnityEngine.Object.Instantiate(darknessOverlayPrefab, refCtrl.transform.position, base.transform.rotation, base.transform);
			refBattleGrid.inBattle = true;
			CreateTileField(refBattleGrid);
		}
		else
		{
			ResetField(refBattleGrid, false);
		}
		StartCoroutine(refBattleGrid.AnimateField());
	}

	public void CreateTileField(BattleGrid battleGrid)
	{
		for (int i = 0; i < battleGrid.gridLength; i++)
		{
			for (int j = 0; j < battleGrid.gridHeight; j++)
			{
				if (battleGrid.grid[i, j] == null)
				{
					battleGrid.grid[i, j] = SimplePool.Spawn(tile, battleGrid.gridContainer.transform.position + new Vector3(i * 40, j * 25, j), base.transform.rotation).GetComponent<Tile>().Set(battleGrid, i, j, false);
				}
			}
		}
	}

	public void ImpactTileField(BattleGrid battleGrid, List<Being> persistentBeings)
	{
		int num = 0;
		for (int i = 0; i < battleGrid.gridLength; i++)
		{
			for (int j = 0; j < battleGrid.gridHeight; j++)
			{
				foreach (Being persistentBeing in persistentBeings)
				{
					if (persistentBeing.mov.currentTile.x == i && persistentBeing.mov.currentTile.y != j)
					{
					}
				}
				if (ctrl.runCtrl.currentZoneDot.type == ZoneType.Boss || ctrl.runCtrl.currentZoneDot.type == ZoneType.Treasure)
				{
					return;
				}
				if (num < 3 && Utils.RandomBool(tileCrackChanceInverse))
				{
					battleGrid.grid[i, j].Crack(10f);
					num++;
				}
			}
		}
	}

	public void ResetField(BattleGrid battleGrid, bool fadeOut)
	{
		for (int i = 0; i < battleGrid.gridLength; i++)
		{
			for (int j = 0; j < battleGrid.gridHeight; j++)
			{
				battleGrid.grid[i, j].Reset();
			}
		}
		if (fadeOut)
		{
			StartCoroutine(battleGrid.FadeOutField());
		}
	}
}
