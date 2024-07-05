using UnityEngine;

public class LineTracer : MonoBehaviour
{
	public float lerpTime = 0.5f;

	public float trackingDuration = 2f;

	public Gradient laserGradient;

	public LineRenderer lineRend;

	public Transform source;

	public Transform target;

	private void OnDisable()
	{
		lineRend.SetPositions(new Vector3[0]);
		lineRend.positionCount = 0;
	}

	public void Set(Transform source, Transform target, string color)
	{
		this.source = source;
		this.target = target;
		Color color2 = Color.white;
		ColorUtility.TryParseHtmlString(color, out color2);
		Gradient gradient = new Gradient();
		GradientColorKey[] array = new GradientColorKey[2];
		array[0].color = color2;
		array[0].time = 0f;
		array[1].color = color2;
		array[1].time = 1f;
		GradientAlphaKey[] array2 = new GradientAlphaKey[2];
		array2[0].alpha = 1f;
		array2[0].time = 0f;
		array2[1].alpha = 0f;
		array2[1].time = 1f;
		gradient.SetKeys(array, array2);
		lineRend.positionCount = 2;
		lineRend.colorGradient = gradient;
		lineRend.sortingLayerName = "ProjChar";
		lineRend.startWidth = 1.6f;
		lineRend.SetPosition(0, source.position);
		lineRend.SetPosition(1, target.position);
	}

	private void LateUpdate()
	{
		if (target.gameObject.activeInHierarchy)
		{
			lineRend.positionCount = 2;
			lineRend.SetPosition(0, source.position);
			lineRend.SetPosition(1, target.position);
		}
		else
		{
			SimplePool.Despawn(base.gameObject);
		}
	}
}
