using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class StatsScreen : MonoBehaviour
{
	public SlideBody slideBody;

	public StatUI manaRegen;

	public StatUI spellPower;

	public StatUI basicDamage;

	public StatUI defense;

	public StatUI luck;

	public StatUI avgManaCost;

	public int consoleHealthChange;

	public int newArts = 0;

	public float manaRegenDeckSizeBonus = 0.1f;

	public AdjustingCursor artCursor;

	private List<ArtifactObject> artObjList = new List<ArtifactObject>();

	private int displayedHealthAmount = 0;

	private int displayedHealthMaxAmount = 0;

	private BC ctrl;

	private DeckCtrl deCtrl;

	private PostCtrl poCtrl;

	private string manaPerSecondLocalized = "";

	private void Start()
	{
		ctrl = S.I.batCtrl;
		deCtrl = S.I.deCtrl;
		poCtrl = S.I.poCtrl;
		slideBody = GetComponent<SlideBody>();
	}

	public void UpdateStats(Player player = null)
	{
		if (player == null)
		{
			player = ctrl.currentPlayer;
		}
		if (player == null)
		{
			return;
		}
		int num = 0;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		float num7 = 0f;
		float num8 = 0f;
		int num9 = 0;
		int num10 = 0;
		float num11 = 0f;
		float num12 = 0f;
		float num13 = 0f;
		artObjList.Clear();
		foreach (ArtifactObject artObj in player.artObjs)
		{
			artObjList.Add(artObj);
		}
		if (ctrl.currentPlayers.Contains(player))
		{
			foreach (PactObject activePact in deCtrl.activePacts)
			{
				artObjList.Add(activePact);
			}
		}
		foreach (ArtifactObject artObj2 in artObjList)
		{
			num += artObj2.maxMana;
			num2 += (float)artObj2.startingManaChange;
			num3 += artObj2.manaRegen;
			num4 += artObj2.atkDmg;
			num5 += artObj2.spellPower;
			num6 += artObj2.defense;
			num7 += artObj2.invulLength;
			num8 += artObj2.shuffleTime;
			num10 += artObj2.lootDropsAdd;
			num11 += artObj2.dropMultiplierAdd;
			num12 += artObj2.chestSpawnRateAdd;
			num13 += artObj2.healSpawnRateAdd;
		}
		if ((bool)player.GetStatusEffect(Status.ManaRegen))
		{
			num3 += player.GetStatusEffect(Status.ManaRegen).amount;
		}
		if ((bool)player.GetStatusEffect(Status.MaxMana))
		{
			num += Mathf.RoundToInt(player.GetStatusEffect(Status.MaxMana).amount);
		}
		BeingObject baseBeingObj = player.baseBeingObj;
		if (baseBeingObj != null)
		{
			player.maxMana = Mathf.Clamp(baseBeingObj.maxMana + num, 0, 99);
			player.startingMana = num2;
			player.manaRegen = baseBeingObj.manaRegen + num3 + (float)Mathf.Clamp(Mathf.FloorToInt((float)player.duelDisk.deck.Count / 5f), 0, deCtrl.deckRegenLimit) * manaRegenDeckSizeBonus;
			player.atkDmg = num4;
			player.spellPower = baseBeingObj.spellPower + num5;
			player.beingObj.defense = baseBeingObj.defense + num6;
			player.duelDisk.shuffleTime = Mathf.Clamp(baseBeingObj.shuffleTime + num8, 0.3f, float.PositiveInfinity);
			deCtrl.maxDeckSize += num9;
			poCtrl.addAmountOfLootDrops = num10;
			ctrl.dropMultiplierAdd = num11;
			ctrl.sp.lootChestChanceAdd = num12;
			ctrl.sp.healChestChanceAdd = num13;
		}
		player.duelDisk.manaBar.UpdateLines(Mathf.Clamp(player.maxMana, 0, player.maxMana));
		if (player.battleGrid != ctrl.ti.mainBattleGrid)
		{
			return;
		}
		float num14 = player.startingMana + num2;
		if (num14 > (float)player.maxMana)
		{
			num14 = player.maxMana;
		}
		if (displayedHealthAmount != player.health.current || displayedHealthMaxAmount != player.health.max)
		{
			ctrl.idCtrl.idleHealthText.text = string.Format("{0}/{1}", player.health.current, player.health.max);
			displayedHealthAmount = player.health.current;
			displayedHealthMaxAmount = player.health.max;
		}
		if (player.maxMana < 1)
		{
			if (!player.IsReference() && !AchievementsCtrl.IsUnlocked("Absolute_Zero"))
			{
				AchievementsCtrl.UnlockAchievement("Absolute_Zero");
			}
			S.AddSkinUnlock("HazelPriestess");
		}
	}

	public void UpdateStatsText(Player player = null)
	{
		if (player == null)
		{
			player = ctrl.currentPlayer;
		}
		if (player == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(manaPerSecondLocalized))
		{
			manaPerSecondLocalized = ScriptLocalization.UI.mana_per_second;
		}
		string arg = "+";
		if (player.manaRegen <= 0f)
		{
			arg = "";
		}
		manaRegen.UpdateStatUI(string.Format("{2}{0} {1}", Mathf.Round(player.manaRegen * 10f) / 10f, manaPerSecondLocalized, arg));
		string arg2 = "+";
		if (player.atkDmg <= 0)
		{
			arg2 = "";
		}
		basicDamage.UpdateStatUI(string.Format("{1}{0}", player.atkDmg, arg2));
		string arg3 = "+";
		if (player.spellPower <= 0)
		{
			arg3 = "";
		}
		spellPower.UpdateStatUI(string.Format("{1}{0}", player.spellPower, arg3));
		defense.UpdateStatUI(string.Format("{0}", player.beingObj.defense));
		luck.UpdateStatUI(string.Format("{0}", Mathf.RoundToInt(poCtrl.CombinedLuck())));
		int count = player.duelDisk.deck.Count;
		float num = 0f;
		foreach (ListCard item in player.duelDisk.deck)
		{
			num += item.spellObj.mana;
		}
		avgManaCost.UpdateStatUI((num / (float)count).ToString("F2"));
	}

	public void AnimateArts()
	{
		StartCoroutine(AnimateArtsC());
	}

	private IEnumerator AnimateArtsC()
	{
		yield return new WaitForSeconds(0.15f);
		if (slideBody.onScreen)
		{
			foreach (Transform child2 in deCtrl.artGrid)
			{
				yield return new WaitForSeconds(0.03f);
				child2.GetComponent<Animator>().SetBool("visible", true);
			}
			yield break;
		}
		foreach (Transform child in deCtrl.artGrid)
		{
			child.GetComponent<Animator>().SetBool("visible", false);
		}
	}
}
