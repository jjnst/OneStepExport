using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatchNotesCtrl : NavPanel
{
	public TMP_Text patchNoteText;

	public TextAsset patchNotesAsset;

	public Scrollbar scrollbar;

	public ScrollRect scrollRect;

	public GameObject steamButton;

	private void Start()
	{
		patchNoteText.text = string.Empty;
		steamButton.SetActive(true);
	}

	public override void Open()
	{
		base.Open();
		btnCtrl.RemoveFocus();
		if (string.IsNullOrEmpty(patchNoteText.text))
		{
			patchNoteText.ClearMesh();
			patchNoteText.text = patchNotesAsset.text;
		}
		StartCoroutine(_SetScrollbar());
	}

	private IEnumerator _SetScrollbar()
	{
		yield return new WaitForEndOfFrame();
		scrollbar.value = 1f;
	}

	public override void Close()
	{
		base.Close();
		S.I.mainCtrl.Open();
	}

	public void ClickPatchNotes()
	{
		Application.OpenURL("https://store.steampowered.com/newshub/app/960690");
	}
}
