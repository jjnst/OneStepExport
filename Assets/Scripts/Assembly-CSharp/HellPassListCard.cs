using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HellPassListCard : ListCard
{
	public Sprite checkmark;

	private bool activated = false;

	public HellPassButton hellPassButton;

	public int hellPassNum = 0;

	[SerializeField]
	private TMP_Text numberText = null;

	[SerializeField]
	private Color hoverColor = Color.white;

	private Color baseColor = Color.clear;

	public VictoryTriangle victoryTriangle = null;

	public Animator iconAnim = null;

	public CanvasGroup iconCanvasGroup = null;

	[SerializeField]
	private AudioClip fillClip = null;

	[SerializeField]
	private AudioClip emptyClip = null;

	private void Start()
	{
		baseColor = image.color;
	}

	public override void Up()
	{
		base.Up();
		if ((bool)btnCtrl.focusedButton.GetComponent<HellPassListCard>() && btnCtrl.focusedButton.GetComponent<HellPassListCard>().hellPassNum > hellPassButton.heCtrl.runCtrl.unlockedHellPassNum + 1)
		{
			S.I.PlayOnce(btnCtrl.lockedSound);
			btnCtrl.SetFocus(this);
		}
	}

	public override void Down()
	{
		base.Down();
		if ((bool)btnCtrl.focusedButton.GetComponent<HellPassListCard>() && btnCtrl.focusedButton.GetComponent<HellPassListCard>().hellPassNum > hellPassButton.heCtrl.runCtrl.unlockedHellPassNum + 1)
		{
			S.I.PlayOnce(btnCtrl.lockedSound);
			btnCtrl.SetFocus(this);
		}
	}

	public override void Left()
	{
	}

	public override void Right()
	{
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (itemObj != null && IsHoverable())
		{
			hovering = true;
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		hovering = false;
	}

	public override void OnPointerClick(PointerEventData data)
	{
		OnAcceptPress();
	}

	protected override void Update()
	{
		base.Update();
	}

	public override void OnAcceptPress()
	{
		if (hellPassNum > hellPassButton.heCtrl.runCtrl.unlockedHellPassNum + 1)
		{
			return;
		}
		activated = !activated;
		SetActivated(activated);
		if (hellPassNum != 0)
		{
			return;
		}
		for (int i = 1; i <= hellPassButton.heCtrl.runCtrl.unlockedHellPassNum + 1; i++)
		{
			if (i < hellPassButton.hellPassListCards.Count)
			{
				hellPassButton.hellPassListCards[i].SetActivated(activated, false);
			}
		}
	}

	public void SetActivated(bool setToActivated, bool playAudio = true)
	{
		activated = setToActivated;
		anim.SetBool("activated", activated);
		if (setToActivated)
		{
			if (playAudio)
			{
				S.I.PlayOnce(fillClip);
			}
			if (!hellPassButton.activatedHellPasses.Contains(hellPassNum))
			{
				hellPassButton.activatedHellPasses.Add(hellPassNum);
			}
		}
		else
		{
			if (playAudio)
			{
				S.I.PlayOnce(emptyClip);
			}
			if (hellPassButton.activatedHellPasses.Contains(hellPassNum))
			{
				hellPassButton.activatedHellPasses.Remove(hellPassNum);
			}
		}
		SaveDataCtrl.Set("HellPassActivated" + hellPassNum, activated);
		hellPassButton.SetActiveHellPasses();
		if (hellPassNum == 0)
		{
			return;
		}
		if (hellPassButton.activatedHellPasses.Count == hellPassButton.heCtrl.runCtrl.unlockedHellPassNum + 1)
		{
			if (!hellPassButton.hellPassListCards[0].activated)
			{
				hellPassButton.hellPassListCards[0].SetActivated(true, false);
			}
		}
		else if (hellPassButton.activatedHellPasses.Count == 1 && hellPassButton.hellPassListCards[0].activated)
		{
			hellPassButton.hellPassListCards[0].SetActivated(false, false);
		}
	}

	public ListCard SetPact(PactObject pactObj, Transform parentTransform, int siblingIndex = -1, bool setSpawned = false)
	{
		SetItem(pactObj, parentTransform, siblingIndex);
		base.name = " ListCard " + siblingIndex + " - " + pactObj.nameString;
		bgImage.sprite = pactObj.sprite;
		base.pactObj = pactObj;
		iconAnim.SetBool("spawned", setSpawned);
		parentList = deCtrl.pactCardList;
		if (hellPassNum == 0)
		{
			tmpText.text = ScriptLocalization.UI.HellPass_ToggleAll;
		}
		else if (hellPassNum <= hellPassButton.heCtrl.runCtrl.unlockedHellPassNum + 1)
		{
			tmpText.text = LocalizationManager.GetTranslation("PactDescriptions/" + pactObj.itemID);
		}
		else
		{
			tmpText.text = ScriptLocalization.UI.Library_LockedName;
		}
		numberText.text = hellPassNum.ToString();
		if (pactObj.pact != null)
		{
			pactObj.pact.listCard = GetComponent<SquareListCard>();
		}
		return this;
	}

	public void UpdatePactText()
	{
		if (hellPassNum != 0)
		{
			if (hellPassNum <= hellPassButton.heCtrl.runCtrl.unlockedHellPassNum + 1)
			{
				tmpText.text = LocalizationManager.GetTranslation("PactDescriptions/" + pactObj.itemID);
			}
			else
			{
				tmpText.text = ScriptLocalization.UI.Library_LockedName;
			}
		}
	}

	public override void Focus(int playerNum = 0)
	{
		hellPassButton.scrollbar.value = 1f - (float)hellPassNum * 1f / (float)(hellPassButton.hellPassListCards.Count - 1) * 1f;
		image.color = hoverColor;
	}

	public override void UnFocus()
	{
		image.color = baseColor;
	}

	private void OnDisable()
	{
		iconCanvasGroup.alpha = 1f;
	}
}
