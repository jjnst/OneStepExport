using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace I2.Loc
{
	public class TranslationJob_GET : TranslationJob_WWW
	{
		private Dictionary<string, TranslationQuery> _requests;

		private Action<Dictionary<string, TranslationQuery>, string> _OnTranslationReady;

		private List<string> mQueries;

		public string mErrorMessage;

		public TranslationJob_GET(Dictionary<string, TranslationQuery> requests, Action<Dictionary<string, TranslationQuery>, string> OnTranslationReady)
		{
			_requests = requests;
			_OnTranslationReady = OnTranslationReady;
			mQueries = GoogleTranslation.ConvertTranslationRequest(requests, true);
			GetState();
		}

		private void ExecuteNextQuery()
		{
			if (mQueries.Count == 0)
			{
				mJobState = eJobState.Succeeded;
				return;
			}
			int index = mQueries.Count - 1;
			string arg = mQueries[index];
			mQueries.RemoveAt(index);
			string uri = string.Format("{0}?action=Translate&list={1}", LocalizationManager.GetWebServiceURL(), arg);
			www = UnityWebRequest.Get(uri);
			I2Utils.SendWebRequest(www);
		}

		public override eJobState GetState()
		{
			if (www != null && www.isDone)
			{
				ProcessResult(www.downloadHandler.data, www.error);
				www.Dispose();
				www = null;
			}
			if (www == null)
			{
				ExecuteNextQuery();
			}
			return mJobState;
		}

		public void ProcessResult(byte[] bytes, string errorMsg)
		{
			if (string.IsNullOrEmpty(errorMsg))
			{
				string @string = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
				errorMsg = GoogleTranslation.ParseTranslationResult(@string, _requests);
				if (string.IsNullOrEmpty(errorMsg))
				{
					if (_OnTranslationReady != null)
					{
						_OnTranslationReady(_requests, null);
					}
					return;
				}
			}
			mJobState = eJobState.Failed;
			mErrorMessage = errorMsg;
		}
	}
}
