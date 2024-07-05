using System.Collections;
using I2.Loc;
using MoonSharp.Interpreter;
using Sirenix.OdinInspector;
using UnityEngine;

[MoonSharpUserData]
public class Item : SerializedMonoBehaviour
{
	public int rarity = 0;

	public ItemType itemType;

	public Brand brand = Brand.None;

	public ItemObject itemObj;

	[HideInInspector]
	public BC ctrl;

	[HideInInspector]
	public SpawnCtrl sp;

	[HideInInspector]
	public Being being;

	protected virtual void Awake()
	{
		ctrl = S.I.batCtrl;
		sp = S.I.spCtrl;
	}

	public void StartEffectRoutine(DynValue result)
	{
		if ((bool)being && being.isActiveAndEnabled && (bool)base.gameObject)
		{
			StartCoroutine(EffectRoutine(result));
		}
	}

	protected virtual IEnumerator EffectRoutine(DynValue result)
	{
		yield return null;
	}

	public void StopEverything()
	{
		StopAllCoroutines();
	}

	public void SummonAfter(ItemObject itemObj, Tile tile, float delay, EffectApp savedApp, bool pet = false)
	{
		if (!ctrl.blockAllSummons)
		{
			StartCoroutine(SummonAfterC(itemObj, tile, delay, savedApp, pet));
		}
	}

	private IEnumerator SummonAfterC(ItemObject itemObj, Tile tile, float delay, EffectApp savedApp, bool pet)
	{
		float timer = 0f;
		while (timer < delay)
		{
			timer = TickTimer(timer);
			yield return null;
		}
		yield return null;
		if (ctrl.blockAllSummons && !pet)
		{
			yield break;
		}
		if (tile.vacant && tile.type != 0 && tile.type != TileType.Broken)
		{
			if (!pet || (itemObj.artObj != null && !itemObj.artObj.dead))
			{
				Being newBeing = itemObj.spCtrl.PlaceBeing(savedApp.value, tile, 0, false, itemObj.being.battleGrid);
				newBeing.parentObj = itemObj;
				newBeing.transform.rotation = itemObj.being.transform.rotation;
				newBeing.health.SetHealth(Mathf.FloorToInt(ctrl.GetAmount(savedApp.amountApp, savedApp.amount, itemObj.spellObj, itemObj.artObj)));
				newBeing.minion = true;
				string enemyName = newBeing.beingObj.beingID;
				string enemyToSpawnEnd = enemyName.Substring(enemyName.Length - 1);
				int originalTier = 1;
				if (int.TryParse(enemyToSpawnEnd, out originalTier))
				{
					enemyName.Remove(enemyName.Length - 1);
				}
				else
				{
					originalTier = 1;
				}
				if (originalTier - 2 >= 0)
				{
					newBeing.defaultShader = sp.tierShaders[originalTier - 2];
				}
				if (pet)
				{
					itemObj.artObj.dead = true;
					newBeing.mov.currentTile.SetOccupation(0, newBeing.mov.hovering);
					itemObj.being.currentPets.Add(newBeing.GetComponent<Cpu>());
					newBeing.GetComponent<Ally>().owner = itemObj.being;
					newBeing.mov.neverOccupy = true;
					newBeing.SetAlignNum(itemObj.being.alignNum);
					newBeing.mov.hovering = true;
					newBeing.pet = true;
					foreach (Player player in ctrl.currentPlayers)
					{
						if (player.petSnap)
						{
							newBeing.beingStatsPanel.healthDisplayCanvasGroup.alpha = 0f;
						}
					}
					if (itemObj.artObj.currentValue > 0f)
					{
						newBeing.health.SetHealth(Mathf.RoundToInt(itemObj.artObj.currentValue), Mathf.RoundToInt(itemObj.artObj.maxValue));
					}
				}
				else
				{
					being.TriggerArtifacts(FTrigger.OnSummon);
				}
			}
		}
		else
		{
			being.CreateFloatText(ctrl.statusTextPrefab, string.Format(ScriptLocalization.UI.Summon_tile_blocked), -20, 65, 0.5f);
		}
		yield return null;
	}

	public float TickTimer(float timer)
	{
		if ((bool)being.player && !itemObj.alwaysUseGameTime)
		{
			return timer += BC.playerChronoTime;
		}
		return timer += Time.deltaTime;
	}
}
