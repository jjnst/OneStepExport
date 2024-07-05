using UnityEngine;

public class StatusDisplay : MonoBehaviour
{
	public Animator anim;

	public SpriteRenderer spriteRend;

	public Status status;

	public void Set(Being being, Status theStatus)
	{
		status = theStatus;
		base.transform.SetParent(being.transform);
		base.transform.localPosition = Vector3.zero;
		base.transform.rotation = being.transform.rotation;
		being.statusDisplays.Add(this);
		anim.runtimeAnimatorController = being.ctrl.itemMan.GetAnim(theStatus.ToString());
	}
}
