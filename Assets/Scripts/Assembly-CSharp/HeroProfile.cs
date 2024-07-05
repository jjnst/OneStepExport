using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class HeroProfile : MonoBehaviour
{
	public string heroName;

	public string description;

	public Sprite splashSprite;

	public int difficulty;

	public bool unlocked = false;

	public RuntimeAnimatorController animController;

	public int money;

	public int numCastSlots;

	public int maxHealth;

	public int startingHealth;

	public int maxMana;

	public float manaRegen;

	public int spellPower;

	public float baseShuffleTime;

	public float basicCooldown;

	public float baseInvulLength;

	public int baseDefense;

	public string equippedWep;

	public List<string> startingSpells;

	public List<string> startingArtifacts;
}
