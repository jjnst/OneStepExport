using UnityEngine;

public class Follower : MonoBehaviour
{
	public Transform target;

	private float originalDamping;

	public float damping = 0.05f;

	public bool disableOnReachTarget = false;

	public Animator anim;

	private float offsetZ;

	private Vector3 currentVelocity;

	protected virtual void Awake()
	{
		originalDamping = damping;
		anim = GetComponent<Animator>();
	}

	protected virtual void Update()
	{
		if ((bool)target && base.transform.position != target.position)
		{
			if (!target.gameObject.activeSelf)
			{
				target = null;
				return;
			}
			Vector3 vector = target.position - base.transform.TransformDirection(Vector3.forward);
			Vector3 position = Vector3.SmoothDamp(base.transform.position, vector, ref currentVelocity, damping);
			base.transform.position = position;
			base.transform.rotation = target.rotation;
		}
	}

	public void ResetDamping()
	{
		damping = originalDamping;
	}
}
