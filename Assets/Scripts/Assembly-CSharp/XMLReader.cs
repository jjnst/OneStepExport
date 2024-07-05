using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEngine;

public class XMLReader : MonoBehaviour
{
	private BC ctrl;

	private RunCtrl runCtrl;

	private SpawnCtrl spCtrl;

	public int width;

	public int height;

	public int prevSetNum = -9999;

	private void Awake()
	{
		ctrl = S.I.batCtrl;
		runCtrl = S.I.runCtrl;
		spCtrl = S.I.spCtrl;
	}

	public string GetDataFile(string fileName)
	{
		string path = Path.Combine(Application.streamingAssetsPath, "Data");
		path = Path.Combine(path, fileName);
		return File.ReadAllText(path);
	}

	public string GetFile(string filePath, string fileName)
	{
		filePath = Path.Combine(filePath, fileName);
		return File.ReadAllText(filePath);
	}

	public string GetDataFilePath(string fileName)
	{
		string path = Path.Combine(Application.streamingAssetsPath, "Data");
		return Path.Combine(path, fileName);
	}

	public string GetFilePath(string filePath, string fileName)
	{
		filePath = Path.Combine(filePath, fileName);
		return filePath;
	}

	public void XMLtoBattlesData(string theBattleName)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(GetDataFile("Battles.xml"));
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("battle");
		foreach (XmlNode item in elementsByTagName)
		{
			string text = "";
			string bgName = "";
			foreach (XmlAttribute attribute in item.Attributes)
			{
				if (attribute.Name == "name")
				{
					text = attribute.Value;
				}
				if (attribute.Name == "background")
				{
					bgName = attribute.Value;
				}
			}
			if (!(text == theBattleName))
			{
				continue;
			}
			XmlNodeList childNodes = item.ChildNodes;
			S.I.bgCtrl.ChangeBG(bgName);
			int num = 0;
			spCtrl.enemyToSpawn.Clear();
			spCtrl.enemySpawnX.Clear();
			spCtrl.enemySpawnY.Clear();
			{
				foreach (XmlNode item2 in childNodes)
				{
					spCtrl.enemyToSpawn.Add(item2.InnerText);
					foreach (XmlAttribute attribute2 in item2.Attributes)
					{
						if (attribute2.Name == "xpos")
						{
							spCtrl.enemySpawnX.Add(int.Parse(attribute2.Value));
						}
						else if (attribute2.Name == "ypos")
						{
							spCtrl.enemySpawnY.Add(int.Parse(attribute2.Value));
						}
					}
					num++;
				}
				break;
			}
		}
	}

	public int XMLtoGetWorlds(string data)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(data);
		int num = 0;
		int id = 0;
		if (runCtrl.worlds == null)
		{
			runCtrl.worlds = new Dictionary<string, World>();
		}
		foreach (XmlNode item in xmlDocument.GetElementsByTagName("world"))
		{
			bool flag = false;
			foreach (XmlAttribute attribute in item.Attributes)
			{
				if (attribute.Name == "num")
				{
					id = int.Parse(attribute.Value);
				}
				if (S.I.ONEWORLD)
				{
					if (attribute.Value == "Violette" || attribute.Value == "Saffron" || attribute.Value == "Gunner" || attribute.Value == "Reva" || attribute.Value == "Terra" || attribute.Value == "Hazel")
					{
						flag = true;
					}
				}
				else if (S.I.BETA && attribute.Name == "name" && (attribute.Value == "Violette" || attribute.Value == "Saffron"))
				{
					flag = true;
				}
			}
			if (flag)
			{
				continue;
			}
			num++;
			World world = new World();
			foreach (XmlAttribute attribute2 in item.Attributes)
			{
				world.id = id;
				if (attribute2.Name == "name")
				{
					world.nameString = attribute2.Value;
				}
				if (attribute2.Name == "background")
				{
					world.background = attribute2.Value;
				}
				if (attribute2.Name == "icon")
				{
					world.iconName = attribute2.Value;
				}
				if (attribute2.Name == "transition")
				{
					world.transition = attribute2.Value;
				}
				if (!(attribute2.Name == "tags"))
				{
					continue;
				}
				world.tagStrings = attribute2.Value.Replace(" ", string.Empty).Split(',');
				string[] tagStrings = world.tagStrings;
				foreach (string text in tagStrings)
				{
					if (Enum.IsDefined(typeof(Tag), text))
					{
						world.tags.Add((Tag)Enum.Parse(typeof(Tag), text));
					}
					else
					{
						Debug.LogWarning("Invalid Tag: " + text + " for " + world.nameString);
					}
				}
			}
			world.node = item;
			world.numZones = S.I.zonesPerWorld;
			runCtrl.worlds[world.nameString] = world;
		}
		return num;
	}

	public void XMLtoZoneData(World worldn, int stageNum, ZoneType zoneType = ZoneType.Normal)
	{
		XmlNodeList childNodes = worldn.node.ChildNodes;
		int num = 0;
		bool flag = false;
		stageNum = Mathf.Clamp(stageNum, 0, childNodes.Count - 2);
		foreach (XmlNode item in childNodes)
		{
			if (item.NodeType == XmlNodeType.Comment)
			{
				continue;
			}
			if (zoneType == ZoneType.Boss || zoneType == ZoneType.Miniboss)
			{
				foreach (XmlAttribute attribute in item.Attributes)
				{
					if (attribute.Name == "description")
					{
						if (attribute.Value == zoneType.ToString())
						{
							flag = true;
						}
					}
					else if (attribute.Name == "background")
					{
						runCtrl.zoneBackground = attribute.Value;
					}
				}
			}
			else
			{
				foreach (XmlAttribute attribute2 in item.Attributes)
				{
					if (attribute2.Name == "num")
					{
						if (int.Parse(attribute2.Value) == stageNum)
						{
							flag = true;
						}
						else if (num == worldn.numStages - 1)
						{
							flag = true;
						}
						else
						{
							num++;
						}
					}
				}
			}
			if (!flag)
			{
				continue;
			}
			XmlNodeList childNodes2 = item.ChildNodes;
			int num2 = 0;
			int num3 = runCtrl.NextPsuedoRand(0, childNodes2.Count);
			if (childNodes2.Count > 1)
			{
				while (num3 == prevSetNum)
				{
					if (childNodes2.Count < 2)
					{
						num3 = 0;
						prevSetNum = -9999;
					}
					else
					{
						num3 = runCtrl.NextPsuedoRand(0, childNodes2.Count);
					}
				}
			}
			{
				foreach (XmlNode item2 in childNodes2)
				{
					if (item2.NodeType != XmlNodeType.Comment && num2 == num3)
					{
						prevSetNum = num2;
						XmlNodeList childNodes3 = item2.ChildNodes;
						foreach (XmlAttribute attribute3 in item2.Attributes)
						{
							if (attribute3.Name == "battles")
							{
								ctrl.numBattles = int.Parse(attribute3.Value);
							}
							if (attribute3.Name == "spawn")
							{
								spCtrl.numEnemiesToSpawn = int.Parse(attribute3.Value);
							}
						}
						spCtrl.enemyToSpawn.Clear();
						spCtrl.bossesToSpawn.Clear();
						spCtrl.enemySpawnX.Clear();
						spCtrl.enemySpawnY.Clear();
						spCtrl.enemyChance.Clear();
						{
							foreach (XmlNode item3 in childNodes3)
							{
								string text = "";
								foreach (XmlAttribute attribute4 in item3.Attributes)
								{
									if (attribute4.Name == "name")
									{
										text = attribute4.Value;
									}
								}
								if (text.StartsWith("Boss"))
								{
									spCtrl.bossesToSpawn.Add(text);
									if (!spCtrl.bossDictionary.ContainsKey(text))
									{
										Debug.LogError("No BossID for " + text);
									}
									else
									{
										Debug.Log(text);
									}
									continue;
								}
								bool flag2 = false;
								spCtrl.enemyToSpawn.Add(text);
								if (!spCtrl.beingDictionary.ContainsKey(text))
								{
									Debug.LogError("No BeingID for " + text);
								}
								if (!flag2)
								{
								}
								foreach (XmlAttribute attribute5 in item3.Attributes)
								{
									if (attribute5.Name == "amount")
									{
										spCtrl.enemyChance.Add(int.Parse(attribute5.Value));
									}
									if (attribute5.Name == "xpos")
									{
										spCtrl.enemySpawnX.Add(int.Parse(attribute5.Value));
									}
									else if (attribute5.Name == "ypos")
									{
										spCtrl.enemySpawnY.Add(int.Parse(attribute5.Value));
									}
								}
							}
							break;
						}
					}
					num2++;
				}
				break;
			}
		}
	}

	public IEnumerator XMLtoTestSceneData()
	{
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(GetDataFile("TestScene.xml"));
		foreach (XmlNode parameters in xmlDoc.GetElementsByTagName("parameters"))
		{
			foreach (XmlNode param in parameters.ChildNodes)
			{
				switch (param.Name)
				{
				case "useTestSceneData":
					if (bool.Parse(param.InnerText))
					{
						S.I.EDITION = Edition.Dev;
						S.I.CAMPAIGN_MODE = false;
						yield return spCtrl.StartCoroutine(spCtrl._LoadHeroObjects());
						Debug.Log("using test scene data from XML");
						break;
					}
					yield break;
				case "installModsOnStart":
					if (bool.Parse(param.InnerText))
					{
						while (!SteamManager.Initialized && Time.timeSinceLevelLoad < S.maxLoadTime)
						{
							yield return null;
						}
						S.I.modCtrl.LoadItems();
						yield return S.I.modCtrl.StartCoroutine(S.I.modCtrl._InstallMods());
					}
					break;
				case "beingID":
					runCtrl.defaultHeroString = param.InnerText;
					break;
				case "startingUpgrades":
					ctrl.deCtrl.deckScreen.startingUpgraders = int.Parse(param.InnerText);
					break;
				case "worldName":
					runCtrl.testWorldName = param.InnerText;
					break;
				case "worldTier":
					runCtrl.testWorldTierNum = int.Parse(param.InnerText);
					break;
				case "zoneNum":
					runCtrl.testZoneNum = int.Parse(param.InnerText);
					break;
				case "zoneType":
					if (Enum.IsDefined(typeof(ZoneType), param.InnerText))
					{
						S.I.testZoneType = (ZoneType)Enum.Parse(typeof(ZoneType), param.InnerText);
					}
					else
					{
						Debug.LogError("Zone Type " + param.InnerText + " not found.");
					}
					break;
				case "gScene":
					if (Enum.IsDefined(typeof(GScene), param.InnerText))
					{
						S.I.scene = (GScene)Enum.Parse(typeof(GScene), param.InnerText);
					}
					else
					{
						Debug.LogError("gScene " + param.InnerText + " not found.");
					}
					break;
				case "zonesPerWorld":
					S.I.zonesPerWorld = int.Parse(param.InnerText);
					break;
				case "devTools":
					S.I.DEVELOPER_TOOLS = bool.Parse(param.InnerText);
					break;
				case "console":
					S.I.CONSOLE = bool.Parse(param.InnerText);
					break;
				case "matchDeckOrder":
					S.I.MATCH_DECK_ORDER = bool.Parse(param.InnerText);
					break;
				case "animations":
					S.I.ANIMATIONS = bool.Parse(param.InnerText);
					break;
				}
			}
		}
		spCtrl.SetDefaultHeroes();
	}

	public void XMLtoTileFieldData(int fieldNum)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(GetDataFile("TileFields.xml"));
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("field");
		int num = 0;
		foreach (XmlNode item in elementsByTagName)
		{
			if (num == fieldNum)
			{
				foreach (XmlAttribute attribute in item.Attributes)
				{
					if (attribute.Name == "width")
					{
						width = int.Parse(attribute.Value);
					}
					else if (attribute.Name == "height")
					{
						height = int.Parse(attribute.Value);
					}
				}
				string text = item.InnerText.Replace(" ", "").Replace("\n", "").Replace("\t", "")
					.Replace("\r", "");
				int num2 = 0;
				for (int num3 = height - 1; num3 >= 0; num3--)
				{
					for (int i = 0; i < width; i++)
					{
						TI.fieldStringGrid[i, num3] = text[num2].ToString();
						num2++;
					}
				}
				break;
			}
			num++;
		}
	}

	public float XMLToModPriority(string modsDir, string fileName)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(GetFile(modsDir, fileName));
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("WorkshopItemInfo");
		float result = 0f;
		foreach (XmlNode item in elementsByTagName)
		{
			XmlNodeList childNodes = item.ChildNodes;
			foreach (XmlNode item2 in childNodes)
			{
				if (item2.Name == "Priority")
				{
					float.TryParse(item2.InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
				}
			}
		}
		return result;
	}

	public IEnumerator XMLtoAnimationInfo(string modsDir, string fileName)
	{
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.LoadXml(GetFile(modsDir, fileName));
		XmlNodeList animationList = xmlDoc.GetElementsByTagName("Animation");
		yield return null;
		foreach (XmlNode animation in animationList)
		{
			string name = "";
			bool loop = false;
			List<Sprite> sprites = new List<Sprite>();
			float keyFrameLength = 0f;
			foreach (XmlAttribute animAtt in animation.Attributes)
			{
				if (animAtt.Name == "loop")
				{
					if (!bool.TryParse(animAtt.Value, out loop))
					{
						loop = false;
					}
				}
				else if (animAtt.Name == "name")
				{
					name = animAtt.Value;
				}
				else if (animAtt.Name == "frameDuration")
				{
					keyFrameLength = float.Parse(animAtt.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
				}
			}
			XmlNodeList frameList = animation.ChildNodes;
			foreach (XmlNode frame in frameList)
			{
				foreach (XmlAttribute frameAtt in frame.Attributes)
				{
					if (frameAtt.Name == "image")
					{
						string theFilePath = Path.Combine(modsDir, frameAtt.Value);
						sprites.Add(S.I.modCtrl.LoadNewSprite(theFilePath));
					}
				}
			}
			Debug.Log("Creating SprClip Mod " + name);
			S.I.itemMan.CreateAnimClip(name, loop, keyFrameLength, sprites.ToArray());
		}
	}
}
