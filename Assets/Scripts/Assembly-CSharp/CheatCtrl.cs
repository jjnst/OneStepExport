using UnityEngine;

public class CheatCtrl : NavPanel
{
	private float savedTimeScale = 0f;

	public SliderButton spellSlider;

	private bool invinceOn = false;

	private void Start()
	{
		spellSlider.slider.value = 1f;
	}

	public override void Open()
	{
		base.Open();
		savedTimeScale = Time.timeScale;
		Time.timeScale = 0f;
		spellSlider.slider.maxValue = S.I.itemMan.playerFullSpellList.Count;
		S.I.batCtrl.AddControlBlocks(Block.Cheat);
	}

	public void ClickHP(int amount)
	{
		foreach (Player currentPlayer in S.I.batCtrl.currentPlayers)
		{
			currentPlayer.health.SetHealth(amount, amount);
		}
	}

	public void ClickInvince()
	{
		invinceOn = !invinceOn;
		foreach (Player currentPlayer in S.I.batCtrl.currentPlayers)
		{
			currentPlayer.AddInvince(invinceOn ? 9999 : (-9999));
		}
	}

	public void ClickMoney(int amount)
	{
		S.I.shopCtrl.sera = amount;
	}

	public void ClickDeathGun(int weaponNum)
	{
		foreach (Player currentPlayer in S.I.batCtrl.currentPlayers)
		{
			if (weaponNum == 0)
			{
				S.I.deCtrl.EquipWep("GodSniper");
			}
			else
			{
				S.I.deCtrl.EquipWep("GodGun");
			}
		}
	}

	public void MoveSpellSlider()
	{
		if (spellSlider.slider.value == -1f)
		{
			spellSlider.slider.value = spellSlider.slider.maxValue - 1f;
		}
		else if (spellSlider.slider.value == spellSlider.slider.maxValue)
		{
			spellSlider.slider.value = 0f;
		}
		spellSlider.tmpText.text = string.Format("Add #{0}: {1}", spellSlider.slider.value, S.I.itemMan.playerFullSpellList[Mathf.RoundToInt(spellSlider.slider.value)].itemID);
	}

	public void AddSpell()
	{
		foreach (Player currentPlayer in S.I.batCtrl.currentPlayers)
		{
			S.I.batCtrl.currentPlayer.duelDisk.AddLiveSpell(null, S.I.itemMan.playerFullSpellList[Mathf.RoundToInt(spellSlider.slider.value)].itemID, S.I.batCtrl.currentPlayer, false, false);
		}
	}

	public void UnlockLvl(int unlockLevel)
	{
		SaveDataCtrl.Set("UnlockLevel", unlockLevel);
		S.I.unCtrl.UnlockAll();
	}

	public void RestartMainMenu()
	{
		S.I.scene = GScene.MainMenu;
		S.I.testZoneType = ZoneType.Battle;
		Restart();
	}

	public void EndingGoodTrue()
	{
		S.I.scene = GScene.Victory;
		Restart();
	}

	public void EndingGoodFalse()
	{
		S.I.scene = GScene.VictoryFalse;
		Restart();
	}

	public void EndingEvil()
	{
		S.I.scene = GScene.VictoryEvil;
		Restart();
	}

	public void EndingGate()
	{
		S.I.scene = GScene.TestZone;
		S.I.testZoneType = ZoneType.Normal;
		Restart();
	}

	public void EndingLoop()
	{
		S.I.scene = GScene.TestZone;
		S.I.testZoneType = ZoneType.Genocide;
		Restart();
	}

	public override void Close()
	{
		base.Close();
		Time.timeScale = savedTimeScale;
		foreach (Player currentPlayer in S.I.batCtrl.currentPlayers)
		{
			currentPlayer.ClearQueuedActions();
		}
		S.I.batCtrl.RemoveControlBlocks(Block.Cheat);
	}

	private void Restart()
	{
		S.I.mainCtrl.ForceClose();
		S.I.batCtrl.Restart(true, true);
		btnCtrl.RemoveFocus();
		S.I.runCtrl.StartRefresh();
	}
}
