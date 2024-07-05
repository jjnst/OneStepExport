using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTracker : Spell
{
	public float lerpTime = 0.5f;

	public float trackingDuration = 2f;

	public TrailRenderer thisTrailer;

	public Gradient laserGradient;

	public LineRenderer lineRend;

	public override IEnumerator Cast()
	{
		being.mov.SetState(State.Attacking);
		if (ctrl.currentPlayers.Count < 1)
		{
			yield break;
		}
		int randomPlayerInt = Random.Range(0, ctrl.currentPlayers.Count);
		Player selectedPlayer = ctrl.currentPlayers[randomPlayerInt];
		Moveable playerMov = ctrl.currentPlayers[randomPlayerInt].mov;
		if (playerMov == null || playerMov.gameObject == null)
		{
			yield break;
		}
		Tile oldTile = playerMov.currentTile;
		List<Tile> tileHistory = new List<Tile>();
		List<float> timeHistory = new List<float>();
		float timeOnTile = 0f;
		float currentTrackTime = Time.time + trackingDuration;
		tileHistory.Add(playerMov.currentTile);
		Projectile firstLerper = spellObj.P.CreateLerperEffect(spellObj, playerMov.endTile, MovPattern.None, lerpTime, bulletSpeed, false, duration, animMark.name, castSound.name);
		firstLerper.transform.position = selectedPlayer.transform.position;
		firstLerper.transform.SetParent(playerMov.transform);
		TrailRenderer trailer = Object.Instantiate(thisTrailer, selectedPlayer.transform.position, base.transform.rotation);
		trailer.transform.SetParent(ctrl.transform);
		trailer.enabled = true;
		beingAnim.SetTrigger("lift");
		tileHistory.Add(playerMov.currentTile);
		timeHistory.Add(0f);
		while (currentTrackTime > Time.time)
		{
			if (selectedPlayer == null)
			{
				yield break;
			}
			timeOnTile += Time.deltaTime;
			firstLerper.transform.position = selectedPlayer.transform.position;
			trailer.transform.position = selectedPlayer.transform.position;
			if (playerMov.currentTile != oldTile)
			{
				tileHistory.Add(playerMov.currentTile);
				timeHistory.Add(timeOnTile);
				timeOnTile = 0f;
				oldTile = playerMov.currentTile;
			}
			yield return new WaitForEndOfFrame();
		}
		tileHistory.Add(playerMov.currentTile);
		timeHistory.Add(timeOnTile);
		if (tileHistory.Count < 1)
		{
			yield break;
		}
		Projectile newLerper = spellObj.P.CreateCustomLerper(spellObj, tileHistory[0], MovPattern.None, lerpTime, bulletSpeed, false, duration, damage, animShot.name, shotSound.name);
		lineRend.enabled = true;
		lineRend.positionCount = 2;
		lineRend.sortingLayerName = "ProjChar";
		lineRend.colorGradient = laserGradient;
		lineRend.material = GetComponent<LineRenderer>().material;
		lineRend.startWidth = 2f;
		lineRend.SetPosition(0, being.gunPoint.position);
		lineRend.SetPosition(1, newLerper.transform.position);
		beingAnim.SetTrigger("charge");
		beingAnim.SetTrigger("channel");
		float currentLaserTime = Time.time;
		for (int i = 0; i < tileHistory.Count - 1; i++)
		{
			if (selectedPlayer == null || i >= timeHistory.Count || i + 1 >= tileHistory.Count)
			{
				yield break;
			}
			currentLaserTime += timeHistory[i];
			yield return new WaitUntil(() => currentLaserTime <= Time.time);
			if (newLerper.isActiveAndEnabled)
			{
				newLerper.mov.MoveToTile(tileHistory[i + 1], false, false, true);
			}
			while (newLerper.mov.state != 0 && newLerper.isActiveAndEnabled)
			{
				lineRend.SetPosition(0, being.gunPoint.position);
				lineRend.SetPosition(1, newLerper.transform.position);
				yield return new WaitForEndOfFrame();
			}
		}
		beingAnim.SetTrigger("release");
		lineRend.enabled = false;
		being.mov.SetState(State.Idle);
		Object.Destroy(trailer.gameObject);
	}

	public void CancelSpell()
	{
		thisTrailer.enabled = false;
		lineRend.enabled = false;
	}
}
