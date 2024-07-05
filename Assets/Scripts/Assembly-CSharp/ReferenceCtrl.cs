using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceCtrl : MonoBehaviour
{
	[NonSerialized]
	public BC ctrl;

	[NonSerialized]
	public DeckCtrl deCtrl;

	[NonSerialized]
	public SpawnCtrl sp;

	[NonSerialized]
	public TI ti;

	public Camera refCam;

	public CanvasGroup referenceDisplay;

	public RectTransform referenceDisplayImage;

	public Canvas canvas;

	private int defaultSortingOrder = 5;

	private int heroSelectSortingOrder = 7;

	private int canvasMainSortingOrder = 11;

	public Canvas canvasMain;

	public Player genPlayer;

	private DuelDisk genDuelDisk;

	private List<Being> exampleBeings = new List<Being>();

	public bool autoPreview = false;

	private float targetAlpha = 0f;

	private float velocity = 0f;

	private Coroutine castExampleSpellCo;

	public bool onScreen = false;

	private void Awake()
	{
		ctrl = S.I.batCtrl;
		deCtrl = S.I.deCtrl;
		sp = S.I.spCtrl;
		ti = S.I.tiCtrl;
		ti.CreateRefField();
		defaultSortingOrder = canvas.sortingOrder;
		heroSelectSortingOrder = ctrl.heCtrl.canvas.sortingOrder + 1;
		canvasMainSortingOrder = canvasMain.sortingOrder + 1;
		autoPreview = true;
	}

	private void Update()
	{
		if (Mathf.Abs(referenceDisplay.alpha - targetAlpha) > 0.01f)
		{
			referenceDisplay.alpha = Mathf.SmoothDamp(referenceDisplay.alpha, targetAlpha, ref velocity, 0.1f, 999f, Time.unscaledDeltaTime);
		}
	}

	public void Hide()
	{
		if (onScreen)
		{
			targetAlpha = 0f;
			ClearReferenceField();
			onScreen = false;
			if (castExampleSpellCo != null)
			{
				StopCoroutine(castExampleSpellCo);
			}
		}
	}

	private void ClearReferenceField()
	{
		if (ti != null)
		{
			ti.refBattleGrid.ClearField(false, true);
		}
		if ((bool)genPlayer)
		{
			if (genPlayer.battleGrid.currentBeings.Contains(genPlayer))
			{
				genPlayer.battleGrid.currentBeings.Remove(genPlayer);
			}
			genPlayer.StopSelfAndChildCoroutines();
			if ((bool)genPlayer.duelDisk)
			{
				UnityEngine.Object.Destroy(genPlayer.duelDisk.gameObject);
			}
		}
		if ((bool)genDuelDisk)
		{
			UnityEngine.Object.Destroy(genDuelDisk.gameObject);
		}
		while (exampleBeings.Count > 0)
		{
			if ((bool)exampleBeings[exampleBeings.Count - 1])
			{
				exampleBeings[exampleBeings.Count - 1].FullClean();
			}
			exampleBeings.RemoveAt(exampleBeings.Count - 1);
		}
	}

	public void DisplaySpell(SpellObject spellObj, CardInner cardInner)
	{
		if (ctrl.GameState == GState.HeroSelect)
		{
			canvas.sortingOrder = heroSelectSortingOrder;
		}
		else if (ctrl.GameState == GState.MainMenu)
		{
			canvas.sortingOrder = canvasMainSortingOrder;
		}
		else
		{
			canvas.sortingOrder = defaultSortingOrder;
		}
		ti.refBattleGrid.ClearField(false, true);
		referenceDisplay.transform.position = (cardInner.transform.position + Vector3.up * cardInner.rect.sizeDelta.y / 2f) * cardInner.transform.lossyScale.y;
		if (referenceDisplay.transform.position.y > 70f + ctrl.camCtrl.transform.position.y)
		{
			if (cardInner.transform.position.y > 46f)
			{
				referenceDisplay.transform.position = cardInner.transform.position + Vector3.right * (referenceDisplayImage.sizeDelta.x / 2f + cardInner.rect.sizeDelta.x / 2f);
			}
			else
			{
				referenceDisplay.transform.position = cardInner.transform.position + Vector3.right * cardInner.rect.sizeDelta.x / 2f;
			}
		}
		StartCoroutine(_ShowSpellScene(spellObj, false));
	}

	private IEnumerator _ShowSpellScene(SpellObject spellObj, bool resetPreview)
	{
		ClearReferenceField();
		yield return new WaitForEndOfFrame();
		if (!(!onScreen || resetPreview))
		{
			yield break;
		}
		onScreen = true;
		ti.CreateRefField();
		BeingObject beingObj = sp.beingDictionary["ReferenceTemplate"].Clone();
		if (ctrl.currentPlayer != null)
		{
			beingObj.animName = ctrl.currentPlayer.beingObj.animName;
		}
		else if (ctrl.heCtrl.currentDisplayedHero != null)
		{
			beingObj.animName = ctrl.heCtrl.currentDisplayedHero.animName;
		}
		genPlayer = sp.SpawnPlayer(beingObj, 1, 1, 0, false, null, ti.refBattleGrid, true);
		genPlayer.duelDisk.currentMana = genPlayer.maxMana;
		genPlayer.duelDisk.matchDeckOrder = true;
		genDuelDisk = genPlayer.duelDisk;
		genPlayer.AddControlBlock(Block.Ref);
		exampleBeings.Add(genPlayer);
		genPlayer.anim.SetTrigger("skipSpawn");
		genPlayer.duelDisk.cardGrid.shuffleTimer.fillAmount = 0f;
		foreach (DiskReference theDiskRef in genPlayer.duelDisk.diskRefs)
		{
			theDiskRef.shuffler.fillAmount = 0f;
		}
		if (!genPlayer.battleGrid.currentBeings.Contains(genPlayer))
		{
			genPlayer.battleGrid.currentBeings.Add(genPlayer);
		}
		CreateDummyDebris();
		foreach (TileApp tileApp in spellObj.tileApps)
		{
			if (tileApp.location == Location.LastHitBy || tileApp.location == Location.LastHitByOther)
			{
				CreateDummyTurret();
			}
		}
		if (castExampleSpellCo != null)
		{
			StopCoroutine(castExampleSpellCo);
			castExampleSpellCo = null;
		}
		castExampleSpellCo = StartCoroutine(_CastExampleSpell(genPlayer, spellObj));
	}

	private void CreateDummyDebris()
	{
		Being being = sp.PlaceBeing("Dummy", ti.refBattleGrid.grid[5, 1], 0, true, ti.refBattleGrid);
		ctrl.RemoveObstacle(being);
		being.RemoveAllBuffs();
		exampleBeings.Add(being);
	}

	private void CreateDummyTurret()
	{
		Being being = sp.PlaceBeing("DummyTurret", ti.refBattleGrid.grid[5, 2], 0, true, ti.refBattleGrid);
		ctrl.RemoveObstacle(being);
		ctrl.RemoveObstacle(being);
		being.Activate();
		being.RemoveAllBuffs();
		exampleBeings.Add(being);
	}

	private IEnumerator _CastExampleSpell(Player thePlayer, SpellObject spellObj)
	{
		yield return new WaitForEndOfFrame();
		if (thePlayer == null)
		{
			yield break;
		}
		if (spellObj.tags.Contains(Tag.Weapon))
		{
			deCtrl.EquipWep(spellObj.itemID, thePlayer);
		}
		else
		{
			foreach (TileApp tileApp in spellObj.tileApps)
			{
				if (tileApp.location == Location.Structures)
				{
					AddSpell(thePlayer, spellObj, "SumWall");
				}
				else if (tileApp.location == Location.Poisoned)
				{
					AddSpell(thePlayer, spellObj, "Anubis");
				}
				else if (tileApp.location == Location.BrokenTiles)
				{
					ti.refBattleGrid.grid[4, 0].Break(99f);
					ti.refBattleGrid.grid[4, 1].Break(99f);
					ti.refBattleGrid.grid[4, 2].Break(99f);
				}
				else if (tileApp.location == Location.CrackedTiles)
				{
					ti.refBattleGrid.grid[4, 0].Crack(99f);
					ti.refBattleGrid.grid[5, 1].Crack(99f);
					ti.refBattleGrid.grid[4, 2].Crack(99f);
				}
				else if (tileApp.location == Location.Frosted)
				{
					AddSpell(thePlayer, spellObj, "ColdSnap");
				}
			}
			if (spellObj.HasAmount(AmountType.Shield) || spellObj.HasAmount(AmountType.ShieldPreCast))
			{
				AddSpell(thePlayer, spellObj, "ShieldsUp");
			}
			if (spellObj.HasAmount(AmountType.Structures))
			{
				AddSpell(thePlayer, spellObj, "SumWall");
			}
			AddSpell(thePlayer, spellObj, "ThisSpell");
			if (spellObj.HasAmount(AmountType.OtherSlotDamage) || spellObj.HasAmount(AmountType.OtherSlotManaCost))
			{
				AddSpell(thePlayer, spellObj, "Thunder", 2);
			}
			if (spellObj.HasCheck(Check.Fragile))
			{
				AddSpell(thePlayer, spellObj, "Showdown");
			}
			if (spellObj.HasAmount(AmountType.CurrentCardtridges))
			{
				AddSpell(thePlayer, spellObj, "Thunder", 3);
			}
			if (spellObj.itemID == "Zenith")
			{
				AddSpell(thePlayer, spellObj, "StepSlash");
			}
			else if (spellObj.itemID == "Echo")
			{
				AddSpell(thePlayer, spellObj, "Thunder");
			}
			else if (spellObj.itemID == "JamCannon")
			{
				AddSpell(thePlayer, spellObj, "Jam", 3);
			}
			else if (spellObj.itemID == "Midnight")
			{
				AddSpell(thePlayer, spellObj, "Thunder", 2);
				AddSpell(thePlayer, spellObj, "KineticWave", 2);
				AddSpell(thePlayer, spellObj, "Minigun", 2);
				AddSpell(thePlayer, spellObj, "FrostBarrage", 2);
			}
			if (spellObj.numShotsType.type == AmountType.FlowSelf)
			{
				thePlayer.AddStatus(Status.Flow, 4f);
			}
			if (spellObj.HasEffect(Effect.Trinity))
			{
				AddSpell(thePlayer, spellObj, "ThisSpell", 2);
			}
			if (spellObj.HasAmount(AmountType.MissingHP))
			{
				thePlayer.Damage(100);
			}
			if (spellObj.HasEffect(Effect.Flame))
			{
				ti.refBattleGrid.grid[4, 1].Flame(ctrl.deCtrl.CreateSpellBase("Default", thePlayer));
			}
		}
		thePlayer.duelDisk.shufflesThisBattle = 1;
		targetAlpha = 1f;
		yield return new WaitForSeconds(0.3f);
		if (thePlayer == null)
		{
			yield break;
		}
		thePlayer.Activate();
		yield return new WaitForSeconds(0.2f);
		if (thePlayer == null)
		{
			yield break;
		}
		if (spellObj.tags.Contains(Tag.Weapon))
		{
			while ((bool)thePlayer)
			{
				thePlayer.CastWeapon();
				yield return new WaitForSeconds(0.7f);
			}
			yield break;
		}
		while ((bool)thePlayer)
		{
			if (spellObj.HasTrigger(FTrigger.Hold))
			{
				yield return new WaitForSeconds(0.6f);
			}
			thePlayer.CastSpell(0);
			if (spellObj.itemID == "Zenith")
			{
				yield return new WaitForSeconds(0.1f);
				thePlayer.CastSpell(1);
			}
			if (spellObj.itemID == "Vivisection")
			{
				if (thePlayer.mov.currentTile.y == 1)
				{
					yield return new WaitForSeconds(0.3f);
					thePlayer.Move(0, 1);
					yield return new WaitForSeconds(0.2f);
					thePlayer.Move(0, 1);
					yield return new WaitForSeconds(0.2f);
					thePlayer.Move(-1, 0);
				}
				else if (thePlayer.mov.currentTile.y == 3)
				{
					yield return new WaitForSeconds(0.3f);
					thePlayer.Move(0, -1);
					yield return new WaitForSeconds(0.2f);
					thePlayer.Move(0, -1);
					yield return new WaitForSeconds(0.2f);
					thePlayer.Move(1, 0);
				}
			}
			if (spellObj.HasEffect(Effect.Summon) && spellObj.HasEffect(Effect.DoubleCast))
			{
				yield return new WaitForSeconds(0.1f);
				DoMove(thePlayer);
			}
			if (spellObj.itemID == "SumSilomini")
			{
				yield return new WaitForSeconds(16f);
			}
			if (thePlayer.duelDisk.shuffling)
			{
				yield return new WaitForSeconds(0.7f);
				if (!thePlayer)
				{
					break;
				}
				yield return new WaitUntil(() => thePlayer.mov.state == State.Idle);
				yield return new WaitForSeconds(0.1f);
				DoMove(thePlayer);
				yield return new WaitForSeconds(0.8f);
				if (spellObj.HasEffect(Effect.Poison))
				{
					yield return new WaitForSeconds(0.7f);
				}
				if (ti.refBattleGrid.grid[5, 1].IsOccupiable())
				{
					CreateDummyDebris();
				}
				if (ti.refBattleGrid.grid[5, 1].IsOccupiable())
				{
					CreateDummyTurret();
				}
			}
			yield return new WaitForSeconds(1f);
			if (thePlayer.duelDisk.currentCardtridges.Count <= 0)
			{
				yield return new WaitForSeconds(3f);
				StartCoroutine(_ShowSpellScene(spellObj, true));
				break;
			}
		}
	}

	private void AddSpell(Player thePlayer, SpellObject spellObj, string newSpellID, int amount = 1)
	{
		for (int i = 0; i < amount; i++)
		{
			spellObj.Set(thePlayer);
			thePlayer.duelDisk.AddLiveSpell(spellObj, newSpellID, thePlayer, false, false);
		}
	}

	private void DoMove(Player thePlayer)
	{
		if (thePlayer.mov.currentTile.y > 1)
		{
			thePlayer.Move(0, -1);
		}
		else if (Utils.RandomBool(2))
		{
			thePlayer.Move(0, 1);
		}
		else if (thePlayer.mov.currentTile.x < 1)
		{
			thePlayer.Move(1, 0);
		}
		else
		{
			thePlayer.Move(-1, 0);
		}
	}
}
