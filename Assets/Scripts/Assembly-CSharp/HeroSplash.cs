using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeroSplash : MonoBehaviour
{
	public Image image;

	public Animator anim;

	public BeingObject queuedHero = null;

	public bool splashIsQueued = false;

	private bool splashTransitioning = false;

	private Coroutine co_changeSplash;

	private bool hidden = false;

	public string shownHeroSplash = "";

	public void ChangeSplash(BeingObject thisHero)
	{
		StartCoroutine(_ChangeSplash(thisHero));
	}

	private IEnumerator _ChangeSplash(BeingObject thisHero)
	{
		if (splashTransitioning)
		{
			queuedHero = thisHero;
			if (splashIsQueued)
			{
				yield break;
			}
			splashIsQueued = true;
		}
		while (splashTransitioning)
		{
			yield return null;
		}
		if (splashIsQueued)
		{
			splashIsQueued = false;
		}
		else
		{
			queuedHero = thisHero;
		}
		if (!(image.sprite == S.I.itemMan.GetSprite(queuedHero.splashSprite)) || hidden || splashTransitioning)
		{
			hidden = false;
			splashTransitioning = true;
			anim.SetBool("OnScreen", false);
			yield return null;
			while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f && !anim.IsInTransition(0))
			{
				yield return null;
			}
			image.sprite = S.I.itemMan.GetSprite(queuedHero.splashSprite);
			shownHeroSplash = queuedHero.splashSprite;
			anim.SetBool("OnScreen", true);
			splashTransitioning = false;
		}
	}

	public void Hide()
	{
		StopAllCoroutines();
		queuedHero = null;
		splashIsQueued = false;
		splashTransitioning = false;
		hidden = true;
		anim.SetBool("OnScreen", false);
	}
}
