using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsBackDisplay : MonoBehaviour
{
	public Animator anim;

	public GameObject controlKeysContainer;

	public CanvasGroup controlsContainer;

	public CanvasGroup keysContainer;

	public Animator pointersAnim;

	public SlideBody slideBody;

	public List<CanvasGroup> idleControls;

	public CanvasGroup canvasGroup;

	public BC ctrl;

	public OptionCtrl optCtrl;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		optCtrl = S.I.optCtrl;
		controlKeysContainer.SetActive(false);
		keysContainer.alpha = 0f;
		controlsContainer.alpha = 0f;
		anim.SetBool("visible", false);
	}

	public void ShowControlsPanel(bool battle)
	{
		StartCoroutine(TutorialControlsPanelC(battle));
	}

	private IEnumerator TutorialControlsPanelC(bool battle)
	{
		canvasGroup.alpha = 1f;
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSecondsRealtime(1.5f);
		}
		slideBody.Show();
		anim.SetBool("visible", true);
		controlKeysContainer.SetActive(true);
		controlsContainer.gameObject.SetActive(true);
		keysContainer.gameObject.SetActive(true);
		pointersAnim.gameObject.SetActive(battle);
		pointersAnim.SetBool("visible", true);
		UpdateControlOverlay(ControlsCtrl.ControllerIDIsKeyboard(S.I.conCtrl.playerControllerIDs[0]));
		float alpha = idleControls[0].alpha;
		float alphaTick = 0.07f;
		if (battle)
		{
			alphaTick *= -1f;
		}
		while ((battle && alpha > 0f) || (!battle && alpha < 1f))
		{
			alpha += alphaTick;
			foreach (CanvasGroup go in idleControls)
			{
				go.alpha = alpha;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public void HideTutorialControlBacklay()
	{
		anim.SetBool("visible", false);
		if (pointersAnim.gameObject.activeInHierarchy)
		{
			pointersAnim.SetBool("visible", false);
		}
	}

	public void UpdateControlOverlay(bool keyboard)
	{
		if (keyboard)
		{
			keysContainer.alpha = 1f;
			controlsContainer.alpha = 0f;
		}
		else
		{
			controlsContainer.alpha = 1f;
			keysContainer.alpha = 0f;
		}
	}
}
