using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldBar : NavPanel, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public ZoneDot zoneDotPrefab;

	public GameObject arrow;

	public Follower locationMarker;

	public Follower selectionMarker;

	public Image selectionMarkerImage;

	public Dictionary<string, Sprite> zoneSprites = new Dictionary<string, Sprite>();

	public List<ZoneDot> currentZoneDots = new List<ZoneDot>();

	public List<List<ZoneDot>> currentZoneSteps = new List<List<ZoneDot>>();

	public Transform zoneDotContainer;

	public RectTransform mapKeyPanel;

	public RectTransform detailPanel;

	public AudioClip openSound;

	public AudioClip closeSound;

	[ReadOnly]
	public float battleChance;

	[ReadOnly]
	public float shopChance;

	[ReadOnly]
	public float distressChance;

	[ReadOnly]
	public float dangerChance;

	public float campsiteChance;

	public float randomChance;

	public TMP_Text zoneText;

	public GameObject confirmButton;

	public GameObject closeButton;

	public Image bg;

	public RectTransform rect;

	public float damping = 0.05f;

	protected Vector3 currentVelocity;

	public float hoverSize = 1.1f;

	public TMP_Text seedText;

	public int savedZoneSelectionNum = 0;

	public float width = 140f;

	public float defaultVerticalSpacing = 14f;

	private Coroutine _SendSelectionMarker;

	private bool genocideRun = false;

	public bool available = false;

	public bool interactable = false;

	public BC ctrl;

	public IdleCtrl idCtrl;

	public RunCtrl runCtrl;

	protected override void Awake()
	{
		base.Awake();
		ctrl = S.I.batCtrl;
		idCtrl = S.I.idCtrl;
		runCtrl = S.I.runCtrl;
		rect = GetComponent<RectTransform>();
	}

	public override void Open()
	{
		if (!available || btnCtrl.IsActivePanel(Nav.Deck) || btnCtrl.IsActivePanel(Nav.Shop) || PostCtrl.transitioning || runCtrl.currentZoneDot.nextDots.Count <= 0)
		{
			return;
		}
		savedZoneSelectionNum = Mathf.Clamp(savedZoneSelectionNum, 0, runCtrl.currentZoneDot.nextDots.Count - 1);
		defaultButton = null;
		foreach (ZoneDot currentZoneDot in currentZoneDots)
		{
			currentZoneDot.hoverable = false;
		}
		foreach (ZoneDot nextDot in runCtrl.currentZoneDot.nextDots)
		{
			nextDot.hoverable = true;
		}
		base.Open();
		S.I.PlayOnce(openSound);
		ctrl.AddControlBlocks(Block.ZoneSelection);
		open = true;
		if (interactable)
		{
			selectionMarkerImage.color = U.I.GetColor(UIColor.Pink);
			confirmButton.SetActive(true);
			closeButton.SetActive(false);
		}
		else
		{
			selectionMarkerImage.color = Color.grey;
			closeButton.SetActive(true);
			confirmButton.SetActive(false);
		}
		idCtrl.onwardBtn.inverse = false;
		idCtrl.HideOnwardButton();
		runCtrl.worldBar.available = true;
		runCtrl.progressBar.anim.SetBool("visible", true);
		runCtrl.progressBar.ResetCanvasOrder();
		StartCoroutine(SetDefaultZoneAfter());
	}

	private IEnumerator SetDefaultZoneAfter()
	{
		yield return null;
		btnCtrl.SetFocus(runCtrl.currentZoneDot.nextDots[savedZoneSelectionNum]);
		if (_SendSelectionMarker == null)
		{
			_SendSelectionMarker = StartCoroutine(SendSelectionMarker());
		}
	}

	public override void Close()
	{
		UIButton item = originButton;
		base.Close();
		S.I.PlayOnce(closeSound);
		ctrl.RemoveControlBlocksNextFrame(Block.ZoneSelection);
		foreach (ZoneDot nextDot in runCtrl.currentZoneDot.nextDots)
		{
			nextDot.hoverable = false;
		}
		open = false;
		idCtrl.ShowOnwardButton();
		runCtrl.progressBar.anim.SetBool("visible", false);
		if (_SendSelectionMarker != null)
		{
			StopCoroutine(_SendSelectionMarker);
			_SendSelectionMarker = null;
			selectionMarker.target = runCtrl.currentZoneDot.transform;
		}
		if (ctrl.poCtrl.rewardCardList.Count > 0 && !ctrl.poCtrl.rewardCardList.Contains(item))
		{
			btnCtrl.SetFocus(ctrl.poCtrl.rewardCardList[0]);
		}
	}

	private void Update()
	{
		if (open)
		{
			zoneDotContainer.localPosition = new Vector3(width / 2f - width * ((float)runCtrl.currentRun.zoneNum * 1f / (float)(runCtrl.currentWorld.numZones - 1)), 0f, zoneDotContainer.position.z);
			if (locationMarker.transform.childCount > 0)
			{
				locationMarker.transform.GetChild(0).right = selectionMarker.transform.position - locationMarker.transform.position;
			}
			else
			{
				Debug.LogError("LocationMarker has no children!");
			}
		}
	}

	private IEnumerator SendSelectionMarker()
	{
		Transform targetHold = selectionMarker.target;
		selectionMarker.target = runCtrl.currentZoneDot.transform;
		yield return new WaitForSeconds(0.3f);
		selectionMarker.target = targetHold;
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
	}

	public virtual void OnPointerClick(PointerEventData data)
	{
	}

	public void GenerateWorldBar(int numSteps)
	{
		foreach (Transform item in zoneDotContainer)
		{
			Object.Destroy(item.gameObject);
		}
		currentZoneDots.Clear();
		currentZoneSteps.Clear();
		if (btnCtrl.hideUICounter < 1)
		{
			detailPanel.gameObject.SetActive(true);
		}
		List<ZoneDot> list = new List<ZoneDot>();
		List<string> list2 = new List<string>(runCtrl.currentRun.unvisitedWorldNames);
		int num = 100;
		ResetZoneVars();
		runCtrl.currentRun.lastWorldGenOrigin = runCtrl.currentRun.currentWorldGen;
		bool flag = false;
		int num2 = numSteps;
		num2++;
		if (runCtrl.currentWorld.nameString == "Pacifist")
		{
			num2 = 2;
		}
		for (int i = 0; i < num2; i++)
		{
			RectTransform rectTransform = new GameObject("ZoneStep").AddComponent<RectTransform>();
			List<ZoneDot> list3 = new List<ZoneDot>();
			currentZoneSteps.Add(list3);
			Vector3 vector = zoneDotContainer.transform.position - new Vector3((width / 2f - width / (float)(Mathf.Clamp(numSteps, 2, numSteps) - 1) * (float)i) * zoneDotContainer.lossyScale.x, 0f, 0f);
			rectTransform.localScale = zoneDotContainer.lossyScale;
			rectTransform.SetParent(zoneDotContainer, true);
			rectTransform.transform.position = vector;
			rectTransform.sizeDelta = new Vector2(10f, num);
			List<ZoneDot> list4 = new List<ZoneDot>();
			int num3 = 1;
			if (i > 0 && i < numSteps - 1)
			{
				num3 = runCtrl.NextWorldRand(2, 4);
			}
			bool flag2 = false;
			if (i == 1 || i == 4)
			{
				num3 = 3;
			}
			if (runCtrl.currentWorld.nameString == "Pacifist")
			{
				distressChance -= 50f;
				num3 = 1;
			}
			List<int> list5 = Utils.RandomList(3, true);
			for (int j = 1; j <= num3; j++)
			{
				ZoneDot zoneDot2 = Object.Instantiate(zoneDotPrefab, base.transform.position, base.transform.rotation, rectTransform.transform);
				zoneDot2.stepNum = i;
				list3.Add(zoneDot2);
				zoneDot2.worldBar = this;
				zoneDot2.idCtrl = idCtrl;
				zoneDot2.btnCtrl = btnCtrl;
				zoneDot2.transform.name = "ZoneDot - Step: " + i + " - " + j;
				zoneDot2.verticalSpacing = defaultVerticalSpacing;
				if (i == num2 - 1)
				{
					zoneDot2.verticalSpacing += 7f;
				}
				ZoneType zoneType = ZoneType.Battle;
				if (i == 0)
				{
					zoneType = ZoneType.Battle;
				}
				else if (i == numSteps - 1)
				{
					num3 = 1;
					zoneType = ZoneType.Boss;
				}
				else if (i == 2)
				{
					num3 = 2;
					if (j == 2)
					{
						if (distressChance > (float)runCtrl.NextWorldRand(0, 100))
						{
							zoneType = ZoneType.Distress;
							distressChance -= distressChance / 4f;
						}
						else
						{
							zoneType = ZoneType.Treasure;
						}
					}
					else if (campsiteChance > (float)runCtrl.NextWorldRand(0, 100))
					{
						zoneType = ZoneType.Campsite;
						campsiteChance -= campsiteChance;
					}
				}
				else
				{
					int num4 = runCtrl.NextWorldRand(0, 100);
					if (i == 1 && j == num3 && runCtrl.currentRun.worldTierNum > 0 && (bool)ctrl.currentPlayer && ctrl.currentPlayer.health.current == ctrl.currentPlayer.health.max)
					{
						zoneDot2.SetDark();
					}
					if (num4 < 20)
					{
						if (distressChance > (float)runCtrl.NextWorldRand(0, 100))
						{
							zoneType = ZoneType.Distress;
							distressChance -= distressChance / 4f;
						}
					}
					else if (num4 < 40)
					{
						if (dangerChance > (float)runCtrl.NextWorldRand(0, 100) && S.I.EDITION != Edition.DemoLive)
						{
							zoneType = ZoneType.Danger;
							dangerChance -= dangerChance / 4f;
						}
					}
					else if (num4 < 60)
					{
						if (randomChance > (float)runCtrl.NextWorldRand(0, 100))
						{
							zoneType = ZoneType.Random;
							randomChance -= randomChance / 4f;
						}
					}
					else
					{
						zoneType = ZoneType.Battle;
					}
					if (i == 3)
					{
						int num5 = runCtrl.NextWorldRand(1, num3);
						if (num5 == j && !flag2 && S.I.scene != GScene.DemoLive)
						{
							zoneType = ZoneType.Miniboss;
							flag2 = true;
						}
					}
					if (i == 4)
					{
						if (list5[j - 1] == 1)
						{
							zoneType = ZoneType.Campsite;
						}
						else if (list5[j - 1] == 2)
						{
							zoneType = ((1 == runCtrl.NextWorldRand(1, 2)) ? ZoneType.Treasure : ZoneType.Battle);
						}
					}
					if (i > 3 && shopChance > (float)runCtrl.NextWorldRand(0, 100) && !flag && zoneType == ZoneType.Battle && S.I.scene != GScene.DemoLive && !runCtrl.currentRun.shopkeeperKilled && !runCtrl.ctrl.currentHeroObj.tags.Contains(Tag.Shopkeeper))
					{
						zoneType = ZoneType.Shop;
						shopChance -= shopChance;
						flag = true;
					}
					if (S.I.EDITION == Edition.DemoLive && i == numSteps - 2)
					{
						num3 = 1;
						zoneType = ZoneType.Miniboss;
					}
				}
				if (S.I.scene == GScene.AllTestZones)
				{
					zoneType = S.I.testZoneType;
				}
				if (i == numSteps)
				{
					num3 = ((runCtrl.currentRun.unvisitedWorldNames.Count < 3) ? runCtrl.currentRun.unvisitedWorldNames.Count : 3);
					if (runCtrl.currentRun.unvisitedWorldNames.Count == 0)
					{
					}
					if (list2.Count > 0)
					{
						int index = runCtrl.NextWorldRand(0, list2.Count);
						zoneDot2.worldName = list2[index];
						zoneDot2.world = runCtrl.worlds[zoneDot2.worldName];
						zoneDot2.imageName = zoneDot2.world.iconName;
						list2.Remove(list2[index]);
					}
					else if (!genocideRun)
					{
						zoneDot2.worldName = "Pacifist";
						zoneDot2.imageName = "WorldWasteland";
					}
					else if (genocideRun)
					{
						zoneDot2.worldName = "Eden";
						zoneDot2.imageName = "WorldEden";
					}
					zoneDot2.world = runCtrl.worlds[zoneDot2.worldName];
					zoneType = ZoneType.World;
				}
				if (runCtrl.currentWorld.nameString == "Pacifist")
				{
					if (i == 0)
					{
						zoneType = ZoneType.Battle;
					}
					else if (i == num2 - 1)
					{
						zoneDot2.worldName = "Pacifist";
						zoneDot2.imageName = "WorldEden";
						zoneType = ZoneType.World;
					}
				}
				zoneDot2.transform.position = vector + new Vector3(0f, ((float)(num3 - 1) / 2f - (float)(j - 1)) * zoneDot2.verticalSpacing * rect.localScale.y, 0f);
				zoneDot2.SetType(zoneType);
				list4.Add(zoneDot2);
				currentZoneDots.Add(zoneDot2);
			}
			list.Clear();
			list = new List<ZoneDot>(list4);
		}
		for (int k = 0; k < currentZoneSteps.Count; k++)
		{
			List<ZoneDot> list6 = currentZoneSteps[k];
			if (currentZoneSteps.Count > k + 1)
			{
				List<ZoneDot> list7 = currentZoneSteps[k + 1];
				list6[0].AddNextDot(list7[0]);
				list6[list6.Count - 1].AddNextDot(list7[list7.Count - 1]);
				if (list7.Count > list6.Count)
				{
					List<ZoneDot> list8 = list7.Where((ZoneDot t) => t.previousDots.Count < 1).ToList();
					while (list8.Count > 0)
					{
						list6[runCtrl.NextWorldRand(0, list6.Count)].AddNextDot(list8[list8.Count - 1]);
						list8.RemoveAt(list8.Count - 1);
					}
				}
				if (list7.Count == list6.Count)
				{
					List<ZoneDot> list9 = list6.Where((ZoneDot t) => t.nextDots.Count < 1).ToList();
					List<ZoneDot> list10 = list7.Where((ZoneDot t) => t.previousDots.Count < 1).ToList();
					while (list9.Count > 0 && list10.Count > 0)
					{
						list9[list9.Count - 1].AddNextDot(list10[list10.Count - 1]);
						list9.RemoveAt(list9.Count - 1);
						list10.RemoveAt(list10.Count - 1);
					}
				}
				if (list7.Count < list6.Count)
				{
					List<ZoneDot> list11 = list6.Where((ZoneDot t) => t.nextDots.Count < 1).ToList();
					while (list11.Count > 0)
					{
						list11[list11.Count - 1].AddNextDot(list7[runCtrl.NextWorldRand(0, list7.Count)]);
						list11.RemoveAt(list11.Count - 1);
					}
				}
			}
			if (k == 3)
			{
				List<ZoneDot> list12 = currentZoneSteps[k + 1];
				if (!list6[0].nextDots.Contains(list12[1]))
				{
					list6[0].AddNextDot(list12[1]);
				}
				else if (!list6[list6.Count - 1].nextDots.Contains(list12[1]))
				{
					list6[list6.Count - 1].AddNextDot(list12[1]);
				}
			}
			if (runCtrl.NextWorldRand(0, 10) < 8 && list6.Count >= 3 && k < numSteps - 1 && list6.Where((ZoneDot t) => t.type == ZoneType.Battle).ToList().Count >= 3)
			{
				if (runCtrl.NextWorldRand(0, 2) == 0 || list6[1].previousDots[0].type == ZoneType.Distress)
				{
					list6[1].SetType(ZoneType.Danger);
				}
				else
				{
					list6[1].SetType(ZoneType.Distress);
				}
			}
			foreach (ZoneDot item2 in list6)
			{
				item2.nextDots = item2.nextDots.OrderBy((ZoneDot t) => t.transform.GetSiblingIndex()).ToList();
			}
		}
		foreach (ZoneDot zoneDot in currentZoneDots)
		{
			if (zoneDot.previousDots.Count > 1 && zoneDot.stepNum < numSteps - 1 && zoneDot.previousDots.Where((ZoneDot t) => t.type == zoneDot.previousDots[0].type).ToList().Count >= zoneDot.previousDots.Count)
			{
				for (int l = 0; l < zoneDot.previousDots.Count - 1; l++)
				{
					if (zoneDot.previousDots[l].previousDots != zoneDot.previousDots[l + 1].previousDots)
					{
						continue;
					}
					if (zoneDot.previousDots[l + 1].type == ZoneType.Battle)
					{
						if (runCtrl.NextWorldRand(0, 2) == 0)
						{
							zoneDot.previousDots[l + 1].SetType(ZoneType.Danger);
						}
						else
						{
							zoneDot.previousDots[l + 1].SetType(ZoneType.Distress);
						}
					}
					else
					{
						zoneDot.previousDots[l + 1].SetType(ZoneType.Battle);
					}
				}
			}
			zoneDot.CreateLines();
		}
		List<ZoneType> list13 = new List<ZoneType>
		{
			ZoneType.Battle,
			ZoneType.Danger,
			ZoneType.Distress
		};
		if (currentZoneSteps[1].Count > 2)
		{
			foreach (ZoneDot item3 in currentZoneSteps[1])
			{
				list13.Remove(item3.type);
			}
			if (list13.Count > 0)
			{
				currentZoneSteps[1][1].SetType(list13[runCtrl.NextWorldRand(0, list13.Count)]);
			}
		}
		selectionMarker.transform.position = currentZoneDots[0].transform.position;
	}

	private void ResetZoneVars()
	{
		battleChance = 100f;
		shopChance = 100f;
		distressChance = 100f;
		if (runCtrl.currentRun != null && runCtrl.currentRun.hellPasses.Contains(2))
		{
			dangerChance = 200f;
		}
		else
		{
			dangerChance = 100f;
		}
	}

	private void ChanceType(ZoneType type, float zoneChance)
	{
	}

	public void UpdateLocation(ZoneDot currentDot)
	{
		locationMarker.target = currentDot.transform;
	}
}
