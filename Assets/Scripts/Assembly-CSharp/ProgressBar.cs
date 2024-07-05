using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
	public RunCtrl runCtrl;

	public Image worldDotPrefab;

	public List<Image> worldDots;

	public Follower marker;

	public Animator anim;

	public RectTransform worldContainer;

	public Canvas canvas;

	public Color executedBossColor;

	private int startingCanvasOrder = 0;

	private float baseMarkerDamping = 0f;

	private void Start()
	{
		baseMarkerDamping = marker.damping;
		startingCanvasOrder = canvas.sortingOrder;
	}

	private void Update()
	{
	}

	public void ResetCanvasOrder()
	{
		canvas.sortingOrder = startingCanvasOrder;
	}

	public void Set()
	{
		worldContainer.DestroyChildren();
		worldDots.Clear();
		for (int i = 0; i < runCtrl.currentRun.visitedWorldNames.Count; i++)
		{
			worldDots.Add(Object.Instantiate(worldDotPrefab, worldContainer));
			worldDots[worldDots.Count - 1].transform.GetChild(0).gameObject.SetActive(false);
			worldDots[worldDots.Count - 1].sprite = runCtrl.worldBar.zoneSprites[runCtrl.worlds[runCtrl.currentRun.visitedWorldNames[i]].iconName];
			SetWorldDotColor(runCtrl.currentRun.visitedWorldNames[i], false);
		}
		for (int j = 0; j < runCtrl.currentRun.unvisitedWorldNames.Count + 1; j++)
		{
			worldDots.Add(Object.Instantiate(worldDotPrefab, worldContainer));
			worldDots[worldDots.Count - 1].color = new Color(1f, 1f, 1f, 0.5f);
		}
	}

	public void SetBossFate()
	{
		if (runCtrl.currentRun.visitedWorldNames.Count > 0)
		{
			SetWorldDotColor(runCtrl.currentRun.visitedWorldNames[runCtrl.currentRun.visitedWorldNames.Count - 1], false);
		}
	}

	private void SetWorldDotColor(string worldDotName, bool passed)
	{
		if (runCtrl.currentRun.assists.ContainsKey("Boss" + worldDotName) || runCtrl.currentRun.worldName == worldDotName)
		{
			worldDots[runCtrl.currentRun.visitedWorldNames.IndexOf(worldDotName)].transform.GetChild(0).gameObject.SetActive(false);
			if (passed)
			{
				worldDots[runCtrl.currentRun.visitedWorldNames.IndexOf(worldDotName)].color = new Color(1f, 1f, 1f, 0.8f);
			}
			else
			{
				worldDots[runCtrl.currentRun.visitedWorldNames.IndexOf(worldDotName)].color = Color.white;
			}
		}
		else
		{
			worldDots[runCtrl.currentRun.visitedWorldNames.IndexOf(worldDotName)].transform.GetChild(0).gameObject.SetActive(true);
			worldDots[runCtrl.currentRun.visitedWorldNames.IndexOf(worldDotName)].color = executedBossColor;
		}
	}

	public void SetLocation(int index, string name, bool newRun)
	{
		if (newRun)
		{
			marker.damping = 0f;
		}
		else
		{
			marker.damping = baseMarkerDamping;
		}
		index -= runCtrl.currentRun.loopNum * (worldDots.Count - 1);
		if (index < 0)
		{
			index = 0;
		}
		if (worldDots.Count > index)
		{
			for (int i = 0; i < index; i++)
			{
				SetWorldDotColor(runCtrl.currentRun.visitedWorldNames[i], true);
			}
			marker.target = worldDots[index].transform;
			worldDots[index].sprite = runCtrl.worldBar.zoneSprites[name];
			worldDots[index].color = Color.white;
		}
		else
		{
			Debug.LogWarning("Index is larger than worldDotsCount");
		}
	}
}
