using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[MoonSharpUserData]
public class Cardtridge : SerializedMonoBehaviour
{
	public TMP_Text mana;

	public Image image;

	public Image imageOverlay;

	public Image imageOverback;

	public Image imageBelt;

	public TMP_Text label;

	public RectTransform mask;

	public ShuffleTrail shuffleTrailPrefab;

	public SpellObject spellObj;

	public Rigidbody2D rBody;

	public Animator anim;

	public bool refCard = false;

	public Vector2 minForce;

	public float minTorque;

	public Vector2 maxForce;

	public float maxTorque;

	public float lerpTime = 0.2f;

	public Canvas canvas;

	public CanvasGroup canvasGroup;

	public bool visible = false;

	public bool ejected = false;

	public GameObject particleBoxPrefab;

	public DuelDisk duelDisk;

	public Dictionary<FTrigger, ParticleSystem> activeEffects = new Dictionary<FTrigger, ParticleSystem>();

	private Vector3 startPos;

	private float currentLerpTime = 0f;

	public int height;

	private void Awake()
	{
		height = Mathf.FloorToInt(GetComponent<RectTransform>().sizeDelta.y) - 4;
		mana.text = "";
		imageBelt.enabled = false;
		image.color = new Color(1f, 1f, 1f, 1f);
		image.fillAmount = 0f;
		label.text = "";
		anim.updateMode = AnimatorUpdateMode.UnscaledTime;
	}

	private void Update()
	{
		if (refCard)
		{
			return;
		}
		if (visible)
		{
			if (duelDisk == null)
			{
				SimplePool.Despawn(base.gameObject);
			}
			else if (base.transform.position.y - duelDisk.transform.position.y > -150f && base.transform.position.y - duelDisk.transform.position.y < 120f)
			{
				canvasGroup.alpha = Mathf.Clamp((100f - base.transform.position.y - duelDisk.transform.position.y) / 100f, 0.2f, 1f);
			}
			else if (base.transform.localPosition.y <= 20f && canvasGroup.alpha != 1f)
			{
				if (base.transform.localPosition.y < -200f)
				{
					rBody.simulated = false;
				}
				else if (!ejected)
				{
					canvasGroup.alpha = 1f;
				}
			}
		}
		else if (canvasGroup.alpha != 0f)
		{
			canvasGroup.alpha = 0f;
		}
	}

	public void Reset(Transform parent, Vector3 position)
	{
		rBody.velocity = Vector2.zero;
		rBody.gravityScale = 0f;
		rBody.angularVelocity = 0f;
		rBody.simulated = true;
		anim.SetTrigger("Reset");
		base.transform.SetParent(parent, true);
		base.transform.position = position;
		base.transform.rotation = parent.rotation;
		mask.SetAll(0f);
		imageBelt.enabled = true;
		image.color = Color.gray;
		image.fillAmount = 1f;
		imageOverback.fillAmount = 0f;
		imageOverlay.fillAmount = 0f;
		StopAllCoroutines();
		canvas.sortingOrder = 0;
		canvasGroup.alpha = 0f;
		visible = false;
		ejected = false;
	}

	public void Eject()
	{
		imageBelt.enabled = false;
		rBody.AddForce(new Vector2(UnityEngine.Random.Range(minForce.x, maxForce.x), UnityEngine.Random.Range(minForce.y, maxForce.y)), ForceMode2D.Impulse);
		rBody.AddTorque(UnityEngine.Random.Range(minTorque, maxTorque), ForceMode2D.Impulse);
		rBody.gravityScale = 100f;
		canvas.sortingOrder = -1;
		image.color = new Color(0.7f, 0.7f, 0.7f, 1f);
		canvasGroup.alpha = 0.4f;
		ejected = true;
	}

	public Cardtridge Set(SpellObject spellObj, DuelDisk theDuelDisk)
	{
		this.spellObj = spellObj;
		duelDisk = theDuelDisk;
		spellObj.cardtridge = this;
		base.name = "Cardtridge - " + spellObj.nameString;
		if (spellObj.manaType.type != 0)
		{
			mana.text = "X";
		}
		else
		{
			mana.text = spellObj.mana.ToString();
		}
		image.sprite = spellObj.sprite;
		imageOverback.sprite = spellObj.sprite;
		imageOverlay.sprite = spellObj.sprite;
		imageBelt.enabled = true;
		visible = false;
		SetLabel(spellObj.shortName);
		return this;
	}

	public void SetInSlot()
	{
		base.transform.SetParent(base.transform);
		image.sprite = spellObj.sprite;
		image.fillAmount = 1f;
		imageOverlay.sprite = spellObj.sprite;
		imageOverback.sprite = spellObj.sprite;
		if (spellObj.HasTrigger(FTrigger.Hold))
		{
			AddGlow(FTrigger.Hold);
		}
		SetLabel(string.Empty);
	}

	public void SetRef(SpellObject theSpellObj)
	{
		image.sprite = theSpellObj.sprite;
		image.fillAmount = 1f;
		imageOverlay.sprite = theSpellObj.sprite;
		imageOverback.sprite = theSpellObj.sprite;
		canvas.sortingOrder = 2;
		if (theSpellObj.HasTrigger(FTrigger.Hold))
		{
			AddGlow(FTrigger.Hold);
		}
		SetLabel(theSpellObj.shortName);
	}

	public Cardtridge SetAsRef(Transform parent)
	{
		base.transform.SetParent(parent, false);
		image.fillAmount = 0f;
		imageOverback.fillAmount = 0f;
		imageOverlay.fillAmount = 0f;
		imageBelt.enabled = false;
		refCard = true;
		return this;
	}

	public void SetLabel(string labelText)
	{
		if (S.I.LABELS)
		{
			label.text = labelText;
		}
		else
		{
			label.text = string.Empty;
		}
	}

	public Cardtridge RemoveFromShuffleQueue(List<Cardtridge> shuffleQueue, List<Cardtridge> queuedCardtridges, CardGrid cardGrid, Vector3 position, Transform parent)
	{
		shuffleQueue.Remove(this);
		int num = queuedCardtridges.Count * height;
		Reset(cardGrid.transform, cardGrid.transform.position + new Vector3(0f, num, 0f));
		queuedCardtridges.Add(this);
		SetLabel(spellObj.shortName);
		CreateShuffleTrail(position, parent);
		return this;
	}

	public ShuffleTrail CreateShuffleTrail(Vector3 position, Transform parent)
	{
		return SimplePool.Spawn(shuffleTrailPrefab.gameObject, position, base.transform.rotation, parent).GetComponent<ShuffleTrail>().Set(this);
	}

	public void MoveTo(Transform theTransform)
	{
		StartCoroutine(MoveCardGrid(theTransform));
	}

	private IEnumerator MoveCardGrid(Transform theTransform)
	{
		startPos = base.transform.position;
		currentLerpTime = 0f;
		while (currentLerpTime < lerpTime)
		{
			yield return new WaitForEndOfFrame();
			currentLerpTime += Time.deltaTime;
			if (currentLerpTime > lerpTime)
			{
				currentLerpTime = lerpTime;
			}
			float t2 = currentLerpTime / lerpTime;
			t2 = Mathf.Sin(t2 * (float)Math.PI * 0.5f);
			base.transform.position = Vector3.Lerp(startPos, theTransform.position, t2);
		}
	}

	public void AddGlow(FTrigger fTrigger)
	{
		if (!activeEffects.ContainsKey(fTrigger))
		{
			ParticleSystem component = SimplePool.Spawn(particleBoxPrefab, base.transform.position, base.transform.rotation, base.transform).GetComponent<ParticleSystem>();
			ParticleSystem.ShapeModule shape = component.shape;
			if (refCard)
			{
				shape.scale = new Vector3(9f, 9f, 1f);
			}
			else
			{
				shape.scale = new Vector3(21f, 21f, 1f);
			}
			ParticleSystem.MainModule main = component.main;
			main.startColor = U.I.triggerPalette[fTrigger];
			activeEffects.Add(fTrigger, component);
			component.transform.SetParent(base.transform);
			component.transform.localScale = Vector3.one;
		}
	}

	public void RemoveGlow(FTrigger fTrigger)
	{
		if (activeEffects.ContainsKey(fTrigger))
		{
			SimplePool.Despawn(activeEffects[fTrigger].gameObject);
			activeEffects.Remove(fTrigger);
		}
	}

	public void RemoveAllGlows()
	{
		foreach (FTrigger key in activeEffects.Keys)
		{
			SimplePool.Despawn(activeEffects[key].gameObject);
		}
		activeEffects.Clear();
	}

	private void OnDisable()
	{
		RemoveAllGlows();
	}
}
