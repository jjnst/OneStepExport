using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGCtrl : MonoBehaviour
{
	public List<Transform> backgroundContainers;

	public List<float> parallaxScales;

	public Image greenScreen;

	public float smoothing = 1f;

	public Vector3 paraTarget;

	public Vector3 paraPoint;

	public Vector3 prevParaPoint;

	public float transitionDuration = 1f;

	private Vector3 currentVelocity;

	public bool moveBG = false;

	public bool changeBG = false;

	public Animator visualizer;

	private string lastBg;

	private ItemManager itemMan;

	public List<Sprite> backgrounds;

	private Dictionary<string, Sprite> backgroundsDict = new Dictionary<string, Sprite>();

	private void Awake()
	{
		itemMan = S.I.itemMan;
	}

	private void Start()
	{
		prevParaPoint = paraPoint;
		foreach (Sprite background in backgrounds)
		{
			backgroundsDict[background.name] = background;
		}
	}

	private void Update()
	{
		if (changeBG)
		{
			ChangeBG(lastBg);
			changeBG = false;
		}
		if (moveBG)
		{
			MoveBG();
			moveBG = false;
		}
		Vector3 target = paraTarget - base.transform.TransformDirection(Vector3.forward);
		if (!(Mathf.Abs(target.x - paraPoint.x) > 1f))
		{
			return;
		}
		paraPoint = Vector3.SmoothDamp(paraPoint, target, ref currentVelocity, transitionDuration);
		for (int i = 0; i < backgroundContainers.Count; i++)
		{
			foreach (Transform item in backgroundContainers[i])
			{
				float num = (prevParaPoint.x - paraPoint.x) * parallaxScales[i];
				float x = item.localPosition.x + num;
				Vector3 localPosition = new Vector3(x, item.localPosition.y, item.localPosition.z);
				item.localPosition = localPosition;
			}
		}
		prevParaPoint = paraPoint;
	}

	public void MoveBG(float screenScrolls = 1f)
	{
		paraPoint = Vector3.zero;
		prevParaPoint = Vector3.zero;
		paraTarget = Vector3.zero;
		paraTarget += Vector3.right * screenScrolls * 1f * ScalableCamera.calculatedPixelWidth;
	}

	public void ChangeBG(string bgName)
	{
		lastBg = bgName;
		for (int i = 0; i < 4; i++)
		{
			Sprite sprite = null;
			switch (i)
			{
			case 0:
				sprite = GetBackgroundSprite(bgName + "-bg_3");
				break;
			case 1:
				sprite = GetBackgroundSprite(bgName + "-bg_2");
				break;
			case 2:
				sprite = GetBackgroundSprite(bgName + "-bg_1");
				break;
			case 3:
				sprite = GetBackgroundSprite(bgName + "-bg_0");
				break;
			}
			if (backgroundContainers[i].childCount > 0)
			{
				Image component = backgroundContainers[i].GetChild(backgroundContainers[i].childCount - 1).GetComponent<Image>();
				component.sprite = sprite;
				component.rectTransform.sizeDelta = new Vector2(sprite.rect.width * component.rectTransform.sizeDelta.y / sprite.rect.height, component.rectTransform.sizeDelta.y);
			}
		}
		MoveBG();
	}

	private Sprite GetBackgroundSprite(string backgroundName)
	{
		if (backgroundsDict.ContainsKey(backgroundName))
		{
			return backgroundsDict[backgroundName];
		}
		return itemMan.GetSprite(backgroundName);
	}
}
