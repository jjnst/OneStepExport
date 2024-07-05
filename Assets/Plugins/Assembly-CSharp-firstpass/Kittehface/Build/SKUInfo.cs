using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace Kittehface.Build
{
	public abstract class SKUInfo : ScriptableObject, IXmlSerializable
	{
		public const string ASSET_PATH = "Assets/Resources/skuInfo.asset";

		public const string RESOURCE_NAME = "skuInfo";

		private XmlSchema schema = null;

		protected abstract string XmlSchemaName { get; }

		public static SKUInfo CreateFromXML(string xml)
		{
			if (string.IsNullOrEmpty(xml))
			{
				return null;
			}
			using (StringReader input = new StringReader(xml))
			{
				using (XmlReader xmlReader = XmlReader.Create(input))
				{
					while (xmlReader.NodeType != XmlNodeType.Element)
					{
						if (!xmlReader.Read())
						{
							throw new XmlException("XML was empty!");
						}
					}
					SKUInfo sKUInfo = null;
					switch (xmlReader.Name)
					{
					case "PS4SKUInfo":
						sKUInfo = ScriptableObject.CreateInstance<PS4SKUInfo>();
						break;
					case "XboxOneSKUInfo":
						sKUInfo = ScriptableObject.CreateInstance<XboxOneSKUInfo>();
						break;
					case "StandaloneSKUInfo":
						sKUInfo = ScriptableObject.CreateInstance<StandaloneSKUInfo>();
						break;
					case "SteamSKUInfo":
						sKUInfo = ScriptableObject.CreateInstance<SteamSKUInfo>();
						break;
					case "StandaloneEpicSKUInfo":
						sKUInfo = ScriptableObject.CreateInstance<StandaloneEpicSKUInfo>();
						break;
					case "StandaloneGOGSKUInfo":
						sKUInfo = ScriptableObject.CreateInstance<StandaloneGOGSKUInfo>();
						break;
					case "SwitchSKUInfo":
						sKUInfo = ScriptableObject.CreateInstance<SwitchSKUInfo>();
						break;
					case "AndroidSKUInfo":
						sKUInfo = ScriptableObject.CreateInstance<AndroidSKUInfo>();
						break;
					case "iOSSKUInfo":
						sKUInfo = ScriptableObject.CreateInstance<iOSSKUInfo>();
						break;
					case "tvOSSKUInfo":
						sKUInfo = ScriptableObject.CreateInstance<tvOSSKUInfo>();
						break;
					}
					if (sKUInfo != null)
					{
						sKUInfo.ReadXml(xmlReader);
						return sKUInfo;
					}
				}
			}
			return null;
		}

		public static SKUInfo CreateFromXMLFile(string xmlFilePath)
		{
			string xml = null;
			FileStream fileStream = null;
			try
			{
				fileStream = File.OpenRead(xmlFilePath);
				StreamReader streamReader = new StreamReader(fileStream);
				xml = streamReader.ReadToEnd();
				streamReader.Close();
				fileStream = null;
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to read SKU Info XML file [" + xmlFilePath + "]: " + ex);
			}
			finally
			{
				if (fileStream != null)
				{
					fileStream.Close();
					fileStream = null;
				}
			}
			return CreateFromXML(xml);
		}

		public XmlSchema GetSchema()
		{
			if (schema == null)
			{
				schema = new XmlSchema();
				schema.Id = XmlSchemaName;
			}
			return schema;
		}

		public void ReadXml(XmlReader reader)
		{
			throw new NotImplementedException("Xml reading not implemented outside the Unity Editor!");
		}

		public void WriteXml(XmlWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}
