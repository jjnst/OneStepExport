using UnityEngine;
using UnityEngine.UI;

public class FlexSelector : MonoBehaviour
{
	public Image image;

	public Transform topLeftPoint;

	public Transform botRightPoint;

	public Transform startTarget;

	public Transform endTarget;

	public Transform rightTarget;

	public Transform leftTarget;

	public float width = 50f;

	public RectTransform rect;

	public float offset;

	public bool smooth = false;

	public float damping = 0.1f;

	public bool fill = false;

	private bool fillProcessing = false;

	public FlexSelector fillNext;

	public FlexSelector fillParent;

	private Vector3 curVelocity;

	private void Awake()
	{
		rect = GetComponent<RectTransform>();
	}

	private void Update()
	{
		UpdateFlex();
	}

	public void UpdateFlex()
	{
		if ((bool)startTarget && (bool)endTarget)
		{
			if (smooth)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, startTarget.position, ref curVelocity, damping);
			}
			else
			{
				base.transform.position = startTarget.position;
			}
			base.transform.right = base.transform.position - endTarget.position;
			rect.sizeDelta = new Vector2(Vector3.Distance(base.transform.position, endTarget.position), 1f);
		}
		else if ((bool)rightTarget)
		{
			rect.pivot = new Vector2(1f, 0.5f);
			Vector3 vector = rightTarget.position - base.transform.TransformDirection(Vector3.forward) - offset * Vector3.right;
			if (smooth)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, vector, ref curVelocity, damping);
			}
			else
			{
				base.transform.position = vector;
			}
			rect.sizeDelta = new Vector2(width, 1f);
		}
		else if ((bool)leftTarget)
		{
			rect.pivot = new Vector2(0f, 0.5f);
			Vector3 vector2 = leftTarget.position - base.transform.TransformDirection(Vector3.forward) + offset * Vector3.right;
			if (smooth)
			{
				base.transform.position = Vector3.SmoothDamp(base.transform.position, vector2, ref curVelocity, damping);
			}
			else
			{
				base.transform.position = vector2;
			}
			rect.sizeDelta = new Vector2(width, 1f);
		}
		if ((bool)startTarget)
		{
			rect.pivot = new Vector2(1f, 0.5f);
		}
		if (!fillProcessing)
		{
			return;
		}
		if (fill)
		{
			image.fillAmount += 0.2f;
			if (image.fillAmount >= 1f && (bool)fillNext)
			{
				fillNext.Fill();
				fillProcessing = false;
			}
		}
		else
		{
			image.fillAmount -= 0.2f;
			if (image.fillAmount <= 0f && (bool)fillParent)
			{
				fillParent.Empty();
				fillProcessing = false;
			}
		}
	}

	public void Fill()
	{
		fillProcessing = true;
		fill = true;
	}

	public void Empty()
	{
		fillProcessing = true;
		fill = false;
	}

	public void Reset()
	{
		Empty();
		width = 0f;
	}
}
