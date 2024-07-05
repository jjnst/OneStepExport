using TMPro;
using UnityEngine;

public class StatUI : MonoBehaviour
{
	public TMP_Text tmp;

	private void Start()
	{
	}

	public void UpdateStatUI(string value)
	{
		tmp.text = value;
	}
}
