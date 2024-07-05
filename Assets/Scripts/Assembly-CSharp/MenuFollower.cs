using UnityEngine;

public class MenuFollower : MonoBehaviour
{
	public RectTransform target;

	private float originalDamping;

	public float damping = 1f;

	public Animator anim;

	private float offsetZ;

	private Vector3 currentVelocity;

	private void Awake()
	{
		originalDamping = damping;
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		if ((bool)target)
		{
			if (!target.gameObject.activeSelf)
			{
				target = null;
				return;
			}
			Vector3 vector = target.position - base.transform.TransformDirection(Vector3.forward);
			Vector3 position = Vector3.SmoothDamp(base.transform.position, vector, ref currentVelocity, damping, float.PositiveInfinity, Time.unscaledDeltaTime);
			base.transform.position = position;
			base.transform.rotation = target.rotation;
		}
	}

	public void ResetDamping()
	{
		damping = originalDamping;
	}
}
