using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialCtrl : NavPanel
{
	public Image tutorialBg;

	public TMP_Text tutorialTitleText;

	public GameObject continueBox;

	public List<Animator> tutorialOvs;

	public GameObject tutorialOvsContainer;

	public List<InputIcon> ovsInputIcons;

	public List<VideoClip> ovsClips;

	private int ovsProgress = 0;

	public GameObject keyboardArrows;

	public VideoPlayer vidPlayer;

	public VideoPlayer vidPlayerAlt;

	public MeshRenderer videoScreen;

	public MeshRenderer videoScreenAlt;

	public List<Animator> tutorialPaths;

	public GameObject tutorialPathsContainer;

	public List<VideoClip> pathsClips;

	private int pathsProgress = 0;

	public List<Animator> tutorialDeck;

	public GameObject tutorialDeckContainer;

	public List<VideoClip> deckClips;

	private int deckProgress = 0;

	public bool tutorialOvsInProgress = false;

	public bool tutorialPathsInProgress = false;

	public bool tutorialDeckInProgress = false;

	public bool tutorialInProgress = false;

	public Animator videoBox;

	public CanvasGroup canvasGroup;

	public Canvas tutorialOverlayCanvas;

	public BC ctrl;

	public OptionCtrl optCtrl;

	private bool preloading = false;

	private bool savedPoCtrlTransitioning = false;

	protected override void Awake()
	{
		base.Awake();
		ovsProgress = 0;
		pathsProgress = 0;
		deckProgress = 0;
		tutorialOvsContainer.SetActive(false);
		tutorialPathsContainer.SetActive(false);
		tutorialDeckContainer.SetActive(false);
		anim = GetComponent<Animator>();
		anim.updateMode = AnimatorUpdateMode.UnscaledTime;
		optCtrl = S.I.optCtrl;
		SetVideoSortingLayers();
		anim.SetBool("visible", false);
	}

	public void ResetTutorialProgress()
	{
		ovsProgress = 0;
		pathsProgress = 0;
		deckProgress = 0;
	}

	private void Start()
	{
		vidPlayer.clip = ovsClips[ovsProgress];
		vidPlayer.Prepare();
		videoScreen.enabled = false;
		videoScreenAlt.enabled = false;
		HideTutorialOverlay(true);
	}

	private void SetVideoSortingLayers()
	{
		videoScreen.sortingLayerName = tutorialOverlayCanvas.sortingLayerName;
		videoScreen.sortingOrder = tutorialOverlayCanvas.sortingOrder;
		videoScreenAlt.sortingLayerName = tutorialOverlayCanvas.sortingLayerName;
		videoScreenAlt.sortingOrder = tutorialOverlayCanvas.sortingOrder;
	}

	private void ActivateTutorial()
	{
		continueBox.SetActive(true);
		tutorialBg.gameObject.SetActive(true);
		tutorialTitleText.gameObject.SetActive(true);
		Open();
		SetVideoSortingLayers();
		savedPoCtrlTransitioning = PostCtrl.transitioning;
		PostCtrl.transitioning = false;
	}

	public void StartTutorialOv()
	{
		if (vidPlayer.clip != ovsClips[0])
		{
			vidPlayer.clip = ovsClips[0];
			vidPlayer.Prepare();
		}
		SaveDataCtrl.Set("TutorialOverlayShown", true);
		ActivateTutorial();
		tutorialOvsContainer.gameObject.SetActive(true);
		tutorialOvsInProgress = true;
		tutorialInProgress = true;
		foreach (Animator tutorialOv in tutorialOvs)
		{
			tutorialOv.gameObject.SetActive(true);
		}
		ContinueAnyTutorial();
	}

	public void ContinueAnyTutorial()
	{
		if (tutorialOvsInProgress)
		{
			if (tutorialOvs.Count <= ovsProgress)
			{
				ctrl.RemoveControlBlocksNextFrame(Block.Tutorial);
			}
			ContinueTutorial(tutorialOvs, ovsClips, ref ovsProgress);
		}
		else if (tutorialPathsInProgress)
		{
			if (tutorialPaths.Count <= pathsProgress)
			{
				ctrl.RemoveControlBlocksNextFrame(Block.Tutorial);
			}
			ContinueTutorial(tutorialPaths, pathsClips, ref pathsProgress);
		}
		else if ((bool)tutorialDeckContainer)
		{
			if (tutorialDeck.Count <= deckProgress)
			{
				ctrl.RemoveControlBlocksNextFrame(Block.Tutorial);
			}
			ContinueTutorial(tutorialDeck, deckClips, ref deckProgress);
		}
	}

	private void ContinueTutorial(List<Animator> tutorialList, List<VideoClip> vidClips, ref int progressNum)
	{
		if (tutorialList.Count <= progressNum)
		{
			EndTutorial();
			return;
		}
		videoBox.SetTrigger("show");
		videoBox.SetBool("visible", true);
		if (progressNum == 0)
		{
			vidPlayer.clip = vidClips[progressNum];
			vidPlayer.Prepare();
		}
		tutorialTitleText.text = string.Format("{2} ({0}/{1})", progressNum + 1, tutorialList.Count, ScriptLocalization.UI.Tutorial);
		if (progressNum % 2 == 0)
		{
			StartCoroutine(PlayAfterLoaded(vidPlayer, videoScreen, videoScreenAlt));
			if (progressNum + 1 < vidClips.Count)
			{
				vidPlayerAlt.clip = vidClips[progressNum + 1];
				vidPlayerAlt.Prepare();
			}
		}
		else
		{
			StartCoroutine(PlayAfterLoaded(vidPlayerAlt, videoScreenAlt, videoScreen));
			if (progressNum + 1 < vidClips.Count)
			{
				vidPlayer.clip = vidClips[progressNum + 1];
				vidPlayer.Prepare();
			}
		}
		tutorialList[progressNum].SetBool("OnScreen", true);
		UpdateTutIcons();
		progressNum++;
	}

	private IEnumerator PlayAfterLoaded(VideoPlayer theVidPlayer, MeshRenderer display, MeshRenderer hide)
	{
		preloading = true;
		hide.enabled = false;
		while (!theVidPlayer.isPrepared)
		{
			preloading = true;
			yield return null;
		}
		theVidPlayer.Play();
		yield return null;
		display.enabled = true;
		hide.enabled = false;
		preloading = false;
	}

	public void SkipTutorialOv()
	{
		ovsProgress = tutorialOvs.Count;
	}

	public void StartTutorialPath()
	{
		if (vidPlayer.clip != pathsClips[0])
		{
			vidPlayer.clip = pathsClips[0];
			vidPlayer.Prepare();
		}
		SaveDataCtrl.Set("TutorialPathShown", true);
		ctrl.AddControlBlocks(Block.Tutorial);
		tutorialPathsContainer.gameObject.SetActive(true);
		ActivateTutorial();
		tutorialPathsInProgress = true;
		tutorialInProgress = true;
		foreach (Animator tutorialPath in tutorialPaths)
		{
			tutorialPath.gameObject.SetActive(true);
		}
		ContinueAnyTutorial();
	}

	public void StartTutorialDeck()
	{
		if (vidPlayer.clip != deckClips[0])
		{
			vidPlayer.clip = deckClips[0];
			vidPlayer.Prepare();
		}
		SaveDataCtrl.Set("TutorialDeckShown", true);
		ctrl.AddControlBlocks(Block.Tutorial);
		tutorialDeckContainer.gameObject.SetActive(true);
		ActivateTutorial();
		tutorialDeckInProgress = true;
		tutorialInProgress = true;
		foreach (Animator item in tutorialDeck)
		{
			item.gameObject.SetActive(true);
		}
		ContinueAnyTutorial();
	}

	public void SkipTutorial()
	{
		if (tutorialOvsInProgress)
		{
			ovsProgress = tutorialOvs.Count;
		}
		else if (tutorialPathsInProgress)
		{
			pathsProgress = tutorialPaths.Count;
		}
		else if ((bool)tutorialDeckContainer)
		{
			deckProgress = tutorialDeck.Count;
		}
		ContinueAnyTutorial();
	}

	private void UpdateTutIcons()
	{
		foreach (InputIcon ovsInputIcon in ovsInputIcons)
		{
			ovsInputIcon.UpdateDisplay();
		}
	}

	public void EndTutorial()
	{
		if (tutorialOvsInProgress)
		{
			vidPlayer.clip = pathsClips[0];
			vidPlayer.Prepare();
		}
		else if (tutorialPathsInProgress)
		{
			vidPlayer.clip = deckClips[0];
			vidPlayer.Prepare();
		}
		tutorialOvsInProgress = false;
		tutorialPathsInProgress = false;
		tutorialDeckInProgress = false;
		tutorialInProgress = false;
		videoScreen.enabled = false;
		videoScreenAlt.enabled = false;
		HideTutorialOverlay(false);
		PostCtrl.transitioning = savedPoCtrlTransitioning;
	}

	public void HideTutorialOverlay(bool instant)
	{
		StartCoroutine(_HideTutorialOverlay(instant));
	}

	private IEnumerator _HideTutorialOverlay(bool instant)
	{
		foreach (Animator animtr2 in tutorialOvs)
		{
			animtr2.gameObject.SetActive(false);
		}
		foreach (Animator animtr in tutorialPaths)
		{
			animtr.gameObject.SetActive(false);
		}
		if (videoBox.isActiveAndEnabled)
		{
			videoBox.SetBool("visible", false);
		}
		if (!instant)
		{
			yield return new WaitForSeconds(0.3f);
		}
		Close();
		yield return new WaitForSeconds(0.5f);
		while (preloading)
		{
			yield return null;
		}
		tutorialBg.gameObject.SetActive(false);
		continueBox.SetActive(false);
		tutorialTitleText.gameObject.SetActive(false);
	}
}
