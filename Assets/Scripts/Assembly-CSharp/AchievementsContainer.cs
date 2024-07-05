using System.Collections;
using UnityEngine;

public class AchievementsContainer : MonoBehaviour
{
	public RectTransform achievementGrid;

	private void Start()
	{
		achievementGrid.transform.DestroyChildren();
		StartCoroutine(_SetAchievementsGrid());
	}

	private IEnumerator _SetAchievementsGrid()
	{
		if (!AchievementsCtrl.Initialized)
		{
			yield return new WaitUntil(() => AchievementsCtrl.Initialized);
		}
		AchievementsCtrl.Instance.achievementPopupGrid = achievementGrid;
	}

	private void Update()
	{
	}
}
