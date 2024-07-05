using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!(eventData.pointerDrag == null))
		{
			SpellListCard component = eventData.pointerDrag.GetComponent<SpellListCard>();
			if (component != null)
			{
				component.placeholderParent = base.transform;
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
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		SpellListCard component = eventData.pointerDrag.GetComponent<SpellListCard>();
		if (!(component != null))
		{
			return;
		}
		if (base.transform.name == "DeckGrid")
		{
			if (base.transform.childCount <= S.I.deCtrl.maxDeckSize && (bool)component.placeholderParent && component.placeholderParent.name != "CollectionGrid")
			{
				component.parentToReturnTo = base.transform;
			}
		}
		else
		{
			component.parentToReturnTo = base.transform;
		}
	}
}
