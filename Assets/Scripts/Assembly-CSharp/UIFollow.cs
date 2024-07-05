using UnityEngine;

public class UIFollow : MonoBehaviour
{
	public Transform following;

	private Camera mainCam;

	public float xOffset = 0f;

	public float yOffset = 0f;

	public bool runOnce = false;

	public RectTransform rect;

	private void Awake()
	{
		mainCam = S.I.camCtrl.mainCam;
		rect = GetComponent<RectTransform>();
	}

	private void Start()
	{
		if ((bool)following)
		{
			rect.position = ObjScreenPos(following.position, xOffset, yOffset);
		}
	}

	private void Update()
	{
		if (!runOnce && (bool)following)
		{
			rect.position = ObjScreenPos(following.position, xOffset, yOffset);
		}
	}

	public Vector2 ObjScreenPos(Vector3 position, float xOffset, float yOffset)
	{
		return new Vector2(position.x + xOffset, position.y + yOffset);
	}

	public virtual void Set(float newXOffset, float newYOffset, Transform following, bool runOnce, Transform newParent)
	{
		xOffset = newXOffset;
		yOffset = newYOffset;
		this.following = following.transform;
		this.runOnce = false;
		base.transform.SetParent(newParent.transform, false);
		rect.position = ObjScreenPos(following.transform.position, xOffset, yOffset);
	}
}
