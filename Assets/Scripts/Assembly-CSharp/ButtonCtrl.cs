using System.Collections;
using System.Collections.Generic;
using Rewired;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonCtrl : SerializedMonoBehaviour
{
	public UIButton focusedButton;

	public UIButton playerTwoFocusedButton;

	public GameObject camFollow;

	public Camera mainCam;

	private bool kUp;

	private bool kRight;

	private bool kDown;

	private bool kLeft;

	public AudioClip pierceSound;

	public AudioClip flipSound;

	public AudioClip chooseSound;

	public AudioClip chooseAltSound;

	public AudioClip hoverSound;

	public AudioClip lockedSound;

	public AudioClip focusSound;

	public List<TopNavButton> topNavButtons;

	public CheatCtrl cheatCtrl;

	private BC ctrl;

	private ConsoleView consoleView;

	private ControlsCtrl conCtrl;

	private CreditsCtrl credCtrl;

	private DeckCtrl deCtrl;

	private IdleCtrl idCtrl;

	private OptionCtrl optCtrl;

	private RunCtrl runCtrl;

	private TutorialCtrl tutCtrl;

	public bool mouseActive = true;

	public List<NavPanel> activeNavPanels;

	public List<TMP_InputField> activeInputFields = new List<TMP_InputField>();

	public bool transitioning = false;

	public float holdTimer = 0f;

	public float holdStartDelay = 0.5f;

	public float holdInputDelay = 0.2f;

	public bool holding = false;

	public int hideUICounter = 0;

	public float holdDuration = 1.2f;

	public int lastInputPlayerIndex = 0;

	private void Start()
	{
		ctrl = S.I.batCtrl;
		consoleView = S.I.consoleView;
		conCtrl = S.I.conCtrl;
		credCtrl = S.I.credCtrl;
		deCtrl = S.I.deCtrl;
		idCtrl = S.I.idCtrl;
		optCtrl = S.I.optCtrl;
		runCtrl = S.I.runCtrl;
		tutCtrl = S.I.tutCtrl;
	}

	private void Update()
	{
		Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
		Rewired.Player rewiredPlayer2 = RunCtrl.GetRewiredPlayer(1);
		lastInputPlayerIndex = 0;
		if (conCtrl.IsRewiredBindingInProgress(0) || conCtrl.IsRewiredBindingInProgress(1) || rewiredPlayer == null || ControllerDisconnectCtrl.controllerDisconnectInProgress || UIInputFieldVirtualKeyboard.inputCaptureInProgress)
		{
			return;
		}
		if (activeInputFields.Count > 0 && !rewiredPlayer.GetButtonDown("Gameplay_Up") && !rewiredPlayer.GetButtonDown("Gameplay_Down") && !rewiredPlayer.GetButton("Gameplay_Up") && !rewiredPlayer.GetButton("Gameplay_Down"))
		{
			holding = false;
		}
		else
		{
			if (consoleView.viewContainer.activeSelf)
			{
				return;
			}
			if (S.I.DEVELOPER_TOOLS)
			{
				if (rewiredPlayer.GetButtonDown("UI_HUD"))
				{
					CycleHiddenUI();
				}
				if (rewiredPlayer.GetButtonDown("UI_Deck"))
				{
					runCtrl.worldBar.detailPanel.gameObject.SetActive(!runCtrl.worldBar.detailPanel.gameObject.activeSelf);
					foreach (DuelDisk duelDisk in deCtrl.duelDisks)
					{
						foreach (CastSlot castSlot in duelDisk.castSlots)
						{
							castSlot.inputIcon.gameObject.SetActive(runCtrl.worldBar.detailPanel.gameObject.activeSelf);
						}
						duelDisk.cardGrid.cardCounter.gameObject.SetActive(runCtrl.worldBar.detailPanel.gameObject.activeSelf);
					}
					optCtrl.settingsPane.spellLabels = 1;
					optCtrl.settingsPane.ClickSpellLabels();
					optCtrl.settingsPane.aimMarkerEnabled = 1;
					optCtrl.settingsPane.ClickAimMarker();
				}
				if (rewiredPlayer.GetButtonDown("UI_Field"))
				{
					ToggleGreenScreen();
				}
				if (rewiredPlayer.GetButtonDown("Debug"))
				{
					if (!S.I.debugLayover.enabled)
					{
						S.I.debugLayover.enabled = true;
					}
					S.I.debugLayover.Toggle();
				}
				if (rewiredPlayer.GetButtonDown("DebugLog"))
				{
					if (!S.I.debugLayover.logger.enabled)
					{
						S.I.debugLayover.logger.enabled = true;
					}
					S.I.debugLayover.logger.Toggle();
				}
				if (rewiredPlayer.GetButtonDown("Autoplay"))
				{
					optCtrl.autoPlayRunning = !optCtrl.autoPlayRunning;
				}
				if (rewiredPlayer.GetButton("Cheat_Mod") && rewiredPlayer.GetButtonDown("CheatMenu") && S.I.EDITION != Edition.DemoLive)
				{
					cheatCtrl.Toggle();
				}
				if (rewiredPlayer.GetButtonDown("Pause"))
				{
					if (BC.GTimeScale == 0f)
					{
						BC.GTimeScale = 1f;
					}
					else
					{
						BC.GTimeScale = 0f;
					}
				}
				if (rewiredPlayer.GetButtonDown("RedrawLoot") && IsActivePanel(Nav.PostBattle))
				{
					ctrl.poCtrl.StartLootOptions(RewardType.Loot);
				}
				if (rewiredPlayer.GetButtonDown("RedrawLevelUp") && IsActivePanel(Nav.PostBattle))
				{
					ctrl.poCtrl.rewardCardList.Clear();
					ctrl.poCtrl.ClearAndHideCards();
					ctrl.poCtrl.StartLevelUpOptions();
				}
				if (rewiredPlayer.GetButtonDown("SingleFrameStep"))
				{
					StartCoroutine(StepOneFrame());
				}
				if (rewiredPlayer.GetButtonDown("Reload"))
				{
					SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				}
			}
			if (rewiredPlayer.GetButtonDown("Menu"))
			{
				if (activeNavPanels.Contains(optCtrl))
				{
					ClickBackButton();
				}
				else if (IsActivePanel(S.I.coOpCtrl))
				{
					optCtrl.ClickQuit();
				}
				else if (!ctrl.mainCtrl.transitioning)
				{
					optCtrl.Open();
				}
			}
			if (tutCtrl.tutorialInProgress)
			{
				if (!activeNavPanels.Contains(optCtrl) && rewiredPlayer.GetButtonDown("Shuffle"))
				{
					tutCtrl.SkipTutorial();
					return;
				}
				if (rewiredPlayer.GetButtonDown("TopNav"))
				{
					return;
				}
			}
			if (transitioning || PostCtrl.transitioning)
			{
				return;
			}
			if (rewiredPlayer.GetButtonDown("TopNav"))
			{
				idCtrl.ToggleDeckShop();
			}
			else if (rewiredPlayer.GetButtonDown("Back"))
			{
				ClickBackButton();
			}
			else if (Input.GetMouseButtonDown(1))
			{
				ClickBackButton();
			}
			else if (rewiredPlayer.GetButtonDown("ChooseZone"))
			{
				bool chooseZoneBindedToAccept = false;
				if (rewiredPlayer.GetButtonDown("Accept"))
				{
					chooseZoneBindedToAccept = true;
				}
				ToggleWorldBar(chooseZoneBindedToAccept);
			}
			else if (rewiredPlayer.GetButton("RemoveSpell") && IsActivePanel(credCtrl))
			{
				credCtrl.SkipCurrentCredits();
			}
			if (IsActivePanel(ctrl.poCtrl) && rewiredPlayer.GetButton("Shuffle"))
			{
				ctrl.poCtrl.HoldContinue();
			}
			if (!focusedButton || activeNavPanels.Count <= 0)
			{
				return;
			}
			if (rewiredPlayer.GetButtonDown("UI_Up"))
			{
				focusedButton.Up();
				Cursor.visible = false;
				mouseActive = false;
			}
			else if (rewiredPlayer.GetButtonDown("UI_Down"))
			{
				focusedButton.Down();
				Cursor.visible = false;
				mouseActive = false;
			}
			else if (rewiredPlayer.GetButtonDown("UI_Left"))
			{
				focusedButton.Left();
				Cursor.visible = false;
				mouseActive = false;
			}
			else if (rewiredPlayer.GetButtonDown("UI_Right"))
			{
				focusedButton.Right();
				Cursor.visible = false;
				mouseActive = false;
			}
			else if (rewiredPlayer.GetButtonDown("Accept"))
			{
				focusedButton.OnAcceptPress();
				Cursor.visible = false;
				mouseActive = false;
			}
			else if (rewiredPlayer.GetButtonDown("Back"))
			{
				focusedButton.OnBackPress();
			}
			else if (Input.GetMouseButtonDown(1))
			{
				focusedButton.OnBackPress();
			}
			else if (rewiredPlayer.GetButtonDown("ChooseZone"))
			{
				focusedButton.ChooseZonePress();
			}
			else if (rewiredPlayer.GetButtonDown("Shuffle"))
			{
				focusedButton.OnShufflePress();
			}
			else if (rewiredPlayer.GetButtonDown("Weapon"))
			{
				focusedButton.OnWeaponPress();
			}
			else if (rewiredPlayer.GetButtonUp("Weapon"))
			{
				focusedButton.OnWeaponRelease();
			}
			else if (rewiredPlayer.GetButton("Weapon"))
			{
				focusedButton.OnWeaponHold();
			}
			else if (rewiredPlayer.GetButton("Accept"))
			{
				focusedButton.OnAcceptHold();
			}
			else if (rewiredPlayer.GetButton("UI_Up"))
			{
				if (CheckHold())
				{
					focusedButton.Up();
				}
			}
			else if (rewiredPlayer.GetButton("UI_Down"))
			{
				if (CheckHold())
				{
					focusedButton.Down();
				}
			}
			else if (rewiredPlayer.GetButton("UI_Left"))
			{
				if (CheckHold())
				{
					focusedButton.Left();
				}
			}
			else if (rewiredPlayer.GetButton("UI_Right"))
			{
				if (CheckHold())
				{
					focusedButton.Right();
				}
			}
			else
			{
				holdTimer = 0f;
				holding = false;
			}
			if ((bool)focusedButton)
			{
				if (rewiredPlayer.GetButtonDown("UpgradeSpell"))
				{
					focusedButton.UpgradeSpellPress();
				}
				else if (rewiredPlayer.GetButtonDown("RemoveSpell"))
				{
					focusedButton.RemoveSpellPress();
				}
				else if (rewiredPlayer.GetButton("RemoveSpell"))
				{
					focusedButton.RemoveSpellHold();
				}
				else if (rewiredPlayer.GetButton("UpgradeSpell"))
				{
					focusedButton.UpgradeSpellHold();
				}
				if (rewiredPlayer.GetButtonDown("Outfit"))
				{
					focusedButton.OnOutfitPress();
				}
			}
			lastInputPlayerIndex = 1;
			if (rewiredPlayer2 != null && playerTwoFocusedButton != null)
			{
				if (rewiredPlayer2.GetButtonDown("UI_Up"))
				{
					playerTwoFocusedButton.Up();
				}
				else if (rewiredPlayer2.GetButtonDown("UI_Down"))
				{
					playerTwoFocusedButton.Down();
				}
				else if (rewiredPlayer2.GetButtonDown("UI_Left"))
				{
					playerTwoFocusedButton.Left();
				}
				else if (rewiredPlayer2.GetButtonDown("UI_Right"))
				{
					playerTwoFocusedButton.Right();
				}
				else if (rewiredPlayer2.GetButtonDown("Accept"))
				{
					playerTwoFocusedButton.OnAcceptPress();
				}
				else if (rewiredPlayer2.GetButtonDown("Back"))
				{
					playerTwoFocusedButton.OnBackPress();
				}
				else if (rewiredPlayer2.GetButtonDown("ChooseZone"))
				{
					playerTwoFocusedButton.ChooseZonePress();
				}
				else if (rewiredPlayer2.GetButtonDown("Shuffle"))
				{
					playerTwoFocusedButton.OnShufflePress();
				}
				else if (rewiredPlayer2.GetButtonDown("Weapon"))
				{
					playerTwoFocusedButton.OnWeaponPress();
				}
				else if (rewiredPlayer2.GetButtonUp("Weapon"))
				{
					playerTwoFocusedButton.OnWeaponRelease();
				}
				else if (rewiredPlayer2.GetButton("Weapon"))
				{
					playerTwoFocusedButton.OnWeaponHold();
				}
			}
		}
	}

	private bool CheckHold()
	{
		holdTimer += Time.unscaledDeltaTime;
		if ((holdTimer > holdStartDelay || holding) && holdTimer > holdInputDelay)
		{
			holdTimer = 0f;
			holding = true;
			return true;
		}
		return false;
	}

	public void ToggleWorldBar(bool chooseZoneBindedToAccept = false)
	{
		if (((!PostCtrl.transitioning && activeNavPanels.Count < 1) || IsActivePanel(runCtrl.worldBar) || (!PostCtrl.transitioning && activeNavPanels.Count < 2 && activeNavPanels.Contains(ctrl.poCtrl))) && (!chooseZoneBindedToAccept || !runCtrl.worldBar.open) && (!chooseZoneBindedToAccept || ctrl.GameState != GState.Loot) && (!chooseZoneBindedToAccept || ctrl.GameState != GState.Experience))
		{
			runCtrl.worldBar.Toggle();
		}
	}

	public bool IsActivePanel(NavPanel navPanel)
	{
		if (activeNavPanels.Count > 0 && activeNavPanels[0] == navPanel)
		{
			return true;
		}
		return false;
	}

	public bool IsActivePanel(Nav navState = Nav.Default)
	{
		if (activeNavPanels.Count > 0)
		{
			if (navState == Nav.Default)
			{
				return true;
			}
			if (activeNavPanels[0] == GetNavPanel(navState))
			{
				return true;
			}
		}
		return false;
	}

	public void AddActivePanel(Nav navState)
	{
		activeNavPanels.Insert(0, GetNavPanel(navState));
	}

	public void AddActivePanel(NavPanel navPanel)
	{
		activeNavPanels.Insert(0, navPanel);
	}

	public void RemoveActivePanel(Nav navState)
	{
		activeNavPanels.Remove(GetNavPanel(navState));
	}

	public void RemoveActivePanel(NavPanel navPanel)
	{
		activeNavPanels.Remove(navPanel);
	}

	private NavPanel GetNavPanel(Nav navState)
	{
		switch (navState)
		{
		case Nav.Main:
			return S.I.mainCtrl;
		case Nav.Deck:
			return deCtrl.deckScreen;
		case Nav.HeroSelect:
			return S.I.heCtrl;
		case Nav.Options:
			return optCtrl;
		case Nav.GameOver:
			return ctrl.gameOverPane;
		case Nav.WorldBar:
			return runCtrl.worldBar;
		case Nav.PostBattle:
			return S.I.poCtrl;
		default:
			return null;
		}
	}

	public bool NoTransitions()
	{
		if (!transitioning && !PostCtrl.transitioning && !ctrl.mainCtrl.transitioning)
		{
			return true;
		}
		return false;
	}

	public void ClickBackButton(bool overrideFocusedButton = false)
	{
		if (IsActivePanel() && (!focusedButton || !focusedButton.back || overrideFocusedButton))
		{
			activeNavPanels[0].ClickBack();
		}
	}

	private IEnumerator StepOneFrame()
	{
		if (BC.GTimeScale == 0f)
		{
			BC.GTimeScale = 1f;
		}
		Debug.Log(Time.frameCount);
		yield return new WaitForFixedUpdate();
		BC.GTimeScale = 0f;
	}

	public void CycleHiddenUI()
	{
		hideUICounter++;
		bool active = true;
		bool active2 = true;
		bool active3 = true;
		if (hideUICounter == 1)
		{
			active = false;
		}
		else if (hideUICounter == 2)
		{
			active = false;
			active2 = false;
		}
		else if (hideUICounter == 3)
		{
			active = false;
			active2 = false;
			active3 = false;
		}
		else if (hideUICounter == 4)
		{
			hideUICounter = 0;
		}
		runCtrl.worldBar.detailPanel.gameObject.SetActive(active);
		runCtrl.progressBar.gameObject.SetActive(active);
		runCtrl.worldBar.title.gameObject.SetActive(active);
		if (deCtrl.duelDisks.Count > 0)
		{
			foreach (DuelDisk duelDisk in deCtrl.duelDisks)
			{
				foreach (CastSlot castSlot in duelDisk.castSlots)
				{
					castSlot.inputIcon.gameObject.SetActive(active);
					castSlot.dmgLabel.gameObject.SetActive(active);
				}
			}
		}
		ctrl.sp.enemyNameGrid.gameObject.SetActive(active);
		idCtrl.moneyTextBattle.gameObject.SetActive(active);
		ctrl.poCtrl.previewButton.gameObject.SetActive(false);
		runCtrl.worldBar.confirmButton.SetActive(false);
		runCtrl.worldBar.closeButton.gameObject.SetActive(false);
		ctrl.centralMessageContainer.SetActive(active2);
		idCtrl.canvas.enabled = active2;
		foreach (DuelDisk duelDisk2 in deCtrl.duelDisks)
		{
			duelDisk2.manaBar.gameObject.SetActive(active2);
			duelDisk2.castSlotsGrid.gameObject.SetActive(active2);
			duelDisk2.cardtridgeSlotContainer.gameObject.SetActive(active2);
			foreach (DiskReference diskRef in duelDisk2.diskRefs)
			{
				diskRef.gameObject.SetActive(active2);
			}
			deCtrl.artGrid.gameObject.SetActive(active2);
			duelDisk2.cardGrid.transform.parent.gameObject.SetActive(active2);
		}
		foreach (Being currentBeing in ctrl.ti.mainBattleGrid.currentBeings)
		{
			currentBeing.beingStatsPanel.gameObject.SetActive(active3);
		}
		ctrl.btnCtrl.gameObject.SetActive(active3);
	}

	private void ToggleGreenScreen()
	{
		if (S.I.tiCtrl.mainBattleGrid.gridContainer.activeInHierarchy)
		{
			S.I.tiCtrl.mainBattleGrid.gridContainer.SetActive(false);
			S.I.bgCtrl.greenScreen.gameObject.SetActive(true);
		}
		else
		{
			S.I.tiCtrl.mainBattleGrid.gridContainer.SetActive(true);
			S.I.bgCtrl.greenScreen.gameObject.SetActive(false);
		}
	}

	public void SetFocus(UIButton newFocusedButton, int playerNum = 0)
	{
		if (!newFocusedButton)
		{
			return;
		}
		switch (playerNum)
		{
		case 0:
			if ((bool)focusedButton)
			{
				focusedButton.UnFocus();
			}
			focusedButton = newFocusedButton;
			focusedButton.Focus();
			break;
		case 1:
			if ((bool)playerTwoFocusedButton)
			{
				playerTwoFocusedButton.UnFocus();
			}
			playerTwoFocusedButton = newFocusedButton;
			playerTwoFocusedButton.Focus(playerNum);
			break;
		}
	}

	private void FocusButton(UIButton oldFocusedButton, UIButton newFocusedButton)
	{
		if ((bool)oldFocusedButton)
		{
			oldFocusedButton.UnFocus();
		}
		oldFocusedButton = newFocusedButton;
		oldFocusedButton.Focus();
	}

	public void SetFocus(GameObject newFocusedGO)
	{
		SetFocus(newFocusedGO.GetComponent<UIButton>());
	}

	public void SetFocus(Transform newFocusedTransform)
	{
		SetFocus(newFocusedTransform.GetComponent<UIButton>());
	}

	public void RemoveFocus()
	{
		if ((bool)focusedButton)
		{
			focusedButton.UnFocus();
		}
		focusedButton = null;
	}

	public void AddInputField(TMP_InputField inputField)
	{
		activeInputFields.Add(inputField);
	}

	public void RemoveInputField(TMP_InputField inputField)
	{
		activeInputFields.Remove(inputField);
	}
}
