using System;

namespace Kittehface.Framework20
{
	public class RichPresence
	{
		private class RichPresenceImpl
		{
			public static RichPresenceImpl GetRichPresenceImpl()
			{
				throw new NotImplementedException("No RichPresenceImpl implementation exists for the requested platform.");
			}

			public virtual void Initialize()
			{
			}

			public virtual void SetRichPresenceMetadata(object metadata)
			{
			}

			public virtual void SetRichPresence(string key, Profiles.Profile profile)
			{
			}
		}

		private static RichPresenceImpl implementation;

		public static void Initialize()
		{
			implementation = RichPresenceImpl.GetRichPresenceImpl();
			implementation.Initialize();
		}

		public static void SetRichPresenceMetadata(object metadata)
		{
			if (implementation != null)
			{
				implementation.SetRichPresenceMetadata(metadata);
			}
		}

		public static void SetRichPresence(string key, Profiles.Profile profile)
		{
			if (implementation != null)
			{
				implementation.SetRichPresence(key, profile);
			}
		}
	}
}
