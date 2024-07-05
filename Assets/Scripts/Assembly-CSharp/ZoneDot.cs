using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ZoneDot : NavButton
{
	public ZoneType type;

	public Image fgImage;

	public Image bgImage;

	public Color defaultFGColor;

	public Color disabledFGColor;

	public Color darkFGColor;

	public Color disabledDarkFGColor;

	public List<ZoneDot> previousDots = new List<ZoneDot>();

	public List<ZoneDot> nextDots = new List<ZoneDot>();

	public ZoneDot pastDot;

	public LineRenderer lineRend;

	public LineRenderer pastLineRend;

	public LineRenderer nextLineRend;

	public int stepNum;

	public List<ZoneDot> savedFutureDots;

	public World world;

	public string worldName;

	public string imageName;

	public bool spareAvailable = false;

	public GameObject lineRePrefab;

	public List<RectTransform> nextLines;

	public Color defaultColor;

	public Color pathColor;

	public Color nextColor;

	public Color futureColor;

	public Color futureDarkColor;

	public Color futureDarkFGColor;

	public bool dark = false;

	public IdleCtrl idCtrl;

	public WorldBar worldBar;

	public float verticalSpacing;

	protected override void Awake()
	{
		rect = GetComponent<RectTransform>();
	}

	protected override void Update()
	{
		if (!worldBar || !worldBar.open)
		{
			return;
		}
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, ((float)(base.transform.parent.childCount - 3) / 2f - (float)(base.transform.GetSiblingIndex() - 1)) * verticalSpacing, base.transform.localPosition.z);
		for (int i = 0; i < nextLines.Count; i++)
		{
			if (nextDots.Count >= i)
			{
				Vector3 position = nextDots[i].transform.position;
				nextLines[i].transform.right = base.transform.position - position;
				nextLines[i].sizeDelta = new Vector2(Vector3.Distance(base.transform.position, position) * 1f / base.transform.lossyScale.x, 1f);
			}
		}
	}

	public void SetDefaultFGColor(float alpha = 1f, bool dark = false)
	{
		if (dark)
		{
			fgImage.color = futureDarkFGColor * new Color(1f, 1f, 1f, alpha);
		}
		else
		{
			fgImage.color = defaultFGColor * new Color(1f, 1f, 1f, alpha);
		}
	}

	public void SetDark()
	{
		dark = true;
		defaultFGColor = darkFGColor;
		disabledFGColor = disabledDarkFGColor;
	}

	public void CreateLines()
	{
		if (nextDots.Count == 0)
		{
			ClearLines();
			return;
		}
		for (int i = 0; i < nextDots.Count; i++)
		{
			CreateLine(nextDots[i].transform.position);
		}
	}

	public void ClearLines()
	{
		for (int num = nextLines.Count - 1; num >= 0; num--)
		{
			SimplePool.Despawn(nextLines[num].gameObject);
			nextLines.Remove(nextLines[num]);
		}
	}

	public void SetType(ZoneType zoneType)
	{
		type = zoneType;
		fgImage.enabled = true;
		if (type == ZoneType.World)
		{
			fgImage.sprite = worldBar.zoneSprites[imageName];
		}
		else
		{
			fgImage.sprite = worldBar.zoneSprites[type.ToString()];
		}
	}

	public void ColorNextLines()
	{
		foreach (RectTransform nextLine in nextLines)
		{
			nextLine.GetComponent<Image>().color = nextColor;
		}
		foreach (ZoneDot nextDot in nextDots)
		{
			nextDot.SetDefaultFGColor();
		}
	}

	public void ColorFutureLines(bool dark)
	{
		foreach (RectTransform nextLine in nextLines)
		{
			if (dark)
			{
				nextLine.GetComponent<Image>().color = futureDarkColor;
			}
			else
			{
				nextLine.GetComponent<Image>().color = futureColor;
			}
		}
		foreach (ZoneDot nextDot in nextDots)
		{
			nextDot.SetDefaultFGColor(0.8f, dark);
		}
	}

	public void KeepColorFor(ZoneDot zoneDot, bool colorNext = false)
	{
		ClearLineColors();
		if (colorNext)
		{
			ColorNextLines();
		}
		if (nextDots.Contains(zoneDot))
		{
			nextLines[nextDots.IndexOf(zoneDot)].GetComponent<Image>().color = pathColor;
			nextDots[nextDots.IndexOf(zoneDot)].SetDefaultFGColor();
		}
	}

	public void ClearLineColors()
	{
		foreach (RectTransform nextLine in nextLines)
		{
			nextLine.GetComponent<Image>().color = defaultColor;
		}
		foreach (ZoneDot nextDot in nextDots)
		{
			nextDot.fgImage.color = nextDot.disabledFGColor;
		}
	}

	public void CreateLine(Vector3 end)
	{
		RectTransform component = SimplePool.Spawn(lineRePrefab, Vector3.zero, base.transform.rotation).GetComponent<RectTransform>();
		component.SetParent(base.transform, false);
		component.SetSiblingIndex(0);
		component.transform.right = base.transform.position - end;
		component.sizeDelta = new Vector2(Vector3.Distance(base.transform.position, end), 1f);
		component.GetComponent<Image>().color = defaultColor;
		nextLines.Add(component);
	}

	public void ClearFuture()
	{
		foreach (ZoneDot savedFutureDot in savedFutureDots)
		{
			savedFutureDot.ClearLineColors();
		}
		savedFutureDots.Clear();
	}

	public IEnumerator HighlightFuture()
	{
		List<ZoneDot> futureDots = new List<ZoneDot>();
		ZoneDot futureDot2 = this;
		futureDots.Add(futureDot2);
		savedFutureDots.Add(futureDot2);
		if (futureDot2.nextDots.Count < 1)
		{
			yield break;
		}
		while (futureDots.Count > 0)
		{
			futureDot2 = futureDots[0];
			for (int i = 0; i < futureDot2.nextDots.Count; i++)
			{
				futureDots.Remove(futureDot2);
				if (futureDot2.nextDots[i].nextDots.Count > 0)
				{
					futureDots.Add(futureDot2.nextDots[i]);
					savedFutureDots.Add(futureDot2.nextDots[i]);
				}
			}
		}
		int sfNum = 0;
		while (sfNum < savedFutureDots.Count)
		{
			foreach (ZoneDot nextDot in savedFutureDots[sfNum].nextDots)
			{
				ZoneDot zoneDot = nextDot;
				savedFutureDots[sfNum].ColorFutureLines(dark);
			}
			sfNum++;
			yield return new WaitForSeconds(0.1f);
		}
	}

	public void AddNextDot(ZoneDot nextDot)
	{
		if ((type == ZoneType.Distress && nextDot.type == ZoneType.Distress) || (type == ZoneType.Treasure && nextDot.type == ZoneType.Treasure))
		{
			nextDot.SetType(ZoneType.Battle);
		}
		nextDots.Add(nextDot);
		nextDot.previousDots.Add(this);
	}

	public override void OnAcceptPress()
	{
		if (previousDots.Contains(worldBar.runCtrl.currentZoneDot) && worldBar.interactable)
		{
			btnCtrl.SetFocus(this);
			S.I.PlayOnce(btnCtrl.chooseAltSound);
			btnCtrl.RemoveFocus();
			idCtrl.StartCoroutine(idCtrl._ClickZoneButton(this));
			idCtrl.runCtrl.worldBar.savedZoneSelectionNum = 0;
		}
	}

	public override void Focus(int playerNum = 0)
	{
		base.Focus(playerNum);
		hovering = true;
		StartCoroutine(HighlightFuture());
		idCtrl.runCtrl.currentZoneDot.KeepColorFor(this, true);
		idCtrl.runCtrl.worldBar.selectionMarker.target = base.transform;
	}

	public override void UnFocus()
	{
		base.UnFocus();
		hovering = false;
		fgImage.color = Color.white;
		ClearFuture();
		idCtrl.runCtrl.worldBar.savedZoneSelectionNum = idCtrl.runCtrl.currentZoneDot.nextDots.IndexOf(this);
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			OnAcceptPress();
		}
	}

	public override void Up()
	{
		S.I.PlayOnce(btnCtrl.hoverSound);
		ZoneDot zoneDot = this;
		zoneDot = ((idCtrl.runCtrl.currentZoneDot.nextDots.IndexOf(this) <= 0) ? idCtrl.runCtrl.currentZoneDot.nextDots[idCtrl.runCtrl.currentZoneDot.nextDots.Count - 1] : idCtrl.runCtrl.currentZoneDot.nextDots[idCtrl.runCtrl.currentZoneDot.nextDots.IndexOf(this) - 1]);
		if (zoneDot != this)
		{
			btnCtrl.SetFocus(zoneDot);
		}
		idCtrl.runCtrl.worldBar.selectionMarker.anim.SetBool("pulse", true);
	}

	public override void Down()
	{
		S.I.PlayOnce(btnCtrl.hoverSound);
		ZoneDot zoneDot = this;
		zoneDot = ((idCtrl.runCtrl.currentZoneDot.nextDots.IndexOf(this) >= idCtrl.runCtrl.currentZoneDot.nextDots.Count - 1) ? idCtrl.runCtrl.currentZoneDot.nextDots[0] : idCtrl.runCtrl.currentZoneDot.nextDots[idCtrl.runCtrl.currentZoneDot.nextDots.IndexOf(this) + 1]);
		if (zoneDot != this)
		{
			btnCtrl.SetFocus(zoneDot);
		}
		idCtrl.runCtrl.worldBar.selectionMarker.anim.SetBool("pulse", true);
	}
}
