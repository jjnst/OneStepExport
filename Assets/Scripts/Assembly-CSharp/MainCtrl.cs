using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainCtrl : NavPanel
{
	public NavButton continueButton;

	public NavButton startButton;

	public NavButton soloButton;

	public NavButton coOpButton;

	public NavButton libraryButton;

	public NavButton optionButton;

	public NavButton modsButton;

	public NavButton quitButton;

	public NavButton profileButton;

	public NavPanel navPanel;

	public Image selectorPointLeft;

	public MenuFollower selectorLeft;

	public Transform mainMenuLeftButtonGrid;

	public Transform mainMenuRightButtonGrid;

	public Animator logo;

	public float selectorGap = 6f;

	public BC ctrl;

	public CameraScript camCtrl;

	public CoOpCtrl coOpCtrl;

	public HeroSelectCtrl heCtrl;

	public MusicCtrl muCtrl;

	public PvPSelectCtrl pvpSelectCtrl;

	public RunCtrl runCtrl;

	public NavPanel modInstallPane;

	public bool transitioning = false;

	public bool startedUp = false;

	public bool quitButtonEnabled = false;

	private Coroutine co_openPanel;

	protected override void Awake()
	{
		base.Awake();
		camCtrl.cameraPane.GetComponent<PanelScaler>().enabled = false;
		camCtrl.cameraPane.GetComponent<RectTransform>().sizeDelta = new Vector2(1600f, 2000f);
	}

	private void Update()
	{
		if ((bool)btnCtrl.focusedButton && btnCtrl.IsActivePanel(this))
		{
			SetSelectorPoint(btnCtrl.focusedButton);
		}
	}

	public void SetSelectorPoint(UIButton uiButton)
	{
		if (!(uiButton == null) && !(uiButton.tmpText == null))
		{
			if (uiButton.tmpText.alignment == TextAlignmentOptions.Left)
			{
				selectorPointLeft.transform.position = uiButton.transform.position - Vector3.right * (uiButton.rect.sizeDelta.x / 2f * uiButton.rect.localScale.x + selectorGap);
			}
			else if (uiButton.tmpText.alignment == TextAlignmentOptions.Right)
			{
				selectorPointLeft.transform.position = uiButton.transform.position - Vector3.right * (uiButton.rect.sizeDelta.x / 2f * uiButton.rect.localScale.x + selectorGap) + Vector3.right * ((uiButton.rect.sizeDelta.x - uiButton.tmpText.textBounds.size.x) * uiButton.rect.localScale.x - selectorGap);
			}
		}
	}

	public void Startup(float delay = 0f, bool flashDark = true, bool pauseIntroLoop = true)
	{
		ctrl.optCtrl.stillInStartup = true;
		PostCtrl.transitioning = true;
		if (pauseIntroLoop)
		{
			muCtrl.PauseIntroLoop();
		}
		ctrl.GameState = GState.MainMenu;
		defaultButton = soloButton;
		startedUp = true;
		StartCoroutine(StartTitle(delay, flashDark, pauseIntroLoop));
	}

	private IEnumerator StartTitle(float delay, bool flashDark, bool introLoopWasPaused)
	{
		S.I.cameraStill = false;
		logo.SetBool("visible", true);
		if (flashDark)
		{
			camCtrl.cameraPane.color = S.I.GetFlashColor(UIColor.BlueDark);
		}
		yield return new WaitUntil(() => SaveDataCtrl.Initialized);
		if (S.I.EDITION == Edition.Full || S.I.EDITION == Edition.QA)
		{
			while (!SteamManager.Initialized && Time.timeSinceLevelLoad < S.maxLoadTime)
			{
				yield return null;
			}
		}
		camCtrl.cameraPane.GetComponent<PanelScaler>().enabled = true;
		camCtrl.CameraChangePos(0, true);
		yield return new WaitForSeconds(delay);
		S.I.CameraStill(UIColor.Clear);
		btnCtrl.RemoveFocus();
		Intro();
		yield return new WaitForSeconds(0.1f);
		if (introLoopWasPaused)
		{
			S.I.muCtrl.PlayTitle();
		}
		S.I.refCtrl.Hide();
		if (runCtrl.LoadRun())
		{
			continueButton.gameObject.SetActive(true);
		}
		else
		{
			continueButton.gameObject.SetActive(false);
		}
		continueButton.anim.SetBool("visible", continueButton.gameObject.activeSelf);
	}

	public void Intro()
	{
		anim.SetBool("Intro", true);
		anim.SetBool("visible", true);
		StartCoroutine(_Intro());
	}

	private IEnumerator _Intro()
	{
		yield return new WaitForEndOfFrame();
		if (!btnCtrl.activeNavPanels.Contains(this))
		{
			Open();
		}
		else
		{
			btnCtrl.SetFocus(defaultButton);
		}
		ctrl.optCtrl.stillInStartup = false;
		foreach (Transform child in mainMenuLeftButtonGrid)
		{
			yield return new WaitForSecondsRealtime(0.05f);
			if ((bool)child && child.gameObject.activeSelf)
			{
				child.GetComponent<Animator>().SetBool("visible", true);
			}
		}
		foreach (Transform child2 in mainMenuRightButtonGrid)
		{
			yield return new WaitForEndOfFrame();
			if ((bool)child2 && child2.gameObject.activeSelf)
			{
				child2.GetComponent<Animator>().SetBool("visible", true);
			}
		}
		ctrl.optCtrl.quitButtonEnabled = true;
		PostCtrl.transitioning = false;
		if (SaveDataCtrl.Get("InstallModsOnStartup", false, true))
		{
			btnCtrl.SetFocus(modsButton);
			modInstallPane.Open();
			while (modInstallPane.slideBody.onScreen)
			{
				yield return null;
			}
		}
	}

	public override void Open()
	{
		base.Open();
		ctrl.GameState = GState.MainMenu;
		StartCoroutine(_OpenMenuC());
	}

	private IEnumerator _OpenMenuC()
	{
		yield return new WaitForEndOfFrame();
		selectorLeft.damping = 0.1f;
		transitioning = false;
		UpdateDiscordData();
	}

	public override void Close()
	{
	}

	public void ForceClose()
	{
		startedUp = false;
		base.Close();
	}

	public void OpenPanel(NavPanel navPanel)
	{
		if (NoTransitions())
		{
			transitioning = true;
			if (co_openPanel != null)
			{
				StopCoroutine(co_openPanel);
				co_openPanel = null;
			}
			if (co_openPanel == null)
			{
				co_openPanel = StartCoroutine(_OpenPanel(navPanel));
			}
		}
	}

	private IEnumerator _OpenPanel(NavPanel navPanel)
	{
		yield return StartCoroutine(_LineProcess());
		if (heCtrl.gameMode == GameMode.CoOp)
		{
			coOpCtrl.Open();
			while (coOpCtrl.controlsProcessing)
			{
				yield return null;
			}
			if (coOpCtrl.aborted)
			{
				while (coOpCtrl.closing)
				{
					yield return null;
				}
				heCtrl.gameMode = GameMode.Solo;
				Open();
				yield break;
			}
		}
		yield return new WaitForSecondsRealtime(0.15f);
		navPanel.Open();
		yield return new WaitForSecondsRealtime(0.3f);
		transitioning = false;
	}

	public void OpenPanelSingpleplayer(NavPanel navPanel)
	{
		if (NoTransitions())
		{
			heCtrl.gameMode = GameMode.Solo;
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
			if (rewiredPlayer != null)
			{
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Gameplay");
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay");
				rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "UIGameplay2");
			}
			HelpIconManager.SetKeyboardAvailable(0, true);
			OpenPanel(navPanel);
		}
	}

	public void OpenPanelCoOp(NavPanel navPanel)
	{
		if (NoTransitions())
		{
			heCtrl.gameMode = GameMode.CoOp;
			OpenPanel(navPanel);
		}
	}

	public void ClickContinueRun()
	{
		if (NoTransitions())
		{
			transitioning = true;
			StartCoroutine(ContinueLoadedCampaign());
		}
	}

	private IEnumerator ContinueLoadedCampaign()
	{
		if (runCtrl.loadedRun != null && runCtrl.loadedRun.coOp)
		{
			heCtrl.gameMode = GameMode.CoOp;
			coOpCtrl.Open();
			while (coOpCtrl.controlsProcessing)
			{
				yield return null;
			}
			if (coOpCtrl.aborted)
			{
				transitioning = false;
				heCtrl.gameMode = GameMode.Solo;
				yield break;
			}
		}
		PostCtrl.transitioning = true;
		ctrl.GameState = GState.Idle;
		yield return StartCoroutine(_LineProcess());
		heCtrl.StartCampaign(true);
		yield return new WaitForSecondsRealtime(0.9f);
		transitioning = false;
	}

	private IEnumerator _LineProcess()
	{
		originButton = null;
		anim.SetBool("Intro", false);
		defaultButton = btnCtrl.focusedButton;
		_003C_003En__0();
		SetSelectorPoint(btnCtrl.focusedButton);
		selectorLeft.damping = 0f;
		yield return null;
		selectorLeft.damping = 0.2f;
		selectorPointLeft.transform.position += Vector3.right * 700f;
		yield return new WaitForSecondsRealtime(0.2f);
	}

	public void ClickPvP()
	{
		if (NoTransitions())
		{
			transitioning = true;
			StartCoroutine(StartPvPRun());
		}
	}

	private IEnumerator StartPvPRun()
	{
		heCtrl.gameMode = GameMode.PvP;
		yield return StartCoroutine(_LineProcess());
		coOpCtrl.Open();
		while (coOpCtrl.controlsProcessing)
		{
			yield return null;
		}
		if (coOpCtrl.aborted)
		{
			while (coOpCtrl.closing)
			{
				yield return null;
			}
			heCtrl.gameMode = GameMode.Solo;
			Open();
		}
		else
		{
			transitioning = false;
			pvpSelectCtrl.Open();
		}
	}

	private bool NoTransitions()
	{
		if (!btnCtrl.transitioning && !transitioning)
		{
			return true;
		}
		return false;
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void UpdateDiscordData()
	{
		string stateText = "";
		string detailText = "";
		string characterName = "";
		runCtrl.disCtrl.RefreshActivityData(stateText, detailText, characterName);
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.Close();
	}
}
