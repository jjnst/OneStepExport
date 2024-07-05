using TMPro;
using UnityEngine;

public class CastSlot : MonoBehaviour
{
	public SpellObject spellObj;

	public Cardtridge cardtridgeFill;

	public Cardtridge cardtridgeRef;

	public Player player;

	public Animator anim;

	public TMP_Text dmgLabel;

	private bool readyToUse;

	public BC ctrl;

	public DuelDisk duelDisk;

	private float displayedDamage = -9999f;

	public InputIcon inputIcon;

	public InputAction inputAction;

	private void Awake()
	{
		inputIcon.inputAction = inputAction;
	}

	public void TriggerHold()
	{
		if ((bool)cardtridgeFill)
		{
			cardtridgeFill.spellObj.Trigger(FTrigger.Hold);
		}
	}

	public void PUpdate()
	{
		if (!cardtridgeFill)
		{
			return;
		}
		float amount = ctrl.GetAmount(spellObj.manaType, spellObj.mana, spellObj);
		if (player.duelDisk.currentMana < amount)
		{
			readyToUse = false;
			if (cardtridgeFill.imageOverback.fillAmount != 1f || cardtridgeRef.imageOverback.fillAmount != 1f)
			{
				cardtridgeFill.imageOverback.fillAmount = 1f;
				cardtridgeRef.imageOverback.fillAmount = 1f;
			}
			cardtridgeFill.imageOverlay.fillAmount = player.duelDisk.currentMana / amount;
			cardtridgeRef.imageOverlay.fillAmount = player.duelDisk.currentMana / amount;
		}
		else if (player.duelDisk.currentMana >= amount)
		{
			if (!readyToUse)
			{
				cardtridgeRef.anim.SetTrigger("ManaReady");
				readyToUse = true;
			}
			cardtridgeFill.imageOverback.fillAmount = 0f;
			cardtridgeFill.imageOverlay.fillAmount = 0f;
			cardtridgeRef.imageOverback.fillAmount = 0f;
			cardtridgeRef.imageOverlay.fillAmount = 0f;
		}
		float num = ctrl.GetAmount(cardtridgeFill.spellObj.damageType, cardtridgeFill.spellObj.damage, cardtridgeFill.spellObj, null, null, true) + (float)cardtridgeFill.spellObj.tempDamage + (float)cardtridgeFill.spellObj.permDamage;
		if (num > 0f && cardtridgeFill.spellObj.damageType.type != AmountType.Zero && !S.I.RECORD_MODE)
		{
			num += (float)(cardtridgeFill.spellObj.being.player.spellPower + cardtridgeFill.spellObj.being.GetAmount(Status.SpellPower));
			if (displayedDamage != num)
			{
				dmgLabel.text = num.ToString();
			}
			displayedDamage = num;
		}
		else
		{
			ResetDamageDisplayText();
		}
	}

	public void Load(Cardtridge cardtridge)
	{
		if ((bool)cardtridge)
		{
			cardtridge.transform.SetParent(duelDisk.cardtridgeSlotContainer);
			cardtridge.MoveTo(base.transform);
			cardtridge.image.color = Color.white;
			cardtridge.imageBelt.enabled = false;
			spellObj = cardtridge.spellObj;
			cardtridgeFill = cardtridge;
			cardtridge.SetInSlot();
			cardtridgeRef.SetRef(spellObj);
			UpdateGlow();
		}
	}

	public CastSlot SetRef(DuelDisk duelDisk, Transform parent, Cardtridge newCardRef, int num, DiskReference diskRef)
	{
		switch (num)
		{
		case 0:
			inputIcon.inputAction = InputAction.FireOne;
			break;
		case 1:
			inputIcon.inputAction = InputAction.FireTwo;
			break;
		}
		base.transform.SetParent(parent, false);
		this.duelDisk = duelDisk;
		player = duelDisk.player;
		ctrl = duelDisk.ctrl;
		cardtridgeRef = newCardRef;
		inputIcon.UpdateDisplay();
		newCardRef.SetAsRef(diskRef.cardSlotsReferenceGrid);
		return this;
	}

	public void UpdateGlow()
	{
		if (!cardtridgeFill)
		{
			return;
		}
		if ((bool)player.GetStatusEffect(Status.Flow) && player.GetStatusEffect(Status.Flow).amount > 0f && cardtridgeFill.spellObj.flow)
		{
			AddGlowToCardtridges(FTrigger.Flow);
		}
		else
		{
			RemoveGlowFromCardtridges(FTrigger.Flow);
		}
		if ((bool)player.GetStatusEffect(Status.Fragile) && player.GetStatusEffect(Status.Fragile).amount > 0f)
		{
			foreach (EffectApp efApp in cardtridgeFill.spellObj.efApps)
			{
				if (efApp.checks != null && efApp.checks.Contains(Check.Fragile))
				{
					AddGlowToCardtridges(FTrigger.Frantic);
					break;
				}
				RemoveGlowFromCardtridges(FTrigger.Frantic);
			}
		}
		else
		{
			RemoveGlowFromCardtridges(FTrigger.Frantic);
		}
		if ((bool)player.GetStatusEffect(Status.Trinity))
		{
			if ((player.GetStatusEffect(Status.Trinity).amount > 1f && cardtridgeFill.spellObj.HasTrigger(FTrigger.TrinityCast) && cardtridgeFill.spellObj.HasEffect(Effect.Trinity)) || (player.GetStatusEffect(Status.Trinity).amount > 2f && cardtridgeFill.spellObj.HasTrigger(FTrigger.TrinityCast)))
			{
				AddGlowToCardtridges(FTrigger.TrinityCast);
			}
		}
		else
		{
			RemoveGlowFromCardtridges(FTrigger.TrinityCast);
		}
	}

	private void AddGlowToCardtridges(FTrigger fTrigger)
	{
		cardtridgeFill.AddGlow(fTrigger);
		cardtridgeRef.AddGlow(fTrigger);
	}

	private void RemoveGlowFromCardtridges(FTrigger fTrigger)
	{
		cardtridgeFill.RemoveGlow(fTrigger);
		cardtridgeRef.RemoveGlow(fTrigger);
	}

	public void StatusUpdateGlow(Status addedStatusType)
	{
		if (!cardtridgeFill)
		{
			return;
		}
		switch (addedStatusType)
		{
		case Status.Flow:
			if ((bool)player.GetStatusEffect(Status.Flow) && player.GetStatusEffect(Status.Flow).amount > 0f && cardtridgeFill.spellObj.flow)
			{
				AddGlowToCardtridges(FTrigger.Flow);
			}
			else
			{
				RemoveGlowFromCardtridges(FTrigger.Flow);
			}
			break;
		case Status.Fragile:
			if ((bool)player.GetStatusEffect(Status.Fragile) && player.GetStatusEffect(Status.Fragile).amount > 0f)
			{
				foreach (EffectApp efApp in cardtridgeFill.spellObj.efApps)
				{
					if (efApp.checks != null && efApp.checks.Contains(Check.Fragile))
					{
						AddGlowToCardtridges(FTrigger.Frantic);
						break;
					}
					RemoveGlowFromCardtridges(FTrigger.Frantic);
				}
				break;
			}
			RemoveGlowFromCardtridges(FTrigger.Frantic);
			break;
		case Status.Trinity:
			if ((bool)player.GetStatusEffect(Status.Trinity))
			{
				if ((player.GetStatusEffect(Status.Trinity).amount > 1f && cardtridgeFill.spellObj.HasTrigger(FTrigger.TrinityCast) && cardtridgeFill.spellObj.HasEffect(Effect.Trinity)) || (player.GetStatusEffect(Status.Trinity).amount > 2f && cardtridgeFill.spellObj.HasTrigger(FTrigger.TrinityCast)))
				{
					AddGlowToCardtridges(FTrigger.TrinityCast);
				}
			}
			else
			{
				RemoveGlowFromCardtridges(FTrigger.TrinityCast);
			}
			break;
		}
	}

	public void Empty()
	{
		if ((bool)cardtridgeFill)
		{
			cardtridgeFill.transform.SetParent(duelDisk.transform);
			cardtridgeFill.imageOverlay.fillAmount = 0f;
			cardtridgeFill.imageOverback.fillAmount = 0f;
			cardtridgeFill.RemoveAllGlows();
			if (S.I.LABELS)
			{
				cardtridgeFill.label.text = string.Empty;
			}
		}
		if ((bool)cardtridgeRef)
		{
			cardtridgeRef.imageOverlay.fillAmount = 0f;
			cardtridgeRef.imageOverback.fillAmount = 0f;
			cardtridgeRef.image.fillAmount = 0f;
			cardtridgeRef.RemoveAllGlows();
			if (S.I.LABELS)
			{
				cardtridgeRef.label.text = string.Empty;
			}
		}
		ResetDamageDisplayText();
		spellObj = null;
		cardtridgeFill = null;
	}

	private void ResetDamageDisplayText()
	{
		dmgLabel.text = string.Empty;
		displayedDamage = -9999f;
	}

	public void Launch(bool forceConsume, FloatingText spellText = null)
	{
		if (spellObj == null)
		{
			return;
		}
		Cardtridge cardtridge = cardtridgeFill;
		if ((bool)cardtridgeFill)
		{
			cardtridgeFill.Eject();
			if ((bool)spellText)
			{
				spellText.anim.SetTrigger("show");
			}
			if ((cardtridgeFill.spellObj.consume && duelDisk.shufflesThisBattle > 0) || forceConsume)
			{
				duelDisk.deCtrl.deckScreen.dirty = true;
				cardtridgeFill.anim.SetTrigger("Consume");
				anim.SetTrigger("consume");
				if ((bool)spellText)
				{
					spellText.anim.SetTrigger("Consume");
				}
				duelDisk.currentCardtridges.Remove(cardtridgeFill);
				S.I.PlayOnce(ctrl.itemMan.GetAudioClip("poof_smoke_rev"));
				if ((bool)player)
				{
					player.TriggerArtifacts(FTrigger.OnConsume);
					player.consumedSpells++;
				}
			}
			else
			{
				anim.SetTrigger("eject");
			}
			cardtridgeRef.anim.SetTrigger("RefLoad");
		}
		Empty();
		if (duelDisk.queuedCardtridges.Contains(cardtridge))
		{
			cardtridge.Reset(duelDisk.cardGrid.transform, duelDisk.cardGrid.transform.position + new Vector3(0f, cardtridge.height * (duelDisk.clipSize - duelDisk.queuedCardtridges.Count + duelDisk.queuedCardtridges.IndexOf(cardtridge)), 0f));
		}
	}
}
