using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[MoonSharpUserData]
public class DeckScreen : NavPanel
{
	public int newSpells = 0;

	public DeckMarker manaRegenDeckMarkerPrefab;

	public DeckMarker deckMarkerPrefab;

	public List<DeckMarker> deckMarkers = new List<DeckMarker>();

	public LayoutElement listPlaceholderPrefab;

	public List<LayoutElement> placeholders = new List<LayoutElement>();

	public RectTransform deckListContainer;

	public Transform deckGrid;

	public Scrollbar deckScrollbar;

	public CanvasGroup underlay;

	public AdjustingCursor spellCursor;

	public DeckCtrl deCtrl;

	public UIButton savedBtn;

	public AudioClip upgradeSound;

	public AudioClip removeSound;

	public int startingRemovals = 1;

	public TMP_Text removersCountText;

	public Animator removerAnimator;

	public SquareListCard weaponListCard;

	public CanvasGroup removeContainer;

	public CanvasGroup upgradeContainer;

	public RectTransform statsGrid;

	public Vector3 originalStatsGridPos;

	public Image upgradeSpellFill;

	public Image removeSpellFill;

	public int startingUpgraders = 0;

	public TMP_Text upgradersCountText;

	public Animator upgraderAnimator;

	public bool upgradeInProgress = false;

	public bool busy = false;

	private int savedDeckFocusIndex = 0;

	public float introDelay = 0.3f;

	public FocusCtrl foCtrl;

	public RunCtrl runCtrl;

	private UnityEngine.Coroutine co_AddManaRegenMarkers;

	private BC ctrl;

	private ShopCtrl shopCtrl;

	public bool dirty = false;

	private void Start()
	{
		ctrl = S.I.batCtrl;
		btnCtrl = S.I.btnCtrl;
		deCtrl = S.I.deCtrl;
		runCtrl = S.I.runCtrl;
		shopCtrl = S.I.shopCtrl;
		originalStatsGridPos = statsGrid.anchoredPosition;
		if (S.I.PS4)
		{
			statsGrid.anchoredPosition = new Vector2(statsGrid.anchoredPosition.x, 13.6f);
		}
		weaponListCard.anim.SetBool("spawned", true);
	}

	public void ResetValues()
	{
		runCtrl.currentRun.removals = startingRemovals;
		runCtrl.currentRun.upgraders = startingUpgraders;
	}

	public void AddNotification()
	{
		newSpells++;
	}

	public void UpdateInfo()
	{
		int count = ctrl.currentPlayer.duelDisk.deck.Count;
		float num = 0f;
		foreach (ListCard item in ctrl.currentPlayer.duelDisk.deck)
		{
			num += item.spellObj.mana;
		}
	}

	public void AnimateCollection()
	{
		StartCoroutine(AnimateCollectionC());
	}

	private IEnumerator AnimateCollectionC()
	{
		yield return new WaitForSeconds(0.15f);
		if (deCtrl.duelDisks.Count <= 0)
		{
			yield break;
		}
		if (slideBody.onScreen)
		{
			float delayTime = 0.03f;
			if (deCtrl.duelDisks[0].deck.Count > 15)
			{
				delayTime = 0.02f;
			}
			for (int j = 0; j < deCtrl.duelDisks[0].deck.Count; j++)
			{
				yield return new WaitForSeconds(delayTime);
				if (deCtrl.duelDisks[0].deck.Count > j)
				{
					deCtrl.duelDisks[0].deck[j].anim.SetBool("visible", true);
				}
			}
			yield break;
		}
		for (int i = 0; i < deCtrl.duelDisks[0].deck.Count; i++)
		{
			if ((bool)deCtrl.duelDisks[0].deck[i].anim)
			{
				deCtrl.duelDisks[0].deck[i].anim.SetBool("visible", false);
			}
		}
	}

	public void RemoveCard(SpellListCard spellListCard)
	{
		if ((bool)ctrl.currentPlayer && deCtrl.ctrl.GameState != GState.GameOver && !busy && ctrl.currentPlayer.duelDisk.deck.Count > deCtrl.minDeckSize)
		{
			spellListCard.RemoveThisCard();
		}
	}

	public void RemoveArtifact(SquareListCard artListCard)
	{
		if ((bool)ctrl.currentPlayer && deCtrl.ctrl.GameState != GState.GameOver && !busy && ctrl.currentPlayer.duelDisk.deck.Count > deCtrl.minDeckSize)
		{
			artListCard.RemoveThisCard();
		}
	}

	public void UpdateRemoverCountText()
	{
		removersCountText.text = runCtrl.currentRun.removals + "x";
		if (runCtrl.currentRun.removals < 1)
		{
			removeContainer.alpha = 0.4f;
		}
		else
		{
			removeContainer.alpha = 1f;
		}
	}

	public void UpdateUpgraderCountText()
	{
		upgradersCountText.text = runCtrl.currentRun.upgraders + "x";
		if (runCtrl.currentRun.upgraders < 1)
		{
			upgradeContainer.alpha = 0.4f;
		}
		else
		{
			upgradeContainer.alpha = 1f;
		}
		if (btnCtrl.activeNavPanels.Contains(S.I.poCtrl))
		{
			upgradeContainer.alpha = 0.4f;
		}
	}

	public override void Open()
	{
		if (!PostCtrl.transitioning && !ShopCtrl.transitioning)
		{
			UpdateRemoverCountText();
			UpdateUpgraderCountText();
			if ((bool)ctrl.currentPlayer)
			{
				deCtrl.ctrl.AddControlBlocks(Block.DeckPanel);
				deCtrl.statsScreen.UpdateStatsText(ctrl.currentPlayer);
			}
			if (((bool)shopCtrl.currentShopkeeper && !shopCtrl.currentShopkeeper.downed && !btnCtrl.activeNavPanels.Contains(ctrl.poCtrl)) || (shopCtrl.selfMode && (bool)ctrl.currentPlayer))
			{
				shopCtrl.Open();
				underlay.alpha = 0f;
				underlay.blocksRaycasts = false;
			}
			else
			{
				underlay.alpha = 1f;
				underlay.blocksRaycasts = true;
			}
			UpdateButtonNavs();
			base.Open();
			ctrl.gameOverPane.killsText.color = Color.clear;
			CalculateGrid();
			AnimateCollection();
			deckScrollbar.value = 1f;
			UpdateInfo();
			StartCoroutine(FocusAfter());
			StartCoroutine(StartDeckTutorial());
		}
	}

	public void UpdateButtonNavs()
	{
		if (deCtrl.artCardList.Count > 0)
		{
			foreach (ListCard item in ctrl.currentPlayer.duelDisk.deck)
			{
				item.left = deCtrl.artCardList[deCtrl.artCardList.Count - 1];
				item.down = null;
			}
			foCtrl.brandDisplayButtons[0].left = deCtrl.artCardList[deCtrl.artCardList.Count - 1];
			foCtrl.brandDisplayButtons[1].right = deCtrl.artCardList[0];
		}
		else if (deCtrl.pactCardList.Count > 0)
		{
			foreach (ListCard item2 in ctrl.currentPlayer.duelDisk.deck)
			{
				item2.left = deCtrl.pactCardList[deCtrl.pactCardList.Count - 1];
				item2.down = null;
			}
			foCtrl.brandDisplayButtons[0].left = deCtrl.pactCardList[deCtrl.pactCardList.Count - 1];
			foCtrl.brandDisplayButtons[1].right = deCtrl.pactCardList[0];
		}
		if (deCtrl.pactCardList.Count > 0)
		{
			for (int i = 0; i < ctrl.currentPlayer.duelDisk.deck.Count; i++)
			{
				if (deCtrl.pactCardList.Count > i && deCtrl.artCardList.Count > i)
				{
					deCtrl.pactCardList[i].up = deCtrl.artCardList[i];
					deCtrl.pactCardList[i].up.down = deCtrl.pactCardList[i];
				}
			}
		}
		if (ctrl.currentPlayer.duelDisk.deck.Count > 0)
		{
			weaponListCard.up = ctrl.currentPlayer.duelDisk.deck[ctrl.currentPlayer.duelDisk.deck.Count - 1];
			for (int j = 0; j < foCtrl.brandDisplayButtons.Length; j++)
			{
				foCtrl.brandDisplayButtons[j].down = ctrl.currentPlayer.duelDisk.deck[0];
			}
		}
		foCtrl.brandDisplayButtons[0].up = weaponListCard;
		foCtrl.brandDisplayButtons[1].up = weaponListCard;
		weaponListCard.down = foCtrl.brandDisplayButtons[0];
	}

	public void CalculateGrid()
	{
		for (int num = deckMarkers.Count - 1; num >= 0; num--)
		{
			Object.Destroy(deckMarkers[num].gameObject);
		}
		deckMarkers.Clear();
		for (int num2 = placeholders.Count - 1; num2 >= 0; num2--)
		{
			Object.Destroy(placeholders[num2].gameObject);
		}
		placeholders.Clear();
		if (placeholders.Count >= (Mathf.FloorToInt((float)ctrl.currentPlayer.duelDisk.deck.Count / 5f) + 1) * 5 - ctrl.currentPlayer.duelDisk.deck.Count)
		{
			for (int num3 = placeholders.Count - 1; num3 >= (Mathf.FloorToInt((float)ctrl.currentPlayer.duelDisk.deck.Count / 5f) + 1) * 5 - ctrl.currentPlayer.duelDisk.deck.Count; num3--)
			{
				Object.Destroy(placeholders[num3].gameObject);
				placeholders.Remove(placeholders[num3]);
			}
		}
		else
		{
			for (int i = placeholders.Count; i <= (Mathf.FloorToInt((float)ctrl.currentPlayer.duelDisk.deck.Count / 5f) + 1) * 5 - ctrl.currentPlayer.duelDisk.deck.Count; i++)
			{
				LayoutElement item = Object.Instantiate(listPlaceholderPrefab, deckGrid);
				placeholders.Add(item);
			}
		}
		if (co_AddManaRegenMarkers != null)
		{
			StopCoroutine(co_AddManaRegenMarkers);
			co_AddManaRegenMarkers = null;
		}
		co_AddManaRegenMarkers = StartCoroutine(_AddManaRegenMarkers());
	}

	private IEnumerator _AddManaRegenMarkers()
	{
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < Mathf.FloorToInt((float)ctrl.currentPlayer.duelDisk.deck.Count / 5f) + 1; i++)
		{
			if (i < deCtrl.deckRegenLimit)
			{
				DeckMarker newDeckMarker2 = Object.Instantiate(manaRegenDeckMarkerPrefab, deckGrid);
				deckMarkers.Add(newDeckMarker2);
				newDeckMarker2.Reset();
			}
			else
			{
				DeckMarker newDeckMarker = Object.Instantiate(deckMarkerPrefab, deckGrid);
				deckMarkers.Add(newDeckMarker);
				newDeckMarker.Reset();
				newDeckMarker.text.text = ((i + 1) * 5).ToString();
			}
		}
		for (int j = 0; j < deckMarkers.Count; j++)
		{
			deckMarkers[j].transform.SetSiblingIndex((j + 1) * 5 + j);
			if (ctrl.currentPlayer.duelDisk.deck.Count >= (j + 1) * 5)
			{
				deckMarkers[j].Highlight();
			}
		}
		CalculateGridHeight(deckGrid);
		co_AddManaRegenMarkers = null;
	}

	private IEnumerator StartDeckTutorial()
	{
		if (!SaveDataCtrl.Get("TutorialDeckShown", false))
		{
			runCtrl.tutCtrl.StartTutorialDeck();
			while (runCtrl.tutCtrl.tutorialDeckInProgress)
			{
				yield return new WaitForEndOfFrame();
			}
			ctrl.RemoveControlBlocks(Block.Tutorial);
		}
		runCtrl.tutCtrl.originButton = GetButtonToFocus();
	}

	private IEnumerator FocusAfter()
	{
		btnCtrl.RemoveFocus();
		float stayTime = introDelay + Time.time;
		while (btnCtrl.IsActivePanel(this) && stayTime > Time.time)
		{
			yield return null;
		}
		while (Time.timeScale < 1f)
		{
			yield return null;
		}
		if (btnCtrl.IsActivePanel(this))
		{
			spellCursor.Hide();
			btnCtrl.SetFocus(GetButtonToFocus());
		}
	}

	private UIButton GetButtonToFocus()
	{
		if (ctrl.currentPlayer.duelDisk.deck.Count > 0)
		{
			if (ctrl.currentPlayer.duelDisk.deck.Count > savedDeckFocusIndex)
			{
				return ctrl.currentPlayer.duelDisk.deck[savedDeckFocusIndex];
			}
			return ctrl.currentPlayer.duelDisk.deck[0];
		}
		if (foCtrl.brandDisplayButtons.Length != 0)
		{
			return foCtrl.brandDisplayButtons[0];
		}
		if (deCtrl.artCardList.Count > 0)
		{
			return deCtrl.artCardList[deCtrl.artCardList.Count - 1];
		}
		return null;
	}

	public void ClearAll()
	{
		deckGrid.DestroyChildren();
		deckMarkers.Clear();
		placeholders.Clear();
	}

	public void CalculateGridHeight(Transform gridTransform)
	{
		RectTransform component = gridTransform.GetComponent<RectTransform>();
		if ((bool)deCtrl && gridTransform == deckGrid)
		{
			float num = (float)(deckGrid.childCount - deckMarkers.Count) * deCtrl.spellListCardPrefab.layoutElement.minHeight;
			if (num < deckListContainer.rect.height)
			{
				num = deckListContainer.rect.height;
			}
			component.sizeDelta = new Vector2(component.sizeDelta.x, num);
		}
	}

	public override void Close()
	{
		if (ShopCtrl.transitioning && ctrl.GameState != GState.MainMenu)
		{
			return;
		}
		if ((bool)btnCtrl.focusedButton && (bool)btnCtrl.focusedButton.GetComponent<SpellListCard>() && ctrl.currentPlayer.duelDisk.deck.Contains(btnCtrl.focusedButton.GetComponent<SpellListCard>()))
		{
			deCtrl.displayCard.Hide();
			savedDeckFocusIndex = ctrl.currentPlayer.duelDisk.deck.IndexOf(btnCtrl.focusedButton.GetComponent<SpellListCard>());
		}
		else
		{
			savedDeckFocusIndex = 0;
		}
		base.Close();
		if (btnCtrl.activeNavPanels.Contains(shopCtrl))
		{
			shopCtrl.Close();
		}
		if ((bool)ctrl.currentPlayer && dirty)
		{
			ctrl.currentPlayer.duelDisk.CreateDeckSpells();
		}
		foreach (Player currentPlayer in ctrl.currentPlayers)
		{
			currentPlayer.dontInterruptAnim = false;
			currentPlayer.RemoveControlBlock(Block.Fake);
			currentPlayer.mov.SetState(State.Idle);
		}
		ctrl.gameOverPane.killsText.color = Color.white;
		deCtrl.ctrl.RemoveControlBlocksNextFrame(Block.DeckPanel);
		newSpells = 0;
		spellCursor.Hide();
		AnimateCollection();
	}
}
