using System;
using System.Collections.Generic;
using System.Globalization;
using Steamworks;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.Impl;

namespace Kittehface.Framework20
{
	public class Achievements
	{
		public delegate void AchievementsLoaded(Profiles.Profile profile, bool success);

		public delegate void AchievementUnlocked(Profiles.Profile profile, IAchievement achievement);

		public delegate void AchievementRetrieved(Profiles.Profile profile, IAchievement achievement);

		private class AchievementsImpl
		{
			public static AchievementsImpl GetAchievementsImpl()
			{
				return new SteamAchievementsImpl();
			}

			public virtual void Initialize()
			{
			}

			public virtual void SetAchievementsMetadata(object metadata)
			{
			}

			public virtual void SetAchievementsFile(Profiles.Profile profile, UserData.File file)
			{
			}

			public virtual void LoadAchievements(Profiles.Profile profile, List<AchievementData> achievementData)
			{
				if (Achievements.OnAchievementsLoaded != null)
				{
					Achievements.OnAchievementsLoaded(profile, true);
				}
			}

			public virtual void UnlockAchievement(Profiles.Profile profile, string achievementID)
			{
			}

			public virtual void GetAchievement(Profiles.Profile profile, string achievementID)
			{
				if (Achievements.OnAchievementRetrieved != null)
				{
					Achievements.OnAchievementRetrieved(profile, null);
				}
			}

			public virtual void Destroy()
			{
			}
		}

		private class AchievementsImpl_Default
		{
			private const string ACHIEVEMENT_UNLOCK_USERDATA_KEY_FORMAT = "AchievementUnlock_{0}";

			private const string ACHIEVEMENT_UNLOCKTIME_USERDATA_KEY_FORMAT = "AchievementUnlockTime_{0}";

			private Dictionary<Profiles.Profile, DefaultAchievementsData> defaultAchievementsData = new Dictionary<Profiles.Profile, DefaultAchievementsData>();

			private bool pendingUpdates = false;

			public AchievementsImpl_Default()
			{
				Profiles.OnDeactivated += Profiles_OnDeactivated;
			}

			public void SetAchievementsFile(Profiles.Profile profile, UserData.File file)
			{
				Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.SetAchievementsFile: " + ((file != null) ? file.filename : "(null)"));
				DefaultAchievementsData achievementsData = GetAchievementsData(profile);
				achievementsData.file = file;
			}

			public void LoadAchievements(Profiles.Profile profile, List<AchievementData> achievementData)
			{
				Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.LoadAchievements: Loading [" + ((achievementData != null) ? achievementData.Count : 0) + "] achievements");
				DefaultAchievementsData achievementsData = GetAchievementsData(profile);
				achievementsData.achievementData.Clear();
				achievementsData.achievements.Clear();
				achievementsData.requestLoad = true;
				if (achievementsData.file == null)
				{
					Log(VerbosityLevel.Warning, "Achievements.AchievementsImpl_Default.LoadAchievements: no save file set for storing achievements!");
				}
				if (achievementData != null)
				{
					int num = 0;
					foreach (AchievementData achievementDatum in achievementData)
					{
						if (achievementDatum != null)
						{
							achievementsData.achievementData.Add(achievementDatum);
						}
						else
						{
							num++;
						}
					}
					if (num > 0)
					{
						Log(VerbosityLevel.Warning, "Achievements.AchievementsImpl_Default.LoadAchievements: skipped " + num + " null achievements while loading.");
					}
				}
				ScheduleUpdate();
			}

			public void UnlockAchievement(Profiles.Profile profile, string achievementID)
			{
				Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.UnlockAchievement: " + achievementID);
				DefaultAchievementsData achievementsData = GetAchievementsData(profile);
				foreach (Achievement achievement in achievementsData.achievements)
				{
					if (achievement.id == achievementID)
					{
						if (achievement.completed)
						{
							Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.UnlockAchievement: " + achievementID + " already unlocked (don't send result event)");
							return;
						}
						break;
					}
				}
				if (!achievementsData.requestUnlockAchievementIDs.Contains(achievementID))
				{
					achievementsData.requestUnlockAchievementIDs.Add(achievementID);
					ScheduleUpdate();
				}
			}

			public void GetAchievement(Profiles.Profile profile, string achievementID)
			{
				Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.GetAchievement: " + achievementID);
				DefaultAchievementsData achievementsData = GetAchievementsData(profile);
				if (!achievementsData.requestGetAchievementIDs.Contains(achievementID))
				{
					achievementsData.requestGetAchievementIDs.Add(achievementID);
					ScheduleUpdate();
				}
			}

			public void Destroy()
			{
				Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.Destroy");
				defaultAchievementsData.Clear();
			}

			private void ScheduleUpdate()
			{
				if (!pendingUpdates)
				{
					Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ScheduleUpdate: Scheduling new update");
					pendingUpdates = true;
					Platform.OnUpdate += OnUpdate;
				}
			}

			private DefaultAchievementsData GetAchievementsData(Profiles.Profile profile)
			{
				if (this.defaultAchievementsData.ContainsKey(profile))
				{
					return this.defaultAchievementsData[profile];
				}
				DefaultAchievementsData defaultAchievementsData = new DefaultAchievementsData();
				this.defaultAchievementsData.Add(profile, defaultAchievementsData);
				return defaultAchievementsData;
			}

			private void OnUpdate()
			{
				bool flag = false;
				List<Profiles.Profile> list = null;
				foreach (KeyValuePair<Profiles.Profile, DefaultAchievementsData> defaultAchievementsDatum in defaultAchievementsData)
				{
					if (!Profiles.ActiveProfiles.Contains(defaultAchievementsDatum.Key))
					{
						if (list == null)
						{
							list = new List<Profiles.Profile>();
						}
						list.Add(defaultAchievementsDatum.Key);
						flag = true;
					}
					if (!flag)
					{
						if (defaultAchievementsDatum.Value.requestLoad)
						{
							defaultAchievementsDatum.Value.requestLoad = false;
							ExecuteLoadAchievements(defaultAchievementsDatum.Key, defaultAchievementsDatum.Value);
							flag = true;
							break;
						}
						if (defaultAchievementsDatum.Value.requestUnlockAchievementIDs.Count > 0)
						{
							string[] achievementIDs = defaultAchievementsDatum.Value.requestUnlockAchievementIDs.ToArray();
							defaultAchievementsDatum.Value.requestUnlockAchievementIDs.Clear();
							ExecuteUnlockAchievements(defaultAchievementsDatum.Key, defaultAchievementsDatum.Value, achievementIDs);
							flag = true;
							break;
						}
						if (defaultAchievementsDatum.Value.requestGetAchievementIDs.Count > 0)
						{
							string[] achievementIDs2 = defaultAchievementsDatum.Value.requestGetAchievementIDs.ToArray();
							defaultAchievementsDatum.Value.requestGetAchievementIDs.Clear();
							ExecuteGetAchievements(defaultAchievementsDatum.Key, defaultAchievementsDatum.Value, achievementIDs2);
							flag = true;
							break;
						}
					}
				}
				if (list != null)
				{
					foreach (Profiles.Profile item in list)
					{
						defaultAchievementsData.Remove(item);
					}
				}
				if (!flag)
				{
					Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.OnUpdate: Unsubscribe from updates");
					pendingUpdates = false;
					Platform.OnUpdate -= OnUpdate;
				}
			}

			private void ExecuteLoadAchievements(Profiles.Profile profile, DefaultAchievementsData data)
			{
				Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ExecuteLoadAchievements");
				if (data.file != null)
				{
					foreach (AchievementData achievementDatum in data.achievementData)
					{
						string key = string.Format(CultureInfo.InvariantCulture, "AchievementUnlock_{0}", achievementDatum.AchievementName);
						string key2 = string.Format(CultureInfo.InvariantCulture, "AchievementUnlockTime_{0}", achievementDatum.AchievementName);
						bool flag = data.file.Get(key, false);
						DateTime lastReportedDate = data.file.Get<DateTime>(key2);
						Achievement achievement = new Achievement(achievementDatum.AchievementName, flag ? 1.0 : 0.0, flag, achievementDatum.Hidden, lastReportedDate);
						data.achievements.Add(achievement);
						Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ExecuteLoadAchievements: Add achievement [" + achievement.id + "], complete [" + achievement.completed + "], time [" + achievement.lastReportedDate.ToLocalTime().ToString() + "]");
					}
					if (Achievements.OnAchievementsLoaded != null)
					{
						Achievements.OnAchievementsLoaded(profile, true);
					}
					return;
				}
				foreach (AchievementData achievementDatum2 in data.achievementData)
				{
					Achievement achievement2 = new Achievement(achievementDatum2.AchievementName, 0.0, false, achievementDatum2.Hidden, DateTime.MinValue);
					data.achievements.Add(achievement2);
					Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ExecuteLoadAchievements: No save data, add achievement [" + achievement2.id + "]");
				}
				if (Achievements.OnAchievementsLoaded != null)
				{
					Achievements.OnAchievementsLoaded(profile, false);
				}
			}

			private void ExecuteUnlockAchievements(Profiles.Profile profile, DefaultAchievementsData data, string[] achievementIDs)
			{
				bool flag = false;
				Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ExecuteUnlockAchievements: [" + ((achievementIDs != null) ? achievementIDs.Length : 0) + "] achievements");
				foreach (string text in achievementIDs)
				{
					bool flag2 = false;
					foreach (Achievement achievement in data.achievements)
					{
						if (!(achievement.id == text))
						{
							continue;
						}
						flag2 = true;
						if (!achievement.completed)
						{
							achievement.SetCompleted(true);
							achievement.percentCompleted = 1.0;
							achievement.SetLastReportedDate(DateTime.UtcNow);
							if (data.file != null)
							{
								string key = string.Format(CultureInfo.InvariantCulture, "AchievementUnlock_{0}", text);
								string key2 = string.Format(CultureInfo.InvariantCulture, "AchievementUnlockTime_{0}", text);
								data.file.Set(key, true, UserData.WriteMode.Deferred);
								data.file.Set(key2, achievement.lastReportedDate, UserData.WriteMode.Deferred);
								flag = true;
							}
							Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ExecuteUnlockAchievements: Unlocked [" + text + "]");
							if (Achievements.OnAchievementUnlocked != null)
							{
								Achievements.OnAchievementUnlocked(profile, achievement);
							}
						}
						else
						{
							Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ExecuteUnlockAchievements: [" + text + "] already unlocked internal (don't send event)");
						}
						break;
					}
					if (!flag2)
					{
						Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ExecuteUnlockAchievements: Failed to unlock [" + text + "] (don't send event)");
					}
				}
				if (flag && data.file != null)
				{
					data.file.Write();
				}
			}

			private void ExecuteGetAchievements(Profiles.Profile profile, DefaultAchievementsData data, string[] achievementIDs)
			{
				Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ExecuteGetAchievements");
				if (Achievements.OnAchievementRetrieved == null)
				{
					return;
				}
				foreach (string text in achievementIDs)
				{
					bool flag = false;
					foreach (Achievement achievement in data.achievements)
					{
						if (achievement.id == text)
						{
							flag = true;
							Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ExecuteGetAchievements: Got [" + text + "]");
							Achievements.OnAchievementRetrieved(profile, achievement);
							break;
						}
					}
					if (!flag)
					{
						Log(VerbosityLevel.Info, "Achievements.AchievementsImpl_Default.ExecuteGetAchievements: Failed to get [" + text + "]");
						Achievements.OnAchievementRetrieved(profile, new Achievement(text, 0.0));
					}
				}
			}

			private void Profiles_OnDeactivated(Profiles.Profile profile)
			{
				if (defaultAchievementsData.ContainsKey(profile))
				{
					defaultAchievementsData.Remove(profile);
				}
			}
		}

		private class DefaultAchievementsData
		{
			public List<AchievementData> achievementData = new List<AchievementData>();

			public List<Achievement> achievements = new List<Achievement>();

			public UserData.File file;

			public bool requestLoad = false;

			public List<string> requestUnlockAchievementIDs = new List<string>();

			public List<string> requestGetAchievementIDs = new List<string>();
		}

		private class SteamAchievementsImpl : AchievementsImpl
		{
			private AchievementsImpl_Default defaultImplementation;

			private Dictionary<Profiles.Profile, SteamAchievementsData> achievementManagerData = new Dictionary<Profiles.Profile, SteamAchievementsData>();

			private bool pendingUpdates = false;

			private bool updateInProgress = false;

			private CGameID gameID;

			private bool gameIDSet = false;

			private bool requestStoreUserStats = false;

			private Callback<UserStatsReceived_t> steamUserStatsReceivedCallback = null;

			private Callback<UserAchievementStored_t> steamUserAchievementStoredCallback = null;

			public override void Initialize()
			{
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.Initialize");
				Profiles.OnDeactivated += Profiles_OnDeactivated;
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					defaultImplementation = new AchievementsImpl_Default();
				}
			}

			public override void SetAchievementsMetadata(object metadata)
			{
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.SetAchievementsMetadata");
				if (metadata != null && metadata is ulong)
				{
					gameID = new CGameID((ulong)metadata);
					gameIDSet = true;
				}
			}

			public override void SetAchievementsFile(Profiles.Profile profile, UserData.File file)
			{
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.SetAchievementsFile");
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					defaultImplementation.SetAchievementsFile(profile, file);
				}
			}

			public override void LoadAchievements(Profiles.Profile profile, List<AchievementData> achievementData)
			{
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.LoadAchievements: [" + ((achievementData != null) ? achievementData.Count : 0) + "] achievements");
				if (!gameIDSet)
				{
					Log(VerbosityLevel.Error, "Achievements.SteamAchievementsImpl.LoadAchievements: Steam game ID not set via SetAchievementsMetadata!");
				}
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					defaultImplementation.LoadAchievements(profile, achievementData);
					return;
				}
				SteamAchievementsData achievementsData = GetAchievementsData(profile);
				achievementsData.achievementData.Clear();
				achievementsData.achievements.Clear();
				achievementsData.requestLoad = true;
				if (achievementData != null)
				{
					int num = 0;
					foreach (AchievementData achievementDatum in achievementData)
					{
						if (achievementDatum != null)
						{
							achievementsData.achievementData.Add(achievementDatum);
						}
						else
						{
							num++;
						}
					}
					if (num > 0)
					{
						Log(VerbosityLevel.Warning, "Achievements.SteamAchievementsImpl.LoadAchievements: skipped " + num + " null achievements while loading.");
					}
				}
				ScheduleUpdate();
			}

			public override void UnlockAchievement(Profiles.Profile profile, string achievementID)
			{
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.UnlockAchievement: " + achievementID);
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					defaultImplementation.UnlockAchievement(profile, achievementID);
					return;
				}
				SteamAchievementsData achievementsData = GetAchievementsData(profile);
				foreach (SteamAchievement achievement in achievementsData.achievements)
				{
					if (achievement.id == achievementID)
					{
						if (achievement.completed)
						{
							Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.UnlockAchievement: Already unlocked " + achievementID + " (don't send event)");
							return;
						}
						break;
					}
				}
				if (!achievementsData.requestUnlockAchievementIDs.Contains(achievementID))
				{
					Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.UnlockAchievement: Request unlock " + achievementID);
					achievementsData.requestUnlockAchievementIDs.Add(achievementID);
					ScheduleUpdate();
				}
			}

			public override void GetAchievement(Profiles.Profile profile, string achievementID)
			{
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.GetAchievement: " + achievementID);
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					defaultImplementation.GetAchievement(profile, achievementID);
					return;
				}
				SteamAchievementsData achievementsData = GetAchievementsData(profile);
				if (!achievementsData.requestGetAchievementIDs.Contains(achievementID))
				{
					Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.GetAchievement: Request get " + achievementID);
					achievementsData.requestGetAchievementIDs.Add(achievementID);
					ScheduleUpdate();
				}
			}

			public override void Destroy()
			{
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.Destroy");
				if (Platform.isEditor && !Platform.RunSteamInEditor)
				{
					defaultImplementation.Destroy();
				}
			}

			private void ScheduleUpdate()
			{
				if (!pendingUpdates)
				{
					Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.ScheduleUpdate: Scheduling new update");
					pendingUpdates = true;
					Platform.OnUpdate += OnUpdate;
				}
			}

			private SteamAchievementsData GetAchievementsData(Profiles.Profile profile)
			{
				if (achievementManagerData.ContainsKey(profile))
				{
					return achievementManagerData[profile];
				}
				SteamAchievementsData steamAchievementsData = new SteamAchievementsData();
				achievementManagerData.Add(profile, steamAchievementsData);
				return steamAchievementsData;
			}

			private void OnUpdate()
			{
				bool flag = false;
				bool flag2 = false;
				List<Profiles.Profile> list = null;
				if (!updateInProgress)
				{
					foreach (KeyValuePair<Profiles.Profile, SteamAchievementsData> achievementManagerDatum in achievementManagerData)
					{
						if (!Profiles.ActiveProfiles.Contains(achievementManagerDatum.Key))
						{
							if (list == null)
							{
								list = new List<Profiles.Profile>();
							}
							list.Add(achievementManagerDatum.Key);
							flag = true;
						}
						if (flag)
						{
							continue;
						}
						if (achievementManagerDatum.Value.requestLoad)
						{
							achievementManagerDatum.Value.requestLoad = false;
							flag = true;
							if (Platform.SteamInitialized && SteamUser.BLoggedOn())
							{
								updateInProgress = true;
								if (steamUserStatsReceivedCallback == null)
								{
									steamUserStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(SteamUserStats_OnUserStatsReceived);
								}
								SteamUserStats.RequestCurrentStats();
							}
							else
							{
								Log(VerbosityLevel.Warning, "Achievements.SteamAchievementsImpl.OnUpdate: Achievements load failed because user is not logged in (Steam may be offline)");
								if (Achievements.OnAchievementsLoaded != null)
								{
									Achievements.OnAchievementsLoaded(achievementManagerDatum.Key, false);
								}
							}
							break;
						}
						if (achievementManagerDatum.Value.requestUnlockAchievementIDs.Count > 0)
						{
							flag = true;
							updateInProgress = true;
							flag2 = true;
							string[] achievementIDs = achievementManagerDatum.Value.requestUnlockAchievementIDs.ToArray();
							achievementManagerDatum.Value.requestUnlockAchievementIDs.Clear();
							if (Platform.SteamInitialized)
							{
								ExecuteUnlockAchievements(achievementManagerDatum.Key, achievementManagerDatum.Value, achievementIDs);
								break;
							}
							Log(VerbosityLevel.Warning, "Achievements.SteamAchievementsImpl.OnUpdate: Achievements unlock failed because Steam is not initialized");
							updateInProgress = false;
							break;
						}
						if (achievementManagerDatum.Value.requestGetAchievementIDs.Count <= 0)
						{
							continue;
						}
						flag = true;
						updateInProgress = true;
						string[] array = achievementManagerDatum.Value.requestGetAchievementIDs.ToArray();
						achievementManagerDatum.Value.requestGetAchievementIDs.Clear();
						if (Platform.SteamInitialized)
						{
							ExecuteGetAchievements(achievementManagerDatum.Key, achievementManagerDatum.Value, array);
							break;
						}
						Log(VerbosityLevel.Warning, "Achievements.SteamAchievementsImpl.OnUpdate: Get Achievements failed because Steam is not initialized");
						updateInProgress = false;
						if (Achievements.OnAchievementRetrieved != null)
						{
							for (int i = 0; i < array.Length; i++)
							{
								Achievements.OnAchievementRetrieved(achievementManagerDatum.Key, new SteamAchievement(array[i], "", false, 0.0, false, DateTime.MinValue));
							}
						}
						break;
					}
				}
				if (list != null)
				{
					foreach (Profiles.Profile item in list)
					{
						achievementManagerData.Remove(item);
					}
				}
				if (requestStoreUserStats && !flag2)
				{
					requestStoreUserStats = !SteamUserStats.StoreStats();
					Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.OnUpdate: StoreStats success [" + !requestStoreUserStats + "]");
				}
				if (!flag && !updateInProgress && !requestStoreUserStats)
				{
					Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.OnUpdate: Unsubscribe from updates");
					pendingUpdates = false;
					Platform.OnUpdate -= OnUpdate;
				}
			}

			private void ExecuteUnlockAchievements(Profiles.Profile profile, SteamAchievementsData data, string[] achievementIDs)
			{
				bool flag = false;
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.ExecuteUnlockAchievements: [" + ((achievementIDs != null) ? achievementIDs.Length : 0) + "] achievements");
				if (steamUserAchievementStoredCallback == null)
				{
					steamUserAchievementStoredCallback = Callback<UserAchievementStored_t>.Create(SteamUserStats_OnAchievementStored);
				}
				if (achievementIDs != null)
				{
					foreach (string text in achievementIDs)
					{
						foreach (AchievementData achievementDatum in data.achievementData)
						{
							if (!(achievementDatum.AchievementName == text))
							{
								continue;
							}
							if (!string.IsNullOrEmpty(achievementDatum.SteamID))
							{
								SteamUserStats.SetAchievement(achievementDatum.SteamID);
								SteamAchievement steamAchievement = null;
								foreach (SteamAchievement achievement in data.achievements)
								{
									if (achievement.steamID == achievementDatum.SteamID)
									{
										steamAchievement = achievement;
										steamAchievement.completed = true;
										steamAchievement.percentCompleted = 1.0;
										steamAchievement.lastReportedDate = DateTime.UtcNow;
										break;
									}
								}
								if (steamAchievement == null)
								{
									steamAchievement = new SteamAchievement(achievementDatum.AchievementName, achievementDatum.SteamID, true, 1.0, achievementDatum.Hidden, DateTime.UtcNow);
									data.achievements.Add(steamAchievement);
								}
								Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.ExecuteUnlockAchievements: SteamUserStats.SetAchievement [" + achievementDatum.AchievementName + "] (" + achievementDatum.SteamID + ")");
								flag = true;
							}
							else
							{
								Log(VerbosityLevel.Error, "Achievements.SteamAchievementsImple.ExecuteUnlockAchievements: " + achievementDatum.AchievementName + "] failed due to invalid SteamID (don't sent event)");
							}
							break;
						}
					}
				}
				if (flag)
				{
					requestStoreUserStats = !SteamUserStats.StoreStats();
					return;
				}
				steamUserAchievementStoredCallback.Dispose();
				steamUserAchievementStoredCallback = null;
				if (achievementIDs != null)
				{
					foreach (string text2 in achievementIDs)
					{
						SteamAchievement steamAchievement2 = null;
						foreach (SteamAchievement achievement2 in data.achievements)
						{
							if (achievement2.id == text2)
							{
								steamAchievement2 = achievement2;
								Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.ExecuteUnlockAchievements: Failed to unlock [" + steamAchievement2.id + "] (" + steamAchievement2.steamID + ") (don't send event)");
								break;
							}
						}
						if (steamAchievement2 == null)
						{
							Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.ExecuteUnlockAchievements: Failed to unlock [" + text2 + "] (invalid achievement, don't send event)");
						}
					}
				}
				updateInProgress = false;
			}

			private void ExecuteGetAchievements(Profiles.Profile profile, SteamAchievementsData data, string[] achievementIDs)
			{
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.ExecuteGetAchievements: [" + ((achievementIDs != null) ? achievementIDs.Length : 0) + "] achievements");
				if (Achievements.OnAchievementRetrieved != null)
				{
					foreach (string text in achievementIDs)
					{
						bool flag = false;
						foreach (SteamAchievement achievement in data.achievements)
						{
							if (achievement.id == text)
							{
								flag = true;
								Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.ExecuteGetAchievements: Got [" + text + "]");
								Achievements.OnAchievementRetrieved(profile, achievement);
								break;
							}
						}
						if (!flag)
						{
							Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.ExecuteGetAchievements: Failed to get [" + text + "]");
							Achievements.OnAchievementRetrieved(profile, new SteamAchievement(text, "", false, 0.0, false, DateTime.MinValue));
						}
					}
				}
				updateInProgress = false;
			}

			private void SteamUserStats_OnUserStatsReceived(UserStatsReceived_t callback)
			{
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.SteamUserStats_OnUserStatsReceived");
				if (callback.m_nGameID != (ulong)gameID)
				{
					return;
				}
				steamUserStatsReceivedCallback.Dispose();
				steamUserStatsReceivedCallback = null;
				foreach (KeyValuePair<Profiles.Profile, SteamAchievementsData> achievementManagerDatum in achievementManagerData)
				{
					if (achievementManagerDatum.Value.achievements.Count != 0)
					{
						continue;
					}
					if (callback.m_eResult == EResult.k_EResultOK)
					{
						Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.SteamUserStats_OnUserStatsReceived: Success");
						foreach (AchievementData achievementDatum in achievementManagerDatum.Value.achievementData)
						{
							bool pbAchieved;
							uint punUnlockTime;
							if (!SteamUserStats.GetAchievementAndUnlockTime(achievementDatum.SteamID, out pbAchieved, out punUnlockTime))
							{
								pbAchieved = false;
								punUnlockTime = 0u;
								Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.SteamUserStats_OnUserStatsReceived: Failed to get [" + achievementDatum.AchievementName + "] (" + achievementDatum.SteamID + ")");
							}
							SteamAchievement steamAchievement = new SteamAchievement(achievementDatum.AchievementName, achievementDatum.SteamID, pbAchieved, pbAchieved ? 1.0 : 0.0, achievementDatum.Hidden, new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(punUnlockTime));
							Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.SteamUserStats_OnUserStatsReceived: [" + steamAchievement.id + "] (" + steamAchievement.steamID + ") unlocked [" + steamAchievement.completed + "], time [" + steamAchievement.lastReportedDate.ToLocalTime().ToString() + "]");
							achievementManagerDatum.Value.achievements.Add(steamAchievement);
						}
						if (Achievements.OnAchievementsLoaded != null)
						{
							Achievements.OnAchievementsLoaded(achievementManagerDatum.Key, true);
						}
						continue;
					}
					Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.SteamUserStats_OnUserStatsReceived: Failed (achievement load complete)");
					foreach (AchievementData achievementDatum2 in achievementManagerDatum.Value.achievementData)
					{
						SteamAchievement item = new SteamAchievement(achievementDatum2.AchievementName, achievementDatum2.SteamID, false, 0.0, achievementDatum2.Hidden, DateTime.MinValue);
						achievementManagerDatum.Value.achievements.Add(item);
					}
					if (Achievements.OnAchievementsLoaded != null)
					{
						Achievements.OnAchievementsLoaded(achievementManagerDatum.Key, false);
					}
				}
				updateInProgress = false;
			}

			private void SteamUserStats_OnAchievementStored(UserAchievementStored_t callback)
			{
				Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.SteamUserStats_OnAchievementStored: [" + callback.m_rgchAchievementName + "]");
				if (callback.m_nGameID != (ulong)gameID)
				{
					return;
				}
				steamUserAchievementStoredCallback.Dispose();
				steamUserAchievementStoredCallback = null;
				if (Achievements.OnAchievementUnlocked != null)
				{
					foreach (KeyValuePair<Profiles.Profile, SteamAchievementsData> achievementManagerDatum in achievementManagerData)
					{
						foreach (SteamAchievement achievement in achievementManagerDatum.Value.achievements)
						{
							if (achievement.steamID == callback.m_rgchAchievementName)
							{
								Log(VerbosityLevel.Info, "Achievements.SteamAchievementsImpl.SteamUserStats_OnAchievementStored: [" + achievement.id + "] (" + achievement.steamID + ") unlocked [" + achievement.completed + "], time [" + achievement.lastReportedDate.ToLocalTime().ToString() + "]");
								Achievements.OnAchievementUnlocked(achievementManagerDatum.Key, achievement);
								break;
							}
						}
					}
				}
				updateInProgress = false;
			}

			private void Profiles_OnDeactivated(Profiles.Profile profile)
			{
				if (achievementManagerData.ContainsKey(profile))
				{
					achievementManagerData.Remove(profile);
				}
			}
		}

		private class SteamAchievementsData
		{
			public List<AchievementData> achievementData = new List<AchievementData>();

			public List<SteamAchievement> achievements = new List<SteamAchievement>();

			public bool requestLoad = false;

			public List<string> requestUnlockAchievementIDs = new List<string>();

			public List<string> requestGetAchievementIDs = new List<string>();
		}

		private class SteamAchievement : IAchievement
		{
			private bool _completed = false;

			private bool _hidden = false;

			private string _id;

			private string _steamID;

			private DateTime _lastReportedDate;

			private double _percentCompleted = 0.0;

			public bool completed
			{
				get
				{
					return _completed;
				}
				set
				{
					_completed = value;
				}
			}

			public bool hidden
			{
				get
				{
					return _hidden;
				}
				set
				{
					_hidden = value;
				}
			}

			public string id
			{
				get
				{
					return _id;
				}
				set
				{
					_id = value;
				}
			}

			public string steamID
			{
				get
				{
					return _steamID;
				}
				set
				{
					_steamID = value;
				}
			}

			public DateTime lastReportedDate
			{
				get
				{
					return _lastReportedDate;
				}
				set
				{
					_lastReportedDate = value;
				}
			}

			public double percentCompleted
			{
				get
				{
					return _percentCompleted;
				}
				set
				{
					_percentCompleted = value;
				}
			}

			public void ReportProgress(Action<bool> callback)
			{
			}

			public SteamAchievement()
			{
			}

			public SteamAchievement(string id, string steamID, bool completed, double progress, bool hidden, DateTime lastReportedDate)
			{
				_id = id;
				_steamID = steamID;
				_completed = completed;
				_percentCompleted = progress;
				_hidden = hidden;
				_lastReportedDate = lastReportedDate;
			}
		}

		private static AchievementsImpl implementation = null;

		public static VerbosityLevel VerbosityLevel = VerbosityLevel.Error;

		public static event AchievementsLoaded OnAchievementsLoaded;

		public static event AchievementUnlocked OnAchievementUnlocked;

		public static event AchievementRetrieved OnAchievementRetrieved;

		public static void Initialize()
		{
			if (implementation == null)
			{
				implementation = AchievementsImpl.GetAchievementsImpl();
				implementation.Initialize();
			}
			else
			{
				Debug.LogError("Achievements already initialized!");
			}
		}

		public static void SetAchievementsMetadata(object metadata)
		{
			if (implementation != null)
			{
				implementation.SetAchievementsMetadata(metadata);
			}
		}

		public static void SetAchievementsFile(Profiles.Profile profile, UserData.File file)
		{
			if (implementation != null)
			{
				implementation.SetAchievementsFile(profile, file);
			}
		}

		public static void LoadAchievements(Profiles.Profile profile, List<AchievementData> achievementData)
		{
			if (implementation != null)
			{
				implementation.LoadAchievements(profile, achievementData);
			}
		}

		public static void UnlockAchievement(Profiles.Profile profile, string achievementID)
		{
			if (implementation != null)
			{
				implementation.UnlockAchievement(profile, achievementID);
			}
		}

		public static void GetAchievement(Profiles.Profile profile, string achievementID)
		{
			if (implementation != null)
			{
				implementation.GetAchievement(profile, achievementID);
			}
		}

		public static void Destroy()
		{
			if (implementation != null)
			{
				implementation.Destroy();
			}
		}

		private static void Log(VerbosityLevel verbosityLevel, string message)
		{
			if (VerbosityLevel >= verbosityLevel)
			{
				switch (verbosityLevel)
				{
				case VerbosityLevel.Error:
					Debug.LogError(message);
					break;
				case VerbosityLevel.Warning:
					Debug.LogWarning(message);
					break;
				default:
					Debug.Log(message);
					break;
				}
			}
		}
	}
}
