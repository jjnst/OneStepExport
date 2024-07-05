using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

public class ConsoleCtrl
{
	public delegate void LogChangedHandler(string[] log);

	public delegate void VisibilityChangedHandler(bool visible);

	private class CommandRegistration
	{
		public string command { get; private set; }

		public CommandHandler handler { get; private set; }

		public string help { get; private set; }

		public CommandRegistration(string command, CommandHandler handler, string help)
		{
			this.command = command;
			this.handler = handler;
			this.help = help;
		}
	}

	private const int scrollbackSize = 20;

	private Queue<string> scrollback = new Queue<string>(20);

	private List<string> commandHistory = new List<string>();

	private Dictionary<string, CommandRegistration> commands = new Dictionary<string, CommandRegistration>();

	private const string repeatCmdName = "!!";

	public string[] log { get; private set; }

	public event LogChangedHandler logChanged;

	public event VisibilityChangedHandler visibilityChanged;

	public ConsoleCtrl()
	{
		registerCommand("help", help, "Print this help.");
		registerCommand("hide", hide, "Hide the console.");
		registerCommand("!!", repeatCommand, "Repeat last command.");
		registerCommand("reload", reload, "Reload game.");
		registerCommand("r", reload, "Reload game.");
		registerCommand("spawn", spawn, "Spawn a being on x and y. {enemy int, field x, field y}");
		registerCommand("phealth", pHealth, "Set player health");
		registerCommand("pdefense", pDefense, "Set player defense");
		registerCommand("money", money, "Set player money");
		registerCommand("resetprefs", resetPrefs, "(WARNING) Reset & saves SavedData.");
		registerCommand("clear", clear, "Go to Zone {world int, zone int}");
		registerCommand("gotozone", goToZone, "Go to Zone {world int, zone int}");
		registerCommand("bg", changeBG, "Change bg to {bgName}");
		registerCommand("addspell", addSpell, "Add spell {spellName} to end of deck");
		registerCommand("animations", setAnimations, "Set animations to {bool}");
		registerCommand("campaign", setCampaign, "Set Campaign to {bool}");
		registerCommand("dev", dev, "Set MainMenu to {bool}");
		registerCommand("transitioncheck", transitionCheck, "Check transitions");
	}

	private void registerCommand(string command, CommandHandler handler, string help)
	{
		commands.Add(command, new CommandRegistration(command, handler, help));
	}

	public void appendLogLine(string line)
	{
		if (scrollback.Count >= 20)
		{
			scrollback.Dequeue();
		}
		scrollback.Enqueue(line);
		log = scrollback.ToArray();
		if (this.logChanged != null)
		{
			this.logChanged(log);
		}
	}

	public void runCommandString(string commandString)
	{
		appendLogLine("$ " + commandString);
		string[] array = parseArguments(commandString);
		string[] array2 = new string[0];
		if (array.Length < 1)
		{
			appendLogLine(string.Format("Unable to process command '{0}'", commandString));
			return;
		}
		if (array.Length >= 2)
		{
			int num = array.Length - 1;
			array2 = new string[num];
			Array.Copy(array, 1, array2, 0, num);
		}
		runCommand(array[0].ToLower(), array2);
		commandHistory.Add(commandString);
	}

	public void runCommand(string command, string[] args)
	{
		CommandRegistration value = null;
		if (!commands.TryGetValue(command, out value))
		{
			appendLogLine(string.Format("Unknown command '{0}', type 'help' for list.", command));
		}
		else if (value.handler == null)
		{
			appendLogLine(string.Format("Unable to process command '{0}', handler was null.", command));
		}
		else
		{
			value.handler(args);
		}
	}

	private static string[] parseArguments(string commandString)
	{
		LinkedList<char> linkedList = new LinkedList<char>(commandString.ToCharArray());
		bool flag = false;
		LinkedListNode<char> linkedListNode = linkedList.First;
		while (linkedListNode != null)
		{
			LinkedListNode<char> next = linkedListNode.Next;
			if (linkedListNode.Value == '"')
			{
				flag = !flag;
				linkedList.Remove(linkedListNode);
			}
			if (!flag && linkedListNode.Value == ' ')
			{
				linkedListNode.Value = '\n';
			}
			linkedListNode = next;
		}
		char[] array = new char[linkedList.Count];
		linkedList.CopyTo(array, 0);
		return new string(array).Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
	}

	private void babble(string[] args)
	{
		if (args.Length < 2)
		{
			appendLogLine("Expected 2 arguments.");
			return;
		}
		string text = args[0];
		if (string.IsNullOrEmpty(text))
		{
			appendLogLine("Expected arg1 to be text.");
			return;
		}
		int result = 0;
		if (!int.TryParse(args[1], out result))
		{
			appendLogLine("Expected an integer for arg2.");
			return;
		}
		for (int i = 0; i < result; i++)
		{
			appendLogLine(string.Format("{0} {1}", text, i));
		}
	}

	private void echo(string[] args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string arg in args)
		{
			stringBuilder.AppendFormat("{0},", arg);
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		appendLogLine(stringBuilder.ToString());
	}

	private void help(string[] args)
	{
		foreach (CommandRegistration value in commands.Values)
		{
			appendLogLine(string.Format("{0}: {1}", value.command, value.help));
		}
	}

	private void hide(string[] args)
	{
		if (this.visibilityChanged != null)
		{
			this.visibilityChanged(false);
		}
	}

	private void repeatCommand(string[] args)
	{
		for (int num = commandHistory.Count - 1; num >= 0; num--)
		{
			string text = commandHistory[num];
			if (!string.Equals("!!", text))
			{
				runCommandString(text);
				break;
			}
		}
	}

	private void reload(string[] args)
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	private void pHealth(string[] args)
	{
		Health health = S.I.batCtrl.currentPlayer.health;
		int maxHealth = -1;
		S.I.deCtrl.statsScreen.consoleHealthChange = int.Parse(args[0]) - health.max;
		health.SetHealth(int.Parse(args[0]), maxHealth);
		if (args.Length > 1)
		{
			S.I.deCtrl.statsScreen.consoleHealthChange = int.Parse(args[1]) - health.max;
			maxHealth = int.Parse(args[1]);
		}
	}

	private void pDefense(string[] args)
	{
		S.I.batCtrl.currentPlayer.beingObj.defense = int.Parse(args[0]);
	}

	private void money(string[] args)
	{
		S.I.shopCtrl.sera = int.Parse(args[0]);
	}

	private void spawn(string[] args)
	{
		SpawnCtrl spCtrl = S.I.spCtrl;
		spCtrl.PlaceBeing(args[0], S.I.tiCtrl.mainBattleGrid.grid[int.Parse(args[1]), int.Parse(args[1])]);
	}

	private void goToZone(string[] args)
	{
		BC batCtrl = S.I.batCtrl;
		batCtrl.numBattlesLeft = 0;
		batCtrl.ti.mainBattleGrid.ClearField();
		RunCtrl runCtrl = S.I.runCtrl;
		runCtrl.currentRun.worldName = args[0];
		runCtrl.currentRun.zoneNum = int.Parse(args[1]);
		S.I.runCtrl.currentRun.worldName = args[0];
		S.I.runCtrl.StartZone(int.Parse(args[1]), null);
	}

	private void changeBG(string[] args)
	{
		if (args[0] == "move")
		{
			S.I.bgCtrl.MoveBG();
		}
		else
		{
			S.I.bgCtrl.ChangeBG(args[0]);
		}
	}

	private void addSpell(string[] args)
	{
		S.I.batCtrl.currentPlayer.duelDisk.AddLiveSpell(null, args[0], S.I.batCtrl.currentPlayer, false, false);
	}

	private void setAnimations(string[] args)
	{
		S.I.ANIMATIONS = bool.Parse(args[0]);
	}

	private void setCampaign(string[] args)
	{
		S.I.CAMPAIGN_MODE = bool.Parse(args[0]);
	}

	private void dev(string[] args)
	{
		appendLogLine("H:UI \nF:PostLoot \nG:PostLevel \nL:Logger \nR:Debug \nT:Autoplay \nU:Greenscreen \nP:Pause \n[:Step \n]:Step hold ");
	}

	private void transitionCheck(string[] args)
	{
		appendLogLine("PoCtrl: " + PostCtrl.transitioning + " btnCtrl: " + S.I.btnCtrl.transitioning);
	}

	private void clear(string[] args)
	{
		BC batCtrl = S.I.batCtrl;
		batCtrl.ti.mainBattleGrid.ClearField();
	}

	private void resetPrefs(string[] args)
	{
	}
}
