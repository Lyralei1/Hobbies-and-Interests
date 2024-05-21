using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Lyralei.InterestMod;
using Sims3.Gameplay.Lyralei.InterestsAndHobbies;
using Sims3.Gameplay.Objects.Alchemy;
using Sims3.Gameplay.Objects.Appliances;
using Sims3.Gameplay.Objects.CookingObjects;
using Sims3.Gameplay.Objects.Counters;
using Sims3.Gameplay.Objects.Decorations;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Fireplaces;
using Sims3.Gameplay.Objects.Fishing;
using Sims3.Gameplay.Objects.Gardening;
using Sims3.Gameplay.Objects.HobbiesSkills;
using Sims3.Gameplay.Objects.HobbiesSkills.Science;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Objects.Plumbing;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.Store.Objects;
using Sims3.UI;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Text;
using static Sims3.Gameplay.ActorSystems.OccultImaginaryFriend;
using static Sims3.Gameplay.Core.Terrain;
using static Sims3.Gameplay.Objects.HobbiesSkills.Easel;
using static Sims3.Gameplay.StoreSystems.SetObjectHelper;
using static Sims3.Gameplay.StoryProgression.Demographics.Demographic;
using Camera = Sims3.Gameplay.Core.Camera;
using Food = Sims3.Gameplay.Lyralei.InterestMod.Food;
using Interest = Sims3.Gameplay.Lyralei.InterestMod.Interest;

namespace Lyralei.InterestMod
{
    /// <summary>
    /// This class is meant for setting and helping already existing sims that are NOT the active household, so that non-played or 'playable' sims do have some interests and interact with your sims.
    /// </summary>
    public class TownieInterestHelper
    {
        public static void DoTraitTownieCalculation(Interest interests, int amountOfTraits, SimDescription simDescription, bool isForPenalty)
        {
            //InterestManager.print("Actually went here");
            for (int i = 0; i < amountOfTraits; i++)
            {
                if (isForPenalty)
                {
                    interests.modifyInterestLevel(InterestManager.perTraitPenaltyPointLost + RandomUtil.GetInt(Tunables.MinMaxForTowniesInterestPenalty[0], Tunables.MinMaxForTowniesInterestPenalty[1]), simDescription.mSimDescriptionId, interests.mInterestsGuid);
                }
                else
                {
                    DoSkillCheckOnPointsGained(simDescription, interests);
                    interests.modifyInterestLevel(InterestManager.perTraitPassionatePointGained + RandomUtil.GetInt(Tunables.MinMaxForTowniesInterestPassionate[0], Tunables.MinMaxForTowniesInterestPassionate[1]), simDescription.mSimDescriptionId, interests.mInterestsGuid);
                }
            }

            if (premadeSims.ContainsKey(simDescription.mSimDescriptionId))
            {
                foreach (Interest positive in premadeSims[simDescription.mSimDescriptionId].positiveInterests)
                {
                    if (interests.GetType() == positive.GetType())
                    {
                        interests.modifyInterestLevel(RandomUtil.GetInt(Tunables.MinMaxForTowniesInterestPassionate[0], Tunables.MinMaxForTowniesInterestPassionate[1]), simDescription.mSimDescriptionId, interests.mInterestsGuid);
                    }
                }

                foreach (Interest negative in premadeSims[simDescription.mSimDescriptionId].negativeInterests)
                {
                    if (interests.GetType() == negative.GetType())
                    {
                        interests.modifyInterestLevel(RandomUtil.GetInt(Tunables.MinMaxForTowniesInterestPenalty[0], Tunables.MinMaxForTowniesInterestPenalty[1]), simDescription.mSimDescriptionId, interests.mInterestsGuid);
                    }
                }
            }
        }

        // Turn into a switch statement.
        public static void DoSkillCheckOnPointsGained(SimDescription desc, Interest interest)
        {
            if (interest.mInterestsGuid == InterestTypes.Environment)
            {
                if (desc.SkillManager.HasElement(SkillNames.Gardening) && desc.SkillManager.GetSkillLevel(SkillNames.Gardening) > 1)
                {
                    interest.modifyInterestLevel(RandomUtil.GetInt(Tunables.MinMaxForTowniesInterestPassionate[0], Tunables.MinMaxForTowniesInterestPassionate[1]), desc.mSimDescriptionId, interest.mInterestsGuid);
                }
            }
        }

        public class simPremadeInterests
        {
            public Interest[] positiveInterests;
            public Interest[] negativeInterests;

            public simPremadeInterests(Interest[] positives, Interest[] negatives)
            {
                positiveInterests = positives;
                negativeInterests = negatives;
            }
        }

        public static Dictionary<ulong, simPremadeInterests> premadeSims = new Dictionary<ulong, simPremadeInterests>()
        {
            {293, new simPremadeInterests(new Interest[] { new Crime(293u), new Money(293u) }, new Interest[0]) }, // Nick alto
            {326, new simPremadeInterests(new Interest[] { new Crime(326u), new Money(326u), new Politics(326u) }, new Interest[0]) }, // Vita Alto
            {306, new simPremadeInterests(new Interest[] { new Health(306u), new Sims3.Gameplay.Lyralei.InterestMod.Food(306u), new Sims3.Gameplay.Lyralei.InterestMod.Environment(306u, true) }, new Interest[0]) }, // Fiona McIrish
            {307, new simPremadeInterests(new Interest[] { new Culture(307u) }, new Interest[0]) }, // River McIrish
            {308, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Food(308u), new Entertainment(308u) }, new Interest[0]) }, // Molly French
            {3185170999403604176, new simPremadeInterests(new Interest[] { new Money(3185170999403604176u), new Paranormal(3185170999403604176u), new Crime(3185170999403604176u) }, new Interest[] { new Food(3185170999403604176u), new Politics(3185170999403604176u), new Health(3185170999403604176u), new Fashion(3185170999403604176u), new Scifi(3185170999403604176u), new Animals(3185170999403604176u) }) }, // Simon Crumplebottom
            {3185170999403604224, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(3185170999403604224u, false), new Food(3185170999403604224u), new Crime(3185170999403604224u) }, new Interest[] { new Politics(3185170999403604224u), new Paranormal(3185170999403604224u), new Sports(3185170999403604224u), new Entertainment(3185170999403604224u), new Scifi(3185170999403604224u) } ) }, // Gretle Goth
            {3185170999403604256, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(3185170999403604256u, false), new Food(3185170999403604256u), new Money(3185170999403604256u), new Paranormal(3185170999403604256u), new Sports(3185170999403604256u), new Entertainment(3185170999403604256u) }, new Interest[] { new Health(3185170999403604256u) }) }, // Prudence Crumplebottom
            {3185170999403604288, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(3185170999403604288u, false), new Politics(3185170999403604288u), new Fashion(3185170999403604288u), new Entertainment(3185170999403604288u) }, new Interest[] { new Food(3185170999403604288u), new Culture(3185170999403604288u), new Health(3185170999403604288u), new Paranormal(3185170999403604288u), new Sports(3185170999403604288u), new Scifi(3185170999403604288u) } ) }, // Victor Goth
            {244, new simPremadeInterests(new Interest[] { new Culture(244), new Health(244), new Fashion(244), new Paranormal(244), new Politics(244) }, new Interest[]{ new Sims3.Gameplay.Lyralei.InterestMod.Environment(244, false), new Food(244), new Animals(244), new Scifi(244) } ) }, // Gunther Goth
            {245, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(245, false), new Food(245), new Culture(245), new Fashion(245), new Crime(245), new Entertainment(245) }, new Interest[]{ new Paranormal(245), new Scifi(245) } ) }, // Cornelia Goth
            {246, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(246, false), new Culture(246), new Money(246), new Paranormal(246), new Health(246), new Animals(246), new Toys(246), new Scifi(246) }, new Interest[] { new Politics(246), new Fashion(246), new Entertainment(246), new Sports(246), new Food(246), new Crime(246) } ) }, // Mortimer Goth
            {13063, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(13063, false), new Crime(13063), new Sports(13063) }, new Interest[] { new Politics(13063) } ) }, // Claire Ursine
            {394, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(394, false), new Scifi(394), new Toys(394), new Entertainment(394), new Money(394) }, new Interest[] { new Sports(394) } ) }, // Susan Wainwright
            {161, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(161, false), new Money(161) }, new Interest[0] ) }, // Gobias Koffi
            {2314, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(2314, false), new Food(2314) }, new Interest[0] ) }, // Christopher Steel
            {28988, new simPremadeInterests(new Interest[] { new Crime(28988), new Sports(28988) }, new Interest[] { new Politics(28988) } ) }, // Justine keaton
            {184, new simPremadeInterests(new Interest[] { new Money(184), new Sports(184)  }, new Interest[0] ) }, // Leighton Sekemoto
            {186, new simPremadeInterests(new Interest[] { new Food(186) }, new Interest[0] ) }, // Yumi Sekemoto
            {257, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(257, false), new Sports(257), new Politics(257), new Food(257), new Culture(257), new Money(257), new Fashion(257) }, new Interest[] { new Health(257), new Crime(257), new Animals(257), new Scifi(257), new Paranormal(257) } ) }, // Jocasta Bachelor
            {1789, new simPremadeInterests(new Interest[] { new Toys(1789) , new Culture(1789), new Fashion(1789) }, new Interest[0] ) }, // Lolita Goth
            {258, new simPremadeInterests(new Interest[] { new Culture(258) , new Fashion(258), new Sports(258), new Entertainment(258) }, new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(258, false), new Food(258), new Paranormal(258), new Health(258), new Scifi(258), new Crime(258) } ) }, // Simis Bachelor
            {259, new simPremadeInterests(new Interest[] { new Fashion(259), new Sims3.Gameplay.Lyralei.InterestMod.Environment(259, false), new Money(259), new Crime(259), new Sports(259), new Scifi(259), new Food(259), new Politics(259), new Toys(259), new Paranormal(259) }, new Interest[] { new Health(259), new Animals(259) } ) }, // Bella Bachelor
            {260, new simPremadeInterests(new Interest[] { new Politics(260), new Sports(260), new Entertainment(260), new Fashion(260), new Scifi(260) }, new Interest[] { new Money(260), new Culture(260), new Food(260) } ) }, // Micheal Bachelor
            {152, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(152, false), new Money(152), new Entertainment(152) }, new Interest[] { new Sports(152) } ) }, // Beau Andrews
            {154, new simPremadeInterests(new Interest[] { new Culture(154) }, new Interest[] { new Scifi(154), new Entertainment(154) } ) }, // Victoria Andrews
            {6326, new simPremadeInterests(new Interest[] { new Entertainment(6326), new Fashion(6326), new Money(6326), new Toys(6326) }, new Interest[] { new Scifi(6326), new Culture(6326), new Sports(6326), new Politics(6326) } ) }, // Madison VanWatson
            {277, new simPremadeInterests(new Interest[] { new Politics(277), new Health(277), new Sports(277), new Culture(277) }, new Interest[] { new Money(277), new Sims3.Gameplay.Lyralei.InterestMod.Environment(277, false), new Entertainment(277), new Scifi(277) } ) }, // Agnes Crumplebottom
            {218, new simPremadeInterests(new Interest[] { new Money(218), new Health(218), new Sports(218), new Entertainment(218) }, new Interest[] { new Toys(218) } ) }, // Thornton Wolff
            {6394, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(6394, true), new Culture(6394) }, new Interest[] { new Crime(6394) } ) }, // Holly Alto
            {6357, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(6357, false), new Scifi(6357) }, new Interest[0] ) }, // Boyd Wainwright
            {29105, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(29105, false), new Money(29105) }, new Interest[] { new Scifi(29105) } ) }, // Stiles McGraw
            {29103, new simPremadeInterests(new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(29103, false), new Crime(29103), new Sports(29103) }, new Interest[0] ) }, // Marty Keaton
            {193, new simPremadeInterests(new Interest[] { new Crime(193), new Fashion(193) }, new Interest[] { new Animals(193) }) }, // Blair Wainwright
            {195, new simPremadeInterests(new Interest[] { new Food(195), new Animals(195), new Culture(195), new Paranormal(195) }, new Interest[0] ) }, // Emma Hatch
            {197, new simPremadeInterests(new Interest[] { new Scifi(197), new Entertainment(197), new Toys(197) }, new Interest[] { new Culture(197), new Paranormal(197) } ) }, // Cycl0n3 Sw0rd
            {198, new simPremadeInterests(new Interest[] { new Politics(198), new Entertainment(198) }, new Interest[] { new Toys(198), new Paranormal(198), new Scifi(198) } ) }, // Tamara Donner
            {163, new simPremadeInterests(new Interest[] { new Politics(163), new Sims3.Gameplay.Lyralei.InterestMod.Environment(163, false), new Food(163), new Sports(163) }, new Interest[] { new Toys(163), new Paranormal(163), new Scifi(163), new Culture(163)  } ) }, // Jack Bunch
            {165, new simPremadeInterests(new Interest[] { new Animals(165), new Sims3.Gameplay.Lyralei.InterestMod.Environment(165, false), new Culture(165) }, new Interest[] { new Toys(165), new Paranormal(165), new Scifi(165) } ) }, // Judy Bunch
            {167, new simPremadeInterests(new Interest[] { new Entertainment(167), new Scifi(167) }, new Interest[] { new Toys(167), new Food(167), new Paranormal(167), new Sports(167) } ) }, // Ethan Bunch
            {169, new simPremadeInterests(new Interest[] { new Crime(169), new Toys(169), new Sports(169) }, new Interest[] { new Paranormal(169), new Scifi(169) } ) }, // Lisa Bunch
            {171, new simPremadeInterests(new Interest[] { new Sports(171), new Toys(171), new Sims3.Gameplay.Lyralei.InterestMod.Environment(171, false) }, new Interest[] { new Paranormal(171), new Scifi(171) } ) }, // Arlo Bunch
            {173, new simPremadeInterests(new Interest[] { new Toys(173) }, new Interest[] { new Paranormal(173), new Scifi(173) } ) }, // Darlene Bunch
            {1225, new simPremadeInterests(new Interest[] { new Culture(1225) }, new Interest[0] ) }, // Bessie Clavell
            {1226, new simPremadeInterests(new Interest[] { new Crime(1226), new Entertainment(1226), new Money(1226), new Fashion(1226) }, new Interest[] { new Health(1226), new Sports(1226) } ) }, // Xander Clavell
            {1227, new simPremadeInterests(new Interest[] { new Sports(1227), new Entertainment(1227), new Money(1227), new Sims3.Gameplay.Lyralei.InterestMod.Environment(1227, false) }, new Interest[] { new Food(1227), new Entertainment(1227), new Culture(1227), new Toys(1227), new Scifi(1227), new Paranormal(1227) } ) }, // Buster Clavell
            {224, new simPremadeInterests(new Interest[] { new Toys(224), new Entertainment(224), new Scifi(224), new Paranormal(224) }, new Interest[] { new Fashion(224) } ) }, // Dorie Hart
            {377, new simPremadeInterests(new Interest[] { new Toys(377), new Entertainment(377), new Scifi(377), new Paranormal(377) }, new Interest[] { new Fashion(377) } ) }, // Gus Hart
            {378, new simPremadeInterests(new Interest[] { new Culture(378), new Money(378) }, new Interest[] { new Toys(378), new Entertainment(378), new Scifi(378) } ) }, // Bebe Hart
            {249, new simPremadeInterests(new Interest[] { new Sports(249), new Toys(249) }, new Interest[] { new Entertainment(249), new Culture(249), new Scifi(249), new Paranormal(249), new Politics(249) } ) }, // Erin Kennedy
            {6340, new simPremadeInterests(new Interest[] { new Food(6340), new Politics(6340) }, new Interest[] { new Entertainment(6340), new Toys(6340), new Scifi(6340), new Paranormal(6340), new Culture(6340) } ) }, // Tori Kimura
            {28823, new simPremadeInterests(new Interest[] { new Entertainment(28823) }, new Interest[0] ) }, // Erik Darling
            {231, new simPremadeInterests(new Interest[] { new Sports(231), new Money(231) }, new Interest[] { new Sims3.Gameplay.Lyralei.InterestMod.Environment(231, false), new Paranormal(231), new Entertainment(231), new Food(231), new Scifi(231), new Paranormal(231) } ) }, // Iliana Langerak
            {232, new simPremadeInterests(new Interest[] { new Entertainment(232), new Sims3.Gameplay.Lyralei.InterestMod.Environment(232, false), new Culture(232), new Toys(232) }, new Interest[0] ) }, // Zelda Mae
            {233, new simPremadeInterests(new Interest[] { new Paranormal(233), new Sims3.Gameplay.Lyralei.InterestMod.Environment(233, false), new Culture(233), new Sports(233), new Fashion(233) }, new Interest[] { new Politics(233), new Crime(233), new Animals(233) } ) }, // Kaylynn Langerak
            {234, new simPremadeInterests(new Interest[] { new Entertainment(234), new Sports(234), new Fashion(234) }, new Interest[] { new Politics(234), new Scifi(234) } ) }, // Parker Langerak
            {235, new simPremadeInterests(new Interest[] { new Sports(235), new Food(235), new Fashion(235) }, new Interest[] { new Politics(235) } ) }, // Dustin Langerak
            {134, new simPremadeInterests(new Interest[] { new Money(134), new Entertainment(134) }, new Interest[] { new Politics(134) } ) }, // Iqbal Alvi
            {136, new simPremadeInterests(new Interest[] { new Crime(136), new Culture(136) }, new Interest[] { new Politics(136) } ) }, // VJ Alvi
            {138, new simPremadeInterests(new Interest[] { new Crime(138), new Culture(138) }, new Interest[] { new Politics() } ) }, // Miraj Alvi
            {314, new simPremadeInterests(new Interest[] { new Money(314), new Sports(314), new Sims3.Gameplay.Lyralei.InterestMod.Environment(314, false) }, new Interest[0] ) }, // Monika Morris
            {329, new simPremadeInterests(new Interest[] { new Money(329), new Crime(329), new Sports(329), new Scifi(329), new Culture(329) }, new Interest[] { new Entertainment(329) } ) }, // Ayesha Ansari
            {7809, new simPremadeInterests(new Interest[] { new Health(7809), new Culture(7809), new Scifi(7809) }, new Interest[0] ) }, // Morgana Wolff
            {240, new simPremadeInterests(new Interest[] { new Health(240), new Sports(240), new Entertainment(240), new Sims3.Gameplay.Lyralei.InterestMod.Environment(240, false) }, new Interest[0] ) }, // Pauline Wan
            {410, new simPremadeInterests(new Interest[] { new Health(410), new Sports(410), new Entertainment(410), new Sims3.Gameplay.Lyralei.InterestMod.Environment(410, false), new Crime(410) }, new Interest[0] ) }, // Hank Goddard
            {8318, new simPremadeInterests(new Interest[] { new Health(8318), new Sims3.Gameplay.Lyralei.InterestMod.Environment(8318, false), new Culture(8318) }, new Interest[] { new Crime(8318), new Money(8318) } ) }, // Geoffrey Landgraab
            {8319, new simPremadeInterests(new Interest[] { new Health(8319), new Politics(8319), new Money(8319), new Sports(8319), new Fashion(8319) }, new Interest[] { new Crime(8319) } ) }, // Nancy Landgraab
            {8320, new simPremadeInterests(new Interest[] { new Culture(8320), new Politics(8320), new Money(8320) }, new Interest[] { new Crime(8320) } ) }, // Malcolm Landgraab
            {237, new simPremadeInterests(new Interest[] { new Health(237), new Culture(237), new Entertainment(237) }, new Interest[0] ) }, // Jamie Jolina
            {148, new simPremadeInterests(new Interest[] { new Culture(148) }, new Interest[0] ) }, // Connor Frio
            {150, new simPremadeInterests(new Interest[] { new Food(150), new Entertainment(150), new Fashion(150) }, new Interest[0] ) }, // Jared Frio
        };

        public static AlarmHandle mDoHobbyAutonomouslyAlarm = AlarmHandle.kInvalidHandle;
        public static AlarmHandle mGoToHobbyLotAlarm = AlarmHandle.kInvalidHandle;
        public static AlarmHandle mDoStuffAtHobbyLot = AlarmHandle.kInvalidHandle;

        public static AlarmHandle mHasMadeFood = AlarmHandle.kInvalidHandle;

        public static void SetTownieAlarms(Sim sim)
        {
            // if the user has decided that townies can't have a life after work, then exit out.
            if (!Tunables.kMayTowniesPerformHobbiesAutonomously) { return; }

            if (sim != null && sim.SimDescription != null && InterestManager.mSavedSimInterests.ContainsKey(sim.SimDescription.SimDescriptionId))
            {
                float TimeUntilEvent = RandomUtil.GetFloat(1f, 24f);// was 24
                float RandomDay = RandomUtil.GetFloat(1f, 7f);

                mDoHobbyAutonomouslyAlarm = sim.AddAlarmRepeating(TimeUntilEvent, TimeUnit.Hours, () => { PreformHobby(sim, true, false); }, TimeUntilEvent, TimeUnit.Hours, "Do_Hobby_Autonomously_Alarm_" + sim.SimDescription.SimDescriptionId.ToString(), AlarmType.AlwaysPersisted);
                mGoToHobbyLotAlarm = sim.AddAlarmRepeating(3f, TimeUnit.Hours, () => { PreformHobby(sim, false, false); }, 3f, TimeUnit.Hours, "GoTo_HobbyLot_Autonomously_Alarm_" + sim.SimDescription.SimDescriptionId.ToString(), AlarmType.AlwaysPersisted);
                mDoStuffAtHobbyLot = sim.AddAlarmRepeating(1f, TimeUnit.Hours, () => { PreformHobby(sim, false, true); }, 1f, TimeUnit.Hours, "DoStuffAtHobbyLot_Autonomously_Alarm_" + sim.SimDescription.SimDescriptionId.ToString(), AlarmType.AlwaysPersisted);
            }
        }

        public static void PreformHobby(Sim sim, bool isForRandomHobbyInteraction, bool isAlreadyOnHobbyLot)
        {
            bool isOnDate = false;
            bool isInPack = false;
            sim.IsInGroupOrDateSituation(out isOnDate);
            sim.IsInGroupOrPackSituation(out isInPack);

            if (sim.IsSleeping || isOnDate || sim.MoodManager.IsInNegativeMood || sim.IsAtWork  || sim.IsDying() || isInPack || sim.IsSimInSituationWhichRulesOutMetaAutonomy || sim.IsSocializing || PromSituation.IsGoingToProm(sim) || sim.IsPerformingAService || sim.WasCreatedByAService || sim.Household.IsServiceNpcHousehold || sim.Household.IsSpecialHousehold || sim.Household.IsAlienHousehold || sim.Household.IsMermaidHousehold || sim.Household.IsPetHousehold || sim.Household.IsPreviousTravelerHousehold || sim.Household.IsServobotHousehold || sim.Household.IsSpecialHousehold || sim.Household.IsTouristHousehold || sim.Household.IsTravelHousehold)
            {
                return;
            }

            if (isAlreadyOnHobbyLot)
            {
                Lot hobbyLot = sim.LotCurrent;
                HobbyLot hobbylotType = null;

                if (InterestManager.mHobbyLot != null || InterestManager.mHobbyLot.Count > 0)
                {
                    foreach (HobbyLot hobby in InterestManager.mHobbyLot)
                    {
                        if (hobby.LotId == hobbyLot.LotId)
                        {
                            hobbylotType = hobby;
                        }
                    }
                }
                if (hobbylotType != null)
                {
                    Interest interest = InterestManager.GetInterestFromInterestType(hobbylotType.typeHobbyLot, sim);
                    DoHobbyAutonomously(interest, sim, false, true);
                }
                else
                {
                    return;
                }
            }
            List<Interest> interests = InterestManager.mSavedSimInterests[sim.SimDescription.SimDescriptionId];
            List<Interest> mCompatibleInterests = new List<Interest>();

            for (int i = 0; i < interests.Count; i++)
            {
                //if (InterestManager.HasTheNecessaryInterest(sim.SimDescription, interests[i].mInterestsGuid, false))
                if (interests[i].mInterestsGuid == InterestTypes.Environment)
                {
                    mCompatibleInterests.Add(interests[i]);
                }
            }

            if (mCompatibleInterests != null || mCompatibleInterests.Count > 0)
            {
                Interest randomInterest = RandomUtil.GetRandomObjectFromList(mCompatibleInterests);


                // If we want our sims to preform stuff in the world rather than visiting any saved hobby lots
                if (isForRandomHobbyInteraction)
                {
                    // Don't use user-defined hobby lot
                    DoHobbyAutonomously(randomInterest, sim, false, false);
                }
                else
                {
                    if (randomInterest.mInterestsGuid == InterestTypes.Environment)
                    {
                        // Use user-defined hobby lot.
                        DoHobbyAutonomously(randomInterest, sim, true, false);
                    }
                }
            }
        }

        public static void DoHobbyAutonomously(Interest interest, Sim sim, bool shouldUseSavedLot, bool isAlreadyOnHobbyLot)
        {
            Lot currentLot = sim.LotHome;
            Plant[] plantsCurrentLot = currentLot.GetObjects<Plant>();
            Plant[] plantsWorld = Sims3.Gameplay.Queries.GetObjects<Plant>();
            FishingSpot[] fishingspot = Sims3.Gameplay.Queries.GetObjects<FishingSpot>();
            FishingSpot[] fishingspotHome = currentLot.GetObjects<FishingSpot>();
            RockGemMetalBase[] RockGemMetals = Sims3.Gameplay.Queries.GetObjects<RockGemMetalBase>();


            List<string> simInterests = new List<string>();
            string skillPicked = "";

            switch (interest.mInterestsGuid)
            {
                case InterestTypes.Animals:
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestAnimalsBalloons);
                    break;
                case InterestTypes.Crime:
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestCrimeBalloons);
                    break;
                case InterestTypes.Culture:
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestCultureBalloons);
                    break;
                case InterestTypes.Entertainment:
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestEntertainmentBalloons);
                    if (sim.SkillManager.HasElement(SkillNames.VideoGame) && sim.SkillManager.GetSkillLevel(SkillNames.VideoGame) > 2) simInterests.Add("VideoGaming");
                    if (sim.SkillManager.HasElement(SkillNames.SocialNetworking) && sim.SkillManager.GetSkillLevel(SkillNames.SocialNetworking) > 2) simInterests.Add("socialnetworking");

                    break;
                case InterestTypes.Environment:

                    if (sim.SkillManager.HasElement(SkillNames.Gardening) && sim.SkillManager.GetSkillLevel(SkillNames.Gardening) > 2) simInterests.Add("Gardening");
                    if (sim.SkillManager.HasElement(SkillNames.Fishing) && sim.SkillManager.GetSkillLevel(SkillNames.Fishing) > 2) simInterests.Add("Fishing");
                    if (sim.SkillManager.HasElement(SkillNames.Handiness) && sim.SkillManager.GetSkillLevel(SkillNames.Handiness) > 2) simInterests.Add("Handiness");

                    if (simInterests.Count > 0)
                    {
                        skillPicked = RandomUtil.GetRandomObjectFromList(simInterests);
                    }

                    if (skillPicked == "Gardening")
                    {
                        // If should used saved lot (so community lot) we get harvest plant. Otherwise Tend home garden.(Tend garden has a test where it checks lot validility. So we can't use that.) 
                        HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, shouldUseSavedLot ? HarvestPlant.Harvest.Singleton : Plant.TendGarden<Plant>.Singleton, sim, typeof(Plant));
                    }
                    else if (skillPicked == "Fishing")
                    {
                        HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, FishAutonomously.Singleton, sim, typeof(FishingSpot));
                    }
                    else if (skillPicked == "Handiness")
                    {
                        Lot getHobbyLot = GetHobbyLotAslot(interest.mInterestsGuid, skillPicked);

                        GameObject[] LotItems;
                        if (!shouldUseSavedLot && getHobbyLot != null)
                        {
                            LotItems = getHobbyLot.GetObjects<GameObject>();
                        }
                        else if (isAlreadyOnHobbyLot && getHobbyLot != null)
                        {
                            LotItems = getHobbyLot.GetObjects<GameObject>();
                        }
                        else
                        {
                            // Get both, assuming there is a hobby lot.
                            List<GameObject> newItems = new List<GameObject>();
                            if (getHobbyLot != null) 
                                newItems = CommonHelpers.GetAllElectronicObjectsOnLot(getHobbyLot);
                            newItems.AddRange(CommonHelpers.GetAllElectronicObjectsOnLot(sim.LotHome));

                            // If we have nothing.
                            if (newItems.Count <= 0)
                                return;

                            LotItems = newItems.ToArray();
                        }
                        // fix empty list cases.
                        if (LotItems != null)
                        {
                            List<GameObject> TinkableItems = new List<GameObject>();

                            foreach (GameObject obj in LotItems)
                            {
                                if (obj.IsUpgradable)
                                {
                                    TinkableItems.Add(obj);
                                }
                            }

                            GameObject randomUpgradableItem = RandomUtil.GetRandomObjectFromList(TinkableItems);

                            InteractionDefinition interaction = GetTinkableInteraction(randomUpgradableItem);
                            if (interaction != null)
                            {
                                HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, interaction, sim, randomUpgradableItem);
                            }
                        }
                        return;
                    }
                    break;
                case InterestTypes.Fashion:
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestFashionBalloons);
                    break;
                case InterestTypes.Food:
                    if (sim.SkillManager.HasElement(SkillNames.Gardening) && sim.SkillManager.GetSkillLevel(SkillNames.Gardening) > 1) simInterests.Add("Gardening");
                    if (sim.SkillManager.HasElement(SkillNames.Fishing) && sim.SkillManager.GetSkillLevel(SkillNames.Fishing) > 1) simInterests.Add("Fishing");
                    if (sim.SkillManager.HasElement(SkillNames.Nectar) && sim.SkillManager.GetSkillLevel(SkillNames.Nectar) > 1) simInterests.Add("Nectar");
                    if (sim.SkillManager.HasElement(SkillNames.Bartending) && sim.SkillManager.GetSkillLevel(SkillNames.Bartending) > 1) simInterests.Add("Mixology");
                    if (sim.SkillManager.HasElement(SkillNames.Cooking) && sim.SkillManager.GetSkillLevel(SkillNames.Cooking) > 1) simInterests.Add("Cooking");

                    if (skillPicked == "Gardening")
                    {
                        HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, shouldUseSavedLot ? HarvestPlant.Harvest.Singleton : Plant.TendGarden<Plant>.Singleton, sim, typeof(Plant));
                    }
                    else if (skillPicked == "Fishing")
                    {
                        HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, FishAutonomously.Singleton, sim, typeof(FishingSpot));
                    }
                    else if (skillPicked == "Nectar")
                    {
                        // Do 50/50 because we don't know if the nectar barrel has something already. And i'm clearly lazy lol.
                        if (RandomUtil.CoinFlip())
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, NectarMaker.AddFruit.Singleton, sim, typeof(NectarMaker));
                        }
                        else
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, NectarMaker.MakeNectar.Singleton, sim, typeof(NectarMaker));
                        }
                    }
                    else if (skillPicked == "Mixology")
                    {
                        if (RandomUtil.CoinFlip())
                        { 

                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, BarAdvanced.PracticeMakingNewDrinks.Singleton, sim, typeof(BarAdvanced));
                        }
                        else
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, BarAdvanced.Practice.Singleton, sim, typeof(BarAdvanced));
                        }
                    }
                    else if (skillPicked == "Cooking")
                    {
                        if (RandomUtil.CoinFlip())
                        {
                            Lot getHobbyLot = null;
                            if (shouldUseSavedLot)
                            {
                                 getHobbyLot = GetHobbyLotAslot(interest.mInterestsGuid, skillPicked);
                            }
                            else if (isAlreadyOnHobbyLot)
                            {
                                 getHobbyLot = sim.LotCurrent;
                            }

                            Sims3.Gameplay.Objects.FoodObjects.Recipe.MealQuantity quantity = Sims3.Gameplay.Objects.FoodObjects.Recipe.MealQuantity.DoNotCheck;
                            if(RandomUtil.CoinFlip())
                            {
                                quantity = Sims3.Gameplay.Objects.FoodObjects.Recipe.MealQuantity.Group;
                            }
                            else
                            {
                                quantity = Sims3.Gameplay.Objects.FoodObjects.Recipe.MealQuantity.Single;
                            }

                            // puruse hobby at home then.
                            if (getHobbyLot == null)
                            {
                                Babysitter.PushPrepareFoodInteraction(sim, RandomUtil.GetRandomObjectFromList(sim.LotCurrent.GetAllActors()), null, quantity, Sims3.Gameplay.Objects.FoodObjects.Recipe.MealDestination.SurfaceOrEat);
                            }
                            else
                            {
                                GetHobbyLotAsHobbyLot(getHobbyLot.LotId).GetRandomInteractionFromWhitelist(interest.Guid, sim.SimDescription);
                                GoToLot.Singleton.CreateInstanceWithCallbacks(getHobbyLot, sim, sim.ForcePushPriority(), true, true, null, (Sim s, float x) => { Babysitter.PushPrepareFoodInteraction(sim, RandomUtil.GetRandomObjectFromList(sim.LotCurrent.GetAllActors()), null, quantity, Sims3.Gameplay.Objects.FoodObjects.Recipe.MealDestination.SurfaceOrEat); },null);
                            }
                        }
                    }
                    break;
                case InterestTypes.Health:
                    if (sim.SkillManager.HasElement(SkillNames.Logic) && sim.SkillManager.GetSkillLevel(SkillNames.Logic) > 1) simInterests.Add("Logic");
                    if (sim.SkillManager.HasElement(SkillNames.Science) && sim.SkillManager.GetSkillLevel(SkillNames.Science) > 1) simInterests.Add("Science");
                    if (sim.SkillManager.HasElement(SkillNames.Athletic) && sim.SkillManager.GetSkillLevel(SkillNames.Science) > 1) simInterests.Add("Athletic");
                    if (sim.SkillManager.HasElement(SkillNames.Gardening) && sim.SkillManager.GetSkillLevel(SkillNames.Gardening) > 1) simInterests.Add("Gardening");
                    if (sim.SkillManager.HasElement(SkillNames.Cooking) && sim.SkillManager.GetSkillLevel(SkillNames.Cooking) > 1) simInterests.Add("Cooking");
                    if (sim.SkillManager.HasElement(SkillNames.Spellcraft) && sim.SkillManager.GetSkillLevel(SkillNames.Spellcraft) > 1) simInterests.Add("Alchemy");

                    if (skillPicked == "Logic")
                    {
                        if(RandomUtil.RandomChance(33))
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, ChessTable.PracticeChess.Singleton, sim, typeof(ChessTable));
                        }
                        else if(RandomUtil.RandomChance(33))
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, Telescope.LookThroughOrStargaze.Singleton, sim, typeof(Telescope));
                        }
                        else
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, Computer.SolveHardProblems.Singleton, sim, typeof(Computer));
                        }
                    }
                    else if (skillPicked == "Science")
                    {
                        if (RandomUtil.CoinFlip())
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, ScienceResearchStation.ResearchScience.Singleton, sim, typeof(ScienceResearchStation));
                        }
                        else
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, AnatomySkeleton.TestSkeletal.Singleton, sim, typeof(ScienceResearchStation));
                        }
                    }
                    else if(skillPicked == "Athletic")
                    {

                        if (RandomUtil.RandomChance(33))
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, Treadmill.WorkOut.Singleton, sim, typeof(Treadmill));
                        }
                        else if (RandomUtil.RandomChance(33))
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, TV.WorkOut.Singleton, sim, typeof(TV));
                        }
                        else
                        {
                            Terrain.GoJogging.Singleton.CreateInstance(sim, sim, sim.ForcePushPriority(), true, true);
                        }
                    }
                    if (skillPicked == "Gardening")
                    {
                        HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, shouldUseSavedLot ? HarvestPlant.Harvest.Singleton : Plant.TendGarden<Plant>.Singleton, sim, typeof(Plant));
                    }
                    else if (skillPicked == "Cooking")
                    {
                        if (RandomUtil.CoinFlip())
                        {
                            Lot getHobbyLot = null;
                            if (shouldUseSavedLot)
                            {
                                getHobbyLot = GetHobbyLotAslot(interest.mInterestsGuid, skillPicked);
                            }
                            else if (isAlreadyOnHobbyLot)
                            {
                                getHobbyLot = sim.LotCurrent;
                            }

                            Sims3.Gameplay.Objects.FoodObjects.Recipe.MealQuantity quantity = Sims3.Gameplay.Objects.FoodObjects.Recipe.MealQuantity.DoNotCheck;
                            if (RandomUtil.CoinFlip())
                            {
                                quantity = Sims3.Gameplay.Objects.FoodObjects.Recipe.MealQuantity.Group;
                            }
                            else
                            {
                                quantity = Sims3.Gameplay.Objects.FoodObjects.Recipe.MealQuantity.Single;
                            }

                            // puruse hobby at home then.
                            if (getHobbyLot == null)
                            {
                                Babysitter.PushPrepareFoodInteraction(sim, RandomUtil.GetRandomObjectFromList(sim.LotCurrent.GetAllActors()), null, quantity, Sims3.Gameplay.Objects.FoodObjects.Recipe.MealDestination.SurfaceOrEat);
                            }
                            else
                            {
                                GetHobbyLotAsHobbyLot(getHobbyLot.LotId).GetRandomInteractionFromWhitelist(interest.Guid, sim.SimDescription);
                                GoToLot.Singleton.CreateInstanceWithCallbacks(getHobbyLot, sim, sim.ForcePushPriority(), true, true, null, (Sim s, float x) => { Babysitter.PushPrepareFoodInteraction(sim, RandomUtil.GetRandomObjectFromList(sim.LotCurrent.GetAllActors()), null, quantity, Sims3.Gameplay.Objects.FoodObjects.Recipe.MealDestination.SurfaceOrEat); }, null);
                            }
                        }
                    }
                    else if(skillPicked == "Alchemy")
                    {
                        HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, AlchemyStation.ResearchAlchemy.Singleton, sim, typeof(AlchemyStation));
                    }
                    break;
                case InterestTypes.Money:
                    if (sim.SkillManager.HasElement(SkillNames.Charisma) && sim.SkillManager.GetSkillLevel(SkillNames.Charisma) > 1) simInterests.Add("Charisma");

                    if (skillPicked == "Charisma")
                    {
                        if (RandomUtil.RandomChance(33))
                        {
                            HandleLotChoosingAndInteractions(shouldUseSavedLot, isAlreadyOnHobbyLot, skillPicked, interest, Mirror.PracticeSpeech.Singleton, sim, typeof(Mirror));
                        }
                    }
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestMoneyBalloons);
                    break;
                case InterestTypes.Paranormal:
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestParanormalBalloons);
                    break;
                case InterestTypes.Politics:
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestPoliticsBalloons);
                    break;
                case InterestTypes.Scifi:
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestScifiBalloons);
                    break;
                case InterestTypes.Sports:
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestSportsBalloons);
                    break;
                case InterestTypes.Toys:
                    //randomStringFromList = RandomUtil.GetRandomStringFromList(InterestManager.Instance.kInterestToysBalloons);
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Use this one ONLY if obtaining objects need to be done differently. Currently it's only used for Tinkerable items
        /// </summary>
        /// <param name="shouldUseSavedLot"></param>
        /// <param name="isAlreadyOnHobbyLot"></param>
        /// <param name="skillPicked"></param>
        /// <param name="interest"></param>
        /// <param name="interactionDefinition"></param>
        /// <param name="sim"></param>
        /// <param name="gameObject"></param>
        private static void HandleLotChoosingAndInteractions(bool shouldUseSavedLot, bool isAlreadyOnHobbyLot, string skillPicked, Interest interest, InteractionDefinition interactionDefinition, Sim sim, GameObject gameObject)
        {

            if (shouldUseSavedLot)
            {
                Lot getHobbyLot = GetHobbyLotAslot(interest.mInterestsGuid, skillPicked);
                if (gameObject == null && getHobbyLot.LotId != 0)
                {
                    Camera.FocusOnLot(getHobbyLot.LotId, 0f);
                    bool acceptedBuyMode = TwoButtonDialog.Show("It seems that you've applied this lot to be a '" + skillPicked + "' hobby lot for your townies to gather, but there aren't any required items of the hobby on this lot: " + getHobbyLot.Name + " - " + getHobbyLot.Address + '\n' + '\n' + "Do you want to fix it now by going into buy mode on that lot? (BuyDeBug on, the cheat, will be turned on for this)", "Yes", "No");

                    if (acceptedBuyMode)
                    {
                        Commands.EnableTestingCheats();
                        BuyController.BuyDebug(true);
                        GameStates.TransitionToBuyMode(getHobbyLot, new EventArgs());
                        Commands.UpdateRoomMarkerVisibility();
                    }
                    return;
                }
                if (gameObject != null)
                {
                    DoHobbyInteraction(sim, gameObject, GetHobbyLotAsHobbyLot(getHobbyLot.LotId).GetRandomInteractionFromWhitelist(interest.Guid, sim.SimDescription), true, null, null, null);
                }
                return;
            }
            else if (isAlreadyOnHobbyLot)
            {
                Lot getHobbyLot = sim.LotCurrent;

                if (gameObject == null && getHobbyLot.LotId != 0)
                {
                    Camera.FocusOnLot(getHobbyLot.LotId, 0f);
                    bool acceptedBuyMode = TwoButtonDialog.Show("It seems that you've applied this lot to be a 'Fishing' hobby lot for your townies to gather, but there aren't any ponds to fish at! Make sure that you add ponds With a fish spanwer to this lot: " + getHobbyLot.Name + " - " + getHobbyLot.Address + '\n' + '\n' + "Do you want to fix it now by going into buy mode on that lot? (BuyDeBug on, the cheat, will be turned on for this)", "Yes", "No");

                    if (acceptedBuyMode && getHobbyLot.LotId != 0)
                    {
                        Commands.EnableTestingCheats();
                        BuyController.BuyDebug(true);
                        GameStates.TransitionToBuyMode(getHobbyLot, new EventArgs());
                        Commands.UpdateRoomMarkerVisibility();
                    }
                    return;

                }
                if (gameObject != null)
                {
                    DoHobbyInteraction(sim, gameObject, GetHobbyLotAsHobbyLot(getHobbyLot.LotId).GetRandomInteractionFromWhitelist(interest.Guid, sim.SimDescription), true, null, null, null);
                }
                return;
            }
            else
            {
                DoHobbyInteraction(sim, gameObject, interactionDefinition, false, null, null, null);
            }


        }


        private static void HandleLotChoosingAndInteractions(bool shouldUseSavedLot, bool isAlreadyOnHobbyLot, string skillPicked, Interest interest, InteractionDefinition interactionDefinition, Sim sim, Type type)
        {

            GameObject[] spots = Sims3.SimIFace.Queries.GetObjects(type) as GameObject[];
            List<IGameObject> spotsHome = sim.LotHome.GetObjects(type, null);
            
            // Stops here and we don't have hobby lots...
            if (shouldUseSavedLot)
            {
                Lot getHobbyLot = GetHobbyLotAslot(interest.mInterestsGuid, skillPicked);
                List<IGameObject> savedLotHobbyItems = getHobbyLot.GetObjects(type, null);
                if (savedLotHobbyItems.Count == 0)
                {
                    Camera.FocusOnLot(getHobbyLot.LotId, 0f);
                    bool acceptedBuyMode = TwoButtonDialog.Show("It seems that you've applied this lot to be a '" + skillPicked + "' hobby lot for your townies to gather, but there aren't any required items of the hobby on this lot: " + getHobbyLot.Name + " - " + getHobbyLot.Address + '\n' + '\n' + "Do you want to fix it now by going into buy mode on that lot? (BuyDeBug on, the cheat, will be turned on for this)", "Yes", "No");

                    if (acceptedBuyMode && getHobbyLot.LotId != 0)
                    {
                        Commands.EnableTestingCheats();
                        BuyController.BuyDebug(true);
                        GameStates.TransitionToBuyMode(getHobbyLot, new EventArgs());
                        Commands.UpdateRoomMarkerVisibility();
                    }
                    return;
                }
                int @int = RandomUtil.GetInt(0, savedLotHobbyItems.Count - 1);
                if (savedLotHobbyItems.Count > 0)
                {
                    DoHobbyInteraction(sim, GameObject.GetObject(savedLotHobbyItems[@int].ObjectId), GetHobbyLotAsHobbyLot(getHobbyLot.LotId).GetRandomInteractionFromWhitelist(interest.Guid, sim.SimDescription), true, null,null,null) ;
                }
                return;
            }
            if (isAlreadyOnHobbyLot)
            {

                Lot getHobbyLot = sim.LotCurrent;
                List<IGameObject> savedLotHobbyItems = getHobbyLot.GetObjects(type, null);
                if (savedLotHobbyItems.Count == 0)
                {
                    Camera.FocusOnLot(getHobbyLot.LotId, 0f);
                    bool acceptedBuyMode = TwoButtonDialog.Show("It seems that you've applied this lot to be a 'Fishing' hobby lot for your townies to gather, but there aren't any ponds to fish at! Make sure that you add ponds With a fish spanwer to this lot: " + getHobbyLot.Name + " - " + getHobbyLot.Address + '\n' + '\n' + "Do you want to fix it now by going into buy mode on that lot? (BuyDeBug on, the cheat, will be turned on for this)", "Yes", "No");

                    if (acceptedBuyMode && getHobbyLot.LotId != 0)
                    {
                        Commands.EnableTestingCheats();
                        BuyController.BuyDebug(true);
                        GameStates.TransitionToBuyMode(getHobbyLot, new EventArgs());
                        Commands.UpdateRoomMarkerVisibility();
                    }
                    return;

                }
                int @int = RandomUtil.GetInt(0, savedLotHobbyItems.Count - 1);
                if (savedLotHobbyItems.Count > 0)
                {
                    DoHobbyInteraction(sim, GameObject.GetObject(savedLotHobbyItems[@int].ObjectId), GetHobbyLotAsHobbyLot(getHobbyLot.LotId).GetRandomInteractionFromWhitelist(interest.Guid, sim.SimDescription), true, null, null, null);
                }
                return;
            }

            // If the home lot AND the residency lot has plants, then choose.
            if (spotsHome.Count != 0 && spots.Length != 0)
            {

                if (RandomUtil.RandomChance(50))
                {
                    // Apply to home
                    DoHobbyInteraction(sim, GameObject.GetObject(spotsHome[0].ObjectId), interactionDefinition, false, null, null, null);
                }
                else
                {
                    // Apply to random lot with plants
                    int @int = RandomUtil.GetInt(0, spots.Length - 1);

                    if (spots[@int].LotCurrent.IsResidentialLot)
                    {
                        return;
                    }
                    DoHobbyInteraction(sim, GameObject.GetObject(spots[@int].ObjectId), interactionDefinition, true, null, null, null);
                }
            }
            // Else if the lot has plants but the world doesn't...
            else if (spotsHome.Count != 0 && spots.Length == 0)
            {
                int @int = RandomUtil.GetInt(0, spotsHome.Count - 1);
                DoHobbyInteraction(sim, GameObject.GetObject(spotsHome[@int].ObjectId), interactionDefinition, false, null, null, null);
            }
            // Else if the homelot doesn't have plants but the world does... 
            else if (spotsHome.Count == 0 && spots.Length != 0)
            {
                // Apply to random lot with plants
                int @int = RandomUtil.GetInt(0, spots.Length - 1);

                if (spots[@int].LotCurrent.IsResidentialLot)
                {
                    return ;
                }
                DoHobbyInteraction(sim, GameObject.GetObject(spots[@int].ObjectId), interactionDefinition, true, null, null, null);
            }
            else
            {
                // Don't do anything at all if there's nothing to do. 
                return;
            }
            return;
        }


        public static InteractionDefinition GetTinkableInteraction(GameObject obj)
        {
            if (IsSameOrSubclass(typeof(WashingMachine), obj.GetType()))
            {
                return WashingMachine.Tinker.Singleton;
            }
            else if(IsSameOrSubclass(typeof(Stove), obj.GetType()))
            {
                return Stove.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(NectarMaker), obj.GetType()))
            {
                return NectarMaker.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(Microwave), obj.GetType()))
            {
                return Microwave.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(Dishwasher), obj.GetType()))
            {
                return Dishwasher.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(SleepPodFuture), obj.GetType()))
            {
                return SleepPodFuture.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(ShowerOutdoor), obj.GetType()))
            {
                return ShowerOutdoor.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(Shower), obj.GetType()))
            {
                return Shower.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(Computer), obj.GetType()))
            {
                return Computer.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(Sink), obj.GetType()))
            {
                return Sink.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(Stereo), obj.GetType()))
            {
                return Stereo.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(TV), obj.GetType()))
            {
                return TV.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(Toilet), obj.GetType()))
            {
                return Toilet.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(Bathtub), obj.GetType()))
            {
                return Bathtub.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(AllInOneBathroom), obj.GetType()))
            {
                return AllInOneBathroom.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(HotTubBase), obj.GetType()))
            {
                return HotTubBase.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(Fireplace), obj.GetType()))
            {
                return Fireplace.Tinker.Singleton;
            }
            else if (IsSameOrSubclass(typeof(TrashCompactor), obj.GetType()))
            {
                return TrashCompactor.Tinker.Singleton;
            }
            else
            {
                return null;
            }
        }

        public static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }

        public static HobbyLot GetHobbyLotAsHobbyLot(ulong Lotid)
        {
            for (int i = 0; i < InterestManager.mHobbyLot.Count; i++)
            {
                if(InterestManager.mHobbyLot[i].lotID == Lotid)
                {
                    return InterestManager.mHobbyLot[i];
                }
            }
            return null;
        }



        public static Lot GetHobbyLotAslot(InterestTypes type, string skillToPreform)
        {
            List<HobbyLot> lotsFound = new List<HobbyLot>();
            for(int i = 0; i < InterestManager.mHobbyLot.Count; i++)
            {
                if(InterestManager.mHobbyLot[i].typeHobbyLot == type)
                {
                    if (skillToPreform == "Gardening")
                    {
                        foreach(KeyValuePair<string, InteractionDefinition> kvp in InterestManager.DefaultGardeningInteractions)
                        {
                            if (InterestManager.mHobbyLot[i].mWhitelistedInteractions.ContainsKey(kvp.Key))
                            {
                                if(InterestManager.mHobbyLot[i] != null && InterestManager.mHobbyLot[i].lot != null)
                                {
                                    return InterestManager.mHobbyLot[i].lot;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                    }
                    if (skillToPreform == "Fishing")
                    {
                        if (InterestManager.mHobbyLot[i].mWhitelistedInteractions.ContainsKey(Localization.LocalizeString("Gameplay/Objects/Fishing:Fish")))
                        {
                            return InterestManager.mHobbyLot[i].lot;
                        }
                    }
                    if (skillToPreform == "Cooking")
                    {
                        if (InterestManager.mHobbyLot[i].mWhitelistedInteractions.ContainsKey(Localization.LocalizeString("Gameplay/Objects/Fishing:Fish")))
                        {
                            return InterestManager.mHobbyLot[i].lot;
                        }
                    }

                    //lotsFound.Add(lot);
                }
            }
            return null;
        }


        public static void DoHobbyInteraction(Sim sim, GameObject gameObject, InteractionDefinition interactionDef,  bool IsForCommunityLot, Callback callbackStarted, Callback callbackOnCompleted, Callback callbackOnFailed)
        {
            if(gameObject == null) {  return; }
            if (sim == null) { return; }
            if (interactionDef == null) { return; }

            try
            {

                bool wasInited = false; // Used in case we got a non-initiated townie.
                // If sim is an NPC with no home in the direct world (so a townie) get their ass into the world :p
                if (sim == null)
                {
                    SimOutfit outfit = sim.SimDescription.GetOutfit(OutfitCategories.Everyday, 0);
                    sim = ((!outfit.IsValid) ? sim.SimDescription.Instantiate(gameObject.LotCurrent) : sim.SimDescription.Instantiate(gameObject.LotCurrent, outfit.Key));
                    wasInited = true;
                }

                InteractionInstance interactionInstance = null;
                InteractionInstance GoTo = null;

                if (IsForCommunityLot)
                {
                    // if(HarvestPlant.HarvestTest(plant, sim))
                    //{
                    if (!wasInited)
                    {
                        InteractionDefinition interactionDefinition = (gameObject.LotCurrent.IsCommunityLot ? VisitCommunityLot.Singleton : GoToLot.Singleton);

                        GoTo = interactionDefinition.CreateInstanceWithCallbacks(gameObject.LotCurrent, sim, sim.ForcePushPriority(), true, true, null, (Sim s, float x) => { GetSimToDoHobby(sim, gameObject, interactionDef); }, null);

                        if (GoTo != null)
                            sim.InteractionQueue.AddNextIfPossible(GoTo);

                        return;
                    }
                }
                else
                {
                    interactionInstance = interactionDef.CreateInstance(gameObject, sim, new InteractionPriority(InteractionPriorityLevel.High), true, true) as InteractionInstance;
                }

                sim.InteractionQueue.AddNextIfPossible(interactionInstance);
            }
            catch (Exception ex)
            {
                GlobalOptionsHobbiesAndInterests.print(ex.ToString());
            }
        }

        public static void GetSimToDoHobby(Sim sim, GameObject gameObject, InteractionDefinition interactionDef)
        {
            if(sim == null || gameObject == null || interactionDef == null) return;

            InteractionInstance interactionInstance = interactionDef.CreateInstance(gameObject, sim, sim.ForcePushPriority(), true, true);
            if(interactionInstance == null) return;

            sim.InteractionQueue.AddNext(interactionInstance); // Doesnt work with gardening
        }

        public static void PutLeftoversAway(Sim sim)
        {
            ServingContainer[] LeftoverFood = sim.LotHome.GetObjects<ServingContainer>();

            if (LeftoverFood.Length > 0)
            {
                foreach (ServingContainer leftover in LeftoverFood)
                {
                    if (leftover == null)
                    {
                        continue;
                    }
                    InteractionInstance putAwayFood = ServingContainer.PutAwayLeftovers.Singleton.CreateInstance(leftover, sim, new InteractionPriority(InteractionPriorityLevel.High), false, true);
                    sim.InteractionQueue.TryPushAsContinuation(sim.CurrentInteraction, putAwayFood);
                }
            }
            mHasMadeFood = AlarmHandle.kInvalidHandle;
        }


    }
}
