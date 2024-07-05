using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossIrelia : Boss
{
	public float[] orbitalLerpTime;

	public bool attacking = false;

	public bool teleSlash = false;

	public bool quadSlash = false;

	public bool swordRows = false;

	public bool trackerSwords = false;

	public bool rainSlash = false;

	public bool blizzard = false;

	public Transform swordBox;

	[SerializeField]
	private Sprite winterSwordSprite = null;

	public List<SwordPoint> swordPoints;

	public Follower swordPrefab;

	public List<Follower> swords;

	public override void Start()
	{
		base.Start();
		for (int i = 0; i < swordPoints.Count; i++)
		{
			Follower follower = UnityEngine.Object.Instantiate(swordPrefab, swordPoints[i].transform.position, swordPoints[i].transform.rotation);
			follower.transform.SetParent(ctrl.transform);
			if (i < 2)
			{
				follower.GetComponent<SpriteRenderer>().sortingOrder = spriteRend.sortingOrder + 1;
			}
			if (DateTime.Now.Month == 12)
			{
				follower.GetComponent<SpriteRenderer>().sprite = winterSwordSprite;
				anim.runtimeAnimatorController = ctrl.itemMan.GetAnim("SelicyHoliday");
			}
			follower.target = swordPoints[i].transform;
			swords.Add(follower);
		}
		if (S.I.EDITION == Edition.DemoLive)
		{
			ctrl.blockAllSummons = false;
		}
	}

	public override void StartAction()
	{
		mov.PatrolRandom();
	}

	private void StartBlizzard()
	{
		StartCoroutine(Blizzard());
	}

	public override IEnumerator Loop()
	{
		if (!introPlayed && tier > 2)
		{
			StartBlizzard();
		}
		yield return StartCoroutine(_StartDialogue("Intro"));
		int arger = 0;
		while (true)
		{
			LoopStart();
			if (stage == 0)
			{
				if (S.I.EDITION == Edition.Dev && S.I.BOSS_TEST_MODE)
				{
					if (teleSlash)
					{
						yield return StartCoroutine(TeleSlash());
					}
					else if (quadSlash)
					{
						yield return StartCoroutine(QuadSlash());
					}
					else if (swordRows)
					{
						yield return StartCoroutine(SwordRows());
					}
					else if (trackerSwords)
					{
						yield return StartCoroutine(TrackerSwords());
					}
					else if (rainSlash)
					{
						yield return new WaitForSeconds(0.5f);
						yield return StartCoroutine(RainSlash());
						yield return new WaitForSeconds(0.5f);
						yield return StartCoroutine(TrackerSwords());
					}
					else if (blizzard)
					{
						yield return StartCoroutine(Blizzard());
					}
					yield return new WaitForSecondsRealtime(2f);
					continue;
				}
				if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(SwordRows());
				}
				else
				{
					yield return StartCoroutine(QuadSlash());
				}
			}
			else if (stage == 1)
			{
				if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(TeleSlash());
				}
				else if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(QuadSlash());
				}
				else
				{
					yield return StartCoroutine(SwordRows());
				}
			}
			else if (stage == 2)
			{
				if (Utils.RandomBool(2))
				{
					yield return StartCoroutine(TrackerSwords());
				}
				else if (Utils.RandomBool(4))
				{
					yield return StartCoroutine(QuadSlash());
				}
				else
				{
					yield return StartCoroutine(RainSlash());
				}
			}
			yield return new WaitForSeconds(loopDelay);
			for (int i = 0; i < 2; i++)
			{
				mov.MoveToRandom();
				yield return new WaitUntil(() => mov.state == State.Idle);
				yield return new WaitForSeconds(beingObj.movementDelay);
			}
			StageCheck();
			arger++;
		}
	}

	private IEnumerator TeleSlash()
	{
		yield return legacySpellList[1].StartCoroutine(legacySpellList[1].Cast());
	}

	private IEnumerator QuadSlash()
	{
		yield return legacySpellList[0].StartCoroutine(legacySpellList[0].Cast());
	}

	private IEnumerator SwordRows()
	{
		yield return legacySpellList[2].StartCoroutine(legacySpellList[2].Cast());
	}

	private IEnumerator TrackerSwords()
	{
		yield return legacySpellList[3].StartCoroutine(legacySpellList[3].Cast());
	}

	private IEnumerator RainSlash()
	{
		yield return legacySpellList[4].StartCoroutine(legacySpellList[4].Cast());
	}

	private IEnumerator Blizzard()
	{
		yield return new WaitForSeconds(0.8f);
		CastSpellObj("SelicyBlizzard");
	}

	public override void Remove()
	{
		foreach (Follower sword in swords)
		{
			UnityEngine.Object.Destroy(sword.gameObject);
		}
		base.Remove();
	}

	protected override void DownEffects()
	{
		base.DownEffects();
		foreach (Follower sword in swords)
		{
			sword.gameObject.SetActive(true);
		}
		S.AddSkinUnlock("SelicyHoliday");
	}

	public override IEnumerator ExecutePlayerC()
	{
		foreach (Follower sword in swords)
		{
			sword.gameObject.SetActive(true);
		}
		yield return new WaitForSeconds(0.2f);
		ResetAnimTriggers();
		yield return new WaitForSeconds(0.1f);
		mov.MoveToTile(battleGrid.Get(new TileApp(Location.HalfField), 1, ctrl.currentPlayer)[0]);
		yield return new WaitForSeconds(0.3f);
		yield return legacySpellList[5].StartCoroutine(legacySpellList[5].Cast());
		yield return new WaitForSeconds(0.3f);
		if (ctrl.PlayersActive())
		{
			if (tier > 2)
			{
				StartBlizzard();
			}
			StartCoroutine(StartLoopC());
		}
	}

	public override IEnumerator DownTalkL()
	{
		S.I.demoLiveCtrl.WinMessage();
		talkBubble.Show();
		yield return new WaitForSeconds(0.1f);
		yield return new WaitForSeconds(talkBubble.AnimateText("Oof.@<nx>You win.@<nx>This time...@<nx>EndDemo();"));
		talkBubble.Hide();
		yield return new WaitForSeconds(0.6f);
		ctrl.StartCoroutine(ctrl.DemoEndC());
	}

	public IEnumerator DownTalkD()
	{
		AddInvince(10f);
		yield return new WaitForSeconds(0.5f);
		talkBubble.Show();
		yield return new WaitForSeconds(0.1f);
		talkBubble.AnimateText("Oh no.");
		yield return new WaitForSeconds(1.3f);
		talkBubble.AnimateText("EndDemo();!");
		yield return new WaitForSeconds(1.5f);
		talkBubble.Hide();
		yield return new WaitForSeconds(0.2f);
		ctrl.StartCoroutine(ctrl.DemoEndC());
	}

	public IEnumerator DownTalkH()
	{
		AddInvince(2f);
		col.enabled = true;
		talkBubble.Show();
		yield return new WaitForSeconds(0.1f);
		talkBubble.AnimateText("hah.");
		yield return new WaitForSeconds(1.6f);
		talkBubble.AnimateText("I can still");
		yield return new WaitForSeconds(1.2f);
		talkBubble.AnimateText("EndDemo()");
		yield return new WaitForSeconds(1.5f);
		talkBubble.Hide();
		yield return new WaitForSeconds(0.5f);
		talkBubble.Show();
		talkBubble.AnimateText("...");
		yield return new WaitForSeconds(1.5f);
		talkBubble.AnimateText("EndDemo()!");
		yield return new WaitForSeconds(1.5f);
		talkBubble.AnimateText("...!");
		yield return new WaitForSeconds(1.4f);
		talkBubble.AnimateText("ENDDEMO();ENDDEMO();ENDDEMO();ENDDEMO();ENDDEMO();ENDDEMO();ENDDEMO();ENDDEMO();ENDDEMO();ENDDEMO();");
		yield return new WaitForSeconds(2.4f);
		ctrl.StartCoroutine(ctrl.DemoEndC());
	}

	public override void StartDeath(bool triggerDeathrattles = true)
	{
		if (downed && S.I.EDITION == Edition.DemoLive)
		{
			endGameOnExecute = false;
			ctrl.RemoveObstacle(this);
			talkBubble.AnimateText("EN-");
			ctrl.StartCoroutine(ctrl.DemoEndC(1.7f));
		}
		base.StartDeath(triggerDeathrattles);
	}
}
