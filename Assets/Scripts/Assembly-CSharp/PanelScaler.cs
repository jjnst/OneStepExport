using UnityEngine;

public class PanelScaler : MonoBehaviour
{
	public bool scaleWithX = false;

	public bool scaleWithY = false;

	public bool scaleWithLargerAxis = false;

	public float excessScale = 1f;

	public bool hud = false;

	public RectTransform rect;

	private void Awake()
	{
		if (rect == null)
		{
			rect = GetComponent<RectTransform>();
		}
	}

	private void Update()
	{
		if (scaleWithLargerAxis)
		{
			if (ScalableCamera.screenSize.x / ScalableCamera.ratio > ScalableCamera.screenSize.y)
			{
				scaleWithX = true;
				scaleWithY = false;
			}
			else
			{
				scaleWithY = true;
				scaleWithX = false;
			}
		}
		if (scaleWithX)
		{
			rect.sizeDelta = new Vector2(ScalableCamera.screenSize.x, ScalableCamera.screenSize.x / ScalableCamera.ratio) * excessScale;
		}
		else if (scaleWithY)
		{
			rect.sizeDelta = new Vector2(ScalableCamera.screenSize.y * ScalableCamera.ratio, ScalableCamera.screenSize.y) * excessScale;
		}
		else
		{
			rect.sizeDelta = ScalableCamera.screenSize * excessScale;
		}
	}
}
