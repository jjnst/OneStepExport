using TMPro;
using UnityEngine;

public class BeingStatsPanel : MonoBehaviour
{
	public TMP_Text healthDisplay;

	public CanvasGroup healthDisplayCanvasGroup;

	public Transform statusEffectsBox;

	public UIFollow uiFollow;

	public GameObject statusFlashPrefab;

	public BC ctrl;

	private void Awake()
	{
		ctrl = S.I.batCtrl;
	}

	public void SetBeing(Being newBeing)
	{
		base.transform.SetParent(ctrl.battleUIContainer, false);
		base.transform.SetAsFirstSibling();
		uiFollow.following = newBeing.transform;
		newBeing.beingStatsPanel = this;
		base.transform.name = "BeingStatsPanel for " + newBeing.beingObj.beingID;
	}

	public void CreateImageFlash(string imageName)
	{
		SpriteRenderer component = SimplePool.Spawn(statusFlashPrefab, base.transform.position + new Vector3(0f, 10f, 0f), base.transform.rotation).GetComponent<SpriteRenderer>();
		component.sprite = ctrl.effectSpritesDict[imageName];
		component.transform.SetParent(base.transform, true);
		component.transform.name = "StatFlash for " + imageName;
		SimplePool.Despawn(component.gameObject, 1f);
	}
}
