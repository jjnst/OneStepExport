using UnityEngine;

public class CardTrail : SinFollower
{
	private float startingDistance;

	public RectTransform rect;

	public ItemObject itemObj;

	public float displayAltLerpTime = 0.3f;

	private float savedLerpTime;

	public PostCtrl poCtrl;

	public DeckCtrl deCtrl;

	public bool expanding = false;

	private bool isListCard = false;

	protected override void Awake()
	{
		deCtrl = S.I.deCtrl;
		poCtrl = S.I.poCtrl;
		rect = GetComponent<RectTransform>();
	}

	protected override void LateUpdate()
	{
		if ((bool)target)
		{
			if (trailing)
			{
				distance = Vector2.Distance(base.transform.position, target.position);
				rect.localScale = Vector3.one * Mathf.Clamp(distance / startingDistance, 0.15f, 1f);
			}
			else if (expanding)
			{
				distance = Vector2.Distance(base.transform.position, target.position);
				rect.localScale = Vector3.one * Mathf.Clamp(1f - distance / startingDistance, 0.15f, 1f);
				if (isListCard && deCtrl.displayCard.canvasGroup.alpha == 0f)
				{
					Reset();
				}
			}
			base.LateUpdate();
		}
		else if (trailing)
		{
			trailing = false;
			DisableSelf();
		}
	}

	public override void Reset()
	{
		base.Reset();
		rect.localScale = Vector3.one;
		expanding = false;
	}

	public void TrailTo(Transform newTarget)
	{
		base.transform.SetParent(deCtrl.cardTrailContainer, true);
		Reset();
		target = newTarget;
		trailing = true;
		startingDistance = Vector3.Distance(base.transform.position, newTarget.position);
	}

	public void ExpandTo(Transform newTarget, bool listCard)
	{
		isListCard = listCard;
		Reset();
		target = newTarget;
		expanding = true;
		savedLerpTime = lerpTime;
		lerpTime = displayAltLerpTime;
		startingDistance = Vector3.Distance(base.transform.position, newTarget.position);
	}

	public override void DisableSelf()
	{
		if (expanding)
		{
			lerpTime = savedLerpTime;
			expanding = false;
		}
		else
		{
			if (itemObj == null)
			{
				return;
			}
			if (itemObj.type == ItemType.Art || itemObj.type == ItemType.Pact)
			{
				if ((bool)target)
				{
					target.GetComponent<Animator>().SetBool("spawned", true);
				}
			}
			else if (itemObj.type == ItemType.Spell && (bool)deCtrl.ctrl.currentPlayer)
			{
				deCtrl.ctrl.currentPlayer.duelDisk.CreateDeckSpells();
			}
			base.DisableSelf();
		}
	}
}
