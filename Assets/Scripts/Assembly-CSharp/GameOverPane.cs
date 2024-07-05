using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPane : NavPanel
{
	public NavButton rewardsButton;

	public NavButton rematchButton;

	public TMP_Text gameOverTitle;

	public TMP_Text charLvlText;

	public TMP_Text worldZoneText;

	public TMP_Text runTimeText;

	public TMP_Text killsText;

	public Transform killsBox;

	public KillRep killRepPrefab;

	public TMP_Text killedByText;

	public Image statsBackgroundLine;

	public TMP_Text seedText;

	public List<Image> playerSplashes;

	public GameObject pvpNamesGrid;

	public TMP_Text playerOneWinsText;

	public TMP_Text playerTwoWinsText;

	public BC ctrl;

	public IdleCtrl idCtrl;

	public RunCtrl runCtrl;

	public PvPSelectCtrl pvpSelectCtrl;

	public UnlockCtrl unCtrl;

	public AudioClip killRepSound;

	public Transform statsGrid;

	public StopWatch stopWatch;

	public CameraScript camCtrl;

	public Animator diamondBox;

	public List<Image> diamonds;

	public List<Color> diamondColors;

	public string killedByName;

	public bool processing = false;

	public bool fastForwardProcessing = false;

	private void Start()
	{
		ctrl = S.I.batCtrl;
		idCtrl = S.I.idCtrl;
		runCtrl = S.I.runCtrl;
		unCtrl = S.I.unCtrl;
		camCtrl = S.I.camCtrl;
		foreach (Transform item in statsGrid)
		{
			item.GetComponent<Animator>().SetBool("visible", false);
		}
	}

	public override void Open()
	{
		if (!ctrl.pvpMode)
		{
			defaultButton = rewardsButton;
		}
		else
		{
			defaultButton = rematchButton;
		}
		base.Open();
		diamondBox.SetBool("visible", true);
		PostCtrl.transitioning = false;
		ShowStats();
	}

	public override void Close()
	{
		if (ctrl.GameState != GState.GameOver)
		{
			base.Close();
		}
	}

	public void Continue()
	{
		if (ctrl.GameState != GState.Unlock)
		{
			if (processing)
			{
				fastForwardProcessing = true;
				return;
			}
			ctrl.GameState = GState.Unlock;
			StartCoroutine(_Continue());
		}
	}

	private IEnumerator _Continue()
	{
		anim.SetBool("visible", false);
		foreach (Transform child2 in statsGrid.transform)
		{
			child2.GetComponent<Animator>().SetBool("visible", false);
		}
		foreach (Transform child in pvpNamesGrid.transform)
		{
			child.GetComponent<Animator>().SetBool("visible", false);
		}
		yield return new WaitForSeconds(0.2f);
		unCtrl.PreOpen();
		yield return new WaitForSeconds(0.6f);
		ctrl.idCtrl.deckScreen.Close();
		_003C_003En__0();
		killsBox.DestroyChildren();
		unCtrl.StartUnlocks();
	}

	public void CharacterSelect()
	{
		if (ctrl.GameState == GState.GameOver)
		{
			ctrl.GameState = GState.Unlock;
			StartCoroutine(_CharacterSelect());
		}
	}

	private IEnumerator _CharacterSelect()
	{
		anim.SetBool("visible", false);
		diamondBox.SetTrigger("next");
		foreach (Transform child2 in statsGrid.transform)
		{
			child2.GetComponent<Animator>().SetBool("visible", false);
		}
		foreach (Transform child in pvpNamesGrid.transform)
		{
			child.GetComponent<Animator>().SetBool("visible", false);
		}
		yield return new WaitForSeconds(0.4f);
		yield return new WaitForSeconds(0.4f);
		camCtrl.CameraChangePos(0, true);
		_003C_003En__0();
		killsBox.DestroyChildren();
		pvpSelectCtrl.Open();
		yield return new WaitForSeconds(0.4f);
		diamondBox.SetTrigger("next");
		yield return new WaitForSeconds(0.1f);
		diamondBox.SetBool("visible", false);
	}

	public void Rematch()
	{
		if (ctrl.GameState == GState.GameOver)
		{
			ctrl.GameState = GState.Unlock;
			StartCoroutine(_Rematch());
		}
	}

	private IEnumerator _Rematch()
	{
		anim.SetBool("visible", false);
		diamondBox.SetTrigger("next");
		foreach (Transform child2 in statsGrid.transform)
		{
			child2.GetComponent<Animator>().SetBool("visible", false);
		}
		foreach (Transform child in pvpNamesGrid.transform)
		{
			child.GetComponent<Animator>().SetBool("visible", false);
		}
		yield return new WaitForSeconds(0.4f);
		camCtrl.TransitionInHigh("LeftWipe");
		yield return new WaitForSeconds(0.4f);
		_003C_003En__0();
		killsBox.DestroyChildren();
		ctrl.StartCoroutine(ctrl._RefreshPvP());
		yield return new WaitForSeconds(0.4f);
		diamondBox.SetTrigger("next");
		yield return new WaitForSeconds(0.1f);
		diamondBox.SetBool("visible", false);
	}

	public void SetType(Ending type)
	{
		charLvlText.text = "";
		switch (type)
		{
		case Ending.PacifistFalse:
			gameOverTitle.text = ScriptLocalization.UI.GameOver_Made_It_To_Eden;
			break;
		case Ending.PacifistTrue:
			gameOverTitle.text = ScriptLocalization.UI.GameOver_Made_It_To_Eden;
			break;
		case Ending.Genocide:
			gameOverTitle.text = ScriptLocalization.UI.GameOver_Destroyed_Eden;
			break;
		}
	}

	public void ShowStats()
	{
		if (runCtrl.currentRun == null)
		{
			return;
		}
		if (S.I.currentProfile == 0)
		{
			SaveDataCtrl.Set("TotalPlaytime", SaveDataCtrl.Get("TotalPlaytime", 0) + ctrl.runStopWatch.RoundedTimeInSeconds());
		}
		else
		{
			SaveDataCtrl.Set("TotalPlaytime" + S.I.currentProfile, SaveDataCtrl.Get("TotalPlaytime" + S.I.currentProfile, 0) + ctrl.runStopWatch.RoundedTimeInSeconds());
		}
		SaveDataCtrl.Set("TotalPlaytime" + S.I.currentProfile, SaveDataCtrl.Get("TotalPlaytime" + S.I.currentProfile, 0) + ctrl.runStopWatch.RoundedTimeInSeconds());
		SaveDataCtrl.Set(ctrl.currentPlayer.beingObj.nameString + "TotalPlaytime", SaveDataCtrl.Get(ctrl.currentPlayer.beingObj.nameString + "TotalPlaytime", 0) + ctrl.runStopWatch.RoundedTimeInSeconds());
		runTimeText.text = ScriptLocalization.UI.Run_time + " " + ctrl.runStopWatch.FormattedTime();
		ctrl.runStopWatch.Pause();
		ctrl.runStopWatch.Reset();
		rematchButton.gameObject.SetActive(ctrl.pvpMode);
		if (!ctrl.pvpMode)
		{
			statsBackgroundLine.enabled = true;
			idCtrl.anim.SetBool("OnScreen", true);
			pvpNamesGrid.SetActive(false);
			rewardsButton.right = null;
			foreach (Image diamond in diamonds)
			{
				diamond.color = diamondColors[0];
			}
			foreach (Image playerSplash in playerSplashes)
			{
				playerSplash.color = Color.clear;
			}
			int worldTierNum = runCtrl.currentRun.worldTierNum;
			worldZoneText.text = runCtrl.GetZoneText(runCtrl.currentRun.zoneNum);
			charLvlText.text = string.Format("{0}", ctrl.currentHeroObj.localizedName);
			charLvlText.text = "";
			if (!runCtrl.currentRun.seedWasPredefined)
			{
				SaveDataCtrl.Set("blessings", Mathf.Clamp(Mathf.CeilToInt(runCtrl.currentRun.worldTierNum / 2), 0, 2));
			}
			if (worldTierNum < 3)
			{
				gameOverTitle.text = ScriptLocalization.UI.GameOver_One_Mile;
			}
			else if (worldTierNum < 5)
			{
				gameOverTitle.text = ScriptLocalization.UI.GameOver_One_Leap;
			}
			else if (worldTierNum < 7)
			{
				gameOverTitle.text = ScriptLocalization.UI.GameOver_One_Skip;
			}
			else
			{
				gameOverTitle.text = ScriptLocalization.UI.GameOver_One_Step;
			}
			StartCoroutine(CreateKillReps());
			seedText.text = ScriptLocalization.UI.Worldbar_Seed + " " + runCtrl.currentRun.seed;
			killedByText.text = string.Format("They were killed by {0}.", killedByName);
			killedByText.text = "";
			if (ctrl.credCtrl.missedCredits.Count > 0)
			{
				killedByText.text = string.Format("{0} {1}/{2}", ScriptLocalization.UI.GameOver_Credits, ctrl.credCtrl.spawnedCredits - (ctrl.credCtrl.missedCredits.Count + ctrl.credCtrl.creditBeingsLeftAlive), ctrl.credCtrl.totalNumCredits);
			}
		}
		else
		{
			statsBackgroundLine.enabled = false;
			pvpNamesGrid.SetActive(true);
			for (int i = 0; i < 4; i++)
			{
				if (i % 2 == 0 || i == 0)
				{
					diamonds[i].color = diamondColors[1];
				}
				else
				{
					diamonds[i].color = diamondColors[2];
				}
			}
			foreach (Image playerSplash2 in playerSplashes)
			{
				playerSplash2.color = Color.white;
			}
			rematchButton.left = rewardsButton;
			rewardsButton.right = rematchButton;
			charLvlText.text = ScriptLocalization.UI.Battle_Winner;
			seedText.text = "";
			killedByText.text = "";
			worldZoneText.text = "";
			killsText.text = "";
			if (ctrl.deCtrl.duelDisks.Count > 1)
			{
				playerOneWinsText.text = string.Format("{0} {1}", ScriptLocalization.UI.GameOver_Wins, ctrl.deCtrl.duelDisks[0].wins);
				playerTwoWinsText.text = string.Format("{0} {1}", ScriptLocalization.UI.GameOver_Wins, ctrl.deCtrl.duelDisks[1].wins);
				gameOverTitle.text = ctrl.deCtrl.duelDisks[0].beingObj.localizedName ?? "";
				if (ctrl.deCtrl.duelDisks[1].wins > ctrl.deCtrl.duelDisks[0].wins)
				{
					gameOverTitle.text = ctrl.deCtrl.duelDisks[1].beingObj.localizedName ?? "";
					playerSplashes[0].color = Color.grey;
				}
				else
				{
					playerSplashes[1].color = Color.grey;
				}
			}
			StartCoroutine(ShowPvPGrid());
		}
		StartCoroutine(ShowStatsGrid());
	}

	private IEnumerator ShowStatsGrid()
	{
		yield return new WaitForSeconds(0.7f);
		foreach (Transform child in statsGrid)
		{
			child.GetComponent<Animator>().SetBool("visible", true);
			yield return new WaitForSeconds(0.3f);
		}
	}

	private IEnumerator ShowPvPGrid()
	{
		yield return new WaitForSeconds(0.7f);
		foreach (Transform child in pvpNamesGrid.transform)
		{
			if (child.GetSiblingIndex() % 2 == 0)
			{
				yield return new WaitForSeconds(0.3f);
			}
			child.GetComponent<Animator>().SetBool("visible", true);
		}
	}

	private IEnumerator CreateKillReps()
	{
		if (processing)
		{
			yield break;
		}
		fastForwardProcessing = false;
		processing = true;
		killsBox.DestroyChildren();
		int numKills = 0;
		killsText.text = string.Format(ScriptLocalization.UI.GameOver_Kills + " {0}", numKills);
		yield return StartCoroutine(ShowStatsGrid());
		int totalKills = ctrl.killedBeingSprites.Count;
		GridLayoutGroup killsBoxGL = killsBox.GetComponent<GridLayoutGroup>();
		float xSpacing = 10f;
		float ySpacing = 0f;
		int boxWidth = 360;
		if (totalKills > 40)
		{
			xSpacing = (float)(boxWidth / 20) - (float)(totalKills - boxWidth / 10) * 20f / (float)totalKills;
		}
		if (totalKills > 160)
		{
			xSpacing = (float)(boxWidth / 20) - (float)(80 - boxWidth / 10) * 20f / 80f;
			ySpacing = (0f - killsBoxGL.cellSize.y) * (float)(totalKills / 30 - 2) / (float)(totalKills / 30);
		}
		killsBoxGL.spacing = new Vector2(xSpacing, ySpacing);
		if (ctrl.killedBeingSprites.Count > 0)
		{
			yield return new WaitForSeconds(0.2f);
		}
		else
		{
			yield return new WaitForSeconds(0.3f);
		}
		idCtrl.HideOnwardButton();
		btnCtrl.SetFocus(defaultButton);
		foreach (Sprite beingSprite in ctrl.killedBeingSprites)
		{
			if (!(beingSprite == null))
			{
				KillRep newKillRep = Object.Instantiate(killRepPrefab, killsBox);
				newKillRep.image.sprite = beingSprite;
				newKillRep.rectTransform.sizeDelta = new Vector2(beingSprite.rect.width, beingSprite.rect.height);
				newKillRep.transform.position += Vector3.back * numKills;
				numKills++;
				killsText.text = string.Format(ScriptLocalization.UI.GameOver_Kills + " {0}", numKills);
				float repTime = ((numKills < 20) ? 0.05f : ((numKills < 50) ? 0.04f : ((numKills >= 80) ? 0.02f : 0.03f)));
				if (numKills > totalKills - 4)
				{
					repTime = 0.3f;
				}
				S.I.PlayOnce(killRepSound);
				if (fastForwardProcessing)
				{
					yield return new WaitForSeconds(0.01f);
				}
				else
				{
					yield return new WaitForSeconds(repTime);
				}
			}
		}
		idCtrl.HideOnwardButton();
		processing = false;
		fastForwardProcessing = false;
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private void _003C_003En__0()
	{
		base.Close();
	}
}
