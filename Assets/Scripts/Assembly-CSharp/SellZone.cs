using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SellZone : MonoBehaviour, IDropHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!(eventData.pointerDrag == null))
		{
			SpellListCard component = eventData.pointerDrag.GetComponent<SpellListCard>();
			if (component != null)
			{
				component.placeholderParent = base.transform;
				GetComponent<Image>().color = new Color32(254, 50, 102, byte.MaxValue);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!(eventData.pointerDrag == null))
		{
			SpellListCard component = eventData.pointerDrag.GetComponent<SpellListCard>();
			if (component != null && component.placeholderParent == base.transform)
			{
				component.placeholderParent = component.parentToReturnTo;
			}
			GetComponent<Image>().color = Color.white;
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		SpellListCard component = eventData.pointerDrag.GetComponent<SpellListCard>();
		if (component != null)
		{
			component.parentToReturnTo = base.transform;
			GetComponent<Image>().color = Color.white;
		}
	}
}
