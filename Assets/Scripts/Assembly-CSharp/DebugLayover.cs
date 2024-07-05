using TMPro;
using UnityEngine;

public class DebugLayover : MonoBehaviour
{
	public TMP_Text textWindow;

	private string debugInfo;

	public MyLog logger;

	private void Start()
	{
	}

	private void Update()
	{
		if (textWindow.gameObject.activeInHierarchy)
		{
			textWindow.text = string.Format("Luck: {0}", S.I.poCtrl.luck.ToString());
		}
	}

	public void Toggle()
	{
		textWindow.gameObject.SetActive(!textWindow.gameObject.activeInHierarchy);
	}
}
