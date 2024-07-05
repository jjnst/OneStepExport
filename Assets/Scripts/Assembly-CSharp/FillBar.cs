using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
	public RectTransform lineGrid;

	public RectTransform barContainer;

	public Image fill;

	public Image cut;

	public TMP_Text displayNum;

	public GameObject manaLinePrefab;

	public RectTransform gem;

	public RectTransform rect;

	public bool showMaxNum = true;

	public int displayedMax = -1;

	public float cutIterationRate = 0.01f;

	private int currentIntValue = 0;

	private float currentMaxValue = 0f;

	public int maxLines = -1;

	private List<GameObject> createdManaLines = new List<GameObject>();

	private Coroutine co_updateLines;

	private void Awake()
	{
		lineGrid.DestroyChildren();
	}

	public void UpdateLines(int maxAmount)
	{
		if (maxLines > -1 && maxAmount > maxLines + 1)
		{
			maxAmount = maxLines + 1;
		}
		if (maxAmount == displayedMax)
		{
			return;
		}
		displayedMax = maxAmount;
		float x = barContainer.sizeDelta.x;
		if (maxAmount > 0)
		{
			for (int i = 0; i < maxAmount - 1; i++)
			{
				Vector3 vector = lineGrid.transform.position + Vector3.right * (x / 2f - (float)(i + 1) * x / (float)maxAmount);
				if (i >= createdManaLines.Count)
				{
					createdManaLines.Add(SimplePool.Spawn(manaLinePrefab, vector, lineGrid.transform.rotation, lineGrid));
					continue;
				}
				if (!createdManaLines[i].activeSelf)
				{
					createdManaLines[i].SetActive(true);
				}
				createdManaLines[i].transform.position = vector;
			}
		}
		for (int j = Mathf.Clamp(maxAmount - 1, 0, maxAmount - 1); j < createdManaLines.Count; j++)
		{
			createdManaLines[j].SetActive(false);
		}
	}

	public void UpdateBar(float current, float max)
	{
		if (max > 0f)
		{
			fill.fillAmount = current / max;
		}
		else
		{
			fill.fillAmount = 0f;
		}
		if (currentIntValue != Mathf.FloorToInt(current) || currentMaxValue != max)
		{
			currentIntValue = Mathf.FloorToInt(current);
			currentMaxValue = max;
			UpdateLines(Mathf.RoundToInt(currentMaxValue));
			if (showMaxNum)
			{
				displayNum.text = currentIntValue + "/" + max;
			}
			else
			{
				displayNum.text = currentIntValue.ToString();
			}
		}
	}

	private void Update()
	{
		if (cut.fillAmount != fill.fillAmount)
		{
			cut.fillAmount = CalculateCut(cut.fillAmount, fill.fillAmount);
			if (Mathf.Abs(cut.fillAmount - fill.fillAmount) < cutIterationRate * 4f)
			{
				cut.fillAmount = fill.fillAmount;
			}
		}
	}

	private float CalculateCut(float displayVal, float currentVal)
	{
		displayVal = ((!(displayVal > currentVal)) ? (displayVal + cutIterationRate) : (displayVal - cutIterationRate));
		return displayVal;
	}

	public void Flip()
	{
		if (fill.fillOrigin == 0)
		{
			fill.fillOrigin = 1;
			cut.fillOrigin = 1;
		}
		else
		{
			fill.fillOrigin = 0;
			cut.fillOrigin = 0;
		}
	}

	public void Resize(int newSize)
	{
		float x = barContainer.sizeDelta.x;
		barContainer.sizeDelta = new Vector2(newSize, barContainer.sizeDelta.y);
		if (base.transform.position.x < 0f)
		{
			barContainer.localPosition = new Vector2(barContainer.localPosition.x - (x - (float)newSize) / 2f, barContainer.localPosition.y);
		}
		else
		{
			barContainer.localPosition = new Vector2(barContainer.localPosition.x + (x - (float)newSize) / 2f, barContainer.localPosition.y);
		}
		UpdateLines(displayedMax);
	}
}
