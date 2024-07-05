using System;
using System.Text;
using Kittehface.Framework20;
using Steamworks;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	private static SteamManager s_instance;

	private static bool s_EverInitialized;

	private bool m_bInitialized;

	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	private static SteamManager Instance
	{
		get
		{
			if (s_instance == null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager>();
			}
			return s_instance;
		}
	}

	public static bool Initialized
	{
		get
		{
			return Instance.m_bInitialized;
		}
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	private void Awake()
	{
		if (s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		s_instance = this;
		if (s_EverInitialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Platform.initialized)
		{
			Platform.OnInitialized += delegate
			{
				m_bInitialized = Platform.SteamInitialized;
			};
		}
		else
		{
			m_bInitialized = Platform.SteamInitialized;
		}
		s_EverInitialized = true;
	}

	private void OnEnable()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (m_bInitialized)
		{
		}
	}

	private void OnDestroy()
	{
		if (!(s_instance != this))
		{
			s_instance = null;
			if (m_bInitialized)
			{
			}
		}
	}

	private void Update()
	{
		if (m_bInitialized)
		{
		}
	}
}
