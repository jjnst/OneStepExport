using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleView : MonoBehaviour
{
	private ConsoleCtrl console = new ConsoleCtrl();

	public GameObject viewContainer;

	public Text logTextArea;

	public InputField inputField;

	private void Start()
	{
		if (console != null)
		{
			console.visibilityChanged += onVisibilityChanged;
			console.logChanged += onLogChanged;
		}
		updateLogStr(console.log);
	}

	~ConsoleView()
	{
		console.visibilityChanged -= onVisibilityChanged;
		console.logChanged -= onLogChanged;
	}

	private void Update()
	{
		if (S.I.CONSOLE)
		{
			Rewired.Player rewiredPlayer = RunCtrl.GetRewiredPlayer();
			if (rewiredPlayer != null && rewiredPlayer.GetButtonDown("Console"))
			{
				toggleVisibility();
			}
			if ((bool)inputField && Input.GetKeyDown(KeyCode.Return))
			{
				runCommand();
			}
		}
	}

	private void toggleVisibility()
	{
		setVisibility(!viewContainer.activeSelf);
	}

	private void setVisibility(bool visible)
	{
		viewContainer.SetActive(visible);
		if (visible)
		{
			S.I.batCtrl.AddControlBlocks(Block.Console);
			inputField.ActivateInputField();
			inputField.Select();
		}
		else
		{
			inputField.text = inputField.text.Replace("`", "");
			S.I.batCtrl.RemoveControlBlocks(Block.Console);
		}
	}

	private void onVisibilityChanged(bool visible)
	{
		setVisibility(visible);
	}

	private void onLogChanged(string[] newLog)
	{
		updateLogStr(newLog);
	}

	private void updateLogStr(string[] newLog)
	{
		if (newLog == null)
		{
			logTextArea.text = "";
		}
		else
		{
			logTextArea.text = string.Join("\n", newLog);
		}
	}

	public void runCommand()
	{
		console.runCommandString(inputField.text);
		inputField.text = "";
		inputField.ActivateInputField();
		inputField.Select();
	}
}
