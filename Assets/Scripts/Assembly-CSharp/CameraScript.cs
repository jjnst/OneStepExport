using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraScript : MonoBehaviour
{
	public Transform target;

	public float damping = 1f;

	public float zOffset = 10f;

	public Transform[] canvasPos;

	private float offsetZ;

	private Vector3 currentVelocity;

	public float shake = 0f;

	public Camera mainCam;

	public float[] shakeAmount;

	public float decreaseFactor = 1f;

	public float shakeMultiplier = 1f;

	public Image cameraPane;

	public Overlay chronoOverlay;

	public Overlay healthOverlay;

	public Animator transitionOverlayHigh;

	public Animator effectOverlayHigh;

	public Animator transitionOverlayLow;

	public Animator designEffects;

	public Canvas canvas;

	public List<SlideBody> slideBodies;

	private void Start()
	{
		base.transform.SetParent(null);
	}

	public void TransitionInHigh(string transitionName)
	{
		transitionOverlayHigh.Play(transitionName);
	}

	public void TransitionOutHigh(string transitionName)
	{
		transitionOverlayHigh.Play(transitionName + "Out");
	}

	public void TransitionInLow(string transitionName)
	{
		transitionOverlayLow.Play(transitionName);
	}

	public void TransitionOutLow(string transitionName)
	{
		transitionOverlayLow.Play(transitionName + "Out");
	}

	public void Effect(string transitionName, Vector3 position)
	{
		effectOverlayHigh.transform.position = position;
		effectOverlayHigh.Play(transitionName);
	}

	private void Update()
	{
		Vector3 position = target.position - base.transform.TransformDirection(Vector3.forward) * zOffset;
		Vector3 vector = Vector3.SmoothDamp(base.transform.position, position, ref currentVelocity, damping, float.PositiveInfinity, Time.unscaledDeltaTime);
		base.transform.position = vector;
		base.transform.rotation = target.rotation;
		if (shake > 0f)
		{
			base.transform.position += new Vector3(Random.insideUnitCircle.x * shake * shakeMultiplier, Random.insideUnitCircle.y * shake * shakeMultiplier, 0f);
			shake -= Time.unscaledDeltaTime * decreaseFactor;
		}
		else
		{
			shake = 0f;
			if (Vector2.Distance(base.transform.position, vector) < 0.001f)
			{
				base.transform.position = position;
			}
			else
			{
				base.transform.position = vector;
			}
		}
		if (BC.playerChronoScale == 1f)
		{
			chronoOverlay.targetAlpha = 1f - Time.timeScale;
		}
		else
		{
			chronoOverlay.targetAlpha = 0f;
		}
	}

	public void Shake(int shakeSize)
	{
		if (shake > 0f)
		{
			if (shake + shakeAmount[shakeSize] / 2f < shakeAmount[shakeSize] * 2f)
			{
				shake += shakeAmount[shakeSize] / 2f;
			}
		}
		else
		{
			shake += shakeAmount[shakeSize];
		}
	}

	public void CameraChangePos(int canvasPosNum, bool instant = false)
	{
		target = canvasPos[canvasPosNum];
		if (instant)
		{
			base.transform.position = canvasPos[canvasPosNum].position + Vector3.forward * (0f - zOffset);
		}
	}
}
