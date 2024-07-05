using System;
using System.Collections.Generic;
using LapinerTools.Steam.Data;
using LapinerTools.Steam.Data.Internal;
using Steamworks;
using UnityEngine;

namespace LapinerTools.Steam
{
	public class SteamMainBase<SteamMainT> : MonoBehaviour where SteamMainT : SteamMainBase<SteamMainT>
	{
		protected static SteamMainT s_instance;

		protected SteamRequestList m_pendingRequests = new SteamRequestList();

		private Dictionary<string, List<object>> m_singleShotEventHandlers = new Dictionary<string, List<object>>();

		protected object m_lock = new object();

		[SerializeField]
		[Tooltip("Set this property to true if you want to see a detailed log in the console. Disabled by default.")]
		protected bool m_isDebugLogEnabled = true;

		public static SteamMainT Instance
		{
			get
			{
				if ((UnityEngine.Object)s_instance == (UnityEngine.Object)null)
				{
					s_instance = UnityEngine.Object.FindObjectOfType<SteamMainT>();
				}
				if ((UnityEngine.Object)s_instance == (UnityEngine.Object)null)
				{
					s_instance = new GameObject(typeof(SteamMainT).Name).AddComponent<SteamMainT>();
				}
				return s_instance;
			}
		}

		public static bool IsInstanceSet
		{
			get
			{
				return (UnityEngine.Object)s_instance != (UnityEngine.Object)null;
			}
		}

		public bool IsDebugLogEnabled
		{
			get
			{
				return m_isDebugLogEnabled;
			}
			set
			{
				m_isDebugLogEnabled = value;
			}
		}

		public event Action<ErrorEventArgs> OnError;

		public void Execute<T>(SteamAPICall_t p_steamCall, CallResult<T>.APIDispatchDelegate p_onCompleted)
		{
			CallResult<T> callResult = CallResult<T>.Create(p_onCompleted);
			callResult.Set(p_steamCall);
			m_pendingRequests.Add(callResult);
		}

		protected virtual void OnDisable()
		{
			if (m_pendingRequests != null)
			{
				m_pendingRequests.Cancel();
			}
		}

		protected virtual void LateUpdate()
		{
			lock (m_lock)
			{
				m_pendingRequests.RemoveInactive();
				if (!IsDebugLogEnabled || Time.frameCount % 300 != 0)
				{
					return;
				}
				if (m_pendingRequests.Count() > 0)
				{
					Debug.Log(typeof(SteamMainT).Name + ": pending requests left: " + m_pendingRequests.Count());
				}
				foreach (KeyValuePair<string, List<object>> singleShotEventHandler in m_singleShotEventHandlers)
				{
					if (singleShotEventHandler.Value.Count > 0)
					{
						Debug.Log(typeof(SteamMainT).Name + ": pending signle shot event handlers for '" + singleShotEventHandler.Key + "' left: " + singleShotEventHandler.Value.Count);
					}
				}
			}
		}

		protected virtual bool CheckAndLogResultNoEvent<Trequest>(string p_logText, EResult p_result, bool p_bIOFailure)
		{
			Action<object> p_event = null;
			return CheckAndLogResult<Trequest, object>(p_logText, p_result, p_bIOFailure, null, ref p_event);
		}

		protected virtual bool CheckAndLogResult<Trequest, Tevent>(string p_logText, EResult p_result, bool p_bIOFailure, string p_eventName, ref Action<Tevent> p_event)
		{
			lock (m_lock)
			{
				m_pendingRequests.RemoveInactive<Trequest>();
				if (IsDebugLogEnabled)
				{
					Debug.Log(string.Concat(p_logText, ": (fail:", p_bIOFailure.ToString(), ") ", p_result, " requests left: ", m_pendingRequests.Count<Trequest>()));
				}
			}
			if (p_result == EResult.k_EResultOK && !p_bIOFailure)
			{
				return true;
			}
			ErrorEventArgs errorEventArgs = ErrorEventArgs.Create(p_result);
			HandleError(p_logText + ": failed! ", errorEventArgs);
			if (p_eventName != null && p_event != null)
			{
				CallSingleShotEventHandlers(p_eventName, (Tevent)Activator.CreateInstance(typeof(Tevent), errorEventArgs), ref p_event);
			}
			return false;
		}

		protected virtual void HandleError(string p_logPrefix, ErrorEventArgs p_error)
		{
			Debug.LogError(p_logPrefix + p_error.ErrorMessage);
			InvokeEventHandlerSafely(this.OnError, p_error);
		}

		protected virtual void InvokeEventHandlerSafely<T>(Action<T> p_handler, T p_data)
		{
			try
			{
				if (p_handler != null)
				{
					p_handler(p_data);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat(typeof(SteamMainT).Name, ": your event handler ('", p_handler.Target, "' - System.Action<", typeof(T), ">) has thrown an excepotion!\n", ex));
			}
		}

		protected virtual void SetSingleShotEventHandler<T>(string p_eventName, ref Action<T> p_event, Action<T> p_handler)
		{
			if (p_handler != null)
			{
				if (!m_singleShotEventHandlers.ContainsKey(p_eventName))
				{
					m_singleShotEventHandlers.Add(p_eventName, new List<object>());
				}
				m_singleShotEventHandlers[p_eventName].Add(p_handler.Target);
				p_event = (Action<T>)Delegate.Combine(p_event, p_handler);
			}
		}

		protected virtual void CallSingleShotEventHandlers<T>(string p_eventName, T p_args, ref Action<T> p_event)
		{
			if (p_event == null || !m_singleShotEventHandlers.ContainsKey(p_eventName))
			{
				return;
			}
			int count = m_singleShotEventHandlers[p_eventName].Count;
			Delegate[] invocationList = p_event.GetInvocationList();
			Delegate[] array = invocationList;
			foreach (Delegate @delegate in array)
			{
				if (m_singleShotEventHandlers[p_eventName].Contains(@delegate.Target))
				{
					p_event = (Action<T>)Delegate.Remove(p_event, (Action<T>)@delegate);
					m_singleShotEventHandlers[p_eventName].Remove(@delegate.Target);
					try
					{
						@delegate.DynamicInvoke(p_args);
					}
					catch (Exception ex)
					{
						Debug.LogError(string.Concat(typeof(SteamMainT).Name, ": your event handler ('", @delegate.Target, "' - System.Action<", typeof(T), ">) has thrown an excepotion!\n", ex));
					}
				}
			}
			if (IsDebugLogEnabled)
			{
				Debug.Log(typeof(SteamMainT).Name + ": CallSingleShotEventHandlers '" + p_eventName + "' left handlers: " + ((p_event != null) ? p_event.GetInvocationList().Length : 0) + "/" + invocationList.Length + " left single shots: " + m_singleShotEventHandlers[p_eventName].Count + "/" + count);
			}
		}

		protected virtual void ClearSingleShotEventHandlers<T>(string p_eventName, ref Action<T> p_event)
		{
			if (p_event == null || !m_singleShotEventHandlers.ContainsKey(p_eventName))
			{
				return;
			}
			int count = m_singleShotEventHandlers[p_eventName].Count;
			Delegate[] invocationList = p_event.GetInvocationList();
			Delegate[] array = invocationList;
			foreach (Delegate @delegate in array)
			{
				if (m_singleShotEventHandlers[p_eventName].Contains(@delegate.Target))
				{
					p_event = (Action<T>)Delegate.Remove(p_event, (Action<T>)@delegate);
					m_singleShotEventHandlers[p_eventName].Remove(@delegate.Target);
				}
			}
			if (IsDebugLogEnabled)
			{
				Debug.Log(typeof(SteamMainT).Name + ": ClearSingleShotEventHandler '" + p_eventName + "' left handlers: " + ((p_event != null) ? p_event.GetInvocationList().Length : 0) + "/" + invocationList.Length + " left single shots: " + m_singleShotEventHandlers[p_eventName].Count + "/" + count);
			}
		}
	}
}
