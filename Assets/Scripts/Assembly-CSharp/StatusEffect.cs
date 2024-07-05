using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[MoonSharpUserData]
public class StatusEffect : MonoBehaviour
{
	public Status status;

	public float duration;

	public float maxDuration;

	public int creationFrame;

	public Image icon;

	public Image iconBackground;

	public float amount;

	public TMP_Text amountText;

	public StatusDisplay display;

	public List<StatusStack> statusStacks = new List<StatusStack>();

	private void OnDisable()
	{
		if ((bool)amountText)
		{
			amountText.text = string.Empty;
		}
		amount = 0f;
	}

	public StatusEffect Set(Being being, Status statusType, float duration, float amount, StatusDisplay display, ItemObject source)
	{
		string text = statusType.ToString();
		status = statusType;
		this.duration = duration;
		maxDuration = duration;
		creationFrame = Time.frameCount;
		this.amount = amount;
		icon.sprite = being.ctrl.effectSpritesDict[text];
		iconBackground.sprite = icon.sprite;
		this.display = display;
		if (duration <= 0f)
		{
			icon.enabled = false;
			iconBackground.enabled = false;
		}
		else
		{
			icon.enabled = true;
			iconBackground.enabled = true;
		}
		base.transform.SetParent(being.beingStatsPanel.statusEffectsBox, false);
		base.transform.name = "StatIcon - " + text;
		if (being.statusesRemovedThisFramePos.ContainsKey(statusType))
		{
			base.transform.SetSiblingIndex(being.statusesRemovedThisFramePos[statusType]);
		}
		statusStacks.Clear();
		statusStacks.Add(new StatusStack(amount, duration, duration, source));
		if (amount != 0f)
		{
			SetText(amount);
		}
		return this;
	}

	public void SetText(float amount)
	{
		amountText.text = Math.Round(amount, 1).ToString();
		if (amount < 0f)
		{
			amountText.text = U.I.Colorify(amountText.text, UIColor.RedLight);
		}
		else if (amount == 0f)
		{
			amountText.text = string.Empty;
		}
	}
}
