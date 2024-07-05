using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetBundles;
using I2.Loc;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsPane : NavPanel
{
	private const string RESOLUTION_PREF_KEY = "resolution";

	private Resolution[] resolutions;

	private int currentResolutionIndex = 0;

	public TMP_Text displayText;

	public TMP_Text resolutionText;

	public TMP_Text screenshakeText;

	public float screenshake;

	public TMP_Text fpsText;

	public int targetFPS;

	public int spellLabels;

	public TMP_Text spellLabelsText;

	public int aimMarkerEnabled;

	public TMP_Text aimMarkerText;

	public int autoPreviewEnabled;

	public TMP_Text autoPreviewText;

	public Slider sfxSlider;

	public TMP_Text sfxText;

	public float sfxVol;

	public Slider musicSlider;

	public TMP_Text musicText;

	public float musicVol;

	public SettingButton languageBtn;

	public TMP_Text languageText;

	public List<string> languageOptions = new List<string>();

	public int currentLanguageIndex = 0;

	public string savedLanguage;

	public bool dataWasReset = false;

	public int angelModeEnabled;

	public TMP_Text angelModeText;

	public SliderButton angelModeDamageSlider;

	public TMP_Text angelModeDamageText;

	public SliderButton angelModeSpeedSlider;

	public TMP_Text angelModeSpeedText;

	public float angelModeCurrentDamageReduction = 0f;

	public float angelModeCurrentSpeedReduction = 0f;

	public float angelModeMaxReduction = 0f;

	public float angelModeMaxReductionMax = 0.5f;

	public int holdMovementEnabled;

	public TMP_Text holdMovementText;

	public int menuMusicEnabled;

	public TMP_Text pauseMenuMusicText;

	public int readabilityModeEnabled;

	public TMP_Text readabilityModeText;

	public int holdToSkipEnabled;

	public TMP_Text holdToSkipText;

	public RectTransform hiddenContainer;

	public TMP_Text resetDataText;

	public TMP_Text resetTutText;

	public Transform buttonGrid;

	public static AudioMixer masterMixer;

	public ScalableCamera scalableCamera;

	public static string localizedFullStop;

	private bool lastSavedFullscreen = false;

	private BC ctrl;

	private MusicCtrl muCtrl;

	private OptionCtrl optCtrl;

	private TutorialCtrl tutCtrl;

	private Coroutine co_HideCursor;

	private void Start()
	{
		optCtrl = S.I.optCtrl;
		tutCtrl = S.I.tutCtrl;
	}

	public void StartSettings()
	{
		StartCoroutine(_SetDefaultPrefs());
	}

	private IEnumerator _SetDefaultPrefs()
	{
		if (!SaveDataCtrl.Initialized)
		{
			yield return new WaitUntil(() => SaveDataCtrl.Initialized);
		}
		S.I.currentProfile = SaveDataCtrl.Get("CurrentProfile", 0, true);
		S.I.mainCtrl.profileButton.tmpText.text = ScriptLocalization.UI.MainMenu_Profile + " (" + SaveDataCtrl.Get("ProfileName" + S.I.currentProfile, "Saffron", true) + ")";
		Debug.Log("Settings load " + S.I.currentProfile + " " + Time.frameCount);
		if (S.I.EDITION == Edition.Full || S.I.EDITION == Edition.QA)
		{
			while (!SteamManager.Initialized && Time.timeSinceLevelLoad < S.maxLoadTime)
			{
				yield return null;
			}
		}
		RefreshLanguageOptions();
		SetInitialLanguage();
		S.I.itemMan.LoadItemData();
		ctrl = S.I.batCtrl;
		muCtrl = S.I.muCtrl;
		RefreshResolutions();
		lastSavedFullscreen = Screen.fullScreen;
		scalableCamera.CalculatePixelRes(GetCurrentResolution());
		StartCoroutine(_CalculateScreenResolutionAfter(GetCurrentResolution()));
		screenshake = SaveDataCtrl.Get("Screenshake", 100f);
		targetFPS = SaveDataCtrl.Get("TargetFPS", 60);
		spellLabels = SaveDataCtrl.Get("SpellLabels", 0);
		aimMarkerEnabled = SaveDataCtrl.Get("AimMarker", 1);
		autoPreviewEnabled = SaveDataCtrl.Get("AutoPreview", 1);
		sfxVol = SaveDataCtrl.Get("SFXVol", 30f);
		musicVol = SaveDataCtrl.Get("MusicVol", 26f);
		angelModeEnabled = SaveDataCtrl.Get("AngelMode", 0);
		angelModeCurrentDamageReduction = SaveDataCtrl.Get("AngelModeCurrentDamageReduction", 0f);
		angelModeCurrentSpeedReduction = SaveDataCtrl.Get("AngelModeCurrentSpeedReduction", 0f);
		angelModeMaxReduction = Mathf.Round(SaveDataCtrl.Get("AngelModeMaxReduction", 0.2f) * 100f) / 100f;
		int totalDeaths = SaveDataCtrl.Get("TotalDeaths", 0);
		if (angelModeMaxReduction < 0.2f + (float)totalDeaths * 0.02f)
		{
			angelModeMaxReduction = Mathf.Clamp(0.2f + (float)totalDeaths * 0.02f, 0.2f, angelModeMaxReductionMax);
		}
		angelModeDamageSlider.slider.value = angelModeCurrentDamageReduction * 100f;
		angelModeSpeedSlider.slider.value = angelModeCurrentSpeedReduction * 100f;
		UpdateAngelSliders(angelModeMaxReduction);
		holdMovementEnabled = SaveDataCtrl.Get("HoldMovementMode", 0);
		readabilityModeEnabled = SaveDataCtrl.Get("ReadabilityMode", 0);
		menuMusicEnabled = SaveDataCtrl.Get("MenuMusicMode", 1);
		holdToSkipEnabled = SaveDataCtrl.Get("HoldToSkipMode", 1);
		SetTexts();
	}

	public void UpdateAngelSliders(float max)
	{
		angelModeDamageSlider.slider.maxValue = max * 100f;
		angelModeSpeedSlider.slider.maxValue = max * 100f;
	}

	public void SetTexts()
	{
		SetDisplayText(lastSavedFullscreen, false);
		SetResolutionText(GetCurrentResolution());
		SetScreenshakeText();
		SetFPSText();
		SetSpellLabelsText();
		SetAimMarkerText();
		SetAutoPreviewText();
		SetSFXText();
		SetMusicText();
		SetPauseMenuMusicText();
		SetHoldToSkipText();
		SetHoldMovementText();
		resetDataText.text = ScriptLocalization.UI.Reset_Data;
		resetTutText.text = ScriptLocalization.UI.Reset_Tutorial;
		SetAngelModeText();
		SetAngelModeDamageText();
		SetAngelModeSpeedText();
		SetReadabilityModeText();
	}

	public void SetInitialLanguage()
	{
		if (SteamManager.Initialized)
		{
			if (!SaveDataCtrl.Get("InitialSteamLanguageSet", false) || SaveDataCtrl.Get("SavedSteamLanguage", "English") != SteamApps.GetCurrentGameLanguage())
			{
				SaveDataCtrl.Set("InitialSteamLanguageSet", true);
				SaveDataCtrl.Set("SavedSteamLanguage", SteamApps.GetCurrentGameLanguage());
				string availableGameLanguages = SteamApps.GetAvailableGameLanguages();
				string[] array = availableGameLanguages.Split(',');
				string currentGameLanguage = SteamApps.GetCurrentGameLanguage();
				savedLanguage = currentGameLanguage;
				if (savedLanguage == "schinese")
				{
					savedLanguage = "Chinese (Simplified)";
				}
				else if (savedLanguage == "tchinese")
				{
					savedLanguage = "Chinese (Traditional)";
				}
				else if (savedLanguage == "koreana")
				{
					savedLanguage = "Korean";
				}
				else if (savedLanguage == "brazilian")
				{
					savedLanguage = "Portuguese";
				}
				else if (savedLanguage == "latam")
				{
					savedLanguage = "SpanishLatam";
				}
				if (!string.IsNullOrEmpty(savedLanguage))
				{
					savedLanguage = char.ToUpper(savedLanguage[0]) + savedLanguage.Substring(1);
				}
			}
			else
			{
				savedLanguage = SaveDataCtrl.Get("Language", languageOptions[currentLanguageIndex]);
			}
		}
		else
		{
			savedLanguage = SaveDataCtrl.Get("Language", languageOptions[currentLanguageIndex]);
		}
		currentLanguageIndex = languageOptions.IndexOf(savedLanguage);
		SetLanguage();
	}

	private void Update()
	{
		if (Input.GetAxis("Mouse X") == 0f && Input.GetAxis("Mouse Y") == 0f && Input.GetAxis("Mouse ScrollWheel") == 0f && Input.touchCount < 1)
		{
			if (co_HideCursor == null)
			{
				co_HideCursor = StartCoroutine(HideCursor());
			}
		}
		else
		{
			if (co_HideCursor != null)
			{
				StopCoroutine(co_HideCursor);
				co_HideCursor = null;
			}
			Cursor.visible = true;
			btnCtrl.mouseActive = true;
		}
		if (lastSavedFullscreen != Screen.fullScreen)
		{
			lastSavedFullscreen = Screen.fullScreen;
			SetDisplayText(lastSavedFullscreen, true);
		}
	}

	private IEnumerator HideCursor()
	{
		yield return new WaitForSecondsRealtime(9f);
		while (!Application.isFocused)
		{
			yield return new WaitForSecondsRealtime(9f);
		}
		btnCtrl.mouseActive = false;
	}

	public void ClickResetTutorial()
	{
		SaveDataCtrl.Set("TutorialOverlayShown", false);
		SaveDataCtrl.Set("TutorialPathShown", false);
		SaveDataCtrl.Set("TutorialDeckShown", false);
		SaveDataCtrl.Set("TutorialBossKills", 0);
		resetTutText.text = ScriptLocalization.UI.Reset_Tutorial_Success;
		tutCtrl.ResetTutorialProgress();
	}

	public void ClickResetAllData()
	{
		if (resetDataText.text == ScriptLocalization.UI.Reset_Data)
		{
			resetDataText.text = ScriptLocalization.UI.Reset_Data_AreYouSure;
		}
		else if (resetDataText.text == ScriptLocalization.UI.Reset_Data_AreYouSure)
		{
			resetDataText.text = ScriptLocalization.UI.Reset_Data_AreYouSure + "??";
		}
		else if (resetDataText.text == ScriptLocalization.UI.Reset_Data_AreYouSure + "??")
		{
			SaveDataCtrl.RemoveAll();
			resetDataText.text = ScriptLocalization.UI.Reset_Data_Success;
			dataWasReset = true;
			SaveDataCtrl.Write();
		}
	}

	public void ClickDisplay()
	{
		Screen.fullScreen = !Screen.fullScreen;
		StartCoroutine(_SetDisplay(Screen.fullScreen));
	}

	private IEnumerator _SetDisplay(bool fullScreen)
	{
		yield return new WaitUntil(() => Screen.fullScreen == fullScreen);
		if (Screen.fullScreen)
		{
			RefreshResolutions();
			currentResolutionIndex = resolutions.Length - 1;
			ApplyCurrentResolution();
		}
	}

	private void SetDisplayText(bool fullScreen, bool autoMaxSize)
	{
		if (fullScreen)
		{
			displayText.text = ScriptLocalization.UI.DISPLAY_FULLSCREEN;
			if (autoMaxSize)
			{
				StartCoroutine(_SetDisplay(Screen.fullScreen));
			}
		}
		else
		{
			displayText.text = ScriptLocalization.UI.DISPLAY_WINDOWED;
		}
	}

	public Resolution GetCurrentResolution()
	{
		Resolution resolution = default(Resolution);
		resolution.width = Screen.width;
		resolution.height = Screen.height;
		Resolution result = resolution;
		if (currentResolutionIndex >= 0 && currentResolutionIndex < resolutions.Length)
		{
			result = resolutions[currentResolutionIndex];
		}
		return result;
	}

	public void ClickResolution()
	{
		currentResolutionIndex = Utils.GetNextWrappedIndex(resolutions, currentResolutionIndex);
		ApplyCurrentResolution();
	}

	public void ClickResolutionBack()
	{
		currentResolutionIndex = Utils.GetPreviousWrappedIndex(resolutions, currentResolutionIndex);
		ApplyCurrentResolution();
	}

	private void RefreshResolutions()
	{
		resolutions = (from t in Screen.resolutions.Select(delegate(Resolution resolution)
			{
				Resolution result = default(Resolution);
				result.width = resolution.width;
				result.height = resolution.height;
				return result;
			}).Distinct()
			where t.width >= 1024
			select t).ToArray();
		currentResolutionIndex = Array.IndexOf(resolutions, new Resolution
		{
			width = Screen.width,
			height = Screen.height
		});
	}

	private void SetAndApplyResolution(int newResolutionIndex)
	{
		currentResolutionIndex = newResolutionIndex;
		ApplyCurrentResolution();
	}

	private void ApplyCurrentResolution()
	{
		if (currentResolutionIndex >= resolutions.Length)
		{
			currentResolutionIndex = resolutions.Length - 1;
		}
		ApplyResolution(GetCurrentResolution());
	}

	private void ApplyResolution(Resolution resolution)
	{
		SetResolutionText(resolution);
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen, targetFPS);
		scalableCamera.CalculatePixelRes(resolution);
		StartCoroutine(_CalculateScreenResolutionAfter(resolution));
	}

	private IEnumerator _CalculateScreenResolutionAfter(Resolution targetResolution)
	{
		yield return new WaitUntil(() => Screen.width == targetResolution.width && Screen.height == targetResolution.height);
		scalableCamera.CalculatePixelRes(targetResolution);
		SetResolutionText(targetResolution);
	}

	private void SetResolutionText(Resolution resolution)
	{
		resolutionText.text = string.Format("{0}x{1}", resolution.width, resolution.height);
	}

	public void ClickScreenshake()
	{
		screenshake += 50f;
		if (screenshake == 150f)
		{
			screenshake = 1000f;
		}
		if (screenshake > 1000f)
		{
			screenshake = 0f;
		}
		SetScreenshakeText();
		SaveDataCtrl.Set("Screenshake", screenshake);
		S.I.camCtrl.Shake(1);
	}

	public void ClickScreenshakeBack()
	{
		screenshake -= 50f;
		if (screenshake == 950f)
		{
			screenshake = 100f;
		}
		if (screenshake < 0f)
		{
			screenshake = 1000f;
		}
		SetScreenshakeText();
		SaveDataCtrl.Set("Screenshake", screenshake);
		S.I.camCtrl.Shake(1);
	}

	private void SetScreenshakeText()
	{
		S.I.camCtrl.shakeMultiplier = screenshake / 100f;
		if (screenshake == 0f)
		{
			screenshakeText.text = ScriptLocalization.UI.SCREENSHAKE_NONE;
		}
		else if (screenshake == 50f)
		{
			screenshakeText.text = ScriptLocalization.UI.SCREENSHAKE_HALF;
		}
		else if (screenshake == 100f)
		{
			screenshakeText.text = ScriptLocalization.UI.SCREENSHAKE_FULL;
		}
		else if (screenshake == 1000f)
		{
			screenshakeText.text = ScriptLocalization.UI.SCREENSHAKE_ELEVEN;
		}
	}

	public void ClickFPS()
	{
		targetFPS += 30;
		if (targetFPS == 150)
		{
			targetFPS = 144;
		}
		if (targetFPS > 144)
		{
			targetFPS = 30;
		}
		SetFPSText();
		SaveDataCtrl.Set("TargetFPS", targetFPS);
	}

	public void ClickFPSBack()
	{
		targetFPS -= 30;
		if (targetFPS == 114)
		{
			targetFPS = 120;
		}
		if (targetFPS < 30)
		{
			targetFPS = 144;
		}
		SetFPSText();
		SaveDataCtrl.Set("TargetFPS", targetFPS);
	}

	private void SetFPSText()
	{
		Application.targetFrameRate = targetFPS;
		QualitySettings.vSyncCount = 0;
		fpsText.text = string.Format("{0} : {1}", ScriptLocalization.UI.FPS, Application.targetFrameRate);
	}

	public void ClickSpellLabels()
	{
		if (spellLabels == 0)
		{
			spellLabels = 1;
		}
		else
		{
			spellLabels = 0;
		}
		SetSpellLabelsText();
		SaveDataCtrl.Set("SpellLabels", spellLabels);
	}

	private void SetSpellLabelsText()
	{
		if (spellLabels == 0)
		{
			spellLabelsText.text = ScriptLocalization.UI.SPELL_LABELS + ": " + ScriptLocalization.UI.Options_OFF;
			S.I.LABELS = false;
			{
				foreach (DuelDisk duelDisk in ctrl.deCtrl.duelDisks)
				{
					foreach (Cardtridge currentCardtridge in duelDisk.currentCardtridges)
					{
						currentCardtridge.label.text = string.Empty;
					}
					foreach (CastSlot castSlot in duelDisk.castSlots)
					{
						castSlot.cardtridgeRef.label.text = string.Empty;
					}
				}
				return;
			}
		}
		if (spellLabels != 1)
		{
			return;
		}
		spellLabelsText.text = ScriptLocalization.UI.SPELL_LABELS + ": " + ScriptLocalization.UI.Options_ON;
		S.I.LABELS = true;
		foreach (DuelDisk duelDisk2 in ctrl.deCtrl.duelDisks)
		{
			foreach (Cardtridge currentCardtridge2 in duelDisk2.currentCardtridges)
			{
				currentCardtridge2.SetLabel(currentCardtridge2.spellObj.shortName);
			}
			foreach (CastSlot castSlot2 in duelDisk2.castSlots)
			{
				if ((bool)castSlot2.cardtridgeRef)
				{
					castSlot2.cardtridgeRef.SetLabel(castSlot2.spellObj.shortName);
				}
				if ((bool)castSlot2.cardtridgeFill)
				{
					castSlot2.cardtridgeFill.SetLabel(string.Empty);
				}
			}
		}
	}

	public void ClickAimMarker()
	{
		if (aimMarkerEnabled == 0)
		{
			aimMarkerEnabled = 1;
		}
		else
		{
			aimMarkerEnabled = 0;
		}
		SetAimMarkerText();
		SaveDataCtrl.Set("AimMarker", aimMarkerEnabled);
	}

	public void SetAimMarkerText()
	{
		if (aimMarkerEnabled == 0)
		{
			aimMarkerText.text = ScriptLocalization.UI.AIM_MARKER + ": " + ScriptLocalization.UI.Options_OFF;
			{
				foreach (Player currentPlayer in ctrl.currentPlayers)
				{
					currentPlayer.aimMarker.color = Color.clear;
				}
				return;
			}
		}
		if (aimMarkerEnabled != 1)
		{
			return;
		}
		aimMarkerText.text = ScriptLocalization.UI.AIM_MARKER + ": " + ScriptLocalization.UI.Options_ON;
		if (ctrl.runCtrl.currentHellPasses.Contains(3))
		{
			return;
		}
		foreach (Player currentPlayer2 in ctrl.currentPlayers)
		{
			currentPlayer2.aimMarker.color = Color.white;
		}
	}

	public void ClickAngelMode()
	{
		if (angelModeEnabled == 0)
		{
			angelModeEnabled = 1;
		}
		else
		{
			angelModeEnabled = 0;
		}
		SetAngelModeText();
		SaveDataCtrl.Set("AngelMode", angelModeEnabled);
	}

	public void SetAngelModeText()
	{
		if (angelModeEnabled == 0)
		{
			angelModeText.text = ScriptLocalization.UI.Settings_AngelMode + ": " + ScriptLocalization.UI.Options_OFF;
			DisableAngelMode();
		}
		else if (angelModeEnabled == 1)
		{
			angelModeText.text = ScriptLocalization.UI.Settings_AngelMode + ": " + ScriptLocalization.UI.Options_ON;
			EnableAngelMode();
		}
	}

	private void EnableAngelMode()
	{
		angelModeEnabled = 1;
		angelModeDamageSlider.transform.SetParent(buttonGrid);
		angelModeDamageSlider.transform.SetSiblingIndex(angelModeText.transform.parent.GetSiblingIndex() + 1);
		angelModeSpeedSlider.transform.SetParent(buttonGrid);
		angelModeSpeedSlider.transform.SetSiblingIndex(angelModeText.transform.parent.GetSiblingIndex() + 2);
		foreach (Player currentPlayer in ctrl.currentPlayers)
		{
			currentPlayer.CreateStatusDisplay(Status.Blessed);
		}
	}

	private void DisableAngelMode()
	{
		angelModeEnabled = 0;
		angelModeDamageSlider.transform.SetParent(hiddenContainer);
		angelModeSpeedSlider.transform.SetParent(hiddenContainer);
		foreach (Player currentPlayer in ctrl.currentPlayers)
		{
			currentPlayer.RemoveStatus(Status.Blessed);
			for (int num = currentPlayer.statusDisplays.Count - 1; num >= 0; num--)
			{
				if (currentPlayer.statusDisplays[num].status == Status.Blessed)
				{
					UnityEngine.Object.Destroy(currentPlayer.statusDisplays[num].gameObject);
					currentPlayer.statusDisplays.RemoveAt(num);
				}
			}
		}
	}

	public void ClickAngelDamage()
	{
		angelModeDamageText.text = angelModeDamageSlider.slider.value.ToString();
		angelModeCurrentDamageReduction = angelModeDamageSlider.slider.value / 100f;
		SetAngelModeDamageText();
		SaveDataCtrl.Set("AngelModeCurrentDamageReduction", angelModeCurrentDamageReduction);
	}

	public void SetAngelModeDamageText()
	{
		angelModeDamageSlider.slider.value = angelModeCurrentDamageReduction * 100f;
		angelModeDamageText.text = string.Format("{0}: -{1} ({2} -{3})", ScriptLocalization.UI.Settings_ReceivedDamage, Percentify(angelModeCurrentDamageReduction * 100f), ScriptLocalization.UI.Settings_Max, Percentify(angelModeMaxReduction * 100f));
	}

	public void ClickAngelSpeed()
	{
		angelModeSpeedText.text = angelModeSpeedSlider.slider.value.ToString();
		angelModeCurrentSpeedReduction = angelModeSpeedSlider.slider.value / 100f;
		SetAngelModeSpeedText();
		SaveDataCtrl.Set("AngelModeCurrentSpeedReduction", angelModeCurrentSpeedReduction);
	}

	public void SetAngelModeSpeedText()
	{
		angelModeSpeedSlider.slider.value = angelModeCurrentSpeedReduction * 100f;
		angelModeSpeedText.text = string.Format("{0}: -{1} ({2} -{3})", ScriptLocalization.UI.Settings_EnemySpeed, Percentify(angelModeCurrentSpeedReduction * 100f), ScriptLocalization.UI.Settings_Max, Percentify(angelModeMaxReduction * 100f));
	}

	public void IncreaseAngelMultiplierMax()
	{
		if (angelModeMaxReduction < angelModeMaxReductionMax)
		{
			angelModeMaxReduction += 0.02f;
			if (angelModeCurrentDamageReduction > 0f)
			{
				angelModeCurrentDamageReduction += 0.02f;
			}
			if (angelModeCurrentSpeedReduction > 0f)
			{
				angelModeCurrentSpeedReduction += 0.02f;
			}
			SaveDataCtrl.Set("AngelModeCurrentDamageReduction", angelModeCurrentDamageReduction);
			SaveDataCtrl.Set("AngelModeCurrentSpeedReduction", angelModeCurrentSpeedReduction);
			SaveDataCtrl.Set("AngelModeMaxReduction", angelModeMaxReduction);
			UpdateAngelSliders(angelModeMaxReduction);
			SetAngelModeDamageText();
			SetAngelModeSpeedText();
		}
	}

	public void ClickHoldMovement()
	{
		if (holdMovementEnabled == 0)
		{
			holdMovementEnabled = 1;
		}
		else
		{
			holdMovementEnabled = 0;
		}
		SetHoldMovementText();
		SaveDataCtrl.Set("HoldMovementMode", holdMovementEnabled);
	}

	public void SetHoldMovementText()
	{
		if (holdMovementEnabled == 0)
		{
			holdMovementText.text = ScriptLocalization.UI.Settings_HoldMovement + ": " + ScriptLocalization.UI.Options_OFF;
		}
		else if (holdMovementEnabled == 1)
		{
			holdMovementText.text = ScriptLocalization.UI.Settings_HoldMovement + ": " + ScriptLocalization.UI.Options_ON;
		}
		S.I.holdMovementEnabled = holdMovementEnabled == 1;
	}

	public void ClickHoldToSkip()
	{
		if (holdToSkipEnabled == 0)
		{
			holdToSkipEnabled = 1;
		}
		else
		{
			holdToSkipEnabled = 0;
		}
		SetHoldToSkipText();
		SaveDataCtrl.Set("HoldToSkipMode", holdToSkipEnabled);
	}

	public void SetHoldToSkipText()
	{
		if (holdToSkipEnabled == 0)
		{
			holdToSkipText.text = ScriptLocalization.UI.Settings_HoldToSkip + ": " + ScriptLocalization.UI.Options_OFF;
		}
		else if (holdToSkipEnabled == 1)
		{
			holdToSkipText.text = ScriptLocalization.UI.Settings_HoldToSkip + ": " + ScriptLocalization.UI.Options_ON;
		}
		S.I.holdToSkipEnabled = holdToSkipEnabled == 1;
	}

	public void ClickPauseMenuMusic()
	{
		if (menuMusicEnabled == 0)
		{
			menuMusicEnabled = 1;
		}
		else
		{
			menuMusicEnabled = 0;
		}
		SetPauseMenuMusicText();
		SaveDataCtrl.Set("MenuMusicMode", menuMusicEnabled);
	}

	public void SetPauseMenuMusicText()
	{
		if (menuMusicEnabled == 0)
		{
			pauseMenuMusicText.text = ScriptLocalization.UI.Settings_PauseMenuMusic + ": " + ScriptLocalization.UI.Options_OFF;
			Application.runInBackground = false;
		}
		else if (menuMusicEnabled == 1)
		{
			pauseMenuMusicText.text = ScriptLocalization.UI.Settings_PauseMenuMusic + ": " + ScriptLocalization.UI.Options_ON;
			Application.runInBackground = true;
		}
		S.I.menuMusicEnabled = menuMusicEnabled == 1;
		if (menuMusicEnabled == 1)
		{
			muCtrl.UnmuteMusic();
		}
		else if (open)
		{
			muCtrl.MuteMusic();
		}
	}

	public void ClickReadabilityMode()
	{
		if (readabilityModeEnabled == 0)
		{
			readabilityModeEnabled = 1;
		}
		else
		{
			readabilityModeEnabled = 0;
		}
		SetReadabilityModeText();
		SaveDataCtrl.Set("ReadabilityMode", readabilityModeEnabled);
	}

	public void SetReadabilityModeText()
	{
		if (readabilityModeEnabled == 0)
		{
			readabilityModeText.text = ScriptLocalization.UI.Settings_Readability + ": " + ScriptLocalization.UI.Options_OFF;
		}
		else if (readabilityModeEnabled == 1)
		{
			readabilityModeText.text = ScriptLocalization.UI.Settings_Readability + ": " + ScriptLocalization.UI.Options_ON;
		}
		S.I.readabilityModeEnabled = readabilityModeEnabled == 1;
	}

	public void ClickAutoPreview()
	{
		if (autoPreviewEnabled == 0)
		{
			autoPreviewEnabled = 1;
		}
		else
		{
			autoPreviewEnabled = 0;
		}
		SetAutoPreviewText();
		SaveDataCtrl.Set("AutoPreview", autoPreviewEnabled);
	}

	public void SetAutoPreviewText()
	{
		if (autoPreviewEnabled == 0)
		{
			autoPreviewText.text = ScriptLocalization.UI.Settings_SpellPreview + ": " + ScriptLocalization.UI.Options_OFF;
			S.I.refCtrl.autoPreview = false;
		}
		else if (autoPreviewEnabled == 1)
		{
			autoPreviewText.text = ScriptLocalization.UI.Settings_SpellPreview + ": " + ScriptLocalization.UI.Options_ON;
			S.I.refCtrl.autoPreview = true;
		}
	}

	public string Percentify(float number)
	{
		if (LocalizationManager.GetAllLanguages()[I2LocIndex()].Contains("Turkish"))
		{
			return "%" + number;
		}
		if (LocalizationManager.GetAllLanguages()[I2LocIndex()].Contains("French") || LocalizationManager.GetAllLanguages()[I2LocIndex()].Contains("German"))
		{
			return number + " %";
		}
		return number + "%";
	}

	public void ClickSFXButton()
	{
		sfxText.text = sfxSlider.value.ToString();
		sfxVol = sfxSlider.value;
		masterMixer.SetFloat("sfxVol", Mathf.Log10(Mathf.Clamp(sfxVol / 100f, 0.0001f, 1f)) * 20f);
		SetSFXText();
		SaveDataCtrl.Set("SFXVol", sfxVol);
	}

	private void SetSFXText()
	{
		sfxSlider.value = sfxVol;
		sfxText.text = string.Format("{0}: {1}", ScriptLocalization.UI.SFX_VOL, Percentify(sfxVol));
	}

	public void ClickMusicButton()
	{
		musicText.text = musicSlider.value.ToString();
		musicVol = musicSlider.value;
		masterMixer.SetFloat("musicVol", Mathf.Log10(Mathf.Clamp(musicVol / 100f, 0.0001f, 1f)) * 20f);
		SetMusicText();
		SaveDataCtrl.Set("MusicVol", musicVol);
	}

	private void SetMusicText()
	{
		musicSlider.value = musicVol;
		musicText.text = string.Format("{0}: {1}", ScriptLocalization.UI.MUSIC_VOL, Percentify(musicVol));
	}

	public void ClickLanguage()
	{
		if (ctrl.GameState != GState.MainMenu)
		{
			S.I.PlayOnce(btnCtrl.lockedSound);
			return;
		}
		currentLanguageIndex++;
		SetLanguage();
		SetTexts();
	}

	public void ClickLanguageBack()
	{
		if (ctrl.GameState != GState.MainMenu)
		{
			S.I.PlayOnce(btnCtrl.lockedSound);
			return;
		}
		currentLanguageIndex--;
		SetLanguage();
		SetTexts();
	}

	private string GetLocalizedLanguageName()
	{
		string Translation = "";
		if (!LocalizationManager.TryGetTranslation("UI/Language_" + languageOptions[currentLanguageIndex], out Translation))
		{
			Translation = languageOptions[currentLanguageIndex];
		}
		return Translation;
	}

	private void SetLanguage()
	{
		if (currentLanguageIndex < 0)
		{
			currentLanguageIndex = Mathf.Clamp(languageOptions.Count - 1, 0, languageOptions.Count);
		}
		if (currentLanguageIndex >= languageOptions.Count)
		{
			currentLanguageIndex = 0;
		}
		if ((bool)optCtrl)
		{
			optCtrl.selectorLeft.damping = 0.01f;
			optCtrl.selectorRight.damping = 0.01f;
		}
		LocalizationManager.SetLanguageAndCode(LocalizationManager.GetAllLanguages()[I2LocIndex()], LocalizationManager.GetAllLanguagesCode()[I2LocIndex()]);
		SaveDataCtrl.Set("Language", languageOptions[currentLanguageIndex]);
		languageText.text = ScriptLocalization.UI.Settings_Language + ": " + GetLocalizedLanguageName();
		localizedFullStop = ". ";
		if (LocalizationManager.GetAllLanguages()[I2LocIndex()].Contains("Chinese") || LocalizationManager.GetAllLanguages()[I2LocIndex()].Contains("Japanese"))
		{
			localizedFullStop = "。";
		}
	}

	private void RefreshLanguageOptions()
	{
		languageOptions.Clear();
		foreach (string allLanguage in LocalizationManager.GetAllLanguages())
		{
			languageOptions.Add(allLanguage);
		}
		currentLanguageIndex = 0;
	}

	private int I2LocIndex()
	{
		List<string> allLanguages = LocalizationManager.GetAllLanguages();
		string text = languageOptions[currentLanguageIndex];
		if (LocalizationManager.HasLanguage(text))
		{
			return allLanguages.IndexOf(text);
		}
		Debug.LogWarning("Couldn't find " + text + " in I2.Loc.LocalizationManager languages! Defaulting to English!");
		RefreshLanguageOptions();
		return allLanguages.IndexOf("English");
	}

	public override void Open()
	{
		foreach (Transform item in buttonGrid)
		{
			if (item.gameObject.activeSelf && (bool)item.GetComponent<UIButton>())
			{
				defaultButton = item.GetComponent<UIButton>();
				break;
			}
		}
		base.Open();
		savedLanguage = languageOptions[currentLanguageIndex];
		if (ctrl.GameState != GState.MainMenu)
		{
			languageText.color = Color.grey;
			languageBtn.disabled = true;
			languageText.text = ScriptLocalization.UI.Settings_Language + ": " + GetLocalizedLanguageName() + " " + ScriptLocalization.UI.Settings_MustBeInMainMenu;
		}
		else
		{
			languageText.text = ScriptLocalization.UI.Settings_Language + ": " + GetLocalizedLanguageName();
			languageText.color = Color.white;
			languageBtn.disabled = false;
		}
		resetDataText.text = ScriptLocalization.UI.Reset_Data;
		resetTutText.text = ScriptLocalization.UI.Reset_Tutorial;
	}

	public override void Close()
	{
		optCtrl.selectorLeft.ResetDamping();
		optCtrl.selectorRight.ResetDamping();
		SaveDataCtrl.Write(true);
		if (dataWasReset || savedLanguage != languageOptions[currentLanguageIndex])
		{
			StartCoroutine(_RestartScene());
			return;
		}
		SaveDataCtrl.Set("Language", savedLanguage);
		optCtrl.ClosePanel(this);
	}

	public IEnumerator _RestartScene()
	{
		S.I.CameraStill(UIColor.BlueDark);
		btnCtrl.transitioning = true;
		AssetBundleLoadOperation assetBundleLoadOperation = AssetBundleManager.LoadLevelAsync("main", "Main", false);
		float startTime = Time.unscaledTime;
		while (!assetBundleLoadOperation.IsDone())
		{
			Mathf.Clamp01(assetBundleLoadOperation.GetProgress() / 0.9f);
			if (assetBundleLoadOperation.GetProgress() >= 1f && startTime + 1f < Time.unscaledTime)
			{
				assetBundleLoadOperation.AllowSceneActivation();
			}
			yield return null;
		}
	}

	public override void CloseBase()
	{
		base.Close();
	}
}
