using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public int buttonNum = 0;

	public GameObject sendTo;

	public string nameOfMethod;

	private float clickTime;

	public bool onClick = true;

	public bool onDoubleClick = false;

	private void Awake()
	{
	}

	public void OnPointerClick(PointerEventData data)
	{
		int num = 1;
		float num2 = data.clickTime - clickTime;
		if ((double)num2 < 1.5 && num2 > 0f)
		{
			num = 2;
		}
		clickTime = data.clickTime;
		if (onClick && num == 1)
		{
			sendTo.SendMessage(nameOfMethod, buttonNum);
		}
		if (onDoubleClick && num != 2)
		{
		}
	}
}
