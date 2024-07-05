using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using E7.Introloop;
using Sirenix.OdinInspector;
using UnityEngine;

public class MusicCtrl : SerializedMonoBehaviour
{
	public float musicVol = 1f;

	public float introLoopFadeOutLength = 1f;

	public AudioSource audioSource;

	public IntroloopAudio titleTrack;

	public IntroloopAudio mainTrack;

	public IntroloopAudio tutorialTrack;

	public IntroloopAudio bossBattleTrack;

	public IntroloopAudio victoryIntroLoop;

	public IntroloopAudio victoryAltIntroLoop;

	public IntroloopAudio executionIntroLoop;

	public IntroloopAudio executionDarkIntroLoop;

	public IntroloopAudio pvpSelectTrack;

	public IntroloopAudio gameOverIntroLoop;

	public IntroloopAudio creditsGood;

	public IntroloopAudio creditsBad;

	public IntroloopAudio creditsTrack;

	public IntroloopAudio creditsKickstarterTrack;

	public Dictionary<string, IntroloopAudio> idleEnvironments;

	public Dictionary<string, IntroloopAudio> battleEnvironments;

	public Dictionary<string, IntroloopAudio> battleEnvironmentsLate;

	public Dictionary<string, IntroloopAudio> characterThemes;

	public BossViolette bossViolette;

	private AudioClip finalClip;

	private IntroloopAudio finalIntroLoop;

	[NonSerialized]
	public RunCtrl runCtrl;

	private bool viMusicWasPlaying = false;

	public bool introLoopWasPlaying = false;

	private Coroutine fadeOutCo;

	private bool optionsMusicMuted = false;

	private void Awake()
	{
		runCtrl = S.I.runCtrl;
	}

	private void Update()
	{
		if ((bool)bossViolette && audioSource.clip == bossViolette.battleTheme)
		{
			audioSource.pitch = Time.timeScale;
		}
	}

	public void PlayTutorial()
	{
		PlayIntroLoop(tutorialTrack);
	}

	public void PlayTitle()
	{
		PlayIntroLoop(titleTrack);
	}

	public void PlayBattle(int tier = 0)
	{
		if (tier == 0)
		{
			if (battleEnvironments.ContainsKey(runCtrl.currentWorld.background) && S.I.EDITION != Edition.DemoLive)
			{
				finalIntroLoop = battleEnvironments[runCtrl.currentWorld.background];
			}
			else
			{
				finalIntroLoop = battleEnvironments.ElementAt(UnityEngine.Random.Range(0, battleEnvironments.Count - 1)).Value;
			}
		}
		else if (battleEnvironmentsLate.ContainsKey(runCtrl.currentWorld.background) && S.I.EDITION != Edition.DemoLive)
		{
			finalIntroLoop = battleEnvironmentsLate[runCtrl.currentWorld.background];
		}
		else
		{
			finalIntroLoop = battleEnvironmentsLate.ElementAt(UnityEngine.Random.Range(0, battleEnvironmentsLate.Count - 1)).Value;
		}
		PlayIntroLoop(finalIntroLoop);
	}

	public void PlayBoss()
	{
		PlayIntroLoop(bossBattleTrack);
	}

	public void PlayPvPSelect()
	{
		PlayIntroLoop(pvpSelectTrack);
	}

	public void PlayIdle()
	{
		finalIntroLoop = idleEnvironments[runCtrl.currentWorld.background];
		if (runCtrl.currentRun.zoneNum > 3 && idleEnvironments.ContainsKey(runCtrl.currentWorld.background + "2"))
		{
			finalIntroLoop = idleEnvironments[runCtrl.currentWorld.background + "2"];
		}
		PlayIntroLoop(finalIntroLoop);
	}

	public void PlayIntroLoop(IntroloopAudio track)
	{
		Stop();
		IntroloopPlayer.Instance.Play(track);
	}

	public void PlayVictory(bool alt = false)
	{
		if (!alt)
		{
			PlayIntroLoop(victoryIntroLoop);
		}
		else
		{
			PlayIntroLoop(victoryAltIntroLoop);
		}
	}

	public void PlayExecution(bool alt = false)
	{
		if (!alt)
		{
			PlayIntroLoop(executionIntroLoop);
		}
		else
		{
			PlayIntroLoop(executionDarkIntroLoop);
		}
	}

	public void PlayGameOver(bool PvP)
	{
		if (PvP)
		{
			PlayIntroLoop(victoryIntroLoop);
		}
		else
		{
			PlayIntroLoop(gameOverIntroLoop);
		}
	}

	public void PlayCredits(Ending ending)
	{
		if (ending == Ending.Genocide)
		{
			PlayIntroLoop(creditsBad);
		}
		else
		{
			PlayIntroLoop(creditsGood);
		}
	}

	public void ReplayCredits()
	{
		PlayIntroLoop(creditsTrack);
	}

	public void PlayCreditsKickstarter()
	{
		PlayIntroLoop(creditsKickstarterTrack);
	}

	public void PlayCharacterTheme(string characterName)
	{
		if (characterThemes.ContainsKey(characterName))
		{
			PlayIntroLoop(characterThemes[characterName]);
		}
		else
		{
			PlayBattle(UnityEngine.Random.Range(0, 2));
		}
	}

	public void Stop()
	{
		fadeOutCo = StartCoroutine(_FadeOut());
	}

	public void TransitionTo(IntroloopAudio nextTrack = null)
	{
		fadeOutCo = StartCoroutine(_FadeOut(nextTrack));
	}

	private IEnumerator _FadeOut(IntroloopAudio nextTrack = null)
	{
		while ((double)audioSource.volume > 0.02)
		{
			audioSource.volume -= 0.02f;
			yield return null;
		}
		audioSource.volume = 0f;
		if ((bool)nextTrack)
		{
			IntroloopPlayer.Instance.Play(nextTrack);
		}
		fadeOutCo = null;
	}

	public void StopIntroLoop()
	{
		IntroloopPlayer.Instance.Stop();
	}

	public void PauseIntroLoop(bool fade = true)
	{
		if (!fade)
		{
			IntroloopPlayer.Instance.Pause();
		}
		else
		{
			IntroloopPlayer.Instance.Pause(introLoopFadeOutLength);
		}
	}

	public void ResumeIntroLoop()
	{
		IntroloopPlayer.Instance.Resume(introLoopFadeOutLength);
	}

	public void Resume()
	{
		if (fadeOutCo != null)
		{
			StopCoroutine(fadeOutCo);
			fadeOutCo = null;
		}
		audioSource.volume = musicVol;
		audioSource.Play();
	}

	public void Play(AudioClip audioClip, bool loop = true)
	{
		audioSource.clip = audioClip;
		audioSource.volume = musicVol;
		audioSource.loop = loop;
		audioSource.Play();
	}

	public void OptionsOpened()
	{
		if (audioSource.clip == bossViolette.battleTheme)
		{
			if (audioSource.isPlaying)
			{
				viMusicWasPlaying = true;
			}
			audioSource.Pause();
		}
		if (!S.I.menuMusicEnabled)
		{
			MuteMusic();
		}
	}

	public void OptionsClosed()
	{
		if (audioSource.clip == bossViolette.battleTheme && viMusicWasPlaying)
		{
			audioSource.Play();
		}
		else if (!S.I.menuMusicEnabled)
		{
			UnmuteMusic();
		}
	}

	public void MuteMusic()
	{
		if (!optionsMusicMuted)
		{
			optionsMusicMuted = true;
			SettingsPane.masterMixer.SetFloat("musicVol", -100f);
		}
	}

	public void UnmuteMusic()
	{
		if (optionsMusicMuted)
		{
			optionsMusicMuted = false;
			SettingsPane.masterMixer.SetFloat("musicVol", Mathf.Log10(Mathf.Clamp(S.I.optCtrl.settingsPane.musicVol / 100f, 0.0001f, 1f)) * 20f);
		}
	}
}
