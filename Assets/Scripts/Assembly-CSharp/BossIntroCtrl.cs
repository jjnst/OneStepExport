using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossIntroCtrl : MonoBehaviour
{
	public TMP_Text nameText;

	public TMP_Text descriptionText;

	public Image image;

	public Image bgImage;

	public Animator anim;

	private void Awake()
	{
		anim = GetComponent<Animator>();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetIntro(GameObject theBoss)
	{
		Boss component = theBoss.GetComponent<Boss>();
		nameText.text = S.I.spCtrl.beingDictionary[component.bossID].localizedName;
		descriptionText.text = component.introDescription;
		image.sprite = component.introSprite;
		bgImage.color = component.introBGColor;
	}
}
