using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LibraryCtrl : NavPanel
{
	private DeckCtrl deCtrl;

	public SlideBody librarySB;

	public NavButton libraryButton;

	public NavButton libraryArtButton;

	public NavButton librarySpellButton;

	public RectTransform libraryDisplay;

	public CanvasGroup spellGrid;

	public CanvasGroup artGrid;

	public ScrollRect libraryScrollRect;

	public Scrollbar libraryScrollbar;

	public float scrollbarTargetValue;

	public List<LibraryCard> displayedSpells;

	public List<LibraryCard> displayedArts;

	public List<LibraryCard> displayedItems;

	public GameObject libraryCardPrefab;

	public List<LibraryCard> newOrder;

	public List<UIButton> sortButtons;

	public List<UIButton> activeSortButtons;

	public List<UIButton> spellsOnlySortButtons;

	public NavButton alphabeticalButton;

	public NavButton damageButton;

	public NavButton manaCostButton;

	public NavButton rarityButton;

	public NavButton shotsButton;

	public NavButton brandButton;

	public TMP_InputField searchbar;

	public Transform sortByGrid;

	public GameObject preloadedCard;

	public float lockedCardsTransparency = 0.6f;

	private NavButton activeSortButton;

	private bool descending = false;

	private Sort currentSort;

	private string searchValue;

	private float velocity = 0f;

	private bool closingCards = false;

	private bool showingCards = false;

	private bool autoScroll = false;

	private float lastScrollbarValue = 1f;

	public int cardsPerRow = 5;

	private Dictionary<string, Effect> localizedEfString = new Dictionary<string, Effect>();

	private Dictionary<string, FTrigger> localizedTrigString = new Dictionary<string, FTrigger>();

	public ItemManager itemMan;

	public UnlockCtrl unCtrl;

	public ReferenceCtrl refCtrl;

	private float cardWidth = 0f;

	private void Start()
	{
		deCtrl = S.I.deCtrl;
		activeSortButtons = new List<UIButton>(sortButtons);
		foreach (UIButton activeSortButton in activeSortButtons)
		{
			activeSortButton.navList = activeSortButtons;
		}
		if (S.I.EDITION != Edition.DemoLive)
		{
			cardWidth = spellGrid.GetComponent<GridLayoutGroup>().cellSize.x;
			StartCoroutine(_Prefill());
		}
	}

	private IEnumerator _Prefill()
	{
		if (!SaveDataCtrl.Initialized)
		{
			yield return new WaitUntil(() => SaveDataCtrl.Initialized);
		}
		if (S.I.EDITION == Edition.Full || S.I.EDITION == Edition.QA)
		{
			while (!SteamManager.Initialized && Time.timeSinceLevelLoad < S.maxLoadTime)
			{
				yield return null;
			}
		}
		yield return new WaitForEndOfFrame();
		int i = 0;
		while (i < itemMan.playerFullSpellList.Count && !libraryDisplay.gameObject.activeInHierarchy)
		{
			SimplePool.Preload(libraryCardPrefab, 1, libraryDisplay.transform);
			i++;
			yield return null;
		}
	}

	public void CheckSearch()
	{
		if (!btnCtrl.IsActivePanel(this))
		{
			return;
		}
		if (searchValue != searchbar.text)
		{
			searchValue = searchbar.text;
			scrollbarTargetValue = 1f;
			StartCoroutine(SetAutoScroll());
		}
		newOrder.Clear();
		Effect parsedEffect = Effect.CastSpell;
		FTrigger parsedTrigger = FTrigger.None;
		Brand parsedBrand = Brand.None;
		Tag parsedTag = Tag.Test;
		foreach (LibraryCard displayedItem in displayedItems)
		{
			displayedItem.anim.SetBool("grey", true);
		}
		string uppedSearchValue = CardInner.FirstCharToUpper(searchValue);
		if (localizedEfString.Keys.Contains(uppedSearchValue))
		{
			parsedEffect = localizedEfString[uppedSearchValue];
			if (parsedEffect == Effect.FlowSet)
			{
				newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.effectTags.Contains(parsedEffect) || t.itemObj.effectTags.Contains(Effect.FlowStack) || t.itemObj.effectTags.Contains(Effect.FlowSet) || t.itemObj.effectTags.Contains(Effect.TriggerFlow)).ToList();
			}
			else if (parsedEffect == Effect.Trinity)
			{
				newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.effectTags.Contains(parsedEffect) || t.itemObj.triggerTags.Contains(FTrigger.OnTrinityCast) || t.itemObj.triggerTags.Contains(FTrigger.TrinityCast)).ToList();
			}
			else if (parsedEffect == Effect.ShieldSet)
			{
				newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.effectTags.Contains(parsedEffect) || t.itemObj.effectTags.Contains(Effect.Shield)).ToList();
			}
			else
			{
				newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.effectTags.Contains(parsedEffect)).ToList();
			}
		}
		else if (localizedTrigString.Keys.Contains(uppedSearchValue))
		{
			parsedTrigger = localizedTrigString[uppedSearchValue];
			newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.triggerTags.Contains(parsedTrigger)).ToList();
		}
		else if (Enum.IsDefined(typeof(Effect), uppedSearchValue))
		{
			parsedEffect = (Effect)Enum.Parse(typeof(Effect), uppedSearchValue);
			if (parsedEffect == Effect.FlowSet)
			{
				newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.effectTags.Contains(parsedEffect) || t.itemObj.effectTags.Contains(Effect.FlowStack) || t.itemObj.effectTags.Contains(Effect.FlowSet) || t.itemObj.effectTags.Contains(Effect.TriggerFlow)).ToList();
			}
			else if (parsedEffect == Effect.Trinity)
			{
				newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.effectTags.Contains(parsedEffect) || t.itemObj.triggerTags.Contains(FTrigger.OnTrinityCast) || t.itemObj.triggerTags.Contains(FTrigger.TrinityCast)).ToList();
			}
			else if (parsedEffect == Effect.ShieldSet)
			{
				newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.effectTags.Contains(parsedEffect) || t.itemObj.effectTags.Contains(Effect.Shield)).ToList();
			}
			else
			{
				newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.effectTags.Contains(parsedEffect)).ToList();
			}
		}
		else if (Enum.IsDefined(typeof(FTrigger), uppedSearchValue))
		{
			parsedTrigger = (FTrigger)Enum.Parse(typeof(FTrigger), uppedSearchValue);
			newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.triggerTags.Contains(parsedTrigger)).ToList();
		}
		else if (Enum.IsDefined(typeof(Brand), uppedSearchValue))
		{
			parsedBrand = (Brand)Enum.Parse(typeof(Brand), uppedSearchValue);
			newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.brand == parsedBrand).ToList();
		}
		else if (Enum.IsDefined(typeof(Tag), uppedSearchValue))
		{
			parsedTag = (Tag)Enum.Parse(typeof(Tag), uppedSearchValue);
			newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.tags.Contains(parsedTag)).ToList();
		}
		else
		{
			newOrder = displayedItems.Where((LibraryCard t) => t.itemObj.nameString.IndexOf(uppedSearchValue, 0, StringComparison.CurrentCultureIgnoreCase) != -1).ToList();
		}
		foreach (NavButton sortButton in sortButtons)
		{
			if (sortButton.gameObject.activeInHierarchy)
			{
				sortButton.GoToColor(UIColor.White);
			}
		}
		if (currentSort == Sort.Name)
		{
			newOrder = newOrder.OrderBy((LibraryCard w) => w.itemObj.nameString).ToList();
			activeSortButton = alphabeticalButton;
		}
		else if (currentSort == Sort.Rarity)
		{
			newOrder = newOrder.OrderBy((LibraryCard w) => w.itemObj.rarity).ToList();
			activeSortButton = rarityButton;
		}
		else if (currentSort == Sort.Damage)
		{
			newOrder = newOrder.OrderBy((LibraryCard w) => w.itemObj.spellObj.damage).ToList();
			activeSortButton = damageButton;
		}
		else if (currentSort == Sort.Mana)
		{
			newOrder = newOrder.OrderBy((LibraryCard w) => w.itemObj.spellObj.mana).ToList();
			activeSortButton = manaCostButton;
		}
		else if (currentSort == Sort.Shots)
		{
			newOrder = newOrder.OrderBy((LibraryCard w) => w.itemObj.spellObj.CalculatedSortingShots()).ToList();
			activeSortButton = shotsButton;
		}
		else if (currentSort == Sort.Brand)
		{
			newOrder = newOrder.OrderBy((LibraryCard w) => w.itemObj.spellObj.brand).ToList();
			activeSortButton = brandButton;
		}
		if (descending)
		{
			newOrder.Reverse();
		}
		for (int i = 0; i < newOrder.Count; i++)
		{
			newOrder[i].transform.SetSiblingIndex(i);
			newOrder[i].anim.SetBool("grey", false);
		}
		if (newOrder.Count > 0)
		{
			SetRightFocuses(newOrder[0]);
		}
		scrollbarTargetValue = 1f;
	}

	private IEnumerator SetAutoScroll()
	{
		autoScroll = true;
		yield return new WaitForSecondsRealtime(0.6f);
		autoScroll = false;
	}

	private void Update()
	{
		if (!btnCtrl.IsActivePanel(this))
		{
			return;
		}
		if (!btnCtrl.mouseActive || autoScroll)
		{
			libraryScrollbar.value = Mathf.SmoothDamp(libraryScrollbar.value, scrollbarTargetValue, ref velocity, 0.1f);
		}
		else
		{
			scrollbarTargetValue = libraryScrollbar.value;
			if (lastScrollbarValue != libraryScrollbar.value)
			{
				lastScrollbarValue = libraryScrollbar.value;
			}
		}
		if (activeSortButton != null)
		{
			if (descending)
			{
				newOrder.Reverse();
				activeSortButton.GoToColor(UIColor.BlueLight);
			}
			else
			{
				activeSortButton.GoToColor(UIColor.Pink);
			}
		}
	}

	public void OrderBy(string sortString)
	{
		if (Enum.IsDefined(typeof(Sort), sortString))
		{
			OrderBy((Sort)Enum.Parse(typeof(Sort), sortString));
		}
	}

	public void OrderBy(Sort newSort, bool toggle = true)
	{
		if (currentSort == newSort && toggle)
		{
			descending = !descending;
		}
		else
		{
			descending = false;
			currentSort = newSort;
		}
		CheckSearch();
	}

	public int GetCardsPerRow()
	{
		cardsPerRow = Mathf.FloorToInt((libraryDisplay.rect.width + 4f) / (cardWidth + 4f));
		return cardsPerRow;
	}

	public override void Open()
	{
		base.Open();
		unCtrl.UpdateUnlockLevel();
		libraryDisplay.gameObject.SetActive(true);
		FillEffectAndTriggerDictionaries();
		ClickLibrarySpells();
		StartCoroutine(OpenMenuC(sortByGrid));
	}

	private void FillEffectAndTriggerDictionaries()
	{
		localizedEfString = new Dictionary<string, Effect>();
		foreach (Effect item in Enum.GetValues(typeof(Effect)).Cast<Effect>().ToList())
		{
			string Translation = string.Empty;
			LocalizationManager.TryGetTranslation("MechKeys/" + item, out Translation);
			if (!string.IsNullOrEmpty(Translation))
			{
				localizedEfString[Translation] = item;
			}
		}
		localizedTrigString = new Dictionary<string, FTrigger>();
		foreach (FTrigger item2 in Enum.GetValues(typeof(FTrigger)).Cast<FTrigger>().ToList())
		{
			string Translation2 = string.Empty;
			LocalizationManager.TryGetTranslation("MechKeys/" + item2, out Translation2);
			if (!string.IsNullOrEmpty(Translation2))
			{
				localizedTrigString[Translation2] = item2;
			}
		}
	}

	private IEnumerator OpenMenuC(Transform parentGrid)
	{
		yield return new WaitForEndOfFrame();
		foreach (Transform child in parentGrid)
		{
			yield return new WaitForSecondsRealtime(0.05f);
			if ((bool)child)
			{
				child.GetComponent<Animator>().SetBool("visible", true);
			}
		}
	}

	public void ClickLibraryArtifacts()
	{
		if (showingCards)
		{
			return;
		}
		libraryScrollRect.content = artGrid.GetComponent<RectTransform>();
		artGrid.gameObject.SetActive(true);
		artGrid.alpha = 1f;
		artGrid.interactable = true;
		artGrid.blocksRaycasts = true;
		artGrid.transform.localScale = Vector3.one;
		artGrid.transform.SetAsFirstSibling();
		if (currentSort != Sort.Rarity && currentSort != Sort.Name)
		{
			OrderBy("Name");
		}
		else
		{
			OrderBy(currentSort, false);
		}
		if (artGrid.transform.childCount < 1)
		{
			foreach (ArtifactObject item in deCtrl.itemMan.playerFullArtList.OrderBy((ArtifactObject w) => w.nameString).ToList())
			{
				CreateNewLibraryCard(item, artGrid.transform, displayedArts, libraryArtButton);
			}
		}
		displayedItems = displayedArts;
		activeSortButtons[activeSortButtons.Count - 1].down = null;
		foreach (UIButton spellsOnlySortButton in spellsOnlySortButtons)
		{
			spellsOnlySortButton.gameObject.SetActive(false);
			activeSortButtons.Remove(spellsOnlySortButton);
		}
		activeSortButtons[activeSortButtons.Count - 1].down = librarySpellButton;
		librarySpellButton.up = activeSortButtons[activeSortButtons.Count - 1];
		searchbar.text = string.Empty;
		StartCoroutine(_ShowLibraryCards(spellGrid));
	}

	public void ClickLibrarySpells()
	{
		if (showingCards)
		{
			return;
		}
		libraryScrollRect.content = spellGrid.GetComponent<RectTransform>();
		spellGrid.gameObject.SetActive(true);
		spellGrid.alpha = 1f;
		spellGrid.interactable = true;
		spellGrid.blocksRaycasts = true;
		spellGrid.transform.localScale = Vector3.one;
		if (spellGrid.transform.childCount < 1)
		{
			foreach (SpellObject item in deCtrl.itemMan.playerFullSpellList.OrderBy((SpellObject w) => w.nameString).ToList())
			{
				CreateNewLibraryCard(item, spellGrid.transform, displayedSpells, librarySpellButton);
			}
		}
		displayedItems = displayedSpells;
		activeSortButtons[activeSortButtons.Count - 1].down = null;
		if (!manaCostButton.gameObject.activeInHierarchy)
		{
			foreach (UIButton spellsOnlySortButton in spellsOnlySortButtons)
			{
				spellsOnlySortButton.gameObject.SetActive(true);
				spellsOnlySortButton.anim.SetBool("visible", true);
				activeSortButtons.Add(spellsOnlySortButton);
			}
		}
		activeSortButtons[activeSortButtons.Count - 1].down = librarySpellButton;
		librarySpellButton.up = activeSortButtons[activeSortButtons.Count - 1];
		searchbar.text = string.Empty;
		StartCoroutine(_ShowLibraryCards(artGrid));
	}

	private void CreateNewLibraryCard(ItemObject itemObj, Transform parent, List<LibraryCard> cardList, NavButton backButton)
	{
		LibraryCard component = SimplePool.Spawn(libraryCardPrefab).GetComponent<LibraryCard>();
		component.transform.SetParent(parent, false);
		component.deCtrl = deCtrl;
		component.refCtrl = refCtrl;
		component.libraryCard = true;
		component.SetHidden(itemObj);
		component.btnCtrl = btnCtrl;
		component.libCtrl = this;
		component.back = backButton;
		cardList.Add(component);
	}

	public override void Close()
	{
		StartCoroutine(_Close());
	}

	private IEnumerator _Close()
	{
		if (!btnCtrl.transitioning)
		{
			btnCtrl.transitioning = true;
			closingCards = true;
			while (showingCards)
			{
				yield return null;
			}
			_003C_003En__0();
			yield return new WaitForSeconds(0.2f);
			S.I.mainCtrl.Open();
			closingCards = false;
			btnCtrl.transitioning = false;
		}
	}

	private IEnumerator _ShowLibraryCards(CanvasGroup otherGrid)
	{
		showingCards = true;
		while (closingCards)
		{
			yield return null;
		}
		otherGrid.alpha = 0f;
		otherGrid.interactable = false;
		otherGrid.blocksRaycasts = false;
		otherGrid.transform.localScale = Vector3.zero;
		foreach (LibraryCard card in displayedItems)
		{
			card.hidden = true;
		}
		yield return new WaitForEndOfFrame();
		scrollbarTargetValue = 1f;
		StartCoroutine(SetAutoScroll());
		SetRightFocuses(displayedItems[0]);
		yield return new WaitForSeconds(0.6f);
		showingCards = false;
	}

	private void SetRightFocuses(LibraryCard cardToFocus)
	{
		libraryArtButton.right = cardToFocus;
		librarySpellButton.right = cardToFocus;
		foreach (UIButton sortButton in sortButtons)
		{
			sortButton.right = cardToFocus;
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.Close();
	}
}
