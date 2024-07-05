using System.Collections;
using TMPro;
using UnityEngine;

public class CreditBeingText : MonoBehaviour
{
	public TMP_Text creditText;

	public Being being;

	public Tile tile;

	private void Start()
	{
	}

	public CreditBeingText Set(string creditName, Being theBeing)
	{
		creditText.text = creditName;
		being = theBeing;
		StartCoroutine(_SetTextBoundsCollider());
		return this;
	}

	private IEnumerator _SetTextBoundsCollider()
	{
		yield return null;
		being.col.size = new Vector2(creditText.textBounds.size.x, being.col.size.y);
		being.col.offset = new Vector2(0f, -20f);
		being.shadow.transform.localPosition += new Vector3(0f, -20f, 0f);
	}

	private void Update()
	{
	}
}
