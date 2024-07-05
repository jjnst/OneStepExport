using System.Collections;
using UnityEngine;

public class SlideBody : MonoBehaviour
{
	public bool onScreen = false;

	public float damping = 0.1f;

	public bool vertical = false;

	public bool inverse = false;

	private Vector3 targetPos;

	private Vector3 offScreenPos;

	public float offsetZ;

	private Vector3 currentVelocity;

	private Vector3 restingPos;

	private CameraScript mainCam;

	private Coroutine co_Move;

	public bool moving = false;

	private void Awake()
	{
		if (vertical)
		{
			restingPos = Vector3.up * 1700f;
		}
		else
		{
			restingPos = Vector3.right * 1000f;
		}
	}

	private void Start()
	{
		mainCam = S.I.mainCam.GetComponent<CameraScript>();
		targetPos = mainCam.transform.position + restingPos;
	}

	private void Update()
	{
		if (!onScreen && !moving && base.transform.position.x != 1700f)
		{
			base.transform.position = Vector3.right * 1700f;
		}
	}

	public void Toggle()
	{
		if (onScreen)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	public void Show()
	{
		if (!onScreen)
		{
			if (!mainCam)
			{
				Start();
			}
			base.transform.position = mainCam.transform.position + restingPos - Vector3.forward * (mainCam.transform.position.z + offsetZ);
			if (inverse)
			{
				base.transform.position = new Vector3(base.transform.position.x * -1f, base.transform.position.y * -1f, base.transform.position.z);
			}
			targetPos = mainCam.transform.position - Vector3.forward * (mainCam.transform.position.z + offsetZ);
			onScreen = true;
			StartMove();
		}
	}

	public void Hide()
	{
		if (!mainCam)
		{
			Start();
		}
		if (onScreen)
		{
			targetPos = mainCam.transform.position + restingPos - Vector3.forward * mainCam.transform.position.z;
			onScreen = false;
			StartMove();
		}
	}

	private void StartMove()
	{
		if (co_Move != null)
		{
			StopCoroutine(co_Move);
			co_Move = null;
		}
		co_Move = StartCoroutine(Move());
	}

	private IEnumerator Move()
	{
		moving = true;
		Vector3 finalTargetPos = targetPos;
		Vector3 one = Vector3.one;
		while (Vector3.Distance(base.transform.position, finalTargetPos) > 1f)
		{
			if (onScreen)
			{
				targetPos = mainCam.transform.position - Vector3.forward * (mainCam.transform.position.z + offsetZ);
			}
			else
			{
				targetPos = mainCam.transform.position + restingPos - Vector3.forward * mainCam.transform.position.z;
			}
			finalTargetPos = targetPos;
			if (inverse)
			{
				finalTargetPos = new Vector3(targetPos.x * -1f, targetPos.y * -1f, targetPos.z);
			}
			Vector3 newPos = Vector3.SmoothDamp(new Vector3(base.transform.position.x, base.transform.position.y), finalTargetPos - base.transform.TransformDirection(Vector3.forward), ref currentVelocity, damping, float.PositiveInfinity, Time.unscaledDeltaTime);
			base.transform.position = newPos;
			if (Mathf.Abs(base.transform.position.x - finalTargetPos.x) <= 0.99f && Mathf.Abs(base.transform.position.y - finalTargetPos.y) <= 0.99f)
			{
				base.transform.position = finalTargetPos;
			}
			yield return null;
		}
		moving = false;
		co_Move = null;
	}
}
