using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.ActiveCareer;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Lyralei.InterestMod.Careers;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Seasons;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HobbiesAndInterests.Career
{
    public class CareerInstantiatorManager
    {

		public static bool kIndoorFarmerCareerInstalled = false;

		public static Dictionary<string, ulong> mCareersGUIDS = new Dictionary<string, ulong>()
		{
			{"PlantsGardener", 0x6E2E4EC20E9DA1A1 },
		};

		public static void PostInitCareer(object sender, EventArgs e)
		{
			try
			{
				foreach(KeyValuePair<string, ulong> kpv in mCareersGUIDS)
                {
					AddEnumValue<OccupationNames>(kpv.Key, kpv.Value);
				}
				
				//ThoughtBalloonManager.ParseBalloonListData(XmlDbData.ReadData("Arsil_MangakaActiveCareer_Balloons"), "Topic", "Key", ThoughtBalloonManager.sBalloons);
				ParseCustomActiveCareers("InterestsAndHobbiesCareers", "", "");
			}
			catch (Exception ex)
			{
				InterestManager.print(ex.ToString());
				//DebugMsg(ex.Message + "\n" + ex.StackTrace.Substring(0, 500));
			}
		}

		public static void AddInteractionsCareers(CityHall rh)
		{
			if (rh.GetType() == typeof(CityHall) || rh.GetType().IsSubclassOf(typeof(CityHall)))
			{
				foreach (InteractionObjectPair interaction in rh.Interactions)
				{
					if (interaction.InteractionDefinition.GetType() == JoinActiveCareerFarmer.Singleton.GetType())
					{
						return;
					}
				}
				rh.AddInteraction(JoinActiveCareerFarmer.Singleton, true);
			}
		}

		public static void ParseCustomActiveCareers(string resourceNameCareer, string resourceNameJobsAndTasks, string resourceNameOcccupationLevelAndJobs)
		{
			XmlDbData xmlDbData = XmlDbData.ReadData(resourceNameCareer);
			XmlDbTable value;
			XmlDbTable value2;
			XmlDbTable value3;

			if (xmlDbData == null || xmlDbData.Tables == null)
			{
				InterestManager.print("No xml data or no resource (" + resourceNameCareer + ")");
			}

			if (xmlDbData == null || xmlDbData.Tables == null || !xmlDbData.Tables.TryGetValue("SkillBasedCareers", out value) || !xmlDbData.Tables.TryGetValue("CareerLevels", out value2))
			{
				return;
			}
            else
            {
				Dictionary<ulong, List<XmlDbRow>> careerToCareerLevelXmlDataMap = ActiveCareer.GenerateCareerToCareerLevelXmlDataMap(value2, "SkilledProfession", "SkillBasedCareers");
				ParseCustomActiveCareers(value, careerToCareerLevelXmlDataMap);
				ActiveCareer.ParseSkillContributions(value2);
			}
		}

		private static void ParseCustomActiveCareers(XmlDbTable activeCareerTable, Dictionary<ulong, List<XmlDbRow>> careerToCareerLevelXmlDataMap)
		{
			try
			{
				foreach (XmlDbRow row in activeCareerTable.Rows)
				{
					ProductVersion value3;
					OccupationNames value4;

					if (!row.TryGetEnum("ProductVersion", out value3, ProductVersion.Undefined) || !GameUtils.IsInstalled(value3) || !row.TryGetEnum("SkilledProfession", out value4, OccupationNames.Undefined) || (Occupation.sOccupationStaticDataMap != null && Occupation.sOccupationStaticDataMap.ContainsKey((ulong)value4)))
					{
						InterestManager.print("Couldn't load: " + row.GetString("SkilledProfession"));
						continue;
					}

					string @string = row.GetString("Skill_Name");
					SkillNames skillName = SkillManager.sSkillEnumValues.ParseEnumValue(@string);
					int @int = row.GetInt("Minimum_Skill_Level", -1);
					float @float = row.GetFloat("XP_Gain_Multiplier", 0f);
					int int2 = row.GetInt("Highest_Level", 0);
					string string2 = row.GetString("Speech_Balloon_Image");
					string string3 = row.GetString("HUD_Icon");
					string string4 = row.GetString("Wishes_Icon");
					string careerDescriptionLocalizationKey = "Gameplay/Excel/SkillBasedCareers/SkillBasedCareers:" + row.GetString("Description_Text");
					string careerOfferLocalizationKey = "Gameplay/Excel/SkillBasedCareers/SkillBasedCareers:" + row.GetString("Offer_Text");
					List<string> list = new List<string>();
					for (int i = 1; i <= 3; i++)
					{
						string string5 = row.GetString("Career_Responsibility_" + i);
						if (string.IsNullOrEmpty(string5))
						{
							break;
						}
						list.Add("Gameplay/Excel/SkillBasedCareers/SkillBasedCareers:" + string5);
					}
					List<string> list2 = new List<string>();
					for (int j = 1; j <= 3; j++)
					{
						string string6 = row.GetString("Career_Responsibility_Short_" + j);
						if (string.IsNullOrEmpty(string6))
						{
							break;
						}
						list2.Add("Gameplay/Excel/SkillBasedCareers/SkillBasedCareers:" + string6);
					}
					List<string> list3 = new List<string>();
					for (int k = 1; k <= 3; k++)
					{
						string string7 = row.GetString("Career_Responsibility_Icon_" + k);
						if (string.IsNullOrEmpty(string7))
						{
							break;
						}
						list3.Add(string7);
					}
					List<XmlDbRow> value5;
					if (!careerToCareerLevelXmlDataMap.TryGetValue((ulong)value4, out value5))
					{
						InterestManager.print("Couldn't find: " + row.GetString("SkilledProfession") + " inside careerToCareerLevelXMLDataMap");
						continue;
					}

					Dictionary<int, OccupationLevelStaticData> levelStaticDataMap = GenerateCareerLevelToStaticLevelDataMap(value4, value5);
					Type type = row.GetClassType("FullClassName");
					Type[] types = new Type[1] { typeof(OccupationNames) };
					ConstructorInfo constructor = type.GetConstructor(types);
					SkillBasedCareer skillBasedCareer = (SkillBasedCareer)constructor.Invoke(new object[1] { value4 });

					if (skillBasedCareer != null)
					{
						Dictionary<uint, JobStaticData> jobStaticDataMap = new Dictionary<uint, JobStaticData>();
						Dictionary<uint, TaskStaticData> taskStaticDataMap = new Dictionary<uint, TaskStaticData>();
						Dictionary<string, TrackedAsStaticData> trackedAsMappingsStaticDataMap = new Dictionary<string, TrackedAsStaticData>();
						SkillBasedCareerStaticData value6 = new SkillBasedCareerStaticData(skillName, @int, @float, int2, string2, string3, string4, careerDescriptionLocalizationKey, careerOfferLocalizationKey, list, list2, list3, levelStaticDataMap, jobStaticDataMap, taskStaticDataMap, trackedAsMappingsStaticDataMap);
						if (Occupation.sOccupationStaticDataMap == null)
						{
							Occupation.sOccupationStaticDataMap = new Dictionary<ulong, OccupationStaticData>();
						}
						Occupation.sOccupationStaticDataMap.Add((ulong)value4, value6);
						CareerManager.AddStaticOccupation(skillBasedCareer);
					}
                    else
                    {
						InterestManager.print("Couldn't load class: " + row.GetString("SkilledProfession") );

					}

				}
			}
			catch(Exception ex)
            {
				InterestManager.print(ex.ToString());
            }
		}

		public static Dictionary<int, OccupationLevelStaticData> GenerateCareerLevelToStaticLevelDataMap(OccupationNames career, List<XmlDbRow> careerLevelRows)
		{
			Dictionary<int, OccupationLevelStaticData> dictionary = new Dictionary<int, OccupationLevelStaticData>();
			foreach (XmlDbRow careerLevelRow in careerLevelRows)
			{
				int @int = careerLevelRow.GetInt("Level", -1);
				if (@int <= 0 || dictionary.ContainsKey(@int))
				{
					continue;
				}
				string title = "Gameplay/Excel/SkillBasedCareers/CareerLevels:" + careerLevelRow.GetString("Title");
				string description = "Gameplay/Excel/SkillBasedCareers/CareerLevels:" + careerLevelRow.GetString("Description");
				int int2 = careerLevelRow.GetInt("Stipend", -1);
				int int3 = careerLevelRow.GetInt("Goal_XP", -1);
				List<string> stringList = careerLevelRow.GetStringList("Reward", ',');
				List<XpBasedCareerReward> list = null;
				foreach (string item2 in stringList)
				{
					string[] array = item2.Split(':');
					if (array != null && array.Length == 2)
					{
						if (list == null)
						{
							list = new List<XpBasedCareerReward>();
						}
						XpBasedCareerRewardType value;
						if (ParserFunctions.TryParseEnum(array[0], out value, XpBasedCareerRewardType.Invalid))
						{
							XpBasedCareerReward item = new XpBasedCareerReward(value, array[1]);
							list.Add(item);
						}
					}
				}
				XpBasedCareerReward[] rewards = ((list != null) ? list.ToArray() : null);
				string @string = careerLevelRow.GetString("Reward_Text");
				string rewardTnsText = (string.IsNullOrEmpty(@string) ? string.Empty : ("Gameplay/Excel/SkillBasedCareers/CareerLevels:" + @string));
				XpBasedCareerLevelStaticData value2 = new XpBasedCareerLevelStaticData(int3, int2, title, description, rewards, rewardTnsText, null, null, null, null, 0f, 0f, 0f);
				dictionary.Add(@int, value2);
			}
			return dictionary;
		}

		public static Dictionary<ulong, List<XmlDbRow>> GenerateCareerToCareerLevelXmlDataMap(XmlDbTable careerLevelTable, string columnName, string spreadsheetName)
		{
			Dictionary<ulong, List<XmlDbRow>> dictionary = new Dictionary<ulong, List<XmlDbRow>>();
			foreach (XmlDbRow row in careerLevelTable.Rows)
			{
				OccupationNames value;
				if (row.TryGetEnum(columnName, out value, OccupationNames.Undefined))
				{
					List<XmlDbRow> value2;
					if (dictionary.TryGetValue((ulong)value, out value2))
					{
						value2.Add(row);
						continue;
					}
					List<XmlDbRow> list = new List<XmlDbRow>();
					list.Add(row);
					dictionary.Add((ulong)value, list);
				}
			}
			return dictionary;
		}

		public static void AddEnumValue<T>(string key, object value) where T : struct
		{
			Type typeFromHandle = typeof(T);
			EnumParser value2;
			if (!ParserFunctions.sCaseInsensitiveEnumParsers.TryGetValue(typeFromHandle, out value2))
			{
				value2 = new EnumParser(typeFromHandle, true);
				ParserFunctions.sCaseInsensitiveEnumParsers.Add(typeFromHandle, value2);
			}
			EnumParser value3;
			if (!ParserFunctions.sCaseSensitiveEnumParsers.TryGetValue(typeFromHandle, out value3))
			{
				value3 = new EnumParser(typeFromHandle, false);
				ParserFunctions.sCaseSensitiveEnumParsers.Add(typeFromHandle, value3);
			}
			if (!value2.mLookup.ContainsKey(key.ToLowerInvariant()) && !value3.mLookup.ContainsKey(key))
			{
				value2.mLookup.Add(key.ToLowerInvariant(), value);
				value3.mLookup.Add(key, value);
			}
		}

		public static void AssignMangakaActiveCareer(Sim createdSim, ulong guid)
		{
			try
			{
				AcquireOccupationParameters occupationParameters = new AcquireOccupationParameters(guid, false, false);
				bool flag = createdSim != null && createdSim.IsSelectable;
				bool flag2 = false;
				bool flag3 = false;
				CareerLocation location = null;
				OccupationNames mangakaOccupationGuid = (OccupationNames)guid;
				Occupation newCareer;
				if (!createdSim.CareerManager.TryGetNewCareer(mangakaOccupationGuid, out newCareer))
				{
					return;
				}
				createdSim.CareerManager.AdjustParameters(createdSim, newCareer, ref occupationParameters);
				string newJobName = string.Empty;
				if (flag2 && flag && !newCareer.TryDisplayingGetHiredUi(location, ref occupationParameters, out newJobName))
				{
					return;
				}
				if (createdSim.CareerManager.mJob != null && !(createdSim.CareerManager.mJob is School))
				{
					if (!flag3 && (flag2 || flag))
					{
						string text = ((!string.IsNullOrEmpty(newJobName)) ? newJobName : newCareer.CareerName);
						if (!Occupation.ShowYesNoCareerOptionDialog(Localization.LocalizeString(createdSim.CareerManager.mSimDescription.IsFemale, "Gameplay/Careers/Career:ConfirmLeavingOldCareer", createdSim.CareerManager.mSimDescription, text, createdSim.CareerManager.mJob.CareerName)))
						{
							return;
						}
						if (createdSim.GetCurrentOutfitCategoryFromOutfitInGameObject() == OutfitCategories.Career)
						{
							createdSim.PushSwitchToOutfitInteraction(Sim.ClothesChangeReason.Force, OutfitCategories.Everyday);
						}
					}
					if (createdSim != null)
					{
						EventTracker.SendEvent(new TransferCareerEvent(createdSim, createdSim.CareerManager.mJob, newCareer));
					}
					createdSim.CareerManager.mJob.LeaveJob(false, Sims3.Gameplay.Careers.Career.LeaveJobReason.kTransfered);
				}
				if (createdSim.CareerManager.RetiredCareer != null)
				{
					if (createdSim.CareerManager.RetiredCareer.Guid == newCareer.Guid && (flag2 || flag) && !Occupation.ShowYesNoCareerOptionDialog(Localization.LocalizeString(createdSim.CareerManager.mSimDescription.IsFemale, "Gameplay/Careers/Career:ConfirmPensionLoss", createdSim.CareerManager.mSimDescription, createdSim.CareerManager.RetiredCareer.CareerName)))
					{
						return;
					}
					createdSim.CareerManager.RetirementKillPension();
				}
				EventTracker.SendEvent(EventTypeId.kCareerNewJob, createdSim);
				newCareer.OwnerDescription = createdSim.CareerManager.mSimDescription;
				newCareer.mDateHired = SimClock.CurrentTime();
				if (createdSim != null && createdSim.IsSelectable && SeasonsManager.Enabled)
				{
					newCareer.WorkAnniversary = new Anniversary(SeasonsManager.CurrentSeason, (int)SeasonsManager.DaysElapsed);
				}
				newCareer.mAgeWhenJobFirstStarted = createdSim.CareerManager.mSimDescription.Age;
				newCareer.SetAttributesForNewJob(location, occupationParameters.LotId, occupationParameters.CharacterImportRequest);
				EventTracker.SendEvent(new CareerEvent(EventTypeId.kEventCareerHired, newCareer));
				EventTracker.SendEvent(new CareerEvent(EventTypeId.kEventCareerChanged, newCareer));
				EventTracker.SendEvent(new CareerEvent(EventTypeId.kCareerDataChanged, newCareer));
				EventTracker.SendEvent(new CareerEvent(EventTypeId.kEventCareerPromoteOrDemote, newCareer));
				newCareer.RefreshMapTagForOccupation();
				createdSim.CareerManager.UpdateCareerUI();
				if (flag2 && flag)
				{
					string newOccupationTnsText = newCareer.GetNewOccupationTnsText();
					if (!string.IsNullOrEmpty(newOccupationTnsText))
					{
						newCareer.ShowOccupationTNS(newOccupationTnsText);
					}
					Audio.StartSound("sting_career_positive");
				}
				if (occupationParameters.JumpStartJob)
				{
					createdSim.CareerManager.mJob.JumpStartJob(occupationParameters);
				}
				if (createdSim != null && createdSim.IsActiveSim)
				{
					HudController.SetInfoState(InfoState.Career);
				}
				if (createdSim != null)
				{
					ActiveTopic.RemoveTopicFromSim(createdSim, "PerformanceDuel");
					ActiveTopic.RemoveTopicFromSim(createdSim, "EverHeardOfMe");
					ActiveTopic.RemoveTopicFromSim(createdSim, "EnthuseAboutUpcomingShow");
					ActiveTopic.RemoveTopicFromSim(createdSim, "HaveConversationWithSelf");
				}
				if (createdSim != null)
				{
					newCareer.OnAcquireOccupation(createdSim);
				}
			}
			catch (Exception ex)
			{
				InterestManager.print(ex.ToString());
			}
		}

	}
}
