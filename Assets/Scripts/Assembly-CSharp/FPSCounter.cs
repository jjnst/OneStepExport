using UnityEngine;

public class FPSCounter : MonoBehaviour
{
	private float deltaTime = 0f;

	public bool fpsCounterOn = false;

	private void Awake()
	{
	}

	private void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}
}
