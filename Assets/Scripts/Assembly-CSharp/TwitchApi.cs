using TwitchLib.Unity;
using UnityEngine;

public class TwitchApi : MonoBehaviour
{
	public TwitchClient twitchClient;

	public Api api;

	private Client client;

	private void Start()
	{
		Application.runInBackground = true;
	}

	public void InitializeApi(string oAuthToken)
	{
		api = new Api();
		api.Settings.AccessToken = oAuthToken;
		api.Settings.ClientId = Secrets.client_id;
	}
}
