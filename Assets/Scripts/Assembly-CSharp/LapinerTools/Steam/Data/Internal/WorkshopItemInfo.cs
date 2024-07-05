namespace LapinerTools.Steam.Data.Internal
{
	public class WorkshopItemInfo
	{
		public ulong PublishedFileId { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public string IconFileName { get; set; }

		public string[] Tags { get; set; }

		public int Priority { get; set; }

		public int ModVersion { get; set; }

		public string GameVersion { get; set; }
	}
}
