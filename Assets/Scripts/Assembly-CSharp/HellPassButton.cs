using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HellPassButton : NavButton
{
	public HeroSelectCtrl heCtrl;

	public int displayedHellPassNum = 0;

	public TMP_Text description;

	public SquareListCard listCard;

	public SquareListCard topCard;

	public SquareListCard botCard;

	public List<PactObject> hellPasses;

	public List<VictoryTriangle> victoryTriangles;

	public List<HellPassListCard> hellPassListCards;

	[SerializeField]
	private HellPassListCard hellPassListCardPrefab = null;

	public CanvasGroup hellPassGrid;

	public CanvasGroup hellPassGridContainer;

	public Scrollbar scrollbar;

	public List<int> activatedHellPasses = new List<int>();

	public CanvasGroup background;

	private Coroutine animateGridCo;

	protected override void Awake()
	{
		base.Awake();
		hellPasses = S.I.itemMan.hellPasses;
		listCard.SetPact(S.I.itemMan.hellPasses[displayedHellPassNum], base.transform);
		StartCoroutine(_SetDefaultPrefs());
		background.alpha = 0f;
		HideHellPassGrid();
		hellPassGrid.transform.DestroyChildren();
	}

	private IEnumerator _SetDefaultPrefs()
	{
		yield return new WaitUntil(() => SaveDataCtrl.Initialized);
		displayedHellPassNum = SaveDataCtrl.Get("HellPassNum", 0);
		for (int i = 0; i < S.I.itemMan.hellPasses.Count; i++)
		{
			if (hellPassListCards.Count <= i)
			{
				hellPassListCards.Add(Object.Instantiate(hellPassListCardPrefab, hellPassGrid.transform));
				hellPassListCards[i].back = this;
				hellPassListCards[i].hellPassButton = this;
				hellPassListCards[i].hellPassNum = i;
			}
			hellPassListCards[i].SetPact(S.I.itemMan.hellPasses[i], hellPassGrid.transform, -1, true);
			hellPassListCards[i].SetActivated(SaveDataCtrl.Get("HellPassActivated" + i, false) && i <= heCtrl.runCtrl.unlockedHellPassNum + 1, false);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (IsHoverable())
		{
			SetLinePath(eventData);
		}
	}

	public int ExtraTriangleOffset()
	{
		return 9;
	}

	public void SetLinePath(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (heCtrl.focusedAltButton == null)
		{
			heCtrl.UpdateHeroSelectors();
			if (heCtrl.unlockedHeroAltButtons.Count > heCtrl.focusedHeroButton.lastAltFocusedIndex)
			{
				btnCtrl.SetFocus(heCtrl.unlockedHeroAltButtons[heCtrl.focusedHeroButton.lastAltFocusedIndex]);
			}
			else
			{
				btnCtrl.SetFocus(heCtrl.unlockedHeroAltButtons[heCtrl.unlockedHeroAltButtons.Count - 1]);
			}
			heCtrl.UpdateAltSelectors(true);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
	}

	public override void Up()
	{
		ShowHellPassGrid();
		btnCtrl.SetFocus(hellPassListCards[displayedHellPassNum]);
	}

	public override void Down()
	{
		ShowHellPassGrid();
		btnCtrl.SetFocus(hellPassListCards[displayedHellPassNum]);
	}

	public override void Focus(int playerNum = 0)
	{
		background.alpha = 1f;
		S.I.PlayOnce(btnCtrl.flipSound);
		base.Focus(playerNum);
		heCtrl.changeSkinButton.alpha = 0.4f;
		UpdateButton();
		listCard.anim.SetBool("hell", true);
		topCard.anim.SetBool("hell", true);
		botCard.anim.SetBool("hell", true);
		HideHellPassGrid();
	}

	public override void UnFocus()
	{
		base.UnFocus();
		topCard.anim.SetBool("spawned", false);
		botCard.anim.SetBool("spawned", false);
		victoryTriangles[1].Hide();
	}

	public void HideHellPassGrid(bool focusHellPassButton = false)
	{
		if (hellPassGridContainer.blocksRaycasts)
		{
			hellPassGridContainer.blocksRaycasts = false;
			hellPassGridContainer.interactable = false;
			if (animateGridCo != null)
			{
				StopCoroutine(animateGridCo);
			}
			StartCoroutine(AnimateGridC(false));
		}
		if (focusHellPassButton)
		{
			btnCtrl.SetFocus(this);
		}
	}

	private void ShowHellPassGrid()
	{
		hellPassGridContainer.alpha = 1f;
		hellPassGridContainer.blocksRaycasts = true;
		hellPassGridContainer.interactable = true;
		for (int i = 0; i < hellPassListCards.Count; i++)
		{
			hellPassListCards[i].iconAnim.SetBool("spawned", true);
			hellPassListCards[i].victoryTriangle.Show(heCtrl.currentDisplayedHero.beingID, i);
			hellPassListCards[i].UpdatePactText();
		}
		for (int j = 0; j < S.I.itemMan.hellPasses.Count; j++)
		{
			hellPassListCards[j].SetActivated(activatedHellPasses.Contains(j) && j <= heCtrl.runCtrl.unlockedHellPassNum + 1, false);
		}
		if (animateGridCo != null)
		{
			StopCoroutine(animateGridCo);
		}
		animateGridCo = StartCoroutine(AnimateGridC(true));
	}

	private IEnumerator AnimateGridC(bool show)
	{
		if (show)
		{
			for (int j = 0; j < hellPassListCards.Count; j++)
			{
				if (displayedHellPassNum <= j)
				{
					yield return new WaitForSeconds(0.03f);
				}
				hellPassListCards[j].anim.SetBool("visible", true);
			}
			yield break;
		}
		for (int i = 0; i < hellPassListCards.Count; i++)
		{
			if ((bool)hellPassListCards[i].anim)
			{
				hellPassListCards[i].anim.SetBool("visible", false);
			}
		}
		yield return new WaitForSeconds(0.07f);
		hellPassGridContainer.alpha = 0f;
	}

	private void UpdateButton()
	{
		if (displayedHellPassNum > heCtrl.runCtrl.unlockedHellPassNum + 1)
		{
			S.I.PlayOnce(btnCtrl.lockedSound);
		}
		int num = 0;
		for (int i = 0; i < activatedHellPasses.Count; i++)
		{
			if (activatedHellPasses[i] > num)
			{
				num = activatedHellPasses[i];
			}
		}
		displayedHellPassNum = num;
		displayedHellPassNum = Mathf.Clamp(displayedHellPassNum, 0, heCtrl.runCtrl.unlockedHellPassNum + 1);
		displayedHellPassNum = Mathf.Clamp(displayedHellPassNum, 0, hellPasses.Count - 1);
		listCard.SetPact(hellPasses[displayedHellPassNum], base.transform, -1, true);
		victoryTriangles[1].Show(heCtrl.currentDisplayedHero.beingID, displayedHellPassNum);
		description.text = LocalizationManager.GetTranslation("PactDescriptions/" + listCard.pactObj.itemID);
		if (displayedHellPassNum > 0)
		{
			topCard.SetPact(hellPasses[displayedHellPassNum - 1], topCard.transform, -1, true);
			victoryTriangles[2].Show(heCtrl.currentDisplayedHero.beingID, displayedHellPassNum - 1);
			victoryTriangles[0].Show(heCtrl.currentDisplayedHero.beingID, displayedHellPassNum - 1);
		}
		else
		{
			topCard.anim.SetBool("spawned", false);
			victoryTriangles[0].Hide();
		}
		if (displayedHellPassNum < hellPasses.Count - 1)
		{
			botCard.SetPact(hellPasses[displayedHellPassNum + 1], botCard.transform, -1, true);
			if (displayedHellPassNum > heCtrl.runCtrl.unlockedHellPassNum)
			{
				botCard.image.color = Color.grey;
				victoryTriangles[2].Hide();
			}
			else
			{
				botCard.image.color = Color.white;
				victoryTriangles[2].Show(heCtrl.currentDisplayedHero.beingID, displayedHellPassNum + 1);
			}
		}
		else
		{
			botCard.anim.SetBool("spawned", false);
		}
		SetActiveHellPasses();
	}

	public override void OnBackPress()
	{
		HideHellPassGrid();
		base.OnBackPress();
	}

	public void SetActiveHellPasses()
	{
		heCtrl.runCtrl.currentHellPasses = new List<int>(activatedHellPasses.OrderBy((int w) => w));
		if (!heCtrl.runCtrl.currentHellPasses.Contains(0))
		{
			heCtrl.runCtrl.currentHellPasses.Insert(0, 0);
		}
	}

	public override void OnAcceptPress()
	{
		S.I.PlayOnce(btnCtrl.chooseSound);
		heCtrl.startButton.back = this;
		SaveDataCtrl.Set("HellPassNum", displayedHellPassNum);
		SetActiveHellPasses();
		StartColorCo(U.I.GetColor(UIColor.Pink));
		heCtrl.FocusStartButton();
		heCtrl.runCtrl.currentHellPassNum = displayedHellPassNum;
		victoryTriangles[1].Show(heCtrl.currentDisplayedHero.beingID, heCtrl.runCtrl.currentHellPassNum);
	}

	public void Hide()
	{
		description.text = "";
		background.alpha = 0f;
		StartColorCo(Color.clear);
		listCard.anim.SetBool("spawned", false);
		victoryTriangles[1].Hide();
	}
}
