using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class U : SerializedMonoBehaviour
{
	[InlineProperty(LabelWidth = 90)]
	public struct MyCustomType
	{
		public int SomeMember;

		public GameObject SomePrefab;
	}

	public static U I;

	public bool ______Colors_________;

	public Dictionary<FTrigger, Color> triggerPalette;

	public Dictionary<UIColor, Color> uiPalette;

	public Dictionary<UIColor, string> uiPaletteHex;

	private void Awake()
	{
		if (I != null)
		{
			Debug.LogError("Multiple instances of Singleton U!");
		}
		I = this;
		foreach (UIColor key in uiPalette.Keys)
		{
			uiPaletteHex[key] = ColorUtility.ToHtmlStringRGB(uiPalette[key]);
		}
	}

	private void Update()
	{
	}

	public void Hide(GameObject uiGO, float delay = 0f)
	{
		if (delay <= 0f)
		{
			uiGO.GetComponent<Animator>().SetBool("OnScreen", false);
		}
		else
		{
			StartCoroutine(HideAfter(uiGO, delay));
		}
	}

	public void Hide(TMP_Text uiTmp, float delay = 0f)
	{
		Hide(uiTmp.gameObject, delay);
	}

	private IEnumerator HideAfter(GameObject uiGO, float delay)
	{
		yield return new WaitForSeconds(delay);
		Hide(uiGO);
	}

	public void Show(GameObject uiGO, float delay = 0f)
	{
		if (delay <= 0f)
		{
			uiGO.GetComponent<Animator>().SetBool("OnScreen", true);
		}
		else
		{
			StartCoroutine(ShowAfter(uiGO, delay));
		}
	}

	public void Show(TMP_Text uiTmp, float delay = 0f)
	{
		Show(uiTmp.gameObject, delay);
	}

	private IEnumerator ShowAfter(GameObject uiGO, float delay)
	{
		yield return new WaitForSeconds(delay);
		Show(uiGO);
	}

	public void Toggle(GameObject uiGO)
	{
		if (uiGO.GetComponent<Animator>().GetBool("OnScreen"))
		{
			uiGO.GetComponent<Animator>().SetBool("OnScreen", false);
		}
		else
		{
			uiGO.GetComponent<Animator>().SetBool("OnScreen", true);
		}
	}

	public string Colorify(string theString, UIColor uiColor)
	{
		return "<color=#" + I.uiPaletteHex[uiColor] + ">" + theString + "</color>";
	}

	public string GetColorStarter(UIColor uiColor)
	{
		return "<color=#" + I.uiPaletteHex[uiColor] + ">";
	}

	public Color GetColor(UIColor uiColor)
	{
		if (uiPalette.ContainsKey(uiColor))
		{
			return uiPalette[uiColor];
		}
		Debug.LogError("COLOR DOES NOT EXIST FOR " + uiColor);
		return Color.white;
	}
}
