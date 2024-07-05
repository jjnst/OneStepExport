using System;
using Steamworks;
using UnityEngine;

namespace LapinerTools.Steam.Data
{
	[Serializable]
	public class WorkshopSortMode
	{
		[SerializeField]
		public EUGCQuery MODE = EUGCQuery.k_EUGCQuery_RankedByVote;

		[SerializeField]
		public EWorkshopSource SOURCE = EWorkshopSource.PUBLIC;

		public WorkshopSortMode()
		{
		}

		public WorkshopSortMode(EUGCQuery p_mode)
		{
			MODE = p_mode;
		}

		public WorkshopSortMode(EWorkshopSource p_source)
		{
			SOURCE = p_source;
		}

		public WorkshopSortMode(EUGCQuery p_mode, EWorkshopSource p_source)
		{
			MODE = p_mode;
			SOURCE = p_source;
		}

		public override bool Equals(object p_other)
		{
			return p_other != null && p_other is WorkshopSortMode && p_other.GetHashCode() == GetHashCode();
		}

		public override int GetHashCode()
		{
			return ((int)(MODE + 100 * (int)SOURCE)).GetHashCode();
		}
	}
}
