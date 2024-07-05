using UnityEngine;

public class HitFX : MonoBehaviour
{
	public Animator anim;

	private float despawnTime = 2.5f;

	private void Update()
	{
		if (despawnTime < Time.time)
		{
			SimplePool.Despawn(base.gameObject);
		}
	}

	private void OnEnable()
	{
		anim.updateMode = AnimatorUpdateMode.Normal;
		despawnTime = Time.time + 2.5f;
	}

	public void Play(string hitType, bool pop, int damage, Transform newParent)
	{
		base.transform.SetParent(newParent);
		if (hitType == "down")
		{
			anim.SetTrigger("down");
		}
		else if (hitType != Status.Normal.ToString())
		{
			string text = hitType;
			if (pop)
			{
				text += "Pop";
			}
			if (hitType == Status.Chrono.ToString())
			{
				anim.updateMode = AnimatorUpdateMode.UnscaledTime;
			}
			anim.SetBool(text, true);
		}
		else if (damage > 99)
		{
			anim.SetTrigger("big");
		}
		else if (damage < 0)
		{
			anim.SetBool("Heal", true);
		}
		else
		{
			int value = Random.Range(1, 5);
			anim.SetInteger("fx", value);
		}
		anim.SetTrigger("play");
	}
}
