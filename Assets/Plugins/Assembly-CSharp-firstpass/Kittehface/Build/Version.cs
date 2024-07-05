using System;
using System.IO;
using UnityEngine;

namespace Kittehface.Build
{
	public class Version
	{
		private const bool Editor = false;

		public const string Platform = "Windows";

		public const string Filename = "Version.txt";

		public const string DefaultValue = "Unknown";

		[Obsolete("Backing value for Version.Revision, no not access directly.")]
		private static string _Revision = null;

		public static string Revision
		{
			get
			{
				if (_Revision == null)
				{
					_Revision = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Version.txt")).Trim();
				}
				return _Revision;
			}
		}
	}
}
