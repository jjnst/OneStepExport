using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;

[MoonSharpUserData]
public class Health : MonoBehaviour
{
	public delegate void OnSetMax();

	public int starting = 100;

	public int current;

	public int max = 100;

	public int shield;

	private int displayedShield;

	public TMP_Text text;

	public List<FloatingTextContainer> currentTexts = new List<FloatingTextContainer>();

	public bool deathTriggered = false;

	private int displayedHealth;

	private Being being;

	private float displayIterationRate = 0.13f;

	private bool tookDamage = false;

	private bool healedDamage = false;

	private float aT = 1f;

	private float textTimer = 0f;

	private float healthTextIterationVelocity = 0f;

	private float shieldTextIterationVelocity = 0f;

	private Color damagedColor;

	private int yAddOffset;

	private float waitTime;

	public int lastDisplayedHealth = 0;

	public int lastDisplayedShield = 0;

	public event OnSetMax onSetMax;

	private void Awake()
	{
		damagedColor = U.I.GetColor(UIColor.RedLight);
		being = GetComponent<Being>();
		if (max < starting)
		{
			max = starting;
		}
		current = starting;
		displayedHealth = current;
	}

	private void Start()
	{
		UpdateHealthText();
	}

	private void FixedUpdate()
	{
		if (max < current)
		{
			current = max;
		}
		if (tookDamage)
		{
			text.color = Color.Lerp(Color.red, Color.white, aT);
			if (current < 100 && current != max)
			{
				text.color = Color.Lerp(Color.red, damagedColor, aT);
			}
		}
		else if (healedDamage)
		{
			text.color = Color.Lerp(Color.green, Color.white, aT);
			if (current < 100 && current != max)
			{
				text.color = Color.Lerp(Color.green, damagedColor, aT);
			}
		}
		if (aT < 1f)
		{
			aT += Time.deltaTime / 1f;
		}
		else
		{
			tookDamage = false;
			healedDamage = false;
		}
		IterateText();
	}

	private void RestartIteration()
	{
		healthTextIterationVelocity = 0f;
		shieldTextIterationVelocity = 0f;
	}

	private void IterateText()
	{
		if ((displayedShield > shield && shield > 0) || displayedShield < shield)
		{
			CalculateDisplayedNum(ref displayedShield, shield, ref shieldTextIterationVelocity);
		}
		else
		{
			displayedShield = shield;
		}
		if (displayedHealth != current)
		{
			CalculateDisplayedNum(ref displayedHealth, current, ref healthTextIterationVelocity);
		}
		else
		{
			displayedHealth = current;
		}
		UpdateHealthText();
	}

	public void SetShield(int amount)
	{
		if (shield > 0 && amount <= 0)
		{
			being.CreateHitFX(Status.Shield, true);
		}
		shield = amount;
		shield = Mathf.Clamp(shield, 0, shield);
		RestartIteration();
		if (shield > 9999 && being.battleGrid != being.ctrl.ti.refBattleGrid)
		{
			AchievementsCtrl.UnlockAchievement("The_Best_Defense");
			S.AddSkinUnlock("RevaCyber");
		}
	}

	public void ModifyShield(int amount)
	{
		shield += amount;
		shield = Mathf.Clamp(shield, 0, shield);
		RestartIteration();
		if (shield > 9999 && being.battleGrid != being.ctrl.ti.refBattleGrid)
		{
			AchievementsCtrl.UnlockAchievement("The_Best_Defense");
			S.AddSkinUnlock("RevaCyber");
		}
	}

	public void ModifyHealth(int amount, bool pierceShield = false)
	{
		if (!being)
		{
			return;
		}
		aT = 0f;
		int num = amount;
		int num2 = amount + shield;
		if (!pierceShield && shield > 0 && amount < 0)
		{
			if (num2 <= 0)
			{
				being.CreateHitFX(Status.Shield, true);
			}
			if (num2 >= 0)
			{
				CreateDmgText(num, TextType.Shield);
				ModifyShield(num);
				amount = 0;
				aT = 0f;
				RestartIteration();
				return;
			}
			amount = num2;
			SetShield(0);
			CreateDmgText(num - num2, TextType.Shield);
			SetShield(Mathf.Clamp(shield, 0, shield));
		}
		current += amount;
		if (current <= 0)
		{
			tookDamage = true;
			CreateDmgText(amount, TextType.Damage);
			TriggerDeath();
		}
		else if (amount <= 0)
		{
			tookDamage = true;
			healedDamage = false;
			CreateDmgText(amount, TextType.Damage);
		}
		else if (amount >= 0)
		{
			healedDamage = true;
			tookDamage = false;
			CreateDmgText(amount, TextType.Heal);
		}
		current = Mathf.Clamp(current, 0, max);
		RestartIteration();
	}

	public void TriggerDeath()
	{
		if (!deathTriggered)
		{
			deathTriggered = true;
			being.StartDeath();
		}
	}

	public void UpdateHealthText()
	{
		if (lastDisplayedHealth != displayedHealth)
		{
			lastDisplayedHealth = displayedHealth;
			text.text = displayedHealth.ToString();
			if (shield > 0)
			{
				lastDisplayedShield = -1;
			}
		}
		if (lastDisplayedShield != displayedShield)
		{
			if (shield > 0)
			{
				lastDisplayedShield = displayedShield;
				text.text = displayedHealth + " <color=#82a2fe>(" + displayedShield + ")";
			}
			else
			{
				text.text = displayedHealth.ToString();
			}
		}
	}

	public void SetMax(int newMaxHealth)
	{
		max = newMaxHealth;
		Mathf.Clamp(max, 1, max);
		if (current > max)
		{
			ModifyHealth(-(current - max), true);
		}
		RaiseOnSetMax();
	}

	private void RaiseOnSetMax()
	{
		if (this.onSetMax != null)
		{
			this.onSetMax();
		}
	}

	public void SetHealth(int newHealth, int maxHealth = -1)
	{
		current = newHealth;
		if (maxHealth == -1)
		{
			max = newHealth;
		}
		else
		{
			max = maxHealth;
		}
		displayedHealth = newHealth;
		UpdateHealthText();
		if (newHealth < 1 || max < 1)
		{
			TriggerDeath();
		}
	}

	public void SetHealth(string newHealth, string maxHealth = "-1")
	{
		SetHealth(int.Parse(newHealth), int.Parse(maxHealth));
	}

	private void CalculateDisplayedNum(ref int displayVal, float currentVal, ref float currentVelocity)
	{
		displayVal = Mathf.FloorToInt(Mathf.SmoothDamp(displayVal, currentVal, ref currentVelocity, displayIterationRate));
		if (Mathf.Abs((float)displayVal - currentVal) < 10f)
		{
			if ((float)displayVal > currentVal)
			{
				displayVal--;
			}
			else if ((float)displayVal < currentVal)
			{
				displayVal++;
			}
		}
	}

	private void CreateDmgText(int amount, TextType type)
	{
		if (amount < 0)
		{
			amount *= -1;
		}
		yAddOffset = 35;
		waitTime = 0f;
		if (Time.time > textTimer + 1f)
		{
			textTimer = Time.time;
			currentTexts.Clear();
		}
		for (int i = 0; i < currentTexts.Count; i++)
		{
			if (currentTexts[i] != null)
			{
				yAddOffset += 10;
				waitTime += 0.05f;
				if (yAddOffset > 135)
				{
					yAddOffset -= 135;
					waitTime -= 0.4f;
				}
			}
		}
		if ((bool)being)
		{
			currentTexts.Add(being.CreateFloatText(being.ctrl.dmgTextPrefab, amount.ToString(), -20, 45).SetHealth(yAddOffset, waitTime, type).container);
		}
	}

	public IEnumerator ClearFromTextList(FloatingTextContainer textContainer, float time)
	{
		yield return new WaitForSeconds(time);
		currentTexts.Remove(textContainer);
	}
}
