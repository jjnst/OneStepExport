using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckMarker : MonoBehaviour
{
	public Image icon;

	public Image line;

	public TMP_Text text;

	public CanvasGroup canvasGroup;

	public void Highlight()
	{
		if ((bool)icon)
		{
			icon.color = U.I.GetColor(UIColor.Effect);
		}
		line.color = U.I.GetColor(UIColor.Effect);
		text.color = U.I.GetColor(UIColor.Effect);
		canvasGroup.alpha = 0.8f;
	}

	public void Reset()
	{
		if ((bool)icon)
		{
			icon.color = Color.white;
		}
		line.color = Color.white;
		text.color = Color.white;
		canvasGroup.alpha = 0.7f;
	}
}
