using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class OptionCtrl : NavPanel
{
	public bool showGUI = false;

	public SettingsPane settingsPane;

	private string saveData;

	public SlideBody controlsSB;

	public SlideBody settingsSB;

	public SlideBody creditsSB;

	public SlideBody bugSB;

	public SlideBody demoEndSB;

	public SlideBody demoLiveEndSB;

	public GameObject steamButton;

	public TMP_Text closeAbandonButtonText;

	public NavButton shutdownButton;

	public Transform navButtonGrid;

	public Transform selectorBox;

	public AudioClip pauseSound;

	public AudioClip resumeSound;

	public Image heroSplash;

	public Image selectorPointLeft;

	public Image selectorPointRight;

	public MenuFollower selectorLeft;

	public MenuFollower selectorRight;

	public Transform selectorsParent;

	public float selectorGap = 6f;

	public bool hideSelectors = true;

	public VideoPlayer idleVidPlayer;

	public GameObject idleVidContainer;

	public float idleVidWaitTime;

	private float savedWaitTime = 10f;

	public Renderer pressToPlay;

	private Coroutine co_HideIdleVideo;

	public bool autoPlayRunning = false;

	private bool abandonRunCheckShown = false;

	private bool savedTransitioning = false;

	private bool savedPoCtrlTransition = false;

	private bool transitioning = false;

	public bool stillInStartup = false;

	private float savedTimeScale = 0f;

	public bool quitButtonEnabled = false;

	private BC ctrl;

	private ControlsCtrl conCtrl;

	private MainCtrl mainCtrl;

	private MusicCtrl muCtrl;

	private RunCtrl runCtrl;

	private TutorialCtrl tutCtrl;

	private void Start()
	{
		ctrl = S.I.batCtrl;
		btnCtrl = S.I.btnCtrl;
		conCtrl = S.I.conCtrl;
		mainCtrl = S.I.mainCtrl;
		muCtrl = S.I.muCtrl;
		runCtrl = S.I.runCtrl;
		tutCtrl = S.I.tutCtrl;
		savedWaitTime = idleVidWaitTime;
		idleVidContainer.gameObject.SetActive(false);
		settingsPane.StartSettings();
		StartCoroutine(TestAuto());
		if (S.I.EDITION == Edition.DemoLive)
		{
			StartVideo();
		}
		steamButton.SetActive(true);
	}

	private void Update()
	{
		if (hideSelectors)
		{
			selectorPointLeft.transform.position = new Vector3(-400f, selectorPointLeft.transform.transform.position.y, selectorPointLeft.transform.transform.position.z);
			selectorPointRight.transform.position = new Vector3(400f, selectorPointRight.transform.position.y, selectorPointRight.transform.position.z);
		}
		else if (btnCtrl.IsActivePanel(this))
		{
			if ((bool)btnCtrl.focusedButton && (bool)btnCtrl.focusedButton.tmpText)
			{
				selectorPointLeft.transform.position = btnCtrl.focusedButton.transform.position - Vector3.right * (btnCtrl.focusedButton.tmpText.textBounds.size.x / 2f + selectorGap);
				selectorPointRight.transform.position = btnCtrl.focusedButton.transform.position + Vector3.right * (btnCtrl.focusedButton.tmpText.textBounds.size.x / 2f + selectorGap);
			}
		}
		else
		{
			if (!btnCtrl.activeNavPanels.Contains(this))
			{
				return;
			}
			NavPanel navPanel = btnCtrl.activeNavPanels[0];
			if (navPanel.title != null)
			{
				if (navPanel == conCtrl)
				{
					selectorPointLeft.transform.position = navPanel.title.transform.position - Vector3.right * (navPanel.title.rectTransform.sizeDelta.x / 2f + selectorGap);
					selectorPointRight.transform.position = navPanel.title.transform.position + Vector3.right * (navPanel.title.textBounds.size.x - navPanel.title.rectTransform.sizeDelta.x / 2f + selectorGap);
				}
				else
				{
					selectorPointLeft.transform.position = navPanel.title.transform.position - Vector3.right * (navPanel.title.textBounds.size.x / 2f + selectorGap);
					selectorPointRight.transform.position = navPanel.title.transform.position + Vector3.right * (navPanel.title.textBounds.size.x / 2f + selectorGap);
				}
			}
		}
	}

	public void StartVideo()
	{
		HideAndCountdownToIdleVid();
		idleVidWaitTime = 6f;
	}

	public void HideAndCountdownToIdleVid()
	{
		idleVidWaitTime = savedWaitTime;
		if (S.I.scene == GScene.DemoLive)
		{
			if (idleVidContainer.activeInHierarchy)
			{
				idleVidContainer.gameObject.SetActive(false);
				idleVidPlayer.Pause();
			}
			if (co_HideIdleVideo != null)
			{
				StopCoroutine(co_HideIdleVideo);
				co_HideIdleVideo = null;
			}
			if (co_HideIdleVideo == null)
			{
				co_HideIdleVideo = StartCoroutine(HideIdleVideo());
			}
		}
	}

	private IEnumerator HideIdleVideo()
	{
		if (S.I.scene == GScene.DemoLive)
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForSecondsRealtime(idleVidWaitTime);
			string filePath = Path.Combine(Application.streamingAssetsPath, "idleVid.webm");
			idleVidPlayer.url = filePath;
			muCtrl.PauseIntroLoop();
			idleVidPlayer.Prepare();
			WaitForSecondsRealtime waitTime = new WaitForSecondsRealtime(3f);
			if (!idleVidPlayer.isPrepared)
			{
				yield return waitTime;
			}
			ctrl.Restart();
			idleVidContainer.gameObject.SetActive(true);
			idleVidPlayer.Play();
			idleVidContainer.transform.localScale = new Vector2(ScalableCamera.screenSize.y * ScalableCamera.PIXELS_TO_UNITS_ROUNDED / ScalableCamera.calculatedPixelHeight, ScalableCamera.screenSize.y * ScalableCamera.PIXELS_TO_UNITS_ROUNDED / ScalableCamera.calculatedPixelHeight);
			idleVidPlayer.targetMaterialRenderer.sortingLayerName = "UI";
			idleVidPlayer.targetMaterialRenderer.sortingOrder = 97;
			pressToPlay.sortingLayerName = "UI";
			pressToPlay.sortingOrder = 98;
		}
	}

	private IEnumerator StepOneFrame()
	{
		if (BC.GTimeScale == 0f)
		{
			BC.GTimeScale = 1f;
		}
		UnityEngine.Debug.Log(Time.frameCount);
		yield return new WaitForFixedUpdate();
		BC.GTimeScale = 0f;
	}

	public override void Open()
	{
		StartCoroutine(OpenMenuC(navButtonGrid));
	}

	private IEnumerator OpenMenuC(Transform parentGrid)
	{
		if (transitioning || stillInStartup)
		{
			yield break;
		}
		transitioning = true;
		_003C_003En__0();
		abandonRunCheckShown = false;
		savedTimeScale = Time.timeScale;
		BC.GTimeScale = 0f;
		if (tutCtrl.tutorialPathsInProgress || tutCtrl.tutorialOvsInProgress)
		{
			tutCtrl.videoScreen.gameObject.SetActive(false);
			tutCtrl.videoScreenAlt.gameObject.SetActive(false);
		}
		shutdownButton.gameObject.SetActive(false);
		if (ctrl.GameState == GState.Idle && !ctrl.poCtrl.open && !PostCtrl.transitioning)
		{
			closeAbandonButtonText.text = ScriptLocalization.UI.SAVE_AND_QUIT;
		}
		else if (ctrl.GameState == GState.MainMenu || ctrl.GameState == GState.HeroSelect || ctrl.GameState == GState.GameOver || ctrl.GameState == GState.Unlock)
		{
			closeAbandonButtonText.text = ScriptLocalization.UI.Options_Close;
			if (quitButtonEnabled)
			{
				shutdownButton.gameObject.SetActive(true);
			}
		}
		else if (ctrl.GameState == GState.PvPSetup || ctrl.heCtrl.gameMode == GameMode.PvP)
		{
			closeAbandonButtonText.text = ScriptLocalization.UI.QUIT;
		}
		else if (ctrl.credCtrl.creditsOngoing || ctrl.GameState == GState.CG)
		{
			closeAbandonButtonText.text = ScriptLocalization.UI.Options_SkipScene;
		}
		else
		{
			closeAbandonButtonText.text = ScriptLocalization.UI.Options_AbandonRun;
		}
		savedPoCtrlTransition = PostCtrl.transitioning;
		savedTransitioning = btnCtrl.transitioning;
		S.I.PlayOnce(pauseSound);
		ctrl.AddControlBlocks(Block.OptionsPanel);
		muCtrl.OptionsOpened();
		btnCtrl.transitioning = true;
		yield return new WaitForEndOfFrame();
		foreach (Transform child in parentGrid)
		{
			if (child.gameObject.activeSelf)
			{
				yield return new WaitForSecondsRealtime(0.03f);
				if ((bool)child)
				{
					child.GetComponent<Animator>().SetBool("visible", true);
				}
			}
		}
		if (btnCtrl.focusedButton != null)
		{
			yield return StartCoroutine(_ResetSelectors(btnCtrl.focusedButton.transform, selectorsParent));
		}
		PostCtrl.transitioning = false;
		hideSelectors = false;
		btnCtrl.transitioning = false;
		transitioning = false;
	}

	public override void Close()
	{
		StartCoroutine(CloseMenuC(navButtonGrid));
	}

	private IEnumerator _ResetSelectors(Transform target, Transform parent)
	{
		selectorBox.SetParent(parent, false);
		selectorPointLeft.transform.position = new Vector3(selectorPointLeft.transform.position.x, target.position.y, selectorPointLeft.transform.position.z);
		selectorPointRight.transform.position = new Vector3(selectorPointRight.transform.position.x, target.position.y, selectorPointRight.transform.position.z);
		selectorLeft.damping = 0f;
		selectorRight.damping = 0f;
		yield return new WaitForSecondsRealtime(0.04f);
		selectorLeft.ResetDamping();
		selectorRight.ResetDamping();
	}

	private IEnumerator CloseMenuC(Transform parentGrid, bool restartAfter = false)
	{
		if (transitioning || btnCtrl.transitioning)
		{
			yield break;
		}
		transitioning = true;
		btnCtrl.transitioning = true;
		anim.SetBool("visible", false);
		hideSelectors = true;
		btnCtrl.RemoveFocus();
		yield return new WaitForEndOfFrame();
		foreach (Transform child in parentGrid)
		{
			if (child.gameObject.activeSelf)
			{
				yield return new WaitForSecondsRealtime(0.02f);
				if ((bool)child)
				{
					child.GetComponent<Animator>().SetBool("visible", false);
				}
			}
		}
		yield return new WaitForSecondsRealtime(0.2f);
		muCtrl.OptionsClosed();
		if (tutCtrl.tutorialPathsInProgress || tutCtrl.tutorialOvsInProgress)
		{
			tutCtrl.videoScreen.gameObject.SetActive(true);
			tutCtrl.videoScreenAlt.gameObject.SetActive(true);
		}
		S.I.PlayOnce(resumeSound);
		ctrl.RemoveControlBlocksNextFrame(Block.OptionsPanel);
		_003C_003En__1();
		if (ctrl.GameState == GState.MainMenu && btnCtrl.activeNavPanels.Count == 0)
		{
			mainCtrl.Open();
		}
		btnCtrl.transitioning = savedTransitioning;
		PostCtrl.transitioning = savedPoCtrlTransition;
		if (restartAfter)
		{
			ctrl.Restart();
			if (conCtrl.co_reconnect != null)
			{
				conCtrl.searchingForReconnection = false;
			}
		}
		else if (conCtrl.co_reconnect == null)
		{
			BC.GTimeScale = 1f;
			Time.timeScale = savedTimeScale;
		}
		transitioning = false;
	}

	public void ClickQuit()
	{
		if (PostCtrl.transitioning || btnCtrl.transitioning)
		{
			return;
		}
		if (S.I.scene == GScene.DemoLive)
		{
			Application.Quit();
			return;
		}
		if (ctrl.GameState == GState.MainMenu || ctrl.GameState == GState.HeroSelect || ctrl.GameState == GState.GameOver || ctrl.GameState == GState.Unlock)
		{
			Close();
			return;
		}
		if (ctrl.credCtrl.creditsOngoing || ctrl.GameState == GState.CG)
		{
			SkipCurrentScene();
			return;
		}
		if ((ctrl.GameState != GState.Idle || ctrl.poCtrl.open) && !abandonRunCheckShown)
		{
			TMP_Text tMP_Text = closeAbandonButtonText;
			tMP_Text.text = tMP_Text.text + " " + ScriptLocalization.UI.Options_AreYouSure;
			abandonRunCheckShown = true;
			return;
		}
		if (ctrl.GameState == GState.Idle && !ctrl.poCtrl.open && !savedPoCtrlTransition)
		{
			runCtrl.SaveRun();
		}
		else
		{
			if (!abandonRunCheckShown)
			{
				TMP_Text tMP_Text2 = closeAbandonButtonText;
				tMP_Text2.text = tMP_Text2.text + " " + ScriptLocalization.UI.Options_AreYouSure;
				abandonRunCheckShown = true;
				return;
			}
			runCtrl.DeleteRun();
		}
		originButton = null;
		StartCoroutine(CloseMenuC(navButtonGrid, true));
	}

	public void Report(string eventName, bool endScreen)
	{
		Ana.CustomEvent(eventName, new Dictionary<string, object>
		{
			{ "endScreen", endScreen },
			{
				"lifetime_battles",
				SaveDataCtrl.Get("LifetimeBattles", 0)
			},
			{
				"time_elapsed",
				Time.time
			},
			{
				"device_unique_id",
				SystemInfo.deviceUniqueIdentifier
			}
		});
	}

	public void ClickDiscord(bool endScreen)
	{
		Application.OpenURL("https://discord.gg/OSFE");
		Report("discord_clicked", endScreen);
	}

	public void ClickFacebook(bool endScreen)
	{
		Application.OpenURL("https://www.facebook.com/OneStepFromEden/");
		Report("facebook_clicked", endScreen);
	}

	public void ClickTwitter(bool endScreen)
	{
		Application.OpenURL("https://twitter.com/OneStepFromEden");
		Report("twitter_clicked", endScreen);
	}

	public void ClickSurvey(bool endScreen)
	{
		Application.OpenURL("https://goo.gl/forms/W5LfbnYfDP4GXq8G2");
		Report("survey_clicked", endScreen);
	}

	public void ClickTwitterShare(bool endScreen)
	{
		Application.OpenURL("https://ctt.ac/ALwDK");
		Report("twitter_clicked", endScreen);
	}

	public void ClickTwitterPersonal()
	{
		Application.OpenURL("https://twitter.com/ThomasMoonKang");
		Report("twitterPersonal_clicked", false);
	}

	public void ClickTwitterSteelPlus()
	{
		Application.OpenURL("https://twitter.com/STEEL_PLUS");
		Report("twitter_steelPlus_clicked", false);
	}

	public void ClickItchio(bool endScreen)
	{
		Application.OpenURL("https://tmkang.itch.io/osfe");
		Report("itchio_clicked", endScreen);
	}

	public void ClickGamejolt(bool endScreen)
	{
		Application.OpenURL("https://gamejolt.com/games/OSFE/366092");
		Report("gamejolt_clicked", endScreen);
	}

	public void ClickKoFi(bool endScreen)
	{
		Application.OpenURL("https://Ko-fi.com/tmkang");
		Report("kofi_clicked", endScreen);
	}

	public void ClickWebsite(bool endScreen)
	{
		Application.OpenURL("https://www.onestepfromeden.com/");
		Report("website_clicked", endScreen);
	}

	public void ClickSteam(bool endScreen)
	{
		Application.OpenURL("https://store.steampowered.com/app/960690/One_Step_From_Eden/");
		Report("website_clicked", endScreen);
	}

	public void ClickSteamDemo(bool endScreen)
	{
		Application.OpenURL("https://store.steampowered.com/app/1007020/");
		Report("website_clicked", endScreen);
	}

	public void ClickKickstarter(bool endScreen)
	{
		Application.OpenURL("https://www.onestepfromeden.com/KS");
		Report("kickstarter_clicked", endScreen);
	}

	public void ClickSeed()
	{
		TextEditor textEditor = new TextEditor();
		textEditor.text = ctrl.runCtrl.currentRun.seed;
		textEditor.SelectAll();
		textEditor.Copy();
	}

	private void OnApplicationQuit()
	{
	}

	private bool BossIsDowned()
	{
		bool result = false;
		foreach (Cpu currentEnemy in ctrl.ti.mainBattleGrid.currentEnemies)
		{
			if ((bool)currentEnemy.GetComponent<Boss>() && currentEnemy.GetComponent<Boss>().downed)
			{
				result = true;
				break;
			}
		}
		if (!S.I.SPARE_BOSSES_AUTO)
		{
			result = false;
		}
		return result;
	}

	private IEnumerator TestAuto()
	{
		yield return new WaitForSeconds(2f);
		bool alwaysExecute = !S.I.SPARE_BOSSES_AUTO;
		while (true)
		{
			if (autoPlayRunning)
			{
				if ((bool)ctrl.currentPlayer)
				{
					ctrl.deCtrl.EquipWep("GodGun");
					yield return new WaitForSeconds(0.4f);
					if ((bool)runCtrl.currentZoneDot && !runCtrl.worldBar.open && !btnCtrl.activeNavPanels.Contains(ctrl.poCtrl))
					{
						btnCtrl.ToggleWorldBar();
					}
					yield return new WaitForSeconds(0.4f);
					if (alwaysExecute && (bool)ctrl.currentPlayer && ctrl.currentPlayer.battleGrid.currentEnemies.Count > 0)
					{
						if (!BossIsDowned())
						{
							ctrl.currentPlayer.QueueAction(InputAction.Weapon);
						}
					}
					else if (btnCtrl.activeNavPanels.Count > 0 && (bool)btnCtrl.focusedButton && !PostCtrl.transitioning && !btnCtrl.transitioning && ctrl.GameState != GState.Battle)
					{
						btnCtrl.focusedButton.OnAcceptPress();
						yield return new WaitForSeconds(0.4f);
					}
					for (int i = 0; i < 5; i++)
					{
						yield return new WaitForSeconds(0.1f);
						if (!BossIsDowned() && (bool)ctrl.currentPlayer && !ctrl.currentPlayer.dead && ctrl.currentPlayer.controlBlocks.Count < 1)
						{
							if (!Utils.RandomBool(4))
							{
								ctrl.currentPlayer.QueueAction(InputAction.Weapon);
							}
							else if (Utils.RandomBool(2))
							{
								ctrl.currentPlayer.QueueAction(InputAction.FireOne);
							}
							else
							{
								ctrl.currentPlayer.QueueAction(InputAction.FireTwo);
							}
							yield return new WaitForSeconds(0.2f);
						}
					}
					yield return new WaitForSeconds(0.4f);
				}
				else
				{
					if ((bool)btnCtrl.focusedButton && !btnCtrl.transitioning && !PostCtrl.transitioning && btnCtrl.focusedButton != null && btnCtrl.activeNavPanels.Count > 0 && ctrl.GameState != GState.Battle)
					{
						btnCtrl.focusedButton.OnAcceptPress();
						yield return new WaitForSeconds(0.4f);
					}
					yield return new WaitForSeconds(0.1f);
				}
			}
			else
			{
				yield return null;
			}
		}
	}

	public void OpenPanel(NavPanel navPanel)
	{
		StartCoroutine(_OpenPanel(navPanel));
	}

	private IEnumerator _OpenPanel(NavPanel navPanel)
	{
		if (!btnCtrl.transitioning)
		{
			btnCtrl.transitioning = true;
			hideSelectors = true;
			yield return new WaitForSecondsRealtime(0.1f);
			navPanel.Open();
			yield return new WaitForSecondsRealtime(0.25f);
			yield return StartCoroutine(_ResetSelectors(navPanel.title.transform, navPanel.transform));
			hideSelectors = false;
			btnCtrl.transitioning = false;
		}
	}

	public void ClosePanel(NavPanel navPanel)
	{
		StartCoroutine(_ClosePanel(navPanel));
	}

	private IEnumerator _ClosePanel(NavPanel navPanel)
	{
		btnCtrl.transitioning = true;
		hideSelectors = true;
		yield return new WaitForSecondsRealtime(0.1f);
		navPanel.CloseBase();
		yield return new WaitForSecondsRealtime(0.15f);
		if ((bool)btnCtrl.focusedButton)
		{
			yield return StartCoroutine(_ResetSelectors(btnCtrl.focusedButton.transform, selectorsParent));
		}
		hideSelectors = false;
		btnCtrl.transitioning = false;
	}

	public void SkipCurrentScene()
	{
		if (ctrl.credCtrl.creditsOngoing)
		{
			ctrl.credCtrl.SkipCurrentCredits();
			Close();
		}
		else if (ctrl.GameState == GState.CG)
		{
			ctrl.cgCtrl.SkipScene();
			Close();
		}
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.Open();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__1()
	{
		base.Close();
	}
}
