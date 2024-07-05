using UnityEngine;

public class ScalableCamera : MonoBehaviour
{
	public static float PIXELS_TO_UNITS_HEIGHT = 3f;

	public static float PIXELS_TO_UNITS_WIDTH = 3f;

	public static float PIXELS_TO_UNITS_ROUNDED = 3f;

	public float baseHeight = 240f;

	public float baseWidth = 0f;

	private int oldHeight = 0;

	public static float calculatedPixelWidth = 0f;

	public static float calculatedPixelHeight = 0f;

	public static float hCoef = 0f;

	public static float vCoef = 0f;

	public static Vector2 screenSize = Vector2.one;

	public static float ratio = 0f;

	private void Start()
	{
		baseWidth = baseHeight * 16f / 9f;
		ratio = baseWidth / baseHeight;
	}

	private void Update()
	{
	}

	public void CalculatePixelRes(Resolution resolution)
	{
		oldHeight = resolution.height;
		PIXELS_TO_UNITS_HEIGHT = (float)resolution.height / baseHeight;
		PIXELS_TO_UNITS_WIDTH = (float)resolution.width / baseWidth;
		PIXELS_TO_UNITS_ROUNDED = Mathf.FloorToInt((float)resolution.height / baseHeight);
		screenSize = new Vector2(resolution.width, resolution.height) / PIXELS_TO_UNITS_ROUNDED;
		Camera.main.orthographicSize = (float)resolution.height / 2f / PIXELS_TO_UNITS_ROUNDED;
		calculatedPixelWidth = baseWidth * PIXELS_TO_UNITS_ROUNDED;
		calculatedPixelHeight = baseHeight * PIXELS_TO_UNITS_ROUNDED;
		hCoef = PIXELS_TO_UNITS_WIDTH / PIXELS_TO_UNITS_ROUNDED / PIXELS_TO_UNITS_ROUNDED * calculatedPixelWidth;
		vCoef = PIXELS_TO_UNITS_HEIGHT / PIXELS_TO_UNITS_ROUNDED / PIXELS_TO_UNITS_ROUNDED * calculatedPixelHeight;
	}
}
