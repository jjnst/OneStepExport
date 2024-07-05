using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MechDetails : MonoBehaviour
{
	public Image icon;

	public TMP_Text keyword;

	public TMP_Text description;

	public CardInner cardInner;

	private void Awake()
	{
	}

	public void FillTra(EffectApp efApp, Sprite icon, string dictKey)
	{
		this.icon.sprite = icon;
		string translation = LocalizationManager.GetTranslation("MechKeys/" + dictKey);
		keyword.text = translation;
		string translation2 = LocalizationManager.GetTranslation("MechTooltips/" + dictKey);
		string text = cardInner.deCtrl.ParseEffectApp(translation2, efApp);
		description.text = text;
	}
}
