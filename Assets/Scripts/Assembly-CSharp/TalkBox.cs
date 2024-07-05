using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using I2.Loc;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;

[MoonSharpUserData]
public class TalkBox : MonoBehaviour
{
	public TMP_Text textBox;

	private UnityEngine.Coroutine textCo;

	public Animator anim;

	public UIFollow follower;

	public float textDelay = 0.012f;

	public float preLineDelay = 0.02f;

	public float lineDelay = 0.4f;

	public float endDelay = 0.02f;

	public float delayLength = 0.5f;

	public AudioClip vocSynth;

	public void Set(Being being)
	{
		if (!string.IsNullOrEmpty(being.beingObj.vocSynth))
		{
			vocSynth = S.I.itemMan.GetAudioClip(being.beingObj.vocSynth);
		}
		follower.Set(0f, 35f, being.transform, false, being.ctrl.talkBubblesContainer);
	}

	public void Show(bool instant = false)
	{
		if ((bool)anim)
		{
			textBox.text = "";
			if (instant)
			{
				anim.SetTrigger("instant");
			}
			anim.SetBool("visible", true);
		}
	}

	public void Hide()
	{
		if ((bool)anim)
		{
			textBox.text = "";
			anim.SetBool("visible", false);
		}
	}

	public void Fade()
	{
		anim.SetTrigger("fade");
		anim.SetBool("visible", false);
	}

	public float AnimateNextLine(string key)
	{
		return AnimateText(GetNextLine(key));
	}

	public float AnimateRandomLine(string key)
	{
		return AnimateText(GetRandomLine(key));
	}

	public void InstantRandomLine(string key)
	{
		Show(true);
		S.I.PlayOnce(vocSynth);
		SetText(GetRandomLine(key));
	}

	public void SetText(string strComplete)
	{
		List<string> list = new List<string>(strComplete.Split(new string[1] { "<nx>" }, StringSplitOptions.None));
		textBox.text = list[0];
		textBox.maxVisibleCharacters = list[0].Length;
	}

	public float AnimateText(string strComplete)
	{
		textBox.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		if (textCo != null)
		{
			StopCoroutine(textCo);
		}
		List<string> list = new List<string>(strComplete.Split(new string[1] { "<nx>" }, StringSplitOptions.None));
		if (list.Count > 0 && base.gameObject.activeInHierarchy)
		{
			textCo = StartCoroutine(AnimateTextC(textBox, list, lineDelay));
		}
		string text = Regex.Replace(strComplete, "[^@]", "");
		return textDelay * (float)strComplete.Length + (float)list.Count * (lineDelay + preLineDelay) + endDelay + (float)text.Length * delayLength;
	}

	private IEnumerator AnimateTextC(TMP_Text textBox, List<string> lines, float finalLineDelay)
	{
		Show();
		for (int i = 0; i < lines.Count; i++)
		{
			if (string.IsNullOrEmpty(lines[i]))
			{
				continue;
			}
			yield return new WaitForSeconds(preLineDelay);
			string delays = Regex.Replace(lines[i], "[^@]", "");
			lines[i] = lines[i].Replace("@", "");
			textBox.text = lines[i];
			int x = 0;
			int vocalInterval = 2;
			char[] charArray = (lines[i] + " ").ToCharArray();
			for (; x <= lines[i].Length; x++)
			{
				if (lines[i].Length <= 0)
				{
					break;
				}
				textBox.maxVisibleCharacters = x;
				if (!char.IsWhiteSpace(charArray[x]))
				{
					yield return new WaitForSeconds(textDelay);
					vocalInterval++;
					if (vocalInterval > 2)
					{
						S.I.PlayOnce(vocSynth);
						vocalInterval = 0;
					}
				}
			}
			yield return new WaitForSeconds(finalLineDelay + (float)delays.Length * delayLength);
		}
		yield return new WaitForSeconds(endDelay);
		Hide();
	}

	public string GetRandomLine(string key)
	{
		List<string> list = new List<string>();
		for (int i = 1; LocalizationManager.GetTranslation(key + i) != null; i++)
		{
			list.Add(LocalizationManager.GetTranslation(key + i));
		}
		string result = "";
		if (list.Count > 0)
		{
			result = list[UnityEngine.Random.Range(0, list.Count)];
		}
		return result;
	}

	public string GetNextLine(string key)
	{
		List<string> list = new List<string>();
		for (int i = 1; LocalizationManager.GetTranslation(key + i) != null; i++)
		{
			list.Add(LocalizationManager.GetTranslation(key + i));
		}
		int num = (num = SaveDataCtrl.Get(key, 1));
		if (num >= list.Count)
		{
			num = list.Count - 1;
		}
		string result = "";
		if (list.Count > 0)
		{
			result = list[num];
		}
		num++;
		SaveDataCtrl.Set(key, num);
		return result;
	}
}
