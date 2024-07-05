using System.Collections;
using TMPro;
using UnityEngine;

public class Cpu : Being
{
	[HideInInspector]
	public TMP_Text nameText;

	[HideInInspector]
	public int spawnNum;

	private bool activated = false;

	public override void Start()
	{
		base.Start();
		mov.alignNum = alignNum;
		for (int i = 0; i < legacyDeckList.Count; i++)
		{
			CreateLegacySpell(i);
		}
		if (beingObj.defense != 0)
		{
			ArtifactObject artifactObject = deCtrl.CreateArtifact("DefenseDisplay", this);
			artifactObject.GetEffect(Effect.DefenseBattle).amount = beingObj.defense;
			beingObj.defense = 0;
		}
	}

	public override void Activate()
	{
		base.Activate();
		if (!activated)
		{
			StartCoroutine(StartLoopC());
		}
	}

	public virtual IEnumerator StartLoopC()
	{
		activated = true;
		while (mov.state != 0)
		{
			yield return new WaitForEndOfFrame();
		}
		if (beingObj.stagger)
		{
			CalculateLoopDelay(true);
			yield return new WaitForSeconds((float)spawnNum * beingObj.loopDelay / 2f);
		}
		yield return new WaitForEndOfFrame();
		StartCoroutine(Loop());
	}

	public virtual IEnumerator Loop()
	{
		yield return new WaitForSeconds(beingObj.startDelay);
		while ((bool)base.gameObject)
		{
			if (!ctrl.PlayersActive())
			{
				yield return new WaitForSeconds(1f);
				continue;
			}
			yield return StartCoroutine(WaitUntilIdle());
			for (int i = 0; i < beingObj.movements; i++)
			{
				mov.Move(beingObj.movementPattern, beingObj.movements, i);
				yield return StartCoroutine(WaitUntilIdle());
				yield return new WaitForSeconds(beingObj.movementDelay);
			}
			StartAction();
			CalculateLoopDelay(false);
			float timeSinceStartLoopDelay = 0f;
			while (timeSinceStartLoopDelay < beingObj.loopDelay)
			{
				timeSinceStartLoopDelay += Time.deltaTime;
				CalculateLoopDelay(false);
				yield return null;
			}
		}
	}

	protected virtual void CalculateLoopDelay(bool staggerDelay)
	{
	}

	public virtual void StartAction()
	{
		StartCoroutine(_Action());
	}

	public virtual IEnumerator _Action()
	{
		if (spellObjList.Count > 0)
		{
			if (beingObj.chargeAnim && !anim.GetBool("fire") && !anim.GetCurrentAnimatorStateInfo(0).IsName("lift"))
			{
				anim.SetTrigger("lift");
			}
			new WaitForSeconds(0.1f);
			TriggerArtifacts(FTrigger.OnSpellCast);
			for (int i = 0; i < spellObjList.Count; i++)
			{
				spellObjList[i].StartCast();
				theSpellCast = spellObjList[i];
			}
		}
		yield return new WaitForEndOfFrame();
	}

	public void CreateLegacySpell(int spellNum)
	{
		GameObject gameObject = Object.Instantiate(legacyDeckList[spellNum], base.transform.position, base.transform.rotation);
		Spell component = gameObject.GetComponent<Spell>();
		component.spellObj = ctrl.deCtrl.CreateSpellBase("Default", this);
		component.spellObj.spell = component;
		component.spellObj.item = component;
		component.beingAnim = anim;
		component.transform.parent = base.transform;
		component.being = GetComponent<Being>();
		legacySpellList.Add(component);
	}

	public void CastSpellObj(string itemID)
	{
		GetSpellObj(itemID).StartCast();
	}

	public SpellObject GetSpellObj(string itemID)
	{
		foreach (SpellObject spellObj in spellObjList)
		{
			if (spellObj.itemID == itemID)
			{
				return spellObj;
			}
		}
		return null;
	}

	public override void Remove()
	{
		if ((bool)nameText)
		{
			Object.Destroy(nameText.gameObject);
		}
		base.Remove();
	}
}
