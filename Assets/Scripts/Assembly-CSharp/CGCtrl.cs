using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CGCtrl : NavPanel
{
	private const string CG_EVIL_3_KEY = "CG_Evil_3_";

	private const string HAND_GOOD_KEY = "ShotGood_03hand-";

	private const string HAND_GRAB_KEY = "ShotGood_04hand-";

	private const string CHAR_EVIL_KEY = "ShotEvil_00Char_";

	private const string CAST_KEY = "ShotEvil_02bCast_";

	private const string MIRROR_KEY = "ShotEvil_08bMirror_";

	private const string STAND_KEY = "ShotEvil_08bStand_";

	public List<Sprite> pacifistCG;

	public List<Sprite> pacifistFalseCG;

	public List<Sprite> genocideCG;

	public List<Sprite> characterEvilCG;

	public Image image;

	public Image image2;

	private Dictionary<string, Sprite> characterEvilCGDictionary = new Dictionary<string, Sprite>();

	public VideoPlayer vidPlayer;

	public MeshRenderer videoScreen;

	public RawImage videoScreenSwitch;

	public Renderer mainVideoScreen;

	public Canvas canvas;

	public List<Sprite> handGoods;

	public List<Sprite> handGrabs;

	public Image handGood;

	public Image handGrab;

	public Image terraArm;

	public List<Sprite> charEvils;

	public List<Sprite> casts;

	public List<Sprite> mirrors;

	public List<Sprite> stands;

	public Animator setPiecesAnim;

	public Image charEvil;

	public Image cast;

	public Image mirror;

	public Image stand;

	public AudioSource audioSource;

	public VideoClip badEnding;

	public VideoClip goodEndingFalse;

	public VideoClip goodEndingTrue;

	private bool endReached = false;

	private bool skip = false;

	public BC ctrl;

	public UnlockCtrl unCtrl;

	private bool playing = false;

	private void OnMovieEvent(int FMVevent)
	{
		Debug.Log("script has received FMV event :" + FMVevent);
	}

	protected override void Awake()
	{
		base.Awake();
		videoScreenSwitch.enabled = false;
		videoScreen.enabled = false;
		mainVideoScreen = videoScreen;
		vidPlayer.loopPointReached += EndReached;
	}

	private void EndReached(VideoPlayer vp)
	{
		endReached = true;
	}

	private void Update()
	{
		if (playing)
		{
			if (skip)
			{
				audioSource.volume = 0f;
				vidPlayer.playbackSpeed = Time.timeScale * 20f;
			}
			else
			{
				audioSource.volume = 1f;
				vidPlayer.playbackSpeed = Time.timeScale;
			}
			SetVideoScreenScale();
		}
	}

	private void SetVideoScreenScale()
	{
		if (ScalableCamera.screenSize.x / ScalableCamera.ratio >= ScalableCamera.screenSize.y)
		{
			float num = Camera.main.orthographicSize * 2f * (float)Screen.width / (float)Screen.height;
			if (mainVideoScreen != null)
			{
				mainVideoScreen.transform.localScale = new Vector3(num, 0f, num * 9f / 16f) / 10f;
			}
			videoScreenSwitch.transform.localScale = new Vector3(num / 42.39555f, num * 9f / 16f / 23.8474f, 1f) / 10f;
			setPiecesAnim.transform.localScale = new Vector3(num / 42.39555f, num * 9f / 16f / 23.8474f, 1f) / 10f;
		}
		else
		{
			float num2 = Camera.main.orthographicSize * 2f;
			if (mainVideoScreen != null)
			{
				mainVideoScreen.transform.localScale = new Vector3(num2 * 16f / 9f, 0f, num2) / 10f;
			}
			videoScreenSwitch.transform.localScale = new Vector3(num2 * 16f / 9f / 42.39555f, num2 / 23.8474f, 1f) / 10f;
			setPiecesAnim.transform.localScale = new Vector3(num2 * 16f / 9f / 42.39555f, num2 / 23.8474f, 1f) / 10f;
		}
	}

	private void SetSpriteFromList(Image theImage, List<Sprite> spriteList, string key)
	{
		theImage.sprite = null;
		foreach (Sprite sprite in spriteList)
		{
			if (sprite.name == key + ctrl.currentPlayer.beingObj.nameString)
			{
				theImage.sprite = sprite;
				break;
			}
		}
		if (!(theImage.sprite == null))
		{
			return;
		}
		foreach (Sprite sprite2 in spriteList)
		{
			if (sprite2.name == key + "Default")
			{
				theImage.sprite = sprite2;
				break;
			}
		}
	}

	public IEnumerator _StartScene(Ending type)
	{
		handGood.gameObject.SetActive(false);
		handGrab.gameObject.SetActive(false);
		terraArm.gameObject.SetActive(false);
		unCtrl.itemGrids.blocksRaycasts = false;
		charEvil.gameObject.SetActive(false);
		cast.gameObject.SetActive(false);
		stand.gameObject.SetActive(false);
		mirror.gameObject.SetActive(false);
		ctrl.GameState = GState.CG;
		vidPlayer.playbackSpeed = Time.timeScale;
		skip = false;
		Open();
		anim.SetBool("OnScreen", true);
		SetSpriteFromList(handGood, handGoods, "ShotGood_03hand-");
		SetSpriteFromList(handGrab, handGrabs, "ShotGood_04hand-");
		SetSpriteFromList(charEvil, charEvils, "ShotEvil_00Char_");
		SetSpriteFromList(cast, casts, "ShotEvil_02bCast_");
		SetSpriteFromList(mirror, mirrors, "ShotEvil_08bMirror_");
		SetSpriteFromList(stand, stands, "ShotEvil_08bStand_");
		if (mainVideoScreen != null)
		{
			mainVideoScreen.sortingLayerName = canvas.sortingLayerName;
			mainVideoScreen.sortingOrder = canvas.sortingOrder;
		}
		switch (type)
		{
		case Ending.PacifistFalse:
			vidPlayer.clip = goodEndingFalse;
			break;
		case Ending.PacifistTrue:
			vidPlayer.clip = goodEndingTrue;
			break;
		case Ending.Genocide:
			vidPlayer.clip = badEnding;
			break;
		}
		videoScreen.enabled = false;
		vidPlayer.Prepare();
		SetVideoScreenScale();
		float prepDuration = 0f;
		while (!vidPlayer.isPrepared || prepDuration > 5f)
		{
			prepDuration += Time.deltaTime;
			yield return null;
		}
		ctrl.camCtrl.TransitionOutHigh("InstantBlack");
		videoScreen.enabled = true;
		vidPlayer.Play();
		playing = true;
		switch (type)
		{
		case Ending.PacifistTrue:
			while (GetVideoTime() < 11.01f)
			{
				yield return null;
			}
			handGood.gameObject.SetActive(true);
			setPiecesAnim.SetTrigger("HandGood");
			while (GetVideoTime() < 13.33f)
			{
				yield return null;
			}
			handGood.gameObject.SetActive(false);
			handGrab.gameObject.SetActive(true);
			terraArm.gameObject.SetActive(true);
			while (GetVideoTime() < 14.95f)
			{
				yield return null;
			}
			StartCoroutine(FadeOver(handGrab, 1.9f));
			StartCoroutine(FadeOver(terraArm, 1.9f));
			break;
		case Ending.Genocide:
			charEvil.gameObject.SetActive(true);
			while (GetVideoTime() < 5.9f)
			{
				yield return null;
			}
			charEvil.gameObject.SetActive(false);
			cast.gameObject.SetActive(true);
			setPiecesAnim.SetTrigger("Cast");
			while (GetVideoTime() < 9.09f)
			{
				yield return null;
			}
			cast.gameObject.SetActive(false);
			while (GetVideoTime() < 36.233f)
			{
				yield return null;
			}
			cast.gameObject.SetActive(false);
			stand.gameObject.SetActive(true);
			mirror.gameObject.SetActive(true);
			setPiecesAnim.SetTrigger("Mirror");
			while (GetVideoTime() < 39.5f)
			{
				yield return null;
			}
			StartCoroutine(FadeOver(stand, 4f));
			StartCoroutine(FadeOver(mirror, 4f));
			break;
		}
		while (!endReached)
		{
			yield return null;
		}
		anim.SetBool("OnScreen", false);
		ctrl.camCtrl.TransitionInHigh("LeftWipe");
		endReached = false;
		if (S.I.ANIMATIONS)
		{
			yield return new WaitForSeconds(1.5f);
		}
		vidPlayer.Stop();
		Close();
		playing = false;
		skip = false;
	}

	private float GetVideoTime()
	{
		return (float)vidPlayer.time;
	}

	public void SkipScene()
	{
		if (playing)
		{
			skip = true;
		}
	}

	private IEnumerator FadeOver(Image image, float fadeDuration)
	{
		Color startColor = Color.white;
		Color endColor = Color.clear;
		float currentLerpTime = 0f;
		float endTime = Time.time + fadeDuration;
		while (currentLerpTime < endTime && image.gameObject.activeSelf)
		{
			currentLerpTime += Time.deltaTime;
			if (currentLerpTime > endTime)
			{
				currentLerpTime = endTime;
			}
			float t = currentLerpTime * 1f / fadeDuration * 1f;
			image.color = Color.Lerp(startColor, endColor, t);
			yield return null;
		}
		image.gameObject.SetActive(false);
	}
}
