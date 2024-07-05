using System.Collections;
using I2.Loc;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[MoonSharpUserData]
public class IdleCtrl : MonoBehaviour
{
	public float slideTime;

	private BC ctrl;

	private DeckCtrl deCtrl;

	public TMP_Text heroNameText;

	public TMP_Text heroLevelText;

	public TMP_Text idleHealthText;

	public TMP_Text moneyText;

	public TMP_Text moneyTextBattle;

	public Transform moneyTextTarget;

	public AudioClip startZoneSound;

	public AudioClip menuSlideSound;

	public BGCtrl bgCtrl;

	public ButtonCtrl btnCtrl;

	public RunCtrl runCtrl;

	public ShopCtrl shopCtrl;

	public DeckScreen deckScreen;

	public Image onwardButtonImage;

	public Sprite onwardButtonSprite;

	public Sprite mapButtonSprite;

	public SlideBody onwardBtn;

	public VoteDisplay spareVoteDisplay;

	public VoteDisplay executeVoteDisplay;

	public TMP_Text onwardBtnText;

	public ControlsBackDisplay controlsBackDisplay;

	public TopNavButton spellsBtn;

	public Canvas canvas;

	public Animator anim;

	private void Start()
	{
		ctrl = S.I.batCtrl;
		deCtrl = S.I.deCtrl;
		spareVoteDisplay.gameObject.SetActive(false);
		executeVoteDisplay.gameObject.SetActive(false);
		anim = canvas.GetComponent<Animator>();
	}

	private IEnumerator AnimateText(Text textBox, string strComplete)
	{
		yield return new WaitForSeconds(0.1f);
		int i = 0;
		string str = "";
		while (i < strComplete.Length)
		{
			str = (textBox.text = str + strComplete[i++]);
			yield return new WaitForSeconds(0.01f);
		}
	}

	public void ShowOnwardButton()
	{
		if (ctrl.poCtrl.open)
		{
			onwardButtonImage.sprite = mapButtonSprite;
			onwardBtnText.text = ScriptLocalization.UI.Map_Map;
		}
		else
		{
			onwardButtonImage.sprite = onwardButtonSprite;
			onwardBtnText.text = ScriptLocalization.UI.MapButton_Onward;
		}
		if (!runCtrl.worldBar.open)
		{
			onwardBtn.Show();
		}
	}

	public void HideOnwardButton()
	{
		onwardBtn.Hide();
	}

	public void MakeWorldBarAvailable(bool spare = false, bool interactable = true)
	{
		if (runCtrl.currentRun == null)
		{
			return;
		}
		runCtrl.worldBar.available = true;
		runCtrl.worldBar.interactable = interactable;
		if (!spare)
		{
			return;
		}
		foreach (ZoneDot nextDot in runCtrl.currentZoneDot.nextDots)
		{
			nextDot.spareAvailable = true;
		}
	}

	public IEnumerator _ClickZoneButton(ZoneDot zoneDot)
	{
		if (PostCtrl.transitioning || ShopCtrl.transitioning)
		{
			yield break;
		}
		runCtrl.worldBar.available = false;
		runCtrl.worldBar.Close();
		ctrl.RemoveControlBlocksNextFrame(Block.ZoneSelection);
		PostCtrl.transitioning = true;
		onwardBtn.inverse = false;
		onwardBtn.Hide();
		deckScreen.slideBody.Hide();
		deckScreen.AnimateCollection();
		shopCtrl.Close();
		if ((bool)zoneDot.gameObject)
		{
			runCtrl.worldBar.selectionMarker.target = zoneDot.transform;
		}
		if (zoneDot.spareAvailable)
		{
			foreach (Cpu enemy in ctrl.ti.mainBattleGrid.currentEnemies)
			{
				if ((bool)enemy.GetComponent<Boss>())
				{
					enemy.GetComponent<Boss>().Spare(zoneDot);
					yield break;
				}
			}
		}
		if (zoneDot.nextDots.Count < 1)
		{
			yield return new WaitForSeconds(0.3f);
		}
		if ((bool)zoneDot && zoneDot.dark)
		{
			runCtrl.GoToDarkZone(zoneDot);
		}
		else
		{
			runCtrl.GoToNextZone(zoneDot);
		}
	}

	public void ClickDarkZoneButton()
	{
		if (!PostCtrl.transitioning)
		{
			deckScreen.slideBody.Hide();
			deckScreen.AnimateCollection();
			shopCtrl.Close();
		}
	}

	public void ToggleDeckShop()
	{
		if ((ctrl.GameState == GState.Idle || ctrl.GameState == GState.Loot || ctrl.GameState == GState.Experience || ctrl.GameState == GState.GameOver) && !btnCtrl.IsActivePanel(Nav.Options) && !ShopCtrl.transitioning && !PostCtrl.transitioning && S.I.heCtrl.gameMode != GameMode.PvP)
		{
			if (btnCtrl.IsActivePanel(deckScreen.foCtrl))
			{
				deckScreen.foCtrl.Close();
			}
			if (btnCtrl.IsActivePanel(deCtrl.deckScreen) || btnCtrl.IsActivePanel(shopCtrl))
			{
				deckScreen.Close();
			}
			else
			{
				deckScreen.Open();
			}
		}
	}

	public void ToggleFocusArtifacts()
	{
		btnCtrl.RemoveFocus();
		btnCtrl.SetFocus(deCtrl.artGrid.GetChild(0).GetComponent<NavButton>());
	}
}
