using UnityEngine;

public class Tiling : MonoBehaviour
{
	public int offsetX = 2;

	public bool hasARightBuddy = false;

	public bool hasALeftBuddy = false;

	public bool reverseScale = false;

	private Camera cam;

	private RectTransform rect;

	private void Awake()
	{
		cam = Camera.main;
		rect = GetComponent<RectTransform>();
	}

	private void Start()
	{
	}

	private void Update()
	{
		float num = cam.orthographicSize * ScalableCamera.PIXELS_TO_UNITS_HEIGHT * (float)Screen.width / (float)Screen.height;
		if (!hasALeftBuddy || !hasARightBuddy)
		{
			float num2 = base.transform.position.x + rect.sizeDelta.x / 2f - num;
			float num3 = base.transform.position.x - rect.sizeDelta.x / 2f + num;
			if (!hasALeftBuddy && cam.transform.position.x <= num3 + (float)offsetX)
			{
				MakeNewBuddy(-1);
				hasALeftBuddy = true;
			}
			if (!hasARightBuddy && cam.transform.position.x >= num2 - (float)offsetX)
			{
				MakeNewBuddy(1);
				hasARightBuddy = true;
			}
		}
		if (cam.transform.position.x >= base.transform.position.x + num + (float)offsetX)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void MakeNewBuddy(int rightOrLeft)
	{
		Vector3 vector = new Vector3(base.transform.localPosition.x + (rect.sizeDelta.x - 2f) * (float)rightOrLeft, 0f, 0f);
		Transform transform = Object.Instantiate(base.gameObject, vector, base.transform.rotation).transform;
		transform.transform.localPosition = vector;
		transform.name = rightOrLeft.ToString();
		if (reverseScale)
		{
			transform.localScale = new Vector3(transform.localScale.x * -1f, 1f, 1f);
		}
		transform.SetParent(base.transform.parent, false);
		if (rightOrLeft > 0)
		{
			transform.GetComponent<Tiling>().hasALeftBuddy = true;
		}
		else
		{
			transform.GetComponent<Tiling>().hasARightBuddy = true;
		}
	}
}
