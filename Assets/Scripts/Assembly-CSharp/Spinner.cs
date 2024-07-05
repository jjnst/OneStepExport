using UnityEngine;

public class Spinner : MonoBehaviour
{
	public Transform target;

	private float originalDamping;

	public float damping = 0.05f;

	public bool disableOnReachTarget = false;

	public Animator anim;

	private float offsetZ;

	private Vector3 currentVelocity;

	public int targetZ = 0;

	public int speedZ = 0;

	public float speed = 0.1f;

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
		Quaternion b = Quaternion.Euler(0f, 0f, targetZ);
		if (speedZ == 0)
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, b, Time.deltaTime * 2f);
		}
		else
		{
			base.transform.Rotate(0f, 0f, (float)speedZ * Time.deltaTime);
		}
	}

	public void FadeRotation()
	{
		speedZ = 0;
		targetZ = 0;
	}
}
