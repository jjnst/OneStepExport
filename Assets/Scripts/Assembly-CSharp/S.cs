using System;
using System.Collections;
using System.Diagnostics;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[MoonSharpUserData]
public class S : MonoBehaviour
{
	public static S I;

	[Header("EDITION")]
	public Edition EDITION;

	[Header("MODES")]
	public bool CAMPAIGN_MODE = false;

	public bool TESTING_MODE = false;

	public ZoneType testZoneType;

	[Header("SCENES")]
	public GScene scene;

	[Header("OPTIONS")]
	public bool ANIMATIONS = false;

	public bool RECORD_MODE = false;

	public bool FULL_DECK = false;

	public bool FULL_ARTS = false;

	public bool LABELS = false;

	public bool DEVELOPER_TOOLS = false;

	public bool CONSOLE = false;

	public bool SHOW_TUTORIAL = false;

	public bool UNLOCK_ALL_CHARS = false;

	public int zonesPerWorld = 7;

	public bool ONEWORLD = false;

	public bool BOSS_TEST_MODE = false;

	public bool BETA = false;

	public bool PS4 = false;

	public bool MATCH_DECK_ORDER = false;

	public bool CO_OP = false;

	public bool SPARE_BOSSES_AUTO = false;

	[Header("SPAWNS")]
	public bool spawnChest = false;

	public bool spawnHealChest = false;

	public bool spawnHostage = false;

	public bool dontSpawnAnything = false;

	public Image flashPane;

	public float lerpTime = 1f;

	public bool screenFlash = false;

	public bool screenStill = false;

	public bool cameraStill = false;

	private float currentLerpTime;

	public static float maxLoadTime = 3f;

	public static AudioMixerGroup sfxGroup;

	public static AudioMixerGroup musicGroup;

	public AudioClip explosionSound;

	public AudioClip beingHitSound;

	public AudioClip beingHitSoundHeavy;

	[HideInInspector]
	public XMLReader xmlReader;

	public Ana ana;

	public BC batCtrl;

	public BGCtrl bgCtrl;

	public ButtonCtrl btnCtrl;

	public CGCtrl cgCtrl;

	public ControlsCtrl conCtrl;

	public CoOpCtrl coOpCtrl;

	public CreditsCtrl credCtrl;

	public DeckCtrl deCtrl;

	public DebugLayover debugLayover;

	public DemoLiveCtrl demoLiveCtrl;

	public FocusCtrl foCtrl;

	public HeroSelectCtrl heCtrl;

	public IdleCtrl idCtrl;

	public MainCtrl mainCtrl;

	public ModCtrl modCtrl;

	public MusicCtrl muCtrl;

	public OptionCtrl optCtrl;

	public PostCtrl poCtrl;

	public ReferenceCtrl refCtrl;

	public RunCtrl runCtrl;

	public ShopCtrl shopCtrl;

	public SpawnCtrl spCtrl;

	public TI tiCtrl;

	public TwitchClient twClient;

	public TutorialCtrl tutCtrl;

	public UnlockCtrl unCtrl;

	public ItemManager itemMan;

	public ConsoleView consoleView;

	public static bool modsInstalled = false;

	public bool holdMovementEnabled = false;

	public bool menuMusicEnabled = false;

	public bool readabilityModeEnabled = false;

	public bool holdToSkipEnabled = false;

	public VersionCtrl vrCtrl;

	public Camera mainCam;

	[HideInInspector]
	public CameraScript camCtrl;

	public Transform pool;

	public AudioSource audioSource;

	private Color flashingColor = Color.white;

	private Color stillColor = Color.white;

	private Color cameraColor = Color.white;

	public float stillChangeRate = 0.12f;

	public float previewVolumeMultiplier = 0.6f;

	public float baseTickRate = 0.5f;

	public int testerInt = 0;

	public int currentProfile = 0;

	private void Awake()
	{
		if (I != null)
		{
			UnityEngine.Debug.LogError("Multiple instances of Singleton S!");
		}
		I = this;
		camCtrl = mainCam.GetComponent<CameraScript>();
		audioSource = GetComponent<AudioSource>();
		xmlReader = runCtrl.xmlReader;
		Edition eDITION = EDITION;
		EDITION = eDITION;
		if (EDITION != Edition.Dev)
		{
			CAMPAIGN_MODE = true;
			TESTING_MODE = false;
			scene = GScene.MainMenu;
			ANIMATIONS = true;
			RECORD_MODE = false;
			FULL_DECK = false;
			FULL_ARTS = false;
			SHOW_TUTORIAL = true;
			zonesPerWorld = 7;
			ONEWORLD = false;
			BOSS_TEST_MODE = false;
			MATCH_DECK_ORDER = false;
			CO_OP = false;
			dontSpawnAnything = false;
			runCtrl.useRandomSeed = true;
			deCtrl.deckScreen.startingUpgraders = 0;
			deCtrl.deckScreen.startingRemovals = 1;
			spCtrl.forceSpawn = false;
			poCtrl.smallTiers = false;
			runCtrl.useTestOrder = false;
		}
		else
		{
			deCtrl.deckScreen.startingUpgraders = 20;
			deCtrl.deckScreen.startingRemovals = 10;
		}
		if (EDITION == Edition.Beta)
		{
			DEVELOPER_TOOLS = false;
			CONSOLE = false;
			UNLOCK_ALL_CHARS = false;
			BETA = true;
		}
		else if (EDITION == Edition.Full)
		{
			DEVELOPER_TOOLS = false;
			SHOW_TUTORIAL = true;
			CONSOLE = false;
			UNLOCK_ALL_CHARS = false;
			BETA = false;
		}
		else if (EDITION == Edition.QA)
		{
			DEVELOPER_TOOLS = true;
			CONSOLE = true;
			UNLOCK_ALL_CHARS = true;
			BETA = false;
			zonesPerWorld = 2;
		}
		else if (EDITION == Edition.DemoLive)
		{
			zonesPerWorld = 9;
			scene = GScene.DemoLive;
		}
		if (sfxGroup == null)
		{
			sfxGroup = audioSource.outputAudioMixerGroup;
		}
		audioSource.outputAudioMixerGroup = sfxGroup;
		if (musicGroup == null)
		{
			musicGroup = muCtrl.audioSource.outputAudioMixerGroup;
			return;
		}
		muCtrl.audioSource.outputAudioMixerGroup = musicGroup;
		AudioMixerGroup audioMixerGroup2 = (cgCtrl.audioSource.outputAudioMixerGroup = musicGroup);
		musicGroup = audioMixerGroup2;
	}

	private void OnApplicationQuit()
	{
		if (!Application.isEditor)
		{
			Process.GetCurrentProcess().Kill();
		}
	}

	private void Update()
	{
		if (screenFlash)
		{
			UpdateColor(flashPane, flashingColor, flashingColor - new Color(0f, 0f, 0f, 1f), ref screenFlash);
		}
		if (cameraStill)
		{
			UpdateColor(camCtrl.cameraPane, camCtrl.cameraPane.color, cameraColor, ref cameraStill, stillChangeRate);
		}
	}

	private void UpdateColor(Image pane, Color startColor, Color endColor, ref bool endVar, float changeRate = 1f)
	{
		currentLerpTime += Time.unscaledDeltaTime * changeRate;
		if (currentLerpTime > lerpTime)
		{
			currentLerpTime = lerpTime;
		}
		float num = currentLerpTime / lerpTime;
		num = Mathf.Sin(num * (float)Math.PI * 0.5f);
		pane.color = Color.Lerp(startColor, endColor, num);
		if (pane.color == endColor)
		{
			endVar = false;
		}
	}

	public static void AddSkinUnlock(string skinID)
	{
		if (I.runCtrl.currentRun != null)
		{
			I.runCtrl.currentRun.skinUnlocks.Add(skinID);
		}
	}

	public static void AddCharUnlock(string charID)
	{
		if (I.runCtrl.currentRun != null)
		{
			I.runCtrl.currentRun.charUnlocks.Add(charID);
		}
	}

	public void HitPause(float pauseLength)
	{
		StartCoroutine(HitPauseC(pauseLength));
	}

	private IEnumerator HitPauseC(float pauseLength)
	{
		BC.GTimeScale = 0f;
		yield return new WaitForSecondsRealtime(pauseLength);
		BC.GTimeScale = 1f;
	}

	public void Flash(UIColor flashColor = UIColor.White)
	{
		flashingColor = GetFlashColor(flashColor);
		currentLerpTime = 0f;
		screenFlash = true;
	}

	public void CameraStill(UIColor flashColor)
	{
		cameraColor = GetFlashColor(flashColor);
		currentLerpTime = 0f;
		cameraStill = true;
	}

	public Color GetFlashColor(UIColor flashColor)
	{
		switch (flashColor)
		{
		case UIColor.Red:
			return flashingColor = new Color32(207, 0, 15, byte.MaxValue);
		case UIColor.RedLight:
			return flashingColor = new Color32(207, 0, 15, 150);
		case UIColor.Black:
			return flashingColor = Color.black;
		case UIColor.Clear:
			return flashingColor = Color.clear;
		default:
			return flashingColor = U.I.GetColor(flashColor);
		}
	}

	private ParticleSystem instantiate(ParticleSystem prefab, Vector3 position)
	{
		ParticleSystem particleSystem = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
		UnityEngine.Object.Destroy(particleSystem.gameObject, particleSystem.main.startLifetime.constant);
		return particleSystem;
	}

	public void DespawnAfter(GameObject obj, float delayTime = 0f)
	{
		StartCoroutine(DespawnAfterC(obj, delayTime));
	}

	private IEnumerator DespawnAfterC(GameObject obj, float delayTime = 0f)
	{
		float timer = delayTime;
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			if (!obj.activeSelf)
			{
				yield break;
			}
			yield return null;
		}
		SimplePool.Despawn(obj);
	}

	public void PlayOnce(AudioClip audio, bool isReference = false)
	{
		if (audio != null)
		{
			if (isReference)
			{
				audioSource.PlayOneShot(audio, previewVolumeMultiplier);
			}
			else
			{
				audioSource.PlayOneShot(audio, 1f);
			}
		}
	}
}
